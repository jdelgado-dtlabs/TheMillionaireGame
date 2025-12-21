using MillionaireGame.Core.Models;
using MillionaireGame.Services;
using MillionaireGame.Core.Helpers;
using MillionaireGame.Graphics;
using MillionaireGame.Controls;
using MillionaireGame.Core.Services;

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
    private MoneyTreeControl? _moneyTreeControl;
    private MoneyTreeService? _moneyTreeService;

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
    }

    protected override void RenderScreen(System.Drawing.Graphics g)
    {
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
        
        if (!_showQuestionAndAnswers) return;

        // Draw question strap (with or without text)
        DrawQuestionStrap(g);

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
        DrawAnswerBox(g, "A", _currentQuestion.AnswerA, _answerABounds, true, _visibleAnswers.Contains("A"));
        DrawAnswerBox(g, "B", _currentQuestion.AnswerB, _answerBBounds, false, _visibleAnswers.Contains("B"));
        DrawAnswerBox(g, "C", _currentQuestion.AnswerC, _answerCBounds, true, _visibleAnswers.Contains("C"));
        DrawAnswerBox(g, "D", _currentQuestion.AnswerD, _answerDBounds, false, _visibleAnswers.Contains("D"));

        // Draw ATA results if visible
        if (_showATA)
        {
            DrawATAResults(g);
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
            "Arial", 32, FontStyle.Bold, Color.White, textBounds, 2);
    }

    private void DrawAnswerBox(System.Drawing.Graphics g, string letter, string text, RectangleF bounds, bool isLeft, bool showText)
    {
        // Determine which texture to use
        var elementType = TextureManager.ElementType.AnswerLeftNormal;
        
        if (_isRevealing && _selectedAnswer == letter && _correctAnswer == letter)
        {
            elementType = isLeft ? TextureManager.ElementType.AnswerLeftCorrect : TextureManager.ElementType.AnswerRightCorrect;
        }
        else if (_selectedAnswer == letter)
        {
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
                "Arial", 24, FontStyle.Regular, Color.White, textBounds, 2, 
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
        using var font = new Font("Arial", 48, FontStyle.Bold);
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
        var treePosition = TextureManager.Instance.GetMoneyTreePosition(_currentMoneyTreeLevel);

        if (treeBase == null) return;

        // Calculate position - slide in from right during animation
        float treeWidth = 1280;
        float treeHeight = 720;
        float targetX = (1920 - treeWidth) / 2; // Center horizontally
        float targetY = (1080 - treeHeight) / 2; // Center vertically
        
        float currentX = targetX;
        if (_moneyTreeAnimating)
        {
            // Slide in from right
            float offscreenX = 1920; // Start position (off-screen right)
            currentX = offscreenX - (_moneyTreeAnimationProgress * (offscreenX - targetX));
        }

        // Draw base tree image
        DrawScaledImage(g, treeBase, currentX, targetY, treeWidth, treeHeight);

        // Draw position highlight overlay if level is set
        if (treePosition != null && _currentMoneyTreeLevel > 0)
        {
            // Position overlay area (matches VB.NET coordinates)
            float overlayX = currentX + 815 - (1280 - treeWidth) / 2;
            float overlayY = targetY + 100 - (720 - treeHeight) / 2;
            float overlayWidth = 399;
            float overlayHeight = 599;
            
            DrawScaledImage(g, treePosition, overlayX, overlayY, overlayWidth, overlayHeight);
        }

        // Draw money values and question numbers (from MoneyTreeService)
        if (_moneyTreeService != null)
        {
            DrawMoneyTreeText(g, currentX, targetY, treeWidth, treeHeight);
        }
    }

    private void DrawMoneyTreeText(System.Drawing.Graphics g, float baseX, float baseY, float width, float height)
    {
        var settings = _moneyTreeService!.Settings;
        
        // Text positions (adjusted from VB.NET coordinates: 855/832 for qno_pos_X, 910 for money_pos_X)
        float qnoBaseX = baseX + 855;
        float moneyBaseX = baseX + 910;
        
        // Y positions for each level (from VB.NET: money_pos_Y array)
        int[] yPositions = { 0, 662, 622, 582, 542, 502, 462, 422, 382, 342, 302, 262, 222, 182, 142, 102 };
        
        for (int level = 1; level <= 15; level++)
        {
            float y = baseY + yPositions[level] - (720 - height) / 2;
            float qnoX = (level >= 10) ? qnoBaseX - 23 : qnoBaseX; // Adjust for double digits
            
            // Determine text color based on level state
            Color textColor;
            if (level == _currentMoneyTreeLevel)
            {
                textColor = Color.Black; // Current level
            }
            else if (level == 5 || level == 10 || level == 15)
            {
                textColor = Color.White; // Safety nets and top prize
            }
            else if (level == settings.SafetyNet1 || level == settings.SafetyNet2)
            {
                textColor = Color.White; // Custom safety nets
            }
            else
            {
                textColor = Color.Gold; // Regular levels
            }
            
            using var font = new Font("Copperplate Gothic Bold", 24, FontStyle.Bold);
            using var brush = new SolidBrush(textColor);
            using var format = new StringFormat { Alignment = StringAlignment.Near };
            
            // Draw question number
            DrawScaledText(g, level.ToString(), font, brush, qnoX, y, 50, 40, format);
            
            // Draw money amount
            string formattedMoney = _moneyTreeService.GetFormattedValue(level);
            DrawScaledText(g, formattedMoney, font, brush, moneyBaseX, y, 350, 40, format);
        }
    }

    private void DrawATAResults(System.Drawing.Graphics g)
    {
        // Simple ATA display for now
        float barWidth = 150;
        float barSpacing = 200;
        float barY = 500;
        float maxBarHeight = 300;

        var answers = new[] { "A", "B", "C", "D" };
        for (int i = 0; i < answers.Length; i++)
        {
            var answer = answers[i];
            var votes = _ataVotes.ContainsKey(answer) ? _ataVotes[answer] : 0;
            var barHeight = (votes / 100f) * maxBarHeight;
            
            var barX = 400 + (i * barSpacing);
            var barRect = ScaleRect(barX, barY + maxBarHeight - barHeight, barWidth, barHeight);
            
            using var barBrush = new SolidBrush(Color.FromArgb(0, 150, 255));
            g.FillRectangle(barBrush, barRect);
            
            // Draw percentage
            using var font = new Font("Arial", 24, FontStyle.Bold);
            using var brush = new SolidBrush(Color.White);
            using var format = CreateCenteredFormat();
            
            DrawScaledText(g, $"{votes}%", font, brush,
                barX, barY + maxBarHeight + 20,
                barWidth, 40,
                format);
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
        _moneyTreeControl = new MoneyTreeControl(moneyTreeService);
        _moneyTreeControl.Location = new Point(Width - 270, 20);
        _moneyTreeControl.Size = new Size(250, 600);
        _moneyTreeControl.Visible = false; // Initially hidden until Show button is clicked
        Controls.Add(_moneyTreeControl);
    }
    
    public void UpdateMoneyTreeLevel(int level)
    {
        _currentMoneyTreeLevel = level;
        _moneyTreeControl?.SetCurrentLevel(level);
        if (_showMoneyTree)
        {
            Invalidate(); // Redraw if money tree is currently visible
        }
    }

    public void UpdateQuestion(Question question)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => UpdateQuestion(question)));
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
            Invoke(new Action(() => SelectAnswer(answer)));
            return;
        }

        _selectedAnswer = answer;
        Invalidate();
    }

    public void ShowAnswer(string answer)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => ShowAnswer(answer)));
            return;
        }

        _visibleAnswers.Add(answer);
        Invalidate();
    }

    public void ShowCorrectAnswerToHost(string correctAnswer)
    {
        // TV screen doesn't show this
    }

    public void ShowQuestion(bool show)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => ShowQuestion(show)));
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
            Invoke(new Action(() => ShowWinnings(state)));
            return;
        }

        // TV Screen: Clear all graphics and show money tree with animation
        _currentMoneyTreeLevel = state.CurrentLevel;
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
            Invoke(new Action(() => ShowWinningsAmount(amount)));
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
            Invoke(new Action(HideWinnings));
            return;
        }

        _currentAmount = null;
        _showWinnings = false;
        _showMoneyTree = false;
        Invalidate();
    }

    public void RevealAnswer(string selectedAnswer, string correctAnswer, bool isCorrect)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => RevealAnswer(selectedAnswer, correctAnswer, isCorrect)));
            return;
        }

        _selectedAnswer = selectedAnswer;
        _correctAnswer = correctAnswer;
        _isRevealing = true;
        Invalidate();
    }

    public void ResetScreen()
    {
        if (InvokeRequired)
        {
            Invoke(new Action(ResetScreen));
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
        Invalidate();
    }

    public void ShowATAResults(Dictionary<string, int> votes)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => ShowATAResults(votes)));
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
            Invoke(new Action(HideATAResults));
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
    
    #region Money Tree Show/Hide with Slide Animation
    
    /// <summary>
    /// Shows the money tree with slide-in animation from the right
    /// </summary>
    public async Task ShowMoneyTreeAsync()
    {
        if (_moneyTreeControl == null || _moneyTreeControl.Visible)
            return;
        
        // Position money tree off-screen to the right (1/3 of screen width = ~640px for 1920 width)
        int targetX = ClientSize.Width - 270; // Final position (20px margin from right)
        int startX = ClientSize.Width; // Start off-screen
        
        _moneyTreeControl.Location = new Point(startX, 50);
        _moneyTreeControl.Visible = true;
        
        // Animate slide in (60 steps, ~500ms total)
        int steps = 30;
        int deltaX = (startX - targetX) / steps;
        
        for (int i = 0; i < steps; i++)
        {
            _moneyTreeControl.Location = new Point(startX - (deltaX * (i + 1)), 50);
            await Task.Delay(16); // ~60 FPS
        }
        
        // Ensure final position
        _moneyTreeControl.Location = new Point(targetX, 50);
    }
    
    /// <summary>
    /// Hides the money tree with slide-out animation to the right
    /// </summary>
    public async Task HideMoneyTreeAsync()
    {
        if (_moneyTreeControl == null || !_moneyTreeControl.Visible)
            return;
        
        int startX = _moneyTreeControl.Location.X;
        int targetX = ClientSize.Width; // Slide off-screen to the right
        
        // Animate slide out
        int steps = 30;
        int deltaX = (targetX - startX) / steps;
        
        for (int i = 0; i < steps; i++)
        {
            _moneyTreeControl.Location = new Point(startX + (deltaX * (i + 1)), 50);
            await Task.Delay(16); // ~60 FPS
        }
        
        _moneyTreeControl.Visible = false;
    }
    
    public void ClearQuestionAndAnswerText()
    {
        // TV screen doesn't need this - it's only for host/guest screens
    }
    
    #endregion
}
