namespace MillionaireGame.QuestionEditor.Forms;

partial class ImportQuestionsForm
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
        this.lblFilePath = new Label();
        this.txtFilePath = new TextBox();
        this.btnBrowse = new Button();
        this.btnImport = new Button();
        this.btnCancel = new Button();
        this.SuspendLayout();
        
        // lblFilePath
        this.lblFilePath.AutoSize = true;
        this.lblFilePath.Location = new Point(20, 20);
        this.lblFilePath.Name = "lblFilePath";
        this.lblFilePath.Size = new Size(113, 20);
        this.lblFilePath.TabIndex = 0;
        this.lblFilePath.Text = "CSV File Path:";
        
        // txtFilePath
        this.txtFilePath.Location = new Point(20, 45);
        this.txtFilePath.Name = "txtFilePath";
        this.txtFilePath.ReadOnly = true;
        this.txtFilePath.Size = new Size(460, 27);
        this.txtFilePath.TabIndex = 1;
        
        // btnBrowse
        this.btnBrowse.Location = new Point(490, 43);
        this.btnBrowse.Name = "btnBrowse";
        this.btnBrowse.Size = new Size(90, 30);
        this.btnBrowse.TabIndex = 2;
        this.btnBrowse.Text = "Browse...";
        this.btnBrowse.UseVisualStyleBackColor = true;
        this.btnBrowse.Click += new EventHandler(this.btnBrowse_Click);
        
        // btnImport
        this.btnImport.Location = new Point(380, 100);
        this.btnImport.Name = "btnImport";
        this.btnImport.Size = new Size(100, 35);
        this.btnImport.TabIndex = 3;
        this.btnImport.Text = "Import";
        this.btnImport.UseVisualStyleBackColor = true;
        this.btnImport.Click += new EventHandler(this.btnImport_Click);
        
        // btnCancel
        this.btnCancel.Location = new Point(490, 100);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new Size(100, 35);
        this.btnCancel.TabIndex = 4;
        this.btnCancel.Text = "Cancel";
        this.btnCancel.UseVisualStyleBackColor = true;
        this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
        
        // ImportQuestionsForm
        this.AutoScaleDimensions = new SizeF(8F, 20F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new Size(600, 155);
        this.Controls.Add(this.btnCancel);
        this.Controls.Add(this.btnImport);
        this.Controls.Add(this.btnBrowse);
        this.Controls.Add(this.txtFilePath);
        this.Controls.Add(this.lblFilePath);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "ImportQuestionsForm";
        this.StartPosition = FormStartPosition.CenterParent;
        this.Text = "Import Questions from CSV";
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion

    private Label lblFilePath;
    private TextBox txtFilePath;
    private Button btnBrowse;
    private Button btnImport;
    private Button btnCancel;
}
