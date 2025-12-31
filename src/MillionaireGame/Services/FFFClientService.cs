using Microsoft.AspNetCore.SignalR.Client;
using MillionaireGame.Utilities;
using MillionaireGame.Web.Models;
using MillionaireGame.Forms;
using System.Text.Json;

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
    private List<ParticipantInfo> _participants = new();
    
    // Events for UI updates
    public event EventHandler<ParticipantInfo>? ParticipantJoined;
    public event EventHandler<string>? ParticipantLeft;
    public event EventHandler<AnswerSubmission>? AnswerSubmitted;
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
            .WithUrl($"{_serverUrl}/hubs/game")
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
    /// <summary>
    /// Start an FFF question
    /// </summary>
    public async Task StartQuestionAsync(int questionId, string questionText, string[] options, int timeLimit = 30)
    {
        if (_connection == null || !_isConnected)
            throw new InvalidOperationException("Not connected to server");
        
        GameConsole.Info($"[FFFClient] Starting question {questionId} with time limit {timeLimit}s");
        await _connection.InvokeAsync("StartQuestion", _sessionId, questionId, questionText, options, timeLimit * 1000); // Convert seconds to milliseconds
    }
    
    /// <summary>
    /// End the current FFF question
    /// </summary>
    public async Task EndQuestionAsync(int questionId)
    {
        if (_connection == null || !_isConnected)
            throw new InvalidOperationException("Not connected to server");
        
        GameConsole.Info($"[FFFClient] Ending question {questionId}");
        await _connection.InvokeAsync("EndQuestion", _sessionId, questionId);
    }
    
    /// <summary>
    /// Broadcast a phase message to all participants in the session
    /// </summary>
    public async Task BroadcastPhaseMessageAsync(string messageType, object data)
    {
        if (_connection == null || !_isConnected)
        {
            GameConsole.Warn($"[FFFClient] Cannot broadcast {messageType} - not connected");
            return;
        }
        
        try
        {
            GameConsole.Info($"[FFFClient] Broadcasting {messageType} to session {_sessionId}");
            await _connection.InvokeAsync("BroadcastPhaseMessage", _sessionId, messageType, data);
        }
        catch (Exception ex)
        {
            GameConsole.Error($"[FFFClient] Error broadcasting {messageType}: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Get current active participants
    /// </summary>
    public async Task<List<ParticipantInfo>> GetActiveParticipantsAsync()
    {
        if (_connection == null || !_isConnected)
        {
            GameConsole.Warn("[FFFClient] Not connected, returning empty list");
            return new List<ParticipantInfo>();
        }
        
        try
        {
            GameConsole.Log($"[FFFClient] Calling GetActiveParticipants for session {_sessionId}");
            var result = await _connection.InvokeAsync<object>("GetActiveParticipants", _sessionId);
            GameConsole.Log($"[FFFClient] Received result: {result?.GetType().Name ?? "null"}");
            
            // Convert dynamic result to typed list
            var participants = ParseParticipants(result!);
            GameConsole.Log($"[FFFClient] Parsed {participants.Count} participants");
            
            // Cache participants for lookup when parsing answers
            _participants = participants;
            
            return participants;
        }
        catch (Exception ex)
        {
            GameConsole.Error($"[FFFClient] Error: {ex.Message}");
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
            GameConsole.Debug($"[FFFClient] Calling CalculateRankings for question {questionId}");
            var result = await _connection.InvokeAsync<object>("CalculateRankings", _sessionId, questionId);
            GameConsole.Debug($"[FFFClient] Received rankings result type: {result?.GetType().Name ?? "null"}");
            
            // Server returns: { Success, QuestionId, Winner, Rankings[], TotalSubmissions, CorrectSubmissions }
            // Extract the Rankings array
            if (result is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object)
            {
                if (jsonElement.TryGetProperty("Rankings", out JsonElement rankingsArray) ||
                    jsonElement.TryGetProperty("rankings", out rankingsArray))
                {
                    GameConsole.Debug($"[FFFClient] Extracted Rankings property");
                    var rankings = ParseRankings(rankingsArray);
                    GameConsole.Debug($"[FFFClient] Parsed {rankings.Count} rankings");
                    foreach (var r in rankings)
                    {
                        GameConsole.Debug($"[FFFClient]   Rank {r.Rank}: {r.DisplayName}, IsCorrect={r.IsCorrect}, TimeElapsed={r.TimeElapsed}");
                    }
                    return rankings;
                }
            }
            
            // Fallback to parsing entire result
            var allRankings = ParseRankings(result!);
            GameConsole.Debug($"[FFFClient] Parsed {allRankings.Count} rankings (fallback)");
            return allRankings;
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
            
            // Add to cache
            var existing = _participants.FirstOrDefault(p => p.Id == participant.Id);
            if (existing == null)
            {
                _participants.Add(participant);
                GameConsole.Debug($"[FFFClient] Added participant to cache: {participant.DisplayName}");
            }
            
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
            GameConsole.Debug($"[FFFClient] OnAnswerSubmitted called with data type: {data?.GetType().Name}");
            var answer = ParseAnswer(data!);
            GameConsole.Debug($"[FFFClient] Parsed answer: ParticipantId={answer.ParticipantId}, DisplayName={answer.DisplayName}, Sequence={answer.AnswerSequence}");
            AnswerSubmitted?.Invoke(this, answer);
            GameConsole.Debug($"[FFFClient] AnswerSubmitted event raised");
        }
        catch (Exception ex)
        {
            GameConsole.Error($"[FFFClient] ERROR parsing answer submission: {ex.Message}");
        }
    }
    
    private ParticipantInfo ParseParticipant(object data)
    {
        // Handle JsonElement
        if (data is JsonElement jsonElement)
        {
            GameConsole.Log($"[FFFClient] ParseParticipant JsonElement properties:");
            string? id = null;
            string? displayName = null;
            
            foreach (var prop in jsonElement.EnumerateObject())
            {
                GameConsole.Log($"[FFFClient]   {prop.Name} = {prop.Value}");
                
                // Manual case-insensitive matching
                if (prop.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
                {
                    id = prop.Value.GetString();
                    GameConsole.Debug($"[FFFClient] Found Id via enumeration: '{id}'");
                }
                else if (prop.Name.Equals("displayName", StringComparison.OrdinalIgnoreCase))
                {
                    displayName = prop.Value.GetString();
                    GameConsole.Debug($"[FFFClient] Found displayName via enumeration: '{displayName}'");
                }
            }
            
            id ??= "";
            displayName ??= "Unknown";
            var isActive = true;
            
            GameConsole.Debug($"[FFFClient] Final parsed: Id='{id}', DisplayName='{displayName}', IsActive={isActive}");
            
            return new ParticipantInfo
            {
                Id = id,
                DisplayName = displayName,
                IsActive = isActive
            };
        }
        
        // Fallback to dynamic for other types
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
        GameConsole.Debug($"[FFFClient] ParseParticipants: data type = {data?.GetType().Name ?? "null"}");
        
        // Handle JsonElement arrays
        if (data is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
        {
            GameConsole.Log("[FFFClient] Data is JsonElement array, enumerating...");
            int index = 0;
            foreach (var item in jsonElement.EnumerateArray())
            {
                try
                {
                    GameConsole.Log($"[FFFClient] Parsing JsonElement item {index}");
                    var participant = ParseParticipant(item);
                    GameConsole.Log($"[FFFClient] Parsed: Id={participant.Id}, Name={participant.DisplayName}");
                    result.Add(participant);
                    index++;
                }
                catch (Exception ex)
                {
                    GameConsole.Log($"[FFFClient] Error parsing item {index}: {ex.Message}");
                }
            }
        }
        else if (data is System.Collections.IEnumerable enumerable && data is not string)
        {
            GameConsole.Log("[FFFClient] Data is IEnumerable, iterating...");
            int index = 0;
            foreach (var item in enumerable)
            {
                try
                {
                    GameConsole.Log($"[FFFClient] Parsing item {index}: {item?.GetType().Name ?? "null"}");
                    var participant = ParseParticipant(item!);
                    GameConsole.Log($"[FFFClient] Parsed: Id={participant.Id}, Name={participant.DisplayName}");
                    result.Add(participant);
                    index++;
                }
                catch (Exception ex)
                {
                    GameConsole.Log($"[FFFClient] Error parsing item {index}: {ex.Message}");
                }
            }
        }
        else
        {
            GameConsole.Warn($"[FFFClient] Data type not recognized as enumerable: {data?.GetType().FullName ?? "null"}");
        }
        
        GameConsole.Debug($"[FFFClient] Total parsed: {result.Count} participants");
        return result;
    }
    
    private AnswerSubmission ParseAnswer(object data)
    {
        // Handle JsonElement from SignalR
        if (data is JsonElement jsonElement)
        {
            string? participantId = null;
            string? answerSequence = null;
            DateTime? submittedAt = null;
            
            foreach (var prop in jsonElement.EnumerateObject())
            {
                if (prop.Name.Equals("ParticipantId", StringComparison.OrdinalIgnoreCase))
                {
                    participantId = prop.Value.GetString();
                }
                else if (prop.Name.Equals("AnswerSequence", StringComparison.OrdinalIgnoreCase))
                {
                    answerSequence = prop.Value.GetString();
                }
                else if (prop.Name.Equals("SubmittedAt", StringComparison.OrdinalIgnoreCase))
                {
                    submittedAt = prop.Value.GetDateTime();
                }
            }
            
            // Look up DisplayName from _participants list
            var participant = _participants.FirstOrDefault(p => p.Id == participantId);
            var displayName = participant?.DisplayName ?? "Unknown";
            
            return new AnswerSubmission
            {
                ParticipantId = participantId ?? "",
                DisplayName = displayName,
                AnswerSequence = answerSequence ?? "",
                SubmittedAt = submittedAt ?? DateTime.UtcNow
            };
        }
        
        // Fallback to dynamic for other types
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
        // Handle JsonElement from SignalR
        if (data is JsonElement jsonElement)
        {
            GameConsole.Debug($"[FFFClient] ParseRanking: JsonElement with ValueKind={jsonElement.ValueKind}");
            
            int? rank = null;
            string? participantId = null;
            string? displayName = null;
            string? answerSequence = null;
            double? timeElapsed = null;
            bool? isCorrect = null;
            
            foreach (var prop in jsonElement.EnumerateObject())
            {
                GameConsole.Debug($"[FFFClient]   Property: {prop.Name} = {prop.Value}");
                
                if (prop.Name.Equals("Rank", StringComparison.OrdinalIgnoreCase))
                {
                    rank = prop.Value.GetInt32();
                }
                else if (prop.Name.Equals("ParticipantId", StringComparison.OrdinalIgnoreCase))
                {
                    participantId = prop.Value.GetString();
                }
                else if (prop.Name.Equals("DisplayName", StringComparison.OrdinalIgnoreCase))
                {
                    displayName = prop.Value.GetString();
                }
                else if (prop.Name.Equals("AnswerSequence", StringComparison.OrdinalIgnoreCase))
                {
                    answerSequence = prop.Value.GetString();
                }
                else if (prop.Name.Equals("TimeElapsed", StringComparison.OrdinalIgnoreCase))
                {
                    timeElapsed = prop.Value.GetDouble();
                }
                else if (prop.Name.Equals("IsCorrect", StringComparison.OrdinalIgnoreCase))
                {
                    isCorrect = prop.Value.GetBoolean();
                }
            }
            
            GameConsole.Debug($"[FFFClient] ParseRanking result: Rank={rank}, DisplayName={displayName}, IsCorrect={isCorrect}, TimeElapsed={timeElapsed}");
            
            return new RankingResult
            {
                Rank = rank ?? 0,
                ParticipantId = participantId ?? "",
                DisplayName = displayName ?? "Unknown",
                AnswerSequence = answerSequence ?? "",
                TimeElapsed = timeElapsed ?? 0.0,
                IsCorrect = isCorrect ?? false
            };
        }
        
        // Fallback to dynamic for other types
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
        
        // Handle JsonElement array from SignalR
        if (data is JsonElement jsonElement)
        {
            GameConsole.Debug($"[FFFClient] ParseRankings: JsonElement ValueKind={jsonElement.ValueKind}");
            
            if (jsonElement.ValueKind == JsonValueKind.Array)
            {
                GameConsole.Debug($"[FFFClient] ParseRankings: JsonElement array with {jsonElement.GetArrayLength()} items");
                foreach (var item in jsonElement.EnumerateArray())
                {
                    try
                    {
                        result.Add(ParseRanking(item));
                    }
                    catch (Exception ex)
                    {
                        GameConsole.Error($"[FFFClient] Error parsing ranking item: {ex.Message}");
                    }
                }
            }
            else
            {
                GameConsole.Debug($"[FFFClient] ParseRankings: Not an array, trying to parse as single item");
                try
                {
                    result.Add(ParseRanking(jsonElement));
                }
                catch (Exception ex)
                {
                    GameConsole.Error($"[FFFClient] Error parsing single ranking: {ex.Message}");
                }
            }
        }
        else if (data is System.Collections.IEnumerable enumerable)
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
