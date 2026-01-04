using MillionaireGame.Web.Models;

namespace MillionaireGame.Web.Services;

/// <summary>
/// Bridge to pass telemetry data from web server to main application
/// Uses static delegates set by main application to receive telemetry callbacks
/// </summary>
public static class TelemetryBridge
{
    /// <summary>
    /// Callback to update participant stats in main app telemetry
    /// </summary>
    public static Action<int, Dictionary<string, int>, Dictionary<string, int>, Dictionary<string, int>>? OnParticipantStats { get; set; }
    
    /// <summary>
    /// Callback to set FFF stats in main app telemetry
    /// </summary>
    public static Action<FFFTelemetryData>? OnFFFStats { get; set; }
    
    /// <summary>
    /// Callback to set ATA stats in main app telemetry
    /// </summary>
    public static Action<ATATelemetryData>? OnATAStats { get; set; }
}

/// <summary>
/// FFF telemetry data transfer object
/// </summary>
public class FFFTelemetryData
{
    public int TotalSubmissions { get; set; }
    public int CorrectSubmissions { get; set; }
    public int IncorrectSubmissions { get; set; }
    public string WinnerName { get; set; } = "None";
    public double WinnerTimeMs { get; set; }
    public double AverageResponseTimeMs { get; set; }
    public double FastestResponseTimeMs { get; set; }
    public double SlowestResponseTimeMs { get; set; }
}

/// <summary>
/// ATA telemetry data transfer object
/// </summary>
public class ATATelemetryData
{
    public int TotalVotesCast { get; set; }
    public int VotesForA { get; set; }
    public int VotesForB { get; set; }
    public int VotesForC { get; set; }
    public int VotesForD { get; set; }
    public double PercentageA { get; set; }
    public double PercentageB { get; set; }
    public double PercentageC { get; set; }
    public double PercentageD { get; set; }
    public double VotingCompletionRate { get; set; }
    public string Mode { get; set; } = "Offline";
}
