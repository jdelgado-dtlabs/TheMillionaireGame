using Xunit;

namespace MillionaireGame.Watchdog.Tests;

/// <summary>
/// Unit tests for WatchdogConsole logging functionality.
/// </summary>
public class WatchdogConsoleTests : IDisposable
{
    private readonly string _testLogDirectory;

    public WatchdogConsoleTests()
    {
        // Create a temporary directory for test logs
        _testLogDirectory = Path.Combine(Path.GetTempPath(), $"WatchdogConsoleTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testLogDirectory);

        // Initialize WatchdogConsole with test directory
        WatchdogConsole.Initialize(_testLogDirectory);
    }

    [Fact]
    public async Task Debug_WritesDebugLevelMessage()
    {
        // Act
        WatchdogConsole.Debug("Debug test message");
        await Task.Delay(500); // Give async queue time to process

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "*_watchdog.log");
        Assert.NotEmpty(logFiles);

        string logContent = await File.ReadAllTextAsync(logFiles[0]);
        Assert.Contains("[DEBUG]", logContent);
        Assert.Contains("Debug test message", logContent);
    }

    [Fact]
    public async Task Info_WritesInfoLevelMessage()
    {
        // Act
        WatchdogConsole.Info("Info test message");
        await Task.Delay(500);

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "*_watchdog.log");
        Assert.NotEmpty(logFiles);

        string logContent = await File.ReadAllTextAsync(logFiles[0]);
        Assert.Contains("[INFO]", logContent);
        Assert.Contains("Info test message", logContent);
    }

    [Fact]
    public async Task Warn_WritesWarnLevelMessage()
    {
        // Act
        WatchdogConsole.Warn("Warning test message");
        await Task.Delay(500);

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "*_watchdog.log");
        Assert.NotEmpty(logFiles);

        string logContent = await File.ReadAllTextAsync(logFiles[0]);
        Assert.Contains("[WARN]", logContent);
        Assert.Contains("Warning test message", logContent);
    }

    [Fact]
    public async Task Error_WritesErrorLevelMessage()
    {
        // Act
        WatchdogConsole.Error("Error test message");
        await Task.Delay(500);

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "*_watchdog.log");
        Assert.NotEmpty(logFiles);

        string logContent = await File.ReadAllTextAsync(logFiles[0]);
        Assert.Contains("[ERROR]", logContent);
        Assert.Contains("Error test message", logContent);
    }

    [Fact]
    public async Task LogSeparator_CreatesVisualSeparator()
    {
        // Act
        WatchdogConsole.LogSeparator();
        await Task.Delay(500);

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "*_watchdog.log");
        Assert.NotEmpty(logFiles);

        string logContent = await File.ReadAllTextAsync(logFiles[0]);
        Assert.Contains("========================================", logContent);
    }

    [Fact]
    public async Task MultipleLevels_AllWrittenInOrder()
    {
        // Act
        WatchdogConsole.Debug("First");
        WatchdogConsole.Info("Second");
        WatchdogConsole.Warn("Third");
        WatchdogConsole.Error("Fourth");
        await Task.Delay(500);

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "*_watchdog.log");
        Assert.NotEmpty(logFiles);

        string logContent = await File.ReadAllTextAsync(logFiles[0]);
        
        // Verify order
        int firstIndex = logContent.IndexOf("First");
        int secondIndex = logContent.IndexOf("Second");
        int thirdIndex = logContent.IndexOf("Third");
        int fourthIndex = logContent.IndexOf("Fourth");

        Assert.True(firstIndex < secondIndex);
        Assert.True(secondIndex < thirdIndex);
        Assert.True(thirdIndex < fourthIndex);
    }

    public void Dispose()
    {
        // Cleanup: Shutdown console and delete test directory
        WatchdogConsole.Shutdown();
        
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
