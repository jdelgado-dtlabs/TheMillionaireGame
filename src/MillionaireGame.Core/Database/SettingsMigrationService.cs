using MillionaireGame.Core.Settings;
using System.Reflection;
using System.Xml.Serialization;

namespace MillionaireGame.Core.Database;

/// <summary>
/// Handles migration of settings from XML files to database
/// </summary>
public class SettingsMigrationService
{
    private readonly ApplicationSettingsRepository _repository;
    private readonly string _xmlFilePath;

    public SettingsMigrationService(ApplicationSettingsRepository repository, string? basePath = null)
    {
        _repository = repository;
        _xmlFilePath = Path.Combine(basePath ?? AppDomain.CurrentDomain.BaseDirectory, "config.xml");
    }

    /// <summary>
    /// Check if migration is needed
    /// </summary>
    public async Task<bool> MigrationNeededAsync()
    {
        // Check if table exists
        if (!await _repository.SettingsTableExistsAsync())
            return true;

        // Check if table has data
        if (!await _repository.SettingsDataExistsAsync())
        {
            // If XML file exists, we need to migrate it
            return File.Exists(_xmlFilePath);
        }

        return false;
    }

    /// <summary>
    /// Perform migration from XML to database
    /// </summary>
    public async Task<MigrationResult> MigrateAsync()
    {
        var result = new MigrationResult();
        
        try
        {
            // Step 1: Create table if it doesn't exist
            if (!await _repository.SettingsTableExistsAsync())
            {
                await _repository.CreateSettingsTableAsync();
                result.TableCreated = true;
            }

            // Step 2: Load settings from XML or use defaults
            ApplicationSettings settings;
            
            if (File.Exists(_xmlFilePath))
            {
                settings = LoadFromXml();
                result.XmlFileFound = true;
            }
            else
            {
                settings = new ApplicationSettings();
                result.UsingDefaults = true;
            }

            // Step 3: Migrate all settings to database
            await MigrateSettingsToDatabase(settings);
            result.SettingsMigrated = CountNonEmptySettings(settings);
            
            // Step 4: Backup XML file
            if (result.XmlFileFound)
            {
                BackupXmlFile();
                result.XmlBackedUp = true;
            }

            result.Success = true;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    private ApplicationSettings LoadFromXml()
    {
        try
        {
            var serializer = new XmlSerializer(typeof(ApplicationSettings));
            using var reader = new StreamReader(_xmlFilePath);
            return (ApplicationSettings?)serializer.Deserialize(reader) ?? new ApplicationSettings();
        }
        catch
        {
            return new ApplicationSettings();
        }
    }

    private async Task MigrateSettingsToDatabase(ApplicationSettings settings)
    {
        var properties = typeof(ApplicationSettings).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (!property.CanRead || !property.CanWrite)
                continue;

            var value = property.GetValue(settings);
            var category = GetCategory(property.Name);
            var description = GetDescription(property.Name);
            
            // Convert value to string
            string? stringValue = value switch
            {
                null => null,
                string s => s,
                bool b => b.ToString(),
                int i => i.ToString(),
                _ => value.ToString()
            };

            await _repository.SaveSettingAsync(property.Name, stringValue, category, description);
        }
    }

    private string GetCategory(string propertyName)
    {
        if (propertyName.StartsWith("Sound"))
        {
            if (propertyName.Contains("FFF"))
                return "Sounds - Fastest Finger First";
            if (propertyName.Contains("ATA"))
                return "Sounds - Ask the Audience";
            if (propertyName.Contains("Switch"))
                return "Sounds - Switch the Question";
            if (propertyName.Contains("5050"))
                return "Sounds - 50:50";
            if (propertyName.Contains("PlusOne") || propertyName.Contains("Phone"))
                return "Sounds - Phone a Friend";
            if (propertyName.Contains("Lifeline"))
                return "Sounds - Lifelines";
            if (propertyName.Contains("Lights") || propertyName.Contains("Bed"))
                return "Sounds - Questions";
            if (propertyName.Contains("Correct") || propertyName.Contains("Wrong") || propertyName.Contains("Final"))
                return "Sounds - Answers";
            
            return "Sounds - General";
        }

        if (propertyName.StartsWith("AutoHide") || propertyName.StartsWith("Show") || propertyName.StartsWith("Use"))
            return "Display Options";

        if (propertyName.Contains("Color"))
            return "Colors";

        if (propertyName.Contains("DB") || propertyName.Contains("Database"))
            return "Database";

        return "General";
    }

    private string GetDescription(string propertyName)
    {
        // Add human-readable descriptions for common settings
        return propertyName switch
        {
            "SoundHostStart" => "Host entrance music",
            "SoundHostEnd" => "Host closing/exit music",
            "SoundExplainRules" => "Game rules explanation music",
            "SoundQ1to5LightsDown" => "Lights down for questions 1-5",
            "Sound5050" => "50:50 lifeline activation sound",
            "SoundATAStart" => "Ask the Audience lifeline start",
            "SoundGameOver" => "Wrong answer / game over sound",
            "SoundWalkAway1" => "Walk away with small winnings",
            "SoundWalkAway2" => "Walk away with large winnings",
            _ => ""
        };
    }

    private int CountNonEmptySettings(ApplicationSettings settings)
    {
        var properties = typeof(ApplicationSettings).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        int count = 0;

        foreach (var property in properties)
        {
            if (!property.CanRead)
                continue;

            var value = property.GetValue(settings);
            if (value != null && value.ToString() != string.Empty)
                count++;
        }

        return count;
    }

    private void BackupXmlFile()
    {
        if (!File.Exists(_xmlFilePath))
            return;

        var backupPath = Path.Combine(
            Path.GetDirectoryName(_xmlFilePath) ?? "",
            $"config.xml.backup.{DateTime.Now:yyyyMMddHHmmss}");

        File.Copy(_xmlFilePath, backupPath, true);
    }
}

/// <summary>
/// Result of settings migration
/// </summary>
public class MigrationResult
{
    public bool Success { get; set; }
    public bool TableCreated { get; set; }
    public bool XmlFileFound { get; set; }
    public bool XmlBackedUp { get; set; }
    public bool UsingDefaults { get; set; }
    public int SettingsMigrated { get; set; }
    public string? ErrorMessage { get; set; }

    public override string ToString()
    {
        if (!Success)
            return $"Migration failed: {ErrorMessage}";

        var details = new List<string>();
        
        if (TableCreated)
            details.Add("Created ApplicationSettings table");
        
        if (XmlFileFound)
            details.Add($"Migrated {SettingsMigrated} settings from config.xml");
        else if (UsingDefaults)
            details.Add($"Initialized {SettingsMigrated} default settings");
        
        if (XmlBackedUp)
            details.Add("Backed up config.xml");

        return $"Migration successful: {string.Join(", ", details)}";
    }
}
