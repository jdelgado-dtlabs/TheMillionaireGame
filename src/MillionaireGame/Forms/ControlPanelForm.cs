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
    private readonly ScreenUpdateService _screenService;
    private readonly SoundService _soundService;
    private string _currentAnswer = string.Empty;

    // Screen forms
    private HostScreenForm? _hostScreen;
    private GuestScreenForm? _guestScreen;
    private TVScreenForm? _tvScreen;

    public ControlPanelForm(
        GameService gameService,
        ApplicationSettingsManager appSettings,
        QuestionRepository questionRepository,
        ScreenUpdateService screenService,
        SoundService soundService)
    {
        _gameService = gameService;
        _appSettings = appSettings;
        _questionRepository = questionRepository;
        _screenService = screenService;
        _soundService = soundService;
        
        // Load sounds from settings
        _soundService.LoadSoundsFromSettings(_appSettings.Settings);
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
        await LoadNewQuestion();
    }

    private async Task LoadNewQuestion()
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

            // Broadcast question to all screens
            _screenService.UpdateQuestion(question);

            // Play question cue sound
            _soundService.PlaySound(SoundEffect.QuestionCue);
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
        // Play lights down sound effect
        _soundService.PlaySound(SoundEffect.LightsDown);
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
        _soundService.PlaySound(SoundEffect.WalkAway);
        MessageBox.Show($"Total winnings: {_gameService.State.CurrentValue}", 
            "Walk Away", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void btnActivateRiskMode_Click(object? sender, EventArgs e)
    {
        var newMode = _gameService.State.Mode == Core.Models.GameMode.Normal
            ? Core.Models.GameMode.Risk
            : Core.Models.GameMode.Normal;

        _gameService.ChangeMode(newMode);
        
        if (newMode == Core.Models.GameMode.Risk)
        {
            _soundService.PlaySound(SoundEffect.RiskMode);
        }
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

    private void btn5050_Click(object? sender, EventArgs e)
    {
        // Check if lifeline is available
        var lifeline = _gameService.State.GetLifeline(Core.Models.LifelineType.FiftyFifty);
        if (lifeline == null || lifeline.IsUsed)
        {
            MessageBox.Show("50:50 lifeline has already been used!", "Lifeline Used", 
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Use the lifeline
        _gameService.UseLifeline(Core.Models.LifelineType.FiftyFifty);
        btn5050.Enabled = false;
        btn5050.BackColor = Color.Gray;

        // Play lifeline sound
        _soundService.PlaySound(SoundEffect.Lifeline5050);

        // Remove two wrong answers
        if (string.IsNullOrEmpty(lblAnswer.Text)) return;

        var wrongAnswers = new List<string> { "A", "B", "C", "D" };
        wrongAnswers.Remove(lblAnswer.Text);

        // Randomly select 2 wrong answers to remove
        var random = new Random();
        for (int i = 0; i < 2; i++)
        {
            var indexToRemove = random.Next(wrongAnswers.Count);
            var answerToRemove = wrongAnswers[indexToRemove];
            wrongAnswers.RemoveAt(indexToRemove);

            // Clear the answer text
            switch (answerToRemove)
            {
                case "A": txtA.Text = ""; break;
                case "B": txtB.Text = ""; break;
                case "C": txtC.Text = ""; break;
                case "D": txtD.Text = ""; break;
            }
        }

        _screenService.ActivateLifeline(lifeline);
    }

    private void btnPhoneFriend_Click(object? sender, EventArgs e)
    {
        var lifeline = _gameService.State.GetLifeline(Core.Models.LifelineType.PlusOne);
        if (lifeline == null || lifeline.IsUsed)
        {
            MessageBox.Show("Phone a Friend lifeline has already been used!", "Lifeline Used",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _gameService.UseLifeline(Core.Models.LifelineType.PlusOne);
        btnPhoneFriend.Enabled = false;
        btnPhoneFriend.BackColor = Color.Gray;

        // Play lifeline sound
        _soundService.PlaySound(SoundEffect.LifelinePhone);

        // Show a simple dialog for phone a friend
        var friendAnswer = MessageBox.Show(
            $"Your friend suggests answer: {lblAnswer.Text}\n\nThey are fairly confident about this.",
            "Phone a Friend",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);

        _screenService.ActivateLifeline(lifeline);
    }

    private void btnAskAudience_Click(object? sender, EventArgs e)
    {
        var lifeline = _gameService.State.GetLifeline(Core.Models.LifelineType.AskTheAudience);
        if (lifeline == null || lifeline.IsUsed)
        {
            MessageBox.Show("Ask the Audience lifeline has already been used!", "Lifeline Used",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _gameService.UseLifeline(Core.Models.LifelineType.AskTheAudience);
        btnAskAudience.Enabled = false;
        btnAskAudience.BackColor = Color.Gray;

        // Play lifeline sound
        _soundService.PlaySound(SoundEffect.LifelineATA);

        // Broadcast to screens to show ATA results
        _screenService.ActivateLifeline(lifeline);

        MessageBox.Show("Ask the Audience results are now displayed on the screens.",
            "Ask the Audience", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private async void btnSwitch_Click(object? sender, EventArgs e)
    {
        var lifeline = _gameService.State.GetLifeline(Core.Models.LifelineType.SwitchQuestion);
        if (lifeline == null || lifeline.IsUsed)
        {
            MessageBox.Show("Switch Question lifeline has already been used!", "Lifeline Used",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var result = MessageBox.Show(
            "Are you sure you want to switch to a new question?",
            "Switch Question",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result == DialogResult.Yes)
        {
            _gameService.UseLifeline(Core.Models.LifelineType.SwitchQuestion);
            btnSwitch.Enabled = false;
            btnSwitch.BackColor = Color.Gray;

            // Play lifeline sound
            _soundService.PlaySound(SoundEffect.LifelineSwitch);

            // Load a new question
            await LoadNewQuestion();

            _screenService.ActivateLifeline(lifeline);
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

        // Broadcast answer selection to all screens
        _screenService.SelectAnswer(answer);

        // Play final answer sound
        _soundService.PlaySound(SoundEffect.FinalAnswer);
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

            // Broadcast correct answer to all screens
            _screenService.RevealAnswer(_currentAnswer, lblAnswer.Text, true);

            // Play correct answer sound
            _soundService.PlaySound(SoundEffect.CorrectAnswer);
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

            // Broadcast wrong answer to all screens
            _screenService.RevealAnswer(_currentAnswer, lblAnswer.Text, false);

            // Play wrong answer sound
            _soundService.PlaySound(SoundEffect.WrongAnswer);
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

        // Reset lifeline buttons
        _gameService.State.ResetLifelines();
        btn5050.Enabled = true;
        btn5050.BackColor = Color.Orange;
        btnPhoneFriend.Enabled = true;
        btnPhoneFriend.BackColor = Color.Orange;
        btnAskAudience.Enabled = true;
        btnAskAudience.BackColor = Color.Orange;
        btnSwitch.Enabled = true;
        btnSwitch.BackColor = Color.Orange;
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
        if (_hostScreen == null || _hostScreen.IsDisposed)
        {
            _hostScreen = new HostScreenForm();
            _screenService.RegisterScreen(_hostScreen);
            _hostScreen.FormClosed += (s, args) => _screenService.UnregisterScreen(_hostScreen);
            _hostScreen.Show();
        }
        else
        {
            _hostScreen.BringToFront();
        }
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
        if (_guestScreen == null || _guestScreen.IsDisposed)
        {
            _guestScreen = new GuestScreenForm();
            _screenService.RegisterScreen(_guestScreen);
            _guestScreen.FormClosed += (s, args) => _screenService.UnregisterScreen(_guestScreen);
            _guestScreen.Show();
        }
        else
        {
            _guestScreen.BringToFront();
        }
    }

    private void TVScreenToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        if (_tvScreen == null || _tvScreen.IsDisposed)
        {
            _tvScreen = new TVScreenForm();
            _screenService.RegisterScreen(_tvScreen);
            _tvScreen.FormClosed += (s, args) => _screenService.UnregisterScreen(_tvScreen);
            _tvScreen.Show();
        }
        else
        {
            _tvScreen.BringToFront();
        }
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
