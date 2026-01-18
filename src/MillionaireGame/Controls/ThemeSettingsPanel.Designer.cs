namespace MillionaireGame.Controls
{
    partial class ThemeSettingsPanel
    {
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.grpThemeList = new GroupBox();
            this.lstThemes = new ListView();
            this.btnRefresh = new Button();
            this.grpPreview = new GroupBox();
            this.picPreview = new PictureBox();
            this.grpThemeDetails = new GroupBox();
            this.txtThemeDetails = new TextBox();
            this.grpActions = new GroupBox();
            this.btnApplyTheme = new Button();
            this.btnDuplicateTheme = new Button();
            this.btnDeleteTheme = new Button();
            this.btnImportPack = new Button();
            this.btnExportTheme = new Button();
            this.btnExportExample = new Button();
            this.grpThemeList.SuspendLayout();
            this.grpPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            this.grpThemeDetails.SuspendLayout();
            this.grpActions.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpThemeList
            // 
            this.grpThemeList.Controls.Add(this.lstThemes);
            this.grpThemeList.Controls.Add(this.btnRefresh);
            this.grpThemeList.Location = new Point(10, 10);
            this.grpThemeList.Name = "grpThemeList";
            this.grpThemeList.Size = new Size(550, 300);
            this.grpThemeList.TabIndex = 0;
            this.grpThemeList.TabStop = false;
            this.grpThemeList.Text = "Available Themes";
            // 
            // lstThemes
            // 
            this.lstThemes.Location = new Point(10, 25);
            this.lstThemes.Name = "lstThemes";
            this.lstThemes.Size = new Size(530, 230);
            this.lstThemes.TabIndex = 0;
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new Point(455, 265);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new Size(85, 25);
            this.btnRefresh.TabIndex = 1;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            // 
            // grpPreview
            // 
            this.grpPreview.Controls.Add(this.picPreview);
            this.grpPreview.Location = new Point(570, 10);
            this.grpPreview.Name = "grpPreview";
            this.grpPreview.Size = new Size(420, 300);
            this.grpPreview.TabIndex = 1;
            this.grpPreview.TabStop = false;
            this.grpPreview.Text = "Theme Preview";
            // 
            // picPreview
            // 
            this.picPreview.BackColor = Color.Black;
            this.picPreview.Location = new Point(10, 25);
            this.picPreview.Name = "picPreview";
            this.picPreview.Size = new Size(400, 265);
            this.picPreview.SizeMode = PictureBoxSizeMode.CenterImage;
            this.picPreview.TabIndex = 0;
            this.picPreview.TabStop = false;
            // 
            // grpThemeDetails
            // 
            this.grpThemeDetails.Controls.Add(this.txtThemeDetails);
            this.grpThemeDetails.Location = new Point(10, 320);
            this.grpThemeDetails.Name = "grpThemeDetails";
            this.grpThemeDetails.Size = new Size(550, 180);
            this.grpThemeDetails.TabIndex = 2;
            this.grpThemeDetails.TabStop = false;
            this.grpThemeDetails.Text = "Theme Details";
            // 
            // txtThemeDetails
            // 
            this.txtThemeDetails.BackColor = SystemColors.Window;
            this.txtThemeDetails.Location = new Point(10, 25);
            this.txtThemeDetails.Multiline = true;
            this.txtThemeDetails.Name = "txtThemeDetails";
            this.txtThemeDetails.ReadOnly = true;
            this.txtThemeDetails.ScrollBars = ScrollBars.Vertical;
            this.txtThemeDetails.Size = new Size(530, 145);
            this.txtThemeDetails.TabIndex = 0;
            // 
            // grpActions
            // 
            this.grpActions.Controls.Add(this.btnApplyTheme);
            this.grpActions.Controls.Add(this.btnDuplicateTheme);
            this.grpActions.Controls.Add(this.btnDeleteTheme);
            this.grpActions.Controls.Add(this.btnImportPack);
            this.grpActions.Controls.Add(this.btnExportTheme);
            this.grpActions.Controls.Add(this.btnExportExample);
            this.grpActions.Location = new Point(570, 320);
            this.grpActions.Name = "grpActions";
            this.grpActions.Size = new Size(420, 180);
            this.grpActions.TabIndex = 3;
            this.grpActions.TabStop = false;
            this.grpActions.Text = "Theme Actions";
            // 
            // btnApplyTheme
            // 
            this.btnApplyTheme.Location = new Point(20, 30);
            this.btnApplyTheme.Name = "btnApplyTheme";
            this.btnApplyTheme.Size = new Size(180, 30);
            this.btnApplyTheme.TabIndex = 0;
            this.btnApplyTheme.Text = "Apply Selected Theme";
            this.btnApplyTheme.UseVisualStyleBackColor = true;
            // 
            // btnDuplicateTheme
            // 
            this.btnDuplicateTheme.Location = new Point(220, 30);
            this.btnDuplicateTheme.Name = "btnDuplicateTheme";
            this.btnDuplicateTheme.Size = new Size(180, 30);
            this.btnDuplicateTheme.TabIndex = 1;
            this.btnDuplicateTheme.Text = "Duplicate Theme";
            this.btnDuplicateTheme.UseVisualStyleBackColor = true;
            // 
            // btnDeleteTheme
            // 
            this.btnDeleteTheme.Location = new Point(20, 70);
            this.btnDeleteTheme.Name = "btnDeleteTheme";
            this.btnDeleteTheme.Size = new Size(180, 30);
            this.btnDeleteTheme.TabIndex = 2;
            this.btnDeleteTheme.Text = "Delete Theme";
            this.btnDeleteTheme.UseVisualStyleBackColor = true;
            // 
            // btnImportPack
            // 
            this.btnImportPack.Location = new Point(20, 120);
            this.btnImportPack.Name = "btnImportPack";
            this.btnImportPack.Size = new Size(180, 30);
            this.btnImportPack.TabIndex = 3;
            this.btnImportPack.Text = "Import Theme Pack...";
            this.btnImportPack.UseVisualStyleBackColor = true;
            // 
            // btnExportTheme
            // 
            this.btnExportTheme.Location = new Point(220, 120);
            this.btnExportTheme.Name = "btnExportTheme";
            this.btnExportTheme.Size = new Size(180, 30);
            this.btnExportTheme.TabIndex = 4;
            this.btnExportTheme.Text = "Export Theme...";
            this.btnExportTheme.UseVisualStyleBackColor = true;
            // 
            // btnExportExample
            // 
            this.btnExportExample.Location = new Point(220, 70);
            this.btnExportExample.Name = "btnExportExample";
            this.btnExportExample.Size = new Size(180, 30);
            this.btnExportExample.TabIndex = 5;
            this.btnExportExample.Text = "Export Example Template...";
            this.btnExportExample.UseVisualStyleBackColor = true;
            // 
            // ThemeSettingsPanel
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this.grpThemeList);
            this.Controls.Add(this.grpPreview);
            this.Controls.Add(this.grpThemeDetails);
            this.Controls.Add(this.grpActions);
            this.Name = "ThemeSettingsPanel";
            this.Size = new Size(1000, 510);
            this.grpThemeList.ResumeLayout(false);
            this.grpPreview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
            this.grpThemeDetails.ResumeLayout(false);
            this.grpThemeDetails.PerformLayout();
            this.grpActions.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private GroupBox grpThemeList;
        private ListView lstThemes;
        private Button btnRefresh;
        private GroupBox grpPreview;
        private PictureBox picPreview;
        private GroupBox grpThemeDetails;
        private TextBox txtThemeDetails;
        private GroupBox grpActions;
        private Button btnApplyTheme;
        private Button btnDuplicateTheme;
        private Button btnDeleteTheme;
        private Button btnImportPack;
        private Button btnExportTheme;
        private Button btnExportExample;
    }
}
