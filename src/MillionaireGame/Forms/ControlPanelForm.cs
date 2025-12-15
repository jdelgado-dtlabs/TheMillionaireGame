using MillionaireGame.Core.Game;
using MillionaireGame.Core.Settings;
using MillionaireGame.Core.Database;
using MillionaireGame.Services;

namespace MillionaireGame.Forms;

/// <summary>
/// Main control panel for managing the game
/// </summary>
public partial class ControlPanelForm : Form
{
    private readonly GameService _gameService;
    private readonly ApplicationSettingsManager _appSettings;
    private readonly QuestionRepository _questionRepository;
    private readonly HotkeyHandler _hotkeyHandler;
    private string _currentAnswer = string.Empty;

    public ControlPanelForm(
        GameService gameService,
        ApplicationSettingsManager appSettings,
        QuestionRepository questionRepository)
    {
        _gameService = gameService;
        _appSettings = appSettings;
        _questionRepository = questionRepository;
// Initialize hotkey handler
        _hotkeyHandler = new HotkeyHandler(
            onF1: () => btnA.PerformClick(),
            onF2: () => btnB.PerformClick(),
            onF3: () => btnC.PerformClick(),
            onF4: () => btnD.PerformClick(),
            onF5: () => btnNewQuestion.PerformClick(),
            onF6: () => btnReveal.PerformClick(),
            onF7: () => btnLightsDown.PerformClick(),
            onPageUp: LevelUp,
            onPageDown: LevelDown,
            onDelete: () => btnResetGame.PerformClick(),
            onEnd: () => btnWalk.PerformClick(),
            onR: () => btnActivateRiskMode.PerformClick()
        );

        InitializeComponent();
        KeyPreview = true; // Enable key event capture
        InitializeComponent();
        SetupEventHandlers();
    }

    private void SetupEventHandlers()
    {
        // Subscribe to game service events
        _gameService.LevelChanged += OnLevelChanged;
        _gameService.ModeChanged += OnModeChanged;
        _gameService.LifelineUsed += OnLifelineUsed;
    }

    private void ControlPanelForm_Load(object? sender, EventArgs e)
    {
        // Initialize game to level 0
        _gameService.ChangeLevel(0);
        UpdateMoneyDisplay();
        
        // Load lifeline images based on settings
        // TODO: Implement lifeline image loading
        
        // Auto-show screens if configured
        if (_appSettings.Settings.AutoShowHostScreen)
        {
            // TODO: Show host screen
        }
        if (_appSettings.Settings.AutoShowGuestScreen)
        {
            // TODO: Show guest screen
        }
        if (_appSettings.Settings.AutoShowTVScreen)
        {
            // TODO: Show TV screen
        }
    }

    #region Event Handlers

    private void OnLevelChanged(object? sender, GameLevelChangedEventArgs e)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => OnLevelChanged(sender, e)));
            return;
        }

        nmrLevel.Value = e.NewLevel;
        UpdateMoneyDisplay();
    }

    private void OnModeChanged(object? sender, GameModeChangedEventArgs e)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => OnModeChanged(sender, e)));
            return;
        }

        btnActivateRiskMode.Text = e.NewMode == Core.Models.GameMode.Risk
            ? "RISK MODE ON"
            : "RISK MODE OFF";
        btnActivateRiskMode.BackColor = e.NewMode == Core.Models.GameMode.Risk
            ? Color.DarkGreen
            : Color.Orange;
    }

    private void OnLifelineUsed(object? sender, LifelineUsedEventArgs e)
    {
        // Update UI to show lifeline as used
        // TODO: Implement lifeline UI updates
    }

    #endregion

    #region Button Click Handlers

    private async void btnNewQuestion_Click(object? sender, EventArgs e)
    {
        try
        {
            var question = await _questionRepository.GetRandomQuestionAsync(
                _gameService.State.CurrentLevel);

            if (question == null)
            {
                MessageBox.Show(
                    "No unused questions available for this level!",
                    "No Questions",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Update UI with question
            txtQuestion.Text = question.QuestionText;
            txtA.Text = question.AnswerA;
            txtB.Text = question.AnswerB;
            txtC.Text = question.AnswerC;
            txtD.Text = question.AnswerD;
            lblAnswer.Text = question.CorrectAnswer;
            txtExplain.Text = question.Explanation;
            txtID.Text = question.Id.ToString();

            // Reset answer selection
            ResetAnswerColors();
            _currentAnswer = string.Empty;

            // Mark question as used
            await _questionRepository.MarkQuestionAsUsedAsync(question.Id);

            // TODO: Play question cue sound
            // TODO: Update other screens
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error loading question: {ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void btnA_Click(object? sender, EventArgs e)
    {
        SelectAnswer("A");
    }

    private void btnB_Click(object? sender, EventArgs e)
    {
        SelectAnswer("B");
    }

    private void btnC_Click(object? sender, EventArgs e)
    {
        SelectAnswer("C");
    }

    private void btnD_Click(object? sender, EventArgs e)
    {
        SelectAnswer("D");
    }

    private void btnLightsDown_Click(object? sender, EventArgs e)
    {
        // TODO: Play lights down sound effect
        // TODO: Dim screens
    }

    private void btnReveal_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_currentAnswer))
        {
            MessageBox.Show(
                "No answer selected!",
                "No Answer",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        bool isCorrect = _currentAnswer == lblAnswer.Text;
        RevealAnswer(isCorrect);
    }

    private void btnWalk_Click(object? sender, EventArgs e)
    {
        _gameService.State.WalkAway = true;
        // TODO: Play walk away sound
        // TODO: Show total winnings
    }

    private void btnActivateRiskMode_Click(object? sender, EventArgs e)
    {
        var newMode = _gameService.State.Mode == Core.Models.GameMode.Normal
            ? Core.Models.GameMode.Risk
            : Core.Models.GameMode.Normal;

        _gameService.ChangeMode(newMode);
    }

    private void btnResetGame_Click(object? sender, EventArgs e)
    {
        var result = MessageBox.Show(
            "Are you sure you want to reset the game?",
            "Reset Game",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result == DialogResult.Yes)
        {
            _gameService.ResetGame();
            ResetAllControls();
        }
    }

    #endregion

    #region Helper Methods

    private void SelectAnswer(string answer)
    {
        ResetAnswerColors();
        _currentAnswer = answer;

        // Highlight selected answer
        switch (answer)
        {
            case "A":
                txtA.BackColor = Color.Yellow;
                break;
            case "B":
                txtB.BackColor = Color.Yellow;
                break;
            case "C":
                txtC.BackColor = Color.Yellow;
                break;
            case "D":
                txtD.BackColor = Color.Yellow;
                break;
        }

        // TODO: Update other screens with selected answer
        // TODO: Play final answer sound
    }

    private void ResetAnswerColors()
    {
        txtA.BackColor = Color.Silver;
        txtB.BackColor = Color.Silver;
        txtC.BackColor = Color.Silver;
        txtD.BackColor = Color.Silver;
    }

    private void RevealAnswer(bool isCorrect)
    {
        if (isCorrect)
        {
            // Correct answer
            switch (_currentAnswer)
            {
                case "A": txtA.BackColor = Color.LimeGreen; break;
                case "B": txtB.BackColor = Color.LimeGreen; break;
                case "C": txtC.BackColor = Color.LimeGreen; break;
                case "D": txtD.BackColor = Color.LimeGreen; break;
            }

            // Advance to next level
            _gameService.ChangeLevel(_gameService.State.CurrentLevel + 1);

            // TODO: Play correct answer sound
            // TODO: Update screens
        }
        else
        {
            // Wrong answer
            switch (_currentAnswer)
            {
                case "A": txtA.BackColor = Color.Red; break;
                case "B": txtB.BackColor = Color.Red; break;
                case "C": txtC.BackColor = Color.Red; break;
                case "D": txtD.BackColor = Color.Red; break;
            }

            // Highlight correct answer
            switch (lblAnswer.Text)
            {
                case "A": txtA.BackColor = Color.LimeGreen; break;
                case "B": txtB.BackColor = Color.LimeGreen; break;
                case "C": txtC.BackColor = Color.LimeGreen; break;
                case "D": txtD.BackColor = Color.LimeGreen; break;
            }

            // TODO: Play wrong answer sound
            // TODO: Show game over
        }
    }

    private void UpdateMoneyDisplay()
    {
        var state = _gameService.State;
        txtCorrect.Text = state.CorrectValue;
        txtCurrent.Text = state.CurrentValue;
        txtWrong.Text = state.WrongValue;
        txtDrop.Text = state.DropValue;
        txtQLeft.Text = state.QuestionsLeft;
    }

    private void ResetAllControls()
    {
        txtQuestion.Clear();
        txtA.Clear();
        txtB.Clear();
        txtC.Clear();
        txtD.Clear();
        txtExplain.Clear();
        lblAnswer.Text = string.Empty;
        txtID.Clear();
        ResetAnswerColors();
        _currentAnswer = string.Empty;
        nmrLevel.Value = 0;
    }

    #endregion

    #region Menu Handlers

    private void DatabaseToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        // TODO: Open database settings
    }

    private void QuestionsEditorToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        // TODO: Open questions editor
    }

    private void HostScreenToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        // TODO: Show host screen
    }
#region Hotkey Handling

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (_hotkeyHandler.ProcessKeyPress(keyData))
        {
            return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void LevelUp()
    {
        if (nmrLevel.Value < 15)
        {
            nmrLevel.Value++;
        }
    }

    private void LevelDown()
    {
        if (nmrLevel.Value > 0)
        {
            nmrLevel.Value--;
        }
    }

    #endregion

    
    private void GuestScreenToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        // TODO: Show guest screen
    }

    private void TVScreenToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        // TODO: Show TV screen
    }

    private void CloseToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        Close();
    }

    #endregion

    private void nmrLevel_ValueChanged(object? sender, EventArgs e)
    {
        _gameService.ChangeLevel((int)nmrLevel.Value);
    }
}
