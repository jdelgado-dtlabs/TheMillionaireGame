using CSCore;
using CSCore.Codecs;
using CSCore.SoundOut;
using MillionaireGame.Utilities;

namespace MillionaireGame.Services;

/// <summary>
/// Manages one-shot sound effect playback using CSCore.
/// Handles sounds that play once and complete (reveal sounds, lifelines, etc.).
/// </summary>
public class EffectsChannel : IDisposable
{
    private readonly Dictionary<string, EffectPlayer> _activePlayers = new();
    private readonly object _lock = new();
    private bool _disposed = false;
    private float _volume = 1.0f;

    /// <summary>
    /// Play a sound effect
    /// </summary>
    public string PlayEffect(string filePath, string? identifier = null)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            if (Program.DebugMode)
            {
                GameConsole.Warn("[EffectsChannel] PlayEffect called with null/empty path");
            }
            return string.Empty;
        }

        if (!File.Exists(filePath))
        {
            if (Program.DebugMode)
            {
                GameConsole.Error($"[EffectsChannel] File not found: {filePath}");
            }
            return string.Empty;
        }

        try
        {
            string id = identifier ?? Guid.NewGuid().ToString();

            if (Program.DebugMode)
            {
                GameConsole.Debug($"[EffectsChannel] Playing: {Path.GetFileName(filePath)} (id: {id})");
            }

            var player = new EffectPlayer(id, filePath, _volume);
            player.PlaybackStopped += OnEffectStopped;

            lock (_lock)
            {
                _activePlayers[id] = player;
            }

            player.Play();
            return id;
        }
        catch (Exception ex)
        {
            GameConsole.Error($"[EffectsChannel] Error playing effect: {ex.Message}");
            return string.Empty;
        }
    }

    /// <summary>
    /// Stop a specific effect by identifier
    /// </summary>
    public void StopEffect(string identifier)
    {
        if (string.IsNullOrEmpty(identifier)) return;

        EffectPlayer? player = null;
        lock (_lock)
        {
            if (_activePlayers.TryGetValue(identifier, out player))
            {
                _activePlayers.Remove(identifier);
            }
        }

        if (player != null)
        {
            try
            {
                player.PlaybackStopped -= OnEffectStopped;
                player.Stop();
                player.Dispose();

                if (Program.DebugMode)
                {
                    GameConsole.Debug($"[EffectsChannel] Stopped effect: {identifier}");
                }
            }
            catch (Exception ex)
            {
                if (Program.DebugMode)
                {
                    GameConsole.Debug($"[EffectsChannel] Error stopping effect: {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// Stop all currently playing effects
    /// </summary>
    public void StopAllEffects()
    {
        List<EffectPlayer> players;
        lock (_lock)
        {
            players = new List<EffectPlayer>(_activePlayers.Values);
            _activePlayers.Clear();

            if (Program.DebugMode)
            {
                GameConsole.Debug($"[EffectsChannel] Stopping {players.Count} effect(s)");
            }
        }

        // Stop and dispose all players asynchronously to avoid blocking
        Task.Run(() =>
        {
            foreach (var player in players)
            {
                try
                {
                    player.PlaybackStopped -= OnEffectStopped;
                    player.Stop();
                    player.Dispose();
                }
                catch
                {
                    // Ignore errors during cleanup
                }
            }
        });
    }

    /// <summary>
    /// Check if a specific effect is currently playing
    /// </summary>
    public bool IsEffectPlaying(string identifier)
    {
        lock (_lock)
        {
            return _activePlayers.ContainsKey(identifier);
        }
    }

    /// <summary>
    /// Set effects volume (0.0 to 1.0)
    /// </summary>
    public void SetVolume(float volume)
    {
        volume = Math.Clamp(volume, 0.0f, 1.0f);

        lock (_lock)
        {
            _volume = volume;

            // Update volume on all active players
            foreach (var player in _activePlayers.Values)
            {
                player.SetVolume(volume);
            }
        }
    }

    /// <summary>
    /// Clear completed effects from memory
    /// </summary>
    public void ClearCompleted()
    {
        lock (_lock)
        {
            var completedIds = _activePlayers
                .Where(kvp => !kvp.Value.IsPlaying)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var id in completedIds)
            {
                if (_activePlayers.TryGetValue(id, out var player))
                {
                    player.PlaybackStopped -= OnEffectStopped;
                    player.Dispose();
                    _activePlayers.Remove(id);
                }
            }

            if (Program.DebugMode && completedIds.Count > 0)
            {
                GameConsole.Debug($"[EffectsChannel] Cleared {completedIds.Count} completed effect(s)");
            }
        }
    }

    /// <summary>
    /// Handle effect playback stopped event
    /// </summary>
    private void OnEffectStopped(object? sender, string identifier)
    {
        // Cleanup completed effect asynchronously
        Task.Run(() =>
        {
            lock (_lock)
            {
                if (_activePlayers.TryGetValue(identifier, out var player))
                {
                    player.PlaybackStopped -= OnEffectStopped;
                    player.Dispose();
                    _activePlayers.Remove(identifier);

                    if (Program.DebugMode)
                    {
                        GameConsole.Debug($"[EffectsChannel] Effect completed: {identifier}");
                    }
                }
            }
        });
    }

    /// <summary>
    /// Dispose of all resources
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        lock (_lock)
        {
            _disposed = true;

            foreach (var player in _activePlayers.Values)
            {
                try
                {
                    player.PlaybackStopped -= OnEffectStopped;
                    player.Dispose();
                }
                catch { }
            }

            _activePlayers.Clear();
        }

        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Single effect player wrapper for CSCore
/// </summary>
internal class EffectPlayer : IDisposable
{
    private readonly string _identifier;
    private readonly string _filePath;
    private ISoundOut? _soundOut;
    private IWaveSource? _waveSource;
    private bool _disposed = false;

    public event EventHandler<string>? PlaybackStopped;

    public EffectPlayer(string identifier, string filePath, float volume)
    {
        _identifier = identifier;
        _filePath = filePath;

        // Load audio file
        _waveSource = CodecFactory.Instance.GetCodec(filePath);

        // Create sound output
        _soundOut = new WasapiOut();
        _soundOut.Initialize(_waveSource);
        _soundOut.Volume = volume;

        // Handle stopped event
        _soundOut.Stopped += (s, e) =>
        {
            PlaybackStopped?.Invoke(this, _identifier);
        };
    }

    public bool IsPlaying => _soundOut?.PlaybackState == PlaybackState.Playing;

    public void Play()
    {
        _soundOut?.Play();
    }

    public void Stop()
    {
        if (_soundOut?.PlaybackState == PlaybackState.Playing)
        {
            _soundOut.Stop();
        }
    }

    public void SetVolume(float volume)
    {
        if (_soundOut != null)
        {
            _soundOut.Volume = Math.Clamp(volume, 0.0f, 1.0f);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        try
        {
            _soundOut?.Dispose();
            _soundOut = null;
        }
        catch { }

        try
        {
            _waveSource?.Dispose();
            _waveSource = null;
        }
        catch { }
    }
}
