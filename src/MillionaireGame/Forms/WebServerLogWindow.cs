using System.Text;
using MillionaireGame.Utilities;
using MillionaireGame.Helpers;

namespace MillionaireGame.Forms;

/// <summary>
/// Separate window for displaying web server logs
/// </summary>
public partial class WebServerLogWindow : Form
{
    private readonly RichTextBox txtLog;
    private readonly ConsoleLogger _logger;

    public WebServerLogWindow()
    {
        InitializeComponent();

        // Initialize logger
        _logger = new ConsoleLogger("webserver", 5);
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
        
        // Apply icon after everything is set up
        IconHelper.ApplyToForm(this);
    }

    private void InitializeComponent()
    {
        Text = "Web Service Console";
        Size = new Size(800, 600);
        StartPosition = FormStartPosition.Manual;
        
        // Position to the right of the main form
        Location = new Point(100, 100);
        
        FormBorderStyle = FormBorderStyle.Sizable;
        MinimumSize = new Size(400, 300);
        
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
        txtLog.AppendText("  WEB-BASED AUDIENCE PARTICIPATION\n");
        txtLog.AppendText("  Service Console\n");
        txtLog.AppendText("===========================================\n");
        txtLog.AppendText($"Started at {DateTime.Now}\n");
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
                // Use BeginInvoke for async, non-blocking marshaling to UI thread
                BeginInvoke(new Action<string, Utilities.LogLevel>(Log), message, level);
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
            var formattedMessage = $"[{timestamp}] [{levelStr}] {message}";

            // Set color based on level
            txtLog.SelectionStart = txtLog.TextLength;
            txtLog.SelectionLength = 0;
            txtLog.SelectionColor = level switch
            {
                Utilities.LogLevel.DEBUG => Color.Gray,
                Utilities.LogLevel.INFO => Color.Lime,
                Utilities.LogLevel.WARN => Color.Yellow,
                Utilities.LogLevel.ERROR => Color.Red,
                _ => Color.Lime
            };
            
            txtLog.AppendText(formattedMessage + "\n");
            txtLog.SelectionColor = txtLog.ForeColor; // Reset color
            
            _logger.Log($"[{levelStr}] {message}");

            // Limit text length to prevent memory issues
            if (txtLog.TextLength > 100000)
            {
                txtLog.Text = txtLog.Text.Substring(txtLog.TextLength - 50000);
            }
        }
        catch (ObjectDisposedException)
        {
            // Window was disposed during operation, ignore
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
            // Use BeginInvoke for async, non-blocking marshaling to UI thread
            BeginInvoke(new Action(LogSeparator));
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
