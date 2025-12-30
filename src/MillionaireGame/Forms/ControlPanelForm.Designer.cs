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

                // Stop and dispose web server
                if (_webServerHost != null)
                {
                    try
                    {
                        _webServerHost.StopAsync().Wait(TimeSpan.FromSeconds(5));
                        _webServerHost.Dispose();
                    }
                    catch
                    {
                        // Ignore errors during shutdown
                    }
                }

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
            btnClosing = new Button();
            btnFadeOutAudio = new Button();
            btnStopAudio = new Button();
            btnResetRound = new Button();
            btnResetGame = new Button();
            
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
            lblExplanationLabel = new Label();
            
            // Lifeline buttons
            btnLifeline1 = new Button();
            btnLifeline2 = new Button();
            btnLifeline3 = new Button();
            btnLifeline4 = new Button();
            
            // Other controls
            chkShowQuestion = new CheckBox();
            chkShowWinnings = new CheckBox();
            chkCorrectAnswer = new CheckBox();
            btnActivateRiskMode = new Button();
            btnShowMoneyTree = new Button();
            
            // Host messaging controls
            lblHostMessage = new Label();
            txtHostMessage = new TextBox();
            btnSendHostMessage = new Button();
            btnClearHostMessage = new Button();
            
            // Menu
            menuStrip = new MenuStrip();
            gameToolStripMenuItem = new ToolStripMenuItem();
            screensToolStripMenuItem = new ToolStripMenuItem();
            hostScreenMenuItem = new ToolStripMenuItem("Host Screen", null, HostScreenToolStripMenuItem_Click);
            guestScreenMenuItem = new ToolStripMenuItem("Guest Screen", null, GuestScreenToolStripMenuItem_Click);
            tvScreenMenuItem = new ToolStripMenuItem("TV Screen", null, TVScreenToolStripMenuItem_Click);
            helpToolStripMenuItem = new ToolStripMenuItem();
            
            ((System.ComponentModel.ISupportInitialize)nmrLevel).BeginInit();
            menuStrip.SuspendLayout();
            SuspendLayout();
            
            // 
            // menuStrip
            // 
            menuStrip.ImageScalingSize = new Size(20, 20);
            menuStrip.Items.AddRange(new ToolStripItem[] {
                gameToolStripMenuItem,
                screensToolStripMenuItem,
                helpToolStripMenuItem});
            menuStrip.Location = new Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Size = new Size(880, 28);
            menuStrip.TabIndex = 0;
            
            // 
            // gameToolStripMenuItem
            // 
            gameToolStripMenuItem.Name = "gameToolStripMenuItem";
            gameToolStripMenuItem.Size = new Size(62, 24);
            gameToolStripMenuItem.Text = "Game";
            #if DEBUG
            gameToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("Database", null, DatabaseToolStripMenuItem_Click),
                new ToolStripMenuItem("Editor", null, QuestionsEditorToolStripMenuItem_Click),
                new ToolStripMenuItem("Settings", null, OptionsToolStripMenuItem_Click),
                new ToolStripSeparator(),
                new ToolStripMenuItem("🔊 DSP Test (Audio Queue & Silence Detection)", null, DSPTestToolStripMenuItem_Click),
                new ToolStripSeparator(),
                new ToolStripMenuItem("Exit", null, CloseToolStripMenuItem_Click)
            });
            #else
            gameToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("Database", null, DatabaseToolStripMenuItem_Click),
                new ToolStripMenuItem("Editor", null, QuestionsEditorToolStripMenuItem_Click),
                new ToolStripMenuItem("Settings", null, OptionsToolStripMenuItem_Click),
                new ToolStripSeparator(),
                new ToolStripMenuItem("Exit", null, CloseToolStripMenuItem_Click)
            });
            #endif
            
            // 
            // screensToolStripMenuItem
            // 
            screensToolStripMenuItem.Name = "screensToolStripMenuItem";
            screensToolStripMenuItem.Size = new Size(72, 24);
            screensToolStripMenuItem.Text = "Screens";
            screensToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
                hostScreenMenuItem,
                guestScreenMenuItem,
                new ToolStripSeparator(),
                tvScreenMenuItem,
                new ToolStripSeparator(),
                new ToolStripMenuItem("Preview Screen", null, PreviewScreenToolStripMenuItem_Click)
            });
            
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new Size(55, 24);
            helpToolStripMenuItem.Text = "Help";
            helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("Usage", null, UsageToolStripMenuItem_Click),
                new ToolStripMenuItem("Check for Updates", null, CheckUpdatesToolStripMenuItem_Click) { Enabled = false },
                new ToolStripSeparator(),
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
            // lblExplanationLabel
            // 
            lblExplanationLabel.AutoSize = true;
            lblExplanationLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblExplanationLabel.Location = new Point(12, 410);
            lblExplanationLabel.Name = "lblExplanationLabel";
            lblExplanationLabel.Size = new Size(75, 15);
            lblExplanationLabel.TabIndex = 0;
            lblExplanationLabel.Text = "Explanation:";
            
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
            btnA.BackColor = Color.Gray;
            btnA.FlatAppearance.BorderColor = Color.Black;
            btnA.FlatAppearance.BorderSize = 2;
            btnA.FlatStyle = FlatStyle.Flat;
            btnA.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnA.Location = new Point(12, 120);
            btnA.Name = "btnA";
            btnA.Size = new Size(60, 48);
            btnA.TabIndex = 6;
            btnA.Text = "A";
            btnA.UseVisualStyleBackColor = false;            btnA.Enabled = false;            btnA.Click += btnA_Click;
            
            // 
            // btnB
            // 
            btnB.BackColor = Color.Gray;
            btnB.FlatAppearance.BorderColor = Color.Black;
            btnB.FlatAppearance.BorderSize = 2;
            btnB.FlatStyle = FlatStyle.Flat;
            btnB.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnB.Location = new Point(502, 120);
            btnB.Name = "btnB";
            btnB.Size = new Size(60, 48);
            btnB.TabIndex = 7;
            btnB.Text = "B";
            btnB.UseVisualStyleBackColor = false;            btnB.Enabled = false;            btnB.Click += btnB_Click;
            
            // 
            // btnC
            // 
            btnC.BackColor = Color.Gray;
            btnC.FlatAppearance.BorderColor = Color.Black;
            btnC.FlatAppearance.BorderSize = 2;
            btnC.FlatStyle = FlatStyle.Flat;
            btnC.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnC.Location = new Point(12, 180);
            btnC.Name = "btnC";
            btnC.Size = new Size(60, 48);
            btnC.TabIndex = 8;
            btnC.Text = "C";
            btnC.UseVisualStyleBackColor = false;            btnC.Enabled = false;            btnC.Click += btnC_Click;
            
            // 
            // btnD
            // 
            btnD.BackColor = Color.Gray;
            btnD.FlatAppearance.BorderColor = Color.Black;
            btnD.FlatAppearance.BorderSize = 2;
            btnD.FlatStyle = FlatStyle.Flat;
            btnD.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnD.Location = new Point(502, 180);
            btnD.Name = "btnD";
            btnD.Size = new Size(60, 48);
            btnD.TabIndex = 9;
            btnD.Text = "D";
            btnD.UseVisualStyleBackColor = false;            btnD.Enabled = false;            btnD.Click += btnD_Click;
            
            // 
            // btnHostIntro
            // 
            btnHostIntro.BackColor = Color.LimeGreen;
            btnHostIntro.FlatAppearance.BorderColor = Color.Black;
            btnHostIntro.FlatAppearance.BorderSize = 2;
            btnHostIntro.FlatStyle = FlatStyle.Flat;
            btnHostIntro.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnHostIntro.ForeColor = Color.Black;
            btnHostIntro.Location = new Point(580, 40);
            btnHostIntro.Name = "btnHostIntro";
            btnHostIntro.Size = new Size(120, 45);
            btnHostIntro.TabIndex = 10;
            btnHostIntro.Text = "Host Intro";
            btnHostIntro.UseVisualStyleBackColor = false;
            btnHostIntro.Click += btnHostIntro_Click;
            
            // 
            // btnPickPlayer
            // 
            btnPickPlayer.BackColor = Color.Gray;
            btnPickPlayer.Enabled = false;
            btnPickPlayer.FlatAppearance.BorderColor = Color.Black;
            btnPickPlayer.FlatAppearance.BorderSize = 2;
            btnPickPlayer.FlatStyle = FlatStyle.Flat;
            btnPickPlayer.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnPickPlayer.ForeColor = Color.White;
            btnPickPlayer.Location = new Point(710, 40);
            btnPickPlayer.Name = "btnPickPlayer";
            btnPickPlayer.Size = new Size(120, 45);
            btnPickPlayer.TabIndex = 11;
            btnPickPlayer.Text = "Pick Player";
            btnPickPlayer.UseVisualStyleBackColor = false;
            btnPickPlayer.Click += btnPickPlayer_Click;
            
            // 
            // btnExplainGame
            // 
            btnExplainGame.BackColor = Color.Gray;
            btnExplainGame.Enabled = false;
            btnExplainGame.FlatAppearance.BorderColor = Color.Black;
            btnExplainGame.FlatAppearance.BorderSize = 2;
            btnExplainGame.FlatStyle = FlatStyle.Flat;
            btnExplainGame.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnExplainGame.ForeColor = Color.White;
            btnExplainGame.Location = new Point(710, 90);
            btnExplainGame.Name = "btnExplainGame";
            btnExplainGame.Size = new Size(120, 45);
            btnExplainGame.TabIndex = 12;
            btnExplainGame.Text = "Explain Game";
            btnExplainGame.UseVisualStyleBackColor = false;
            btnExplainGame.Click += btnExplainGame_Click;
            
            // 
            // btnLightsDown
            // 
            btnLightsDown.BackColor = Color.Gray;
            btnLightsDown.Enabled = false;
            btnLightsDown.FlatAppearance.BorderColor = Color.Black;
            btnLightsDown.FlatAppearance.BorderSize = 2;
            btnLightsDown.FlatStyle = FlatStyle.Flat;
            btnLightsDown.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnLightsDown.Location = new Point(580, 90);
            btnLightsDown.Name = "btnLightsDown";
            btnLightsDown.Size = new Size(120, 45);
            btnLightsDown.TabIndex = 13;
            btnLightsDown.Text = "Lights Down";
            btnLightsDown.UseVisualStyleBackColor = false;
            btnLightsDown.Click += btnLightsDown_Click;
            
            // 
            // btnNewQuestion
            // 
            btnNewQuestion.BackColor = Color.Gray;
            btnNewQuestion.Enabled = false;
            btnNewQuestion.FlatAppearance.BorderColor = Color.Black;
            btnNewQuestion.FlatAppearance.BorderSize = 2;
            btnNewQuestion.FlatStyle = FlatStyle.Flat;
            btnNewQuestion.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnNewQuestion.ForeColor = Color.White;
            btnNewQuestion.Location = new Point(580, 140);
            btnNewQuestion.Name = "btnNewQuestion";
            btnNewQuestion.Size = new Size(120, 45);
            btnNewQuestion.TabIndex = 14;
            btnNewQuestion.Text = "Question";
            btnNewQuestion.UseVisualStyleBackColor = false;
            btnNewQuestion.Click += btnNewQuestion_Click;
            
            // 
            // btnReveal
            // 
            btnReveal.BackColor = Color.Gray;
            btnReveal.Enabled = false;
            btnReveal.FlatAppearance.BorderColor = Color.Black;
            btnReveal.FlatAppearance.BorderSize = 2;
            btnReveal.FlatStyle = FlatStyle.Flat;
            btnReveal.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnReveal.Location = new Point(710, 140);
            btnReveal.Name = "btnReveal";
            btnReveal.Size = new Size(120, 45);
            btnReveal.TabIndex = 15;
            btnReveal.Text = "Reveal";
            btnReveal.UseVisualStyleBackColor = false;
            btnReveal.Click += btnReveal_Click;
            
            // 
            // btnWalk
            // 
            btnWalk.BackColor = Color.Gray;
            btnWalk.Enabled = false;
            btnWalk.FlatAppearance.BorderColor = Color.Black;
            btnWalk.FlatAppearance.BorderSize = 2;
            btnWalk.FlatStyle = FlatStyle.Flat;
            btnWalk.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnWalk.Location = new Point(580, 190);
            btnWalk.Name = "btnWalk";
            btnWalk.Size = new Size(120, 45);
            btnWalk.TabIndex = 16;
            btnWalk.Text = "Walk Away";
            btnWalk.UseVisualStyleBackColor = false;
            btnWalk.Click += btnWalk_Click;
            
            // 
            // btnClosing
            // 
            btnClosing.BackColor = Color.Gray;
            btnClosing.Enabled = false;
            btnClosing.FlatAppearance.BorderColor = Color.Black;
            btnClosing.FlatAppearance.BorderSize = 2;
            btnClosing.FlatStyle = FlatStyle.Flat;
            btnClosing.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnClosing.ForeColor = Color.White;
            btnClosing.Location = new Point(710, 190);
            btnClosing.Name = "btnClosing";
            btnClosing.Size = new Size(120, 45);
            btnClosing.TabIndex = 17;
            btnClosing.Text = "Closing";
            btnClosing.UseVisualStyleBackColor = false;
            btnClosing.Click += btnClosing_Click;
            
            // 
            // btnFadeOutAudio
            // 
            btnFadeOutAudio.BackColor = Color.DarkRed;
            btnFadeOutAudio.FlatAppearance.BorderColor = Color.Black;
            btnFadeOutAudio.FlatAppearance.BorderSize = 2;
            btnFadeOutAudio.FlatStyle = FlatStyle.Flat;
            btnFadeOutAudio.ForeColor = Color.White;
            btnFadeOutAudio.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnFadeOutAudio.Location = new Point(580, 245);
            btnFadeOutAudio.Name = "btnFadeOutAudio";
            btnFadeOutAudio.Size = new Size(250, 45);
            btnFadeOutAudio.TabIndex = 19;
            btnFadeOutAudio.Text = "🔉 FADE OUT ALL SOUNDS";
            btnFadeOutAudio.UseVisualStyleBackColor = false;
            btnFadeOutAudio.Click += btnFadeOutAudio_Click;
            
            // 
            // btnStopAudio
            // 
            btnStopAudio.BackColor = Color.DarkRed;
            btnStopAudio.FlatAppearance.BorderColor = Color.Black;
            btnStopAudio.FlatAppearance.BorderSize = 2;
            btnStopAudio.FlatStyle = FlatStyle.Flat;
            btnStopAudio.ForeColor = Color.White;
            btnStopAudio.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnStopAudio.Location = new Point(580, 295);
            btnStopAudio.Name = "btnStopAudio";
            btnStopAudio.Size = new Size(250, 45);
            btnStopAudio.TabIndex = 20;
            btnStopAudio.Text = "⏹ STOP ALL AUDIO";
            btnStopAudio.UseVisualStyleBackColor = false;
            btnStopAudio.Click += btnStopAudio_Click;
            
            // 
            // btnResetRound
            // 
            InitializeStopImages();
            btnResetRound.BackColor = Color.Gray;
            btnResetRound.FlatAppearance.BorderColor = Color.Black;
            btnResetRound.FlatAppearance.BorderSize = 2;
            btnResetRound.FlatStyle = FlatStyle.Flat;
            btnResetRound.ForeColor = Color.Black;
            btnResetRound.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnResetRound.Location = new Point(580, 535);
            btnResetRound.Name = "btnResetRound";
            btnResetRound.Size = new Size(120, 45);
            btnResetRound.TabIndex = 50;
            btnResetRound.Text = "Round";
            btnResetRound.TextAlign = ContentAlignment.MiddleCenter;
            btnResetRound.ImageAlign = ContentAlignment.MiddleCenter;
            btnResetRound.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnResetRound.Image = _stopImageWhite;
            btnResetRound.UseVisualStyleBackColor = false;
            btnResetRound.Enabled = false;
            btnResetRound.Click += btnResetRound_Click;
            
            // 
            // btnResetGame
            // 
            btnResetGame.BackColor = Color.Gray;
            btnResetGame.FlatAppearance.BorderColor = Color.Black;
            btnResetGame.FlatAppearance.BorderSize = 2;
            btnResetGame.FlatStyle = FlatStyle.Flat;
            btnResetGame.ForeColor = Color.Black;
            btnResetGame.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnResetGame.Location = new Point(710, 535);
            btnResetGame.Name = "btnResetGame";
            btnResetGame.Size = new Size(120, 45);
            btnResetGame.TabIndex = 51;
            btnResetGame.Text = "Game";
            btnResetGame.TextAlign = ContentAlignment.MiddleCenter;
            btnResetGame.ImageAlign = ContentAlignment.MiddleCenter;
            btnResetGame.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnResetGame.Image = _stopImageWhite;
            btnResetGame.UseVisualStyleBackColor = false;
            btnResetGame.Enabled = false;
            btnResetGame.Click += btnResetGame_Click;
            
            // 
            // nmrLevel
            // 
            nmrLevel.Font = new Font("Segoe UI", 11F);
            nmrLevel.Location = new Point(120, 245);
            nmrLevel.Maximum = new decimal(new int[] { 15, 0, 0, 0 });
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
            txtID.Location = new Point(840, 376);
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
            lblDropLabel.Text = "If Walk:";
            
            // 
            // lblQLeftLabel
            // 
            lblQLeftLabel.AutoSize = true;
            lblQLeftLabel.Location = new Point(227, 326);
            lblQLeftLabel.Name = "lblQLeftLabel";
            lblQLeftLabel.Size = new Size(111, 20);
            lblQLeftLabel.TabIndex = 35;
            lblQLeftLabel.Text = "Q's left:";
            
            // 
            // btnLifeline1
            // 
            btnLifeline1.BackColor = Color.DarkOrange;
            btnLifeline1.FlatAppearance.BorderColor = Color.Black;
            btnLifeline1.FlatAppearance.BorderSize = 2;
            btnLifeline1.FlatStyle = FlatStyle.Flat;
            btnLifeline1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnLifeline1.Location = new Point(580, 350);
            btnLifeline1.Name = "btnLifeline1";
            btnLifeline1.Size = new Size(120, 40);
            btnLifeline1.TabIndex = 36;
            btnLifeline1.Text = "";
            btnLifeline1.UseVisualStyleBackColor = false;
            btnLifeline1.Click += btnLifeline1_Click;
            
            // 
            // btnLifeline2
            // 
            btnLifeline2.BackColor = Color.DarkOrange;
            btnLifeline2.FlatAppearance.BorderColor = Color.Black;
            btnLifeline2.FlatAppearance.BorderSize = 2;
            btnLifeline2.FlatStyle = FlatStyle.Flat;
            btnLifeline2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnLifeline2.Location = new Point(580, 395);
            btnLifeline2.Name = "btnLifeline2";
            btnLifeline2.Size = new Size(120, 40);
            btnLifeline2.TabIndex = 37;
            btnLifeline2.Text = "";
            btnLifeline2.UseVisualStyleBackColor = false;
            btnLifeline2.Click += btnLifeline2_Click;
            
            // 
            // btnLifeline3
            // 
            btnLifeline3.BackColor = Color.DarkOrange;
            btnLifeline3.FlatAppearance.BorderColor = Color.Black;
            btnLifeline3.FlatAppearance.BorderSize = 2;
            btnLifeline3.FlatStyle = FlatStyle.Flat;
            btnLifeline3.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnLifeline3.Location = new Point(580, 440);
            btnLifeline3.Name = "btnLifeline3";
            btnLifeline3.Size = new Size(120, 40);
            btnLifeline3.TabIndex = 38;
            btnLifeline3.Text = "";
            btnLifeline3.UseVisualStyleBackColor = false;
            btnLifeline3.Click += btnLifeline3_Click;
            
            // 
            // btnLifeline4
            // 
            btnLifeline4.BackColor = Color.DarkOrange;
            btnLifeline4.FlatAppearance.BorderColor = Color.Black;
            btnLifeline4.FlatAppearance.BorderSize = 2;
            btnLifeline4.FlatStyle = FlatStyle.Flat;
            btnLifeline4.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnLifeline4.Location = new Point(580, 485);
            btnLifeline4.Name = "btnLifeline4";
            btnLifeline4.Size = new Size(120, 40);
            btnLifeline4.TabIndex = 39;
            btnLifeline4.Text = "";
            btnLifeline4.UseVisualStyleBackColor = false;
            btnLifeline4.Click += btnLifeline4_Click;
            
            // 
            // chkShowQuestion
            // 
            chkShowQuestion.AutoSize = true;
            chkShowQuestion.Checked = false;
            chkShowQuestion.CheckState = CheckState.Unchecked;
            chkShowQuestion.Location = new Point(12, 618);
            chkShowQuestion.Name = "chkShowQuestion";
            chkShowQuestion.Size = new Size(190, 24);
            chkShowQuestion.TabIndex = 40;
            chkShowQuestion.Text = "Show Question on TV";
            chkShowQuestion.UseVisualStyleBackColor = true;
            chkShowQuestion.CheckedChanged += chkShowQuestion_CheckedChanged;
            
            // 
            // chkShowWinnings
            // 
            chkShowWinnings.AutoSize = true;
            chkShowWinnings.Checked = false;
            chkShowWinnings.CheckState = CheckState.Unchecked;
            chkShowWinnings.Location = new Point(220, 618);
            chkShowWinnings.Name = "chkShowWinnings";
            chkShowWinnings.Size = new Size(200, 24);
            chkShowWinnings.TabIndex = 43;
            chkShowWinnings.Text = "Show Current Winnings";
            chkShowWinnings.UseVisualStyleBackColor = true;
            chkShowWinnings.CheckedChanged += chkShowWinnings_CheckedChanged;
            
            // 
            // chkCorrectAnswer
            // 
            chkCorrectAnswer.AutoSize = true;
            chkCorrectAnswer.Location = new Point(440, 618);
            chkCorrectAnswer.Name = "chkCorrectAnswer";
            chkCorrectAnswer.Size = new Size(235, 24);
            chkCorrectAnswer.TabIndex = 41;
            chkCorrectAnswer.Text = "Show Correct Answer to Host";
            chkCorrectAnswer.UseVisualStyleBackColor = true;
            chkCorrectAnswer.CheckedChanged += chkCorrectAnswer_CheckedChanged;
            
            // 
            // btnShowMoneyTree
            // 
            btnShowMoneyTree.BackColor = Color.Gray;
            btnShowMoneyTree.Enabled = false;
            btnShowMoneyTree.FlatAppearance.BorderColor = Color.Black;
            btnShowMoneyTree.FlatAppearance.BorderSize = 2;
            btnShowMoneyTree.FlatStyle = FlatStyle.Flat;
            btnShowMoneyTree.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnShowMoneyTree.ForeColor = Color.White;
            btnShowMoneyTree.Location = new Point(710, 350);
            btnShowMoneyTree.Name = "btnShowMoneyTree";
            btnShowMoneyTree.Size = new Size(120, 85);
            btnShowMoneyTree.TabIndex = 42;
            btnShowMoneyTree.Text = "Show Money Tree";
            btnShowMoneyTree.UseVisualStyleBackColor = false;
            btnShowMoneyTree.Click += btnShowMoneyTree_Click;
            
            // 
            // btnActivateRiskMode
            // 
            btnActivateRiskMode.BackColor = Color.Gray;
            btnActivateRiskMode.Enabled = false;
            btnActivateRiskMode.FlatAppearance.BorderColor = Color.Black;
            btnActivateRiskMode.FlatAppearance.BorderSize = 2;
            btnActivateRiskMode.FlatStyle = FlatStyle.Flat;
            btnActivateRiskMode.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnActivateRiskMode.Location = new Point(710, 440);
            btnActivateRiskMode.Name = "btnActivateRiskMode";
            btnActivateRiskMode.Size = new Size(120, 85);
            btnActivateRiskMode.TabIndex = 43;
            btnActivateRiskMode.Text = "Activate Risk Mode";
            btnActivateRiskMode.UseVisualStyleBackColor = false;
            btnActivateRiskMode.Click += btnActivateRiskMode_Click;

            // 
            // lblHostMessage
            // 
            lblHostMessage.AutoSize = true;
            lblHostMessage.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblHostMessage.Location = new Point(12, 510);
            lblHostMessage.Name = "lblHostMessage";
            lblHostMessage.Size = new Size(71, 15);
            lblHostMessage.TabIndex = 44;
            lblHostMessage.Text = "Host Note:";
            
            // 
            // txtHostMessage
            // 
            txtHostMessage.Location = new Point(12, 535);
            txtHostMessage.Multiline = true;
            txtHostMessage.Name = "txtHostMessage";
            txtHostMessage.Size = new Size(470, 64);
            txtHostMessage.TabIndex = 45;
            txtHostMessage.AcceptsReturn = false;
            txtHostMessage.KeyDown += txtHostMessage_KeyDown;
            
            // 
            // btnSendHostMessage
            // 
            btnSendHostMessage.BackColor = Color.FromArgb(70, 130, 180);
            btnSendHostMessage.FlatStyle = FlatStyle.Flat;
            btnSendHostMessage.ForeColor = Color.White;
            btnSendHostMessage.Location = new Point(487, 535);
            btnSendHostMessage.Name = "btnSendHostMessage";
            btnSendHostMessage.Size = new Size(75, 32);
            btnSendHostMessage.TabIndex = 46;
            btnSendHostMessage.Text = "Send";
            btnSendHostMessage.UseVisualStyleBackColor = false;
            btnSendHostMessage.Click += btnSendHostMessage_Click;
            
            // 
            // btnClearHostMessage
            // 
            btnClearHostMessage.BackColor = Color.FromArgb(180, 70, 70);
            btnClearHostMessage.FlatStyle = FlatStyle.Flat;
            btnClearHostMessage.ForeColor = Color.White;
            btnClearHostMessage.Location = new Point(487, 567);
            btnClearHostMessage.Name = "btnClearHostMessage";
            btnClearHostMessage.Size = new Size(75, 32);
            btnClearHostMessage.TabIndex = 47;
            btnClearHostMessage.Text = "Clear";
            btnClearHostMessage.UseVisualStyleBackColor = false;
            btnClearHostMessage.Click += btnClearHostMessage_Click;
            
            // 
            // ControlPanelForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(880, 660);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Controls.Add(btnShowMoneyTree);
            Controls.Add(btnActivateRiskMode);
            Controls.Add(chkCorrectAnswer);
            Controls.Add(chkShowWinnings);
            Controls.Add(chkShowQuestion);
            Controls.Add(lblHostMessage);
            Controls.Add(txtHostMessage);
            Controls.Add(btnSendHostMessage);
            Controls.Add(btnClearHostMessage);
            Controls.Add(btnLifeline4);
            Controls.Add(btnLifeline3);
            Controls.Add(btnLifeline2);
            Controls.Add(btnLifeline1);
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
            Controls.Add(btnResetRound);
            Controls.Add(btnResetGame);
            Controls.Add(btnFadeOutAudio);
            Controls.Add(btnStopAudio);
            Controls.Add(btnClosing);
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
            Controls.Add(lblExplanationLabel);
            Controls.Add(txtExplain);
            Controls.Add(txtD);
            Controls.Add(txtC);
            Controls.Add(txtB);
            Controls.Add(txtA);
            Controls.Add(txtQuestion);
            Controls.Add(menuStrip);
            MainMenuStrip = menuStrip;
            Name = "ControlPanelForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Millionaire Game";
            Load += ControlPanelForm_Load;
            ((System.ComponentModel.ISupportInitialize)nmrLevel).EndInit();
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip;
        private ToolStripMenuItem gameToolStripMenuItem;
        private ToolStripMenuItem screensToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem hostScreenMenuItem;
        private ToolStripMenuItem guestScreenMenuItem;
        private ToolStripMenuItem tvScreenMenuItem;
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
        private Label lblExplanationLabel;
        private Button btnHostIntro;
        private Button btnPickPlayer;
        private Button btnExplainGame;
        private Button btnLightsDown;
        private Button btnNewQuestion;
        private Button btnReveal;
        private Button btnWalk;
        private Button btnClosing;
        private Button btnFadeOutAudio;
        private Button btnStopAudio;
        private Button btnResetRound;
        private Button btnResetGame;
        private Button btnA;
        private Button btnB;
        private Button btnC;
        private Button btnD;
        private Button btnActivateRiskMode;
        private Button btnShowMoneyTree;
        private Button btnLifeline1;
        private Button btnLifeline2;
        private Button btnLifeline3;
        private Button btnLifeline4;
        private NumericUpDown nmrLevel;
        private CheckBox chkShowQuestion;
        private CheckBox chkShowWinnings;
        private CheckBox chkCorrectAnswer;
        
        // Host messaging controls
        private Label lblHostMessage;
        private TextBox txtHostMessage;
        private Button btnSendHostMessage;
        private Button btnClearHostMessage;
        
        private static Image? _stopImageNormal;
        private static Image? _stopImageWhite;
        
        private static Image? LoadEmbeddedImage(string fileName)
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                string resourceName = $"MillionaireGame.lib.textures.{fileName}";
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream != null)
                {
                    return Image.FromStream(stream);
                }
            }
            catch (Exception ex)
            {
                if (Program.DebugMode)
                {
                    Console.WriteLine($"[ControlPanelForm] Error loading image '{fileName}': {ex.Message}");
                }
            }
            return null;
        }
        
        private static Image? CreateBlackImage(Image? source)
        {
            if (source == null) return null;
            
            var bitmap = new Bitmap(source.Width, source.Height);
            using (var g = System.Drawing.Graphics.FromImage(bitmap))
            {
                // Create color matrix to convert to black
                var colorMatrix = new System.Drawing.Imaging.ColorMatrix(new float[][]
                {
                    new float[] {0, 0, 0, 0, 0},
                    new float[] {0, 0, 0, 0, 0},
                    new float[] {0, 0, 0, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
                });
                
                var imageAttributes = new System.Drawing.Imaging.ImageAttributes();
                imageAttributes.SetColorMatrix(colorMatrix);
                
                g.DrawImage(source,
                    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    0, 0, source.Width, source.Height,
                    GraphicsUnit.Pixel,
                    imageAttributes);
            }
            return bitmap;
        }
        
        private static Image? ResizeImage(Image? source, int width, int height)
        {
            if (source == null) return null;
            
            var bitmap = new Bitmap(width, height);
            using (var g = System.Drawing.Graphics.FromImage(bitmap))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(source, 0, 0, width, height);
            }
            return bitmap;
        }
        
        private static void InitializeStopImages()
        {
            if (_stopImageNormal == null)
            {
                var originalImage = LoadEmbeddedImage("stop.png");
                // Resize to 20x20 for inline display
                var resizedImage = ResizeImage(originalImage, 20, 20);
                _stopImageNormal = resizedImage; // Keep original red color for disabled state
                _stopImageWhite = CreateBlackImage(resizedImage); // Black for enabled state
            }
        }
    }
}
