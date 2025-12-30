using MillionaireGame.Services;
using MillionaireGame.Utilities;

namespace MillionaireGame.Forms;

/// <summary>
/// User control for managing Fastest Finger First rounds - OFFLINE MODE (Local player selection)
/// </summary>
public partial class FFFOfflinePanel : UserControl
{
    private readonly List<TextBox> _playerTextBoxes = new List<TextBox>();
    private System.Windows.Forms.Timer? _animationTimer;
    private int _animationTicks = 0;
    private int _currentHighlightedIndex = 0;
    private readonly Random _random = new Random();
    private bool _playersIntroduced = false;
    
    private SoundService? _soundService;
    private ScreenUpdateService? _screenService;
    
    /// <summary>
    /// Gets the selected player name from local mode
    /// </summary>
    public string? SelectedPlayerName { get; private set; }
    
    /// <summary>
    /// Gets whether no more players are available (2 or fewer remaining)
    /// </summary>
    public bool NoMorePlayers { get; private set; } = false;
    
    public FFFOfflinePanel()
    {
        InitializeComponent();
    }
    
    /// <summary>
    /// Sets the sound service for audio playback
    /// </summary>
    public void SetSoundService(SoundService soundService)
    {
        _soundService = soundService;
    }
    
    /// <summary>
    /// Sets the screen update service for TV display
    /// </summary>
    public void SetScreenService(ScreenUpdateService screenService)
    {
        _screenService = screenService;
    }
    
    /// <summary>
    /// Initializes the local player UI
    /// </summary>
    public void Initialize()
    {
        // Set initial player count to dropdown value (default 8)
        UpdatePlayerTextBoxes((int)cmbPlayerCount.Value);
        
        // Initially disable buttons until flow progresses
        btnPlayerIntro.Enabled = false;
        btnRandomSelect.Enabled = false;
    }
    
    /// <summary>
    /// Resets the local player state for a new game
    /// </summary>
    public void ResetState()
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
    
    private void cmbPlayerCount_ValueChanged(object? sender, EventArgs e)
    {
        UpdatePlayerTextBoxes((int)cmbPlayerCount.Value);
        
        // Reset flow when player count changes
        _playersIntroduced = false;
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
        
        // Queue FFFLightsDown sound
        _soundService?.QueueSound(SoundEffect.FFFLightsDown, AudioPriority.Normal);
        
        // Wait for sound to finish (monitor queue instead of fixed delay)
        while (_soundService?.IsQueuePlaying() == true)
        {
            await Task.Delay(100); // Check every 100ms
        }
        
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
        
        // Queue BOTH sounds - they'll play sequentially with automatic crossfade!
        _soundService?.QueueSound(SoundEffect.FFFRandomPicker, AudioPriority.Normal);
        _soundService?.QueueSound(SoundEffect.FFFWinner, AudioPriority.Normal);
        
        // Wait for random picker sound to finish before stopping animation
        // (Winner sound will be queued and waiting)
        int initialQueueSize = _soundService?.GetTotalSoundCount() ?? 0;
        while ((_soundService?.GetTotalSoundCount() ?? 0) > 1) // Wait until only winner sound remains
        {
            await Task.Delay(100);
        }
        
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
        
        // Winner sound is already queued and playing - just wait for it to finish
        while (_soundService?.IsQueuePlaying() == true)
        {
            await Task.Delay(100);
        }
        
        // Clear TV screen
        ClearTVScreen();
        
        // Set to "completed" (grey)
        btnRandomSelect.BackColor = Color.Gray;
        
        // Show result (no icon to suppress Windows system sound)
        MessageBox.Show($"{SelectedPlayerName} has been selected!",
            "Player Selected", MessageBoxButtons.OK, MessageBoxIcon.None);
        
        // Remove winner and shift remaining players
        RemovePlayerAndShift(winnerIndex);
        
        // Raise event to notify parent window that selection is complete
        OnPlayerSelected();
    }
    
    /// <summary>
    /// Event raised when a player is selected
    /// </summary>
    public event EventHandler? PlayerSelected;
    
    protected virtual void OnPlayerSelected()
    {
        PlayerSelected?.Invoke(this, EventArgs.Empty);
    }
    
    /// <summary>
    /// Removes the winning player and shifts remaining players up
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
