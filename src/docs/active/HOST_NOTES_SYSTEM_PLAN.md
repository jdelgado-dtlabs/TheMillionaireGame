# Host Notes/Messaging System Implementation Plan

## Overview
Implement a real-time messaging system that allows the control panel operator to send notes/messages to the host screen during gameplay.

## Requirements
- Chat/message input on Control Panel
- Message display on Host Screen
- Compact UI sizing to fit existing layouts
- Real-time message delivery
- No blocking of UI operations
- Message persistence during game session

## Architecture

### Communication Layer
**Option 1: Event-Based (Recommended)**
- Use existing event pattern similar to other screen updates
- Add `MessageSent` event to ControlPanelForm
- Subscribe HostScreenForm to the event
- Advantages: Simple, consistent with existing architecture, no additional dependencies

**Option 2: Messaging Service**
- Create dedicated MessageService in Services folder
- Singleton pattern for centralized message distribution
- Advantages: More scalable for future multi-screen messaging

**Decision: Option 1** - Use event-based approach for consistency and simplicity

### UI Components

#### Control Panel (ControlPanelForm)
**Location:** Bottom section, near existing game controls
**Components:**
- `txtHostMessage`: TextBox (multi-line, ~300-400px width, ~60-80px height)
- `btnSendMessage`: Button ("Send" or "→", ~60-80px width)
- `lblMessageLabel`: Label ("Host Note:")
- Optional: `chkShowOnHost`: CheckBox to toggle visibility

**Layout:**
```
[Host Note:] [___________________________] [Send]
             [                           ]
```

**Keyboard Behavior:**
- **Enter key**: Send message (when textbox has focus)
- **Alt+Enter**: Insert newline (for multi-line messages)
- Standard chat/messaging interface behavior

**Size Constraints:**
- Height: ~60-80px (multi-line)
- Total width: ~400-500px
- Position: Bottom-left or below question controls

#### Host Screen (HostScreenForm)
**Location:** Bottom-right corner or top-right corner
**Components:**
- `txtHostNotes`: TextBox or Label
  - Multi-line if showing history
  - Single line if showing only current message
  - Semi-transparent background (60-80% opacity)
  - Auto-hide after timeout (optional)

**Display Options:**
- **Option A:** Floating overlay (always on top)
- **Option B:** Fixed position, slides in when message received
- **Option C:** Static position, toggleable visibility

**Size Constraints:**
- Width: 300-400px
- Height: 50-100px (adjustable based on content)
- Font: Readable from distance (14-16pt)

### Data Model

```csharp
public class HostMessage
{
    public string Text { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsVisible { get; set; }
}
```

### Event Architecture

```csharp
// In ControlPanelForm
public event EventHandler<HostMessageEventArgs>? MessageSent;

public class HostMessageEventArgs : EventArgs
{
    public string Message { get; set; }
    public bool ShowOnScreen { get; set; }
}

// Usage
private void btnSendMessage_Click(object sender, EventArgs e)
{
    SendMessage();
}

private void txtHostMessage_KeyDown(object sender, KeyEventArgs e)
{
    // Enter to send, Alt+Enter for newline
    if (e.KeyCode == Keys.Enter && !e.Alt)
    {
        e.Handled = true;
        e.SuppressKeyPress = true;
        SendMessage();
    }
    // Alt+Enter inserts newline (default behavior, no special handling needed)
}

private void SendMessage()
{
    if (string.IsNullOrWhiteSpace(txtHostMessage.Text))
        return;
        
    MessageSent?.Invoke(this, new HostMessageEventArgs 
    { 
        Message = txtHostMessage.Text.Trim(),
        ShowOnScreen = true  // or use checkbox value
    });
    txtHostMessage.Clear();
}

// In HostScreenForm
public void OnMessageReceived(object? sender, HostMessageEventArgs e)
{
    if (InvokeRequired)
    {
        Invoke(() => OnMessageReceived(sender, e));
        return;
    }
    
    if (e.ShowOnScreen)
    {
        txtHostNotes.Text = e.Message;
        txtHostNotes.Visible = true;
        // Optional: Auto-hide after delay
    }
    else
    {
        txtHostNotes.Visible = false;
    }
}
```

## Implementation Steps

### Phase 1: Core Messaging (Essential)
1. ✅ Create plan document
2. ⬜ Add message event infrastructure
   - Define `HostMessageEventArgs` class
   - Add `MessageSent` event to ControlPanelForm
3. ⬜ Add Control Panel UI
   - Add TextBox, Button, Label to ControlPanelForm.Designer.cs
   - Wire up button click handler
4. ⬜ Add Host Screen UI
   - Add message display control to HostScreenForm.Designer.cs
   - Position in non-intrusive location
5. ⬜ Connect event handlers
   - Subscribe HostScreenForm to ControlPanelForm.MessageSent
   - Implement message display logic
6. ⬜ Test basic messaging flow

### Phase 2: Enhancements (Optional)
7. ⬜ Add message history (store last 5-10 messages)
8. ⬜ Add auto-hide timer (fade out after 30-60 seconds)
9. ⬜ Add message templates/quick messages
10. ⬜ Add clear/hide button on host screen
11. ⬜ Persist messages across screen refreshes

### Phase 3: Polish (Nice-to-Have)
13. ⬜ Add slide-in/fade animations
14. ⬜ Add sound notification on message received
15. ⬜ Style message box to match game theme
16. ⬜ Add message priority levels (normal, urgent)

## UI Layout Mockup

### Control Panel (Bottom Section)
```
┌─────────────────────────────────────────────────────────┐
│ [Show Contestant]  [Call Guest]  [Lock In Answer]     │
│                                                         │
│ Host Note: [Type message here...              ] [Send] │
│            [Press Enter to send, Alt+Enter for ]       │
│            [new line...                        ]       │
└─────────────────────────────────────────────────────────┘
```

### Host Screen (Top-Right Corner)
```
┌──────────────────────────────────────────────┐
│                                  ┌──────────┐│
│  MILLIONAIRE                     │ Check    ││
│                                  │ question ││
│  Question: What is...            │ timing   ││
│                                  └──────────┘│
│  A: Option A                                 │
│  B: Option B                                 │
└──────────────────────────────────────────────┘
```

## Technical Considerations

### Thread Safety
- All UI updates must use `Invoke()` for cross-thread safety
- Event handlers must check `InvokeRequired`

### Performance
- Messages are lightweight (just string data)
- No impact on game performance
- UI updates are async and non-blocking

### Error Handling
- Null reference checks on event invocation
- Try-catch blocks around UI updates
- Graceful degradation if messaging fails

### Settings Integration
- Add option to enable/disable host notes
- Save preference in ApplicationSettings
- Default: Enabled

## Testing Checklist

- [ ] Send message from control panel → appears on host screen
- [ ] Send multiple messages rapidly → no UI freezing
- [ ] Enter key sends message when textbox has focus
- [ ] Alt+Enter inserts newline in textbox
- [ ] Empty messages are not sent
- [ ] Message display on host screen is readable from distance
- [ ] Message box doesn't block important game information
- [ ] Clear/hide message functionality works
- [ ] Messages persist during question changes
- [ ] No crashes if host screen not open
- [ ] No crashes if message contains special characters
- [ ] Keyboard shortcuts work correctly
- [ ] Settings persistence works

## Future Enhancements

1. **Multi-screen messaging**: Send to guest screen, TV screen
2. **Bidirectional messaging**: Host can send messages back
3. **Message logging**: Save all messages to file
4. **Remote messaging**: Send messages via web interface
5. **Message categories**: Questions, timing reminders, general notes
6. **Integration with lifelines**: Auto-message on lifeline activation
7. **Teleprompter mode**: Display scrolling script text

## Related Files

- `src/MillionaireGame/Forms/ControlPanelForm.cs` - Control panel logic
- `src/MillionaireGame/Forms/ControlPanelForm.Designer.cs` - Control panel UI
- `src/MillionaireGame/Forms/HostScreenForm.cs` - Host screen logic
- `src/MillionaireGame/Forms/HostScreenForm.Designer.cs` - Host screen UI
- `src/MillionaireGame.Core/Settings/ApplicationSettings.cs` - Settings storage

---
**Status:** Planning Complete - Ready for Implementation
**Priority:** High (Outstanding Bug)
**Estimated Time:** 2-4 hours for Phase 1
