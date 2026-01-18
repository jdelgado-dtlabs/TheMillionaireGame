using Microsoft.Data.SqlClient;
using MillionaireGame.Core.Models;

namespace MillionaireGame.Core.Database;

/// <summary>
/// Repository for managing ThemeMoneyTree data in the database
/// </summary>
public class ThemeMoneyTreeRepository : BaseRepository
{
    public ThemeMoneyTreeRepository(string connectionString) : base(connectionString)
    {
    }

    /// <summary>
    /// Get money tree configuration for a theme
    /// </summary>
    public async Task<ThemeMoneyTree?> GetMoneyTreeByThemeIdAsync(int themeId)
    {
        const string query = "SELECT * FROM ThemeMoneyTree WHERE ThemeId = @ThemeId";

        using var connection = await OpenConnectionAsync();
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@ThemeId", themeId);
        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapMoneyTreeFromReader(reader);
        }

        return null;
    }

    /// <summary>
    /// Save money tree configuration (insert or update)
    /// </summary>
    public async Task<int> SaveMoneyTreeAsync(ThemeMoneyTree moneyTree)
    {
        // Check if it exists
        var existing = await GetMoneyTreeByThemeIdAsync(moneyTree.ThemeId);
        
        if (existing == null)
        {
            return await InsertMoneyTreeAsync(moneyTree);
        }
        else
        {
            moneyTree.ThemeMoneyTreeId = existing.ThemeMoneyTreeId;
            await UpdateMoneyTreeAsync(moneyTree);
            return moneyTree.ThemeMoneyTreeId;
        }
    }

    /// <summary>
    /// Delete money tree configuration
    /// </summary>
    public async Task DeleteMoneyTreeAsync(int moneyTreeId)
    {
        const string query = "DELETE FROM ThemeMoneyTree WHERE ThemeMoneyTreeId = @MoneyTreeId";

        using var connection = await OpenConnectionAsync();
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@MoneyTreeId", moneyTreeId);
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Delete money tree configuration by theme ID
    /// </summary>
    public async Task DeleteMoneyTreeByThemeIdAsync(int themeId)
    {
        const string query = "DELETE FROM ThemeMoneyTree WHERE ThemeId = @ThemeId";

        using var connection = await OpenConnectionAsync();
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@ThemeId", themeId);
        await command.ExecuteNonQueryAsync();
    }

    private async Task<int> InsertMoneyTreeAsync(ThemeMoneyTree moneyTree)
    {
        const string query = @"
            INSERT INTO ThemeMoneyTree (
                ThemeId, BackgroundImagePath,
                InactiveColor, ActiveColor, CompletedColor, SafeHavenColor,
                HighlightEnabled, HighlightType, HighlightColor, HighlightIntensity,
                FontFamily, FontSize, FontBold
            )
            VALUES (
                @ThemeId, @BackgroundImagePath,
                @InactiveColor, @ActiveColor, @CompletedColor, @SafeHavenColor,
                @HighlightEnabled, @HighlightType, @HighlightColor, @HighlightIntensity,
                @FontFamily, @FontSize, @FontBold
            );
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        using var connection = await OpenConnectionAsync();
        using var command = new SqlCommand(query, connection);
        
        AddMoneyTreeParameters(command, moneyTree);

        var newId = await command.ExecuteScalarAsync();
        return Convert.ToInt32(newId);
    }

    private async Task UpdateMoneyTreeAsync(ThemeMoneyTree moneyTree)
    {
        const string query = @"
            UPDATE ThemeMoneyTree
            SET BackgroundImagePath = @BackgroundImagePath,
                InactiveColor = @InactiveColor,
                ActiveColor = @ActiveColor,
                CompletedColor = @CompletedColor,
                SafeHavenColor = @SafeHavenColor,
                HighlightEnabled = @HighlightEnabled,
                HighlightType = @HighlightType,
                HighlightColor = @HighlightColor,
                HighlightIntensity = @HighlightIntensity,
                FontFamily = @FontFamily,
                FontSize = @FontSize,
                FontBold = @FontBold
            WHERE ThemeMoneyTreeId = @MoneyTreeId";

        using var connection = await OpenConnectionAsync();
        using var command = new SqlCommand(query, connection);
        
        command.Parameters.AddWithValue("@MoneyTreeId", moneyTree.ThemeMoneyTreeId);
        AddMoneyTreeParameters(command, moneyTree);

        await command.ExecuteNonQueryAsync();
    }

    private void AddMoneyTreeParameters(SqlCommand command, ThemeMoneyTree moneyTree)
    {
        command.Parameters.AddWithValue("@ThemeId", moneyTree.ThemeId);
        command.Parameters.AddWithValue("@BackgroundImagePath", (object?)moneyTree.BackgroundImagePath ?? DBNull.Value);
        command.Parameters.AddWithValue("@InactiveColor", moneyTree.InactiveColor);
        command.Parameters.AddWithValue("@ActiveColor", moneyTree.ActiveColor);
        command.Parameters.AddWithValue("@CompletedColor", moneyTree.CompletedColor);
        command.Parameters.AddWithValue("@SafeHavenColor", moneyTree.SafeHavenColor);
        command.Parameters.AddWithValue("@HighlightEnabled", moneyTree.HighlightEnabled);
        command.Parameters.AddWithValue("@HighlightType", moneyTree.HighlightType);
        command.Parameters.AddWithValue("@HighlightColor", moneyTree.HighlightColor);
        command.Parameters.AddWithValue("@HighlightIntensity", moneyTree.HighlightIntensity);
        command.Parameters.AddWithValue("@FontFamily", moneyTree.FontFamily);
        command.Parameters.AddWithValue("@FontSize", moneyTree.FontSize);
        command.Parameters.AddWithValue("@FontBold", moneyTree.FontBold);
    }

    private ThemeMoneyTree MapMoneyTreeFromReader(SqlDataReader reader)
    {
        return new ThemeMoneyTree
        {
            ThemeMoneyTreeId = reader.GetInt32(reader.GetOrdinal("ThemeMoneyTreeId")),
            ThemeId = reader.GetInt32(reader.GetOrdinal("ThemeId")),
            BackgroundImagePath = reader.IsDBNull(reader.GetOrdinal("BackgroundImagePath")) ? null : reader.GetString(reader.GetOrdinal("BackgroundImagePath")),
            InactiveColor = reader.GetString(reader.GetOrdinal("InactiveColor")),
            ActiveColor = reader.GetString(reader.GetOrdinal("ActiveColor")),
            CompletedColor = reader.GetString(reader.GetOrdinal("CompletedColor")),
            SafeHavenColor = reader.GetString(reader.GetOrdinal("SafeHavenColor")),
            HighlightEnabled = reader.GetBoolean(reader.GetOrdinal("HighlightEnabled")),
            HighlightType = reader.GetString(reader.GetOrdinal("HighlightType")),
            HighlightColor = reader.GetString(reader.GetOrdinal("HighlightColor")),
            HighlightIntensity = reader.GetInt32(reader.GetOrdinal("HighlightIntensity")),
            FontFamily = reader.GetString(reader.GetOrdinal("FontFamily")),
            FontSize = reader.GetInt32(reader.GetOrdinal("FontSize")),
            FontBold = reader.GetBoolean(reader.GetOrdinal("FontBold"))
        };
    }
}
