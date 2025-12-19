namespace MillionaireGame.Forms
{
    partial class ControlPanelForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _gameService.LevelChanged -= OnLevelChanged;
                _gameService.ModeChanged -= OnModeChanged;
                _gameService.LifelineUsed -= OnLifelineUsed;

                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            // Question display
            txtQuestion = new TextBox();
            txtA = new TextBox();
            txtB = new TextBox();
            txtC = new TextBox();
            txtD = new TextBox();
            txtExplain = new TextBox();
            
            // Answer selection buttons
            btnA = new Button();
            btnB = new Button();
            btnC = new Button();
            btnD = new Button();
            
            // Broadcast flow buttons
            btnHostIntro = new Button();
            btnPickPlayer = new Button();
            btnExplainGame = new Button();
            btnLightsDown = new Button();
            btnNewQuestion = new Button();
            btnReveal = new Button();
            btnWalk = new Button();
            btnThanksForPlaying = new Button();
            btnResetGame = new Button();
            btnClosing = new Button();
            btnStopAudio = new Button();
            
            // Money tree controls
            nmrLevel = new NumericUpDown();
            lblLevelLabel = new Label();
            lblAnswer = new Label();
            lblCorrectAnswerLabel = new Label();
            
            // Money display
            txtCorrect = new TextBox();
            txtCurrent = new TextBox();
            txtWrong = new TextBox();
            txtDrop = new TextBox();
            txtQLeft = new TextBox();
            txtID = new TextBox();
            lblCorrectLabel = new Label();
            lblCurrentLabel = new Label();
            lblWrongLabel = new Label();
            lblDropLabel = new Label();
            lblQLeftLabel = new Label();
            
            // Lifeline buttons
            btn5050 = new Button();
            btnPhoneFriend = new Button();
            btnAskAudience = new Button();
            btnSwitch = new Button();
            
            // Other controls
            chkShowQuestion = new CheckBox();
            chkCorrectAnswer = new CheckBox();
            btnActivateRiskMode = new Button();
            
            // Menu
            menuStrip = new MenuStrip();
            databaseToolStripMenuItem = new ToolStripMenuItem();
            gameToolStripMenuItem = new ToolStripMenuItem();
            viewToolStripMenuItem = new ToolStripMenuItem();
            helpToolStripMenuItem = new ToolStripMenuItem();
            
            ((System.ComponentModel.ISupportInitialize)nmrLevel).BeginInit();
            menuStrip.SuspendLayout();
            SuspendLayout();
            
            // 
            // menuStrip
            // 
            menuStrip.ImageScalingSize = new Size(20, 20);
            menuStrip.Items.AddRange(new ToolStripItem[] {
                databaseToolStripMenuItem,
                gameToolStripMenuItem,
                viewToolStripMenuItem,
                helpToolStripMenuItem});
            menuStrip.Location = new Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Size = new Size(1000, 28);
            menuStrip.TabIndex = 0;
            
            // 
            // databaseToolStripMenuItem
            // 
            databaseToolStripMenuItem.Name = "databaseToolStripMenuItem";
            databaseToolStripMenuItem.Size = new Size(84, 24);
            databaseToolStripMenuItem.Text = "Database";
            databaseToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("Database Settings", null, DatabaseToolStripMenuItem_Click),
                new ToolStripSeparator(),
                new ToolStripMenuItem("Close", null, CloseToolStripMenuItem_Click)
            });
            
            // 
            // gameToolStripMenuItem
            // 
            gameToolStripMenuItem.Name = "gameToolStripMenuItem";
            gameToolStripMenuItem.Size = new Size(62, 24);
            gameToolStripMenuItem.Text = "Game";
            gameToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("Questions Editor", null, QuestionsEditorToolStripMenuItem_Click),
                new ToolStripSeparator(),
                new ToolStripMenuItem("Options", null, OptionsToolStripMenuItem_Click)
            });
            
            // 
            // viewToolStripMenuItem
            // 
            viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            viewToolStripMenuItem.Size = new Size(55, 24);
            viewToolStripMenuItem.Text = "View";
            viewToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("Host Screen", null, HostScreenToolStripMenuItem_Click),
                new ToolStripMenuItem("Guest Screen", null, GuestScreenToolStripMenuItem_Click),
                new ToolStripSeparator(),
                new ToolStripMenuItem("TV Screen", null, TVScreenToolStripMenuItem_Click)
            });
            
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new Size(55, 24);
            helpToolStripMenuItem.Text = "Help";
            helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("About", null, AboutToolStripMenuItem_Click)
            });
            
            // 
            // txtQuestion
            // 
            txtQuestion.BackColor = SystemColors.Info;
            txtQuestion.Location = new Point(12, 40);
            txtQuestion.Multiline = true;
            txtQuestion.Name = "txtQuestion";
            txtQuestion.ReadOnly = true;
            txtQuestion.ScrollBars = ScrollBars.Vertical;
            txtQuestion.Size = new Size(550, 80);
            txtQuestion.TabStop = false;
            txtQuestion.TabIndex = 0;
            
            // 
            // txtA
            // 
            txtA.BackColor = Color.Silver;
            txtA.Location = new Point(77, 131);
            txtA.Name = "txtA";
            txtA.ReadOnly = true;
            txtA.Size = new Size(200, 27);
            txtA.TabStop = false;
            txtA.TabIndex = 1;
            
            // 
            // txtB
            // 
            txtB.BackColor = Color.Silver;
            txtB.Location = new Point(297, 131);
            txtB.Name = "txtB";
            txtB.ReadOnly = true;
            txtB.Size = new Size(200, 27);
            txtB.TabStop = false;
            txtB.TabIndex = 2;
            
            // 
            // txtC
            // 
            txtC.BackColor = Color.Silver;
            txtC.Location = new Point(77, 191);
            txtC.Name = "txtC";
            txtC.ReadOnly = true;
            txtC.Size = new Size(200, 27);
            txtC.TabStop = false;
            txtC.TabIndex = 3;
            
            // 
            // txtD
            // 
            txtD.BackColor = Color.Silver;
            txtD.Location = new Point(297, 191);
            txtD.Name = "txtD";
            txtD.ReadOnly = true;
            txtD.Size = new Size(200, 27);
            txtD.TabStop = false;
            txtD.TabIndex = 4;
            
            // 
            // txtExplain
            // 
            txtExplain.BackColor = SystemColors.Info;
            txtExplain.Location = new Point(12, 430);
            txtExplain.Multiline = true;
            txtExplain.Name = "txtExplain";
            txtExplain.ReadOnly = true;
            txtExplain.ScrollBars = ScrollBars.Vertical;
            txtExplain.Size = new Size(550, 70);
            txtExplain.TabIndex = 5;
            
            // 
            // btnA
            // 
            btnA.BackColor = Color.DarkOrange;
            btnA.FlatStyle = FlatStyle.Flat;
            btnA.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnA.Location = new Point(12, 120);
            btnA.Name = "btnA";
            btnA.Size = new Size(60, 48);
            btnA.TabIndex = 6;
            btnA.Text = "A";
            btnA.UseVisualStyleBackColor = false;
            btnA.Click += btnA_Click;
            
            // 
            // btnB
            // 
            btnB.BackColor = Color.DarkOrange;
            btnB.FlatStyle = FlatStyle.Flat;
            btnB.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnB.Location = new Point(502, 120);
            btnB.Name = "btnB";
            btnB.Size = new Size(60, 48);
            btnB.TabIndex = 7;
            btnB.Text = "B";
            btnB.UseVisualStyleBackColor = false;
            btnB.Click += btnB_Click;
            
            // 
            // btnC
            // 
            btnC.BackColor = Color.DarkOrange;
            btnC.FlatStyle = FlatStyle.Flat;
            btnC.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnC.Location = new Point(12, 180);
            btnC.Name = "btnC";
            btnC.Size = new Size(60, 48);
            btnC.TabIndex = 8;
            btnC.Text = "C";
            btnC.UseVisualStyleBackColor = false;
            btnC.Click += btnC_Click;
            
            // 
            // btnD
            // 
            btnD.BackColor = Color.DarkOrange;
            btnD.FlatStyle = FlatStyle.Flat;
            btnD.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnD.Location = new Point(502, 180);
            btnD.Name = "btnD";
            btnD.Size = new Size(60, 48);
            btnD.TabIndex = 9;
            btnD.Text = "D";
            btnD.UseVisualStyleBackColor = false;
            btnD.Click += btnD_Click;
            
            // 
            // btnHostIntro
            // 
            btnHostIntro.BackColor = Color.Blue;
            btnHostIntro.FlatStyle = FlatStyle.Flat;
            btnHostIntro.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnHostIntro.ForeColor = Color.White;
            btnHostIntro.Location = new Point(580, 40);
            btnHostIntro.Name = "btnHostIntro";
            btnHostIntro.Size = new Size(120, 45);
            btnHostIntro.TabIndex = 10;
            btnHostIntro.Text = "1. Host Intro";
            btnHostIntro.UseVisualStyleBackColor = false;
            btnHostIntro.Click += btnHostIntro_Click;
            
            // 
            // btnPickPlayer
            // 
            btnPickPlayer.BackColor = Color.Blue;
            btnPickPlayer.FlatStyle = FlatStyle.Flat;
            btnPickPlayer.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnPickPlayer.ForeColor = Color.White;
            btnPickPlayer.Location = new Point(710, 40);
            btnPickPlayer.Name = "btnPickPlayer";
            btnPickPlayer.Size = new Size(120, 45);
            btnPickPlayer.TabIndex = 11;
            btnPickPlayer.Text = "2. Pick Player";
            btnPickPlayer.UseVisualStyleBackColor = false;
            btnPickPlayer.Click += btnPickPlayer_Click;
            
            // 
            // btnExplainGame
            // 
            btnExplainGame.BackColor = Color.Blue;
            btnExplainGame.FlatStyle = FlatStyle.Flat;
            btnExplainGame.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnExplainGame.ForeColor = Color.White;
            btnExplainGame.Location = new Point(840, 40);
            btnExplainGame.Name = "btnExplainGame";
            btnExplainGame.Size = new Size(120, 45);
            btnExplainGame.TabIndex = 12;
            btnExplainGame.Text = "3. Explain Game";
            btnExplainGame.UseVisualStyleBackColor = false;
            btnExplainGame.Click += btnExplainGame_Click;
            
            // 
            // btnLightsDown
            // 
            btnLightsDown.BackColor = Color.MediumTurquoise;
            btnLightsDown.FlatStyle = FlatStyle.Flat;
            btnLightsDown.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnLightsDown.Location = new Point(580, 90);
            btnLightsDown.Name = "btnLightsDown";
            btnLightsDown.Size = new Size(120, 45);
            btnLightsDown.TabIndex = 13;
            btnLightsDown.Text = "4. Lights Down (F7)";
            btnLightsDown.UseVisualStyleBackColor = false;
            btnLightsDown.Click += btnLightsDown_Click;
            
            // 
            // btnNewQuestion
            // 
            btnNewQuestion.BackColor = Color.Teal;
            btnNewQuestion.FlatStyle = FlatStyle.Flat;
            btnNewQuestion.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnNewQuestion.ForeColor = Color.White;
            btnNewQuestion.Location = new Point(710, 90);
            btnNewQuestion.Name = "btnNewQuestion";
            btnNewQuestion.Size = new Size(120, 45);
            btnNewQuestion.TabIndex = 14;
            btnNewQuestion.Text = "5. Question (F5)";
            btnNewQuestion.UseVisualStyleBackColor = false;
            btnNewQuestion.Click += btnNewQuestion_Click;
            
            // 
            // btnReveal
            // 
            btnReveal.BackColor = Color.Turquoise;
            btnReveal.FlatStyle = FlatStyle.Flat;
            btnReveal.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnReveal.Location = new Point(840, 90);
            btnReveal.Name = "btnReveal";
            btnReveal.Size = new Size(120, 45);
            btnReveal.TabIndex = 15;
            btnReveal.Text = "7. Reveal (F6)";
            btnReveal.UseVisualStyleBackColor = false;
            btnReveal.Click += btnReveal_Click;
            
            // 
            // btnWalk
            // 
            btnWalk.BackColor = Color.FromArgb(255, 128, 0);
            btnWalk.FlatStyle = FlatStyle.Flat;
            btnWalk.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnWalk.Location = new Point(580, 140);
            btnWalk.Name = "btnWalk";
            btnWalk.Size = new Size(120, 45);
            btnWalk.TabIndex = 16;
            btnWalk.Text = "Walk Away (End)";
            btnWalk.UseVisualStyleBackColor = false;
            btnWalk.Click += btnWalk_Click;
            
            // 
            // btnThanksForPlaying
            // 
            btnThanksForPlaying.BackColor = Color.Blue;
            btnThanksForPlaying.FlatStyle = FlatStyle.Flat;
            btnThanksForPlaying.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnThanksForPlaying.ForeColor = Color.White;
            btnThanksForPlaying.Location = new Point(710, 140);
            btnThanksForPlaying.Name = "btnThanksForPlaying";
            btnThanksForPlaying.Size = new Size(120, 45);
            btnThanksForPlaying.TabIndex = 17;
            btnThanksForPlaying.Text = "8. Thanks!";
            btnThanksForPlaying.UseVisualStyleBackColor = false;
            btnThanksForPlaying.Click += btnThanksForPlaying_Click;
            
            // 
            // btnResetGame
            // 
            btnResetGame.BackColor = Color.Red;
            btnResetGame.FlatStyle = FlatStyle.Flat;
            btnResetGame.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnResetGame.ForeColor = Color.White;
            btnResetGame.Location = new Point(840, 140);
            btnResetGame.Name = "btnResetGame";
            btnResetGame.Size = new Size(120, 45);
            btnResetGame.TabIndex = 18;
            btnResetGame.Text = "9. Reset (Del)";
            btnResetGame.UseVisualStyleBackColor = false;
            btnResetGame.Click += btnResetGame_Click;
            
            // 
            // btnClosing
            // 
            btnClosing.BackColor = Color.Blue;
            btnClosing.FlatStyle = FlatStyle.Flat;
            btnClosing.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnClosing.ForeColor = Color.White;
            btnClosing.Location = new Point(580, 190);
            btnClosing.Name = "btnClosing";
            btnClosing.Size = new Size(120, 45);
            btnClosing.TabIndex = 19;
            btnClosing.Text = "10. Closing";
            btnClosing.UseVisualStyleBackColor = false;
            btnClosing.Click += btnClosing_Click;
            
            // 
            // btnStopAudio
            // 
            btnStopAudio.BackColor = Color.DarkRed;
            btnStopAudio.FlatStyle = FlatStyle.Flat;
            btnStopAudio.ForeColor = Color.White;
            btnStopAudio.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnStopAudio.Location = new Point(710, 190);
            btnStopAudio.Name = "btnStopAudio";
            btnStopAudio.Size = new Size(250, 45);
            btnStopAudio.TabIndex = 20;
            btnStopAudio.Text = "⏹ STOP ALL AUDIO";
            btnStopAudio.UseVisualStyleBackColor = false;
            btnStopAudio.Click += btnStopAudio_Click;
            
            // 
            // nmrLevel
            // 
            nmrLevel.Font = new Font("Segoe UI", 11F);
            nmrLevel.Location = new Point(120, 245);
            nmrLevel.Maximum = new decimal(new int[] { 14, 0, 0, 0 });
            nmrLevel.Name = "nmrLevel";
            nmrLevel.Size = new Size(65, 32);
            nmrLevel.TabIndex = 21;
            nmrLevel.ValueChanged += nmrLevel_ValueChanged;
            
            // 
            // lblLevelLabel
            // 
            lblLevelLabel.AutoSize = true;
            lblLevelLabel.Location = new Point(12, 250);
            lblLevelLabel.Name = "lblLevelLabel";
            lblLevelLabel.Size = new Size(110, 20);
            lblLevelLabel.TabIndex = 22;
            lblLevelLabel.Text = "Question #:";
            
            // 
            // lblCorrectAnswerLabel
            // 
            lblCorrectAnswerLabel.AutoSize = true;
            lblCorrectAnswerLabel.Location = new Point(195, 250);
            lblCorrectAnswerLabel.Name = "lblCorrectAnswerLabel";
            lblCorrectAnswerLabel.Size = new Size(122, 20);
            lblCorrectAnswerLabel.TabIndex = 23;
            lblCorrectAnswerLabel.Text = "Correct Answer:";
            
            // 
            // lblAnswer
            // 
            lblAnswer.AutoSize = true;
            lblAnswer.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblAnswer.ForeColor = Color.Green;
            lblAnswer.Location = new Point(325, 242);
            lblAnswer.Name = "lblAnswer";
            lblAnswer.Size = new Size(30, 37);
            lblAnswer.TabIndex = 24;
            lblAnswer.Text = "";
            
            // 
            // txtCorrect
            // 
            txtCorrect.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            txtCorrect.Location = new Point(120, 290);
            txtCorrect.Name = "txtCorrect";
            txtCorrect.ReadOnly = true;
            txtCorrect.Size = new Size(100, 27);
            txtCorrect.TabIndex = 25;
            txtCorrect.TextAlign = HorizontalAlignment.Right;
            
            // 
            // txtCurrent
            // 
            txtCurrent.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            txtCurrent.Location = new Point(120, 323);
            txtCurrent.Name = "txtCurrent";
            txtCurrent.ReadOnly = true;
            txtCurrent.Size = new Size(100, 27);
            txtCurrent.TabIndex = 26;
            txtCurrent.TextAlign = HorizontalAlignment.Right;
            
            // 
            // txtWrong
            // 
            txtWrong.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            txtWrong.Location = new Point(120, 356);
            txtWrong.Name = "txtWrong";
            txtWrong.ReadOnly = true;
            txtWrong.Size = new Size(100, 27);
            txtWrong.TabIndex = 27;
            txtWrong.TextAlign = HorizontalAlignment.Right;
            
            // 
            // txtDrop
            // 
            txtDrop.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            txtDrop.Location = new Point(295, 290);
            txtDrop.Name = "txtDrop";
            txtDrop.ReadOnly = true;
            txtDrop.Size = new Size(100, 27);
            txtDrop.TabIndex = 28;
            txtDrop.TextAlign = HorizontalAlignment.Right;
            
            // 
            // txtQLeft
            // 
            txtQLeft.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            txtQLeft.Location = new Point(295, 323);
            txtQLeft.Name = "txtQLeft";
            txtQLeft.ReadOnly = true;
            txtQLeft.Size = new Size(100, 27);
            txtQLeft.TabIndex = 29;
            txtQLeft.TextAlign = HorizontalAlignment.Right;
            
            // 
            // txtID
            // 
            txtID.Location = new Point(840, 326);
            txtID.Name = "txtID";
            txtID.ReadOnly = true;
            txtID.Size = new Size(60, 27);
            txtID.TabIndex = 30;
            txtID.Visible = false;
            
            // 
            // lblCorrectLabel
            // 
            lblCorrectLabel.AutoSize = true;
            lblCorrectLabel.Location = new Point(12, 293);
            lblCorrectLabel.Name = "lblCorrectLabel";
            lblCorrectLabel.Size = new Size(76, 20);
            lblCorrectLabel.TabIndex = 31;
            lblCorrectLabel.Text = "If Correct:";
            
            // 
            // lblCurrentLabel
            // 
            lblCurrentLabel.AutoSize = true;
            lblCurrentLabel.Location = new Point(12, 326);
            lblCurrentLabel.Name = "lblCurrentLabel";
            lblCurrentLabel.Size = new Size(64, 20);
            lblCurrentLabel.TabIndex = 32;
            lblCurrentLabel.Text = "Current:";
            
            // 
            // lblWrongLabel
            // 
            lblWrongLabel.AutoSize = true;
            lblWrongLabel.Location = new Point(12, 359);
            lblWrongLabel.Name = "lblWrongLabel";
            lblWrongLabel.Size = new Size(72, 20);
            lblWrongLabel.TabIndex = 33;
            lblWrongLabel.Text = "If Wrong:";
            
            // 
            // lblDropLabel
            // 
            lblDropLabel.AutoSize = true;
            lblDropLabel.Location = new Point(227, 293);
            lblDropLabel.Name = "lblDropLabel";
            lblDropLabel.Size = new Size(60, 20);
            lblDropLabel.TabIndex = 34;
            lblDropLabel.Text = "If Drop:";
            
            // 
            // lblQLeftLabel
            // 
            lblQLeftLabel.AutoSize = true;
            lblQLeftLabel.Location = new Point(227, 326);
            lblQLeftLabel.Name = "lblQLeftLabel";
            lblQLeftLabel.Size = new Size(111, 20);
            lblQLeftLabel.TabIndex = 35;
            lblQLeftLabel.Text = "Questions Left:";
            
            // 
            // btn5050
            // 
            btn5050.BackColor = Color.DarkOrange;
            btn5050.FlatStyle = FlatStyle.Flat;
            btn5050.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btn5050.Location = new Point(580, 245);
            btn5050.Name = "btn5050";
            btn5050.Size = new Size(120, 40);
            btn5050.TabIndex = 36;
            btn5050.Text = "";
            btn5050.UseVisualStyleBackColor = false;
            btn5050.Click += btn5050_Click;
            
            // 
            // btnPhoneFriend
            // 
            btnPhoneFriend.BackColor = Color.DarkOrange;
            btnPhoneFriend.FlatStyle = FlatStyle.Flat;
            btnPhoneFriend.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnPhoneFriend.Location = new Point(580, 290);
            btnPhoneFriend.Name = "btnPhoneFriend";
            btnPhoneFriend.Size = new Size(120, 40);
            btnPhoneFriend.TabIndex = 37;
            btnPhoneFriend.Text = "";
            btnPhoneFriend.UseVisualStyleBackColor = false;
            btnPhoneFriend.Click += btnPhoneFriend_Click;
            
            // 
            // btnAskAudience
            // 
            btnAskAudience.BackColor = Color.DarkOrange;
            btnAskAudience.FlatStyle = FlatStyle.Flat;
            btnAskAudience.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnAskAudience.Location = new Point(580, 335);
            btnAskAudience.Name = "btnAskAudience";
            btnAskAudience.Size = new Size(120, 40);
            btnAskAudience.TabIndex = 38;
            btnAskAudience.Text = "";
            btnAskAudience.UseVisualStyleBackColor = false;
            btnAskAudience.Click += btnAskAudience_Click;
            
            // 
            // btnSwitch
            // 
            btnSwitch.BackColor = Color.DarkOrange;
            btnSwitch.FlatStyle = FlatStyle.Flat;
            btnSwitch.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnSwitch.Location = new Point(580, 380);
            btnSwitch.Name = "btnSwitch";
            btnSwitch.Size = new Size(120, 40);
            btnSwitch.TabIndex = 39;
            btnSwitch.Text = "";
            btnSwitch.UseVisualStyleBackColor = false;
            btnSwitch.Click += btnSwitch_Click;
            
            // 
            // chkShowQuestion
            // 
            chkShowQuestion.AutoSize = true;
            chkShowQuestion.Checked = true;
            chkShowQuestion.CheckState = CheckState.Checked;
            chkShowQuestion.Location = new Point(580, 435);
            chkShowQuestion.Name = "chkShowQuestion";
            chkShowQuestion.Size = new Size(190, 24);
            chkShowQuestion.TabIndex = 40;
            chkShowQuestion.Text = "Show Question on TV";
            chkShowQuestion.UseVisualStyleBackColor = true;
            
            // 
            // chkCorrectAnswer
            // 
            chkCorrectAnswer.AutoSize = true;
            chkCorrectAnswer.Location = new Point(580, 465);
            chkCorrectAnswer.Name = "chkCorrectAnswer";
            chkCorrectAnswer.Size = new Size(183, 24);
            chkCorrectAnswer.TabIndex = 41;
            chkCorrectAnswer.Text = "Show Correct Answer";
            chkCorrectAnswer.UseVisualStyleBackColor = true;
            
            // 
            // btnActivateRiskMode
            // 
            btnActivateRiskMode.BackColor = Color.Orange;
            btnActivateRiskMode.FlatStyle = FlatStyle.Flat;
            btnActivateRiskMode.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnActivateRiskMode.Location = new Point(710, 245);
            btnActivateRiskMode.Name = "btnActivateRiskMode";
            btnActivateRiskMode.Size = new Size(100, 175);
            btnActivateRiskMode.TabIndex = 42;
            btnActivateRiskMode.Text = "RISK MODE OFF";
            btnActivateRiskMode.UseVisualStyleBackColor = false;
            btnActivateRiskMode.Click += btnActivateRiskMode_Click;

            
            // 
            // ControlPanelForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(980, 530);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Controls.Add(btnActivateRiskMode);
            Controls.Add(chkCorrectAnswer);
            Controls.Add(chkShowQuestion);
            Controls.Add(btnSwitch);
            Controls.Add(btnAskAudience);
            Controls.Add(btnPhoneFriend);
            Controls.Add(btn5050);
            Controls.Add(lblQLeftLabel);
            Controls.Add(lblDropLabel);
            Controls.Add(lblWrongLabel);
            Controls.Add(lblCurrentLabel);
            Controls.Add(lblCorrectLabel);
            Controls.Add(txtID);
            Controls.Add(txtQLeft);
            Controls.Add(txtDrop);
            Controls.Add(txtWrong);
            Controls.Add(txtCurrent);
            Controls.Add(txtCorrect);
            Controls.Add(lblAnswer);
            Controls.Add(lblCorrectAnswerLabel);
            Controls.Add(lblLevelLabel);
            Controls.Add(nmrLevel);
            Controls.Add(btnStopAudio);
            Controls.Add(btnClosing);
            Controls.Add(btnResetGame);
            Controls.Add(btnThanksForPlaying);
            Controls.Add(btnWalk);
            Controls.Add(btnReveal);
            Controls.Add(btnNewQuestion);
            Controls.Add(btnLightsDown);
            Controls.Add(btnExplainGame);
            Controls.Add(btnPickPlayer);
            Controls.Add(btnHostIntro);
            Controls.Add(btnD);
            Controls.Add(btnC);
            Controls.Add(btnB);
            Controls.Add(btnA);
            Controls.Add(txtExplain);
            Controls.Add(txtD);
            Controls.Add(txtC);
            Controls.Add(txtB);
            Controls.Add(txtA);
            Controls.Add(txtQuestion);
            Controls.Add(menuStrip);
            MainMenuStrip = menuStrip;
            Name = "ControlPanelForm";
            Text = "The Millionaire Game - Control Panel";
            Load += ControlPanelForm_Load;
            ((System.ComponentModel.ISupportInitialize)nmrLevel).EndInit();
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip;
        private ToolStripMenuItem databaseToolStripMenuItem;
        private ToolStripMenuItem gameToolStripMenuItem;
        private ToolStripMenuItem viewToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private TextBox txtQuestion;
        private TextBox txtA;
        private TextBox txtB;
        private TextBox txtC;
        private TextBox txtD;
        private TextBox txtExplain;
        private TextBox txtCorrect;
        private TextBox txtCurrent;
        private TextBox txtWrong;
        private TextBox txtDrop;
        private TextBox txtQLeft;
        private TextBox txtID;
        private Label lblAnswer;
        private Label lblLevelLabel;
        private Label lblCorrectAnswerLabel;
        private Label lblCorrectLabel;
        private Label lblCurrentLabel;
        private Label lblWrongLabel;
        private Label lblDropLabel;
        private Label lblQLeftLabel;
        private Button btnHostIntro;
        private Button btnPickPlayer;
        private Button btnExplainGame;
        private Button btnLightsDown;
        private Button btnNewQuestion;
        private Button btnReveal;
        private Button btnWalk;
        private Button btnThanksForPlaying;
        private Button btnResetGame;
        private Button btnClosing;
        private Button btnStopAudio;
        private Button btnA;
        private Button btnB;
        private Button btnC;
        private Button btnD;
        private Button btnActivateRiskMode;
        private Button btn5050;
        private Button btnPhoneFriend;
        private Button btnAskAudience;
        private Button btnSwitch;
        private NumericUpDown nmrLevel;
        private CheckBox chkShowQuestion;
        private CheckBox chkCorrectAnswer;
    }
}
