# Release v1.0.6: First-Run Wizard & Enhanced Crash Reporting

## 🎯 Overview
Version 1.0.6 introduces the **First-Run Database Setup Wizard** for effortless database configuration and significantly enhances the **GitHub Crash Reporting** system with automated one-click submission, OAuth authentication, and privacy-focused data sanitization. This release eliminates manual database setup complexity and provides users with a professional crash reporting experience.

## ⭐ Major Features

### First-Run Database Setup Wizard
A comprehensive guided setup wizard that appears on first launch, eliminating all manual database configuration:

**Automatic Detection:**
- Detects when database configuration is missing (`sql.xml` absent)
- Installer intelligently detects existing SQL Server installations
- Detection hierarchy: Full SQL Server → Express → LocalDB
- Simplified 2-parameter system: no flag = LocalDB, `--db-type=sqlserver` = any SQL Server

**User-Friendly Interface:**
- Step-by-step guided configuration with clear instructions
- LocalDB (automatic) or SQL Server (advanced) options
- SQL Server instance enumeration with Browse button
- Connection testing with detailed feedback
- Database creation/existence verification
- Optional sample data loading (80 trivia + 44 FFF questions)

**Robust Error Handling:**
- Comprehensive validation at each step
- Automatic retry logic for transient failures
- Detailed error messages with troubleshooting guidance
- Cancel support with clean exit handling

**Integration:**
- Integrated with watchdog heartbeat system (prevents timeout during setup)
- Settings persistence via SqlSettingsManager
- Command-line parameter support from installer
- Respects existing configurations (wizard only appears when needed)

**Bug Fixes:**
- Fixed SqlConnectionSettings property setter (private → public)
- Resolved settings persistence issue
- Fixed SqlDataSourceEnumerator crashes on Windows 11 (replaced with static instance list)
- Fixed watchdog timeout killing wizard (HeartbeatService integration)

### Enhanced GitHub Crash Reporting System

**Phase 0: Hidden Watchdog Architecture**
- Converted watchdog from console app to WinExe (completely invisible)
- Added Windows Forms support for professional crash dialogs
- Implemented file-based logging system (WatchdogConsole API)
- Runs silently until crash/freeze detected
- Logs: `%LOCALAPPDATA%\TheMillionaireGame\Logs\Watchdog_*.log`

**Phase 1: Core Infrastructure**
- **GitHubOAuthManager**: OAuth Device Flow authentication with GitHub
- **SecureTokenManager**: Windows Credential Manager integration for secure token storage
- **DataSanitizer**: Comprehensive PII removal with 13 passing unit tests
  - Machine names, usernames, file paths → placeholders
  - Connection strings, API keys, secrets → `<REDACTED>`
  - Email addresses → `<EMAIL>`, IP addresses → `<IP>`
  - Environment variables redacted

**Phase 2: Professional UI Dialogs**
- **CrashReportDialog** (350+ lines):
  - User input fields: description, reproduction steps, optional email
  - Checkboxes: include system info, include logs
  - Three actions: Submit to GitHub, Save Locally, Don't Send
  - Email validation, report preview
  - Crash summary with exit code meaning
- **GitHubAuthDialog** (250+ lines):
  - Large copyable verification code display
  - Copy to clipboard and browser launch buttons
  - Real-time authentication status with progress indicator
  - Error handling with retry logic
- **ReviewReportDialog** (150+ lines):
  - Read-only preview of sanitized report
  - Formatted with section headers and metadata
  - Copy to clipboard functionality
  - Explains sanitization placeholders

**Phase 3: GitHub Integration**
- **GitHubIssueSubmitter** (360+ lines):
  - Complete GitHub REST API integration
  - Formatted markdown issues with tables and collapsible sections
  - Automatic labels: `bug`, `crash-report`, `automated`
  - 10KB report truncation for large crashes
- **Duplicate Detection**:
  - Searches for same exit code in last 7 days
  - Returns existing issue details if duplicate found
  - Prevents issue flooding
- **ProcessMonitor Integration**:
  - Complete HandleCrash() workflow rewrite
  - 7-step crash handling pipeline
  - Optional browser launch to view created issue
  - Comprehensive fallback error handling

**OAuth Configuration:**
- Registered GitHub OAuth App: "Millionaire Game Crash Reporter"
- Client ID: `Ov23li3IoDybo9YFX1wm`
- Device flow (no client secret needed - secure by design)
- Requests `public_repo` scope for issue creation only

**Testing:**
- 25/36 unit tests passing (69%)
- 13/13 DataSanitizer tests passing
- MillionaireGame.Watchdog.Tests project created
- Perfect build: 0 warnings, 0 errors

**Implementation Metrics:**
- ~2,000+ lines of production code
- 10 new files created
- 6 files modified
- 21 files changed, 4,176 insertions(+), 224 deletions(-)

### Installer Enhancements
- **Database Choice Page**: Radio buttons to select LocalDB or SQL Server Express
- **Automatic Detection**: Detects existing SQL Server installations
- **Smart Defaults**: Pre-selects detected database engine
- **Command-Line Flags**: Passes `--db-type=sqlserver` to application
- **Simplified Logic**: Two scenarios instead of three (LocalDB vs SQL Server)

## 🐛 Bug Fixes

### First-Run Wizard
- Settings property setter accessibility (private → public)
- Settings persistence after wizard completion
- SqlDataSourceEnumerator crashes on Windows 11
- Watchdog timeout during modal dialogs
- Connection string validation logic

### Database
- SQL batch execution compatibility (consolidated from 20+ batches to 3)
- Settings path consistency (LocalApplicationData)
- Database creation race conditions

## 📚 Documentation Updates

### Wiki Updates (5 Files)
- **Installation.md**: Complete First-Run Wizard documentation
- **Quick-Start-Guide.md**: Updated Step 1 with wizard workflow
- **System-Requirements.md**: LocalDB as recommended option
- **Troubleshooting.md**: First-Run Wizard troubleshooting section (5 scenarios)
- **User-Guide.md**: Crash Recovery section with automated reporting

### Developer Documentation
- Archived FIRST_RUN_WIZARD_PLAN.md (implementation complete)
- Updated UNIFIED_PLUGIN_MANAGER_PLAN.md (deferred to v1.1.0)
- Session documents:
  - SESSION_2026-01-10_FIRST_RUN_WIZARD_PHASE1.md
  - SESSION_2026-01-11_FIRST_RUN_WIZARD_COMPLETE.md
  - SESSION_2026-01-12_WATCHDOG_IMPROVEMENTS.md

## 🔄 Changed

- **Watchdog Logging**: All Console.WriteLine replaced with WatchdogConsole
- **Error Handling**: Enhanced with comprehensive try-catch and MessageBox fallbacks
- **Project Structure**: Added MillionaireGame.Watchdog.Tests to solution
- **Database Setup**: From manual SQL scripts to automated wizard
- **Crash Reporting**: From manual file submission to one-click GitHub integration

## 🎯 Testing

### First-Run Wizard Testing
- ✅ All 24 test cases passed successfully
- LocalDB automatic installation
- SQL Server instance detection and connection
- Remote SQL Server support
- Sample data loading verification
- Cancel/retry scenarios
- Error handling validation

### Crash Reporting Testing
- ✅ 13/13 DataSanitizer unit tests passing
- ✅ Perfect build (0 warnings, 0 errors)
- OAuth device flow authentication
- GitHub API integration
- Duplicate detection logic
- Dialog workflow (requires manual testing with actual crash)

## 📦 Distribution

**Installer**: `MillionaireGameSetup.exe`
- Intelligent SQL Server detection
- LocalDB bundled by default
- Optional SQL Server Express download
- First-Run Wizard auto-launches
- Sample data included (optional)

**Prerequisites:**
- .NET 8 Desktop Runtime (x64)
- Windows 10 1809+ or Windows 11
- LocalDB (bundled) or SQL Server Express/Full

## 🔗 Links

- [Installation Guide](../../wiki/Installation)
- [Quick Start Guide](../../wiki/Quick-Start-Guide)
- [Troubleshooting](../../wiki/Troubleshooting)
- [System Requirements](../../wiki/System-Requirements)
- [GitHub Repository](https://github.com/jdelgado-dtlabs/TheMillionaireGame)
- [Report Issues](https://github.com/jdelgado-dtlabs/TheMillionaireGame/issues)

## 👏 Credits

**Development**: Jean Francois Delgado ([@jdelgado-dtlabs](https://github.com/jdelgado-dtlabs))  
**Testing**: Community feedback and bug reports  
**Original Concept**: Marco Loenen ([@Macronair](https://github.com/Macronair))

---

**Release Date**: January 12, 2026  
**Build Status**: ✅ Perfect (0 warnings, 0 errors)  
**Download**: [Releases Page](https://github.com/jdelgado-dtlabs/TheMillionaireGame/releases/tag/v1.0.6)
