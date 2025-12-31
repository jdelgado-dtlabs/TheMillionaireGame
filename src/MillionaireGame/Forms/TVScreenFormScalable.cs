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
/// TV screen with scalable rendering - proof of concept
/// </summary>
public class TVScreenFormScalable : ScalableScreenBase, IGameScreen
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
    
    /// <summary>
    /// Gets or sets whether this screen is a preview instance.
    /// Preview screens skip intensive animations like confetti.
    /// </summary>
    public bool IsPreview { get; set; } = false;
    private bool _useSafetyNetAltGraphic = false; // Track if we should use alternate lock-in graphic
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

    public TVScreenFormScalable()
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
            GameConsole.Debug($"[TVScreen] Rendering background via BackgroundRenderer");
            _backgroundRenderer.RenderBackground(e.Graphics, ClientSize.Width, ClientSize.Height);
        }
        else
        {
            GameConsole.Debug($"[TVScreen] BackgroundRenderer is NULL - using black fallback");
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
        
        // If money tree is showing, render it (takes over entire screen)
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
        var texture = TextureManager.GetTexture(TextureManager.ElementType.QuestionStrap, CurrentTextureSet);
        
        if (texture != null)
        {
            DrawScaledImage(g, texture, 
                _questionStrapBounds.X, _questionStrapBounds.Y, 
                _questionStrapBounds.Width, _questionStrapBounds.Height);
        }

        // Draw question text with wrapping and auto-scaling
        // Substantial padding to keep text within visible bounds (180px each side)
        var textBounds = new RectangleF(
            _questionStrapBounds.X + 180, 
            _questionStrapBounds.Y + 15,
            _questionStrapBounds.Width - 360, 
            _questionStrapBounds.Height - 30);
            
        DrawScaledTextWithWrap(g, _currentQuestion?.QuestionText ?? "", 
            "Copperplate Gothic Bold", 32, FontStyle.Bold, Color.White, textBounds, 2);
    }

    private void DrawAnswerBox(System.Drawing.Graphics g, string letter, string text, RectangleF bounds, bool isLeft, bool showText)
    {
        // Determine which texture to use
        var elementType = TextureManager.ElementType.AnswerLeftNormal;
        
        if (_isRevealing && _correctAnswer == letter)
        {
            // Always show correct answer in green
            elementType = isLeft ? TextureManager.ElementType.AnswerLeftCorrect : TextureManager.ElementType.AnswerRightCorrect;
        }
        else if (_isRevealing && _selectedAnswer == letter && _selectedAnswer != _correctAnswer)
        {
            // Show wrong answer in red (final answer that was incorrect)
            elementType = isLeft ? TextureManager.ElementType.AnswerLeftFinal : TextureManager.ElementType.AnswerRightFinal;
        }
        else if (_selectedAnswer == letter)
        {
            // Show selected answer (before reveal)
            elementType = isLeft ? TextureManager.ElementType.AnswerLeftFinal : TextureManager.ElementType.AnswerRightFinal;
        }
        else
        {
            elementType = isLeft ? TextureManager.ElementType.AnswerLeftNormal : TextureManager.ElementType.AnswerRightNormal;
        }

        var texture = TextureManager.GetTexture(elementType, CurrentTextureSet);
        
        if (texture != null)
        {
            DrawScaledImage(g, texture, bounds.X, bounds.Y, bounds.Width, bounds.Height);
        }

        // Only draw text if answer is visible
        if (showText)
        {
            // Left answers (A, C) have more padding, right answers (B, D) have less
            float letterLeftPadding = isLeft ? 150 : 40;
            float textLeftPadding = isLeft ? 240 : 130;
            float textRightPadding = 80;
            
            // Draw answer letter
            using var letterFont = new Font("Arial", 28, FontStyle.Bold);
            using var letterBrush = new SolidBrush(Color.White);
            using var letterFormat = CreateCenteredFormat();
            
            DrawScaledText(g, letter + ":", letterFont, letterBrush,
                bounds.X + letterLeftPadding, bounds.Y + 15,
                60, bounds.Height - 30,
                letterFormat);

            // Draw answer text with wrapping and auto-scaling
            var textBounds = new RectangleF(
                bounds.X + textLeftPadding, 
                bounds.Y + 15,
                bounds.Width - textLeftPadding - textRightPadding, 
                bounds.Height - 30);
                
            DrawScaledTextWithWrap(g, text, 
                "Copperplate Gothic Bold", 24, FontStyle.Regular, Color.White, textBounds, 2, 
                StringAlignment.Near);
        }
    }

    private void DrawWinningsDisplay(System.Drawing.Graphics g)
    {
        // Draw question strap as background for winnings
        var texture = TextureManager.GetTexture(TextureManager.ElementType.QuestionStrap, CurrentTextureSet);
        
        if (texture != null)
        {
            DrawScaledImage(g, texture, 
                _winningsStrapBounds.X, _winningsStrapBounds.Y, 
                _winningsStrapBounds.Width, _winningsStrapBounds.Height);
        }

        // Draw winnings amount in center
        using var font = new Font("Copperplate Gothic Bold", 48, FontStyle.Bold);
        using var brush = new SolidBrush(Color.Gold);
        using var format = CreateCenteredFormat();
        
        DrawScaledText(g, _currentAmount!, font, brush,
            _winningsStrapBounds.X, _winningsStrapBounds.Y,
            _winningsStrapBounds.Width, _winningsStrapBounds.Height,
            format);
    }

    private void DrawMoneyTreeGraphical(System.Drawing.Graphics g)
    {
        // Get base money tree image for current texture set
        var treeBase = TextureManager.GetTexture(TextureManager.ElementType.MoneyTreeBase, CurrentTextureSet);
        
        // Get position overlay - use alternate lock-in graphic if in safety net animation
        var treePosition = _useSafetyNetAltGraphic 
            ? TextureManager.Instance.GetMoneyTreePositionLockAlt(_currentMoneyTreeLevel)
            : TextureManager.Instance.GetMoneyTreePosition(_currentMoneyTreeLevel);

        if (treeBase == null) return;

        // TV screen - scale from original 720p to 1080p
        // Original VB.NET (720p): 630x720 canvas after crop, 399x599 overlay at (165, 100)
        // New assets (1080p): 745x1080 background, 571x868 overlay
        // Scale factors: width 745/630=1.182, height 1080/720=1.5
        
        float screenHeight = 1080f;
        float padding = screenHeight * 0.005f; // 0.5% padding = ~5px (reduced from 1%)
        
        // New overlay dimensions (from assets)
        float overlayWidth = 571f;
        float overlayHeight = 868f;
        
        // Available height for drawing
        float availableHeight = screenHeight - (2 * padding);
        
        // Scale to fit screen (if needed)
        float screenScale = availableHeight / screenHeight; // ~0.98
        
        // Background dimensions
        float backgroundHeight = screenHeight; // Full height
        float backgroundWidth = 745f * screenScale;
        
        // Position background on right edge, sliding in from right
        float rightMargin = 0f; // No margin, flush to right edge
        float targetX = 1920 - backgroundWidth - rightMargin;
        float targetY = 0f; // Top of screen, no padding
        
        float currentX = targetX;
        if (_moneyTreeAnimating)
        {
            // Slide in from right
            float offscreenX = 1920; // Start position (off-screen right)
            currentX = offscreenX - (_moneyTreeAnimationProgress * (offscreenX - targetX));
        }

        // Draw base tree image (background) - full height
        DrawScaledImage(g, treeBase, currentX, targetY, backgroundWidth, backgroundHeight);

        // Calculate overlay position with margin from right edge
        // Background: 745px, Overlay: 571px, use 65px right margin
        float rightMarginOverlay = 65f * screenScale;
        float overlayXOffset = (745f - 571f - 65f) * screenScale; // 109px from left
        
        // Draw position highlight overlay if level is set
        if (treePosition != null && _currentMoneyTreeLevel > 0)
        {
            // Position overlay with right margin
            float overlayX = currentX + overlayXOffset;
            float overlayY = 106f * screenScale; // Vertically centered: (1080 - 868) / 2 = 106px
            
            // Overlay dimensions scaled to screen
            float scaledOverlayWidth = overlayWidth * screenScale;
            float scaledOverlayHeight = overlayHeight * screenScale;
            
            DrawScaledImage(g, treePosition, overlayX, overlayY, scaledOverlayWidth, scaledOverlayHeight);
        }

        // Draw money values and question numbers
        if (_moneyTreeService != null)
        {
            DrawMoneyTreeText(g, currentX, padding, screenScale, overlayXOffset);
        }
    }

    private void DrawMoneyTreeText(System.Drawing.Graphics g, float baseX, float padding, float screenScale, float overlayXOffset)
    {
        var settings = _moneyTreeService!.Settings;
        
        // Text positions are now absolute from background left edge (not relative to overlay)
        // This allows overlay to move independently without affecting text
        // Moved left by 20px from previous positions (179, 164, 264)
        float heightScale = 1080f / 720f; // 1.5
        
        float qnoBaseX_1to9 = baseX + (159f * screenScale);   // Absolute position: 159px from background left
        float qnoBaseX_10to15 = baseX + (144f * screenScale); // Absolute position: 144px from background left
        float moneyBaseX = baseX + (244f * screenScale);      // Absolute position: 244px from background left
        
        // Vertical positioning: Text constrained to overlay height
        // Overlay at 106px, Text starts at 109px (3px offset to center on strap)
        // Text height matches overlay height exactly: 868px
        float textStartY = 109f * screenScale;
        float textHeight = 868f * screenScale;
        float rowHeight = textHeight / 15f; // 15 levels, each gets 868/15 = 57.87px row height
        
        // Font size: reduced from 24pt to 22pt to prevent text drift
        float baseFontSize = 22f * heightScale * screenScale;
        
        for (int level = 15; level >= 1; level--)
        {
            // Y position: each level occupies a row of fixed height
            float y = textStartY + ((15 - level) * rowHeight);
            
            // Question number X position - varies by level (855 for 1-9, 832 for 10-15)
            float qnoX = (level >= 10) ? qnoBaseX_10to15 : qnoBaseX_1to9;
            
            // Determine text color based on level state
            Color textColor;
            if (level == _currentMoneyTreeLevel)
            {
                // Current level - use white text if showing alternate lock-in graphic, black otherwise
                textColor = _useSafetyNetAltGraphic ? Color.White : Color.Black;
            }
            else if (level == 15)
            {
                textColor = Color.White; // Q15 is always white (million dollar question)
            }
            else if (level == 5 || level == 10)
            {
                // Q5 and Q10 are white only if they're enabled as safety nets
                bool isQ5Enabled = (settings.SafetyNet1 == 5 || settings.SafetyNet2 == 5);
                bool isQ10Enabled = (settings.SafetyNet1 == 10 || settings.SafetyNet2 == 10);
                bool isRiskMode = _currentGameMode == GameMode.Risk;
                
                // White only if safety net is enabled AND not in Risk Mode
                if (level == 5)
                    textColor = (isQ5Enabled && !isRiskMode) ? Color.White : Color.Gold;
                else // level == 10
                    textColor = (isQ10Enabled && !isRiskMode) ? Color.White : Color.Gold;
            }
            else if (level == settings.SafetyNet1 || level == settings.SafetyNet2)
            {
                // Custom safety nets (not Q5/Q10) - white only if not in Risk Mode
                textColor = (_currentGameMode == GameMode.Risk) ? Color.Gold : Color.White;
            }
            else
            {
                textColor = Color.Gold; // Regular levels
            }
            
            using var font = new Font("Copperplate Gothic Bold", baseFontSize, FontStyle.Bold);
            using var brush = new SolidBrush(textColor);
            using var format = new StringFormat { Alignment = StringAlignment.Near };
            
            // Draw question number - width scaled for double digits
            DrawScaledText(g, level.ToString(), font, brush, qnoX, y, 80 * screenScale, 40 * heightScale * screenScale, format);
            
            // Draw money amount
            string formattedMoney = _moneyTreeService.GetFormattedValue(level);
            DrawScaledText(g, formattedMoney, font, brush, moneyBaseX, y, 350 * screenScale, 40 * heightScale * screenScale, format);
        }
    }

    private void DrawATAResults(System.Drawing.Graphics g)
    {
        if (_ataVotes.Count == 0) return;

        // TV Screen: Top center position for audience viewing after voting completes
        // Position: 585, 50 (centered horizontally in 1920 width), Size: 750x450
        var overlayBounds = new RectangleF(585, 50, 750, 450);
        var scaledBounds = ScaleRect(overlayBounds.X, overlayBounds.Y, overlayBounds.Width, overlayBounds.Height);

        // Semi-transparent background
        using var bgBrush = new SolidBrush(Color.FromArgb(200, 0, 0, 0));
        g.FillRectangle(bgBrush, scaledBounds);

        // Draw title
        var titleBounds = new RectangleF(overlayBounds.X, overlayBounds.Y + 20, overlayBounds.Width, 60);
        using var titleFont = new Font("Arial", 32, FontStyle.Bold);
        using var titleBrush = new SolidBrush(Color.Yellow);
        using var titleFormat = CreateCenteredFormat();
        DrawScaledText(g, "Ask the Audience", titleFont, titleBrush,
            titleBounds.X, titleBounds.Y, titleBounds.Width, titleBounds.Height, titleFormat);

        // Draw vote bars
        float yOffset = 100;
        float barWidth = 650;
        float barHeight = 60;

        foreach (var kvp in _ataVotes.OrderBy(x => x.Key))
        {
            var barBounds = new RectangleF(overlayBounds.X + 50, overlayBounds.Y + yOffset, barWidth, barHeight);
            var scaledBarBounds = ScaleRect(barBounds.X, barBounds.Y, barBounds.Width, barBounds.Height);
            
            // Background
            using var barBgBrush = new SolidBrush(Color.FromArgb(100, 100, 100));
            g.FillRectangle(barBgBrush, scaledBarBounds);
            
            // Percentage bar
            float fillWidth = scaledBarBounds.Width * (kvp.Value / 100f);
            using var barBrush = new SolidBrush(Color.FromArgb(0, 120, 215));
            g.FillRectangle(barBrush, scaledBarBounds.X, scaledBarBounds.Y, fillWidth, scaledBarBounds.Height);
            
            // Text
            var text = $"{kvp.Key}: {kvp.Value}%";
            using var font = new Font("Arial", 26, FontStyle.Bold);
            using var textBrush = new SolidBrush(Color.White);
            DrawScaledText(g, text, font, textBrush,
                barBounds.X, barBounds.Y, barBounds.Width, barBounds.Height);
            
            yOffset += 80;
        }
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
        
        // Draw winning amount centered on screen
        var designBounds = new RectangleF(200, 400, 1520, 280);
        using var font = new Font("Copperplate Gothic Bold", 120, FontStyle.Bold);
        using var brush = new SolidBrush(Color.Gold);
        using var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        
        DrawScaledText(g, _gameWinnerAmount, font, brush,
            designBounds.X, designBounds.Y, designBounds.Width, designBounds.Height, format);
        
        // Draw "You Won" above
        using var titleFont = new Font("Copperplate Gothic Bold", 80, FontStyle.Bold);
        using var titleBrush = new SolidBrush(Color.White);
        var titleBounds = new RectangleF(200, 280, 1520, 100);
        DrawScaledText(g, "You Won", titleFont, titleBrush,
            titleBounds.X, titleBounds.Y, titleBounds.Width, titleBounds.Height, format);
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
            using var font = new Font("Copperplate Gothic Bold", 120, FontStyle.Bold);
            using var brush = new SolidBrush(Color.Gold);
            using var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            
            DrawScaledText(g, _fffWinnerName, font, brush,
                designBounds.X, designBounds.Y, designBounds.Width, designBounds.Height, format);
            
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
            
            for (int i = 0; i < _fffContestants.Count; i++)
            {
                var name = _fffContestants[i];
                bool isHighlighted = i == _fffHighlightedIndex;
                
                // Full-width strap bounds
                var designBounds = new RectangleF(0, currentY, strapWidth, strapHeight);
                
                // Determine strap image based on state
                Image? strapImage;
                if (isHighlighted)
                {
                    strapImage = FFFGraphics.GetFastestStrap(); // Yellow/gold highlight strap
                }
                else
                {
                    strapImage = FFFGraphics.GetIdleStrap(); // Normal blue strap
                }
                
                // Draw strap image
                var scaledBounds = ScaleRect(designBounds.X, designBounds.Y, designBounds.Width, designBounds.Height);
                if (strapImage != null)
                {
                    g.DrawImage(strapImage, scaledBounds);
                }
                else
                {
                    // Fallback: colored rectangle if image not found
                    Color bgColor = isHighlighted ? Color.Yellow : Color.FromArgb(0, 0, 102);
                    using var bgBrush = new SolidBrush(bgColor);
                    g.FillRectangle(bgBrush, scaledBounds);
                    
                    using var borderPen = new Pen(Color.White, 3);
                    g.DrawRectangle(borderPen, scaledBounds.X, scaledBounds.Y, scaledBounds.Width, scaledBounds.Height);
                }
                
                // Draw contestant name (left side of strap)
                // VB.NET position: X=379 (scaled to 570px for 1920 width)
                using var font = new Font("Copperplate Gothic Bold", 30, FontStyle.Bold); // Scaled from 20.25pt
                Color textColor = isHighlighted ? Color.Black : Color.White;
                using var brush = new SolidBrush(textColor);
                using var format = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
                
                // Name text bounds (left offset matching VB.NET layout)
                DrawScaledText(g, name, font, brush,
                    designBounds.X + 570, designBounds.Y, designBounds.Width - 1200, designBounds.Height, format);
                
                // Draw time (right side of strap) if available
                if (_fffTimes.Count > i && _fffTimes[i] > 0)
                {
                    using var timeFormat = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
                    var timeText = $"{_fffTimes[i]:F2}s";
                    DrawScaledText(g, timeText, font, brush,
                        designBounds.X + 1200, designBounds.Y, designBounds.Width - 1300, designBounds.Height, timeFormat);
                }
                
                currentY += spacing; // Move to next strap position
            }
        }
    }
    
    private void DrawLifelineIcons(System.Drawing.Graphics g)
    {
        // Design-time coordinates (1920x1080)
        // Position: Right edge, stacked vertically (1770, 36), spacing 82px vertical
        float baseX = 1770;
        float baseY = 36;
        float spacingY = 82;  // Vertical spacing for stacking
        float iconWidth = 72;  // Slightly smaller for TV screen
        float iconHeight = 44;
        
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

    #region IGameScreen Implementation
    
    public void Initialize(MoneyTreeService moneyTreeService)
    {
        _moneyTreeService = moneyTreeService;
        
        // Initialize background renderer with current settings
        // Try to get settings from Program.ServiceProvider
        try
        {
            var settingsManager = Program.ServiceProvider?.GetRequiredService<ApplicationSettingsManager>();
            if (settingsManager?.Settings != null)
            {
                _backgroundRenderer = new BackgroundRenderer(settingsManager.Settings);
            }
        }
        catch
        {
            // If DI not available, background renderer will remain null and fall back to black background
        }
    }
    
    public void UpdateMoneyTreeLevel(int level)
    {
        _currentMoneyTreeLevel = level;
        _useSafetyNetAltGraphic = false; // Reset to normal graphic
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
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => ShowGameWinner(amount, questionLevel)));
            return;
        }
        
        _showGameWinner = true;
        _gameWinnerAmount = amount;
        
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

