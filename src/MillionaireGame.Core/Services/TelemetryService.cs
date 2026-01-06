using MillionaireGame.Core.Models.Telemetry;
using MillionaireGame.Core.Settings;
using MillionaireGame.Core.Database;

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
    private TelemetryRepository? _repository;
    private string? _connectionString;

    private TelemetryService()
    {
        _currentGame = new GameTelemetry();
    }

    /// <summary>
    /// Initialize the telemetry service with database connection
    /// </summary>
    public void Initialize(string connectionString)
    {
        _connectionString = connectionString;
        _repository = new TelemetryRepository(connectionString);
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
            GameStartTime = DateTime.Now,
            Currency1Name = _moneyTreeSettings?.Currency ?? "$",
            Currency2Name = _moneyTreeSettings?.Currency2
        };
        _currentRound = null;

        // Save to database on background thread - don't block UI
        if (_repository != null)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await _repository.SaveGameSessionAsync(_currentGame);
                }
                catch (Exception ex)
                {
                    // Log error but don't fail - telemetry is not critical
                    Console.WriteLine($"Failed to save game session: {ex.Message}");
                }
            });
        }
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
    public void CompleteRound(string outcome, string finalWinnings, int questionReached, int currency1Value = 0, int currency2Value = 0)
    {
        if (_currentRound == null) return;

        _currentRound.EndTime = DateTime.Now;
        
        // Map string outcome to enum
        _currentRound.Outcome = outcome.ToLower() switch
        {
            "win" => RoundOutcome.Won,
            "loss" or "incorrect answer" or "wrong" => RoundOutcome.Lost,
            "walk away" or "drop" => RoundOutcome.WalkedAway,
            "interrupted" => RoundOutcome.Interrupted,
            _ => null
        };
        
        _currentRound.FinalWinnings = finalWinnings;
        _currentRound.FinalQuestionReached = questionReached;
        _currentRound.Currency1Winnings = currency1Value;
        _currentRound.Currency2Winnings = currency2Value;

        _currentGame.Rounds.Add(_currentRound);

        // Save to database on background thread - don't block UI
        if (_repository != null)
        {
            var roundToSave = _currentRound;
            var sessionId = _currentGame.SessionId;
            _ = Task.Run(async () =>
            {
                try
                {
                    await _repository.SaveGameRoundAsync(sessionId, roundToSave);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to save game round: {ex.Message}");
                }
            });
        }

        _currentRound = null;
    }

    /// <summary>
    /// Calculate how much was won in each currency
    /// Finds the highest question value reached in each currency (not cumulative)
    /// </summary>


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

        // Save to database on background thread - don't block UI
        if (_repository != null && _currentRound.RoundId > 0)
        {
            var sessionId = _currentGame.SessionId;
            var roundId = _currentRound.RoundId;
            _ = Task.Run(async () =>
            {
                try
                {
                    // Map lifeline name to type enum
                    int lifelineType = lifelineName.ToLower() switch
                    {
                        "fifty fifty" or "50:50" or "5050" => 1,
                        "plus one" or "phone a friend" => 2,
                        "ask the audience" => 3,
                        "switch question" => 4,
                        "double dip" => 5,
                        "ask the host" => 6,
                        _ => 0
                    };

                    if (lifelineType > 0)
                    {
                        await _repository.SaveLifelineUsageAsync(sessionId, roundId, 
                            lifelineType, questionNumber, metadata);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to save lifeline usage: {ex.Message}");
                }
            });
        }
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

        // Update database with end time on background thread - don't block UI
        if (_repository != null)
        {
            var sessionId = _currentGame.SessionId;
            var endTime = _currentGame.GameEndTime;
            _ = Task.Run(async () =>
            {
                try
                {
                    await _repository.UpdateGameSessionEndTimeAsync(sessionId, endTime);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to update game session end time: {ex.Message}");
                }
            });
        }
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
