using Xunit;

namespace MillionaireGame.Watchdog.Tests;

/// <summary>
/// Unit tests for DataSanitizer to verify PII removal
/// </summary>
public class DataSanitizerTests
{
    [Fact]
    public void SanitizeCrashReport_RemovesUsername()
    {
        // Arrange
        var username = Environment.UserName;
        var content = $"Error in C:\\Users\\{username}\\Documents\\test.txt";
        
        // Act
        var sanitized = DataSanitizer.SanitizeCrashReport(content);
        
        // Assert
        Assert.DoesNotContain(username, sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("<USERPATH>", sanitized);
    }
    
    [Fact]
    public void SanitizeCrashReport_RemovesMachineName()
    {
        // Arrange
        var machineName = Environment.MachineName;
        var content = $"Connection to {machineName} failed";
        
        // Act
        var sanitized = DataSanitizer.SanitizeCrashReport(content);
        
        // Assert
        Assert.DoesNotContain(machineName, sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("<MACHINE>", sanitized);
    }
    
    [Fact]
    public void SanitizeCrashReport_RemovesUserProfilePath()
    {
        // Arrange
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var content = $"Log file: {userProfile}\\AppData\\test.log";
        
        // Act
        var sanitized = DataSanitizer.SanitizeCrashReport(content);
        
        // Assert
        Assert.DoesNotContain(userProfile, sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("<USERPATH>", sanitized);
    }
    
    [Fact]
    public void SanitizeCrashReport_RemovesFilePaths()
    {
        // Arrange
        var content = "Error at C:\\Users\\SomeUser\\Documents\\file.txt";
        
        // Act
        var sanitized = DataSanitizer.SanitizeCrashReport(content);
        
        // Assert
        Assert.DoesNotContain("SomeUser", sanitized);
        Assert.Contains("<USERPATH>", sanitized);
    }
    
    [Fact]
    public void SanitizeCrashReport_RemovesConnectionStrings()
    {
        // Arrange
        var content = "Connection: Server=localhost;User ID=admin;Password=secret123;";
        
        // Act
        var sanitized = DataSanitizer.SanitizeCrashReport(content);
        
        // Assert
        Assert.DoesNotContain("admin", sanitized);
        Assert.DoesNotContain("secret123", sanitized);
        Assert.Contains("Server=<REDACTED>", sanitized);
        Assert.Contains("User ID=<REDACTED>", sanitized);
        Assert.Contains("Password=<REDACTED>", sanitized);
    }
    
    [Fact]
    public void SanitizeCrashReport_RemovesApiKeys()
    {
        // Arrange
        var content = "api_key=sk-1234567890abcdefghijklmnopqrstuvwxyz";
        
        // Act
        var sanitized = DataSanitizer.SanitizeCrashReport(content);
        
        // Assert
        Assert.DoesNotContain("sk-1234567890abcdefghijklmnopqrstuvwxyz", sanitized);
        Assert.Contains("api_key=<REDACTED>", sanitized);
    }
    
    [Fact]
    public void SanitizeCrashReport_SanitizesEmails()
    {
        // Arrange
        var content = "Contact: user@example.com for support";
        
        // Act
        var sanitized = DataSanitizer.SanitizeCrashReport(content);
        
        // Assert
        Assert.DoesNotContain("user@example.com", sanitized);
        Assert.Contains("@example.com", sanitized); // Domain preserved
        Assert.Contains("<EMAIL>", sanitized);
    }
    
    [Fact]
    public void SanitizeCrashReport_SanitizesIpAddresses()
    {
        // Arrange
        var content = "Server IP: 192.168.1.100";
        
        // Act
        var sanitized = DataSanitizer.SanitizeCrashReport(content);
        
        // Assert
        Assert.DoesNotContain("192.168.1.100", sanitized);
        Assert.Contains("192.<IP>", sanitized); // First octet preserved
    }
    
    [Fact]
    public void SanitizeCrashReport_PreservesLocalhostIp()
    {
        // Arrange
        var content = "Localhost: 127.0.0.1";
        
        // Act
        var sanitized = DataSanitizer.SanitizeCrashReport(content);
        
        // Assert
        Assert.Contains("127.0.0.1", sanitized); // Localhost preserved
    }
    
    [Fact]
    public void SanitizeCrashReport_HandlesNullOrEmpty()
    {
        // Act & Assert
        Assert.Null(DataSanitizer.SanitizeCrashReport(null!));
        Assert.Equal(string.Empty, DataSanitizer.SanitizeCrashReport(string.Empty));
    }
    
    [Fact]
    public void SanitizeEnvironmentVariables_RedactsSensitiveKeys()
    {
        // Arrange
        var env = new System.Collections.Hashtable
        {
            ["USERNAME"] = "JohnDoe",
            ["COMPUTERNAME"] = "MYPC",
            ["PATH"] = "C:\\Windows\\System32",
            ["API_KEY"] = "secret123",
            ["PROCESSOR_ARCHITECTURE"] = "AMD64"
        };
        
        // Act
        var sanitized = DataSanitizer.SanitizeEnvironmentVariables(env);
        
        // Assert
        Assert.Equal("<REDACTED>", sanitized["USERNAME"]);
        Assert.Equal("<REDACTED>", sanitized["COMPUTERNAME"]);
        Assert.Equal("<REDACTED>", sanitized["PATH"]);
        Assert.Equal("<REDACTED>", sanitized["API_KEY"]);
        Assert.Equal("AMD64", sanitized["PROCESSOR_ARCHITECTURE"]); // Non-sensitive preserved
    }
    
    [Fact]
    public void SanitizeStackTrace_PreservesStructure()
    {
        // Arrange
        var username = Environment.UserName;
        var stackTrace = $@"   at MyApp.Program.Main() in C:\Users\{username}\Source\MyApp\Program.cs:line 42
   at System.AppDomain.ExecuteAssembly(String file)";
        
        // Act
        var sanitized = DataSanitizer.SanitizeStackTrace(stackTrace);
        
        // Assert
        Assert.DoesNotContain(username, sanitized);
        Assert.Contains("at MyApp.Program.Main()", sanitized);
        Assert.Contains("at System.AppDomain.ExecuteAssembly", sanitized);
        Assert.Contains("<USERPATH>", sanitized);
    }
    
    [Fact]
    public void GetSanitizedSystemInfo_NoPersonalInfo()
    {
        // Act
        var systemInfo = DataSanitizer.GetSanitizedSystemInfo();
        
        // Assert
        Assert.Contains("OS", systemInfo.Keys);
        Assert.Contains("Processor Count", systemInfo.Keys);
        Assert.Equal("<REDACTED>", systemInfo["System Directory"]);
        Assert.Equal("<REDACTED>", systemInfo["Current Directory"]);
        
        // Verify no username or machine name leaked
        var username = Environment.UserName;
        var machineName = Environment.MachineName;
        
        foreach (var value in systemInfo.Values)
        {
            Assert.DoesNotContain(username, value, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(machineName, value, StringComparison.OrdinalIgnoreCase);
        }
    }
}
