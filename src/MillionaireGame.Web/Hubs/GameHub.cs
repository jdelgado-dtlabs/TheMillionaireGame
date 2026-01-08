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
        
        // Sync current game state to reconnecting/joining client
        await SyncClientStateAsync(sessionId, participant.Id);
        
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
        
        // Get session for timer validation
        var session = await _sessionService.GetSessionAsync(sessionId);
        if (session == null)
        {
            return new { Success = false, Error = "Session not found" };
        }
        
        // Validate timer expiry (20 second limit + 500ms grace period for network latency)
        if (session.QuestionStartTime.HasValue)
        {
            var elapsed = (submittedAt - session.QuestionStartTime.Value).TotalMilliseconds;
            if (elapsed > 20500) // 20s + 500ms grace
            {
                _logger.LogWarning("Late FFF answer rejected - Participant: {ParticipantId}, Question: {QuestionId}, Elapsed: {Elapsed}ms",
                    participantId, questionId, elapsed);
                return new { Success = false, Error = "Cannot submit - time expired" };
            }
        }
        
        // Get participant for eligibility validation
        var participant = await _sessionService.GetParticipantAsync(participantId);
        if (participant == null)
        {
            return new { Success = false, Error = "Participant not found" };
        }
        
        // Validate participant joined before question started (anti-cheat)
        if (session.QuestionStartTime.HasValue && participant.LastSeenAt > session.QuestionStartTime.Value)
        {
            _logger.LogWarning("Late joiner FFF answer rejected - Participant: {ParticipantId}, Question: {QuestionId}",
                participantId, questionId);
            return new { Success = false, Error = "Cannot submit - joined after question started" };
        }
        
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
        
        // Update session mode WITH question details for state sync
        await _sessionService.UpdateSessionModeAsync(sessionId, SessionMode.FFF, questionId, questionText, options);
        
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
            
            // Get session for timer validation
            var session = await _sessionService.GetSessionAsync(sessionId);
            if (session == null)
            {
                return new { Success = false, Error = "Session not found" };
            }
            
            // Validate timer expiry (60 second limit + 500ms grace period for network latency)
            if (session.QuestionStartTime.HasValue)
            {
                var elapsed = (submittedAt - session.QuestionStartTime.Value).TotalMilliseconds;
                if (elapsed > 60500) // 60s + 500ms grace
                {
                    _logger.LogWarning("Late ATA vote rejected - Participant: {ParticipantId}, Elapsed: {Elapsed}ms",
                        participantId, elapsed);
                    return new { Success = false, Error = "Cannot vote - time expired" };
                }
            }
            
            // Get participant
            var participant = await _sessionService.GetParticipantAsync(participantId);
            
            if (participant == null)
            {
                return new { Success = false, Error = "Participant not found" };
            }
            
            // Validate participant joined before voting started (anti-cheat)
            if (session.QuestionStartTime.HasValue && participant.LastSeenAt > session.QuestionStartTime.Value)
            {
                _logger.LogWarning("Late joiner ATA vote rejected - Participant: {ParticipantId}",
                    participantId);
                return new { Success = false, Error = "Cannot vote - joined after voting started" };
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

    #region Game State Management

    /// <summary>
    /// Broadcast game state change to all clients in session
    /// </summary>
    public async Task BroadcastGameState(string sessionId, GameStateType state, string? message = null, object? data = null)
    {
        _sessionService.SetCurrentState(state);
        
        var stateData = new GameStateData
        {
            State = state,
            Message = message,
            Data = data,
            Timestamp = DateTime.UtcNow
        };
        
        _logger.LogInformation("Broadcasting game state {State} to session {SessionId}", state, sessionId);
        await Clients.Group(sessionId).SendAsync("GameStateChanged", stateData);
    }

    /// <summary>
    /// Get current game state (for new joiners)
    /// </summary>
    public GameStateType GetCurrentGameState()
    {
        return _sessionService.GetCurrentState();
    }

    /// <summary>
    /// Sync current game state to a specific client (typically after reconnect)
    /// </summary>
    private async Task SyncClientStateAsync(string sessionId, string participantId)
    {
        var session = await _sessionService.GetSessionAsync(sessionId);
        if (session?.CurrentMode == null || session.CurrentMode == SessionMode.Idle)
            return;
        
        var participant = await _sessionService.GetParticipantAsync(participantId);
        if (participant == null)
            return;
        
        if (session.CurrentMode == SessionMode.FFF)
        {
            var fffState = await _sessionService.GetCurrentFFFStateAsync(sessionId);
            if (fffState != null)
            {
                // Calculate remaining time
                var elapsed = (DateTime.UtcNow - fffState.StartTime).TotalMilliseconds;
                var remaining = Math.Max(0, fffState.TimeLimit - elapsed);
                var isStillActive = remaining > 0;
                
                // Determine if participant can submit answer
                // Only if they were connected BEFORE question started OR question already ended
                var canSubmit = participant.LastSeenAt <= fffState.StartTime || !isStillActive;
                
                // Check if already answered
                if (await _sessionService.HasParticipantAnsweredAsync(sessionId, participantId, fffState.QuestionId))
                {
                    canSubmit = false;
                }
                
                await Clients.Caller.SendAsync("QuestionStarted", new
                {
                    QuestionId = fffState.QuestionId,
                    Question = fffState.QuestionText,
                    Options = fffState.Options,
                    TimeLimit = (int)remaining, // Send remaining time, not full time
                    StartTime = fffState.StartTime,
                    IsResync = true, // Flag to indicate this is a state sync
                    CanSubmit = canSubmit, // Can this participant submit?
                    SpectatorMode = !canSubmit && isStillActive, // Viewing only
                    SpectatorReason = !canSubmit && isStillActive ? "You joined after the question started" : null
                });
                
                _logger.LogInformation("Synced FFF state to reconnected client for question {QuestionId} (CanSubmit: {CanSubmit})", 
                    fffState.QuestionId, canSubmit);
            }
        }
        else if (session.CurrentMode == SessionMode.ATA)
        {
            var ataState = await _sessionService.GetCurrentATAStateAsync(sessionId);
            if (ataState != null)
            {
                // Calculate if voting is still active (60 second window)
                var elapsed = (DateTime.UtcNow - ataState.StartTime).TotalSeconds;
                var isStillActive = elapsed < 60 && ataState.IsActive;
                
                // Determine if participant can vote
                // Only if they were connected BEFORE voting started OR voting already ended
                var canVote = participant.LastSeenAt <= ataState.StartTime || !isStillActive;
                
                // Check if already voted
                if (await _sessionService.HasParticipantVotedAsync(sessionId, participantId))
                {
                    canVote = false;
                }
                
                // Check if used ATA lifeline
                if (participant.HasUsedATA)
                {
                    canVote = false;
                }
                
                // Send both the start event and current results
                await Clients.Caller.SendAsync("ATAStarted", new
                {
                    QuestionId = ataState.QuestionId,
                    QuestionText = ataState.QuestionText,
                    StartTime = ataState.StartTime,
                    IsResync = true,
                    CanVote = canVote, // Can this participant vote?
                    SpectatorMode = !canVote && isStillActive, // Viewing only
                    SpectatorReason = !canVote && isStillActive ? "You joined after voting started" : null
                });
                
                await Clients.Caller.SendAsync("VotesUpdated", new
                {
                    Results = ataState.CurrentResults,
                    TotalVotes = ataState.TotalVotes,
                    Timestamp = DateTime.UtcNow
                });
                
                _logger.LogInformation("Synced ATA state to reconnected client for question {QuestionId} (CanVote: {CanVote})", 
                    ataState.QuestionId, canVote);
            }
        }
    }

    /// <summary>
    /// Request explicit state sync (called by client after reconnection)
    /// </summary>
    public async Task RequestStateSync(string sessionId, string participantId)
    {
        _logger.LogInformation("Client {ConnectionId} requested state sync for session {SessionId}", 
            Context.ConnectionId, sessionId);
        await SyncClientStateAsync(sessionId, participantId);
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
