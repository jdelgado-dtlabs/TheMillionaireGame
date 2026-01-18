using MillionaireGame.Core.Models.Telemetry;

namespace MillionaireGame.Core.Database;

public interface ITelemetryRepository
{
    Task SaveGameSessionAsync(GameTelemetry gameTelemetry);
    Task UpdateGameSessionEndTimeAsync(string sessionId, DateTime endTime);
    Task<List<GameSessionSummary>> GetAllGameSessionsAsync();
    Task<List<GameSessionSummary>> GetSessionsByDateAsync(DateTime date);
    Task<List<DateTime>> GetSessionDatesAsync();
    Task<List<string>> GetIncompleteGameSessionsAsync();
    Task<GameTelemetry> GetGameSessionWithRoundsAsync(string sessionId);
    Task SaveGameRoundAsync(string sessionId, RoundTelemetry roundTelemetry);
    Task UpdateGameRoundAsync(string sessionId, RoundTelemetry roundTelemetry);
    Task SaveLifelineUsageAsync(string sessionId, int roundId, int lifelineType, int questionNumber, string? metadata = null);
    Task<List<LifelineUsageData>> GetLifelineUsagesForSessionAsync(string sessionId);
    Task<int> GetParticipantCountForSessionAsync(string sessionId);
    Task<Dictionary<string, int>> GetDeviceStatsForSessionAsync(string sessionId);
    Task<Dictionary<string, int>> GetBrowserStatsForSessionAsync(string sessionId);
    Task<FFFStatsData> GetFFFStatsForSessionAsync(string sessionId);
    Task<Dictionary<string, int>> GetATAStatsForSessionAsync(string sessionId);
}
