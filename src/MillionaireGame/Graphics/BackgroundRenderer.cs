using MillionaireGame.Core.Settings;
using MillionaireGame.Core.Services;
using MillionaireGame.Utilities;
using System.Drawing.Drawing2D;

namespace MillionaireGame.Graphics;

/// <summary>
/// Handles background rendering for TV screen based on broadcast settings
/// Supports both prerendered theme backgrounds and solid chroma key colors
/// Integrates with theming system for dynamic background loading
/// </summary>
public class BackgroundRenderer
{
    private readonly ApplicationSettings _settings;
    private readonly ThemeService? _themeService;
    private Image? _cachedBackground;
    private string? _cachedBackgroundPath;

    public BackgroundRenderer(ApplicationSettings settings, ThemeService? themeService = null)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _themeService = themeService;
    }

    /// <summary>
    /// Render the background based on current broadcast settings
    /// </summary>
    public void RenderBackground(System.Drawing.Graphics g, int width, int height)
    {
        if (_settings.Broadcast.Mode == BackgroundMode.ChromaKey)
        {
            // Solid color chroma key background
            g.Clear(_settings.Broadcast.ChromaKeyColor);
        }
        else
        {
            // Prerendered theme background
            RenderPrerenderedBackground(g, width, height);
        }
    }

    private void RenderPrerenderedBackground(System.Drawing.Graphics g, int width, int height)
    {
        // Try to get background from active theme first
        string? backgroundPath = null;
        
        if (_themeService != null && _themeService.CurrentTheme != null)
        {
            try
            {
                // Load complete theme asynchronously if not already loaded
                var completeTheme = Task.Run(async () => 
                    await _themeService.GetCompleteThemeAsync(_themeService.CurrentTheme.ThemeId)).Result;
                
                // Find TV screen background
                if (completeTheme != null)
                {
                    var tvBackground = completeTheme.Backgrounds.FirstOrDefault(b => b.ComponentType == "TVScreen");
                    if (tvBackground != null && !string.IsNullOrWhiteSpace(tvBackground.ImagePath))
                    {
                        backgroundPath = tvBackground.ImagePath;
                        GameConsole.Debug($"[BackgroundRenderer] Using theme background: {backgroundPath}");
                    }
                }
            }
            catch (Exception ex)
            {
                GameConsole.Error($"[BackgroundRenderer] Error loading theme background: {ex.Message}");
            }
        }
        
        // Fall back to legacy background path from broadcast settings
        if (string.IsNullOrWhiteSpace(backgroundPath))
        {
            backgroundPath = _settings.Broadcast.SelectedBackgroundPath;
            GameConsole.Debug($"[BackgroundRenderer] Using legacy background path: {backgroundPath}");
        }
        
        // If no background selected or empty path, fall back to black
        if (string.IsNullOrWhiteSpace(backgroundPath))
        {
            g.Clear(Color.Black);
            return;
        }

        try
        {
            // Load or retrieve cached background image
            var backgroundImage = GetCachedBackground(backgroundPath);
            
            if (backgroundImage != null)
            {
                // Set high quality scaling
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                
                // Draw scaled to fill entire screen
                g.DrawImage(backgroundImage, 0, 0, width, height);
            }
            else
            {
                // Background image not found - fall back to black
                g.Clear(Color.Black);
            }
        }
        catch
        {
            // Error loading background - fall back to black
            g.Clear(Color.Black);
        }
    }

    private Image? GetCachedBackground(string backgroundPath)
    {
        // If cached and path hasn't changed, return cached image
        if (_cachedBackground != null && _cachedBackgroundPath == backgroundPath)
        {
            return _cachedBackground;
        }

        // Clear old cache
        _cachedBackground?.Dispose();
        _cachedBackground = null;
        _cachedBackgroundPath = null;

        // Load new background
        try
        {
            // Check if it's an embedded resource
            if (backgroundPath.StartsWith("embedded://"))
            {
                var resourceName = backgroundPath.Substring("embedded://".Length);
                GameConsole.Debug($"[BackgroundRenderer] Loading embedded resource: {resourceName}");
                _cachedBackground = LoadEmbeddedResource(resourceName);
                if (_cachedBackground != null)
                {
                    _cachedBackgroundPath = backgroundPath;
                    GameConsole.Debug($"[BackgroundRenderer] Successfully loaded embedded resource: {resourceName}");
                }
                return _cachedBackground;
            }

            // Check if it's a custom placeholder (no actual file yet)
            if (backgroundPath.StartsWith("custom://"))
            {
                // Return null, will fall back to black
                return null;
            }

            // Regular file path
            var fullPath = Path.IsPathRooted(backgroundPath)
                ? backgroundPath
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, backgroundPath);

            if (File.Exists(fullPath))
            {
                _cachedBackground = Image.FromFile(fullPath);
                _cachedBackgroundPath = backgroundPath;
                return _cachedBackground;
            }
        }
        catch
        {
            // Silently ignore loading errors
        }

        return null;
    }

    private Image? LoadEmbeddedResource(string resourceName)
    {
        try
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var resourcePath = $"MillionaireGame.lib.textures.{resourceName}";
            
            GameConsole.Debug($"[BackgroundRenderer] Looking for resource: {resourcePath}");
            
            using var stream = assembly.GetManifestResourceStream(resourcePath);
            if (stream != null)
            {
                var image = Image.FromStream(stream);
                GameConsole.Debug($"[BackgroundRenderer] ✓ Successfully loaded embedded resource: {resourceName}");
                return image;
            }
            else
            {
                GameConsole.Warn($"[BackgroundRenderer] ✗ Resource stream is NULL for: {resourcePath}");
                
                // List all available resources for debugging
                var allResources = assembly.GetManifestResourceNames();
                GameConsole.Debug($"[BackgroundRenderer] Available embedded resources ({allResources.Length}):");
                foreach (var res in allResources.Where(r => r.Contains("bkg") || r.Contains("FFF")))
                {
                    GameConsole.Debug($"  - {res}");
                }
            }
        }
        catch (Exception ex)
        {
            GameConsole.Error($"[BackgroundRenderer] Error loading embedded resource '{resourceName}': {ex.Message}");
        }
        return null;
    }

    /// <summary>
    /// Clear cached background image (call when theme or background changes)
    /// </summary>
    public void ClearCache()
    {
        _cachedBackground?.Dispose();
        _cachedBackground = null;
        _cachedBackgroundPath = null;
    }

    /// <summary>
    /// Dispose cached resources
    /// </summary>
    public void Dispose()
    {
        ClearCache();
    }
}
