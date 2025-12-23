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
            SuspendLayout();
            
            // 
            // fffControlPanel
            // 
            fffControlPanel.Dock = DockStyle.Fill;
            fffControlPanel.Location = new Point(0, 0);
            fffControlPanel.Name = "fffControlPanel";
            fffControlPanel.Size = new Size(800, 600);
            fffControlPanel.TabIndex = 0;
            
            // 
            // FFFWindow
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 600);
            Controls.Add(fffControlPanel);
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

        private FFFControlPanel fffControlPanel;
    }
}
