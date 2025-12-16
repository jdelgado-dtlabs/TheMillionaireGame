namespace MillionaireGame.Forms
{
    partial class ControlPanelForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Unsubscribe from events
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

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            // Main question controls
            txtQuestion = new TextBox();
            txtA = new TextBox();
            txtB = new TextBox();
            txtC = new TextBox();
            txtD = new TextBox();
            txtExplain = new TextBox();
            
            // Labels
            lblAnswer = new Label();
            Label lblCorrectLabel = new Label();
            Label lblCurrentLabel = new Label();
            Label lblWrongLabel = new Label();
            Label lblDropLabel = new Label();
            Label lblQLeftLabel = new Label();
            Label lblLevelLabel = new Label();
            
            // Money display
            txtCorrect = new TextBox();
            txtCurrent = new TextBox();
            txtWrong = new TextBox();
            txtDrop = new TextBox();
            txtQLeft = new TextBox();
            txtID = new TextBox();
            
            // Buttons
            btnNewQuestion = new Button();
            btnA = new Button();
            btnB = new Button();
            btnC = new Button();
            btnD = new Button();
            btnLightsDown = new Button();
            btnReveal = new Button();
            btnWalk = new Button();
            btnActivateRiskMode = new Button();
            btnResetGame = new Button();
            btnToHotSeat = new Button();
            
            // Numeric control
            nmrLevel = new NumericUpDown();
            
            // Checkboxes
            chkShowQuestion = new CheckBox();
            chkCorrectAnswer = new CheckBox();
            
            // Menu
            var menuStrip = new MenuStrip();
            var databaseMenuItem = new ToolStripMenuItem("Database");
            var gameMenuItem = new ToolStripMenuItem("Game");
            var viewMenuItem = new ToolStripMenuItem("View");
            var helpMenuItem = new ToolStripMenuItem("Help");
            
            // Initialize components
            SuspendLayout();
            
            // txtQuestion
            txtQuestion.Location = new Point(12, 40);
            txtQuestion.Multiline = true;
            txtQuestion.Name = "txtQuestion";
            txtQuestion.Size = new Size(500, 60);
            txtQuestion.ReadOnly = true;
            txtQuestion.ScrollBars = ScrollBars.Vertical;
            
            // Answer boxes
            txtA.Location = new Point(12, 110);
            txtA.Name = "txtA";
            txtA.Size = new Size(240, 25);
            txtA.ReadOnly = true;
            txtA.BackColor = Color.Silver;
            
            txtB.Location = new Point(272, 110);
            txtB.Name = "txtB";
            txtB.Size = new Size(240, 25);
            txtB.ReadOnly = true;
            txtB.BackColor = Color.Silver;
            
            txtC.Location = new Point(12, 145);
            txtC.Name = "txtC";
            txtC.Size = new Size(240, 25);
            txtC.ReadOnly = true;
            txtC.BackColor = Color.Silver;
            
            txtD.Location = new Point(272, 145);
            txtD.Name = "txtD";
            txtD.Size = new Size(240, 25);
            txtD.ReadOnly = true;
            txtD.BackColor = Color.Silver;
            
            // Answer buttons
            btnA.Location = new Point(12, 180);
            btnA.Name = "btnA";
            btnA.Size = new Size(115, 35);
            btnA.Text = "A";
            btnA.UseVisualStyleBackColor = true;
            btnA.Click += btnA_Click;
            
            btnB.Location = new Point(137, 180);
            btnB.Name = "btnB";
            btnB.Size = new Size(115, 35);
            btnB.Text = "B";
            btnB.UseVisualStyleBackColor = true;
            btnB.Click += btnB_Click;
            
            btnC.Location = new Point(272, 180);
            btnC.Name = "btnC";
            btnC.Size = new Size(115, 35);
            btnC.Text = "C";
            btnC.UseVisualStyleBackColor = true;
            btnC.Click += btnC_Click;
            
            btnD.Location = new Point(397, 180);
            btnD.Name = "btnD";
            btnD.Size = new Size(115, 35);
            btnD.Text = "D";
            btnD.UseVisualStyleBackColor = true;
            btnD.Click += btnD_Click;
            
            // Control buttons
            btnNewQuestion.Location = new Point(12, 230);
            btnNewQuestion.Name = "btnNewQuestion";
            btnNewQuestion.Size = new Size(120, 40);
            btnNewQuestion.Text = "New Question (F5)";
            btnNewQuestion.UseVisualStyleBackColor = true;
            btnNewQuestion.Click += btnNewQuestion_Click;
            
            btnLightsDown.Location = new Point(142, 230);
            btnLightsDown.Name = "btnLightsDown";
            btnLightsDown.Size = new Size(120, 40);
            btnLightsDown.Text = "Lights Down (F7)";
            btnLightsDown.UseVisualStyleBackColor = true;
            btnLightsDown.Click += btnLightsDown_Click;
            
            btnReveal.Location = new Point(272, 230);
            btnReveal.Name = "btnReveal";
            btnReveal.Size = new Size(120, 40);
            btnReveal.Text = "Reveal (F6)";
            btnReveal.UseVisualStyleBackColor = true;
            btnReveal.Click += btnReveal_Click;
            
            btnWalk.Location = new Point(402, 230);
            btnWalk.Name = "btnWalk";
            btnWalk.Size = new Size(110, 40);
            btnWalk.Text = "Walk Away";
            btnWalk.UseVisualStyleBackColor = true;
            btnWalk.Click += btnWalk_Click;
            
            // Level control
            lblLevelLabel.Location = new Point(540, 40);
            lblLevelLabel.Name = "lblLevelLabel";
            lblLevelLabel.Size = new Size(100, 23);
            lblLevelLabel.Text = "Current Level:";
            lblLevelLabel.TextAlign = ContentAlignment.MiddleRight;
            
            nmrLevel.Location = new Point(650, 40);
            nmrLevel.Name = "nmrLevel";
            nmrLevel.Size = new Size(80, 25);
            nmrLevel.Maximum = 15;
            nmrLevel.Minimum = 0;
            nmrLevel.Value = 0;
            nmrLevel.ValueChanged += nmrLevel_ValueChanged;
            
            // Money display
            lblCorrectLabel.Location = new Point(540, 80);
            lblCorrectLabel.Size = new Size(100, 23);
            lblCorrectLabel.Text = "If Correct:";
            lblCorrectLabel.TextAlign = ContentAlignment.MiddleRight;
            
            txtCorrect.Location = new Point(650, 80);
            txtCorrect.Size = new Size(100, 25);
            txtCorrect.ReadOnly = true;
            
            lblCurrentLabel.Location = new Point(540, 110);
            lblCurrentLabel.Size = new Size(100, 23);
            lblCurrentLabel.Text = "Current:";
            lblCurrentLabel.TextAlign = ContentAlignment.MiddleRight;
            
            txtCurrent.Location = new Point(650, 110);
            txtCurrent.Size = new Size(100, 25);
            txtCurrent.ReadOnly = true;
            
            lblWrongLabel.Location = new Point(540, 140);
            lblWrongLabel.Size = new Size(100, 23);
            lblWrongLabel.Text = "If Wrong:";
            lblWrongLabel.TextAlign = ContentAlignment.MiddleRight;
            
            txtWrong.Location = new Point(650, 140);
            txtWrong.Size = new Size(100, 25);
            txtWrong.ReadOnly = true;
            
            lblDropLabel.Location = new Point(540, 170);
            lblDropLabel.Size = new Size(100, 23);
            lblDropLabel.Text = "If Drop:";
            lblDropLabel.TextAlign = ContentAlignment.MiddleRight;
            
            txtDrop.Location = new Point(650, 170);
            txtDrop.Size = new Size(100, 25);
            txtDrop.ReadOnly = true;
            
            lblQLeftLabel.Location = new Point(540, 200);
            lblQLeftLabel.Size = new Size(100, 23);
            lblQLeftLabel.Text = "Questions Left:";
            lblQLeftLabel.TextAlign = ContentAlignment.MiddleRight;
            
            txtQLeft.Location = new Point(650, 200);
            txtQLeft.Size = new Size(100, 25);
            txtQLeft.ReadOnly = true;
            
            // Correct answer display
            lblCorrectLabel = new Label();
            lblCorrectLabel.Location = new Point(540, 230);
            lblCorrectLabel.Size = new Size(100, 23);
            lblCorrectLabel.Text = "Correct Answer:";
            lblCorrectLabel.TextAlign = ContentAlignment.MiddleRight;
            
            lblAnswer.Location = new Point(650, 230);
            lblAnswer.Size = new Size(50, 30);
            lblAnswer.Text = "";
            lblAnswer.Font = new Font(lblAnswer.Font.FontFamily, 16, FontStyle.Bold);
            lblAnswer.ForeColor = Color.Green;
            
            // Explanation box
            txtExplain.Location = new Point(12, 280);
            txtExplain.Multiline = true;
            txtExplain.Name = "txtExplain";
            txtExplain.Size = new Size(500, 60);
            txtExplain.ScrollBars = ScrollBars.Vertical;
            txtExplain.ReadOnly = true;
            
            // Game controls
            btnActivateRiskMode.Location = new Point(540, 280);
            btnActivateRiskMode.Size = new Size(150, 35);
            btnActivateRiskMode.Text = "RISK MODE OFF";
            btnActivateRiskMode.BackColor = Color.Orange;
            btnActivateRiskMode.Click += btnActivateRiskMode_Click;
            
            btnResetGame.Location = new Point(540, 320);
            btnResetGame.Size = new Size(150, 35);
            btnResetGame.Text = "Reset Game";
            btnResetGame.Click += btnResetGame_Click;
            
            txtID.Location = new Point(700, 320);
            txtID.Size = new Size(50, 25);
            txtID.ReadOnly = true;
            txtID.Visible = false;
            
            // Checkboxes
            chkShowQuestion.Location = new Point(12, 350);
            chkShowQuestion.Size = new Size(200, 24);
            chkShowQuestion.Text = "Show Question on Screens";
            chkShowQuestion.Checked = true;
            
            chkCorrectAnswer.Location = new Point(220, 350);
            chkCorrectAnswer.Size = new Size(200, 24);
            chkCorrectAnswer.Text = "Show Correct Answer";
            
            // Lifeline buttons
            btn5050 = new Button();
            btn5050.Location = new Point(770, 80);
            btn5050.Size = new Size(120, 40);
            btn5050.Text = "50:50";
            btn5050.Font = new Font("Arial", 12F, FontStyle.Bold);
            btn5050.BackColor = Color.Orange;
            btn5050.Click += btn5050_Click;

            btnPhoneFriend = new Button();
            btnPhoneFriend.Location = new Point(770, 130);
            btnPhoneFriend.Size = new Size(120, 40);
            btnPhoneFriend.Text = "Phone Friend";
            btnPhoneFriend.Font = new Font("Arial", 10F, FontStyle.Bold);
            btnPhoneFriend.BackColor = Color.Orange;
            btnPhoneFriend.Click += btnPhoneFriend_Click;

            btnAskAudience = new Button();
            btnAskAudience.Location = new Point(770, 180);
            btnAskAudience.Size = new Size(120, 40);
            btnAskAudience.Text = "Ask Audience";
            btnAskAudience.Font = new Font("Arial", 10F, FontStyle.Bold);
            btnAskAudience.BackColor = Color.Orange;
            btnAskAudience.Click += btnAskAudience_Click;

            btnSwitch = new Button();
            btnSwitch.Location = new Point(770, 230);
            btnSwitch.Size = new Size(120, 40);
            btnSwitch.Text = "Switch Q";
            btnSwitch.Font = new Font("Arial", 10F, FontStyle.Bold);
            btnSwitch.BackColor = Color.Orange;
            btnSwitch.Click += btnSwitch_Click;
            
            // Menu strip
            databaseMenuItem.Text = "Database";
            databaseMenuItem.Click += DatabaseToolStripMenuItem_Click;
            
            var questionsEditorItem = new ToolStripMenuItem("Questions Editor");
            questionsEditorItem.Click += QuestionsEditorToolStripMenuItem_Click;
            gameMenuItem.DropDownItems.Add(questionsEditorItem);
            
            var optionsItem = new ToolStripMenuItem("Options");
            optionsItem.Click += OptionsToolStripMenuItem_Click;
            gameMenuItem.DropDownItems.Add(optionsItem);
            
            var hostScreenItem = new ToolStripMenuItem("Host Screen");
            hostScreenItem.Click += HostScreenToolStripMenuItem_Click;
            viewMenuItem.DropDownItems.Add(hostScreenItem);
            
            var guestScreenItem = new ToolStripMenuItem("Guest Screen");
            guestScreenItem.Click += GuestScreenToolStripMenuItem_Click;
            viewMenuItem.DropDownItems.Add(guestScreenItem);
            
            var tvScreenItem = new ToolStripMenuItem("TV Screen");
            tvScreenItem.Click += TVScreenToolStripMenuItem_Click;
            viewMenuItem.DropDownItems.Add(tvScreenItem);
            
            var closeItem = new ToolStripMenuItem("Close");
            closeItem.Click += CloseToolStripMenuItem_Click;
            databaseMenuItem.DropDownItems.Add(new ToolStripSeparator());
            databaseMenuItem.DropDownItems.Add(closeItem);
            
            var aboutItem = new ToolStripMenuItem("About");
            aboutItem.Click += AboutToolStripMenuItem_Click;
            helpMenuItem.DropDownItems.Add(aboutItem);
            
            menuStrip.Items.Add(databaseMenuItem);
            menuStrip.Items.Add(gameMenuItem);
            menuStrip.Items.Add(viewMenuItem);
            menuStrip.Items.Add(helpMenuItem);
            
            // ControlPanelForm
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 400);
            Controls.Add(menuStrip);
            Controls.Add(txtQuestion);
            Controls.Add(txtA);
            Controls.Add(txtB);
            Controls.Add(txtC);
            Controls.Add(txtD);
            Controls.Add(btnA);
            Controls.Add(btnB);
            Controls.Add(btnC);
            Controls.Add(btnD);
            Controls.Add(btnNewQuestion);
            Controls.Add(btnLightsDown);
            Controls.Add(btnReveal);
            Controls.Add(btnWalk);
            Controls.Add(lblLevelLabel);
            Controls.Add(nmrLevel);
            Controls.Add(lblCorrectLabel);
            Controls.Add(txtCorrect);
            Controls.Add(lblCurrentLabel);
            Controls.Add(txtCurrent);
            Controls.Add(lblWrongLabel);
            Controls.Add(txtWrong);
            Controls.Add(lblDropLabel);
            Controls.Add(txtDrop);
            Controls.Add(lblQLeftLabel);
            Controls.Add(txtQLeft);
            Controls.Add(lblAnswer);
            Controls.Add(txtExplain);
            Controls.Add(btnActivateRiskMode);
            Controls.Add(btnResetGame);
            Controls.Add(txtID);
            Controls.Add(chkShowQuestion);
            Controls.Add(chkCorrectAnswer);
            Controls.Add(btn5050);
            Controls.Add(btnPhoneFriend);
            Controls.Add(btnAskAudience);
            Controls.Add(btnSwitch);
            
            MainMenuStrip = menuStrip;
            Name = "ControlPanelForm";
            Text = "The Millionaire Game - Control Panel";
            Load += ControlPanelForm_Load;
            
            ((System.ComponentModel.ISupportInitialize)nmrLevel).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

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
        private Button btnNewQuestion;
        private Button btnA;
        private Button btnB;
        private Button btnC;
        private Button btnD;
        private Button btnLightsDown;
        private Button btnReveal;
        private Button btnWalk;
        private Button btnActivateRiskMode;
        private Button btnResetGame;
        private Button btnToHotSeat;
        private NumericUpDown nmrLevel;
        private CheckBox chkShowQuestion;
        private CheckBox chkCorrectAnswer;
        private Button btn5050;
        private Button btnPhoneFriend;
        private Button btnAskAudience;
        private Button btnSwitch;
    }
}
