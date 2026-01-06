using ClosedXML.Excel;
using MillionaireGame.Core.Models.Telemetry;

namespace MillionaireGame.Core.Services;

/// <summary>
/// Service for exporting telemetry data to Excel XLSX format
/// </summary>
public class TelemetryExportService
{
    /// <summary>
    /// Export game telemetry to XLSX file
    /// </summary>
    /// <param name="filePath">Full path where XLSX will be saved</param>
    /// <param name="gameData">Game telemetry data to export</param>
    public void ExportToExcel(string filePath, GameTelemetry gameData)
    {
        using var workbook = new XLWorkbook();

        // Create Summary Sheet
        CreateSummarySheet(workbook, gameData);

        // Create a sheet for each round
        for (int i = 0; i < gameData.Rounds.Count; i++)
        {
            CreateRoundSheet(workbook, gameData.Rounds[i], i + 1, gameData);
        }

        workbook.SaveAs(filePath);
    }

    private void CreateSummarySheet(XLWorkbook workbook, GameTelemetry gameData)
    {
        var ws = workbook.Worksheets.Add("Game Summary");
        int row = 1;

        // Title
        ws.Cell(row, 1).Value = "GAME SESSION SUMMARY";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Font.FontSize = 14;
        row += 2;

        // Session Information
        AddTextRow(ws, ref row, "Session ID", gameData.SessionId);
        AddTextRow(ws, ref row, "Game Start Time", gameData.GameStartTime.ToString("yyyy-MM-dd HH:mm:ss"));
        AddTextRow(ws, ref row, "Game End Time", gameData.GameEndTime.ToString("yyyy-MM-dd HH:mm:ss"));
        AddTextRow(ws, ref row, "Total Duration", gameData.TotalDuration.ToString());
        AddNumberRow(ws, ref row, "Total Rounds", gameData.TotalRounds);
        AddNumberRow(ws, ref row, "Total Lifelines Used", gameData.TotalLifelinesUsed);
        AddNumberRow(ws, ref row, "Total Questions Answered", gameData.Rounds.Sum(r => r.FinalQuestionReached));
        row++;

        // Currency Breakdown
        ws.Cell(row, 1).Value = "CURRENCY BREAKDOWN";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
        row++;
        
        AddNumberRow(ws, ref row, $"Currency 1 Total ({gameData.Currency1Name})", gameData.Currency1TotalWinnings);
        
        if (!string.IsNullOrEmpty(gameData.Currency2Name))
        {
            AddNumberRow(ws, ref row, $"Currency 2 Total ({gameData.Currency2Name})", gameData.Currency2TotalWinnings);
        }

        // Auto-fit columns
        ws.Columns().AdjustToContents();
    }

    private void CreateRoundSheet(XLWorkbook workbook, RoundTelemetry round, int roundNumber, GameTelemetry gameData)
    {
        var ws = workbook.Worksheets.Add($"Round {roundNumber}");
        int row = 1;

        // Round Header
        ws.Cell(row, 1).Value = $"ROUND {roundNumber}";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Font.FontSize = 14;
        row += 2;

        // Round Summary
        AddTextRow(ws, ref row, "Start Time", round.StartTime.ToString("yyyy-MM-dd HH:mm:ss"));
        AddTextRow(ws, ref row, "End Time", round.EndTime.ToString("yyyy-MM-dd HH:mm:ss"));
        AddTextRow(ws, ref row, "Duration", round.Duration.ToString());
        AddTextRow(ws, ref row, "Outcome", round.Outcome);
        AddNumberRow(ws, ref row, "Final Question Reached", round.FinalQuestionReached);
        row++;
        
        // Currency Breakdown for this round
        ws.Cell(row, 1).Value = "WINNINGS BY CURRENCY";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
        row++;
        
        AddNumberRow(ws, ref row, $"Currency 1 ({gameData.Currency1Name})", round.Currency1Winnings);
        
        if (!string.IsNullOrEmpty(gameData.Currency2Name))
        {
            AddNumberRow(ws, ref row, $"Currency 2 ({gameData.Currency2Name})", round.Currency2Winnings);
        }
        row++;

        // Web Participants Section
        ws.Cell(row, 1).Value = "WEB PARTICIPANTS";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
        row++;
        AddNumberRow(ws, ref row, "Total Participants", round.TotalParticipants);
        row++;

        // Device Types
        if (round.DeviceTypes.Any())
        {
            ws.Cell(row, 1).Value = "Device Type";
            ws.Cell(row, 2).Value = "Count";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 2).Style.Font.Bold = true;
            row++;
            foreach (var kvp in round.DeviceTypes)
            {
                ws.Cell(row, 1).Value = kvp.Key;
                ws.Cell(row, 2).Value = kvp.Value;
                row++;
            }
            row++;
        }

        // Browser Types
        if (round.BrowserTypes.Any())
        {
            ws.Cell(row, 1).Value = "Browser";
            ws.Cell(row, 2).Value = "Count";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 2).Style.Font.Bold = true;
            row++;
            foreach (var kvp in round.BrowserTypes)
            {
                ws.Cell(row, 1).Value = kvp.Key;
                ws.Cell(row, 2).Value = kvp.Value;
                row++;
            }
            row++;
        }

        // OS Types
        if (round.OSTypes.Any())
        {
            ws.Cell(row, 1).Value = "Operating System";
            ws.Cell(row, 2).Value = "Count";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 2).Style.Font.Bold = true;
            row++;
            foreach (var kvp in round.OSTypes)
            {
                ws.Cell(row, 1).Value = kvp.Key;
                ws.Cell(row, 2).Value = kvp.Value;
                row++;
            }
            row++;
        }

        // FFF Performance
        if (round.FFFPerformance != null)
        {
            ws.Cell(row, 1).Value = "FASTEST FINGER FIRST PERFORMANCE";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
            row++;
            AddNumberRow(ws, ref row, "Total Submissions", round.FFFPerformance.TotalSubmissions);
            AddNumberRow(ws, ref row, "Correct Submissions", round.FFFPerformance.CorrectSubmissions);
            AddNumberRow(ws, ref row, "Incorrect Submissions", round.FFFPerformance.IncorrectSubmissions);
            AddTextRow(ws, ref row, "Winner Name", round.FFFPerformance.WinnerName);
            AddNumberRow(ws, ref row, "Winner Time (ms)", round.FFFPerformance.WinnerTimeMs);
            AddNumberRow(ws, ref row, "Average Response Time (ms)", Math.Round(round.FFFPerformance.AverageResponseTimeMs, 2));
            AddNumberRow(ws, ref row, "Fastest Response Time (ms)", round.FFFPerformance.FastestResponseTimeMs);
            AddNumberRow(ws, ref row, "Slowest Response Time (ms)", round.FFFPerformance.SlowestResponseTimeMs);
            row++;
        }

        // ATA Performance
        if (round.ATAPerformance != null)
        {
            ws.Cell(row, 1).Value = "ASK THE AUDIENCE PERFORMANCE";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightGreen;
            row++;
            AddNumberRow(ws, ref row, "Total Votes Cast", round.ATAPerformance.TotalVotesCast);
            AddTextRow(ws, ref row, "Mode", round.ATAPerformance.Mode);
            row++;

            // Votes table
            ws.Cell(row, 1).Value = "Answer";
            ws.Cell(row, 2).Value = "Vote Count";
            ws.Cell(row, 3).Value = "Percentage";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 2).Style.Font.Bold = true;
            ws.Cell(row, 3).Style.Font.Bold = true;
            row++;

            AddVoteRow(ws, ref row, "A", round.ATAPerformance.VotesForA, round.ATAPerformance.PercentageA);
            AddVoteRow(ws, ref row, "B", round.ATAPerformance.VotesForB, round.ATAPerformance.PercentageB);
            AddVoteRow(ws, ref row, "C", round.ATAPerformance.VotesForC, round.ATAPerformance.PercentageC);
            AddVoteRow(ws, ref row, "D", round.ATAPerformance.VotesForD, round.ATAPerformance.PercentageD);
            row++;

            AddNumberRow(ws, ref row, "Voting Completion Rate (%)", Math.Round(round.ATAPerformance.VotingCompletionRate, 1));
            row++;
        }

        // Lifelines Used
        if (round.LifelinesUsed.Any())
        {
            ws.Cell(row, 1).Value = "LIFELINES USED";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightYellow;
            row++;

            ws.Cell(row, 1).Value = "Lifeline";
            ws.Cell(row, 2).Value = "Question Number";
            ws.Cell(row, 3).Value = "Timestamp";
            ws.Cell(row, 4).Value = "Metadata";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 2).Style.Font.Bold = true;
            ws.Cell(row, 3).Style.Font.Bold = true;
            ws.Cell(row, 4).Style.Font.Bold = true;
            row++;

            foreach (var lifeline in round.LifelinesUsed)
            {
                ws.Cell(row, 1).Value = lifeline.LifelineName;
                ws.Cell(row, 2).Value = lifeline.QuestionNumber;
                ws.Cell(row, 3).Value = lifeline.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
                ws.Cell(row, 4).Value = lifeline.Metadata ?? "";
                row++;
            }
        }

        // Auto-fit columns
        ws.Columns().AdjustToContents();
    }

    private void AddTextRow(IXLWorksheet ws, ref int row, string label, object value)
    {
        ws.Cell(row, 1).Value = label;
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 2).Value = value?.ToString() ?? "";
        row++;
    }

    private void AddNumberRow(IXLWorksheet ws, ref int row, string label, object value)
    {
        ws.Cell(row, 1).Value = label;
        ws.Cell(row, 1).Style.Font.Bold = true;
        
        // Try to parse as number
        if (value != null)
        {
            if (double.TryParse(value.ToString(), out double numValue))
            {
                ws.Cell(row, 2).Value = numValue;
                ws.Cell(row, 2).Style.NumberFormat.Format = "#,##0";
            }
            else
            {
                ws.Cell(row, 2).Value = value.ToString();
            }
        }
        row++;
    }

    private void AddVoteRow(IXLWorksheet ws, ref int row, string answer, int votes, double percentage)
    {
        ws.Cell(row, 1).Value = answer;
        ws.Cell(row, 2).Value = votes;
        ws.Cell(row, 3).Value = $"{Math.Round(percentage, 1)}%";
        row++;
    }

    /// <summary>
    /// Generate a default filename for Excel export
    /// </summary>
    public string GenerateDefaultFilename()
    {
        return $"MillionaireGame_Telemetry_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
    }

    /// <summary>
    /// Get the default export directory (AppData\Local\TheMillionaireGame\Telemetry)
    /// </summary>
    public string GetDefaultExportDirectory()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataPath, "TheMillionaireGame");
        var telemetryDir = Path.Combine(appFolder, "Telemetry");
        
        if (!Directory.Exists(telemetryDir))
        {
            Directory.CreateDirectory(telemetryDir);
        }

        return telemetryDir;
    }

    /// <summary>
    /// Export game telemetry with default filename and location
    /// </summary>
    public string ExportWithDefaults(GameTelemetry gameData)
    {
        var directory = GetDefaultExportDirectory();
        var filename = GenerateDefaultFilename();
        var fullPath = Path.Combine(directory, filename);

        ExportToExcel(fullPath, gameData);

        return fullPath;
    }
}
