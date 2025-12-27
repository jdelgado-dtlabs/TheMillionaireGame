namespace MillionaireGame.Core.Settings;

/// <summary>
/// Settings for automatic silence detection in audio playback.
/// Integrated into AudioCueQueue for automatic track advancement.
/// When audio drops below threshold for specified duration, triggers crossfade to next track.
/// </summary>
public class SilenceDetectionSettings
{
    /// <summary>
    /// Enable automatic silence detection
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Amplitude threshold in dB below which audio is considered silent
    /// Typical values: -60dB (very sensitive), -40dB (moderate), -20dB (only loud silence)
    /// </summary>
    public float ThresholdDb { get; set; } = -40f;

    /// <summary>
    /// Duration in milliseconds that silence must be sustained before auto-completion
    /// Prevents false triggers during brief pauses in audio
    /// </summary>
    public int SilenceDurationMs { get; set; } = 250;

    /// <summary>
    /// Fadeout duration in milliseconds applied when silence is detected
    /// Prevents DC pops and clicks by smoothly ramping to zero
    /// </summary>
    public int FadeoutDurationMs { get; set; } = 20;

    /// <summary>
    /// Initial delay in milliseconds before silence detection begins
    /// Prevents false triggers on sounds with slow ramp-ups or quiet intros
    /// Recommended: 2000-2500ms for sounds with gradual fade-ins or short sounds that need to play completely
    /// </summary>
    public int InitialDelayMs { get; set; } = 2500;

    /// <summary>
    /// Apply silence detection to music channel (looping bed music)
    /// Generally should be false since music loops continuously
    /// </summary>
    public bool ApplyToMusic { get; set; } = false;

    /// <summary>
    /// Apply silence detection to effects channel (one-shot sounds)
    /// Generally should be true since effects have definite endings
    /// </summary>
    public bool ApplyToEffects { get; set; } = true;
}
