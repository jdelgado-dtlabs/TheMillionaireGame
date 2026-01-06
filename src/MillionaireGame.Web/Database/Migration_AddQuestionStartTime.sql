-- Migration: Add QuestionStartTime to Sessions table
-- Date: 2026-01-06
-- Purpose: Track when FFF/ATA questions start to accurately calculate response times

-- Add QuestionStartTime column to Sessions table
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID(N'[dbo].[Sessions]') 
               AND name = 'QuestionStartTime')
BEGIN
    ALTER TABLE [dbo].[Sessions]
    ADD [QuestionStartTime] DATETIME2 NULL;
    
    PRINT 'Added QuestionStartTime column to Sessions table';
END
ELSE
BEGIN
    PRINT 'QuestionStartTime column already exists in Sessions table';
END
GO
