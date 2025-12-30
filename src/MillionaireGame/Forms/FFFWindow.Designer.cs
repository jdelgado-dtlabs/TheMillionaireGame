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
            fffOnlinePanel = new FFFOnlinePanel();
            fffOfflinePanel = new FFFOfflinePanel();
            
            SuspendLayout();
            
            // 
            // fffOnlinePanel
            // 
            fffOnlinePanel.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            fffOnlinePanel.Location = new Point(10, 10);
            fffOnlinePanel.Name = "fffOnlinePanel";
            fffOnlinePanel.Size = new Size(1010, 740);
            fffOnlinePanel.TabIndex = 0;
            
            // 
            // fffOfflinePanel
            // 
            fffOfflinePanel.Dock = DockStyle.Fill;
            fffOfflinePanel.Location = new Point(0, 0);
            fffOfflinePanel.Name = "fffOfflinePanel";
            fffOfflinePanel.Size = new Size(1000, 700);
            fffOfflinePanel.TabIndex = 1;
            fffOfflinePanel.Visible = false;
            fffOfflinePanel.PlayerSelected += (s, e) => Hide();
            
            // 
            // FFFWindow
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(1030, 760);
            Controls.Add(fffOnlinePanel);
            Controls.Add(fffOfflinePanel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "FFFWindow";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Fastest Finger First Control";
            FormClosing += FFFWindow_FormClosing;
            Load += FFFWindow_Load;
            ResumeLayout(false);
        }

        #endregion

        private FFFOnlinePanel fffOnlinePanel;
        private FFFOfflinePanel fffOfflinePanel;
    }
}
