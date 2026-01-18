using System.Xml.Linq;
using MillionaireGame.Core.Models;
using MillionaireGame.Core.Database;

namespace MillionaireGame.Core.Services;

/// <summary>
/// Manages theme pack import/export operations (XML-based with database storage).
/// Similar pattern to SoundPackManager but themes are stored in SQL database.
/// </summary>
public class ThemePackManager
{
    private readonly ThemeService _themeService;

    public ThemePackManager(ThemeService themeService)
    {
        _themeService = themeService;
    }

    /// <summary>
    /// Import a theme pack from a zip file containing themepack.xml
    /// </summary>
    public async Task<(bool Success, string Message)> ImportThemePackAsync(string zipPath)
    {
        try
        {
            // Validate zip file exists
            if (!File.Exists(zipPath))
            {
                return (false, "Zip file not found.");
            }

            // Extract to temp directory first to validate
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, tempDir);

                // Look for themepack.xml
                var xmlPath = Path.Combine(tempDir, "themepack.xml");
                if (!File.Exists(xmlPath))
                {
                    return (false, "No themepack.xml found in zip file.");
                }

                // Load and parse XML
                var doc = XDocument.Load(xmlPath);
                var root = doc.Root;
                if (root == null || root.Name != "ThemePack")
                {
                    return (false, "Invalid themepack.xml structure (missing <ThemePack> root).");
                }

                var packNameElement = root.Element("PackName");
                if (packNameElement == null || string.IsNullOrWhiteSpace(packNameElement.Value))
                {
                    return (false, "PackName not found or empty in themepack.xml");
                }

                var packName = packNameElement.Value.Trim();

                // Validate pack name
                if (string.Equals(packName, "Default", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(packName, "Classic Gold", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(packName, "Classic Black", StringComparison.OrdinalIgnoreCase) ||
                    packName.StartsWith("Preset:", StringComparison.OrdinalIgnoreCase))
                {
                    return (false, $"Theme name '{packName}' is reserved and cannot be used.");
                }

                // Parse theme from XML
                var theme = ParseThemeFromXml(root);
                if (theme == null)
                {
                    return (false, "Failed to parse theme data from themepack.xml");
                }

                // Override theme name with pack name
                theme.Theme.ThemeName = packName;
                theme.Theme.ThemeType = "Custom";
                theme.Theme.Author = root.Element("Author")?.Value?.Trim() ?? "Unknown";
                theme.Theme.Description = root.Element("Description")?.Value?.Trim() ?? "";
                theme.Theme.Version = root.Element("Version")?.Value?.Trim() ?? "1.0.0";

                // Check if theme with same name exists
                var existingThemes = await _themeService.GetAllThemesAsync();
                var existing = existingThemes.FirstOrDefault(t => 
                    string.Equals(t.ThemeName, packName, StringComparison.OrdinalIgnoreCase));

                if (existing != null)
                {
                    return (false, $"A theme named '{packName}' already exists. Please rename it or remove the existing theme first.");
                }

                // Save complete theme to database (using SaveCompleteThemeAsync)
                theme.Theme.ThemeId = 0; // Ensure it's treated as new
                var themeId = await _themeService.SaveCompleteThemeAsync(theme);
                if (themeId <= 0)
                {
                    return (false, "Failed to save theme to database.");
                }

                return (true, $"Theme '{packName}' imported successfully!");
            }
            finally
            {
                // Clean up temp directory
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }
        catch (Exception ex)
        {
            return (false, $"Error importing theme pack: {ex.Message}");
        }
    }

    /// <summary>
    /// Export a theme as a zip file containing themepack.xml
    /// </summary>
    public async Task<(bool Success, string Message)> ExportThemePackAsync(int themeId, string savePath)
    {
        try
        {
            var theme = await _themeService.GetCompleteThemeAsync(themeId);
            if (theme == null)
            {
                return (false, "Theme not found.");
            }

            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                // Generate XML
                var xmlContent = GenerateThemeXml(theme);
                File.WriteAllText(Path.Combine(tempDir, "themepack.xml"), xmlContent);

                // Create instructions file
                var instructions = CreateInstructionsText();
                File.WriteAllText(Path.Combine(tempDir, "INSTRUCTIONS.txt"), instructions);

                // Create zip file
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                }

                System.IO.Compression.ZipFile.CreateFromDirectory(tempDir, savePath);

                return (true, $"Theme '{theme.Theme.ThemeName}' exported successfully!");
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }
        catch (Exception ex)
        {
            return (false, $"Error exporting theme pack: {ex.Message}");
        }
    }

    /// <summary>
    /// Export an example theme pack template
    /// </summary>
    public bool ExportExamplePack(string savePath)
    {
        try
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                // Create example XML
                var exampleXml = CreateExampleThemeXml();
                File.WriteAllText(Path.Combine(tempDir, "themepack.xml"), exampleXml);

                // Create instructions file
                var instructions = CreateInstructionsText();
                File.WriteAllText(Path.Combine(tempDir, "INSTRUCTIONS.txt"), instructions);

                // Create zip file
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                }

                System.IO.Compression.ZipFile.CreateFromDirectory(tempDir, savePath);

                return true;
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }
        catch
        {
            return false;
        }
    }

    private CompleteTheme? ParseThemeFromXml(XElement root)
    {
        try
        {
            var theme = new Theme();
            var backgrounds = new List<ThemeBackground>();
            var straps = new List<ThemeStrap>();
            ThemeMoneyTree? moneyTree = null;

            // Parse straps
            var strapsElement = root.Element("Straps");
            if (strapsElement != null)
            {
                foreach (var strapElement in strapsElement.Elements("Strap"))
                {
                    var strap = new ThemeStrap
                    {
                        StrapType = strapElement.Attribute("Type")?.Value ?? "Question",
                        SvgShape = strapElement.Element("SvgShape")?.Value ?? "Classic",
                        PrimaryColor = strapElement.Element("PrimaryColor")?.Value ?? "#8B4513",
                        SecondaryColor = strapElement.Element("SecondaryColor")?.Value,
                        GradientEnabled = bool.Parse(strapElement.Element("GradientEnabled")?.Value ?? "false"),
                        GradientAngle = int.Parse(strapElement.Element("GradientAngle")?.Value ?? "90"),
                        EffectType = strapElement.Element("EffectType")?.Value,
                        EffectIntensity = int.Parse(strapElement.Element("EffectIntensity")?.Value ?? "50"),
                        EffectColor = strapElement.Element("EffectColor")?.Value,
                        BorderEnabled = bool.Parse(strapElement.Element("BorderEnabled")?.Value ?? "true"),
                        BorderColor = strapElement.Element("BorderColor")?.Value ?? "#000000",
                        BorderWidth = int.Parse(strapElement.Element("BorderWidth")?.Value ?? "2"),
                        BorderStyle = strapElement.Element("BorderStyle")?.Value ?? "Solid",
                        FontFamily = strapElement.Element("FontFamily")?.Value ?? "Arial",
                        FontSize = int.Parse(strapElement.Element("FontSize")?.Value ?? "24"),
                        FontColor = strapElement.Element("FontColor")?.Value ?? "#FFFFFF",
                        FontBold = bool.Parse(strapElement.Element("FontBold")?.Value ?? "false"),
                        FontItalic = bool.Parse(strapElement.Element("FontItalic")?.Value ?? "false"),
                        AnimationEnabled = bool.Parse(strapElement.Element("AnimationEnabled")?.Value ?? "false"),
                        AnimationType = strapElement.Element("AnimationType")?.Value,
                        AnimationDuration = int.Parse(strapElement.Element("AnimationDuration")?.Value ?? "500")
                    };
                    straps.Add(strap);
                }
            }

            // Parse money tree
            var moneyTreeElement = root.Element("MoneyTree");
            if (moneyTreeElement != null)
            {
                moneyTree = new ThemeMoneyTree
                {
                    InactiveColor = moneyTreeElement.Element("InactiveColor")?.Value ?? "#808080",
                    ActiveColor = moneyTreeElement.Element("ActiveColor")?.Value ?? "#FFD700",
                    CompletedColor = moneyTreeElement.Element("CompletedColor")?.Value ?? "#00FF00",
                    SafeHavenColor = moneyTreeElement.Element("SafeHavenColor")?.Value ?? "#0080FF",
                    HighlightEnabled = bool.Parse(moneyTreeElement.Element("HighlightEnabled")?.Value ?? "true"),
                    HighlightType = moneyTreeElement.Element("HighlightType")?.Value ?? "PulsingGlow",
                    HighlightColor = moneyTreeElement.Element("HighlightColor")?.Value ?? "#FFFF00",
                    HighlightIntensity = int.Parse(moneyTreeElement.Element("HighlightIntensity")?.Value ?? "80"),
                    FontFamily = moneyTreeElement.Element("FontFamily")?.Value ?? "Arial Bold",
                    FontSize = int.Parse(moneyTreeElement.Element("FontSize")?.Value ?? "18"),
                    FontBold = bool.Parse(moneyTreeElement.Element("FontBold")?.Value ?? "true")
                };
            }

            return new CompleteTheme
            {
                Theme = theme,
                Backgrounds = backgrounds,
                Straps = straps,
                MoneyTree = moneyTree
            };
        }
        catch
        {
            return null;
        }
    }

    private string GenerateThemeXml(CompleteTheme theme)
    {
        var doc = new XDocument(
            new XElement("ThemePack",
                new XElement("PackName", theme.Theme.ThemeName),
                new XElement("Author", theme.Theme.Author ?? "Unknown"),
                new XElement("Version", theme.Theme.Version ?? "1.0.0"),
                new XElement("Description", theme.Theme.Description ?? ""),
                new XElement("Straps",
                    theme.Straps.Select(s => new XElement("Strap",
                        new XAttribute("Type", s.StrapType),
                        new XElement("SvgShape", s.SvgShape),
                        new XElement("PrimaryColor", s.PrimaryColor),
                        new XElement("SecondaryColor", s.SecondaryColor ?? ""),
                        new XElement("GradientEnabled", s.GradientEnabled),
                        new XElement("GradientAngle", s.GradientAngle),
                        new XElement("EffectType", s.EffectType ?? ""),
                        new XElement("EffectIntensity", s.EffectIntensity),
                        new XElement("EffectColor", s.EffectColor ?? ""),
                        new XElement("BorderEnabled", s.BorderEnabled),
                        new XElement("BorderColor", s.BorderColor),
                        new XElement("BorderWidth", s.BorderWidth),
                        new XElement("BorderStyle", s.BorderStyle),
                        new XElement("FontFamily", s.FontFamily),
                        new XElement("FontSize", s.FontSize),
                        new XElement("FontColor", s.FontColor),
                        new XElement("FontBold", s.FontBold),
                        new XElement("FontItalic", s.FontItalic),
                        new XElement("AnimationEnabled", s.AnimationEnabled),
                        new XElement("AnimationType", s.AnimationType ?? ""),
                        new XElement("AnimationDuration", s.AnimationDuration)
                    ))
                ),
                theme.MoneyTree != null ? new XElement("MoneyTree",
                    new XElement("InactiveColor", theme.MoneyTree.InactiveColor),
                    new XElement("ActiveColor", theme.MoneyTree.ActiveColor),
                    new XElement("CompletedColor", theme.MoneyTree.CompletedColor),
                    new XElement("SafeHavenColor", theme.MoneyTree.SafeHavenColor),
                    new XElement("HighlightEnabled", theme.MoneyTree.HighlightEnabled),
                    new XElement("HighlightType", theme.MoneyTree.HighlightType),
                    new XElement("HighlightColor", theme.MoneyTree.HighlightColor),
                    new XElement("HighlightIntensity", theme.MoneyTree.HighlightIntensity),
                    new XElement("FontFamily", theme.MoneyTree.FontFamily),
                    new XElement("FontSize", theme.MoneyTree.FontSize),
                    new XElement("FontBold", theme.MoneyTree.FontBold)
                ) : null
            )
        );

        return doc.ToString();
    }

    private string CreateExampleThemeXml()
    {
        var doc = new XDocument(
            new XElement("ThemePack",
                new XElement("PackName", "My Custom Theme"),
                new XElement("Author", "Your Name"),
                new XElement("Version", "1.0.0"),
                new XElement("Description", "A custom theme for the Millionaire game"),
                new XElement("Straps",
                    new XElement("Strap",
                        new XAttribute("Type", "Question"),
                        new XElement("SvgShape", "Classic"),
                        new XElement("PrimaryColor", "#8B4513"),
                        new XElement("SecondaryColor", "#D4AF37"),
                        new XElement("GradientEnabled", "true"),
                        new XElement("GradientAngle", "90"),
                        new XElement("EffectType", "Outline"),
                        new XElement("EffectIntensity", "40"),
                        new XElement("EffectColor", "#FFFFFF"),
                        new XElement("BorderEnabled", "true"),
                        new XElement("BorderColor", "#000000"),
                        new XElement("BorderWidth", "4"),
                        new XElement("BorderStyle", "Solid"),
                        new XElement("FontFamily", "Copperplate Gothic"),
                        new XElement("FontSize", "24"),
                        new XElement("FontColor", "#FFFFFF"),
                        new XElement("FontBold", "true"),
                        new XElement("FontItalic", "false"),
                        new XElement("AnimationEnabled", "false"),
                        new XElement("AnimationType", ""),
                        new XElement("AnimationDuration", "500")
                    ),
                    new XElement("Strap",
                        new XAttribute("Type", "Answer"),
                        new XElement("SvgShape", "Classic"),
                        new XElement("PrimaryColor", "#8B4513"),
                        new XElement("SecondaryColor", "#D4AF37"),
                        new XElement("GradientEnabled", "true"),
                        new XElement("GradientAngle", "90"),
                        new XElement("EffectType", "Outline"),
                        new XElement("EffectIntensity", "40"),
                        new XElement("EffectColor", "#FFFFFF"),
                        new XElement("BorderEnabled", "true"),
                        new XElement("BorderColor", "#000000"),
                        new XElement("BorderWidth", "4"),
                        new XElement("BorderStyle", "Solid"),
                        new XElement("FontFamily", "Arial"),
                        new XElement("FontSize", "22"),
                        new XElement("FontColor", "#FFFFFF"),
                        new XElement("FontBold", "true"),
                        new XElement("FontItalic", "false"),
                        new XElement("AnimationEnabled", "false"),
                        new XElement("AnimationType", ""),
                        new XElement("AnimationDuration", "500")
                    )
                ),
                new XElement("MoneyTree",
                    new XElement("InactiveColor", "#808080"),
                    new XElement("ActiveColor", "#FFD700"),
                    new XElement("CompletedColor", "#00FF00"),
                    new XElement("SafeHavenColor", "#0080FF"),
                    new XElement("HighlightEnabled", "true"),
                    new XElement("HighlightType", "PulsingGlow"),
                    new XElement("HighlightColor", "#FFFF00"),
                    new XElement("HighlightIntensity", "80"),
                    new XElement("FontFamily", "Arial Bold"),
                    new XElement("FontSize", "18"),
                    new XElement("FontBold", "true")
                )
            )
        );

        return doc.ToString();
    }

    private string CreateInstructionsText()
    {
        return @"THEME PACK INSTRUCTIONS
=======================

1. Edit themepack.xml and change <PackName> to your custom theme name
   - Do NOT use reserved names: 'Default', 'Classic Gold', 'Classic Black', or anything starting with 'Preset:'
   
2. Customize the theme settings in themepack.xml:
   - Strap colors (PrimaryColor, SecondaryColor)
   - Strap shapes (Classic, Modern, Rounded, Sharp)
   - Strap effects (None, Glow, Shadow, 3D, Outline, Emboss)
   - Font settings (FontFamily, FontSize, FontColor, FontBold, FontItalic)
   - Money tree colors and highlight effects
   
3. Available strap types:
   - Question: The main question text strap
   - Answer: Answer option straps (A, B, C, D)
   - AnswerLabel: The letter labels on answer straps (optional)

4. Color format:
   - Use HEX colors: #RRGGBB (e.g., #FFD700 for gold)
   - Examples: #FFFFFF (white), #000000 (black), #FF0000 (red)

5. SvgShape options:
   - Classic: Traditional ribbon/strap shape
   - Modern: Clean, rectangular shape
   - Rounded: Rounded corners
   - Sharp: Angular, sharp corners

6. EffectType options:
   - None: No effect
   - Glow: Outer glow effect
   - Shadow: Drop shadow effect
   - 3D: Three-dimensional depth effect
   - Outline: Text outline effect (recommended with EffectColor=#FFFFFF)
   - Emboss: Raised embossed effect

7. Zip this folder (include themepack.xml and INSTRUCTIONS.txt)

8. Import the zip file using the Import button in Settings > Themes

EXAMPLE VALUES
==============
Primary Colors: #8B4513 (brown), #0047AB (blue), #8B0000 (red), #006400 (green)
Secondary Colors: #D4AF37 (gold), #87CEEB (light blue), #FFD700 (gold), #90EE90 (light green)
Font Families: 'Copperplate Gothic', 'Arial', 'Times New Roman', 'Georgia', 'Segoe UI'
Effect Examples: 
  - Outline at 40% intensity with white color (#FFFFFF) gives crisp text
  - Shadow at 50% intensity creates depth
  - Glow at 60% intensity creates soft halo
";
    }
}
