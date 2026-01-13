# ETC Ion Classic Plugin Implementation Plan

**Status**: 📋 Planning  
**Priority**: HIGH - First plugin implementation  
**Target Release**: v1.1.0  
**Created**: January 10, 2026  
**Last Updated**: January 10, 2026  
**Plugin Architecture**: See [LIGHTING_PLUGIN_ARCHITECTURE.md](LIGHTING_PLUGIN_ARCHITECTURE.md)

---

## Executive Summary

This document outlines the implementation plan for the **ETC Ion Classic plugin** - the first lighting console plugin for The Millionaire Game. This plugin implements OSC (Open Sound Control) communication with ETC Ion Classic 2025 consoles using the EOS operating system.

**Key Features:**
- OSC protocol over UDP (port 8000/8001) or TCP (port 3032)
- Bi-directional communication with feedback listener
- Dynamic cue list discovery via UDP port 8001
- Support for cues, macros, GO/STOP commands
- EOS `/eos/user/0/` prefix for safe operation

---

## ETC Ion Classic Console

### Hardware & Software

**Product**: ETC Ion Classic  
**Platform**: Dedicated hardware console (EOS family)  
**Website**: https://www.etcconnect.com/Products/Consoles/Eos-Family/Ion/  
**Purpose**: Professional lighting control for theatrical and live events

**Key Specifications:**
- EOS (Eos Operating System) family console
- Support for 2,048 control channels (expandable)
- Network-based OSC remote control
- sACN (Streaming ACN/E1.31) support for future
- Cue lists, macros, and presets
- Hardware faders and buttons

**Supported Protocols:**
1. **OSC** (Primary) - Full remote control capability
2. **sACN** (Future) - DMX over Ethernet
3. **Telnet** (Future) - Alternative text-based control

---

## ETC EOS OSC Protocol

### Connection Details

- **Protocol**: OSC (Open Sound Control) over UDP or TCP
- **Ion Receives On** (Send commands to console):
  - UDP: Port **8000** (default, recommended)
  - TCP: Port **3032** (alternative)
- **Ion Transmits On** (Feedback from console):
  - UDP: Port **8001** (CRITICAL for cue discovery)
- **IP Address**: Typically static IP configured on console (e.g., 192.168.1.100)

**CRITICAL REQUIREMENT**: All commands MUST use `/eos/user/0/` prefix to operate in "User 0" context. This prevents interference with the live programmer's command line.

### OSC Message Format

```
/eos/user/0/cue/{cue_list}/{cue_number}/fire
/eos/user/0/go/{cue_list}
/eos/user/0/macro/{macro_number}/fire
```

### Data Type Requirements

- **Cue Numbers**: 
  - String if contains decimals: `"1.5"`
  - Integer if whole: `5`
- **Fader/Levels**: Float (0.0 = 0%, 1.0 = 100%)
- **List/Index Numbers**: uint32

### Key OSC Commands

#### Firing Cues
```
/eos/user/0/cue/{list}/{cue}/fire           - Fire specific cue
/eos/user/0/go/{list}                       - GO on specified cue list
/eos/user/0/cue/{list}/fire                 - Fire current cue in list
```

#### Cue Control
```
/eos/user/0/stop/{list}                     - Stop running cue in list
/eos/user/0/back/{list}                     - Move back one cue
/eos/user/0/assert/{list}/{cue}             - Jump to cue without firing
```

#### Macros
```
/eos/user/0/macro/{macro_number}/fire       - Fire macro by number
/eos/user/0/macro/label/{label}/fire        - Fire macro by label
```

#### Submasters & Faders
```
/eos/user/0/sub/{sub_number} {level}        - Set submaster level (Float: 0.0-1.0)
/eos/user/0/fader/{fader_number} {level}    - Set fader level (Float: 0.0-1.0)
```

#### System Control
```
/eos/user/0/key/blackout                    - Toggle blackout
/eos/user/0/key/clear                       - Clear command line
/eos/user/0/cmd {text}                      - Send raw command line text
```

#### Status Queries
```
/eos/get/cue/{list}/count                   - Get number of cues in list
/eos/get/cue/{list}/{index}/*               - Get cue info at index position
/eos/get/cuelist/count                      - Get number of cue lists
```

### Feedback Messages (Port 8001)

ETC consoles send OSC feedback messages to UDP port 8001:

```
/eos/out/active/cue/{list} {cue_number} {cue_label}
/eos/out/pending/cue/{list} {cue_number} {cue_label}
/eos/out/event/cue/{list}/{cue} {fired|stopped}
/eos/out/get/cue/{list}/count {count}
/eos/out/get/cue/{list}/{index}/* [args array]
```

---

## Cue List Discovery (UDP Port 8001)

### Query Structure

To retrieve the list of cues from the console:

1. **Query cue count**: Send `/eos/get/cue/{list}/count` to port 8000
2. **Console replies**: `/eos/out/get/cue/{list}/count {count}` on port 8001
3. **Iterate through index**: For each cue (0 to count-1), send `/eos/get/cue/{list}/{index}/*`
4. **Console replies with array**:
   - `args[0]`: uint32 - Index number
   - `args[1]`: string - Unique ID (UID)
   - `args[2]`: string - **Cue Label** (display name for UI)
   - `args[3]`: string|int - **Cue Number** (identifier to trigger)
   - `args[4]`: uint32 - Cue Part number

### Example Query Flow

```
App  → Ion: /eos/get/cue/1/count
Ion  → App: /eos/out/get/cue/1/count 25

App  → Ion: /eos/get/cue/1/0/*
Ion  → App: [0, "abc123", "House Lights Up", "1", 0]

App  → Ion: /eos/get/cue/1/1/*
Ion  → App: [1, "def456", "Stage Wash Blue", "1.5", 0]

App  → Ion: /eos/get/cue/1/2/*
Ion  → App: [2, "ghi789", "Contestant Spot", "2", 0]
```

**Key Points:**
- `args[2]` = Cue Label → Used for UI display
- `args[3]` = Cue Number → Used to trigger cue
- Cache results and provide refresh mechanism in UI

---

## Plugin Architecture

### Project Structure

**Project**: `MillionaireGame.Plugins.EtcIon`  
**Output**: `lib/dlls/MillionaireGame.Plugins.EtcIon.dll`  
**Target**: .NET 8.0  
**Dependencies**: `MillionaireGame.Core` (for ILightingPlugin interface)

### Class Structure

```
MillionaireGame.Plugins.EtcIon/
├── EtcIonPlugin.cs              - Main plugin class (implements ILightingPlugin)
├── EtcOscClient.cs              - OSC UDP/TCP client with feedback listener
├── EtcCommandBuilder.cs         - EOS OSC command construction
├── OscMessage.cs                - OSC message encoding/decoding
└── EtcIonPlugin.csproj          - Project file
```

---

## Implementation Details

### 1. EtcIonPlugin.cs

**Purpose**: Main plugin class implementing `ILightingPlugin` interface.

```csharp
public class EtcIonPlugin : ILightingPlugin
{
    private readonly EtcOscClient _oscClient;
    private LightingConnectionSettings? _settings;
    private bool _isConnected;
    
    public LightingPluginMetadata Metadata => new()
    {
        PluginType = "LightingControl",
        SystemName = "ETC Ion Classic",
        DisplayName = "ETC Ion Classic 2025",
        Manufacturer = "Electronic Theatre Controls (ETC)",
        Version = "1.0.0",
        Author = "Millionaire Game Team",
        Description = "Plugin for ETC Ion Classic lighting console using OSC protocol",
        SupportedProtocols = new List<string> { "OSC", "sACN" },
        IconResourcePath = "pack://application:,,,/Resources/etc_icon.png"
    };
    
    public LightingSystemCapabilities GetCapabilities() => new()
    {
        SupportsCueLists = true,
        SupportsMacros = true,
        SupportsSubmasters = true,
        SupportsBlackout = true,
        SupportsDirectDMX = false,  // Future: sACN
        SupportsBidirectionalCommunication = true,
        MaxCueLists = 99,
        MaxCuesPerList = 9999
    };
    
    public LightingPluginConfigUI GetConfigurationUI() => new()
    {
        ConnectionGroupTitle = "ETC Ion Connection",
        CueMappingsGroupTitle = "Cue List Mappings",
        SupportsCueListRefresh = true,
        SupportedCommandTypes = new List<string> { "FireCue", "FireMacro", "Go", "Stop", "Custom" },
        ConnectionSettings = new List<ConfigSettingDefinition>
        {
            new() { 
                Key = "HostAddress", 
                Label = "Host Address:", 
                Type = ConfigSettingType.TextBox, 
                DefaultValue = "192.168.1.100", 
                Tooltip = "IP address of ETC Ion console" 
            },
            new() { 
                Key = "SendPort", 
                Label = "Send Port (to console):", 
                Type = ConfigSettingType.NumericUpDown,
                DefaultValue = 8000, 
                ValidationRule = new { Min = 1024, Max = 65535 }, 
                Tooltip = "Port console receives commands on (UDP: 8000, TCP: 3032)" 
            },
            new() { 
                Key = "ReceivePort", 
                Label = "Receive Port (feedback):", 
                Type = ConfigSettingType.NumericUpDown,
                DefaultValue = 8001, 
                ValidationRule = new { Min = 1024, Max = 65535 },
                Tooltip = "Port console sends feedback on (default: 8001)" 
            },
            new() { 
                Key = "TransportProtocol", 
                Label = "Transport:", 
                Type = ConfigSettingType.RadioButtonGroup,
                DefaultValue = "UDP", 
                OptionValues = new List<string> { "UDP", "TCP" },
                Tooltip = "Communication protocol (UDP recommended)" 
            },
            new() { 
                Key = "AutoConnect", 
                Label = "Auto-connect on startup", 
                Type = ConfigSettingType.CheckBox,
                DefaultValue = false, 
                Tooltip = "Automatically connect when application starts" 
            }
        }
    };
    
    // Connection Management
    public async Task<bool> ConnectAsync(LightingConnectionSettings settings)
    {
        _settings = settings;
        _oscClient = new EtcOscClient(settings.HostAddress, settings.SendPort, settings.ReceivePort);
        
        var connected = await _oscClient.ConnectAsync();
        if (connected)
        {
            _isConnected = true;
            ConnectionStatusChanged?.Invoke(this, new ConnectionStatusChangedEventArgs 
            { 
                IsConnected = true, 
                Message = $"Connected to ETC Ion at {settings.HostAddress}:{settings.SendPort}" 
            });
        }
        return connected;
    }
    
    public void Disconnect()
    {
        _oscClient?.Disconnect();
        _isConnected = false;
        ConnectionStatusChanged?.Invoke(this, new ConnectionStatusChangedEventArgs 
        { 
            IsConnected = false, 
            Message = "Disconnected from ETC Ion" 
        });
    }
    
    // Command Execution
    public async Task<LightingCommandResult> TriggerCueAsync(string cueIdentifier)
    {
        if (!_isConnected || _settings == null)
            return new LightingCommandResult { Success = false, Message = "Not connected" };
        
        var command = EtcCommandBuilder.BuildFireCue("1", cueIdentifier);  // Default cue list 1
        await _oscClient.SendCommandAsync(command);
        
        return new LightingCommandResult 
        { 
            Success = true, 
            CommandSent = command.Address,
            Message = $"Fired cue {cueIdentifier}" 
        };
    }
    
    public async Task<LightingCommandResult> TriggerMacroAsync(string macroIdentifier)
    {
        if (!_isConnected)
            return new LightingCommandResult { Success = false, Message = "Not connected" };
        
        var command = EtcCommandBuilder.BuildFireMacro(macroIdentifier);
        await _oscClient.SendCommandAsync(command);
        
        return new LightingCommandResult 
        { 
            Success = true, 
            CommandSent = command.Address,
            Message = $"Fired macro {macroIdentifier}" 
        };
    }
    
    public async Task<LightingCommandResult> GoAsync(string? cueListId = null)
    {
        if (!_isConnected)
            return new LightingCommandResult { Success = false, Message = "Not connected" };
        
        var command = EtcCommandBuilder.BuildGo(cueListId ?? "1");
        await _oscClient.SendCommandAsync(command);
        
        return new LightingCommandResult 
        { 
            Success = true, 
            CommandSent = command.Address,
            Message = $"GO on cue list {cueListId ?? "1"}" 
        };
    }
    
    public async Task<LightingCommandResult> StopAsync(string? cueListId = null)
    {
        if (!_isConnected)
            return new LightingCommandResult { Success = false, Message = "Not connected" };
        
        var command = EtcCommandBuilder.BuildStop(cueListId ?? "1");
        await _oscClient.SendCommandAsync(command);
        
        return new LightingCommandResult 
        { 
            Success = true, 
            CommandSent = command.Address,
            Message = $"STOP on cue list {cueListId ?? "1"}" 
        };
    }
    
    public async Task<LightingCommandResult> SendCustomCommandAsync(string command)
    {
        if (!_isConnected)
            return new LightingCommandResult { Success = false, Message = "Not connected" };
        
        var oscCommand = EtcCommandBuilder.BuildCustomCommand(command);
        await _oscClient.SendCommandAsync(oscCommand);
        
        return new LightingCommandResult 
        { 
            Success = true, 
            CommandSent = oscCommand.Address,
            Message = "Custom command sent" 
        };
    }
    
    // Cue List Discovery
    public async Task<List<LightingCueInfo>> GetCueListAsync(string cueListId)
    {
        if (!_isConnected)
            return new List<LightingCueInfo>();
        
        var cueList = new List<LightingCueInfo>();
        
        // Step 1: Query cue count
        var countQuery = EtcCommandBuilder.BuildGetCueCount(cueListId);
        await _oscClient.SendCommandAsync(countQuery);
        
        // Step 2: Wait for count response on port 8001
        var countResponse = await WaitForResponseAsync($"/eos/out/get/cue/{cueListId}/count", 2000);
        if (countResponse == null || countResponse.Arguments.Count == 0)
            return cueList;
        
        int count = Convert.ToInt32(countResponse.Arguments[0]);
        
        // Step 3: Iterate through each cue
        for (int i = 0; i < count; i++)
        {
            var cueQuery = EtcCommandBuilder.BuildGetCueAtIndex(cueListId, i);
            await _oscClient.SendCommandAsync(cueQuery);
            
            // Wait for cue info response on port 8001
            var cueResponse = await WaitForResponseAsync($"/eos/out/get/cue/{cueListId}/{i}", 1000);
            if (cueResponse == null || cueResponse.Arguments.Count < 5)
                continue;
            
            // Parse arguments: [index, uid, label, cueNumber, part]
            var cueInfo = new LightingCueInfo
            {
                Index = Convert.ToInt32(cueResponse.Arguments[0]),
                UniqueId = cueResponse.Arguments[1].ToString() ?? "",
                Label = cueResponse.Arguments[2].ToString() ?? "",       // args[2] = Display name
                CueNumber = cueResponse.Arguments[3].ToString() ?? "",   // args[3] = Trigger identifier
                PartNumber = Convert.ToInt32(cueResponse.Arguments[4])
            };
            
            cueList.Add(cueInfo);
        }
        
        return cueList;
    }
    
    public async Task<int> GetCueCountAsync(string cueListId)
    {
        if (!_isConnected)
            return 0;
        
        var countQuery = EtcCommandBuilder.BuildGetCueCount(cueListId);
        await _oscClient.SendCommandAsync(countQuery);
        
        var countResponse = await WaitForResponseAsync($"/eos/out/get/cue/{cueListId}/count", 2000);
        if (countResponse == null || countResponse.Arguments.Count == 0)
            return 0;
        
        return Convert.ToInt32(countResponse.Arguments[0]);
    }
    
    // Helper method for waiting for OSC responses
    private async Task<OscMessage?> WaitForResponseAsync(string addressPrefix, int timeoutMs)
    {
        var tcs = new TaskCompletionSource<OscMessage>();
        
        void OnMessageReceived(object? sender, OscMessageReceivedEventArgs e)
        {
            if (e.Address.StartsWith(addressPrefix))
            {
                tcs.TrySetResult(new OscMessage { Address = e.Address, Arguments = e.Arguments });
            }
        }
        
        _oscClient.MessageReceived += OnMessageReceived;
        
        try
        {
            var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(timeoutMs));
            return completedTask == tcs.Task ? await tcs.Task : null;
        }
        catch (Exception ex)
        {
            GameConsole.Error($"[ETC Ion] Error waiting for response: {ex.Message}");
            return null;
        }
        finally
        {
            _oscClient.MessageReceived -= OnMessageReceived;
        }
    }
    
    // Validation
    public bool ValidateSettings(LightingConnectionSettings settings, out string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(settings.HostAddress))
        {
            errorMessage = "Host address is required";
            return false;
        }
        
        if (settings.SendPort < 1024 || settings.SendPort > 65535)
        {
            errorMessage = "Send port must be between 1024 and 65535";
            return false;
        }
        
        if (settings.ReceivePort < 1024 || settings.ReceivePort > 65535)
        {
            errorMessage = "Receive port must be between 1024 and 65535";
            return false;
        }
        
        errorMessage = string.Empty;
        return true;
    }
    
    public void Dispose()
    {
        _oscClient?.Dispose();
    }
}
```

---

### 2. EtcOscClient.cs

**Purpose**: OSC UDP/TCP client with background listener on port 8001.

```csharp
internal class EtcOscClient : IDisposable
{
    private UdpClient? _sendClient;              // Send commands to Ion (port 8000/3032)
    private UdpClient? _feedbackClient;          // Listen for feedback from Ion (port 8001)
    private readonly string _hostAddress;
    private readonly int _txPort;                // Default: 8000 (UDP) or 3032 (TCP)
    private readonly int _rxPort;                // Default: 8001 (UDP feedback)
    private CancellationTokenSource? _listenerCancellation;
    private Task? _listenerTask;
    
    public event EventHandler<OscMessageReceivedEventArgs>? MessageReceived;
    
    public EtcOscClient(string hostAddress, int txPort, int rxPort)
    {
        _hostAddress = hostAddress;
        _txPort = txPort;
        _rxPort = rxPort;
    }
    
    public async Task<bool> ConnectAsync()
    {
        try
        {
            // Create send client
            _sendClient = new UdpClient();
            _sendClient.Connect(_hostAddress, _txPort);
            
            // Create feedback listener on port 8001
            _feedbackClient = new UdpClient(_rxPort);
            _listenerCancellation = new CancellationTokenSource();
            _listenerTask = ListenForMessagesAsync(_listenerCancellation.Token);
            
            GameConsole.Info($"[ETC Ion] Connected to {_hostAddress}:{_txPort}, listening on {_rxPort}");
            return true;
        }
        catch (Exception ex)
        {
            GameConsole.Error($"[ETC Ion] Connection failed: {ex.Message}");
            return false;
        }
    }
    
    public void Disconnect()
    {
        _listenerCancellation?.Cancel();
        _listenerTask?.Wait(1000);
        
        _sendClient?.Close();
        _feedbackClient?.Close();
        
        _sendClient = null;
        _feedbackClient = null;
        
        GameConsole.Info("[ETC Ion] Disconnected");
    }
    
    public async Task SendCommandAsync(OscMessage message)
    {
        if (_sendClient == null)
            throw new InvalidOperationException("Not connected");
        
        var bytes = message.ToBytes();
        await _sendClient.SendAsync(bytes, bytes.Length);
        
        GameConsole.Debug($"[ETC Ion] Sent: {message.Address}");
    }
    
    // Background listener for UDP port 8001
    private async Task ListenForMessagesAsync(CancellationToken cancellationToken)
    {
        if (_feedbackClient == null)
            return;
        
        GameConsole.Info("[ETC Ion] Started UDP listener on port " + _rxPort);
        
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = await _feedbackClient.ReceiveAsync();
                ProcessOscMessage(result.Buffer);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                GameConsole.Error($"[ETC Ion] Listener error: {ex.Message}");
            }
        }
    }
    
    private void ProcessOscMessage(byte[] data)
    {
        try
        {
            var message = OscMessage.FromBytes(data);
            MessageReceived?.Invoke(this, new OscMessageReceivedEventArgs
            {
                Address = message.Address,
                Arguments = message.Arguments,
                ReceivedAt = DateTime.Now
            });
            
            GameConsole.Debug($"[ETC Ion] Received: {message.Address}");
        }
        catch (Exception ex)
        {
            GameConsole.Error($"[ETC Ion] Failed to parse OSC message: {ex.Message}");
        }
    }
    
    public void Dispose()
    {
        Disconnect();
        _listenerCancellation?.Dispose();
    }
}

public class OscMessageReceivedEventArgs : EventArgs
{
    public string Address { get; set; } = "";
    public List<object> Arguments { get; set; } = new();
    public DateTime ReceivedAt { get; set; }
}
```

---

### 3. EtcCommandBuilder.cs

**Purpose**: Build EOS OSC commands with `/eos/user/0/` prefix.

```csharp
internal static class EtcCommandBuilder
{
    private const string USER_PREFIX = "/eos/user/0";
    
    public static OscMessage BuildFireCue(string cueList, string cueNumber)
    {
        // Check if cue number is decimal (needs string) or whole (can be int)
        var isDecimal = cueNumber.Contains(".");
        var address = $"{USER_PREFIX}/cue/{cueList}/{cueNumber}/fire";
        
        return new OscMessage { Address = address, Arguments = new List<object>() };
    }
    
    public static OscMessage BuildGo(string cueList)
    {
        return new OscMessage 
        { 
            Address = $"{USER_PREFIX}/go/{cueList}", 
            Arguments = new List<object>() 
        };
    }
    
    public static OscMessage BuildStop(string cueList)
    {
        return new OscMessage 
        { 
            Address = $"{USER_PREFIX}/stop/{cueList}", 
            Arguments = new List<object>() 
        };
    }
    
    public static OscMessage BuildFireMacro(string macroNumber)
    {
        return new OscMessage 
        { 
            Address = $"{USER_PREFIX}/macro/{macroNumber}/fire", 
            Arguments = new List<object>() 
        };
    }
    
    public static OscMessage BuildBlackout()
    {
        return new OscMessage 
        { 
            Address = $"{USER_PREFIX}/key/blackout", 
            Arguments = new List<object>() 
        };
    }
    
    public static OscMessage BuildGetCueCount(string cueList)
    {
        return new OscMessage 
        { 
            Address = $"/eos/get/cue/{cueList}/count", 
            Arguments = new List<object>() 
        };
    }
    
    public static OscMessage BuildGetCueAtIndex(string cueList, int index)
    {
        return new OscMessage 
        { 
            Address = $"/eos/get/cue/{cueList}/{index}/*", 
            Arguments = new List<object>() 
        };
    }
    
    public static OscMessage BuildCustomCommand(string command)
    {
        // If command doesn't start with /eos/user/0, add prefix
        if (!command.StartsWith("/eos/"))
            command = $"{USER_PREFIX}{command}";
        
        return new OscMessage 
        { 
            Address = command, 
            Arguments = new List<object>() 
        };
    }
}
```

---

### 4. OscMessage.cs

**Purpose**: OSC message encoding/decoding (binary format).

```csharp
public class OscMessage
{
    public string Address { get; set; } = "";
    public List<object> Arguments { get; set; } = new();
    
    public byte[] ToBytes()
    {
        using var ms = new MemoryStream();
        
        // Write address (null-terminated, 4-byte aligned)
        var addressBytes = Encoding.ASCII.GetBytes(Address);
        ms.Write(addressBytes, 0, addressBytes.Length);
        ms.WriteByte(0); // Null terminator
        AlignTo4Bytes(ms);
        
        // Write type tags
        var typeTags = BuildTypeTags();
        var typeTagBytes = Encoding.ASCII.GetBytes(typeTags);
        ms.Write(typeTagBytes, 0, typeTagBytes.Length);
        ms.WriteByte(0); // Null terminator
        AlignTo4Bytes(ms);
        
        // Write arguments
        foreach (var arg in Arguments)
        {
            WriteArgument(ms, arg);
        }
        
        return ms.ToArray();
    }
    
    public static OscMessage FromBytes(byte[] data)
    {
        var message = new OscMessage();
        int index = 0;
        
        // Read address
        message.Address = ReadString(data, ref index);
        
        // Read type tags
        var typeTags = ReadString(data, ref index);
        if (typeTags.StartsWith(","))
            typeTags = typeTags.Substring(1);
        
        // Read arguments
        foreach (char tag in typeTags)
        {
            message.Arguments.Add(ReadArgument(data, ref index, tag));
        }
        
        return message;
    }
    
    private string BuildTypeTags()
    {
        if (Arguments.Count == 0)
            return ",";
        
        var tags = ",";
        foreach (var arg in Arguments)
        {
            tags += arg switch
            {
                int => "i",
                float => "f",
                string => "s",
                bool => arg.Equals(true) ? "T" : "F",
                _ => throw new NotSupportedException($"Type {arg.GetType()} not supported")
            };
        }
        return tags;
    }
    
    private void WriteArgument(MemoryStream ms, object arg)
    {
        switch (arg)
        {
            case int i:
                ms.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(i)), 0, 4);
                break;
            case float f:
                var floatBytes = BitConverter.GetBytes(f);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(floatBytes);
                ms.Write(floatBytes, 0, 4);
                break;
            case string s:
                var stringBytes = Encoding.ASCII.GetBytes(s);
                ms.Write(stringBytes, 0, stringBytes.Length);
                ms.WriteByte(0);
                AlignTo4Bytes(ms);
                break;
            case bool:
                // Boolean encoded in type tag, no data
                break;
        }
    }
    
    private static object ReadArgument(byte[] data, ref int index, char tag)
    {
        return tag switch
        {
            'i' => IPAddress.NetworkToHostOrder(BitConverter.ToInt32(data, index)),
            'f' => ReadFloat(data, ref index),
            's' => ReadString(data, ref index),
            'T' => true,
            'F' => false,
            _ => throw new NotSupportedException($"Type tag '{tag}' not supported")
        };
    }
    
    private static string ReadString(byte[] data, ref int index)
    {
        int start = index;
        while (index < data.Length && data[index] != 0)
            index++;
        
        var str = Encoding.ASCII.GetString(data, start, index - start);
        
        // Advance past null and align to 4 bytes
        index++;
        while (index % 4 != 0)
            index++;
        
        return str;
    }
    
    private static float ReadFloat(byte[] data, ref int index)
    {
        var floatBytes = new byte[4];
        Array.Copy(data, index, floatBytes, 0, 4);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(floatBytes);
        index += 4;
        return BitConverter.ToSingle(floatBytes, 0);
    }
    
    private static void AlignTo4Bytes(MemoryStream ms)
    {
        while (ms.Length % 4 != 0)
            ms.WriteByte(0);
    }
}
```

---

## Testing Strategy

### Unit Tests
- OSC message encoding/decoding
- EOS command builder correctness
- Settings validation

### Integration Tests
1. **Mock OSC Server**
   - Respond to `/eos/get/cue/1/count` with test count
   - Respond to `/eos/get/cue/1/{index}/*` with test cue data
   - Verify cue list parsing

2. **Actual ETC Ion Console**
   - Connect to real console
   - Fire test cues
   - Verify feedback listener receives messages
   - Test cue list discovery

### Manual Testing Checklist
- [ ] Plugin appears in settings dropdown
- [ ] Configuration UI generates correctly
- [ ] Test connection succeeds with real console
- [ ] Cue list refresh populates from console
- [ ] Trigger cue from game event fires on console
- [ ] UDP port 8001 listener receives feedback
- [ ] GO/STOP commands work
- [ ] Macro commands work
- [ ] Connection status updates correctly

---

## Known Limitations

1. **sACN not implemented** - Future enhancement
2. **TCP transport not implemented** - UDP only for v1.0
3. **Single cue list** - Defaults to cue list 1
4. **No heartbeat/keepalive** - Assumes stable connection
5. **No command queue** - Commands sent immediately

---

## Future Enhancements

1. **TCP Support** - Alternative to UDP on port 3032
2. **sACN Protocol** - Direct DMX control without console
3. **Multi-Cue-List Support** - User selects cue list per mapping
4. **Heartbeat** - Keep connection alive with periodic `/thump`
5. **Command Queue** - Batch multiple commands
6. **Bi-Directional Sync** - Subscribe to console updates
7. **Submaster Control** - Expose fader control in UI

---

## References

- [ETC EOS OSC Documentation](https://www.etcconnect.com/Support/Articles/OSC.aspx)
- [OSC 1.1 Specification](https://opensoundcontrol.stanford.edu/spec-1_1.html)
- [ETC Ion Classic Product Page](https://www.etcconnect.com/Products/Consoles/Eos-Family/Ion/)
