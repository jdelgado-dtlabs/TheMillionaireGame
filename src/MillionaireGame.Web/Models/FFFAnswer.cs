namespace MillionaireGame.Web.Models;

/// <summary>
/// Represents a submitted FFF answer
/// </summary>
public class FFFAnswer
{
    public int Id { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string ParticipantId { get; set; } = string.Empty;
    public int QuestionId { get; set; }
    public string AnswerSequence { get; set; } = string.Empty; // e.g., "C,A,D,B"
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public double TimeElapsed { get; set; } // Milliseconds from question start
    public bool IsCorrect { get; set; }
    public int? Rank { get; set; } // NULL until evaluated
    
    // Link to game telemetry session
    public string? GameSessionId { get; set; }
    
    // Navigation properties
    public virtual Session? Session { get; set; }
    public virtual Participant? Participant { get; set; }
}
