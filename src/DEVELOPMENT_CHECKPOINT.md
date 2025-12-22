# Development Checkpoint - v0.3-2512
**Date**: December 22, 2025  
**Version**: 0.3-2512  
**Branch**: master-csharp  
**Author**: jdelgado-dtlabs

---

## Session Summary

### Latest Session (Repository Management) - December 22, 2025

#### Sound Files Added to Repository
- ✅ Default Soundpack Added
  - 120 MP3 sound files for complete game audio
  - soundpack.xml configuration file
  - README.md with soundpack documentation
  - Total: 123 files in src/MillionaireGame/lib/sounds/Default/
  - Files now tracked as part of complete package distribution

#### Repository Cleanup
- ✅ .gitignore Optimization
  - Removed .github/copilot-instructions.md from git tracking (keeping local file)
  - Simplified src/.gitignore to only src-specific ignores
  - src/.gitignore now only contains: config.xml, sql.xml (runtime configs)
  - Removed redundant patterns already covered by root .gitignore
  - Sound files, textures, and images now properly tracked

#### Git Operations
- ✅ Commits:
  - 00e1ecb: Remove .github/copilot-instructions.md from tracking
  - aa6f719: Add sound files to repository and clean up src/.gitignore
- ✅ Pushed to remote: master-csharp branch
- ✅ Total additions: 121 files, 211 insertions

### Previous Session (Settings & Bug Fixes) - December 22, 2025

#### Settings System Improvements
- ✅ Monitor Selection Enhancement
  - Enhanced dropdown format: "ID:Manufacturer:Model (Resolution)"
  - WMI queries via System.Management to extract monitor metadata
  - Uses WmiMonitorID class for UserFriendlyName and ManufacturerName
  - Handles cases where manufacturer/model unavailable (falls back to basic format)

- ✅ Full Screen & Auto Show Checkboxes
  - Full Screen checkbox has immediate effect (applies full-screen on toggle)
  - Auto Show checkbox behavior at startup (shows screens automatically)
  - Dropdowns disable when Full Screen is enabled (grey out)
  - Event handlers: chkFullScreenHost_CheckedChanged, chkFullScreenGuest_CheckedChanged, chkFullScreenTV_CheckedChanged

- ✅ Settings Persistence to XML
  - Fixed Program.cs to use XML mode: `new ApplicationSettingsManager()` without connection string
  - Removed database migration code
  - Synchronous SaveSettings() instead of SaveSettingsAsync().Wait() to prevent deadlocks
  - ApplicationSettingsManager instance properly passed to OptionsDialog
  - Settings load from config.xml on startup

#### Sound System Cleanup
- ✅ Deprecated Properties Removed
  - Removed ~160 Sound* properties from ApplicationSettings.cs
  - Properties removed: SoundOpening, SoundCommercialIn, SoundLifeline*, SoundFFF*, SoundQ1-Q15 variants
  - Only SelectedSoundPack property retained
  - Removed LoadSoundsFromSettingsLegacy method from SoundService.cs
  - Updated LoadSoundsFromSettings to use soundpack system exclusively
  - No fallback to legacy properties (logs error if soundpack fails)

- ✅ Soundpack System
  - Primary: SoundPackManager loads from lib/sounds/{PackName}/soundpack.xml
  - Default pack at lib/sounds/Default/soundpack.xml fully operational
  - Cleaner config.xml with only SelectedSoundPack for sounds

#### Bug Fixes
- ✅ Money Tree Reset
  - Added UpdateMoneyTreeOnScreens(0) after StopMoneyTreeDemo() in btnLightsDown_Click
  - Money tree now properly resets to level 0 when starting new player's game
  - Prevents Q15 position from remaining after demo animation

- ✅ ATA Statistics Display
  - Removed DrawATAPreview() call that was triggering on all 4 answers revealed
  - ATA preview now only shows when _showATA flag is true (lifeline activated)
  - Fixed unwanted ATA statistics window appearing on host screen

- ✅ Show Correct Answer to Host
  - Fixed ShowCorrectAnswerToHost to set _isRevealing = true
  - Added immediate effect checkbox handler: chkCorrectAnswer_CheckedChanged
  - Checkbox only works when _answerRevealStep == 5 (all answers revealed)
  - Toggle show/hide correct answer at any time after full reveal
  - Updated interface signatures to accept nullable string for hide functionality

#### Files Modified
**Settings System:**
- `MillionaireGame.Core/Settings/ApplicationSettings.cs` - Removed deprecated Sound* properties
- `MillionaireGame/Program.cs` - Fixed to XML-only mode
- `MillionaireGame/Forms/Options/OptionsDialog.cs` - Added checkbox event handlers, receives ApplicationSettingsManager
- `MillionaireGame/Forms/ControlPanelForm.cs` - Passes ApplicationSettingsManager to OptionsDialog

**Sound System:**
- `MillionaireGame/Services/SoundService.cs` - Removed legacy loading, soundpack-only
- `MillionaireGame/Services/SoundPackManager.cs` - Primary sound loading system

**Bug Fixes:**
- `MillionaireGame/Forms/ControlPanelForm.cs` - Money tree reset, checkbox handler
- `MillionaireGame/Forms/HostScreenForm.cs` - Fixed correct answer display, removed ATA preview
- `MillionaireGame/Services/ScreenUpdateService.cs` - Updated interface for nullable string
- `MillionaireGame/Forms/TVScreenForm.cs` - Updated interface implementation
- `MillionaireGame/Forms/TVScreenFormScalable.cs` - Updated interface implementation
- `MillionaireGame/Forms/GuestScreenForm.cs` - Updated interface implementation

### Previous Session (v0.3-2512) - December 20-21, 2025

#### Money Tree Settings System
- ✅ Complete settings UI implementation
  - Descending money tree display (Q15→Q1)
  - 15 prize input fields with validation
  - Safety net checkboxes at Q5 and Q10
  - Dual currency support (Currency 1 and Currency 2)
  - Per-question currency selector (dropdown showing "1" or "2")
  - Enable/disable toggle for Currency 2
  - Currency symbol selection: $, €, £, ¥, or custom text
  - Currency position control (prefix/suffix for each currency)

- ✅ MoneyTreeSettings Model
  - Properties: 15 level values, SafetyNet1/2, Currency1/2, CurrencyAtSuffix1/2
  - LevelCurrencies array (15 elements) for per-level currency assignment
  - Currency2Enabled flag to control dual currency mode
  - Helper methods: GetLevelValue(), FormatMoney(), IsSafetyNet()

- ✅ MoneyTreeService
  - XML persistence to AppData/MillionaireGame/tree.xml
  - GetFormattedValue(level) with dual currency support
  - LoadSettings() and SaveSettings() with error handling
  - GetWrongValue() and GetDropValue() for safety net calculations

- ✅ Game Integration
  - Prize displays reference money tree configuration
  - Dynamic per-level currency selection in GetFormattedValue()
  - Risk mode button reflects safety net configuration
  - Four button states:
    * Yellow "Activate Risk Mode" (both safety nets enabled)
    * Blue "RISK MODE: 5" (Q5 safety net disabled)
    * Blue "RISK MODE: 10" (Q10 safety net disabled)
    * Red "RISK MODE: ON" (both disabled, unclickable)
  - UpdateRiskModeButton() called on level/mode changes

- ✅ Settings Dialog Improvements
  - Removed Apply button for cleaner two-button layout
  - OK button saves all settings and fires SettingsApplied event
  - Cancel button checks for unsaved changes and shows warning
  - Real-time updates: control panel immediately reflects settings changes
  - Event-driven architecture: SettingsApplied event subscribed by ControlPanelForm
  - Change tracking with _hasChanges flag

#### Technical Architecture
- Event-driven settings reload pattern
- Dynamic WinForms control creation (30+ controls in InitializeMoneyTreeTab)
- Clean separation: settings persistence vs. game logic vs. UI display
- Immediate visual feedback without dialog closure

---

## Previous Session Summary

### Completed Features (v0.2-2512)

#### Control Panel UI
- ✅ Complete game flow management
- ✅ Progressive answer reveal system (Question → A → B → C → D)
- ✅ Question button routes based on `_answerRevealStep` (0-5 state machine)
- ✅ Answer textboxes (txtA, txtB, txtC, txtD) reveal progressively
- ✅ "Show Correct Answer to Host" label visible after 4th answer revealed
- ✅ Walk Away button enabled after all answers revealed, disabled when answer selected
- ✅ Game outcome tracking: `GameOutcome` enum (InProgress, Win, Drop, Wrong)
- ✅ Milestone prize calculations: Q5 → $1,000, Q10 → $32,000
- ✅ "Show Winnings" and "Show Question" checkboxes with mutual exclusivity
- ✅ Auto-show winnings: 2 seconds after reveal, on walk away, on thanks for playing
- ✅ Q6+ Lights Down disables Show Winnings checkbox and resets `_answerRevealStep`
- ✅ Closing sequence with auto-reset cancellation using `CancellationTokenSource`
- ✅ Thanks for Playing auto-reset (10 seconds) cancellable when Closing clicked

#### Screen Implementations
- ✅ Host Screen: Displays game to host with correct answer indicator
- ✅ Guest Screen: Contestant/audience view without answers initially
- ✅ TV Screen: Broadcast display for TV/projector
- ✅ Screen synchronization via `ScreenUpdateService`
- ✅ Event-driven updates for all screens

#### Sound System
- ✅ Question-specific audio files (Q1-Q15)
- ✅ Audio transitions with 500ms timing between tracks
- ✅ Lifeline-specific sounds (50:50, PAF intro/loop/out, ATA intro)
- ✅ Quit sounds (small/large based on question number)
- ✅ Walk away sounds (small/large)
- ✅ Final answer sound
- ✅ Correct/wrong answer sounds
- ✅ Game over sound for closing sequence

#### Lifelines Implemented
1. **50:50** - Removes two wrong answers
   - Random selection of two incorrect answers
   - Visual feedback on control panel and screens
   
2. **Phone-a-Friend (PAF)** - 30-second countdown
   - Three stages: CallingIntro (blue), CountingDown (red), Completed (grey)
   - Audio: intro loop, transition to countdown with 500ms delay
   - 30-second timer with seconds display
   - Button color changes based on stage
   
3. **Ask the Audience (ATA)** - 2-minute timer
   - Three stages: Intro (2min, blue), Voting (1min, grey), Completed (grey)
   - Audio: intro music during explanation phase
   - Total 3 minutes: 2min intro + 1min voting
   - Button color changes and seconds display

#### Question Editor
- ✅ Full CRUD operations for questions
- ✅ CSV import with validation
- ✅ CSV export functionality
- ✅ FFF (Fastest Finger First) question management
- ✅ Level and difficulty filtering

#### Game State Management
- ✅ `GameService` with event-driven updates
- ✅ Risk Mode support (disables 2nd safety net)
- ✅ Free Safety Net Mode
- ✅ Level tracking (0-14, displayed as 1-15)
- ✅ Current/Drop/Wrong/Correct value calculations
- ✅ Lifeline usage tracking

#### Technical Architecture
- ✅ Clean separation: MillionaireGame.Core (business logic) + MillionaireGame (UI)
- ✅ Repository pattern for database access
- ✅ Async/await throughout for responsiveness
- ✅ Nullable reference types enabled
- ✅ Event-driven screen updates
- ✅ Cancellation token support for timer management
- ✅ IconHelper utility for embedded resource management

---

## Next Steps (Immediate Priority)

### Screen Money Tree Visualization
**Status**: Not yet started  
**Priority**: High  
**Implementation Plan**:
1. Add money tree visual component to Host, Guest, and TV screens
2. Display all 15 prize levels with current position highlighting
3. Show currency symbols based on money tree configuration
4. Implement visual states:
   - Unplayed levels (default appearance)
   - Current level (highlighted)
   - Passed levels (dimmed or different color)
   - Safety net levels (special indicator)

### Money Tree Animations
**Status**: Not yet started  
**Priority**: High  
**Implementation Plan**:
1. Level progression animation (moving up the tree)
2. Safety net "lock-in" animation when passing Q5/Q10
3. Wrong answer animation (fall to safety net level)
4. Walk away animation (highlight final value)
5. Smooth transitions using async/await patterns

### Technical Approach
- Add MoneyTreeControl custom control for reusability
- Use TableLayoutPanel or custom drawing for tree layout
- Subscribe to GameService level change events
- Coordinate animations with screen update service
- Test with various money tree configurations (different currencies, safety nets)

---

## Future Enhancements

### High Priority

#### 1. Switch the Question (STQ) Lifeline
**Status**: Not implemented  
**Original VB.NET Location**: `Het DJG Toernooi/Source_Scripts/Lifelines/`  
**Implementation Plan**:
- Add `SwitchQuestion` button to Control Panel lifeline area
- Load new question at same level when activated
- Maintain same lifeline availability settings
- Play appropriate sound effect
- Update all screens to show new question
- Mark old question as "skipped" in database (optional tracking)

**Required Changes**:
- `ControlPanelForm.cs`: Add STQ button handler
- `GameService.cs`: Add `SwitchQuestion()` method
- `QuestionRepository.cs`: Ensure proper random question selection (exclude previously used)
- `ScreenUpdateService.cs`: Handle question switch updates
- Sound files: Add STQ sound effect

**Estimated Complexity**: Medium (3-4 hours)

#### 2. Fastest Finger First (FFF) Networking
**Status**: Partial (guest client exists, networking not implemented)  
**Original VB.NET Location**: `Het DJG Toernooi/Source_Scripts/Lifelines/`, FFF server/client forms  
**Implementation Plan**:
- TCP/IP server for FFF questions
- Client application connects to server
- Real-time answer submission and timing
- Leaderboard display
- Winner selection

**Required Changes**:
- Create `FFFServer` service class for networking
- Implement TCP listener and client management
- Update `MillionaireGame.FFFGuest` with networking code
- Add FFF server control panel in main app
- Database tracking of FFF results

**Estimated Complexity**: High (8-10 hours)

### Medium Priority

#### 3. Double Dip Lifeline
**Status**: Not implemented  
**Original VB.NET Location**: `Het DJG Toernooi/Source_Scripts/Lifelines/`  
**Implementation Plan**:
- Allow two answer attempts for current question
- First wrong answer doesn't end game
- Second wrong answer follows normal wrong answer logic
- Disable other lifelines during Double Dip
- Visual indicator showing "attempts remaining"

**Required Changes**:
- `ControlPanelForm.cs`: Add Double Dip button and logic
- `GameService.cs`: Track Double Dip state and attempt count
- Answer selection logic: Check if first or second attempt
- Screen updates: Show attempt indicator

**Estimated Complexity**: Medium (3-4 hours)

#### 4. Ask the Host Lifeline
**Status**: Not implemented  
**Original VB.NET Location**: `Het DJG Toernooi/Source_Scripts/Lifelines/`  
**Implementation Plan**:
- Host gives their opinion on correct answer
- Free-form text input or answer selection
- Display host's choice to contestant
- No time limit (host controls when to reveal)

**Required Changes**:
- `ControlPanelForm.cs`: Add Ask Host button and input dialog
- `ScreenUpdateService.cs`: Show host's opinion on screens
- Sound effect for Ask the Host activation

**Estimated Complexity**: Low (2-3 hours)

#### 5. Enhanced Screen Transitions
**Status**: Basic implementation exists, needs polish  
**Original VB.NET Location**: Various screen forms  
**Implementation Plan**:
- Fade in/out effects for question changes
- Animated prize ladder highlights
- Lifeline activation animations
- Winner celebration effects

**Required Changes**:
- WinForms animation helpers
- Timer-based fade effects
- Image/color transition utilities

**Estimated Complexity**: Medium (4-5 hours)

### Low Priority

#### 6. Game Statistics and Reporting
**Status**: Not implemented  
**Implementation Plan**:
- Track game history (winners, walkaways, wrong answers)
- Question usage statistics
- Lifeline usage patterns
- Export reports to CSV

**Required Changes**:
- New database tables for game history
- Statistics repository class
- Reporting form/dialog
- CSV export functionality

**Estimated Complexity**: Medium (4-5 hours)

#### 7. Customizable Themes
**Status**: Not implemented  
**Implementation Plan**:
- Multiple color schemes
- Custom background images
- Font customization
- Save/load theme presets

**Required Changes**:
- Theme settings class
- Dynamic color/font application
- Theme editor dialog
- Persist theme settings in config.xml

**Estimated Complexity**: Medium-High (6-8 hours)

---

## Future Enhancements Discussed

### Version 0.3 Targets
1. **Switch the Question lifeline** - Complete STQ implementation
2. **Double Dip lifeline** - Two-attempt system
3. **Ask the Host lifeline** - Host opinion feature
4. **FFF Networking** - Begin networking infrastructure
5. **Screen transitions** - Polish animations and effects

### Version 0.4 Targets
1. **FFF Networking** - Complete online features
2. **Game statistics** - History tracking and reporting
3. **Enhanced sound options** - Custom sound packs
4. **Improved error handling** - Better error messages and recovery

### Version 1.0 Targets (Feature Parity)
1. Complete feature parity with VB.NET version
2. Comprehensive testing suite
3. Installer and deployment package
4. User documentation and help system
5. Performance optimization
6. Bug fixes and stability improvements

---

## Technical Debt & Known Issues

### Current Limitations
1. **STQ Lifeline**: Not yet implemented
2. **FFF Networking**: Guest client exists but no server implementation
3. **Double Dip/Ask Host**: Not implemented
4. **Screen animations**: Basic, could be enhanced
5. **Error handling**: Could be more robust in some areas

### Performance Considerations
- Sound file loading is synchronous (could be optimized with caching)
- Database queries could benefit from connection pooling
- Screen updates could be batched for better performance

### Code Quality
- ✅ Async/await used throughout
- ✅ Nullable reference types enabled
- ✅ Clean architecture with separation of concerns
- ✅ Event-driven design pattern
- ⚠️ Some methods in ControlPanelForm are becoming long (consider refactoring)
- ⚠️ Sound service could be more testable (dependency injection)

---

## Migration Strategy from VB.NET

### Completed Migrations
1. ✅ Core models (GameState, Question, Lifeline, etc.)
2. ✅ Settings management (ApplicationSettings, SqlSettings)
3. ✅ Database layer (QuestionRepository, GameDatabaseContext)
4. ✅ Game logic (GameService)
5. ✅ Control Panel UI (ControlPanelForm)
6. ✅ Screen implementations (Host, Guest, TV)
7. ✅ Sound engine (SoundService)
8. ✅ Question Editor (QuestionEditorMainForm)
9. ✅ Three lifelines: 50:50, PAF, ATA

### Remaining VB.NET Files to Review

#### High Priority
- `Source_Scripts/Lifelines/` - STQ, Double Dip, Ask Host implementations
- `Windows/Fastest Finger First/` - FFF server and networking logic

#### Medium Priority
- `Source_Scripts/Classes/` - Any utility classes not yet ported
- `Windows/General/` - Additional dialogs and utilities
- `Source_Scripts/DatabaseAndSettings/` - Any missing database utilities

#### Low Priority
- `Resources/` - Additional resource files
- `Test/` - Test forms and utilities

### Migration Approach
1. **Identify**: Review VB.NET source for specific feature
2. **Design**: Plan C# implementation with modern patterns
3. **Implement**: Write C# code with async/await, events, etc.
4. **Test**: Verify functionality matches original
5. **Document**: Update README and CHANGELOG

---

## Key Design Decisions

### Progressive Answer Reveal System
- State machine approach with `_answerRevealStep` (0-5)
- Question button acts as "Next" during reveal sequence
- Textboxes on control panel populate progressively to match screen behavior
- "Show Correct Answer to Host" only visible after all answers shown

### Game Outcome Tracking
- `GameOutcome` enum distinguishes Win/Drop/Wrong for proper winnings calculation
- Milestone checks use `>=` instead of `>` (Q5+ and Q10+)
- Thanks for Playing uses outcome to display correct final amount

### Cancellation Token Pattern
- Auto-reset after Thanks for Playing can be cancelled
- Closing button acts as "final task" - cancels all timers
- Proper cleanup in finally blocks

### Mutual Exclusivity Pattern
- Show Question and Show Winnings checkboxes cannot both be checked
- CheckedChanged event handlers enforce exclusivity
- Auto-show winnings respects exclusivity rules

### Screen Coordination
- `ScreenUpdateService` broadcasts to all screens via interfaces
- Event-driven updates prevent tight coupling
- Screens implement `IGameScreen` interface for consistency

---

## Important Files Reference

### Core Project Files
- `MillionaireGame.Core/Game/GameService.cs` - Main game logic
- `MillionaireGame.Core/Database/QuestionRepository.cs` - Database access
- `MillionaireGame.Core/Settings/ApplicationSettings.cs` - Config management
- `MillionaireGame.Core/Models/GameState.cs` - Game state model
- `MillionaireGame.Core/Helpers/IconHelper.cs` - Resource loading

### Main Application Files
- `MillionaireGame/Forms/ControlPanelForm.cs` - Main control panel (2262 lines)
- `MillionaireGame/Forms/HostScreenForm.cs` - Host screen
- `MillionaireGame/Forms/GuestScreenForm.cs` - Guest screen
- `MillionaireGame/Forms/TVScreenForm.cs` - TV screen
- `MillionaireGame/Services/SoundService.cs` - Audio playback
- `MillionaireGame/Services/ScreenUpdateService.cs` - Screen coordination
- `MillionaireGame/Helpers/IconHelper.cs` - UI resource loading

### Configuration Files
- `MillionaireGame/lib/config.xml` - Application settings
- `MillionaireGame/lib/sql.xml` - Database connection settings

### Documentation
- `src/README.md` - Main documentation
- `src/CHANGELOG.md` - Version history
- `src/DEVELOPMENT_CHECKPOINT.md` - This file

---

## Testing Checklist for Next Session

### Core Gameplay Flow
- [ ] Start new game → Pick Player → Explain Game → Lights Down
- [ ] Q1-5: Load question, reveal answers progressively, select answer
- [ ] Q6-15: Same flow with higher stakes
- [ ] Walk Away: Proper winnings display, Thanks for Playing
- [ ] Wrong Answer: Milestone or $0 based on level
- [ ] Win Q15: $1,000,000 display

### Lifelines
- [ ] 50:50: Removes two wrong answers correctly
- [ ] PAF: Intro loop → countdown → timeout/stop
- [ ] ATA: 2min intro → 1min voting → completion
- [ ] All lifelines: Proper button state changes (color, enabled/disabled)

### Screen Coordination
- [ ] Host screen shows correct answer indicator
- [ ] Guest screen hides answers until revealed
- [ ] TV screen matches guest screen
- [ ] All screens update simultaneously
- [ ] Winnings display works on all screens

### Edge Cases
- [ ] Closing button cancels auto-reset timer
- [ ] Show Question/Winnings mutual exclusivity works
- [ ] Q6+ Lights Down resets reveal state
- [ ] Reset button properly clears all state
- [ ] Audio transitions work smoothly

---

## Next Development Session Goals

### Immediate (Next Session)
1. Implement **Switch the Question** lifeline
2. Add STQ to lifeline configuration system
3. Test STQ with all game flow scenarios

### Short-term (1-2 Sessions)
1. Implement **Double Dip** lifeline
2. Implement **Ask the Host** lifeline
3. Add all three to lifeline configuration

### Medium-term (3-5 Sessions)
1. Begin FFF networking infrastructure
2. Implement FFF server service
3. Update FFF guest client with networking
4. Test multi-client FFF scenarios

### Long-term (Version 0.3)
1. Enhanced screen transitions
2. Game statistics and reporting
3. Performance optimizations
4. Comprehensive testing

---

## Future Planned Features (Post v1.0)

### 1. Broadcasting Integration
**Goal**: Enable streaming and broadcast platform compatibility
- OBS Studio integration for scene management
- Virtual camera output for streaming software
- Direct streaming protocol support
- Chroma key/green screen support for TV screen output
- NDI (Network Device Interface) output support
- Custom RTMP streaming capabilities

**Use Cases**:
- Live streaming to Twitch, YouTube, Facebook Gaming
- Professional production with multi-camera setups
- Recording and playback capabilities
- Picture-in-picture contestant view

### 2. Stream Deck Integration
**Goal**: Hardware control panel for game show production
- Elgato Stream Deck plugin development
- Button mapping for core game functions:
  - Question reveal (progressive A→B→C→D)
  - Answer selection (A/B/C/D buttons)
  - Lifeline activation
  - Lights down / New question
  - Show/hide screens
- Custom button icons matching game branding
- LED feedback for button states (active/disabled)
- Profile switching for different game modes
- Macro support for common sequences

**Technical Requirements**:
- Stream Deck SDK integration
- WebSocket/HTTP API for external control
- Real-time state synchronization
- Button state feedback system

### 3. Web-Based Mobile Integration
**Goal**: Replace standalone FFF client with unified web-based system
- Mobile-responsive web interface
- Real-time WebSocket communication
- Cross-platform support (iOS, Android, desktop browsers)

**Features**:
- **FFF (Fastest Finger First) Participant System**
  - Mobile device registration via QR code or URL
  - Real-time question display
  - Answer submission with timing
  - Results display
  - Contestant queue management
  
- **ATA (Ask the Audience) Voting**
  - Anonymous voting via mobile devices
  - Real-time vote aggregation
  - Results visualization
  - Vote percentage display

- **Game Control Dashboard**
  - Web-based control panel alternative
  - Read-only spectator view
  - Admin controls via web interface

**Architecture**:
- ASP.NET Core web server
- SignalR for real-time communication
- Progressive Web App (PWA) capabilities
- Offline-capable service workers
- Mobile-first responsive design

**Benefits**:
- No client installation required
- Unified codebase (deprecate separate FFF client)
- Cross-platform compatibility (Mac, Linux, mobile)
- Easier updates and maintenance
- Lower barrier to entry for participants

### 4. QR Code Display System
**Goal**: Seamless audience participation via mobile devices
- Dynamic QR code generation
- TV screen display integration
- Broadcast-safe QR code positioning

**Features**:
- **Context-Aware Display**
  - Show during FFF mode: "Join Fastest Finger First"
  - Show during ATA lifeline: "Vote Now - Ask the Audience"
  - Auto-hide when not needed
  
- **Connection Management**
  - Unique session codes for each game
  - Device registration tracking
  - Connection status indicators
  - Automatic reconnection handling

- **Display Options**
  - Corner overlay on TV screen
  - Full-screen display mode
  - Customizable positioning and size
  - Branding/styling options

**Technical Implementation**:
- QR code library integration (QRCoder or ZXing.Net)
- Local network hosting (mDNS/Bonjour discovery)
- Public URL support (ngrok, cloudflare tunnel)
- SSL/TLS for secure connections
- Session management and security

**Integration Points**:
- TVScreenForm overlay rendering
- Broadcast output composition
- Web server URL management
- Network configuration UI

---

## Notes for Future Developer (or Future Me)

### Code Style Conventions
- Use async/await for all I/O operations
- Prefer nullable reference types (enable warnings)
- Use event-driven patterns for UI updates
- Keep business logic in Core library
- XML documentation for public APIs

### Testing Strategies
- Manual testing with debug mode enabled (`--debug` flag)
- Console.WriteLine statements for debugging (wrapped in Program.DebugMode checks)
- Test with actual database and sound files
- Verify all screen states simultaneously

### Common Pitfalls
- Remember to reset `_answerRevealStep` for Q6+ Lights Down
- Milestone checks need `>=` not `>` (Q5 is level 4, Q10 is level 9)
- Audio file paths are relative to executable directory
- Closing button must cancel all active timers

### VB.NET → C# Translation Tips
- VB `Handles` → C# event subscription in constructor
- VB `Dim` → C# `var` or explicit type
- VB `Module` → C# `static class`
- VB `Optional` parameters → C# default parameters
- VB `ByRef` → C# `ref` or `out`

---

## Resources

### Documentation
- [Original VB.NET README](../README.md)
- [C# README](README.md)
- [CHANGELOG](CHANGELOG.md)

### External Dependencies
- .NET 8.0 SDK
- NAudio 2.2.1 (audio playback)
- System.Data.SqlClient 4.8.6 (database)

### Useful Links
- **C# Repository** (Current): https://github.com/jdelgado-dtlabs/TheMillionaireGame
  - Branch: master-csharp
- **Original VB.NET Repository**: https://github.com/Macronair/TheMillionaireGame
  - Branch: master (VB.NET version)

---

**End of Checkpoint - v0.2-2512**

