namespace MillionaireGame.Web.Models;

/// <summary>
/// Defines the various states of the game for web client synchronization.
/// </summary>
public enum GameStateType
{
    /// <summary>
    /// Initial lobby state - users can verify browser functions
    /// </summary>
    InitialLobby,
    
    /// <summary>
    /// Waiting lobby - game has started, users waiting for next activity
    /// </summary>
    WaitingLobby,
    
    /// <summary>
    /// FFF lobby - "Get ready to play!"
    /// </summary>
    FFFLobby,
    
    /// <summary>
    /// FFF question displayed with answer options
    /// </summary>
    FFFQuestion,
    
    /// <summary>
    /// FFF timer expired with response - "Calculating your response..."
    /// </summary>
    FFFCalculating,
    
    /// <summary>
    /// FFF timer expired without response - "Thanks for participating!"
    /// </summary>
    FFFNoResponse,
    
    /// <summary>
    /// FFF results - showing correct order and user results
    /// </summary>
    FFFResults,
    
    /// <summary>
    /// FFF winner announcement
    /// </summary>
    FFFWinner,
    
    /// <summary>
    /// ATA intro - "Get ready to vote!"
    /// </summary>
    ATAReady,
    
    /// <summary>
    /// ATA voting active - display question and voting buttons
    /// </summary>
    ATAVoting,
    
    /// <summary>
    /// ATA vote submitted - user has voted, waiting for results
    /// </summary>
    ATAVoteSubmitted,
    
    /// <summary>
    /// ATA results - display voting results graph
    /// </summary>
    ATAResults,
    
    /// <summary>
    /// Game complete - thank you message and cleanup
    /// </summary>
    GameComplete
}

/// <summary>
/// Data payload for game state changes.
/// </summary>
public class GameStateData
{
    public GameStateType State { get; set; }
    public string? Message { get; set; }
    public object? Data { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
