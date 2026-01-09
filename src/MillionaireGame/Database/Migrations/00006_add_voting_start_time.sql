-- ============================================================================
-- Migration: 00006_add_voting_start_time
-- Description: Add VotingStartTime column to Sessions table to track when
--              ATA voting actually begins (separate from question display time).
--              Fixes bug where intro time was counted against voting timeout.
-- Author: System
-- Date: 2026-01-09
-- Dependencies: Requires 00003_create_waps_tables (Sessions table must exist)
-- ============================================================================

-- Add VotingStartTime column for tracking actual voting start (not question display)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Sessions' AND COLUMN_NAME = 'VotingStartTime')
BEGIN
    ALTER TABLE Sessions 
    ADD VotingStartTime DATETIME2 NULL
    
    PRINT 'Added VotingStartTime column to Sessions table'
END
ELSE
BEGIN
    PRINT 'VotingStartTime column already exists'
END
GO

-- Update performance index to include VotingStartTime
IF EXISTS (SELECT * FROM sys.indexes 
           WHERE name = 'IX_Sessions_CurrentMode' AND object_id = OBJECT_ID('Sessions'))
BEGIN
    DROP INDEX IX_Sessions_CurrentMode ON Sessions
    PRINT 'Dropped old IX_Sessions_CurrentMode index'
END
GO

-- Recreate index with VotingStartTime included
CREATE INDEX IX_Sessions_CurrentMode 
ON Sessions(CurrentMode, QuestionStartTime, VotingStartTime)
WHERE CurrentMode IS NOT NULL

PRINT 'Created updated IX_Sessions_CurrentMode index with VotingStartTime'
GO

PRINT 'Migration 00006_add_voting_start_time completed successfully'
GO
