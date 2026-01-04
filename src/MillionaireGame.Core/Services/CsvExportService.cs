using System.Text;
using MillionaireGame.Core.Models.Telemetry;

namespace MillionaireGame.Core.Services;

/// <summary>
/// Service for exporting telemetry data to CSV format
/// </summary>
public class CsvExportService
{
    /// <summary>
    /// Export game telemetry to CSV file
    /// </summary>
    /// <param name="filePath">Full path where CSV will be saved</param>
    /// <param name="gameData">Game telemetry data to export</param>
    public void ExportToCsv(string filePath, GameTelemetry gameData)
    {
        var sb = new StringBuilder();

        // Game Summary Section
        sb.AppendLine("=== GAME SESSION SUMMARY ===");
        sb.AppendLine($"Session ID,{gameData.SessionId}");
        sb.AppendLine($"Game Start Time,{gameData.GameStartTime:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Game End Time,{gameData.GameEndTime:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Total Duration,{gameData.TotalDuration}");
        sb.AppendLine($"Total Rounds,{gameData.TotalRounds}");
        sb.AppendLine($"Total Winnings Awarded,{gameData.TotalWinningsAwarded}");
        sb.AppendLine($"Total Lifelines Used,{gameData.TotalLifelinesUsed}");
        sb.AppendLine($"Total Questions Answered,{gameData.TotalQuestionsAnswered}");
        sb.AppendLine();

        // Rounds Section
        foreach (var round in gameData.Rounds)
        {
            sb.AppendLine($"=== ROUND {round.RoundNumber} ===");
            sb.AppendLine($"Start Time,{round.StartTime:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"End Time,{round.EndTime:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Duration,{round.Duration}");
            sb.AppendLine($"Outcome,{round.Outcome}");
            sb.AppendLine($"Final Winnings,{round.FinalWinnings}");
            sb.AppendLine($"Final Question Reached,{round.FinalQuestionReached}");
            sb.AppendLine();

            // Participant Statistics
            sb.AppendLine("--- Web Participants ---");
            sb.AppendLine($"Total Participants,{round.TotalParticipants}");
            
            if (round.DeviceTypes.Any())
            {
                sb.AppendLine("Device Type,Count");
                foreach (var kvp in round.DeviceTypes)
                {
                    sb.AppendLine($"{kvp.Key},{kvp.Value}");
                }
            }
            
            if (round.BrowserTypes.Any())
            {
                sb.AppendLine("Browser,Count");
                foreach (var kvp in round.BrowserTypes)
                {
                    sb.AppendLine($"{kvp.Key},{kvp.Value}");
                }
            }
            
            if (round.OSTypes.Any())
            {
                sb.AppendLine("Operating System,Count");
                foreach (var kvp in round.OSTypes)
                {
                    sb.AppendLine($"{kvp.Key},{kvp.Value}");
                }
            }
            sb.AppendLine();

            // FFF Performance
            if (round.FFFPerformance != null)
            {
                sb.AppendLine("--- Fastest Finger First Performance ---");
                sb.AppendLine($"Total Submissions,{round.FFFPerformance.TotalSubmissions}");
                sb.AppendLine($"Correct Submissions,{round.FFFPerformance.CorrectSubmissions}");
                sb.AppendLine($"Incorrect Submissions,{round.FFFPerformance.IncorrectSubmissions}");
                sb.AppendLine($"Winner Name,{round.FFFPerformance.WinnerName}");
                sb.AppendLine($"Winner Time (ms),{round.FFFPerformance.WinnerTimeMs}");
                sb.AppendLine($"Average Response Time (ms),{round.FFFPerformance.AverageResponseTimeMs:F2}");
                sb.AppendLine($"Fastest Response Time (ms),{round.FFFPerformance.FastestResponseTimeMs}");
                sb.AppendLine($"Slowest Response Time (ms),{round.FFFPerformance.SlowestResponseTimeMs}");
                sb.AppendLine();
            }

            // ATA Performance
            if (round.ATAPerformance != null)
            {
                sb.AppendLine("--- Ask the Audience Performance ---");
                sb.AppendLine($"Total Votes Cast,{round.ATAPerformance.TotalVotesCast}");
                sb.AppendLine($"Mode,{round.ATAPerformance.Mode}");
                
                sb.AppendLine("Answer,Vote Count,Percentage");
                sb.AppendLine($"A,{round.ATAPerformance.VotesForA},{round.ATAPerformance.PercentageA:F1}%");
                sb.AppendLine($"B,{round.ATAPerformance.VotesForB},{round.ATAPerformance.PercentageB:F1}%");
                sb.AppendLine($"C,{round.ATAPerformance.VotesForC},{round.ATAPerformance.PercentageC:F1}%");
                sb.AppendLine($"D,{round.ATAPerformance.VotesForD},{round.ATAPerformance.PercentageD:F1}%");
                sb.AppendLine($"Voting Completion Rate,{round.ATAPerformance.VotingCompletionRate:F1}%");
                sb.AppendLine();
            }

            // Lifelines Used
            if (round.LifelinesUsed.Any())
            {
                sb.AppendLine("--- Lifelines Used ---");
                sb.AppendLine("Lifeline,Question Number,Timestamp,Metadata");
                foreach (var lifeline in round.LifelinesUsed)
                {
                    var metadata = string.IsNullOrEmpty(lifeline.Metadata) ? "" : lifeline.Metadata;
                    sb.AppendLine($"{lifeline.LifelineName},{lifeline.QuestionNumber},{lifeline.Timestamp:yyyy-MM-dd HH:mm:ss},{metadata}");
                }
                sb.AppendLine();
            }
        }

        // Write to file
        File.WriteAllText(filePath, sb.ToString());
    }

    /// <summary>
    /// Generate a default filename for CSV export
    /// </summary>
    public string GenerateDefaultFilename()
    {
        return $"MillionaireGame_Telemetry_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
    }

    /// <summary>
    /// Get the default export directory (Logs folder next to executable)
    /// </summary>
    public string GetDefaultExportDirectory()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var logsDir = Path.Combine(baseDir, "Logs");
        
        if (!Directory.Exists(logsDir))
        {
            Directory.CreateDirectory(logsDir);
        }

        return logsDir;
    }

    /// <summary>
    /// Export game telemetry with default filename and location
    /// </summary>
    public string ExportWithDefaults(GameTelemetry gameData)
    {
        var directory = GetDefaultExportDirectory();
        var filename = GenerateDefaultFilename();
        var fullPath = Path.Combine(directory, filename);

        ExportToCsv(fullPath, gameData);

        return fullPath;
    }
}
