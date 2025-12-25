using CSCore;
using CSCore.Codecs;
using CSCore.Codecs.MP3;
using CSCore.MediaFoundation;
using CSCore.SoundOut;
using CSCore.Streams;
using CSCore.Streams.SampleConverter;
using MillionaireGame.Utilities;

namespace MillionaireGame.Services;

/// <summary>
/// Manages looping background music playback using CSCore.
/// Handles bed music that plays continuously across multiple questions.
/// Provides an ISampleSource stream for mixer integration.
/// </summary>
public class MusicChannel : IDisposable
{
    private IWaveSource? _waveSource;
    private LoopStream? _loopStream;
    private ISampleSource? _currentMusicSource;
    private readonly MusicSourceProvider _sourceProvider;
    private readonly object _lock = new();
    private bool _disposed = false;
    private string? _currentFile;
    private float _volume = 1.0f;

    public MusicChannel()
    {
        _sourceProvider = new MusicSourceProvider();
    }

    /// <summary>
    /// Gets whether music is currently playing
    /// </summary>
    public bool IsPlaying
    {
        get
        {
            lock (_lock)
            {
                return _currentMusicSource != null;
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
    /// Gets the output stream for mixer integration
    /// </summary>
    public ISampleSource GetOutputStream()
    {
        return _sourceProvider;
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
                    GameConsole.Debug($"[MusicChannel] Starting playback: {Path.GetFileName(filePath)} (loop: {loop})");
                    GameConsole.Debug($"[MusicChannel] Full path: {filePath}");
                }

                // Load audio file - use MediaFoundation for MP3 files
                if (filePath.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        _waveSource = new MediaFoundationDecoder(filePath);
                        if (Program.DebugMode)
                        {
                            GameConsole.Debug($"[MusicChannel] Using MediaFoundationDecoder for MP3");
                        }
                    }
                    catch (Exception ex)
                    {
                        if (Program.DebugMode)
                        {
                            GameConsole.Warn($"[MusicChannel] MediaFoundation failed ({ex.Message}), falling back to DmoMp3Decoder");
                        }
                        _waveSource = new DmoMp3Decoder(filePath);
                    }
                }
                else
                {
                    _waveSource = CodecFactory.Instance.GetCodec(filePath);
                }

                IWaveSource sourceToUse;
                if (loop)
                {
                    // Wrap in loop stream for seamless looping
                    _loopStream = new LoopStream(_waveSource);
                    _loopStream.EnableLooping = true;
                    sourceToUse = _loopStream;
                }
                else
                {
                    // Direct playback without looping
                    sourceToUse = _waveSource;
                }

                // Convert to ISampleSource for mixer
                _currentMusicSource = sourceToUse.ToSampleSource();
                
                // Apply volume
                var volumeSource = new VolumeSource(_currentMusicSource);
                volumeSource.Volume = _volume;
                _currentMusicSource = volumeSource;

                // Set in provider
                _sourceProvider.SetSource(_currentMusicSource);

                // Store current file
                _currentFile = filePath;

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
            if (_currentMusicSource is VolumeSource volumeSource)
            {
                volumeSource.Volume = volume;
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

        VolumeSource? volumeSource = null;
        lock (_lock)
        {
            if (_currentMusicSource == null)
            {
                return;
            }
            volumeSource = _currentMusicSource as VolumeSource;
            if (volumeSource == null)
            {
                // Can't fade without volume control
                StopMusic();
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
                if (_currentMusicSource == null || volumeSource == null)
                {
                    break;
                }

                float newVolume = startVolume - (volumeStep * (i + 1));
                volumeSource.Volume = Math.Max(0, newVolume);
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
        _sourceProvider.SetSource(null); // Clear source provider (outputs silence)
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
            _currentMusicSource?.Dispose();
            _currentMusicSource = null;
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
/// Provides a continuous ISampleSource stream that outputs current music or silence
/// </summary>
internal class MusicSourceProvider : ISampleSource
{
    private ISampleSource? _currentSource;
    private readonly object _lock = new();
    private long _position;

    public void SetSource(ISampleSource? source)
    {
        lock (_lock)
        {
            _currentSource = source;
        }
    }

    public bool CanSeek => false;
    public WaveFormat WaveFormat => new WaveFormat(44100, 32, 2, AudioEncoding.IeeeFloat); // Float format for ISampleSource
    public long Position
    {
        get => _position;
        set => throw new NotSupportedException("MusicSourceProvider does not support seeking");
    }
    public long Length => 0; // Infinite length for continuous stream

    public int Read(float[] buffer, int offset, int count)
    {
        lock (_lock)
        {
            if (_currentSource == null)
            {
                // No music playing - output silence
                Array.Clear(buffer, offset, count);
                _position += count;
                return count;
            }

            // Output current music
            int read = _currentSource.Read(buffer, offset, count);
            _position += read;
            return read;
        }
    }

    public void Dispose()
    {
        // Don't dispose currentSource - managed by MusicChannel
    }
}

/// <summary>
/// Applies volume control to an ISampleSource
/// </summary>
public class VolumeSource : ISampleSource
{
    private readonly ISampleSource _source;
    private float _volume = 1.0f;

    public VolumeSource(ISampleSource source)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
    }

    public float Volume
    {
        get => _volume;
        set => _volume = Math.Clamp(value, 0.0f, 1.0f);
    }

    public bool CanSeek => _source.CanSeek;
    public WaveFormat WaveFormat => _source.WaveFormat;
    public long Position
    {
        get => _source.Position;
        set => _source.Position = value;
    }
    public long Length => _source.Length;

    public int Read(float[] buffer, int offset, int count)
    {
        int read = _source.Read(buffer, offset, count);
        
        if (_volume != 1.0f)
        {
            for (int i = 0; i < read; i++)
            {
                buffer[offset + i] *= _volume;
            }
        }

        return read;
    }

    public void Dispose()
    {
        _source?.Dispose();
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
