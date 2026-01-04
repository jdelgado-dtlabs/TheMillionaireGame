using System.Diagnostics;
using System.IO.Pipes;
using System.Text.Json;

namespace MillionaireGame.Watchdog;

/// <summary>
/// Listens for heartbeat messages from the main application via named pipe
/// </summary>
public class HeartbeatListener : IDisposable
{
    private const string PipeName = "MillionaireGame.Heartbeat";
    private const int TimeoutSeconds = 15;
    
    private Thread? _listenerThread;
    private CancellationTokenSource? _cancellationTokenSource;
    private DateTime _lastHeartbeat = DateTime.Now;
    private HeartbeatMessage? _lastMessage;
    private readonly object _lock = new object();
    private bool _isDisposed;

    public event EventHandler<HeartbeatMessage>? HeartbeatReceived;
    public event EventHandler? HeartbeatTimeout;

    public void Start()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _lastHeartbeat = DateTime.Now;
        
        // Start listener thread
        _listenerThread = new Thread(() => ListenForHeartbeats(_cancellationTokenSource.Token))
        {
            IsBackground = true,
            Name = "Heartbeat Listener"
        };
        _listenerThread.Start();
        
        Console.WriteLine($"[Watchdog] Heartbeat listener started (timeout: {TimeoutSeconds}s)");
    }

    public void Stop()
    {
        _cancellationTokenSource?.Cancel();
        _listenerThread?.Join(TimeSpan.FromSeconds(2));
        Console.WriteLine("[Watchdog] Heartbeat listener stopped");
    }

    private void ListenForHeartbeats(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var pipeServer = new NamedPipeServerStream(
                    PipeName,
                    PipeDirection.In,
                    1,
                    PipeTransmissionMode.Message,
                    PipeOptions.Asynchronous);

                // Wait for connection with timeout
                var connectTask = pipeServer.WaitForConnectionAsync(cancellationToken);
                if (!connectTask.Wait(TimeSpan.FromSeconds(1), cancellationToken))
                {
                    // Check for timeout
                    CheckHeartbeatTimeout();
                    continue;
                }

                // Read message
                using var reader = new StreamReader(pipeServer);
                var json = reader.ReadToEnd();
                
                if (!string.IsNullOrEmpty(json))
                {
                    var message = JsonSerializer.Deserialize<HeartbeatMessage>(json);
                    if (message != null)
                    {
                        lock (_lock)
                        {
                            _lastHeartbeat = DateTime.Now;
                            _lastMessage = message;
                        }
                        
                        HeartbeatReceived?.Invoke(this, message);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Watchdog] Heartbeat listener error: {ex.Message}");
                Thread.Sleep(1000);
            }
        }
    }

    private void CheckHeartbeatTimeout()
    {
        lock (_lock)
        {
            var timeSinceLastHeartbeat = DateTime.Now - _lastHeartbeat;
            
            // Don't timeout if app is shutting down
            if (_lastMessage?.State == "ShuttingDown")
                return;
            
            if (timeSinceLastHeartbeat.TotalSeconds > TimeoutSeconds)
            {
                Console.WriteLine($"[Watchdog] HEARTBEAT TIMEOUT - No heartbeat for {timeSinceLastHeartbeat.TotalSeconds:F1}s");
                HeartbeatTimeout?.Invoke(this, EventArgs.Empty);
                _lastHeartbeat = DateTime.Now; // Reset to avoid repeated events
            }
        }
    }

    public HeartbeatMessage? GetLastHeartbeat()
    {
        lock (_lock)
        {
            return _lastMessage;
        }
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        Stop();
        _cancellationTokenSource?.Dispose();
        _isDisposed = true;
    }
}
