# Elgato Stream Deck Integration Plan

**Date**: December 31, 2025  
**Priority**: Medium (v1.2 Target)  
**Estimated Time**: 12-16 hours  
**Status**: 📋 PLANNING

---

## 📋 Overview

Integrate Elgato Stream Deck hardware to provide physical button control for game operations, enhancing live production workflow with tactile feedback, LED status indicators, and multi-action support. This transforms the Stream Deck into a dedicated control surface for running the game show.

---

## 🎯 Goals

### Primary Objectives
1. **Physical Button Control**: Map all critical game functions to Stream Deck buttons
2. **Visual Feedback**: LED indicators show button availability and state
3. **Custom Icons**: Professional button graphics matching game theme
4. **Multi-Action Support**: Single press, long press, multi-tap gestures
5. **Dynamic Updates**: Button state changes reflect game state in real-time

### User Experience
- Host can control entire game without mouse/keyboard
- Visual confirmation of button availability before pressing
- Reduced cognitive load during live production
- Professional broadcast control surface feel

---

## 🔧 Integration Approaches

### Option 1: Stream Deck Plugin (Native Integration) ⭐ RECOMMENDED
**Description**: Create a custom Stream Deck plugin that communicates with the game via TCP/WebSocket

**Pros**:
- Native Stream Deck integration (appears in Stream Deck software)
- Full access to LED control, icons, feedback
- Multi-action support (press, long press, rotate for Stream Deck+)
- Persistent configuration (survives restarts)
- Professional appearance

**Cons**:
- Requires JavaScript/TypeScript plugin development
- Plugin distribution via Elgato Marketplace or manual install
- Separate codebase to maintain

**Toolset Required**:
- Stream Deck SDK (JavaScript/TypeScript)
- Node.js runtime
- WebSocket client library
- Icon assets (72x72 PNG with transparency)

### Option 2: HTTP API Integration (Simple)
**Description**: Stream Deck's "Website" action triggers HTTP endpoints

**Pros**:
- No plugin development needed
- Uses built-in Stream Deck "Website" actions
- Simpler implementation

**Cons**:
- No LED feedback or dynamic icons
- No button state management
- Limited to simple button presses
- Requires web server running

**Toolset Required**:
- ASP.NET Core Web API endpoints
- Stream Deck "Website" action configuration

### Option 3: Companion App Integration
**Description**: Use Bitfocus Companion software as middleware

**Pros**:
- Companion handles Stream Deck communication
- HTTP/WebSocket/OSC protocol support
- No plugin development

**Cons**:
- Requires third-party software (Companion)
- Additional configuration complexity
- Less integrated experience

---

## 🚀 Recommended Approach: Stream Deck Plugin

### Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                    ELGATO STREAM DECK                       │
│  ┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐  │
│  │50:50   │ │Phone   │ │ATA     │ │Switch  │ │Reveal  │  │
│  │(Green) │ │(Green) │ │(Green) │ │(Grey)  │ │(Orange)│  │
│  └────────┘ └────────┘ └────────┘ └────────┘ └────────┘  │
│  ┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐  │
│  │Answer A│ │Answer B│ │Answer C│ │Answer D│ │New Q   │  │
│  └────────┘ └────────┘ └────────┘ └────────┘ └────────┘  │
└─────────────────────────────────────────────────────────────┘
                            │
                   WebSocket Connection
                            │
┌─────────────────────────────────────────────────────────────┐
│            MILLIONAIRE GAME APPLICATION                     │
│  ┌───────────────────────────────────────────────────────┐ │
│  │     StreamDeckHub (SignalR Hub)                       │ │
│  │  - Command reception (button presses)                 │ │
│  │  - Game phase tracking & broadcasting                 │ │
│  │  - Button state management                            │ │
│  │  - FFF screen control integration                     │ │
│  └───────────────────────────────────────────────────────┘ │
│                            │                                │
│  ┌───────────────────────────────────────────────────────┐ │
│  │     Control Panel Integration                         │ │
│  │  - HotkeyHandler.ProcessKeyPress() invocation         │ │
│  │  - Button click simulation                            │ │
│  │  - State event subscriptions                          │ │
│  │  - FFFOfflineForm / FFFOnlineForm control             │ │
│  └───────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

**Game Phase State Machine**:
```
[Game Reset] → GamePhase.HostIntro
      ↓ Press "Host Intro" button
[Host Intro Plays] → GamePhase.PickPlayer
      ↓ Press "Pick Player" button
[FFF Opens] → GamePhase.FFFActive → **ALL BUTTONS DISABLED**
      ↓ FFF window closed
[FFF Closed] → GamePhase.LightsDown
      ↓ Press "Lights Down" button
[Lights Lower] → GamePhase.RoundActive
      ↓ Question → Answer → Repeat
[Round Ends] → GamePhase.RoundComplete
      ↓ Round Reset pressed
[Back to Pick Player] → GamePhase.PickPlayer
```

**Button State Synchronization Strategy**:
1. **On Connection**: Plugin queries `GetAllButtonStates()` for full state sync
2. **On Control Panel Change**: Game broadcasts `UpdateControlPanelButtonState(buttonName, enabled)`
3. **On FFF Open**: Game broadcasts `DisableAllButtons()`
4. **On FFF Close**: Game broadcasts `EnableAllButtons()` → Re-sync all states
5. **On Game Phase Change**: Game broadcasts `UpdateGamePhase(phase)` → Plugin updates Game Phase button icon/label
6. **On Lifeline Use**: Game broadcasts `UpdateLifelineState(position, LifelineState.Used)`
7. **On Settings Change**: Game broadcasts `UpdateLifelineConfiguration(lifelines)` → Plugin reloads icons

### Communication Protocol: WebSocket/SignalR

**Why SignalR?**
- Already implemented in WAPS infrastructure
- Bi-directional communication (commands + state updates)
- Automatic reconnection
- TypeScript/JavaScript client available

---

## 📦 Required Toolsets

### 1. Elgato Stream Deck SDK
**Download**: https://developer.elgato.com/documentation/stream-deck/sdk/overview/  
**Version**: Latest (v2.0+)  
**Language**: JavaScript/TypeScript  
**Purpose**: Plugin development framework

**Key Files**:
- `manifest.json` - Plugin metadata and action definitions
- `index.html` - Property Inspector UI (button configuration)
- `plugin/app.js` - Plugin backend (communicates with game)
- `inspector/inspector.js` - Property Inspector logic

### 2. Node.js Environment
**Version**: 18.x LTS or higher  
**Purpose**: Plugin runtime environment  
**Required Packages**:
```json
{
  "dependencies": {
    "@microsoft/signalr": "^8.0.0",
    "websocket": "^1.0.34"
  },
  "devDependencies": {
    "typescript": "^5.0.0",
    "@types/node": "^20.0.0"
  }
}
```

### 3. Stream Deck Software
**Download**: https://www.elgato.com/downloads  
**Version**: 6.4+ (latest)  
**Purpose**: Testing and plugin deployment

### 4. Icon Design Tools
**Recommended**: Adobe Photoshop, Affinity Designer, or Figma  
**Format**: 72x72 PNG with transparency  
**Style**: Match game theme (blue/gold colors, professional look)

**Required Icons** (minimum 20 + state variants):
- **Row 1**: New Question, Answer A, B, C, D (5)
- **Row 2**: 
  - Lifeline icons: 50:50, Phone, ATA, Switch, Double Dip, Ask Host, Plus One (7)
  - Unassigned lifeline: Black/blank (1)
  - Reveal Answer (1)
- **Row 3**: 
  - Game Phase: Host Intro, Pick Player, Lights Down (3)
  - Explain Game, Money Tree, Walk Away, Closing (4)

**State Variants (per button type)**:
- **Enabled**: Full color, normal appearance
- **Disabled**: Greyed out, 50% opacity overlay
- **Demo** (lifelines only): Pulsing animation during Explain Game
- **Used** (lifelines only): Red X overlay or crossed out
  - Icons dynamically loaded based on user's configuration
  - Support all available lifeline types
- Game Control: New Question, Reveal, Lights Down (3)
- Walk Away, Risk Mode (2)
- Level Up, Level Down (2)

**Note**: Lifeline icons are dynamically selected based on the user's Settings → Lifelines configuration. The plugin queries the game for current lifeline assignments and displays the appropriate icons.

**State Variants** (per button):
- Default (grey - disabled)
- Available (green/blue - clickable)
- Active (yellow - in use)
- Used (red - depleted)

### 5. MillionaireGame Application Changes

**New Components**:
- `StreamDeckHub.cs` - SignalR hub for Stream Deck communication
- `StreamDeckService.cs` - Button state management
- `StreamDeckModels.cs` - Command/state DTOs

**Modified Components**:
- `Program.cs` - Register StreamDeckHub endpoint
- `ControlPanelForm.cs` - Subscribe to game state events
- `WebServerHost.cs` - Add StreamDeck hub to endpoints

---

## 🏗️ Implementation Plan

### Phase 1: Plugin Skeleton (2-3 hours)

**Deliverables**:
1. Stream Deck plugin project structure
2. `manifest.json` with action definitions
3. Basic WebSocket connection to localhost
4. Simple button press logging

**Files to Create**:
```
StreamDeck-MillionaireGame/
├── manifest.json
├── package.json
├── plugin/
│   ├── app.js (or app.ts)
│   └── index.html
├── inspector/
│   ├── inspector.html
│   └── inspector.js
└── icons/
    ├── action@2x.png
    ├── category@2x.png
    └── pluginIcon@2x.png
```

### Phase 2: Game SignalR Hub (6-8 hours)

**Tasks**:
1. Create `StreamDeckHub.cs` with all button press handler methods
2. Create `GamePhase` enum and add tracking property to ControlPanelForm
3. **Implement full button state synchronization**:
   - Hook into ALL Control Panel button enable/disable events
   - Create `UpdateControlPanelButtonState()` broadcast method
   - Track button states and broadcast on change
4. **Implement FFF screen integration**:
   - Subscribe to FFF window open/close events
   - Broadcast `DisableAllButtons()` on FFF open
   - Broadcast `EnableAllButtons()` and re-sync states on FFF close
5. **Implement Game Phase tracking**:
   - Add phase transitions in existing button handlers (btnHostIntro, btnFFFOnline/Offline, etc.)
   - Broadcast `UpdateGamePhase()` on each transition
   - Handle Round Reset → PickPlayer phase
   - Handle Game Reset → HostIntro phase
6. **Implement lifeline state broadcasting**:
   - Track lifeline assignment (Settings → Lifelines)
   - Broadcast `UpdateLifelineConfiguration()` on startup and settings change
   - Broadcast `UpdateLifelineState()` when lifeline used/enabled/disabled
   - Handle Explain Game mode (lifelines in Demo state)
7. **Add connection state management**:
   - `GetAllButtonStates()` query method for initial sync
   - `GetLifelineConfiguration()` query method
   - `GetCurrentGamePhase()` query method
8. Add hub registration in `Program.cs`:
   ```csharp
   app.MapHub<StreamDeckHub>("/hubs/streamdeck");
   ```
9. Test with SignalR test client or browser console

**Hub Methods (Game → Plugin)**:
```csharp
// State broadcasts
Task UpdateButtonState(string buttonId, ButtonState state);
Task UpdateGamePhase(GamePhase phase); // Game Phase button state tracking
Task UpdateLifelineConfiguration(LifelineConfig[] lifelines); // On startup or settings change
Task UpdateLifelineState(int position, LifelineState state); // Available/Used/Disabled/Demo
Task UpdateControlPanelButtonState(string buttonName, bool enabled); // Sync all button states
Task DisableAllButtons(); // When FFF opens
Task EnableAllButtons(); // When FFF closes

// Connection management
Task OnConnected(string connectionId);
Task OnDisconnected(string connectionId);
```

**Hub Methods (Plugin → Game)**:
```csharp
// Row 1: Question & Answers
Task PressNewQuestion(); // → btnQuestion.PerformClick()
Task PressAnswer(string answer); // A, B, C, D → btnAnswerA-D.PerformClick()

// Row 2: Lifelines & Reveal
Task PressLifeline(int position); // 1-4 → btnLifeline1-4.PerformClick()
Task PressReveal(); // → btnReveal.PerformClick()

// Row 3: Game Flow
Task PressGamePhase(); // Multi-state: Host Intro, Pick Player, or Lights Down
Task PressExplainGame(); // → btnExplainGame.PerformClick()
Task PressMoneyTree(); // → btnMoneyTree.PerformClick()
Task PressWalkAway(); // → btnWalkAway.PerformClick()
Task PressClosing(); // → btnClosing.PerformClick() - Ends entire game

// Configuration queries
Task<LifelineConfig[]> GetLifelineConfiguration();
Task<GamePhase> GetCurrentGamePhase();
Task<Dictionary<string, bool>> GetAllButtonStates(); // Full state sync on connect
```

**LifelineState Enum**:
```csharp
public enum LifelineState
{
    Unassigned,  // Black button, always disabled
    Disabled,    // Greyed out, not yet available
    Available,   // Active, ready to use
    Demo,        // Active during Explain Game for "ping"
    Used         // Depleted, greyed out/crossed
}
```

**ButtonState Enum**:
```csharp
public enum ButtonState
{
    Disabled,    // Greyed out, no action on press
    Enabled,     // Normal appearance, clickable
    Active,      // Currently in use (e.g., pressed/animated)
    Hidden       // Not visible (for dynamic layouts)
}
```

**GamePhase Enum**:
```csharp
public enum GamePhase
{
    HostIntro,      // Initial state, "Host Intro" button visible
    PickPlayer,     // After host intro, "Pick Player" button visible
    FFFActive,      // FFF screen open, ALL Stream Deck buttons disabled
    LightsDown,     // FFF closed, "Lights Down" button visible (stays for round)
    RoundActive,    // Round in progress, "Lights Down" remains
    RoundComplete   // Round finished, ready for reset
}
```

**Phase Transitions**:
- **Game Reset**: → `HostIntro`
- **Host Intro Pressed**: `HostIntro` → `PickPlayer`
- **Pick Player Pressed** (FFF opens): `PickPlayer` → `FFFActive`
- **FFF Closed**: `FFFActive` → `LightsDown`
- **Lights Down Pressed**: `LightsDown` → `RoundActive`
- **Round Reset**: `RoundActive`/`RoundComplete` → `PickPlayer`

**FFFMode Enum**:
```csharp
public enum FFFMode
{
    Offline, // FFFOffline screen (local contestants)
    Online   // FFFOnline screen (remote contestants)
}
```

### Phase 3: Icon Design (4-5 hours)

**Tasks**:
1. Design base icon templates (72x72)
2. Create state variants (grey, green, yellow, red)
3. Export all icons as PNG with transparency
4. Organize in plugin icons folder

**Design Guidelines**:
- Use game color palette (blue: #001489, gold: #FFD700)
- Clear, readable text/symbols
- Professional broadcast aesthetic
- LED-friendly colors (high contrast)

### Phase 4: Plugin Logic (6-8 hours)

**Tasks**:
1. **SignalR connection management**:
   - Connect to localhost:5000/hubs/streamdeck
   - Query initial states on connection: `GetAllButtonStates()`, `GetLifelineConfiguration()`, `GetCurrentGamePhase()`
   - Implement auto-reconnect with state re-sync
   
2. **Button press event handling**:
   - Wire all 15 Stream Deck keys to corresponding hub methods
   - Add debounce logic (prevent double-press within 200ms)
   - Validate button enabled state before sending command
   
3. **Button state synchronization**:
   - Listen for `UpdateControlPanelButtonState(buttonName, enabled)` broadcasts
   - Update button appearance (greyed out vs enabled) based on Control Panel state
   - Exception: Game Phase button always enabled (changes icon/label instead)
   
4. **FFF screen handling**:
   - Listen for `DisableAllButtons()` → Grey out entire Stream Deck
   - Listen for `EnableAllButtons()` → Restore all button states
   - Visual feedback: Show "FFF Active" overlay or dim all buttons
   
5. **Game Phase button state machine**:
   - Track current phase via `UpdateGamePhase()` broadcasts
   - Update button icon and label based on phase:
     - `HostIntro` → "Host Intro" icon
     - `PickPlayer` → "Pick Player" icon  
     - `LightsDown`/`RoundActive` → "Lights Down" icon (stays until round reset)
   - Handle button press based on current phase:
     - `HostIntro` → Call `PressGamePhase()` (triggers host intro)
     - `PickPlayer` → Call `PressGamePhase()` (opens FFF)
     - `LightsDown` → Call `PressGamePhase()` (lowers lights)
   
6. **Lifeline dynamic icon loading**:
   - On startup: Query `GetLifelineConfiguration()`
   - Load icons for assigned lifelines (50:50, Phone, ATA, Switch, etc.)
   - Show black button for unassigned lifelines (positions 1-4 with no config)
   - Update icons when `UpdateLifelineConfiguration()` received (settings changed)
   - Update button state when `UpdateLifelineState()` received (used/demo/disabled)
   
7. **Error handling & logging**:
   - Log all SignalR events and button presses
   - Handle connection failures gracefully
   - Show connection status indicator on Stream Deck (optional)
   
8. **Visual feedback system**:
   - Button press animation (brief flash)
   - State change transitions (smooth icon swap)
   - Disabled state styling (grey overlay)
   - Demo mode styling (pulsing for lifelines during Explain Game)

**Key Functions**:
```javascript
// plugin/app.js
class MillionaireGamePlugin {
    constructor() {
        this.connection = null;
        this.buttonStates = {};
    }

    async connect() {
        // SignalR connection to localhost:5000/hubs/streamdeck
    }

    async sendCommand(buttonId, action) {
        // Invoke hub method
    }

    updateButtonState(buttonId, state) {
        // Update icon, enable/disable
    }
}
```

### Phase 5: Testing & Polish (4-6 hours)

**Test Scenarios**:
1. **Initial Connection**:
   - Connect Stream Deck while game running
   - Verify all buttons sync to correct state (enabled/disabled)
   - Verify lifeline icons load correctly (including unassigned = black)
   - Verify Game Phase button shows correct state
   
2. **Button State Synchronization**:
   - Disable button in Control Panel → Verify Stream Deck button greys out
   - Enable button in Control Panel → Verify Stream Deck button becomes active
   - Test ALL 15 buttons for proper sync
   
3. **Game Phase Button Cycle**:
   - Start game (Game Reset) → Verify "Host Intro" appears
   - Press Host Intro → Verify button changes to "Pick Player"
   - Press Pick Player → Verify FFF opens AND all buttons disable
   - Close FFF → Verify button changes to "Lights Down" AND all buttons re-enable
   - Round Reset → Verify button returns to "Pick Player"
   - Game Reset → Verify button returns to "Host Intro"
   
4. **FFF Screen Disable Logic**:
   - Open FFF (Online or Offline) → Verify ALL Stream Deck buttons disabled/greyed
   - Try pressing any button → Verify no action taken
   - Close FFF → Verify all buttons re-enable to previous states
   
5. **Lifeline Behavior**:
   - Unassigned lifeline → Verify black button, always disabled
   - During Explain Game → Verify assigned lifelines enter "Demo" state (pulsing)
   - Use lifeline → Verify button changes to "Used" state (red X / crossed)
   - Change lifeline assignment in Settings → Verify Stream Deck updates icons
   
6. **Money Tree Button**:
   - During Explain Game → Verify demo mode active
   - During Q5 (Risk OFF) → Verify lock-in functionality available
   - During Q10 (Risk OFF) → Verify lock-in functionality available
   - Risk Mode ON → Verify button behavior matches Control Panel
   
7. **Rapid Button Presses**:
   - Press same button rapidly → Verify debounce (only one action)
   - Press different buttons rapidly → Verify all register correctly
   
8. **Connection Resilience**:
   - Disconnect Stream Deck → Reconnect → Verify state re-syncs
   - Restart game → Verify Stream Deck reconnects and syncs
   - Network interruption → Verify auto-reconnect works
   
9. **Edge Cases**:
   - Press disabled button → Verify no action taken
   - Change settings while Stream Deck connected → Verify updates propagate
   - Multiple game phase transitions → Verify button keeps up

**Time Estimate Summary**:
- Phase 1 (Plugin Skeleton): 2-3 hours
- Phase 2 (Game Integration + Full Button Sync): 6-8 hours *(increased for comprehensive state tracking)*
- Phase 3 (Icon Design): 5-6 hours
- Phase 4 (Plugin Logic + Sync Handling): 6-8 hours *(increased for FFF disable logic)*
- Phase 5 (Testing): 4-6 hours *(increased for comprehensive scenarios)*

**Total: 23-31 hours** (increased due to full button state synchronization complexity)

---

## 🎮 Button Layout Design

### Recommended 15-Key Layout (Stream Deck Regular)

```
┌─────────────────────────────────────────────────────────────┐
│  Row 1: Question & Answers                                  │
├─────────────────────────────────────────────────────────────┤
│  [Question] [  A  ] [  B  ] [  C  ] [  D  ]                │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│  Row 2: Lifelines & Reveal (Dynamic - respects Settings)   │
├─────────────────────────────────────────────────────────────┤
│  [LL 1 ] [LL 2 ] [LL 3 ] [LL 4 ] [Reveal]                  │
│  Unassigned lifelines: Black button, disabled               │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│  Row 3: Game Flow & Control                                 │
├─────────────────────────────────────────────────────────────┤
│  [Phase*] [Explain][Money Tree][Walk] [Closing]            │
│  *Multi-state: Host Intro → Pick Player → Lights Down      │
└─────────────────────────────────────────────────────────────┘
```

**Row 1 - Question & Answers** (Keys 0-4):
- **Key 0**: New Question (maps to F5)
  - Follows Control Panel button state (disabled = greyed out)
- **Keys 1-4**: Answer A, B, C, D (maps to F1-F4)
  - Follows Control Panel button state
  - Visual: Standard game colors, disabled state matches Control Panel

**Row 2 - Lifelines & Reveal** (Keys 5-9):
- **Keys 5-8**: Lifeline 1-4 (maps to F8-F11)
  - **Icons/labels**: Dynamically loaded based on Settings → Lifelines configuration
  - **Unassigned lifelines**: Black button, no action when pressed
  - **During Explain Game**: Lifelines active for "ping" demonstration
  - **During gameplay**: Follow Control Panel state (available/used/disabled)
  - Supported icons: 50:50, Phone, ATA, Switch, Double Dip, Ask Host, Plus One
- **Key 9**: Reveal Answer (maps to F6)
  - Follows Control Panel button state

**Row 3 - Game Flow & Control** (Keys 10-14):
- **Key 10: Game Phase Button** (multi-state, EXCEPTION to disable rule):
  - **State 1** - "Host Intro": Initial state → Press triggers host intro → Changes to "Pick Player"
  - **State 2** - "Pick Player": Opens FFF screen (btnFFFOffline/btnFFFOnline)
    - While FFF screen open: **ALL Stream Deck buttons disabled** (screen interface used instead)
    - When FFF closed: Button changes to "Lights Down"
  - **State 3** - "Lights Down": Lowers lights (maps to F7) → Stays active for rest of round
  - **Round Reset**: Button returns to "Pick Player"
  - **Game Reset**: Button returns to "Host Intro"
  
- **Key 11**: Explain Game
  - Active when Control Panel "Explain Game" button is active
  - Enables lifeline "ping" demonstrations
  
- **Key 12**: Money Tree / Lock In Safety Net
  - **During Explain Game**: Demo mode (same as Control Panel)
  - **During Q5/Q10**: Lock-in functionality if Risk Mode OFF and safety nets active
  - Follows Control Panel button state and behavior exactly
  
- **Key 13**: Walk Away (maps to End key)
  - Follows Control Panel button state
  
- **Key 14**: Game Closing
  - Ends the entire game (maps to btnClosing.PerformClick())
  - Follows Control Panel button state

**Game Phase Button State Machine**:
```
[Game Start] → "Host Intro" → Press → Host intro plays
      ↓
"Pick Player" → Press → FFF screen opens
      ↓
[FFF Active] → ALL BUTTONS DISABLED (screen control only)
      ↓
FFF Closed → "Lights Down" → Stays until Round Reset
      ↓
[Round Reset] → Returns to "Pick Player"
[Game Reset] → Returns to "Host Intro"
```

**Button State Synchronization Rules**:
1. **Default**: All buttons mirror Control Panel button state (enabled/disabled)
2. **Exception**: Game Phase button (Row 3, Key 10) changes independently based on game state
3. **During FFF**: ALL buttons disabled regardless of Control Panel state
4. **Unassigned Lifelines**: Always black/disabled, even during Explain Game
5. **Explain Game Active**: Assigned lifelines become active for demonstration "pings"

### Alternative 32-Key Layout (Stream Deck XL)
Adds:
- Level Up/Down buttons
- FFF Online/Offline toggle
- Host message quick buttons
- Scene switching (for OBS integration)

---

## 📊 Button State Management

### State Machine per Button

```
┌────────────────────────────────────────────────────────────┐
│  Button State Lifecycle                                    │
├────────────────────────────────────────────────────────────┤
│                                                             │
│  DISABLED (Grey)                                           │
│       ↓                                                     │
│  [Game state change]                                       │
│       ↓                                                     │
│  AVAILABLE (Green) ←───────────┐                          │
│       ↓                         │                          │
│  [Button pressed]               │                          │
│       ↓                         │                          │
│  ACTIVE (Yellow)                │                          │
│       ↓                         │                          │
│  [Action completes]             │                          │
│       ↓                         │                          │
│  USED (Red) OR ─────────────────┘                          │
│                                                             │
└────────────────────────────────────────────────────────────┘
```

### State Colors & Icons

| State      | Color  | Icon Style           | Description                    |
|------------|--------|----------------------|--------------------------------|
| Disabled   | Grey   | Desaturated          | Button unavailable             |
| Available  | Green  | Full color, pulsing  | Ready to press                 |
| Active     | Yellow | Animated             | Action in progress             |
| Used       | Red    | Crossed out          | Depleted (lifelines only)      |
| Standby    | Orange | Dimmed               | Waiting (after Lights Down)    |

---

## 🔒 Security Considerations

### Connection Security
- **Local Only**: Stream Deck connects to localhost:5000
- **Optional Authentication**: API key in plugin config
- **CORS Policy**: Restrict to localhost origin

### Command Validation
- **Server-Side Validation**: All button commands validated in hub
- **State Checks**: Commands rejected if button disabled
- **Rate Limiting**: Prevent rapid button spam (100ms debounce)

---

## 🧪 Testing Strategy

### Unit Tests (Game Side)
- StreamDeckHub command handling
- Button state broadcasting
- State synchronization logic

### Integration Tests
- Plugin connects to game
- Button presses trigger correct actions
- State updates reach plugin
- Reconnection after disconnect

### Manual Tests
- Full game playthrough using only Stream Deck
- Lifeline activation and state changes
- Answer selection and reveal
- Edge cases (rapid presses, disconnect during action)

---

## 📚 Documentation Requirements

### User Documentation
1. **Setup Guide**: Installing plugin, connecting to game
2. **Button Reference**: Complete button layout with descriptions
3. **Troubleshooting**: Common issues and solutions

### Developer Documentation
1. **Plugin Architecture**: Code structure explanation
2. **Hub API Reference**: Available commands and events
3. **Adding New Buttons**: How to extend functionality

---

## 🚧 Known Challenges & Mitigations

### Challenge 1: State Synchronization
**Issue**: Game state changes while Stream Deck disconnected  
**Mitigation**: Send full state snapshot on reconnection

### Challenge 2: Button Press Latency
**Issue**: WebSocket round-trip adds delay  
**Mitigation**: 
- Immediate visual feedback on plugin side
- Optimized SignalR configuration
- Local network (no internet latency)

### Challenge 3: Plugin Distribution
**Issue**: Users must manually install plugin  
**Mitigation**: 
- Clear installation instructions
- Automated installer script (PowerShell)
- Consider Elgato Marketplace submission (post-v1.2)

### Challenge 4: Multiple Stream Deck Devices
**Issue**: Some users have multiple Stream Decks  
**Mitigation**: Plugin works per-device, no conflicts

---

## 📈 Future Enhancements (Post-v1.2)

### v1.3 Features
- **Multi-Page Support**: Switch between Answer/Lifeline/Control pages
- **Rotary Encoder**: Volume control (Stream Deck+)
- **Touch Screen**: Display game info (Stream Deck Neo)
- **Scene Switching**: OBS/vMix integration

### v1.4 Features
- **Profile Switching**: Different layouts per game mode
- **Custom Commands**: User-defined macro buttons
- **LED Patterns**: Animated feedback (breathing, flashing)

---

## 💰 Cost Analysis

### Software (Free)
- Stream Deck SDK: Free
- Node.js: Free
- Development tools: Free (VS Code, etc.)

### Hardware (One-Time Purchase)
- Stream Deck Regular (15 keys): $149.99
- Stream Deck MK.2 (15 keys): $149.99
- Stream Deck XL (32 keys): $249.99
- Stream Deck + (8 keys + 4 encoders): $199.99

**Recommendation**: Start with Stream Deck Regular (15 keys) - sufficient for all core functions

---

## ⏱️ Time Estimate Breakdown

| Phase                      | Estimated Time | Priority |
|----------------------------|----------------|----------|
| Plugin Skeleton            | 2-3 hours      | High     |
| Game SignalR Hub           | 3-4 hours      | High     |
| Icon Design                | 4-5 hours      | Medium   |
| Plugin Logic               | 3-4 hours      | High     |
| Testing & Polish           | 2-3 hours      | High     |
| Documentation              | 1-2 hours      | Medium   |
| **Total**                  | **15-21 hours**| -        |

**Realistic Timeline**: 3-4 days of focused development

---

## ✅ Success Criteria

- [ ] Stream Deck plugin connects to game via SignalR
- [ ] All 15 core buttons functional (answers, lifelines, controls)
- [ ] Button states update in real-time with game state
- [ ] LED feedback matches button availability
- [ ] Complete game playthrough possible using only Stream Deck
- [ ] Plugin survives reconnection after game restart
- [ ] User documentation complete
- [ ] No build warnings or errors
- [ ] Plugin installable via .streamDeckPlugin file

---

## 📞 Support Resources

### Official Documentation
- Stream Deck SDK: https://developer.elgato.com/documentation/stream-deck/
- SignalR TypeScript Client: https://learn.microsoft.com/en-us/aspnet/core/signalr/javascript-client

### Community Resources
- Stream Deck Discord: https://discord.gg/elgato
- Stream Deck Reddit: r/elgato

### Example Plugins
- Philips Hue Plugin (open source)
- OBS Studio Plugin (open source)

---

## 🎯 Next Steps

### Before Starting
1. ✅ Review this plan with user
2. ✅ Stream Deck hardware available (Regular - 15 keys)
3. ⬜ Download Stream Deck SDK and software
4. ⬜ Set up Node.js development environment

### Phase 1 Kickoff
1. ⬜ Create plugin project structure
2. ⬜ Test basic Stream Deck button press
3. ⬜ Verify SignalR connection from JavaScript
4. ⬜ Implement first button (Answer A)

---

**Document Created**: December 31, 2025  
**Purpose**: Comprehensive plan for Elgato Stream Deck integration  
**Target Release**: v1.2  
**Next Action**: User review and approval
