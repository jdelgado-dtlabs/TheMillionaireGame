using System.Globalization;
using System.Text;

namespace MillionaireGame.Utilities;

/// <summary>
/// Manages console output with file logging and automatic log rotation
/// </summary>
public class ConsoleLogger
{
    private readonly string _logDirectory;
    private readonly string _logPrefix;
    private readonly int _maxLogFiles;
    private StreamWriter? _logWriter;
    private readonly object _lock = new object();

    public ConsoleLogger(string logPrefix, int maxLogFiles = 5)
    {
        _logPrefix = logPrefix;
        _maxLogFiles = maxLogFiles;
        
        // Store logs in application directory under "Logs" folder
        var appPath = AppDomain.CurrentDomain.BaseDirectory;
        _logDirectory = Path.Combine(appPath, "Logs");
        
        // Ensure logs directory exists
        if (!Directory.Exists(_logDirectory))
        {
            Directory.CreateDirectory(_logDirectory);
        }
    }

    /// <summary>
    /// Starts logging to a new file with timestamp
    /// </summary>
    public void StartLogging()
    {
        lock (_lock)
        {
            // Close existing writer if any
            _logWriter?.Close();
            _logWriter?.Dispose();

            // Create log file with Windows-compatible date format
            var timestamp = GetWindowsFormattedTimestamp();
            var fileName = $"{timestamp}_{_logPrefix}.log";
            var filePath = Path.Combine(_logDirectory, fileName);

            // Create new log file
            _logWriter = new StreamWriter(filePath, false, Encoding.UTF8)
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
    /// Logs a message to both console and file
    /// </summary>
    public void Log(string message)
    {
        lock (_lock)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var formattedMessage = $"[{timestamp}] {message}";
            
            // Only write to file, not Console (GameLogWindow handles UI display)
            _logWriter?.WriteLine(formattedMessage);
        }
    }

    /// <summary>
    /// Logs a formatted message
    /// </summary>
    public void Log(string format, params object[] args)
    {
        Log(string.Format(format, args));
    }

    /// <summary>
    /// Logs a separator line
    /// </summary>
    public void LogSeparator()
    {
        lock (_lock)
        {
            var separator = "-------------------------------------------";
            Console.WriteLine(separator);
            _logWriter?.WriteLine(separator);
        }
    }

    /// <summary>
    /// Closes the log file
    /// </summary>
    public void Close()
    {
        lock (_lock)
        {
            _logWriter?.Close();
            _logWriter?.Dispose();
            _logWriter = null;
        }
    }

    /// <summary>
    /// Gets a Windows-formatted timestamp for filename based on system culture
    /// Uses sortable format that respects local date ordering
    /// </summary>
    private string GetWindowsFormattedTimestamp()
    {
        var now = DateTime.Now;
        var culture = CultureInfo.CurrentCulture;
        
        // Get the date separator from culture (typically - or /)
        var dateSeparator = culture.DateTimeFormat.DateSeparator;
        
        // Build date string based on culture's short date pattern
        var shortDatePattern = culture.DateTimeFormat.ShortDatePattern.ToLower();
        
        string dateString;
        if (shortDatePattern.StartsWith("m") || shortDatePattern.StartsWith("d/m") || shortDatePattern.StartsWith("d.m"))
        {
            // European format: DD-MM-YYYY
            dateString = $"{now:dd-MM-yyyy}";
        }
        else if (shortDatePattern.StartsWith("y"))
        {
            // ISO format: YYYY-MM-DD (most compatible)
            dateString = $"{now:yyyy-MM-dd}";
        }
        else
        {
            // US format: MM-DD-YYYY
            dateString = $"{now:MM-dd-yyyy}";
        }
        
        // Time is always 24-hour format for consistency
        var timeString = $"{now:HH-mm}";
        
        return $"{dateString}_{timeString}";
    }

    /// <summary>
    /// Removes old log files, keeping only the most recent ones
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

            // Delete old files if we have more than max
            if (logFiles.Count > _maxLogFiles)
            {
                var filesToDelete = logFiles.Skip(_maxLogFiles);
                foreach (var file in filesToDelete)
                {
                    try
                    {
                        file.Delete();
                        Console.WriteLine($"Deleted old log file: {file.Name}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Warning: Could not delete old log file {file.Name}: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Error during log cleanup: {ex.Message}");
        }
    }
}
