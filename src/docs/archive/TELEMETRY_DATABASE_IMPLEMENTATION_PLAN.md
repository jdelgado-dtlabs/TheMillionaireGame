# Telemetry Database Storage & Viewer Implementation Plan

**Version**: v1.0.1 Telemetry Fix  
**Date**: 2026-01-05  
**Status**: Planning

## Overview

This document outlines the implementation plan for adding persistent database storage for game telemetry and creating a Telemetry Viewer interface to view and export historical game data.

---

## Current State Analysis

### Existing Telemetry System
- **Storage**: In-memory only (singleton `TelemetryService`)
- **Lifecycle**: Data lost when application closes
- **Export**: `TelemetryExportService` exports to Excel (XLSX)
- **Location**: `%LocalAppData%\TheMillionaireGame\Telemetry\`

### Data Structure
- `GameTelemetry` - Root object containing session info and rounds
- `RoundTelemetry` - Individual round data (outcomes, lifelines, participants, FFF/ATA stats)
- `FFFStats` - Fast Finger First performance data
- `ATAStats` - Ask the Audience performance data
- `LifelineUsage` - Individual lifeline usage tracking

### Current Game Flow
1. **Game Start**: Currently tracked when `StartNewGame()` is called manually
2. **Round Start**: `StartNewRound(roundNumber)` creates new round
3. **Round End**: `CompleteRound()` records outcome and winnings
4. **Game End**: `CompleteGame()` calculates aggregate statistics

---

## Goals

1. **Persistent Storage**: Save telemetry data to SQL database
2. **Crash-Safe**: Mark incomplete sessions when app crashes
3. **Lifecycle Tracking**: Auto-track game start/end based on UI events
4. **Smart FFF/ATA Tracking**: Only record when online modes are used
5. **Telemetry Viewer**: UI to browse and export historical game data
6. **Export Functionality**: User-initiated export from viewer (no auto-export)

---

## Database Schema Design

### Table 1: `GameSessions`

Stores high-level game session information.

```sql
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
```

### Table 2: `GameRounds`

Stores individual round data, linked to game sessions.

```sql
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
```

**Design Notes:**
- Outcome enum: 1=Won, 2=Lost, 3=Walked Away, 4=Interrupted
- Foreign key cascade delete: removing session removes all rounds
- Aggregate data (totals, stats) calculated from rounds, not stored
- Participant/device/FFF/ATA data already stored in existing WAPS tables (see below)

---

## Existing Database Tables for Telemetry Data

The following tables already exist and store telemetry-related data. They need to be updated with the `SessionId` foreign key to link them to game sessions:

### Table: `Sessions` (WAPS)
Currently stores web participation session data:
- `Id` (NVARCHAR(450)) - Web session identifier
- `HostName`, `CreatedAt`, `StartedAt`, `EndedAt`, `Status`, `CurrentMode`, `CurrentQuestionId`

**Required Changes:**
- This table serves a different purpose (web sessions), but we can cross-reference using timestamps
- No modifications needed

### Table: `Participants` (WAPS)
Stores participant information including device/browser data:
- `Id`, `SessionId`, `DisplayName`, `JoinedAt`, `ConnectionId`, `LastSeenAt`, `IsActive`
- `DeviceType`, `OSType`, `OSVersion`, `BrowserType`, `BrowserVersion`
- `HasPlayedFFF`, `HasUsedATA`, `State`

**Required Changes:**
- Add `GameSessionId NVARCHAR(50) NULL` column with FK to `GameSessions(SessionId)`
- This links participants to game sessions for device/browser statistics aggregation

### Table: `FFFAnswers` (WAPS)
Stores FFF answer submissions:
- `Id`, `SessionId`, `ParticipantId`, `QuestionId`, `AnswerSequence`
- `SubmittedAt`, `TimeElapsed`, `IsCorrect`, `Rank`

**Required Changes:**
- Add `GameSessionId NVARCHAR(50) NULL` column with FK to `GameSessions(SessionId)`
- This links FFF attempts to game sessions for FFF statistics aggregation

### Table: `ATAVotes` (WAPS)
Stores ATA vote submissions:
- `Id`, `SessionId`, `ParticipantId`, `QuestionText`, `SelectedOption`, `SubmittedAt`

**Required Changes:**
- Add `GameSessionId NVARCHAR(50) NULL` column with FK to `GameSessions(SessionId)`
- This links ATA votes to game sessions for ATA statistics aggregation

### Table: `LifelineUsages` (NEW)
**This table needs to be created** to store lifeline usage events:
```sql
CREATE TABLE LifelineUsages (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    GameSessionId NVARCHAR(50) NOT NULL,          -- FK to GameSessions
    RoundId INT NOT NULL,                         -- FK to GameRounds
    LifelineType INT NOT NULL,                    -- Maps to LifelineType enum
    QuestionNumber INT NOT NULL,                  -- Question number where used
    Metadata NVARCHAR(200) NULL,                  -- e.g., "Online", "Offline"
    
    CONSTRAINT FK_LifelineUsages_GameSessionId FOREIGN KEY (GameSessionId) 
        REFERENCES GameSessions(SessionId) ON DELETE CASCADE,
    CONSTRAINT FK_LifelineUsages_RoundId FOREIGN KEY (RoundId) 
        REFERENCES GameRounds(RoundId) ON DELETE NO ACTION
);

CREATE INDEX IX_LifelineUsages_GameSessionId ON LifelineUsages(GameSessionId);
CREATE INDEX IX_LifelineUsages_RoundId ON LifelineUsages(RoundId);
```

**Lifeline Type Enum (from `MillionaireGame.Core.Models.LifelineType`):**
- `1` = FiftyFifty (50:50)
- `2` = PlusOne (Phone a Friend)
- `3` = AskTheAudience
- `4` = SwitchQuestion
- `5` = DoubleDip
- `6` = AskTheHost

**Note:** If no lifelines are used in a round, there will be no rows in this table for that round.

---

## Implementation Phases

### Phase 1: Database Infrastructure

**Files to Modify/Create:**
- `src/MillionaireGame.Core/Database/TelemetryRepository.cs` (NEW)
- `src/MillionaireGame.Core/Models/Telemetry/` (existing models)

**Tasks:**
1. Create `TelemetryRepository` class
   - `SaveGameSession(GameTelemetry)` - Insert/Update game session
   - `SaveGameRound(RoundTelemetry, string sessionId)` - Insert round
   - `GetAllGameSessions()` - Retrieve session list for dropdown
   - `GetSessionsByDate(DateTime date)` - Retrieve sessions for specific date
   - `GetSessionDates()` - Get list of dates with telemetry data (for calendar highlighting)
   - `GetGameSessionWithRounds(string sessionId)` - Retrieve full session data with aggregated stats
   - `UpdateGameSessionEndTime(string sessionId, DateTime endTime)` - Mark completion
   - `GetIncompleteGameSessions()` - Find sessions without end time
   - Aggregate methods:
     - `GetParticipantCountForSession(string sessionId)` - Count from Participants table
     - `GetDeviceStatsForSession(string sessionId)` - Aggregate from Participants.DeviceType
     - `GetBrowserStatsForSession(string sessionId)` - Aggregate from Participants.BrowserType
     - `GetFFFStatsForSession(string sessionId)` - Aggregate from FFFAnswers table
     - `GetATAStatsForSession(string sessionId)` - Aggregate from ATAVotes table
     - `GetLifelineUsagesForSession(string sessionId)` - Query from LifelineUsages table

2. Create SQL migration script
   - `src/MillionaireGame/lib/sql/telemetry_tables.sql`
   - Include CREATE TABLE statements for GameSessions, GameRounds, LifelineUsages
   - Include ALTER TABLE statements to add GameSessionId FK to Participants, FFFAnswers, ATAVotes
   - Include indexes
   - Safe to run multiple times (IF NOT EXISTS checks)

3. Add database initialization
   - Run telemetry table creation on app startup
   - Location: `Program.cs` after SQL settings load
   - Update `GameDatabaseContext.CreateDatabaseAsync()` to include new tables

---

### Phase 2: Telemetry Service Integration

**Files to Modify:**
- `src/MillionaireGame.Core/Services/TelemetryService.cs`

**Tasks:**
1. Add database persistence to existing methods:
   - `StartNewGame()` → Insert GameSession row, capture SessionId for linking
   - `StartNewRound(roundNumber)` → Create round and link to current session
   - `CompleteRound()` → Insert GameRound row with outcome and winnings
   - `CompleteGame()` → Update GameSession (set GameEndTime)
   - `RecordLifelineUsage()` → Insert into LifelineUsages table with GameSessionId and RoundId

2. Add GameSessionId linking for web data:
   - When web session starts (FFF/ATA online), update Participants table with current GameSessionId
   - When FFF answers are submitted, update FFFAnswers with GameSessionId
   - When ATA votes are submitted, update ATAVotes with GameSessionId
   - Location: `TelemetryBridge` callbacks in Program.cs

3. Add error handling:
   - Graceful degradation if database save fails
   - Log errors to GameConsole
   - In-memory data still available for export

---

### Phase 3: Game Lifecycle Tracking

**Files to Modify:**
- `src/MillionaireGame/Forms/ControlPanelForm.cs`
- `src/MillionaireGame/Program.cs`

**Tasks:**

#### ControlPanelForm.cs Changes

1. **Game Start Tracking** (`btnHostIntro_Click`)
   ```csharp
   private void btnHostIntro_Click(object? sender, EventArgs e)
   {
       // Existing code...
       
       // Start telemetry session
       var telemetryService = TelemetryService.Instance;
       telemetryService.StartNewGame();
       
       // Existing code...
   }
   ```

2. **Game End Tracking** (`CompleteClosing`)
   ```csharp
   private void CompleteClosing()
   {
       _closingStage = ClosingStage.Complete;
       
       // Complete telemetry session
       var telemetryService = TelemetryService.Instance;
       telemetryService.CompleteGame();
       
       // Existing code...
   }
   ```

#### Program.cs Changes

1. **Shutdown Handler** - Update game end time on app close
   ```csharp
   Application.ApplicationExit += (s, e) =>
   {
       var telemetryService = TelemetryService.Instance;
       var currentGame = telemetryService.GetCurrentGameData();
       
       // If game started but not completed, mark end time
       if (currentGame.GameStartTime != default && 
           currentGame.GameEndTime == default)
       {
           telemetryService.CompleteGame();
       }
   };
   ```

---

### Phase 4: Telemetry Viewer Form

**Files to Create:**
- `src/MillionaireGame/Forms/TelemetryViewerForm.cs` (NEW)
- `src/MillionaireGame/Forms/TelemetryViewerForm.Designer.cs` (NEW)

**UI Design:**

```
┌─────────────────────────────────────────────────────────────┐
│  Telemetry Viewer                                    [_][□][X]│
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  Filter by Date: [📅 Select Date]  [Clear Filter]            │
│  Game Session: [Dropdown: Session ID - Date ▼]  [Export] [Batch Export]    │
│                                                               │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ Session Details:                                      │   │
│  │   Session ID: a1b2c3d4-e5f6-7890...                 │   │
│  │   Start Time: 2026-01-05 14:30:15                    │   │
│  │   End Time: 2026-01-05 15:45:22                      │   │
│  │   Duration: 1h 15m 7s                                │   │
│  │   Status: ✓ Complete / ⚠ Incomplete                 │   │
│  │   Total Rounds: 3                                    │   │
│  │   Total Winnings: $75,000 €50,000                   │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                               │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │ Round Data:                                        DataGridView │   │
│  │ Round | Start     | End       | Outcome | Currency1 | Currency2 │   │
│  │   1   | 14:30:15  | 14:52:30  | Won     | $32,000   |           │   │
│  │   2   | 14:53:00  | 15:18:45  | Lost    | $1,000    |           │   │
│  │   3   | 15:19:00  | 15:45:22  | Won     | $64,000   |           │   │
│  │                                                                   │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                                                               │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │ Round Details:                                    (Select row)│   │
│  │   Questions Reached: 12                                        │   │
│  │   Lifelines Used: 50/50 (Q7), ATA (Q11)                       │   │
│  │   Participants: 15                                             │   │
│  │   FFF Stats: 15 submitted, 8 correct, Winner: John (3.45s)    │   │
│  │   ATA Stats: A:35%, B:15%, C:42%, D:8%                        │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                                                               │
│                                           [Close]             │
└─────────────────────────────────────────────────────────────┘
```

**Implementation Details:**

1. **Date Filtering**
   - Button with calendar icon opens MonthCalendar control
   - Dates with telemetry data are marked in bold
   - When date selected, dropdown filters to sessions from that date only
   - "Clear Filter" button shows all sessions again
   - Implementation:
     ```csharp
     // Get dates with data for calendar highlighting
     var datesWithData = repository.GetSessionDates();
     monthCalendar.BoldedDates = datesWithData.ToArray();
     
     // Filter dropdown by selected date
     var filteredSessions = repository.GetSessionsByDate(selectedDate);
     cmbGameSessions.DataSource = filteredSessions;
     ```

2. **Dropdown Population**
   - Load all sessions ordered by start time (newest first)
   - Display format: `"SessionID (Short) - Date - Status"`
   - Example: `"f67890 - 2026-01-05 14:30 - Complete"` (last 6 characters of GUID)
   - Mark incomplete sessions: `"f67890 - 2026-01-05 14:30 - ⚠ INCOMPLETE"`
   - Filters by selected date if filter active

3. **Session Details Panel**
   - Read-only labels showing session metadata
   - Visual indicator for complete vs incomplete
   - Calculated values (duration, total winnings)
   - Display winnings as "Currency1Total Currency2Total" (e.g., "$75,000 €50,000")
   - If Currency2 not enabled, show only Currency1 (e.g., "$75,000")

4. **Rounds DataGridView**
   - Display all rounds for selected session
   - Columns: Round #, Start, End, Duration, Outcome, Currency1 Winnings, Currency2 Winnings, Questions
   - Currency2 column will be blank if not enabled or not used in that round
   - Click row → Load round details below

5. **Round Details Panel**
   - Display full round information when row selected
   - Parse JSON for lifelines, FFF/ATA stats
   - Format for readability

6. **Export Button**
   - Use SaveFileDialog pattern from QuestionEditor
   - Thread-safe modal handling (STA thread)
   - Export to Excel using existing `TelemetryExportService`
   - Excel workbook includes:
     - Session data sheet
     - Round data sheet
     - Statistics sheet with charts (aggregate stats, win rate analysis, performance graphs)

7. **Batch Export Button**
   - Export multiple sessions at once
   - Use FolderBrowserDialog for output directory selection
   - For each session:
     - Create individual Excel file named `Telemetry_{SessionIdShort}_{Date}.xlsx`
     - Include all sheets (session data, rounds, statistics)
   - Package all Excel files into a single ZIP file
   - ZIP filename: `TelemetryBatch_{Date}_{Count}Sessions.zip`
   - Implementation:
     ```csharp
     // Get all sessions (or filtered by date)
     var sessions = _currentFilterDate.HasValue 
         ? repository.GetSessionsByDate(_currentFilterDate.Value)
         : repository.GetAllGameSessions();
     
     // Use FolderBrowserDialog
     using var folderDialog = new FolderBrowserDialog
     {
         Description = "Select folder for batch export"
     };
     
     if (folderDialog.ShowDialog() == DialogResult.OK)
     {
         var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
         Directory.CreateDirectory(tempFolder);
         
         // Export each session to temp folder
         foreach (var session in sessions)
         {
             var fileName = $"Telemetry_{session.SessionIdShort}_{session.StartTime:yyyyMMdd}.xlsx";
             var filePath = Path.Combine(tempFolder, fileName);
             exportService.ExportToExcel(filePath, session);
         }
         
         // Create ZIP
         var zipPath = Path.Combine(folderDialog.SelectedPath, 
             $"TelemetryBatch_{DateTime.Now:yyyyMMdd_HHmmss}_{sessions.Count}Sessions.zip");
         ZipFile.CreateFromDirectory(tempFolder, zipPath);
         
         // Cleanup temp folder
         Directory.Delete(tempFolder, true);
     }
     ```
   - Shows progress form during export (non-blocking)
   - Confirms completion with file location

**CRITICAL: Non-Blocking UI Requirements**
- **NO UI BLOCKING**: Never use blocking waits or synchronous database queries on the UI thread
- **Background Operations**: All database queries must run on background threads (Task.Run or async/await)
- **Progress Indication**: If calculations take time, show a non-modal, auto-dismissing message:
  ```csharp
  // Show calculating message (MessageBoxIcon.None, no buttons)
  var calculatingForm = new Form
  {
      Text = "Please Wait",
      StartPosition = FormStartPosition.CenterParent,
      Size = new Size(300, 100),
      FormBorderStyle = FormBorderStyle.FixedDialog,
      ControlBox = false
  };
  var label = new Label { Text = "Calculating stats...", AutoSize = true, Location = new Point(20, 30) };
  calculatingForm.Controls.Add(label);
  
  // Run query async, auto-close when done
  Task.Run(async () => {
      var data = await repository.GetStatsAsync();
      calculatingForm.Invoke(() => calculatingForm.Close());
  });
  calculatingForm.ShowDialog(this);
  ```
- **Async Methods**: Use async/await pattern for all repository methods
- **UI Updates**: Use `Invoke()` or `BeginInvoke()` for cross-thread UI updates
- **Never Freeze Game**: Telemetry operations must never impact game performance or responsiveness

**Export Implementation (Based on QuestionEditor Pattern):**

```csharp
private void btnExport_Click(object sender, EventArgs e)
{
    if (cmbGameSessions.SelectedValue == null) return;
    
    string sessionId = cmbGameSessions.SelectedValue.ToString();
    
    using var saveFileDialog = new SaveFileDialog
    {
        Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
        Title = "Export Telemetry Data",
        FileName = $"Telemetry_{sessionId}_{ DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
        DefaultExt = "xlsx",
        RestoreDirectory = true
    };
    
    // Run on separate thread to avoid modal deadlock
    DialogResult result = DialogResult.Cancel;
    var thread = new Thread(() =>
    {
        result = saveFileDialog.ShowDialog();
    });
    thread.SetApartmentState(ApartmentState.STA);
    thread.Start();
    
    // Keep UI responsive
    while (thread.IsAlive)
    {
        Application.DoEvents();
        Thread.Sleep(10);
    }
    
    if (result == DialogResult.OK)
    {
        try
        {
            btnExport.Enabled = false;
            var repository = new TelemetryRepository(_connectionString);
            var gameData = repository.GetGameSessionWithRounds(sessionId);
            
            var exportService = new TelemetryExportService();
            exportService.ExportToExcel(saveFileDialog.FileName, gameData);
            
            MessageBox.Show($"Telemetry exported successfully!\n{saveFileDialog.FileName}",
                "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Export failed: {ex.Message}",
                "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnExport.Enabled = true;
        }
    }
}
```

---

### Phase 5: Menu Integration

**Files to Modify:**
- `src/MillionaireGame/Forms/ControlPanelForm.Designer.cs`
- `src/MillionaireGame/Forms/ControlPanelForm.cs`

**Tasks:**

1. Add menu item to Game menu
   ```csharp
   // In ControlPanelForm.Designer.cs
   private ToolStripMenuItem menuTelemetryViewer;
   
   // Initialize
   menuTelemetryViewer = new ToolStripMenuItem
   {
       Name = "menuTelemetryViewer",
       Text = "Telemetry Viewer",
       ShortcutKeys = Keys.Control | Keys.T
   };
   menuTelemetryViewer.Click += menuTelemetryViewer_Click;
   
   // Add to Game menu (after Settings)
   menuGame.DropDownItems.Add(menuTelemetryViewer);
   ```

2. Add event handler
   ```csharp
   // In ControlPanelForm.cs
   private void menuTelemetryViewer_Click(object? sender, EventArgs e)
   {
       var form = new TelemetryViewerForm(_sqlConnectionString);
       form.ShowDialog(this); // Modal dialog
   }
   ```

---

### Phase 6: Remove Auto-Export

**Files to Modify:**
- `src/MillionaireGame/Forms/ControlPanelForm.cs` (if auto-export exists)
- `src/MillionaireGame.Core/Services/TelemetryService.cs` (if auto-export logic exists)

**Tasks:**
1. Remove any automatic export calls on game completion
2. Remove auto-export settings if they exist
3. Export is now user-initiated via Telemetry Viewer only

---

## Testing Checklist

### Unit Testing
- [ ] TelemetryRepository CRUD operations
- [ ] JSON serialization/deserialization for complex fields
- [ ] Incomplete session detection
- [ ] Game lifecycle start/end tracking

### Integration Testing
- [ ] Full game flow: Host Intro → Rounds → Closing
- [ ] Database persistence across app restarts
- [ ] Incomplete session marking on crash
- [ ] FFF Online vs Offline (data presence vs NULL)
- [ ] ATA Online vs Offline (data presence vs NULL)

### UI Testing
- [ ] Telemetry Viewer opens and loads sessions
- [ ] Dropdown populates correctly
- [ ] Session details display correctly
- [ ] Rounds grid displays and updates
- [ ] Round details panel updates on selection
- [ ] Export button saves XLSX file
- [ ] Batch export creates ZIP with multiple XLSX files
- [ ] Incomplete sessions marked visually

### Edge Cases
- [ ] No telemetry data (empty dropdown)
- [ ] Session with no rounds
- [ ] Session with incomplete rounds
- [ ] Application crash during game (incomplete session)
- [ ] Database connection failure (graceful degradation)
- [ ] Export with very long session (many rounds)
- [ ] Batch export with large number of sessions (50+)
- [ ] Batch export with filtered date range

---

## Implementation Order

1. **Day 1**: Database infrastructure
   - Create SQL tables
   - Create TelemetryRepository
   - Add table initialization to Program.cs

2. **Day 2**: Service integration
   - Update TelemetryService with database saves
   - Add lifecycle tracking to ControlPanelForm
   - Add shutdown handler

3. **Day 3**: Telemetry Viewer
   - Create TelemetryViewerForm UI
   - Implement data loading and display
   - Add export functionality

4. **Day 4**: Testing and refinement
   - Integration testing
   - Edge case handling
   - UI polish

---

## Migration Considerations

### For Users Upgrading from v1.0.0
- No migration needed (new feature)
- Telemetry tables created automatically on first launch
- Historical data not recoverable (was in-memory only)

### Database Performance
- Indexes on SessionId and StartTime for fast queries
- JSON columns for flexibility without additional tables
- Cascade delete maintains referential integrity

---

## Future Enhancements (Post v1.0.1)

No additional enhancements planned for post-v1.0.1 at this time.

**Note**: Statistics/charts are handled via Excel export. Session comparisons can be done in Excel. Data is stored indefinitely in the database with no auto-archiving.

---

## Risk Assessment

| Risk | Impact | Mitigation |
|------|--------|------------|
| Large Excel export files | Medium | Progress indication during export, limit batch export size |
| Modal dialog deadlock | Medium | Use STA thread pattern from QuestionEditor |
| Incomplete data on crash | Low | Expected behavior, marked visually |

**Note**: Database connection failure affects the entire application, not just telemetry. No JSON storage is used in telemetry tables.

---

## Success Criteria

- ✅ Game sessions persist across app restarts
- ✅ Incomplete sessions detectable and marked
- ✅ Telemetry Viewer displays all historical data
- ✅ Export works without UI freezing
- ✅ Batch export creates ZIP package with individual Excel files
- ✅ FFF/ATA data only recorded when online modes used
- ✅ No auto-export (user-initiated only)
- ✅ Graceful handling of database errors

---

## Code Locations Summary

### New Files
- `src/MillionaireGame.Core/Database/TelemetryRepository.cs`
- `src/MillionaireGame/Forms/TelemetryViewerForm.cs`
- `src/MillionaireGame/Forms/TelemetryViewerForm.Designer.cs`
- `src/MillionaireGame/lib/sql/telemetry_tables.sql`

### Modified Files
- `src/MillionaireGame.Core/Services/TelemetryService.cs`
- `src/MillionaireGame.Core/Services/TelemetryExportService.cs` (add statistics sheet with charts)
- `src/MillionaireGame/Forms/ControlPanelForm.cs`
- `src/MillionaireGame/Forms/ControlPanelForm.Designer.cs`
- `src/MillionaireGame/Program.cs`

### Reference Files
- `src/MillionaireGame/Forms/QuestionEditor/ExportQuestionsForm.cs` (SaveFileDialog pattern)
- `src/MillionaireGame.Core/Services/TelemetryExportService.cs` (existing Excel export)

---

## Timeline Estimate

- **Phase 1**: 4-6 hours (Database infrastructure)
- **Phase 2**: 2-3 hours (Service integration)
- **Phase 3**: 2 hours (Lifecycle tracking)
- **Phase 4**: 8-10 hours (Telemetry Viewer with batch export)
- **Phase 5**: 1 hour (Menu integration)
- **Phase 6**: 30 minutes (Remove auto-export)
- **Testing**: 3-4 hours

**Total Estimate**: 20-26 hours

---

## Sign-off

This plan has been reviewed and approved for implementation as part of v1.0.1.

**Next Steps**: Begin implementation starting with Phase 1 (Database Infrastructure).
