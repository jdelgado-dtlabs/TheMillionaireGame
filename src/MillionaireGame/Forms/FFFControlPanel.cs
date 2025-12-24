using MillionaireGame.Web.Models;
using MillionaireGame.Web.Database;
using MillionaireGame.Services;
using MillionaireGame.Core.Database;
using MillionaireGame.Core.Settings;
using MillionaireGame.Utilities;
using System.Timers;

namespace MillionaireGame.Forms;

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
        var display = $"{(int)elapsed.TotalMinutes:D2}:{elapsed.Seconds:D2}";
        
        if (InvokeRequired)
        {
            Invoke(new Action(() => lblTimer.Text = display));
        }
        else
        {
            lblTimer.Text = display;
        }
    }
    
    private void UpdateUIState()
    {
        var hasQuestion = _currentQuestion != null;
        var hasParticipants = _participants.Count > 0;
        var hasAnswers = _submissions.Count > 0;
        var hasRankings = _rankings.Count > 0;
        
        // TODO: Phase 3 - Implement button state management based on FFFFlowState
        // btnIntroExplain.Enabled = hasQuestion && !_isFFFActive;
        // btnShowQuestion.Enabled = _isFFFActive;
        // etc...
    }
    
    private void ResetFFFRound()
    {
        _currentQuestion = null;
        _submissions.Clear();
        _rankings.Clear();
        
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
        // TODO: Phase 3 - Play FFFLightsDown, then FFFExplain
        MessageBox.Show("Intro + Explain - Coming in Phase 3", "UI Preview", MessageBoxButtons.OK, MessageBoxIcon.None);
    }
    
    private void btnShowQuestion_Click(object? sender, EventArgs e)
    {
        // Randomly select a question from loaded questions
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
        
        MessageBox.Show($"Question {_currentQuestion.Id} selected and displayed.\n\nNext: Click 'Reveal Answers & Start' to randomize and show answers.",
            "Question Loaded", MessageBoxButtons.OK, MessageBoxIcon.None);
    }
    
    private void btnRevealAnswers_Click(object? sender, EventArgs e)
    {
        // TODO: Phase 3 - Randomize answers, start timer, play FFFThinking
        MessageBox.Show("Reveal Answers - Coming in Phase 3", "UI Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    
    private void btnRevealCorrect_Click(object? sender, EventArgs e)
    {
        // TODO: Phase 3 - Reveal correct answers one by one (4 clicks)
        MessageBox.Show("Reveal Correct - Coming in Phase 3", "UI Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    
    private void btnShowWinners_Click(object? sender, EventArgs e)
    {
        // TODO: Phase 3 - Display list of winners
        MessageBox.Show("Show Winners - Coming in Phase 3", "UI Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    
    private void btnWinner_Click(object? sender, EventArgs e)
    {
        // TODO: Phase 3 - Declare winner (auto if 1, times if multiple)
        MessageBox.Show("Winner - Coming in Phase 3", "UI Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    
    private void btnStopAudio_Click(object? sender, EventArgs e)
    {
        // TODO: Phase 3 - Stop all audio playback
        MessageBox.Show("Stop Audio - Coming in Phase 3", "UI Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
