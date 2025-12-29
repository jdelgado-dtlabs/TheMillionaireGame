namespace MillionaireGame.Forms.QuestionEditor;

partial class EditFFFQuestionForm
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
        this.lblQuestion = new Label();
        this.txtQuestion = new TextBox();
        this.lblAnswer1 = new Label();
        this.txtAnswer1 = new TextBox();
        this.lblAnswer2 = new Label();
        this.txtAnswer2 = new TextBox();
        this.lblAnswer3 = new Label();
        this.txtAnswer3 = new TextBox();
        this.lblAnswer4 = new Label();
        this.txtAnswer4 = new TextBox();
        this.lblCorrectOrder = new Label();
        this.txtCorrectOrder = new TextBox();
        this.chkUsed = new CheckBox();
        this.btnSave = new Button();
        this.btnCancel = new Button();
        this.SuspendLayout();
        
        // lblQuestion
        this.lblQuestion.AutoSize = true;
        this.lblQuestion.Location = new Point(20, 20);
        this.lblQuestion.Name = "lblQuestion";
        this.lblQuestion.Size = new Size(75, 20);
        this.lblQuestion.TabIndex = 0;
        this.lblQuestion.Text = "Question:";
        
        // txtQuestion
        this.txtQuestion.Location = new Point(20, 45);
        this.txtQuestion.Multiline = true;
        this.txtQuestion.Name = "txtQuestion";
        this.txtQuestion.Size = new Size(560, 60);
        this.txtQuestion.TabIndex = 1;
        
        // lblAnswer1
        this.lblAnswer1.AutoSize = true;
        this.lblAnswer1.Location = new Point(20, 120);
        this.lblAnswer1.Name = "lblAnswer1";
        this.lblAnswer1.Size = new Size(74, 20);
        this.lblAnswer1.TabIndex = 2;
        this.lblAnswer1.Text = "Answer 1:";
        
        // txtAnswer1
        this.txtAnswer1.Location = new Point(20, 145);
        this.txtAnswer1.Name = "txtAnswer1";
        this.txtAnswer1.Size = new Size(560, 27);
        this.txtAnswer1.TabIndex = 3;
        
        // lblAnswer2
        this.lblAnswer2.AutoSize = true;
        this.lblAnswer2.Location = new Point(20, 185);
        this.lblAnswer2.Name = "lblAnswer2";
        this.lblAnswer2.Size = new Size(74, 20);
        this.lblAnswer2.TabIndex = 4;
        this.lblAnswer2.Text = "Answer 2:";
        
        // txtAnswer2
        this.txtAnswer2.Location = new Point(20, 210);
        this.txtAnswer2.Name = "txtAnswer2";
        this.txtAnswer2.Size = new Size(560, 27);
        this.txtAnswer2.TabIndex = 5;
        
        // lblAnswer3
        this.lblAnswer3.AutoSize = true;
        this.lblAnswer3.Location = new Point(20, 250);
        this.lblAnswer3.Name = "lblAnswer3";
        this.lblAnswer3.Size = new Size(74, 20);
        this.lblAnswer3.TabIndex = 6;
        this.lblAnswer3.Text = "Answer 3:";
        
        // txtAnswer3
        this.txtAnswer3.Location = new Point(20, 275);
        this.txtAnswer3.Name = "txtAnswer3";
        this.txtAnswer3.Size = new Size(560, 27);
        this.txtAnswer3.TabIndex = 7;
        
        // lblAnswer4
        this.lblAnswer4.AutoSize = true;
        this.lblAnswer4.Location = new Point(20, 315);
        this.lblAnswer4.Name = "lblAnswer4";
        this.lblAnswer4.Size = new Size(74, 20);
        this.lblAnswer4.TabIndex = 8;
        this.lblAnswer4.Text = "Answer 4:";
        
        // txtAnswer4
        this.txtAnswer4.Location = new Point(20, 340);
        this.txtAnswer4.Name = "txtAnswer4";
        this.txtAnswer4.Size = new Size(560, 27);
        this.txtAnswer4.TabIndex = 9;
        
        // lblCorrectOrder
        this.lblCorrectOrder.AutoSize = true;
        this.lblCorrectOrder.Location = new Point(20, 380);
        this.lblCorrectOrder.Name = "lblCorrectOrder";
        this.lblCorrectOrder.Size = new Size(164, 20);
        this.lblCorrectOrder.TabIndex = 10;
        this.lblCorrectOrder.Text = "Correct Order (e.g., 3,1,4,2):";
        
        // txtCorrectOrder
        this.txtCorrectOrder.Location = new Point(20, 405);
        this.txtCorrectOrder.Name = "txtCorrectOrder";
        this.txtCorrectOrder.Size = new Size(200, 27);
        this.txtCorrectOrder.TabIndex = 11;
        
        // chkUsed
        this.chkUsed.AutoSize = true;
        this.chkUsed.Location = new Point(240, 408);
        this.chkUsed.Name = "chkUsed";
        this.chkUsed.Size = new Size(166, 24);
        this.chkUsed.TabIndex = 12;
        this.chkUsed.Text = "Mark as Used";
        this.chkUsed.UseVisualStyleBackColor = true;
        
        // btnSave
        this.btnSave.Location = new Point(380, 460);
        this.btnSave.Name = "btnSave";
        this.btnSave.Size = new Size(100, 35);
        this.btnSave.TabIndex = 13;
        this.btnSave.Text = "Save";
        this.btnSave.UseVisualStyleBackColor = true;
        this.btnSave.Click += new EventHandler(this.btnSave_Click);
        
        // btnCancel
        this.btnCancel.Location = new Point(490, 460);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new Size(100, 35);
        this.btnCancel.TabIndex = 14;
        this.btnCancel.Text = "Cancel";
        this.btnCancel.UseVisualStyleBackColor = true;
        this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
        
        // EditFFFQuestionForm
        this.AutoScaleDimensions = new SizeF(8F, 20F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new Size(600, 515);
        this.Controls.Add(this.btnCancel);
        this.Controls.Add(this.btnSave);
        this.Controls.Add(this.chkUsed);
        this.Controls.Add(this.txtCorrectOrder);
        this.Controls.Add(this.lblCorrectOrder);
        this.Controls.Add(this.txtAnswer4);
        this.Controls.Add(this.lblAnswer4);
        this.Controls.Add(this.txtAnswer3);
        this.Controls.Add(this.lblAnswer3);
        this.Controls.Add(this.txtAnswer2);
        this.Controls.Add(this.lblAnswer2);
        this.Controls.Add(this.txtAnswer1);
        this.Controls.Add(this.lblAnswer1);
        this.Controls.Add(this.txtQuestion);
        this.Controls.Add(this.lblQuestion);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "EditFFFQuestionForm";
        this.StartPosition = FormStartPosition.CenterParent;
        this.Text = "Edit FFF Question";
        this.Load += new EventHandler(this.EditFFFQuestionForm_Load);
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion

    private Label lblQuestion;
    private TextBox txtQuestion;
    private Label lblAnswer1;
    private TextBox txtAnswer1;
    private Label lblAnswer2;
    private TextBox txtAnswer2;
    private Label lblAnswer3;
    private TextBox txtAnswer3;
    private Label lblAnswer4;
    private TextBox txtAnswer4;
    private Label lblCorrectOrder;
    private TextBox txtCorrectOrder;
    private CheckBox chkUsed;
    private Button btnSave;
    private Button btnCancel;
}
