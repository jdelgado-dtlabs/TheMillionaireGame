using System.Diagnostics;
using System.Windows.Forms;

namespace MillionaireGame.Watchdog;

/// <summary>
/// Monitors the main application process for crashes and freezes
/// </summary>
public class ProcessMonitor
{
    private readonly string _applicationPath;
    private readonly string[] _applicationArgs;
    private readonly HeartbeatListener _heartbeatListener;
    private readonly CrashReportGenerator _reportGenerator;
    private Process? _process;
    private DateTime _processStartTime;
    private bool _shutdownRequested;

    public ProcessMonitor(string applicationPath, string[] applicationArgs)
    {
        _applicationPath = applicationPath;
        _applicationArgs = applicationArgs;
        _heartbeatListener = new HeartbeatListener();
        _reportGenerator = new CrashReportGenerator();

        _heartbeatListener.HeartbeatReceived += OnHeartbeatReceived;
        _heartbeatListener.HeartbeatTimeout += OnHeartbeatTimeout;
    }

    public void StartMonitoring()
    {
        WatchdogConsole.Info("[Watchdog] Starting process monitor...");
        WatchdogConsole.Info($"[Watchdog] Application path: {_applicationPath}");
        if (_applicationArgs.Length > 0)
        {
            WatchdogConsole.Info($"[Watchdog] Application arguments: {string.Join(" ", _applicationArgs)}");
        }

        // Start heartbeat listener
        _heartbeatListener.Start();

        // Launch application
        LaunchApplication();
    }

    private void LaunchApplication()
    {
        try
        {
            _processStartTime = DateTime.Now;
            _shutdownRequested = false;

            var startInfo = new ProcessStartInfo
            {
                FileName = _applicationPath,
                Arguments = string.Join(" ", _applicationArgs),
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                CreateNoWindow = false
            };

            _process = Process.Start(startInfo);
            if (_process == null)
            {
                WatchdogConsole.Error("[Watchdog] ERROR: Failed to start application process");
                return;
            }

            WatchdogConsole.Info($"[Watchdog] Application started (PID: {_process.Id})");

            // Monitor process
            _process.EnableRaisingEvents = true;
            _process.Exited += OnProcessExited;

            // Wait for process to exit
            _process.WaitForExit();
        }
        catch (Exception ex)
        {
            WatchdogConsole.Error($"[Watchdog] ERROR launching application: {ex.Message}");
        }
    }

    private void OnProcessExited(object? sender, EventArgs e)
    {
        if (_process == null)
            return;

        var exitCode = _process.ExitCode;
        var runningTime = DateTime.Now - _processStartTime;
        var lastHeartbeat = _heartbeatListener.GetLastHeartbeat();

        WatchdogConsole.Info($"[Watchdog] Application exited (Exit Code: {exitCode}, Running Time: {runningTime:hh\\:mm\\:ss})");

        // Determine if this was a crash
        var isCrash = DetermineCrash(exitCode, lastHeartbeat);

        if (isCrash)
        {
            WatchdogConsole.Warn("[Watchdog] CRASH DETECTED - Generating crash report...");
            
            var crashInfo = new CrashInfo
            {
                ProcessId = _process.Id,
                ExitCode = exitCode,
                CrashTime = DateTime.Now,
                RunningTime = runningTime,
                LastHeartbeat = lastHeartbeat?.Timestamp,
                LastState = lastHeartbeat?.State,
                LastActivity = lastHeartbeat?.CurrentActivity,
                LastMemoryMB = lastHeartbeat?.MemoryUsageMB ?? 0,
                LastThreadCount = lastHeartbeat?.ThreadCount ?? 0,
                WasResponsive = lastHeartbeat != null
            };

            HandleCrash(crashInfo);
        }
        else
        {
            WatchdogConsole.Info("[Watchdog] Clean shutdown detected");
        }

        _process?.Dispose();
        _process = null;
    }

    private bool DetermineCrash(int exitCode, HeartbeatMessage? lastHeartbeat)
    {
        // Exit code 0 or 1 are clean exits
        if (exitCode == 0 || exitCode == 1)
            return false;

        // If last heartbeat was "ShuttingDown", treat as clean
        if (lastHeartbeat?.State == "ShuttingDown")
            return false;

        // Any other exit code is a crash
        return true;
    }

    private void OnHeartbeatReceived(object? sender, HeartbeatMessage message)
    {
        // Detect shutdown request
        if (message.State == "ShuttingDown")
        {
            _shutdownRequested = true;
            WatchdogConsole.Info("[Watchdog] Application is shutting down gracefully");
        }
    }

    private void OnHeartbeatTimeout(object? sender, EventArgs e)
    {
        if (_shutdownRequested)
            return;

        WatchdogConsole.Warn("[Watchdog] APPLICATION FREEZE DETECTED - No heartbeat received");
        
        // Application is frozen/hung - force terminate and generate crash report
        if (_process != null && !_process.HasExited)
        {
            var lastHeartbeat = _heartbeatListener.GetLastHeartbeat();
            var runningTime = DateTime.Now - _processStartTime;

            var crashInfo = new CrashInfo
            {
                ProcessId = _process.Id,
                ExitCode = -1,
                CrashTime = DateTime.Now,
                RunningTime = runningTime,
                LastHeartbeat = lastHeartbeat?.Timestamp,
                LastState = lastHeartbeat?.State,
                LastActivity = lastHeartbeat?.CurrentActivity,
                LastMemoryMB = lastHeartbeat?.MemoryUsageMB ?? 0,
                LastThreadCount = lastHeartbeat?.ThreadCount ?? 0,
                WasResponsive = false
            };

            WatchdogConsole.Info("[Watchdog] Terminating frozen process...");
            try
            {
                _process.Kill();
            }
            catch (Exception ex)
            {
                WatchdogConsole.Error($"[Watchdog] Error killing process: {ex.Message}");
            }

            HandleCrash(crashInfo);
        }
    }

    private void HandleCrash(CrashInfo crashInfo)
    {
        // Log crash detection
        WatchdogConsole.LogSeparator();
        WatchdogConsole.Error("APPLICATION CRASH DETECTED");
        WatchdogConsole.Error($"Exit Code: {crashInfo.ExitCode} (0x{crashInfo.ExitCode:X8})");
        WatchdogConsole.Error($"Last Activity: {crashInfo.LastActivity ?? "Unknown"}");
        WatchdogConsole.Error($"Running Time: {crashInfo.RunningTime.ToString(@"hh\:mm\:ss")}");
        WatchdogConsole.LogSeparator();
        
        // Generate crash report
        var reportPath = _reportGenerator.GenerateReport(crashInfo);
        WatchdogConsole.Info($"[Watchdog] Crash report saved: {reportPath}");
        
        // Populate CrashInfo with additional data
        crashInfo.ExitCodeMeaning = GetExitCodeMeaning(crashInfo.ExitCode);
        crashInfo.CrashReportPath = reportPath;
        // TODO: Get app version from main app assembly if available
        
        try
        {
            // Show crash report dialog on UI thread
            var dialogThread = new System.Threading.Thread(() =>
            {
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                
                using var dialog = new CrashReportDialog(crashInfo);
                var result = dialog.ShowDialog();
                
                if (result == DialogResult.OK && dialog.ShouldSubmitToGitHub)
                {
                    // User wants to submit to GitHub
                    SubmitCrashReportAsync(crashInfo, dialog.UserContext, reportPath).Wait();
                }
                else
                {
                    WatchdogConsole.Info("[Watchdog] User chose not to submit crash report");
                }
            });
            
            dialogThread.SetApartmentState(System.Threading.ApartmentState.STA);
            dialogThread.Start();
            dialogThread.Join(); // Wait for dialog to close
        }
        catch (Exception ex)
        {
            WatchdogConsole.Error($"[Watchdog] Error showing crash dialog: {ex.Message}");
            
            // Fallback to simple MessageBox
            var exitCodeMeaning = GetExitCodeMeaning(crashInfo.ExitCode);
            var message = $"The Millionaire Game has crashed unexpectedly.\n\n" +
                         $"Exit Code: {crashInfo.ExitCode} (0x{crashInfo.ExitCode:X8})\n" +
                         $"Meaning: {exitCodeMeaning}\n" +
                         $"Last Activity: {crashInfo.LastActivity ?? "Unknown"}\n" +
                         $"Running Time: {crashInfo.RunningTime.ToString(@"hh\:mm\:ss")}\n\n" +
                         $"A crash report has been saved to:\n{reportPath}\n\n" +
                         $"Log file: {WatchdogConsole.CurrentLogFilePath}";
            
            MessageBox.Show(
                message,
                "Millionaire Game - Crash Detected",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
    
    private async Task SubmitCrashReportAsync(CrashInfo crashInfo, UserCrashContext userContext, string reportPath)
    {
        try
        {
            WatchdogConsole.Info("[Watchdog] Starting crash report submission...");
            
            // Check if authenticated
            var oauthManager = new GitHubOAuthManager();
            if (!oauthManager.IsAuthenticated())
            {
                WatchdogConsole.Info("[Watchdog] Not authenticated, showing auth dialog...");
                
                // Show authentication dialog
                var authResult = false;
                var authThread = new System.Threading.Thread(() =>
                {
                    Application.SetHighDpiMode(HighDpiMode.SystemAware);
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    
                    using var authDialog = new GitHubAuthDialog();
                    authResult = authDialog.ShowDialog() == DialogResult.OK;
                });
                
                authThread.SetApartmentState(System.Threading.ApartmentState.STA);
                authThread.Start();
                authThread.Join();
                
                if (!authResult)
                {
                    WatchdogConsole.Info("[Watchdog] User cancelled authentication");
                    MessageBox.Show("Authentication cancelled. Crash report was not submitted.",
                        "Submission Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
            
            // Read and sanitize the crash report
            string sanitizedReport;
            if (File.Exists(reportPath))
            {
                string rawReport = await File.ReadAllTextAsync(reportPath);
                sanitizedReport = DataSanitizer.SanitizeCrashReport(rawReport);
            }
            else
            {
                sanitizedReport = "Crash report file not found.";
            }
            
            // Submit to GitHub
            var issueSubmitter = new GitHubIssueSubmitter();
            var result = await issueSubmitter.SubmitCrashReportAsync(crashInfo, userContext, sanitizedReport);
            
            if (result.IsSuccess)
            {
                WatchdogConsole.Info($"[Watchdog] Successfully submitted crash report as issue #{result.IssueNumber}");
                
                var confirmMessage = $"Crash report successfully submitted!\n\n" +
                                   $"Issue #{result.IssueNumber}\n" +
                                   $"{result.IssueUrl}\n\n" +
                                   $"Thank you for helping improve the Millionaire Game!";
                
                MessageBox.Show(confirmMessage, "Report Submitted",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Optionally open the issue in browser
                if (MessageBox.Show("Would you like to view the issue in your browser?",
                    "View Issue", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = result.IssueUrl,
                        UseShellExecute = true
                    });
                }
            }
            else if (result.IsDuplicate)
            {
                WatchdogConsole.Info($"[Watchdog] Duplicate crash detected: Issue #{result.ExistingIssueNumber}");
                
                var duplicateMessage = $"A similar crash has already been reported.\n\n" +
                                      $"Existing Issue: #{result.ExistingIssueNumber}\n" +
                                      $"{result.IssueUrl}\n\n" +
                                      $"Your crash details have been logged locally.";
                
                MessageBox.Show(duplicateMessage, "Duplicate Crash",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                WatchdogConsole.Error($"[Watchdog] Failed to submit crash report: {result.ErrorMessage}");
                
                MessageBox.Show($"Failed to submit crash report:\n{result.ErrorMessage}\n\n" +
                              $"The crash report has been saved locally at:\n{reportPath}",
                    "Submission Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            WatchdogConsole.Error($"[Watchdog] Error submitting crash report: {ex.Message}");
            MessageBox.Show($"An error occurred while submitting the crash report:\n{ex.Message}",
                "Submission Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    /// <summary>
    /// Interprets Windows exit codes and returns a human-readable meaning.
    /// Made public for unit testing purposes.
    /// </summary>
    public static string GetExitCodeMeaning(int exitCode)
    {
        return exitCode switch
        {
            0 => "Successful completion",
            1 => "General error",
            -1 => "General failure",
            unchecked((int)0xC0000005) => "Access violation (native crash)",
            unchecked((int)0xE0434352) => "CLR exception",
            unchecked((int)0xC0000374) => "Heap corruption",
            unchecked((int)0xC000013A) => "Application terminated by Ctrl+C",
            unchecked((int)0xC00000FD) => "Stack overflow",
            unchecked((int)0xC0000409) => "Stack buffer overrun",
            >= 200 and <= 255 => $"Unknown error ({exitCode})",
            _ => $"Application-specific exit code: {exitCode}"
        };
    }

    public void Shutdown()
    {
        _heartbeatListener.Stop();
        _heartbeatListener.Dispose();
    }
}
