using MillionaireGame.Core.Models;
using MillionaireGame.Forms;
using MillionaireGame.Core.Graphics;

namespace MillionaireGame.Services;

/// <summary>
/// Coordinates updates across all game screens (Host, Guest, TV)
/// </summary>
public class ScreenUpdateService
{
    private readonly List<IGameScreen> _registeredScreens = new();
    private Question? _currentQuestion;

    public event EventHandler<QuestionUpdatedEventArgs>? QuestionUpdated;
    public event EventHandler<AnswerSelectedEventArgs>? AnswerSelected;
    public event EventHandler<AnswerRevealedEventArgs>? AnswerRevealed;
    public event EventHandler<LifelineActivatedEventArgs>? LifelineActivated;
    public event EventHandler<MoneyUpdatedEventArgs>? MoneyUpdated;
    public event EventHandler<EventArgs>? GameReset;
    public event EventHandler<EventArgs>? GeneralUpdate;
    
    public string GetCorrectAnswer() => _currentQuestion?.CorrectAnswer ?? "A";
    public Question? GetCurrentQuestion() => _currentQuestion;

    /// <summary>
    /// Triggers a general update event for cases where multiple screen changes happen rapidly
    /// </summary>
    public void TriggerGeneralUpdate()
    {
        GeneralUpdate?.Invoke(this, EventArgs.Empty);
    }

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
        _currentQuestion = question;
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
    /// Show a specific answer option on all screens (for progressive reveal)
    /// </summary>
    public void ShowAnswer(string answer)
    {
        foreach (var screen in _registeredScreens)
        {
            screen.ShowAnswer(answer);
        }
        GeneralUpdate?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Remove (hide) an answer from all screens - used for Double Dip wrong first attempt
    /// </summary>
    public void RemoveAnswer(string answer)
    {
        foreach (var screen in _registeredScreens)
        {
            screen.RemoveAnswer(answer);
        }
        GeneralUpdate?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Show the correct answer to the host screen only
    /// </summary>
    public void ShowCorrectAnswerToHost(string? correctAnswer)
    {
        foreach (var screen in _registeredScreens)
        {
            screen.ShowCorrectAnswerToHost(correctAnswer);
        }
        GeneralUpdate?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Show PAF timer on all screens
    /// </summary>
    public void ShowPAFTimer(int secondsRemaining, string stage)
    {
        foreach (var screen in _registeredScreens)
        {
            screen.ShowPAFTimer(secondsRemaining, stage);
        }
        GeneralUpdate?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Show ATA timer on all screens
    /// </summary>
    public void ShowATATimer(int secondsRemaining, string stage)
    {
        foreach (var screen in _registeredScreens)
        {
            screen.ShowATATimer(secondsRemaining, stage);
        }
        GeneralUpdate?.Invoke(this, EventArgs.Empty);
    }
    
    /// <summary>
    /// Show ATA voting results on all screens
    /// </summary>
    public void ShowATAResults(Dictionary<string, int> votes)
    {
        foreach (var screen in _registeredScreens)
        {
            screen.ShowATAResults(votes);
        }
        GeneralUpdate?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Hide ATA voting results from all screens
    /// </summary>
    public void HideATAResults()
    {
        foreach (var screen in _registeredScreens)
        {
            screen.HideATAResults();
        }
        GeneralUpdate?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Show or hide the question on all screens
    /// </summary>
    public void ShowQuestion(bool show)
    {
        foreach (var screen in _registeredScreens)
        {
            screen.ShowQuestion(show);
        }
        
        // Manage lifeline icon visibility
        if (show)
        {
            // Show lifeline icons on all screens when question is shown
            ShowLifelineIcons();
        }
        else
        {
            // When question is hidden, only hide icons on TV screen
            // Keep icons visible on Host/Guest screens
            foreach (var screen in _registeredScreens)
            {
                if (screen is TVScreenFormScalable)
                {
                    screen.HideLifelineIcons();
                }
            }
        }
    }

    /// <summary>
    /// Show the current winnings on all screens
    /// </summary>
    public void ShowWinnings(GameState state)
    {
        foreach (var screen in _registeredScreens)
        {
            screen.ShowWinnings(state);
        }
        
        // Hide lifeline icons when winnings are shown
        HideLifelineIcons();
        GeneralUpdate?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Show a specific winnings amount on all screens
    /// </summary>
    public void ShowWinningsAmount(string amount)
    {
        // Safety check
        if (string.IsNullOrEmpty(amount))
        {
            return;
        }
        
        foreach (var screen in _registeredScreens)
        {
            if (screen is TVScreenFormScalable scalableScreen)
            {
                scalableScreen.ShowWinningsAmount(amount);
            }
            else
            {
                // For non-scalable screens, use the existing ShowWinnings with current state
                screen.ShowWinnings(new GameState { CurrentValue = amount });
            }
        }
        
        // Icons stay visible on Host/Guest screens during winnings display
        // TV screen handles hiding icons internally when showing winnings
        GeneralUpdate?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Hide the winnings on all screens
    /// </summary>
    public void HideWinnings()
    {
        foreach (var screen in _registeredScreens)
        {
            screen.HideWinnings();
        }
        GeneralUpdate?.Invoke(this, EventArgs.Empty);
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
        
        // Clear lifeline icons on full game reset
        ClearLifelineIcons();
    }

    /// <summary>
    /// Clear question and answer text on all screens (for Q6+ lights down)
    /// </summary>
    public void ClearQuestionAndAnswerText()
    {
        foreach (var screen in _registeredScreens)
        {
            screen.ClearQuestionAndAnswerText();
        }
        GeneralUpdate?.Invoke(this, EventArgs.Empty);
    }
    
    /// <summary>
    /// Show lifeline icons on all screens
    /// </summary>
    public void ShowLifelineIcons()
    {
        foreach (var screen in _registeredScreens)
        {
            screen.ShowLifelineIcons();
        }
        GeneralUpdate?.Invoke(this, EventArgs.Empty);
    }
    
    /// <summary>
    /// Hide lifeline icons on all screens
    /// </summary>
    public void HideLifelineIcons()
    {
        foreach (var screen in _registeredScreens)
        {
            screen.HideLifelineIcons();
        }
        GeneralUpdate?.Invoke(this, EventArgs.Empty);
    }
    
    /// <summary>
    /// Set a lifeline icon state on all screens
    /// </summary>
    public void SetLifelineIcon(int lifelineNumber, LifelineType type, LifelineIconState state)
    {
        foreach (var screen in _registeredScreens)
        {
            screen.SetLifelineIcon(lifelineNumber, type, state);
        }
        GeneralUpdate?.Invoke(this, EventArgs.Empty);
    }
    
    /// <summary>
    /// Clear all lifeline icons on all screens
    /// </summary>
    public void ClearLifelineIcons()
    {
        foreach (var screen in _registeredScreens)
        {
            screen.ClearLifelineIcons();
        }
        GeneralUpdate?.Invoke(this, EventArgs.Empty);
    }
    
    #region FFF Display Methods
    
    /// <summary>
    /// Show a single FFF contestant on TV screen
    /// </summary>
    public void ShowFFFContestant(int index, string name)
    {
        foreach (var screen in _registeredScreens)
        {
            screen.ShowFFFContestant(index, name);
        }
    }
    
    /// <summary>
    /// Show all FFF contestants with straps on TV screen
    /// </summary>
    public void ShowAllFFFContestants(List<string> names, List<double>? times = null)
    {
        foreach (var screen in _registeredScreens)
        {
            screen.ShowAllFFFContestants(names, times);
        }
    }
    
    /// <summary>
    /// Highlight a specific FFF contestant during animation
    /// </summary>
    public void HighlightFFFContestant(int index, bool isWinner = false)
    {
        foreach (var screen in _registeredScreens)
        {
            screen.HighlightFFFContestant(index, isWinner);
        }
    }
    
    /// <summary>
    /// Show the FFF winner
    /// </summary>
    public void ShowFFFWinner(string name, double? time = null)
    {
        foreach (var screen in _registeredScreens)
        {
            screen.ShowFFFWinner(name, time);
        }
    }
    
    /// <summary>
    /// Clear FFF display from all screens
    /// </summary>
    public void ClearFFFDisplay()
    {
        foreach (var screen in _registeredScreens)
        {
            screen.ClearFFFDisplay();
        }
    }
    
    /// <summary>
    /// Show game winner display (Thanks for Playing)
    /// </summary>
    public void ShowGameWinner(string amount, int questionLevel)
    {
        foreach (var screen in _registeredScreens)
        {
            screen.ShowGameWinner(amount, questionLevel);
        }
    }
    
    /// <summary>
    /// Clear game winner display
    /// </summary>
    public void ClearGameWinnerDisplay()
    {
        foreach (var screen in _registeredScreens)
        {
            screen.ClearFFFDisplay();
        }
    }
    
    #endregion
}

/// <summary>
/// Interface that all game screens must implement
/// </summary>
public interface IGameScreen
{
    /// <summary>
    /// Gets whether this screen is a preview instance (part of the preview window).
    /// Preview screens should skip intensive animations like confetti.
    /// </summary>
    bool IsPreview { get; }
    
    void UpdateQuestion(Question question);
    void SelectAnswer(string answer);
    void RevealAnswer(string selectedAnswer, string correctAnswer, bool isCorrect);
    void ShowAnswer(string answer);
    void RemoveAnswer(string answer);
    void ShowCorrectAnswerToHost(string? correctAnswer);
    void ShowQuestion(bool show);
    void ShowWinnings(GameState state);
    void HideWinnings();
    void UpdateMoney(string current, string correct, string wrong, string drop, string questionsLeft);
    void ActivateLifeline(Lifeline lifeline);
    void ResetScreen();
    void ClearQuestionAndAnswerText();
    void ShowPAFTimer(int secondsRemaining, string stage);
    void ShowATATimer(int secondsRemaining, string stage);
    void ShowATAResults(Dictionary<string, int> votes);
    void HideATAResults();
    void ShowLifelineIcons();
    void HideLifelineIcons();
    void SetLifelineIcon(int lifelineNumber, LifelineType type, LifelineIconState state);
    void ClearLifelineIcons();
    
    // FFF display methods
    void ShowFFFContestant(int index, string name);
    void ShowAllFFFContestants(List<string> names, List<double>? times = null);
    void HighlightFFFContestant(int index, bool isWinner = false);
    void ShowFFFWinner(string name, double? time = null);
    void ClearFFFDisplay();
    
    // Game winner display methods
    void ShowGameWinner(string amount, int questionLevel);
    void ClearGameWinnerDisplay();
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
