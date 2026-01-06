using Microsoft.AspNetCore.Mvc;
using MillionaireGame.Web.Services;
using MillionaireGame.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace MillionaireGame.Web.Controllers;

/// <summary>
/// Host control API for managing game sessions
/// </summary>
[ApiController]
[Route("api/host")]
public class HostController : ControllerBase
{
    private readonly SessionService _sessionService;
    private readonly StatisticsService _statisticsService;
    private readonly IHubContext<GameHub> _hubContext;
    private readonly ILogger<HostController> _logger;

    public HostController(
        SessionService sessionService,
        StatisticsService statisticsService,
        IHubContext<GameHub> hubContext,
        ILogger<HostController> logger)
    {
        _sessionService = sessionService;
        _statisticsService = statisticsService;
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// Start the game - move from PreGame to Lobby
    /// </summary>
    [HttpPost("session/{sessionId}/start")]
    public async Task<IActionResult> StartGame(string sessionId)
    {
        try
        {
            var success = await _sessionService.StartGameAsync(sessionId);
            if (!success)
            {
                return NotFound(new { error = "Session not found" });
            }

            // Notify all participants
            await _hubContext.Clients.Group(sessionId)
                .SendAsync("GameStarted", new { sessionId, message = "Game is starting! Get ready!" });

            _logger.LogInformation("Game started for session {SessionId}", sessionId);
            return Ok(new { message = "Game started successfully", sessionId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting game for session {SessionId}", sessionId);
            return StatusCode(500, new { error = "Failed to start game" });
        }
    }

    /// <summary>
    /// Select 8 random players for FFF round
    /// </summary>
    [HttpPost("session/{sessionId}/selectFFFPlayers")]
    public async Task<IActionResult> SelectFFFPlayers(string sessionId, [FromQuery] int count = 8)
    {
        try
        {
            // Check if session exists
            var session = await _sessionService.GetSessionAsync(sessionId);
            if (session == null)
            {
                _logger.LogWarning("SelectFFFPlayers: Session {SessionId} not found", sessionId);
                return NotFound(new { error = "Session not found" });
            }
            
            var selected = await _sessionService.SelectFFFPlayersAsync(sessionId, count);
            
            if (!selected.Any())
            {
                _logger.LogWarning("SelectFFFPlayers: No eligible participants for session {SessionId}", sessionId);
                return BadRequest(new { error = "No eligible participants available" });
            }

            // Notify selected participants
            foreach (var participant in selected)
            {
                if (!string.IsNullOrEmpty(participant.ConnectionId))
                {
                    await _hubContext.Clients.Client(participant.ConnectionId)
                        .SendAsync("SelectedForFFF", new 
                        { 
                            participantId = participant.Id,
                            message = "You've been selected for Fastest Finger First!" 
                        });
                }
            }

            // Notify all in session about selection
            await _hubContext.Clients.Group(sessionId)
                .SendAsync("FFFPlayersSelected", new 
                { 
                    count = selected.Count,
                    participants = selected.Select(p => new { p.Id, p.DisplayName })
                });

            _logger.LogInformation("Selected {Count} players for FFF in session {SessionId}", 
                selected.Count, sessionId);

            return Ok(new 
            { 
                message = $"Selected {selected.Count} players for FFF",
                participants = selected.Select(p => new { p.Id, p.DisplayName, p.SelectedForFFFAt })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error selecting FFF players for session {SessionId}", sessionId);
            return StatusCode(500, new { error = "Failed to select FFF players" });
        }
    }

    /// <summary>
    /// Select one random player to be the winner (bypass FFF)
    /// </summary>
    [HttpPost("session/{sessionId}/selectRandomPlayer")]
    public async Task<IActionResult> SelectRandomPlayer(string sessionId)
    {
        try
        {
            var selected = await _sessionService.SelectRandomPlayerAsync(sessionId);
            
            if (selected == null)
            {
                return BadRequest(new { error = "No eligible participants available" });
            }

            // Notify the winner
            if (!string.IsNullOrEmpty(selected.ConnectionId))
            {
                await _hubContext.Clients.Client(selected.ConnectionId)
                    .SendAsync("SelectedAsWinner", new 
                    { 
                        participantId = selected.Id,
                        message = "You've been randomly selected to play!" 
                    });
            }

            // Notify all in session
            await _hubContext.Clients.Group(sessionId)
                .SendAsync("PlayerSelected", new 
                { 
                    participantId = selected.Id,
                    displayName = selected.DisplayName,
                    message = $"{selected.DisplayName} has been selected to play!"
                });

            _logger.LogInformation("Randomly selected participant {ParticipantId} in session {SessionId}", 
                selected.Id, sessionId);

            return Ok(new 
            { 
                message = "Player selected successfully",
                participant = new { selected.Id, selected.DisplayName, selected.BecameWinnerAt }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error selecting random player for session {SessionId}", sessionId);
            return StatusCode(500, new { error = "Failed to select random player" });
        }
    }

    /// <summary>
    /// Return eliminated players to lobby after FFF round
    /// </summary>
    [HttpPost("session/{sessionId}/returnToLobby")]
    public async Task<IActionResult> ReturnEliminatedToLobby(string sessionId)
    {
        try
        {
            var count = await _sessionService.ReturnEliminatedToLobbyAsync(sessionId);

            // Notify all participants
            await _hubContext.Clients.Group(sessionId)
                .SendAsync("PlayersReturnedToLobby", new 
                { 
                    count,
                    message = $"{count} players returned to lobby"
                });

            return Ok(new { message = $"{count} players returned to lobby", count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error returning players to lobby for session {SessionId}", sessionId);
            return StatusCode(500, new { error = "Failed to return players to lobby" });
        }
    }

    /// <summary>
    /// Set FFF winner after fastest finger first round
    /// </summary>
    [HttpPost("session/{sessionId}/setWinner/{participantId}")]
    public async Task<IActionResult> SetFFFWinner(string sessionId, string participantId)
    {
        try
        {
            var success = await _sessionService.SetWinnerAsync(sessionId, participantId);
            
            if (!success)
            {
                return BadRequest(new { error = "Failed to set winner - participant not found or invalid state" });
            }

            // Get the updated participant
            var winner = await _sessionService.GetParticipantAsync(participantId);
            
            if (winner == null)
            {
                return StatusCode(500, new { error = "Winner set but could not retrieve updated data" });
            }

            // Notify the winner
            if (!string.IsNullOrEmpty(winner.ConnectionId))
            {
                await _hubContext.Clients.Client(winner.ConnectionId)
                    .SendAsync("ConfirmedAsWinner", new 
                    { 
                        participantId = winner.Id,
                        message = "You are the winner and will play the main game!" 
                    });
            }

            // Notify all in session
            await _hubContext.Clients.Group(sessionId)
                .SendAsync("WinnerSet", new 
                { 
                    participantId = winner.Id,
                    displayName = winner.DisplayName,
                    becameWinnerAt = winner.BecameWinnerAt,
                    message = $"{winner.DisplayName} has won FFF and will play the main game!"
                });

            _logger.LogInformation("Set participant {ParticipantId} as FFF winner in session {SessionId}", 
                participantId, sessionId);

            return Ok(new 
            { 
                message = "Winner set successfully",
                participant = new 
                { 
                    winner.Id, 
                    winner.DisplayName, 
                    winner.BecameWinnerAt,
                    winner.SelectedForFFFAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting FFF winner for session {SessionId}", sessionId);
            return StatusCode(500, new { error = "Failed to set FFF winner" });
        }
    }

    /// <summary>
    /// Start Ask The Audience round
    /// </summary>
    [HttpPost("session/{sessionId}/ata/start")]
    public async Task<IActionResult> StartATA(string sessionId, [FromBody] ATAStartRequest request)
    {
        try
        {
            var eligibleParticipants = await _sessionService.GetATAEligibleParticipantsAsync(sessionId);
            
            if (!eligibleParticipants.Any())
            {
                return BadRequest(new { error = "No eligible participants for ATA" });
            }

            // Update session mode
            await _sessionService.UpdateSessionModeAsync(sessionId, Models.SessionMode.ATA, request.QuestionId);

            // Notify eligible participants
            await _hubContext.Clients.Group(sessionId)
                .SendAsync("ATAStarted", new 
                { 
                    questionId = request.QuestionId,
                    questionText = request.QuestionText,
                    optionA = request.OptionA,
                    optionB = request.OptionB,
                    optionC = request.OptionC,
                    optionD = request.OptionD,
                    eligibleCount = eligibleParticipants.Count
                });

            _logger.LogInformation("Started ATA for session {SessionId}, question {QuestionId}", 
                sessionId, request.QuestionId);

            return Ok(new 
            { 
                message = "ATA started successfully",
                eligibleParticipants = eligibleParticipants.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting ATA for session {SessionId}", sessionId);
            return StatusCode(500, new { error = "Failed to start ATA" });
        }
    }

    /// <summary>
    /// End game with statistics export
    /// </summary>
    [HttpPost("session/{sessionId}/end")]
    public async Task<IActionResult> EndGame(string sessionId, [FromQuery] bool cleanup = false)
    {
        try
        {
            // Generate and get CSV statistics
            var csvData = await _sessionService.EndGameAsync(sessionId, _statisticsService);

            // Notify all participants
            await _hubContext.Clients.Group(sessionId)
                .SendAsync("GameEnded", new 
                { 
                    sessionId,
                    message = "Game has ended. Thank you for playing!"
                });

            // Optional: Cleanup database after statistics are exported
            if (cleanup)
            {
                await _sessionService.CleanupSessionAsync(sessionId);
                _logger.LogInformation("Cleaned up session {SessionId} after game end", sessionId);
            }

            _logger.LogInformation("Ended game for session {SessionId}", sessionId);

            // Return CSV as downloadable file
            var bytes = System.Text.Encoding.UTF8.GetBytes(csvData);
            return File(bytes, "text/csv", $"game_stats_{sessionId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending game for session {SessionId}", sessionId);
            return StatusCode(500, new { error = "Failed to end game" });
        }
    }

    /// <summary>
    /// Get current session status and participant summary
    /// </summary>
    [HttpGet("session/{sessionId}/status")]
    public async Task<IActionResult> GetSessionStatus(string sessionId)
    {
        try
        {
            var session = await _sessionService.GetSessionAsync(sessionId);
            if (session == null)
            {
                return NotFound(new { error = "Session not found" });
            }

            var lobbyCount = await _sessionService.GetLobbyParticipantsAsync(sessionId);
            var ataEligible = await _sessionService.GetATAEligibleParticipantsAsync(sessionId);
            var stats = await _statisticsService.GetSessionStatisticsAsync(sessionId);

            return Ok(new
            {
                sessionId = session.Id,
                status = session.Status.ToString(),
                currentMode = session.CurrentMode?.ToString(),
                totalParticipants = session.Participants.Count,
                activeParticipants = session.Participants.Count(p => p.IsActive),
                lobbyParticipants = lobbyCount.Count,
                ataEligibleParticipants = ataEligible.Count,
                createdAt = session.CreatedAt,
                startedAt = session.StartedAt,
                statistics = stats
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting status for session {SessionId}", sessionId);
            return StatusCode(500, new { error = "Failed to get session status" });
        }
    }

    /// <summary>
    /// Get lobby participants (for selection UI)
    /// </summary>
    [HttpGet("session/{sessionId}/lobby")]
    public async Task<IActionResult> GetLobbyParticipants(string sessionId)
    {
        try
        {
            var participants = await _sessionService.GetLobbyParticipantsAsync(sessionId);
            
            return Ok(new
            {
                count = participants.Count,
                participants = participants.Select(p => new
                {
                    p.Id,
                    p.DisplayName,
                    p.JoinedAt,
                    p.State,
                    p.HasPlayedFFF,
                    p.HasUsedATA
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting lobby participants for session {SessionId}", sessionId);
            return StatusCode(500, new { error = "Failed to get lobby participants" });
        }
    }
}

/// <summary>
/// Request body for starting ATA
/// </summary>
public class ATAStartRequest
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;
}
