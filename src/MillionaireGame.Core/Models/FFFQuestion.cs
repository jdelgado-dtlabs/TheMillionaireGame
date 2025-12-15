namespace MillionaireGame.Core.Models;

/// <summary>
/// Represents a Fastest Finger First question
/// </summary>
public class FFFQuestion
{
    public int Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string AnswerA { get; set; } = string.Empty;
    public string AnswerB { get; set; } = string.Empty;
    public string AnswerC { get; set; } = string.Empty;
    public string AnswerD { get; set; } = string.Empty;
    public string CorrectOrder { get; set; } = string.Empty; // e.g., "A,C,B,D"
    public int Level { get; set; } = 0;
    public string Note { get; set; } = string.Empty;
    public bool Used { get; set; } = false;
}
