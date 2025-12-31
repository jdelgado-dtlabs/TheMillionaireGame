using MillionaireGame.Core.Settings;
using System.Drawing.Drawing2D;

namespace MillionaireGame.Graphics;

/// <summary>
/// Handles background rendering for TV screen based on broadcast settings
/// Supports both prerendered theme backgrounds and solid chroma key colors
/// </summary>
public class BackgroundRenderer
{
    private readonly ApplicationSettings _settings;
    private Image? _cachedBackground;
    private string? _cachedBackgroundPath;

    public BackgroundRenderer(ApplicationSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
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
        var backgroundPath = _settings.Broadcast.SelectedBackgroundPath;
        
        // If no background selected or path is invalid, fall back to black
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
            // TODO Phase 4: Resolve relative path through ThemeManager
            // For now, treat as absolute path or relative to application directory
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
