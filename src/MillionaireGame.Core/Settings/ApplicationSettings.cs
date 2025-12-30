using MillionaireGame.Core.Database;
using System.Reflection;

namespace MillionaireGame.Core.Settings;

/// <summary>
/// Main application settings and profile configuration
/// Stored in the database ApplicationSettings table
/// </summary>
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
    public bool ShowWebServerConsole { get; set; } = false;

    // Web Server / Audience Participation Settings
    public string AudienceServerIP { get; set; } = "127.0.0.1";
    public int AudienceServerPort { get; set; } = 5278;
    public bool AudienceServerAutoStart { get; set; } = false;
}

/// <summary>
/// Manager for application settings persistence
/// All settings are stored in and loaded from the database
/// </summary>
public class ApplicationSettingsManager
{
    private readonly ApplicationSettingsRepository _repository;

    public ApplicationSettings Settings { get; private set; }

    public ApplicationSettingsManager(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));
            
        _repository = new ApplicationSettingsRepository(connectionString);
        Settings = new ApplicationSettings();
    }

    /// <summary>
    /// Load settings from database
    /// </summary>
    public async Task LoadSettingsAsync()
    {
        await LoadFromDatabaseAsync();
    }

    /// <summary>
    /// Synchronous load wrapper - runs on background thread to avoid UI deadlocks
    /// </summary>
    public void LoadSettings()
    {
        Task.Run(async () => await LoadSettingsAsync()).GetAwaiter().GetResult();
    }

    private async Task LoadFromDatabaseAsync()
    {
        try
        {
            // Ensure table exists
            if (!await _repository.SettingsTableExistsAsync())
            {
                await _repository.CreateSettingsTableAsync();
            }

            var dbSettings = await _repository.GetAllSettingsAsync();
            
            // If no settings exist, save defaults
            if (dbSettings.Count == 0)
            {
                await SaveToDatabaseAsync();
                return;
            }
            
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
            throw;
        }
    }

    /// <summary>
    /// Save settings to database
    /// </summary>
    public async Task SaveSettingsAsync()
    {
        await SaveToDatabaseAsync();
    }

    /// <summary>
    /// Synchronous save wrapper - runs on background thread to avoid UI deadlocks
    /// </summary>
    public void SaveSettings()
    {
        Task.Run(async () => await SaveSettingsAsync()).GetAwaiter().GetResult();
    }

    private async Task SaveToDatabaseAsync()
    {
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

