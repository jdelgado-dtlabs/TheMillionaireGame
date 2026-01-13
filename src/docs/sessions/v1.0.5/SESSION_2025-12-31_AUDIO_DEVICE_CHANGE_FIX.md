# Session: Audio Device Change Fix
**Date:** December 31, 2025  
**Status:** ✅ COMPLETE

## Problem Summary
Wireless display devices (and any device changes) were causing audio to stop working after 3-6 seconds. The device would initialize successfully and pass verification, but then stop reading audio shortly after.

## Investigation Timeline

### Initial Symptoms
- Wireless display selected as default → audio initialized but never played
- Device switching worked initially but audio died after ~5 seconds
- Happened consistently with ALL device changes (not just wireless displays)
- Even DirectSound fallback stopped working after device changes

### Root Cause Discovery
The issue was NOT with:
- ❌ Wireless display compatibility
- ❌ AudioCueQueue being removed (already fixed earlier)
- ❌ Health check interfering with playback
- ❌ Lock contention or race conditions
- ❌ WASAPI verification process

**The actual problem:** Our automatic device change reinitialization was breaking CSCore's built-in device change handling.

### Testing Process
1. Implemented automatic device change detection and reinitialization → devices stopped working after changes
2. Added duplicate prevention with Interlocked flags → still failed
3. Added 100ms delay between disposal and recreation → slightly longer but still failed
4. **Disabled automatic reinitialization entirely → devices switched perfectly and kept working**

## Solution
**Remove automatic reinitialization completely.** CSCore/WASAPI has built-in device change handling that works seamlessly when left alone.

### Changes Made

**AudioMixer.cs:**
- Removed entire automatic reinitialization logic from `OnDefaultDeviceChanged()`
- Kept the notification client for logging purposes only
- Removed `_deviceChangeInProgress` flag (no longer needed)
- Device changes now logged but not acted upon

### Code Changes
```csharp
// OLD (BROKEN):
private void OnDefaultDeviceChanged(object? sender, DefaultDeviceChangedEventArgs e)
{
    // Complex logic to dispose old device and create new one
    _systemOutput?.Stop();
    _systemOutput?.Dispose();
    _systemOutput = TryInitializeOutput(null, out _currentOutputType);
    // ... resulted in audio stopping after a few seconds
}

// NEW (WORKING):
private void OnDefaultDeviceChanged(object? sender, DefaultDeviceChangedEventArgs e)
{
    // CSCore/WASAPI handles device changes natively - no manual reinitialization needed
    if (Program.DebugMode)
    {
        GameConsole.Debug($"[AudioMixer] Default device changed to: {e.DeviceId}");
    }
}
```

## Results
✅ Device switching works perfectly (including wireless displays)  
✅ Audio continues playing after device changes  
✅ No mixer disconnections  
✅ Queue persistence maintained  
✅ Health check still monitors for genuine device failures  
✅ DirectSound fallback still available if WASAPI initialization fails

## Key Lessons
1. **Trust the library:** CSCore already handles device changes internally - don't fight it
2. **Sometimes doing nothing is the right solution**
3. **Test without your "helpful" code** - disable features to isolate problems
4. **Native audio APIs handle device changes automatically** - Windows redirects audio streams

## Related Files
- `MillionaireGame/Services/AudioMixer.cs` - Main fix location
- `MillionaireGame.Core/Services/EffectsMixerSource.cs` - Queue persistence (already fixed)

## Technical Notes
CSCore's WASAPI implementation automatically handles:
- Device disconnection/reconnection
- Default device changes
- Audio stream redirection
- Sample rate adjustments

Manual disposal and recreation interferes with this internal state management, causing the audio pipeline to break.

## Future Considerations
- Consider removing the MMNotificationClient entirely if we only use it for logging
- Health check can still detect genuine device failures (no reads + PlaybackState != Playing)
- Manual device selection via UI still requires reinitialization (different code path)
