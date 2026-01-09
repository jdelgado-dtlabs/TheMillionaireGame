# State Synchronization Implementation Plan

**Status:** ✅ **IMPLEMENTATION COMPLETE** - Ready for Testing  
**Branch:** `feature/web-state-sync`  
**Date Completed:** January 8, 2026  
**Session Document:** `src/docs/sessions/20260108_WEB_STATE_SYNC_IMPLEMENTATION.md`

## Implementation Status

✅ **Phase 1: Backend State Management** - Complete  
✅ **Phase 2: State Sync Logic** - Complete  
✅ **Phase 3: Frontend Integration** - Complete  
⏳ **Phase 4: Testing** - Pending User Testing

All code changes committed to feature branch. Ready for build, test, and merge.

---

## Problem Statement

Currently, the webserver does not maintain persistent state awareness for game phases (FFF Online, Ask The Audience). When clients disconnect and reconnect:

1. **FFF Online**: If a participant reconnects during an active FFF question, they don't receive the current question state (to view as spectator)
2. **Ask The Audience (ATA)**: If a participant joins/reconnects during ATA, they don't see the voting UI (to view as spectator)
3. **General Issue**: The server broadcasts events only to connected clients at the moment of the event, missing anyone who joins later

### Important Fairness Constraints

**Participants who join/reconnect DURING active competition/voting are spectators only:**

- **FFF Online**: Once answers are revealed and the 20-second timer starts, late/reconnecting clients can VIEW the question but CANNOT submit answers
- **ATA Voting**: Once the 60-second voting timer starts, late/reconnecting clients can VIEW the voting UI but CANNOT cast votes
- **Allowed Participation Windows**:
  - ✅ **Before** the timer starts (pre-question/voting state)
  - ❌ **During** active timer (spectator mode only)
  - ✅ **After** the question/voting ends (can participate in next round)

**NO AUTO-SUBMISSION ON TIMER EXPIRY:**

- **Critical Rule**: If a participant does not click the Submit button before the timer expires, NO answer is submitted
- **Current Bug**: Client currently auto-submits whatever sequence is arranged when timer hits 0:00
- **Correct Behavior**: Timer expiry = missed opportunity, participant is NOT ranked/considered
- **Only Explicit Submissions Count**: Participants MUST click Submit to have their answer recorded
- **Server Validation**: Server should reject any submission received after timer expiry (use question start time + time limit)

## Root Cause Analysis

### Current Architecture Issues

1. **No State Persistence on Reconnect**: The `OnConnectedAsync` and `JoinSession` methods don't check current session mode/state
2. **Event-Only Broadcasting**: Messages like `QuestionStarted` and `ATAStarted` are only sent when the event occurs
3. **Client-Side Logic**: Clients store state locally but have no way to retrieve current server state on reconnect
4. **Missing State Sync Method**: No hub method exists for clients to request current game state

### What Works Currently

- Session mode tracking exists (`Session.CurrentMode`, `Session.CurrentQuestionId`, `Session.QuestionStartTime`)
- `UpdateSessionModeAsync` properly sets state when questions start/end
- `GetCurrentGameState()` exists but only returns high-level game state, not specific FFF/ATA details
- Client reconnection logic attempts to rejoin session

### What's Missing

- **State synchronization on reconnect**: No mechanism to send current question/vote state to reconnecting clients
- **New joiner state sync**: No way for late joiners to get current phase state
- **State retrieval methods**: No hub methods to get current FFF question or ATA vote state

## Solution Architecture

### Core Concept: State Sync on Connection with Participation Eligibility

When a client connects or reconnects to a session, they should immediately receive:
1. Current session mode (Idle/FFF/ATA)
2. Active question details (if applicable) - **FOR VIEWING ONLY**
3. Time remaining (if applicable)
4. Current voting results (for ATA, if applicable)
5. **Participation eligibility flag** - Can they submit answers/votes?

**Eligibility Rules:**
- Client connected BEFORE question started → Can participate
- Client connected DURING active question/voting → Spectator only (view but no submit)
- Client connected AFTER question ended → Can participate in next round

### Implementation Approach

#### Phase 1: Server-Side State Management

**1. Add State Retrieval Methods to SessionService**

```csharp
// SessionService.cs - New methods

/// <summary>
/// Get current active FFF question state for a session
/// </summary>
public async Task<FFFQuestionState?> GetCurrentFFFStateAsync(string sessionId)
{
    var session = await _context.Sessions.FindAsync(sessionId);
    if (session?.CurrentMode != SessionMode.FFF || !session.CurrentQuestionId.HasValue)
        return null;
    
    // Get question from FFF database
    var question = await _fffService.GetQuestionByIdAsync(session.CurrentQuestionId.Value);
    if (question == null)
        return null;
    
    return new FFFQuestionState
    {
        QuestionId = session.CurrentQuestionId.Value,
        QuestionText = question.QuestionText,
        Options = new[] { question.AnswerA, question.AnswerB, question.AnswerC, question.AnswerD },
        StartTime = session.QuestionStartTime ?? DateTime.UtcNow,
        TimeLimit = 20000 // Default time limit
    };
}

/// <summary>
/// Get current active ATA state for a session
/// </summary>
public async Task<ATAQuestionState?> GetCurrentATAStateAsync(string sessionId)
{
    var session = await _context.Sessions.FindAsync(sessionId);
    if (session?.CurrentMode != SessionMode.ATA || !session.CurrentQuestionId.HasValue)
        return null;
    
    // Get current voting percentages
    var percentages = await CalculateATAPercentagesAsync(sessionId);
    var totalVotes = await GetATAVoteCountAsync(sessionId);
    
    return new ATAQuestionState
    {
        QuestionId = session.CurrentQuestionId.Value,
        QuestionText = "Current Ask The Audience Question", // Would need to store this
        StartTime = session.QuestionStartTime ?? DateTime.UtcNow,
        CurrentResults = percentages,
        TotalVotes = totalVotes
    };
}
```

**2. Add State Models**

```csharp
// Models/GameState.cs - New models

public class FFFQuestionState
    public bool IsActive { get; set; } // Timer still running?
}

public class ATAQuestionState
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public Dictionary<string, double> CurrentResults { get; set; } = new();
    public int TotalVotes { get; set; }
    public bool IsActive { get; set; } // Voting still open?
}

public class ParticipantEligibility
{
    public bool CanSubmitAnswer { get; set; } // For FFF
    public bool CanVote { get; set; } // For ATA
    public string Reason { get; set; } = string.Empty; // Why they can't participate
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public Dictionary<string, double> CurrentResults { get; set; } = new();
    public int TotalVotes { get; set; }
}
```

**3. Enhance GameHub with State Sync Methods**

```csharp
// GameHub.cs - Enhanced JoinSession

public async Task<object> JoinSession(string sessionId, string displayName, string? participantId = null, DeviceTelemetry? telemetry = null)
{
    // ... existing join logic ...
    
    // After successful join, send current state (with participant ID for eligibility check)
    await SyncClientStateAsync(sessionId, participant.Id);
    
    return new
    {
        Success = true,
        ParticipantId = participant.Id,
        DisplayName = participant.DisplayName,
        SessionId = sessionId,
        State = participant.State.ToString(),, string participantId)
{
    var session = await _sessionService.GetSessionAsync(sessionId);
    if (session?.CurrentMode == null || session.CurrentMode == SessionMode.Idle)
        return;
    
    var participant = await _sessionService.GetParticipantAsync(participantId);
    if (participant == null)
        return;
    
    if (session.CurrentMode == SessionMode.FFF)
    {
        var fffState = await _sessionService.GetCurrentFFFStateAsync(sessionId);
        if (fffState != null)
        {
            // Calculate remaining time
            var elapsed = (DateTime.UtcNow - fffState.StartTime).TotalMilliseconds;
            var remaining = Math.Max(0, fffState.TimeLimit - elapsed);
            var isStillActive = remaining > 0;
            
            // Determine if participant can submit answer
            // Only if they were connected BEFORE question started OR question already ended
            var canSubmit = participant.LastSeenAt <= fffState.StartTime || !isStillActive;
            
            // Check if already answered
            if (await _sessionService.HasParticipantAnsweredAsync(sessionId, participantId, fffState.QuestionId))
            {
                canSubmit = false;
            }
            
            await Clients.Caller.SendAsync("QuestionStarted", new
            {
                QuestionId = fffState.QuestionId,
                Question = fffState.QuestionText,
                Options = fffState.Options,
                TimeLimit = (int)remaining, // Send remaining time, not full time
                StartTime = fffState.StartTime,
                IsResync = true, // Flag to indicate this is a state sync
                CanSubmit = canSubmit, // NEW: Can this participant submit?
                SpectatorMode = !canSubmit && isStillActive, // NEW: Viewing only
                SpectatorReason = !canSubmit && isStillActive ? "You joined after the question started" : null
            });
            
            _logger.LogInformation("Synced FFF state to reconnected client for question {QuestionId} (CanSubmit: {CanSubmit})", 
                fffState.QuestionId, canSubmit);
        }
    }
    else if (session.CurrentMode == SessionMode.ATA)
    {
        var ataState = await _sessionService.GetCurrentATAStateAsync(sessionId);
        if (ataState != null)
        {
            // Calculate if voting is still active (60 second window)
            var elapsed = (DateTime.UtcNow - ataState.StartTime).TotalSeconds;
            var isStillActive = elapsed < 60 && ataState.IsActive;
            
            // Determine if participant can vote
            // Only if they were connected BEFORE voting started OR voting already ended
            var canVote = participant.LastSeenAt <= ataState.StartTime || !isStillActive;
            
            // Check if already voted
            if (await _sessionService.HasParticipantVotedAsync(sessionId, participantId))
            {
                canVote = false;
            }
            
            // Check if used ATA lifeline
            if (participant.HasUsedATA)
            {, string participantId)
{
    _logger.LogInformation("Client {ConnectionId} requested state sync for session {SessionId}", 
        Context.ConnectionId, sessionId);
    await SyncClientStateAsync(sessionId, participantnt and current results
            await Clients.Caller.SendAsync("ATAStarted", new
            {
                QuestionId = ataState.QuestionId,
                QuestionText = ataState.QuestionText,
                StartTime = ataState.StartTime,
                IsResync = true,
                CanVote = canVote, // NEW: Can this participant vote?
                SpectatorMode = !canVote && isStillActive, // NEW: Viewing only
                SpectatorReason = !canVote && isStillActive ? "You joined after voting started" : null
            });
            
            await Clients.Caller.SendAsync("VotesUpdated", new
            {
                Results = ataState.CurrentResults,
                TotalVotes = ataState.TotalVotes,
                Timestamp = DateTime.UtcNow
            });
            
            _logger.LogInformation("Synced ATA state to reconnected client for question {QuestionId} (CanVote: {CanVote})", 
                ataState.QuestionId, canVote
            await Clients.Caller.SendAsync("VotesUpdated", new
            {
                Results = ataState.CurrentResults,
                TotalVotes = ataState.TotalVotes,
                Timestamp = DateTime.UtcNow
            });
            
            _logger.LogInformation("Synced ATA state to reconnected client for question {QuestionId}", 
                ataState.QuestionId);
        }
    }
}

/// <summary>
/// New hub method for clients to explicitly request state sync
/// </summary>
public async Task RequestStateSync(string sessionId)
{
    _logger.LogInformation("Client {ConnectionId} requested state sync for session {SessionId}", 
        Context.ConnectionId, sessionId);
    await SyncClientStateAsync(sessionId);
}
```

#### Phase 2: Enhanced Session State Tracking

**1. Store Question Text in Session**

Problem: ATA question text is not stored in the session, only the question ID. For proper state sync, we need the full question details.

```csharp
// Session.cs - Add property
public string? CurrentQuestionText { get; set; }
public string[]? CurrentQuestionOptions { get; set; } // For FFF
```

**2. Update StartQuestion to Store Details**

```csharp
// GameHub.cs - Enhanced StartQuestion
public async Task StartQuestion(string sessionId, int questionId, string questionText, string[] options, int timeLimit = 20000)
{
    _logger.LogInformation("Starting FFF question {QuestionId} in session {SessionId}", questionId, sessionId);
    
    // ... cancel existing timer ...
    
    // Update session mode WITH question details
    await _sessionService.UpdateSessionModeAsync(sessionId, SessionMode.FFF, questionId, questionText, options);
    
    // ... rest of method ...
}
```

**3. Update SessionService Methods**

```csharp
// SessionService.cs - Enhanced method signature
public async Task UpdateSessionModeAsync(string sessionId, SessionMode mode, int? questionId, string? questionText = null, string[]? options = null)
{
    var session = await _context.Sessions.FindAsync(sessionId);
    if (session != null)
    {
        session.CurrentMode = mode;
        session.CurrentQuestionId = questionId;
        session.CurrentQuestionText = questionText;
        
        // Store options as JSON if provided
        if (options != null)
        {
            session.CurrentQuestionOptions = options;
        }
        
        // ... existing timing logic ...
        
        await _context.SaveChangesAsync();
    }
}
```

#### Phase 3: Client-Side Enhancements

**1. Handle IsResync Flag and Spectator Mode**

```javascript
// app.js - Enhanced QuestionStarted handler

connection.on("QuestionStarted", data => {
    console.log("FFF Question Started:", data);
    
    if (data.isResync) {
        console.log("⟳ Syncing to existing FFF question (reconnected)");
        
        if (data.spectatorMode) {
            // Joined late - can view but not participate
            console.warn("⚠ Spectator mode: " + data.spectatorReason);
            showTemporaryNotification("You joined late - viewing only", 3000);
            disableAnswerSubmission(); // Disable submit buttons
        } else if (!data.canSubmit) {
            // Already answered or other reason
            console.log("Cannot submit answer (already answered or inactive)");
            disableAnswerSubmission();
        } else {
            // Reconnected but can still participate!
            showTemporaryNotification("Rejoined active question - you can still answer!", 2000);
        }
    }
    
    // Update UI with question
    updateFFFQuestion(data);
    startFFFTimer(data.timeLimit); // Use remaining time, not full time
     (with participant ID for eligibility check)
    try {
        await connection.invoke("RequestStateSync", currentSessionId, currentParticipant
    
    showScreen('fffQuestionScreen');
});

function disableAnswerSubmission() {
    const submitBtn = document.getElementById('btnSubmitAnswer');
    if (submitBtn) {
        submitBtn.disabled = true;
        submitBtn.textContent = 'Viewing Only';
        submitBtn.classLis with Spectator Mode**

```javascript
// app.js - Enhanced ATAStarted handler

connection.on("ATAStarted", data => {
    console.log("ATA Started:", data);
    
    if (data.isResync) {
        console.log("⟳ Syncing to active ATA (reconnected)");
        
        if (data.spectatorMode) {
            // Joined late - can view results but not vote
            console.warn("⚠ Spectator mode: " + data.spectatorReason);
            showTemporaryNotification("You joined late - viewing results only", 3000);
            disableVoting(); // Disable vote buttons
        } else if (!data.canVote) {
            // Already voted or other reason
            console.log("Cannot vote (already voted or inactive)");
            disableVoting();
        } else {
            // Reconnected and can still vote!
            showTemporaryNotification("Rejoined voting - you can still participate!", 2000);
        }
    }
    
    // Show ATA voting screen
    showATAVotingUI(data);
    
    // Set voting buttons state based on eligibility
    setVotingButtonsState(data.canVote);
});

function disableVoting() {
    const voteButtons = document.querySelectorAll('.vote-button');
    voteButtons.forEach(btn => {
        btn.disabled = true;
        btn.classList.add('spectator-mode');
    });
    
    // Add visual indicator to ATA screen
    const spectatorBanner = document.createElement('div');
    spectatorBanner.id = 'ataSpectatorBanner';
    spectatorBanner.className = 'spectator-banner';
    spectatorBanner.textContent = '👁 Spectator Mode - You joined after voting started';
    document.querySelector('.ata-container')?.prepend(spectatorBanner);
}

function setVotingButtonsState(canVote) {
    const voteButtons = document.querySelectorAll('.vote-button');
    voteButtons.forEach(btn => { (returns state + IsActive flag)
  - [ ] Add `GetCurrentATAStateAsync()` method (returns state + IsActive flag)
  - [ ] Update `UpdateSessionModeAsync()` signature to accept question text and options
  - [ ] Store question details in session when starting questions

- [ ] **GameHub.cs**
  - [ ] Add `SyncClientStateAsync(sessionId, participantId)` private method
  - [ ] Update `JoinSession()` to call state sync with participant ID
  - [ ] Add `RequestStateSync(sessionId, participantId)` hub method
  - [ ] Update `StartQuestion()` to pass question details to SessionService
  - [ ] Add eligibility checking logic (compare participant.LastSeenAt vs question start time)
  - [ ] **Enhanced `SubmitAnswer()` - Server-side validation**:
    - [ ] Reject submissions if participant connected after question started
    - [ ] Return specific error: "Cannot submit - joined after question started"
    - [ ] **Reject submissions if received after timer expiry**
    - [ ] Calculate elapsed time: `submissionTime - session.QuestionStartTime`
    - [ ] If elapsed > timeLimit (20000ms), reject with error: "Cannot submit - time expired"
    - [ ] Log late submission attempts for monitoring
  - [ ] **Enhanced `SubmitVote()` - Server-side validation**:
    - [ ] Reject votes if participant connected after voting started
    - [ ] Return specific error: "Cannot vote - joined after voting started"
    - [ ] **Reject votes if received after 60-second timer expiry**
    - [ ] Same time validation as FFF answers

- [ ] **Models/GameState.cs** (new file)
  - [ ] Create `FFFQuestionState` class with `IsActive` property
  - [ ] Create `ATAQuestionState` class with `IsActive` property
  - [ ] Create `ParticipantEligibilityuest**

```javascript
// app.js - After reconnection

connection.onreconnected(async connectionId => {
    // ... existing rejoin logic ...
    , `canSubmit`, and `spectatorMode` flags
  - [ ] Update `ATAStarted` handler to handle `isResync`, `canVote`, and `spectatorMode` flags
  - [ ] Add `RequestStateSync(sessionId, participantId)` call in `onreconnected` handler
  - [ ] Add `disableAnswerSubmission()` function for FFF spectator mode
  - [ ] Add `disableVoting()` function for ATA spectator mode
  - [ ] Add `setSubmitButtonState(canSubmit)` function
  - [ ] Add `setVotingButtonsState(canVote)` function
  - [ ] Add spectator banner UI elements
  - [ ] Add UI notificatio - BEFORE Timer Starts**
   - [ ] Select 8 players for FFF
   - [ ] One player disconnects before "Reveal Answers" is clicked
   - [ ] Player reconnects
   - [ ] Host clicks "Reveal Answers" to start timer
   - [ ] **Expected**: Reconnected player CAN submit answer (joined before timer)

2. **FFF Late Reconnection Test - DURING Active Timer**
   - [ ] Start FFF question with 8 players (answers revealed, timer running)
   - [ ] Have one player disconnect/close browser at 15 seconds remaining
   - [ ] Player reconnects at 8 seconds remaining
   - [ ] **Expected**: Player sees question with remaining time in **SPECTATOR MODE** (cannot submit, button disabled, banner shown)

3. **FFF Post-Question Reconnection Test**
   - [ ] Complete FFF question (timer expired or all answered)
   - [ ] Player reconnects after results shown
   - [ ] **Expected**: Player sees results screen, can participate in next round

4. **ATA Spectator Mode Test - Late Join**
   - [ ] Start ATA voting (60-second timer)
   - [ ] New player joins session at 40 seconds remaining
   - [ ] **Expected**: Player sees voting UI and current percentages in **SPECTATOR MODE** (cannot vote, buttons disabled)

5. **ATA Early Join Test - Before Voting Starts**
   - [ ] Player joins session
   - [ ] Host starts ATA voting 5 seconds later
   - [ ] **Expected**: Player CAN vote (joined before voting started)

6. **Multiple Disconnects Test with Mixed Eligibility**
   - [ ] Have 5 players connected
   - [ ] Start FFF question (timer running)
   - [ ] 2 players disconnect at 12s remaining, 1 disconnects at 5s remaining
   - [ ] All 3 reconnect at different times during question
   - [ ] **Expected**: All 3 see spectator mode (joined after timer started)

7. **Server-Side Validation Test (Security)**
   - [ ] Player joins during active FFF question (gets spectator mode)
   - [ ] Player attempts to submit answer via browser console/API directly
   - [ ] **Expected**: Server rejects submission with error "Cannot submit - joined after question started"

8. **State Cleanup Test**
   - [ ] End FFF question normally
   - [ ] New player joins after question ended (session in Idle mode)
   - [ ] **Expected**: Player sees lobby, no spectator mode, ready for next round

9. **ATA Already Voted Test**
   - [ ] Player votes in ATA
   - [ ] Player disconnects and reconnects during same voting window
   - [ ] **Expected**: Player sees "Already Voted" message, cannot vote again

10. **Server Restart Test**
    - [ ] Start FFF question with players
    - [ ] Restart web server (simulates crash)
    - [ ] Players reconnect
    - [ ] **Expected**: Graceful degradation, session state lost, players see lobby

11. **Timer Expiry - No Auto-Submit Test (CRITICAL)**
    - [ ] Player is in FFF question with answers arranged (e.g., "DCBA")
    - [ ] Player does NOT click Submit button
    - [ ] Timer counts down to 0:00 and expires
    - [ ] **Expected**: 
      - Submit button is disabled
      - "Time Expired" message shown
      - NO answer is submitted to server
      - Player is NOT included in rankings/results
      - Player sees message: "You did not submit in time"

12. **Explicit Submit Only Test**
    - [ ] Player arranges answer sequence
    - [ ] Player clicks Submit button at 5 seconds remaining
    - [ ] **Expected**: Answer is recorded with correct timestamp
    - [ ] Player receives confirmation: "Answer submitted!"

13. **Late Submission Attempt Test (Server-Side)**
    - [ ] Player arranges answer during question
    - [ ] Timer expires (client shows "Time's Up")
    - [ ] Malicious user tries to submit via browser console after expiry
    - [ ] **Expected**: Server rejects with error "Cannot submit - time expired"

14. **Timer Expiry During Submission Test**
    - [ ] Player clicks Submit at 0.5 seconds remaining
    - [ ] Network delay causes submission to arrive after expiry
    - [ ] **Expected**: Server checks submission timestamp vs question start + limit
    - [ ] If within grace period (network latency ~200-500ms), accept
    - [ ] If clearly late (>1s after expiry), reject
    
    // Show ATA voting screen
    showATAVotingUI(data);
});
```

## Database Schema Changes

### Required Migrations

Add columns to `Sessions` table:

```sql
ALTER TABLE Sessions ADD CurrentQuestionText NVARCHAR(500) NULL;
ALTER TABLE Sessions ADD CurrentQuestionOptionsJson NVARCHAR(MAX) NULL; -- JSON array of options
```

Or in Entity Framework migration:

```csharp
migrationBuilder.AddColumn<string>(
    name: "CurrentQuestionText",
    table: "Sessions",
    type: "nvarchar(500)",
    nullable: true);

migrationBuilder.AddColumn<string>(
    name: "CurrentQuestionOptionsJson",
    table: "Sessions",
    type: "nvarchar(max)",
    nullable: true);

### Timer Expiry and Late Submissions

- **Issue**: Client might try to submit after timer expires (network lag or malicious)
- **Solution**: Server validates submission timing:
  - Calculate `elapsed = submissionTime - session.QuestionStartTime`
  - Reject if `elapsed > timeLimit + graceperiod`
  - Grace period: 500ms (allows for network latency)
  - Return error: "Cannot submit - time expired"
- **Client Behavior**: No auto-submission, disable button when timer hits 0
- **Data Integrity**: Only participants who clicked Submit within time limit are ranked

### Client-Side Timer Management

- **Issue**: How to handle timer expiry without auto-submission?
- **Solution**: 
  - Timer countdown updates UI every 100ms
  - At 0:00, trigger `onTimerExpired()` function:
    - Disable submit button immediately
    - Show "Time's Up!" overlay
    - Keep arranged answer visible (for user to see what they had)
    - Do NOT call `submitAnswer()` automatically
  - Only explicit button clicks call `submitAnswer()`
```

## Implementation Checklist

### Backend Changes

- [ ] **SessionService.cs**
  - [ ] Add `GetCurrentFFFStateAsync()` method
  - [ ] Add `GetCurrentATAStateAsync()` method
  - [ ] Update `UpdateSessionModeAsync()` signature to accept question text and options
  - [ ] Store question details in session when starting questions

- [ ] **GameHub.cs**
  - [ ] Add `SyncClientStateAsync()` private method+ connection timing determines permissions

### Late Joiner Eligibility Tracking

- **Issue**: How to determine if participant connected before or after question started?
- **Solution**: Compare `participant.LastSeenAt` (updated on join/reconnect) with `session.QuestionStartTime`:
  - If `LastSeenAt <= QuestionStartTime`: Connected before → Can participate
  - If `LastSeenAt > QuestionStartTime`: Connected after → Spectator only
  - If question not active: Can participate in next round

### Server-Side Security Validation

- **Issue**: Malicious clients might try to bypass client-side spectator restrictions
- **Solution**: Server must validate eligibility in `SubmitAnswer()` and `SubmitVote()`:
  - Check participant connection time vs question start time
  - Return specific error if submission rejected
  - Log security violations for monitoring
  - [ ] Update `JoinSession()` to call state sync
  - [ ] Add `RequestStateSync()` hub method
  - [ ] Update `StartQuestion()` to pass question details to SessionService
  - [ ] Update `OnConnectedAsync()` to sync state for reconnecting clients

- [ ] **Models/GameState.cs** (new file)
  - [ ] Create `FFFQuestionState` class
  - [ ] Create `ATAQuestionState` class

- [ ] **Session.cs**
  - [ ] Add `CurrentQuestionText` property
  - [ ] Add `CurrentQuestionOptionsJson` property (store as JSON)

- [ ] **Database Migration**
  - [ ] Create migration for new Session columns
  - [ ] Apply migration to development database
  - [ ] Test with existing data

### Frontend Changes

- [ ] **app.js**
  - [ ] Update `QuestionStarted` handler to handle `isResync`, `canSubmit`, and `spectatorMode` flags
  - [ ] Update `ATAStarted` handler to handle `isResync`, `canVote`, and `spectatorMode` flags
  - [ ] Add `RequestStateSync(sessionId, participantId)` call in `onreconnected` handler
  - [ ] Add `disableAnswerSubmission()` function for FFF spectator mode
  - [ ] Add `disableVoting()` function for ATA spectator mode
  - [ ] Add `setSubmitButtonState(canSubmit)` function
  - [ ] Add `setVotingButtonsState(canVote)` function
  - [ ] Add spectator banner UI elements
  - [ ] Add UI notifications for "spectator mode" vs "can still participate"
  - [ ] Handle remaining time vs full time in FFF timer
  - [ ] **FIX: Remove auto-submission on timer expiry**
    - [ ] Remove any code that calls `submitAnswer()` when timer reaches 0
    - [ ] Timer expiry should disable submit button and show "Time's Up!" message
    - [ ] No answer submission unless user explicitly clicked Submit button
  - [ ] **Add timer expiry handling**:
    - [ ] When timer hits 0:00, disable submit button immediately
    - [ ] Show "Time Expired" message to user
    - [ ] Keep arranged answer visible but prevent submission

- [ ] **CSS (styles.css or inline)**
  - [ ] Add `.spectator-banner` style (visible banner at top of screen)
  - [ ] Add `.spectator-mode` button style (grayed out with icon)
  - [ ] Add `.disabled-mode` button style
  - [ ] Add `.time-expired` style for expired state indication

### Testing Scenarios

1. **FFF Reconnection Test - BEFORE Timer Starts**
   - [ ] Select 8 players for FFF
   - [ ] One player disconnects before "Reveal Answers" is clicked
   - [ ] Player reconnects
   - [ ] Host clicks "Reveal Answers" to start timer
   - [ ] **Expected**: Reconnected player CAN submit answer (joined before timer)

2. **FFF Late Reconnection Test - DURING Active Timer**
   - [ ] Start FFF question with 8 players (answers revealed, timer running)
   - [ ] Have one player disconnect/close browser at 15 seconds remaining
   - [ ] Player reconnects at 8 seconds remaining
   - [ ] **Expected**: Player sees question with remaining time in **SPECTATOR MODE** (cannot submit, button disabled, banner shown)

3. **FFF Post-Question Reconnection Test**
   - [ ] Complete FFF question (timer expired or all answered)
   - [ ] Player reconnects after results shown
   - [ ] **Expected**: Player sees results screen, can participate in next round

4. **ATA Spectator Mode Test - Late Join**
   - [ ] Start ATA voting (60-second timer)
   - [ ] New player joins session at 40 seconds remaining
   - [ ] **Expected**: Player sees voting UI and current percentages in **SPECTATOR MODE** (cannot vote, buttons disabled)

5. **ATA Early Join Test - Before Voting Starts**
   - [ ] Player joins session
   - [ ] Host starts ATA voting 5 seconds later
   - [ ] **Expected**: Player CAN vote (joined before voting started)

6. **Multiple Disconnects Test with Mixed Eligibility**
   - [ ] Have 5 players connected
   - [ ] Start FFF question (timer running)
   - [ ] 2 players disconnect at 12s remaining, 1 disconnects at 5s remaining
   - [ ] All 3 reconnect at different times during question
   - [ ] **Expected**: All 3 see spectator mode (joined after timer started)

7. **Server-Side Validation Test (Security)**
   - [ ] Player joins during active FFF question (gets spectator mode)
   - [ ] Player attempts to submit answer via browser console/API directly
   - [ ] **Expected**: Server rejects submission with error "Cannot submit - joined after question started"

8. **State Cleanup Test**
   - [ ] End FFF question normally
   - [ ] New player joins after question ended (session in Idle mode)
   - [ ] **Expected**: Player sees lobby, no spectator mode, ready for next round

9. **ATA Already Voted Test**
   - [ ] Player votes in ATA
   - [ ] Player disconnects and reconnects during same voting window
   - [ ] **Expected**: Player sees "Already Voted" message, cannot vote again

10. **Server Restart Test**
    - [ ] Start FFF question with players
    - [ ] Restart web server (simulates crash)
    - [ ] Players reconnect
    - [ ] **Expected**: Graceful degradation, session state lost, players see lobby

11. **Timer Expiry - No Auto-Submit Test (CRITICAL)**
    - [ ] Player is in FFF question with answers arranged (e.g., "DCBA")
    - [ ] Player does NOT click Submit button
    - [ ] Timer counts down to 0:00 and expires
    - [ ] **Expected**: 
      - Submit button is disabled
      - "Time Expired" message shown
      - NO answer is submitted to server
      - Player is NOT included in rankings/results
      - Player sees message: "You did not submit in time"

12. **Explicit Submit Only Test**
    - [ ] Player arranges answer sequence
    - [ ] Player clicks Submit button at 5 seconds remaining
    - [ ] **Expected**: Answer is recorded with correct timestamp
    - [ ] Player receives confirmation: "Answer submitted!"

13. **Late Submission Attempt Test (Server-Side)**
    - [ ] Player arranges answer during question
    - [ ] Timer expires (client shows "Time's Up")
    - [ ] Malicious user tries to submit via browser console after expiry
    - [ ] **Expected**: Server rejects with error "Cannot submit - time expired"

14. **Timer Expiry During Submission Test**
    - [ ] Player clicks Submit at 0.5 seconds remaining
    - [ ] Network delay causes submission to arrive after expiry
    - [ ] **Expected**: Server checks submission timestamp vs question start + limit
    - [ ] If within grace period (network latency ~200-500ms), accept
    - [ ] If clearly late (>1s after expiry), reject

## Edge Cases & Considerations

### Time Synchronization

- **Issue**: Clients may have clock skew
- **Solution**: Server sends `StartTime` and `TimeLimit`, client calculates remaining time locally
- **Alternative**: Send `RemainingTime` calculated server-side for precision

### Duplicate Question Starts

- **Issue**: Client might receive both live `QuestionStarted` and resync `QuestionStarted`
- **Solution**: Client checks if already on question screen, updates timer but doesn't reset UI

### Session State Cleanup

- **Issue**: When does `CurrentQuestionText` get cleared?
- **Solution**: Clear when `EndQuestion` is called and mode set to `Idle`

### ATA Question Text Storage

- **Issue**: ATA currently uses placeholder question text
- **Solution**: 
  - Option A: Store actual question text when starting ATA (requires main app to pass it)
  - Option B: Use generic text "Vote for the best answer" for ATA

### Participant State vs Session State

- **Issue**: Participant has individual state (SelectedForFFF, etc.), session has global state
- **Solution**: Sync both - session state determines UI screen, participant state determines permissions

## Performance Considerations

### Database Impact

- Each reconnection will query session state (minimal impact)
- Consider caching session state in-memory for active sessions
- Clear cache when question ends

### SignalR Overhead

- Each reconnection sends full state (acceptable for occasional reconnects)
- Avoid sending state to all clients, only caller
- Consider adding state version number to detect stale clients

## Alternative Approaches Considered

### 1. Client State Management Only
- **Pros**: No server changes needed
- **Cons**: Client state can become stale, no source of truth

### 2. Periodic State Polling
- **Pros**: Always in sync
- **Cons**: Unnecessary network traffic, battery drain on mobile

### 3. Event Replay
- **Pros**: Complete history, can reconstruct any state
- **Cons**: Complex, storage intensive, potential replay issues

**Selected Approach: State Sync on Reconnect** is the optimal balance of reliability, performance, and complexity.

## Rollout Plan

### Development Phase (Week 1)
1. Day 1-2: Backend state retrieval methods
2. Day 3: Database migration and testing
3. Day 4: GameHub state sync integration
4. Day 5: Frontend handlers and testing

### Testing Phase (Week 2)
1. Unit tests for state retrieval
2. Integration tests for reconnection scenarios
3. Load testing with multiple reconnects
4. Mobile device testing (sleep/wake cycles)

### Deployment
1. Deploy to staging environment
2. Run automated test suite
3. Manual testing with real devices
4. Deploy to production
5. Monitor logs for state sync metrics

## Success Metrics

- **Zero missed state syncs**: All reconnecting clients receive current state
- **<500ms sync time**: State delivered within half second of reconnection
- **No duplicate submissions**: Participants can't submit multiple answers via reconnect tricks
- **Battery friendly**: No continuous polling, only on-demand state requests

## Future Enhancements

1. **State History**: Store last N state changes for debugging
2. **State Compression**: For large states, use compressed formats
3. **Differential Updates**: Send only changed fields, not full state
4. **Client State Validation**: Server validates client state matches server
5. **Predictive Pre-sync**: Pre-send state before client explicitly joins

## Related Documentation

- [FFF Online Flow](archive/FFF_ONLINE_FLOW_DOCUMENT.md)
- [Web Server Standalone Deployment](archive/DEPLOYMENT_WEB_STANDALONE.md)
- [Phase 5.1 Complete](archive/phases/PHASE_5.1_COMPLETE.md)

---

**Document Status**: Planning Phase  
**Last Updated**: 2026-01-08  
**Author**: Development Team  
**Priority**: High - Critical for user experience
