namespace MillionaireGame.Forms
{
    partial class GuestScreenForm
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
            SuspendLayout();

            // Question panel
            lblQuestion = new Label();
            lblQuestion.Location = new Point(50, 50);
            lblQuestion.Size = new Size(900, 100);
            lblQuestion.Font = new Font("Arial", 22F, FontStyle.Bold);
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

            // Winning strap (money display)
            lblWinningStrap = new Label();
            lblWinningStrap.Location = new Point(350, 330);
            lblWinningStrap.Size = new Size(300, 40);
            lblWinningStrap.Font = new Font("Arial", 18F, FontStyle.Bold);
            lblWinningStrap.ForeColor = Color.Gold;
            lblWinningStrap.BackColor = Color.Transparent;
            lblWinningStrap.TextAlign = ContentAlignment.MiddleCenter;

            // Lifeline indicators
            picLifeline1 = new PictureBox();
            picLifeline1.Location = new Point(50, 400);
            picLifeline1.Size = new Size(50, 50);
            picLifeline1.BackColor = Color.Orange;
            picLifeline1.SizeMode = PictureBoxSizeMode.StretchImage;

            picLifeline2 = new PictureBox();
            picLifeline2.Location = new Point(120, 400);
            picLifeline2.Size = new Size(50, 50);
            picLifeline2.BackColor = Color.Orange;
            picLifeline2.SizeMode = PictureBoxSizeMode.StretchImage;

            picLifeline3 = new PictureBox();
            picLifeline3.Location = new Point(190, 400);
            picLifeline3.Size = new Size(50, 50);
            picLifeline3.BackColor = Color.Orange;
            picLifeline3.SizeMode = PictureBoxSizeMode.StretchImage;

            picLifeline4 = new PictureBox();
            picLifeline4.Location = new Point(260, 400);
            picLifeline4.Size = new Size(50, 50);
            picLifeline4.BackColor = Color.Orange;
            picLifeline4.SizeMode = PictureBoxSizeMode.StretchImage;

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
            Controls.Add(lblWinningStrap);
            Controls.Add(picLifeline1);
            Controls.Add(picLifeline2);
            Controls.Add(picLifeline3);
            Controls.Add(picLifeline4);
            Name = "GuestScreenForm";
            Text = "Guest Screen";
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
        private Label lblWinningStrap;
        private PictureBox picLifeline1;
        private PictureBox picLifeline2;
        private PictureBox picLifeline3;
        private PictureBox picLifeline4;
    }
}
