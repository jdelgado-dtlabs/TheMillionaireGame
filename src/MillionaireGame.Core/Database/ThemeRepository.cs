using Microsoft.Data.SqlClient;
using MillionaireGame.Core.Models;

namespace MillionaireGame.Core.Database;

/// <summary>
/// Repository for managing Theme data in the database
/// </summary>
public class ThemeRepository : BaseRepository, IThemeRepository
{
    public ThemeRepository(string connectionString) : base(connectionString)
    {
    }

    /// <summary>
    /// Get the currently active theme
    /// </summary>
    public async Task<Theme?> GetActiveThemeAsync()
    {
        const string query = "SELECT * FROM Themes WHERE IsActive = 1";

        using var connection = await OpenConnectionAsync();
        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapThemeFromReader(reader);
        }

        return null;
    }

    /// <summary>
    /// Get a theme by ID
    /// </summary>
    public async Task<Theme?> GetThemeByIdAsync(int themeId)
    {
        const string query = "SELECT * FROM Themes WHERE ThemeId = @ThemeId";

        using var connection = await OpenConnectionAsync();
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@ThemeId", themeId);
        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapThemeFromReader(reader);
        }

        return null;
    }

    /// <summary>
    /// Get all themes
    /// </summary>
    public async Task<List<Theme>> GetAllThemesAsync()
    {
        const string query = "SELECT * FROM Themes ORDER BY ThemeType, ThemeName";
        var themes = new List<Theme>();

        using var connection = await OpenConnectionAsync();
        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            themes.Add(MapThemeFromReader(reader));
        }

        return themes;
    }

    /// <summary>
    /// Get themes by type (Preset, UserProfile1, UserProfile2, Custom)
    /// </summary>
    public async Task<List<Theme>> GetThemesByTypeAsync(string themeType)
    {
        const string query = "SELECT * FROM Themes WHERE ThemeType = @ThemeType ORDER BY ThemeName";
        var themes = new List<Theme>();

        using var connection = await OpenConnectionAsync();
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@ThemeType", themeType);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            themes.Add(MapThemeFromReader(reader));
        }

        return themes;
    }

    /// <summary>
    /// Save a theme (insert or update)
    /// </summary>
    public async Task<int> SaveThemeAsync(Theme theme)
    {
        if (theme.ThemeId == 0)
        {
            return await InsertThemeAsync(theme);
        }
        else
        {
            await UpdateThemeAsync(theme);
            return theme.ThemeId;
        }
    }

    /// <summary>
    /// Set a theme as active (deactivates all other themes)
    /// </summary>
    public async Task SetActiveThemeAsync(int themeId)
    {
        using var connection = await OpenConnectionAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            // Deactivate all themes
            const string deactivateQuery = "UPDATE Themes SET IsActive = 0";
            using (var command = new SqlCommand(deactivateQuery, connection, transaction))
            {
                await command.ExecuteNonQueryAsync();
            }

            // Activate the specified theme
            const string activateQuery = "UPDATE Themes SET IsActive = 1, ModifiedDate = GETDATE() WHERE ThemeId = @ThemeId";
            using (var command = new SqlCommand(activateQuery, connection, transaction))
            {
                command.Parameters.AddWithValue("@ThemeId", themeId);
                await command.ExecuteNonQueryAsync();
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    /// <summary>
    /// Delete a theme
    /// </summary>
    public async Task DeleteThemeAsync(int themeId)
    {
        const string query = "DELETE FROM Themes WHERE ThemeId = @ThemeId";

        using var connection = await OpenConnectionAsync();
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@ThemeId", themeId);
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Check if a theme exists by name
    /// </summary>
    public async Task<bool> ThemeExistsAsync(string themeName)
    {
        const string query = "SELECT COUNT(*) FROM Themes WHERE ThemeName = @ThemeName";

        using var connection = await OpenConnectionAsync();
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@ThemeName", themeName);
        var count = (int)(await command.ExecuteScalarAsync() ?? 0);
        return count > 0;
    }

    private async Task<int> InsertThemeAsync(Theme theme)
    {
        const string query = @"
            INSERT INTO Themes (ThemeName, ThemeType, ThemePackId, IsActive, Description, Author, Version, CreatedDate, ModifiedDate)
            VALUES (@ThemeName, @ThemeType, @ThemePackId, @IsActive, @Description, @Author, @Version, GETDATE(), GETDATE());
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        using var connection = await OpenConnectionAsync();
        using var command = new SqlCommand(query, connection);
        
        command.Parameters.AddWithValue("@ThemeName", theme.ThemeName);
        command.Parameters.AddWithValue("@ThemeType", theme.ThemeType);
        command.Parameters.AddWithValue("@ThemePackId", (object?)theme.ThemePackId ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsActive", theme.IsActive);
        command.Parameters.AddWithValue("@Description", (object?)theme.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@Author", (object?)theme.Author ?? DBNull.Value);
        command.Parameters.AddWithValue("@Version", (object?)theme.Version ?? DBNull.Value);

        var newId = await command.ExecuteScalarAsync();
        return Convert.ToInt32(newId);
    }

    private async Task UpdateThemeAsync(Theme theme)
    {
        const string query = @"
            UPDATE Themes
            SET ThemeName = @ThemeName,
                ThemeType = @ThemeType,
                ThemePackId = @ThemePackId,
                IsActive = @IsActive,
                Description = @Description,
                Author = @Author,
                Version = @Version,
                ModifiedDate = GETDATE()
            WHERE ThemeId = @ThemeId";

        using var connection = await OpenConnectionAsync();
        using var command = new SqlCommand(query, connection);
        
        command.Parameters.AddWithValue("@ThemeId", theme.ThemeId);
        command.Parameters.AddWithValue("@ThemeName", theme.ThemeName);
        command.Parameters.AddWithValue("@ThemeType", theme.ThemeType);
        command.Parameters.AddWithValue("@ThemePackId", (object?)theme.ThemePackId ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsActive", theme.IsActive);
        command.Parameters.AddWithValue("@Description", (object?)theme.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@Author", (object?)theme.Author ?? DBNull.Value);
        command.Parameters.AddWithValue("@Version", (object?)theme.Version ?? DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }

    private Theme MapThemeFromReader(SqlDataReader reader)
    {
        return new Theme
        {
            ThemeId = reader.GetInt32(reader.GetOrdinal("ThemeId")),
            ThemeName = reader.GetString(reader.GetOrdinal("ThemeName")),
            ThemeType = reader.GetString(reader.GetOrdinal("ThemeType")),
            ThemePackId = reader.IsDBNull(reader.GetOrdinal("ThemePackId")) ? null : reader.GetInt32(reader.GetOrdinal("ThemePackId")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
            Author = reader.IsDBNull(reader.GetOrdinal("Author")) ? null : reader.GetString(reader.GetOrdinal("Author")),
            Version = reader.IsDBNull(reader.GetOrdinal("Version")) ? null : reader.GetString(reader.GetOrdinal("Version")),
            CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
            ModifiedDate = reader.GetDateTime(reader.GetOrdinal("ModifiedDate"))
        };
    }
}
