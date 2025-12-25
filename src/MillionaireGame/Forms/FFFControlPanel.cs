using MillionaireGame.Web.Models;
using MillionaireGame.Web.Database;
using MillionaireGame.Services;
using MillionaireGame.Core.Database;
using MillionaireGame.Core.Settings;
using MillionaireGame.Utilities;
using System.Timers;

namespace MillionaireGame.Forms;

/// <summary>
/// Game flow states for FFF round
/// </summary>
public enum FFFFlowState
{
    NotStarted,          // Initial state, select question
    IntroPlaying,        // FFFLightsDown + FFFExplain playing
    QuestionReady,       // Ready to show question
    QuestionShown,       // Question displayed, FFFReadQuestion playing
    AnswersRevealing,    // FFFThreeNotes playing
    AnswersRevealed,     // Answers shown, FFFThinking playing, timer active
    TimerExpired,        // FFFReadAnswers playing
    RevealingCorrect,    // Revealing correct answers (multi-click)
    WinnersShown,        // Winners list displayed
    WinnerAnnounced,     // Winner declared
    Complete             // Round complete
}

/// <summary>
/// User control for managing Fastest Finger First (FFF) rounds
/// </summary>
public partial class FFFControlPanel : UserControl
{
    private readonly System.Timers.Timer _fffTimer;
    private DateTime _fffStartTime;
    private bool _isFFFActive;
    private List<FFFQuestion> _questions = new();
    private FFFQuestion? _currentQuestion;
    private List<ParticipantInfo> _participants = new();
    private List<AnswerSubmission> _submissions = new();
    private List<RankingResult> _rankings = new();
    private FFFClientService? _fffClient;
    private readonly MillionaireGame.Web.Database.FFFQuestionRepository? _fffRepository;
    private const string _sessionId = "LIVE"; // Fixed session ID for live game
    
    // Phase 3: Game Flow State Management
    private FFFFlowState _currentState = FFFFlowState.NotStarted;
    private int _revealCorrectClickCount = 0;
    private string[] _randomizedAnswers = new string[4];
    private string _correctSequence = string.Empty;
    private SoundService? _soundService;
    
    public FFFControlPanel()
    {
        InitializeComponent();
        
        _fffTimer = new System.Timers.Timer(100); // Update every 100ms
        _fffTimer.Elapsed += FFFTimer_Elapsed;
        
        // Initialize repository
        try
        {
            var sqlSettings = new SqlSettingsManager();
            var connectionString = sqlSettings.Settings.GetConnectionString("dbMillionaire");
            _fffRepository = new MillionaireGame.Web.Database.FFFQuestionRepository(connectionString);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing FFF repository: {ex.Message}");
        }
        
        // Auto-refresh participants when control becomes visible
        this.VisibleChanged += async (s, e) =>
        {
            if (this.Visible)
            {
                await RefreshParticipantsAsync();
            }
        };
    }
    
    /// <summary>
    /// Load available FFF questions into memory
    /// </summary>
    public async Task LoadQuestionsAsync()
    {
        try
        {
            if (_fffRepository != null)
            {
                _questions = await _fffRepository.GetAllQuestionsAsync();
                MessageBox.Show($"Loaded {_questions.Count} FFF questions from database.",
                    "Questions Loaded", MessageBoxButtons.OK, MessageBoxIcon.None);
            }
            else
            {
                MessageBox.Show("FFF Repository is not initialized.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
            }
            
            UpdateUIState();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading FFF questions: {ex.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
        }
    }
    
    /// <summary>
    /// Initialize SignalR client connection
    /// </summary>
    public async Task InitializeClientAsync(string serverUrl)
    {
        try
        {
            GameConsole.Log($"[FFFControlPanel] InitializeClientAsync called with serverUrl: {serverUrl}");
            
            _fffClient = new FFFClientService(serverUrl, _sessionId);
            GameConsole.Log($"[FFFControlPanel] FFFClientService created");
            
            // Subscribe to events
            _fffClient.ParticipantJoined += async (s, p) =>
            {
                GameConsole.Log($"[FFFControlPanel] ParticipantJoined event: {p.DisplayName}");
                await RefreshParticipantsAsync();
            };
            _fffClient.AnswerSubmitted += (s, a) => AddAnswerSubmission(a);
            _fffClient.ConnectionStatusChanged += (s, status) =>
            {
                GameConsole.Log($"[FFFControlPanel] Connection status changed: {status}");
                if (InvokeRequired)
                    Invoke(() => Text = $"FFF Control - {status}");
                else
                    Text = $"FFF Control - {status}";
            };
            
            GameConsole.Log($"[FFFControlPanel] Calling ConnectAsync...");
            await _fffClient.ConnectAsync();
            GameConsole.Log($"[FFFControlPanel] ConnectAsync completed");
            
            // Load initial participants
            GameConsole.Log($"[FFFControlPanel] Getting active participants...");
            var participants = await _fffClient.GetActiveParticipantsAsync();
            GameConsole.Log($"[FFFControlPanel] Received {participants.Count} participants");
            UpdateParticipants(participants);
        }
        catch (Exception ex)
        {
            GameConsole.Log($"[FFFControlPanel] ERROR in InitializeClientAsync: {ex.Message}");
            GameConsole.Log($"[FFFControlPanel] Stack trace: {ex.StackTrace}");
            MessageBox.Show($"Error connecting to server: {ex.Message}",
                "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.None);
        }
    }
    
    /// <summary>
    /// Update participant list
    /// </summary>
    public void UpdateParticipants(List<ParticipantInfo> participants)
    {
        GameConsole.Log($"[FFFControlPanel] UpdateParticipants called with {participants.Count} participants, InvokeRequired={InvokeRequired}");
        
        if (InvokeRequired)
        {
            GameConsole.Log("[FFFControlPanel] Invoking on UI thread...");
            Invoke(new Action(() => UpdateParticipants(participants)));
            return;
        }
        
        GameConsole.Log("[FFFControlPanel] On UI thread, updating UI...");
        _participants = participants;
        lstParticipants.Items.Clear();
        GameConsole.Log($"[FFFControlPanel] Cleared list, adding {participants.Count} items");
        
        foreach (var participant in participants)
        {
            var idPreview = participant.Id.Length > 8 
                ? participant.Id.Substring(0, 8) + "..." 
                : participant.Id;
            var displayText = $"{participant.DisplayName} ({idPreview})";
            lstParticipants.Items.Add(displayText);
            GameConsole.Log($"[FFFControlPanel] Added: {displayText}");
        }
        
        lblParticipantCount.Text = $"{participants.Count} Participant{(participants.Count != 1 ? "s" : "")}";
        GameConsole.Log($"[FFFControlPanel] Updated label: {lblParticipantCount.Text}");
        UpdateUIState();
        GameConsole.Log("[FFFControlPanel] UpdateParticipants completed");
    }
    
    /// <summary>
    /// Refresh participant list from server
    /// </summary>
    public async Task RefreshParticipantsAsync()
    {
        try
        {
            GameConsole.Log("[FFFControlPanel] RefreshParticipantsAsync called");
            if (_fffClient != null && _fffClient.IsConnected)
            {
                GameConsole.Log("[FFFControlPanel] Client connected, calling GetActiveParticipantsAsync");
                var participants = await _fffClient.GetActiveParticipantsAsync();
                GameConsole.Log($"[FFFControlPanel] Received {participants.Count} participants");
                UpdateParticipants(participants);
                GameConsole.Log("[FFFControlPanel] UpdateParticipants completed");
            }
            else
            {
                GameConsole.Log($"[FFFControlPanel] Client not available - IsNull:{_fffClient == null}, IsConnected:{_fffClient?.IsConnected}");
            }
        }
        catch (Exception ex)
        {
            GameConsole.Log($"[FFFControlPanel] Error refreshing participants: {ex.Message}");
            MessageBox.Show($"Error refreshing participants: {ex.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
        }
    }
    
    /// <summary>
    /// Add answer submission
    /// </summary>
    public void AddAnswerSubmission(AnswerSubmission submission)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => AddAnswerSubmission(submission)));
            return;
        }
        
        _submissions.Add(submission);
        
        var timeSeconds = (submission.SubmittedAt - _fffStartTime).TotalSeconds;
        lstAnswers.Items.Add($"{submission.DisplayName}: {submission.AnswerSequence} ({timeSeconds:F2}s)");
        
        lblAnswerCount.Text = $"{_submissions.Count} Answer{(_submissions.Count != 1 ? "s" : "")}";
        UpdateUIState();
    }
    
    /// <summary>
    /// Update rankings display
    /// </summary>
    public void UpdateRankings(List<RankingResult> rankings)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => UpdateRankings(rankings)));
            return;
        }
        
        GameConsole.Log($"[FFFControlPanel] UpdateRankings called with {rankings.Count} rankings");
        
        _rankings = rankings;
        lstRankings.Items.Clear();
        
        foreach (var ranking in rankings)
        {
            GameConsole.Log($"[FFFControlPanel] Ranking: Rank={ranking.Rank}, DisplayName={ranking.DisplayName}, IsCorrect={ranking.IsCorrect}, TimeElapsed={ranking.TimeElapsed}ms");
            var icon = ranking.IsCorrect ? "✓" : "✗";
            var timeSeconds = ranking.TimeElapsed / 1000.0;
            lstRankings.Items.Add($"#{ranking.Rank} {icon} {ranking.DisplayName} ({timeSeconds:F2}s)");
        }
        
        if (rankings.Count > 0 && rankings[0].IsCorrect)
        {
            GameConsole.Log($"[FFFControlPanel] Winner detected: {rankings[0].DisplayName}");
            lblWinner.Text = $"Winner: {rankings[0].DisplayName}";
            lblWinner.ForeColor = Color.Green;
        }
        else
        {
            GameConsole.Log($"[FFFControlPanel] No winner - Count={rankings.Count}, FirstIsCorrect={rankings.Count > 0 && rankings[0].IsCorrect}");
            lblWinner.Text = "Winner: None (no correct answers)";
            lblWinner.ForeColor = Color.Red;
        }
        
        UpdateUIState();
    }
    
    /* REMOVED - No longer using dropdown
    private void cmbQuestions_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (cmbQuestions.SelectedIndex > 0 && cmbQuestions.SelectedItem is ComboBoxItem item && item.Value is FFFQuestion question)
        {
            _currentQuestion = question;
            LoadQuestionDetails(question);
        }
        else
        {
            _currentQuestion = null;
            ClearQuestionDetails();
        }
        
        UpdateUIState();
    }
    
    private void LoadQuestionDetails(FFFQuestion question)
    {
        txtOption1.Text = question.AnswerA;
        txtOption2.Text = question.AnswerB;
        txtOption3.Text = question.AnswerC;
        txtOption4.Text = question.AnswerD;
        lblCorrectOrder.Text = $"Correct Order: {question.CorrectOrder}";
    }
    */
    
    private void ClearQuestionDetails()
    {
        txtOption1.Clear();
        txtOption2.Clear();
        txtOption3.Clear();
        txtOption4.Clear();
        lblCorrectOrder.Text = "Correct Order: ---";
    }
    
    /* OLD - REPLACED BY NEW BUTTON FLOW
    private async void btnStartFFF_Click(object? sender, EventArgs e)
    {
        if (_currentQuestion == null)
        {
            MessageBox.Show("Please select a question first.", "No Question Selected",
                MessageBoxButtons.OK, MessageBoxIcon.None);
            return;
        }
        
        if (_participants.Count == 0)
        {
            var result = MessageBox.Show(
                "No participants are currently connected. Start FFF anyway?",
                "No Participants",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.None);
                
            if (result == DialogResult.No)
                return;
        }
        
        try
        {
            if (_fffClient == null || !_fffClient.IsConnected)
            {
                MessageBox.Show("Not connected to web server. Start the web server from Settings first.",
                    "Not Connected", MessageBoxButtons.OK, MessageBoxIcon.None);
                return;
            }
            
            await _fffClient.StartQuestionAsync(
                _currentQuestion.Id, 
                _currentQuestion.QuestionText,
                new[] { _currentQuestion.AnswerA, _currentQuestion.AnswerB, _currentQuestion.AnswerC, _currentQuestion.AnswerD },
                timeLimit: 30);
            
            _fffStartTime = DateTime.UtcNow;
            _isFFFActive = true;
            _submissions.Clear();
            lstAnswers.Items.Clear();
            lblAnswerCount.Text = "0 Answers";
            
            _fffTimer.Start();
            UpdateUIState();
            
            MessageBox.Show("FFF question started! Participants can now submit answers.",
                "FFF Started", MessageBoxButtons.OK, MessageBoxIcon.None);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error starting FFF: {ex.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
        }
    }
    */
    
    /* OLD - REPLACED BY NEW BUTTON FLOW
    private async void btnEndFFF_Click(object? sender, EventArgs e)
    {
        try
        {
            _fffTimer.Stop();
            _isFFFActive = false;
            
            if (_fffClient != null && _fffClient.IsConnected)
            {
                await _fffClient.EndQuestionAsync(_currentQuestion.Id);
            }
            
            UpdateUIState();
            
            MessageBox.Show("FFF question ended. Calculate results to see rankings.",
                "FFF Ended", MessageBoxButtons.OK, MessageBoxIcon.None);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error ending FFF: {ex.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
        }
    }
    */
    
    /* OLD - REPLACED BY NEW BUTTON FLOW
    private async void btnCalculateResults_Click(object? sender, EventArgs e)
    {
        if (_currentQuestion == null)
        {
            MessageBox.Show("No question is currently loaded.", "No Question",
                MessageBoxButtons.OK, MessageBoxIcon.None);
            return;
        }
        
        try
        {
            if (_fffClient != null && _fffClient.IsConnected)
            {
                var rankings = await _fffClient.CalculateRankingsAsync(_currentQuestion.Id);
                UpdateRankings(rankings);
                
                MessageBox.Show("Results calculated! Check the Rankings panel.",
                    "Results Ready", MessageBoxButtons.OK, MessageBoxIcon.None);
            }
            else
            {
                MessageBox.Show("Not connected to web server.", "Not Connected",
                    MessageBoxButtons.OK, MessageBoxIcon.None);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error calculating results: {ex.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
        }
    }
    */
    
    /* OLD - REPLACED BY NEW BUTTON FLOW
    private void btnSelectWinner_Click(object? sender, EventArgs e)
    {
        if (_rankings.Count == 0 || !_rankings[0].IsCorrect)
        {
            MessageBox.Show("No winner available. Calculate results first or ensure there are correct answers.",
                "No Winner", MessageBoxButtons.OK, MessageBoxIcon.None);
            return;
        }
        
        var winner = _rankings[0];
        var result = MessageBox.Show(
            $"Confirm {winner.DisplayName} as the winner?\n\n" +
            $"Time: {winner.TimeElapsed / 1000.0:F2} seconds\n" +
            $"Answer: {winner.AnswerSequence}",
            "Confirm Winner",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.None);
            
        if (result == DialogResult.Yes)
        {
            // TODO: Notify game service that this player won FFF and should proceed to hot seat
            MessageBox.Show($"{winner.DisplayName} selected as winner!",
                "Winner Selected", MessageBoxButtons.OK, MessageBoxIcon.None);
            
            // Reset for next round
            ResetFFFRound();
        }
    }
    */
    
    private void FFFTimer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        if (!_isFFFActive) return;
        
        var elapsed = DateTime.UtcNow - _fffStartTime;
        var remaining = TimeSpan.FromSeconds(20) - elapsed;
        
        if (remaining.TotalSeconds <= 0)
        {
            remaining = TimeSpan.Zero;
        }
        
        // Round up seconds to match JavaScript behavior (19.9 seconds shows as "20")
        var displaySeconds = (int)Math.Ceiling(remaining.TotalSeconds);
        var displayMinutes = displaySeconds / 60;
        displaySeconds = displaySeconds % 60;
        var display = $"{displayMinutes:D2}:{displaySeconds:D2}";
        
        if (InvokeRequired)
        {
            Invoke(new Action(() => lblTimer.Text = display));
        }
        else
        {
            lblTimer.Text = display;
        }
    }
    
    /// <summary>
    /// Initialize sound service for audio playback
    /// </summary>
    public void SetSoundService(SoundService soundService)
    {
        _soundService = soundService;
    }
    
    /// <summary>
    /// Update button states based on current game flow state
    /// </summary>
    private void UpdateUIState()
    {
        var hasQuestion = _currentQuestion != null;
        var hasParticipants = _participants.Count > 0;
        
        GameConsole.Log($"[FFF] UpdateUIState - State: {_currentState}, HasQuestion: {hasQuestion}, HasParticipants: {hasParticipants}");
        
        // Update button enabled states based on game flow
        switch (_currentState)
        {
            case FFFFlowState.NotStarted:
                btnIntroExplain.Enabled = hasQuestion && hasParticipants;
                btnIntroExplain.BackColor = btnIntroExplain.Enabled ? Color.LightGreen : Color.Gray;
                btnShowQuestion.Enabled = false;
                btnShowQuestion.BackColor = Color.Gray;
                btnRevealAnswers.Enabled = false;
                btnRevealAnswers.BackColor = Color.Gray;
                btnRevealCorrect.Enabled = false;
                btnRevealCorrect.BackColor = Color.Gray;
                btnShowWinners.Enabled = false;
                btnShowWinners.BackColor = Color.Gray;
                btnWinner.Enabled = false;
                btnWinner.BackColor = Color.Gray;
                break;
                
            case FFFFlowState.IntroPlaying:
                btnIntroExplain.Enabled = false;
                btnIntroExplain.BackColor = Color.Yellow;
                break;
                
            case FFFFlowState.QuestionReady:
                btnIntroExplain.Enabled = false;
                btnIntroExplain.BackColor = Color.Gray;
                btnShowQuestion.Enabled = true;
                btnShowQuestion.BackColor = Color.LightGreen;
                break;
                
            case FFFFlowState.QuestionShown:
                btnShowQuestion.Enabled = false;
                btnShowQuestion.BackColor = Color.Yellow;
                btnRevealAnswers.Enabled = true;
                btnRevealAnswers.BackColor = Color.LightGreen;
                break;
                
            case FFFFlowState.AnswersRevealing:
            case FFFFlowState.AnswersRevealed:
                btnShowQuestion.Enabled = false;
                btnShowQuestion.BackColor = Color.Gray;
                btnRevealAnswers.Enabled = false;
                btnRevealAnswers.BackColor = Color.Yellow;
                break;
                
            case FFFFlowState.TimerExpired:
                btnRevealAnswers.BackColor = Color.Gray;
                btnRevealCorrect.Enabled = true;
                btnRevealCorrect.BackColor = Color.LightGreen;
                btnRevealCorrect.Text = "4. Reveal Correct";
                break;
                
            case FFFFlowState.RevealingCorrect:
                btnRevealCorrect.Enabled = true;
                btnRevealCorrect.BackColor = Color.Yellow;
                break;
                
            case FFFFlowState.WinnersShown:
                btnRevealCorrect.Enabled = false;
                btnRevealCorrect.BackColor = Color.Gray;
                btnShowWinners.Enabled = false;
                btnShowWinners.BackColor = Color.Gray;
                btnWinner.Enabled = true;
                btnWinner.BackColor = Color.LightGreen;
                break;
                
            case FFFFlowState.WinnerAnnounced:
            case FFFFlowState.Complete:
                btnWinner.Enabled = false;
                btnWinner.BackColor = Color.Gray;
                break;
        }
    }
    
    public void ResetFFFRound()
    {
        // Stop sounds and timer
        _soundService?.StopAllSounds();
        _fffTimer?.Stop();
        _isFFFActive = false;
        
        _currentQuestion = null;
        _submissions.Clear();
        _rankings.Clear();
        _currentState = FFFFlowState.NotStarted;
        _revealCorrectClickCount = 0;
        
        lstAnswers.Items.Clear();
        lstRankings.Items.Clear();
        lblAnswerCount.Text = "0 Answers";
        lblWinner.Text = "Winner: ---";
        lblTimer.Text = "00:00";
        
        ClearQuestionDetails();
        UpdateUIState();
    }
    
    #region New Button Handlers (Stubs for UI Preview)
    
    private void btnLoadQuestions_Click(object? sender, EventArgs e)
    {
        // Load questions from database
        _ = LoadQuestionsAsync();
    }
    
    private void btnRefreshParticipants_Click(object? sender, EventArgs e)
    {
        // Refresh participant list
        _ = RefreshParticipantsAsync();
    }
    
    private void btnIntroExplain_Click(object? sender, EventArgs e)
    {
        if (_soundService == null)
        {
            MessageBox.Show("Sound service not initialized.", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
            return;
        }
        
        _currentState = FFFFlowState.IntroPlaying;
        UpdateUIState();
        
        // Play FFFLightsDown, then FFFExplain
        // TODO Phase 4: Update TV screen with FFF title
        _soundService.PlaySound(SoundEffect.FFFLightsDown);
        
        // Wait for FFFLightsDown to finish, then play FFFExplain
        // For now, using async pattern to sequence sounds
        Task.Run(async () =>
        {
            await Task.Delay(3000); // Estimate for FFFLightsDown duration
            if (_soundService != null)
            {
                _soundService.PlaySound(SoundEffect.FFFExplain);
            }
            await Task.Delay(5000); // Estimate for FFFExplain duration
            
            // Move to next state
            if (InvokeRequired)
            {
                Invoke(() =>
                {
                    _currentState = FFFFlowState.QuestionReady;
                    UpdateUIState();
                    GameConsole.Log("[FFF] Step 1 complete - Ready for Step 2");
                });
            }
            else
            {
                _currentState = FFFFlowState.QuestionReady;
                UpdateUIState();
                GameConsole.Log("[FFF] Step 1 complete - Ready for Step 2");
            }
        });
    }
    
    private void btnShowQuestion_Click(object? sender, EventArgs e)
    {
        GameConsole.Log($"[FFF] btnShowQuestion_Click - Current State: {_currentState}");
        
        // Prevent double-click
        if (_currentState != FFFFlowState.QuestionReady)
        {
            GameConsole.Log($"[FFF] Button click ignored - state is {_currentState}, expected QuestionReady");
            return;
        }
        
        // Randomly select a question from loaded questions if not already selected
        if (_currentQuestion == null)
        {
            if (_questions == null || _questions.Count == 0)
            {
                MessageBox.Show("No questions loaded. Please wait for questions to load on startup.",
                    "No Questions", MessageBoxButtons.OK, MessageBoxIcon.None);
                return;
            }
            
            var random = new Random();
            var index = random.Next(_questions.Count);
            _currentQuestion = _questions[index];
            
            // Display question
            txtQuestionDisplay.Text = _currentQuestion.QuestionText;
            txtOption1.Text = _currentQuestion.AnswerA;
            txtOption2.Text = _currentQuestion.AnswerB;
            txtOption3.Text = _currentQuestion.AnswerC;
            txtOption4.Text = _currentQuestion.AnswerD;
            lblCorrectOrder.Text = $"Correct Order: {_currentQuestion.CorrectOrder}";
            _correctSequence = _currentQuestion.CorrectOrder;
        }
        
        if (_soundService == null)
        {
            MessageBox.Show("Sound service not initialized.", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
            return;
        }
        
        _currentState = FFFFlowState.QuestionShown;
        UpdateUIState();
        
        // Stop all sounds (end FFFExplain if still playing)
        _soundService.StopAllSounds();
        
        // Change state immediately - enables next button
        _currentState = FFFFlowState.QuestionShown;
        UpdateUIState();
        
        // TODO Phase 4: Display question on TV (no answers yet)
        
        // Play FFFReadQuestion
        _soundService.PlaySound(SoundEffect.FFFReadQuestion);
        GameConsole.Log("[FFF] Step 2 complete - Ready for Step 3");
    }
    
    private async void btnRevealAnswers_Click(object? sender, EventArgs e)
    {
        GameConsole.Log($"[FFF] btnRevealAnswers_Click - Current State: {_currentState}");
        
        if (_currentQuestion == null)
        {
            MessageBox.Show("No question selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
            return;
        }
        
        if (_soundService == null)
        {
            MessageBox.Show("Sound service not initialized.", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
            return;
        }
        
        GameConsole.Log("[FFF] Step 3: Reveal Answers started");
        
        _currentState = FFFFlowState.AnswersRevealing;
        UpdateUIState();
        
        // Stop all sounds (end FFFReadQuestion if still playing)
        _soundService.StopAllSounds();
        
        // Play FFFThreeNotes
        _soundService.PlaySound(SoundEffect.FFFThreeNotes);
        
        // Wait for FFFThreeNotes to complete
        await Task.Delay(3000); // Estimate for FFFThreeNotes duration
        
        // Randomize answer positions (NEVER in correct order A-B-C-D)
        var answers = new List<string>
        {
            _currentQuestion.AnswerA,
            _currentQuestion.AnswerB,
            _currentQuestion.AnswerC,
            _currentQuestion.AnswerD
        };
        
        var random = new Random();
        var randomized = answers.OrderBy(x => random.Next()).ToArray();
        _randomizedAnswers = randomized;
        
        // Send question and randomized answers to participants (START COMPETITION)
        if (_fffClient != null && _fffClient.IsConnected)
        {
            try
            {
                // TODO: Send randomized answers to participants via SignalR
                // await _fffClient.StartRoundAsync(_currentQuestion.Id, randomized);
                _fffStartTime = DateTime.Now;
                _isFFFActive = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting round: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                return;
            }
        }
        
        // TODO Phase 4: Display on TV with randomized answers
        
        // Update state and start timer (after transmission)
        lblTimer.Text = "00:20"; // Set initial display before timer starts
        _fffStartTime = DateTime.UtcNow;
        _isFFFActive = true;
        _fffTimer.Start();
        
        _currentState = FFFFlowState.AnswersRevealed;
        UpdateUIState();
        
        GameConsole.Log("[FFF] Starting timer...");
        GameConsole.Log($"[FFF] Timer started: {_fffTimer.Enabled}");
        
        // Play FFFThinking
        _soundService.PlaySound(SoundEffect.FFFThinking);
        
        // Wait for FFFThinking duration (approximately 20 seconds)
        await Task.Delay(20000); // Timer expires at right moment using fade-out gap
        
        _fffTimer.Stop();
        _isFFFActive = false;
        
        GameConsole.Log("[FFF] FFFThinking finished - Timer expired");
        
        // Play FFFReadAnswers in background (continues during Step 4: Reveal Correct)
        GameConsole.Log("[FFF] Playing FFFReadAnswers (background music)...");
        _soundService.PlaySound(SoundEffect.FFFReadAnswers);
        
        // Move to next state - ready to reveal correct answers
        _currentState = FFFFlowState.TimerExpired;
        UpdateUIState();
        GameConsole.Log("[FFF] Step 3 complete - Ready for Step 4");
    }
    
    private void btnRevealCorrect_Click(object? sender, EventArgs e)
    {
        _revealCorrectClickCount++;
        
        _currentState = FFFFlowState.RevealingCorrect;
        
        // TODO Phase 4: Highlight correct answer position on TV
        
        // Play appropriate sound for this reveal (1-4)
        // Note: These sound effects need to be added to sound packs
        // For now, using placeholder logic
        
        if (_revealCorrectClickCount < 4)
        {
            btnRevealCorrect.Text = $"4. Reveal Correct ({_revealCorrectClickCount}/4)";
            // TODO: Play sound for revealing answer 1, 2, or 3
            // _soundService?.PlaySound(SoundEffect.FFFRevealCorrect1); // etc
        }
        else
        {
            // Fourth click - all revealed
            btnRevealCorrect.Text = "4. Reveal Correct";
            // TODO: Play sound for revealing 4th answer
            
            // Calculate rankings if not already done
            if (_rankings.Count == 0 && _submissions.Count > 0)
            {
                CalculateRankings();
            }
            
            // Move to winners shown state
            _currentState = FFFFlowState.WinnersShown;
            _revealCorrectClickCount = 0; // Reset for next round
        }
        
        UpdateUIState();
    }
    
    private void CalculateRankings()
    {
        // Calculate rankings based on correct answers and time
        var rankings = new List<RankingResult>();
        int rank = 1;
        
        foreach (var submission in _submissions.OrderBy(s => (s.SubmittedAt - _fffStartTime).TotalMilliseconds))
        {
            var isCorrect = submission.AnswerSequence == _correctSequence;
            rankings.Add(new RankingResult
            {
                Rank = rank++,
                ParticipantId = submission.ParticipantId,
                DisplayName = submission.DisplayName,
                AnswerSequence = submission.AnswerSequence,
                TimeElapsed = (submission.SubmittedAt - _fffStartTime).TotalMilliseconds,
                IsCorrect = isCorrect
            });
        }
        
        UpdateRankings(rankings);
    }
    
    private void btnShowWinners_Click(object? sender, EventArgs e)
    {
        // TODO Phase 4: Display list of winners on TV
        // Clear TV screen and show names of participants who answered correctly
        
        _currentState = FFFFlowState.WinnersShown;
        UpdateUIState();
    }
    
    private async void btnWinner_Click(object? sender, EventArgs e)
    {
        if (_soundService == null || _rankings.Count == 0)
        {
            MessageBox.Show("No rankings available.", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
            return;
        }
        
        var correctAnswers = _rankings.Where(r => r.IsCorrect).ToList();
        
        if (correctAnswers.Count == 0)
        {
            MessageBox.Show("No correct answers - no winner to announce.", "No Winner", MessageBoxButtons.OK, MessageBoxIcon.None);
            return;
        }
        
        if (correctAnswers.Count == 1)
        {
            // Only 1 winner - auto-win
            var winner = correctAnswers[0];
            lblWinner.Text = $"Winner: 🏆 {winner.DisplayName}";
            lblWinner.ForeColor = Color.Green;
            
            // TODO Phase 4: Display winner celebration on TV
            
            _soundService.PlaySound(SoundEffect.FFFWinner);
            await Task.Delay(3000);
            _soundService.PlaySound(SoundEffect.FFFWalkDown);
        }
        else
        {
            // Multiple winners - reveal times slowest to fastest
            // TODO Phase 4: Display winner list with times on TV, highlight fastest
            
            var winner = correctAnswers[0]; // Already sorted by time (fastest first)
            lblWinner.Text = $"Winner: 🏆 {winner.DisplayName} ({winner.TimeElapsed / 1000.0:F2}s)";
            lblWinner.ForeColor = Color.Green;
            
            _soundService.PlaySound(SoundEffect.FFFWinner);
            await Task.Delay(3000);
            _soundService.PlaySound(SoundEffect.FFFWalkDown);
        }
        
        _currentState = FFFFlowState.WinnerAnnounced;
        UpdateUIState();
        GameConsole.Log("[FFF] Step 6 complete - FFF Round finished");
        
        // TODO: Notify main control panel of winner
    }
    
    private void btnStopAudio_Click(object? sender, EventArgs e)
    {
        _soundService?.StopAllSounds();
    }
    
    #endregion
}

/// <summary>
/// Participant information for display
/// </summary>
public class ParticipantInfo
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

/// <summary>
/// Answer submission information
/// </summary>
public class AnswerSubmission
{
    public string ParticipantId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string AnswerSequence { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
}

/// <summary>
/// Ranking result for display
/// </summary>
public class RankingResult
{
    public int Rank { get; set; }
    public string ParticipantId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string AnswerSequence { get; set; } = string.Empty;
    public double TimeElapsed { get; set; }
    public bool IsCorrect { get; set; }
}

/// <summary>
/// Helper class for ComboBox items
/// </summary>
internal class ComboBoxItem
{
    public string Text { get; set; } = string.Empty;
    public object? Value { get; set; }
    
    public override string ToString() => Text;
}
