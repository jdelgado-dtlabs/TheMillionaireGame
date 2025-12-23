using MillionaireGame.Core.Models;
using System.Reflection;

namespace MillionaireGame.Core.Graphics;

/// <summary>
/// State of a lifeline icon display
/// </summary>
public enum LifelineIconState
{
    /// <summary>Icon is not shown</summary>
    Hidden,
    /// <summary>Lifeline is available (black/normal)</summary>
    Normal,
    /// <summary>Lifeline is being pinged or activated (yellow/glint)</summary>
    Bling,
    /// <summary>Lifeline has been used (red X)</summary>
    Used
}

/// <summary>
/// Helper class for loading and managing lifeline icons from embedded resources
/// </summary>
public static class LifelineIcons
{
    private static readonly Dictionary<string, Image?> _iconCache = new();

    /// <summary>
    /// Gets the appropriate icon for a lifeline based on its type and state
    /// </summary>
    public static Image? GetLifelineIcon(LifelineType type, LifelineIconState state)
    {
        if (state == LifelineIconState.Hidden)
            return null;

        var baseName = GetIconBaseName(type);
        var suffix = GetStateSuffix(state);
        var fileName = $"{baseName}{suffix}.png";
        
        return LoadIcon(fileName);
    }

    /// <summary>
    /// Gets the base filename for a lifeline type
    /// </summary>
    private static string GetIconBaseName(LifelineType type)
    {
        return type switch
        {
            LifelineType.AskTheAudience => "ll_audience",
            LifelineType.FiftyFifty => "ll_5050",
            LifelineType.PlusOne => "ll_phone",  // Plus One is actually Phone a Friend
            LifelineType.SwitchQuestion => "ll_switch",
            LifelineType.AskTheHost => "ll_host",
            LifelineType.DoubleDip => "ll_double",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    /// <summary>
    /// Gets the suffix for an icon state
    /// </summary>
    private static string GetStateSuffix(LifelineIconState state)
    {
        return state switch
        {
            LifelineIconState.Normal => "",
            LifelineIconState.Bling => "_glint",
            LifelineIconState.Used => "_used",
            _ => ""
        };
    }

    /// <summary>
    /// Loads an icon from embedded resources in the main MillionaireGame assembly
    /// </summary>
    private static Image? LoadIcon(string fileName)
    {
        if (_iconCache.TryGetValue(fileName, out var cached))
            return cached;

        try
        {
            // Load from the main MillionaireGame assembly
            var mainAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "MillionaireGame");
            
            if (mainAssembly == null)
            {
                return null;
            }

            var resourceName = $"MillionaireGame.lib.textures.{fileName}";
            
            using var stream = mainAssembly.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                var image = Image.FromStream(stream);
                _iconCache[fileName] = image;
                return image;
            }
        }
        catch
        {
        }

        _iconCache[fileName] = null;
        return null;
    }

    /// <summary>
    /// Clears the icon cache
    /// </summary>
    public static void ClearCache()
    {
        foreach (var image in _iconCache.Values)
        {
            image?.Dispose();
        }
        _iconCache.Clear();
    }
}
