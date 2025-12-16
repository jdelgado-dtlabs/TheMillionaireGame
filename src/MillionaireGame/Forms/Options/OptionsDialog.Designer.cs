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
            grpScreens = new GroupBox();
            chkFullScreenTVScreen = new CheckBox();
            chkFullScreenGuestScreen = new CheckBox();
            chkFullScreenHostScreen = new CheckBox();
            chkAutoShowTVScreen = new CheckBox();
            chkAutoShowGuestScreen = new CheckBox();
            chkAutoShowHostScreen = new CheckBox();
            grpGameplay = new GroupBox();
            chkHideAnswersOnNewQuestion = new CheckBox();
            chkAutoHideQuestionAtWalkAway = new CheckBox();
            chkAutoShowTotalWinnings = new CheckBox();
            chkShowAnswerOnlyAtFinal = new CheckBox();
            tabLifelines = new TabPage();
            grpLifeline4 = new GroupBox();
            radL4RiskMode = new RadioButton();
            radL4AfterQ10 = new RadioButton();
            radL4AfterQ5 = new RadioButton();
            radL4Always = new RadioButton();
            cmbLifeline4Type = new ComboBox();
            lblLifeline4 = new Label();
            grpLifeline3 = new GroupBox();
            radL3RiskMode = new RadioButton();
            radL3AfterQ10 = new RadioButton();
            radL3AfterQ5 = new RadioButton();
            radL3Always = new RadioButton();
            cmbLifeline3Type = new ComboBox();
            lblLifeline3 = new Label();
            grpLifeline2 = new GroupBox();
            radL2RiskMode = new RadioButton();
            radL2AfterQ10 = new RadioButton();
            radL2AfterQ5 = new RadioButton();
            radL2Always = new RadioButton();
            cmbLifeline2Type = new ComboBox();
            lblLifeline2 = new Label();
            grpLifeline1 = new GroupBox();
            radL1RiskMode = new RadioButton();
            radL1AfterQ10 = new RadioButton();
            radL1AfterQ5 = new RadioButton();
            radL1Always = new RadioButton();
            cmbLifeline1Type = new ComboBox();
            lblLifeline1 = new Label();
            numTotalLifelines = new NumericUpDown();
            lblTotalLifelines = new Label();
            tabSounds = new TabPage();
            grpGameSounds = new GroupBox();
            btnBrowseWalkAway = new Button();
            txtSoundWalkAway = new TextBox();
            lblSoundWalkAway = new Label();
            btnBrowseCorrectAnswer = new Button();
            txtSoundCorrectAnswer = new TextBox();
            lblSoundCorrectAnswer = new Label();
            btnBrowseWrongAnswer = new Button();
            txtSoundWrongAnswer = new TextBox();
            lblSoundWrongAnswer = new Label();
            btnBrowseFinalAnswer = new Button();
            txtSoundFinalAnswer = new TextBox();
            lblSoundFinalAnswer = new Label();
            btnBrowseQuestionCue = new Button();
            txtSoundQuestionCue = new TextBox();
            lblSoundQuestionCue = new Label();
            grpLifelineSounds = new GroupBox();
            btnBrowseSwitch = new Button();
            txtSoundSwitch = new TextBox();
            lblSoundSwitch = new Label();
            btnBrowseATA = new Button();
            txtSoundATA = new TextBox();
            lblSoundATA = new Label();
            btnBrowsePhone = new Button();
            txtSoundPhone = new TextBox();
            lblSoundPhone = new Label();
            btnBrowse5050 = new Button();
            txtSound5050 = new TextBox();
            lblSound5050 = new Label();
            btnOK = new Button();
            btnCancel = new Button();
            btnApply = new Button();
            tabControl.SuspendLayout();
            tabGeneral.SuspendLayout();
            grpScreens.SuspendLayout();
            grpGameplay.SuspendLayout();
            tabLifelines.SuspendLayout();
            grpLifeline4.SuspendLayout();
            grpLifeline3.SuspendLayout();
            grpLifeline2.SuspendLayout();
            grpLifeline1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numTotalLifelines).BeginInit();
            tabSounds.SuspendLayout();
            grpGameSounds.SuspendLayout();
            grpLifelineSounds.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl
            // 
            tabControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl.Controls.Add(tabGeneral);
            tabControl.Controls.Add(tabLifelines);
            tabControl.Controls.Add(tabSounds);
            tabControl.Location = new Point(12, 12);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(660, 487);
            tabControl.TabIndex = 0;
            // 
            // tabGeneral
            // 
            tabGeneral.Controls.Add(grpScreens);
            tabGeneral.Controls.Add(grpGameplay);
            tabGeneral.Location = new Point(4, 24);
            tabGeneral.Name = "tabGeneral";
            tabGeneral.Padding = new Padding(3);
            tabGeneral.Size = new Size(652, 459);
            tabGeneral.TabIndex = 0;
            tabGeneral.Text = "General";
            tabGeneral.UseVisualStyleBackColor = true;
            // 
            // grpScreens
            // 
            grpScreens.Controls.Add(chkFullScreenTVScreen);
            grpScreens.Controls.Add(chkFullScreenGuestScreen);
            grpScreens.Controls.Add(chkFullScreenHostScreen);
            grpScreens.Controls.Add(chkAutoShowTVScreen);
            grpScreens.Controls.Add(chkAutoShowGuestScreen);
            grpScreens.Controls.Add(chkAutoShowHostScreen);
            grpScreens.Location = new Point(16, 16);
            grpScreens.Name = "grpScreens";
            grpScreens.Size = new Size(300, 200);
            grpScreens.TabIndex = 0;
            grpScreens.TabStop = false;
            grpScreens.Text = "Screen Settings";
            // 
            // chkFullScreenTVScreen
            // 
            chkFullScreenTVScreen.AutoSize = true;
            chkFullScreenTVScreen.Location = new Point(20, 160);
            chkFullScreenTVScreen.Name = "chkFullScreenTVScreen";
            chkFullScreenTVScreen.Size = new Size(147, 19);
            chkFullScreenTVScreen.TabIndex = 5;
            chkFullScreenTVScreen.Text = "Full Screen TV Screen";
            chkFullScreenTVScreen.UseVisualStyleBackColor = true;
            chkFullScreenTVScreen.CheckedChanged += Control_Changed;
            // 
            // chkFullScreenGuestScreen
            // 
            chkFullScreenGuestScreen.AutoSize = true;
            chkFullScreenGuestScreen.Location = new Point(20, 130);
            chkFullScreenGuestScreen.Name = "chkFullScreenGuestScreen";
            chkFullScreenGuestScreen.Size = new Size(170, 19);
            chkFullScreenGuestScreen.TabIndex = 4;
            chkFullScreenGuestScreen.Text = "Full Screen Guest Screen";
            chkFullScreenGuestScreen.UseVisualStyleBackColor = true;
            chkFullScreenGuestScreen.CheckedChanged += Control_Changed;
            // 
            // chkFullScreenHostScreen
            // 
            chkFullScreenHostScreen.AutoSize = true;
            chkFullScreenHostScreen.Location = new Point(20, 100);
            chkFullScreenHostScreen.Name = "chkFullScreenHostScreen";
            chkFullScreenHostScreen.Size = new Size(163, 19);
            chkFullScreenHostScreen.TabIndex = 3;
            chkFullScreenHostScreen.Text = "Full Screen Host Screen";
            chkFullScreenHostScreen.UseVisualStyleBackColor = true;
            chkFullScreenHostScreen.CheckedChanged += Control_Changed;
            // 
            // chkAutoShowTVScreen
            // 
            chkAutoShowTVScreen.AutoSize = true;
            chkAutoShowTVScreen.Location = new Point(20, 70);
            chkAutoShowTVScreen.Name = "chkAutoShowTVScreen";
            chkAutoShowTVScreen.Size = new Size(142, 19);
            chkAutoShowTVScreen.TabIndex = 2;
            chkAutoShowTVScreen.Text = "Auto Show TV Screen";
            chkAutoShowTVScreen.UseVisualStyleBackColor = true;
            chkAutoShowTVScreen.CheckedChanged += Control_Changed;
            // 
            // chkAutoShowGuestScreen
            // 
            chkAutoShowGuestScreen.AutoSize = true;
            chkAutoShowGuestScreen.Location = new Point(20, 45);
            chkAutoShowGuestScreen.Name = "chkAutoShowGuestScreen";
            chkAutoShowGuestScreen.Size = new Size(165, 19);
            chkAutoShowGuestScreen.TabIndex = 1;
            chkAutoShowGuestScreen.Text = "Auto Show Guest Screen";
            chkAutoShowGuestScreen.UseVisualStyleBackColor = true;
            chkAutoShowGuestScreen.CheckedChanged += Control_Changed;
            // 
            // chkAutoShowHostScreen
            // 
            chkAutoShowHostScreen.AutoSize = true;
            chkAutoShowHostScreen.Location = new Point(20, 20);
            chkAutoShowHostScreen.Name = "chkAutoShowHostScreen";
            chkAutoShowHostScreen.Size = new Size(158, 19);
            chkAutoShowHostScreen.TabIndex = 0;
            chkAutoShowHostScreen.Text = "Auto Show Host Screen";
            chkAutoShowHostScreen.UseVisualStyleBackColor = true;
            chkAutoShowHostScreen.CheckedChanged += Control_Changed;
            // 
            // grpGameplay
            // 
            grpGameplay.Controls.Add(chkHideAnswersOnNewQuestion);
            grpGameplay.Controls.Add(chkAutoHideQuestionAtWalkAway);
            grpGameplay.Controls.Add(chkAutoShowTotalWinnings);
            grpGameplay.Controls.Add(chkShowAnswerOnlyAtFinal);
            grpGameplay.Location = new Point(336, 16);
            grpGameplay.Name = "grpGameplay";
            grpGameplay.Size = new Size(300, 200);
            grpGameplay.TabIndex = 1;
            grpGameplay.TabStop = false;
            grpGameplay.Text = "Gameplay Settings";
            // 
            // chkHideAnswersOnNewQuestion
            // 
            chkHideAnswersOnNewQuestion.AutoSize = true;
            chkHideAnswersOnNewQuestion.Location = new Point(20, 100);
            chkHideAnswersOnNewQuestion.Name = "chkHideAnswersOnNewQuestion";
            chkHideAnswersOnNewQuestion.Size = new Size(201, 19);
            chkHideAnswersOnNewQuestion.TabIndex = 3;
            chkHideAnswersOnNewQuestion.Text = "Hide Answers on New Question";
            chkHideAnswersOnNewQuestion.UseVisualStyleBackColor = true;
            chkHideAnswersOnNewQuestion.CheckedChanged += Control_Changed;
            // 
            // chkAutoHideQuestionAtWalkAway
            // 
            chkAutoHideQuestionAtWalkAway.AutoSize = true;
            chkAutoHideQuestionAtWalkAway.Location = new Point(20, 70);
            chkAutoHideQuestionAtWalkAway.Name = "chkAutoHideQuestionAtWalkAway";
            chkAutoHideQuestionAtWalkAway.Size = new Size(213, 19);
            chkAutoHideQuestionAtWalkAway.TabIndex = 2;
            chkAutoHideQuestionAtWalkAway.Text = "Auto Hide Question at Walk Away";
            chkAutoHideQuestionAtWalkAway.UseVisualStyleBackColor = true;
            chkAutoHideQuestionAtWalkAway.CheckedChanged += Control_Changed;
            // 
            // chkAutoShowTotalWinnings
            // 
            chkAutoShowTotalWinnings.AutoSize = true;
            chkAutoShowTotalWinnings.Location = new Point(20, 45);
            chkAutoShowTotalWinnings.Name = "chkAutoShowTotalWinnings";
            chkAutoShowTotalWinnings.Size = new Size(172, 19);
            chkAutoShowTotalWinnings.TabIndex = 1;
            chkAutoShowTotalWinnings.Text = "Auto Show Total Winnings";
            chkAutoShowTotalWinnings.UseVisualStyleBackColor = true;
            chkAutoShowTotalWinnings.CheckedChanged += Control_Changed;
            // 
            // chkShowAnswerOnlyAtFinal
            // 
            chkShowAnswerOnlyAtFinal.AutoSize = true;
            chkShowAnswerOnlyAtFinal.Location = new Point(20, 20);
            chkShowAnswerOnlyAtFinal.Name = "chkShowAnswerOnlyAtFinal";
            chkShowAnswerOnlyAtFinal.Size = new Size(168, 19);
            chkShowAnswerOnlyAtFinal.TabIndex = 0;
            chkShowAnswerOnlyAtFinal.Text = "Show Answer Only at Final";
            chkShowAnswerOnlyAtFinal.UseVisualStyleBackColor = true;
            chkShowAnswerOnlyAtFinal.CheckedChanged += Control_Changed;
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
            // grpLifeline4
            // 
            grpLifeline4.Controls.Add(radL4RiskMode);
            grpLifeline4.Controls.Add(radL4AfterQ10);
            grpLifeline4.Controls.Add(radL4AfterQ5);
            grpLifeline4.Controls.Add(radL4Always);
            grpLifeline4.Controls.Add(cmbLifeline4Type);
            grpLifeline4.Controls.Add(lblLifeline4);
            grpLifeline4.Location = new Point(336, 236);
            grpLifeline4.Name = "grpLifeline4";
            grpLifeline4.Size = new Size(300, 200);
            grpLifeline4.TabIndex = 5;
            grpLifeline4.TabStop = false;
            grpLifeline4.Text = "Lifeline 4";
            // 
            // radL4RiskMode
            // 
            radL4RiskMode.AutoSize = true;
            radL4RiskMode.Location = new Point(20, 140);
            radL4RiskMode.Name = "radL4RiskMode";
            radL4RiskMode.Size = new Size(128, 19);
            radL4RiskMode.TabIndex = 5;
            radL4RiskMode.Text = "In Risk Mode Only";
            radL4RiskMode.UseVisualStyleBackColor = true;
            radL4RiskMode.CheckedChanged += Control_Changed;
            // 
            // radL4AfterQ10
            // 
            radL4AfterQ10.AutoSize = true;
            radL4AfterQ10.Location = new Point(20, 115);
            radL4AfterQ10.Name = "radL4AfterQ10";
            radL4AfterQ10.Size = new Size(134, 19);
            radL4AfterQ10.TabIndex = 4;
            radL4AfterQ10.Text = "After Question 10";
            radL4AfterQ10.UseVisualStyleBackColor = true;
            radL4AfterQ10.CheckedChanged += Control_Changed;
            // 
            // radL4AfterQ5
            // 
            radL4AfterQ5.AutoSize = true;
            radL4AfterQ5.Location = new Point(20, 90);
            radL4AfterQ5.Name = "radL4AfterQ5";
            radL4AfterQ5.Size = new Size(125, 19);
            radL4AfterQ5.TabIndex = 3;
            radL4AfterQ5.Text = "After Question 5";
            radL4AfterQ5.UseVisualStyleBackColor = true;
            radL4AfterQ5.CheckedChanged += Control_Changed;
            // 
            // radL4Always
            // 
            radL4Always.AutoSize = true;
            radL4Always.Checked = true;
            radL4Always.Location = new Point(20, 65);
            radL4Always.Name = "radL4Always";
            radL4Always.Size = new Size(108, 19);
            radL4Always.TabIndex = 2;
            radL4Always.TabStop = true;
            radL4Always.Text = "Always Available";
            radL4Always.UseVisualStyleBackColor = true;
            radL4Always.CheckedChanged += Control_Changed;
            // 
            // cmbLifeline4Type
            // 
            cmbLifeline4Type.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLifeline4Type.FormattingEnabled = true;
            cmbLifeline4Type.Items.AddRange(new object[] { "50:50", "Phone a Friend", "Ask the Audience", "Switch the Question" });
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
            grpLifeline3.Controls.Add(radL3RiskMode);
            grpLifeline3.Controls.Add(radL3AfterQ10);
            grpLifeline3.Controls.Add(radL3AfterQ5);
            grpLifeline3.Controls.Add(radL3Always);
            grpLifeline3.Controls.Add(cmbLifeline3Type);
            grpLifeline3.Controls.Add(lblLifeline3);
            grpLifeline3.Location = new Point(16, 236);
            grpLifeline3.Name = "grpLifeline3";
            grpLifeline3.Size = new Size(300, 200);
            grpLifeline3.TabIndex = 4;
            grpLifeline3.TabStop = false;
            grpLifeline3.Text = "Lifeline 3";
            // 
            // radL3RiskMode
            // 
            radL3RiskMode.AutoSize = true;
            radL3RiskMode.Location = new Point(20, 140);
            radL3RiskMode.Name = "radL3RiskMode";
            radL3RiskMode.Size = new Size(128, 19);
            radL3RiskMode.TabIndex = 5;
            radL3RiskMode.Text = "In Risk Mode Only";
            radL3RiskMode.UseVisualStyleBackColor = true;
            radL3RiskMode.CheckedChanged += Control_Changed;
            // 
            // radL3AfterQ10
            // 
            radL3AfterQ10.AutoSize = true;
            radL3AfterQ10.Location = new Point(20, 115);
            radL3AfterQ10.Name = "radL3AfterQ10";
            radL3AfterQ10.Size = new Size(134, 19);
            radL3AfterQ10.TabIndex = 4;
            radL3AfterQ10.Text = "After Question 10";
            radL3AfterQ10.UseVisualStyleBackColor = true;
            radL3AfterQ10.CheckedChanged += Control_Changed;
            // 
            // radL3AfterQ5
            // 
            radL3AfterQ5.AutoSize = true;
            radL3AfterQ5.Location = new Point(20, 90);
            radL3AfterQ5.Name = "radL3AfterQ5";
            radL3AfterQ5.Size = new Size(125, 19);
            radL3AfterQ5.TabIndex = 3;
            radL3AfterQ5.Text = "After Question 5";
            radL3AfterQ5.UseVisualStyleBackColor = true;
            radL3AfterQ5.CheckedChanged += Control_Changed;
            // 
            // radL3Always
            // 
            radL3Always.AutoSize = true;
            radL3Always.Checked = true;
            radL3Always.Location = new Point(20, 65);
            radL3Always.Name = "radL3Always";
            radL3Always.Size = new Size(108, 19);
            radL3Always.TabIndex = 2;
            radL3Always.TabStop = true;
            radL3Always.Text = "Always Available";
            radL3Always.UseVisualStyleBackColor = true;
            radL3Always.CheckedChanged += Control_Changed;
            // 
            // cmbLifeline3Type
            // 
            cmbLifeline3Type.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLifeline3Type.FormattingEnabled = true;
            cmbLifeline3Type.Items.AddRange(new object[] { "50:50", "Phone a Friend", "Ask the Audience", "Switch the Question" });
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
            grpLifeline2.Controls.Add(radL2RiskMode);
            grpLifeline2.Controls.Add(radL2AfterQ10);
            grpLifeline2.Controls.Add(radL2AfterQ5);
            grpLifeline2.Controls.Add(radL2Always);
            grpLifeline2.Controls.Add(cmbLifeline2Type);
            grpLifeline2.Controls.Add(lblLifeline2);
            grpLifeline2.Location = new Point(336, 56);
            grpLifeline2.Name = "grpLifeline2";
            grpLifeline2.Size = new Size(300, 160);
            grpLifeline2.TabIndex = 3;
            grpLifeline2.TabStop = false;
            grpLifeline2.Text = "Lifeline 2";
            // 
            // radL2RiskMode
            // 
            radL2RiskMode.AutoSize = true;
            radL2RiskMode.Location = new Point(20, 130);
            radL2RiskMode.Name = "radL2RiskMode";
            radL2RiskMode.Size = new Size(128, 19);
            radL2RiskMode.TabIndex = 5;
            radL2RiskMode.Text = "In Risk Mode Only";
            radL2RiskMode.UseVisualStyleBackColor = true;
            radL2RiskMode.CheckedChanged += Control_Changed;
            // 
            // radL2AfterQ10
            // 
            radL2AfterQ10.AutoSize = true;
            radL2AfterQ10.Location = new Point(20, 105);
            radL2AfterQ10.Name = "radL2AfterQ10";
            radL2AfterQ10.Size = new Size(134, 19);
            radL2AfterQ10.TabIndex = 4;
            radL2AfterQ10.Text = "After Question 10";
            radL2AfterQ10.UseVisualStyleBackColor = true;
            radL2AfterQ10.CheckedChanged += Control_Changed;
            // 
            // radL2AfterQ5
            // 
            radL2AfterQ5.AutoSize = true;
            radL2AfterQ5.Location = new Point(20, 80);
            radL2AfterQ5.Name = "radL2AfterQ5";
            radL2AfterQ5.Size = new Size(125, 19);
            radL2AfterQ5.TabIndex = 3;
            radL2AfterQ5.Text = "After Question 5";
            radL2AfterQ5.UseVisualStyleBackColor = true;
            radL2AfterQ5.CheckedChanged += Control_Changed;
            // 
            // radL2Always
            // 
            radL2Always.AutoSize = true;
            radL2Always.Checked = true;
            radL2Always.Location = new Point(20, 55);
            radL2Always.Name = "radL2Always";
            radL2Always.Size = new Size(108, 19);
            radL2Always.TabIndex = 2;
            radL2Always.TabStop = true;
            radL2Always.Text = "Always Available";
            radL2Always.UseVisualStyleBackColor = true;
            radL2Always.CheckedChanged += Control_Changed;
            // 
            // cmbLifeline2Type
            // 
            cmbLifeline2Type.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLifeline2Type.FormattingEnabled = true;
            cmbLifeline2Type.Items.AddRange(new object[] { "50:50", "Phone a Friend", "Ask the Audience", "Switch the Question" });
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
            grpLifeline1.Controls.Add(radL1RiskMode);
            grpLifeline1.Controls.Add(radL1AfterQ10);
            grpLifeline1.Controls.Add(radL1AfterQ5);
            grpLifeline1.Controls.Add(radL1Always);
            grpLifeline1.Controls.Add(cmbLifeline1Type);
            grpLifeline1.Controls.Add(lblLifeline1);
            grpLifeline1.Location = new Point(16, 56);
            grpLifeline1.Name = "grpLifeline1";
            grpLifeline1.Size = new Size(300, 160);
            grpLifeline1.TabIndex = 2;
            grpLifeline1.TabStop = false;
            grpLifeline1.Text = "Lifeline 1";
            // 
            // radL1RiskMode
            // 
            radL1RiskMode.AutoSize = true;
            radL1RiskMode.Location = new Point(20, 130);
            radL1RiskMode.Name = "radL1RiskMode";
            radL1RiskMode.Size = new Size(128, 19);
            radL1RiskMode.TabIndex = 5;
            radL1RiskMode.Text = "In Risk Mode Only";
            radL1RiskMode.UseVisualStyleBackColor = true;
            radL1RiskMode.CheckedChanged += Control_Changed;
            // 
            // radL1AfterQ10
            // 
            radL1AfterQ10.AutoSize = true;
            radL1AfterQ10.Location = new Point(20, 105);
            radL1AfterQ10.Name = "radL1AfterQ10";
            radL1AfterQ10.Size = new Size(134, 19);
            radL1AfterQ10.TabIndex = 4;
            radL1AfterQ10.Text = "After Question 10";
            radL1AfterQ10.UseVisualStyleBackColor = true;
            radL1AfterQ10.CheckedChanged += Control_Changed;
            // 
            // radL1AfterQ5
            // 
            radL1AfterQ5.AutoSize = true;
            radL1AfterQ5.Location = new Point(20, 80);
            radL1AfterQ5.Name = "radL1AfterQ5";
            radL1AfterQ5.Size = new Size(125, 19);
            radL1AfterQ5.TabIndex = 3;
            radL1AfterQ5.Text = "After Question 5";
            radL1AfterQ5.UseVisualStyleBackColor = true;
            radL1AfterQ5.CheckedChanged += Control_Changed;
            // 
            // radL1Always
            // 
            radL1Always.AutoSize = true;
            radL1Always.Checked = true;
            radL1Always.Location = new Point(20, 55);
            radL1Always.Name = "radL1Always";
            radL1Always.Size = new Size(108, 19);
            radL1Always.TabIndex = 2;
            radL1Always.TabStop = true;
            radL1Always.Text = "Always Available";
            radL1Always.UseVisualStyleBackColor = true;
            radL1Always.CheckedChanged += Control_Changed;
            // 
            // cmbLifeline1Type
            // 
            cmbLifeline1Type.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLifeline1Type.FormattingEnabled = true;
            cmbLifeline1Type.Items.AddRange(new object[] { "50:50", "Phone a Friend", "Ask the Audience", "Switch the Question" });
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
            numTotalLifelines.ValueChanged += Control_Changed;
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
            tabSounds.Controls.Add(grpGameSounds);
            tabSounds.Controls.Add(grpLifelineSounds);
            tabSounds.Location = new Point(4, 24);
            tabSounds.Name = "tabSounds";
            tabSounds.Padding = new Padding(3);
            tabSounds.Size = new Size(652, 459);
            tabSounds.TabIndex = 2;
            tabSounds.Text = "Sounds";
            tabSounds.UseVisualStyleBackColor = true;
            // 
            // grpGameSounds
            // 
            grpGameSounds.Controls.Add(btnBrowseWalkAway);
            grpGameSounds.Controls.Add(txtSoundWalkAway);
            grpGameSounds.Controls.Add(lblSoundWalkAway);
            grpGameSounds.Controls.Add(btnBrowseCorrectAnswer);
            grpGameSounds.Controls.Add(txtSoundCorrectAnswer);
            grpGameSounds.Controls.Add(lblSoundCorrectAnswer);
            grpGameSounds.Controls.Add(btnBrowseWrongAnswer);
            grpGameSounds.Controls.Add(txtSoundWrongAnswer);
            grpGameSounds.Controls.Add(lblSoundWrongAnswer);
            grpGameSounds.Controls.Add(btnBrowseFinalAnswer);
            grpGameSounds.Controls.Add(txtSoundFinalAnswer);
            grpGameSounds.Controls.Add(lblSoundFinalAnswer);
            grpGameSounds.Controls.Add(btnBrowseQuestionCue);
            grpGameSounds.Controls.Add(txtSoundQuestionCue);
            grpGameSounds.Controls.Add(lblSoundQuestionCue);
            grpGameSounds.Location = new Point(16, 16);
            grpGameSounds.Name = "grpGameSounds";
            grpGameSounds.Size = new Size(620, 200);
            grpGameSounds.TabIndex = 0;
            grpGameSounds.TabStop = false;
            grpGameSounds.Text = "Game Sounds";
            // 
            // btnBrowseWalkAway
            // 
            btnBrowseWalkAway.Location = new Point(570, 158);
            btnBrowseWalkAway.Name = "btnBrowseWalkAway";
            btnBrowseWalkAway.Size = new Size(32, 23);
            btnBrowseWalkAway.TabIndex = 14;
            btnBrowseWalkAway.Tag = "txtSoundWalkAway";
            btnBrowseWalkAway.Text = "...";
            btnBrowseWalkAway.UseVisualStyleBackColor = true;
            btnBrowseWalkAway.Click += btnBrowseSoundFile_Click;
            // 
            // txtSoundWalkAway
            // 
            txtSoundWalkAway.Location = new Point(130, 158);
            txtSoundWalkAway.Name = "txtSoundWalkAway";
            txtSoundWalkAway.Size = new Size(434, 23);
            txtSoundWalkAway.TabIndex = 13;
            txtSoundWalkAway.TextChanged += Control_Changed;
            // 
            // lblSoundWalkAway
            // 
            lblSoundWalkAway.AutoSize = true;
            lblSoundWalkAway.Location = new Point(20, 161);
            lblSoundWalkAway.Name = "lblSoundWalkAway";
            lblSoundWalkAway.Size = new Size(67, 15);
            lblSoundWalkAway.TabIndex = 12;
            lblSoundWalkAway.Text = "Walk Away:";
            // 
            // btnBrowseCorrectAnswer
            // 
            btnBrowseCorrectAnswer.Location = new Point(570, 123);
            btnBrowseCorrectAnswer.Name = "btnBrowseCorrectAnswer";
            btnBrowseCorrectAnswer.Size = new Size(32, 23);
            btnBrowseCorrectAnswer.TabIndex = 11;
            btnBrowseCorrectAnswer.Tag = "txtSoundCorrectAnswer";
            btnBrowseCorrectAnswer.Text = "...";
            btnBrowseCorrectAnswer.UseVisualStyleBackColor = true;
            btnBrowseCorrectAnswer.Click += btnBrowseSoundFile_Click;
            // 
            // txtSoundCorrectAnswer
            // 
            txtSoundCorrectAnswer.Location = new Point(130, 123);
            txtSoundCorrectAnswer.Name = "txtSoundCorrectAnswer";
            txtSoundCorrectAnswer.Size = new Size(434, 23);
            txtSoundCorrectAnswer.TabIndex = 10;
            txtSoundCorrectAnswer.TextChanged += Control_Changed;
            // 
            // lblSoundCorrectAnswer
            // 
            lblSoundCorrectAnswer.AutoSize = true;
            lblSoundCorrectAnswer.Location = new Point(20, 126);
            lblSoundCorrectAnswer.Name = "lblSoundCorrectAnswer";
            lblSoundCorrectAnswer.Size = new Size(93, 15);
            lblSoundCorrectAnswer.TabIndex = 9;
            lblSoundCorrectAnswer.Text = "Correct Answer:";
            // 
            // btnBrowseWrongAnswer
            // 
            btnBrowseWrongAnswer.Location = new Point(570, 88);
            btnBrowseWrongAnswer.Name = "btnBrowseWrongAnswer";
            btnBrowseWrongAnswer.Size = new Size(32, 23);
            btnBrowseWrongAnswer.TabIndex = 8;
            btnBrowseWrongAnswer.Tag = "txtSoundWrongAnswer";
            btnBrowseWrongAnswer.Text = "...";
            btnBrowseWrongAnswer.UseVisualStyleBackColor = true;
            btnBrowseWrongAnswer.Click += btnBrowseSoundFile_Click;
            // 
            // txtSoundWrongAnswer
            // 
            txtSoundWrongAnswer.Location = new Point(130, 88);
            txtSoundWrongAnswer.Name = "txtSoundWrongAnswer";
            txtSoundWrongAnswer.Size = new Size(434, 23);
            txtSoundWrongAnswer.TabIndex = 7;
            txtSoundWrongAnswer.TextChanged += Control_Changed;
            // 
            // lblSoundWrongAnswer
            // 
            lblSoundWrongAnswer.AutoSize = true;
            lblSoundWrongAnswer.Location = new Point(20, 91);
            lblSoundWrongAnswer.Name = "lblSoundWrongAnswer";
            lblSoundWrongAnswer.Size = new Size(88, 15);
            lblSoundWrongAnswer.TabIndex = 6;
            lblSoundWrongAnswer.Text = "Wrong Answer:";
            // 
            // btnBrowseFinalAnswer
            // 
            btnBrowseFinalAnswer.Location = new Point(570, 53);
            btnBrowseFinalAnswer.Name = "btnBrowseFinalAnswer";
            btnBrowseFinalAnswer.Size = new Size(32, 23);
            btnBrowseFinalAnswer.TabIndex = 5;
            btnBrowseFinalAnswer.Tag = "txtSoundFinalAnswer";
            btnBrowseFinalAnswer.Text = "...";
            btnBrowseFinalAnswer.UseVisualStyleBackColor = true;
            btnBrowseFinalAnswer.Click += btnBrowseSoundFile_Click;
            // 
            // txtSoundFinalAnswer
            // 
            txtSoundFinalAnswer.Location = new Point(130, 53);
            txtSoundFinalAnswer.Name = "txtSoundFinalAnswer";
            txtSoundFinalAnswer.Size = new Size(434, 23);
            txtSoundFinalAnswer.TabIndex = 4;
            txtSoundFinalAnswer.TextChanged += Control_Changed;
            // 
            // lblSoundFinalAnswer
            // 
            lblSoundFinalAnswer.AutoSize = true;
            lblSoundFinalAnswer.Location = new Point(20, 56);
            lblSoundFinalAnswer.Name = "lblSoundFinalAnswer";
            lblSoundFinalAnswer.Size = new Size(75, 15);
            lblSoundFinalAnswer.TabIndex = 3;
            lblSoundFinalAnswer.Text = "Final Answer:";
            // 
            // btnBrowseQuestionCue
            // 
            btnBrowseQuestionCue.Location = new Point(570, 23);
            btnBrowseQuestionCue.Name = "btnBrowseQuestionCue";
            btnBrowseQuestionCue.Size = new Size(32, 23);
            btnBrowseQuestionCue.TabIndex = 2;
            btnBrowseQuestionCue.Tag = "txtSoundQuestionCue";
            btnBrowseQuestionCue.Text = "...";
            btnBrowseQuestionCue.UseVisualStyleBackColor = true;
            btnBrowseQuestionCue.Click += btnBrowseSoundFile_Click;
            // 
            // txtSoundQuestionCue
            // 
            txtSoundQuestionCue.Location = new Point(130, 23);
            txtSoundQuestionCue.Name = "txtSoundQuestionCue";
            txtSoundQuestionCue.Size = new Size(434, 23);
            txtSoundQuestionCue.TabIndex = 1;
            txtSoundQuestionCue.TextChanged += Control_Changed;
            // 
            // lblSoundQuestionCue
            // 
            lblSoundQuestionCue.AutoSize = true;
            lblSoundQuestionCue.Location = new Point(20, 26);
            lblSoundQuestionCue.Name = "lblSoundQuestionCue";
            lblSoundQuestionCue.Size = new Size(82, 15);
            lblSoundQuestionCue.TabIndex = 0;
            lblSoundQuestionCue.Text = "Question Cue:";
            // 
            // grpLifelineSounds
            // 
            grpLifelineSounds.Controls.Add(btnBrowseSwitch);
            grpLifelineSounds.Controls.Add(txtSoundSwitch);
            grpLifelineSounds.Controls.Add(lblSoundSwitch);
            grpLifelineSounds.Controls.Add(btnBrowseATA);
            grpLifelineSounds.Controls.Add(txtSoundATA);
            grpLifelineSounds.Controls.Add(lblSoundATA);
            grpLifelineSounds.Controls.Add(btnBrowsePhone);
            grpLifelineSounds.Controls.Add(txtSoundPhone);
            grpLifelineSounds.Controls.Add(lblSoundPhone);
            grpLifelineSounds.Controls.Add(btnBrowse5050);
            grpLifelineSounds.Controls.Add(txtSound5050);
            grpLifelineSounds.Controls.Add(lblSound5050);
            grpLifelineSounds.Location = new Point(16, 230);
            grpLifelineSounds.Name = "grpLifelineSounds";
            grpLifelineSounds.Size = new Size(620, 165);
            grpLifelineSounds.TabIndex = 1;
            grpLifelineSounds.TabStop = false;
            grpLifelineSounds.Text = "Lifeline Sounds";
            // 
            // btnBrowseSwitch
            // 
            btnBrowseSwitch.Location = new Point(570, 123);
            btnBrowseSwitch.Name = "btnBrowseSwitch";
            btnBrowseSwitch.Size = new Size(32, 23);
            btnBrowseSwitch.TabIndex = 11;
            btnBrowseSwitch.Tag = "txtSoundSwitch";
            btnBrowseSwitch.Text = "...";
            btnBrowseSwitch.UseVisualStyleBackColor = true;
            btnBrowseSwitch.Click += btnBrowseSoundFile_Click;
            // 
            // txtSoundSwitch
            // 
            txtSoundSwitch.Location = new Point(130, 123);
            txtSoundSwitch.Name = "txtSoundSwitch";
            txtSoundSwitch.Size = new Size(434, 23);
            txtSoundSwitch.TabIndex = 10;
            txtSoundSwitch.TextChanged += Control_Changed;
            // 
            // lblSoundSwitch
            // 
            lblSoundSwitch.AutoSize = true;
            lblSoundSwitch.Location = new Point(20, 126);
            lblSoundSwitch.Name = "lblSoundSwitch";
            lblSoundSwitch.Size = new Size(108, 15);
            lblSoundSwitch.TabIndex = 9;
            lblSoundSwitch.Text = "Switch Question:";
            // 
            // btnBrowseATA
            // 
            btnBrowseATA.Location = new Point(570, 88);
            btnBrowseATA.Name = "btnBrowseATA";
            btnBrowseATA.Size = new Size(32, 23);
            btnBrowseATA.TabIndex = 8;
            btnBrowseATA.Tag = "txtSoundATA";
            btnBrowseATA.Text = "...";
            btnBrowseATA.UseVisualStyleBackColor = true;
            btnBrowseATA.Click += btnBrowseSoundFile_Click;
            // 
            // txtSoundATA
            // 
            txtSoundATA.Location = new Point(130, 88);
            txtSoundATA.Name = "txtSoundATA";
            txtSoundATA.Size = new Size(434, 23);
            txtSoundATA.TabIndex = 7;
            txtSoundATA.TextChanged += Control_Changed;
            // 
            // lblSoundATA
            // 
            lblSoundATA.AutoSize = true;
            lblSoundATA.Location = new Point(20, 91);
            lblSoundATA.Name = "lblSoundATA";
            lblSoundATA.Size = new Size(105, 15);
            lblSoundATA.TabIndex = 6;
            lblSoundATA.Text = "Ask the Audience:";
            // 
            // btnBrowsePhone
            // 
            btnBrowsePhone.Location = new Point(570, 53);
            btnBrowsePhone.Name = "btnBrowsePhone";
            btnBrowsePhone.Size = new Size(32, 23);
            btnBrowsePhone.TabIndex = 5;
            btnBrowsePhone.Tag = "txtSoundPhone";
            btnBrowsePhone.Text = "...";
            btnBrowsePhone.UseVisualStyleBackColor = true;
            btnBrowsePhone.Click += btnBrowseSoundFile_Click;
            // 
            // txtSoundPhone
            // 
            txtSoundPhone.Location = new Point(130, 53);
            txtSoundPhone.Name = "txtSoundPhone";
            txtSoundPhone.Size = new Size(434, 23);
            txtSoundPhone.TabIndex = 4;
            txtSoundPhone.TextChanged += Control_Changed;
            // 
            // lblSoundPhone
            // 
            lblSoundPhone.AutoSize = true;
            lblSoundPhone.Location = new Point(20, 56);
            lblSoundPhone.Name = "lblSoundPhone";
            lblSoundPhone.Size = new Size(96, 15);
            lblSoundPhone.TabIndex = 3;
            lblSoundPhone.Text = "Phone a Friend:";
            // 
            // btnBrowse5050
            // 
            btnBrowse5050.Location = new Point(570, 23);
            btnBrowse5050.Name = "btnBrowse5050";
            btnBrowse5050.Size = new Size(32, 23);
            btnBrowse5050.TabIndex = 2;
            btnBrowse5050.Tag = "txtSound5050";
            btnBrowse5050.Text = "...";
            btnBrowse5050.UseVisualStyleBackColor = true;
            btnBrowse5050.Click += btnBrowseSoundFile_Click;
            // 
            // txtSound5050
            // 
            txtSound5050.Location = new Point(130, 23);
            txtSound5050.Name = "txtSound5050";
            txtSound5050.Size = new Size(434, 23);
            txtSound5050.TabIndex = 1;
            txtSound5050.TextChanged += Control_Changed;
            // 
            // lblSound5050
            // 
            lblSound5050.AutoSize = true;
            lblSound5050.Location = new Point(20, 26);
            lblSound5050.Name = "lblSound5050";
            lblSound5050.Size = new Size(39, 15);
            lblSound5050.TabIndex = 0;
            lblSound5050.Text = "50:50:";
            // 
            // btnOK
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
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(502, 515);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(80, 30);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnApply
            // 
            btnApply.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnApply.Location = new Point(588, 515);
            btnApply.Name = "btnApply";
            btnApply.Size = new Size(80, 30);
            btnApply.TabIndex = 3;
            btnApply.Text = "Apply";
            btnApply.UseVisualStyleBackColor = true;
            btnApply.Click += btnApply_Click;
            // 
            // OptionsDialog
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(684, 561);
            Controls.Add(btnApply);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(tabControl);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "OptionsDialog";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Options";
            tabControl.ResumeLayout(false);
            tabGeneral.ResumeLayout(false);
            grpScreens.ResumeLayout(false);
            grpScreens.PerformLayout();
            grpGameplay.ResumeLayout(false);
            grpGameplay.PerformLayout();
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
            grpGameSounds.ResumeLayout(false);
            grpGameSounds.PerformLayout();
            grpLifelineSounds.ResumeLayout(false);
            grpLifelineSounds.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabControl;
        private TabPage tabGeneral;
        private TabPage tabLifelines;
        private TabPage tabSounds;
        private Button btnOK;
        private Button btnCancel;
        private Button btnApply;
        private GroupBox grpScreens;
        private CheckBox chkAutoShowHostScreen;
        private CheckBox chkAutoShowGuestScreen;
        private CheckBox chkAutoShowTVScreen;
        private CheckBox chkFullScreenHostScreen;
        private CheckBox chkFullScreenGuestScreen;
        private CheckBox chkFullScreenTVScreen;
        private GroupBox grpGameplay;
        private CheckBox chkShowAnswerOnlyAtFinal;
        private CheckBox chkAutoShowTotalWinnings;
        private CheckBox chkAutoHideQuestionAtWalkAway;
        private CheckBox chkHideAnswersOnNewQuestion;
        private Label lblTotalLifelines;
        private NumericUpDown numTotalLifelines;
        private GroupBox grpLifeline1;
        private Label lblLifeline1;
        private ComboBox cmbLifeline1Type;
        private RadioButton radL1Always;
        private RadioButton radL1AfterQ5;
        private RadioButton radL1AfterQ10;
        private RadioButton radL1RiskMode;
        private GroupBox grpLifeline2;
        private RadioButton radL2RiskMode;
        private RadioButton radL2AfterQ10;
        private RadioButton radL2AfterQ5;
        private RadioButton radL2Always;
        private ComboBox cmbLifeline2Type;
        private Label lblLifeline2;
        private GroupBox grpLifeline3;
        private RadioButton radL3RiskMode;
        private RadioButton radL3AfterQ10;
        private RadioButton radL3AfterQ5;
        private RadioButton radL3Always;
        private ComboBox cmbLifeline3Type;
        private Label lblLifeline3;
        private GroupBox grpLifeline4;
        private RadioButton radL4RiskMode;
        private RadioButton radL4AfterQ10;
        private RadioButton radL4AfterQ5;
        private RadioButton radL4Always;
        private ComboBox cmbLifeline4Type;
        private Label lblLifeline4;
        private GroupBox grpGameSounds;
        private Label lblSoundQuestionCue;
        private TextBox txtSoundQuestionCue;
        private Button btnBrowseQuestionCue;
        private GroupBox grpLifelineSounds;
        private Button btnBrowseFinalAnswer;
        private TextBox txtSoundFinalAnswer;
        private Label lblSoundFinalAnswer;
        private Button btnBrowseWrongAnswer;
        private TextBox txtSoundWrongAnswer;
        private Label lblSoundWrongAnswer;
        private Button btnBrowseCorrectAnswer;
        private TextBox txtSoundCorrectAnswer;
        private Label lblSoundCorrectAnswer;
        private Button btnBrowseWalkAway;
        private TextBox txtSoundWalkAway;
        private Label lblSoundWalkAway;
        private Button btnBrowse5050;
        private TextBox txtSound5050;
        private Label lblSound5050;
        private Button btnBrowsePhone;
        private TextBox txtSoundPhone;
        private Label lblSoundPhone;
        private Button btnBrowseATA;
        private TextBox txtSoundATA;
        private Label lblSoundATA;
        private Button btnBrowseSwitch;
        private TextBox txtSoundSwitch;
        private Label lblSoundSwitch;
    }
}
