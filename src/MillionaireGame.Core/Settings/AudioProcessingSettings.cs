namespace MillionaireGame.Core.Settings;

/// <summary>
/// Settings for audio gain control and limiting
/// Note: All audio files should be pre-normalized to -3dB to -4dB peak level
/// before import. Use batch processing in Audacity if needed.
/// </summary>
public class AudioProcessingSettings
{
    /// <summary>
    /// Master gain adjustment in dB applied to all audio
    /// Positive values increase volume, negative values decrease
    /// Range: -20dB to +20dB
    /// </summary>
    public float MasterGainDb { get; set; } = 0.0f;

    /// <summary>
    /// Separate gain adjustment for effects channel in dB
    /// Applied in addition to master gain
    /// Use to balance effects volume relative to music
    /// </summary>
    public float EffectsGainDb { get; set; } = 0.0f;

    /// <summary>
    /// Separate gain adjustment for music channel in dB
    /// Applied in addition to master gain
    /// Use to balance music volume relative to effects
    /// </summary>
    public float MusicGainDb { get; set; } = 0.0f;

    /// <summary>
    /// Apply hard limiting to prevent clipping
    /// Ceiling at 0dBFS with soft knee
    /// </summary>
    public bool EnableLimiter { get; set; } = true;

    /// <summary>
    /// Limiter ceiling in dBFS
    /// Prevents audio from exceeding this level
    /// Default: -0.3dB to provide small safety margin
    /// </summary>
    public float LimiterCeilingDb { get; set; } = -0.3f;
}
