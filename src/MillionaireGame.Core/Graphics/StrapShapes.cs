using System.Drawing;
using System.Drawing.Drawing2D;

namespace MillionaireGame.Core.Graphics;

/// <summary>
/// Provides SVG-style shape definitions for strap overlays
/// </summary>
public static class StrapShapes
{
    /// <summary>
    /// Get the GraphicsPath for a specific shape type
    /// </summary>
    /// <param name="shapeName">Shape name (Classic, Modern, Rounded, Sharp, Elegant)</param>
    /// <param name="bounds">Bounding rectangle for the shape</param>
    /// <returns>GraphicsPath representing the shape</returns>
    public static GraphicsPath GetShape(string shapeName, Rectangle bounds)
    {
        return shapeName.ToLower() switch
        {
            "classic" => CreateClassicShape(bounds),
            "modern" => CreateModernShape(bounds),
            "rounded" => CreateRoundedShape(bounds),
            "sharp" => CreateSharpShape(bounds),
            "elegant" => CreateElegantShape(bounds),
            _ => CreateClassicShape(bounds) // Default fallback
        };
    }

    /// <summary>
    /// Classic hexagonal shape (Who Wants to Be a Millionaire style)
    /// </summary>
    private static GraphicsPath CreateClassicShape(Rectangle bounds)
    {
        var path = new GraphicsPath();
        
        // Calculate points for hexagon with angled sides
        int cornerWidth = bounds.Width / 10; // Width of angled corners
        
        var points = new PointF[]
        {
            new PointF(bounds.Left + cornerWidth, bounds.Top),                    // Top left
            new PointF(bounds.Right - cornerWidth, bounds.Top),                   // Top right
            new PointF(bounds.Right, bounds.Top + bounds.Height / 2),             // Right point
            new PointF(bounds.Right - cornerWidth, bounds.Bottom),                // Bottom right
            new PointF(bounds.Left + cornerWidth, bounds.Bottom),                 // Bottom left
            new PointF(bounds.Left, bounds.Top + bounds.Height / 2)               // Left point
        };
        
        path.AddPolygon(points);
        path.CloseFigure();
        
        return path;
    }

    /// <summary>
    /// Modern rectangular shape with subtle angled ends
    /// </summary>
    private static GraphicsPath CreateModernShape(Rectangle bounds)
    {
        var path = new GraphicsPath();
        
        // Sleek rectangle with slight angles
        int angleWidth = bounds.Width / 20;
        
        var points = new PointF[]
        {
            new PointF(bounds.Left + angleWidth, bounds.Top),
            new PointF(bounds.Right - angleWidth, bounds.Top),
            new PointF(bounds.Right, bounds.Bottom),
            new PointF(bounds.Left, bounds.Bottom)
        };
        
        path.AddPolygon(points);
        path.CloseFigure();
        
        return path;
    }

    /// <summary>
    /// Rounded rectangle shape with curved corners
    /// </summary>
    private static GraphicsPath CreateRoundedShape(Rectangle bounds)
    {
        var path = new GraphicsPath();
        
        int cornerRadius = Math.Min(bounds.Width, bounds.Height) / 8;
        
        // Top left arc
        path.AddArc(bounds.Left, bounds.Top, cornerRadius * 2, cornerRadius * 2, 180, 90);
        // Top right arc
        path.AddArc(bounds.Right - cornerRadius * 2, bounds.Top, cornerRadius * 2, cornerRadius * 2, 270, 90);
        // Bottom right arc
        path.AddArc(bounds.Right - cornerRadius * 2, bounds.Bottom - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 0, 90);
        // Bottom left arc
        path.AddArc(bounds.Left, bounds.Bottom - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 90, 90);
        
        path.CloseFigure();
        
        return path;
    }

    /// <summary>
    /// Sharp angular shape with dramatic points
    /// </summary>
    private static GraphicsPath CreateSharpShape(Rectangle bounds)
    {
        var path = new GraphicsPath();
        
        // Aggressive angular design
        int pointWidth = bounds.Width / 6;
        int midY = bounds.Top + bounds.Height / 2;
        
        var points = new PointF[]
        {
            new PointF(bounds.Left + pointWidth, bounds.Top),
            new PointF(bounds.Right - pointWidth * 2, bounds.Top),
            new PointF(bounds.Right - pointWidth, bounds.Top + bounds.Height / 4),
            new PointF(bounds.Right, midY),
            new PointF(bounds.Right - pointWidth, bounds.Bottom - bounds.Height / 4),
            new PointF(bounds.Right - pointWidth * 2, bounds.Bottom),
            new PointF(bounds.Left + pointWidth, bounds.Bottom),
            new PointF(bounds.Left, midY)
        };
        
        path.AddPolygon(points);
        path.CloseFigure();
        
        return path;
    }

    /// <summary>
    /// Elegant curved shape with flowing lines
    /// </summary>
    private static GraphicsPath CreateElegantShape(Rectangle bounds)
    {
        var path = new GraphicsPath();
        
        // Bezier curves for smooth, elegant appearance
        int curveDepth = bounds.Height / 6;
        
        // Start from top left
        path.StartFigure();
        
        // Top edge with gentle curve
        path.AddBezier(
            new PointF(bounds.Left, bounds.Top + curveDepth),
            new PointF(bounds.Left + bounds.Width / 4, bounds.Top),
            new PointF(bounds.Left + bounds.Width * 3 / 4, bounds.Top),
            new PointF(bounds.Right, bounds.Top + curveDepth)
        );
        
        // Right edge
        path.AddLine(
            new PointF(bounds.Right, bounds.Top + curveDepth),
            new PointF(bounds.Right, bounds.Bottom - curveDepth)
        );
        
        // Bottom edge with gentle curve
        path.AddBezier(
            new PointF(bounds.Right, bounds.Bottom - curveDepth),
            new PointF(bounds.Left + bounds.Width * 3 / 4, bounds.Bottom),
            new PointF(bounds.Left + bounds.Width / 4, bounds.Bottom),
            new PointF(bounds.Left, bounds.Bottom - curveDepth)
        );
        
        // Left edge
        path.AddLine(
            new PointF(bounds.Left, bounds.Bottom - curveDepth),
            new PointF(bounds.Left, bounds.Top + curveDepth)
        );
        
        path.CloseFigure();
        
        return path;
    }

    /// <summary>
    /// Get all available shape names
    /// </summary>
    public static string[] GetAvailableShapes()
    {
        return new[]
        {
            "Classic",
            "Modern",
            "Rounded",
            "Sharp",
            "Elegant"
        };
    }

    /// <summary>
    /// Create a simple rectangular shape (fallback/default)
    /// </summary>
    public static GraphicsPath CreateRectangle(Rectangle bounds)
    {
        var path = new GraphicsPath();
        path.AddRectangle(bounds);
        return path;
    }

    /// <summary>
    /// Create an arrow-shaped strap (for directional indicators)
    /// </summary>
    public static GraphicsPath CreateArrowShape(Rectangle bounds, ArrowDirection direction)
    {
        var path = new GraphicsPath();
        
        int arrowWidth = bounds.Width / 8;
        int midY = bounds.Top + bounds.Height / 2;
        
        switch (direction)
        {
            case ArrowDirection.Right:
                var pointsRight = new PointF[]
                {
                    new PointF(bounds.Left, bounds.Top),
                    new PointF(bounds.Right - arrowWidth, bounds.Top),
                    new PointF(bounds.Right, midY),
                    new PointF(bounds.Right - arrowWidth, bounds.Bottom),
                    new PointF(bounds.Left, bounds.Bottom)
                };
                path.AddPolygon(pointsRight);
                break;
                
            case ArrowDirection.Left:
                var pointsLeft = new PointF[]
                {
                    new PointF(bounds.Left + arrowWidth, bounds.Top),
                    new PointF(bounds.Right, bounds.Top),
                    new PointF(bounds.Right, bounds.Bottom),
                    new PointF(bounds.Left + arrowWidth, bounds.Bottom),
                    new PointF(bounds.Left, midY)
                };
                path.AddPolygon(pointsLeft);
                break;
        }
        
        path.CloseFigure();
        return path;
    }
}

/// <summary>
/// Arrow direction for arrow-shaped straps
/// </summary>
public enum ArrowDirection
{
    Left,
    Right
}
