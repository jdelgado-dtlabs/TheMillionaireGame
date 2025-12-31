using MillionaireGame.Core.Services;
using MillionaireGame.Core.Helpers;
using MillionaireGame.Core.Settings;
using MillionaireGame.Core.Game;
using MillionaireGame.Graphics;
using MillionaireGame.Services;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MillionaireGame.Forms;

/// <summary>
/// Preview orientation for the preview screen
/// </summary>
public enum PreviewOrientation
{
    Vertical,   // Screens stacked vertically (top to bottom)
    Horizontal  // Screens arranged horizontally (left to right)
}

/// <summary>
/// Preview screen that shows Host, Guest, and TV screens
/// </summary>
public class PreviewScreenForm : Form
{
    // Windows API for controlling resize behavior
    private const int WM_NCHITTEST = 0x84;
    private const int HTLEFT = 10;
    private const int HTRIGHT = 11;
    private const int HTTOP = 12;
    private const int HTTOPLEFT = 13;
    private const int HTTOPRIGHT = 14;
    private const int HTBOTTOM = 15;
    private const int HTBOTTOMLEFT = 16;
    private const int HTBOTTOMRIGHT = 17;

    private PreviewPanel _hostPanel;
    private PreviewPanel _guestPanel;
    private PreviewPanel _tvPanel;
    private HostScreenForm _hostScreen;
    private GuestScreenForm _guestScreen;
    private TVScreenFormScalable _tvScreen;
    private ScreenUpdateService _screenService;
    private PreviewOrientation _orientation;
    
    public PreviewOrientation Orientation => _orientation;

    public PreviewScreenForm(GameService gameService, ScreenUpdateService screenService, ControlPanelForm controlPanel, PreviewOrientation orientation = PreviewOrientation.Vertical)
    {
        // Create dedicated screen instances for preview and register with ScreenService
        _hostScreen = new HostScreenForm();
        _hostScreen.IsPreview = true; // Mark as preview to skip intensive animations
        _hostScreen.Initialize(gameService.MoneyTree);
        _hostScreen.CreateControl(); // Force control creation without showing
        screenService.RegisterScreen(_hostScreen); // Register to receive display updates
        
        // Subscribe to host messaging events from control panel
        controlPanel.MessageSent += _hostScreen.OnMessageReceived;
        
        _guestScreen = new GuestScreenForm();
        _guestScreen.IsPreview = true; // Mark as preview to skip intensive animations
        _guestScreen.Initialize(gameService.MoneyTree);
        _guestScreen.CreateControl(); // Force control creation without showing
        screenService.RegisterScreen(_guestScreen); // Register to receive display updates
        
        _tvScreen = new TVScreenFormScalable();
        _tvScreen.IsPreview = true; // Mark as preview to skip intensive animations
        _tvScreen.Initialize(gameService.MoneyTree);
        _tvScreen.CreateControl(); // Force control creation without showing
        screenService.RegisterScreen(_tvScreen); // Register to receive display updates
        
        _orientation = orientation;

        IconHelper.ApplyToForm(this);
        Text = "Preview Screen";
        BackColor = Color.Black;
        FormBorderStyle = FormBorderStyle.Sizable;
        StartPosition = FormStartPosition.Manual;

        // Set initial size based on orientation
        var screen = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1920, 1080);
        
        if (_orientation == PreviewOrientation.Vertical)
        {
            // Vertical: 80% of screen height, width calculated from 16:9 ratio per panel
            int targetHeight = (int)(screen.Height * 0.8);
            int padding = 40; // 10px * 4 (top, between panels x2, bottom)
            int availableHeight = targetHeight - padding;
            int panelHeight = availableHeight / 3;
            int panelWidth = (int)(panelHeight * 16.0 / 9.0);
            Width = panelWidth + 20; // Add margin
            Height = targetHeight;
            
            // Set maximum size to prevent over-expanding when maximized or snapped
            MaximumSize = new Size(panelWidth + 20, screen.Height);
            
            // Set MaximizedBounds for right-side alignment when maximized
            MaximizedBounds = new Rectangle(
                screen.Right - (panelWidth + 20),
                screen.Top,
                panelWidth + 20,
                screen.Height
            );
            
            // Position on the right side of the screen
            Location = new Point(screen.Right - Width, screen.Top + (screen.Height - Height) / 2);
        }
        else
        {
            // Horizontal: 90% of screen width, height calculated from 16:9 ratio per panel
            int targetWidth = (int)(screen.Width * 0.9);
            int padding = 40; // 10px * 4 (left, between panels x2, right)
            int availableWidth = targetWidth - padding;
            int panelWidth = availableWidth / 3;
            int panelHeight = (int)(panelWidth * 9.0 / 16.0);
            Width = targetWidth;
            Height = panelHeight + 20; // Add margin
            
            // Set maximum size to prevent over-expanding when maximized or snapped
            MaximumSize = new Size(screen.Width, panelHeight + 20);
            
            // Set MaximizedBounds for right-side alignment when maximized
            MaximizedBounds = new Rectangle(
                screen.Right - targetWidth,
                screen.Top,
                targetWidth,
                panelHeight + 20
            );
            
            // Position on the right side of the screen
            Location = new Point(screen.Right - Width, screen.Top + (screen.Height - Height) / 2);
        }
        
        MinimumSize = new Size(400, 300);

        // Create preview panels that will render the screens
        _hostPanel = new PreviewPanel(_hostScreen, "Host Screen");
        _guestPanel = new PreviewPanel(_guestScreen, "Guest Screen");
        _tvPanel = new PreviewPanel(_tvScreen, "TV Screen");

        Controls.Add(_hostPanel);
        Controls.Add(_guestPanel);
        Controls.Add(_tvPanel);

        LayoutPanels();

        // Handle form resize - only allow vertical resize in vertical mode, horizontal in horizontal mode
        Resize += PreviewScreenForm_Resize;
        
        // Track window state to restore border when going back to normal
        FormWindowState _lastState = FormWindowState.Normal;
        Resize += (s, e) =>
        {
            // When restoring from Maximized, restore the border style
            if (_lastState == FormWindowState.Maximized && WindowState == FormWindowState.Normal)
            {
                BeginInvoke(new Action(() =>
                {
                    FormBorderStyle = FormBorderStyle.Sizable;
                    Refresh();
                }));
            }
            _lastState = WindowState;
        };
        
        // Handle ESC key to close
        KeyPreview = true;
        KeyDown += PreviewScreenForm_KeyDown;
        
        // Store reference to screen service for event subscriptions
        _screenService = screenService;
        
        // Subscribe to ScreenUpdateService events to invalidate cache only when state changes
        // This is much more efficient than a polling timer
        _screenService.QuestionUpdated += (s, e) => InvalidateAllCaches();
        _screenService.AnswerSelected += (s, e) => InvalidateAllCaches();
        _screenService.AnswerRevealed += (s, e) => InvalidateAllCaches();
        _screenService.LifelineActivated += (s, e) => InvalidateAllCaches();
        _screenService.MoneyUpdated += (s, e) => InvalidateAllCaches();
        _screenService.GameReset += (s, e) => InvalidateAllCaches();
        
        // Also subscribe to screen invalidation events for immediate updates
        // (e.g., animations, timer ticks)
        _hostScreen.Invalidated += (s, e) => _hostPanel.InvalidateCache();
        _guestScreen.Invalidated += (s, e) => _guestPanel.InvalidateCache();
        _tvScreen.Invalidated += (s, e) => _tvPanel.InvalidateCache();
    }

    /// <summary>
    /// Invalidates all preview panel caches, forcing a re-render on next paint
    /// </summary>
    private void InvalidateAllCaches()
    {
        _hostPanel?.InvalidateCache();
        _guestPanel?.InvalidateCache();
        _tvPanel?.InvalidateCache();
    }

    protected override void WndProc(ref Message m)
    {
        // Intercept resize messages to block unwanted resize directions
        if (m.Msg == WM_NCHITTEST)
        {
            base.WndProc(ref m);
            
            var result = (int)m.Result;
            
            if (_orientation == PreviewOrientation.Vertical)
            {
                // In vertical mode, only allow top/bottom resize
                // Block left, right, and corner resizing
                if (result == HTLEFT || result == HTRIGHT || 
                    result == HTTOPLEFT || result == HTTOPRIGHT ||
                    result == HTBOTTOMLEFT || result == HTBOTTOMRIGHT)
                {
                    m.Result = (IntPtr)1; // HTCLIENT - treat as client area (no resize)
                }
            }
            else // Horizontal
            {
                // In horizontal mode, only allow left/right resize
                // Block top, bottom, and corner resizing
                if (result == HTTOP || result == HTBOTTOM ||
                    result == HTTOPLEFT || result == HTTOPRIGHT ||
                    result == HTBOTTOMLEFT || result == HTBOTTOMRIGHT)
                {
                    m.Result = (IntPtr)1; // HTCLIENT - treat as client area (no resize)
                }
            }
            return;
        }
        
        base.WndProc(ref m);
    }

    private void LayoutPanels()
    {
        int padding = 10;
        
        if (_orientation == PreviewOrientation.Vertical)
        {
            // Vertical layout: stacked top to bottom
            int availableHeight = ClientSize.Height - (padding * 4);
            int panelHeight = availableHeight / 3;
            int panelWidth = ClientSize.Width - (padding * 2);

            _hostPanel.Location = new Point(padding, padding);
            _hostPanel.Size = new Size(panelWidth, panelHeight);

            _guestPanel.Location = new Point(padding, padding + panelHeight + padding);
            _guestPanel.Size = new Size(panelWidth, panelHeight);

            _tvPanel.Location = new Point(padding, padding + (panelHeight + padding) * 2);
            _tvPanel.Size = new Size(panelWidth, panelHeight);
        }
        else
        {
            // Horizontal layout: side by side
            int availableWidth = ClientSize.Width - (padding * 4);
            int panelWidth = availableWidth / 3;
            int panelHeight = ClientSize.Height - (padding * 2);

            _hostPanel.Location = new Point(padding, padding);
            _hostPanel.Size = new Size(panelWidth, panelHeight);

            _guestPanel.Location = new Point(padding + panelWidth + padding, padding);
            _guestPanel.Size = new Size(panelWidth, panelHeight);

            _tvPanel.Location = new Point(padding + (panelWidth + padding) * 2, padding);
            _tvPanel.Size = new Size(panelWidth, panelHeight);
        }
    }

    private void PreviewScreenForm_Resize(object? sender, EventArgs e)
    {
        if (Width < MinimumSize.Width || Height < MinimumSize.Height)
            return;

        // Maintain aspect ratio based on orientation
        if (_orientation == PreviewOrientation.Vertical)
        {
            // Vertical: Maintain width proportional to height (16:9 per panel)
            int padding = 40; // Total vertical padding
            int availableHeight = ClientSize.Height - padding;
            int panelHeight = availableHeight / 3;
            int panelWidth = (int)(panelHeight * 16.0 / 9.0);
            int targetWidth = panelWidth + 20; // Add horizontal margin
            
            if (Width != targetWidth)
            {
                Width = targetWidth;
            }
        }
        else
        {
            // Horizontal: Maintain height proportional to width (16:9 per panel)
            int padding = 40; // Total horizontal padding
            int availableWidth = ClientSize.Width - padding;
            int panelWidth = availableWidth / 3;
            int panelHeight = (int)(panelWidth * 9.0 / 16.0);
            int targetHeight = panelHeight + 20; // Add vertical margin
            
            if (Height != targetHeight)
            {
                Height = targetHeight;
            }
        }

        LayoutPanels();
    }

    /// <summary>
    /// Updates the money tree level on all preview screen instances
    /// </summary>
    public void UpdateMoneyTreeLevel(int level)
    {
        _hostScreen?.UpdateMoneyTreeLevel(level);
        _guestScreen?.UpdateMoneyTreeLevel(level);
        _tvScreen?.UpdateMoneyTreeLevel(level);
    }

    /// <summary>
    /// Updates the money tree with safety net lock-in flash animation on all preview screen instances
    /// </summary>
    public void UpdateMoneyTreeWithSafetyNetFlash(int safetyNetLevel, bool flashState)
    {
        _hostScreen?.UpdateMoneyTreeWithSafetyNetFlash(safetyNetLevel, flashState);
        _guestScreen?.UpdateMoneyTreeWithSafetyNetFlash(safetyNetLevel, flashState);
        _tvScreen?.UpdateMoneyTreeWithSafetyNetFlash(safetyNetLevel, flashState);
    }

    private void PreviewScreenForm_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            Close();
            e.Handled = true;
        }
    }
}

/// <summary>
/// Panel that renders a screen form's content
/// </summary>
public class PreviewPanel : Panel
{
    private ScalableScreenBase? _screen;
    private Label _label;
    private Bitmap? _cachedScreenBitmap; // Cached screen render at design resolution
    private bool _isCacheDirty = true; // Track if cache needs regeneration

    public PreviewPanel(ScalableScreenBase screen, string labelText)
    {
        _screen = screen;
        DoubleBuffered = true;
        BorderStyle = BorderStyle.FixedSingle;
        BackColor = Color.Black;
        Paint += PreviewPanel_Paint;
        
        // Add overlay label
        _label = new Label
        {
            Text = labelText,
            ForeColor = Color.White,
            BackColor = Color.FromArgb(128, 0, 0, 0), // Semi-transparent black
            AutoSize = true,
            Padding = new Padding(5),
            Location = new Point(5, 5),
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };
        Controls.Add(_label);
        _label.BringToFront();
    }

    /// <summary>
    /// Invalidates the cached screen bitmap, forcing a re-render on next paint
    /// </summary>
    public void InvalidateCache()
    {
        _isCacheDirty = true;
        Invalidate();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cachedScreenBitmap?.Dispose();
            _cachedScreenBitmap = null;
        }
        base.Dispose(disposing);
    }

    private void PreviewPanel_Paint(object? sender, PaintEventArgs e)
    {
        if (_screen == null) return;

        // Get the screen's actual client size for proper aspect ratio
        var screenForm = _screen as Form;
        int designWidth = 1920;
        int designHeight = 1080;
        
        if (screenForm != null && screenForm.ClientSize.Width > 0 && screenForm.ClientSize.Height > 0)
        {
            designWidth = screenForm.ClientSize.Width;
            designHeight = screenForm.ClientSize.Height;
        }

        // Regenerate cached bitmap if needed
        if (_isCacheDirty || _cachedScreenBitmap == null || 
            _cachedScreenBitmap.Width != designWidth || _cachedScreenBitmap.Height != designHeight)
        {
            // Dispose old cached bitmap if size changed
            if (_cachedScreenBitmap != null && 
                (_cachedScreenBitmap.Width != designWidth || _cachedScreenBitmap.Height != designHeight))
            {
                _cachedScreenBitmap.Dispose();
                _cachedScreenBitmap = null;
            }

            // Create new cached bitmap if needed
            if (_cachedScreenBitmap == null)
            {
                _cachedScreenBitmap = new Bitmap(designWidth, designHeight);
            }

            // Render screen to cached bitmap
            using (var g = System.Drawing.Graphics.FromImage(_cachedScreenBitmap))
            {
                g.Clear(Color.Black);
                
                // Set up high-quality rendering
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                // Call the screen's protected RenderScreen method via reflection
                var renderMethod = _screen.GetType().GetMethod("RenderScreen", 
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                renderMethod?.Invoke(_screen, new object[] { g });
            }

            _isCacheDirty = false;
        }

        // Calculate scaling to fit panel while maintaining aspect ratio
        float scaleX = (float)Width / designWidth;
        float scaleY = (float)Height / designHeight;
        float scale = Math.Min(scaleX, scaleY);
        
        int scaledWidth = (int)(designWidth * scale);
        int scaledHeight = (int)(designHeight * scale);
        int x = (Width - scaledWidth) / 2;
        int y = (Height - scaledHeight) / 2;

        // Scale and draw the cached bitmap to fit the panel with proper aspect ratio
        e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        e.Graphics.DrawImage(_cachedScreenBitmap, x, y, scaledWidth, scaledHeight);
    }

    // Trigger repaint when screen updates
    public void RefreshScreen()
    {
        InvalidateCache();
    }
}
