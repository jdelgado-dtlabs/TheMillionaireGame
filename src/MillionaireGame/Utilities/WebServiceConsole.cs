using System.Runtime.InteropServices;
using MillionaireGame.Forms;

namespace MillionaireGame.Utilities;

/// <summary>
/// Manages a separate window for web server logging
/// </summary>
public static class WebServiceConsole
{
    private static WebServerLogWindow? _logWindow;
    private static readonly object _lock = new object();

    /// <summary>
    /// Gets whether the console window is currently visible
    /// </summary>
    public static bool IsAllocated => _logWindow != null && _logWindow.Visible;

    /// <summary>
    /// Shows the web service log window
    /// </summary>
    public static void Show()
    {
        lock (_lock)
        {
            if (_logWindow == null || _logWindow.IsDisposed)
            {
                _logWindow = new WebServerLogWindow();
            }

            if (!_logWindow.Visible)
            {
                _logWindow.Show();
            }
        }
    }

    /// <summary>
    /// Hides the web service log window
    /// </summary>
    public static void Hide()
    {
        lock (_lock)
        {
            _logWindow?.Hide();
        }
    }

    /// <summary>
    /// Logs a message to the web service window with log level
    /// </summary>
    public static void Log(string message, LogLevel level = LogLevel.INFO)
    {
        _logWindow?.Log(message, level);
    }

    /// <summary>
    /// Logs a formatted message with arguments
    /// </summary>
    public static void Log(string format, LogLevel level = LogLevel.INFO, params object[] args)
    {
        Log(string.Format(format, args), level);
    }

    // Convenience methods for specific log levels
    public static void Debug(string message) => Log(message, LogLevel.DEBUG);
    public static void Info(string message) => Log(message, LogLevel.INFO);
    public static void Warn(string message) => Log(message, LogLevel.WARN);
    public static void Error(string message) => Log(message, LogLevel.ERROR);

    /// <summary>
    /// Logs a separator line
    /// </summary>
    public static void LogSeparator()
    {
        _logWindow?.LogSeparator();
    }
}
