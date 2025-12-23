using MillionaireGame.Web.Models;
using MillionaireGame.Web.Database;
using MillionaireGame.Services;
using MillionaireGame.Core.Database;
using MillionaireGame.Core.Settings;
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
    private readonly string _sessionId = "game-session";
    
    public FFFControlPanel()
    {
        InitializeComponent();
        
        _fffTimer = new System.Timers.Timer(100); // Update every 100ms
        _fffTimer.Elapsed += FFFTimer_Elapsed;
        
        // Initialize repository
        try
        {
            var sqlSettings = new SqlSettingsManager();
            var connectionString = sqlSettings.Settings.GetConnectionString("waps.db");
            _fffRepository = new MillionaireGame.Web.Database.FFFQuestionRepository(connectionString);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing FFF repository: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Load available FFF questions
    /// </summary>
    public async Task LoadQuestionsAsync()
    {
        try
        {
            cmbQuestions.Items.Clear();
            cmbQuestions.Items.Add("-- Select a question --");
            
            if (_fffRepository != null)
            {
                _questions = await _fffRepository.GetAllQuestionsAsync();
                
                foreach (var question in _questions)
                {
                    var displayText = $"Q{question.Id}: {question.QuestionText.Substring(0, Math.Min(60, question.QuestionText.Length))}...";
                    cmbQuestions.Items.Add(new ComboBoxItem { Text = displayText, Value = question });
                }
            }
            
            cmbQuestions.SelectedIndex = 0;
            UpdateUIState();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading FFF questions: {ex.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    /// <summary>
    /// Initialize SignalR client connection
    /// </summary>
    public async Task InitializeClientAsync(string serverUrl)
    {
        try
        {
            _fffClient = new FFFClientService(serverUrl, _sessionId);
            
            // Subscribe to events
            _fffClient.ParticipantJoined += (s, p) => UpdateParticipants(new List<ParticipantInfo> { p });
            _fffClient.AnswerSubmitted += (s, a) => AddAnswerSubmission(a);
            _fffClient.ConnectionStatusChanged += (s, status) =>
            {
                if (InvokeRequired)
                    Invoke(() => Text = $"FFF Control - {status}");
                else
                    Text = $"FFF Control - {status}";
            };
            
            await _fffClient.ConnectAsync();
            
            // Load initial participants
            var participants = await _fffClient.GetActiveParticipantsAsync();
            UpdateParticipants(participants);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error connecting to server: {ex.Message}",
                "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    /// <summary>
    /// Update participant list
    /// </summary>
    public void UpdateParticipants(List<ParticipantInfo> participants)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => UpdateParticipants(participants)));
            return;
        }
        
        _participants = participants;
        lstParticipants.Items.Clear();
        
        foreach (var participant in participants)
        {
            lstParticipants.Items.Add($"{participant.DisplayName} ({participant.Id.Substring(0, 8)}...)");
        }
        
        lblParticipantCount.Text = $"{participants.Count} Participant{(participants.Count != 1 ? "s" : "")}";
        UpdateUIState();
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
        
        _rankings = rankings;
        lstRankings.Items.Clear();
        
        foreach (var ranking in rankings)
        {
            var icon = ranking.IsCorrect ? "✓" : "✗";
            var timeSeconds = ranking.TimeElapsed / 1000.0;
            lstRankings.Items.Add($"#{ranking.Rank} {icon} {ranking.DisplayName} ({timeSeconds:F2}s)");
        }
        
        if (rankings.Count > 0 && rankings[0].IsCorrect)
        {
            lblWinner.Text = $"Winner: {rankings[0].DisplayName}";
            lblWinner.ForeColor = Color.Green;
        }
        else
        {
            lblWinner.Text = "Winner: None (no correct answers)";
            lblWinner.ForeColor = Color.Red;
        }
        
        UpdateUIState();
    }
    
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
    
    private void ClearQuestionDetails()
    {
        txtOption1.Clear();
        txtOption2.Clear();
        txtOption3.Clear();
        txtOption4.Clear();
        lblCorrectOrder.Text = "Correct Order: ---";
    }
    
    private async void btnStartFFF_Click(object? sender, EventArgs e)
    {
        if (_currentQuestion == null)
        {
            MessageBox.Show("Please select a question first.", "No Question Selected",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        
        if (_participants.Count == 0)
        {
            var result = MessageBox.Show(
                "No participants are currently connected. Start FFF anyway?",
                "No Participants",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
                
            if (result == DialogResult.No)
                return;
        }
        
        try
        {
            if (_fffClient == null || !_fffClient.IsConnected)
            {
                MessageBox.Show("Not connected to web server. Start the web server from Settings first.",
                    "Not Connected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            await _fffClient.StartQuestionAsync(_currentQuestion.Id, timeLimit: 30);
            
            _fffStartTime = DateTime.UtcNow;
            _isFFFActive = true;
            _submissions.Clear();
            lstAnswers.Items.Clear();
            lblAnswerCount.Text = "0 Answers";
            
            _fffTimer.Start();
            UpdateUIState();
            
            MessageBox.Show("FFF question started! Participants can now submit answers.",
                "FFF Started", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error starting FFF: {ex.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private async void btnEndFFF_Click(object? sender, EventArgs e)
    {
        try
        {
            _fffTimer.Stop();
            _isFFFActive = false;
            
            if (_fffClient != null && _fffClient.IsConnected)
            {
                await _fffClient.EndQuestionAsync();
            }
            
            UpdateUIState();
            
            MessageBox.Show("FFF question ended. Calculate results to see rankings.",
                "FFF Ended", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error ending FFF: {ex.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private async void btnCalculateResults_Click(object? sender, EventArgs e)
    {
        if (_currentQuestion == null)
        {
            MessageBox.Show("No question is currently loaded.", "No Question",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        
        try
        {
            if (_fffClient != null && _fffClient.IsConnected)
            {
                var rankings = await _fffClient.CalculateRankingsAsync(_currentQuestion.Id);
                UpdateRankings(rankings);
                
                MessageBox.Show("Results calculated! Check the Rankings panel.",
                    "Results Ready", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Not connected to web server.", "Not Connected",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error calculating results: {ex.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private void btnSelectWinner_Click(object? sender, EventArgs e)
    {
        if (_rankings.Count == 0 || !_rankings[0].IsCorrect)
        {
            MessageBox.Show("No winner available. Calculate results first or ensure there are correct answers.",
                "No Winner", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        
        var winner = _rankings[0];
        var result = MessageBox.Show(
            $"Confirm {winner.DisplayName} as the winner?\n\n" +
            $"Time: {winner.TimeElapsed / 1000.0:F2} seconds\n" +
            $"Answer: {winner.AnswerSequence}",
            "Confirm Winner",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);
            
        if (result == DialogResult.Yes)
        {
            // TODO: Notify game service that this player won FFF and should proceed to hot seat
            MessageBox.Show($"{winner.DisplayName} selected as winner!",
                "Winner Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            // Reset for next round
            ResetFFFRound();
        }
    }
    
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
        
        btnStartFFF.Enabled = hasQuestion && !_isFFFActive;
        btnEndFFF.Enabled = _isFFFActive;
        btnCalculateResults.Enabled = !_isFFFActive && hasAnswers;
        btnSelectWinner.Enabled = hasRankings && _rankings[0].IsCorrect;
        
        cmbQuestions.Enabled = !_isFFFActive;
    }
    
    private void ResetFFFRound()
    {
        _currentQuestion = null;
        _submissions.Clear();
        _rankings.Clear();
        
        cmbQuestions.SelectedIndex = 0;
        lstAnswers.Items.Clear();
        lstRankings.Items.Clear();
        lblAnswerCount.Text = "0 Answers";
        lblWinner.Text = "Winner: ---";
        lblTimer.Text = "00:00";
        
        ClearQuestionDetails();
        UpdateUIState();
    }
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
