using Microsoft.EntityFrameworkCore;
using MillionaireGame.Web.Data;
using MillionaireGame.Web.Database;
using MillionaireGame.Web.Models;

namespace MillionaireGame.Web.Services;

/// <summary>
/// Service for managing FFF question lifecycle and answer validation
/// </summary>
public class FFFService
{
    private readonly WAPSDbContext _context;
    private readonly FFFQuestionRepository _fffRepo;
    private readonly ILogger<FFFService> _logger;

    public FFFService(WAPSDbContext context, FFFQuestionRepository fffRepo, ILogger<FFFService> logger)
    {
        _context = context;
        _fffRepo = fffRepo;
        _logger = logger;
    }

    /// <summary>
    /// Get a random unused FFF question
    /// </summary>
    public async Task<FFFQuestion?> GetRandomQuestionAsync()
    {
        return await _fffRepo.GetRandomQuestionAsync();
    }

    /// <summary>
    /// Get specific FFF question by ID
    /// </summary>
    public async Task<FFFQuestion?> GetQuestionByIdAsync(int questionId)
    {
        return await _fffRepo.GetQuestionByIdAsync(questionId);
    }

    /// <summary>
    /// Mark question as used
    /// </summary>
    public async Task MarkQuestionAsUsedAsync(int questionId)
    {
        await _fffRepo.MarkQuestionAsUsedAsync(questionId);
    }

    /// <summary>
    /// Calculate FFF rankings based on correctness and speed
    /// </summary>
    public async Task<FFFResults> CalculateRankingsAsync(string sessionId, int questionId)
    {
        // Get the correct answer
        var question = await _fffRepo.GetQuestionByIdAsync(questionId);
        if (question == null)
        {
            _logger.LogWarning("Question {QuestionId} not found for ranking calculation", questionId);
            return new FFFResults();
        }

        var correctAnswer = question.CorrectOrder;
        _logger.LogInformation("Calculating rankings for question {QuestionId}, correct answer: {CorrectAnswer}", 
            questionId, correctAnswer);

        // Get all submissions for this question
        var submissions = await _context.FFFAnswers
            .Include(a => a.Participant)
            .Where(a => a.SessionId == sessionId && a.QuestionId == questionId)
            .OrderBy(a => a.SubmittedAt)
            .ToListAsync();

        _logger.LogInformation("Found {Count} submissions for question {QuestionId}", submissions.Count, questionId);

        // Validate answers (compare without spaces/case)
        foreach (var submission in submissions)
        {
            var normalizedSubmission = NormalizeAnswer(submission.AnswerSequence);
            var normalizedCorrect = NormalizeAnswer(correctAnswer);
            submission.IsCorrect = normalizedSubmission == normalizedCorrect;
            
            _logger.LogDebug("Participant {ParticipantId} answer: {Answer} (normalized: {Normalized}), correct: {IsCorrect}",
                submission.ParticipantId, submission.AnswerSequence, normalizedSubmission, submission.IsCorrect);
        }

        // Rank correct answers by time
        var correctSubmissions = submissions
            .Where(s => s.IsCorrect)
            .OrderBy(s => s.TimeElapsed)
            .ToList();

        for (int i = 0; i < correctSubmissions.Count; i++)
        {
            correctSubmissions[i].Rank = i + 1;
        }

        // Save all updates
        await _context.SaveChangesAsync();

        _logger.LogInformation("Rankings calculated: {CorrectCount} correct answers out of {TotalCount} submissions",
            correctSubmissions.Count, submissions.Count);
        
        // WebService console logging for contestant selection
        try 
        {
            var winner = correctSubmissions.FirstOrDefault();
            if (winner != null)
            {
                var consoleType = Type.GetType("MillionaireGame.Utilities.WebServerConsole, MillionaireGame");
                if (consoleType != null)
                {
                    var logMethod = consoleType.GetMethod("Log", new[] { typeof(string) });
                    logMethod?.Invoke(null, new object[] { $"Player {winner.ParticipantId} selected for FFF" });
                    logMethod?.Invoke(null, new object[] { $"Player {winner.ParticipantId} is the next contestant" });
                }
            }
        }
        catch { /* WebService console not available - ignore */ }

        // Update telemetry with FFF performance stats
        var fffWinner = correctSubmissions.FirstOrDefault();
        var avgResponseTime = submissions.Any() ? submissions.Average(s => s.TimeElapsed) : 0;
        var fastestTime = correctSubmissions.Any() ? correctSubmissions.Min(s => s.TimeElapsed) : 0;
        var slowestTime = correctSubmissions.Any() ? correctSubmissions.Max(s => s.TimeElapsed) : 0;
        
        var fffData = new FFFTelemetryData
        {
            TotalSubmissions = submissions.Count,
            CorrectSubmissions = correctSubmissions.Count,
            IncorrectSubmissions = submissions.Count - correctSubmissions.Count,
            WinnerName = fffWinner?.Participant?.DisplayName ?? "None",
            WinnerTimeMs = fffWinner?.TimeElapsed ?? 0,
            AverageResponseTimeMs = avgResponseTime,
            FastestResponseTimeMs = fastestTime,
            SlowestResponseTimeMs = slowestTime
        };
        
        TelemetryBridge.OnFFFStats?.Invoke(fffData);
        _logger.LogInformation("[Telemetry] FFF stats recorded: {Total} submissions, {Correct} correct, Winner: {Winner} ({Time}ms)",
            submissions.Count, correctSubmissions.Count, fffData.WinnerName, fffData.WinnerTimeMs);

        return new FFFResults
        {
            Winner = correctSubmissions.FirstOrDefault(),
            Rankings = correctSubmissions.Take(10).ToList(),
            TotalSubmissions = submissions.Count,
            CorrectSubmissions = correctSubmissions.Count
        };
    }

    /// <summary>
    /// Normalize answer string for comparison (remove spaces, uppercase)
    /// </summary>
    private string NormalizeAnswer(string answer)
    {
        return answer.Replace(" ", "").Replace(",", "").ToUpperInvariant();
    }
}

/// <summary>
/// Results from FFF question ranking
/// </summary>
public class FFFResults
{
    public FFFAnswer? Winner { get; set; }
    public List<FFFAnswer> Rankings { get; set; } = new();
    public int TotalSubmissions { get; set; }
    public int CorrectSubmissions { get; set; }
}
