# Unified Plugin Manager Architecture

**Status**: 📋 Planning  
**Priority**: MEDIUM - Optional optimization for shared plugin infrastructure  
**Target Release**: v1.2.0  
**Created**: January 10, 2026  
**Last Updated**: January 10, 2026

---

## Executive Summary

This document outlines an **optional unified plugin manager** that consolidates common plugin management functionality shared between `LightingManager` and `SoundManager`. This is an architectural enhancement to reduce code duplication while maintaining separation of concerns for domain-specific logic.

**Key Decision**: This is **NOT required for v1.1.0**. Start with separate managers (`LightingManager` and `SoundManager`) and consider this refactoring for v1.2.0 if duplication becomes problematic.

---

## Current Architecture (v1.1.0)

### Separate Managers Approach

```
MillionaireGame.Core/
├── Lighting/
│   ├── ILightingPlugin.cs
│   ├── LightingManager.cs              ← Lighting-specific manager
│   └── [lighting models...]
└── Sound/
    ├── ISoundPlugin.cs
    ├── SoundManager.cs                 ← Sound-specific manager
    └── [sound models...]
```

**Pros:**
- ✅ Simple to implement initially
- ✅ Clear separation of concerns
- ✅ No abstraction overhead
- ✅ Easy to understand and maintain

**Cons:**
- ❌ Duplicate plugin discovery code
- ❌ Duplicate plugin loading logic
- ❌ Duplicate connection management patterns

---

## Proposed Unified Architecture (v1.2.0)

### Shared Base Manager

```
MillionaireGame.Core/
├── Plugins/
│   ├── IPlugin.cs                      ← Base plugin interface
│   ├── PluginManager<T>.cs             ← Generic plugin manager
│   ├── PluginMetadata.cs               ← Shared metadata model
│   └── PluginDiscovery.cs              ← Plugin discovery service
├── Lighting/
│   ├── ILightingPlugin.cs              ← Extends IPlugin
│   ├── LightingManager.cs              ← Extends PluginManager<ILightingPlugin>
│   └── [lighting models...]
└── Sound/
    ├── ISoundPlugin.cs                 ← Extends IPlugin
    ├── SoundManager.cs                 ← Extends PluginManager<ISoundPlugin>
    └── [sound models...]
```

---

## Base Plugin Interface

### IPlugin

**Location**: `src/MillionaireGame.Core/Plugins/IPlugin.cs`

```csharp
namespace MillionaireGame.Core.Plugins
{
    /// <summary>
    /// Base interface for all plugins in the Millionaire Game ecosystem.
    /// </summary>
    public interface IPlugin : IDisposable
    {
        /// <summary>
        /// Plugin metadata for discovery and identification.
        /// </summary>
        PluginMetadata Metadata { get; }
        
        /// <summary>
        /// Connect to the external system.
        /// </summary>
        Task<bool> ConnectAsync(IConnectionSettings settings);
        
        /// <summary>
        /// Disconnect from the external system.
        /// </summary>
        void Disconnect();
        
        /// <summary>
        /// Current connection status.
        /// </summary>
        bool IsConnected { get; }
        
        /// <summary>
        /// Raised when connection status changes.
        /// </summary>
        event EventHandler<ConnectionStatusChangedEventArgs>? ConnectionStatusChanged;
        
        /// <summary>
        /// Validate connection settings before attempting connection.
        /// </summary>
        bool ValidateSettings(IConnectionSettings settings, out string errorMessage);
        
        /// <summary>
        /// Get dynamic configuration UI metadata.
        /// </summary>
        PluginConfigUI GetConfigurationUI();
    }
}
```

### PluginMetadata

```csharp
namespace MillionaireGame.Core.Plugins
{
    public class PluginMetadata
    {
        /// <summary>
        /// Plugin category (e.g., "LightingControl", "SoundControl", "VideoControl").
        /// Used for filtering during discovery.
        /// </summary>
        public string PluginType { get; set; } = "";
        
        /// <summary>
        /// Unique system identifier (e.g., "ETC_Ion_Classic", "Yamaha_TF_Rack").
        /// </summary>
        public string SystemName { get; set; } = "";
        
        /// <summary>
        /// User-friendly display name.
        /// </summary>
        public string DisplayName { get; set; } = "";
        
        /// <summary>
        /// Manufacturer or vendor name.
        /// </summary>
        public string Manufacturer { get; set; } = "";
        
        /// <summary>
        /// Plugin version (SemVer recommended).
        /// </summary>
        public string Version { get; set; } = "";
        
        /// <summary>
        /// Plugin developer/author.
        /// </summary>
        public string Author { get; set; } = "";
        
        /// <summary>
        /// Short description of plugin functionality.
        /// </summary>
        public string Description { get; set; } = "";
        
        /// <summary>
        /// Supported protocols (e.g., "OSC", "RCP", "MIDI").
        /// </summary>
        public List<string> SupportedProtocols { get; set; } = new();
        
        /// <summary>
        /// Resource path to plugin icon (optional).
        /// </summary>
        public string IconResourcePath { get; set; } = "";
        
        /// <summary>
        /// URL to plugin documentation or help.
        /// </summary>
        public string HelpUrl { get; set; } = "";
        
        /// <summary>
        /// Minimum API version required.
        /// </summary>
        public string MinApiVersion { get; set; } = "1.0.0";
    }
}
```

---

## Plugin Discovery Service

### PluginDiscovery

**Location**: `src/MillionaireGame.Core/Plugins/PluginDiscovery.cs`

```csharp
namespace MillionaireGame.Core.Plugins
{
    /// <summary>
    /// Service for discovering and loading plugins from lib/dlls directory.
    /// </summary>
    public class PluginDiscovery
    {
        private readonly string _pluginDirectory;
        private readonly Dictionary<string, Type> _discoveredPlugins;
        
        public PluginDiscovery(string pluginDirectory = "lib/dlls")
        {
            _pluginDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, pluginDirectory);
            _discoveredPlugins = new Dictionary<string, Type>();
        }
        
        /// <summary>
        /// Scan plugin directory for assemblies implementing IPlugin.
        /// </summary>
        /// <param name="pluginType">Filter by plugin type (e.g., "LightingControl", "SoundControl")</param>
        public List<PluginMetadata> DiscoverPlugins(string? pluginType = null)
        {
            var metadata = new List<PluginMetadata>();
            
            if (!Directory.Exists(_pluginDirectory))
            {
                GameConsole.Warn($"Plugin directory not found: {_pluginDirectory}");
                return metadata;
            }
            
            var dllFiles = Directory.GetFiles(_pluginDirectory, "*.dll", SearchOption.TopDirectoryOnly);
            
            foreach (var dllPath in dllFiles)
            {
                try
                {
                    var assembly = Assembly.LoadFrom(dllPath);
                    var pluginTypes = assembly.GetTypes()
                        .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
                    
                    foreach (var type in pluginTypes)
                    {
                        // Instantiate temporarily to get metadata
                        var instance = (IPlugin)Activator.CreateInstance(type)!;
                        var pluginMetadata = instance.Metadata;
                        instance.Dispose();
                        
                        // Filter by plugin type if specified
                        if (pluginType == null || pluginMetadata.PluginType == pluginType)
                        {
                            metadata.Add(pluginMetadata);
                            _discoveredPlugins[pluginMetadata.SystemName] = type;
                            GameConsole.Debug($"Discovered plugin: {pluginMetadata.DisplayName} ({pluginMetadata.SystemName})");
                        }
                    }
                }
                catch (Exception ex)
                {
                    GameConsole.Error($"Failed to load plugin from {dllPath}: {ex.Message}");
                }
            }
            
            return metadata;
        }
        
        /// <summary>
        /// Instantiate a plugin by system name.
        /// </summary>
        public IPlugin? InstantiatePlugin(string systemName)
        {
            if (!_discoveredPlugins.TryGetValue(systemName, out var type))
            {
                GameConsole.Warn($"Plugin not found: {systemName}");
                return null;
            }
            
            try
            {
                var instance = (IPlugin)Activator.CreateInstance(type)!;
                GameConsole.Info($"Instantiated plugin: {instance.Metadata.DisplayName}");
                return instance;
            }
            catch (Exception ex)
            {
                GameConsole.Error($"Failed to instantiate plugin {systemName}: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Get all discovered plugin types.
        /// </summary>
        public Dictionary<string, Type> GetDiscoveredTypes() => _discoveredPlugins;
    }
}
```

---

## Generic Plugin Manager

### PluginManager<T>

**Location**: `src/MillionaireGame.Core/Plugins/PluginManager.cs`

```csharp
namespace MillionaireGame.Core.Plugins
{
    /// <summary>
    /// Generic base class for managing plugins of a specific type.
    /// </summary>
    /// <typeparam name="T">Plugin interface type (e.g., ILightingPlugin, ISoundPlugin)</typeparam>
    public abstract class PluginManager<T> : IDisposable where T : class, IPlugin
    {
        protected readonly PluginDiscovery _discovery;
        protected readonly Dictionary<string, T> _loadedPlugins;
        protected T? _activePlugin;
        protected bool _disposed;
        
        /// <summary>
        /// Plugin type filter for discovery (e.g., "LightingControl", "SoundControl").
        /// </summary>
        protected abstract string PluginTypeFilter { get; }
        
        public event EventHandler<PluginStatusChangedEventArgs>? PluginStatusChanged;
        
        protected PluginManager()
        {
            _discovery = new PluginDiscovery();
            _loadedPlugins = new Dictionary<string, T>();
            DiscoverPlugins();
        }
        
        /// <summary>
        /// Discover plugins of the specific type.
        /// </summary>
        protected void DiscoverPlugins()
        {
            var plugins = _discovery.DiscoverPlugins(PluginTypeFilter);
            GameConsole.Info($"Discovered {plugins.Count} {PluginTypeFilter} plugin(s)");
        }
        
        /// <summary>
        /// Get list of available plugins.
        /// </summary>
        public List<PluginMetadata> GetAvailablePlugins()
        {
            return _discovery.DiscoverPlugins(PluginTypeFilter);
        }
        
        /// <summary>
        /// Load a plugin by system name.
        /// </summary>
        public T? LoadPlugin(string systemName)
        {
            if (_loadedPlugins.TryGetValue(systemName, out var existingPlugin))
            {
                return existingPlugin;
            }
            
            var plugin = _discovery.InstantiatePlugin(systemName) as T;
            if (plugin != null)
            {
                _loadedPlugins[systemName] = plugin;
                plugin.ConnectionStatusChanged += OnPluginConnectionStatusChanged;
                GameConsole.Info($"Loaded {PluginTypeFilter} plugin: {plugin.Metadata.DisplayName}");
            }
            
            return plugin;
        }
        
        /// <summary>
        /// Unload a plugin.
        /// </summary>
        public void UnloadPlugin(string systemName)
        {
            if (_loadedPlugins.TryGetValue(systemName, out var plugin))
            {
                plugin.ConnectionStatusChanged -= OnPluginConnectionStatusChanged;
                plugin.Disconnect();
                plugin.Dispose();
                _loadedPlugins.Remove(systemName);
                GameConsole.Info($"Unloaded {PluginTypeFilter} plugin: {systemName}");
            }
        }
        
        /// <summary>
        /// Set the active plugin.
        /// </summary>
        public async Task<bool> SetActivePluginAsync(string systemName)
        {
            if (_activePlugin?.Metadata.SystemName == systemName)
            {
                return true; // Already active
            }
            
            // Disconnect current plugin
            if (_activePlugin != null)
            {
                _activePlugin.Disconnect();
                GameConsole.Info($"Disconnected previous {PluginTypeFilter} plugin");
            }
            
            // Load and set new plugin
            var plugin = LoadPlugin(systemName);
            if (plugin == null)
            {
                return false;
            }
            
            _activePlugin = plugin;
            PluginStatusChanged?.Invoke(this, new PluginStatusChangedEventArgs(systemName, true));
            GameConsole.Info($"Active {PluginTypeFilter} plugin set to: {plugin.Metadata.DisplayName}");
            return true;
        }
        
        /// <summary>
        /// Get the currently active plugin.
        /// </summary>
        public T? GetActivePlugin() => _activePlugin;
        
        /// <summary>
        /// Check if a plugin is currently active.
        /// </summary>
        public bool HasActivePlugin => _activePlugin != null;
        
        /// <summary>
        /// Check if the active plugin is connected.
        /// </summary>
        public bool IsActivePluginConnected => _activePlugin?.IsConnected ?? false;
        
        /// <summary>
        /// Handle plugin connection status changes.
        /// </summary>
        protected virtual void OnPluginConnectionStatusChanged(object? sender, ConnectionStatusChangedEventArgs e)
        {
            GameConsole.Info($"{PluginTypeFilter} plugin connection status: {(e.IsConnected ? "Connected" : "Disconnected")}");
        }
        
        public virtual void Dispose()
        {
            if (!_disposed)
            {
                _activePlugin?.Disconnect();
                _activePlugin?.Dispose();
                
                foreach (var plugin in _loadedPlugins.Values)
                {
                    plugin.Dispose();
                }
                
                _loadedPlugins.Clear();
                _disposed = true;
            }
        }
    }
    
    public class PluginStatusChangedEventArgs : EventArgs
    {
        public string SystemName { get; }
        public bool IsActive { get; }
        
        public PluginStatusChangedEventArgs(string systemName, bool isActive)
        {
            SystemName = systemName;
            IsActive = isActive;
        }
    }
}
```

---

## Updated Domain-Specific Managers

### LightingManager (Refactored)

```csharp
namespace MillionaireGame.Core.Lighting
{
    public class LightingManager : PluginManager<ILightingPlugin>
    {
        private readonly LightingSettingsRepository _repository;
        
        protected override string PluginTypeFilter => "LightingControl";
        
        public event EventHandler<LightingEventArgs>? LightingEventTriggered;
        
        public LightingManager(LightingSettingsRepository repository) : base()
        {
            _repository = repository;
        }
        
        // Lighting-specific command routing
        public async Task TriggerGameEventAsync(string gameEvent)
        {
            if (_activePlugin == null || !_activePlugin.IsConnected)
            {
                GameConsole.Warn($"Cannot trigger lighting event '{gameEvent}': No active plugin");
                return;
            }
            
            // Look up cue mapping in database
            var mapping = await _repository.GetCueMappingAsync(gameEvent);
            if (mapping == null)
            {
                GameConsole.Debug($"No lighting cue mapped for event: {gameEvent}");
                return;
            }
            
            // Execute command based on mapping type
            switch (mapping.CommandType)
            {
                case LightingCommandType.FireCue:
                    await _activePlugin.TriggerCueAsync(mapping.CueIdentifier);
                    break;
                case LightingCommandType.FireMacro:
                    await _activePlugin.TriggerMacroAsync(mapping.CueIdentifier);
                    break;
                case LightingCommandType.Go:
                    await _activePlugin.GoAsync();
                    break;
                case LightingCommandType.Stop:
                    await _activePlugin.StopAsync();
                    break;
            }
            
            LightingEventTriggered?.Invoke(this, new LightingEventArgs(gameEvent, mapping));
        }
        
        public async Task EmergencyStopAsync()
        {
            if (_activePlugin?.IsConnected == true)
            {
                await _activePlugin.StopAsync();
            }
        }
    }
}
```

### SoundManager (Refactored)

```csharp
namespace MillionaireGame.Core.Sound
{
    public class SoundManager : PluginManager<ISoundPlugin>
    {
        private readonly SoundSettingsRepository _repository;
        
        protected override string PluginTypeFilter => "SoundControl";
        
        public event EventHandler<SoundEventArgs>? SoundEventTriggered;
        public event EventHandler<ChannelStateChangedEventArgs>? ChannelStateChanged;
        
        public SoundManager(SoundSettingsRepository repository) : base()
        {
            _repository = repository;
        }
        
        // Sound-specific command routing
        public async Task TriggerGameEventAsync(string gameEvent)
        {
            if (_activePlugin == null || !_activePlugin.IsConnected)
            {
                GameConsole.Warn($"Cannot trigger sound event '{gameEvent}': No active plugin");
                return;
            }
            
            // Look up scene mapping in database
            var mapping = await _repository.GetSceneMappingAsync(gameEvent);
            if (mapping == null)
            {
                GameConsole.Debug($"No sound scene mapped for event: {gameEvent}");
                return;
            }
            
            // Delay if specified
            if (mapping.DelayMs > 0)
            {
                await Task.Delay(mapping.DelayMs);
            }
            
            await _activePlugin.RecallSceneAsync(mapping.SceneNumber);
            SoundEventTriggered?.Invoke(this, new SoundEventArgs(gameEvent, mapping));
        }
        
        // Direct channel control methods
        public async Task SetChannelLevelAsync(int channel, float levelDb)
        {
            if (_activePlugin?.IsConnected == true)
            {
                await _activePlugin.SetChannelLevelAsync(channel, levelDb);
            }
        }
        
        public async Task MuteChannelAsync(int channel)
        {
            if (_activePlugin?.IsConnected == true)
            {
                await _activePlugin.SetChannelMuteAsync(channel, true);
            }
        }
        
        public async Task EmergencyMuteAllAsync()
        {
            if (_activePlugin?.IsConnected == true)
            {
                var capabilities = _activePlugin.GetCapabilities();
                var channels = Enumerable.Range(1, capabilities.MaxInputChannels).ToList();
                await _activePlugin.MuteAllChannelsAsync(channels);
            }
        }
    }
}
```

---

## Updated Plugin Interfaces

### ILightingPlugin (Updated)

```csharp
namespace MillionaireGame.Core.Lighting
{
    public interface ILightingPlugin : IPlugin
    {
        // Lighting-specific capabilities
        LightingSystemCapabilities GetCapabilities();
        
        // Lighting-specific commands
        Task<LightingCommandResult> TriggerCueAsync(string cueIdentifier);
        Task<LightingCommandResult> TriggerMacroAsync(string macroIdentifier);
        Task<LightingCommandResult> GoAsync(string? cueListId = null);
        Task<LightingCommandResult> StopAsync(string? cueListId = null);
        Task<LightingCommandResult> SendCustomCommandAsync(string command);
        
        // Cue list retrieval
        Task<List<LightingCueInfo>> GetCueListAsync(string cueListId);
        Task<int> GetCueCountAsync(string cueListId);
        
        // Note: ConnectAsync, Disconnect, IsConnected inherited from IPlugin
    }
}
```

### ISoundPlugin (Updated)

```csharp
namespace MillionaireGame.Core.Sound
{
    public interface ISoundPlugin : IPlugin
    {
        // Sound-specific capabilities
        MixerCapabilities GetCapabilities();
        
        // Channel control
        Task<SoundCommandResult> SetChannelLevelAsync(int channel, float levelDb);
        Task<SoundCommandResult> SetChannelMuteAsync(int channel, bool muted);
        Task<SoundCommandResult> SetChannelLabelAsync(int channel, string label);
        Task<float> GetChannelLevelAsync(int channel);
        Task<bool> GetChannelMuteAsync(int channel);
        Task<string> GetChannelLabelAsync(int channel);
        
        // Bulk operations
        Task<SoundCommandResult> SetMultipleChannelLevelsAsync(Dictionary<int, float> channelLevels);
        Task<SoundCommandResult> MuteAllChannelsAsync(List<int> channels);
        Task<SoundCommandResult> UnmuteAllChannelsAsync(List<int> channels);
        
        // Scene management
        Task<SoundCommandResult> RecallSceneAsync(int sceneNumber);
        Task<SoundCommandResult> StoreSceneAsync(int sceneNumber, string? sceneName = null);
        Task<List<SceneInfo>> GetSceneListAsync();
        
        // Channel discovery
        Task<List<ChannelInfo>> GetChannelListAsync();
        Task<int> GetChannelCountAsync();
        
        // Custom commands
        Task<SoundCommandResult> SendCustomCommandAsync(string command);
        
        // Events
        event EventHandler<ChannelStateChangedEventArgs>? ChannelStateChanged;
        event EventHandler<SoundCommandResultEventArgs>? CommandResultReceived;
        
        // Note: ConnectAsync, Disconnect, IsConnected inherited from IPlugin
    }
}
```

---

## Migration Strategy

### Phase 1: Create Base Infrastructure (v1.2.0)

**Week 1-2: Base Classes**
1. Create `IPlugin` interface
2. Create `PluginMetadata` class
3. Create `PluginDiscovery` service
4. Create `PluginManager<T>` abstract class

### Phase 2: Refactor Lighting (v1.2.0)

**Week 3: Lighting Migration**
1. Update `ILightingPlugin` to extend `IPlugin`
2. Refactor `LightingManager` to extend `PluginManager<ILightingPlugin>`
3. Update existing lighting plugins (ETC Ion)
4. Test lighting functionality

### Phase 3: Refactor Sound (v1.2.0)

**Week 4: Sound Migration**
1. Update `ISoundPlugin` to extend `IPlugin`
2. Refactor `SoundManager` to extend `PluginManager<ISoundPlugin>`
3. Update Yamaha plugin
4. Test sound functionality

### Phase 4: Cleanup & Testing (v1.2.0)

**Week 5: Final Migration**
1. Remove duplicated code
2. Update documentation
3. Integration testing
4. Performance testing

---

## Benefits of Unified Architecture

### Code Reusability
- ✅ Shared plugin discovery logic
- ✅ Shared plugin loading/unloading
- ✅ Shared connection management patterns
- ✅ Consistent error handling

### Maintainability
- ✅ Single source of truth for plugin lifecycle
- ✅ Easier to add new plugin types (Video, DMX, etc.)
- ✅ Consistent logging patterns
- ✅ Reduced code duplication

### Extensibility
- ✅ Easy to add new plugin categories (Video Control, Broadcast Control, etc.)
- ✅ Consistent plugin development experience
- ✅ Shared plugin utilities and helpers

---

## Risks & Considerations

### Abstraction Complexity
- ⚠️ Additional layer of abstraction may be harder to understand initially
- ⚠️ Generic constraints may limit flexibility
- **Mitigation**: Comprehensive documentation and examples

### Migration Effort
- ⚠️ Requires refactoring existing working code
- ⚠️ Risk of introducing bugs during migration
- **Mitigation**: Incremental migration with thorough testing

### Performance Overhead
- ⚠️ Generic method calls may have minimal overhead
- ⚠️ Additional abstraction layers
- **Mitigation**: Profile before/after, overhead should be negligible

---

## Decision Matrix

| Criterion | Separate Managers | Unified Manager |
|-----------|------------------|-----------------|
| **Initial Development Speed** | ✅ Fast | ❌ Slower |
| **Code Duplication** | ❌ High | ✅ Low |
| **Maintainability** | ⚠️ Medium | ✅ High |
| **Extensibility** | ⚠️ Medium | ✅ High |
| **Complexity** | ✅ Low | ⚠️ Medium |
| **Type Safety** | ✅ High | ✅ High (generics) |
| **Learning Curve** | ✅ Low | ⚠️ Medium |

---

## Recommendation

**For v1.1.0**: Use **Separate Managers** approach
- Faster initial implementation
- Simpler to understand and maintain
- Allows learning from real-world usage

**For v1.2.0**: Consider **Unified Manager** refactoring IF:
- Code duplication becomes problematic
- Adding 3+ additional plugin types
- Team comfortable with generics and abstraction
- Performance testing shows no significant overhead

---

## Alternative: Hybrid Approach

Keep separate managers but extract common utilities:

```
MillionaireGame.Core/
├── Plugins/
│   ├── PluginDiscoveryService.cs       ← Shared discovery
│   ├── PluginMetadataHelpers.cs        ← Shared metadata utilities
│   └── PluginConnectionHelpers.cs      ← Shared connection patterns
├── Lighting/
│   ├── ILightingPlugin.cs
│   ├── LightingManager.cs              ← Uses shared utilities
│   └── [lighting models...]
└── Sound/
    ├── ISoundPlugin.cs
    ├── SoundManager.cs                 ← Uses shared utilities
    └── [sound models...]
```

**Benefits:**
- ✅ Reduces duplication without heavy abstraction
- ✅ Easier migration path
- ✅ Lower risk

---

## Success Criteria

- [ ] Plugin discovery works for all plugin types
- [ ] No code duplication in plugin lifecycle management
- [ ] Lighting functionality unchanged after refactoring
- [ ] Sound functionality unchanged after refactoring
- [ ] No performance degradation
- [ ] Clear documentation for plugin developers
- [ ] Existing plugins work without modification (backward compatible)

---

## References

- **Sound Plugin Architecture**: `src/docs/active/SOUND_PLUGIN_ARCHITECTURE.md`
- **Lighting Plugin Architecture**: `src/docs/active/LIGHTING_PLUGIN_ARCHITECTURE.md`
- [Generic Classes (C#)](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/generics)
- [Plugin Architecture Best Practices](https://en.wikipedia.org/wiki/Plug-in_(computing))

---

## Summary

A unified plugin manager provides long-term maintainability benefits but adds initial complexity. **Recommend starting with separate managers for v1.1.0** to get the plugin architecture working, then **evaluate refactoring to unified manager for v1.2.0** based on real-world usage and team feedback.

The hybrid approach (shared utilities without full unification) may provide the best balance of reduced duplication without excessive abstraction.
