using System.Reflection;

namespace MillionaireGame.Watchdog;

class Program
{
    static void Main(string[] args)
    {
        Console.Title = "Millionaire Game - Watchdog";
        
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        Console.WriteLine("====================================");
        Console.WriteLine("Millionaire Game Watchdog");
        Console.WriteLine($"Version {version?.ToString(3) ?? "1.0.0"}");
        Console.WriteLine("====================================");
        Console.WriteLine();

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
            Console.WriteLine($"ERROR: Application not found at: {appPath}");
            Console.WriteLine();
            Console.WriteLine("Usage: MillionaireGame.Watchdog.exe [path-to-MillionaireGame.exe] [app-arguments]");
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
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
            Console.WriteLine($"[Watchdog] FATAL ERROR: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
        finally
        {
            monitor.Shutdown();
        }
    }
}
