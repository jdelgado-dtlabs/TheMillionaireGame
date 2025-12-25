namespace MillionaireGame.Core.Settings;

/// <summary>
/// Settings for audio cue queue and automatic crossfading
/// </summary>
public class CrossfadeSettings
{
    /// <summary>
    /// Enable automatic crossfading between queued sounds
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Duration of crossfade transitions in milliseconds
    /// Typical values: 100ms (quick), 200ms (smooth), 500ms (slow/dramatic)
    /// </summary>
    public int CrossfadeDurationMs { get; set; } = 200;

    /// <summary>
    /// Maximum number of sounds that can be queued
    /// Prevents memory issues from excessive queueing
    /// </summary>
    public int QueueLimit { get; set; } = 10;

    /// <summary>
    /// Automatically crossfade between consecutive queued sounds
    /// When false, sounds play sequentially with no overlap
    /// </summary>
    public bool AutoCrossfade { get; set; } = true;
}
