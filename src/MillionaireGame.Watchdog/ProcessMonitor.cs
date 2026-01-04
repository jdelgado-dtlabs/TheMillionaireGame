using System.Diagnostics;

namespace MillionaireGame.Watchdog;

/// <summary>
/// Monitors the main application process for crashes and freezes
/// </summary>
public class ProcessMonitor
{
    private readonly string _applicationPath;
    private readonly HeartbeatListener _heartbeatListener;
    private readonly CrashReportGenerator _reportGenerator;
    private Process? _process;
    private DateTime _processStartTime;
    private bool _shutdownRequested;

    public ProcessMonitor(string applicationPath)
    {
        _applicationPath = applicationPath;
        _heartbeatListener = new HeartbeatListener();
        _reportGenerator = new CrashReportGenerator();

        _heartbeatListener.HeartbeatReceived += OnHeartbeatReceived;
        _heartbeatListener.HeartbeatTimeout += OnHeartbeatTimeout;
    }

    public void StartMonitoring()
    {
        Console.WriteLine("[Watchdog] Starting process monitor...");
        Console.WriteLine($"[Watchdog] Application path: {_applicationPath}");

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
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                CreateNoWindow = false
            };

            _process = Process.Start(startInfo);
            if (_process == null)
            {
                Console.WriteLine("[Watchdog] ERROR: Failed to start application process");
                return;
            }

            Console.WriteLine($"[Watchdog] Application started (PID: {_process.Id})");

            // Monitor process
            _process.EnableRaisingEvents = true;
            _process.Exited += OnProcessExited;

            // Wait for process to exit
            _process.WaitForExit();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Watchdog] ERROR launching application: {ex.Message}");
        }
    }

    private void OnProcessExited(object? sender, EventArgs e)
    {
        if (_process == null)
            return;

        var exitCode = _process.ExitCode;
        var runningTime = DateTime.Now - _processStartTime;
        var lastHeartbeat = _heartbeatListener.GetLastHeartbeat();

        Console.WriteLine($"[Watchdog] Application exited (Exit Code: {exitCode}, Running Time: {runningTime:hh\\:mm\\:ss})");

        // Determine if this was a crash
        var isCrash = DetermineCrash(exitCode, lastHeartbeat);

        if (isCrash)
        {
            Console.WriteLine("[Watchdog] CRASH DETECTED - Generating crash report...");
            
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
            Console.WriteLine("[Watchdog] Clean shutdown detected");
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
            Console.WriteLine("[Watchdog] Application is shutting down gracefully");
        }
    }

    private void OnHeartbeatTimeout(object? sender, EventArgs e)
    {
        if (_shutdownRequested)
            return;

        Console.WriteLine("[Watchdog] APPLICATION FREEZE DETECTED - No heartbeat received");
        
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

            Console.WriteLine("[Watchdog] Terminating frozen process...");
            try
            {
                _process.Kill();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Watchdog] Error killing process: {ex.Message}");
            }

            HandleCrash(crashInfo);
        }
    }

    private void HandleCrash(CrashInfo crashInfo)
    {
        // Generate crash report
        var reportPath = _reportGenerator.GenerateReport(crashInfo);
        
        // Show user notification
        Console.WriteLine();
        Console.WriteLine("====================================");
        Console.WriteLine("APPLICATION CRASH DETECTED");
        Console.WriteLine("====================================");
        Console.WriteLine($"The application has crashed unexpectedly.");
        Console.WriteLine($"Exit Code: {crashInfo.ExitCode} (0x{crashInfo.ExitCode:X8})");
        Console.WriteLine();
        Console.WriteLine($"A crash report has been saved to:");
        Console.WriteLine(reportPath);
        Console.WriteLine();
        Console.WriteLine("You can submit this report to help us fix the issue.");
        Console.WriteLine("====================================");
        Console.WriteLine();
        Console.WriteLine("Press any key to exit watchdog...");
        Console.ReadKey();
    }

    public void Shutdown()
    {
        _heartbeatListener.Stop();
        _heartbeatListener.Dispose();
    }
}
