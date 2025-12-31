# Audio Device Behavior Reference

## System Default Device Behavior

### How "System Default" Works

When you select **"System Default"** in the audio device settings:

- The application uses CSCore's `WasapiOut()` without specifying a device
- CSCore calls Windows' `GetDefaultAudioEndpoint()` to get the **current** default device
- The audio output is initialized and **locked** to that specific device

### Important Limitation: No Automatic Following

**System Default does NOT automatically follow Windows default device changes.**

Once the audio mixer is initialized:
- The application is locked to whichever device was the default at initialization time
- If you change the Windows default device in Windows Settings or Sound Panel, the application **will not automatically switch**
- The application will continue using the original device until you manually change it in the Options dialog

### To Switch Devices

To switch to a different device (including a newly set Windows default):
1. Open the Options dialog
2. Select the new device from the dropdown (or select "System Default" again to re-query Windows)
3. Click Apply - this triggers `ChangeDevice()` which reinitializes the audio output

### Future Enhancement

To implement automatic following of Windows default device changes, the application would need to:
1. Register an `IMMNotificationClient` with Windows Core Audio API
2. Listen for `OnDefaultDeviceChanged` events
3. Automatically call `ChangeDevice()` when the default changes

This is not currently implemented to avoid unexpected audio interruptions during gameplay.

---

## Wireless Display Devices

### Known Issue: Miracast/Wireless Display

When using **Miracast** or **Wireless Display** to cast to a TV:

#### Symptoms
- Device appears as "Digital Output" with empty or "( )" name in device list
- Application fails to initialize audio when this device is selected
- Audio system enters a failed state where silence generation stops
- Changing to a local device doesn't work until application restart

#### Root Cause
Wireless display audio devices can fail initialization because:
1. **Device State**: The device may be in a transitional state (connecting/disconnecting)
2. **Format Support**: The device may not support the audio format the app is trying to use (48kHz, stereo, 32-bit float)
3. **Device Properties**: Empty/missing FriendlyName causes enumeration or initialization issues
4. **Latency**: Network latency may cause initialization timeout

#### Current Mitigations (v0.9.9)

1. **Device Filtering**: 
   - `AudioDeviceManager` now skips devices with empty `FriendlyName`
   - Prevents problematic wireless devices from appearing in the device list

2. **Automatic Fallback**:
   - If device initialization fails, automatically falls back to system default
   - Logs detailed error information (exception type and message)
   - No longer requires application restart

3. **Enhanced Logging**:
   - Logs device discovery process
   - Shows when devices are skipped due to empty names
   - Detailed initialization failure messages with exception types

#### User Workaround

If you need to use audio on a wireless display:
1. **Option A**: Set Windows default audio to a local device (speakers/headphones) and select "System Default" in the app
2. **Option B**: Use OBS or streaming software to capture the local audio separately and mix it with the video cast
3. **Option C**: Use a physical HDMI cable for direct connection (eliminates wireless device issues)

#### Technical Details

CSCore's `WasapiOut` uses Windows Audio Session API (WASAPI) in shared mode, which requires:
- **Sample Rate**: 48000 Hz (the app's default)
- **Bit Depth**: 32-bit float
- **Channels**: Stereo (2)

Wireless display devices may:
- Only support 44100 Hz or 16-bit formats
- Have additional latency requirements
- Require exclusive mode for certain formats
- Dynamically change their supported formats during connection

---

## Device Enumeration

### How Devices Are Listed

The `AudioDeviceManager.GetAudioOutputDevices()` method:

1. **System Default**: Always added as first option with `DeviceId = null`
2. **Physical Devices**: Enumerates using `MMDeviceEnumerator.EnumAudioEndpoints()`
   - Only `DataFlow.Render` (output) devices
   - Only `DeviceState.Active` devices
3. **Filtering**: Skips devices with empty `FriendlyName` (as of v0.9.9)

### Device Properties

Each `AudioDeviceInfo` contains:
- **DeviceId**: Unique Windows device identifier (null for system default)
- **FriendlyName**: Human-readable name shown in UI
- **IsDefault**: Boolean indicating if this is the "System Default" entry

---

## Troubleshooting

### Audio Not Starting

**Symptoms**: No audio output, silence logging shows no activity

**Check**:
1. Open GameConsole (View → Game Console) with Debug Mode enabled
2. Look for initialization errors
3. Check if device failed to initialize and fallback occurred

**Solutions**:
1. Switch to "System Default" in Options
2. Check Windows Sound Settings - ensure default device is working
3. Try a different audio device
4. Restart the application

### Device Not Appearing in List

**Causes**:
- Device has empty FriendlyName (filtered out intentionally)
- Device is not in Active state
- Device is an input device (microphone)

**Solutions**:
1. Check Windows Sound Settings - ensure device is enabled
2. Use "System Default" and set the device as default in Windows
3. Update audio drivers

### Audio Switches When Windows Default Changes

**This is NOT a bug** - see "System Default Device Behavior" above. This is expected behavior for how WASAPI works. Once initialized, the output device is locked until manually changed.

---

## Version History

- **v0.9.9**: Added device filtering, automatic fallback, enhanced logging
- **v0.9.8**: Initial audio device selection implementation
