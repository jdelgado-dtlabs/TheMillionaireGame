# Stream Deck Integration

The Millionaire Game includes direct hardware integration with Elgato Stream Deck devices, allowing hosts to lock in and reveal answers using physical buttons. This guide covers setup, configuration, and available controls.

---

## 🎮 Overview

Stream Deck integration provides tactile, visual control over answer locking and reveal operations during gameplay. The 6-button layout displays answer choices (A, B, C, D), a reveal button, and a dynamic feedback indicator that shows correctness before revealing to the audience.

### Key Benefits
- **Physical Control** - Tactile feedback for confident answer locking
- **Visual Feedback** - Button states update in real-time with game state
- **Pre-Reveal Confirmation** - See if answer is correct/incorrect before revealing to audience
- **Direct Hardware Control** - No Stream Deck software required while playing

### Important Note
This integration communicates directly with Stream Deck hardware through the StreamDeckSharp library (OpenMacroBoard SDK). The official Elgato Stream Deck software must be **CLOSED** while using The Millionaire Game, as both applications cannot control the device simultaneously.

---

## 📋 Requirements

### Supported Hardware
- **Stream Deck Module 6** (6 buttons, 3x2 layout) - Fully tested ✅
- **Stream Deck Mini** (6 buttons, 3x2 layout) - Should work, not tested ⚠️

**Note:** This application includes a custom build of StreamDeckSharp with Module 6 (USB PID 0x00B8) support. The official NuGet package does not yet include this support, so custom DLLs are bundled in `lib/StreamDeck/` until the upstream pull request is merged.

### Unsupported Hardware
- Stream Deck (15 buttons) - Requires 6-button layout
- Stream Deck XL (32 buttons) - Requires 6-button layout
- Stream Deck MK.2 (15 buttons) - Requires 6-button layout
- Stream Deck Plus - Different layout, not tested

### Software
- **The Millionaire Game v1.0.5+** - Stream Deck support built-in with embedded images
- **Windows 10/11** - Required operating system
- **NO Stream Deck Software Required** - Direct hardware control via StreamDeckSharp library
- **Stream Deck Software Must Be CLOSED** - Conflicts with direct hardware control

---

## 🔧 Setup

### 1. Hardware Connection

1. **Close Stream Deck Software** - The official Elgato software MUST be closed
   - Right-click system tray icon → Exit
   - Or kill via Task Manager if needed
2. **Connect Stream Deck Device** - Plug in via USB
3. **Verify Device** - Device will be detected automatically by The Millionaire Game when enabled

### 2. Enable in Application

1. Launch **The Millionaire Game**
2. Open **Control Panel**
3. Navigate to **Game** → **Settings** → **StreamDeck** tab
4. Check **Enable Stream Deck Integration**
5. Click **Save** to apply settings
6. **Restart The Millionaire Game** - Required for Stream Deck initialization

### 3. Verify Connection

After restarting with Stream Deck enabled:

1. Check **Game Console** (Game → Settings → Screens tab → Open Console)
2. Look for initialization messages:
   ```
   [StreamDeck] Detected 1 Stream Deck device(s):
     - Stream Deck Module 6: 6 buttons, 3x2 grid (96x96px)
   [StreamDeck] Found compatible device: Stream Deck Module 6
   [StreamDeck] Connected to Stream Deck Module 6 (2x3 layout)
   [StreamDeckIntegration] Initialized successfully
   ```
3. Stream Deck should show all blank buttons initially

### Troubleshooting Setup

**Device Not Detected:**
- Verify Stream Deck software is CLOSED (check Task Manager)
- Unplug and replug USB cable
- Try different USB port (USB 2.0 or 3.0, Module 6 uses USB 2.0 protocol)
- Verify custom StreamDeckSharp DLLs are present in application's `lib/StreamDeck/` folder

**Integration Failed:**
- Restart application with Stream Deck plugged in
- Check Windows Device Manager for USB device
- Verify Stream Deck works in official Elgato software first
- Review logs in `%LocalAppData%\TheMillionaireGame\Logs\`

---

## 🎯 Button Layout (6 Buttons)

The application uses a fixed 6-button layout optimized for answer lock and reveal workflow:

### Physical Layout

```
┌─────────────┬─────────────┬─────────────┐
│  Dynamic    │  Answer A   │  Answer B   │
│  (Row 0)    │  (Row 0)    │  (Row 0)    │
│             │             │             │
│  Feedback   │      A      │      B      │
│  Indicator  │             │             │
├─────────────┼─────────────┼─────────────┤
│  Reveal     │  Answer C   │  Answer D   │
│  (Row 1)    │  (Row 1)    │  (Row 1)    │
│             │             │             │
│      👁     │      C      │      D      │
│             │             │             │
└─────────────┴─────────────┴─────────────┘
```

### Button Mapping

**Row 0 (Top Row):**
- **Col 0:** Dynamic Feedback Indicator (not pressable)
- **Col 1:** Answer A
- **Col 2:** Answer B

**Row 1 (Bottom Row):**
- **Col 0:** Reveal Button
- **Col 1:** Answer C
- **Col 2:** Answer D

---

## 🎮 How It Works

### Workflow

1. **Question Displayed**
   - Answer buttons A, B, C, D become active (show answer icons)
   - Dynamic button and Reveal button are blank (disabled)
   - Host waits for contestant to make their choice

2. **Host Locks Answer**
   - Press the button corresponding to contestant's answer (A, B, C, or D)
   - Selected answer button shows "lock" icon (answer-a-lock.png, etc.)
   - Other answer buttons go blank (disabled)
   - **Dynamic button shows feedback:**
     - ✅ Green checkmark = Correct answer
     - ❌ Red X = Incorrect answer
   - Reveal button becomes active

3. **Host Reveals Answer**
   - Press the Reveal button (bottom left)
   - Game performs reveal sequence on screens
   - All buttons go blank (disabled) after reveal
   - Ready for next question

### Context-Aware Behavior

- **No Active Question:** All buttons blank (disabled)
- **Question Displayed:** Answer buttons active, Dynamic/Reveal blank
- **Answer Locked:** Locked answer highlighted, Dynamic shows correctness, Reveal active
- **After Reveal:** All buttons blank until next question

---

## 🎨 Button Images

The following images are used (located in `lib/image/streamdeck/`):

### Answer Buttons (Active State)
- `answer-a.png` - Answer A button
- `answer-b.png` - Answer B button
- `answer-c.png` - Answer C button
- `answer-d.png` - Answer D button

### Answer Buttons (Locked State)
- `answer-a-lock.png` - Answer A locked
- `answer-b-lock.png` - Answer B locked
- `answer-c-lock.png` - Answer C locked
- `answer-d-lock.png` - Answer D locked

### Reveal Button
- `answer-reveal.png` - Reveal button (active)

### Dynamic Feedback
- `correct.png` - Green checkmark (correct answer)
- `wrong.png` - Red X (incorrect answer)

### Disabled State
- `blank.png` - Blank/disabled button

**Image Specifications:**
- Resolution: 96x96 pixels (automatically resized from source images)
- Format: PNG with transparency support
- Color Space: RGB24

---

## ⚙️ Technical Details

### Library & SDK
- **StreamDeckSharp (Custom Build)** - Open-source .NET library for Stream Deck hardware with Module 6 support
- **OpenMacroBoard SDK** - Cross-device abstraction layer
- **Upstream Repository:** [github.com/OpenMacroBoard/StreamDeckSharp](https://github.com/OpenMacroBoard/StreamDeckSharp)
- **Custom Fork (Module 6 Support):** [github.com/jdelgado-dtlabs/StreamDeckSharp](https://github.com/jdelgado-dtlabs/StreamDeckSharp) (branch: `add-streamdeck-module6-support`)

**Note:** This application bundles custom-built StreamDeckSharp DLLs located in `lib/StreamDeck/` that include Module 6 support. A pull request has been submitted to the upstream repository. Once merged, the application will switch back to the official NuGet package.

### Device Detection
The application automatically enumerates all connected Stream Deck devices at startup and selects the first device matching these criteria:
- **Button Count:** Exactly 6 buttons
- **Layout:** 3x2 grid (3 columns, 2 rows)

### Key Indexing
Stream Deck uses row-major ordering for key indices:
```
Physical Layout:  [0] [1] [2]
                  [3] [4] [5]

Calculation: index = (row × 3) + col
```

### Event System
- **AnswerButtonPressed** - Fired when A, B, C, or D pressed
- **RevealButtonPressed** - Fired when Reveal button pressed
- **DeviceConnected** - Fired when device connects/reconnects
- **DeviceDisconnected** - Fired when device disconnects

### Integration with ControlPanelForm
The `StreamDeckIntegration` class bridges StreamDeckService events to ControlPanelForm:
- **AnswerLockedByHost** - Triggers answer lock logic in Control Panel
- **RevealTriggeredByHost** - Triggers reveal sequence in Control Panel
- **DeviceStatusChanged** - Updates UI connection status indicator

### Brightness
- Default brightness: 80%
- Set during initialization via `SetBrightness(80)`

---

## 🐛 Troubleshooting

### Stream Deck Not Detected

**Symptoms:** 
- Console shows "No Stream Deck devices found"
- Console shows "Stream Deck Module 6 (PID 0x00B8) is NOT supported"

**Solutions:**
1. **Close Stream Deck Software** - The official Elgato software conflicts with direct hardware control
   - Right-click system tray icon → Exit
   - Check Task Manager → End "Stream Deck" process
3. **Verify USB Connection** - Unplug and replug device
4. **Try Different USB Port** - USB 2.0 or 3.0 (Module 6 uses USB 2.0 protocol via USB-C connector)
5. **Check Device Manager** - Verify device shows up under "Human Interface Devices"
6. **Restart Application** - Close and reopen The Millionaire Game

### Wrong Device Layout

**Symptoms:** 
- Console shows "Skipping [Device Name] (X buttons) - requires 6-button device"
- Console shows "Skipping [Device Name] (XxY layout) - requires 3x2 layout"

**Solutions:**
1. **Use Compatible Device** - Requires 6-button device with 3x2 layout (Module 6 or Mini)
2. **Disconnect Other Devices** - Unplug other Stream Deck models to avoid conflicts

### Buttons Not Responding

**Symptoms:** Pressing buttons has no effect

**Solutions:**
1. **Verify Integration Enabled** - Check Game → Settings → General → Enable Stream Deck Integration
2. **Restart Application** - Integration initializes at startup
3. **Check Active Question** - Buttons only work during active questions
4. **Review Console Logs** - Check for "Answer X pressed" messages when pressing buttons
5. **Answer Already Locked** - Can only lock once per question

### Images Not Displaying

**Symptoms:** 
- Buttons show blank or missing images
- Console shows "IMAGE NOT FOUND" errors

**Solutions:**
1. **Verify Image Files** - Check `lib/image/streamdeck/` folder contains all required PNG files:
   - answer-a.png, answer-b.png, answer-c.png, answer-d.png
   - answer-a-lock.png, answer-b-lock.png, answer-c-lock.png, answer-d-lock.png
   - answer-reveal.png, correct.png, wrong.png, blank.png
2. **Check File Permissions** - Ensure application can read image files
3. **Reinstall Application** - May restore missing image files

### Device Disconnects During Use

**Symptoms:** Console shows "Device disconnected" during gameplay

**Solutions:**
1. **Check USB Cable** - Ensure cable is securely connected
2. **Try Different USB Port** - Avoid USB hubs, use direct motherboard connection
3. **Update USB Drivers** - Check Windows Update for driver updates
4. **Power Supply** - Some USB ports may not provide enough power

### Answer Lock Not Working

**Symptoms:** Pressing answer buttons has no effect

**Solutions:**
1. **Wait for Question Display** - Buttons only active after question loads
2. **Check Console** - Look for "No active question - ignoring button press" warning
3. **Verify Not Already Locked** - Can only lock once per question (check for "Answer already locked" message)
4. **Integration State** - Verify integration didn't disconnect (check console for device status)

---

## 💡 Best Practices

### Operational Workflow

1. **Test Before Events** - Verify all buttons work before contestants arrive
2. **Keep Console Open** - Monitor device connection status during live events
3. **Physical Placement** - Position Stream Deck within easy reach of host podium
4. **Practice Session** - Familiarize yourself with button positions and workflow
5. **Backup Plan** - Keep keyboard shortcuts handy (F1-F4 for answers, F6 for reveal)

### Pre-Event Checklist

- [ ] Stream Deck plugged in via USB
- [ ] Stream Deck software CLOSED
- [ ] The Millionaire Game running with Stream Deck enabled
- [ ] Console shows "Initialized successfully"
- [ ] All buttons showing blank (disabled) at startup
- [ ] Test answer lock + reveal with practice question

### During Gameplay

1. **Wait for Contestant** - Don't lock answer until contestant verbally confirms
2. **Check Dynamic Button** - Green check or red X appears immediately after locking
3. **Dramatic Pause** - Use the pre-reveal feedback to build suspense before pressing Reveal
4. **Button Discipline** - Avoid accidental presses by resting hands away from device

### Troubleshooting During Events

If device disconnects mid-event:
1. **Switch to Keyboard** - Use F1-F4 (answers) and F6 (reveal) hotkeys
2. **Don't Restart** - Avoid interrupting gameplay to fix Stream Deck
3. **Fix After Round** - Replug device during break, check console for reconnection
4. **Continue Without** - Stream Deck is optional, all features accessible via keyboard

---

## 🔗 Related Documentation

- **[Quick Start Guide](Quick-Start-Guide)** - Basic game operation and hotkeys
- **[User Guide](User-Guide)** - Complete feature documentation
- **[Troubleshooting](Troubleshooting)** - General problem solving
- **[Building from Source](Building-from-Source)** - Developer information

---

## 📞 Support

### Application Issues
- [Report a Bug](https://github.com/jdelgado-dtlabs/TheMillionaireGame/issues)
- [Ask Questions](https://github.com/jdelgado-dtlabs/TheMillionaireGame/discussions)

### Stream Deck Hardware Issues
- [Elgato Support](https://help.elgato.com/) - For hardware problems, driver issues, official software

### StreamDeckSharp Library (Custom Build)
- [Report Library Issues](https://github.com/jdelgado-dtlabs/StreamDeckSharp/issues) - For bugs or device support requests with the custom Module 6 build
- **Note:** This application uses a custom build with Module 6 support. Issues should be reported to the custom fork maintainer who will add support and submit pull requests to upstream

---

## 📝 Version History

### v1.0 (Current)
- ✅ Stream Deck Module 6 support (6 buttons, 3x2 layout) - Fully tested
- ✅ Stream Deck Mini compatibility (6 buttons, 3x2 layout) - Should work, not tested
- ✅ Direct hardware control via custom StreamDeckSharp build (includes Module 6 support)
- ✅ Answer lock and reveal workflow
- ✅ Pre-reveal correctness feedback (Dynamic button)
- ✅ Context-aware button states
- ✅ Auto-detection and enumeration

### Known Limitations
- **Custom DLLs:** Application bundles custom StreamDeckSharp build in `lib/StreamDeck/` until upstream PR is merged
- **15-Button Layouts:** Not supported - requires 3x2 (6 button) layout
- **Multiple Devices:** Only first compatible device used
- **Custom Images:** Fixed image set, no UI for customization (can manually replace images in `lib/image/streamdeck/`)
- **FFF Mode:** No Stream Deck integration for Fastest Finger First (main game only)
- **Lifelines:** No Stream Deck buttons for lifeline activation (use Control Panel buttons)

### Future Enhancements (Post-v1.0)
- 🔄 Switch to official StreamDeckSharp NuGet package (once Module 6 PR is merged)
- 🔄 15-button layout support for Stream Deck / MK.2 (full game control)
- 🔄 Custom image upload via UI
- 🔄 FFF mode button integration
- 🔄 Lifeline quick buttons
- 🔄 Multi-device support (select device via settings)

---

**Ready to use Stream Deck?** Close the Stream Deck software, enable integration in settings, restart the application, and verify connection in the Game Console!
