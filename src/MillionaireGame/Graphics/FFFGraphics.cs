using System.Drawing;
using System.Reflection;

namespace MillionaireGame.Graphics;

/// <summary>
/// Manages FFF contestant strap graphics for random selection display
/// </summary>
public static class FFFGraphics
{
    private static Image? _idleStrap;
    private static Image? _fastestStrap;
    private static Image? _background;
    
    /// <summary>
    /// Load FFF background image
    /// </summary>
    public static Image? GetBackground()
    {
        if (_background == null)
        {
            _background = LoadEmbeddedImage("02_FFF.png");
        }
        return _background;
    }
    
    /// <summary>
    /// Load contestant strap for normal/idle state (white text background)
    /// </summary>
    public static Image? GetIdleStrap()
    {
        if (_idleStrap == null)
        {
            _idleStrap = LoadEmbeddedImage("fff_idle_new.png");
        }
        return _idleStrap;
    }
    
    /// <summary>
    /// Load contestant strap for highlighted/fastest state (black text background)
    /// </summary>
    public static Image? GetFastestStrap()
    {
        if (_fastestStrap == null)
        {
            _fastestStrap = LoadEmbeddedImage("fff_fastest_new.png");
        }
        return _fastestStrap;
    }
    
    /// <summary>
    /// Load image from embedded resources
    /// </summary>
    private static Image? LoadEmbeddedImage(string filename)
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"MillionaireGame.lib.textures.{filename}";
            
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                return Image.FromStream(stream);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load embedded image {filename}: {ex.Message}");
        }
        
        return null;
    }
    
    /// <summary>
    /// Clear cached images (call on shutdown)
    /// </summary>
    public static void ClearCache()
    {
        _idleStrap?.Dispose();
        _fastestStrap?.Dispose();
        
        _idleStrap = null;
        _fastestStrap = null;
    }
}
