namespace MillionaireGame.Core.Settings;

/// <summary>
/// Represents the money tree configuration including prize values, safety nets, and currency settings
/// </summary>
public class MoneyTreeSettings
{
    /// <summary>
    /// First safety net position (default: 5 for Q5 = $1,000 milestone)
    /// </summary>
    public int SafetyNet1 { get; set; } = 5;

    /// <summary>
    /// Second safety net position (default: 10 for Q10 = $32,000 milestone)
    /// </summary>
    public int SafetyNet2 { get; set; } = 10;

    /// <summary>
    /// Risk mode safety net (1 = first net disabled, 2 = second net disabled)
    /// </summary>
    public int RiskModeSafetyNet { get; set; } = 2;

    /// <summary>
    /// Free mode allows custom safety net positioning
    /// </summary>
    public bool FreeSafetyNetMode { get; set; } = false;

    /// <summary>
    /// Currency symbol (e.g., "$", "€", "£", "¥")
    /// </summary>
    public string Currency { get; set; } = "$";

    /// <summary>
    /// If true, currency appears after value (e.g., "100€"), otherwise before (e.g., "$100")
    /// </summary>
    public bool CurrencyAtSuffix { get; set; } = false;

    /// <summary>
    /// Enable second currency for mixed currency trees
    /// </summary>
    public bool Currency2Enabled { get; set; } = false;

    /// <summary>
    /// Second currency symbol (e.g., "€", "£", "¥")
    /// </summary>
    public string Currency2 { get; set; } = "€";

    /// <summary>
    /// If true, second currency appears after value
    /// </summary>
    public bool Currency2AtSuffix { get; set; } = false;

    /// <summary>
    /// Stores which currency each level uses (1 or 2). Array indexed 0-14 for levels 1-15.
    /// </summary>
    public int[] LevelCurrencies { get; set; } = new int[15] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };

    // Prize values for each level (stored as integers, formatted with currency when displayed)
    public int Level01Value { get; set; } = 100;
    public int Level02Value { get; set; } = 200;
    public int Level03Value { get; set; } = 300;
    public int Level04Value { get; set; } = 500;
    public int Level05Value { get; set; } = 1_000;
    public int Level06Value { get; set; } = 2_000;
    public int Level07Value { get; set; } = 4_000;
    public int Level08Value { get; set; } = 8_000;
    public int Level09Value { get; set; } = 16_000;
    public int Level10Value { get; set; } = 32_000;
    public int Level11Value { get; set; } = 64_000;
    public int Level12Value { get; set; } = 125_000;
    public int Level13Value { get; set; } = 250_000;
    public int Level14Value { get; set; } = 500_000;
    public int Level15Value { get; set; } = 1_000_000;

    /// <summary>
    /// Gets the prize value for a specific level (1-15)
    /// </summary>
    public int GetLevelValue(int level) => level switch
    {
        1 => Level01Value,
        2 => Level02Value,
        3 => Level03Value,
        4 => Level04Value,
        5 => Level05Value,
        6 => Level06Value,
        7 => Level07Value,
        8 => Level08Value,
        9 => Level09Value,
        10 => Level10Value,
        11 => Level11Value,
        12 => Level12Value,
        13 => Level13Value,
        14 => Level14Value,
        15 => Level15Value,
        _ => 0
    };

    /// <summary>
    /// Sets the prize value for a specific level (1-15)
    /// </summary>
    public void SetLevelValue(int level, int value)
    {
        switch (level)
        {
            case 1: Level01Value = value; break;
            case 2: Level02Value = value; break;
            case 3: Level03Value = value; break;
            case 4: Level04Value = value; break;
            case 5: Level05Value = value; break;
            case 6: Level06Value = value; break;
            case 7: Level07Value = value; break;
            case 8: Level08Value = value; break;
            case 9: Level09Value = value; break;
            case 10: Level10Value = value; break;
            case 11: Level11Value = value; break;
            case 12: Level12Value = value; break;
            case 13: Level13Value = value; break;
            case 14: Level14Value = value; break;
            case 15: Level15Value = value; break;
        }
    }

    /// <summary>
    /// Formats a prize value with the configured currency symbol and position
    /// </summary>
    public string FormatMoney(int value)
    {
        var formattedValue = value.ToString("N0"); // Format with thousands separator
        return CurrencyAtSuffix 
            ? $"{formattedValue}{Currency}" 
            : $"{Currency}{formattedValue}";
    }

    /// <summary>
    /// Determines if a specific level is a safety net
    /// </summary>
    public bool IsSafetyNet(int level)
    {
        if (FreeSafetyNetMode)
        {
            // In free mode, only Q5 is always a safety net
            return level == 5;
        }
        
        // Standard mode: Q5 and Q10 are safety nets
        return level == SafetyNet1 || level == SafetyNet2;
    }

    /// <summary>
    /// Determines if a safety net is disabled in risk mode
    /// </summary>
    public bool IsSafetyNetDisabledInRiskMode(int level)
    {
        if (FreeSafetyNetMode)
            return false;
            
        if (RiskModeSafetyNet == 1)
            return level == SafetyNet1; // First net disabled
        else if (RiskModeSafetyNet == 2)
            return level == SafetyNet2; // Second net disabled
            
        return false;
    }
}
