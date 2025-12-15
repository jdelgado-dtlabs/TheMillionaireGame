namespace MillionaireGame.Core.Models;

/// <summary>
/// Represents a player/contestant in the game
/// </summary>
public class Player
{
    public string Name { get; set; } = string.Empty;
    public int Score { get; set; } = 0;
    public TimeSpan? AnswerTime { get; set; }
    public int SlotNumber { get; set; } = 0;
    public bool IsConnected { get; set; } = false;
    public string IpAddress { get; set; } = string.Empty;
}
