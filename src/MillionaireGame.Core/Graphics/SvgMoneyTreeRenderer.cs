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
    public void RenderMoneyTreeToGraphics(
        System.Drawing.Graphics g,
        ThemeMoneyTree moneyTree,
        Rectangle bounds,
        int currentLevel,
        int safetyNet1 = 0,
        int safetyNet2 = 0,
        bool isRiskMode = false,
        bool useSafetyNetAltGraphic = false)
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
                // Current level - use alternate color if showing safety net lock-in
                levelColor = useSafetyNetAltGraphic ? highlightColor : activeColor;
            }
            else if (isCompleted)
            {
                // Completed level
                levelColor = completedColor;
            }
            else if (isSafetyNet)
            {
                // Safety net (future level)
                levelColor = safeHavenColor;
            }
            else
            {
                // Inactive (future level)
                levelColor = inactiveColor;
            }

            // Draw level rung with rounded corners
            float rungHeight = levelHeight * 0.7f; // 70% of level height
            float yCenter = y + (levelHeight / 2f);
            float rungY = yCenter - (rungHeight / 2f);

            DrawRoundedRectangle(g, levelColor, x, rungY, ladderWidth, rungHeight, 8);

            // Add glow/highlight effect for current level
            if (isCurrentLevel && moneyTree.HighlightEnabled)
            {
                DrawHighlightEffect(g, highlightColor, x, rungY, ladderWidth, rungHeight, 
                    moneyTree.HighlightType, moneyTree.HighlightIntensity);
            }

            // Draw safety net indicator (thicker border)
            if (isSafetyNet)
            {
                using var safetyPen = new Pen(safeHavenColor, 4);
                DrawRoundedRectangleBorder(g, safetyPen, x, rungY, ladderWidth, rungHeight, 8);
            }
        }

        // Draw vertical rails (side bars) to connect rungs
        float railWidth = 6;
        float railX1 = bounds.X + leftMargin - 5;
        float railX2 = bounds.X + leftMargin + ladderWidth + 5;

        using var railBrush = new SolidBrush(ColorTranslator.FromHtml(moneyTree.InactiveColor));
        g.FillRectangle(railBrush, railX1, bounds.Y, railWidth, bounds.Height);
        g.FillRectangle(railBrush, railX2 - railWidth, bounds.Y, railWidth, bounds.Height);
    }

    private void DrawRoundedRectangle(System.Drawing.Graphics g, Color color, float x, float y, float width, float height, float radius)
    {
        using var brush = new SolidBrush(color);
        using var path = CreateRoundedRectanglePath(x, y, width, height, radius);
        g.FillPath(brush, path);

        // Draw border
        using var pen = new Pen(Color.FromArgb(100, 0, 0, 0), 2);
        g.DrawPath(pen, path);
    }

    private void DrawRoundedRectangleBorder(System.Drawing.Graphics g, Pen pen, float x, float y, float width, float height, float radius)
    {
        using var path = CreateRoundedRectanglePath(x, y, width, height, radius);
        g.DrawPath(pen, path);
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

    private void DrawHighlightEffect(System.Drawing.Graphics g, Color highlightColor, float x, float y, float width, float height, string highlightType, int intensity)
    {
        switch (highlightType)
        {
            case "PulsingGlow":
                DrawGlowEffect(g, highlightColor, x, y, width, height, intensity);
                break;
            case "SolidBorder":
                DrawSolidBorderEffect(g, highlightColor, x, y, width, height, intensity);
                break;
            case "Shadow":
                DrawShadowEffect(g, highlightColor, x, y, width, height, intensity);
                break;
            default:
                DrawGlowEffect(g, highlightColor, x, y, width, height, intensity);
                break;
        }
    }

    private void DrawGlowEffect(System.Drawing.Graphics g, Color glowColor, float x, float y, float width, float height, int intensity)
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
            using var glowPath = CreateRoundedRectanglePath(
                x - expansion, y - expansion,
                width + (expansion * 2), height + (expansion * 2),
                8 + expansion);
            g.FillPath(glowBrush, glowPath);
        }
    }

    private void DrawSolidBorderEffect(System.Drawing.Graphics g, Color borderColor, float x, float y, float width, float height, int intensity)
    {
        float borderWidth = intensity / 20f; // 80 intensity = 4px border
        using var pen = new Pen(borderColor, borderWidth);
        using var path = CreateRoundedRectanglePath(x, y, width, height, 8);
        g.DrawPath(pen, path);
    }

    private void DrawShadowEffect(System.Drawing.Graphics g, Color shadowColor, float x, float y, float width, float height, int intensity)
    {
        float shadowOffset = intensity / 20f; // 80 intensity = 4px shadow
        int shadowAlpha = Math.Min(255, intensity * 2);

        using var shadowBrush = new SolidBrush(Color.FromArgb(shadowAlpha, shadowColor));
        using var shadowPath = CreateRoundedRectanglePath(
            x + shadowOffset, y + shadowOffset,
            width, height, 8);
        g.FillPath(shadowBrush, shadowPath);
    }
}
