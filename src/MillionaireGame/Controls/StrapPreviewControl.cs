using System.ComponentModel;
using MillionaireGame.Core.Graphics;
using MillionaireGame.Core.Models;

namespace MillionaireGame.Controls;

/// <summary>
/// Windows Forms control for previewing strap appearance in real-time
/// </summary>
public class StrapPreviewControl : Control
{
    private ThemeStrap? _strap;
    private string _previewText = "Preview Text";
    private System.Windows.Forms.Timer? _animationTimer;
    private float _animationProgress;
    private SvgStrapRenderer? _renderer;

    /// <summary>
    /// Gets or sets the strap configuration to preview
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ThemeStrap? Strap
    {
        get => _strap;
        set
        {
            _strap = value;
            Invalidate(); // Trigger repaint
        }
    }

    /// <summary>
    /// Gets or sets the preview text
    /// </summary>
    [DefaultValue("Preview Text")]
    [Description("Text to display in the preview")]
    public string PreviewText
    {
        get => _previewText;
        set
        {
            _previewText = value ?? string.Empty;
            Invalidate();
        }
    }

    /// <summary>
    /// Gets or sets whether animation preview is enabled
    /// </summary>
    [DefaultValue(false)]
    [Description("Enable animation preview loop")]
    public bool AnimationPreview { get; set; }

    public StrapPreviewControl()
    {
        // Enable double buffering for smooth rendering
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw, true);

        _renderer = new SvgStrapRenderer();
        
        // Initialize animation timer
        _animationTimer = new System.Windows.Forms.Timer
        {
            Interval = 16 // ~60 FPS
        };
        _animationTimer.Tick += AnimationTimer_Tick;
    }

    private void AnimationTimer_Tick(object? sender, EventArgs e)
    {
        if (!AnimationPreview || _strap == null || !_strap.AnimationEnabled)
        {
            _animationTimer?.Stop();
            return;
        }

        // Update animation progress
        _animationProgress += 0.016f; // ~16ms per frame
        if (_animationProgress > 1.0f)
            _animationProgress = 0.0f;

        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var graphics = e.Graphics;
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

        // Background
        using (var bgBrush = new SolidBrush(BackColor))
        {
            graphics.FillRectangle(bgBrush, ClientRectangle);
        }

        // Render strap if configured
        if (_strap != null && _renderer != null)
        {
            try
            {
                // Add padding to preview bounds
                var previewBounds = new Rectangle(
                    10,
                    10,
                    ClientRectangle.Width - 20,
                    ClientRectangle.Height - 20);

                _renderer.RenderStrapToGraphics(
                    graphics,
                    _strap,
                    _previewText,
                    previewBounds,
                    _animationProgress);
            }
            catch (Exception ex)
            {
                // Draw error message
                using (var errorBrush = new SolidBrush(Color.Red))
                using (var font = new Font("Arial", 10))
                {
                    graphics.DrawString(
                        $"Preview Error: {ex.Message}",
                        font,
                        errorBrush,
                        ClientRectangle);
                }
            }
        }
        else
        {
            // Draw placeholder text
            using (var textBrush = new SolidBrush(Color.Gray))
            using (var font = new Font("Arial", 12))
            {
                var format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                graphics.DrawString(
                    "No strap configured",
                    font,
                    textBrush,
                    ClientRectangle,
                    format);
            }
        }
    }

    /// <summary>
    /// Start animation preview
    /// </summary>
    public void StartAnimation()
    {
        if (_strap?.AnimationEnabled == true)
        {
            _animationProgress = 0f;
            _animationTimer?.Start();
        }
    }

    /// <summary>
    /// Stop animation preview
    /// </summary>
    public void StopAnimation()
    {
        _animationTimer?.Stop();
        _animationProgress = 1.0f; // Show final state
        Invalidate();
    }

    /// <summary>
    /// Refresh the preview
    /// </summary>
    public void RefreshPreview()
    {
        Invalidate();
    }

    protected override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);

        if (!Visible)
        {
            StopAnimation();
        }
        else if (AnimationPreview && _strap?.AnimationEnabled == true)
        {
            StartAnimation();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _animationTimer?.Stop();
            _animationTimer?.Dispose();
            _renderer?.Dispose();
        }
        base.Dispose(disposing);
    }
}
