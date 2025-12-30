using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using MillionaireGame.Forms;

namespace MillionaireGame.Utilities;

/// <summary>
/// Manages a separate window for web server logging with async background processing
/// </summary>
public static class WebServerConsole
{
    private static WebServerLogWindow? _logWindow;
    private static readonly object _lock = new object();
    
    // Async logging infrastructure (same pattern as GameConsole)
    private static readonly ConcurrentQueue<(string message, LogLevel level, bool isSeparator)> _logQueue = new();
    private static readonly CancellationTokenSource _cts = new();
    private static readonly Task _logTask;

    static WebServerConsole()
    {
        // Start background processing task
        _logTask = Task.Run(ProcessLogQueue, _cts.Token);
    }

    /// <summary>
    /// Background task that processes log messages asynchronously
    /// </summary>
    private static async Task ProcessLogQueue()
    {
        while (!_cts.Token.IsCancellationRequested)
        {
            if (_logQueue.TryDequeue(out var logEntry))
            {
                lock (_lock)
                {
                    if (_logWindow != null && !_logWindow.IsDisposed)
                    {
                        if (logEntry.isSeparator)
                        {
                            _logWindow.LogSeparator();
                        }
                        else
                        {
                            _logWindow.Log(logEntry.message, logEntry.level);
                        }
                    }
                }
            }
            else
            {
                // No messages, wait briefly before checking again
                try
                {
                    await Task.Delay(10, _cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }

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
    /// Logs a message to the web service window with log level (non-blocking, async)
    /// </summary>
    public static void Log(string message, LogLevel level = LogLevel.INFO)
    {
        // Just enqueue the message - never blocks!
        _logQueue.Enqueue((message, level, false));
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
    /// Logs a separator line (non-blocking, async)
    /// </summary>
    public static void LogSeparator()
    {
        // Just enqueue the separator request - never blocks!
        _logQueue.Enqueue((string.Empty, LogLevel.INFO, true));
    }
    
    /// <summary>
    /// Shuts down the background logging task
    /// </summary>
    public static void Shutdown()
    {
        _cts.Cancel();
        try
        {
            _logTask.Wait(1000); // Wait up to 1 second for clean shutdown
        }
        catch { /* Ignore timeout */ }
    }
}
