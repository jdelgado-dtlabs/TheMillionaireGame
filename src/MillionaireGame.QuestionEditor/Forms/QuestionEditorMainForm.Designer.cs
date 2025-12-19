namespace MillionaireGame.QuestionEditor.Forms;

partial class QuestionEditorMainForm
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
        this.tabControl = new TabControl();
        this.tabRegularQuestions = new TabPage();
        this.dgvQuestions = new DataGridView();
        this.panel1 = new Panel();
        this.lblTotalQuestions = new Label();
        this.tabFFFQuestions = new TabPage();
        this.dgvFFFQuestions = new DataGridView();
        this.panel2 = new Panel();
        this.lblTotalFFFQuestions = new Label();
        this.toolStrip1 = new ToolStrip();
        this.btnAdd = new ToolStripButton();
        this.btnEdit = new ToolStripButton();
        this.btnDelete = new ToolStripButton();
        this.toolStripSeparator1 = new ToolStripSeparator();
        this.btnImport = new ToolStripButton();
        this.btnExport = new ToolStripButton();
        this.toolStripSeparator2 = new ToolStripSeparator();
        this.btnResetUsed = new ToolStripButton();
        this.btnRefresh = new ToolStripButton();
        
        this.tabControl.SuspendLayout();
        this.tabRegularQuestions.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvQuestions)).BeginInit();
        this.panel1.SuspendLayout();
        this.tabFFFQuestions.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvFFFQuestions)).BeginInit();
        this.panel2.SuspendLayout();
        this.toolStrip1.SuspendLayout();
        this.SuspendLayout();
        
        // tabControl
        this.tabControl.Controls.Add(this.tabRegularQuestions);
        this.tabControl.Controls.Add(this.tabFFFQuestions);
        this.tabControl.Dock = DockStyle.Fill;
        this.tabControl.Location = new Point(0, 39);
        this.tabControl.Name = "tabControl";
        this.tabControl.SelectedIndex = 0;
        this.tabControl.Size = new Size(1200, 600);
        this.tabControl.TabIndex = 0;
        
        // tabRegularQuestions
        this.tabRegularQuestions.Controls.Add(this.dgvQuestions);
        this.tabRegularQuestions.Controls.Add(this.panel1);
        this.tabRegularQuestions.Location = new Point(4, 29);
        this.tabRegularQuestions.Name = "tabRegularQuestions";
        this.tabRegularQuestions.Padding = new Padding(3);
        this.tabRegularQuestions.Size = new Size(1192, 567);
        this.tabRegularQuestions.TabIndex = 0;
        this.tabRegularQuestions.Text = "Regular Questions";
        this.tabRegularQuestions.UseVisualStyleBackColor = true;
        
        // dgvQuestions
        this.dgvQuestions.AllowUserToAddRows = false;
        this.dgvQuestions.AllowUserToDeleteRows = false;
        this.dgvQuestions.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        this.dgvQuestions.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this.dgvQuestions.Dock = DockStyle.Fill;
        this.dgvQuestions.Location = new Point(3, 3);
        this.dgvQuestions.MultiSelect = false;
        this.dgvQuestions.Name = "dgvQuestions";
        this.dgvQuestions.ReadOnly = true;
        this.dgvQuestions.RowHeadersWidth = 51;
        this.dgvQuestions.RowTemplate.Height = 29;
        this.dgvQuestions.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        this.dgvQuestions.Size = new Size(1186, 531);
        this.dgvQuestions.TabIndex = 0;
        
        // panel1
        this.panel1.Controls.Add(this.lblTotalQuestions);
        this.panel1.Dock = DockStyle.Bottom;
        this.panel1.Location = new Point(3, 534);
        this.panel1.Name = "panel1";
        this.panel1.Size = new Size(1186, 30);
        this.panel1.TabIndex = 1;
        
        // lblTotalQuestions
        this.lblTotalQuestions.AutoSize = true;
        this.lblTotalQuestions.Location = new Point(10, 6);
        this.lblTotalQuestions.Name = "lblTotalQuestions";
        this.lblTotalQuestions.Size = new Size(139, 20);
        this.lblTotalQuestions.TabIndex = 0;
        this.lblTotalQuestions.Text = "Total Questions: 0";
        
        // tabFFFQuestions
        this.tabFFFQuestions.Controls.Add(this.dgvFFFQuestions);
        this.tabFFFQuestions.Controls.Add(this.panel2);
        this.tabFFFQuestions.Location = new Point(4, 29);
        this.tabFFFQuestions.Name = "tabFFFQuestions";
        this.tabFFFQuestions.Padding = new Padding(3);
        this.tabFFFQuestions.Size = new Size(1192, 567);
        this.tabFFFQuestions.TabIndex = 1;
        this.tabFFFQuestions.Text = "FFF Questions";
        this.tabFFFQuestions.UseVisualStyleBackColor = true;
        
        // dgvFFFQuestions
        this.dgvFFFQuestions.AllowUserToAddRows = false;
        this.dgvFFFQuestions.AllowUserToDeleteRows = false;
        this.dgvFFFQuestions.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        this.dgvFFFQuestions.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this.dgvFFFQuestions.Dock = DockStyle.Fill;
        this.dgvFFFQuestions.Location = new Point(3, 3);
        this.dgvFFFQuestions.MultiSelect = false;
        this.dgvFFFQuestions.Name = "dgvFFFQuestions";
        this.dgvFFFQuestions.ReadOnly = true;
        this.dgvFFFQuestions.RowHeadersWidth = 51;
        this.dgvFFFQuestions.RowTemplate.Height = 29;
        this.dgvFFFQuestions.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        this.dgvFFFQuestions.Size = new Size(1186, 531);
        this.dgvFFFQuestions.TabIndex = 0;
        
        // panel2
        this.panel2.Controls.Add(this.lblTotalFFFQuestions);
        this.panel2.Dock = DockStyle.Bottom;
        this.panel2.Location = new Point(3, 534);
        this.panel2.Name = "panel2";
        this.panel2.Size = new Size(1186, 30);
        this.panel2.TabIndex = 1;
        
        // lblTotalFFFQuestions
        this.lblTotalFFFQuestions.AutoSize = true;
        this.lblTotalFFFQuestions.Location = new Point(10, 6);
        this.lblTotalFFFQuestions.Name = "lblTotalFFFQuestions";
        this.lblTotalFFFQuestions.Size = new Size(184, 20);
        this.lblTotalFFFQuestions.TabIndex = 0;
        this.lblTotalFFFQuestions.Text = "Total FFF Questions: 0";
        
        // toolStrip1
        this.toolStrip1.ImageScalingSize = new Size(20, 20);
        this.toolStrip1.Items.AddRange(new ToolStripItem[] {
            this.btnAdd,
            this.btnEdit,
            this.btnDelete,
            this.toolStripSeparator1,
            this.btnImport,
            this.btnExport,
            this.toolStripSeparator2,
            this.btnResetUsed,
            this.btnRefresh});
        this.toolStrip1.Location = new Point(0, 0);
        this.toolStrip1.Name = "toolStrip1";
        this.toolStrip1.Size = new Size(1200, 39);
        this.toolStrip1.TabIndex = 1;
        this.toolStrip1.Text = "toolStrip1";
        
        // btnAdd
        this.btnAdd.Image = null;
        this.btnAdd.ImageTransparentColor = Color.Magenta;
        this.btnAdd.Name = "btnAdd";
        this.btnAdd.Size = new Size(49, 36);
        this.btnAdd.Text = "Add";
        this.btnAdd.Click += new EventHandler(this.btnAdd_Click);
        
        // btnEdit
        this.btnEdit.Image = null;
        this.btnEdit.ImageTransparentColor = Color.Magenta;
        this.btnEdit.Name = "btnEdit";
        this.btnEdit.Size = new Size(44, 36);
        this.btnEdit.Text = "Edit";
        this.btnEdit.Click += new EventHandler(this.btnEdit_Click);
        
        // btnDelete
        this.btnDelete.Image = null;
        this.btnDelete.ImageTransparentColor = Color.Magenta;
        this.btnDelete.Name = "btnDelete";
        this.btnDelete.Size = new Size(60, 36);
        this.btnDelete.Text = "Delete";
        this.btnDelete.Click += new EventHandler(this.btnDelete_Click);
        
        // toolStripSeparator1
        this.toolStripSeparator1.Name = "toolStripSeparator1";
        this.toolStripSeparator1.Size = new Size(6, 39);
        
        // btnImport
        this.btnImport.Image = null;
        this.btnImport.ImageTransparentColor = Color.Magenta;
        this.btnImport.Name = "btnImport";
        this.btnImport.Size = new Size(63, 36);
        this.btnImport.Text = "Import";
        this.btnImport.Click += new EventHandler(this.btnImport_Click);
        
        // btnExport
        this.btnExport.Image = null;
        this.btnExport.ImageTransparentColor = Color.Magenta;
        this.btnExport.Name = "btnExport";
        this.btnExport.Size = new Size(60, 36);
        this.btnExport.Text = "Export";
        this.btnExport.Click += new EventHandler(this.btnExport_Click);
        
        // toolStripSeparator2
        this.toolStripSeparator2.Name = "toolStripSeparator2";
        this.toolStripSeparator2.Size = new Size(6, 39);
        
        // btnResetUsed
        this.btnResetUsed.Image = null;
        this.btnResetUsed.ImageTransparentColor = Color.Magenta;
        this.btnResetUsed.Name = "btnResetUsed";
        this.btnResetUsed.Size = new Size(88, 36);
        this.btnResetUsed.Text = "Reset Used";
        this.btnResetUsed.Click += new EventHandler(this.btnResetUsed_Click);
        
        // btnRefresh
        this.btnRefresh.Image = null;
        this.btnRefresh.ImageTransparentColor = Color.Magenta;
        this.btnRefresh.Name = "btnRefresh";
        this.btnRefresh.Size = new Size(66, 36);
        this.btnRefresh.Text = "Refresh";
        this.btnRefresh.Click += new EventHandler(this.btnRefresh_Click);
        
        // QuestionEditorMainForm
        this.AutoScaleDimensions = new SizeF(8F, 20F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new Size(1200, 639);
        this.Controls.Add(this.tabControl);
        this.Controls.Add(this.toolStrip1);
        this.Name = "QuestionEditorMainForm";
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Text = "Millionaire Game - Question Editor";
        this.Load += new EventHandler(this.QuestionEditorMainForm_Load);
        this.tabControl.ResumeLayout(false);
        this.tabRegularQuestions.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.dgvQuestions)).EndInit();
        this.panel1.ResumeLayout(false);
        this.panel1.PerformLayout();
        this.tabFFFQuestions.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.dgvFFFQuestions)).EndInit();
        this.panel2.ResumeLayout(false);
        this.panel2.PerformLayout();
        this.toolStrip1.ResumeLayout(false);
        this.toolStrip1.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion

    private TabControl tabControl;
    private TabPage tabRegularQuestions;
    private TabPage tabFFFQuestions;
    private DataGridView dgvQuestions;
    private DataGridView dgvFFFQuestions;
    private ToolStrip toolStrip1;
    private ToolStripButton btnAdd;
    private ToolStripButton btnEdit;
    private ToolStripButton btnDelete;
    private ToolStripSeparator toolStripSeparator1;
    private ToolStripButton btnImport;
    private ToolStripButton btnExport;
    private ToolStripSeparator toolStripSeparator2;
    private ToolStripButton btnResetUsed;
    private ToolStripButton btnRefresh;
    private Panel panel1;
    private Label lblTotalQuestions;
    private Panel panel2;
    private Label lblTotalFFFQuestions;
}
