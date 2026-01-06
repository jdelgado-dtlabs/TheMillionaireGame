using System.Data;
using Microsoft.Data.SqlClient;
using MillionaireGame.Core.Models.Telemetry;

namespace MillionaireGame.Core.Database;

/// <summary>
/// Repository for telemetry data persistence in SQL Server
/// </summary>
public class TelemetryRepository
{
    private readonly string _connectionString;

    public TelemetryRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    #region GameSessions

    /// <summary>
    /// Save or update a game session
    /// </summary>
    public async Task SaveGameSessionAsync(GameTelemetry gameTelemetry)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            IF EXISTS (SELECT 1 FROM GameSessions WHERE SessionId = @SessionId)
            BEGIN
                UPDATE GameSessions 
                SET GameEndTime = @GameEndTime,
                    Currency1Name = @Currency1Name,
                    Currency2Name = @Currency2Name,
                    UpdatedAt = GETDATE()
                WHERE SessionId = @SessionId
            END
            ELSE
            BEGIN
                INSERT INTO GameSessions (SessionId, GameStartTime, GameEndTime, Currency1Name, Currency2Name)
                VALUES (@SessionId, @GameStartTime, @GameEndTime, @Currency1Name, @Currency2Name)
            END";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@SessionId", gameTelemetry.SessionId);
        command.Parameters.AddWithValue("@GameStartTime", gameTelemetry.GameStartTime);
        command.Parameters.AddWithValue("@GameEndTime", gameTelemetry.GameEndTime == default ? DBNull.Value : gameTelemetry.GameEndTime);
        command.Parameters.AddWithValue("@Currency1Name", string.IsNullOrEmpty(gameTelemetry.Currency1Name) ? DBNull.Value : gameTelemetry.Currency1Name);
        command.Parameters.AddWithValue("@Currency2Name", string.IsNullOrEmpty(gameTelemetry.Currency2Name) ? DBNull.Value : gameTelemetry.Currency2Name);

        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Update game session end time
    /// </summary>
    public async Task UpdateGameSessionEndTimeAsync(string sessionId, DateTime endTime)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            UPDATE GameSessions 
            SET GameEndTime = @EndTime,
                UpdatedAt = GETDATE()
            WHERE SessionId = @SessionId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@SessionId", sessionId);
        command.Parameters.AddWithValue("@EndTime", endTime);

        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Get all game sessions ordered by start time (newest first)
    /// </summary>
    public async Task<List<GameSessionSummary>> GetAllGameSessionsAsync()
    {
        var sessions = new List<GameSessionSummary>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT SessionId, GameStartTime, GameEndTime, Currency1Name, Currency2Name
            FROM GameSessions
            ORDER BY GameStartTime DESC";

        using var command = new SqlCommand(sql, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            sessions.Add(new GameSessionSummary
            {
                SessionId = reader.GetString(0),
                GameStartTime = reader.GetDateTime(1),
                GameEndTime = reader.IsDBNull(2) ? null : reader.GetDateTime(2),
                Currency1Name = reader.IsDBNull(3) ? null : reader.GetString(3),
                Currency2Name = reader.IsDBNull(4) ? null : reader.GetString(4)
            });
        }

        return sessions;
    }

    /// <summary>
    /// Get sessions for a specific date
    /// </summary>
    public async Task<List<GameSessionSummary>> GetSessionsByDateAsync(DateTime date)
    {
        var sessions = new List<GameSessionSummary>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT SessionId, GameStartTime, GameEndTime, Currency1Name, Currency2Name
            FROM GameSessions
            WHERE CAST(GameStartTime AS DATE) = @Date
            ORDER BY GameStartTime DESC";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Date", date.Date);

        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            sessions.Add(new GameSessionSummary
            {
                SessionId = reader.GetString(0),
                GameStartTime = reader.GetDateTime(1),
                GameEndTime = reader.IsDBNull(2) ? null : reader.GetDateTime(2),
                Currency1Name = reader.IsDBNull(3) ? null : reader.GetString(3),
                Currency2Name = reader.IsDBNull(4) ? null : reader.GetString(4)
            });
        }

        return sessions;
    }

    /// <summary>
    /// Get list of dates with telemetry data (for calendar highlighting)
    /// </summary>
    public async Task<List<DateTime>> GetSessionDatesAsync()
    {
        var dates = new List<DateTime>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT DISTINCT CAST(GameStartTime AS DATE) AS SessionDate
            FROM GameSessions
            ORDER BY SessionDate DESC";

        using var command = new SqlCommand(sql, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            dates.Add(reader.GetDateTime(0));
        }

        return dates;
    }

    /// <summary>
    /// Get incomplete game sessions (no end time)
    /// </summary>
    public async Task<List<string>> GetIncompleteGameSessionsAsync()
    {
        var sessionIds = new List<string>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT SessionId
            FROM GameSessions
            WHERE GameEndTime IS NULL";

        using var command = new SqlCommand(sql, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            sessionIds.Add(reader.GetString(0));
        }

        return sessionIds;
    }

    /// <summary>
    /// Get full game session with all rounds and aggregated stats
    /// </summary>
    public async Task<GameTelemetry> GetGameSessionWithRoundsAsync(string sessionId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        // Get session
        const string sessionSql = @"
            SELECT SessionId, GameStartTime, GameEndTime, Currency1Name, Currency2Name
            FROM GameSessions
            WHERE SessionId = @SessionId";

        GameTelemetry gameTelemetry;

        using (var command = new SqlCommand(sessionSql, connection))
        {
            command.Parameters.AddWithValue("@SessionId", sessionId);
            using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                throw new InvalidOperationException($"Session {sessionId} not found");

            gameTelemetry = new GameTelemetry
            {
                SessionId = reader.GetString(0),
                GameStartTime = reader.GetDateTime(1),
                GameEndTime = reader.IsDBNull(2) ? default : reader.GetDateTime(2),
                Currency1Name = reader.IsDBNull(3) ? "$" : reader.GetString(3),
                Currency2Name = reader.IsDBNull(4) ? null : reader.GetString(4)
            };
        }

        // Get rounds
        const string roundsSql = @"
            SELECT RoundId, RoundNumber, StartTime, EndTime, Outcome, FinalQuestionReached, 
                   Currency1Winnings, Currency2Winnings
            FROM GameRounds
            WHERE SessionId = @SessionId
            ORDER BY RoundNumber";

        using (var command = new SqlCommand(roundsSql, connection))
        {
            command.Parameters.AddWithValue("@SessionId", sessionId);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                gameTelemetry.Rounds.Add(new RoundTelemetry
                {
                    RoundNumber = reader.GetInt32(1),
                    StartTime = reader.GetDateTime(2),
                    EndTime = reader.IsDBNull(3) ? default : reader.GetDateTime(3),
                    Outcome = reader.IsDBNull(4) ? null : (RoundOutcome)reader.GetInt32(4),
                    FinalQuestionReached = reader.GetInt32(5),
                    Currency1Winnings = reader.GetInt32(6),
                    Currency2Winnings = reader.GetInt32(7)
                });
            }
        }

        // Get aggregated stats
        gameTelemetry.TotalUniqueParticipants = await GetParticipantCountForSessionAsync(connection, sessionId);

        return gameTelemetry;
    }

    #endregion

    #region GameRounds

    /// <summary>
    /// Save a game round
    /// </summary>
    public async Task SaveGameRoundAsync(string sessionId, RoundTelemetry roundTelemetry)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            INSERT INTO GameRounds (SessionId, RoundNumber, StartTime, EndTime, Outcome, 
                                    FinalQuestionReached, Currency1Winnings, Currency2Winnings)
            VALUES (@SessionId, @RoundNumber, @StartTime, @EndTime, @Outcome, 
                    @FinalQuestionReached, @Currency1Winnings, @Currency2Winnings);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@SessionId", sessionId);
        command.Parameters.AddWithValue("@RoundNumber", roundTelemetry.RoundNumber);
        command.Parameters.AddWithValue("@StartTime", roundTelemetry.StartTime);
        command.Parameters.AddWithValue("@EndTime", roundTelemetry.EndTime == default ? DBNull.Value : roundTelemetry.EndTime);
        command.Parameters.AddWithValue("@Outcome", roundTelemetry.Outcome.HasValue ? (int)roundTelemetry.Outcome.Value : DBNull.Value);
        command.Parameters.AddWithValue("@FinalQuestionReached", roundTelemetry.FinalQuestionReached);
        command.Parameters.AddWithValue("@Currency1Winnings", roundTelemetry.Currency1Winnings);
        command.Parameters.AddWithValue("@Currency2Winnings", roundTelemetry.Currency2Winnings);

        var roundId = (int)await command.ExecuteScalarAsync();
        roundTelemetry.RoundId = roundId;
    }

    #endregion

    #region LifelineUsages

    /// <summary>
    /// Save a lifeline usage
    /// </summary>
    public async Task SaveLifelineUsageAsync(string sessionId, int roundId, int lifelineType, int questionNumber, string? metadata = null)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            INSERT INTO LifelineUsages (GameSessionId, RoundId, LifelineType, QuestionNumber, Metadata)
            VALUES (@GameSessionId, @RoundId, @LifelineType, @QuestionNumber, @Metadata)";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@GameSessionId", sessionId);
        command.Parameters.AddWithValue("@RoundId", roundId);
        command.Parameters.AddWithValue("@LifelineType", lifelineType);
        command.Parameters.AddWithValue("@QuestionNumber", questionNumber);
        command.Parameters.AddWithValue("@Metadata", metadata ?? (object)DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Get lifeline usages for a session
    /// </summary>
    public async Task<List<LifelineUsageData>> GetLifelineUsagesForSessionAsync(string sessionId)
    {
        var usages = new List<LifelineUsageData>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT RoundId, LifelineType, QuestionNumber, Metadata
            FROM LifelineUsages
            WHERE GameSessionId = @SessionId
            ORDER BY RoundId, QuestionNumber";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@SessionId", sessionId);

        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            usages.Add(new LifelineUsageData
            {
                RoundId = reader.GetInt32(0),
                LifelineType = reader.GetInt32(1),
                QuestionNumber = reader.GetInt32(2),
                Metadata = reader.IsDBNull(3) ? null : reader.GetString(3)
            });
        }

        return usages;
    }

    #endregion

    #region Aggregate Stats

    private async Task<int> GetParticipantCountForSessionAsync(SqlConnection connection, string sessionId)
    {
        const string sql = @"
            SELECT COUNT(DISTINCT Id)
            FROM Participants
            WHERE GameSessionId = @SessionId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@SessionId", sessionId);

        var result = await command.ExecuteScalarAsync();
        return result == DBNull.Value ? 0 : Convert.ToInt32(result);
    }

    /// <summary>
    /// Get participant count for a session
    /// </summary>
    public async Task<int> GetParticipantCountForSessionAsync(string sessionId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        return await GetParticipantCountForSessionAsync(connection, sessionId);
    }

    /// <summary>
    /// Get device statistics for a session
    /// </summary>
    public async Task<Dictionary<string, int>> GetDeviceStatsForSessionAsync(string sessionId)
    {
        var stats = new Dictionary<string, int>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT DeviceType, COUNT(*) as Count
            FROM Participants
            WHERE GameSessionId = @SessionId AND DeviceType IS NOT NULL
            GROUP BY DeviceType";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@SessionId", sessionId);

        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            stats[reader.GetString(0)] = reader.GetInt32(1);
        }

        return stats;
    }

    /// <summary>
    /// Get browser statistics for a session
    /// </summary>
    public async Task<Dictionary<string, int>> GetBrowserStatsForSessionAsync(string sessionId)
    {
        var stats = new Dictionary<string, int>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT BrowserType, COUNT(*) as Count
            FROM Participants
            WHERE GameSessionId = @SessionId AND BrowserType IS NOT NULL
            GROUP BY BrowserType";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@SessionId", sessionId);

        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            stats[reader.GetString(0)] = reader.GetInt32(1);
        }

        return stats;
    }

    /// <summary>
    /// Get FFF statistics for a session
    /// </summary>
    public async Task<FFFStatsData> GetFFFStatsForSessionAsync(string sessionId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT 
                COUNT(*) as TotalSubmissions,
                SUM(CASE WHEN IsCorrect = 1 THEN 1 ELSE 0 END) as CorrectSubmissions,
                AVG(CAST(TimeElapsed as FLOAT)) as AverageTime
            FROM FFFAnswers
            WHERE GameSessionId = @SessionId";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@SessionId", sessionId);

        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new FFFStatsData
            {
                TotalSubmissions = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                CorrectSubmissions = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                AverageTime = reader.IsDBNull(2) ? 0 : reader.GetDouble(2)
            };
        }

        return new FFFStatsData();
    }

    /// <summary>
    /// Get ATA vote statistics for a session
    /// </summary>
    public async Task<Dictionary<string, int>> GetATAStatsForSessionAsync(string sessionId)
    {
        var stats = new Dictionary<string, int>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            SELECT SelectedOption, COUNT(*) as Count
            FROM ATAVotes
            WHERE GameSessionId = @SessionId
            GROUP BY SelectedOption";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@SessionId", sessionId);

        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            stats[reader.GetString(0)] = reader.GetInt32(1);
        }

        return stats;
    }

    #endregion
}

#region Helper Classes

/// <summary>
/// Summary information for a game session
/// </summary>
public class GameSessionSummary
{
    public string SessionId { get; set; } = "";
    public DateTime GameStartTime { get; set; }
    public DateTime? GameEndTime { get; set; }
    public string? Currency1Name { get; set; }
    public string? Currency2Name { get; set; }
    
    public string SessionIdShort => SessionId.Length > 6 ? SessionId.Substring(SessionId.Length - 6) : SessionId;
    public bool IsComplete => GameEndTime.HasValue;
}

/// <summary>
/// Lifeline usage data
/// </summary>
public class LifelineUsageData
{
    public int RoundId { get; set; }
    public int LifelineType { get; set; }
    public int QuestionNumber { get; set; }
    public string? Metadata { get; set; }
}

/// <summary>
/// FFF statistics data
/// </summary>
public class FFFStatsData
{
    public int TotalSubmissions { get; set; }
    public int CorrectSubmissions { get; set; }
    public double AverageTime { get; set; }
}

#endregion
