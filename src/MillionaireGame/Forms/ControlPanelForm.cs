using MillionaireGame.Core.Game;
using MillionaireGame.Core.Settings;
using MillionaireGame.Core.Database;
using MillionaireGame.Core.Models;
using MillionaireGame.Services;
using MillionaireGame.Core.Helpers;
using MillionaireGame.Hosting;
using MillionaireGame.Utilities;
using MillionaireGame.Web.Models;
using MillionaireGame.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MillionaireGame.Forms;

/// <summary>
/// Lifeline mode states
/// </summary>
public enum LifelineMode
{
    Inactive,  // Before Explain Game (grey, disabled)
    Demo,      // During Explain Game (yellow, clickable for demo)
    Standby,   // After Lights Down but before all answers revealed (orange, not clickable)
    Active     // After all 4 answers revealed (green, clickable, then grey when used)
}

/// <summary>
/// Closing sequence stages
/// </summary>
public enum ClosingStage
{
    NotStarted,     // Closing not started
    GameOver,       // Red - Playing game over sound (if round in progress)
    Underscore,     // Orange - Playing closing underscore
    Theme,          // Yellow - Playing closing theme
    Complete        // Green - Sequence complete, ready to reset
}

/// <summary>
/// Game outcome states for determining final winnings
/// </summary>
public enum GameOutcome
{
    InProgress,     // Game is still ongoing
    Win,            // Player won Q15
    Drop,           // Player walked away
    Wrong           // Player answered incorrectly
}

/// <summary>
/// Event arguments for host messages
/// </summary>
public class HostMessageEventArgs : EventArgs
{
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Main control panel for managing the game
/// </summary>
public partial class ControlPanelForm : Form
{
    private readonly GameService _gameService;
    private readonly ApplicationSettingsManager _appSettings;
    private readonly SqlSettingsManager _sqlSettings;
    private readonly QuestionRepository _questionRepository;
    private readonly HotkeyHandler _hotkeyHandler;
    private readonly ScreenUpdateService _screenService;
    private readonly SoundService _soundService;
    private readonly StreamDeckService _streamDeckService;
    private StreamDeckIntegration? _streamDeckIntegration;
    private string _currentAnswer = string.Empty;
    private LifelineMode _lifelineMode = LifelineMode.Inactive;
    private string? _currentFinalAnswerKey = null; // Track the final answer sound key for stopping
    private string? _currentLightsDownIdentifier = null; // Track the lights down sound identifier (GUID) for stopping
    
    // Money Tree demo animation state tracking
    private System.Windows.Forms.Timer? _moneyTreeDemoTimer;
    private int _moneyTreeDemoLevel = 0;
    private bool _isMoneyTreeDemoActive = false;
    private bool _isExplainGameActive = false; // Track if Explain Game mode is active
    private const int MONEY_TREE_DEMO_INTERVAL = 500; // milliseconds between level changes
    
    // Safety Net Lock-In Animation state tracking
    private System.Windows.Forms.Timer? _safetyNetAnimationTimer;
    private int _safetyNetAnimationLevel = 0;
    private int _safetyNetFlashCount = 0;
    private bool _safetyNetFlashState = false; // true = highlighted, false = normal
    private int _pendingSafetyNetLevel = 0; // Tracks if Q5 (5) or Q10 (10) just passed and lock-in is available
    private int _safetyNetAnimationTargetLevel = 0; // Level to stay on after animation completes
    private bool _safetyNetAnimationPlaySound = true; // Whether to play sound during animation
    private const int SAFETY_NET_FLASH_INTERVAL = 300; // milliseconds between flashes
    private const int SAFETY_NET_FLASH_TOTAL = 6; // total number of flashes (3 on, 3 off)
    
    // Question reveal state tracking
    private int _answerRevealStep = 0; // 0 = not started, 1 = question shown, 2-5 = answers A-D shown
    private Question? _currentQuestion = null; // Store current question for progressive reveal
    
    // Game outcome tracking
    private GameOutcome _gameOutcome = GameOutcome.InProgress;
    private CancellationTokenSource? _automatedSequenceCts = null;
    private CancellationTokenSource? _lightsDownCts = null;
    private string? _finalWinningsAmount = null; // Store final winnings for end-of-round display
    
    // Track if automated sequence is running (walk away, thanks for playing)
    private bool _isAutomatedSequenceRunning = false;
    
    // Track if current round has completed (win/loss/walk away)
    private bool _roundCompleted = false;
    
    // Lifeline manager handles all lifeline logic
    private LifelineManager? _lifelineManager;
    
    // Telemetry services for game statistics
    private readonly TelemetryService _telemetryService = TelemetryService.Instance;
    private readonly TelemetryExportService _telemetryExportService = new TelemetryExportService();
    
    // Closing sequence state tracking
    private System.Windows.Forms.Timer? _closingTimer;
    private ClosingStage _closingStage = ClosingStage.NotStarted;
    
    // Track round number (0 = no rounds, 1+ = round count)
    private int _roundNumber = 0;
    
    // Track if bed music should be restarted after lifeline use (for Q1-5)
    private bool _shouldRestartBedMusic = false;

    // Host messaging event
    public event EventHandler<HostMessageEventArgs>? MessageSent;

    // Screen forms
    private HostScreenForm? _hostScreen;
    private GuestScreenForm? _guestScreen;
    private IGameScreen? _tvScreen;
    private PreviewScreenForm? _previewScreen;
    private PreviewOrientation _lastPreviewOrientation = PreviewOrientation.Vertical;
    
    // FFF window for audience participation
    private FFFWindow? _fffWindow;
    
    // Web server for audience participation
    private WebServerHost? _webServerHost;
    public WebServerHost? WebServerHost => _webServerHost;
    public bool IsWebServerRunning => _webServerHost != null && _webServerHost.IsRunning;
    
    // Shutdown flag to prevent re-entry
    private bool _isShuttingDown = false;

    // Helper methods to access stop images from Designer
    private static Image? GetRedStopImage()
    {
        return _stopImageNormal; // Red for disabled state
    }

    private static Image? GetBlackStopImage()
    {
        return _stopImageWhite; // Actually black, named _stopImageWhite for compatibility
    }

    public ControlPanelForm(
        GameService gameService,
        ApplicationSettingsManager appSettings,
        SqlSettingsManager sqlSettings,
        QuestionRepository questionRepository,
        ScreenUpdateService screenService,
        SoundService soundService)
    {
        _gameService = gameService;
        _appSettings = appSettings;
        _sqlSettings = sqlSettings;
        _questionRepository = questionRepository;
        _screenService = screenService;
        _soundService = soundService;
        
        // Initialize Stream Deck service (always created, but only enabled if setting is on)
        _streamDeckService = new StreamDeckService();
        
        // Initialize lifeline manager with WebServerHost accessor
        _lifelineManager = new LifelineManager(
            gameService, 
            soundService, 
            screenService, 
            () => _webServerHost);
        _lifelineManager.ButtonStateChanged += OnLifelineButtonStateChanged;
        _lifelineManager.SetOtherButtonsToStandby += OnSetOtherButtonsToStandby;
        _lifelineManager.RequestAsyncOperation += OnLifelineRequestAsyncOperation;
        _lifelineManager.RequestAnswerRemoval += OnLifelineRequestAnswerRemoval;
        _lifelineManager.LogMessage += OnLifelineLogMessage;
        _lifelineManager.RequestBedMusicRestart += OnLifelineBedMusicRestart;
        
        // Sounds are already loaded in Program.cs during initialization
        
        // Initialize hotkey handler
        _hotkeyHandler = new HotkeyHandler(
            onF1: () => btnA.PerformClick(),
            onF2: () => btnB.PerformClick(),
            onF3: () => btnC.PerformClick(),
            onF4: () => btnD.PerformClick(),
            onF5: () => btnNewQuestion.PerformClick(),
            onF6: () => btnReveal.PerformClick(),
            onF7: () => btnLightsDown.PerformClick(),
            onF8: () => btnLifeline1.PerformClick(),
            onF9: () => btnLifeline2.PerformClick(),
            onF10: () => btnLifeline3.PerformClick(),
            onF11: () => btnLifeline4.PerformClick(),
            onPageUp: LevelUp,
            onPageDown: LevelDown,
            onEnd: () => btnWalk.PerformClick(),
            onR: () => btnActivateRiskMode.PerformClick()
        );

        InitializeComponent();
        IconHelper.ApplyToForm(this);
        KeyPreview = true; // Enable key event capture
        SetupEventHandlers();
        
        // Initialize Stream Deck integration if enabled
        InitializeStreamDeck();
        
        // Handle form closing to ensure proper cleanup
        FormClosing += ControlPanelForm_FormClosing;
    }
    
    private void ControlPanelForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        // Prevent re-entry
        if (_isShuttingDown)
            return;
            
        // Don't allow normal closing - we need to run async shutdown
        if (e.CloseReason == CloseReason.UserClosing || e.CloseReason == CloseReason.ApplicationExitCall)
        {
            _isShuttingDown = true;
            e.Cancel = true;
            
            // Run shutdown sequence in background
            Task.Run(async () => await ShutdownApplicationAsync());
        }
    }

    /// <summary>
    /// Properly shut down all application components with progress tracking
    /// </summary>
    private async Task ShutdownApplicationAsync()
    {
        ShutdownProgressDialog? progressDialog = null;
        var globalStopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            // Create and show progress dialog on UI thread
            await Task.Run(() =>
            {
                Invoke(new Action(() =>
                {
                    progressDialog = new ShutdownProgressDialog();
                    progressDialog.Show();
                }));
            });

            if (progressDialog == null)
            {
                GameConsole.Error("[Shutdown] Failed to create progress dialog");
                ForceApplicationExit();
                return;
            }

            GameConsole.Info("========================================");
            GameConsole.Info("[Shutdown] Beginning application shutdown");
            GameConsole.Info("========================================");

            // Step 1: Stop audio playback
            var stepStopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                progressDialog.UpdateStatus("Stopping audio playback...");
                await _soundService.StopAllSoundsAsync();
                stepStopwatch.Stop();
                progressDialog.AddStep("Stop Audio Playback", true, stepStopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stepStopwatch.Stop();
                progressDialog.AddStep("Stop Audio Playback", false, stepStopwatch.ElapsedMilliseconds);
                progressDialog.AddMessage($"  Error: {ex.Message}");
                GameConsole.Error($"[Shutdown] Error stopping audio: {ex.Message}");
            }

            // Step 2: Dispose audio system
            stepStopwatch.Restart();
            try
            {
                progressDialog.UpdateStatus("Disposing audio system...");
                _soundService.Dispose();
                stepStopwatch.Stop();
                progressDialog.AddStep("Dispose Audio System", true, stepStopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stepStopwatch.Stop();
                progressDialog.AddStep("Dispose Audio System", false, stepStopwatch.ElapsedMilliseconds);
                progressDialog.AddMessage($"  Error: {ex.Message}");
                GameConsole.Error($"[Shutdown] Error disposing audio: {ex.Message}");
            }

            // Step 3: Stop web server if running
            if (_webServerHost != null)
            {
                stepStopwatch.Restart();
                try
                {
                    progressDialog.UpdateStatus("Stopping web server...");
                    await _webServerHost.StopAsync();
                    stepStopwatch.Stop();
                    progressDialog.AddStep("Stop Web Server", true, stepStopwatch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    stepStopwatch.Stop();
                    progressDialog.AddStep("Stop Web Server", false, stepStopwatch.ElapsedMilliseconds);
                    progressDialog.AddMessage($"  Error: {ex.Message}");
                    GameConsole.Error($"[Shutdown] Error stopping web server: {ex.Message}");
                }
            }
            else
            {
                progressDialog.AddMessage("Web server not running - skipped");
            }

            // Step 4: Close child windows
            stepStopwatch.Restart();
            try
            {
                progressDialog.UpdateStatus("Closing child windows...");
                await Task.Run(() =>
                {
                    Invoke(new Action(() =>
                    {
                        // Close FFF window if open
                        if (_fffWindow != null && !_fffWindow.IsDisposed)
                        {
                            _fffWindow.Close();
                            _fffWindow.Dispose();
                        }
                    }));
                });
                stepStopwatch.Stop();
                progressDialog.AddStep("Close Child Windows", true, stepStopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stepStopwatch.Stop();
                progressDialog.AddStep("Close Child Windows", false, stepStopwatch.ElapsedMilliseconds);
                progressDialog.AddMessage($"  Error: {ex.Message}");
                GameConsole.Error($"[Shutdown] Error closing child windows: {ex.Message}");
            }

            // Step 5: Shutdown Stream Deck integration
            stepStopwatch.Restart();
            try
            {
                progressDialog.UpdateStatus("Shutting down Stream Deck...");
                if (_streamDeckIntegration != null)
                {
                    _streamDeckIntegration.Shutdown();
                    _streamDeckIntegration.Dispose();
                }
                stepStopwatch.Stop();
                progressDialog.AddStep("Shutdown Stream Deck", true, stepStopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stepStopwatch.Stop();
                progressDialog.AddStep("Shutdown Stream Deck", false, stepStopwatch.ElapsedMilliseconds);
                progressDialog.AddMessage($"  Error: {ex.Message}");
                GameConsole.Error($"[Shutdown] Error shutting down Stream Deck: {ex.Message}");
            }

            // Step 6: Stop timers
            stepStopwatch.Restart();
            try
            {
                progressDialog.UpdateStatus("Stopping timers...");
                _moneyTreeDemoTimer?.Stop();
                _safetyNetAnimationTimer?.Stop();
                _closingTimer?.Stop();
                stepStopwatch.Stop();
                progressDialog.AddStep("Stop Timers", true, stepStopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stepStopwatch.Stop();
                progressDialog.AddStep("Stop Timers", false, stepStopwatch.ElapsedMilliseconds);
                progressDialog.AddMessage($"  Error: {ex.Message}");
                GameConsole.Error($"[Shutdown] Error stopping timers: {ex.Message}");
            }

            // Step 7: Dispose lifeline manager
            stepStopwatch.Restart();
            try
            {
                progressDialog.UpdateStatus("Disposing lifeline manager...");
                _lifelineManager?.Dispose();
                stepStopwatch.Stop();
                progressDialog.AddStep("Dispose Lifeline Manager", true, stepStopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stepStopwatch.Stop();
                progressDialog.AddStep("Dispose Lifeline Manager", false, stepStopwatch.ElapsedMilliseconds);
                progressDialog.AddMessage($"  Error: {ex.Message}");
                GameConsole.Error($"[Shutdown] Error disposing lifeline manager: {ex.Message}");
            }

            // Step 7: Shutdown console windows
            stepStopwatch.Restart();
            try
            {
                progressDialog.UpdateStatus("Shutting down console windows...");
                GameConsole.Shutdown();
                WebServerConsole.Shutdown();
                stepStopwatch.Stop();
                progressDialog.AddStep("Shutdown Consoles", true, stepStopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stepStopwatch.Stop();
                progressDialog.AddStep("Shutdown Consoles", false, stepStopwatch.ElapsedMilliseconds);
                progressDialog.AddMessage($"  Error: {ex.Message}");
            }

            globalStopwatch.Stop();
            progressDialog.AddMessage("");
            progressDialog.AddMessage($"Total shutdown time: {globalStopwatch.ElapsedMilliseconds}ms");

            // Mark as complete
            progressDialog.Complete();

            // Wait for dialog to auto-close or force close
            await Task.Delay(2000);

            // Check if user requested force close
            if (progressDialog.ForceClose)
            {
                ForceApplicationExit();
            }
            else
            {
                // Normal exit
                Invoke(new Action(() =>
                {
                    Application.Exit();
                }));
            }
        }
        catch (Exception ex)
        {
            GameConsole.Error($"[Shutdown] CRITICAL ERROR during shutdown: {ex.Message}");
            GameConsole.Error($"[Shutdown] Stack trace: {ex.StackTrace}");
            
            if (progressDialog != null)
            {
                progressDialog.Failed(ex.Message);
                await Task.Delay(3000); // Give user time to read error
            }

            ForceApplicationExit();
        }
    }

    /// <summary>
    /// Force immediate application exit when normal shutdown fails
    /// </summary>
    private void ForceApplicationExit()
    {
        try
        {
            GameConsole.Error("[Shutdown] Force exiting application");
            Environment.Exit(1);
        }
        catch
        {
            // Last resort - kill the process
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }

    private void SetupEventHandlers()
    {
        // Subscribe to game service events
        _gameService.LevelChanged += OnLevelChanged;
        _gameService.ModeChanged += OnModeChanged;
        _gameService.LifelineUsed += OnLifelineUsed;
    }
    
    /// <summary>
    /// Initialize Stream Deck integration if enabled in settings
    /// </summary>
    private void InitializeStreamDeck()
    {
        try
        {
            // Check if Stream Deck is enabled in settings
            if (!_appSettings.Settings.StreamDeckEnabled)
            {
                GameConsole.Info("[ControlPanel] Stream Deck integration disabled in settings");
                return;
            }
            
            // Create integration bridge
            _streamDeckIntegration = new StreamDeckIntegration(
                _streamDeckService,
                _gameService,
                _screenService);
            
            // Wire up events
            _streamDeckIntegration.AnswerLockedByHost += OnStreamDeckAnswerLocked;
            _streamDeckIntegration.RevealTriggeredByHost += OnStreamDeckRevealTriggered;
            _streamDeckIntegration.DeviceStatusChanged += OnStreamDeckDeviceStatusChanged;
            
            // Attempt initialization
            if (_streamDeckIntegration.Initialize())
            {
                GameConsole.Info("[ControlPanel] Stream Deck integration initialized successfully");
            }
            else
            {
                GameConsole.Warn("[ControlPanel] Stream Deck device not found");
            }
        }
        catch (Exception ex)
        {
            GameConsole.Error($"[ControlPanel] Stream Deck initialization error: {ex.Message}");
        }
    }
    
    #region Stream Deck Event Handlers
    
    /// <summary>
    /// Called when host locks in an answer via Stream Deck
    /// </summary>
    private void OnStreamDeckAnswerLocked(object? sender, AnswerLockedEventArgs e)
    {
        GameConsole.Info($"[ControlPanel] Stream Deck answer locked: {e.Answer} ({(e.IsCorrect ? "CORRECT" : "INCORRECT")})");
        
        // Stream Deck events fire on background thread - must invoke to UI thread
        if (InvokeRequired)
        {
            Invoke(new Action(() => OnStreamDeckAnswerLocked(sender, e)));
            return;
        }
        
        // Simulate clicking the corresponding answer button
        _currentAnswer = e.Answer.ToString();
        
        Button? targetButton = e.Answer switch
        {
            'A' => btnA,
            'B' => btnB,
            'C' => btnC,
            'D' => btnD,
            _ => null
        };
        
        if (targetButton != null && targetButton.Enabled && targetButton.Visible)
        {
            GameConsole.Debug($"[ControlPanel] Triggering answer button {e.Answer} click");
            targetButton.PerformClick();
        }
        else
        {
            GameConsole.Warn($"[ControlPanel] Answer button {e.Answer} not available");
        }
    }
    
    /// <summary>
    /// Called when host presses Reveal button on Stream Deck
    /// </summary>
    private void OnStreamDeckRevealTriggered(object? sender, EventArgs e)
    {
        GameConsole.Info("[ControlPanel] Stream Deck reveal triggered");
        
        // Stream Deck events fire on background thread - must invoke to UI thread
        if (InvokeRequired)
        {
            Invoke(new Action(() => OnStreamDeckRevealTriggered(sender, e)));
            return;
        }
        
        // Simulate clicking the Reveal button
        if (btnReveal.Enabled && btnReveal.Visible)
        {
            GameConsole.Debug("[ControlPanel] Triggering reveal button click");
            btnReveal.PerformClick();
        }
        else
        {
            GameConsole.Warn("[ControlPanel] Reveal button not available");
        }
    }
    
    /// <summary>
    /// Called when Stream Deck device connection status changes
    /// </summary>
    private void OnStreamDeckDeviceStatusChanged(object? sender, DeviceStatusEventArgs e)
    {
        if (e.IsConnected)
        {
            GameConsole.Info("[ControlPanel] Stream Deck device connected");
        }
        else
        {
            GameConsole.Warn("[ControlPanel] Stream Deck device disconnected");
        }
    }
    
    #endregion
    
    #region Lifeline Manager Event Handlers
    
    private void OnLifelineButtonStateChanged(int buttonNumber, Color color, bool enabled)
    {
        var button = buttonNumber switch
        {
            1 => btnLifeline1,
            2 => btnLifeline2,
            3 => btnLifeline3,
            4 => btnLifeline4,
            _ => null
        };
        
        if (button != null)
        {
            button.BackColor = color;
            button.Enabled = enabled;
        }
    }
    
    private void OnSetOtherButtonsToStandby(int activeButtonNumber, bool isStandby)
    {
        // Set all buttons except the active one to standby (orange) or back to normal (LimeGreen)
        var buttons = new[] { 
            (1, btnLifeline1), 
            (2, btnLifeline2), 
            (3, btnLifeline3), 
            (4, btnLifeline4) 
        };
        
        foreach (var (number, button) in buttons)
        {
            if (number == activeButtonNumber || !button.Visible)
                continue;
                
            var lifeline = _gameService.State.GetLifeline(GetLifelineTypeFromSettings(number));
            if (lifeline != null && lifeline.IsUsed)
                continue; // Don't change color of already-used lifelines (grey)
            
            // Set to standby (orange) or back to normal (LimeGreen)
            button.BackColor = isStandby ? Color.Orange : Color.LimeGreen;
        }
    }
    
    private void OnLifelineRequestAsyncOperation(Func<Task> operation)
    {
        // Execute the async operation (typically LoadAndDisplayQuestionAsync for STQ)
        // Use core method without audio handling - lifeline handles its own sound
        // Note: Bed music continues playing, no need to restart it
        Task.Run(async () =>
        {
            await LoadAndDisplayQuestionAsync();
            
            // Set up for progressive answer reveal
            // This ensures the Question button is enabled to reveal answers one by one
            this.Invoke(() =>
            {
                _answerRevealStep = 1; // Question loaded, ready to reveal answers
                chkShowQuestion.Checked = true;
                btnNewQuestion.Enabled = true;
                btnNewQuestion.BackColor = Color.LimeGreen;
                btnNewQuestion.Text = "Question";
            });
        });
    }
    
    private void OnLifelineRequestAnswerRemoval(string answer, string unused)
    {
        // Disable and grey out button on control panel for 50:50
        switch (answer)
        {
            case "A":
                btnA.Enabled = false;
                btnA.BackColor = Color.Gray;
                break;
            case "B":
                btnB.Enabled = false;
                btnB.BackColor = Color.Gray;
                break;
            case "C":
                btnC.Enabled = false;
                btnC.BackColor = Color.Gray;
                break;
            case "D":
                btnD.Enabled = false;
                btnD.BackColor = Color.Gray;
                break;
        }
    }
    
    private void OnLifelineLogMessage(string message)
    {
        if (Program.DebugMode)
        {
            GameConsole.Log(message);
        }
    }
    
    private void OnLifelineBedMusicRestart()
    {
        // Lifeline was used on Q1-5, so bed music should restart after correct answer
        _shouldRestartBedMusic = true;
    }
    
    #endregion

    private void ControlPanelForm_Load(object? sender, EventArgs e)
    {
        // Set initial window title with debug mode indicator
        UpdateWindowTitle();
        
        // Initialize game to level 0
        _gameService.ChangeLevel(0);
        UpdateMoneyDisplay();
        
        // Disable question level selector until Pick a Player is clicked
        nmrLevel.Enabled = false;
        
        // Set lifelines to inactive mode (disabled, not clickable)
        SetLifelineMode(LifelineMode.Inactive);
        
        // Update lifeline button labels based on settings
        UpdateLifelineButtonLabels();
        
        // Update menu item enabled states based on settings
        UpdateScreenMenuItemStates();
        
        // Initialize web server host first
        InitializeWebServer();
        
        // Initialize WebService console only if in debug mode and web server will be started
        // (before auto-start so it doesn't steal focus)
        if (Program.DebugMode && _appSettings.Settings.AudienceServerAutoStart)
        {
            WebServerConsole.Show();
        }
        
        // Auto-start web server if enabled in settings
        if (_appSettings.Settings.AudienceServerAutoStart)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    var ip = _appSettings.Settings.AudienceServerIP;
                    var port = _appSettings.Settings.AudienceServerPort;
                    await StartWebServerAsync(ip, port);
                }
                catch (Exception ex)
                {
                    this.Invoke(() =>
                    {
                        GameConsole.Error($"[Web Server] Failed to auto-start web server: {ex.Message}");
                    });
                }
            });
        }
        
        // Auto-show Preview Screen after web server (so it loads in correct order)
        if (_appSettings.Settings.EnablePreviewAutomatically)
        {
            PreviewScreenToolStripMenuItem_Click(null, EventArgs.Empty);
        }
        
        // Initialize GameConsole LAST (after all screens are shown)
        // This prevents it from stealing focus and ensures proper icon loading
        // Only auto-show in debug mode; otherwise user must manually open via menu
        if (Program.DebugMode)
        {
            var gameConsoleWindow = new GameConsoleWindow();
            gameConsoleWindow.Show();
            GameConsole.SetWindow(gameConsoleWindow);
            
            if (Program.DebugMode)
            {
                GameConsole.Info("===== THE MILLIONAIRE GAME - Debug Console =====");
                GameConsole.Info($"Version: v0.9.8 Debug Build");
                GameConsole.Info($"Started: {DateTime.Now}");
                GameConsole.Warn("⚠ DEBUG MODE IS ACTIVE - Verbose logging enabled");
            }
            else
            {
                GameConsole.Info("===== THE MILLIONAIRE GAME - Console =====");
                GameConsole.Info($"Version: v0.9.8");
                GameConsole.Info($"Started: {DateTime.Now}");
            }
            GameConsole.LogSeparator();
            GameConsole.Info("Application initialized successfully.");
        }
        
        // Bring Control Panel to front after everything else loads
        // Use BeginInvoke to let all other windows finish loading first
        this.BeginInvoke(new Action(() =>
        {
            System.Threading.Thread.Sleep(100); // Small delay to ensure preview screens are fully loaded
            this.Activate();
            this.BringToFront();
            this.Focus();
        }));
    }
    
    /// <summary>
    /// Updates the enabled state of screen menu items based on Full Screen checkbox settings
    /// </summary>
    public void UpdateScreenMenuItemStates()
    {
        // Menu items are always enabled regardless of monitor count
        // This allows screens to be opened in windowed mode for:
        // - Development/debug purposes
        // - Screen capture/streaming applications
        // - Window sharing for broadcast
        // The FullScreen settings only control whether screens open in fullscreen mode
        hostScreenMenuItem.Enabled = true;
        guestScreenMenuItem.Enabled = true;
        tvScreenMenuItem.Enabled = true;
    }
    
    #region Full Screen Management
    
    /// <summary>
    /// Apply full-screen settings to host screen
    /// </summary>
    public void ApplyFullScreenToHostScreen(bool fullScreen, int monitorIndex)
    {
        if (_hostScreen == null || _hostScreen.IsDisposed) return;
        
        if (fullScreen)
        {
            // Set to full-screen on specified monitor
            var screens = Screen.AllScreens;
            if (monitorIndex >= 0 && monitorIndex < screens.Length)
            {
                var targetScreen = screens[monitorIndex];
                _hostScreen.FormBorderStyle = FormBorderStyle.None;
                _hostScreen.WindowState = FormWindowState.Normal; // Reset before setting bounds
                _hostScreen.Bounds = targetScreen.Bounds;
                _hostScreen.WindowState = FormWindowState.Maximized;
            }
        }
        else
        {
            // Revert to windowed mode
            _hostScreen.FormBorderStyle = FormBorderStyle.Sizable;
            _hostScreen.WindowState = FormWindowState.Normal;
            _hostScreen.StartPosition = FormStartPosition.CenterScreen;
        }
    }
    
    /// <summary>
    /// Apply full-screen settings to guest screen
    /// </summary>
    public void ApplyFullScreenToGuestScreen(bool fullScreen, int monitorIndex)
    {
        if (_guestScreen == null || _guestScreen.IsDisposed) return;
        
        if (fullScreen)
        {
            // Set to full-screen on specified monitor
            var screens = Screen.AllScreens;
            if (monitorIndex >= 0 && monitorIndex < screens.Length)
            {
                var targetScreen = screens[monitorIndex];
                _guestScreen.FormBorderStyle = FormBorderStyle.None;
                _guestScreen.WindowState = FormWindowState.Normal; // Reset before setting bounds
                _guestScreen.Bounds = targetScreen.Bounds;
                _guestScreen.WindowState = FormWindowState.Maximized;
            }
        }
        else
        {
            // Revert to windowed mode
            _guestScreen.FormBorderStyle = FormBorderStyle.Sizable;
            _guestScreen.WindowState = FormWindowState.Normal;
            _guestScreen.StartPosition = FormStartPosition.CenterScreen;
        }
    }
    
    /// <summary>
    /// Apply full-screen settings to TV screen
    /// </summary>
    public void ApplyFullScreenToTVScreen(bool fullScreen, int monitorIndex)
    {
        if (_tvScreen == null) return;
        
        var tvForm = _tvScreen as Form;
        if (tvForm == null || tvForm.IsDisposed) return;
        
        if (fullScreen)
        {
            // Set to full-screen on specified monitor
            var screens = Screen.AllScreens;
            if (monitorIndex >= 0 && monitorIndex < screens.Length)
            {
                var targetScreen = screens[monitorIndex];
                tvForm.FormBorderStyle = FormBorderStyle.None;
                tvForm.WindowState = FormWindowState.Normal; // Reset before setting bounds
                tvForm.Bounds = targetScreen.Bounds;
                tvForm.WindowState = FormWindowState.Maximized;
            }
        }
        else
        {
            // Revert to windowed mode
            tvForm.FormBorderStyle = FormBorderStyle.Sizable;
            tvForm.WindowState = FormWindowState.Normal;
            tvForm.StartPosition = FormStartPosition.CenterScreen;
        }
    }
    
    #endregion

    #region Web Server / Audience Participation

    private void InitializeWebServer()
    {
        try
        {
            // Get the SQL connection string from settings
            var connectionString = _sqlSettings.Settings.GetConnectionString("dbMillionaire");
            
            // Create web server host
            _webServerHost = new WebServerHost(connectionString);
            
            // Subscribe to server events
            _webServerHost.ServerStarted += OnWebServerStarted;
            _webServerHost.ServerStopped += OnWebServerStopped;
            _webServerHost.ServerError += OnWebServerError;
        }
        catch (Exception ex)
        {
            GameConsole.Error($"[Web Server] Failed to initialize web server: {ex.Message}");
        }
    }

    public async Task StartWebServerAsync(string ipAddress, int port)
    {
        if (_webServerHost == null)
        {
            InitializeWebServer();
        }
        
        if (_webServerHost == null)
        {
            throw new InvalidOperationException("Web server host is not initialized.");
        }
        
        if (_webServerHost.IsRunning)
        {
            throw new InvalidOperationException("Web server is already running.");
        }
        
        await _webServerHost.StartAsync(ipAddress, port);
    }

    public async Task StopWebServerAsync()
    {
        if (_webServerHost == null || !_webServerHost.IsRunning)
        {
            return;
        }
        
        await _webServerHost.StopAsync();
    }

    private void OnWebServerStarted(object? sender, string baseUrl)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => OnWebServerStarted(sender, baseUrl)));
            return;
        }
        
        // Log to WebService console
        WebServerConsole.LogSeparator();
        WebServerConsole.Info("Server started successfully");
        WebServerConsole.Info($"URL: {baseUrl}");
        WebServerConsole.LogSeparator();
        
        // Log to console or status bar if available
        UpdateWindowTitle(baseUrl);
    }

    private void OnWebServerStopped(object? sender, EventArgs e)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => OnWebServerStopped(sender, e)));
            return;
        }
        
        // Log to WebService console
        WebServerConsole.LogSeparator();
        WebServerConsole.Info("Server stopped");
        WebServerConsole.LogSeparator();
        
        // Hide WebService console when server stops
        WebServerConsole.Hide();
        
        // Reset title
        UpdateWindowTitle();
    }

    private void OnWebServerError(object? sender, Exception ex)
    {
        // Both WebServerConsole and GameConsole are thread-safe with internal queuing
        // No UI thread marshalling needed
        WebServerConsole.Error($"Ã¢ÂÅ’ Error: {ex.Message}");
        GameConsole.Error($"[Web Server] {ex.Message}");
    }
    
    /// <summary>
    /// Update the window title with optional web server URL and debug mode indicator
    /// </summary>
    private void UpdateWindowTitle(string? webServerUrl = null)
    {
        string baseTitle = "The Millionaire Game - Control Panel";
        
        if (!string.IsNullOrEmpty(webServerUrl))
        {
            baseTitle += $" [Web Server: {webServerUrl}]";
        }
        
        if (Program.DebugMode)
        {
            baseTitle += " - DEBUG ENABLED";
        }
        
        Text = baseTitle;
    }

    #endregion

    #region Event Handlers

    private void OnLevelChanged(object? sender, GameLevelChangedEventArgs e)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => OnLevelChanged(sender, e)));
            return;
        }

        nmrLevel.Value = e.NewLevel;
        UpdateMoneyDisplay();
        UpdateRiskModeButton();
        
        // Update money tree on all screens - show winnings (level - 1), not current level
        var displayLevel = _gameService.MoneyTree.GetDisplayLevel(e.NewLevel, _gameService.State.GameWin);
        UpdateMoneyTreeOnScreens(displayLevel);
    }

    private void OnModeChanged(object? sender, GameModeChangedEventArgs e)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => OnModeChanged(sender, e)));
            return;
        }

        UpdateRiskModeButton();
    }
    
    /// <summary>
    /// Updates risk mode button appearance based on safety net configuration
    /// </summary>
    private void UpdateRiskModeButton()
    {
        var settings = _gameService.MoneyTree.Settings;
        bool isNet5Active = (settings.SafetyNet1 == 5 || settings.SafetyNet2 == 5);
        bool isNet10Active = (settings.SafetyNet1 == 10 || settings.SafetyNet2 == 10);
        
        // If already in Risk Mode, keep button disabled and red
        if (_gameService.State.Mode == Core.Models.GameMode.Risk)
        {
            btnActivateRiskMode.BackColor = Color.Red;
            btnActivateRiskMode.Text = "RISK MODE: ON";
            btnActivateRiskMode.Enabled = false; // Stay disabled once activated
            return;
        }
        
        // Determine button state based on safety net configuration
        if (!isNet5Active && !isNet10Active)
        {
            // Both safety nets disabled = permanent risk mode
            btnActivateRiskMode.BackColor = Color.Red;
            btnActivateRiskMode.Text = "RISK MODE: ON";
            btnActivateRiskMode.Enabled = false; // Unclickable
        }
        else if (!isNet5Active)
        {
            // Q5 safety net disabled
            btnActivateRiskMode.BackColor = Color.Blue;
            btnActivateRiskMode.Text = "RISK MODE: 5";
            btnActivateRiskMode.Enabled = (_gameService.State.CurrentLevel == 0);
        }
        else if (!isNet10Active)
        {
            // Q10 safety net disabled
            btnActivateRiskMode.BackColor = Color.Blue;
            btnActivateRiskMode.Text = "RISK MODE: 10";
            btnActivateRiskMode.Enabled = (_gameService.State.CurrentLevel == 0);
        }
        else
        {
            // Both safety nets active = normal risk mode toggle available at start
            btnActivateRiskMode.BackColor = Color.Yellow;
            btnActivateRiskMode.Text = "Activate Risk Mode";
            btnActivateRiskMode.Enabled = (_gameService.State.CurrentLevel == 0);
        }
    }
    
    /// <summary>
    /// Determines if safety net lock-in is available (not disabled by Risk Mode or unchecked in settings)
    /// </summary>
    private bool CanLockInSafetyNet()
    {
        var settings = _gameService.MoneyTree.Settings;
        var currentMode = _gameService.State.Mode;
        
        // Check if the pending safety net level is actually enabled in settings
        bool isNet5Enabled = (settings.SafetyNet1 == 5 || settings.SafetyNet2 == 5);
        bool isNet10Enabled = (settings.SafetyNet1 == 10 || settings.SafetyNet2 == 10);
        
        // If Q5 is pending but disabled in settings, cannot lock in
        if (_pendingSafetyNetLevel == 5 && !isNet5Enabled)
            return false;
            
        // If Q10 is pending but disabled in settings, cannot lock in
        if (_pendingSafetyNetLevel == 10 && !isNet10Enabled)
            return false;
        
        // Check if safety net is disabled due to Risk Mode
        if (_gameService.MoneyTree.IsSafetyNetDisabledInRiskMode(_pendingSafetyNetLevel, currentMode))
            return false;
        
        return true;
    }
    
    /// <summary>
    /// Updates money tree display on all open screens
    /// </summary>
    private void UpdateMoneyTreeOnScreens(int level)
    {
        var currentMode = _gameService.State.Mode;
        _hostScreen?.UpdateMoneyTreeLevel(level, currentMode);
        _guestScreen?.UpdateMoneyTreeLevel(level, currentMode);
        
        if (_tvScreen is TVScreenForm tvScalable)
        {
            tvScalable.UpdateMoneyTreeLevel(level);
        }
        
        // Update preview screen if open
        _previewScreen?.UpdateMoneyTreeLevel(level);
        
        // Trigger preview screen cache invalidation
        _screenService.TriggerGeneralUpdate();
    }

    private void OnLifelineUsed(object? sender, LifelineUsedEventArgs e)
    {
        // Event handler required by GameService.LifelineUsed subscription - UI updates handled by LifelineManager
    }

    #endregion

    #region Checkbox Event Handlers

    private void chkShowQuestion_CheckedChanged(object? sender, EventArgs e)
    {
        if (chkShowQuestion.Checked && chkShowWinnings.Checked)
        {
            // Uncheck winnings when question is checked
            chkShowWinnings.CheckedChanged -= chkShowWinnings_CheckedChanged;
            chkShowWinnings.Checked = false;
            chkShowWinnings.CheckedChanged += chkShowWinnings_CheckedChanged;
        }
        
        // Show or hide question on screens
        _screenService.ShowQuestion(chkShowQuestion.Checked);
    }

    private void chkShowWinnings_CheckedChanged(object? sender, EventArgs e)
    {
        if (chkShowWinnings.Checked && chkShowQuestion.Checked)
        {
            // Uncheck question when winnings is checked
            chkShowQuestion.CheckedChanged -= chkShowQuestion_CheckedChanged;
            chkShowQuestion.Checked = false;
            chkShowQuestion.CheckedChanged += chkShowQuestion_CheckedChanged;
        }
        
        // Show or hide winnings on screens
        if (chkShowWinnings.Checked)
        {
            // Use final winnings amount if available (end-of-round), otherwise use current value
            var amountToShow = _finalWinningsAmount ?? _gameService.State.CurrentValue;
            _screenService.ShowWinningsAmount(amountToShow);
            // Icons stay visible on Host/Guest screens, hidden on TV
        }
        else
        {
            _screenService.HideWinnings();
            _finalWinningsAmount = null; // Clear stored amount when hiding
        }
    }

    private void chkCorrectAnswer_CheckedChanged(object? sender, EventArgs e)
    {
        // Show or hide correct answer on host screen based on checkbox state
        if (chkCorrectAnswer.Checked)
        {
            // Only show if all answers have been revealed (step 5) and we have a correct answer loaded
            if (_answerRevealStep == 5 && !string.IsNullOrEmpty(lblAnswer.Text))
            {
                _screenService.ShowCorrectAnswerToHost(lblAnswer.Text);
            }
        }
        else
        {
            // Hide the correct answer by clearing it on the host screen
            _hostScreen?.ShowCorrectAnswerToHost(null);
        }
    }

    #endregion

    #region Button Click Handlers

    private async void btnNewQuestion_Click(object? sender, EventArgs e)
    {
        if (_answerRevealStep == 0)
        {
            // Disable button immediately to prevent double-clicks
            btnNewQuestion.Enabled = false;
            
            // First click: Load question but don't show answers yet
            try
            {
                GameConsole.Debug("[NewQuestion] Button clicked, _answerRevealStep = 0");
                
                // Get current question number to determine if we need to stop sounds (now 1-indexed)
                var currentQuestionNumber = (int)nmrLevel.Value;
                
                // Only stop sounds for Q6+ (lights down sound)
                // For Q1-5, bed music continues playing
                if (currentQuestionNumber >= 6 && !string.IsNullOrEmpty(_currentLightsDownIdentifier))
                {
                    GameConsole.Debug($"[NewQuestion] Q{currentQuestionNumber} - stopping lights down sound");
                    await _soundService.StopAllSoundsAsync();
                    _currentLightsDownIdentifier = null;
                }
                else if (currentQuestionNumber <= 5)
                {
                    GameConsole.Debug($"[NewQuestion] Q{currentQuestionNumber} - keeping bed music, not stopping sounds");
                }
                
                GameConsole.Debug("[NewQuestion] Loading new question");
                await LoadNewQuestion();
                
                // Ensure UI updates happen on UI thread
                if (!IsDisposed && IsHandleCreated)
                {
                    BeginInvoke(() =>
                    {
                        if (!IsDisposed)
                        {
                            _answerRevealStep = 1; // Question shown, no answers
                            
                            // Enable checkbox and keep Question button enabled for answer reveals
                            chkShowQuestion.Checked = true;
                            btnNewQuestion.Enabled = true;
                            btnNewQuestion.BackColor = Color.LimeGreen;
                            btnNewQuestion.Text = "Question";
                            GameConsole.Debug("[NewQuestion] UI updated, ready for answer reveals");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                GameConsole.Error($"[NewQuestion] Error loading question: {ex.Message}");
                // Re-enable button on error
                if (!IsDisposed && IsHandleCreated)
                {
                    BeginInvoke(() =>
                    {
                        if (!IsDisposed)
                        {
                            btnNewQuestion.Enabled = true;
                        }
                    });
                }
            }
        }
        else if (_answerRevealStep >= 1 && _answerRevealStep <= 4)
        {
            // Subsequent clicks: Reveal answers one by one
            RevealNextAnswer();
        }
    }

    private void RevealNextAnswer()
    {
        if (_currentQuestion == null) return;

        switch (_answerRevealStep)
        {
            case 1: // Reveal answer A
                txtA.Text = _currentQuestion.AnswerA;
                _screenService.ShowAnswer("A");
                _answerRevealStep = 2;
                break;
            case 2: // Reveal answer B
                txtB.Text = _currentQuestion.AnswerB;
                _screenService.ShowAnswer("B");
                _answerRevealStep = 3;
                break;
            case 3: // Reveal answer C
                txtC.Text = _currentQuestion.AnswerC;
                _screenService.ShowAnswer("C");
                _answerRevealStep = 4;
                break;
            case 4: // Reveal answer D
                txtD.Text = _currentQuestion.AnswerD;
                _screenService.ShowAnswer("D");
                _answerRevealStep = 5; // All answers revealed
                
                // Show correct answer after all answers are revealed
                lblAnswer.Visible = true;
                
                // If checkbox is checked, show correct answer to host screen
                if (chkCorrectAnswer.Checked)
                {
                    _screenService.ShowCorrectAnswerToHost(lblAnswer.Text);
                }
                
                // Activate lifelines (green, clickable) now that all answers are revealed
                SetLifelineMode(LifelineMode.Active);
                
                // Enable Walk Away button once all answers are revealed (green)
                btnWalk.Enabled = true;
                btnWalk.BackColor = Color.LimeGreen;
                btnWalk.ForeColor = Color.Black;
                
                // Disable Question button after all answers shown
                btnNewQuestion.Enabled = false;
                btnNewQuestion.BackColor = Color.Gray;
                
                // Notify Stream Deck that question is fully displayed (all answers revealed)
                _streamDeckIntegration?.OnQuestionDisplayed();
                break;
        }
    }

    /// <summary>
    /// Core method to load and display a question without audio handling.
    /// Use this for lifelines like Switch the Question where audio is handled separately.
    /// </summary>
    private async Task LoadAndDisplayQuestionAsync()
    {
        // Clear any pending safety net lock-in when starting a new question
        _pendingSafetyNetLevel = 0;
        
        // Clear old answers from host/guest screens immediately
        _screenService.ClearQuestionAndAnswerText();
        
        try
        {
            // Use the question number directly from the control (user may have manually set it)
            var currentQuestion = (int)nmrLevel.Value; // Already 1-indexed (0=not started, 1-15=questions)
            
            // Map current question number to database level (1-4)
            // Q1-5 = Level 1, Q6-10 = Level 2, Q11-14 = Level 3, Q15 = Level 4
            var dbLevel = currentQuestion switch
            {
                >= 1 and <= 5 => 1,
                >= 6 and <= 10 => 2,
                >= 11 and <= 14 => 3,
                15 => 4,
                _ => 1 // Default to level 1
            };
            
            if (Program.DebugMode)
            {
                GameConsole.Debug($"[Question] Requesting question #{currentQuestion} (DB level {dbLevel})");
            }
            
            // Get random question for this level
            var question = await _questionRepository.GetRandomQuestionAsync(currentQuestion);

            if (question == null)
            {
                // Get diagnostic info about questions in database
                var (total, unused) = await _questionRepository.GetQuestionCountAsync(currentQuestion);
                
                if (Program.DebugMode)
                {
                    GameConsole.Debug($"[Question] No unused questions found for question #{currentQuestion} (DB level {dbLevel})");
                    GameConsole.Debug($"[Question] Database has {total} total questions at level {dbLevel} ({unused} unused)");
                }
                
                GameConsole.Error($"[Question] No unused questions available for question #{currentQuestion}! Database has {total} total, {unused} unused. Use Database menu to reset or add questions.");
                return;
            }

            if (Program.DebugMode)
            {
                GameConsole.Debug($"[Question] Loaded question ID {question.Id}: {question.QuestionText.Substring(0, Math.Min(50, question.QuestionText.Length))}...");
            }

            // Store question for progressive reveal
            _currentQuestion = question;

            // Initialize lifeline icons if not already done (for games that skip explain phase)
            if (!_isExplainGameActive)
            {
                InitializeLifelineIcons();
            }

            // Mark question as used (before UI updates)
            await _questionRepository.MarkQuestionAsUsedAsync(question.Id);

            // Update control panel UI on UI thread for instant display (like screens)
            if (!IsDisposed && IsHandleCreated)
            {
                BeginInvoke(() =>
                {
                    if (!IsDisposed)
                    {
                        // Update UI with question only - answers will be revealed progressively
                        txtQuestion.Text = question.QuestionText;
                        txtA.Clear(); // Will be revealed on first answer click
                        txtB.Clear(); // Will be revealed on second answer click
                        txtC.Clear(); // Will be revealed on third answer click
                        txtD.Clear(); // Will be revealed on fourth answer click
                        lblAnswer.Text = question.CorrectAnswer;
                        lblAnswer.Visible = false; // Hide until all answers revealed
                        txtExplain.Text = question.Explanation;
                        txtID.Text = question.Id.ToString();

                        // Reset answer selection
                        ResetAnswerColors();
                        _currentAnswer = string.Empty;
                        // Note: _answerRevealStep is managed by callers (btnNewQuestion_Click, lights down, STQ)
                        // Do NOT reset it here to avoid race conditions with caller's state management
                        
                        // Enable answer buttons now that a question is loaded and set to orange
                        btnA.Enabled = true;
                        btnA.BackColor = Color.DarkOrange;
                        btnA.Visible = true;
                        txtA.Visible = true;
                        btnB.Enabled = true;
                        btnB.BackColor = Color.DarkOrange;
                        btnB.Visible = true;
                        txtB.Visible = true;
                        btnC.Enabled = true;
                        btnC.BackColor = Color.DarkOrange;
                        btnC.Visible = true;
                        txtC.Visible = true;
                        btnD.Enabled = true;
                        btnD.BackColor = Color.DarkOrange;
                        btnD.Visible = true;
                        txtD.Visible = true;
                    }
                });
            }

            // Broadcast question to all screens (ensure on UI thread for cross-thread safety)
            if (!IsDisposed && IsHandleCreated)
            {
                BeginInvoke(() =>
                {
                    if (!IsDisposed)
                    {
                        _screenService.UpdateQuestion(question);
                    }
                });
            }

            // Get question number for audio logic
            var questionNumber = (int)nmrLevel.Value;
            
            // Question button remains enabled for progressive reveal
            // Will be disabled after all 4 answers are shown
            
            // Walk Away button will be enabled (green) after all answers are revealed
            // See the answer reveal step 5 logic below
            
            // Closing button logic handled in ShowWinningsAndEnableButtons based on round completion
            // Disable Closing button when loading new question (will re-enable after answer reveal on Q6+ for round 2+)
            btnClosing.Enabled = false;
            btnClosing.BackColor = Color.Gray;
        }
        catch (Exception ex)
        {
            GameConsole.Error($"[Question] Error loading question: {ex.Message}");
        }
    }

    /// <summary>
    /// Wrapper method that loads a new question with full audio handling (stops sounds, plays bed music).
    /// Use this for normal question progression.
    /// </summary>
    private async Task LoadNewQuestion()
    {
        // Load and display the question
        await LoadAndDisplayQuestionAsync();
        
        // Handle audio separately based on question number (now 1-indexed)
        var questionNumber = (int)nmrLevel.Value;
        
        // For Q1-5: Don't stop lights down or restart bed music (continuous quick round)
        // For Q6+: Stop lights down, then play question-specific bed music
        if (questionNumber >= 6)
        {
            _ = Task.Run(async () =>
            {
                await _soundService.StopAllSoundsAsync();
                if (!IsDisposed)
                {
                    PlayQuestionBed();
                }
            });
        }
    }

    private void btnA_Click(object? sender, EventArgs e)
    {
        SelectAnswer("A");
    }

    private void btnB_Click(object? sender, EventArgs e)
    {
        SelectAnswer("B");
    }

    private void btnC_Click(object? sender, EventArgs e)
    {
        SelectAnswer("C");
    }

    private void btnD_Click(object? sender, EventArgs e)
    {
        SelectAnswer("D");
    }

    private async void btnLightsDown_Click(object? sender, EventArgs e)
    {
        // Only increment for first question (0Ã¢â€ â€™1). After that, ProcessNormalReveal handles increment.
        if (nmrLevel.Value == 0)
        {
            nmrLevel.Value++;
        }
        
        // Cancel any previous lights down operation
        _lightsDownCts?.Cancel();
        _lightsDownCts?.Dispose();
        _lightsDownCts = new CancellationTokenSource();
        var token = _lightsDownCts.Token;
        
        // Disable Lights Down immediately to prevent double-clicks
        btnLightsDown.Enabled = false;
        btnLightsDown.BackColor = Color.Gray;
        
        // Disable Explain Game immediately (grey)
        btnExplainGame.Enabled = false;
        btnExplainGame.BackColor = Color.Gray;
        
        // Disable question level selector during active question
        nmrLevel.Enabled = false;
        
        // Exit Explain Game mode
        _isExplainGameActive = false;
        
        // Stop money tree demo if running
        StopMoneyTreeDemo();
        
        // Hide money tree if visible on TV screen
        if (btnShowMoneyTree.Text == "Hide Money Tree" || btnShowMoneyTree.Text == "Demo Money Tree" || btnShowMoneyTree.Text == "Demo Running...")
        {
            // Money tree will be hidden via HideWinnings on the screen
            _screenService.HideWinnings();
            
            // Reset button to Show state
            btnShowMoneyTree.BackColor = Color.LimeGreen;
            btnShowMoneyTree.ForeColor = Color.Black;
            btnShowMoneyTree.Text = "Show Money Tree";
            btnShowMoneyTree.Enabled = true;
        }
        
        var questionNumber = (int)nmrLevel.Value; // Now 1-indexed
        
        try
        {
            // Stop ExplainRules music if it was playing, or stop all sounds for Q6+
            // For Q1-5 (without explain game active), let existing sounds continue
            if (questionNumber >= 6)
            {
                await _soundService.StopAllSoundsAsync();
            }
            else
            {
                // For Q1-5, only stop music if coming from Explain Game mode
                // Check if ExplainRules music is currently playing
                var currentMusic = _soundService.GetCurrentMusicIdentifier();
                if (currentMusic == "ExplainGame")
                {
                    await _soundService.StopAllSoundsAsync();
                }
            }
            
            // Play lights down sound
            if (!token.IsCancellationRequested && !IsDisposed)
            {
                PlayLightsDownSound();
            }
            
            // Wait for lights down sound to finish (monitor queue instead of fixed delay)
            GameConsole.Debug("[LightsDown] Waiting for lights down sound to finish...");
            while (_soundService.IsQueuePlaying())
            {
                await Task.Delay(100, token);
            }
            GameConsole.Debug("[LightsDown] Lights down sound finished");
            
            // Start new round telemetry tracking
            _roundNumber++;
            _telemetryService.StartNewRound(_roundNumber);
            GameConsole.Debug($"[Telemetry] Started Round {_roundNumber}");
            
            // Load question - this updates all screens
            await LoadNewQuestion();
            
            // For all questions (Q1-15), show question immediately after lights down
            if (!token.IsCancellationRequested && !IsDisposed)
            {
                _answerRevealStep = 1; // Question shown, ready to reveal answers
                chkShowQuestion.Checked = true;
                
                // Start appropriate bed music for question level
                PlayQuestionBed();
            }
        }
        catch (OperationCanceledException)
        {
            // Cancellation is normal, don't log it
        }
        catch (Exception ex)
        {
            GameConsole.Error($"[LightsDown] Error in lights down sequence: {ex.Message}");
        }
        
        // For Q1-5, setup UI for first question
        if (questionNumber >= 1 && questionNumber <= 5)
        {
            // Disable risk mode button after first lights down (grey)
            btnActivateRiskMode.Enabled = false;
            btnActivateRiskMode.BackColor = Color.Gray;
        }
        
        // Enable Question button (green) for all questions after lights down
        btnNewQuestion.Enabled = true;
        btnNewQuestion.BackColor = Color.LimeGreen;
        btnNewQuestion.ForeColor = Color.Black;
        
        // Set lifelines to standby mode (orange, not clickable until all answers revealed)
        SetLifelineMode(LifelineMode.Standby);
    }

    private void btnReveal_Click(object? sender, EventArgs e)
    {
        // Disable Reveal button immediately to prevent double-clicks
        btnReveal.Enabled = false;
        btnReveal.BackColor = Color.Gray;
        
        GameConsole.Debug("[Reveal] Button clicked");
        
        if (string.IsNullOrEmpty(_currentAnswer))
        {
            GameConsole.Error("[Reveal] No answer selected!");
            // Re-enable button since reveal failed
            btnReveal.Enabled = true;
            btnReveal.BackColor = Color.LimeGreen;
            return;
        }

        bool isCorrect = _currentAnswer == lblAnswer.Text;
        GameConsole.Debug($"[Reveal] Answer: {_currentAnswer}, Correct: {lblAnswer.Text}, IsCorrect: {isCorrect}");
        
        // Check if Double Dip is active (synchronously)
        if (_lifelineManager != null)
        {
            GameConsole.Debug("[Reveal] Lifeline manager exists, checking DD in background");
            // Run DD check in background
            _ = Task.Run(async () =>
            {
                try
                {
                    GameConsole.Debug("[Reveal] Starting DD check");
                    var result = await _lifelineManager.HandleDoubleDipRevealAsync(_currentAnswer, lblAnswer.Text, isCorrect);
                    GameConsole.Debug($"[Reveal] DD check result: {result}");
                    
                    if (result == DoubleDipRevealResult.FirstAttemptWrong)
                    {
                        // First DD attempt was wrong - update UI on main thread
                        if (!IsDisposed && IsHandleCreated)
                        {
                            BeginInvoke(() =>
                            {
                                try
                                {
                                    if (!IsDisposed)
                                    {
                                        // Show wrong answer on control panel briefly
                                        switch (_currentAnswer)
                                        {
                                            case "A": txtA.BackColor = Color.Red; break;
                                            case "B": txtB.BackColor = Color.Red; break;
                                            case "C": txtC.BackColor = Color.Red; break;
                                            case "D": txtD.BackColor = Color.Red; break;
                                        }
                                        
                                        // Play DD first wrong attempt sound
                                        _soundService.PlaySound(SoundEffect.LifelineDoubleDipFirst, "dd_first");
                                        
                                        // Remove from screens (not control panel)
                                        _screenService.RemoveAnswer(_currentAnswer);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    GameConsole.Error($"[RevealAnswer] Error updating UI for DD first attempt: {ex.Message}");
                                }
                            });
                        }
                        
                        // Wait a moment for dramatic effect
                        await Task.Delay(1500);
                        
                        // Update UI on main thread after delay
                        if (!IsDisposed && IsHandleCreated)
                        {
                            BeginInvoke(() =>
                            {
                                try
                                {
                                    if (!IsDisposed)
                                    {
                                        // Disable the wrong answer button on control panel
                                        switch (_currentAnswer)
                                        {
                                            case "A": 
                                                btnA.Enabled = false;
                                                btnA.BackColor = Color.Gray;
                                                break;
                                            case "B": 
                                                btnB.Enabled = false;
                                                btnB.BackColor = Color.Gray;
                                                break;
                                            case "C": 
                                                btnC.Enabled = false;
                                                btnC.BackColor = Color.Gray;
                                                break;
                                            case "D": 
                                                btnD.Enabled = false;
                                                btnD.BackColor = Color.Gray;
                                                break;
                                        }
                                        
                                        // Reset answer colors for second attempt
                                        ResetAnswerColors();
                                        
                                        // Re-enable remaining answer buttons for second attempt (that aren't the wrong one)
                                        if (_currentAnswer != "A" && !btnA.Enabled) 
                                        { 
                                            btnA.Enabled = true;
                                            btnA.BackColor = Color.DarkOrange; 
                                        }
                                        if (_currentAnswer != "B" && !btnB.Enabled) 
                                        { 
                                            btnB.Enabled = true;
                                            btnB.BackColor = Color.DarkOrange; 
                                        }
                                        if (_currentAnswer != "C" && !btnC.Enabled) 
                                        { 
                                            btnC.Enabled = true;
                                            btnC.BackColor = Color.DarkOrange; 
                                        }
                                        if (_currentAnswer != "D" && !btnD.Enabled) 
                                        { 
                                            btnD.Enabled = true;
                                            btnD.BackColor = Color.DarkOrange; 
                                        }
                                        
                                        // Disable Reveal button - wait for second answer
                                        btnReveal.Enabled = false;
                                        btnReveal.BackColor = Color.Gray;
                                        
                                        // Clear current answer for second attempt
                                        _currentAnswer = string.Empty;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    GameConsole.Error($"[RevealAnswer] Error updating UI after DD delay: {ex.Message}");
                                }
                            });
                        }
                        
                        return; // Don't proceed with normal reveal
                    }
                    
                    // If ddResult is SecondAttempt or NotActive, proceed with normal reveal on UI thread
                    GameConsole.Debug("[Reveal] Proceeding with normal reveal");
                    if (!IsDisposed && IsHandleCreated)
                    {
                        BeginInvoke(() =>
                        {
                            try
                            {
                                if (!IsDisposed)
                                {
                                    GameConsole.Debug("[Reveal] Calling ProcessNormalReveal");
                                    ProcessNormalReveal(isCorrect);
                                }
                            }
                            catch (Exception ex)
                            {
                                GameConsole.Error($"[RevealAnswer] Error in normal reveal: {ex.Message}");
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    GameConsole.Error($"[RevealAnswer] Error in DD background task: {ex.Message}");
                }
            });
            return; // Exit early if DD is being checked
        }
        
        // No Double Dip - process reveal immediately on UI thread
        GameConsole.Debug("[Reveal] No lifeline manager, calling ProcessNormalReveal directly");
        ProcessNormalReveal(isCorrect);
    }

    private void btnWalk_Click(object? sender, EventArgs e)
    {
        // Notify Stream Deck that question has ended (walk away)
        _streamDeckIntegration?.OnQuestionEnd();
        
        // Capture winnings BEFORE modifying game state
        // If final winnings already set (after wrong answer), use that; otherwise use current value
        var winnings = _finalWinningsAmount ?? _gameService.State.CurrentValue;
        
        _gameService.State.WalkAway = true;
        _gameOutcome = _finalWinningsAmount != null ? _gameOutcome : GameOutcome.Drop; // Preserve outcome if already set (Wrong), otherwise set to Drop
        _isAutomatedSequenceRunning = true;
        
        // Create cancellation token for this sequence
        _automatedSequenceCts?.Cancel();
        _automatedSequenceCts?.Dispose();
        _automatedSequenceCts = new CancellationTokenSource();
        var token = _automatedSequenceCts.Token;
        
        // Use current question level to determine which quit sound to play (now 1-indexed)
        var questionNumber = (int)nmrLevel.Value;
        var quitSound = questionNumber <= 10 ? SoundEffect.QuitSmall : SoundEffect.QuitLarge;
        var quitSoundId = "quit_sound";
        
        // Stop all sounds and play quit sound in background to prevent UI blocking
        _ = Task.Run(async () =>
        {
            await _soundService.StopAllSoundsAsync();
            
            if (!IsDisposed)
            {
                _soundService.PlaySound(quitSound, quitSoundId, loop: false);
            }
        });
        
        // Log walk away to console
        GameConsole.Info($"[WALK AWAY] Player walked away with: {winnings}");
        
        // Show winnings on screens
        if (!chkShowWinnings.Checked)
        {
            chkShowWinnings.Checked = true;
        }
        
        // Disable Walk Away (grey)
        btnWalk.Enabled = false;
        btnWalk.BackColor = Color.Gray;
        
        // Disable lifelines immediately - round is over
        SetLifelineMode(LifelineMode.Inactive);
        
        // Enable Closing (green)
        btnClosing.Enabled = true;
        btnClosing.BackColor = Color.LimeGreen;
        btnClosing.ForeColor = Color.Black;
        
        // Wait for quit sound to finish, then auto-trigger end-of-round sequence - in background
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(100, token);
                await _soundService.WaitForSoundAsync(quitSoundId, token);
                await EndRoundSequence(winnings);
            }
            catch (OperationCanceledException)
            {
                // Sequence was cancelled by reset
            }
            catch (Exception ex)
            {
                GameConsole.Error($"[WalkAway] Error in background task: {ex.Message}");
            }
        });
    }

    private void btnActivateRiskMode_Click(object? sender, EventArgs e)
    {
        // Risk mode can only be activated at the start of the game
        if (_gameService.State.CurrentLevel > 0)
        {
            GameConsole.Error("[RiskMode] Risk Mode can only be activated at the beginning of the game, before the first question.");
            return;
        }
        
        // If already in Risk Mode, do nothing (button should be disabled but check anyway)
        if (_gameService.State.Mode == Core.Models.GameMode.Risk)
        {
            GameConsole.Warn("[RiskMode] Risk Mode is already active.");
            return;
        }
        
        // Activate Risk Mode
        _gameService.ChangeMode(Core.Models.GameMode.Risk);
        
        // Play Risk Mode activation sound
        _soundService.PlaySound(SoundEffect.RiskModeActive, "risk_mode_active", loop: false);
        
        // Update button appearance - set to active state and disable
        btnActivateRiskMode.BackColor = Color.Red;
        btnActivateRiskMode.Text = "RISK MODE: ON";
        btnActivateRiskMode.Enabled = false; // Disable button once activated
        
        if (Program.DebugMode)
        {
            GameConsole.Debug("[Risk Mode] Activated - No safety net at Q5/Q10, uses alternate sounds. Button disabled until reset.");
        }
    }
    
    private void btnShowMoneyTree_Click(object? sender, EventArgs e)
    {
        if (Program.DebugMode)
        {
            GameConsole.Debug($"[MoneyTreeButton] Button clicked, current text: '{btnShowMoneyTree.Text}', _pendingSafetyNetLevel={_pendingSafetyNetLevel}, _isExplainGameActive={_isExplainGameActive}");
        }
        
        if (btnShowMoneyTree.Text == "Show Money Tree")
        {
            // Hide the winning strap if it's showing (to avoid overlap)
            if (chkShowWinnings.Checked)
            {
                chkShowWinnings.Checked = false;
            }
            
            // Show the money tree on TV screen only
            var currentState = _gameService.State;
            _screenService.ShowWinnings(currentState);
            
            // Update button state based on context
            if (_isExplainGameActive)
            {
                // In Explain Game mode - go directly to Demo state
                btnShowMoneyTree.BackColor = Color.DeepSkyBlue;
                btnShowMoneyTree.ForeColor = Color.Black;
                btnShowMoneyTree.Text = "Demo Money Tree";
            }
            else if (_pendingSafetyNetLevel > 0 && CanLockInSafetyNet())
            {
                // Safety net lock-in is available - show "Lock In Safety" button
                btnShowMoneyTree.BackColor = Color.LightBlue;
                btnShowMoneyTree.ForeColor = Color.Black;
                btnShowMoneyTree.Text = "Lock In Safety";
            }
            else
            {
                // Normal mode - just hide
                btnShowMoneyTree.BackColor = Color.Orange;
                btnShowMoneyTree.ForeColor = Color.Black;
                btnShowMoneyTree.Text = "Hide Money Tree";
            }
        }
        else if (btnShowMoneyTree.Text == "Lock In Safety")
        {
            // User clicked to lock in the safety net - play animation
            var levelToLock = _pendingSafetyNetLevel;
            _pendingSafetyNetLevel = 0; // Clear the pending state
            
            // Change button to indicate animation is running
            btnShowMoneyTree.BackColor = Color.Yellow;
            btnShowMoneyTree.ForeColor = Color.Black;
            btnShowMoneyTree.Text = "Locking In...";
            btnShowMoneyTree.Enabled = false;
            
            // Start the animation with sound and revert to current level after
            StartSafetyNetAnimation(levelToLock, playSound: true, targetLevelAfterAnimation: _gameService.MoneyTree.GetDisplayLevel(_gameService.State.CurrentLevel, _gameService.State.GameWin));
            
            // Wait for animation to complete (6 flashes Ãƒâ€” 300ms = 1800ms + small buffer) - in background
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(2000);
                    
                    if (!IsDisposed && IsHandleCreated)
                    {
                        BeginInvoke(() =>
                        {
                            try
                            {
                                if (!IsDisposed)
                                {
                                    // After animation, change to "Hide Money Tree"
                                    btnShowMoneyTree.BackColor = Color.Orange;
                                    btnShowMoneyTree.ForeColor = Color.Black;
                                    btnShowMoneyTree.Text = "Hide Money Tree";
                                    btnShowMoneyTree.Enabled = true;
                                }
                            }
                            catch (Exception ex)
                            {
                                GameConsole.Error($"[MoneyTree] Error updating button after animation: {ex.Message}");
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    GameConsole.Error($"[MoneyTree] Error in animation wait: {ex.Message}");
                }
            });
        }
        else if (btnShowMoneyTree.Text == "Demo Money Tree")
        {
            // Start demo animation - progress through levels 1-15
            StartMoneyTreeDemo();
        }
        else // "Hide Money Tree"
        {
            // Check if demo was running before we stop it
            bool wasDemoActive = _isMoneyTreeDemoActive;
            
            // Stop demo if it's running
            StopMoneyTreeDemo();
            
            // Clear any pending safety net lock-in
            _pendingSafetyNetLevel = 0;
            
            // Hide the money tree from TV screen
            _screenService.HideWinnings();
            
            // If demo was running, reset money tree to level 0 after hiding (so host/guest screens update)
            if (wasDemoActive)
            {
                UpdateMoneyTreeOnScreens(0);
            }
            
            // If in explain game mode, ensure lifeline icons remain visible after hiding money tree
            if (_isExplainGameActive)
            {
                _screenService.ShowLifelineIcons();
                GameConsole.Debug("[MoneyTree] Re-enabled lifeline icon rendering after hiding money tree in explain game mode");
            }
            
            // Update button state
            btnShowMoneyTree.BackColor = Color.LimeGreen;
            btnShowMoneyTree.ForeColor = Color.Black;
            btnShowMoneyTree.Text = "Show Money Tree";
        }
    }

    private void StartMoneyTreeDemo()
    {
        _isMoneyTreeDemoActive = true;
        _moneyTreeDemoLevel = 1; // Start at level 1
        
        // Initialize timer if not already created
        if (_moneyTreeDemoTimer == null)
        {
            _moneyTreeDemoTimer = new System.Windows.Forms.Timer();
            _moneyTreeDemoTimer.Interval = MONEY_TREE_DEMO_INTERVAL;
            _moneyTreeDemoTimer.Tick += MoneyTreeDemoTimer_Tick;
        }
        
        // Update to first level immediately
        UpdateMoneyTreeOnScreens(_moneyTreeDemoLevel);
        
        // Start timer for subsequent levels
        _moneyTreeDemoTimer.Start();
        
        // Update button appearance during demo
        btnShowMoneyTree.BackColor = Color.Yellow;
        btnShowMoneyTree.ForeColor = Color.Black;
        btnShowMoneyTree.Text = "Demo Running...";
        btnShowMoneyTree.Enabled = false;
    }
    
    private void MoneyTreeDemoTimer_Tick(object? sender, EventArgs e)
    {
        _moneyTreeDemoLevel++;
        
        if (_moneyTreeDemoLevel <= 15)
        {
            // Update all screens with new level
            UpdateMoneyTreeOnScreens(_moneyTreeDemoLevel);
        }
        else
        {
            // Demo complete - stop timer but keep _isMoneyTreeDemoActive = true
            // so that hiding the tree will reset it to level 0
            _moneyTreeDemoTimer?.Stop();
            
            btnShowMoneyTree.BackColor = Color.Orange;
            btnShowMoneyTree.ForeColor = Color.Black;
            btnShowMoneyTree.Text = "Hide Money Tree";
            btnShowMoneyTree.Enabled = true;
        }
    }
    
    private void StopMoneyTreeDemo()
    {
        if (_moneyTreeDemoTimer != null)
        {
            _moneyTreeDemoTimer.Stop();
        }
        _isMoneyTreeDemoActive = false;
        _moneyTreeDemoLevel = 0;
    }
    
    /// <summary>
    /// Starts the safety net lock-in animation for Q5 or Q10
    /// Flashes the safety net level 3 times to show it's locked in
    /// </summary>
    private void StartSafetyNetAnimation(int safetyNetLevel, bool playSound = true, int? targetLevelAfterAnimation = null)
    {
        if (Program.DebugMode)
        {
            GameConsole.Debug($"[SafetyNetAnimation] StartSafetyNetAnimation called for level {safetyNetLevel}, playSound={playSound}, targetLevel={targetLevelAfterAnimation}");
        }
        
        // Optionally play safety net lock-in sound
        _safetyNetAnimationPlaySound = playSound;
        if (playSound)
        {
            _soundService.PlaySound(SoundEffect.SetSafetyNet, "safety_net_lock_in", loop: false);
        }
        
        _safetyNetAnimationLevel = safetyNetLevel;
        _safetyNetAnimationTargetLevel = targetLevelAfterAnimation ?? safetyNetLevel; // Default to animation level if not specified
        _safetyNetFlashCount = 0;
        _safetyNetFlashState = false;
        
        if (_safetyNetAnimationTimer == null)
        {
            _safetyNetAnimationTimer = new System.Windows.Forms.Timer();
            _safetyNetAnimationTimer.Interval = SAFETY_NET_FLASH_INTERVAL;
            _safetyNetAnimationTimer.Tick += SafetyNetAnimationTimer_Tick;
        }
        
        _safetyNetAnimationTimer.Start();
        
        if (Program.DebugMode)
        {
            GameConsole.Debug($"[SafetyNetAnimation] Started lock-in animation for level {safetyNetLevel}");
        }
    }
    
    private void SafetyNetAnimationTimer_Tick(object? sender, EventArgs e)
    {
        _safetyNetFlashCount++;
        _safetyNetFlashState = !_safetyNetFlashState; // Toggle flash state
        
        if (_safetyNetFlashCount >= SAFETY_NET_FLASH_TOTAL)
        {
            // Animation complete, stop timer
            StopSafetyNetAnimation();
            
            // Update money tree to target level (stays on dropped level for wrong answer, reverts for correct answer)
            UpdateMoneyTreeOnScreens(_safetyNetAnimationTargetLevel);
            
            if (Program.DebugMode)
            {
                GameConsole.Debug($"[SafetyNetAnimation] Animation complete for level {_safetyNetAnimationLevel}, staying on target level {_safetyNetAnimationTargetLevel}");
            }
        }
        else
        {
            // Update screens with flash state
            // We'll need to add a method to handle this special state
            UpdateMoneyTreeWithSafetyNetFlash(_safetyNetAnimationLevel, _safetyNetFlashState);
            
            if (Program.DebugMode)
            {
                GameConsole.Debug($"[SafetyNetAnimation] Flash {_safetyNetFlashCount}/{SAFETY_NET_FLASH_TOTAL}, State: {(_safetyNetFlashState ? "ON" : "OFF")}");
            }
        }
    }
    
    private void StopSafetyNetAnimation()
    {
        if (_safetyNetAnimationTimer != null)
        {
            _safetyNetAnimationTimer.Stop();
        }
        _safetyNetFlashCount = 0;
        _safetyNetFlashState = false;
    }
    
    /// <summary>
    /// Updates money tree on all screens with safety net flash animation
    /// </summary>
    private void UpdateMoneyTreeWithSafetyNetFlash(int safetyNetLevel, bool flashState)
    {
        // Host screen
        if (_hostScreen != null)
        {
            _hostScreen.Invoke((MethodInvoker)delegate
            {
                _hostScreen.UpdateMoneyTreeWithSafetyNetFlash(safetyNetLevel, flashState);
            });
        }
        
        // Guest screen
        if (_guestScreen != null)
        {
            _guestScreen.Invoke((MethodInvoker)delegate
            {
                _guestScreen.UpdateMoneyTreeWithSafetyNetFlash(safetyNetLevel, flashState);
            });
        }
        
        // TV screen
        if (_tvScreen is TVScreenForm tvScalable)
        {
            tvScalable.Invoke((MethodInvoker)delegate
            {
                tvScalable.UpdateMoneyTreeWithSafetyNetFlash(safetyNetLevel, flashState);
            });
        }
        
        // Preview screen
        if (_previewScreen != null)
        {
            _previewScreen.Invoke((MethodInvoker)delegate
            {
                _previewScreen.UpdateMoneyTreeWithSafetyNetFlash(safetyNetLevel, flashState);
            });
        }
    }

    private void btnLifeline1_Click(object? sender, EventArgs e)
    {
        _ = Task.Run(async () => await HandleLifelineClickAsync(1, btnLifeline1));
    }

    private void btnLifeline2_Click(object? sender, EventArgs e)
    {
        _ = Task.Run(async () => await HandleLifelineClickAsync(2, btnLifeline2));
    }

    private void btnLifeline3_Click(object? sender, EventArgs e)
    {
        _ = Task.Run(async () => await HandleLifelineClickAsync(3, btnLifeline3));
    }

    private void btnLifeline4_Click(object? sender, EventArgs e)
    {
        _ = Task.Run(async () => await HandleLifelineClickAsync(4, btnLifeline4));
    }

    private Core.Models.LifelineType GetLifelineTypeFromSettings(int lifelineNumber)
    {
        var lifelineType = lifelineNumber switch
        {
            1 => _appSettings.Settings.Lifeline1,
            2 => _appSettings.Settings.Lifeline2,
            3 => _appSettings.Settings.Lifeline3,
            4 => _appSettings.Settings.Lifeline4,
            _ => "5050"
        };

        return lifelineType.ToLower() switch
        {
            "5050" => Core.Models.LifelineType.FiftyFifty,
            "plusone" => Core.Models.LifelineType.PlusOne,
            "ata" => Core.Models.LifelineType.AskTheAudience,
            "switch" => Core.Models.LifelineType.SwitchQuestion,
            "ath" => Core.Models.LifelineType.AskTheHost,
            "dd" => Core.Models.LifelineType.DoubleDip,
            _ => Core.Models.LifelineType.FiftyFifty
        };
    }

    private string GetLifelineDisplayName(int lifelineNumber)
    {
        var type = GetLifelineTypeFromSettings(lifelineNumber);
        return type switch
        {
            Core.Models.LifelineType.FiftyFifty => "50:50",
            Core.Models.LifelineType.PlusOne => "PAF",
            Core.Models.LifelineType.AskTheAudience => "ATA",
            Core.Models.LifelineType.SwitchQuestion => "Switch",
            Core.Models.LifelineType.AskTheHost => "AskHost",
            Core.Models.LifelineType.DoubleDip => "Double",
            _ => $"LL{lifelineNumber}"
        };
    }

    private bool IsLifelineUsed(int lifelineNumber)
    {
        var type = GetLifelineTypeFromSettings(lifelineNumber);
        var lifeline = _gameService.State.GetLifeline(type);
        return lifeline != null && lifeline.IsUsed;
    }

    private void UpdateLifelineButtonLabels()
    {
        InitializeLifelineButtons();
    }

    /// <summary>
    /// Initializes lifeline buttons based on settings: sets visibility, labels, and default state
    /// </summary>
    private void InitializeLifelineButtons()
    {
        int totalLifelines = _appSettings.Settings.TotalLifelines;
        
        // Get labels for all 4 buttons
        var label1 = GetLifelineDisplayName(1);
        var label2 = GetLifelineDisplayName(2);
        var label3 = GetLifelineDisplayName(3);
        var label4 = GetLifelineDisplayName(4);
        
        if (Program.DebugMode)
        {
            GameConsole.Debug($"[Lifelines] Initializing {totalLifelines} lifeline button(s):");
            GameConsole.Debug($"  Button 1: {label1} (Type: {_appSettings.Settings.Lifeline1}) - Visible: {totalLifelines >= 1}");
            GameConsole.Debug($"  Button 2: {label2} (Type: {_appSettings.Settings.Lifeline2}) - Visible: {totalLifelines >= 2}");
            GameConsole.Debug($"  Button 3: {label3} (Type: {_appSettings.Settings.Lifeline3}) - Visible: {totalLifelines >= 3}");
            GameConsole.Debug($"  Button 4: {label4} (Type: {_appSettings.Settings.Lifeline4}) - Visible: {totalLifelines >= 4}");
        }
        
        // Button 1 - always visible if TotalLifelines >= 1
        btnLifeline1.Text = label1;
        btnLifeline1.Visible = totalLifelines >= 1;
        
        // Button 2
        btnLifeline2.Text = label2;
        btnLifeline2.Visible = totalLifelines >= 2;
        
        // Button 3
        btnLifeline3.Text = label3;
        btnLifeline3.Visible = totalLifelines >= 3;
        
        // Button 4
        btnLifeline4.Text = label4;
        btnLifeline4.Visible = totalLifelines >= 4;
        
        // Disable "Show Correct Answer to Host" checkbox if ATH lifeline is enabled
        // This prevents the host from seeing the correct answer when ATH is available
        bool athEnabled = IsAskTheHostEnabled();
        
        if (Program.DebugMode)
        {
            GameConsole.Debug($"[Lifelines] ATH enabled check: {athEnabled}");
            GameConsole.Debug($"  Lifeline1: {_appSettings.Settings.Lifeline1}");
            GameConsole.Debug($"  Lifeline2: {_appSettings.Settings.Lifeline2}");
            GameConsole.Debug($"  Lifeline3: {_appSettings.Settings.Lifeline3}");
            GameConsole.Debug($"  Lifeline4: {_appSettings.Settings.Lifeline4}");
        }
        
        // If ATH is enabled, disable and uncheck the checkbox
        if (athEnabled)
        {
            chkCorrectAnswer.Checked = false; // Uncheck first
            chkCorrectAnswer.Enabled = false; // Then disable
            if (Program.DebugMode)
            {
                GameConsole.Log("[Lifelines] ATH is enabled - 'Show Correct Answer to Host' checkbox disabled");
            }
        }
        else
        {
            chkCorrectAnswer.Enabled = true; // Enable if ATH is not configured
            chkCorrectAnswer.Checked = false; // But still start unchecked
            if (Program.DebugMode)
            {
                GameConsole.Log("[Lifelines] ATH is NOT enabled - 'Show Correct Answer to Host' checkbox enabled");
            }
        }
    }
    
    /// <summary>
    /// Checks if Ask the Host lifeline is configured and enabled
    /// </summary>
    private bool IsAskTheHostEnabled()
    {
        return _appSettings.Settings.Lifeline1?.Equals("ath", StringComparison.OrdinalIgnoreCase) == true ||
               _appSettings.Settings.Lifeline2?.Equals("ath", StringComparison.OrdinalIgnoreCase) == true ||
               _appSettings.Settings.Lifeline3?.Equals("ath", StringComparison.OrdinalIgnoreCase) == true ||
               _appSettings.Settings.Lifeline4?.Equals("ath", StringComparison.OrdinalIgnoreCase) == true;
    }

    /// <summary>
    /// Helper method to play a lifeline sound while stopping all other background audio.
    /// Stops all sounds first, waits 500ms, then plays the lifeline sound to ensure clean audio.
    /// </summary>
    private async Task PlayLifelineSoundAsync(SoundEffect soundEffect, string? identifier = null, bool loop = false)
    {
        // For Q1-5, track that bed music should be restarted after answer is revealed (now 1-indexed)
        var questionNumber = (int)nmrLevel.Value;
        if (questionNumber >= 1 && questionNumber <= 5)
        {
            _shouldRestartBedMusic = true;
        }
        
        // Stop all background sounds first
        await _soundService.StopAllSoundsAsync();
        
        // Wait 500ms for clean audio transition
        await Task.Delay(500);
        
        // Play the lifeline sound
        if (identifier != null)
        {
            _soundService.PlaySound(soundEffect, identifier, loop);
        }
        else
        {
            _soundService.PlaySound(soundEffect);
        }
    }

    private async Task HandleLifelineClickAsync(int lifelineNumber, Button button)
    {
        if (_lifelineManager == null) return;
        
        // Prevent clicking if button is in standby (orange)
        if (button.BackColor == Color.Orange)
        {
            GameConsole.Warn($"[Lifeline] Click ignored - button {lifelineNumber} is in standby mode");
            return;
        }
        
        var type = GetLifelineTypeFromSettings(lifelineNumber);
        
        // Demo mode - ping the lifeline icon
        if (_lifelineMode == LifelineMode.Demo)
        {
            _lifelineManager.PingLifelineIcon(lifelineNumber, type);
            return;
        }

        // Active mode - delegate to lifeline manager
        
        // Check if this is a multi-stage lifeline click
        if (_lifelineManager.IsInMultiStageState(type))
        {
            _lifelineManager.HandleMultiStageClick(type, lifelineNumber);
            return;
        }
        
        var lifeline = _gameService.State.GetLifeline(type);
        
        if (lifeline == null || lifeline.IsUsed)
        {
            GameConsole.Error($"[Lifeline] This lifeline has already been used!");
            return;
        }

        // Execute lifeline via manager
        await _lifelineManager.ExecuteLifelineAsync(type, lifelineNumber, lblAnswer.Text);
    }

    private void btnHostIntro_Click(object? sender, EventArgs e)
    {
        // Disable Host Intro until closing
        btnHostIntro.Enabled = false;
        btnHostIntro.BackColor = Color.Gray;
        
        // Enable Pick Player (green)
        btnPickPlayer.Enabled = true;
        btnPickPlayer.BackColor = Color.LimeGreen;
        btnPickPlayer.ForeColor = Color.Black;
        
        // Enable Reset Game button (grey with red border/symbol and black text)
        btnResetGame.Enabled = true;
        btnResetGame.BackColor = Color.Gray;
        btnResetGame.FlatAppearance.BorderColor = Color.Red;
        btnResetGame.FlatAppearance.BorderSize = 3;
        btnResetGame.ForeColor = Color.Black;
        btnResetGame.Image = GetRedStopImage();
        
        // Play host entrance audio once
        _soundService.PlaySound(SoundEffect.HostEntrance, loop: false);
        
        // Broadcast game state to web clients (transition from InitialLobby to WaitingLobby)
        _ = Task.Run(async () =>
        {
            if (IsWebServerRunning && _webServerHost != null)
            {
                try
                {
                    var sessionId = await GetActiveSessionIdAsync();
                    if (!string.IsNullOrEmpty(sessionId))
                    {
                        await _webServerHost.BroadcastGameStateAsync(
                            sessionId, 
                            Web.Models.GameStateType.WaitingLobby,
                            "Game has started! Waiting for next activity...");
                        
                        GameConsole.Info($"[HostIntro] Broadcasted WaitingLobby state to session {sessionId}");
                    }
                    else
                    {
                        GameConsole.Warn("[HostIntro] No active session found - skipping state broadcast");
                    }
                }
                catch (Exception ex)
                {
                    GameConsole.Error($"[HostIntro] Error broadcasting game state: {ex.Message}");
                }
            }
        });
        
#if DEBUG
        // Reset all questions to unused for new game - DEBUG ONLY
        // In release builds, the database is used as-is, respecting the Used flags
        _ = Task.Run(async () =>
        {
            try
            {
                await _questionRepository.ResetAllQuestionsAsync();
                GameConsole.Debug("[HostIntro] [DEBUG] Reset all questions to unused");
            }
            catch (Exception ex)
            {
                GameConsole.Error($"[HostIntro] Error resetting questions: {ex.Message}");
            }
        });
#else
        GameConsole.Info("[HostIntro] Using database as-is (Used flags respected in Release mode)");
#endif
    }
    
    /// <summary>
    /// Get the active session ID from the database
    /// </summary>
    private async Task<string?> GetActiveSessionIdAsync()
    {
        if (_webServerHost == null || !_webServerHost.IsRunning)
            return null;

        try
        {
            var serviceScopeFactory = _webServerHost.GetService<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();
            if (serviceScopeFactory == null)
                return null;

            using var scope = serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetService<MillionaireGame.Web.Data.WAPSDbContext>();
            if (dbContext == null)
                return null;

            var activeSession = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
                dbContext.Sessions.Where(s => s.Status == MillionaireGame.Web.Models.SessionStatus.Active));

            return activeSession?.Id;
        }
        catch (Exception ex)
        {
            GameConsole.Error($"[GetActiveSession] Error: {ex.Message}");
            return null;
        }
    }

    private async void btnPickPlayer_Click(object? sender, EventArgs e)
    {
        // Open FFF Window to manage Fastest Finger First
        
        // Always use localhost for internal connections, regardless of what IP the web server listens on
        // The web server may listen on 0.0.0.0, 192.168.x.x, etc., but we always connect via localhost
        var serverPort = _appSettings.Settings.AudienceServerPort;
        var serverUrl = $"http://127.0.0.1:{serverPort}";
        
        // Check if web server is actually running
        bool isWebServerRunning = _webServerHost != null && _webServerHost.IsRunning;
        
        if (_fffWindow == null || _fffWindow.IsDisposed)
        {
            // Create new window with current server state
            _fffWindow = new FFFWindow(serverUrl, isWebServerRunning, _screenService as ScreenUpdateService, _soundService);
        }
        else
        {
            // Update existing window's mode based on current server state
            await _fffWindow.UpdateModeAsync(isWebServerRunning);
        }
        
        // Check if no more players available (offline mode only)
        if (!isWebServerRunning && _fffWindow.NoMorePlayers)
        {
            GameConsole.Warn("[FFF] No more players available! Please reset the game to start a new session.");
            return;
        }
        
        // Show the window (or bring to front if already visible)
        _fffWindow.Show();
        _fffWindow.BringToFront();
        
        // Reset game win flag for new round
        _gameService.State.GameWin = false;
        
        // After picking player, disable Pick Player
        btnPickPlayer.Enabled = false;
        btnPickPlayer.BackColor = Color.Gray;
        
        // Enable Explain Game (green)
        btnExplainGame.Enabled = true;
        btnExplainGame.BackColor = Color.LimeGreen;
        btnExplainGame.ForeColor = Color.Black;
        
        // Enable Lights Down (green)
        btnLightsDown.Enabled = true;
        btnLightsDown.BackColor = Color.LimeGreen;
        btnLightsDown.ForeColor = Color.Black;
        
        // Enable Show Money Tree button (green)
        btnShowMoneyTree.Enabled = true;
        btnShowMoneyTree.BackColor = Color.LimeGreen;
        btnShowMoneyTree.ForeColor = Color.Black;
        
        // Enable Activate Risk Mode button (yellow - deactivated but available)
        btnActivateRiskMode.Enabled = true;
        btnActivateRiskMode.BackColor = Color.Yellow;
        btnActivateRiskMode.ForeColor = Color.Black;
        
        // Enable Reset Round button (grey with red border/symbol and black text)
        // UNLESS no more players are available in offline mode
        if (!isWebServerRunning && _fffWindow.NoMorePlayers)
        {
            btnResetRound.Enabled = false;
            btnResetRound.BackColor = Color.DarkGray;
        }
        else
        {
            btnResetRound.Enabled = true;
            btnResetRound.BackColor = Color.Gray;
            btnResetRound.FlatAppearance.BorderColor = Color.Red;
            btnResetRound.FlatAppearance.BorderSize = 3;
            btnResetRound.ForeColor = Color.Black;
            btnResetRound.Image = GetRedStopImage();
        }
        
        // Enable question level selector - allow setting before first lights down
        nmrLevel.Enabled = true;
    }

    private void btnExplainGame_Click(object? sender, EventArgs e)
    {
        // Start new game telemetry session (game start time)
        if (_roundNumber == 0)
        {
            _telemetryService.StartNewGame();
            _telemetryService.SetMoneyTreeSettings(_gameService.MoneyTree.Settings);
            GameConsole.Debug("[Telemetry] Started new game session");
        }
        
        // Play game explanation audio on loop
        _soundService.PlaySound(SoundEffect.ExplainGame, loop: true);
        
        // Enter demo mode - lifelines turn yellow
        SetLifelineMode(LifelineMode.Demo);
        
        // Set explain game active state BEFORE initializing icons
        _isExplainGameActive = true;
        
        // Initialize lifeline icons on all screens (Hidden state during explain)
        InitializeLifelineIcons();
        
        // Enable icon rendering (but Hidden icons won't be drawn)
        _screenService.ShowLifelineIcons();
    }

    /// <summary>
    /// End-of-round sequence: plays walk away sound, shows winnings, and resets game for next player
    /// </summary>
    /// <param name="capturedWinnings">Optional pre-captured winnings value to display</param>
    private async Task EndRoundSequence(string? capturedWinnings = null)
    {
        // Use captured winnings if provided, otherwise calculate from game outcome
        string winnings = capturedWinnings ?? (_gameOutcome switch
        {
            // Use top prize from money tree settings (properly formatted with currency)
            GameOutcome.Win => _gameService.MoneyTree.GetFormattedValue(15),  // Won Q15 - show top prize from settings
            GameOutcome.Drop => _gameService.State.DropValue,      // Walked away - show drop value  
            GameOutcome.Wrong => _gameService.State.WrongValue,    // Answered wrong - show wrong value (0 or last milestone)
            _ => _gameService.State.CurrentValue                   // Fallback to current value
        });
        
        // Store final winnings for screen display
        _finalWinningsAmount = winnings;
        
        // Use current question level to determine which walk away sound to play (now 1-indexed)
        var questionNumber = (int)nmrLevel.Value;
        var walkAwaySound = questionNumber <= 10 ? SoundEffect.WalkAwaySmall : SoundEffect.WalkAwayLarge;
        var walkAwaySoundId = "walkaway_sound";
        
        _soundService.PlaySound(walkAwaySound, walkAwaySoundId, loop: false);
        
        // Give sound time to register before checking status
        await Task.Delay(100);
        
        // Log final winnings to console
        GameConsole.Info($"[GAME OVER] Total Winnings: {winnings} - Thanks for playing!");
        
        // Show winnings on screens
        if (!chkShowWinnings.Checked)
        {
            chkShowWinnings.Checked = true;
        }
        else
        {
            // If already checked, manually update the amount displayed
            // This ensures the correct winnings value is shown
            var amountToShow = _finalWinningsAmount ?? _gameService.State.CurrentValue;
            _screenService.ShowWinningsAmount(amountToShow);
        }
        
        // Show full-screen game winner display (Thanks for Playing portion)
        // Calculate the actual winning level based on outcome
        int actualWinningLevel = _gameOutcome switch
        {
            GameOutcome.Win => 15,  // Won Q15
            GameOutcome.Drop => questionNumber - 1,  // Walked away - get last correct answer (haven't answered current question)
            GameOutcome.Wrong => GetDroppedLevel(questionNumber),  // Safety net level (based on which safety nets were passed)
            _ => questionNumber
        };
        
        // Calculate currency breakdown for dual currency support
        // Pass both the question reached (to know which currencies were played) 
        // and the actual winning level (to know what they won)
        GameConsole.Info($"[EndRound] Calling GetCurrencyBreakdown - QuestionNumber: {questionNumber}, ActualWinningLevel: {actualWinningLevel}");
        var (currency1Display, currency2Display, hasCurrency1, hasCurrency2) = 
            _gameService.MoneyTree.GetCurrencyBreakdown(questionNumber, actualWinningLevel);
        
        GameConsole.Info($"[EndRound] Currency breakdown result - C1: '{currency1Display}' ({hasCurrency1}), C2: '{currency2Display}' ({hasCurrency2})");
        
        _screenService.ShowGameWinner(winnings, currency1Display, currency2Display, 
            hasCurrency1, hasCurrency2, questionNumber);
        
        // Wait for walk away sound to finish
        await _soundService.WaitForSoundAsync(walkAwaySoundId);
        
        // Reset automated sequence flag
        _isAutomatedSequenceRunning = false;
        
        // Mark round as completed
        _roundCompleted = true;
        
        // Complete round telemetry
        var outcomeText = _gameOutcome switch
        {
            GameOutcome.Win => "Win",
            GameOutcome.Drop => "Walk Away",
            GameOutcome.Wrong => "Incorrect Answer",
            _ => "Unknown"
        };
        _telemetryService.CompleteRound(outcomeText, winnings, actualWinningLevel);
        GameConsole.Debug($"[Telemetry] Completed Round {_roundNumber} - {outcomeText}, Winnings: {winnings}");
        
        // Don't automatically reset - let user manually reset with Reset Round button
        // All buttons should already be disabled at this point except Closing
    }

    private void btnClosing_Click(object? sender, EventArgs e)
    {
        switch (_closingStage)
        {
            case ClosingStage.NotStarted:
                // Capture game state BEFORE any resets
                bool isGameRoundActive = _gameService.State.CurrentLevel > 0 || _isAutomatedSequenceRunning;
                
                GameConsole.Debug($"[Closing] Starting closure - GameRoundActive: {isGameRoundActive}, CurrentLevel: {_gameService.State.CurrentLevel}, AutomatedSeq: {_isAutomatedSequenceRunning}");
                
                // Start closing sequence
                _closingStage = ClosingStage.GameOver;
                
                // Disable Walk Away button immediately
                btnWalk.Enabled = false;
                btnWalk.BackColor = Color.Gray;
                
                // Clear question display but DON'T reset controls yet
                txtQuestion.Clear();
                txtA.Clear();
                txtB.Clear();
                txtC.Clear();
                txtD.Clear();
                txtExplain.Clear();
                lblAnswer.Text = string.Empty;
                
                // Hide question and answers from screens
                _screenService.ShowQuestion(false);
                _screenService.RemoveAnswer("A");
                _screenService.RemoveAnswer("B");
                _screenService.RemoveAnswer("C");
                _screenService.RemoveAnswer("D");
                
                // Stop all sounds and play game over ONLY if round is actively in progress (not after completion)
                _ = Task.Run(async () =>
                {
                    await _soundService.StopAllSoundsAsync();
                    
                    // Only play GameOver if round was interrupted (not after walk away/win/loss completion)
                    // Check if round is active AND not completed
                    bool shouldPlayGameOver = isGameRoundActive && !_roundCompleted;
                    
                    if (shouldPlayGameOver && !IsDisposed)
                    {
                        // Round in progress - play game over sound first
                        GameConsole.Debug("[Closing] Playing GameOver sound for interrupted round");
                        _soundService.PlaySoundByKey("GameOver");
                        
                        if (Program.DebugMode)
                        {
                            GameConsole.Debug("[Closing] Stage: GameOver - playing sound (5s)");
                        }
                        
                        if (IsHandleCreated)
                        {
                            BeginInvoke(() =>
                            {
                                if (!IsDisposed)
                                {
                                    btnClosing.BackColor = Color.Red;
                                    btnClosing.Enabled = true;  // Ensure button is enabled for skip
                                    
                                    // Set timer for 5 seconds, then move to underscore
                                    _closingTimer = new System.Windows.Forms.Timer();
                                    _closingTimer.Interval = 5000;
                                    _closingTimer.Tick += (s, args) => { _closingTimer?.Stop(); MoveToUnderscoreStage(); };
                                    _closingTimer.Start();
                                }
                            });
                        }
                    }
                    else if (!IsDisposed)
                    {
                        // No round in progress - skip game over, go straight to underscore
                        GameConsole.Debug("[Closing] No active round - skipping GameOver, moving to Underscore");
                        if (IsHandleCreated)
                        {
                            BeginInvoke(() =>
                            {
                                if (!IsDisposed) MoveToUnderscoreStage();
                            });
                        }
                    }
                });
                break;
                
            case ClosingStage.GameOver:
                // Skip game over, move to underscore
                _closingTimer?.Stop();
                if (!IsDisposed) MoveToUnderscoreStage();
                break;
                
            case ClosingStage.Underscore:
                // Skip underscore, move to theme
                _closingTimer?.Stop();
                // MoveToThemeStage will handle stopping sounds and starting the theme
                if (!IsDisposed) MoveToThemeStage();
                break;
                
            case ClosingStage.Theme:
                // Theme is playing - clicking again will reset game
                _closingTimer?.Stop();
                CompleteClosing();
                break;
                
            case ClosingStage.Complete:
                // Already complete, do nothing
                break;
        }
    }
    
    private void MoveToUnderscoreStage()
    {
        _closingStage = ClosingStage.Underscore;
        btnClosing.BackColor = Color.Orange;
        btnClosing.Enabled = true;  // Ensure button is enabled for skip
        
        _soundService.QueueSoundByKey("CloseUnderscore", AudioPriority.Immediate);
        
        if (Program.DebugMode)
        {
            GameConsole.Debug("[Closing] Stage: Underscore - playing sound (150s)");
        }
        
        // Set timer for 150 seconds, then move to theme
        _closingTimer = new System.Windows.Forms.Timer();
        _closingTimer.Interval = 150000;
        _closingTimer.Tick += (s, args) => { _closingTimer?.Stop(); MoveToThemeStage(); };
        _closingTimer.Start();
    }
    
    private void MoveToThemeStage()
    {
        _closingStage = ClosingStage.Theme;
        
        // Disable Closing button - no more clicks needed, theme will play out
        btnClosing.BackColor = Color.Gray;
        btnClosing.Enabled = false;
        
        // Disable Reset Round button
        btnResetRound.Enabled = false;
        btnResetRound.BackColor = Color.Gray;
        
        // Disable remaining active buttons during theme
        btnLightsDown.Enabled = false;
        btnLightsDown.BackColor = Color.Gray;
        
        btnLifeline1.Enabled = false;
        btnLifeline1.BackColor = Color.Gray;
        btnLifeline2.Enabled = false;
        btnLifeline2.BackColor = Color.Gray;
        btnLifeline3.Enabled = false;
        btnLifeline3.BackColor = Color.Gray;
        btnLifeline4.Enabled = false;
        btnLifeline4.BackColor = Color.Gray;
        
        btnShowMoneyTree.Enabled = false;
        btnShowMoneyTree.BackColor = Color.Gray;
        
        btnActivateRiskMode.Enabled = false;
        btnActivateRiskMode.BackColor = Color.Gray;
        
        // Play closing theme with immediate priority (crossfades from underscore)
        _soundService.QueueSoundByKey("ClosingTheme", AudioPriority.Immediate);
        
        // Subscribe to completion event to trigger CompleteClosing when theme finishes
        EventHandler? completionHandler = null;
        completionHandler = (s, e) =>
        {
            // Unsubscribe to prevent multiple triggers
            _soundService.EffectsQueueCompleted -= completionHandler;
            
            // Call CompleteClosing on UI thread
            if (!IsDisposed)
            {
                BeginInvoke(new Action(CompleteClosing));
            }
        };
        _soundService.EffectsQueueCompleted += completionHandler;
        
        // Notify web clients that game is complete
        _ = Task.Run(async () =>
        {
            try
            {
                var sessionId = await GetActiveSessionIdAsync();
                if (!string.IsNullOrEmpty(sessionId))
                {
                    await _webServerHost!.BroadcastGameStateAsync(
                        sessionId,
                        GameStateType.GameComplete,
                        "Thank you for watching! The show has concluded.",
                        null
                    );
                    
                    if (Program.DebugMode)
                    {
                        GameConsole.Log("[Closing] Broadcasted GameComplete state to web clients");
                    }
                }
            }
            catch (Exception ex)
            {
                GameConsole.Log($"[Closing] Error broadcasting game complete: {ex.Message}");
            }
        });
        
        if (Program.DebugMode)
        {
            GameConsole.Log("[Closing] Stage: Theme - playing sound, will auto-complete when finished");
        }
    }
    
    private void CompleteClosing()
    {
        _closingStage = ClosingStage.Complete;
        btnClosing.BackColor = Color.Gray;
        btnClosing.Enabled = false;
        
        // Stop and dispose timer if still running
        if (_closingTimer != null)
        {
            _closingTimer.Stop();
            _closingTimer.Dispose();
            _closingTimer = null;
        }
        
        if (Program.DebugMode)
        {
            GameConsole.Log("[Closing] Complete - game is over, shutting down");
        }
        
        // Stop all sounds
        _soundService.StopAllSounds();
        
        // Clear game winner display from screens
        _screenService.ClearGameWinnerDisplay();
        
        // Export telemetry data to CSV if any rounds were played
        if (_roundNumber > 0)
        {
            try
            {
                _telemetryService.CompleteGame();
                var excelPath = _telemetryExportService.ExportWithDefaults(_telemetryService.GetCurrentGameData());
                GameConsole.Info($"[Telemetry] Game statistics exported to: {excelPath}");
            }
            catch (Exception ex)
            {
                GameConsole.Error($"[Telemetry] Failed to export telemetry: {ex.Message}");
            }
        }
        
        // Clear all visual elements on screens to create pristine "blank slate" appearance
        // This gives the impression of a reset without actually resetting game data
        _screenService.ShowQuestion(false);  // Hide question/answers display on all screens
        _screenService.RevealAnswer(string.Empty, string.Empty, false);  // Clear reveal state (removes green/red highlighting)
        _screenService.HideWinnings();  // Clear money tree display
        _screenService.ClearQuestionAndAnswerText();  // Clear question/answer text on host/guest screens
        
        // Clear all questions and displays on control panel
        nmrLevel.Value = 0;
        txtQuestion.Clear();
        txtA.Clear();
        txtB.Clear();
        txtC.Clear();
        txtD.Clear();
        txtExplain.Clear();
        lblAnswer.Text = string.Empty;
        txtID.Clear();
        
        // Reset closing stage
        _closingStage = ClosingStage.NotStarted;
        
        // Reset round number and telemetry
        _roundNumber = 0;
        _telemetryService.Reset();
        
        // DISABLE ALL BUTTONS - game is completely over
        btnHostIntro.Enabled = false;
        btnHostIntro.BackColor = Color.Gray;
        
        btnPickPlayer.Enabled = false;
        btnPickPlayer.BackColor = Color.Gray;
        
        btnExplainGame.Enabled = false;
        btnExplainGame.BackColor = Color.Gray;
        
        btnLightsDown.Enabled = false;
        btnLightsDown.BackColor = Color.Gray;
        
        btnNewQuestion.Enabled = false;
        btnNewQuestion.BackColor = Color.Gray;
        
        btnReveal.Enabled = false;
        btnReveal.BackColor = Color.Gray;
        
        btnWalk.Enabled = false;
        btnWalk.BackColor = Color.Gray;
        
        btnA.Enabled = false;
        btnA.BackColor = Color.Gray;
        btnB.Enabled = false;
        btnB.BackColor = Color.Gray;
        btnC.Enabled = false;
        btnC.BackColor = Color.Gray;
        btnD.Enabled = false;
        btnD.BackColor = Color.Gray;
        
        btnLifeline1.Enabled = false;
        btnLifeline1.BackColor = Color.Gray;
        btnLifeline2.Enabled = false;
        btnLifeline2.BackColor = Color.Gray;
        btnLifeline3.Enabled = false;
        btnLifeline3.BackColor = Color.Gray;
        btnLifeline4.Enabled = false;
        btnLifeline4.BackColor = Color.Gray;
        
        btnShowMoneyTree.Enabled = false;
        btnShowMoneyTree.BackColor = Color.Gray;
        
        // Keep Reset Game enabled (red) for manual app reset or shutdown
        btnResetGame.Enabled = true;
        btnResetGame.BackColor = Color.Gray;
        btnResetGame.FlatAppearance.BorderColor = Color.Red;
        btnResetGame.FlatAppearance.BorderSize = 3;
        btnResetGame.ForeColor = Color.Black;
        
        GameConsole.Info("[Closing] Show complete. Use Reset Game to restart or close the application.");
        
        // Disable all other broadcast buttons (grey)
        btnPickPlayer.Enabled = false;
        btnPickPlayer.BackColor = Color.Gray;
        
        btnExplainGame.Enabled = false;
        btnExplainGame.BackColor = Color.Gray;
        
        btnLightsDown.Enabled = false;
        btnLightsDown.BackColor = Color.Gray;
        
        btnNewQuestion.Enabled = false;
        btnNewQuestion.BackColor = Color.Gray;
        
        btnReveal.Enabled = false;
        btnReveal.BackColor = Color.Gray;
        
        btnWalk.Enabled = false;
        btnWalk.BackColor = Color.Gray;
        
        btnClosing.Enabled = false;
        btnClosing.BackColor = Color.Gray;
    }

    private void btnFadeOutAudio_Click(object? sender, EventArgs e)
    {
        // Fade out all sounds over 1.5 seconds
        _soundService.StopAllSounds(fadeout: true, fadeoutDurationMs: 1500);
    }

    private void btnStopAudio_Click(object? sender, EventArgs e)
    {
        // Run on background thread to prevent UI deadlock
        _ = _soundService.StopAllSoundsAsync();
    }

    private async void btnResetGame_Click(object? sender, EventArgs e)
    {
        // Cancel any running async operations
        _automatedSequenceCts?.Cancel();
        _automatedSequenceCts?.Dispose();
        _automatedSequenceCts = null;
        
        _lightsDownCts?.Cancel();
        _lightsDownCts?.Dispose();
        _lightsDownCts = null;
        
        // Reset to fresh initialization - stop all sounds first
        await _soundService.StopAllSoundsAsync();
        
        // Stop and dispose all timers
        _lifelineManager?.Reset();
        _moneyTreeDemoTimer?.Stop();
        _moneyTreeDemoTimer?.Dispose();
        _moneyTreeDemoTimer = null;
        _safetyNetAnimationTimer?.Stop();
        _safetyNetAnimationTimer?.Dispose();
        _safetyNetAnimationTimer = null;
        _closingTimer?.Stop();
        _closingTimer?.Dispose();
        _closingTimer = null;
        
        // Reset all state
        _isMoneyTreeDemoActive = false;
        _moneyTreeDemoLevel = 0;
        _isExplainGameActive = false;
        _pendingSafetyNetLevel = 0;
        _closingStage = ClosingStage.NotStarted;
        _isAutomatedSequenceRunning = false;
        _roundCompleted = false;
        _gameOutcome = GameOutcome.InProgress;
        _finalWinningsAmount = null; // Clear stored winnings amount
        
        // Reset game service
        _gameService.ResetGame();
        
        // Reset all controls
        ResetAllControls();

        // Reset FFF Online control panel state
        _fffWindow?.OnlinePanel.ResetFFFRound();
        
        // Clear winner display and confetti
        _screenService.ClearGameWinnerDisplay();
        
        // Clear screens
        _screenService.ResetAllScreens();
        
        // Reset to initial button states
        btnHostIntro.Enabled = true;
        btnHostIntro.BackColor = Color.LimeGreen;
        btnHostIntro.ForeColor = Color.Black;
        
        btnPickPlayer.Enabled = false;
        btnPickPlayer.BackColor = Color.Gray;
        
        btnExplainGame.Enabled = false;
        btnExplainGame.BackColor = Color.Gray;
        
        btnLightsDown.Enabled = false;
        btnLightsDown.BackColor = Color.Gray;
        
        btnClosing.Enabled = false;
        btnClosing.BackColor = Color.Gray;
        
        btnActivateRiskMode.Enabled = false;
        btnActivateRiskMode.BackColor = Color.Gray;
        
        btnResetGame.Enabled = false;
        btnResetGame.BackColor = Color.Gray;
        btnResetGame.FlatAppearance.BorderColor = Color.Black;
        btnResetGame.FlatAppearance.BorderSize = 2;
        btnResetGame.ForeColor = Color.Black;
        btnResetGame.Image = GetBlackStopImage();
        
        btnResetRound.Enabled = false;
        btnResetRound.BackColor = Color.Gray;
        btnResetRound.FlatAppearance.BorderColor = Color.Black;
        btnResetRound.FlatAppearance.BorderSize = 2;
        btnResetRound.ForeColor = Color.Black;
        btnResetRound.Image = GetBlackStopImage();
    }

    private void btnResetRound_Click(object? sender, EventArgs e)
    {
        // Reset to after Host Intro - ready for Pick a Player
        _ = Task.Run(async () => await _soundService.StopAllSoundsAsync());
        
        // Stop and dispose all timers
        _lifelineManager?.Reset();
        _moneyTreeDemoTimer?.Stop();
        _moneyTreeDemoTimer?.Dispose();
        _moneyTreeDemoTimer = null;
        _safetyNetAnimationTimer?.Stop();
        _safetyNetAnimationTimer?.Dispose();
        _safetyNetAnimationTimer = null;
        
        // Reset round state
        _isMoneyTreeDemoActive = false;
        _moneyTreeDemoLevel = 0;
        _isExplainGameActive = false;
        _pendingSafetyNetLevel = 0;
        _isAutomatedSequenceRunning = false;
        _roundCompleted = false;
        _gameOutcome = GameOutcome.InProgress;
        _finalWinningsAmount = null; // Clear stored winnings amount
        
        // Reset game service
        _gameService.ResetGame();
        
        // Reset all controls but preserve FFF window player state
        ResetAllControls(resetFFFWindow: false);

        // Reset FFF Online control panel state
        _fffWindow?.OnlinePanel.ResetFFFRound();
        
        // Clear winner display and confetti
        _screenService.ClearGameWinnerDisplay();
        
        // Clear screens
        _screenService.ResetAllScreens();
        
        // Set to checkpoint after Host Intro
        btnHostIntro.Enabled = false;
        btnHostIntro.BackColor = Color.Gray;
        
        btnPickPlayer.Enabled = true;
        btnPickPlayer.BackColor = Color.LimeGreen;
        btnPickPlayer.ForeColor = Color.Black;
        
        btnExplainGame.Enabled = false;
        btnExplainGame.BackColor = Color.Gray;
        
        btnLightsDown.Enabled = false;
        btnLightsDown.BackColor = Color.Gray;
        
        btnClosing.Enabled = false;
        btnClosing.BackColor = Color.Gray;
        
        // Risk Mode should be disabled (grey) - not available until Pick Player
        btnActivateRiskMode.Enabled = false;
        btnActivateRiskMode.BackColor = Color.Gray;
        
        // Reset Round should still be enabled
        btnResetRound.Enabled = true;
        btnResetRound.BackColor = Color.Gray;
        btnResetRound.FlatAppearance.BorderColor = Color.Red;
        btnResetRound.FlatAppearance.BorderSize = 3;
        btnResetRound.ForeColor = Color.Black;
        btnResetRound.Image = GetRedStopImage();
        
        // Reset Game should still be enabled
        btnResetGame.Enabled = true;
        btnResetGame.BackColor = Color.Gray;
        btnResetGame.FlatAppearance.BorderColor = Color.Red;
        btnResetGame.FlatAppearance.BorderSize = 3;
        btnResetGame.ForeColor = Color.Black;
        btnResetGame.Image = GetRedStopImage();
    }

    #endregion

    #region Helper Methods

    private int GetDroppedLevel(int currentQuestionNumber)
    {
        // Determine which safety net level the player drops to based on question number
        // Safety nets are at Q5 and Q10 by game design
        // If you're past Q10, drop to Q10; if past Q5, drop to Q5; otherwise drop to 0
        
        if (currentQuestionNumber > 10)
        {
            GameConsole.Debug($"[GetDroppedLevel] Q{currentQuestionNumber} -> Dropping to Q10 safety net");
            return 10;
        }
        else if (currentQuestionNumber > 5)
        {
            GameConsole.Debug($"[GetDroppedLevel] Q{currentQuestionNumber} -> Dropping to Q5 safety net");
            return 5;
        }
        else
        {
            GameConsole.Debug($"[GetDroppedLevel] Q{currentQuestionNumber} -> Dropping to Q0 (no safety net reached)");
            return 0;
        }
    }
    
    private int ParseMoneyValue(string moneyString)
    {
        // Remove currency symbols (including international ones), commas, and spaces
        var cleaned = moneyString;
        
        // Remove common currency symbols
        foreach (var symbol in new[] { "$", "€", "£", "¥", "₹", "₽", "¢" })
        {
            cleaned = cleaned.Replace(symbol, "");
        }
        
        // Remove formatting characters
        cleaned = cleaned.Replace(",", "").Replace(" ", "").Replace(".", "").Trim();
        
        if (int.TryParse(cleaned, out int value))
        {
            return value;
        }
        
        return 0;
    }

    private void SelectAnswer(string answer)
    {
        // ATH is now handled by second button click, not answer selection
        // Proceed directly with answer selection
        ContinueAnswerSelection(answer);
    }
    
    private void ContinueAnswerSelection(string answer)
    {
        // For Double Dip, just proceed with normal selection - DD logic handled in Reveal
        
        ResetAnswerColors();
        _currentAnswer = answer;

        // Highlight selected answer
        switch (answer)
        {
            case "A":
                txtA.BackColor = Color.Yellow;
                break;
            case "B":
                txtB.BackColor = Color.Yellow;
                break;
            case "C":
                txtC.BackColor = Color.Yellow;
                break;
            case "D":
                txtD.BackColor = Color.Yellow;
                break;
        }
        
        // Disable all answer buttons to lock in the answer (final answer)
        btnA.Enabled = false;
        btnB.Enabled = false;
        btnC.Enabled = false;
        btnD.Enabled = false;
        
        // Enable Reveal button (green)
        btnReveal.Enabled = true;
        btnReveal.BackColor = Color.LimeGreen;
        btnReveal.ForeColor = Color.Black;
        
        // Disable Walk Away once an answer is selected (grey)
        btnWalk.Enabled = false;
        btnWalk.BackColor = Color.Gray;
        
        // Return ONLY active (enabled) lifelines to standby mode (orange, not clickable)
        // Used/disabled lifelines are left alone
        if (btnLifeline1.Visible && btnLifeline1.Enabled)
        {
            btnLifeline1.BackColor = Color.Orange;
            btnLifeline1.Enabled = false;
        }
        if (btnLifeline2.Visible && btnLifeline2.Enabled)
        {
            btnLifeline2.BackColor = Color.Orange;
            btnLifeline2.Enabled = false;
        }
        if (btnLifeline3.Visible && btnLifeline3.Enabled)
        {
            btnLifeline3.BackColor = Color.Orange;
            btnLifeline3.Enabled = false;
        }
        if (btnLifeline4.Visible && btnLifeline4.Enabled)
        {
            btnLifeline4.BackColor = Color.Orange;
            btnLifeline4.Enabled = false;
        }

        // Broadcast answer selection to all screens
        _screenService.SelectAnswer(answer);
        
        // Clear ATA results from screens and web clients (if ATA was used)
        if (_lifelineManager != null)
        {
            _ = _lifelineManager.ClearATAFromScreens();
        }

        // Sound behavior changes based on question level:
        // Q1-5: Don't stop sounds, Q6+: Stop sounds
        // Q1-4: Don't play final answer, Q5+: Play final answer (now 1-indexed)
        var questionNumber = (int)nmrLevel.Value;
        
        if (questionNumber >= 6)
        {
            // Q6+: Stop all sounds and play final answer async
            _ = _soundService.StopAllSoundsAsync().ContinueWith(_ =>
            {
                PlayFinalAnswerSound();
            }, TaskScheduler.Default);
        }
        else if (questionNumber == 5)
        {
            // Q5: Don't stop sounds, but play final answer
            _ = Task.Run(() =>
            {
                if (!IsDisposed)
                {
                    PlayFinalAnswerSound();
                }
            });
        }
        // Q1-4: Don't stop sounds, don't play final answer (do nothing)
    }

    private void ResetAnswerColors()
    {
        txtA.BackColor = Color.Silver;
        txtB.BackColor = Color.Silver;
        txtC.BackColor = Color.Silver;
        txtD.BackColor = Color.Silver;
    }

    private void PlayFinalAnswerSound()
    {
        var questionNumber = _gameService.State.CurrentLevel; // Now 1-indexed
        
        if (Program.DebugMode)
        {
            GameConsole.Log($"[Sound] Looking for final answer sound for question #{questionNumber}");
        }
        
        var soundKey = questionNumber switch
        {
            1 => "Q01Final",
            2 => "Q02Final",
            3 => "Q03Final",
            4 => "Q04Final",
            5 => "Q05Final",
            6 => "Q06Final",
            7 => "Q07Final",
            8 => "Q08Final",
            9 => "Q09Final",
            10 => "Q10Final",
            11 => "Q11Final",
            12 => "Q12Final",
            13 => "Q13Final",
            14 => "Q14Final",
            15 => "Q15Final",
            _ => string.Empty
        };

        if (string.IsNullOrEmpty(soundKey))
        {
            if (Program.DebugMode)
            {
                GameConsole.Log($"[Sound] No final answer sound key for Q{questionNumber}");
            }
            return;
        }
        
        if (Program.DebugMode)
        {
            GameConsole.Log($"[Sound] Playing final answer sound for Q{questionNumber}: {soundKey}");
        }
        
        // Play without loop and store the identifier for later stopping
        _currentFinalAnswerKey = _soundService.PlaySoundByKey(soundKey, loop: false);
    }

    private void PlayLoseSound(int? questionNumber = null)
    {
        var currentQuestion = questionNumber ?? _gameService.State.CurrentLevel; // Now 1-indexed
        
        if (Program.DebugMode)
        {
            GameConsole.Log($"[Sound] Looking for lose sound for question #{currentQuestion}");
        }
        
        var soundKey = currentQuestion switch
        {
            1 => "Q01to05Wrong",
            2 => "Q01to05Wrong",
            3 => "Q01to05Wrong",
            4 => "Q01to05Wrong",
            5 => "Q01to05Wrong",
            6 => "Q06Wrong",
            7 => "Q07Wrong",
            8 => "Q08Wrong",
            9 => "Q09Wrong",
            10 => "Q10Wrong",
            11 => "Q11Wrong",
            12 => "Q12Wrong",
            13 => "Q13Wrong",
            14 => "Q14Wrong",
            15 => "Q15Wrong",
            _ => string.Empty
        };

        if (string.IsNullOrEmpty(soundKey))
        {
            if (Program.DebugMode)
            {
                GameConsole.Log($"[Sound] No lose sound key for Q{currentQuestion}");
            }
            return;
        }
        
        if (Program.DebugMode)
        {
            GameConsole.Log($"[Sound] Playing lose sound for Q{currentQuestion}: {soundKey}");
        }
        
        _soundService.PlaySoundByKey(soundKey);
    }

    private void PlayCorrectSound(int? questionNumber = null)
    {
        // Use passed questionNumber or fall back to nmrLevel (now 1-indexed)
        var currentQuestion = questionNumber ?? ((int)nmrLevel.Value);
        var isRiskMode = _gameService.State.Mode == GameMode.Risk;
        var settings = _gameService.MoneyTree.Settings;
        
        // Check if specific safety nets are disabled (either by Risk Mode or unchecked in settings)
        bool isQ5SafetyNetDisabled = isRiskMode || (settings.SafetyNet1 != 5 && settings.SafetyNet2 != 5);
        bool isQ10SafetyNetDisabled = isRiskMode || (settings.SafetyNet1 != 10 && settings.SafetyNet2 != 10);
        
        if (Program.DebugMode)
        {
            GameConsole.Log($"[Sound] Looking for correct sound for question #{currentQuestion}, Risk Mode: {isRiskMode}, Q5 disabled: {isQ5SafetyNetDisabled}, Q10 disabled: {isQ10SafetyNetDisabled}");
        }
        
        var soundKey = currentQuestion switch
        {
            1 => "Q01to04Correct",
            2 => "Q01to04Correct",
            3 => "Q01to04Correct",
            4 => "Q01to04Correct",
            5 => isQ5SafetyNetDisabled ? "Q05Correct2" : "Q05Correct",
            6 => "Q06Correct",
            7 => "Q07Correct",
            8 => "Q08Correct",
            9 => "Q09Correct",
            10 => isQ10SafetyNetDisabled ? "Q10Correct2" : "Q10Correct",
            11 => "Q11Correct",
            12 => "Q12Correct",
            13 => "Q13Correct",
            14 => "Q14Correct",
            15 => "Q15Correct",
            _ => string.Empty
        };

        if (string.IsNullOrEmpty(soundKey))
        {
            if (Program.DebugMode)
            {
                GameConsole.Log($"[Sound] No correct sound key for Q{currentQuestion}");
            }
            return;
        }
        
        if (Program.DebugMode)
        {
            GameConsole.Log($"[Sound] Playing correct sound for Q{currentQuestion}: {soundKey}");
        }
        
        _soundService.PlaySoundByKey(soundKey);
    }

    private void PlayQuestionBed()
    {
        var questionNumber = (int)nmrLevel.Value; // Now 1-indexed
        
        if (Program.DebugMode)
        {
            GameConsole.Log($"[Sound] Looking for bed music for question #{questionNumber}");
        }
        
        var soundKey = questionNumber switch
        {
            1 => "Q01to05Bed",
            2 => "Q01to05Bed",
            3 => "Q01to05Bed",
            4 => "Q01to05Bed",
            5 => "Q01to05Bed",
            6 => "Q06Bed",
            7 => "Q07Bed",
            8 => "Q08Bed",
            9 => "Q09Bed",
            10 => "Q10Bed",
            11 => "Q11Bed",
            12 => "Q12Bed",
            13 => "Q13Bed",
            14 => "Q14Bed",
            15 => "Q15Bed",
            _ => string.Empty
        };

        if (string.IsNullOrEmpty(soundKey))
        {
            if (Program.DebugMode)
            {
                GameConsole.Log($"[Sound] No bed music key for Q{questionNumber}");
            }
            return;
        }
        
        if (Program.DebugMode)
        {
            GameConsole.Log($"[Sound] Playing bed music for Q{questionNumber}: {soundKey}");
        }
        
        _soundService.PlaySoundByKey(soundKey, loop: true);
    }

    private void PlayLightsDownSound()
    {
        var questionNumber = (int)nmrLevel.Value; // Now 1-indexed
        
        if (Program.DebugMode)
        {
            GameConsole.Log($"[Sound] Looking for lights down sound for question #{questionNumber}");
        }
        
        var soundKey = questionNumber switch
        {
            1 => "Q01to05LightsDown",
            2 => "Q01to05LightsDown",
            3 => "Q01to05LightsDown",
            4 => "Q01to05LightsDown",
            5 => "Q01to05LightsDown",
            6 => "Q06LightsDown",
            7 => "Q07LightsDown",
            8 => "Q08LightsDown",
            9 => "Q09LightsDown",
            10 => "Q10LightsDown",
            11 => "Q11LightsDown",
            12 => "Q12LightsDown",
            13 => "Q13LightsDown",
            14 => "Q14LightsDown",
            15 => "Q15LightsDown",
            _ => string.Empty
        };

        if (string.IsNullOrEmpty(soundKey))
        {
            if (Program.DebugMode)
            {
                GameConsole.Log($"[Sound] No lights down sound key for Q{questionNumber}");
            }
            return;
        }
        
        if (Program.DebugMode)
        {
            GameConsole.Log($"[Sound] Queueing lights down sound for Q{questionNumber}: {soundKey}");
        }
        
        // Queue with custom threshold (-35dB instead of default -40dB, +5dB tolerance) to prevent premature silence detection on quiet tails
        _soundService.QueueSoundByKey(soundKey, AudioPriority.Normal, customThresholdDb: -35.0);
        // Note: Can't track individual queued sounds, so identifier not set
    }
    
    private async void ProcessNormalReveal(bool isCorrect)
    {
        GameConsole.Debug($"[ProcessNormalReveal] Starting, isCorrect: {isCorrect}");
        
        // Notify Stream Deck that question has ended
        _streamDeckIntegration?.OnQuestionEnd();
        
        if (isCorrect)
        {
            GameConsole.Debug("[ProcessNormalReveal] Correct answer path");
            // Correct answer
            switch (_currentAnswer)
            {
                case "A": txtA.BackColor = Color.LimeGreen; break;
                case "B": txtB.BackColor = Color.LimeGreen; break;
                case "C": txtC.BackColor = Color.LimeGreen; break;
                case "D": txtD.BackColor = Color.LimeGreen; break;
            }

            GameConsole.Debug("[ProcessNormalReveal] Set answer color");

            // Current question number (nmrLevel is now 1-indexed)
            var currentQuestionNumber = (int)nmrLevel.Value;

            GameConsole.Debug($"[ProcessNormalReveal] Current question: {currentQuestionNumber}");

            // If this is Q15, set GameWin flag immediately so money tree shows level 15
            if (currentQuestionNumber == 15)
            {
                _gameService.State.GameWin = true;
                _gameService.RefreshMoneyValues(); // Update CurrentValue to $1,000,000 for winning strap
            }

            // Advance to next level (but not beyond question 15)
            if (_gameService.State.CurrentLevel < 15)
            {
                _gameService.ChangeLevel(_gameService.State.CurrentLevel + 1);
                GameConsole.Debug($"[ProcessNormalReveal] Advanced to level {_gameService.State.CurrentLevel}");
            }

            // Broadcast correct answer to all screens in background to prevent blocking
            GameConsole.Debug("[ProcessNormalReveal] Broadcasting to screens");
            _ = Task.Run(() => _screenService.RevealAnswer(_currentAnswer, lblAnswer.Text, true));

            // Handle sounds based on question level
            // Q1-4: Bed music keeps playing, simple correct sound over bed
            // Q5+: Stop all sounds before playing custom correct answer sound
            if (currentQuestionNumber >= 5)
            {
                GameConsole.Debug($"[ProcessNormalReveal] Q{currentQuestionNumber}: Stopping all sounds");
                await _soundService.StopAllSoundsAsync();
                GameConsole.Debug("[ProcessNormalReveal] Sounds stopped");
            }
            else
            {
                GameConsole.Debug($"[ProcessNormalReveal] Q{currentQuestionNumber}: Keeping bed music, not stopping sounds");
            }
            
            _currentFinalAnswerKey = null;
            PlayCorrectSound(currentQuestionNumber);
            
            // Check if this is a safety net question (Q5 or Q10) and mark it as pending for manual lock-in
            var isSafetyNet = _gameService.MoneyTree.IsSafetyNet(currentQuestionNumber);
            if (isSafetyNet && !_gameService.MoneyTree.IsSafetyNetDisabledInRiskMode(currentQuestionNumber, _gameService.State.Mode))
            {
                _pendingSafetyNetLevel = currentQuestionNumber;
                
                if (Program.DebugMode)
                {
                    GameConsole.Log($"[SafetyNetAnimation] Q{currentQuestionNumber} is a safety net, lock-in available");
                    GameConsole.Log($"[SafetyNetAnimation] _pendingSafetyNetLevel set to {_pendingSafetyNetLevel}");
                }
            }
            else if (Program.DebugMode)
            {
                GameConsole.Log($"[SafetyNetAnimation] Q{currentQuestionNumber} - isSafetyNet={isSafetyNet}, disabled={_gameService.MoneyTree.IsSafetyNetDisabledInRiskMode(currentQuestionNumber, _gameService.State.Mode)}, mode={_gameService.State.Mode}");
            }
            
            // For Q1-4, restart bed music after correct answer if it was stopped by a lifeline
            // Not needed for Q5 (milestone) or any lose action
            if (currentQuestionNumber >= 1 && currentQuestionNumber <= 4 && _shouldRestartBedMusic)
            {
                if (Program.DebugMode)
                {
                    GameConsole.Log($"[BedMusic] Q{currentQuestionNumber} correct after lifeline - will restart bed music after 2 seconds");
                }
                
                // Wait for correct answer sound to play a bit, then restart bed music
                var bedMusicTimer = new System.Windows.Forms.Timer();
                bedMusicTimer.Interval = 2000;
                bedMusicTimer.Tick += (s, e) =>
                {
                    bedMusicTimer.Stop();
                    bedMusicTimer.Dispose();
                    PlayQuestionBed();
                    _shouldRestartBedMusic = false;
                };
                bedMusicTimer.Start();
            }
            
            // Auto-show winnings after 2 seconds using a timer
            var winningsTimer = new System.Windows.Forms.Timer();
            winningsTimer.Interval = 2000;
            winningsTimer.Tick += (s, e) =>
            {
                winningsTimer.Stop();
                winningsTimer.Dispose();
                ShowWinningsAndEnableButtons(currentQuestionNumber);
            };
            winningsTimer.Start();
            
            GameConsole.Debug("[ProcessNormalReveal] Correct path complete, timer started");
        }
        else
        {
            GameConsole.Debug("[ProcessNormalReveal] Wrong answer path");
            // Capture current question number for lose sound (1-indexed)
            var currentQuestionNumber = (int)nmrLevel.Value;
            
            _gameOutcome = GameOutcome.Wrong; // Track that player answered incorrectly
            _shouldRestartBedMusic = false; // Don't restart bed music on wrong answer
            
            // Wrong answer
            switch (_currentAnswer)
            {
                case "A": txtA.BackColor = Color.Red; break;
                case "B": txtB.BackColor = Color.Red; break;
                case "C": txtC.BackColor = Color.Red; break;
                case "D": txtD.BackColor = Color.Red; break;
            }

            // Highlight correct answer
            switch (lblAnswer.Text)
            {
                case "A": txtA.BackColor = Color.LimeGreen; break;
                case "B": txtB.BackColor = Color.LimeGreen; break;
                case "C": txtC.BackColor = Color.LimeGreen; break;
                case "D": txtD.BackColor = Color.LimeGreen; break;
            }

            // Broadcast wrong answer to all screens in background to prevent blocking
            _ = Task.Run(() => _screenService.RevealAnswer(_currentAnswer, lblAnswer.Text, false));

            // Stop all sounds and play lose sound with await
            await _soundService.StopAllSoundsAsync();
            PlayLoseSound(currentQuestionNumber);
            
            // Calculate the dropped level based on wrong value (safety net level or 0)
            int droppedLevel = GetDroppedLevel(currentQuestionNumber);
            
            // Wait for lose sound to play using a timer (stays on UI thread)
            var initialDelayTimer = new System.Windows.Forms.Timer();
            initialDelayTimer.Interval = 2000; // 2 seconds
            initialDelayTimer.Tick += (s, e) =>
            {
                initialDelayTimer.Stop();
                initialDelayTimer.Dispose();
                ContinueWrongAnswerSequence(droppedLevel);
            };
            initialDelayTimer.Start();
        }
    }

    /// <summary>
    /// Shows winnings and enables appropriate buttons after correct answer
    /// </summary>
    private void ShowWinningsAndEnableButtons(int currentQuestionNumber)
    {
        if (!chkShowWinnings.Checked)
        {
            chkShowWinnings.Checked = true;
        }
        
        // Disable Reveal button (grey)
        btnReveal.Enabled = false;
        btnReveal.BackColor = Color.Gray;
        
        // If Q5 was just answered correctly, enable Lights Down for Q6
        if (currentQuestionNumber == 5)
        {
            // Enable Lights Down (green) for Q6
            btnLightsDown.Enabled = true;
            btnLightsDown.BackColor = Color.LimeGreen;
            btnLightsDown.ForeColor = Color.Black;
            
            // Re-enable question level selector (can now change for Q6+)
            nmrLevel.Enabled = true;
        }
        // For Q1-Q4, re-enable Question button (green) for next question
        else if (currentQuestionNumber >= 1 && currentQuestionNumber <= 4)
        {
            _answerRevealStep = 0; // Reset for next question
            btnNewQuestion.Enabled = true;
            btnNewQuestion.BackColor = Color.LimeGreen;
            btnNewQuestion.ForeColor = Color.Black;
            
            // Disable Walk Away for Q1-Q4 (grey) - only available after milestones
            btnWalk.Enabled = false;
            btnWalk.BackColor = Color.Gray;
        }
        // For Q6-Q14, enable Lights Down (green)
        else if (currentQuestionNumber >= 6 && currentQuestionNumber <= 14)
        {
            _answerRevealStep = 0; // Reset for next question
            btnLightsDown.Enabled = true;
            btnLightsDown.BackColor = Color.LimeGreen;
            btnLightsDown.ForeColor = Color.Black;
            
            // Re-enable question level selector (can now change for next question)
            nmrLevel.Enabled = true;
            
            // Disable Walk Away until next question is fully revealed (grey)
            btnWalk.Enabled = false;
            btnWalk.BackColor = Color.Gray;
            
            // Enable Closing (green) on Q6+ if round 2 or higher - only immediately after answer reveal
            // This gets enabled here and then disabled again in LoadNewQuestion
            if (_roundNumber >= 2)
            {
                btnClosing.Enabled = true;
                btnClosing.BackColor = Color.LimeGreen;
                btnClosing.ForeColor = Color.Black;
            }
        }
        // For Q15, enable Thanks for Playing (green)
        else if (currentQuestionNumber == 15)
        {
            HandleQ15Win();
        }
    }

    /// <summary>
    /// Handles Q15 win sequence
    /// </summary>
    private void HandleQ15Win()
    {
        _gameService.State.GameWin = true; // Set flag immediately when Q15 is answered correctly
        
        // Ensure level is set to 15 (with 1-indexed system, Q15 = level 15)
        if (_gameService.State.CurrentLevel != 15)
        {
            _gameService.ChangeLevel(15);
        }
        
        _gameService.RefreshMoneyValues(); // Update CurrentValue to show top prize
        UpdateMoneyDisplay(); // Update control panel display to show $1,000,000
        UpdateMoneyTreeOnScreens(_gameService.MoneyTree.GetDisplayLevel(_gameService.State.CurrentLevel, _gameService.State.GameWin)); // Update money tree to level 15
        _gameOutcome = GameOutcome.Win; // Player won the game!
        _isAutomatedSequenceRunning = true; // Mark sequence as automated
        _roundCompleted = true; // Mark round as completed - prevents GameOver sound when Closing is clicked
        
        // Stop final answer sound immediately (it was already stopped above but ensure it's stopped)
        if (!string.IsNullOrEmpty(_currentFinalAnswerKey))
        {
            _currentFinalAnswerKey = null;
            _ = Task.Run(async () => await _soundService.StopAllSoundsAsync());
        }
        
        // Create cancellation token for this sequence
        _automatedSequenceCts?.Cancel();
        _automatedSequenceCts?.Dispose();
        _automatedSequenceCts = new CancellationTokenSource();
        
        // Disable Walk Away (grey)
        btnWalk.Enabled = false;
        btnWalk.BackColor = Color.Gray;
        
        // Disable Lights Down (grey)
        btnLightsDown.Enabled = false;
        btnLightsDown.BackColor = Color.Gray;
        
        // Enable Closing button (green) - round will complete after Q15 win sequence
        btnClosing.Enabled = true;
        btnClosing.BackColor = Color.LimeGreen;
        btnClosing.ForeColor = Color.Black;
        
        // Wait for Q15 correct sound to finish (approximately 20-30 seconds) using a timer
        var q15Timer = new System.Windows.Forms.Timer();
        q15Timer.Interval = 25000;
        q15Timer.Tick += async (s, e) =>
        {
            q15Timer.Stop();
            q15Timer.Dispose();
            
            // Check if sequence was cancelled
            if (_automatedSequenceCts != null && !_automatedSequenceCts.Token.IsCancellationRequested)
            {
                // Auto-trigger end-of-round sequence
                await EndRoundSequence();
            }
        };
        q15Timer.Start();
    }

    /// <summary>
    /// Continues the wrong answer sequence on the UI thread after the initial delay
    /// </summary>
    private void ContinueWrongAnswerSequence(int droppedLevel)
    {
        // If dropped to a safety net level (Q5 or Q10), play lock-in animation
        if (droppedLevel == 5 || droppedLevel == 10)
        {
            // Start safety net lock-in animation WITHOUT sound, stay on dropped level after animation
            StartSafetyNetAnimation(droppedLevel, playSound: false, targetLevelAfterAnimation: droppedLevel);
            
            // Wait for animation to complete using a timer (12 flashes Ãƒâ€” 400ms = 4800ms + small buffer)
            var completionTimer = new System.Windows.Forms.Timer();
            completionTimer.Interval = 5000;
            completionTimer.Tick += (s, e) =>
            {
                completionTimer.Stop();
                completionTimer.Dispose();
                FinishWrongAnswerSequence();
            };
            completionTimer.Start();
        }
        else
        {
            // No safety net, just update to level 0 immediately
            UpdateMoneyTreeOnScreens(droppedLevel);
            FinishWrongAnswerSequence();
        }
    }

    /// <summary>
    /// Finishes the wrong answer sequence after animation completes
    /// </summary>
    private void FinishWrongAnswerSequence()
    {
        // Store the wrong value as final winnings for TV screen display
        _finalWinningsAmount = _gameService.State.WrongValue;
        
        // Auto-show winnings after animation completes
        if (!chkShowWinnings.Checked)
        {
            chkShowWinnings.Checked = true;
        }
        
        // Disable Reveal button (grey)
        btnReveal.Enabled = false;
        btnReveal.BackColor = Color.Gray;
        
        // Disable Lights Down (grey)
        btnLightsDown.Enabled = false;
        btnLightsDown.BackColor = Color.Gray;
        
        // Enable Walk Away button (green) so host can manually trigger end-of-round sequence
        // This gives the host and player time to talk about the loss before moving forward
        btnWalk.Enabled = true;
        btnWalk.BackColor = Color.LimeGreen;
        btnWalk.ForeColor = Color.Black;
    }

    private void UpdateMoneyDisplay()
    {
        var state = _gameService.State;
        txtCorrect.Text = state.CorrectValue;
        txtCurrent.Text = state.CurrentValue;
        txtWrong.Text = state.WrongValue;
        txtDrop.Text = state.DropValue;
        txtQLeft.Text = state.QuestionsLeft;
    }

    private void ResetAllControls(bool resetFFFWindow = true)
    {
        txtQuestion.Clear();
        txtA.Clear();
        txtB.Clear();
        txtC.Clear();
        txtD.Clear();
        txtExplain.Clear();
        lblAnswer.Text = string.Empty;
        txtID.Clear();
        ResetAnswerColors();
        _currentAnswer = string.Empty;
        _answerRevealStep = 0; // Reset reveal state
        _gameOutcome = GameOutcome.InProgress; // Reset game outcome
        nmrLevel.Value = 0;
        nmrLevel.Enabled = false; // Disable until Pick a Player is clicked
        
        // Reset FFF Window state (offline mode only) - only if requested
        if (resetFFFWindow && _fffWindow != null && !_fffWindow.IsDisposed)
        {
            _fffWindow.OfflinePanel.ResetState();
        }
        
        // Disable answer buttons until a question is loaded and set to grey
        btnA.Enabled = false;
        btnA.BackColor = Color.Gray;
        btnB.Enabled = false;
        btnB.BackColor = Color.Gray;
        btnC.Enabled = false;
        btnC.BackColor = Color.Gray;
        btnD.Enabled = false;
        btnD.BackColor = Color.Gray;

        // Reset lifeline buttons
        _gameService.State.ResetLifelines();
        _lifelineMode = LifelineMode.Inactive;
        SetLifelineMode(LifelineMode.Inactive);
        
        // Reinitialize lifeline buttons (visibility, labels, default state)
        InitializeLifelineButtons();
        
        // Explicitly disable and grey out all lifeline buttons when round is over
        // Only apply to visible buttons
        if (btnLifeline1.Visible)
        {
            btnLifeline1.Enabled = false;
            btnLifeline1.BackColor = Color.Gray;
        }
        if (btnLifeline2.Visible)
        {
            btnLifeline2.Enabled = false;
            btnLifeline2.BackColor = Color.Gray;
        }
        if (btnLifeline3.Visible)
        {
            btnLifeline3.Enabled = false;
            btnLifeline3.BackColor = Color.Gray;
        }
        if (btnLifeline4.Visible)
        {
            btnLifeline4.Enabled = false;
            btnLifeline4.BackColor = Color.Gray;
        }
        
        // Reset closing state
        if (_closingTimer != null)
        {
            _closingTimer.Stop();
            _closingTimer.Dispose();
            _closingTimer = null;
        }
        _closingStage = ClosingStage.NotStarted;
        
        // Disable Host Intro (grey)
        btnHostIntro.Enabled = false;
        btnHostIntro.BackColor = Color.Gray;
        
        // Enable Pick Player for next player (green)
        btnPickPlayer.Enabled = true;
        btnPickPlayer.BackColor = Color.LimeGreen;
        btnPickPlayer.ForeColor = Color.Black;
        
        // Disable all other buttons (grey)
        btnExplainGame.Enabled = false;
        btnExplainGame.BackColor = Color.Gray;
        
        btnLightsDown.Enabled = false;
        btnLightsDown.BackColor = Color.Gray;
        
        btnNewQuestion.Enabled = false;
        btnNewQuestion.BackColor = Color.Gray;
        
        btnReveal.Enabled = false;
        btnReveal.BackColor = Color.Gray;
        
        btnWalk.Enabled = false;
        btnWalk.BackColor = Color.Gray;
        
        // Keep Closing enabled after first round completes
        if (_roundNumber >= 1)
        {
            btnClosing.Enabled = true;
            btnClosing.BackColor = Color.LimeGreen;
            btnClosing.ForeColor = Color.Black;
        }
        else
        {
            btnClosing.Enabled = false;
            btnClosing.BackColor = Color.Gray;
        }
        
        // Re-enable and reset risk mode button
        UpdateRiskModeButton();
    }

    private void SetLifelineMode(LifelineMode mode)
    {
        _lifelineMode = mode;
        
        switch (mode)
        {
            case LifelineMode.Inactive:
                // Grey - default state (disabled, not clickable)
                // Only apply to visible buttons
                if (btnLifeline1.Visible)
                    SetLifelineButtonColor(btnLifeline1, Color.Gray, false);
                if (btnLifeline2.Visible)
                    SetLifelineButtonColor(btnLifeline2, Color.Gray, false);
                if (btnLifeline3.Visible)
                    SetLifelineButtonColor(btnLifeline3, Color.Gray, false);
                if (btnLifeline4.Visible)
                    SetLifelineButtonColor(btnLifeline4, Color.Gray, false);
                break;
                
            case LifelineMode.Demo:
                // Yellow - demo mode
                // Only apply to visible buttons
                if (btnLifeline1.Visible)
                    SetLifelineButtonColor(btnLifeline1, Color.Yellow, true);
                if (btnLifeline2.Visible)
                    SetLifelineButtonColor(btnLifeline2, Color.Yellow, true);
                if (btnLifeline3.Visible)
                    SetLifelineButtonColor(btnLifeline3, Color.Yellow, true);
                if (btnLifeline4.Visible)
                    SetLifelineButtonColor(btnLifeline4, Color.Yellow, true);
                break;
                
            case LifelineMode.Standby:
                // Orange - standby mode (not clickable until all answers revealed)
                // Only apply to visible buttons that haven't been used yet
                // Grey/disabled buttons (used lifelines) are left alone
                if (btnLifeline1.Visible && !IsLifelineUsed(1))
                {
                    btnLifeline1.BackColor = Color.Orange;
                    btnLifeline1.Enabled = false;
                }
                if (btnLifeline2.Visible && !IsLifelineUsed(2))
                {
                    btnLifeline2.BackColor = Color.Orange;
                    btnLifeline2.Enabled = false;
                }
                if (btnLifeline3.Visible && !IsLifelineUsed(3))
                {
                    btnLifeline3.BackColor = Color.Orange;
                    btnLifeline3.Enabled = false;
                }
                if (btnLifeline4.Visible && !IsLifelineUsed(4))
                {
                    btnLifeline4.BackColor = Color.Orange;
                    btnLifeline4.Enabled = false;
                }
                break;
                
            case LifelineMode.Active:
                // Green - active mode (clickable)
                // Only apply to visible buttons that haven't been used
                // Check if Q15 - if so, disable STQ lifeline (now 1-indexed)
                var currentQuestionNumber = (int)nmrLevel.Value;
                var isQ15 = (currentQuestionNumber == 15);
                
                if (btnLifeline1.Visible && !IsLifelineUsed(1))
                {
                    // Check if this is STQ lifeline at Q15
                    if (isQ15 && GetLifelineTypeFromSettings(1) == Core.Models.LifelineType.SwitchQuestion)
                    {
                        btnLifeline1.BackColor = Color.Gray;
                        btnLifeline1.Enabled = false;
                    }
                    else
                    {
                        btnLifeline1.BackColor = Color.LimeGreen;
                        btnLifeline1.Enabled = true;
                    }
                }
                if (btnLifeline2.Visible && !IsLifelineUsed(2))
                {
                    // Check if this is STQ lifeline at Q15
                    if (isQ15 && GetLifelineTypeFromSettings(2) == Core.Models.LifelineType.SwitchQuestion)
                    {
                        btnLifeline2.BackColor = Color.Gray;
                        btnLifeline2.Enabled = false;
                    }
                    else
                    {
                        btnLifeline2.BackColor = Color.LimeGreen;
                        btnLifeline2.Enabled = true;
                    }
                }
                if (btnLifeline3.Visible && !IsLifelineUsed(3))
                {
                    // Check if this is STQ lifeline at Q15
                    if (isQ15 && GetLifelineTypeFromSettings(3) == Core.Models.LifelineType.SwitchQuestion)
                    {
                        btnLifeline3.BackColor = Color.Gray;
                        btnLifeline3.Enabled = false;
                    }
                    else
                    {
                        btnLifeline3.BackColor = Color.LimeGreen;
                        btnLifeline3.Enabled = true;
                    }
                }
                if (btnLifeline4.Visible && !IsLifelineUsed(4))
                {
                    // Check if this is STQ lifeline at Q15
                    if (isQ15 && GetLifelineTypeFromSettings(4) == Core.Models.LifelineType.SwitchQuestion)
                    {
                        btnLifeline4.BackColor = Color.Gray;
                        btnLifeline4.Enabled = false;
                    }
                    else
                    {
                        btnLifeline4.BackColor = Color.LimeGreen;
                        btnLifeline4.Enabled = true;
                    }
                }
                break;
        }
    }

    private void SetLifelineButtonColor(Button button, Color color, bool enabled)
    {
        button.BackColor = color;
        button.Enabled = enabled;
    }

    /// <summary>
    /// Initialize lifeline icons on all screens with their current types
    /// </summary>
    private void InitializeLifelineIcons()
    {
        // Don't clear existing icons - preserve Used states
        // Only initialize icons that don't exist yet
        
        // Set up icon types for visible lifelines
        // During explain phase: Hidden until pinged
        // During normal game: Normal (visible) immediately
        var initialState = _isExplainGameActive 
            ? Core.Graphics.LifelineIconState.Hidden 
            : Core.Graphics.LifelineIconState.Normal;
        
        // Only initialize lifelines up to TotalLifelines count
        int totalLifelines = _appSettings.Settings.TotalLifelines;
        
        for (int i = 1; i <= totalLifelines; i++)
        {
            var button = GetLifelineButtonByNumber(i);
            if (button != null && button.Visible)
            {
                var type = GetLifelineTypeFromSettings(i);
                // Check if this lifeline is already used
                var lifeline = _gameService.State.GetLifeline(type);
                if (lifeline != null && lifeline.IsUsed)
                {
                    // Preserve Used state
                    _screenService.SetLifelineIcon(i, type, Core.Graphics.LifelineIconState.Used);
                }
                else
                {
                    // Set to initial state (Hidden or Normal)
                    _screenService.SetLifelineIcon(i, type, initialState);
                }
            }
        }
    }
    
    private Button? GetLifelineButtonByNumber(int number) => number switch
    {
        1 => btnLifeline1,
        2 => btnLifeline2,
        3 => btnLifeline3,
        4 => btnLifeline4,
        _ => null
    };

    #endregion

    #region Menu Handlers

    private void DatabaseToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        using var dbDialog = new Options.DatabaseSettingsDialog(_sqlSettings.Settings);
        if (dbDialog.ShowDialog() == DialogResult.OK)
        {
            // Database settings changed - might need to reconnect
            GameConsole.Info("[Database] Settings saved. Please restart the application to apply changes.");
        }
    }

    private void QuestionsEditorToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        var editorForm = new Forms.QuestionEditor.QuestionEditorMainForm();
        editorForm.Show();
    }

    private void OptionsToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        using var optionsDialog = new Options.OptionsDialog(_appSettings.Settings, _appSettings);
        
        // Subscribe to settings applied event to update immediately when Apply is clicked
        optionsDialog.SettingsApplied += (s, ev) =>
        {
            // Reload sounds after settings change
            _soundService.LoadSoundsFromSettings(_appSettings.Settings);
            
            // Reinitialize lifeline buttons to reflect new settings (visibility, labels)
            InitializeLifelineButtons();
            
            // Reapply current lifeline mode to update colors
            SetLifelineMode(_lifelineMode);
            
            // Reload money tree settings and update risk mode button
            _gameService.MoneyTree.LoadSettings();
            UpdateRiskModeButton();
        };
        
        optionsDialog.ShowDialog(this);
    }

    private void HostScreenToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        if (_hostScreen == null || _hostScreen.IsDisposed)
        {
            _hostScreen = new HostScreenForm();
            _hostScreen.Initialize(_gameService.MoneyTree);
            _screenService.RegisterScreen(_hostScreen);
            _hostScreen.FormClosed += (s, args) => _screenService.UnregisterScreen(_hostScreen);
            
            // Subscribe to host messaging events
            MessageSent += _hostScreen.OnMessageReceived;
            
            // Sync current game state to the newly opened screen
            SyncScreenState(_hostScreen);
            
            _hostScreen.Show();
            
            // Auto fullscreen to assigned monitor if enabled
            if (_appSettings.Settings.FullScreenHostScreenEnable)
            {
                ApplyFullScreenToHostScreen(true, _appSettings.Settings.FullScreenHostScreenMonitor);
            }
        }
        else
        {
            _hostScreen.BringToFront();
        }
    }
#region Hotkey Handling

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        // Don't intercept keys when host message textbox has focus
        if (ActiveControl == txtHostMessage)
        {
            return base.ProcessCmdKey(ref msg, keyData);
        }
        
        if (_hotkeyHandler.ProcessKeyPress(keyData))
        {
            return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void LevelUp()
    {
        if (nmrLevel.Value < 15)
        {
            nmrLevel.Value++;
        }
    }

    private void LevelDown()
    {
        if (nmrLevel.Value > 0)
        {
            nmrLevel.Value--;
        }
    }

    #endregion

    
    private void GuestScreenToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        if (_guestScreen == null || _guestScreen.IsDisposed)
        {
            _guestScreen = new GuestScreenForm();
            _guestScreen.Initialize(_gameService.MoneyTree);
            _screenService.RegisterScreen(_guestScreen);
            _guestScreen.FormClosed += (s, args) => _screenService.UnregisterScreen(_guestScreen);
            
            // Sync current game state to the newly opened screen
            SyncScreenState(_guestScreen);
            
            _guestScreen.Show();
            
            // Auto fullscreen to assigned monitor if enabled
            if (_appSettings.Settings.FullScreenGuestScreenEnable)
            {
                ApplyFullScreenToGuestScreen(true, _appSettings.Settings.FullScreenGuestScreenMonitor);
            }
        }
        else
        {
            _guestScreen.BringToFront();
        }
    }

    /// <summary>
    /// Synchronizes the current game state to a newly opened screen
    /// </summary>
    private void SyncScreenState(IGameScreen screen)
    {
        // Sync current question if one is loaded
        if (_currentQuestion != null)
        {
            screen.UpdateQuestion(_currentQuestion);
            
            // Show visible answers progressively based on reveal step
            if (_answerRevealStep >= 2) screen.ShowAnswer("A");
            if (_answerRevealStep >= 3) screen.ShowAnswer("B");
            if (_answerRevealStep >= 4) screen.ShowAnswer("C");
            if (_answerRevealStep >= 5) screen.ShowAnswer("D");
            
            // If answer has been selected, show it
            if (!string.IsNullOrEmpty(_currentAnswer))
            {
                screen.SelectAnswer(_currentAnswer);
            }
        }
        
        // Sync lifeline icons if in demo mode or if they should be visible
        if (_isExplainGameActive || chkShowQuestion.Checked)
        {
            // Set up each lifeline icon
            for (int i = 1; i <= 4; i++)
            {
                var type = GetLifelineTypeFromSettings(i);
                screen.SetLifelineIcon(i, type, Core.Graphics.LifelineIconState.Normal);
            }
            screen.ShowLifelineIcons();
        }
        
        // Sync question visibility state
        if (chkShowQuestion.Checked)
        {
            screen.ShowQuestion(true);
        }
        
        // Sync winnings visibility state
        if (chkShowWinnings.Checked)
        {
            // Use the final winnings amount if available, otherwise current value
            var amountToShow = _finalWinningsAmount ?? _gameService.State.CurrentValue;
            if (screen is TVScreenForm scalableScreen)
            {
                scalableScreen.ShowWinningsAmount(amountToShow);
            }
            else
            {
                screen.ShowWinnings(_gameService.State);
            }
        }
        
        // Sync money tree visibility state
        if (btnShowMoneyTree.Text == "Hide Money Tree" || btnShowMoneyTree.Text == "Demo Money Tree" || btnShowMoneyTree.Text == "Demo Running...")
        {
            // Money tree is currently visible, show it on the new screen
            var currentState = _gameService.State;
            screen.ShowWinnings(currentState);
            
            // If demo is active, sync the current demo level (only works for scalable screens)
            if (_isMoneyTreeDemoActive && _moneyTreeDemoLevel > 0 && screen is TVScreenForm tvScalable)
            {
                tvScalable.UpdateMoneyTreeLevel(_moneyTreeDemoLevel);
            }
        }
        
        // Sync money display
        screen.UpdateMoney(
            _gameService.State.CurrentValue,
            _gameService.State.CorrectValue,
            _gameService.State.WrongValue,
            _gameService.State.DropValue,
            _gameService.State.QuestionsLeft
        );
    }

    private void TVScreenToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        if (_tvScreen == null || (_tvScreen as Form)?.IsDisposed == true)
        {
            // Use the new scalable TV screen
            var tvForm = new TVScreenForm();
            tvForm.Initialize(_gameService.MoneyTree);
            _tvScreen = tvForm;
            _screenService.RegisterScreen(_tvScreen);
            tvForm.FormClosed += (s, args) => _screenService.UnregisterScreen(_tvScreen);
            
            // Sync current game state to the newly opened screen
            SyncScreenState(_tvScreen);
            
            tvForm.Show();
            
            // Auto fullscreen to assigned monitor if enabled
            if (_appSettings.Settings.FullScreenTVScreenEnable)
            {
                ApplyFullScreenToTVScreen(true, _appSettings.Settings.FullScreenTVScreenMonitor);
            }
        }
        else
        {
            (_tvScreen as Form)?.BringToFront();
        }
    }

    private void PreviewScreenToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        // Toggle preview screen visibility
        if (_previewScreen != null && !_previewScreen.IsDisposed && _previewScreen.Visible)
        {
            _previewScreen.Close();
            _previewScreen = null;
            return;
        }
        
        // Check if orientation changed since last time
        var currentOrientation = _appSettings.Settings.PreviewOrientation == "Horizontal" 
            ? PreviewOrientation.Horizontal 
            : PreviewOrientation.Vertical;
        
        if (_previewScreen != null && !_previewScreen.IsDisposed)
        {
            if (_previewScreen.Orientation != currentOrientation)
            {
                // Orientation changed, recreate the window
                _previewScreen.Close();
                _previewScreen = null;
            }
            else
            {
                // Just show the existing window
                _previewScreen.Show();
                return;
            }
        }
        
        // Preview screen creates its own dedicated instances
        
        // Create preview screen with dedicated instances
        _previewScreen = new PreviewScreenForm(_gameService, _screenService, this, currentOrientation);
        _lastPreviewOrientation = currentOrientation;
        _previewScreen.Show();
    }

    private void CloseToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        Close();
    }
    
    private void UsageToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        // Future feature: Show usage documentation
        GameConsole.Info("[Help] Usage documentation will be available in a future update. Please refer to the README.md file in the project repository.");
    }
    
    private void CheckUpdatesToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        // Future feature: Check for updates
        GameConsole.Info("[Updates] Update checking will be available in a future update.");
    }

    private void AboutToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        using var aboutDialog = new About.AboutDialog();
        aboutDialog.ShowDialog(this);
    }

    #endregion

    private void nmrLevel_ValueChanged(object? sender, EventArgs e)
    {
        _gameService.ChangeLevel((int)nmrLevel.Value);
    }

    #region Host Messaging

    /// <summary>
    /// Sends a message to the host screen
    /// </summary>
    private void SendHostMessage()
    {
        if (string.IsNullOrWhiteSpace(txtHostMessage.Text))
            return;

        // Send message to host screen if it exists (including preview window)
        MessageSent?.Invoke(this, new HostMessageEventArgs
        {
            Message = txtHostMessage.Text.Trim()
        });

        GameConsole.Debug("[Host Message] Message sent to host screen: " + txtHostMessage.Text.Trim());
    }
    
    /// <summary>
    /// Clears the host message from the host screen
    /// </summary>
    private void ClearHostMessage()
    {
        txtHostMessage.Clear();
        MessageSent?.Invoke(this, new HostMessageEventArgs { Message = string.Empty });
        GameConsole.Debug("[Host Message] Message cleared from host screen");
    }
    
    /// <summary>
    /// Button click handler for clearing host messages
    /// </summary>
    private void btnClearHostMessage_Click(object? sender, EventArgs e)
    {
        ClearHostMessage();
    }

    /// <summary>
    /// Button click handler for sending host messages
    /// </summary>
    private void btnSendHostMessage_Click(object? sender, EventArgs e)
    {
        SendHostMessage();
    }

    /// <summary>
    /// KeyDown handler for host message textbox (Enter to send, Alt+Enter for newline)
    /// </summary>
    private void txtHostMessage_KeyDown(object? sender, KeyEventArgs e)
    {
        // Enter (without modifiers) to send message
        if (e.KeyCode == Keys.Enter && !e.Alt && !e.Control && !e.Shift)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
            SendHostMessage();
            return;
        }
        // Alt+Enter to insert newline - allow default behavior
    }

    #endregion
}



