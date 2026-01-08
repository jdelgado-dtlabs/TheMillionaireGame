using Microsoft.EntityFrameworkCore;
using MillionaireGame.Web.Data;
using MillionaireGame.Web.Models;
using MillionaireGame.Core.Services;

namespace MillionaireGame.Web.Services;

/// <summary>
/// Service for managing game sessions
/// </summary>
public class SessionService
{
    private readonly WAPSDbContext _context;
    private readonly ILogger<SessionService> _logger;
    private GameStateType _currentState = GameStateType.InitialLobby;

    public SessionService(WAPSDbContext context, ILogger<SessionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get the current game state
    /// </summary>
    public GameStateType GetCurrentState() => _currentState;

    /// <summary>
    /// Set the current game state
    /// </summary>
    public void SetCurrentState(GameStateType state)
    {
        _currentState = state;
        _logger.LogInformation("Game state changed to: {State}", state);
    }

    /// <summary>
    /// Create a new session
    /// </summary>
    public async Task<Session> CreateSessionAsync(string hostName)
    {
        var session = new Session
        {
            HostName = hostName,
            CreatedAt = DateTime.UtcNow,
            Status = SessionStatus.Waiting
        };

        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created session {SessionId} for host {HostName}", session.Id, hostName);
        return session;
    }

    /// <summary>
    /// Get a session by ID
    /// </summary>
    public async Task<Session?> GetSessionAsync(string sessionId)
    {
        return await _context.Sessions
            .Include(s => s.Participants)
            .FirstOrDefaultAsync(s => s.Id == sessionId);
    }

    /// <summary>
    /// Get existing participant or create new one (supports reconnection)
    /// </summary>
    public async Task<Participant> GetOrCreateParticipantAsync(string sessionId, string displayName, string connectionId, string? participantId = null, DeviceTelemetry? telemetry = null)
    {
        // First, ensure the session exists (auto-create if needed for demo/testing)
        var session = await _context.Sessions.FindAsync(sessionId);
        if (session == null)
        {
            _logger.LogInformation("Auto-creating session {SessionId} for participant join", sessionId);
            session = new Session
            {
                Id = sessionId,
                HostName = "Auto-Created Session",
                CreatedAt = DateTime.UtcNow,
                StartedAt = DateTime.UtcNow,
                Status = SessionStatus.Active  // Auto-created sessions are immediately active
            };
            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();
        }

        Participant? participant = null;

        // If participantId provided, try to find existing participant (reconnection)
        if (!string.IsNullOrEmpty(participantId))
        {
            participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.Id == participantId && p.SessionId == sessionId);
            
            if (participant != null)
            {
                // Update connection info for reconnection
                participant.ConnectionId = connectionId;
                participant.LastSeenAt = DateTime.UtcNow;
                participant.IsActive = true;
                
                // Update telemetry if provided (user might have changed device)
                if (telemetry != null)
                {
                    participant.DeviceType = telemetry.DeviceType;
                    participant.OSType = telemetry.OSType;
                    participant.OSVersion = telemetry.OSVersion;
                    participant.BrowserType = telemetry.BrowserType;
                    participant.BrowserVersion = telemetry.BrowserVersion;
                }
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Participant {ParticipantId} reconnected to session {SessionId} with new connection {ConnectionId}",
                    participantId, sessionId, connectionId);
                
                // WebService console logging
                try 
                {
                    var activeCount = await _context.Participants.CountAsync(p => p.SessionId == sessionId && p.IsActive);
                    var consoleType = Type.GetType("MillionaireGame.Utilities.WebServerConsole, MillionaireGame");
                    if (consoleType != null)
                    {
                        var logMethod = consoleType.GetMethod("Log", new[] { typeof(string) });
                        logMethod?.Invoke(null, new object[] { $"Player {participantId} reconnected" });
                        logMethod?.Invoke(null, new object[] { $"Total: {activeCount} players on APS" });
                    }
                }
                catch { /* WebService console not available - ignore */ }
                
                return participant;
            }
        }

        // Create new participant with telemetry
        participant = new Participant
        {
            SessionId = sessionId,
            DisplayName = displayName,
            ConnectionId = connectionId,
            JoinedAt = DateTime.UtcNow,
            LastSeenAt = DateTime.UtcNow,
            IsActive = true,
            DeviceType = telemetry?.DeviceType,
            OSType = telemetry?.OSType,
            OSVersion = telemetry?.OSVersion,
            BrowserType = telemetry?.BrowserType,
            BrowserVersion = telemetry?.BrowserVersion,
            HasAgreedToPrivacy = telemetry?.HasAgreedToPrivacy ?? false,
            GameSessionId = TelemetryService.Instance.CurrentSessionId
        };

        _context.Participants.Add(participant);
        await _context.SaveChangesAsync();

        _logger.LogInformation("New participant {DisplayName} ({ParticipantId}) added to session {SessionId} with device {DeviceType}/{OSType}",
            displayName, participant.Id, sessionId, telemetry?.DeviceType, telemetry?.OSType);
        
        // WebService console logging
        try 
        {
            var activeCount = await _context.Participants.CountAsync(p => p.SessionId == sessionId && p.IsActive);
            var consoleType = Type.GetType("MillionaireGame.Utilities.WebServerConsole, MillionaireGame");
            if (consoleType != null)
            {
                var logMethod = consoleType.GetMethod("Log", new[] { typeof(string) });
                logMethod?.Invoke(null, new object[] { $"Player {participant.Id} registered" });
                logMethod?.Invoke(null, new object[] { $"Player {participant.Id} connected" });
                logMethod?.Invoke(null, new object[] { $"Total: {activeCount} players on APS" });
            }
        }
        catch { /* WebService console not available - ignore */ }

        return participant;
    }

    /// <summary>
    /// Check if participant has already answered a FFF question
    /// </summary>
    public async Task<bool> HasParticipantAnsweredAsync(string sessionId, string participantId, int questionId)
    {
        return await _context.FFFAnswers
            .AnyAsync(a => a.SessionId == sessionId && 
                          a.ParticipantId == participantId && 
                          a.QuestionId == questionId);
    }

    /// <summary>
    /// Check if participant has already voted in current ATA
    /// </summary>
    public async Task<bool> HasParticipantVotedAsync(string sessionId, string participantId)
    {
        var session = await _context.Sessions.FindAsync(sessionId);
        if (session == null) return false;

        // Check for votes in the current ATA session (within last 5 minutes to handle question changes)
        var recentVoteCutoff = DateTime.UtcNow.AddMinutes(-5);
        return await _context.ATAVotes
            .AnyAsync(v => v.SessionId == sessionId && 
                          v.ParticipantId == participantId &&
                          v.SubmittedAt > recentVoteCutoff);
    }

    /// <summary>
    /// Save a FFF answer
    /// </summary>
    public async Task SaveFFFAnswerAsync(string sessionId, string participantId, int questionId, string answerSequence, DateTime submittedAt)
    {
        var session = await _context.Sessions
            .FirstOrDefaultAsync(s => s.Id == sessionId);
        
        if (session == null)
        {
            throw new InvalidOperationException($"Session {sessionId} not found");
        }

        // Calculate time elapsed from question start time
        var timeElapsed = session.QuestionStartTime.HasValue 
            ? (submittedAt - session.QuestionStartTime.Value).TotalMilliseconds 
            : 0;

        var answer = new FFFAnswer
        {
            SessionId = sessionId,
            ParticipantId = participantId,
            QuestionId = questionId,
            AnswerSequence = answerSequence,
            SubmittedAt = submittedAt,
            TimeElapsed = timeElapsed,
            IsCorrect = false, // Will be validated later
            GameSessionId = TelemetryService.Instance.CurrentSessionId
        };

        _context.FFFAnswers.Add(answer);
        await _context.SaveChangesAsync();

        _logger.LogInformation("FFF answer saved - Session: {SessionId}, Participant: {ParticipantId}, Question: {QuestionId}, TimeElapsed: {TimeElapsed}ms",
            sessionId, participantId, questionId, timeElapsed);
    }

    /// <summary>
    /// Get all answers for a specific FFF question
    /// </summary>
    public async Task<List<FFFAnswer>> GetAnswersForQuestionAsync(string sessionId, int questionId)
    {
        return await _context.FFFAnswers
            .Include(a => a.Participant)
            .Where(a => a.SessionId == sessionId && a.QuestionId == questionId)
            .OrderBy(a => a.SubmittedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Save an ATA vote
    /// </summary>
    public async Task SaveATAVoteAsync(string sessionId, string participantId, string questionText, string selectedOption, DateTime submittedAt)
    {
        var vote = new ATAVote
        {
            SessionId = sessionId,
            ParticipantId = participantId,
            QuestionText = questionText,
            SelectedOption = selectedOption,
            SubmittedAt = submittedAt,
            GameSessionId = TelemetryService.Instance.CurrentSessionId
        };

        _context.ATAVotes.Add(vote);
        await _context.SaveChangesAsync();

        _logger.LogInformation("ATA vote saved - Session: {SessionId}, Participant: {ParticipantId}, Option: {Option}",
            sessionId, participantId, selectedOption);
    }

    /// <summary>
    /// Calculate ATA voting percentages
    /// </summary>
    public async Task<Dictionary<string, double>> CalculateATAPercentagesAsync(string sessionId)
    {
        // Get votes from last 5 minutes (current question)
        var recentVoteCutoff = DateTime.UtcNow.AddMinutes(-5);
        var votes = await _context.ATAVotes
            .Where(v => v.SessionId == sessionId && v.SubmittedAt > recentVoteCutoff)
            .GroupBy(v => v.SelectedOption)
            .Select(g => new { Option = g.Key, Count = g.Count() })
            .ToListAsync();

        var total = votes.Sum(v => v.Count);
        
        var percentages = new Dictionary<string, double>();
        foreach (var vote in votes)
        {
            percentages[vote.Option] = total > 0 ? (double)vote.Count / total * 100 : 0;
        }

        // Ensure all options (A, B, C, D) are present even if zero
        foreach (var option in new[] { "A", "B", "C", "D" })
        {
            if (!percentages.ContainsKey(option))
            {
                percentages[option] = 0;
            }
        }

        return percentages;
    }

    /// <summary>
    /// Mark participant as disconnected (but not inactive - they can reconnect)
    /// </summary>
    public async Task MarkParticipantDisconnectedAsync(string connectionId)
    {
        var participant = await _context.Participants
            .FirstOrDefaultAsync(p => p.ConnectionId == connectionId);
        
        if (participant != null)
        {
            participant.LastSeenAt = DateTime.UtcNow;
            participant.DisconnectedAt = DateTime.UtcNow; // Track when they left for play duration
            // Keep IsActive = true so they can reconnect
            await _context.SaveChangesAsync();

            _logger.LogInformation("Participant {ParticipantId} disconnected (connection {ConnectionId})",
                participant.Id, connectionId);
            
            // WebService console logging
            try 
            {
                var activeCount = await _context.Participants.CountAsync(p => p.SessionId == participant.SessionId && p.IsActive);
                var consoleType = Type.GetType("MillionaireGame.Utilities.WebServerConsole, MillionaireGame");
                if (consoleType != null)
                {
                    var logMethod = consoleType.GetMethod("Log", new[] { typeof(string) });
                    logMethod?.Invoke(null, new object[] { $"Player {participant.Id} disconnected" });
                    logMethod?.Invoke(null, new object[] { $"Total: {activeCount} players on APS" });
                }
            }
            catch { /* WebService console not available - ignore */ }
        }
    }

    /// <summary>
    /// Get active participants in a session
    /// </summary>
    public async Task<IEnumerable<Participant>> GetActiveParticipantsAsync(string sessionId)
    {
        return await _context.Participants
            .Where(p => p.SessionId == sessionId && p.IsActive)
            .OrderBy(p => p.JoinedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Find participant by display name (for reconnection detection)
    /// </summary>
    public async Task<Participant?> FindParticipantByNameAsync(string sessionId, string displayName)
    {
        return await _context.Participants
            .FirstOrDefaultAsync(p => p.SessionId == sessionId && 
                                     p.DisplayName == displayName && 
                                     p.IsActive);
    }

    /// <summary>
    /// Update participant last seen timestamp
    /// </summary>
    public async Task UpdateParticipantLastSeenAsync(string participantId)
    {
        var participant = await _context.Participants.FindAsync(participantId);
        if (participant != null)
        {
            participant.LastSeenAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Deactivate a participant (on disconnect)
    /// </summary>
    public async Task DeactivateParticipantAsync(string connectionId)
    {
        var participant = await _context.Participants
            .FirstOrDefaultAsync(p => p.ConnectionId == connectionId && p.IsActive);
        
        if (participant != null)
        {
            participant.IsActive = false;
            participant.LastSeenAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deactivated participant {ParticipantId} (connection {ConnectionId})",
                participant.Id, connectionId);
        }
    }

    /// <summary>
    /// Start a session
    /// </summary>
    public async Task StartSessionAsync(string sessionId)
    {
        var session = await _context.Sessions.FindAsync(sessionId);
        if (session != null)
        {
            session.Status = SessionStatus.Active;
            session.StartedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Started session {SessionId}", sessionId);
        }
    }

    /// <summary>
    /// End a session
    /// </summary>
    public async Task EndSessionAsync(string sessionId)
    {
        var session = await _context.Sessions.FindAsync(sessionId);
        if (session != null)
        {
            session.Status = SessionStatus.Completed;
            session.EndedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Ended session {SessionId}", sessionId);
        }
    }

    /// <summary>
    /// Update session mode (FFF, ATA, or Idle)
    /// </summary>
    public async Task UpdateSessionModeAsync(string sessionId, SessionMode mode)
    {
        var session = await _context.Sessions.FindAsync(sessionId);
        if (session != null)
        {
            session.CurrentMode = mode;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated session {SessionId} mode to {Mode}", sessionId, mode);
        }
    }

    /// <summary>
    /// Update session mode and current question
    /// </summary>
    public async Task UpdateSessionModeAsync(string sessionId, SessionMode mode, int? questionId)
    {
        await UpdateSessionModeAsync(sessionId, mode, questionId, null, null);
    }

    /// <summary>
    /// Update session mode, current question, and question details (for state sync)
    /// </summary>
    public async Task UpdateSessionModeAsync(string sessionId, SessionMode mode, int? questionId, 
        string? questionText = null, string[]? options = null)
    {
        var session = await _context.Sessions.FindAsync(sessionId);
        if (session != null)
        {
            session.CurrentMode = mode;
            session.CurrentQuestionId = questionId;
            session.CurrentQuestionText = questionText;
            
            // Store options as JSON if provided
            if (options != null && options.Length > 0)
            {
                session.CurrentQuestionOptionsJson = System.Text.Json.JsonSerializer.Serialize(options);
            }
            
            // Set question start time when FFF or ATA question starts
            if ((mode == SessionMode.FFF || mode == SessionMode.ATA) && questionId.HasValue)
            {
                session.QuestionStartTime = DateTime.UtcNow;
                _logger.LogInformation("Question {QuestionId} started at {StartTime}", 
                    questionId, session.QuestionStartTime);
            }
            else if (mode == SessionMode.Idle)
            {
                session.QuestionStartTime = null;
                session.CurrentQuestionText = null;
                session.CurrentQuestionOptionsJson = null;
            }
            
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated session {SessionId} mode to {Mode}, question {QuestionId}", 
                sessionId, mode, questionId);
        }
    }

    /// <summary>
    /// Get a single participant by ID
    /// </summary>
    public async Task<Participant?> GetParticipantAsync(string participantId)
    {
        return await _context.Participants
            .FirstOrDefaultAsync(p => p.Id == participantId);
    }

    // ===== HOST CONTROL METHODS (Phase 2.5) =====

    /// <summary>
    /// Start the game - transitions from PreGame to Lobby
    /// </summary>
    public async Task<bool> StartGameAsync(string sessionId)
    {
        var session = await _context.Sessions
            .Include(s => s.Participants)
            .FirstOrDefaultAsync(s => s.Id == sessionId);
        
        if (session == null)
        {
            _logger.LogWarning("Cannot start game - session {SessionId} not found", sessionId);
            return false;
        }

        session.Status = SessionStatus.Lobby;
        session.StartedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Started game for session {SessionId} with {Count} participants", 
            sessionId, session.Participants.Count);
        return true;
    }

    /// <summary>
    /// Select players for FFF round (exactly 8 from lobby)
    /// </summary>
    public async Task<List<Participant>> SelectFFFPlayersAsync(string sessionId, int count = 8)
    {
        var eligibleParticipants = await _context.Participants
            .Where(p => p.SessionId == sessionId && 
                       p.IsActive && 
                       p.State == ParticipantState.Lobby)
            .ToListAsync();

        if (eligibleParticipants.Count < count)
        {
            _logger.LogWarning("Not enough eligible participants for FFF - need {Need}, have {Have}", 
                count, eligibleParticipants.Count);
            count = eligibleParticipants.Count;
        }

        // Randomly select participants
        var random = new Random();
        var selected = eligibleParticipants
            .OrderBy(_ => random.Next())
            .Take(count)
            .ToList();

        // Update their state
        foreach (var participant in selected)
        {
            participant.State = ParticipantState.SelectedForFFF;
            participant.SelectedForFFFAt = DateTime.UtcNow;
        }

        // Update session status
        var session = await _context.Sessions.FindAsync(sessionId);
        if (session != null)
        {
            session.Status = SessionStatus.FFFSelection;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Selected {Count} participants for FFF in session {SessionId}", 
            selected.Count, sessionId);
        return selected;
    }

    /// <summary>
    /// Select one random player from lobby (when winner directly chosen)
    /// </summary>
    public async Task<Participant?> SelectRandomPlayerAsync(string sessionId)
    {
        var eligibleParticipants = await _context.Participants
            .Where(p => p.SessionId == sessionId && 
                       p.IsActive && 
                       p.State == ParticipantState.Lobby)
            .ToListAsync();

        if (!eligibleParticipants.Any())
        {
            _logger.LogWarning("No eligible participants to select in session {SessionId}", sessionId);
            return null;
        }

        // Randomly select one
        var random = new Random();
        var selected = eligibleParticipants[random.Next(eligibleParticipants.Count)];
        
        // Mark as winner and transition to main game
        selected.State = ParticipantState.Winner;
        selected.BecameWinnerAt = DateTime.UtcNow;
        selected.HasPlayedFFF = true; // Even though selected randomly, they've "played"

        var session = await _context.Sessions.FindAsync(sessionId);
        if (session != null)
        {
            session.Status = SessionStatus.MainGame;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Randomly selected participant {ParticipantId} as winner in session {SessionId}", 
            selected.Id, sessionId);
        return selected;
    }

    /// <summary>
    /// Set a participant as the winner (after FFF)
    /// </summary>
    public async Task<bool> SetWinnerAsync(string sessionId, string participantId)
    {
        var participant = await _context.Participants
            .FirstOrDefaultAsync(p => p.Id == participantId && p.SessionId == sessionId);
        
        if (participant == null)
        {
            _logger.LogWarning("Cannot set winner - participant {ParticipantId} not found", participantId);
            return false;
        }

        // Update winner
        participant.State = ParticipantState.Winner;
        participant.BecameWinnerAt = DateTime.UtcNow;
        participant.HasPlayedFFF = true;

        // Return losers to lobby
        var losers = await _context.Participants
            .Where(p => p.SessionId == sessionId && 
                       p.State == ParticipantState.SelectedForFFF &&
                       p.Id != participantId)
            .ToListAsync();

        foreach (var loser in losers)
        {
            loser.State = ParticipantState.Eliminated;
        }

        // Update session to main game
        var session = await _context.Sessions.FindAsync(sessionId);
        if (session != null)
        {
            session.Status = SessionStatus.MainGame;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Set participant {ParticipantId} as winner, {LoserCount} returned to lobby", 
            participantId, losers.Count);
        return true;
    }

    /// <summary>
    /// Return eliminated FFF players to lobby for next round
    /// </summary>
    public async Task<int> ReturnEliminatedToLobbyAsync(string sessionId)
    {
        var eliminated = await _context.Participants
            .Where(p => p.SessionId == sessionId && 
                       p.State == ParticipantState.Eliminated)
            .ToListAsync();

        foreach (var participant in eliminated)
        {
            participant.State = ParticipantState.Lobby;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Returned {Count} eliminated participants to lobby in session {SessionId}", 
            eliminated.Count, sessionId);
        return eliminated.Count;
    }

    /// <summary>
    /// End game with statistics export and cleanup
    /// </summary>
    public async Task<string> EndGameAsync(string sessionId, StatisticsService statisticsService)
    {
        var session = await _context.Sessions
            .Include(s => s.Participants)
            .Include(s => s.FFFAnswers)
            .Include(s => s.ATAVotes)
            .FirstOrDefaultAsync(s => s.Id == sessionId);
        
        if (session == null)
        {
            throw new InvalidOperationException("Session not found");
        }

        // Generate statistics CSV
        string csvData = await statisticsService.GenerateSessionStatisticsCsvAsync(sessionId);

        // Update session status
        session.Status = SessionStatus.GameOver;
        session.EndedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Ended game for session {SessionId}, generated statistics", sessionId);
        
        return csvData;
    }

    /// <summary>
    /// Cleanup session data (after stats are exported and saved)
    /// </summary>
    public async Task CleanupSessionAsync(string sessionId)
    {
        var session = await _context.Sessions
            .Include(s => s.Participants)
            .Include(s => s.FFFAnswers)
            .Include(s => s.ATAVotes)
            .FirstOrDefaultAsync(s => s.Id == sessionId);
        
        if (session == null)
        {
            return;
        }

        // Remove all related data
        _context.FFFAnswers.RemoveRange(session.FFFAnswers);
        _context.ATAVotes.RemoveRange(session.ATAVotes);
        _context.Participants.RemoveRange(session.Participants);
        _context.Sessions.Remove(session);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Cleaned up session {SessionId} and all related data", sessionId);
    }

    /// <summary>
    /// Get lobby participants (eligible for selection)
    /// </summary>
    public async Task<List<Participant>> GetLobbyParticipantsAsync(string sessionId)
    {
        return await _context.Participants
            .Where(p => p.SessionId == sessionId && 
                       p.IsActive && 
                       p.State == ParticipantState.Lobby)
            .OrderBy(p => p.JoinedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Get participants who can participate in ATA (everyone in lobby + HasPlayedFFF)
    /// </summary>
    public async Task<List<Participant>> GetATAEligibleParticipantsAsync(string sessionId)
    {
        return await _context.Participants
            .Where(p => p.SessionId == sessionId && 
                       p.IsActive && 
                       !p.HasUsedATA &&
                       (p.State == ParticipantState.Lobby || 
                        p.State == ParticipantState.HasPlayedFFF))
            .ToListAsync();
    }

    /// <summary>
    /// Get total vote count for current ATA question
    /// </summary>
    public async Task<int> GetATAVoteCountAsync(string sessionId)
    {
        var recentVoteCutoff = DateTime.UtcNow.AddMinutes(-5);
        return await _context.ATAVotes
            .Where(v => v.SessionId == sessionId && v.SubmittedAt > recentVoteCutoff)
            .CountAsync();
    }

    /// <summary>
    /// Mark all participants who voted as having used ATA for this round
    /// </summary>
    public async Task MarkATAUsedForVotersAsync(string sessionId)
    {
        var recentVoteCutoff = DateTime.UtcNow.AddMinutes(-5);
        var voterIds = await _context.ATAVotes
            .Where(v => v.SessionId == sessionId && v.SubmittedAt > recentVoteCutoff)
            .Select(v => v.ParticipantId)
            .Distinct()
            .ToListAsync();

        var voters = await _context.Participants
            .Where(p => voterIds.Contains(p.Id))
            .ToListAsync();

        foreach (var voter in voters)
        {
            voter.HasUsedATA = true;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Marked {Count} participants as having used ATA in session {SessionId}", 
            voters.Count, sessionId);
    }
    
    /// <summary>
    /// Get participant telemetry aggregates for current session
    /// </summary>
    public async Task<(int totalCount, Dictionary<string, int> deviceTypes, Dictionary<string, int> browserTypes, Dictionary<string, int> osTypes)> GetParticipantTelemetryAsync(string sessionId)
    {
        var participants = await _context.Participants
            .Where(p => p.SessionId == sessionId && p.IsActive)
            .ToListAsync();
        
        var totalCount = participants.Count;
        
        // Count device types
        var deviceTypes = participants
            .Where(p => !string.IsNullOrEmpty(p.DeviceType))
            .GroupBy(p => p.DeviceType!)
            .ToDictionary(g => g.Key, g => g.Count());
        
        // Count browser types
        var browserTypes = participants
            .Where(p => !string.IsNullOrEmpty(p.BrowserType))
            .GroupBy(p => p.BrowserType!)
            .ToDictionary(g => g.Key, g => g.Count());
        
        // Count OS types
        var osTypes = participants
            .Where(p => !string.IsNullOrEmpty(p.OSType))
            .GroupBy(p => p.OSType!)
            .ToDictionary(g => g.Key, g => g.Count());
        
        return (totalCount, deviceTypes, browserTypes, osTypes);
    }
    
    /// <summary>
    /// Finalize ATA telemetry after voting ends
    /// </summary>
    public async Task FinalizeATATelemetryAsync(string sessionId, string mode = "Online")
    {
        // Get participant count for completion rate
        var (totalCount, _, _, _) = await GetParticipantTelemetryAsync(sessionId);
        
        // Get recent votes (last 5 minutes)
        var recentVoteCutoff = DateTime.UtcNow.AddMinutes(-5);
        var votes = await _context.ATAVotes
            .Where(v => v.SessionId == sessionId && v.SubmittedAt > recentVoteCutoff)
            .ToListAsync();
        
        var totalVotes = votes.Count;
        
        // Count votes per option
        var votesA = votes.Count(v => v.SelectedOption == "A");
        var votesB = votes.Count(v => v.SelectedOption == "B");
        var votesC = votes.Count(v => v.SelectedOption == "C");
        var votesD = votes.Count(v => v.SelectedOption == "D");
        
        // Calculate percentages
        var percentageA = totalVotes > 0 ? (double)votesA / totalVotes * 100 : 0;
        var percentageB = totalVotes > 0 ? (double)votesB / totalVotes * 100 : 0;
        var percentageC = totalVotes > 0 ? (double)votesC / totalVotes * 100 : 0;
        var percentageD = totalVotes > 0 ? (double)votesD / totalVotes * 100 : 0;
        
        // Calculate completion rate
        var completionRate = totalCount > 0 ? (double)totalVotes / totalCount * 100 : 0;
        
        var ataData = new ATATelemetryData
        {
            TotalVotesCast = totalVotes,
            VotesForA = votesA,
            VotesForB = votesB,
            VotesForC = votesC,
            VotesForD = votesD,
            PercentageA = percentageA,
            PercentageB = percentageB,
            PercentageC = percentageC,
            PercentageD = percentageD,
            VotingCompletionRate = completionRate,
            Mode = mode
        };
        
        TelemetryBridge.OnATAStats?.Invoke(ataData);
        _logger.LogInformation("[Telemetry] ATA stats recorded: {Total} votes, Completion: {Completion:F1}%, Mode: {Mode}",
            totalVotes, completionRate, mode);
    }

    // ===== STATE SYNCHRONIZATION METHODS (Phase: Web State Sync) =====

    /// <summary>
    /// Get current active FFF question state for a session (for reconnection sync)
    /// </summary>
    public async Task<FFFQuestionState?> GetCurrentFFFStateAsync(string sessionId)
    {
        var session = await _context.Sessions.FindAsync(sessionId);
        if (session?.CurrentMode != SessionMode.FFF || !session.CurrentQuestionId.HasValue)
            return null;
        
        if (string.IsNullOrEmpty(session.CurrentQuestionText))
        {
            _logger.LogWarning("FFF question {QuestionId} active but no question text stored", 
                session.CurrentQuestionId);
            return null;
        }
        
        // Parse options from JSON
        string[] options = Array.Empty<string>();
        if (!string.IsNullOrEmpty(session.CurrentQuestionOptionsJson))
        {
            try
            {
                options = System.Text.Json.JsonSerializer.Deserialize<string[]>(
                    session.CurrentQuestionOptionsJson) ?? Array.Empty<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse question options JSON");
            }
        }
        
        // Calculate if question is still active (20 second limit)
        var elapsed = session.QuestionStartTime.HasValue
            ? (DateTime.UtcNow - session.QuestionStartTime.Value).TotalMilliseconds
            : 0;
        var isActive = elapsed < 20000;
        
        return new FFFQuestionState
        {
            QuestionId = session.CurrentQuestionId.Value,
            QuestionText = session.CurrentQuestionText,
            Options = options,
            StartTime = session.QuestionStartTime ?? DateTime.UtcNow,
            TimeLimit = 20000,
            IsActive = isActive
        };
    }

    /// <summary>
    /// Get current active ATA state for a session (for reconnection sync)
    /// </summary>
    public async Task<ATAQuestionState?> GetCurrentATAStateAsync(string sessionId)
    {
        var session = await _context.Sessions.FindAsync(sessionId);
        if (session?.CurrentMode != SessionMode.ATA || !session.CurrentQuestionId.HasValue)
            return null;
        
        // Calculate if voting is still active (60 second window)
        var elapsed = session.QuestionStartTime.HasValue
            ? (DateTime.UtcNow - session.QuestionStartTime.Value).TotalSeconds
            : 0;
        var isActive = elapsed < 60;
        
        // Get current voting percentages
        var percentages = await CalculateATAPercentagesAsync(sessionId);
        var totalVotes = await GetATAVoteCountAsync(sessionId);
        
        var questionText = session.CurrentQuestionText ?? "Ask The Audience Question";
        
        return new ATAQuestionState
        {
            QuestionId = session.CurrentQuestionId.Value,
            QuestionText = questionText,
            StartTime = session.QuestionStartTime ?? DateTime.UtcNow,
            CurrentResults = percentages,
            TotalVotes = totalVotes,
            IsActive = isActive
        };
    }
}
