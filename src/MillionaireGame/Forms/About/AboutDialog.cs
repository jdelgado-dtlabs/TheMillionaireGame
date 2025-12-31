using System.Reflection;
using System.Diagnostics;
using MillionaireGame.Core.Helpers;

namespace MillionaireGame.Forms.About;

public partial class AboutDialog : Form
{
    private readonly System.Windows.Forms.Timer _animationTimer;
    private float _beamAngle = 0f;
    private const int BeamCount = 6;
    private const float BeamWidth = 25f;
    private const float BeamSpeed = 1.5f;
    private const float MaxSweepAngle = 45f; // Maximum angle from vertical

    public AboutDialog()
    {
        InitializeComponent();
        IconHelper.ApplyToForm(this);
        
        // Enable double buffering for smooth animation
        SetStyle(ControlStyles.AllPaintingInWmPaint | 
                 ControlStyles.UserPaint | 
                 ControlStyles.OptimizedDoubleBuffer, true);
        
        pnlBackground.Paint += PnlBackground_Paint;
        
        // Setup animation timer (but don't start it yet)
        _animationTimer = new System.Windows.Forms.Timer
        {
            Interval = 30 // ~33 FPS
        };
        _animationTimer.Tick += AnimationTimer_Tick;
        
        LoadLogo();
        LoadVersionInfo();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        // Start animation when dialog is shown
        _beamAngle = 0f; // Reset angle
        _animationTimer.Start();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // Stop animation when dialog is closing
        _animationTimer.Stop();
        base.OnFormClosing(e);
    }

    private void AnimationTimer_Tick(object? sender, EventArgs e)
    {
        _beamAngle += BeamSpeed;
        if (_beamAngle >= 360f)
        {
            _beamAngle -= 360f;
        }
        pnlBackground.Invalidate();
    }

    private void PnlBackground_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        
        var panelWidth = pnlBackground.Width;
        var panelHeight = pnlBackground.Height;
        
        // Calculate oscillating angle (sweep from outside to parallel to outside)
        // Use sine wave to create smooth oscillation: 0 degrees (parallel) to MaxSweepAngle
        var sweepProgress = (float)Math.Sin(_beamAngle * Math.PI / 180.0);
        var currentSweepAngle = sweepProgress * MaxSweepAngle;
        
        // Position beams evenly across the width
        var spacing = panelWidth / (float)(BeamCount + 1);
        
        for (int i = 0; i < BeamCount; i++)
        {
            var beamX = spacing * (i + 1);
            
            // Alternate beam directions (left side sweeps outward-left, right side sweeps outward-right)
            var centerOffset = beamX - (panelWidth / 2f);
            var beamDirection = centerOffset > 0 ? 1f : -1f; // Right side positive, left side negative
            
            // Calculate beam angle: parallel (90°) + sweep based on position
            var beamAngle = 90f + (currentSweepAngle * beamDirection);
            
            DrawBeam(g, beamX, 0, beamAngle, panelHeight * 1.5f);
        }
    }
    
    private void DrawBeam(System.Drawing.Graphics g, float startX, float startY, float angleDegrees, float length)
    {
        // Convert angle to radians
        var angleRad = angleDegrees * (float)Math.PI / 180f;
        
        // Calculate end point
        var endX = startX + (float)Math.Cos(angleRad) * length;
        var endY = startY + (float)Math.Sin(angleRad) * length;
        
        // Create beam polygon (wider at the bottom)
        var halfWidthTop = BeamWidth / 2f;
        var halfWidthBottom = BeamWidth * 2f;
        
        var perpAngle = angleRad - (float)Math.PI / 2f;
        
        var points = new PointF[]
        {
            new PointF(startX + (float)Math.Cos(perpAngle) * halfWidthTop, 
                      startY + (float)Math.Sin(perpAngle) * halfWidthTop),
            new PointF(startX - (float)Math.Cos(perpAngle) * halfWidthTop, 
                      startY - (float)Math.Sin(perpAngle) * halfWidthTop),
            new PointF(endX - (float)Math.Cos(perpAngle) * halfWidthBottom, 
                      endY - (float)Math.Sin(perpAngle) * halfWidthBottom),
            new PointF(endX + (float)Math.Cos(perpAngle) * halfWidthBottom, 
                      endY + (float)Math.Sin(perpAngle) * halfWidthBottom)
        };
        
        // Create gradient brush (bright at top, fading to transparent at bottom)
        using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
            new PointF(startX, startY),
            new PointF(endX, endY),
            Color.FromArgb(60, 135, 206, 250), // Brighter blue at origin
            Color.FromArgb(0, 135, 206, 250));  // Fade to transparent
        
        g.FillPolygon(brush, points);
    }

    private void LoadLogo()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "MillionaireGame.lib.image.logo.png";
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                picLogo.Image = Image.FromStream(stream);
            }
        }
        catch (Exception ex)
        {
            if (Program.DebugMode)
            {
                Console.WriteLine($"[About] Failed to load logo: {ex.Message}");
            }
        }
    }

    private void LoadVersionInfo()
    {
        // Get version from assembly attributes
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        
        if (version != null)
        {
            // Display as Major.Minor.Build (e.g., 0.9.8)
            lblVersion.Text = $"{version.Major}.{version.Minor}.{version.Build}";
        }
        else
        {
            lblVersion.Text = "0.9.8";
        }
        
        // Get .NET runtime version dynamically
        var dotnetVersion = Environment.Version;
        lblBuildInfo.Text = $"Built with C# and .NET {dotnetVersion.Major}.{dotnetVersion.Minor}";
    }

    private void lnkAuthor_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/jdelgado-dtlabs",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unable to open link: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void lnkOriginalAuthor_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/Macronair",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unable to open link: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnClose_Click(object sender, EventArgs e)
    {
        Close();
    }
}
