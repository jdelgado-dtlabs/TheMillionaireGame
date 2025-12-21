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
    Inactive,  // Before Explain Game (grey, disabled)
    Demo,      // During Explain Game (yellow, clickable for demo)
    Standby,   // After Lights Down but before all answers revealed (orange, not clickable)
    Active     // After all 4 answers revealed (green, clickable, then grey when used)
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
/// Closing sequence stages
/// </summary>
public enum ClosingStage
{
    NotStarted,     // Closing not started
    GameOver,       // Red - Playing game over sound (if round in progress)
    Underscore,     // Orange - Playing closing underscore
    Theme,          // Yellow - Playing closing theme
    Complete        // Green - Sequence complete, ready to reset
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
    private string? _currentFinalAnswerKey = null; // Track the final answer sound key for stopping
    private string? _currentLightsDownIdentifier = null; // Track the lights down sound identifier (GUID) for stopping
    
    // Money Tree demo animation state tracking
    private System.Windows.Forms.Timer? _moneyTreeDemoTimer;
    private int _moneyTreeDemoLevel = 0;
    private bool _isMoneyTreeDemoActive = false;
    private bool _isExplainGameActive = false; // Track if Explain Game mode is active
    private const int MONEY_TREE_DEMO_INTERVAL = 500; // milliseconds between level changes
    
    // Question reveal state tracking
    private int _answerRevealStep = 0; // 0 = not started, 1 = question shown, 2-5 = answers A-D shown
    private Question? _currentQuestion = null; // Store current question for progressive reveal
    
    // Game outcome tracking
    private GameOutcome _gameOutcome = GameOutcome.InProgress;
    private CancellationTokenSource? _automatedSequenceCts = null;
    private string? _finalWinningsAmount = null; // Store final winnings for end-of-round display
    
    // Track if automated sequence is running (walk away, thanks for playing)
    private bool _isAutomatedSequenceRunning = false;
    
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
    private ClosingStage _closingStage = ClosingStage.NotStarted;
    
    // Track if at least one round has been completed
    private bool _firstRoundCompleted = false;
    
    // Track if bed music should be restarted after lifeline use (for Q1-5)
    private bool _shouldRestartBedMusic = false;

    // Screen forms
    private HostScreenForm? _hostScreen;
    private GuestScreenForm? _guestScreen;
    private IGameScreen? _tvScreen;

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
        
        // Sounds are already loaded in Program.cs during initialization
        
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
        
        // Disable question level selector until Pick a Player is clicked
        nmrLevel.Enabled = false;
        
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
        UpdateRiskModeButton();
        
        // Update money tree on all screens
        UpdateMoneyTreeOnScreens(e.NewLevel);
    }

    private void OnModeChanged(object? sender, GameModeChangedEventArgs e)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => OnModeChanged(sender, e)));
            return;
        }

        UpdateRiskModeButton();
    }
    
    /// <summary>
    /// Updates risk mode button appearance based on safety net configuration
    /// </summary>
    private void UpdateRiskModeButton()
    {
        var settings = _gameService.MoneyTree.Settings;
        bool isNet5Active = (settings.SafetyNet1 == 5 || settings.SafetyNet2 == 5);
        bool isNet10Active = (settings.SafetyNet1 == 10 || settings.SafetyNet2 == 10);
        
        // Determine button state based on safety net configuration
        if (!isNet5Active && !isNet10Active)
        {
            // Both safety nets disabled = permanent risk mode
            btnActivateRiskMode.BackColor = Color.Red;
            btnActivateRiskMode.Text = "RISK MODE: ON";
            btnActivateRiskMode.Enabled = false; // Unclickable
        }
        else if (!isNet5Active)
        {
            // Q5 safety net disabled
            if (_gameService.State.Mode == Core.Models.GameMode.Risk)
            {
                btnActivateRiskMode.BackColor = Color.Red;
                btnActivateRiskMode.Text = "RISK MODE: ON";
            }
            else
            {
                btnActivateRiskMode.BackColor = Color.Blue;
                btnActivateRiskMode.Text = "RISK MODE: 5";
            }
            btnActivateRiskMode.Enabled = (_gameService.State.CurrentLevel == 0);
        }
        else if (!isNet10Active)
        {
            // Q10 safety net disabled
            if (_gameService.State.Mode == Core.Models.GameMode.Risk)
            {
                btnActivateRiskMode.BackColor = Color.Red;
                btnActivateRiskMode.Text = "RISK MODE: ON";
            }
            else
            {
                btnActivateRiskMode.BackColor = Color.Blue;
                btnActivateRiskMode.Text = "RISK MODE: 10";
            }
            btnActivateRiskMode.Enabled = (_gameService.State.CurrentLevel == 0);
        }
        else
        {
            // Both safety nets active = normal risk mode toggle
            if (_gameService.State.Mode == Core.Models.GameMode.Risk)
            {
                btnActivateRiskMode.BackColor = Color.Red;
                btnActivateRiskMode.Text = "RISK MODE: ON";
            }
            else
            {
                btnActivateRiskMode.BackColor = Color.Yellow;
                btnActivateRiskMode.Text = "Activate Risk Mode";
            }
            btnActivateRiskMode.Enabled = (_gameService.State.CurrentLevel == 0);
        }
    }
    
    /// <summary>
    /// Updates money tree display on all open screens
    /// </summary>
    private void UpdateMoneyTreeOnScreens(int level)
    {
        _hostScreen?.UpdateMoneyTreeLevel(level);
        _guestScreen?.UpdateMoneyTreeLevel(level);
        
        if (_tvScreen is TVScreenForm tvForm)
        {
            tvForm.UpdateMoneyTreeLevel(level);
        }
        else if (_tvScreen is TVScreenFormScalable tvScalable)
        {
            tvScalable.UpdateMoneyTreeLevel(level);
        }
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
            // Use final winnings amount if available (end-of-round), otherwise use current value
            var amountToShow = _finalWinningsAmount ?? _gameService.State.CurrentValue;
            _screenService.ShowWinningsAmount(amountToShow);
        }
        else
        {
            _screenService.HideWinnings();
            _finalWinningsAmount = null; // Clear stored amount when hiding
        }
    }

    #endregion

    #region Button Click Handlers

    private async void btnNewQuestion_Click(object? sender, EventArgs e)
    {
        if (_answerRevealStep == 0)
        {
            // Stop lights down sound if it's playing (for Q6+)
            if (!string.IsNullOrEmpty(_currentLightsDownIdentifier))
            {
                _soundService.StopSound(_currentLightsDownIdentifier);
                _currentLightsDownIdentifier = null; // Clear it
            }
            
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
                
                // Activate lifelines (green, clickable) now that all answers are revealed
                SetLifelineMode(LifelineMode.Active);
                
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
            
            // Walk Away button will be enabled (green) after all answers are revealed
            // See the answer reveal step 5 logic below
            
            // Enable Closing (green) for Q2-14 only (disable at Q1 and Q15)
            if (questionNumber >= 2 && questionNumber <= 14)
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
        // Disable Lights Down immediately to prevent double-clicks
        btnLightsDown.Enabled = false;
        btnLightsDown.BackColor = Color.Gray;
        
        // Disable Explain Game immediately (grey)
        btnExplainGame.Enabled = false;
        btnExplainGame.BackColor = Color.Gray;
        
        // Disable question level selector during active question
        nmrLevel.Enabled = false;
        
        // Exit Explain Game mode
        _isExplainGameActive = false;
        
        // Stop money tree demo if running
        StopMoneyTreeDemo();
        
        // Hide money tree if visible on TV screen
        if (btnShowMoneyTree.Text == "Hide Money Tree" || btnShowMoneyTree.Text == "Demo Money Tree" || btnShowMoneyTree.Text == "Demo Running...")
        {
            if (_tvScreen is TVScreenFormScalable tvScalable)
            {
                await tvScalable.HideMoneyTreeAsync();
            }
            else if (_tvScreen is TVScreenForm tvForm)
            {
                await tvForm.HideMoneyTreeAsync();
            }
            
            // Reset button to Show state
            btnShowMoneyTree.BackColor = Color.LimeGreen;
            btnShowMoneyTree.ForeColor = Color.Black;
            btnShowMoneyTree.Text = "Show Money Tree";
            btnShowMoneyTree.Enabled = true;
        }
        
        // Stop any playing sounds first
        _soundService.StopAllSounds();
        
        // Play question-specific lights down sound
        PlayLightsDownSound();
        
        var questionNumber = (int)nmrLevel.Value + 1;
        
        // For Q1-5, enable Reset button on first lights down and play bed music after delay
        if (questionNumber >= 1 && questionNumber <= 5)
        {
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
            
            // Clear text on host and guest screens for Q6+
            _screenService.ClearQuestionAndAnswerText();
        }
        
        // Enable Question button (green) for all questions after lights down
        btnNewQuestion.Enabled = true;
        btnNewQuestion.BackColor = Color.LimeGreen;
        btnNewQuestion.ForeColor = Color.Black;
        
        // Set lifelines to standby mode (orange, not clickable until all answers revealed)
        SetLifelineMode(LifelineMode.Standby);
        
        // TODO: Dim screens
    }

    private void btnReveal_Click(object? sender, EventArgs e)
    {
        // Disable Reveal button immediately to prevent double-clicks
        btnReveal.Enabled = false;
        btnReveal.BackColor = Color.Gray;
        
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
        // Capture winnings BEFORE modifying game state
        var winnings = _gameService.State.CurrentValue;
        
        _gameService.State.WalkAway = true;
        _gameOutcome = GameOutcome.Drop; // Track that player walked away
        _isAutomatedSequenceRunning = true;
        
        // Create cancellation token for this sequence
        _automatedSequenceCts?.Cancel();
        _automatedSequenceCts?.Dispose();
        _automatedSequenceCts = new CancellationTokenSource();
        var token = _automatedSequenceCts.Token;
        
        // Stop all sounds before playing quit sound
        _soundService.StopAllSounds();
        
        // Use current question level to determine which quit sound to play
        var questionNumber = (int)nmrLevel.Value + 1; // Convert 0-indexed to 1-indexed
        var quitSound = questionNumber <= 10 ? SoundEffect.QuitSmall : SoundEffect.QuitLarge;
        var quitSoundId = "quit_sound";
        
        _soundService.PlaySound(quitSound, quitSoundId, loop: false);
        
        // Give sound time to register before checking status
        try
        {
            await Task.Delay(100, token);
        }
        catch (OperationCanceledException)
        {
            return; // Sequence was cancelled
        }
        
        // Only show message box in debug mode (non-blocking)
        if (Program.DebugMode)
        {
#pragma warning disable CS4014
            Task.Run(() => MessageBox.Show($"Total winnings: {winnings}", 
                "Walk Away", MessageBoxButtons.OK, MessageBoxIcon.Information));
#pragma warning restore CS4014
        }
        
        // Show winnings on screens
        if (!chkShowWinnings.Checked)
        {
            chkShowWinnings.Checked = true;
        }
        
        // Disable Walk Away (grey)
        btnWalk.Enabled = false;
        btnWalk.BackColor = Color.Gray;
        
        // Reset button deprecated - automated sequences handle all resets
        
        // Disable lifelines immediately - round is over
        SetLifelineMode(LifelineMode.Inactive);
        
        // Enable Closing (green)
        btnClosing.Enabled = true;
        btnClosing.BackColor = Color.LimeGreen;
        btnClosing.ForeColor = Color.Black;
        
        // Wait for quit sound to finish, then auto-trigger end-of-round sequence
        try
        {
            await _soundService.WaitForSoundAsync(quitSoundId, token);
            await EndRoundSequence(winnings);
        }
        catch (OperationCanceledException)
        {
            // Sequence was cancelled by reset
        }
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
    
    private async void btnShowMoneyTree_Click(object? sender, EventArgs e)
    {
        if (btnShowMoneyTree.Text == "Show Money Tree")
        {
            // Hide the winning strap if it's showing (to avoid overlap)
            if (chkShowWinnings.Checked)
            {
                chkShowWinnings.Checked = false;
            }
            
            // Show the money tree on TV screen only
            if (_tvScreen is TVScreenFormScalable tvScalable)
            {
                await tvScalable.ShowMoneyTreeAsync();
            }
            else if (_tvScreen is TVScreenForm tvForm)
            {
                await tvForm.ShowMoneyTreeAsync();
            }
            
            // Update button state based on whether Explain Game is active
            if (_isExplainGameActive)
            {
                // In Explain Game mode - go directly to Demo state
                btnShowMoneyTree.BackColor = Color.DeepSkyBlue;
                btnShowMoneyTree.ForeColor = Color.Black;
                btnShowMoneyTree.Text = "Demo Money Tree";
            }
            else
            {
                // Normal mode - just hide
                btnShowMoneyTree.BackColor = Color.Orange;
                btnShowMoneyTree.ForeColor = Color.Black;
                btnShowMoneyTree.Text = "Hide Money Tree";
            }
        }
        else if (btnShowMoneyTree.Text == "Demo Money Tree")
        {
            // Start demo animation - progress through levels 1-15
            StartMoneyTreeDemo();
        }
        else // "Hide Money Tree"
        {
            // Stop demo if it's running
            StopMoneyTreeDemo();
            
            // Hide the money tree from TV screen
            if (_tvScreen is TVScreenFormScalable tvScalable)
            {
                await tvScalable.HideMoneyTreeAsync();
            }
            else if (_tvScreen is TVScreenForm tvForm)
            {
                await tvForm.HideMoneyTreeAsync();
            }
            
            // Update button state
            btnShowMoneyTree.BackColor = Color.LimeGreen;
            btnShowMoneyTree.ForeColor = Color.Black;
            btnShowMoneyTree.Text = "Show Money Tree";
        }
    }

    private void StartMoneyTreeDemo()
    {
        _isMoneyTreeDemoActive = true;
        _moneyTreeDemoLevel = 1; // Start at level 1
        
        // Initialize timer if not already created
        if (_moneyTreeDemoTimer == null)
        {
            _moneyTreeDemoTimer = new System.Windows.Forms.Timer();
            _moneyTreeDemoTimer.Interval = MONEY_TREE_DEMO_INTERVAL;
            _moneyTreeDemoTimer.Tick += MoneyTreeDemoTimer_Tick;
        }
        
        // Update to first level immediately
        UpdateMoneyTreeOnScreens(_moneyTreeDemoLevel);
        
        // Start timer for subsequent levels
        _moneyTreeDemoTimer.Start();
        
        // Update button appearance during demo
        btnShowMoneyTree.BackColor = Color.Yellow;
        btnShowMoneyTree.ForeColor = Color.Black;
        btnShowMoneyTree.Text = "Demo Running...";
        btnShowMoneyTree.Enabled = false;
    }
    
    private void MoneyTreeDemoTimer_Tick(object? sender, EventArgs e)
    {
        _moneyTreeDemoLevel++;
        
        if (_moneyTreeDemoLevel <= 15)
        {
            // Update all screens with new level
            UpdateMoneyTreeOnScreens(_moneyTreeDemoLevel);
        }
        else
        {
            // Demo complete - stop and revert to Hide state
            StopMoneyTreeDemo();
            btnShowMoneyTree.BackColor = Color.Orange;
            btnShowMoneyTree.ForeColor = Color.Black;
            btnShowMoneyTree.Text = "Hide Money Tree";
            btnShowMoneyTree.Enabled = true;
        }
    }
    
    private void StopMoneyTreeDemo()
    {
        if (_moneyTreeDemoTimer != null)
        {
            _moneyTreeDemoTimer.Stop();
        }
        _isMoneyTreeDemoActive = false;
        _moneyTreeDemoLevel = 0;
    }
    
    // DEPRECATED: Reset button removed from UI - automated sequences handle all resets
    // Keeping this method commented out for reference in case manual reset is needed in future
    /*
    private void btnResetGame_Click(object? sender, EventArgs e)
    {
        // If automated sequence is running, cancel it
        if (_isAutomatedSequenceRunning)
        {
            _isAutomatedSequenceRunning = false;
            _automatedSequenceCts?.Cancel();
            _automatedSequenceCts?.Dispose();
            _automatedSequenceCts = null;
            _soundService.StopAllSounds();
            
            // Clean up all timers
            _pafTimer?.Stop();
            _pafTimer?.Dispose();
            _pafTimer = null;
            
            _ataTimer?.Stop();
            _ataTimer?.Dispose();
            _ataTimer = null;
            
            _closingTimer?.Stop();
            _closingTimer?.Dispose();
            _closingTimer = null;
            
            if (Program.DebugMode)
            {
                Console.WriteLine("[Reset] Cancelled automated sequence and cleaned up timers");
            }
        }
        
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
            
            // Reset closing timer
            _closingTimer?.Stop();
            _closingTimer?.Dispose();
            _closingTimer = null;
            
            _gameService.ResetGame();
            _firstRoundCompleted = true; // Mark that at least one round has been completed
            ResetAllControls();
        }
    }
    */

    private void btnLifeline1_Click(object? sender, EventArgs e)
    {
        HandleLifelineClick(1, btnLifeline1);
    }

    private void btnLifeline2_Click(object? sender, EventArgs e)
    {
        HandleLifelineClick(2, btnLifeline2);
    }

    private void btnLifeline3_Click(object? sender, EventArgs e)
    {
        HandleLifelineClick(3, btnLifeline3);
    }

    private async void btnLifeline4_Click(object? sender, EventArgs e)
    {
        await HandleLifelineClickAsync(4, btnLifeline4);
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
        btnLifeline1.Text = label1;
        btnLifeline1.Visible = totalLifelines >= 1;
        
        // Button 2
        btnLifeline2.Text = label2;
        btnLifeline2.Visible = totalLifelines >= 2;
        
        // Button 3
        btnLifeline3.Text = label3;
        btnLifeline3.Visible = totalLifelines >= 3;
        
        // Button 4
        btnLifeline4.Text = label4;
        btnLifeline4.Visible = totalLifelines >= 4;
    }

    /// <summary>
    /// Helper method to play a lifeline sound while stopping all other background audio.
    /// Stops all sounds first, waits 500ms, then plays the lifeline sound to ensure clean audio.
    /// </summary>
    private async Task PlayLifelineSoundAsync(SoundEffect soundEffect, string? identifier = null, bool loop = false)
    {
        // For Q1-5, track that bed music should be restarted after answer is revealed
        var questionNumber = (int)nmrLevel.Value + 1;
        if (questionNumber >= 1 && questionNumber <= 5)
        {
            _shouldRestartBedMusic = true;
        }
        
        // Stop all background sounds first
        _soundService.StopAllSounds();
        
        // Wait 500ms for clean audio transition
        await Task.Delay(500);
        
        // Play the lifeline sound
        if (identifier != null)
        {
            _soundService.PlaySound(soundEffect, identifier, loop);
        }
        else
        {
            _soundService.PlaySound(soundEffect);
        }
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

    private async void ExecuteFiftyFifty(Core.Models.Lifeline lifeline, Button button)
    {
        _gameService.UseLifeline(lifeline.Type);
        button.Enabled = false;
        button.BackColor = Color.Gray;

        // Play lifeline sound (stops background audio, waits 500ms, then plays)
        await PlayLifelineSoundAsync(SoundEffect.Lifeline5050);

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

    private async void ExecutePhoneFriend(Core.Models.Lifeline lifeline, Button button)
    {
        // Stage 1: Start intro/calling sequence
        _pafStage = PAFStage.CallingIntro;
        button.BackColor = Color.Blue;
        
        // Play intro sound on loop (stops background audio, waits 500ms, then plays)
        await PlayLifelineSoundAsync(SoundEffect.LifelinePAFStart, "paf_intro", loop: true);
        
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
            1 => btnLifeline1,
            2 => btnLifeline2,
            3 => btnLifeline3,
            4 => btnLifeline4,
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

    private async void ExecuteAskAudience(Core.Models.Lifeline lifeline, Button button)
    {
        // Stage 1: Start intro/explanation (2 minutes)
        _ataStage = ATAStage.Intro;
        button.BackColor = Color.Blue;
        
        // Play intro sound (stops background audio, waits 500ms, then plays)
        await PlayLifelineSoundAsync(SoundEffect.LifelineATAStart, "ata_intro");
        
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
            1 => btnLifeline1,
            2 => btnLifeline2,
            3 => btnLifeline3,
            4 => btnLifeline4,
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

            // Play lifeline sound (stops background audio, waits 500ms, then plays)
            await PlayLifelineSoundAsync(SoundEffect.LifelineSwitch);

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
        
        // Enable Show Money Tree button (green)
        btnShowMoneyTree.Enabled = true;
        btnShowMoneyTree.BackColor = Color.LimeGreen;
        btnShowMoneyTree.ForeColor = Color.Black;
        
        // Enable question level selector - allow setting before first lights down
        nmrLevel.Enabled = true;
    }

    private void btnExplainGame_Click(object? sender, EventArgs e)
    {
        // Play game explanation audio on loop
        _soundService.PlaySound(SoundEffect.ExplainGame, loop: true);
        
        // Enter demo mode - lifelines turn yellow
        SetLifelineMode(LifelineMode.Demo);
        
        // Set explain game active state
        _isExplainGameActive = true;
    }

    /// <summary>
    /// End-of-round sequence: plays walk away sound, shows winnings, and resets game for next player
    /// </summary>
    /// <param name="capturedWinnings">Optional pre-captured winnings value to display</param>
    private async Task EndRoundSequence(string? capturedWinnings = null)
    {
        // Use captured winnings if provided, otherwise calculate from game outcome
        string winnings = capturedWinnings ?? (_gameOutcome switch
        {
            // Use top prize from money tree settings (properly formatted with currency)
            GameOutcome.Win => _gameService.MoneyTree.GetFormattedValue(15),  // Won Q15 - show top prize from settings
            GameOutcome.Drop => _gameService.State.DropValue,      // Walked away - show drop value  
            GameOutcome.Wrong => _gameService.State.WrongValue,    // Answered wrong - show wrong value (0 or last milestone)
            _ => _gameService.State.CurrentValue                   // Fallback to current value
        });
        
        // Store final winnings for screen display
        _finalWinningsAmount = winnings;
        
        // Use current question level to determine which walk away sound to play
        var questionNumber = (int)nmrLevel.Value + 1; // Convert 0-indexed to 1-indexed
        var walkAwaySound = questionNumber <= 10 ? SoundEffect.WalkAwaySmall : SoundEffect.WalkAwayLarge;
        var walkAwaySoundId = "walkaway_sound";
        
        _soundService.PlaySound(walkAwaySound, walkAwaySoundId, loop: false);
        
        // Give sound time to register before checking status
        await Task.Delay(100);
        
        // Only show message box in debug mode (non-blocking)
        if (Program.DebugMode)
        {
#pragma warning disable CS4014
            Task.Run(() => MessageBox.Show($"Total Winnings: {winnings}\n\nThanks for playing!", 
                "Game Over", MessageBoxButtons.OK, MessageBoxIcon.Information));
#pragma warning restore CS4014
        }
        
        // Show winnings on screens
        if (!chkShowWinnings.Checked)
        {
            chkShowWinnings.Checked = true;
        }
        
        // Wait for walk away sound to finish
        await _soundService.WaitForSoundAsync(walkAwaySoundId);
        
        // Reset automated sequence flag
        _isAutomatedSequenceRunning = false;
        
        // Now reset the game (after winnings have been shown)
        _soundService.StopAllSounds();
        
        // Reset PAF state
        _pafTimer?.Stop();
        _pafTimer?.Dispose();
        _pafTimer = null;
        _pafStage = PAFStage.NotStarted;
        _pafLifelineNumber = 0;
        _pafSecondsRemaining = 30;
        
        // Reset money tree demo state
        _moneyTreeDemoTimer?.Stop();
        _moneyTreeDemoTimer?.Dispose();
        _moneyTreeDemoTimer = null;
        _isMoneyTreeDemoActive = false;
        _moneyTreeDemoLevel = 0;
        
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
        
        // After reset, ensure Closing button is enabled and ready for next closing sequence
        // (ResetAllControls already handles this based on _firstRoundCompleted)
    }

    private async void btnClosing_Click(object? sender, EventArgs e)
    {
        switch (_closingStage)
        {
            case ClosingStage.NotStarted:
                // Start closing sequence
                _closingStage = ClosingStage.GameOver;
                
                // Reset question level and clear all questions/lifelines
                nmrLevel.Value = 0;
                ResetAllControls();
                
                // Stop all sounds
                _soundService.StopAllSounds();
                
                // Check if round is in progress
                if (_isAutomatedSequenceRunning)
                {
                    // Round in progress - play game over sound first
                    btnClosing.BackColor = Color.Red;
                    _soundService.PlaySoundByKey("GameOver");
                    
                    if (Program.DebugMode)
                    {
                        Console.WriteLine("[Closing] Stage: GameOver - playing sound (5s)");
                    }
                    
                    // Set timer for 5 seconds, then move to underscore
                    _closingTimer = new System.Windows.Forms.Timer();
                    _closingTimer.Interval = 5000;
                    _closingTimer.Tick += (s, args) => { _closingTimer?.Stop(); MoveToUnderscoreStage(); };
                    _closingTimer.Start();
                }
                else
                {
                    // No round in progress - skip game over, go straight to underscore
                    MoveToUnderscoreStage();
                }
                break;
                
            case ClosingStage.GameOver:
                // Skip game over, move to underscore
                _closingTimer?.Stop();
                _soundService.StopSound("GameOver");
                MoveToUnderscoreStage();
                break;
                
            case ClosingStage.Underscore:
                // Skip underscore, move to theme
                _closingTimer?.Stop();
                _soundService.StopSound("CloseUnderscore");
                MoveToThemeStage();
                break;
                
            case ClosingStage.Theme:
                // Skip to completion
                _closingTimer?.Stop();
                CompleteClosing();
                break;
                
            case ClosingStage.Complete:
                // Already complete, do nothing
                break;
        }
    }
    
    private void MoveToUnderscoreStage()
    {
        _closingStage = ClosingStage.Underscore;
        btnClosing.BackColor = Color.Orange;
        
        _soundService.PlaySoundByKey("CloseUnderscore");
        
        if (Program.DebugMode)
        {
            Console.WriteLine("[Closing] Stage: Underscore - playing sound (150s)");
        }
        
        // Set timer for 150 seconds, then move to theme
        _closingTimer = new System.Windows.Forms.Timer();
        _closingTimer.Interval = 150000;
        _closingTimer.Tick += (s, args) => { _closingTimer?.Stop(); MoveToThemeStage(); };
        _closingTimer.Start();
    }
    
    private async void MoveToThemeStage()
    {
        _closingStage = ClosingStage.Theme;
        btnClosing.BackColor = Color.Yellow;
        
        // Stop underscore
        _soundService.StopSound("CloseUnderscore");
        
        // Play closing theme
        _soundService.PlaySound(SoundEffect.CloseTheme, "close_theme", loop: false);
        
        if (Program.DebugMode)
        {
            Console.WriteLine("[Closing] Stage: Theme - playing sound (wait for completion)");
        }
        
        // Wait for closing theme to finish
        await _soundService.WaitForSoundAsync("close_theme");
        
        CompleteClosing();
    }
    
    private void CompleteClosing()
    {
        _closingStage = ClosingStage.Complete;
        btnClosing.BackColor = Color.LimeGreen;
        
        // Stop and dispose timer if still running
        if (_closingTimer != null)
        {
            _closingTimer.Stop();
            _closingTimer.Dispose();
            _closingTimer = null;
        }
        
        if (Program.DebugMode)
        {
            Console.WriteLine("[Closing] Complete - resetting application");
        }
        
        // Stop all sounds
        _soundService.StopAllSounds();
        
        // Reset closing stage
        _closingStage = ClosingStage.NotStarted;
        
        // Reset first round flag
        _firstRoundCompleted = false;
        
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
        
        btnClosing.Enabled = false;
        btnClosing.BackColor = Color.Gray;
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
        
        // Return ONLY active (enabled) lifelines to standby mode (orange, not clickable)
        // Used/disabled lifelines are left alone
        if (btnLifeline1.Visible && btnLifeline1.Enabled)
        {
            btnLifeline1.BackColor = Color.Orange;
            btnLifeline1.Enabled = false;
        }
        if (btnLifeline2.Visible && btnLifeline2.Enabled)
        {
            btnLifeline2.BackColor = Color.Orange;
            btnLifeline2.Enabled = false;
        }
        if (btnLifeline3.Visible && btnLifeline3.Enabled)
        {
            btnLifeline3.BackColor = Color.Orange;
            btnLifeline3.Enabled = false;
        }
        if (btnLifeline4.Visible && btnLifeline4.Enabled)
        {
            btnLifeline4.BackColor = Color.Orange;
            btnLifeline4.Enabled = false;
        }

        // Broadcast answer selection to all screens
        _screenService.SelectAnswer(answer);

        // Stop any existing final answer sound first to prevent overlapping
        _soundService.StopSound("final_answer");
        
        // Play final answer sound based on current question number
        PlayFinalAnswerSound();
        
        // For Q6-15, stop bed music when final answer starts
        var questionNumber = (int)nmrLevel.Value + 1;
        if (questionNumber >= 6)
        {
            // Stop the bed music using the question-specific identifier
            var bedMusicKey = questionNumber switch
            {
                6 => "Q06Bed",
                7 => "Q07Bed",
                8 => "Q08Bed",
                9 => "Q09Bed",
                10 => "Q10Bed",
                11 => "Q11Bed",
                12 => "Q12Bed",
                13 => "Q13Bed",
                14 => "Q14Bed",
                15 => "Q15Bed",
                _ => string.Empty
            };
            if (!string.IsNullOrEmpty(bedMusicKey))
            {
                _soundService.StopSound(bedMusicKey);
            }
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
        
        var soundKey = questionNumber switch
        {
            1 => "Q01Final",
            2 => "Q02Final",
            3 => "Q03Final",
            4 => "Q04Final",
            5 => "Q05Final",
            6 => "Q06Final",
            7 => "Q07Final",
            8 => "Q08Final",
            9 => "Q09Final",
            10 => "Q10Final",
            11 => "Q11Final",
            12 => "Q12Final",
            13 => "Q13Final",
            14 => "Q14Final",
            15 => "Q15Final",
            _ => string.Empty
        };

        if (string.IsNullOrEmpty(soundKey))
        {
            if (Program.DebugMode)
            {
                Console.WriteLine($"[Sound] No final answer sound key for Q{questionNumber}");
            }
            return;
        }
        
        if (Program.DebugMode)
        {
            Console.WriteLine($"[Sound] Playing final answer sound for Q{questionNumber}: {soundKey}");
        }
        
        // Play without loop and store the identifier for later stopping
        _currentFinalAnswerKey = _soundService.PlaySoundByKeyWithIdentifier(soundKey, loop: false);
    }

    private void PlayLoseSound(int? questionNumber = null)
    {
        var currentQuestion = questionNumber ?? (_gameService.State.CurrentLevel + 1); // Convert 0-indexed to 1-indexed
        
        if (Program.DebugMode)
        {
            Console.WriteLine($"[Sound] Looking for lose sound for question #{currentQuestion}");
        }
        
        var soundKey = currentQuestion switch
        {
            1 => "Q01to05Wrong",
            2 => "Q01to05Wrong",
            3 => "Q01to05Wrong",
            4 => "Q01to05Wrong",
            5 => "Q01to05Wrong",
            6 => "Q06Wrong",
            7 => "Q07Wrong",
            8 => "Q08Wrong",
            9 => "Q09Wrong",
            10 => "Q10Wrong",
            11 => "Q11Wrong",
            12 => "Q12Wrong",
            13 => "Q13Wrong",
            14 => "Q14Wrong",
            15 => "Q15Wrong",
            _ => string.Empty
        };

        if (string.IsNullOrEmpty(soundKey))
        {
            if (Program.DebugMode)
            {
                Console.WriteLine($"[Sound] No lose sound key for Q{currentQuestion}");
            }
            return;
        }
        
        if (Program.DebugMode)
        {
            Console.WriteLine($"[Sound] Playing lose sound for Q{currentQuestion}: {soundKey}");
        }
        
        _soundService.PlaySoundByKey(soundKey);
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
        
        var soundKey = currentQuestion switch
        {
            1 => "Q01to04Correct",
            2 => "Q01to04Correct",
            3 => "Q01to04Correct",
            4 => "Q01to04Correct",
            5 => isRiskMode ? "Q05Correct2" : "Q05Correct",
            6 => "Q06Correct",
            7 => "Q07Correct",
            8 => "Q08Correct",
            9 => "Q09Correct",
            10 => isRiskMode ? "Q10Correct2" : "Q10Correct",
            11 => "Q11Correct",
            12 => "Q12Correct",
            13 => "Q13Correct",
            14 => "Q14Correct",
            15 => "Q15Correct",
            _ => string.Empty
        };

        if (string.IsNullOrEmpty(soundKey))
        {
            if (Program.DebugMode)
            {
                Console.WriteLine($"[Sound] No correct sound key for Q{currentQuestion}");
            }
            return;
        }
        
        if (Program.DebugMode)
        {
            Console.WriteLine($"[Sound] Playing correct sound for Q{currentQuestion}: {soundKey}");
        }
        
        _soundService.PlaySoundByKey(soundKey);
    }

    private void PlayQuestionBed()
    {
        var questionNumber = (int)nmrLevel.Value + 1; // Use current control value, convert 0-indexed to 1-indexed
        
        if (Program.DebugMode)
        {
            Console.WriteLine($"[Sound] Looking for bed music for question #{questionNumber}");
        }
        
        var soundKey = questionNumber switch
        {
            1 => "Q01to05Bed",
            2 => "Q01to05Bed",
            3 => "Q01to05Bed",
            4 => "Q01to05Bed",
            5 => "Q01to05Bed",
            6 => "Q06Bed",
            7 => "Q07Bed",
            8 => "Q08Bed",
            9 => "Q09Bed",
            10 => "Q10Bed",
            11 => "Q11Bed",
            12 => "Q12Bed",
            13 => "Q13Bed",
            14 => "Q14Bed",
            15 => "Q15Bed",
            _ => string.Empty
        };

        if (string.IsNullOrEmpty(soundKey))
        {
            if (Program.DebugMode)
            {
                Console.WriteLine($"[Sound] No bed music key for Q{questionNumber}");
            }
            return;
        }
        
        if (Program.DebugMode)
        {
            Console.WriteLine($"[Sound] Playing bed music for Q{questionNumber}: {soundKey}");
        }
        
        _soundService.PlaySoundByKey(soundKey, loop: true);
    }

    private void PlayLightsDownSound()
    {
        var questionNumber = (int)nmrLevel.Value + 1; // Use current control value, convert 0-indexed to 1-indexed
        
        if (Program.DebugMode)
        {
            Console.WriteLine($"[Sound] Looking for lights down sound for question #{questionNumber}");
        }
        
        var soundKey = questionNumber switch
        {
            1 => "Q01to05LightsDown",
            2 => "Q01to05LightsDown",
            3 => "Q01to05LightsDown",
            4 => "Q01to05LightsDown",
            5 => "Q01to05LightsDown",
            6 => "Q06LightsDown",
            7 => "Q07LightsDown",
            8 => "Q08LightsDown",
            9 => "Q09LightsDown",
            10 => "Q10LightsDown",
            11 => "Q11LightsDown",
            12 => "Q12LightsDown",
            13 => "Q13LightsDown",
            14 => "Q14LightsDown",
            15 => "Q15LightsDown",
            _ => string.Empty
        };

        if (string.IsNullOrEmpty(soundKey))
        {
            if (Program.DebugMode)
            {
                Console.WriteLine($"[Sound] No lights down sound key for Q{questionNumber}");
            }
            return;
        }
        
        if (Program.DebugMode)
        {
            Console.WriteLine($"[Sound] Playing lights down sound for Q{questionNumber}: {soundKey}");
        }
        
        // Play without loop and store the identifier for later stopping
        _currentLightsDownIdentifier = _soundService.PlaySoundByKeyWithIdentifier(soundKey, loop: false);
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

            // Stop final answer sound before playing correct sound
            // Stop final answer using the stored key
            if (!string.IsNullOrEmpty(_currentFinalAnswerKey))
            {
                _soundService.StopSound(_currentFinalAnswerKey);
                _currentFinalAnswerKey = null; // Clear it
            }
            
            // Stop bed music immediately before playing correct sound (Q5+)
            // For Q1-4, the bed continues playing through the correct answer
            if (currentQuestionNumber >= 5)
            {
                var bedMusicKey = currentQuestionNumber switch
                {
                    >= 1 and <= 5 => "Q01to05Bed",
                    6 => "Q06Bed",
                    7 => "Q07Bed",
                    8 => "Q08Bed",
                    9 => "Q09Bed",
                    10 => "Q10Bed",
                    11 => "Q11Bed",
                    12 => "Q12Bed",
                    13 => "Q13Bed",
                    14 => "Q14Bed",
                    15 => "Q15Bed",
                    _ => string.Empty
                };
                
                if (!string.IsNullOrEmpty(bedMusicKey))
                {
                    _soundService.StopSound(bedMusicKey);
                }
            }
            
            // Play question-specific correct answer sound using captured question number
            PlayCorrectSound(currentQuestionNumber);
            
            // For Q1-5, restart bed music after correct answer if it was stopped by a lifeline
            if (currentQuestionNumber >= 1 && currentQuestionNumber <= 5 && _shouldRestartBedMusic)
            {
                // Wait for correct answer sound to play a bit, then restart bed music
                await Task.Delay(2000);
                PlayQuestionBed();
                _shouldRestartBedMusic = false; // Reset flag
            }
            
            // Auto-show winnings after 2 seconds
            await Task.Delay(2000);
            if (!chkShowWinnings.Checked)
            {
                chkShowWinnings.Checked = true;
            }
            
            // Disable Reveal button (grey)
            btnReveal.Enabled = false;
            btnReveal.BackColor = Color.Gray;
            
            // If Q5 was just answered correctly, enable Lights Down for Q6
            if (currentQuestionNumber == 5)
            {
                // Enable Lights Down (green) for Q6
                btnLightsDown.Enabled = true;
                btnLightsDown.BackColor = Color.LimeGreen;
                btnLightsDown.ForeColor = Color.Black;
                
                // Re-enable question level selector (can now change for Q6+)
                nmrLevel.Enabled = true;
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
                
                // Re-enable question level selector (can now change for next question)
                nmrLevel.Enabled = true;
                
                // Disable Walk Away until next question is fully revealed (grey)
                btnWalk.Enabled = false;
                btnWalk.BackColor = Color.Gray;
            }
            // For Q15, enable Thanks for Playing (green)
            else if (currentQuestionNumber == 15)
            {
                _gameOutcome = GameOutcome.Win; // Player won the game!
                _isAutomatedSequenceRunning = true; // Mark sequence as automated
                
                // Stop final answer sound immediately (it was already stopped above but ensure it's stopped)
                // This is needed because we're about to wait 25 seconds for Q15Correct to finish
                if (!string.IsNullOrEmpty(_currentFinalAnswerKey))
                {
                    _soundService.StopSound(_currentFinalAnswerKey);
                    _currentFinalAnswerKey = null;
                }
                
                // Create cancellation token for this sequence
                _automatedSequenceCts?.Cancel();
                _automatedSequenceCts?.Dispose();
                _automatedSequenceCts = new CancellationTokenSource();
                var token = _automatedSequenceCts.Token;
                
                // Disable Walk Away (grey)
                btnWalk.Enabled = false;
                btnWalk.BackColor = Color.Gray;
                
                // Disable Lights Down (grey)
                btnLightsDown.Enabled = false;
                btnLightsDown.BackColor = Color.Gray;
                
                // Wait for Q15 correct sound to finish (approximately 20-30 seconds)
                try
                {
                    await Task.Delay(25000, token);
                    // Auto-trigger end-of-round sequence
                    await EndRoundSequence();
                }
                catch (OperationCanceledException)
                {
                    // Sequence was cancelled by reset
                }
            }
        }
        else
        {
            // Capture current question number for lose sound
            var currentQuestionNumber = (int)nmrLevel.Value + 1;
            
            _gameOutcome = GameOutcome.Wrong; // Track that player answered incorrectly
            _shouldRestartBedMusic = false; // Don't restart bed music on wrong answer
            
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

            // Stop all audio first (including final answer), then play question-specific lose sound
            _soundService.StopAllSounds();
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
            
            // Disable Walk Away (grey)
            btnWalk.Enabled = false;
            btnWalk.BackColor = Color.Gray;
            
            // Disable Lights Down (grey)
            btnLightsDown.Enabled = false;
            btnLightsDown.BackColor = Color.Gray;
            
            // Auto-trigger end-of-round sequence after wrong answer
            await Task.Delay(2000);
            await EndRoundSequence();
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
        nmrLevel.Enabled = false; // Disable until Pick a Player is clicked
        
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
        if (btnLifeline1.Visible)
        {
            btnLifeline1.Enabled = false;
            btnLifeline1.BackColor = Color.Gray;
        }
        if (btnLifeline2.Visible)
        {
            btnLifeline2.Enabled = false;
            btnLifeline2.BackColor = Color.Gray;
        }
        if (btnLifeline3.Visible)
        {
            btnLifeline3.Enabled = false;
            btnLifeline3.BackColor = Color.Gray;
        }
        if (btnLifeline4.Visible)
        {
            btnLifeline4.Enabled = false;
            btnLifeline4.BackColor = Color.Gray;
        }
        
        // Reset closing state
        if (_closingTimer != null)
        {
            _closingTimer.Stop();
            _closingTimer.Dispose();
            _closingTimer = null;
        }
        _closingStage = ClosingStage.NotStarted;
        
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
        UpdateRiskModeButton();
    }

    private void SetLifelineMode(LifelineMode mode)
    {
        _lifelineMode = mode;
        
        switch (mode)
        {
            case LifelineMode.Inactive:
                // Grey - default state (disabled, not clickable)
                // Only apply to visible buttons
                if (btnLifeline1.Visible)
                    SetLifelineButtonColor(btnLifeline1, Color.Gray, false);
                if (btnLifeline2.Visible)
                    SetLifelineButtonColor(btnLifeline2, Color.Gray, false);
                if (btnLifeline3.Visible)
                    SetLifelineButtonColor(btnLifeline3, Color.Gray, false);
                if (btnLifeline4.Visible)
                    SetLifelineButtonColor(btnLifeline4, Color.Gray, false);
                break;
                
            case LifelineMode.Demo:
                // Yellow - demo mode
                // Only apply to visible buttons
                if (btnLifeline1.Visible)
                    SetLifelineButtonColor(btnLifeline1, Color.Yellow, true);
                if (btnLifeline2.Visible)
                    SetLifelineButtonColor(btnLifeline2, Color.Yellow, true);
                if (btnLifeline3.Visible)
                    SetLifelineButtonColor(btnLifeline3, Color.Yellow, true);
                if (btnLifeline4.Visible)
                    SetLifelineButtonColor(btnLifeline4, Color.Yellow, true);
                break;
                
            case LifelineMode.Standby:
                // Orange - standby mode (not clickable until all answers revealed)
                // Only apply to visible buttons (used buttons are left alone)
                if (btnLifeline1.Visible)
                {
                    btnLifeline1.BackColor = Color.Orange;
                    btnLifeline1.Enabled = false;
                }
                if (btnLifeline2.Visible)
                {
                    btnLifeline2.BackColor = Color.Orange;
                    btnLifeline2.Enabled = false;
                }
                if (btnLifeline3.Visible)
                {
                    btnLifeline3.BackColor = Color.Orange;
                    btnLifeline3.Enabled = false;
                }
                if (btnLifeline4.Visible)
                {
                    btnLifeline4.BackColor = Color.Orange;
                    btnLifeline4.Enabled = false;
                }
                break;
                
            case LifelineMode.Active:
                // Green - active mode (clickable)
                // Only apply to visible buttons that are in standby (orange)
                // Grey/disabled buttons (used lifelines) are left alone
                if (btnLifeline1.Visible && btnLifeline1.BackColor == Color.Orange)
                {
                    btnLifeline1.BackColor = Color.LimeGreen;
                    btnLifeline1.Enabled = true;
                }
                if (btnLifeline2.Visible && btnLifeline2.BackColor == Color.Orange)
                {
                    btnLifeline2.BackColor = Color.LimeGreen;
                    btnLifeline2.Enabled = true;
                }
                if (btnLifeline3.Visible && btnLifeline3.BackColor == Color.Orange)
                {
                    btnLifeline3.BackColor = Color.LimeGreen;
                    btnLifeline3.Enabled = true;
                }
                if (btnLifeline4.Visible && btnLifeline4.BackColor == Color.Orange)
                {
                    btnLifeline4.BackColor = Color.LimeGreen;
                    btnLifeline4.Enabled = true;
                }
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
        
        // Subscribe to settings applied event to update immediately when Apply is clicked
        optionsDialog.SettingsApplied += (s, ev) =>
        {
            // Reload sounds after settings change
            _soundService.LoadSoundsFromSettings(_appSettings.Settings);
            
            // Reinitialize lifeline buttons to reflect new settings (visibility, labels)
            InitializeLifelineButtons();
            
            // Reapply current lifeline mode to update colors
            SetLifelineMode(_lifelineMode);
            
            // Reload money tree settings and update risk mode button
            _gameService.MoneyTree.LoadSettings();
            UpdateRiskModeButton();
        };
        
        optionsDialog.ShowDialog();
    }

    private void HostScreenToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        if (_hostScreen == null || _hostScreen.IsDisposed)
        {
            _hostScreen = new HostScreenForm();
            _hostScreen.Initialize(_gameService.MoneyTree);
            _screenService.RegisterScreen(_hostScreen);
            _hostScreen.FormClosed += (s, args) => _screenService.UnregisterScreen(_hostScreen);
            
            // Sync current game state to the newly opened screen
            SyncScreenState(_hostScreen);
            
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
            _guestScreen.Initialize(_gameService.MoneyTree);
            _screenService.RegisterScreen(_guestScreen);
            _guestScreen.FormClosed += (s, args) => _screenService.UnregisterScreen(_guestScreen);
            
            // Sync current game state to the newly opened screen
            SyncScreenState(_guestScreen);
            
            _guestScreen.Show();
        }
        else
        {
            _guestScreen.BringToFront();
        }
    }

    /// <summary>
    /// Synchronizes the current game state to a newly opened screen
    /// </summary>
    private void SyncScreenState(IGameScreen screen)
    {
        // Sync current question if one is loaded
        if (_currentQuestion != null)
        {
            screen.UpdateQuestion(_currentQuestion);
            
            // Show visible answers progressively based on reveal step
            if (_answerRevealStep >= 2) screen.ShowAnswer("A");
            if (_answerRevealStep >= 3) screen.ShowAnswer("B");
            if (_answerRevealStep >= 4) screen.ShowAnswer("C");
            if (_answerRevealStep >= 5) screen.ShowAnswer("D");
            
            // If answer has been selected, show it
            if (!string.IsNullOrEmpty(_currentAnswer))
            {
                screen.SelectAnswer(_currentAnswer);
            }
        }
        
        // Sync question visibility state
        if (chkShowQuestion.Checked)
        {
            screen.ShowQuestion(true);
        }
        
        // Sync winnings visibility state
        if (chkShowWinnings.Checked)
        {
            // Use the final winnings amount if available, otherwise current value
            var amountToShow = _finalWinningsAmount ?? _gameService.State.CurrentValue;
            if (screen is TVScreenFormScalable scalableScreen)
            {
                scalableScreen.ShowWinningsAmount(amountToShow);
            }
            else
            {
                screen.ShowWinnings(_gameService.State);
            }
        }
        
        // Sync money display
        screen.UpdateMoney(
            _gameService.State.CurrentValue,
            _gameService.State.CorrectValue,
            _gameService.State.WrongValue,
            _gameService.State.DropValue,
            _gameService.State.QuestionsLeft
        );
    }

    private void TVScreenToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        if (_tvScreen == null || (_tvScreen as Form)?.IsDisposed == true)
        {
            // Use the new scalable TV screen
            var tvForm = new TVScreenFormScalable();
            tvForm.Initialize(_gameService.MoneyTree);
            _tvScreen = tvForm;
            _screenService.RegisterScreen(_tvScreen);
            tvForm.FormClosed += (s, args) => _screenService.UnregisterScreen(_tvScreen);
            
            // Sync current game state to the newly opened screen
            SyncScreenState(_tvScreen);
            
            tvForm.Show();
        }
        else
        {
            (_tvScreen as Form)?.BringToFront();
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
