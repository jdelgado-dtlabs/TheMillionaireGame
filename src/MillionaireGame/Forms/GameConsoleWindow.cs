using System.Text;
using MillionaireGame.Utilities;
using MillionaireGame.Helpers;

namespace MillionaireGame.Forms;

/// <summary>
/// Window that displays game logs by tailing the log file
/// </summary>
public partial class GameConsoleWindow : Form
{
    private readonly RichTextBox txtLog;
    private readonly System.Windows.Forms.Timer _refreshTimer;
    private long _lastFilePosition;
    private int _lastLineCount;

    public GameConsoleWindow()
    {
        InitializeComponent();

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
        
        // Setup refresh timer to tail the log file
        _refreshTimer = new System.Windows.Forms.Timer();
        _refreshTimer.Interval = 100; // Refresh every 100ms
        _refreshTimer.Tick += RefreshTimer_Tick;
        _refreshTimer.Start();

        // Apply icon after everything is set up
        IconHelper.ApplyToForm(this);
        
        // Load initial log content
        RefreshLogDisplay();
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
    /// Timer tick event to refresh log display from file
    /// </summary>
    private void RefreshTimer_Tick(object? sender, EventArgs e)
    {
        RefreshLogDisplay();
    }

    /// <summary>
    /// Notifies the window that the log has been updated (called by GameConsole)
    /// </summary>
    public void NotifyLogUpdated()
    {
        if (InvokeRequired)
        {
            try
            {
                BeginInvoke(new Action(RefreshLogDisplay));
            }
            catch (ObjectDisposedException)
            {
                // Window was disposed, ignore
            }
            return;
        }

        RefreshLogDisplay();
    }

    /// <summary>
    /// Reads new lines from the log file and displays them
    /// </summary>
    private void RefreshLogDisplay()
    {
        if (IsDisposed || !Visible)
            return;

        var logFilePath = GameConsole.CurrentLogFilePath;
        if (string.IsNullOrEmpty(logFilePath) || !File.Exists(logFilePath))
            return;

        try
        {
            // Read file without locking
            using var fs = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            
            // Check if file has new content
            if (fs.Length <= _lastFilePosition)
                return;

            // Seek to last read position
            fs.Seek(_lastFilePosition, SeekOrigin.Begin);

            // Read new lines
            using var reader = new StreamReader(fs, Encoding.UTF8);
            var newLines = new List<string>();
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                newLines.Add(line);
            }

            // Update position
            _lastFilePosition = fs.Position;

            // Display new lines with color coding
            if (newLines.Count > 0)
            {
                foreach (var logLine in newLines)
                {
                    AppendColoredLine(logLine);
                }

                txtLog.ScrollToCaret();

                // Limit text length to prevent memory issues
                if (txtLog.Lines.Length > 5000)
                {
                    var lines = txtLog.Lines.Skip(txtLog.Lines.Length - 2500).ToArray();
                    txtLog.Lines = lines;
                    _lastLineCount = lines.Length;
                }
                else
                {
                    _lastLineCount = txtLog.Lines.Length;
                }
            }
        }
        catch (IOException)
        {
            // File might be locked, try again next time
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[GameConsoleWindow] Error reading log file: {ex.Message}");
        }
    }

    /// <summary>
    /// Appends a line with color coding based on log level
    /// </summary>
    private void AppendColoredLine(string line)
    {
        Color color = Color.Lime; // Default

        if (line.Contains("[DEBUG]"))
            color = Color.Gray;
        else if (line.Contains("[INFO]"))
            color = Color.Lime;
        else if (line.Contains("[WARN]"))
            color = Color.Yellow;
        else if (line.Contains("[ERROR]"))
            color = Color.Red;

        txtLog.SelectionStart = txtLog.TextLength;
        txtLog.SelectionLength = 0;
        txtLog.SelectionColor = color;
        txtLog.AppendText(line + "\n");
        txtLog.SelectionColor = txtLog.ForeColor;
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _refreshTimer?.Stop();
        _refreshTimer?.Dispose();
        base.OnFormClosing(e);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _refreshTimer?.Dispose();
        }
        base.Dispose(disposing);
    }
}
