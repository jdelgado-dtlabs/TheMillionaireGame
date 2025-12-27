# Audio Settings UI Implementation Plan

**Date Created:** December 27, 2025  
**Status:** 🎯 READY TO IMPLEMENT  
**Priority:** HIGH  
**Estimated Time:** 6-8 hours  
**Dependencies:** Audio System (Phase 1-2) - ✅ COMPLETE

---

## Overview

With the audio processing system fully implemented (silence detection, crossfading, queue management), the next phase is creating UI components in the OptionsDialog to allow operators to configure these audio features.

### Current Implementation State ✅
- ✅ AudioCueQueue with silence detection (703 lines)
- ✅ SilenceDetectorSource wrapper (155 lines)
- ✅ EffectsChannel with queue integration (528 lines)
- ✅ SoundService public API (616 lines)
- ✅ ApplicationSettings with audio configuration classes
- ✅ DSPTestDialog for testing (395 lines)

### What Needs UI 🎯
- Silence Detection settings (ThresholdDb, SilenceDurationMs, InitialDelayMs, FadeoutDurationMs)
- Crossfade settings (DurationMs, Enabled checkbox)
- Audio Processing settings (MasterGainDb, EffectsGainDb, MusicGainDb, EnableLimiter)
- Real-time queue monitoring panel (optional enhancement)

---

## Architecture Design

### Target: OptionsDialog
**File:** `src/MillionaireGame/Forms/OptionsDialog.cs`  
**Current State:** Existing settings dialog with multiple tabs

### New Components Needed

```
OptionsDialog
├── (Existing tabs...)
└── Audio Settings Tab ← NEW
    ├── Silence Detection GroupBox
    │   ├── ThresholdDb TrackBar (-60dB to -20dB, default: -40dB)
    │   ├── SilenceDurationMs NumericUpDown (100-1000ms, default: 250ms)
    │   ├── InitialDelayMs NumericUpDown (0-5000ms, default: 2500ms)
    │   ├── FadeoutDurationMs NumericUpDown (10-200ms, default: 50ms)
    │   └── Enable Silence Detection CheckBox
    │
    ├── Crossfade Settings GroupBox
    │   ├── CrossfadeDurationMs NumericUpDown (10-200ms, default: 50ms)
    │   └── Enable Crossfade CheckBox
    │
    ├── Audio Processing GroupBox
    │   ├── MasterGainDb TrackBar (-20dB to +20dB, default: 0dB)
    │   ├── EffectsGainDb TrackBar (-20dB to +20dB, default: 0dB)
    │   ├── MusicGainDb TrackBar (-20dB to +20dB, default: 0dB)
    │   └── Enable Limiter CheckBox
    │
    └── Preview/Test Panel (Optional)
        ├── Test Sound ComboBox
        ├── Test Button
        └── Queue Status Label
```

---

## Implementation Plan

### Phase 1: Create Audio Settings Tab (1-2 hours)

#### Step 1.1: Add Tab to OptionsDialog.Designer.cs
```csharp
private TabPage tabPageAudio;

// In InitializeComponent():
this.tabPageAudio = new System.Windows.Forms.TabPage();
this.tabPageAudio.Text = "Audio";
this.tabPageAudio.Name = "tabPageAudio";
this.tabControl1.Controls.Add(this.tabPageAudio);
```

#### Step 1.2: Create Base Layout
- Add AutoScroll = true to tab page
- Use FlowLayoutPanel or TableLayoutPanel for organized layout
- Set proper padding and margins

---

### Phase 2: Silence Detection UI (1.5-2 hours)

#### Controls Needed:

1. **Threshold TrackBar + Label**
   ```csharp
   private TrackBar trackBarSilenceThreshold;
   private Label labelSilenceThresholdValue;
   
   // Properties:
   Minimum = -60
   Maximum = -20
   TickFrequency = 5
   Value = -40 (default from settings)
   LargeChange = 5
   SmallChange = 1
   
   // Display format: "-40 dB"
   ```

2. **Silence Duration NumericUpDown**
   ```csharp
   private NumericUpDown numericUpDownSilenceDuration;
   
   // Properties:
   Minimum = 100
   Maximum = 1000
   Increment = 50
   Value = 250 (default)
   
   // Label: "Silence Duration (ms)"
   ```

3. **Initial Delay NumericUpDown**
   ```csharp
   private NumericUpDown numericUpDownInitialDelay;
   
   // Properties:
   Minimum = 0
   Maximum = 5000
   Increment = 100
   Value = 2500 (default)
   
   // Label: "Initial Delay (ms)"
   // Tooltip: "Delay before silence detection starts (prevents fade-in cutoff)"
   ```

4. **Fadeout Duration NumericUpDown**
   ```csharp
   private NumericUpDown numericUpDownFadeoutDuration;
   
   // Properties:
   Minimum = 10
   Maximum = 200
   Increment = 10
   Value = 50 (default)
   
   // Label: "Fadeout Duration (ms)"
   ```

5. **Enable CheckBox**
   ```csharp
   private CheckBox checkBoxEnableSilenceDetection;
   
   // Properties:
   Text = "Enable Silence Detection"
   Checked = true (default)
   ```

#### Event Handlers:
```csharp
private void trackBarSilenceThreshold_ValueChanged(object sender, EventArgs e)
{
    labelSilenceThresholdValue.Text = $"{trackBarSilenceThreshold.Value} dB";
    // Update settings preview
}

private void checkBoxEnableSilenceDetection_CheckedChanged(object sender, EventArgs e)
{
    // Enable/disable related controls
    bool enabled = checkBoxEnableSilenceDetection.Checked;
    trackBarSilenceThreshold.Enabled = enabled;
    numericUpDownSilenceDuration.Enabled = enabled;
    numericUpDownInitialDelay.Enabled = enabled;
    numericUpDownFadeoutDuration.Enabled = enabled;
}
```

---

### Phase 3: Crossfade Settings UI (0.5-1 hour)

#### Controls Needed:

1. **Crossfade Duration NumericUpDown**
   ```csharp
   private NumericUpDown numericUpDownCrossfadeDuration;
   
   // Properties:
   Minimum = 10
   Maximum = 200
   Increment = 10
   Value = 50 (default)
   
   // Label: "Crossfade Duration (ms)"
   ```

2. **Enable Crossfade CheckBox**
   ```csharp
   private CheckBox checkBoxEnableCrossfade;
   
   // Properties:
   Text = "Enable Crossfade"
   Checked = true (default)
   ```

#### Event Handlers:
```csharp
private void checkBoxEnableCrossfade_CheckedChanged(object sender, EventArgs e)
{
    numericUpDownCrossfadeDuration.Enabled = checkBoxEnableCrossfade.Checked;
}
```

---

### Phase 4: Audio Processing UI (1-1.5 hours)

#### Controls Needed:

1. **Master Gain TrackBar + Label**
   ```csharp
   private TrackBar trackBarMasterGain;
   private Label labelMasterGainValue;
   
   // Properties:
   Minimum = -20
   Maximum = 20
   TickFrequency = 5
   Value = 0 (default)
   LargeChange = 5
   SmallChange = 1
   
   // Display format: "0 dB" or "+5 dB" or "-10 dB"
   ```

2. **Effects Gain TrackBar + Label**
   ```csharp
   private TrackBar trackBarEffectsGain;
   private Label labelEffectsGainValue;
   
   // Same properties as Master Gain
   ```

3. **Music Gain TrackBar + Label**
   ```csharp
   private TrackBar trackBarMusicGain;
   private Label labelMusicGainValue;
   
   // Same properties as Master Gain
   ```

4. **Enable Limiter CheckBox**
   ```csharp
   private CheckBox checkBoxEnableLimiter;
   
   // Properties:
   Text = "Enable Limiter"
   Checked = true (default)
   Tooltip = "Prevents audio distortion by limiting peak levels"
   ```

#### Event Handlers:
```csharp
private void trackBarGain_ValueChanged(object sender, EventArgs e)
{
    var trackBar = (TrackBar)sender;
    var label = GetCorrespondingLabel(trackBar); // Helper method
    
    int value = trackBar.Value;
    string sign = value > 0 ? "+" : "";
    label.Text = $"{sign}{value} dB";
}
```

---

### Phase 5: Settings Integration (1-1.5 hours)

#### Load Settings on Dialog Open
```csharp
private void OptionsDialog_Load(object sender, EventArgs e)
{
    LoadAudioSettings();
}

private void LoadAudioSettings()
{
    var settings = ApplicationSettings.Instance;
    
    // Silence Detection
    trackBarSilenceThreshold.Value = (int)settings.SilenceDetection.ThresholdDb;
    numericUpDownSilenceDuration.Value = settings.SilenceDetection.SilenceDurationMs;
    numericUpDownInitialDelay.Value = settings.SilenceDetection.InitialDelayMs;
    numericUpDownFadeoutDuration.Value = settings.SilenceDetection.FadeoutDurationMs;
    checkBoxEnableSilenceDetection.Checked = settings.SilenceDetection.Enabled;
    
    // Crossfade
    numericUpDownCrossfadeDuration.Value = settings.Crossfade.DurationMs;
    checkBoxEnableCrossfade.Checked = settings.Crossfade.Enabled;
    
    // Audio Processing
    trackBarMasterGain.Value = (int)settings.AudioProcessing.MasterGainDb;
    trackBarEffectsGain.Value = (int)settings.AudioProcessing.EffectsGainDb;
    trackBarMusicGain.Value = (int)settings.AudioProcessing.MusicGainDb;
    checkBoxEnableLimiter.Checked = settings.AudioProcessing.EnableLimiter;
    
    // Update display labels
    UpdateAllGainLabels();
}
```

#### Save Settings on OK/Apply
```csharp
private void btnOK_Click(object sender, EventArgs e)
{
    SaveAudioSettings();
    // ... existing save logic
}

private void SaveAudioSettings()
{
    var settings = ApplicationSettings.Instance;
    
    // Silence Detection
    settings.SilenceDetection.ThresholdDb = trackBarSilenceThreshold.Value;
    settings.SilenceDetection.SilenceDurationMs = (int)numericUpDownSilenceDuration.Value;
    settings.SilenceDetection.InitialDelayMs = (int)numericUpDownInitialDelay.Value;
    settings.SilenceDetection.FadeoutDurationMs = (int)numericUpDownFadeoutDuration.Value;
    settings.SilenceDetection.Enabled = checkBoxEnableSilenceDetection.Checked;
    
    // Crossfade
    settings.Crossfade.DurationMs = (int)numericUpDownCrossfadeDuration.Value;
    settings.Crossfade.Enabled = checkBoxEnableCrossfade.Checked;
    
    // Audio Processing
    settings.AudioProcessing.MasterGainDb = trackBarMasterGain.Value;
    settings.AudioProcessing.EffectsGainDb = trackBarEffectsGain.Value;
    settings.AudioProcessing.MusicGainDb = trackBarMusicGain.Value;
    settings.AudioProcessing.EnableLimiter = checkBoxEnableLimiter.Checked;
    
    // Save to config.xml
    settings.Save();
    
    // Apply changes to running audio system
    ApplyAudioSettings();
}

private void ApplyAudioSettings()
{
    // Notify SoundService to reload settings
    // This may require adding a RefreshSettings() method to SoundService
    var soundService = ServiceLocator.Instance.GetService<SoundService>();
    soundService?.RefreshSettings();
}
```

---

### Phase 6: Optional - Preview/Test Panel (1-2 hours)

**NOTE:** This is optional but highly valuable for testing settings in real-time.

#### Controls Needed:

1. **Test Sound ComboBox**
   ```csharp
   private ComboBox comboBoxTestSound;
   
   // Items:
   "Lights Down"
   "Let's Play"
   "Final Answer"
   "Correct Answer"
   "Wrong Answer"
   ```

2. **Test Button**
   ```csharp
   private Button btnTestAudio;
   
   // Properties:
   Text = "Test Sound"
   ```

3. **Queue Status Label**
   ```csharp
   private Label labelQueueStatus;
   
   // Display: "Queue: Idle" or "Queue: Playing (2 sounds)"
   ```

#### Event Handlers:
```csharp
private void btnTestAudio_Click(object sender, EventArgs e)
{
    if (comboBoxTestSound.SelectedItem == null) return;
    
    string selectedSound = comboBoxTestSound.SelectedItem.ToString();
    SoundEffect effect = MapToSoundEffect(selectedSound);
    
    var soundService = ServiceLocator.Instance.GetService<SoundService>();
    soundService?.QueueSound(effect, AudioPriority.Normal);
    
    // Update status
    UpdateQueueStatus();
}

private void UpdateQueueStatus()
{
    var soundService = ServiceLocator.Instance.GetService<SoundService>();
    if (soundService?.IsQueuePlaying() == true)
    {
        int count = soundService.GetQueueCount();
        labelQueueStatus.Text = $"Queue: Playing ({count} sounds)";
    }
    else
    {
        labelQueueStatus.Text = "Queue: Idle";
    }
}
```

#### Real-time Monitoring (Optional Timer)
```csharp
private System.Windows.Forms.Timer timerQueueMonitor;

private void InitializeQueueMonitor()
{
    timerQueueMonitor = new System.Windows.Forms.Timer();
    timerQueueMonitor.Interval = 100; // 100ms updates
    timerQueueMonitor.Tick += (s, e) => UpdateQueueStatus();
}

private void tabPageAudio_Enter(object sender, EventArgs e)
{
    // Start monitoring when tab is active
    timerQueueMonitor?.Start();
}

private void tabPageAudio_Leave(object sender, EventArgs e)
{
    // Stop monitoring when tab is not active
    timerQueueMonitor?.Stop();
}
```

---

## UI Layout Mockup

```
╔══════════════════════════════════════════════════════════════╗
║ [General] [Gameplay] [Display] [Audio] [Network] [Advanced]  ║
╠══════════════════════════════════════════════════════════════╣
║                                                              ║
║  ┌─ Silence Detection ────────────────────────────────┐     ║
║  │  ☑ Enable Silence Detection                       │     ║
║  │                                                     │     ║
║  │  Threshold:     [■■■■■■■■░░░░░░░░░░]  -40 dB      │     ║
║  │                 -60                    -20          │     ║
║  │                                                     │     ║
║  │  Silence Duration (ms):  [▼ 250]                   │     ║
║  │  Initial Delay (ms):     [▼ 2500]                  │     ║
║  │  Fadeout Duration (ms):  [▼ 50]                    │     ║
║  └─────────────────────────────────────────────────────┘     ║
║                                                              ║
║  ┌─ Crossfade Settings ───────────────────────────────┐     ║
║  │  ☑ Enable Crossfade                                │     ║
║  │                                                     │     ║
║  │  Crossfade Duration (ms):  [▼ 50]                  │     ║
║  └─────────────────────────────────────────────────────┘     ║
║                                                              ║
║  ┌─ Audio Processing ─────────────────────────────────┐     ║
║  │  Master Gain:   [■■■■■■■■■■■■■░░░░░░]  0 dB       │     ║
║  │                 -20             0      +20          │     ║
║  │                                                     │     ║
║  │  Effects Gain:  [■■■■■■■■■■■■■░░░░░░]  0 dB       │     ║
║  │  Music Gain:    [■■■■■■■■■■■■■░░░░░░]  0 dB       │     ║
║  │                                                     │     ║
║  │  ☑ Enable Limiter                                  │     ║
║  └─────────────────────────────────────────────────────┘     ║
║                                                              ║
║  ┌─ Test Audio (Optional) ────────────────────────────┐     ║
║  │  Test Sound:  [Lights Down ▼]  [Test Sound]       │     ║
║  │                                                     │     ║
║  │  Queue Status: Idle                                │     ║
║  └─────────────────────────────────────────────────────┘     ║
║                                                              ║
║                              [OK] [Cancel] [Apply]          ║
╚══════════════════════════════════════════════════════════════╝
```

---

## Implementation Checklist

### Phase 1: Tab Creation ✅
- [ ] Add tabPageAudio to OptionsDialog
- [ ] Set up layout containers (GroupBox, FlowLayoutPanel)
- [ ] Test tab navigation

### Phase 2: Silence Detection UI ✅
- [ ] Add threshold TrackBar + label
- [ ] Add silence duration NumericUpDown
- [ ] Add initial delay NumericUpDown
- [ ] Add fadeout duration NumericUpDown
- [ ] Add enable CheckBox
- [ ] Wire up event handlers
- [ ] Test enable/disable functionality

### Phase 3: Crossfade UI ✅
- [ ] Add crossfade duration NumericUpDown
- [ ] Add enable CheckBox
- [ ] Wire up event handlers
- [ ] Test enable/disable functionality

### Phase 4: Audio Processing UI ✅
- [ ] Add master gain TrackBar + label
- [ ] Add effects gain TrackBar + label
- [ ] Add music gain TrackBar + label
- [ ] Add enable limiter CheckBox
- [ ] Wire up event handlers
- [ ] Test value display formatting

### Phase 5: Settings Integration ✅
- [ ] Implement LoadAudioSettings()
- [ ] Implement SaveAudioSettings()
- [ ] Implement ApplyAudioSettings()
- [ ] Add RefreshSettings() to SoundService (if needed)
- [ ] Test settings persistence (save/load from config.xml)
- [ ] Test runtime settings application

### Phase 6: Preview/Test Panel (Optional) ⏳
- [ ] Add test sound ComboBox
- [ ] Add test button
- [ ] Add queue status label
- [ ] Implement test button handler
- [ ] Implement queue monitoring timer
- [ ] Test real-time queue status updates

### Phase 7: Polish & Testing ✅
- [ ] Add tooltips to all controls
- [ ] Set proper tab order
- [ ] Test with keyboard navigation
- [ ] Test with screen reader (accessibility)
- [ ] Add input validation
- [ ] Test edge cases (min/max values)
- [ ] Test settings persistence across app restarts
- [ ] Document any limitations or known issues

---

## Testing Scenarios

1. **Load Existing Settings**
   - Open OptionsDialog
   - Verify all values match current settings
   - Close without saving
   - Reopen, verify values unchanged

2. **Save New Settings**
   - Change multiple settings
   - Click OK
   - Reopen dialog, verify values persisted

3. **Apply Settings**
   - Change settings
   - Click Apply
   - Verify audio system uses new settings immediately
   - Test with actual sound playback

4. **Enable/Disable Controls**
   - Uncheck "Enable Silence Detection"
   - Verify related controls disabled
   - Re-check, verify controls enabled

5. **Edge Cases**
   - Set threshold to -60dB (minimum)
   - Set threshold to -20dB (maximum)
   - Set durations to minimum values
   - Set durations to maximum values
   - Verify no crashes or invalid states

6. **Test Panel (Optional)**
   - Select test sound
   - Click Test Sound button
   - Verify sound plays with current settings
   - Change settings, test again
   - Verify new settings applied

---

## Potential Issues & Solutions

### Issue 1: Settings Not Applied in Real-Time
**Problem:** Changing settings requires app restart  
**Solution:** Add RefreshSettings() method to SoundService that reloads settings from ApplicationSettings

### Issue 2: TrackBar Value Display
**Problem:** dB values may not align with TrackBar tick marks  
**Solution:** Use ValueChanged event to update label, not relying on tick positions

### Issue 3: Queue Status Update Performance
**Problem:** Timer polling every 100ms may impact performance  
**Solution:** Only enable timer when Audio tab is active, disable when switched away

### Issue 4: Settings Validation
**Problem:** Invalid combinations of settings (e.g., fadeout > silence duration)  
**Solution:** Add validation in SaveAudioSettings() before applying

---

## Future Enhancements

1. **Presets System**
   - "Conservative" (high threshold, long duration)
   - "Aggressive" (low threshold, short duration)
   - "Balanced" (current defaults)
   - "Custom" (user-defined)

2. **Visual Feedback**
   - Real-time waveform display
   - Silence detection visualization
   - Crossfade preview graph

3. **Advanced Options**
   - Per-sound threshold overrides UI
   - Custom sound profiles
   - Import/export settings

4. **Diagnostics Panel**
   - Audio device information
   - Buffer size/latency stats
   - Performance metrics

---

## Time Breakdown

| Phase | Task | Estimated Time |
|-------|------|----------------|
| 1 | Tab Creation | 1-2 hours |
| 2 | Silence Detection UI | 1.5-2 hours |
| 3 | Crossfade UI | 0.5-1 hour |
| 4 | Audio Processing UI | 1-1.5 hours |
| 5 | Settings Integration | 1-1.5 hours |
| 6 | Preview/Test Panel | 1-2 hours (optional) |
| 7 | Polish & Testing | 1 hour |
| **Total** | | **6-10 hours** |

**Recommended:** Start with Phases 1-5 (core functionality), then add Phase 6 (test panel) if time permits.

---

## Success Criteria

- ✅ All audio settings configurable through UI
- ✅ Settings persist across app restarts
- ✅ Settings apply to audio system in real-time (or with Apply button)
- ✅ UI is intuitive and self-explanatory
- ✅ Input validation prevents invalid configurations
- ✅ No crashes or exceptions when changing settings
- ✅ Tooltips provide helpful context for each setting

---

## Next Steps

1. Review this plan with user for approval
2. Confirm which phases to implement (core vs. optional)
3. Begin implementation starting with Phase 1
4. Test incrementally after each phase
5. Document any deviations or discoveries during implementation
6. Create session summary when complete
