# Changelog

All notable changes to The Millionaire Game C# Edition will be documented in this file.

## [v0.9.8] - 2026-01-03

### Added
- **Closing Sequence Auto-Completion** ✅ COMPLETE
  * QueueCompleted event system through audio stack (AudioCueQueue → EffectsChannel → SoundService)
  * Event-based detection when closing theme finishes playing
  * _completionEventFired flag prevents repeated firing while queue empty
  * Event fires in both paths: normal empty queue AND silence-detected fadeout
  * Replaces unreliable hardcoded 45-second timer
  * Location: Services/AudioCueQueue.cs, Services/EffectsChannel.cs, Services/SoundService.cs

- **Debug Mode Runtime Support** ✅ COMPLETE
  * --debug flag now works in Release builds for production troubleshooting
  * UpdateWindowTitle() helper maintains " - DEBUG ENABLED" suffix through web server lifecycle
  * Console visibility uses runtime Program.DebugMode checks instead of compile-time #if DEBUG
  * Location: Forms/ControlPanelForm.cs, Program.cs

### Changed
- **Closing Sequence Visual Clearing** ✅ COMPLETE
  * CompleteClosing() now clears all visual elements for pristine "blank slate" appearance
  * Added RevealAnswer(string.Empty, string.Empty, false) to clear answer highlight states
  * Clears Q&A display, money tree, answer highlights (orange/green/red), and rug text
  * All buttons disabled except Reset Game (red border)
  * Location: Forms/ControlPanelForm.cs (CompleteClosing method)

- **Event Subscription in Closing** ✅ COMPLETE
  * MoveToThemeStage() subscribes to SoundService.EffectsQueueCompleted event
  * Replaces timer-based approach with audio completion detection
  * Automatically triggers CompleteClosing() when theme finishes
  * Location: Forms/ControlPanelForm.cs (MoveToThemeStage method)

### Fixed
- **QueueCompleted Event Not Firing** ✅ COMPLETE
  * Added event trigger in silence-detected fadeout completion path (line ~612)
  * Event now fires when audio fades out due to silence detection
  * Previously only fired in normal empty queue path
  * Critical fix for automatic closing completion
  * Location: Services/AudioCueQueue.cs (Read method)

- **Debug Title Cleared by Web Server** ✅ COMPLETE
  * UpdateWindowTitle() helper ensures debug suffix persists
  * Called from ControlPanelForm_Load, OnWebServerStarted, OnWebServerStopped
  * Title maintains " - DEBUG ENABLED" through all lifecycle events
  * Location: Forms/ControlPanelForm.cs

- **Green Answer Highlight Persisting** ✅ COMPLETE
  * RevealAnswer(empty) call clears _selectedAnswer, _correctAnswer, _isRevealing states
  * Removes all answer highlights: orange (selected), green (correct), red (wrong)
  * ShowQuestion(false) hides display, RevealAnswer(empty) clears state
  * Location: Forms/ControlPanelForm.cs (CompleteClosing method)

### Removed
- **Deprecated Console Settings** ✅ COMPLETE
  * Removed ShowGameConsole property from ApplicationSettings
  * Removed ShowWebServerConsole property from ApplicationSettings
  * Removed UpdateConsoleVisibility() method from Program.cs (18 lines)
  * Removed call to deprecated method in OptionsDialog.SaveSettings()
  * Location: MillionaireGame.Core/Settings/ApplicationSettings.cs, Program.cs, OptionsDialog.cs

### Technical Details
- **Event Architecture**: QueueCompleted propagates through 3 layers with single-fire flag system
- **Audio Completion**: Fires in both normal empty queue and silence-detected fadeout paths
- **State Clearing**: Distinction between hiding display (ShowQuestion) vs clearing state (RevealAnswer)
- **Runtime Checks**: Console display logic uses Program.DebugMode instead of #if DEBUG
- **Helper Methods**: UpdateWindowTitle() centralizes title management for consistency

## [v0.9.8] - 2025-12-31

### Added
- **Background Graphics System** ✅ COMPLETE
  * Added BackgroundRenderer class for theme-based backgrounds on TV Screen
  * Supports embedded resources (PNG files) and chroma key colors
  * Background images renamed: 01_bkg.png through 06_bkg.png (16:9 aspect ratio)
  * Preview window now displays backgrounds correctly
  * Integrated with OptionsDialog for background selection
  * Custom-drawn ComboBox with 16:9 thumbnail previews (71x40 pixels)
  * Location: Graphics/BackgroundRenderer.cs, lib/textures/*.png
- **MIT License** ✅ COMPLETE
  * Added MIT License with dual copyright (Jean Francois Delgado 2025-2026, Marco Loenen 2017-2024)
  * Updated all documentation with proper attribution
  * License clearly establishes project as substantial rewrite with attribution requirements
  * Location: LICENSE, README.md, src/README.md

### Changed
- **About Dialog Enhancements** ✅ COMPLETE
  * Dynamic version display from Assembly.GetName().Version (no more hardcoding)
  * Added MIT License information with copyright notice
  * Updated author attribution: "C# Version: Jean Francois Delgado"
  * Updated original author: "Original VB.NET Version: Marco Loenen (Macronair)"
  * Added animated sweeping light beams (6 beams, "lights down" effect)
  * Animation only runs when dialog is visible (CPU-efficient)
  * Beams sweep from outside-in to parallel and back (continuous loop)
  * Dialog height increased to 320px to accommodate license info
  * Location: Forms/About/AboutDialog.cs, AboutDialog.Designer.cs

- **Copyright Updates** ✅ COMPLETE
  * All copyrights updated to 2025-2026 for release in new year
  * Project file copyright: "Copyright © 2025-2026 Jean Francois Delgado"
  * Documentation updated with correct attribution and GitHub links
  * Location: MillionaireGame.csproj, LICENSE, README files

- **Package Cleanup** ✅ COMPLETE
  * Removed obsolete packages: QRCoder, System.Data.SqlClient, EF Sqlite
  * Updated to Microsoft.Data.SqlClient 5.2.2 across all projects
  * Removed unused CSV package references
  * Location: All .csproj files

- **Code Quality Improvements** ✅ COMPLETE
  * Eliminated all Console.WriteLine usage (migrated to GameConsole)
  * Removed misleading comments on functional code
  * Build warnings reduced from 36 to 0 (100% elimination)
  * Surgical warning suppression with #pragma directives
  * Version synchronization to v0.9.8 everywhere
  * Location: Multiple files across solution

- **Documentation Reorganization** ✅ COMPLETE
  * Created comprehensive INDEX.md for documentation navigation
  * Organized docs into guides/, reference/, sessions/, archive/
  * Updated START_HERE.md with current v0.9.8 status
  * Fixed duplicate priorities in START_HERE.md
  * All READMEs updated to reflect v0.9.8 state
  * Location: src/docs/

### Fixed
- **Icon Display Issues** ✅ COMPLETE
  * Fixed icon not appearing on GameLogWindow and WebServerLogWindow
  * Corrected namespace: Using MillionaireGame.Helpers.IconHelper (not Core.Helpers)
  * Icon application moved to end of constructor after all UI initialization
  * Improved logging: Now shows form type and title for better debugging
  * Location: GameLogWindow.cs, WebServerLogWindow.cs, IconHelper.cs

- **Background Image Loading** ✅ COMPLETE
  * Fixed embedded resource path: Changed from "MillionaireGame.Graphics" to "MillionaireGame.lib.textures"
  * Background images now load correctly from embedded resources
  * Added comprehensive debug logging to trace resource loading
  * Updated background file references from _FFF.png to _bkg.png naming convention
  * Location: BackgroundRenderer.cs, OptionsDialog.cs

- **UI Layout Issues** ✅ COMPLETE
  * Fixed dropdown overlap with info label in OptionsDialog
  * Moved lblBackgroundInfo from Y:110 to Y:140 (30px down)
  * Dropdown height set to 40px for 16:9 thumbnail display
  * Background selection now displays 6 options (01-06) instead of selective array
  * Location: OptionsDialog.Designer.cs, OptionsDialog.cs

- **Logging System Improvements** ✅ COMPLETE
  * GameConsole now logs even when window is hidden/closed
  * File logging continues independently of window visibility
  * ProcessLogQueue always writes to file logger when available
  * Prevents log loss when console window is minimized
  * Location: GameConsole.cs

- **Logging Architecture Refactor** ✅ COMPLETE (Priority 1)
  * **File-First Logging**: Created FileLogger class as primary logger with async queue processing
  * **GameConsole Refactor**: Simplified from 204 to ~155 lines, removed queue/background task, writes to FileLogger first
  * **WebServerConsole Refactor**: Complete rewrite matching GameConsole pattern
  * **Console Windows**: GameConsoleWindow and WebServerConsoleWindow now tail log files with 100ms refresh timer
  * **File Rotation**: 5-file rotation system (game.log, game.1.log through game.4.log)
  * **Thread Safety**: FileLogger uses concurrent queue and thread-safe FileStream operations
  * **Color Coding**: DEBUG=Gray, INFO=Lime, WARN=Yellow, ERROR=Red in console displays
  * **Obsolete Code Removal**: Deleted ConsoleLogger.cs (replaced by FileLogger)
  * Location: FileLogger.cs, GameConsole.cs, WebServerConsole.cs, GameConsoleWindow.cs, WebServerConsoleWindow.cs

- **Settings UI Improvements** ✅ COMPLETE
  * Replaced console visibility checkboxes with "Open Console" buttons
  * Buttons always enabled (removed DEBUG mode disable logic)
  * Users can now reopen console windows after closing them
  * Removed ShowConsole and ShowWebServerConsole settings (windows are independent)
  * Location: OptionsDialog.cs, OptionsDialog.Designer.cs

- **Naming Consistency Refactor** ✅ COMPLETE
  * **GameLogWindow → GameConsoleWindow**: Renamed for clarity and consistency
  * **ShowConsole → ShowGameConsole**: More descriptive setting name
  * **WebServerLogWindow → WebServerConsoleWindow**: Parallel naming with GameConsoleWindow
  * Updated all references across Program.cs, GameConsole.cs, WebServerConsole.cs, OptionsDialog.cs
  * Improved code maintainability with predictable naming patterns
  * Location: GameConsoleWindow.cs, WebServerConsoleWindow.cs, ApplicationSettings.cs, Program.cs

- **Window Initialization Order Fix** ✅ COMPLETE
  * Fixed icon loading issue by correcting window startup sequence
  * ControlPanel now initializes first, then Preview/Screens, then GameConsole last
  * GameConsole moved from Program.cs to ControlPanelForm_Load (end of method)
  * Prevents focus stealing and ensures proper icon loading on all windows
  * Startup order: ControlPanel → Preview/Screens → GameConsole → WebServerConsole
  * Location: Program.cs, ControlPanelForm.cs

- **Lifeline Hotkey Mapping** ✅ COMPLETE
  * Added F8-F11 keyboard shortcuts for direct lifeline activation
  * F8 → Lifeline Button 1, F9 → Lifeline Button 2, F10 → Lifeline Button 3, F11 → Lifeline Button 4
  * Hotkeys map to button positions (respects user's lifeline configuration)
  * Respects button state (inactive/demo/standby/active modes)
  * Enables complete keyboard control without mouse interaction
  * Location: HotkeyHandler.cs, ControlPanelForm.cs

## [v0.9.5] - 2025-12-30

### Added
- **ATA Online Voting System** ✅ COMPLETE
  * Full Ask the Audience dual-mode implementation (offline + online)
  * Real-time voting from web clients with live percentage updates
  * Multi-phase voting flow:
    - Intro Phase (120s): Question displayed, voting disabled
    - Voting Phase (60s): Clients can submit votes, real-time counts
    - Results Phase: Final percentages displayed, persists until answer selected
    - Clear Phase: Results cleared when host selects answer
  * Database integration: ATAVotes table stores all submissions
  * Auto-completion when all participants have voted
  * Vote tracking: Participants marked as "used" after voting
  * Duplicate vote prevention per session
  * SignalR events: ATAIntroStarted, VotingStarted, VotesUpdated, VotingEnded, ATACleared
  * Location: LifelineManager.cs, GameHub.cs, SessionService.cs, app.js

- **Hub Architecture Consolidation** ✅ COMPLETE
  * Merged FFFHub and ATAHub into unified GameHub at `/hubs/game`
  * Single SignalR endpoint for all game functionality (FFF, ATA, future lifelines)
  * Reduced complexity and improved maintainability
  * Updated all clients: FFFClientService, web client (app.js)
  * Location: GameHub.cs, WebServerHost.cs, FFFClientService.cs, app.js

- **Web Client Session Persistence** ✅ COMPLETE
  * Auto-reconnection on page refresh using localStorage
  * Session data preserved across browser refreshes
  * Participant ID, display name, and session ID cached locally
  * Seamless rejoin without re-entering credentials
  * Server-side reconnection detection by display name
  * Removed aggressive cleanup handlers that cleared data on page transitions
  * Location: app.js, GameHub.cs, SessionService.cs

### Fixed
- **Vote Persistence Bug**
  * Removed `_ataQuestions` dictionary requirement in GameHub.SubmitVote()
  * Dictionary was never populated since LifelineManager broadcasts events directly
  * Votes now save correctly without dictionary validation
  * Location: GameHub.cs (line 318-325)

- **UTF-8 Encoding Issues**
  * Fixed corrupted checkmark character display ("Γ£ô" → "✓")
  * Vote confirmation message now displays properly: "✓ Your vote has been recorded!"
  * Location: app.js (lines 516, 555)

- **DOM Element Null Reference Errors**
  * Added null checks before accessing sessionCode and displayName input fields
  * Prevents crash when auto-reconnecting (fields don't exist on lobby screen)
  * Location: app.js (DOMContentLoaded handler)

- **Service Scope Disposal Errors**
  * Implemented IServiceScopeFactory pattern in all ATA notification methods
  * Creates fresh DbContext instances to prevent disposal conflicts
  * Added `using Microsoft.Extensions.DependencyInjection;`
  * Location: LifelineManager.cs (NotifyWebClientsATAIntro, NotifyWebClientsATAVoting, NotifyWebClientsATAComplete, CheckVoteCompletion)

### Changed
- **ATA Results Display Duration**
  * Results now persist on screen until host selects an answer (previously 5-second auto-hide)
  * Web clients show "Voting has ended - waiting for answer..." message
  * Physical screens maintain results display
  * `ClearATAFromScreens()` method called on answer selection
  * ATACleared event broadcasts to web clients to return to lobby
  * Location: LifelineManager.cs (CompleteATA, ClearATAFromScreens), ControlPanelForm.cs (ContinueAnswerSelection), app.js

- **Reconnection User Experience**
  * Brief flash of login screen on refresh (expected behavior - HTML renders before JS executes)
  * Auto-reconnect initiates immediately after page load
  * Global variables initialized from localStorage before connection attempt
  * Location: app.js (DOMContentLoaded handler)

### Removed
- **beforeunload Cleanup Handler**
  * Removed aggressive session data clearing on page unload
  * Session persistence now takes priority over privacy cleanup
  * Users must explicitly leave game to clear session
  * Location: app.js (setupCleanupHandlers)

- **pageshow Reload Handler**
  * Removed forced reload when page restored from back/forward cache
  * Prevented alternating reconnection behavior
  * Location: app.js (setupCleanupHandlers)

## [v0.8.2-2512] - 2025-12-30

### Fixed
- **Code Quality Improvements** ✅ COMPLETE
  * Reduced compiler warnings from 66 to 17 (74% reduction)
  * Removed unused fields: `_currentAmount` (GuestScreenForm, HostScreenForm), `_currentIntroIndex` (FFFOfflinePanel)
  * Removed unused variables: `widthScale`, `heightScale` (TVScreenFormScalable), `ddResult` (ControlPanelForm)
  * Removed unused event: `RankingsUpdated` (FFFClientService)
  * Fixed unawaited async calls with discard operator (`_`)
  * Added null-forgiving operators (!) to verified non-null references
  * Added null-coalescing operators to unboxing operations
  * Suppressed obsolete API warning for SqlConnection with pragma directive

- **Monitor Settings Flexibility** ✅ COMPLETE
  * Changed monitor requirement from 4 to 2 monitors minimum in Settings dialog
  * Implemented dynamic checkbox logic: 2 monitors→1 screen max, 3→2 max, 4+→3 max
  * Added real-time dropdown filtering to exclude already-selected monitors
  * Screen menu items remain unrestricted for windowed mode and streaming use cases

### Removed
- **Test/Debug Code Cleanup** ✅ COMPLETE
  * Removed DSPTestDialog.cs (394 lines of test code)
  * Removed DSP Test menu item from Game menu
  * Removed associated event handler

### Changed
- **Documentation Updates**
  * Updated comment in UpdateScreenMenuItemStates() to clarify streaming/broadcast support
  * Emphasized that screen menu items have no monitor restrictions for screen capture compatibility

## [v0.8.1-2512] - 2025-12-30

### Added
- **Host Notes/Messaging System** ✅ COMPLETE
  * Implemented real-time messaging from Control Panel to Host Screen
  * Event-based architecture: MessageSent event broadcasts to all HostScreenForm instances
  * Control Panel UI: Multi-line textbox (470×64px), Send button (blue), Clear button (red)
  * Host Screen Display:
    - Explanation box: 1100×70px at position (180, 490) - displays contextual clues about answers
    - Host message box: 1100×dynamic height at (180, 570) - displays operator messages
    - Semi-transparent black background (70% opacity) with steel blue border (3px)
    - Arial 16pt Bold white text with word wrapping
  * Keyboard handling: Enter key sends message, ProcessCmdKey skips hotkeys when textbox focused
  * Preview window integration: Subscribes to MessageSent event in constructor
  * Thread-safe updates using BeginInvoke() for cross-thread communication
  * Location: ControlPanelForm.cs, HostScreenForm.cs, PreviewScreenForm.cs

- **Question Explanation System** ✅ COMPLETE
  * Updated all 80 questions in database with contextual clues about correct answers
  * Explanations provide hints/clues for hosts to use during contestant conversation
  * Example: "Who painted the Mona Lisa?" → "This Renaissance artist was also an inventor who designed flying machines"
  * Renders above host message box when question has explanation text
  * SQL Script: 01_reset_questions_table.sql updated and executed against database
  * Location: src/docs/database/01_reset_questions_table.sql

### Fixed
- **Host Screen Instance Conflicts**
  * Removed auto-open Host Screen feature from SendHostMessage() to prevent multiple instance conflicts
  * Preview window's HostScreenForm instance no longer conflicts with manually opened Host Screen
  * Messages now broadcast via event to all subscribers instead of auto-creating new windows
  * Location: ControlPanelForm.cs

- **Host Screen Rendering Order**
  * Moved DrawHostMessage() and DrawExplanation() calls before early return in RenderScreen()
  * Messages and explanations now display even when no question is loaded
  * Ensures host notes are always visible regardless of game state
  * Location: HostScreenForm.cs

### Changed
- **Control Panel Layout Refinement**
  * Repositioned checkboxes to Y=618 (vertically centered in available space)
  * Final spacing: 24px gap between textbox bottom (Y=599) and checkboxes (Y=618)
  * Improved visual balance in bottom section of control panel
  * Location: ControlPanelForm.Designer.cs

- **Host Screen Message Positioning**
  * Message box width reduced from 1560px to 1100px to avoid overlapping money tree
  * Message box ends at X=1280, 70px clearance before money tree (starts at X≈1351)
  * Positioned above question strap at Y=570 (strap at Y=650)
  * Left-aligned at X=180 for consistency with question display
  * Location: HostScreenForm.cs

## [v0.8.0-2512] - 2025-12-29

### Changed
- **Web Server Integration** ✅ COMPLETE
  * Consolidated standalone web server into main application for single-executable architecture
  * Converted MillionaireGame.Web from standalone app to class library
  * Web functionality now embedded via WebServerHost.cs in main application
  * Removed: Standalone Program.cs, appsettings.json, launchSettings.json, Swagger packages
  * Added: Microsoft.EntityFrameworkCore.Sqlite and QRCoder packages to main project
  * Result: Single MillionaireGame.exe with embedded ASP.NET Core server
  * Testing: 7/8 automated tests passing, all critical infrastructure operational
  * Location: MillionaireGame.Web.csproj, WebServerHost.cs, MillionaireGame.csproj

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

