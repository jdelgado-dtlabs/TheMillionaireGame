using Microsoft.AspNetCore.SignalR;
using MillionaireGame.Web.Services;
using MillionaireGame.Web.Models;

namespace MillionaireGame.Web.Hubs;

/// <summary>
/// SignalR hub for Ask The Audience (ATA) functionality
/// </summary>
public class ATAHub : Hub
{
    private readonly ILogger<ATAHub> _logger;
    private readonly SessionService _sessionService;
    private static readonly Dictionary<string, CancellationTokenSource> _votingTimers = new();
    private static readonly Dictionary<string, string> _currentQuestions = new();

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
            Success = true,
            ParticipantId = participant.Id,
            DisplayName = participant.DisplayName,
            SessionId = sessionId,
            State = participant.State.ToString(),
            CanVote = !participant.HasUsedATA && participant.IsActive
        };
    }

    /// <summary>
    /// Host starts ATA voting for a question
    /// </summary>
    public async Task StartVoting(string sessionId, string questionText, string optionA, string optionB, string optionC, string optionD, int timeLimit = 30)
    {
        try
        {
            // Cancel any existing timer for this session
            if (_votingTimers.ContainsKey(sessionId))
            {
                _votingTimers[sessionId].Cancel();
                _votingTimers[sessionId].Dispose();
                _votingTimers.Remove(sessionId);
            }

            // Store current question
            _currentQuestions[sessionId] = questionText;

            // Update session mode to ATA
            await _sessionService.UpdateSessionModeAsync(sessionId, SessionMode.ATA);

            _logger.LogInformation("Starting ATA voting in session {SessionId} with {TimeLimit}s timer", sessionId, timeLimit);
            
            // Broadcast voting started with question details
            await Clients.Group(sessionId).SendAsync("VotingStarted", new
            {
                QuestionText = questionText,
                OptionA = optionA,
                OptionB = optionB,
                OptionC = optionC,
                OptionD = optionD,
                TimeLimit = timeLimit,
                StartTime = DateTime.UtcNow
            });
            
            // Set up auto-end timer
            var cts = new CancellationTokenSource();
            _votingTimers[sessionId] = cts;
            
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(timeLimit), cts.Token);
                    
                    if (!cts.Token.IsCancellationRequested)
                    {
                        _logger.LogInformation("Auto-ending ATA voting for session {SessionId} after {TimeLimit}s", sessionId, timeLimit);
                        await EndVoting(sessionId);
                    }
                }
                catch (TaskCanceledException)
                {
                    _logger.LogInformation("ATA voting timer cancelled for session {SessionId}", sessionId);
                }
                finally
                {
                    _votingTimers.Remove(sessionId);
                    cts.Dispose();
                }
            }, cts.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting ATA voting in session {SessionId}", sessionId);
            throw;
        }
    }

    /// <summary>
    /// Submit a vote for ATA (once per round per participant)
    /// </summary>
    public async Task<object> SubmitVote(string sessionId, string participantId, string selectedOption)
    {
        try
        {
            var submittedAt = DateTime.UtcNow;
            
            // Get participant to check eligibility
            var participants = await _sessionService.GetActiveParticipantsAsync(sessionId);
            var participant = participants.FirstOrDefault(p => p.Id == participantId);
            
            if (participant == null)
            {
                _logger.LogWarning("Vote rejected - Participant not found: {ParticipantId}", participantId);
                return new { Success = false, Error = "Participant not found" };
            }

            // Check once-per-round restriction (HasUsedATA flag)
            if (participant.HasUsedATA)
            {
                _logger.LogWarning("Vote rejected - Participant {ParticipantId} has already used ATA this round", participantId);
                return new { Success = false, Error = "You have already used Ask The Audience in this round" };
            }

            // Get current question
            if (!_currentQuestions.TryGetValue(sessionId, out var questionText))
            {
                _logger.LogWarning("Vote rejected - No active ATA question in session {SessionId}", sessionId);
                return new { Success = false, Error = "No active voting in progress" };
            }

            // Check for duplicate vote in current question
            if (await _sessionService.HasParticipantVotedAsync(sessionId, participantId))
            {
                _logger.LogWarning("Duplicate ATA vote blocked - Session: {SessionId}, Participant: {ParticipantId}",
                    sessionId, participantId);
                
                return new { Success = false, Error = "You have already voted for this question" };
            }
            
            // Validate option
            if (!new[] { "A", "B", "C", "D" }.Contains(selectedOption.ToUpper()))
            {
                _logger.LogWarning("Invalid vote option: {Option}", selectedOption);
                return new { Success = false, Error = "Invalid option selected" };
            }

            _logger.LogInformation("Vote submitted - Session: {SessionId}, Participant: {ParticipantId}, Option: {Option}",
                sessionId, participantId, selectedOption);
            
            // Save vote to database
            await _sessionService.SaveATAVoteAsync(sessionId, participantId, questionText, selectedOption.ToUpper(), submittedAt);
            
            // Calculate and broadcast updated percentages in real-time
            var percentages = await _sessionService.CalculateATAPercentagesAsync(sessionId);
            var totalVotes = await _sessionService.GetATAVoteCountAsync(sessionId);
            var activeParticipants = await _sessionService.GetActiveParticipantsAsync(sessionId);
            var activeCount = activeParticipants.Count();
            
            // WebServer console logging
            try 
            {
                var consoleType = Type.GetType("MillionaireGame.Utilities.WebServerConsole, MillionaireGame");
                if (consoleType != null)
                {
                    var logMethod = consoleType.GetMethod("Log", new[] { typeof(string) });
                    logMethod?.Invoke(null, new object[] { $"Player {participantId} voted in ATA" });
                    logMethod?.Invoke(null, new object[] { $"ATA Progress: {totalVotes} of {activeCount} players voted" });
                }
            }
            catch { /* WebService console not available - ignore */ }
            
            await Clients.Group(sessionId).SendAsync("VotesUpdated", new
            {
                Percentages = percentages,
                TotalVotes = totalVotes,
                Timestamp = DateTime.UtcNow
            });
            
            // Acknowledge vote to participant
            await Clients.Caller.SendAsync("VoteReceived", new
            {
                Success = true,
                SelectedOption = selectedOption.ToUpper(),
                Timestamp = submittedAt
            });
            
            return new { Success = true, SelectedOption = selectedOption.ToUpper(), Timestamp = submittedAt };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting vote in session {SessionId}", sessionId);
            return new { Success = false, Error = "An error occurred while submitting your vote" };
        }
    }

    /// <summary>
    /// Host ends ATA voting and marks participants as having used ATA
    /// </summary>
    public async Task EndVoting(string sessionId)
    {
        try
        {
            _logger.LogInformation("Ending ATA voting in session {SessionId}", sessionId);
            
            // Cancel timer if still running
            if (_votingTimers.ContainsKey(sessionId))
            {
                _votingTimers[sessionId].Cancel();
                _votingTimers[sessionId].Dispose();
                _votingTimers.Remove(sessionId);
            }

            // Calculate final percentages
            var percentages = await _sessionService.CalculateATAPercentagesAsync(sessionId);
            var totalVotes = await _sessionService.GetATAVoteCountAsync(sessionId);
            
            // Mark all voters as having used ATA for this round
            await _sessionService.MarkATAUsedForVotersAsync(sessionId);
            
            // Update session mode back to idle
            await _sessionService.UpdateSessionModeAsync(sessionId, SessionMode.Idle);

            // Remove current question
            _currentQuestions.Remove(sessionId);
            
            // Broadcast final results
            await Clients.Group(sessionId).SendAsync("VotingEnded", new
            {
                EndTime = DateTime.UtcNow,
                FinalResults = percentages,
                TotalVotes = totalVotes
            });
            
            _logger.LogInformation("ATA voting ended - Session: {SessionId}, Total Votes: {TotalVotes}", sessionId, totalVotes);
            
            // WebService console logging
            try 
            {
                var activeParticipants = await _sessionService.GetActiveParticipantsAsync(sessionId);
                var activeCount = activeParticipants.Count();
                var consoleType = Type.GetType("MillionaireGame.Utilities.WebServiceConsole, MillionaireGame");
                if (consoleType != null)
                {
                    var logSeparatorMethod = consoleType.GetMethod("LogSeparator");
                    var logMethod = consoleType.GetMethod("Log", new[] { typeof(string) });
                    logSeparatorMethod?.Invoke(null, null);
                    logMethod?.Invoke(null, new object[] { $"ATA voting complete: {totalVotes} of {activeCount} players voted" });
                    logSeparatorMethod?.Invoke(null, null);
                }
            }
            catch { /* WebService console not available - ignore */ }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending ATA voting in session {SessionId}", sessionId);
            throw;
        }
    }

    /// <summary>
    /// Get current voting status for a session
    /// </summary>
    public async Task<object> GetVotingStatus(string sessionId, string participantId)
    {
        try
        {
            var participants = await _sessionService.GetActiveParticipantsAsync(sessionId);
            var participant = participants.FirstOrDefault(p => p.Id == participantId);
            
            if (participant == null)
            {
                return new { Success = false, Error = "Participant not found" };
            }

            var hasVoted = await _sessionService.HasParticipantVotedAsync(sessionId, participantId);
            var percentages = await _sessionService.CalculateATAPercentagesAsync(sessionId);
            var totalVotes = await _sessionService.GetATAVoteCountAsync(sessionId);
            var isActive = _currentQuestions.ContainsKey(sessionId);

            return new
            {
                Success = true,
                IsActive = isActive,
                HasVoted = hasVoted,
                HasUsedATA = participant.HasUsedATA,
                CanVote = isActive && !hasVoted && !participant.HasUsedATA,
                CurrentResults = percentages,
                TotalVotes = totalVotes
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting voting status for session {SessionId}", sessionId);
            return new { Success = false, Error = "An error occurred" };
        }
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
