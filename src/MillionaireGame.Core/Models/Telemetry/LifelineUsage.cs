namespace MillionaireGame.Core.Models.Telemetry;

/// <summary>
/// Represents a single lifeline usage event during a round
/// </summary>
public class LifelineUsage
{
    /// <summary>
    /// Name of the lifeline used (e.g., "50:50", "Phone-a-Friend", "Ask the Audience")
    /// </summary>
    public string LifelineName { get; set; } = string.Empty;

    /// <summary>
    /// Question number where lifeline was used (1-15)
    /// </summary>
    public int QuestionNumber { get; set; }

    /// <summary>
    /// Timestamp when lifeline was activated
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Additional metadata (e.g., "Online" or "Offline" for ATA)
    /// </summary>
    public string? Metadata { get; set; }
}
