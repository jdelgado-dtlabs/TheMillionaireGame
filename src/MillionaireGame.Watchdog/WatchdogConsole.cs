namespace MillionaireGame.Watchdog;

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
/// Static console for watchdog logging with file-first architecture
/// Logs are written to disk first, similar to GameConsole/WebServerConsole
/// No UI window - watchdog runs completely hidden
/// </summary>
public static class WatchdogConsole
{
    private static FileLogger _fileLogger = new("watchdog", 5);
    private static readonly object _lock = new();

    /// <summary>
    /// Gets the current log file path
    /// </summary>
    public static string? CurrentLogFilePath => _fileLogger.CurrentLogFilePath;

    /// <summary>
    /// Initializes the console with a custom log directory (for testing)
    /// </summary>
    public static void Initialize(string logDirectory)
    {
        lock (_lock)
        {
            _fileLogger?.Dispose();
            _fileLogger = new("watchdog", 5, logDirectory);
        }
    }

    /// <summary>
    /// Logs a message to file
    /// Non-blocking - queues message for background processing
    /// </summary>
    public static void Log(string message, LogLevel level = LogLevel.INFO)
    {
        // Write to file (primary and only destination for watchdog)
        _fileLogger.Log(message, level);
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
    }

    /// <summary>
    /// Shutdown the logging system and flush pending messages
    /// </summary>
    public static void Shutdown()
    {
        try
        {
            _fileLogger.Dispose();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"WatchdogConsole shutdown error: {ex.Message}");
        }
    }
}
