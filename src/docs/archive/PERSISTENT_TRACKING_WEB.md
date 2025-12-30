# WAPS Persistent User Tracking

## Overview
WAPS implements **persistent participant tracking using GUIDs** that survive disconnections, page refreshes, and browser sessions. This ensures:
- ✅ Users cannot vote/answer multiple times
- ✅ Users maintain their progress in FFF if disconnected
- ✅ Clean reconnection without creating duplicate participants
- ✅ Server-side validation prevents cheating

---

## Architecture

### Client-Side (Browser)
```javascript
// Persistent storage using localStorage
const STORAGE_KEYS = {
    PARTICIPANT_ID: 'waps_participant_id',  // User's unique GUID
    SESSION_ID: 'waps_session_id',          // Current game session
    DISPLAY_NAME: 'waps_display_name'       // User's display name
};
```

**Flow:**
1. **First Join**: Generate new participant, receive GUID from server, save to localStorage
2. **Reconnect**: Send stored GUID to server, update connection ID
3. **Page Refresh**: Auto-reconnect with stored GUID
4. **Leave Session**: Clear localStorage

### Server-Side (ASP.NET Core)

**Participant Model:**
```csharp
public class Participant
{
    public string Id { get; set; }           // Persistent GUID (never changes)
    public string SessionId { get; set; }     // Which game session
    public string DisplayName { get; set; }   // User's name
    public string? ConnectionId { get; set; } // Current SignalR connection (changes on reconnect)
    public DateTime JoinedAt { get; set; }    // First join timestamp
    public DateTime? LastSeenAt { get; set; } // Last activity
    public bool IsActive { get; set; }        // Currently in session
}
```

**Key Methods:**

1. **GetOrCreateParticipantAsync** - Smart participant management
   ```csharp
   public async Task<Participant> GetOrCreateParticipantAsync(
       string sessionId, 
       string displayName, 
       string connectionId, 
       string? participantId = null)
   {
       // If participantId provided, find existing participant
       if (!string.IsNullOrEmpty(participantId))
       {
           var existing = await FindParticipant(participantId, sessionId);
           if (existing != null)
           {
               // Reconnection: Update connection ID only
               existing.ConnectionId = connectionId;
               existing.LastSeenAt = DateTime.UtcNow;
               existing.IsActive = true;
               return existing;
           }
       }
       
       // New participant: Create with new GUID
       return await CreateNewParticipant(sessionId, displayName, connectionId);
   }
   ```

2. **HasParticipantAnsweredAsync** - Prevent duplicate FFF submissions
   ```csharp
   public async Task<bool> HasParticipantAnsweredAsync(
       string sessionId, 
       string participantId, 
       int questionId)
   {
       return await _context.FFFAnswers
           .AnyAsync(a => a.SessionId == sessionId && 
                         a.ParticipantId == participantId && 
                         a.QuestionId == questionId);
   }
   ```

3. **HasParticipantVotedAsync** - Prevent duplicate ATA votes
   ```csharp
   public async Task<bool> HasParticipantVotedAsync(
       string sessionId, 
       string participantId)
   {
       // Check for votes in last 5 minutes (current question)
       var recentVoteCutoff = DateTime.UtcNow.AddMinutes(-5);
       return await _context.ATAVotes
           .AnyAsync(v => v.SessionId == sessionId && 
                         v.ParticipantId == participantId &&
                         v.SubmittedAt > recentVoteCutoff);
   }
   ```

---

## SignalR Hub Integration

### FFFHub Example
```csharp
public async Task<object> JoinSession(
    string sessionId, 
    string displayName, 
    string? participantId = null)  // Optional: for reconnection
{
    await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
    
    // GetOrCreateParticipantAsync handles new vs. reconnecting users
    var participant = await _sessionService.GetOrCreateParticipantAsync(
        sessionId, displayName, Context.ConnectionId, participantId);
    
    // Return participant ID to client for localStorage
    return new
    {
        ParticipantId = participant.Id,
        DisplayName = participant.DisplayName,
        SessionId = sessionId
    };
}

public async Task<object> SubmitAnswer(
    string sessionId, 
    string participantId,  // Client sends their stored GUID
    int questionId, 
    string answerSequence)
{
    // Server-side validation: Check for duplicate submission
    if (await _sessionService.HasParticipantAnsweredAsync(sessionId, participantId, questionId))
    {
        return new { Success = false, Error = "You have already submitted an answer" };
    }
    
    // Save answer
    await _sessionService.SaveFFFAnswerAsync(sessionId, participantId, questionId, answerSequence);
    
    return new { Success = true };
}
```

---

## Client Implementation

### JavaScript Example
```javascript
let currentParticipantId = null;
let connection = null;

// On page load: Check for stored participant ID
window.addEventListener('DOMContentLoaded', async () => {
    const storedParticipantId = localStorage.getItem('waps_participant_id');
    const storedSessionId = localStorage.getItem('waps_session_id');
    const storedDisplayName = localStorage.getItem('waps_display_name');
    
    if (storedParticipantId && storedSessionId && storedDisplayName) {
        // Auto-reconnect with existing participant ID
        await joinSession(storedSessionId, storedDisplayName, storedParticipantId);
    }
});

// Join session (new or reconnect)
async function joinSession(sessionId, displayName, participantId = null) {
    connection = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/fff')
        .withAutomaticReconnect()  // SignalR auto-reconnect
        .build();
    
    await connection.start();
    
    // Join with optional participant ID
    const result = await connection.invoke('JoinSession', sessionId, displayName, participantId);
    
    // Save participant ID for future reconnections
    currentParticipantId = result.participantId;
    localStorage.setItem('waps_participant_id', result.participantId);
    localStorage.setItem('waps_session_id', result.sessionId);
    localStorage.setItem('waps_display_name', result.displayName);
}

// Submit FFF answer
async function submitAnswer(questionId, answerSequence) {
    // Send participant ID with submission
    const result = await connection.invoke('SubmitAnswer', 
        sessionId, 
        currentParticipantId,  // Persistent GUID
        questionId, 
        answerSequence);
    
    if (!result.success) {
        alert(result.error);  // "You have already submitted an answer"
    }
}

// Submit ATA vote
async function submitVote(questionText, selectedOption) {
    const result = await connection.invoke('SubmitVote', 
        sessionId, 
        currentParticipantId,  // Persistent GUID
        questionText, 
        selectedOption);
    
    if (!result.success) {
        alert(result.error);  // "You have already voted"
    }
}
```

---

## Database Schema

### Participants Table
```sql
CREATE TABLE "Participants" (
    "Id" TEXT PRIMARY KEY,              -- Persistent GUID
    "SessionId" TEXT NOT NULL,
    "DisplayName" TEXT NOT NULL,
    "ConnectionId" TEXT,                -- Current SignalR connection (nullable, changes on reconnect)
    "JoinedAt" DATETIME NOT NULL,
    "LastSeenAt" DATETIME,
    "IsActive" INTEGER NOT NULL,
    FOREIGN KEY ("SessionId") REFERENCES "Sessions" ("Id")
);
```

### FFFAnswers Table
```sql
CREATE TABLE "FFFAnswers" (
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT,
    "SessionId" TEXT NOT NULL,
    "ParticipantId" TEXT NOT NULL,      -- Links to Participant.Id (persistent GUID)
    "QuestionId" INTEGER NOT NULL,
    "AnswerSequence" TEXT NOT NULL,
    "SubmittedAt" DATETIME NOT NULL,
    "TimeElapsed" REAL NOT NULL,
    "IsCorrect" INTEGER NOT NULL,
    "Rank" INTEGER,
    FOREIGN KEY ("SessionId") REFERENCES "Sessions" ("Id"),
    FOREIGN KEY ("ParticipantId") REFERENCES "Participants" ("Id")
);

-- Prevent duplicate answers
CREATE UNIQUE INDEX "IX_FFFAnswers_Unique" ON "FFFAnswers" ("SessionId", "ParticipantId", "QuestionId");
```

### ATAVotes Table
```sql
CREATE TABLE "ATAVotes" (
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT,
    "SessionId" TEXT NOT NULL,
    "ParticipantId" TEXT NOT NULL,      -- Links to Participant.Id (persistent GUID)
    "QuestionText" TEXT NOT NULL,
    "SelectedOption" TEXT NOT NULL,
    "SubmittedAt" DATETIME NOT NULL,
    FOREIGN KEY ("SessionId") REFERENCES "Sessions" ("Id"),
    FOREIGN KEY ("ParticipantId") REFERENCES "Participants" ("Id")
);

-- Prevent recent duplicate votes (5-minute window)
CREATE INDEX "IX_ATAVotes_Recent" ON "ATAVotes" ("SessionId", "ParticipantId", "SubmittedAt");
```

---

## Scenarios & Solutions

### Scenario 1: User Disconnects Mid-FFF
**Problem**: User loses connection while FFF question is active  
**Solution**:
1. Server marks `LastSeenAt` but keeps `IsActive = true`
2. Client auto-reconnects with stored `participantId`
3. Server updates `ConnectionId` in existing participant record
4. User can continue where they left off
5. If they already submitted, duplicate check blocks re-submission

### Scenario 2: User Refreshes Page
**Problem**: Page refresh creates new connection  
**Solution**:
1. On page load, check `localStorage` for `waps_participant_id`
2. If found, auto-join with stored participant ID
3. Server recognizes reconnection and updates connection ID
4. User's previous state (votes, answers) is preserved

### Scenario 3: User Tries to Vote Twice
**Problem**: User closes tab and rejoins to vote again  
**Solution**:
1. Client sends stored `participantId` when voting
2. Server calls `HasParticipantVotedAsync(sessionId, participantId)`
3. Database query checks for existing vote (last 5 minutes)
4. If found, return error: `"You have already voted"`
5. Vote rejected before reaching database

### Scenario 4: User Clears Browser Data
**Problem**: localStorage cleared, participant ID lost  
**Solution**:
1. User joins as "new" participant
2. Server creates new participant record with new GUID
3. Previous participant remains in database (for history)
4. User can participate again (legitimate new session)

### Scenario 5: Multiple Devices
**Problem**: User joins from phone, then switches to laptop  
**Solution**:
1. Each device stores its own `participantId` in localStorage
2. Option A: Each device = separate participant (default)
3. Option B: Share participant ID via QR code or link (advanced feature)

---

## Security Considerations

### 1. Server-Side Validation
**Always validate on server**, never trust client:
```csharp
// ❌ BAD: Trust client's claim they haven't voted
public async Task SubmitVote(string selectedOption) { ... }

// ✅ GOOD: Verify on server with participant ID
public async Task<object> SubmitVote(string participantId, string selectedOption)
{
    if (await _sessionService.HasParticipantVotedAsync(sessionId, participantId))
    {
        return new { Success = false, Error = "Already voted" };
    }
    // Proceed...
}
```

### 2. ConnectionId vs ParticipantId
**Never use ConnectionId for identity**:
- ❌ `ConnectionId` changes on every reconnect
- ✅ `ParticipantId` (GUID) is persistent

### 3. Time-Based Validation
**ATA votes use 5-minute window**:
```csharp
// Only check votes from current question (last 5 minutes)
var recentVoteCutoff = DateTime.UtcNow.AddMinutes(-5);
return await _context.ATAVotes
    .AnyAsync(v => v.ParticipantId == participantId && 
                   v.SubmittedAt > recentVoteCutoff);
```

This allows new ATA questions without clearing old votes.

### 4. Session Scope
**All checks are session-scoped**:
```csharp
// ✅ User can answer FFF in multiple sessions
return await _context.FFFAnswers
    .AnyAsync(a => a.SessionId == sessionId &&      // Session scope
                   a.ParticipantId == participantId && 
                   a.QuestionId == questionId);
```

---

## Testing the System

### Test 1: Basic Join & Reconnect
1. Open browser: http://localhost:5278/?session=YOUR_SESSION_ID
2. Enter name and join
3. Note participant ID in console/UI
4. Refresh page → Auto-reconnects with same participant ID
5. Check server logs: "Participant {ID} reconnected"

### Test 2: Duplicate Vote Prevention
1. Join session and vote in ATA
2. Try to vote again → Error: "You have already voted"
3. Open DevTools → Check localStorage for `waps_participant_id`
4. Clear localStorage → Can vote again (new participant)

### Test 3: Disconnect & Reconnect
1. Join session
2. Open browser DevTools → Network tab
3. Disable network (simulate disconnect)
4. Wait 5 seconds
5. Enable network → SignalR auto-reconnects
6. Same participant ID maintained

### Test 4: Multiple Sessions
1. Create two sessions (Session A, Session B)
2. Join both with same device/browser
3. Participant can vote in both (different sessions)
4. localStorage stores latest session info

---

## Monitoring & Debugging

### Server Logs
```
info: Participant TestUser (abc-123-def) joined session xyz-789 (Reconnect: False)
info: Participant TestUser (abc-123-def) reconnected to session xyz-789 with new connection ghi-456
info: Duplicate FFF answer blocked - Session: xyz-789, Participant: abc-123-def, Question: 5
info: Duplicate ATA vote blocked - Session: xyz-789, Participant: abc-123-def
```

### Client Console Debugging
```javascript
console.log('Stored Participant ID:', localStorage.getItem('waps_participant_id'));
console.log('Current Connection ID:', connection.connectionId);
console.log('SignalR State:', connection.state);
```

### Database Queries
```sql
-- Check for duplicate participants (shouldn't happen)
SELECT DisplayName, COUNT(*) as Count
FROM Participants
WHERE SessionId = 'your-session-id'
GROUP BY DisplayName
HAVING COUNT(*) > 1;

-- Check participant's answers
SELECT * FROM FFFAnswers WHERE ParticipantId = 'participant-guid';

-- Check participant's votes
SELECT * FROM ATAVotes WHERE ParticipantId = 'participant-guid';
```

---

## Future Enhancements

### 1. Multi-Device Sync
Allow users to join from multiple devices with same identity:
- Generate QR code with participant ID
- Scan QR on second device
- Both devices share same participant record

### 2. Participant Analytics
Track participant behavior:
- Join time vs. answer time
- Answer accuracy history
- Voting patterns
- Reconnection frequency

### 3. Admin Dashboard
Host can see:
- Active vs. disconnected participants
- Who has answered/voted
- Duplicate attempt logs
- Connection health metrics

### 4. Kick/Ban Feature
Host can:
- Deactivate specific participant IDs
- Block by localStorage check on client
- Maintain ban list per session

---

## Summary

✅ **Persistent GUID Tracking Implemented**
- Client stores participant ID in localStorage
- Server maintains participant records across reconnections
- Duplicate submissions prevented server-side
- FFF progress maintained through disconnects

✅ **Security**
- Server-side validation for all submissions
- Time-based vote windows for ATA
- Session-scoped identity checks
- Never trust client-side state

✅ **User Experience**
- Seamless reconnection
- Auto-join on page refresh
- Clear error messages for duplicates
- No manual re-entry required

**Next Steps**: Test with multiple clients, add more FFF/ATA functionality, implement leaderboard with persistent participant tracking.
