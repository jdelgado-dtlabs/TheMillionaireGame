using MillionaireGame.Core.Database;
using MillionaireGame.Core.Models;

namespace MillionaireGame.Core.Services;

/// <summary>
/// Service for managing theme operations and business logic
/// </summary>
public class ThemeService : IDisposable
{
    private readonly ThemeRepository _themeRepository;
    private readonly ThemeBackgroundRepository _backgroundRepository;
    private readonly ThemeStrapRepository _strapRepository;
    private readonly ThemeMoneyTreeRepository _moneyTreeRepository;
    private readonly ThemePackRepository _packRepository;
    private Theme? _currentTheme;
    private bool _disposed;

    public ThemeService(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));

        _themeRepository = new ThemeRepository(connectionString);
        _backgroundRepository = new ThemeBackgroundRepository(connectionString);
        _strapRepository = new ThemeStrapRepository(connectionString);
        _moneyTreeRepository = new ThemeMoneyTreeRepository(connectionString);
        _packRepository = new ThemePackRepository(connectionString);
    }

    /// <summary>
    /// Get the currently active theme
    /// </summary>
    public Theme? CurrentTheme => _currentTheme;

    /// <summary>
    /// Load the active theme from the database
    /// </summary>
    public async Task LoadActiveThemeAsync()
    {
        _currentTheme = await _themeRepository.GetActiveThemeAsync();
    }

    /// <summary>
    /// Get all available themes
    /// </summary>
    public async Task<List<Theme>> GetAllThemesAsync()
    {
        return await _themeRepository.GetAllThemesAsync();
    }

    /// <summary>
    /// Get themes by type (Preset, UserProfile1, UserProfile2, Custom)
    /// </summary>
    public async Task<List<Theme>> GetThemesByTypeAsync(string themeType)
    {
        return await _themeRepository.GetThemesByTypeAsync(themeType);
    }

    /// <summary>
    /// Get a complete theme with all components (backgrounds, straps, money tree)
    /// </summary>
    public async Task<CompleteTheme?> GetCompleteThemeAsync(int themeId)
    {
        var theme = await _themeRepository.GetThemeByIdAsync(themeId);
        if (theme == null)
            return null;

        var backgrounds = await _backgroundRepository.GetBackgroundsByThemeIdAsync(themeId);
        var straps = await _strapRepository.GetStrapsByThemeIdAsync(themeId);
        var moneyTree = await _moneyTreeRepository.GetMoneyTreeByThemeIdAsync(themeId);

        return new CompleteTheme
        {
            Theme = theme,
            Backgrounds = backgrounds,
            Straps = straps,
            MoneyTree = moneyTree
        };
    }

    /// <summary>
    /// Apply a theme (set it as active)
    /// </summary>
    public async Task ApplyThemeAsync(int themeId)
    {
        await _themeRepository.SetActiveThemeAsync(themeId);
        await LoadActiveThemeAsync();
    }

    /// <summary>
    /// Create a new custom theme
    /// </summary>
    public async Task<int> CreateThemeAsync(string themeName, string themeType, string? description = null)
    {
        // Validate theme type
        if (!IsValidThemeType(themeType))
            throw new ArgumentException($"Invalid theme type: {themeType}", nameof(themeType));

        // Check if theme name already exists
        if (await _themeRepository.ThemeExistsAsync(themeName))
            throw new InvalidOperationException($"Theme with name '{themeName}' already exists");

        var theme = new Theme
        {
            ThemeName = themeName,
            ThemeType = themeType,
            Description = description,
            Author = "User",
            Version = "1.0.0",
            IsActive = false
        };

        return await _themeRepository.SaveThemeAsync(theme);
    }

    /// <summary>
    /// Save a complete theme with all components
    /// </summary>
    public async Task<int> SaveCompleteThemeAsync(CompleteTheme completeTheme)
    {
        // Validate theme
        ValidateTheme(completeTheme);

        // Save theme
        var themeId = await _themeRepository.SaveThemeAsync(completeTheme.Theme);

        // Update component theme IDs if this is a new theme
        if (completeTheme.Theme.ThemeId == 0)
        {
            foreach (var bg in completeTheme.Backgrounds)
                bg.ThemeId = themeId;
            foreach (var strap in completeTheme.Straps)
                strap.ThemeId = themeId;
            if (completeTheme.MoneyTree != null)
                completeTheme.MoneyTree.ThemeId = themeId;
        }

        // Save components
        foreach (var background in completeTheme.Backgrounds)
        {
            await _backgroundRepository.SaveBackgroundAsync(background);
        }

        foreach (var strap in completeTheme.Straps)
        {
            await _strapRepository.SaveStrapAsync(strap);
        }

        if (completeTheme.MoneyTree != null)
        {
            await _moneyTreeRepository.SaveMoneyTreeAsync(completeTheme.MoneyTree);
        }

        return themeId;
    }

    /// <summary>
    /// Delete a theme and all its components
    /// </summary>
    public async Task DeleteThemeAsync(int themeId)
    {
        // Don't allow deletion of preset themes
        var theme = await _themeRepository.GetThemeByIdAsync(themeId);
        if (theme?.ThemeType == "Preset")
            throw new InvalidOperationException("Cannot delete preset themes");

        // Don't allow deletion of active theme
        if (theme?.IsActive == true)
            throw new InvalidOperationException("Cannot delete the active theme. Please activate another theme first.");

        // Delete theme (cascade will handle components)
        await _themeRepository.DeleteThemeAsync(themeId);
    }

    /// <summary>
    /// Duplicate a theme (for creating user profiles from presets)
    /// </summary>
    public async Task<int> DuplicateThemeAsync(int sourceThemeId, string newName, string newType)
    {
        var source = await GetCompleteThemeAsync(sourceThemeId);
        if (source == null)
            throw new ArgumentException($"Source theme not found: {sourceThemeId}", nameof(sourceThemeId));

        // Create new theme
        var newTheme = new Theme
        {
            ThemeName = newName,
            ThemeType = newType,
            Description = $"Based on {source.Theme.ThemeName}",
            Author = "User",
            Version = "1.0.0",
            IsActive = false
        };

        var newThemeId = await _themeRepository.SaveThemeAsync(newTheme);

        // Copy backgrounds
        foreach (var bg in source.Backgrounds)
        {
            var newBg = new ThemeBackground
            {
                ThemeId = newThemeId,
                ComponentType = bg.ComponentType,
                ImagePath = bg.ImagePath,
                ChromaKeyEnabled = bg.ChromaKeyEnabled,
                ChromaKeyColor = bg.ChromaKeyColor,
                ChromaKeyTolerance = bg.ChromaKeyTolerance,
                ScaleMode = bg.ScaleMode,
                PositionX = bg.PositionX,
                PositionY = bg.PositionY,
                Transparency = bg.Transparency
            };
            await _backgroundRepository.SaveBackgroundAsync(newBg);
        }

        // Copy straps
        foreach (var strap in source.Straps)
        {
            var newStrap = new ThemeStrap
            {
                ThemeId = newThemeId,
                StrapType = strap.StrapType,
                SvgShape = strap.SvgShape,
                PrimaryColor = strap.PrimaryColor,
                SecondaryColor = strap.SecondaryColor,
                GradientEnabled = strap.GradientEnabled,
                GradientAngle = strap.GradientAngle,
                EffectType = strap.EffectType,
                EffectIntensity = strap.EffectIntensity,
                EffectColor = strap.EffectColor,
                BorderEnabled = strap.BorderEnabled,
                BorderColor = strap.BorderColor,
                BorderWidth = strap.BorderWidth,
                BorderStyle = strap.BorderStyle,
                FontFamily = strap.FontFamily,
                FontSize = strap.FontSize,
                FontColor = strap.FontColor,
                FontBold = strap.FontBold,
                FontItalic = strap.FontItalic,
                AnimationEnabled = strap.AnimationEnabled,
                AnimationType = strap.AnimationType,
                AnimationDuration = strap.AnimationDuration
            };
            await _strapRepository.SaveStrapAsync(newStrap);
        }

        // Copy money tree
        if (source.MoneyTree != null)
        {
            var newMoneyTree = new ThemeMoneyTree
            {
                ThemeId = newThemeId,
                BackgroundImagePath = source.MoneyTree.BackgroundImagePath,
                InactiveColor = source.MoneyTree.InactiveColor,
                ActiveColor = source.MoneyTree.ActiveColor,
                CompletedColor = source.MoneyTree.CompletedColor,
                SafeHavenColor = source.MoneyTree.SafeHavenColor,
                HighlightEnabled = source.MoneyTree.HighlightEnabled,
                HighlightType = source.MoneyTree.HighlightType,
                HighlightColor = source.MoneyTree.HighlightColor,
                HighlightIntensity = source.MoneyTree.HighlightIntensity,
                FontFamily = source.MoneyTree.FontFamily,
                FontSize = source.MoneyTree.FontSize,
                FontBold = source.MoneyTree.FontBold
            };
            await _moneyTreeRepository.SaveMoneyTreeAsync(newMoneyTree);
        }

        return newThemeId;
    }

    /// <summary>
    /// Create a dark/black variant of an existing theme by name.
    /// Duplicates the source theme and then adjusts strap and money-tree colors
    /// to a grey/black palette similar to legacy PNG straps.
    /// </summary>
    public async Task<int> CreateClassicBlackVariantAsync(string sourceThemeName, string newThemeName)
    {
        if (string.IsNullOrWhiteSpace(sourceThemeName))
            throw new ArgumentException("Source theme name required", nameof(sourceThemeName));

        if (string.IsNullOrWhiteSpace(newThemeName))
            throw new ArgumentException("New theme name required", nameof(newThemeName));

        // Find source by name
        var all = await GetAllThemesAsync();
        var source = all.FirstOrDefault(t => string.Equals(t.ThemeName, sourceThemeName, StringComparison.OrdinalIgnoreCase));
        if (source == null)
            throw new InvalidOperationException($"Source theme not found: {sourceThemeName}");

        // Ensure new theme doesn't already exist
        if (await _themeRepository.ThemeExistsAsync(newThemeName))
            throw new InvalidOperationException($"Theme already exists: {newThemeName}");

        // Duplicate as a Preset so it appears with other presets
        var newThemeId = await DuplicateThemeAsync(source.ThemeId, newThemeName, "Preset");

        // Load the duplicated complete theme and modify colors
        var complete = await GetCompleteThemeAsync(newThemeId);
        if (complete == null)
            throw new InvalidOperationException("Failed to load duplicated theme");

        // Apply grey/black palette to straps
        foreach (var strap in complete.Straps)
        {
            strap.PrimaryColor = "#1f1f1f"; // dark grey
            strap.SecondaryColor = "#4b4b4b"; // lighter grey for gradient
            strap.GradientEnabled = true;
            strap.GradientAngle = 90;
            strap.BorderEnabled = true;
            strap.BorderColor = "#000000";
            strap.BorderWidth = Math.Max(1, strap.BorderWidth);
            // Preserve font settings but ensure font color is light for contrast
            strap.FontColor = "#FFFFFF";
        }

        // Update money tree colors if present
        if (complete.MoneyTree != null)
        {
            complete.MoneyTree.InactiveColor = "#2b2b2b";
            complete.MoneyTree.ActiveColor = "#3f3f3f";
            complete.MoneyTree.CompletedColor = "#ffffff";
            complete.MoneyTree.SafeHavenColor = "#666666";
            complete.MoneyTree.HighlightColor = "#FFD700"; // keep gold highlight for readability
        }

        // Save changes
        await SaveCompleteThemeAsync(complete);

        return newThemeId;
    }

    /// <summary>
    /// Validate theme data
    /// </summary>
    private void ValidateTheme(CompleteTheme theme)
    {
        if (string.IsNullOrWhiteSpace(theme.Theme.ThemeName))
            throw new ArgumentException("Theme name is required");

        if (!IsValidThemeType(theme.Theme.ThemeType))
            throw new ArgumentException($"Invalid theme type: {theme.Theme.ThemeType}");

        // Validate at least one background
        if (theme.Backgrounds == null || theme.Backgrounds.Count == 0)
            throw new ArgumentException("Theme must have at least one background");

        // Validate color format for straps
        foreach (var strap in theme.Straps)
        {
            if (!IsValidHexColor(strap.PrimaryColor))
                throw new ArgumentException($"Invalid primary color format: {strap.PrimaryColor}");
            
            if (strap.SecondaryColor != null && !IsValidHexColor(strap.SecondaryColor))
                throw new ArgumentException($"Invalid secondary color format: {strap.SecondaryColor}");
        }
    }

    /// <summary>
    /// Check if theme type is valid
    /// </summary>
    private bool IsValidThemeType(string themeType)
    {
        return themeType switch
        {
            "Preset" => true,
            "UserProfile1" => true,
            "UserProfile2" => true,
            "Custom" => true,
            _ => false
        };
    }

    /// <summary>
    /// Check if a string is a valid hex color (#RRGGBB or #RGB)
    /// </summary>
    private bool IsValidHexColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color))
            return false;

        if (!color.StartsWith("#"))
            return false;

        var hex = color.Substring(1);
        return hex.Length == 6 || hex.Length == 3;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            // Repositories don't implement IDisposable, nothing to dispose
            _disposed = true;
        }
    }
}

/// <summary>
/// Complete theme data including all components
/// </summary>
public class CompleteTheme
{
    public Theme Theme { get; set; } = new();
    public List<ThemeBackground> Backgrounds { get; set; } = new();
    public List<ThemeStrap> Straps { get; set; } = new();
    public ThemeMoneyTree? MoneyTree { get; set; }
}
