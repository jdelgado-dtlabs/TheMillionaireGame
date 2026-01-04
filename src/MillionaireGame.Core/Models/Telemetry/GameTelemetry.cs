namespace MillionaireGame.Core.Models.Telemetry;

/// <summary>
/// Telemetry data for an entire game session (multiple rounds)
/// </summary>
public class GameTelemetry
{
    // Game Identity
    public string SessionId { get; set; } = Guid.NewGuid().ToString();
    public DateTime GameStartTime { get; set; }
    public DateTime GameEndTime { get; set; }
    public TimeSpan TotalDuration => GameEndTime - GameStartTime;
    
    // Rounds
    public List<RoundTelemetry> Rounds { get; set; } = new();
    public int TotalRounds => Rounds.Count;
    
    // Aggregate Stats
    public int TotalUniqueParticipants { get; set; }
    public int TotalQuestionsAnswered { get; set; }
    public int TotalLifelinesUsed { get; set; }
    public string TotalWinningsAwarded { get; set; } = "$0";
    
    // Currency Tracking
    public string Currency1Name { get; set; } = "$";
    public int Currency1TotalWinnings { get; set; } = 0;
    public string Currency2Name { get; set; } = "€";
    public int Currency2TotalWinnings { get; set; } = 0;
    public bool Currency2Enabled { get; set; } = false;
}
