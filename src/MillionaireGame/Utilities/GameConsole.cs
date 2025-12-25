using MillionaireGame.Forms;

namespace MillionaireGame.Utilities;

/// <summary>
/// Log level enumeration following Linux conventions
/// </summary>
public enum LogLevel
{
    DEBUG,  // Only shown in debug mode
    INFO,   // Normal informational messages
    WARN,   // Warning messages
    ERROR   // Error messages
}

/// <summary>
/// Manages a separate window for game logging
/// </summary>
public static class GameConsole
{
    private static GameLogWindow? _logWindow;
    private static readonly object _lock = new object();

    /// <summary>
    /// Gets whether the console window is currently visible
    /// </summary>
    public static bool IsVisible => _logWindow != null && _logWindow.Visible;

    /// <summary>
    /// Sets the log window instance (called from Program.cs)
    /// </summary>
    public static void SetWindow(GameLogWindow window)
    {
        lock (_lock)
        {
            _logWindow = window;
            System.Diagnostics.Debug.WriteLine($"[GameConsole] Window set: {window != null}, IsDisposed: {window?.IsDisposed}");
        }
    }

    /// <summary>
    /// Shows the game log window
    /// </summary>
    public static void Show()
    {
        lock (_lock)
        {
            if (_logWindow == null || _logWindow.IsDisposed)
            {
                _logWindow = new GameLogWindow();
            }

            if (!_logWindow.Visible)
            {
                _logWindow.Show();
            }
        }
    }

    /// <summary>
    /// Hides the game log window
    /// </summary>
    public static void Hide()
    {
        lock (_lock)
        {
            _logWindow?.Hide();
        }
    }

    /// <summary>
    /// Logs a message to the game console window with specified log level
    /// </summary>
    public static void Log(string message, LogLevel level = LogLevel.INFO)
    {
        // Skip DEBUG messages if not in debug mode
        if (level == LogLevel.DEBUG && !Program.DebugMode)
            return;

        lock (_lock)
        {
            if (_logWindow != null && !_logWindow.IsDisposed)
            {
                _logWindow.Log(message, level);
            }
            else
            {
                // Fallback to debug output if window not available
                System.Diagnostics.Debug.WriteLine($"[GameConsole] Window not available: {message}");
            }
        }
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
