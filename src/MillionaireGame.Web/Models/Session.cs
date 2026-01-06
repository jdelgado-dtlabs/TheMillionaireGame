namespace MillionaireGame.Web.Models;

/// <summary>
/// Represents a game session
/// </summary>
public class Session
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string HostName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.Waiting;
    public SessionMode? CurrentMode { get; set; }
    public int? CurrentQuestionId { get; set; }
    public DateTime? QuestionStartTime { get; set; } // Timestamp when current question started
    
    // Navigation properties
    public virtual ICollection<Participant> Participants { get; set; } = new List<Participant>();
    public virtual ICollection<FFFAnswer> FFFAnswers { get; set; } = new List<FFFAnswer>();
    public virtual ICollection<ATAVote> ATAVotes { get; set; } = new List<ATAVote>();
}

/// <summary>
/// Session status enum - Enhanced for complete game flow
/// </summary>
public enum SessionStatus
{
    /// <summary>Pre-game: QR code displayed, participants can join</summary>
    PreGame,
    
    /// <summary>Lobby: All participants waiting for host to start</summary>
    Lobby,
    
    /// <summary>Host selecting players for FFF</summary>
    FFFSelection,
    
    /// <summary>FFF round active with question</summary>
    FFFActive,
    
    /// <summary>Main game with winner playing</summary>
    MainGame,
    
    /// <summary>Ask The Audience lifeline active</summary>
    ATAActive,
    
    /// <summary>Game completed, showing final stats</summary>
    GameOver,
    
    /// <summary>Legacy: Waiting state (deprecated)</summary>
    Waiting,
    
    /// <summary>Legacy: Active state (deprecated)</summary>
    Active,
    
    /// <summary>Legacy: Completed state (deprecated)</summary>
    Completed
}

/// <summary>
/// Current session mode
/// </summary>
public enum SessionMode
{
    Idle,
    FFF,
    ATA
}
