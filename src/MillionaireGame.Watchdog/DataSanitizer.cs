using System.Text;
using System.Text.RegularExpressions;

namespace MillionaireGame.Watchdog;

/// <summary>
/// Sanitizes crash reports by removing personally identifiable information (PII)
/// and sensitive data while preserving debugging value
/// </summary>
public static class DataSanitizer
{
    private static readonly string _userName = Environment.UserName;
    private static readonly string _machineName = Environment.MachineName;
    private static readonly string _userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    
    // Regex patterns for sensitive data detection
    private static readonly Regex _filePathRegex = new(@"[A-Z]:\\(?:Users\\[^\\]+|Documents and Settings\\[^\\]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex _connectionStringRegex = new(@"(Server|Data Source|Initial Catalog|User ID|Password|Uid|Pwd)\s*=\s*[^;]+", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex _apiKeyRegex = new(@"(api[_-]?key|token|secret|password)['""]?\s*[:=]\s*['""]?[\w\-]{20,}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex _emailRegex = new(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b", RegexOptions.Compiled);
    private static readonly Regex _ipAddressRegex = new(@"\b(?:[0-9]{1,3}\.){3}[0-9]{1,3}\b", RegexOptions.Compiled);
    
    /// <summary>
    /// Sanitizes a crash report by removing all PII and sensitive data
    /// </summary>
    /// <param name="content">The original crash report content</param>
    /// <returns>Sanitized content safe for public submission</returns>
    public static string SanitizeCrashReport(string content)
    {
        if (string.IsNullOrEmpty(content))
            return content;
        
        var sanitized = content;
        
        // Replace username
        sanitized = sanitized.Replace(_userName, "<USER>", StringComparison.OrdinalIgnoreCase);
        
        // Replace machine name
        sanitized = sanitized.Replace(_machineName, "<MACHINE>", StringComparison.OrdinalIgnoreCase);
        
        // Replace user profile path
        if (!string.IsNullOrEmpty(_userProfile))
        {
            sanitized = sanitized.Replace(_userProfile, "<USERPROFILE>", StringComparison.OrdinalIgnoreCase);
        }
        
        // Sanitize file paths
        sanitized = _filePathRegex.Replace(sanitized, "<USERPATH>");
        
        // Remove connection strings
        sanitized = _connectionStringRegex.Replace(sanitized, m => 
        {
            var key = m.Groups[1].Value;
            return $"{key}=<REDACTED>";
        });
        
        // Remove API keys and tokens
        sanitized = _apiKeyRegex.Replace(sanitized, m =>
        {
            var key = m.Groups[1].Value;
            return $"{key}=<REDACTED>";
        });
        
        // Sanitize email addresses (but keep domain for context)
        sanitized = _emailRegex.Replace(sanitized, m =>
        {
            var email = m.Value;
            var atIndex = email.IndexOf('@');
            if (atIndex > 0)
            {
                var domain = email.Substring(atIndex);
                return $"<EMAIL>{domain}";
            }
            return "<EMAIL>";
        });
        
        // Sanitize IP addresses (keep first octet for network context)
        sanitized = _ipAddressRegex.Replace(sanitized, m =>
        {
            var ip = m.Value;
            // Preserve localhost IPs
            if (ip.StartsWith("127.") || ip.StartsWith("0."))
                return ip;
            
            var firstOctet = ip.Split('.')[0];
            return $"{firstOctet}.<IP>";
        });
        
        return sanitized;
    }
    
    /// <summary>
    /// Sanitizes environment variables, removing sensitive values
    /// </summary>
    public static Dictionary<string, string> SanitizeEnvironmentVariables(System.Collections.IDictionary environmentVariables)
    {
        var sanitized = new Dictionary<string, string>();
        var sensitiveKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "USERNAME", "USERDOMAIN", "COMPUTERNAME", "USERPROFILE",
            "HOMEDRIVE", "HOMEPATH", "TEMP", "TMP", "APPDATA", "LOCALAPPDATA",
            "PATH", "PATHEXT" // These contain user paths
        };
        
        foreach (var key in environmentVariables.Keys)
        {
            var keyStr = key?.ToString();
            if (string.IsNullOrEmpty(keyStr))
                continue;
                
            var value = environmentVariables[keyStr]?.ToString() ?? string.Empty;
            
            if (sensitiveKeys.Contains(keyStr))
            {
                sanitized[keyStr] = "<REDACTED>";
            }
            else if (keyStr.Contains("KEY", StringComparison.OrdinalIgnoreCase) ||
                     keyStr.Contains("TOKEN", StringComparison.OrdinalIgnoreCase) ||
                     keyStr.Contains("SECRET", StringComparison.OrdinalIgnoreCase) ||
                     keyStr.Contains("PASSWORD", StringComparison.OrdinalIgnoreCase))
            {
                sanitized[keyStr] = "<REDACTED>";
            }
            else
            {
                // Still sanitize the value for paths
                sanitized[keyStr] = SanitizeCrashReport(value);
            }
        }
        
        return sanitized;
    }
    
    /// <summary>
    /// Sanitizes a stack trace, preserving structure but removing PII
    /// </summary>
    public static string SanitizeStackTrace(string stackTrace)
    {
        if (string.IsNullOrEmpty(stackTrace))
            return stackTrace;
        
        var lines = stackTrace.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var sanitizedLines = new StringBuilder();
        
        foreach (var line in lines)
        {
            var sanitizedLine = SanitizeCrashReport(line);
            
            // Preserve "at" keyword and method signatures
            // But sanitize file paths
            sanitizedLines.AppendLine(sanitizedLine);
        }
        
        return sanitizedLines.ToString();
    }
    
    /// <summary>
    /// Gets a sanitized version of system information
    /// </summary>
    public static Dictionary<string, string> GetSanitizedSystemInfo()
    {
        return new Dictionary<string, string>
        {
            ["OS"] = $"{Environment.OSVersion.Platform} {Environment.OSVersion.Version}",
            ["CLR Version"] = Environment.Version.ToString(),
            ["64-bit OS"] = Environment.Is64BitOperatingSystem.ToString(),
            ["64-bit Process"] = Environment.Is64BitProcess.ToString(),
            ["Processor Count"] = Environment.ProcessorCount.ToString(),
            ["Working Set"] = $"{Environment.WorkingSet / 1024 / 1024} MB",
            ["System Directory"] = "<REDACTED>",
            ["Current Directory"] = "<REDACTED>"
        };
    }
}
