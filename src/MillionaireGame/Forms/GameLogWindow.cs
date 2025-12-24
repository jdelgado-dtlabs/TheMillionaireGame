using System.Text;
using MillionaireGame.Utilities;

namespace MillionaireGame.Forms;

/// <summary>
/// Separate window for displaying game logs
/// </summary>
public partial class GameLogWindow : Form
{
    private readonly RichTextBox txtLog;
    private readonly ConsoleLogger _logger;

    public GameLogWindow()
    {
        InitializeComponent();

        // Initialize logger
        _logger = new ConsoleLogger("game", 5);
        _logger.StartLogging();

        // Configure log text box
        txtLog = new RichTextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            BackColor = Color.Black,
            ForeColor = Color.Lime,
            Font = new Font("Consolas", 10),
            BorderStyle = BorderStyle.None,
            WordWrap = true
        };

        Controls.Add(txtLog);

        // Write header
        LogHeader();
    }

    private void InitializeComponent()
    {
        Text = "MillionaireGame Console";
        Size = new Size(800, 600);
        StartPosition = FormStartPosition.Manual;
        
        // Position to the left
        Location = new Point(50, 50);
        
        FormBorderStyle = FormBorderStyle.Sizable;
        MinimumSize = new Size(400, 300);
        
        // Set icon
        try
        {
            var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lib", "image", "logo.ico");
            if (File.Exists(iconPath))
            {
                Icon = new Icon(iconPath);
            }
        }
        catch { /* Ignore icon load errors */ }
        
        // Don't close, just hide
        FormClosing += (s, e) =>
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        };
    }

    private void LogHeader()
    {
        txtLog.AppendText("===========================================\n");
        txtLog.AppendText("  THE MILLIONAIRE GAME - DEBUG MODE\n");
        txtLog.AppendText("===========================================\n");
        txtLog.AppendText($"Application starting at {DateTime.Now}\n");
        txtLog.AppendText("\n");
    }

    /// <summary>
    /// Logs a message to the window and file
    /// </summary>
    public void Log(string message)
    {
        if (InvokeRequired)
        {
            try
            {
                Invoke(new Action<string>(Log), message);
            }
            catch (ObjectDisposedException)
            {
                // Window was disposed, ignore
                return;
            }
            return;
        }

        try
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var formattedMessage = $"[{timestamp}] {message}\n";
            
            txtLog.AppendText(formattedMessage);
            txtLog.SelectionStart = txtLog.TextLength;
            txtLog.ScrollToCaret();
            
            _logger.Log(message);

            // Limit text length to prevent memory issues
            if (txtLog.TextLength > 100000)
            {
                txtLog.Text = txtLog.Text.Substring(txtLog.TextLength - 50000);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[GameLogWindow] Error logging: {ex.Message}");
        }
    }

    /// <summary>
    /// Logs a formatted message
    /// </summary>
    public void Log(string format, params object[] args)
    {
        Log(string.Format(format, args));
    }

    /// <summary>
    /// Logs a separator line
    /// </summary>
    public void LogSeparator()
    {
        if (InvokeRequired)
        {
            Invoke(new Action(LogSeparator));
            return;
        }

        txtLog.AppendText("-------------------------------------------\n");
        _logger.LogSeparator();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _logger?.Close();
        base.OnFormClosing(e);
    }
}
