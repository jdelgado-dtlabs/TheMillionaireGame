using MillionaireGame.Utilities;

namespace MillionaireGame.Forms;

/// <summary>
/// Progress dialog that shows what's being shut down and how long each step takes
/// </summary>
public class ShutdownProgressDialog : Form
{
    private readonly ListBox _progressListBox;
    private readonly Label _statusLabel;
    private readonly Button _forceCloseButton;
    private readonly System.Diagnostics.Stopwatch _stopwatch;
    private readonly System.Windows.Forms.Timer _timeoutTimer;
    private bool _shutdownComplete = false;
    private bool _forceClose = false;

    public bool ForceClose => _forceClose;

    public ShutdownProgressDialog()
    {
        Text = "Shutting Down...";
        Size = new Size(600, 400);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ControlBox = false; // Prevent user from closing manually

        _progressListBox = new ListBox
        {
            Location = new Point(12, 12),
            Size = new Size(560, 280),
            Font = new Font("Consolas", 9F),
            IntegralHeight = false
        };

        _statusLabel = new Label
        {
            Location = new Point(12, 300),
            Size = new Size(560, 20),
            Text = "Shutting down application components..."
        };

        _forceCloseButton = new Button
        {
            Location = new Point(460, 330),
            Size = new Size(120, 30),
            Text = "Force Close",
            Enabled = false,
            DialogResult = DialogResult.Cancel
        };
        _forceCloseButton.Click += (s, e) =>
        {
            _forceClose = true;
            _statusLabel.Text = "Force closing application...";
            _forceCloseButton.Enabled = false;
            GameConsole.Warn("[Shutdown] User requested force close");
        };

        Controls.Add(_progressListBox);
        Controls.Add(_statusLabel);
        Controls.Add(_forceCloseButton);

        _stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Enable force close button after 10 seconds if not completed
        _timeoutTimer = new System.Windows.Forms.Timer();
        _timeoutTimer.Interval = 10000; // 10 seconds
        _timeoutTimer.Tick += (s, e) =>
        {
            if (!_shutdownComplete)
            {
                _forceCloseButton.Enabled = true;
                _statusLabel.Text = "Shutdown is taking longer than expected. You can force close if needed.";
                _statusLabel.ForeColor = Color.DarkOrange;
                GameConsole.Warn("[Shutdown] Timeout reached - enabling force close button");
            }
            _timeoutTimer.Stop();
        };
        _timeoutTimer.Start();
    }

    /// <summary>
    /// Add a shutdown step with timing information
    /// </summary>
    public void AddStep(string stepName, bool success, long elapsedMs)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => AddStep(stepName, success, elapsedMs)));
            return;
        }

        var status = success ? "✓" : "✗";
        var color = success ? "SUCCESS" : "FAILED";
        var message = $"[{_stopwatch.ElapsedMilliseconds:D5}ms] {status} {stepName} ({elapsedMs}ms)";
        
        _progressListBox.Items.Add(message);
        _progressListBox.TopIndex = _progressListBox.Items.Count - 1; // Auto-scroll

        GameConsole.Info($"[Shutdown] {stepName}: {color} in {elapsedMs}ms");
    }

    /// <summary>
    /// Add a message without timing
    /// </summary>
    public void AddMessage(string message)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => AddMessage(message)));
            return;
        }

        var msg = $"[{_stopwatch.ElapsedMilliseconds:D5}ms]   {message}";
        _progressListBox.Items.Add(msg);
        _progressListBox.TopIndex = _progressListBox.Items.Count - 1;
        
        // Log to GameConsole as well
        GameConsole.Info($"[Shutdown] {message}");
    }

    /// <summary>
    /// Update the status label
    /// </summary>
    public void UpdateStatus(string status)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => UpdateStatus(status)));
            return;
        }

        _statusLabel.Text = status;
        GameConsole.Debug($"[Shutdown] Status: {status}");
    }

    /// <summary>
    /// Mark shutdown as complete
    /// </summary>
    public void Complete()
    {
        if (InvokeRequired)
        {
            Invoke(new Action(Complete));
            return;
        }

        _shutdownComplete = true;
        _timeoutTimer.Stop();
        _statusLabel.Text = $"Shutdown complete. Total time: {_stopwatch.ElapsedMilliseconds}ms";
        _statusLabel.ForeColor = Color.Green;
        
        // Log completion to GameConsole with separator
        GameConsole.LogSeparator();
        GameConsole.Info($"[Shutdown] COMPLETE - Total time: {_stopwatch.ElapsedMilliseconds}ms");
        GameConsole.Info("[Shutdown] Application shutdown successful");
        GameConsole.LogSeparator();
        
        // Auto-close after 1 second
        var closeTimer = new System.Windows.Forms.Timer();
        closeTimer.Interval = 1000;
        closeTimer.Tick += (s, e) =>
        {
            closeTimer.Stop();
            DialogResult = DialogResult.OK;
            Close();
        };
        closeTimer.Start();
    }

    /// <summary>
    /// Mark shutdown as failed
    /// </summary>
    public void Failed(string reason)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => Failed(reason)));
            return;
        }

        _timeoutTimer.Stop();
        _statusLabel.Text = $"Shutdown failed: {reason}";
        _statusLabel.ForeColor = Color.Red;
        _forceCloseButton.Enabled = true;
        _forceCloseButton.Text = "Close Anyway";

        GameConsole.Error($"[Shutdown] FAILED: {reason}");
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // Only allow closing if shutdown is complete or force close is requested
        if (!_shutdownComplete && !_forceClose && e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
        }
        base.OnFormClosing(e);
    }
}
