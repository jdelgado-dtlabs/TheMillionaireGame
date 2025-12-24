using MillionaireGame.Forms;

namespace MillionaireGame.Utilities;

/// <summary>
/// Manages a separate window for game logging
/// </summary>
public static class GameConsole
{
    private static GameLogWindow? _logWindow;
    private static readonly object _lock = new object();

    /// <summary>
    /// Gets whether the console window is currently visible
    /// </summary>
    public static bool IsVisible => _logWindow != null && _logWindow.Visible;

    /// <summary>
    /// Sets the log window instance (called from Program.cs)
    /// </summary>
    public static void SetWindow(GameLogWindow window)
    {
        lock (_lock)
        {
            _logWindow = window;
            System.Diagnostics.Debug.WriteLine($"[GameConsole] Window set: {window != null}, IsDisposed: {window?.IsDisposed}");
        }
    }

    /// <summary>
    /// Shows the game log window
    /// </summary>
    public static void Show()
    {
        lock (_lock)
        {
            if (_logWindow == null || _logWindow.IsDisposed)
            {
                _logWindow = new GameLogWindow();
            }

            if (!_logWindow.Visible)
            {
                _logWindow.Show();
            }
        }
    }

    /// <summary>
    /// Hides the game log window
    /// </summary>
    public static void Hide()
    {
        lock (_lock)
        {
            _logWindow?.Hide();
        }
    }

    /// <summary>
    /// Logs a message to the game console window
    /// </summary>
    public static void Log(string message)
    {
        lock (_lock)
        {
            if (_logWindow != null && !_logWindow.IsDisposed)
            {
                _logWindow.Log(message);
            }
            else
            {
                // Fallback to debug output if window not available
                System.Diagnostics.Debug.WriteLine($"[GameConsole] Window not available: {message}");
            }
        }
    }

    /// <summary>
    /// Logs a formatted message with arguments
    /// </summary>
    public static void Log(string format, params object[] args)
    {
        _logWindow?.Log(format, args);
    }

    /// <summary>
    /// Logs a separator line
    /// </summary>
    public static void LogSeparator()
    {
        _logWindow?.LogSeparator();
    }
}
