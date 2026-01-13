using Xunit;

namespace MillionaireGame.Watchdog.Tests;

/// <summary>
/// Unit tests for ProcessMonitor exit code interpretation logic.
/// </summary>
public class ProcessMonitorTests
{
    [Theory]
    [InlineData(0, "Successful completion")]
    [InlineData(1, "General error")]
    [InlineData(-1, "General failure")]
    [InlineData(unchecked((int)0xC0000005), "Access violation (native crash)")]
    [InlineData(unchecked((int)0xE0434352), "CLR exception")]
    [InlineData(unchecked((int)0xC0000374), "Heap corruption")]
    [InlineData(unchecked((int)0xC000013A), "Application terminated by Ctrl+C")]
    [InlineData(255, "Unknown error (255)")]
    [InlineData(42, "Application-specific exit code: 42")]
    public void GetExitCodeMeaning_ReturnsExpectedDescription(int exitCode, string expectedMeaning)
    {
        // Act
        string actualMeaning = ProcessMonitor.GetExitCodeMeaning(exitCode);

        // Assert
        Assert.Equal(expectedMeaning, actualMeaning);
    }

    [Fact]
    public void GetExitCodeMeaning_IdentifiesClrException()
    {
        // Arrange
        int clrExceptionCode = unchecked((int)0xE0434352);

        // Act
        string meaning = ProcessMonitor.GetExitCodeMeaning(clrExceptionCode);

        // Assert
        Assert.Contains("CLR exception", meaning);
    }

    [Fact]
    public void GetExitCodeMeaning_IdentifiesAccessViolation()
    {
        // Arrange
        int accessViolationCode = unchecked((int)0xC0000005);

        // Act
        string meaning = ProcessMonitor.GetExitCodeMeaning(accessViolationCode);

        // Assert
        Assert.Contains("Access violation", meaning);
    }

    [Fact]
    public void GetExitCodeMeaning_IdentifiesHeapCorruption()
    {
        // Arrange
        int heapCorruptionCode = unchecked((int)0xC0000374);

        // Act
        string meaning = ProcessMonitor.GetExitCodeMeaning(heapCorruptionCode);

        // Assert
        Assert.Contains("Heap corruption", meaning);
    }
}
