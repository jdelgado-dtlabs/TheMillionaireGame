using System.Data.SqlClient;

namespace MillionaireGame.Core.Database;

/// <summary>
/// Manages ApplicationSettings in the database
/// </summary>
public class ApplicationSettingsRepository
{
    private readonly string _connectionString;

    public ApplicationSettingsRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Check if the ApplicationSettings table exists
    /// </summary>
    public async Task<bool> SettingsTableExistsAsync()
    {
        const string query = @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_NAME = 'ApplicationSettings'
            ) THEN 1 ELSE 0 END";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using var command = new SqlCommand(query, connection);
        var result = await command.ExecuteScalarAsync();
        return Convert.ToBoolean(result);
    }

    /// <summary>
    /// Create the ApplicationSettings table
    /// </summary>
    public async Task CreateSettingsTableAsync()
    {
        const string query = @"
            CREATE TABLE ApplicationSettings (
                SettingKey NVARCHAR(255) PRIMARY KEY,
                SettingValue NVARCHAR(MAX) NULL,
                Category NVARCHAR(100) NULL,
                Description NVARCHAR(500) NULL,
                LastModified DATETIME DEFAULT GETDATE()
            );

            CREATE INDEX IX_ApplicationSettings_Category ON ApplicationSettings(Category);
        ";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using var command = new SqlCommand(query, connection);
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Check if settings data exists in the table
    /// </summary>
    public async Task<bool> SettingsDataExistsAsync()
    {
        const string query = "SELECT COUNT(*) FROM ApplicationSettings";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using var command = new SqlCommand(query, connection);
        var count = (int)await command.ExecuteScalarAsync();
        return count > 0;
    }

    /// <summary>
    /// Save a setting value
    /// </summary>
    public async Task SaveSettingAsync(string key, string? value, string? category = null, string? description = null)
    {
        const string query = @"
            IF EXISTS (SELECT 1 FROM ApplicationSettings WHERE SettingKey = @Key)
                UPDATE ApplicationSettings 
                SET SettingValue = @Value, 
                    Category = @Category,
                    Description = @Description,
                    LastModified = GETDATE()
                WHERE SettingKey = @Key
            ELSE
                INSERT INTO ApplicationSettings (SettingKey, SettingValue, Category, Description)
                VALUES (@Key, @Value, @Category, @Description)
        ";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Key", key);
        command.Parameters.AddWithValue("@Value", (object?)value ?? DBNull.Value);
        command.Parameters.AddWithValue("@Category", (object?)category ?? DBNull.Value);
        command.Parameters.AddWithValue("@Description", (object?)description ?? DBNull.Value);
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Get a setting value
    /// </summary>
    public async Task<string?> GetSettingAsync(string key)
    {
        const string query = "SELECT SettingValue FROM ApplicationSettings WHERE SettingKey = @Key";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Key", key);
        var result = await command.ExecuteScalarAsync();
        return result == DBNull.Value ? null : result?.ToString();
    }

    /// <summary>
    /// Get all settings
    /// </summary>
    public async Task<Dictionary<string, string>> GetAllSettingsAsync()
    {
        const string query = "SELECT SettingKey, SettingValue FROM ApplicationSettings";
        var settings = new Dictionary<string, string>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var key = reader.GetString(0);
            var value = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
            settings[key] = value;
        }

        return settings;
    }

    /// <summary>
    /// Get all settings by category
    /// </summary>
    public async Task<Dictionary<string, string>> GetSettingsByCategoryAsync(string category)
    {
        const string query = "SELECT SettingKey, SettingValue FROM ApplicationSettings WHERE Category = @Category";
        var settings = new Dictionary<string, string>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Category", category);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var key = reader.GetString(0);
            var value = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
            settings[key] = value;
        }

        return settings;
    }

    /// <summary>
    /// Delete all settings (for testing/reset)
    /// </summary>
    public async Task DeleteAllSettingsAsync()
    {
        const string query = "DELETE FROM ApplicationSettings";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using var command = new SqlCommand(query, connection);
        await command.ExecuteNonQueryAsync();
    }
}
