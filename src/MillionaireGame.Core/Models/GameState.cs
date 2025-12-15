namespace MillionaireGame.Core.Models;

/// <summary>
/// Represents the current state of the game
/// </summary>
public class GameState
{
    public int CurrentLevel { get; set; } = 0;
    public GameMode Mode { get; set; } = GameMode.Normal;
    public bool FirstQuestion { get; set; } = true;
    public bool IntoCommercials { get; set; } = false;
    public bool WalkAway { get; set; } = false;
    public bool ShowTree { get; set; } = false;
    public bool FreeSafetyNetSet { get; set; } = false;
    public int FreeSafetyNetAt { get; set; } = 0;

    // Money values
    public string CurrentValue { get; set; } = "0";
    public string CorrectValue { get; set; } = "0";
    public string WrongValue { get; set; } = "0";
    public string DropValue { get; set; } = "0";
    public string QuestionsLeft { get; set; } = "0";

    public int TotalLifelines { get; set; } = 0;
}

/// <summary>
/// Game mode enumeration
/// </summary>
public enum GameMode
{
    Normal = 0,
    Risk = 1
}
