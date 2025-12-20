namespace MillionaireGame.Forms.Options
{
    partial class OptionsDialog
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
            tabControl = new TabControl();
            tabGeneral = new TabPage();
            grpScreens = new GroupBox();
            chkFullScreenGuestScreen = new CheckBox();
            chkFullScreenHostScreen = new CheckBox();
            chkAutoShowGuestScreen = new CheckBox();
            chkAutoShowHostScreen = new CheckBox();
            tabBroadcast = new TabPage();
            grpBroadcast = new GroupBox();
            chkFullScreenTVScreen = new CheckBox();
            chkAutoShowTVScreen = new CheckBox();
            tabLifelines = new TabPage();
            grpLifeline4 = new GroupBox();
            cmbLifeline4Availability = new ComboBox();
            cmbLifeline4Type = new ComboBox();
            lblLifeline4 = new Label();
            grpLifeline3 = new GroupBox();
            cmbLifeline3Availability = new ComboBox();
            cmbLifeline3Type = new ComboBox();
            lblLifeline3 = new Label();
            grpLifeline2 = new GroupBox();
            cmbLifeline2Availability = new ComboBox();
            cmbLifeline2Type = new ComboBox();
            lblLifeline2 = new Label();
            grpLifeline1 = new GroupBox();
            cmbLifeline1Availability = new ComboBox();
            cmbLifeline1Type = new ComboBox();
            lblLifeline1 = new Label();
            numTotalLifelines = new NumericUpDown();
            lblTotalLifelines = new Label();
            tabSounds = new TabPage();
            grpSoundPack = new GroupBox();
            lstSoundPackInfo = new ListBox();
            btnExportExample = new Button();
            btnRemovePack = new Button();
            btnImportPack = new Button();
            cmbSoundPack = new ComboBox();
            lblSoundPack = new Label();
            btnOK = new Button();
            btnCancel = new Button();
            btnApply = new Button();
            tabControl.SuspendLayout();
            tabGeneral.SuspendLayout();
            grpScreens.SuspendLayout();
            tabBroadcast.SuspendLayout();
            grpBroadcast.SuspendLayout();
            tabLifelines.SuspendLayout();
            grpLifeline4.SuspendLayout();
            grpLifeline3.SuspendLayout();
            grpLifeline2.SuspendLayout();
            grpLifeline1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numTotalLifelines).BeginInit();
            tabSounds.SuspendLayout();
            grpSoundPack.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl
            // 
            tabControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl.Controls.Add(tabGeneral);
            tabControl.Controls.Add(tabBroadcast);
            tabControl.Controls.Add(tabLifelines);
            tabControl.Controls.Add(tabSounds);
            tabControl.Location = new Point(12, 12);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(660, 487);
            tabControl.TabIndex = 0;
            // 
            // tabGeneral
            // 
            tabGeneral.Controls.Add(grpScreens);
            tabGeneral.Location = new Point(4, 24);
            tabGeneral.Name = "tabGeneral";
            tabGeneral.Padding = new Padding(3);
            tabGeneral.Size = new Size(652, 459);
            tabGeneral.TabIndex = 0;
            tabGeneral.Text = "Screens";
            tabGeneral.UseVisualStyleBackColor = true;
            // 
            // grpScreens
            // 
            grpScreens.Controls.Add(chkFullScreenGuestScreen);
            grpScreens.Controls.Add(chkFullScreenHostScreen);
            grpScreens.Controls.Add(chkAutoShowGuestScreen);
            grpScreens.Controls.Add(chkAutoShowHostScreen);
            grpScreens.Location = new Point(16, 16);
            grpScreens.Name = "grpScreens";
            grpScreens.Size = new Size(300, 150);
            grpScreens.TabIndex = 0;
            grpScreens.TabStop = false;
            grpScreens.Text = "Host and Guest Screens";
            // 
            // chkFullScreenGuestScreen
            // 
            chkFullScreenGuestScreen.AutoSize = true;
            chkFullScreenGuestScreen.Location = new Point(20, 100);
            chkFullScreenGuestScreen.Name = "chkFullScreenGuestScreen";
            chkFullScreenGuestScreen.Size = new Size(170, 19);
            chkFullScreenGuestScreen.TabIndex = 3;
            chkFullScreenGuestScreen.Text = "Full Screen Guest Screen";
            chkFullScreenGuestScreen.UseVisualStyleBackColor = true;
            chkFullScreenGuestScreen.CheckedChanged += Control_Changed;
            // 
            // chkFullScreenHostScreen
            // 
            chkFullScreenHostScreen.AutoSize = true;
            chkFullScreenHostScreen.Location = new Point(20, 70);
            chkFullScreenHostScreen.Name = "chkFullScreenHostScreen";
            chkFullScreenHostScreen.Size = new Size(163, 19);
            chkFullScreenHostScreen.TabIndex = 2;
            chkFullScreenHostScreen.Text = "Full Screen Host Screen";
            chkFullScreenHostScreen.UseVisualStyleBackColor = true;
            chkFullScreenHostScreen.CheckedChanged += Control_Changed;
            // 
            // chkAutoShowGuestScreen
            // 
            chkAutoShowGuestScreen.AutoSize = true;
            chkAutoShowGuestScreen.Location = new Point(20, 45);
            chkAutoShowGuestScreen.Name = "chkAutoShowGuestScreen";
            chkAutoShowGuestScreen.Size = new Size(165, 19);
            chkAutoShowGuestScreen.TabIndex = 1;
            chkAutoShowGuestScreen.Text = "Auto Show Guest Screen";
            chkAutoShowGuestScreen.UseVisualStyleBackColor = true;
            chkAutoShowGuestScreen.CheckedChanged += Control_Changed;
            // 
            // chkAutoShowHostScreen
            // 
            chkAutoShowHostScreen.AutoSize = true;
            chkAutoShowHostScreen.Location = new Point(20, 20);
            chkAutoShowHostScreen.Name = "chkAutoShowHostScreen";
            chkAutoShowHostScreen.Size = new Size(158, 19);
            chkAutoShowHostScreen.TabIndex = 0;
            chkAutoShowHostScreen.Text = "Auto Show Host Screen";
            chkAutoShowHostScreen.UseVisualStyleBackColor = true;
            chkAutoShowHostScreen.CheckedChanged += Control_Changed;
            // 
            // tabBroadcast
            // 
            tabBroadcast.Controls.Add(grpBroadcast);
            tabBroadcast.Location = new Point(4, 24);
            tabBroadcast.Name = "tabBroadcast";
            tabBroadcast.Padding = new Padding(3);
            tabBroadcast.Size = new Size(652, 459);
            tabBroadcast.TabIndex = 1;
            tabBroadcast.Text = "Broadcast";
            tabBroadcast.UseVisualStyleBackColor = true;
            // 
            // grpBroadcast
            // 
            grpBroadcast.Controls.Add(chkFullScreenTVScreen);
            grpBroadcast.Controls.Add(chkAutoShowTVScreen);
            grpBroadcast.Location = new Point(16, 16);
            grpBroadcast.Name = "grpBroadcast";
            grpBroadcast.Size = new Size(300, 100);
            grpBroadcast.TabIndex = 0;
            grpBroadcast.TabStop = false;
            grpBroadcast.Text = "TV Screen (Broadcast)";
            // 
            // chkFullScreenTVScreen
            // 
            chkFullScreenTVScreen.AutoSize = true;
            chkFullScreenTVScreen.Location = new Point(20, 50);
            chkFullScreenTVScreen.Name = "chkFullScreenTVScreen";
            chkFullScreenTVScreen.Size = new Size(147, 19);
            chkFullScreenTVScreen.TabIndex = 1;
            chkFullScreenTVScreen.Text = "Full Screen TV Screen";
            chkFullScreenTVScreen.UseVisualStyleBackColor = true;
            chkFullScreenTVScreen.CheckedChanged += Control_Changed;
            // 
            // chkAutoShowTVScreen
            // 
            chkAutoShowTVScreen.AutoSize = true;
            chkAutoShowTVScreen.Location = new Point(20, 25);
            chkAutoShowTVScreen.Name = "chkAutoShowTVScreen";
            chkAutoShowTVScreen.Size = new Size(142, 19);
            chkAutoShowTVScreen.TabIndex = 0;
            chkAutoShowTVScreen.Text = "Auto Show TV Screen";
            chkAutoShowTVScreen.UseVisualStyleBackColor = true;
            chkAutoShowTVScreen.CheckedChanged += Control_Changed;
            // 
            // tabLifelines
            // 
            tabLifelines.Controls.Add(grpLifeline4);
            tabLifelines.Controls.Add(grpLifeline3);
            tabLifelines.Controls.Add(grpLifeline2);
            tabLifelines.Controls.Add(grpLifeline1);
            tabLifelines.Controls.Add(numTotalLifelines);
            tabLifelines.Controls.Add(lblTotalLifelines);
            tabLifelines.Location = new Point(4, 24);
            tabLifelines.Name = "tabLifelines";
            tabLifelines.Padding = new Padding(3);
            tabLifelines.Size = new Size(652, 459);
            tabLifelines.TabIndex = 1;
            tabLifelines.Text = "Lifelines";
            tabLifelines.UseVisualStyleBackColor = true;
            // 
            // grpLifeline4
            // 
            grpLifeline4.Controls.Add(cmbLifeline4Availability);
            grpLifeline4.Controls.Add(cmbLifeline4Type);
            grpLifeline4.Controls.Add(lblLifeline4);
            grpLifeline4.Location = new Point(336, 236);
            grpLifeline4.Name = "grpLifeline4";
            grpLifeline4.Size = new Size(300, 200);
            grpLifeline4.TabIndex = 5;
            grpLifeline4.TabStop = false;
            grpLifeline4.Text = "Lifeline 4";
            // 
            // cmbLifeline4Availability
            // 
            cmbLifeline4Availability.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLifeline4Availability.FormattingEnabled = true;
            cmbLifeline4Availability.Items.AddRange(new object[] { "Always Available", "After Question 5", "After Question 10", "In Risk Mode Only" });
            cmbLifeline4Availability.Location = new Point(20, 65);
            cmbLifeline4Availability.Name = "cmbLifeline4Availability";
            cmbLifeline4Availability.Size = new Size(260, 23);
            cmbLifeline4Availability.TabIndex = 2;
            cmbLifeline4Availability.SelectedIndex = 0;
            cmbLifeline4Availability.SelectedIndexChanged += Control_Changed;
            // 
            // cmbLifeline4Type
            // 
            cmbLifeline4Type.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLifeline4Type.FormattingEnabled = true;
            cmbLifeline4Type.Items.AddRange(new object[] { "50:50", "Phone a Friend", "Ask the Audience", "Switch the Question" });
            cmbLifeline4Type.Location = new Point(80, 25);
            cmbLifeline4Type.Name = "cmbLifeline4Type";
            cmbLifeline4Type.Size = new Size(200, 23);
            cmbLifeline4Type.TabIndex = 1;
            cmbLifeline4Type.SelectedIndexChanged += Control_Changed;
            // 
            // lblLifeline4
            // 
            lblLifeline4.AutoSize = true;
            lblLifeline4.Location = new Point(20, 28);
            lblLifeline4.Name = "lblLifeline4";
            lblLifeline4.Size = new Size(34, 15);
            lblLifeline4.TabIndex = 0;
            lblLifeline4.Text = "Type:";
            // 
            // grpLifeline3
            // 
            grpLifeline3.Controls.Add(cmbLifeline3Availability);
            grpLifeline3.Controls.Add(cmbLifeline3Type);
            grpLifeline3.Controls.Add(lblLifeline3);
            grpLifeline3.Location = new Point(16, 236);
            grpLifeline3.Name = "grpLifeline3";
            grpLifeline3.Size = new Size(300, 200);
            grpLifeline3.TabIndex = 4;
            grpLifeline3.TabStop = false;
            grpLifeline3.Text = "Lifeline 3";
            // 
            // cmbLifeline3Availability
            // 
            cmbLifeline3Availability.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLifeline3Availability.FormattingEnabled = true;
            cmbLifeline3Availability.Items.AddRange(new object[] { "Always Available", "After Question 5", "After Question 10", "In Risk Mode Only" });
            cmbLifeline3Availability.Location = new Point(20, 65);
            cmbLifeline3Availability.Name = "cmbLifeline3Availability";
            cmbLifeline3Availability.Size = new Size(260, 23);
            cmbLifeline3Availability.TabIndex = 2;
            cmbLifeline3Availability.SelectedIndex = 0;
            cmbLifeline3Availability.SelectedIndexChanged += Control_Changed;
            // 
            // cmbLifeline3Type
            // 
            cmbLifeline3Type.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLifeline3Type.FormattingEnabled = true;
            cmbLifeline3Type.Items.AddRange(new object[] { "50:50", "Phone a Friend", "Ask the Audience", "Switch the Question" });
            cmbLifeline3Type.Location = new Point(80, 25);
            cmbLifeline3Type.Name = "cmbLifeline3Type";
            cmbLifeline3Type.Size = new Size(200, 23);
            cmbLifeline3Type.TabIndex = 1;
            cmbLifeline3Type.SelectedIndexChanged += Control_Changed;
            // 
            // lblLifeline3
            // 
            lblLifeline3.AutoSize = true;
            lblLifeline3.Location = new Point(20, 28);
            lblLifeline3.Name = "lblLifeline3";
            lblLifeline3.Size = new Size(34, 15);
            lblLifeline3.TabIndex = 0;
            lblLifeline3.Text = "Type:";
            // 
            // grpLifeline2
            // 
            grpLifeline2.Controls.Add(cmbLifeline2Availability);
            grpLifeline2.Controls.Add(cmbLifeline2Type);
            grpLifeline2.Controls.Add(lblLifeline2);
            grpLifeline2.Location = new Point(336, 56);
            grpLifeline2.Name = "grpLifeline2";
            grpLifeline2.Size = new Size(300, 160);
            grpLifeline2.TabIndex = 3;
            grpLifeline2.TabStop = false;
            grpLifeline2.Text = "Lifeline 2";
            // 
            // cmbLifeline2Availability
            // 
            cmbLifeline2Availability.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLifeline2Availability.FormattingEnabled = true;
            cmbLifeline2Availability.Items.AddRange(new object[] { "Always Available", "After Question 5", "After Question 10", "In Risk Mode Only" });
            cmbLifeline2Availability.Location = new Point(20, 55);
            cmbLifeline2Availability.Name = "cmbLifeline2Availability";
            cmbLifeline2Availability.Size = new Size(260, 23);
            cmbLifeline2Availability.TabIndex = 2;
            cmbLifeline2Availability.SelectedIndex = 0;
            cmbLifeline2Availability.SelectedIndexChanged += Control_Changed;
            // 
            // cmbLifeline2Type
            // 
            cmbLifeline2Type.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLifeline2Type.FormattingEnabled = true;
            cmbLifeline2Type.Items.AddRange(new object[] { "50:50", "Phone a Friend", "Ask the Audience", "Switch the Question" });
            cmbLifeline2Type.Location = new Point(80, 25);
            cmbLifeline2Type.Name = "cmbLifeline2Type";
            cmbLifeline2Type.Size = new Size(200, 23);
            cmbLifeline2Type.TabIndex = 1;
            cmbLifeline2Type.SelectedIndexChanged += Control_Changed;
            // 
            // lblLifeline2
            // 
            lblLifeline2.AutoSize = true;
            lblLifeline2.Location = new Point(20, 28);
            lblLifeline2.Name = "lblLifeline2";
            lblLifeline2.Size = new Size(34, 15);
            lblLifeline2.TabIndex = 0;
            lblLifeline2.Text = "Type:";
            // 
            // grpLifeline1
            // 
            grpLifeline1.Controls.Add(cmbLifeline1Availability);
            grpLifeline1.Controls.Add(cmbLifeline1Type);
            grpLifeline1.Controls.Add(lblLifeline1);
            grpLifeline1.Location = new Point(16, 56);
            grpLifeline1.Name = "grpLifeline1";
            grpLifeline1.Size = new Size(300, 160);
            grpLifeline1.TabIndex = 2;
            grpLifeline1.TabStop = false;
            grpLifeline1.Text = "Lifeline 1";
            // 
            // cmbLifeline1Availability
            // 
            cmbLifeline1Availability.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLifeline1Availability.FormattingEnabled = true;
            cmbLifeline1Availability.Items.AddRange(new object[] { "Always Available", "After Question 5", "After Question 10", "In Risk Mode Only" });
            cmbLifeline1Availability.Location = new Point(20, 55);
            cmbLifeline1Availability.Name = "cmbLifeline1Availability";
            cmbLifeline1Availability.Size = new Size(260, 23);
            cmbLifeline1Availability.TabIndex = 2;
            cmbLifeline1Availability.SelectedIndex = 0;
            cmbLifeline1Availability.SelectedIndexChanged += Control_Changed;
            // 
            // cmbLifeline1Type
            // 
            cmbLifeline1Type.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLifeline1Type.FormattingEnabled = true;
            cmbLifeline1Type.Items.AddRange(new object[] { "50:50", "Phone a Friend", "Ask the Audience", "Switch the Question" });
            cmbLifeline1Type.Location = new Point(80, 25);
            cmbLifeline1Type.Name = "cmbLifeline1Type";
            cmbLifeline1Type.Size = new Size(200, 23);
            cmbLifeline1Type.TabIndex = 1;
            cmbLifeline1Type.SelectedIndexChanged += Control_Changed;
            // 
            // lblLifeline1
            // 
            lblLifeline1.AutoSize = true;
            lblLifeline1.Location = new Point(20, 28);
            lblLifeline1.Name = "lblLifeline1";
            lblLifeline1.Size = new Size(34, 15);
            lblLifeline1.TabIndex = 0;
            lblLifeline1.Text = "Type:";
            // 
            // numTotalLifelines
            // 
            numTotalLifelines.Location = new Point(140, 16);
            numTotalLifelines.Maximum = new decimal(new int[] { 4, 0, 0, 0 });
            numTotalLifelines.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numTotalLifelines.Name = "numTotalLifelines";
            numTotalLifelines.Size = new Size(80, 23);
            numTotalLifelines.TabIndex = 1;
            numTotalLifelines.Value = new decimal(new int[] { 4, 0, 0, 0 });
            numTotalLifelines.ValueChanged += numTotalLifelines_ValueChanged;
            // 
            // lblTotalLifelines
            // 
            lblTotalLifelines.AutoSize = true;
            lblTotalLifelines.Location = new Point(16, 18);
            lblTotalLifelines.Name = "lblTotalLifelines";
            lblTotalLifelines.Size = new Size(86, 15);
            lblTotalLifelines.TabIndex = 0;
            lblTotalLifelines.Text = "Total Lifelines:";
            // 
            // tabSounds
            // 
            tabSounds.AutoScroll = true;
            tabSounds.Controls.Add(grpSoundPack);
            tabSounds.Location = new Point(4, 24);
            tabSounds.Name = "tabSounds";
            tabSounds.Padding = new Padding(3);
            tabSounds.Size = new Size(652, 459);
            tabSounds.TabIndex = 2;
            tabSounds.Text = "Sounds";
            tabSounds.UseVisualStyleBackColor = true;
            // 
            // grpSoundPack
            // 
            grpSoundPack.Controls.Add(lstSoundPackInfo);
            grpSoundPack.Controls.Add(btnExportExample);
            grpSoundPack.Controls.Add(btnRemovePack);
            grpSoundPack.Controls.Add(btnImportPack);
            grpSoundPack.Controls.Add(cmbSoundPack);
            grpSoundPack.Location = new Point(16, 16);
            grpSoundPack.Name = "grpSoundPack";
            grpSoundPack.Size = new Size(620, 420);
            grpSoundPack.TabIndex = 0;
            grpSoundPack.TabStop = false;
            grpSoundPack.Text = "Sound Pack";
            // 
            // cmbSoundPack
            // 
            cmbSoundPack.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSoundPack.FormattingEnabled = true;
            cmbSoundPack.Location = new Point(90, 25);
            cmbSoundPack.Name = "cmbSoundPack";
            cmbSoundPack.Size = new Size(200, 23);
            cmbSoundPack.TabIndex = 0;
            cmbSoundPack.SelectedIndexChanged += cmbSoundPack_SelectedIndexChanged;
            // 
            // 
            // 
            // btnImportPack
            // 
            btnImportPack.Location = new Point(300, 24);
            btnImportPack.Name = "btnImportPack";
            btnImportPack.Size = new Size(90, 25);
            btnImportPack.TabIndex = 2;
            btnImportPack.Text = "Import...";
            btnImportPack.UseVisualStyleBackColor = true;
            btnImportPack.Click += btnImportPack_Click;
            // 
            // btnRemovePack
            // 
            btnRemovePack.Location = new Point(400, 24);
            btnRemovePack.Name = "btnRemovePack";
            btnRemovePack.Size = new Size(90, 25);
            btnRemovePack.TabIndex = 3;
            btnRemovePack.Text = "Remove";
            btnRemovePack.UseVisualStyleBackColor = true;
            btnRemovePack.Click += btnRemovePack_Click;
            // 
            // btnExportExample
            // 
            btnExportExample.Location = new Point(500, 24);
            btnExportExample.Name = "btnExportExample";
            btnExportExample.Size = new Size(110, 25);
            btnExportExample.TabIndex = 4;
            btnExportExample.Text = "Export Example";
            btnExportExample.UseVisualStyleBackColor = true;
            btnExportExample.Click += btnExportExample_Click;
            // 
            // lstSoundPackInfo
            // 
            lstSoundPackInfo.FormattingEnabled = true;
            lstSoundPackInfo.IntegralHeight = false;
            lstSoundPackInfo.Location = new Point(16, 60);
            lstSoundPackInfo.Name = "lstSoundPackInfo";
            lstSoundPackInfo.SelectionMode = SelectionMode.None;
            lstSoundPackInfo.Size = new Size(588, 345);
            lstSoundPackInfo.TabIndex = 5;
            // // btnOK
            // 
            btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnOK.Location = new Point(416, 515);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(80, 30);
            btnOK.TabIndex = 1;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.Location = new Point(502, 515);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(80, 30);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnApply
            // 
            btnApply.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnApply.Location = new Point(588, 515);
            btnApply.Name = "btnApply";
            btnApply.Size = new Size(80, 30);
            btnApply.TabIndex = 3;
            btnApply.Text = "Apply";
            btnApply.UseVisualStyleBackColor = true;
            btnApply.Click += btnApply_Click;
            // 
            // OptionsDialog
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(684, 561);
            Controls.Add(btnApply);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(tabControl);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "OptionsDialog";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Settings";
            tabControl.ResumeLayout(false);
            tabGeneral.ResumeLayout(false);
            grpScreens.ResumeLayout(false);
            grpScreens.PerformLayout();
            tabBroadcast.ResumeLayout(false);
            grpBroadcast.ResumeLayout(false);
            grpBroadcast.PerformLayout();
            tabLifelines.ResumeLayout(false);
            tabLifelines.PerformLayout();
            grpLifeline4.ResumeLayout(false);
            grpLifeline4.PerformLayout();
            grpLifeline3.ResumeLayout(false);
            grpLifeline3.PerformLayout();
            grpLifeline2.ResumeLayout(false);
            grpLifeline2.PerformLayout();
            grpLifeline1.ResumeLayout(false);
            grpLifeline1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numTotalLifelines).EndInit();
            tabSounds.ResumeLayout(false);
            grpSoundPack.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabControl;
        private TabPage tabGeneral;
        private TabPage tabBroadcast;
        private TabPage tabLifelines;
        private TabPage tabSounds;
        private Button btnOK;
        private Button btnCancel;
        private Button btnApply;
        private GroupBox grpScreens;
        private GroupBox grpBroadcast;
        private CheckBox chkAutoShowHostScreen;
        private CheckBox chkAutoShowGuestScreen;
        private CheckBox chkAutoShowTVScreen;
        private CheckBox chkFullScreenHostScreen;
        private CheckBox chkFullScreenGuestScreen;
        private CheckBox chkFullScreenTVScreen;
        private Label lblTotalLifelines;
        private NumericUpDown numTotalLifelines;
        private GroupBox grpLifeline1;
        private Label lblLifeline1;
        private ComboBox cmbLifeline1Type;
        private ComboBox cmbLifeline1Availability;
        private GroupBox grpLifeline2;
        private ComboBox cmbLifeline2Type;
        private Label lblLifeline2;
        private ComboBox cmbLifeline2Availability;
        private GroupBox grpLifeline3;
        private ComboBox cmbLifeline3Type;
        private Label lblLifeline3;
        private ComboBox cmbLifeline3Availability;
        private GroupBox grpLifeline4;
        private ComboBox cmbLifeline4Type;
        private Label lblLifeline4;
        private ComboBox cmbLifeline4Availability;
        private GroupBox grpSoundPack;
        private Label lblSoundPack;
        private ComboBox cmbSoundPack;
        private Button btnImportPack;
        private Button btnRemovePack;
        private Button btnExportExample;
        private ListBox lstSoundPackInfo;
    }
}



