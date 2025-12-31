using MillionaireGame.Forms;

namespace MillionaireGame.Utilities;

/// <summary>
/// Static console for web server logging with file-first architecture
/// Logs are written to disk first, then UI window tails the file
/// </summary>
public static class WebServerConsole
{
    private static WebServerLogWindow? _logWindow;
    private static readonly FileLogger _fileLogger = new("webserver", 5);
    private static readonly object _lock = new();

    /// <summary>
    /// Gets whether the console window is currently visible
    /// </summary>
    public static bool IsAllocated => _logWindow != null && _logWindow.Visible;

    /// <summary>
    /// Gets the current log file path
    /// </summary>
    public static string? CurrentLogFilePath => _fileLogger.CurrentLogFilePath;

    /// <summary>
    /// Sets the log window instance
    /// </summary>
    public static void SetWindow(WebServerLogWindow window)
    {
        lock (_lock)
        {
            _logWindow = window;
            System.Diagnostics.Debug.WriteLine($"[WebServerConsole] Window set: {window != null}, IsDisposed: {window?.IsDisposed}");
        }
    }

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
    /// Hides the web service log window (file logging continues)
    /// </summary>
    public static void Hide()
    {
        lock (_lock)
        {
            if (_logWindow != null && !_logWindow.IsDisposed)
            {
                _logWindow.Hide();
            }
        }
    }

    /// <summary>
    /// Logs a message to file and notifies the window
    /// Non-blocking - queues message for background processing
    /// </summary>
    public static void Log(string message, LogLevel level = LogLevel.INFO)
    {
        // Skip DEBUG messages if not in debug mode
        if (level == LogLevel.DEBUG && !Program.DebugMode)
            return;

        // Write to file first (primary log destination)
        _fileLogger.Log(message, level);

        // Notify window if it exists (secondary - for UI display)
        lock (_lock)
        {
            if (_logWindow != null && !_logWindow.IsDisposed && _logWindow.Visible)
            {
                try
                {
                    _logWindow.NotifyLogUpdated();
                }
                catch
                {
                    // Window might be closing, ignore
                }
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
        _fileLogger.LogSeparator();
        
        lock (_lock)
        {
            if (_logWindow != null && !_logWindow.IsDisposed && _logWindow.Visible)
            {
                try
                {
                    _logWindow.NotifyLogUpdated();
                }
                catch
                {
                    // Window might be closing, ignore
                }
            }
        }
    }

    /// <summary>
    /// Shutdown the logging system and flush pending messages
    /// </summary>
    public static void Shutdown()
    {
        try
        {
            // Close file logger
            _fileLogger.Dispose();

            // Close window
            lock (_lock)
            {
                if (_logWindow != null && !_logWindow.IsDisposed)
                {
                    try
                    {
                        _logWindow.Invoke(new Action(() =>
                        {
                            _logWindow.Close();
                            _logWindow.Dispose();
                        }));
                    }
                    catch
                    {
                        // Window might already be disposed
                    }
                }
                _logWindow = null;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"WebServerConsole shutdown error: {ex.Message}");
        }
    }
}
