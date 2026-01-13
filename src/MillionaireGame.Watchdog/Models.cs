namespace MillionaireGame.Watchdog;

/// <summary>
/// Heartbeat message sent from main application to watchdog
/// </summary>
public class HeartbeatMessage
{
    public DateTime Timestamp { get; set; }
    public string State { get; set; } = "Running";
    public int ThreadCount { get; set; }
    public long MemoryUsageMB { get; set; }
    public string? CurrentActivity { get; set; }
}

/// <summary>
/// Crash information captured by watchdog
/// </summary>
public class CrashInfo
{
    public int ProcessId { get; set; }
    public int ExitCode { get; set; }
    public DateTime CrashTime { get; set; }
    public TimeSpan RunningTime { get; set; }
    public DateTime? LastHeartbeat { get; set; }
    public string? LastState { get; set; }
    public string? LastActivity { get; set; }
    public long LastMemoryMB { get; set; }
    public int LastThreadCount { get; set; }
    public bool WasResponsive { get; set; } = true;
    
    // Additional fields for GitHub crash reporting
    public string? ExitCodeMeaning { get; set; }
    public string? CrashReportPath { get; set; }
    public string? AppVersion { get; set; }
}

/// <summary>
/// User-provided context for crash reporting
/// </summary>
public class UserCrashContext
{
    /// <summary>
    /// User's description of what they were doing when the crash occurred
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Optional email for follow-up (never shared publicly)
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Whether to include system information in the report
    /// </summary>
    public bool IncludeSystemInfo { get; set; } = true;

    /// <summary>
    /// Whether to include sanitized logs in the report
    /// </summary>
    public bool IncludeLogs { get; set; } = true;

    /// <summary>
    /// Steps to reproduce the issue
    /// </summary>
    public string ReproductionSteps { get; set; } = string.Empty;
}

/// <summary>
/// Result of a crash report submission
/// </summary>
public class SubmissionResult
{
    /// <summary>
    /// Whether the submission was successful
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// GitHub issue number if successfully created
    /// </summary>
    public int? IssueNumber { get; set; }

    /// <summary>
    /// URL to the created GitHub issue
    /// </summary>
    public string? IssueUrl { get; set; }

    /// <summary>
    /// Error message if submission failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Whether this crash was detected as a duplicate
    /// </summary>
    public bool IsDuplicate { get; set; }

    /// <summary>
    /// Existing issue number if duplicate
    /// </summary>
    public int? ExistingIssueNumber { get; set; }
}

