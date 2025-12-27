# Threading Issues Discovery and Resolution

**Date**: December 24, 2025  
**Context**: Issues discovered after adding FFF Online functionality exposed pre-existing threading bugs in the codebase at commit `af19671`.

## Root Cause Analysis

### The Core Problem
The application had latent threading issues that were not triggered until FFF Online added new execution paths. The "working" code at `af19671` was actually broken - we just weren't executing the sequences that exposed the bugs.

### Key Finding
**Only 6 lines changed in ControlPanelForm.cs** between working commit `af19671` and the commit that exposed the issues (`8ded113`). SoundService.cs was NOT modified. This proves the bugs already existed in the "stable" code.

## Threading Anti-Patterns Discovered

### 1. **async void Event Handlers with await**
**Problem**: Windows Forms `async void` event handlers lose UI thread context when using `await`, causing deadlocks and freezes.

**Locations Found**:
- `btnLightsDown_Click` - Froze when clicked
- `btnReveal_Click` - Froze during answer reveal
- `SelectAnswer()` - Froze during answer selection
- Multiple lifeline button handlers
- FFF winner announcement

**Symptom**: UI completely freezes, no response, appears hung

**Root Cause**: 
```csharp
private async void btnReveal_Click(object? sender, EventArgs e)
{
    await SomeAsyncOperation(); // UI thread context lost here
    // Any UI operations after this point may deadlock
}
```

**Fix Pattern**:
```csharp
private void btnReveal_Click(object? sender, EventArgs e)
{
    // Disable button immediately on UI thread
    btnReveal.Enabled = false;
    
    // Run async work in background
    _ = Task.Run(async () =>
    {
        await DoAsyncWork();
        
        // Return to UI thread for UI updates
        if (!IsDisposed && IsHandleCreated)
        {
            BeginInvoke(() =>
            {
                if (!IsDisposed)
                {
                    UpdateUI();
                }
            });
        }
    });
}
```

### 2. **Synchronous Invoke() from Background Thread**
**Problem**: Calling `Invoke()` from a background thread to a busy UI thread causes deadlock.

**Locations**: All screen forms (TVScreenForm, HostScreenForm, GuestScreenForm, TVScreenFormScalable)
- Over 50+ methods using synchronous `Invoke()`

**Symptom**: Application hangs when updating screens during async operations

**Fix**: Changed ALL `Invoke()` to `BeginInvoke()` (fire-and-forget async)
```csharp
// BEFORE - DEADLOCK RISK
if (InvokeRequired)
{
    Invoke(new Action(() => ShowAnswer(answer)));
    return;
}

// AFTER - NON-BLOCKING
if (InvokeRequired)
{
    BeginInvoke(new Action(() => ShowAnswer(answer)));
    return;
}
```

### 3. **Lock Contention Between UI and Background Threads**
**Problem**: UI thread calling `StopSound()` acquires `_lock` while audio playback thread's `PlaybackStopped` event handler also needs `_lock`, causing classic deadlock.

**Location**: SoundService.cs
- `StopSound()` - called from UI thread
- `StopAllSounds()` - called from UI thread  
- `PlaybackStopped` event handler - runs on NAudio background thread

**Symptom**: 
- Freeze when clicking "Stop All Sounds" button
- Freeze when clicking "Lights Down" while audio playing
- Freeze during "Reveal" when stopping previous sounds

**Fix**: Created async versions that run on background threads
```csharp
// NEW - Safe async versions
public Task StopSoundAsync(string identifier)
{
    return Task.Run(() => StopSound(identifier));
}

public Task StopAllSoundsAsync()
{
    return Task.Run(() => StopAllSounds());
}

// USAGE - Await when needed
private void btnStopAudio_Click(object? sender, EventArgs e)
{
    _ = _soundService.StopAllSoundsAsync(); // Fire and forget safe
}

// When you need sequential operations
await _soundService.StopSoundAsync(keyToStop);  // Wait for completion
PlayNewSound(); // Then play new sound
```

### 4. **Race Conditions from Fire-and-Forget Task.Run**
**Problem**: Starting multiple `Task.Run` operations without coordination created race conditions.

**Location**: Lights Down button - multiple overlapping async operations

**Symptom**: Freeze when clicking Lights Down after closing FFF window

**Before** (Race Condition):
```csharp
// Fire and forget - runs in parallel
_ = Task.Run(async () => { await StopAllSounds(); });

// Also fires immediately - RACES with above!
_ = Task.Run(async () => 
{ 
    await LoadNewQuestion();  // Calls ClearAnswers which uses BeginInvoke
});
```

**After** (Sequential):
```csharp
_ = Task.Run(async () =>
{
    // Step 1: Stop sounds and WAIT
    await _soundService.StopAllSoundsAsync();
    
    // Step 2: Play lights down sound
    BeginInvoke(() => PlayLightsDownSound());
    
    // Step 3: Load question (Q1-5 only)
    if (isQ1to5)
    {
        await LoadNewQuestion();
        BeginInvoke(() => UpdateUI());
        await Task.Delay(4000);
        BeginInvoke(() => PlayQuestionBed());
    }
});
```

### 5. **Sequential Screen Updates Appearing Delayed**
**Problem**: `ClearQuestionAndAnswerText()` called at wrong time + multiple `BeginInvoke()` calls queued separately caused visible sequential "erasing" effect.

**Symptom**: After reveal, clicking next question showed old answers briefly then they disappeared one by one over ~500ms

**Fix**: Clear answers at start of `LoadNewQuestion()` before any display updates
```csharp
private async Task LoadNewQuestion()
{
    _pendingSafetyNetLevel = 0;
    
    // Clear old answers FIRST, before showing new question
    _screenService.ClearQuestionAndAnswerText();
    
    // Now load and display new question
    var question = await _questionRepository.GetRandomQuestionAsync(...);
    // ...
}
```

## Summary of Fixes Applied

| Issue | Files Changed | Fix Type |
|-------|--------------|----------|
| async void handlers | ControlPanelForm.cs | Convert to void + Task.Run pattern |
| Synchronous Invoke() | All 4 screen forms | Changed 50+ Invoke() → BeginInvoke() |
| Lock contention | SoundService.cs | Added StopSoundAsync/StopAllSoundsAsync |
| Race conditions | ControlPanelForm.cs (Lights Down) | Sequential await chain in single Task.Run |
| Sequential clearing | ControlPanelForm.cs (LoadNewQuestion) | Clear at start, not during display |

## Best Practices Going Forward

### ✅ DO
1. **Never use `async void` in event handlers** - Use `void` + `Task.Run` instead
2. **Always use `BeginInvoke`** for cross-thread UI updates - Never `Invoke`
3. **Create async versions of lock-heavy methods** - Let them run on background threads
4. **Sequential async operations** - Use `await` to ensure proper ordering
5. **Check disposal** - Always check `IsDisposed` and `IsHandleCreated` before `BeginInvoke`

### ❌ DON'T
1. **Never `await` in `async void` handlers** - Loses UI thread context
2. **Never use `Invoke()` from background threads** - Blocks caller until UI thread free
3. **Never call lock-heavy methods from UI thread** - Use async versions
4. **Never fire multiple `Task.Run` operations** without coordination
5. **Never assume "working code" is bug-free** - May just not be triggering the paths

## Pattern Reference

### Safe Button Click Pattern
```csharp
private void btnSomething_Click(object? sender, EventArgs e)
{
    // 1. Update UI immediately (disable button, etc.)
    btnSomething.Enabled = false;
    
    // 2. Run work in background
    _ = Task.Run(async () =>
    {
        try
        {
            // 3. Do async work
            await DoAsyncWork();
            
            // 4. Return to UI thread for updates
            if (!IsDisposed && IsHandleCreated)
            {
                BeginInvoke(() =>
                {
                    if (!IsDisposed)
                    {
                        btnSomething.Enabled = true;
                    }
                });
            }
        }
        catch (Exception ex)
        {
            GameConsole.Log($"Error: {ex.Message}");
        }
    });
}
```

### Safe Cross-Thread UI Update Pattern
```csharp
public void UpdateFromAnyThread(string value)
{
    if (InvokeRequired)
    {
        BeginInvoke(() => UpdateFromAnyThread(value));  // Async, non-blocking
        return;
    }
    
    // Now safe - we're on UI thread
    label.Text = value;
}
```

### Safe Sound Stopping Pattern
```csharp
// When you need to play a new sound after stopping old one
_ = Task.Run(async () =>
{
    // Wait for stop to complete
    await _soundService.StopSoundAsync(oldSound);
    
    // Now safe to play new sound
    BeginInvoke(() => PlayNewSound());
});
```

## Testing Sequences That Exposed Bugs

These specific sequences revealed the latent threading issues:

1. **FFF Winner Announcement**: Winner selected → FFF closes → Sounds continue → **FROZE**
2. **Lights Down with Audio**: Host Intro playing → Pick Player → Close FFF → Lights Down → **FROZE**
3. **Reveal Button**: Select Answer → Click Reveal → **FROZE** during sound transition
4. **Stop All Sounds**: Playing audio → Click Stop All Sounds button → **FROZE**
5. **Sequential Questions**: Reveal answer → Click Question → Old answers erase one-by-one → **VISUAL BUG**

## Conclusion

The threading issues were **always present** in the codebase. The FFF Online additions simply exposed them by:
- Adding new execution paths that triggered race conditions
- Increasing the frequency of cross-thread operations
- Creating new sequences that exposed the lock contention

**The fixes we implemented are permanent improvements** that make the entire application more robust, not just patches for FFF-specific issues.

## Lessons Learned

1. **"Working code" may have latent bugs** - Just because it works doesn't mean it's correct
2. **Windows Forms threading is tricky** - Easy to create deadlocks even with "simple" async code
3. **Test edge cases early** - Clicking buttons rapidly, interrupting operations, etc.
4. **BeginInvoke is safer than Invoke** - Fire-and-forget is better than blocking
5. **Lock-free is better than locks** - When possible, use concurrent collections instead

---

**Status**: All threading issues resolved. Application now handles all previously-failing sequences correctly.
