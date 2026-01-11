# UDP Port 8001 Listener Implementation

**Component**: ETC Ion Plugin - Cue List Population  
**Purpose**: Listen for OSC responses from ETC Ion console to populate Event Trigger dropdown  
**Date**: January 10, 2026

---

## Overview

The ETC Ion plugin needs to listen for UDP packets on **port 8001** to receive OSC responses from the Ion console. These responses contain cue information that will populate the Event Trigger dropdown in the settings UI.

---

## ETC OSC Response Format

When querying cues, the Ion console sends responses on UDP port 8001:

### Cue Count Response
```
Address: /eos/out/get/cue/{list}/count
Arguments: [{count}]
```

### Individual Cue Response
```
Address: /eos/out/get/cue/{list}/{index}
Arguments: [
    {0}: uint32  - Index number (position in cue list)
    {1}: string  - Unique ID (UID)
    {2}: string  - **Cue Label** (display name for UI)
    {3}: string|int - **Cue Number** (identifier to trigger the cue)
    {4}: uint32  - Cue Part number
]
```

---

## Implementation: EtcOscClient.cs

### UDP Listener Setup

```csharp
private UdpClient? _feedbackClient;     // Listen for feedback from Ion (port 8001)
private CancellationTokenSource? _listenerCancellation;

public event EventHandler<OscMessageReceivedEventArgs>? MessageReceived;

public async Task<bool> ConnectAsync()
{
    try
    {
        // Create send client (to port 8000)
        _udpClient = new UdpClient();
        _udpClient.Connect(_hostAddress, _txPort);
        
        // Create receive client (from port 8001)
        _feedbackClient = new UdpClient(_rxPort);  // Bind to port 8001
        
        // Start listening in background
        _listenerCancellation = new CancellationTokenSource();
        _ = ListenForMessagesAsync(_listenerCancellation.Token);
        
        return true;
    }
    catch (Exception ex)
    {
        GameConsole.Error($"Failed to connect: {ex.Message}");
        return false;
    }
}

private async Task ListenForMessagesAsync(CancellationToken cancellationToken)
{
    while (!cancellationToken.IsCancellationRequested)
    {
        try
        {
            var result = await _feedbackClient.ReceiveAsync(cancellationToken);
            ProcessOscMessage(result.Buffer);
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown
            break;
        }
        catch (Exception ex)
        {
            GameConsole.Error($"Error receiving OSC message: {ex.Message}");
        }
    }
}

private void ProcessOscMessage(byte[] data)
{
    try
    {
        var message = OscMessage.FromBytes(data);
        
        // Fire event for listeners
        MessageReceived?.Invoke(this, new OscMessageReceivedEventArgs
        {
            Address = message.Address,
            Arguments = message.Arguments,
            ReceivedAt = DateTime.Now
        });
        
        GameConsole.Debug($"OSC RX: {message.Address} [{string.Join(", ", message.Arguments)}]");
    }
    catch (Exception ex)
    {
        GameConsole.Error($"Failed to parse OSC message: {ex.Message}");
    }
}

public void Disconnect()
{
    _listenerCancellation?.Cancel();
    _feedbackClient?.Close();
    _udpClient?.Close();
}
```

---

## Implementation: EtcIonPlugin.GetCueListAsync()

### Querying Cue List

```csharp
public async Task<List<LightingCueInfo>> GetCueListAsync(string cueListId)
{
    var cueList = new List<LightingCueInfo>();
    
    try
    {
        // Step 1: Query cue count
        var countQuery = new OscMessage 
        { 
            Address = $"/eos/get/cue/{cueListId}/count",
            Arguments = new List<object>()
        };
        
        await _oscClient.SendCommandAsync(countQuery);
        
        // Step 2: Wait for count response on port 8001
        var countResponse = await WaitForResponseAsync(
            $"/eos/out/get/cue/{cueListId}/count", 
            timeoutMs: 2000
        );
        
        int count = Convert.ToInt32(countResponse.Arguments[0]);
        GameConsole.Info($"Cue list {cueListId} contains {count} cues");
        
        // Step 3: Query each cue by index
        for (int i = 0; i < count; i++)
        {
            var cueQuery = new OscMessage
            {
                Address = $"/eos/get/cue/{cueListId}/{i}/*",
                Arguments = new List<object>()
            };
            
            await _oscClient.SendCommandAsync(cueQuery);
            
            // Wait for response on port 8001
            var cueResponse = await WaitForResponseAsync(
                $"/eos/out/get/cue/{cueListId}/{i}",
                timeoutMs: 1000
            );
            
            // Parse OSC arguments
            var cueInfo = new LightingCueInfo
            {
                Index = Convert.ToInt32(cueResponse.Arguments[0]),
                UniqueId = cueResponse.Arguments[1]?.ToString() ?? "",
                Label = cueResponse.Arguments[2]?.ToString() ?? "",       // <-- args[2]
                CueNumber = cueResponse.Arguments[3]?.ToString() ?? "",   // <-- args[3]
                PartNumber = Convert.ToInt32(cueResponse.Arguments[4])
            };
            
            cueList.Add(cueInfo);
            GameConsole.Debug($"Loaded cue: {cueInfo.DisplayText}");
        }
        
        return cueList;
    }
    catch (TimeoutException ex)
    {
        GameConsole.Error($"Timeout retrieving cue list: {ex.Message}");
        throw;
    }
}
```

### Awaiting Specific Responses

```csharp
private async Task<OscMessage> WaitForResponseAsync(string addressPrefix, int timeoutMs)
{
    var tcs = new TaskCompletionSource<OscMessage>();
    
    void OnMessageReceived(object? sender, OscMessageReceivedEventArgs e)
    {
        if (e.Address.StartsWith(addressPrefix))
        {
            var message = new OscMessage 
            { 
                Address = e.Address, 
                Arguments = e.Arguments 
            };
            tcs.TrySetResult(message);
        }
    }
    
    // Subscribe to incoming messages
    _oscClient.MessageReceived += OnMessageReceived;
    
    try
    {
        // Race between response and timeout
        var completedTask = await Task.WhenAny(
            tcs.Task, 
            Task.Delay(timeoutMs)
        );
        
        if (completedTask == tcs.Task)
        {
            return await tcs.Task;
        }
        else
        {
            throw new TimeoutException(
                $"No response received for {addressPrefix} within {timeoutMs}ms"
            );
        }
    }
    finally
    {
        // Always unsubscribe
        _oscClient.MessageReceived -= OnMessageReceived;
    }
}
```

---

## Settings UI Integration

### Refresh Cues Button Handler

```csharp
private async void btnRefreshCues_Click(object sender, EventArgs e)
{
    if (_activePlugin == null || !_activePlugin.IsConnected)
    {
        MessageBox.Show("Please connect to the lighting console first.", "Not Connected");
        return;
    }
    
    try
    {
        btnRefreshCues.Enabled = false;
        btnRefreshCues.Text = "Loading...";
        Cursor = Cursors.WaitCursor;
        
        string selectedList = cmbCueList.SelectedItem?.ToString() ?? "1";
        
        // Query cues from console (listens on port 8001)
        var cues = await _activePlugin.GetCueListAsync(selectedList);
        
        // Populate dropdown with cue labels and numbers
        cmbAvailableCues.Items.Clear();
        foreach (var cue in cues)
        {
            // Format: "House Lights Up (Cue 1)"
            cmbAvailableCues.Items.Add(cue.DisplayText);
            cmbAvailableCues.Tag = cue; // Store full object
        }
        
        GameConsole.Info($"Loaded {cues.Count} cues from cue list {selectedList}");
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Failed to refresh cues: {ex.Message}", "Error");
        GameConsole.Error($"Cue refresh failed: {ex.Message}");
    }
    finally
    {
        btnRefreshCues.Enabled = true;
        btnRefreshCues.Text = "Refresh Cues";
        Cursor = Cursors.Default;
    }
}
```

### Event Mapping Dialog

When user selects a cue and maps it to a game event:

```csharp
private void btnAddMapping_Click(object sender, EventArgs e)
{
    var selectedCue = cmbAvailableCues.SelectedItem as LightingCueInfo;
    if (selectedCue == null) return;
    
    string gameEvent = cmbGameEvent.SelectedItem?.ToString() ?? "";
    
    var mapping = new LightingCueMapping
    {
        GameEvent = gameEvent,
        CueIdentifier = selectedCue.CueNumber,  // Use args[3]
        CueListId = cmbCueList.SelectedItem?.ToString() ?? "1",
        CommandType = LightingCommandType.FireCue,
        Enabled = true
    };
    
    // Add to ListView
    var item = new ListViewItem(new[]
    {
        gameEvent,
        selectedCue.Label,      // Display args[2]
        selectedCue.CueNumber   // Show args[3]
    });
    item.Tag = mapping;
    lvCueMappings.Items.Add(item);
}
```

---

## Execution Flow Diagram

```
┌─────────────────────────────────────────────────────────────┐
│               User Interface (Settings Dialog)              │
└─────────────────────────┬───────────────────────────────────┘
                          │
                          ├─▶ User clicks "Refresh Cues"
                          │
┌─────────────────────────▼───────────────────────────────────┐
│            EtcIonPlugin.GetCueListAsync()                   │
├─────────────────────────────────────────────────────────────┤
│  1. Send: /eos/get/cue/1/count → UDP 8000                   │
│  2. WaitForResponseAsync("/eos/out/get/cue/1/count")        │
└─────────────────────────┬───────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────┐
│         EtcOscClient (Background UDP Listener)              │
├─────────────────────────────────────────────────────────────┤
│  ListenForMessagesAsync() running on port 8001              │
│  ◄── Receives: /eos/out/get/cue/1/count [25]                │
│  Fires: MessageReceived event                               │
└─────────────────────────┬───────────────────────────────────┘
                          │
                          ├─▶ TaskCompletionSource completes
                          │
┌─────────────────────────▼───────────────────────────────────┐
│  Loop for each cue (0 to 24):                               │
│  3. Send: /eos/get/cue/1/{i}/* → UDP 8000                   │
│  4. WaitForResponseAsync("/eos/out/get/cue/1/{i}")          │
└─────────────────────────┬───────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────────┐
│         EtcOscClient (Background UDP Listener)              │
├─────────────────────────────────────────────────────────────┤
│  ◄── Receives: [0, "abc123", "House Lights", "1", 0]        │
│  Fires: MessageReceived event                               │
│       args[2] = "House Lights" (Label)                      │
│       args[3] = "1" (Cue Number)                            │
└─────────────────────────┬───────────────────────────────────┘
                          │
                          ├─▶ Create LightingCueInfo object
                          │
┌─────────────────────────▼───────────────────────────────────┐
│  5. Return List<LightingCueInfo> to UI                      │
│  6. Populate dropdown:                                      │
│     - "House Lights (Cue 1)"                                │
│     - "Stage Wash (Cue 1.5)"                                │
│     - "Contestant Spot (Cue 2)"                             │
└─────────────────────────────────────────────────────────────┘
```

---

## Error Handling

### Timeout Protection
```csharp
if (await Task.WhenAny(tcs.Task, Task.Delay(timeoutMs)) != tcs.Task)
{
    throw new TimeoutException("Console did not respond in time");
}
```

### Connection Failures
```csharp
try
{
    var cues = await _activePlugin.GetCueListAsync(cueListId);
}
catch (TimeoutException)
{
    MessageBox.Show("Console did not respond. Check network connection.");
}
catch (SocketException ex)
{
    MessageBox.Show($"Network error: {ex.Message}");
}
```

### Partial Responses
- If some cues timeout, return partial list with warning
- Log which cue indices failed to load
- Allow user to retry individual cues

---

## Testing Checklist

- [ ] UDP port 8001 listener starts on connection
- [ ] MessageReceived event fires for incoming packets
- [ ] OSC packets correctly parsed (address + arguments)
- [ ] Cue count query returns expected count
- [ ] Individual cue queries parse args[2] and args[3]
- [ ] Timeout handling prevents UI freeze
- [ ] Dropdown populated with "{Label} (Cue {Number})" format
- [ ] Selected cue stores correct CueNumber for triggering
- [ ] Listener stops cleanly on disconnect
- [ ] Multiple refresh operations don't leak listeners

---

## Performance Considerations

- **Background Listening**: UDP listener runs continuously (low CPU)
- **Event-Driven**: No polling, only respond to incoming packets
- **Async Throughout**: No UI blocking during queries
- **Caching**: Store cue list in memory, refresh only on button click
- **Throttling**: Limit query rate if console is slow (add delays between queries)

---

## Notes

- Ion console must be configured to send OSC feedback on port 8001
- Network firewall must allow UDP 8000 (outbound) and 8001 (inbound)
- Large cue lists (100+ cues) may take several seconds to load
- Consider showing progress bar for large cue lists
- args[2] (Label) is for display only, args[3] (CueNumber) is what gets sent to trigger
