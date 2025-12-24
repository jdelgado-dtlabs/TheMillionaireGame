using MillionaireGame.Services;
using MillionaireGame.Utilities;

namespace MillionaireGame.Forms;

/// <summary>
/// Window for managing Fastest Finger First rounds
/// </summary>
public partial class FFFWindow : Form
{
    private readonly string _serverUrl;
    private readonly bool _isWebServerRunning;
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
    /// Gets the FFF control panel for external access
    /// </summary>
    public FFFControlPanel ControlPanel => fffControlPanel;
    
    /// <summary>
    /// Gets the selected player name from local mode
    /// </summary>
    public string? SelectedPlayerName { get; private set; }
    
    /// <summary>
    /// Gets whether no more players are available (2 or fewer remaining) - offline mode only
    /// </summary>
    public bool NoMorePlayers { get; private set; } = false;
    
    private async void FFFWindow_Load(object sender, EventArgs e)
    {
        try
        {
            GameConsole.Log($"[FFFWindow] FFFWindow_Load called - _isWebServerRunning={_isWebServerRunning}, _serverUrl={_serverUrl}");
            
            if (_isWebServerRunning)
            {
                GameConsole.Log("[FFFWindow] Web server is running, showing web-based UI");
                
                // Show web-based UI
                fffControlPanel.Visible = true;
                localPlayerPanel.Visible = false;
                
                // Set sound service for control panel
                if (_soundService != null)
                {
                    fffControlPanel.SetSoundService(_soundService);
                    GameConsole.Log("[FFFWindow] Sound service set for control panel");
                }
                
                // Initialize SignalR client
                GameConsole.Log("[FFFWindow] Initializing SignalR client...");
                await fffControlPanel.InitializeClientAsync(_serverUrl);
                GameConsole.Log("[FFFWindow] SignalR client initialized");
                
                // Load available questions
                GameConsole.Log("[FFFWindow] Loading FFF questions...");
                await fffControlPanel.LoadQuestionsAsync();
                GameConsole.Log("[FFFWindow] FFF questions loaded");
            }
            else
            {
                GameConsole.Log("[FFFWindow] Web server is NOT running, showing local player selection UI");
                
                // Show local player selection UI
                fffControlPanel.Visible = false;
                localPlayerPanel.Visible = true;
                
                // Initialize local UI
                InitializeLocalPlayerUI();
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
            
            // Clear TV screen when closing
            ClearTVScreen();
            
            Hide();
        }
    }
    
    #region Local Player Selection
    
    private List<TextBox> _playerTextBoxes = new List<TextBox>();
    private System.Windows.Forms.Timer? _animationTimer;
    private int _animationTicks = 0;
    private int _currentHighlightedIndex = 0;
    private Random _random = new Random();
    private bool _playersIntroduced = false;
    private int _currentIntroIndex = 0;
    
    private void InitializeLocalPlayerUI()
    {
        // Set initial player count to dropdown value (default 2)
        UpdatePlayerTextBoxes((int)cmbPlayerCount.Value);
        
        // Initially disable buttons until flow progresses
        btnPlayerIntro.Enabled = false;
        btnRandomSelect.Enabled = false;
    }
    
    /// <summary>
    /// Resets the local player state for a new game (offline mode only)
    /// </summary>
    public void ResetLocalPlayerState()
    {
        if (!_isWebServerRunning)
        {
            // Reset flags
            NoMorePlayers = false;
            _playersIntroduced = false;
            SelectedPlayerName = null;
            
            // Reset player count to default (8)
            cmbPlayerCount.Value = 8;
            UpdatePlayerTextBoxes(8);
            
            // Reset buttons to initial state
            btnFFFIntro.BackColor = Color.LimeGreen;
            btnFFFIntro.Enabled = true;
            
            btnPlayerIntro.BackColor = Color.Gray;
            btnPlayerIntro.Enabled = false;
            
            btnRandomSelect.BackColor = Color.Gray;
            btnRandomSelect.Enabled = false;
            
            // Clear TV screen
            ClearTVScreen();
        }
    }
    
    private void cmbPlayerCount_ValueChanged(object? sender, EventArgs e)
    {
        UpdatePlayerTextBoxes((int)cmbPlayerCount.Value);
        
        // Reset flow when player count changes
        _playersIntroduced = false;
        _currentIntroIndex = 0;
        btnFFFIntro.Enabled = true;
        btnPlayerIntro.Enabled = false;
        btnRandomSelect.Enabled = false;
    }
    
    private void UpdatePlayerTextBoxes(int playerCount)
    {
        // Clear existing text boxes
        foreach (var tb in _playerTextBoxes)
        {
            var label = pnlPlayers.Controls.OfType<Label>().FirstOrDefault(l => l.Top == tb.Top - 5);
            if (label != null)
            {
                pnlPlayers.Controls.Remove(label);
                label.Dispose();
            }
            pnlPlayers.Controls.Remove(tb);
            tb.Dispose();
        }
        _playerTextBoxes.Clear();
        
        // Create new text boxes
        int yPosition = 10;
        for (int i = 0; i < playerCount; i++)
        {
            var label = new Label
            {
                Text = $"Player {i + 1}:",
                Location = new Point(20, yPosition + 5),
                AutoSize = true,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            
            var textBox = new TextBox
            {
                Name = $"txtPlayer{i + 1}",
                Text = $"Player {i + 1}",
                Location = new Point(120, yPosition),
                Size = new Size(350, 25),
                MaxLength = 35,
                Font = new Font("Arial", 10),
                Tag = i
            };
            
            pnlPlayers.Controls.Add(label);
            pnlPlayers.Controls.Add(textBox);
            _playerTextBoxes.Add(textBox);
            
            yPosition += 40;
        }
        
        // Keep panel height fixed for 8 players to avoid visual bugs
        pnlPlayers.Height = 340; // Fixed height for 8 players (8 * 40 + 20)
    }
    
    private async void btnFFFIntro_Click(object? sender, EventArgs e)
    {
        // Set to "in progress" (blue)
        btnFFFIntro.BackColor = Color.DodgerBlue;
        btnFFFIntro.Enabled = false;
        
        // Play FFFLightsDown sound
        _soundService?.PlaySound(SoundEffect.FFFLightsDown, loop: false);
        
        // Wait for sound to finish (approximately 3 seconds)
        await Task.Delay(3000);
        
        // Set to "completed" (grey) and enable next step (green)
        btnFFFIntro.BackColor = Color.Gray;
        btnPlayerIntro.BackColor = Color.LimeGreen;
        btnPlayerIntro.Enabled = true;
    }
    
    private async void btnPlayerIntro_Click(object? sender, EventArgs e)
    {
        // Set to "in progress" (blue)
        btnPlayerIntro.BackColor = Color.DodgerBlue;
        btnPlayerIntro.Enabled = false;
        
        int playerCount = (int)cmbPlayerCount.Value;
        
        // Play appropriate meet sound once at the start
        PlayFFFMeetSound(playerCount);
        
        // Introduce each player automatically with 3-second delay
        for (int i = 0; i < playerCount; i++)
        {
            // Display current player on TV screen
            DisplayPlayerOnTV(i, _playerTextBoxes[i].Text);
            
            // Wait 3 seconds before next player
            await Task.Delay(3000);
        }
        
        _playersIntroduced = true;
        
        // Wait 2 seconds then show all players with straps
        await Task.Delay(2000);
        DisplayAllPlayersOnTV();
        
        // Set to "completed" (grey) and enable next step (green)
        btnPlayerIntro.BackColor = Color.Gray;
        btnRandomSelect.BackColor = Color.LimeGreen;
        btnRandomSelect.Enabled = true;
    }
    
    private async void btnRandomSelect_Click(object? sender, EventArgs e)
    {
        if (_playerTextBoxes.Count == 0 || !_playersIntroduced)
            return;
            
        // Set to "in progress" (blue)
        btnRandomSelect.BackColor = Color.DodgerBlue;
        btnRandomSelect.Enabled = false;
        
        // Reset all text boxes to white background
        foreach (var tb in _playerTextBoxes)
        {
            tb.BackColor = Color.White;
        }
        
        // Start animation
        _animationTicks = 0;
        _currentHighlightedIndex = 0;
        
        _animationTimer = new System.Windows.Forms.Timer();
        _animationTimer.Interval = 150; // Update every 150ms
        _animationTimer.Tick += AnimationTimer_Tick;
        _animationTimer.Start();
        
        // Play FFFRandomPicker sound
        _soundService?.PlaySound(SoundEffect.FFFRandomPicker, loop: false);
        
        // Wait for the 5-second duration of the sound
        await Task.Delay(5000);
        
        // Stop animation
        _animationTimer?.Stop();
        _animationTimer?.Dispose();
        
        // Select random winner
        int winnerIndex = _random.Next(_playerTextBoxes.Count);
        SelectedPlayerName = _playerTextBoxes[winnerIndex].Text;
        
        // Highlight winner on form
        foreach (var tb in _playerTextBoxes)
        {
            tb.BackColor = Color.White;
        }
        _playerTextBoxes[winnerIndex].BackColor = Color.LimeGreen;
        
        // Show winner on TV with blinking effect
        await ShowWinnerOnTV(winnerIndex, SelectedPlayerName);
        
        // Play winner sound
        _soundService?.PlaySound(SoundEffect.FFFWinner, loop: false);
        
        // Wait for winner sound to finish (approximately 5 seconds)
        await Task.Delay(5000);
        
        // Clear TV screen
        ClearTVScreen();
        
        // Set to "completed" (grey)
        btnRandomSelect.BackColor = Color.Gray;
        
        // Show result (no icon to suppress Windows system sound)
        MessageBox.Show($"{SelectedPlayerName} has been selected!",
            "Player Selected", MessageBoxButtons.OK, MessageBoxIcon.None);
        
        // For offline mode: Remove winner and shift remaining players
        if (!_isWebServerRunning)
        {
            RemovePlayerAndShift(winnerIndex);
        }
        
        // Hide window after selection (don't close - preserve state)
        Hide();
    }
    
    /// <summary>
    /// Removes the winning player and shifts remaining players up (offline mode only)
    /// </summary>
    private void RemovePlayerAndShift(int winnerIndex)
    {
        // Check if we're down to 2 or fewer players
        if (_playerTextBoxes.Count <= 2)
        {
            // Set flag - no more FFF rounds possible
            NoMorePlayers = true;
            return;
        }
        
        // Remove the winner's textbox and label from the panel
        var winnerTextBox = _playerTextBoxes[winnerIndex];
        var winnerLabel = pnlPlayers.Controls.OfType<Label>().FirstOrDefault(l => l.Top == winnerTextBox.Top - 5);
        
        pnlPlayers.Controls.Remove(winnerTextBox);
        if (winnerLabel != null)
        {
            pnlPlayers.Controls.Remove(winnerLabel);
            winnerLabel.Dispose();
        }
        winnerTextBox.Dispose();
        _playerTextBoxes.RemoveAt(winnerIndex);
        
        // Shift remaining players up
        for (int i = winnerIndex; i < _playerTextBoxes.Count; i++)
        {
            var tb = _playerTextBoxes[i];
            var label = pnlPlayers.Controls.OfType<Label>().FirstOrDefault(l => l.Top == tb.Top - 5);
            
            // Update label number
            if (label != null)
            {
                label.Text = $"Player {i + 1}:";
            }
            
            // Update textbox tag
            tb.Tag = $"player_{i}";
        }
        
        // Decrement player count
        cmbPlayerCount.Value = _playerTextBoxes.Count;
        
        // Reset button states for next round
        btnFFFIntro.BackColor = Color.LimeGreen;
        btnFFFIntro.Enabled = true;
        
        btnPlayerIntro.BackColor = Color.Gray;
        btnPlayerIntro.Enabled = false;
        
        btnRandomSelect.BackColor = Color.Gray;
        btnRandomSelect.Enabled = false;
        
        _playersIntroduced = false;
    }
    
    private void AnimationTimer_Tick(object? sender, EventArgs e)
    {
        _animationTicks++;
        
        // Reset previous highlight
        if (_currentHighlightedIndex >= 0 && _currentHighlightedIndex < _playerTextBoxes.Count)
        {
            _playerTextBoxes[_currentHighlightedIndex].BackColor = Color.White;
        }
        
        // Move to random player (more random as it goes)
        _currentHighlightedIndex = _random.Next(_playerTextBoxes.Count);
        
        // Highlight current player
        _playerTextBoxes[_currentHighlightedIndex].BackColor = Color.Yellow;
        
        // Also highlight on TV
        HighlightPlayerOnTV(_currentHighlightedIndex);
    }
    
    #endregion
    
    #region TV Screen Integration
    
    private void DisplayPlayerOnTV(int playerIndex, string playerName)
    {
        _screenService?.ShowFFFContestant(playerIndex, playerName);
    }
    
    private void DisplayAllPlayersOnTV()
    {
        var players = new List<string>();
        foreach (var tb in _playerTextBoxes)
        {
            players.Add(tb.Text);
        }
        _screenService?.ShowAllFFFContestants(players);
    }
    
    private void HighlightPlayerOnTV(int playerIndex)
    {
        _screenService?.HighlightFFFContestant(playerIndex);
    }
    
    private async Task ShowWinnerOnTV(int winnerIndex, string winnerName)
    {
        // Blink the winner's strap (6 blinks)
        for (int i = 0; i < 6; i++)
        {
            _screenService?.HighlightFFFContestant(winnerIndex, isWinner: true);
            await Task.Delay(250);
            _screenService?.HighlightFFFContestant(winnerIndex, isWinner: false);
            await Task.Delay(250);
        }
        
        // Show final winner state
        _screenService?.ShowFFFWinner(winnerName);
    }
    
    private void ClearTVScreen()
    {
        _screenService?.ClearFFFDisplay();
    }
    
    #endregion
    
    #region Sound Helpers
    
    private void PlayFFFMeetSound(int playerCount)
    {
        SoundEffect sound = playerCount switch
        {
            2 => SoundEffect.FFFMeet2,
            3 => SoundEffect.FFFMeet3,
            4 => SoundEffect.FFFMeet4,
            5 => SoundEffect.FFFMeet5,
            6 => SoundEffect.FFFMeet6,
            7 => SoundEffect.FFFMeet7,
            8 => SoundEffect.FFFMeet8,
            _ => SoundEffect.FFFMeet2
        };
        
        _soundService?.PlaySound(sound, loop: false);
    }
    
    #endregion
}
