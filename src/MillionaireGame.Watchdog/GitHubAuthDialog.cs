using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MillionaireGame.Watchdog
{
    /// <summary>
    /// Dialog for GitHub OAuth device flow authentication
    /// Shows user code and waits for browser authorization
    /// </summary>
    public class GitHubAuthDialog : Form
    {
        private readonly GitHubOAuthManager _oauthManager;
        private Task<AuthResult>? _authTask;
        
        // Form controls
        private Label lblTitle = null!;
        private Label lblInstructions = null!;
        private Label lblUserCode = null!;
        private TextBox txtUserCode = null!;
        private Button btnCopyCode = null!;
        private Button btnOpenBrowser = null!;
        private Label lblStatus = null!;
        private ProgressBar progressBar = null!;
        private Button btnCancel = null!;

        public GitHubAuthDialog()
        {
            _oauthManager = new GitHubOAuthManager();
            
            InitializeForm();
            InitializeControls();
            _ = StartAuthenticationFlow();
        }

        private void InitializeForm()
        {
            this.Text = "GitHub Authentication";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new Size(500, 400);
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 9F);
        }

        private void InitializeControls()
        {
            int y = 20;
            const int leftMargin = 30;
            const int controlWidth = 420;

            // Title
            lblTitle = new Label
            {
                Text = "🔑 Connect to GitHub",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 120, 212),
                AutoSize = true,
                Location = new Point(leftMargin, y)
            };
            this.Controls.Add(lblTitle);
            y += 50;

            // Instructions
            lblInstructions = new Label
            {
                Text = "To submit crash reports to GitHub, you need to authenticate.\n\n" +
                       "Step 1: Copy the code below\n" +
                       "Step 2: Click 'Open GitHub' to authorize in your browser\n" +
                       "Step 3: Paste the code when prompted\n\n" +
                       "This is a one-time setup.",
                AutoSize = false,
                Size = new Size(controlWidth, 120),
                Location = new Point(leftMargin, y),
                Font = new Font("Segoe UI", 9F)
            };
            this.Controls.Add(lblInstructions);
            y += 130;

            // User code label
            lblUserCode = new Label
            {
                Text = "Your verification code:",
                AutoSize = true,
                Location = new Point(leftMargin, y),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            this.Controls.Add(lblUserCode);
            y += 25;

            // User code text box
            txtUserCode = new TextBox
            {
                Size = new Size(180, 35),
                Location = new Point(leftMargin, y),
                Font = new Font("Consolas", 18F, FontStyle.Bold),
                TextAlign = HorizontalAlignment.Center,
                ReadOnly = true,
                BackColor = Color.FromArgb(255, 255, 200),
                Text = "Loading..."
            };
            this.Controls.Add(txtUserCode);

            // Copy code button
            btnCopyCode = new Button
            {
                Text = "📋 Copy",
                Size = new Size(80, 35),
                Location = new Point(leftMargin + 190, y),
                Font = new Font("Segoe UI", 9F),
                FlatStyle = FlatStyle.System,
                Enabled = false
            };
            btnCopyCode.Click += BtnCopyCode_Click;
            this.Controls.Add(btnCopyCode);

            // Open browser button
            btnOpenBrowser = new Button
            {
                Text = "🌐 Open GitHub",
                Size = new Size(130, 35),
                Location = new Point(leftMargin + 280, y),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnOpenBrowser.FlatAppearance.BorderSize = 0;
            btnOpenBrowser.Click += BtnOpenBrowser_Click;
            this.Controls.Add(btnOpenBrowser);
            y += 50;

            // Status label
            lblStatus = new Label
            {
                Text = "Requesting authorization code...",
                AutoSize = false,
                Size = new Size(controlWidth, 20),
                Location = new Point(leftMargin, y),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.Gray
            };
            this.Controls.Add(lblStatus);
            y += 25;

            // Progress bar
            progressBar = new ProgressBar
            {
                Size = new Size(controlWidth, 20),
                Location = new Point(leftMargin, y),
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 30
            };
            this.Controls.Add(progressBar);
            y += 35;

            // Cancel button
            btnCancel = new Button
            {
                Text = "Cancel",
                Size = new Size(100, 35),
                Location = new Point(leftMargin + 320, y),
                Font = new Font("Segoe UI", 9F),
                DialogResult = DialogResult.Cancel,
                FlatStyle = FlatStyle.System
            };
            this.Controls.Add(btnCancel);
        }

        private async Task StartAuthenticationFlow()
        {
            try
            {
                // Start authentication
                _authTask = _oauthManager.AuthenticateAsync();
                
                // Wait a moment for device code to be ready
                await Task.Delay(1500);
                
                // Get device code response to show user code
                var deviceCodeResponse = _oauthManager.GetDeviceCodeResponse();
                
                if (deviceCodeResponse != null)
                {
                    // Update UI with user code
                    this.Invoke(new Action(() =>
                    {
                        txtUserCode.Text = deviceCodeResponse.UserCode;
                        lblStatus.Text = "Waiting for authorization in browser...";
                        btnCopyCode.Enabled = true;
                        btnOpenBrowser.Enabled = true;
                    }));
                }

                // Wait for authentication to complete
                var result = await _authTask;
                
                this.Invoke(new Action(() =>
                {
                    if (result.IsSuccess)
                    {
                        lblStatus.Text = "✅ Authentication successful!";
                        lblStatus.ForeColor = Color.Green;
                        progressBar.Style = ProgressBarStyle.Continuous;
                        progressBar.Value = 100;
                        
                        MessageBox.Show("Successfully authenticated with GitHub!\nYou can now submit crash reports.",
                            "Authentication Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        lblStatus.Text = $"❌ Authentication failed: {result.ErrorMessage}";
                        lblStatus.ForeColor = Color.Red;
                        progressBar.Visible = false;
                        
                        MessageBox.Show($"Authentication failed:\n{result.ErrorMessage}",
                            "Authentication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        
                        this.DialogResult = DialogResult.Cancel;
                    }
                }));
            }
            catch (Exception ex)
            {
                this.Invoke(new Action(() =>
                {
                    lblStatus.Text = $"❌ Error: {ex.Message}";
                    lblStatus.ForeColor = Color.Red;
                    progressBar.Visible = false;
                    
                    MessageBox.Show($"Authentication error:\n{ex.Message}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    
                    this.DialogResult = DialogResult.Cancel;
                }));
            }
        }

        private void BtnCopyCode_Click(object? sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtUserCode.Text) && txtUserCode.Text != "Loading...")
            {
                Clipboard.SetText(txtUserCode.Text);
                lblStatus.Text = "✅ Code copied to clipboard!";
                lblStatus.ForeColor = Color.Green;
                
                // Reset status message after 2 seconds
                var timer = new System.Windows.Forms.Timer { Interval = 2000 };
                timer.Tick += (s, args) =>
                {
                    lblStatus.Text = "Waiting for authorization in browser...";
                    lblStatus.ForeColor = Color.Gray;
                    timer.Stop();
                };
                timer.Start();
            }
        }

        private void BtnOpenBrowser_Click(object? sender, EventArgs e)
        {
            try
            {
                var deviceCodeResponse = _oauthManager.GetDeviceCodeResponse();
                if (deviceCodeResponse != null)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = deviceCodeResponse.VerificationUri,
                        UseShellExecute = true
                    });
                    
                    lblStatus.Text = "Browser opened. Please authorize the application.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open browser:\n{ex.Message}\n\nPlease manually navigate to: https://github.com/login/device",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
        }
    }
}
