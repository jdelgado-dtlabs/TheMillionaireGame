# Silence Detection & Auto-Completion Feature
**Date**: December 25, 2025  
**Status**: ✅ IMPLEMENTED (December 27, 2025)  
**Priority**: HIGH  
**Dependencies**: Audio System (CSCore) - ✅ COMPLETE

---

## IMPLEMENTATION SUMMARY

The silence detection system has been **successfully implemented** and integrated into the audio queue system.

### Implementation Status: ✅ COMPLETE

**Key Features Implemented:**
- ✅ RMS amplitude-based silence detection  
- ✅ Configurable threshold (-40dB default, customizable per sound)
- ✅ Configurable silence duration (250ms default)
- ✅ Initial delay before detection starts (2500ms default)
- ✅ Custom threshold overrides per sound (e.g., -35dB for lights down)
- ✅ Integrated into AudioCueQueue for seamless crossfading
- ✅ SilenceDetectorSource wrapper class for ISampleSource compatibility

**Implementation Files:**
- **AudioCueQueue.cs** (703 lines): Main queue engine with integrated silence detection
- **SilenceDetectorSource.cs** (155 lines): ISampleSource wrapper with silence monitoring
- **EffectsChannel.cs** (528 lines): Integration layer with custom threshold support
- **SoundService.cs** (616 lines): Public API with QueueSound methods

**Configuration:**
```csharp
// Application settings
SilenceDetectionSettings
{
    ThresholdDb = -40.0,           // -40dB silence threshold
    SilenceDurationMs = 250,       // 250ms of silence to trigger
    FadeoutDurationMs = 50,        // 50ms fade when stopping
    InitialDelayMs = 2500          // 2.5s before detection starts
}

// Custom threshold per sound
QueueSound(SoundEffect.LightsDown, 
           AudioPriority.Normal, 
           customThresholdDb: -35.0);  // Less sensitive for specific sound
```

**Testing Results:**
- ✅ Q1-Q5 sequence tested successfully
- ✅ FFF intro sequence working correctly
- ✅ Custom thresholds functioning as expected
- ✅ No premature cutoffs or gaps between sounds
- ✅ Initial delay prevents early detection during fade-ins

**Next Steps:**
- 🎯 **Phase 4: UI Implementation** - Create settings UI in OptionsDialog for configuring thresholds, durations, and initial delays
- See UI_SETTINGS_IMPLEMENTATION_PLAN.md for detailed planning

---

## ORIGINAL PROPOSAL (REFERENCE)

---

## Problem Statement

### Current Behavior
Audio files with long silent tails at the end cause unnecessary gaps between audio cues because the system plays the entire file duration, even when the actual audible content ends much earlier.

**Example:**
```
[Audio Content]████████▓▓▒▒░░░░░░░░░░░░░░░░░░░░░░░░
                       ↑                        ↑
                  Audio ends              File ends
                  @ 3.2s                  @ 5.0s
                  
Gap created: 1.8 seconds of unnecessary silence
```

### User Impact
- Delayed response between audio cues
- Unprofessional "dead air" feeling
- Timing issues in fast-paced game show scenarios
- Operator frustration waiting for silent tails to finish

---

## Proposed Solution: Automatic Silence Detection

### Feature Overview
Implement a real-time silence detection engine that monitors audio amplitude during playback and automatically stops when the audio drops below a configurable threshold for a sustained period.

### Key Concepts

**1. Amplitude Threshold (dB)**
- Monitor RMS (Root Mean Square) or peak amplitude of audio samples
- Compare against threshold (e.g., -60dB = very quiet background noise level)
- Below threshold = "silent"

**2. Silence Duration (ms)**
- Require sustained silence to avoid false triggers
- Example: 100ms of continuous silence = truly done
- Prevents stopping during brief pauses in audio

**3. Completion Signal**
- When silence detected, mark effect as "complete"
- Stop reading from source early
- Free resources immediately
- Trigger next audio cue without delay

### Technical Implementation

#### Architecture Addition

```
┌─────────────────────────────────────────────┐
│          EffectsChannel.PlayEffect()        │
│  - Load audio file (MediaFoundationDecoder) │
└────────────────────┬────────────────────────┘
                     │
          ┌──────────▼──────────┐
          │  ISampleSource      │
          │  (Decoded Audio)    │
          └──────────┬──────────┘
                     │
          ┌──────────▼──────────────────┐
          │  SilenceDetectorSource      │  ← NEW
          │  - Monitors amplitude       │
          │  - Detects sustained silence│
          │  - Stops early if detected  │
          └──────────┬──────────────────┘
                     │
          ┌──────────▼──────────┐
          │  VolumeSource       │
          │  (Volume control)   │
          └──────────┬──────────┘
                     │
          ┌──────────▼──────────┐
          │  FadeOutSource      │
          │  (Smooth ending)    │
          └──────────┬──────────┘
                     │
          ┌──────────▼──────────┐
          │  EffectsMixerSource │
          │  (Mix with others)  │
          └─────────────────────┘
```

#### New Class: SilenceDetectorSource

```csharp
/// <summary>
/// ISampleSource wrapper that detects silence and signals early completion.
/// Monitors audio amplitude and stops reading when sustained silence detected.
/// </summary>
public class SilenceDetectorSource : ISampleSourceBase, ISampleSource
{
    private readonly ISampleSource _source;
    private readonly float _thresholdAmplitude;
    private readonly int _silenceDurationSamples;
    
    private int _consecutiveSilentSamples = 0;
    private bool _silenceDetected = false;
    
    /// <summary>
    /// Fired when sustained silence is detected
    /// </summary>
    public event EventHandler? SilenceDetected;
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="source">The audio source to monitor</param>
    /// <param name="thresholdDb">Silence threshold in dB (e.g., -60dB)</param>
    /// <param name="silenceDurationMs">How long silence must be sustained (e.g., 100ms)</param>
    public SilenceDetectorSource(ISampleSource source, float thresholdDb, int silenceDurationMs)
    {
        _source = source;
        
        // Convert dB to linear amplitude
        // -60dB = 0.001 amplitude, -40dB = 0.01, -20dB = 0.1
        _thresholdAmplitude = (float)Math.Pow(10, thresholdDb / 20.0);
        
        // Convert milliseconds to sample count
        _silenceDurationSamples = (int)(silenceDurationMs * source.WaveFormat.SampleRate / 1000.0);
    }
    
    public override int Read(float[] buffer, int offset, int count)
    {
        // If silence already detected, stop reading
        if (_silenceDetected)
            return 0;
        
        int read = _source.Read(buffer, offset, count);
        
        if (read == 0)
            return 0;
        
        // Analyze amplitude of samples read
        float maxAmplitude = 0;
        for (int i = offset; i < offset + read; i++)
        {
            float absValue = Math.Abs(buffer[i]);
            if (absValue > maxAmplitude)
                maxAmplitude = absValue;
        }
        
        // Check if below threshold (silent)
        if (maxAmplitude < _thresholdAmplitude)
        {
            _consecutiveSilentSamples += read;
            
            // Check if silence sustained long enough
            if (_consecutiveSilentSamples >= _silenceDurationSamples)
            {
                _silenceDetected = true;
                SilenceDetected?.Invoke(this, EventArgs.Empty);
                
                if (Program.DebugMode)
                {
                    GameConsole.WriteLine(
                        $"[SilenceDetector] Silence detected after {_consecutiveSilentSamples} samples " +
                        $"({_consecutiveSilentSamples / (float)WaveFormat.SampleRate:F2}s)",
                        ConsoleColor.Cyan
                    );
                }
                
                return 0; // Stop reading
            }
        }
        else
        {
            // Reset counter if audio detected
            _consecutiveSilentSamples = 0;
        }
        
        return read;
    }
    
    public override long Position
    {
        get => _source.Position;
        set => _source.Position = value;
    }
    
    public override long Length => _source.Length;
    
    public override WaveFormat WaveFormat => _source.WaveFormat;
    
    public override bool CanSeek => _source.CanSeek;
}
```

#### Integration into EffectsChannel

**Modify `PlayEffect()` method:**

```csharp
// Load audio file
ISampleSource waveSource;
if (filePath.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
{
    try
    {
        waveSource = new MediaFoundationDecoder(filePath).ToSampleSource();
    }
    catch
    {
        waveSource = new DmoMp3Decoder(filePath).ToSampleSource();
    }
}
else
{
    waveSource = CodecFactory.Instance.GetCodec(filePath).ToSampleSource();
}

// Wrap with silence detector (NEW)
if (_silenceDetectionEnabled)
{
    var silenceDetector = new SilenceDetectorSource(
        waveSource,
        _silenceThresholdDb,  // e.g., -60dB
        _silenceDurationMs    // e.g., 100ms
    );
    
    // Log when silence detected
    silenceDetector.SilenceDetected += (s, e) =>
    {
        if (Program.DebugMode)
        {
            GameConsole.WriteLine(
                $"[EffectsChannel] Effect '{identifier}' auto-completed via silence detection",
                ConsoleColor.Green
            );
        }
    };
    
    waveSource = silenceDetector;
}

// Continue with volume, fadeout, etc.
var volumeSource = new VolumeSource(waveSource);
// ... rest of existing code
```

---

## Configuration Settings

### ApplicationSettings.cs Integration

Add to `ApplicationSettings.cs`:

```csharp
/// <summary>
/// Silence detection settings for automatic audio completion
/// </summary>
public class SilenceDetectionSettings
{
    /// <summary>
    /// Enable automatic silence detection
    /// </summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>
    /// Amplitude threshold in dB below which audio is considered silent
    /// Typical values: -60dB (very sensitive), -40dB (moderate), -20dB (only loud silence)
    /// </summary>
    public float ThresholdDb { get; set; } = -60f;
    
    /// <summary>
    /// Duration in milliseconds that silence must be sustained before auto-completion
    /// Prevents false triggers during brief pauses
    /// </summary>
    public int SilenceDurationMs { get; set; } = 100;
    
    /// <summary>
    /// Apply to music channel (looping bed music)
    /// Generally should be false since music loops
    /// </summary>
    public bool ApplyToMusic { get; set; } = false;
    
    /// <summary>
    /// Apply to effects channel (one-shot sounds)
    /// Generally should be true since effects have definite endings
    /// </summary>
    public bool ApplyToEffects { get; set; } = true;
}

// Add property to ApplicationSettings
public SilenceDetectionSettings SilenceDetection { get; set; } = new();
```

### UI Controls in OptionsDialog

Add to **DSP tab** (or create dedicated "Playback" tab):

```
┌─ Automatic Silence Detection ────────────────────────┐
│ [✓] Enable Silence Detection                         │
│                                                       │
│ Threshold: [-80] ◄──────●─────► [-20] dB            │
│            └─ -60dB (default)                        │
│                                                       │
│ Duration:  [50]  ◄──────●─────► [500] ms            │
│            └─ 100ms (default)                        │
│                                                       │
│ Apply To:  [✓] Effects   [ ] Music                   │
│                                                       │
│ ℹ️ Automatically stops audio when sustained silence  │
│    is detected, eliminating long silent tails        │
│                                                       │
│ [Test with Current Sound]  [Reset to Default]       │
└───────────────────────────────────────────────────────┘
```

---

## Implementation Phases

### Phase 1: Core Implementation (4-6 hours)

**Tasks:**
1. Create `SilenceDetectorSource.cs` class (2 hours)
   - Implement ISampleSource wrapper
   - Amplitude monitoring logic
   - Sustained silence detection
   - SilenceDetected event
   - Debug logging

2. Integrate into `EffectsChannel.cs` (1 hour)
   - Add fields for settings
   - Wrap audio source with SilenceDetectorSource
   - Wire up event handler
   - Enable/disable toggle

3. Add settings to `ApplicationSettings.cs` (1 hour)
   - SilenceDetectionSettings class
   - Default values
   - Serialization/deserialization

4. Testing (2 hours)
   - Test with various threshold values
   - Test with different silence durations
   - Verify no false triggers
   - Measure timing improvement

### Phase 2: UI Implementation (3-4 hours)

**Tasks:**
1. Add UI controls to OptionsDialog (2 hours)
   - CheckBox for enable/disable
   - TrackBar for threshold (dB)
   - TrackBar for duration (ms)
   - CheckBoxes for Music/Effects
   - Info label explaining feature

2. Wire up event handlers (1 hour)
   - Update settings on change
   - Save to ApplicationSettings
   - Apply to channels immediately

3. Add "Test" button (1 hour)
   - Play selected sound with silence detection
   - Show detection results
   - Compare with/without detection

### Phase 3: Advanced Features (2-3 hours - OPTIONAL)

**Tasks:**
1. Visual feedback in soundpack viewer
   - Show actual audio duration vs file duration
   - Display "Tail: X.Xs" for sounds with long silent tails
   - Color code sounds with significant tails

2. Batch analysis tool
   - Scan entire soundpack
   - Report sounds with long tails
   - Suggest optimal threshold values

3. Per-sound override
   - Store silence detection settings per sound key
   - Override global settings for specific sounds
   - Useful for sounds that intentionally have silence

---

## Testing Strategy

### Unit Tests

**Test 1: Detect Silence Correctly**
```csharp
[Test]
public void SilenceDetectorSource_DetectsSilence()
{
    // Create audio with 1s content + 1s silence
    var source = CreateTestAudio(contentDuration: 1.0f, silenceDuration: 1.0f);
    var detector = new SilenceDetectorSource(source, -60f, 100);
    
    bool silenceDetected = false;
    detector.SilenceDetected += (s, e) => silenceDetected = true;
    
    // Read until complete
    float[] buffer = new float[4410];
    while (detector.Read(buffer, 0, buffer.Length) > 0) { }
    
    Assert.IsTrue(silenceDetected);
}
```

**Test 2: Don't Trigger on Brief Pauses**
```csharp
[Test]
public void SilenceDetectorSource_IgnoresBriefPauses()
{
    // Create audio with brief 50ms pause in middle
    var source = CreateTestAudioWithPause(pauseDuration: 50);
    var detector = new SilenceDetectorSource(source, -60f, 100); // Requires 100ms
    
    bool silenceDetected = false;
    detector.SilenceDetected += (s, e) => silenceDetected = true;
    
    // Read entire file
    float[] buffer = new float[4410];
    while (detector.Read(buffer, 0, buffer.Length) > 0) { }
    
    Assert.IsFalse(silenceDetected); // Should NOT trigger on 50ms pause
}
```

**Test 3: Threshold Sensitivity**
```csharp
[Test]
public void SilenceDetectorSource_ThresholdWorks()
{
    // Create audio with quiet ending (-50dB)
    var source = CreateQuietAudio(-50f);
    
    // Test with -60dB threshold (should trigger)
    var detector1 = new SilenceDetectorSource(source, -60f, 100);
    Assert.IsTrue(ReadUntilSilence(detector1));
    
    // Test with -40dB threshold (should NOT trigger)
    source.Position = 0; // Reset
    var detector2 = new SilenceDetectorSource(source, -40f, 100);
    Assert.IsFalse(ReadUntilSilence(detector2));
}
```

### Integration Tests

**Test 1: Real Audio File**
- Use actual soundpack audio file with known silent tail
- Measure playback time with/without silence detection
- Verify time saved

**Test 2: Multiple Effects**
- Play multiple overlapping effects
- Verify each detects silence independently
- No interference between effects

**Test 3: Settings Persistence**
- Change settings in UI
- Restart application
- Verify settings loaded correctly

### Performance Tests

**Test 1: CPU Impact**
- Measure CPU usage with silence detection enabled/disabled
- Should be negligible (<1% difference)
- Amplitude checking is very lightweight

**Test 2: Latency**
- Ensure no added latency to audio playback
- Silence detection happens during Read(), no buffering delays

---

## Expected Benefits

### Time Savings
**Example soundpack analysis:**
- 50 sound effects in pack
- Average file length: 4.5 seconds
- Average actual audio: 3.2 seconds
- Average tail: 1.3 seconds

**Without silence detection:**
- Total silent time wasted per full playthrough: 65 seconds
- User experiences noticeable gaps

**With silence detection:**
- Silent tails eliminated automatically
- Audio feels snappier and more responsive
- Professional timing without manual editing

### User Experience Improvements
- ✅ Faster audio response
- ✅ No "dead air" between cues
- ✅ Professional broadcast feel
- ✅ Less operator frustration
- ✅ No need to manually edit audio files

### Technical Advantages
- ✅ Works with any audio format (MP3, WAV, etc.)
- ✅ Non-destructive (doesn't modify files)
- ✅ Configurable per user preference
- ✅ Low CPU overhead
- ✅ Real-time detection

---

## Configuration Recommendations

### Preset Configurations

**Conservative (Safe)**
- Threshold: -60dB
- Duration: 150ms
- Apply to: Effects only
- Use case: Ensure no premature stops

**Balanced (Recommended)**
- Threshold: -60dB
- Duration: 100ms
- Apply to: Effects only
- Use case: General use, good balance

**Aggressive (Fast)**
- Threshold: -50dB
- Duration: 50ms
- Apply to: Effects only
- Use case: Maximum responsiveness

**Disabled**
- Enabled: False
- Use case: Audio files professionally edited, no tails

---

## Edge Cases & Considerations

### Fade-Out Detection
**Problem**: Audio with slow fadeouts might trigger too early  
**Solution**: Don't apply to sounds with `FadeOutMs > 0` specified, or lower threshold for faded sounds

### Very Quiet Sounds
**Problem**: Ambient sounds naturally quiet, might trigger false positive  
**Solution**: Per-sound override or global "minimum audible threshold" setting

### Looping Music
**Problem**: Bed music should NOT use silence detection (it loops)  
**Solution**: `ApplyToMusic = false` by default, only for Effects channel

### Corrupted/Malformed Files
**Problem**: File might have intermittent silence  
**Solution**: Reset counter on any audio detected, only sustained silence triggers

---

## Success Criteria

### Must Have (MVP)
- ✅ SilenceDetectorSource class working
- ✅ Integration with EffectsChannel
- ✅ Configurable threshold (dB)
- ✅ Configurable duration (ms)
- ✅ Enable/disable toggle
- ✅ Settings persist
- ✅ No false triggers during testing
- ✅ Measurable time savings

### Should Have
- ✅ UI controls in OptionsDialog
- ✅ Test button to preview detection
- ✅ Apply to Effects only (not Music)
- ✅ Debug logging for troubleshooting

### Nice to Have (Future)
- ⏳ Visual analysis in soundpack viewer
- ⏳ Batch analysis tool
- ⏳ Per-sound overrides
- ⏳ Preset configurations
- ⏳ Statistics (total time saved)

---

## Timeline Estimate

**Phase 1: Core Implementation** - 4-6 hours
- SilenceDetectorSource class: 2 hours
- EffectsChannel integration: 1 hour
- ApplicationSettings: 1 hour
- Testing: 2 hours

**Phase 2: UI Implementation** - 3-4 hours
- UI controls: 2 hours
- Event handlers: 1 hour
- Test button: 1 hour

**Phase 3: Advanced Features** (OPTIONAL) - 2-3 hours
- Visual feedback: 1 hour
- Batch analysis: 1 hour
- Per-sound overrides: 1 hour

**TOTAL ESTIMATE: 7-10 hours** (1-1.5 working days for MVP)

---

## Integration with DSP Plan

### Relationship to DSP Implementation

**Option 1: Part of DSP Tab**
- Add silence detection controls to DSP tab
- Conceptually related (audio processing)
- Keep all audio enhancements in one place

**Option 2: Separate "Playback" Tab**
- Silence detection is playback behavior, not DSP effect
- DSP = effects applied to audio (EQ, comp, etc.)
- Playback = how audio is triggered/stopped
- Clearer separation of concerns

**RECOMMENDATION: Option 1** - Add to DSP tab for now, easy to move later if needed

### Combined Architecture

```
ISampleSource (decoded audio)
    ↓
SilenceDetectorSource (NEW - stop on silence)
    ↓
DSPProcessor (EQ, Compressor, Limiter)
    ↓
VolumeSource
    ↓
FadeOutSource
    ↓
Mixer
```

**Order matters:**
1. **Silence detection FIRST**: Detect silence in original audio
2. **DSP effects SECOND**: Don't let DSP noise trigger detection
3. **Volume/Fade THIRD**: Cosmetic adjustments

---

## Next Steps

1. **Get Approval**: Confirm feature desired and priority
2. **Implement MVP**: Phase 1 core implementation
3. **Test Thoroughly**: Verify no false triggers
4. **Add UI**: Phase 2 controls in OptionsDialog
5. **Document**: Update user guide with silence detection explanation
6. **Release**: Ship with DSP feature or separately

---

## Questions to Resolve

1. **Q**: Should this be part of DSP tab or separate Playback tab?
   - **A**: TBD - Recommend DSP tab for now

2. **Q**: Default threshold value?
   - **A**: TBD - Recommend -60dB (very sensitive but safe)

3. **Q**: Apply to all sounds by default or opt-in?
   - **A**: TBD - Recommend enabled by default for Effects, disabled for Music

4. **Q**: Should we analyze files on load and warn about long tails?
   - **A**: TBD - Nice to have but not critical for MVP

5. **Q**: Integration with existing fadeout system?
   - **A**: TBD - Silence detection should happen before fadeout applied

---

## References

- [Audio Silence Detection Algorithms](https://en.wikipedia.org/wiki/Voice_activity_detection)
- [dB to Amplitude Conversion](https://www.sengpielaudio.com/calculator-db.htm)
- [CSCore ISampleSource Documentation](https://github.com/filoe/cscore)

---

**Proposal Status**: ✅ COMPLETE - Ready for approval and implementation  
**Estimated Effort**: 7-10 hours for MVP  
**Priority**: HIGH - Solves real user pain point  
**Next Action**: Get user approval, then implement Phase 1
