namespace MillionaireGame.Forms.Options
{
    partial class OptionsDialog
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
            tabControl = new TabControl();
            tabGeneral = new TabPage();
            grpPreviews = new GroupBox();
            chkEnablePreviewAutomatically = new CheckBox();
            cmbPreviewOrientation = new ComboBox();
            lblPreviewOrientation = new Label();
            grpMultipleMonitorControl = new GroupBox();
            tabAudience = new TabPage();
            grpAudienceServer = new GroupBox();
            lblServerIP = new Label();
            cmbServerIP = new ComboBox();
            lblServerPort = new Label();
            txtServerPort = new TextBox();
            btnCheckPort = new Button();
            lblPortStatus = new Label();
            chkAutoStart = new CheckBox();
            btnStartServer = new Button();
            btnStopServer = new Button();
            lblServerStatus = new Label();
            lblAudienceInfo = new Label();
            lblMonitorCount = new Label();
            lblDebugMode = new Label();
            chkFullScreenGuestScreen = new CheckBox();
            chkFullScreenHostScreen = new CheckBox();
            chkFullScreenTVScreen = new CheckBox();
            cmbMonitorHost = new ComboBox();
            cmbMonitorGuest = new ComboBox();
            cmbMonitorTV = new ComboBox();
            btnIdentifyMonitors = new Button();
            grpConsole = new GroupBox();
            chkShowConsole = new CheckBox();
            chkShowWebServiceConsole = new CheckBox();
            tabBroadcast = new TabPage();
            grpBroadcast = new GroupBox();
            tabLifelines = new TabPage();
            tabMoneyTree = new TabPage();
            grpPrizeValues = new GroupBox();
            numLevel15 = new NumericUpDown();
            numLevel14 = new NumericUpDown();
            numLevel13 = new NumericUpDown();
            numLevel12 = new NumericUpDown();
            numLevel11 = new NumericUpDown();
            numLevel10 = new NumericUpDown();
            numLevel09 = new NumericUpDown();
            numLevel08 = new NumericUpDown();
            numLevel07 = new NumericUpDown();
            numLevel06 = new NumericUpDown();
            numLevel05 = new NumericUpDown();
            numLevel04 = new NumericUpDown();
            numLevel03 = new NumericUpDown();
            numLevel02 = new NumericUpDown();
            numLevel01 = new NumericUpDown();
            grpCurrency = new GroupBox();
            txtCustomCurrency = new TextBox();
            radCurrencyOther = new RadioButton();
            radCurrencyYen = new RadioButton();
            radCurrencyPound = new RadioButton();
            radCurrencyEuro = new RadioButton();
            radCurrencyDollar = new RadioButton();
            chkCurrencyAtSuffix = new CheckBox();
            grpSafetyNets = new GroupBox();
            numSafetyNet2 = new NumericUpDown();
            numSafetyNet1 = new NumericUpDown();
            lblSafetyNet2 = new Label();
            lblSafetyNet1 = new Label();
            grpLifeline4 = new GroupBox();
            cmbLifeline4Availability = new ComboBox();
            cmbLifeline4Type = new ComboBox();
            lblLifeline4 = new Label();
            grpLifeline3 = new GroupBox();
            cmbLifeline3Availability = new ComboBox();
            cmbLifeline3Type = new ComboBox();
            lblLifeline3 = new Label();
            grpLifeline2 = new GroupBox();
            cmbLifeline2Availability = new ComboBox();
            cmbLifeline2Type = new ComboBox();
            lblLifeline2 = new Label();
            grpLifeline1 = new GroupBox();
            cmbLifeline1Availability = new ComboBox();
            cmbLifeline1Type = new ComboBox();
            lblLifeline1 = new Label();
            numTotalLifelines = new NumericUpDown();
            lblTotalLifelines = new Label();
            tabSounds = new TabPage();
            grpSoundPack = new GroupBox();
            lstSoundPackInfo = new ListBox();
            btnExportExample = new Button();
            btnRemovePack = new Button();
            btnImportPack = new Button();
            cmbSoundPack = new ComboBox();
            lblSoundPack = new Label();
            btnOK = new Button();
            btnCancel = new Button();
            tabControl.SuspendLayout();
            tabGeneral.SuspendLayout();
            grpPreviews.SuspendLayout();
            grpMultipleMonitorControl.SuspendLayout();
            grpConsole.SuspendLayout();
            tabBroadcast.SuspendLayout();
            grpBroadcast.SuspendLayout();
            tabLifelines.SuspendLayout();
            grpLifeline4.SuspendLayout();
            grpLifeline3.SuspendLayout();
            grpLifeline2.SuspendLayout();
            grpLifeline1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numTotalLifelines).BeginInit();
            tabSounds.SuspendLayout();
            grpSoundPack.SuspendLayout();
            tabAudience.SuspendLayout();
            grpAudienceServer.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl
            // 
            tabControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl.Controls.Add(tabGeneral);
            tabControl.Controls.Add(tabBroadcast);
            tabControl.Controls.Add(tabLifelines);
            tabControl.Controls.Add(tabMoneyTree);
            tabControl.Controls.Add(tabSounds);
            tabControl.Controls.Add(tabAudience);
            tabControl.Location = new Point(12, 12);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(660, 487);
            tabControl.TabIndex = 0;
            // 
            // tabGeneral
            // 
            tabGeneral.Controls.Add(grpConsole);
            tabGeneral.Controls.Add(grpMultipleMonitorControl);
            tabGeneral.Controls.Add(grpPreviews);
            tabGeneral.Location = new Point(4, 24);
            tabGeneral.Name = "tabGeneral";
            tabGeneral.Padding = new Padding(3);
            tabGeneral.Size = new Size(652, 459);
            tabGeneral.TabIndex = 0;
            tabGeneral.Text = "Screens";
            tabGeneral.UseVisualStyleBackColor = true;
            // 
            // grpPreviews
            // 
            grpPreviews.Controls.Add(lblPreviewOrientation);
            grpPreviews.Controls.Add(cmbPreviewOrientation);
            grpPreviews.Controls.Add(chkEnablePreviewAutomatically);
            grpPreviews.Location = new Point(16, 16);
            grpPreviews.Name = "grpPreviews";
            grpPreviews.Size = new Size(570, 90);
            grpPreviews.TabIndex = 0;
            grpPreviews.TabStop = false;
            grpPreviews.Text = "Previews";
            // 
            // chkFullScreenGuestScreen
            // 
            chkFullScreenGuestScreen.AutoSize = true;
            chkFullScreenGuestScreen.Location = new Point(20, 100);
            chkFullScreenGuestScreen.Name = "chkFullScreenGuestScreen";
            chkFullScreenGuestScreen.Size = new Size(170, 19);
            chkFullScreenGuestScreen.TabIndex = 3;
            chkFullScreenGuestScreen.Text = "Full Screen Guest Screen";
            chkFullScreenGuestScreen.UseVisualStyleBackColor = true;
            chkFullScreenGuestScreen.CheckedChanged += chkFullScreenGuest_CheckedChanged;
            // 
            // chkFullScreenHostScreen
            // 
            chkFullScreenHostScreen.AutoSize = true;
            chkFullScreenHostScreen.Location = new Point(20, 70);
            chkFullScreenHostScreen.Name = "chkFullScreenHostScreen";
            chkFullScreenHostScreen.Size = new Size(163, 19);
            chkFullScreenHostScreen.TabIndex = 2;
            chkFullScreenHostScreen.Text = "Full Screen Host Screen";
            chkFullScreenHostScreen.UseVisualStyleBackColor = true;
            chkFullScreenHostScreen.CheckedChanged += chkFullScreenHost_CheckedChanged;
            // 
            // chkEnablePreviewAutomatically
            // 
            chkEnablePreviewAutomatically.AutoSize = true;
            chkEnablePreviewAutomatically.Location = new Point(20, 20);
            chkEnablePreviewAutomatically.Name = "chkEnablePreviewAutomatically";
            chkEnablePreviewAutomatically.Size = new Size(200, 19);
            chkEnablePreviewAutomatically.TabIndex = 0;
            chkEnablePreviewAutomatically.Text = "Enable Preview Automatically";
            chkEnablePreviewAutomatically.UseVisualStyleBackColor = true;
            chkEnablePreviewAutomatically.CheckedChanged += Control_Changed;
            // 
            // lblPreviewOrientation
            // 
            lblPreviewOrientation.AutoSize = true;
            lblPreviewOrientation.Location = new Point(40, 45);
            lblPreviewOrientation.Name = "lblPreviewOrientation";
            lblPreviewOrientation.Size = new Size(120, 15);
            lblPreviewOrientation.TabIndex = 6;
            lblPreviewOrientation.Text = "Preview Orientation:";
            // 
            // cmbPreviewOrientation
            // 
            cmbPreviewOrientation.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPreviewOrientation.FormattingEnabled = true;
            cmbPreviewOrientation.Items.AddRange(new object[] { "Vertical", "Horizontal" });
            cmbPreviewOrientation.Location = new Point(165, 42);
            cmbPreviewOrientation.Name = "cmbPreviewOrientation";
            cmbPreviewOrientation.Size = new Size(120, 23);
            cmbPreviewOrientation.TabIndex = 7;
            cmbPreviewOrientation.SelectedIndexChanged += Control_Changed;
            // 
            // grpMultipleMonitorControl
            // 
            grpMultipleMonitorControl.Controls.Add(btnIdentifyMonitors);
            grpMultipleMonitorControl.Controls.Add(lblMonitorCount);
            grpMultipleMonitorControl.Controls.Add(lblDebugMode);
            grpMultipleMonitorControl.Controls.Add(cmbMonitorTV);
            grpMultipleMonitorControl.Controls.Add(cmbMonitorGuest);
            grpMultipleMonitorControl.Controls.Add(cmbMonitorHost);
            grpMultipleMonitorControl.Controls.Add(chkFullScreenTVScreen);
            grpMultipleMonitorControl.Controls.Add(chkFullScreenGuestScreen);
            grpMultipleMonitorControl.Controls.Add(chkFullScreenHostScreen);
            grpMultipleMonitorControl.Location = new Point(16, 112);
            grpMultipleMonitorControl.Name = "grpMultipleMonitorControl";
            grpMultipleMonitorControl.Size = new Size(570, 250);
            grpMultipleMonitorControl.TabIndex = 1;
            grpMultipleMonitorControl.TabStop = false;
            grpMultipleMonitorControl.Text = "Multiple Monitor Control";
            // 
            // lblMonitorCount
            // 
            lblMonitorCount.AutoSize = true;
            lblMonitorCount.Location = new Point(20, 25);
            lblMonitorCount.Name = "lblMonitorCount";
            lblMonitorCount.Size = new Size(200, 15);
            lblMonitorCount.TabIndex = 10;
            lblMonitorCount.Text = "Number of Monitors: 0 (4 Monitors are required for this feature)";
            // 
            // lblDebugMode
            // 
            lblDebugMode.AutoSize = true;
            lblDebugMode.ForeColor = Color.Green;
            lblDebugMode.Location = new Point(20, 45);
            lblDebugMode.Name = "lblDebugMode";
            lblDebugMode.Size = new Size(100, 15);
            lblDebugMode.TabIndex = 11;
            lblDebugMode.Text = "**DEBUG MODE**";
            lblDebugMode.Visible = false;
            // 
            // chkFullScreenHostScreen
            // 
            chkFullScreenHostScreen.AutoSize = true;
            chkFullScreenHostScreen.Location = new Point(20, 75);
            chkFullScreenHostScreen.Name = "chkFullScreenHostScreen";
            chkFullScreenHostScreen.Size = new Size(163, 19);
            chkFullScreenHostScreen.TabIndex = 2;
            chkFullScreenHostScreen.Text = "Full Screen Host Screen";
            chkFullScreenHostScreen.UseVisualStyleBackColor = true;
            chkFullScreenHostScreen.CheckedChanged += chkFullScreenHost_CheckedChanged;
            // 
            // chkFullScreenGuestScreen
            // 
            chkFullScreenGuestScreen.AutoSize = true;
            chkFullScreenGuestScreen.Location = new Point(20, 108);
            chkFullScreenGuestScreen.Name = "chkFullScreenGuestScreen";
            chkFullScreenGuestScreen.Size = new Size(170, 19);
            chkFullScreenGuestScreen.TabIndex = 3;
            chkFullScreenGuestScreen.Text = "Full Screen Guest Screen";
            chkFullScreenGuestScreen.UseVisualStyleBackColor = true;
            chkFullScreenGuestScreen.CheckedChanged += chkFullScreenGuest_CheckedChanged;
            // 
            // chkFullScreenTVScreen
            // 
            chkFullScreenTVScreen.AutoSize = true;
            chkFullScreenTVScreen.Location = new Point(20, 141);
            chkFullScreenTVScreen.Name = "chkFullScreenTVScreen";
            chkFullScreenTVScreen.Size = new Size(151, 19);
            chkFullScreenTVScreen.TabIndex = 1;
            chkFullScreenTVScreen.Text = "Full Screen TV Screen";
            chkFullScreenTVScreen.UseVisualStyleBackColor = true;
            chkFullScreenTVScreen.CheckedChanged += chkFullScreenTV_CheckedChanged;
            // 
            // cmbMonitorHost
            // 
            cmbMonitorHost.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMonitorHost.FormattingEnabled = true;
            cmbMonitorHost.Location = new Point(210, 73);
            cmbMonitorHost.Name = "cmbMonitorHost";
            cmbMonitorHost.Size = new Size(340, 23);
            cmbMonitorHost.TabIndex = 4;
            cmbMonitorHost.SelectedIndexChanged += Control_Changed;
            // 
            // cmbMonitorGuest
            // 
            cmbMonitorGuest.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMonitorGuest.FormattingEnabled = true;
            cmbMonitorGuest.Location = new Point(210, 106);
            cmbMonitorGuest.Name = "cmbMonitorGuest";
            cmbMonitorGuest.Size = new Size(340, 23);
            cmbMonitorGuest.TabIndex = 5;
            cmbMonitorGuest.SelectedIndexChanged += Control_Changed;
            // 
            // cmbMonitorTV
            // 
            cmbMonitorTV.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMonitorTV.FormattingEnabled = true;
            cmbMonitorTV.Location = new Point(210, 139);
            cmbMonitorTV.Name = "cmbMonitorTV";
            cmbMonitorTV.Size = new Size(340, 23);
            cmbMonitorTV.TabIndex = 6;
            cmbMonitorTV.SelectedIndexChanged += Control_Changed;
            // 
            // btnIdentifyMonitors
            // 
            btnIdentifyMonitors.Location = new Point(20, 175);
            btnIdentifyMonitors.Name = "btnIdentifyMonitors";
            btnIdentifyMonitors.Size = new Size(150, 30);
            btnIdentifyMonitors.TabIndex = 7;
            btnIdentifyMonitors.Text = "Identify Monitors";
            btnIdentifyMonitors.UseVisualStyleBackColor = true;
            btnIdentifyMonitors.Click += btnIdentifyMonitors_Click;
            // 
            // grpConsole
            // 
            grpConsole.Controls.Add(chkShowConsole);            grpConsole.Controls.Add(chkShowWebServiceConsole);            grpConsole.Location = new Point(16, 370);
            grpConsole.Name = "grpConsole";
            grpConsole.Size = new Size(570, 60);
            grpConsole.TabIndex = 2;
            grpConsole.TabStop = false;
            grpConsole.Text = "Console";
            // 
            // chkShowConsole
            // 
            chkShowConsole.AutoSize = true;
            chkShowConsole.Location = new Point(20, 25);
            chkShowConsole.Name = "chkShowConsole";
            chkShowConsole.Size = new Size(103, 19);
            chkShowConsole.TabIndex = 0;
            chkShowConsole.Text = "Show Console";
            chkShowConsole.UseVisualStyleBackColor = true;
            chkShowConsole.CheckedChanged += Control_Changed;            // 
            // chkShowWebServiceConsole
            // 
            chkShowWebServiceConsole.AutoSize = true;
            chkShowWebServiceConsole.Location = new Point(20, 55);
            chkShowWebServiceConsole.Name = "chkShowWebServiceConsole";
            chkShowWebServiceConsole.Size = new Size(200, 19);
            chkShowWebServiceConsole.TabIndex = 1;
            chkShowWebServiceConsole.Text = "Show Web Service Console";
            chkShowWebServiceConsole.UseVisualStyleBackColor = true;
            chkShowWebServiceConsole.CheckedChanged += chkShowWebServiceConsole_CheckedChanged;            // 
            // tabBroadcast
            // 
            tabBroadcast.Controls.Add(grpBroadcast);
            tabBroadcast.Location = new Point(4, 24);
            tabBroadcast.Name = "tabBroadcast";
            tabBroadcast.Padding = new Padding(3);
            tabBroadcast.Size = new Size(652, 459);
            tabBroadcast.TabIndex = 1;
            tabBroadcast.Text = "Broadcast";
            tabBroadcast.UseVisualStyleBackColor = true;
            // 
            // grpBroadcast
            // 
            grpBroadcast.Location = new Point(16, 16);
            grpBroadcast.Name = "grpBroadcast";
            grpBroadcast.Size = new Size(600, 400);
            grpBroadcast.TabIndex = 0;
            grpBroadcast.TabStop = false;
            grpBroadcast.Text = "Broadcast Settings (Future Feature)";
            // 
            // tabLifelines
            // 
            tabLifelines.Controls.Add(grpLifeline4);
            tabLifelines.Controls.Add(grpLifeline3);
            tabLifelines.Controls.Add(grpLifeline2);
            tabLifelines.Controls.Add(grpLifeline1);
            tabLifelines.Controls.Add(numTotalLifelines);
            tabLifelines.Controls.Add(lblTotalLifelines);
            tabLifelines.Location = new Point(4, 24);
            tabLifelines.Name = "tabLifelines";
            tabLifelines.Padding = new Padding(3);
            tabLifelines.Size = new Size(652, 459);
            tabLifelines.TabIndex = 1;
            tabLifelines.Text = "Lifelines";
            tabLifelines.UseVisualStyleBackColor = true;
            // 
            // tabMoneyTree
            // 
            tabMoneyTree.Location = new Point(4, 24);
            tabMoneyTree.Name = "tabMoneyTree";
            tabMoneyTree.Padding = new Padding(3);
            tabMoneyTree.Size = new Size(652, 459);
            tabMoneyTree.TabIndex = 3;
            tabMoneyTree.Text = "Money Tree";
            tabMoneyTree.UseVisualStyleBackColor = true;
            // 
            // grpLifeline4
            // 
            grpLifeline4.Controls.Add(cmbLifeline4Availability);
            grpLifeline4.Controls.Add(cmbLifeline4Type);
            grpLifeline4.Controls.Add(lblLifeline4);
            grpLifeline4.Location = new Point(336, 236);
            grpLifeline4.Name = "grpLifeline4";
            grpLifeline4.Size = new Size(300, 200);
            grpLifeline4.TabIndex = 5;
            grpLifeline4.TabStop = false;
            grpLifeline4.Text = "Lifeline 4";
            // 
            // cmbLifeline4Availability
            // 
            cmbLifeline4Availability.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLifeline4Availability.FormattingEnabled = true;
            cmbLifeline4Availability.Items.AddRange(new object[] { "Always Available", "After Question 5", "After Question 10", "In Risk Mode Only" });
            cmbLifeline4Availability.Location = new Point(20, 65);
            cmbLifeline4Availability.Name = "cmbLifeline4Availability";
            cmbLifeline4Availability.Size = new Size(260, 23);
            cmbLifeline4Availability.TabIndex = 2;
            cmbLifeline4Availability.SelectedIndex = 0;
            cmbLifeline4Availability.SelectedIndexChanged += Control_Changed;
            // 
            // cmbLifeline4Type
            // 
            cmbLifeline4Type.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLifeline4Type.FormattingEnabled = true;
            cmbLifeline4Type.Items.AddRange(new object[] { "50:50", "Phone a Friend", "Ask the Audience", "Switch the Question", "Ask the Host", "Double Dip" });
            cmbLifeline4Type.Location = new Point(80, 25);
            cmbLifeline4Type.Name = "cmbLifeline4Type";
            cmbLifeline4Type.Size = new Size(200, 23);
            cmbLifeline4Type.TabIndex = 1;
            cmbLifeline4Type.SelectedIndexChanged += Control_Changed;
            // 
            // lblLifeline4
            // 
            lblLifeline4.AutoSize = true;
            lblLifeline4.Location = new Point(20, 28);
            lblLifeline4.Name = "lblLifeline4";
            lblLifeline4.Size = new Size(34, 15);
            lblLifeline4.TabIndex = 0;
            lblLifeline4.Text = "Type:";
            // 
            // grpLifeline3
            // 
            grpLifeline3.Controls.Add(cmbLifeline3Availability);
            grpLifeline3.Controls.Add(cmbLifeline3Type);
            grpLifeline3.Controls.Add(lblLifeline3);
            grpLifeline3.Location = new Point(16, 236);
            grpLifeline3.Name = "grpLifeline3";
            grpLifeline3.Size = new Size(300, 200);
            grpLifeline3.TabIndex = 4;
            grpLifeline3.TabStop = false;
            grpLifeline3.Text = "Lifeline 3";
            // 
            // cmbLifeline3Availability
            // 
            cmbLifeline3Availability.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLifeline3Availability.FormattingEnabled = true;
            cmbLifeline3Availability.Items.AddRange(new object[] { "Always Available", "After Question 5", "After Question 10", "In Risk Mode Only" });
            cmbLifeline3Availability.Location = new Point(20, 65);
            cmbLifeline3Availability.Name = "cmbLifeline3Availability";
            cmbLifeline3Availability.Size = new Size(260, 23);
            cmbLifeline3Availability.TabIndex = 2;
            cmbLifeline3Availability.SelectedIndex = 0;
            cmbLifeline3Availability.SelectedIndexChanged += Control_Changed;
            // 
            // cmbLifeline3Type
            // 
            cmbLifeline3Type.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLifeline3Type.FormattingEnabled = true;
            cmbLifeline3Type.Items.AddRange(new object[] { "50:50", "Phone a Friend", "Ask the Audience", "Switch the Question", "Ask the Host", "Double Dip" });
            cmbLifeline3Type.Location = new Point(80, 25);
            cmbLifeline3Type.Name = "cmbLifeline3Type";
            cmbLifeline3Type.Size = new Size(200, 23);
            cmbLifeline3Type.TabIndex = 1;
            cmbLifeline3Type.SelectedIndexChanged += Control_Changed;
            // 
            // lblLifeline3
            // 
            lblLifeline3.AutoSize = true;
            lblLifeline3.Location = new Point(20, 28);
            lblLifeline3.Name = "lblLifeline3";
            lblLifeline3.Size = new Size(34, 15);
            lblLifeline3.TabIndex = 0;
            lblLifeline3.Text = "Type:";
            // 
            // grpLifeline2
            // 
            grpLifeline2.Controls.Add(cmbLifeline2Availability);
            grpLifeline2.Controls.Add(cmbLifeline2Type);
            grpLifeline2.Controls.Add(lblLifeline2);
            grpLifeline2.Location = new Point(336, 56);
            grpLifeline2.Name = "grpLifeline2";
            grpLifeline2.Size = new Size(300, 160);
            grpLifeline2.TabIndex = 3;
            grpLifeline2.TabStop = false;
            grpLifeline2.Text = "Lifeline 2";
            // 
            // cmbLifeline2Availability
            // 
            cmbLifeline2Availability.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLifeline2Availability.FormattingEnabled = true;
            cmbLifeline2Availability.Items.AddRange(new object[] { "Always Available", "After Question 5", "After Question 10", "In Risk Mode Only" });
            cmbLifeline2Availability.Location = new Point(20, 55);
            cmbLifeline2Availability.Name = "cmbLifeline2Availability";
            cmbLifeline2Availability.Size = new Size(260, 23);
            cmbLifeline2Availability.TabIndex = 2;
            cmbLifeline2Availability.SelectedIndex = 0;
            cmbLifeline2Availability.SelectedIndexChanged += Control_Changed;
            // 
            // cmbLifeline2Type
            // 
            cmbLifeline2Type.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLifeline2Type.FormattingEnabled = true;
            cmbLifeline2Type.Items.AddRange(new object[] { "50:50", "Phone a Friend", "Ask the Audience", "Switch the Question", "Ask the Host", "Double Dip" });
            cmbLifeline2Type.Location = new Point(80, 25);
            cmbLifeline2Type.Name = "cmbLifeline2Type";
            cmbLifeline2Type.Size = new Size(200, 23);
            cmbLifeline2Type.TabIndex = 1;
            cmbLifeline2Type.SelectedIndexChanged += Control_Changed;
            // 
            // lblLifeline2
            // 
            lblLifeline2.AutoSize = true;
            lblLifeline2.Location = new Point(20, 28);
            lblLifeline2.Name = "lblLifeline2";
            lblLifeline2.Size = new Size(34, 15);
            lblLifeline2.TabIndex = 0;
            lblLifeline2.Text = "Type:";
            // 
            // grpLifeline1
            // 
            grpLifeline1.Controls.Add(cmbLifeline1Availability);
            grpLifeline1.Controls.Add(cmbLifeline1Type);
            grpLifeline1.Controls.Add(lblLifeline1);
            grpLifeline1.Location = new Point(16, 56);
            grpLifeline1.Name = "grpLifeline1";
            grpLifeline1.Size = new Size(300, 160);
            grpLifeline1.TabIndex = 2;
            grpLifeline1.TabStop = false;
            grpLifeline1.Text = "Lifeline 1";
            // 
            // cmbLifeline1Availability
            // 
            cmbLifeline1Availability.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLifeline1Availability.FormattingEnabled = true;
            cmbLifeline1Availability.Items.AddRange(new object[] { "Always Available", "After Question 5", "After Question 10", "In Risk Mode Only" });
            cmbLifeline1Availability.Location = new Point(20, 55);
            cmbLifeline1Availability.Name = "cmbLifeline1Availability";
            cmbLifeline1Availability.Size = new Size(260, 23);
            cmbLifeline1Availability.TabIndex = 2;
            cmbLifeline1Availability.SelectedIndex = 0;
            cmbLifeline1Availability.SelectedIndexChanged += Control_Changed;
            // 
            // cmbLifeline1Type
            // 
            cmbLifeline1Type.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLifeline1Type.FormattingEnabled = true;
            cmbLifeline1Type.Items.AddRange(new object[] { "50:50", "Phone a Friend", "Ask the Audience", "Switch the Question", "Ask the Host", "Double Dip" });
            cmbLifeline1Type.Location = new Point(80, 25);
            cmbLifeline1Type.Name = "cmbLifeline1Type";
            cmbLifeline1Type.Size = new Size(200, 23);
            cmbLifeline1Type.TabIndex = 1;
            cmbLifeline1Type.SelectedIndexChanged += Control_Changed;
            // 
            // lblLifeline1
            // 
            lblLifeline1.AutoSize = true;
            lblLifeline1.Location = new Point(20, 28);
            lblLifeline1.Name = "lblLifeline1";
            lblLifeline1.Size = new Size(34, 15);
            lblLifeline1.TabIndex = 0;
            lblLifeline1.Text = "Type:";
            // 
            // numTotalLifelines
            // 
            numTotalLifelines.Location = new Point(140, 16);
            numTotalLifelines.Maximum = new decimal(new int[] { 4, 0, 0, 0 });
            numTotalLifelines.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numTotalLifelines.Name = "numTotalLifelines";
            numTotalLifelines.Size = new Size(80, 23);
            numTotalLifelines.TabIndex = 1;
            numTotalLifelines.Value = new decimal(new int[] { 4, 0, 0, 0 });
            numTotalLifelines.ValueChanged += numTotalLifelines_ValueChanged;
            // 
            // lblTotalLifelines
            // 
            lblTotalLifelines.AutoSize = true;
            lblTotalLifelines.Location = new Point(16, 18);
            lblTotalLifelines.Name = "lblTotalLifelines";
            lblTotalLifelines.Size = new Size(86, 15);
            lblTotalLifelines.TabIndex = 0;
            lblTotalLifelines.Text = "Total Lifelines:";
            // 
            // tabSounds
            // 
            tabSounds.AutoScroll = true;
            tabSounds.Controls.Add(grpSoundPack);
            tabSounds.Location = new Point(4, 24);
            tabSounds.Name = "tabSounds";
            tabSounds.Padding = new Padding(3);
            tabSounds.Size = new Size(652, 459);
            tabSounds.TabIndex = 2;
            tabSounds.Text = "Sounds";
            tabSounds.UseVisualStyleBackColor = true;
            // 
            // grpSoundPack
            // 
            grpSoundPack.Controls.Add(lstSoundPackInfo);
            grpSoundPack.Controls.Add(btnExportExample);
            grpSoundPack.Controls.Add(btnRemovePack);
            grpSoundPack.Controls.Add(btnImportPack);
            grpSoundPack.Controls.Add(cmbSoundPack);
            grpSoundPack.Location = new Point(16, 16);
            grpSoundPack.Name = "grpSoundPack";
            grpSoundPack.Size = new Size(620, 420);
            grpSoundPack.TabIndex = 0;
            grpSoundPack.TabStop = false;
            grpSoundPack.Text = "Sound Pack";
            // 
            // cmbSoundPack
            // 
            cmbSoundPack.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSoundPack.FormattingEnabled = true;
            cmbSoundPack.Location = new Point(90, 25);
            cmbSoundPack.Name = "cmbSoundPack";
            cmbSoundPack.Size = new Size(200, 23);
            cmbSoundPack.TabIndex = 0;
            cmbSoundPack.SelectedIndexChanged += cmbSoundPack_SelectedIndexChanged;
            // 
            // 
            // 
            // btnImportPack
            // 
            btnImportPack.Location = new Point(300, 24);
            btnImportPack.Name = "btnImportPack";
            btnImportPack.Size = new Size(90, 25);
            btnImportPack.TabIndex = 2;
            btnImportPack.Text = "Import...";
            btnImportPack.UseVisualStyleBackColor = true;
            btnImportPack.Click += btnImportPack_Click;
            // 
            // btnRemovePack
            // 
            btnRemovePack.Location = new Point(400, 24);
            btnRemovePack.Name = "btnRemovePack";
            btnRemovePack.Size = new Size(90, 25);
            btnRemovePack.TabIndex = 3;
            btnRemovePack.Text = "Remove";
            btnRemovePack.UseVisualStyleBackColor = true;
            btnRemovePack.Click += btnRemovePack_Click;
            // 
            // btnExportExample
            // 
            btnExportExample.Location = new Point(500, 24);
            btnExportExample.Name = "btnExportExample";
            btnExportExample.Size = new Size(110, 25);
            btnExportExample.TabIndex = 4;
            btnExportExample.Text = "Export Example";
            btnExportExample.UseVisualStyleBackColor = true;
            btnExportExample.Click += btnExportExample_Click;
            // 
            // lstSoundPackInfo
            // 
            lstSoundPackInfo.FormattingEnabled = true;
            lstSoundPackInfo.IntegralHeight = false;
            lstSoundPackInfo.Location = new Point(16, 60);
            lstSoundPackInfo.Name = "lstSoundPackInfo";
            lstSoundPackInfo.SelectionMode = SelectionMode.None;
            lstSoundPackInfo.Size = new Size(588, 345);
            lstSoundPackInfo.TabIndex = 5;
            // 
            // tabAudience
            // 
            tabAudience.AutoScroll = true;
            tabAudience.Controls.Add(grpAudienceServer);
            tabAudience.Controls.Add(lblAudienceInfo);
            tabAudience.Location = new Point(4, 24);
            tabAudience.Name = "tabAudience";
            tabAudience.Padding = new Padding(3);
            tabAudience.Size = new Size(652, 459);
            tabAudience.TabIndex = 5;
            tabAudience.Text = "Audience";
            tabAudience.UseVisualStyleBackColor = true;
            // 
            // grpAudienceServer
            // 
            grpAudienceServer.Controls.Add(lblServerIP);
            grpAudienceServer.Controls.Add(cmbServerIP);
            grpAudienceServer.Controls.Add(lblServerPort);
            grpAudienceServer.Controls.Add(txtServerPort);
            grpAudienceServer.Controls.Add(btnCheckPort);
            grpAudienceServer.Controls.Add(lblPortStatus);
            grpAudienceServer.Controls.Add(chkAutoStart);
            grpAudienceServer.Controls.Add(btnStartServer);
            grpAudienceServer.Controls.Add(btnStopServer);
            grpAudienceServer.Controls.Add(lblServerStatus);
            grpAudienceServer.Location = new Point(16, 16);
            grpAudienceServer.Name = "grpAudienceServer";
            grpAudienceServer.Size = new Size(620, 260);
            grpAudienceServer.TabIndex = 0;
            grpAudienceServer.TabStop = false;
            grpAudienceServer.Text = "Web Server Configuration";
            // 
            // lblServerIP
            // 
            lblServerIP.AutoSize = true;
            lblServerIP.Location = new Point(20, 30);
            lblServerIP.Name = "lblServerIP";
            lblServerIP.Size = new Size(70, 15);
            lblServerIP.TabIndex = 0;
            lblServerIP.Text = "IP Address:";
            // 
            // cmbServerIP
            // 
            cmbServerIP.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbServerIP.FormattingEnabled = true;
            cmbServerIP.Location = new Point(140, 27);
            cmbServerIP.Name = "cmbServerIP";
            cmbServerIP.Size = new Size(400, 23);
            cmbServerIP.TabIndex = 1;
            cmbServerIP.SelectedIndexChanged += Control_Changed;
            // 
            // lblServerPort
            // 
            lblServerPort.AutoSize = true;
            lblServerPort.Location = new Point(20, 65);
            lblServerPort.Name = "lblServerPort";
            lblServerPort.Size = new Size(32, 15);
            lblServerPort.TabIndex = 2;
            lblServerPort.Text = "Port:";
            // 
            // txtServerPort
            // 
            txtServerPort.Location = new Point(140, 62);
            txtServerPort.Name = "txtServerPort";
            txtServerPort.Size = new Size(100, 23);
            txtServerPort.TabIndex = 3;
            txtServerPort.TextChanged += Control_Changed;
            // 
            // btnCheckPort
            // 
            btnCheckPort.Location = new Point(250, 61);
            btnCheckPort.Name = "btnCheckPort";
            btnCheckPort.Size = new Size(100, 25);
            btnCheckPort.TabIndex = 4;
            btnCheckPort.Text = "Check in use";
            btnCheckPort.UseVisualStyleBackColor = true;
            btnCheckPort.Click += btnCheckPort_Click;
            // 
            // lblPortStatus
            // 
            lblPortStatus.AutoSize = true;
            lblPortStatus.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblPortStatus.Location = new Point(360, 63);
            lblPortStatus.Name = "lblPortStatus";
            lblPortStatus.Size = new Size(0, 21);
            lblPortStatus.TabIndex = 5;
            // 
            // chkAutoStart
            // 
            chkAutoStart.AutoSize = true;
            chkAutoStart.Location = new Point(20, 105);
            chkAutoStart.Name = "chkAutoStart";
            chkAutoStart.Size = new Size(260, 19);
            chkAutoStart.TabIndex = 6;
            chkAutoStart.Text = "Start server automatically on application startup";
            chkAutoStart.UseVisualStyleBackColor = true;
            chkAutoStart.CheckedChanged += chkAutoStart_CheckedChanged;
            // 
            // btnStartServer
            // 
            btnStartServer.Location = new Point(20, 145);
            btnStartServer.Name = "btnStartServer";
            btnStartServer.Size = new Size(120, 35);
            btnStartServer.TabIndex = 7;
            btnStartServer.Text = "Start Server";
            btnStartServer.UseVisualStyleBackColor = true;
            btnStartServer.Click += btnStartServer_Click;
            // 
            // btnStopServer
            // 
            btnStopServer.Enabled = false;
            btnStopServer.Location = new Point(150, 145);
            btnStopServer.Name = "btnStopServer";
            btnStopServer.Size = new Size(120, 35);
            btnStopServer.TabIndex = 8;
            btnStopServer.Text = "Stop Server";
            btnStopServer.UseVisualStyleBackColor = true;
            btnStopServer.Click += btnStopServer_Click;
            // 
            // lblServerStatus
            // 
            lblServerStatus.AutoSize = true;
            lblServerStatus.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblServerStatus.ForeColor = SystemColors.ControlDarkDark;
            lblServerStatus.Location = new Point(20, 195);
            lblServerStatus.Name = "lblServerStatus";
            lblServerStatus.Size = new Size(101, 15);
            lblServerStatus.TabIndex = 9;
            lblServerStatus.Text = "Server Stopped";
            // 
            // lblAudienceInfo
            // 
            lblAudienceInfo.Location = new Point(16, 290);
            lblAudienceInfo.Name = "lblAudienceInfo";
            lblAudienceInfo.Size = new Size(620, 150);
            lblAudienceInfo.TabIndex = 1;
            lblAudienceInfo.Text = @"Information:
• Audience members connect via web browser to participate
• Use QR code or URL to join FFF and Ask The Audience sessions
• Port must be open on firewall for external network access
• Localhost (127.0.0.1) restricts connections to this computer only
• 0.0.0.0 allows connections from all network interfaces
• Local IP addresses restrict to specific network subnets";
            // // btnOK
            // 
            btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnOK.Location = new Point(416, 515);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(80, 30);
            btnOK.TabIndex = 1;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.Location = new Point(502, 515);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(80, 30);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // OptionsDialog
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(684, 561);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(tabControl);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "OptionsDialog";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Settings";
            tabControl.ResumeLayout(false);
            tabGeneral.ResumeLayout(false);
            grpPreviews.ResumeLayout(false);
            grpPreviews.PerformLayout();
            grpMultipleMonitorControl.ResumeLayout(false);
            grpMultipleMonitorControl.PerformLayout();
            grpConsole.ResumeLayout(false);
            grpConsole.PerformLayout();
            tabBroadcast.ResumeLayout(false);
            grpBroadcast.ResumeLayout(false);
            grpBroadcast.PerformLayout();
            tabLifelines.ResumeLayout(false);
            tabLifelines.PerformLayout();
            grpLifeline4.ResumeLayout(false);
            grpLifeline4.PerformLayout();
            grpLifeline3.ResumeLayout(false);
            grpLifeline3.PerformLayout();
            grpLifeline2.ResumeLayout(false);
            grpLifeline2.PerformLayout();
            grpLifeline1.ResumeLayout(false);
            grpLifeline1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numTotalLifelines).EndInit();
            tabSounds.ResumeLayout(false);
            grpSoundPack.ResumeLayout(false);
            tabAudience.ResumeLayout(false);
            grpAudienceServer.ResumeLayout(false);
            grpAudienceServer.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabControl;
        private TabPage tabGeneral;
        private TabPage tabBroadcast;
        private TabPage tabLifelines;
        private TabPage tabMoneyTree;
        private TabPage tabSounds;
        private TabPage tabAudience;
        private GroupBox grpPrizeValues;
        private NumericUpDown numLevel01;
        private NumericUpDown numLevel02;
        private NumericUpDown numLevel03;
        private NumericUpDown numLevel04;
        private NumericUpDown numLevel05;
        private NumericUpDown numLevel06;
        private NumericUpDown numLevel07;
        private NumericUpDown numLevel08;
        private NumericUpDown numLevel09;
        private NumericUpDown numLevel10;
        private NumericUpDown numLevel11;
        private NumericUpDown numLevel12;
        private NumericUpDown numLevel13;
        private NumericUpDown numLevel14;
        private NumericUpDown numLevel15;
        private GroupBox grpCurrency;
        private RadioButton radCurrencyDollar;
        private RadioButton radCurrencyEuro;
        private RadioButton radCurrencyPound;
        private RadioButton radCurrencyYen;
        private RadioButton radCurrencyOther;
        private TextBox txtCustomCurrency;
        private CheckBox chkCurrencyAtSuffix;
        private GroupBox grpSafetyNets;
        private Label lblSafetyNet1;
        private Label lblSafetyNet2;
        private NumericUpDown numSafetyNet1;
        private NumericUpDown numSafetyNet2;
        private Button btnOK;
        private Button btnCancel;
        private GroupBox grpPreviews;
        private GroupBox grpMultipleMonitorControl;
        private GroupBox grpBroadcast;
        private CheckBox chkEnablePreviewAutomatically;
        private Label lblPreviewOrientation;
        private ComboBox cmbPreviewOrientation;
        private Label lblMonitorCount;
        private Label lblDebugMode;
        private CheckBox chkFullScreenHostScreen;
        private CheckBox chkFullScreenGuestScreen;
        private CheckBox chkFullScreenTVScreen;
        private Label lblTotalLifelines;
        private ComboBox cmbMonitorHost;
        private ComboBox cmbMonitorGuest;
        private ComboBox cmbMonitorTV;
        private Button btnIdentifyMonitors;
        private NumericUpDown numTotalLifelines;
        private GroupBox grpLifeline1;
        private Label lblLifeline1;
        private ComboBox cmbLifeline1Type;
        private ComboBox cmbLifeline1Availability;
        private GroupBox grpLifeline2;
        private ComboBox cmbLifeline2Type;
        private Label lblLifeline2;
        private ComboBox cmbLifeline2Availability;
        private GroupBox grpLifeline3;
        private ComboBox cmbLifeline3Type;
        private Label lblLifeline3;
        private ComboBox cmbLifeline3Availability;
        private GroupBox grpLifeline4;
        private ComboBox cmbLifeline4Type;
        private Label lblLifeline4;
        private ComboBox cmbLifeline4Availability;
        private GroupBox grpSoundPack;
        private Label lblSoundPack;
        private ComboBox cmbSoundPack;
        private Button btnImportPack;
        private Button btnRemovePack;
        private Button btnExportExample;
        private ListBox lstSoundPackInfo;
        private GroupBox grpConsole;
        private CheckBox chkShowConsole;
        private CheckBox chkShowWebServiceConsole;
        private GroupBox grpAudienceServer;
        private Label lblServerIP;
        private ComboBox cmbServerIP;
        private Label lblServerPort;
        private TextBox txtServerPort;
        private Button btnCheckPort;
        private Label lblPortStatus;
        private CheckBox chkAutoStart;
        private Button btnStartServer;
        private Button btnStopServer;
        private Label lblServerStatus;
        private Label lblAudienceInfo;
    }
}



