namespace MillionaireGame.Forms
{
    partial class FFFWindow
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
            fffControlPanel = new FFFControlPanel();
            localPlayerPanel = new Panel();
            lblPlayerCount = new Label();
            cmbPlayerCount = new NumericUpDown();
            pnlPlayers = new Panel();
            pnlSoundCues = new Panel();
            btnFFFIntro = new Button();
            btnPlayerIntro = new Button();
            btnRandomSelect = new Button();
            lblSoundCues = new Label();
            
            localPlayerPanel.SuspendLayout();
            pnlSoundCues.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)cmbPlayerCount).BeginInit();
            SuspendLayout();
            
            // 
            // fffControlPanel
            // 
            fffControlPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            fffControlPanel.Location = new Point(10, 10);
            fffControlPanel.Name = "fffControlPanel";
            fffControlPanel.Size = new Size(1010, 740);
            fffControlPanel.TabIndex = 0;
            
            // 
            // localPlayerPanel
            // 
            localPlayerPanel.Dock = DockStyle.Fill;
            localPlayerPanel.Location = new Point(0, 0);
            localPlayerPanel.Name = "localPlayerPanel";
            localPlayerPanel.Size = new Size(1000, 700);
            localPlayerPanel.TabIndex = 1;
            localPlayerPanel.Visible = false;
            localPlayerPanel.Controls.Add(lblPlayerCount);
            localPlayerPanel.Controls.Add(cmbPlayerCount);
            localPlayerPanel.Controls.Add(pnlPlayers);
            localPlayerPanel.Controls.Add(pnlSoundCues);
            localPlayerPanel.BackColor = Color.FromArgb(240, 240, 240);
            
            // 
            // lblPlayerCount
            // 
            lblPlayerCount.AutoSize = true;
            lblPlayerCount.Font = new Font("Arial", 12F, FontStyle.Bold);
            lblPlayerCount.Location = new Point(20, 20);
            lblPlayerCount.Name = "lblPlayerCount";
            lblPlayerCount.Size = new Size(150, 20);
            lblPlayerCount.TabIndex = 0;
            lblPlayerCount.Text = "Number of Players:";
            
            // 
            // cmbPlayerCount
            // 
            cmbPlayerCount.Font = new Font("Arial", 12F);
            cmbPlayerCount.Location = new Point(200, 18);
            cmbPlayerCount.Minimum = 2;
            cmbPlayerCount.Maximum = 8;
            cmbPlayerCount.Value = 8;
            cmbPlayerCount.Name = "cmbPlayerCount";
            cmbPlayerCount.Size = new Size(60, 30);
            cmbPlayerCount.TabIndex = 1;
            cmbPlayerCount.ValueChanged += cmbPlayerCount_ValueChanged;
            
            // 
            // pnlPlayers
            // 
            pnlPlayers.Location = new Point(20, 70);
            pnlPlayers.Name = "pnlPlayers";
            pnlPlayers.Size = new Size(580, 600);
            pnlPlayers.TabIndex = 2;
            pnlPlayers.AutoScroll = false;
            pnlPlayers.BorderStyle = BorderStyle.FixedSingle;
            pnlPlayers.BackColor = Color.White;
            
            // 
            // pnlSoundCues
            // 
            pnlSoundCues.Location = new Point(620, 70);
            pnlSoundCues.Name = "pnlSoundCues";
            pnlSoundCues.Size = new Size(360, 600);
            pnlSoundCues.TabIndex = 3;
            pnlSoundCues.BorderStyle = BorderStyle.FixedSingle;
            pnlSoundCues.BackColor = Color.FromArgb(250, 250, 250);
            pnlSoundCues.Controls.Add(lblSoundCues);
            pnlSoundCues.Controls.Add(btnFFFIntro);
            pnlSoundCues.Controls.Add(btnPlayerIntro);
            pnlSoundCues.Controls.Add(btnRandomSelect);
            
            // 
            // lblSoundCues
            // 
            lblSoundCues.AutoSize = false;
            lblSoundCues.Font = new Font("Arial", 14F, FontStyle.Bold);
            lblSoundCues.Location = new Point(10, 20);
            lblSoundCues.Name = "lblSoundCues";
            lblSoundCues.Size = new Size(340, 30);
            lblSoundCues.TabIndex = 0;
            lblSoundCues.Text = "FFF Sound Cues";
            lblSoundCues.TextAlign = ContentAlignment.MiddleCenter;
            
            // 
            // btnFFFIntro
            // 
            btnFFFIntro.BackColor = Color.LimeGreen; // Green = Active/Ready
            btnFFFIntro.FlatStyle = FlatStyle.Flat;
            btnFFFIntro.Font = new Font("Arial", 11F, FontStyle.Bold);
            btnFFFIntro.ForeColor = Color.White;
            btnFFFIntro.Location = new Point(10, 70);
            btnFFFIntro.Name = "btnFFFIntro";
            btnFFFIntro.Size = new Size(340, 50);
            btnFFFIntro.TabIndex = 1;
            btnFFFIntro.Text = "1. FFF Intro (Lights Down)";
            btnFFFIntro.UseVisualStyleBackColor = false;
            btnFFFIntro.Click += btnFFFIntro_Click;
            
            // 
            // btnPlayerIntro
            // 
            btnPlayerIntro.BackColor = Color.Gray; // Grey = Disabled
            btnPlayerIntro.Enabled = false;
            btnPlayerIntro.FlatStyle = FlatStyle.Flat;
            btnPlayerIntro.Font = new Font("Arial", 11F, FontStyle.Bold);
            btnPlayerIntro.ForeColor = Color.White;
            btnPlayerIntro.Location = new Point(10, 135);
            btnPlayerIntro.Name = "btnPlayerIntro";
            btnPlayerIntro.Size = new Size(340, 50);
            btnPlayerIntro.TabIndex = 2;
            btnPlayerIntro.Text = "2. Introduce Players (Automated)";
            btnPlayerIntro.UseVisualStyleBackColor = false;
            btnPlayerIntro.Click += btnPlayerIntro_Click;
            
            // 
            // btnRandomSelect
            // 
            btnRandomSelect.BackColor = Color.Gray; // Grey = Disabled
            btnRandomSelect.Enabled = false;
            btnRandomSelect.FlatStyle = FlatStyle.Flat;
            btnRandomSelect.Font = new Font("Arial", 11F, FontStyle.Bold);
            btnRandomSelect.ForeColor = Color.White;
            btnRandomSelect.Location = new Point(10, 200);
            btnRandomSelect.Name = "btnRandomSelect";
            btnRandomSelect.Size = new Size(340, 50);
            btnRandomSelect.TabIndex = 3;
            btnRandomSelect.Text = "3. Randomly Select Player";
            btnRandomSelect.UseVisualStyleBackColor = false;
            btnRandomSelect.Click += btnRandomSelect_Click;
            
            // 
            // FFFWindow
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(1030, 760);
            Controls.Add(fffControlPanel);
            Controls.Add(localPlayerPanel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "FFFWindow";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Fastest Finger First Control";
            FormClosing += FFFWindow_FormClosing;
            Load += FFFWindow_Load;
            localPlayerPanel.ResumeLayout(false);
            localPlayerPanel.PerformLayout();
            pnlSoundCues.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)cmbPlayerCount).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private FFFControlPanel fffControlPanel;
        private Panel localPlayerPanel;
        private Label lblPlayerCount;
        private NumericUpDown cmbPlayerCount;
        private Panel pnlPlayers;
        private Panel pnlSoundCues;
        private Label lblSoundCues;
        private Button btnFFFIntro;
        private Button btnPlayerIntro;
        private Button btnRandomSelect;
    }
}
