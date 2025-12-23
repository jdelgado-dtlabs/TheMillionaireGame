using Microsoft.AspNetCore.SignalR.Client;
using MillionaireGame.Web.Models;
using MillionaireGame.Forms;

namespace MillionaireGame.Services;

/// <summary>
/// SignalR client service for managing FFF (Fastest Finger First) communication with web server
/// </summary>
public class FFFClientService : IAsyncDisposable
{
    private HubConnection? _connection;
    private readonly string _serverUrl;
    private readonly string _sessionId;
    private bool _isConnected;
    
    // Events for UI updates
    public event EventHandler<ParticipantInfo>? ParticipantJoined;
    public event EventHandler<string>? ParticipantLeft;
    public event EventHandler<AnswerSubmission>? AnswerSubmitted;
    public event EventHandler<List<RankingResult>>? RankingsUpdated;
    public event EventHandler<string>? ConnectionStatusChanged;
    
    public bool IsConnected => _isConnected;
    
    public FFFClientService(string serverUrl, string sessionId)
    {
        _serverUrl = serverUrl.TrimEnd('/');
        _sessionId = sessionId;
    }
    
    /// <summary>
    /// Connect to the FFF hub on the web server
    /// </summary>
    public async Task ConnectAsync()
    {
        if (_connection != null)
        {
            await _connection.DisposeAsync();
        }
        
        _connection = new HubConnectionBuilder()
            .WithUrl($"{_serverUrl}/hubs/fff")
            .WithAutomaticReconnect()
            .Build();
        
        // Subscribe to hub events
        _connection.On<object>("ParticipantJoined", OnParticipantJoined);
        _connection.On<string>("ParticipantLeft", OnParticipantLeft);
        _connection.On<object>("AnswerSubmitted", OnAnswerSubmitted);
        
        _connection.Reconnecting += error =>
        {
            _isConnected = false;
            ConnectionStatusChanged?.Invoke(this, "Reconnecting...");
            return Task.CompletedTask;
        };
        
        _connection.Reconnected += connectionId =>
        {
            _isConnected = true;
            ConnectionStatusChanged?.Invoke(this, "Connected");
            return Task.CompletedTask;
        };
        
        _connection.Closed += error =>
        {
            _isConnected = false;
            ConnectionStatusChanged?.Invoke(this, "Disconnected");
            return Task.CompletedTask;
        };
        
        try
        {
            await _connection.StartAsync();
            _isConnected = true;
            ConnectionStatusChanged?.Invoke(this, "Connected");
        }
        catch (Exception ex)
        {
            _isConnected = false;
            ConnectionStatusChanged?.Invoke(this, $"Error: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// Start an FFF question
    /// </summary>
    public async Task StartQuestionAsync(int questionId, int timeLimit = 30)
    {
        if (_connection == null || !_isConnected)
            throw new InvalidOperationException("Not connected to server");
        
        await _connection.InvokeAsync("StartQuestion", _sessionId, questionId, timeLimit);
    }
    
    /// <summary>
    /// End the current FFF question
    /// </summary>
    public async Task EndQuestionAsync()
    {
        if (_connection == null || !_isConnected)
            throw new InvalidOperationException("Not connected to server");
        
        await _connection.InvokeAsync("EndQuestion", _sessionId);
    }
    
    /// <summary>
    /// Get current active participants
    /// </summary>
    public async Task<List<ParticipantInfo>> GetActiveParticipantsAsync()
    {
        if (_connection == null || !_isConnected)
            return new List<ParticipantInfo>();
        
        try
        {
            var result = await _connection.InvokeAsync<object>("GetActiveParticipants", _sessionId);
            // Convert dynamic result to typed list
            return ParseParticipants(result);
        }
        catch
        {
            return new List<ParticipantInfo>();
        }
    }
    
    /// <summary>
    /// Get answers for current question
    /// </summary>
    public async Task<List<AnswerSubmission>> GetAnswersAsync(int questionId)
    {
        if (_connection == null || !_isConnected)
            return new List<AnswerSubmission>();
        
        try
        {
            var result = await _connection.InvokeAsync<object>("GetAnswers", _sessionId, questionId);
            return ParseAnswers(result);
        }
        catch
        {
            return new List<AnswerSubmission>();
        }
    }
    
    /// <summary>
    /// Calculate rankings for current question
    /// </summary>
    public async Task<List<RankingResult>> CalculateRankingsAsync(int questionId)
    {
        if (_connection == null || !_isConnected)
            return new List<RankingResult>();
        
        try
        {
            var result = await _connection.InvokeAsync<object>("CalculateRankings", _sessionId, questionId);
            return ParseRankings(result);
        }
        catch
        {
            return new List<RankingResult>();
        }
    }
    
    private void OnParticipantJoined(object data)
    {
        try
        {
            var participant = ParseParticipant(data);
            ParticipantJoined?.Invoke(this, participant);
        }
        catch { /* Ignore parse errors */ }
    }
    
    private void OnParticipantLeft(string participantId)
    {
        ParticipantLeft?.Invoke(this, participantId);
    }
    
    private void OnAnswerSubmitted(object data)
    {
        try
        {
            var answer = ParseAnswer(data);
            AnswerSubmitted?.Invoke(this, answer);
        }
        catch { /* Ignore parse errors */ }
    }
    
    private ParticipantInfo ParseParticipant(object data)
    {
        // Simple parsing - in production, use proper JSON deserialization
        dynamic d = data;
        return new ParticipantInfo
        {
            Id = d.ParticipantId?.ToString() ?? d.Id?.ToString() ?? "",
            DisplayName = d.DisplayName?.ToString() ?? "Unknown",
            IsActive = d.IsActive ?? true
        };
    }
    
    private List<ParticipantInfo> ParseParticipants(object data)
    {
        var result = new List<ParticipantInfo>();
        if (data is System.Collections.IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                try
                {
                    result.Add(ParseParticipant(item));
                }
                catch { /* Skip invalid items */ }
            }
        }
        return result;
    }
    
    private AnswerSubmission ParseAnswer(object data)
    {
        dynamic d = data;
        return new AnswerSubmission
        {
            ParticipantId = d.ParticipantId?.ToString() ?? "",
            DisplayName = d.DisplayName?.ToString() ?? "Unknown",
            AnswerSequence = d.AnswerSequence?.ToString() ?? "",
            SubmittedAt = d.SubmittedAt ?? DateTime.UtcNow
        };
    }
    
    private List<AnswerSubmission> ParseAnswers(object data)
    {
        var result = new List<AnswerSubmission>();
        if (data is System.Collections.IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                try
                {
                    result.Add(ParseAnswer(item));
                }
                catch { /* Skip invalid items */ }
            }
        }
        return result;
    }
    
    private RankingResult ParseRanking(object data)
    {
        dynamic d = data;
        return new RankingResult
        {
            Rank = d.Rank ?? 0,
            ParticipantId = d.ParticipantId?.ToString() ?? "",
            DisplayName = d.DisplayName?.ToString() ?? "Unknown",
            AnswerSequence = d.AnswerSequence?.ToString() ?? "",
            TimeElapsed = d.TimeElapsed ?? 0.0,
            IsCorrect = d.IsCorrect ?? false
        };
    }
    
    private List<RankingResult> ParseRankings(object data)
    {
        var result = new List<RankingResult>();
        if (data is System.Collections.IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                try
                {
                    result.Add(ParseRanking(item));
                }
                catch { /* Skip invalid items */ }
            }
        }
        return result;
    }
    
    public async ValueTask DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }
        _isConnected = false;
    }
}
