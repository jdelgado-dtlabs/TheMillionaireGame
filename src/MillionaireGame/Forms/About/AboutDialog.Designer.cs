namespace MillionaireGame.Forms.About
{
    partial class AboutDialog
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
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
            lblAuthor = new Label();
            lblVersionLabel = new Label();
            lblVersion = new Label();
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
            pnlBackground.Controls.Add(lblAuthor);
            pnlBackground.Controls.Add(lblVersionLabel);
            pnlBackground.Controls.Add(lblVersion);
            pnlBackground.Controls.Add(btnClose);
            pnlBackground.Dock = DockStyle.Fill;
            pnlBackground.Location = new Point(0, 0);
            pnlBackground.Name = "pnlBackground";
            pnlBackground.Size = new Size(500, 280);
            pnlBackground.TabIndex = 0;
            // 
            // picLogo
            // 
            picLogo.BackColor = Color.Transparent;
            picLogo.Location = new Point(20, 20);
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
            // lblAuthor
            // 
            lblAuthor.AutoSize = true;
            lblAuthor.BackColor = Color.Transparent;
            lblAuthor.Font = new Font("Segoe UI", 9.75F);
            lblAuthor.ForeColor = Color.LightGray;
            lblAuthor.Location = new Point(160, 140);
            lblAuthor.Name = "lblAuthor";
            lblAuthor.Size = new Size(172, 17);
            lblAuthor.TabIndex = 3;
            lblAuthor.Text = "Made by Marco (Macronair)";
            // 
            // lblVersionLabel
            // 
            lblVersionLabel.AutoSize = true;
            lblVersionLabel.BackColor = Color.Transparent;
            lblVersionLabel.Font = new Font("Segoe UI", 9.75F);
            lblVersionLabel.ForeColor = Color.LightGray;
            lblVersionLabel.Location = new Point(160, 165);
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
            lblVersion.Location = new Point(220, 165);
            lblVersion.Name = "lblVersion";
            lblVersion.Size = new Size(50, 17);
            lblVersion.TabIndex = 5;
            lblVersion.Text = "1.0.0.0";
            // 
            // btnClose
            // 
            btnClose.BackColor = Color.FromArgb(60, 60, 80);
            btnClose.FlatAppearance.BorderColor = Color.Gold;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            btnClose.ForeColor = Color.White;
            btnClose.Location = new Point(200, 220);
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
            ClientSize = new Size(500, 280);
            Controls.Add(pnlBackground);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AboutDialog";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "About The Millionaire Game";
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
        private Label lblAuthor;
        private Label lblVersionLabel;
        private Label lblVersion;
        private Button btnClose;
    }
}
