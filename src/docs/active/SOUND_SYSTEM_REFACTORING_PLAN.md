# Sound System Refactoring Plan

**Status:** READY TO IMPLEMENT - CSCore Migration Approved  
**Priority:** High (Blocking Issue)  
**Created:** December 25, 2025  
**Last Updated:** December 25, 2025 2:00 AM  

---

## EXECUTIVE SUMMARY FOR NEW SESSION

### Critical Context
The sound system currently uses NAudio and experiences **UI freezing** when stopping/disposing audio players. This is caused by NAudio's blocking disposal pattern where `Dispose()` waits for playback threads to exit, causing deadlocks when called from UI thread or event handlers.

### Decision Made
**Migrate to CSCore library** because:
1. ✓ **Solves freezing issues** - Better async disposal patterns
2. ✓ **Broadcasting ready** - User confirmed future requirement for OBS/streaming integration
3. ✓ **Professional architecture** - Source → Mixer → Output pipeline matches industry standards
4. ✓ **Multi-channel support** - Separates music (looping bed) from effects (one-shot sounds)

### What NOT To Do (Failed Attempts)
DO NOT try these NAudio fixes - all have been tested and failed:
- ❌ Removing Task.Run wrappers
- ❌ Using Monitor.TryEnter for non-blocking locks  
- ❌ Removing Stop() and only calling Dispose()
- ❌ Background thread disposal with Task.Run
- ❌ Fire-and-forget disposal
- ❌ Clearing dictionary without disposing
- ❌ Disposing from PlaybackStopped event handler

### Current Code State
**Branch:** `master-csharp`  
**Sound System:** NAudio-based, located at `src/MillionaireGame/Services/SoundService.cs`  
**Status:** Functional but has freezing issues on:
- Reveal button (Q5+)
- Question button (Q6+)
- Lock-in button (safety net animation)
- Lights Down button (Q6+)
- App shutdown with active sounds

### Next Actions (Start Here Tomorrow)
1. **Create feature branch**: `feature/cscore-sound-system`
2. **Install CSCore**: `dotnet add package CSCore` to MillionaireGame project
3. **Follow Phase 1-6** in Implementation Steps section below
4. **Test thoroughly** before merging
5. **Document any CSCore issues** - if critical, fall back to NAudio multi-channel approach

### Time Estimate
7-9 hours total implementation time

---

## Problem Statement

The current sound system experiences deadlocks and UI freezes when stopping/disposing NAudio players. This is caused by:

1. **Single-channel architecture**: All sounds (looping bed music + one-shot effects) share the same player pool
2. **Thread blocking**: NAudio's `Stop()` and `Dispose()` methods block waiting for playback thread termination
3. **Event handler deadlocks**: Disposing players from within the `PlaybackStopped` event handler causes the thread to wait for itself
4. **Mixed responsibilities**: The sound service tries to be both a music player and an effects soundboard

### Current Issues
- ✗ Clicking Reveal, Question, Lock-in, or Lights Down can freeze the UI
- ✗ Closing the app while sounds are playing can freeze or leave zombie processes
- ✗ Bed music gets unintentionally stopped when trying to stop effects
- ✗ Q1-5 vs Q6+ sound behavior is fragile and error-prone

## Solution A: Multi-Channel NAudio Architecture (Primary Plan)

### Concept
Separate the sound system into two independent channels, mimicking a real soundboard:

**Music Channel** (Background/Bed Music)
- Single dedicated player for continuous looping tracks
- Never stopped/restarted during gameplay (except for explicit game state changes)
- Long-running, persistent across questions Q1-5
- Simple volume control for ducking/fading

**Effects Channel** (One-shot Sounds)
- Pool of players for short sound effects
- Can be stopped/cleared without affecting music
- Disposable after playback completes
- Handle reveal sounds, correct/wrong answers, lifelines, etc.

### Architecture Design

```
SoundService
├── MusicChannel (NAudio-based)
│   ├── _musicPlayer (IWavePlayer)
│   ├── _currentMusicFile (AudioFileReader)
│   ├── PlayMusic(file, loop)
│   ├── StopMusic()
│   ├── SetVolume(float)
│   └── FadeOut(duration)
│
└── EffectsChannel (NAudio-based)
    ├── _effectPlayers (Dictionary<string, IWavePlayer>)
    ├── _effectPool (Queue<IWavePlayer>) [reusable players]
    ├── PlayEffect(file)
    ├── StopEffect(identifier)
    ├── StopAllEffects()
    └── ClearCompleted() [cleanup finished players]
```

**Note:** This approach works for immediate needs but will require additional refactoring for broadcasting. Audio tapping/routing to multiple outputs must be manually implemented.

## Solution B: CSCore Library (Primary Plan - Broadcasting Ready)

### Recommended Approach
Given the broadcasting requirement, **start with CSCore** for its superior audio routing capabilities.

**CSCore Architecture for Broadcasting:**

```
SoundService
├── MusicChannel (CSCore-based)
│   ├── _musicSource (ISampleSource)
│   ├── _musicOutput (ISoundOut)
│   ├── PlayMusic(file, loop)
│   ├── StopMusic()
│   ├── SetVolume(float)
│   └── FadeOut(duration)
│
├── EffectsChannel (CSCore-based)
│   ├── _effectSources (List<ISampleSource>)
│   ├── PlayEffect(file)
│   ├── StopAllEffects()
│   └── ClearCompleted()
│
├── Mixer (MultiplexingSampleSource)
│   ├── AddChannel(musicChannel)
│   ├── AddChannel(effectsChannel)
│   └── MixedOutput → Can be tapped for multiple destinations
│
└── OutputRouter
    ├── SystemSpeakers (WasapiOut)
    ├── VirtualCable (WasapiOut to specific device)
    ├── BroadcastStream (NetworkStream or file)
    └── AddOutput(ISoundOut) [extensible]
```

### CSCore Implementation Steps

#### Phase 1: Install and Setup CSCore (30 min)
1. Install CSCore NuGet package:
   ```bash
   dotnet add package CSCore
   ```
2. Test basic playback to verify compatibility
3. Create basic ISampleSource for a single sound

#### Phase 2: Create Music Channel (1-2 hours)
1. Create `MusicChannel.cs` using CSCore:
   ```csharp
   public class MusicChannel : IDisposable
   {
       private ISampleSource _musicSource;
       private ISoundOut _soundOut;
       private LoopStream _loopWrapper; // Custom loop implementation
       
       public void PlayMusic(string filePath, bool loop) { }
       public void StopMusic() { }
       public void SetVolume(float volume) { }
   }
   ```

2. Implement loop handling with CSCore's stream model
3. Add volume control via VolumeSampleSource

#### Phase 3: Create Effects Channel (1-2 hours)
1. Create `EffectsChannel.cs`:
   ```csharp
   public class EffectsChannel : IDisposable
   {
       private List<ISampleSource> _activeSources;
       
       public void PlayEffect(string filePath) { }
       public void StopAllEffects() { }
   }
   ```

2. Implement fire-and-forget playback
3. Auto-cleanup completed effects

#### Phase 4: Create Mixer (1 hour)
1. Use `MultiplexingSampleSource` to combine channels
2. Route mixed output to system audio
3. Prepare tap points for future broadcasting

#### Phase 5: Update SoundService API (2 hours)
1. Replace NAudio calls with CSCore equivalents
2. Maintain backward compatibility with existing calls
3. Update all `PlaySound()`, `StopSound()` methods

#### Phase 6: Testing (2 hours)
1. Full game flow Q1-15
2. Verify all sounds work correctly
3. Test shutdown behavior
4. Memory leak testing

**Total Estimated Time:** 7-9 hours

### Future Broadcasting Implementation (Phase 7 - Later)

Once core system works, add broadcasting:

```csharp
public class BroadcastRouter
{
    private ISampleSource _mixedSource;
    
    public void AddOutput(string deviceName) 
    {
        // Create new WasapiOut to specific device
        var output = new WasapiOut(GetDevice(deviceName));
        output.Initialize(_mixedSource.ToWaveSource());
        output.Play();
    }
    
    public void SendToVirtualCable() { }
    public void SendToOBS() { }
}
```

**Advantages:**
- Audio pipeline is already split/routable
- Adding outputs doesn't require system redesign
- Can control which channels go to which outputs
- Professional-grade architecture from the start

## Decision Criteria (UPDATED)

**Proceed with Solution B (CSCore) if:**
- ✓ Broadcasting/streaming is a confirmed future requirement **(YES)**
- ✓ Need professional audio routing capabilities **(YES)**
- ✓ Want modern async/await patterns
- ✓ Willing to invest 7-9 hours for long-term benefits **(RECOMMENDED)**

**Fall back to Solution A (NAudio) if:**
- CSCore has critical bugs or compatibility issues
- Package is unmaintained or has breaking changes
- Development time exceeds 12 hours without working solution
- CSCore documentation is insufficient

## Recommendation

**Start with CSCore (Solution B)** because:
1. Broadcasting requirement makes CSCore the better long-term choice
2. Cleaner architecture will solve current freezing issues
3. Won't need to refactor again when adding broadcast features
4. Time investment (7-9 hours) is justified by future-proofing
5. Better audio routing matches "production-level application" goal

### Implementation Steps

#### Phase 1: Create Channel Classes (2-3 hours)
1. Create `MusicChannel.cs`
   - Single player management
   - Loop handling
   - Volume control
   - Clean stop/start methods

2. Create `EffectsChannel.cs`
   - Player pool management
   - Fire-and-forget playback
   - Non-blocking stop operations
   - Automatic cleanup of completed sounds

3. Update `SoundService.cs`
   - Instantiate both channels
   - Route calls to appropriate channel based on sound type
   - Maintain backward compatibility with existing API

#### Phase 2: Update Sound Effect Categorization (1 hour)
1. Classify all `SoundEffect` enum values as MUSIC or EFFECT
2. Add `SoundType` property or separate enums:
   ```csharp
   public enum SoundType { Music, Effect }
   ```

3. Tag bed music sounds:
   - `LightsDown1to5`
   - `LightsDown6to10`
   - `LightsDown11to15`
   - Any continuous background music

#### Phase 3: Refactor Playback Methods (2-3 hours)
1. Update `PlaySound()` to route to correct channel
2. Update `StopAllSounds()` to only stop effects (preserve music)
3. Add `StopMusic()` for explicit music control
4. Update question-level logic:
   - Q1-5: Start music once, let it loop
   - Q6+: Stop music, play new music
   - Effects: Always independent of music

#### Phase 4: Fix Disposal Pattern (1 hour)
1. Music channel: Dispose only on app shutdown
2. Effects channel: Dispose completed players in background thread
3. Remove all synchronous disposal from event handlers
4. Implement proper cleanup on `FormClosing`

#### Phase 5: Testing & Validation (2 hours)
1. Test full game flow Q1-15
2. Verify music continuity Q1-5
3. Verify music changes Q6+
4. Test all UI buttons (Reveal, Question, Lock-in, Lights Down)
5. Test app shutdown with active sounds
6. Memory leak testing (ensure disposed players are GC'd)

### Technical Benefits
- ✓ No more mixing music and effects in same pool
- ✓ Music can loop indefinitely without interference
- ✓ Effects can be stopped/cleared without blocking
- ✓ Clearer separation of concerns
- ✓ Easier to debug and maintain
- ✓ Better performance (reusable effect player pool)

### Risk Assessment
**Low Risk:**
- NAudio remains the core library (known quantity)
- Incremental refactoring (backward compatible during transition)
- Clear rollback path if issues arise

**Potential Issues:**
- Need careful testing of player pool reuse
- Must verify no memory leaks from unreleased players
- Possible synchronization issues between channels (unlikely)

## Future Requirement: Broadcasting & Streaming Integration

**Critical Feature:** Audio output must be routable to multiple destinations:
- System speakers (in-studio monitoring)
- Virtual audio devices (OBS, streaming platforms)
- Broadcast mixing boards
- Recording software

This requires:
- Multi-output audio routing
- Ability to split/tap audio streams
- Clean mixing of music + effects channels
- Low latency for real-time streaming
- Professional-grade reliability

### Broadcasting Capability Comparison

**NAudio for Broadcasting:**
- ✓ Can output to specific devices (WaveOut/DirectSound/WASAPI)
- ✓ WASAPI loopback for recording system audio
- ✓ Mature, proven in production environments
- ✗ Manual multi-output routing (requires custom ISampleProvider)
- ✗ More complex to split audio to multiple destinations
- ✗ Mixing requires additional plumbing

**CSCore for Broadcasting:**
- ✓ **Superior audio routing architecture**
- ✓ **Built-in mixing via ISampleSource/ISampleProvider**
- ✓ **Cleaner separation of source → processor → output**
- ✓ **Easier to tap streams for multiple outputs**
- ✓ Better pipeline model (source → effects → mixer → outputs)
- ✓ Modern async patterns for real-time streaming
- ✓ More flexible for adding effects/processing later
- ~ Less mature than NAudio (but actively developed)

### Broadcasting Impact on Decision

Given the broadcasting requirement, **CSCore is now the recommended primary approach** because:

1. **Multi-output routing is built-in**: Can easily send audio to multiple sinks
2. **Better mixing model**: Separate channels naturally feed into a mixer
3. **Future-proof architecture**: Adding broadcast features won't require major refactoring
4. **Professional audio workflows**: ISampleSource pattern matches industry standards
5. **Easier integration**: OBS, virtual cables, streaming platforms integrate more cleanly

## Solution A: Multi-Channel NAudio Architecture (Fallback Plan)

**Use NAudio if:**
- CSCore proves unstable or has compatibility issues
- NuGet package has breaking changes
- Documentation/community support is insufficient
- Migration time exceeds estimates

### NAudio Approach

**Migration Strategy:**
1. Install CSCore NuGet package
2. Create similar two-channel architecture using CSCore
3. Replace NAudio-specific code:
   - `WaveOutEvent` → `WasapiOut` or `DirectSoundOut`
   - `AudioFileReader` → CSCore's audio readers
   - Event handlers → CSCore's async patterns

**Estimated Time:** 4-6 hours (if needed)

## Decision Criteria

**Proceed with Solution B if:**
- Solution A still experiences freezes after 3 implementation attempts
- Memory leaks cannot be resolved with NAudio
- Blocking issues persist despite channel separation
- Development time exceeds 10 hours without resolution

**Stick with Solution A if:**
- Freezing issues are resolved
- Music/effects separation works cleanly
- No memory leaks observed
- Code complexity remains manageable

## Current Session State

### Attempts Made (Prior to This Plan)
1. ✗ Removed Task.Run wrappers - still blocked
2. ✗ Used Monitor.TryEnter for non-blocking locks - helped but not enough
3. ✗ Removed Stop() call, just Dispose() - still blocked
4. ✗ Background thread disposal with Task.Run - caused race conditions
5. ✗ Fire-and-forget disposal - left zombie processes
6. ✗ Clear dictionary without disposal - sounds continued playing
7. ✗ Dispose on background thread from PlaybackStopped - still blocking

### Root Cause Identified
NAudio's thread model requires the playback thread to fully exit before `Dispose()` returns. When disposing from event handlers or UI thread, this creates blocking. **The fundamental issue is architectural - mixing looping music and one-shot effects in a single player pool with synchronous disposal.**

### Critical User Requirements
1. **Sound Behavior by Question Level:**
   - Q1-4: Bed music loops continuously, no final answer sound
   - Q5: Bed music loops, play final answer sound after stopping all sounds
   - Q6+: Stop bed music when loading new question, play final answer sound

2. **Future Broadcasting:**
   - Must support audio output to multiple destinations simultaneously
   - System speakers + OBS/streaming software + recording software
   - Professional-grade audio routing for studio production use

### Why CSCore Was Chosen Over NAudio Multi-Channel
While NAudio multi-channel would fix the freezing, CSCore is superior because:
- Broadcasting is easier (built-in ISampleSource tapping)
- Won't need to refactor again when adding streaming features
- Better separation of concerns (source → processor → output)
- Modern async patterns prevent UI blocking
- Matches professional audio software architecture

## Next Steps

1. ✓ **Broadcasting requirement identified** - CSCore is recommended
2. ✓ **Plan document created and approved**
3. **TOMORROW - Create feature branch**: `git checkout -b feature/cscore-sound-system`
4. **TOMORROW - Phase 1**: Install CSCore, test basic playback (30 min)
5. **TOMORROW - Phase 2-3**: Implement Music + Effects channels (2-4 hours)
6. **TOMORROW - Phase 4**: Create mixer for combined output (1 hour)
7. **TOMORROW - Phase 5-6**: Update API and test thoroughly (4 hours)
8. **Document progress**: Update this file after each phase

If CSCore fails (bugs, compatibility issues, >12 hours), fall back to NAudio multi-channel approach (Solution A).

### Commands to Start Tomorrow

```bash
# 1. Create feature branch
cd c:\Users\djtam\OneDrive\Documents\Coding\Project\Millionaire\TheMillionaireGame
git checkout -b feature/cscore-sound-system

# 2. Install CSCore
cd src\MillionaireGame
dotnet add package CSCore

# 3. Build to verify
cd ..
dotnet build TheMillionaireGame.sln

# 4. Start implementing MusicChannel.cs in src/MillionaireGame/Services/Audio/
```

### Files That Will Be Created
- `src/MillionaireGame/Services/Audio/MusicChannel.cs`
- `src/MillionaireGame/Services/Audio/EffectsChannel.cs`
- `src/MillionaireGame/Services/Audio/AudioMixer.cs` (optional)

### Files That Will Be Modified
- `src/MillionaireGame/Services/SoundService.cs` (major refactor)
- `src/MillionaireGame/Forms/ControlPanelForm.cs` (minimal - just method calls)

### Testing Checklist (Phase 6)
- [ ] Q1-4: Bed music loops without interruption
- [ ] Q5: Stop all sounds, play correct answer
- [ ] Q6+: Stop bed music, load new question with new music
- [ ] Reveal button: No freeze at any question level
- [ ] Question button: No freeze at Q6+
- [ ] Lock-in button: No freeze during safety net animation
- [ ] Lights Down button: No freeze at Q6+
- [ ] Stop All Audio button: Works instantly
- [ ] App shutdown: Clean exit with sounds playing
- [ ] 30+ minute session: No memory leaks

## Success Metrics

- ✓ No UI freezes when clicking any button
- ✓ Bed music plays continuously Q1-5 without interruption
- ✓ Music changes correctly at Q6+
- ✓ All effects play without affecting music
- ✓ App closes cleanly with sounds playing
- ✓ No memory leaks after 30+ minute gameplay session
- ✓ Code is more maintainable and understandable

---

**Last Updated:** December 25, 2025  
**Status:** Awaiting approval to begin Phase 1
