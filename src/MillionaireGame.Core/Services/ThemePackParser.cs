using System.Xml.Linq;
using MillionaireGame.Core.Models;

namespace MillionaireGame.Core.Services;

/// <summary>
/// Parses theme pack XML configuration files
/// </summary>
public class ThemePackParser
{
    /// <summary>
    /// Parse a theme pack XML file
    /// </summary>
    public ThemePackData ParsePackXml(string xmlPath)
    {
        if (!File.Exists(xmlPath))
            throw new FileNotFoundException($"Theme pack XML not found: {xmlPath}");

        var doc = XDocument.Load(xmlPath);
        var root = doc.Element("ThemePack");
        if (root == null)
            throw new InvalidOperationException("Invalid theme pack XML: Missing ThemePack root element");

        var packData = new ThemePackData();

        // Parse metadata
        var metadata = root.Element("Metadata");
        if (metadata != null)
        {
            packData.PackName = metadata.Element("PackName")?.Value ?? throw new InvalidOperationException("Pack name is required");
            packData.PackVersion = metadata.Element("Version")?.Value ?? "1.0.0";
            packData.Author = metadata.Element("Author")?.Value;
            packData.Description = metadata.Element("Description")?.Value;
        }

        // Parse themes
        var themesElement = root.Element("Themes");
        if (themesElement != null)
        {
            foreach (var themeElement in themesElement.Elements("Theme"))
            {
                var theme = ParseThemeElement(themeElement);
                packData.Themes.Add(theme);
            }
        }

        return packData;
    }

    /// <summary>
    /// Create theme pack XML from data
    /// </summary>
    public XDocument CreatePackXml(ThemePackData packData)
    {
        var doc = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement("ThemePack",
                new XElement("Metadata",
                    new XElement("PackName", packData.PackName),
                    new XElement("Version", packData.PackVersion),
                    new XElement("Author", packData.Author ?? string.Empty),
                    new XElement("Description", packData.Description ?? string.Empty)
                ),
                new XElement("Themes",
                    packData.Themes.Select(t => CreateThemeElement(t))
                )
            )
        );

        return doc;
    }

    private CompleteTheme ParseThemeElement(XElement themeElement)
    {
        var theme = new Theme
        {
            ThemeName = themeElement.Element("Name")?.Value ?? "Unnamed",
            ThemeType = "Custom", // Imported themes are always custom
            Description = themeElement.Element("Description")?.Value,
            Author = themeElement.Element("Author")?.Value,
            Version = themeElement.Element("Version")?.Value ?? "1.0.0"
        };

        var backgrounds = new List<ThemeBackground>();
        var backgroundsElement = themeElement.Element("Backgrounds");
        if (backgroundsElement != null)
        {
            foreach (var bgElement in backgroundsElement.Elements("Background"))
            {
                backgrounds.Add(ParseBackgroundElement(bgElement));
            }
        }

        var straps = new List<ThemeStrap>();
        var strapsElement = themeElement.Element("Straps");
        if (strapsElement != null)
        {
            foreach (var strapElement in strapsElement.Elements("Strap"))
            {
                straps.Add(ParseStrapElement(strapElement));
            }
        }

        ThemeMoneyTree? moneyTree = null;
        var moneyTreeElement = themeElement.Element("MoneyTree");
        if (moneyTreeElement != null)
        {
            moneyTree = ParseMoneyTreeElement(moneyTreeElement);
        }

        return new CompleteTheme
        {
            Theme = theme,
            Backgrounds = backgrounds,
            Straps = straps,
            MoneyTree = moneyTree
        };
    }

    private ThemeBackground ParseBackgroundElement(XElement element)
    {
        return new ThemeBackground
        {
            ComponentType = element.Element("ComponentType")?.Value ?? "TVScreen",
            ImagePath = element.Element("ImagePath")?.Value,
            ChromaKeyEnabled = bool.Parse(element.Element("ChromaKeyEnabled")?.Value ?? "false"),
            ChromaKeyColor = element.Element("ChromaKeyColor")?.Value,
            ChromaKeyTolerance = int.Parse(element.Element("ChromaKeyTolerance")?.Value ?? "50"),
            ScaleMode = element.Element("ScaleMode")?.Value ?? "Fill",
            PositionX = int.Parse(element.Element("PositionX")?.Value ?? "0"),
            PositionY = int.Parse(element.Element("PositionY")?.Value ?? "0"),
            Transparency = int.Parse(element.Element("Transparency")?.Value ?? "100")
        };
    }

    private ThemeStrap ParseStrapElement(XElement element)
    {
        return new ThemeStrap
        {
            StrapType = element.Element("StrapType")?.Value ?? "Question",
            SvgShape = element.Element("SvgShape")?.Value ?? "Classic",
            PrimaryColor = element.Element("PrimaryColor")?.Value ?? "#8B4513",
            SecondaryColor = element.Element("SecondaryColor")?.Value,
            GradientEnabled = bool.Parse(element.Element("GradientEnabled")?.Value ?? "false"),
            GradientAngle = int.Parse(element.Element("GradientAngle")?.Value ?? "90"),
            EffectType = element.Element("EffectType")?.Value,
            EffectIntensity = int.Parse(element.Element("EffectIntensity")?.Value ?? "50"),
            EffectColor = element.Element("EffectColor")?.Value,
            BorderEnabled = bool.Parse(element.Element("BorderEnabled")?.Value ?? "true"),
            BorderColor = element.Element("BorderColor")?.Value ?? "#000000",
            BorderWidth = int.Parse(element.Element("BorderWidth")?.Value ?? "2"),
            BorderStyle = element.Element("BorderStyle")?.Value ?? "Solid",
            FontFamily = element.Element("FontFamily")?.Value ?? "Arial",
            FontSize = int.Parse(element.Element("FontSize")?.Value ?? "24"),
            FontColor = element.Element("FontColor")?.Value ?? "#FFFFFF",
            FontBold = bool.Parse(element.Element("FontBold")?.Value ?? "false"),
            FontItalic = bool.Parse(element.Element("FontItalic")?.Value ?? "false"),
            AnimationEnabled = bool.Parse(element.Element("AnimationEnabled")?.Value ?? "false"),
            AnimationType = element.Element("AnimationType")?.Value,
            AnimationDuration = int.Parse(element.Element("AnimationDuration")?.Value ?? "500")
        };
    }

    private ThemeMoneyTree ParseMoneyTreeElement(XElement element)
    {
        return new ThemeMoneyTree
        {
            BackgroundImagePath = element.Element("BackgroundImagePath")?.Value,
            InactiveColor = element.Element("InactiveColor")?.Value ?? "#808080",
            ActiveColor = element.Element("ActiveColor")?.Value ?? "#FFD700",
            CompletedColor = element.Element("CompletedColor")?.Value ?? "#00FF00",
            SafeHavenColor = element.Element("SafeHavenColor")?.Value ?? "#0080FF",
            HighlightEnabled = bool.Parse(element.Element("HighlightEnabled")?.Value ?? "true"),
            HighlightType = element.Element("HighlightType")?.Value ?? "PulsingGlow",
            HighlightColor = element.Element("HighlightColor")?.Value ?? "#FFFF00",
            HighlightIntensity = int.Parse(element.Element("HighlightIntensity")?.Value ?? "80"),
            FontFamily = element.Element("FontFamily")?.Value ?? "Arial Bold",
            FontSize = int.Parse(element.Element("FontSize")?.Value ?? "18"),
            FontBold = bool.Parse(element.Element("FontBold")?.Value ?? "true")
        };
    }

    private XElement CreateThemeElement(CompleteTheme theme)
    {
        return new XElement("Theme",
            new XElement("Name", theme.Theme.ThemeName),
            new XElement("Description", theme.Theme.Description ?? string.Empty),
            new XElement("Author", theme.Theme.Author ?? string.Empty),
            new XElement("Version", theme.Theme.Version ?? "1.0.0"),
            new XElement("Backgrounds",
                theme.Backgrounds.Select(bg => new XElement("Background",
                    new XElement("ComponentType", bg.ComponentType),
                    new XElement("ImagePath", bg.ImagePath ?? string.Empty),
                    new XElement("ChromaKeyEnabled", bg.ChromaKeyEnabled),
                    new XElement("ChromaKeyColor", bg.ChromaKeyColor ?? string.Empty),
                    new XElement("ChromaKeyTolerance", bg.ChromaKeyTolerance),
                    new XElement("ScaleMode", bg.ScaleMode),
                    new XElement("PositionX", bg.PositionX),
                    new XElement("PositionY", bg.PositionY),
                    new XElement("Transparency", bg.Transparency)
                ))
            ),
            new XElement("Straps",
                theme.Straps.Select(strap => new XElement("Strap",
                    new XElement("StrapType", strap.StrapType),
                    new XElement("SvgShape", strap.SvgShape),
                    new XElement("PrimaryColor", strap.PrimaryColor),
                    new XElement("SecondaryColor", strap.SecondaryColor ?? string.Empty),
                    new XElement("GradientEnabled", strap.GradientEnabled),
                    new XElement("GradientAngle", strap.GradientAngle),
                    new XElement("EffectType", strap.EffectType ?? string.Empty),
                    new XElement("EffectIntensity", strap.EffectIntensity),
                    new XElement("EffectColor", strap.EffectColor ?? string.Empty),
                    new XElement("BorderEnabled", strap.BorderEnabled),
                    new XElement("BorderColor", strap.BorderColor),
                    new XElement("BorderWidth", strap.BorderWidth),
                    new XElement("BorderStyle", strap.BorderStyle),
                    new XElement("FontFamily", strap.FontFamily),
                    new XElement("FontSize", strap.FontSize),
                    new XElement("FontColor", strap.FontColor),
                    new XElement("FontBold", strap.FontBold),
                    new XElement("FontItalic", strap.FontItalic),
                    new XElement("AnimationEnabled", strap.AnimationEnabled),
                    new XElement("AnimationType", strap.AnimationType ?? string.Empty),
                    new XElement("AnimationDuration", strap.AnimationDuration)
                ))
            ),
            theme.MoneyTree != null ? new XElement("MoneyTree",
                new XElement("BackgroundImagePath", theme.MoneyTree.BackgroundImagePath ?? string.Empty),
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
        );
    }
}

/// <summary>
/// Theme pack data structure
/// </summary>
public class ThemePackData
{
    public string PackName { get; set; } = string.Empty;
    public string PackVersion { get; set; } = "1.0.0";
    public string? Author { get; set; }
    public string? Description { get; set; }
    public List<CompleteTheme> Themes { get; set; } = new();
}
