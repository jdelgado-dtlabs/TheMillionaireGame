using System.Collections.Concurrent;
using System.Globalization;
using System.Text;

namespace MillionaireGame.Watchdog;

/// <summary>
/// Primary file-based logging system with async queue processing
/// Logs are written to disk first
/// Thread-safe with automatic log rotation and cleanup
/// </summary>
public class FileLogger : IDisposable
{
    private readonly string _logDirectory;
    private readonly string _logPrefix;
    private readonly int _maxLogFiles;
    private readonly ConcurrentQueue<string> _logQueue = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _writerTask;
    private StreamWriter? _logWriter;
    private readonly object _lock = new();
    private bool _disposed;

    /// <summary>
    /// Gets the full path to the current log file
    /// </summary>
    public string? CurrentLogFilePath { get; private set; }

    /// <summary>
    /// Event raised when a new log entry is written
    /// </summary>
    public event EventHandler<string>? LogWritten;

    /// <summary>
    /// Creates a FileLogger with the default app data directory
    /// </summary>
    public FileLogger(string logPrefix, int maxLogFiles = 5)
        : this(logPrefix, maxLogFiles, null)
    {
    }

    /// <summary>
    /// Creates a FileLogger with a custom log directory (primarily for testing)
    /// </summary>
    /// <param name="logPrefix">Prefix for log files</param>
    /// <param name="maxLogFiles">Maximum number of log files to retain</param>
    /// <param name="customLogDirectory">Custom directory for logs. If null, uses AppData\Local\TheMillionaireGame\Logs</param>
    public FileLogger(string logPrefix, int maxLogFiles = 5, string? customLogDirectory = null)
    {
        _logPrefix = logPrefix;
        _maxLogFiles = maxLogFiles;
        
        if (customLogDirectory != null)
        {
            _logDirectory = customLogDirectory;
        }
        else
        {
            // Store logs in AppData\Local\TheMillionaireGame\Logs (same location as main app)
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(appDataPath, "TheMillionaireGame");
            _logDirectory = Path.Combine(appFolder, "Logs");
        }
        
        // Ensure logs directory exists
        if (!Directory.Exists(_logDirectory))
        {
            Directory.CreateDirectory(_logDirectory);
        }

        // Initialize log file
        InitializeLogFile();

        // Start background writer task
        _writerTask = Task.Run(ProcessLogQueue, _cts.Token);
    }

    /// <summary>
    /// Initializes a new log file with timestamp and header
    /// </summary>
    private void InitializeLogFile()
    {
        lock (_lock)
        {
            // Close existing writer if any
            _logWriter?.Close();
            _logWriter?.Dispose();

            // Create log file with Windows-compatible date format
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture);
            var fileName = $"{timestamp}_{_logPrefix}.log";
            CurrentLogFilePath = Path.Combine(_logDirectory, fileName);

            // Create new log file
            _logWriter = new StreamWriter(CurrentLogFilePath, false, Encoding.UTF8)
            {
                AutoFlush = true
            };

            // Write header
            _logWriter.WriteLine("===========================================");
            _logWriter.WriteLine($"  LOG FILE: {_logPrefix}");
            _logWriter.WriteLine($"  Started: {DateTime.Now}");
            _logWriter.WriteLine("===========================================");
            _logWriter.WriteLine();

            // Cleanup old log files
            CleanupOldLogs();
        }
    }

    /// <summary>
    /// Background task that processes queued log messages
    /// </summary>
    private async Task ProcessLogQueue()
    {
        while (!_cts.Token.IsCancellationRequested)
        {
            try
            {
                if (_logQueue.TryDequeue(out var message))
                {
                    lock (_lock)
                    {
                        if (_logWriter != null && !_disposed)
                        {
                            _logWriter.WriteLine(message);
                            
                            // Notify listeners that a log was written
                            LogWritten?.Invoke(this, message);
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
                System.Diagnostics.Debug.WriteLine($"[FileLogger] Error processing log: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Logs a message to the file (non-blocking)
    /// </summary>
    public void Log(string message, LogLevel level = LogLevel.INFO)
    {
        if (_disposed)
            return;

        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        var levelStr = level switch
        {
            LogLevel.DEBUG => "DEBUG",
            LogLevel.INFO => "INFO",
            LogLevel.WARN => "WARN",
            LogLevel.ERROR => "ERROR",
            _ => "INFO"
        };
        
        var formattedMessage = $"[{timestamp}] [{levelStr}] {message}";
        
        // Queue message for async processing - never blocks
        _logQueue.Enqueue(formattedMessage);
    }

    /// <summary>
    /// Logs a separator line
    /// </summary>
    public void LogSeparator()
    {
        if (_disposed)
            return;

        _logQueue.Enqueue("-------------------------------------------");
    }

    /// <summary>
    /// Removes old log files, keeping only the most recent N files
    /// </summary>
    private void CleanupOldLogs()
    {
        try
        {
            // Get all log files for this prefix
            var pattern = $"*_{_logPrefix}.log";
            var logFiles = Directory.GetFiles(_logDirectory, pattern)
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.CreationTime)
                .ToList();

            // Keep the most recent N files, delete the rest
            foreach (var file in logFiles.Skip(_maxLogFiles))
            {
                try
                {
                    file.Delete();
                }
                catch
                {
                    // If we can't delete a file, just skip it
                }
            }
        }
        catch
        {
            // Cleanup failure is non-critical
        }
    }

    /// <summary>
    /// Disposes the logger and flushes all pending messages
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        try
        {
            // Stop the writer task
            _cts.Cancel();

            // Wait for remaining messages to be written (max 5 seconds)
            var timeout = Task.Delay(5000);
            var writerCompleted = _writerTask;
            Task.WaitAny(writerCompleted, timeout);

            // Close and dispose the writer
            lock (_lock)
            {
                _logWriter?.Flush();
                _logWriter?.Close();
                _logWriter?.Dispose();
                _logWriter = null;
            }

            _cts.Dispose();
        }
        catch
        {
            // Errors during dispose are non-critical
        }
    }
}
