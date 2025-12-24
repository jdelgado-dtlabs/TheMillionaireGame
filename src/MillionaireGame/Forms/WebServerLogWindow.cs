using System.Text;
using MillionaireGame.Utilities;

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
        txtLog.AppendText("  WEB-BASED AUDIENCE PARTICIPATION\n");
        txtLog.AppendText("  Service Console\n");
        txtLog.AppendText("===========================================\n");
        txtLog.AppendText($"Started at {DateTime.Now}\n");
        txtLog.AppendText("\n");
    }

    /// <summary>
    /// Logs a message to the window and file
    /// </summary>
    public void Log(string message)
    {
        if (InvokeRequired)
        {
            Invoke(new Action<string>(Log), message);
            return;
        }

        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        var formattedMessage = $"[{timestamp}] {message}\n";
        
        txtLog.AppendText(formattedMessage);
        _logger.Log(message);

        // Limit text length to prevent memory issues
        if (txtLog.TextLength > 100000)
        {
            txtLog.Text = txtLog.Text.Substring(txtLog.TextLength - 50000);
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
