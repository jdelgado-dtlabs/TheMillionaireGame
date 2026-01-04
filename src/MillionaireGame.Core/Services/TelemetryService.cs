using MillionaireGame.Core.Models.Telemetry;
using MillionaireGame.Core.Settings;

namespace MillionaireGame.Core.Services;

/// <summary>
/// Service for tracking game telemetry and statistics
/// </summary>
public class TelemetryService
{
    private static TelemetryService? _instance;
    public static TelemetryService Instance => _instance ??= new TelemetryService();

    private GameTelemetry _currentGame;
    private RoundTelemetry? _currentRound;
    private MoneyTreeSettings? _moneyTreeSettings;

    private TelemetryService()
    {
        _currentGame = new GameTelemetry();
    }

    /// <summary>
    /// Set the money tree settings for currency tracking
    /// </summary>
    public void SetMoneyTreeSettings(MoneyTreeSettings settings)
    {
        _moneyTreeSettings = settings;
        _currentGame.Currency1Name = settings.Currency;
        _currentGame.Currency2Name = settings.Currency2;
        _currentGame.Currency2Enabled = settings.Currency2Enabled;
    }

    /// <summary>
    /// Start a new game session
    /// </summary>
    public void StartNewGame()
    {
        _currentGame = new GameTelemetry
        {
            GameStartTime = DateTime.Now
        };
        _currentRound = null;
    }

    /// <summary>
    /// Start a new round within the current game
    /// </summary>
    public void StartNewRound(int roundNumber)
    {
        _currentRound = new RoundTelemetry
        {
            RoundNumber = roundNumber,
            StartTime = DateTime.Now
        };

        // Set game start time on first round if not already set
        if (_currentGame.GameStartTime == default)
        {
            _currentGame.GameStartTime = DateTime.Now;
        }
    }

    /// <summary>
    /// Complete the current round with outcome information
    /// </summary>
    public void CompleteRound(string outcome, string finalWinnings, int questionReached)
    {
        if (_currentRound == null) return;

        _currentRound.EndTime = DateTime.Now;
        _currentRound.Outcome = outcome;
        _currentRound.FinalWinnings = finalWinnings;
        _currentRound.FinalQuestionReached = questionReached;

        // Calculate currency breakdown if settings are available
        if (_moneyTreeSettings != null)
        {
            CalculateCurrencyBreakdown(_currentRound, questionReached);
        }

        _currentGame.Rounds.Add(_currentRound);
        _currentRound = null;
    }

    /// <summary>
    /// Calculate how much was won in each currency
    /// Finds the highest question value reached in each currency (not cumulative)
    /// </summary>
    private void CalculateCurrencyBreakdown(RoundTelemetry round, int finalQuestion)
    {
        if (_moneyTreeSettings == null) return;

        int currency1HighestLevel = 0;
        int currency2HighestLevel = 0;

        // Find the highest question reached in each currency
        for (int level = 1; level <= finalQuestion; level++)
        {
            int currencyIndex = _moneyTreeSettings.LevelCurrencies[level - 1];

            if (currencyIndex == 1)
            {
                currency1HighestLevel = level;
            }
            else if (currencyIndex == 2)
            {
                currency2HighestLevel = level;
            }
        }

        // Get the value of the highest question in each currency
        if (currency1HighestLevel > 0)
        {
            round.Currency1Winnings = _moneyTreeSettings.GetLevelValue(currency1HighestLevel);
        }

        if (currency2HighestLevel > 0)
        {
            round.Currency2Winnings = _moneyTreeSettings.GetLevelValue(currency2HighestLevel);
        }
    }

    /// <summary>
    /// Record lifeline usage
    /// </summary>
    public void RecordLifelineUsage(string lifelineName, int questionNumber, string? metadata = null)
    {
        if (_currentRound == null) return;

        _currentRound.LifelinesUsed.Add(new LifelineUsage
        {
            LifelineName = lifelineName,
            QuestionNumber = questionNumber,
            Timestamp = DateTime.Now,
            Metadata = metadata
        });
    }

    /// <summary>
    /// Update web participant counts and device/browser/OS breakdowns
    /// </summary>
    public void UpdateParticipantStats(int totalParticipants, 
        Dictionary<string, int> deviceTypes,
        Dictionary<string, int> browserTypes,
        Dictionary<string, int> osTypes)
    {
        if (_currentRound == null) return;

        _currentRound.TotalParticipants = totalParticipants;
        _currentRound.DeviceTypes = deviceTypes;
        _currentRound.BrowserTypes = browserTypes;
        _currentRound.OSTypes = osTypes;
    }

    /// <summary>
    /// Set FFF performance statistics
    /// </summary>
    public void SetFFFStats(FFFStats stats)
    {
        if (_currentRound == null) return;
        _currentRound.FFFPerformance = stats;
    }

    /// <summary>
    /// Set ATA performance statistics
    /// </summary>
    public void SetATAStats(ATAStats stats)
    {
        if (_currentRound == null) return;
        _currentRound.ATAPerformance = stats;
    }

    /// <summary>
    /// Complete the game session
    /// </summary>
    public void CompleteGame()
    {
        _currentGame.GameEndTime = DateTime.Now;
        
        // Calculate aggregate statistics
        _currentGame.TotalLifelinesUsed = _currentGame.Rounds.Sum(r => r.LifelinesUsed.Count);
        _currentGame.TotalQuestionsAnswered = _currentGame.Rounds.Sum(r => r.FinalQuestionReached);
        
        // Calculate total winnings (legacy combined format)
        decimal totalWinnings = 0;
        foreach (var round in _currentGame.Rounds)
        {
            // Try parsing without currency symbols
            var winningsStr = round.FinalWinnings;
            // Remove common currency symbols and separators
            foreach (var symbol in new[] { "$", "€", "£", "¥", "," })
            {
                winningsStr = winningsStr.Replace(symbol, "");
            }
            if (decimal.TryParse(winningsStr.Trim(), out var amount))
            {
                totalWinnings += amount;
            }
        }
        _currentGame.TotalWinningsAwarded = $"${totalWinnings:N0}";
        
        // Calculate per-currency totals
        _currentGame.Currency1TotalWinnings = _currentGame.Rounds.Sum(r => r.Currency1Winnings);
        _currentGame.Currency2TotalWinnings = _currentGame.Rounds.Sum(r => r.Currency2Winnings);
    }

    /// <summary>
    /// Get the current game telemetry data
    /// </summary>
    public GameTelemetry GetCurrentGameData()
    {
        return _currentGame;
    }

    /// <summary>
    /// Reset telemetry data (for new game)
    /// </summary>
    public void Reset()
    {
        _currentGame = new GameTelemetry();
        _currentRound = null;
    }
}
