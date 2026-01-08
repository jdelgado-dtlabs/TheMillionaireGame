namespace MillionaireGame.Web.Models;

/// <summary>
/// Represents the current state of an active FFF question
/// </summary>
public class FFFQuestionState
{
    /// <summary>
    /// Question ID
    /// </summary>
    public int QuestionId { get; set; }
    
    /// <summary>
    /// The question text
    /// </summary>
    public string QuestionText { get; set; } = string.Empty;
    
    /// <summary>
    /// Answer options (4 items: A, B, C, D)
    /// </summary>
    public string[] Options { get; set; } = Array.Empty<string>();
    
    /// <summary>
    /// When the question timer started
    /// </summary>
    public DateTime StartTime { get; set; }
    
    /// <summary>
    /// Time limit in milliseconds (default 20000ms = 20 seconds)
    /// </summary>
    public int TimeLimit { get; set; }
    
    /// <summary>
    /// Whether the timer is still running
    /// </summary>
    public bool IsActive { get; set; }
}

/// <summary>
/// Represents the current state of an active ATA voting question
/// </summary>
public class ATAQuestionState
{
    /// <summary>
    /// Question ID
    /// </summary>
    public int QuestionId { get; set; }
    
    /// <summary>
    /// The question text being voted on
    /// </summary>
    public string QuestionText { get; set; } = string.Empty;
    
    /// <summary>
    /// When the voting started
    /// </summary>
    public DateTime StartTime { get; set; }
    
    /// <summary>
    /// Current voting percentages by option (A, B, C, D)
    /// </summary>
    public Dictionary<string, double> CurrentResults { get; set; } = new();
    
    /// <summary>
    /// Total number of votes cast
    /// </summary>
    public int TotalVotes { get; set; }
    
    /// <summary>
    /// Whether voting is still open (60 second window)
    /// </summary>
    public bool IsActive { get; set; }
}

/// <summary>
/// Represents a participant's eligibility to participate in current game phase
/// </summary>
public class ParticipantEligibility
{
    /// <summary>
    /// Can submit FFF answer
    /// </summary>
    public bool CanSubmitAnswer { get; set; }
    
    /// <summary>
    /// Can vote in ATA
    /// </summary>
    public bool CanVote { get; set; }
    
    /// <summary>
    /// Reason why participant cannot participate (if applicable)
    /// </summary>
    public string Reason { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether participant is in spectator mode (viewing only)
    /// </summary>
    public bool IsSpectator { get; set; }
}
