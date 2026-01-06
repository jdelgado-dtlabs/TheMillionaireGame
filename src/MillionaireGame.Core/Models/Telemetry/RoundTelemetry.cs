namespace MillionaireGame.Core.Models.Telemetry;

/// <summary>
/// Telemetry data for a single game round (one contestant's playthrough)
/// </summary>
public class RoundTelemetry
{
    // Database ID (set when saved)
    public int RoundId { get; set; }
    
    // Round Identity
    public int RoundNumber { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration => EndTime - StartTime;
    
    // Web Participants
    public int TotalParticipants { get; set; }
    public Dictionary<string, int> DeviceTypes { get; set; } = new();
    public Dictionary<string, int> BrowserTypes { get; set; } = new();
    public Dictionary<string, int> OSTypes { get; set; } = new();
    
    // FFF Performance (nullable if not played)
    public FFFStats? FFFPerformance { get; set; }
    
    // ATA Performance (nullable if not used)
    public ATAStats? ATAPerformance { get; set; }
    
    // Player Performance
    public int FinalQuestionReached { get; set; } // 1-15
    public RoundOutcome? Outcome { get; set; }
    public string FinalWinnings { get; set; } = "$0";
    public List<LifelineUsage> LifelinesUsed { get; set; } = new();
    
    // Currency Breakdown
    public int Currency1Winnings { get; set; } = 0;
    public int Currency2Winnings { get; set; } = 0;
}

/// <summary>
/// Round outcome enum (matches database values)
/// </summary>
public enum RoundOutcome
{
    Won = 1,
    Lost = 2,
    WalkedAway = 3,
    Interrupted = 4
}
