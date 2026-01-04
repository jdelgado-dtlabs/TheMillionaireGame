# Stream Deck Integration Implementation Plan

**Branch**: `feature/streamdeck-integration`  
**Status**: Planning Phase  
**Priority**: High (Final v1.0 Feature)  
**Created**: January 3, 2026  
**Target Device**: Elgato Stream Deck (6-button models - Module 6, Mini)

---

## Overview

Integrate Elgato Stream Deck as a physical control interface for the **host** (show facilitator) to control answer lock-in and reveal during gameplay. This provides tactile control with visual feedback, allowing the host to build dramatic tension before revealing results.

### Key Terminology
- **User**: Application operator (Control Panel)
- **Host**: Show facilitator (Stream Deck controller)
- **Player/Contestant**: Person answering questions
- **Audience**: Viewers/participants

---

## Requirements

### Functional Requirements

#### FR-1: Answer Lock-In Control
- Host can lock in contestant's answer using Stream Deck buttons
- Buttons A, B, C, D correspond to answer choices
- Lock-in triggers same logic as Control Panel's final answer button
- Lock-in is only active when game state allows answer submission

#### FR-2: Dynamic Visual Feedback
- **Dynamic Button (Row 1, Position 1)**:
  - **Blank/Idle**: No answer locked in yet
  - **Green Check**: Correct answer locked in (before reveal)
  - **Red X**: Incorrect answer locked in (before reveal)
  - **Cleared**: After reveal, returns to blank
- Provides host with immediate private feedback on correctness
- Does NOT trigger automatic reveal

#### FR-3: Reveal Control
- Host can trigger answer reveal using dedicated button
- Reveal button triggers same logic as Control Panel's reveal button
- Only active when answer is locked in and pending reveal

#### FR-4: State Synchronization
- Stream Deck buttons reflect game state
- Buttons disabled/enabled based on current game phase
- Control Panel actions sync with Stream Deck state
- Stream Deck actions sync with Control Panel state

#### FR-5: Visual Button States
- **Enabled**: Bright, full color (action available)
- **Disabled**: Dimmed/grayed out (action unavailable)
- **Active**: Highlighted (currently selected answer)
- **Locked**: Special indicator (answer locked in)

### Non-Functional Requirements

#### NFR-1: Performance
- Button press response time < 100ms
- No lag in visual feedback updates
- No impact on game rendering performance

#### NFR-2: Reliability
- Graceful handling of Stream Deck disconnect
- Automatic reconnection on device plug-in
- Fallback to Control Panel if Stream Deck unavailable

#### NFR-3: Usability
- Intuitive button layout matching game flow
- Clear visual states with high contrast
- Consistent behavior with Control Panel

#### NFR-4: Maintainability
- Decoupled from core game logic
- Easy to disable/enable via settings
- Logging for debugging

---

## Button Layout

### Physical Layout (6-Button Stream Deck)
```
┌─────────┬─────────┬─────────┐
│ Dynamic │    A    │    B    │  Row 1
├─────────┼─────────┼─────────┤
│ Reveal  │    C    │    D    │  Row 2
└─────────┴─────────┴─────────┘
```

### Button Mappings

| Position | Label | Function | Control Panel Equivalent |
|----------|-------|----------|--------------------------|
| **Row 1, Col 1** | Dynamic | Visual feedback only | N/A |
| **Row 1, Col 2** | A | Lock in Answer A | Answer A + Final Answer |
| **Row 1, Col 3** | B | Lock in Answer B | Answer B + Final Answer |
| **Row 2, Col 1** | Reveal | Reveal correct answer | Reveal Answer button |
| **Row 2, Col 2** | C | Lock in Answer C | Answer C + Final Answer |
| **Row 2, Col 3** | D | Lock in Answer D | Answer D + Final Answer |

---

## User Flow Example

### Scenario: Contestant Answers Question Incorrectly

1. **Question Displayed**: All answer buttons A/B/C/D enabled (bright), Dynamic blank, Reveal disabled (dim)
2. **Contestant Selects C**: Host recognizes verbal selection, but does NOT press button yet
3. **Host Asks Confirmation**: "Is that your final answer?"
4. **Contestant Confirms**: "Yes, C is my final answer"
5. **Host Presses C Button**: 
   - C button highlights/locks
   - Dynamic button shows **Red X** (incorrect)
   - Reveal button becomes enabled (bright)
   - Other answer buttons disable
6. **Host Builds Tension**: "Let's see if you're right..." (pauses for dramatic effect)
7. **Host Presses Reveal**: 
   - TV screen reveals correct answer (D)
   - Contestant loses
   - Dynamic button clears to blank
   - All buttons disable
8. **Host Consoles Contestant**: "I'm sorry, the correct answer was D..."

### Scenario: Contestant Answers Correctly

Same flow, but:
- Step 5: Dynamic button shows **Green Check** (correct)
- Step 6: Host builds excitement: "You look confident..."
- Step 7: TV screen reveals correct answer (C), contestant wins level
- Step 8: Host congratulates: "That's right! You've won [prize]!"

---

## Technical Architecture

### Components

#### 1. StreamDeckService
**Purpose**: Manage Stream Deck connection, button events, and state updates  
**Location**: `src/MillionaireGame/Services/StreamDeckService.cs`

**Responsibilities**:
- Initialize Stream Deck connection
- Register button press handlers
- Update button images/states
- Handle disconnect/reconnect
- Expose events for button presses

**Key Methods**:
```csharp
public class StreamDeckService : IDisposable
{
    // Lifecycle
    public bool Initialize();
    public void Shutdown();
    
    // Button State Management
    public void SetButtonImage(int row, int col, Bitmap image);
    public void EnableButton(int row, int col, bool enabled);
    public void ClearButton(int row, int col);
    public void SetButtonColor(int row, int col, Color color);
    
    // Answer Buttons (A, B, C, D)
    public void SetAnswerButtonState(char answer, ButtonState state);
    public void HighlightAnswer(char answer);
    public void DisableAllAnswers();
    
    // Dynamic Button (Feedback)
    public void ShowCorrectIndicator();
    public void ShowIncorrectIndicator();
    public void ClearDynamicButton();
    
    // Reveal Button
    public void EnableReveal(bool enabled);
    
    // Events
    public event EventHandler<char> AnswerButtonPressed;
    public event EventHandler RevealButtonPressed;
    public event EventHandler DeviceConnected;
    public event EventHandler DeviceDisconnected;
}
```

#### 2. StreamDeckIntegration Class
**Purpose**: Bridge between StreamDeckService and game logic  
**Location**: `src/MillionaireGame/Hosting/StreamDeckIntegration.cs`

**Responsibilities**:
- Subscribe to game state changes
- Translate game events to button states
- Handle Stream Deck button events
- Coordinate with ControlPanelForm

**Key Methods**:
```csharp
public class StreamDeckIntegration
{
    private readonly StreamDeckService _streamDeck;
    private readonly GameService _gameService;
    private char? _lockedAnswer;
    private bool _isCorrectAnswer;
    
    public void OnQuestionDisplayed();
    public void OnAnswerSelected(char answer);
    public void OnAnswerLockedIn(char answer);
    public void OnAnswerRevealed();
    public void OnQuestionEnd();
    
    private void HandleAnswerButtonPressed(char answer);
    private void HandleRevealButtonPressed();
}
```

#### 3. ControlPanelForm Integration
**Purpose**: Synchronize Control Panel with Stream Deck  
**Location**: `src/MillionaireGame/Forms/ControlPanelForm.cs`

**Changes**:
- Add StreamDeckIntegration instance
- Subscribe to Stream Deck events
- Update button states when Control Panel actions occur
- Disable duplicate actions (prevent double-triggering)

#### 4. Button Image Assets
**Purpose**: Visual representations for button states  
**Location**: `src/MillionaireGame/lib/image/streamdeck/` ✅ USER PROVIDED

**Available Images** (72x72 pixels for Stream Deck):
- `blank.png` - Empty/idle state (also used for disabled buttons)
- `correct.png` - Correct answer indicator (green check)
- `wrong.png` - Incorrect answer indicator (red X)
- `answer-reveal.png` - Reveal button
- `answer-a.png` - Answer A button (enabled)
- `answer-a-lock.png` - Answer A locked/highlighted
- `answer-b.png` - Answer B button (enabled)
- `answer-b-lock.png` - Answer B locked/highlighted
- `answer-c.png` - Answer C button (enabled)
- `answer-c-lock.png` - Answer C locked/highlighted
- `answer-d.png` - Answer D button (enabled)
- `answer-d-lock.png` - Answer D locked/highlighted

**Button State Logic**:
- **Enabled**: Show answer/reveal button image
- **Disabled**: Show `blank.png`
- **Locked**: Show `-lock` variant
- **Correct Feedback**: Show `correct.png` on dynamic button
- **Wrong Feedback**: Show `wrong.png` on dynamic button

---

## State Machine

### Button States

#### Answer Buttons (A, B, C, D)
```
State: Disabled (Question not active)
  └─> Event: Question displayed
      └─> State: Enabled
          ├─> Event: Answer button pressed
          │   └─> State: Locked (this button)
          │       └─> Event: Reveal pressed
          │           └─> State: Disabled
          └─> Event: Other answer locked
              └─> State: Disabled
```

#### Dynamic Button
```
State: Blank (Idle)
  └─> Event: Answer locked in
      ├─> State: Green Check (if correct)
      │   └─> Event: Reveal pressed
      │       └─> State: Blank
      └─> State: Red X (if incorrect)
          └─> Event: Reveal pressed
              └─> State: Blank
```

#### Reveal Button
```
State: Disabled (No answer locked)
  └─> Event: Answer locked in
      └─> State: Enabled
          └─> Event: Reveal pressed
              └─> State: Disabled
```

---

## Implementation Phases

### Phase 1: Foundation (Days 1-2)
**Goal**: Set up Stream Deck SDK and basic connectivity

- [ ] **Task 1.1**: Research Stream Deck SDK options for .NET
  - Official Elgato SDK
  - StreamDeck-Tools library (community)
  - OpenStreamDeck (open-source alternative)
  - Choose based on: ease of use, maintenance, licensing
  
- [ ] **Task 1.2**: Add NuGet package to project
  - Install chosen SDK
  - Update project references
  - Verify build success

- [ ] **Task 1.3**: Create StreamDeckService skeleton
  - Basic class structure
  - Connection initialization
  - Device detection
  - Disconnect handling
  - Logging integration (GameConsole)

- [ ] **Task 1.4**: Test device connection
  - Detect Stream Deck on startup
  - Log connection status
  - Handle graceful failure if not connected

**Validation**: Stream Deck detected and logged in GameConsole

---

### Phase 2: Button Rendering (Days 3-4)
**Goal**: Display images on Stream Deck buttons

- [x] **Task 2.1**: Create button image assets ✅ USER PROVIDED
  - 12 button images (72x72px)
  - High contrast design
  - Match game theme colors
  - Located: `lib/image/streamdeck/`

- [ ] **Task 2.2**: Implement image loading
  - Load images from `lib/image/streamdeck/` directory
  - Create helper methods to load by filename
  - Handle missing images gracefully (fallback to blank.png)

- [ ] **Task 2.3**: Implement SetButtonImage()
  - Convert Bitmap to Stream Deck format
  - Send to specific button (row, col)
  - Handle errors

- [ ] **Task 2.4**: Test static button display
  - Display all enabled answer buttons on startup
  - Display blank dynamic button
  - Display disabled reveal button
  - Verify visual appearance on physical device

**Validation**: All buttons display correct images on Stream Deck

---

### Phase 3: Button Input (Days 5-6)
**Goal**: Capture button presses and raise events

- [ ] **Task 3.1**: Register button press handlers
  - Subscribe to SDK button events
  - Map physical button positions to logical buttons
  - Implement debouncing if needed

- [ ] **Task 3.2**: Implement AnswerButtonPressed event
  - Raise event with char parameter ('A', 'B', 'C', 'D')
  - Log button press (GameConsole.Info)
  - Ignore if button disabled

- [ ] **Task 3.3**: Implement RevealButtonPressed event
  - Raise event
  - Log button press
  - Ignore if button disabled

- [ ] **Task 3.4**: Test button input
  - Press each button, verify console logs
  - Verify event parameters correct
  - Test disabled button behavior

**Validation**: Button presses logged correctly, events fire

---

### Phase 4: Game Integration (Days 7-9)
**Goal**: Connect Stream Deck to game logic

- [ ] **Task 4.1**: Create StreamDeckIntegration class
  - Initialize with dependencies (StreamDeckService, GameService)
  - Subscribe to game events
  - Subscribe to Stream Deck events

- [ ] **Task 4.2**: Implement OnQuestionDisplayed()
  - Enable answer buttons (A, B, C, D)
  - Clear dynamic button (blank)
  - Disable reveal button
  - Reset locked answer state

- [ ] **Task 4.3**: Implement HandleAnswerButtonPressed()
  - Store locked answer
  - Determine if answer is correct
  - Highlight locked answer button
  - Disable other answer buttons
  - Update dynamic button (green check / red X)
  - Enable reveal button
  - Trigger Control Panel answer lock-in logic

- [ ] **Task 4.4**: Implement HandleRevealButtonPressed()
  - Trigger Control Panel reveal logic
  - Clear dynamic button (blank)
  - Disable all answer buttons
  - Disable reveal button

- [ ] **Task 4.5**: Integrate with ControlPanelForm
  - Add StreamDeckIntegration instance
  - Initialize on form load (if Stream Deck connected)
  - Subscribe to Stream Deck events
  - Pass events to existing game logic
  - Prevent duplicate actions from Control Panel

**Validation**: Stream Deck controls game, actions execute correctly

---

### Phase 5: Bidirectional Sync (Days 10-11)
**Goal**: Control Panel actions update Stream Deck

- [ ] **Task 5.1**: Sync Control Panel answer selection
  - When user clicks answer on Control Panel
  - Update corresponding Stream Deck button state
  - Maintain consistency

- [ ] **Task 5.2**: Sync Control Panel final answer
  - When user clicks Final Answer on Control Panel
  - Lock answer button on Stream Deck
  - Update dynamic button
  - Enable reveal button

- [ ] **Task 5.3**: Sync Control Panel reveal
  - When user clicks Reveal on Control Panel
  - Clear Stream Deck dynamic button
  - Disable all buttons

- [ ] **Task 5.4**: Handle game state changes
  - Next question: Reset Stream Deck
  - Game end: Disable all buttons
  - Lifeline used: No Stream Deck impact (lifelines controlled by user only)

**Validation**: Control Panel and Stream Deck stay in sync

---

### Phase 6: Polish & Settings (Days 12-13)
**Goal**: User experience refinements

- [ ] **Task 6.1**: Add settings toggle
  - Options screen: "Enable Stream Deck Integration"
  - Persist setting
  - Only initialize if enabled

- [ ] **Task 6.2**: Implement disconnect handling
  - Monitor device connection status
  - Log disconnect (GameConsole.Warn)
  - Attempt auto-reconnect (5-second intervals)
  - Notify user on Control Panel (status indicator)

- [ ] **Task 6.3**: Add brightness control
  - Setting for Stream Deck brightness (0-100%)
  - Apply on connection

- [ ] **Task 6.4**: Optimize performance
  - Batch image updates if possible
  - Minimize update frequency
  - Profile CPU/memory usage

**Validation**: Settings work, disconnect handled gracefully

---

### Phase 7: Testing & Documentation (Days 14-15)
**Goal**: Ensure reliability and document usage

- [ ] **Task 7.1**: Comprehensive testing
  - Test all game scenarios
  - Test with/without Stream Deck connected
  - Test disconnect during game
  - Test rapid button presses
  - Test edge cases (answer change before reveal, etc.)

- [ ] **Task 7.2**: Update copilot-instructions.md
  - Add Stream Deck setup notes
  - Document host control workflow

- [ ] **Task 7.3**: Create user documentation
  - Add to GITHUB_WIKI_DOCUMENTATION_PLAN.md (Section 4.6)
  - Write setup guide
  - Include troubleshooting
  - Add screenshots/photos

- [ ] **Task 7.4**: Update CHANGELOG.md
  - Document new feature
  - List requirements

- [ ] **Task 7.5**: Update V1.0_RELEASE_STATUS.md
  - Mark Stream Deck integration complete
  - Check off v1.0 milestone

**Validation**: All tests pass, documentation complete

---

## SDK Evaluation

### Option 1: Official Elgato Stream Deck SDK
- **Pros**: Official support, stable, comprehensive
- **Cons**: May require C++ interop, licensing?
- **Link**: https://developer.elgato.com/documentation/stream-deck/sdk/overview/

### Option 2: StreamDeck-Tools (.NET Library)
- **Pros**: Native C#, NuGet available, active community
- **Cons**: Third-party, may lag behind SDK updates
- **Link**: https://github.com/FritzAndFriends/StreamDeckToolkit

### Option 3: OpenStreamDeck
- **Pros**: Open-source, full control
- **Cons**: Less documentation, may require more work
- **Link**: Research needed

**Recommended**: Start with StreamDeck-Tools for rapid .NET integration, fallback to official SDK if limitations found

---

## Dependencies

### NuGet Packages (To Be Determined)
- StreamDeck-Tools (or chosen SDK)
- System.Drawing.Common (if not already referenced)

### External Requirements
- Elgato Stream Deck device (6-button model)
- Elgato Stream Deck software (for initial device setup/firmware)
- Windows 10/11 (device drivers)

---

## Configuration

### App.config Settings
```xml
<appSettings>
  <add key="StreamDeck.Enabled" value="true" />
  <add key="StreamDeck.Brightness" value="80" />
  <add key="StreamDeck.AutoReconnect" value="true" />
  <add key="StreamDeck.ReconnectInterval" value="5000" />
</appSettings>
```

### User Settings (ApplicationSettings)
- `bool StreamDeckEnabled`
- `int StreamDeckBrightness` (0-100)
- `bool StreamDeckAutoReconnect`

---

## Error Handling

### Device Not Found
- **When**: Application starts, Stream Deck not connected
- **Action**: Log warning, continue without Stream Deck, check setting
- **User Feedback**: Status indicator on Control Panel (optional)

### Device Disconnected During Game
- **When**: Stream Deck unplugged mid-game
- **Action**: Log warning, disable Stream Deck features, continue game with Control Panel
- **User Feedback**: GameConsole.Warn message

### SDK Error
- **When**: Unexpected SDK exception
- **Action**: Log error, disable Stream Deck, allow game to continue
- **User Feedback**: GameConsole.Error message

### Button Press Error
- **When**: Button pressed in invalid state
- **Action**: Log debug message, ignore press
- **User Feedback**: None (silent fail)

---

## Testing Strategy

### Unit Tests
- StreamDeckService button state management
- StreamDeckIntegration event handling
- Button image loading

### Integration Tests
- Control Panel + Stream Deck synchronization
- Game state changes reflected on Stream Deck
- Stream Deck actions trigger game logic

### Manual Tests
- Physical button press responsiveness
- Visual feedback accuracy
- All game scenarios (win, lose, walk away)
- Disconnect/reconnect scenarios
- Settings changes

### User Acceptance Tests
- Host controls game using Stream Deck only
- Dramatic tension build (lock + wait + reveal)
- Error recovery (wrong button, rapid presses)

---

## Known Limitations

### Out of Scope for v1.0
- **Custom Button Layouts**: Fixed 6-button layout only
- **Multiple Stream Decks**: Single device only
- **Button Customization**: No user-defined button images
- **Lifeline Control**: Host cannot use lifelines (user-only)
- **Question Navigation**: Host cannot skip questions
- **Game Start/End**: User controls game lifecycle

### Future Enhancements (Post-v1.0)
- Stream Deck XL support (15 buttons)
- Custom button image upload
- Configurable button mappings
- Additional host controls (e.g., lifeline shortcuts)
- Audience interaction via Stream Deck

---

## Success Criteria

### Functional
- ✅ Host can lock in answers using A/B/C/D buttons
- ✅ Dynamic button shows correct/incorrect feedback before reveal
- ✅ Reveal button triggers answer reveal
- ✅ Buttons disabled/enabled based on game state
- ✅ Control Panel and Stream Deck stay synchronized

### Non-Functional
- ✅ Button response < 100ms
- ✅ No game performance impact
- ✅ Graceful handling of device disconnect
- ✅ Clear logging for debugging

### User Experience
- ✅ Host can build dramatic tension (lock + pause + reveal)
- ✅ Visual feedback is clear and immediate
- ✅ Setup process is straightforward
- ✅ Documentation is complete

---

## Risks & Mitigations

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| SDK compatibility issues | High | Medium | Evaluate multiple SDKs, have fallback |
| Device driver problems | High | Low | Document driver requirements, test on multiple PCs |
| Performance degradation | Medium | Low | Profile early, optimize image updates |
| Sync issues (Control Panel vs Stream Deck) | Medium | Medium | Careful event handling, debouncing, testing |
| User confusion (duplicate controls) | Low | Medium | Clear documentation, visual indicators |
| Stream Deck unavailable at runtime | Low | Medium | Graceful degradation, logging |

---

## Timeline

**Total Estimated Time**: 15 days (assumes part-time development)

| Phase | Duration | Key Deliverables |
|-------|----------|------------------|
| Phase 1: Foundation | 2 days | SDK integrated, device connected |
| Phase 2: Button Rendering | 2 days | Images display on Stream Deck |
| Phase 3: Button Input | 2 days | Button presses captured |
| Phase 4: Game Integration | 3 days | Stream Deck controls game |
| Phase 5: Bidirectional Sync | 2 days | Control Panel syncs with Stream Deck |
| Phase 6: Polish & Settings | 2 days | Settings, disconnect handling |
| Phase 7: Testing & Documentation | 2 days | Complete testing, docs ready |

**Target Completion**: Mid-January 2026 (before v1.0 release)

---

## Next Steps

1. **Immediate**: Research and select Stream Deck SDK (.NET library)
2. **Day 1**: Install SDK, create StreamDeckService, test connection
3. **Day 2**: Design and create button image assets
4. **Day 3**: Implement button rendering, display static images
5. **Day 4**: Continue implementation following phase plan

---

## Notes

- This is the final major feature for v1.0 release
- After completion, focus shifts to documentation and user guides
- Stream Deck is OPTIONAL - game must work fully without it
- Host controls are LIMITED to answer lock-in and reveal - user retains primary control
- All Stream Deck actions must respect game state and rules (no cheating)

---

## References

- Project Instructions: `.github/copilot-instructions.md`
- Control Panel Logic: `src/MillionaireGame/Forms/ControlPanelForm.cs`
- Game Service: `src/MillionaireGame.Core/Services/GameService.cs`
- Terminology: Host = show facilitator, User = application operator
