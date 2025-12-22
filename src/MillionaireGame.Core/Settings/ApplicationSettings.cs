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
    public bool AutoShowHostScreen { get; set; } = false;
    public bool AutoShowGuestScreen { get; set; } = false;
    public bool AutoShowTVScreen { get; set; } = false;
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
            Settings = new ApplicationSettings();
            return;
        }

        try
        {
            var serializer = new XmlSerializer(typeof(ApplicationSettings));
            using var reader = new StreamReader(_filePath);
            var loadedSettings = (ApplicationSettings?)serializer.Deserialize(reader);
            if (loadedSettings != null)
            {
                Settings = loadedSettings;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading application settings from XML: {ex.Message}");
            Settings = new ApplicationSettings();
        }
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

