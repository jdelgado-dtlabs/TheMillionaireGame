using MillionaireGame.Core.Models.Telemetry;

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

    private TelemetryService()
    {
        _currentGame = new GameTelemetry();
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

        _currentGame.Rounds.Add(_currentRound);
        _currentRound = null;
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
        
        // Calculate total winnings
        decimal totalWinnings = 0;
        foreach (var round in _currentGame.Rounds)
        {
            var winningsStr = round.FinalWinnings.Replace("$", "").Replace(",", "");
            if (decimal.TryParse(winningsStr, out var amount))
            {
                totalWinnings += amount;
            }
        }
        _currentGame.TotalWinningsAwarded = $"${totalWinnings:N0}";
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
