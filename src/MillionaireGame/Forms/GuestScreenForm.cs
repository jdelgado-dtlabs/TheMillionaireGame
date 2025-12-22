using MillionaireGame.Core.Models;
using MillionaireGame.Services;
using MillionaireGame.Core.Helpers;
using MillionaireGame.Graphics;
using MillionaireGame.Core.Services;

namespace MillionaireGame.Forms;

/// <summary>
/// Guest screen with scalable rendering - shown to contestants and audience
/// </summary>
public class GuestScreenForm : ScalableScreenBase, IGameScreen
{
    private Question? _currentQuestion;
    private string? _selectedAnswer;
    private string? _correctAnswer;
    private bool _isRevealing;
    private bool _showATA;
    private Dictionary<string, int> _ataVotes = new();
    private string? _currentAmount;
    private HashSet<string> _visibleAnswers = new();
    private int _currentMoneyTreeLevel = 0;
    private MoneyTreeService? _moneyTreeService;

    // Design-time coordinates (based on 1920x1080, matching TV screen layout)
    // Question in upper part of lower third
    private readonly RectangleF _questionStrapBounds = new(0, 650, 1920, 120);
    // Answers with full width boxes and minimal center gap
    private readonly RectangleF _answerABounds = new(0, 800, 950, 100);
    private readonly RectangleF _answerBBounds = new(970, 800, 950, 100);
    private readonly RectangleF _answerCBounds = new(0, 920, 950, 100);
    private readonly RectangleF _answerDBounds = new(970, 920, 950, 100);

    public GuestScreenForm()
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
        
        Text = "Guest screen";
        BackColor = Color.Black;
        
        // Save initial state for fullscreen toggle
        SaveWindowState();
    }

    public void Initialize(MoneyTreeService moneyTreeService)
    {
        _moneyTreeService = moneyTreeService;
    }

    public void UpdateMoneyTreeLevel(int level)
    {
        _currentMoneyTreeLevel = level;
        Invalidate(); // Redraw to update money tree
    }

    protected override void RenderScreen(System.Drawing.Graphics g)
    {
        // Draw graphical money tree in right 1/4 width and upper 2/3 height (always visible)
        DrawMoneyTreeGraphical(g);
        
        // Always draw question strap (with or without text)
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

        // Always draw answer backgrounds, showing visible answers
        DrawAnswerBox(g, "A", _currentQuestion.AnswerA, _answerABounds, true, _visibleAnswers.Contains("A"));
        DrawAnswerBox(g, "B", _currentQuestion.AnswerB, _answerBBounds, false, _visibleAnswers.Contains("B"));
        DrawAnswerBox(g, "C", _currentQuestion.AnswerC, _answerCBounds, true, _visibleAnswers.Contains("C"));
        DrawAnswerBox(g, "D", _currentQuestion.AnswerD, _answerDBounds, false, _visibleAnswers.Contains("D"));

        // Draw ATA results if active
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



    private void DrawAnswerBox(System.Drawing.Graphics g, string letter, string text, RectangleF bounds, bool isLeftSide, bool isVisible)
    {
        // Determine if this is a left or right answer
        bool isLeft = (letter == "A" || letter == "C");
        
        // Determine texture based on state
        TextureManager.ElementType elementType;
        
        if (_isRevealing && letter == _correctAnswer)
        {
            elementType = isLeft ? TextureManager.ElementType.AnswerLeftCorrect : TextureManager.ElementType.AnswerRightCorrect;
        }
        else if (_selectedAnswer == letter && !_isRevealing)
        {
            elementType = isLeft ? TextureManager.ElementType.AnswerLeftFinal : TextureManager.ElementType.AnswerRightFinal;
        }
        else if (_isRevealing && _selectedAnswer == letter && letter != _correctAnswer)
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
        if (isVisible)
        {
            // Left answers (A, C) have more padding, right answers (B, D) have less
            float letterLeftPadding = isLeft ? 150 : 40;
            float textLeftPadding = isLeft ? 240 : 130;
            float textRightPadding = 80;
            
            // Draw answer letter
            using var letterFont = new Font("Arial", 28, FontStyle.Bold);
            using var letterBrush = new SolidBrush(Color.White);
            using var letterFormat = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            
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
        // Cropped tree image is 630x720 (aspect ratio 0.875)
        // Display at 650px height to avoid question strap blocking bottom
        float treeHeight = 650;
        float treeWidth = treeHeight * (630f / 720f); // Maintain aspect ratio: ~569px
        float treeX = 1920 - treeWidth; // Right edge
        float treeY = 0; // Top of screen

        // Get base money tree image
        var treeBase = TextureManager.GetTexture(TextureManager.ElementType.MoneyTreeBase, CurrentTextureSet);
        var treePosition = TextureManager.Instance.GetMoneyTreePosition(_currentMoneyTreeLevel);

        if (treeBase == null) return;

        // Draw base tree
        DrawScaledImage(g, treeBase, treeX, treeY, treeWidth, treeHeight);

        // Draw position highlight overlay at proper position
        if (treePosition != null && _currentMoneyTreeLevel > 0)
        {
            // Overlay positioned relative to cropped tree
            // Original overlay was at (815, 100) in 1280x720, which is (165, 100) in cropped 630x720
            // Scale proportionally based on tree height
            float scale = treeHeight / 720f;
            float overlayX = treeX + (165 * scale);
            float overlayY = treeY + (100 * scale);
            float overlayWidth = 399 * scale;
            float overlayHeight = 599 * scale;
            
            DrawScaledImage(g, treePosition, overlayX, overlayY, overlayWidth, overlayHeight);
        }

        // Draw money values and question numbers
        DrawMoneyTreeText(g, treeX, treeY, treeWidth, treeHeight);
    }

    private void DrawMoneyTreeText(System.Drawing.Graphics g, float baseX, float baseY, float width, float height)
    {
        var settings = _moneyTreeService!.Settings;
        
        // VB.NET Y positions for text (in original 720px height)
        int[] originalYPositions = new int[]
        {
            0,   // Placeholder for index 0 (not used)
            662, // Level 1
            622, // Level 2
            582, // Level 3
            542, // Level 4
            502, // Level 5
            462, // Level 6
            422, // Level 7
            382, // Level 8
            342, // Level 9
            302, // Level 10
            262, // Level 11
            222, // Level 12
            182, // Level 13
            142, // Level 14
            102  // Level 15
        };
        
        // VB.NET X positions in original 1280x720 canvas
        // money_pos_X = 910, qno_pos_X = 855 (levels 1-9) or 832 (levels 10-15)
        // After cropping 650px from left: money_pos_X = 260, qno_pos_X = 205 or 182
        
        // Scale factor for current tree size vs original cropped 630x720
        float scale = height / 720f;
        
        for (int level = 15; level >= 1; level--) // Draw from top (15) to bottom (1)
        {
            float y = baseY + (originalYPositions[level] * scale);
            
            // X position for question number (adjusted for crop)
            float qnoX = baseX + ((level >= 10 ? 182 : 205) * scale);
            // X position for money value (adjusted for crop)
            float moneyX = baseX + (260 * scale);
            
            // Determine text color
            Color textColor;
            if (level == _currentMoneyTreeLevel)
            {
                textColor = Color.Black; // Current level - highlighted by overlay
            }
            else if (level == 5 || level == 10 || level == 15)
            {
                textColor = Color.White; // Milestone levels
            }
            else if (level == settings.SafetyNet1 || level == settings.SafetyNet2)
            {
                textColor = Color.White; // Custom safety nets
            }
            else
            {
                textColor = Color.Gold; // Regular levels
            }
            
            using var font = new Font("Copperplate Gothic Bold", 24 * scale, FontStyle.Bold);
            using var brush = new SolidBrush(textColor);
            
            // Draw question number
            DrawScaledText(g, level.ToString(), font, brush, qnoX, y, 100, 100, null);
            
            // Draw money amount
            string formattedMoney = _moneyTreeService.GetFormattedValue(level);
            DrawScaledText(g, formattedMoney, font, brush, moneyX, y, 350, 100, null);
        }
    }

    private void DrawATAResults(System.Drawing.Graphics g)
    {
        if (_ataVotes.Count == 0) return;

        var overlayBounds = new RectangleF(600, 500, 720, 400);
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

    // IGameScreen implementation
    public void UpdateQuestion(Question question)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => UpdateQuestion(question)));
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
            Invoke(new Action(() => SelectAnswer(answer)));
            return;
        }

        _selectedAnswer = answer;
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

    public void ShowCorrectAnswerToHost(string? correctAnswer)
    {
        // Guest screen doesn't show correct answer to host
    }

    public void ShowQuestion(bool show)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => ShowQuestion(show)));
            return;
        }

        // Question strap is always visible, just trigger repaint
        Invalidate();
    }

    public void ShowWinnings(GameState state)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => ShowWinnings(state)));
            return;
        }

        // Money tree is always visible, nothing to do
        Invalidate();
    }

    public void HideWinnings()
    {
        if (InvokeRequired)
        {
            Invoke(new Action(HideWinnings));
            return;
        }

        // Money tree is always visible, nothing to hide
        Invalidate();
    }

    public void UpdateMoney(string current, string correct, string wrong, string drop, string questionsLeft)
    {
        // Guest screen shows money tree control instead
    }

    public void ActivateLifeline(Lifeline lifeline)
    {
        // Could show visual effects for lifeline activation
        if (lifeline.Type == LifelineType.AskTheAudience)
        {
            _showATA = true;
            // Generate ATA results based on question percentages
            if (_currentQuestion != null)
            {
                _ataVotes = new Dictionary<string, int>
                {
                    ["A"] = _currentQuestion.ATAPercentageA ?? 0,
                    ["B"] = _currentQuestion.ATAPercentageB ?? 0,
                    ["C"] = _currentQuestion.ATAPercentageC ?? 0,
                    ["D"] = _currentQuestion.ATAPercentageD ?? 0
                };
                Invalidate();
            }
        }
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
        // Straps remain always visible
        Invalidate();
    }

    public void ClearQuestionAndAnswerText()
    {
        if (InvokeRequired)
        {
            Invoke(new Action(ClearQuestionAndAnswerText));
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
}
