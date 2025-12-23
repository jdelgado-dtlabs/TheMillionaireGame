namespace MillionaireGame.Web.Models;

/// <summary>
/// Represents a Fastest Finger First question where contestants must order answers correctly
/// </summary>
public class FFFQuestion
{
    public int Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string AnswerA { get; set; } = string.Empty;
    public string AnswerB { get; set; } = string.Empty;
    public string AnswerC { get; set; } = string.Empty;
    public string AnswerD { get; set; } = string.Empty;
    
    /// <summary>
    /// The correct order of answers (e.g., "ABCD", "BADC", etc.)
    /// </summary>
    public string CorrectOrder { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether this question has been used in the game
    /// </summary>
    public bool Used { get; set; } = false;
}
