using System.Drawing;
using System.Drawing.Drawing2D;

namespace MillionaireGame.Core.Graphics;

/// <summary>
/// Provides visual effects for strap rendering (glow, shadow, 3D, outline, emboss)
/// </summary>
public static class StrapEffects
{
    /// <summary>
    /// Apply an effect to a strap path
    /// </summary>
    /// <param name="graphics">Graphics context</param>
    /// <param name="path">Shape path</param>
    /// <param name="effectType">Type of effect (Glow, Shadow, 3D, Outline, Emboss)</param>
    /// <param name="effectColor">Color for the effect</param>
    /// <param name="intensity">Effect intensity (0-100)</param>
    public static void ApplyEffect(
        System.Drawing.Graphics graphics,
        GraphicsPath path,
        string effectType,
        Color effectColor,
        int intensity)
    {
        if (string.IsNullOrEmpty(effectType))
            return;

        // Clamp intensity to valid range
        intensity = Math.Max(0, Math.Min(100, intensity));

        switch (effectType.ToLower())
        {
            case "glow":
                ApplyGlow(graphics, path, effectColor, intensity);
                break;
            case "shadow":
                ApplyShadow(graphics, path, effectColor, intensity);
                break;
            case "3d":
                Apply3D(graphics, path, effectColor, intensity);
                break;
            case "outline":
                ApplyOutline(graphics, path, effectColor, intensity);
                break;
            case "emboss":
                ApplyEmboss(graphics, path, effectColor, intensity);
                break;
        }
    }

    /// <summary>
    /// Apply glow effect (soft outer glow)
    /// </summary>
    private static void ApplyGlow(System.Drawing.Graphics graphics, GraphicsPath path, Color glowColor, int intensity)
    {
        // Glow is multiple outlines with decreasing opacity
        int glowRadius = (int)(intensity / 10.0f) + 1;
        float baseAlpha = (intensity / 100.0f) * 255;

        for (int i = glowRadius; i > 0; i--)
        {
            float alpha = (baseAlpha / glowRadius) * (glowRadius - i + 1);
            alpha = Math.Min(255, alpha);
            
            using (var pen = new Pen(Color.FromArgb((int)alpha, glowColor), i * 2))
            {
                pen.LineJoin = LineJoin.Round;
                graphics.DrawPath(pen, path);
            }
        }
    }

    /// <summary>
    /// Apply drop shadow effect
    /// </summary>
    private static void ApplyShadow(System.Drawing.Graphics graphics, GraphicsPath path, Color shadowColor, int intensity)
    {
        // Shadow offset based on intensity
        int offsetX = (int)(intensity / 20.0f) + 2;
        int offsetY = (int)(intensity / 20.0f) + 2;
        float alpha = (intensity / 100.0f) * 200; // Max 200 for shadow

        // Create shadow path (offset)
        using (var shadowPath = (GraphicsPath)path.Clone())
        {
            var matrix = new Matrix();
            matrix.Translate(offsetX, offsetY);
            shadowPath.Transform(matrix);

            // Draw shadow with blur simulation
            int blurRadius = (int)(intensity / 25.0f) + 1;
            for (int i = blurRadius; i > 0; i--)
            {
                float shadowAlpha = alpha / (blurRadius * 2) * i;
                using (var shadowBrush = new SolidBrush(Color.FromArgb((int)shadowAlpha, shadowColor)))
                {
                    graphics.FillPath(shadowBrush, shadowPath);
                }
            }
        }
    }

    /// <summary>
    /// Apply 3D effect (highlight and shadow)
    /// </summary>
    private static void Apply3D(System.Drawing.Graphics graphics, GraphicsPath path, Color effectColor, int intensity)
    {
        // 3D effect: light from top-left, shadow to bottom-right
        int depth = (int)(intensity / 20.0f) + 1;
        float alpha = (intensity / 100.0f) * 150;

        // Highlight (top-left)
        using (var highlightPath = (GraphicsPath)path.Clone())
        {
            var highlightMatrix = new Matrix();
            highlightMatrix.Translate(-depth, -depth);
            highlightPath.Transform(highlightMatrix);

            var highlightColor = Color.FromArgb((int)(alpha * 0.7), Color.White);
            using (var pen = new Pen(highlightColor, depth))
            {
                pen.LineJoin = LineJoin.Round;
                graphics.DrawPath(pen, highlightPath);
            }
        }

        // Shadow (bottom-right)
        using (var shadowPath = (GraphicsPath)path.Clone())
        {
            var shadowMatrix = new Matrix();
            shadowMatrix.Translate(depth, depth);
            shadowPath.Transform(shadowMatrix);

            var shadowColor = Color.FromArgb((int)alpha, effectColor);
            using (var pen = new Pen(shadowColor, depth))
            {
                pen.LineJoin = LineJoin.Round;
                graphics.DrawPath(pen, shadowPath);
            }
        }
    }

    /// <summary>
    /// Apply outline effect (stroke around shape)
    /// </summary>
    private static void ApplyOutline(System.Drawing.Graphics graphics, GraphicsPath path, Color outlineColor, int intensity)
    {
        // Outline width based on intensity
        int width = (int)(intensity / 10.0f) + 1;
        float alpha = (intensity / 100.0f) * 255;

        using (var pen = new Pen(Color.FromArgb((int)alpha, outlineColor), width))
        {
            pen.LineJoin = LineJoin.Round;
            graphics.DrawPath(pen, path);
        }
    }

    /// <summary>
    /// Apply emboss effect (raised 3D appearance)
    /// </summary>
    private static void ApplyEmboss(System.Drawing.Graphics graphics, GraphicsPath path, Color effectColor, int intensity)
    {
        // Emboss: highlight on top-left edge, shadow on bottom-right edge
        int offset = (int)(intensity / 30.0f) + 1;
        float alpha = (intensity / 100.0f) * 180;

        // Top-left highlight
        using (var highlightPath = (GraphicsPath)path.Clone())
        {
            var matrix = new Matrix();
            matrix.Translate(-offset, -offset);
            highlightPath.Transform(matrix);

            using (var pen = new Pen(Color.FromArgb((int)(alpha * 0.8), Color.White), 2))
            {
                pen.LineJoin = LineJoin.Round;
                graphics.DrawPath(pen, highlightPath);
            }
        }

        // Bottom-right shadow
        using (var shadowPath = (GraphicsPath)path.Clone())
        {
            var matrix = new Matrix();
            matrix.Translate(offset, offset);
            shadowPath.Transform(matrix);

            using (var pen = new Pen(Color.FromArgb((int)alpha, effectColor), 2))
            {
                pen.LineJoin = LineJoin.Round;
                graphics.DrawPath(pen, shadowPath);
            }
        }
    }

    /// <summary>
    /// Create a gradient brush based on configuration
    /// </summary>
    public static Brush CreateGradientBrush(
        Rectangle bounds,
        Color primaryColor,
        Color? secondaryColor,
        bool gradientEnabled,
        int gradientAngle)
    {
        if (!gradientEnabled || secondaryColor == null)
        {
            return new SolidBrush(primaryColor);
        }

        // Create linear gradient brush
        return new LinearGradientBrush(
            bounds,
            primaryColor,
            secondaryColor.Value,
            gradientAngle);
    }

    /// <summary>
    /// Get all available effect types
    /// </summary>
    public static string[] GetAvailableEffects()
    {
        return new[]
        {
            "None",
            "Glow",
            "Shadow",
            "3D",
            "Outline",
            "Emboss"
        };
    }

    /// <summary>
    /// Apply animation transform to a path (for preview purposes)
    /// </summary>
    public static GraphicsPath ApplyAnimationTransform(
        GraphicsPath path,
        string animationType,
        float progress,
        Rectangle bounds)
    {
        if (string.IsNullOrEmpty(animationType) || progress <= 0)
            return path;

        var animatedPath = (GraphicsPath)path.Clone();
        var matrix = new Matrix();

        // Progress should be 0.0 to 1.0
        progress = Math.Max(0, Math.Min(1, progress));

        switch (animationType.ToLower())
        {
            case "fade":
                // Fade doesn't affect path, only alpha (handled in rendering)
                break;

            case "slide":
                // Slide from right to left
                float slideOffset = bounds.Width * (1 - progress);
                matrix.Translate(slideOffset, 0);
                animatedPath.Transform(matrix);
                break;

            case "zoom":
                // Zoom from center
                float scale = progress;
                float centerX = bounds.Left + bounds.Width / 2f;
                float centerY = bounds.Top + bounds.Height / 2f;
                
                matrix.Translate(-centerX, -centerY);
                matrix.Scale(scale, scale);
                matrix.Translate(centerX, centerY);
                animatedPath.Transform(matrix);
                break;

            case "pulse":
                // Pulse effect (scale oscillation)
                float pulseScale = 1.0f + (float)Math.Sin(progress * Math.PI * 2) * 0.1f;
                float pulseCenterX = bounds.Left + bounds.Width / 2f;
                float pulseCenterY = bounds.Top + bounds.Height / 2f;
                
                matrix.Translate(-pulseCenterX, -pulseCenterY);
                matrix.Scale(pulseScale, pulseScale);
                matrix.Translate(pulseCenterX, pulseCenterY);
                animatedPath.Transform(matrix);
                break;
        }

        return animatedPath;
    }

    /// <summary>
    /// Get animation alpha multiplier based on animation type and progress
    /// </summary>
    public static float GetAnimationAlpha(string animationType, float progress)
    {
        if (string.IsNullOrEmpty(animationType))
            return 1.0f;

        // Progress should be 0.0 to 1.0
        progress = Math.Max(0, Math.Min(1, progress));

        switch (animationType.ToLower())
        {
            case "fade":
                return progress;

            case "pulse":
                // Pulse between 70% and 100% opacity
                return 0.7f + (float)Math.Sin(progress * Math.PI * 2) * 0.15f;

            default:
                return 1.0f;
        }
    }

    /// <summary>
    /// Get all available animation types
    /// </summary>
    public static string[] GetAvailableAnimations()
    {
        return new[]
        {
            "None",
            "Fade",
            "Slide",
            "Zoom",
            "Pulse"
        };
    }
}
