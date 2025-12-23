using System.Runtime.InteropServices;

namespace MillionaireGame.Utilities;

/// <summary>
/// Manages a separate console window for web server logging
/// </summary>
public static class WebServiceConsole
{
    [DllImport("kernel32.dll")]
    private static extern bool AllocConsole();

    [DllImport("kernel32.dll")]
    private static extern bool FreeConsole();

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetConsoleTitle(string lpConsoleTitle);

    private static bool _isAllocated = false;
    private static readonly object _lock = new object();

    /// <summary>
    /// Gets whether the console is currently allocated
    /// </summary>
    public static bool IsAllocated => _isAllocated;

    /// <summary>
    /// Allocates and shows the web service console
    /// </summary>
    public static void Show()
    {
        lock (_lock)
        {
            if (_isAllocated)
                return;

            AllocConsole();
            SetConsoleTitle("WebService");
            
            Console.WriteLine("===========================================");
            Console.WriteLine("  WEB-BASED AUDIENCE PARTICIPATION");
            Console.WriteLine("  Service Console");
            Console.WriteLine("===========================================");
            Console.WriteLine($"Started at {DateTime.Now}");
            Console.WriteLine();

            _isAllocated = true;
        }
    }

    /// <summary>
    /// Hides and frees the web service console
    /// </summary>
    public static void Hide()
    {
        lock (_lock)
        {
            if (!_isAllocated)
                return;

            FreeConsole();
            _isAllocated = false;
        }
    }

    /// <summary>
    /// Logs a message to the web service console
    /// </summary>
    public static void Log(string message)
    {
        if (!_isAllocated)
            return;

        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        Console.WriteLine($"[{timestamp}] {message}");
    }

    /// <summary>
    /// Logs a formatted message with arguments
    /// </summary>
    public static void Log(string format, params object[] args)
    {
        if (!_isAllocated)
            return;

        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        Console.WriteLine($"[{timestamp}] {string.Format(format, args)}");
    }

    /// <summary>
    /// Logs a separator line
    /// </summary>
    public static void LogSeparator()
    {
        if (!_isAllocated)
            return;

        Console.WriteLine("-------------------------------------------");
    }
}
