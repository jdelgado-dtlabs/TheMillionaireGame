using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using MillionaireGame.Core.Models;

namespace MillionaireGame.Core.Graphics;

/// <summary>
/// Renders strap overlays (question/answer displays) with SVG-style shapes, effects, and text
/// </summary>
public class SvgStrapRenderer : IDisposable
{
    private bool _disposed;

    /// <summary>
    /// Render a strap to a bitmap
    /// </summary>
    /// <param name="strap">Strap configuration</param>
    /// <param name="text">Text to display on strap</param>
    /// <param name="width">Width of output image</param>
    /// <param name="height">Height of output image</param>
    /// <param name="animationProgress">Animation progress (0.0 to 1.0)</param>
    /// <returns>Rendered bitmap</returns>
    public Bitmap RenderStrap(
        ThemeStrap strap,
        string text,
        int width,
        int height,
        float animationProgress = 1.0f)
    {
        var bitmap = new Bitmap(width, height);
        
        using (var graphics = System.Drawing.Graphics.FromImage(bitmap))
        {
            // Enable anti-aliasing for smooth rendering
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
            graphics.CompositingQuality = CompositingQuality.HighQuality;

            // Clear with transparency
            graphics.Clear(Color.Transparent);

            var bounds = new Rectangle(0, 0, width, height);
            RenderStrapToGraphics(graphics, strap, text, bounds, animationProgress);
        }

        return bitmap;
    }

    /// <summary>
    /// Render a strap directly to a Graphics context
    /// </summary>
    public void RenderStrapToGraphics(
        System.Drawing.Graphics graphics,
        ThemeStrap strap,
        string text,
        Rectangle bounds,
        float animationProgress = 1.0f)
    {
        // Save graphics state
        var state = graphics.Save();

        try
        {
            // Get the base shape
            using (var shapePath = StrapShapes.GetShape(strap.SvgShape, bounds))
            {
                // Apply animation transform if enabled
                GraphicsPath renderPath = shapePath;
                if (strap.AnimationEnabled && !string.IsNullOrEmpty(strap.AnimationType))
                {
                    renderPath = StrapEffects.ApplyAnimationTransform(
                        shapePath,
                        strap.AnimationType,
                        animationProgress,
                        bounds);
                }

                // Apply effects (shadow, glow, etc.) - render behind shape
                if (!string.IsNullOrEmpty(strap.EffectType) && strap.EffectType.ToLower() != "none")
                {
                    var effectColor = string.IsNullOrEmpty(strap.EffectColor) 
                        ? ParseColor(strap.PrimaryColor) 
                        : ParseColor(strap.EffectColor);
                    StrapEffects.ApplyEffect(
                        graphics,
                        renderPath,
                        strap.EffectType,
                        effectColor,
                        strap.EffectIntensity);
                }

                // Fill the shape with gradient or solid color
                using (var fillBrush = CreateFillBrush(strap, bounds))
                {
                    // Apply animation alpha
                    if (strap.AnimationEnabled && !string.IsNullOrEmpty(strap.AnimationType))
                    {
                        float alpha = StrapEffects.GetAnimationAlpha(strap.AnimationType, animationProgress);
                        if (fillBrush is SolidBrush solidBrush)
                        {
                            var color = solidBrush.Color;
                            var alphaBrush = new SolidBrush(Color.FromArgb(
                                (int)(color.A * alpha),
                                color.R,
                                color.G,
                                color.B));
                            graphics.FillPath(alphaBrush, renderPath);
                            alphaBrush.Dispose();
                        }
                        else
                        {
                            graphics.FillPath(fillBrush, renderPath);
                        }
                    }
                    else
                    {
                        graphics.FillPath(fillBrush, renderPath);
                    }
                }

                // Draw border if enabled
                if (strap.BorderEnabled)
                {
                    using (var borderPen = CreateBorderPen(strap))
                    {
                        graphics.DrawPath(borderPen, renderPath);
                    }
                }

                // Render text
                if (!string.IsNullOrEmpty(text))
                {
                    RenderText(graphics, strap, text, bounds, animationProgress);
                }

                // Clean up animated path if created
                if (renderPath != shapePath)
                {
                    renderPath.Dispose();
                }
            }
        }
        finally
        {
            // Restore graphics state
            graphics.Restore(state);
        }
    }

    /// <summary>
    /// Create fill brush (gradient or solid)
    /// </summary>
    private Brush CreateFillBrush(ThemeStrap strap, Rectangle bounds)
    {
        var primaryColor = ParseColor(strap.PrimaryColor);
        Color? secondaryColor = null;
        if (strap.SecondaryColor != null)
        {
            secondaryColor = ParseColor(strap.SecondaryColor);
        }

        return StrapEffects.CreateGradientBrush(
            bounds,
            primaryColor,
            secondaryColor,
            strap.GradientEnabled,
            strap.GradientAngle);
    }

    /// <summary>
    /// Create border pen
    /// </summary>
    private Pen CreateBorderPen(ThemeStrap strap)
    {
        var borderColor = ParseColor(strap.BorderColor);
        var pen = new Pen(borderColor, strap.BorderWidth)
        {
            LineJoin = LineJoin.Round
        };

        // Apply border style
        switch (strap.BorderStyle.ToLower())
        {
            case "dashed":
                pen.DashStyle = DashStyle.Dash;
                break;
            case "dotted":
                pen.DashStyle = DashStyle.Dot;
                break;
            case "dashdot":
                pen.DashStyle = DashStyle.DashDot;
                break;
            default:
                pen.DashStyle = DashStyle.Solid;
                break;
        }

        return pen;
    }

    /// <summary>
    /// Render text on the strap
    /// </summary>
    private void RenderText(
        System.Drawing.Graphics graphics,
        ThemeStrap strap,
        string text,
        Rectangle bounds,
        float animationProgress)
    {
        // Create font
        var fontStyle = FontStyle.Regular;
        if (strap.FontBold) fontStyle |= FontStyle.Bold;
        if (strap.FontItalic) fontStyle |= FontStyle.Italic;

        using (var font = new Font(strap.FontFamily, strap.FontSize, fontStyle))
        {
            var fontColor = ParseColor(strap.FontColor);
            
            // Apply animation alpha to text
            if (strap.AnimationEnabled && !string.IsNullOrEmpty(strap.AnimationType))
            {
                float alpha = StrapEffects.GetAnimationAlpha(strap.AnimationType, animationProgress);
                fontColor = Color.FromArgb(
                    (int)(fontColor.A * alpha),
                    fontColor.R,
                    fontColor.G,
                    fontColor.B);
            }

            using (var brush = new SolidBrush(fontColor))
            {
                // Calculate text position (centered)
                var format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter
                };

                // Add padding to bounds
                var textBounds = new Rectangle(
                    bounds.X + 20,
                    bounds.Y + 10,
                    bounds.Width - 40,
                    bounds.Height - 20);

                graphics.DrawString(text, font, brush, textBounds, format);
            }
        }
    }

    /// <summary>
    /// Render a preview of multiple straps stacked
    /// </summary>
    public Bitmap RenderStrapPreview(
        ThemeStrap questionStrap,
        ThemeStrap answerStrap,
        int width,
        int height)
    {
        var bitmap = new Bitmap(width, height);
        
        using (var graphics = System.Drawing.Graphics.FromImage(bitmap))
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.Clear(Color.Black); // Background

            int strapHeight = height / 3;
            int spacing = 20;

            // Question strap
            var questionBounds = new Rectangle(50, spacing, width - 100, strapHeight);
            RenderStrapToGraphics(graphics, questionStrap, "Sample Question Text", questionBounds);

            // Answer straps (4 answers)
            int answerWidth = (width - 120) / 2;
            int answerHeight = strapHeight / 2;
            int startY = strapHeight + spacing * 2;

            string[] answers = { "A: Answer 1", "B: Answer 2", "C: Answer 3", "D: Answer 4" };
            for (int i = 0; i < 4; i++)
            {
                int row = i / 2;
                int col = i % 2;
                var answerBounds = new Rectangle(
                    50 + col * (answerWidth + 20),
                    startY + row * (answerHeight + 10),
                    answerWidth,
                    answerHeight);
                
                RenderStrapToGraphics(graphics, answerStrap, answers[i], answerBounds);
            }
        }

        return bitmap;
    }

    /// <summary>
    /// Parse hex color string to Color
    /// </summary>
    private Color ParseColor(string? hexColor)
    {
        if (string.IsNullOrEmpty(hexColor))
            return Color.Gray;

        try
        {
            // Remove # if present
            hexColor = hexColor.TrimStart('#');

            // Handle 3-digit hex (e.g., "F00" -> "FF0000")
            if (hexColor.Length == 3)
            {
                hexColor = string.Concat(
                    hexColor[0], hexColor[0],
                    hexColor[1], hexColor[1],
                    hexColor[2], hexColor[2]);
            }

            // Parse RGB
            if (hexColor.Length == 6)
            {
                int r = Convert.ToInt32(hexColor.Substring(0, 2), 16);
                int g = Convert.ToInt32(hexColor.Substring(2, 2), 16);
                int b = Convert.ToInt32(hexColor.Substring(4, 2), 16);
                return Color.FromArgb(255, r, g, b);
            }

            // Parse ARGB
            if (hexColor.Length == 8)
            {
                int a = Convert.ToInt32(hexColor.Substring(0, 2), 16);
                int r = Convert.ToInt32(hexColor.Substring(2, 2), 16);
                int g = Convert.ToInt32(hexColor.Substring(4, 2), 16);
                int b = Convert.ToInt32(hexColor.Substring(6, 2), 16);
                return Color.FromArgb(a, r, g, b);
            }

            return Color.Gray;
        }
        catch
        {
            return Color.Gray;
        }
    }

    /// <summary>
    /// Create a thumbnail preview of a strap
    /// </summary>
    public Bitmap CreateThumbnail(ThemeStrap strap, int width, int height)
    {
        return RenderStrap(strap, strap.StrapType, width, height);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            // Clean up any resources if needed
            _disposed = true;
        }
    }
}
