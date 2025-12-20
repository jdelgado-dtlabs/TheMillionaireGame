using NAudio.Wave;

namespace MillionaireGame.Services;

/// <summary>
/// Manages game audio playback using NAudio
/// </summary>
public class SoundService : IDisposable
{
    private readonly Dictionary<SoundEffect, string> _soundPaths = new();
    private readonly Dictionary<string, IWavePlayer> _activePlayers = new();
    private readonly object _lock = new();
    private bool _soundEnabled = true;
    private bool _disposed = false;

    public bool SoundEnabled
    {
        get => _soundEnabled;
        set => _soundEnabled = value;
    }

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
                Console.WriteLine($"[Sound] Registered {effect}: {resolvedPath}");
            }
        }
        else
        {
            if (Program.DebugMode)
            {
                Console.WriteLine($"[Sound] Warning: File not found for {effect}: {filePath}");
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
                Console.WriteLine($"[Sound] File not found: {filePath} (resolved to: {resolvedPath})");
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
        // Broadcast flow sounds
        RegisterSound(SoundEffect.HostEntrance, settings.SoundHostStart);
        RegisterSound(SoundEffect.ExplainGame, settings.SoundExplainRules);
        RegisterSound(SoundEffect.QuitSmall, settings.SoundQuitSmall);
        RegisterSound(SoundEffect.QuitLarge, settings.SoundQuitLarge);
        RegisterSound(SoundEffect.WalkAwaySmall, settings.SoundWalkAway1);
        RegisterSound(SoundEffect.WalkAwayLarge, settings.SoundWalkAway2);
        RegisterSound(SoundEffect.CloseTheme, settings.SoundCloseFinal);
        RegisterSound(SoundEffect.CloseUnderscore, settings.SoundCloseStart);
        
        // Register common game sounds
        RegisterSound(SoundEffect.LightsDown, settings.SoundQ1to5LightsDown);
        RegisterSound(SoundEffect.QuestionCue, settings.SoundQ1to5Bed);
        RegisterSound(SoundEffect.FinalAnswer, settings.SoundATAVoting); // Placeholder
        RegisterSound(SoundEffect.CorrectAnswer, settings.SoundQ1to5Bed); // Placeholder
        RegisterSound(SoundEffect.WrongAnswer, settings.SoundGameOver);
        RegisterSound(SoundEffect.WalkAway, settings.SoundWalkAway1);
        
        // Lifelines
        RegisterSound(SoundEffect.Lifeline5050, settings.Sound5050);
        RegisterSound(SoundEffect.LifelinePhone, "paf_start.mp3");
        RegisterSound(SoundEffect.LifelinePAFStart, settings.SoundLifelinePAFStart ?? "paf_start.mp3");
        RegisterSound(SoundEffect.LifelinePAFCountdown, settings.SoundLifelinePAFCountdown ?? "paf_countdown.mp3");
        RegisterSound(SoundEffect.LifelinePAFEndEarly, settings.SoundLifelinePAFEndEarly ?? "paf_end_call_early.mp3");
        RegisterSound(SoundEffect.LifelineATA, settings.SoundATAStart);
        RegisterSound(SoundEffect.LifelineATAStart, settings.SoundLifelineATAStart ?? "ata_start.mp3");
        RegisterSound(SoundEffect.LifelineATAVote, settings.SoundLifelineATAVote ?? "ata_vote.mp3");
        RegisterSound(SoundEffect.LifelineATAEnd, settings.SoundLifelineATAEnd ?? "ata_end.mp3");
        RegisterSound(SoundEffect.LifelineSwitch, settings.SoundSwitchActivate);
        
        // Other
        RegisterSound(SoundEffect.ToHotSeat, settings.SoundToHotSeat);
        RegisterSound(SoundEffect.ExplainRules, settings.SoundExplainRules);
        RegisterSound(SoundEffect.RiskMode, settings.SoundRiskModeActive);
        RegisterSound(SoundEffect.LifelinePing1, settings.SoundLifeline1Ping);
        RegisterSound(SoundEffect.LifelinePing2, settings.SoundLifeline2Ping);
        RegisterSound(SoundEffect.LifelinePing3, settings.SoundLifeline3Ping);
        RegisterSound(SoundEffect.LifelinePing4, settings.SoundLifeline4Ping);
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
    LifelineATAEnd
}