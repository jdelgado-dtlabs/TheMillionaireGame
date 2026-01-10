using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text.Json;
using System.Threading;
using System.Windows.Forms;
using MillionaireGame.Utilities;

namespace MillionaireGame.Services
{
    /// <summary>
    /// Heartbeat message sent to watchdog
    /// </summary>
    internal class HeartbeatMessage
    {
        public DateTime Timestamp { get; set; }
        public string State { get; set; } = "Running";
        public int ThreadCount { get; set; }
        public long MemoryUsageMB { get; set; }
        public string? CurrentActivity { get; set; }
    }

    /// <summary>
    /// Sends periodic heartbeat messages to the watchdog process
    /// </summary>
    internal class HeartbeatService : IDisposable
    {
        private const string PipeName = "MillionaireGame.Heartbeat";
        private const int HeartbeatIntervalMs = 5000; // 5 seconds
        private const int UIResponseTimeoutMs = 3000; // 3 seconds for UI to respond
        
        private System.Threading.Timer? _heartbeatTimer;
        private string _currentActivity = "Initializing";
        private string _currentState = "Running";
        private readonly object _lock = new object();
        private bool _isDisposed;
        private bool _uiThreadResponsive = true;
        private Form? _mainForm;

        public void Start()
        {
            _heartbeatTimer = new System.Threading.Timer(
                SendHeartbeat,
                null,
                TimeSpan.FromSeconds(2), // Initial delay
                TimeSpan.FromMilliseconds(HeartbeatIntervalMs));
        }

        public void Start(Form mainForm)
        {
            _mainForm = mainForm;
            Start();
        }

        public void Stop()
        {
            lock (_lock)
            {
                _currentState = "ShuttingDown";
            }
            
            // Send final heartbeat
            SendHeartbeat(null);
            
            _heartbeatTimer?.Dispose();
            _heartbeatTimer = null;
        }

        public void SetActivity(string activity)
        {
            lock (_lock)
            {
                _currentActivity = activity;
            }
        }

        private void SendHeartbeat(object? state)
        {
            try
            {
                // Check if UI thread is responsive before sending heartbeat
                if (_mainForm != null && !CheckUIThreadResponsive())
                {
                    // UI thread is frozen - don't send heartbeat so watchdog can detect it
                    GameConsole.Error("[Heartbeat] UI thread unresponsive - stopping heartbeats to trigger watchdog");
                    return;
                }

                var process = Process.GetCurrentProcess();
                
                HeartbeatMessage message;
                lock (_lock)
                {
                    message = new HeartbeatMessage
                    {
                        Timestamp = DateTime.Now,
                        State = _currentState,
                        ThreadCount = process.Threads.Count,
                        MemoryUsageMB = process.WorkingSet64 / (1024 * 1024),
                        CurrentActivity = _currentActivity
                    };
                }

                // Send via named pipe
                using var pipeClient = new NamedPipeClientStream(
                    ".",
                    PipeName,
                    PipeDirection.Out,
                    PipeOptions.None,
                    System.Security.Principal.TokenImpersonationLevel.None);

                // Try to connect with timeout
                var connectTask = pipeClient.ConnectAsync(CancellationToken.None);
                if (!connectTask.Wait(TimeSpan.FromSeconds(1)))
                {
                    // Watchdog not running - that's okay
                    return;
                }

                using var writer = new StreamWriter(pipeClient);
                var json = JsonSerializer.Serialize(message);
                writer.Write(json);
                writer.Flush();
            }
            catch
            {
                // Silently ignore heartbeat errors - watchdog may not be running
            }
        }

        /// <summary>
        /// Checks if the UI thread is responsive by posting a message and waiting for response
        /// </summary>
        /// <returns>True if UI thread responds within timeout, false if frozen</returns>
        private bool CheckUIThreadResponsive()
        {
            if (_mainForm == null || _mainForm.IsDisposed)
                return true; // No form to check

            try
            {
                var responseEvent = new ManualResetEventSlim(false);

                // Post a message to the UI thread
                _mainForm.BeginInvoke(new Action(() =>
                {
                    responseEvent.Set();
                }));

                // Wait for response with timeout
                var responded = responseEvent.Wait(UIResponseTimeoutMs);
                
                lock (_lock)
                {
                    _uiThreadResponsive = responded;
                }

                return responded;
            }
            catch (Exception ex)
            {
                GameConsole.Error($"[Heartbeat] Error checking UI thread: {ex.Message}");
                return true; // Assume responsive on error to avoid false positives
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            Stop();
            _isDisposed = true;
        }
    }
}
