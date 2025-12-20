using MillionaireGame.Core.Models;
using MillionaireGame.Services;
using MillionaireGame.Core.Helpers;

namespace MillionaireGame.Forms;

/// <summary>
/// Guest screen - displays game information for contestants/audience
/// </summary>
public partial class GuestScreenForm : Form, IGameScreen
{
    private Question? _currentQuestion;

    public GuestScreenForm()
    {
        InitializeComponent();
        IconHelper.ApplyToForm(this);
        FormBorderStyle = FormBorderStyle.None;
        WindowState = FormWindowState.Maximized;
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
        // For now, this is a no-op as answers are shown by default on guest screen
    }

    public void ShowCorrectAnswerToHost(string correctAnswer)
    {
        // Guest screen does nothing - this is host-only
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

        if (isCorrect)
        {
            HighlightAnswer(selectedAnswer, Color.LimeGreen, Color.Black);
        }
        else
        {
            HighlightAnswer(selectedAnswer, Color.Red, Color.White);
            HighlightAnswer(correctAnswer, Color.LimeGreen, Color.Black);
        }
    }

    public void UpdateMoney(string current, string correct, string wrong, string drop, string questionsLeft)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => UpdateMoney(current, correct, wrong, drop, questionsLeft)));
            return;
        }

        lblWinningStrap.Text = $"Current: {current}";
    }

    public void ActivateLifeline(Lifeline lifeline)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => ActivateLifeline(lifeline)));
            return;
        }

        // Update lifeline display
        switch (lifeline.Type)
        {
            case LifelineType.FiftyFifty:
                picLifeline1.Visible = false;
                break;
            case LifelineType.PlusOne:
                picLifeline2.Visible = false;
                break;
            case LifelineType.AskTheAudience:
                picLifeline3.Visible = false;
                break;
            case LifelineType.SwitchQuestion:
                picLifeline4.Visible = false;
                break;
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
        lblWinningStrap.Text = string.Empty;
        ResetAnswerColors();
        
        // Reset lifelines
        picLifeline1.Visible = true;
        picLifeline2.Visible = true;
        picLifeline3.Visible = true;
        picLifeline4.Visible = true;
    }

    #endregion

    #region Helper Methods

    private void ResetAnswerColors()
    {
        var defaultColor = Color.FromArgb(30, 30, 60);
        
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
}
