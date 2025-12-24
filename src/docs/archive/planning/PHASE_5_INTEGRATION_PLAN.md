# Phase 5: Main Application Integration - Implementation Plan

## Overview
Integrate the web-based audience participation system (WAPS) into the main WinForms application with full control from the Options dialog.

**Version**: 0.7.0-2512  
**Status**: 🚧 In Progress  
**Date**: December 23, 2025

---

## Requirements

### 1. Audience Settings Tab

**Location**: OptionsDialog → New "Audience" tab

**Controls**:

#### IP Address Dropdown
- **Options**:
  - `0.0.0.0` - Open to all (any network interface)
  - `127.0.0.1` - Localhost only
  - Dynamic: Detected local IP addresses (e.g., `192.168.1.100/24`)
- **Behavior**: Disabled when "Auto-start" checked or server running

#### Port Text Box
- **Default Value**: `5278` (current web server port)
- **Validation**: Must be valid port number (1-65535)
- **Check Button**: "Check if in use"
  - Red ❌ if port in use (except our own process)
  - Green ✓ if available
- **Behavior**: Disabled when "Auto-start" checked or server running

#### Auto-Start Checkbox
- **Label**: "Start server automatically on application startup"
- **Effect**: When checked:
  - Disables IP and Port controls
  - Server starts automatically when main app starts
- **Editable**: Only when server is stopped

#### Start/Stop Server Buttons
- **Start Server Button**:
  - Enabled when server stopped
  - Disabled when server running
  - Starts web server with selected IP/Port
- **Stop Server Button**:
  - Enabled when server running
  - Disabled when server stopped
  - Gracefully stops web server

#### Status Label
- **When Running**: `Server started at {ip} on port {port}`
- **When Stopped**: `Server Stopped`
- **Color**: Green when running, Gray when stopped

### 2. Application Settings (New Properties)

Add to `ApplicationSettings.cs`:

```csharp
// Web Server / Audience Participation Settings
public string AudienceServerIP { get; set; } = "127.0.0.1";
public int AudienceServerPort { get; set; } = 5278;
public bool AudienceServerAutoStart { get; set; } = false;
```

### 3. Web Server Hosting

**Implementation**: Create `WebServerHost` class in MillionaireGame project

**Features**:
- Host ASP.NET Core web server in WinForms application
- Start/Stop functionality
- Health check endpoint
- Proper disposal on application exit

**Dependencies**:
- Reference `MillionaireGame.Web` project
- Add NuGet: `Microsoft.AspNetCore.Hosting.WindowsServices` (if needed)

### 4. Network Utilities

**Create**: `NetworkHelper` class

**Functions**:
- `GetLocalIPAddresses()` - Returns list of local IPs with subnet masks
- `IsPortAvailable(int port)` - Checks if port is free
- `IsPortOwnedByProcess(int port, int processId)` - Checks ownership

---

## Implementation Steps

### Step 1: Update ApplicationSettings Model ✅
- Add 3 new properties for web server configuration
- Save/load from XML

### Step 2: Create NetworkHelper Utility ✅
- Implement IP detection
- Implement port checking

### Step 3: Create WebServerHost Class ✅
- Embed web server hosting logic
- Start/Stop/Health check methods
- Event handlers for status changes

### Step 4: Design Audience Tab UI ✅
- Create tab in OptionsDialog.Designer.cs
- Add all required controls:
  - ComboBox for IP
  - TextBox for Port
  - Button for port check
  - CheckBox for auto-start
  - Buttons for Start/Stop
  - Label for status

### Step 5: Wire Up Audience Tab Logic ✅
- Load settings
- Save settings
- IP dropdown population
- Port validation
- Start/Stop handlers
- Auto-start logic

### Step 6: Integrate with Main Application ✅
- Initialize WebServerHost in main form
- Handle auto-start on application launch
- Handle disposal on application exit
- Add error handling and logging

### Step 7: Testing ✅
- Test all IP options (0.0.0.0, localhost, local IP)
- Test port validation
- Test auto-start functionality
- Test manual start/stop
- Test persistence of settings

---

## Technical Details

### Web Server Hosting Architecture

```csharp
public class WebServerHost
{
    private IHost? _host;
    private string _baseUrl;
    
    public bool IsRunning => _host != null;
    
    public event EventHandler<string>? ServerStarted;
    public event EventHandler? ServerStopped;
    public event EventHandler<Exception>? ServerError;
    
    public async Task StartAsync(string ipAddress, int port)
    {
        _baseUrl = $"http://{ipAddress}:{port}";
        
        var builder = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>(); // From MillionaireGame.Web
                webBuilder.UseUrls(_baseUrl);
            });
        
        _host = builder.Build();
        await _host.StartAsync();
        
        ServerStarted?.Invoke(this, _baseUrl);
    }
    
    public async Task StopAsync()
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
            _host = null;
            ServerStopped?.Invoke(this, EventArgs.Empty);
        }
    }
}
```

### Port Checking Logic

```csharp
public static bool IsPortAvailable(int port)
{
    try
    {
        var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        listener.Stop();
        return true;
    }
    catch (SocketException)
    {
        return false;
    }
}
```

### IP Address Detection

```csharp
public static List<string> GetLocalIPAddresses()
{
    var addresses = new List<string>();
    
    foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
    {
        if (ni.OperationalStatus == OperationalStatus.Up)
        {
            var properties = ni.GetIPProperties();
            foreach (var ip in properties.UnicastAddresses)
            {
                if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                {
                    addresses.Add($"{ip.Address}/{ip.IPv4Mask}");
                }
            }
        }
    }
    
    return addresses;
}
```

---

## UI Layout (Audience Tab)

```
┌─────────────────────────────────────────────────────────────┐
│ Audience Participation Server                               │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  IP Address:  [Dropdown: 0.0.0.0 / Local IPs / Localhost] │
│                                                             │
│  Port:        [5278]  [Check if in use]  [✓ or ❌]        │
│                                                             │
│  ☐ Start server automatically on startup                   │
│                                                             │
│  [Start Server]  [Stop Server]                             │
│                                                             │
│  Status: Server started at 192.168.1.100 on port 5278     │
│                                                             │
├─────────────────────────────────────────────────────────────┤
│  Information:                                               │
│  • Audience members connect via web browser                 │
│  • Use QR code or URL to join sessions                     │
│  • Port must be open on firewall for external access       │
│  • Localhost restricts to this computer only               │
│  • 0.0.0.0 allows all network interfaces                   │
└─────────────────────────────────────────────────────────────┘
```

---

## Testing Checklist

- [ ] Settings persist correctly (save/load)
- [ ] IP dropdown populates with local IPs
- [ ] Localhost option works
- [ ] 0.0.0.0 option works
- [ ] Port validation catches invalid ports
- [ ] Port check detects in-use ports
- [ ] Port check allows our own process
- [ ] Auto-start checkbox behavior correct
- [ ] Controls disabled/enabled appropriately
- [ ] Start Server button works
- [ ] Stop Server button works
- [ ] Status label updates correctly
- [ ] Server starts on app launch (if auto-start enabled)
- [ ] Server stops on app exit
- [ ] Error handling for start failures
- [ ] QR code generation still works
- [ ] Sessions can be created
- [ ] Audience can join from browser

---

## Deferred to Future Phases

### Phase 6: FFF Interface
- Control panel integration for FFF
- Start/stop FFF questions
- View participants
- Monitor answers
- Select winner

### Phase 7: ATA (Ask The Audience) Interface  
- Control panel integration for ATA
- Start/stop voting
- Real-time results display
- Export results

---

## Dependencies

**NuGet Packages** (may be needed):
- `Microsoft.AspNetCore.Hosting` (likely already in .Web project)
- `Microsoft.Extensions.Hosting` (likely already available)

**Project References**:
- `MillionaireGame` → `MillionaireGame.Web` (add reference)

---

## Notes

- Web server runs in same process as WinForms app
- Graceful shutdown ensures database cleanup
- Port conflicts handled with clear error messages
- Auto-start useful for production shows
- Manual control useful for testing/development

---

**Next Steps**: Begin implementation starting with ApplicationSettings updates.
