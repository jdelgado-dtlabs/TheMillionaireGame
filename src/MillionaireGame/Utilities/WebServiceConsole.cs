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
    /// Logs a message to the web service window
    /// </summary>
    public static void Log(string message)
    {
        _logWindow?.Log(message);
    }

    /// <summary>
    /// Logs a formatted message with arguments
    /// </summary>
    public static void Log(string format, params object[] args)
    {
        _logWindow?.Log(format, args);
    }

    /// <summary>
    /// Logs a separator line
    /// </summary>
    public static void LogSeparator()
    {
        _logWindow?.LogSeparator();
    }
}
