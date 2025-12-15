using MillionaireGame.Core.Models;

namespace MillionaireGame.Core.Game;

/// <summary>
/// Main game service that manages game state and logic
/// </summary>
public class GameService
{
    private readonly GameState _gameState;
    private readonly List<Lifeline> _lifelines;

    public GameState State => _gameState;
    public IReadOnlyList<Lifeline> Lifelines => _lifelines.AsReadOnly();

    public event EventHandler<GameLevelChangedEventArgs>? LevelChanged;
    public event EventHandler<GameModeChangedEventArgs>? ModeChanged;
    public event EventHandler<LifelineUsedEventArgs>? LifelineUsed;

    public GameService()
    {
        _gameState = new GameState();
        _lifelines = new List<Lifeline>();
        InitializeLifelines();
    }

    private void InitializeLifelines()
    {
        // Initialize with default 4 lifelines
        _lifelines.Clear();
        _lifelines.Add(new Lifeline { Type = LifelineType.FiftyFifty, Availability = LifelineAvailability.Always });
        _lifelines.Add(new Lifeline { Type = LifelineType.PlusOne, Availability = LifelineAvailability.Always });
        _lifelines.Add(new Lifeline { Type = LifelineType.AskTheAudience, Availability = LifelineAvailability.Always });
        _lifelines.Add(new Lifeline { Type = LifelineType.SwitchQuestion, Availability = LifelineAvailability.Always });
    }

    /// <summary>
    /// Changes the current game level
    /// </summary>
    public void ChangeLevel(int newLevel)
    {
        if (newLevel < 0 || newLevel > 15)
            throw new ArgumentOutOfRangeException(nameof(newLevel), "Level must be between 0 and 15");

        var oldLevel = _gameState.CurrentLevel;
        _gameState.CurrentLevel = newLevel;
        UpdateMoneyValues();

        LevelChanged?.Invoke(this, new GameLevelChangedEventArgs(oldLevel, newLevel));

        // Unlock lifelines based on level
        UnlockLifelinesForLevel(newLevel);
    }

    /// <summary>
    /// Changes the game mode
    /// </summary>
    public void ChangeMode(GameMode newMode)
    {
        var oldMode = _gameState.Mode;
        _gameState.Mode = newMode;

        ModeChanged?.Invoke(this, new GameModeChangedEventArgs(oldMode, newMode));

        // Re-evaluate lifeline unlocks
        UnlockLifelinesForLevel(_gameState.CurrentLevel);
    }

    /// <summary>
    /// Uses a lifeline
    /// </summary>
    public void UseLifeline(LifelineType lifelineType)
    {
        var lifeline = _lifelines.FirstOrDefault(l => l.Type == lifelineType);
        if (lifeline == null)
            throw new InvalidOperationException($"Lifeline {lifelineType} not found");

        if (lifeline.IsUsed)
            throw new InvalidOperationException($"Lifeline {lifelineType} has already been used");

        if (!lifeline.IsUnlocked)
            throw new InvalidOperationException($"Lifeline {lifelineType} is not yet unlocked");

        if (!lifeline.IsEnabled)
            throw new InvalidOperationException($"Lifeline {lifelineType} is disabled");

        lifeline.IsUsed = true;
        LifelineUsed?.Invoke(this, new LifelineUsedEventArgs(lifelineType));
    }

    /// <summary>
    /// Resets the game to initial state
    /// </summary>
    public void ResetGame()
    {
        _gameState.CurrentLevel = 0;
        _gameState.Mode = GameMode.Normal;
        _gameState.FirstQuestion = true;
        _gameState.IntoCommercials = false;
        _gameState.WalkAway = false;
        _gameState.ShowTree = false;
        _gameState.FreeSafetyNetSet = false;
        _gameState.FreeSafetyNetAt = 0;

        // Reset all lifelines
        foreach (var lifeline in _lifelines)
        {
            lifeline.IsUsed = false;
            lifeline.IsUnlocked = false;
        }

        UpdateMoneyValues();
        UnlockLifelinesForLevel(0);
    }

    /// <summary>
    /// Updates money values based on current level
    /// </summary>
    private void UpdateMoneyValues()
    {
        var level = _gameState.CurrentLevel;
        var isRiskMode = _gameState.Mode == GameMode.Risk;

        // These values would come from MoneyTreeSettings in the full implementation
        _gameState.CurrentValue = GetMoneyValue(level);
        _gameState.CorrectValue = GetMoneyValue(level + 1);
        _gameState.WrongValue = GetWrongValue(level, isRiskMode);
        _gameState.DropValue = GetDropValue(level, isRiskMode);
        _gameState.QuestionsLeft = (15 - level).ToString();
    }

    /// <summary>
    /// Unlocks lifelines based on the current level and mode
    /// </summary>
    private void UnlockLifelinesForLevel(int level)
    {
        foreach (var lifeline in _lifelines)
        {
            lifeline.IsUnlocked = lifeline.Availability switch
            {
                LifelineAvailability.Always => true,
                LifelineAvailability.AfterQ5 => level >= 5,
                LifelineAvailability.AfterQ10 => level >= 10,
                LifelineAvailability.RiskMode => _gameState.Mode == GameMode.Risk,
                _ => false
            };
        }
    }

    // Helper methods for money values (simplified version)
    private string GetMoneyValue(int level) => level switch
    {
        0 => "$0",
        1 => "$100",
        2 => "$200",
        3 => "$300",
        4 => "$500",
        5 => "$1,000",
        6 => "$2,000",
        7 => "$4,000",
        8 => "$8,000",
        9 => "$16,000",
        10 => "$32,000",
        11 => "$64,000",
        12 => "$125,000",
        13 => "$250,000",
        14 => "$500,000",
        15 => "$1,000,000",
        _ => "$0"
    };

    private string GetWrongValue(int level, bool riskMode)
    {
        if (riskMode)
        {
            return level > 10 ? "$32,000" : "$0";
        }
        return level > 10 ? "$32,000" : level > 5 ? "$1,000" : "$0";
    }

    private string GetDropValue(int level, bool riskMode)
    {
        if (riskMode)
        {
            return level > 10 ? "$32,000" : "$0";
        }
        return level > 10 ? "$32,000" : level > 5 ? "$1,000" : "$0";
    }
}

// Event argument classes
public class GameLevelChangedEventArgs : EventArgs
{
    public int OldLevel { get; }
    public int NewLevel { get; }

    public GameLevelChangedEventArgs(int oldLevel, int newLevel)
    {
        OldLevel = oldLevel;
        NewLevel = newLevel;
    }
}

public class GameModeChangedEventArgs : EventArgs
{
    public GameMode OldMode { get; }
    public GameMode NewMode { get; }

    public GameModeChangedEventArgs(GameMode oldMode, GameMode newMode)
    {
        OldMode = oldMode;
        NewMode = newMode;
    }
}

public class LifelineUsedEventArgs : EventArgs
{
    public LifelineType LifelineType { get; }

    public LifelineUsedEventArgs(LifelineType lifelineType)
    {
        LifelineType = lifelineType;
    }
}
