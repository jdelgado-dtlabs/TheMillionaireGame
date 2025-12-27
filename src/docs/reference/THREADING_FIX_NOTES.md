# Threading Fix Notes

## Stop Sounds / Lights Down Freeze Fix (2025-12-24)

### Issue
When clicking "Lights Down" while audio was playing, the application would freeze.

### Root Cause
In `ControlPanelForm.cs` `btnLightsDown_Click` (lines 1073-1089):
- After `await StopAllSoundsAsync()` completed on background thread
- Code used `BeginInvoke(() => PlayLightsDownSound())` to queue the sound playback on UI thread
- **Problem**: `BeginInvoke` only QUEUES the operation, doesn't execute immediately
- This queueing delay created a race condition / timing gap that allowed freezes

### Solution
Removed the `BeginInvoke` wrapper and called `PlayLightsDownSound()` directly from the `Task.Run` context.

**Why this works:**
- `PlayLightsDownSound()` doesn't perform UI operations
- It just looks up a sound key and calls `_soundService.PlaySoundByKeyWithIdentifier()`
- `PlaySoundByKeyWithIdentifier` already uses `Task.Run` internally for playback
- No UI thread access needed, so no BeginInvoke needed

### Code Change
```csharp
// BEFORE (caused freeze):
if (!token.IsCancellationRequested && !IsDisposed && IsHandleCreated)
{
    BeginInvoke(() =>
    {
        if (!IsDisposed)
        {
            PlayLightsDownSound();
        }
    });
}

// AFTER (fixed):
if (!token.IsCancellationRequested && !IsDisposed)
{
    PlayLightsDownSound();
}
```

### Key Lesson
**Only use BeginInvoke for actual UI thread operations** (updating controls, colors, etc.)

If a method handles its own threading internally (like sound playback via Task.Run), calling it directly is both:
1. **Simpler** - less code, fewer indirection layers
2. **Faster** - no UI thread queueing delay
3. **Safer** - eliminates race conditions from queueing delays

Methods that handle their own threading:
- `PlaySoundByKeyWithIdentifier` - uses Task.Run
- `PlayQuestionBed()` - calls PlaySoundByKeyWithIdentifier
- `PlayLightsDownSound()` - calls PlaySoundByKeyWithIdentifier
- `PlayFinalAnswerSound()` - calls PlaySoundByKeyWithIdentifier
- `PlayCorrectSound()` - calls PlaySoundByKeyWithIdentifier

Methods that NEED BeginInvoke (direct UI updates):
- Setting `txtQuestion.Text`
- Setting `btnA.BackColor`
- Setting `chkShowQuestion.Checked`
- Any Label/TextBox/Button/Control property changes

## Related Fixes
- All previous async void → async Task conversions
- 50+ Invoke → BeginInvoke conversions for non-blocking UI updates
- StopSoundAsync / StopAllSoundsAsync methods added to SoundService
