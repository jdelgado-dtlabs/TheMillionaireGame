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
}
