using System.Management;
using MillionaireGame.Utilities;

namespace MillionaireGame.Services;

/// <summary>
/// Provides safe, async monitor detection with timeout protection and graceful fallback.
/// Designed to prevent UI freezes and system crashes from WMI queries.
/// </summary>
public class MonitorInfoService
{
    private readonly Dictionary<string, MonitorInfo> _cache = new();
    private readonly SemaphoreSlim _cacheLock = new(1, 1);
    private const int WMI_TIMEOUT_SECONDS = 2;

    /// <summary>
    /// Get information for a specific monitor asynchronously with timeout protection.
    /// </summary>
    public async Task<MonitorInfo> GetMonitorInfoAsync(Screen screen, CancellationToken cancellationToken = default)
    {
        if (screen == null)
            throw new ArgumentNullException(nameof(screen));

        string cacheKey = screen.DeviceName;

        // Check cache first
        await _cacheLock.WaitAsync(cancellationToken);
        try
        {
            if (_cache.TryGetValue(cacheKey, out var cached))
            {
                GameConsole.Debug($"[MonitorInfo] Using cached info for {screen.DeviceName}");
                return cached;
            }
        }
        finally
        {
            _cacheLock.Release();
        }

        // Query WMI with timeout protection
        var info = await QueryWmiSafeAsync(screen, cancellationToken);

        // Cache the result
        await _cacheLock.WaitAsync(cancellationToken);
        try
        {
            _cache[cacheKey] = info;
        }
        finally
        {
            _cacheLock.Release();
        }

        return info;
    }

    /// <summary>
    /// Load all connected monitors in parallel with timeout protection.
    /// Returns a mix of WMI and fallback results - never throws exceptions.
    /// </summary>
    public async Task<List<MonitorInfo>> GetAllMonitorsAsync(CancellationToken cancellationToken = default)
    {
        var screens = Screen.AllScreens;
        GameConsole.Info($"[MonitorInfo] GetAllMonitorsAsync called - Screen.AllScreens.Length = {screens.Length}");
        
        for (int i = 0; i < screens.Length; i++)
        {
            GameConsole.Debug($"[MonitorInfo]   Screen[{i}]: {screens[i].DeviceName}, Primary: {screens[i].Primary}, Bounds: {screens[i].Bounds}");
        }

        var tasks = screens.Select(screen => GetMonitorInfoAsync(screen, cancellationToken));
        var results = await Task.WhenAll(tasks);

        GameConsole.Info($"[MonitorInfo] Loaded {results.Count(r => r.IsFromWmi)} from WMI, {results.Count(r => !r.IsFromWmi)} from fallback");
        
        var list = results.ToList();
        foreach (var result in list)
        {
            GameConsole.Debug($"[MonitorInfo]   Result: Index={result.MonitorIndex}, DisplayText={result.DisplayText}, FromWMI={result.IsFromWmi}");
        }
        
        return list;
    }

    /// <summary>
    /// Clear the cache - useful when display configuration changes.
    /// </summary>
    public void ClearCache()
    {
        _cacheLock.Wait();
        try
        {
            _cache.Clear();
            GameConsole.Info("[MonitorInfo] Cache cleared");
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    /// <summary>
    /// Query WMI for monitor information with timeout and error handling.
    /// NEVER blocks UI thread - all WMI operations run in background.
    /// </summary>
    private async Task<MonitorInfo> QueryWmiSafeAsync(Screen screen, CancellationToken cancellationToken)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(WMI_TIMEOUT_SECONDS));

            // Run WMI query in background thread to avoid blocking UI
            var result = await Task.Run(() => QueryWmiInternal(screen, cts.Token), cts.Token);
            
            if (result != null)
            {
                GameConsole.Debug($"[MonitorInfo] WMI success for {screen.DeviceName}: {result.DisplayText}");
                return result;
            }
            else
            {
                GameConsole.Warn($"[MonitorInfo] WMI returned null for {screen.DeviceName}, using fallback");
                return CreateFallbackInfo(screen);
            }
        }
        catch (OperationCanceledException)
        {
            GameConsole.Warn($"[MonitorInfo] WMI query timeout ({WMI_TIMEOUT_SECONDS}s) for {screen.DeviceName}");
            return CreateFallbackInfo(screen);
        }
        catch (ManagementException ex)
        {
            GameConsole.Warn($"[MonitorInfo] WMI query failed for {screen.DeviceName}: {ex.Message}");
            return CreateFallbackInfo(screen);
        }
        catch (Exception ex)
        {
            GameConsole.Error($"[MonitorInfo] Unexpected error for {screen.DeviceName}: {ex.Message}");
            return CreateFallbackInfo(screen);
        }
    }

    /// <summary>
    /// Internal WMI query - runs in background thread.
    /// Matches monitors by extracting UID from InstanceName for proper Windows display correlation.
    /// </summary>
    private MonitorInfo? QueryWmiInternal(Screen screen, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            // Get WmiMonitorID for detailed hardware info
            var wmiScope = new ManagementScope(@"\\.\root\wmi");
            var wmiQuery = new ObjectQuery("SELECT * FROM WmiMonitorID");
            
            using var wmiSearcher = new ManagementObjectSearcher(wmiScope, wmiQuery);
            var wmiMonitors = wmiSearcher.Get().Cast<ManagementObject>().ToList();

            if (wmiMonitors.Count == 0)
            {
                GameConsole.Debug($"[MonitorInfo] WMI returned no monitors");
                return null;
            }

            // Extract UID from each monitor's InstanceName and create a sorted mapping
            // InstanceName format: DISPLAY\[Manufacturer]\[Instance]&UID[Number]_0
            // The UID number corresponds to Windows display numbering
            var monitorsByUid = new List<(int uid, ManagementObject monitor)>();
            
            foreach (var monitor in wmiMonitors)
            {
                var instanceName = monitor["InstanceName"] as string;
                if (string.IsNullOrEmpty(instanceName))
                    continue;
                    
                // Extract UID from instanceName (e.g., "UID256" from "DISPLAY\LEN9156\5&1c43350&0&UID256_0")
                var uidMatch = System.Text.RegularExpressions.Regex.Match(instanceName, @"UID(\d+)");
                if (uidMatch.Success && int.TryParse(uidMatch.Groups[1].Value, out int uid))
                {
                    monitorsByUid.Add((uid, monitor));
                    GameConsole.Debug($"[MonitorInfo] Parsed InstanceName: {instanceName} -> UID={uid}");
                }
            }
            
            // Sort by UID (this gives us Windows display order)
            monitorsByUid.Sort((a, b) => a.uid.CompareTo(b.uid));
            
            GameConsole.Debug($"[MonitorInfo] Found {monitorsByUid.Count} monitors with UIDs (sorted by UID)");

            cancellationToken.ThrowIfCancellationRequested();

            // Get screen index in Screen.AllScreens array
            int screenIndex = Array.IndexOf(Screen.AllScreens, screen);
            
            // Map screen to monitor using SORTED UID order
            // Screen.AllScreens order may not match Windows display numbers,
            // but now we have monitors sorted by UID which DOES match Windows
            ManagementObject? matchedMonitor = null;
            int displayNumber = -1;
            
            // Primary display should be the first in Windows display order (lowest UID)
            if (screen.Primary && monitorsByUid.Count > 0)
            {
                matchedMonitor = monitorsByUid[0].monitor;
                displayNumber = 1; // Primary is always "Display 1" in Windows
                GameConsole.Debug($"[MonitorInfo] Matched PRIMARY {screen.DeviceName} to UID {monitorsByUid[0].uid} (Display {displayNumber})");
            }
            else
            {
                // For non-primary screens, we need to figure out which UID corresponds to this screen
                // Use process of elimination: find which UID-sorted monitor hasn't been matched yet
                // For now, use a heuristic: match by position/resolution
                
                // Try matching by X position (screens are typically arranged left to right)
                var screensByX = Screen.AllScreens.OrderBy(s => s.Bounds.X).ToList();
                int positionIndex = screensByX.IndexOf(screen);
                
                if (positionIndex >= 0 && positionIndex < monitorsByUid.Count)
                {
                    matchedMonitor = monitorsByUid[positionIndex].monitor;
                    displayNumber = positionIndex + 1;
                    GameConsole.Debug($"[MonitorInfo] Matched {screen.DeviceName} by position (X={screen.Bounds.X}) to UID {monitorsByUid[positionIndex].uid} (Display {displayNumber})");
                }
            }
            
            if (matchedMonitor == null)
            {
                GameConsole.Warn($"[MonitorInfo] Could not match WMI monitor for {screen.DeviceName}");
                return null;
            }
            
            // Log the InstanceName for debugging
            var finalInstanceName = matchedMonitor["InstanceName"] as string;
            GameConsole.Debug($"[MonitorInfo] Final match InstanceName: {finalInstanceName}");
            
            string manufacturer = "";
            string modelName = "";

            // Get ManufacturerName
            var manufacturerData = matchedMonitor["ManufacturerName"] as ushort[];
            if (manufacturerData != null && manufacturerData.Length > 0)
            {
                manufacturer = new string(manufacturerData.Where(c => c != 0).Select(c => (char)c).ToArray()).Trim();
            }

            // Get UserFriendlyName (model)
            var modelData = matchedMonitor["UserFriendlyName"] as ushort[];
            if (modelData != null && modelData.Length > 0)
            {
                modelName = new string(modelData.Where(c => c != 0).Select(c => (char)c).ToArray()).Trim();
            }

            // If we have at least one value, create MonitorInfo
            if (!string.IsNullOrEmpty(manufacturer) || !string.IsNullOrEmpty(modelName))
            {
                // Use the calculated display number instead of array index
                string displayText = FormatDisplayText(displayNumber > 0 ? displayNumber - 1 : screenIndex, manufacturer, modelName, screen);

                GameConsole.Debug($"[MonitorInfo] Created MonitorInfo: {displayText}");

                return new MonitorInfo
                {
                    Screen = screen,
                    Manufacturer = manufacturer,
                    ModelName = modelName,
                    DisplayText = displayText,
                    IsFromWmi = true,
                    MonitorIndex = displayNumber > 0 ? displayNumber - 1 : screenIndex
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            GameConsole.Debug($"[MonitorInfo] WMI internal error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Create fallback monitor info using basic screen properties.
    /// Used when WMI fails or times out.
    /// </summary>
    private MonitorInfo CreateFallbackInfo(Screen screen)
    {
        int index = Array.IndexOf(Screen.AllScreens, screen);
        string displayName = screen.Primary ? "Primary" : $"Display {index + 1}";
        string displayText = FormatDisplayText(index, "", displayName, screen);

        return new MonitorInfo
        {
            Screen = screen,
            Manufacturer = "",
            ModelName = displayName,
            DisplayText = displayText,
            IsFromWmi = false,
            MonitorIndex = index
        };
    }

    /// <summary>
    /// Format display text consistently: "2:Dell:P2419H (1920x1080)"
    /// </summary>
    private string FormatDisplayText(int index, string manufacturer, string modelName, Screen screen)
    {
        string manufacturerPart = !string.IsNullOrEmpty(manufacturer) ? manufacturer : "";
        string modelPart = !string.IsNullOrEmpty(modelName) ? modelName : $"Display {index + 1}";
        
        return $"{index + 1}:{manufacturerPart}:{modelPart} ({screen.Bounds.Width}x{screen.Bounds.Height})";
    }
}

/// <summary>
/// Monitor information with manufacturer, model, and display details.
/// </summary>
public class MonitorInfo
{
    public Screen Screen { get; set; } = null!;
    public string Manufacturer { get; set; } = "";
    public string ModelName { get; set; } = "";
    public string DisplayText { get; set; } = "";
    public bool IsFromWmi { get; set; }
    public int MonitorIndex { get; set; }
}
