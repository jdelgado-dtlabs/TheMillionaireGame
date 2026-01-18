using Microsoft.Data.SqlClient;
using MillionaireGame.Core.Models;

namespace MillionaireGame.Core.Database;

/// <summary>
/// Repository for managing ThemePack data in the database
/// </summary>
public class ThemePackRepository : BaseRepository
{
    public ThemePackRepository(string connectionString) : base(connectionString)
    {
    }

    /// <summary>
    /// Get all theme packs
    /// </summary>
    public async Task<List<ThemePack>> GetAllPacksAsync()
    {
        const string query = "SELECT * FROM ThemePacks ORDER BY PackName";
        var packs = new List<ThemePack>();

        using var connection = await OpenConnectionAsync();
        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            packs.Add(MapPackFromReader(reader));
        }

        return packs;
    }

    /// <summary>
    /// Get a theme pack by ID
    /// </summary>
    public async Task<ThemePack?> GetPackByIdAsync(int packId)
    {
        const string query = "SELECT * FROM ThemePacks WHERE ThemePackId = @PackId";

        using var connection = await OpenConnectionAsync();
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@PackId", packId);
        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapPackFromReader(reader);
        }

        return null;
    }

    /// <summary>
    /// Get a theme pack by name
    /// </summary>
    public async Task<ThemePack?> GetPackByNameAsync(string packName)
    {
        const string query = "SELECT * FROM ThemePacks WHERE PackName = @PackName";

        using var connection = await OpenConnectionAsync();
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@PackName", packName);
        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapPackFromReader(reader);
        }

        return null;
    }

    /// <summary>
    /// Save a theme pack (insert or update)
    /// </summary>
    public async Task<int> SavePackAsync(ThemePack pack)
    {
        if (pack.ThemePackId == 0)
        {
            return await InsertPackAsync(pack);
        }
        else
        {
            await UpdatePackAsync(pack);
            return pack.ThemePackId;
        }
    }

    /// <summary>
    /// Delete a theme pack
    /// </summary>
    public async Task DeletePackAsync(int packId)
    {
        const string query = "DELETE FROM ThemePacks WHERE ThemePackId = @PackId";

        using var connection = await OpenConnectionAsync();
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@PackId", packId);
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Check if a pack exists by name
    /// </summary>
    public async Task<bool> PackExistsAsync(string packName)
    {
        const string query = "SELECT COUNT(*) FROM ThemePacks WHERE PackName = @PackName";

        using var connection = await OpenConnectionAsync();
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@PackName", packName);
        var count = (int)(await command.ExecuteScalarAsync() ?? 0);
        return count > 0;
    }

    private async Task<int> InsertPackAsync(ThemePack pack)
    {
        const string query = @"
            INSERT INTO ThemePacks (PackName, PackVersion, Author, Description, InstallPath, ImportDate)
            VALUES (@PackName, @PackVersion, @Author, @Description, @InstallPath, GETDATE());
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        using var connection = await OpenConnectionAsync();
        using var command = new SqlCommand(query, connection);
        
        command.Parameters.AddWithValue("@PackName", pack.PackName);
        command.Parameters.AddWithValue("@PackVersion", pack.PackVersion);
        command.Parameters.AddWithValue("@Author", (object?)pack.Author ?? DBNull.Value);
        command.Parameters.AddWithValue("@Description", (object?)pack.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@InstallPath", pack.InstallPath);

        var newId = await command.ExecuteScalarAsync();
        return Convert.ToInt32(newId);
    }

    private async Task UpdatePackAsync(ThemePack pack)
    {
        const string query = @"
            UPDATE ThemePacks
            SET PackName = @PackName,
                PackVersion = @PackVersion,
                Author = @Author,
                Description = @Description,
                InstallPath = @InstallPath
            WHERE ThemePackId = @PackId";

        using var connection = await OpenConnectionAsync();
        using var command = new SqlCommand(query, connection);
        
        command.Parameters.AddWithValue("@PackId", pack.ThemePackId);
        command.Parameters.AddWithValue("@PackName", pack.PackName);
        command.Parameters.AddWithValue("@PackVersion", pack.PackVersion);
        command.Parameters.AddWithValue("@Author", (object?)pack.Author ?? DBNull.Value);
        command.Parameters.AddWithValue("@Description", (object?)pack.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@InstallPath", pack.InstallPath);

        await command.ExecuteNonQueryAsync();
    }

    private ThemePack MapPackFromReader(SqlDataReader reader)
    {
        return new ThemePack
        {
            ThemePackId = reader.GetInt32(reader.GetOrdinal("ThemePackId")),
            PackName = reader.GetString(reader.GetOrdinal("PackName")),
            PackVersion = reader.GetString(reader.GetOrdinal("PackVersion")),
            Author = reader.IsDBNull(reader.GetOrdinal("Author")) ? null : reader.GetString(reader.GetOrdinal("Author")),
            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
            InstallPath = reader.GetString(reader.GetOrdinal("InstallPath")),
            ImportDate = reader.GetDateTime(reader.GetOrdinal("ImportDate"))
        };
    }
}
