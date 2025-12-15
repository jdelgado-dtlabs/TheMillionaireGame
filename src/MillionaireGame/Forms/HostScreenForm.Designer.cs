namespace MillionaireGame.Forms
{
    partial class HostScreenForm
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
            // Form setup
            SuspendLayout();

            // Question panel
            lblQuestion = new Label();
            lblQuestion.Location = new Point(50, 50);
            lblQuestion.Size = new Size(900, 100);
            lblQuestion.Font = new Font("Arial", 24F, FontStyle.Bold);
            lblQuestion.ForeColor = Color.White;
            lblQuestion.BackColor = Color.Transparent;
            lblQuestion.TextAlign = ContentAlignment.MiddleCenter;

            // Answer panels
            pnlAnswerA = new Panel();
            pnlAnswerA.Location = new Point(50, 170);
            pnlAnswerA.Size = new Size(440, 60);
            pnlAnswerA.BackColor = Color.FromArgb(30, 30, 60);

            lblAnswerA = new Label();
            lblAnswerA.Dock = DockStyle.Fill;
            lblAnswerA.Font = new Font("Arial", 16F);
            lblAnswerA.ForeColor = Color.White;
            lblAnswerA.Text = "A:";
            lblAnswerA.TextAlign = ContentAlignment.MiddleLeft;
            lblAnswerA.Padding = new Padding(10, 0, 0, 0);
            pnlAnswerA.Controls.Add(lblAnswerA);

            pnlAnswerB = new Panel();
            pnlAnswerB.Location = new Point(510, 170);
            pnlAnswerB.Size = new Size(440, 60);
            pnlAnswerB.BackColor = Color.FromArgb(30, 30, 60);

            lblAnswerB = new Label();
            lblAnswerB.Dock = DockStyle.Fill;
            lblAnswerB.Font = new Font("Arial", 16F);
            lblAnswerB.ForeColor = Color.White;
            lblAnswerB.Text = "B:";
            lblAnswerB.TextAlign = ContentAlignment.MiddleLeft;
            lblAnswerB.Padding = new Padding(10, 0, 0, 0);
            pnlAnswerB.Controls.Add(lblAnswerB);

            pnlAnswerC = new Panel();
            pnlAnswerC.Location = new Point(50, 250);
            pnlAnswerC.Size = new Size(440, 60);
            pnlAnswerC.BackColor = Color.FromArgb(30, 30, 60);

            lblAnswerC = new Label();
            lblAnswerC.Dock = DockStyle.Fill;
            lblAnswerC.Font = new Font("Arial", 16F);
            lblAnswerC.ForeColor = Color.White;
            lblAnswerC.Text = "C:";
            lblAnswerC.TextAlign = ContentAlignment.MiddleLeft;
            lblAnswerC.Padding = new Padding(10, 0, 0, 0);
            pnlAnswerC.Controls.Add(lblAnswerC);

            pnlAnswerD = new Panel();
            pnlAnswerD.Location = new Point(510, 250);
            pnlAnswerD.Size = new Size(440, 60);
            pnlAnswerD.BackColor = Color.FromArgb(30, 30, 60);

            lblAnswerD = new Label();
            lblAnswerD.Dock = DockStyle.Fill;
            lblAnswerD.Font = new Font("Arial", 16F);
            lblAnswerD.ForeColor = Color.White;
            lblAnswerD.Text = "D:";
            lblAnswerD.TextAlign = ContentAlignment.MiddleLeft;
            lblAnswerD.Padding = new Padding(10, 0, 0, 0);
            pnlAnswerD.Controls.Add(lblAnswerD);

            // Correct answer indicator (host only)
            lblCorrectAnswer = new Label();
            lblCorrectAnswer.Location = new Point(50, 330);
            lblCorrectAnswer.Size = new Size(200, 30);
            lblCorrectAnswer.Font = new Font("Arial", 14F, FontStyle.Bold);
            lblCorrectAnswer.ForeColor = Color.LimeGreen;
            lblCorrectAnswer.BackColor = Color.Transparent;

            // ATA percentages (host only)
            lblATAa = new Label();
            lblATAa.Location = new Point(270, 330);
            lblATAa.Size = new Size(80, 30);
            lblATAa.Font = new Font("Arial", 12F);
            lblATAa.ForeColor = Color.Yellow;

            lblATAb = new Label();
            lblATAb.Location = new Point(360, 330);
            lblATAb.Size = new Size(80, 30);
            lblATAb.Font = new Font("Arial", 12F);
            lblATAb.ForeColor = Color.Yellow;

            lblATAc = new Label();
            lblATAc.Location = new Point(450, 330);
            lblATAc.Size = new Size(80, 30);
            lblATAc.Font = new Font("Arial", 12F);
            lblATAc.ForeColor = Color.Yellow;

            lblATAd = new Label();
            lblATAd.Location = new Point(540, 330);
            lblATAd.Size = new Size(80, 30);
            lblATAd.Font = new Font("Arial", 12F);
            lblATAd.ForeColor = Color.Yellow;

            // Money display
            lblCurrent = new Label();
            lblCurrent.Location = new Point(50, 380);
            lblCurrent.Size = new Size(150, 25);
            lblCurrent.Font = new Font("Arial", 12F);
            lblCurrent.ForeColor = Color.White;
            lblCurrent.Text = "Current: $0";

            lblCorrect = new Label();
            lblCorrect.Location = new Point(220, 380);
            lblCorrect.Size = new Size(150, 25);
            lblCorrect.Font = new Font("Arial", 12F);
            lblCorrect.ForeColor = Color.LimeGreen;
            lblCorrect.Text = "Correct: $0";

            lblWrong = new Label();
            lblWrong.Location = new Point(390, 380);
            lblWrong.Size = new Size(150, 25);
            lblWrong.Font = new Font("Arial", 12F);
            lblWrong.ForeColor = Color.Red;
            lblWrong.Text = "Wrong: $0";

            lblDrop = new Label();
            lblDrop.Location = new Point(560, 380);
            lblDrop.Size = new Size(150, 25);
            lblDrop.Font = new Font("Arial", 12F);
            lblDrop.ForeColor = Color.Orange;
            lblDrop.Text = "Drop: $0";

            lblQLeft = new Label();
            lblQLeft.Location = new Point(730, 380);
            lblQLeft.Size = new Size(150, 25);
            lblQLeft.Font = new Font("Arial", 12F);
            lblQLeft.ForeColor = Color.White;
            lblQLeft.Text = "Q Left: 15";

            // Form settings
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(1024, 768);
            Controls.Add(lblQuestion);
            Controls.Add(pnlAnswerA);
            Controls.Add(pnlAnswerB);
            Controls.Add(pnlAnswerC);
            Controls.Add(pnlAnswerD);
            Controls.Add(lblCorrectAnswer);
            Controls.Add(lblATAa);
            Controls.Add(lblATAb);
            Controls.Add(lblATAc);
            Controls.Add(lblATAd);
            Controls.Add(lblCurrent);
            Controls.Add(lblCorrect);
            Controls.Add(lblWrong);
            Controls.Add(lblDrop);
            Controls.Add(lblQLeft);
            Name = "HostScreenForm";
            Text = "Host Screen";
            ResumeLayout(false);
        }

        #endregion

        private Label lblQuestion;
        private Panel pnlAnswerA;
        private Label lblAnswerA;
        private Panel pnlAnswerB;
        private Label lblAnswerB;
        private Panel pnlAnswerC;
        private Label lblAnswerC;
        private Panel pnlAnswerD;
        private Label lblAnswerD;
        private Label lblCorrectAnswer;
        private Label lblATAa;
        private Label lblATAb;
        private Label lblATAc;
        private Label lblATAd;
        private Label lblCurrent;
        private Label lblCorrect;
        private Label lblWrong;
        private Label lblDrop;
        private Label lblQLeft;
    }
}
