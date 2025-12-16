namespace MillionaireGame.Forms.Options
{
    partial class DatabaseSettingsDialog
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
            pnlHeader = new Panel();
            lblTitle = new Label();
            lblSubtitle = new Label();
            radLocal = new RadioButton();
            radRemote = new RadioButton();
            grpRemote = new GroupBox();
            txtPassword = new TextBox();
            txtUsername = new TextBox();
            txtDatabase = new TextBox();
            numPort = new NumericUpDown();
            txtServerInstance = new TextBox();
            lblPassword = new Label();
            lblUsername = new Label();
            lblDatabase = new Label();
            lblPort = new Label();
            lblServerInstance = new Label();
            chkUseLocalDB = new CheckBox();
            chkHideAtStart = new CheckBox();
            btnOK = new Button();
            btnCancel = new Button();
            btnTest = new Button();
            pnlHeader.SuspendLayout();
            grpRemote.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numPort).BeginInit();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.FromArgb(30, 30, 30);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(484, 70);
            pnlHeader.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(12, 15);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(460, 40);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Database Settings";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblSubtitle
            // 
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(20, 85);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(294, 15);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Connect to a SQL server to use this application.";
            // 
            // radLocal
            // 
            radLocal.AutoSize = true;
            radLocal.Checked = true;
            radLocal.Location = new Point(20, 115);
            radLocal.Name = "radLocal";
            radLocal.Size = new Size(156, 19);
            radLocal.TabIndex = 2;
            radLocal.TabStop = true;
            radLocal.Text = "Use Local SQL Server";
            radLocal.UseVisualStyleBackColor = true;
            // 
            // radRemote
            // 
            radRemote.AutoSize = true;
            radRemote.Location = new Point(20, 145);
            radRemote.Name = "radRemote";
            radRemote.Size = new Size(169, 19);
            radRemote.TabIndex = 3;
            radRemote.Text = "Use Remote SQL Server";
            radRemote.UseVisualStyleBackColor = true;
            radRemote.CheckedChanged += radRemote_CheckedChanged;
            // 
            // grpRemote
            // 
            grpRemote.Controls.Add(txtPassword);
            grpRemote.Controls.Add(txtUsername);
            grpRemote.Controls.Add(txtDatabase);
            grpRemote.Controls.Add(numPort);
            grpRemote.Controls.Add(txtServerInstance);
            grpRemote.Controls.Add(lblPassword);
            grpRemote.Controls.Add(lblUsername);
            grpRemote.Controls.Add(lblDatabase);
            grpRemote.Controls.Add(lblPort);
            grpRemote.Controls.Add(lblServerInstance);
            grpRemote.Enabled = false;
            grpRemote.Location = new Point(40, 170);
            grpRemote.Name = "grpRemote";
            grpRemote.Size = new Size(420, 185);
            grpRemote.TabIndex = 4;
            grpRemote.TabStop = false;
            grpRemote.Text = "Remote Server Configuration";
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(120, 145);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '●';
            txtPassword.Size = new Size(280, 23);
            txtPassword.TabIndex = 9;
            // 
            // txtUsername
            // 
            txtUsername.Location = new Point(120, 115);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(280, 23);
            txtUsername.TabIndex = 8;
            // 
            // txtDatabase
            // 
            txtDatabase.Location = new Point(120, 85);
            txtDatabase.Name = "txtDatabase";
            txtDatabase.Size = new Size(280, 23);
            txtDatabase.TabIndex = 7;
            // 
            // numPort
            // 
            numPort.Location = new Point(120, 55);
            numPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numPort.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numPort.Name = "numPort";
            numPort.Size = new Size(100, 23);
            numPort.TabIndex = 6;
            numPort.Value = new decimal(new int[] { 1433, 0, 0, 0 });
            // 
            // txtServerInstance
            // 
            txtServerInstance.Location = new Point(120, 25);
            txtServerInstance.Name = "txtServerInstance";
            txtServerInstance.Size = new Size(280, 23);
            txtServerInstance.TabIndex = 5;
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Location = new Point(20, 148);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(60, 15);
            lblPassword.TabIndex = 4;
            lblPassword.Text = "Password:";
            // 
            // lblUsername
            // 
            lblUsername.AutoSize = true;
            lblUsername.Location = new Point(20, 118);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(63, 15);
            lblUsername.TabIndex = 3;
            lblUsername.Text = "Username:";
            // 
            // lblDatabase
            // 
            lblDatabase.AutoSize = true;
            lblDatabase.Location = new Point(20, 88);
            lblDatabase.Name = "lblDatabase";
            lblDatabase.Size = new Size(58, 15);
            lblDatabase.TabIndex = 2;
            lblDatabase.Text = "Database:";
            // 
            // lblPort
            // 
            lblPort.AutoSize = true;
            lblPort.Location = new Point(20, 58);
            lblPort.Name = "lblPort";
            lblPort.Size = new Size(32, 15);
            lblPort.TabIndex = 1;
            lblPort.Text = "Port:";
            // 
            // lblServerInstance
            // 
            lblServerInstance.AutoSize = true;
            lblServerInstance.Location = new Point(20, 28);
            lblServerInstance.Name = "lblServerInstance";
            lblServerInstance.Size = new Size(94, 15);
            lblServerInstance.TabIndex = 0;
            lblServerInstance.Text = "Server Instance:";
            // 
            // chkUseLocalDB
            // 
            chkUseLocalDB.AutoSize = true;
            chkUseLocalDB.Location = new Point(20, 370);
            chkUseLocalDB.Name = "chkUseLocalDB";
            chkUseLocalDB.Size = new Size(223, 19);
            chkUseLocalDB.TabIndex = 5;
            chkUseLocalDB.Text = "Use LocalDB (SQL Server Express)";
            chkUseLocalDB.UseVisualStyleBackColor = true;
            // 
            // chkHideAtStart
            // 
            chkHideAtStart.AutoSize = true;
            chkHideAtStart.Location = new Point(20, 395);
            chkHideAtStart.Name = "chkHideAtStart";
            chkHideAtStart.Size = new Size(254, 19);
            chkHideAtStart.TabIndex = 6;
            chkHideAtStart.Text = "Hide this dialog at startup (if connected)";
            chkHideAtStart.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            btnOK.Location = new Point(238, 430);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(75, 30);
            btnOK.TabIndex = 7;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(319, 430);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 30);
            btnCancel.TabIndex = 8;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnTest
            // 
            btnTest.Location = new Point(400, 430);
            btnTest.Name = "btnTest";
            btnTest.Size = new Size(60, 30);
            btnTest.TabIndex = 9;
            btnTest.Text = "Test";
            btnTest.UseVisualStyleBackColor = true;
            btnTest.Click += btnTest_Click;
            // 
            // DatabaseSettingsDialog
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(484, 481);
            Controls.Add(btnTest);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(chkHideAtStart);
            Controls.Add(chkUseLocalDB);
            Controls.Add(grpRemote);
            Controls.Add(radRemote);
            Controls.Add(radLocal);
            Controls.Add(lblSubtitle);
            Controls.Add(pnlHeader);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "DatabaseSettingsDialog";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Database Settings";
            pnlHeader.ResumeLayout(false);
            grpRemote.ResumeLayout(false);
            grpRemote.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numPort).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel pnlHeader;
        private Label lblTitle;
        private Label lblSubtitle;
        private RadioButton radLocal;
        private RadioButton radRemote;
        private GroupBox grpRemote;
        private TextBox txtServerInstance;
        private Label lblServerInstance;
        private Label lblPort;
        private Label lblDatabase;
        private Label lblUsername;
        private Label lblPassword;
        private NumericUpDown numPort;
        private TextBox txtDatabase;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private CheckBox chkUseLocalDB;
        private CheckBox chkHideAtStart;
        private Button btnOK;
        private Button btnCancel;
        private Button btnTest;
    }
}
