namespace MillionaireGame.Core.Models;

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
    
    // Compatibility properties for old database schema
    public string Answer1
    {
        get => AnswerA;
        set => AnswerA = value;
    }
    
    public string Answer2
    {
        get => AnswerB;
        set => AnswerB = value;
    }
    
    public string Answer3
    {
        get => AnswerC;
        set => AnswerC = value;
    }
    
    public string Answer4
    {
        get => AnswerD;
        set => AnswerD = value;
    }
    
    /// <summary>
    /// Creates a new FFF question
    /// </summary>
    public FFFQuestion()
    {
    }
    
    /// <summary>
    /// Creates a new FFF question with specified values
    /// </summary>
    public FFFQuestion(int id, string questionText, string answerA, string answerB, string answerC, string answerD, string correctOrder)
    {
        Id = id;
        QuestionText = questionText;
        AnswerA = answerA;
        AnswerB = answerB;
        AnswerC = answerC;
        AnswerD = answerD;
        CorrectOrder = correctOrder.ToUpper();
    }
    
    /// <summary>
    /// Validates if a given answer order is correct
    /// </summary>
    public bool IsCorrectAnswer(string playerAnswer)
    {
        return string.Equals(playerAnswer?.ToUpper(), CorrectOrder, StringComparison.OrdinalIgnoreCase);
    }
}
