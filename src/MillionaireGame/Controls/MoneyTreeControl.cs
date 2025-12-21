using MillionaireGame.Core.Services;
using MillionaireGame.Core.Settings;

namespace MillionaireGame.Controls;

/// <summary>
/// Custom control that displays the money tree with 15 prize levels
/// Highlights current level, shows safety nets, and supports animations
/// </summary>
public class MoneyTreeControl : UserControl
{
    private readonly MoneyTreeService _moneyTreeService;
    private readonly List<MoneyTreeLevelPanel> _levelPanels = new();
    private int _currentLevel = -1;
    private bool _animating = false;

    public MoneyTreeControl(MoneyTreeService moneyTreeService)
    {
        _moneyTreeService = moneyTreeService;
        InitializeControl();
    }

    private void InitializeControl()
    {
        BackColor = Color.Transparent;
        Size = new Size(250, 600);

        // Create 15 level panels from Q15 (top) to Q1 (bottom)
        for (int i = 15; i >= 1; i--)
        {
            var panel = new MoneyTreeLevelPanel(i, _moneyTreeService);
            panel.Location = new Point(10, (15 - i) * 38 + 5);
            panel.Size = new Size(230, 36);
            
            _levelPanels.Add(panel);
            Controls.Add(panel);
        }

        // Initial state - all levels unplayed
        UpdateDisplay();
    }

    /// <summary>
    /// Updates the money tree display for the current level
    /// </summary>
    public void SetCurrentLevel(int level)
    {
        if (_currentLevel == level) return;

        _currentLevel = level;
        UpdateDisplay();
    }

    /// <summary>
    /// Animates progression to a new level
    /// </summary>
    public async Task AnimateToLevel(int targetLevel)
    {
        if (_animating) return;
        _animating = true;

        try
        {
            // Fade out previous level
            if (_currentLevel >= 0 && _currentLevel < _levelPanels.Count)
            {
                await _levelPanels[_currentLevel].FadeOut();
            }

            _currentLevel = targetLevel;

            // Fade in new level
            if (_currentLevel >= 0 && _currentLevel < _levelPanels.Count)
            {
                await _levelPanels[_currentLevel].FadeIn();
            }

            UpdateDisplay();
        }
        finally
        {
            _animating = false;
        }
    }

    /// <summary>
    /// Shows safety net lock-in animation when passing Q5 or Q10
    /// </summary>
    public async Task AnimateSafetyNetLockIn(int level)
    {
        if (level < 0 || level >= _levelPanels.Count) return;

        await _levelPanels[level].FlashSafetyNet();
    }

    /// <summary>
    /// Reloads money tree data from settings (call after settings change)
    /// </summary>
    public void ReloadSettings()
    {
        _moneyTreeService.LoadSettings();
        foreach (var panel in _levelPanels)
        {
            panel.UpdatePrizeText();
        }
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        for (int i = 0; i < _levelPanels.Count; i++)
        {
            var panel = _levelPanels[i];
            var level = panel.Level;

            if (level == _currentLevel + 1) // +1 because CurrentLevel is 0-based
            {
                // Current question - highlight
                panel.SetState(MoneyTreeLevelState.Current);
            }
            else if (level < _currentLevel + 1)
            {
                // Passed levels - dimmed
                panel.SetState(MoneyTreeLevelState.Passed);
            }
            else
            {
                // Future levels - unplayed
                panel.SetState(MoneyTreeLevelState.Unplayed);
            }
        }
    }
}

/// <summary>
/// Individual panel for one money tree level
/// </summary>
public class MoneyTreeLevelPanel : Panel
{
    private readonly Label _lblLevel;
    private readonly Label _lblPrize;
    private readonly MoneyTreeService _moneyTreeService;
    private MoneyTreeLevelState _state = MoneyTreeLevelState.Unplayed;

    public int Level { get; }

    public MoneyTreeLevelPanel(int level, MoneyTreeService moneyTreeService)
    {
        Level = level;
        _moneyTreeService = moneyTreeService;

        // Panel setup
        BorderStyle = BorderStyle.FixedSingle;
        BackColor = Color.FromArgb(20, 20, 40);

        // Level number label (Q1, Q2, etc.)
        _lblLevel = new Label
        {
            Text = $"Q{level}",
            Font = new Font("Arial", 11F, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(5, 8),
            Size = new Size(40, 20),
            TextAlign = ContentAlignment.MiddleLeft
        };

        // Prize value label
        _lblPrize = new Label
        {
            Text = GetPrizeText(),
            Font = new Font("Arial", 11F, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(50, 8),
            Size = new Size(170, 20),
            TextAlign = ContentAlignment.MiddleRight
        };

        Controls.Add(_lblLevel);
        Controls.Add(_lblPrize);

        // Check if this is a safety net level
        if (_moneyTreeService.Settings.IsSafetyNet(level))
        {
            // Add visual indicator (border or different color)
            BorderStyle = BorderStyle.Fixed3D;
        }
    }

    public void UpdatePrizeText()
    {
        _lblPrize.Text = GetPrizeText();
    }

    private string GetPrizeText()
    {
        return _moneyTreeService.GetFormattedValue(Level);
    }

    public void SetState(MoneyTreeLevelState state)
    {
        _state = state;

        switch (state)
        {
            case MoneyTreeLevelState.Unplayed:
                BackColor = Color.FromArgb(20, 20, 40);
                _lblLevel.ForeColor = Color.White;
                _lblPrize.ForeColor = Color.White;
                break;

            case MoneyTreeLevelState.Current:
                BackColor = Color.FromArgb(255, 165, 0); // Orange
                _lblLevel.ForeColor = Color.Black;
                _lblPrize.ForeColor = Color.Black;
                break;

            case MoneyTreeLevelState.Passed:
                BackColor = Color.FromArgb(40, 40, 60);
                _lblLevel.ForeColor = Color.Gray;
                _lblPrize.ForeColor = Color.Gray;
                break;
        }
    }

    /// <summary>
    /// Fade in animation for level highlight
    /// </summary>
    public async Task FadeIn()
    {
        for (int alpha = 0; alpha <= 255; alpha += 25)
        {
            BackColor = Color.FromArgb(alpha, 255, 165, 0);
            await Task.Delay(30);
        }
    }

    /// <summary>
    /// Fade out animation when leaving level
    /// </summary>
    public async Task FadeOut()
    {
        for (int alpha = 255; alpha >= 0; alpha -= 25)
        {
            BackColor = Color.FromArgb(alpha, 255, 165, 0);
            await Task.Delay(30);
        }
    }

    /// <summary>
    /// Flash animation for safety net lock-in
    /// </summary>
    public async Task FlashSafetyNet()
    {
        var originalColor = BackColor;
        
        for (int i = 0; i < 3; i++)
        {
            BackColor = Color.Gold;
            await Task.Delay(200);
            BackColor = originalColor;
            await Task.Delay(200);
        }
    }
}

/// <summary>
/// Visual states for money tree levels
/// </summary>
public enum MoneyTreeLevelState
{
    Unplayed,  // Future level, not yet reached
    Current,   // Current question being played
    Passed     // Already completed level
}
