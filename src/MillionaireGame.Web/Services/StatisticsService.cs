using System.Text;
using MillionaireGame.Web.Data;
using MillionaireGame.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace MillionaireGame.Web.Services;

/// <summary>
/// Generates statistics and exports for game sessions
/// </summary>
public class StatisticsService
{
    private readonly WAPSDbContext _context;

    public StatisticsService(WAPSDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Generates CSV export of session statistics
    /// </summary>
    public async Task<string> GenerateSessionStatisticsCsvAsync(string sessionId)
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

        var csv = new StringBuilder();
        
        // Header
        csv.AppendLine("=== SESSION SUMMARY ===");
        csv.AppendLine($"Session ID,{session.Id}");
        csv.AppendLine($"Host Name,{session.HostName}");
        csv.AppendLine($"Created At,{session.CreatedAt:yyyy-MM-dd HH:mm:ss UTC}");
        csv.AppendLine($"Started At,{session.StartedAt?.ToString("yyyy-MM-dd HH:mm:ss UTC") ?? "N/A"}");
        csv.AppendLine($"Ended At,{session.EndedAt?.ToString("yyyy-MM-dd HH:mm:ss UTC") ?? "N/A"}");
        
        if (session.StartedAt.HasValue && session.EndedAt.HasValue)
        {
            var duration = session.EndedAt.Value - session.StartedAt.Value;
            csv.AppendLine($"Duration,{duration.TotalMinutes:F2} minutes");
        }
        
        csv.AppendLine($"Final Status,{session.Status}");
        csv.AppendLine($"Total Participants,{session.Participants.Count}");
        csv.AppendLine();

        // Participant Statistics (Names removed for privacy)
        csv.AppendLine("=== PARTICIPANT STATISTICS ===");
        csv.AppendLine("Note: Display names and participant IDs removed for privacy compliance");
        csv.AppendLine();
        
        // Anonymized Telemetry Statistics
        csv.AppendLine("=== DEVICE & USAGE TELEMETRY (ANONYMIZED) ===");
        csv.AppendLine("Device Type,OS Type,OS Version,Browser Type,Browser Version,Play Duration (minutes),Played FFF,Used ATA,Final State");
        
        foreach (var participant in session.Participants.OrderBy(p => p.JoinedAt))
        {
            var playDuration = 0.0;
            if (participant.DisconnectedAt.HasValue)
            {
                playDuration = (participant.DisconnectedAt.Value - participant.JoinedAt).TotalMinutes;
            }
            else if (session.EndedAt.HasValue)
            {
                playDuration = (session.EndedAt.Value - participant.JoinedAt).TotalMinutes;
            }
            
            csv.AppendLine($"{participant.DeviceType ?? "Unknown"}," +
                          $"{participant.OSType ?? "Unknown"}," +
                          $"{participant.OSVersion ?? "Unknown"}," +
                          $"{participant.BrowserType ?? "Unknown"}," +
                          $"{participant.BrowserVersion ?? "Unknown"}," +
                          $"{playDuration:F2}," +
                          $"{participant.HasPlayedFFF}," +
                          $"{participant.HasUsedATA}," +
                          $"{participant.State}");
        }
        csv.AppendLine();

        // FFF Statistics (Anonymized)
        if (session.FFFAnswers.Any())
        {
            csv.AppendLine("=== FASTEST FINGER FIRST STATISTICS (ANONYMIZED) ===");
            csv.AppendLine("Note: Participant names removed for privacy compliance");
            csv.AppendLine("Question ID,Participant Index,Answer,Time Elapsed (ms),Submitted At,Is Correct");
            
            var fffAnswers = session.FFFAnswers
                .OrderBy(a => a.QuestionId)
                .ThenBy(a => a.TimeElapsed);
            
            var participantIndex = 1;
            var participantIndexMap = new Dictionary<string, int>();
            
            foreach (var answer in fffAnswers)
            {
                if (!participantIndexMap.ContainsKey(answer.ParticipantId))
                {
                    participantIndexMap[answer.ParticipantId] = participantIndex++;
                }
                
                csv.AppendLine($"{answer.QuestionId}," +
                              $"Participant {participantIndexMap[answer.ParticipantId]}," +
                              $"\"{answer.AnswerSequence}\"," +
                              $"{answer.TimeElapsed}," +
                              $"{answer.SubmittedAt:yyyy-MM-dd HH:mm:ss UTC}," +
                              $"{answer.IsCorrect}");
            }
            csv.AppendLine();

            // FFF Round Summary
            csv.AppendLine("=== FFF ROUND SUMMARY ===");
            csv.AppendLine("Question ID,Total Submissions,Correct Submissions,Fastest Time (ms),Winner");
            
            var roundSummaries = fffAnswers
                .GroupBy(a => a.QuestionId)
                .Select(g => new
                {
                    QuestionId = g.Key,
                    TotalSubmissions = g.Count(),
                    CorrectSubmissions = g.Count(a => a.IsCorrect),
                    FastestTime = g.Where(a => a.IsCorrect).Any() 
                        ? (int?)g.Where(a => a.IsCorrect).Min(a => a.TimeElapsed) 
                        : null,
                    Winner = g.Where(a => a.IsCorrect)
                              .OrderBy(a => a.TimeElapsed)
                              .Select(a => session.Participants.FirstOrDefault(p => p.Id == a.ParticipantId)?.DisplayName)
                              .FirstOrDefault()
                });

            foreach (var round in roundSummaries)
            {
                csv.AppendLine($"{round.QuestionId}," +
                              $"{round.TotalSubmissions}," +
                              $"{round.CorrectSubmissions}," +
                              $"{round.FastestTime?.ToString() ?? "N/A"}," +
                              $"\"{round.Winner ?? "N/A"}\"");
            }
            csv.AppendLine();
        }

        // ATA Statistics
        if (session.ATAVotes.Any())
        {
            csv.AppendLine("=== ASK THE AUDIENCE STATISTICS ===");
            csv.AppendLine("Question Text,Participant Name,Vote,Voted At");
            
            var ataVotes = session.ATAVotes
                .OrderBy(v => v.QuestionText)
                .ThenBy(v => v.SubmittedAt);
            
            foreach (var vote in ataVotes)
            {
                var participant = session.Participants.FirstOrDefault(p => p.Id == vote.ParticipantId);
                csv.AppendLine($"\"{vote.QuestionText}\"," +
                              $"\"{participant?.DisplayName ?? "Unknown"}\"," +
                              $"\"{vote.SelectedOption}\"," +
                              $"{vote.SubmittedAt:yyyy-MM-dd HH:mm:ss UTC}");
            }
            csv.AppendLine();

            // ATA Vote Tallies
            csv.AppendLine("=== ATA VOTE TALLIES ===");
            csv.AppendLine("Question Text,Option A,Option B,Option C,Option D,Total Votes");
            
            var voteTallies = ataVotes
                .GroupBy(v => v.QuestionText)
                .Select(g => new
                {
                    QuestionText = g.Key,
                    OptionA = g.Count(v => v.SelectedOption == "A"),
                    OptionB = g.Count(v => v.SelectedOption == "B"),
                    OptionC = g.Count(v => v.SelectedOption == "C"),
                    OptionD = g.Count(v => v.SelectedOption == "D"),
                    Total = g.Count()
                });

            foreach (var tally in voteTallies)
            {
                csv.AppendLine($"\"{tally.QuestionText}\"," +
                              $"{tally.OptionA}," +
                              $"{tally.OptionB}," +
                              $"{tally.OptionC}," +
                              $"{tally.OptionD}," +
                              $"{tally.Total}");
            }
            csv.AppendLine();
        }

        // Trend Analysis Data
        csv.AppendLine("=== TREND ANALYSIS ===");
        csv.AppendLine("Metric,Value");
        csv.AppendLine($"Avg FFF Response Time (ms),{(session.FFFAnswers.Any() ? session.FFFAnswers.Average(a => a.TimeElapsed).ToString("F2") : "N/A")}");
        csv.AppendLine($"FFF Participation Rate,{(session.Participants.Any() ? (session.FFFAnswers.Select(a => a.ParticipantId).Distinct().Count() * 100.0 / session.Participants.Count).ToString("F2") : "N/A")}%");
        csv.AppendLine($"ATA Participation Rate,{(session.Participants.Any() ? (session.ATAVotes.Select(v => v.ParticipantId).Distinct().Count() * 100.0 / session.Participants.Count).ToString("F2") : "N/A")}%");
        csv.AppendLine($"Active Participants,{session.Participants.Count(p => p.IsActive)}");
        csv.AppendLine($"Winners,{session.Participants.Count(p => p.State == ParticipantState.Winner)}");
        csv.AppendLine($"Eliminated Players,{session.Participants.Count(p => p.State == ParticipantState.Eliminated)}");
        
        return csv.ToString();
    }

    /// <summary>
    /// Gets summary statistics for a session
    /// </summary>
    public async Task<SessionStatistics> GetSessionStatisticsAsync(string sessionId)
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

        return new SessionStatistics
        {
            SessionId = session.Id,
            TotalParticipants = session.Participants.Count,
            ActiveParticipants = session.Participants.Count(p => p.IsActive),
            FFFRoundsPlayed = session.FFFAnswers.Select(a => a.QuestionId).Distinct().Count(),
            ATARoundsPlayed = session.ATAVotes.Select(v => v.QuestionText).Distinct().Count(),
            AverageFFFResponseTime = session.FFFAnswers.Any() 
                ? session.FFFAnswers.Average(a => a.TimeElapsed) 
                : 0,
            FFFParticipationRate = session.Participants.Any() 
                ? session.FFFAnswers.Select(a => a.ParticipantId).Distinct().Count() * 100.0 / session.Participants.Count
                : 0,
            ATAParticipationRate = session.Participants.Any()
                ? session.ATAVotes.Select(v => v.ParticipantId).Distinct().Count() * 100.0 / session.Participants.Count
                : 0,
            Winners = session.Participants.Count(p => p.State == ParticipantState.Winner),
            Duration = session.StartedAt.HasValue && session.EndedAt.HasValue
                ? session.EndedAt.Value - session.StartedAt.Value
                : null
        };
    }
}

/// <summary>
/// Session statistics summary
/// </summary>
public class SessionStatistics
{
    public string SessionId { get; set; } = string.Empty;
    public int TotalParticipants { get; set; }
    public int ActiveParticipants { get; set; }
    public int FFFRoundsPlayed { get; set; }
    public int ATARoundsPlayed { get; set; }
    public double AverageFFFResponseTime { get; set; }
    public double FFFParticipationRate { get; set; }
    public double ATAParticipationRate { get; set; }
    public int Winners { get; set; }
    public TimeSpan? Duration { get; set; }
}
