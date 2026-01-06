-- =============================================
-- Telemetry Database Tables
-- Version: v1.0.1
-- Description: Creates telemetry tracking tables and updates existing WAPS tables
-- =============================================

-- Table 1: GameSessions
-- Stores high-level game session information
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'GameSessions')
BEGIN
    CREATE TABLE GameSessions (
        SessionId NVARCHAR(50) PRIMARY KEY,           -- GUID identifier
        GameStartTime DATETIME NOT NULL,              -- When Host Intro was clicked
        GameEndTime DATETIME NULL,                    -- When CompleteClosing() finished (NULL = incomplete)
        Currency1Name NVARCHAR(5) NULL,               -- e.g., "$", "€" (MaxLength matches UI constraint)
        Currency2Name NVARCHAR(5) NULL,               -- Second currency if enabled (NULL = not enabled)
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME NOT NULL DEFAULT GETDATE()
    );

    CREATE INDEX IX_GameSessions_StartTime ON GameSessions(GameStartTime DESC);
    CREATE INDEX IX_GameSessions_EndTime ON GameSessions(GameEndTime);
    
    PRINT 'Created table: GameSessions';
END
ELSE
BEGIN
    PRINT 'Table already exists: GameSessions';
END
GO

-- Table 2: GameRounds
-- Stores individual round data, linked to game sessions
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'GameRounds')
BEGIN
    CREATE TABLE GameRounds (
        RoundId INT IDENTITY(1,1) PRIMARY KEY,
        SessionId NVARCHAR(50) NOT NULL,              -- FK to GameSessions
        RoundNumber INT NOT NULL,                     -- Round number within session
        StartTime DATETIME NOT NULL,                  -- Round start time
        EndTime DATETIME NULL,                        -- Round end time
        Outcome INT NULL,                             -- 1=Won, 2=Lost, 3=Walked Away, 4=Interrupted
        FinalQuestionReached INT NOT NULL DEFAULT 0,  -- Highest question reached
        Currency1Winnings INT NOT NULL DEFAULT 0,     -- Amount won in currency 1
        Currency2Winnings INT NOT NULL DEFAULT 0,     -- Amount won in currency 2
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        
        CONSTRAINT FK_GameRounds_SessionId FOREIGN KEY (SessionId) 
            REFERENCES GameSessions(SessionId) ON DELETE CASCADE
    );

    CREATE INDEX IX_GameRounds_SessionId ON GameRounds(SessionId);
    CREATE INDEX IX_GameRounds_RoundNumber ON GameRounds(SessionId, RoundNumber);
    
    PRINT 'Created table: GameRounds';
END
ELSE
BEGIN
    PRINT 'Table already exists: GameRounds';
END
GO

-- Table 3: LifelineUsages
-- Stores lifeline usage events
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LifelineUsages')
BEGIN
    CREATE TABLE LifelineUsages (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        GameSessionId NVARCHAR(50) NOT NULL,          -- FK to GameSessions
        RoundId INT NOT NULL,                         -- FK to GameRounds
        LifelineType INT NOT NULL,                    -- Maps to LifelineType enum (1-6)
        QuestionNumber INT NOT NULL,                  -- Question number where used
        Metadata NVARCHAR(200) NULL,                  -- e.g., "Online", "Offline"
        
        CONSTRAINT FK_LifelineUsages_GameSessionId FOREIGN KEY (GameSessionId) 
            REFERENCES GameSessions(SessionId) ON DELETE CASCADE,
        CONSTRAINT FK_LifelineUsages_RoundId FOREIGN KEY (RoundId) 
            REFERENCES GameRounds(RoundId) ON DELETE NO ACTION
    );

    CREATE INDEX IX_LifelineUsages_GameSessionId ON LifelineUsages(GameSessionId);
    CREATE INDEX IX_LifelineUsages_RoundId ON LifelineUsages(RoundId);
    
    PRINT 'Created table: LifelineUsages';
END
ELSE
BEGIN
    PRINT 'Table already exists: LifelineUsages';
END
GO

-- Update existing WAPS tables with GameSessionId foreign key

-- Add GameSessionId to Participants table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Participants') AND name = 'GameSessionId')
BEGIN
    ALTER TABLE Participants ADD GameSessionId NVARCHAR(50) NULL;
    
    ALTER TABLE Participants ADD CONSTRAINT FK_Participants_GameSessionId 
        FOREIGN KEY (GameSessionId) REFERENCES GameSessions(SessionId) ON DELETE SET NULL;
    
    CREATE INDEX IX_Participants_GameSessionId ON Participants(GameSessionId);
    
    PRINT 'Added GameSessionId to Participants table';
END
ELSE
BEGIN
    PRINT 'GameSessionId already exists in Participants table';
END
GO

-- Add GameSessionId to FFFAnswers table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'FFFAnswers') AND name = 'GameSessionId')
BEGIN
    ALTER TABLE FFFAnswers ADD GameSessionId NVARCHAR(50) NULL;
    
    ALTER TABLE FFFAnswers ADD CONSTRAINT FK_FFFAnswers_GameSessionId 
        FOREIGN KEY (GameSessionId) REFERENCES GameSessions(SessionId) ON DELETE SET NULL;
    
    CREATE INDEX IX_FFFAnswers_GameSessionId ON FFFAnswers(GameSessionId);
    
    PRINT 'Added GameSessionId to FFFAnswers table';
END
ELSE
BEGIN
    PRINT 'GameSessionId already exists in FFFAnswers table';
END
GO

-- Add GameSessionId to ATAVotes table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'ATAVotes') AND name = 'GameSessionId')
BEGIN
    ALTER TABLE ATAVotes ADD GameSessionId NVARCHAR(50) NULL;
    
    ALTER TABLE ATAVotes ADD CONSTRAINT FK_ATAVotes_GameSessionId 
        FOREIGN KEY (GameSessionId) REFERENCES GameSessions(SessionId) ON DELETE SET NULL;
    
    CREATE INDEX IX_ATAVotes_GameSessionId ON ATAVotes(GameSessionId);
    
    PRINT 'Added GameSessionId to ATAVotes table';
END
ELSE
BEGIN
    PRINT 'GameSessionId already exists in ATAVotes table';
END
GO

PRINT 'Telemetry tables setup complete!';
