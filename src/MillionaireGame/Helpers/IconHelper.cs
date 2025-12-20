using System.Reflection;

namespace MillionaireGame.Helpers;

/// <summary>
/// Helper class for loading and applying the application icon to forms
/// </summary>
public static class IconHelper
{
    private static Icon? _cachedIcon;

    /// <summary>
    /// Loads the application icon from embedded resources
    /// </summary>
    public static Icon? LoadApplicationIcon()
    {
        if (_cachedIcon != null)
            return _cachedIcon;

        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "MillionaireGame.lib.image.logo.ico";
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                _cachedIcon = new Icon(stream);
                return _cachedIcon;
            }
        }
        catch (Exception ex)
        {
            if (Program.DebugMode)
            {
                Console.WriteLine($"[IconHelper] Failed to load icon: {ex.Message}");
            }
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
