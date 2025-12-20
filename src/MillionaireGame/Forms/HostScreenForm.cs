using MillionaireGame.Core.Models;
using MillionaireGame.Services;
using MillionaireGame.Core.Helpers;

namespace MillionaireGame.Forms;

/// <summary>
/// Host screen - displays game information for the host
/// </summary>
public partial class HostScreenForm : Form, IGameScreen
{
    private Question? _currentQuestion;

    public HostScreenForm()
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
        
        // Update question text
        lblQuestion.Text = question.QuestionText;
        
        // Update answers
        lblAnswerA.Text = question.AnswerA;
        lblAnswerB.Text = question.AnswerB;
        lblAnswerC.Text = question.AnswerC;
        lblAnswerD.Text = question.AnswerD;

        // Update correct answer indicator (shown only to host)
        lblCorrectAnswer.Text = $"Correct: {question.CorrectAnswer}";
        lblCorrectAnswer.Visible = false; // Hidden until all answers are revealed

        // Update ATA percentages (shown to host before lifeline activation)
        lblATAa.Text = $"A: {question.ATAPercentageA ?? 0}%";
        lblATAb.Text = $"B: {question.ATAPercentageB ?? 0}%";
        lblATAc.Text = $"C: {question.ATAPercentageC ?? 0}%";
        lblATAd.Text = $"D: {question.ATAPercentageD ?? 0}%";

        // Reset answer highlighting
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

        // Highlight selected answer
        switch (answer)
        {
            case "A":
                pnlAnswerA.BackColor = Color.Yellow;
                break;
            case "B":
                pnlAnswerB.BackColor = Color.Yellow;
                break;
            case "C":
                pnlAnswerC.BackColor = Color.Yellow;
                break;
            case "D":
                pnlAnswerD.BackColor = Color.Yellow;
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
        // For now, this is a no-op as answers are shown by default on host screen
    }

    public void ShowCorrectAnswerToHost(string correctAnswer)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => ShowCorrectAnswerToHost(correctAnswer)));
            return;
        }

        // Show correct answer on host screen
        lblCorrectAnswer.Visible = true;
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

        // Hide question and show money values prominently
        ShowQuestion(false);
        // TODO: Could add special winnings display panel
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
            // Show correct answer in green
            HighlightAnswer(selectedAnswer, Color.LimeGreen);
        }
        else
        {
            // Show wrong answer in red, correct in green
            HighlightAnswer(selectedAnswer, Color.Red);
            HighlightAnswer(correctAnswer, Color.LimeGreen);
        }
    }

    public void UpdateMoney(string current, string correct, string wrong, string drop, string questionsLeft)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => UpdateMoney(current, correct, wrong, drop, questionsLeft)));
            return;
        }

        lblCurrent.Text = current;
        lblCorrect.Text = correct;
        lblWrong.Text = wrong;
        lblDrop.Text = drop;
        lblQLeft.Text = questionsLeft;
    }

    public void ActivateLifeline(Lifeline lifeline)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => ActivateLifeline(lifeline)));
            return;
        }

        // Update lifeline status display
        // TODO: Visual indication of which lifeline was used
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
        lblCorrectAnswer.Text = string.Empty;
        ResetAnswerColors();
    }

    #endregion

    #region Helper Methods

    private void ResetAnswerColors()
    {
        pnlAnswerA.BackColor = Color.FromArgb(30, 30, 60);
        pnlAnswerB.BackColor = Color.FromArgb(30, 30, 60);
        pnlAnswerC.BackColor = Color.FromArgb(30, 30, 60);
        pnlAnswerD.BackColor = Color.FromArgb(30, 30, 60);
    }

    private void HighlightAnswer(string answer, Color color)
    {
        switch (answer)
        {
            case "A":
                pnlAnswerA.BackColor = color;
                break;
            case "B":
                pnlAnswerB.BackColor = color;
                break;
            case "C":
                pnlAnswerC.BackColor = color;
                break;
            case "D":
                pnlAnswerD.BackColor = color;
                break;
        }
    }

    #endregion
}
