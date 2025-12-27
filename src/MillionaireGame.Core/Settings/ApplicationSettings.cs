using MillionaireGame.Core.Database;
using System.Reflection;
using System.Xml.Serialization;

namespace MillionaireGame.Core.Settings;

/// <summary>
/// Main application settings and profile configuration
/// </summary>
[XmlRoot("AppSettings")]
public class ApplicationSettings
{
    // Lifeline Configuration
    public int TotalLifelines { get; set; } = 4;
    public string Lifeline1 { get; set; } = "5050";
    public string Lifeline2 { get; set; } = "plusone";
    public string Lifeline3 { get; set; } = "ata";
    public string Lifeline4 { get; set; } = "switch";
    public int Lifeline1Available { get; set; } = 0; // 0 = Always
    public int Lifeline2Available { get; set; } = 0;
    public int Lifeline3Available { get; set; } = 0;
    public int Lifeline4Available { get; set; } = 0;

    // Graphics Settings
    public int WinningStrapTexture { get; set; } = 0;
    public int QuestionsTexture { get; set; } = 0;

    // Screen Display Settings
    public bool EnablePreviewAutomatically { get; set; } = false;
    public string PreviewOrientation { get; set; } = "Vertical";
    public bool FullScreenHostScreenEnable { get; set; } = false;
    public int FullScreenHostScreenMonitor { get; set; } = 0;
    public bool FullScreenGuestScreenEnable { get; set; } = false;
    public int FullScreenGuestScreenMonitor { get; set; } = 0;
    public bool FullScreenTVScreenEnable { get; set; } = false;
    public int FullScreenTVScreenMonitor { get; set; } = 0;

    // Game Behavior Settings
    public bool ClearHostMessagesAtNewQuestion { get; set; } = false;
    public bool ShowAnswerOnlyOnHostScreenAtFinal { get; set; } = false;
    public bool AutoHideQuestionAtPlusOne { get; set; } = false;
    public bool AutoShowTotalWinnings { get; set; } = false;
    public bool AutoHideQuestionAtWalkAway { get; set; } = false;
    public bool HideAnswerInControlPanelAtNewQ { get; set; } = false;
    public bool ATAIsAlwaysCorrect { get; set; } = false;

    // Fastest Finger First Settings
    public int FFFPort { get; set; } = 3818;
    public string FFFPlayer1Name { get; set; } = "Player 1";
    public string FFFPlayer2Name { get; set; } = "Player 2";
    public string FFFPlayer3Name { get; set; } = "Player 3";
    public string FFFPlayer4Name { get; set; } = "Player 4";
    public string FFFPlayer5Name { get; set; } = "Player 5";
    public string FFFPlayer6Name { get; set; } = "Player 6";
    public string FFFPlayer7Name { get; set; } = "Player 7";
    public string FFFPlayer8Name { get; set; } = "Player 8";

    // Sound Pack Settings
    public string SelectedSoundPack { get; set; } = "Default";
    public string? AudioOutputDevice { get; set; } = null; // null = System Default

    // Audio Processing Settings
    public SilenceDetectionSettings SilenceDetection { get; set; } = new();
    public CrossfadeSettings Crossfade { get; set; } = new();
    public AudioProcessingSettings AudioProcessing { get; set; } = new();

    // Debug Console Settings
    public bool ShowConsole { get; set; } = false;
    public bool ShowWebServiceConsole { get; set; } = false;

    // Web Server / Audience Participation Settings
    public string AudienceServerIP { get; set; } = "127.0.0.1";
    public int AudienceServerPort { get; set; } = 5278;
    public bool AudienceServerAutoStart { get; set; } = false;
}

/// <summary>
/// Manager for application settings persistence
/// </summary>
public class ApplicationSettingsManager
{
    private const string FileName = "config.xml";
    private readonly string _filePath;
    private readonly ApplicationSettingsRepository? _repository;
    private readonly bool _useDatabaseMode;

    public ApplicationSettings Settings { get; private set; }

    public ApplicationSettingsManager(string? basePath = null, string? connectionString = null)
    {
        _filePath = Path.Combine(basePath ?? AppDomain.CurrentDomain.BaseDirectory, FileName);
        Settings = new ApplicationSettings();
        
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            _repository = new ApplicationSettingsRepository(connectionString);
            _useDatabaseMode = true;
        }
    }

    /// <summary>
    /// Load settings from database or XML file
    /// </summary>
    public async Task LoadSettingsAsync()
    {
        if (_useDatabaseMode && _repository != null)
        {
            await LoadFromDatabaseAsync();
        }
        else
        {
            LoadFromXml();
        }
    }

    /// <summary>
    /// Synchronous load for backward compatibility (uses XML only)
    /// </summary>
    public void LoadSettings()
    {
        LoadFromXml();
    }

    private async Task LoadFromDatabaseAsync()
    {
        if (_repository == null)
            return;

        try
        {
            var dbSettings = await _repository.GetAllSettingsAsync();
            Settings = new ApplicationSettings();
            
            var properties = typeof(ApplicationSettings).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (!property.CanWrite)
                    continue;

                if (dbSettings.TryGetValue(property.Name, out var value))
                {
                    // Convert string value to property type
                    object? convertedValue = ConvertValue(value, property.PropertyType);
                    if (convertedValue != null)
                    {
                        property.SetValue(Settings, convertedValue);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading settings from database: {ex.Message}");
            // Fall back to defaults
            Settings = new ApplicationSettings();
        }
    }

    private void LoadFromXml()
    {
        if (!File.Exists(_filePath))
        {
            // Settings already initialized with defaults in constructor
            return;
        }

        try
        {
            var serializer = new XmlSerializer(typeof(ApplicationSettings));
            using var reader = new StreamReader(_filePath);
            var loadedSettings = (ApplicationSettings?)serializer.Deserialize(reader);
            if (loadedSettings != null)
            {
                // CRITICAL: Update properties instead of replacing Settings object
                // This maintains references held by SoundService -> EffectsChannel -> AudioCueQueue
                CopySettingsProperties(loadedSettings, Settings);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading application settings from XML: {ex.Message}");
            // Keep defaults that were initialized in constructor
        }
    }

    /// <summary>
    /// Copy all properties from source to destination settings object.
    /// Maintains object references for audio settings.
    /// </summary>
    private void CopySettingsProperties(ApplicationSettings source, ApplicationSettings destination)
    {
        // Copy simple properties
        destination.TotalLifelines = source.TotalLifelines;
        destination.Lifeline1 = source.Lifeline1;
        destination.Lifeline2 = source.Lifeline2;
        destination.Lifeline3 = source.Lifeline3;
        destination.Lifeline4 = source.Lifeline4;
        destination.Lifeline1Available = source.Lifeline1Available;
        destination.Lifeline2Available = source.Lifeline2Available;
        destination.Lifeline3Available = source.Lifeline3Available;
        destination.Lifeline4Available = source.Lifeline4Available;
        destination.WinningStrapTexture = source.WinningStrapTexture;
        destination.QuestionsTexture = source.QuestionsTexture;
        destination.EnablePreviewAutomatically = source.EnablePreviewAutomatically;
        destination.PreviewOrientation = source.PreviewOrientation;
        destination.FullScreenHostScreenEnable = source.FullScreenHostScreenEnable;
        destination.FullScreenHostScreenMonitor = source.FullScreenHostScreenMonitor;
        destination.FullScreenGuestScreenEnable = source.FullScreenGuestScreenEnable;
        destination.FullScreenGuestScreenMonitor = source.FullScreenGuestScreenMonitor;
        destination.FullScreenTVScreenEnable = source.FullScreenTVScreenEnable;
        destination.FullScreenTVScreenMonitor = source.FullScreenTVScreenMonitor;
        destination.ClearHostMessagesAtNewQuestion = source.ClearHostMessagesAtNewQuestion;
        destination.ShowAnswerOnlyOnHostScreenAtFinal = source.ShowAnswerOnlyOnHostScreenAtFinal;
        destination.AutoHideQuestionAtPlusOne = source.AutoHideQuestionAtPlusOne;
        destination.AutoShowTotalWinnings = source.AutoShowTotalWinnings;
        destination.AutoHideQuestionAtWalkAway = source.AutoHideQuestionAtWalkAway;
        destination.HideAnswerInControlPanelAtNewQ = source.HideAnswerInControlPanelAtNewQ;
        destination.ATAIsAlwaysCorrect = source.ATAIsAlwaysCorrect;
        destination.FFFPort = source.FFFPort;
        destination.FFFPlayer1Name = source.FFFPlayer1Name;
        destination.FFFPlayer2Name = source.FFFPlayer2Name;
        destination.FFFPlayer3Name = source.FFFPlayer3Name;
        destination.FFFPlayer4Name = source.FFFPlayer4Name;
        destination.FFFPlayer5Name = source.FFFPlayer5Name;
        destination.FFFPlayer6Name = source.FFFPlayer6Name;
        destination.FFFPlayer7Name = source.FFFPlayer7Name;
        destination.FFFPlayer8Name = source.FFFPlayer8Name;
        destination.SelectedSoundPack = source.SelectedSoundPack;
        destination.AudioOutputDevice = source.AudioOutputDevice;
        destination.ShowConsole = source.ShowConsole;
        destination.ShowWebServiceConsole = source.ShowWebServiceConsole;
        destination.AudienceServerIP = source.AudienceServerIP;
        destination.AudienceServerPort = source.AudienceServerPort;
        destination.AudienceServerAutoStart = source.AudienceServerAutoStart;

        // Copy audio settings properties (maintain object references)
        destination.SilenceDetection.Enabled = source.SilenceDetection.Enabled;
        destination.SilenceDetection.ThresholdDb = source.SilenceDetection.ThresholdDb;
        destination.SilenceDetection.SilenceDurationMs = source.SilenceDetection.SilenceDurationMs;
        destination.SilenceDetection.FadeoutDurationMs = source.SilenceDetection.FadeoutDurationMs;
        destination.SilenceDetection.InitialDelayMs = source.SilenceDetection.InitialDelayMs;
        destination.SilenceDetection.ApplyToMusic = source.SilenceDetection.ApplyToMusic;
        destination.SilenceDetection.ApplyToEffects = source.SilenceDetection.ApplyToEffects;

        destination.Crossfade.Enabled = source.Crossfade.Enabled;
        destination.Crossfade.CrossfadeDurationMs = source.Crossfade.CrossfadeDurationMs;
        destination.Crossfade.QueueLimit = source.Crossfade.QueueLimit;
        destination.Crossfade.AutoCrossfade = source.Crossfade.AutoCrossfade;

        destination.AudioProcessing.MasterGainDb = source.AudioProcessing.MasterGainDb;
        destination.AudioProcessing.EffectsGainDb = source.AudioProcessing.EffectsGainDb;
        destination.AudioProcessing.MusicGainDb = source.AudioProcessing.MusicGainDb;
        destination.AudioProcessing.EnableLimiter = source.AudioProcessing.EnableLimiter;
        destination.AudioProcessing.LimiterCeilingDb = source.AudioProcessing.LimiterCeilingDb;
    }

    /// <summary>
    /// Save settings to database or XML file
    /// </summary>
    public async Task SaveSettingsAsync()
    {
        if (_useDatabaseMode && _repository != null)
        {
            await SaveToDatabaseAsync();
        }
        else
        {
            SaveToXml();
        }
    }

    /// <summary>
    /// Synchronous save for backward compatibility (uses XML only)
    /// </summary>
    public void SaveSettings()
    {
        SaveToXml();
    }

    private async Task SaveToDatabaseAsync()
    {
        if (_repository == null)
            return;

        try
        {
            var properties = typeof(ApplicationSettings).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (!property.CanRead)
                    continue;

                var value = property.GetValue(Settings);
                string? stringValue = value?.ToString() ?? string.Empty;
                
                await _repository.SaveSettingAsync(property.Name, stringValue);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving settings to database: {ex.Message}");
            throw;
        }
    }

    private void SaveToXml()
    {
        try
        {
            var serializer = new XmlSerializer(typeof(ApplicationSettings));
            using var writer = new StreamWriter(_filePath);
            serializer.Serialize(writer, Settings);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving application settings to XML: {ex.Message}");
            throw;
        }
    }

    private object? ConvertValue(string value, Type targetType)
    {
        if (string.IsNullOrEmpty(value))
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;

        try
        {
            if (targetType == typeof(string))
                return value;
            if (targetType == typeof(bool))
                return bool.Parse(value);
            if (targetType == typeof(int))
                return int.Parse(value);
            if (targetType == typeof(double))
                return double.Parse(value);
            
            return Convert.ChangeType(value, targetType);
        }
        catch
        {
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
        }
    }
}

