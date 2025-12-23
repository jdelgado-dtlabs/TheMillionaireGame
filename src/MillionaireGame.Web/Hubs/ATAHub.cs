using Microsoft.AspNetCore.SignalR;
using MillionaireGame.Web.Services;

namespace MillionaireGame.Web.Hubs;

/// <summary>
/// SignalR hub for Ask The Audience (ATA) functionality
/// </summary>
public class ATAHub : Hub
{
    private readonly ILogger<ATAHub> _logger;
    private readonly SessionService _sessionService;

    public ATAHub(ILogger<ATAHub> logger, SessionService sessionService)
    {
        _logger = logger;
        _sessionService = sessionService;
    }

    /// <summary>
    /// Join a game session for ATA voting with persistent participant ID
    /// </summary>
    public async Task<object> JoinSession(string sessionId, string displayName, string? participantId = null)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
        
        // Check if this is a reconnection
        var participant = await _sessionService.GetOrCreateParticipantAsync(
            sessionId, displayName, Context.ConnectionId, participantId);
        
        _logger.LogInformation("Participant {DisplayName} ({ParticipantId}) joined ATA session {SessionId} (Reconnect: {IsReconnect})", 
            displayName, participant.Id, sessionId, participantId != null);
        
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
            ParticipantId = participant.Id,
            DisplayName = participant.DisplayName,
            SessionId = sessionId
        };
    }

    /// <summary>
    /// Host starts ATA voting
    /// </summary>
    public async Task StartVoting(string sessionId, string questionText, string[] options, int timeLimit)
    {
        _logger.LogInformation("Starting ATA voting in session {SessionId}", sessionId);
        
        await Clients.Group(sessionId).SendAsync("VotingStarted", new
        {
            QuestionText = questionText,
            Options = options,
            TimeLimit = timeLimit,
            StartTime = DateTime.UtcNow
        });
        
        // TODO: Auto-end voting after timeout
    }

    /// <summary>
    /// Submit a vote for ATA
    /// </summary>
    public async Task<object> SubmitVote(string sessionId, string participantId, string questionText, string selectedOption)
    {
        var submittedAt = DateTime.UtcNow;
        
        // Check for duplicate vote
        if (await _sessionService.HasParticipantVotedAsync(sessionId, participantId))
        {
            _logger.LogWarning("Duplicate ATA vote blocked - Session: {SessionId}, Participant: {ParticipantId}",
                sessionId, participantId);
            
            return new { Success = false, Error = "You have already voted for this question" };
        }
        
        _logger.LogInformation("Vote submitted - Session: {SessionId}, Participant: {ParticipantId}, Option: {Option}",
            sessionId, participantId, selectedOption);
        
        // Save vote to database
        await _sessionService.SaveATAVoteAsync(sessionId, participantId, questionText, selectedOption, submittedAt);
        
        // Calculate and broadcast updated percentages in real-time
        var percentages = await _sessionService.CalculateATAPercentagesAsync(sessionId);
        await Clients.Group(sessionId).SendAsync("VotesUpdated", percentages);
        
        // Acknowledge vote to participant
        await Clients.Caller.SendAsync("VoteReceived", new
        {
            Success = true,
            SelectedOption = selectedOption,
            Timestamp = submittedAt
        });
        
        return new { Success = true, Timestamp = submittedAt };
    }

    /// <summary>
    /// Host ends ATA voting
    /// </summary>
    public async Task EndVoting(string sessionId)
    {
        _logger.LogInformation("Ending ATA voting in session {SessionId}", sessionId);
        
        // TODO: Calculate final percentages
        
        await Clients.Group(sessionId).SendAsync("VotingEnded", new
        {
            EndTime = DateTime.UtcNow
            // TODO: Add final results with percentages
        });
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("ATA client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("ATA client disconnected: {ConnectionId}", Context.ConnectionId);
        
        // Mark participant as disconnected (not deactivated - they can reconnect)
        await _sessionService.MarkParticipantDisconnectedAsync(Context.ConnectionId);
        
        await base.OnDisconnectedAsync(exception);
    }
}
