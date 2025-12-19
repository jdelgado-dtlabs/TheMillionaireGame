namespace MillionaireGame.QuestionEditor.Forms;

partial class EditQuestionForm
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
        this.lblCorrectAnswer = new Label();
        this.txtCorrectAnswer = new TextBox();
        this.lblWrong1 = new Label();
        this.txtWrong1 = new TextBox();
        this.lblWrong2 = new Label();
        this.txtWrong2 = new TextBox();
        this.lblWrong3 = new Label();
        this.txtWrong3 = new TextBox();
        this.lblLevel = new Label();
        this.cmbLevel = new ComboBox();
        this.lblDifficultyType = new Label();
        this.cmbDifficultyType = new ComboBox();
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
        this.txtQuestion.Size = new Size(560, 80);
        this.txtQuestion.TabIndex = 1;
        
        // lblCorrectAnswer
        this.lblCorrectAnswer.AutoSize = true;
        this.lblCorrectAnswer.Location = new Point(20, 140);
        this.lblCorrectAnswer.Name = "lblCorrectAnswer";
        this.lblCorrectAnswer.Size = new Size(122, 20);
        this.lblCorrectAnswer.TabIndex = 2;
        this.lblCorrectAnswer.Text = "Correct Answer:";
        
        // txtCorrectAnswer
        this.txtCorrectAnswer.Location = new Point(20, 165);
        this.txtCorrectAnswer.Name = "txtCorrectAnswer";
        this.txtCorrectAnswer.Size = new Size(560, 27);
        this.txtCorrectAnswer.TabIndex = 3;
        
        // lblWrong1
        this.lblWrong1.AutoSize = true;
        this.lblWrong1.Location = new Point(20, 205);
        this.lblWrong1.Name = "lblWrong1";
        this.lblWrong1.Size = new Size(118, 20);
        this.lblWrong1.TabIndex = 4;
        this.lblWrong1.Text = "Wrong Answer 1:";
        
        // txtWrong1
        this.txtWrong1.Location = new Point(20, 230);
        this.txtWrong1.Name = "txtWrong1";
        this.txtWrong1.Size = new Size(560, 27);
        this.txtWrong1.TabIndex = 5;
        
        // lblWrong2
        this.lblWrong2.AutoSize = true;
        this.lblWrong2.Location = new Point(20, 270);
        this.lblWrong2.Name = "lblWrong2";
        this.lblWrong2.Size = new Size(118, 20);
        this.lblWrong2.TabIndex = 6;
        this.lblWrong2.Text = "Wrong Answer 2:";
        
        // txtWrong2
        this.txtWrong2.Location = new Point(20, 295);
        this.txtWrong2.Name = "txtWrong2";
        this.txtWrong2.Size = new Size(560, 27);
        this.txtWrong2.TabIndex = 7;
        
        // lblWrong3
        this.lblWrong3.AutoSize = true;
        this.lblWrong3.Location = new Point(20, 335);
        this.lblWrong3.Name = "lblWrong3";
        this.lblWrong3.Size = new Size(118, 20);
        this.lblWrong3.TabIndex = 8;
        this.lblWrong3.Text = "Wrong Answer 3:";
        
        // txtWrong3
        this.txtWrong3.Location = new Point(20, 360);
        this.txtWrong3.Name = "txtWrong3";
        this.txtWrong3.Size = new Size(560, 27);
        this.txtWrong3.TabIndex = 9;
        
        // lblLevel
        this.lblLevel.AutoSize = true;
        this.lblLevel.Location = new Point(20, 400);
        this.lblLevel.Name = "lblLevel";
        this.lblLevel.Size = new Size(48, 20);
        this.lblLevel.TabIndex = 10;
        this.lblLevel.Text = "Level:";
        
        // cmbLevel
        this.cmbLevel.DropDownStyle = ComboBoxStyle.DropDownList;
        this.cmbLevel.FormattingEnabled = true;
        this.cmbLevel.Location = new Point(20, 425);
        this.cmbLevel.Name = "cmbLevel";
        this.cmbLevel.Size = new Size(150, 28);
        this.cmbLevel.TabIndex = 11;
        
        // lblDifficultyType
        this.lblDifficultyType.AutoSize = true;
        this.lblDifficultyType.Location = new Point(190, 400);
        this.lblDifficultyType.Name = "lblDifficultyType";
        this.lblDifficultyType.Size = new Size(103, 20);
        this.lblDifficultyType.TabIndex = 12;
        this.lblDifficultyType.Text = "Difficulty Type:";
        
        // cmbDifficultyType
        this.cmbDifficultyType.DropDownStyle = ComboBoxStyle.DropDownList;
        this.cmbDifficultyType.FormattingEnabled = true;
        this.cmbDifficultyType.Location = new Point(190, 425);
        this.cmbDifficultyType.Name = "cmbDifficultyType";
        this.cmbDifficultyType.Size = new Size(150, 28);
        this.cmbDifficultyType.TabIndex = 13;
        
        // chkUsed
        this.chkUsed.AutoSize = true;
        this.chkUsed.Location = new Point(370, 428);
        this.chkUsed.Name = "chkUsed";
        this.chkUsed.Size = new Size(166, 24);
        this.chkUsed.TabIndex = 14;
        this.chkUsed.Text = "Mark as Used";
        this.chkUsed.UseVisualStyleBackColor = true;
        
        // btnSave
        this.btnSave.Location = new Point(380, 480);
        this.btnSave.Name = "btnSave";
        this.btnSave.Size = new Size(100, 35);
        this.btnSave.TabIndex = 15;
        this.btnSave.Text = "Save";
        this.btnSave.UseVisualStyleBackColor = true;
        this.btnSave.Click += new EventHandler(this.btnSave_Click);
        
        // btnCancel
        this.btnCancel.Location = new Point(490, 480);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new Size(100, 35);
        this.btnCancel.TabIndex = 16;
        this.btnCancel.Text = "Cancel";
        this.btnCancel.UseVisualStyleBackColor = true;
        this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
        
        // EditQuestionForm
        this.AutoScaleDimensions = new SizeF(8F, 20F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new Size(600, 535);
        this.Controls.Add(this.btnCancel);
        this.Controls.Add(this.btnSave);
        this.Controls.Add(this.chkUsed);
        this.Controls.Add(this.cmbDifficultyType);
        this.Controls.Add(this.lblDifficultyType);
        this.Controls.Add(this.cmbLevel);
        this.Controls.Add(this.lblLevel);
        this.Controls.Add(this.txtWrong3);
        this.Controls.Add(this.lblWrong3);
        this.Controls.Add(this.txtWrong2);
        this.Controls.Add(this.lblWrong2);
        this.Controls.Add(this.txtWrong1);
        this.Controls.Add(this.lblWrong1);
        this.Controls.Add(this.txtCorrectAnswer);
        this.Controls.Add(this.lblCorrectAnswer);
        this.Controls.Add(this.txtQuestion);
        this.Controls.Add(this.lblQuestion);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "EditQuestionForm";
        this.StartPosition = FormStartPosition.CenterParent;
        this.Text = "Edit Question";
        this.Load += new EventHandler(this.EditQuestionForm_Load);
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion

    private Label lblQuestion;
    private TextBox txtQuestion;
    private Label lblCorrectAnswer;
    private TextBox txtCorrectAnswer;
    private Label lblWrong1;
    private TextBox txtWrong1;
    private Label lblWrong2;
    private TextBox txtWrong2;
    private Label lblWrong3;
    private TextBox txtWrong3;
    private Label lblLevel;
    private ComboBox cmbLevel;
    private Label lblDifficultyType;
    private ComboBox cmbDifficultyType;
    private CheckBox chkUsed;
    private Button btnSave;
    private Button btnCancel;
}
