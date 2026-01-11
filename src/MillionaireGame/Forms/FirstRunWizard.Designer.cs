namespace MillionaireGame.Forms;

partial class FirstRunWizard
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
        this.pnlHeader = new Panel();
        this.lblTitle = new Label();
        this.lblSubtitle = new Label();
        this.grpDatabaseType = new GroupBox();
        this.radLocalDB = new RadioButton();
        this.lblLocalDBDescription = new Label();
        this.radSqlServer = new RadioButton();
        this.cmbServerInstance = new ComboBox();
        this.lblSqlServerStatus = new Label();
        this.lblSqlServerDescription = new Label();
        this.grpConnectionDetails = new GroupBox();
        this.lblServer = new Label();
        this.txtServer = new TextBox();
        this.lblPort = new Label();
        this.numPort = new NumericUpDown();
        this.lblAuthentication = new Label();
        this.radWindowsAuth = new RadioButton();
        this.radSqlAuth = new RadioButton();
        this.lblUsername = new Label();
        this.txtUsername = new TextBox();
        this.lblPassword = new Label();
        this.txtPassword = new TextBox();
        this.grpConnectionTest = new GroupBox();
        this.btnTest = new Button();
        this.lblConnectionStatus = new Label();
        this.grpOptions = new GroupBox();
        this.chkLoadSampleData = new CheckBox();
        this.pnlButtons = new Panel();
        this.btnFinish = new Button();
        this.btnCancel = new Button();
        this.pnlHeader.SuspendLayout();
        this.grpDatabaseType.SuspendLayout();
        this.grpConnectionDetails.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)this.numPort).BeginInit();
        this.grpConnectionTest.SuspendLayout();
        this.grpOptions.SuspendLayout();
        this.pnlButtons.SuspendLayout();
        this.SuspendLayout();
        // 
        // pnlHeader
        // 
        this.pnlHeader.BackColor = Color.FromArgb(30, 30, 30);
        this.pnlHeader.Controls.Add(this.lblTitle);
        this.pnlHeader.Controls.Add(this.lblSubtitle);
        this.pnlHeader.Dock = DockStyle.Top;
        this.pnlHeader.Location = new Point(0, 0);
        this.pnlHeader.Name = "pnlHeader";
        this.pnlHeader.Size = new Size(600, 80);
        this.pnlHeader.TabIndex = 0;
        // 
        // lblTitle
        // 
        this.lblTitle.AutoSize = true;
        this.lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
        this.lblTitle.ForeColor = Color.White;
        this.lblTitle.Location = new Point(12, 12);
        this.lblTitle.Name = "lblTitle";
        this.lblTitle.Size = new Size(400, 32);
        this.lblTitle.TabIndex = 0;
        this.lblTitle.Text = "Welcome to The Millionaire Game";
        // 
        // lblSubtitle
        // 
        this.lblSubtitle.AutoSize = true;
        this.lblSubtitle.Font = new Font("Segoe UI", 10F);
        this.lblSubtitle.ForeColor = Color.LightGray;
        this.lblSubtitle.Location = new Point(12, 48);
        this.lblSubtitle.Name = "lblSubtitle";
        this.lblSubtitle.Size = new Size(200, 19);
        this.lblSubtitle.TabIndex = 1;
        this.lblSubtitle.Text = "Let's set up your database";
        // 
        // grpDatabaseType
        // 
        this.grpDatabaseType.Controls.Add(this.radLocalDB);
        this.grpDatabaseType.Controls.Add(this.lblLocalDBDescription);
        this.grpDatabaseType.Controls.Add(this.radSqlServer);
        this.grpDatabaseType.Controls.Add(this.cmbServerInstance);
        this.grpDatabaseType.Controls.Add(this.lblSqlServerStatus);
        this.grpDatabaseType.Controls.Add(this.lblSqlServerDescription);
        this.grpDatabaseType.Location = new Point(12, 95);
        this.grpDatabaseType.Name = "grpDatabaseType";
        this.grpDatabaseType.Size = new Size(576, 220);
        this.grpDatabaseType.TabIndex = 1;
        this.grpDatabaseType.TabStop = false;
        this.grpDatabaseType.Text = "Database Type";
        // 
        // radLocalDB
        // 
        this.radLocalDB.AutoSize = true;
        this.radLocalDB.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        this.radLocalDB.Location = new Point(12, 25);
        this.radLocalDB.Name = "radLocalDB";
        this.radLocalDB.Size = new Size(240, 23);
        this.radLocalDB.TabIndex = 0;
        this.radLocalDB.TabStop = true;
        this.radLocalDB.Text = "⚪ LocalDB (Automatic) [Recommended]";
        this.radLocalDB.UseVisualStyleBackColor = true;
        this.radLocalDB.CheckedChanged += radLocalDB_CheckedChanged;
        // 
        // lblLocalDBDescription
        // 
        this.lblLocalDBDescription.Location = new Point(32, 52);
        this.lblLocalDBDescription.Name = "lblLocalDBDescription";
        this.lblLocalDBDescription.Size = new Size(530, 60);
        this.lblLocalDBDescription.TabIndex = 2;
        this.lblLocalDBDescription.Text = "Simplest option - zero configuration needed. Lightweight, on-demand database perfect for single-user game show hosting. Installed automatically with the game.";
        this.lblLocalDBDescription.ForeColor = Color.Gray;
        // 
        // radSqlServer
        // 
        this.radSqlServer.AutoSize = true;
        this.radSqlServer.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        this.radSqlServer.Location = new Point(12, 120);
        this.radSqlServer.Name = "radSqlServer";
        this.radSqlServer.Size = new Size(200, 23);
        this.radSqlServer.TabIndex = 3;
        this.radSqlServer.TabStop = true;
        this.radSqlServer.Text = "⚪ SQL Server (Advanced)";
        this.radSqlServer.UseVisualStyleBackColor = true;
        this.radSqlServer.CheckedChanged += radSqlServer_CheckedChanged;
        // 
        // cmbServerInstance
        // 
        this.cmbServerInstance.DropDownStyle = ComboBoxStyle.DropDownList;
        this.cmbServerInstance.FormattingEnabled = true;
        this.cmbServerInstance.Location = new Point(32, 148);
        this.cmbServerInstance.Name = "cmbServerInstance";
        this.cmbServerInstance.Size = new Size(530, 23);
        this.cmbServerInstance.TabIndex = 4;
        this.cmbServerInstance.SelectedIndexChanged += cmbServerInstance_SelectedIndexChanged;
        // 
        // lblSqlServerStatus
        // 
        this.lblSqlServerStatus.AutoSize = true;
        this.lblSqlServerStatus.Location = new Point(32, 177);
        this.lblSqlServerStatus.Name = "lblSqlServerStatus";
        this.lblSqlServerStatus.Size = new Size(220, 15);
        this.lblSqlServerStatus.TabIndex = 5;
        this.lblSqlServerStatus.Text = "Scanning for SQL Server instances...";
        this.lblSqlServerStatus.ForeColor = Color.Gray;
        this.lblSqlServerStatus.Visible = false;
        // 
        // lblSqlServerDescription
        // 
        this.lblSqlServerDescription.Location = new Point(32, 192);
        this.lblSqlServerDescription.Name = "lblSqlServerDescription";
        this.lblSqlServerDescription.Size = new Size(530, 20);
        this.lblSqlServerDescription.TabIndex = 6;
        this.lblSqlServerDescription.Text = "For users who want to use existing SQL Server installation.";
        this.lblSqlServerDescription.ForeColor = Color.Gray;
        // 
        // grpConnectionDetails
        // 
        this.grpConnectionDetails.Controls.Add(this.lblServer);
        this.grpConnectionDetails.Controls.Add(this.txtServer);
        this.grpConnectionDetails.Controls.Add(this.lblPort);
        this.grpConnectionDetails.Controls.Add(this.numPort);
        this.grpConnectionDetails.Controls.Add(this.lblAuthentication);
        this.grpConnectionDetails.Controls.Add(this.radWindowsAuth);
        this.grpConnectionDetails.Controls.Add(this.radSqlAuth);
        this.grpConnectionDetails.Controls.Add(this.lblUsername);
        this.grpConnectionDetails.Controls.Add(this.txtUsername);
        this.grpConnectionDetails.Controls.Add(this.lblPassword);
        this.grpConnectionDetails.Controls.Add(this.txtPassword);
        this.grpConnectionDetails.Location = new Point(12, 321);
        this.grpConnectionDetails.Name = "grpConnectionDetails";
        this.grpConnectionDetails.Size = new Size(576, 200);
        this.grpConnectionDetails.TabIndex = 2;
        this.grpConnectionDetails.TabStop = false;
        this.grpConnectionDetails.Text = "Server Connection Details";
        this.grpConnectionDetails.Visible = false;
        // 
        // lblServer
        // 
        this.lblServer.AutoSize = true;
        this.lblServer.Location = new Point(12, 25);
        this.lblServer.Name = "lblServer";
        this.lblServer.Size = new Size(100, 15);
        this.lblServer.TabIndex = 0;
        this.lblServer.Text = "Server/Instance:";
        // 
        // txtServer
        // 
        this.txtServer.Location = new Point(130, 22);
        this.txtServer.Name = "txtServer";
        this.txtServer.Size = new Size(300, 23);
        this.txtServer.TabIndex = 1;
        this.txtServer.Text = "localhost\\SQLEXPRESS";
        // 
        // lblPort
        // 
        this.lblPort.AutoSize = true;
        this.lblPort.Location = new Point(445, 25);
        this.lblPort.Name = "lblPort";
        this.lblPort.Size = new Size(32, 15);
        this.lblPort.TabIndex = 2;
        this.lblPort.Text = "Port:";
        // 
        // numPort
        // 
        this.numPort.Location = new Point(485, 22);
        this.numPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
        this.numPort.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        this.numPort.Name = "numPort";
        this.numPort.Size = new Size(75, 23);
        this.numPort.TabIndex = 3;
        this.numPort.Value = new decimal(new int[] { 1433, 0, 0, 0 });
        // 
        // lblAuthentication
        // 
        this.lblAuthentication.AutoSize = true;
        this.lblAuthentication.Location = new Point(12, 58);
        this.lblAuthentication.Name = "lblAuthentication";
        this.lblAuthentication.Size = new Size(95, 15);
        this.lblAuthentication.TabIndex = 4;
        this.lblAuthentication.Text = "Authentication:";
        // 
        // radWindowsAuth
        // 
        this.radWindowsAuth.AutoSize = true;
        this.radWindowsAuth.Checked = true;
        this.radWindowsAuth.Location = new Point(130, 56);
        this.radWindowsAuth.Name = "radWindowsAuth";
        this.radWindowsAuth.Size = new Size(165, 19);
        this.radWindowsAuth.TabIndex = 5;
        this.radWindowsAuth.TabStop = true;
        this.radWindowsAuth.Text = "Windows Authentication";
        this.radWindowsAuth.UseVisualStyleBackColor = true;
        this.radWindowsAuth.CheckedChanged += radWindowsAuth_CheckedChanged;
        // 
        // radSqlAuth
        // 
        this.radSqlAuth.AutoSize = true;
        this.radSqlAuth.Location = new Point(310, 56);
        this.radSqlAuth.Name = "radSqlAuth";
        this.radSqlAuth.Size = new Size(180, 19);
        this.radSqlAuth.TabIndex = 6;
        this.radSqlAuth.Text = "SQL Server Authentication";
        this.radSqlAuth.UseVisualStyleBackColor = true;
        this.radSqlAuth.CheckedChanged += radSqlAuth_CheckedChanged;
        // 
        // lblUsername
        // 
        this.lblUsername.AutoSize = true;
        this.lblUsername.Location = new Point(12, 90);
        this.lblUsername.Name = "lblUsername";
        this.lblUsername.Size = new Size(63, 15);
        this.lblUsername.TabIndex = 7;
        this.lblUsername.Text = "Username:";
        // 
        // txtUsername
        // 
        this.txtUsername.Enabled = false;
        this.txtUsername.Location = new Point(130, 87);
        this.txtUsername.Name = "txtUsername";
        this.txtUsername.Size = new Size(430, 23);
        this.txtUsername.TabIndex = 8;
        // 
        // lblPassword
        // 
        this.lblPassword.AutoSize = true;
        this.lblPassword.Location = new Point(12, 122);
        this.lblPassword.Name = "lblPassword";
        this.lblPassword.Size = new Size(60, 15);
        this.lblPassword.TabIndex = 9;
        this.lblPassword.Text = "Password:";
        // 
        // txtPassword
        // 
        this.txtPassword.Enabled = false;
        this.txtPassword.Location = new Point(130, 119);
        this.txtPassword.Name = "txtPassword";
        this.txtPassword.PasswordChar = '●';
        this.txtPassword.Size = new Size(430, 23);
        this.txtPassword.TabIndex = 10;
        // 
        // grpConnectionTest
        // 
        this.grpConnectionTest.Controls.Add(this.btnTest);
        this.grpConnectionTest.Controls.Add(this.lblConnectionStatus);
        this.grpConnectionTest.Location = new Point(12, 527);
        this.grpConnectionTest.Name = "grpConnectionTest";
        this.grpConnectionTest.Size = new Size(576, 80);
        this.grpConnectionTest.TabIndex = 3;
        this.grpConnectionTest.TabStop = false;
        this.grpConnectionTest.Text = "Connection Testing";
        // 
        // btnTest
        // 
        this.btnTest.Location = new Point(12, 22);
        this.btnTest.Name = "btnTest";
        this.btnTest.Size = new Size(130, 30);
        this.btnTest.TabIndex = 0;
        this.btnTest.Text = "Test Connection";
        this.btnTest.UseVisualStyleBackColor = true;
        this.btnTest.Click += btnTest_Click;
        // 
        // lblConnectionStatus
        // 
        this.lblConnectionStatus.Location = new Point(150, 22);
        this.lblConnectionStatus.Name = "lblConnectionStatus";
        this.lblConnectionStatus.Size = new Size(410, 50);
        this.lblConnectionStatus.TabIndex = 1;
        this.lblConnectionStatus.Text = "Ready to test connection";
        this.lblConnectionStatus.ForeColor = Color.Gray;
        // 
        // grpOptions
        // 
        this.grpOptions.Controls.Add(this.chkLoadSampleData);
        this.grpOptions.Location = new Point(12, 613);
        this.grpOptions.Name = "grpOptions";
        this.grpOptions.Size = new Size(576, 60);
        this.grpOptions.TabIndex = 4;
        this.grpOptions.TabStop = false;
        this.grpOptions.Text = "Database Creation Options";
        // 
        // chkLoadSampleData
        // 
        this.chkLoadSampleData.AutoSize = true;
        this.chkLoadSampleData.Enabled = false;
        this.chkLoadSampleData.Location = new Point(12, 25);
        this.chkLoadSampleData.Name = "chkLoadSampleData";
        this.chkLoadSampleData.Size = new Size(450, 19);
        this.chkLoadSampleData.TabIndex = 0;
        this.chkLoadSampleData.Text = "☑ Load sample trivia data (80 questions + 44 FFF questions from init_database.sql)";
        this.chkLoadSampleData.UseVisualStyleBackColor = true;
        // 
        // pnlButtons
        // 
        this.pnlButtons.Controls.Add(this.btnFinish);
        this.pnlButtons.Controls.Add(this.btnCancel);
        this.pnlButtons.Dock = DockStyle.Bottom;
        this.pnlButtons.Location = new Point(0, 685);
        this.pnlButtons.Name = "pnlButtons";
        this.pnlButtons.Size = new Size(600, 50);
        this.pnlButtons.TabIndex = 5;
        // 
        // btnFinish
        // 
        this.btnFinish.Enabled = false;
        this.btnFinish.Location = new Point(390, 10);
        this.btnFinish.Name = "btnFinish";
        this.btnFinish.Size = new Size(95, 30);
        this.btnFinish.TabIndex = 0;
        this.btnFinish.Text = "Finish";
        this.btnFinish.UseVisualStyleBackColor = true;
        this.btnFinish.Click += btnFinish_Click;
        // 
        // btnCancel
        // 
        this.btnCancel.Location = new Point(493, 10);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new Size(95, 30);
        this.btnCancel.TabIndex = 1;
        this.btnCancel.Text = "Cancel";
        this.btnCancel.UseVisualStyleBackColor = true;
        this.btnCancel.Click += btnCancel_Click;
        // 
        // FirstRunWizard
        // 
        this.AutoScaleDimensions = new SizeF(7F, 15F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new Size(600, 735);
        this.Controls.Add(this.pnlButtons);
        this.Controls.Add(this.grpOptions);
        this.Controls.Add(this.grpConnectionTest);
        this.Controls.Add(this.grpConnectionDetails);
        this.Controls.Add(this.grpDatabaseType);
        this.Controls.Add(this.pnlHeader);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "FirstRunWizard";
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Text = "Database Setup - The Millionaire Game";
        this.pnlHeader.ResumeLayout(false);
        this.pnlHeader.PerformLayout();
        this.grpDatabaseType.ResumeLayout(false);
        this.grpDatabaseType.PerformLayout();
        this.grpConnectionDetails.ResumeLayout(false);
        this.grpConnectionDetails.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)this.numPort).EndInit();
        this.grpConnectionTest.ResumeLayout(false);
        this.grpOptions.ResumeLayout(false);
        this.grpOptions.PerformLayout();
        this.pnlButtons.ResumeLayout(false);
        this.ResumeLayout(false);
    }

    #endregion

    private Panel pnlHeader;
    private Label lblTitle;
    private Label lblSubtitle;
    private GroupBox grpDatabaseType;
    private RadioButton radLocalDB;
    private Label lblLocalDBDescription;
    private RadioButton radSqlServer;
    private ComboBox cmbServerInstance;
    private Label lblSqlServerStatus;
    private Label lblSqlServerDescription;
    private GroupBox grpConnectionDetails;
    private Label lblServer;
    private TextBox txtServer;
    private Label lblPort;
    private NumericUpDown numPort;
    private Label lblAuthentication;
    private RadioButton radWindowsAuth;
    private RadioButton radSqlAuth;
    private Label lblUsername;
    private TextBox txtUsername;
    private Label lblPassword;
    private TextBox txtPassword;
    private GroupBox grpConnectionTest;
    private Button btnTest;
    private Label lblConnectionStatus;
    private GroupBox grpOptions;
    private CheckBox chkLoadSampleData;
    private Panel pnlButtons;
    private Button btnFinish;
    private Button btnCancel;
}
