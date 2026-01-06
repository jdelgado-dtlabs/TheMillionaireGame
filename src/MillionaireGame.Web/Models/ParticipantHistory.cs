namespace MillionaireGame.Web.Models;

/// <summary>
/// Historical record of participants for telemetry purposes
/// Preserves participant data after session completion while clearing live Participants table
/// </summary>
public class ParticipantHistory
{
    public int HistoryId { get; set; } // Auto-increment primary key
    public string ParticipantId { get; set; } = string.Empty; // Original participant GUID
    public string SessionId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public DateTime? DisconnectedAt { get; set; }
    public string State { get; set; } = ParticipantState.Lobby.ToString(); // Stored as string for history
    public bool HasPlayedFFF { get; set; } = false;
    public bool HasUsedATA { get; set; } = false;
    public DateTime? SelectedForFFFAt { get; set; }
    public DateTime? BecameWinnerAt { get; set; }
    
    // Device Telemetry (anonymized, non-identifying)
    public string? DeviceType { get; set; }
    public string? OSType { get; set; }
    public string? OSVersion { get; set; }
    public string? BrowserType { get; set; }
    public string? BrowserVersion { get; set; }
    public bool HasAgreedToPrivacy { get; set; } = false;
    
    // Link to game telemetry session
    public string? GameSessionId { get; set; }
    
    // Archive timestamp
    public DateTime ArchivedAt { get; set; } = DateTime.UtcNow;
}
