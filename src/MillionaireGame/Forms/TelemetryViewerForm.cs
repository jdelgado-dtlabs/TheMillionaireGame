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
            Round = r.RoundNumber,
            Start = r.StartTime.ToString("HH:mm:ss"),
            End = r.EndTime == default ? "" : r.EndTime.ToString("HH:mm:ss"),
            Outcome = r.Outcome?.ToString() ?? "",
            Currency1 = r.Currency1Winnings > 0 ? $"{gameData.Currency1Name}{r.Currency1Winnings:N0}" : "",
            Currency2 = r.Currency2Winnings > 0 && !string.IsNullOrEmpty(gameData.Currency2Name) 
                ? $"{gameData.Currency2Name}{r.Currency2Winnings:N0}" : "",
            Questions = r.FinalQuestionReached
        }).ToList();
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
            var roundNumber = (int)dgvRounds.SelectedRows[0].Cells["Round"].Value;
            
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
                    var roundLifelines = lifelines.Where(l => l.RoundId == roundNumber).ToList();
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
                        detailsText += $"FFF Stats: {fffStats.TotalSubmissions} submitted, {fffStats.CorrectSubmissions} correct, Avg time: {fffStats.AverageTime:F2}s\n";
                    }
                    
                    // ATA Stats
                    if (ataStats.Any())
                    {
                        var statsStr = string.Join(", ", ataStats.Select(kvp => $"{kvp.Key}:{kvp.Value}%"));
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

    private void btnExport_Click(object sender, EventArgs e)
    {
        if (cmbGameSessions.SelectedValue == null) return;

        string sessionId = cmbGameSessions.SelectedValue.ToString()!;

        using var saveFileDialog = new SaveFileDialog
        {
            Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
            Title = "Export Telemetry Data",
            FileName = $"Telemetry_{sessionId.Substring(sessionId.Length - 6)}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
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
                
                var gameData = _repository.GetGameSessionWithRoundsAsync(sessionId).Result;
                _exportService.ExportToExcel(saveFileDialog.FileName, gameData);

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

    private void btnBatchExport_Click(object sender, EventArgs e)
    {
        var sessions = _currentFilterDate.HasValue
            ? _repository.GetSessionsByDateAsync(_currentFilterDate.Value).Result
            : _repository.GetAllGameSessionsAsync().Result;

        if (sessions.Count == 0)
        {
            MessageBox.Show("No sessions to export.", "Batch Export", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var folderDialog = new FolderBrowserDialog
        {
            Description = "Select folder for batch export"
        };

        if (folderDialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                btnBatchExport.Enabled = false;

                // Show progress
                var progressForm = new Form
                {
                    Text = "Exporting...",
                    StartPosition = FormStartPosition.CenterParent,
                    Size = new Size(400, 100),
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    ControlBox = false
                };
                var progressLabel = new Label
                {
                    Text = "Exporting sessions...",
                    AutoSize = true,
                    Location = new Point(20, 30)
                };
                progressForm.Controls.Add(progressLabel);

                // Run export async
                var exportTask = Task.Run(() =>
                {
                    var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                    Directory.CreateDirectory(tempFolder);

                    int count = 0;
                    foreach (var session in sessions)
                    {
                        count++;
                        Invoke(() => progressLabel.Text = $"Exporting session {count} of {sessions.Count}...");

                        var fileName = $"Telemetry_{session.SessionIdShort}_{session.GameStartTime:yyyyMMdd}.xlsx";
                        var filePath = Path.Combine(tempFolder, fileName);

                        var gameData = _repository.GetGameSessionWithRoundsAsync(session.SessionId).Result;
                        _exportService.ExportToExcel(filePath, gameData);
                    }

                    // Create ZIP
                    var zipPath = Path.Combine(folderDialog.SelectedPath,
                        $"TelemetryBatch_{DateTime.Now:yyyyMMdd_HHmmss}_{sessions.Count}Sessions.zip");
                    ZipFile.CreateFromDirectory(tempFolder, zipPath);

                    // Cleanup
                    Directory.Delete(tempFolder, true);

                    // Close progress and show success
                    Invoke(() =>
                    {
                        progressForm.Close();
                        MessageBox.Show($"Batch export complete!\n{sessions.Count} sessions exported to:\n{zipPath}",
                            "Batch Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    });
                });

                progressForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Batch export failed: {ex.Message}",
                    "Batch Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
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
