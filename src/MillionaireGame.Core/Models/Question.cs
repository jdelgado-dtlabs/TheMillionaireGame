namespace MillionaireGame.Core.Models;

/// <summary>
/// Represents a single game question
/// Level: 1=Easy(Q1-5), 2=Medium(Q6-10), 3=Hard(Q11-14), 4=Million(Q15)
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
    public int Level { get; set; }
    public string Note { get; set; } = string.Empty;
    public bool Used { get; set; } = false;
    public string Explanation { get; set; } = string.Empty;

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

    /// <summary>
    /// Generates random Ask The Audience percentages that favor the correct answer
    /// </summary>
    public Dictionary<string, int> GenerateATAPercentages()
    {
        var random = new Random();
        var percentages = new Dictionary<string, int>();
        
        // Generate random percentages that add up to 100
        // Favor the correct answer with 40-70% of votes
        var correctPercentage = random.Next(40, 71);
        var remaining = 100 - correctPercentage;
        
        // Distribute remaining votes among wrong answers
        var wrongAnswers = new List<string> { "A", "B", "C", "D" }
            .Where(x => x != CorrectAnswer)
            .ToList();
        
        var wrongPercentages = new List<int>();
        for (int i = 0; i < wrongAnswers.Count - 1; i++)
        {
            var max = remaining - (wrongAnswers.Count - 1 - i);
            var value = random.Next(0, Math.Max(1, max));
            wrongPercentages.Add(value);
            remaining -= value;
        }
        wrongPercentages.Add(remaining); // Last answer gets remainder
        
        // Assign percentages
        percentages[CorrectAnswer] = correctPercentage;
        for (int i = 0; i < wrongAnswers.Count; i++)
        {
            percentages[wrongAnswers[i]] = wrongPercentages[i];
        }
        
        return percentages;
    }
}
