using NAudio.Wave;

namespace MillionaireGame.Services;

/// <summary>
/// Manages game audio playback using NAudio
/// </summary>
public class SoundService : IDisposable
{
    private readonly Dictionary<SoundEffect, string> _soundPaths = new();
    private readonly Dictionary<string, IWavePlayer> _activePlayers = new();
    private readonly SoundPackManager _soundPackManager;
    private readonly object _lock = new();
    private bool _soundEnabled = true;
    private bool _disposed = false;
    private static readonly string _baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

    public SoundService()
    {
        _soundPackManager = new SoundPackManager();
    }

    public bool SoundEnabled
    {
        get => _soundEnabled;
        set => _soundEnabled = value;
    }

    /// <summary>
    /// Convert absolute path to relative path from executable directory for logging
    /// </summary>
    private static string GetRelativePath(string absolutePath)
    {
        if (string.IsNullOrEmpty(absolutePath))
            return absolutePath;

        try
        {
            if (absolutePath.StartsWith(_baseDirectory, StringComparison.OrdinalIgnoreCase))
            {
                return absolutePath.Substring(_baseDirectory.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }
        }
        catch
        {
            // If anything fails, return the original path
        }

        return absolutePath;
    }

    /// <summary>
    /// Gets the sound pack manager for managing sound packs
    /// </summary>
    public SoundPackManager GetSoundPackManager() => _soundPackManager;

    /// <summary>
    /// Resolve a file path - handles both absolute paths and relative paths from application directory
    /// </summary>
    private string ResolvePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return filePath;

        // If absolute path and exists, return as-is
        if (Path.IsPathRooted(filePath) && File.Exists(filePath))
            return filePath;

        // Try relative to application directory
        var appDir = AppDomain.CurrentDomain.BaseDirectory;
        var relativePath = Path.Combine(appDir, filePath);
        if (File.Exists(relativePath))
            return relativePath;

        // Try in lib/sounds directory
        var soundsPath = Path.Combine(appDir, "lib", "sounds", Path.GetFileName(filePath));
        if (File.Exists(soundsPath))
            return soundsPath;

        // Return original path
        return filePath;
    }

    /// <summary>
    /// Register a sound file path for a specific sound effect
    /// </summary>
    public void RegisterSound(SoundEffect effect, string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return;

        var resolvedPath = ResolvePath(filePath);
        if (File.Exists(resolvedPath))
        {
            _soundPaths[effect] = resolvedPath;
            if (Program.DebugMode)
            {
                Console.WriteLine($"[Sound] Registered {effect}: {GetRelativePath(resolvedPath)}");
            }
        }
        else
        {
            if (Program.DebugMode)
            {
                Console.WriteLine($"[Sound] Warning: File not found for {effect}: {GetRelativePath(filePath)}");
            }
        }
    }

    /// <summary>
    /// Play a sound effect
    /// </summary>
    public void PlaySound(SoundEffect effect, bool loop = false)
    {
        if (!_soundEnabled)
        {
            if (Program.DebugMode)
            {
                Console.WriteLine($"[Sound] Playback disabled for {effect}");
            }
            return;
        }

        if (_soundPaths.TryGetValue(effect, out var filePath))
        {
            var identifier = loop ? effect.ToString() : Guid.NewGuid().ToString();
            Task.Run(() => PlaySoundFile(filePath, identifier, loop));
        }
        else
        {
            if (Program.DebugMode)
            {
                Console.WriteLine($"[Sound] No path registered for {effect}");
            }
        }
    }

    /// <summary>
    /// Play a sound effect asynchronously
    /// </summary>
    public async Task PlaySoundAsync(SoundEffect effect, bool loop = false)
    {
        if (!_soundEnabled) return;

        if (_soundPaths.TryGetValue(effect, out var filePath))
        {
            var identifier = loop ? effect.ToString() : Guid.NewGuid().ToString();
            await Task.Run(() => PlaySoundFile(filePath, identifier, loop));
        }
    }

    /// <summary>
    /// Play a sound by its key name from the sound pack
    /// </summary>
    public void PlaySoundByKey(string key, bool loop = false)
    {
        if (!_soundEnabled)
        {
            if (Program.DebugMode)
            {
                Console.WriteLine($"[Sound] Playback disabled for key {key}");
            }
            return;
        }

        var filePath = _soundPackManager.GetSoundFile(key);
        if (!string.IsNullOrEmpty(filePath))
        {
            var identifier = loop ? key : Guid.NewGuid().ToString();
            Task.Run(() => PlaySoundFile(filePath, identifier, loop));
        }
        else
        {
            if (Program.DebugMode)
            {
                Console.WriteLine($"[Sound] No file found for key: {key}");
            }
        }
    }

    /// <summary>
    /// Play a sound by its key name and return the identifier for later stopping
    /// </summary>
    public string PlaySoundByKeyWithIdentifier(string key, bool loop = false)
    {
        if (!_soundEnabled)
        {
            if (Program.DebugMode)
            {
                Console.WriteLine($"[Sound] Playback disabled for key {key}");
            }
            return string.Empty;
        }

        var filePath = _soundPackManager.GetSoundFile(key);
        if (!string.IsNullOrEmpty(filePath))
        {
            var identifier = loop ? key : Guid.NewGuid().ToString();
            Task.Run(() => PlaySoundFile(filePath, identifier, loop));
            return identifier;
        }
        else
        {
            if (Program.DebugMode)
            {
                Console.WriteLine($"[Sound] No file found for key: {key}");
            }
            return string.Empty;
        }
    }

    /// <summary>
    /// Play a sound by its key name asynchronously
    /// </summary>
    public async Task PlaySoundByKeyAsync(string key, bool loop = false)
    {
        if (!_soundEnabled) return;

        var filePath = _soundPackManager.GetSoundFile(key);
        if (!string.IsNullOrEmpty(filePath))
        {
            var identifier = loop ? key : Guid.NewGuid().ToString();
            await Task.Run(() => PlaySoundFile(filePath, identifier, loop));
        }
    }

    /// <summary>
    /// Play a sound effect with a specific identifier
    /// </summary>
    public void PlaySound(SoundEffect effect, string identifier, bool loop = false)
    {
        if (!_soundEnabled) return;

        if (_soundPaths.TryGetValue(effect, out var filePath))
        {
            Task.Run(() => PlaySoundFile(filePath, identifier, loop));
        }
    }

    /// <summary>
    /// Stop a specific sound by identifier
    /// </summary>
    public void StopSound(string identifier)
    {
        lock (_lock)
        {
            if (_activePlayers.TryGetValue(identifier, out var player))
            {
                player.Stop();
                player.Dispose();
                _activePlayers.Remove(identifier);
            }
        }
    }

    /// <summary>
    /// Stop all currently playing sounds
    /// </summary>
    public void StopAllSounds()
    {
        lock (_lock)
        {
            foreach (var player in _activePlayers.Values)
            {
                try
                {
                    player.Stop();
                    player.Dispose();
                }
                catch
                {
                    // Ignore disposal errors
                }
            }
            _activePlayers.Clear();
        }
    }

    /// <summary>
    /// Check if a specific sound is currently playing
    /// </summary>
    public bool IsSoundPlaying(string identifier)
    {
        lock (_lock)
        {
            return _activePlayers.ContainsKey(identifier);
        }
    }

    /// <summary>
    /// Wait for a specific sound to finish playing
    /// </summary>
    public async Task WaitForSoundAsync(string identifier, CancellationToken cancellationToken = default, int timeoutMs = 30000)
    {
        var startTime = DateTime.Now;
        while (IsSoundPlaying(identifier) && !cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(100, cancellationToken);
            
            // Safety timeout
            if ((DateTime.Now - startTime).TotalMilliseconds > timeoutMs)
            {
                if (Program.DebugMode)
                {
                    Console.WriteLine($"[Sound] Timeout waiting for {identifier}");
                }
                break;
            }
        }
    }

    /// <summary>
    /// Play a sound file directly
    /// </summary>
    public void PlaySoundFile(string filePath, string? identifier = null, bool loop = false)
    {
        var resolvedPath = ResolvePath(filePath);
        
        if (!_soundEnabled)
        {
            if (Program.DebugMode)
            {
                Console.WriteLine($"[Sound] Playback disabled");
            }
            return;
        }
        
        if (!File.Exists(resolvedPath))
        {
            if (Program.DebugMode)
            {
                Console.WriteLine($"[Sound] File not found: {GetRelativePath(filePath)} (resolved to: {GetRelativePath(resolvedPath)})");
            }
            return;
        }

        if (Program.DebugMode)
        {
            Console.WriteLine($"[Sound] Playing: {Path.GetFileName(resolvedPath)} (loop: {loop})");
        }

        try
        {
            var id = identifier ?? Guid.NewGuid().ToString();
            
            // Stop existing sound with same identifier
            if (identifier != null)
            {
                StopSound(identifier);
            }

            IWavePlayer? player = null;
            AudioFileReader? audioFile = null;

            try
            {
                // Create audio file reader
                audioFile = new AudioFileReader(resolvedPath);
                
                // Create output device
                player = new WaveOutEvent();
                player.Init(audioFile);

                // Handle playback stopped event
                player.PlaybackStopped += (s, e) =>
                {
                    lock (_lock)
                    {
                        // If looping, restart playback
                        if (loop && _activePlayers.ContainsKey(id))
                        {
                            try
                            {
                                audioFile.Position = 0;
                                player.Play();
                                if (Program.DebugMode)
                                {
                                    Console.WriteLine($"[Sound] Looping: {Path.GetFileName(resolvedPath)}");
                                }
                                return;
                            }
                            catch
                            {
                                // If restart fails, clean up
                            }
                        }
                        
                        // Clean up resources
                        if (_activePlayers.ContainsKey(id))
                        {
                            _activePlayers[id].Dispose();
                            _activePlayers.Remove(id);
                        }
                        if (Program.DebugMode)
                        {
                            Console.WriteLine($"[Sound] Stopped: {Path.GetFileName(resolvedPath)}");
                        }
                    }
                    audioFile?.Dispose();
                };

                // Store player
                lock (_lock)
                {
                    _activePlayers[id] = player;
                }

                // Start playback
                player.Play();
            }
            catch
            {
                // Cleanup on error
                audioFile?.Dispose();
                player?.Dispose();
                throw;
            }
        }
        catch (Exception ex)
        {
            // Log error but don't crash the application
            System.Diagnostics.Debug.WriteLine($"Error playing sound: {ex.Message}");
        }
    }

    /// <summary>
    /// Load sound paths from settings
    /// </summary>
    public void LoadSoundsFromSettings(Core.Settings.ApplicationSettings settings)
    {
        // Load from selected sound pack
        var packName = settings.SelectedSoundPack ?? "Default";
        var success = _soundPackManager.LoadSoundPack(packName);
        
        if (success)
        {
            if (Program.DebugMode)
            {
                Console.WriteLine($"[Sound] Loading sounds from pack: {packName}");
            }
            
            LoadSoundsFromPack(_soundPackManager.CurrentSounds);
            
            if (Program.DebugMode)
            {
                Console.WriteLine($"[Sound] Registered {_soundPaths.Count} sounds to enum values");
                Console.WriteLine($"[Sound] {_soundPackManager.CurrentSounds.Count - _soundPaths.Count} sounds available via PlaySoundByKey()");
            }
        }
        else
        {
            if (Program.DebugMode)
            {
                Console.WriteLine($"[Sound] Error: Could not load sound pack '{packName}'");
            }
        }
    }

    /// <summary>
    /// Load sounds from a sound pack dictionary
    /// </summary>
    private void LoadSoundsFromPack(IReadOnlyDictionary<string, string> soundPack)
    {
        // Helper to register sound with debug logging
        void TryRegister(SoundEffect effect, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (soundPack.TryGetValue(key, out var path) && !string.IsNullOrEmpty(path))
                {
                    RegisterSound(effect, path);
                    return; // Use first match
                }
            }
        }

        // Broadcast flow sounds
        TryRegister(SoundEffect.HostEntrance, "HostEntrance", "host_entrance");
        TryRegister(SoundEffect.ExplainGame, "ExplainRules", "explain_rules");
        TryRegister(SoundEffect.QuitSmall, "QuitSmall", "quit_small");
        TryRegister(SoundEffect.QuitLarge, "QuitLarge", "quit_large");
        TryRegister(SoundEffect.WalkAwaySmall, "WalkAwaySmall", "walk_away_small");
        TryRegister(SoundEffect.WalkAwayLarge, "WalkAwayLarge", "walk_away_large");
        TryRegister(SoundEffect.CloseTheme, "ClosingTheme", "close_theme");
        TryRegister(SoundEffect.CloseUnderscore, "CloseUnderscore", "close_underscore");
        
        // Register common game sounds (try with and without underscores, with leading zeros)
        TryRegister(SoundEffect.LightsDown, "Q01to05LightsDown", "Q1to5LightsDown", "q1_to_q5_lights_down");
        TryRegister(SoundEffect.QuestionCue, "Q01to05Bed", "Q1to5Bed", "q1_to_q5_bed");
        TryRegister(SoundEffect.FinalAnswer, "Q01Final", "Q1Final", "q1_final");
        TryRegister(SoundEffect.CorrectAnswer, "Q01to04Correct", "Q1to4Correct", "q1_to_q4_correct");
        TryRegister(SoundEffect.WrongAnswer, "Q01to05Wrong", "Q1to5Wrong", "q1_to_q5_wrong", "GameOver");
        TryRegister(SoundEffect.WalkAway, "WalkAwaySmall", "walk_away_small");
        
        // Lifelines (try multiple naming conventions)
        TryRegister(SoundEffect.Lifeline5050, "5050", "fifty_fifty", "FiftyFifty");
        TryRegister(SoundEffect.LifelinePhone, "PAFStart", "paf_start", "PhoneAFriend");
        TryRegister(SoundEffect.LifelinePAFStart, "PAFStart", "paf_start");
        TryRegister(SoundEffect.LifelinePAFCountdown, "PAFCountdown", "paf_countdown");
        TryRegister(SoundEffect.LifelinePAFEndEarly, "PAFEndEarly", "paf_end_call_early", "paf_end_early");
        TryRegister(SoundEffect.LifelineATA, "ATAStart", "ata_start", "AskTheAudience");
        TryRegister(SoundEffect.LifelineATAStart, "ATAStart", "ata_start");
        TryRegister(SoundEffect.LifelineATAVote, "ATAVote", "ata_vote");
        TryRegister(SoundEffect.LifelineATAEnd, "ATAEnd", "ata_end");
        TryRegister(SoundEffect.LifelineSwitch, "SwitchActivate", "stq_start", "switch_activate");
        TryRegister(SoundEffect.LifelineATHBed, "ATHBed", "host_bed");
        TryRegister(SoundEffect.LifelineATHEnd, "ATHEnd", "host_end");
        TryRegister(SoundEffect.LifelineDoubleDipStart, "DoubleDipStart", "doubledip_start");
        TryRegister(SoundEffect.LifelineDoubleDipFirst, "DoubleDipFirst", "doubledip_first");
        TryRegister(SoundEffect.LifelineDoubleDipSecond, "DoubleDipSecond", "doubledip_second");
        
        // Safety Net
        TryRegister(SoundEffect.SetSafetyNet, "SetSafetyNet", "set_safety_net");
        
        // Other
        TryRegister(SoundEffect.ToHotSeat, "ToHotSeat", "to_hotseat");
        TryRegister(SoundEffect.ExplainRules, "ExplainRules", "explain_rules");
        TryRegister(SoundEffect.RiskMode, "RiskModeActive", "risk_mode");
        TryRegister(SoundEffect.LifelinePing1, "Lifeline1Ping", "lifeline_1_on");
        TryRegister(SoundEffect.LifelinePing2, "Lifeline2Ping", "lifeline_2_on");
        TryRegister(SoundEffect.LifelinePing3, "Lifeline3Ping", "lifeline_3_on");
        TryRegister(SoundEffect.LifelinePing4, "Lifeline4Ping", "lifeline_4_on");
    }

    public void Dispose()
    {
        if (_disposed) return;

        StopAllSounds();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Enumeration of all game sound effects
/// </summary>
public enum SoundEffect
{
    QuestionCue,
    LightsDown,
    FinalAnswer,
    CorrectAnswer,
    WrongAnswer,
    WalkAway,
    Lifeline5050,
    LifelinePhone,
    LifelineATA,
    LifelineSwitch,
    ToHotSeat,
    ExplainRules,
    RiskMode,
    BackgroundMusic,
    HostEntrance,
    ExplainGame,
    QuitSmall,
    QuitLarge,
    WalkAwaySmall,
    WalkAwayLarge,
    CloseTheme,
    CloseUnderscore,
    LifelinePing1,
    LifelinePing2,
    LifelinePing3,
    LifelinePing4,
    LifelinePAFStart,
    LifelinePAFCountdown,
    LifelinePAFEndEarly,
    LifelineATAStart,
    LifelineATAVote,
    LifelineATAEnd,
    LifelineATHBed,
    LifelineATHEnd,
    LifelineDoubleDipStart,
    LifelineDoubleDipFirst,
    LifelineDoubleDipSecond,
    SetSafetyNet
}