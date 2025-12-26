using CSCore;
using MillionaireGame.Utilities;
using System;

namespace MillionaireGame.Services
{
    /// <summary>
    /// ISampleSource wrapper that detects silence and applies automatic fadeout.
    /// Monitors audio amplitude during playback and stops early when sustained silence is detected,
    /// preventing unnecessary playback of silent tails in audio files.
    /// </summary>
    /// <remarks>
    /// Key Features:
    /// - Monitors amplitude during Read() calls
    /// - Detects threshold crossing (e.g., -60dB)
    /// - Requires sustained silence (e.g., 100ms) to prevent false triggers
    /// - Applies automatic fadeout (20ms default) to prevent DC pops/clicks
    /// - Fires SilenceDetected event when silence is confirmed
    /// - Returns 0 after fadeout completes, signaling completion
    /// </remarks>
    public class SilenceDetectorSource : ISampleSource
    {
        private readonly ISampleSource _source;
        private readonly float _thresholdAmplitude;
        private readonly int _silenceDurationSamples;
        private readonly int _fadeoutSamples;
        private int _consecutiveSilentSamples = 0;
        private bool _silenceDetected = false;
        private bool _fadingOut = false;
        private int _fadeoutPosition = 0;

        /// <summary>
        /// Fired when sustained silence is detected and fadeout begins
        /// </summary>
        public event EventHandler? SilenceDetected;

        /// <summary>
        /// Initializes a new instance of the SilenceDetectorSource class
        /// </summary>
        /// <param name="source">The audio source to monitor</param>
        /// <param name="thresholdDb">Silence threshold in dB (e.g., -60dB = very quiet)</param>
        /// <param name="silenceDurationMs">Duration in milliseconds that silence must be sustained</param>
        /// <param name="fadeoutDurationMs">Fadeout duration in milliseconds to prevent DC pops (default: 20ms)</param>
        public SilenceDetectorSource(ISampleSource source, float thresholdDb, int silenceDurationMs, int fadeoutDurationMs = 20)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));

            // Convert dB to linear amplitude
            // Formula: amplitude = 10^(dB/20)
            // Examples: -60dB = 0.001, -40dB = 0.01, -20dB = 0.1
            _thresholdAmplitude = (float)Math.Pow(10, thresholdDb / 20.0);

            // Convert milliseconds to sample count
            _silenceDurationSamples = (int)(silenceDurationMs * source.WaveFormat.SampleRate / 1000.0);
            _fadeoutSamples = (int)(fadeoutDurationMs * source.WaveFormat.SampleRate / 1000.0);

            if (Program.DebugMode)
            {
                GameConsole.Debug(
                    $"[SilenceDetector] Created: threshold={thresholdDb}dB ({_thresholdAmplitude:F6}), " +
                    $"duration={silenceDurationMs}ms ({_silenceDurationSamples} samples), " +
                    $"fadeout={fadeoutDurationMs}ms ({_fadeoutSamples} samples)"
                );
            }
        }

        /// <summary>
        /// Reads audio samples and monitors for silence
        /// </summary>
        public int Read(float[] buffer, int offset, int count)
        {
            // If silence already detected and fadeout complete, stop reading
            if (_silenceDetected && !_fadingOut)
            {
                return 0;
            }

            // Read from source
            int read = _source.Read(buffer, offset, count);

            if (read == 0)
            {
                return 0;
            }

            // If currently fading out, apply gain ramp
            if (_fadingOut)
            {
                int samplesProcessed = 0;
                bool fadeoutJustCompleted = false;
                
                for (int i = offset; i < offset + read; i++)
                {
                    // Check if fadeout complete before processing this sample
                    if (_fadeoutPosition >= _fadeoutSamples)
                    {
                        // Fadeout complete - signal EOF immediately
                        _silenceDetected = true;
                        _fadingOut = false;
                        fadeoutJustCompleted = true;
                        
                        // Log AFTER the loop, outside audio thread hot path
                        break;
                    }

                    // Linear fadeout from 1.0 to 0.0 (with safety check)
                    float fadeGain = (_fadeoutSamples > 0) 
                        ? 1.0f - ((float)_fadeoutPosition / _fadeoutSamples)
                        : 0.0f;
                    buffer[i] *= Math.Max(0, fadeGain);
                    _fadeoutPosition++;
                    samplesProcessed++;
                }
                
                // Log completion AFTER exiting the loop
                if (fadeoutJustCompleted && Program.DebugMode)
                {
                    float fadeoutMs = (_fadeoutSamples * 1000.0f) / _source.WaveFormat.SampleRate;
                    GameConsole.Info($"[SilenceDetector] Fadeout complete ({fadeoutMs:F0}ms), stopping playback");
                }
                
                return fadeoutJustCompleted ? 0 : read;
            }

            // Analyze amplitude of samples read
            float maxAmplitude = 0;
            for (int i = offset; i < offset + read; i++)
            {
                float absValue = Math.Abs(buffer[i]);
                if (absValue > maxAmplitude)
                {
                    maxAmplitude = absValue;
                }
            }

            // Check if below threshold (silent)
            if (maxAmplitude < _thresholdAmplitude)
            {
                _consecutiveSilentSamples += read;

                // Check if silence sustained long enough
                if (_consecutiveSilentSamples >= _silenceDurationSamples && !_fadingOut)
                {
                    // Start fadeout instead of abrupt stop (prevents DC pops!)
                    _fadingOut = true;
                    _fadeoutPosition = 0;
                    
                    // Log OUTSIDE the hot path - safe to call here as we only trigger once
                    if (Program.DebugMode)
                    {
                        float silenceMs = (_silenceDurationSamples * 1000.0f) / _source.WaveFormat.SampleRate;
                        float thresholdDb = 20.0f * (float)Math.Log10(_thresholdAmplitude);
                        float fadeoutMs = (_fadeoutSamples * 1000.0f) / _source.WaveFormat.SampleRate;
                        GameConsole.Info($"[SilenceDetector] Silence detected ({silenceMs:F0}ms at {thresholdDb:F1} dB), starting {fadeoutMs:F0}ms fadeout");
                    }
                    
                    // Notify listeners asynchronously (don't block audio thread)
                    Task.Run(() => SilenceDetected?.Invoke(this, EventArgs.Empty));
                }
            }
            else
            {
                // Reset counter if audio detected (above threshold)
                _consecutiveSilentSamples = 0;
            }

            return read;
        }

        /// <summary>
        /// Gets or sets the position within the audio source
        /// </summary>
        public long Position
        {
            get => _source.Position;
            set
            {
                _source.Position = value;
                // Reset silence detection state when seeking
                _consecutiveSilentSamples = 0;
                _silenceDetected = false;
                _fadingOut = false;
                _fadeoutPosition = 0;
            }
        }

        /// <summary>
        /// Gets the length of the audio source
        /// </summary>
        public long Length => _source.Length;

        /// <summary>
        /// Gets the wave format of the audio source
        /// </summary>
        public WaveFormat WaveFormat => _source.WaveFormat;

        /// <summary>
        /// Gets whether the source supports seeking
        /// </summary>
        public bool CanSeek => _source.CanSeek;

        /// <summary>
        /// Disposes the underlying audio source
        /// </summary>
        public void Dispose()
        {
            _source?.Dispose();
        }
    }
}
