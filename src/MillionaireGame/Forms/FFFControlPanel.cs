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
        
        // Initialize UI state when control is fully loaded
        this.Load += (s, e) => UpdateUIState();
        
        // Also try immediately after InitializeComponent
        UpdateUIState();
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
                GameConsole.Log($"[FFF] Loaded {_questions.Count} FFF questions from database.");
            }
            else
            {
                GameConsole.Log("[FFF] ERROR: FFF Repository is not initialized.");
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
    public void UpdateUIState()
    {
        var hasQuestionsAvailable = (_questions?.Count ?? 0) > 0;
        var hasParticipants = _participants.Count > 0;
        
        GameConsole.Log($"[FFF] UpdateUIState - State: {_currentState}, HasQuestionsAvailable: {hasQuestionsAvailable}, HasParticipants: {hasParticipants}, Questions: {(_questions?.Count ?? 0)}");
        
        // Update button enabled states based on game flow
        switch (_currentState)
        {
            case FFFFlowState.NotStarted:
                btnIntroExplain.Enabled = hasQuestionsAvailable && hasParticipants;
                btnIntroExplain.BackColor = btnIntroExplain.Enabled ? Color.LightGreen : Color.Gray;
                GameConsole.Log($"[FFF] NotStarted - Button enabled: {btnIntroExplain.Enabled}");
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
                btnShowQuestion.Enabled = true; // Allow interrupting intro/explain
                btnShowQuestion.BackColor = Color.LightGreen;
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
    
    /// <summary>
    /// Reset FFF round state - can be called from parent Control Panel reset buttons
    /// </summary>
    public void ResetFFFRound()
    {
        // Stop sounds and timer
        _soundService?.StopAllSounds();
        _fffTimer?.Stop();
        
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
        
        GameConsole.Log("[FFF] Round reset from Control Panel");
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
        
        GameConsole.Log("[FFF] Step 1: Intro/Explain started");
        
        // Queue FFFLightsDown and FFFExplain for seamless playback
        // TODO Phase 4: Update TV screen with FFF title
        Task.Run(async () =>
        {
            GameConsole.Log("[FFF] Queueing FFFLightsDown and FFFExplain...");
            
            // Queue both sounds upfront for seamless crossfade
            _soundService.QueueSound(SoundEffect.FFFLightsDown, AudioPriority.Normal);
            _soundService.QueueSound(SoundEffect.FFFExplain, AudioPriority.Normal);
            
            // Wait for LightsDown to finish (FFFExplain will be queued and waiting)
            GameConsole.Log("[FFF] Waiting for FFFLightsDown to complete...");
            while (_soundService.GetTotalSoundCount() > 1)
            {
                await Task.Delay(100);
            }
            GameConsole.Log("[FFF] FFFLightsDown finished, FFFExplain now playing");
            
            // Move to next state immediately after LightsDown
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
            
            // Wait for FFFExplain to finish
            while (_soundService.IsQueuePlaying())
            {
                await Task.Delay(100);
            }
            GameConsole.Log("[FFF] FFFExplain finished");
        });
    }
    
    private void btnShowQuestion_Click(object? sender, EventArgs e)
    {
        // Prevent double-click
        if (_currentState != FFFFlowState.QuestionReady)
        {
            return;
        }
        
        // Randomly select a question from loaded questions if not already selected
        if (_currentQuestion == null)
        {
            if (_questions == null || _questions.Count == 0)
            {
                GameConsole.Log("[FFF] ERROR: No questions loaded. Please wait for questions to load on startup.");
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
        
        GameConsole.Log("[FFF] Step 2: Show Question started");
        
        // TODO Phase 4: Display question on TV (no answers yet)
        
        // Change state immediately - enables next button
        _currentState = FFFFlowState.QuestionShown;
        UpdateUIState();
        GameConsole.Log("[FFF] Step 2 complete - Ready for Step 3");
        
        // Stop sounds and play in background (can be interrupted by next button)
        Task.Run(async () =>
        {
            _soundService.StopAllSounds();
            GameConsole.Log("[FFF] Playing FFFReadQuestion...");
            await Task.Run(() => _soundService.PlaySound(SoundEffect.FFFReadQuestion));
            GameConsole.Log("[FFF] FFFReadQuestion finished");
        });
    }
    
    private async void btnRevealAnswers_Click(object? sender, EventArgs e)
    {
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
        
        // Play sounds sequentially in background thread - randomize/transmit AFTER cue plays
        Task.Run(async () =>
        {
            // Stop all sounds (end FFFReadQuestion if still playing)
            _soundService.StopAllSounds();
            
            // Play FFFThreeNotes first (the cue sound)
            GameConsole.Log("[FFF] Playing FFFThreeNotes...");
            await Task.Run(() => _soundService.PlaySound(SoundEffect.FFFThreeNotes));
            GameConsole.Log("[FFF] FFFThreeNotes finished");
            
            // NOW randomize answer positions (after cue finishes)
            var answers = new List<string>
            {
                _currentQuestion.AnswerA,
                _currentQuestion.AnswerB,
                _currentQuestion.AnswerC,
                _currentQuestion.AnswerD
            };
            
            var random = new Random();
            var randomized = answers.OrderBy(x => random.Next()).ToArray();
            
            // Ensure randomized order doesn't match correct order
            var currentSequence = GetAnswerSequence(_currentQuestion, randomized);
            if (currentSequence == _currentQuestion.CorrectOrder)
            {
                // Randomization matched correct order - swap two answers to change it
                (randomized[0], randomized[1]) = (randomized[1], randomized[0]);
            }
            
            _randomizedAnswers = randomized;
            
            // Send question and randomized answers to participants via SignalR
            if (_fffClient != null && _fffClient.IsConnected)
            {
                try
                {
                    GameConsole.Log("[FFF] Sending question to participants...");
                    await _fffClient.StartQuestionAsync(_currentQuestion.Id, _currentQuestion.QuestionText, randomized, 20);
                    GameConsole.Log("[FFF] Question transmitted successfully");
                }
                catch (Exception ex)
                {
                    GameConsole.Log($"[FFF] Error starting round: {ex.Message}");
                    if (InvokeRequired)
                    {
                        Invoke(() => MessageBox.Show($"Error starting round: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.None));
                    }
                    else
                    {
                        MessageBox.Show($"Error starting round: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
                    }
                    return;
                }
            }
            
            // TODO Phase 4: Display on TV with randomized answers
            
            // Update state and start timer (after transmission)
            if (InvokeRequired)
            {
                Invoke(() =>
                {
                    _currentState = FFFFlowState.AnswersRevealed;
                    UpdateUIState();
                    GameConsole.Log("[FFF] Starting timer...");
                    lblTimer.Text = "00:20"; // Set initial display before timer starts
                    _fffStartTime = DateTime.UtcNow;
                    _isFFFActive = true;
                    _fffTimer.Start();
                    GameConsole.Log($"[FFF] Timer started: {_fffTimer.Enabled}");
                });
            }
            else
            {
                _currentState = FFFFlowState.AnswersRevealed;
                UpdateUIState();
                GameConsole.Log("[FFF] Starting timer...");
                lblTimer.Text = "00:20"; // Set initial display before timer starts
                _fffStartTime = DateTime.UtcNow;
                _isFFFActive = true;
                _fffTimer.Start();
                GameConsole.Log($"[FFF] Timer started: {_fffTimer.Enabled}");
            }
            
            // Now play FFFThinking (20 seconds)
            GameConsole.Log("[FFF] Playing FFFThinking (20s)...");
            await Task.Run(() => _soundService.PlaySound(SoundEffect.FFFThinking));
            
            if (InvokeRequired)
            {
                Invoke(() =>
                {
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
                });
            }
            else
            {
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
        });
    }
    
    private void btnRevealCorrect_Click(object? sender, EventArgs e)
    {
        _revealCorrectClickCount++;
        
        _currentState = FFFFlowState.RevealingCorrect;
        
        GameConsole.Log($"[FFF] Step 4: Reveal Correct ({_revealCorrectClickCount}/4)");
        
        // Update button text immediately (before sound plays)
        if (_revealCorrectClickCount < 4)
        {
            btnRevealCorrect.Text = $"4. Reveal Correct ({_revealCorrectClickCount}/4)";
        }
        else
        {
            btnRevealCorrect.Text = "4. Reveal Correct";
        }
        
        // Update UI state immediately
        UpdateUIState();
        
        // TODO Phase 4: Highlight correct answer position on TV
        
        // Play appropriate sound for this reveal (1-4) in background
        // Note: FFFReadAnswers continues playing in background during all reveals
        var clickCount = _revealCorrectClickCount; // Capture for async use
        
        Task.Run(async () =>
        {
            switch (clickCount)
            {
                case 1:
                    GameConsole.Log("[FFF] Playing FFFOrder1...");
                    await Task.Run(() => _soundService.PlaySound(SoundEffect.FFFOrder1));
                    break;
                case 2:
                    GameConsole.Log("[FFF] Playing FFFOrder2...");
                    await Task.Run(() => _soundService.PlaySound(SoundEffect.FFFOrder2));
                    break;
                case 3:
                    GameConsole.Log("[FFF] Playing FFFOrder3...");
                    await Task.Run(() => _soundService.PlaySound(SoundEffect.FFFOrder3));
                    break;
                case 4:
                    GameConsole.Log("[FFF] Playing FFFOrder4...");
                    await Task.Run(() => _soundService.PlaySound(SoundEffect.FFFOrder4));
                    
                    // After final sound, move to next state
                    if (InvokeRequired)
                    {
                        Invoke(() =>
                        {
                            // Calculate rankings if not already done
                            if (_rankings.Count == 0 && _submissions.Count > 0)
                            {
                                CalculateRankings();
                            }
                            
                            GameConsole.Log("[FFF] Step 4 complete - Ready for Step 5");
                            
                            // Move to winners shown state
                            _currentState = FFFFlowState.WinnersShown;
                            _revealCorrectClickCount = 0; // Reset for next round
                            UpdateUIState();
                        });
                    }
                    else
                    {
                        // Calculate rankings if not already done
                        if (_rankings.Count == 0 && _submissions.Count > 0)
                        {
                            CalculateRankings();
                        }
                        
                        GameConsole.Log("[FFF] Step 4 complete - Ready for Step 5");
                        
                        // Move to winners shown state
                        _currentState = FFFFlowState.WinnersShown;
                        _revealCorrectClickCount = 0; // Reset for next round
                        UpdateUIState();
                    }
                    break;
            }
        });
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
    
    /// <summary>
    /// Get the answer sequence (e.g., "DABC") for a given randomized answer array
    /// </summary>
    private string GetAnswerSequence(FFFQuestion question, string[] randomizedAnswers)
    {
        var sequence = "";
        
        foreach (var answer in randomizedAnswers)
        {
            if (answer == question.AnswerA)
                sequence += "A";
            else if (answer == question.AnswerB)
                sequence += "B";
            else if (answer == question.AnswerC)
                sequence += "C";
            else if (answer == question.AnswerD)
                sequence += "D";
        }
        
        return sequence;
    }
    
    private void btnShowWinners_Click(object? sender, EventArgs e)
    {
        GameConsole.Log("[FFF] Step 5: Show Winners");
        
        // TODO Phase 4: Display list of winners on TV
        // Clear TV screen and show names of participants who answered correctly
        
        _currentState = FFFFlowState.WinnersShown;
        UpdateUIState();
        
        GameConsole.Log("[FFF] Step 5 complete - Ready for Step 6");
    }
    
    private void btnWinner_Click(object? sender, EventArgs e)
    {
        if (_soundService == null || _rankings.Count == 0)
        {
            MessageBox.Show("No rankings available.", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
            return;
        }
        
        GameConsole.Log("[FFF] Step 6: Confirm Winner started");
        
        var correctAnswers = _rankings.Where(r => r.IsCorrect).ToList();
        
        if (correctAnswers.Count == 0)
        {
            MessageBox.Show("No correct answers - no winner to announce.", "No Winner", MessageBoxButtons.OK, MessageBoxIcon.None);
            return;
        }
        
        // Stop FFFReadAnswers (playing in background from Step 3)
        _soundService.StopAllSounds();
        
        if (correctAnswers.Count == 1)
        {
            // Only 1 winner - auto-win
            var winner = correctAnswers[0];
            lblWinner.Text = $"Winner: 🏆 {winner.DisplayName}";
            lblWinner.ForeColor = Color.Green;
            
            GameConsole.Log($"[FFF] Winner: {winner.DisplayName}");
            
            // TODO Phase 4: Display winner celebration on TV
            
            // Update UI immediately
            _currentState = FFFFlowState.WinnerAnnounced;
            UpdateUIState();
            
            // Play sounds sequentially in background - don't block on UI updates
            Task.Run(async () =>
            {
                try
                {
                    GameConsole.Log("[FFF] Playing FFFWinner...");
                    await Task.Run(() => _soundService.PlaySound(SoundEffect.FFFWinner));
                    GameConsole.Log("[FFF] FFFWinner finished");
                    
                    if (_soundService != null)
                    {
                        GameConsole.Log("[FFF] Playing FFFWalkDown...");
                        await Task.Run(() => _soundService.PlaySound(SoundEffect.FFFWalkDown));
                        GameConsole.Log("[FFF] FFFWalkDown finished");
                    }
                    
                    GameConsole.Log("[FFF] Step 6 complete - FFF Round finished");
                }
                catch (Exception ex)
                {
                    GameConsole.Log($"[FFF] Error in winner announcement: {ex.Message}");
                }
            });
        }
        else
        {
            // Multiple winners - reveal times slowest to fastest
            // TODO Phase 4: Display winner list with times on TV, highlight fastest
            
            var winner = correctAnswers[0]; // Already sorted by time (fastest first)
            lblWinner.Text = $"Winner: 🏆 {winner.DisplayName} ({winner.TimeElapsed / 1000.0:F2}s)";
            lblWinner.ForeColor = Color.Green;
            
            GameConsole.Log($"[FFF] Winner: {winner.DisplayName} ({winner.TimeElapsed / 1000.0:F2}s) - {correctAnswers.Count} correct answers");
            
            // Update UI immediately
            _currentState = FFFFlowState.WinnerAnnounced;
            UpdateUIState();
            
            // Play sounds sequentially in background - don't block on UI updates
            Task.Run(async () =>
            {
                try
                {
                    GameConsole.Log("[FFF] Playing FFFWinner...");
                    await Task.Run(() => _soundService.PlaySound(SoundEffect.FFFWinner));
                    GameConsole.Log("[FFF] FFFWinner finished");
                    
                    if (_soundService != null)
                    {
                        GameConsole.Log("[FFF] Playing FFFWalkDown...");
                        await Task.Run(() => _soundService.PlaySound(SoundEffect.FFFWalkDown));
                        GameConsole.Log("[FFF] FFFWalkDown finished");
                    }
                    
                    GameConsole.Log("[FFF] Step 6 complete - FFF Round finished");
                }
                catch (Exception ex)
                {
                    GameConsole.Log($"[FFF] Error in winner announcement: {ex.Message}");
                }
            });
        }
        
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
