using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using MillionaireGame.Utilities;

namespace MillionaireGame.Database
{
    /// <summary>
    /// Manages database schema migrations using embedded SQL files.
    /// Migrations are executed sequentially on application startup.
    /// </summary>
    public class MigrationRunner
    {
        private readonly string _connectionString;

        public MigrationRunner(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        /// <summary>
        /// Run all pending migrations. Returns true if all succeeded or were already applied.
        /// </summary>
        public async Task<bool> RunMigrationsAsync()
        {
            try
            {
                GameConsole.Info("[Migrations] Starting database migration check...");

                // 1. Ensure __MigrationHistory table exists
                await EnsureMigrationTableExistsAsync();

                // 2. Get list of already applied migrations
                var appliedMigrations = await GetAppliedMigrationsAsync();
                GameConsole.Debug($"[Migrations] Found {appliedMigrations.Count} applied migrations");

                // 3. Load embedded migration resources
                var migrationFiles = LoadEmbeddedMigrations();
                GameConsole.Debug($"[Migrations] Found {migrationFiles.Count} embedded migration(s)");

                // 4. Filter to pending migrations
                var pendingMigrations = migrationFiles
                    .Where(m => !appliedMigrations.Contains(m.MigrationId))
                    .OrderBy(m => m.SerialNumber)
                    .ToList();

                if (pendingMigrations.Count == 0)
                {
                    GameConsole.Info("[Migrations] Database schema is up to date");
                    return true;
                }

                GameConsole.Info($"[Migrations] Found {pendingMigrations.Count} pending migration(s)");

                // 5. Apply each pending migration
                int successCount = 0;
                foreach (var migration in pendingMigrations)
                {
                    if (await ApplyMigrationAsync(migration))
                    {
                        successCount++;
                    }
                    else
                    {
                        GameConsole.Error($"[Migrations] Migration {migration.MigrationId} failed - stopping");
                        return false;
                    }
                }

                GameConsole.Info($"[Migrations] Successfully applied {successCount} migration(s)");
                return true;
            }
            catch (Exception ex)
            {
                GameConsole.Error($"[Migrations] Fatal error during migration: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get list of all migrations and their status
        /// </summary>
        public async Task<List<MigrationStatus>> GetMigrationStatusAsync()
        {
            await EnsureMigrationTableExistsAsync();

            var appliedMigrations = await GetAppliedMigrationsAsync();
            var allMigrations = LoadEmbeddedMigrations();

            var statusList = new List<MigrationStatus>();

            foreach (var migration in allMigrations.OrderBy(m => m.SerialNumber))
            {
                var isApplied = appliedMigrations.Contains(migration.MigrationId);
                DateTime? appliedAt = null;
                bool? success = null;

                if (isApplied)
                {
                    // Get details from history
                    using var connection = new SqlConnection(_connectionString);
                    await connection.OpenAsync();
                    var sql = "SELECT AppliedAt, Success FROM __MigrationHistory WHERE MigrationId = @MigrationId";
                    using var command = new SqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@MigrationId", migration.MigrationId);
                    using var reader = await command.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        appliedAt = reader.GetDateTime(0);
                        success = reader.GetBoolean(1);
                    }
                }

                statusList.Add(new MigrationStatus
                {
                    MigrationId = migration.MigrationId,
                    FileName = migration.FileName,
                    IsApplied = isApplied,
                    AppliedAt = appliedAt,
                    Success = success
                });
            }

            return statusList;
        }

        private async Task EnsureMigrationTableExistsAsync()
        {
            var sql = @"
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '__MigrationHistory')
                BEGIN
                    CREATE TABLE __MigrationHistory (
                        MigrationId NVARCHAR(50) PRIMARY KEY,
                        FileName NVARCHAR(255) NOT NULL,
                        AppliedAt DATETIME NOT NULL,
                        ExecutionTimeMs INT NOT NULL,
                        Success BIT NOT NULL,
                        ErrorMessage NVARCHAR(MAX) NULL,
                        SqlHash NVARCHAR(64) NULL
                    )
                END";

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new SqlCommand(sql, connection);
            await command.ExecuteNonQueryAsync();
        }

        private async Task<List<string>> GetAppliedMigrationsAsync()
        {
            var migrations = new List<string>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = "SELECT MigrationId FROM __MigrationHistory WHERE Success = 1 ORDER BY AppliedAt";
            using var command = new SqlCommand(sql, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                migrations.Add(reader.GetString(0));
            }

            return migrations;
        }

        private List<MigrationFile> LoadEmbeddedMigrations()
        {
            var migrations = new List<MigrationFile>();
            var assembly = Assembly.GetExecutingAssembly();

            // Get all embedded resources that match the migration pattern
            // Resource names format: MillionaireGame.Database.Migrations.00005_add_session_state_tracking.sql
            var resourceNames = assembly.GetManifestResourceNames()
                .Where(r => r.Contains(".Database.Migrations.") && r.EndsWith(".sql"))
                .OrderBy(r => r)
                .ToList();

            foreach (var resourceName in resourceNames)
            {
                try
                {
                    // Extract filename from resource name
                    // Format: MillionaireGame.Database.Migrations.00005_add_session_state_tracking.sql
                    var parts = resourceName.Split('.');
                    
                    // Find the index where "Migrations" appears
                    var migrationsIndex = Array.IndexOf(parts, "Migrations");
                    if (migrationsIndex == -1 || migrationsIndex >= parts.Length - 1)
                    {
                        GameConsole.Warn($"[Migrations] Skipping invalid resource name format: {resourceName}");
                        continue;
                    }

                    // Everything after "Migrations." is the filename (may contain dots)
                    var fileNameParts = parts.Skip(migrationsIndex + 1).ToArray();
                    var fileName = string.Join(".", fileNameParts);

                    // Extract serial number from filename (first part before underscore)
                    var underscoreIndex = fileName.IndexOf('_');
                    if (underscoreIndex == -1)
                    {
                        GameConsole.Warn($"[Migrations] Skipping invalid migration filename: {fileName}");
                        continue;
                    }

                    var serialString = fileName.Substring(0, underscoreIndex);
                    if (!int.TryParse(serialString, out int serial))
                    {
                        GameConsole.Warn($"[Migrations] Skipping migration with invalid serial number: {fileName}");
                        continue;
                    }

                    // Read embedded resource content
                    string sqlContent;
                    using (var stream = assembly.GetManifestResourceStream(resourceName))
                    {
                        if (stream == null)
                        {
                            GameConsole.Warn($"[Migrations] Could not load resource: {resourceName}");
                            continue;
                        }

                        using (var reader = new StreamReader(stream))
                        {
                            sqlContent = reader.ReadToEnd();
                        }
                    }

                    migrations.Add(new MigrationFile
                    {
                        MigrationId = ExtractMigrationId(fileName),
                        FileName = fileName,
                        FullPath = resourceName, // Store resource name instead of file path
                        SerialNumber = serial,
                        SqlContent = sqlContent
                    });
                }
                catch (Exception ex)
                {
                    GameConsole.Warn($"[Migrations] Error loading migration resource {resourceName}: {ex.Message}");
                }
            }

            return migrations;
        }

        private async Task<bool> ApplyMigrationAsync(MigrationFile migration)
        {
            var startTime = DateTime.UtcNow;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            GameConsole.Info($"[Migrations] Applying: {migration.MigrationId}...");

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                // Split SQL into batches (by GO statements)
                var batches = SplitSqlBatches(migration.SqlContent);

                foreach (var batch in batches)
                {
                    if (string.IsNullOrWhiteSpace(batch)) continue;

                    using var command = new SqlCommand(batch, connection, transaction);
                    command.CommandTimeout = 300; // 5 minutes max per batch
                    await command.ExecuteNonQueryAsync();
                }

                // Record successful migration
                stopwatch.Stop();
                var recordSql = @"
                    INSERT INTO __MigrationHistory (MigrationId, FileName, AppliedAt, ExecutionTimeMs, Success, SqlHash)
                    VALUES (@MigrationId, @FileName, @AppliedAt, @ExecutionTimeMs, 1, @SqlHash)";

                using var recordCommand = new SqlCommand(recordSql, connection, transaction);
                recordCommand.Parameters.AddWithValue("@MigrationId", migration.MigrationId);
                recordCommand.Parameters.AddWithValue("@FileName", migration.FileName);
                recordCommand.Parameters.AddWithValue("@AppliedAt", startTime);
                recordCommand.Parameters.AddWithValue("@ExecutionTimeMs", (int)stopwatch.ElapsedMilliseconds);
                recordCommand.Parameters.AddWithValue("@SqlHash", ComputeSqlHash(migration.SqlContent));
                await recordCommand.ExecuteNonQueryAsync();

                transaction.Commit();

                GameConsole.Info($"[Migrations] ✓ Applied {migration.MigrationId} ({stopwatch.ElapsedMilliseconds}ms)");
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                stopwatch.Stop();

                GameConsole.Error($"[Migrations] ✗ Failed {migration.MigrationId}: {ex.Message}");

                // Record failed migration (outside transaction)
                try
                {
                    using var failConnection = new SqlConnection(_connectionString);
                    await failConnection.OpenAsync();

                    var recordSql = @"
                        INSERT INTO __MigrationHistory (MigrationId, FileName, AppliedAt, ExecutionTimeMs, Success, ErrorMessage)
                        VALUES (@MigrationId, @FileName, @AppliedAt, @ExecutionTimeMs, 0, @ErrorMessage)";

                    using var recordCommand = new SqlCommand(recordSql, failConnection);
                    recordCommand.Parameters.AddWithValue("@MigrationId", migration.MigrationId);
                    recordCommand.Parameters.AddWithValue("@FileName", migration.FileName);
                    recordCommand.Parameters.AddWithValue("@AppliedAt", startTime);
                    recordCommand.Parameters.AddWithValue("@ExecutionTimeMs", (int)stopwatch.ElapsedMilliseconds);
                    recordCommand.Parameters.AddWithValue("@ErrorMessage", ex.ToString());
                    await recordCommand.ExecuteNonQueryAsync();
                }
                catch
                {
                    // Ignore errors recording failure
                }

                return false;
            }
        }

        private string[] SplitSqlBatches(string sql)
        {
            // Split on GO statements (must be on its own line)
            return sql.Split(new[] { "\r\nGO\r\n", "\nGO\n", "\r\nGO\n", "\nGO\r\n" },
                           StringSplitOptions.RemoveEmptyEntries);
        }

        private string ExtractMigrationId(string fileName)
        {
            // Remove .sql extension
            return Path.GetFileNameWithoutExtension(fileName);
        }

        private string ComputeSqlHash(string sql)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(sql);
            var hash = sha256.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }

    /// <summary>
    /// Represents a migration file with metadata
    /// </summary>
    public class MigrationFile
    {
        public string MigrationId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public int SerialNumber { get; set; }
        public string SqlContent { get; set; } = string.Empty;
    }

    /// <summary>
    /// Status information for a migration
    /// </summary>
    public class MigrationStatus
    {
        public string MigrationId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public bool IsApplied { get; set; }
        public DateTime? AppliedAt { get; set; }
        public bool? Success { get; set; }
    }
}
