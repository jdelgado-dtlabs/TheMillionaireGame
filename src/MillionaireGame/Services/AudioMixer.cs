using CSCore;
using CSCore.CoreAudioAPI;
using CSCore.SoundOut;
using CSCore.Streams;
using MillionaireGame.Utilities;

namespace MillionaireGame.Services;

/// <summary>
/// Mixes multiple audio sources and routes to output devices.
/// Designed for future multi-output support (system speakers + OBS/streaming).
/// </summary>
public class AudioMixer : IDisposable
{
    private ISoundOut? _systemOutput;
    private MixingSampleSource? _mixer;
    private readonly object _lock = new();
    private bool _disposed = false;
    private float _masterVolume = 1.0f;

    // Future: Add additional outputs here
    // private ISoundOut? _broadcastOutput;
    // private ISoundOut? _recordingOutput;

    /// <summary>
    /// Initialize the mixer with source streams and optional output device
    /// </summary>
    /// <param name="musicSource">Music channel audio source</param>
    /// <param name="effectsSource">Effects channel audio source</param>
    /// <param name="deviceId">Optional audio output device ID (null for system default)</param>
    public void Initialize(ISampleSource musicSource, ISampleSource effectsSource, string? deviceId = null)
    {
        lock (_lock)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AudioMixer));

            // Clean up existing resources
            CleanupInternal();

            try
            {
                // Create mixer that combines music and effects
                _mixer = new MixingSampleSource(musicSource.WaveFormat);
                _mixer.AddSource(musicSource);
                _mixer.AddSource(effectsSource);

                // Create system audio output with device selection
                if (string.IsNullOrWhiteSpace(deviceId))
                {
                    _systemOutput = new WasapiOut(); // Use system default
                }
                else
                {
                    try
                    {
                        var device = AudioDeviceManager.GetDeviceById(deviceId);
                        if (device != null)
                        {
                            _systemOutput = new WasapiOut { Device = device };
                        }
                        else
                        {
                            if (Program.DebugMode)
                            {
                                GameConsole.Warn($"[AudioMixer] Device not found: {deviceId}, using default");
                            }
                            _systemOutput = new WasapiOut();
                        }
                    }
                    catch (Exception ex)
                    {
                        if (Program.DebugMode)
                        {
                            GameConsole.Warn($"[AudioMixer] Failed to use device {deviceId}, using default: {ex.Message}");
                        }
                        _systemOutput = new WasapiOut();
                    }
                }

                _systemOutput.Initialize(_mixer.ToWaveSource());
                _systemOutput.Volume = _masterVolume;

                if (Program.DebugMode)
                {
                    var deviceName = string.IsNullOrWhiteSpace(deviceId) ? "System Default" : deviceId;
                    GameConsole.Debug($"[AudioMixer] Initialized with device: {deviceName}");
                    GameConsole.Debug($"[AudioMixer] WasapiOut state after Initialize: {_systemOutput.PlaybackState}");
                }

                // Future: Initialize additional outputs here
                // _broadcastOutput = new WasapiOut(/* specific device */);
                // _broadcastOutput.Initialize(CreateTappedStream(_mixer));
            }
            catch (Exception ex)
            {
                GameConsole.Error($"[AudioMixer] Initialization failed: {ex.Message}");
                CleanupInternal();
                throw;
            }
        }
    }

    /// <summary>
    /// Change the audio output device without disposing the mixer or channel connections.
    /// Only swaps the WasapiOut device - all channel state is preserved.
    /// </summary>
    /// <param name="musicSource">Music channel audio source (not used, kept for compatibility)</param>
    /// <param name="effectsSource">Effects channel audio source (not used, kept for compatibility)</param>
    /// <param name="deviceId">Optional audio output device ID (null for system default)</param>
    public void ChangeDevice(ISampleSource musicSource, ISampleSource effectsSource, string? deviceId = null)
    {
        lock (_lock)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AudioMixer));

            // CRITICAL: Check if mixer exists - if not, do full initialization
            if (_mixer == null)
            {
                if (Program.DebugMode)
                {
                    GameConsole.Warn("[AudioMixer] Mixer not initialized, performing full initialization");
                }
                Initialize(musicSource, effectsSource, deviceId);
                return;
            }

            // Remember playback state
            bool wasPlaying = _systemOutput?.PlaybackState == PlaybackState.Playing;
            
            if (Program.DebugMode)
            {
                GameConsole.Debug($"[AudioMixer] ChangeDevice called - wasPlaying: {wasPlaying}");
            }

            try
            {
                // Stop current output
                if (wasPlaying)
                {
                    _systemOutput?.Stop();
                }

                // Dispose ONLY the WasapiOut - keep the mixer and all channel connections intact
                _systemOutput?.Dispose();
                _systemOutput = null;

                // Create new WasapiOut with new device
                if (string.IsNullOrWhiteSpace(deviceId))
                {
                    _systemOutput = new WasapiOut();
                }
                else
                {
                    try
                    {
                        var device = AudioDeviceManager.GetDeviceById(deviceId);
                        if (device != null)
                        {
                            _systemOutput = new WasapiOut { Device = device };
                        }
                        else
                        {
                            if (Program.DebugMode)
                            {
                                GameConsole.Warn($"[AudioMixer] Device not found: {deviceId}, using default");
                            }
                            _systemOutput = new WasapiOut();
                        }
                    }
                    catch (Exception ex)
                    {
                        if (Program.DebugMode)
                        {
                            GameConsole.Warn($"[AudioMixer] Failed to use device {deviceId}, using default: {ex.Message}");
                        }
                        _systemOutput = new WasapiOut();
                    }
                }

                // Initialize with EXISTING mixer (preserves all channel connections and state)
                _systemOutput.Initialize(_mixer.ToWaveSource());
                _systemOutput.Volume = _masterVolume;

                if (Program.DebugMode)
                {
                    var deviceName = string.IsNullOrWhiteSpace(deviceId) ? "System Default" : deviceId;
                    GameConsole.Debug($"[AudioMixer] Device changed to: {deviceName}");
                }

                // Always restart playback (even if nothing was playing, mixer needs to be active)
                Start();
                
                if (Program.DebugMode)
                {
                    GameConsole.Debug($"[AudioMixer] Device change complete - mixer connections preserved");
                }
            }
            catch (Exception ex)
            {
                GameConsole.Error($"[AudioMixer] Device change failed: {ex.Message}");
                // Don't cleanup - try to preserve existing state
                throw;
            }
        }
    }

    /// <summary>
    /// Start audio playback through all outputs
    /// </summary>
    public void Start()
    {
        lock (_lock)
        {
            if (Program.DebugMode)
            {
                GameConsole.Debug($"[AudioMixer] Start() called");
                GameConsole.Debug($"[AudioMixer] _systemOutput null? {_systemOutput == null}");
                GameConsole.Debug($"[AudioMixer] Current state: {_systemOutput?.PlaybackState}");
            }
            
            if (_systemOutput?.PlaybackState != PlaybackState.Playing)
            {
                if (Program.DebugMode)
                {
                    GameConsole.Debug($"[AudioMixer] Calling Play()...");
                }
                _systemOutput?.Play();
                if (Program.DebugMode)
                {
                    GameConsole.Debug($"[AudioMixer] Play() completed. New state: {_systemOutput?.PlaybackState}");
                }
            }
            else
            {
                if (Program.DebugMode)
                {
                    GameConsole.Debug($"[AudioMixer] Already playing, skipping Play() call");
                }
            }

            // Future: Start additional outputs
            // _broadcastOutput?.Play();
            // _recordingOutput?.Play();
        }
    }

    /// <summary>
    /// Stop audio playback through all outputs
    /// </summary>
    public void Stop()
    {
        lock (_lock)
        {
            if (_systemOutput?.PlaybackState == PlaybackState.Playing)
            {
                _systemOutput.Stop();

                // Future: Stop additional outputs
                // _broadcastOutput?.Stop();
                // _recordingOutput?.Stop();

                if (Program.DebugMode)
                {
                    GameConsole.Debug("[AudioMixer] Playback stopped");
                }
            }
        }
    }

    /// <summary>
    /// Set master output volume (0.0 to 1.0)
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        volume = Math.Clamp(volume, 0.0f, 1.0f);

        lock (_lock)
        {
            _masterVolume = volume;

            if (_systemOutput != null)
            {
                _systemOutput.Volume = volume;
            }

            // Future: Set volume for additional outputs
            // if (_broadcastOutput != null) _broadcastOutput.Volume = volume;
        }
    }

    /// <summary>
    /// Check if mixer is currently playing
    /// </summary>
    public bool IsPlaying
    {
        get
        {
            lock (_lock)
            {
                return _systemOutput?.PlaybackState == PlaybackState.Playing;
            }
        }
    }

    /// <summary>
    /// Future: Add a new output destination for broadcasting
    /// </summary>
    /// <remarks>
    /// Example usage when broadcasting is implemented:
    /// - AddOutput("OBS", new WasapiOut(obsDeviceId));
    /// - AddOutput("Recording", new WasapiOut(recordingDeviceId));
    /// </remarks>
    public void AddOutput(string name, ISoundOut output)
    {
        // TODO [POST-1.0]: Implement when OBS/streaming feature is added (v1.1 target)
        // Status: Deferred - Advanced feature for power users
        // This will allow routing audio to OBS, virtual cables, etc.
        // See: docs/active/PRE_1.0_FINAL_CHECKLIST.md - Deferred section
        throw new NotImplementedException("Multi-output routing will be implemented with broadcasting feature");
    }

    /// <summary>
    /// Future: Create a tapped stream for additional outputs
    /// </summary>
    private ISampleSource CreateTappedStream(ISampleSource source)
    {
        // TODO [POST-1.0]: Implement audio tapping for multi-output (v1.1 target)
        // Status: Deferred - Required for OBS/streaming integration
        // This will duplicate the audio stream so it can go to multiple destinations
        // without interfering with each other
        return source;
    }

    /// <summary>
    /// Cleanup resources - must be called within lock
    /// </summary>
    private void CleanupInternal()
    {
        try
        {
            _systemOutput?.Stop();
            _systemOutput?.Dispose();
            _systemOutput = null;
        }
        catch { }

        try
        {
            _mixer?.Dispose();
            _mixer = null;
        }
        catch { }

        // Future: Cleanup additional outputs
        // _broadcastOutput?.Dispose();
        // _recordingOutput?.Dispose();
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
            CleanupInternal();
        }

        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Mixes multiple sample sources into a single output stream.
/// Each source is mixed at equal volume (can be extended for per-source volume control).
/// </summary>
public class MixingSampleSource : ISampleSource
{
    private readonly WaveFormat _waveFormat;
    private readonly List<ISampleSource> _sources = new();
    private readonly object _lock = new();
    private long _position;

    public MixingSampleSource(WaveFormat waveFormat)
    {
        _waveFormat = waveFormat ?? throw new ArgumentNullException(nameof(waveFormat));
    }

    public void AddSource(ISampleSource source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        if (!source.WaveFormat.Equals(_waveFormat))
            throw new ArgumentException("Source format must match mixer format");

        lock (_lock)
        {
            _sources.Add(source);
        }
    }

    public void RemoveSource(ISampleSource source)
    {
        lock (_lock)
        {
            _sources.Remove(source);
        }
    }

    public bool CanSeek => false;

    public WaveFormat WaveFormat => _waveFormat;

    public long Position
    {
        get => _position;
        set => throw new NotSupportedException("MixingSampleSource does not support seeking");
    }

    public long Length => 0; // Infinite length for live mixing

    public int Read(float[] buffer, int offset, int count)
    {
        // Clear buffer
        Array.Clear(buffer, offset, count);

        int maxSamplesRead = 0;

        lock (_lock)
        {
            // Mix all sources
            float[] tempBuffer = new float[count];

            foreach (var source in _sources.ToList()) // ToList to avoid modification during enumeration
            {
                Array.Clear(tempBuffer, 0, count);
                int samplesRead = source.Read(tempBuffer, 0, count);

                if (samplesRead > maxSamplesRead)
                    maxSamplesRead = samplesRead;

                // Add this source to the mix
                for (int i = 0; i < samplesRead; i++)
                {
                    buffer[offset + i] += tempBuffer[i];
                }
            }

            // Prevent clipping by normalizing if necessary
            if (_sources.Count > 1)
            {
                float divisor = (float)Math.Sqrt(_sources.Count); // Simple mixing normalization
                for (int i = 0; i < maxSamplesRead; i++)
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
            foreach (var source in _sources)
            {
                source?.Dispose();
            }
            _sources.Clear();
        }
    }
}
