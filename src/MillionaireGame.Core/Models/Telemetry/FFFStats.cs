namespace MillionaireGame.Core.Models.Telemetry;

/// <summary>
/// Statistics for a Fastest Finger First (FFF) round
/// </summary>
public class FFFStats
{
    /// <summary>
    /// Total number of participants who submitted answers
    /// </summary>
    public int TotalSubmissions { get; set; }

    /// <summary>
    /// Number of correct answers submitted
    /// </summary>
    public int CorrectSubmissions { get; set; }

    /// <summary>
    /// Number of incorrect answers submitted
    /// </summary>
    public int IncorrectSubmissions { get; set; }

    /// <summary>
    /// Name of the winner (fastest correct answer)
    /// </summary>
    public string WinnerName { get; set; } = "None";

    /// <summary>
    /// Winner's response time in milliseconds
    /// </summary>
    public double WinnerTimeMs { get; set; }

    /// <summary>
    /// Average response time across all submissions in milliseconds
    /// </summary>
    public double AverageResponseTimeMs { get; set; }
    
    /// <summary>
    /// Fastest response time in milliseconds
    /// </summary>
    public double FastestResponseTimeMs { get; set; }
    
    /// <summary>
    /// Slowest response time in milliseconds
    /// </summary>
    public double SlowestResponseTimeMs { get; set; }
}
