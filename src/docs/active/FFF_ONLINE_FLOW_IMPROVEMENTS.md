# FFF Online Web Client Flow Improvements

**Status:** 🔄 IN PROGRESS
**Priority:** HIGH
**Date:** December 2025

---

## Current Issues
After timer expires, the question with response stays on screen with no feedback to participants.

## Desired Flow

### 1. Reveal Answers Start
- **Current**: Working correctly - participants see question and submit answers
- **No changes needed**

### 2. Timer Expires
- **New Message**: `TimerExpired`
- **Web Client Display**:
  ```
  Results are being calculated.
  Please Stand By.
  The Host will announce the winners shortly.
  ```
- **Implementation Location**: FFFControlPanel.cs - Timer expiry handler (line ~901)

### 3. Reveal Correct Clicked (1-4 times)
- **New Message**: After 4th click only - `RevealingWinner`
- **Web Client Display**:
  ```
  And the winner is...
  ```
- **Implementation Location**: FFFControlPanel.cs - btnRevealCorrect_Click handler (line ~916)
- **Trigger**: When `_revealCorrectClickCount == 4`

### 4. Show Winners Clicked/Skipped
- **Current**: No change to web client needed
- **Note**: This step is for TV screen only

### 5. Confirm Winner Clicked
- **New Message**: `WinnerConfirmed` with participant outcome
- **Web Client Display** (personalized per participant):
  - **Winner**: "Congratulations! You are the next contestant! Come up to the stage!"
  - **Correct but too slow**: "Sorry! You weren't fast enough, but you can try again later!"
  - **Incorrect**: "Sorry! Your answer was incorrect, but you can try again later!"
- **Implementation Location**: FFFControlPanel.cs - btnConfirmWinner_Click handler
- **Data Required**: Winner ID, all correct participant IDs

### 6. No Winner Scenario
- **New Message**: `NoWinner`
- **Web Client Display**:
  ```
  No winners! You get to try again! Get ready to answer the next one!
  ```
- **Implementation Location**: FFFControlPanel.cs - No winner path in ranking calculation
- **Stays until**: Reveal Answers Start pressed again

### 7. FFF Round Complete & Window Close
- **New Message**: `ResetToLobby`
- **Web Client**: Returns to connectedScreen
- **Implementation Location**: FFFWindow.cs - FormClosing event

## Implementation Tasks

### Backend Changes (C#)

#### 1. FFFControlPanel.cs - Timer Expiry
```csharp
// In _fffTimer_Elapsed after line 901
await SendFFFMessageAsync("TimerExpired", new { });
```

#### 2. FFFControlPanel.cs - Reveal Complete
```csharp
// In btnRevealCorrect_Click after 4th click
if (_revealCorrectClickCount == 4)
{
    await SendFFFMessageAsync("RevealingWinner", new { });
}
```

####  3. FFFControlPanel.cs - Confirm Winner
```csharp
// In btnConfirmWinner_Click
var winner = _rankings.FirstOrDefault(r => r.Rank == 1 && r.IsCorrect);
var correctParticipants = _rankings.Where(r => r.IsCorrect).Select(r => r.ParticipantId).ToList();

await SendFFFMessageAsync("WinnerConfirmed", new
{
    WinnerId = winner?.ParticipantId,
    CorrectParticipants = correctParticipants
});
```

#### 4. FFFControlPanel.cs - No Winner
```csharp
// In CalculateRankings or Show Winners check
if (!_rankings.Any(r => r.IsCorrect))
{
    await SendFFFMessageAsync("NoWinner", new { });
}
```

#### 5. FFFWindow.cs - Window Close
```csharp
// In FormClosing event
await SendFFFMessageAsync("ResetToLobby", new { });
```

#### 6. Create SendFFFMessageAsync Helper
```csharp
private async Task SendFFFMessageAsync(string messageType, object data)
{
    if (_fffService != null)
    {
        await _fffService.BroadcastToParticipantsAsync(_sessionId, messageType, data);
    }
}
```

### Frontend Changes (JavaScript)

#### 1. Add Message Handlers in app.js
```javascript
connection.on('TimerExpired', handleTimerExpired);
connection.on('RevealingWinner', handleRevealingWinner);
connection.on('WinnerConfirmed', handleWinnerConfirmed);
connection.on('NoWinner', handleNoWinner);
connection.on('ResetToLobby', handleResetToLobby);
```

#### 2. Implement Handlers
```javascript
function handleTimerExpired() {
    showScreen('fffWaitingScreen'); // New screen
}

function handleRevealingWinner() {
    document.getElementById('fffWaitingMessage').textContent = 'And the winner is...';
}

function handleWinnerConfirmed(data) {
    const myId = getParticipantId();
    let message;
    
    if (data.WinnerId === myId) {
        message = 'Congratulations! You are the next contestant! Come up to the stage!';
    } else if (data.CorrectParticipants.includes(myId)) {
        message = "Sorry! You weren't fast enough, but you can try again later!";
    } else {
        message = "Sorry! Your answer was incorrect, but you can try again later!";
    }
    
    showResultScreen(message);
}

function handleNoWinner() {
    showResultScreen('No winners! You get to try again! Get ready to answer the next one!');
}

function handleResetToLobby() {
    showScreen('connectedScreen');
}
```

#### 3. Add New HTML Screens in index.html
```html
<!-- FFF Waiting Screen -->
<div id="fffWaitingScreen" class="screen">
    <div class="status connected">⚡ Fastest Finger First</div>
    <h2 id="fffWaitingMessage" style="color: #FFD700;">
        Results are being calculated.<br>
        Please Stand By.<br>
        The Host will announce the winners shortly.
    </h2>
</div>

<!-- FFF Result Screen -->
<div id="fffResultScreen" class="screen">
    <div class="status connected">⚡ Results</div>
    <h2 id="fffResultMessage" style="color: #FFD700;"></h2>
</div>
```

## Testing Checklist

- [ ] Timer expires → Shows "Results being calculated"
- [ ] Reveal correct (4th click) → Shows "And the winner is..."
- [ ] Confirm winner (winner) → Shows congratulations
- [ ] Confirm winner (correct but slow) → Shows "too slow" message
- [ ] Confirm winner (incorrect) → Shows "incorrect" message
- [ ] No winner scenario → Shows "try again" message
- [ ] FFF window close → Returns to lobby
- [ ] Multiple participants see personalized messages

## Files to Modify

1. `FFFControlPanel.cs` - Add message sending logic
2. `FFFHub.cs` or create `FFFHostService.cs` - Add broadcast methods
3. `index.html` - Add new screens
4. `app.js` - Add message handlers and screen logic
5. `app.css` - Style new screens (if needed)

## Dependencies

- WebSocket connection must be active
- Session ID must be available in FFFControlPanel
- Participant IDs must be tracked correctly

