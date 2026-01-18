using Microsoft.Data.SqlClient;
using MillionaireGame.Core.Models;

namespace MillionaireGame.Core.Database;

/// <summary>
/// Repository for managing ThemeBackground data in the database
/// </summary>
public class ThemeBackgroundRepository : BaseRepository, IThemeBackgroundRepository
{
    public ThemeBackgroundRepository(string connectionString) : base(connectionString)
    {
    }

    /// <summary>
    /// Get all backgrounds for a theme
    /// </summary>
    public async Task<List<ThemeBackground>> GetBackgroundsByThemeIdAsync(int themeId)
    {
        const string query = "SELECT * FROM ThemeBackgrounds WHERE ThemeId = @ThemeId ORDER BY ComponentType";
        var backgrounds = new List<ThemeBackground>();

        using var connection = await OpenConnectionAsync();
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@ThemeId", themeId);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            backgrounds.Add(MapBackgroundFromReader(reader));
        }

        return backgrounds;
    }

    /// <summary>
    /// Get a specific background by component type
    /// </summary>
    public async Task<ThemeBackground?> GetBackgroundByComponentAsync(int themeId, string componentType)
    {
        const string query = "SELECT * FROM ThemeBackgrounds WHERE ThemeId = @ThemeId AND ComponentType = @ComponentType";

        using var connection = await OpenConnectionAsync();
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@ThemeId", themeId);
        command.Parameters.AddWithValue("@ComponentType", componentType);
        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapBackgroundFromReader(reader);
        }

        return null;
    }

    /// <summary>
    /// Save a background (insert or update)
    /// </summary>
    public async Task<int> SaveBackgroundAsync(ThemeBackground background)
    {
        if (background.ThemeBackgroundId == 0)
        {
            return await InsertBackgroundAsync(background);
        }
        else
        {
            await UpdateBackgroundAsync(background);
            return background.ThemeBackgroundId;
        }
    }

    /// <summary>
    /// Delete a background
    /// </summary>
    public async Task DeleteBackgroundAsync(int backgroundId)
    {
        const string query = "DELETE FROM ThemeBackgrounds WHERE ThemeBackgroundId = @BackgroundId";

        using var connection = await OpenConnectionAsync();
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@BackgroundId", backgroundId);
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Delete all backgrounds for a theme
    /// </summary>
    public async Task DeleteBackgroundsByThemeIdAsync(int themeId)
    {
        const string query = "DELETE FROM ThemeBackgrounds WHERE ThemeId = @ThemeId";

        using var connection = await OpenConnectionAsync();
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@ThemeId", themeId);
        await command.ExecuteNonQueryAsync();
    }

    private async Task<int> InsertBackgroundAsync(ThemeBackground background)
    {
        const string query = @"
            INSERT INTO ThemeBackgrounds (ThemeId, ComponentType, ImagePath, ChromaKeyEnabled, ChromaKeyColor, ChromaKeyTolerance,
                                          ScaleMode, PositionX, PositionY, Transparency)
            VALUES (@ThemeId, @ComponentType, @ImagePath, @ChromaKeyEnabled, @ChromaKeyColor, @ChromaKeyTolerance,
                    @ScaleMode, @PositionX, @PositionY, @Transparency);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        using var connection = await OpenConnectionAsync();
        using var command = new SqlCommand(query, connection);
        
        command.Parameters.AddWithValue("@ThemeId", background.ThemeId);
        command.Parameters.AddWithValue("@ComponentType", background.ComponentType);
        command.Parameters.AddWithValue("@ImagePath", (object?)background.ImagePath ?? DBNull.Value);
        command.Parameters.AddWithValue("@ChromaKeyEnabled", background.ChromaKeyEnabled);
        command.Parameters.AddWithValue("@ChromaKeyColor", (object?)background.ChromaKeyColor ?? DBNull.Value);
        command.Parameters.AddWithValue("@ChromaKeyTolerance", background.ChromaKeyTolerance);
        command.Parameters.AddWithValue("@ScaleMode", background.ScaleMode);
        command.Parameters.AddWithValue("@PositionX", background.PositionX);
        command.Parameters.AddWithValue("@PositionY", background.PositionY);
        command.Parameters.AddWithValue("@Transparency", background.Transparency);

        var newId = await command.ExecuteScalarAsync();
        return Convert.ToInt32(newId);
    }

    private async Task UpdateBackgroundAsync(ThemeBackground background)
    {
        const string query = @"
            UPDATE ThemeBackgrounds
            SET ComponentType = @ComponentType,
                ImagePath = @ImagePath,
                ChromaKeyEnabled = @ChromaKeyEnabled,
                ChromaKeyColor = @ChromaKeyColor,
                ChromaKeyTolerance = @ChromaKeyTolerance,
                ScaleMode = @ScaleMode,
                PositionX = @PositionX,
                PositionY = @PositionY,
                Transparency = @Transparency
            WHERE ThemeBackgroundId = @BackgroundId";

        using var connection = await OpenConnectionAsync();
        using var command = new SqlCommand(query, connection);
        
        command.Parameters.AddWithValue("@BackgroundId", background.ThemeBackgroundId);
        command.Parameters.AddWithValue("@ComponentType", background.ComponentType);
        command.Parameters.AddWithValue("@ImagePath", (object?)background.ImagePath ?? DBNull.Value);
        command.Parameters.AddWithValue("@ChromaKeyEnabled", background.ChromaKeyEnabled);
        command.Parameters.AddWithValue("@ChromaKeyColor", (object?)background.ChromaKeyColor ?? DBNull.Value);
        command.Parameters.AddWithValue("@ChromaKeyTolerance", background.ChromaKeyTolerance);
        command.Parameters.AddWithValue("@ScaleMode", background.ScaleMode);
        command.Parameters.AddWithValue("@PositionX", background.PositionX);
        command.Parameters.AddWithValue("@PositionY", background.PositionY);
        command.Parameters.AddWithValue("@Transparency", background.Transparency);

        await command.ExecuteNonQueryAsync();
    }

    private ThemeBackground MapBackgroundFromReader(SqlDataReader reader)
    {
        return new ThemeBackground
        {
            ThemeBackgroundId = reader.GetInt32(reader.GetOrdinal("ThemeBackgroundId")),
            ThemeId = reader.GetInt32(reader.GetOrdinal("ThemeId")),
            ComponentType = reader.GetString(reader.GetOrdinal("ComponentType")),
            ImagePath = reader.IsDBNull(reader.GetOrdinal("ImagePath")) ? null : reader.GetString(reader.GetOrdinal("ImagePath")),
            ChromaKeyEnabled = reader.GetBoolean(reader.GetOrdinal("ChromaKeyEnabled")),
            ChromaKeyColor = reader.IsDBNull(reader.GetOrdinal("ChromaKeyColor")) ? null : reader.GetString(reader.GetOrdinal("ChromaKeyColor")),
            ChromaKeyTolerance = reader.GetInt32(reader.GetOrdinal("ChromaKeyTolerance")),
            ScaleMode = reader.GetString(reader.GetOrdinal("ScaleMode")),
            PositionX = reader.GetInt32(reader.GetOrdinal("PositionX")),
            PositionY = reader.GetInt32(reader.GetOrdinal("PositionY")),
            Transparency = reader.GetInt32(reader.GetOrdinal("Transparency"))
        };
    }
}
