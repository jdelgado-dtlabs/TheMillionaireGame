using System.Data.SqlClient;
using MillionaireGame.Core.Models;

namespace MillionaireGame.Core.Database;

/// <summary>
/// Database context for accessing the Millionaire Game database
/// </summary>
public class GameDatabaseContext : IDisposable
{
    private readonly string _connectionString;
    private SqlConnection? _connection;
    private const string DatabaseName = "dbMillionaire";

    public GameDatabaseContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Opens a connection to the database
    /// </summary>
    public async Task OpenConnectionAsync()
    {
        if (_connection?.State == System.Data.ConnectionState.Open)
            return;

        _connection = new SqlConnection(_connectionString);
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
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = $"SELECT COUNT(*) FROM sys.databases WHERE name = '{DatabaseName}'";
            using var command = new SqlCommand(query, connection);
            var result = (int)await command.ExecuteScalarAsync();

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
        using var connection = new SqlConnection(_connectionString);
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
        var useDbConnection = new SqlConnection(_connectionString + $";Database={DatabaseName}");
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
                    Difficulty_Type NVARCHAR(50) NOT NULL,
                    Level INT NOT NULL,
                    LevelRange NVARCHAR(50),
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

        useDbConnection.Close();
    }

    /// <summary>
    /// Gets the full connection string including database name
    /// </summary>
    public string GetFullConnectionString()
    {
        return _connectionString + $";Database={DatabaseName}";
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }
}
