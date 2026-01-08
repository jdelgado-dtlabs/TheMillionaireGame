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
            tabScreens = new TabPage();
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
            btnOpenGameConsole = new Button();
            btnOpenWebServerConsole = new Button();
            tabBroadcast = new TabPage();
            grpBroadcast = new GroupBox();
            lblBackgroundMode = new Label();
            radModePrerendered = new RadioButton();
            radModeChromaKey = new RadioButton();
            lblBackground = new Label();
            cmbBackground = new ComboBox();
            btnSelectBackground = new Button();
            lblChromaColor = new Label();
            btnChromaColor = new Button();
            lblChromaColorPreview = new Label();
            lblBackgroundInfo = new Label();
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
            grpNumberFormat = new GroupBox();
            lblThousandsSeparator = new Label();
            radSeparatorComma = new RadioButton();
            radSeparatorPeriod = new RadioButton();
            radSeparatorSpace = new RadioButton();
            radSeparatorNone = new RadioButton();
            grpSafetyNets = new GroupBox();
            numSafetyNet2 = new NumericUpDown();
            numSafetyNet1 = new NumericUpDown();
            lblSafetyNet2 = new Label();
            lblSafetyNet1 = new Label();
            cmbLifeline4Availability = new ComboBox();
            cmbLifeline4Type = new ComboBox();
            lblLifeline4 = new Label();
            cmbLifeline3Availability = new ComboBox();
            cmbLifeline3Type = new ComboBox();
            lblLifeline3 = new Label();
            cmbLifeline2Availability = new ComboBox();
            cmbLifeline2Type = new ComboBox();
            lblLifeline2 = new Label();
            cmbLifeline1Availability = new ComboBox();
            cmbLifeline1Type = new ComboBox();
            lblLifeline1 = new Label();
            numTotalLifelines = new NumericUpDown();
            lblTotalLifelines = new Label();
            tabSounds = new TabPage();
            tabStreamDeck = new TabPage();
            grpStreamDeckConfig = new GroupBox();
            chkEnableStreamDeck = new CheckBox();
            lblStreamDeckInfo = new Label();
            pnlStreamDeckLayout = new Panel();
            picDynamic = new PictureBox();
            picAnswerA = new PictureBox();
            picAnswerB = new PictureBox();
            picReveal = new PictureBox();
            picAnswerC = new PictureBox();
            picAnswerD = new PictureBox();
            lblDynamic = new Label();
            lblAnswerA = new Label();
            lblAnswerB = new Label();
            lblReveal = new Label();
            lblAnswerC = new Label();
            lblAnswerD = new Label();
            tabControlSounds = new TabControl();
            tabSoundpack = new TabPage();
            tabAudio = new TabPage();
            grpSilenceDetection = new GroupBox();
            trackBarSilenceThreshold = new TrackBar();
            lblSilenceThreshold = new Label();
            lblSilenceThresholdValue = new Label();
            numSilenceDuration = new NumericUpDown();
            lblSilenceDuration = new Label();
            numInitialDelay = new NumericUpDown();
            lblInitialDelay = new Label();
            numFadeoutDuration = new NumericUpDown();
            lblFadeoutDuration = new Label();
            chkEnableSilenceDetection = new CheckBox();
            grpCrossfade = new GroupBox();
            numCrossfadeDuration = new NumericUpDown();
            lblCrossfadeDuration = new Label();
            chkEnableCrossfade = new CheckBox();
            grpAudioProcessing = new GroupBox();
            trackBarMasterGain = new TrackBar();
            lblMasterGain = new Label();
            lblMasterGainValue = new Label();
            trackBarEffectsGain = new TrackBar();
            lblEffectsGain = new Label();
            lblEffectsGainValue = new Label();
            trackBarMusicGain = new TrackBar();
            lblMusicGain = new Label();
            lblMusicGainValue = new Label();
            chkEnableLimiter = new CheckBox();
            dgvSoundPackInfo = new DataGridView();
            txtSearchSounds = new TextBox();
            lblSearchSounds = new Label();
            btnPlaySelected = new Button();
            btnExportExample = new Button();
            btnRemovePack = new Button();
            btnImportPack = new Button();
            cmbSoundPack = new ComboBox();
            lblSoundPack = new Label();
            tabMixer = new TabPage();
            grpAudioDevice = new GroupBox();
            cmbAudioDevice = new ComboBox();
            lblAudioDevice = new Label();
            btnRefreshDevices = new Button();
            lblMixerInfo = new Label();
            btnOK = new Button();
            btnCancel = new Button();
            tabControl.SuspendLayout();
            tabScreens.SuspendLayout();
            grpPreviews.SuspendLayout();
            grpMultipleMonitorControl.SuspendLayout();
            grpConsole.SuspendLayout();
            tabBroadcast.SuspendLayout();
            grpBroadcast.SuspendLayout();
            tabStreamDeck.SuspendLayout();
            grpStreamDeckConfig.SuspendLayout();
            pnlStreamDeckLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picDynamic).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picAnswerA).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picAnswerB).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picReveal).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picAnswerC).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picAnswerD).BeginInit();
            tabLifelines.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numTotalLifelines).BeginInit();
            tabSounds.SuspendLayout();
            tabAudio.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvSoundPackInfo).BeginInit();
            grpSilenceDetection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarSilenceThreshold).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numSilenceDuration).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numInitialDelay).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numFadeoutDuration).BeginInit();
            grpCrossfade.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numCrossfadeDuration).BeginInit();
            grpAudioProcessing.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarMasterGain).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBarEffectsGain).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBarMusicGain).BeginInit();
            tabAudience.SuspendLayout();
            grpAudienceServer.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl
            // 
            tabControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            // TEMPORARILY DISABLED: Screens tab removed due to multi-monitor freeze issue
            // tabControl.Controls.Add(tabScreens);
            tabControl.Controls.Add(tabBroadcast);
            tabControl.Controls.Add(tabLifelines);
            tabControl.Controls.Add(tabMoneyTree);
            tabControl.Controls.Add(tabSounds);
            tabControl.Controls.Add(tabStreamDeck);
            tabControl.Controls.Add(tabAudience);
            tabControl.Location = new Point(12, 12);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(660, 546);
            tabControl.TabIndex = 0;
            // 
            // tabScreens
            // 
            tabScreens.Controls.Add(grpConsole);
            tabScreens.Controls.Add(grpMultipleMonitorControl);
            tabScreens.Controls.Add(grpPreviews);
            tabScreens.Location = new Point(4, 24);
            tabScreens.Name = "tabScreens";
            tabScreens.Padding = new Padding(3);
            tabScreens.Size = new Size(652, 438);
            tabScreens.TabIndex = 0;
            tabScreens.Text = "Screens";
            tabScreens.UseVisualStyleBackColor = true;
            // 
            // grpPreviews
            // 
            grpPreviews.Controls.Add(lblPreviewOrientation);
            grpPreviews.Controls.Add(cmbPreviewOrientation);
            grpPreviews.Controls.Add(chkEnablePreviewAutomatically);
            grpPreviews.Location = new Point(16, 16);
            grpPreviews.Name = "grpPreviews";
            grpPreviews.Size = new Size(620, 90);
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
            grpMultipleMonitorControl.Size = new Size(620, 220);
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
            cmbMonitorHost.SelectedIndexChanged += cmbMonitorHost_SelectedIndexChanged;
            // 
            // cmbMonitorGuest
            // 
            cmbMonitorGuest.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMonitorGuest.FormattingEnabled = true;
            cmbMonitorGuest.Location = new Point(210, 106);
            cmbMonitorGuest.Name = "cmbMonitorGuest";
            cmbMonitorGuest.Size = new Size(340, 23);
            cmbMonitorGuest.TabIndex = 5;
            cmbMonitorGuest.SelectedIndexChanged += cmbMonitorGuest_SelectedIndexChanged;
            // 
            // cmbMonitorTV
            // 
            cmbMonitorTV.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMonitorTV.FormattingEnabled = true;
            cmbMonitorTV.Location = new Point(210, 139);
            cmbMonitorTV.Name = "cmbMonitorTV";
            cmbMonitorTV.Size = new Size(340, 23);
            cmbMonitorTV.TabIndex = 6;
            cmbMonitorTV.SelectedIndexChanged += cmbMonitorTV_SelectedIndexChanged;
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
            grpConsole.Controls.Add(btnOpenGameConsole);
            grpConsole.Controls.Add(btnOpenWebServerConsole);
            grpConsole.Location = new Point(16, 340);
            grpConsole.Name = "grpConsole";
            grpConsole.Size = new Size(620, 90);
            grpConsole.TabIndex = 2;
            grpConsole.TabStop = false;
            grpConsole.Text = "Console Windows";
            // 
            // btnOpenGameConsole
            // 
            btnOpenGameConsole.Location = new Point(20, 25);
            btnOpenGameConsole.Name = "btnOpenGameConsole";
            btnOpenGameConsole.Size = new Size(200, 30);
            btnOpenGameConsole.TabIndex = 0;
            btnOpenGameConsole.Text = "Open Game Console";
            btnOpenGameConsole.UseVisualStyleBackColor = true;
            btnOpenGameConsole.Click += btnOpenGameConsole_Click;
            // 
            // btnOpenWebServerConsole
            // 
            btnOpenWebServerConsole.Location = new Point(240, 25);
            btnOpenWebServerConsole.Name = "btnOpenWebServerConsole";
            btnOpenWebServerConsole.Size = new Size(200, 30);
            btnOpenWebServerConsole.TabIndex = 1;
            btnOpenWebServerConsole.Text = "Open Web Server Console";
            btnOpenWebServerConsole.UseVisualStyleBackColor = true;
            btnOpenWebServerConsole.Click += btnOpenWebServerConsole_Click;
            // 
            // tabBroadcast
            // 
            tabBroadcast.Controls.Add(grpBroadcast);
            tabBroadcast.Location = new Point(4, 24);
            tabBroadcast.Name = "tabBroadcast";
            tabBroadcast.Padding = new Padding(3);
            tabBroadcast.Size = new Size(652, 438);
            tabBroadcast.TabIndex = 1;
            tabBroadcast.Text = "Broadcast";
            tabBroadcast.UseVisualStyleBackColor = true;
            // 
            // grpBroadcast
            // 
            grpBroadcast.Controls.Add(lblBackgroundMode);
            grpBroadcast.Controls.Add(radModePrerendered);
            grpBroadcast.Controls.Add(radModeChromaKey);
            grpBroadcast.Controls.Add(lblBackground);
            grpBroadcast.Controls.Add(cmbBackground);
            grpBroadcast.Controls.Add(btnSelectBackground);
            grpBroadcast.Controls.Add(lblChromaColor);
            grpBroadcast.Controls.Add(btnChromaColor);
            grpBroadcast.Controls.Add(lblChromaColorPreview);
            grpBroadcast.Controls.Add(lblBackgroundInfo);
            grpBroadcast.Location = new Point(16, 16);
            grpBroadcast.Name = "grpBroadcast";
            grpBroadcast.Size = new Size(600, 300);
            grpBroadcast.TabIndex = 0;
            grpBroadcast.TabStop = false;
            grpBroadcast.Text = "TV Screen Background";
            // 
            // lblBackgroundMode
            // 
            lblBackgroundMode.AutoSize = true;
            lblBackgroundMode.Location = new Point(16, 30);
            lblBackgroundMode.Name = "lblBackgroundMode";
            lblBackgroundMode.Size = new Size(110, 15);
            lblBackgroundMode.TabIndex = 0;
            lblBackgroundMode.Text = "Background Mode:";
            // 
            // radModePrerendered
            // 
            radModePrerendered.AutoSize = true;
            radModePrerendered.Checked = true;
            radModePrerendered.Location = new Point(140, 28);
            radModePrerendered.Name = "radModePrerendered";
            radModePrerendered.Size = new Size(137, 19);
            radModePrerendered.TabIndex = 1;
            radModePrerendered.TabStop = true;
            radModePrerendered.Text = "Theme Background";
            radModePrerendered.UseVisualStyleBackColor = true;
            radModePrerendered.CheckedChanged += radModePrerendered_CheckedChanged;
            // 
            // radModeChromaKey
            // 
            radModeChromaKey.AutoSize = true;
            radModeChromaKey.Location = new Point(300, 28);
            radModeChromaKey.Name = "radModeChromaKey";
            radModeChromaKey.Size = new Size(138, 19);
            radModeChromaKey.TabIndex = 2;
            radModeChromaKey.Text = "Chroma Key (Solid)";
            radModeChromaKey.UseVisualStyleBackColor = true;
            radModeChromaKey.CheckedChanged += radModeChromaKey_CheckedChanged;
            // 
            // lblBackground
            // 
            lblBackground.AutoSize = true;
            lblBackground.Location = new Point(16, 70);
            lblBackground.Name = "lblBackground";
            lblBackground.Size = new Size(117, 15);
            lblBackground.TabIndex = 3;
            lblBackground.Text = "Select Background:";
            // 
            // cmbBackground
            // 
            cmbBackground.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBackground.DrawMode = DrawMode.OwnerDrawFixed;
            cmbBackground.FormattingEnabled = true;
            cmbBackground.ItemHeight = 40;
            cmbBackground.Location = new Point(140, 67);
            cmbBackground.Name = "cmbBackground";
            cmbBackground.Size = new Size(250, 23);
            cmbBackground.TabIndex = 4;
            cmbBackground.DrawItem += cmbBackground_DrawItem;
            cmbBackground.SelectedIndexChanged += cmbBackground_SelectedIndexChanged;
            // 
            // btnSelectBackground
            // 
            btnSelectBackground.Location = new Point(400, 66);
            btnSelectBackground.Name = "btnSelectBackground";
            btnSelectBackground.Size = new Size(80, 25);
            btnSelectBackground.TabIndex = 5;
            btnSelectBackground.Text = "Select...";
            btnSelectBackground.UseVisualStyleBackColor = true;
            btnSelectBackground.Visible = false;
            btnSelectBackground.Click += btnSelectBackground_Click;
            // 
            // lblChromaColor
            // 
            lblChromaColor.AutoSize = true;
            lblChromaColor.Location = new Point(16, 70);
            lblChromaColor.Name = "lblChromaColor";
            lblChromaColor.Size = new Size(84, 15);
            lblChromaColor.TabIndex = 5;
            lblChromaColor.Text = "Chroma Color:";
            lblChromaColor.Visible = false;
            // 
            // btnChromaColor
            // 
            btnChromaColor.Location = new Point(140, 66);
            btnChromaColor.Name = "btnChromaColor";
            btnChromaColor.Size = new Size(100, 25);
            btnChromaColor.TabIndex = 6;
            btnChromaColor.Text = "Select Color...";
            btnChromaColor.UseVisualStyleBackColor = true;
            btnChromaColor.Visible = false;
            btnChromaColor.Click += btnChromaColor_Click;
            // 
            // lblChromaColorPreview
            // 
            lblChromaColorPreview.BackColor = System.Drawing.Color.Blue;
            lblChromaColorPreview.BorderStyle = BorderStyle.FixedSingle;
            lblChromaColorPreview.Location = new Point(250, 66);
            lblChromaColorPreview.Name = "lblChromaColorPreview";
            lblChromaColorPreview.Size = new Size(80, 25);
            lblChromaColorPreview.TabIndex = 7;
            lblChromaColorPreview.Visible = false;
            // 
            // lblBackgroundInfo
            // 
            lblBackgroundInfo.Location = new Point(16, 140);
            lblBackgroundInfo.Name = "lblBackgroundInfo";
            lblBackgroundInfo.Size = new Size(560, 170);
            lblBackgroundInfo.TabIndex = 8;
            lblBackgroundInfo.Text = "Background settings only affect the TV Screen (broadcast/streaming output).\r\n\r\n" +
                "Theme Background: Use prerendered background images from the selected theme.\r\n\r\n" +
                "Chroma Key: Use a solid color background for chroma keying in OBS/streaming software.\r\n" +
                "  • Default: Blue (#0000FF) - safe choice that won't conflict with game UI\r\n" +
                "  • Avoid: Green, Red, Yellow, Orange, Cyan (used by game UI elements)\r\n" +
                "  • Recommended: Blue or Magenta for best keying results\r\n\r\n" +
                "Note: Guest and Host screens always use black backgrounds.";
            // 
            // tabLifelines
            // 
            tabLifelines.Controls.Add(cmbLifeline4Availability);
            tabLifelines.Controls.Add(cmbLifeline4Type);
            tabLifelines.Controls.Add(lblLifeline4);
            tabLifelines.Controls.Add(cmbLifeline3Availability);
            tabLifelines.Controls.Add(cmbLifeline3Type);
            tabLifelines.Controls.Add(lblLifeline3);
            tabLifelines.Controls.Add(cmbLifeline2Availability);
            tabLifelines.Controls.Add(cmbLifeline2Type);
            tabLifelines.Controls.Add(lblLifeline2);
            tabLifelines.Controls.Add(cmbLifeline1Availability);
            tabLifelines.Controls.Add(cmbLifeline1Type);
            tabLifelines.Controls.Add(lblLifeline1);
            tabLifelines.Controls.Add(numTotalLifelines);
            tabLifelines.Controls.Add(lblTotalLifelines);
            tabLifelines.Location = new Point(4, 24);
            tabLifelines.Name = "tabLifelines";
            tabLifelines.Padding = new Padding(3);
            tabLifelines.Size = new Size(652, 518);
            tabLifelines.TabIndex = 1;
            tabLifelines.Text = "Lifelines";
            tabLifelines.UseVisualStyleBackColor = true;
            // 
            // lblLifeline1
            // 
            lblLifeline1.AutoSize = true;
            lblLifeline1.Location = new Point(16, 66);
            lblLifeline1.Name = "lblLifeline1";
            lblLifeline1.Size = new Size(68, 15);
            lblLifeline1.TabIndex = 2;
            lblLifeline1.Text = "Lifeline 1:";
            // 
            // cmbLifeline1Type
            // 
            cmbLifeline1Type.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLifeline1Type.FormattingEnabled = true;
            cmbLifeline1Type.Items.AddRange(new object[] { "50:50", "Phone a Friend", "Ask the Audience", "Switch the Question", "Ask the Host", "Double Dip" });
            cmbLifeline1Type.Location = new Point(100, 63);
            cmbLifeline1Type.Name = "cmbLifeline1Type";
            cmbLifeline1Type.Size = new Size(250, 23);
            cmbLifeline1Type.TabIndex = 3;
            cmbLifeline1Type.SelectedIndexChanged += Control_Changed;
            // 
            // cmbLifeline1Availability
            // 
            cmbLifeline1Availability.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLifeline1Availability.FormattingEnabled = true;
            cmbLifeline1Availability.Items.AddRange(new object[] { "Always Available", "After Question 5", "After Question 10", "In Risk Mode Only" });
            cmbLifeline1Availability.Location = new Point(360, 63);
            cmbLifeline1Availability.Name = "cmbLifeline1Availability";
            cmbLifeline1Availability.Size = new Size(250, 23);
            cmbLifeline1Availability.TabIndex = 4;
            cmbLifeline1Availability.SelectedIndex = 0;
            cmbLifeline1Availability.SelectedIndexChanged += Control_Changed;
            // 
            // lblLifeline2
            // 
            lblLifeline2.AutoSize = true;
            lblLifeline2.Location = new Point(16, 101);
            lblLifeline2.Name = "lblLifeline2";
            lblLifeline2.Size = new Size(68, 15);
            lblLifeline2.TabIndex = 5;
            lblLifeline2.Text = "Lifeline 2:";
            // 
            // cmbLifeline2Type
            // 
            cmbLifeline2Type.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLifeline2Type.FormattingEnabled = true;
            cmbLifeline2Type.Items.AddRange(new object[] { "50:50", "Phone a Friend", "Ask the Audience", "Switch the Question", "Ask the Host", "Double Dip" });
            cmbLifeline2Type.Location = new Point(100, 98);
            cmbLifeline2Type.Name = "cmbLifeline2Type";
            cmbLifeline2Type.Size = new Size(250, 23);
            cmbLifeline2Type.TabIndex = 6;
            cmbLifeline2Type.SelectedIndexChanged += Control_Changed;
            // 
            // cmbLifeline2Availability
            // 
            cmbLifeline2Availability.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLifeline2Availability.FormattingEnabled = true;
            cmbLifeline2Availability.Items.AddRange(new object[] { "Always Available", "After Question 5", "After Question 10", "In Risk Mode Only" });
            cmbLifeline2Availability.Location = new Point(360, 98);
            cmbLifeline2Availability.Name = "cmbLifeline2Availability";
            cmbLifeline2Availability.Size = new Size(250, 23);
            cmbLifeline2Availability.TabIndex = 7;
            cmbLifeline2Availability.SelectedIndex = 0;
            cmbLifeline2Availability.SelectedIndexChanged += Control_Changed;
            // 
            // lblLifeline3
            // 
            lblLifeline3.AutoSize = true;
            lblLifeline3.Location = new Point(16, 136);
            lblLifeline3.Name = "lblLifeline3";
            lblLifeline3.Size = new Size(68, 15);
            lblLifeline3.TabIndex = 8;
            lblLifeline3.Text = "Lifeline 3:";
            // 
            // cmbLifeline3Type
            // 
            cmbLifeline3Type.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLifeline3Type.FormattingEnabled = true;
            cmbLifeline3Type.Items.AddRange(new object[] { "50:50", "Phone a Friend", "Ask the Audience", "Switch the Question", "Ask the Host", "Double Dip" });
            cmbLifeline3Type.Location = new Point(100, 133);
            cmbLifeline3Type.Name = "cmbLifeline3Type";
            cmbLifeline3Type.Size = new Size(250, 23);
            cmbLifeline3Type.TabIndex = 9;
            cmbLifeline3Type.SelectedIndexChanged += Control_Changed;
            // 
            // cmbLifeline3Availability
            // 
            cmbLifeline3Availability.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLifeline3Availability.FormattingEnabled = true;
            cmbLifeline3Availability.Items.AddRange(new object[] { "Always Available", "After Question 5", "After Question 10", "In Risk Mode Only" });
            cmbLifeline3Availability.Location = new Point(360, 133);
            cmbLifeline3Availability.Name = "cmbLifeline3Availability";
            cmbLifeline3Availability.Size = new Size(250, 23);
            cmbLifeline3Availability.TabIndex = 10;
            cmbLifeline3Availability.SelectedIndex = 0;
            cmbLifeline3Availability.SelectedIndexChanged += Control_Changed;
            // 
            // lblLifeline4
            // 
            lblLifeline4.AutoSize = true;
            lblLifeline4.Location = new Point(16, 171);
            lblLifeline4.Name = "lblLifeline4";
            lblLifeline4.Size = new Size(68, 15);
            lblLifeline4.TabIndex = 11;
            lblLifeline4.Text = "Lifeline 4:";
            // 
            // cmbLifeline4Type
            // 
            cmbLifeline4Type.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLifeline4Type.FormattingEnabled = true;
            cmbLifeline4Type.Items.AddRange(new object[] { "50:50", "Phone a Friend", "Ask the Audience", "Switch the Question", "Ask the Host", "Double Dip" });
            cmbLifeline4Type.Location = new Point(100, 168);
            cmbLifeline4Type.Name = "cmbLifeline4Type";
            cmbLifeline4Type.Size = new Size(250, 23);
            cmbLifeline4Type.TabIndex = 12;
            cmbLifeline4Type.SelectedIndexChanged += Control_Changed;
            // 
            // cmbLifeline4Availability
            // 
            cmbLifeline4Availability.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLifeline4Availability.FormattingEnabled = true;
            cmbLifeline4Availability.Items.AddRange(new object[] { "Always Available", "After Question 5", "After Question 10", "In Risk Mode Only" });
            cmbLifeline4Availability.Location = new Point(360, 168);
            cmbLifeline4Availability.Name = "cmbLifeline4Availability";
            cmbLifeline4Availability.Size = new Size(250, 23);
            cmbLifeline4Availability.TabIndex = 13;
            cmbLifeline4Availability.SelectedIndex = 0;
            cmbLifeline4Availability.SelectedIndexChanged += Control_Changed;
            // 
            // tabMoneyTree
            // 
            // tabMoneyTree
            // 
            tabMoneyTree.AutoScroll = true;
            tabMoneyTree.Location = new Point(4, 24);
            tabMoneyTree.Name = "tabMoneyTree";
            tabMoneyTree.Padding = new Padding(3);
            tabMoneyTree.Size = new Size(652, 520);
            tabMoneyTree.TabIndex = 3;
            tabMoneyTree.Text = "Money Tree";
            tabMoneyTree.UseVisualStyleBackColor = true;
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
            tabSounds.Controls.Add(tabControlSounds);
            tabSounds.Location = new Point(4, 24);
            tabSounds.Name = "tabSounds";
            tabSounds.Padding = new Padding(3);
            tabSounds.Size = new Size(652, 459);
            tabSounds.TabIndex = 2;
            tabSounds.Text = "Sounds";
            tabSounds.UseVisualStyleBackColor = true;
            // 
            // tabControlSounds
            // 
            tabControlSounds.Controls.Add(tabSoundpack);
            tabControlSounds.Controls.Add(tabAudio);
            tabControlSounds.Controls.Add(tabMixer);
            tabControlSounds.Dock = DockStyle.Fill;
            tabControlSounds.Location = new Point(3, 3);
            tabControlSounds.Name = "tabControlSounds";
            tabControlSounds.SelectedIndex = 0;
            tabControlSounds.Size = new Size(646, 432);
            tabControlSounds.TabIndex = 0;
            // 
            // tabSoundpack
            // 
            tabSoundpack.Controls.Add(btnPlaySelected);
            tabSoundpack.Controls.Add(lblSearchSounds);
            tabSoundpack.Controls.Add(txtSearchSounds);
            tabSoundpack.Controls.Add(dgvSoundPackInfo);
            tabSoundpack.Controls.Add(btnExportExample);
            tabSoundpack.Controls.Add(btnRemovePack);
            tabSoundpack.Controls.Add(btnImportPack);
            tabSoundpack.Controls.Add(cmbSoundPack);
            tabSoundpack.Controls.Add(lblSoundPack);
            tabSoundpack.Location = new Point(4, 24);
            tabSoundpack.Name = "tabSoundpack";
            tabSoundpack.Padding = new Padding(3);
            tabSoundpack.Size = new Size(638, 404);
            tabSoundpack.TabIndex = 0;
            tabSoundpack.Text = "Soundpack";
            tabSoundpack.UseVisualStyleBackColor = true;
            // 
            // tabAudio
            // 
            tabAudio.AutoScroll = true;
            tabAudio.Controls.Add(grpAudioProcessing);
            tabAudio.Controls.Add(grpCrossfade);
            tabAudio.Controls.Add(grpSilenceDetection);
            tabAudio.Location = new Point(4, 24);
            tabAudio.Name = "tabAudio";
            tabAudio.Padding = new Padding(3);
            tabAudio.Size = new Size(638, 404);
            tabAudio.TabIndex = 1;
            tabAudio.Text = "Audio Settings";
            tabAudio.UseVisualStyleBackColor = true;
            // 
            // grpSilenceDetection
            // 
            grpSilenceDetection.Controls.Add(chkEnableSilenceDetection);
            grpSilenceDetection.Controls.Add(lblSilenceThreshold);
            grpSilenceDetection.Controls.Add(trackBarSilenceThreshold);
            grpSilenceDetection.Controls.Add(lblSilenceThresholdValue);
            grpSilenceDetection.Controls.Add(lblSilenceDuration);
            grpSilenceDetection.Controls.Add(numSilenceDuration);
            grpSilenceDetection.Controls.Add(lblInitialDelay);
            grpSilenceDetection.Controls.Add(numInitialDelay);
            grpSilenceDetection.Controls.Add(lblFadeoutDuration);
            grpSilenceDetection.Controls.Add(numFadeoutDuration);
            grpSilenceDetection.Location = new Point(16, 16);
            grpSilenceDetection.Name = "grpSilenceDetection";
            grpSilenceDetection.Size = new Size(600, 180);
            grpSilenceDetection.TabIndex = 0;
            grpSilenceDetection.TabStop = false;
            grpSilenceDetection.Text = "Silence Detection";
            // 
            // chkEnableSilenceDetection
            // 
            chkEnableSilenceDetection.AutoSize = true;
            chkEnableSilenceDetection.Location = new Point(16, 25);
            chkEnableSilenceDetection.Name = "chkEnableSilenceDetection";
            chkEnableSilenceDetection.Size = new Size(189, 19);
            chkEnableSilenceDetection.TabIndex = 0;
            chkEnableSilenceDetection.Text = "Enable Silence Detection";
            chkEnableSilenceDetection.UseVisualStyleBackColor = true;
            chkEnableSilenceDetection.CheckedChanged += chkEnableSilenceDetection_CheckedChanged;
            // 
            // lblSilenceThreshold
            // 
            lblSilenceThreshold.AutoSize = true;
            lblSilenceThreshold.Location = new Point(16, 55);
            lblSilenceThreshold.Name = "lblSilenceThreshold";
            lblSilenceThreshold.Size = new Size(95, 15);
            lblSilenceThreshold.TabIndex = 1;
            lblSilenceThreshold.Text = "Threshold (dB):";
            // 
            // trackBarSilenceThreshold
            // 
            trackBarSilenceThreshold.Location = new Point(120, 50);
            trackBarSilenceThreshold.Maximum = -20;
            trackBarSilenceThreshold.Minimum = -60;
            trackBarSilenceThreshold.Name = "trackBarSilenceThreshold";
            trackBarSilenceThreshold.Size = new Size(400, 45);
            trackBarSilenceThreshold.TabIndex = 2;
            trackBarSilenceThreshold.TickFrequency = 5;
            trackBarSilenceThreshold.Value = -40;
            trackBarSilenceThreshold.ValueChanged += trackBarSilenceThreshold_ValueChanged;
            // 
            // lblSilenceThresholdValue
            // 
            lblSilenceThresholdValue.Location = new Point(525, 55);
            lblSilenceThresholdValue.Name = "lblSilenceThresholdValue";
            lblSilenceThresholdValue.Size = new Size(60, 15);
            lblSilenceThresholdValue.TabIndex = 3;
            lblSilenceThresholdValue.Text = "-40 dB";
            lblSilenceThresholdValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblSilenceDuration
            // 
            lblSilenceDuration.AutoSize = true;
            lblSilenceDuration.Location = new Point(16, 100);
            lblSilenceDuration.Name = "lblSilenceDuration";
            lblSilenceDuration.Size = new Size(137, 15);
            lblSilenceDuration.TabIndex = 4;
            lblSilenceDuration.Text = "Silence Duration (ms):";
            // 
            // numSilenceDuration
            // 
            numSilenceDuration.Increment = new decimal(new int[] { 50, 0, 0, 0 });
            numSilenceDuration.Location = new Point(160, 98);
            numSilenceDuration.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numSilenceDuration.Minimum = new decimal(new int[] { 100, 0, 0, 0 });
            numSilenceDuration.Name = "numSilenceDuration";
            numSilenceDuration.Size = new Size(80, 23);
            numSilenceDuration.TabIndex = 5;
            numSilenceDuration.Value = new decimal(new int[] { 250, 0, 0, 0 });
            numSilenceDuration.ValueChanged += Control_Changed;
            // 
            // lblInitialDelay
            // 
            lblInitialDelay.AutoSize = true;
            lblInitialDelay.Location = new Point(260, 100);
            lblInitialDelay.Name = "lblInitialDelay";
            lblInitialDelay.Size = new Size(110, 15);
            lblInitialDelay.TabIndex = 6;
            lblInitialDelay.Text = "Initial Delay (ms):";
            // 
            // numInitialDelay
            // 
            numInitialDelay.Increment = new decimal(new int[] { 100, 0, 0, 0 });
            numInitialDelay.Location = new Point(375, 98);
            numInitialDelay.Maximum = new decimal(new int[] { 5000, 0, 0, 0 });
            numInitialDelay.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numInitialDelay.Name = "numInitialDelay";
            numInitialDelay.Size = new Size(80, 23);
            numInitialDelay.TabIndex = 7;
            numInitialDelay.Value = new decimal(new int[] { 2500, 0, 0, 0 });
            numInitialDelay.ValueChanged += Control_Changed;
            // 
            // lblFadeoutDuration
            // 
            lblFadeoutDuration.AutoSize = true;
            lblFadeoutDuration.Location = new Point(16, 135);
            lblFadeoutDuration.Name = "lblFadeoutDuration";
            lblFadeoutDuration.Size = new Size(138, 15);
            lblFadeoutDuration.TabIndex = 8;
            lblFadeoutDuration.Text = "Fadeout Duration (ms):";
            // 
            // numFadeoutDuration
            // 
            numFadeoutDuration.Increment = new decimal(new int[] { 5, 0, 0, 0 });
            numFadeoutDuration.Location = new Point(160, 133);
            numFadeoutDuration.Maximum = new decimal(new int[] { 200, 0, 0, 0 });
            numFadeoutDuration.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            numFadeoutDuration.Name = "numFadeoutDuration";
            numFadeoutDuration.Size = new Size(80, 23);
            numFadeoutDuration.TabIndex = 9;
            numFadeoutDuration.Value = new decimal(new int[] { 50, 0, 0, 0 });
            numFadeoutDuration.ValueChanged += Control_Changed;
            // 
            // grpCrossfade
            // 
            grpCrossfade.Controls.Add(chkEnableCrossfade);
            grpCrossfade.Controls.Add(lblCrossfadeDuration);
            grpCrossfade.Controls.Add(numCrossfadeDuration);
            grpCrossfade.Location = new Point(16, 210);
            grpCrossfade.Name = "grpCrossfade";
            grpCrossfade.Size = new Size(600, 90);
            grpCrossfade.TabIndex = 1;
            grpCrossfade.TabStop = false;
            grpCrossfade.Text = "Crossfade Settings";
            // 
            // chkEnableCrossfade
            // 
            chkEnableCrossfade.AutoSize = true;
            chkEnableCrossfade.Location = new Point(16, 25);
            chkEnableCrossfade.Name = "chkEnableCrossfade";
            chkEnableCrossfade.Size = new Size(129, 19);
            chkEnableCrossfade.TabIndex = 0;
            chkEnableCrossfade.Text = "Enable Crossfade";
            chkEnableCrossfade.UseVisualStyleBackColor = true;
            chkEnableCrossfade.CheckedChanged += chkEnableCrossfade_CheckedChanged;
            // 
            // lblCrossfadeDuration
            // 
            lblCrossfadeDuration.AutoSize = true;
            lblCrossfadeDuration.Location = new Point(16, 55);
            lblCrossfadeDuration.Name = "lblCrossfadeDuration";
            lblCrossfadeDuration.Size = new Size(147, 15);
            lblCrossfadeDuration.TabIndex = 1;
            lblCrossfadeDuration.Text = "Crossfade Duration (ms):";
            // 
            // numCrossfadeDuration
            // 
            numCrossfadeDuration.Increment = new decimal(new int[] { 10, 0, 0, 0 });
            numCrossfadeDuration.Location = new Point(170, 53);
            numCrossfadeDuration.Maximum = new decimal(new int[] { 200, 0, 0, 0 });
            numCrossfadeDuration.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            numCrossfadeDuration.Name = "numCrossfadeDuration";
            numCrossfadeDuration.Size = new Size(80, 23);
            numCrossfadeDuration.TabIndex = 2;
            numCrossfadeDuration.Value = new decimal(new int[] { 50, 0, 0, 0 });
            numCrossfadeDuration.ValueChanged += Control_Changed;
            // 
            // grpAudioProcessing
            // 
            grpAudioProcessing.Controls.Add(lblMasterGain);
            grpAudioProcessing.Controls.Add(trackBarMasterGain);
            grpAudioProcessing.Controls.Add(lblMasterGainValue);
            grpAudioProcessing.Controls.Add(lblEffectsGain);
            grpAudioProcessing.Controls.Add(trackBarEffectsGain);
            grpAudioProcessing.Controls.Add(lblEffectsGainValue);
            grpAudioProcessing.Controls.Add(lblMusicGain);
            grpAudioProcessing.Controls.Add(trackBarMusicGain);
            grpAudioProcessing.Controls.Add(lblMusicGainValue);
            grpAudioProcessing.Controls.Add(chkEnableLimiter);
            grpAudioProcessing.Location = new Point(16, 310);
            grpAudioProcessing.Name = "grpAudioProcessing";
            grpAudioProcessing.Size = new Size(600, 220);
            grpAudioProcessing.TabIndex = 2;
            grpAudioProcessing.TabStop = false;
            grpAudioProcessing.Text = "Audio Processing";
            // 
            // lblMasterGain
            // 
            lblMasterGain.AutoSize = true;
            lblMasterGain.Location = new Point(16, 30);
            lblMasterGain.Name = "lblMasterGain";
            lblMasterGain.Size = new Size(96, 15);
            lblMasterGain.TabIndex = 0;
            lblMasterGain.Text = "Master Gain (dB):";
            // 
            // trackBarMasterGain
            // 
            trackBarMasterGain.Location = new Point(120, 25);
            trackBarMasterGain.Maximum = 20;
            trackBarMasterGain.Minimum = -20;
            trackBarMasterGain.Name = "trackBarMasterGain";
            trackBarMasterGain.Size = new Size(400, 45);
            trackBarMasterGain.TabIndex = 1;
            trackBarMasterGain.TickFrequency = 5;
            trackBarMasterGain.Value = 0;
            trackBarMasterGain.ValueChanged += trackBarGain_ValueChanged;
            // 
            // lblMasterGainValue
            // 
            lblMasterGainValue.Location = new Point(525, 30);
            lblMasterGainValue.Name = "lblMasterGainValue";
            lblMasterGainValue.Size = new Size(60, 15);
            lblMasterGainValue.TabIndex = 2;
            lblMasterGainValue.Text = "0 dB";
            lblMasterGainValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblEffectsGain
            // 
            lblEffectsGain.AutoSize = true;
            lblEffectsGain.Location = new Point(16, 85);
            lblEffectsGain.Name = "lblEffectsGain";
            lblEffectsGain.Size = new Size(97, 15);
            lblEffectsGain.TabIndex = 3;
            lblEffectsGain.Text = "Effects Gain (dB):";
            // 
            // trackBarEffectsGain
            // 
            trackBarEffectsGain.Location = new Point(120, 80);
            trackBarEffectsGain.Maximum = 20;
            trackBarEffectsGain.Minimum = -20;
            trackBarEffectsGain.Name = "trackBarEffectsGain";
            trackBarEffectsGain.Size = new Size(400, 45);
            trackBarEffectsGain.TabIndex = 4;
            trackBarEffectsGain.TickFrequency = 5;
            trackBarEffectsGain.Value = 0;
            trackBarEffectsGain.ValueChanged += trackBarGain_ValueChanged;
            // 
            // lblEffectsGainValue
            // 
            lblEffectsGainValue.Location = new Point(525, 85);
            lblEffectsGainValue.Name = "lblEffectsGainValue";
            lblEffectsGainValue.Size = new Size(60, 15);
            lblEffectsGainValue.TabIndex = 5;
            lblEffectsGainValue.Text = "0 dB";
            lblEffectsGainValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblMusicGain
            // 
            lblMusicGain.AutoSize = true;
            lblMusicGain.Location = new Point(16, 140);
            lblMusicGain.Name = "lblMusicGain";
            lblMusicGain.Size = new Size(92, 15);
            lblMusicGain.TabIndex = 6;
            lblMusicGain.Text = "Music Gain (dB):";
            // 
            // trackBarMusicGain
            // 
            trackBarMusicGain.Location = new Point(120, 135);
            trackBarMusicGain.Maximum = 20;
            trackBarMusicGain.Minimum = -20;
            trackBarMusicGain.Name = "trackBarMusicGain";
            trackBarMusicGain.Size = new Size(400, 45);
            trackBarMusicGain.TabIndex = 7;
            trackBarMusicGain.TickFrequency = 5;
            trackBarMusicGain.Value = 0;
            trackBarMusicGain.ValueChanged += trackBarGain_ValueChanged;
            // 
            // lblMusicGainValue
            // 
            lblMusicGainValue.Location = new Point(525, 140);
            lblMusicGainValue.Name = "lblMusicGainValue";
            lblMusicGainValue.Size = new Size(60, 15);
            lblMusicGainValue.TabIndex = 8;
            lblMusicGainValue.Text = "0 dB";
            lblMusicGainValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // chkEnableLimiter
            // 
            chkEnableLimiter.AutoSize = true;
            chkEnableLimiter.Location = new Point(16, 185);
            chkEnableLimiter.Name = "chkEnableLimiter";
            chkEnableLimiter.Size = new Size(106, 19);
            chkEnableLimiter.TabIndex = 9;
            chkEnableLimiter.Text = "Enable Limiter";
            chkEnableLimiter.UseVisualStyleBackColor = true;
            chkEnableLimiter.CheckedChanged += Control_Changed;
            // 
            // tabMixer
            // 
            tabMixer.Controls.Add(grpAudioDevice);
            tabMixer.Controls.Add(lblMixerInfo);
            tabMixer.Location = new Point(4, 24);
            tabMixer.Name = "tabMixer";
            tabMixer.Padding = new Padding(3);
            tabMixer.Size = new Size(638, 404);
            tabMixer.TabIndex = 1;
            tabMixer.Text = "Mixer";
            tabMixer.UseVisualStyleBackColor = true;
            // 
            // grpAudioDevice
            // 
            grpAudioDevice.Controls.Add(btnRefreshDevices);
            grpAudioDevice.Controls.Add(lblAudioDevice);
            grpAudioDevice.Controls.Add(cmbAudioDevice);
            grpAudioDevice.Location = new Point(16, 60);
            grpAudioDevice.Name = "grpAudioDevice";
            grpAudioDevice.Size = new Size(600, 120);
            grpAudioDevice.TabIndex = 1;
            grpAudioDevice.TabStop = false;
            grpAudioDevice.Text = "Audio Output";
            // 
            // lblAudioDevice
            // 
            lblAudioDevice.AutoSize = true;
            lblAudioDevice.Location = new Point(16, 30);
            lblAudioDevice.Name = "lblAudioDevice";
            lblAudioDevice.Size = new Size(87, 15);
            lblAudioDevice.TabIndex = 0;
            lblAudioDevice.Text = "Output Device:";
            // 
            // cmbAudioDevice
            // 
            cmbAudioDevice.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbAudioDevice.FormattingEnabled = true;
            cmbAudioDevice.Location = new Point(16, 50);
            cmbAudioDevice.Name = "cmbAudioDevice";
            cmbAudioDevice.Size = new Size(450, 23);
            cmbAudioDevice.TabIndex = 1;
            // 
            // btnRefreshDevices
            // 
            btnRefreshDevices.Location = new Point(475, 49);
            btnRefreshDevices.Name = "btnRefreshDevices";
            btnRefreshDevices.Size = new Size(105, 25);
            btnRefreshDevices.TabIndex = 2;
            btnRefreshDevices.Text = "Refresh Devices";
            btnRefreshDevices.UseVisualStyleBackColor = true;
            // 
            // lblMixerInfo
            // 
            lblMixerInfo.AutoSize = true;
            lblMixerInfo.Location = new Point(16, 16);
            lblMixerInfo.Name = "lblMixerInfo";
            lblMixerInfo.Size = new Size(550, 30);
            lblMixerInfo.TabIndex = 0;
            lblMixerInfo.Text = "Configure audio output routing. Select the device where you want game sounds to play.\r\nChanges take effect immediately.";
            // 
            // lblSoundPack
            // 
            lblSoundPack.AutoSize = true;
            lblSoundPack.Location = new Point(16, 16);
            lblSoundPack.Name = "lblSoundPack";
            lblSoundPack.Size = new Size(70, 15);
            lblSoundPack.TabIndex = 0;
            lblSoundPack.Text = "Sound Pack:";
            // 
            // cmbSoundPack
            // 
            cmbSoundPack.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSoundPack.FormattingEnabled = true;
            cmbSoundPack.Location = new Point(90, 13);
            cmbSoundPack.Name = "cmbSoundPack";
            cmbSoundPack.Size = new Size(200, 23);
            cmbSoundPack.TabIndex = 1;
            cmbSoundPack.SelectedIndexChanged += cmbSoundPack_SelectedIndexChanged;
            // 
            // btnImportPack
            // 
            btnImportPack.Location = new Point(300, 12);
            btnImportPack.Name = "btnImportPack";
            btnImportPack.Size = new Size(90, 25);
            btnImportPack.TabIndex = 2;
            btnImportPack.Text = "Import...";
            btnImportPack.UseVisualStyleBackColor = true;
            btnImportPack.Click += btnImportPack_Click;
            // 
            // btnRemovePack
            // 
            btnRemovePack.Location = new Point(400, 12);
            btnRemovePack.Name = "btnRemovePack";
            btnRemovePack.Size = new Size(90, 25);
            btnRemovePack.TabIndex = 3;
            btnRemovePack.Text = "Remove";
            btnRemovePack.UseVisualStyleBackColor = true;
            btnRemovePack.Click += btnRemovePack_Click;
            // 
            // btnExportExample
            // 
            btnExportExample.Location = new Point(500, 12);
            btnExportExample.Name = "btnExportExample";
            btnExportExample.Size = new Size(110, 25);
            btnExportExample.TabIndex = 4;
            btnExportExample.Text = "Export Example";
            btnExportExample.UseVisualStyleBackColor = true;
            btnExportExample.Click += btnExportExample_Click;
            // 
            // lblSearchSounds
            // 
            lblSearchSounds.AutoSize = true;
            lblSearchSounds.Location = new Point(16, 51);
            lblSearchSounds.Name = "lblSearchSounds";
            lblSearchSounds.Size = new Size(45, 15);
            lblSearchSounds.TabIndex = 5;
            lblSearchSounds.Text = "Search:";
            // 
            // txtSearchSounds
            // 
            txtSearchSounds.Location = new Point(70, 48);
            txtSearchSounds.Name = "txtSearchSounds";
            txtSearchSounds.PlaceholderText = "Filter by key or filename...";
            txtSearchSounds.Size = new Size(400, 23);
            txtSearchSounds.TabIndex = 6;
            txtSearchSounds.TextChanged += txtSearchSounds_TextChanged;
            // 
            // btnPlaySelected
            // 
            btnPlaySelected.Location = new Point(500, 47);
            btnPlaySelected.Name = "btnPlaySelected";
            btnPlaySelected.Size = new Size(110, 25);
            btnPlaySelected.TabIndex = 7;
            btnPlaySelected.Text = "▶ Play Selected";
            btnPlaySelected.UseVisualStyleBackColor = true;
            btnPlaySelected.Click += btnPlaySelected_Click;
            // 
            // dgvSoundPackInfo
            // 
            dgvSoundPackInfo.AllowUserToAddRows = false;
            dgvSoundPackInfo.AllowUserToDeleteRows = false;
            dgvSoundPackInfo.AllowUserToResizeRows = false;
            dgvSoundPackInfo.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvSoundPackInfo.BackgroundColor = System.Drawing.SystemColors.Window;
            dgvSoundPackInfo.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvSoundPackInfo.Location = new Point(16, 83);
            dgvSoundPackInfo.MultiSelect = false;
            dgvSoundPackInfo.Name = "dgvSoundPackInfo";
            dgvSoundPackInfo.ReadOnly = true;
            dgvSoundPackInfo.RowHeadersVisible = false;
            dgvSoundPackInfo.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvSoundPackInfo.Size = new Size(588, 390);
            dgvSoundPackInfo.TabIndex = 8;
            // 
            // tabStreamDeck
            // 
            tabStreamDeck.AutoScroll = true;
            tabStreamDeck.Controls.Add(grpStreamDeckConfig);
            tabStreamDeck.Location = new Point(4, 24);
            tabStreamDeck.Name = "tabStreamDeck";
            tabStreamDeck.Padding = new Padding(3);
            tabStreamDeck.Size = new Size(652, 438);
            tabStreamDeck.TabIndex = 8;
            tabStreamDeck.Text = "Stream Deck";
            tabStreamDeck.UseVisualStyleBackColor = true;
            // 
            // grpStreamDeckConfig
            // 
            grpStreamDeckConfig.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpStreamDeckConfig.Controls.Add(chkEnableStreamDeck);
            grpStreamDeckConfig.Controls.Add(lblStreamDeckInfo);
            grpStreamDeckConfig.Controls.Add(pnlStreamDeckLayout);
            grpStreamDeckConfig.Location = new Point(6, 6);
            grpStreamDeckConfig.Name = "grpStreamDeckConfig";
            grpStreamDeckConfig.Size = new Size(640, 420);
            grpStreamDeckConfig.TabIndex = 0;
            grpStreamDeckConfig.TabStop = false;
            grpStreamDeckConfig.Text = "Stream Deck Integration";
            // 
            // chkEnableStreamDeck
            // 
            chkEnableStreamDeck.AutoSize = true;
            chkEnableStreamDeck.Location = new Point(15, 25);
            chkEnableStreamDeck.Name = "chkEnableStreamDeck";
            chkEnableStreamDeck.Size = new Size(200, 19);
            chkEnableStreamDeck.TabIndex = 0;
            chkEnableStreamDeck.Text = "Enable Stream Deck Integration";
            chkEnableStreamDeck.UseVisualStyleBackColor = true;
            chkEnableStreamDeck.CheckedChanged += (s, e) => _hasChanges = true;
            // 
            // lblStreamDeckInfo
            // 
            lblStreamDeckInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblStreamDeckInfo.Location = new Point(15, 50);
            lblStreamDeckInfo.Name = "lblStreamDeckInfo";
            lblStreamDeckInfo.Size = new Size(610, 60);
            lblStreamDeckInfo.TabIndex = 1;
            lblStreamDeckInfo.Text = "Stream Deck provides physical button control for the host to lock in contestant answers and reveal results.\n\n" +
                "Button Layout (6-button Stream Deck):";
            // 
            // pnlStreamDeckLayout
            // 
            pnlStreamDeckLayout.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            pnlStreamDeckLayout.BorderStyle = BorderStyle.FixedSingle;
            pnlStreamDeckLayout.Controls.Add(lblDynamic);
            pnlStreamDeckLayout.Controls.Add(lblAnswerA);
            pnlStreamDeckLayout.Controls.Add(lblAnswerB);
            pnlStreamDeckLayout.Controls.Add(lblReveal);
            pnlStreamDeckLayout.Controls.Add(lblAnswerC);
            pnlStreamDeckLayout.Controls.Add(lblAnswerD);
            pnlStreamDeckLayout.Controls.Add(picDynamic);
            pnlStreamDeckLayout.Controls.Add(picAnswerA);
            pnlStreamDeckLayout.Controls.Add(picAnswerB);
            pnlStreamDeckLayout.Controls.Add(picReveal);
            pnlStreamDeckLayout.Controls.Add(picAnswerC);
            pnlStreamDeckLayout.Controls.Add(picAnswerD);
            pnlStreamDeckLayout.Location = new Point(15, 115);
            pnlStreamDeckLayout.Name = "pnlStreamDeckLayout";
            pnlStreamDeckLayout.Size = new Size(280, 240);
            pnlStreamDeckLayout.TabIndex = 2;
            // 
            // Row 1: Dynamic, A, B
            // picDynamic
            // 
            picDynamic.Location = new Point(10, 10);
            picDynamic.Name = "picDynamic";
            picDynamic.Size = new Size(72, 72);
            picDynamic.SizeMode = PictureBoxSizeMode.StretchImage;
            picDynamic.TabIndex = 0;
            picDynamic.TabStop = false;
            // 
            // lblDynamic
            // 
            lblDynamic.Location = new Point(10, 85);
            lblDynamic.Name = "lblDynamic";
            lblDynamic.Size = new Size(72, 30);
            lblDynamic.TabIndex = 1;
            lblDynamic.Text = "Feedback";
            lblDynamic.TextAlign = ContentAlignment.TopCenter;
            // 
            // picAnswerA
            // 
            picAnswerA.Location = new Point(94, 10);
            picAnswerA.Name = "picAnswerA";
            picAnswerA.Size = new Size(72, 72);
            picAnswerA.SizeMode = PictureBoxSizeMode.StretchImage;
            picAnswerA.TabIndex = 2;
            picAnswerA.TabStop = false;
            // 
            // lblAnswerA
            // 
            lblAnswerA.Location = new Point(94, 85);
            lblAnswerA.Name = "lblAnswerA";
            lblAnswerA.Size = new Size(72, 30);
            lblAnswerA.TabIndex = 3;
            lblAnswerA.Text = "Answer A";
            lblAnswerA.TextAlign = ContentAlignment.TopCenter;
            // 
            // picAnswerB
            // 
            picAnswerB.Location = new Point(178, 10);
            picAnswerB.Name = "picAnswerB";
            picAnswerB.Size = new Size(72, 72);
            picAnswerB.SizeMode = PictureBoxSizeMode.StretchImage;
            picAnswerB.TabIndex = 4;
            picAnswerB.TabStop = false;
            // 
            // lblAnswerB
            // 
            lblAnswerB.Location = new Point(178, 85);
            lblAnswerB.Name = "lblAnswerB";
            lblAnswerB.Size = new Size(72, 30);
            lblAnswerB.TabIndex = 5;
            lblAnswerB.Text = "Answer B";
            lblAnswerB.TextAlign = ContentAlignment.TopCenter;
            // 
            // Row 2: Reveal, C, D
            // picReveal
            // 
            picReveal.Location = new Point(10, 120);
            picReveal.Name = "picReveal";
            picReveal.Size = new Size(72, 72);
            picReveal.SizeMode = PictureBoxSizeMode.StretchImage;
            picReveal.TabIndex = 6;
            picReveal.TabStop = false;
            // 
            // lblReveal
            // 
            lblReveal.Location = new Point(10, 195);
            lblReveal.Name = "lblReveal";
            lblReveal.Size = new Size(72, 30);
            lblReveal.Name = "lblReveal";
            lblReveal.Size = new Size(72, 30);
            lblReveal.TabIndex = 7;
            lblReveal.Text = "Reveal";
            lblReveal.TextAlign = ContentAlignment.TopCenter;
            // 
            // picAnswerC
            // 
            picAnswerC.Location = new Point(94, 120);
            picAnswerC.Name = "picAnswerC";
            picAnswerC.Size = new Size(72, 72);
            picAnswerC.SizeMode = PictureBoxSizeMode.StretchImage;
            picAnswerC.TabIndex = 8;
            picAnswerC.TabStop = false;
            // 
            // lblAnswerC
            // 
            lblAnswerC.Location = new Point(94, 195);
            lblAnswerC.Name = "lblAnswerC";
            lblAnswerC.Size = new Size(72, 30);
            lblAnswerC.TabIndex = 9;
            lblAnswerC.Text = "Answer C";
            lblAnswerC.TextAlign = ContentAlignment.TopCenter;
            // 
            // picAnswerD
            // 
            picAnswerD.Location = new Point(178, 120);
            picAnswerD.Name = "picAnswerD";
            picAnswerD.Size = new Size(72, 72);
            picAnswerD.SizeMode = PictureBoxSizeMode.StretchImage;
            picAnswerD.TabIndex = 10;
            picAnswerD.TabStop = false;
            // 
            // lblAnswerD
            // 
            lblAnswerD.Location = new Point(178, 195);
            lblAnswerD.Name = "lblAnswerD";
            lblAnswerD.Size = new Size(72, 30);
            lblAnswerD.TabIndex = 11;
            lblAnswerD.Text = "Answer D";
            lblAnswerD.TextAlign = ContentAlignment.TopCenter;
            // 
            // tabAudience
            // 
            tabAudience.AutoScroll = true;
            tabAudience.Controls.Add(grpAudienceServer);
            tabAudience.Controls.Add(lblAudienceInfo);
            tabAudience.Location = new Point(4, 24);
            tabAudience.Name = "tabAudience";
            tabAudience.Padding = new Padding(3);
            tabAudience.Size = new Size(652, 438);
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
            grpAudienceServer.Size = new Size(615, 220);
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
            lblAudienceInfo.Location = new Point(16, 250);
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
            btnOK.Location = new Point(416, 570);
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
            btnCancel.Location = new Point(502, 570);
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
            ClientSize = new Size(684, 620);
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
            tabScreens.ResumeLayout(false);
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
            ((System.ComponentModel.ISupportInitialize)numTotalLifelines).EndInit();
            tabSounds.ResumeLayout(false);
            tabControlSounds.ResumeLayout(false);
            tabSoundpack.ResumeLayout(false);
            tabAudio.ResumeLayout(false);
            grpSilenceDetection.ResumeLayout(false);
            grpSilenceDetection.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarSilenceThreshold).EndInit();
            ((System.ComponentModel.ISupportInitialize)numSilenceDuration).EndInit();
            ((System.ComponentModel.ISupportInitialize)numInitialDelay).EndInit();
            ((System.ComponentModel.ISupportInitialize)numFadeoutDuration).EndInit();
            grpCrossfade.ResumeLayout(false);
            grpCrossfade.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numCrossfadeDuration).EndInit();
            grpAudioProcessing.ResumeLayout(false);
            grpAudioProcessing.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarMasterGain).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBarEffectsGain).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBarMusicGain).EndInit();
            tabMixer.ResumeLayout(false);
            tabMixer.PerformLayout();
            grpAudioDevice.ResumeLayout(false);
            grpAudioDevice.PerformLayout();
            tabStreamDeck.ResumeLayout(false);
            grpStreamDeckConfig.ResumeLayout(false);
            grpStreamDeckConfig.PerformLayout();
            pnlStreamDeckLayout.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picDynamic).EndInit();
            ((System.ComponentModel.ISupportInitialize)picAnswerA).EndInit();
            ((System.ComponentModel.ISupportInitialize)picAnswerB).EndInit();
            ((System.ComponentModel.ISupportInitialize)picReveal).EndInit();
            ((System.ComponentModel.ISupportInitialize)picAnswerC).EndInit();
            ((System.ComponentModel.ISupportInitialize)picAnswerD).EndInit();
            tabAudience.ResumeLayout(false);
            grpAudienceServer.ResumeLayout(false);
            grpAudienceServer.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabControl;
        private TabPage tabScreens;
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
        private Label lblBackgroundMode;
        private RadioButton radModePrerendered;
        private RadioButton radModeChromaKey;
        private Label lblBackground;
        private ComboBox cmbBackground;
        private Button btnSelectBackground;
        private Label lblChromaColor;
        private Button btnChromaColor;
        private Label lblChromaColorPreview;
        private Label lblBackgroundInfo;
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
        private Label lblLifeline1;
        private ComboBox cmbLifeline1Type;
        private ComboBox cmbLifeline1Availability;
        private ComboBox cmbLifeline2Type;
        private Label lblLifeline2;
        private ComboBox cmbLifeline2Availability;
        private ComboBox cmbLifeline3Type;
        private Label lblLifeline3;
        private ComboBox cmbLifeline3Availability;
        private ComboBox cmbLifeline4Type;
        private Label lblLifeline4;
        private ComboBox cmbLifeline4Availability;
        private Label lblSoundPack;
        private ComboBox cmbSoundPack;
        private Button btnImportPack;
        private Button btnRemovePack;
        private Button btnExportExample;
        private DataGridView dgvSoundPackInfo;
        private TextBox txtSearchSounds;
        private Label lblSearchSounds;
        private Button btnPlaySelected;
        private TabControl tabControlSounds;
        private TabPage tabSoundpack;
        private TabPage tabAudio;
        private TabPage tabMixer;
        private GroupBox grpAudioDevice;
        private GroupBox grpSilenceDetection;
        private TrackBar trackBarSilenceThreshold;
        private Label lblSilenceThreshold;
        private Label lblSilenceThresholdValue;
        private NumericUpDown numSilenceDuration;
        private Label lblSilenceDuration;
        private NumericUpDown numInitialDelay;
        private Label lblInitialDelay;
        private NumericUpDown numFadeoutDuration;
        private Label lblFadeoutDuration;
        private CheckBox chkEnableSilenceDetection;
        private GroupBox grpCrossfade;
        private NumericUpDown numCrossfadeDuration;
        private Label lblCrossfadeDuration;
        private CheckBox chkEnableCrossfade;
        private GroupBox grpAudioProcessing;
        private TrackBar trackBarMasterGain;
        private Label lblMasterGain;
        private Label lblMasterGainValue;
        private TrackBar trackBarEffectsGain;
        private Label lblEffectsGain;
        private Label lblEffectsGainValue;
        private TrackBar trackBarMusicGain;
        private Label lblMusicGain;
        private Label lblMusicGainValue;
        private CheckBox chkEnableLimiter;
        private Label lblAudioDevice;
        private ComboBox cmbAudioDevice;
        private Button btnRefreshDevices;
        private Label lblMixerInfo;
        private GroupBox grpConsole;
        private Button btnOpenGameConsole;
        private Button btnOpenWebServerConsole;
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
        private TabPage tabStreamDeck;
        private GroupBox grpStreamDeckConfig;
        private CheckBox chkEnableStreamDeck;
        private Label lblStreamDeckInfo;
        private Panel pnlStreamDeckLayout;
        private PictureBox picDynamic;
        private PictureBox picAnswerA;
        private PictureBox picAnswerB;
        private PictureBox picReveal;
        private PictureBox picAnswerC;
        private PictureBox picAnswerD;
        private Label lblDynamic;
        private Label lblAnswerA;
        private Label lblAnswerB;
        private Label lblReveal;
        private Label lblAnswerC;
        private Label lblAnswerD;
        private GroupBox grpNumberFormat;
        private Label lblThousandsSeparator;
        private RadioButton radSeparatorComma;
        private RadioButton radSeparatorPeriod;
        private RadioButton radSeparatorSpace;
        private RadioButton radSeparatorNone;
    }
}



