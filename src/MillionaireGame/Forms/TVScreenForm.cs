using MillionaireGame.Core.Models;
using MillionaireGame.Services;
using MillionaireGame.Core.Helpers;
using MillionaireGame.Controls;
using MillionaireGame.Core.Services;

namespace MillionaireGame.Forms;

/// <summary>
/// TV screen - public display screen for audience viewing
/// </summary>
public partial class TVScreenForm : Form, IGameScreen
{
    private Question? _currentQuestion;
    private System.Windows.Forms.Timer? _flashTimer;
    private int _flashStep = 0;
    private bool _flashState = false;
    private MoneyTreeControl? _moneyTreeControl;
    private MoneyTreeService? _moneyTreeService;

    public TVScreenForm()
    {
        InitializeComponent();
        IconHelper.ApplyToForm(this);
        FormBorderStyle = FormBorderStyle.None;
        WindowState = FormWindowState.Maximized;
        
        // Initialize flash timer for answer reveals
        _flashTimer = new System.Windows.Forms.Timer();
        _flashTimer.Interval = 500; // Flash every 500ms
        _flashTimer.Tick += FlashTimer_Tick;
    }

    public void Initialize(MoneyTreeService moneyTreeService)
    {
        _moneyTreeService = moneyTreeService;
        _moneyTreeControl = new MoneyTreeControl(moneyTreeService);
        _moneyTreeControl.Location = new Point(1050, 50);
        _moneyTreeControl.Size = new Size(250, 600);
        _moneyTreeControl.Visible = false; // Initially hidden until Show button is clicked
        Controls.Add(_moneyTreeControl);
    }

    public void UpdateMoneyTreeLevel(int level)
    {
        _moneyTreeControl?.SetCurrentLevel(level);
    }

    #region IGameScreen Implementation

    public void UpdateQuestion(Question question)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => UpdateQuestion(question)));
            return;
        }

        _currentQuestion = question;
        
        lblQuestion.Text = question.QuestionText;
        lblAnswerA.Text = question.AnswerA;
        lblAnswerB.Text = question.AnswerB;
        lblAnswerC.Text = question.AnswerC;
        lblAnswerD.Text = question.AnswerD;

        ResetAnswerColors();
        StopFlashing();
    }

    public void SelectAnswer(string answer)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => SelectAnswer(answer)));
            return;
        }

        ResetAnswerColors();

        switch (answer)
        {
            case "A":
                pnlAnswerA.BackColor = Color.Yellow;
                lblAnswerA.ForeColor = Color.Black;
                break;
            case "B":
                pnlAnswerB.BackColor = Color.Yellow;
                lblAnswerB.ForeColor = Color.Black;
                break;
            case "C":
                pnlAnswerC.BackColor = Color.Yellow;
                lblAnswerC.ForeColor = Color.Black;
                break;
            case "D":
                pnlAnswerD.BackColor = Color.Yellow;
                lblAnswerD.ForeColor = Color.Black;
                break;
        }
    }

    public void ShowAnswer(string answer)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => ShowAnswer(answer)));
            return;
        }

        // Make the specified answer visible (implementation depends on how answers are hidden initially)
        // For now, this is a no-op as answers are shown by default on TV screen
    }

    public void ShowCorrectAnswerToHost(string correctAnswer)
    {
        // TV screen does nothing - this is host-only
    }

    public void ShowQuestion(bool show)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => ShowQuestion(show)));
            return;
        }

        // Show or hide question and answers
        lblQuestion.Visible = show;
        pnlAnswerA.Visible = show;
        pnlAnswerB.Visible = show;
        pnlAnswerC.Visible = show;
        pnlAnswerD.Visible = show;
    }

    public void ShowWinnings(GameState state)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => ShowWinnings(state)));
            return;
        }

        // Hide question to show winnings
        ShowQuestion(false);
    }

    public void HideWinnings()
    {
        if (InvokeRequired)
        {
            Invoke(new Action(HideWinnings));
            return;
        }

        // Just hide - question will be shown when checkbox is checked
    }

    public void RevealAnswer(string selectedAnswer, string correctAnswer, bool isCorrect)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => RevealAnswer(selectedAnswer, correctAnswer, isCorrect)));
            return;
        }

        // Start flashing animation
        _flashStep = 0;
        _flashTimer?.Start();
    }

    public void UpdateMoney(string current, string correct, string wrong, string drop, string questionsLeft)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => UpdateMoney(current, correct, wrong, drop, questionsLeft)));
            return;
        }

        lblAmount.Text = current;
    }

    public void ActivateLifeline(Lifeline lifeline)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => ActivateLifeline(lifeline)));
            return;
        }

        // Show ATA results if Ask the Audience was used
        if (lifeline.Type == LifelineType.AskTheAudience && _currentQuestion != null)
        {
            pnlATA.Visible = true;
            lblATA_A.Text = $"A: {_currentQuestion.ATAPercentageA ?? 0}%";
            lblATA_B.Text = $"B: {_currentQuestion.ATAPercentageB ?? 0}%";
            lblATA_C.Text = $"C: {_currentQuestion.ATAPercentageC ?? 0}%";
            lblATA_D.Text = $"D: {_currentQuestion.ATAPercentageD ?? 0}%";
        }
    }

    public void ResetScreen()
    {
        if (InvokeRequired)
        {
            Invoke(new Action(ResetScreen));
            return;
        }

        lblQuestion.Text = string.Empty;
        lblAnswerA.Text = string.Empty;
        lblAnswerB.Text = string.Empty;
        lblAnswerC.Text = string.Empty;
        lblAnswerD.Text = string.Empty;
        lblAmount.Text = string.Empty;
        pnlATA.Visible = false;
        ResetAnswerColors();
        StopFlashing();
    }

    public void ClearQuestionAndAnswerText()
    {
        // TV screen doesn't need this - it's only for host/guest screens
    }

    #endregion

    #region Flash Animation

    private void FlashTimer_Tick(object? sender, EventArgs e)
    {
        if (_currentQuestion == null)
        {
            StopFlashing();
            return;
        }

        _flashState = !_flashState;
        _flashStep++;

        // Flash 6 times (3 cycles), then stop and show final result
        if (_flashStep > 6)
        {
            StopFlashing();
            ShowFinalAnswer();
            return;
        }

        // Animate the selected answer
        var selectedAnswer = GetSelectedAnswer();
        var correctAnswer = _currentQuestion.CorrectAnswer;
        
        if (_flashState)
        {
            // Flash on
            if (selectedAnswer == correctAnswer)
            {
                HighlightAnswer(selectedAnswer, Color.LimeGreen, Color.Black);
            }
            else
            {
                HighlightAnswer(selectedAnswer, Color.Red, Color.White);
            }
        }
        else
        {
            // Flash off - return to yellow
            HighlightAnswer(selectedAnswer, Color.Yellow, Color.Black);
        }
    }

    private void StopFlashing()
    {
        _flashTimer?.Stop();
        _flashStep = 0;
        _flashState = false;
    }

    private void ShowFinalAnswer()
    {
        if (_currentQuestion == null) return;

        var selectedAnswer = GetSelectedAnswer();
        var correctAnswer = _currentQuestion.CorrectAnswer;

        ResetAnswerColors();

        if (selectedAnswer == correctAnswer)
        {
            HighlightAnswer(selectedAnswer, Color.LimeGreen, Color.Black);
        }
        else
        {
            HighlightAnswer(selectedAnswer, Color.Red, Color.White);
            HighlightAnswer(correctAnswer, Color.LimeGreen, Color.Black);
        }
    }

    private string GetSelectedAnswer()
    {
        if (pnlAnswerA.BackColor == Color.Yellow || pnlAnswerA.BackColor == Color.Red || pnlAnswerA.BackColor == Color.LimeGreen)
            return "A";
        if (pnlAnswerB.BackColor == Color.Yellow || pnlAnswerB.BackColor == Color.Red || pnlAnswerB.BackColor == Color.LimeGreen)
            return "B";
        if (pnlAnswerC.BackColor == Color.Yellow || pnlAnswerC.BackColor == Color.Red || pnlAnswerC.BackColor == Color.LimeGreen)
            return "C";
        if (pnlAnswerD.BackColor == Color.Yellow || pnlAnswerD.BackColor == Color.Red || pnlAnswerD.BackColor == Color.LimeGreen)
            return "D";
        return "";
    }

    #endregion

    #region Helper Methods

    private void ResetAnswerColors()
    {
        var defaultColor = Color.FromArgb(0, 0, 80);
        
        pnlAnswerA.BackColor = defaultColor;
        pnlAnswerB.BackColor = defaultColor;
        pnlAnswerC.BackColor = defaultColor;
        pnlAnswerD.BackColor = defaultColor;

        lblAnswerA.ForeColor = Color.White;
        lblAnswerB.ForeColor = Color.White;
        lblAnswerC.ForeColor = Color.White;
        lblAnswerD.ForeColor = Color.White;
    }

    private void HighlightAnswer(string answer, Color backgroundColor, Color textColor)
    {
        switch (answer)
        {
            case "A":
                pnlAnswerA.BackColor = backgroundColor;
                lblAnswerA.ForeColor = textColor;
                break;
            case "B":
                pnlAnswerB.BackColor = backgroundColor;
                lblAnswerB.ForeColor = textColor;
                break;
            case "C":
                pnlAnswerC.BackColor = backgroundColor;
                lblAnswerC.ForeColor = textColor;
                break;
            case "D":
                pnlAnswerD.BackColor = backgroundColor;
                lblAnswerD.ForeColor = textColor;
                break;
        }
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
        
        int targetX = ClientSize.Width - 270;
        int startX = ClientSize.Width;
        
        _moneyTreeControl.Location = new Point(startX, 50);
        _moneyTreeControl.Visible = true;
        
        int steps = 30;
        int deltaX = (startX - targetX) / steps;
        
        for (int i = 0; i < steps; i++)
        {
            _moneyTreeControl.Location = new Point(startX - (deltaX * (i + 1)), 50);
            await Task.Delay(16);
        }
        
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
        int targetX = ClientSize.Width;
        
        int steps = 30;
        int deltaX = (targetX - startX) / steps;
        
        for (int i = 0; i < steps; i++)
        {
            _moneyTreeControl.Location = new Point(startX + (deltaX * (i + 1)), 50);
            await Task.Delay(16);
        }
        
        _moneyTreeControl.Visible = false;
    }
    
    #endregion

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _flashTimer?.Dispose();
            components?.Dispose();
        }
        base.Dispose(disposing);
    }
}
