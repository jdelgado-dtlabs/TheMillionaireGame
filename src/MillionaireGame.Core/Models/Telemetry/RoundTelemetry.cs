namespace MillionaireGame.Core.Models.Telemetry;

/// <summary>
/// Telemetry data for a single game round (one contestant's playthrough)
/// </summary>
public class RoundTelemetry
{
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
    public string Outcome { get; set; } = string.Empty; // "Win", "Walk Away", "Loss"
    public string FinalWinnings { get; set; } = "$0";
    public List<LifelineUsage> LifelinesUsed { get; set; } = new();
}
