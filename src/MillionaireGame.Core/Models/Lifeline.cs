namespace MillionaireGame.Core.Models;

/// <summary>
/// Represents a lifeline in the game
/// </summary>
public class Lifeline
{
    public LifelineType Type { get; set; }
    public bool IsUsed { get; set; } = false;
    public bool IsEnabled { get; set; } = true;
    public bool IsUnlocked { get; set; } = false;
    public LifelineAvailability Availability { get; set; } = LifelineAvailability.Always;
}

/// <summary>
/// Types of lifelines available
/// </summary>
public enum LifelineType
{
    None = 0,
    FiftyFifty = 1,
    PlusOne = 2,        // Phone a Friend
    AskTheAudience = 3,
    SwitchQuestion = 4,
    DoubleDip = 5,
    AskTheHost = 6
}

/// <summary>
/// When a lifeline becomes available
/// </summary>
public enum LifelineAvailability
{
    Always = 0,
    AfterQ5 = 1,
    AfterQ10 = 2,
    RiskMode = 3
}
