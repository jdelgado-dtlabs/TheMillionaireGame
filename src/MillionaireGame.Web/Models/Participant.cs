namespace MillionaireGame.Web.Models;

/// <summary>
/// Represents a participant in a session
/// </summary>
public class Participant
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string SessionId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public string? ConnectionId { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public bool IsActive { get; set; } = true;
    public ParticipantState State { get; set; } = ParticipantState.Lobby;
    public bool HasPlayedFFF { get; set; } = false;
    public bool HasUsedATA { get; set; } = false;
    public DateTime? SelectedForFFFAt { get; set; }
    public DateTime? BecameWinnerAt { get; set; }
    
    // Device Telemetry (for statistics, non-identifying)
    public string? DeviceType { get; set; }  // e.g., "Mobile", "Desktop", "Tablet"
    public string? OSType { get; set; }       // e.g., "iOS", "Android", "Windows", "macOS"
    public string? OSVersion { get; set; }    // e.g., "17.1", "14", "11"
    public string? BrowserType { get; set; }  // e.g., "Chrome", "Safari", "Edge"
    public string? BrowserVersion { get; set; } // e.g., "120.0", "17.2"
    public DateTime? DisconnectedAt { get; set; } // For calculating play duration
    public bool HasAgreedToPrivacy { get; set; } = false;
    
    // Navigation property
    public virtual Session? Session { get; set; }
}

/// <summary>
/// Participant state in game flow
/// </summary>
public enum ParticipantState
{
    /// <summary>Waiting in lobby for selection</summary>
    Lobby,
    
    /// <summary>Selected to play FFF round</summary>
    SelectedForFFF,
    
    /// <summary>Currently playing FFF question</summary>
    PlayingFFF,
    
    /// <summary>Has completed FFF, can only participate in ATA now</summary>
    HasPlayedFFF,
    
    /// <summary>Won FFF and is the main game player</summary>
    Winner,
    
    /// <summary>Eliminated from FFF, returned to lobby</summary>
    Eliminated,
    
    /// <summary>Disconnected from session</summary>
    Disconnected
}
