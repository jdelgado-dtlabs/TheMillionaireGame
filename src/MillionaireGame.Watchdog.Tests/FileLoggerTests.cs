using Xunit;

namespace MillionaireGame.Watchdog.Tests;

/// <summary>
/// Unit tests for FileLogger functionality.
/// </summary>
public class FileLoggerTests : IDisposable
{
    private readonly string _testLogDirectory;
    private readonly FileLogger _logger;

    public FileLoggerTests()
    {
        // Create a temporary directory for test logs
        _testLogDirectory = Path.Combine(Path.GetTempPath(), $"WatchdogTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testLogDirectory);

        _logger = new FileLogger("watchdog", 5, _testLogDirectory);
    }

    [Fact]
    public async Task Log_CreatesLogFile()
    {
        // Arrange
        string testMessage = "Test log message";

        // Act
        _logger.Log(testMessage);
        await Task.Delay(500); // Give async queue time to process

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "watchdog_*.log");
        Assert.NotEmpty(logFiles);
    }

    [Fact]
    public async Task Log_WritesMessageToFile()
    {
        // Arrange
        string testMessage = "Test message with special chars: @#$%";

        // Act
        _logger.Log(testMessage);
        await Task.Delay(500); // Give async queue time to process

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "*_watchdog.log");
        Assert.NotEmpty(logFiles);

        string logContent = await File.ReadAllTextAsync(logFiles[0]);
        Assert.Contains(testMessage, logContent);
    }

    [Fact]
    public async Task Log_PreservesMessageOrder()
    {
        // Arrange
        var messages = new[] { "Message 1", "Message 2", "Message 3", "Message 4", "Message 5" };

        // Act
        foreach (var message in messages)
        {
            _logger.Log(message);
        }
        await Task.Delay(500); // Give async queue time to process

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "*_watchdog.log");
        Assert.NotEmpty(logFiles);

        string logContent = await File.ReadAllTextAsync(logFiles[0]);
        
        // Verify all messages are present in order
        int lastIndex = -1;
        foreach (var message in messages)
        {
            int currentIndex = logContent.IndexOf(message);
            Assert.True(currentIndex > lastIndex, $"Message '{message}' not in expected order");
            lastIndex = currentIndex;
        }
    }

    [Fact]
    public async Task CleanupOldLogs_RemovesOldFiles()
    {
        // Arrange - Create more than 5 log files
        for (int i = 0; i < 7; i++)
        {
            string fileName = $"2026-01-0{i+1}_00-00-00_watchdog.log";
            string filePath = Path.Combine(_testLogDirectory, fileName);
            await File.WriteAllTextAsync(filePath, $"Test log {i}");
        }

        // Act
        var logger = new FileLogger("watchdog", 5, _testLogDirectory);
        await Task.Delay(200); // Let cleanup run

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "*_watchdog.log");
        Assert.True(logFiles.Length <= 5, $"Expected <= 5 log files, but found {logFiles.Length}");
        
        logger.Dispose();
    }

    [Fact]
    public async Task Log_HandlesConcurrentWrites()
    {
        // Arrange
        const int messageCount = 100;
        var tasks = new List<Task>();

        // Act - Simulate concurrent logging from multiple threads
        for (int i = 0; i < messageCount; i++)
        {
            int messageId = i;
            tasks.Add(Task.Run(() => _logger.Log($"Concurrent message {messageId}")));
        }

        await Task.WhenAll(tasks);
        await Task.Delay(1000); // Give async queue time to process all messages

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "*_watchdog.log");
        Assert.NotEmpty(logFiles);

        string logContent = await File.ReadAllTextAsync(logFiles[0]);
        
        // Verify all messages were written
        for (int i = 0; i < messageCount; i++)
        {
            Assert.Contains($"Concurrent message {i}", logContent);
        }
    }

    public void Dispose()
    {
        // Cleanup: Dispose logger and delete test directory
        _logger?.Dispose();
        
        if (Directory.Exists(_testLogDirectory))
        {
            try
            {
                Directory.Delete(_testLogDirectory, true);
            }
            catch
            {
                // Ignore cleanup errors in tests
            }
        }
    }
}
