using MillionaireGame.Forms;
using System.Collections.Concurrent;

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
/// Manages a separate window for game logging with async background processing
/// </summary>
public static class GameConsole
{
    private static GameLogWindow? _logWindow;
    private static readonly object _lock = new object();
    private static readonly ConcurrentQueue<(string message, LogLevel level)> _logQueue = new();
    private static readonly CancellationTokenSource _cts = new();
    private static readonly Task _logTask;
    
    static GameConsole()
    {
        // Start background thread to process log messages
        _logTask = Task.Run(ProcessLogQueue, _cts.Token);
    }

    /// <summary>
    /// Gets whether the console window is currently visible
    /// </summary>
    public static bool IsVisible => _logWindow != null && _logWindow.Visible;

    /// <summary>
    /// Background task that processes queued log messages
    /// </summary>
    private static async Task ProcessLogQueue()
    {
        while (!_cts.Token.IsCancellationRequested)
        {
            try
            {
                if (_logQueue.TryDequeue(out var logEntry))
                {
                    // Process log message
                    lock (_lock)
                    {
                        // Always log - window will handle file logging even if hidden
                        if (_logWindow != null && !_logWindow.IsDisposed)
                        {
                            _logWindow.Log(logEntry.message, logEntry.level);
                        }
                        else
                        {
                            // No window - just write to debug console
                            System.Diagnostics.Debug.WriteLine($"[{logEntry.level}] {logEntry.message}");
                        }
                    }
                }
                else
                {
                    // No messages - wait a bit before checking again
                    await Task.Delay(10, _cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GameConsole] Error processing log: {ex.Message}");
            }
        }
    }

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
    /// Hides the game log window (file logging continues)
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
    /// Logs a message to the game console window with specified log level
    /// Non-blocking - queues message for background processing
    /// </summary>
    public static void Log(string message, LogLevel level = LogLevel.INFO)
    {
        // Skip DEBUG messages if not in debug mode
        if (level == LogLevel.DEBUG && !Program.DebugMode)
            return;

        // Queue message for async processing - never blocks
        _logQueue.Enqueue((message, level));
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

    /// <summary>
    /// Shutdown the logging system and flush pending messages
    /// </summary>
    public static void Shutdown()
    {
        try
        {
            // Signal cancellation
            _cts.Cancel();

            // Wait for log task to complete (up to 1 second)
            _logTask.Wait(1000);

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
            // Can't log to GameConsole here, just fail silently
            System.Diagnostics.Debug.WriteLine($"GameConsole shutdown error: {ex.Message}");
        }
    }
}
