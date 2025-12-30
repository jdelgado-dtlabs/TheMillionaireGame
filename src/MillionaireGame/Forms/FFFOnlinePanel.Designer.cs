namespace MillionaireGame.Forms
{
    partial class FFFOnlinePanel
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

        #region Component Designer generated code

        private void InitializeComponent()
        {
            // Question Setup Section
            grpQuestion = new GroupBox();
            txtQuestionDisplay = new TextBox();
            txtOption1 = new TextBox();
            txtOption2 = new TextBox();
            txtOption3 = new TextBox();
            txtOption4 = new TextBox();
            lblCorrectOrder = new Label();
            
            // Participants Section
            grpParticipants = new GroupBox();
            lstParticipants = new ListBox();
            lblParticipantCount = new Label();
            btnRefreshParticipants = new Button();
            
            // Game Flow Section
            grpGameFlow = new GroupBox();
            btnIntroExplain = new Button();
            btnShowQuestion = new Button();
            btnRevealAnswers = new Button();
            btnRevealCorrect = new Button();
            btnShowWinners = new Button();
            btnWinner = new Button();
            lblSeparator = new Label();
            
            // Timer and Status Section
            grpTimerStatus = new GroupBox();
            lblTimer = new Label();
            lblTimerLabel = new Label();
            lblStatus = new Label();
            lblStatusLabel = new Label();
            
            // Answer Submissions Section
            grpAnswers = new GroupBox();
            lstAnswers = new ListBox();
            lblAnswerCount = new Label();
            
            // Rankings Section
            grpRankings = new GroupBox();
            lstRankings = new ListBox();
            lblWinner = new Label();
            
            // Audio Control
            btnStopAudio = new Button();
            
            grpQuestion.SuspendLayout();
            grpParticipants.SuspendLayout();
            grpGameFlow.SuspendLayout();
            grpTimerStatus.SuspendLayout();
            grpAnswers.SuspendLayout();
            grpRankings.SuspendLayout();
            SuspendLayout();
            
            // 
            // grpQuestion
            // 
            grpQuestion.Controls.Add(txtQuestionDisplay);
            grpQuestion.Controls.Add(txtOption1);
            grpQuestion.Controls.Add(txtOption2);
            grpQuestion.Controls.Add(txtOption3);
            grpQuestion.Controls.Add(txtOption4);
            grpQuestion.Controls.Add(lblCorrectOrder);
            grpQuestion.Location = new Point(10, 10);
            grpQuestion.Name = "grpQuestion";
            grpQuestion.Size = new Size(990, 150);
            grpQuestion.TabIndex = 0;
            grpQuestion.TabStop = false;
            grpQuestion.Text = "Question Setup";
            grpQuestion.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            
            // 
            // txtQuestionDisplay
            // 
            txtQuestionDisplay.BackColor = SystemColors.Info;
            txtQuestionDisplay.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            txtQuestionDisplay.Location = new Point(15, 25);
            txtQuestionDisplay.Multiline = true;
            txtQuestionDisplay.Name = "txtQuestionDisplay";
            txtQuestionDisplay.ReadOnly = true;
            txtQuestionDisplay.Size = new Size(960, 50);
            txtQuestionDisplay.TabIndex = 3;
            txtQuestionDisplay.TabStop = false;
            txtQuestionDisplay.Text = "Click 'Show Question' to randomly select and display a question...";
            
            // 
            // txtOption1
            // 
            txtOption1.BackColor = Color.LightGray;
            txtOption1.Font = new Font("Segoe UI", 9F);
            txtOption1.Location = new Point(15, 83);
            txtOption1.Name = "txtOption1";
            txtOption1.ReadOnly = true;
            txtOption1.Size = new Size(230, 27);
            txtOption1.TabIndex = 4;
            txtOption1.TabStop = false;
            txtOption1.Text = "A:";
            
            // 
            // txtOption2
            // 
            txtOption2.BackColor = Color.LightGray;
            txtOption2.Font = new Font("Segoe UI", 9F);
            txtOption2.Location = new Point(255, 83);
            txtOption2.Name = "txtOption2";
            txtOption2.ReadOnly = true;
            txtOption2.Size = new Size(230, 27);
            txtOption2.TabIndex = 5;
            txtOption2.TabStop = false;
            txtOption2.Text = "B:";
            
            // 
            // txtOption3
            // 
            txtOption3.BackColor = Color.LightGray;
            txtOption3.Font = new Font("Segoe UI", 9F);
            txtOption3.Location = new Point(495, 83);
            txtOption3.Name = "txtOption3";
            txtOption3.ReadOnly = true;
            txtOption3.Size = new Size(230, 27);
            txtOption3.TabIndex = 6;
            txtOption3.TabStop = false;
            txtOption3.Text = "C:";
            
            // 
            // txtOption4
            // 
            txtOption4.BackColor = Color.LightGray;
            txtOption4.Font = new Font("Segoe UI", 9F);
            txtOption4.Location = new Point(735, 83);
            txtOption4.Name = "txtOption4";
            txtOption4.ReadOnly = true;
            txtOption4.Size = new Size(230, 27);
            txtOption4.TabIndex = 7;
            txtOption4.TabStop = false;
            txtOption4.Text = "D:";
            
            // 
            // lblCorrectOrder
            // 
            lblCorrectOrder.AutoSize = true;
            lblCorrectOrder.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblCorrectOrder.ForeColor = Color.Green;
            lblCorrectOrder.Location = new Point(15, 118);
            lblCorrectOrder.Name = "lblCorrectOrder";
            lblCorrectOrder.Size = new Size(150, 23);
            lblCorrectOrder.TabIndex = 8;
            lblCorrectOrder.Text = "Correct Order: ---";
            
            // 
            // grpParticipants
            // 
            grpParticipants.Controls.Add(lstParticipants);
            grpParticipants.Controls.Add(lblParticipantCount);
            grpParticipants.Controls.Add(btnRefreshParticipants);
            grpParticipants.Location = new Point(10, 170);
            grpParticipants.Name = "grpParticipants";
            grpParticipants.Size = new Size(240, 470);
            grpParticipants.TabIndex = 1;
            grpParticipants.TabStop = false;
            grpParticipants.Text = "👥 Participants";
            grpParticipants.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            
            // 
            // lstParticipants
            // 
            lstParticipants.Font = new Font("Segoe UI", 9F);
            lstParticipants.FormattingEnabled = true;
            lstParticipants.ItemHeight = 20;
            lstParticipants.Location = new Point(10, 30);
            lstParticipants.Name = "lstParticipants";
            lstParticipants.Size = new Size(220, 384);
            lstParticipants.TabIndex = 0;
            
            // 
            // lblParticipantCount
            // 
            lblParticipantCount.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblParticipantCount.Location = new Point(10, 424);
            lblParticipantCount.Name = "lblParticipantCount";
            lblParticipantCount.Size = new Size(220, 30);
            lblParticipantCount.TabIndex = 1;
            lblParticipantCount.Text = "0 Participants";
            lblParticipantCount.TextAlign = ContentAlignment.MiddleCenter;
            
            // 
            // btnRefreshParticipants
            // 
            btnRefreshParticipants.BackColor = Color.FromArgb(30, 144, 255);
            btnRefreshParticipants.FlatAppearance.BorderColor = Color.Black;
            btnRefreshParticipants.FlatAppearance.BorderSize = 2;
            btnRefreshParticipants.FlatStyle = FlatStyle.Flat;
            btnRefreshParticipants.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnRefreshParticipants.ForeColor = Color.White;
            btnRefreshParticipants.Location = new Point(10, 10);
            btnRefreshParticipants.Name = "btnRefreshParticipants";
            btnRefreshParticipants.Size = new Size(200, 10);
            btnRefreshParticipants.TabIndex = 2;
            btnRefreshParticipants.Text = "🔄 Refresh";
            btnRefreshParticipants.UseVisualStyleBackColor = false;
            btnRefreshParticipants.Click += btnRefreshParticipants_Click;
            btnRefreshParticipants.Visible = false;
            
            // 
            // grpGameFlow
            // 
            grpGameFlow.Controls.Add(btnIntroExplain);
            grpGameFlow.Controls.Add(btnShowQuestion);
            grpGameFlow.Controls.Add(btnRevealAnswers);
            grpGameFlow.Controls.Add(btnRevealCorrect);
            grpGameFlow.Controls.Add(btnShowWinners);
            grpGameFlow.Controls.Add(btnWinner);
            grpGameFlow.Controls.Add(lblSeparator);
            grpGameFlow.Controls.Add(btnStopAudio);
            grpGameFlow.Location = new Point(760, 170);
            grpGameFlow.Name = "grpGameFlow";
            grpGameFlow.Size = new Size(240, 560);
            grpGameFlow.TabIndex = 2;
            grpGameFlow.TabStop = false;
            grpGameFlow.Text = "Game Flow";
            grpGameFlow.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            
            // 
            // btnIntroExplain
            // 
            btnIntroExplain.BackColor = Color.LimeGreen;
            btnIntroExplain.FlatAppearance.BorderColor = Color.Black;
            btnIntroExplain.FlatAppearance.BorderSize = 2;
            btnIntroExplain.FlatStyle = FlatStyle.Flat;
            btnIntroExplain.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnIntroExplain.ForeColor = Color.Black;
            btnIntroExplain.Location = new Point(15, 27);
            btnIntroExplain.Name = "btnIntroExplain";
            btnIntroExplain.Size = new Size(210, 62);
            btnIntroExplain.TabIndex = 0;
            btnIntroExplain.Text = "1. Intro + Explain";
            btnIntroExplain.UseVisualStyleBackColor = false;
            btnIntroExplain.Click += btnIntroExplain_Click;
            
            // 
            // btnShowQuestion
            // 
            btnShowQuestion.BackColor = Color.Gray;
            btnShowQuestion.Enabled = false;
            btnShowQuestion.FlatAppearance.BorderColor = Color.Black;
            btnShowQuestion.FlatAppearance.BorderSize = 2;
            btnShowQuestion.FlatStyle = FlatStyle.Flat;
            btnShowQuestion.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnShowQuestion.ForeColor = Color.White;
            btnShowQuestion.Location = new Point(15, 102);
            btnShowQuestion.Name = "btnShowQuestion";
            btnShowQuestion.Size = new Size(210, 62);
            btnShowQuestion.TabIndex = 1;
            btnShowQuestion.Text = "2. Show Question";
            btnShowQuestion.UseVisualStyleBackColor = false;
            btnShowQuestion.Click += btnShowQuestion_Click;
            
            // 
            // btnRevealAnswers
            // 
            btnRevealAnswers.BackColor = Color.Gray;
            btnRevealAnswers.Enabled = false;
            btnRevealAnswers.FlatAppearance.BorderColor = Color.Black;
            btnRevealAnswers.FlatAppearance.BorderSize = 2;
            btnRevealAnswers.FlatStyle = FlatStyle.Flat;
            btnRevealAnswers.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnRevealAnswers.ForeColor = Color.White;
            btnRevealAnswers.Location = new Point(15, 174);
            btnRevealAnswers.Name = "btnRevealAnswers";
            btnRevealAnswers.Size = new Size(210, 62);
            btnRevealAnswers.TabIndex = 2;
            btnRevealAnswers.Text = "3. Reveal Answers & Start";
            btnRevealAnswers.UseVisualStyleBackColor = false;
            btnRevealAnswers.Click += btnRevealAnswers_Click;
            
            // 
            // btnRevealCorrect
            // 
            btnRevealCorrect.BackColor = Color.Gray;
            btnRevealCorrect.Enabled = false;
            btnRevealCorrect.FlatAppearance.BorderColor = Color.Black;
            btnRevealCorrect.FlatAppearance.BorderSize = 2;
            btnRevealCorrect.FlatStyle = FlatStyle.Flat;
            btnRevealCorrect.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnRevealCorrect.ForeColor = Color.White;
            btnRevealCorrect.Location = new Point(15, 246);
            btnRevealCorrect.Name = "btnRevealCorrect";
            btnRevealCorrect.Size = new Size(210, 62);
            btnRevealCorrect.TabIndex = 3;
            btnRevealCorrect.Text = "4. Reveal Correct";
            btnRevealCorrect.UseVisualStyleBackColor = false;
            btnRevealCorrect.Click += btnRevealCorrect_Click;
            
            // 
            // btnShowWinners
            // 
            btnShowWinners.BackColor = Color.Gray;
            btnShowWinners.Enabled = false;
            btnShowWinners.FlatAppearance.BorderColor = Color.Black;
            btnShowWinners.FlatAppearance.BorderSize = 2;
            btnShowWinners.FlatStyle = FlatStyle.Flat;
            btnShowWinners.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnShowWinners.ForeColor = Color.White;
            btnShowWinners.Location = new Point(15, 318);
            btnShowWinners.Name = "btnShowWinners";
            btnShowWinners.Size = new Size(210, 62);
            btnShowWinners.TabIndex = 4;
            btnShowWinners.Text = "5. Show Winners";
            btnShowWinners.UseVisualStyleBackColor = false;
            btnShowWinners.Click += btnShowWinners_Click;
            
            // 
            // btnWinner
            // 
            btnWinner.BackColor = Color.Gray;
            btnWinner.Enabled = false;
            btnWinner.FlatAppearance.BorderColor = Color.Black;
            btnWinner.FlatAppearance.BorderSize = 2;
            btnWinner.FlatStyle = FlatStyle.Flat;
            btnWinner.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnWinner.ForeColor = Color.White;
            btnWinner.Location = new Point(15, 390);
            btnWinner.Name = "btnWinner";
            btnWinner.Size = new Size(210, 62);
            btnWinner.TabIndex = 6;
            btnWinner.Text = "6. 🏆 Confirm Winner";
            btnWinner.UseVisualStyleBackColor = false;
            btnWinner.Click += btnWinner_Click;
            
            //             // lblSeparator
            // 
            lblSeparator.BorderStyle = BorderStyle.Fixed3D;
            lblSeparator.Location = new Point(15, 462);
            lblSeparator.Name = "lblSeparator";
            lblSeparator.Size = new Size(210, 2);
            lblSeparator.TabIndex = 7;
            
            //             // grpTimerStatus
            // 
            grpTimerStatus.Controls.Add(lblTimerLabel);
            grpTimerStatus.Controls.Add(lblTimer);
            grpTimerStatus.Controls.Add(lblStatusLabel);
            grpTimerStatus.Controls.Add(lblStatus);
            grpTimerStatus.Location = new Point(10, 650);
            grpTimerStatus.Name = "grpTimerStatus";
            grpTimerStatus.Size = new Size(740, 80);
            grpTimerStatus.TabIndex = 3;
            grpTimerStatus.TabStop = false;
            grpTimerStatus.Text = "Timer & Status";
            grpTimerStatus.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            
            // 
            // lblTimerLabel
            // 
            lblTimerLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblTimerLabel.Location = new Point(20, 30);
            lblTimerLabel.Name = "lblTimerLabel";
            lblTimerLabel.Size = new Size(80, 20);
            lblTimerLabel.TabIndex = 0;
            lblTimerLabel.Text = "⏱️ Timer:";
            lblTimerLabel.TextAlign = ContentAlignment.MiddleCenter;
            
            // 
            // lblTimer
            // 
            lblTimer.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            lblTimer.ForeColor = Color.Red;
            lblTimer.Location = new Point(110, 25);
            lblTimer.Name = "lblTimer";
            lblTimer.Size = new Size(150, 40);
            lblTimer.TabIndex = 1;
            lblTimer.Text = "00:00";
            lblTimer.TextAlign = ContentAlignment.MiddleCenter;
            
            // 
            // lblStatusLabel
            // 
            lblStatusLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblStatusLabel.Location = new Point(320, 30);
            lblStatusLabel.Name = "lblStatusLabel";
            lblStatusLabel.Size = new Size(80, 20);
            lblStatusLabel.TabIndex = 2;
            lblStatusLabel.Text = "Status:";
            lblStatusLabel.TextAlign = ContentAlignment.MiddleCenter;
            
            // 
            // lblStatus
            // 
            lblStatus.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblStatus.ForeColor = Color.Gray;
            lblStatus.Location = new Point(410, 25);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(240, 40);
            lblStatus.TabIndex = 3;
            lblStatus.Text = "● Not Started";
            lblStatus.TextAlign = ContentAlignment.MiddleCenter;
            
            // 
            // grpAnswers
            // 
            grpAnswers.Controls.Add(lstAnswers);
            grpAnswers.Controls.Add(lblAnswerCount);
            grpAnswers.Location = new Point(260, 170);
            grpAnswers.Name = "grpAnswers";
            grpAnswers.Size = new Size(240, 470);
            grpAnswers.TabIndex = 4;
            grpAnswers.TabStop = false;
            grpAnswers.Text = "Answer Submissions";
            grpAnswers.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            
            // 
            // lstAnswers
            // 
            lstAnswers.FormattingEnabled = true;
            lstAnswers.ItemHeight = 20;
            lstAnswers.Location = new Point(10, 30);
            lstAnswers.Name = "lstAnswers";
            lstAnswers.Size = new Size(220, 384);
            lstAnswers.TabIndex = 0;
            
            // 
            // lblAnswerCount
            // 
            lblAnswerCount.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblAnswerCount.Location = new Point(10, 424);
            lblAnswerCount.Name = "lblAnswerCount";
            lblAnswerCount.Size = new Size(220, 30);
            lblAnswerCount.TabIndex = 1;
            lblAnswerCount.Text = "0 Answers";
            lblAnswerCount.TextAlign = ContentAlignment.MiddleCenter;
            
            // 
            // grpRankings
            // 
            grpRankings.Controls.Add(lstRankings);
            grpRankings.Controls.Add(lblWinner);
            grpRankings.Location = new Point(510, 170);
            grpRankings.Name = "grpRankings";
            grpRankings.Size = new Size(240, 470);
            grpRankings.TabIndex = 5;
            grpRankings.TabStop = false;
            grpRankings.Text = "Rankings & Winner";
            grpRankings.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            
            // 
            // lstRankings
            // 
            lstRankings.Font = new Font("Segoe UI", 9F);
            lstRankings.FormattingEnabled = true;
            lstRankings.ItemHeight = 20;
            lstRankings.Location = new Point(10, 30);
            lstRankings.Name = "lstRankings";
            lstRankings.Size = new Size(220, 384);
            lstRankings.TabIndex = 0;
            
            // 
            // lblWinner
            // 
            lblWinner.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblWinner.ForeColor = Color.Green;
            lblWinner.Location = new Point(10, 424);
            lblWinner.Name = "lblWinner";
            lblWinner.Size = new Size(220, 30);
            lblWinner.TabIndex = 1;
            lblWinner.Text = "Winner: ---";
            lblWinner.TextAlign = ContentAlignment.MiddleCenter;
            
            // 
            // btnStopAudio
            // 
            btnStopAudio.BackColor = Color.DarkRed;
            btnStopAudio.FlatAppearance.BorderColor = Color.Black;
            btnStopAudio.FlatAppearance.BorderSize = 2;
            btnStopAudio.FlatStyle = FlatStyle.Flat;
            btnStopAudio.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnStopAudio.ForeColor = Color.White;
            btnStopAudio.Location = new Point(15, 474);
            btnStopAudio.Name = "btnStopAudio";
            btnStopAudio.Size = new Size(210, 62);
            btnStopAudio.TabIndex = 8;
            btnStopAudio.Text = "⏹ STOP ALL AUDIO";
            btnStopAudio.UseVisualStyleBackColor = false;
            btnStopAudio.Click += btnStopAudio_Click;
            
            // 
            // FFFControlPanel
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.None;
            BackColor = SystemColors.Control;
            Controls.Add(grpRankings);
            Controls.Add(grpAnswers);
            Controls.Add(grpTimerStatus);
            Controls.Add(grpGameFlow);
            Controls.Add(grpParticipants);
            Controls.Add(grpQuestion);
            Name = "FFFControlPanel";
            Size = new Size(1010, 740);
            grpQuestion.ResumeLayout(false);
            grpQuestion.PerformLayout();
            grpParticipants.ResumeLayout(false);
            grpGameFlow.ResumeLayout(false);
            grpTimerStatus.ResumeLayout(false);
            grpAnswers.ResumeLayout(false);
            grpRankings.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        // Question Setup
        private GroupBox grpQuestion;
        private TextBox txtQuestionDisplay;
        private TextBox txtOption1;
        private TextBox txtOption2;
        private TextBox txtOption3;
        private TextBox txtOption4;
        private Label lblCorrectOrder;
        
        // Participants
        private GroupBox grpParticipants;
        private ListBox lstParticipants;
        private Label lblParticipantCount;
        private Button btnRefreshParticipants;
        
        // Game Flow
        private GroupBox grpGameFlow;
        private Button btnIntroExplain;
        private Button btnShowQuestion;
        private Button btnRevealAnswers;
        private Button btnRevealCorrect;
        private Button btnShowWinners;
        private Button btnWinner;
        private Label lblSeparator;
        
        // Timer and Status
        private GroupBox grpTimerStatus;
        private Label lblTimerLabel;
        private Label lblTimer;
        private Label lblStatusLabel;
        private Label lblStatus;
        
        // Answer Submissions
        private GroupBox grpAnswers;
        private ListBox lstAnswers;
        private Label lblAnswerCount;
        
        // Rankings
        private GroupBox grpRankings;
        private ListBox lstRankings;
        private Label lblWinner;
        
        // Audio Control
        private Button btnStopAudio;
    }
}
