# NAudio Implementation Reference Document

**Purpose**: Reference document for CSCore migration - preserves exact behavior and logic of current NAudio implementation  
**Created**: December 25, 2025  
**Status**: Reference Only - DO NOT MODIFY ORIGINAL NAUDIO CODE  

---

## Critical: This is a Reference Document Only

⚠️ **DO NOT MODIFY THE ORIGINAL NAUDIO CODE** ⚠️

This document captures the current NAudio implementation to guide the CSCore migration. The original NAudio code will remain untouched until CSCore is fully working and tested.

---

## Current Architecture Overview

### File Location
- **Primary File**: `src/MillionaireGame/Services/SoundService.cs`
- **Lines of Code**: 1003 lines
- **Library**: NAudio (WaveOutEvent + AudioFileReader)
- **Architecture**: Single-channel pool with shared player dictionary

### Core Components

```csharp
// Key fields
private readonly Dictionary<SoundEffect, string> _soundPaths = new();
private readonly Dictionary<string, IWavePlayer> _activePlayers = new();
private readonly SoundPackManager _soundPackManager;
private readonly object _lock = new();
private bool _soundEnabled = true;
private bool _disposed = false;
```

### Player Model
- **Type**: `WaveOutEvent` (NAudio's event-driven output device)
- **Reader**: `AudioFileReader` (handles MP3/WAV decoding)
- **Storage**: Players stored in `_activePlayers` dictionary with string identifiers
- **Looping**: Handled in `PlaybackStopped` event by resetting position and calling `Play()` again

---

## Sound Categorization

### Music Sounds (Looping Bed Music)
These should loop continuously across questions:
- `LightsDown` (Q1-5): Key: "Q01to05LightsDown", "Q1to5LightsDown", "q1_to_q5_lights_down"
- Question Bed Music (Q1-5): Key: "Q01to05Bed", "Q1to5Bed", "q1_to_q5_bed"
- Each Q6-15: Individual bed tracks per question level

### Effect Sounds (One-Shot)
These play once and complete:
- **Reveal sounds**: FinalAnswer, CorrectAnswer, WrongAnswer
- **Lifelines**: 5050, PAF (multiple phases), ATA (multiple phases), Switch, ATH, DoubleDip
- **Game flow**: HostEntrance, ExplainGame, QuitSmall/Large, WalkAway, CloseTheme, ToHotSeat
- **FFF**: All fastest finger first sounds
- **UI feedback**: LifelinePing1-4, SetSafetyNet, RiskMode

---

## Key Methods - Current Behavior

### PlaySound(SoundEffect, bool loop)
```csharp
// Synchronous wrapper - runs PlaySoundFile in background
public void PlaySound(SoundEffect effect, bool loop = false)
{
    if (!_soundEnabled) return;
    
    if (_soundPaths.TryGetValue(effect, out var filePath))
    {
        var identifier = loop ? effect.ToString() : Guid.NewGuid().ToString();
        Task.Run(() => PlaySoundFile(filePath, identifier, loop));
    }
}
```

**Identifier Logic**:
- Looping sounds: Use `effect.ToString()` as identifier (allows stopping by name later)
- One-shot sounds: Use `Guid.NewGuid()` (unique, can't be stopped individually)

### PlaySoundByKey(string key, bool loop)
```csharp
public void PlaySoundByKey(string key, bool loop = false)
{
    if (!_soundEnabled) return;
    
    var filePath = _soundPackManager.GetSoundFile(key);
    if (!string.IsNullOrEmpty(filePath))
    {
        // Looping: use key as identifier
        // One-shot: use null (no tracking)
        var identifier = loop ? key : (string?)null;
        PlaySoundFile(filePath, identifier, loop);
    }
}
```

**Note**: Currently calls PlaySoundFile directly (not Task.Run) due to threading issues

### PlaySoundFile(string filePath, string? identifier, bool loop)
```csharp
public void PlaySoundFile(string filePath, string? identifier = null, bool loop = false)
{
    // Core playback logic:
    // 1. Resolve file path
    // 2. Create AudioFileReader
    // 3. Create WaveOutEvent
    // 4. Initialize player with audio file
    // 5. Set up PlaybackStopped event handler (handles looping)
    // 6. Store player in _activePlayers dictionary
    // 7. Call player.Play()
    
    var id = identifier ?? Guid.NewGuid().ToString();
    
    AudioFileReader audioFile = new AudioFileReader(resolvedPath);
    IWavePlayer player = new WaveOutEvent();
    player.Init(audioFile);
    
    player.PlaybackStopped += (s, e) =>
    {
        lock (_lock)
        {
            if (loop && _activePlayers.ContainsKey(id))
            {
                audioFile.Position = 0;
                player.Play(); // Restart
                return;
            }
            
            // Cleanup
            if (_activePlayers.ContainsKey(id))
            {
                _activePlayers[id].Dispose();
                _activePlayers.Remove(id);
            }
        }
        audioFile?.Dispose();
    };
    
    lock (_lock)
    {
        _activePlayers[id] = player;
    }
    
    player.Play();
}
```

**Threading Issue**: `Dispose()` in event handler can block - this is the root cause of UI freezing

### StopAllSounds()
```csharp
public void StopAllSounds()
{
    List<IWavePlayer> playersToStop;
    lock (_lock)
    {
        playersToStop = new List<IWavePlayer>(_activePlayers.Values);
        _activePlayers.Clear();
    }
    
    foreach (var player in playersToStop)
    {
        try
        {
            player.Dispose(); // BLOCKING - causes freezes
        }
        catch { }
    }
}
```

**Problem**: `Dispose()` blocks waiting for playback thread to exit. When called from UI thread or event handler, this causes freezing.

### StopSound(string identifier)
```csharp
public void StopSound(string identifier)
{
    IWavePlayer? playerToStop = null;
    lock (_lock)
    {
        if (_activePlayers.TryGetValue(identifier, out playerToStop))
        {
            _activePlayers.Remove(identifier);
        }
    }
    
    if (playerToStop != null)
    {
        playerToStop.Stop();   // BLOCKING
        playerToStop.Dispose(); // BLOCKING
    }
}
```

**Problem**: Same blocking issue as StopAllSounds

---

## Question-Level Sound Behavior (CRITICAL)

### Q1-4: Continuous Bed Music
```csharp
// From ControlPanelForm.cs btnLightsDown_Click()
if (questionNumber >= 1 && questionNumber <= 5)
{
    // Q1-4: Bed music should loop continuously
    PlayLightsDownSound(); // Plays looping bed music
}
```

**Expected Behavior**:
- Bed music starts on Q1 Lights Down
- Loops continuously through Q1, Q2, Q3, Q4
- Does NOT stop between questions
- Only stops when:
  - Player hits Q5 (game stops all sounds for final answer)
  - Player quits/walks away
  - Wrong answer (game over)

### Q5: Stop All Before Correct Answer
```csharp
// From ProcessNormalReveal() - Q5 behavior
if (questionNumber == 5)
{
    // Stop all sounds (including bed music)
    await _soundService.StopAllSoundsAsync();
    
    // Play correct answer sound
    PlayCorrectAnswerSound();
}
```

**Expected Behavior**:
- Bed music should stop BEFORE playing correct answer sound
- This is the "safety net" moment - dramatic pause
- Clean audio transition (no overlap)

### Q6+: Stop Bed Music on Question Load
```csharp
// From btnQuestion_Click()
if (questionNumber >= 6)
{
    // Stop bed music when loading new question
    await _soundService.StopAllSoundsAsync();
}

// Later in btnLightsDown_Click()
if (questionNumber >= 6)
{
    await _soundService.StopAllSoundsAsync();
    PlayLightsDownSound(); // Plays Q6+ specific bed track
}
```

**Expected Behavior**:
- Each question Q6-15 has its own bed music track
- Old bed music stops when "Question" button clicked
- New bed music starts when "Lights Down" button clicked
- Clean transition between tracks

---

## Control Panel Integration

### Sound Calls from ControlPanelForm.cs

**Reveal Button** (`btnReveal_Click`):
```csharp
// Lines 1100-1200 (approximate)
// Reveal answer logic - plays FinalAnswer, CorrectAnswer, or WrongAnswer
```

**Question Button** (`btnQuestion_Click`):
```csharp
// Line 772: Stop sounds for Q6+
if (currentQuestionNumber >= 6 && !string.IsNullOrEmpty(_currentLightsDownIdentifier))
{
    await _soundService.StopAllSoundsAsync();
    _currentLightsDownIdentifier = null;
}
```

**Lights Down Button** (`btnLightsDown_Click`):
```csharp
// Lines 1056-1140
// Q1-5: Starts looping bed music
// Q6+: Stops all sounds, plays question-specific bed music
```

**Lock In Safety Net** (`StartSafetyNetAnimation`):
```csharp
// Line 1657
if (playSound)
{
    _soundService.PlaySound(SoundEffect.SetSafetyNet, "safety_net_lock_in", loop: false);
}
```

**Walk Away** (`btnWalk_Click`):
```csharp
// Line 1371-1375
await _soundService.StopAllSoundsAsync();
_soundService.PlaySound(quitSound, quitSoundId, loop: false);
```

---

## Lifeline Manager Integration

### Lifeline Sound Calls

**50:50**:
```csharp
_soundService.PlaySound(SoundEffect.Lifeline5050);
```

**Phone-a-Friend**:
```csharp
await _soundService.StopAllSoundsAsync();
_soundService.PlaySound(SoundEffect.LifelinePAFCountdown, "paf_countdown");
```

**Ask the Audience**:
```csharp
await _soundService.StopAllSoundsAsync();
_soundService.PlaySound(SoundEffect.LifelineATAVote, "ata_vote");
```

**Pattern**: Most lifelines call `StopAllSoundsAsync()` before playing lifeline sound

---

## Threading and Disposal Issues (WHY MIGRATION IS NEEDED)

### Issue 1: UI Thread Blocking
**Location**: `StopAllSounds()`, `StopSound()`  
**Problem**: `Dispose()` called on UI thread blocks waiting for playback thread  
**Symptom**: Clicking Reveal, Question, Lock-in, or Lights Down freezes UI for 1-3 seconds  

### Issue 2: Event Handler Deadlocks
**Location**: `PlaybackStopped` event handler in `PlaySoundFile()`  
**Problem**: Disposing player from its own callback thread causes thread to wait for itself  
**Symptom**: App freeze or hang when sounds complete naturally  

### Issue 3: Mixed Responsibilities
**Location**: Single `_activePlayers` dictionary  
**Problem**: Looping bed music and one-shot effects share same pool  
**Symptom**: StopAllSounds() unintentionally stops bed music when only effects should stop  

### Issue 4: Shutdown Hangs
**Location**: `Dispose()` method  
**Problem**: Calls `StopAllSounds()` which blocks  
**Symptom**: App takes 3-5 seconds to close, sometimes leaves zombie processes  

---

## Failed Workaround Attempts (DO NOT RETRY)

❌ **Attempt 1**: Wrap disposal in `Task.Run()`  
**Result**: Race conditions, players not disposed  

❌ **Attempt 2**: Use `Monitor.TryEnter()` for non-blocking locks  
**Result**: Helps but doesn't solve blocking disposal  

❌ **Attempt 3**: Remove `Stop()`, only call `Dispose()`  
**Result**: Still blocks, no improvement  

❌ **Attempt 4**: Dispose from background thread  
**Result**: Race conditions with dictionary access  

❌ **Attempt 5**: Fire-and-forget disposal  
**Result**: Sounds continue playing, memory leaks  

❌ **Attempt 6**: Clear dictionary without disposing  
**Result**: Zombie players, memory leaks  

❌ **Attempt 7**: Dispose on background thread from `PlaybackStopped`  
**Result**: Still blocks, no improvement  

**Conclusion**: NAudio's synchronous disposal model cannot be fixed with workarounds. Architecture must change.

---

## Sound Pack Manager

### Current Implementation
```csharp
public class SoundPackManager
{
    // Loads sound packs from XML files in lib/soundpacks/
    // Maps sound keys to file paths
    // Supports multiple sound packs (Default, Shuffle, etc.)
}
```

**Sound Pack Structure**:
```xml
<SoundPack name="Default">
  <Sound key="Q01to05LightsDown">lib/sounds/q1to5_lights_down.mp3</Sound>
  <Sound key="Q01to05Bed">lib/sounds/q1to5_bed.mp3</Sound>
  <!-- ... more sounds ... -->
</SoundPack>
```

**Integration**: SoundService uses `_soundPackManager.GetSoundFile(key)` to resolve paths

---

## CSCore Migration Requirements

### Must Preserve

1. **All sound timing and sequencing**:
   - Q1-4 bed music loops continuously
   - Q5 stops all sounds before correct answer
   - Q6+ stops bed music on question load
   
2. **Identifier system**:
   - Looping sounds: identifiable by name for stopping
   - One-shot sounds: fire-and-forget (no tracking needed)
   
3. **API compatibility**:
   - `PlaySound(SoundEffect, bool loop)`
   - `PlaySoundByKey(string key, bool loop)`
   - `StopSound(string identifier)`
   - `StopAllSounds()`
   - `IsSoundPlaying(string identifier)`
   - `WaitForSoundAsync(string identifier, CancellationToken)`
   
4. **Sound pack integration**:
   - Continue using SoundPackManager
   - Support XML-based sound packs
   
5. **Application shutdown**:
   - Clean disposal (no blocking)
   - No zombie processes

### Must Fix

1. **UI freezing on button clicks**:
   - Reveal button (Q5+)
   - Question button (Q6+)
   - Lock-in button (safety net animation)
   - Lights Down button (Q6+)
   
2. **Non-blocking disposal**:
   - Use async disposal patterns
   - Background cleanup of completed sounds
   
3. **Channel separation**:
   - Music channel for looping bed music
   - Effects channel for one-shot sounds
   - StopAllSounds() should only stop effects, not music (unless explicitly called)

---

## Testing Checklist (For CSCore Implementation)

### Functional Tests
- [ ] Q1-4: Bed music loops continuously without stopping
- [ ] Q5: All sounds stop before correct answer plays
- [ ] Q6+: Bed music stops when loading new question
- [ ] Q6+: New bed music starts on Lights Down
- [ ] Lifelines: Sounds play correctly (50:50, PAF, ATA, etc.)
- [ ] Safety net: Lock-in sound plays during animation
- [ ] Walk away: Quit sound plays after stopping all audio
- [ ] Game over: Wrong answer sound plays correctly

### UI Responsiveness Tests
- [ ] Reveal button: No freeze at Q1 (with bed music playing)
- [ ] Reveal button: No freeze at Q5 (stopping all sounds)
- [ ] Reveal button: No freeze at Q10 (with bed music playing)
- [ ] Question button: No freeze at Q6 (stopping bed music)
- [ ] Lights Down button: No freeze at Q6 (stopping + playing new bed)
- [ ] Lock-in button: No freeze during safety net animation
- [ ] All buttons: Responsive within 50ms (perceived as instant)

### Stability Tests
- [ ] Play full game Q1-15: No crashes
- [ ] Play 3 full games in a row: No memory leaks
- [ ] Rapid button clicking: No crashes or race conditions
- [ ] Close app while sounds playing: Clean exit, no zombie processes
- [ ] Close app during looping bed music: Clean exit
- [ ] Lifeline usage: All phases work correctly (PAF countdown, ATA vote, etc.)

### Audio Quality Tests
- [ ] No audio glitches or pops when stopping sounds
- [ ] Smooth transitions between bed music tracks
- [ ] No overlap when stopping + starting new sounds
- [ ] Looping is seamless (no gap at loop point)
- [ ] Volume control works correctly
- [ ] Multiple sounds can play simultaneously (if needed)

---

## Implementation Notes for CSCore

### Recommended Channel Architecture

**Music Channel**:
- Single persistent player for looping bed music
- Methods: `PlayMusic(file, loop)`, `StopMusic()`, `SetVolume()`
- Never stopped by general `StopAllSounds()` call
- Only stopped when explicitly changing questions or ending game

**Effects Channel**:
- Pool of players for one-shot sounds
- Methods: `PlayEffect(file)`, `StopAllEffects()`, `ClearCompleted()`
- Automatically cleaned up after playback completes
- Can be stopped without affecting music

### CSCore API Mapping

| NAudio | CSCore |
|--------|--------|
| `WaveOutEvent` | `WasapiOut` or `DirectSoundOut` |
| `AudioFileReader` | `AudioFileReader` (CSCore has its own) |
| `PlaybackStopped` event | `Stopped` event + async disposal |
| `player.Stop()` | `soundOut.Stop()` |
| `player.Dispose()` | `await soundOut.DisposeAsync()` (if available) |

### Key Differences to Handle

1. **ISampleSource vs IWaveProvider**: CSCore uses `ISampleSource` for audio pipeline
2. **Mixing**: CSCore has `MultiplexingSampleSource` for combining channels
3. **Looping**: May need custom `LoopStream` wrapper or use CSCore's loop support
4. **Async disposal**: CSCore may have better async patterns (verify in docs)

---

## File Paths and Structure

### Current Sound Files Location
```
lib/
  sounds/
    q1to5_lights_down.mp3
    q1to5_bed.mp3
    q6_bed.mp3
    ... (60+ sound files)
  soundpacks/
    Default.xml
    Shuffle.xml
```

### Sound Pack Keys (Examples)
```
Q01to05LightsDown
Q01to05Bed
Q01Final
Q01to04Correct
Q01to05Wrong
Q06LightsDown
Q06Bed
Q06Final
Q06to10Correct
... (100+ keys)
```

---

## References

- **Current NAudio Code**: `src/MillionaireGame/Services/SoundService.cs`
- **Control Panel Integration**: `src/MillionaireGame/Forms/ControlPanelForm.cs`
- **Lifeline Integration**: `src/MillionaireGame/Services/LifelineManager.cs`
- **Sound Pack Manager**: `src/MillionaireGame/Utilities/SoundPackManager.cs`
- **Refactoring Plan**: `src/docs/active/SOUND_SYSTEM_REFACTORING_PLAN.md`

---

## Version Control

**Branch**: `master-csharp` (current)  
**Feature Branch**: `feature/cscore-sound-system` (to be created)  

**Backup Strategy**:
- Git history provides full backup
- Original NAudio code remains in `master-csharp`
- CSCore implementation in feature branch
- Merge only after full testing

---

## Next Steps

1. ✅ Document current implementation (this file)
2. ⏭️ Create feature branch `feature/cscore-sound-system`
3. ⏭️ Install CSCore NuGet package
4. ⏭️ Implement MusicChannel.cs (CSCore-based)
5. ⏭️ Implement EffectsChannel.cs (CSCore-based)
6. ⏭️ Update SoundService.cs to use new channels
7. ⏭️ Test thoroughly per checklist above
8. ⏭️ Document results in refactoring plan
9. ⏭️ Merge to master-csharp after approval

---

**End of Reference Document**
