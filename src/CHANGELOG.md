# Changelog

All notable changes to The Millionaire Game C# Edition will be documented in this file.

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

