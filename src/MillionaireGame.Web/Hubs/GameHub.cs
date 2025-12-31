using Microsoft.AspNetCore.SignalR;
using MillionaireGame.Web.Models;
using MillionaireGame.Web.Services;

namespace MillionaireGame.Web.Hubs;

/// <summary>
/// Unified SignalR hub for all game functionality (FFF, ATA, etc.)
/// </summary>
public class GameHub : Hub
{
    private readonly ILogger<GameHub> _logger;
    private readonly SessionService _sessionService;
    private readonly FFFService _fffService;
    private readonly NameValidationService _nameValidationService;
    private static readonly Dictionary<string, CancellationTokenSource> _fffTimers = new();
    private static readonly Dictionary<string, CancellationTokenSource> _ataTimers = new();
    private static readonly Dictionary<string, string> _ataQuestions = new();

    public GameHub(
        ILogger<GameHub> logger, 
        SessionService sessionService, 
        FFFService fffService, 
        NameValidationService nameValidationService)
    {
        _logger = logger;
        _sessionService = sessionService;
        _fffService = fffService;
        _nameValidationService = nameValidationService;
    }

    #region Session Management

    /// <summary>
    /// Join a game session with persistent participant ID
    /// </summary>
    public async Task<object> JoinSession(string sessionId, string displayName, string? participantId = null, DeviceTelemetry? telemetry = null)
    {
        // Validate name (only for new participants, not reconnections)
        if (string.IsNullOrEmpty(participantId))
        {
            var validationResult = _nameValidationService.ValidateName(displayName);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Name validation failed for {DisplayName}: {Error}", displayName, validationResult.ErrorMessage);
                return new
                {
                    Success = false,
                    Error = validationResult.ErrorMessage
                };
            }

            // Use sanitized name
            displayName = validationResult.SanitizedName!;

            // Check if a participant with this name exists (might be a refresh/reconnect)
            var existingParticipant = await _sessionService.FindParticipantByNameAsync(sessionId, displayName);
            if (existingParticipant != null)
            {
                // Found existing participant - treat as reconnection
                participantId = existingParticipant.Id;
                _logger.LogInformation("Treating page refresh as reconnection for {DisplayName} ({ParticipantId})", 
                    displayName, participantId);
            }
            else
            {
                // Check name uniqueness within session (only for truly new participants)
                var existingParticipants = await _sessionService.GetActiveParticipantsAsync(sessionId);
                var existingNames = existingParticipants.Select(p => p.DisplayName);
                
                if (!_nameValidationService.IsNameUnique(displayName, existingNames))
                {
                    _logger.LogWarning("Duplicate name rejected: {DisplayName} in session {SessionId}", displayName, sessionId);
                    return new
                    {
                        Success = false,
                        Error = "This name is already in use. Please choose a different name."
                    };
                }
            }
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
        
        // Check if this is a reconnection
        var participant = await _sessionService.GetOrCreateParticipantAsync(
            sessionId, displayName, Context.ConnectionId, participantId, telemetry);
        
        _logger.LogInformation("Participant {DisplayName} ({ParticipantId}) joined session {SessionId} (Reconnect: {IsReconnect})", 
            displayName, participant.Id, sessionId, participantId != null);
        
        // WebService console logging
        try 
        {
            var consoleType = Type.GetType("MillionaireGame.Utilities.WebServerConsole, MillionaireGame");
            if (consoleType != null)
            {
                var logMethod = consoleType.GetMethod("Log", new[] { typeof(string) });
                logMethod?.Invoke(null, new object[] { $"[GameHub] Player {displayName} ({participant.Id}) joined session {sessionId}" });
            }
        }
        catch { /* WebService console not available - ignore */ }
        
        // Notify all clients about the participant
        await Clients.All.SendAsync("ParticipantJoined", new
        {
            ParticipantId = participant.Id,
            DisplayName = displayName,
            SessionId = sessionId,
            IsReconnect = participantId != null,
            Timestamp = DateTime.UtcNow
        });
        
        // Return participant info
        return new
        {
            Success = true,
            ParticipantId = participant.Id,
            DisplayName = participant.DisplayName,
            SessionId = sessionId,
            State = participant.State.ToString(),
            CanVote = !participant.HasUsedATA && participant.IsActive
        };
    }

    /// <summary>
    /// Get list of active participants for a session
    /// </summary>
    public async Task<object> GetActiveParticipants(string sessionId)
    {
        var participants = await _sessionService.GetActiveParticipantsAsync(sessionId);
        
        _logger.LogInformation("GetActiveParticipants: {Count} participants in session {SessionId}", participants.Count(), sessionId);
        
        return participants.Select(p => new
        {
            Id = p.Id,
            DisplayName = p.DisplayName,
            JoinedAt = p.JoinedAt,
            DeviceType = p.DeviceType,
            State = p.State.ToString()
        }).ToList();
    }

    #endregion

    #region FFF (Fastest Finger First)

    /// <summary>
    /// Submit an answer to a FFF question
    /// </summary>
    public async Task<object> SubmitAnswer(string sessionId, string participantId, int questionId, string answerSequence)
    {
        var submittedAt = DateTime.UtcNow;
        
        // Check for duplicate submission
        if (await _sessionService.HasParticipantAnsweredAsync(sessionId, participantId, questionId))
        {
            _logger.LogWarning("Duplicate FFF answer blocked - Participant: {ParticipantId}, Question: {QuestionId}",
                participantId, questionId);
            
            return new { Success = false, Error = "You have already submitted an answer for this question" };
        }
        
        _logger.LogInformation("FFF Answer submitted - Participant: {ParticipantId}, Sequence: {Sequence}",
            participantId, answerSequence);
        
        // Save answer to database
        await _sessionService.SaveFFFAnswerAsync(sessionId, participantId, questionId, answerSequence, submittedAt);
        
        // Broadcast answer submission
        await Clients.All.SendAsync("AnswerSubmitted", new
        {
            ParticipantId = participantId,
            QuestionId = questionId,
            AnswerSequence = answerSequence,
            SubmittedAt = submittedAt
        });
        
        // Acknowledge to caller
        await Clients.Caller.SendAsync("AnswerReceived", new
        {
            Success = true,
            Timestamp = submittedAt,
            Sequence = answerSequence
        });
        
        return new { Success = true, Timestamp = submittedAt };
    }

    /// <summary>
    /// Host starts a FFF question
    /// </summary>
    public async Task StartQuestion(string sessionId, int questionId, string questionText, string[] options, int timeLimit = 20000)
    {
        _logger.LogInformation("Starting FFF question {QuestionId} in session {SessionId}", questionId, sessionId);
        
        // Cancel any existing timer
        var timerKey = $"{sessionId}_{questionId}";
        if (_fffTimers.ContainsKey(timerKey))
        {
            _fffTimers[timerKey].Cancel();
            _fffTimers.Remove(timerKey);
        }
        
        // Update session mode
        await _sessionService.UpdateSessionModeAsync(sessionId, SessionMode.FFF, questionId);
        
        // Broadcast question
        await Clients.Group(sessionId).SendAsync("QuestionStarted", new
        {
            QuestionId = questionId,
            Question = questionText,
            Options = options,
            TimeLimit = timeLimit,
            StartTime = DateTime.UtcNow
        });
        
        // Auto-end timer
        var cts = new CancellationTokenSource();
        _fffTimers[timerKey] = cts;
        
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(timeLimit, cts.Token);
                if (!cts.Token.IsCancellationRequested)
                {
                    await EndQuestion(sessionId, questionId);
                }
            }
            catch (TaskCanceledException) { }
            finally
            {
                _fffTimers.Remove(timerKey);
            }
        }, cts.Token);
    }

    /// <summary>
    /// Host ends a FFF question
    /// </summary>
    public async Task EndQuestion(string sessionId, int questionId)
    {
        _logger.LogInformation("Ending FFF question {QuestionId}", questionId);
        
        // Cancel timer
        var timerKey = $"{sessionId}_{questionId}";
        if (_fffTimers.ContainsKey(timerKey))
        {
            _fffTimers[timerKey].Cancel();
            _fffTimers.Remove(timerKey);
        }
        
        // Calculate rankings
        var results = await _fffService.CalculateRankingsAsync(sessionId, questionId);
        
        // Mark question as used
        await _fffService.MarkQuestionAsUsedAsync(questionId);
        
        // Clear session mode
        await _sessionService.UpdateSessionModeAsync(sessionId, SessionMode.Idle, null);
        
        // Broadcast results
        await Clients.Group(sessionId).SendAsync("QuestionEnded", new
        {
            QuestionId = questionId,
            EndTime = DateTime.UtcNow,
            Results = results
        });
    }

    /// <summary>
    /// Get FFF results for display
    /// </summary>
    public async Task<object> GetFFFResults(string sessionId, int questionId)
    {
        var results = await _fffService.CalculateRankingsAsync(sessionId, questionId);
        
        return new
        {
            Winner = results.Winner != null ? new
            {
                ParticipantId = results.Winner.ParticipantId,
                DisplayName = results.Winner.Participant?.DisplayName,
                TimeElapsed = results.Winner.TimeElapsed,
                Rank = results.Winner.Rank,
                IsCorrect = results.Winner.IsCorrect
            } : null,
            Rankings = results.Rankings.Select(r => new
            {
                ParticipantId = r.ParticipantId,
                DisplayName = r.Participant?.DisplayName,
                TimeElapsed = r.TimeElapsed,
                Rank = r.Rank,
                AnswerSequence = r.AnswerSequence,
                IsCorrect = r.IsCorrect
            }).ToList(),
            TotalSubmissions = results.TotalSubmissions,
            CorrectSubmissions = results.CorrectSubmissions
        };
    }

    #endregion

    #region ATA (Ask The Audience)

    /// <summary>
    /// Submit a vote for ATA
    /// </summary>
    public async Task<object> SubmitVote(string sessionId, string participantId, string selectedOption)
    {
        try
        {
            var submittedAt = DateTime.UtcNow;
            
            // Get participant
            var participants = await _sessionService.GetActiveParticipantsAsync(sessionId);
            var participant = participants.FirstOrDefault(p => p.Id == participantId);
            
            if (participant == null)
            {
                return new { Success = false, Error = "Participant not found" };
            }

            // Check if already used ATA this round
            if (participant.HasUsedATA)
            {
                return new { Success = false, Error = "You have already used Ask The Audience in this round" };
            }

            // Check for duplicate vote in current question
            if (await _sessionService.HasParticipantVotedAsync(sessionId, participantId))
            {
                return new { Success = false, Error = "You have already voted for this question" };
            }
            
            // Use a placeholder question text for vote tracking
            var questionText = $"ATA_Question_{DateTime.UtcNow:yyyyMMddHHmmss}";
            
            // Validate option
            if (!new[] { "A", "B", "C", "D" }.Contains(selectedOption.ToUpper()))
            {
                return new { Success = false, Error = "Invalid option selected" };
            }

            _logger.LogInformation("ATA Vote submitted - Participant: {ParticipantId}, Option: {Option}",
                participantId, selectedOption);
            
            // Save vote
            await _sessionService.SaveATAVoteAsync(sessionId, participantId, questionText, selectedOption.ToUpper(), submittedAt);
            
            // Calculate and broadcast updated percentages
            var percentages = await _sessionService.CalculateATAPercentagesAsync(sessionId);
            var totalVotes = await _sessionService.GetATAVoteCountAsync(sessionId);
            
            // WebServer console logging
            try 
            {
                var consoleType = Type.GetType("MillionaireGame.Utilities.WebServerConsole, MillionaireGame");
                if (consoleType != null)
                {
                    var logMethod = consoleType.GetMethod("Log", new[] { typeof(string) });
                    logMethod?.Invoke(null, new object[] { $"Player {participantId} voted in ATA ({totalVotes} votes)" });
                }
            }
            catch { }
            
            await Clients.Group(sessionId).SendAsync("VotesUpdated", new
            {
                Results = percentages,
                TotalVotes = totalVotes,
                Timestamp = submittedAt
            });
            
            return new { Success = true, Timestamp = submittedAt };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting ATA vote");
            return new { Success = false, Error = "An error occurred while submitting your vote" };
        }
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Broadcast a phase message to session
    /// </summary>
    public async Task BroadcastPhaseMessage(string sessionId, string messageType, object data)
    {
        _logger.LogInformation("Broadcasting {MessageType} to session {SessionId}", messageType, sessionId);
        await Clients.Group(sessionId).SendAsync(messageType, data);
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await _sessionService.MarkParticipantDisconnectedAsync(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    #endregion
}
