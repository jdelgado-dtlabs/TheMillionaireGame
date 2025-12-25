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
        this.Size = new Size(600, 500);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        // Silence Detection Group
        var grpSilence = new GroupBox
        {
            Text = "Silence Detection Test",
            Location = new Point(10, 10),
            Size = new Size(560, 120)
        };

        chkEnableSilenceDetection = new CheckBox
        {
            Text = "Enable Silence Detection (shows in console)",
            Location = new Point(10, 25),
            Size = new Size(300, 25),
            Checked = true
        };

        btnTestSilenceDetection = new Button
        {
            Text = "Play Sound with Silence Detection",
            Location = new Point(10, 55),
            Size = new Size(250, 30)
        };
        btnTestSilenceDetection.Click += BtnTestSilenceDetection_Click;

        var lblSilenceInfo = new Label
        {
            Text = "Plays a sound with silence detection enabled.\nWatch the console for 'Silence detected' messages.",
            Location = new Point(270, 55),
            Size = new Size(280, 50),
            ForeColor = Color.Gray
        };

        grpSilence.Controls.AddRange(new Control[] { chkEnableSilenceDetection, btnTestSilenceDetection, lblSilenceInfo });

        // Queue Test Group
        var grpQueue = new GroupBox
        {
            Text = "Audio Queue with Crossfading Test",
            Location = new Point(10, 140),
            Size = new Size(560, 200)
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
            Location = new Point(10, 55),
            Size = new Size(200, 30)
        };
        btnTestQueue.Click += BtnTestQueue_Click;

        btnTestPriority = new Button
        {
            Text = "Test Priority Interrupt",
            Location = new Point(220, 55),
            Size = new Size(180, 30)
        };
        btnTestPriority.Click += BtnTestPriority_Click;

        btnClearQueue = new Button
        {
            Text = "Clear Queue",
            Location = new Point(10, 95),
            Size = new Size(120, 30)
        };
        btnClearQueue.Click += BtnClearQueue_Click;

        btnStopQueue = new Button
        {
            Text = "Stop Queue",
            Location = new Point(140, 95),
            Size = new Size(120, 30)
        };
        btnStopQueue.Click += BtnStopQueue_Click;

        lblQueueCount = new Label
        {
            Text = "Queue Count: 0",
            Location = new Point(270, 100),
            Size = new Size(150, 25),
            Font = new Font(Font.FontFamily, 10, FontStyle.Bold)
        };

        txtQueueStatus = new TextBox
        {
            Location = new Point(10, 135),
            Size = new Size(540, 50),
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Text = "Watch console for crossfade messages.\nListen for smooth transitions between sounds."
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
            Location = new Point(10, 350),
            Size = new Size(560, 80),
            ForeColor = Color.DarkBlue,
            BackColor = Color.AliceBlue,
            Padding = new Padding(5)
        };

        // Close button
        var btnClose = new Button
        {
            Text = "Close",
            Location = new Point(250, 440),
            Size = new Size(100, 30),
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
        bool isPlaying = _soundService.IsQueuePlaying();
        bool isCrossfading = _soundService.IsQueueCrossfading();

        lblQueueCount.Text = $"Queue Count: {queueCount}";
        lblQueueCount.ForeColor = queueCount > 0 ? Color.Green : Color.Gray;

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
        GameConsole.Info("[DSP Test] Testing silence detection...");
        
        // Play a sound with silence detection
        // The EffectsChannel should wrap it with SilenceDetectorSource
        _soundService.PlaySound(SoundEffect.ExplainGame);
        
        txtQueueStatus.Text = "Playing sound with silence detection.\r\nWatch console for 'Silence detected' message.";
        GameConsole.Info("[DSP Test] Sound playing. Check console for silence detection events.");
    }

    private void BtnTestQueue_Click(object? sender, EventArgs e)
    {
        GameConsole.Info("[DSP Test] Queuing 5 sounds for crossfade test...");
        
        // Queue 5 different sounds
        _soundService.QueueSound(SoundEffect.LifelinePing1, AudioPriority.Normal);
        _soundService.QueueSound(SoundEffect.LifelinePing2, AudioPriority.Normal);
        _soundService.QueueSound(SoundEffect.LifelinePing3, AudioPriority.Normal);
        _soundService.QueueSound(SoundEffect.LifelinePing4, AudioPriority.Normal);
        _soundService.QueueSound(SoundEffect.FinalAnswer, AudioPriority.Normal);
        
        txtQueueStatus.Text = "Queued 5 sounds:\r\n" +
                             "1. Lifeline Ping 1\r\n" +
                             "2. Lifeline Ping 2\r\n" +
                             "3. Lifeline Ping 3\r\n" +
                             "4. Lifeline Ping 4\r\n" +
                             "5. Final Answer\r\n\r\n" +
                             "Listen for smooth crossfades between sounds.";
        
        GameConsole.Info("[DSP Test] 5 sounds queued. Watch console for crossfade progress.");
    }

    private void BtnTestPriority_Click(object? sender, EventArgs e)
    {
        GameConsole.Info("[DSP Test] Testing priority interrupt...");
        
        // Queue normal sounds first
        _soundService.QueueSound(SoundEffect.LifelinePing1, AudioPriority.Normal);
        _soundService.QueueSound(SoundEffect.LifelinePing2, AudioPriority.Normal);
        _soundService.QueueSound(SoundEffect.LifelinePing3, AudioPriority.Normal);
        
        // Wait a moment, then send immediate priority
        Task.Delay(1000).ContinueWith(_ =>
        {
            this.Invoke(() =>
            {
                _soundService.QueueSound(SoundEffect.FinalAnswer, AudioPriority.Immediate);
                txtQueueStatus.Text += "\r\n\r\n⚡ Immediate priority sound sent!";
                GameConsole.Info("[DSP Test] Immediate priority sound queued.");
            });
        });
        
        txtQueueStatus.Text = "Queued 3 normal sounds.\r\n" +
                             "After 1 second, an IMMEDIATE priority sound will interrupt.\r\n" +
                             "The immediate sound should play next, skipping the queue.";
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
