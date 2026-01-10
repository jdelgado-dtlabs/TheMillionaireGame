# Yamaha TF Rack Mixer Control Integration Plan

**Status**: 📋 Planning  
**Priority**: Medium - Feature Enhancement  
**Target Release**: v1.1.0  
**Created**: January 10, 2026  
**Last Updated**: January 10, 2026

---

## Executive Summary

This document outlines the plan to integrate Yamaha TF rack mixer control capabilities into The Millionaire Game application. This feature will allow users to control audio inputs (microphones, other sources) on their Yamaha TF rack system directly from the game interface, providing a unified control surface for professional audio production during game shows.

The integration leverages Yamaha's Remote Control Protocol (RCP), a TCP-based command protocol used across Yamaha's professional mixing console lineup (TF, CL, QL, Rivage PM, DM3 series).

---

## Problem Statement

Users running live game shows with professional audio setups often use Yamaha TF rack systems for audio mixing. Currently, they must:
- Switch between the game application and separate mixing software (TF-Rack Controller, Yamaha TF StageMix, etc.)
- Manage multiple control surfaces simultaneously
- Lack integration between game events and mixer control

**Goal**: Provide integrated mixer control within the game application for users with Yamaha TF rack systems, allowing them to adjust mic levels, mute/unmute channels, and control other inputs without leaving the application.

---

## Technical Research

### Yamaha Remote Control Protocol (RCP)

**Key Findings:**
- **Protocol**: TCP-based text protocol on port **49280** (default)
- **Message Format**: Newline-delimited text commands
- **Supported Devices**: TF series (TF1, TF3, TF5, TF-RACK), CL/QL series, Rivage PM, DM3, DM7
- **Connection Model**: Multiple simultaneous TCP connections supported (typical limit: 8 clients)
- **Open Source Resources**:
  - [yamaha-rcp-docs](https://github.com/BrenekH/yamaha-rcp-docs) - Unofficial protocol documentation
  - [yamaha-rcp-rs](https://github.com/BrenekH/yamaha-rcp-rs) - Rust implementation (reference)
  - [companion-module-yamaha-rcp](https://github.com/bitfocus/companion-module-yamaha-rcp) - Bitfocus Companion module

### Protocol Basics

#### Command Structure
```
Client → Mixer:
  get <parameter_path> <channel> <index>
  set <parameter_path> <channel> <index> <value>
  ssrecall_ex <scene_list> <scene_number>

Mixer → Client:
  OK <command_echo> <return_value>
  OKm <command_echo> <return_value>  (modified value)
  NOTIFY <parameter_path> <channel> <index> <new_value>
  ERROR <error_description>
```

#### Data Types
- **Faders/Levels**: Integer dB × 100 (e.g., `1000` = 10.00 dB, `-4000` = -40.00 dB)
- **Range**: -13800 to 1000 (-138.00 dB to +10.00 dB)
- **Negative Infinity**: -32768
- **Boolean**: 0 (false) or 1 (true)
- **Strings**: Double-quoted (e.g., `"Channel 1"`)

### Key Parameters for Input Channel Control

#### Channel Identification
- **Input Channels**: 1-40 on TF-RACK (varies by model)
- **Zero-indexed**: Channel 1 = index 0

#### Essential Parameters

| Parameter Path | Type | Range | Description |
|---------------|------|-------|-------------|
| `MIXER:Current/InCh/Fader/Level` | int | -32768 to 1000 | Channel fader level (dB × 100) |
| `MIXER:Current/InCh/Fader/On` | bool | 0-1 | Fader on/off (1 = unmuted, 0 = muted) |
| `MIXER:Current/InCh/Label/Name` | string | 64 chars | Channel name/label |
| `MIXER:Current/InCh/Label/Color` | string | enum | Channel color (Purple, Pink, Red, Orange, Yellow, Blue, SkyBlue, Green) |
| `MIXER:Current/InCh/ToMix/Level` | int | -32768 to 1000 | Send level to mix bus |
| `MIXER:Current/InCh/ToMix/On` | bool | 0-1 | Send on/off to mix bus |
| `MIXER:Current/InCh/ToMix/Pan` | int | -63 to 63 | Pan position |

#### Example Commands
```
# Get channel 1 fader level
get MIXER:Current/InCh/Fader/Level 0 0
→ OK get MIXER:Current/InCh/Fader/Level 0 0 -1000

# Set channel 1 to -10.00 dB
set MIXER:Current/InCh/Fader/Level 0 0 -1000
→ OK set MIXER:Current/InCh/Fader/Level 0 0 -1000

# Mute channel 1
set MIXER:Current/InCh/Fader/On 0 0 0
→ OK set MIXER:Current/InCh/Fader/On 0 0 0

# Get channel 1 label
get MIXER:Current/InCh/Label/Name 0 0
→ OK get MIXER:Current/InCh/Label/Name 0 0 "Main Mic"
```

### Alternative Protocol: OSC

**Note**: Yamaha also supports Open Sound Control (OSC) on some models:
- **Supported Models**: Rivage PM series, DM3 series (confirmed)
- **TF Series**: OSC support **NOT officially documented** for TF/TF-RACK
- **Recommendation**: Use RCP for TF series compatibility

---

## Architecture Design

### Component Structure

```
MillionaireGame/
├── Services/
│   └── Audio/
│       ├── YamahaRcpClient.cs          # Core RCP TCP client
│       ├── YamahaRcpService.cs         # High-level mixer service
│       └── Models/
│           ├── RcpCommand.cs           # Command/response models
│           ├── ChannelState.cs         # Channel state tracking
│           └── MixerCapabilities.cs    # Device capability detection
├── Forms/
│   └── YamahaMixerControlForm.cs       # UI form for mixer control
└── Settings/
    └── YamahaRcpSettings.cs            # Settings management

MillionaireGame.Core/
└── Services/
    └── Audio/
        └── IYamahaRcpService.cs        # Service interface
```

### Class Responsibilities

#### `YamahaRcpClient` (Low-Level Protocol)
- TCP connection management
- Command serialization/deserialization
- Response parsing
- Error handling
- Asynchronous message handling
- NOTIFY message subscription

#### `YamahaRcpService` (Business Logic)
- Connection lifecycle (connect/disconnect)
- Channel state management
- Command queueing and throttling
- Capability detection (device model identification)
- Event notifications (channel updates)
- Settings persistence integration

#### `YamahaMixerControlForm` (UI)
- Channel faders (volume sliders)
- Mute buttons
- Channel labels
- Connection status indicator
- Device discovery/selection
- Real-time level meters (if feasible)

---

## Implementation Plan

### Phase 1: Core Protocol Implementation (Week 1)

#### 1.1 RCP Client Foundation
**Files**: `YamahaRcpClient.cs`, `RcpCommand.cs`

**Tasks**:
- [ ] Create `YamahaRcpClient` class with TCP connection management
  - Async `ConnectAsync(string host, int port)` method
  - `DisconnectAsync()` method
  - Connection timeout handling (3 seconds)
- [ ] Implement command send/receive pipeline
  - `SendCommandAsync(RcpCommand)` method
  - Response parsing (`OK`, `OKm`, `ERROR` detection)
  - Message framing (newline delimiter)
- [ ] Implement NOTIFY message handling
  - Background listener task
  - Event-based notification system
- [ ] Error handling and reconnection logic
  - Automatic reconnect on connection loss
  - Exponential backoff strategy
- [ ] Logging integration with `GameConsole`
  - Debug: Raw commands/responses
  - Info: Connection events
  - Error: Protocol errors

**Test Cases**:
- Connect to TF-RACK and verify OK response
- Send `get` command and parse response
- Handle connection timeout
- Verify NOTIFY message reception

#### 1.2 Data Models
**Files**: `RcpCommand.cs`, `ChannelState.cs`, `MixerCapabilities.cs`

**Tasks**:
- [ ] Define `RcpCommand` class
  - Properties: `CommandType` (Get/Set/Recall), `ParameterPath`, `Channel`, `Index`, `Value`
  - Methods: `ToString()` for serialization
- [ ] Define `RcpResponse` class
  - Properties: `IsSuccess`, `ResponseType`, `Value`, `ErrorMessage`
  - Static factory methods: `FromRawResponse(string)`
- [ ] Define `ChannelState` class
  - Properties: `ChannelNumber`, `FaderLevel`, `IsMuted`, `Label`, `Color`, `LastUpdated`
- [ ] Define `MixerCapabilities` class
  - Properties: `ModelName`, `NumInputChannels`, `SupportedParameters`

**Test Cases**:
- Serialize/deserialize RCP commands
- Parse various response formats
- Handle malformed responses gracefully

---

### Phase 2: Service Layer (Week 2)

#### 2.1 YamahaRcpService Implementation
**Files**: `YamahaRcpService.cs`, `IYamahaRcpService.cs`

**Tasks**:
- [ ] Implement `IYamahaRcpService` interface
  - `ConnectAsync(string host, int port)`: Task<bool>
  - `DisconnectAsync()`: Task
  - `GetChannelLevelAsync(int channel)`: Task<int>
  - `SetChannelLevelAsync(int channel, int level)`: Task<bool>
  - `GetChannelMutedAsync(int channel)`: Task<bool>
  - `SetChannelMutedAsync(int channel, bool muted)`: Task<bool>
  - `GetChannelLabelAsync(int channel)`: Task<string>
  - `SetChannelLabelAsync(int channel, string label)`: Task<bool>
  - `GetAllChannelStatesAsync()`: Task<List<ChannelState>>
  - Events: `ConnectionStatusChanged`, `ChannelStateChanged`
- [ ] Implement channel state caching
  - In-memory dictionary: `Dictionary<int, ChannelState>`
  - Automatic refresh on NOTIFY messages
- [ ] Implement command rate limiting
  - Throttle to max 50 commands/second (TF series limit)
  - Queue overflow protection
- [ ] Device capability detection
  - Query mixer model on connection
  - Populate `MixerCapabilities` object
- [ ] Integration with existing `GameConsole` logging

**Test Cases**:
- Connect and verify capability detection
- Set/get channel levels and verify state cache
- Verify rate limiting under high load
- Test event notifications

#### 2.2 Settings Integration
**Files**: `YamahaRcpSettings.cs` (MillionaireGame.Core), Settings UI updates

**Tasks**:
- [ ] Add to `Settings.cs` (Core)
  ```csharp
  public class YamahaRcpSettings
  {
      public bool Enabled { get; set; } = false;
      public string MixerIpAddress { get; set; } = "192.168.0.128";
      public int MixerPort { get; set; } = 49280;
      public bool AutoConnect { get; set; } = false;
      public List<int> ControlledChannels { get; set; } = new(); // Empty = all
  }
  ```
- [ ] Update `SettingsManager` to persist Yamaha RCP settings
- [ ] Create settings UI tab: `Sounds > Yamaha RCP`
  - Enable/disable checkbox
  - IP address text field
  - Port number field (default 49280)
  - "Test Connection" button
  - "Auto-connect on startup" checkbox
  - Channel selection (multi-select list or "All channels")
- [ ] Implement immediate effect on settings save
  - Connect if `Enabled` changed from false → true
  - Disconnect if `Enabled` changed from true → false
  - Reconnect if IP/port changed while enabled

**Test Cases**:
- Save settings and verify persistence
- Toggle enable and verify connection state
- Change IP address and verify reconnection

---

### Phase 3: User Interface (Week 3)

#### 3.1 Main Menu Integration
**Files**: `MainForm.cs` (Game menu)

**Tasks**:
- [ ] Add menu item: **Game → Yamaha Mixer Control**
  - Initially disabled (grayed out)
  - Enable when `YamahaRcpSettings.Enabled == true` AND connected
- [ ] Bind menu item to open `YamahaMixerControlForm`
- [ ] Add status indicator in main form status bar (optional)
  - "Yamaha TF: Connected" / "Yamaha TF: Disconnected"

#### 3.2 Mixer Control Form
**Files**: `YamahaMixerControlForm.cs`, `YamahaMixerControlForm.Designer.cs`

**UI Components**:
- **Connection Panel** (Top)
  - Connection status label with icon (green = connected, red = disconnected)
  - Mixer model/IP display
  - Disconnect button
- **Channel Strip Panel** (Main Area - Scrollable)
  - For each controlled channel:
    - Channel number label
    - Channel name label (editable on double-click)
    - Vertical fader (TrackBar or custom control)
      - Range: -138.00 dB to +10.00 dB
      - Display current value label
    - Mute button (toggle)
    - Peak indicator (optional - visual only, no metering data from RCP)
  - Layout: Horizontal strips (e.g., 8 channels visible, scrollable)
- **Master Section** (Bottom)
  - "Refresh All" button
  - "Close" button

**Tasks**:
- [ ] Design form layout in Visual Studio Designer
- [ ] Implement channel strip factory method
  - `CreateChannelStrip(int channelNumber)` → Panel
- [ ] Bind UI controls to service methods
  - Fader `ValueChanged` → `SetChannelLevelAsync`
  - Mute button `Click` → `SetChannelMutedAsync`
  - Label `DoubleClick` → Edit dialog → `SetChannelLabelAsync`
- [ ] Implement real-time updates
  - Subscribe to `YamahaRcpService.ChannelStateChanged`
  - Update UI controls on notification (use `Invoke` for thread safety)
- [ ] Implement connection status monitoring
  - Subscribe to `YamahaRcpService.ConnectionStatusChanged`
  - Update connection panel
  - Disable controls when disconnected
- [ ] Handle form close gracefully
  - Don't disconnect on form close (connection persists)

**Test Cases**:
- Open form and verify channel strips load
- Move fader and verify level changes on mixer
- Toggle mute and verify mixer state
- Verify UI updates on external mixer changes (via NOTIFY)
- Test with 1, 8, 16, 40 channels

#### 3.3 Error Handling & User Feedback
**Tasks**:
- [ ] Display error messages for:
  - Connection failures
  - Command timeouts
  - Protocol errors
- [ ] Use non-blocking notifications (status bar, inline messages)
  - **NO MessageBox dialogs** (per project standards)
- [ ] Implement retry logic with user notification
  - "Connection lost, retrying in 5 seconds..."

---

### Phase 4: Testing & Refinement (Week 4)

#### 4.1 Unit Tests
**Files**: `MillionaireGame.Tests/Services/YamahaRcpClientTests.cs`

**Tasks**:
- [ ] Mock TCP server for testing
- [ ] Test command serialization/deserialization
- [ ] Test error response handling
- [ ] Test reconnection logic
- [ ] Test rate limiting

#### 4.2 Integration Tests
**Tasks**:
- [ ] Test with actual Yamaha TF-RACK (if available)
- [ ] Test with Yamaha TF series console
- [ ] Test concurrent control (game app + TF Editor simultaneously)
- [ ] Test high-frequency updates (stress test)
- [ ] Test network interruption recovery

#### 4.3 Performance Optimization
**Tasks**:
- [ ] Profile TCP connection overhead
- [ ] Optimize state cache invalidation
- [ ] Implement command batching (if needed)
- [ ] Optimize UI updates (throttle refresh rate)

#### 4.4 Documentation
**Files**: 
- `docs/guides/YAMAHA_MIXER_INTEGRATION_GUIDE.md` (User guide)
- `docs/reference/YAMAHA_RCP_API.md` (Developer reference)

**Tasks**:
- [ ] Write user setup guide
  - Network configuration
  - Finding mixer IP address
  - Enabling feature in settings
- [ ] Document supported models and capabilities
- [ ] Document troubleshooting steps
- [ ] API documentation for developers

---

## Configuration & Settings

### Settings Structure (JSON)
```json
{
  "YamahaRcp": {
    "Enabled": false,
    "MixerIpAddress": "192.168.0.128",
    "MixerPort": 49280,
    "AutoConnect": false,
    "ControlledChannels": [], // Empty = all channels
    "ConnectionTimeoutMs": 3000,
    "CommandTimeoutMs": 1000,
    "ReconnectDelayMs": 5000
  }
}
```

### Settings UI Location
**Path**: Settings → **Sounds** tab → **Yamaha RPC** subtab

**Controls**:
1. **Enable Yamaha Mixer Control** (Checkbox)
   - Default: Unchecked
   - Tooltip: "Enable control of Yamaha TF rack mixer from within the application"
2. **Mixer IP Address** (TextBox)
   - Default: "192.168.0.128"
   - Validation: Valid IPv4 address format
3. **Mixer Port** (NumericUpDown)
   - Default: 49280
   - Range: 1-65535
4. **Auto-connect on startup** (Checkbox)
   - Default: Unchecked
5. **Test Connection** (Button)
   - Opens modal dialog showing connection attempt result
6. **Controlled Channels** (CheckedListBox or "Select All")
   - Lists available channels (1-40)
   - Default: All selected

### Immediate Effect Implementation
```csharp
// In SettingsForm.cs
private void btnSaveYamahaSettings_Click(object sender, EventArgs e)
{
    var previousEnabled = Settings.Instance.YamahaRcp.Enabled;
    var previousIp = Settings.Instance.YamahaRcp.MixerIpAddress;
    var previousPort = Settings.Instance.YamahaRcp.MixerPort;
    
    // Save new settings
    Settings.Instance.YamahaRcp.Enabled = chkYamahaEnabled.Checked;
    Settings.Instance.YamahaRcp.MixerIpAddress = txtYamahaMixerIp.Text;
    Settings.Instance.YamahaRcp.MixerPort = (int)numYamahaMixerPort.Value;
    Settings.Instance.Save();
    
    // Apply changes immediately
    if (Settings.Instance.YamahaRcp.Enabled && !previousEnabled)
    {
        // Just enabled - connect
        _ = YamahaRcpService.Instance.ConnectAsync(
            Settings.Instance.YamahaRcp.MixerIpAddress,
            Settings.Instance.YamahaRcp.MixerPort
        );
    }
    else if (!Settings.Instance.YamahaRcp.Enabled && previousEnabled)
    {
        // Just disabled - disconnect
        _ = YamahaRcpService.Instance.DisconnectAsync();
    }
    else if (Settings.Instance.YamahaRcp.Enabled && 
             (previousIp != Settings.Instance.YamahaRcp.MixerIpAddress ||
              previousPort != Settings.Instance.YamahaRcp.MixerPort))
    {
        // Settings changed while enabled - reconnect
        _ = Task.Run(async () =>
        {
            await YamahaRcpService.Instance.DisconnectAsync();
            await YamahaRcpService.Instance.ConnectAsync(
                Settings.Instance.YamahaRcp.MixerIpAddress,
                Settings.Instance.YamahaRcp.MixerPort
            );
        });
    }
    
    // Update main menu item enabled state
    MainForm.Instance?.UpdateYamahaMixerMenuState();
}
```

---

## Game Menu Integration

### Menu Structure
```
Game
├── New Game
├── Load Game
├── Save Game
├── ───────────
├── Yamaha Mixer Control  ← NEW
├── ───────────
└── Exit
```

### Menu Item Logic
```csharp
// In MainForm.cs
public void UpdateYamahaMixerMenuState()
{
    bool shouldEnable = Settings.Instance.YamahaRcp.Enabled && 
                       YamahaRcpService.Instance.IsConnected;
    
    menuGameYamahaMixer.Enabled = shouldEnable;
}

private void menuGameYamahaMixer_Click(object sender, EventArgs e)
{
    if (yamahaMixerControlForm == null || yamahaMixerControlForm.IsDisposed)
    {
        yamahaMixerControlForm = new YamahaMixerControlForm();
    }
    
    yamahaMixerControlForm.Show();
    yamahaMixerControlForm.BringToFront();
}
```

### Connection Status Monitoring
```csharp
// In MainForm constructor
YamahaRcpService.Instance.ConnectionStatusChanged += (sender, isConnected) =>
{
    this.Invoke((MethodInvoker)(() => UpdateYamahaMixerMenuState()));
};
```

---

## Security & Network Considerations

### Network Configuration
- **Default Port**: 49280 (TCP)
- **Firewall**: May require exception for outbound TCP connections
- **Network Latency**: Protocol is synchronous; UI should handle delays gracefully
- **No Authentication**: RCP protocol has no built-in authentication (secure network required)

### Connection Management
- **Max Concurrent Connections**: 8 clients (TF series limit)
  - Game app uses 1 connection
  - Leaves 7 for other control surfaces
- **Connection Pooling**: Use single persistent connection (no pooling needed)
- **Timeout Handling**:
  - Connection timeout: 3 seconds
  - Command timeout: 1 second
  - Reconnect delay: 5 seconds (exponential backoff)

### Error Scenarios
| Scenario | Handling |
|----------|----------|
| Mixer not found | Disable menu item, log warning |
| Connection lost mid-session | Auto-reconnect with user notification |
| Invalid IP address | Show validation error in settings |
| Command timeout | Retry once, then log error |
| Protocol error | Log error, show user notification |

---

## Dependencies

### NuGet Packages
None required - uses built-in .NET TCP/IP classes:
- `System.Net.Sockets.TcpClient`
- `System.Net.Sockets.NetworkStream`
- `System.Threading.Tasks`

### External Resources
- Yamaha TF Series firmware (ensure updated to latest version)
- Network connectivity between PC and mixer

---

## Testing Strategy

### Test Environments
1. **Development**: Mock TCP server for offline testing
2. **Staging**: Yamaha TF-RACK or TF console (if available)
3. **Production**: User environments with various TF models

### Test Cases

#### Connection Tests
- [ ] Connect to valid IP address
- [ ] Connect to invalid IP address (timeout)
- [ ] Connect with incorrect port
- [ ] Disconnect gracefully
- [ ] Reconnect after connection loss
- [ ] Concurrent connections (game app + TF Editor)

#### Command Tests
- [ ] Get channel fader level
- [ ] Set channel fader level (valid range)
- [ ] Set channel fader level (out of range - should clamp)
- [ ] Get/set channel mute state
- [ ] Get/set channel label
- [ ] Rapid command sequence (rate limiting)

#### UI Tests
- [ ] Form opens with correct channel count
- [ ] Fader movement updates mixer
- [ ] Mute button toggles correctly
- [ ] UI updates on external mixer changes (NOTIFY)
- [ ] Form persists connection on close
- [ ] Menu item enabled/disabled based on connection

#### Error Handling Tests
- [ ] Network interruption during command
- [ ] Protocol error response
- [ ] Timeout handling
- [ ] Invalid command response

---

## Future Enhancements (Post-v1.1)

### Phase 2 Features
- [ ] Scene recall integration
  - Trigger mixer scene changes from game events
  - Example: Recall "Question Mode" scene when question starts
- [ ] Advanced channel routing
  - Control aux sends
  - Manage matrix outputs
- [ ] Level metering (if protocol supports)
  - Real-time VU meters on UI
- [ ] Mixer snapshot backup/restore
  - Save mixer state to file
  - Restore on connection
- [ ] Automation recording
  - Record fader movements during show
  - Playback for rehearsal

### Other Yamaha Models
- [ ] CL/QL series support
- [ ] Rivage PM series support
- [ ] DM3 series support (with OSC option)

### OSC Protocol Support
- [ ] Dual protocol support (RCP + OSC)
- [ ] Auto-detection of supported protocol

---

## Open Questions

1. **Q**: Should the mixer control form be modal or modeless?
   **A**: Modeless - allows simultaneous game control and mixer adjustments

2. **Q**: How many channels should be displayed by default?
   **A**: All enabled channels (from settings), scrollable interface

3. **Q**: Should disconnection on settings disable be immediate or graceful?
   **A**: Immediate with notification - no data loss risk

4. **Q**: Should we support mixer state persistence?
   **A**: Phase 2 - not critical for MVP

5. **Q**: How to handle mixer firmware version differences?
   **A**: Detect capabilities on connect, disable unsupported features

---

## Success Criteria

### Minimum Viable Product (MVP)
- [x] RCP protocol implemented with TF series support
- [ ] Connect/disconnect to mixer via settings
- [ ] Control 8-40 input channels (fader level, mute)
- [ ] Real-time UI updates from mixer changes
- [ ] Menu integration with enable/disable logic
- [ ] Settings persistence and immediate effect
- [ ] Non-blocking error notifications
- [ ] Comprehensive logging

### Quality Gates
- [ ] No crashes or exceptions during normal operation
- [ ] Connection recovery from network interruptions
- [ ] UI remains responsive during mixer operations
- [ ] Settings changes apply immediately
- [ ] Menu item reflects correct enabled state
- [ ] Compatible with TF1, TF3, TF5, TF-RACK

---

## Documentation Requirements

### User Documentation
1. **Setup Guide** (`docs/guides/YAMAHA_MIXER_SETUP.md`)
   - Network configuration
   - Finding mixer IP address
   - Enabling feature in settings
   - Troubleshooting connection issues

2. **User Manual Update** (Wiki: `User-Guide.md`)
   - New section: "Yamaha Mixer Control"
   - Screenshots of mixer control form
   - Common use cases

### Developer Documentation
1. **API Reference** (`docs/reference/YAMAHA_RCP_API.md`)
   - Protocol specification
   - Service interface documentation
   - Code examples

2. **Architecture Document** (`docs/reference/YAMAHA_RCP_ARCHITECTURE.md`)
   - Component diagram
   - Sequence diagrams
   - Class relationships

---

## Timeline & Milestones

### Week 1: Core Protocol (Jan 13-17, 2026)
- Day 1-2: RCP client implementation
- Day 3-4: Data models and response parsing
- Day 5: Testing and debugging

### Week 2: Service Layer (Jan 20-24, 2026)
- Day 1-2: Service implementation
- Day 3: Settings integration
- Day 4-5: Testing and refinement

### Week 3: User Interface (Jan 27-31, 2026)
- Day 1-2: Main menu integration and settings UI
- Day 3-4: Mixer control form implementation
- Day 5: UI testing and polish

### Week 4: Testing & Release (Feb 3-7, 2026)
- Day 1-2: Integration testing with hardware
- Day 3: Bug fixes and optimization
- Day 4: Documentation
- Day 5: Release preparation (v1.1.0)

---

## Risk Assessment

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Protocol changes in firmware updates | Low | High | Version detection, graceful degradation |
| Network latency affecting responsiveness | Medium | Medium | Async operations, timeout handling, UI feedback |
| Hardware unavailability for testing | Medium | High | Mock server for development, community testing |
| Concurrent control conflicts (multiple clients) | Low | Low | Document best practices, state sync |
| Memory leaks from long-running connections | Low | Medium | Proper disposal, connection monitoring |

---

## References

### External Resources
- [yamaha-rcp-docs](https://github.com/BrenekH/yamaha-rcp-docs) - Protocol documentation
- [yamaha-rcp-rs](https://github.com/BrenekH/yamaha-rcp-rs) - Reference implementation (Rust)
- [Bitfocus Companion - Yamaha RCP Module](https://github.com/bitfocus/companion-module-yamaha-rcp)
- [Yamaha TF Series Manual](https://usa.yamaha.com/products/proaudio/mixers/tf/index.html)

### Yamaha Official Documentation
- [MTX/MRX/XMV/EXi8/EXo8 RCP Spec V4.0.0](https://usa.yamaha.com/files/download/other_assets/5/1343735/200330_mtx_mrx_xmv_ex_rcps_v400_rev14_en.pdf)
- [RIVAGE PM OSC Specifications V1.0.2](https://uk.yamaha.com/files/download/other_assets/5/1407565/RIVAGE_PM_osc_specs_v102_en.pdf)
- [DM3 Series OSC Specifications V1.0.0](https://fr.yamaha.com/files/download/other_assets/2/2063222/DM3_osc_specs_v100_en.pdf)

---

## Approval & Sign-Off

**Plan Author**: GitHub Copilot  
**Date**: January 10, 2026  
**Status**: 📋 Awaiting Review

**Next Steps**:
1. Review and approve plan
2. Create GitHub project board for tracking
3. Begin Phase 1 implementation
4. Update `docs/INDEX.md` to reference this plan

---

**Last Updated**: January 10, 2026
