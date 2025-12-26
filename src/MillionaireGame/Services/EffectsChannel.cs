using CSCore;
using CSCore.Codecs;
using CSCore.Codecs.MP3;
using CSCore.MediaFoundation;
using CSCore.Streams.SampleConverter;
using MillionaireGame.Core.Settings;
using MillionaireGame.Utilities;

namespace MillionaireGame.Services;

/// <summary>
/// Manages one-shot sound effect playback using CSCore.
/// Handles sounds that play once and complete (reveal sounds, lifelines, etc.).
/// Provides a continuous ISampleSource stream for mixer integration.
/// </summary>
public class EffectsChannel : IDisposable
{
    private readonly EffectsMixerSource _mixerSource;
    private readonly AudioCueQueue _cueQueue;
    private readonly object _lock = new();
    private readonly SilenceDetectionSettings _silenceSettings;
    private readonly CrossfadeSettings _crossfadeSettings;
    private bool _disposed = false;
    private float _volume = 1.0f;

    public EffectsChannel(SilenceDetectionSettings? silenceSettings = null, CrossfadeSettings? crossfadeSettings = null)
    {
        // Create a standard wave format (44.1kHz, 16-bit, stereo)
        var waveFormat = new WaveFormat(44100, 16, 2);
        _mixerSource = new EffectsMixerSource(waveFormat);
        _silenceSettings = silenceSettings ?? new SilenceDetectionSettings();
        _crossfadeSettings = crossfadeSettings ?? new CrossfadeSettings();
        
        // Initialize audio cue queue with configured settings including silence detection
        // AudioCueQueue needs ISampleSource format (44.1kHz, 32-bit float, stereo)
        var sampleFormat = new CSCore.WaveFormat(44100, 32, 2, AudioEncoding.IeeeFloat);
        _cueQueue = new AudioCueQueue(
            sampleFormat,
            _crossfadeSettings.CrossfadeDurationMs,
            _crossfadeSettings.QueueLimit,
            _silenceSettings  // Pass silence settings to queue
        );
        
        // CRITICAL: Add the queue's output stream to the mixer so queued audio plays through
        // Use a special identifier so it persists and isn't removed like normal effects
        _mixerSource.AddEffect("__queue__", _cueQueue);
        
        // Always log this critical initialization step
        GameConsole.Info("[EffectsChannel] Audio queue connected to mixer");
    }

    /// <summary>
    /// Gets the output stream for mixer integration
    /// </summary>
    public ISampleSource GetOutputStream()
    {
        return _mixerSource;
    }
    
    /// <summary>
    /// Gets the count of active effects
    /// </summary>
    public int ActiveEffectCount => _mixerSource.GetActiveEffectCount();

    /// <summary>
    /// Play a sound effect immediately (interrupts queue with crossfade).
    /// Wrapper for QueueEffect with Immediate priority for backward compatibility.
    /// </summary>
    /// <param name="filePath">Path to the audio file</param>
    /// <param name="identifier">Optional identifier (not used in queue mode)</param>
    /// <returns>Identifier for the sound (always returns filePath for compatibility)</returns>
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

        // Use Immediate priority to interrupt current playback with crossfade
        bool queued = QueueEffect(filePath, AudioPriority.Immediate);
        
        return queued ? (identifier ?? filePath) : string.Empty;
    }

    /// <summary>
    /// Stop a specific effect by identifier (not supported in queue mode)
    /// </summary>
    public void StopEffect(string identifier)
    {
        if (Program.DebugMode)
        {
            GameConsole.Warn("[EffectsChannel] StopEffect not supported in queue mode - use StopQueue() instead");
        }
    }

    /// <summary>
    /// Stop all currently playing effects (clears queue and stops playback)
    /// </summary>
    public void StopAllEffects()
    {
        StopQueue();
        if (Program.DebugMode)
        {
            GameConsole.Debug("[EffectsChannel] Stopped all effects (cleared queue)");
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
            _mixerSource.SetVolume(volume);
        }
    }

    /// <summary>
    /// Queue an audio file for sequential playback with automatic crossfading and silence detection
    /// </summary>
    /// <param name="filePath">Path to the audio file</param>
    /// <param name="priority">Priority level (Normal or Immediate)</param>
    /// <returns>True if queued successfully, false if queue is full</returns>
    public bool QueueEffect(string filePath, AudioPriority priority = AudioPriority.Normal)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            if (Program.DebugMode)
            {
                GameConsole.Warn("[EffectsChannel] QueueEffect called with null/empty path");
            }
            return false;
        }

        if (!File.Exists(filePath))
        {
            if (Program.DebugMode)
            {
                GameConsole.Error($"[EffectsChannel] File not found for queue: {filePath}");
            }
            return false;
        }

        return _cueQueue.QueueAudio(filePath, priority);
    }

    /// <summary>
    /// Clear all queued audio (does not stop current playback)
    /// </summary>
    public void ClearQueue()
    {
        _cueQueue.ClearQueue();
    }

    /// <summary>
    /// Stop the queue and clear all queued audio (abrupt stop)
    /// </summary>
    public void StopQueue()
    {
        _cueQueue.Stop();
    }

    /// <summary>
    /// Fade out current audio and clear queue (smooth stop)
    /// </summary>
    /// <param name="fadeoutDurationMs">Duration of fadeout in milliseconds (default: 50ms)</param>
    public void StopQueueWithFadeout(int fadeoutDurationMs = 50)
    {
        _cueQueue.StopWithFadeout(fadeoutDurationMs);
    }

    /// <summary>
    /// Skip to next queued sound with crossfade
    /// </summary>
    public void SkipToNext()
    {
        _cueQueue.SkipToNext();
    }

    /// <summary>
    /// Get the number of sounds currently in the queue (waiting)
    /// </summary>
    public int GetQueueCount()
    {
        return _cueQueue.QueueCount;
    }

    /// <summary>
    /// Get the total number of sounds (playing + next + queued)
    /// </summary>
    public int GetTotalSoundCount()
    {
        return _cueQueue.TotalSoundCount;
    }

    /// <summary>
    /// Check if the queue is currently playing audio
    /// </summary>
    public bool IsQueuePlaying()
    {
        return _cueQueue.IsPlaying;
    }

    /// <summary>
    /// Check if a crossfade is currently in progress
    /// </summary>
    public bool IsQueueCrossfading()
    {
        return _cueQueue.IsCrossfading;
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
            _cueQueue?.Dispose();
            _mixerSource?.Dispose();
        }

        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Continuously mixes multiple effect sources into a single ISampleSource stream
/// </summary>
internal class EffectsMixerSource : ISampleSource
{
    private readonly WaveFormat _waveFormat;
    private readonly Dictionary<string, EffectStream> _effects = new();
    private readonly object _lock = new();
    private long _position;
    private int _silenceLogCounter = 0; // Rate limiter for silence logging
    private int _audioLogCounter = 0; // Rate limiter for audio logging

    private class EffectStream
    {
        public ISampleSource Source { get; set; }
        public bool IsCompleted { get; set; }

        public EffectStream(ISampleSource source)
        {
            Source = source;
            IsCompleted = false;
        }
    }

    public EffectsMixerSource(WaveFormat waveFormat)
    {
        _waveFormat = waveFormat ?? throw new ArgumentNullException(nameof(waveFormat));
    }

    public void AddEffect(string identifier, ISampleSource source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        lock (_lock)
        {
            _effects[identifier] = new EffectStream(source);
        }

        if (Program.DebugMode)
        {
            GameConsole.Debug($"[EffectsMixerSource] Added effect: {identifier}");
        }
    }

    public void RemoveEffect(string identifier)
    {
        lock (_lock)
        {
            if (_effects.TryGetValue(identifier, out var effect))
            {
                effect.Source?.Dispose();
                _effects.Remove(identifier);
            }
        }
    }

    public void RemoveAllEffects()
    {
        lock (_lock)
        {
            foreach (var effect in _effects.Values)
            {
                effect.Source?.Dispose();
            }
            _effects.Clear();
        }
    }

    public bool HasEffect(string identifier)
    {
        lock (_lock)
        {
            return _effects.ContainsKey(identifier);
        }
    }

    public int GetActiveEffectCount()
    {
        lock (_lock)
        {
            return _effects.Count;
        }
    }

    public void SetVolume(float volume)
    {
        lock (_lock)
        {
            foreach (var effect in _effects.Values)
            {
                if (effect.Source is VolumeSource volumeSource)
                {
                    volumeSource.Volume = volume;
                }
            }
        }
    }

    public int ClearCompleted()
    {
        int cleared = 0;
        lock (_lock)
        {
            var completedIds = _effects
                .Where(kvp => kvp.Value.IsCompleted)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var id in completedIds)
            {
                if (_effects.TryGetValue(id, out var effect))
                {
                    effect.Source?.Dispose();
                    _effects.Remove(id);
                    cleared++;
                }
            }
        }
        return cleared;
    }

    public bool CanSeek => false;
    public WaveFormat WaveFormat => new WaveFormat(44100, 32, 2, AudioEncoding.IeeeFloat); // Float format for ISampleSource
    public long Position
    {
        get => _position;
        set => throw new NotSupportedException("EffectsMixerSource does not support seeking");
    }
    public long Length => 0; // Infinite length for continuous mixing

    public int Read(float[] buffer, int offset, int count)
    {
        // Collect logging data without calling GameConsole inside lock
        int activeEffectsCount = 0;
        float maxAmplitude = 0;
        int samplesReturned = 0;
        var completedEffects = new List<string>();
        
        // Clear buffer
        Array.Clear(buffer, offset, count);

        int maxSamplesRead = count; // Always output full buffer (silence if no effects)

        // Collect sources to dispose OUTSIDE the lock
        var sourcesToDispose = new List<ISampleSource>();
        
        lock (_lock)
        {
            activeEffectsCount = _effects.Count;
            
            if (_effects.Count == 0)
            {
                // No effects playing - return silence
                _position += count;
                return count;
            }

            // Mix all active effects
            float[] tempBuffer = new float[count];
            var effectsToRemove = new List<string>();

            foreach (var kvp in _effects.ToList())
            {
                var effect = kvp.Value;
                Array.Clear(tempBuffer, 0, count);

                int samplesRead = effect.Source.Read(tempBuffer, 0, count);

                if (samplesRead == 0)
                {
                    // Effect completed - mark for removal
                    effect.IsCompleted = true;
                    effectsToRemove.Add(kvp.Key);
                    completedEffects.Add(kvp.Key);
                }
                else
                {
                    // Add this effect to the mix
                    for (int i = 0; i < samplesRead; i++)
                    {
                        buffer[offset + i] += tempBuffer[i];
                    }
                }
            }

            // Remove completed effects and collect sources for disposal
            foreach (var id in effectsToRemove)
            {
                if (_effects.TryGetValue(id, out var effect))
                {
                    if (effect.Source != null)
                    {
                        sourcesToDispose.Add(effect.Source);
                    }
                    _effects.Remove(id);
                }
            }

            // Prevent clipping if multiple effects are playing
            if (_effects.Count > 1)
            {
                float divisor = (float)Math.Sqrt(_effects.Count);
                for (int i = 0; i < count; i++)
                {
                    buffer[offset + i] /= divisor;
                }
            }
            
            samplesReturned = maxSamplesRead;
        }
        
        // Dispose sources OUTSIDE the lock to prevent blocking
        foreach (var source in sourcesToDispose)
        {
            try
            {
                source.Dispose();
            }
            catch (Exception ex)
            {
                if (Program.DebugMode)
                {
                    GameConsole.Warn($"[EffectsMixer] Error disposing source: {ex.Message}");
                }
            }
        }
        
        // Calculate max amplitude and log OUTSIDE the lock
        if (Program.DebugMode && activeEffectsCount > 0)
        {
            for (int i = offset; i < offset + samplesReturned; i++)
            {
                if (Math.Abs(buffer[i]) > maxAmplitude)
                    maxAmplitude = Math.Abs(buffer[i]);
            }
            
            if (maxAmplitude > 0)
            {
                // Rate limit audio logging - log every 20th call (still ~2-5 times per second)
                _audioLogCounter++;
                if (_audioLogCounter >= 20)
                {
                    GameConsole.Debug($"[EffectsMixer] {samplesReturned} samples, {activeEffectsCount} active, max: {maxAmplitude:F4} (logged 1/20 calls)");
                    _audioLogCounter = 0;
                }
                _silenceLogCounter = 0; // Reset silence counter when we have audio
            }
            else if (samplesReturned > 0)
            {
                // Only log silence every 100th call to avoid flooding logs
                _silenceLogCounter++;
                if (_silenceLogCounter >= 100)
                {
                    GameConsole.Debug($"[EffectsMixer] {samplesReturned} samples (SILENCE), {activeEffectsCount} active (logged 1/100 calls)");
                    _silenceLogCounter = 0;
                }
                _audioLogCounter = 0; // Reset audio counter when silent
            }
            
            // Always log completed effects
            foreach (var id in completedEffects)
            {
                GameConsole.Debug($"[EffectsMixer] Effect completed: {id}");
            }
        }

        _position += maxSamplesRead;
        return maxSamplesRead;
    }

    public void Dispose()
    {
        lock (_lock)
        {
            foreach (var effect in _effects.Values)
            {
                effect.Source?.Dispose();
            }
            _effects.Clear();
        }
    }
}
