using MillionaireGame.Core.Game;
using MillionaireGame.Core.Models;
using MillionaireGame.Core.Services;
using MillionaireGame.Core.Graphics;
using MillionaireGame.Hosting;
using System.Drawing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace MillionaireGame.Services;

/// <summary>
/// Manages all lifeline execution logic, state tracking, and multi-stage behavior
/// </summary>
public class LifelineManager
{
    private readonly GameService _gameService;
    private readonly SoundService _soundService;
    private readonly ScreenUpdateService _screenService;
    private readonly Func<WebServerHost?> _getWebServerHost;
    
    // PAF state tracking
    private PAFStage _pafStage = PAFStage.NotStarted;
    private int _pafLifelineButtonNumber = 0;
    private System.Threading.Timer? _pafTimer;
    private int _pafSecondsRemaining = 30;
    
    // ATA state tracking
    private ATAStage _ataStage = ATAStage.NotStarted;
    private int _ataLifelineButtonNumber = 0;
    private System.Threading.Timer? _ataTimer;
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
    
    // Lifeline icon ping animation state
    private Dictionary<int, (LifelineType type, CancellationTokenSource cts)> _activePings = new();
    
    // Click cooldown protection
    private DateTime _lastClickTime = DateTime.MinValue;
    private const int CLICK_COOLDOWN_MS = 1000;
    
    // Active multi-stage lifeline tracking
    private bool _isMultiStageActive = false;
    
    // Events for UI updates
    public event Action<int, Color, bool>? ButtonStateChanged; // buttonNumber, color, enabled
    public event Action<int, bool>? SetOtherButtonsToStandby; // activeButtonNumber, isStandby (true=orange, false=reset)
    public event Action<Func<Task>>? RequestAsyncOperation; // For operations that need to await
    public event Action<string, string>? RequestAnswerRemoval; // For 50:50 answer removal
    public event Action<string>? LogMessage; // For debug logging
    public event Action? RequestBedMusicRestart; // For Q1-5 bed music restart after lifeline
    
    public LifelineManager(
        GameService gameService, 
        SoundService soundService, 
        ScreenUpdateService screenService,
        Func<WebServerHost?> getWebServerHost)
    {
        _gameService = gameService;
        _soundService = soundService;
        _screenService = screenService;
        _getWebServerHost = getWebServerHost;
    }
    
    /// <summary>
    /// Execute a lifeline based on its type
    /// </summary>
    public async Task ExecuteLifelineAsync(LifelineType type, int buttonNumber, string correctAnswer)
    {
        // Click cooldown protection
        var timeSinceLastClick = (DateTime.Now - _lastClickTime).TotalMilliseconds;
        if (timeSinceLastClick < CLICK_COOLDOWN_MS)
        {
            LogMessage?.Invoke($"[Lifeline] Click ignored - cooldown active ({CLICK_COOLDOWN_MS - (int)timeSinceLastClick}ms remaining)");
            return;
        }
        _lastClickTime = DateTime.Now;
        
        // Check if another multi-stage lifeline is already active
        if (_isMultiStageActive)
        {
            LogMessage?.Invoke($"[Lifeline] Click ignored - another multi-stage lifeline is active");
            return;
        }
        
        var lifeline = _gameService.State.GetLifeline(type);
        if (lifeline == null || lifeline.IsUsed)
        {
            return;
        }
        
        // Mark multi-stage lifelines as active and set other buttons to standby
        bool isMultiStage = type is LifelineType.PlusOne or LifelineType.AskTheAudience or LifelineType.DoubleDip or LifelineType.AskTheHost;
        if (isMultiStage)
        {
            _isMultiStageActive = true;
            SetOtherButtonsToStandby?.Invoke(buttonNumber, true); // Set other buttons to orange/standby
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
            case LifelineType.AskTheHost when _athStage != ATHStage.NotStarted:
                HandleATHStageClick(buttonNumber);
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
            LifelineType.AskTheAudience => _ataStage != ATAStage.NotStarted,            LifelineType.AskTheHost => _athStage != ATHStage.NotStarted,            LifelineType.DoubleDip => _doubleDipStage != DoubleDipStage.NotStarted,
            _ => false
        };
    }
    
    #region 50:50 Lifeline
    
    private async Task ExecuteFiftyFiftyAsync(Lifeline lifeline, int buttonNumber, string correctAnswer)
    {
        LogMessage?.Invoke("[Lifeline] 50:50 activated");
        
        _gameService.UseLifeline(lifeline.Type);
        ButtonStateChanged?.Invoke(buttonNumber, Color.Gray, false);
        
        // Update icon to Used state
        _screenService.SetLifelineIcon(buttonNumber, lifeline.Type, LifelineIconState.Used);
        
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
        
        // Stop bed music for multi-stage lifeline
        _soundService.StopMusic();
        
        _pafStage = PAFStage.CallingIntro;
        _pafLifelineButtonNumber = buttonNumber;
        ButtonStateChanged?.Invoke(buttonNumber, Color.Blue, true);
        
        // Activate the icon to show it's in use (no sound)
        ActivateLifelineIcon(buttonNumber, lifeline.Type);
        
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
                
                // IMMEDIATE priority will auto-stop previous sound
                _soundService.PlaySound(SoundEffect.LifelinePAFCountdown, "paf_countdown");
                
                _pafStage = PAFStage.CountingDown;
                ButtonStateChanged?.Invoke(buttonNumber, Color.Red, true);
                
                _pafSecondsRemaining = 30;
                
                // Show initial countdown on screens
                _screenService.ShowPAFTimer(_pafSecondsRemaining, "Countdown");
                
                _pafTimer = new System.Threading.Timer(
                    PAFTimer_Tick,
                    null,
                    1000,  // Start after 1 second
                    1000); // Repeat every 1 second
                break;
                
            case PAFStage.CountingDown:
                LogMessage?.Invoke("[Lifeline] PAF ending early (manual)");
                EndPAFEarly(buttonNumber);
                break;
                
            case PAFStage.Completed:
                break;
        }
    }
    
    private void PAFTimer_Tick(object? state)
    {
        // Guard: Exit if already completed (prevents queued timer events from firing)
        if (_pafStage == PAFStage.Completed)
        {
            _pafTimer?.Dispose();
            return;
        }
            
        _pafSecondsRemaining--;
        
        LogMessage?.Invoke($"[PAF] Timer tick: {_pafSecondsRemaining} seconds remaining");
        
        // Update screens with current countdown
        _screenService.ShowPAFTimer(_pafSecondsRemaining, "Countdown");
        
        if (_pafSecondsRemaining <= 0)
        {
            LogMessage?.Invoke("[PAF] Timer reached 0, completing PAF");
            CompletePAF();
        }
    }
    
    private void EndPAFEarly(int buttonNumber)
    {
        _pafTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        _pafTimer?.Dispose();
        _pafTimer = null;
        
        // IMMEDIATE priority will auto-stop countdown sound
        _soundService.PlaySound(SoundEffect.LifelinePAFEndEarly);
        
        CompletePAF();
    }
    
    private void CompletePAF()
    {
        // Set stage first so any queued timer ticks will exit early
        _pafStage = PAFStage.Completed;
        
        _pafTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        _pafTimer?.Dispose();
        _pafTimer = null;
        
        _gameService.UseLifeline(LifelineType.PlusOne);
        ButtonStateChanged?.Invoke(_pafLifelineButtonNumber, Color.Gray, false);
        
        // Update icon to Used state
        _screenService.SetLifelineIcon(_pafLifelineButtonNumber, LifelineType.PlusOne, LifelineIconState.Used);
        
        // Hide PAF timer on screens
        _screenService.ShowPAFTimer(0, "Completed");
        
        // Reset other lifeline buttons from standby
        _isMultiStageActive = false;
        SetOtherButtonsToStandby?.Invoke(_pafLifelineButtonNumber, false);
        
        LogMessage?.Invoke("[Lifeline] PAF completed and marked as used");
    }
    
    #endregion
    
    #region Ask the Audience (ATA) Lifeline
    
    private async Task ExecuteAskAudienceAsync(Lifeline lifeline, int buttonNumber)
    {
        LogMessage?.Invoke("[Lifeline] Ask the Audience (ATA) activated - Stage 1: Intro (120 seconds)");
        
        // Stop bed music for multi-stage lifeline
        _soundService.StopMusic();
        
        _ataStage = ATAStage.Intro;
        _ataLifelineButtonNumber = buttonNumber;
        _ataCorrectAnswer = _screenService.GetCorrectAnswer();
        ButtonStateChanged?.Invoke(buttonNumber, Color.Blue, true);
        
        // Activate the icon to show it's in use
        ActivateLifelineIcon(buttonNumber, lifeline.Type);
        
        await PlayLifelineSoundAsync(SoundEffect.LifelineATAStart, "ata_intro");
        
        _ataSecondsRemaining = 120;
        _ataTimer = new System.Threading.Timer(
            ATATimer_Tick,
            null,
            1000,  // Start after 1 second
            1000); // Repeat every 1 second
        
        _screenService.ActivateLifeline(lifeline);
        _screenService.ShowATATimer(_ataSecondsRemaining, "Intro");
        
        // Notify web clients: Show question in view-only mode during intro
        await NotifyWebClientsATAIntro();
        
        LogMessage?.Invoke("[Lifeline] ATA displayed on screens - intro timer started");
    }
    
    private void HandleATAStageClick(int buttonNumber)
    {
        switch (_ataStage)
        {
            case ATAStage.Intro:
                // Check if offline mode
                var webServerHost = _getWebServerHost();
                bool isOffline = webServerHost == null || !webServerHost.IsRunning;
                
                if (isOffline)
                {
                    LogMessage?.Invoke("[ATA] Offline mode - skipping voting, going directly to results");
                    CompleteATA();
                }
                else
                {
                    StartATAVoting(buttonNumber);
                }
                break;
            case ATAStage.Voting:
                CompleteATA();
                break;
            case ATAStage.Completed:
                break;
        }
    }
    
    private void ATATimer_Tick(object? state)
    {
        // Guard: Exit if already completed (prevents queued timer events from firing)
        if (_ataStage == ATAStage.Completed)
        {
            _ataTimer?.Dispose();
            return;
        }
            
        _ataSecondsRemaining--;
        
        var stageName = _ataStage == ATAStage.Intro ? "Intro" : "Voting";
        LogMessage?.Invoke($"[ATA] Timer tick - {stageName}: {_ataSecondsRemaining} seconds remaining");
        
        _screenService.ShowATATimer(_ataSecondsRemaining, stageName);
        
        // Show random percentages during voting stage
        if (_ataStage == ATAStage.Voting)
        {
            var randomVotes = GenerateRandomATAPercentages();
            _screenService.ShowATAResults(randomVotes);
            
            // Check if all participants have voted (online mode only)
            CheckForVoteCompletion();
        }
        
        if (_ataSecondsRemaining <= 0)
        {
            LogMessage?.Invoke($"[ATA] Timer reached 0 at {stageName} stage");
            if (_ataStage == ATAStage.Intro)
            {
                // Check if offline mode
                var webServerHost = _getWebServerHost();
                bool isOffline = webServerHost == null || !webServerHost.IsRunning;
                
                if (isOffline)
                {
                    LogMessage?.Invoke("[ATA] Offline mode - skipping voting, going directly to results");
                    CompleteATA();
                }
                else
                {
                    StartATAVoting(_ataLifelineButtonNumber);
                }
            }
            else
            {
                CompleteATA();
            }
        }
    }
    
    private async void StartATAVoting(int buttonNumber)
    {
        _ataTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        _ataTimer?.Dispose();
        _ataTimer = null;
        
        LogMessage?.Invoke("[ATA] Intro stage ended, starting voting stage");
        
        _ataStage = ATAStage.Voting;
        ButtonStateChanged?.Invoke(buttonNumber, Color.Red, true);
        
        // IMMEDIATE priority will auto-stop intro sound
        _soundService.PlaySound(SoundEffect.LifelineATAVote, "ata_vote");
        
        _ataSecondsRemaining = 60;
        _ataTimer = new System.Threading.Timer(
            ATATimer_Tick,
            null,
            1000,  // Start after 1 second
            1000); // Repeat every 1 second
        
        _screenService.ShowATATimer(_ataSecondsRemaining, "Voting");
        
        // Notify web clients: Enable voting
        await NotifyWebClientsATAVoting(60);
        
        LogMessage?.Invoke("[ATA] Voting timer started - 60 seconds");
    }
    
    private async void CompleteATA()
    {
        // Set stage first so any queued timer ticks will exit early
        _ataStage = ATAStage.Completed;
        
        _ataTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        _ataTimer?.Dispose();
        _ataTimer = null;
        
        // Notify web clients: End voting and show results
        await NotifyWebClientsATAComplete();
        
        // Step 1: Get and show results first
        var finalResults = await GetATAResultsAsync();
        _screenService.ShowATAResults(finalResults);
        
        // Step 2: Play ending sound (IMMEDIATE priority will stop previous sounds automatically)
        _soundService.PlaySound(SoundEffect.LifelineATAEnd);
        
        // Step 3: Results will persist on screen until answer is selected
        // (No auto-hide - ClearATAFromScreens() will be called when answer is selected)
        
        _gameService.UseLifeline(LifelineType.AskTheAudience);
        ButtonStateChanged?.Invoke(_ataLifelineButtonNumber, Color.Gray, false);
        
        // Update icon to Used state
        _screenService.SetLifelineIcon(_ataLifelineButtonNumber, LifelineType.AskTheAudience, LifelineIconState.Used);
        
        _screenService.ShowATATimer(0, "Completed");
        
        // Reset other lifeline buttons from standby
        _isMultiStageActive = false;
        SetOtherButtonsToStandby?.Invoke(_ataLifelineButtonNumber, false);
        
        LogMessage?.Invoke("[ATA] Completed and marked as used");
    }

    /// <summary>
    /// Clear ATA results from all screens and notify web clients when answer is selected
    /// </summary>
    public async Task ClearATAFromScreens()
    {
        // Clear from physical screens
        _screenService.HideATAResults();
        
        // Notify web clients to return to lobby
        var webServerHost = _getWebServerHost();
        if (webServerHost != null && webServerHost.IsRunning)
        {
            try
            {
                var serviceScopeFactory = webServerHost.GetService<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();
                if (serviceScopeFactory != null)
                {
                    using var scope = serviceScopeFactory.CreateScope();
                    var hubContext = webServerHost.GetService<Microsoft.AspNetCore.SignalR.IHubContext<MillionaireGame.Web.Hubs.GameHub>>();
                    var dbContext = scope.ServiceProvider.GetService<MillionaireGame.Web.Data.WAPSDbContext>();
                    
                    if (hubContext != null && dbContext != null)
                    {
                        var activeSession = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
                            dbContext.Sessions.Where(s => s.Status == MillionaireGame.Web.Models.SessionStatus.Active));
                        
                        if (activeSession != null)
                        {
                            // Broadcast state change back to Waiting Lobby
                            await webServerHost.BroadcastGameStateAsync(
                                activeSession.Id,
                                Web.Models.GameStateType.WaitingLobby,
                                "Waiting for next activity...");
                            
                            await hubContext.Clients.Group(activeSession.Id).SendAsync("ATACleared");
                            LogMessage?.Invoke("[ATA] Cleared from web clients");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke($"[ATA] Error clearing from web clients: {ex.Message}");
            }
        }
        
        LogMessage?.Invoke("[ATA] Cleared from screens");
    }
    
    /// <summary>
    /// Check if all online participants have voted and auto-complete if so
    /// </summary>
    private async void CheckForVoteCompletion()
    {
        var webServerHost = _getWebServerHost();
        
        // Only check if web server is running (online mode)
        if (webServerHost == null || !webServerHost.IsRunning)
        {
            return;
        }
        
        try
        {
            // Create a service scope to get fresh services
            var serviceScopeFactory = webServerHost.GetService<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();
            if (serviceScopeFactory == null)
            {
                return;
            }

            using var scope = serviceScopeFactory.CreateScope();
            var sessionService = scope.ServiceProvider.GetService<MillionaireGame.Web.Services.SessionService>();
            
            if (sessionService == null)
            {
                return;
            }
            
            var dbContext = scope.ServiceProvider.GetService<MillionaireGame.Web.Data.WAPSDbContext>();
            
            if (dbContext == null)
            {
                return;
            }
            
            // Get active session
            var activeSession = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
                dbContext.Sessions.Where(s => s.Status == MillionaireGame.Web.Models.SessionStatus.Active));
            
            if (activeSession == null)
            {
                return;
            }
            
            // Get active participant count
            var activeParticipants = await sessionService.GetActiveParticipantsAsync(activeSession.Id);
            var participantCount = activeParticipants.Count();
            
            // Get vote count
            var voteCount = await sessionService.GetATAVoteCountAsync(activeSession.Id);
            
            // If all participants have voted, complete ATA early
            if (participantCount > 0 && voteCount >= participantCount)
            {
                LogMessage?.Invoke($"[ATA] All participants voted ({voteCount}/{participantCount}) - auto-completing");
                CompleteATA();
            }
        }
        catch (Exception ex)
        {
            // Silent failure - this is just an optimization, timer will complete normally
            LogMessage?.Invoke($"[ATA] Vote completion check failed: {ex.Message}");
        }
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
        // Offline Mode: Generate realistic-looking voting distribution
        // Correct answer gets 40-80%, wrong answers split the remainder
        // IMPORTANT: Correct answer must ALWAYS have the highest percentage
        var votes = new Dictionary<string, int>
        {
            { "A", 0 },
            { "B", 0 },
            { "C", 0 },
            { "D", 0 }
        };
        
        // Generate realistic percentage for correct answer (40-80%)
        int correctPercentage = _random.Next(40, 81);
        
        // Distribute remaining percentage across wrong answers
        int remainingPercentage = 100 - correctPercentage;
        var wrongAnswers = new[] { "A", "B", "C", "D" }.Where(a => a != _ataCorrectAnswer).ToArray();
        
        // Split remaining percentage across 3 wrong answers
        // Each wrong answer can get at most (correctPercentage - 1) to ensure correct answer is always highest
        int maxWrongPercentage = correctPercentage - 1;
        
        for (int i = 0; i < wrongAnswers.Length - 1; i++)
        {
            // Cap the wrong answer percentage to be less than correct answer
            int maxAllowed = Math.Min(maxWrongPercentage, remainingPercentage);
            int wrongPercentage = _random.Next(0, maxAllowed + 1);
            votes[wrongAnswers[i]] = wrongPercentage;
            remainingPercentage -= wrongPercentage;
        }
        
        // Last wrong answer gets whatever's left (capped to be less than correct answer)
        votes[wrongAnswers[^1]] = Math.Min(remainingPercentage, maxWrongPercentage);
        
        // Assign correct answer percentage last
        votes[_ataCorrectAnswer] = correctPercentage;
        
        LogMessage?.Invoke($"[ATA] Offline results: {votes["A"]}% A, {votes["B"]}% B, {votes["C"]}% C, {votes["D"]}% D (Correct: {_ataCorrectAnswer})");
        return votes;
    }
    
    /// <summary>
    /// Get ATA results - online from WAPS database if available, otherwise offline mode
    /// </summary>
    private async Task<Dictionary<string, int>> GetATAResultsAsync()
    {
        var webServerHost = _getWebServerHost();
        
        // Check if web server is running
        if (webServerHost == null || !webServerHost.IsRunning)
        {
            LogMessage?.Invoke("[ATA] Web server not running - using offline mode");
            return GeneratePlaceholderResults();
        }
        
        try
        {
            // Create a service scope to get fresh services
            var serviceScopeFactory = webServerHost.GetService<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();
            if (serviceScopeFactory == null)
            {
                LogMessage?.Invoke("[ATA] ServiceScopeFactory not available - falling back to offline mode");
                return GeneratePlaceholderResults();
            }

            using var scope = serviceScopeFactory.CreateScope();
            
            // Get SessionService from scope
            var sessionService = scope.ServiceProvider.GetService<MillionaireGame.Web.Services.SessionService>();
            
            if (sessionService == null)
            {
                LogMessage?.Invoke("[ATA] SessionService not available - falling back to offline mode");
                return GeneratePlaceholderResults();
            }
            
            // Get WAPSDbContext from scope
            var dbContext = scope.ServiceProvider.GetService<MillionaireGame.Web.Data.WAPSDbContext>();
            
            if (dbContext == null)
            {
                LogMessage?.Invoke("[ATA] WAPSDbContext not available - falling back to offline mode");
                return GeneratePlaceholderResults();
            }
            
            // Query for active session
            var activeSession = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
                dbContext.Sessions.Where(s => s.Status == MillionaireGame.Web.Models.SessionStatus.Active));
            
            if (activeSession == null)
            {
                LogMessage?.Invoke("[ATA] No active session found - falling back to offline mode");
                return GeneratePlaceholderResults();
            }
            
            // Get real voting percentages from database
            var percentages = await sessionService.CalculateATAPercentagesAsync(activeSession.Id);
            var totalVotes = await sessionService.GetATAVoteCountAsync(activeSession.Id);
            
            if (totalVotes == 0)
            {
                LogMessage?.Invoke("[ATA] No votes received - falling back to offline mode");
                return GeneratePlaceholderResults();
            }
            
            // Convert double percentages to int
            var results = new Dictionary<string, int>
            {
                { "A", (int)Math.Round(percentages.GetValueOrDefault("A", 0)) },
                { "B", (int)Math.Round(percentages.GetValueOrDefault("B", 0)) },
                { "C", (int)Math.Round(percentages.GetValueOrDefault("C", 0)) },
                { "D", (int)Math.Round(percentages.GetValueOrDefault("D", 0)) }
            };
            
            LogMessage?.Invoke($"[ATA] Online results: {results["A"]}% A, {results["B"]}% B, {results["C"]}% C, {results["D"]}% D (Total votes: {totalVotes})");
            return results;
        }
        catch (Exception ex)
        {
            LogMessage?.Invoke($"[ATA] Error querying online votes: {ex.Message} - falling back to offline mode");
            return GeneratePlaceholderResults();
        }
    }
    
    /// <summary>
    /// Notify web clients that ATA intro has started (show question in view-only mode)
    /// </summary>
    private async Task NotifyWebClientsATAIntro()
    {
        var webServerHost = _getWebServerHost();
        if (webServerHost == null || !webServerHost.IsRunning)
        {
            return; // Offline mode - no web clients
        }

        try
        {
            var question = _screenService.GetCurrentQuestion();
            if (question == null)
            {
                LogMessage?.Invoke("[ATA] No current question to send to web clients");
                return;
            }

            // Get the Game hub context (web clients are connected to GameHub)
            var hubContext = webServerHost.GetService<Microsoft.AspNetCore.SignalR.IHubContext<MillionaireGame.Web.Hubs.GameHub>>();
            if (hubContext == null)
            {
                LogMessage?.Invoke("[ATA] GameHub context not available");
                return;
            }

            // Create a service scope to get a fresh DbContext
            var serviceScopeFactory = webServerHost.GetService<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();
            if (serviceScopeFactory == null)
            {
                LogMessage?.Invoke("[ATA] ServiceScopeFactory not available");
                return;
            }

            using var scope = serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetService<MillionaireGame.Web.Data.WAPSDbContext>();
            if (dbContext == null)
            {
                return;
            }

            var activeSession = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
                dbContext.Sessions.Where(s => s.Status == MillionaireGame.Web.Models.SessionStatus.Active));

            if (activeSession == null)
            {
                LogMessage?.Invoke("[ATA] No active session found for web notification");
                return;
            }

            LogMessage?.Invoke($"[ATA] Broadcasting ATAIntroStarted to session group: {activeSession.Id}");

            // Broadcast game state change to ATAReady
            await webServerHost.BroadcastGameStateAsync(
                activeSession.Id,
                Web.Models.GameStateType.ATAReady,
                "Get ready to vote!");

            // Broadcast intro message to web clients (showing question but voting disabled)
            await hubContext.Clients.Group(activeSession.Id).SendAsync("ATAIntroStarted", new
            {
                QuestionText = question.QuestionText,
                OptionA = question.AnswerA,
                OptionB = question.AnswerB,
                OptionC = question.AnswerC,
                OptionD = question.AnswerD,
                TimeLimit = 120,
                StartTime = DateTime.UtcNow
            });

            LogMessage?.Invoke($"[ATA] Intro notification sent to web clients in session {activeSession.Id}");
        }
        catch (Exception ex)
        {
            LogMessage?.Invoke($"[ATA] Error notifying web clients of intro: {ex.Message}");
        }
    }

    /// <summary>
    /// Notify web clients that voting has started (enable voting)
    /// </summary>
    private async Task NotifyWebClientsATAVoting(int timeLimit)
    {
        var webServerHost = _getWebServerHost();
        if (webServerHost == null || !webServerHost.IsRunning)
        {
            return; // Offline mode
        }

        try
        {
            var question = _screenService.GetCurrentQuestion();
            if (question == null)
            {
                return;
            }

            // Create a service scope to get a fresh DbContext
            var serviceScopeFactory = webServerHost.GetService<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();
            if (serviceScopeFactory == null)
            {
                return;
            }

            using var scope = serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetService<MillionaireGame.Web.Data.WAPSDbContext>();
            if (dbContext == null)
            {
                return;
            }

            var activeSession = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
                dbContext.Sessions.Where(s => s.Status == MillionaireGame.Web.Models.SessionStatus.Active));

            if (activeSession == null)
            {
                LogMessage?.Invoke("[ATA] No active session found for voting notification");
                return;
            }

            LogMessage?.Invoke($"[ATA] Broadcasting VotingStarted to session group: {activeSession.Id}");

            // Broadcast game state change to ATAVoting
            await webServerHost.BroadcastGameStateAsync(
                activeSession.Id,
                Web.Models.GameStateType.ATAVoting,
                "Vote now!");

            // Use the GameHub to broadcast voting started
            var hubContext = webServerHost.GetService<Microsoft.AspNetCore.SignalR.IHubContext<MillionaireGame.Web.Hubs.GameHub>>();
            if (hubContext == null)
            {
                return;
            }

            await hubContext.Clients.Group(activeSession.Id).SendAsync("VotingStarted", new
            {
                QuestionText = question.QuestionText,
                OptionA = question.AnswerA,
                OptionB = question.AnswerB,
                OptionC = question.AnswerC,
                OptionD = question.AnswerD,
                TimeLimit = timeLimit,
                StartTime = DateTime.UtcNow
            });

            LogMessage?.Invoke($"[ATA] Voting notification sent to web clients in session {activeSession.Id}");
        }
        catch (Exception ex)
        {
            LogMessage?.Invoke($"[ATA] Error notifying web clients of voting: {ex.Message}");
        }
    }

    /// <summary>
    /// Notify web clients that ATA has completed (show results, then return to lobby)
    /// </summary>
    private async Task NotifyWebClientsATAComplete()
    {
        var webServerHost = _getWebServerHost();
        if (webServerHost == null || !webServerHost.IsRunning)
        {
            return; // Offline mode
        }

        try
        {
            // Create a service scope to get fresh services
            var serviceScopeFactory = webServerHost.GetService<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();
            if (serviceScopeFactory == null)
            {
                return;
            }

            using var scope = serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetService<MillionaireGame.Web.Data.WAPSDbContext>();
            if (dbContext == null)
            {
                return;
            }

            var activeSession = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
                dbContext.Sessions.Where(s => s.Status == MillionaireGame.Web.Models.SessionStatus.Active));

            if (activeSession == null)
            {
                LogMessage?.Invoke("[ATA] No active session found for completion notification");
                return;
            }

            LogMessage?.Invoke($"[ATA] Broadcasting VotingEnded to session group: {activeSession.Id}");

            // Get SessionService from scope
            var sessionService = scope.ServiceProvider.GetService<MillionaireGame.Web.Services.SessionService>();
            if (sessionService == null)
            {
                return;
            }

            // Calculate final percentages
            var percentages = await sessionService.CalculateATAPercentagesAsync(activeSession.Id);
            var totalVotes = await sessionService.GetATAVoteCountAsync(activeSession.Id);

            // Mark all voters as having used ATA
            await sessionService.MarkATAUsedForVotersAsync(activeSession.Id);

            // Broadcast game state change to ATAResults
            await webServerHost.BroadcastGameStateAsync(
                activeSession.Id,
                Web.Models.GameStateType.ATAResults,
                "Here are the results!");

            // Broadcast completion to web clients via GameHub
            var hubContext = webServerHost.GetService<Microsoft.AspNetCore.SignalR.IHubContext<MillionaireGame.Web.Hubs.GameHub>>();
            if (hubContext == null)
            {
                return;
            }

            await hubContext.Clients.Group(activeSession.Id).SendAsync("VotingEnded", new
            {
                EndTime = DateTime.UtcNow,
                FinalResults = percentages,
                TotalVotes = totalVotes
            });

            LogMessage?.Invoke($"[ATA] Completion notification sent to web clients in session {activeSession.Id}");
        }
        catch (Exception ex)
        {
            LogMessage?.Invoke($"[ATA] Error notifying web clients of completion: {ex.Message}");
        }
    }

    
    #endregion
    
    #region Switch the Question (STQ) Lifeline
    
    private async Task ExecuteSwitchQuestionAsync(Lifeline lifeline, int buttonNumber)
    {
        var currentQuestionNumber = _gameService.State.CurrentLevel + 1;
        
        LogMessage?.Invoke($"[Lifeline] Switch the Question (STQ) activated at Q{currentQuestionNumber}");
        
        // Play flip sound effect
        _soundService.PlaySound(SoundEffect.SwitchFlip);
        
        _screenService.ActivateLifeline(lifeline);
        
        _gameService.UseLifeline(lifeline.Type);
        ButtonStateChanged?.Invoke(buttonNumber, Color.Gray, false);
        
        // Update icon to Used state
        _screenService.SetLifelineIcon(buttonNumber, lifeline.Type, LifelineIconState.Used);
        
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
        
        // Stop bed music for multi-stage lifeline
        _soundService.StopMusic();
        
        _athStage = ATHStage.Active;
        _athLifelineButtonNumber = buttonNumber;
        
        // Activate the icon to show it's in use
        ActivateLifelineIcon(buttonNumber, lifeline.Type);
        
        // Play host bed music (looped)
        await PlayLifelineSoundAsync(SoundEffect.LifelineATHBed, "ath_bed", loop: true);
        
        _screenService.ActivateLifeline(lifeline);
        
        // Keep button enabled (blue) - click again to play ATHEnd and complete
        ButtonStateChanged?.Invoke(buttonNumber, Color.Blue, true);
        
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
        
        // Activate the icon to show it's in use
        ActivateLifelineIcon(buttonNumber, lifeline.Type);
        
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
                
                // DD start sound will stop naturally when answer reveal plays (IMMEDIATE priority)
                
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
        // DD sounds will stop naturally when next sound plays (IMMEDIATE priority)
        
        // Mark as used
        _gameService.UseLifeline(LifelineType.DoubleDip);
        
        // Disable button (grey)
        ButtonStateChanged?.Invoke(_doubleDipLifelineButtonNumber, Color.Gray, false);
        
        // Update icon to Used state
        _screenService.SetLifelineIcon(_doubleDipLifelineButtonNumber, LifelineType.DoubleDip, LifelineIconState.Used);
        
        // Reset other lifeline buttons from standby
        _isMultiStageActive = false;
        SetOtherButtonsToStandby?.Invoke(_doubleDipLifelineButtonNumber, false);
        
        _doubleDipStage = DoubleDipStage.Completed;
        
        LogMessage?.Invoke("[Lifeline] DD completed");
    }
    
    /// <summary>
    /// Handle second click on ATH button to complete the lifeline
    /// Plays ATHEnd and marks the lifeline as used
    /// </summary>
    private void HandleATHStageClick(int buttonNumber)
    {
        if (_athStage != ATHStage.Active)
        {
            LogMessage?.Invoke($"[Lifeline] ATH stage click ignored - not in active state");
            return;
        }
        
        LogMessage?.Invoke($"[Lifeline] ATH - Second button click, ending lifeline");
        
        // Play ATH end sound (immediate priority to stop the bed music)
        _soundService.PlaySound(SoundEffect.LifelineATHEnd);
        
        // Mark lifeline as used and disable button
        _gameService.UseLifeline(LifelineType.AskTheHost);
        _screenService.SetLifelineIcon(buttonNumber, LifelineType.AskTheHost, LifelineIconState.Used);
        ButtonStateChanged?.Invoke(buttonNumber, Color.Gray, false);
        
        // Reset multi-stage state
        _isMultiStageActive = false;
        SetOtherButtonsToStandby?.Invoke(buttonNumber, false);
        
        _athStage = ATHStage.Completed;
        
        LogMessage?.Invoke($"[Lifeline] ATH completed - answer buttons resume normal operation");
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
        
        // IMMEDIATE priority will auto-stop current sounds
        
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
        _pafTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        _pafTimer?.Dispose();
        _ataTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        _ataTimer?.Dispose();
    }
    
    /// <summary>
    /// Resets all lifeline state to initial values
    /// </summary>
    public void Reset()
    {
        // Stop and dispose timers
        _pafTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        _pafTimer?.Dispose();
        _pafTimer = null;
        _ataTimer?.Change(Timeout.Infinite, Timeout.Infinite);
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
    
    #region Lifeline Icon Ping Animation
    
    /// <summary>
    /// Activate a lifeline icon (turn yellow/bling) without sound - for lifeline execution
    /// </summary>
    private void ActivateLifelineIcon(int lifelineNumber, LifelineType type)
    {
        LogMessage?.Invoke($"[LifelineManager] Activating lifeline icon {lifelineNumber} ({type})");
        
        // Set icon to bling state (no timer, stays until changed)
        _screenService.SetLifelineIcon(lifelineNumber, type, LifelineIconState.Bling);
    }
    
    /// <summary>
    /// Ping a lifeline icon (turn yellow/bling for 2 seconds) - for demo mode
    /// </summary>
    public async void PingLifelineIcon(int lifelineNumber, LifelineType type)
    {
        LogMessage?.Invoke($"[LifelineManager] Pinging lifeline {lifelineNumber} ({type})");
        
        // Set icon to bling state
        _screenService.SetLifelineIcon(lifelineNumber, type, LifelineIconState.Bling);
        
        // Cancel any existing ping for this lifeline
        if (_activePings.TryGetValue(lifelineNumber, out var existing))
        {
            existing.cts.Cancel();
            existing.cts.Dispose();
        }
        
        // Create cancellation token for this ping
        var cts = new CancellationTokenSource();
        _activePings[lifelineNumber] = (type, cts);
        
        // Play ping sound based on lifeline number
        SoundEffect soundEffect = lifelineNumber switch
        {
            1 => SoundEffect.LifelinePing1,
            2 => SoundEffect.LifelinePing2,
            3 => SoundEffect.LifelinePing3,
            4 => SoundEffect.LifelinePing4,
            _ => SoundEffect.LifelinePing1
        };
        _soundService.PlaySound(soundEffect);
        
        // Wait 2 seconds then return to normal
        try
        {
            await Task.Delay(2000, cts.Token);
            
            // Remove from dictionary and return icon to normal state
            if (_activePings.TryGetValue(lifelineNumber, out var ping) && ping.cts == cts)
            {
                _activePings.Remove(lifelineNumber);
                _screenService.SetLifelineIcon(lifelineNumber, type, LifelineIconState.Normal);
            }
        }
        catch (TaskCanceledException)
        {
            // Ping was cancelled, do nothing
        }
        finally
        {
            cts.Dispose();
        }
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
