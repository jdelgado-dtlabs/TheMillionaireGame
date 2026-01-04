using System.Collections.Concurrent;
using System.Globalization;
using System.Text;

namespace MillionaireGame.Utilities;

/// <summary>
/// Primary file-based logging system with async queue processing
/// Logs are written to disk first, then UI windows tail the files
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

    public FileLogger(string logPrefix, int maxLogFiles = 5)
    {
        _logPrefix = logPrefix;
        _maxLogFiles = maxLogFiles;
        
        // Store logs in AppData\Local\TheMillionaireGame\Logs
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataPath, "TheMillionaireGame");
        _logDirectory = Path.Combine(appFolder, "Logs");
        
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
    /// Logs a formatted message with arguments
    /// </summary>
    public void Log(string format, LogLevel level, params object[] args)
    {
        Log(string.Format(format, args), level);
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
    /// Cleans up old log files, keeping only the most recent ones
    /// </summary>
    private void CleanupOldLogs()
    {
        try
        {
            var logFiles = Directory.GetFiles(_logDirectory, $"*_{_logPrefix}.log")
                .OrderByDescending(f => File.GetCreationTime(f))
                .ToList();

            // Delete files beyond the limit
            foreach (var file in logFiles.Skip(_maxLogFiles))
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                    // Ignore errors deleting old logs
                }
            }
        }
        catch
        {
            // Ignore errors during cleanup
        }
    }

    /// <summary>
    /// Reads all lines from the current log file
    /// </summary>
    public List<string> ReadAllLines()
    {
        if (CurrentLogFilePath == null || !File.Exists(CurrentLogFilePath))
            return new List<string>();

        try
        {
            // Read without locking to allow concurrent writes
            using var fs = new FileStream(CurrentLogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(fs, Encoding.UTF8);
            
            var lines = new List<string>();
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                lines.Add(line);
            }
            return lines;
        }
        catch
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// Reads the last N lines from the current log file
    /// </summary>
    public List<string> ReadLastLines(int count)
    {
        var allLines = ReadAllLines();
        return allLines.Skip(Math.Max(0, allLines.Count - count)).ToList();
    }

    /// <summary>
    /// Flushes pending log messages and closes the log file
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        try
        {
            // Signal cancellation
            _cts.Cancel();

            // Wait for writer task to complete (up to 2 seconds)
            _writerTask.Wait(2000);

            // Close file writer
            lock (_lock)
            {
                _logWriter?.Close();
                _logWriter?.Dispose();
                _logWriter = null;
            }

            _cts.Dispose();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FileLogger] Error during disposal: {ex.Message}");
        }
    }
}
