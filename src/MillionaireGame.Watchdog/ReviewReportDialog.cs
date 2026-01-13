using System;
using System.Drawing;
using System.Windows.Forms;

namespace MillionaireGame.Watchdog
{
    /// <summary>
    /// Dialog to review sanitized crash report before submission
    /// Allows users to see exactly what will be submitted to GitHub
    /// </summary>
    public class ReviewReportDialog : Form
    {
        private readonly string _sanitizedReport;
        private readonly CrashInfo _crashInfo;
        
        // Form controls
        private Label lblTitle = null!;
        private Label lblInfo = null!;
        private TextBox txtReport = null!;
        private Label lblNote = null!;
        private Button btnClose = null!;
        private Button btnCopy = null!;

        public ReviewReportDialog(string sanitizedReport, CrashInfo crashInfo)
        {
            _sanitizedReport = sanitizedReport;
            _crashInfo = crashInfo;
            
            InitializeForm();
            InitializeControls();
            LoadReport();
        }

        private void InitializeForm()
        {
            this.Text = "Review Crash Report";
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(900, 700);
            this.MinimumSize = new Size(600, 400);
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 9F);
        }

        private void InitializeControls()
        {
            int y = 15;
            const int leftMargin = 20;
            const int rightMargin = 20;

            // Title
            lblTitle = new Label
            {
                Text = "📄 Review Crash Report",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 120, 212),
                AutoSize = true,
                Location = new Point(leftMargin, y)
            };
            this.Controls.Add(lblTitle);
            y += 40;

            // Info label
            lblInfo = new Label
            {
                Text = "This is the sanitized crash report that will be submitted to GitHub.\n" +
                       "All personal information (usernames, file paths, passwords) has been removed.",
                AutoSize = false,
                Size = new Size(this.ClientSize.Width - leftMargin - rightMargin, 40),
                Location = new Point(leftMargin, y),
                Font = new Font("Segoe UI", 9F),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            this.Controls.Add(lblInfo);
            y += 50;

            // Report text box (expandable)
            txtReport = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                Font = new Font("Consolas", 9F),
                BackColor = Color.FromArgb(250, 250, 250),
                Location = new Point(leftMargin, y),
                Size = new Size(this.ClientSize.Width - leftMargin - rightMargin, 
                               this.ClientSize.Height - y - 110),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            this.Controls.Add(txtReport);
            
            // Note label (at bottom)
            lblNote = new Label
            {
                Text = "🔒 Note: Personal information like usernames and file paths have been replaced with placeholders.",
                AutoSize = false,
                Size = new Size(this.ClientSize.Width - leftMargin - rightMargin, 30),
                Location = new Point(leftMargin, this.ClientSize.Height - 80),
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.Gray,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            this.Controls.Add(lblNote);

            // Copy button
            btnCopy = new Button
            {
                Text = "📋 Copy to Clipboard",
                Size = new Size(150, 35),
                Location = new Point(this.ClientSize.Width - leftMargin - 150 - 110, 
                                    this.ClientSize.Height - 45),
                Font = new Font("Segoe UI", 9F),
                FlatStyle = FlatStyle.System,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnCopy.Click += BtnCopy_Click;
            this.Controls.Add(btnCopy);

            // Close button
            btnClose = new Button
            {
                Text = "Close",
                Size = new Size(100, 35),
                Location = new Point(this.ClientSize.Width - leftMargin - 100, 
                                    this.ClientSize.Height - 45),
                Font = new Font("Segoe UI", 9F),
                DialogResult = DialogResult.OK,
                FlatStyle = FlatStyle.System,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            this.Controls.Add(btnClose);
        }

        private void LoadReport()
        {
            // Build the report with headers
            var reportBuilder = new System.Text.StringBuilder();
            
            reportBuilder.AppendLine("================================================================================");
            reportBuilder.AppendLine("                    SANITIZED CRASH REPORT");
            reportBuilder.AppendLine("================================================================================");
            reportBuilder.AppendLine();
            reportBuilder.AppendLine($"Exit Code:      {_crashInfo.ExitCode} (0x{_crashInfo.ExitCode:X8})");
            reportBuilder.AppendLine($"Exit Meaning:   {_crashInfo.ExitCodeMeaning ?? ProcessMonitor.GetExitCodeMeaning(_crashInfo.ExitCode)}");
            reportBuilder.AppendLine($"Crash Time:     {_crashInfo.CrashTime:yyyy-MM-dd HH:mm:ss}");
            reportBuilder.AppendLine($"Running Time:   {_crashInfo.RunningTime}");
            reportBuilder.AppendLine($"Last Activity:  {_crashInfo.LastActivity ?? "Unknown"}");
            reportBuilder.AppendLine($"Was Responsive: {(_crashInfo.WasResponsive ? "Yes" : "No (Application froze)")}");
            reportBuilder.AppendLine();
            reportBuilder.AppendLine("================================================================================");
            reportBuilder.AppendLine("                    SANITIZED DETAILS");
            reportBuilder.AppendLine("================================================================================");
            reportBuilder.AppendLine();
            reportBuilder.AppendLine(_sanitizedReport);
            reportBuilder.AppendLine();
            reportBuilder.AppendLine("================================================================================");
            reportBuilder.AppendLine("Note: Personal information has been replaced with placeholders:");
            reportBuilder.AppendLine("  <USERPATH>     - File paths containing your username");
            reportBuilder.AppendLine("  <MACHINE>      - Your computer name");
            reportBuilder.AppendLine("  <REDACTED>     - Passwords and sensitive data");
            reportBuilder.AppendLine("  <EMAIL>        - Email addresses");
            reportBuilder.AppendLine("  <IP>           - IP addresses");
            reportBuilder.AppendLine("================================================================================");

            txtReport.Text = reportBuilder.ToString();
            txtReport.SelectionStart = 0;
            txtReport.SelectionLength = 0;
        }

        private void BtnCopy_Click(object? sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtReport.Text))
            {
                Clipboard.SetText(txtReport.Text);
                
                // Show temporary feedback
                string originalText = btnCopy.Text;
                btnCopy.Text = "✅ Copied!";
                btnCopy.Enabled = false;
                
                var timer = new System.Windows.Forms.Timer { Interval = 1500 };
                timer.Tick += (s, args) =>
                {
                    btnCopy.Text = originalText;
                    btnCopy.Enabled = true;
                    timer.Stop();
                };
                timer.Start();
            }
        }
    }
}
