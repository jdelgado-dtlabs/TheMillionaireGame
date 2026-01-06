namespace MillionaireGame.Forms
{
    partial class TelemetryViewerForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblFilterDate = new System.Windows.Forms.Label();
            this.btnSelectDate = new System.Windows.Forms.Button();
            this.btnClearFilter = new System.Windows.Forms.Button();
            this.lblGameSession = new System.Windows.Forms.Label();
            this.cmbGameSessions = new System.Windows.Forms.ComboBox();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnBatchExport = new System.Windows.Forms.Button();
            this.grpSessionDetails = new System.Windows.Forms.GroupBox();
            this.lblSessionId = new System.Windows.Forms.Label();
            this.lblStartTime = new System.Windows.Forms.Label();
            this.lblEndTime = new System.Windows.Forms.Label();
            this.lblDuration = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblTotalRounds = new System.Windows.Forms.Label();
            this.lblTotalWinnings = new System.Windows.Forms.Label();
            this.dgvRounds = new System.Windows.Forms.DataGridView();
            this.grpRoundDetails = new System.Windows.Forms.GroupBox();
            this.lblRoundDetails = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.monthCalendar = new System.Windows.Forms.MonthCalendar();
            this.grpSessionDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRounds)).BeginInit();
            this.grpRoundDetails.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblFilterDate
            // 
            this.lblFilterDate.AutoSize = true;
            this.lblFilterDate.Location = new System.Drawing.Point(20, 20);
            this.lblFilterDate.Name = "lblFilterDate";
            this.lblFilterDate.Size = new System.Drawing.Size(79, 15);
            this.lblFilterDate.TabIndex = 0;
            this.lblFilterDate.Text = "Filter by Date:";
            // 
            // btnSelectDate
            // 
            this.btnSelectDate.Location = new System.Drawing.Point(105, 16);
            this.btnSelectDate.Name = "btnSelectDate";
            this.btnSelectDate.Size = new System.Drawing.Size(120, 23);
            this.btnSelectDate.TabIndex = 1;
            this.btnSelectDate.Text = "📅 Select Date";
            this.btnSelectDate.UseVisualStyleBackColor = true;
            this.btnSelectDate.Click += new System.EventHandler(this.btnSelectDate_Click);
            // 
            // btnClearFilter
            // 
            this.btnClearFilter.Location = new System.Drawing.Point(231, 16);
            this.btnClearFilter.Name = "btnClearFilter";
            this.btnClearFilter.Size = new System.Drawing.Size(90, 23);
            this.btnClearFilter.TabIndex = 2;
            this.btnClearFilter.Text = "Clear Filter";
            this.btnClearFilter.UseVisualStyleBackColor = true;
            this.btnClearFilter.Click += new System.EventHandler(this.btnClearFilter_Click);
            // 
            // lblGameSession
            // 
            this.lblGameSession.AutoSize = true;
            this.lblGameSession.Location = new System.Drawing.Point(20, 50);
            this.lblGameSession.Name = "lblGameSession";
            this.lblGameSession.Size = new System.Drawing.Size(85, 15);
            this.lblGameSession.TabIndex = 3;
            this.lblGameSession.Text = "Game Session:";
            // 
            // cmbGameSessions
            // 
            this.cmbGameSessions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbGameSessions.FormattingEnabled = true;
            this.cmbGameSessions.Location = new System.Drawing.Point(111, 47);
            this.cmbGameSessions.Name = "cmbGameSessions";
            this.cmbGameSessions.Size = new System.Drawing.Size(450, 23);
            this.cmbGameSessions.TabIndex = 4;
            this.cmbGameSessions.SelectedIndexChanged += new System.EventHandler(this.cmbGameSessions_SelectedIndexChanged);
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(567, 46);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(75, 25);
            this.btnExport.TabIndex = 5;
            this.btnExport.Text = "Export";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // btnBatchExport
            // 
            this.btnBatchExport.Location = new System.Drawing.Point(648, 46);
            this.btnBatchExport.Name = "btnBatchExport";
            this.btnBatchExport.Size = new System.Drawing.Size(100, 25);
            this.btnBatchExport.TabIndex = 6;
            this.btnBatchExport.Text = "Batch Export";
            this.btnBatchExport.UseVisualStyleBackColor = true;
            this.btnBatchExport.Click += new System.EventHandler(this.btnBatchExport_Click);
            // 
            // grpSessionDetails
            // 
            this.grpSessionDetails.Controls.Add(this.lblTotalWinnings);
            this.grpSessionDetails.Controls.Add(this.lblTotalRounds);
            this.grpSessionDetails.Controls.Add(this.lblStatus);
            this.grpSessionDetails.Controls.Add(this.lblDuration);
            this.grpSessionDetails.Controls.Add(this.lblEndTime);
            this.grpSessionDetails.Controls.Add(this.lblStartTime);
            this.grpSessionDetails.Controls.Add(this.lblSessionId);
            this.grpSessionDetails.Location = new System.Drawing.Point(20, 85);
            this.grpSessionDetails.Name = "grpSessionDetails";
            this.grpSessionDetails.Size = new System.Drawing.Size(728, 120);
            this.grpSessionDetails.TabIndex = 7;
            this.grpSessionDetails.TabStop = false;
            this.grpSessionDetails.Text = "Session Details";
            // 
            // lblSessionId
            // 
            this.lblSessionId.AutoSize = true;
            this.lblSessionId.Location = new System.Drawing.Point(15, 25);
            this.lblSessionId.Name = "lblSessionId";
            this.lblSessionId.Size = new System.Drawing.Size(63, 15);
            this.lblSessionId.TabIndex = 0;
            this.lblSessionId.Text = "Session ID:";
            // 
            // lblStartTime
            // 
            this.lblStartTime.AutoSize = true;
            this.lblStartTime.Location = new System.Drawing.Point(15, 45);
            this.lblStartTime.Name = "lblStartTime";
            this.lblStartTime.Size = new System.Drawing.Size(64, 15);
            this.lblStartTime.TabIndex = 1;
            this.lblStartTime.Text = "Start Time:";
            // 
            // lblEndTime
            // 
            this.lblEndTime.AutoSize = true;
            this.lblEndTime.Location = new System.Drawing.Point(15, 65);
            this.lblEndTime.Name = "lblEndTime";
            this.lblEndTime.Size = new System.Drawing.Size(57, 15);
            this.lblEndTime.TabIndex = 2;
            this.lblEndTime.Text = "End Time:";
            // 
            // lblDuration
            // 
            this.lblDuration.AutoSize = true;
            this.lblDuration.Location = new System.Drawing.Point(15, 85);
            this.lblDuration.Name = "lblDuration";
            this.lblDuration.Size = new System.Drawing.Size(56, 15);
            this.lblDuration.TabIndex = 3;
            this.lblDuration.Text = "Duration:";
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(400, 25);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(42, 15);
            this.lblStatus.TabIndex = 4;
            this.lblStatus.Text = "Status:";
            // 
            // lblTotalRounds
            // 
            this.lblTotalRounds.AutoSize = true;
            this.lblTotalRounds.Location = new System.Drawing.Point(400, 45);
            this.lblTotalRounds.Name = "lblTotalRounds";
            this.lblTotalRounds.Size = new System.Drawing.Size(78, 15);
            this.lblTotalRounds.TabIndex = 5;
            this.lblTotalRounds.Text = "Total Rounds:";
            // 
            // lblTotalWinnings
            // 
            this.lblTotalWinnings.AutoSize = true;
            this.lblTotalWinnings.Location = new System.Drawing.Point(400, 65);
            this.lblTotalWinnings.Name = "lblTotalWinnings";
            this.lblTotalWinnings.Size = new System.Drawing.Size(88, 15);
            this.lblTotalWinnings.TabIndex = 6;
            this.lblTotalWinnings.Text = "Total Winnings:";
            // 
            // dgvRounds
            // 
            this.dgvRounds.AllowUserToAddRows = false;
            this.dgvRounds.AllowUserToDeleteRows = false;
            this.dgvRounds.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvRounds.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRounds.Location = new System.Drawing.Point(20, 211);
            this.dgvRounds.MultiSelect = false;
            this.dgvRounds.Name = "dgvRounds";
            this.dgvRounds.ReadOnly = true;
            this.dgvRounds.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvRounds.Size = new System.Drawing.Size(728, 200);
            this.dgvRounds.TabIndex = 8;
            this.dgvRounds.SelectionChanged += new System.EventHandler(this.dgvRounds_SelectionChanged);
            // 
            // grpRoundDetails
            // 
            this.grpRoundDetails.Controls.Add(this.lblRoundDetails);
            this.grpRoundDetails.Location = new System.Drawing.Point(20, 417);
            this.grpRoundDetails.Name = "grpRoundDetails";
            this.grpRoundDetails.Size = new System.Drawing.Size(728, 120);
            this.grpRoundDetails.TabIndex = 9;
            this.grpRoundDetails.TabStop = false;
            this.grpRoundDetails.Text = "Round Details (Select row)";
            // 
            // lblRoundDetails
            // 
            this.lblRoundDetails.AutoSize = true;
            this.lblRoundDetails.Location = new System.Drawing.Point(15, 25);
            this.lblRoundDetails.Name = "lblRoundDetails";
            this.lblRoundDetails.Size = new System.Drawing.Size(0, 15);
            this.lblRoundDetails.TabIndex = 0;
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(673, 543);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 25);
            this.btnClose.TabIndex = 10;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // monthCalendar
            // 
            this.monthCalendar.Location = new System.Drawing.Point(105, 45);
            this.monthCalendar.MaxSelectionCount = 1;
            this.monthCalendar.Name = "monthCalendar";
            this.monthCalendar.TabIndex = 11;
            this.monthCalendar.Visible = false;
            this.monthCalendar.DateSelected += new System.Windows.Forms.DateRangeEventHandler(this.monthCalendar_DateSelected);
            // 
            // TelemetryViewerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(768, 580);
            this.Controls.Add(this.monthCalendar);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.grpRoundDetails);
            this.Controls.Add(this.dgvRounds);
            this.Controls.Add(this.grpSessionDetails);
            this.Controls.Add(this.btnBatchExport);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.cmbGameSessions);
            this.Controls.Add(this.lblGameSession);
            this.Controls.Add(this.btnClearFilter);
            this.Controls.Add(this.btnSelectDate);
            this.Controls.Add(this.lblFilterDate);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TelemetryViewerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Telemetry Viewer";
            this.grpSessionDetails.ResumeLayout(false);
            this.grpSessionDetails.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRounds)).EndInit();
            this.grpRoundDetails.ResumeLayout(false);
            this.grpRoundDetails.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private Label lblFilterDate;
        private Button btnSelectDate;
        private Button btnClearFilter;
        private Label lblGameSession;
        private ComboBox cmbGameSessions;
        private Button btnExport;
        private Button btnBatchExport;
        private GroupBox grpSessionDetails;
        private Label lblTotalWinnings;
        private Label lblTotalRounds;
        private Label lblStatus;
        private Label lblDuration;
        private Label lblEndTime;
        private Label lblStartTime;
        private Label lblSessionId;
        private DataGridView dgvRounds;
        private GroupBox grpRoundDetails;
        private Label lblRoundDetails;
        private Button btnClose;
        private MonthCalendar monthCalendar;
    }
}
