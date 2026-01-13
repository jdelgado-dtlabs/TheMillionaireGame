# Sound Control Plugin Architecture

**Status**: 📋 Planning  
**Priority**: HIGH - Foundation for all audio mixer integrations  
**Target Release**: v1.1.0  
**Created**: January 10, 2026  
**Last Updated**: January 10, 2026

---

## Executive Summary

This document defines the **plugin architecture** for sound/audio mixer control integration in The Millionaire Game. The architecture enables modular support for multiple mixer systems (Yamaha TF, Behringer X32, Allen & Heath, Soundcraft, etc.) through a DLL-based plugin system, mirroring the lighting control plugin architecture.

**Key Features:**
- Plugin discovery from `lib/dlls/` directory
- Self-identifying plugins with metadata
- Dynamic UI generation based on plugin requirements
- Database-backed configuration storage
- Event-driven command routing
- Support for channel control, muting, scene recall, and mixer automation

---

## Architecture Overview

### Component Diagram

```
┌──────────────────────────────────────────────────────────────────┐
│                      Millionaire Game (Main App)                 │
│                                                                  │
│  ┌──────────────────┐         ┌─────────────────────────────┐   │
│  │ ControlPanelForm │────────▶│  Sound Manager (Core)       │   │
│  │  (UI Actions)    │         │  - Plugin Discovery         │   │
│  └──────────────────┘         │  - Event Routing            │   │
│                               │  - Connection Management    │   │
│  ┌──────────────────┐         └────────┬────────────────────┘   │
│  │  Settings Dialog │◀────────────────┘                         │
│  │  (Configuration) │         Plugin Interface (ISoundPlugin)   │
│  │  - Mixer Picker  │                  ▲                        │
│  │  - Channel Map   │                  │                        │
│  └──────────────────┘                  │                        │
│                                        │                        │
│  ┌──────────────────┐                  │                        │
│  │ Mixer Control    │◀─────────────────┘                        │
│  │ Form (Modeless)  │         Mixer Control UI                  │
│  │  - Channel Strips│                                           │
│  │  - Faders/Mutes  │                                           │
│  └──────────────────┘                                           │
└─────────────────────────────────────────┼────────────────────────┘
                                         │
                     ┌───────────────────┴───────────────────┐
                     │                                       │
      ┌──────────────▼───────────┐         ┌────────────────▼─────────┐
      │  Plugin DLL              │         │  Plugin DLL              │
      │  (lib/dlls/)             │         │  (lib/dlls/)             │
      │                          │         │                          │
      │  YamahaRcpPlugin.dll     │         │  BehringerX32Plugin.dll  │
      │  - ISoundPlugin          │         │  - ISoundPlugin          │
      │  - RCP Protocol Handler  │         │  - OSC Protocol Handler  │
      │  - Channel Management    │         │  - Channel Management    │
      │  - Plugin Metadata       │         │  - Plugin Metadata       │
      └──────────────────────────┘         └──────────────────────────┘
               ▼                                     ▼
       [Yamaha TF Rack]                     [Behringer X32]
```

---

## Core Plugin System

### 1. Plugin Interface (`ISoundPlugin`)

**Location**: `src/MillionaireGame.Core/Sound/ISoundPlugin.cs`

```csharp
namespace MillionaireGame.Core.Sound
{
    public interface ISoundPlugin : IDisposable
    {
        // Plugin Metadata
        SoundPluginMetadata Metadata { get; }
        MixerCapabilities GetCapabilities();
        
        // Connection Management
        Task<bool> ConnectAsync(SoundConnectionSettings settings);
        void Disconnect();
        bool IsConnected { get; }
        
        // Event Handlers
        event EventHandler<ConnectionStatusChangedEventArgs>? ConnectionStatusChanged;
        event EventHandler<ChannelStateChangedEventArgs>? ChannelStateChanged;
        event EventHandler<SoundCommandResultEventArgs>? CommandResultReceived;
        
        // Channel Control
        Task<SoundCommandResult> SetChannelLevelAsync(int channel, float levelDb);
        Task<SoundCommandResult> SetChannelMuteAsync(int channel, bool muted);
        Task<SoundCommandResult> SetChannelLabelAsync(int channel, string label);
        Task<float> GetChannelLevelAsync(int channel);
        Task<bool> GetChannelMuteAsync(int channel);
        Task<string> GetChannelLabelAsync(int channel);
        
        // Bulk Operations
        Task<SoundCommandResult> SetMultipleChannelLevelsAsync(Dictionary<int, float> channelLevels);
        Task<SoundCommandResult> MuteAllChannelsAsync(List<int> channels);
        Task<SoundCommandResult> UnmuteAllChannelsAsync(List<int> channels);
        
        // Scene Management (if supported)
        Task<SoundCommandResult> RecallSceneAsync(int sceneNumber);
        Task<SoundCommandResult> StoreSceneAsync(int sceneNumber, string? sceneName = null);
        Task<List<SceneInfo>> GetSceneListAsync();
        
        // Advanced Features (if supported)
        Task<SoundCommandResult> SetChannelPanAsync(int channel, float panValue);
        Task<SoundCommandResult> SetChannelGainAsync(int channel, float gainDb);
        Task<SoundCommandResult> SetAuxSendAsync(int channel, int auxBus, float levelDb);
        
        // Channel Discovery
        Task<List<ChannelInfo>> GetChannelListAsync();
        Task<int> GetChannelCountAsync();
        
        // Configuration UI Metadata (plugins define their own settings)
        SoundPluginConfigUI GetConfigurationUI();
        
        // Configuration Validation
        bool ValidateSettings(SoundConnectionSettings settings, out string errorMessage);
        
        // Custom Commands (for advanced users)
        Task<SoundCommandResult> SendCustomCommandAsync(string command);
    }
}
```

**Purpose:** Standardized interface that all sound/mixer plugins must implement. Enables the main application to interact with any mixer system without knowing implementation details.

---

### 2. Plugin Metadata

```csharp
namespace MillionaireGame.Core.Sound
{
    public class SoundPluginMetadata
    {
        public string PluginType { get; set; } = "SoundControl";
        public string SystemName { get; set; } = "";        // Unique identifier (e.g., "Yamaha_TF_Rack")
        public string DisplayName { get; set; } = "";       // User-friendly name (e.g., "Yamaha TF Series")
        public string Manufacturer { get; set; } = "";      // Mixer manufacturer
        public string Version { get; set; } = "";           // Plugin version
        public string Author { get; set; } = "";            // Plugin developer
        public string Description { get; set; } = "";       // Short description
        public List<string> SupportedProtocols { get; set; } = new(); // e.g., ["RCP", "OSC", "MIDI"]
        public List<string> SupportedModels { get; set; } = new();    // e.g., ["TF1", "TF3", "TF5", "TF-RACK"]
        public string IconResourcePath { get; set; } = "";  // Optional icon
        public string HelpUrl { get; set; } = "";           // Link to documentation
    }
}
```

**Purpose:** Allows plugins to self-identify their capabilities and metadata for display in UI dropdowns.

---

### 3. Mixer Capabilities

```csharp
namespace MillionaireGame.Core.Sound
{
    public class MixerCapabilities
    {
        // Channel Features
        public int MaxInputChannels { get; set; }           // Maximum input channels
        public int MaxOutputChannels { get; set; }          // Maximum output/bus channels
        public int MaxAuxBuses { get; set; }                // Maximum aux sends
        public bool SupportsChannelLabels { get; set; }     // Can set channel names
        public bool SupportsChannelColors { get; set; }     // Can set channel colors
        
        // Control Features
        public bool SupportsFaderControl { get; set; }      // Can control fader levels
        public bool SupportsMuteControl { get; set; }       // Can mute/unmute channels
        public bool SupportsPanControl { get; set; }        // Can control pan
        public bool SupportsGainControl { get; set; }       // Can control input gain
        public bool SupportsAuxSends { get; set; }          // Can control aux sends
        
        // Scene Management
        public bool SupportsScenes { get; set; }            // Has scene recall
        public int MaxScenes { get; set; }                  // Maximum scenes
        public bool SupportsSceneNames { get; set; }        // Can name scenes
        
        // Advanced Features
        public bool SupportsBidirectionalCommunication { get; set; } // Can receive state updates
        public bool SupportsLevelMetering { get; set; }     // Can read channel levels
        public bool SupportsEQ { get; set; }                // Has EQ control
        public bool SupportsDynamics { get; set; }          // Has compressor/gate
        public bool SupportsEffects { get; set; }           // Has effects processing
        public bool SupportsAutomation { get; set; }        // Supports automation recording
        
        // Connection
        public List<string> SupportedConnectionTypes { get; set; } = new(); // TCP, UDP, MIDI, etc.
        public bool RequiresAuthentication { get; set; }    // Needs password/auth
        
        // Data Range Information
        public float MinFaderLevelDb { get; set; } = -138f;
        public float MaxFaderLevelDb { get; set; } = 10f;
        public float FaderResolution { get; set; } = 0.01f; // dB per step
    }
}
```

**Purpose:** Declares what features a specific mixer supports, allowing UI to adapt dynamically.

---

### 4. Dynamic Configuration UI

```csharp
namespace MillionaireGame.Core.Sound
{
    public class SoundPluginConfigUI
    {
        public List<ConfigSettingDefinition> ConnectionSettings { get; set; } = new();
        public List<ConfigSettingDefinition> ChannelSettings { get; set; } = new();
        public bool SupportsChannelRefresh { get; set; }
        public bool SupportsSceneManagement { get; set; }
        public string ConnectionGroupTitle { get; set; } = "Connection Settings";
        public string ChannelGroupTitle { get; set; } = "Channel Configuration";
        public string SceneGroupTitle { get; set; } = "Scene Mappings";
    }

    public class ConfigSettingDefinition
    {
        public string Key { get; set; } = "";              // Setting identifier (saved to DB)
        public string Label { get; set; } = "";            // UI label text
        public ConfigSettingType Type { get; set; }        // Control type
        public object? DefaultValue { get; set; }          // Default value
        public string Tooltip { get; set; } = "";          // Tooltip text
        public object? ValidationRule { get; set; }        // Min/max, regex, etc.
        public List<string>? OptionValues { get; set; }    // For dropdowns/radios
        public bool IsRequired { get; set; } = true;       // Field is mandatory
    }

    public enum ConfigSettingType
    {
        TextBox,
        NumericUpDown,
        CheckBox,
        RadioButtonGroup,
        ComboBox,
        PasswordBox
    }
}
```

**Purpose:** Plugins define what settings they need, and the UI dynamically generates controls. Eliminates hardcoded UI for each mixer type.

---

### 5. Plugin Manager (`SoundManager`)

**Location**: `src/MillionaireGame.Core/Sound/SoundManager.cs`

```csharp
namespace MillionaireGame.Core.Sound
{
    public class SoundManager : IDisposable
    {
        private readonly SoundSettingsRepository _repository;
        private readonly Dictionary<string, ISoundPlugin> _loadedPlugins;
        private ISoundPlugin? _activePlugin;
        
        public event EventHandler<SoundEventArgs>? SoundEventTriggered;
        public event EventHandler<ChannelStateChangedEventArgs>? ChannelStateChanged;
        
        public SoundManager(SoundSettingsRepository repository)
        {
            _repository = repository;
            _loadedPlugins = new Dictionary<string, ISoundPlugin>();
            DiscoverPlugins();
        }
        
        // Plugin Discovery & Management
        private void DiscoverPlugins()
        {
            // Scan lib/dlls/ for assemblies implementing ISoundPlugin
            // Filter by Metadata.PluginType == "SoundControl"
            // Load plugin metadata without instantiating
        }
        
        public List<SoundPluginMetadata> GetAvailablePlugins()
        {
            // Return metadata for all discovered sound plugins
        }
        
        public ISoundPlugin? LoadPlugin(string systemName)
        {
            // Instantiate plugin by system name
        }
        
        public void UnloadPlugin(string systemName)
        {
            // Dispose and remove plugin
        }
        
        // Active Plugin Management
        public async Task<bool> SetActivePluginAsync(string systemName)
        {
            // Unload current, load new, connect if AutoConnect
        }
        
        public ISoundPlugin? GetActivePlugin()
        {
            return _activePlugin;
        }
        
        // Command Routing (called from game events)
        public async Task TriggerGameEventAsync(string gameEvent)
        {
            // Look up mapping in database
            // Route to active plugin's appropriate method
        }
        
        public async Task SetChannelLevelAsync(int channel, float levelDb)
        {
            // Direct channel control
        }
        
        public async Task MuteChannelAsync(int channel)
        {
            // Direct mute control
        }
        
        public async Task UnmuteChannelAsync(int channel)
        {
            // Direct unmute control
        }
        
        public async Task RecallSceneAsync(int sceneNumber)
        {
            // Scene recall
        }
        
        public async Task EmergencyMuteAllAsync()
        {
            // Emergency mute all channels
        }
        
        public void Dispose()
        {
            _activePlugin?.Disconnect();
            _activePlugin?.Dispose();
            foreach (var plugin in _loadedPlugins.Values)
            {
                plugin.Dispose();
            }
        }
    }
}
```

**Key Responsibilities:**
1. **Plugin Discovery**: Scans `lib/dlls/` for sound plugin assemblies (filtered by PluginType)
2. **Plugin Lifecycle**: Load, instantiate, unload plugins
3. **Event Routing**: Routes game events to appropriate plugin commands
4. **State Management**: Tracks active plugin and connection state

---

### 6. Data Models

#### ChannelInfo

```csharp
namespace MillionaireGame.Core.Sound
{
    public class ChannelInfo
    {
        public int ChannelNumber { get; set; }          // 1-based channel number
        public string Label { get; set; } = "";         // Channel name
        public string Color { get; set; } = "";         // Color code (if supported)
        public ChannelType Type { get; set; }           // Input, Output, Bus, Aux, etc.
        public string DisplayText => $"Ch {ChannelNumber}: {Label}";
    }

    public enum ChannelType
    {
        Input,
        Output,
        Bus,
        Aux,
        Matrix,
        FX,
        DCA,
        Group
    }
}
```

#### ChannelState

```csharp
namespace MillionaireGame.Core.Sound
{
    public class ChannelState
    {
        public int ChannelNumber { get; set; }
        public float FaderLevel { get; set; }           // dB
        public bool IsMuted { get; set; }
        public string Label { get; set; } = "";
        public string Color { get; set; } = "";
        public float PanValue { get; set; }             // -1.0 (L) to 1.0 (R)
        public float GainValue { get; set; }            // dB
        public DateTime LastUpdated { get; set; }
    }
}
```

#### SoundConnectionSettings

```csharp
namespace MillionaireGame.Core.Sound
{
    public class SoundConnectionSettings
    {
        public string HostAddress { get; set; } = "";
        public int Port { get; set; } = 49280;
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public bool UseSSL { get; set; } = false;
        public int TimeoutMs { get; set; } = 3000;
        public Dictionary<string, string> CustomSettings { get; set; } = new();  // Plugin-specific
    }
}
```

#### SceneInfo

```csharp
namespace MillionaireGame.Core.Sound
{
    public class SceneInfo
    {
        public int SceneNumber { get; set; }
        public string SceneName { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime? LastModified { get; set; }
        public string DisplayText => $"Scene {SceneNumber}: {SceneName}";
    }
}
```

#### ChannelMapping

```csharp
namespace MillionaireGame.Core.Sound
{
    public class ChannelMapping
    {
        public string GameRole { get; set; } = "";      // e.g., "HostMicrophone", "PlayerMicrophone"
        public int ChannelNumber { get; set; }
        public string Description { get; set; } = "";
    }

    public static class GameRoles
    {
        public const string HostMicrophone = "HostMicrophone";
        public const string PlayerMicrophone = "PlayerMicrophone";
        public const string AudienceMicrophone = "AudienceMicrophone";
        public const string MusicPlayback = "MusicPlayback";
        public const string SFXPlayback = "SFXPlayback";
        public const string PhoneFriendAudio = "PhoneFriendAudio";
    }
}
```

#### Command Result

```csharp
namespace MillionaireGame.Core.Sound
{
    public class SoundCommandResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string ErrorCode { get; set; } = "";
        public object? ReturnValue { get; set; }
        public TimeSpan ExecutionTime { get; set; }
    }
}
```

---

## Database Schema

### SoundSettings Table
```sql
CREATE TABLE SoundSettings (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Enabled BIT NOT NULL DEFAULT 0,
    ActivePluginSystemName NVARCHAR(100) NULL,
    AutoConnectOnStartup BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);
```

### SoundConnectionSettings Table
```sql
CREATE TABLE SoundConnectionSettings (
    Id INT PRIMARY KEY IDENTITY(1,1),
    SoundSettingsId INT NOT NULL,
    HostAddress NVARCHAR(255) NOT NULL,
    Port INT NOT NULL DEFAULT 49280,
    Username NVARCHAR(100) NULL,
    Password NVARCHAR(255) NULL,  -- Encrypted
    UseSSL BIT NOT NULL DEFAULT 0,
    TimeoutMs INT NOT NULL DEFAULT 3000,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (SoundSettingsId) REFERENCES SoundSettings(Id) ON DELETE CASCADE
);
```

### SoundPluginSettings Table
```sql
CREATE TABLE SoundPluginSettings (
    Id INT PRIMARY KEY IDENTITY(1,1),
    SoundConnectionSettingsId INT NOT NULL,
    SettingKey NVARCHAR(100) NOT NULL,
    SettingValue NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (SoundConnectionSettingsId) REFERENCES SoundConnectionSettings(Id) ON DELETE CASCADE,
    CONSTRAINT UK_SoundPluginSettings UNIQUE (SoundConnectionSettingsId, SettingKey)
);
```

### ChannelMappings Table
```sql
CREATE TABLE ChannelMappings (
    Id INT PRIMARY KEY IDENTITY(1,1),
    SoundSettingsId INT NOT NULL,
    GameRole NVARCHAR(100) NOT NULL,
    ChannelNumber INT NOT NULL,
    Description NVARCHAR(255) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (SoundSettingsId) REFERENCES SoundSettings(Id) ON DELETE CASCADE,
    CONSTRAINT UK_ChannelMappings UNIQUE (SoundSettingsId, GameRole)
);
```

### SceneMappings Table
```sql
CREATE TABLE SceneMappings (
    Id INT PRIMARY KEY IDENTITY(1,1),
    SoundSettingsId INT NOT NULL,
    GameEvent NVARCHAR(100) NOT NULL,
    SceneNumber INT NOT NULL,
    DelayMs INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (SoundSettingsId) REFERENCES SoundSettings(Id) ON DELETE CASCADE,
    CONSTRAINT UK_SceneMappings UNIQUE (SoundSettingsId, GameEvent)
);
```

---

## Yamaha RCP Plugin Implementation

### Plugin Project Structure

```
src/MillionaireGame.Plugins.Yamaha/
├── MillionaireGame.Plugins.Yamaha.csproj
├── YamahaRcpPlugin.cs              (ISoundPlugin implementation)
├── YamahaRcpClient.cs              (Low-level RCP protocol)
├── Models/
│   ├── RcpCommand.cs
│   ├── RcpResponse.cs
│   └── YamahaChannelState.cs
└── Resources/
    └── yamaha_icon.png
```

### YamahaRcpPlugin.cs

```csharp
using MillionaireGame.Core.Sound;

namespace MillionaireGame.Plugins.Yamaha
{
    public class YamahaRcpPlugin : ISoundPlugin
    {
        private readonly YamahaRcpClient _client;
        private readonly Dictionary<int, ChannelState> _channelCache;
        private bool _disposed;

        public SoundPluginMetadata Metadata { get; }

        public YamahaRcpPlugin()
        {
            _client = new YamahaRcpClient();
            _channelCache = new Dictionary<int, ChannelState>();
            
            Metadata = new SoundPluginMetadata
            {
                PluginType = "SoundControl",
                SystemName = "Yamaha_TF_Rack",
                DisplayName = "Yamaha TF Series",
                Manufacturer = "Yamaha",
                Version = "1.0.0",
                Author = "The Millionaire Game Team",
                Description = "Control Yamaha TF series mixing consoles via RCP protocol",
                SupportedProtocols = new List<string> { "RCP (TCP)" },
                SupportedModels = new List<string> { "TF1", "TF3", "TF5", "TF-RACK" },
                HelpUrl = "https://github.com/jdelgado-dtlabs/TheMillionaireGame/wiki/Yamaha-TF-Integration"
            };
        }

        public MixerCapabilities GetCapabilities()
        {
            return new MixerCapabilities
            {
                MaxInputChannels = 40,
                MaxOutputChannels = 20,
                MaxAuxBuses = 8,
                SupportsChannelLabels = true,
                SupportsChannelColors = true,
                SupportsFaderControl = true,
                SupportsMuteControl = true,
                SupportsPanControl = true,
                SupportsGainControl = false,
                SupportsAuxSends = true,
                SupportsScenes = true,
                MaxScenes = 100,
                SupportsSceneNames = true,
                SupportsBidirectionalCommunication = true,
                SupportsLevelMetering = false,
                SupportsEQ = true,
                SupportsDynamics = true,
                SupportsEffects = true,
                SupportsAutomation = false,
                SupportedConnectionTypes = new List<string> { "TCP" },
                RequiresAuthentication = false,
                MinFaderLevelDb = -138f,
                MaxFaderLevelDb = 10f,
                FaderResolution = 0.01f
            };
        }

        public SoundPluginConfigUI GetConfigurationUI()
        {
            return new SoundPluginConfigUI
            {
                ConnectionGroupTitle = "Yamaha TF Connection",
                ChannelGroupTitle = "Channel Configuration",
                SceneGroupTitle = "Scene Mappings",
                SupportsChannelRefresh = true,
                SupportsSceneManagement = true,
                ConnectionSettings = new List<ConfigSettingDefinition>
                {
                    new ConfigSettingDefinition
                    {
                        Key = "HostAddress",
                        Label = "Mixer IP Address:",
                        Type = ConfigSettingType.TextBox,
                        DefaultValue = "192.168.0.128",
                        Tooltip = "IP address of the Yamaha TF console",
                        ValidationRule = @"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$",
                        IsRequired = true
                    },
                    new ConfigSettingDefinition
                    {
                        Key = "Port",
                        Label = "Port:",
                        Type = ConfigSettingType.NumericUpDown,
                        DefaultValue = 49280,
                        Tooltip = "RCP port (default: 49280)",
                        ValidationRule = new { Min = 1, Max = 65535 },
                        IsRequired = true
                    },
                    new ConfigSettingDefinition
                    {
                        Key = "AutoConnect",
                        Label = "Auto-connect on startup",
                        Type = ConfigSettingType.CheckBox,
                        DefaultValue = false,
                        Tooltip = "Automatically connect when the application starts"
                    }
                },
                ChannelSettings = new List<ConfigSettingDefinition>
                {
                    new ConfigSettingDefinition
                    {
                        Key = "ControlledChannels",
                        Label = "Controlled Channels:",
                        Type = ConfigSettingType.TextBox,
                        DefaultValue = "1-8",
                        Tooltip = "Channel range to display (e.g., 1-8, 1,3,5-10)",
                        IsRequired = true
                    }
                }
            };
        }

        public async Task<bool> ConnectAsync(SoundConnectionSettings settings)
        {
            return await _client.ConnectAsync(settings.HostAddress, settings.Port);
        }

        public void Disconnect()
        {
            _client.Disconnect();
        }

        public bool IsConnected => _client.IsConnected;

        public event EventHandler<ConnectionStatusChangedEventArgs>? ConnectionStatusChanged;
        public event EventHandler<ChannelStateChangedEventArgs>? ChannelStateChanged;
        public event EventHandler<SoundCommandResultEventArgs>? CommandResultReceived;

        public async Task<SoundCommandResult> SetChannelLevelAsync(int channel, float levelDb)
        {
            // Convert dB to RCP integer format (dB * 100)
            int levelValue = (int)(levelDb * 100);
            var command = new RcpCommand("set", "MIXER:Current/InCh/Fader/Level", channel - 1, 0, levelValue);
            var response = await _client.SendCommandAsync(command);
            
            return new SoundCommandResult
            {
                Success = response.IsSuccess,
                Message = response.ErrorMessage ?? "Level set successfully",
                ExecutionTime = response.ExecutionTime
            };
        }

        // Additional methods implementation...

        public bool ValidateSettings(SoundConnectionSettings settings, out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(settings.HostAddress))
            {
                errorMessage = "IP address is required";
                return false;
            }

            if (settings.Port < 1 || settings.Port > 65535)
            {
                errorMessage = "Port must be between 1 and 65535";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _client.Dispose();
                _disposed = true;
            }
        }
    }
}
```

---

## Unified Plugin Manager Architecture

### Option 1: Separate Managers (Recommended)

**Rationale:** Sound and lighting plugins serve different purposes with distinct UIs and workflows.

```
PluginManager (Abstract Base)
    ├── LightingManager (Lighting-specific)
    └── SoundManager (Sound-specific)
```

**Shared Code**: Plugin discovery, loading, and lifecycle management
**Separate Code**: Command routing, event handling, UI generation

### Option 2: Unified Plugin Manager

**Rationale:** Single manager handles all plugin types.

```csharp
public class PluginManager : IDisposable
{
    private readonly Dictionary<string, IPlugin> _loadedPlugins;
    
    public List<IPlugin> DiscoverPlugins(string pluginType)
    {
        // Scan lib/dlls/ and filter by Metadata.PluginType
    }
    
    public IPlugin? LoadPlugin(string systemName, string pluginType)
    {
        // Load and instantiate plugin
    }
}
```

**Decision**: Use **Option 1** for better separation of concerns and maintainability.

---

## Settings UI Architecture

### Plugin-Provided Configuration Flow

1. **User selects plugin** from dropdown in Settings → Sound tab
2. **OptionsDialog calls** `plugin.GetConfigurationUI()`
3. **UI generator dynamically creates controls** based on `ConfigSettingDefinition` list
4. **Plugin-specific groups** appear with custom titles
5. **User configures and saves** → Settings written to database
6. **Plugin validates** via `ValidateSettings()` before saving

### UI Layout

```
╔══════════════════════════════════════════════════════════════════╗
║                           Sound                                  ║
╠══════════════════════════════════════════════════════════════════╣
║                                                                  ║
║  ┌─ Sound System (Built-in) ──────────────────────────────────┐  ║
║  │  ☑ Enable sound control                                    │  ║
║  │  System:  [Select Mixer...                              ▼] │  ║
║  │  Status:  ● Connected | ○ Disconnected                     │  ║
║  └────────────────────────────────────────────────────────────┘  ║
║                                                                  ║
║  ┌─ {Plugin-Provided Connection Title} ─────────────────────┐  ║
║  │  [Dynamically generated controls based on plugin]        │  ║
║  │  [ Test Connection ]                                      │  ║
║  └────────────────────────────────────────────────────────────┘  ║
║                                                                  ║
║  ┌─ {Plugin-Provided Channel Title} ──────────────────────────┐  ║
║  │  [Channel configuration controls]                         │  ║
║  │  [ Open Mixer Control ]                                   │  ║
║  └────────────────────────────────────────────────────────────┘  ║
║                                                                  ║
║  ┌─ Channel Role Mappings ─────────────────────────────────────┐  ║
║  │  Host Microphone:     [Channel:  8              ▼]        │  ║
║  │  Player Microphone:   [Channel:  9              ▼]        │  ║
║  │  Audience Microphone: [Channel: 10              ▼]        │  ║
║  │  Music Playback:      [Channel: 15              ▼]        │  ║
║  │  SFX Playback:        [Channel: 16              ▼]        │  ║
║  └────────────────────────────────────────────────────────────┘  ║
╚══════════════════════════════════════════════════════════════════╝
```

---

## Mixer Control Form

### Modeless Window Design

**Location**: `src/MillionaireGame/Forms/MixerControlForm.cs`

```csharp
public partial class MixerControlForm : Form
{
    private readonly ISoundPlugin? _plugin;
    private readonly List<int> _controlledChannels;
    
    public MixerControlForm(ISoundPlugin plugin, List<int> controlledChannels)
    {
        InitializeComponent();
        _plugin = plugin;
        _controlledChannels = controlledChannels;
        
        _plugin.ChannelStateChanged += OnChannelStateChanged;
        _plugin.ConnectionStatusChanged += OnConnectionStatusChanged;
        
        InitializeChannelStrips();
    }
    
    private void InitializeChannelStrips()
    {
        // Create channel strip for each controlled channel
    }
    
    private Panel CreateChannelStrip(int channelNumber)
    {
        // Vertical fader + mute button + label
    }
}
```

### UI Components

- **Connection Panel** (Top): Status, connect/disconnect
- **Channel Strip Panel** (Main): Scrollable horizontal strips
- **Master Section** (Bottom): Refresh, close buttons

---

## Game Event Integration

### Event Trigger Points

| Game Event | Description | Potential Action |
|-----------|-------------|------------------|
| `GameStart` | Game begins | Unmute host mic, recall intro scene |
| `QuestionDisplayed` | Question appears | Adjust music level |
| `PlayerAnswering` | Player thinking | Mute audience mics |
| `FinalAnswerLocked` | Answer submitted | Recall dramatic scene |
| `CorrectAnswer` | Correct response | Unmute celebration audio |
| `WrongAnswer` | Wrong response | Recall failure scene |
| `Lifeline_PhoneFriend` | Phone-a-Friend | Unmute phone audio channel |
| `GameEnd_Win` | Player wins | Recall victory scene |
| `GameEnd_Lose` | Player loses | Recall consolation scene |

### Integration in ControlPanelForm

```csharp
private readonly SoundManager? _soundManager;

// In constructor:
if (settings.Sound.Enabled)
{
    _soundManager = new SoundManager(soundRepository);
    await _soundManager.SetActivePluginAsync(settings.Sound.ActivePluginSystemName);
}

// In event handlers:
private async void OnQuestionDisplayed()
{
    // Existing game logic...
    
    // Trigger sound automation
    await _soundManager?.TriggerGameEventAsync("QuestionDisplayed");
}
```

---

## Implementation Phases

### Phase 1: Core Plugin System (8-10 hours)

**Files to Create:**
- `src/MillionaireGame.Core/Sound/ISoundPlugin.cs`
- `src/MillionaireGame.Core/Sound/SoundManager.cs`
- `src/MillionaireGame.Core/Sound/SoundPluginMetadata.cs`
- `src/MillionaireGame.Core/Sound/MixerCapabilities.cs`
- `src/MillionaireGame.Core/Sound/SoundPluginConfigUI.cs`
- `src/MillionaireGame.Core/Sound/ChannelInfo.cs`
- `src/MillionaireGame.Core/Sound/ChannelState.cs`
- `src/MillionaireGame.Core/Sound/SoundCommandResult.cs`
- `src/MillionaireGame.Core/Sound/SoundConnectionSettings.cs`
- `src/MillionaireGame.Core/Sound/ChannelMapping.cs`
- `src/MillionaireGame.Core/Sound/SceneInfo.cs`

**Tasks:**
1. Define `ISoundPlugin` interface
2. Implement `SoundManager` with plugin discovery
3. Create all data model classes
4. Implement plugin loading from `lib/dlls/` (filter by PluginType)
5. Add logging via `GameConsole`

### Phase 2: Database Schema & Repository (4-5 hours)

**Files to Create:**
- `src/MillionaireGame.Core/Database/SoundSettingsRepository.cs`
- `lib/sql/sound_schema.sql`

**Tasks:**
1. Create SQL schema migration script
2. Implement repository CRUD operations
3. Add validation logic
4. Create default data initialization

### Phase 3: Yamaha RCP Plugin (12-15 hours)

**Files to Create:**
- `src/MillionaireGame.Plugins.Yamaha/YamahaRcpPlugin.cs`
- `src/MillionaireGame.Plugins.Yamaha/YamahaRcpClient.cs`
- `src/MillionaireGame.Plugins.Yamaha/Models/RcpCommand.cs`
- `src/MillionaireGame.Plugins.Yamaha/Models/RcpResponse.cs`

**Tasks:**
1. Implement ISoundPlugin interface
2. Implement RCP protocol TCP client
3. Implement command serialization/deserialization
4. Implement NOTIFY message handling
5. Implement all channel control methods
6. Add error handling and reconnection logic
7. Build and deploy to `lib/dlls/`

### Phase 4: Settings UI (8-10 hours)

**Files to Modify:**
- `src/MillionaireGame/Forms/Options/OptionsDialog.Designer.cs`
- `src/MillionaireGame/Forms/Options/OptionsDialog.cs`

**Tasks:**
1. Add Sound tab to OptionsDialog
2. Implement dynamic control generation based on `GetConfigurationUI()`
3. Create channel mapping UI
4. Implement scene mapping UI (if supported)
5. Implement Load/Save logic with database
6. Add plugin dropdown population

### Phase 5: Mixer Control Form (10-12 hours)

**Files to Create:**
- `src/MillionaireGame/Forms/MixerControlForm.cs`
- `src/MillionaireGame/Forms/MixerControlForm.Designer.cs`

**Tasks:**
1. Design modeless form layout
2. Implement channel strip factory
3. Bind faders to plugin methods
4. Bind mute buttons to plugin methods
5. Implement real-time state updates
6. Add connection status monitoring
7. Integrate with main menu (Game → Mixer Control)

### Phase 6: Game Integration (6-8 hours)

**Files to Modify:**
- `src/MillionaireGame/Forms/ControlPanelForm.cs`
- `src/MillionaireGame/Forms/MainForm.cs`

**Tasks:**
1. Add `SoundManager` instance
2. Integrate event triggers at game event points
3. Add connection status indicator
4. Add menu item for mixer control form
5. Implement cleanup/disposal

---

## Plugin Development Guide

### Creating a New Sound Plugin

1. **Create new Class Library project**
   ```
   dotnet new classlib -n MillionaireGame.Plugins.[Manufacturer]
   ```

2. **Reference Core**
   ```xml
   <ProjectReference Include="..\..\MillionaireGame.Core\MillionaireGame.Core.csproj" />
   ```

3. **Implement ISoundPlugin**
   ```csharp
   public class MyMixerPlugin : ISoundPlugin
   {
       public SoundPluginMetadata Metadata { get; }
       
       public MyMixerPlugin()
       {
           Metadata = new SoundPluginMetadata
           {
               PluginType = "SoundControl",  // CRITICAL for discovery
               SystemName = "MyMixer_System",
               // ... other metadata
           };
       }
       
       // Implement all ISoundPlugin methods...
   }
   ```

4. **Define Configuration UI**
   ```csharp
   public SoundPluginConfigUI GetConfigurationUI()
   {
       return new SoundPluginConfigUI
       {
           ConnectionSettings = new List<ConfigSettingDefinition>
           {
               // Define your connection settings
           }
       };
   }
   ```

5. **Build and deploy to `lib/dlls/`**

---

## Testing Strategy

### Unit Tests
- Plugin discovery and loading (filter by PluginType)
- Metadata parsing
- Database CRUD operations
- Settings validation
- Channel control methods
- Scene recall methods

### Integration Tests
- Plugin lifecycle (load, connect, disconnect, unload)
- Command routing through SoundManager
- Database transactions
- Dynamic UI generation
- Bidirectional communication (NOTIFY messages)

### Manual Testing
- Install Yamaha plugin DLL → appears in dropdown
- Select plugin → settings UI generates correctly
- Configure connection → saves to database
- Test connection → connects successfully
- Open mixer control form → channels display
- Move fader → mixer responds
- External mixer change → UI updates
- Trigger game event → routes to plugin
- Disable plugin → commands stop

---

## Security Considerations

### Network Security
- **No Authentication**: Most mixer protocols lack built-in auth (secure network required)
- **Encryption**: Consider SSL/TLS for plugins that support it
- **Password Storage**: Encrypt passwords in database

### Plugin Security
- **Code Signing**: Consider signing plugin DLLs
- **Sandboxing**: Future enhancement - isolate plugin execution
- **Validation**: Validate plugin assemblies before loading

---

## Success Criteria

- [ ] Sound plugin DLLs discovered automatically from `lib/dlls/` (filtered by PluginType)
- [ ] Plugin metadata displayed in settings dropdown
- [ ] Dynamic UI generated based on `GetConfigurationUI()`
- [ ] Settings persist to database correctly
- [ ] Yamaha RCP plugin connects and controls mixer
- [ ] Channel fader control works bidirectionally
- [ ] Mute/unmute works correctly
- [ ] Scene recall functions properly
- [ ] Mixer control form opens and operates
- [ ] Game events route to active plugin
- [ ] Multiple plugins can coexist without conflicts
- [ ] Plugin load/unload without memory leaks
- [ ] Connection status updates in real-time

---

## Future Plugin Examples

### Behringer X32/M32
- **Protocol**: OSC (UDP port 10023)
- **Features**: 32 channels, scenes, effects

### Allen & Heath SQ/Avantis
- **Protocol**: MIDI or TCP/IP
- **Features**: Scene recall, channel control

### Soundcraft Ui/Vi Series
- **Protocol**: HTTP REST API
- **Features**: Web-based control

### PreSonus StudioLive
- **Protocol**: UC Surface protocol
- **Features**: Fat Channel processing control

---

## Migration Path from Current Yamaha Implementation

1. **Phase 1**: Implement plugin architecture in Core
2. **Phase 2**: Create Yamaha plugin from existing YamahaRcpService code
3. **Phase 3**: Update UI to use plugin system
4. **Phase 4**: Deprecate old YamahaRcpService (mark obsolete)
5. **Phase 5**: Remove old implementation in v1.2.0

**Backward Compatibility**: Settings migration script to convert old Yamaha settings to new plugin format.

---

## Open Questions

1. **Q**: Should we support multiple active plugins simultaneously (e.g., Yamaha + X32)?
   **A**: Phase 2 - start with single active plugin for MVP

2. **Q**: How to handle plugin updates without breaking compatibility?
   **A**: Implement plugin API versioning

3. **Q**: Should mixer control form be dockable?
   **A**: Start modeless, consider docking in future

4. **Q**: How to handle network latency for real-time fader updates?
   **A**: Implement command throttling and optimistic UI updates

5. **Q**: Should we provide a plugin template/SDK?
   **A**: Yes - create template project for developers

---

## References

- **Lighting Plugin Architecture**: `src/docs/active/LIGHTING_PLUGIN_ARCHITECTURE.md`
- **Yamaha TF Mixer Control Plan**: `src/docs/active/YAMAHA_TF_MIXER_CONTROL_PLAN.md`
- [Yamaha RCP Documentation](https://github.com/BrenekH/yamaha-rcp-docs)
- [Behringer X32 OSC Protocol](https://wiki.munichmakerlab.de/wiki/X32)
- [.NET Assembly Loading](https://learn.microsoft.com/en-us/dotnet/core/dependency-loading/)

---

## Summary

This architecture provides:
- **Modularity**: Easy addition of new mixer support
- **Consistency**: Parallel with lighting plugin architecture
- **Flexibility**: Dynamic UI generation based on plugin needs
- **Maintainability**: Separation of concerns, clear plugin boundaries
- **Extensibility**: Support for future mixer protocols and features

The Yamaha TF mixer becomes the **first sound plugin**, residing in `lib/dlls/`, with the architecture designed to support Behringer X32, Allen & Heath, and other mixers through additional plugins.
