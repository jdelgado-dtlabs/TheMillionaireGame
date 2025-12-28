namespace MillionaireGame.Core.Models;

/// <summary>
/// Represents a single game question
/// </summary>
public class Question
{
    public int Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string AnswerA { get; set; } = string.Empty;
    public string AnswerB { get; set; } = string.Empty;
    public string AnswerC { get; set; } = string.Empty;
    public string AnswerD { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public DifficultyType DifficultyType { get; set; }
    public int Level { get; set; }
    public LevelRange? LevelRange { get; set; }
    public string Note { get; set; } = string.Empty;
    public bool Used { get; set; } = false;
    public string Explanation { get; set; } = string.Empty;

    // Ask the Audience percentages (optional, can be customized per question)
    public int? ATAPercentageA { get; set; }
    public int? ATAPercentageB { get; set; }
    public int? ATAPercentageC { get; set; }
    public int? ATAPercentageD { get; set; }

    // Custom answer labels (for FFF reveal - shows correct order labels)
    // If null, defaults to "A", "B", "C", "D"
    public string? AnswerALabel { get; set; }
    public string? AnswerBLabel { get; set; }
    public string? AnswerCLabel { get; set; }
    public string? AnswerDLabel { get; set; }

    // Compatibility properties for numbered answer format (Answer1-4)
    // TODO: Migrate database to use Answer1-4 column names to support random answer ordering
    // This will prevent players from memorizing answer positions
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
}

/// <summary>
/// Type of difficulty setting for a question
/// </summary>
public enum DifficultyType
{
    Specific = 0,
    Range = 1
}

/// <summary>
/// Level range categories for questions
/// </summary>
public enum LevelRange
{
    Level1 = 0,  // Questions 1-5
    Level2 = 1,  // Questions 6-10
    Level3 = 2,  // Questions 11-14
    Level4 = 3   // Question 15
}
