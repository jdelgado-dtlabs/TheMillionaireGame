using MillionaireGame.Core.Database;
using MillionaireGame.Core.Services;
using System.IO.Compression;

namespace MillionaireGame.Forms;

/// <summary>
/// Form for viewing and exporting historical telemetry data
/// </summary>
public partial class TelemetryViewerForm : Form
{
    private readonly string _connectionString;
    private readonly TelemetryRepository _repository;
    private readonly TelemetryExportService _exportService;
    private DateTime? _currentFilterDate;
    private List<GameSessionSummary> _allSessions = new();

    public TelemetryViewerForm(string connectionString)
    {
        InitializeComponent();
        
        _connectionString = connectionString;
        _repository = new TelemetryRepository(connectionString);
        _exportService = new TelemetryExportService();
        
        // Load sessions on form load
        Load += async (s, e) => await LoadSessionsAsync();
        
        // Position calendar below the Select Date button (hidden by default)
        monthCalendar.Location = new Point(btnSelectDate.Left, btnSelectDate.Bottom + 5);
        monthCalendar.BringToFront();
    }

    #region Data Loading

    private async Task LoadSessionsAsync()
    {
        try
        {
            // Run query on background thread
            var sessions = await Task.Run(async () =>
            {
                if (_currentFilterDate.HasValue)
                {
                    return await _repository.GetSessionsByDateAsync(_currentFilterDate.Value);
                }
                else
                {
                    return await _repository.GetAllGameSessionsAsync();
                }
            });

            _allSessions = sessions;
            
            // Update UI on main thread
            cmbGameSessions.DisplayMember = "DisplayText";
            cmbGameSessions.ValueMember = "SessionId";
            cmbGameSessions.DataSource = sessions.Select(s => new
            {
                SessionId = s.SessionId,
                DisplayText = $"{s.SessionIdShort} - {s.GameStartTime:yyyy-MM-dd HH:mm} - {(s.IsComplete ? "Complete" : "⚠ INCOMPLETE")}"
            }).ToList();

            // Load dates with data for calendar bolding
            var dates = await Task.Run(() => _repository.GetSessionDatesAsync());
            monthCalendar.BoldedDates = dates.ToArray();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load sessions: {ex.Message}", "Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task LoadSessionDetailsAsync(string sessionId)
    {
        try
        {
            // Show loading message
            var calculatingForm = new Form
            {
                Text = "Please Wait",
                StartPosition = FormStartPosition.CenterParent,
                Size = new Size(300, 100),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                ControlBox = false
            };
            var label = new Label 
            { 
                Text = "Loading session data...", 
                AutoSize = true, 
                Location = new Point(20, 30) 
            };
            calculatingForm.Controls.Add(label);

            // Run query async
            var loadTask = Task.Run(async () =>
            {
                var gameData = await _repository.GetGameSessionWithRoundsAsync(sessionId);
                
                // Invoke on UI thread to close form and update UI
                Invoke(() =>
                {
                    calculatingForm.Close();
                    DisplaySessionDetails(gameData);
                });
            });

            calculatingForm.ShowDialog(this);
            await loadTask;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load session details: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DisplaySessionDetails(Core.Models.Telemetry.GameTelemetry gameData)
    {
        // Session details
        lblSessionId.Text = $"Session ID: {gameData.SessionId}";
        lblStartTime.Text = $"Start Time: {gameData.GameStartTime:yyyy-MM-dd HH:mm:ss}";
        lblEndTime.Text = gameData.GameEndTime == default 
            ? "End Time: (incomplete)" 
            : $"End Time: {gameData.GameEndTime:yyyy-MM-dd HH:mm:ss}";
        
        if (gameData.GameEndTime != default)
        {
            var duration = gameData.GameEndTime - gameData.GameStartTime;
            lblDuration.Text = $"Duration: {duration.Hours}h {duration.Minutes}m {duration.Seconds}s";
        }
        else
        {
            lblDuration.Text = "Duration: (incomplete)";
        }

        lblStatus.Text = gameData.GameEndTime == default ? "Status: ⚠ Incomplete" : "Status: ✓ Complete";
        lblTotalRounds.Text = $"Total Rounds: {gameData.Rounds.Count}";

        // Calculate total winnings
        var currency1Total = gameData.Rounds.Sum(r => r.Currency1Winnings);
        var currency2Total = gameData.Rounds.Sum(r => r.Currency2Winnings);
        
        if (currency2Total > 0 && !string.IsNullOrEmpty(gameData.Currency2Name))
        {
            lblTotalWinnings.Text = $"Total Winnings: {gameData.Currency1Name}{currency1Total:N0} {gameData.Currency2Name}{currency2Total:N0}";
        }
        else
        {
            lblTotalWinnings.Text = $"Total Winnings: {gameData.Currency1Name}{currency1Total:N0}";
        }

        // Rounds grid
        dgvRounds.DataSource = gameData.Rounds.Select(r => new
        {
            RoundId = r.RoundId,  // Hidden column for lookups
            Round = r.RoundNumber,
            Start = ((int)(r.StartTime - gameData.GameStartTime).TotalMinutes).ToString("D2") + ":" + 
                    ((r.StartTime - gameData.GameStartTime).Seconds).ToString("D2"),
            End = r.EndTime == default ? "" : 
                    ((int)(r.EndTime - gameData.GameStartTime).TotalMinutes).ToString("D2") + ":" + 
                    ((r.EndTime - gameData.GameStartTime).Seconds).ToString("D2"),
            Outcome = r.Outcome?.ToString() ?? "",
            Currency1 = $"{gameData.Currency1Name}{r.Currency1Winnings:N0}",
            Currency2 = !string.IsNullOrEmpty(gameData.Currency2Name) 
                ? $"{gameData.Currency2Name}{r.Currency2Winnings:N0}" : "",
            Questions = r.FinalQuestionReached
        }).ToList();
        
        // Hide RoundId column
        if (dgvRounds.Columns["RoundId"] != null)
        {
            dgvRounds.Columns["RoundId"].Visible = false;
        }
    }

    #endregion

    #region Event Handlers

    private void btnSelectDate_Click(object sender, EventArgs e)
    {
        // Toggle calendar visibility
        monthCalendar.Visible = !monthCalendar.Visible;
    }

    private async void monthCalendar_DateSelected(object sender, DateRangeEventArgs e)
    {
        _currentFilterDate = e.Start.Date;
        monthCalendar.Visible = false;
        btnSelectDate.Text = $"📅 {_currentFilterDate.Value:yyyy-MM-dd}";
        await LoadSessionsAsync();
    }

    private async void btnClearFilter_Click(object sender, EventArgs e)
    {
        _currentFilterDate = null;
        btnSelectDate.Text = "📅 Select Date";
        await LoadSessionsAsync();
    }

    private async void cmbGameSessions_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (cmbGameSessions.SelectedValue != null && cmbGameSessions.SelectedValue is string sessionId)
        {
            await LoadSessionDetailsAsync(sessionId);
        }
    }

    private async void dgvRounds_SelectionChanged(object sender, EventArgs e)
    {
        if (dgvRounds.SelectedRows.Count > 0 && cmbGameSessions.SelectedValue is string sessionId)
        {
            var roundId = (int)dgvRounds.SelectedRows[0].Cells["RoundId"].Value;
            
            try
            {
                await Task.Run(async () =>
                {
                    // Get lifeline usages for this session
                    var lifelines = await _repository.GetLifelineUsagesForSessionAsync(sessionId);
                    var participants = await _repository.GetParticipantCountForSessionAsync(sessionId);
                    var fffStats = await _repository.GetFFFStatsForSessionAsync(sessionId);
                    var ataStats = await _repository.GetATAStatsForSessionAsync(sessionId);
                    
                    // Build details text
                    var detailsText = $"Questions Reached: {dgvRounds.SelectedRows[0].Cells["Questions"].Value}\n";
                    
                    // Lifelines
                    var roundLifelines = lifelines.Where(l => l.RoundId == roundId).ToList();
                    if (roundLifelines.Any())
                    {
                        var lifelineNames = roundLifelines.Select(l => 
                        {
                            var name = l.LifelineType switch
                            {
                                1 => "50/50",
                                2 => "Plus One",
                                3 => "ATA",
                                4 => "Switch",
                                5 => "Double Dip",
                                6 => "Ask Host",
                                _ => "Unknown"
                            };
                            return $"{name} (Q{l.QuestionNumber})";
                        });
                        detailsText += $"Lifelines Used: {string.Join(", ", lifelineNames)}\n";
                    }
                    else
                    {
                        detailsText += "Lifelines Used: None\n";
                    }
                    
                    // Participants
                    detailsText += $"Participants: {participants}\n";
                    
                    // FFF Stats
                    if (fffStats.TotalSubmissions > 0)
                    {
                        detailsText += $"FFF Stats: {fffStats.TotalSubmissions} submitted, {fffStats.CorrectSubmissions} correct, Avg time: {(fffStats.AverageTime / 1000):F2}s\n";
                    }
                    
                    // ATA Stats
                    if (ataStats.Any())
                    {
                        var totalVotes = ataStats.Values.Sum();
                        var statsStr = string.Join(", ", ataStats.Select(kvp => 
                        {
                            var percentage = totalVotes > 0 ? (kvp.Value * 100.0 / totalVotes) : 0;
                            return $"{kvp.Key}:{percentage:F0}%";
                        }));
                        detailsText += $"ATA Stats: {statsStr}\n";
                    }
                    
                    // Update UI on main thread
                    Invoke(() =>
                    {
                        lblRoundDetails.Text = detailsText;
                    });
                });
            }
            catch (Exception ex)
            {
                lblRoundDetails.Text = $"Error loading round details: {ex.Message}";
            }
        }
    }

    private async void btnExport_Click(object sender, EventArgs e)
    {
        if (cmbGameSessions.SelectedValue == null) return;

        string sessionId = cmbGameSessions.SelectedValue.ToString()!;
        
        // Get session data to get accurate start time
        var gameData = await _repository.GetGameSessionWithRoundsAsync(sessionId);
        var startTime = gameData.GameStartTime;

        using var saveFileDialog = new SaveFileDialog
        {
            Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
            Title = "Export Telemetry Data",
            FileName = $"Telemetry_{sessionId.Substring(sessionId.Length - 6)}_{startTime:yyyyMMdd_HHmmss}.xlsx",
            DefaultExt = "xlsx",
            RestoreDirectory = true
        };

        // Run on separate thread to avoid modal deadlock
        DialogResult result = DialogResult.Cancel;
        var thread = new Thread(() =>
        {
            result = saveFileDialog.ShowDialog();
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        // Keep UI responsive
        while (thread.IsAlive)
        {
            Application.DoEvents();
            Thread.Sleep(10);
        }

        if (result == DialogResult.OK)
        {
            try
            {
                btnExport.Enabled = false;
                
                // Run export on background thread to avoid blocking UI
                await Task.Run(() =>
                {
                    _exportService.ExportToExcel(saveFileDialog.FileName, gameData);
                });

                MessageBox.Show($"Telemetry exported successfully!\n{saveFileDialog.FileName}",
                    "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}",
                    "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnExport.Enabled = true;
            }
        }
    }

    private async void btnBatchExport_Click(object sender, EventArgs e)
    {
        var sessions = _currentFilterDate.HasValue
            ? await _repository.GetSessionsByDateAsync(_currentFilterDate.Value)
            : await _repository.GetAllGameSessionsAsync();

        if (sessions.Count == 0)
        {
            MessageBox.Show("No sessions to export.", "Batch Export", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // Show confirmation with what will be exported
        var filterInfo = _currentFilterDate.HasValue 
            ? $"filtered by date ({_currentFilterDate.Value:yyyy-MM-dd})" 
            : "all sessions in database";
        var confirmResult = MessageBox.Show(
            $"This will export {sessions.Count} session(s) ({filterInfo}).\n\n" +
            "Use 'Select Date' to filter by specific date before batch export.\n\n" +
            "Continue with batch export?",
            "Confirm Batch Export",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirmResult != DialogResult.Yes)
            return;

        // Run folder dialog on STA thread to avoid blocking
        DialogResult folderResult = DialogResult.Cancel;
        string selectedPath = string.Empty;
        
        var thread = new Thread(() =>
        {
            using var folderDialog = new FolderBrowserDialog
            {
                Description = "Select folder for batch export"
            };
            folderResult = folderDialog.ShowDialog();
            selectedPath = folderDialog.SelectedPath;
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        // Keep UI responsive
        while (thread.IsAlive)
        {
            Application.DoEvents();
            Thread.Sleep(10);
        }

        if (folderResult == DialogResult.OK && !string.IsNullOrEmpty(selectedPath))
        {
            Form? progressForm = null;
            try
            {
                btnBatchExport.Enabled = false;

                // Show progress form (non-modal)
                progressForm = new Form
                {
                    Text = "Exporting...",
                    StartPosition = FormStartPosition.CenterParent,
                    Size = new Size(400, 100),
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    ControlBox = false
                };
                var progressLabel = new Label
                {
                    Text = "Preparing export...",
                    AutoSize = true,
                    Location = new Point(20, 30)
                };
                progressForm.Controls.Add(progressLabel);
                progressForm.Show(this);

                // Run export async
                await Task.Run(async () =>
                {
                    try
                    {
                        var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                        Directory.CreateDirectory(tempFolder);

                        int count = 0;
                        foreach (var session in sessions)
                        {
                            count++;
                            var currentCount = count; // Capture for closure
                            BeginInvoke(() => progressLabel.Text = $"Exporting session {currentCount} of {sessions.Count}...");

                            var fileName = $"Telemetry_{session.SessionIdShort}_{session.GameStartTime:yyyyMMdd_HHmmss}.xlsx";
                            var filePath = Path.Combine(tempFolder, fileName);

                            var gameData = await _repository.GetGameSessionWithRoundsAsync(session.SessionId);
                            _exportService.ExportToExcel(filePath, gameData);
                        }

                        BeginInvoke(() => progressLabel.Text = "Creating ZIP archive...");

                        // Create ZIP - use date range from sessions
                        var firstSessionDate = sessions.Min(s => s.GameStartTime);
                        var lastSessionDate = sessions.Max(s => s.GameStartTime);
                        var dateRange = firstSessionDate.Date == lastSessionDate.Date 
                            ? $"{firstSessionDate:yyyyMMdd}"
                            : $"{firstSessionDate:yyyyMMdd}_to_{lastSessionDate:yyyyMMdd}";
                        
                        var zipPath = Path.Combine(selectedPath,
                            $"TelemetryBatch_{dateRange}_{sessions.Count}Sessions.zip");
                        
                        // Check if file exists and prompt to overwrite
                        if (File.Exists(zipPath))
                        {
                            DialogResult overwriteResult = DialogResult.No;
                            Invoke(() =>
                            {
                                overwriteResult = MessageBox.Show(
                                    $"The file already exists:\n{Path.GetFileName(zipPath)}\n\nDo you want to replace it?",
                                    "File Exists",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Warning);
                            });

                            if (overwriteResult != DialogResult.Yes)
                            {
                                BeginInvoke(() =>
                                {
                                    MessageBox.Show("Batch export cancelled.",
                                        "Export Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                });
                                return;
                            }

                            // Delete existing file
                            File.Delete(zipPath);
                        }
                        
                        ZipFile.CreateFromDirectory(tempFolder, zipPath);

                        // Cleanup
                        Directory.Delete(tempFolder, true);

                        // Show success on UI thread
                        BeginInvoke(() =>
                        {
                            MessageBox.Show($"Batch export complete!\n{sessions.Count} sessions exported to:\n{zipPath}",
                                "Batch Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        });
                    }
                    catch (Exception ex)
                    {
                        BeginInvoke(() =>
                        {
                            MessageBox.Show($"Batch export failed: {ex.Message}",
                                "Batch Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Batch export failed: {ex.Message}",
                    "Batch Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                progressForm?.Close();
                progressForm?.Dispose();
                btnBatchExport.Enabled = true;
            }
        }
    }

    private void btnClose_Click(object sender, EventArgs e)
    {
        Close();
    }

    #endregion
}
