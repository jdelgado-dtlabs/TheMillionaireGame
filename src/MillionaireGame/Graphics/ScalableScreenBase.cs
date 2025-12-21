using System.Drawing.Drawing2D;

namespace MillionaireGame.Graphics;

/// <summary>
/// Base class for scalable game screens with proportional layout and custom rendering
/// </summary>
public abstract class ScalableScreenBase : Form
{
    // Design resolution - all positions and sizes are calculated relative to this
    protected const int DesignWidth = 1920;
    protected const int DesignHeight = 1080;
    
    // Current scale factors
    protected float ScaleX => (float)ClientSize.Width / DesignWidth;
    protected float ScaleY => (float)ClientSize.Height / DesignHeight;
    
    protected readonly TextureManager TextureManager;
    protected TextureManager.TextureSet CurrentTextureSet;

    // Window state tracking for borderless fullscreen
    private FormWindowState _previousWindowState;
    private FormBorderStyle _previousBorderStyle;
    private Size _previousSize;
    private Point _previousLocation;

    protected ScalableScreenBase()
    {
        TextureManager = TextureManager.Instance;
        CurrentTextureSet = TextureManager.TextureSet.Default;
        
        // Enable double buffering to prevent flicker
        DoubleBuffered = true;
        
        // Set up event handlers
        Resize += OnFormResize;
        SizeChanged += OnFormSizeChanged;
        KeyPreview = true; // Enable key events on form
        KeyDown += OnFormKeyDown;
    }

    private void OnFormResize(object? sender, EventArgs e)
    {
        Invalidate(); // Trigger repaint when window is resized
    }

    private void OnFormSizeChanged(object? sender, EventArgs e)
    {
        // Handle maximize: switch to borderless fullscreen
        if (WindowState == FormWindowState.Maximized && FormBorderStyle != FormBorderStyle.None)
        {
            _previousWindowState = FormWindowState.Maximized; // Track that we're maximized
            _previousBorderStyle = FormBorderStyle;
            FormBorderStyle = FormBorderStyle.None;
        }
        // Handle restore via window controls: restore borders
        else if (WindowState == FormWindowState.Normal && _previousWindowState == FormWindowState.Maximized && FormBorderStyle == FormBorderStyle.None)
        {
            FormBorderStyle = _previousBorderStyle != FormBorderStyle.None ? _previousBorderStyle : FormBorderStyle.Sizable;
            _previousWindowState = FormWindowState.Normal;
        }
    }

    private void OnFormKeyDown(object? sender, KeyEventArgs e)
    {
        // ESC key exits fullscreen
        if (e.KeyCode == Keys.Escape && WindowState == FormWindowState.Maximized)
        {
            // Restore previous state
            FormBorderStyle = _previousBorderStyle != FormBorderStyle.None ? _previousBorderStyle : FormBorderStyle.Sizable;
            WindowState = FormWindowState.Normal;
            _previousWindowState = FormWindowState.Normal;
            if (_previousSize != Size.Empty)
            {
                Size = _previousSize;
            }
            if (_previousLocation != Point.Empty)
            {
                Location = _previousLocation;
            }
            e.Handled = true;
        }
    }

    protected void SaveWindowState()
    {
        if (WindowState == FormWindowState.Normal)
        {
            _previousSize = Size;
            _previousLocation = Location;
            _previousBorderStyle = FormBorderStyle;
            _previousWindowState = WindowState;
        }
    }

    /// <summary>
    /// Scale a value from design coordinates to current screen coordinates
    /// </summary>
    protected float ScaleValue(float designValue, bool useXScale = true)
    {
        return designValue * (useXScale ? ScaleX : ScaleY);
    }

    /// <summary>
    /// Scale a point from design coordinates to current screen coordinates
    /// </summary>
    protected PointF ScalePoint(float designX, float designY)
    {
        return new PointF(designX * ScaleX, designY * ScaleY);
    }

    /// <summary>
    /// Scale a rectangle from design coordinates to current screen coordinates
    /// </summary>
    protected RectangleF ScaleRect(float designX, float designY, float designWidth, float designHeight)
    {
        return new RectangleF(
            designX * ScaleX,
            designY * ScaleY,
            designWidth * ScaleX,
            designHeight * ScaleY
        );
    }

    /// <summary>
    /// Draw a scaled image
    /// </summary>
    protected void DrawScaledImage(System.Drawing.Graphics g, Image? image, float designX, float designY, float designWidth, float designHeight)
    {
        if (image == null) return;

        var destRect = ScaleRect(designX, designY, designWidth, designHeight);
        
        // Use high quality rendering
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        
        g.DrawImage(image, destRect);
    }

    /// <summary>
    /// Draw a scaled image with source rectangle (for cropping)
    /// </summary>
    protected void DrawScaledImage(System.Drawing.Graphics g, Image? image, RectangleF sourceRect, float designX, float designY, float designWidth, float designHeight)
    {
        if (image == null) return;

        var destRect = ScaleRect(designX, designY, designWidth, designHeight);
        
        // Use high quality rendering
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        
        g.DrawImage(image, destRect, sourceRect, GraphicsUnit.Pixel);
    }

    /// <summary>
    /// Draw text with scaled font and position
    /// </summary>
    protected void DrawScaledText(System.Drawing.Graphics g, string text, Font baseFont, Brush brush, float designX, float designY, float designWidth, float designHeight, StringFormat? format = null)
    {
        var destRect = ScaleRect(designX, designY, designWidth, designHeight);
        
        // Scale font size
        float scaledFontSize = baseFont.Size * Math.Min(ScaleX, ScaleY);
        using var scaledFont = new Font(baseFont.FontFamily, scaledFontSize, baseFont.Style, baseFont.Unit);
        
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
        g.DrawString(text, scaledFont, brush, destRect, format);
    }

    /// <summary>
    /// Create a centered string format
    /// </summary>
    protected StringFormat CreateCenteredFormat()
    {
        return new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        // Fill with black background
        e.Graphics.Clear(Color.Black);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        
        // Set high quality rendering
        e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        
        // Let derived classes do their custom rendering
        RenderScreen(e.Graphics);
    }

    /// <summary>
    /// Override this method in derived classes to implement custom rendering
    /// </summary>
    protected abstract void RenderScreen(System.Drawing.Graphics g);

    /// <summary>
    /// Update the texture set used for rendering
    /// </summary>
    public void SetTextureSet(TextureManager.TextureSet textureSet)
    {
        CurrentTextureSet = textureSet;
        Invalidate(); // Trigger repaint
    }
}
