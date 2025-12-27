using System.Xml.Serialization;
using MillionaireGame.Core.Settings;

namespace MillionaireGame.Core.Services;

/// <summary>
/// Service for managing money tree settings including loading, saving, and formatting prize values
/// </summary>
public class MoneyTreeService
{
    private readonly string _settingsFilePath;
    private MoneyTreeSettings _settings;

    public MoneyTreeSettings Settings => _settings;

    public MoneyTreeService(string? settingsFilePath = null)
    {
        _settingsFilePath = settingsFilePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MillionaireGame",
            "tree.xml"
        );
        
        _settings = new MoneyTreeSettings();
        LoadSettings();
    }

    /// <summary>
    /// Loads money tree settings from XML file, creates default if not found
    /// </summary>
    public void LoadSettings()
    {
        try
        {
            if (!File.Exists(_settingsFilePath))
            {
                SaveDefaultSettings();
                return;
            }

            using var reader = new StreamReader(_settingsFilePath);
            var serializer = new XmlSerializer(typeof(MoneyTreeSettings));
            var loadedSettings = serializer.Deserialize(reader) as MoneyTreeSettings;

            if (loadedSettings != null)
            {
                _settings = loadedSettings;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading money tree settings: {ex.Message}");
            SaveDefaultSettings();
        }
    }

    /// <summary>
    /// Saves current money tree settings to XML file
    /// </summary>
    public void SaveSettings()
    {
        try
        {
            // Ensure directory exists
            var directory = Path.GetDirectoryName(_settingsFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var writer = new StreamWriter(_settingsFilePath);
            var serializer = new XmlSerializer(typeof(MoneyTreeSettings));
            serializer.Serialize(writer, _settings);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving money tree settings: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates and saves default US millionaire money tree settings
    /// </summary>
    private void SaveDefaultSettings()
    {
        _settings = new MoneyTreeSettings
        {
            SafetyNet1 = 5,  // Q5 = $1,000
            SafetyNet2 = 10, // Q10 = $32,000
            RiskModeSafetyNet = 2,
            FreeSafetyNetMode = false,
            Currency = "$",
            CurrencyAtSuffix = false,
            
            Level01Value = 100,
            Level02Value = 200,
            Level03Value = 300,
            Level04Value = 500,
            Level05Value = 1_000,
            Level06Value = 2_000,
            Level07Value = 4_000,
            Level08Value = 8_000,
            Level09Value = 16_000,
            Level10Value = 32_000,
            Level11Value = 64_000,
            Level12Value = 125_000,
            Level13Value = 250_000,
            Level14Value = 500_000,
            Level15Value = 1_000_000
        };

        SaveSettings();
    }

    /// <summary>
    /// Gets formatted money string for a specific level (1-15)
    /// </summary>
    public string GetFormattedValue(int level)
    {
        var value = _settings.GetLevelValue(level);
        
        // Check if dual currency is enabled and which currency this level uses
        if (_settings.Currency2Enabled && level >= 1 && level <= 15)
        {
            int currencyIndex = _settings.LevelCurrencies[level - 1];
            if (currencyIndex == 2)
            {
                // Use Currency 2
                var formattedValue = value.ToString("N0");
                return _settings.Currency2AtSuffix 
                    ? $"{formattedValue}{_settings.Currency2}" 
                    : $"{_settings.Currency2}{formattedValue}";
            }
        }
        
        // Use Currency 1 (default)
        return _settings.FormatMoney(value);
    }

    /// <summary>
    /// Gets the raw integer value for a specific level (1-15)
    /// </summary>
    public int GetValue(int level)
    {
        return _settings.GetLevelValue(level);
    }

    /// <summary>
    /// Gets the value a player receives when answering wrong at a specific level
    /// </summary>
    public string GetWrongValue(int level, bool isRiskMode)
    {
        int wrongValue;

        if (_settings.FreeSafetyNetMode)
        {
            // Free mode: only Q5 is guaranteed
            wrongValue = level >= 5 ? _settings.Level05Value : 0;
        }
        else
        {
            if (isRiskMode)
            {
                // Risk mode: one safety net disabled
                if (_settings.RiskModeSafetyNet == 1)
                {
                    // First net disabled: only Q10 protects
                    wrongValue = level >= 10 ? _settings.Level10Value : 0;
                }
                else
                {
                    // Second net disabled: only Q5 protects
                    wrongValue = level >= 5 ? _settings.Level05Value : 0;
                }
            }
            else
            {
                // Normal mode: both safety nets active
                if (level >= 10)
                    wrongValue = _settings.Level10Value;
                else if (level >= 5)
                    wrongValue = _settings.Level05Value;
                else
                    wrongValue = 0;
            }
        }

        return _settings.FormatMoney(wrongValue);
    }

    /// <summary>
    /// Gets the value a player receives when walking away at a specific level
    /// </summary>
    public string GetDropValue(int level, bool isRiskMode)
    {
        // When walking away, player gets the current level's value
        var value = _settings.GetLevelValue(level);
        return _settings.FormatMoney(value);
    }

    /// <summary>
    /// Checks if a specific level is a safety net
    /// </summary>
    public bool IsSafetyNet(int level)
    {
        return _settings.IsSafetyNet(level);
    }

    /// <summary>
    /// Checks if a safety net is disabled in risk mode at a specific level
    /// </summary>
    public bool IsSafetyNetDisabledInRiskMode(int level, Models.GameMode currentMode)
    {
        return _settings.IsSafetyNetDisabledInRiskMode(level, currentMode);
    }

    /// <summary>
    /// Updates a level's prize value
    /// </summary>
    public void UpdateLevelValue(int level, int value)
    {
        _settings.SetLevelValue(level, value);
    }

    /// <summary>
    /// Updates currency settings
    /// </summary>
    public void UpdateCurrency(string currency, bool suffixPosition)
    {
        _settings.Currency = currency;
        _settings.CurrencyAtSuffix = suffixPosition;
    }

    /// <summary>
    /// Gets the display level for winnings and money tree, considering GameWin flag
    /// Tree shows what you've won, not what you're playing for
    /// If GameWin is true, show level 15 (top prize won)
    /// Otherwise, show currentLevel - 1 (winnings), floored at 0
    /// </summary>
    public int GetDisplayLevel(int currentLevel, bool gameWin)
    {
        if (gameWin) return 15; // Show top prize
        return Math.Max(0, currentLevel - 1); // Show winnings, never negative
    }

    /// <summary>
    /// Updates safety net configuration
    /// </summary>
    public void UpdateSafetyNets(int safetyNet1, int safetyNet2, int riskModeSafetyNet, bool freeMode)
    {
        _settings.SafetyNet1 = safetyNet1;
        _settings.SafetyNet2 = safetyNet2;
        _settings.RiskModeSafetyNet = riskModeSafetyNet;
        _settings.FreeSafetyNetMode = freeMode;
    }
}
