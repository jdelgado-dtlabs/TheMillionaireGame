using MillionaireGame.Services;
using MillionaireGame.Utilities;
using MillionaireGame.Core.Game;

namespace MillionaireGame.Forms;

/// <summary>
/// Test dialog for DSP features: silence detection and audio queue with crossfading
/// </summary>
public partial class DSPTestDialog : Form
{
    private readonly SoundService _soundService;
    private Button btnTestSilenceDetection;
    private Button btnTestQueue;
    private Button btnTestPriority;
    private Button btnClearQueue;
    private Button btnStopQueue;
    private TextBox txtQueueStatus;
    private CheckBox chkEnableSilenceDetection;
    private CheckBox chkEnableCrossfade;
    private Label lblQueueCount;
    private System.Windows.Forms.Timer _statusTimer;

    public DSPTestDialog(SoundService soundService)
    {
        _soundService = soundService;
        InitializeComponent();
        InitializeStatusTimer();
    }

    private void InitializeComponent()
    {
        this.Text = "DSP Test Dialog";
        this.ClientSize = new Size(650, 590);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.AutoScaleMode = AutoScaleMode.None;
        this.AutoSize = false;

        // Silence Detection Group
        var grpSilence = new GroupBox
        {
            Text = "Silence Detection Test",
            Location = new Point(10, 10),
            Size = new Size(620, 130)
        };

        chkEnableSilenceDetection = new CheckBox
        {
            Text = "Enable Silence Detection (shows in console)",
            Location = new Point(10, 25),
            Size = new Size(320, 25),
            Checked = true
        };

        btnTestSilenceDetection = new Button
        {
            Text = "Play Sound with Silence Detection",
            Location = new Point(10, 55),
            Size = new Size(280, 35)
        };
        btnTestSilenceDetection.Click += BtnTestSilenceDetection_Click;

        var lblSilenceInfo = new Label
        {
            Text = "Plays a sound with silence detection enabled.\nWatch the console for 'Silence detected' messages.",
            Location = new Point(300, 55),
            Size = new Size(310, 50),
            ForeColor = Color.Gray
        };

        grpSilence.Controls.AddRange(new Control[] { chkEnableSilenceDetection, btnTestSilenceDetection, lblSilenceInfo });

        // Queue Test Group
        var grpQueue = new GroupBox
        {
            Text = "Audio Queue with Crossfading Test",
            Location = new Point(10, 150),
            Size = new Size(620, 260)
        };

        chkEnableCrossfade = new CheckBox
        {
            Text = "Enable Automatic Crossfading",
            Location = new Point(10, 25),
            Size = new Size(300, 25),
            Checked = true
        };

        btnTestQueue = new Button
        {
            Text = "Queue 5 Sounds (Sequential)",
            Location = new Point(10, 60),
            Size = new Size(220, 35)
        };
        btnTestQueue.Click += BtnTestQueue_Click;

        btnTestPriority = new Button
        {
            Text = "Test Priority Interrupt",
            Location = new Point(240, 60),
            Size = new Size(180, 35)
        };
        btnTestPriority.Click += BtnTestPriority_Click;

        btnClearQueue = new Button
        {
            Text = "Clear Queue",
            Location = new Point(10, 105),
            Size = new Size(120, 35)
        };
        btnClearQueue.Click += BtnClearQueue_Click;

        btnStopQueue = new Button
        {
            Text = "Stop Queue",
            Location = new Point(140, 105),
            Size = new Size(120, 35)
        };
        btnStopQueue.Click += BtnStopQueue_Click;

        lblQueueCount = new Label
        {
            Text = "Queue Count: 0",
            Location = new Point(270, 110),
            Size = new Size(180, 25),
            Font = new Font(Font.FontFamily, 10, FontStyle.Bold)
        };

        txtQueueStatus = new TextBox
        {
            Location = new Point(10, 150),
            Size = new Size(600, 95),
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Text = "Status: Idle\r\nWatch console for crossfade messages.\r\nListen for smooth transitions between sounds."
        };

        grpQueue.Controls.AddRange(new Control[] { 
            chkEnableCrossfade, btnTestQueue, btnTestPriority, 
            btnClearQueue, btnStopQueue, lblQueueCount, txtQueueStatus 
        });

        // Info Panel
        var lblInfo = new Label
        {
            Text = "💡 Tips:\n" +
                   "• Open the Game Console (if available) to see debug output\n" +
                   "• Silence detection logs when audio ends early\n" +
                   "• Crossfading creates smooth transitions (200ms default)\n" +
                   "• Priority sounds interrupt normal queue playback",
            Location = new Point(10, 420),
            Size = new Size(620, 95),
            ForeColor = Color.DarkBlue,
            BackColor = Color.AliceBlue,
            Padding = new Padding(8),
            BorderStyle = BorderStyle.FixedSingle
        };

        // Close button  
        var btnClose = new Button
        {
            Text = "Close",
            Location = new Point(275, 530),
            Size = new Size(100, 35),
            DialogResult = DialogResult.OK
        };

        this.Controls.AddRange(new Control[] { grpSilence, grpQueue, lblInfo, btnClose });
        this.AcceptButton = btnClose;
    }

    private void InitializeStatusTimer()
    {
        _statusTimer = new System.Windows.Forms.Timer();
        _statusTimer.Interval = 100; // Update every 100ms
        _statusTimer.Tick += StatusTimer_Tick;
        _statusTimer.Start();
    }

    private void StatusTimer_Tick(object? sender, EventArgs e)
    {
        int queueCount = _soundService.GetQueueCount();
        int totalCount = _soundService.GetTotalSoundCount();
        bool isPlaying = _soundService.IsQueuePlaying();
        bool isCrossfading = _soundService.IsQueueCrossfading();

        lblQueueCount.Text = $"Total Sounds: {totalCount} (Waiting: {queueCount})";
        lblQueueCount.ForeColor = totalCount > 0 ? Color.Green : Color.Gray;

        string status = isPlaying ? "Playing" : "Idle";
        if (isCrossfading)
            status += " (Crossfading)";

        if (txtQueueStatus.Lines.Length > 0 && !txtQueueStatus.Lines[0].StartsWith("Status:"))
        {
            txtQueueStatus.Text = $"Status: {status}\r\n" + txtQueueStatus.Text;
        }
    }

    private void BtnTestSilenceDetection_Click(object? sender, EventArgs e)
    {
        // Disable button to prevent multiple clicks during playback
        btnTestSilenceDetection.Enabled = false;
        
        GameConsole.Info("[DSP Test] Testing silence detection with queue...");
        
        // Use QUEUE system (not direct PlaySoundByKey) - silence detection works on queue items too!
        _soundService.QueueSoundByKey("Q06LightsDown", AudioPriority.Normal);
        
        txtQueueStatus.Text = "Queued 'Lights Down' sound with silence detection.\r\nWatch console for silence detection and fadeout.";
        GameConsole.Info("[DSP Test] Sound queued. Check console for silence detection events.");
        
        // Poll until queue is empty and playback stopped
        Task.Run(async () =>
        {
            // Wait at least 1 second before checking (give sound time to start)
            await Task.Delay(1000);
            
            // Poll every 500ms until queue is done
            while (!IsDisposed)
            {
                if (_soundService.GetQueueCount() == 0 && !_soundService.IsQueuePlaying())
                {
                    // Queue finished, re-enable button
                    if (!IsDisposed)
                    {
                        try
                        {
                            BeginInvoke(new Action(() =>
                            {
                                if (!IsDisposed)
                                {
                                    btnTestSilenceDetection.Enabled = true;
                                }
                            }));
                        }
                        catch { /* Window disposed */ }
                    }
                    break;
                }
                await Task.Delay(500);
            }
        });
    }

    private void BtnTestQueue_Click(object? sender, EventArgs e)
    {
        // Disable button during queue playback
        btnTestQueue.Enabled = false;
        
        GameConsole.Info("[DSP Test] Queuing 5 sounds for crossfade test...");
        
        // Queue 5 FFF sounds (longer duration for better crossfade testing)
        _soundService.QueueSoundByKey("FFFLightsDown", AudioPriority.Normal);
        _soundService.QueueSoundByKey("FFFExplain", AudioPriority.Normal);
        _soundService.QueueSoundByKey("FFFReadQuestion", AudioPriority.Normal);
        _soundService.QueueSoundByKey("FFFThreeNotes", AudioPriority.Normal);
        _soundService.QueueSoundByKey("FFFThinking", AudioPriority.Normal);
        
        txtQueueStatus.Text = "Queued 5 sounds:\r\n" +
                             "1. FFF Lights Down\r\n" +
                             "2. FFF Explain\r\n" +
                             "3. FFF Read Question\r\n" +
                             "4. FFF Three Notes\r\n" +
                             "5. FFF Thinking\r\n\r\n" +
                             "Listen for smooth crossfades between sounds.";
        
        GameConsole.Info("[DSP Test] 5 sounds queued. Watch console for crossfade progress.");
        
        // Poll until queue is empty and no effects are playing
        Task.Run(async () =>
        {
            // Wait at least 1 second before checking
            await Task.Delay(1000);
            
            // Poll every 500ms until queue and effects are done
            while (!IsDisposed)
            {
                if (_soundService.GetQueueCount() == 0 && !_soundService.IsQueuePlaying())
                {
                    // Queue finished, re-enable button
                    if (!IsDisposed)
                    {
                        try
                        {
                            BeginInvoke(new Action(() =>
                            {
                                if (!IsDisposed)
                                {
                                    btnTestQueue.Enabled = true;
                                }
                            }));
                        }
                        catch { /* Window disposed */ }
                    }
                    break;
                }
                await Task.Delay(500);
            }
        });
    }

    private void BtnTestPriority_Click(object? sender, EventArgs e)
    {
        // Disable button during test
        btnTestPriority.Enabled = false;
        
        GameConsole.Info("[DSP Test] Testing priority interrupt...");
        
        // Queue normal sounds first using actual soundpack keys
        _soundService.QueueSoundByKey("Lifeline1Ping", AudioPriority.Normal);
        _soundService.QueueSoundByKey("Lifeline2Ping", AudioPriority.Normal);
        _soundService.QueueSoundByKey("Lifeline3Ping", AudioPriority.Normal);
        
        // Wait a moment, then send immediate priority
        Task.Delay(1000).ContinueWith(_ =>
        {
            this.Invoke(() =>
            {
                _soundService.QueueSoundByKey("FFFThreeNotes", AudioPriority.Immediate);
                txtQueueStatus.Text += "\r\n\r\n⚡ Immediate priority sound sent (FFF Three Notes)!";
                GameConsole.Info("[DSP Test] Immediate priority sound queued.");
            });
        });
        
        txtQueueStatus.Text = "Queued 3 normal sounds (Lifeline pings).\r\n" +
                             "After 1 second, an IMMEDIATE priority sound will interrupt.\r\n" +
                             "The immediate sound should play next, skipping the queue.";
        
        // Poll until queue is empty and no effects are playing
        Task.Run(async () =>
        {
            // Wait at least 2 seconds before checking (give time for priority interrupt)
            await Task.Delay(2000);
            
            // Poll every 500ms until queue and effects are done
            while (!IsDisposed)
            {
                if (_soundService.GetQueueCount() == 0 && !_soundService.IsQueuePlaying())
                {
                    // Test finished, re-enable button
                    if (!IsDisposed)
                    {
                        try
                        {
                            BeginInvoke(new Action(() =>
                            {
                                if (!IsDisposed)
                                {
                                    btnTestPriority.Enabled = true;
                                }
                            }));
                        }
                        catch { /* Window disposed */ }
                    }
                    break;
                }
                await Task.Delay(500);
            }
        });
    }

    private void BtnClearQueue_Click(object? sender, EventArgs e)
    {
        int count = _soundService.GetQueueCount();
        _soundService.ClearQueue();
        
        txtQueueStatus.Text = $"Queue cleared! ({count} sounds removed)";
        GameConsole.Info($"[DSP Test] Queue cleared. {count} sounds removed.");
    }

    private void BtnStopQueue_Click(object? sender, EventArgs e)
    {
        _soundService.StopQueue();
        
        txtQueueStatus.Text = "Queue stopped and cleared.";
        GameConsole.Info("[DSP Test] Queue stopped.");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _statusTimer?.Stop();
            _statusTimer?.Dispose();
        }
        base.Dispose(disposing);
    }
}
