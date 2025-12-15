using System.Media;
using System.Runtime.InteropServices;
using System.Text;

namespace MillionaireGame.Services;

/// <summary>
/// Manages game audio playback using Windows Media Player and DirectSound
/// </summary>
public class SoundService : IDisposable
{
    private readonly Dictionary<SoundEffect, string> _soundPaths = new();
    private readonly SoundPlayer _soundPlayer = new();
    private bool _soundEnabled = true;
    private bool _disposed = false;

    // WinMM API for MP3 playback
    [DllImport("winmm.dll")]
    private static extern int mciSendString(string command, StringBuilder? returnValue, int returnLength, IntPtr hwndCallback);

    public bool SoundEnabled
    {
        get => _soundEnabled;
        set => _soundEnabled = value;
    }

    /// <summary>
    /// Register a sound file path for a specific sound effect
    /// </summary>
    public void RegisterSound(SoundEffect effect, string filePath)
    {
        if (File.Exists(filePath))
        {
            _soundPaths[effect] = filePath;
        }
    }

    /// <summary>
    /// Play a sound effect
    /// </summary>
    public void PlaySound(SoundEffect effect)
    {
        if (!_soundEnabled) return;

        if (_soundPaths.TryGetValue(effect, out var filePath))
        {
            Task.Run(() => PlaySoundFile(filePath));
        }
    }

    /// <summary>
    /// Play a sound effect asynchronously
    /// </summary>
    public async Task PlaySoundAsync(SoundEffect effect)
    {
        if (!_soundEnabled) return;

        if (_soundPaths.TryGetValue(effect, out var filePath))
        {
            await Task.Run(() => PlaySoundFile(filePath));
        }
    }

    /// <summary>
    /// Stop all currently playing sounds
    /// </summary>
    public void StopAllSounds()
    {
        try
        {
            mciSendString("close all", null, 0, IntPtr.Zero);
        }
        catch
        {
            // Ignore errors on stop
        }
    }

    /// <summary>
    /// Play a sound file directly
    /// </summary>
    public void PlaySoundFile(string filePath)
    {
        if (!_soundEnabled || !File.Exists(filePath)) return;

        try
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            
            if (extension == ".wav")
            {
                // Use SoundPlayer for WAV files
                _soundPlayer.SoundLocation = filePath;
                _soundPlayer.Play();
            }
            else if (extension == ".mp3")
            {
                // Use MCI for MP3 files
                var alias = $"sound_{Guid.NewGuid():N}";
                mciSendString($"open \"{filePath}\" type mpegvideo alias {alias}", null, 0, IntPtr.Zero);
                mciSendString($"play {alias}", null, 0, IntPtr.Zero);
                
                // Close after a delay (simplified approach)
                Task.Delay(5000).ContinueWith(_ => 
                {
                    mciSendString($"close {alias}", null, 0, IntPtr.Zero);
                });
            }
        }
        catch (Exception ex)
        {
            // Log error but don't crash the application
            System.Diagnostics.Debug.WriteLine($"Error playing sound: {ex.Message}");
        }
    }

    /// <summary>
    /// Load sound paths from settings
    /// </summary>
    public void LoadSoundsFromSettings(Core.Settings.ApplicationSettings settings)
    {
        // Register common game sounds
        RegisterSound(SoundEffect.LightsDown, settings.SoundQ1to5LightsDown);
        RegisterSound(SoundEffect.QuestionCue, settings.SoundQ1to5Bed);
        RegisterSound(SoundEffect.FinalAnswer, settings.SoundATAVoting); // Placeholder
        RegisterSound(SoundEffect.CorrectAnswer, settings.SoundQ1to5Bed); // Placeholder
        RegisterSound(SoundEffect.WrongAnswer, settings.SoundGameOver);
        RegisterSound(SoundEffect.WalkAway, settings.SoundWalkAway1);
        RegisterSound(SoundEffect.Lifeline5050, settings.Sound5050);
        RegisterSound(SoundEffect.LifelinePhone, settings.SoundPlusOneStart);
        RegisterSound(SoundEffect.LifelineATA, settings.SoundATAStart);
        RegisterSound(SoundEffect.LifelineSwitch, settings.SoundSwitchActivate);
        RegisterSound(SoundEffect.ToHotSeat, settings.SoundToHotSeat);
        RegisterSound(SoundEffect.ExplainRules, settings.SoundExplainRules);
        RegisterSound(SoundEffect.RiskMode, settings.SoundRiskModeActive);
    }

    public void Dispose()
    {
        if (_disposed) return;

        StopAllSounds();
        _soundPlayer?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Enumeration of all game sound effects
/// </summary>
public enum SoundEffect
{
    QuestionCue,
    LightsDown,
    FinalAnswer,
    CorrectAnswer,
    WrongAnswer,
    WalkAway,
    Lifeline5050,
    LifelinePhone,
    LifelineATA,
    LifelineSwitch,
    ToHotSeat,
    ExplainRules,
    RiskMode,
    BackgroundMusic
}
