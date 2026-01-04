namespace MillionaireGame.Core.Models.Telemetry;

/// <summary>
/// Statistics for an Ask the Audience (ATA) lifeline usage
/// </summary>
public class ATAStats
{
    /// <summary>
    /// Total number of votes cast
    /// </summary>
    public int TotalVotesCast { get; set; }

    /// <summary>
    /// Votes for option A
    /// </summary>
    public int VotesForA { get; set; }

    /// <summary>
    /// Votes for option B
    /// </summary>
    public int VotesForB { get; set; }

    /// <summary>
    /// Votes for option C
    /// </summary>
    public int VotesForC { get; set; }

    /// <summary>
    /// Votes for option D
    /// </summary>
    public int VotesForD { get; set; }

    /// <summary>
    /// Percentage for option A
    /// </summary>
    public double PercentageA { get; set; }

    /// <summary>
    /// Percentage for option B
    /// </summary>
    public double PercentageB { get; set; }

    /// <summary>
    /// Percentage for option C
    /// </summary>
    public double PercentageC { get; set; }

    /// <summary>
    /// Percentage for option D
    /// </summary>
    public double PercentageD { get; set; }

    /// <summary>
    /// Voting completion rate (votes / participants)
    /// </summary>
    public double VotingCompletionRate { get; set; }

    /// <summary>
    /// Mode: "Online" or "Offline"
    /// </summary>
    public string Mode { get; set; } = "Offline";
}
