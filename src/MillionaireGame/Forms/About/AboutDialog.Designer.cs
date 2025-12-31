namespace MillionaireGame.Forms.About
{
    partial class AboutDialog
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _animationTimer?.Stop();
                _animationTimer?.Dispose();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            pnlBackground = new Panel();
            picLogo = new PictureBox();
            lblTitle = new Label();
            lblSubtitle = new Label();
            lnkAuthor = new LinkLabel();
            lnkOriginalAuthor = new LinkLabel();
            lblVersionLabel = new Label();
            lblVersion = new Label();
            lblBuildInfo = new Label();
            lblLicense = new Label();
            btnClose = new Button();
            pnlBackground.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picLogo).BeginInit();
            SuspendLayout();
            // 
            // pnlBackground
            // 
            pnlBackground.BackColor = Color.FromArgb(20, 20, 40);
            pnlBackground.Controls.Add(picLogo);
            pnlBackground.Controls.Add(lblTitle);
            pnlBackground.Controls.Add(lblSubtitle);
            pnlBackground.Controls.Add(lnkAuthor);
            pnlBackground.Controls.Add(lnkOriginalAuthor);
            pnlBackground.Controls.Add(lblVersionLabel);
            pnlBackground.Controls.Add(lblVersion);
            pnlBackground.Controls.Add(lblBuildInfo);
            pnlBackground.Controls.Add(lblLicense);
            pnlBackground.Controls.Add(btnClose);
            pnlBackground.Dock = DockStyle.Fill;
            pnlBackground.Location = new Point(0, 0);
            pnlBackground.Name = "pnlBackground";
            pnlBackground.Size = new Size(500, 320);
            pnlBackground.TabIndex = 0;
            // 
            // picLogo
            // 
            picLogo.BackColor = Color.Transparent;
            picLogo.Location = new Point(20, 60);
            picLogo.Name = "picLogo";
            picLogo.Size = new Size(120, 120);
            picLogo.SizeMode = PictureBoxSizeMode.Zoom;
            picLogo.TabIndex = 0;
            picLogo.TabStop = false;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.BackColor = Color.Transparent;
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.ForeColor = Color.Gold;
            lblTitle.Location = new Point(160, 30);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(236, 30);
            lblTitle.TabIndex = 1;
            lblTitle.Text = "The Millionaire Game";
            // 
            // lblSubtitle
            // 
            lblSubtitle.AutoSize = true;
            lblSubtitle.BackColor = Color.Transparent;
            lblSubtitle.Font = new Font("Segoe UI", 10F);
            lblSubtitle.ForeColor = Color.White;
            lblSubtitle.Location = new Point(160, 70);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(264, 38);
            lblSubtitle.TabIndex = 2;
            lblSubtitle.Text = "Based on the TV Show\r\n\"Who Wants To Be A Millionaire\"";
            // 
            // lnkAuthor
            // 
            lnkAuthor.AutoSize = true;
            lnkAuthor.BackColor = Color.Transparent;
            lnkAuthor.Font = new Font("Segoe UI", 9.75F);
            lnkAuthor.LinkColor = Color.SkyBlue;
            lnkAuthor.Location = new Point(160, 125);
            lnkAuthor.Name = "lnkAuthor";
            lnkAuthor.Size = new Size(204, 17);
            lnkAuthor.TabIndex = 3;
            lnkAuthor.TabStop = true;
            lnkAuthor.Text = "C# Version: Jean Francois Delgado";
            lnkAuthor.LinkClicked += lnkAuthor_LinkClicked;
            // 
            // lnkOriginalAuthor
            // 
            lnkOriginalAuthor.AutoSize = true;
            lnkOriginalAuthor.BackColor = Color.Transparent;
            lnkOriginalAuthor.Font = new Font("Segoe UI", 8.25F);
            lnkOriginalAuthor.LinkColor = Color.SkyBlue;
            lnkOriginalAuthor.Location = new Point(160, 145);
            lnkOriginalAuthor.Name = "lnkOriginalAuthor";
            lnkOriginalAuthor.Size = new Size(250, 13);
            lnkOriginalAuthor.TabIndex = 7;
            lnkOriginalAuthor.TabStop = true;
            lnkOriginalAuthor.Text = "Original VB.NET Version: Marco Loenen (Macronair)";
            lnkOriginalAuthor.LinkClicked += lnkOriginalAuthor_LinkClicked;
            // 
            // lblVersionLabel
            // 
            lblVersionLabel.AutoSize = true;
            lblVersionLabel.BackColor = Color.Transparent;
            lblVersionLabel.Font = new Font("Segoe UI", 9.75F);
            lblVersionLabel.ForeColor = Color.LightGray;
            lblVersionLabel.Location = new Point(160, 170);
            lblVersionLabel.Name = "lblVersionLabel";
            lblVersionLabel.Size = new Size(56, 17);
            lblVersionLabel.TabIndex = 4;
            lblVersionLabel.Text = "Version:";
            // 
            // lblVersion
            // 
            lblVersion.AutoSize = true;
            lblVersion.BackColor = Color.Transparent;
            lblVersion.Font = new Font("Segoe UI", 9.75F);
            lblVersion.ForeColor = Color.White;
            lblVersion.Location = new Point(220, 170);
            lblVersion.Name = "lblVersion";
            lblVersion.Size = new Size(34, 17);
            lblVersion.TabIndex = 5;
            lblVersion.Text = "0.9.8";
            // 
            // lblBuildInfo
            // 
            lblBuildInfo.AutoSize = true;
            lblBuildInfo.BackColor = Color.Transparent;
            lblBuildInfo.Font = new Font("Segoe UI", 8.25F);
            lblBuildInfo.ForeColor = Color.DarkGray;
            lblBuildInfo.Location = new Point(160, 190);
            lblBuildInfo.Name = "lblBuildInfo";
            lblBuildInfo.Size = new Size(162, 13);
            lblBuildInfo.TabIndex = 10;
            lblBuildInfo.Text = "Built with C# and .NET 8.0";
            // 
            // lblLicense
            // 
            lblLicense.AutoSize = true;
            lblLicense.BackColor = Color.Transparent;
            lblLicense.Font = new Font("Segoe UI", 8.25F);
            lblLicense.ForeColor = Color.DarkGray;
            lblLicense.Location = new Point(160, 210);
            lblLicense.Name = "lblLicense";
            lblLicense.Size = new Size(280, 26);
            lblLicense.TabIndex = 11;
            lblLicense.Text = "Licensed under MIT License\r\nCopyright © 2025-2026 Jean Francois Delgado";
            // 
            // btnClose
            // 
            btnClose.BackColor = Color.FromArgb(60, 60, 80);
            btnClose.FlatAppearance.BorderColor = Color.Gold;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            btnClose.ForeColor = Color.White;
            btnClose.Location = new Point(200, 260);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(100, 35);
            btnClose.TabIndex = 6;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = false;
            btnClose.Click += btnClose_Click;
            // 
            // AboutDialog
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(500, 320);
            Controls.Add(pnlBackground);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AboutDialog";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Millionaire Game - About";
            pnlBackground.ResumeLayout(false);
            pnlBackground.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picLogo).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlBackground;
        private PictureBox picLogo;
        private Label lblTitle;
        private Label lblSubtitle;
        private LinkLabel lnkAuthor;
        private LinkLabel lnkOriginalAuthor;
        private Label lblVersionLabel;
        private Label lblVersion;
        private Label lblBuildInfo;
        private Label lblLicense;
        private Button btnClose;
    }
}
