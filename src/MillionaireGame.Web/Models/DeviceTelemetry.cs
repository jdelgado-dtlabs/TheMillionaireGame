namespace MillionaireGame.Web.Models;

/// <summary>
/// Device telemetry data for anonymous statistics (non-identifying)
/// </summary>
public class DeviceTelemetry
{
    public string? DeviceType { get; set; }      // e.g., "Mobile", "Desktop", "Tablet"
    public string? OSType { get; set; }           // e.g., "iOS", "Android", "Windows", "macOS"
    public string? OSVersion { get; set; }        // e.g., "17.1", "14", "11"
    public string? BrowserType { get; set; }      // e.g., "Chrome", "Safari", "Edge"
    public string? BrowserVersion { get; set; }   // e.g., "120.0", "17.2"
    public bool HasAgreedToPrivacy { get; set; } = false;
}
