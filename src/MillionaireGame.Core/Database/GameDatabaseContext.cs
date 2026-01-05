using Microsoft.Data.SqlClient;
using MillionaireGame.Core.Models;

namespace MillionaireGame.Core.Database;

/// <summary>
/// Database context for accessing the Millionaire Game database
/// </summary>
public class GameDatabaseContext : IDisposable
{
    private readonly string _connectionStringWithoutDb;
    private readonly string _connectionStringWithDb;
    private SqlConnection? _connection;
    private const string DatabaseName = "dbMillionaire";

    public GameDatabaseContext(string connectionStringWithoutDb)
    {
        _connectionStringWithoutDb = connectionStringWithoutDb;
        _connectionStringWithDb = connectionStringWithoutDb + $";Database={DatabaseName}";
    }

    /// <summary>
    /// Opens a connection to the database
    /// </summary>
    public async Task OpenConnectionAsync()
    {
        if (_connection?.State == System.Data.ConnectionState.Open)
            return;

        _connection = new SqlConnection(_connectionStringWithDb);
        await _connection.OpenAsync();
    }

    /// <summary>
    /// Closes the current database connection
    /// </summary>
    public void CloseConnection()
    {
        if (_connection?.State == System.Data.ConnectionState.Open)
        {
            _connection.Close();
        }
    }

    /// <summary>
    /// Checks if the database exists
    /// </summary>
    public async Task<bool> DatabaseExistsAsync()
    {
        try
        {
            using var connection = new SqlConnection(_connectionStringWithoutDb);
            await connection.OpenAsync();

            var query = $"SELECT COUNT(*) FROM sys.databases WHERE name = '{DatabaseName}'";
            using var command = new SqlCommand(query, connection);
#pragma warning disable CS8605 // Unboxing a possibly null value - SQL COUNT always returns int
            var result = (int)await command.ExecuteScalarAsync();
#pragma warning restore CS8605

            return result > 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Creates the Millionaire database and all required tables
    /// </summary>
    public async Task CreateDatabaseAsync()
    {
        using var connection = new SqlConnection(_connectionStringWithoutDb);
        await connection.OpenAsync();

        // Create database
        var createDbQuery = $@"
            IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '{DatabaseName}')
            BEGIN
                CREATE DATABASE {DatabaseName}
            END";

        using (var command = new SqlCommand(createDbQuery, connection))
        {
            await command.ExecuteNonQueryAsync();
        }

        // Switch to the new database
        var useDbConnection = new SqlConnection(_connectionStringWithDb);
        await useDbConnection.OpenAsync();

        // Create questions table
        var createQuestionsTable = @"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'questions')
            BEGIN
                CREATE TABLE questions (
                    Id INT PRIMARY KEY IDENTITY(1,1),
                    Question NVARCHAR(MAX) NOT NULL,
                    A NVARCHAR(500) NOT NULL,
                    B NVARCHAR(500) NOT NULL,
                    C NVARCHAR(500) NOT NULL,
                    D NVARCHAR(500) NOT NULL,
                    CorrectAnswer NVARCHAR(1) NOT NULL,
                    Level INT NOT NULL,
                    Note NVARCHAR(MAX),
                    Used BIT DEFAULT 0,
                    Explanation NVARCHAR(MAX),
                    ATA_A INT,
                    ATA_B INT,
                    ATA_C INT,
                    ATA_D INT
                )
            END";

        using (var command = new SqlCommand(createQuestionsTable, useDbConnection))
        {
            await command.ExecuteNonQueryAsync();
        }

        // Create FFF questions table
        var createFFFTable = @"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'fff_questions')
            BEGIN
                CREATE TABLE fff_questions (
                    Id INT PRIMARY KEY IDENTITY(1,1),
                    Question NVARCHAR(MAX) NOT NULL,
                    A NVARCHAR(500) NOT NULL,
                    B NVARCHAR(500) NOT NULL,
                    C NVARCHAR(500) NOT NULL,
                    D NVARCHAR(500) NOT NULL,
                    CorrectAnswer NVARCHAR(10) NOT NULL,
                    Level INT DEFAULT 0,
                    Note NVARCHAR(MAX),
                    Used BIT DEFAULT 0
                )
            END";

        using (var command = new SqlCommand(createFFFTable, useDbConnection))
        {
            await command.ExecuteNonQueryAsync();
        }

        // Create WAPS tables (Web-Based Audience Participation System)
        var createWAPSTables = @"
            -- Sessions table
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Sessions')
            BEGIN
                CREATE TABLE Sessions (
                    Id NVARCHAR(450) PRIMARY KEY,
                    HostName NVARCHAR(100) NOT NULL,
                    CreatedAt DATETIME2 NOT NULL,
                    StartedAt DATETIME2 NULL,
                    EndedAt DATETIME2 NULL,
                    Status NVARCHAR(50) NOT NULL,
                    CurrentMode NVARCHAR(50) NULL,
                    CurrentQuestionId INT NULL
                );
                CREATE INDEX IX_Sessions_CreatedAt ON Sessions(CreatedAt);
            END

            -- Participants table
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Participants')
            BEGIN
                CREATE TABLE Participants (
                    Id NVARCHAR(450) PRIMARY KEY,
                    SessionId NVARCHAR(450) NOT NULL,
                    DisplayName NVARCHAR(50) NOT NULL,
                    JoinedAt DATETIME2 NOT NULL,
                    ConnectionId NVARCHAR(450) NULL,
                    LastSeenAt DATETIME2 NULL,
                    IsActive BIT NOT NULL DEFAULT 1,
                    State NVARCHAR(50) NOT NULL DEFAULT 'Lobby',
                    HasPlayedFFF BIT NOT NULL DEFAULT 0,
                    HasUsedATA BIT NOT NULL DEFAULT 0,
                    SelectedForFFFAt DATETIME2 NULL,
                    BecameWinnerAt DATETIME2 NULL,
                    DeviceType NVARCHAR(50) NULL,
                    OSType NVARCHAR(50) NULL,
                    OSVersion NVARCHAR(50) NULL,
                    BrowserType NVARCHAR(100) NULL,
                    BrowserVersion NVARCHAR(50) NULL,
                    DisconnectedAt DATETIME2 NULL,
                    HasAgreedToPrivacy BIT NOT NULL DEFAULT 0,
                    FOREIGN KEY (SessionId) REFERENCES Sessions(Id) ON DELETE CASCADE
                );
                CREATE INDEX IX_Participants_SessionId ON Participants(SessionId);
                CREATE INDEX IX_Participants_ConnectionId ON Participants(ConnectionId);
            END

            -- FFFAnswers table
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FFFAnswers')
            BEGIN
                CREATE TABLE FFFAnswers (
                    Id INT PRIMARY KEY IDENTITY(1,1),
                    SessionId NVARCHAR(450) NOT NULL,
                    ParticipantId NVARCHAR(450) NOT NULL,
                    QuestionId INT NOT NULL,
                    AnswerSequence NVARCHAR(20) NOT NULL,
                    SubmittedAt DATETIME2 NOT NULL,
                    TimeElapsed FLOAT NOT NULL,
                    IsCorrect BIT NOT NULL DEFAULT 0,
                    Rank INT NULL,
                    FOREIGN KEY (SessionId) REFERENCES Sessions(Id) ON DELETE CASCADE
                );
                CREATE INDEX IX_FFFAnswers_SessionId_QuestionId ON FFFAnswers(SessionId, QuestionId);
                CREATE INDEX IX_FFFAnswers_SubmittedAt ON FFFAnswers(SubmittedAt);
            END

            -- ATAVotes table
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ATAVotes')
            BEGIN
                CREATE TABLE ATAVotes (
                    Id INT PRIMARY KEY IDENTITY(1,1),
                    SessionId NVARCHAR(450) NOT NULL,
                    ParticipantId NVARCHAR(450) NOT NULL,
                    QuestionText NVARCHAR(500) NOT NULL,
                    SelectedOption NVARCHAR(1) NOT NULL,
                    SubmittedAt DATETIME2 NOT NULL,
                    FOREIGN KEY (SessionId) REFERENCES Sessions(Id) ON DELETE CASCADE
                );
                CREATE INDEX IX_ATAVotes_SessionId ON ATAVotes(SessionId);
                CREATE INDEX IX_ATAVotes_SubmittedAt ON ATAVotes(SubmittedAt);
            END";

        using (var command = new SqlCommand(createWAPSTables, useDbConnection))
        {
            await command.ExecuteNonQueryAsync();
        }

        useDbConnection.Close();
    }

    /// <summary>
    /// Gets the full connection string including database name
    /// </summary>
    public string GetFullConnectionString()
    {
        return _connectionStringWithDb;
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }
}
