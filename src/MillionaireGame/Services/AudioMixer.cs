using CSCore;
using CSCore.CoreAudioAPI;
using CSCore.SoundOut;
using CSCore.Streams;
using MillionaireGame.Utilities;

namespace MillionaireGame.Services;

/// <summary>
/// Type of audio output being used
/// </summary>
public enum AudioOutputType
{
    WASAPI,
    DirectSound
}

/// <summary>
/// Mixes multiple audio sources and routes to output devices.
/// Supports automatic fallback from WASAPI to DirectSound for compatibility.
/// Monitors Windows default device changes for automatic reinitialization.
/// </summary>
public class AudioMixer : IDisposable
{
    private ISoundOut? _systemOutput;
    private MixingSampleSource? _mixer;
    private readonly object _lock = new();
    private bool _disposed = false;
    private float _masterVolume = 1.0f;
    private AudioOutputType _currentOutputType = AudioOutputType.WASAPI;
    private string? _currentDeviceId;
    private MMDeviceEnumerator? _deviceEnumerator;
    private MMNotificationClient? _notificationClient;
    private System.Threading.Timer? _healthCheckTimer;
    private volatile bool _healthCheckPaused = false;

    // Future: Add additional outputs here
    // private ISoundOut? _broadcastOutput;
    // private ISoundOut? _recordingOutput;

    /// <summary>
    /// Initialize the mixer with source streams and optional output device.
    /// Automatically falls back: WASAPI(device) → WASAPI(default) → DirectSound
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

                // Store device ID for reinitialization on default device changes
                _currentDeviceId = deviceId;

                // Try to initialize with hybrid fallback approach
                _systemOutput = TryInitializeOutput(deviceId, out _currentOutputType);

                if (_systemOutput == null)
                {
                    throw new InvalidOperationException("Failed to initialize audio output with any available method");
                }

                // Set up device change monitoring if using system default
                if (string.IsNullOrWhiteSpace(deviceId))
                {
                    StartDeviceMonitoring();
                }

                // Start health check to detect if device stops working after initialization
                StartHealthCheck();

                // Future: Initialize additional outputs here
                // _broadcastOutput = new WasapiOut(/* specific device */);
                // _broadcastOutput.Initialize(CreateTappedStream(_mixer));
            }
            catch (Exception ex)
            {
                GameConsole.Error($"[AudioMixer] Initialization failed completely: {ex.Message}");
                CleanupInternal();
                throw;
            }
        }
    }

    /// <summary>
    /// Attempts to initialize audio output with fallback chain:
    /// 1. WASAPI with specified device (verify it actually works)
    /// 2. WASAPI with system default (verify it actually works)
    /// 3. DirectSound
    /// </summary>
    private ISoundOut? TryInitializeOutput(string? deviceId, out AudioOutputType outputType)
    {
        // Step 1: Try WASAPI with specified device (or system default if null)
        try
        {
            ISoundOut output;
            
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                GameConsole.Info("[AudioMixer] Attempting WASAPI with system default device");
                output = new WasapiOut();
            }
            else
            {
                GameConsole.Info($"[AudioMixer] Attempting WASAPI with device: {deviceId}");
                var device = AudioDeviceManager.GetDeviceById(deviceId);
                
                if (device != null)
                {
                    output = new WasapiOut { Device = device };
                }
                else
                {
                    throw new InvalidOperationException($"Device not found: {deviceId}");
                }
            }

            output.Initialize(_mixer!.ToWaveSource());
            output.Volume = _masterVolume;
            
            // CRITICAL: Verify the device actually works by checking if audio flows CONTINUOUSLY
            // Some devices (wireless displays) work briefly then stop pulling audio
            var startPosition = _mixer!.LastReadPosition;
            var startTime = DateTime.Now;
            
            output.Play();
            
            // Phase 1: Wait for initial audio flow (up to 500ms)
            bool audioFlowing = false;
            for (int i = 0; i < 50; i++)
            {
                System.Threading.Thread.Sleep(10);
                if (_mixer!.LastReadPosition > startPosition)
                {
                    audioFlowing = true;
                    break;
                }
            }
            
            if (!audioFlowing)
            {
                output.Stop();
                output.Dispose();
                throw new InvalidOperationException($"Device accepted initialization but no initial audio flow after 500ms");
            }
            
            // Phase 2: Verify SUSTAINED audio flow (check for 1 second that position keeps advancing)
            GameConsole.Debug($"[AudioMixer] Initial audio detected, verifying sustained flow...");
            var lastCheckPosition = _mixer.LastReadPosition;
            var sustainedFlowChecks = 0;
            
            for (int i = 0; i < 10; i++) // Check 10 times over 1 second
            {
                System.Threading.Thread.Sleep(100);
                if (_mixer.LastReadPosition > lastCheckPosition)
                {
                    lastCheckPosition = _mixer.LastReadPosition;
                    sustainedFlowChecks++;
                }
            }
            
            if (sustainedFlowChecks < 8) // Must succeed at least 8 out of 10 checks
            {
                output.Stop();
                output.Dispose();
                throw new InvalidOperationException($"Device audio flow stopped after initial burst (only {sustainedFlowChecks}/10 checks passed)");
            }
            
            outputType = AudioOutputType.WASAPI;
            GameConsole.Info($"[AudioMixer] ✓ WASAPI verified - sustained audio flow confirmed after {(DateTime.Now - startTime).TotalMilliseconds:F0}ms ({sustainedFlowChecks}/10 checks)");
            return output;
        }
        catch (Exception ex)
        {
            GameConsole.Warn($"[AudioMixer] WASAPI initialization failed: {ex.GetType().Name} - {ex.Message}");
        }

        // Step 2: If specified device failed, try WASAPI with system default
        if (!string.IsNullOrWhiteSpace(deviceId))
        {
            try
            {
                GameConsole.Info("[AudioMixer] Attempting WASAPI fallback to system default");
                var output = new WasapiOut();
                output.Initialize(_mixer!.ToWaveSource());
                output.Volume = _masterVolume;
                
                var startPosition = _mixer!.LastReadPosition;
                var startTime = DateTime.Now;
                
                output.Play();
                
                bool audioFlowing = false;
                for (int i = 0; i < 50; i++)
                {
                    System.Threading.Thread.Sleep(10);
                    if (_mixer!.LastReadPosition > startPosition)
                    {
                        audioFlowing = true;
                        break;
                    }
                }
                
                if (!audioFlowing)
                {
                    output.Stop();
                    output.Dispose();
                    throw new InvalidOperationException($"System default accepted initialization but no audio flowed after 500ms");
                }
                
                outputType = AudioOutputType.WASAPI;
                GameConsole.Info($"[AudioMixer] ✓ WASAPI (system default) verified - audio flowing after {(DateTime.Now - startTime).TotalMilliseconds:F0}ms (keeping playback active)");
                return output;
            }
            catch (Exception ex)
            {
                GameConsole.Warn($"[AudioMixer] WASAPI system default failed: {ex.GetType().Name} - {ex.Message}");
            }
        }

        // Step 3: Final fallback to DirectSound
        try
        {
            GameConsole.Info("[AudioMixer] Attempting DirectSound fallback (compatibility mode)");
            var output = new DirectSoundOut(100); // 100ms latency for compatibility
            output.Initialize(_mixer!.ToWaveSource());
            output.Volume = _masterVolume;
            
            var startPosition = _mixer!.LastReadPosition;
            var startTime = DateTime.Now;
            
            output.Play();
            
            bool audioFlowing = false;
            for (int i = 0; i < 50; i++)
            {
                System.Threading.Thread.Sleep(10);
                if (_mixer!.LastReadPosition > startPosition)
                {
                    audioFlowing = true;
                    break;
                }
            }
            
            if (!audioFlowing)
            {
                output.Stop();
                output.Dispose();
                throw new InvalidOperationException("DirectSound failed to start audio flow after 500ms");
            }
            
            outputType = AudioOutputType.DirectSound;
            GameConsole.Warn($"[AudioMixer] ✓ DirectSound verified - audio flowing after {(DateTime.Now - startTime).TotalMilliseconds:F0}ms (compatibility mode, keeping playback active)");
            return output;
        }
        catch (Exception ex)
        {
            GameConsole.Error($"[AudioMixer] DirectSound fallback failed: {ex.GetType().Name} - {ex.Message}");
        }

        // All methods failed
        outputType = AudioOutputType.WASAPI;
        return null;
    }

    /// <summary>
    /// Start monitoring for Windows default device changes
    /// </summary>
    private void StartDeviceMonitoring()
    {
        try
        {
            _deviceEnumerator = new MMDeviceEnumerator();
            _notificationClient = new MMNotificationClient();
            
            _notificationClient.DefaultDeviceChanged += OnDefaultDeviceChanged;
            _deviceEnumerator.RegisterEndpointNotificationCallback(_notificationClient);
            
            if (Program.DebugMode)
            {
                GameConsole.Debug("[AudioMixer] Started monitoring for default device changes");
            }
        }
        catch (Exception ex)
        {
            GameConsole.Warn($"[AudioMixer] Failed to start device monitoring: {ex.Message}");
        }
    }

    /// <summary>
    /// Start periodic health check to detect if device stops working
    /// </summary>
    private void StartHealthCheck()
    {
        // Stop existing timer first to avoid duplicates
        _healthCheckTimer?.Dispose();
        _healthCheckTimer = null;
        
        // Check every 5 seconds if audio is flowing
        _healthCheckTimer = new System.Threading.Timer(HealthCheckCallback, null, 5000, 5000);
        
        if (Program.DebugMode)
        {
            GameConsole.Debug("[AudioMixer] Started audio flow health monitoring (5s intervals)");
        }
    }

    /// <summary>
    /// Health check callback - verifies audio is still flowing
    /// </summary>
    private void HealthCheckCallback(object? state)
    {
        try
        {
            if (_disposed || _healthCheckPaused)
                return;

            PlaybackState currentState;
            DateTime lastReadTime;
            
            // Safely access mixer and output state under lock
            lock (_lock)
            {
                if (_mixer == null || _systemOutput == null)
                    return;
                    
                lastReadTime = _mixer.LastReadTime;
                currentState = _systemOutput.PlaybackState;
            }
            
            var timeSinceLastRead = DateTime.Now - lastReadTime;
            
            // If no reads in the last 10 seconds AND playback state is not Playing, device may have stopped
            // (If PlaybackState is still Playing but no reads, device is just idle - this is normal)
            if (timeSinceLastRead.TotalSeconds > 10 && currentState != PlaybackState.Playing)
            {
                GameConsole.Warn($"[AudioMixer] Device health check FAILED - no audio reads for {(DateTime.Now - lastReadTime).TotalSeconds:F1}s, PlaybackState={currentState}");
                GameConsole.Warn($"[AudioMixer] Device appears to have stopped working, attempting fallback...");
                
                // Attempt fallback in background thread
                Task.Run(() =>
                {
                    try
                    {
                        lock (_lock)
                        {
                            if (_disposed || _mixer == null)
                                return;

                            // Only fallback if using WASAPI - DirectSound is already the final fallback
                            if (_currentOutputType == AudioOutputType.WASAPI)
                            {
                                GameConsole.Info("[AudioMixer] Attempting DirectSound fallback due to device failure...");
                                
                                // Dispose failed WASAPI output
                                _systemOutput?.Stop();
                                _systemOutput?.Dispose();
                                _systemOutput = null;

                                // Try DirectSound
                                try
                                {
                                    var output = new DirectSoundOut(100);
                                    output.Initialize(_mixer.ToWaveSource());
                                    output.Volume = _masterVolume;
                                    output.Play();
                                    
                                    _systemOutput = output;
                                    _currentOutputType = AudioOutputType.DirectSound;
                                    
                                    GameConsole.Warn("[AudioMixer] ✓ Successfully fell back to DirectSound after device failure");
                                }
                                catch (Exception ex)
                                {
                                    GameConsole.Error($"[AudioMixer] DirectSound fallback failed: {ex.Message}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        GameConsole.Error($"[AudioMixer] Health check recovery failed: {ex.Message}");
                    }
                });
            }
        }
        catch (Exception ex)
        {
            GameConsole.Error($"[AudioMixer] Health check error: {ex.Message}");
        }
    }

    /// <summary>
    /// Handle Windows default device changes
    /// </summary>
    private void OnDefaultDeviceChanged(object? sender, DefaultDeviceChangedEventArgs e)
    {
        // CSCore/WASAPI handles device changes natively - no manual reinitialization needed
        if (Program.DebugMode)
        {
            GameConsole.Debug($"[AudioMixer] Default device changed to: {e.DeviceId}");
        }
    }

    /// <summary>
    /// Change the audio output device without disposing the mixer or channel connections.
    /// Uses hybrid fallback approach: WASAPI(device) → WASAPI(default) → DirectSound
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

            // Pause health check during device change
            _healthCheckPaused = true;

            // CRITICAL: Check if mixer exists - if not, do full initialization
            if (_mixer == null)
            {
                GameConsole.Warn("[AudioMixer] Mixer not initialized, performing full initialization");
                Initialize(musicSource, effectsSource, deviceId);
                _healthCheckPaused = false;
                return;
            }

            // Remember playback state
            bool wasPlaying = _systemOutput?.PlaybackState == PlaybackState.Playing;
            
            GameConsole.Info($"[AudioMixer] Changing device (wasPlaying: {wasPlaying})");

            try
            {
                // Stop and dispose current output
                _systemOutput?.Stop();
                _systemOutput?.Dispose();
                _systemOutput = null;

                // Store new device ID
                var oldDeviceId = _currentDeviceId;
                _currentDeviceId = deviceId;

                // Stop monitoring if switching from system default to specific device
                if (string.IsNullOrWhiteSpace(oldDeviceId) && !string.IsNullOrWhiteSpace(deviceId))
                {
                    StopDeviceMonitoring();
                }

                // Try to initialize with new device using fallback chain
                _systemOutput = TryInitializeOutput(deviceId, out _currentOutputType);

                if (_systemOutput == null)
                {
                    throw new InvalidOperationException("Failed to initialize audio output with any available method");
                }

                // Start monitoring if switching to system default
                if (string.IsNullOrWhiteSpace(deviceId))
                {
                    StartDeviceMonitoring();
                }

                // Always restart playback (even if nothing was playing, mixer needs to be active)
                Start();
                
                // Restart health check monitoring
                StartHealthCheck();
                
                // Resume health check
                _healthCheckPaused = false;
                
                GameConsole.Info($"[AudioMixer] ✓ Device change complete - using {_currentOutputType}");
            }
            catch (Exception ex)
            {
                _healthCheckPaused = false; // Resume health check even on error
                GameConsole.Error($"[AudioMixer] Device change failed completely: {ex.Message}");
                // Try to recover with system default
                try
                {
                    _systemOutput?.Dispose();
                    _systemOutput = new WasapiOut();
                    _systemOutput.Initialize(_mixer.ToWaveSource());
                    _systemOutput.Volume = _masterVolume;
                    Start();
                    _currentOutputType = AudioOutputType.WASAPI;
                    GameConsole.Warn("[AudioMixer] Recovered with system default WASAPI");
                }
                catch
                {
                    // Last resort: DirectSound
                    try
                    {
                        _systemOutput?.Dispose();
                        _systemOutput = new DirectSoundOut(100);
                        _systemOutput.Initialize(_mixer.ToWaveSource());
                        _systemOutput.Volume = _masterVolume;
                        Start();
                        _currentOutputType = AudioOutputType.DirectSound;
                        GameConsole.Warn("[AudioMixer] Recovered with DirectSound");
                    }
                    catch (Exception recoveryEx)
                    {
                        GameConsole.Error($"[AudioMixer] Complete recovery failure: {recoveryEx.Message}");
                        throw;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Stop monitoring for default device changes
    /// </summary>
    private void StopDeviceMonitoring()
    {
        try
        {
            if (_notificationClient != null && _deviceEnumerator != null)
            {
                _notificationClient.DefaultDeviceChanged -= OnDefaultDeviceChanged;
                _deviceEnumerator.UnregisterEndpointNotificationCallback(_notificationClient);
                
                if (Program.DebugMode)
                {
                    GameConsole.Debug("[AudioMixer] Stopped monitoring for default device changes");
                }
            }
        }
        catch (Exception ex)
        {
            GameConsole.Warn($"[AudioMixer] Error stopping device monitoring: {ex.Message}");
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
        // Stop health check
        _healthCheckTimer?.Dispose();
        _healthCheckTimer = null;
        
        // Stop device monitoring
        StopDeviceMonitoring();
        
        try
        {
            _systemOutput?.Stop();
            _systemOutput?.Dispose();
            _systemOutput = null;
        }
        catch { }

        try
        {
            _deviceEnumerator?.Dispose();
            _deviceEnumerator = null;
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
    private long _lastReadPosition;
    private DateTime _lastReadTime = DateTime.MinValue;

    public MixingSampleSource(WaveFormat waveFormat)
    {
        _waveFormat = waveFormat ?? throw new ArgumentNullException(nameof(waveFormat));
    }

    /// <summary>
    /// Gets the position of the last read operation (used for verification)
    /// </summary>
    public long LastReadPosition => _lastReadPosition;

    /// <summary>
    /// Gets the time of the last read operation (used for verification)
    /// </summary>
    public DateTime LastReadTime => _lastReadTime;

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
        _lastReadPosition = _position;
        _lastReadTime = DateTime.Now;
        
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
