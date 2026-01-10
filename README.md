# The Millionaire Game

A modern C# implementation of the classic "Who Wants to Be a Millionaire?" game show experience, built with .NET 8 and Windows Forms.

**Version**: 1.0.5 (January 9, 2026)  
**Build Status**: ✅ **PERFECT** (0 warnings, 0 errors)  
**Status**: 🎉 v1.0.5 Released - Mobile/Tablet Optimizations & Critical Bug Fixes

## Quick Start

See [`src/docs/START_HERE.md`](src/docs/START_HERE.md) for complete setup and build instructions.

## Wiki Documentation

**Looking for game usage documentation?** Visit the **[Wiki](../../wiki)** for comprehensive guides:

- **[Home](../../wiki/Home)** - Overview and getting started
- **[Installation](../../wiki/Installation)** - Download and install the game
- **[Quick Start Guide](../../wiki/Quick-Start-Guide)** - Run your first game in 10 minutes
- **[User Guide](../../wiki/User-Guide)** - Complete feature documentation
- **[Troubleshooting](../../wiki/Troubleshooting)** - Solutions to common problems
- **[Stream Deck Integration](../../wiki/Stream-Deck-Integration)** - Control the game with Stream Deck

The wiki contains all information needed to **install, configure, and run the game**. The `src/docs/` folder is for **development** documentation only.

## Project Structure

- **`src/`** - Main C# solution and projects (v1.0.5)
  - `MillionaireGame/` - Main game application (Windows Forms)
  - `MillionaireGame.Core/` - Core business logic and services
  - `MillionaireGame.Web/` - Web API and SignalR backend for WAPS
  - `MillionaireGame.Watchdog/` - Crash monitoring and diagnostic reporting
- **`installer/`** - Inno Setup installer script and assets

## Documentation

### For Users & Operators
**[📖 Visit the Wiki](../../wiki)** for complete game usage documentation, installation guides, troubleshooting, and feature tutorials.

### For Developers
Development documentation is available in the [`src/docs/`](src/docs/) folder:

- **[START_HERE.md](src/docs/START_HERE.md)** - Development quick start guide
- **[INDEX.md](src/docs/INDEX.md)** - Complete documentation navigation
- **[V1.0_RELEASE_STATUS.md](src/docs/V1.0_RELEASE_STATUS.md)** - Release readiness tracking

Browse by category:
- **guides/** - How-to guides and tutorials
- **reference/** - Architecture and technical documentation
- **sessions/** - Development session logs
- **archive/** - Historical documentation

## Features

### Core Game Experience
- ✅ Classic game show format with authentic sound and visuals
- ✅ Fastest Finger First contestant selection (online and offline modes)
- ✅ **All six lifelines**: Phone-a-Friend, Ask the Audience, 50:50, Double Dip, Ask the Host, Switch the Question
- ✅ Multi-screen support (Host control panel, TV display, audience view)
- ✅ Progressive answer reveal system with automatic audio transitions
- ✅ Risk Mode and Free Safety Net options
- ✅ Dual currency support with per-level customization
- ✅ **Stream Deck Module 6 integration** for physical button control (answer lock-in and reveal)
- ✅ Host notes and messaging system with keyboard shortcuts
- ✅ Winner confetti animation (physics-based particle system)

### Web-Based Audience Participation (WAPS)
- ✅ Real-time audience voting via mobile web interface
- ✅ FFF (Fastest Finger First) online mode with all-play participation
- ✅ SignalR-based real-time communication with mid-game join support
- ✅ QR code joining for easy mobile access
- ✅ Progressive Web App (PWA) for cross-platform support
- ✅ Enhanced mobile/tablet detection with on-screen debug panel
- ✅ Device telemetry and privacy-compliant data collection
- ✅ Dynamic container height with responsive design
- ✅ mDNS hostname resolution (wwtbam.local)

### Technical Excellence
- ✅ **CSCore audio engine** with DSP (silence detection, crossfading, audio queue)
- ✅ **Unified SQL Server database** with simplified 4-level difficulty system (80 main + 44 FFF questions)
- ✅ **Watchdog crash monitoring** with automatic restart and diagnostic reporting
- ✅ Question database editor with CSV import/export
- ✅ Comprehensive settings management
- ✅ **Zero-warning build** (0 warnings, 0 errors)
- ✅ File-first logging architecture with async queue processing
- ✅ Optimized preview screen rendering with caching

## Technology Stack

- **.NET 8.0-windows** - Modern framework with Windows Forms
- **CSCore 1.2.1.2** - Professional audio engine with DSP capabilities
- **SignalR 8.0.11** - Real-time web communication for WAPS
- **Microsoft.Data.SqlClient 5.2.2** - Modern SQL Server connectivity
- **Entity Framework Core 8.0.11** - Database ORM
- **SQL Server Express** - Unified database for all game data (questions, FFF, ATA, WAPS)
- **StreamDeckSharp (Custom)** - Module 6 support with custom HID driver

## Development Status

✅ **Version 1.0.5 - RELEASED** (January 9, 2026)

All core features complete, perfect build quality (0 warnings, 0 errors), production-ready installer available.

**Latest Release Highlights**:
- Mobile/tablet optimization with enhanced device detection
- FFF online system fixes (rankings display, all-play participation)
- ATA online system fixes (offline mode detection, vote timeout)
- Multi-monitor support restored with safe async detection
- Web state synchronization for mid-game joiners
- mDNS hostname resolution (A/AAAA records)
- Answer letter wrapping fixes across all screens
- Stream Deck images embedded as resources
- 7 automatic database migrations

**Key Achievements**:
- 18 major features implemented and tested
- Simplified 4-level question difficulty system
- Stream Deck Module 6 integration with custom HID driver
- Watchdog crash monitoring and auto-recovery
- Web-Based Audience Participation System (WAPS)
- Comprehensive documentation and wiki

See [`src/docs/active/V1.0_RELEASE_STATUS.md`](src/docs/active/V1.0_RELEASE_STATUS.md) for complete feature list and [`src/CHANGELOG.md`](src/CHANGELOG.md) for detailed release notes.

## Contributing

This is a personal project, but feedback and suggestions are welcome via Issues.

## License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

## Credits & Attribution

**C# Modernization & Development**: Jean Francois Delgado ([@jdelgado-dtlabs](https://github.com/jdelgado-dtlabs)) (2024-2026)  
**Original VB.NET Creator**: Marco Loenen ([@Macronair](https://github.com/Macronair)) (2017-2024)  

This C# version represents a complete rewrite and substantial modernization of the original VB.NET implementation. While maintaining the core concept and inspiration from the original work, this version features:

- Complete architectural redesign with modern .NET 8.0
- Unified SQL Server database (migrated from SQLite)
- Web-Based Audience Participation System (WAPS)
- Advanced audio engine with DSP capabilities
- All six lifelines fully implemented
- Real-time SignalR communication
- Progressive Web App (PWA) for mobile devices
- And many other enhancements

**Original Project**: [Macronair/TheMillionaireGame](https://github.com/Macronair/TheMillionaireGame)

---

**Ready to dive in?** See [`src/docs/START_HERE.md`](src/docs/START_HERE.md) for development setup and current priorities.
