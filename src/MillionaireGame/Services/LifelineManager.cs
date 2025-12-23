using MillionaireGame.Core.Game;
using MillionaireGame.Core.Models;
using MillionaireGame.Core.Services;
using System.Drawing;

namespace MillionaireGame.Services;

/// <summary>
/// Manages all lifeline execution logic, state tracking, and multi-stage behavior
/// </summary>
public class LifelineManager
{
    private readonly GameService _gameService;
    private readonly SoundService _soundService;
    private readonly ScreenUpdateService _screenService;
    
    // PAF state tracking
    private PAFStage _pafStage = PAFStage.NotStarted;
    private int _pafLifelineButtonNumber = 0;
    private System.Windows.Forms.Timer? _pafTimer;
    private int _pafSecondsRemaining = 30;
    
    // ATA state tracking
    private ATAStage _ataStage = ATAStage.NotStarted;
    private int _ataLifelineButtonNumber = 0;
    private System.Windows.Forms.Timer? _ataTimer;
    private int _ataSecondsRemaining = 120;
    private string _ataCorrectAnswer = "";
    private Random _random = new Random();
    
    // Double Dip state tracking
    private DoubleDipStage _doubleDipStage = DoubleDipStage.NotStarted;
    private int _doubleDipLifelineButtonNumber = 0;
    private string _doubleDipFirstAnswer = "";
    
    // Ask the Host state tracking
    private ATHStage _athStage = ATHStage.NotStarted;
    private int _athLifelineButtonNumber = 0;
    
    // Events for UI updates
    public event Action<int, Color, bool>? ButtonStateChanged; // buttonNumber, color, enabled
    public event Action<Func<Task>>? RequestAsyncOperation; // For operations that need to await
    public event Action<string, string>? RequestAnswerRemoval; // For 50:50 answer removal
    public event Action<string>? LogMessage; // For debug logging
    public event Action? RequestBedMusicRestart; // For Q1-5 bed music restart after lifeline
    
    public LifelineManager(GameService gameService, SoundService soundService, ScreenUpdateService screenService)
    {
        _gameService = gameService;
        _soundService = soundService;
        _screenService = screenService;
    }
    
    /// <summary>
    /// Execute a lifeline based on its type
    /// </summary>
    public async Task ExecuteLifelineAsync(LifelineType type, int buttonNumber, string correctAnswer)
    {
        var lifeline = _gameService.State.GetLifeline(type);
        if (lifeline == null || lifeline.IsUsed)
        {
            return;
        }
        
        switch (type)
        {
            case LifelineType.FiftyFifty:
                await ExecuteFiftyFiftyAsync(lifeline, buttonNumber, correctAnswer);
                break;
            case LifelineType.PlusOne:
                await ExecutePhoneFriendAsync(lifeline, buttonNumber);
                break;
            case LifelineType.AskTheAudience:
                await ExecuteAskAudienceAsync(lifeline, buttonNumber);
                break;
            case LifelineType.SwitchQuestion:
                await ExecuteSwitchQuestionAsync(lifeline, buttonNumber);
                break;
            case LifelineType.AskTheHost:
                await ExecuteAskTheHostAsync(lifeline, buttonNumber, correctAnswer);
                break;
            case LifelineType.DoubleDip:
                await ExecuteDoubleDipAsync(lifeline, buttonNumber);
                break;
        }
    }
    
    /// <summary>
    /// Handle multi-stage lifeline button clicks (PAF/ATA/DD/ATH)
    /// </summary>
    public void HandleMultiStageClick(LifelineType type, int buttonNumber)
    {
        switch (type)
        {
            case LifelineType.PlusOne when _pafStage != PAFStage.NotStarted:
                HandlePAFStageClick(buttonNumber);
                break;
            case LifelineType.AskTheAudience when _ataStage != ATAStage.NotStarted:
                HandleATAStageClick(buttonNumber);
                break;
            case LifelineType.DoubleDip when _doubleDipStage != DoubleDipStage.NotStarted:
                HandleDoubleDipStageClick(buttonNumber);
                break;
        }
    }
    
    /// <summary>
    /// Check if a lifeline is in a multi-stage state
    /// </summary>
    public bool IsInMultiStageState(LifelineType type)
    {
        return type switch
        {
            LifelineType.PlusOne => _pafStage != PAFStage.NotStarted,
            LifelineType.AskTheAudience => _ataStage != ATAStage.NotStarted,
            LifelineType.DoubleDip => _doubleDipStage != DoubleDipStage.NotStarted,
            _ => false
        };
    }
    
    #region 50:50 Lifeline
    
    private async Task ExecuteFiftyFiftyAsync(Lifeline lifeline, int buttonNumber, string correctAnswer)
    {
        LogMessage?.Invoke("[Lifeline] 50:50 activated");
        
        _gameService.UseLifeline(lifeline.Type);
        ButtonStateChanged?.Invoke(buttonNumber, Color.Gray, false);
        
        // Play lifeline sound without stopping bed music
        _soundService.PlaySound(SoundEffect.Lifeline5050);
        await Task.Delay(100); // Small delay for sound to register
        
        // Remove two wrong answers
        if (string.IsNullOrEmpty(correctAnswer)) return;
        
        var wrongAnswers = new List<string> { "A", "B", "C", "D" };
        wrongAnswers.Remove(correctAnswer);
        
        // Randomly select 2 wrong answers to remove
        var random = new Random();
        var removedAnswers = new List<string>();
        for (int i = 0; i < 2; i++)
        {
            var indexToRemove = random.Next(wrongAnswers.Count);
            var answerToRemove = wrongAnswers[indexToRemove];
            wrongAnswers.RemoveAt(indexToRemove);
            removedAnswers.Add(answerToRemove);
            
            // Request button disable on control panel
            RequestAnswerRemoval?.Invoke(answerToRemove, "");
            
            // Remove answer from all screens
            _screenService.RemoveAnswer(answerToRemove);
        }
        
        LogMessage?.Invoke($"[Lifeline] 50:50 removed answers: {string.Join(", ", removedAnswers)}");
        LogMessage?.Invoke($"[Lifeline] 50:50 correct answer is: {correctAnswer}");
        
        _screenService.ActivateLifeline(lifeline);
        
        LogMessage?.Invoke("[Lifeline] 50:50 completed and displayed on screens");
    }
    
    #endregion
    
    #region Phone a Friend (PAF) Lifeline
    
    private async Task ExecutePhoneFriendAsync(Lifeline lifeline, int buttonNumber)
    {
        LogMessage?.Invoke("[Lifeline] Phone a Friend (PAF) activated - Stage 1: Calling intro");
        
        _pafStage = PAFStage.CallingIntro;
        _pafLifelineButtonNumber = buttonNumber;
        ButtonStateChanged?.Invoke(buttonNumber, Color.Blue, true);
        
        // Play intro sound on loop
        await PlayLifelineSoundAsync(SoundEffect.LifelinePAFStart, "paf_intro", loop: true);
        
        _screenService.ActivateLifeline(lifeline);
        
        // Show "Calling..." on screens
        _screenService.ShowPAFTimer(0, "Calling");
        
        LogMessage?.Invoke("[Lifeline] PAF intro sound playing (looped) - waiting for host to start countdown");
    }
    
    private void HandlePAFStageClick(int buttonNumber)
    {
        switch (_pafStage)
        {
            case PAFStage.CallingIntro:
                LogMessage?.Invoke("[Lifeline] PAF Stage 2: Starting 30-second countdown");
                
                _soundService.StopSound("paf_intro");
                _pafStage = PAFStage.CountingDown;
                ButtonStateChanged?.Invoke(buttonNumber, Color.Red, true);
                
                _soundService.PlaySound(SoundEffect.LifelinePAFCountdown, "paf_countdown");
                
                _pafSecondsRemaining = 30;
                
                // Show initial countdown on screens
                _screenService.ShowPAFTimer(_pafSecondsRemaining, "Countdown");
                
                _pafTimer = new System.Windows.Forms.Timer();
                _pafTimer.Interval = 1000;
                _pafTimer.Tick += PAFTimer_Tick;
                _pafTimer.Start();
                break;
                
            case PAFStage.CountingDown:
                LogMessage?.Invoke("[Lifeline] PAF ending early (manual)");
                EndPAFEarly(buttonNumber);
                break;
                
            case PAFStage.Completed:
                break;
        }
    }
    
    private void PAFTimer_Tick(object? sender, EventArgs e)
    {
        _pafSecondsRemaining--;
        
        // Update screens with current countdown
        _screenService.ShowPAFTimer(_pafSecondsRemaining, "Countdown");
        
        LogMessage?.Invoke($"[PAF] Countdown: {_pafSecondsRemaining} seconds remaining");
        
        if (_pafSecondsRemaining <= 0)
        {
            CompletePAF();
        }
    }
    
    private void EndPAFEarly(int buttonNumber)
    {
        _pafTimer?.Stop();
        _pafTimer?.Dispose();
        _pafTimer = null;
        
        _soundService.StopSound("paf_countdown");
        _soundService.PlaySound(SoundEffect.LifelinePAFEndEarly);
        
        CompletePAF();
    }
    
    private void CompletePAF()
    {
        _pafTimer?.Stop();
        _pafTimer?.Dispose();
        _pafTimer = null;
        
        _gameService.UseLifeline(LifelineType.PlusOne);
        ButtonStateChanged?.Invoke(_pafLifelineButtonNumber, Color.Gray, false);
        
        _pafStage = PAFStage.Completed;
        
        // Hide PAF timer on screens
        _screenService.ShowPAFTimer(0, "Completed");
        
        LogMessage?.Invoke("[Lifeline] PAF completed and marked as used");
    }
    
    #endregion
    
    #region Ask the Audience (ATA) Lifeline
    
    private async Task ExecuteAskAudienceAsync(Lifeline lifeline, int buttonNumber)
    {
        LogMessage?.Invoke("[Lifeline] Ask the Audience (ATA) activated - Stage 1: Intro (120 seconds)");
        
        _ataStage = ATAStage.Intro;
        _ataLifelineButtonNumber = buttonNumber;
        _ataCorrectAnswer = _screenService.GetCorrectAnswer();
        ButtonStateChanged?.Invoke(buttonNumber, Color.Blue, true);
        
        await PlayLifelineSoundAsync(SoundEffect.LifelineATAStart, "ata_intro");
        
        _ataSecondsRemaining = 120;
        _ataTimer = new System.Windows.Forms.Timer();
        _ataTimer.Interval = 1000;
        _ataTimer.Tick += ATATimer_Tick;
        _ataTimer.Start();
        
        _screenService.ActivateLifeline(lifeline);
        _screenService.ShowATATimer(_ataSecondsRemaining, "Intro");
        
        LogMessage?.Invoke("[Lifeline] ATA displayed on screens - intro timer started");
    }
    
    private void HandleATAStageClick(int buttonNumber)
    {
        switch (_ataStage)
        {
            case ATAStage.Intro:
                StartATAVoting(buttonNumber);
                break;
            case ATAStage.Voting:
                CompleteATA();
                break;
            case ATAStage.Completed:
                break;
        }
    }
    
    private void ATATimer_Tick(object? sender, EventArgs e)
    {
        _ataSecondsRemaining--;
        
        var stageName = _ataStage == ATAStage.Intro ? "Intro" : "Voting";
        LogMessage?.Invoke($"[ATA] {stageName} Countdown: {_ataSecondsRemaining} seconds remaining");
        
        _screenService.ShowATATimer(_ataSecondsRemaining, stageName);
        
        // Show random percentages during voting stage
        if (_ataStage == ATAStage.Voting)
        {
            var randomVotes = GenerateRandomATAPercentages();
            _screenService.ShowATAResults(randomVotes);
        }
        
        if (_ataSecondsRemaining <= 0)
        {
            if (_ataStage == ATAStage.Intro)
            {
                StartATAVoting(_ataLifelineButtonNumber);
            }
            else
            {
                CompleteATA();
            }
        }
    }
    
    private async void StartATAVoting(int buttonNumber)
    {
        _ataTimer?.Stop();
        _ataTimer?.Dispose();
        _ataTimer = null;
        
        LogMessage?.Invoke("[ATA] Intro stage ended, starting voting stage");
        
        _ataStage = ATAStage.Voting;
        ButtonStateChanged?.Invoke(buttonNumber, Color.Red, true);
        
        _soundService.PlaySound(SoundEffect.LifelineATAVote, "ata_vote");
        await Task.Delay(500);
        _soundService.StopSound("ata_intro");
        
        _ataSecondsRemaining = 60;
        _ataTimer = new System.Windows.Forms.Timer();
        _ataTimer.Interval = 1000;
        _ataTimer.Tick += ATATimer_Tick;
        _ataTimer.Start();
        
        _screenService.ShowATATimer(_ataSecondsRemaining, "Voting");
        
        LogMessage?.Invoke("[ATA] Voting timer started - 60 seconds");
    }
    
    private async void CompleteATA()
    {
        _ataTimer?.Stop();
        _ataTimer?.Dispose();
        _ataTimer = null;
        
        _soundService.PlaySound(SoundEffect.LifelineATAEnd);
        await Task.Delay(500);
        _soundService.StopSound("ata_intro");
        _soundService.StopSound("ata_vote");
        
        // Generate placeholder results: 100% on correct answer
        var finalResults = GeneratePlaceholderResults();
        _screenService.ShowATAResults(finalResults);
        
        _gameService.UseLifeline(LifelineType.AskTheAudience);
        ButtonStateChanged?.Invoke(_ataLifelineButtonNumber, Color.Gray, false);
        
        _ataStage = ATAStage.Completed;
        _screenService.ShowATATimer(0, "Completed");
        
        LogMessage?.Invoke("[ATA] Completed and marked as used");
    }
    
    private Dictionary<string, int> GenerateRandomATAPercentages()
    {
        var votes = new Dictionary<string, int>();
        var answers = new[] { "A", "B", "C", "D" };
        
        // Generate random percentages that sum to 100
        var percentages = new int[4];
        var remaining = 100;
        
        for (int i = 0; i < 3; i++)
        {
            percentages[i] = _random.Next(0, remaining + 1);
            remaining -= percentages[i];
        }
        percentages[3] = remaining;
        
        // Shuffle the percentages
        for (int i = 3; i > 0; i--)
        {
            int j = _random.Next(i + 1);
            (percentages[i], percentages[j]) = (percentages[j], percentages[i]);
        }
        
        for (int i = 0; i < 4; i++)
        {
            votes[answers[i]] = percentages[i];
        }
        
        return votes;
    }
    
    private Dictionary<string, int> GeneratePlaceholderResults()
    {
        // Placeholder: 100% on correct answer
        // TODO: Replace with real voting results from database/API
        var votes = new Dictionary<string, int>
        {
            { "A", 0 },
            { "B", 0 },
            { "C", 0 },
            { "D", 0 }
        };
        
        votes[_ataCorrectAnswer] = 100;
        
        LogMessage?.Invoke($"[ATA] Placeholder results: 100% on answer {_ataCorrectAnswer}");
        return votes;
    }
    
    #endregion
    
    #region Switch the Question (STQ) Lifeline
    
    private async Task ExecuteSwitchQuestionAsync(Lifeline lifeline, int buttonNumber)
    {
        var currentQuestionNumber = _gameService.State.CurrentLevel + 1;
        
        LogMessage?.Invoke($"[Lifeline] Switch the Question (STQ) activated at Q{currentQuestionNumber}");
        
        _screenService.ActivateLifeline(lifeline);
        
        _gameService.UseLifeline(lifeline.Type);
        ButtonStateChanged?.Invoke(buttonNumber, Color.Gray, false);
        
        LogMessage?.Invoke($"[Lifeline] STQ loading new question at same difficulty level (Q{currentQuestionNumber})");
        
        // Request new question load via event
        if (RequestAsyncOperation != null)
        {
            var tcs = new TaskCompletionSource<bool>();
            RequestAsyncOperation.Invoke(async () =>
            {
                // The control panel will handle LoadNewQuestion()
                await Task.CompletedTask;
                tcs.SetResult(true);
            });
            await tcs.Task;
        }
        
        LogMessage?.Invoke("[Lifeline] STQ completed - new question loaded");
    }
    
    private async Task ExecuteAskTheHostAsync(Lifeline lifeline, int buttonNumber, string correctAnswer)
    {
        var currentQuestionNumber = _gameService.State.CurrentLevel + 1;
        
        LogMessage?.Invoke($"[Lifeline] Ask the Host (ATH) activated at Q{currentQuestionNumber}");
        
        _athStage = ATHStage.Active;
        _athLifelineButtonNumber = buttonNumber;
        
        // Play host bed music (looped)
        await PlayLifelineSoundAsync(SoundEffect.LifelineATHBed, "ath_bed", loop: true);
        
        _screenService.ActivateLifeline(lifeline);
        
        // Disable button - ATH is now active until player selects an answer
        ButtonStateChanged?.Invoke(buttonNumber, Color.Blue, false);
        
        // Don't reveal the answer - let the host speak in the real game
        LogMessage?.Invoke($"[Lifeline] ATH - Waiting for host response...");
        LogMessage?.Invoke($"[Lifeline] ATH - Active, waiting for answer selection");
        
        // In real game, host would speak here
        // When answer is selected, host_end will play and ATH completes
    }
    

    
    private async Task ExecuteDoubleDipAsync(Lifeline lifeline, int buttonNumber)
    {
        var currentQuestionNumber = _gameService.State.CurrentLevel + 1;
        
        LogMessage?.Invoke($"[Lifeline] Double Dip (DD) activated at Q{currentQuestionNumber}");
        
        _doubleDipStage = DoubleDipStage.FirstAttempt;
        _doubleDipLifelineButtonNumber = buttonNumber;
        
        // Play double dip start sound
        await PlayLifelineSoundAsync(SoundEffect.LifelineDoubleDipStart, "dd_start");
        
        _screenService.ActivateLifeline(lifeline);
        
        // Keep button enabled but change color to indicate active
        ButtonStateChanged?.Invoke(buttonNumber, Color.Blue, true);
        
        LogMessage?.Invoke("[Lifeline] DD - Select your first answer");
    }
    
    private void HandleDoubleDipStageClick(int buttonNumber)
    {
        // This is called when the DD button is clicked during an active DD session
        // Not typically used - DD works through answer selection
        LogMessage?.Invoke("[Lifeline] DD button clicked during active session");
    }
    
    /// <summary>
    /// Handle reveal during Double Dip - called by RevealAnswer
    /// </summary>
    public async Task<DoubleDipRevealResult> HandleDoubleDipRevealAsync(string answer, string correctAnswer, bool isCorrect)
    {
        if (_doubleDipStage == DoubleDipStage.NotStarted)
            return DoubleDipRevealResult.NotActive; // Not in DD mode
        
        if (_doubleDipStage == DoubleDipStage.FirstAttempt)
        {
            _doubleDipFirstAnswer = answer;
            
            if (isCorrect)
            {
                // First answer correct! Complete DD and proceed normally
                LogMessage?.Invoke($"[Lifeline] DD First answer '{answer}' is CORRECT!");
                await CompleteDoubleDip();
                return DoubleDipRevealResult.NotActive; // Proceed with normal reveal
            }
            else
            {
                // First answer wrong - transition to second attempt
                LogMessage?.Invoke($"[Lifeline] DD First answer '{answer}' is WRONG - allowing second attempt");
                _doubleDipStage = DoubleDipStage.SecondAttempt;
                
                // Stop DD start sound
                _soundService.StopSound("dd_start");
                
                LogMessage?.Invoke("[Lifeline] DD - Select your second answer");
                return DoubleDipRevealResult.FirstAttemptWrong; // Special handling in RevealAnswer
            }
        }
        else if (_doubleDipStage == DoubleDipStage.SecondAttempt)
        {
            // Second attempt - complete DD regardless of correct/wrong
            LogMessage?.Invoke($"[Lifeline] DD Second answer '{answer}' - {(isCorrect ? "CORRECT" : "WRONG")}");
            await CompleteDoubleDip();
            return DoubleDipRevealResult.NotActive; // Proceed with normal reveal (correct or wrong)
        }
        
        return DoubleDipRevealResult.NotActive;
    }
    
    private async Task CompleteDoubleDip()
    {
        // Stop DD sounds
        _soundService.StopSound("dd_start");
        _soundService.StopSound("dd_first");
        await Task.Delay(100);
        
        // Mark as used
        _gameService.UseLifeline(LifelineType.DoubleDip);
        
        // Disable button (grey)
        ButtonStateChanged?.Invoke(_doubleDipLifelineButtonNumber, Color.Gray, false);
        
        _doubleDipStage = DoubleDipStage.Completed;
        
        LogMessage?.Invoke("[Lifeline] DD completed");
    }
    
    /// <summary>
    /// Check if ATH is active, and if so, complete it when answer is selected
    /// </summary>
    public async Task<bool> HandleAskTheHostAnswerAsync()
    {
        if (_athStage == ATHStage.Active)
        {
            LogMessage?.Invoke("[Lifeline] ATH answer selected - completing ATH");
            
            // Stop bed music
            _soundService.StopSound("ath_bed");
            
            // Play end sound
            await Task.Delay(100);
            _soundService.PlaySound(SoundEffect.LifelineATHEnd);
            
            // Mark as used
            _gameService.UseLifeline(LifelineType.AskTheHost);
            
            // Button already disabled, just mark as completed
            _athStage = ATHStage.Completed;
            
            LogMessage?.Invoke("[Lifeline] ATH completed");
            return true;
        }
        
        return false;
    }

    #endregion
    
    #region Helper Methods
    
    private async Task PlayLifelineSoundAsync(SoundEffect effect, string? key = null, bool loop = false)
    {
        // For Q1-4, notify UI that bed music should restart after correct answer reveal
        // Not needed for Q5 (milestone) - bed music won't be playing anyway
        var questionNumber = _gameService.State.CurrentLevel + 1;
        if (questionNumber >= 1 && questionNumber <= 4)
        {
            RequestBedMusicRestart?.Invoke();
            LogMessage?.Invoke($"[BedMusic] Q{questionNumber} lifeline will trigger bed music restart after correct answer");
        }
        
        // Stop all sounds
        _soundService.StopAllSounds();
        
        // Wait 500ms
        await Task.Delay(500);
        
        // Play the lifeline sound
        if (loop && key != null)
        {
            _soundService.PlaySound(effect, key, loop: true);
        }
        else if (key != null)
        {
            _soundService.PlaySound(effect, key);
        }
        else
        {
            _soundService.PlaySound(effect);
        }
    }
    
    /// <summary>
    /// Clean up resources when closing
    /// </summary>
    public void Dispose()
    {
        _pafTimer?.Stop();
        _pafTimer?.Dispose();
        _ataTimer?.Stop();
        _ataTimer?.Dispose();
    }
    
    /// <summary>
    /// Resets all lifeline state to initial values
    /// </summary>
    public void Reset()
    {
        // Stop and dispose timers
        _pafTimer?.Stop();
        _pafTimer?.Dispose();
        _pafTimer = null;
        _ataTimer?.Stop();
        _ataTimer?.Dispose();
        _ataTimer = null;
        
        // Reset state fields
        _pafStage = PAFStage.NotStarted;
        _pafLifelineButtonNumber = 0;
        _pafSecondsRemaining = 30;
        _ataStage = ATAStage.NotStarted;
        _ataLifelineButtonNumber = 0;
        _ataSecondsRemaining = 120;
        _doubleDipStage = DoubleDipStage.NotStarted;
        _doubleDipLifelineButtonNumber = 0;
        _doubleDipFirstAnswer = "";
        _athStage = ATHStage.NotStarted;
        _athLifelineButtonNumber = 0;
    }
    
    #endregion
}

/// <summary>
/// Phone a Friend lifeline stages
/// </summary>
public enum PAFStage
{
    NotStarted,
    CallingIntro,
    CountingDown,
    Completed
}

/// <summary>
/// Ask the Audience lifeline stages
/// </summary>
public enum ATAStage
{
    NotStarted,
    Intro,
    Voting,
    Completed
}

/// <summary>
/// Double Dip lifeline stages
/// </summary>
public enum DoubleDipStage
{
    NotStarted,
    FirstAttempt,
    SecondAttempt,
    Completed
}

/// <summary>
/// Result of Double Dip reveal handling
/// </summary>
public enum DoubleDipRevealResult
{
    NotActive,
    FirstAttemptWrong,
    SecondAttempt
}

/// <summary>
/// Ask the Host lifeline stages
/// </summary>
public enum ATHStage
{
    NotStarted,
    Active,
    Completed
}
