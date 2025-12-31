using CSCore.CoreAudioAPI;

namespace MillionaireGame.Services;

/// <summary>
/// Manages audio output device enumeration and selection
/// </summary>
public static class AudioDeviceManager
{
    /// <summary>
    /// Gets all available audio output devices
    /// </summary>
    public static List<AudioDeviceInfo> GetAudioOutputDevices()
    {
        var devices = new List<AudioDeviceInfo>();
        
        // Add System Default as first option
        devices.Add(new AudioDeviceInfo
        {
            DeviceId = null,
            FriendlyName = "System Default",
            IsDefault = true
        });

        try
        {
            using var deviceEnumerator = new MMDeviceEnumerator();
            using var deviceCollection = deviceEnumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active);

            foreach (var device in deviceCollection)
            {
                var friendlyName = device.FriendlyName;
                var deviceId = device.DeviceID;
                
                // Allow all devices now that we have DirectSound fallback for problematic ones
                // If FriendlyName is empty, use a placeholder
                if (string.IsNullOrWhiteSpace(friendlyName))
                {
                    friendlyName = $"Audio Device ({deviceId.Substring(0, Math.Min(8, deviceId.Length))}...)";
                    if (Program.DebugMode)
                    {
                        Utilities.GameConsole.Debug($"[AudioDeviceManager] Device has empty name, using: {friendlyName}");
                    }
                }
                
                devices.Add(new AudioDeviceInfo
                {
                    DeviceId = deviceId,
                    FriendlyName = friendlyName,
                    IsDefault = false
                });
                device.Dispose();
            }
        }
        catch (Exception ex)
        {
            if (Program.DebugMode)
            {
                Utilities.GameConsole.Error($"[AudioDeviceManager] Error enumerating devices: {ex.Message}");
            }
        }

        return devices;
    }

    /// <summary>
    /// Gets an MMDevice by ID, or null for system default
    /// </summary>
    public static MMDevice? GetDeviceById(string? deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return null; // Use system default
        }

        try
        {
            var deviceEnumerator = new MMDeviceEnumerator();
            return deviceEnumerator.GetDevice(deviceId);
        }
        catch (Exception ex)
        {
            if (Program.DebugMode)
            {
                Utilities.GameConsole.Warn($"[AudioDeviceManager] Could not get device {deviceId}: {ex.Message}");
            }
            return null;
        }
    }
}

/// <summary>
/// Information about an audio output device
/// </summary>
public class AudioDeviceInfo
{
    public string? DeviceId { get; set; }
    public string FriendlyName { get; set; } = string.Empty;
    public bool IsDefault { get; set; }

    public override string ToString() => FriendlyName;
}
