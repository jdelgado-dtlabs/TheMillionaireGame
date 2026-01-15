namespace MillionaireGame.Core.Models;

/// <summary>
/// Represents a visual theme for the game
/// </summary>
public class Theme
{
    public int ThemeId { get; set; }
    public string ThemeName { get; set; } = string.Empty;
    public string ThemeType { get; set; } = "Custom"; // Preset, UserProfile1, UserProfile2, Custom
    public int? ThemePackId { get; set; }
    public bool IsActive { get; set; }
    public string? Description { get; set; }
    public string? Author { get; set; }
    public string? Version { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}

/// <summary>
/// Represents a background configuration for a theme component
/// </summary>
public class ThemeBackground
{
    public int ThemeBackgroundId { get; set; }
    public int ThemeId { get; set; }
    public string ComponentType { get; set; } = "TVScreen"; // TVScreen, MoneyTree, General
    public string? ImagePath { get; set; }
    public bool ChromaKeyEnabled { get; set; }
    public string? ChromaKeyColor { get; set; }
    public int ChromaKeyTolerance { get; set; } = 50;
    public string ScaleMode { get; set; } = "Fill"; // Stretch, Fill, Fit, Center
    public int PositionX { get; set; }
    public int PositionY { get; set; }
    public int Transparency { get; set; } = 100;
}

/// <summary>
/// Represents a strap (question/answer overlay) configuration
/// </summary>
public class ThemeStrap
{
    public int ThemeStrapId { get; set; }
    public int ThemeId { get; set; }
    public string StrapType { get; set; } = "Question"; // Question, Answer, MoneyAmount, PlayerName, HostMessage
    public string SvgShape { get; set; } = "Classic";
    
    // Colors
    public string PrimaryColor { get; set; } = "#8B4513";
    public string? SecondaryColor { get; set; }
    public bool GradientEnabled { get; set; }
    public int GradientAngle { get; set; } = 90;
    
    // Effects
    public string? EffectType { get; set; }
    public int EffectIntensity { get; set; } = 50;
    public string? EffectColor { get; set; }
    
    // Border
    public bool BorderEnabled { get; set; } = true;
    public string BorderColor { get; set; } = "#000000";
    public int BorderWidth { get; set; } = 2;
    public string BorderStyle { get; set; } = "Solid";
    
    // Typography
    public string FontFamily { get; set; } = "Arial";
    public int FontSize { get; set; } = 24;
    public string FontColor { get; set; } = "#FFFFFF";
    public bool FontBold { get; set; }
    public bool FontItalic { get; set; }
    
    // Animation
    public bool AnimationEnabled { get; set; }
    public string? AnimationType { get; set; }
    public int AnimationDuration { get; set; } = 500;
}

/// <summary>
/// Represents money tree display configuration
/// </summary>
public class ThemeMoneyTree
{
    public int ThemeMoneyTreeId { get; set; }
    public int ThemeId { get; set; }
    public string? BackgroundImagePath { get; set; }
    
    // Colors
    public string InactiveColor { get; set; } = "#808080";
    public string ActiveColor { get; set; } = "#FFD700";
    public string CompletedColor { get; set; } = "#00FF00";
    public string SafeHavenColor { get; set; } = "#0080FF";
    
    // Highlight
    public bool HighlightEnabled { get; set; } = true;
    public string HighlightType { get; set; } = "PulsingGlow";
    public string HighlightColor { get; set; } = "#FFFF00";
    public int HighlightIntensity { get; set; } = 80;
    
    // Typography
    public string FontFamily { get; set; } = "Arial Bold";
    public int FontSize { get; set; } = 18;
    public bool FontBold { get; set; } = true;
}

/// <summary>
/// Represents a theme pack (imported collection of themes with assets)
/// </summary>
public class ThemePack
{
    public int ThemePackId { get; set; }
    public string PackName { get; set; } = string.Empty;
    public string PackVersion { get; set; } = "1.0.0";
    public string? Author { get; set; }
    public string? Description { get; set; }
    public string InstallPath { get; set; } = string.Empty;
    public DateTime ImportDate { get; set; }
}
