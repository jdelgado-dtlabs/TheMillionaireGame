using MillionaireGame.Core.Models;
using MillionaireGame.Services;
using MillionaireGame.Core.Helpers;
using MillionaireGame.Graphics;
using MillionaireGame.Core.Services;
using MillionaireGame.Core.Graphics;
using MillionaireGame.Utilities;
using MillionaireGame.Core.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace MillionaireGame.Forms;

/// <summary>
/// TV screen with scalable rendering
/// </summary>
public class TVScreenForm : ScalableScreenBase, IGameScreen
{
    private Question? _currentQuestion;
    private string? _selectedAnswer;
    private string? _correctAnswer;
    private bool _isRevealing;
    private bool _showATA;
    private Dictionary<string, int> _ataVotes = new();
    private string? _currentAmount;
    private HashSet<string> _visibleAnswers = new();
    private bool _showQuestionAndAnswers = false;
    private bool _showWinnings = false;
    private bool _showMoneyTree = false;
    private bool _moneyTreeAnimating = false;
    private float _moneyTreeAnimationProgress = 0f; // 0.0 to 1.0
    private int _currentMoneyTreeLevel = 0;
    private MoneyTreeService? _moneyTreeService;
    private BackgroundRenderer? _backgroundRenderer; // Broadcast background renderer
    private CompleteTheme? _activeTheme; // Active theme for strap rendering
    private SvgStrapRenderer? _svgStrapRenderer; // SVG strap renderer
    private SvgMoneyTreeRenderer? _svgMoneyTreeRenderer; // SVG money tree renderer
    private const float TvFontScale = 1.6f; // Multiplier to increase strap fonts for large TV displays
    
    /// <summary>
    /// Gets or sets whether this screen is a preview instance.
    /// Preview screens skip intensive animations like confetti.
    /// </summary>
    public bool IsPreview { get; set; } = false;
    private bool _useSafetyNetAltGraphic = false; // Track if we should use alternate lock-in graphic
    private bool _isSafetyNetFlashing = false; // True while safety net flash animation is active
    private GameMode _currentGameMode = GameMode.Normal; // Track current game mode for money tree rendering
    
    // PAF timer display
    private bool _showPAFTimer = false;
    private int _pafSecondsRemaining = 0;
    private string _pafStage = "";
    
    // ATA timer display
    private bool _showATATimer = false;
    private int _ataSecondsRemaining = 0;
    private string _ataStage = "";
    
    // FFF display
    private bool _showFFF = false;
    private List<string> _fffContestants = new();
    private List<double> _fffTimes = new();
    private int _fffHighlightedIndex = -1;
    private bool _fffShowWinner = false;
    private string? _fffWinnerName = null;
    private double? _fffWinnerTime = null;
    
    // Game winner display (Thanks for Playing)
    private bool _showGameWinner = false;
    private string? _gameWinnerAmount = null;
    private string? _gameWinnerCurrency1 = null;
    private string? _gameWinnerCurrency2 = null;
    private bool _gameWinnerHasCurrency1 = false;
    private bool _gameWinnerHasCurrency2 = false;
    private List<ConfettiParticle> _confettiParticles = new();
    private System.Threading.Timer? _confettiTimer;
    
    // Lifeline icon display
    private bool _showLifelineIcons = false;
    private Dictionary<int, LifelineIconState> _lifelineStates = new();
    private Dictionary<int, LifelineType> _lifelineTypes = new();

    // Design-time coordinates (based on 1920x1080, positioned in lower third)
    // Backgrounds are fully edge-to-edge (0 margins)
    // Question in upper part of lower third
    private readonly RectangleF _questionStrapBounds = new(0, 650, 1920, 120);
    // Winnings strap in lower half of lower third (below Y=900)
    private readonly RectangleF _winningsStrapBounds = new(0, 900, 1920, 120);
    // Answers with full width boxes and minimal center gap
    private readonly RectangleF _answerABounds = new(0, 800, 950, 100);
    private readonly RectangleF _answerBBounds = new(970, 800, 950, 100);
    private readonly RectangleF _answerCBounds = new(0, 920, 950, 100);
    private readonly RectangleF _answerDBounds = new(970, 920, 950, 100);

    public TVScreenForm()
    {
        IconHelper.ApplyToForm(this);
        
        // Initialize background renderer with settings
        // Note: Settings should be injected via DI, but for now we'll initialize on first use
        // _backgroundRenderer will be initialized in Initialize() method with proper settings
        
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
        
        Text = "TV Screen (Scalable)";
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

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        // Use broadcast background renderer if available, otherwise fall back to black
        if (_backgroundRenderer != null)
        {
            _backgroundRenderer.RenderBackground(e.Graphics, ClientSize.Width, ClientSize.Height);
        }
        else
        {
            // Fallback: Fill with black background
            e.Graphics.Clear(Color.Black);
        }
    }

    protected override void RenderScreen(System.Drawing.Graphics g)
    {
        // If game winner is showing, render full-screen winner display (takes over entire screen)
        if (_showGameWinner)
        {
            DrawGameWinnerDisplay(g);
            return;
        }
        
        // If FFF is showing, render FFF display (takes over entire screen)
        if (_showFFF)
        {
            DrawFFFDisplay(g);
            return;
        }
        
        // Draw lifeline icons first (if visible) so they appear behind other content
        if (_showLifelineIcons)
        {
            DrawLifelineIcons(g);
        }
        
        // If money tree is showing, render it on top
        if (_showMoneyTree)
        {
            DrawMoneyTreeGraphical(g);
            return;
        }
        
        // Always show winnings if enabled
        if (_showWinnings && !string.IsNullOrEmpty(_currentAmount))
        {
            DrawWinningsDisplay(g);
            return; // Only show winnings, nothing else
        }
        
        // Draw lifeline icons if visible (even without question/answer visibility during explain game)
        if (_showLifelineIcons)
        {
            DrawLifelineIcons(g);
        }
        
        if (!_showQuestionAndAnswers) return;

        // Draw question strap (with or without text)
        DrawQuestionStrap(g);
        if (_showLifelineIcons)
        {
            DrawLifelineIcons(g);
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

        // Always draw answer backgrounds, but only show text for visible answers
        // Use custom labels if provided (for FFF reveal), otherwise default to A, B, C, D
        DrawAnswerBox(g, _currentQuestion.AnswerALabel ?? "A", _currentQuestion.AnswerA, _answerABounds, true, _visibleAnswers.Contains("A"));
        DrawAnswerBox(g, _currentQuestion.AnswerBLabel ?? "B", _currentQuestion.AnswerB, _answerBBounds, false, _visibleAnswers.Contains("B"));
        DrawAnswerBox(g, _currentQuestion.AnswerCLabel ?? "C", _currentQuestion.AnswerC, _answerCBounds, true, _visibleAnswers.Contains("C"));
        DrawAnswerBox(g, _currentQuestion.AnswerDLabel ?? "D", _currentQuestion.AnswerD, _answerDBounds, false, _visibleAnswers.Contains("D"));

        // Draw ATA results if visible
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
                    
                DrawScaledTextWithWrapAndOutline(g, _currentQuestion?.QuestionText ?? "", 
                    questionStrap.FontFamily, questionStrap.FontSize * TvFontScale, fontStyle, fontColor, textBounds, 2);
            }
        }
    }

    private void DrawAnswerBox(System.Drawing.Graphics g, string letter, string text, RectangleF bounds, bool isLeft, bool showText)
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
                if (_isRevealing && _correctAnswer == letter)
                {
                    // Correct answer - green
                    strapToRender.PrimaryColor = "#228B22"; // Forest green
                    strapToRender.SecondaryColor = "#90EE90"; // Light green
                }
                else if (_isRevealing && _selectedAnswer == letter && _selectedAnswer != _correctAnswer)
                {
                    // Wrong answer - red
                    strapToRender.PrimaryColor = "#8B0000"; // Dark red
                    strapToRender.SecondaryColor = "#FF6347"; // Tomato red
                }
                else if (_selectedAnswer == letter)
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
                if (showText)
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
                    
                    // Draw answer letter using AnswerLabel strap font with outline
                    using var letterFont = new Font(labelStrap.FontFamily, labelStrap.FontSize * TvFontScale, labelFontStyle);
                    using var letterFormat = CreateCenteredFormat();
                    
                    DrawScaledTextWithOutline(g, letter + ":", letterFont, labelFontColor,
                        bounds.X + letterLeftPadding, bounds.Y + 15,
                        80, bounds.Height - 30,
                        letterFormat);

                    // Draw answer text with wrapping, auto-scaling, and outline using theme font
                    var textBounds = new RectangleF(
                        bounds.X + textLeftPadding, 
                        bounds.Y + 15,
                        bounds.Width - textLeftPadding - textRightPadding, 
                        bounds.Height - 30);
                        
                    DrawScaledTextWithWrapAndOutline(g, text, 
                        answerStrap.FontFamily, answerStrap.FontSize * TvFontScale, fontStyle, fontColor, textBounds, 2, 
                        StringAlignment.Near);
                }
                
                return;
            }
            else
            {
                GameConsole.Warn($"[TVScreenForm] Theme '{_activeTheme?.Theme.ThemeName}' has no Answer strap configured");
            }
        }
    }

    private void DrawWinningsDisplay(System.Drawing.Graphics g)
    {
        // Render winnings strap with theme if available (uses Question strap styling)
        if (_activeTheme != null && _svgStrapRenderer != null)
        {
            var questionStrap = _activeTheme.Straps.FirstOrDefault(s => s.StrapType == "Question");
            if (questionStrap != null)
            {
                // Scale bounds from design resolution to actual screen size
                var scaledBounds = ScaleRect(
                    _winningsStrapBounds.X,
                    _winningsStrapBounds.Y,
                    _winningsStrapBounds.Width,
                    _winningsStrapBounds.Height);

                var renderBounds = new Rectangle(
                    (int)scaledBounds.X,
                    (int)scaledBounds.Y,
                    (int)scaledBounds.Width,
                    (int)scaledBounds.Height);

                // Render strap shape without text
                _svgStrapRenderer.RenderStrapToGraphics(g, questionStrap, "", renderBounds);

                // Draw winnings amount in center with gold color and outline
                using var font = new Font(questionStrap.FontFamily, 48, FontStyle.Bold);
                using var format = CreateCenteredFormat();

                DrawScaledTextWithOutline(g, _currentAmount!, font, Color.Gold,
                    _winningsStrapBounds.X, _winningsStrapBounds.Y,
                    _winningsStrapBounds.Width, _winningsStrapBounds.Height,
                    format);
                
                return;
            }
        }

        // Fallback: Draw question strap as background for winnings
        var texture = TextureManager.GetTexture(TextureManager.ElementType.QuestionStrap, CurrentTextureSet);
        
        if (texture != null)
        {
            DrawScaledImage(g, texture, 
                _winningsStrapBounds.X, _winningsStrapBounds.Y, 
                _winningsStrapBounds.Width, _winningsStrapBounds.Height);
        }

        // Draw winnings amount in center
        using var fontFallback = new Font("Copperplate Gothic Bold", 48, FontStyle.Bold);
        using var brush = new SolidBrush(Color.Gold);
        using var formatFallback = CreateCenteredFormat();
        
        DrawScaledText(g, _currentAmount!, fontFallback, brush,
            _winningsStrapBounds.X, _winningsStrapBounds.Y,
            _winningsStrapBounds.Width, _winningsStrapBounds.Height,
            formatFallback);
    }

    private void DrawMoneyTreeGraphical(System.Drawing.Graphics g)
    {
        // Render money tree with SVG if theme is available
        if (_activeTheme?.MoneyTree != null && _svgMoneyTreeRenderer != null && _moneyTreeService != null)
        {
            var moneyTree = _activeTheme.MoneyTree;
            var settings = _moneyTreeService.Settings;

            // TV screen dimensions - money tree appears on right side
            float screenHeight = 1080f;
            float padding = screenHeight * 0.005f; // 0.5% padding

            // Money tree ladder dimensions (narrower than old PNG)
            float ladderWidth = 571f; // Same as old overlay width
            float ladderHeight = 868f; // Same as old overlay height

            // Position on right edge
            float rightMargin = 65f;
            float targetX = 1920 - ladderWidth - rightMargin;
            float targetY = (screenHeight - ladderHeight) / 2f; // Vertically centered

            float currentX = targetX;
            if (_moneyTreeAnimating)
            {
                // Slide in from right
                float offscreenX = 1920; // Start position (off-screen right)
                currentX = offscreenX - (_moneyTreeAnimationProgress * (offscreenX - targetX));
            }

            // Scale bounds to actual screen size
            var designBounds = new Rectangle(
                (int)currentX,
                (int)targetY,
                (int)ladderWidth,
                (int)ladderHeight);
            
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

            // Draw money values and question numbers (derive rows from renderer bounds)
            DrawMoneyTreeText(g, currentX, targetY, ladderWidth, ladderHeight);
        }
        else
        {
            // Fallback: Use PNG textures if theme not available
            var treeBase = TextureManager.GetTexture(TextureManager.ElementType.MoneyTreeBase, CurrentTextureSet);
            
            var treePosition = _useSafetyNetAltGraphic 
                ? TextureManager.Instance.GetMoneyTreePositionLockAlt(_currentMoneyTreeLevel)
                : TextureManager.Instance.GetMoneyTreePosition(_currentMoneyTreeLevel);

            if (treeBase == null) return;

            // TV screen - scale from original 720p to 1080p
            float screenHeight = 1080f;
            float padding = screenHeight * 0.005f;
            
            float overlayWidth = 571f;
            float overlayHeight = 868f;
            
            float availableHeight = screenHeight - (2 * padding);
            float screenScale = availableHeight / screenHeight;
            
            float backgroundHeight = screenHeight;
            float backgroundWidth = 745f * screenScale;
            
            float rightMargin = 0f;
            float targetX = 1920 - backgroundWidth - rightMargin;
            float targetY = 0f;
            
            float currentX = targetX;
            if (_moneyTreeAnimating)
            {
                float offscreenX = 1920;
                currentX = offscreenX - (_moneyTreeAnimationProgress * (offscreenX - targetX));
            }

            // Draw base tree image (background)
            DrawScaledImage(g, treeBase, currentX, targetY, backgroundWidth, backgroundHeight);

            float rightMarginOverlay = 65f * screenScale;
            float overlayXOffset = (745f - 571f - 65f) * screenScale;
            
            // Draw position highlight overlay if level is set
            if (treePosition != null && _currentMoneyTreeLevel > 0)
            {
                float overlayX = currentX + overlayXOffset;
                float overlayY = 106f * screenScale;
                
                float scaledOverlayWidth = overlayWidth * screenScale;
                float scaledOverlayHeight = overlayHeight * screenScale;
                
                DrawScaledImage(g, treePosition, overlayX, overlayY, scaledOverlayWidth, scaledOverlayHeight);
            }

            // Draw money values and question numbers (fallback PNG overlay uses scaled coordinates)
            if (_moneyTreeService != null)
            {
                float designBaseX = currentX / screenScale;
                float designBaseY = (106f * screenScale) / screenScale; // = 106f design
                DrawMoneyTreeText(g, designBaseX, designBaseY, overlayWidth, overlayHeight);
            }
        }
    }

    private void DrawMoneyTreeText(System.Drawing.Graphics g, float baseX, float baseY, float width, float height)
    {
        var settings = _moneyTreeService!.Settings;
        var moneyTree = _activeTheme?.MoneyTree;

        // Font settings
        string fontFamily = moneyTree?.FontFamily ?? "Copperplate Gothic Bold";
        float baseFontSize = (moneyTree?.FontSize ?? 22);
        FontStyle fontStyle = FontStyle.Regular;

        // Calculate level dimensions to match renderer
        const int levelCount = 15;
        float levelHeight = height / (float)levelCount;

        // X positions (design coordinates)
        float qnoX_1to9 = baseX + 159f;
        float qnoX_10to15 = baseX + 144f;
        float moneyBaseX = baseX + 244f;

        for (int level = 15; level >= 1; level--)
        {
            // Calculate Y position to match renderer (level 1 at bottom)
            float levelY = baseY + height - (level * levelHeight);
            float designY = levelY;
            float designHeight = levelHeight;

            // Determine text color based on level state
            Color textColor;
            if (level == _currentMoneyTreeLevel)
            {
                if (moneyTree != null)
                    textColor = ColorTranslator.FromHtml(moneyTree.ActiveColor);
                else
                    textColor = _useSafetyNetAltGraphic ? Color.White : Color.Black;
            }
            else if (level == 15)
            {
                textColor = Color.White;
            }
            else if (level == 5 || level == 10)
            {
                bool isQ5Enabled = (settings.SafetyNet1 == 5 || settings.SafetyNet2 == 5);
                bool isQ10Enabled = (settings.SafetyNet1 == 10 || settings.SafetyNet2 == 10);
                bool isRiskMode = _currentGameMode == GameMode.Risk;
                if (level == 5)
                    textColor = (isQ5Enabled && !isRiskMode) ? Color.White : Color.Gold;
                else
                    textColor = (isQ10Enabled && !isRiskMode) ? Color.White : Color.Gold;
            }
            else if (level == settings.SafetyNet1 || level == settings.SafetyNet2)
            {
                if (moneyTree != null && !(_currentGameMode == GameMode.Risk))
                    textColor = ColorTranslator.FromHtml(moneyTree.SafeHavenColor);
                else
                    textColor = (_currentGameMode == GameMode.Risk) ? Color.Gold : Color.White;
            }
            else
            {
                textColor = moneyTree != null ? ColorTranslator.FromHtml(moneyTree.InactiveColor) : Color.Gold;
            }

            using var font = new Font(fontFamily, baseFontSize, fontStyle);
            using var format = new StringFormat { Alignment = StringAlignment.Near };

            float qnoX = (level >= 10) ? qnoX_10to15 : qnoX_1to9;

            // Draw question number and money amount centered on rung
            DrawScaledTextWithOutline(g, level.ToString(), font, textColor, qnoX, designY, 80, designHeight, format, 2);
            string formattedMoney = _moneyTreeService.GetFormattedValue(level);
            DrawScaledTextWithOutline(g, formattedMoney, font, textColor, moneyBaseX, designY, 350, designHeight, format, 2);
        }
    }

    private void DrawATAResults(System.Drawing.Graphics g)
    {
        if (_ataVotes.Count == 0) return;

        // Position centered horizontally, below lifeline icons
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
    
    private void DrawGameWinnerDisplay(System.Drawing.Graphics g)
    {
        // Full-screen winner display (similar to FFF winner display)
        // No background - alpha keyed/transparent
        
        // Draw confetti particles as background
        foreach (var particle in _confettiParticles)
        {
            var state = g.Save();
            g.TranslateTransform(particle.X, particle.Y);
            g.RotateTransform(particle.Rotation);
            
            using (var confettiBrush = new SolidBrush(particle.Color))
            {
                g.FillRectangle(confettiBrush, -particle.Size / 2, -particle.Size / 2, particle.Size, particle.Size * 3);
            }
            
            g.Restore(state);
        }
        
        if (string.IsNullOrEmpty(_gameWinnerAmount))
            return;
        
        using var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        
        // Draw "You Won" above
        using var titleFont = new Font("Copperplate Gothic Bold", 80, FontStyle.Bold);
        using var titleBrush = new SolidBrush(Color.White);
        var titleBounds = new RectangleF(200, 280, 1520, 100);
        DrawScaledText(g, "You Won", titleFont, titleBrush,
            titleBounds.X, titleBounds.Y, titleBounds.Width, titleBounds.Height, format);
        
        // Determine what to display based on currency breakdown
        if (_gameWinnerHasCurrency1 && _gameWinnerHasCurrency2)
        {
            // Show both currencies - Currency 1 first (larger), then Currency 2 below
            var currency1Bounds = new RectangleF(200, 400, 1520, 200);
            using var font1 = new Font("Copperplate Gothic Bold", 100, FontStyle.Bold);
            using var brush1 = new SolidBrush(Color.Gold);
            DrawScaledText(g, _gameWinnerCurrency1 ?? "", font1, brush1,
                currency1Bounds.X, currency1Bounds.Y, currency1Bounds.Width, currency1Bounds.Height, format);
            
            var currency2Bounds = new RectangleF(200, 600, 1520, 150);
            using var font2 = new Font("Copperplate Gothic Bold", 70, FontStyle.Bold);
            using var brush2 = new SolidBrush(Color.LightGoldenrodYellow);
            DrawScaledText(g, _gameWinnerCurrency2 ?? "", font2, brush2,
                currency2Bounds.X, currency2Bounds.Y, currency2Bounds.Width, currency2Bounds.Height, format);
        }
        else if (_gameWinnerHasCurrency2 && !_gameWinnerHasCurrency1)
        {
            // Only Currency 2 (no Currency 1)
            var designBounds = new RectangleF(200, 400, 1520, 280);
            using var font = new Font("Copperplate Gothic Bold", 120, FontStyle.Bold);
            using var brush = new SolidBrush(Color.Gold);
            DrawScaledText(g, _gameWinnerCurrency2 ?? "", font, brush,
                designBounds.X, designBounds.Y, designBounds.Width, designBounds.Height, format);
        }
        else
        {
            // Only Currency 1 or legacy combined amount
            var displayAmount = _gameWinnerCurrency1 ?? _gameWinnerAmount;
            var designBounds = new RectangleF(200, 400, 1520, 280);
            using var font = new Font("Copperplate Gothic Bold", 120, FontStyle.Bold);
            using var brush = new SolidBrush(Color.Gold);
            DrawScaledText(g, displayAmount ?? "", font, brush,
                designBounds.X, designBounds.Y, designBounds.Width, designBounds.Height, format);
        }
    }
    
    private void DrawFFFDisplay(System.Drawing.Graphics g)
    {
        // No background for FFF display - alpha keyed/transparent
        // Background will be added as a game-wide option later
        
        // Show winner with large text
        if (_fffShowWinner && !string.IsNullOrEmpty(_fffWinnerName))
        {
            // Draw winner text centered on screen
            var designBounds = new RectangleF(200, 400, 1520, 280);
            using var brush = new SolidBrush(Color.Gold);
            using var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            
            // Dynamically scale font size to fit long names
            float baseFontSize = 120;
            float scaledFontSize = baseFontSize * Math.Min(ScaleX, ScaleY);
            var destRect = ScaleRect(designBounds.X, designBounds.Y, designBounds.Width, designBounds.Height);
            
            // Measure text WITHOUT width constraint to get true width
            using var testFont = new Font("Copperplate Gothic Bold", scaledFontSize, FontStyle.Bold);
            var textSize = g.MeasureString(_fffWinnerName, testFont);
            
            // If text is wider than available space, scale down proportionally
            float maxWidth = destRect.Width * 0.90f; // Leave 10% margin for safety
            if (textSize.Width > maxWidth)
            {
                scaledFontSize *= maxWidth / textSize.Width;
            }
            
            using var font = new Font("Copperplate Gothic Bold", scaledFontSize, FontStyle.Bold);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            g.DrawString(_fffWinnerName, font, brush, destRect, format);
            
            // Draw "WINNER!" above
            using var titleFont = new Font("Copperplate Gothic Bold", 80, FontStyle.Bold);
            using var titleBrush = new SolidBrush(Color.White);
            var titleBounds = new RectangleF(200, 280, 1520, 100);
            DrawScaledText(g, "WINNER!", titleFont, titleBrush,
                titleBounds.X, titleBounds.Y, titleBounds.Width, titleBounds.Height, format);
            
            // Draw time below winner name if available
            if (_fffWinnerTime.HasValue)
            {
                using var timeFont = new Font("Arial", 60, FontStyle.Bold);
                using var timeBrush = new SolidBrush(Color.White);
                var timeBounds = new RectangleF(200, 700, 1520, 100);
                var timeText = $"{_fffWinnerTime.Value:F2}s";
                DrawScaledText(g, timeText, timeFont, timeBrush,
                    timeBounds.X, timeBounds.Y, timeBounds.Width, timeBounds.Height, format);
            }
            return;
        }
        
        // Draw contestant straps
        if (_fffContestants.Count > 0)
        {
            // Layout contestants vertically with full-width straps
            float strapHeight = 77;    // Scaled from VB.NET's 51px (51 * 1.5 ≈ 77)
            float spacing = 80;        // Scaled from VB.NET's 53px spacing (53 * 1.5 ≈ 80)
            float strapWidth = 1920;   // Full screen width
            
            // Calculate total height and start position to center vertically
            float totalHeight = (_fffContestants.Count * strapHeight) + ((_fffContestants.Count - 1) * (spacing - strapHeight));
            float currentY = (1080 - totalHeight) / 2; // Center vertically
            
            // Get theme strap settings
            var themeStrap = _activeTheme?.Straps.FirstOrDefault(s => s.StrapType == "Answer");
            
            for (int i = 0; i < _fffContestants.Count; i++)
            {
                var name = _fffContestants[i];
                bool isHighlighted = i == _fffHighlightedIndex;
                
                // Full-width strap bounds
                var designBounds = new RectangleF(0, currentY, strapWidth, strapHeight);
                var scaledBounds = ScaleRect(designBounds.X, designBounds.Y, designBounds.Width, designBounds.Height);
                
                var bounds = new Rectangle(
                    (int)scaledBounds.X,
                    (int)scaledBounds.Y,
                    (int)scaledBounds.Width,
                    (int)scaledBounds.Height);
                
                // Determine strap colors based on state
                ThemeStrap? strapToRender = null;
                if (themeStrap != null)
                {
                    if (isHighlighted)
                    {
                        // Use gold/yellow highlight colors for fastest contestant
                        strapToRender = new ThemeStrap
                        {
                            StrapType = themeStrap.StrapType,
                            SvgShape = themeStrap.SvgShape,
                            PrimaryColor = "#FFD700",      // Gold
                            SecondaryColor = "#FFA500",    // Orange
                            GradientEnabled = themeStrap.GradientEnabled,
                            GradientAngle = themeStrap.GradientAngle,
                            EffectType = themeStrap.EffectType,
                            EffectIntensity = themeStrap.EffectIntensity,
                            BorderEnabled = themeStrap.BorderEnabled,
                            FontFamily = themeStrap.FontFamily,
                            FontSize = themeStrap.FontSize,
                            FontColor = themeStrap.FontColor,
                            FontBold = themeStrap.FontBold
                        };
                    }
                    else
                    {
                        // Use theme colors for normal state
                        strapToRender = themeStrap;
                    }
                }
                
                // Render SVG strap shape without text
                if (strapToRender != null && _svgStrapRenderer != null)
                {
                    _svgStrapRenderer.RenderStrapToGraphics(g, strapToRender, "", bounds);
                }
                else
                {
                    // Fallback: colored rectangle if theme not available
                    Color bgColor = isHighlighted ? Color.Gold : Color.FromArgb(0, 0, 102);
                    using var bgBrush = new SolidBrush(bgColor);
                    g.FillRectangle(bgBrush, scaledBounds);
                    
                    using var borderPen = new Pen(Color.White, 3);
                    g.DrawRectangle(borderPen, scaledBounds.X, scaledBounds.Y, scaledBounds.Width, scaledBounds.Height);
                }
                
                // Draw contestant name and time together (left side of strap)
                // VB.NET position: X=379 (scaled to 570px for 1920 width)
                using var font = new Font("Copperplate Gothic Bold", 30, FontStyle.Bold); // Scaled from 20.25pt
                Color textColor = isHighlighted ? Color.Black : Color.White;
                using var brush = new SolidBrush(textColor);
                using var format = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
                
                // Build display text: name followed by time (if available) with 2 spaces between
                string displayText = name;
                if (_fffTimes.Count > i && _fffTimes[i] > 0)
                {
                    displayText = $"{name}  {_fffTimes[i]:F2}s";
                }
                
                // Draw combined text (name + time) starting from left offset
                DrawScaledText(g, displayText, font, brush,
                    designBounds.X + 570, designBounds.Y, designBounds.Width - 1200, designBounds.Height, format);
                
                currentY += spacing; // Move to next strap position
            }
        }
    }
    
    private void DrawLifelineIcons(System.Drawing.Graphics g)
    {
        // Design-time coordinates (1920x1080)
        // Position: Right edge, stacked vertically (1770, 36), spacing 82px vertical
        float baseX = 1700;
        float baseY = 36;
        float spacingY = 100;  // Vertical spacing for stacking
        float iconWidth = 150;  // Slightly smaller for TV screen
        float iconHeight = 90;  // Slightly smaller for TV screen
        
        // Draw up to 4 lifeline icons (stacked vertically on right edge)
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
                float y = baseY + ((i - 1) * spacingY);
                DrawScaledImage(g, icon, baseX, y, iconWidth, iconHeight);
            }
        }
    }

    /// <summary>
    /// Draw text with automatic wrapping to specified max lines and auto-scaling if text is too large
    /// </summary>
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

    private void DrawScaledTextWithWrapAndOutline(System.Drawing.Graphics g, string text, 
        string fontFamily, float baseFontSize, FontStyle fontStyle, Color color, 
        RectangleF bounds, int maxLines, StringAlignment alignment = StringAlignment.Center, int outlineWidth = 2)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        var scaledBounds = ScaleRect(bounds.X, bounds.Y, bounds.Width, bounds.Height);
        float fontSize = baseFontSize * ScaleX;

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
                
                // Draw black outline
                using var outlineBrush = new SolidBrush(Color.Black);
                for (int x = -outlineWidth; x <= outlineWidth; x++)
                {
                    for (int y = -outlineWidth; y <= outlineWidth; y++)
                    {
                        if (x == 0 && y == 0) continue;
                        
                        var outlineRect = new RectangleF(
                            scaledBounds.X + x,
                            scaledBounds.Y + y,
                            scaledBounds.Width,
                            scaledBounds.Height);
                        g.DrawString(text, testFont, outlineBrush, outlineRect, format);
                    }
                }
                
                // Draw main text
                using var brush = new SolidBrush(color);
                g.DrawString(text, testFont, brush, scaledBounds, format);
                return;
            }
        }

        // Fallback: draw with smallest tested size
        using var fallbackFont = new Font(fontFamily, fontSize * 0.5f, fontStyle);
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
        
        // Draw outline for fallback
        using var outlineBrushFallback = new SolidBrush(Color.Black);
        for (int x = -outlineWidth; x <= outlineWidth; x++)
        {
            for (int y = -outlineWidth; y <= outlineWidth; y++)
            {
                if (x == 0 && y == 0) continue;
                
                var outlineRect = new RectangleF(
                    scaledBounds.X + x,
                    scaledBounds.Y + y,
                    scaledBounds.Width,
                    scaledBounds.Height);
                g.DrawString(text, fallbackFont, outlineBrushFallback, outlineRect, format);
            }
        }
        
        using var brushFallback = new SolidBrush(color);
        g.DrawString(text, fallbackFont, brushFallback, scaledBounds, format);
    }

    #region IGameScreen Implementation
    
    public void Initialize(MoneyTreeService moneyTreeService)
    {
        _moneyTreeService = moneyTreeService;
        
        // Initialize background renderer with current settings and theme service
        // Try to get settings and theme service from Program.ServiceProvider
        try
        {
            var settingsManager = Program.ServiceProvider?.GetRequiredService<ApplicationSettingsManager>();
            if (settingsManager?.Settings != null)
            {
                // Try to get ThemeService - it may not be registered yet (Phase 5)
                ThemeService? themeService = null;
                try
                {
                    themeService = new ThemeService(settingsManager.ConnectionString);
                    // Load active theme asynchronously for both backgrounds and straps
                    _ = Task.Run(async () =>
                    {
                        await themeService.LoadActiveThemeAsync();
                        var activeTheme = themeService.CurrentTheme;
                        if (activeTheme != null)
                        {
                            _activeTheme = await themeService.GetCompleteThemeAsync(activeTheme.ThemeId);
                            _svgStrapRenderer = new SvgStrapRenderer();
                            _svgMoneyTreeRenderer = new SvgMoneyTreeRenderer();
                            var strapCount = _activeTheme?.Straps?.Count ?? 0;
                            var questionStraps = _activeTheme?.Straps?.Count(s => s.StrapType == "Question") ?? 0;
                            var answerStraps = _activeTheme?.Straps?.Count(s => s.StrapType == "Answer") ?? 0;
                            var hasMoneyTree = _activeTheme?.MoneyTree != null;
                            GameConsole.Info($"[TVScreenForm] Theme '{_activeTheme?.Theme.ThemeName}' loaded: {strapCount} straps ({questionStraps} Question, {answerStraps} Answer), MoneyTree: {hasMoneyTree}");
                            Invalidate(); // Redraw with themed straps
                        }
                    });
                    GameConsole.Debug("[TVScreenForm] ThemeService initialized for background and strap rendering");
                }
                catch (Exception ex)
                {
                    GameConsole.Warn($"[TVScreenForm] ThemeService not available: {ex.Message}");
                    // Continue without theme service - will fall back to legacy backgrounds and PNG straps
                }
                
                _backgroundRenderer = new BackgroundRenderer(settingsManager.Settings, themeService);
                GameConsole.Debug("[TVScreenForm] BackgroundRenderer initialized");
            }
        }
        catch (Exception ex)
        {
            GameConsole.Error($"[TVScreenForm] Error initializing background renderer: {ex.Message}");
            // If DI not available, background renderer will remain null and fall back to black background
        }
    }
    
    public void UpdateMoneyTreeLevel(int level)
    {
        _currentMoneyTreeLevel = level;
        _useSafetyNetAltGraphic = false; // Reset to normal graphic
        _isSafetyNetFlashing = false; // stop flashing
        if (_showMoneyTree)
        {
            Invalidate(); // Redraw if money tree is currently visible
        }
    }
    
    /// <summary>
    /// Updates money tree with safety net lock-in flash animation
    /// </summary>
    public void UpdateMoneyTreeWithSafetyNetFlash(int safetyNetLevel, bool flashState)
    {
        _currentMoneyTreeLevel = safetyNetLevel;
        _useSafetyNetAltGraphic = flashState; // true = use alternate graphic, false = use regular
        _isSafetyNetFlashing = true;
        if (_showMoneyTree)
        {
            Invalidate(); // Redraw to show flash state if money tree is visible
        }
    }

    public void UpdateQuestion(Question question)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => UpdateQuestion(question)));
            return;
        }

        _currentQuestion = question;
        _selectedAnswer = null;
        _correctAnswer = null;
        _isRevealing = false;
        _showATA = false;
        _ataVotes.Clear();
        _visibleAnswers.Clear(); // Reset visible answers
        
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
        // TV screen doesn't show this
    }

    public void ShowQuestion(bool show)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => ShowQuestion(show)));
            return;
        }

        _showQuestionAndAnswers = show;
        _showWinnings = false; // Hide winnings when showing question
        Invalidate();
    }

    public void ShowWinnings(GameState state)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => ShowWinnings(state)));
            return;
        }

        // TV Screen: Clear all graphics and show money tree with animation
        // Use GetDisplayLevel to show level 15 when game is won
        _currentMoneyTreeLevel = _moneyTreeService!.GetDisplayLevel(state.CurrentLevel, state.GameWin);
        _currentGameMode = state.Mode; // Store game mode for rendering
        _showWinnings = false;
        _showQuestionAndAnswers = false;
        _showMoneyTree = true;
        
        // Start slide-in animation
        _ = AnimateMoneyTreeSlideIn();
    }

    private async Task AnimateMoneyTreeSlideIn()
    {
        _moneyTreeAnimating = true;
        _moneyTreeAnimationProgress = 0f;

        // Animate over ~500ms
        int steps = 30;
        for (int i = 0; i <= steps; i++)
        {
            _moneyTreeAnimationProgress = i / (float)steps;
            Invalidate();
            await Task.Delay(16); // ~60 FPS
        }

        _moneyTreeAnimating = false;
        _moneyTreeAnimationProgress = 1f;
        Invalidate();
    }

    public void ShowWinningsAmount(string amount)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => ShowWinningsAmount(amount)));
            return;
        }

        _currentAmount = amount; // Use specific amount
        _showWinnings = true;
        _showQuestionAndAnswers = false; // Hide question when showing winnings
        Invalidate();
    }

    public void HideWinnings()
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(HideWinnings));
            return;
        }

        // Start slide-out animation if money tree is showing
        if (_showMoneyTree)
        {
            _ = AnimateMoneyTreeSlideOut();
        }
        else
        {
            // No animation needed, just hide
            _currentAmount = null;
            _showWinnings = false;
            _showMoneyTree = false;
            Invalidate();
        }
    }

    private async Task AnimateMoneyTreeSlideOut()
    {
        _moneyTreeAnimating = true;
        _moneyTreeAnimationProgress = 1f; // Start at fully visible

        // Animate over ~500ms (slide to the right, off screen)
        int steps = 30;
        for (int i = 0; i <= steps; i++)
        {
            _moneyTreeAnimationProgress = 1f - (i / (float)steps); // Reverse: 1.0 -> 0.0
            Invalidate();
            await Task.Delay(16); // ~60 FPS
        }

        // Animation complete, hide everything
        _moneyTreeAnimating = false;
        _moneyTreeAnimationProgress = 0f;
        _currentAmount = null;
        _showWinnings = false;
        _showMoneyTree = false;
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
        _currentAmount = null;
        _visibleAnswers.Clear();
        _showQuestionAndAnswers = false;
        _showWinnings = false;
        _showPAFTimer = false; // Hide PAF timer on reset
        _showATATimer = false; // Hide ATA timer on reset
        _showLifelineIcons = false; // Hide lifeline icons on reset
        
        // Clear winner display and confetti
        _showGameWinner = false;
        _gameWinnerAmount = null;
        _confettiTimer?.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        _confettiTimer?.Dispose();
        _confettiTimer = null;
        _confettiParticles.Clear();
        
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

    public void ShowATAResults(Dictionary<string, int> votes)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => ShowATAResults(votes)));
            return;
        }

        _ataVotes = new Dictionary<string, int>(votes);
        _showATA = true;
        Invalidate();
    }

    public void HideATAResults()
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(HideATAResults));
            return;
        }

        _showATA = false;
        _ataVotes.Clear();
        Invalidate();
    }

    public void UpdateMoney(string current, string correct, string wrong, string drop, string questionsLeft)
    {
        // TV screen typically doesn't show detailed money tree - could implement if needed
    }

    public void ActivateLifeline(Lifeline lifeline)
    {
        // Lifeline activation visual effects - implement as needed
    }

    #endregion
    
    public void ClearQuestionAndAnswerText()
    {
        // TV screen doesn't need this - it's only for host/guest screens
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
    
    #region FFF Display Methods
    
    public void ShowFFFContestant(int index, string name)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => ShowFFFContestant(index, name)));
            return;
        }
        
        _showFFF = true;
        _fffShowWinner = false;
        if (_fffContestants.Count <= index)
        {
            _fffContestants.Add(name);
        }
        Invalidate();
    }
    
    public void ShowAllFFFContestants(List<string> names, List<double>? times = null)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => ShowAllFFFContestants(names, times)));
            return;
        }
        
        _showFFF = true;
        _fffContestants = new List<string>(names);
        _fffTimes = times != null ? new List<double>(times) : new List<double>();
        _fffHighlightedIndex = -1;
        _fffShowWinner = false;
        Invalidate();
    }
    
    public void HighlightFFFContestant(int index, bool isWinner = false)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => HighlightFFFContestant(index, isWinner)));
            return;
        }
        
        _fffHighlightedIndex = index;
        _fffShowWinner = isWinner;
        Invalidate();
    }
    
    public void ShowFFFWinner(string name, double? time = null)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => ShowFFFWinner(name, time)));
            return;
        }
        
        _showFFF = true;
        _fffShowWinner = true;
        _fffWinnerName = name;
        _fffWinnerTime = time;
        Invalidate();
    }
    
    public void ClearFFFDisplay()
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(ClearFFFDisplay));
            return;
        }
        
        _showFFF = false;
        _fffContestants.Clear();
        _fffTimes.Clear();
        _fffHighlightedIndex = -1;
        _fffShowWinner = false;
        _fffWinnerName = null;
        _fffWinnerTime = null;
        Invalidate();
    }
    
    /// <summary>
    /// Show full-screen game winner display (Thanks for Playing portion)
    /// </summary>
    public void ShowGameWinner(string amount, int questionLevel)
    {
        ShowGameWinner(amount, null, null, false, false, questionLevel);
    }

    /// <summary>
    /// Show full-screen game winner display with currency breakdown
    /// </summary>
    public void ShowGameWinner(string combinedAmount, string? currency1Amount, string? currency2Amount, 
        bool hasCurrency1, bool hasCurrency2, int questionLevel)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => ShowGameWinner(combinedAmount, currency1Amount, currency2Amount, hasCurrency1, hasCurrency2, questionLevel)));
            return;
        }
        
        GameConsole.Info($"[TVScreen] ShowGameWinner - Combined: {combinedAmount}, C1: {currency1Amount} ({hasCurrency1}), C2: {currency2Amount} ({hasCurrency2})");
        
        _showGameWinner = true;
        _gameWinnerAmount = combinedAmount;
        _gameWinnerCurrency1 = currency1Amount;
        _gameWinnerCurrency2 = currency2Amount;
        _gameWinnerHasCurrency1 = hasCurrency1;
        _gameWinnerHasCurrency2 = hasCurrency2;
        
        // Skip confetti for preview screens to avoid performance issues
        if (IsPreview)
        {
            GameConsole.Debug("Skipping confetti for preview screen");
            Invalidate();
            return;
        }
        
        // Only show confetti for significant wins (Q11+)
        // Walking away at Q10 gives Q9 prize, so confetti starts at Q11
        if (questionLevel >= 11)
        {
            GameConsole.Info($"Initializing confetti for Q{questionLevel}");
            
            // Stop any existing timer first
            if (_confettiTimer != null)
            {
                GameConsole.Warn("Confetti timer already exists, stopping it");
                _confettiTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                _confettiTimer.Dispose();
                _confettiTimer = null;
            }
            
            // Initialize confetti particles
            InitializeConfetti();
            
            // Ensure form handle is created for timer to work
            if (!IsHandleCreated)
            {
                var _ = Handle; // Force handle creation
            }
            
            // Start confetti animation timer using Threading.Timer (doesn't depend on Windows message pump)
            _confettiTimer = new System.Threading.Timer(
                callback: _ => ConfettiTimer_Tick(),
                state: null,
                dueTime: 67,
                period: 67  // 15 FPS for better performance with multiple screens
            );
        }
        
        Invalidate();
    }
    
    /// <summary>
    /// Initialize confetti particles for celebration animation
    /// </summary>
    private void InitializeConfetti()
    {
        _confettiParticles.Clear();
        var random = new Random();
        var colors = new[] { Color.Gold, Color.Yellow, Color.Orange, Color.Red, Color.Blue, Color.Green, Color.Purple, Color.Magenta };
        
        // Create 100 confetti particles
        for (int i = 0; i < 100; i++)
        {
            _confettiParticles.Add(new ConfettiParticle
            {
                X = random.Next(0, 1920),
                Y = random.Next(-500, 0), // Start above screen
                VelocityY = random.Next(2, 6),
                VelocityX = random.Next(-2, 3),
                Rotation = random.Next(0, 360),
                RotationSpeed = random.Next(-10, 11),
                Color = colors[random.Next(colors.Length)],
                Size = random.Next(8, 20)
            });
        }
    }
    
    /// <summary>
    /// Timer tick handler for confetti animation
    /// </summary>
    private void ConfettiTimer_Tick()
    {
        // Use Invoke to update UI from background thread
        if (InvokeRequired)
        {
            try
            {
                Invoke(new Action(UpdateConfetti));
            }
            catch (ObjectDisposedException)
            {
                // Form was disposed, stop timer
                _confettiTimer?.Dispose();
                _confettiTimer = null;
            }
        }
        else
        {
            UpdateConfetti();
        }
    }
    
    /// <summary>
    /// Update confetti particle positions for animation
    /// </summary>
    private void UpdateConfetti()
    {
        // Stop updating if display was cleared
        if (!_showGameWinner || _confettiTimer == null)
        {
            return;
        }
        
        var random = new Random();
        
        foreach (var particle in _confettiParticles)
        {
            // Update position
            particle.Y += particle.VelocityY;
            particle.X += particle.VelocityX;
            particle.Rotation += particle.RotationSpeed;
            
            // Reset particle if it falls off screen
            if (particle.Y > 1080)
            {
                particle.Y = -20;
                particle.X = random.Next(0, 1920);
            }
        }
        
        // Always redraw while animation is active
        Invalidate();
    }
    
    /// <summary>
    /// Hide full-screen game winner display
    /// </summary>
    public void ClearGameWinnerDisplay()
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(ClearGameWinnerDisplay));
            return;
        }
        
        _showGameWinner = false;
        _gameWinnerAmount = null;
        
        // Stop confetti animation (Threading.Timer)
        _confettiTimer?.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        _confettiTimer?.Dispose();
        _confettiTimer = null;
        _confettiParticles.Clear();
        
        Invalidate();
    }
    
    /// <summary>
    /// Refresh background and straps when theme changes
    /// Clears background cache and reloads from active theme
    /// </summary>
    public void RefreshTheme()
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(RefreshTheme));
            return;
        }
        
        GameConsole.Info("[TVScreenForm] Refreshing theme backgrounds and straps");
        
        // Clear background cache to force reload
        _backgroundRenderer?.ClearCache();
        
        // Reload theme data for straps
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
                        _svgStrapRenderer ??= new SvgStrapRenderer();
                        _svgMoneyTreeRenderer ??= new SvgMoneyTreeRenderer();
                        GameConsole.Info($"[TVScreenForm] Theme '{_activeTheme?.Theme?.ThemeName ?? "Unknown"}' reloaded");
                    }
                    else
                    {
                        GameConsole.Info("[TVScreenForm] No active theme, using legacy straps");
                    }
                    
                    Invalidate(); // Redraw screen with new theme
                }
            }
            catch (Exception ex)
            {
                GameConsole.Error($"[TVScreenForm] Error refreshing theme: {ex.Message}");
            }
        });
    }
    
    #endregion
}

/// <summary>
/// Confetti particle for winner celebration animation
/// </summary>
internal class ConfettiParticle
{
    public float X { get; set; }
    public float Y { get; set; }
    public float VelocityY { get; set; }
    public float VelocityX { get; set; }
    public float Rotation { get; set; }
    public float RotationSpeed { get; set; }
    public Color Color { get; set; }
    public float Size { get; set; }
}

