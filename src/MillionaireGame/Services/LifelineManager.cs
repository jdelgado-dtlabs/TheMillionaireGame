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
        }
    }
    
    /// <summary>
    /// Handle multi-stage lifeline button clicks (PAF/ATA)
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
            _ => false
        };
    }
    
    #region 50:50 Lifeline
    
    private async Task ExecuteFiftyFiftyAsync(Lifeline lifeline, int buttonNumber, string correctAnswer)
    {
        LogMessage?.Invoke("[Lifeline] 50:50 activated");
        
        _gameService.UseLifeline(lifeline.Type);
        ButtonStateChanged?.Invoke(buttonNumber, Color.Gray, false);
        
        // Play lifeline sound
        await PlayLifelineSoundAsync(SoundEffect.Lifeline5050);
        
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
            
            // Request answer removal via event
            RequestAnswerRemoval?.Invoke(answerToRemove, "");
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
        
        LogMessage?.Invoke("[Lifeline] PAF completed and marked as used");
    }
    
    #endregion
    
    #region Ask the Audience (ATA) Lifeline
    
    private async Task ExecuteAskAudienceAsync(Lifeline lifeline, int buttonNumber)
    {
        LogMessage?.Invoke("[Lifeline] Ask the Audience (ATA) activated - Stage 1: Intro (120 seconds)");
        
        _ataStage = ATAStage.Intro;
        _ataLifelineButtonNumber = buttonNumber;
        ButtonStateChanged?.Invoke(buttonNumber, Color.Blue, true);
        
        await PlayLifelineSoundAsync(SoundEffect.LifelineATAStart, "ata_intro");
        
        _ataSecondsRemaining = 120;
        _ataTimer = new System.Windows.Forms.Timer();
        _ataTimer.Interval = 1000;
        _ataTimer.Tick += ATATimer_Tick;
        _ataTimer.Start();
        
        _screenService.ActivateLifeline(lifeline);
        
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
        
        _gameService.UseLifeline(LifelineType.AskTheAudience);
        ButtonStateChanged?.Invoke(_ataLifelineButtonNumber, Color.Gray, false);
        
        _ataStage = ATAStage.Completed;
        
        LogMessage?.Invoke("[ATA] Completed and marked as used");
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
