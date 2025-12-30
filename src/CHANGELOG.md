# Changelog

All notable changes to The Millionaire Game C# Edition will be documented in this file.

## [v0.8.0-2512] - 2025-12-29

### Fixed
- **Settings Dialog UI Refinement** ✅ COMPLETE
  * Standardized window dimensions to 684x540px for consistent layout across all tabs
  * Standardized all main tabs (Screens, Broadcast, Lifelines, Money Tree, Sounds, Audience) to 652x438px
  * Standardized nested Sound tabs (Soundpack, Audio Settings, Mixer) to 638x404px
  * **Lifelines Tab**: Complete redesign - removed 4 GroupBox containers, implemented flat 3-column grid layout for cleaner appearance
  * **Money Tree Tab**: Removed Prizes GroupBox, positioned controls directly on tab, expanded currency groups from 210px to 280px width, centered currency header, renamed first group to "Currency 1"
  * **Audience Tab**: Fixed IP/port enable/disable logic, reduced Server group height from 240px to 220px to eliminate scrollbars
  * **Screens Tab**: Expanded all groups (Previews, Multiple Monitor Control, Console) to 620px width for full-width consistency
  * Repositioned OK/Cancel buttons to Y=490 for proper window fit
  * Eliminated all horizontal and vertical scrollbars across all tabs
  * Location: OptionsDialog.Designer.cs, OptionsDialog.cs

- **FFF Mode Persistence Bug** ✅ COMPLETE
  * Fixed issue where FFF window retained "Online" mode after web server stopped and window was reset
  * Implemented dynamic mode switching via UpdateModeAsync() method
  * Window now checks web server state before showing and reconfigures UI accordingly
  * Location: FFFWindow.cs, ControlPanelForm.cs

### Changed
- **Workspace Reorganization**:
  * All VB.NET projects moved to `archive-vbnet/` folder (scheduled for removal at v1.0)
  * Documentation consolidated in `src/docs/` with organized structure (sessions/, reference/, guides/)
  * New root README.md with GitHub Wiki reference for comprehensive documentation
  * Removed obsolete FFF_BACKUP folders (OneDrive merge recovery backups no longer needed)
  * Clean workspace structure: archive-vbnet/, src/, and configuration files only at root

- **FFF Architecture Refactoring** ✅ COMPLETE
  * Renamed FFFControlPanel → FFFOnlinePanel for clarity (Online = web-based mode)
  * Renamed localPlayerPanel → fffOfflinePanel for clarity (Offline = local player selection mode)
  * Extracted FFFOfflinePanel into separate UserControl (FFFOfflinePanel.cs + Designer.cs)
  * Reduced FFFWindow.cs from 597 to 236 lines (60% reduction) by extracting offline logic
  * Clear separation of concerns: FFFWindow (mode switcher), FFFOnlinePanel (web mode), FFFOfflinePanel (local mode)
  * Event-driven architecture with PlayerSelected event for offline mode completion
  * Location: FFFWindow.cs, FFFOnlinePanel.cs, FFFOfflinePanel.cs

### Added
- **Question Editor Enhancements** ✅ FEATURE COMPLETE
  * CSV Import: Full parsing with validation, error reporting by line number, support for quoted fields
  * CSV Export: Proper escaping of quotes and commas, includes all question data and ATA percentages
  * Sound Pack Removal: UI integration with confirmation dialog, prevents deletion of Default pack
  * Location: ImportQuestionsForm.cs, ExportQuestionsForm.cs, OptionsDialog.cs

- **CSCore Audio System - Complete Implementation** ✅ FEATURE COMPLETE
  - **DSP Core Infrastructure (Phase 1-2)**:
    * AudioCueQueue with FIFO queue and priority system (Normal/Immediate)
    * Equal-power crossfading between sounds (configurable, default 50ms)
    * Silence detection with RMS amplitude monitoring (configurable threshold, default -40dB)
    * Automatic fadeout on silence (default 50ms) to prevent DC pops
    * Initial delay before silence detection (default 2500ms) for fade-in protection
    * Custom threshold overrides per sound for special cases
  - **Audio Settings UI (Phase 3-4)**:
    * Complete Audio Settings tab in Options dialog (30 UI controls)
    * Silence Detection: Enable/disable, threshold (-60 to -20dB), duration (100-1000ms), initial delay (0-5000ms), fadeout (10-200ms)
    * Crossfade Settings: Enable/disable, duration (10-200ms)
    * Audio Processing: Master/Effects/Music gain controls (-20 to +20dB), limiter enable
    * Real-time value display with TrackBar labels
    * Settings persistence to/from XML with in-place property updates
  - **Game Integration**:
    * Q1-Q5 audio sequences with silence-based transitions (no manual timing)
    * FFF intro sequence with automatic progression
    * All lifelines integrated with queue system
    * Comprehensive testing confirms smooth transitions, no premature cutoffs
  - **Location**: SoundService.cs, EffectsChannel.cs, AudioCueQueue.cs, SilenceDetectorSource.cs, OptionsDialog.cs, ApplicationSettings.cs

- **Shutdown System Enhancement** ✅ FEATURE COMPLETE
  - **ShutdownProgressDialog**:
    * Real-time progress tracking with 7-step shutdown sequence
    * Component-level visibility (Stop Audio, Dispose Audio, Stop Web Server, Close Windows, Stop Timers, Dispose Lifeline Manager, Shutdown Consoles)
    * Per-step timing with stopwatch (helps identify slow components)
    * Force-close safety button with 10-second timeout
    * GameConsole logging integration for all shutdown steps
  - **Audio Disposal Fix**:
    * Proper Stop → Dispose sequence prevents orphaned audio processes
    * No more audio playing after application close
    * No more file locks preventing builds
  - **Shutdown Loop Protection**:
    * _isShuttingDown flag prevents FormClosing re-entry
    * Async shutdown with Task.Run prevents UI blocking
  - **Comprehensive Logging**:
    * All shutdown steps logged to GameConsole with Debug/Info levels
    * AddStep(), AddMessage(), UpdateStatus(), Complete() all write to game log
    * LogSeparator() and summary at completion
  - **Location**: ControlPanelForm.cs, ShutdownProgressDialog.cs, GameConsole.cs

### Fixed
- **Audio Settings Persistence Bug** ✅ CRITICAL FIX
  - **Root Cause**: ApplicationSettings.LoadFromXml() replaced entire Settings object with `Settings = loadedSettings`
  - **Impact**: Broke reference chain in SoundService → EffectsChannel → AudioCueQueue
  - **Solution**: Implemented CopySettingsProperties() to update properties in-place (95 properties)
  - **Result**: AudioCueQueue maintains valid references to SilenceDetectionSettings throughout app lifetime
  - **Location**: ApplicationSettings.cs

- **Shutdown Audio Orphaning** ✅ FIXED
  - Audio processes no longer continue running after application close
  - Proper disposal sequence implemented (StopAllSoundsAsync → SoundService.Dispose → AudioMixer.Dispose)
  - Build file locks resolved (no more "file in use" errors)

### Changed
- **ApplicationSettings**: LoadFromXml() now uses CopySettingsProperties() instead of object replacement
- **ControlPanelForm**: FormClosing handler rewritten with re-entry protection and async shutdown
- **GameConsole**: Added Shutdown() method with cancellation token and log flush

## [v0.5.3-2512] - 2025-12-26

### Fixed
- **FFF Winner Detection and Display** ✅ COMPLETE
  - **Ranking Algorithm**: Fixed to rank correct answers first (by time), then incorrect answers
  - **Winner Display**: Only fastest correct answer marked with ✓, slower correct marked with ✗ and "(too slow)"
  - **Button Flow**: Show Winners button now appears only when >1 correct answer exists
  - **Visual Indicators**: Clear distinction between winner (✓), eliminated (✗ too slow), and incorrect (✗ incorrect)
  
- **Audio System - MusicChannel Stop Bug** ✅ COMPLETE
  - **Root Cause**: _currentMusicIdentifier only set when loop=true, causing StopSound() to fail for non-looping music
  - **Fix**: Always set _currentMusicIdentifier for music sounds regardless of loop parameter
  - **Impact**: FFFReadCorrectOrder music now stops immediately when Winner button clicked
  - **Location**: SoundService.cs line 137 - Removed conditional identifier setting

### Changed
- **FFFControlPanel.cs**:
  * CalculateRankings() now separates correct/incorrect answers before sorting by time
  * UpdateRankings() shows status text instead of premature winner display
  * UpdateUIState() enables Show Winners button only when multiple correct answers exist
  * Visual display shows only Rank 1 as winner (✓), all others as eliminated (✗)
  
- **SoundService.cs**:
  * PlaySound(SoundEffect, bool loop) now always sets _currentMusicIdentifier for music sounds
  * Enables StopSound() to work correctly for both looping and non-looping music

## [v0.5.2-2512] - 2025-12-23

### Added
- **FFF Web Participant Interface - Phase 5.2** ✅ COMPLETE
  - **Answer Submission Real-Time Events**:
    * AnswerSubmitted broadcasts to all clients (including control panel)
    * Participant cache for DisplayName lookup
    * Comprehensive JsonElement parsing for answer submissions
  - **Rankings Calculation and Display**:
    * Extract Rankings array from server wrapper object
    * JsonElement array parsing for rankings
    * Manual property enumeration for all ranking fields
    * Time-based winner determination (fastest correct answer)
  - **Enhanced Logging**:
    * Detailed GameConsole.Log throughout answer submission flow
    * Property-level logging in ParseRanking
    * Data type and ValueKind logging for JsonElement debugging
  - **UI Polish**:
    * Changed MessageBoxIcon.Information to MessageBoxIcon.None (silent notifications)
    * Affects: FFF Started, FFF Ended, Results Ready, Winner Selected

### Changed
- **FFFHub.SubmitAnswer**: Now broadcasts AnswerSubmitted event to Clients.All
- **FFFClientService**:
  * Added `_participants` cache field
  * CalculateRankingsAsync extracts Rankings from wrapper object
  * ParseRankings handles JsonElement.ValueKind.Array
  * ParseRanking manually enumerates JsonElement properties
  * ParseAnswer uses _participants cache for DisplayName lookup
- **FFFControlPanel**: All Information message boxes now use Icon.None

### Fixed
- Answer submissions not appearing in control panel (AnswerSubmitted broadcast missing)
- Rankings showing as empty despite correct answer submissions (wrapper object parsing)
- IsCorrect flag not parsing correctly (JsonElement property enumeration)
- DisplayName showing "Unknown" in answer submissions (participant cache)
- System beep sounds on FFF notification dialogs

## [v0.5-2512] - 2025-12-23

### Added
- **FFF Offline Mode - Complete Implementation** ✅ FEATURE COMPLETE
  - **Player Elimination System**: Winner automatically removed from contestant pool
    * RemovePlayerAndShift() removes winner and renumbers remaining players
    * NoMorePlayers flag tracks when ≤ 2 contestants remain
    * Player names and state persist between rounds
  - **Window State Management**: Preserve player data across rounds
    * Changed from Close() to Hide() to maintain window instance
    * Player list, names, and elimination state preserved
    * ResetLocalPlayerState() only called on game reset
  - **Control Panel Integration**:
    * NoMorePlayers validation before opening FFF window
    * Reset Round preserves player state (resetFFFWindow: false)
    * Reset Game/Closing fully resets to 8 players
  - **UI Optimizations**:
    * Button sizes: 340x50 to match Control Panel design
    * Player panel width: 580px (was 500px) for better name display
    * Fixed panel height: 340px to prevent visual bugs
    * Default player count: 8 (was 2)
    * Disabled auto-scroll on player panel
  - **Sound Timing Improvements**:
    * Player introduction sound plays once (not per player)
    * Random selection uses 5-second timer matching FFFRandomPicker cue length
    * Silent message box (MessageBoxIcon.None) for winner dialog
  - **FFF Texture Assets**: Copied 9 FFF graphics from VB.NET project
    * Theme-specific backgrounds (01_FFF, 02_FFF, 04_FFF, 05_FFF)
    * Contestant strap states (idle, correct, fastest/winner)
    * Ready for future graphics implementation
  - **Button State Machine**: Consistent three-state system (Green/Blue/Grey)
  - **Visual Bug Fixes**:
    * Fixed orphaned Player 8 label with 7 players
    * Fixed panel shrinking during elimination
    * Proper cleanup of textbox and label pairs

### Changed
- **FFF Window Lifecycle**: Hide() instead of Close() to preserve state
- **ResetAllControls()**: Added optional resetFFFWindow parameter
- **Sound Playback**: Reverted to fixed 5-second timer for reliability

### Fixed
- Player state resetting to 8 players after each round
- Player names not persisting between rounds
- Visual bugs with panel height and orphaned labels
- System beep sound on winner dialog

## [v0.4-2512] - 2025-12-23

### Added
- **Lifeline Icon Display System** ✅ FEATURE COMPLETE
  - **Visual Icons**: Four-state icon display on all screens
    * Hidden state (invisible): During explain game phase until pinged
    * Normal state (black): Lifeline available and visible
    * Bling state (yellow/glint): During activation or demo ping
    * Used state (red X): Lifeline consumed
  - **Icon Positioning**: Screen-specific placement optimized to avoid UI overlaps
    * HostScreen & GuestScreen: (680, 18) horizontal, spacing 138px, Size: 129x78
    * TVScreenFormScalable: (1770, 36) vertical stack, spacing 82px, Size: 72x44
  - **Dual Animation Modes**:
    * Demo mode (explain phase): PingLifelineIcon() - Bling with sound and 2s timer → Normal
    * Execution mode (actual use): ActivateLifelineIcon() - Silent Bling state without timer
  - **Progressive Reveal**: Manual display during explain game
    * Icons start Hidden during explain phase
    * Appear when pinged for demonstration
    * Automatically visible (Normal) during regular gameplay
  - **State Persistence**: Used states maintained across questions
    * GameService synchronizes dual lifeline collections (List and Dictionary)
    * InitializeLifelineIcons() preserves Used states from GameState
    * Icons clear only on full game reset
  - **Multi-Stage Protection**:
    * Standby mode (orange buttons) when multi-stage lifeline active
    * Click cooldown: 1-second delay between lifeline clicks
    * Timer guards prevent queued events from firing after completion
    * Other lifeline buttons locked until active lifeline completes
  - **Screen-Specific Visibility**:
    * Host/Guest: Icons remain visible when question hidden or during winnings display
    * TV Screen: Icons hidden when showing winnings or question hidden (audience focus)
  - **All Lifeline Types Supported** (6 total):
    * 50:50 - Shows Bling when activated, Used when complete
    * Phone-a-Friend (PAF) - Shows Bling during call, Used when complete
    * Ask the Audience (ATA) - Shows Bling during voting, Used when complete
    * Switch the Question (STQ) - Shows Bling and Used immediately
    * Double Dip (DD) - Shows Bling when activated, Used after second attempt
    * Ask the Host (ATH) - Shows Bling when activated, Used when answer given
  - **Architecture**:
    * LifelineIcons helper class loads icons from embedded resources
    * LifelineIconState enum (Hidden, Normal, Bling, Used)
    * IGameScreen interface methods: ShowLifelineIcons(), HideLifelineIcons(), SetLifelineIcon(), ClearLifelineIcons()
    * ScreenUpdateService broadcasts icon state changes to all screens
    * 18 embedded icon resources in lib/textures (6 types × 3 states each)
    * Independent timer system for demo pings via Dictionary<int, (LifelineType, Timer)>

- **Ask the Audience (ATA) Enhanced Visual System**
  - **Timer Display**: Two-phase visual timer on all screens
    * Intro phase: 2 minutes (blue border, MM:SS format)
    * Voting phase: 1 minute (red border, MM:SS format)
    * Position: Upper-left below PAF (50, 50), Size: 300x150
    * Real-time updates every second
  - **Animated Voting Results**: Random percentages during voting
    * Generates random A/B/C/D percentages summing to 100%
    * Updates every second during 60-second voting phase
    * Creates dramatic visual feedback for audience
  - **Results Display**: Post-voting placeholder results
    * Shows 100% on correct answer when voting completes
    * Position centered below lifeline icons: Host/Guest (635, 150, 650x400)
    * Top-center on TV screen for audience view (585, 50, 750x450)
  - **Architecture Enhancements**:
    * ShowATATimer() and ShowATAResults() in IGameScreen interface
    * ScreenUpdateService tracks current question for correct answer access
    * GetCorrectAnswer() method for lifeline access to question data
    * Helper methods: GenerateRandomATAPercentages(), GeneratePlaceholderResults()

- **Phone a Friend (PAF) Timer Visual Display**
  - Visual timer window on all screens showing PAF countdown
  - Three display states: "Calling..." (intro), countdown (30→0), hidden (completed)
  - ShowPAFTimer(int secondsRemaining, string stage) added to IGameScreen interface
  - Semi-transparent timer box in upper-left corner (50, 50, 300x150)
  - Color-coded border: Blue during "Calling...", Red during countdown
  - Large centered text: "Calling..." (28pt) or countdown number (60pt)
  - Real-time updates every second during 30-second countdown
  - Implemented in HostScreenForm, GuestScreenForm, TVScreenFormScalable
  - Timer hides automatically on completion or screen reset

- **Screen Synchronization Verification**
  - All TV screens (actual and preview) confirmed synchronized via ScreenUpdateService
  - Both use TVScreenFormScalable and register for broadcasts
  - Preview Screen shows live rendering of all three screens (Host, Guest, TV)
  - Control panel can monitor TV display through Preview Screen feature

## [Unreleased] - 2025-12-22

### Added
- **Switch the Question (STQ) Lifeline**
  - Fully functional STQ lifeline implementation
  - Confirmation dialog before switching questions
  - Loads new random question at same difficulty level
  - Marks lifeline as used and updates button state (grey/disabled)
  - Sound effect integration with stq_start.mp3
  - Configurable via Settings: Lifeline slots can be set to "switch" type

### Fixed
- **Threading Issues in Answer Reveal**
  - Fixed persistent cross-thread exceptions during answer reveal sequences
  - Eliminated all async/await from RevealAnswer method - replaced with timer-based delays
  - Changed RevealAnswer from async void to void (synchronous method)
  - All UI operations now guaranteed to execute on UI thread
  - Five timers implemented: initialDelayTimer, bedMusicTimer, winningsTimer, completionTimer, q15Timer
  - Benefits: Thread-safe by design, no context switching, no Invoke() needed, easier to debug
  - Created helper methods: ShowWinningsAndEnableButtons(), HandleQ15Win(), FinishWrongAnswerSequence()
  - Removed misleading stack trace logging from StartSafetyNetAnimation()

### Added
- **Console Management System**
  - Added "Console" group with "Show Console" checkbox in Settings > Screens
  - Debug mode: Checkbox checked and disabled (console always visible)
  - Release mode: User can toggle console window visibility via checkbox
  - Console window show/hide functionality with Windows API integration (AllocConsole/FreeConsole)
  - Console output persists when window is reopened
  - ShowConsole property added to ApplicationSettings with XML persistence

- **Preview Screen Feature**
  - Unified preview window showing Host, Guest, and TV screens simultaneously
  - Two orientation modes: Vertical (stacked) and Horizontal (side-by-side)
  - Real-time updates synchronized with main screens via ScreenUpdateService
  - Right-side maximize behavior with aspect ratio preservation
  - Screen labels overlay for easy identification
  - Toggle visibility from Screens menu
  - Dedicated screen instances to prevent conflicts with main display screens
  - Settings integration: Preview Orientation dropdown in Options dialog
  - Demo money tree animation support on preview screens
  - Safety net lock-in flash animation support on preview screens

- **Sound Files to Repository**
  - Added Default soundpack with 123 sound files (120 MP3s + soundpack.xml + README.md)
  - Complete audio package now included in repository distribution
  - Sound files tracked in src/MillionaireGame/lib/sounds/Default/

### Changed
- **Settings Dialog Reorganization**
  - Split Screens menu into "Previews" and "Multiple Monitor Control" groups
  - Added monitor count display: "Number of Monitors: # (4 Monitors are required)"
  - Added DEBUG MODE indicator when running in debug configuration
  - Display 1 (control screen) restricted in release mode, available in debug mode
  - Duplicate monitor assignment validation (release mode only)

- **Question Editor Build Output**
  - Converted from standalone executable to class library
  - Now builds as DLL only (MillionaireGameQEditor.dll)
  - Eliminates redundant exe file in build output
  - Question Editor remains accessible from main application menu

- **Repository Management**
  - Simplified src/.gitignore to only contain src-specific ignores
  - Removed .github/copilot-instructions.md from git tracking (keeping local file)
  - Removed redundant ignore patterns already covered by root .gitignore
  - src/.gitignore now only tracks: config.xml, sql.xml (runtime configs)

### Fixed
- **TV Screen Answer Highlighting**
  - Fixed TV screens (both live and preview) not showing correct answer when player selects wrong answer
  - Modified TVScreenFormScalable.DrawAnswerBox() to independently show correct answer in green
  - Correct answer now highlights green even when different from selected answer
  - Wrong answer shows in red as expected

- **Debug Logging Infrastructure**
  - Replaced Walk Away and Thanks for Playing MessageBox dialogs with console logging
  - All game events now use Console.WriteLine() with tagged prefixes ([WALK AWAY], [GAME OVER])
  - Consistent console-first approach for all debug notifications

- **Wrong Answer Display**
  - Fixed money tree displaying wrong value instead of dropped value after wrong answer
  - Fixed safety net animation playing with sound on wrong answer (now silent animation only)
  - Fixed TV screens showing correct winnings amount after wrong answer
  - Fixed dialog boxes showing correct dropped winnings value

- **Preview Screen Updates**
  - Fixed demo money tree animation not displaying on preview screens
  - Fixed safety net lock-in animation not displaying on preview screens
  - Added UpdateMoneyTreeLevel and UpdateMoneyTreeWithSafetyNetFlash methods to PreviewScreenForm

- **Wrong Answer Money Tree Display**
  - Fixed money tree not updating to dropped level (Q0, Q5, or Q10) when player loses
  - Fixed TV screen preview showing incorrect winning amount after wrong answer
  - Fixed Guest and Host screens clearing money tree instead of showing dropped level
  - Money tree now properly displays safety net level reached on all screens

- **Wrong Answer Flow**
  - Wrong answer now triggers safety net lock-in animation when dropping to Q5 or Q10
  - Added 5-second animation delay (12 flashes at 400ms intervals) before showing final dropped value
  - Walk Away button now enabled after wrong answer instead of auto-triggering end-of-round sequence
  - Host and player now have time to discuss the loss before manually ending the round

## [0.3-2512] - 2025-12-22

### Added
- **Settings Enhancements**
  - Screen monitor selection with detailed format: "ID:Manufacturer:Model (Resolution)"
  - WMI queries to extract monitor manufacturer and model names
  - Auto Show and Full Screen checkboxes with immediate effect
  - Dropdown enable/disable based on Full Screen checkbox state
  - Full-screen mode applied immediately when checkbox is toggled
  - Settings persistence to XML (config.xml)

- **Show Correct Answer to Host**
  - Checkbox control with immediate effect (only when all answers revealed)
  - Green highlight on correct answer for host screen preview
  - Toggle on/off at any time during question review
  - Prevents premature answer reveal (requires _answerRevealStep == 5)

### Fixed
- **Money Tree Display**
  - Money tree now resets to level 0 when lights down starts a new player's game
  - Prevents Q15 from remaining displayed after demo animation
  
- **ATA Statistics Display**
  - Removed automatic ATA preview that appeared when all answers were revealed
  - ATA statistics now only show when ATA lifeline is actually activated
  - Cleaned up DrawATAPreview call that was triggering incorrectly

- **Settings Persistence**
  - Fixed ApplicationSettingsManager to use XML mode instead of database
  - Removed database migration code from Program.cs
  - Synchronous SaveSettings() to prevent deadlocks
  - ApplicationSettingsManager instance properly shared across forms

### Removed
- **Deprecated Sound Properties**
  - Removed ~160 individual Sound* properties from ApplicationSettings
  - Removed LoadSoundsFromSettingsLegacy fallback method
  - Soundpack system now exclusively used for all audio
  - Only SelectedSoundPack property retained for sound configuration
  - Cleaner config.xml with minimal sound settings

### Technical Improvements
- System.Management 8.0.0 for WMI monitor queries
- Event handlers for immediate checkbox effects
- Enhanced ApplicationSettings with only active properties
- Nullable string support for ShowCorrectAnswerToHost interface

## [0.3-2512] - 2025-12-21

### Added
- **Graphical Money Tree Rendering**
  - Replaced text-based MoneyTreeControl with VB.NET-style graphical version
  - Host/Guest screens: Money tree on right side (650×569px) with overlays
  - Uses cropped 630×720 tree images with VB.NET coordinate system
  - Text positioning: question numbers and money values with Copperplate Gothic Bold
  - Color-coded levels: Black (current), White (milestones), Gold (regular)

- **Money Tree Demo Animation**
  - Timer-based progression through levels 1-15 at 500ms intervals
  - Three-state button: Show → Hide → Demo (during Explain Game)
  - State tracking with _isExplainGameActive flag for automatic Demo transition
  - Button becomes disabled "Demo Running..." during animation
  - Integrated with Explain Game workflow (no audio restart)

### Fixed
- Money tree automatically hides winning strap when shown (prevents overlap)
- Proportional scaling maintains aspect ratio (650px height avoids question strap)
- Lights Down exits Explain Game mode and resets money tree state

### Known Issues
- TV Screen money tree needs updating to match Host/Guest graphical implementation

## [0.3-2512] - 2025-12-20

### Added
- **Money Tree Settings System**
  - Complete settings UI with descending money tree (Q15→Q1)
  - Dual currency support (Currency 1 and Currency 2)
  - Per-question currency selection (dropdown for each level)
  - Safety net configuration via checkboxes at Q5 and Q10
  - Currency symbols: $, €, £, ¥, and custom text option
  - Currency position control (prefix/suffix)
  - XML persistence to AppData/MillionaireGame/tree.xml

- **Game Integration**
  - Prize displays now reference money tree configuration
  - Dynamic currency selection per level (GetFormattedValue)
  - Risk mode button reflects safety net configuration:
    * Yellow "Activate Risk Mode" = both nets enabled
    * Blue "RISK MODE: 5" = Q5 disabled
    * Blue "RISK MODE: 10" = Q10 disabled
    * Red "RISK MODE: ON" = both disabled (unclickable)

- **Settings Dialog Improvements**
  - Real-time updates via SettingsApplied event
  - Removed Apply button for cleaner UI
  - Cancel button warns about unsaved changes
  - OK button saves and fires update events
  - Settings changes immediately reflected in Control Panel

### Technical Improvements
- MoneyTreeSettings model with dual currency properties
- MoneyTreeService for XML serialization and formatting
- Event-driven settings reload architecture
- Dynamic control creation for flexible money tree UI

### Next Steps
- Add money tree visualization to screen outputs
- Implement money tree animations

## [0.2-2512] - 2025-12-19

### Added
- Complete Control Panel UI with full game flow management
- Progressive answer reveal system (Question → A → B → C → D)
- Host, Guest, and TV screen implementations with synchronized updates
- Question-specific sound system with audio transitions (500ms timing)
- Three lifelines with full functionality:
  - 50:50 (removes two wrong answers)
  - Phone-a-Friend (30-second countdown timer)
  - Ask the Audience (2-minute explanation + 1-minute voting)
- Game outcome tracking (Win/Walk Away/Wrong Answer)
- Milestone prize calculations ($1,000 at Q5, $32,000 at Q10)
- Auto-show winnings feature with checkbox mutual exclusivity
- Closing sequence with auto-reset cancellation support
- Question Editor with CSV import/export functionality
- Risk Mode and Free Safety Net Mode support

### Technical Improvements
- Event-driven screen update service for synchronized display
- Async/await patterns throughout for better responsiveness
- Nullable reference types for improved null safety
- Clean separation between Core library and UI layer
- Repository pattern for database access
- Cancellation token support for timer management

### Known Limitations
- Switch the Question lifeline not yet implemented
- FFF networking features pending
- Double Dip and Ask the Host lifelines not yet implemented

## [0.1] - Initial Development

### Added
- Project structure and solution setup
- Core models (GameState, Question, Lifeline, etc.)
- Settings management (ApplicationSettings, SqlSettings)
- Database layer with QuestionRepository
- Game logic service (GameService)
- Basic UI foundation

