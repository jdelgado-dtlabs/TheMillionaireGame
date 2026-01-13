using System.Reflection;

namespace MillionaireGame.Watchdog;

class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        // Initialize Windows Forms for hidden operation
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        WatchdogConsole.Info("====================================");
        WatchdogConsole.Info("Millionaire Game Watchdog");
        WatchdogConsole.Info($"Version {version?.ToString(3) ?? "1.0.0"}");
        WatchdogConsole.Info("====================================");
        WatchdogConsole.Info("");

        // Check for crash test dummy mode
        bool isDebugMode = args.Contains("--debug", StringComparer.OrdinalIgnoreCase);
        bool isCrashTestDummy = args.Contains("--ctd", StringComparer.OrdinalIgnoreCase);
        
        if (isDebugMode && isCrashTestDummy)
        {
            WatchdogConsole.Warn("[DEBUG] CRASH TEST DUMMY MODE ACTIVATED");
            WatchdogConsole.Warn("[DEBUG] Simulating application crash for testing...");
            RunCrashTestDummy();
            return;
        }
        else if (isCrashTestDummy && !isDebugMode)
        {
            WatchdogConsole.Error("ERROR: --ctd requires --debug flag");
            MessageBox.Show(
                "Crash Test Dummy mode requires both --debug and --ctd flags.\n\n" +
                "Usage: MillionaireGame.Watchdog.exe --debug --ctd",
                "Invalid Arguments",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        // Get application path
        string appPath;
        string[] appArgs = Array.Empty<string>();
        
        if (args.Length > 0)
        {
            appPath = args[0];
            // Pass through any additional arguments to the application
            appArgs = args.Skip(1).ToArray();
        }
        else
        {
            // Default to MillionaireGame.exe in same directory
            var watchdogDir = AppDomain.CurrentDomain.BaseDirectory;
            appPath = Path.Combine(watchdogDir, "MillionaireGame.exe");
        }

        if (!File.Exists(appPath))
        {
            WatchdogConsole.Error($"Application not found at: {appPath}");
            WatchdogConsole.Info("Usage: MillionaireGame.Watchdog.exe [path-to-MillionaireGame.exe] [app-arguments]");
            
            // Show error dialog since this is a fatal error
            MessageBox.Show(
                $"ERROR: Application not found at:\n{appPath}\n\nUsage: MillionaireGame.Watchdog.exe [path-to-MillionaireGame.exe] [app-arguments]",
                "Millionaire Game Watchdog - Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        // Start monitoring
        var monitor = new ProcessMonitor(appPath, appArgs);
        
        try
        {
            monitor.StartMonitoring();
        }
        catch (Exception ex)
        {
            WatchdogConsole.Error($"FATAL ERROR in watchdog: {ex.Message}");
            WatchdogConsole.Error($"Exception: {ex.GetType().Name}");
            WatchdogConsole.Error($"Stack trace: {ex.StackTrace}");
            
            // Show error dialog
            MessageBox.Show(
                $"Watchdog FATAL ERROR:\n{ex.Message}\n\nLog file: {WatchdogConsole.CurrentLogFilePath}",
                "Millionaire Game Watchdog - Fatal Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            monitor.Shutdown();
            WatchdogConsole.Shutdown();
        }
    }

    /// <summary>
    /// Simulates a realistic application crash for testing the crash reporting pipeline.
    /// This allows testing the entire flow: crash detection → report generation → UI → GitHub submission.
    /// </summary>
    static void RunCrashTestDummy()
    {
        try
        {
            WatchdogConsole.Info("[TEST] Generating synthetic crash data...");
            
            // Create a realistic crash scenario
            var crashInfo = new CrashInfo
            {
                ProcessId = 12345,
                ExitCode = unchecked((int)0xE0434352), // CLR exception
                CrashTime = DateTime.Now,
                RunningTime = TimeSpan.FromMinutes(15).Add(TimeSpan.FromSeconds(37)),
                LastHeartbeat = DateTime.Now.AddSeconds(-5),
                LastState = "InGame",
                LastActivity = "Player answering question 8 ($32,000)",
                LastMemoryMB = 256,  // Changed to long (was 256.5)
                LastThreadCount = 14,
                WasResponsive = true,
                ExitCodeMeaning = "CLR exception (managed code error)",
                AppVersion = "2.0.0-beta"  // Changed from ApplicationVersion
            };

            WatchdogConsole.Info("[TEST] Creating synthetic crash report file...");
            
            // Generate a test crash report
            var reportGenerator = new CrashReportGenerator();
            
            // Create synthetic crash report content
            var testReportContent = @"================================================================================
                           MILLIONAIRE GAME CRASH REPORT
================================================================================

Process ID:       12345
Exit Code:        -532462766 (0xE0434352)
Exit Meaning:     CLR exception (managed code error)
Crash Time:       " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"
Running Time:     00:15:37
Application Path: C:\Users\TestUser\AppData\Local\Programs\MillionaireGame\MillionaireGame.exe
Application Ver:  2.0.0-beta

================================================================================
                            HEARTBEAT INFORMATION
================================================================================

Last Heartbeat:   " + DateTime.Now.AddSeconds(-5).ToString("yyyy-MM-dd HH:mm:ss") + @"
Last State:       InGame
Last Activity:    Player answering question 8 ($32,000)
Memory Usage:     256.5 MB
Thread Count:     14
Was Responsive:   Yes

================================================================================
                            SYSTEM INFORMATION
================================================================================

OS Version:       Windows 11 (10.0.22631.0)
.NET Version:     8.0.11
Processor:        Intel Core i7-9700K @ 3.60GHz
RAM:              16 GB
Screen Resolution: 1920x1080

================================================================================
                            EXCEPTION DETAILS
================================================================================

Exception Type:   System.NullReferenceException
Message:          Object reference not set to an instance of an object
Source:           MillionaireGame.Core
Stack Trace:
   at MillionaireGame.Game.GameLogic.ProcessAnswer(Int32 questionId, String answer)
   at MillionaireGame.Forms.MainGameForm.OnAnswerSelected(Object sender, EventArgs e)
   at System.Windows.Forms.Control.OnClick(EventArgs e)
   at System.Windows.Forms.Button.OnClick(EventArgs e)

Inner Exception:  None

================================================================================
                            RECENT LOG ENTRIES
================================================================================

[INFO ] Game started - Difficulty: Medium
[INFO ] Question 1 loaded: What is the capital of France?
[INFO ] Player used Phone-a-Friend lifeline
[INFO ] Question 8 loaded: Which element has atomic number 79?
[WARN ] Answer validation took longer than expected (2.3s)
[ERROR] NullReferenceException in GameLogic.ProcessAnswer

================================================================================
                            END OF CRASH REPORT
================================================================================";

            // Save the test report
            var crashReportsDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MillionaireGame", "Watchdog", "CrashReports");
            Directory.CreateDirectory(crashReportsDir);
            
            var reportPath = Path.Combine(crashReportsDir, 
                $"CrashReport_TEST_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
            File.WriteAllText(reportPath, testReportContent);
            
            crashInfo.CrashReportPath = reportPath;
            
            WatchdogConsole.Info($"[TEST] Crash report saved: {reportPath}");
            WatchdogConsole.Info("[TEST] Launching crash report dialog...");
            
            // Show the crash report dialog (same as real crash)
            var dialogThread = new System.Threading.Thread(() =>
            {
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                
                WatchdogConsole.Info("[TEST] Dialog thread started");
                
                using var dialog = new CrashReportDialog(crashInfo);
                var result = dialog.ShowDialog();
                
                if (result == DialogResult.OK && dialog.ShouldSubmitToGitHub)
                {
                    WatchdogConsole.Info("[TEST] User chose to submit to GitHub - starting submission...");
                    
                    // Check if authenticated (same as real crash flow)
                    var oauthManager = new GitHubOAuthManager();
                    if (!oauthManager.IsAuthenticated())
                    {
                        WatchdogConsole.Info("[TEST] Not authenticated, showing auth dialog...");
                        
                        // Show authentication dialog (must be on STA thread)
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
                            WatchdogConsole.Info("[TEST] User cancelled authentication");
                            MessageBox.Show("Authentication cancelled. Test crash report was not submitted.",
                                "Test Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                    }
                    
                    // Read and sanitize the crash report
                    string sanitizedReport;
                    if (File.Exists(reportPath))
                    {
                        string rawReport = File.ReadAllText(reportPath);
                        sanitizedReport = DataSanitizer.SanitizeCrashReport(rawReport);
                    }
                    else
                    {
                        sanitizedReport = "Crash report file not found.";
                    }
                    
                    // Submit to GitHub using the same code path as real crashes
                    var issueSubmitter = new GitHubIssueSubmitter();
                    var submissionTask = issueSubmitter.SubmitCrashReportAsync(
                        crashInfo, 
                        dialog.UserContext, 
                        sanitizedReport);
                    submissionTask.Wait();
                    
                    var submissionResult = submissionTask.Result;
                    
                    if (submissionResult.IsSuccess)
                    {
                        WatchdogConsole.Info($"[TEST] ✓ Successfully submitted test crash as issue #{submissionResult.IssueNumber}");
                        WatchdogConsole.Info($"[TEST] Issue URL: {submissionResult.IssueUrl}");
                        
                        var confirmMessage = $"🧪 TEST CRASH REPORT SUBMITTED SUCCESSFULLY!\n\n" +
                                           $"Issue #{submissionResult.IssueNumber}\n" +
                                           $"{submissionResult.IssueUrl}\n\n" +
                                           $"This was a simulated crash for testing purposes.\n" +
                                           $"You may want to close this test issue on GitHub.";
                        
                        MessageBox.Show(confirmMessage, "Test Submission Complete",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (submissionResult.IsDuplicate)
                    {
                        WatchdogConsole.Info($"[TEST] Duplicate detected: Issue #{submissionResult.ExistingIssueNumber}");
                        
                        var duplicateMessage = $"🧪 TEST: Duplicate crash detected!\n\n" +
                                             $"Existing Issue: #{submissionResult.ExistingIssueNumber}\n" +
                                             $"{submissionResult.IssueUrl}\n\n" +
                                             $"✅ Your test crash details were added as a comment.\n" +
                                             $"The duplicate detection system is working correctly!";
                        
                        MessageBox.Show(duplicateMessage, "Duplicate Test - Success!",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        WatchdogConsole.Error($"[TEST] ✗ Failed to submit: {submissionResult.ErrorMessage}");
                        
                        MessageBox.Show(
                            $"Test submission failed:\n{submissionResult.ErrorMessage}",
                            "Test Submission Failed",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
                else
                {
                    WatchdogConsole.Info("[TEST] User cancelled crash report submission");
                }
                
                WatchdogConsole.Info("[TEST] Crash test dummy completed");
            });
            
            dialogThread.SetApartmentState(System.Threading.ApartmentState.STA);
            dialogThread.Start();
            dialogThread.Join(); // Wait for dialog to close
            
            WatchdogConsole.Info("[TEST] All crash test dummy operations complete");
        }
        catch (Exception ex)
        {
            WatchdogConsole.Error($"[TEST] ERROR in crash test dummy: {ex.Message}");
            WatchdogConsole.Error($"[TEST] Stack trace: {ex.StackTrace}");
            
            MessageBox.Show(
                $"Crash test dummy failed:\n{ex.Message}\n\nSee log for details.",
                "Test Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
