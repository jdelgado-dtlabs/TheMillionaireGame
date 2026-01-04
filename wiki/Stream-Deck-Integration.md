# Stream Deck Integration

The Millionaire Game includes full support for Elgato Stream Deck hardware, allowing hosts to control the game using physical buttons. This guide covers setup, configuration, and available controls.

---

## 🎮 Overview

Stream Deck integration provides tactile, visual control over game operations through a dedicated 6-button module. Each button is context-aware, showing relevant actions based on the current game state.

### Key Benefits
- **Physical Control** - Tactile feedback for confident operation
- **Visual Feedback** - Button states update in real-time
- **Streamlined Workflow** - Most common actions at your fingertips
- **Host-Focused** - Designed specifically for host control operations

---

## 📋 Requirements

### Hardware
- **Elgato Stream Deck** (any model)
- **Minimum 6 buttons** recommended for full host control module
- USB connection to the PC running The Millionaire Game

### Software
- **Stream Deck Software** - Latest version from Elgato
- **The Millionaire Game v1.0+** - Stream Deck support built-in
- **Windows 10/11** - Required for both applications

---

## 🔧 Setup

### 1. Install Stream Deck Software

1. Download from [Elgato's website](https://www.elgato.com/en/downloads)
2. Install and connect your Stream Deck device
3. Verify device is recognized in Stream Deck software

### 2. Configure The Millionaire Game

1. Launch The Millionaire Game
2. Open **Control Panel**
3. Navigate to **Settings** → **Stream Deck**
4. Enable **Stream Deck Integration**
5. Configure button layout (6-button host control module)

### 3. Link Actions to Buttons

The application communicates directly with Stream Deck hardware through the Elgato SDK:

1. Launch **The Millionaire Game** with Stream Deck enabled
2. The application automatically detects connected Stream Deck devices
3. Buttons are automatically configured based on your saved layout
4. Button states update in real-time as game state changes

---

## 🎯 Host Control Module (6 Buttons)

The recommended layout uses 6 buttons for essential host operations:

### Button Layout

```
┌─────────┬─────────┬─────────┐
│ Button 1│ Button 2│ Button 3│
│  REVEAL │  ANSWER │ LIFELINE│
├─────────┼─────────┼─────────┤
│ Button 4│ Button 5│ Button 6│
│  NEXT Q │   WALK  │  FINAL  │
└─────────┴─────────┴─────────┘
```

### Button Functions

#### Button 1: REVEAL
- **Primary Action**: Reveal correct answer after contestant response
- **Game Phase**: Main game question resolution
- **Visual State**: Highlights in green when available
- **Usage**: Press after contestant has locked in their answer

#### Button 2: ANSWER
- **Primary Action**: Lock in contestant's answer selection
- **Game Phase**: During question with selected answer
- **Visual State**: Orange when answer is selected but not locked
- **Usage**: Confirm contestant's choice before revealing

#### Button 3: LIFELINE
- **Primary Action**: Quick access to lifeline activation
- **Game Phase**: Active question with available lifelines
- **Visual State**: Blue with lifeline icons
- **Usage**: Opens lifeline selection menu

#### Button 4: NEXT Q
- **Primary Action**: Advance to next question
- **Game Phase**: After correct answer or walk away
- **Visual State**: Green arrow when ready to proceed
- **Usage**: Progress through the money ladder

#### Button 5: WALK
- **Primary Action**: Contestant walks away with current winnings
- **Game Phase**: Active question before answer lock
- **Visual State**: Yellow warning indicator
- **Usage**: Activate when contestant chooses to leave

#### Button 6: FINAL
- **Primary Action**: Final answer confirmation
- **Game Phase**: After answer selected, before lock
- **Visual State**: Red when in "final answer" mode
- **Usage**: Dramatic final answer moment before lock

---

## 🎨 Button States

Buttons dynamically change appearance based on game state:

### Available (Active)
- **Appearance**: Full color, bright icon
- **Behavior**: Press to activate function
- **Indicator**: Pulsing or solid color

### Unavailable (Disabled)
- **Appearance**: Dimmed/grayed out
- **Behavior**: No action when pressed
- **Indicator**: Faded icon

### In Progress
- **Appearance**: Animated or flashing
- **Behavior**: Action is executing
- **Indicator**: Progress animation

### Completed
- **Appearance**: Success checkmark or color change
- **Behavior**: Brief confirmation before returning to normal
- **Indicator**: Green flash or checkmark

---

## ⚙️ Configuration Options

### Button Customization

Access via **Settings** → **Stream Deck**:

- **Button Icons** - Choose from preset icons or upload custom images
- **Button Labels** - Show/hide text labels on buttons
- **Color Scheme** - Match your event branding
- **Haptic Feedback** - Enable vibration confirmation (if supported)

### Action Mapping

Customize which game actions map to which buttons:

1. Open **Stream Deck Settings** in The Millionaire Game
2. Select **Action Mapping**
3. Drag and drop actions to desired button positions
4. Save configuration

### Profiles

Create multiple button profiles for different scenarios:

- **Standard Game** - Main game host controls
- **FFF Mode** - Fastest Finger First controls
- **Practice Mode** - Training/demo controls

---

## 🔄 Context-Aware Behavior

The Stream Deck integration is fully context-aware:

### During Fastest Finger First
- Buttons show FFF-specific actions
- Start round, reveal order, select winner

### During Main Game
- Standard host control layout active
- Question navigation, answer handling, lifelines

### During Break/Idle
- Minimal active buttons
- Start new game, load settings, exit

---

## 🐛 Troubleshooting

### Stream Deck Not Detected

**Symptoms**: Application doesn't recognize Stream Deck device

**Solutions**:
1. Verify Stream Deck software is running
2. Check USB connection
3. Restart Stream Deck service
4. Restart The Millionaire Game

### Buttons Not Responding

**Symptoms**: Pressing buttons has no effect

**Solutions**:
1. Check if Stream Deck integration is enabled in settings
2. Verify correct profile is active
3. Confirm game is in focus (not minimized)
4. Check for action mapping conflicts

### Icons Not Updating

**Symptoms**: Button states don't change with game state

**Solutions**:
1. Restart Stream Deck software
2. Re-enable integration in The Millionaire Game
3. Clear Stream Deck cache
4. Update Stream Deck software to latest version

### Action Delay

**Symptoms**: Noticeable lag between button press and action

**Solutions**:
1. Close unnecessary background applications
2. Check CPU usage in Task Manager
3. Verify Stream Deck polling rate in settings
4. Update USB drivers

---

## 💡 Best Practices

### Host Operation

1. **Practice First** - Familiarize yourself with button layout before live events
2. **Visual Confirmation** - Always check game state after button press
3. **Backup Controls** - Keep keyboard shortcuts handy as backup
4. **Button Discipline** - Avoid accidental presses during dramatic moments

### Event Setup

1. **Test Beforehand** - Verify all buttons work before contestants arrive
2. **Spare Device** - Keep backup Stream Deck if possible
3. **Clear Layout** - Use simple, readable button labels
4. **Lighting** - Ensure button labels are visible in event lighting

### Custom Configurations

1. **Match Branding** - Customize colors to match event theme
2. **Simplify Layout** - Only show buttons you actually use
3. **Save Profiles** - Create profiles for different event types
4. **Document Changes** - Keep notes on custom configurations

---

## 🔗 Related Documentation

- **[Quick Start Guide](Quick-Start-Guide)** - Basic game operation
- **[User Guide](User-Guide)** - Complete feature documentation
- **[Troubleshooting](Troubleshooting)** - General problem solving
- **[Configuration Files](Configuration-Files)** - Advanced settings

---

## 📞 Support

**Issues with Stream Deck Integration?**
- [Report a Bug](https://github.com/jdelgado-dtlabs/TheMillionaireGame/issues)
- [Ask Questions](https://github.com/jdelgado-dtlabs/TheMillionaireGame/discussions)
- [Elgato Support](https://help.elgato.com/) - For Stream Deck hardware issues

---

## 📝 Version History

### v1.0
- ✅ Initial Stream Deck support
- ✅ 6-button host control module
- ✅ Context-aware button states
- ✅ Custom action mapping
- ✅ Profile support

### Future Enhancements
- 🔄 15-button full control layout
- 🔄 Multi-page button profiles
- 🔄 Advanced animation effects
- 🔄 Custom button image uploads

---

**Ready to use Stream Deck?** Make sure Stream Deck software is running, then enable integration in The Millionaire Game settings!
