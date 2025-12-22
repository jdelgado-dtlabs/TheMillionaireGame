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
    public bool GameWin { get; set; } = false; // True when Q15 is answered correctly

    // Money values
    public string CurrentValue { get; set; } = "0";
    public string CorrectValue { get; set; } = "0";
    public string WrongValue { get; set; } = "0";
    public string DropValue { get; set; } = "0";
    public string QuestionsLeft { get; set; } = "0";

    public int TotalLifelines { get; set; } = 0;
    
    // Lifeline management
    private readonly Dictionary<LifelineType, Lifeline> _lifelines = new();

    public GameState()
    {
        // Initialize default lifelines
        _lifelines[LifelineType.FiftyFifty] = new Lifeline { Type = LifelineType.FiftyFifty };
        _lifelines[LifelineType.PlusOne] = new Lifeline { Type = LifelineType.PlusOne };
        _lifelines[LifelineType.AskTheAudience] = new Lifeline { Type = LifelineType.AskTheAudience };
        _lifelines[LifelineType.SwitchQuestion] = new Lifeline { Type = LifelineType.SwitchQuestion };
    }

    public Lifeline? GetLifeline(LifelineType type)
    {
        return _lifelines.TryGetValue(type, out var lifeline) ? lifeline : null;
    }

    public IReadOnlyDictionary<LifelineType, Lifeline> GetAllLifelines()
    {
        return _lifelines;
    }

    public void ResetLifelines()
    {
        foreach (var lifeline in _lifelines.Values)
        {
            lifeline.IsUsed = false;
        }
    }
}

/// <summary>
/// Game mode enumeration
/// </summary>
public enum GameMode
{
    Normal = 0,
    Risk = 1
}
