namespace MillionaireGame.Web.Models;

/// <summary>
/// Represents an ATA vote
/// </summary>
public class ATAVote
{
    public int Id { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string ParticipantId { get; set; } = string.Empty;
    public string QuestionText { get; set; } = string.Empty;
    public string SelectedOption { get; set; } = string.Empty; // 'A', 'B', 'C', 'D'
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Session? Session { get; set; }
    public virtual Participant? Participant { get; set; }
}
