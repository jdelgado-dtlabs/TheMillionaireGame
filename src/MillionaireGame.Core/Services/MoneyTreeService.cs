using MillionaireGame.Core.Settings;
using MillionaireGame.Core.Database;

namespace MillionaireGame.Core.Services;

/// <summary>
/// Service for managing money tree settings including loading, saving, and formatting prize values
/// </summary>
public class MoneyTreeService
{
    private readonly ApplicationSettingsRepository _repository;
    private MoneyTreeSettings _settings;

    public MoneyTreeSettings Settings => _settings;

    public MoneyTreeService(string? connectionString = null)
    {
        _repository = new ApplicationSettingsRepository(connectionString ?? GetDefaultConnectionString());
        _settings = new MoneyTreeSettings();
        LoadSettings();
    }
    
    private static string GetDefaultConnectionString()
    {
        var sqlSettings = new SqlSettings();
        sqlSettings.LoadSettings();
        return sqlSettings.GetConnectionString();
    }

    /// <summary>
    /// Loads money tree settings from database, creates default if not found
    /// </summary>
    public void LoadSettings()
    {
        try
        {
            var task = LoadSettingsAsync();
            task.Wait();
        }
        catch
        {
            // Use default settings on load error
            SaveDefaultSettings();
        }
    }
    
    private async Task LoadSettingsAsync()
    {
        // Ensure table exists
        if (!await _repository.SettingsTableExistsAsync())
        {
            await _repository.CreateSettingsTableAsync();
        }
        
        var dbSettings = await _repository.GetAllSettingsAsync();
        
        // If no MoneyTree settings exist, save defaults
        if (!dbSettings.Any(kvp => kvp.Key.StartsWith("MoneyTree.")))
        {
            SaveDefaultSettings();
            return;
        }
        
        // Load from database
        LoadFromDictionary(dbSettings);
    }
    
    private void LoadFromDictionary(Dictionary<string, string> dbSettings)
    {
        // Load each property from database
        foreach (var property in typeof(MoneyTreeSettings).GetProperties())
        {
            var key = $"MoneyTree.{property.Name}";
            if (dbSettings.TryGetValue(key, out var value))
            {
                try
                {
                    if (property.PropertyType == typeof(int))
                    {
                        property.SetValue(_settings, int.Parse(value));
                    }
                    else if (property.PropertyType == typeof(bool))
                    {
                        property.SetValue(_settings, bool.Parse(value));
                    }
                    else if (property.PropertyType == typeof(string))
                    {
                        property.SetValue(_settings, value);
                    }
                    else if (property.PropertyType == typeof(int[]))
                    {
                        var values = value.Split(',').Select(int.Parse).ToArray();
                        property.SetValue(_settings, values);
                    }
                }
                catch
                {
                    // Skip invalid values
                }
            }
        }
    }

    /// <summary>
    /// Saves current money tree settings to database
    /// </summary>
    public void SaveSettings()
    {
        try
        {
            var task = SaveSettingsAsync();
            task.Wait();
        }
        catch
        {
            // Silently ignore save errors
        }
    }
    
    private async Task SaveSettingsAsync()
    {
        // Ensure table exists
        if (!await _repository.SettingsTableExistsAsync())
        {
            await _repository.CreateSettingsTableAsync();
        }
        
        // Save each property to database
        foreach (var property in typeof(MoneyTreeSettings).GetProperties())
        {
            var key = $"MoneyTree.{property.Name}";
            var value = property.GetValue(_settings);
            string? stringValue = null;
            
            if (value != null)
            {
                if (property.PropertyType == typeof(int[]))
                {
                    stringValue = string.Join(",", (int[])value);
                }
                else
                {
                    stringValue = value.ToString();
                }
            }
            
            await _repository.SaveSettingAsync(key, stringValue, "MoneyTree", property.Name);
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
    /// Calculate winnings breakdown by currency
    /// </summary>
    /// <param name="highestQuestionReached">The highest question the player reached (determines which currencies were played)</param>
    /// <param name="actualWinningLevel">The level they actually won (considering safety nets)</param>
    public (string currency1Display, string currency2Display, bool hasCurrency1, bool hasCurrency2) GetCurrencyBreakdown(int highestQuestionReached, int actualWinningLevel)
    {
        int currency1Value = 0;
        int currency2Value = 0;
        bool hasCurrency1 = false;
        bool hasCurrency2 = false;

        // Loop from the actual winning level down to 1 to find the highest value in each currency
        for (int level = actualWinningLevel; level >= 1; level--)
        {
            int currencyIndex = _settings.LevelCurrencies[level - 1];

            // Only set if we haven't found a value for this currency yet (we want the highest)
            if (currencyIndex == 1 && !hasCurrency1)
            {
                currency1Value = _settings.GetLevelValue(level);
                hasCurrency1 = true;
            }
            else if (currencyIndex == 2 && _settings.Currency2Enabled && !hasCurrency2)
            {
                currency2Value = _settings.GetLevelValue(level);
                hasCurrency2 = true;
            }

            // If we've found both currencies, we can stop
            if (hasCurrency1 && hasCurrency2)
                break;
        }

        // Format the amounts with their respective currencies
        string currency1Display = "";
        string currency2Display = "";

        if (hasCurrency1)
        {
            var formatted = currency1Value.ToString("N0");
            currency1Display = _settings.CurrencyAtSuffix
                ? $"{formatted}{_settings.Currency}"
                : $"{_settings.Currency}{formatted}";
        }

        if (hasCurrency2)
        {
            var formatted = currency2Value.ToString("N0");
            currency2Display = _settings.Currency2AtSuffix
                ? $"{formatted}{_settings.Currency2}"
                : $"{_settings.Currency2}{formatted}";
        }

        return (currency1Display, currency2Display, hasCurrency1, hasCurrency2);
    }

    /// <summary>
    /// Calculate winnings breakdown by currency (legacy single-parameter version)
    /// </summary>
    public (string currency1Display, string currency2Display, bool hasCurrency1, bool hasCurrency2) GetCurrencyBreakdown(int finalQuestionReached)
    {
        return GetCurrencyBreakdown(finalQuestionReached, finalQuestionReached);
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
