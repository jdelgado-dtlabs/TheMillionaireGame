using Microsoft.AspNetCore.SignalR;
using MillionaireGame.Web.Models;
using MillionaireGame.Web.Services;

namespace MillionaireGame.Web.Hubs;

/// <summary>
/// SignalR hub for Fastest Finger First (FFF) functionality
/// </summary>
public class FFFHub : Hub
{
    private readonly ILogger<FFFHub> _logger;
    private readonly SessionService _sessionService;
    private readonly FFFService _fffService;
    private readonly NameValidationService _nameValidationService;
    private static readonly Dictionary<string, CancellationTokenSource> _questionTimers = new();

    public FFFHub(ILogger<FFFHub> logger, SessionService sessionService, FFFService fffService, NameValidationService nameValidationService)
    {
        _logger = logger;
        _sessionService = sessionService;
        _fffService = fffService;
        _nameValidationService = nameValidationService;
    }

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

            // Check name uniqueness within session
            var existingParticipants = await _sessionService.GetActiveParticipantsAsync(sessionId);
            var existingNames = existingParticipants.Select(p => p.DisplayName);
            
            if (!_nameValidationService.IsNameUnique(validationResult.SanitizedName!, existingNames))
            {
                _logger.LogWarning("Duplicate name rejected: {DisplayName} in session {SessionId}", displayName, sessionId);
                return new
                {
                    Success = false,
                    Error = "This name is already in use. Please choose a different name."
                };
            }

            // Use sanitized name
            displayName = validationResult.SanitizedName!;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
        
        // Check if this is a reconnection
        var participant = await _sessionService.GetOrCreateParticipantAsync(
            sessionId, displayName, Context.ConnectionId, participantId, telemetry);
        
        _logger.LogInformation("Participant {DisplayName} ({ParticipantId}) joined session {SessionId} (Reconnect: {IsReconnect}, Device: {Device}/{OS})", 
            displayName, participant.Id, sessionId, participantId != null, telemetry?.DeviceType, telemetry?.OSType);
        
        // Notify all clients in the session about the participant
        await Clients.Group(sessionId).SendAsync("ParticipantJoined", new
        {
            ParticipantId = participant.Id,
            DisplayName = displayName,
            IsReconnect = participantId != null,
            Timestamp = DateTime.UtcNow
        });
        
        // Return participant ID to caller for localStorage persistence
        return new
        {
            Success = true,
            ParticipantId = participant.Id,
            DisplayName = participant.DisplayName,
            SessionId = sessionId,
            State = participant.State.ToString()
        };
    }

    /// <summary>
    /// Submit an answer to a FFF question
    /// </summary>
    public async Task<object> SubmitAnswer(string sessionId, string participantId, int questionId, string answerSequence)
    {
        var submittedAt = DateTime.UtcNow;
        
        // Check for duplicate submission
        if (await _sessionService.HasParticipantAnsweredAsync(sessionId, participantId, questionId))
        {
            _logger.LogWarning("Duplicate FFF answer blocked - Session: {SessionId}, Participant: {ParticipantId}, Question: {QuestionId}",
                sessionId, participantId, questionId);
            
            return new { Success = false, Error = "You have already submitted an answer for this question" };
        }
        
        _logger.LogInformation("Answer submitted - Session: {SessionId}, Participant: {ParticipantId}, Sequence: {Sequence}",
            sessionId, participantId, answerSequence);
        
        // Save answer to database
        await _sessionService.SaveFFFAnswerAsync(sessionId, participantId, questionId, answerSequence, submittedAt);
        
        // Acknowledge submission to the participant
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
        _logger.LogInformation("Starting FFF question {QuestionId} in session {SessionId} with {TimeLimit}ms time limit", 
            questionId, sessionId, timeLimit);
        
        // Cancel any existing timer for this session
        var timerKey = $"{sessionId}_{questionId}";
        if (_questionTimers.ContainsKey(timerKey))
        {
            _questionTimers[timerKey].Cancel();
            _questionTimers.Remove(timerKey);
        }
        
        // Update session with current question
        await _sessionService.UpdateSessionModeAsync(sessionId, SessionMode.FFF, questionId);
        
        // Broadcast question to all participants
        await Clients.Group(sessionId).SendAsync("QuestionStarted", new
        {
            QuestionId = questionId,
            Question = questionText,
            Options = options, // ["A", "B", "C", "D"]
            TimeLimit = timeLimit,
            StartTime = DateTime.UtcNow
        });
        
        // Start server-side timer to auto-end question
        var cts = new CancellationTokenSource();
        _questionTimers[timerKey] = cts;
        
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(timeLimit, cts.Token);
                if (!cts.Token.IsCancellationRequested)
                {
                    _logger.LogInformation("Auto-ending FFF question {QuestionId} after timeout", questionId);
                    await EndQuestion(sessionId, questionId);
                }
            }
            catch (TaskCanceledException)
            {
                _logger.LogDebug("Timer cancelled for question {QuestionId}", questionId);
            }
            finally
            {
                _questionTimers.Remove(timerKey);
            }
        }, cts.Token);
    }

    /// <summary>
    /// Host ends a FFF question (manually or via timer)
    /// </summary>
    public async Task EndQuestion(string sessionId, int questionId)
    {
        _logger.LogInformation("Ending FFF question {QuestionId} in session {SessionId}", questionId, sessionId);
        
        // Cancel timer if it exists
        var timerKey = $"{sessionId}_{questionId}";
        if (_questionTimers.ContainsKey(timerKey))
        {
            _questionTimers[timerKey].Cancel();
            _questionTimers.Remove(timerKey);
        }
        
        // Calculate rankings
        var results = await _fffService.CalculateRankingsAsync(sessionId, questionId);
        
        // Mark question as used
        await _fffService.MarkQuestionAsUsedAsync(questionId);
        
        // Clear session mode
        await _sessionService.UpdateSessionModeAsync(sessionId, SessionMode.Idle, null);
        
        // Broadcast results to all participants
        await Clients.Group(sessionId).SendAsync("QuestionEnded", new
        {
            QuestionId = questionId,
            EndTime = DateTime.UtcNow,
            Winner = results.Winner != null ? new
            {
                ParticipantId = results.Winner.ParticipantId,
                DisplayName = results.Winner.Participant?.DisplayName,
                TimeElapsed = results.Winner.TimeElapsed,
                Rank = results.Winner.Rank
            } : null,
            Leaderboard = results.Rankings.Select(r => new
            {
                ParticipantId = r.ParticipantId,
                DisplayName = r.Participant?.DisplayName,
                TimeElapsed = r.TimeElapsed,
                Rank = r.Rank,
                AnswerSequence = r.AnswerSequence
            }).ToList(),
            TotalSubmissions = results.TotalSubmissions,
            CorrectSubmissions = results.CorrectSubmissions
        });
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        
        // Mark participant as disconnected (not deactivated - they can reconnect)
        await _sessionService.MarkParticipantDisconnectedAsync(Context.ConnectionId);
        
        await base.OnDisconnectedAsync(exception);
    }
}
