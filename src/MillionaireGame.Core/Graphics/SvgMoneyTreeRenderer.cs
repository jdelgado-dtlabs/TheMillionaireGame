using System.Drawing;
using System.Drawing.Drawing2D;
using MillionaireGame.Core.Models;

namespace MillionaireGame.Core.Graphics;

/// <summary>
/// Renders money tree ladder using SVG-style graphics with theme colors
/// </summary>
public class SvgMoneyTreeRenderer
{
    /// <summary>
    /// Renders the money tree ladder to a Graphics object
    /// </summary>
    /// <param name="g">Graphics context</param>
    /// <param name="moneyTree">Theme money tree configuration</param>
    /// <param name="bounds">Rendering bounds</param>
    /// <param name="currentLevel">Current active level (1-15)</param>
    /// <param name="safetyNet1">First safety net level (0 = disabled)</param>
    /// <param name="safetyNet2">Second safety net level (0 = disabled)</param>
    /// <param name="isRiskMode">Whether Risk Mode is active (disables safety nets)</param>
    /// <param name="useSafetyNetAltGraphic">Whether to use alternate safety net lock-in graphic</param>
    /// <param name="shapeType">Shape type from theme strap (e.g., 'Classic', 'Modern', 'Rounded', 'Sharp')</param>
    public void RenderMoneyTreeToGraphics(
        System.Drawing.Graphics g,
        ThemeMoneyTree moneyTree,
        Rectangle bounds,
        int currentLevel,
        int safetyNet1 = 0,
        int safetyNet2 = 0,
        bool isRiskMode = false,
        bool useSafetyNetAltGraphic = false,
        string shapeType = "Classic",
        bool isFlashing = false)
    {
        if (moneyTree == null) return;

        // Enable high quality rendering
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.CompositingQuality = CompositingQuality.HighQuality;

        // Calculate dimensions for 15 levels
        const int levelCount = 15;
        float levelHeight = bounds.Height / (float)levelCount;
        float leftMargin = 20;
        float rightMargin = 20;
        float ladderWidth = bounds.Width - leftMargin - rightMargin;

        // Parse theme colors
        var inactiveColor = ColorTranslator.FromHtml(moneyTree.InactiveColor);
        var activeColor = ColorTranslator.FromHtml(moneyTree.ActiveColor);
        var completedColor = ColorTranslator.FromHtml(moneyTree.CompletedColor);
        var safeHavenColor = ColorTranslator.FromHtml(moneyTree.SafeHavenColor);
        var highlightColor = ColorTranslator.FromHtml(moneyTree.HighlightColor);

        // Draw levels from bottom to top (level 1 at bottom, 15 at top)
        for (int level = 1; level <= levelCount; level++)
        {
            // Calculate Y position (inverted - level 1 at bottom)
            float y = bounds.Y + bounds.Height - (level * levelHeight);
            float x = bounds.X + leftMargin;

            // Determine level state and color
            Color levelColor = inactiveColor; // Default: not yet reached
            bool isSafetyNet = false;
            bool isCurrentLevel = (level == currentLevel);
            bool isCompleted = (level < currentLevel);

            // Check if this is a safety net level (and not disabled by Risk Mode)
            if (!isRiskMode)
            {
                if ((level == 5 && (safetyNet1 == 5 || safetyNet2 == 5)) ||
                    (level == 10 && (safetyNet1 == 10 || safetyNet2 == 10)) ||
                    (level == safetyNet1 || level == safetyNet2))
                {
                    isSafetyNet = true;
                }
            }

            // Determine color based on state
            if (isCurrentLevel)
            {
                if (isFlashing)
                {
                    // During flashing: show highlighted color when flash state is ON,
                    // otherwise render as outline-only (blank) so overlay/PNG behavior is preserved
                    if (useSafetyNetAltGraphic)
                    {
                        levelColor = highlightColor;
                    }
                    else
                    {
                        levelColor = inactiveColor; // will be drawn as outline-only below
                    }
                }
                else
                {
                    // Normal (not flashing): draw current level as active
                    levelColor = activeColor;
                }
            }
            else if (isCompleted)
            {
                // Completed level - use grey
                levelColor = Color.Gray;
            }
            else if (isSafetyNet)
            {
                // Safety net (future level) - outline only
                levelColor = safeHavenColor;
            }
            else
            {
                // Inactive (future level) - outline only
                levelColor = inactiveColor;
            }

            // Determine if we should draw as outline only (unearned levels)
            bool drawOutlineOnly = !isCurrentLevel && !isCompleted;

            // Draw level rung with theme shape
            float rungHeight = levelHeight * 0.7f; // 70% of level height
            float yCenter = y + (levelHeight / 2f);
            float rungY = yCenter - (rungHeight / 2f);

            DrawRung(g, levelColor, x, rungY, ladderWidth, rungHeight, shapeType, drawOutlineOnly, isCurrentLevel || isCompleted);

            // Add glow/highlight effect for current level
            bool shouldDrawHighlight = false;
            if (isCurrentLevel && moneyTree.HighlightEnabled)
            {
                // If we're in flashing mode, only draw the highlight when the
                // alternate graphic (flash ON) is active. Otherwise draw normally.
                shouldDrawHighlight = isFlashing ? useSafetyNetAltGraphic : true;
            }

            if (shouldDrawHighlight)
            {
                DrawHighlightEffect(g, highlightColor, x, rungY, ladderWidth, rungHeight,
                    moneyTree.HighlightType, moneyTree.HighlightIntensity, shapeType);
            }

            // Draw safety net indicator (thicker border)
            if (isSafetyNet)
            {
                using var safetyPen = new Pen(safeHavenColor, 4);
                DrawRungBorder(g, safetyPen, x, rungY, ladderWidth, rungHeight, shapeType);
            }
        }
    }

    private void DrawRung(System.Drawing.Graphics g, Color color, float x, float y, float width, float height, string shapeType, bool outlineOnly = false, bool useGradient = false)
    {
        using var path = CreateRungPath(x, y, width, height, shapeType);

        if (outlineOnly)
        {
            // Outline only with transparent background for unearned levels
            using var pen = new Pen(color, 2);
            g.DrawPath(pen, path);
        }
        else if (useGradient)
        {
            // 3D gradient: dark-bright-dark (top to bottom)
            var darkColor = ControlPaint.Dark(color, 0.3f);
            var brightColor = ControlPaint.Light(color, 0.3f);
            
            using var brush = new LinearGradientBrush(
                new PointF(x, y),
                new PointF(x, y + height),
                darkColor,
                darkColor);
            
            // Create color blend for dark-bright-dark effect
            var colorBlend = new ColorBlend(3);
            colorBlend.Colors = new[] { darkColor, brightColor, darkColor };
            colorBlend.Positions = new[] { 0f, 0.5f, 1f };
            brush.InterpolationColors = colorBlend;
            
            g.FillPath(brush, path);
            
            // Draw subtle border
            using var pen = new Pen(Color.FromArgb(100, 0, 0, 0), 2);
            g.DrawPath(pen, path);
        }
        else
        {
            // Solid fill (legacy mode)
            using var brush = new SolidBrush(color);
            g.FillPath(brush, path);
            
            // Draw border
            using var pen = new Pen(Color.FromArgb(100, 0, 0, 0), 2);
            g.DrawPath(pen, path);
        }
    }

    private void DrawRungBorder(System.Drawing.Graphics g, Pen pen, float x, float y, float width, float height, string shapeType)
    {
        using var path = CreateRungPath(x, y, width, height, shapeType);
        g.DrawPath(pen, path);
    }

    private GraphicsPath CreateRungPath(float x, float y, float width, float height, string shapeType)
    {
        var path = new GraphicsPath();

        switch (shapeType?.ToLowerInvariant())
        {
            case "sharp":
                // Sharp corners - simple rectangle
                path.AddRectangle(new RectangleF(x, y, width, height));
                break;

            case "modern":
                // Modern style - medium rounded corners (half of Classic)
                float modernRadius = Math.Min(height * 0.15f, 6f);
                path = CreateRoundedRectanglePath(x, y, width, height, modernRadius);
                break;

            case "rounded":
                // Rounded style - pillbox shape (heavily rounded)
                float pillRadius = height / 2f;
                path = CreateRoundedRectanglePath(x, y, width, height, pillRadius);
                break;

            case "classic":
            default:
                // Classic style - subtle rounded corners
                float classicRadius = Math.Min(height * 0.3f, 8f);
                path = CreateRoundedRectanglePath(x, y, width, height, classicRadius);
                break;
        }

        return path;
    }

    private GraphicsPath CreateRoundedRectanglePath(float x, float y, float width, float height, float radius)
    {
        var path = new GraphicsPath();
        float diameter = radius * 2;

        path.AddArc(x, y, diameter, diameter, 180, 90); // Top-left
        path.AddArc(x + width - diameter, y, diameter, diameter, 270, 90); // Top-right
        path.AddArc(x + width - diameter, y + height - diameter, diameter, diameter, 0, 90); // Bottom-right
        path.AddArc(x, y + height - diameter, diameter, diameter, 90, 90); // Bottom-left
        path.CloseFigure();

        return path;
    }

    private void DrawHighlightEffect(System.Drawing.Graphics g, Color highlightColor, float x, float y, float width, float height, string highlightType, int intensity, string shapeType)
    {
        switch (highlightType)
        {
            case "PulsingGlow":
                DrawGlowEffect(g, highlightColor, x, y, width, height, intensity, shapeType);
                break;
            case "SolidBorder":
                DrawSolidBorderEffect(g, highlightColor, x, y, width, height, intensity, shapeType);
                break;
            case "Shadow":
                DrawShadowEffect(g, highlightColor, x, y, width, height, intensity, shapeType);
                break;
            default:
                DrawGlowEffect(g, highlightColor, x, y, width, height, intensity, shapeType);
                break;
        }
    }

    private void DrawGlowEffect(System.Drawing.Graphics g, Color glowColor, float x, float y, float width, float height, int intensity, string shapeType)
    {
        // Draw multiple expanding rectangles with decreasing opacity
        int glowSize = (int)(intensity / 10f); // 80 intensity = 8px glow
        int steps = 5;

        for (int i = steps; i > 0; i--)
        {
            float expansion = (glowSize * i) / steps;
            int alpha = (int)((intensity / steps) * (steps - i + 1));
            alpha = Math.Min(255, Math.Max(0, alpha));

            using var glowBrush = new SolidBrush(Color.FromArgb(alpha, glowColor));
            using var glowPath = CreateRungPath(
                x - expansion, y - expansion,
                width + (expansion * 2), height + (expansion * 2), shapeType);
            g.FillPath(glowBrush, glowPath);
        }
    }

    private void DrawSolidBorderEffect(System.Drawing.Graphics g, Color borderColor, float x, float y, float width, float height, int intensity, string shapeType)
    {
        float borderWidth = intensity / 20f; // 80 intensity = 4px border
        using var pen = new Pen(borderColor, borderWidth);
        using var path = CreateRungPath(x, y, width, height, shapeType);
        g.DrawPath(pen, path);
    }

    private void DrawShadowEffect(System.Drawing.Graphics g, Color shadowColor, float x, float y, float width, float height, int intensity, string shapeType)
    {
        float shadowOffset = intensity / 20f; // 80 intensity = 4px shadow
        int shadowAlpha = Math.Min(255, intensity * 2);

        using var shadowBrush = new SolidBrush(Color.FromArgb(shadowAlpha, shadowColor));
        using var shadowPath = CreateRungPath(
            x + shadowOffset, y + shadowOffset,
            width, height, shapeType);
        g.FillPath(shadowBrush, shadowPath);
    }
}
