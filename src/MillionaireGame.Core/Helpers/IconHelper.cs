using System.Reflection;

namespace MillionaireGame.Core.Helpers;

/// <summary>
/// Helper class for loading and applying the application icon to forms
/// </summary>
public static class IconHelper
{
    private static Icon? _cachedIcon;

    /// <summary>
    /// Loads the application icon from embedded resources in the main MillionaireGame assembly
    /// </summary>
    public static Icon? LoadApplicationIcon()
    {
        if (_cachedIcon != null)
            return _cachedIcon;

        try
        {
            // Load from the main MillionaireGame assembly, not the Core assembly
            var mainAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "MillionaireGame");
            
            if (mainAssembly == null)
                return null;

            var resourceName = "MillionaireGame.lib.image.logo.ico";
            using var stream = mainAssembly.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                _cachedIcon = new Icon(stream);
                return _cachedIcon;
            }
        }
        catch (Exception)
        {
            // Silently fail if icon cannot be loaded
        }

        return null;
    }

    /// <summary>
    /// Applies the application icon to a form
    /// </summary>
    public static void ApplyToForm(Form form)
    {
        var icon = LoadApplicationIcon();
        if (icon != null)
        {
            form.Icon = icon;
        }
    }
}
