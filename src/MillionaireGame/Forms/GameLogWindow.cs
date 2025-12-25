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
    /// Logs a message to the window and file with log level
    /// </summary>
    public void Log(string message, Utilities.LogLevel level = Utilities.LogLevel.INFO)
    {
        if (InvokeRequired)
        {
            try
            {
                Invoke(new Action<string, Utilities.LogLevel>(Log), message, level);
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
            var levelStr = level switch
            {
                Utilities.LogLevel.DEBUG => "DEBUG",
                Utilities.LogLevel.INFO => "INFO",
                Utilities.LogLevel.WARN => "WARN",
                Utilities.LogLevel.ERROR => "ERROR",
                _ => "INFO"
            };
            
            var formattedMessage = $"[{timestamp}] [{levelStr}] {message}\n";
            
            // Color code by log level
            var color = level switch
            {
                Utilities.LogLevel.DEBUG => Color.Gray,
                Utilities.LogLevel.INFO => Color.Lime,
                Utilities.LogLevel.WARN => Color.Yellow,
                Utilities.LogLevel.ERROR => Color.Red,
                _ => Color.Lime
            };
            
            txtLog.SelectionStart = txtLog.TextLength;
            txtLog.SelectionLength = 0;
            txtLog.SelectionColor = color;
            txtLog.AppendText(formattedMessage);
            txtLog.SelectionColor = txtLog.ForeColor;
            txtLog.ScrollToCaret();
            
            _logger.Log($"[{levelStr}] {message}");

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
    public void Log(string format, Utilities.LogLevel level, params object[] args)
    {
        Log(string.Format(format, args), level);
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
