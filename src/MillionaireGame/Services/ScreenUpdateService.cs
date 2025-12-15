using MillionaireGame.Core.Models;

namespace MillionaireGame.Services;

/// <summary>
/// Coordinates updates across all game screens (Host, Guest, TV)
/// </summary>
public class ScreenUpdateService
{
    private readonly List<IGameScreen> _registeredScreens = new();

    public event EventHandler<QuestionUpdatedEventArgs>? QuestionUpdated;
    public event EventHandler<AnswerSelectedEventArgs>? AnswerSelected;
    public event EventHandler<AnswerRevealedEventArgs>? AnswerRevealed;
    public event EventHandler<LifelineActivatedEventArgs>? LifelineActivated;
    public event EventHandler<MoneyUpdatedEventArgs>? MoneyUpdated;
    public event EventHandler<EventArgs>? GameReset;

    /// <summary>
    /// Register a screen to receive updates
    /// </summary>
    public void RegisterScreen(IGameScreen screen)
    {
        if (!_registeredScreens.Contains(screen))
        {
            _registeredScreens.Add(screen);
        }
    }

    /// <summary>
    /// Unregister a screen from receiving updates
    /// </summary>
    public void UnregisterScreen(IGameScreen screen)
    {
        _registeredScreens.Remove(screen);
    }

    /// <summary>
    /// Update all screens with a new question
    /// </summary>
    public void UpdateQuestion(Question question)
    {
        var args = new QuestionUpdatedEventArgs(question);
        QuestionUpdated?.Invoke(this, args);

        foreach (var screen in _registeredScreens)
        {
            screen.UpdateQuestion(question);
        }
    }

    /// <summary>
    /// Update all screens with selected answer
    /// </summary>
    public void SelectAnswer(string answer)
    {
        var args = new AnswerSelectedEventArgs(answer);
        AnswerSelected?.Invoke(this, args);

        foreach (var screen in _registeredScreens)
        {
            screen.SelectAnswer(answer);
        }
    }

    /// <summary>
    /// Reveal the answer on all screens
    /// </summary>
    public void RevealAnswer(string selectedAnswer, string correctAnswer, bool isCorrect)
    {
        var args = new AnswerRevealedEventArgs(selectedAnswer, correctAnswer, isCorrect);
        AnswerRevealed?.Invoke(this, args);

        foreach (var screen in _registeredScreens)
        {
            screen.RevealAnswer(selectedAnswer, correctAnswer, isCorrect);
        }
    }

    /// <summary>
    /// Update money displays on all screens
    /// </summary>
    public void UpdateMoney(string current, string correct, string wrong, string drop, string questionsLeft)
    {
        var args = new MoneyUpdatedEventArgs(current, correct, wrong, drop, questionsLeft);
        MoneyUpdated?.Invoke(this, args);

        foreach (var screen in _registeredScreens)
        {
            screen.UpdateMoney(current, correct, wrong, drop, questionsLeft);
        }
    }

    /// <summary>
    /// Activate a lifeline on all screens
    /// </summary>
    public void ActivateLifeline(Lifeline lifeline)
    {
        var args = new LifelineActivatedEventArgs(lifeline);
        LifelineActivated?.Invoke(this, args);

        foreach (var screen in _registeredScreens)
        {
            screen.ActivateLifeline(lifeline);
        }
    }

    /// <summary>
    /// Reset all screens
    /// </summary>
    public void ResetAllScreens()
    {
        GameReset?.Invoke(this, EventArgs.Empty);

        foreach (var screen in _registeredScreens)
        {
            screen.ResetScreen();
        }
    }
}

/// <summary>
/// Interface that all game screens must implement
/// </summary>
public interface IGameScreen
{
    void UpdateQuestion(Question question);
    void SelectAnswer(string answer);
    void RevealAnswer(string selectedAnswer, string correctAnswer, bool isCorrect);
    void UpdateMoney(string current, string correct, string wrong, string drop, string questionsLeft);
    void ActivateLifeline(Lifeline lifeline);
    void ResetScreen();
}

#region Event Args

public class QuestionUpdatedEventArgs : EventArgs
{
    public Question Question { get; }
    public QuestionUpdatedEventArgs(Question question) => Question = question;
}

public class AnswerSelectedEventArgs : EventArgs
{
    public string Answer { get; }
    public AnswerSelectedEventArgs(string answer) => Answer = answer;
}

public class AnswerRevealedEventArgs : EventArgs
{
    public string SelectedAnswer { get; }
    public string CorrectAnswer { get; }
    public bool IsCorrect { get; }

    public AnswerRevealedEventArgs(string selectedAnswer, string correctAnswer, bool isCorrect)
    {
        SelectedAnswer = selectedAnswer;
        CorrectAnswer = correctAnswer;
        IsCorrect = isCorrect;
    }
}

public class MoneyUpdatedEventArgs : EventArgs
{
    public string Current { get; }
    public string Correct { get; }
    public string Wrong { get; }
    public string Drop { get; }
    public string QuestionsLeft { get; }

    public MoneyUpdatedEventArgs(string current, string correct, string wrong, string drop, string questionsLeft)
    {
        Current = current;
        Correct = correct;
        Wrong = wrong;
        Drop = drop;
        QuestionsLeft = questionsLeft;
    }
}

public class LifelineActivatedEventArgs : EventArgs
{
    public Lifeline Lifeline { get; }
    public LifelineActivatedEventArgs(Lifeline lifeline) => Lifeline = lifeline;
}

#endregion
