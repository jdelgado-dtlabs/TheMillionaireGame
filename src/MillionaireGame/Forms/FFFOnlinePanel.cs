using MillionaireGame.Web.Models;
using MillionaireGame.Web.Database;
using MillionaireGame.Services;
using MillionaireGame.Core.Database;
using MillionaireGame.Core.Settings;
using MillionaireGame.Utilities;
using System.Timers;

namespace MillionaireGame.Forms;

/// <summary>
/// Game flow states for FFF Online round
/// </summary>
public enum FFFFlowState
{
    NotStarted,          // Initial state, select question
    IntroPlaying,        // FFFLightsDown + FFFExplain playing
    QuestionReady,       // Ready to show question
    QuestionShown,       // Question displayed, FFFReadQuestion playing
    AnswersRevealing,    // FFFThreeNotes playing
    AnswersRevealed,     // Answers shown, FFFThinking playing, timer active
    TimerExpired,        // FFFReadCorrectOrder playing
    RevealingCorrect,    // Revealing correct answers (multi-click)
    WinnersShown,        // Winners list displayed
    WinnerAnnounced,     // Winner declared
    Complete             // Round complete
}

/// <summary>
/// User control for managing Fastest Finger First (FFF) rounds - ONLINE MODE (Web-based)
/// </summary>
public partial class FFFOnlinePanel : UserControl
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
    private SoundService? _soundService;
    private ScreenUpdateService? _screenService;
    
    public FFFOnlinePanel()
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
        
        // Stop audio on control disposal
        this.Disposed += FFFOnlinePanel_Disposed;
        
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
    
    private void FFFOnlinePanel_Disposed(object? sender, EventArgs e)
    {
        // Stop all audio to prevent it from continuing after disposal
        if (_soundService != null)
        {
            _ = _soundService.StopAllSoundsAsync();
            GameConsole.Log("[FFFOnlinePanel] Control disposed - stopping all audio");
        }
        _fffTimer?.Stop();
        _fffTimer?.Dispose();
    }
    
    /// <summary>
    /// Clears the TV screen to prepare for main game (called when FFF window closes)
    /// </summary>
    public void ClearScreenForMainGame()
    {
        if (_screenService != null)
        {
            _screenService.ShowQuestion(false);
            _screenService.RemoveAnswer("A");
            _screenService.RemoveAnswer("B");
            _screenService.RemoveAnswer("C");
            _screenService.RemoveAnswer("D");
            _screenService.ClearFFFDisplay();
            GameConsole.Log("[FFFControlPanel] Cleared TV screen for main game");
        }
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
            GameConsole.Error($"[FFFControlPanel] Error loading FFF questions: {ex.Message}");
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
            GameConsole.Error($"[FFFControlPanel] Connection Error: {ex.Message}");
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
            GameConsole.Error($"[FFFControlPanel] Failed to refresh participants: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Broadcast ResetToLobby message to all web clients
    /// </summary>
    public async Task BroadcastResetToLobbyAsync()
    {
        if (_fffClient != null && _fffClient.IsConnected)
        {
            await _fffClient.BroadcastPhaseMessageAsync("ResetToLobby", new { });
            GameConsole.Log("[FFFControlPanel] Broadcast ResetToLobby to web clients");
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
            // Only the fastest correct answer (Rank 1) gets a checkmark
            // Everyone else gets an X (slower correct answers are eliminated, incorrect answers are wrong)
            var icon = (ranking.Rank == 1 && ranking.IsCorrect) ? "✓" : "✗";
            var timeSeconds = ranking.TimeElapsed / 1000.0;
            var status = ranking.IsCorrect ? (ranking.Rank == 1 ? "" : " (too slow)") : " (incorrect)";
            lstRankings.Items.Add($"#{ranking.Rank} {icon} {ranking.DisplayName} ({timeSeconds:F2}s){status}");
        }
        
        // Don't set winner label here - it should only be set when Winner button is clicked
        // Just show how many correct answers there are
        var correctCount = rankings.Count(r => r.IsCorrect);
        if (correctCount == 0)
        {
            lblWinner.Text = "Winner: ---";
            lblWinner.ForeColor = SystemColors.ControlText;
        }
        else if (correctCount == 1)
        {
            lblWinner.Text = $"Winner: Ready (1 correct)";
            lblWinner.ForeColor = Color.Blue;
        }
        else
        {
            lblWinner.Text = $"Winner: Ready ({correctCount} correct - reveal times)";
            lblWinner.ForeColor = Color.Blue;
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
            Invoke(new Action(() => 
            {
                lblTimer.Text = display;
                // Update screen timer (use remaining seconds, not display seconds)
                _screenService?.ShowPAFTimer((int)Math.Ceiling(remaining.TotalSeconds), "FFF");
            }));
        }
        else
        {
            lblTimer.Text = display;
            _screenService?.ShowPAFTimer((int)Math.Ceiling(remaining.TotalSeconds), "FFF");
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
    /// Set screen update service for TV display
    /// </summary>
    public void SetScreenService(ScreenUpdateService screenService)
    {
        _screenService = screenService;
        GameConsole.Log("[FFFControlPanel] Screen service set");
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
                // Check if we've completed all 4 reveals
                if (_revealCorrectClickCount == 0 && _rankings.Count > 0)
                {
                    // Step 4 complete - check if we need Show Winners button
                    var correctCount = _rankings.Count(r => r.IsCorrect);
                    GameConsole.Log($"[FFF] RevealingCorrect complete - {correctCount} correct answers");
                    
                    if (correctCount > 1)
                    {
                        // Multiple correct - enable Show Winners button
                        GameConsole.Log("[FFF] Multiple winners - enabling Show Winners button");
                        btnRevealCorrect.Enabled = false;
                        btnRevealCorrect.BackColor = Color.Gray;
                        btnShowWinners.Enabled = true;
                        btnShowWinners.BackColor = Color.LightGreen;
                        btnWinner.Enabled = false;
                        btnWinner.BackColor = Color.Gray;
                    }
                    else
                    {
                        // Single/no correct - skip Show Winners, go straight to Confirm Winner
                        GameConsole.Log($"[FFF] Only {correctCount} correct answer(s) - skipping Show Winners, enabling Confirm Winner directly");
                        btnRevealCorrect.Enabled = false;
                        btnRevealCorrect.BackColor = Color.Gray;
                        btnShowWinners.Enabled = false;
                        btnShowWinners.BackColor = Color.Gray;
                        btnWinner.Enabled = true;
                        btnWinner.BackColor = Color.LightGreen;
                    }
                }
                else
                {
                    // Still revealing (clicks 1-3)
                    btnRevealCorrect.Enabled = true;
                    btnRevealCorrect.BackColor = Color.Yellow;
                }
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
        // TODO [PRE-1.0]: FFF Online TV screen animations
        // Status: Remaining work (2-3 hours) - Task #1 in PRE_1.0_FINAL_CHECKLIST.md
        // Priority: MEDIUM-HIGH - FFF Online is 80% complete
        // See: docs/active/PRE_1.0_FINAL_CHECKLIST.md for requirements
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
            _currentState = FFFFlowState.QuestionReady;
            
            if (InvokeRequired)
            {
                Invoke(() => UpdateUIState());
            }
            else
            {
                UpdateUIState();
            }
            
            GameConsole.Log("[FFF] Step 1 complete - Ready for Step 2");
            
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
        
        // ALWAYS select a new random question when Show Question is clicked
        if (_questions == null || _questions.Count == 0)
        {
            GameConsole.Error("[FFF] No questions loaded. Please click Load Questions first.");
            return;
        }
        
        // Select random question
        var random = new Random();
        var index = random.Next(_questions.Count);
        _currentQuestion = _questions[index];
        
        GameConsole.Log($"[FFF] Selected question ID: {_currentQuestion.Id}, Text: {_currentQuestion.QuestionText}");
        
        // Display question in control panel (for host reference)
        txtQuestionDisplay.Text = _currentQuestion.QuestionText;
        txtOption1.Text = _currentQuestion.AnswerA;
        txtOption2.Text = _currentQuestion.AnswerB;
        txtOption3.Text = _currentQuestion.AnswerC;
        txtOption4.Text = _currentQuestion.AnswerD;
        lblCorrectOrder.Text = $"Correct Order: {_currentQuestion.CorrectOrder}";
        
        if (_soundService == null)
        {
            MessageBox.Show("Sound service not initialized.", "Error", MessageBoxButtons.OK, MessageBoxIcon.None);
            return;
        }
        
        GameConsole.Log("[FFF] Step 2: Show Question started");
        
        // Display ONLY question text on TV screen (no answers yet)
        var displayQuestion = new MillionaireGame.Core.Models.Question
        {
            QuestionText = _currentQuestion.QuestionText,
            AnswerA = _currentQuestion.AnswerA,
            AnswerB = _currentQuestion.AnswerB,
            AnswerC = _currentQuestion.AnswerC,
            AnswerD = _currentQuestion.AnswerD,
            CorrectAnswer = "A" // Not used for FFF display
        };
        _screenService?.UpdateQuestion(displayQuestion);
        _screenService?.ShowQuestion(true); // Make question visible on TV screen
        // Do NOT show answers yet - wait for Reveal Answers button
        GameConsole.Log($"[FFF] Question displayed on TV (answers hidden). A:{displayQuestion.AnswerA}, B:{displayQuestion.AnswerB}, C:{displayQuestion.AnswerC}, D:{displayQuestion.AnswerD}");
        
        // Change state immediately - enables next button
        _currentState = FFFFlowState.QuestionShown;
        UpdateUIState();
        GameConsole.Log("[FFF] Step 2 complete - Ready for Step 3");
        
        // Stop Music Channel first (in case background music is playing)
        _soundService.StopAllSounds(fadeout: false);
        
        // Play with Immediate priority (stops any previous sounds)
        GameConsole.Log("[FFF] Playing FFFReadQuestion...");
        _soundService.QueueSound(SoundEffect.FFFReadQuestion, AudioPriority.Immediate);
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
        
        // Stop Music Channel first (in case background music is playing)
        _soundService.StopAllSounds(fadeout: false);
        
        // Play FFFThreeNotes with Immediate priority (stops FFFReadQuestion if still playing)
        GameConsole.Log("[FFF] Playing FFFThreeNotes...");
        _soundService.QueueSound(SoundEffect.FFFThreeNotes, AudioPriority.Immediate);
        
        // Wait for FFFThreeNotes to finish, then transmit and play FFFThinking
        _ = Task.Run(async () =>
        {
            // Wait for FFFThreeNotes to complete
            while (_soundService.IsQueuePlaying())
            {
                await Task.Delay(100);
            }
            
            GameConsole.Log("[FFF] FFFThreeNotes finished, starting transmission...");
            
            // Prepare answers in original order (A, B, C, D)
            var answers = new string[]
            {
                _currentQuestion.AnswerA,
                _currentQuestion.AnswerB,
                _currentQuestion.AnswerC,
                _currentQuestion.AnswerD
            };
            
            // Send question and answers to participants via SignalR
            if (_fffClient != null && _fffClient.IsConnected)
            {
                try
                {
                    GameConsole.Log("[FFF] Sending question to participants...");
                    await _fffClient.StartQuestionAsync(_currentQuestion.Id, _currentQuestion.QuestionText, answers, 20);
                    GameConsole.Log("[FFF] Question transmitted successfully");
                }
                catch (Exception ex)
                {
                    GameConsole.Log($"[FFF] Error starting round: {ex.Message}");
                    GameConsole.Error($"[FFF] Failed to start round - {ex.Message}");
                    return; // Don't proceed if transmission failed
                }
            }
            else
            {
                GameConsole.Warn("[FFF] SignalR client not connected - skipping transmission");
            }
            
            // Display answers on TV in normal order (A, B, C, D)
            if (InvokeRequired)
            {
                Invoke(() =>
                {
                    _screenService?.ShowAnswer("A");
                    _screenService?.ShowAnswer("B");
                    _screenService?.ShowAnswer("C");
                    _screenService?.ShowAnswer("D");
                    
                    // Show timer on screen (20 seconds)
                    _screenService?.ShowPAFTimer(20, "FFF");
                    
                    GameConsole.Log("[FFF] Answers and timer displayed on TV in original order");
                });
            }
            else
            {
                _screenService?.ShowAnswer("A");
                _screenService?.ShowAnswer("B");
                _screenService?.ShowAnswer("C");
                _screenService?.ShowAnswer("D");
                
                // Show timer on screen (20 seconds)
                _screenService?.ShowPAFTimer(20, "FFF");
                
                GameConsole.Log("[FFF] Answers and timer displayed on TV in original order");
            }
            
            // Only proceed if transmission was successful
            // Update state and start timer
            _currentState = FFFFlowState.AnswersRevealed;
            GameConsole.Log("[FFF] Starting timer...");
            _fffStartTime = DateTime.UtcNow;
            _isFFFActive = true;
            _fffTimer.Start();
            GameConsole.Log($"[FFF] Timer started: {_fffTimer.Enabled}");
            
            // Update UI (needs Invoke for label update)
            if (InvokeRequired)
            {
                Invoke(() =>
                {
                    lblTimer.Text = "00:20"; // Set initial display before timer starts
                    UpdateUIState();
                });
            }
            else
            {
                lblTimer.Text = "00:20";
                UpdateUIState();
            }
            
            // Now queue FFFThinking (20 seconds) with Normal priority
            GameConsole.Log("[FFF] Queueing FFFThinking (20s)...");
            _soundService.QueueSound(SoundEffect.FFFThinking, AudioPriority.Normal);
            
            // Wait for FFFThinking to finish
            while (_soundService.IsQueuePlaying())
            {
                await Task.Delay(100);
            }
            
            _fffTimer.Stop();
            _isFFFActive = false;
            GameConsole.Log("[FFF] FFFThinking finished - Timer expired");
            
            // Broadcast TimerExpired to web clients
            if (_fffClient != null && _fffClient.IsConnected)
            {
                await _fffClient.BroadcastPhaseMessageAsync("TimerExpired", new { });
                GameConsole.Log("[FFF] Broadcast TimerExpired to web clients");
            }
            
            // Play FFFReadCorrectOrder once on Music Channel (background bed)
            GameConsole.Log("[FFF] Playing FFFReadCorrectOrder (background bed)...");
            _soundService.PlaySound(SoundEffect.FFFReadCorrectOrder, loop: false);
            
            // Move to next state - ready to reveal correct answers
            _currentState = FFFFlowState.TimerExpired;
            
            if (InvokeRequired)
            {
                Invoke(() => UpdateUIState());
            }
            else
            {
                UpdateUIState();
            }
            
            GameConsole.Log("[FFF] Step 3 complete - Ready for Step 4");
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
        
        if (_soundService == null)
        {
            GameConsole.Warn("[FFF] Sound service not available");
            return;
        }
        
        // On first click, clear all answers from TV to prepare for reveal
        if (_revealCorrectClickCount == 1)
        {
            _screenService?.RemoveAnswer("A");
            _screenService?.RemoveAnswer("B");
            _screenService?.RemoveAnswer("C");
            _screenService?.RemoveAnswer("D");
            GameConsole.Log("[FFF] Cleared answers from TV");
        }
        
        // Play appropriate sound for this reveal (1-4) over background music
        // Note: FFFReadCorrectOrder continues playing in background during all reveals
        var clickCount = _revealCorrectClickCount; // Capture for async use
        
        // Reveal answer in correct order position
        // Example: If correct order is "CBAD", click 1 shows C in position A, click 2 shows B in position B, etc.
        if (_currentQuestion != null && !string.IsNullOrEmpty(_currentQuestion.CorrectOrder))
        {
            // Split string into individual characters (e.g., "CBAD" → ["C", "B", "A", "D"])
            var correctOrder = _currentQuestion.CorrectOrder.Select(c => c.ToString()).ToArray();
            
            GameConsole.Log($"[FFF] CorrectOrder parsed: [{string.Join(", ", correctOrder)}], Length: {correctOrder.Length}");
            
            if (clickCount <= correctOrder.Length)
            {
                var answerLetter = correctOrder[clickCount - 1]; // Which answer (A, B, C, or D)
                var position = clickCount; // Which position (1=A, 2=B, 3=C, 4=D)
                var positionLetter = position switch { 1 => "A", 2 => "B", 3 => "C", 4 => "D", _ => "A" };
                
                // Store original answer texts before any modifications
                var originalAnswers = new Dictionary<string, string>
                {
                    ["A"] = _currentQuestion.AnswerA,
                    ["B"] = _currentQuestion.AnswerB,
                    ["C"] = _currentQuestion.AnswerC,
                    ["D"] = _currentQuestion.AnswerD
                };
                
                // Build progressively revealed question with answers in correct order positions
                // Set custom labels to preserve original letter order (e.g., if CBDA, show "C:", "D:", "B:", "A:")
                var reorderedQuestion = new MillionaireGame.Core.Models.Question
                {
                    QuestionText = _currentQuestion.QuestionText,
                    AnswerA = clickCount >= 1 && correctOrder.Length >= 1 ? originalAnswers[correctOrder[0]] : "",
                    AnswerB = clickCount >= 2 && correctOrder.Length >= 2 ? originalAnswers[correctOrder[1]] : "",
                    AnswerC = clickCount >= 3 && correctOrder.Length >= 3 ? originalAnswers[correctOrder[2]] : "",
                    AnswerD = clickCount >= 4 && correctOrder.Length >= 4 ? originalAnswers[correctOrder[3]] : "",
                    AnswerALabel = clickCount >= 1 && correctOrder.Length >= 1 ? correctOrder[0] : null,
                    AnswerBLabel = clickCount >= 2 && correctOrder.Length >= 2 ? correctOrder[1] : null,
                    AnswerCLabel = clickCount >= 3 && correctOrder.Length >= 3 ? correctOrder[2] : null,
                    AnswerDLabel = clickCount >= 4 && correctOrder.Length >= 4 ? correctOrder[3] : null,
                    CorrectAnswer = "A"
                };
                
                _screenService?.UpdateQuestion(reorderedQuestion);
                
                // Show all answers up to current click (not just the new one)
                for (int i = 1; i <= clickCount; i++)
                {
                    var pos = i switch { 1 => "A", 2 => "B", 3 => "C", 4 => "D", _ => "A" };
                    _screenService?.ShowAnswer(pos);
                }
                
                GameConsole.Log($"[FFF] Revealed position {position} ({positionLetter}): Showing {correctOrder[clickCount - 1]}'s text = '{originalAnswers[answerLetter]}'. Total visible: {clickCount}");
            }
        }
        
        switch (clickCount)
        {
            case 1:
                GameConsole.Log("[FFF] Playing FFFOrder1 (immediate)...");
                _soundService.QueueSound(SoundEffect.FFFOrder1, AudioPriority.Immediate);
                break;
            case 2:
                GameConsole.Log("[FFF] Playing FFFOrder2 (immediate)...");
                _soundService.QueueSound(SoundEffect.FFFOrder2, AudioPriority.Immediate);
                break;
            case 3:
                GameConsole.Log("[FFF] Playing FFFOrder3 (immediate)...");
                _soundService.QueueSound(SoundEffect.FFFOrder3, AudioPriority.Immediate);
                break;
            case 4:
                GameConsole.Log("[FFF] Playing FFFOrder4 (immediate)...");
                _soundService.QueueSound(SoundEffect.FFFOrder4, AudioPriority.Immediate);
                
                // Broadcast RevealingWinner to web clients after 4th click
                if (_fffClient != null && _fffClient.IsConnected)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _fffClient.BroadcastPhaseMessageAsync("RevealingWinner", new { });
                            GameConsole.Log("[FFF] Broadcast RevealingWinner to web clients");
                        }
                        catch (Exception ex)
                        {
                            GameConsole.Error($"[FFF] Error broadcasting RevealingWinner: {ex.Message}");
                        }
                    });
                }
                
                // After final sound, move to next state
                // Calculate rankings if not already done
                if (_rankings.Count == 0 && _submissions.Count > 0)
                {
                    CalculateRankings();
                }
                
                // Check how many correct answers there are
                var correctCount = _rankings.Count(r => r.IsCorrect);
                GameConsole.Log($"[FFF] Step 4 complete - {correctCount} correct answer(s)");
                
                // State changes are thread-safe
                if (correctCount > 1)
                {
                    // Multiple correct answers - enable "Show Winners" button to reveal times
                    _currentState = FFFFlowState.RevealingCorrect; // Stay in this state
                    GameConsole.Log("[FFF] Multiple correct answers - enabling Show Winners button");
                }
                else
                {
                    // Only 1 or 0 correct answers - skip Show Winners, go directly to Winner button
                    _currentState = FFFFlowState.WinnersShown;
                    GameConsole.Log("[FFF] Single/No correct answer - skipping to Winner button");
                }
                
                _revealCorrectClickCount = 0; // Reset for next round
                
                // Only UpdateUIState needs UI thread
                if (InvokeRequired)
                    Invoke(UpdateUIState);
                else
                    UpdateUIState();
                break;
        }
    }
    
    /// <summary>
    /// Helper method to get answer text for a specific position during correct order reveal
    /// </summary>
    private string GetAnswerTextForPosition(int position, string[] correctOrder)
    {
        if (_currentQuestion == null || position < 1 || position > correctOrder.Length)
            return string.Empty;
            
        // Get which answer letter should be in this position
        var answerLetter = correctOrder[position - 1]; // position 1 = index 0
        
        return answerLetter switch
        {
            "A" => _currentQuestion.AnswerA,
            "B" => _currentQuestion.AnswerB,
            "C" => _currentQuestion.AnswerC,
            "D" => _currentQuestion.AnswerD,
            _ => string.Empty
        };
    }
    
    private void CalculateRankings()
    {
        // Calculate rankings based on correct answers and time
        var rankings = new List<RankingResult>();
        
        // First, check each submission for correctness
        var submissionsWithCorrectness = _submissions.Select(s => new
        {
            Submission = s,
            IsCorrect = s.AnswerSequence == _currentQuestion?.CorrectOrder,
            TimeElapsed = (s.SubmittedAt - _fffStartTime).TotalMilliseconds
        }).ToList();
        
        // Separate correct and incorrect answers
        var correctAnswers = submissionsWithCorrectness
            .Where(s => s.IsCorrect)
            .OrderBy(s => s.TimeElapsed)
            .ToList();
        
        var incorrectAnswers = submissionsWithCorrectness
            .Where(s => !s.IsCorrect)
            .OrderBy(s => s.TimeElapsed)
            .ToList();
        
        GameConsole.Log($"[FFF] CalculateRankings: {correctAnswers.Count} correct, {incorrectAnswers.Count} incorrect");
        
        // Rank correct answers first (by time)
        int rank = 1;
        foreach (var item in correctAnswers)
        {
            rankings.Add(new RankingResult
            {
                Rank = rank++,
                ParticipantId = item.Submission.ParticipantId,
                DisplayName = item.Submission.DisplayName,
                AnswerSequence = item.Submission.AnswerSequence,
                TimeElapsed = item.TimeElapsed,
                IsCorrect = true
            });
            GameConsole.Log($"[FFF] Rank {rank - 1}: {item.Submission.DisplayName} - CORRECT - {item.TimeElapsed / 1000.0:F2}s");
        }
        
        // Then rank incorrect answers (by time)
        foreach (var item in incorrectAnswers)
        {
            rankings.Add(new RankingResult
            {
                Rank = rank++,
                ParticipantId = item.Submission.ParticipantId,
                DisplayName = item.Submission.DisplayName,
                AnswerSequence = item.Submission.AnswerSequence,
                TimeElapsed = item.TimeElapsed,
                IsCorrect = false
            });
            GameConsole.Log($"[FFF] Rank {rank - 1}: {item.Submission.DisplayName} - INCORRECT - {item.TimeElapsed / 1000.0:F2}s");
        }
        
        UpdateRankings(rankings);
    }
    

    
    private void btnShowWinners_Click(object? sender, EventArgs e)
    {
        GameConsole.Log("[FFF] Step 5: Show Winners");
        
        // Display top 8 correct winners on TV (alphabetically sorted)
        // DEFENSIVE: Filter for IsCorrect=true to ensure only correct answers are shown
        var correctWinners = _rankings.Where(r => r.IsCorrect)
                                      .OrderBy(r => r.DisplayName)
                                      .Take(8)
                                      .ToList();
        
        GameConsole.Log($"[FFF] Found {correctWinners.Count} correct winners to display:");
        foreach (var w in correctWinners)
        {
            GameConsole.Log($"[FFF]   - {w.DisplayName} ({w.TimeElapsed / 1000.0:F2}s) IsCorrect={w.IsCorrect}");
        }
        
        // Sanity check: Should never show winners if there's only 0 or 1 correct
        if (correctWinners.Count <= 1)
        {
            GameConsole.Error($"[FFF] ERROR: Show Winners called with {correctWinners.Count} correct winners. This should not happen!");
            GameConsole.Error("[FFF] Skipping Show Winners and proceeding to Confirm Winner...");
            btnWinner_Click(sender, e);
            return;
        }
        
        // Play FFFWhoWasCorrect over the background music
        if (_soundService != null)
        {
            GameConsole.Log("[FFF] Playing FFFWhoWasCorrect (over background)...");
            _soundService.QueueSound(SoundEffect.FFFWhoWasCorrect, AudioPriority.Normal);
        }
        
        if (correctWinners.Count > 0)
        {
            // Clear question and answers from all screens (TV, Host, Guest)
            // Update with blank question to clear text while keeping straps visible
            var blankQuestion = new MillionaireGame.Core.Models.Question
            {
                QuestionText = "",
                AnswerA = "",
                AnswerB = "",
                AnswerC = "",
                AnswerD = "",
                CorrectAnswer = "A"
            };
            _screenService?.UpdateQuestion(blankQuestion);
            
            // Clear timer from all screens
            _screenService?.ShowPAFTimer(0, "Completed");
            
            // Clear answers from TV only
            _screenService?.ShowQuestion(false);
            _screenService?.RemoveAnswer("A");
            _screenService?.RemoveAnswer("B");
            _screenService?.RemoveAnswer("C");
            _screenService?.RemoveAnswer("D");
            GameConsole.Log("[FFF] Cleared question/answers/timer from all screens");
            
            // Extract names and times (convert milliseconds to seconds)
            var names = correctWinners.Select(w => w.DisplayName).ToList();
            var times = correctWinners.Select(w => w.TimeElapsed / 1000.0).ToList();
            
            _screenService?.ShowAllFFFContestants(names, times);
            GameConsole.Log($"[FFF] Displaying {correctWinners.Count} correct winners with times on TV");
        }
        else
        {
            GameConsole.Warn("[FFF] No correct winners to display");
        }
        
        _currentState = FFFFlowState.WinnersShown;
        UpdateUIState();
        
        GameConsole.Log("[FFF] Step 5 complete - Ready for Step 6");
    }
    
    private void btnWinner_Click(object? sender, EventArgs e)
    {
        if (_soundService == null || _rankings.Count == 0)
        {
            GameConsole.Error("[FFF] No rankings available - cannot announce winner");
            return;
        }
        
        GameConsole.Log("[FFF] Step 6: Confirm Winner started");
        
        var correctAnswers = _rankings.Where(r => r.IsCorrect).ToList();
        
        if (correctAnswers.Count == 0)
        {
            // NO WINNERS SCENARIO - All participants answered incorrectly
            GameConsole.Warn("[FFF] No correct answers - handling no-winner scenario");
            
            // Broadcast NoWinner to web clients
            if (_fffClient != null && _fffClient.IsConnected)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _fffClient.BroadcastPhaseMessageAsync("NoWinner", new { });
                        GameConsole.Log("[FFF] Broadcast NoWinner to web clients");
                    }
                    catch (Exception ex)
                    {
                        GameConsole.Error($"[FFF] Error broadcasting NoWinner: {ex.Message}");
                    }
                });
            }
            
            // Stop all sounds first
            _soundService.StopAllSounds(fadeout: false);
            GameConsole.Log("[FFF] All sounds stopped");
            
            // Display "No Winners" message
            lblWinner.Text = "Winner: ❌ No Winners";
            lblWinner.ForeColor = Color.Red;
            GameConsole.Log("[FFF] Displaying 'No Winners' message");
            // TODO [PRE-1.0]: Show "No Winners" on TV screen (FFF Online animations)
            // Part of Task #1 in PRE_1.0_FINAL_CHECKLIST.md
            // TODO: Show empty strap on TV screen with "No Winners" text (Phase 4 - Screen Animations)
            
            // Play Q01to05Wrong sound effect
            GameConsole.Log("[FFF] Playing Q01to05Wrong sound...");
            _soundService.PlaySound(SoundEffect.Q01to05Wrong, loop: false);
            
            // Update state to allow retry
            _currentState = FFFFlowState.QuestionReady;
            
            // Clear submissions and rankings to prepare for new question
            _submissions.Clear();
            _rankings.Clear();
            lstAnswers.Items.Clear();
            lstRankings.Items.Clear();
            lblAnswerCount.Text = "0 Answers";
            
            // Reset reveal click counter
            _revealCorrectClickCount = 0;
            btnRevealCorrect.Text = "4. Reveal Correct";
            
            GameConsole.Log("[FFF] No-winner scenario complete - ready to select new question");
            GameConsole.Info("[FFF] ⚠️ All participants failed - select a new question to retry");
            
            // Update UI to enable Show Questions button
            UpdateUIState();
            
            return;
        }
        
        // Get the winner (fastest correct answer)
        var winner = correctAnswers[0]; // Rankings are sorted: fastest correct answer is first
        
        // Display winner information
        if (correctAnswers.Count == 1)
        {
            lblWinner.Text = $"Winner: 🏆 {winner.DisplayName}";
            GameConsole.Log($"[FFF] Winner: {winner.DisplayName} (only correct answer)");
        }
        else
        {
            lblWinner.Text = $"Winner: 🏆 {winner.DisplayName} ({winner.TimeElapsed / 1000.0:F2}s)";
            GameConsole.Log($"[FFF] Winner: {winner.DisplayName} ({winner.TimeElapsed / 1000.0:F2}s) - fastest of {correctAnswers.Count} correct answers");
        }
        
        lblWinner.ForeColor = Color.Gold;
        
        // Display winner on TV
        // If we skipped Show Winners (single correct answer), go directly to Winner screen
        // If we came from Show Winners (multiple correct), highlight first then show Winner screen
        var correctCount = correctAnswers.Count;
        var winnerTimeSeconds = winner.TimeElapsed / 1000.0;
        
        if (correctCount == 1)
        {
            // Single winner - go directly to Winner screen
            GameConsole.Log($"[FFF] Single winner - displaying Winner screen immediately");
            
            // Clear question/answers from all screens
            // Update with blank question to clear text while keeping straps visible on Host/Guest
            var blankQuestion = new MillionaireGame.Core.Models.Question
            {
                QuestionText = "",
                AnswerA = "",
                AnswerB = "",
                AnswerC = "",
                AnswerD = "",
                CorrectAnswer = "A"
            };
            _screenService?.UpdateQuestion(blankQuestion);
            
            // Clear timer from all screens
            _screenService?.ShowPAFTimer(0, "Completed");
            
            // Clear from TV
            _screenService?.ShowQuestion(false);
            _screenService?.RemoveAnswer("A");
            _screenService?.RemoveAnswer("B");
            _screenService?.RemoveAnswer("C");
            _screenService?.RemoveAnswer("D");
            
            _screenService?.ShowFFFWinner(winner.DisplayName, winnerTimeSeconds);
        }
        else
        {
            // Multiple winners - highlight the fastest, wait 3 seconds, then show Winner screen
            Task.Run(async () =>
            {
                try
                {
                    // Find winner's index in the displayed list (alphabetically sorted)
                    var correctWinners = _rankings.Where(r => r.IsCorrect)
                                                  .OrderBy(r => r.DisplayName)
                                                  .Take(8)
                                                  .ToList();
                    var winnerIndex = correctWinners.FindIndex(r => r.DisplayName == winner.DisplayName);
                    
                    if (winnerIndex >= 0)
                    {
                        _screenService?.HighlightFFFContestant(winnerIndex, isWinner: true);
                        GameConsole.Log($"[FFF] Highlighted winner at index {winnerIndex}");
                        
                        // Wait 3 seconds
                        await Task.Delay(3000);
                        
                        // Show full winner screen with name and time
                        _screenService?.ShowFFFWinner(winner.DisplayName, winnerTimeSeconds);
                        GameConsole.Log($"[FFF] Displayed winner celebration: {winner.DisplayName} - {winnerTimeSeconds:F2}s");
                    }
                    else
                    {
                        // Fallback: couldn't find in list, show directly
                        GameConsole.Warn($"[FFF] Could not find winner in displayed list, showing directly");
                        _screenService?.ShowFFFWinner(winner.DisplayName, winnerTimeSeconds);
                    }
                }
                catch (Exception ex)
                {
                    GameConsole.Error($"[FFF] Error displaying winner on TV: {ex.Message}");
                }
            });
        }
        
        // Broadcast WinnerConfirmed with personalized outcomes to web clients
        Task.Run(async () =>
        {
            if (_fffClient != null && _fffClient.IsConnected)
            {
                var correctParticipantIds = correctAnswers.Select(r => r.ParticipantId).ToList();
                await _fffClient.BroadcastPhaseMessageAsync("WinnerConfirmed", new
                {
                    WinnerId = winner.ParticipantId,
                    CorrectParticipants = correctParticipantIds
                });
                GameConsole.Log($"[FFF] Broadcast WinnerConfirmed: Winner={winner.ParticipantId}, Correct={correctParticipantIds.Count}");
            }
            else
            {
                GameConsole.Warn("[FFF] Cannot broadcast WinnerConfirmed - client not connected");
            }
        });
        
        // Update UI immediately
        _currentState = FFFFlowState.WinnerAnnounced;
        UpdateUIState();
        
        // Play winner sounds sequentially
        try
        {
            // Stop FFFReadCorrectOrder background music first
            GameConsole.Log("[FFF] Stopping background music...");
            _soundService.StopSound(SoundEffect.FFFReadCorrectOrder.ToString());
            
            // Play FFFWinner (Immediate priority to stop any other sounds)
            GameConsole.Log("[FFF] Playing FFFWinner...");
            _soundService.QueueSound(SoundEffect.FFFWinner, AudioPriority.Immediate);
            
            // Queue FFFWalkDown to play after FFFWinner
            GameConsole.Log("[FFF] Queueing FFFWalkDown...");
            _soundService.QueueSound(SoundEffect.FFFWalkDown, AudioPriority.Normal);
            
            GameConsole.Log("[FFF] Step 6 complete - FFF Round finished");
        }
        catch (Exception ex)
        {
            GameConsole.Error($"[FFF] Error in winner announcement: {ex.Message}");
        }
        
        // TODO [POST-1.0]: Notify main control panel of winner
        // Related to hot seat integration (Post-1.0)
        // See: docs/active/PRE_1.0_FINAL_CHECKLIST.md - Deferred section
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
