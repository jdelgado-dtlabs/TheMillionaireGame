# Game Telemetry & Audit System Implementation Plan
**Date**: January 3, 2026  
**Status**: Planning Phase  
**Priority**: HIGH - Production Feature  

---

## 📋 Overview

Implement comprehensive game telemetry tracking and CSV export for post-game analysis and auditing. This system will capture detailed statistics per round and per game session.

---

## 🎯 Requirements

### Round-Level Data (Per Round)
1. **Web Participant Telemetry**
   - Total participant count
   - Device type breakdown (Mobile, Tablet, Desktop)
   - Browser type breakdown (Chrome, Edge, Firefox, Safari, etc.)
   - OS breakdown (Windows, iOS, Android, etc.)

2. **FFF Performance** (if FFF was played)
   - Total participants who submitted answers
   - Correct answer count
   - Incorrect answer count
   - Winner's time (fastest correct)
   - Average response time

3. **ATA Performance** (if ATA was used)
   - Total votes cast
   - Vote distribution (A, B, C, D percentages)
   - Voting completion rate
   - Average voting time

4. **Player Performance**
   - Final question reached (1-15)
   - Outcome: Win / Walk Away / Loss
   - Final winnings amount
   - Lifelines used:
     - 50:50 (Yes/No, which question)
     - Phone-a-Friend (Yes/No, which question)
     - Ask the Audience (Yes/No, which question, online/offline)
     - Switch the Question (Yes/No, which question)
     - Double Dip (Yes/No, which question)
     - Ask Host (Yes/No, which question)

5. **Round Timing**
   - Start time (first Lights Down timestamp)
   - End time (win/lose/walk away timestamp)
   - Duration (HH:MM:SS)

### Game-Level Data (Per Game Session)
1. **Game Summary**
   - Total rounds played
   - Game start time (first round Lights Down or Explain Game start)
   - Game end time (CompleteClosing() execution)
   - Total game duration (HH:MM:SS)

2. **Aggregate Statistics**
   - Total participants across all rounds (unique count if trackable)
   - Total questions answered correctly
   - Total lifelines used
   - Total winnings awarded

---

## 🏗️ Architecture

### Data Models

#### 1. RoundTelemetry Class
```csharp
public class RoundTelemetry
{
    // Round Identity
    public int RoundNumber { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration => EndTime - StartTime;
    
    // Web Participants
    public int TotalParticipants { get; set; }
    public Dictionary<string, int> DeviceTypes { get; set; } // Mobile: 5, Desktop: 3, etc.
    public Dictionary<string, int> BrowserTypes { get; set; }
    public Dictionary<string, int> OSTypes { get; set; }
    
    // FFF Performance (nullable if not played)
    public FFFStats? FFFPerformance { get; set; }
    
    // ATA Performance (nullable if not used)
    public ATAStats? ATAPerformance { get; set; }
    
    // Player Performance
    public int FinalQuestionReached { get; set; } // 1-15
    public string Outcome { get; set; } // "Win", "Walk Away", "Loss"
    public string FinalWinnings { get; set; }
    public List<LifelineUsage> LifelinesUsed { get; set; }
}

public class FFFStats
{
    public int TotalSubmissions { get; set; }
    public int CorrectAnswers { get; set; }
    public int IncorrectAnswers { get; set; }
    public string? WinnerName { get; set; }
    public double WinnerTime { get; set; } // seconds
    public double AverageResponseTime { get; set; }
}

public class ATAStats
{
    public int TotalVotes { get; set; }
    public Dictionary<string, int> VoteDistribution { get; set; } // A: 10, B: 5, C: 2, D: 8
    public Dictionary<string, double> VotePercentages { get; set; } // A: 40%, B: 20%, etc.
    public double VotingCompletionRate { get; set; } // votes / participants
    public double AverageVotingTime { get; set; }
    public bool WasOnlineMode { get; set; }
}

public class LifelineUsage
{
    public string LifelineName { get; set; }
    public int QuestionNumber { get; set; }
    public DateTime Timestamp { get; set; }
}

#### 2. GameTelemetry Class
```csharp
public class GameTelemetry
{
    // Game Identity
    public string SessionId { get; set; } = Guid.NewGuid().ToString();
    public DateTime GameStartTime { get; set; }
    public DateTime GameEndTime { get; set; }
    public TimeSpan TotalDuration => GameEndTime - GameStartTime;
    
    // Rounds
    public List<RoundTelemetry> Rounds { get; set; } = new();
    public int TotalRounds => Rounds.Count;
    
    // Aggregate Stats
    public int TotalUniqueParticipants { get; set; }
    public int TotalQuestionsAnswered { get; set; }
    public int TotalLifelinesUsed { get; set; }
    public string TotalWinningsAwarded { get; set; }
}
```

### CSV Export Structure

#### Round-Level CSV: `Game_{SessionId}_Rounds.csv`
```csv
RoundNumber,StartTime,EndTime,Duration,Participants,Devices_Mobile,Devices_Tablet,Devices_Desktop,Browsers_Chrome,Browsers_Edge,Browsers_Firefox,OS_Windows,OS_iOS,OS_Android,FFF_Played,FFF_Submissions,FFF_Correct,FFF_Incorrect,FFF_Winner,FFF_WinnerTime,ATA_Used,ATA_Votes,ATA_VoteA,ATA_VoteB,ATA_VoteC,ATA_VoteD,ATA_OnlineMode,FinalQuestion,Outcome,Winnings,Lifeline_5050,Lifeline_5050_Q,Lifeline_PAF,Lifeline_PAF_Q,Lifeline_ATA,Lifeline_ATA_Q,Lifeline_Switch,Lifeline_Switch_Q,Lifeline_DD,Lifeline_DD_Q,Lifeline_AskHost,Lifeline_AskHost_Q
1,2026-01-03 19:30:00,2026-01-03 19:45:30,00:15:30,12,4,2,6,8,3,1,7,3,2,Yes,10,8,2,John Doe,3.2,Yes,25,10,8,5,2,Online,10,Walk Away,$32000,Yes,6,No,,Yes,8,No,,No,,No,
2,2026-01-03 20:00:00,2026-01-03 20:12:15,00:12:15,15,5,3,7,10,4,1,9,4,2,No,,,,,,Yes,28,12,9,5,2,Online,15,Win,$1000000,Yes,12,Yes,14,No,,Yes,10,No,,No,
```

#### Game-Level CSV: `Game_{SessionId}_Summary.csv`
```csv
SessionId,GameStartTime,GameEndTime,TotalDuration,TotalRounds,TotalParticipants,TotalQuestions,TotalLifelines,TotalWinnings
abc123-def456,2026-01-03 19:30:00,2026-01-03 20:15:00,00:45:00,2,18,25,6,$1032000
```

---

## 🔧 Implementation Steps

### Phase 1: Data Model & Infrastructure (2-3 hours)

**Files to Create:**
- `src/MillionaireGame.Core/Models/Telemetry/RoundTelemetry.cs`
- `src/MillionaireGame.Core/Models/Telemetry/GameTelemetry.cs`
- `src/MillionaireGame.Core/Models/Telemetry/FFFStats.cs`
- `src/MillionaireGame.Core/Models/Telemetry/ATAStats.cs`
- `src/MillionaireGame.Core/Models/Telemetry/LifelineUsage.cs`
- `src/MillionaireGame.Core/Services/TelemetryService.cs`
- `src/MillionaireGame.Core/Services/CsvExportService.cs`

**Tasks:**
1. [ ] Create telemetry model classes in Core project
2. [ ] Create TelemetryService to manage current game/round data
3. [ ] Create CsvExportService with methods:
   - `ExportRoundsCSV(GameTelemetry data, string filePath)`
   - `ExportSummaryCSV(GameTelemetry data, string filePath)`
4. [ ] Add dependency injection registration in Program.cs

### Phase 2: Control Panel Integration (2-3 hours)

**File to Modify:**
- `src/MillionaireGame/Forms/ControlPanelForm.cs`

**Tasks:**
1. [ ] Replace `_firstRoundCompleted` with `_roundNumber` counter
   - Initialize to 0
   - Increment in `EndRoundSequence()` after completion
   - Reset to 0 in `ResetGameState()` (Reset Game button)

2. [ ] Add TelemetryService field and initialize in constructor

3. [ ] Track game start time:
   - In `btnExplainGame_Click()` → set `GameStartTime`
   - OR in first `btnLightsDown_Click()` if no Explain Game

4. [ ] Track round start time:
   - In `btnLightsDown_Click()` when `nmrLevel.Value == 1` (first question)
   - Call `TelemetryService.StartNewRound(roundNumber)`

5. [ ] Track round end time:
   - In `EndRoundSequence()` → `TelemetryService.CompleteRound(outcome, finalWinnings, questionReached)`

6. [ ] Track lifeline usage:
   - In each lifeline button click → `TelemetryService.RecordLifelineUsage(lifelineName, questionNumber)`

7. [ ] Track game end time:
   - In `CompleteClosing()` → set `GameEndTime`

8. [ ] Export CSV on game completion:
   - In `CompleteClosing()` after game ends
   - Prompt user for export location (SaveFileDialog)
   - Call `CsvExportService.ExportRoundsCSV()` and `ExportSummaryCSV()`
   - Optional: Auto-save to default location (Documents/MillionaireGame/Telemetry/)

### Phase 3: Web Server Integration (1-2 hours)

**Files to Modify:**
- `src/MillionaireGame.Web/Services/SessionService.cs`
- `src/MillionaireGame.Web/Hubs/GameHub.cs`

**Tasks:**
1. [ ] Collect participant telemetry on session join:
   - Store device type, browser, OS in Participant table (already exists)
   - Add telemetry fields to Participant model if needed

2. [ ] Track FFF performance:
   - In `SessionService.SubmitFFFAnswer()` → track submissions, correctness
   - Calculate average response time from submission timestamps

3. [ ] Track ATA performance:
   - In `SessionService.SubmitATAVote()` → track votes, distribution
   - Calculate completion rate and average voting time

4. [ ] Expose telemetry data via API endpoint:
   - Create `GetRoundTelemetry(sessionId)` method in SessionService
   - Return aggregated stats for current round

5. [ ] Send telemetry to main app:
   - After FFF/ATA completes, send stats to LifelineManager
   - LifelineManager forwards to TelemetryService

### Phase 4: Testing & Refinement (1-2 hours)

**Tasks:**
1. [ ] Unit tests for telemetry models
2. [ ] Integration tests for CSV export
3. [ ] Manual testing with full game flow:
   - Play 2-3 rounds with different outcomes
   - Use various lifelines
   - Run FFF and ATA with multiple participants
   - Verify CSV exports have correct data

4. [ ] Edge case handling:
   - Game reset mid-round (discard incomplete round?)
   - Application crash (save telemetry to temp file?)
   - No web participants (zeros in participant columns)

---

## 📁 File Structure

```
MillionaireGame.Core/
├── Models/
│   └── Telemetry/
│       ├── RoundTelemetry.cs
│       ├── GameTelemetry.cs
│       ├── FFFStats.cs
│       ├── ATAStats.cs
│       └── LifelineUsage.cs
└── Services/
    ├── TelemetryService.cs
    └── CsvExportService.cs

MillionaireGame/
└── Forms/
    └── ControlPanelForm.cs (modified)

MillionaireGame.Web/
├── Services/
│   └── SessionService.cs (modified)
└── Hubs/
    └── GameHub.cs (modified)

Documents/MillionaireGame/Telemetry/ (default export location)
├── Game_abc123_Rounds.csv
└── Game_abc123_Summary.csv
```

---

## 🎨 UI Enhancements (Optional)

1. **Export Menu Item**
   - Add "Export Game Telemetry" to File menu
   - Allow exporting telemetry at any time (not just at game end)

2. **Telemetry Dashboard** (Future Enhancement)
   - Live statistics display during game
   - Show current round number, participants online, lifelines remaining
   - Could be separate window or panel in Control Panel

3. **Auto-Export Toggle**
   - Settings option to auto-export CSV on game completion
   - Default export location preference

---

## 🔍 Data Privacy Considerations

1. **Participant Anonymization**
   - Store display names in FFF/ATA stats but NOT in CSV export
   - Replace with "Participant 1", "Participant 2", etc.
   - OR: Add checkbox "Include Participant Names in Export"

2. **GDPR Compliance**
   - Telemetry data should be stored temporarily (in-memory)
   - CSV export is manual action (explicit consent)
   - Clear all telemetry on "Reset Game" button

3. **Session Persistence**
   - Consider: Save telemetry to database for multi-session tracking
   - OR: Keep in-memory only (current session only)

---

## 📊 Sample Output

### Game_abc123_Rounds.csv (Excerpt)
```csv
RoundNumber,StartTime,EndTime,Duration,Participants,FinalQuestion,Outcome,Winnings,Lifeline_5050,Lifeline_ATA
1,2026-01-03 19:30:00,2026-01-03 19:45:30,00:15:30,12,10,Walk Away,$32000,Yes,Yes
2,2026-01-03 20:00:00,2026-01-03 20:12:15,00:12:15,15,15,Win,$1000000,Yes,No
```

### Game_abc123_Summary.csv
```csv
SessionId,GameStartTime,GameEndTime,TotalDuration,TotalRounds,TotalParticipants,TotalWinnings
abc123-def456,2026-01-03 19:30:00,2026-01-03 20:15:00,00:45:00,2,18,$1032000
```

---

## ✅ Acceptance Criteria

- [ ] Round number counter replaces boolean
- [ ] All round-level data captured accurately
- [ ] All game-level data captured accurately
- [ ] CSV exports contain all required columns
- [ ] CSV files readable in Excel/Google Sheets
- [ ] Telemetry resets properly on "Reset Game"
- [ ] No performance impact during gameplay
- [ ] User prompted for export location at game end
- [ ] Optional: Auto-export to default location
- [ ] Documentation updated with telemetry feature

---

## 🚀 Estimated Time

- **Phase 1**: 2-3 hours (Data models & infrastructure)
- **Phase 2**: 2-3 hours (Control panel integration)
- **Phase 3**: 1-2 hours (Web server integration)
- **Phase 4**: 1-2 hours (Testing & refinement)

**Total**: 6-10 hours

---

## 📚 Dependencies

- **CsvHelper** NuGet package (for robust CSV generation)
  - Alternative: Manual CSV writing (StringBuilder)
- **System.IO** (already included)
- **Newtonsoft.Json** (already in project)

---

## 🔄 Future Enhancements

1. **JSON Export** (alternative to CSV)
2. **Database Storage** (persistent telemetry across sessions)
3. **Web Dashboard** (real-time stats viewer)
4. **Comparative Analysis** (compare performance across multiple games)
5. **Email Reports** (auto-send CSV to producer after show)
6. **Cloud Storage Integration** (OneDrive, Google Drive, Dropbox)

---

## 📝 Notes

- Keep telemetry lightweight - don't impact game performance
- Consider memory usage if storing large amounts of participant data
- CSV format ensures universal compatibility (Excel, Google Sheets, Python, R)
- SessionId should be unique per game (GUID recommended)
- Consider timestamp format (ISO 8601 for international compatibility)

---

**Status**: Ready for implementation  
**Next Steps**: Begin Phase 1 - Create data models in Core project
