namespace MillionaireGame.Forms
{
    partial class FFFControlPanel
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
            grpQuestion = new GroupBox();
            cmbQuestions = new ComboBox();
            lblQuestion = new Label();
            txtOption1 = new TextBox();
            txtOption2 = new TextBox();
            txtOption3 = new TextBox();
            txtOption4 = new TextBox();
            lblCorrectOrder = new Label();
            btnStartFFF = new Button();
            btnEndFFF = new Button();
            lblTimer = new Label();
            
            grpParticipants = new GroupBox();
            lstParticipants = new ListBox();
            lblParticipantCount = new Label();
            
            grpAnswers = new GroupBox();
            lstAnswers = new ListBox();
            lblAnswerCount = new Label();
            btnCalculateResults = new Button();
            
            grpRankings = new GroupBox();
            lstRankings = new ListBox();
            lblWinner = new Label();
            btnSelectWinner = new Button();
            
            grpQuestion.SuspendLayout();
            grpParticipants.SuspendLayout();
            grpAnswers.SuspendLayout();
            grpRankings.SuspendLayout();
            SuspendLayout();
            
            // 
            // grpQuestion
            // 
            grpQuestion.Controls.Add(cmbQuestions);
            grpQuestion.Controls.Add(lblQuestion);
            grpQuestion.Controls.Add(txtOption1);
            grpQuestion.Controls.Add(txtOption2);
            grpQuestion.Controls.Add(txtOption3);
            grpQuestion.Controls.Add(txtOption4);
            grpQuestion.Controls.Add(lblCorrectOrder);
            grpQuestion.Controls.Add(btnStartFFF);
            grpQuestion.Controls.Add(btnEndFFF);
            grpQuestion.Controls.Add(lblTimer);
            grpQuestion.Location = new Point(10, 10);
            grpQuestion.Name = "grpQuestion";
            grpQuestion.Size = new Size(780, 220);
            grpQuestion.TabIndex = 0;
            grpQuestion.TabStop = false;
            grpQuestion.Text = "FFF Question";
            
            // 
            // cmbQuestions
            // 
            cmbQuestions.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbQuestions.FormattingEnabled = true;
            cmbQuestions.Location = new Point(15, 50);
            cmbQuestions.Name = "cmbQuestions";
            cmbQuestions.Size = new Size(750, 28);
            cmbQuestions.TabIndex = 0;
            cmbQuestions.SelectedIndexChanged += cmbQuestions_SelectedIndexChanged;
            
            // 
            // lblQuestion
            // 
            lblQuestion.AutoSize = true;
            lblQuestion.Location = new Point(15, 25);
            lblQuestion.Name = "lblQuestion";
            lblQuestion.Size = new Size(145, 20);
            lblQuestion.TabIndex = 1;
            lblQuestion.Text = "Select FFF Question:";
            
            // 
            // txtOption1
            // 
            txtOption1.Location = new Point(15, 90);
            txtOption1.Name = "txtOption1";
            txtOption1.ReadOnly = true;
            txtOption1.Size = new Size(360, 27);
            txtOption1.TabIndex = 2;
            
            // 
            // txtOption2
            // 
            txtOption2.Location = new Point(405, 90);
            txtOption2.Name = "txtOption2";
            txtOption2.ReadOnly = true;
            txtOption2.Size = new Size(360, 27);
            txtOption2.TabIndex = 3;
            
            // 
            // txtOption3
            // 
            txtOption3.Location = new Point(15, 125);
            txtOption3.Name = "txtOption3";
            txtOption3.ReadOnly = true;
            txtOption3.Size = new Size(360, 27);
            txtOption3.TabIndex = 4;
            
            // 
            // txtOption4
            // 
            txtOption4.Location = new Point(405, 125);
            txtOption4.Name = "txtOption4";
            txtOption4.ReadOnly = true;
            txtOption4.Size = new Size(360, 27);
            txtOption4.TabIndex = 5;
            
            // 
            // lblCorrectOrder
            // 
            lblCorrectOrder.AutoSize = true;
            lblCorrectOrder.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblCorrectOrder.ForeColor = Color.Green;
            lblCorrectOrder.Location = new Point(15, 160);
            lblCorrectOrder.Name = "lblCorrectOrder";
            lblCorrectOrder.Size = new Size(150, 23);
            lblCorrectOrder.TabIndex = 6;
            lblCorrectOrder.Text = "Correct Order: ---";
            
            // 
            // btnStartFFF
            // 
            btnStartFFF.BackColor = Color.FromArgb(0, 120, 215);
            btnStartFFF.FlatStyle = FlatStyle.Flat;
            btnStartFFF.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnStartFFF.ForeColor = Color.White;
            btnStartFFF.Location = new Point(540, 155);
            btnStartFFF.Name = "btnStartFFF";
            btnStartFFF.Size = new Size(100, 50);
            btnStartFFF.TabIndex = 7;
            btnStartFFF.Text = "START";
            btnStartFFF.UseVisualStyleBackColor = false;
            btnStartFFF.Click += btnStartFFF_Click;
            
            // 
            // btnEndFFF
            // 
            btnEndFFF.BackColor = Color.FromArgb(192, 0, 0);
            btnEndFFF.Enabled = false;
            btnEndFFF.FlatStyle = FlatStyle.Flat;
            btnEndFFF.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnEndFFF.ForeColor = Color.White;
            btnEndFFF.Location = new Point(650, 155);
            btnEndFFF.Name = "btnEndFFF";
            btnEndFFF.Size = new Size(100, 50);
            btnEndFFF.TabIndex = 8;
            btnEndFFF.Text = "END";
            btnEndFFF.UseVisualStyleBackColor = false;
            btnEndFFF.Click += btnEndFFF_Click;
            
            // 
            // lblTimer
            // 
            lblTimer.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTimer.ForeColor = Color.Red;
            lblTimer.Location = new Point(400, 155);
            lblTimer.Name = "lblTimer";
            lblTimer.Size = new Size(120, 50);
            lblTimer.TabIndex = 9;
            lblTimer.Text = "00:00";
            lblTimer.TextAlign = ContentAlignment.MiddleCenter;
            
            // 
            // grpParticipants
            // 
            grpParticipants.Controls.Add(lstParticipants);
            grpParticipants.Controls.Add(lblParticipantCount);
            grpParticipants.Location = new Point(10, 240);
            grpParticipants.Name = "grpParticipants";
            grpParticipants.Size = new Size(250, 350);
            grpParticipants.TabIndex = 1;
            grpParticipants.TabStop = false;
            grpParticipants.Text = "Active Participants";
            
            // 
            // lstParticipants
            // 
            lstParticipants.FormattingEnabled = true;
            lstParticipants.ItemHeight = 20;
            lstParticipants.Location = new Point(10, 50);
            lstParticipants.Name = "lstParticipants";
            lstParticipants.Size = new Size(230, 284);
            lstParticipants.TabIndex = 0;
            
            // 
            // lblParticipantCount
            // 
            lblParticipantCount.AutoSize = true;
            lblParticipantCount.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblParticipantCount.Location = new Point(10, 25);
            lblParticipantCount.Name = "lblParticipantCount";
            lblParticipantCount.Size = new Size(134, 23);
            lblParticipantCount.TabIndex = 1;
            lblParticipantCount.Text = "0 Participants";
            
            // 
            // grpAnswers
            // 
            grpAnswers.Controls.Add(lstAnswers);
            grpAnswers.Controls.Add(lblAnswerCount);
            grpAnswers.Controls.Add(btnCalculateResults);
            grpAnswers.Location = new Point(270, 240);
            grpAnswers.Name = "grpAnswers";
            grpAnswers.Size = new Size(260, 350);
            grpAnswers.TabIndex = 2;
            grpAnswers.TabStop = false;
            grpAnswers.Text = "Submitted Answers";
            
            // 
            // lstAnswers
            // 
            lstAnswers.FormattingEnabled = true;
            lstAnswers.ItemHeight = 20;
            lstAnswers.Location = new Point(10, 50);
            lstAnswers.Name = "lstAnswers";
            lstAnswers.Size = new Size(240, 224);
            lstAnswers.TabIndex = 0;
            
            // 
            // lblAnswerCount
            // 
            lblAnswerCount.AutoSize = true;
            lblAnswerCount.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblAnswerCount.Location = new Point(10, 25);
            lblAnswerCount.Name = "lblAnswerCount";
            lblAnswerCount.Size = new Size(107, 23);
            lblAnswerCount.TabIndex = 1;
            lblAnswerCount.Text = "0 Answers";
            
            // 
            // btnCalculateResults
            // 
            btnCalculateResults.BackColor = Color.FromArgb(0, 120, 215);
            btnCalculateResults.Enabled = false;
            btnCalculateResults.FlatStyle = FlatStyle.Flat;
            btnCalculateResults.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnCalculateResults.ForeColor = Color.White;
            btnCalculateResults.Location = new Point(10, 285);
            btnCalculateResults.Name = "btnCalculateResults";
            btnCalculateResults.Size = new Size(240, 50);
            btnCalculateResults.TabIndex = 2;
            btnCalculateResults.Text = "Calculate Results";
            btnCalculateResults.UseVisualStyleBackColor = false;
            btnCalculateResults.Click += btnCalculateResults_Click;
            
            // 
            // grpRankings
            // 
            grpRankings.Controls.Add(lstRankings);
            grpRankings.Controls.Add(lblWinner);
            grpRankings.Controls.Add(btnSelectWinner);
            grpRankings.Location = new Point(540, 240);
            grpRankings.Name = "grpRankings";
            grpRankings.Size = new Size(250, 350);
            grpRankings.TabIndex = 3;
            grpRankings.TabStop = false;
            grpRankings.Text = "Rankings";
            
            // 
            // lstRankings
            // 
            lstRankings.FormattingEnabled = true;
            lstRankings.ItemHeight = 20;
            lstRankings.Location = new Point(10, 50);
            lstRankings.Name = "lstRankings";
            lstRankings.Size = new Size(230, 164);
            lstRankings.TabIndex = 0;
            
            // 
            // lblWinner
            // 
            lblWinner.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblWinner.ForeColor = Color.Green;
            lblWinner.Location = new Point(10, 225);
            lblWinner.Name = "lblWinner";
            lblWinner.Size = new Size(230, 50);
            lblWinner.TabIndex = 1;
            lblWinner.Text = "Winner: ---";
            lblWinner.TextAlign = ContentAlignment.MiddleCenter;
            
            // 
            // btnSelectWinner
            // 
            btnSelectWinner.BackColor = Color.FromArgb(0, 150, 0);
            btnSelectWinner.Enabled = false;
            btnSelectWinner.FlatStyle = FlatStyle.Flat;
            btnSelectWinner.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnSelectWinner.ForeColor = Color.White;
            btnSelectWinner.Location = new Point(10, 285);
            btnSelectWinner.Name = "btnSelectWinner";
            btnSelectWinner.Size = new Size(230, 50);
            btnSelectWinner.TabIndex = 2;
            btnSelectWinner.Text = "Select Winner";
            btnSelectWinner.UseVisualStyleBackColor = false;
            btnSelectWinner.Click += btnSelectWinner_Click;
            
            // 
            // FFFControlPanel
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(grpRankings);
            Controls.Add(grpAnswers);
            Controls.Add(grpParticipants);
            Controls.Add(grpQuestion);
            Name = "FFFControlPanel";
            Size = new Size(800, 600);
            grpQuestion.ResumeLayout(false);
            grpQuestion.PerformLayout();
            grpParticipants.ResumeLayout(false);
            grpParticipants.PerformLayout();
            grpAnswers.ResumeLayout(false);
            grpAnswers.PerformLayout();
            grpRankings.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private GroupBox grpQuestion;
        private ComboBox cmbQuestions;
        private Label lblQuestion;
        private TextBox txtOption1;
        private TextBox txtOption2;
        private TextBox txtOption3;
        private TextBox txtOption4;
        private Label lblCorrectOrder;
        private Button btnStartFFF;
        private Button btnEndFFF;
        private Label lblTimer;
        
        private GroupBox grpParticipants;
        private ListBox lstParticipants;
        private Label lblParticipantCount;
        
        private GroupBox grpAnswers;
        private ListBox lstAnswers;
        private Label lblAnswerCount;
        private Button btnCalculateResults;
        
        private GroupBox grpRankings;
        private ListBox lstRankings;
        private Label lblWinner;
        private Button btnSelectWinner;
    }
}
