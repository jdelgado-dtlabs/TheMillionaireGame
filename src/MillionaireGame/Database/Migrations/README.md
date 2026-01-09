# Database Migrations

This folder contains SQL migration files that are **embedded as resources** and automatically applied on application startup.

**Important:** These files are compiled into the application assembly and are NOT visible to end users.

## How It Works

1. Application starts
2. MigrationRunner scans for embedded SQL migration resources
3. Checks `__MigrationHistory` table for applied migrations
4. Runs pending migrations in serial number order
5. Each migration runs in a transaction (auto-rollback on error)
6. Success/failure recorded in `__MigrationHistory`

## File Naming Convention

Format: `NNNNN_descriptive_name.sql`
- NNNNN: 5-digit serial number (00001, 00002, etc.)
- descriptive_name: Lowercase with underscores
- Must have .sql extension

Example: `00005_add_session_state_tracking.sql`

## Creating New Migrations

1. Find the highest numbered migration file
2. Create new file with next number
3. Use provided template (see below)
4. Make migration idempotent (use IF NOT EXISTS checks)
5. Test locally before committing
6. Files are automatically embedded as resources during build

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

-- Use PRINT for feedback visible in logs
PRINT 'Created MyTable table'
GO
```

## Best Practices

- ✅ Always use IF NOT EXISTS checks (idempotent)
- ✅ Use GO to separate batches
- ✅ Use PRINT statements for progress feedback
- ✅ Test on a copy of production database first
- ✅ Keep migrations small and focused
- ✅ Never modify a migration after it's been released
- ❌ Don't use DROP TABLE unless absolutely necessary
- ❌ Don't alter column types on production data without backups
- ❌ Don't assume data exists - check before updating

## Troubleshooting

- **Migration failed**: Check GameConsole for error message
- **Migration marked as failed**: Fix the SQL and it will retry next run
- **Need to re-run migration**: `DELETE FROM __MigrationHistory WHERE MigrationId = 'NNNNN_name'`
- **Skip migration**: Mark as applied: `INSERT INTO __MigrationHistory (MigrationId, FileName, AppliedAt, ExecutionTimeMs, Success) VALUES ('NNNNN_name', 'NNNNN_name.sql', GETUTCDATE(), 0, 1)`

## Viewing Migration Status

Check the `__MigrationHistory` table:
```sql
SELECT * FROM __MigrationHistory ORDER BY AppliedAt DESC
```

Columns:
- `MigrationId`: Unique migration identifier (00005_add_session_state_tracking)
- `FileName`: SQL file name
- `AppliedAt`: UTC timestamp when migration was applied
- `ExecutionTimeMs`: How long the migration took to run
- `Success`: 1 = succeeded, 0 = failed
- `ErrorMessage`: Error details if failed (NULL if succeeded)
- `SqlHash`: SHA256 hash of SQL content for integrity verification

## Current Migrations

### 00005_add_session_state_tracking.sql
- **Added:** 2026-01-08
- **Purpose:** Add CurrentQuestionText and CurrentQuestionOptionsJson columns to Sessions table
- **Dependency:** Requires Sessions table from WAPS setup
- **Use Case:** Enables web state synchronization for reconnecting clients

## Security

Migration SQL files are embedded into the application assembly during compilation. This means:
- Users cannot view the SQL source code
- Files cannot be modified after build
- No external SQL files are distributed
- Protected from tampering

The migration system uses parameterized queries and transactions to ensure database integrity.
