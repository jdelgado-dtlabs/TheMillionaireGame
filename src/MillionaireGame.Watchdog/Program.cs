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
}
