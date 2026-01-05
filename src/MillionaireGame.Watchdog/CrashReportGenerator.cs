using System.Diagnostics;
using System.Text;

namespace MillionaireGame.Watchdog;

/// <summary>
/// Generates comprehensive crash reports for debugging
/// </summary>
public class CrashReportGenerator
{
    private readonly string _reportDirectory;

    public CrashReportGenerator()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _reportDirectory = Path.Combine(localAppData, "TheMillionaireGame", "CrashReports");
        Directory.CreateDirectory(_reportDirectory);
    }

    public string GenerateReport(CrashInfo crashInfo, string? gameBuildInfo = null)
    {
        var timestamp = crashInfo.CrashTime.ToString("yyyyMMdd_HHmmss");
        var reportPath = Path.Combine(_reportDirectory, $"CrashReport_{timestamp}.txt");

        var report = new StringBuilder();
        report.AppendLine("====================================");
        report.AppendLine("MILLIONAIRE GAME CRASH REPORT");
        report.AppendLine("====================================");
        report.AppendLine($"Crash Time: {crashInfo.CrashTime:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine($"Report Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine();

        // Process Information
        report.AppendLine("[PROCESS INFORMATION]");
        report.AppendLine($"- Process ID: {crashInfo.ProcessId}");
        report.AppendLine($"- Exit Code: {crashInfo.ExitCode} (0x{crashInfo.ExitCode:X8})");
        report.AppendLine($"- Exit Code Meaning: {GetExitCodeMeaning(crashInfo.ExitCode)}");
        report.AppendLine($"- Running Time: {crashInfo.RunningTime:hh\\:mm\\:ss}");
        
        if (crashInfo.LastHeartbeat.HasValue)
        {
            var timeSinceHeartbeat = crashInfo.CrashTime - crashInfo.LastHeartbeat.Value;
            report.AppendLine($"- Last Heartbeat: {crashInfo.LastHeartbeat:yyyy-MM-dd HH:mm:ss} ({timeSinceHeartbeat.TotalSeconds:F1}s ago)");
        }
        else
        {
            report.AppendLine("- Last Heartbeat: None received");
        }
        report.AppendLine($"- Was Responsive: {(crashInfo.WasResponsive ? "Yes" : "No (frozen/hung)")}");
        report.AppendLine();

        // Application State
        report.AppendLine("[APPLICATION STATE]");
        if (!string.IsNullOrEmpty(gameBuildInfo))
        {
            report.AppendLine($"- Build Info: {gameBuildInfo}");
        }
        report.AppendLine($"- Last State: {crashInfo.LastState ?? "Unknown"}");
        report.AppendLine($"- Last Activity: {crashInfo.LastActivity ?? "None"}");
        report.AppendLine();

        // System Information
        report.AppendLine("[SYSTEM INFORMATION]");
        report.AppendLine($"- OS: {Environment.OSVersion.VersionString}");
        report.AppendLine($"- 64-bit OS: {Environment.Is64BitOperatingSystem}");
        report.AppendLine($"- 64-bit Process: {Environment.Is64BitProcess}");
        report.AppendLine($"- Processor Count: {Environment.ProcessorCount}");
        report.AppendLine($"- .NET Runtime: {Environment.Version}");
        report.AppendLine();

        // Resource Usage
        report.AppendLine("[RESOURCE USAGE (Last Known)]");
        report.AppendLine($"- Memory: {crashInfo.LastMemoryMB} MB");
        report.AppendLine($"- Threads: {crashInfo.LastThreadCount}");
        report.AppendLine();

        // GameConsole log
        report.AppendLine("[RECENT LOGS]");
        try
        {
            var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GameConsole.log");
            if (File.Exists(logPath))
            {
                var logLines = File.ReadLines(logPath).Reverse().Take(100).Reverse();
                foreach (var line in logLines)
                {
                    report.AppendLine(line);
                }
            }
            else
            {
                report.AppendLine("(Log file not found)");
            }
        }
        catch (Exception ex)
        {
            report.AppendLine($"(Error reading log: {ex.Message})");
        }
        report.AppendLine();

        report.AppendLine("====================================");
        report.AppendLine("END OF CRASH REPORT");
        report.AppendLine("====================================");

        File.WriteAllText(reportPath, report.ToString());
        
        // Clean up old reports (keep last 10)
        CleanupOldReports();
        
        Console.WriteLine($"[Watchdog] Crash report saved: {reportPath}");
        return reportPath;
    }

    private string GetExitCodeMeaning(int exitCode)
    {
        return exitCode switch
        {
            0 => "Clean exit",
            1 => "User-requested exit",
            -1 => "General error",
            -532462766 => "Stack overflow (0xE0434352)",
            -1073741819 => "Access violation (0xC0000005)",
            -1073740791 => "Stack overflow (0xC00000FD)",
            -1073741510 => "Application hang (0xC000013A)",
            _ => "Unknown error"
        };
    }

    private void CleanupOldReports()
    {
        try
        {
            var reports = Directory.GetFiles(_reportDirectory, "CrashReport_*.txt")
                .OrderByDescending(f => File.GetCreationTime(f))
                .Skip(10)
                .ToList();

            foreach (var report in reports)
            {
                File.Delete(report);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Watchdog] Error cleaning up old reports: {ex.Message}");
        }
    }
}
