# Lighting Control Plugin Architecture

**Status**: 📋 Planning  
**Priority**: HIGH - Foundation for all lighting integrations  
**Target Release**: v1.1.0  
**Created**: January 10, 2026  
**Last Updated**: January 10, 2026

---

## Executive Summary

This document defines the **plugin architecture** for lighting control integration in The Millionaire Game. The architecture enables modular support for multiple lighting console systems (ETC Ion, GrandMA, Chamsys, etc.) through a DLL-based plugin system.

**Key Features:**
- Plugin discovery from `lib/dlls/` directory
- Self-identifying plugins with metadata
- Dynamic UI generation based on plugin requirements
- Database-backed configuration storage
- Event-driven command routing

---

## Architecture Overview

### Component Diagram

```
┌──────────────────────────────────────────────────────────────────┐
│                      Millionaire Game (Main App)                 │
│                                                                  │
│  ┌──────────────────┐         ┌─────────────────────────────┐   │
│  │ ControlPanelForm │────────▶│  Lighting Manager (Core)    │   │
│  │  (UI Actions)    │         │  - Plugin Discovery         │   │
│  └──────────────────┘         │  - Event Routing            │   │
│                               │  - Connection Management    │   │
│  ┌──────────────────┐         └────────┬────────────────────┘   │
│  │  Settings Dialog │◀────────────────┘                         │
│  │  (Configuration) │         Plugin Interface (ILightingPlugin)│
│  │  - System Picker │                  ▲                        │
│  │  - Cue Mappings  │                  │                        │
│  └──────────────────┘                  │                        │
└─────────────────────────────────────────┼────────────────────────┘
                                         │
                     ┌───────────────────┴───────────────────┐
                     │                                       │
      ┌──────────────▼───────────┐         ┌────────────────▼─────────┐
      │  Plugin DLL              │         │  Plugin DLL              │
      │  (lib/dlls/)             │         │  (lib/dlls/)             │
      │                          │         │                          │
      │  - ILightingPlugin       │         │  - ILightingPlugin       │
      │  - Protocol Handler      │         │  - Protocol Handler      │
      │  - Command Builder       │         │  - Command Builder       │
      │  - Plugin Metadata       │         │  - Plugin Metadata       │
      └──────────────────────────┘         └──────────────────────────┘
               ▼                                     ▼
       [Lighting Console]                   [Lighting Console]
```

---

## Core Plugin System

### 1. Plugin Interface (`ILightingPlugin`)

**Location**: `src/MillionaireGame.Core/Lighting/ILightingPlugin.cs`

```csharp
public interface ILightingPlugin : IDisposable
{
    // Plugin Metadata
    LightingPluginMetadata Metadata { get; }
    LightingSystemCapabilities GetCapabilities();
    
    // Connection Management
    Task<bool> ConnectAsync(LightingConnectionSettings settings);
    void Disconnect();
    bool IsConnected { get; }
    
    // Event Handlers
    event EventHandler<ConnectionStatusChangedEventArgs>? ConnectionStatusChanged;
    event EventHandler<LightingCommandResultEventArgs>? CommandResultReceived;
    
    // Command Execution
    Task<LightingCommandResult> TriggerCueAsync(string cueIdentifier);
    Task<LightingCommandResult> TriggerMacroAsync(string macroIdentifier);
    Task<LightingCommandResult> GoAsync(string? cueListId = null);
    Task<LightingCommandResult> StopAsync(string? cueListId = null);
    Task<LightingCommandResult> SendCustomCommandAsync(string command);
    
    // Cue List Retrieval (for UI population)
    Task<List<LightingCueInfo>> GetCueListAsync(string cueListId);
    Task<int> GetCueCountAsync(string cueListId);
    
    // Configuration UI Metadata (plugins define their own settings)
    LightingPluginConfigUI GetConfigurationUI();
    
    // Configuration Validation
    bool ValidateSettings(LightingConnectionSettings settings, out string errorMessage);
}
```

**Purpose:** Standardized interface that all lighting plugins must implement. Enables the main application to interact with any lighting system without knowing implementation details.

---

### 2. Plugin Metadata

```csharp
public class LightingPluginMetadata
{
    public string PluginType { get; set; } = "LightingControl";
    public string SystemName { get; set; } = "";        // Unique identifier (e.g., "ETC Ion Classic")
    public string DisplayName { get; set; } = "";       // User-friendly name
    public string Manufacturer { get; set; } = "";      // Console manufacturer
    public string Version { get; set; } = "";           // Plugin version
    public string Author { get; set; } = "";            // Plugin developer
    public string Description { get; set; } = "";       // Short description
    public List<string> SupportedProtocols { get; set; } = new(); // e.g., ["OSC", "sACN", "ArtNet"]
    public string IconResourcePath { get; set; } = "";  // Optional icon
}
```

**Purpose:** Allows plugins to self-identify their capabilities and metadata for display in UI dropdowns.

---

### 3. System Capabilities

```csharp
public class LightingSystemCapabilities
{
    public bool SupportsCueLists { get; set; }          // Can organize cues in lists
    public bool SupportsMacros { get; set; }            // Has macro functionality
    public bool SupportsSubmasters { get; set; }        // Has submaster faders
    public bool SupportsBlackout { get; set; }          // Has blackout function
    public bool SupportsDirectDMX { get; set; }         // Can control DMX directly
    public bool SupportsBidirectionalCommunication { get; set; } // Can receive feedback
    public int MaxCueLists { get; set; }                // Maximum cue lists
    public int MaxCuesPerList { get; set; }             // Maximum cues per list
}
```

**Purpose:** Declares what features a specific console supports, allowing UI to adapt dynamically.

---

### 4. Dynamic Configuration UI

```csharp
public class LightingPluginConfigUI
{
    public List<ConfigSettingDefinition> ConnectionSettings { get; set; } = new();
    public bool SupportsCueListRefresh { get; set; }
    public List<string> SupportedCommandTypes { get; set; } = new();
    public string ConnectionGroupTitle { get; set; } = "Connection Settings";
    public string CueMappingsGroupTitle { get; set; } = "Cue Mappings";
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
}

public enum ConfigSettingType
{
    TextBox,
    NumericUpDown,
    CheckBox,
    RadioButtonGroup,
    ComboBox
}
```

**Purpose:** Plugins define what settings they need, and the UI dynamically generates controls. Eliminates hardcoded UI for each console type.

**Example Usage:**
```csharp
// ETC Ion plugin defines its settings
public LightingPluginConfigUI GetConfigurationUI()
{
    return new LightingPluginConfigUI
    {
        ConnectionGroupTitle = "ETC Ion Connection",
        ConnectionSettings = new List<ConfigSettingDefinition>
        {
            new() { Key = "HostAddress", Label = "IP Address:", Type = ConfigSettingType.TextBox, DefaultValue = "192.168.1.100" },
            new() { Key = "SendPort", Label = "Send Port:", Type = ConfigSettingType.NumericUpDown, DefaultValue = 8000 },
            new() { Key = "ReceivePort", Label = "Receive Port:", Type = ConfigSettingType.NumericUpDown, DefaultValue = 8001 },
            new() { Key = "Transport", Label = "Protocol:", Type = ConfigSettingType.RadioButtonGroup, OptionValues = new() { "UDP", "TCP" } }
        }
    };
}
```

---

### 5. Plugin Manager (`LightingManager`)

**Location**: `src/MillionaireGame.Core/Lighting/LightingManager.cs`

```csharp
public class LightingManager : IDisposable
{
    private readonly LightingSettingsRepository _repository;
    private readonly Dictionary<string, ILightingPlugin> _loadedPlugins;
    private ILightingPlugin? _activePlugin;
    
    public event EventHandler<LightingEventArgs>? LightingEventTriggered;
    
    public LightingManager(LightingSettingsRepository repository)
    {
        _repository = repository;
        _loadedPlugins = new Dictionary<string, ILightingPlugin>();
        DiscoverPlugins();
    }
    
    // Plugin Discovery & Management
    private void DiscoverPlugins()
    {
        // Scan lib/dlls/ for assemblies implementing ILightingPlugin
        // Load plugin metadata without instantiating
    }
    
    public List<LightingPluginMetadata> GetAvailablePlugins()
    {
        // Return metadata for all discovered plugins
    }
    
    public ILightingPlugin? LoadPlugin(string systemName)
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
    
    public ILightingPlugin? GetActivePlugin()
    {
        return _activePlugin;
    }
    
    // Command Routing (called from game events)
    public async Task TriggerGameEventAsync(string gameEvent)
    {
        // Look up mapping in database
        // Route to active plugin's TriggerCueAsync()
    }
    
    public async Task TriggerCueAsync(string cueIdentifier)
    {
        // Direct cue trigger bypass mapping lookup
    }
    
    public async Task EmergencyStopAsync()
    {
        // Send STOP command to active plugin
    }
    
    public void Dispose()
    {
        foreach (var plugin in _loadedPlugins.Values)
        {
            plugin?.Dispose();
        }
    }
}
```

**Key Responsibilities:**
1. **Plugin Discovery**: Scans `lib/dlls/` for plugin assemblies
2. **Plugin Lifecycle**: Load, instantiate, unload plugins
3. **Event Routing**: Routes game events to appropriate plugin commands
4. **State Management**: Tracks active plugin and connection state

---

### 6. Data Models

#### LightingCueInfo

```csharp
public class LightingCueInfo
{
    public int Index { get; set; }              // Position in cue list
    public string UniqueId { get; set; } = "";  // UID from console
    public string Label { get; set; } = "";     // Display name
    public string CueNumber { get; set; } = ""; // Trigger identifier
    public int PartNumber { get; set; }         // Cue part (if multi-part)
    public string DisplayText => $"{Label} (Cue {CueNumber})"; // For UI dropdowns
}
```

#### LightingConnectionSettings

```csharp
public class LightingConnectionSettings
{
    public string HostAddress { get; set; } = "";
    public int SendPort { get; set; }
    public int ReceivePort { get; set; }
    public string Protocol { get; set; } = "";              // OSC, sACN, ArtNet, etc.
    public string TransportProtocol { get; set; } = "";     // UDP, TCP
    public Dictionary<string, string> CustomSettings { get; set; } = new();  // Plugin-specific
}
```

#### LightingCueMapping

```csharp
public class LightingCueMapping
{
    public string GameEvent { get; set; } = "";        // e.g., "LightsDown_Q6"
    public string CueIdentifier { get; set; } = "";    // Cue number/name
    public string CueListId { get; set; } = "1";       // Default cue list
    public LightingCommandType CommandType { get; set; }
    public bool Enabled { get; set; } = true;
    public int DelayMs { get; set; } = 0;
}

public enum LightingCommandType
{
    FireCue,
    FireMacro,
    Go,
    Stop,
    Custom
}
```

#### Command Result

```csharp
public class LightingCommandResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public string CommandSent { get; set; } = "";
    public object? ResponseData { get; set; }
    public TimeSpan ExecutionTime { get; set; }
}
```

---

## Database Schema

### LightingSettings Table
```sql
CREATE TABLE LightingSettings (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Enabled BIT NOT NULL DEFAULT 0,
    SelectedSystemName NVARCHAR(100) NULL,     -- Matches plugin Metadata.SystemName
    AutoConnect BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);
```

### LightingConnectionSettings Table
```sql
CREATE TABLE LightingConnectionSettings (
    Id INT PRIMARY KEY IDENTITY(1,1),
    LightingSettingsId INT NOT NULL,
    HostAddress NVARCHAR(255) NOT NULL DEFAULT '192.168.1.100',
    SendPort INT NOT NULL DEFAULT 8000,
    ReceivePort INT NOT NULL DEFAULT 8001,
    Protocol NVARCHAR(50) NOT NULL DEFAULT 'OSC',
    TransportProtocol NVARCHAR(50) NOT NULL DEFAULT 'UDP',
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (LightingSettingsId) REFERENCES LightingSettings(Id) ON DELETE CASCADE
);
```

### LightingPluginSettings Table
```sql
CREATE TABLE LightingPluginSettings (
    Id INT PRIMARY KEY IDENTITY(1,1),
    LightingConnectionSettingsId INT NOT NULL,
    SettingKey NVARCHAR(100) NOT NULL,         -- Maps to ConfigSettingDefinition.Key
    SettingValue NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (LightingConnectionSettingsId) REFERENCES LightingConnectionSettings(Id) ON DELETE CASCADE,
    CONSTRAINT UK_PluginSettings UNIQUE (LightingConnectionSettingsId, SettingKey)
);
```

### LightingCueMappings Table
```sql
CREATE TABLE LightingCueMappings (
    Id INT PRIMARY KEY IDENTITY(1,1),
    LightingSettingsId INT NOT NULL,
    GameEvent NVARCHAR(100) NOT NULL,
    CueIdentifier NVARCHAR(100) NOT NULL,
    CueListId NVARCHAR(50) NOT NULL DEFAULT '1',
    CommandType NVARCHAR(50) NOT NULL DEFAULT 'FireCue',
    Enabled BIT NOT NULL DEFAULT 1,
    DelayMs INT NOT NULL DEFAULT 0,
    CueLabel NVARCHAR(255) NULL,
    CueNumber NVARCHAR(100) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (LightingSettingsId) REFERENCES LightingSettings(Id) ON DELETE CASCADE
);
```

---

## Settings UI Architecture

### Plugin-Provided Configuration Flow

1. **User selects plugin** from dropdown in Settings → Lighting tab
2. **OptionsDialog calls** `plugin.GetConfigurationUI()`
3. **UI generator dynamically creates controls** based on `ConfigSettingDefinition` list
4. **Plugin-specific groups** appear with custom titles
5. **User configures and saves** → Settings written to database
6. **Plugin validates** via `ValidateSettings()` before saving

### UI Layout

```
╔══════════════════════════════════════════════════════════════════╗
║                          Lighting                                ║
╠══════════════════════════════════════════════════════════════════╣
║                                                                  ║
║  ┌─ Lighting System (Built-in) ──────────────────────────────┐  ║
║  │  ☑ Enable lighting control                                │  ║
║  │  System:  [Select Console...                           ▼] │  ║
║  │  Status:  ● Connected | ○ Disconnected                    │  ║
║  └────────────────────────────────────────────────────────────┘  ║
║                                                                  ║
║  ┌─ {Plugin-Provided Connection Title} ─────────────────────┐  ║
║  │  [Dynamically generated controls based on plugin]        │  ║
║  │  [ Test Connection ]                                      │  ║
║  └────────────────────────────────────────────────────────────┘  ║
║                                                                  ║
║  ┌─ {Plugin-Provided Mappings Title} ────────────────────────┐  ║
║  │  [Cue list selector if plugin supports refresh]          │  ║
║  │  [Mappings ListView]                                      │  ║
║  │  [ Add ]  [ Edit ]  [ Delete ]  [ Clear All ]            │  ║
║  └────────────────────────────────────────────────────────────┘  ║
╚══════════════════════════════════════════════════════════════════╝
```

---

## Game Event Integration

### Event Trigger Points

| Game Event | Description | Source |
|-----------|-------------|--------|
| `LightsDown` | Generic lights down (Q1-5) | `btnLightsDown_Click` |
| `LightsDown_Q6` - `LightsDown_Q15` | Question-specific | `btnLightsDown_Click` + question # |
| `QuestionReveal` | Question displayed | `DisplayQuestion()` |
| `AnswerReveal_A/B/C/D` | Answer revealed | Answer reveal logic |
| `CorrectAnswer` / `WrongAnswer` | Answer result | `RevealAnswer()` |
| `Lifeline_FiftyFifty` | 50:50 activated | `ExecuteLifeline()` |
| `Lifeline_PhoneFriend` | Phone-a-Friend | `ExecuteLifeline()` |
| `Lifeline_AskAudience` | Ask Audience | `ExecuteLifeline()` |
| `Lifeline_SwitchQuestion` | Switch Question | `ExecuteLifeline()` |
| `FinalAnswerLocked` | Final answer confirmed | `btnFinalAnswer_Click` |
| `GameStart` / `GameEnd_Win` / `GameEnd_Lose` | Game lifecycle | Game initialization/completion |
| `Milestone_1000` / `Milestone_32000` / `Milestone_1000000` | Prize milestones | Milestone logic |

### Integration in ControlPanelForm

```csharp
private readonly LightingManager? _lightingManager;

// In constructor:
if (settings.Lighting.Enabled)
{
    _lightingManager = new LightingManager(lightingRepository);
    if (settings.Lighting.AutoConnect)
    {
        _ = _lightingManager.SetActivePluginAsync(settings.Lighting.SelectedSystemName);
    }
}

// In event handlers:
private async void btnLightsDown_Click(object? sender, EventArgs e)
{
    // Existing game logic...
    
    // Trigger lighting
    var questionNumber = _gameService.CurrentQuestionNumber;
    var eventName = questionNumber >= 6 ? $"LightsDown_Q{questionNumber}" : "LightsDown";
    await _lightingManager?.TriggerGameEventAsync(eventName);
}
```

---

## Implementation Phases

### Phase 1: Core Plugin System (6-8 hours)

**Files to Create:**
- `src/MillionaireGame.Core/Lighting/ILightingPlugin.cs`
- `src/MillionaireGame.Core/Lighting/LightingManager.cs`
- `src/MillionaireGame.Core/Lighting/LightingPluginMetadata.cs`
- `src/MillionaireGame.Core/Lighting/LightingSystemCapabilities.cs`
- `src/MillionaireGame.Core/Lighting/LightingPluginConfigUI.cs`
- `src/MillionaireGame.Core/Lighting/LightingCueInfo.cs`
- `src/MillionaireGame.Core/Lighting/LightingCommandResult.cs`
- `src/MillionaireGame.Core/Lighting/LightingConnectionSettings.cs`
- `src/MillionaireGame.Core/Lighting/LightingCueMapping.cs`

**Tasks:**
1. Define `ILightingPlugin` interface
2. Implement `LightingManager` with plugin discovery
3. Create all data model classes
4. Implement plugin loading from `lib/dlls/`
5. Add logging via `GameConsole`

### Phase 2: Database Schema & Repository (3-4 hours)

**Files to Create:**
- `src/MillionaireGame.Core/Database/LightingSettingsRepository.cs`
- `lib/sql/lighting_schema.sql`

**Tasks:**
1. Create SQL schema migration script
2. Implement repository CRUD operations
3. Add validation logic
4. Create default data initialization

### Phase 3: Settings UI (6-8 hours)

**Files to Modify:**
- `src/MillionaireGame/Forms/Options/OptionsDialog.Designer.cs`
- `src/MillionaireGame/Forms/Options/OptionsDialog.cs`

**Tasks:**
1. Add Lighting tab to OptionsDialog
2. Implement dynamic control generation based on `GetConfigurationUI()`
3. Create mapping ListView and Add/Edit dialogs
4. Implement Load/Save logic with database
5. Add plugin dropdown population

### Phase 4: Game Integration (4-5 hours)

**Files to Modify:**
- `src/MillionaireGame/Forms/ControlPanelForm.cs`

**Tasks:**
1. Add `LightingManager` instance
2. Integrate event triggers at all game event points
3. Add connection status indicator
4. Implement cleanup/disposal

---

## Plugin Development Guide

### Creating a New Plugin

1. **Create new Class Library project**
   ```
   Project: MillionaireGame.Plugins.{YourConsole}
   Target: .NET 8.0
   Output: lib/dlls/MillionaireGame.Plugins.{YourConsole}.dll
   ```

2. **Reference Core**
   ```xml
   <ProjectReference Include="..\..\MillionaireGame.Core\MillionaireGame.Core.csproj" />
   ```

3. **Implement ILightingPlugin**
   ```csharp
   public class YourConsolePlugin : ILightingPlugin
   {
       public LightingPluginMetadata Metadata => new()
       {
           SystemName = "YourConsole",
           DisplayName = "Your Console Name",
           Manufacturer = "Manufacturer",
           Version = "1.0.0",
           SupportedProtocols = new List<string> { "OSC", "sACN" }
       };
       
       // Implement all interface members...
   }
   ```

4. **Define Configuration UI**
   ```csharp
   public LightingPluginConfigUI GetConfigurationUI()
   {
       return new LightingPluginConfigUI
       {
           ConnectionGroupTitle = "Your Console Connection",
           ConnectionSettings = new List<ConfigSettingDefinition>
           {
               // Define your settings...
           }
       };
   }
   ```

5. **Build and deploy to `lib/dlls/`**

---

## Testing Strategy

### Unit Tests
- Plugin discovery and loading
- Metadata parsing
- Database CRUD operations
- Settings validation

### Integration Tests
- Plugin lifecycle (load, connect, disconnect, unload)
- Command routing through LightingManager
- Database transactions
- Dynamic UI generation

### Manual Testing
- Install new plugin DLL → appears in dropdown
- Select plugin → settings UI generates correctly
- Configure connection → saves to database
- Trigger game event → routes to plugin
- Disable plugin → commands stop

---

## Success Criteria

- [ ] Plugin DLLs discovered automatically from `lib/dlls/`
- [ ] Plugin metadata displayed in settings dropdown
- [ ] Dynamic UI generated based on `GetConfigurationUI()`
- [ ] Settings persist to database correctly
- [ ] Game events route to active plugin
- [ ] Multiple plugins can coexist without conflicts
- [ ] Plugin load/unload without memory leaks
- [ ] Connection status updates in real-time

---

## Future Enhancements

1. **Plugin Marketplace** - Download additional console plugins
2. **Plugin API Versioning** - Manage breaking changes
3. **Plugin Sandboxing** - Security isolation
4. **Plugin Hot-Reload** - Update without restart
5. **Plugin Telemetry** - Usage analytics
6. **Multi-Plugin Support** - Control multiple consoles simultaneously

---

## References

- [Plugin Architecture Pattern](https://en.wikipedia.org/wiki/Plug-in_(computing))
- [.NET Assembly Loading](https://learn.microsoft.com/en-us/dotnet/core/dependency-loading/)
- [Dependency Injection in Plugins](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
