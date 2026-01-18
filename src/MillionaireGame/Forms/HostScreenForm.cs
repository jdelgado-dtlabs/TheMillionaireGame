using MillionaireGame.Core.Models;
using MillionaireGame.Services;
using MillionaireGame.Core.Helpers;
using MillionaireGame.Graphics;
using MillionaireGame.Core.Services;
using MillionaireGame.Core.Graphics;
using MillionaireGame.Utilities;
using MillionaireGame.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using MillionaireGame.Utilities;

namespace MillionaireGame.Forms;

/// <summary>
/// Host screen with scalable rendering - shows correct answers and ATA percentages
/// </summary>
public class HostScreenForm : ScalableScreenBase, IGameScreen
{
    private Question? _currentQuestion;
    private string? _selectedAnswer;
    private string? _correctAnswer;
    private bool _isRevealing;
    private bool _showATA;
    private Dictionary<string, int> _ataVotes = new();
    private HashSet<string> _visibleAnswers = new();
    private int _currentMoneyTreeLevel = 0;
    private MoneyTreeService? _moneyTreeService;
    private CompleteTheme? _activeTheme; // Active theme for strap rendering
    private SvgStrapRenderer? _svgStrapRenderer; // SVG strap renderer
    private SvgMoneyTreeRenderer? _svgMoneyTreeRenderer; // SVG money tree renderer
    private bool _useSafetyNetAltGraphic = false; // Track if we should use alternate lock-in graphic
    private bool _isSafetyNetFlashing = false; // True while safety net flash animation is active
    private GameMode _currentGameMode = GameMode.Normal; // Track current game mode for money tree rendering
    
    /// <summary>
    /// Gets or sets whether this screen is a preview instance.
    /// Preview screens skip intensive animations like confetti.
    /// </summary>
    public bool IsPreview { get; set; } = false;
    
    // PAF timer display
    private bool _showPAFTimer = false;
    private int _pafSecondsRemaining = 0;
    private string _pafStage = "";
    
    // ATA timer display
    private bool _showATATimer = false;
    private int _ataSecondsRemaining = 0;
    private string _ataStage = "";
    
    // Lifeline icon display
    private bool _showLifelineIcons = false;
    private Dictionary<int, LifelineIconState> _lifelineStates = new();
    private Dictionary<int, LifelineType> _lifelineTypes = new();
    
    // Host message display
    private string? _hostMessage = null;
    private bool _showHostMessage = false;

    // Design-time coordinates (based on 1920x1080, matching TV screen layout)
    // Question in upper part of lower third
    private readonly RectangleF _questionStrapBounds = new(0, 650, 1920, 120);
    // Answers with full width boxes and minimal center gap
    private readonly RectangleF _answerABounds = new(0, 800, 950, 100);
    private readonly RectangleF _answerBBounds = new(970, 800, 950, 100);
    private readonly RectangleF _answerCBounds = new(0, 920, 950, 100);
    private readonly RectangleF _answerDBounds = new(970, 920, 950, 100);
    
    // ATA percentages display area (shown to host)
    private readonly RectangleF _ataDisplayBounds = new(250, 500, 720, 200);

    public HostScreenForm()
    {
        IconHelper.ApplyToForm(this);
        
        // Start at 50% of screen resolution with borders
        var screen = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1920, 1080);
        int width = screen.Width / 2;
        int height = screen.Height / 2;
        Size = new Size(width, height);
        
        // Center on screen
        StartPosition = FormStartPosition.CenterScreen;
        
        // Enable borders and window controls
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimizeBox = true;
        
        Text = "Host Screen";
        BackColor = Color.Black;
        
        // Save initial state for fullscreen toggle
        SaveWindowState();
        
        // Enable ESC key to close
        KeyPreview = true;
        KeyDown += (s, e) => {
            if (e.KeyCode == Keys.Escape)
            {
                Close();
                e.Handled = true;
            }
        };
    }

    public void Initialize(MoneyTreeService moneyTreeService)
    {
        _moneyTreeService = moneyTreeService;
        
        // Load active theme for strap rendering
        _ = Task.Run(async () =>
        {
            try
            {
                var settingsManager = Program.ServiceProvider?.GetRequiredService<ApplicationSettingsManager>();
                if (settingsManager != null)
                {
                    var themeService = new ThemeService(settingsManager.ConnectionString);
                    await themeService.LoadActiveThemeAsync();
                    var activeTheme = themeService.CurrentTheme;
                    
                    if (activeTheme != null)
                    {
                        _activeTheme = await themeService.GetCompleteThemeAsync(activeTheme.ThemeId);
                        _svgStrapRenderer = new SvgStrapRenderer();
                        _svgMoneyTreeRenderer = new SvgMoneyTreeRenderer();
                        GameConsole.Info($"[HostScreenForm] Theme '{_activeTheme.Theme.ThemeName}' loaded for strap and money tree rendering");
                        Invalidate(); // Redraw with themed straps
                    }
                }
            }
            catch (Exception ex)
            {
                GameConsole.Warn($"[HostScreenForm] ThemeService not available: {ex.Message}");
                // Continue without theme service - will fall back to PNG straps
            }
        });
    }

    public void UpdateMoneyTreeLevel(int level, GameMode mode = GameMode.Normal)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => UpdateMoneyTreeLevel(level, mode)));
            return;
        }
        
        _currentMoneyTreeLevel = level;
        _currentGameMode = mode; // Store game mode for rendering
        _useSafetyNetAltGraphic = false; // Reset to normal graphic
        _isSafetyNetFlashing = false; // Not flashing anymore
        Refresh(); // Force immediate redraw to update money tree
    }
    
    /// <summary>
    /// Updates money tree with safety net lock-in flash animation
    /// </summary>
    public void UpdateMoneyTreeWithSafetyNetFlash(int safetyNetLevel, bool flashState)
    {
        _currentMoneyTreeLevel = safetyNetLevel;
        _useSafetyNetAltGraphic = flashState; // true = use alternate graphic, false = use regular
        _isSafetyNetFlashing = true; // We're in flashing mode while this method is called repeatedly
        Invalidate(); // Redraw to show flash state
    }

    protected override void RenderScreen(System.Drawing.Graphics g)
    {
        // Draw graphical money tree in right 1/4 width and upper 2/3 height (always visible)
        DrawMoneyTreeGraphical(g);
        
        // Always draw question strap (with or without text)
        DrawQuestionStrap(g);
        
        // Draw lifeline icons if visible (always check, even without a question)
        if (_showLifelineIcons)
        {
            DrawLifelineIcons(g);
        }
        
        // Draw explanation if question has one
        if (_currentQuestion != null && !string.IsNullOrEmpty(_currentQuestion.Explanation))
        {
            DrawExplanation(g);
        }
        
        // Draw host message if visible (always draw, regardless of question state)
        if (_showHostMessage && !string.IsNullOrEmpty(_hostMessage))
        {
            DrawHostMessage(g);
        }

        // If no question loaded yet, still draw empty answer backgrounds
        if (_currentQuestion == null)
        {
            DrawAnswerBox(g, "A", "", _answerABounds, true, false);
            DrawAnswerBox(g, "B", "", _answerBBounds, false, false);
            DrawAnswerBox(g, "C", "", _answerCBounds, true, false);
            DrawAnswerBox(g, "D", "", _answerDBounds, false, false);
            return;
        }

        // Always draw answer backgrounds, showing visible answers
        // Use custom labels if provided (for FFF reveal), otherwise default to A, B, C, D
        DrawAnswerBox(g, _currentQuestion.AnswerALabel ?? "A", _currentQuestion.AnswerA, _answerABounds, true, _visibleAnswers.Contains("A"));
        DrawAnswerBox(g, _currentQuestion.AnswerBLabel ?? "B", _currentQuestion.AnswerB, _answerBBounds, false, _visibleAnswers.Contains("B"));
        DrawAnswerBox(g, _currentQuestion.AnswerCLabel ?? "C", _currentQuestion.AnswerC, _answerCBounds, true, _visibleAnswers.Contains("C"));
        DrawAnswerBox(g, _currentQuestion.AnswerDLabel ?? "D", _currentQuestion.AnswerD, _answerDBounds, false, _visibleAnswers.Contains("D"));

        // Draw ATA results if active
        if (_showATA)
        {
            DrawATAResults(g);
        }

        // Draw PAF timer if active
        if (_showPAFTimer)
        {
            DrawPAFTimer(g);
        }

        // Draw ATA timer if active
        if (_showATATimer)
        {
            DrawATATimer(g);
        }
        
        // Draw lifeline icons if visible
        if (_showLifelineIcons)
        {
            DrawLifelineIcons(g);
        }
    }

    private void DrawQuestionStrap(System.Drawing.Graphics g)
    {
        // Check if theme is active and has question strap
        if (_activeTheme != null && _svgStrapRenderer != null)
        {
            var questionStrap = _activeTheme.Straps.FirstOrDefault(s => s.StrapType == "Question");
            if (questionStrap != null)
            {
                // Scale bounds from design resolution to actual screen size
                var scaledBounds = ScaleRect(
                    _questionStrapBounds.X, 
                    _questionStrapBounds.Y,
                    _questionStrapBounds.Width,
                    _questionStrapBounds.Height);
                
                var bounds = new Rectangle(
                    (int)scaledBounds.X, 
                    (int)scaledBounds.Y,
                    (int)scaledBounds.Width,
                    (int)scaledBounds.Height);
                
                // Render strap shape without text (we'll draw text separately for better control)
                _svgStrapRenderer.RenderStrapToGraphics(g, questionStrap, "", bounds);
                
                // Draw question text using theme font settings
                var textBounds = new RectangleF(
                    _questionStrapBounds.X + 180, 
                    _questionStrapBounds.Y + 15,
                    _questionStrapBounds.Width - 360, 
                    _questionStrapBounds.Height - 30);
                
                var fontStyle = questionStrap.FontBold ? FontStyle.Bold : FontStyle.Regular;
                var fontColor = ColorTranslator.FromHtml(questionStrap.FontColor);
                    
                DrawScaledTextWithWrap(g, _currentQuestion?.QuestionText ?? "", 
                    questionStrap.FontFamily, questionStrap.FontSize, fontStyle, fontColor, textBounds, 2);
            }
        }
    }



    private void DrawAnswerBox(System.Drawing.Graphics g, string letter, string text, RectangleF bounds, bool isLeftSide, bool isVisible)
    {
        // Render answer strap with theme if available
        if (_activeTheme != null && _svgStrapRenderer != null)
        {
            var answerStrap = _activeTheme.Straps.FirstOrDefault(s => s.StrapType == "Answer");
            var answerLabelStrap = _activeTheme.Straps.FirstOrDefault(s => s.StrapType == "AnswerLabel");
            if (answerStrap != null)
            {
                // Clone the strap to modify colors based on state
                var strapToRender = new ThemeStrap
                {
                    StrapType = answerStrap.StrapType,
                    SvgShape = answerStrap.SvgShape,
                    PrimaryColor = answerStrap.PrimaryColor,
                    SecondaryColor = answerStrap.SecondaryColor,
                    GradientEnabled = answerStrap.GradientEnabled,
                    GradientAngle = answerStrap.GradientAngle,
                    EffectType = answerStrap.EffectType,
                    EffectIntensity = answerStrap.EffectIntensity,
                    EffectColor = answerStrap.EffectColor,
                    BorderEnabled = answerStrap.BorderEnabled,
                    BorderColor = answerStrap.BorderColor,
                    BorderWidth = answerStrap.BorderWidth,
                    BorderStyle = answerStrap.BorderStyle,
                    FontFamily = answerStrap.FontFamily,
                    FontSize = answerStrap.FontSize,
                    FontColor = answerStrap.FontColor,
                    FontBold = answerStrap.FontBold,
                    FontItalic = answerStrap.FontItalic,
                    AnimationEnabled = answerStrap.AnimationEnabled,
                    AnimationType = answerStrap.AnimationType,
                    AnimationDuration = answerStrap.AnimationDuration
                };
                
                // Modify colors based on answer state
                if (_isRevealing && letter == _correctAnswer)
                {
                    // Correct answer - green
                    strapToRender.PrimaryColor = "#228B22"; // Forest green
                    strapToRender.SecondaryColor = "#90EE90"; // Light green
                }
                else if (_isRevealing && _selectedAnswer == letter && letter != _correctAnswer)
                {
                    // Wrong answer - red
                    strapToRender.PrimaryColor = "#8B0000"; // Dark red
                    strapToRender.SecondaryColor = "#FF6347"; // Tomato red
                }
                else if (_selectedAnswer == letter && !_isRevealing)
                {
                    // Selected answer (before reveal) - orange/gold
                    strapToRender.PrimaryColor = "#FF8C00"; // Dark orange
                    strapToRender.SecondaryColor = "#FFD700"; // Gold
                }
                
                // Scale bounds from design resolution to actual screen size
                var scaledBounds = ScaleRect(
                    bounds.X, 
                    bounds.Y,
                    bounds.Width,
                    bounds.Height);
                
                // Render with SVG
                var renderBounds = new Rectangle(
                    (int)scaledBounds.X, 
                    (int)scaledBounds.Y,
                    (int)scaledBounds.Width,
                    (int)scaledBounds.Height);
                
                // Render strap shape without text (we'll draw text separately for better control)
                _svgStrapRenderer.RenderStrapToGraphics(g, strapToRender, "", renderBounds);
                
                // Draw text with original positioning if answer is revealed
                if (isVisible)
                {
                    // Balanced padding for all answers
                    float letterLeftPadding = 60;
                    float textLeftPadding = 150;
                    float textRightPadding = 80;
                    
                    // Get font settings from theme
                    var fontStyle = answerStrap.FontBold ? FontStyle.Bold : FontStyle.Regular;
                    var fontColor = ColorTranslator.FromHtml(answerStrap.FontColor);
                    
                    // Get label font settings from AnswerLabel strap (or fallback to Answer strap)
                    var labelStrap = answerLabelStrap ?? answerStrap;
                    var labelFontStyle = labelStrap.FontBold ? FontStyle.Bold : FontStyle.Regular;
                    var labelFontColor = ColorTranslator.FromHtml(labelStrap.FontColor);
                    
                    // Draw answer letter using AnswerLabel strap font
                    using var letterFont = new Font(labelStrap.FontFamily, labelStrap.FontSize, labelFontStyle);
                    using var letterBrush = new SolidBrush(labelFontColor);
                    using var letterFormat = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    
                    DrawScaledText(g, letter + ":", letterFont, letterBrush,
                        bounds.X + letterLeftPadding, bounds.Y + 15,
                        80, bounds.Height - 30,
                        letterFormat);

                    // Draw answer text with wrapping and auto-scaling using theme font
                    var textBounds = new RectangleF(
                        bounds.X + textLeftPadding, 
                        bounds.Y + 15,
                        bounds.Width - textLeftPadding - textRightPadding, 
                        bounds.Height - 30);
                        
                    DrawScaledTextWithWrap(g, text, 
                        answerStrap.FontFamily, answerStrap.FontSize, fontStyle, fontColor, textBounds, 2, 
                        StringAlignment.Near);
                }
            }
        }
    }

    private void DrawScaledTextWithWrap(System.Drawing.Graphics g, string text, 
        string fontFamily, float baseFontSize, FontStyle fontStyle, Color color, 
        RectangleF bounds, int maxLines, StringAlignment alignment = StringAlignment.Center)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        var scaledBounds = ScaleRect(bounds.X, bounds.Y, bounds.Width, bounds.Height);
        float fontSize = baseFontSize * ScaleX;

        using var brush = new SolidBrush(color);
        var format = new StringFormat
        {
            Alignment = alignment,
            LineAlignment = StringAlignment.Center,
            Trimming = StringTrimming.Word
        };

        // Try progressively smaller font sizes until text fits within maxLines
        for (float testSize = fontSize; testSize >= fontSize * 0.5f; testSize -= fontSize * 0.05f)
        {
            using var testFont = new Font(fontFamily, testSize, fontStyle);
            var measuredSize = g.MeasureString(text, testFont, (int)scaledBounds.Width, format);
            var lineHeight = testFont.GetHeight(g);
            var estimatedLines = Math.Ceiling(measuredSize.Height / lineHeight);

            if (estimatedLines <= maxLines)
            {
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                g.DrawString(text, testFont, brush, scaledBounds, format);
                return;
            }
        }

        // Fallback: draw with smallest tested size
        using var fallbackFont = new Font(fontFamily, fontSize * 0.5f, fontStyle);
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
        g.DrawString(text, fallbackFont, brush, scaledBounds, format);
    }

    private void DrawMoneyTreeGraphical(System.Drawing.Graphics g)
    {
        if (_moneyTreeService == null) return;

        // Money tree occupies right portion of screen
        float treeHeight = 650;
        float treeWidth = treeHeight * (630f / 720f); // Maintain aspect ratio: ~569px
        float treeX = 1920 - treeWidth; // Right edge
        float treeY = 0; // Top of screen

        // Render with SVG if theme is available
        if (_activeTheme?.MoneyTree != null && _svgMoneyTreeRenderer != null)
        {
            var moneyTree = _activeTheme.MoneyTree;
            var settings = _moneyTreeService.Settings;

            // Scale bounds to actual screen size
            var designBounds = new Rectangle(
                (int)treeX,
                (int)treeY,
                (int)treeWidth,
                (int)treeHeight);
            
            var scaledBounds = new Rectangle(
                (int)(designBounds.X * ScaleX),
                (int)(designBounds.Y * ScaleY),
                (int)(designBounds.Width * ScaleX),
                (int)(designBounds.Height * ScaleY));

            // Get shape type from Question strap (or default to Classic)
            string shapeType = _activeTheme.Straps
                ?.FirstOrDefault(s => s.StrapType == "Question")
                ?.SvgShape ?? "Classic";

            // Render SVG money tree ladder
            _svgMoneyTreeRenderer.RenderMoneyTreeToGraphics(
                g,
                moneyTree,
                scaledBounds,
                _currentMoneyTreeLevel,
                settings.SafetyNet1,
                settings.SafetyNet2,
                _currentGameMode == GameMode.Risk,
                _useSafetyNetAltGraphic,
                shapeType,
                _isSafetyNetFlashing);

            // Draw money values and question numbers
            DrawMoneyTreeText(g, treeX, treeY, treeWidth, treeHeight);
        }
        else
        {
            // Fallback: PNG textures
            var treeBase = TextureManager.GetTexture(TextureManager.ElementType.MoneyTreeBase, CurrentTextureSet);
            var treePosition = _useSafetyNetAltGraphic 
                ? TextureManager.Instance.GetMoneyTreePositionLockAlt(_currentMoneyTreeLevel)
                : TextureManager.Instance.GetMoneyTreePosition(_currentMoneyTreeLevel);

            if (treeBase == null) return;

            DrawScaledImage(g, treeBase, treeX, treeY, treeWidth, treeHeight);

            if (treePosition != null && _currentMoneyTreeLevel > 0)
            {
                float scale = treeHeight / 720f;
                float overlayX = treeX + (165 * scale);
                float overlayY = treeY + (100 * scale);
                float overlayWidth = 399 * scale;
                float overlayHeight = 599 * scale;
                
                DrawScaledImage(g, treePosition, overlayX, overlayY, overlayWidth, overlayHeight);
            }

            DrawMoneyTreeText(g, treeX, treeY, treeWidth, treeHeight);
        }
    }

    private void DrawMoneyTreeText(System.Drawing.Graphics g, float baseX, float baseY, float width, float height)
    {
        var settings = _moneyTreeService!.Settings;
        var moneyTree = _activeTheme?.MoneyTree;
        
        // Calculate level dimensions to match renderer
        const int levelCount = 15;
        float levelHeight = height / (float)levelCount;
        float leftMargin = 20;
        
        // Font: Use theme font if available, otherwise fallback
        string fontFamily = moneyTree?.FontFamily ?? "Copperplate Gothic Bold";
        float baseFontSize = (moneyTree?.FontSize ?? 18);
        // Use non-bold font for money tree text for improved readability
        FontStyle fontStyle = FontStyle.Regular;
        
        for (int level = 15; level >= 1; level--) // Draw from top (15) to bottom (1)
        {
            // Calculate Y position to match renderer (level 1 at bottom)
            float levelY = baseY + height - (level * levelHeight);
            float yCenter = levelY + (levelHeight / 2f);
            
            // X positions - use proportions of available width to match TV layout
            float qnoX = baseX + (width * 0.2786f);
            if (level >= 10) qnoX = baseX + (width * 0.252f);
            // X position for money value
            float moneyX = baseX + (width * 0.4275f);
            
            // Determine text color based on level state
            Color textColor;
            if (level == _currentMoneyTreeLevel)
            {
                // Current level - use theme ActiveColor if available
                if (moneyTree != null)
                {
                    textColor = ColorTranslator.FromHtml(moneyTree.ActiveColor);
                }
                else
                {
                    textColor = _useSafetyNetAltGraphic ? Color.White : Color.Black;
                }
            }
            else if (level == 15)
            {
                textColor = Color.White; // Q15 is always white (million dollar question)
            }
            else if (level == 5 || level == 10)
            {
                bool isQ5Enabled = (settings.SafetyNet1 == 5 || settings.SafetyNet2 == 5);
                bool isQ10Enabled = (settings.SafetyNet1 == 10 || settings.SafetyNet2 == 10);
                bool isRiskMode = _currentGameMode == GameMode.Risk;
                
                if (level == 5)
                    textColor = (isQ5Enabled && !isRiskMode) ? Color.White : Color.Gold;
                else // level == 10
                    textColor = (isQ10Enabled && !isRiskMode) ? Color.White : Color.Gold;
            }
            else if (level == settings.SafetyNet1 || level == settings.SafetyNet2)
            {
                // Custom safety nets - use theme SafeHavenColor or fallback
                if (moneyTree != null && !(_currentGameMode == GameMode.Risk))
                {
                    textColor = ColorTranslator.FromHtml(moneyTree.SafeHavenColor);
                }
                else
                {
                    textColor = (_currentGameMode == GameMode.Risk) ? Color.Gold : Color.White;
                }
            }
            else
            {
                // Regular levels - use theme InactiveColor or Gold fallback
                textColor = moneyTree != null ? ColorTranslator.FromHtml(moneyTree.InactiveColor) : Color.Gold;
            }
            
            using var font = new Font(fontFamily, baseFontSize, fontStyle);
            using var format = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
            
            // Compute design Y and height to center text on rung using row center
            float designY = yCenter - (levelHeight / 2f);
            float designHeight = levelHeight;

            // No per-form inset: centering is handled by DrawScaledTextWithOutline using typographic measurement

            // Draw question number with outline (centered vertically on rung)
            DrawScaledTextWithOutline(g, level.ToString(), font, textColor, qnoX, designY, 100, designHeight, format, 2);

            // Draw money amount with outline (centered vertically on rung)
            string formattedMoney = _moneyTreeService.GetFormattedValue(level);
            DrawScaledTextWithOutline(g, formattedMoney, font, textColor, moneyX, designY, 350, designHeight, format, 2);
        }
    }

    private void DrawATAPreview(System.Drawing.Graphics g)
    {
        if (_currentQuestion == null) return;

        var previewBounds = new RectangleF(250, 500, 400, 180);
        var scaledBounds = ScaleRect(previewBounds.X, previewBounds.Y, previewBounds.Width, previewBounds.Height);

        // Semi-transparent background
        using var brush = new SolidBrush(Color.FromArgb(180, 20, 20, 40));
        g.FillRectangle(brush, scaledBounds);

        // Draw title
        var titleBounds = new RectangleF(previewBounds.X, previewBounds.Y + 10, previewBounds.Width, 40);
        using var titleFont = new Font("Arial", 28, FontStyle.Bold);
        using var titleBrush = new SolidBrush(Color.Yellow);
        using var titleFormat = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        DrawScaledText(g, "ATA Preview", titleFont, titleBrush,
            titleBounds.X, titleBounds.Y, titleBounds.Width, titleBounds.Height, titleFormat);

        // Draw percentages
        float yOffset = 60;
        var lineHeight = 30f;
        
        // Generate preview percentages
        var previewPercentages = _currentQuestion.GenerateATAPercentages();
        
        DrawATALine(g, "A", previewPercentages["A"], new RectangleF(previewBounds.X + 20, previewBounds.Y + yOffset, previewBounds.Width - 40, lineHeight));
        yOffset += lineHeight;
        DrawATALine(g, "B", previewPercentages["B"], new RectangleF(previewBounds.X + 20, previewBounds.Y + yOffset, previewBounds.Width - 40, lineHeight));
        yOffset += lineHeight;
        DrawATALine(g, "C", previewPercentages["C"], new RectangleF(previewBounds.X + 20, previewBounds.Y + yOffset, previewBounds.Width - 40, lineHeight));
        yOffset += lineHeight;
        DrawATALine(g, "D", previewPercentages["D"], new RectangleF(previewBounds.X + 20, previewBounds.Y + yOffset, previewBounds.Width - 40, lineHeight));
    }

    private void DrawATALine(System.Drawing.Graphics g, string answer, int percentage, RectangleF bounds)
    {
        var text = $"{answer}: {percentage}%";
        using var font = new Font("Arial", 24, FontStyle.Regular);
        using var brush = new SolidBrush(Color.White);
        DrawScaledText(g, text, font, brush,
            bounds.X, bounds.Y, bounds.Width, bounds.Height);
    }

    private void DrawATAResults(System.Drawing.Graphics g)
    {
        if (_ataVotes.Count == 0) return;

        // Position centered horizontally, below lifeline icons
        // Lifelines at (680, 18) with height ~78, so start at y=150
        var overlayBounds = new RectangleF(635, 150, 650, 400);
        var scaledBounds = ScaleRect(overlayBounds.X, overlayBounds.Y, overlayBounds.Width, overlayBounds.Height);

        // Semi-transparent background
        using var brush = new SolidBrush(Color.FromArgb(200, 0, 0, 0));
        g.FillRectangle(brush, scaledBounds);

        // Draw title
        var titleBounds = new RectangleF(overlayBounds.X, overlayBounds.Y + 20, overlayBounds.Width, 60);
        using var titleFont = new Font("Arial", 48, FontStyle.Bold);
        using var titleBrush = new SolidBrush(Color.Yellow);
        using var titleFormat = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        DrawScaledText(g, "Ask the Audience", titleFont, titleBrush,
            titleBounds.X, titleBounds.Y, titleBounds.Width, titleBounds.Height, titleFormat);

        // Draw vote bars
        float yOffset = 100;
        foreach (var kvp in _ataVotes.OrderBy(x => x.Key))
        {
            var barBounds = new RectangleF(overlayBounds.X + 50, overlayBounds.Y + yOffset, 620, 60);
            DrawVoteBar(g, kvp.Key, kvp.Value, barBounds);
            yOffset += 80;
        }
    }

    private void DrawVoteBar(System.Drawing.Graphics g, string answer, int percentage, RectangleF bounds)
    {
        var scaledBounds = ScaleRect(bounds.X, bounds.Y, bounds.Width, bounds.Height);
        
        // Background
        using var bgBrush = new SolidBrush(Color.FromArgb(100, 100, 100));
        g.FillRectangle(bgBrush, scaledBounds);

        // Percentage bar
        float barWidth = scaledBounds.Width * (percentage / 100f);
        using var barBrush = new SolidBrush(Color.FromArgb(0, 120, 215));
        g.FillRectangle(barBrush, scaledBounds.X, scaledBounds.Y, barWidth, scaledBounds.Height);

        // Text
        var text = $"{answer}: {percentage}%";
        using var font = new Font("Arial", 32, FontStyle.Bold);
        using var textBrush = new SolidBrush(Color.White);
        DrawScaledText(g, text, font, textBrush,
            bounds.X, bounds.Y, bounds.Width, bounds.Height);
    }

    private void DrawPAFTimer(System.Drawing.Graphics g)
    {
        // Define timer display bounds - upper left area to avoid overlap
        var designTimerBounds = new RectangleF(50, 50, 300, 150);
        
        // Scale to actual screen coordinates
        var actualBounds = new RectangleF(
            designTimerBounds.X * ScaleX,
            designTimerBounds.Y * ScaleY,
            designTimerBounds.Width * ScaleX,
            designTimerBounds.Height * ScaleY
        );
        
        // Background box
        using var bgBrush = new SolidBrush(Color.FromArgb(200, 0, 0, 0)); // Semi-transparent black
        g.FillRectangle(bgBrush, actualBounds);
        
        // Border
        using var borderPen = new Pen(_pafStage == "Calling" ? Color.DodgerBlue : Color.OrangeRed, 3);
        g.DrawRectangle(borderPen, actualBounds.X, actualBounds.Y, actualBounds.Width, actualBounds.Height);
        
        // Text content
        string displayText = _pafStage == "Calling" ? "Calling..." : $"{_pafSecondsRemaining}";
        using var font = new Font("Arial", _pafStage == "Calling" ? 28 : 60, FontStyle.Bold);
        using var textBrush = new SolidBrush(Color.White);
        
        // Center text in bounds
        DrawScaledText(g, displayText, font, textBrush,
            designTimerBounds.X, designTimerBounds.Y, designTimerBounds.Width, designTimerBounds.Height);
    }

    private void DrawATATimer(System.Drawing.Graphics g)
    {
        // Define timer display bounds - upper left area below PAF timer
        var designTimerBounds = new RectangleF(50, 220, 300, 150);
        
        // Scale to actual screen coordinates
        var actualBounds = new RectangleF(
            designTimerBounds.X * ScaleX,
            designTimerBounds.Y * ScaleY,
            designTimerBounds.Width * ScaleX,
            designTimerBounds.Height * ScaleY
        );
        
        // Background box
        using var bgBrush = new SolidBrush(Color.FromArgb(200, 0, 0, 0)); // Semi-transparent black
        g.FillRectangle(bgBrush, actualBounds);
        
        // Border  
        using var borderPen = new Pen(_ataStage == "Intro" ? Color.DodgerBlue : Color.OrangeRed, 3);
        g.DrawRectangle(borderPen, actualBounds.X, actualBounds.Y, actualBounds.Width, actualBounds.Height);
        
        // Format time as MM:SS
        int minutes = _ataSecondsRemaining / 60;
        int seconds = _ataSecondsRemaining % 60;
        string displayText = $"{minutes}:{seconds:D2}";
        
        using var font = new Font("Arial", 60, FontStyle.Bold);
        using var textBrush = new SolidBrush(Color.White);
        
        // Center text in bounds
        DrawScaledText(g, displayText, font, textBrush,
            designTimerBounds.X, designTimerBounds.Y, designTimerBounds.Width, designTimerBounds.Height);
    }
    
    private void DrawExplanation(System.Drawing.Graphics g)
    {
        if (_currentQuestion == null || string.IsNullOrEmpty(_currentQuestion.Explanation))
            return;
            
        // Design coordinates - above host message box
        // Position above host message (Y=570), with enough space for 2 lines
        const float designX = 180; // Match question text left padding
        const float designY = 490; // Above host message box (570 - 80 for space)
        const float designMaxWidth = 1100; // Same width as host message box
        const float designPadding = 20;
        const float designHeight = 70; // Enough for 2 lines of text
        
        // Scale to actual screen coordinates
        var actualBounds = new RectangleF(
            designX * ScaleX,
            designY * ScaleY,
            designMaxWidth * ScaleX,
            designHeight * ScaleY
        );
        
        // Semi-transparent background (70% opacity black)
        using var bgBrush = new SolidBrush(Color.FromArgb(178, 0, 0, 0));
        g.FillRectangle(bgBrush, actualBounds);
        
        // Border with accent color
        using var borderPen = new Pen(Color.FromArgb(255, 70, 130, 180), 3); // Steel blue
        g.DrawRectangle(borderPen, actualBounds.X, actualBounds.Y, actualBounds.Width, actualBounds.Height);
        
        // Draw explanation text with wrapping
        var textBounds = new RectangleF(
            actualBounds.X + (designPadding * ScaleX),
            actualBounds.Y + (designPadding * ScaleY),
            actualBounds.Width - (designPadding * 2 * ScaleX),
            actualBounds.Height - (designPadding * 2 * ScaleY)
        );
        
        using var textBrush = new SolidBrush(Color.White);
        using var textFont = new Font("Arial", 16, FontStyle.Bold);
        var scaledFont = new Font(textFont.FontFamily, textFont.Size * Math.Min(ScaleX, ScaleY), textFont.Style, textFont.Unit);
        using var format = new StringFormat
        {
            Alignment = StringAlignment.Near,
            LineAlignment = StringAlignment.Near,
            Trimming = StringTrimming.Word
        };
        
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
        g.DrawString(_currentQuestion.Explanation, scaledFont, textBrush, textBounds, format);
        
        scaledFont.Dispose();
    }
    
    private void DrawHostMessage(System.Drawing.Graphics g)
    {
        if (string.IsNullOrEmpty(_hostMessage))
            return;
            
        // Design coordinates - just above question strap (Y=650), aligned left
        // Position above question strap with some margin, ending before money tree
        const float designX = 180; // Match question text left padding
        const float designY = 570; // Just above question strap (650 - 80)
        const float designMaxWidth = 1100; // End before money tree at ~X=1351 (1280 max - 180 start)
        const float designPadding = 20;
        const float designMinHeight = 60;
        
        // Calculate actual text size to determine box height
        using var measureFont = new Font("Arial", 16, FontStyle.Bold);
        var scaledMeasureFont = new Font(measureFont.FontFamily, measureFont.Size * Math.Min(ScaleX, ScaleY), measureFont.Style, measureFont.Unit);
        var maxWidth = designMaxWidth * ScaleX - (designPadding * 2 * ScaleX);
        var textSize = g.MeasureString(_hostMessage, scaledMeasureFont, (int)maxWidth);
        
        // Calculate box height with padding
        float designHeight = Math.Max(designMinHeight, (textSize.Height / ScaleY) + (designPadding * 2));
        
        // Scale to actual screen coordinates
        var actualBounds = new RectangleF(
            designX * ScaleX,
            designY * ScaleY,
            designMaxWidth * ScaleX,
            designHeight * ScaleY
        );
        
        // Semi-transparent background (70% opacity black)
        using var bgBrush = new SolidBrush(Color.FromArgb(178, 0, 0, 0));
        g.FillRectangle(bgBrush, actualBounds);
        
        // Border with accent color
        using var borderPen = new Pen(Color.FromArgb(255, 70, 130, 180), 3); // Steel blue
        g.DrawRectangle(borderPen, actualBounds.X, actualBounds.Y, actualBounds.Width, actualBounds.Height);
        
        // Draw message text with wrapping
        var textBounds = new RectangleF(
            actualBounds.X + (designPadding * ScaleX),
            actualBounds.Y + (designPadding * ScaleY),
            actualBounds.Width - (designPadding * 2 * ScaleX),
            actualBounds.Height - (designPadding * 2 * ScaleY)
        );
        
        using var textBrush = new SolidBrush(Color.White);
        using var format = new StringFormat
        {
            Alignment = StringAlignment.Near,
            LineAlignment = StringAlignment.Near,
            Trimming = StringTrimming.Word
        };
        
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
        g.DrawString(_hostMessage, scaledMeasureFont, textBrush, textBounds, format);
        
        scaledMeasureFont.Dispose();
    }
    
    private void DrawLifelineIcons(System.Drawing.Graphics g)
    {
        // Design-time coordinates (1920x1080)
        // Position: Upper right area, moved left to avoid money tree (680, 18), spacing 138px, size 129x78
        float baseX = 680;
        float baseY = 18;
        float spacing = 138;
        float iconWidth = 129;
        float iconHeight = 78;
        
        // Draw up to 4 lifeline icons
        for (int i = 1; i <= 4; i++)
        {
            if (!_lifelineTypes.ContainsKey(i) || !_lifelineStates.ContainsKey(i))
                continue;
                
            var type = _lifelineTypes[i];
            var state = _lifelineStates[i];
            
            if (state == LifelineIconState.Hidden)
                continue;
            
            var icon = LifelineIcons.GetLifelineIcon(type, state);
            if (icon != null)
            {
                float x = baseX + ((i - 1) * spacing);
                DrawScaledImage(g, icon, x, baseY, iconWidth, iconHeight);
            }
        }
    }

    // IGameScreen implementation
    public void UpdateQuestion(Question question)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => UpdateQuestion(question)));
            return;
        }

        _currentQuestion = question;
        _selectedAnswer = null;
        _correctAnswer = question.CorrectAnswer.ToString();
        _isRevealing = false;
        _showATA = false;
        _ataVotes.Clear();
        _visibleAnswers.Clear();
        
        Invalidate();
    }

    public void SelectAnswer(string answer)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => SelectAnswer(answer)));
            return;
        }

        _selectedAnswer = answer;
        Invalidate();
    }

    public void RevealAnswer(string selectedAnswer, string correctAnswer, bool isCorrect)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => RevealAnswer(selectedAnswer, correctAnswer, isCorrect)));
            return;
        }

        _selectedAnswer = selectedAnswer;
        _correctAnswer = correctAnswer;
        _isRevealing = true;
        _showATA = false; // Hide ATA results when revealing answer
        Invalidate();
    }

    public void ShowAnswer(string answer)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => ShowAnswer(answer)));
            return;
        }

        _visibleAnswers.Add(answer);
        Invalidate();
    }

    public void RemoveAnswer(string answer)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => RemoveAnswer(answer)));
            return;
        }

        // Remove the answer from visible list - used for Double Dip first wrong attempt
        _visibleAnswers.Remove(answer);
        Invalidate();
    }

    public void ShowCorrectAnswerToHost(string? correctAnswer)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => ShowCorrectAnswerToHost(correctAnswer)));
            return;
        }

        _correctAnswer = correctAnswer;
        _isRevealing = !string.IsNullOrEmpty(correctAnswer);
        Invalidate();
    }

    public void ShowQuestion(bool show)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => ShowQuestion(show)));
            return;
        }

        // Question strap is always visible, just trigger repaint
        Invalidate();
    }

    public void ShowWinnings(GameState state)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => ShowWinnings(state)));
            return;
        }

        // Money tree is always visible, nothing to do
        Invalidate();
    }

    public void HideWinnings()
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(HideWinnings));
            return;
        }

        // Money tree is always visible, nothing to hide
        Invalidate();
    }

    public void UpdateMoney(string current, string correct, string wrong, string drop, string questionsLeft)
    {
        // Host screen shows money tree control instead
    }

    public void ActivateLifeline(Lifeline lifeline)
    {
        // Could show visual effects for lifeline activation
        // For ATA, don't show results yet - wait for ShowATAResults() to be called
        if (lifeline.Type == LifelineType.AskTheAudience)
        {
            // Clear any previous ATA data
            _showATA = false;
            _ataVotes.Clear();
            Invalidate();
        }
    }
    
    public void ShowATAResults(Dictionary<string, int> votes)
    {
        _showATA = true;
        _ataVotes = votes;
        Invalidate();
    }

    public void HideATAResults()
    {
        _showATA = false;
        _ataVotes.Clear();
        Invalidate();
    }

    public void ResetScreen()
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(ResetScreen));
            return;
        }

        _currentQuestion = null;
        _selectedAnswer = null;
        _correctAnswer = null;
        _isRevealing = false;
        _showATA = false;
        _ataVotes.Clear();
        _visibleAnswers.Clear();
        _showPAFTimer = false; // Hide PAF timer on reset
        _showATATimer = false; // Hide ATA timer on reset
        _showLifelineIcons = false; // Hide lifeline icons on reset
        // Straps remain always visible
        Invalidate();
    }

    public void ShowPAFTimer(int secondsRemaining, string stage)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => ShowPAFTimer(secondsRemaining, stage)));
            return;
        }

        _pafSecondsRemaining = secondsRemaining;
        _pafStage = stage;
        _showPAFTimer = stage != "Completed"; // Hide when completed
        Invalidate();
    }

    public void ShowATATimer(int secondsRemaining, string stage)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => ShowATATimer(secondsRemaining, stage)));
            return;
        }

        _ataSecondsRemaining = secondsRemaining;
        _ataStage = stage;
        _showATATimer = stage != "Completed"; // Hide when completed
        Invalidate();
    }

    public void ClearQuestionAndAnswerText()
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(ClearQuestionAndAnswerText));
            return;
        }

        // Clear question and answer text but keep straps visible
        if (_currentQuestion != null)
        {
            _currentQuestion = new Question
            {
                Id = _currentQuestion.Id,
                Level = _currentQuestion.Level,
                QuestionText = string.Empty,
                AnswerA = string.Empty,
                AnswerB = string.Empty,
                AnswerC = string.Empty,
                AnswerD = string.Empty,
                CorrectAnswer = _currentQuestion.CorrectAnswer,
                Note = _currentQuestion.Note
            };
        }
        _visibleAnswers.Clear();
        Invalidate();
    }
    
    public void ShowLifelineIcons()
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(ShowLifelineIcons));
            return;
        }
        
        _showLifelineIcons = true;
        Invalidate();
    }
    
    public void HideLifelineIcons()
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(HideLifelineIcons));
            return;
        }
        
        _showLifelineIcons = false;
        Invalidate();
    }
    
    public void SetLifelineIcon(int lifelineNumber, LifelineType type, LifelineIconState state)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => SetLifelineIcon(lifelineNumber, type, state)));
            return;
        }
        
        _lifelineTypes[lifelineNumber] = type;
        _lifelineStates[lifelineNumber] = state;
        Invalidate();
    }
    
    public void ClearLifelineIcons()
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(ClearLifelineIcons));
            return;
        }
        
        _lifelineTypes.Clear();
        _lifelineStates.Clear();
        _showLifelineIcons = false;
        Invalidate();
    }
    
    // IGameScreen interface stubs - FFF display not used on Host screen (only TVScreenForm)
    public void ShowFFFContestant(int index, string name) { }
    public void ShowAllFFFContestants(List<string> names, List<double>? times = null) { }
    public void HighlightFFFContestant(int index, bool isWinner = false) { }
    public void ShowFFFWinner(string name, double? time = null) { }
    
    #region Host Messaging
    
    /// <summary>
    /// Receives and displays a message from the control panel
    /// </summary>
    public void OnMessageReceived(object? sender, HostMessageEventArgs e)
    {
        var instanceId = this.GetHashCode();
        var stackTrace = new System.Diagnostics.StackTrace(1, true);
        GameConsole.Debug($"[HostScreen:{instanceId}] OnMessageReceived called with message: '{e.Message}' (empty={string.IsNullOrWhiteSpace(e.Message)})");
        GameConsole.Debug($"[HostScreen:{instanceId}] Call stack: {stackTrace.GetFrame(0)?.GetMethod()?.Name}");
        
        if (InvokeRequired)
        {
            BeginInvoke(() => OnMessageReceived(sender, e));
            return;
        }
        
        // Empty message means clear, otherwise show the message
        if (string.IsNullOrWhiteSpace(e.Message))
        {
            _hostMessage = null;
            _showHostMessage = false;
            GameConsole.Debug($"[HostScreen:{instanceId}] Message cleared");
        }
        else
        {
            _hostMessage = e.Message;
            _showHostMessage = true;
            GameConsole.Debug($"[HostScreen:{instanceId}] Message received: {e.Message}, Show: {_showHostMessage}");
        }
        
        GameConsole.Debug($"[HostScreen:{instanceId}] OnMessageReceived complete - about to Invalidate. Current state: Show={_showHostMessage}, Message='{_hostMessage}'");
        Invalidate();
        GameConsole.Debug($"[HostScreen:{instanceId}] Invalidate() completed. Final state: Show={_showHostMessage}, Message='{_hostMessage}'");
    }
    
    /// <summary>
    /// Hides the host message
    /// </summary>
    public void HideHostMessage()
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(HideHostMessage));
            return;
        }
        
        _showHostMessage = false;
        Invalidate();
    }
    
    #endregion
    public void ClearFFFDisplay() { }
    public void ShowGameWinner(string amount, int questionLevel) { }
    public void ShowGameWinner(string combinedAmount, string? currency1Amount, string? currency2Amount, 
        bool hasCurrency1, bool hasCurrency2, int questionLevel) { }
    public void ClearGameWinnerDisplay() { }
}

