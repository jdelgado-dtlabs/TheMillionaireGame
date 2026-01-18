using Microsoft.Data.SqlClient;
using MillionaireGame.Core.Models;

namespace MillionaireGame.Core.Database;

/// <summary>
/// Repository for managing ThemeStrap data in the database
/// </summary>
public class ThemeStrapRepository : BaseRepository
{
    public ThemeStrapRepository(string connectionString) : base(connectionString)
    {
    }

    /// <summary>
    /// Get all straps for a theme
    /// </summary>
    public async Task<List<ThemeStrap>> GetStrapsByThemeIdAsync(int themeId)
    {
        const string query = "SELECT * FROM ThemeStraps WHERE ThemeId = @ThemeId ORDER BY StrapType";
        var straps = new List<ThemeStrap>();

        using var connection = await OpenConnectionAsync();
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@ThemeId", themeId);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            straps.Add(MapStrapFromReader(reader));
        }

        return straps;
    }

    /// <summary>
    /// Get a specific strap by type
    /// </summary>
    public async Task<ThemeStrap?> GetStrapByTypeAsync(int themeId, string strapType)
    {
        const string query = "SELECT * FROM ThemeStraps WHERE ThemeId = @ThemeId AND StrapType = @StrapType";

        using var connection = await OpenConnectionAsync();
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@ThemeId", themeId);
        command.Parameters.AddWithValue("@StrapType", strapType);
        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapStrapFromReader(reader);
        }

        return null;
    }

    /// <summary>
    /// Save a strap (insert or update)
    /// </summary>
    public async Task<int> SaveStrapAsync(ThemeStrap strap)
    {
        if (strap.ThemeStrapId == 0)
        {
            return await InsertStrapAsync(strap);
        }
        else
        {
            await UpdateStrapAsync(strap);
            return strap.ThemeStrapId;
        }
    }

    /// <summary>
    /// Delete a strap
    /// </summary>
    public async Task DeleteStrapAsync(int strapId)
    {
        const string query = "DELETE FROM ThemeStraps WHERE ThemeStrapId = @StrapId";

        using var connection = await OpenConnectionAsync();
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@StrapId", strapId);
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Delete all straps for a theme
    /// </summary>
    public async Task DeleteStrapsByThemeIdAsync(int themeId)
    {
        const string query = "DELETE FROM ThemeStraps WHERE ThemeId = @ThemeId";

        using var connection = await OpenConnectionAsync();
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@ThemeId", themeId);
        await command.ExecuteNonQueryAsync();
    }

    private async Task<int> InsertStrapAsync(ThemeStrap strap)
    {
        const string query = @"
            INSERT INTO ThemeStraps (
                ThemeId, StrapType, SvgShape,
                PrimaryColor, SecondaryColor, GradientEnabled, GradientAngle,
                EffectType, EffectIntensity, EffectColor,
                BorderEnabled, BorderColor, BorderWidth, BorderStyle,
                FontFamily, FontSize, FontColor, FontBold, FontItalic,
                AnimationEnabled, AnimationType, AnimationDuration
            )
            VALUES (
                @ThemeId, @StrapType, @SvgShape,
                @PrimaryColor, @SecondaryColor, @GradientEnabled, @GradientAngle,
                @EffectType, @EffectIntensity, @EffectColor,
                @BorderEnabled, @BorderColor, @BorderWidth, @BorderStyle,
                @FontFamily, @FontSize, @FontColor, @FontBold, @FontItalic,
                @AnimationEnabled, @AnimationType, @AnimationDuration
            );
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        using var connection = await OpenConnectionAsync();
        using var command = new SqlCommand(query, connection);
        
        AddStrapParameters(command, strap);

        var newId = await command.ExecuteScalarAsync();
        return Convert.ToInt32(newId);
    }

    private async Task UpdateStrapAsync(ThemeStrap strap)
    {
        const string query = @"
            UPDATE ThemeStraps
            SET StrapType = @StrapType,
                SvgShape = @SvgShape,
                PrimaryColor = @PrimaryColor,
                SecondaryColor = @SecondaryColor,
                GradientEnabled = @GradientEnabled,
                GradientAngle = @GradientAngle,
                EffectType = @EffectType,
                EffectIntensity = @EffectIntensity,
                EffectColor = @EffectColor,
                BorderEnabled = @BorderEnabled,
                BorderColor = @BorderColor,
                BorderWidth = @BorderWidth,
                BorderStyle = @BorderStyle,
                FontFamily = @FontFamily,
                FontSize = @FontSize,
                FontColor = @FontColor,
                FontBold = @FontBold,
                FontItalic = @FontItalic,
                AnimationEnabled = @AnimationEnabled,
                AnimationType = @AnimationType,
                AnimationDuration = @AnimationDuration
            WHERE ThemeStrapId = @StrapId";

        using var connection = await OpenConnectionAsync();
        using var command = new SqlCommand(query, connection);
        
        command.Parameters.AddWithValue("@StrapId", strap.ThemeStrapId);
        AddStrapParameters(command, strap);

        await command.ExecuteNonQueryAsync();
    }

    private void AddStrapParameters(SqlCommand command, ThemeStrap strap)
    {
        command.Parameters.AddWithValue("@ThemeId", strap.ThemeId);
        command.Parameters.AddWithValue("@StrapType", strap.StrapType);
        command.Parameters.AddWithValue("@SvgShape", strap.SvgShape);
        command.Parameters.AddWithValue("@PrimaryColor", strap.PrimaryColor);
        command.Parameters.AddWithValue("@SecondaryColor", (object?)strap.SecondaryColor ?? DBNull.Value);
        command.Parameters.AddWithValue("@GradientEnabled", strap.GradientEnabled);
        command.Parameters.AddWithValue("@GradientAngle", strap.GradientAngle);
        command.Parameters.AddWithValue("@EffectType", (object?)strap.EffectType ?? DBNull.Value);
        command.Parameters.AddWithValue("@EffectIntensity", strap.EffectIntensity);
        command.Parameters.AddWithValue("@EffectColor", (object?)strap.EffectColor ?? DBNull.Value);
        command.Parameters.AddWithValue("@BorderEnabled", strap.BorderEnabled);
        command.Parameters.AddWithValue("@BorderColor", strap.BorderColor);
        command.Parameters.AddWithValue("@BorderWidth", strap.BorderWidth);
        command.Parameters.AddWithValue("@BorderStyle", strap.BorderStyle);
        command.Parameters.AddWithValue("@FontFamily", strap.FontFamily);
        command.Parameters.AddWithValue("@FontSize", strap.FontSize);
        command.Parameters.AddWithValue("@FontColor", strap.FontColor);
        command.Parameters.AddWithValue("@FontBold", strap.FontBold);
        command.Parameters.AddWithValue("@FontItalic", strap.FontItalic);
        command.Parameters.AddWithValue("@AnimationEnabled", strap.AnimationEnabled);
        command.Parameters.AddWithValue("@AnimationType", (object?)strap.AnimationType ?? DBNull.Value);
        command.Parameters.AddWithValue("@AnimationDuration", strap.AnimationDuration);
    }

    private ThemeStrap MapStrapFromReader(SqlDataReader reader)
    {
        return new ThemeStrap
        {
            ThemeStrapId = reader.GetInt32(reader.GetOrdinal("ThemeStrapId")),
            ThemeId = reader.GetInt32(reader.GetOrdinal("ThemeId")),
            StrapType = reader.GetString(reader.GetOrdinal("StrapType")),
            SvgShape = reader.GetString(reader.GetOrdinal("SvgShape")),
            PrimaryColor = reader.GetString(reader.GetOrdinal("PrimaryColor")),
            SecondaryColor = reader.IsDBNull(reader.GetOrdinal("SecondaryColor")) ? null : reader.GetString(reader.GetOrdinal("SecondaryColor")),
            GradientEnabled = reader.GetBoolean(reader.GetOrdinal("GradientEnabled")),
            GradientAngle = reader.GetInt32(reader.GetOrdinal("GradientAngle")),
            EffectType = reader.IsDBNull(reader.GetOrdinal("EffectType")) ? null : reader.GetString(reader.GetOrdinal("EffectType")),
            EffectIntensity = reader.GetInt32(reader.GetOrdinal("EffectIntensity")),
            EffectColor = reader.IsDBNull(reader.GetOrdinal("EffectColor")) ? null : reader.GetString(reader.GetOrdinal("EffectColor")),
            BorderEnabled = reader.GetBoolean(reader.GetOrdinal("BorderEnabled")),
            BorderColor = reader.GetString(reader.GetOrdinal("BorderColor")),
            BorderWidth = reader.GetInt32(reader.GetOrdinal("BorderWidth")),
            BorderStyle = reader.GetString(reader.GetOrdinal("BorderStyle")),
            FontFamily = reader.GetString(reader.GetOrdinal("FontFamily")),
            FontSize = reader.GetInt32(reader.GetOrdinal("FontSize")),
            FontColor = reader.GetString(reader.GetOrdinal("FontColor")),
            FontBold = reader.GetBoolean(reader.GetOrdinal("FontBold")),
            FontItalic = reader.GetBoolean(reader.GetOrdinal("FontItalic")),
            AnimationEnabled = reader.GetBoolean(reader.GetOrdinal("AnimationEnabled")),
            AnimationType = reader.IsDBNull(reader.GetOrdinal("AnimationType")) ? null : reader.GetString(reader.GetOrdinal("AnimationType")),
            AnimationDuration = reader.GetInt32(reader.GetOrdinal("AnimationDuration"))
        };
    }
}
