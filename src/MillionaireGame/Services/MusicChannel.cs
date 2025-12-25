using CSCore;
using CSCore.Codecs;
using CSCore.SoundOut;
using CSCore.Streams;
using MillionaireGame.Utilities;

namespace MillionaireGame.Services;

/// <summary>
/// Manages looping background music playback using CSCore.
/// Handles bed music that plays continuously across multiple questions.
/// </summary>
public class MusicChannel : IDisposable
{
    private ISoundOut? _soundOut;
    private IWaveSource? _waveSource;
    private LoopStream? _loopStream;
    private readonly object _lock = new();
    private bool _disposed = false;
    private string? _currentFile;
    private float _volume = 1.0f;

    /// <summary>
    /// Gets whether music is currently playing
    /// </summary>
    public bool IsPlaying
    {
        get
        {
            lock (_lock)
            {
                return _soundOut?.PlaybackState == PlaybackState.Playing;
            }
        }
    }

    /// <summary>
    /// Gets the currently playing music file path
    /// </summary>
    public string? CurrentFile
    {
        get
        {
            lock (_lock)
            {
                return _currentFile;
            }
        }
    }

    /// <summary>
    /// Play a music file with optional looping
    /// </summary>
    public void PlayMusic(string filePath, bool loop = false)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            if (Program.DebugMode)
            {
                GameConsole.Warn("[MusicChannel] PlayMusic called with null/empty path");
            }
            return;
        }

        if (!File.Exists(filePath))
        {
            if (Program.DebugMode)
            {
                GameConsole.Error($"[MusicChannel] File not found: {filePath}");
            }
            return;
        }

        try
        {
            lock (_lock)
            {
                // Stop and dispose existing playback
                StopInternal();

                if (Program.DebugMode)
                {
                    GameConsole.Debug($"[MusicChannel] Loading: {Path.GetFileName(filePath)} (loop: {loop})");
                }

                // Load audio file
                _waveSource = CodecFactory.Instance.GetCodec(filePath);

                if (loop)
                {
                    // Wrap in loop stream for seamless looping
                    _loopStream = new LoopStream(_waveSource);
                    _loopStream.EnableLooping = true;

                    // Create sound output with loop stream
                    _soundOut = new WasapiOut();
                    _soundOut.Initialize(_loopStream);
                }
                else
                {
                    // Direct playback without looping
                    _soundOut = new WasapiOut();
                    _soundOut.Initialize(_waveSource);
                }

                // Set volume
                _soundOut.Volume = _volume;

                // Set up stopped event for cleanup
                _soundOut.Stopped += OnPlaybackStopped;

                // Store current file
                _currentFile = filePath;

                // Start playback
                _soundOut.Play();

                if (Program.DebugMode)
                {
                    GameConsole.Debug($"[MusicChannel] Playing: {Path.GetFileName(filePath)}");
                }
            }
        }
        catch (Exception ex)
        {
            GameConsole.Error($"[MusicChannel] Error playing music: {ex.Message}");
            CleanupResources();
        }
    }

    /// <summary>
    /// Stop currently playing music
    /// </summary>
    public void StopMusic()
    {
        lock (_lock)
        {
            StopInternal();
        }
    }

    /// <summary>
    /// Set music volume (0.0 to 1.0)
    /// </summary>
    public void SetVolume(float volume)
    {
        volume = Math.Clamp(volume, 0.0f, 1.0f);

        lock (_lock)
        {
            _volume = volume;
            if (_soundOut != null)
            {
                _soundOut.Volume = volume;
            }
        }
    }

    /// <summary>
    /// Fade out music over specified duration (milliseconds)
    /// </summary>
    public async Task FadeOut(int durationMs)
    {
        if (durationMs <= 0)
        {
            StopMusic();
            return;
        }

        lock (_lock)
        {
            if (_soundOut == null || _soundOut.PlaybackState != PlaybackState.Playing)
            {
                return;
            }
        }

        const int steps = 20;
        int stepDelay = durationMs / steps;
        float startVolume = _volume;
        float volumeStep = startVolume / steps;

        for (int i = 0; i < steps; i++)
        {
            lock (_lock)
            {
                if (_soundOut == null || _soundOut.PlaybackState != PlaybackState.Playing)
                {
                    break;
                }

                float newVolume = startVolume - (volumeStep * (i + 1));
                _soundOut.Volume = Math.Max(0, newVolume);
            }

            await Task.Delay(stepDelay);
        }

        StopMusic();
        SetVolume(startVolume); // Restore original volume for next playback
    }

    /// <summary>
    /// Internal stop method - must be called within lock
    /// </summary>
    private void StopInternal()
    {
        if (_soundOut != null)
        {
            try
            {
                if (_soundOut.PlaybackState == PlaybackState.Playing)
                {
                    _soundOut.Stop();
                }

                _soundOut.Stopped -= OnPlaybackStopped;
            }
            catch (Exception ex)
            {
                if (Program.DebugMode)
                {
                    GameConsole.Debug($"[MusicChannel] Error stopping: {ex.Message}");
                }
            }
        }

        CleanupResources();
        _currentFile = null;
    }

    /// <summary>
    /// Cleanup audio resources
    /// </summary>
    private void CleanupResources()
    {
        try
        {
            _soundOut?.Dispose();
            _soundOut = null;
        }
        catch { }

        try
        {
            _loopStream?.Dispose();
            _loopStream = null;
        }
        catch { }

        try
        {
            _waveSource?.Dispose();
            _waveSource = null;
        }
        catch { }
    }

    /// <summary>
    /// Handle playback stopped event
    /// </summary>
    private void OnPlaybackStopped(object? sender, PlaybackStoppedEventArgs e)
    {
        if (Program.DebugMode)
        {
            GameConsole.Debug($"[MusicChannel] Playback stopped: {Path.GetFileName(_currentFile ?? "unknown")}");
        }

        // Cleanup resources asynchronously to avoid blocking
        Task.Run(() =>
        {
            lock (_lock)
            {
                if (!_disposed)
                {
                    CleanupResources();
                    _currentFile = null;
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
            StopInternal();
        }

        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Stream wrapper that enables seamless looping of audio
/// </summary>
public class LoopStream : IWaveSource
{
    private readonly IWaveSource _source;
    private bool _enableLooping;

    public LoopStream(IWaveSource source)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
    }

    public bool EnableLooping
    {
        get => _enableLooping;
        set => _enableLooping = value;
    }

    public bool CanSeek => _source.CanSeek;

    public WaveFormat WaveFormat => _source.WaveFormat;

    public long Position
    {
        get => _source.Position;
        set => _source.Position = value;
    }

    public long Length => _source.Length;

    public int Read(byte[] buffer, int offset, int count)
    {
        int totalBytesRead = 0;

        while (totalBytesRead < count)
        {
            int bytesRead = _source.Read(buffer, offset + totalBytesRead, count - totalBytesRead);

            if (bytesRead == 0)
            {
                // End of stream reached
                if (_enableLooping && _source.CanSeek)
                {
                    // Loop back to beginning
                    _source.Position = 0;
                }
                else
                {
                    // No looping - stop here
                    break;
                }
            }

            totalBytesRead += bytesRead;
        }

        return totalBytesRead;
    }

    public void Dispose()
    {
        _source?.Dispose();
    }
}
