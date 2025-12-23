using Microsoft.AspNetCore.Mvc;
using MillionaireGame.Web.Services;
using MillionaireGame.Web.Models;

namespace MillionaireGame.Web.Controllers;

[ApiController]
[Route("api/fff")]
public class FFFController : ControllerBase
{
    private readonly FFFService _fffService;
    private readonly SessionService _sessionService;
    private readonly ILogger<FFFController> _logger;

    public FFFController(FFFService fffService, SessionService sessionService, ILogger<FFFController> logger)
    {
        _fffService = fffService;
        _sessionService = sessionService;
        _logger = logger;
    }

    /// <summary>
    /// Get a random unused FFF question
    /// </summary>
    [HttpGet("random")]
    public async Task<IActionResult> GetRandomQuestion()
    {
        var question = await _fffService.GetRandomQuestionAsync();
        
        if (question == null)
        {
            return NotFound(new { message = "No unused questions available" });
        }

        return Ok(new
        {
            id = question.Id,
            question = question.QuestionText,
            options = new
            {
                a = question.AnswerA,
                b = question.AnswerB,
                c = question.AnswerC,
                d = question.AnswerD
            }
        });
    }

    /// <summary>
    /// Get specific FFF question by ID
    /// </summary>
    [HttpGet("{questionId}")]
    public async Task<IActionResult> GetQuestion(int questionId)
    {
        var question = await _fffService.GetQuestionByIdAsync(questionId);
        
        if (question == null)
        {
            return NotFound(new { message = $"Question {questionId} not found" });
        }

        return Ok(new
        {
            id = question.Id,
            question = question.QuestionText,
            options = new
            {
                a = question.AnswerA,
                b = question.AnswerB,
                c = question.AnswerC,
                d = question.AnswerD
            },
            used = question.Used
        });
    }

    /// <summary>
    /// Get leaderboard for a specific question
    /// </summary>
    [HttpGet("sessions/{sessionId}/question/{questionId}/leaderboard")]
    public async Task<IActionResult> GetLeaderboard(string sessionId, int questionId)
    {
        try
        {
            var results = await _fffService.CalculateRankingsAsync(sessionId, questionId);

            return Ok(new
            {
                winner = results.Winner != null ? new
                {
                    participantId = results.Winner.ParticipantId,
                    displayName = results.Winner.Participant?.DisplayName,
                    timeElapsed = results.Winner.TimeElapsed,
                    answerSequence = results.Winner.AnswerSequence,
                    rank = results.Winner.Rank
                } : null,
                rankings = results.Rankings.Select(r => new
                {
                    participantId = r.ParticipantId,
                    displayName = r.Participant?.DisplayName,
                    timeElapsed = r.TimeElapsed,
                    answerSequence = r.AnswerSequence,
                    rank = r.Rank
                }).ToList(),
                totalSubmissions = results.TotalSubmissions,
                correctSubmissions = results.CorrectSubmissions
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating leaderboard for session {SessionId}, question {QuestionId}", 
                sessionId, questionId);
            return StatusCode(500, new { message = "Error calculating leaderboard", error = ex.Message });
        }
    }

    /// <summary>
    /// Get current session status
    /// </summary>
    [HttpGet("sessions/{sessionId}/status")]
    public async Task<IActionResult> GetSessionStatus(string sessionId)
    {
        var session = await _sessionService.GetSessionAsync(sessionId);
        
        if (session == null)
        {
            return NotFound(new { message = $"Session {sessionId} not found" });
        }

        return Ok(new
        {
            sessionId = session.Id,
            status = session.Status.ToString(),
            currentMode = session.CurrentMode?.ToString(),
            participantCount = session.Participants?.Count ?? 0,
            createdAt = session.CreatedAt,
            startedAt = session.StartedAt
        });
    }
}
