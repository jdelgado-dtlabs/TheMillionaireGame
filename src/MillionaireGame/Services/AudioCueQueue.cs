using CSCore;
using CSCore.Codecs;
using CSCore.Codecs.MP3;
using CSCore.MediaFoundation;
using MillionaireGame.Core.Settings;
using System;
using System.Collections.Generic;

using MillionaireGame.Utilities;

namespace MillionaireGame.Services
{
    /// <summary>
    /// Queue manager for sequential audio playback with automatic crossfading and integrated silence detection.
    /// Manages FIFO queue of audio files and seamlessly transitions between them
    /// using configurable crossfade durations. Automatically detects silence and triggers crossfades.
    /// </summary>
    /// <remarks>
    /// Key Features:
    /// - FIFO queue for normal priority sounds
    /// - Immediate interrupt capability for high priority
    /// - Automatic crossfade between queued sounds (50ms default)
    /// - Integrated silence detection (auto-crossfade when < -40dB for 100ms)
    /// - Configurable crossfade duration
    /// - Queue limit enforcement (10 default)
    /// - Preview: count + estimated time
    /// </remarks>
    public class AudioCueQueue : ISampleSource
    {
        private readonly Queue<AudioCue> _normalQueue = new();
        private readonly int _queueLimit;
        private readonly int _crossfadeDurationSamples;
        private readonly SilenceDetectionSettings? _silenceSettings;
        private readonly object _lock = new object(); // Thread safety for queue operations
        private AudioCue? _currentCue;
        private AudioCue? _nextCue;
        private bool _crossfading = false;
        private int _crossfadePosition = 0;
        private WaveFormat _waveFormat;
        
        // Silence detection state
        private int _silenceSampleCount = 0;
        private int _silenceDurationSamples = 0;
        private float _silenceThresholdAmplitude = 0f;
        private int _currentCueSamplesProcessed = 0;
        private int _initialDelaySamples = 0;
        
        // Per-cue threshold overrides (maps cue to custom threshold amplitude)
        private readonly Dictionary<AudioCue, float> _cueThresholdOverrides = new();
        
        // Manual fadeout state
        private bool _fadingOut = false;
        private int _fadeoutPosition = 0;
        private int _fadeoutDurationSamples = 0;

        /// <summary>
        /// Gets the number of sounds currently queued (waiting)
        /// </summary>
        public int QueueCount { get { lock (_lock) return _normalQueue.Count; } }

        /// <summary>
        /// Gets the total number of sounds (playing + next + queued)
        /// </summary>
        public int TotalSoundCount 
        { 
            get 
            { 
                lock (_lock) 
                {
                    int count = _normalQueue.Count;
                    if (_currentCue != null) count++;
                    if (_nextCue != null && !_crossfading) count++; // Don't double-count during crossfade
                    return count;
                }
            } 
        }

        /// <summary>
        /// Gets whether the queue is currently playing audio
        /// </summary>
        public bool IsPlaying { get { lock (_lock) return _currentCue != null; } }

        /// <summary>
        /// Gets whether a crossfade is currently in progress
        /// </summary>
        public bool IsCrossfading { get { lock (_lock) return _crossfading; } }

        /// <summary>
        /// Initializes a new instance of the AudioCueQueue class
        /// </summary>
        /// <param name="waveFormat">The wave format for audio playback</param>
        /// <param name="crossfadeDurationMs">Duration of crossfades in milliseconds (default: 200ms)</param>
        /// <param name="queueLimit">Maximum number of sounds that can be queued (default: 10)</param>
        /// <param name="silenceSettings">Optional silence detection settings to apply to queued audio</param>
        public AudioCueQueue(WaveFormat waveFormat, int crossfadeDurationMs = 50, int queueLimit = 10, 
            SilenceDetectionSettings? silenceSettings = null)
        {
            _waveFormat = waveFormat ?? throw new ArgumentNullException(nameof(waveFormat));
            _queueLimit = queueLimit;
            _crossfadeDurationSamples = (int)(crossfadeDurationMs * waveFormat.SampleRate / 1000.0);
            _silenceSettings = silenceSettings ?? new SilenceDetectionSettings();
            
            // Calculate silence detection thresholds
            _silenceDurationSamples = (int)(_silenceSettings.SilenceDurationMs * waveFormat.SampleRate / 1000.0);
            _silenceThresholdAmplitude = (float)Math.Pow(10, _silenceSettings.ThresholdDb / 20.0);
            _initialDelaySamples = (int)(_silenceSettings.InitialDelayMs * waveFormat.SampleRate / 1000.0);

            if (Program.DebugMode)
            {
                GameConsole.Debug(
                    $"[AudioCueQueue] Created: crossfade={crossfadeDurationMs}ms ({_crossfadeDurationSamples} samples), " +
                    $"queueLimit={queueLimit}, silenceThreshold={_silenceSettings.ThresholdDb}dB ({_silenceThresholdAmplitude:F6}), " +
                    $"silenceDuration={_silenceSettings.SilenceDurationMs}ms ({_silenceDurationSamples} samples), " +
                    $"initialDelay={_silenceSettings.InitialDelayMs}ms ({_initialDelaySamples} samples)"
                );
            }
        }

        /// <summary>
        /// Queues an audio file for playback
        /// </summary>
        /// <param name="filePath">Path to the audio file</param>
        /// <param name="priority">Priority level (Normal or Immediate)</param>
        /// <param name="customThresholdDb">Optional custom silence detection threshold in dB (e.g., -30 for lights down sounds)</param>
        /// <returns>True if queued successfully, false if queue is full</returns>
        public bool QueueAudio(string filePath, AudioPriority priority = AudioPriority.Normal, double? customThresholdDb = null)
        {
            lock (_lock)
            {
                if (priority == AudioPriority.Normal && _normalQueue.Count >= _queueLimit)
                {
                    if (Program.DebugMode)
                    {
                        GameConsole.Warn(
                            $"[AudioCueQueue] Queue full ({_queueLimit}), rejecting: {System.IO.Path.GetFileName(filePath)}"
                        );
                    }
                    return false;
                }

                try
                {
                    var cue = new AudioCue(filePath, priority, _waveFormat, _silenceSettings);
                    
                    // Store custom threshold override if provided
                    if (customThresholdDb.HasValue)
                    {
                        float customThresholdAmplitude = (float)Math.Pow(10, customThresholdDb.Value / 20.0);
                        _cueThresholdOverrides[cue] = customThresholdAmplitude;
                        
                        if (Program.DebugMode)
                        {
                            GameConsole.Debug(
                                $"[AudioCueQueue] Custom threshold for {System.IO.Path.GetFileName(filePath)}: {customThresholdDb.Value}dB ({customThresholdAmplitude:F6})"
                            );
                        }
                    }

                    if (priority == AudioPriority.Immediate)
                    {
                        if (Program.DebugMode)
                        {
                            GameConsole.Warn(
                                $"[AudioCueQueue] IMMEDIATE priority: {System.IO.Path.GetFileName(filePath)}, interrupting current"
                            );
                        }

                        // Interrupt current, start crossfade immediately
                        if (_currentCue != null)
                        {
                            _nextCue = cue;
                            _crossfading = true;
                            _crossfadePosition = 0;
                            _silenceSampleCount = 0; // Reset for crossfade
                        }
                        else
                        {
                            _currentCue = cue;
                            _silenceSampleCount = 0; // Reset silence detection for new sound
                            _currentCueSamplesProcessed = 0; // Reset initial delay
                        }
                    }
                    else
                    {
                        _normalQueue.Enqueue(cue);

                        if (Program.DebugMode)
                        {
                            GameConsole.Debug(
                                $"[AudioCueQueue] Queued: {System.IO.Path.GetFileName(filePath)}, queue size: {_normalQueue.Count}"
                            );
                        }

                        // If no current cue, start immediately
                        if (_currentCue == null)
                        {
                            _currentCue = _normalQueue.Dequeue();
                            _silenceSampleCount = 0; // Reset silence detection for new sound
                            _currentCueSamplesProcessed = 0; // Reset initial delay
                            if (Program.DebugMode)
                            {
                                GameConsole.Info(
                                    $"[AudioCueQueue] Starting playback: {System.IO.Path.GetFileName(_currentCue.FilePath)}"
                                );
                            }
                        }
                        // If queue has items and not already crossfading, prepare next transition
                        else if (_normalQueue.Count > 0 && !_crossfading && _nextCue == null)
                        {
                            _nextCue = _normalQueue.Dequeue();
                            if (Program.DebugMode)
                            {
                                GameConsole.Debug(
                                    $"[AudioCueQueue] Prepared next: {System.IO.Path.GetFileName(_nextCue.FilePath)}"
                                );
                            }
                        }
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    if (Program.DebugMode)
                    {
                        GameConsole.Error(
                            $"[AudioCueQueue] ERROR loading {System.IO.Path.GetFileName(filePath)}: {ex.Message}"
                        );
                    }
                    return false;
                }
            }
        }

        /// <summary>
        /// Clears all queued sounds (does not stop current playback)
        /// </summary>
        public void ClearQueue()
        {
            lock (_lock)
            {
                ClearQueueInternal();
            }
        }

        /// <summary>
        /// Internal queue clearing without lock (must be called within lock)
        /// </summary>
        private void ClearQueueInternal()
        {
            _normalQueue.Clear();
            _nextCue?.Dispose();
            _nextCue = null;
            _crossfading = false;
            _crossfadePosition = 0;

            if (Program.DebugMode)
            {
                GameConsole.Warn("[AudioCueQueue] Queue cleared");
            }
        }

        /// <summary>
        /// Stops all playback and clears the queue (abrupt stop)
        /// </summary>
        public void Stop()
        {
            lock (_lock)
            {
                if (_currentCue != null)
                {
                    _cueThresholdOverrides.Remove(_currentCue); // Clean up threshold override
                    _currentCue.Dispose();
                    _currentCue = null;
                }
                _fadingOut = false;
                _fadeoutPosition = 0;
                ClearQueueInternal(); // Use internal version to avoid nested lock

                if (Program.DebugMode)
                {
                    GameConsole.Warn("[AudioCueQueue] Stopped (abrupt)");
                }
            }
        }

        /// <summary>
        /// Fades out current audio and clears queue (smooth stop)
        /// </summary>
        /// <param name="fadeoutDurationMs">Duration of fadeout in milliseconds (default: 50ms, minimum: 10ms)</param>
        public void StopWithFadeout(int fadeoutDurationMs = 50)
        {
            lock (_lock)
            {
                if (_currentCue == null)
                {
                    // Nothing playing, just clear queue
                    ClearQueueInternal();
                    return;
                }

                // Ensure minimum fadeout duration to avoid division by zero
                fadeoutDurationMs = Math.Max(10, fadeoutDurationMs);

                // Trigger manual fadeout
                _fadingOut = true;
                _fadeoutPosition = 0;
                _fadeoutDurationSamples = (int)(fadeoutDurationMs * _waveFormat.SampleRate / 1000.0);
                
                // Clear queue so nothing plays after fadeout
                ClearQueueInternal();

                if (Program.DebugMode)
                {
                    GameConsole.Info($"[AudioCueQueue] Fading out to silence ({fadeoutDurationMs}ms)...");
                }
            }
        }

        /// <summary>
        /// Skip current sound and crossfade to next queued sound (if available)
        /// If no next sound, fades out current. Uses normal crossfade duration.
        /// </summary>
        public void SkipToNext()
        {
            lock (_lock)
            {
                if (_currentCue == null)
                {
                    if (Program.DebugMode)
                    {
                        GameConsole.Warn("[AudioCueQueue] SkipToNext: Nothing playing");
                    }
                    return;
                }

                // Check if we have a next sound to skip to
                bool hasNext = _nextCue != null || _normalQueue.Count > 0;

                if (hasNext)
                {
                    // If next cue not prepared, prepare it now
                    if (_nextCue == null && _normalQueue.Count > 0)
                    {
                        _nextCue = _normalQueue.Dequeue();
                        if (Program.DebugMode)
                        {
                            GameConsole.Debug($"[AudioCueQueue] Prepared next for skip: {System.IO.Path.GetFileName(_nextCue.FilePath)}");
                        }
                    }

                    // Trigger immediate crossfade to next sound (uses configured crossfade duration)
                    if (_nextCue != null && !_crossfading)
                    {
                        _crossfading = true;
                        _crossfadePosition = 0;

                        if (Program.DebugMode)
                        {
                            GameConsole.Info(
                                $"[AudioCueQueue] Skipping: {System.IO.Path.GetFileName(_currentCue.FilePath)} → " +
                                $"{System.IO.Path.GetFileName(_nextCue.FilePath)}"
                            );
                        }
                    }
                }
                else
                {
                    // No next sound, just fadeout current (use 50ms for quick skip)
                    if (Program.DebugMode)
                    {
                        GameConsole.Info($"[AudioCueQueue] No next sound, fading out current (50ms)");
                    }
                    StopWithFadeout(50);
                }
            }
        }

        /// <summary>
        /// Reads audio samples from the queue, handling crossfades automatically
        /// </summary>
        public int Read(float[] buffer, int offset, int count)
        {
            lock (_lock)
            {
                if (_currentCue == null)
                {
                    // Return silence instead of 0 to keep queue active in mixer
                    // This prevents the mixer from removing the queue when idle
                    Array.Clear(buffer, offset, count);
                    return count;
                }

            // Handle crossfading between current and next cue
            if (_crossfading && _nextCue != null && _currentCue != null)
            {
                float[] currentBuffer = new float[count];
                float[] nextBuffer = new float[count];

                int currentRead = _currentCue.Source.Read(currentBuffer, 0, count);
                int nextRead = _nextCue.Source.Read(nextBuffer, 0, count);

                int samplesToProcess = Math.Max(currentRead, nextRead);

                for (int i = 0; i < samplesToProcess; i++)
                {
                    // Equal-power crossfade for smoother transitions
                    float crossfadeProgress = Math.Min(1.0f, (float)_crossfadePosition / _crossfadeDurationSamples);
                    
                    // Equal-power curve: sqrt(1 - x) for current, sqrt(x) for next
                    float currentGain = (float)Math.Sqrt(1.0f - crossfadeProgress);
                    float nextGain = (float)Math.Sqrt(crossfadeProgress);

                    buffer[offset + i] =
                        (i < currentRead ? currentBuffer[i] * currentGain : 0) +
                        (i < nextRead ? nextBuffer[i] * nextGain : 0);

                    _crossfadePosition++;

                    // Check if crossfade complete
                    if (_crossfadePosition >= _crossfadeDurationSamples)
                    {
                        if (Program.DebugMode)
                        {
                            GameConsole.Info(
                                $"[AudioCueQueue] Crossfade complete: {System.IO.Path.GetFileName(_currentCue.FilePath)} → " +
                                $"{System.IO.Path.GetFileName(_nextCue.FilePath)}"
                            );
                        }

                        // Crossfade complete, switch to next
                        var oldCue = _currentCue;
                        _currentCue.Dispose();
                        _cueThresholdOverrides.Remove(oldCue); // Clean up threshold override
                        _currentCue = _nextCue;
                        _nextCue = null;
                        _crossfading = false;
                        _crossfadePosition = 0;
                        _silenceSampleCount = 0; // Reset silence detection for new track
                        _currentCueSamplesProcessed = 0;

                        // Check if more in queue
                        if (_normalQueue.Count > 0)
                        {
                            _nextCue = _normalQueue.Dequeue();
                            if (Program.DebugMode)
                            {
                                GameConsole.Debug(
                                    $"[AudioCueQueue] Prepared next: {System.IO.Path.GetFileName(_nextCue.FilePath)}"
                                );
                            }
                        }

                        // Continue reading from current position
                        break;
                    }
                }

                return samplesToProcess;
            }
            else
            {
                // Normal playback from current cue
                int read = _currentCue.Source!.Read(buffer, offset, count);

                // Handle manual fadeout (only if not crossfading)
                if (_fadingOut && read > 0 && _fadeoutDurationSamples > 0)
                {
                    for (int i = 0; i < read; i++)
                    {
                        if (_fadeoutPosition < _fadeoutDurationSamples)
                        {
                            // Linear fadeout from 1.0 to 0.0
                            float fadeGain = 1.0f - ((float)_fadeoutPosition / _fadeoutDurationSamples);
                            buffer[offset + i] *= fadeGain;
                            _fadeoutPosition++;
                        }
                        else
                        {
                            // Fadeout complete, fill rest with silence
                            buffer[offset + i] = 0f;
                        }
                    }

                    // If fadeout complete, stop playback
                    if (_fadeoutPosition >= _fadeoutDurationSamples)
                    {
                        if (_currentCue != null)
                        {
                            _cueThresholdOverrides.Remove(_currentCue); // Clean up threshold override
                            _currentCue.Dispose();
                            _currentCue = null;
                        }
                        _fadingOut = false;
                        _fadeoutPosition = 0;

                        if (Program.DebugMode)
                        {
                            GameConsole.Info("[AudioCueQueue] Fadeout complete, stopped");
                        }
                    }

                    return read;
                }

                // Monitor amplitude for silence detection (if enabled and not crossfading)
                if (read > 0 && _silenceSettings != null && _silenceSettings.Enabled && !_crossfading)
                {
                    _currentCueSamplesProcessed += read;
                    
                    if (_currentCueSamplesProcessed > _initialDelaySamples)
                    {
                    // Get the threshold for this cue (custom or default)
                    float thresholdAmplitude = _silenceThresholdAmplitude;
                    if (_currentCue != null && _cueThresholdOverrides.TryGetValue(_currentCue, out float customThreshold))
                    {
                        thresholdAmplitude = customThreshold;
                    }
                    
                    // Calculate RMS amplitude of this buffer
                    float sumSquares = 0f;
                    for (int i = 0; i < read; i++)
                    {
                        float sample = buffer[offset + i];
                        sumSquares += sample * sample;
                    }
                    float rms = (float)Math.Sqrt(sumSquares / read);
                    
                    // Check if below silence threshold
                    if (rms < thresholdAmplitude)
                    {
                        _silenceSampleCount += read;
                        
                        // If silence sustained long enough, trigger crossfade
                        if (_silenceSampleCount >= _silenceDurationSamples)
                        {
                            if (Program.DebugMode)
                            {
                                GameConsole.Info(
                                    $"[AudioCueQueue] Silence detected ({_silenceSettings.SilenceDurationMs}ms at {_silenceSettings.ThresholdDb}dB), " +
                                    $"starting {(_nextCue != null || _normalQueue.Count > 0 ? "crossfade to next" : "fadeout")}"
                                );
                            }
                            
                            // If we have a next cue or queue items, start crossfade
                            if (_nextCue != null || _normalQueue.Count > 0)
                            {
                                // Prepare next cue if not already prepared
                                if (_nextCue == null && _normalQueue.Count > 0)
                                {
                                    _nextCue = _normalQueue.Dequeue();
                                    if (Program.DebugMode)
                                    {
                                        GameConsole.Debug(
                                            $"[AudioCueQueue] Prepared next: {System.IO.Path.GetFileName(_nextCue.FilePath)}"
                                        );
                                    }
                                }
                                
                                // Start crossfade
                                _crossfading = true;
                                _crossfadePosition = 0;
                                _silenceSampleCount = 0;
                            }
                            else
                            {
                                // Nothing queued - apply fadeout to current, then it will naturally complete
                                // This prevents abrupt stops
                                if (Program.DebugMode)
                                {
                                    GameConsole.Debug("[AudioCueQueue] Applying fadeout (no items in queue)");
                                }
                                
                                // Apply a short fadeout to current buffer
                                int fadeoutSamples = Math.Min(read, (int)(_silenceSettings.FadeoutDurationMs * _waveFormat.SampleRate / 1000.0));
                                for (int i = 0; i < fadeoutSamples; i++)
                                {
                                    float fadeGain = 1.0f - ((float)i / fadeoutSamples);
                                    buffer[offset + i] *= fadeGain;
                                }
                                
                                // Mark current as complete
                                if (_currentCue != null)
                                {
                                    _cueThresholdOverrides.Remove(_currentCue); // Clean up threshold override
                                    _currentCue.Dispose();
                                    _currentCue = null;
                                }
                                _silenceSampleCount = 0;
                                
                                // Return silence for remaining buffer
                                if (read < count)
                                {
                                    Array.Clear(buffer, offset + read, count - read);
                                }
                                return count;
                            }
                        }
                    }
                    else
                    {
                        // Reset silence counter when audio is above threshold
                        _silenceSampleCount = 0;
                    }
                    }
                }

                // If current cue finished
                if (read == 0)
                {
                    // Don't log from audio thread - causes UI freezes
                    if (_currentCue != null)
                    {
                        _cueThresholdOverrides.Remove(_currentCue); // Clean up threshold override
                        _currentCue.Dispose();
                        _currentCue = null;
                    }
                    _silenceSampleCount = 0;
                    _currentCueSamplesProcessed = 0;

                    // If next cue is waiting, start it
                    if (_nextCue != null)
                    {
                        _currentCue = _nextCue;
                        _nextCue = null;
                        return Read(buffer, offset, count); // Recursive read from new cue
                    }
                    // If queue has items, start next
                    else if (_normalQueue.Count > 0)
                    {
                        _currentCue = _normalQueue.Dequeue();
                        // Don't log from audio thread - causes freezes
                        return Read(buffer, offset, count); // Recursive read from new cue
                    }
                    // Nothing queued - return silence to stay active in mixer
                    else
                    {
                        Array.Clear(buffer, offset, count);
                        return count;
                    }
                }
                // If current cue is near end and we have a next cue, start crossfade
                else if (_nextCue != null && !_crossfading)
                {
                    // Check if we're within crossfade distance of the end
                    long remainingSamples = _currentCue.Source!.Length - _currentCue.Source!.Position;
                    if (remainingSamples <= _crossfadeDurationSamples)
                    {
                        _crossfading = true;
                        _crossfadePosition = 0;
                        _silenceSampleCount = 0;
                        if (Program.DebugMode)
                        {
                            GameConsole.Debug(
                                $"[AudioCueQueue] Starting crossfade: {System.IO.Path.GetFileName(_currentCue.FilePath)} → " +
                                $"{System.IO.Path.GetFileName(_nextCue.FilePath)}"
                            );
                        }
                    }
                }

                return read;
            }
        }
        }

        public long Position
        {
            get => _currentCue?.Source.Position ?? 0;
            set => throw new NotSupportedException("Seeking not supported in AudioCueQueue");
        }

        public long Length => _currentCue?.Source.Length ?? 0;

        public WaveFormat WaveFormat => _waveFormat;

        public bool CanSeek => false;

        public void Dispose()
        {
            if (_currentCue != null)
            {
                _cueThresholdOverrides.Remove(_currentCue);
                _currentCue.Dispose();
            }
            if (_nextCue != null)
            {
                _cueThresholdOverrides.Remove(_nextCue);
                _nextCue.Dispose();
            }
            foreach (var cue in _normalQueue)
            {
                _cueThresholdOverrides.Remove(cue);
                cue.Dispose();
            }
            _normalQueue.Clear();
            _cueThresholdOverrides.Clear();
        }
    }

    /// <summary>
    /// Represents a single audio cue in the queue
    /// </summary>
    internal class AudioCue : IDisposable
    {
        public string FilePath { get; }
        public AudioPriority Priority { get; }
        public ISampleSource Source { get; }

        public AudioCue(string filePath, AudioPriority priority, WaveFormat targetFormat, 
            SilenceDetectionSettings? silenceSettings = null)
        {
            FilePath = filePath;
            Priority = priority;

            // Load audio file with appropriate decoder
            ISampleSource waveSource;
            if (filePath.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    waveSource = new MediaFoundationDecoder(filePath).ToSampleSource();
                }
                catch
                {
                    waveSource = new DmoMp3Decoder(filePath).ToSampleSource();
                }
            }
            else
            {
                waveSource = CodecFactory.Instance.GetCodec(filePath).ToSampleSource();
            }

            // Convert to target format if needed
            if (waveSource.WaveFormat.SampleRate != targetFormat.SampleRate ||
                waveSource.WaveFormat.Channels != targetFormat.Channels)
            {
                waveSource = waveSource.ChangeSampleRate(targetFormat.SampleRate);
            }

            // Audio files should be pre-normalized before import
            // Silence detection is integrated into AudioCueQueue.Read()
            Source = waveSource;
        }

        public void Dispose()
        {
            Source?.Dispose();
        }
    }

    /// <summary>
    /// Priority levels for audio cues
    /// </summary>
    public enum AudioPriority
    {
        /// <summary>
        /// Normal priority - queued in FIFO order
        /// </summary>
        Normal,

        /// <summary>
        /// Immediate priority - interrupts current playback with crossfade
        /// </summary>
        Immediate
    }
}
