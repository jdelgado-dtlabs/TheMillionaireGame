# Database Migration System - Implementation Plan

**Status:** 📋 Planning Phase  
**Source Branch:** `feature/web-state-sync` (commit: TBD)  
**Target Branch:** `feature/database-migrations` (new branch from feature/web-state-sync)  
**Merge Back To:** `feature/web-state-sync` (IMPORTANT: Must merge back after completion)  
**Date:** January 8, 2026  
**Complexity:** Medium  
**Estimated Time:** 4-6 hours

## Branch Strategy

⚠️ **IMPORTANT:** This migration system will be implemented in a separate branch but MUST be merged back to `feature/web-state-sync` before that feature can be completed and tested. The web state sync feature depends on the migration system to properly create the required database columns.

**Workflow:**
1. Create `feature/database-migrations` from current `feature/web-state-sync` HEAD
2. Implement migration system in new branch
3. Test migration system independently
4. Merge `feature/database-migrations` → `feature/web-state-sync`
5. Remove temporary SQL code from WebServerHost.cs in web-state-sync
6. Complete web state sync testing
7. Merge `feature/web-state-sync` → `master-v1.0.5`

## Executive Summary

Implement a sequential, file-based database migration system for `dbMillionaire` that runs automatically on application startup. Migrations are stored as SQL files with sequential numbering, tracked in a history table, and executed in order. This replaces the current ad-hoc schema update approach with a reliable, auditable system.

---

## Architecture Overview

### System Design Principles

1. **Single Entry Point**: Run migrations once at app startup before any database access
2. **Sequential Execution**: Files run in alphanumeric order based on 5-digit prefix
3. **Idempotent**: Safe to run multiple times - already applied migrations are skipped
4. **Atomic**: Each migration runs in a transaction with automatic rollback on error
5. **Logged**: All activity logged to GameConsole for visibility
6. **Thread-Safe**: Concurrent runs (if web server starts early) won't conflict

### File Structure

```
TheMillionaireGame/
└── src/
    └── MillionaireGame/
        └── Database/
            ├── MigrationRunner.cs                    # NEW CLASS
            ├── MigrationRecord.cs                    # NEW CLASS (model)
            └── Migrations/                           # NEW FOLDER (embedded resources)
                ├── 00001_initial_telemetry_schema.sql
                ├── 00002_add_game_session_tracking.sql
                ├── 00003_create_waps_tables.sql
                ├── 00004_add_question_start_time.sql
                ├── 00005_add_session_state_tracking.sql
                └── README.md
```

**Note:** Migration SQL files are embedded as resources in the assembly (not exposed to users).

### Migration Tracking Table

```sql
CREATE TABLE __MigrationHistory (
    MigrationId NVARCHAR(50) PRIMARY KEY,        -- e.g., "00005_add_session_state_tracking"
    FileName NVARCHAR(255) NOT NULL,             -- Full filename
    AppliedAt DATETIME NOT NULL,                  -- Timestamp of application
    ExecutionTimeMs INT NOT NULL,                 -- How long it took
    Success BIT NOT NULL,                         -- 1 = success, 0 = failed
    ErrorMessage NVARCHAR(MAX) NULL,              -- Error details if failed
    SqlHash NVARCHAR(64) NULL                     -- SHA256 of SQL content (future: detect changes)
)
```

**Key Fields:**
- `MigrationId`: Serial number + name (without .sql)
- `Success`: Allows tracking failed migrations without blocking future attempts
- `SqlHash`: Future enhancement - detect if migration file was modified after application

---

## Component Design

### 1. MigrationRunner Class

**Location:** `src/MillionaireGame/Database/MigrationRunner.cs`  
**Namespace:** `MillionaireGame.Database`

**Responsibilities:**
- Scan migrations folder for .sql files
- Check __MigrationHistory for applied migrations
- Execute pending migrations in sequence
- Log all activity
- Handle errors gracefully

**Public Interface:**
```csharp
public class MigrationRunner
{
    private readonly string _connectionString;
    private readonly string _migrationsPath;
    
    public MigrationRunner(string connectionString, string migrationsPath);
    
    /// <summary>
    /// Run all pending migrations. Returns true if all succeeded or were already applied.
    /// </summary>
    public async Task<bool> RunMigrationsAsync();
    
    /// <summary>
    /// Get list of all migrations and their status
    /// </summary>
    public async Task<List<MigrationStatus>> GetMigrationStatusAsync();
}

public class MigrationStatus
{
    public string MigrationId { get; set; }
    public string FileName { get; set; }
    public bool IsApplied { get; set; }
    public DateTime? AppliedAt { get; set; }
    public bool? Success { get; set; }
}
```

**Key Methods (Private):**
```csharp
private async Task EnsureMigrationTableExistsAsync();
private async Task<List<string>> GetAppliedMigrationsAsync();
private List<MigrationFile> ScanMigrationFiles();
private async Task<bool> ApplyMigrationAsync(MigrationFile migration);
private string[] SplitSqlBatches(string sql);  // Handle GO statements
private string ExtractMigrationId(string fileName);
private string ComputeSqlHash(string sql);
```

### 2. MigrationFile Class

**Purpose:** Represents a migration file with metadata

```csharp
public class MigrationFile
{
    public string MigrationId { get; set; }      // "00005_add_session_state_tracking"
    public string FileName { get; set; }          // "00005_add_session_state_tracking.sql"
    public string FullPath { get; set; }          // Absolute path to file
    public int SerialNumber { get; set; }         // 5 for sorting
    public string SqlContent { get; set; }        // File content
}
```

### 3. Migration File Format

**Standard Template:**
```sql
-- ============================================================================
-- Migration: 00005_add_session_state_tracking
-- Description: Add CurrentQuestionText and CurrentQuestionOptionsJson columns
--              to Sessions table for state sync support
-- Author: Developer Name
-- Date: 2026-01-08
-- Dependencies: Requires 00003_create_waps_tables
-- ============================================================================

-- Add columns for question state tracking
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Sessions' AND COLUMN_NAME = 'CurrentQuestionText')
BEGIN
    ALTER TABLE Sessions 
    ADD CurrentQuestionText NVARCHAR(500) NULL
END
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Sessions' AND COLUMN_NAME = 'CurrentQuestionOptionsJson')
BEGIN
    ALTER TABLE Sessions 
    ADD CurrentQuestionOptionsJson NVARCHAR(MAX) NULL
END
GO

-- Add index for performance (optional)
IF NOT EXISTS (SELECT * FROM sys.indexes 
               WHERE name = 'IX_Sessions_QuestionStartTime' AND object_id = OBJECT_ID('Sessions'))
BEGIN
    CREATE INDEX IX_Sessions_QuestionStartTime 
    ON Sessions(QuestionStartTime) 
    WHERE QuestionStartTime IS NOT NULL
END
GO
```

**Format Rules:**
1. **Filename:** `NNNNN_descriptive_name.sql` (5 digits, underscore separator, lowercase)
2. **Header:** Comment block with metadata (migration ID, description, author, date)
3. **Idempotent Checks:** Always use `IF NOT EXISTS` or `IF EXISTS` checks
4. **Batch Separators:** Use `GO` to separate batches (MigrationRunner will split and execute separately)
5. **Transactions:** MigrationRunner wraps entire migration in transaction (auto-rollback on error)

**Idempotent Patterns:**
```sql
-- Add table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'MyTable')
BEGIN
    CREATE TABLE MyTable (...)
END
GO

-- Add column
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'MyTable' AND COLUMN_NAME = 'MyColumn')
BEGIN
    ALTER TABLE MyTable ADD MyColumn INT NULL
END
GO

-- Add index
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MyTable_MyColumn')
BEGIN
    CREATE INDEX IX_MyTable_MyColumn ON MyTable(MyColumn)
END
GO

-- Add constraint
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
               WHERE CONSTRAINT_NAME = 'FK_MyTable_OtherTable')
BEGIN
    ALTER TABLE MyTable 
    ADD CONSTRAINT FK_MyTable_OtherTable FOREIGN KEY (OtherId) REFERENCES OtherTable(Id)
END
GO
```

---

## Implementation Steps

### Phase 1: Core Infrastructure (2-3 hours)

#### Step 1.1: Create MigrationRunner Class
**File:** `src/MillionaireGame/Database/MigrationRunner.cs`

**Implementation Details:**
```csharp
using System;
using System.Collections.Generic;
using System.Data;
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
    public class MigrationRunner
    {
        private readonly string _connectionString;
        
        public MigrationRunner(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }
        
        public async Task<bool> RunMigrationsAsync()
        {
            try
            {
                GameConsole.Info("[Migrations] Starting database migration check...");
                
                // 1. Ensure migrations folder exists
                if (!Director__MigrationHistory table exists
                await EnsureMigrationTableExistsAsync();
                
                // 2. Get list of already applied migrations
                var appliedMigrations = await GetAppliedMigrationsAsync();
                GameConsole.Debug($"[Migrations] Found {appliedMigrations.Count} applied migrations");
                
                // 3. Load embedded migration resources
                var migrationFiles = LoadEmbeddedMigrations();
                GameConsole.Debug($"[Migrations] Found {migrationFiles.Count} embedded migration(s)");
                
                // 4pendingMigrations = migrationFiles
                    .Where(m => !appliedMigrations.Contains(m.MigrationId))
                    .OrderBy(m => m.SerialNumber)
                    .ToList();
                
                if (pendingMigrations.Count == 0)
                {
                    GameConsole.Info("[Migrations] Database schema is up to date");
                    return true;
                }
                
                GameConsole.Info($"[Migrations] Found {pendingMigrations.Count} pending migration(s)");
                
                // 6. Apply each pending migration
                int successCount = 0;
                foreach (var migration in pendingMigrations)
                {
                    if (await ApplyMigrationAsync(migration))
                    {
                        successCount++;
                   5}
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
            // Resource names format: MillionaireGame.Database.Migrations.00001_description.sql
            var resourceNames = assembly.GetManifestResourceNames()
                .Where(r => r.Contains(".Database.Migrations.") && r.EndsWith(".sql"))
                .OrderBy(r => r)
                .ToList();
            
            foreach (var resourceName in resourceNames)
            {
                // Extract filename from resource name
                var parts = resourceName.Split('.');
                var fileName = parts[parts.Length - 2] + "_" + parts[parts.Length - 1].Replace("_sql", ".sql");
                
                // Handle actual format: last two parts before .sql
                var fileNameParts = resourceName.Replace(".sql", "").Split('.');
                var serialPart = fileNameParts[fileNameParts.Length - 2];
                var descPart = fileNameParts[fileNameParts.Length - 1];
                fileName = $"{serialPart}_{descPart}.sql";
                
                // Extract serial number (first 5 digits)
                if (!int.TryParse(serialPart, out int serial))
                {
                    GameConsole.Warn($"[Migrations] Skipping invalid migration resource: {fileName}");
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
            using var sha2 (reads from embedded resources)
try
{
    var migrationRunner = new MigrationRunner(connectionString
    }
    
    public class MigrationFile
    {
        public string MigrationId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public int SerialNumber { get; set; }
        public string SqlContent { get; set; } = string.Empty;
    }
}
```

#### Step 1.2: Integrate into Application Startup

**File:** `src/MillionaireGame/Program.cs`

**Location:** Insert right after database connection is established, before any forms are created

```csharp
// Run database migrations
try
{
    var migrationsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lib", "sql", "migrations");
    var migrationRunner = new MigrationRunner(connectionString, migrationsPath);
    var migrationSuccess = await migrationRunner.RunMigrationsAsync();
    
    if (!migrationSuccess)
    {
        GameConsole.Warn("[Startup] Database migrations encountered errors - proceeding with caution");
   # Step 1.4: Configure Project for Embedded Resources

**File:** `src/MillionaireGame/MillionaireGame.csproj`

**Add to .csproj:**
```xml
<ItemGroup>
  <!-- Embed all SQL migration files as resources -->
  <Embeddedsrc/MillionaireGame/Database/Migrations/00002_add_game_session_tracking.sql`

Document the GameSession and related telemetry tracking tables.

#### Migration 00003: WAPS Tables
**File:** `src/MillionaireGame/Database/Migrations/00003_create_waps_tables.sql`

Create Sessions, Participants, FFFAnswers, ATAVotes, ParticipantHistory tables.

#### Migration 00004: Question Start Time
**File:** `src/MillionaireGame/Database/Migrations/00004_add_question_start_time.sql`

Add QuestionStartTime column to Sessions table (existing migration).

#### Migration 00005: Session State Tracking
**File:** `src/MillionaireGame/Database/May - app might still work with existing schema
}
```

#### Step 1.3: Remove Old Web Server Schema Update Code

**File:** `src/MillionaireGame/Hosting/WebServerHost.cs`

**Action:** Remove the direct SQL schema update code and replace with simpler call:

```csharp
// Old code to remove (lines ~172-194):
// The IF NOT EXISTS checks for CurrentQuestionText and CurrentQuestionOptionsJson

// Replace with:
WebServerConsole.Info("[WebServer] Database migrations are handled by main app startup");
// Schema should already be updated by the time web server starts
```

### Phase 2: Create Initial Migrations (1-2 hours)

#### Migration 00001: Initial Telemetry Schema
**File:** `lib/sql/migrations/00001_initial_telemetry_schema.sql`

Document the existing telemetry tables that were created earlier in the project lifecycle.

#### Migration 00002: Game Session Tracking
**File:** `lib/sql/migrations/00002_add_game_session_tracking.sql`

Document the GameSession and related telemetry tracking tables.

#### Migration 00003: WAPS Tables
**File:** `lib/sql/migrations/00003_create_waps_tables.sql`

Create Sesssrc/MillionaireGame/Database/Migrations/README.md`

```markdown
# Database Migrations

This folder contains SQL migration files that are **embedded as resources** and automatically applied on application startup.

**Important:** These files are compiled into the application assembly and are NOT visible to end users

#### Migration 00005: Session State Tracking
**File:** `lib/sql/migrations/00005_add_session_state_tracking.sql`

Add CurrentQuestionText and CurrentQuestionOptionsJson columns (current feature).

**Note:** Migrations 00001-00004 are "retroactive" - they document changes already made. Since these tables already exist in production databases, the migrations MUST use `IF NOT EXISTS` checks to be idempotent.

### Phase 3: Documentation (30 minutes)

#### Create Migration Guide
**File:** `lib/sql/migrations/README.md`

```markdown
# Database Migrations

This folder contains SQL migration files that are automatically applied on application startup.

## File Naming Convention

Format: `NNNNN_descriptive_name.sql`
- NNNNN: 5-digit serial number (00001, 00002, etc.)
- descriptive_name: Lowercase with underscores
- Must have .sql extension

Example: `00005_add_session_state_tracking.sql`

## How Migrations Work

1. Application starts
2. MigrationRunner scans this folder for .sql files
3. Checks __MigrationHistory table for applied migrations
4. Runs pending migrations in serial number order
5. Each migration runs in a transaction (auto-rollback on error)
6. Success/failure recorded in __MigrationHistory

## Creating New Migrations

1. Find the highest numbered migration file
2. Create new file with next number
3. Use provided template (see below)
4. Make migration idempotent (use IF NOT EXISTS checks)
5. Test locally before committing
6. Document in CHANGELOG

## Migration Template

```sql
-- ============================================================================
-- Migration: NNNNN_descriptive_name
-- Description: What this migration does
-- Author: Your Name
-- Date: YYYY-MM-DD
-- Dependencies: (optional) Requires NNNNN_other_migration
-- ============================================================================

-- Your SQL here
-- Always use IF NOT EXISTS/IF EXISTS checks!

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'MyTable')
BEGIN
    CREATE TABLE MyTable (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(100) NOT NULL
    )
END
GO
```

## Best Practices

- ✅ Always use IF NOT EXISTS checks
- ✅ Use GO to separate batches
- ✅ Test on a copy of production database first
- ✅ Keep migrations small and focused
- ✅ Never modify a migration after it's been released
- ❌ Don't use DROP TABLE unless absolutely necessary
- ❌ Don't alter column types on production data without backups
- ❌ Don't assume data exists - check before updating

## Troubleshooting

- **Migration failed**: Check GameConsole for error message
- **Migration marked as failed**: Fix the SQL and it will retry next run
- **Need to re-run migration**: DELETE FROM __MigrationHistory WHERE MigrationId = 'NNNNN_name'
- **Skip migration**: Mark as applied: INSERT INTO __MigrationHistory ... (see schema)
```

### Phase 4: Testing (1 hour)

#### Test Scenarios

1. **Fresh Database**: Run app with empty database, verify all migrations apply
2. **Existing Database**: Run app with production copy, verify idempotent checks work
3. **Failed Migration**: Introduce SQL error, verify rollback and error recording
4. **Partial Application**: Stop app mid-migration, restart, verify continues correctly
5. **Duplicate Run**: Run migrations twice rapidly, verify no duplicate applications

#### Test Checklist

- [ ] __MigrationHistory table created automatically
- [ ] All pending migrations apply in order
- [ ] Already-applied migration (reads from embedded resources)
try
{
    var migrationRunner = new MigrationRunner(connectionString
- [ ] Idempotent checks prevent duplicate table/column creation

---

## Integration Points

### Main Application Startup
**File:** `src/MillionaireGame/Program.cs`

**Location:** After database connection, before creating any forms

```csharp
// Around line 50-60 in Main() method
var connectionString = ConnectionHelper.GetConnectionString();

// NEW: Run database migrations
try
{
    var migrationsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lib", "sql", "migrations");
    var migrationRunner = new MigrationRunner(connectionString, migrationsPath);
    await migrationRunner.RunMigrationsAsync();
}
catch (Exception ex)
{
    GameConsole.Error($"[Startup] Migration error: {ex.Message}");
}

// Continue with application initialization...
```

### Web Server Startup
**File:** `src/MillionaireGame/Hosting/WebServerHost.cs`

**Action:** Remove the direct SQL schema update code (lines ~172-194)

**Replace with:**
```csharp
// Database migrations are handled by main app startup
// No schema updates needed here
```

---

## Migration Files to Create

### Priority Migrations (for this feature)

**00005_add_session_state_tracking.sql** (HIGHEST PRIORITY)
```sql
-- ============================================================================
-- Migration: 00005_add_session_state_tracking
-- Description: Add columns to Sessions table for web state synchronization
--              CurrentQuestionText stores the active question text
--              CurrentQuestionOptionsJson stores answer options as JSON
-- Author: System
-- Date: 2026-01-08
-- Dependencies: Requires 00003_create_waps_tables
-- ============================================================================

-- Add CurrentQuestionText column
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Sessions' AND COLUMN_NAME = 'CurrentQuestionText')
BEGIN
    ALTER TABLE Sessions 
    ADD CurrentQuestionText NVARCHAR(500) NULL
    
    PRINT 'Added CurrentQuestionText column to Sessions table'
END
GO

-- Add CurrentQuestionOptionsJson column
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Sessions' AND COLUMN_NAME = 'CurrentQuestionOptionsJson')
BEGIN
    ALTER TABLE Sessions 
    ADD CurrentQuestionOptionsJson NVARCHAR(MAX) NULL
    
    PRINT 'Added CurrentQuestionOptionsJson column to Sessions table'
END
GO

-- Add index for performance (queries filter by CurrentMode)
IF NOT EXISTS (SELECT * FROM sys.indexes 
               WHERE name = 'IX_Sessions_CurrentMode' AND object_id = OBJECT_ID('Sessions'))
BEGIN
    CREATE INDEX IX_Sessions_CurrentMode 
    ON Sessions(CurrentMode, QuestionStartTime)
    WHERE CurrentMode IS NOT NULL
    
    PRINT 'Added performance index IX_Sessions_CurrentMode'
END
GO
```

### Retroactive Migrations (document existing schema)

These migrations document tables that already exist. They use IF NOT EXISTS checks to avoid errors on existing databases.

**00001_initial_telemetry_schema.sql**
**00002_add_game_session_tracking.sql**
**00003_create_waps_tables.sql**
**00004_add_question_start_time.sql**

(Content to be extracted from existing database schema and CREATE TABLE scripts)

---

## Rollout Strategy

### Development Branch Workflow

1. Create new branch: `feature/database-migrations`
2. Implement MigrationRunner class
3. Create migration files
4. Test locally with fresh and existing databases
5. Commit and push
6. Merge to master-v1.0.5

### Merging with feature/web-state-sync

**Option A: Merge migrations first, then web-state-sync**
1. Merge feature/database-migrations → master-v1.0.5
2. Rebase feature/web-state-sync on updated master-v1.0.5
3. Remove manual schema update code from web-state-sync
4. Migration 00005 is already in place

**Option B: Include migration 00005 in web-state-sync** (RECOMMENDED)
1. Finish web-state-sync feature first
2. Create feature/database-migrations from master-v1.0.5
3. Implement migration system
4. Create 00001-00004 retroactive migrations
5. When merging web-state-sync, add migration 00005
6. Merge both branches together

### User Impact

- **Existing Users**: First startup after update will apply pending migrations (2-5 seconds)
- **New Users**: All migrations run automatically on first startup
- **Failed Migrations**: App continues but logs errors (graceful degradation)
- **No User Action Required**: Completely automatic

---

## Future Enhancements

### Phase 2 Features (Post-MVP)

1. **Migration Verification**
   - Check SqlHash to detect modified migrations
   - Warn if applied migration was changed
   
2. **Rollback Support**
   - Add `Down` migration files for reversing changes
   - `NNNNN_up.sql` and `NNNNN_down.sql` pairs
   
3. **Data Migrations**
   - Support for complex data transformations
   - C# code-based migrations for logic that can't be done in SQL
   
4. **Migration UI**
   - Admin panel showing migration status
   - Manual migration control (apply/rollback specific migrations)
   
5. **Pre-flight Checks**
   - Validate migrations before applying (dry-run mode)
   - Check for common issues (missing indexes, orphaned data)

---

## Risks and Mitigations

### Risk 1: Migration Failure on User Systems
**Mitigation:** 
- Extensive idempotent checks (IF NOT EXISTS)
- Comprehensive testing on various database states
- Transaction-based rollback on error
- Graceful degradation (app continues if migration fails)
### Risk 5: User Tampering with Migration Files
**Mitigation:**
- ✅ SQL files embedded as resources (not accessible to users)
- Files compiled into assembly (cannot be modified after build)
- SqlHash verification can detect if embedded content changes between versions


### Risk 2: Long-Running Migrations
**Mitigation:**
- Keep migrations small and focused
- Avoid full table scans on large tables
- Use batched updates for data migrations
- Set 5-minute timeout per batch

### Risk 3: Concurrent Migration Attempts
**Mitigation:**
- Database-level locking (transaction on __MigrationHistory)
- Quick check at start - second runner sees migrations already applied

### Risk 4: Breaking Changes
**Mitigation:**
- Never DROP columns (mark as deprecated instead)
- Keep backward compatibility for 1-2 versions
- Announce breaking changes in release notes

---

## Success Criteria

✅ **Phase 1 Complete When:**
- MigrationRunner class implemented and tested
- Integrated into Program.cs startup
- __MigrationHistory table created automatically
- At least one test migration runs successfully
- Error handling and logging work correctly

✅ **Phase 2 Complete When:**
- Migration 00005 (session state tracking) created and tested
- Retroactive migrations (00001-00004) created
- All migrations run successfully on fresh database
- All migrations skip correctly on existing database

✅ **Phase 3 Complete When:**
- README.md with migration guide created
- Code comments explain migration process
- CHANGELOG updated with migration system description

✅ **System Ready for Production When:**
- All tests pass (fresh DB, existing DB, failure scenarios)
- Web server no longer does direct schema updates
- Migration logs are clear and actionable
- Feature branch merged to master

---

## Timeline

**Estimated Total: 4-6 hours**

| Phase | Task | Time | Priority |
|-------|------|------|----------|
| 1 | Design and document plan | 1h | ✅ DONE |
| 2 | Implement MigrationRunner | 2h | HIGH |
| 3 | Create migration 00005 | 30min | HIGH |
| 4 | Create retroactive migrations | 1h | MEDIUM |
| 5 | Integration and testing | 1h | HIGH |
| 6 | Documentation | 30min | MEDIUM |

**Total: 5 hours**

---

## Notes for Implementation

1. **Start Fresh**: Create `feature/database-migrations` from clean `master-v1.0.5`
2. **Test First**: Write tests before creating retroactive migrations
3. **One at a Time**: Implement, test, commit each migration file separately
4. **Log Everything**: Use GameConsole for all migration activity
5. **Handle Errors Gracefully**: Don't crash app if migration fails
6. **Keep It Simple**: MVP first, enhancements later

---YES - protects from user tampering

## Questions to Address Before Implementation

1. ✅ Should migrations support C# code or SQL only? → **SQL only for MVP**
2. ✅ Should failed migrations block app startup? → **No, log and continue**
3. ✅ Should we support migration rollback? → **Not in MVP**
4. ✅ Should migration files be embedded resources? → **No, filesystem is simpler**
5. ✅ Should we validate SQL syntax before running? → **No, let database handle it**

---

**Status:** Ready for implementation  
**Next Step:** Create feature branch and implement Phase 1
