using Microsoft.AspNetCore.Mvc;
using MillionaireGame.Web.Services;
using MillionaireGame.Web.Models;

namespace MillionaireGame.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SessionController : ControllerBase
{
    private readonly SessionService _sessionService;
    private readonly ILogger<SessionController> _logger;

    public SessionController(SessionService sessionService, ILogger<SessionController> logger)
    {
        _sessionService = sessionService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new session
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest request)
    {
        var session = await _sessionService.CreateSessionAsync(request.HostName);
        
        var joinUrl = $"{Request.Scheme}://{Request.Host}/join?session={session.Id}";
        
        return Ok(new
        {
            SessionId = session.Id,
            HostName = session.HostName,
            CreatedAt = session.CreatedAt,
            JoinUrl = joinUrl,
            Status = session.Status.ToString()
        });
    }

    /// <summary>
    /// Get session details
    /// </summary>
    [HttpGet("{sessionId}")]
    public async Task<IActionResult> GetSession(string sessionId)
    {
        var session = await _sessionService.GetSessionAsync(sessionId);
        if (session == null)
        {
            return NotFound(new { Message = "Session not found" });
        }

        return Ok(new
        {
            SessionId = session.Id,
            HostName = session.HostName,
            CreatedAt = session.CreatedAt,
            StartedAt = session.StartedAt,
            EndedAt = session.EndedAt,
            Status = session.Status.ToString(),
            CurrentMode = session.CurrentMode?.ToString(),
            ParticipantCount = session.Participants.Count(p => p.IsActive)
        });
    }

    /// <summary>
    /// Get active participants in a session
    /// </summary>
    [HttpGet("{sessionId}/participants")]
    public async Task<IActionResult> GetParticipants(string sessionId)
    {
        var participants = await _sessionService.GetActiveParticipantsAsync(sessionId);
        
        return Ok(participants.Select(p => new
        {
            p.Id,
            p.DisplayName,
            p.JoinedAt,
            p.LastSeenAt
        }));
    }

    /// <summary>
    /// Generate QR code for session join
    /// </summary>
    [HttpGet("{sessionId}/qr")]
    public IActionResult GetQRCode(string sessionId)
    {
        var joinUrl = $"{Request.Scheme}://{Request.Host}/join?session={sessionId}";
        var qrCodeBytes = _sessionService.GenerateQRCode(joinUrl);
        
        return File(qrCodeBytes, "image/png");
    }

    /// <summary>
    /// Start a session
    /// </summary>
    [HttpPost("{sessionId}/start")]
    public async Task<IActionResult> StartSession(string sessionId)
    {
        await _sessionService.StartSessionAsync(sessionId);
        return Ok(new { Message = "Session started" });
    }

    /// <summary>
    /// End a session
    /// </summary>
    [HttpPost("{sessionId}/end")]
    public async Task<IActionResult> EndSession(string sessionId)
    {
        await _sessionService.EndSessionAsync(sessionId);
        return Ok(new { Message = "Session ended" });
    }
}

public record CreateSessionRequest(string HostName);
