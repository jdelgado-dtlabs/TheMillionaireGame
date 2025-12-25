using CSCore;
using CSCore.Codecs;
using CSCore.Codecs.MP3;
using CSCore.MediaFoundation;
using System;
using System.Collections.Generic;

using MillionaireGame.Utilities;

namespace MillionaireGame.Services
{
    /// <summary>
    /// Queue manager for sequential audio playback with automatic crossfading.
    /// Manages FIFO queue of audio files and seamlessly transitions between them
    /// using configurable crossfade durations.
    /// </summary>
    /// <remarks>
    /// Key Features:
    /// - FIFO queue for normal priority sounds
    /// - Immediate interrupt capability for high priority
    /// - Automatic crossfade between queued sounds (200ms default)
    /// - Configurable crossfade duration
    /// - Queue limit enforcement (10 default)
    /// - Preview: count + estimated time
    /// </remarks>
    public class AudioCueQueue : ISampleSource
    {
        private readonly Queue<AudioCue> _normalQueue = new();
        private readonly int _queueLimit;
        private readonly int _crossfadeDurationSamples;
        private AudioCue? _currentCue;
        private AudioCue? _nextCue;
        private bool _crossfading = false;
        private int _crossfadePosition = 0;
        private WaveFormat _waveFormat;

        /// <summary>
        /// Gets the number of sounds currently queued
        /// </summary>
        public int QueueCount => _normalQueue.Count;

        /// <summary>
        /// Gets whether the queue is currently playing audio
        /// </summary>
        public bool IsPlaying => _currentCue != null;

        /// <summary>
        /// Gets whether a crossfade is currently in progress
        /// </summary>
        public bool IsCrossfading => _crossfading;

        /// <summary>
        /// Initializes a new instance of the AudioCueQueue class
        /// </summary>
        /// <param name="waveFormat">The wave format for audio playback</param>
        /// <param name="crossfadeDurationMs">Duration of crossfades in milliseconds (default: 200ms)</param>
        /// <param name="queueLimit">Maximum number of sounds that can be queued (default: 10)</param>
        public AudioCueQueue(WaveFormat waveFormat, int crossfadeDurationMs = 200, int queueLimit = 10)
        {
            _waveFormat = waveFormat ?? throw new ArgumentNullException(nameof(waveFormat));
            _queueLimit = queueLimit;
            _crossfadeDurationSamples = (int)(crossfadeDurationMs * waveFormat.SampleRate / 1000.0);

            if (Program.DebugMode)
            {
                GameConsole.Debug(
                    $"[AudioCueQueue] Created: crossfade={crossfadeDurationMs}ms ({_crossfadeDurationSamples} samples), " +
                    $"queueLimit={queueLimit}"
                );
            }
        }

        /// <summary>
        /// Queues an audio file for playback
        /// </summary>
        /// <param name="filePath">Path to the audio file</param>
        /// <param name="priority">Priority level (Normal or Immediate)</param>
        /// <returns>True if queued successfully, false if queue is full</returns>
        public bool QueueAudio(string filePath, AudioPriority priority = AudioPriority.Normal)
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
                var cue = new AudioCue(filePath, priority, _waveFormat);

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
                    }
                    else
                    {
                        _currentCue = cue;
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

        /// <summary>
        /// Clears all queued sounds (does not stop current playback)
        /// </summary>
        public void ClearQueue()
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
        /// Stops all playback and clears the queue
        /// </summary>
        public void Stop()
        {
            _currentCue?.Dispose();
            _currentCue = null;
            ClearQueue();

            if (Program.DebugMode)
            {
                GameConsole.Warn("[AudioCueQueue] Stopped");
            }
        }

        /// <summary>
        /// Reads audio samples from the queue, handling crossfades automatically
        /// </summary>
        public int Read(float[] buffer, int offset, int count)
        {
            if (_currentCue == null)
            {
                return 0;
            }

            // Handle crossfading between current and next cue
            if (_crossfading && _nextCue != null)
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
                        _currentCue.Dispose();
                        _currentCue = _nextCue;
                        _nextCue = null;
                        _crossfading = false;
                        _crossfadePosition = 0;

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
                int read = _currentCue.Source.Read(buffer, offset, count);

                // If current cue finished
                if (read == 0)
                {
                    if (Program.DebugMode)
                    {
                        GameConsole.Info(
                            $"[AudioCueQueue] Finished: {System.IO.Path.GetFileName(_currentCue.FilePath)}"
                        );
                    }

                    _currentCue.Dispose();
                    _currentCue = null;

                    // If next cue is waiting, start it
                    if (_nextCue != null)
                    {
                        _currentCue = _nextCue;
                        _nextCue = null;
                        if (Program.DebugMode)
                        {
                            GameConsole.Info(
                                $"[AudioCueQueue] Starting: {System.IO.Path.GetFileName(_currentCue.FilePath)}"
                            );
                        }
                        return Read(buffer, offset, count); // Recursive read from new cue
                    }
                    // If queue has items, start next
                    else if (_normalQueue.Count > 0)
                    {
                        _currentCue = _normalQueue.Dequeue();
                        if (Program.DebugMode)
                        {
                            GameConsole.Info(
                                $"[AudioCueQueue] Starting: {System.IO.Path.GetFileName(_currentCue.FilePath)}"
                            );
                        }
                        return Read(buffer, offset, count); // Recursive read from new cue
                    }
                }
                // If current cue is near end and we have a next cue, start crossfade
                else if (_nextCue != null && !_crossfading)
                {
                    // Check if we're within crossfade distance of the end
                    long remainingSamples = _currentCue.Source.Length - _currentCue.Source.Position;
                    if (remainingSamples <= _crossfadeDurationSamples)
                    {
                        _crossfading = true;
                        _crossfadePosition = 0;
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
            _currentCue?.Dispose();
            _nextCue?.Dispose();
            foreach (var cue in _normalQueue)
            {
                cue.Dispose();
            }
            _normalQueue.Clear();
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

        public AudioCue(string filePath, AudioPriority priority, WaveFormat targetFormat)
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
