# Release v1.0.5: Mobile/Tablet Optimization & Critical Bug Fixes

## 🎯 Overview
Version 1.0.5 focuses on **mobile/tablet optimization**, **critical bug fixes** for the Fastest Finger First (FFF) and Ask The Audience (ATA) online systems, **multi-monitor support restoration**, and **comprehensive web state synchronization**. This release enhances the audience participation experience with improved device detection, responsive design fixes, and diagnostic tools for live show troubleshooting.

## ⭐ Major Features

### Multi-Monitor Support Restored
- Screens tab re-enabled with safe async monitor detection
- MonitorInfoService with 2-second timeout protection and comprehensive error handling
- UID-based monitor ordering to match Windows display numbers
- Lazy initialization - monitors load only when Screens tab selected
- Monitor assignments persist across sessions
- Screens auto-open at startup when full screen enabled
- Single WMI query optimization (consolidated from 3 parallel queries)
- Fixed dropdown enable/disable logic - checkboxes properly disable monitor selectors when full screen active

### Enhanced Mobile/Tablet Detection
- Multi-strategy device detection (UA patterns, Android-specific checks, screen size heuristics >=768px, touch + size combination)
- Proper classification of Android tablets (previously detected as Desktop)
- Mobile features now activate correctly on all tablet devices
- Console logging for each detection path

### On-Screen Debug Panel
- Fixed-position diagnostic overlay showing device information (device type, screen resolution, touch support, wake lock status, user agent)
- Auto-hides after 10 seconds with manual close button
- Only displays on Mobile/Tablet devices
- Valuable for troubleshooting audience member connection issues during live shows

### Mobile Container Optimization
- Dynamic height calculation using actual window.innerHeight - 40px
- More reliable than CSS vh/dvh units across mobile browsers
- Automatic recalculation on resize and orientation change
- Vertical centering with overflow support
- Internal scrolling when content exceeds calculated height

### Web State Synchronization (NEW)
- **Mid-Game Joiners**: Clients joining during active game phases now see current state instead of lobby
- **ATA Intro Sync**: Participants joining during ATA intro see question/answers with disabled buttons
- **ATA Voting Sync**: Participants joining during voting see active question with vote buttons enabled
- **VotingStartTime Tracking**: New database field separates intro phase (120s) from voting window (60s)
- **Session Persistence**: LIVE session created on web server startup (prevents auto-create race conditions)
- **Smart Join Logic**: Frontend only shows connected screen if actually in lobby state

### mDNS Hostname Resolution (NEW)
- **A/AAAA Records**: Added IPv4 (A) and IPv6 (AAAA) records for hostname resolution
- **Complete mDNS**: Previously only had SRV/TXT service discovery records
- **`wwtbam.local` Resolution**: Clients can now resolve hostname to actual IP addresses
- **120-second TTL**: Appropriate record caching for local network

## 🐛 Critical Bug Fixes

### FFF Online System Fixes
- **Rankings Display**: Fixed FFF rankings always showing 0 - changed desktop client to call correct hub method (`GetFFFResults` vs non-existent `CalculateRankings`)
- **Player Pre-Selection Removed**: Eliminated confusing `SelectFFFPlayers` endpoint that randomly picked 8 players before question shown
- **All Can Play**: All participants in lobby can now answer FFF questions, top 8 fastest correct answers shown in rankings
- **Host Intro Broadcast**: Fixed Host Intro not broadcasting to waiting lobby - made sessionId nullable, broadcasts to all clients when null

### ATA Online System Fixes (CRITICAL)
- **ATA Mode Detection**: Fixed ATA always showing offline mode despite participants in lobby
- **LIVE Session Refactor**: All ATA methods now use direct `LIVE` session reference instead of querying for Active status
- **Session Status Compatibility**: Removed incompatible Active session queries (sessions change status throughout game lifecycle)
- **Startup Cleanup**: Reset LIVE session to Active status on web server start for proper state management
- **Vote Timeout Fix**: Votes no longer rejected as "70+ seconds late" when submitted within 2-3 seconds
  - Now uses `VotingStartTime` (60s window) instead of `QuestionStartTime` (120s intro phase)
  - Prevents false timeout rejections

### Answer Letter Wrapping Fix
- Increased answer letter rendering width from 60 to 80 pixels across all three screen forms (Host, Guest, TV)
- Prevents letter and colon separation ("A:" → "A\n:") during text wrapping
- Fixed graphical glitches where letters wrapped incorrectly at scaled resolutions

### FFF No-Winner Scenario Handling
- Fixed button enable logic when no participants answered correctly
- Orange button color as visual indicator for no-winner scenario
- Proper retry flow with reset to QuestionReady state
- Explicit button state management to prevent re-clicking

### Web UI Improvements
- Fixed submit button appearing greyed but clickable on new questions
- Auto scroll-to-top on screen transitions prevents hidden content
- Dynamic container height prevents viewport overflow on small screens
- Enhanced wake lock debugging with comprehensive error logging

### Monitor Detection Safety
- All WMI queries now async with timeout protection
- No more UI thread blocking during monitor detection
- Graceful degradation on WMI failure
- Comprehensive error handling throughout

## 🛠️ Deployment Improvements

- **Stream Deck Images Embedded**: All 12 Stream Deck PNG files now part of executable
- **Telemetry Migration Integrated**: Migration 00007 creates telemetry tables automatically
- **Zero Manual Setup**: No separate SQL scripts to run
- **Clean Build**: Zero compiler warnings

## 📥 Installation

### Requirements
- .NET 8 Desktop Runtime (x64)
- Windows 10/11 (64-bit)
- SQL Server LocalDB or SQL Server Express

### New Installation
1. Run `MillionaireGameSetup-v1.0.5.exe`
2. Follow installation wizard
3. Launch application

### Upgrade from v1.0.1+
- Application automatically migrates database schema (including new VotingStartTime field)
- Settings and question data preserved
- No manual intervention required

## 📝 What's Changed

**Full Changelog**: https://github.com/Macronair/TheMillionaireGame/compare/v1.0.1...v1.0.5

### Commits Summary
- 20+ commits merged to master
- Multi-monitor support restoration with dropdown UX fixes
- Mobile/tablet detection enhancements
- **FFF online system fixes** (rankings, pre-selection, Host Intro broadcast)
- **ATA online system fixes** (offline mode detection, vote timeout, LIVE session refactor)
- **Web state synchronization** (mid-game joiners, ATA intro/voting sync)
- **mDNS hostname resolution** (A/AAAA records)
- Answer letter wrapping fixes
- FFF no-winner flow fixes
- Container overflow and scrolling fixes
- Build system improvements

## 📚 Documentation

- Updated User Guide with mobile/tablet information
- Enhanced troubleshooting documentation
- Migration system documentation (now includes 7 automatic migrations)

## 👏 Credits

**Development**: Jean Francois Delgado  
**Based on Original Work**: Marco Loenen (Macronair)

---

**Release Date**: January 9, 2026
