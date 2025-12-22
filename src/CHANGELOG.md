# Changelog

All notable changes to The Millionaire Game C# Edition will be documented in this file.

## [Unreleased] - 2025-12-22

### Added
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

