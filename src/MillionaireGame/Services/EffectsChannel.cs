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
        
        // Initialize audio cue queue with configured settings
        // AudioCueQueue needs ISampleSource format (44.1kHz, 32-bit float, stereo)
        var sampleFormat = new CSCore.WaveFormat(44100, 32, 2, AudioEncoding.IeeeFloat);
        _cueQueue = new AudioCueQueue(
            sampleFormat,
            _crossfadeSettings.CrossfadeDurationMs,
            _crossfadeSettings.QueueLimit
        );
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
                GameConsole.Debug($"[EffectsChannel] Starting playback: {Path.GetFileName(filePath)} (id: {id})");
                GameConsole.Debug($"[EffectsChannel] Full path: {filePath}");
                GameConsole.Debug($"[EffectsChannel] Loading codec...");
            }
            
            // Try MediaFoundation decoder first for MP3 files
            IWaveSource waveSource;
            if (filePath.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    waveSource = new MediaFoundationDecoder(filePath);
                    if (Program.DebugMode)
                    {
                        GameConsole.Debug($"[EffectsChannel] Using MediaFoundationDecoder for MP3");
                    }
                }
                catch (Exception ex)
                {
                    if (Program.DebugMode)
                    {
                        GameConsole.Warn($"[EffectsChannel] MediaFoundation failed ({ex.Message}), falling back to DmoMp3Decoder");
                    }
                    waveSource = new DmoMp3Decoder(filePath);
                }
            }
            else
            {
                waveSource = CodecFactory.Instance.GetCodec(filePath);
            }
            
            if (Program.DebugMode)
            {
                GameConsole.Debug($"[EffectsChannel] Codec loaded: Format={waveSource.WaveFormat}, Length={waveSource.Length}");
                GameConsole.Debug($"[EffectsChannel] Codec CanSeek: {waveSource.CanSeek}, Position: {waveSource.Position}");
            }
            var sampleSource = waveSource.ToSampleSource();
            
            // Wrap with silence detector if enabled for effects
            if (_silenceSettings.Enabled && _silenceSettings.ApplyToEffects)
            {
                var silenceDetector = new SilenceDetectorSource(
                    sampleSource,
                    _silenceSettings.ThresholdDb,
                    _silenceSettings.SilenceDurationMs,
                    _silenceSettings.FadeoutDurationMs
                );
                
                // Log when silence is detected
                silenceDetector.SilenceDetected += (s, e) =>
                {
                    if (Program.DebugMode)
                    {
                        GameConsole.Info($"[EffectsChannel] Effect '{id}' auto-completed via silence detection");
                    }
                };
                
                sampleSource = silenceDetector;
            }
            
            // Apply volume
            var volumeSource = new VolumeSource(sampleSource);
            volumeSource.Volume = _volume;
            
            if (Program.DebugMode)
            {
                GameConsole.Debug($"[EffectsChannel] Volume set to: {_volume}");
                GameConsole.Debug($"[EffectsChannel] Adding to mixer source...");
            }
            
            _mixerSource.AddEffect(id, volumeSource);
            
            if (Program.DebugMode)
            {
                GameConsole.Debug($"[EffectsChannel] Added to mixer successfully. Active effects: {ActiveEffectCount}");
            }

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

        _mixerSource.RemoveEffect(identifier);

        if (Program.DebugMode)
        {
            GameConsole.Debug($"[EffectsChannel] Stopped effect: {identifier}");
        }
    }

    /// <summary>
    /// Stop all currently playing effects
    /// </summary>
    public void StopAllEffects()
    {
        int count = _mixerSource.GetActiveEffectCount();
        _mixerSource.RemoveAllEffects();

        if (Program.DebugMode)
        {
            GameConsole.Debug($"[EffectsChannel] Stopped {count} effect(s)");
        }
    }

    /// <summary>
    /// Check if a specific effect is currently playing
    /// </summary>
    public bool IsEffectPlaying(string identifier)
    {
        return _mixerSource.HasEffect(identifier);
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
    /// Clear completed effects from memory
    /// </summary>
    public void ClearCompleted()
    {
        int cleared = _mixerSource.ClearCompleted();

        if (Program.DebugMode && cleared > 0)
        {
            GameConsole.Debug($"[EffectsChannel] Cleared {cleared} completed effect(s)");
        }
    }

    /// <summary>
    /// Queue an audio file for sequential playback with automatic crossfading
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
    /// Stop the queue and clear all queued audio
    /// </summary>
    public void StopQueue()
    {
        _cueQueue.Stop();
    }

    /// <summary>
    /// Get the number of sounds currently in the queue
    /// </summary>
    public int GetQueueCount()
    {
        return _cueQueue.QueueCount;
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
        if (Program.DebugMode && _effects.Count > 0)
        {
            GameConsole.Debug($"[EffectsMixerSource] Read() called: count={count}, active effects={_effects.Count}");
        }
        
        // Clear buffer
        Array.Clear(buffer, offset, count);

        int maxSamplesRead = count; // Always output full buffer (silence if no effects)

        lock (_lock)
        {
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
                    // Effect completed
                    effect.IsCompleted = true;
                    effectsToRemove.Add(kvp.Key);

                    if (Program.DebugMode)
                    {
                        GameConsole.Debug($"[EffectsMixerSource] Effect completed: {kvp.Key}");
                    }
                }
                else
                {
                    // Check if buffer has actual audio data
                    float maxSample = 0;
                    for (int i = 0; i < samplesRead; i++)
                    {
                        if (Math.Abs(tempBuffer[i]) > maxSample)
                            maxSample = Math.Abs(tempBuffer[i]);
                    }
                    
                    if (Program.DebugMode && maxSample > 0)
                    {
                        GameConsole.Debug($"[EffectsMixerSource] Read {samplesRead} samples, max amplitude: {maxSample:F4}");
                    }
                    else if (Program.DebugMode && maxSample == 0)
                    {
                        GameConsole.Debug($"[EffectsMixerSource] WARNING: Read {samplesRead} samples but all are ZERO (silence)");
                    }
                    
                    // Add this effect to the mix
                    for (int i = 0; i < samplesRead; i++)
                    {
                        buffer[offset + i] += tempBuffer[i];
                    }
                }
            }

            // Remove completed effects
            foreach (var id in effectsToRemove)
            {
                if (_effects.TryGetValue(id, out var effect))
                {
                    effect.Source?.Dispose();
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
