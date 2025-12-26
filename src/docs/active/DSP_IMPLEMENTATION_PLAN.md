# DSP (Digital Signal Processing) Implementation Plan
**Date**: December 25, 2025  
**Status**: ✅ **PHASE 1 & 2 COMPLETE** - Ready for Phase 3  
**Priority**: NORMAL  
**Dependencies**: Audio System (CSCore) - ✅ COMPLETE

---

## 🎉 IMPLEMENTATION STATUS UPDATE

### Phase 1: Core Infrastructure - ✅ COMPLETE
**Completed**: December 25, 2025  
**Status**: Fully implemented and tested

**Deliverables:**
- ✅ AudioCueQueue.cs (703 lines) - Queue engine with crossfading & silence detection
- ✅ Integrated RMS amplitude monitoring directly in Read() loop
- ✅ 50ms crossfades (configurable in config.xml)
- ✅ 250ms silence detection (configurable in config.xml)
- ✅ Fadeout DSP with linear 1.0→0.0 curve
- ✅ Thread-safe state management
- ✅ Null safety and division by zero protection

### Phase 2: Integration & Public API - ✅ COMPLETE
**Completed**: December 25, 2025  
**Status**: Fully integrated and tested

**Deliverables:**
- ✅ EffectsChannel integration (528 lines)
- ✅ SoundService public API (616 lines)
- ✅ QueueSound(), SkipToNext(), StopWithFadeout()
- ✅ GetTotalSoundCount(), GetQueueCount()
- ✅ Priority system (Normal/Immediate)
- ✅ DSPTestDialog for testing (395 lines)
- ✅ Comprehensive testing (5-sound sequence verified)

### Phase 3: Advanced DSP Effects - ⏳ PENDING
**Status**: Not started  
**Estimated Time**: 14-19 hours

**Planned Features:**
- Equalizer (3-band or parametric)
- Compressor (dynamics processing)
- Limiter (peak limiting)
- Reverb (optional enhancement)

### Phase 4: UI Implementation - ⏳ PENDING
**Status**: Not started  
**Estimated Time**: 6-8 hours

**Planned Features:**
- Settings UI in OptionsDialog
- Real-time queue monitoring
- Test/preview controls

---

## 📊 CURRENT IMPLEMENTATION DETAILS

---

## Overview

### Goal
Implement a comprehensive Digital Signal Processing (DSP) system using CSCore's DSP capabilities to allow real-time audio effects processing on both music and sound effects channels, plus automatic silence detection to eliminate long silent tails in audio files.

### Vision
Provide professional-grade audio processing with an intuitive UI that allows operators to apply effects like EQ, reverb, compression, and limiting to enhance the game show's audio production quality. Additionally, automatically detect and stop playback when sustained silence is encountered, eliminating unnecessary gaps caused by silent tails in audio files.

---

## Architecture Design

### DSP Pipeline Structure

```
┌─────────────────────────────────────────────────────┐
│              SoundService (482 lines)                │
│  - Routes sounds to appropriate channel             │
│  - Manages ApplicationSettings integration          │
└──────────┬─────────────────────────┬────────────────┘
           │                         │
  ┌────────▼─────────┐      ┌───────▼────────┐
  │  MusicChannel    │      │ EffectsChannel │
  │  (450 lines)     │      │  (420 lines)   │
  │  - Looping beds  │      │  - One-shots   │
  │  - MediaFound.   │      │  - MediaFound. │
  │    decoder       │      │    decoder     │
  └────────┬─────────┘      └───────┬────────┘
           │                        │
           │  Add DSP Here:         │  Add DSP Here:
           │  ┌──────────────────┐   │  ┌──────────────────┐
           │  │ SilenceDetector  │   │  │ SilenceDetector  │
           │  │  - Auto-stop     │   │  │  - Auto-stop     │
           │  └──────┬───────────┘   │  └──────┬───────────┘
           │         │                │         │
           │  ┌──────▼───────┐       │  ┌──────▼───────┐
           │  │ DSPProcessor │       │  │ DSPProcessor │
           │  │  - EQ        │       │  │  - EQ        │
           │  │  - Reverb    │       │  │  - Reverb    │
           │  │  - Compressor│       │  │  - Compressor│
           │  │  - Limiter   │       │  │  - Limiter   │
           │  └──────┬───────┘       │  └──────┬───────┘
           │         │                │         │
           │  ISampleSource           │  ISampleSource
           │                         │
  ┌────────▼─────────────────────────▼────────┐
  │         AudioMixer (447 lines)            │
  │  - WasapiOut with device selection        │
  │  - Initialize() + Start() + ChangeDevice  │
  │  - Playing state, continuous Read()       │
  └──────────────────┬───────────────────────┘
                     │
              ┌──────▼─────────┐
              │  WasapiOut     │
              │  (CSCore)      │
              └────────┬───────┘
                       │
                ┌──────▼──────────┐
                │  Audio Output   │
                │  (Speakers/etc) │
                └─────────────────┘
```

### Integration Points

**Option A: Pre-Mixer DSP (Recommended)**
- Apply DSP effects BEFORE mixing in AudioMixer
- Each channel (Music/Effects) has its own DSP chain
- Allows independent processing of music vs effects
- Better CPU distribution

**Option B: Post-Mixer DSP**
- Apply DSP effects AFTER mixing
- Single DSP chain for final output
- Simpler architecture but less flexible
- All sounds processed together

**DECISION: Use Option A** - Pre-Mixer DSP for maximum flexibility

---

## CSCore DSP Capabilities Research

### Available DSP Effects in CSCore

#### 0. **Silence Detection** (NEW)
- **Class**: `SilenceDetectorSource` (custom ISampleSource wrapper)
- **Features**: Automatic detection of sustained silence and early audio completion with smooth fadeout
- **Use Case**: Eliminate long silent tails in audio files, reduce dead air between cues
- **Parameters**: Threshold (dB), Silence duration (ms), Fadeout duration (ms), Apply to Music/Effects
- **Priority**: HIGH - Solves real user pain point, improves timing significantly
- **Implementation**: 
  - Monitors audio amplitude in real-time during Read()
  - Detects when amplitude drops below threshold (e.g., -60dB)
  - Requires sustained silence for configured duration (e.g., 100ms)
  - **Applies automatic fadeout when silence detected** (e.g., 20ms linear ramp to zero)
  - **Prevents DC pops/clicks** from abrupt audio termination
  - Stops reading early after fadeout completes, freeing resources immediately
  - Applied BEFORE DSP effects to detect silence in original audio

#### 0.5. **Audio Cue Queue with Automatic Crossfading** (NEW)
- **Class**: `AudioCueQueue` (custom queue manager for EffectsChannel)
- **Features**: Queue multiple audio cues with automatic crossfade transitions between them
- **Use Case**: Play sequential sounds without gaps, eliminate timing code in game logic
- **Parameters**: Crossfade duration (ms), Queue limit, Priority levels
- **Priority**: HIGH - Dramatically simplifies game logic, eliminates timing issues
- **Implementation**:
  - Queue system for pending audio cues (FIFO or priority-based)
  - Automatic crossfade between queued sounds (when 2+ in queue)
  - Configurable crossfade duration (e.g., 100-500ms)
  - **No gaps between sequential effects** - seamless transitions
  - Priority system: Normal queue vs immediate interrupt
  - Integration with EffectsChannel for fire-and-forget playback
  - **Eliminates need for manual timing in game code**
  - Preview: Shows queued effects and estimated completion time

#### 1. **Equalizer (EQ)**
- **Class**: `CSCore.DSP.Equalizer`
- **Features**: Multi-band parametric EQ
- **Use Case**: Tone shaping, frequency balance
- **Parameters**: Band frequency, gain, Q factor
- **Priority**: HIGH - Essential for professional sound

#### 2. **Reverb**
- **Class**: Custom implementation or third-party
- **Note**: CSCore doesn't include reverb out-of-box
- **Solution**: Implement convolution reverb or use DMO effects
- **Use Case**: Add space/ambience to sounds
- **Priority**: MEDIUM - Nice to have but not critical

#### 3. **Compressor**
- **Class**: Custom implementation needed
- **Features**: Dynamic range compression
- **Use Case**: Control volume consistency, prevent clipping
- **Parameters**: Threshold, ratio, attack, release, makeup gain
- **Priority**: HIGH - Important for broadcast-quality audio

#### 4. **Limiter**
- **Class**: Custom implementation needed
- **Features**: Hard ceiling on audio level
- **Use Case**: Prevent clipping and distortion
- **Parameters**: Threshold, release
- **Priority**: HIGH - Essential for safety

#### 5. **Noise Gate**
- **Class**: Custom implementation needed
- **Features**: Silence sounds below threshold
- **Use Case**: Clean up background noise
- **Priority**: LOW - Less relevant for pre-recorded sounds

#### 6. **Delay/Echo**
- **Class**: `CSCore.Streams.Effects.DmoEchoEffect` (DMO-based)
- **Features**: Echo effect using DirectX Media Objects
- **Use Case**: Special effects, creative sound design
- **Priority**: LOW - Creative enhancement

#### 7. **Chorus/Flanger**
- **Class**: `CSCore.Streams.Effects.DmoChorusEffect` (DMO-based)
- **Features**: Modulation effects
- **Use Case**: Creative sound enhancement
- **Priority**: LOW - Creative enhancement

### CSCore DSP Implementation Patterns

**Pattern 1: Silence Detection with Fadeout (First in Chain)**
```csharp
public class SilenceDetectorSource : ISampleSource
{
    private readonly ISampleSource _source;
    private readonly float _thresholdAmplitude;
    private readonly int _silenceDurationSamples;
    private readonly int _fadeoutSamples;
    private int _consecutiveSilentSamples = 0;
    private bool _silenceDetected = false;
    private bool _fadingOut = false;
    private int _fadeoutPosition = 0;
    
    public event EventHandler? SilenceDetected;
    
    public int Read(float[] buffer, int offset, int count)
    {
        if (_silenceDetected && !_fadingOut) return 0; // Already done
        
        int read = _source.Read(buffer, offset, count);
        if (read == 0) return 0;
        
        // If fading out, apply gain ramp
        if (_fadingOut)
        {
            for (int i = offset; i < offset + read; i++)
            {
                float fadeGain = 1.0f - ((float)_fadeoutPosition / _fadeoutSamples);
                buffer[i] *= Math.Max(0, fadeGain); // Linear fadeout
                _fadeoutPosition++;
                
                if (_fadeoutPosition >= _fadeoutSamples)
                {
                    _silenceDetected = true;
                    return i - offset + 1; // Return partial buffer
                }
            }
            return read;
        }
        
        // Analyze amplitude
        float maxAmplitude = 0;
        for (int i = offset; i < offset + read; i++)
            maxAmplitude = Math.Max(maxAmplitude, Math.Abs(buffer[i]));
        
        // Check if silent
        if (maxAmplitude < _thresholdAmplitude)
        {
            _consecutiveSilentSamples += read;
            if (_consecutiveSilentSamples >= _silenceDurationSamples)
            {
                // Start fadeout instead of abrupt stop (prevents DC pops!)
                _fadingOut = true;
                _fadeoutPosition = 0;
                SilenceDetected?.Invoke(this, EventArgs.Empty);
            }
        }
        else
        {
            _consecutiveSilentSamples = 0; // Reset on audio
        }
        
        return read;
    }
}
```

**Pattern 2: DSP Effects Processing**
```csharp
public class DspProcessor : ISampleSource
{
    private ISampleSource _source;
    private Equalizer _eq;
    private Compressor _compressor;
    private Limiter _limiter;
    
    public int Read(float[] buffer, int offset, int count)
    {
        // Read from source (potentially SilenceDetectorSource)
        int read = _source.Read(buffer, offset, count);
        
        // Apply EQ
        _eq.Process(buffer, offset, read);
        
        // Apply compressor
        _compressor.Process(buffer, offset, read);
        
        // Apply limiter
        _limiter.Process(buffer, offset, read);
        
        return read;
    }
}
```

**Pattern 3: Audio Cue Queue with Crossfading**
```csharp
public class AudioCueQueue
{
    private readonly Queue<AudioCue> _normalQueue = new();
    private AudioCue? _currentCue;
    private AudioCue? _nextCue;
    private bool _crossfading = false;
    private int _crossfadePosition = 0;
    private int _crossfadeDurationSamples;
    
    public void QueueAudio(string filePath, AudioPriority priority)
    {
        var cue = new AudioCue(filePath, priority);
        
        if (priority == AudioPriority.Immediate)
        {
            // Interrupt current, start crossfade immediately
            if (_currentCue != null)
            {
                _nextCue = cue;
                _crossfading = true;
                _crossfadePosition = 0;
            }
            else
            {
                _currentCue = cue;
            }
        }
        else
        {
            _normalQueue.Enqueue(cue);
            
            // If queue has 2+ items and not already crossfading, start transition
            if (_normalQueue.Count >= 2 && !_crossfading && _currentCue != null)
            {
                _nextCue = _normalQueue.Dequeue();
                _crossfading = true;
                _crossfadePosition = 0;
            }
        }
    }
    
    public int Read(float[] buffer, int offset, int count)
    {
        if (_currentCue == null) return 0;
        
        if (_crossfading && _nextCue != null)
        {
            // Read from both sources and crossfade
            float[] currentBuffer = new float[count];
            float[] nextBuffer = new float[count];
            
            int currentRead = _currentCue.Source.Read(currentBuffer, 0, count);
            int nextRead = _nextCue.Source.Read(nextBuffer, 0, count);
            
            for (int i = 0; i < Math.Max(currentRead, nextRead); i++)
            {
                float crossfadeProgress = (float)_crossfadePosition / _crossfadeDurationSamples;
                float currentGain = 1.0f - crossfadeProgress;
                float nextGain = crossfadeProgress;
                
                buffer[offset + i] = 
                    (i < currentRead ? currentBuffer[i] * currentGain : 0) +
                    (i < nextRead ? nextBuffer[i] * nextGain : 0);
                
                _crossfadePosition++;
                
                if (_crossfadePosition >= _crossfadeDurationSamples)
                {
                    // Crossfade complete
                    _currentCue = _nextCue;
                    _nextCue = null;
                    _crossfading = false;
                    _crossfadePosition = 0;
                    
                    // Check if more in queue
                    if (_normalQueue.Count > 0)
                    {
                        _nextCue = _normalQueue.Dequeue();
                        _crossfading = true;
                    }
                }
            }
            
            return Math.Max(currentRead, nextRead);
        }
        else
        {
            // Normal playback
            int read = _currentCue.Source.Read(buffer, offset, count);
            
            // If current cue finished and queue has items, start next
            if (read == 0 && _normalQueue.Count > 0)
            {
                _currentCue = _normalQueue.Dequeue();
                return Read(buffer, offset, count); // Recursive read from new cue
            }
            
            return read;
        }
    }
}

public enum AudioPriority { Normal, Immediate }
```

**Pattern 2: DMO Effects (Windows Built-in)**
```csharp
using CSCore.Streams.Effects;

// Create DMO effect
var chorus = new DmoChorusEffect(_waveSource);
var echo = new DmoEchoEffect(_waveSource);

// Chain effects
_waveSource = echo.ToSampleSource();
```

---

## UI Design

### DSP Tab in OptionsDialog

**Location**: Insert between `tabPageSoundpack` and `tabPageMixer` in `tabControlSounds`

**Tab Order:**
1. Soundpack Tab (existing)
2. **DSP Tab** ← NEW
3. Mixer Tab (existing)

### DSP Tab Layout

```
┌─────────────────────────────────────────────────────────┐
│ [DSP Settings]                                          │
│                                                         │
│ ┌─ Master Enable ────────────────────────────────────┐ │
│ │ [✓] Enable DSP Processing                          │ │
│ │                                                     │ │
│ │ Apply To: (*) Music   ( ) Effects   ( ) Both       │ │
│ └─────────────────────────────────────────────────────┘ │
│                                                         │
│ ┌─ Silence Detection ───────────────────────────────┐  │
│ │ [✓] Enable Automatic Silence Detection            │  │
│ │                                                    │  │
│ │ Threshold: [-80] ◄───●────► [-20] dB             │  │
│ │             └─ -60dB (default)                    │  │
│ │                                                    │  │
│ │ Duration:  [50]  ◄───●────► [500] ms             │  │
│ │             └─ 100ms (default)                    │  │
│ │                                                    │  │
│ │ Fadeout:   [5]   ◄───●────► [100] ms             │  │
│ │             └─ 20ms (default)                     │  │
│ │                                                    │  │
│ │ Apply To:  [✓] Effects   [ ] Music                │  │
│ │                                                    │  │
│ │ ℹ️ Stops audio when sustained silence detected,   │  │
│ │    with smooth fadeout to prevent clicks          │  │
│ │                                                    │  │
│ │ [Reset to Default]                                │  │
│ └────────────────────────────────────────────────────┘  │
│                                                         │
│ ┌─ Audio Cue Queue ────────────────────────────────┐  │
│ │ [✓] Enable Automatic Crossfading                 │  │
│ │                                                    │  │
│ │ Crossfade: [50]  ◄───●────► [1000] ms            │  │
│ │             └─ 200ms (default)                    │  │
│ │                                                    │  │
│ │ Queue Limit: [5] ◄───●────► [20] sounds          │  │
│ │               └─ 10 (default)                     │  │
│ │                                                    │  │
│ │ ℹ️ Automatically crossfades between queued sounds, │  │
│ │    eliminating gaps and timing code requirements  │  │
│ │                                                    │  │
│ │ Queued: [0 sounds]  Est. Time: [0.0s]            │  │
│ │                                                    │  │
│ │ [Reset to Default]  [Clear Queue]                │  │
│ └────────────────────────────────────────────────────┘  │
│                                                         │
│ ┌─ Equalizer (EQ) ──────────────────────────────────┐  │
│ │ [✓] Enable EQ                                      │  │
│ │                                                    │  │
│ │ Low:     [-10] ◄──────●─────► [+10] dB           │  │
│ │ Mid:     [-10] ◄──────●─────► [+10] dB           │  │
│ │ High:    [-10] ◄──────●─────► [+10] dB           │  │
│ │                                                    │  │
│ │ [Reset to Flat]                                   │  │
│ └────────────────────────────────────────────────────┘  │
│                                                         │
│ ┌─ Compressor ──────────────────────────────────────┐  │
│ │ [✓] Enable Compressor                             │  │
│ │                                                    │  │
│ │ Threshold: [-20] ◄───●─────► [0] dB              │  │
│ │ Ratio:     [1:1] ◄───●─────► [10:1]              │  │
│ │ Attack:    [1]   ◄───●─────► [100] ms            │  │
│ │ Release:   [10]  ◄───●─────► [1000] ms           │  │
│ │ Makeup:    [0]   ◄───●─────► [+20] dB            │  │
│ │                                                    │  │
│ │ [Reset to Default]                                │  │
│ └────────────────────────────────────────────────────┘  │
│                                                         │
│ ┌─ Limiter ─────────────────────────────────────────┐  │
│ │ [✓] Enable Limiter                                │  │
│ │                                                    │  │
│ │ Ceiling:   [-1]  ◄───●─────► [0] dB              │  │
│ │ Release:   [10]  ◄───●─────► [1000] ms           │  │
│ │                                                    │  │
│ │ [Reset to Default]                                │  │
│ └────────────────────────────────────────────────────┘  │
│                                                         │
│ ┌─ Presets ─────────────────────────────────────────┐  │
│ │ [Load Preset ▼]                                   │  │
│ │   - Broadcast Standard                            │  │
│ │   - Cinematic                                     │  │
│ │   - Game Show                                     │  │
│ │   - Custom...                                     │  │
│ │                                                    │  │
│ │ [Save Current as Preset...]                       │  │
│ └────────────────────────────────────────────────────┘  │
│                                                         │
│ [Apply] [Reset All] [Cancel]                           │
└─────────────────────────────────────────────────────────┘
```

### Control Specifications

**Master Controls:**
- Checkbox: Enable/Disable entire DSP system
- Radio Buttons: Apply to Music only, Effects only, or Both channels
- Instant apply on checkbox/radio change

**Equalizer Section:**
- 3-band EQ (Low/Mid/High) with track bars
- Range: -10dB to +10dB, default 0dB (flat)
- Reset button returns all to 0dB
- Real-time preview when checked

**Compressor Section:**
- Threshold slider (-20dB to 0dB)
- Ratio selector (1:1 to 10:1)
- Attack time (1ms to 100ms)
- Release time (10ms to 1000ms)
- Makeup gain (0dB to +20dB)
- Reset button returns to sensible defaults

**Limiter Section:**
- Ceiling slider (-1dB to 0dB, default -0.5dB)
- Release time (10ms to 1000ms)
- Reset button returns to defaults

**Presets System:**
- ComboBox with named presets
- Save current settings as new preset
- Presets stored in ApplicationSettings

---

## Implementation Plan

### Phase 1: DSP Infrastructure (PRIORITY: HIGH)

**Goal**: Create core DSP processing classes including silence detection

**Tasks:**
1. Create `SilenceDetectorSource.cs` class (NEW - HIGH PRIORITY)
   - Implements ISampleSource wrapper
   - Monitors amplitude during Read()
   - Detects threshold (dB to linear amplitude conversion)
   - Tracks sustained silence duration
   - **Applies automatic fadeout when silence detected** (prevents DC pops)
   - **Configurable fadeout duration** (5-100ms, default 20ms)
   - **Linear gain ramp to zero** during fadeout period
   - Fires SilenceDetected event
   - Returns 0 to stop reading early (after fadeout completes)
   - Configurable threshold, silence duration, and fadeout duration
   - Debug logging for troubleshooting

1b. Create `AudioCueQueue.cs` class (NEW - HIGH PRIORITY)
   - Queue manager for sequential audio playback
   - FIFO queue for normal priority cues
   - Immediate interrupt priority option
   - **Automatic crossfade between queued sounds** (when 2+ in queue)
   - Configurable crossfade duration (50-1000ms, default 200ms)
   - **Eliminates gaps between sequential effects**
   - Preview functionality (queued count, estimated time)
   - Queue limit enforcement
   - Integration with EffectsChannel
   - Debug logging for queue state

2. Create `DSPProcessor.cs` class
   - Implements ISampleSource
   - Wraps another ISampleSource (potentially SilenceDetectorSource)
   - Applies DSP chain in Read() method
   - Enable/disable flag

3. Create `Equalizer.cs` class
   - 3-band parametric EQ
   - Low/Mid/High frequency control
   - Gain adjustment per band
   - Process(buffer, offset, count) method

3. Create `Compressor.cs` class
   - Dynamic range compression
   - Threshold, ratio, attack, release, makeup gain
   - RMS level detection
   - Process(buffer, offset, count) method

4. Create `Limiter.cs` class
   - Hard ceiling limiter
   - Threshold and release time
   - Lookahead buffer for artifact-free limiting
   - Process(buffer, offset, count) method

**Testing:**
- Unit tests for each DSP class
- Verify audio passes through unchanged when disabled
- Verify effects apply correctly when enabled
- Performance testing (CPU usage)

### Phase 2: Channel Integration (PRIORITY: HIGH)

**Goal**: Integrate DSP into MusicChannel and EffectsChannel

**Tasks:**
1. Modify `MusicChannel.cs`
   - Add SilenceDetectorSource field
   - Add DspProcessor field
   - Wrap _waveSource with SilenceDetector FIRST, then DSPProcessor
   - Enable/disable based on settings
   - Example:
     ```csharp
     _waveSource = new MediaFoundationDecoder(filePath);
     // Silence detection FIRST (on original audio)
     if (_silenceDetectionEnabled)
         _waveSource = new SilenceDetectorSource(_waveSource, _silenceThresholdDb, _silenceDurationMs);
     // DSP effects SECOND
     if (_dspEnabled)
         _waveSource = new DspProcessor(_waveSource, _dspSettings);
     // Volume/Fade LAST
     _waveSource = new VolumeSource(_waveSource);
     ```

2. Modify `EffectsChannel.cs`
   - Integrate AudioCueQueue for sequential playback
   - Add QueueEffect() method (enqueues instead of immediate play)
   - Add PlayEffect() method with priority parameter
   - Wrap decoded audio with SilenceDetectorSource FIRST
   - Then wrap with DSPProcessor
   - Wire up SilenceDetected event for logging
   - Share or separate DSP settings (discuss)
   - Expose queue state (count, estimated time)
   - Note: Silence detection typically enabled for Effects, disabled for Music (looping)
   - Note: Crossfading only applies to queued effects, not immediate interrupts

3. Add DSP, Silence Detection, and Queue methods to SoundService
   - `EnableDsp(bool music, bool effects)`
   - `UpdateDspSettings(DspSettings settings)`
   - `GetDspSettings()` returns current settings
   - `EnableSilenceDetection(bool music, bool effects)` (NEW)
   - `UpdateSilenceDetectionSettings(SilenceDetectionSettings settings)` (NEW)
   - `GetSilenceDetectionSettings()` returns current settings (NEW)
   - `QueueSound(string key, AudioPriority priority = Normal)` (NEW)
   - `GetQueuedSoundCount()` returns queued effects count (NEW)
   - `GetEstimatedQueueTime()` returns estimated completion time (NEW)
   - `ClearSoundQueue()` clears all queued effects (NEW)
   - `EnableCrossfading(bool enabled)` (NEW)
   - `UpdateCrossfadeSettings(CrossfadeSettings settings)` (NEW)

**Testing:**
- Test music playback with DSP enabled/disabled
- Test effects playback with DSP enabled/disabled
- Verify no audio glitches during playback
- Test changing DSP settings during playback

### Phase 3: Settings Persistence (PRIORITY: MEDIUM)

**Goal**: Save/load DSP settings in ApplicationSettings

**Tasks:**
1. Create `DspSettings.cs`, `SilenceDetectionSettings.cs`, and `CrossfadeSettings.cs` in MillionaireGame.Core
   ```csharp
   // Silence Detection Settings (NEW)
   public class SilenceDetectionSettings
   {
       public bool Enabled { get; set; } = true;
       public float ThresholdDb { get; set; } = -60f;
       public int SilenceDurationMs { get; set; } = 100;
       public int FadeoutDurationMs { get; set; } = 20; // Prevents DC pops
       public bool ApplyToMusic { get; set; } = false; // Looping music shouldn't auto-stop
       public bool ApplyToEffects { get; set; } = true; // One-shot effects should
   }
   
   // Audio Cue Queue Settings (NEW)
   public class CrossfadeSettings
   {
       public bool Enabled { get; set; } = true;
       public int CrossfadeDurationMs { get; set; } = 200;
       public int QueueLimit { get; set; } = 10;
       public bool AutoCrossfade { get; set; } = true; // Crossfade when 2+ in queue
   }
   
   // DSP Effects Settings
   public class DspSettings
   {
       public bool Enabled { get; set; }
       public DspTarget Target { get; set; } // Music, Effects, Both
       
       public bool EqEnabled { get; set; }
       public float EqLowGain { get; set; }
       public float EqMidGain { get; set; }
       public float EqHighGain { get; set; }
       
       public bool CompressorEnabled { get; set; }
       public float CompressorThreshold { get; set; }
       public float CompressorRatio { get; set; }
       public float CompressorAttack { get; set; }
       public float CompressorRelease { get; set; }
       public float CompressorMakeup { get; set; }
       
       public bool LimiterEnabled { get; set; }
       public float LimiterCeiling { get; set; }
       public float LimiterRelease { get; set; }
   }
   
   public enum DspTarget
   {
       Music,
       Effects,
       Both
   }
   ```

2. Add DspSettings, SilenceDetectionSettings, and CrossfadeSettings to ApplicationSettings.cs
   - Add `DspSettings DspSettings { get; set; }` property
   - Add `SilenceDetectionSettings SilenceDetection { get; set; }` property (NEW)
   - Add `CrossfadeSettings Crossfade { get; set; }` property (NEW)
   - Serialize/deserialize with JSON
   - Provide sensible defaults

3. Update ApplicationSettingsManager.cs
   - Load DSP settings on startup
   - Save DSP settings on change
   - Validate ranges and sanity check

**Testing:**
- Verify settings save correctly
- Verify settings load on restart
- Test with missing/corrupted settings file
- Verify defaults applied when no settings exist

### Phase 4: UI Implementation (PRIORITY: MEDIUM)

**Goal**: Create DSP tab in OptionsDialog

**Tasks:**
1. Add `tabPageDSP` to `tabControlSounds` in OptionsDialog.Designer.cs
   - Insert between tabPageSoundpack (index 0) and tabPageMixer (index 1)
   - Set Text property to "DSP"
   - Design layout per mockup above

2. Add controls for master settings
   - CheckBox: chkEnableDsp
   - RadioButtons: rbDspMusic, rbDspEffects, rbDspBoth

3. Add Silence Detection controls (NEW)
   - GroupBox: grpSilenceDetection
   - CheckBox: chkSilenceDetectionEnabled
   - TrackBar: trkSilenceThreshold (-80 to -20 dB)
   - TrackBar: trkSilenceDuration (50 to 500 ms)
   - TrackBar: trkSilenceFadeout (5 to 100 ms) - NEW
   - CheckBox: chkApplyToMusic, chkApplyToEffects
   - Label: lblSilenceInfo (info text)
   - Button: btnSilenceDetectionReset

3b. Add Audio Cue Queue controls (NEW)
   - GroupBox: grpAudioCueQueue
   - CheckBox: chkCrossfadeEnabled
   - TrackBar: trkCrossfadeDuration (50 to 1000 ms)
   - TrackBar: trkQueueLimit (5 to 20 sounds)
   - Label: lblQueuedCount (shows current queued count)
   - Label: lblEstimatedTime (shows estimated completion time)
   - Label: lblCrossfadeInfo (info text)
   - Button: btnClearQueue
   - Button: btnCrossfadeReset

4. Add EQ controls
   - GroupBox: grpEqualizer
   - CheckBox: chkEqEnabled
   - TrackBar: trkEqLow, trkEqMid, trkEqHigh
   - Labels for values
   - Button: btnEqReset

5. Add Compressor controls
   - GroupBox: grpCompressor
   - CheckBox: chkCompressorEnabled
   - TrackBar: trkCompThreshold, trkCompRatio, trkCompAttack, trkCompRelease, trkCompMakeup
   - Labels for values
   - Button: btnCompressorReset

6. Add Limiter controls
   - GroupBox: grpLimiter
   - CheckBox: chkLimiterEnabled
   - TrackBar: trkLimiterCeiling, trkLimiterRelease
   - Labels for values
   - Button: btnLimiterReset

7. Add Preset controls
   - ComboBox: cmbPresets
   - Button: btnSavePreset

8. Wire up event handlers
   - CheckBox changes trigger immediate apply
   - TrackBar ValueChanged events update labels and settings
   - Reset buttons restore defaults
   - Preset loading/saving

**Testing:**
- Test all controls respond correctly
- Verify real-time updates work
- Test reset buttons restore defaults
- Test preset system saves/loads
- Verify settings persist across dialog open/close

### Phase 5: Presets System (PRIORITY: LOW)

**Goal**: Provide ready-to-use DSP presets

**Tasks:**
1. Create preset definitions
   - **Broadcast Standard**: Safe broadcast levels, gentle compression
   - **Cinematic**: Wide dynamic range, subtle EQ
   - **Game Show**: High energy, aggressive compression, bright EQ

2. Implement preset loading
   - ComboBox populated with preset names
   - Selection loads preset values into controls
   - Apply immediately to audio

3. Implement preset saving
   - Dialog to name new preset
   - Save current settings as custom preset
   - Store in ApplicationSettings

4. Preset management
   - Delete custom presets
   - Reset to factory presets
   - Import/export presets (JSON)

**Testing:**
- Load each preset and verify sound
- Save custom preset and reload
- Delete custom preset
- Import/export presets

### Phase 6: Advanced Features (PRIORITY: LOW - FUTURE)

**Optional enhancements for future versions:**

1. **Reverb Effect**
   - Research convolution reverb implementation
   - Or use DMO reverb effect
   - Add reverb controls to UI

2. **Visual Feedback**
   - Real-time level meters showing input/output
   - Spectrum analyzer showing frequency response
   - Gain reduction meter for compressor

3. **Per-Soundpack DSP**
   - Store DSP settings per soundpack
   - Automatically load when soundpack changes
   - Override global DSP settings

4. **A/B Comparison**
   - Toggle button to bypass DSP instantly
   - Compare processed vs unprocessed sound
   - Helps fine-tune settings

5. **Auto-Gain Staging**
   - Analyze audio levels and suggest optimal DSP settings
   - Prevent clipping and maintain consistent volume
   - "Auto-Configure" button

---

## Technical Considerations

### Performance

**CPU Usage:**
- DSP processing adds computational overhead
- Each effect requires buffer processing
- Target: <5% CPU usage on modern hardware
- Optimization: Use SIMD (Vector<T>) for buffer operations

**Latency:**
- Real-time processing should not introduce noticeable delay
- Target: <10ms additional latency
- Lookahead buffers in limiter may add 1-2ms
- Acceptable for game show context (not live music)

**Memory:**
- DSP processors hold small buffers (typically <1MB)
- Preset storage minimal (JSON, <10KB)
- No significant memory concerns

### Audio Quality

**Sample Rate:**
- Maintain 44.1kHz/48kHz throughout DSP chain
- No resampling within DSP processing
- Match WasapiOut output sample rate

**Bit Depth:**
- Process in 32-bit float for maximum headroom
- No quantization until final output
- CSCore ISampleSource uses float[]

**Clipping Prevention:**
- Limiter as final stage prevents digital clipping
- Headroom management throughout chain
- Visual indicators if clipping detected

### Backwards Compatibility

**Graceful Degradation:**
- If DSP settings missing, use defaults
- If DSP disabled, audio bypasses processing
- No impact on existing code if DSP not enabled

**Settings Migration:**
- Old settings files without DSP continue to work
- DSP settings added seamlessly
- Version check in ApplicationSettings

---

## Testing Checklist

### Unit Tests
- [ ] Equalizer processes audio correctly
- [ ] Compressor reduces dynamic range as expected
- [ ] Limiter prevents clipping above ceiling
- [ ] DSPProcessor chains effects correctly
- [ ] Bypassed DSP passes audio unchanged
- [ ] Settings serialize/deserialize correctly

### Integration Tests
- [ ] Music plays with DSP enabled
- [ ] Effects play with DSP enabled
- [ ] Music plays with DSP disabled
- [ ] Effects play with DSP disabled
- [ ] DSP settings apply in real-time
- [ ] Device hot-swap doesn't break DSP
- [ ] Multiple sounds with DSP don't glitch

### UI Tests
- [ ] DSP tab appears between Soundpack and Mixer
- [ ] All controls respond correctly
- [ ] TrackBars update value labels
- [ ] Reset buttons restore defaults
- [ ] Presets load correctly
- [ ] Custom preset saving works
- [ ] Settings persist across sessions

### Performance Tests
- [ ] CPU usage acceptable (<5%)
- [ ] No audio dropouts during DSP
- [ ] Latency acceptable (<10ms)
- [ ] Memory usage reasonable

### Audio Quality Tests
- [ ] No clipping with limiter enabled
- [ ] EQ changes audible and accurate
- [ ] Compressor smooths dynamics as expected
- [ ] No artifacts or distortion
- [ ] A/B comparison sounds professional

---

## Success Criteria

### Must Have (MVP)
- ✅ DSP tab in OptionsDialog between Soundpack and Mixer
- ✅ Enable/disable DSP processing
- ✅ **Silence Detection with threshold, duration, and fadeout controls** (NEW)
- ✅ **Automatic fadeout on silence detection (prevents DC pops)** (NEW)
- ✅ **Audio Cue Queue with automatic crossfading** (NEW)
- ✅ **Seamless transitions between queued sounds (no gaps)** (NEW)
- ✅ **Apply silence detection to Effects only by default** (NEW)
- ✅ 3-band Equalizer with reset
- ✅ Compressor with all standard parameters
- ✅ Limiter with ceiling and release
- ✅ Settings persist across sessions
- ✅ Apply to Music, Effects, or Both channels
- ✅ Real-time parameter updates

### Should Have
- ✅ At least 3 factory presets
- ✅ Custom preset saving
- ✅ Reset all button
- ✅ Visual feedback (value labels)
- ✅ Performance: <5% CPU, <10ms latency

### Nice to Have (Future)
- ⏳ Reverb effect
- ⏳ Visual level meters
- ⏳ Spectrum analyzer
- ⏳ Per-soundpack DSP settings
- ⏳ A/B comparison toggle
- ⏳ Auto-configuration wizard

---

## Timeline Estimate

**Phase 1: DSP Infrastructure** - 14-19 hours
- SilenceDetectorSource class (with fadeout): 3 hours (NEW)
- AudioCueQueue class (with crossfading): 4 hours (NEW)
- Equalizer class: 3 hours
- Compressor class: 4 hours
- Limiter class: 3 hours
- DSPProcessor wrapper: 2 hours
- Testing silence detection & queue: 2 hours (NEW)

**Phase 2: Channel Integration** - 7-9 hours
- MusicChannel integration (w/ SilenceDetector): 2.5 hours
- EffectsChannel integration (w/ SilenceDetector + Queue): 3.5 hours
- SoundService methods (DSP + Silence + Queue): 3 hours

**Phase 3: Settings Persistence** - 5-6 hours
- DspSettings class: 1 hour
- SilenceDetectionSettings class (w/ fadeout): 0.5 hours (NEW)
- CrossfadeSettings class: 0.5 hours (NEW)
- ApplicationSettings integration: 3 hours

**Phase 4: UI Implementation** - 15-20 hours
- Tab and layout design: 3 hours
- Silence Detection controls (w/ fadeout): 2.5 hours (NEW)
- Audio Cue Queue controls: 2.5 hours (NEW)
- DSP controls creation and wiring: 5 hours
- Event handlers and logic: 6 hours
- Polish and refinement: 3 hours

**Phase 5: Presets System** - 3-5 hours
- Preset definitions: 1 hour
- Load/save logic: 2 hours
- UI integration: 2 hours

**Phase 6: Testing** - 6-10 hours
- Unit tests: 3 hours
- Integration tests: 3 hours
- Audio quality verification: 2 hours
- Performance testing: 2 hours

**TOTAL ESTIMATE: 50-69 hours** (6.5-9 working days)

**Note**: Silence Detection (with fadeout) adds ~7 hours, and Audio Cue Queue (with crossfading) adds ~10 hours to the timeline. Both features provide significant user value:
- **Silence Detection**: Eliminates silent tails, prevents DC pops/clicks
- **Audio Cue Queue**: Seamless transitions, eliminates timing code in game logic

---

## Dependencies

### Required
- ✅ CSCore 1.2.1.2 installed
- ✅ Audio system operational (MusicChannel, EffectsChannel, AudioMixer)
- ✅ ApplicationSettings system working
- ✅ OptionsDialog with nested TabControl

### Optional
- ⏳ Third-party DSP libraries (if custom implementation insufficient)
- ⏳ SIMD optimization libraries (System.Numerics.Vectors)

---

## Risk Assessment

### Technical Risks

**Risk 1: DSP Processing Causes Audio Glitches**
- **Likelihood**: Medium
- **Impact**: High
- **Mitigation**: Extensive testing, buffer size tuning, CPU profiling
- **Fallback**: Disable DSP if glitches detected

**Risk 2: CPU Usage Too High**
- **Likelihood**: Low
- **Impact**: Medium
- **Mitigation**: Optimize algorithms, use SIMD, profile hot paths
- **Fallback**: Reduce DSP chain complexity or disable by default

**Risk 3: Settings Corruption**
- **Likelihood**: Low
- **Impact**: Low
- **Mitigation**: Validation on load, defaults on error, JSON schema
- **Fallback**: Reset to factory defaults

**Risk 4: UI Complexity Overwhelms Users**
- **Likelihood**: Medium
- **Impact**: Low
- **Mitigation**: Presets make it simple, good defaults, help text
- **Fallback**: Hide advanced controls, "Simple" vs "Advanced" mode

### Schedule Risks

**Risk 1: Implementation Takes Longer Than Expected**
- **Likelihood**: Medium
- **Impact**: Low (not blocking other features)
- **Mitigation**: Break into phases, MVP first, iterate
- **Fallback**: Ship MVP without advanced features

**Risk 2: Testing Reveals Major Issues**
- **Likelihood**: Low
- **Impact**: Medium
- **Mitigation**: Early testing, incremental development
- **Fallback**: Disable problematic features, ship core functionality

---

## Next Steps

1. **Review Plan**: Get feedback on architecture and UI design
2. **Create Feature Branch**: `git checkout -b feature/dsp-implementation`
3. **Start Phase 1**: Begin with DSP infrastructure classes
4. **Iterate and Test**: Build incrementally, test continuously
5. **Document as You Go**: Update this plan with discoveries and changes

---

## Questions to Resolve

1. **Q**: Should silence detection threshold be same for Music and Effects?
   - **A**: TBD - Recommend global settings but allow independent enable/disable per channel

2. **Q**: Should DSP settings be per-soundpack or global?
   - **A**: TBD - Start with global, add per-soundpack later if needed

3. **Q**: Should we use DMO effects or custom implementations?
   - **A**: TBD - Custom for EQ/Comp/Limiter (more control), DMO for reverb/effects

4. **Q**: Real-time parameter updates or apply on "OK"?
   - **A**: TBD - Prefer real-time for professional feel, but may need performance tuning

5. **Q**: Visual feedback (meters) in MVP or Phase 6?
   - **A**: TBD - Phase 6 (nice to have), focus on functionality first

6. **Q**: Separate DSP settings for Music vs Effects by default?
   - **A**: TBD - Single setting that applies to both, with option to target specific channel

---

## References

- [CSCore Documentation](https://github.com/filoe/cscore)
- [CSCore Samples](https://github.com/filoe/cscore/tree/master/Samples)
- [Audio DSP Principles](https://en.wikipedia.org/wiki/Audio_signal_processing)
- [Compression/Limiting Theory](https://www.soundonsound.com/techniques/compression-made-easy)
- [Equalizer Design](https://www.musictech.net/guides/essential-guide/how-to-use-eq-like-a-pro/)
- [Voice Activity Detection (Silence Detection)](https://en.wikipedia.org/wiki/Voice_activity_detection)
- [Silence Detection Proposal](SILENCE_DETECTION_PROPOSAL.md) - Detailed technical design

---

**Plan Status**: ✅ COMPLETE - Ready to begin implementation  
**Next Action**: Create feature branch and start Phase 1
