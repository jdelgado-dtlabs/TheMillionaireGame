namespace MillionaireGame.Forms
{
    partial class TVScreenForm
    {
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            SuspendLayout();

            // Question panel
            lblQuestion = new Label();
            lblQuestion.Location = new Point(100, 100);
            lblQuestion.Size = new Size(824, 120);
            lblQuestion.Font = new Font("Arial", 26F, FontStyle.Bold);
            lblQuestion.ForeColor = Color.White;
            lblQuestion.BackColor = Color.Transparent;
            lblQuestion.TextAlign = ContentAlignment.MiddleCenter;

            // Answer panels (larger for TV display)
            pnlAnswerA = new Panel();
            pnlAnswerA.Location = new Point(100, 240);
            pnlAnswerA.Size = new Size(400, 80);
            pnlAnswerA.BackColor = Color.FromArgb(0, 0, 80);

            lblAnswerA = new Label();
            lblAnswerA.Dock = DockStyle.Fill;
            lblAnswerA.Font = new Font("Arial", 20F, FontStyle.Bold);
            lblAnswerA.ForeColor = Color.White;
            lblAnswerA.Text = "A:";
            lblAnswerA.TextAlign = ContentAlignment.MiddleLeft;
            lblAnswerA.Padding = new Padding(15, 0, 0, 0);
            pnlAnswerA.Controls.Add(lblAnswerA);

            pnlAnswerB = new Panel();
            pnlAnswerB.Location = new Point(524, 240);
            pnlAnswerB.Size = new Size(400, 80);
            pnlAnswerB.BackColor = Color.FromArgb(0, 0, 80);

            lblAnswerB = new Label();
            lblAnswerB.Dock = DockStyle.Fill;
            lblAnswerB.Font = new Font("Arial", 20F, FontStyle.Bold);
            lblAnswerB.ForeColor = Color.White;
            lblAnswerB.Text = "B:";
            lblAnswerB.TextAlign = ContentAlignment.MiddleLeft;
            lblAnswerB.Padding = new Padding(15, 0, 0, 0);
            pnlAnswerB.Controls.Add(lblAnswerB);

            pnlAnswerC = new Panel();
            pnlAnswerC.Location = new Point(100, 340);
            pnlAnswerC.Size = new Size(400, 80);
            pnlAnswerC.BackColor = Color.FromArgb(0, 0, 80);

            lblAnswerC = new Label();
            lblAnswerC.Dock = DockStyle.Fill;
            lblAnswerC.Font = new Font("Arial", 20F, FontStyle.Bold);
            lblAnswerC.ForeColor = Color.White;
            lblAnswerC.Text = "C:";
            lblAnswerC.TextAlign = ContentAlignment.MiddleLeft;
            lblAnswerC.Padding = new Padding(15, 0, 0, 0);
            pnlAnswerC.Controls.Add(lblAnswerC);

            pnlAnswerD = new Panel();
            pnlAnswerD.Location = new Point(524, 340);
            pnlAnswerD.Size = new Size(400, 80);
            pnlAnswerD.BackColor = Color.FromArgb(0, 0, 80);

            lblAnswerD = new Label();
            lblAnswerD.Dock = DockStyle.Fill;
            lblAnswerD.Font = new Font("Arial", 20F, FontStyle.Bold);
            lblAnswerD.ForeColor = Color.White;
            lblAnswerD.Text = "D:";
            lblAnswerD.TextAlign = ContentAlignment.MiddleLeft;
            lblAnswerD.Padding = new Padding(15, 0, 0, 0);
            pnlAnswerD.Controls.Add(lblAnswerD);

            // Money amount display
            lblAmount = new Label();
            lblAmount.Location = new Point(362, 450);
            lblAmount.Size = new Size(300, 50);
            lblAmount.Font = new Font("Arial", 28F, FontStyle.Bold);
            lblAmount.ForeColor = Color.Gold;
            lblAmount.BackColor = Color.Transparent;
            lblAmount.TextAlign = ContentAlignment.MiddleCenter;

            // Ask the Audience panel (hidden by default)
            pnlATA = new Panel();
            pnlATA.Location = new Point(100, 520);
            pnlATA.Size = new Size(824, 100);
            pnlATA.BackColor = Color.FromArgb(20, 20, 40);
            pnlATA.Visible = false;

            lblATA_A = new Label();
            lblATA_A.Location = new Point(50, 30);
            lblATA_A.Size = new Size(150, 40);
            lblATA_A.Font = new Font("Arial", 18F, FontStyle.Bold);
            lblATA_A.ForeColor = Color.Yellow;
            lblATA_A.TextAlign = ContentAlignment.MiddleCenter;
            pnlATA.Controls.Add(lblATA_A);

            lblATA_B = new Label();
            lblATA_B.Location = new Point(230, 30);
            lblATA_B.Size = new Size(150, 40);
            lblATA_B.Font = new Font("Arial", 18F, FontStyle.Bold);
            lblATA_B.ForeColor = Color.Yellow;
            lblATA_B.TextAlign = ContentAlignment.MiddleCenter;
            pnlATA.Controls.Add(lblATA_B);

            lblATA_C = new Label();
            lblATA_C.Location = new Point(410, 30);
            lblATA_C.Size = new Size(150, 40);
            lblATA_C.Font = new Font("Arial", 18F, FontStyle.Bold);
            lblATA_C.ForeColor = Color.Yellow;
            lblATA_C.TextAlign = ContentAlignment.MiddleCenter;
            pnlATA.Controls.Add(lblATA_C);

            lblATA_D = new Label();
            lblATA_D.Location = new Point(590, 30);
            lblATA_D.Size = new Size(150, 40);
            lblATA_D.Font = new Font("Arial", 18F, FontStyle.Bold);
            lblATA_D.ForeColor = Color.Yellow;
            lblATA_D.TextAlign = ContentAlignment.MiddleCenter;
            pnlATA.Controls.Add(lblATA_D);

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
            Controls.Add(lblAmount);
            Controls.Add(pnlATA);
            Name = "TVScreenForm";
            Text = "TV Screen";
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
        private Label lblAmount;
        private Panel pnlATA;
        private Label lblATA_A;
        private Label lblATA_B;
        private Label lblATA_C;
        private Label lblATA_D;
    }
}
