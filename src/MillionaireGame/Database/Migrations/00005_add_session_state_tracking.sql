-- ============================================================================
-- Migration: 00005_add_session_state_tracking
-- Description: Add CurrentQuestionText and CurrentQuestionOptionsJson columns
--              to Sessions table for web state synchronization support.
--              Enables reconnecting clients to see current question state.
-- Author: System
-- Date: 2026-01-08
-- Dependencies: Requires 00003_create_waps_tables (Sessions table must exist)
-- ============================================================================

-- Add CurrentQuestionText column for storing active question text
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Sessions' AND COLUMN_NAME = 'CurrentQuestionText')
BEGIN
    ALTER TABLE Sessions 
    ADD CurrentQuestionText NVARCHAR(500) NULL
    
    PRINT 'Added CurrentQuestionText column to Sessions table'
END
ELSE
BEGIN
    PRINT 'CurrentQuestionText column already exists'
END
GO

-- Add CurrentQuestionOptionsJson column for storing answer options as JSON
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Sessions' AND COLUMN_NAME = 'CurrentQuestionOptionsJson')
BEGIN
    ALTER TABLE Sessions 
    ADD CurrentQuestionOptionsJson NVARCHAR(MAX) NULL
    
    PRINT 'Added CurrentQuestionOptionsJson column to Sessions table'
END
ELSE
BEGIN
    PRINT 'CurrentQuestionOptionsJson column already exists'
END
GO

-- Add performance index for queries that filter by CurrentMode
-- (helps when web server queries for active session state)
IF NOT EXISTS (SELECT * FROM sys.indexes 
               WHERE name = 'IX_Sessions_CurrentMode' AND object_id = OBJECT_ID('Sessions'))
BEGIN
    CREATE INDEX IX_Sessions_CurrentMode 
    ON Sessions(CurrentMode, QuestionStartTime)
    WHERE CurrentMode IS NOT NULL
    
    PRINT 'Added performance index IX_Sessions_CurrentMode'
END
ELSE
BEGIN
    PRINT 'Performance index IX_Sessions_CurrentMode already exists'
END
GO
