using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace MillionaireGame.Watchdog
{
    /// <summary>
    /// Professional Windows Forms dialog for crash reporting
    /// Shows crash details and allows user to submit to GitHub or save locally
    /// </summary>
    public class CrashReportDialog : Form
    {
        private readonly CrashInfo _crashInfo;
        private string? _sanitizedReport;
        
        // User input
        public UserCrashContext UserContext { get; private set; }
        public bool ShouldSubmitToGitHub { get; private set; }
        
        // Form controls
        private Label lblHeader = null!;
        private Label lblCrashSummary = null!;
        private Label lblDescription = null!;
        private TextBox txtDescription = null!;
        private Label lblReproSteps = null!;
        private TextBox txtReproSteps = null!;
        private Label lblEmail = null!;
        private TextBox txtEmail = null!;
        private CheckBox chkIncludeSystemInfo = null!;
        private CheckBox chkIncludeLogs = null!;
        private Button btnReview = null!;
        private Button btnSubmit = null!;
        private Button btnSave = null!;
        private Button btnClose = null!;

        public CrashReportDialog(CrashInfo crashInfo)
        {
            _crashInfo = crashInfo;
            UserContext = new UserCrashContext();
            
            InitializeForm();
            InitializeControls();
            LoadCrashSummary();
            
            // Pre-generate sanitized report for review
            PrepareSanitizedReport();
        }

        private void InitializeForm()
        {
            this.Text = "Millionaire Game Crash Reporter";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new Size(650, 650);
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 9F);
        }

        private void InitializeControls()
        {
            int y = 20;
            const int leftMargin = 20;
            const int controlWidth = 590;

            // Header with icon
            lblHeader = new Label
            {
                Text = "⚠️  The application has crashed unexpectedly.",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(192, 0, 0),
                AutoSize = true,
                Location = new Point(leftMargin, y)
            };
            this.Controls.Add(lblHeader);
            y += 35;

            // Crash summary
            lblCrashSummary = new Label
            {
                AutoSize = false,
                Size = new Size(controlWidth, 100),
                Location = new Point(leftMargin, y),
                Font = new Font("Consolas", 8.5F),
                BackColor = Color.FromArgb(245, 245, 245),
                Padding = new Padding(10),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(lblCrashSummary);
            y += 110;

            // Description label
            lblDescription = new Label
            {
                Text = "What were you doing when this happened? (Optional - helps us fix the issue)",
                AutoSize = false,
                Size = new Size(controlWidth, 20),
                Location = new Point(leftMargin, y)
            };
            this.Controls.Add(lblDescription);
            y += 25;

            // Description text box
            txtDescription = new TextBox
            {
                Multiline = true,
                Size = new Size(controlWidth, 60),
                Location = new Point(leftMargin, y),
                ScrollBars = ScrollBars.Vertical,
                PlaceholderText = "Example: I was selecting an answer when the program froze..."
            };
            this.Controls.Add(txtDescription);
            y += 70;

            // Reproduction steps label
            lblReproSteps = new Label
            {
                Text = "Steps to reproduce (Optional):",
                AutoSize = false,
                Size = new Size(controlWidth, 20),
                Location = new Point(leftMargin, y)
            };
            this.Controls.Add(lblReproSteps);
            y += 25;

            // Reproduction steps text box
            txtReproSteps = new TextBox
            {
                Multiline = true,
                Size = new Size(controlWidth, 60),
                Location = new Point(leftMargin, y),
                ScrollBars = ScrollBars.Vertical,
                PlaceholderText = "Example: 1. Start new game 2. Answer first question 3. Select 50/50 lifeline..."
            };
            this.Controls.Add(txtReproSteps);
            y += 70;

            // Email label
            lblEmail = new Label
            {
                Text = "Email (optional - only for follow-up on this specific crash):",
                AutoSize = false,
                Size = new Size(controlWidth, 20),
                Location = new Point(leftMargin, y)
            };
            this.Controls.Add(lblEmail);
            y += 25;

            // Email text box
            txtEmail = new TextBox
            {
                Size = new Size(controlWidth, 25),
                Location = new Point(leftMargin, y),
                PlaceholderText = "your@email.com"
            };
            this.Controls.Add(txtEmail);
            y += 35;

            // Include system info checkbox
            chkIncludeSystemInfo = new CheckBox
            {
                Text = "Include system information (OS, .NET version, processor)",
                AutoSize = true,
                Location = new Point(leftMargin, y),
                Checked = true
            };
            this.Controls.Add(chkIncludeSystemInfo);
            y += 30;

            // Include logs checkbox
            chkIncludeLogs = new CheckBox
            {
                Text = "Include sanitized game logs (no personal data)",
                AutoSize = true,
                Location = new Point(leftMargin, y),
                Checked = true
            };
            this.Controls.Add(chkIncludeLogs);
            y += 40;

            // Review button
            btnReview = new Button
            {
                Text = "📄 Review Full Report...",
                Size = new Size(180, 35),
                Location = new Point(leftMargin, y),
                Font = new Font("Segoe UI", 9F),
                FlatStyle = FlatStyle.System
            };
            btnReview.Click += BtnReview_Click;
            this.Controls.Add(btnReview);
            y += 45;

            // Button panel at bottom
            int buttonY = y;
            
            // Submit button
            btnSubmit = new Button
            {
                Text = "Submit to GitHub",
                Size = new Size(150, 40),
                Location = new Point(leftMargin + 280, buttonY),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSubmit.FlatAppearance.BorderSize = 0;
            btnSubmit.Click += BtnSubmit_Click;
            this.Controls.Add(btnSubmit);

            // Save button
            btnSave = new Button
            {
                Text = "Save Locally",
                Size = new Size(110, 40),
                Location = new Point(leftMargin + 440, buttonY),
                Font = new Font("Segoe UI", 9F),
                FlatStyle = FlatStyle.System
            };
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            // Close button
            btnClose = new Button
            {
                Text = "Don't Send",
                Size = new Size(100, 40),
                Location = new Point(leftMargin + 560, buttonY),
                Font = new Font("Segoe UI", 9F),
                FlatStyle = FlatStyle.System
            };
            btnClose.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.Controls.Add(btnClose);
        }

        private void LoadCrashSummary()
        {
            string exitCodeMeaning = _crashInfo.ExitCodeMeaning ?? ProcessMonitor.GetExitCodeMeaning(_crashInfo.ExitCode);
            
            lblCrashSummary.Text = $"Exit Code:      {_crashInfo.ExitCode} (0x{_crashInfo.ExitCode:X8})\n" +
                                  $"Meaning:        {exitCodeMeaning}\n" +
                                  $"Last Activity:  {_crashInfo.LastActivity ?? "Unknown"}\n" +
                                  $"Running Time:   {FormatTimeSpan(_crashInfo.RunningTime)}\n" +
                                  $"Was Responsive: {(_crashInfo.WasResponsive ? "Yes" : "No (Application froze)")}";
        }

        private void PrepareSanitizedReport()
        {
            try
            {
                if (!string.IsNullOrEmpty(_crashInfo.CrashReportPath) && File.Exists(_crashInfo.CrashReportPath))
                {
                    string rawReport = File.ReadAllText(_crashInfo.CrashReportPath);
                    _sanitizedReport = DataSanitizer.SanitizeCrashReport(rawReport);
                }
                else
                {
                    _sanitizedReport = "Crash report file not found.";
                }
            }
            catch (Exception ex)
            {
                _sanitizedReport = $"Error reading crash report: {ex.Message}";
            }
        }

        private void BtnReview_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_sanitizedReport))
            {
                MessageBox.Show("No crash report available to review.", "Review Report",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var reviewDialog = new ReviewReportDialog(_sanitizedReport, _crashInfo);
            reviewDialog.ShowDialog(this);
        }

        private void BtnSubmit_Click(object? sender, EventArgs e)
        {
            // Capture user context
            UserContext.Description = txtDescription.Text.Trim();
            UserContext.ReproductionSteps = txtReproSteps.Text.Trim();
            UserContext.Email = txtEmail.Text.Trim();
            UserContext.IncludeSystemInfo = chkIncludeSystemInfo.Checked;
            UserContext.IncludeLogs = chkIncludeLogs.Checked;

            // Validate email format if provided
            if (!string.IsNullOrWhiteSpace(UserContext.Email) && !IsValidEmail(UserContext.Email))
            {
                MessageBox.Show("Please enter a valid email address or leave it blank.",
                    "Invalid Email", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return;
            }

            ShouldSubmitToGitHub = true;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(_crashInfo.CrashReportPath))
                {
                    MessageBox.Show("No crash report file to save.", "Save Report",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using var saveDialog = new SaveFileDialog
                {
                    Title = "Save Crash Report",
                    Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                    DefaultExt = "txt",
                    FileName = $"CrashReport_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
                };

                if (saveDialog.ShowDialog(this) == DialogResult.OK)
                {
                    File.Copy(_crashInfo.CrashReportPath, saveDialog.FileName, overwrite: true);
                    MessageBox.Show($"Crash report saved to:\n{saveDialog.FileName}",
                        "Report Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save crash report:\n{ex.Message}",
                    "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string FormatTimeSpan(TimeSpan ts)
        {
            if (ts.TotalHours >= 1)
                return $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
            else if (ts.TotalMinutes >= 1)
                return $"{ts.Minutes:D2}:{ts.Seconds:D2}";
            else
                return $"{ts.Seconds} seconds";
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
