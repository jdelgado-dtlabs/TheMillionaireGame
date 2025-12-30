using MillionaireGame.Services;
using MillionaireGame.Utilities;

namespace MillionaireGame.Forms;

/// <summary>
/// Window for managing Fastest Finger First rounds (switches between Online and Offline modes)
/// </summary>
public partial class FFFWindow : Form
{
    private readonly string _serverUrl;
    private bool _isWebServerRunning;
    private readonly ScreenUpdateService? _screenService;
    private readonly SoundService? _soundService;
    
    public FFFWindow(string serverUrl = "http://localhost:5278", bool isWebServerRunning = false, 
                     ScreenUpdateService? screenService = null, SoundService? soundService = null)
    {
        InitializeComponent();
        _serverUrl = serverUrl;
        _isWebServerRunning = isWebServerRunning;
        _screenService = screenService;
        _soundService = soundService;
    }
    
    /// <summary>
    /// Gets the FFF Online control panel for external access
    /// </summary>
    public FFFOnlinePanel OnlinePanel => fffOnlinePanel;
    
    /// <summary>
    /// Gets the FFF Offline control panel for external access
    /// </summary>
    public FFFOfflinePanel OfflinePanel => fffOfflinePanel;
    
    /// <summary>
    /// Gets the selected player name from offline mode
    /// </summary>
    public string? SelectedPlayerName => fffOfflinePanel.SelectedPlayerName;
    
    /// <summary>
    /// Gets whether no more players are available (2 or fewer remaining) - offline mode only
    /// </summary>
    public bool NoMorePlayers => fffOfflinePanel.NoMorePlayers;
    
    /// <summary>
    /// Updates FFF mode based on current web server state
    /// </summary>
    /// <param name="isWebServerRunning">True if web server is running, false for offline mode</param>
    public async Task UpdateModeAsync(bool isWebServerRunning)
    {
        // If mode hasn't changed, no need to update
        if (_isWebServerRunning == isWebServerRunning)
        {
            GameConsole.Debug($"[FFFWindow] Mode unchanged (isWebServerRunning={isWebServerRunning}), skipping update");
            return;
        }
        
        GameConsole.Info($"[FFFWindow] Updating mode from {(_isWebServerRunning ? "Online" : "Offline")} to {(isWebServerRunning ? "Online" : "Offline")}");
        
        _isWebServerRunning = isWebServerRunning;
        
        // Clear TV screen when switching modes
        ClearTVScreen();
        
        if (_isWebServerRunning)
        {
            // Switching to Online mode
            GameConsole.Log("[FFFWindow] Switching to Online (web server) mode");
            
            // Show web-based UI
            fffOnlinePanel.Visible = true;
            fffOfflinePanel.Visible = false;
            
            // Set services
            if (_soundService != null)
            {
                fffOnlinePanel.SetSoundService(_soundService);
            }
            
            if (_screenService != null)
            {
                fffOnlinePanel.SetScreenService(_screenService);
            }
            
            // Initialize button states
            fffOnlinePanel.UpdateUIState();
            
            // Initialize SignalR client
            GameConsole.Log("[FFFWindow] Initializing SignalR client...");
            await fffOnlinePanel.InitializeClientAsync(_serverUrl);
            GameConsole.Log("[FFFWindow] SignalR client initialized");
            
            // Load available questions
            GameConsole.Log("[FFFWindow] Loading FFF questions...");
            await fffOnlinePanel.LoadQuestionsAsync();
            GameConsole.Log("[FFFWindow] FFF questions loaded");
        }
        else
        {
            // Switching to Offline mode
            GameConsole.Log("[FFFWindow] Switching to Offline (local player) mode");
            
            // Show local player selection UI
            fffOnlinePanel.Visible = false;
            fffOfflinePanel.Visible = true;
            
            // Set services
            if (_soundService != null)
            {
                fffOfflinePanel.SetSoundService(_soundService);
            }
            
            if (_screenService != null)
            {
                fffOfflinePanel.SetScreenService(_screenService);
            }
            
            // Reset local UI state
            fffOfflinePanel.ResetState();
        }
    }
    
    private async void FFFWindow_Load(object sender, EventArgs e)
    {
        try
        {
            GameConsole.Log($"[FFFWindow] FFFWindow_Load called - _isWebServerRunning={_isWebServerRunning}, _serverUrl={_serverUrl}");
            
            if (_isWebServerRunning)
            {
                GameConsole.Log("[FFFWindow] Web server is running, showing web-based UI");
                
                // Show web-based UI
                fffOnlinePanel.Visible = true;
                fffOfflinePanel.Visible = false;
                
                // Set sound service for control panel
                if (_soundService != null)
                {
                    fffOnlinePanel.SetSoundService(_soundService);
                    GameConsole.Log("[FFFWindow] Sound service set for control panel");
                }
                
                // Set screen update service for TV display
                if (_screenService != null)
                {
                    fffOnlinePanel.SetScreenService(_screenService);
                    GameConsole.Log("[FFFWindow] Screen service set for control panel");
                }
                
                // Initialize button states
                fffOnlinePanel.UpdateUIState();
                GameConsole.Log("[FFFWindow] Button states initialized");
                
                // Initialize SignalR client
                GameConsole.Log("[FFFWindow] Initializing SignalR client...");
                await fffOnlinePanel.InitializeClientAsync(_serverUrl);
                GameConsole.Log("[FFFWindow] SignalR client initialized");
                
                // Load available questions
                GameConsole.Log("[FFFWindow] Loading FFF questions...");
                await fffOnlinePanel.LoadQuestionsAsync();
                GameConsole.Log("[FFFWindow] FFF questions loaded");
            }
            else
            {
                GameConsole.Log("[FFFWindow] Web server is NOT running, showing local player selection UI");
                
                // Show local player selection UI
                fffOnlinePanel.Visible = false;
                fffOfflinePanel.Visible = true;
                
                // Set services
                if (_soundService != null)
                {
                    fffOfflinePanel.SetSoundService(_soundService);
                }
                
                if (_screenService != null)
                {
                    fffOfflinePanel.SetScreenService(_screenService);
                }
                
                // Initialize local UI
                fffOfflinePanel.Initialize();
            }
        }
        catch (Exception ex)
        {
            GameConsole.Log($"[FFFWindow] ERROR in FFFWindow_Load: {ex.Message}");
            GameConsole.Log($"[FFFWindow] Stack trace: {ex.StackTrace}");
            MessageBox.Show($"Error initializing FFF window: {ex.Message}",
                "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private void FFFWindow_FormClosing(object sender, FormClosingEventArgs e)
    {
        // Don't actually close, just hide
        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            
            // Broadcast ResetToLobby to web clients before hiding
            if (_isWebServerRunning)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        await fffOnlinePanel.BroadcastResetToLobbyAsync();
                        GameConsole.Log("[FFFWindow] Broadcast ResetToLobby to web clients");
                    }
                    catch (Exception ex)
                    {
                        GameConsole.Error($"[FFFWindow] Error broadcasting ResetToLobby: {ex.Message}");
                    }
                });
            }
            
            // Clear TV screen when closing
            ClearTVScreen();
            
            // Clear the control's screen state (question/answers/winner)
            fffOnlinePanel?.ClearScreenForMainGame();
            
            Hide();
        }
    }
    
    private void ClearTVScreen()
    {
        _screenService?.ClearFFFDisplay();
    }
}
