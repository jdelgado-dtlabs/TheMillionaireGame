using MillionaireGame.Core.Game;
using MillionaireGame.Core.Settings;
using MillionaireGame.Core.Database;
using MillionaireGame.Core.Models;
using MillionaireGame.Services;
using MillionaireGame.Core.Helpers;

namespace MillionaireGame.Forms;

/// <summary>
/// Lifeline mode states
/// </summary>
public enum LifelineMode
{
    Inactive,  // Before Explain Game (default orange)
    Demo,      // During Explain Game (yellow)
    Active     // After Lights Down (green, then grey when used)
}

/// <summary>
/// PAF (Phone a Friend) stages
/// </summary>
public enum PAFStage
{
    NotStarted,     // PAF not activated
    CallingIntro,   // Blue - playing intro loop
    CountingDown,   // Red - 30 second countdown active
    Completed       // Grey - finished/used
}

/// <summary>
/// ATA (Ask the Audience) stages
/// </summary>
public enum ATAStage
{
    NotStarted,     // ATA not activated
    Intro,          // Blue - 2 min intro/explanation
    Voting,         // Grey - 1 min voting period
    Completed       // Grey - finished/used
}

/// <summary>
/// Game outcome states for determining final winnings
/// </summary>
public enum GameOutcome
{
    InProgress,     // Game is still ongoing
    Win,            // Player won Q15
    Drop,           // Player walked away
    Wrong           // Player answered incorrectly
}

/// <summary>
/// Main control panel for managing the game
/// </summary>
public partial class ControlPanelForm : Form
{
    private readonly GameService _gameService;
    private readonly ApplicationSettingsManager _appSettings;
    private readonly SqlSettingsManager _sqlSettings;
    private readonly QuestionRepository _questionRepository;
    private readonly HotkeyHandler _hotkeyHandler;
    private readonly ScreenUpdateService _screenService;
    private readonly SoundService _soundService;
    private string _currentAnswer = string.Empty;
    private LifelineMode _lifelineMode = LifelineMode.Inactive;
    
    // Question reveal state tracking
    private int _answerRevealStep = 0; // 0 = not started, 1 = question shown, 2-5 = answers A-D shown
    private Question? _currentQuestion = null; // Store current question for progressive reveal
    
    // Game outcome tracking
    private GameOutcome _gameOutcome = GameOutcome.InProgress;
    
    // PAF lifeline state tracking
    private PAFStage _pafStage = PAFStage.NotStarted;
    private int _pafLifelineNumber = 0; // Which button slot is PAF
    private System.Windows.Forms.Timer? _pafTimer;
    private int _pafSecondsRemaining = 30;
    
    // ATA lifeline state tracking
    private ATAStage _ataStage = ATAStage.NotStarted;
    private int _ataLifelineNumber = 0; // Which button slot is ATA
    private System.Windows.Forms.Timer? _ataTimer;
    private int _ataSecondsRemaining = 120; // Start with 2 min for intro
    
    // Closing sequence state tracking
    private System.Windows.Forms.Timer? _closingTimer;
    private bool _closingInProgress = false;
    
    // Auto-reset cancellation from Thanks for Playing
    private System.Threading.CancellationTokenSource? _autoResetCancellation;
    
    // Track if at least one round has been completed
    private bool _firstRoundCompleted = false;

    // Screen forms
    private HostScreenForm? _hostScreen;
    private GuestScreenForm? _guestScreen;
    private TVScreenForm? _tvScreen;

    public ControlPanelForm(
        GameService gameService,
        ApplicationSettingsManager appSettings,
        SqlSettingsManager sqlSettings,
        QuestionRepository questionRepository,
        ScreenUpdateService screenService,
        SoundService soundService)
    {
        _gameService = gameService;
        _appSettings = appSettings;
        _sqlSettings = sqlSettings;
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
        IconHelper.ApplyToForm(this);
        KeyPreview = true; // Enable key event capture
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
        
        // Set lifelines to inactive mode (disabled, not clickable)
        SetLifelineMode(LifelineMode.Inactive);
        
        // Update lifeline button labels based on settings
        UpdateLifelineButtonLabels();
        
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

    #region Checkbox Event Handlers

    private void chkShowQuestion_CheckedChanged(object? sender, EventArgs e)
    {
        if (chkShowQuestion.Checked && chkShowWinnings.Checked)
        {
            // Uncheck winnings when question is checked
            chkShowWinnings.CheckedChanged -= chkShowWinnings_CheckedChanged;
            chkShowWinnings.Checked = false;
            chkShowWinnings.CheckedChanged += chkShowWinnings_CheckedChanged;
        }
        
        // Show or hide question on screens
        _screenService.ShowQuestion(chkShowQuestion.Checked);
    }

    private void chkShowWinnings_CheckedChanged(object? sender, EventArgs e)
    {
        if (chkShowWinnings.Checked && chkShowQuestion.Checked)
        {
            // Uncheck question when winnings is checked
            chkShowQuestion.CheckedChanged -= chkShowQuestion_CheckedChanged;
            chkShowQuestion.Checked = false;
            chkShowQuestion.CheckedChanged += chkShowQuestion_CheckedChanged;
        }
        
        // Show or hide winnings on screens
        if (chkShowWinnings.Checked)
        {
            _screenService.ShowWinnings(_gameService.State);
        }
        else
        {
            _screenService.HideWinnings();
        }
    }

    #endregion

    #region Button Click Handlers

    private async void btnNewQuestion_Click(object? sender, EventArgs e)
    {
        if (_answerRevealStep == 0)
        {
            // First click: Load question but don't show answers yet
            await LoadNewQuestion();
            _answerRevealStep = 1; // Question shown, no answers
            
            // Enable checkbox and keep Question button enabled for answer reveals
            chkShowQuestion.Checked = true;
            btnNewQuestion.Enabled = true;
            btnNewQuestion.BackColor = Color.LimeGreen;
            btnNewQuestion.Text = "Question";
        }
        else if (_answerRevealStep >= 1 && _answerRevealStep <= 4)
        {
            // Subsequent clicks: Reveal answers one by one
            RevealNextAnswer();
        }
    }

    private void RevealNextAnswer()
    {
        if (_currentQuestion == null) return;

        switch (_answerRevealStep)
        {
            case 1: // Reveal answer A
                txtA.Text = _currentQuestion.AnswerA;
                _screenService.ShowAnswer("A");
                _answerRevealStep = 2;
                break;
            case 2: // Reveal answer B
                txtB.Text = _currentQuestion.AnswerB;
                _screenService.ShowAnswer("B");
                _answerRevealStep = 3;
                break;
            case 3: // Reveal answer C
                txtC.Text = _currentQuestion.AnswerC;
                _screenService.ShowAnswer("C");
                _answerRevealStep = 4;
                break;
            case 4: // Reveal answer D
                txtD.Text = _currentQuestion.AnswerD;
                _screenService.ShowAnswer("D");
                _answerRevealStep = 5; // All answers revealed
                
                // Show correct answer after all answers are revealed
                lblAnswer.Visible = true;
                
                // If checkbox is checked, show correct answer to host screen
                if (chkCorrectAnswer.Checked)
                {
                    _screenService.ShowCorrectAnswerToHost(lblAnswer.Text);
                }
                
                // Enable Walk Away button once all answers are revealed (green)
                btnWalk.Enabled = true;
                btnWalk.BackColor = Color.LimeGreen;
                btnWalk.ForeColor = Color.Black;
                
                // Disable Question button after all answers shown
                btnNewQuestion.Enabled = false;
                btnNewQuestion.BackColor = Color.Gray;
                break;
        }
    }

    private async Task LoadNewQuestion()
    {
        try
        {
            // Use the question number directly from the control (user may have manually set it)
            var currentQuestion = (int)nmrLevel.Value + 1; // Convert 0-indexed to 1-indexed
            
            // Map current question number to difficulty level (1-4)
            // Q1-5 = Level 1, Q6-10 = Level 2, Q11-14 = Level 3, Q15 = Level 4
            var difficultyLevel = currentQuestion switch
            {
                >= 1 and <= 5 => 1,
                >= 6 and <= 10 => 2,
                >= 11 and <= 14 => 3,
                15 => 4,
                _ => 1 // Default to level 1
            };
            
            if (Program.DebugMode)
            {
                Console.WriteLine($"[Question] Requesting question #{currentQuestion} (difficulty level {difficultyLevel})");
            }
            
            // Try Range type first (most common), fall back to Specific if needed
            // Pass currentQuestion (not difficultyLevel) because GetLevelRangeString expects question numbers 1-15
            var question = await _questionRepository.GetRandomQuestionAsync(currentQuestion, Core.Models.DifficultyType.Range);
            if (question == null)
            {
                question = await _questionRepository.GetRandomQuestionAsync(currentQuestion, Core.Models.DifficultyType.Specific);
            }

            if (question == null)
            {
                // Get diagnostic info about questions in database
                var (total, unused) = await _questionRepository.GetQuestionCountAsync(difficultyLevel);
                
                if (Program.DebugMode)
                {
                    Console.WriteLine($"[Question] No unused questions found for difficulty level {difficultyLevel}");
                    Console.WriteLine($"[Question] Database has {total} total questions at level {difficultyLevel} ({unused} unused)");
                }
                
                MessageBox.Show(
                    $"No unused questions available for difficulty level {difficultyLevel}!\n\n" +
                    $"Database contains:\n" +
                    $"  • {total} total questions at level {difficultyLevel}\n" +
                    $"  • {unused} unused questions\n\n" +
                    $"Use Database menu to reset questions or add new ones.",
                    "No Questions",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (Program.DebugMode)
            {
                Console.WriteLine($"[Question] Loaded question ID {question.Id}: {question.QuestionText.Substring(0, Math.Min(50, question.QuestionText.Length))}...");
            }

            // Store question for progressive reveal
            _currentQuestion = question;

            // Update UI with question only - answers will be revealed progressively
            txtQuestion.Text = question.QuestionText;
            txtA.Clear(); // Will be revealed on first answer click
            txtB.Clear(); // Will be revealed on second answer click
            txtC.Clear(); // Will be revealed on third answer click
            txtD.Clear(); // Will be revealed on fourth answer click
            lblAnswer.Text = question.CorrectAnswer;
            lblAnswer.Visible = false; // Hide until all answers revealed
            txtExplain.Text = question.Explanation;
            txtID.Text = question.Id.ToString();

            // Reset answer selection
            ResetAnswerColors();
            _currentAnswer = string.Empty;
            _answerRevealStep = 0; // Reset for new question
            
            // Enable answer buttons now that a question is loaded and set to orange
            btnA.Enabled = true;
            btnA.BackColor = Color.DarkOrange;
            btnB.Enabled = true;
            btnB.BackColor = Color.DarkOrange;
            btnC.Enabled = true;
            btnC.BackColor = Color.DarkOrange;
            btnD.Enabled = true;
            btnD.BackColor = Color.DarkOrange;

            // Mark question as used
            await _questionRepository.MarkQuestionAsUsedAsync(question.Id);

            // Broadcast question to all screens
            _screenService.UpdateQuestion(question);

            // Get question number for audio logic
            var questionNumber = (int)nmrLevel.Value + 1;
            
            // Question button remains enabled for progressive reveal
            // Will be disabled after all 4 answers are shown
            
            // Enable Walk Away (yellow) after Q2 is presented
            if (questionNumber >= 2)
            {
                btnWalk.Enabled = true;
                btnWalk.BackColor = Color.Yellow;
                btnWalk.ForeColor = Color.Black;
            }
            
            // Enable Closing (green) from Q6 onwards to allow ending show mid-game
            if (questionNumber >= 6)
            {
                btnClosing.Enabled = true;
                btnClosing.BackColor = Color.LimeGreen;
                btnClosing.ForeColor = Color.Black;
            }
            
            // For Q1-5: Don't stop lights down or restart bed music (continuous quick round)
            // For Q6+: Stop lights down, then play question-specific bed music
            if (questionNumber >= 6)
            {
                _soundService.StopSound("lights_down");
                PlayQuestionBed();
            }
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

    private async void btnLightsDown_Click(object? sender, EventArgs e)
    {
        // Disable Explain Game immediately (grey)
        btnExplainGame.Enabled = false;
        btnExplainGame.BackColor = Color.Gray;
        
        // Stop any playing sounds first
        _soundService.StopAllSounds();
        
        // Play question-specific lights down sound
        PlayLightsDownSound();
        
        var questionNumber = (int)nmrLevel.Value + 1;
        
        // For Q1-5, enable Reset button on first lights down and play bed music after delay
        if (questionNumber >= 1 && questionNumber <= 5)
        {
            // Enable Reset button (red) on first lights down
            btnResetGame.Enabled = true;
            btnResetGame.BackColor = Color.Red;
            
            // Disable risk mode button after first lights down
            btnActivateRiskMode.Enabled = false;
            
            // Wait for lights down sound to finish before starting bed music
            await Task.Delay(4000);
            PlayQuestionBed();
            
            // Emulate first Question button click: load question and prepare for progressive reveal
            // This ensures consistent state management between Q1-5 and Q6+
            await LoadNewQuestion();
            _answerRevealStep = 1; // Question shown, ready to reveal answers
            chkShowQuestion.Checked = true;
        }
        else
        {
            // For Q6+, reset answer reveal state so Question button works properly
            _answerRevealStep = 0;
            
            // Disable Show Winnings checkbox for Q6+ (show question instead)
            if (chkShowWinnings.Checked)
            {
                chkShowWinnings.Checked = false;
            }
        }
        
        // Disable Lights Down and make grey
        btnLightsDown.Enabled = false;
        btnLightsDown.BackColor = Color.Gray;
        
        // Enable Question button (green) for all questions after lights down
        btnNewQuestion.Enabled = true;
        btnNewQuestion.BackColor = Color.LimeGreen;
        btnNewQuestion.ForeColor = Color.Black;
        
        // Enter active mode - lifelines turn green
        SetLifelineMode(LifelineMode.Active);
        
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

    private async void btnWalk_Click(object? sender, EventArgs e)
    {
        _gameService.State.WalkAway = true;
        _gameOutcome = GameOutcome.Drop; // Track that player walked away
        
        // Stop all sounds before playing quit sound
        _soundService.StopAllSounds();
        
        // Use current question level to determine which quit sound to play
        var questionNumber = (int)nmrLevel.Value + 1; // Convert 0-indexed to 1-indexed
        var quitSound = questionNumber <= 10 ? SoundEffect.QuitSmall : SoundEffect.QuitLarge;
        
        _soundService.PlaySound(quitSound);
        MessageBox.Show($"Total winnings: {_gameService.State.CurrentValue}", 
            "Walk Away", MessageBoxButtons.OK, MessageBoxIcon.Information);
        
        // Show winnings on screens
        if (!chkShowWinnings.Checked)
        {
            chkShowWinnings.Checked = true;
        }
        
        // Disable Walk Away (grey)
        btnWalk.Enabled = false;
        btnWalk.BackColor = Color.Gray;
        
        // Disable Reset (grey) - round is over
        btnResetGame.Enabled = false;
        btnResetGame.BackColor = Color.Gray;
        
        // Disable lifelines immediately - round is over
        SetLifelineMode(LifelineMode.Inactive);
        
        // Enable Thanks for Playing (green)
        btnThanksForPlaying.Enabled = true;
        btnThanksForPlaying.BackColor = Color.LimeGreen;
        btnThanksForPlaying.ForeColor = Color.Black;
        
        // Enable Closing (green)
        btnClosing.Enabled = true;
        btnClosing.BackColor = Color.LimeGreen;
        btnClosing.ForeColor = Color.Black;
    }

    private void btnActivateRiskMode_Click(object? sender, EventArgs e)
    {
        // Risk mode can only be activated at the start of the game
        if (_gameService.State.CurrentLevel > 0)
        {
            MessageBox.Show(
                "Risk Mode can only be activated at the beginning of the game, before the first question.",
                "Risk Mode",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }
        
        var newMode = _gameService.State.Mode == Core.Models.GameMode.Normal
            ? Core.Models.GameMode.Risk
            : Core.Models.GameMode.Normal;

        _gameService.ChangeMode(newMode);
        
        // Update button appearance based on mode
        if (newMode == Core.Models.GameMode.Risk)
        {
            btnActivateRiskMode.BackColor = Color.Red;
            btnActivateRiskMode.Text = "RISK MODE: ON";
            
            if (Program.DebugMode)
            {
                Console.WriteLine("[Risk Mode] Activated - No safety net at Q5/Q10, uses alternate sounds");
            }
        }
        else
        {
            btnActivateRiskMode.BackColor = Color.Yellow;
            btnActivateRiskMode.Text = "Activate Risk Mode";
            
            if (Program.DebugMode)
            {
                Console.WriteLine("[Risk Mode] Deactivated - Normal mode restored");
            }
        }
    }

    private void btnResetGame_Click(object? sender, EventArgs e)
    {
        // Show confirmation dialog (Reset only active during game play)
        var result = MessageBox.Show(
            "Are you sure you want to reset the game?",
            "Reset Game",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result == DialogResult.Yes)
        {
            _soundService.StopAllSounds();
            
            // Reset PAF state
            _pafTimer?.Stop();
            _pafTimer?.Dispose();
            _pafTimer = null;
            _pafStage = PAFStage.NotStarted;
            _pafLifelineNumber = 0;
            _pafSecondsRemaining = 30;
            
            // Reset ATA state
            _ataTimer?.Stop();
            _ataTimer?.Dispose();
            _ataTimer = null;
            _ataStage = ATAStage.NotStarted;
            _ataLifelineNumber = 0;
            _ataSecondsRemaining = 120;
            
            _gameService.ResetGame();
            _firstRoundCompleted = true; // Mark that at least one round has been completed
            ResetAllControls();
        }
    }

    private void btn5050_Click(object? sender, EventArgs e)
    {
        HandleLifelineClick(1, btn5050);
    }

    private void btnPhoneFriend_Click(object? sender, EventArgs e)
    {
        HandleLifelineClick(2, btnPhoneFriend);
    }

    private void btnAskAudience_Click(object? sender, EventArgs e)
    {
        HandleLifelineClick(3, btnAskAudience);
    }

    private async void btnSwitch_Click(object? sender, EventArgs e)
    {
        await HandleLifelineClickAsync(4, btnSwitch);
    }

    private Core.Models.LifelineType GetLifelineTypeFromSettings(int lifelineNumber)
    {
        var lifelineType = lifelineNumber switch
        {
            1 => _appSettings.Settings.Lifeline1,
            2 => _appSettings.Settings.Lifeline2,
            3 => _appSettings.Settings.Lifeline3,
            4 => _appSettings.Settings.Lifeline4,
            _ => "5050"
        };

        return lifelineType.ToLower() switch
        {
            "5050" => Core.Models.LifelineType.FiftyFifty,
            "plusone" => Core.Models.LifelineType.PlusOne,
            "ata" => Core.Models.LifelineType.AskTheAudience,
            "switch" => Core.Models.LifelineType.SwitchQuestion,
            _ => Core.Models.LifelineType.FiftyFifty
        };
    }

    private string GetLifelineDisplayName(int lifelineNumber)
    {
        var type = GetLifelineTypeFromSettings(lifelineNumber);
        return type switch
        {
            Core.Models.LifelineType.FiftyFifty => "50:50",
            Core.Models.LifelineType.PlusOne => "PAF",
            Core.Models.LifelineType.AskTheAudience => "ATA",
            Core.Models.LifelineType.SwitchQuestion => "Switch",
            _ => $"LL{lifelineNumber}"
        };
    }

    private void UpdateLifelineButtonLabels()
    {
        InitializeLifelineButtons();
    }

    /// <summary>
    /// Initializes lifeline buttons based on settings: sets visibility, labels, and default state
    /// </summary>
    private void InitializeLifelineButtons()
    {
        int totalLifelines = _appSettings.Settings.TotalLifelines;
        
        // Get labels for all 4 buttons
        var label1 = GetLifelineDisplayName(1);
        var label2 = GetLifelineDisplayName(2);
        var label3 = GetLifelineDisplayName(3);
        var label4 = GetLifelineDisplayName(4);
        
        if (Program.DebugMode)
        {
            Console.WriteLine($"[Lifelines] Initializing {totalLifelines} lifeline button(s):");
            Console.WriteLine($"  Button 1: {label1} (Type: {_appSettings.Settings.Lifeline1}) - Visible: {totalLifelines >= 1}");
            Console.WriteLine($"  Button 2: {label2} (Type: {_appSettings.Settings.Lifeline2}) - Visible: {totalLifelines >= 2}");
            Console.WriteLine($"  Button 3: {label3} (Type: {_appSettings.Settings.Lifeline3}) - Visible: {totalLifelines >= 3}");
            Console.WriteLine($"  Button 4: {label4} (Type: {_appSettings.Settings.Lifeline4}) - Visible: {totalLifelines >= 4}");
        }
        
        // Button 1 - always visible if TotalLifelines >= 1
        btn5050.Text = label1;
        btn5050.Visible = totalLifelines >= 1;
        
        // Button 2
        btnPhoneFriend.Text = label2;
        btnPhoneFriend.Visible = totalLifelines >= 2;
        
        // Button 3
        btnAskAudience.Text = label3;
        btnAskAudience.Visible = totalLifelines >= 3;
        
        // Button 4
        btnSwitch.Text = label4;
        btnSwitch.Visible = totalLifelines >= 4;
    }

    private void HandleLifelineClick(int lifelineNumber, Button button)
    {
        // Demo mode - just show on screens and play numbered ping
        if (_lifelineMode == LifelineMode.Demo)
        {
            var pingSound = lifelineNumber switch
            {
                1 => SoundEffect.LifelinePing1,
                2 => SoundEffect.LifelinePing2,
                3 => SoundEffect.LifelinePing3,
                4 => SoundEffect.LifelinePing4,
                _ => SoundEffect.LifelinePing1
            };
            _soundService.PlaySound(pingSound);
            
            var lifelineTypeName = GetLifelineTypeFromSettings(lifelineNumber) switch
            {
                Core.Models.LifelineType.FiftyFifty => "50:50",
                Core.Models.LifelineType.PlusOne => "Phone a Friend",
                Core.Models.LifelineType.AskTheAudience => "Ask the Audience",
                Core.Models.LifelineType.SwitchQuestion => "Switch Question",
                _ => "Unknown"
            };
            
            MessageBox.Show($"Lifeline {lifelineNumber} ({lifelineTypeName}) demonstrated on screens", "Demo Mode", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // Active mode - check if this is PAF or ATA and handle multi-stage behavior
        var type = GetLifelineTypeFromSettings(lifelineNumber);
        
        if (type == Core.Models.LifelineType.PlusOne && _pafStage != PAFStage.NotStarted)
        {
            HandlePAFStageClick(button);
            return;
        }
        
        if (type == Core.Models.LifelineType.AskTheAudience && _ataStage != ATAStage.NotStarted)
        {
            HandleATAStageClick(button);
            return;
        }
        
        var lifeline = _gameService.State.GetLifeline(type);
        
        if (lifeline == null || lifeline.IsUsed)
        {
            MessageBox.Show($"This lifeline has already been used!", "Lifeline Used", 
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Execute lifeline based on type
        switch (type)
        {
            case Core.Models.LifelineType.FiftyFifty:
                ExecuteFiftyFifty(lifeline, button);
                break;
            case Core.Models.LifelineType.PlusOne:
                _pafLifelineNumber = lifelineNumber;
                ExecutePhoneFriend(lifeline, button);
                break;
            case Core.Models.LifelineType.AskTheAudience:
                _ataLifelineNumber = lifelineNumber;
                ExecuteAskAudience(lifeline, button);
                break;
        }
    }

    private async Task HandleLifelineClickAsync(int lifelineNumber, Button button)
    {
        // Demo mode - just show on screens and play numbered ping
        if (_lifelineMode == LifelineMode.Demo)
        {
            var pingSound = lifelineNumber switch
            {
                1 => SoundEffect.LifelinePing1,
                2 => SoundEffect.LifelinePing2,
                3 => SoundEffect.LifelinePing3,
                4 => SoundEffect.LifelinePing4,
                _ => SoundEffect.LifelinePing1
            };
            _soundService.PlaySound(pingSound);
            
            var lifelineTypeName = GetLifelineTypeFromSettings(lifelineNumber) switch
            {
                Core.Models.LifelineType.FiftyFifty => "50:50",
                Core.Models.LifelineType.PlusOne => "Phone a Friend",
                Core.Models.LifelineType.AskTheAudience => "Ask the Audience",
                Core.Models.LifelineType.SwitchQuestion => "Switch Question",
                _ => "Unknown"
            };
            
            MessageBox.Show($"Lifeline {lifelineNumber} ({lifelineTypeName}) demonstrated on screens", "Demo Mode", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // Active mode - execute the lifeline based on configured type
        var type = GetLifelineTypeFromSettings(lifelineNumber);
        var lifeline = _gameService.State.GetLifeline(type);
        
        if (lifeline == null || lifeline.IsUsed)
        {
            MessageBox.Show($"This lifeline has already been used!", "Lifeline Used", 
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Execute lifeline based on type
        switch (type)
        {
            case Core.Models.LifelineType.FiftyFifty:
                ExecuteFiftyFifty(lifeline, button);
                break;
            case Core.Models.LifelineType.PlusOne:
                ExecutePhoneFriend(lifeline, button);
                break;
            case Core.Models.LifelineType.AskTheAudience:
                ExecuteAskAudience(lifeline, button);
                break;
            case Core.Models.LifelineType.SwitchQuestion:
                await ExecuteSwitchQuestion(lifeline, button);
                break;
        }
    }

    private void ExecuteFiftyFifty(Core.Models.Lifeline lifeline, Button button)
    {
        _gameService.UseLifeline(lifeline.Type);
        button.Enabled = false;
        button.BackColor = Color.Gray;

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

    private void ExecutePhoneFriend(Core.Models.Lifeline lifeline, Button button)
    {
        // Stage 1: Start intro/calling sequence
        _pafStage = PAFStage.CallingIntro;
        button.BackColor = Color.Blue;
        
        // Play intro sound on loop with identifier for later stopping
        _soundService.PlaySound(SoundEffect.LifelinePAFStart, "paf_intro", loop: true);
        
        _screenService.ActivateLifeline(lifeline);
    }
    
    private void HandlePAFStageClick(Button button)
    {
        switch (_pafStage)
        {
            case PAFStage.CallingIntro:
                // Stage 2: Stop intro, start countdown
                _soundService.StopSound("paf_intro");
                _pafStage = PAFStage.CountingDown;
                button.BackColor = Color.Red;
                
                // Play countdown sound once
                _soundService.PlaySound(SoundEffect.LifelinePAFCountdown, "paf_countdown");
                
                // Start 30-second timer
                _pafSecondsRemaining = 30;
                _pafTimer = new System.Windows.Forms.Timer();
                _pafTimer.Interval = 1000; // 1 second
                _pafTimer.Tick += PAFTimer_Tick;
                _pafTimer.Start();
                break;
                
            case PAFStage.CountingDown:
                // Stage 3b: Early end
                EndPAFEarly(button);
                break;
                
            case PAFStage.Completed:
                // Already complete, do nothing
                break;
        }
    }
    
    private void PAFTimer_Tick(object? sender, EventArgs e)
    {
        _pafSecondsRemaining--;
        
        if (Program.DebugMode)
        {
            Console.WriteLine($"[PAF] Countdown: {_pafSecondsRemaining} seconds remaining");
        }
        
        if (_pafSecondsRemaining <= 0)
        {
            // Stage 3a: Time's up - complete PAF
            CompletePAF();
        }
    }
    
    private void EndPAFEarly(Button button)
    {
        // Stop countdown timer
        _pafTimer?.Stop();
        _pafTimer?.Dispose();
        _pafTimer = null;
        
        // Stop countdown sound if still playing
        _soundService.StopSound("paf_countdown");
        
        // Play early end sound
        _soundService.PlaySound(SoundEffect.LifelinePAFEndEarly);
        
        // Mark as completed
        CompletePAF();
    }
    
    private void CompletePAF()
    {
        // Stop timer if running
        _pafTimer?.Stop();
        _pafTimer?.Dispose();
        _pafTimer = null;
        
        // Mark PAF as used in game state
        _gameService.UseLifeline(Core.Models.LifelineType.PlusOne);
        
        // Update button state
        var button = GetPAFButton();
        if (button != null)
        {
            button.BackColor = Color.Gray;
            button.Enabled = false;
        }
        
        _pafStage = PAFStage.Completed;
    }
    
    private Button? GetPAFButton()
    {
        return _pafLifelineNumber switch
        {
            1 => btn5050,
            2 => btnPhoneFriend,
            3 => btnAskAudience,
            4 => btnSwitch,
            _ => null
        };
    }

    private void OldExecutePhoneFriend_Unused(Core.Models.Lifeline lifeline, Button button)
    {
        _gameService.UseLifeline(lifeline.Type);
        button.Enabled = false;
        button.BackColor = Color.Gray;

        // Play lifeline sound
        _soundService.PlaySound(SoundEffect.LifelinePhone);

        // Show a simple dialog for phone a friend
        MessageBox.Show(
            $"Your friend suggests answer: {lblAnswer.Text}\n\nThey are fairly confident about this.",
            "Phone a Friend",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);

        _screenService.ActivateLifeline(lifeline);
    }

    private void ExecuteAskAudience(Core.Models.Lifeline lifeline, Button button)
    {
        // Stage 1: Start intro/explanation (2 minutes)
        _ataStage = ATAStage.Intro;
        button.BackColor = Color.Blue;
        
        // Play intro sound
        _soundService.PlaySound(SoundEffect.LifelineATAStart, "ata_intro");
        
        // Start 2-minute timer
        _ataSecondsRemaining = 120;
        _ataTimer = new System.Windows.Forms.Timer();
        _ataTimer.Interval = 1000; // 1 second
        _ataTimer.Tick += ATATimer_Tick;
        _ataTimer.Start();
        
        _screenService.ActivateLifeline(lifeline);
    }
    
    private void HandleATAStageClick(Button button)
    {
        switch (_ataStage)
        {
            case ATAStage.Intro:
                // Stage 2: Start voting (1 minute)
                StartATAVoting(button);
                break;
                
            case ATAStage.Voting:
                // End voting early
                CompleteATA();
                break;
                
            case ATAStage.Completed:
                // Already complete, do nothing
                break;
        }
    }
    
    private void ATATimer_Tick(object? sender, EventArgs e)
    {
        _ataSecondsRemaining--;
        
        if (Program.DebugMode)
        {
            var stageName = _ataStage == ATAStage.Intro ? "Intro" : "Voting";
            Console.WriteLine($"[ATA] {stageName} Countdown: {_ataSecondsRemaining} seconds remaining");
        }
        
        if (_ataSecondsRemaining <= 0)
        {
            if (_ataStage == ATAStage.Intro)
            {
                // Intro time elapsed - auto-start voting
                var button = GetATAButton();
                if (button != null)
                {
                    StartATAVoting(button);
                }
            }
            else if (_ataStage == ATAStage.Voting)
            {
                // Voting complete
                CompleteATA();
            }
        }
    }
    
    private async void StartATAVoting(Button button)
    {
        // Stop and dispose the 2-minute intro timer
        _ataTimer?.Stop();
        _ataTimer?.Dispose();
        _ataTimer = null;
        
        if (Program.DebugMode)
        {
            Console.WriteLine("[ATA] Intro stage ended, starting voting stage");
        }
        
        // Change to voting stage
        _ataStage = ATAStage.Voting;
        button.BackColor = Color.Red;
        button.Enabled = true; // Keep clickable to end vote early
        
        // Play voting sound once with identifier, wait 500ms for overlap, then stop intro
        _soundService.PlaySound(SoundEffect.LifelineATAVote, "ata_vote");
        await Task.Delay(500);
        _soundService.StopSound("ata_intro");
        
        // Start 1-minute voting timer
        _ataSecondsRemaining = 60;
        _ataTimer = new System.Windows.Forms.Timer();
        _ataTimer.Interval = 1000;
        _ataTimer.Tick += ATATimer_Tick;
        _ataTimer.Start();
        
        if (Program.DebugMode)
        {
            Console.WriteLine("[ATA] Voting timer started - 60 seconds");
        }
    }
    
    private async void CompleteATA()
    {
        // Stop timer
        _ataTimer?.Stop();
        _ataTimer?.Dispose();
        _ataTimer = null;
        
        // Play end sound, wait 500ms for overlap, then stop any playing ATA sounds
        _soundService.PlaySound(SoundEffect.LifelineATAEnd);
        await Task.Delay(500);
        _soundService.StopSound("ata_intro");
        _soundService.StopSound("ata_vote");
        
        // Mark as used
        _gameService.UseLifeline(Core.Models.LifelineType.AskTheAudience);
        
        // Update button to grey and disabled
        var button = GetATAButton();
        if (button != null)
        {
            button.BackColor = Color.Gray;
            button.Enabled = false;
        }
        
        _ataStage = ATAStage.Completed;
        
        if (Program.DebugMode)
        {
            Console.WriteLine("[ATA] Completed and marked as used");
        }
    }
    
    private Button? GetATAButton()
    {
        return _ataLifelineNumber switch
        {
            1 => btn5050,
            2 => btnPhoneFriend,
            3 => btnAskAudience,
            4 => btnSwitch,
            _ => null
        };
    }

    private void OldExecuteAskAudience_Unused(Core.Models.Lifeline lifeline, Button button)
    {
        _gameService.UseLifeline(lifeline.Type);
        button.Enabled = false;
        button.BackColor = Color.Gray;

        // Play lifeline sound
        _soundService.PlaySound(SoundEffect.LifelineATA);

        // Broadcast to screens to show ATA results
        _screenService.ActivateLifeline(lifeline);

        MessageBox.Show("Ask the Audience results are now displayed on the screens.",
            "Ask the Audience", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private async Task ExecuteSwitchQuestion(Core.Models.Lifeline lifeline, Button button)
    {
        var result = MessageBox.Show(
            "Are you sure you want to switch to a new question?",
            "Switch Question",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result == DialogResult.Yes)
        {
            _gameService.UseLifeline(lifeline.Type);
            button.Enabled = false;
            button.BackColor = Color.Gray;

            // Play lifeline sound
            _soundService.PlaySound(SoundEffect.LifelineSwitch);

            // Load a new question
            await LoadNewQuestion();

            _screenService.ActivateLifeline(lifeline);
        }
    }

    private async void btnHostIntro_Click(object? sender, EventArgs e)
    {
        // Reset all questions to unused for new game
        await _questionRepository.ResetAllQuestionsAsync();
        
        // Disable Host Intro until closing
        btnHostIntro.Enabled = false;
        btnHostIntro.BackColor = Color.Gray;
        
        // Enable Pick Player (green)
        btnPickPlayer.Enabled = true;
        btnPickPlayer.BackColor = Color.LimeGreen;
        btnPickPlayer.ForeColor = Color.Black;
        
        // Play host entrance audio once
        _soundService.PlaySound(SoundEffect.HostEntrance, loop: false);
    }

    private void btnPickPlayer_Click(object? sender, EventArgs e)
    {
        // TODO: Open FFF Server dialog to pick contestant
        MessageBox.Show("FFF Server functionality will be implemented later.", 
            "Pick Player", MessageBoxButtons.OK, MessageBoxIcon.Information);
        
        // After picking player, disable Pick Player
        btnPickPlayer.Enabled = false;
        btnPickPlayer.BackColor = Color.Gray;
        
        // Enable Explain Game (green)
        btnExplainGame.Enabled = true;
        btnExplainGame.BackColor = Color.LimeGreen;
        btnExplainGame.ForeColor = Color.Black;
        
        // Enable Lights Down (green)
        btnLightsDown.Enabled = true;
        btnLightsDown.BackColor = Color.LimeGreen;
        btnLightsDown.ForeColor = Color.Black;
    }

    private void btnExplainGame_Click(object? sender, EventArgs e)
    {
        // Play game explanation audio on loop
        _soundService.PlaySound(SoundEffect.ExplainGame, loop: true);
        
        // Enter demo mode - lifelines turn yellow
        SetLifelineMode(LifelineMode.Demo);
    }

    private async void btnThanksForPlaying_Click(object? sender, EventArgs e)
    {
        // Use current question level to determine which walk away sound to play
        var questionNumber = (int)nmrLevel.Value + 1; // Convert 0-indexed to 1-indexed
        var walkAwaySound = questionNumber <= 10 ? SoundEffect.WalkAwaySmall : SoundEffect.WalkAwayLarge;
        
        _soundService.PlaySound(walkAwaySound, loop: false);
        
        // Determine winnings based on game outcome
        string winnings = _gameOutcome switch
        {
            GameOutcome.Win => _gameService.State.CorrectValue,   // Won Q15 - show the million
            GameOutcome.Drop => _gameService.State.DropValue,      // Walked away - show drop value
            GameOutcome.Wrong => _gameService.State.WrongValue,    // Answered wrong - show wrong value (0 or last milestone)
            _ => _gameService.State.CurrentValue                   // Fallback to current value
        };
        
        MessageBox.Show($"Total Winnings: {winnings}\n\nThanks for playing!", 
            "Game Over", MessageBoxButtons.OK, MessageBoxIcon.Information);
        
        // Show winnings on screens
        if (!chkShowWinnings.Checked)
        {
            chkShowWinnings.Checked = true;
        }
        
        // Disable Thanks for Playing (grey)
        btnThanksForPlaying.Enabled = false;
        btnThanksForPlaying.BackColor = Color.Gray;
        
        // Disable Reset (grey) - round is over
        btnResetGame.Enabled = false;
        btnResetGame.BackColor = Color.Gray;
        
        // Enable Closing (green)
        btnClosing.Enabled = true;
        btnClosing.BackColor = Color.LimeGreen;
        btnClosing.ForeColor = Color.Black;
        
        // Store the auto-reset task so we can cancel it if Closing is clicked
        _autoResetCancellation = new System.Threading.CancellationTokenSource();
        var cancellationToken = _autoResetCancellation.Token;
        
        // Wait for walk away sound to finish (approximately 5 seconds) + additional 5 seconds
        try
        {
            await Task.Delay(10000, cancellationToken);
        
            // Automatically reset for next player
            _soundService.StopAllSounds();
        
            // Reset PAF state
            _pafTimer?.Stop();
            _pafTimer?.Dispose();
            _pafTimer = null;
            _pafStage = PAFStage.NotStarted;
            _pafLifelineNumber = 0;
            _pafSecondsRemaining = 30;
        
            // Reset ATA state
            _ataTimer?.Stop();
            _ataTimer?.Dispose();
            _ataTimer = null;
            _ataStage = ATAStage.NotStarted;
            _ataLifelineNumber = 0;
            _ataSecondsRemaining = 120;
        
            _gameService.ResetGame();
            _firstRoundCompleted = true; // Mark that at least one round has been completed
            ResetAllControls();
        }
        catch (TaskCanceledException)
        {
            // Auto-reset was cancelled because Closing was clicked
            if (Program.DebugMode)
            {
                Console.WriteLine("[Thanks for Playing] Auto-reset cancelled - Closing was clicked");
            }
        }
        finally
        {
            _autoResetCancellation?.Dispose();
            _autoResetCancellation = null;
        }
    }

    private async void btnClosing_Click(object? sender, EventArgs e)
    {
        if (!_closingInProgress)
        {
            // First click - start closing sequence
            _closingInProgress = true;
            btnClosing.BackColor = Color.Red;
            
            // Cancel any pending auto-reset from Thanks for Playing
            _autoResetCancellation?.Cancel();
            _autoResetCancellation?.Dispose();
            _autoResetCancellation = null;
            
            // Disable Reset button to prevent interference (grey)
            btnResetGame.Enabled = false;
            btnResetGame.BackColor = Color.Gray;
            
            // Stop all sounds before starting closing sequence
            _soundService.StopAllSounds();
            
            // If round is active (Reset button enabled), play game over sound first
            if (btnResetGame.Enabled)
            {
                _soundService.PlaySoundFile(_appSettings.Settings.SoundGameOver, "game_over", loop: false);
                
                if (Program.DebugMode)
                {
                    Console.WriteLine("[Closing] Round in play - playing game_over.mp3 (5 seconds)");
                }
                
                // Wait for game over sound to finish (approximately 5 seconds)
                await Task.Delay(5000);
            }
            
            // Play closing underscore with identifier
            _soundService.PlaySoundFile(_appSettings.Settings.SoundCloseStart, "close_underscore", loop: false);
            
            // Start 150 second timer
            _closingTimer = new System.Windows.Forms.Timer();
            _closingTimer.Interval = 150000; // 150 seconds
            _closingTimer.Tick += (s, args) => CompleteClosing();
            _closingTimer.Start();
            
            if (Program.DebugMode)
            {
                Console.WriteLine("[Closing] Started closing sequence - 150 second timer");
            }
        }
        else
        {
            // Second click - expire timer immediately
            if (_closingTimer != null)
            {
                _closingTimer.Stop();
                _closingTimer.Dispose();
                _closingTimer = null;
            }
            
            if (Program.DebugMode)
            {
                Console.WriteLine("[Closing] Timer expired early by user");
            }
            
            CompleteClosing();
        }
    }
    
    private async void CompleteClosing()
    {
        // Stop and dispose timer if still running
        if (_closingTimer != null)
        {
            _closingTimer.Stop();
            _closingTimer.Dispose();
            _closingTimer = null;
        }
        
        if (Program.DebugMode)
        {
            Console.WriteLine("[Closing] Completing closing sequence");
        }
        
        // Play closing theme
        _soundService.PlaySound(SoundEffect.CloseTheme, loop: false);
        
        // Wait 500ms then stop close_underscore if still playing
        await Task.Delay(500);
        _soundService.StopSound("close_underscore");
        
        // Enable Host Intro (green) for next show
        btnHostIntro.Enabled = true;
        btnHostIntro.BackColor = Color.LimeGreen;
        btnHostIntro.ForeColor = Color.Black;
        
        // Disable all other broadcast buttons (grey)
        btnPickPlayer.Enabled = false;
        btnPickPlayer.BackColor = Color.Gray;
        
        btnExplainGame.Enabled = false;
        btnExplainGame.BackColor = Color.Gray;
        
        btnLightsDown.Enabled = false;
        btnLightsDown.BackColor = Color.Gray;
        
        btnNewQuestion.Enabled = false;
        btnNewQuestion.BackColor = Color.Gray;
        
        btnReveal.Enabled = false;
        btnReveal.BackColor = Color.Gray;
        
        btnWalk.Enabled = false;
        btnWalk.BackColor = Color.Gray;
        
        btnThanksForPlaying.Enabled = false;
        btnThanksForPlaying.BackColor = Color.Gray;
        
        btnClosing.Enabled = false;
        btnClosing.BackColor = Color.Gray;
        
        btnResetGame.Enabled = false;
        btnResetGame.BackColor = Color.Gray;
        
        btnActivateRiskMode.Enabled = true;
        btnActivateRiskMode.BackColor = Color.Yellow;
        btnActivateRiskMode.Text = "Activate Risk Mode";
        
        // Keep btnStopAudio enabled
    }

    private void btnStopAudio_Click(object? sender, EventArgs e)
    {
        // Stop all currently playing audio
        _soundService.StopAllSounds();
    }

    #endregion

    #region Helper Methods

    private async void SelectAnswer(string answer)
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
        
        // Disable all answer buttons to lock in the answer (final answer)
        btnA.Enabled = false;
        btnB.Enabled = false;
        btnC.Enabled = false;
        btnD.Enabled = false;
        
        // Enable Reveal button (green)
        btnReveal.Enabled = true;
        btnReveal.BackColor = Color.LimeGreen;
        btnReveal.ForeColor = Color.Black;
        
        // Disable Walk Away once an answer is selected (grey)
        btnWalk.Enabled = false;
        btnWalk.BackColor = Color.Gray;

        // Broadcast answer selection to all screens
        _screenService.SelectAnswer(answer);

        // Stop any existing final answer sound first to prevent overlapping
        _soundService.StopSound("final_answer");
        
        // Play final answer sound based on current question number
        PlayFinalAnswerSound();
        
        // For Q6-15, stop bed music 500ms after final answer starts
        var questionNumber = (int)nmrLevel.Value + 1;
        if (questionNumber >= 6)
        {
            await Task.Delay(500);
            _soundService.StopSound("bed_music"); // Stop the bed music
        }
    }

    private void ResetAnswerColors()
    {
        txtA.BackColor = Color.Silver;
        txtB.BackColor = Color.Silver;
        txtC.BackColor = Color.Silver;
        txtD.BackColor = Color.Silver;
    }

    private void PlayFinalAnswerSound()
    {
        var questionNumber = _gameService.State.CurrentLevel + 1; // Convert 0-indexed to 1-indexed
        
        if (Program.DebugMode)
        {
            Console.WriteLine($"[Sound] Looking for final answer sound for question #{questionNumber}");
        }
        
        var soundFile = questionNumber switch
        {
            1 => _appSettings.Settings.SoundQ1Final,
            2 => _appSettings.Settings.SoundQ1Final,
            3 => _appSettings.Settings.SoundQ1Final,
            4 => _appSettings.Settings.SoundQ1Final,
            5 => _appSettings.Settings.SoundQ1Final,
            6 => _appSettings.Settings.SoundQ6Final,
            7 => _appSettings.Settings.SoundQ7Final,
            8 => _appSettings.Settings.SoundQ8Final,
            9 => _appSettings.Settings.SoundQ9Final,
            10 => _appSettings.Settings.SoundQ10Final,
            11 => _appSettings.Settings.SoundQ11Final,
            12 => _appSettings.Settings.SoundQ12Final,
            13 => _appSettings.Settings.SoundQ13Final,
            14 => _appSettings.Settings.SoundQ14Final,
            15 => _appSettings.Settings.SoundQ15Final,
            _ => string.Empty
        };

        if (string.IsNullOrEmpty(soundFile))
        {
            if (Program.DebugMode)
            {
                Console.WriteLine($"[Sound] No final answer sound configured for Q{questionNumber} (sound file is empty)");
            }
            return;
        }
        
        if (Program.DebugMode)
        {
            Console.WriteLine($"[Sound] Playing final answer sound for Q{questionNumber}: {soundFile}");
        }
        
        _soundService.PlaySoundFile(soundFile, "final_answer");
    }

    private void PlayLoseSound(int? questionNumber = null)
    {
        var currentQuestion = questionNumber ?? (_gameService.State.CurrentLevel + 1); // Convert 0-indexed to 1-indexed
        
        if (Program.DebugMode)
        {
            Console.WriteLine($"[Sound] Looking for lose sound for question #{currentQuestion}");
        }
        
        var soundFile = currentQuestion switch
        {
            1 => _appSettings.Settings.SoundQ1to5Wrong,
            2 => _appSettings.Settings.SoundQ1to5Wrong,
            3 => _appSettings.Settings.SoundQ1to5Wrong,
            4 => _appSettings.Settings.SoundQ1to5Wrong,
            5 => _appSettings.Settings.SoundQ1to5Wrong,
            6 => _appSettings.Settings.SoundQ6Wrong,
            7 => _appSettings.Settings.SoundQ7Wrong,
            8 => _appSettings.Settings.SoundQ8Wrong,
            9 => _appSettings.Settings.SoundQ9Wrong,
            10 => _appSettings.Settings.SoundQ10Wrong,
            11 => _appSettings.Settings.SoundQ11Wrong,
            12 => _appSettings.Settings.SoundQ12Wrong,
            13 => _appSettings.Settings.SoundQ13Wrong,
            14 => _appSettings.Settings.SoundQ14Wrong,
            15 => _appSettings.Settings.SoundQ15Wrong,
            _ => string.Empty
        };

        if (string.IsNullOrEmpty(soundFile))
        {
            if (Program.DebugMode)
            {
                Console.WriteLine($"[Sound] No lose sound configured for Q{currentQuestion} (sound file is empty)");
            }
            return;
        }
        
        if (Program.DebugMode)
        {
            Console.WriteLine($"[Sound] Playing lose sound for Q{currentQuestion}: {soundFile}");
        }
        
        _soundService.PlaySoundFile(soundFile);
    }

    private void PlayCorrectSound(int? questionNumber = null)
    {
        // Use passed questionNumber or fall back to nmrLevel
        var currentQuestion = questionNumber ?? ((int)nmrLevel.Value + 1);
        var isRiskMode = _gameService.State.Mode == GameMode.Risk;
        
        if (Program.DebugMode)
        {
            Console.WriteLine($"[Sound] Looking for correct sound for question #{currentQuestion}, Risk Mode: {isRiskMode}");
        }
        
        var soundFile = currentQuestion switch
        {
            1 => _appSettings.Settings.SoundQ1to4Correct,
            2 => _appSettings.Settings.SoundQ1to4Correct,
            3 => _appSettings.Settings.SoundQ1to4Correct,
            4 => _appSettings.Settings.SoundQ1to4Correct,
            5 => isRiskMode ? _appSettings.Settings.SoundQ5Correct2 : _appSettings.Settings.SoundQ5Correct,
            6 => _appSettings.Settings.SoundQ6Correct,
            7 => _appSettings.Settings.SoundQ7Correct,
            8 => _appSettings.Settings.SoundQ8Correct,
            9 => _appSettings.Settings.SoundQ9Correct,
            10 => isRiskMode ? _appSettings.Settings.SoundQ10Correct2 : _appSettings.Settings.SoundQ10Correct,
            11 => _appSettings.Settings.SoundQ11Correct,
            12 => _appSettings.Settings.SoundQ12Correct,
            13 => _appSettings.Settings.SoundQ13Correct,
            14 => _appSettings.Settings.SoundQ14Correct,
            15 => _appSettings.Settings.SoundQ15Correct,
            _ => string.Empty
        };

        if (string.IsNullOrEmpty(soundFile))
        {
            if (Program.DebugMode)
            {
                Console.WriteLine($"[Sound] No correct sound configured for Q{currentQuestion} (sound file is empty)");
            }
            return;
        }
        
        if (Program.DebugMode)
        {
            Console.WriteLine($"[Sound] Playing correct sound for Q{currentQuestion}: {soundFile}");
        }
        
        _soundService.PlaySoundFile(soundFile);
    }

    private void PlayQuestionBed()
    {
        var questionNumber = (int)nmrLevel.Value + 1; // Use current control value, convert 0-indexed to 1-indexed
        
        if (Program.DebugMode)
        {
            Console.WriteLine($"[Sound] Looking for bed music for question #{questionNumber}");
        }
        
        var soundFile = questionNumber switch
        {
            1 => _appSettings.Settings.SoundQ1to5Bed,
            2 => _appSettings.Settings.SoundQ1to5Bed,
            3 => _appSettings.Settings.SoundQ1to5Bed,
            4 => _appSettings.Settings.SoundQ1to5Bed,
            5 => _appSettings.Settings.SoundQ1to5Bed,
            6 => _appSettings.Settings.SoundQ6Bed,
            7 => _appSettings.Settings.SoundQ7Bed,
            8 => _appSettings.Settings.SoundQ8Bed,
            9 => _appSettings.Settings.SoundQ9Bed,
            10 => _appSettings.Settings.SoundQ10Bed,
            11 => _appSettings.Settings.SoundQ11Bed,
            12 => _appSettings.Settings.SoundQ12Bed,
            13 => _appSettings.Settings.SoundQ13Bed,
            14 => _appSettings.Settings.SoundQ14Bed,
            15 => _appSettings.Settings.SoundQ15Bed,
            _ => string.Empty
        };

        if (string.IsNullOrEmpty(soundFile))
        {
            if (Program.DebugMode)
            {
                Console.WriteLine($"[Sound] No bed music configured for Q{questionNumber} (sound file is empty)");
            }
            return;
        }
        
        if (Program.DebugMode)
        {
            Console.WriteLine($"[Sound] Playing bed music for Q{questionNumber}: {soundFile}");
        }
        
        _soundService.PlaySoundFile(soundFile, "bed_music", loop: true);
    }

    private void PlayLightsDownSound()
    {
        var questionNumber = (int)nmrLevel.Value + 1; // Use current control value, convert 0-indexed to 1-indexed
        
        if (Program.DebugMode)
        {
            Console.WriteLine($"[Sound] Looking for lights down sound for question #{questionNumber}");
        }
        
        var soundFile = questionNumber switch
        {
            1 => _appSettings.Settings.SoundQ1to5LightsDown,
            2 => _appSettings.Settings.SoundQ1to5LightsDown,
            3 => _appSettings.Settings.SoundQ1to5LightsDown,
            4 => _appSettings.Settings.SoundQ1to5LightsDown,
            5 => _appSettings.Settings.SoundQ1to5LightsDown,
            6 => _appSettings.Settings.SoundQ6LightsDown,
            7 => _appSettings.Settings.SoundQ7LightsDown,
            8 => _appSettings.Settings.SoundQ8LightsDown,
            9 => _appSettings.Settings.SoundQ9LightsDown,
            10 => _appSettings.Settings.SoundQ10LightsDown,
            11 => _appSettings.Settings.SoundQ11LightsDown,
            12 => _appSettings.Settings.SoundQ12LightsDown,
            13 => _appSettings.Settings.SoundQ13LightsDown,
            14 => _appSettings.Settings.SoundQ14LightsDown,
            15 => _appSettings.Settings.SoundQ15LightsDown,
            _ => string.Empty
        };

        if (string.IsNullOrEmpty(soundFile))
        {
            if (Program.DebugMode)
            {
                Console.WriteLine($"[Sound] No lights down sound configured for Q{questionNumber} (sound file is empty)");
            }
            return;
        }
        
        if (Program.DebugMode)
        {
            Console.WriteLine($"[Sound] Playing lights down sound for Q{questionNumber}: {soundFile}");
        }
        
        _soundService.PlaySoundFile(soundFile, "lights_down");
    }

    private async void RevealAnswer(bool isCorrect)
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

            // Capture current question number BEFORE advancing level
            var currentQuestionNumber = (int)nmrLevel.Value + 1;

            // Advance to next level (but not beyond question 15)
            if (_gameService.State.CurrentLevel < 14)
            {
                _gameService.ChangeLevel(_gameService.State.CurrentLevel + 1);
            }

            // Broadcast correct answer to all screens
            _screenService.RevealAnswer(_currentAnswer, lblAnswer.Text, true);

            // Stop final answer sound 500ms before playing correct sound
            await Task.Delay(500);
            _soundService.StopSound("final_answer");
            
            // Play question-specific correct answer sound using captured question number
            PlayCorrectSound(currentQuestionNumber);
            
            // Auto-show winnings after 2 seconds
            await Task.Delay(2000);
            if (!chkShowWinnings.Checked)
            {
                chkShowWinnings.Checked = true;
            }
            
            // Disable Reveal button (grey)
            btnReveal.Enabled = false;
            btnReveal.BackColor = Color.Gray;
            
            // If Q5 was just answered correctly, stop the bed music and enable Lights Down for Q6
            if (currentQuestionNumber == 5)
            {
                await Task.Delay(1000);
                _soundService.StopSound("bed_music");
                
                // Enable Lights Down (green) for Q6
                btnLightsDown.Enabled = true;
                btnLightsDown.BackColor = Color.LimeGreen;
                btnLightsDown.ForeColor = Color.Black;
            }
            // For Q1-Q4, re-enable Question button (green) for next question
            else if (currentQuestionNumber >= 1 && currentQuestionNumber <= 4)
            {
                _answerRevealStep = 0; // Reset for next question
                btnNewQuestion.Enabled = true;
                btnNewQuestion.BackColor = Color.LimeGreen;
                btnNewQuestion.ForeColor = Color.Black;
                
                // Disable Walk Away for Q1-Q4 (grey) - only available after milestones
                btnWalk.Enabled = false;
                btnWalk.BackColor = Color.Gray;
            }
            // For Q6-Q14, enable Lights Down (green)
            else if (currentQuestionNumber >= 6 && currentQuestionNumber <= 14)
            {
                _answerRevealStep = 0; // Reset for next question
                btnLightsDown.Enabled = true;
                btnLightsDown.BackColor = Color.LimeGreen;
                btnLightsDown.ForeColor = Color.Black;
                
                // Disable Walk Away until next question is fully revealed (grey)
                btnWalk.Enabled = false;
                btnWalk.BackColor = Color.Gray;
            }
            // For Q15, enable Thanks for Playing (green)
            else if (currentQuestionNumber == 15)
            {
                _gameOutcome = GameOutcome.Win; // Player won the game!
                btnThanksForPlaying.Enabled = true;
                btnThanksForPlaying.BackColor = Color.LimeGreen;
                btnThanksForPlaying.ForeColor = Color.Black;
                
                // Disable Walk Away (grey)
                btnWalk.Enabled = false;
                btnWalk.BackColor = Color.Gray;
                
                // Disable Lights Down (grey)
                btnLightsDown.Enabled = false;
                btnLightsDown.BackColor = Color.Gray;
            }
        }
        else
        {
            // Capture current question number for lose sound
            var currentQuestionNumber = (int)nmrLevel.Value + 1;
            
            _gameOutcome = GameOutcome.Wrong; // Track that player answered incorrectly
            
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

            // Stop all audio first (including final answer), wait 500ms, then play question-specific lose sound
            _soundService.StopAllSounds();
            await Task.Delay(500);
            PlayLoseSound(currentQuestionNumber);
            
            // Auto-show winnings after 2 seconds
            await Task.Delay(2000);
            if (!chkShowWinnings.Checked)
            {
                chkShowWinnings.Checked = true;
            }
            
            // Disable Reveal button (grey)
            btnReveal.Enabled = false;
            btnReveal.BackColor = Color.Gray;
            
            // Enable Thanks for Playing (green)
            btnThanksForPlaying.Enabled = true;
            btnThanksForPlaying.BackColor = Color.LimeGreen;
            btnThanksForPlaying.ForeColor = Color.Black;
            
            // Disable Walk Away (grey)
            btnWalk.Enabled = false;
            btnWalk.BackColor = Color.Gray;
            
            // Disable Lights Down (grey)
            btnLightsDown.Enabled = false;
            btnLightsDown.BackColor = Color.Gray;
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
        _answerRevealStep = 0; // Reset reveal state
        _gameOutcome = GameOutcome.InProgress; // Reset game outcome
        nmrLevel.Value = 0;
        
        // Disable answer buttons until a question is loaded and set to grey
        btnA.Enabled = false;
        btnA.BackColor = Color.Gray;
        btnB.Enabled = false;
        btnB.BackColor = Color.Gray;
        btnC.Enabled = false;
        btnC.BackColor = Color.Gray;
        btnD.Enabled = false;
        btnD.BackColor = Color.Gray;

        // Reset lifeline buttons
        _gameService.State.ResetLifelines();
        _lifelineMode = LifelineMode.Inactive;
        SetLifelineMode(LifelineMode.Inactive);
        
        // Reinitialize lifeline buttons (visibility, labels, default state)
        InitializeLifelineButtons();
        
        // Explicitly disable and grey out all lifeline buttons when round is over
        // Only apply to visible buttons
        if (btn5050.Visible)
        {
            btn5050.Enabled = false;
            btn5050.BackColor = Color.Gray;
        }
        if (btnPhoneFriend.Visible)
        {
            btnPhoneFriend.Enabled = false;
            btnPhoneFriend.BackColor = Color.Gray;
        }
        if (btnAskAudience.Visible)
        {
            btnAskAudience.Enabled = false;
            btnAskAudience.BackColor = Color.Gray;
        }
        if (btnSwitch.Visible)
        {
            btnSwitch.Enabled = false;
            btnSwitch.BackColor = Color.Gray;
        }
        
        // Reset closing state
        if (_closingTimer != null)
        {
            _closingTimer.Stop();
            _closingTimer.Dispose();
            _closingTimer = null;
        }
        _closingInProgress = false;
        
        // Disable Host Intro (grey)
        btnHostIntro.Enabled = false;
        btnHostIntro.BackColor = Color.Gray;
        
        // Enable Pick Player for next player (green)
        btnPickPlayer.Enabled = true;
        btnPickPlayer.BackColor = Color.LimeGreen;
        btnPickPlayer.ForeColor = Color.Black;
        
        // Disable all other buttons (grey)
        btnExplainGame.Enabled = false;
        btnExplainGame.BackColor = Color.Gray;
        
        btnLightsDown.Enabled = false;
        btnLightsDown.BackColor = Color.Gray;
        
        btnNewQuestion.Enabled = false;
        btnNewQuestion.BackColor = Color.Gray;
        
        btnReveal.Enabled = false;
        btnReveal.BackColor = Color.Gray;
        
        btnWalk.Enabled = false;
        btnWalk.BackColor = Color.Gray;
        
        btnThanksForPlaying.Enabled = false;
        btnThanksForPlaying.BackColor = Color.Gray;
        
        // Keep Closing enabled after first round completes
        if (_firstRoundCompleted)
        {
            btnClosing.Enabled = true;
            btnClosing.BackColor = Color.LimeGreen;
            btnClosing.ForeColor = Color.Black;
        }
        else
        {
            btnClosing.Enabled = false;
            btnClosing.BackColor = Color.Gray;
        }
        
        // Re-enable and reset risk mode button
        btnActivateRiskMode.Enabled = true;
        btnActivateRiskMode.BackColor = Color.Yellow;
        btnActivateRiskMode.Text = "Activate Risk Mode";
        
        // Disable Reset button until lights down again (grey)
        btnResetGame.Enabled = false;
        btnResetGame.BackColor = Color.Gray;
    }

    private void SetLifelineMode(LifelineMode mode)
    {
        _lifelineMode = mode;
        
        switch (mode)
        {
            case LifelineMode.Inactive:
                // Grey - default state (disabled, not clickable)
                // Only apply to visible buttons
                if (btn5050.Visible)
                    SetLifelineButtonColor(btn5050, Color.Gray, false);
                if (btnPhoneFriend.Visible)
                    SetLifelineButtonColor(btnPhoneFriend, Color.Gray, false);
                if (btnAskAudience.Visible)
                    SetLifelineButtonColor(btnAskAudience, Color.Gray, false);
                if (btnSwitch.Visible)
                    SetLifelineButtonColor(btnSwitch, Color.Gray, false);
                break;
                
            case LifelineMode.Demo:
                // Yellow - demo mode
                // Only apply to visible buttons
                if (btn5050.Visible)
                    SetLifelineButtonColor(btn5050, Color.Yellow, true);
                if (btnPhoneFriend.Visible)
                    SetLifelineButtonColor(btnPhoneFriend, Color.Yellow, true);
                if (btnAskAudience.Visible)
                    SetLifelineButtonColor(btnAskAudience, Color.Yellow, true);
                if (btnSwitch.Visible)
                    SetLifelineButtonColor(btnSwitch, Color.Yellow, true);
                break;
                
            case LifelineMode.Active:
                // Green - active mode (check if already used based on configured types)
                // Only apply to visible buttons
                var type1 = GetLifelineTypeFromSettings(1);
                var type2 = GetLifelineTypeFromSettings(2);
                var type3 = GetLifelineTypeFromSettings(3);
                var type4 = GetLifelineTypeFromSettings(4);
                
                var lifeline1 = _gameService.State.GetLifeline(type1);
                var lifeline2 = _gameService.State.GetLifeline(type2);
                var lifeline3 = _gameService.State.GetLifeline(type3);
                var lifeline4 = _gameService.State.GetLifeline(type4);
                
                if (btn5050.Visible)
                    SetLifelineButtonColor(btn5050, lifeline1?.IsUsed == true ? Color.Gray : Color.LimeGreen, lifeline1?.IsUsed != true);
                if (btnPhoneFriend.Visible)
                    SetLifelineButtonColor(btnPhoneFriend, lifeline2?.IsUsed == true ? Color.Gray : Color.LimeGreen, lifeline2?.IsUsed != true);
                if (btnAskAudience.Visible)
                    SetLifelineButtonColor(btnAskAudience, lifeline3?.IsUsed == true ? Color.Gray : Color.LimeGreen, lifeline3?.IsUsed != true);
                if (btnSwitch.Visible)
                    SetLifelineButtonColor(btnSwitch, lifeline4?.IsUsed == true ? Color.Gray : Color.LimeGreen, lifeline4?.IsUsed != true);
                break;
        }
    }

    private void SetLifelineButtonColor(Button button, Color color, bool enabled)
    {
        button.BackColor = color;
        button.Enabled = enabled;
    }

    #endregion

    #region Menu Handlers

    private void DatabaseToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        using var dbDialog = new Options.DatabaseSettingsDialog(_sqlSettings.Settings);
        if (dbDialog.ShowDialog() == DialogResult.OK)
        {
            // Database settings changed - might need to reconnect
            MessageBox.Show(
                "Database settings saved. Please restart the application to apply changes.",
                "Database Settings",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }

    private void QuestionsEditorToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        var editorForm = new QuestionEditor.Forms.QuestionEditorMainForm();
        editorForm.Show();
    }

    private void OptionsToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        using var optionsDialog = new Options.OptionsDialog(_appSettings.Settings);
        if (optionsDialog.ShowDialog() == DialogResult.OK)
        {
            // Reload sounds after settings change
            _soundService.LoadSoundsFromSettings(_appSettings.Settings);
        }
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
    
    private void UsageToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        // Future feature: Show usage documentation
        MessageBox.Show(
            "Usage documentation will be available in a future update.\n\n" +
            "For now, please refer to the README.md file in the project repository.",
            "Usage",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }
    
    private void CheckUpdatesToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        // Future feature: Check for updates
        MessageBox.Show(
            "Update checking will be available in a future update.",
            "Check for Updates",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void AboutToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        using var aboutDialog = new About.AboutDialog();
        aboutDialog.ShowDialog(this);
    }

    #endregion

    private void nmrLevel_ValueChanged(object? sender, EventArgs e)
    {
        _gameService.ChangeLevel((int)nmrLevel.Value);
    }
}
