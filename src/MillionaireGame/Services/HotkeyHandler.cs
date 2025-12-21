namespace MillionaireGame.Services;

/// <summary>
/// Handles keyboard shortcuts for the application
/// </summary>
public class HotkeyHandler
{
    private readonly Action? _onF1; // Answer A
    private readonly Action? _onF2; // Answer B
    private readonly Action? _onF3; // Answer C
    private readonly Action? _onF4; // Answer D
    private readonly Action? _onF5; // New Question
    private readonly Action? _onF6; // Reveal Answer
    private readonly Action? _onF7; // Lights Down
    private readonly Action? _onHome; // Toggle Tree
    private readonly Action? _onEnd; // Walk Away
    private readonly Action? _onInsert; // To Hot Seat
    private readonly Action? _onDelete; // Reset Game
    private readonly Action? _onTab; // Toggle Question Display
    private readonly Action? _onPageUp; // Level Up
    private readonly Action? _onPageDown; // Level Down
    private readonly Action? _onBackspace; // Undo
    private readonly Action? _onR; // Risk Mode

    public HotkeyHandler(
        Action? onF1 = null,
        Action? onF2 = null,
        Action? onF3 = null,
        Action? onF4 = null,
        Action? onF5 = null,
        Action? onF6 = null,
        Action? onF7 = null,
        Action? onHome = null,
        Action? onEnd = null,
        Action? onInsert = null,
        Action? onDelete = null,
        Action? onTab = null,
        Action? onPageUp = null,
        Action? onPageDown = null,
        Action? onBackspace = null,
        Action? onR = null)
    {
        _onF1 = onF1;
        _onF2 = onF2;
        _onF3 = onF3;
        _onF4 = onF4;
        _onF5 = onF5;
        _onF6 = onF6;
        _onF7 = onF7;
        _onHome = onHome;
        _onEnd = onEnd;
        _onInsert = onInsert;
        _onDelete = onDelete;
        _onTab = onTab;
        _onPageUp = onPageUp;
        _onPageDown = onPageDown;
        _onBackspace = onBackspace;
        _onR = onR;
    }

    /// <summary>
    /// Processes a key press event
    /// </summary>
    public bool ProcessKeyPress(Keys keyData)
    {
        var handled = true;

        switch (keyData)
        {
            case Keys.F1:
                _onF1?.Invoke();
                break;
            case Keys.F2:
                _onF2?.Invoke();
                break;
            case Keys.F3:
                _onF3?.Invoke();
                break;
            case Keys.F4:
                _onF4?.Invoke();
                break;
            case Keys.F5:
                _onF5?.Invoke();
                break;
            case Keys.F6:
                _onF6?.Invoke();
                break;
            case Keys.F7:
                _onF7?.Invoke();
                break;
            case Keys.End:
                _onEnd?.Invoke();
                break;
            case Keys.Insert:
                _onInsert?.Invoke();
                break;
            case Keys.Delete:
                _onDelete?.Invoke();
                break;
            case Keys.Tab:
                _onTab?.Invoke();
                break;
            case Keys.PageUp:
                _onPageUp?.Invoke();
                break;
            case Keys.PageDown:
                _onPageDown?.Invoke();
                break;
            case Keys.Back:
                _onBackspace?.Invoke();
                break;
            case Keys.R:
                _onR?.Invoke();
                break;
            default:
                handled = false;
                break;
        }

        return handled;
    }

    /// <summary>
    /// Processes a key press with modifiers (Ctrl, Alt)
    /// </summary>
    public bool ProcessKeyPress(Keys keyData, bool ctrlPressed, bool altPressed)
    {
        // Handle Ctrl+1-4 for lifeline activation (if needed in future)
        if (ctrlPressed)
        {
            switch (keyData)
            {
                case Keys.D1:
                case Keys.NumPad1:
                    // TODO: Lifeline 1
                    return true;
                case Keys.D2:
                case Keys.NumPad2:
                    // TODO: Lifeline 2
                    return true;
                case Keys.D3:
                case Keys.NumPad3:
                    // TODO: Lifeline 3
                    return true;
                case Keys.D4:
                case Keys.NumPad4:
                    // TODO: Lifeline 4
                    return true;
            }
        }

        return ProcessKeyPress(keyData);
    }
}
