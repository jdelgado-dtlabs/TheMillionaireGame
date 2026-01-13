# The Millionaire Game - Wiki

Welcome to The Millionaire Game documentation! This is a feature-rich Windows application that recreates the classic game show experience with modern enhancements.

<img src="images/logo.png" alt="The Millionaire Game" width="400">

## 🎮 Quick Navigation

### Getting Started
- **[System Requirements](System-Requirements)** - Check if your system is ready
- **[Installation](Installation)** - Download and install the game
- **[Quick Start Guide](Quick-Start-Guide)** - Get playing in 5 minutes

### For Developers
- **[Building from Source](Building-from-Source)** - Compile and run the project
- **[Contributing](Contributing)** - Join the development effort

### User Documentation
- **[User Guide](User-Guide)** - Complete guide to using the application
- **[Troubleshooting](Troubleshooting)** - Solutions to common issues

### Advanced Topics
- **[Configuration Files](Configuration-Files)** - Customize settings
- **[Architecture](Architecture)** - Technical overview
- **[Stream Deck Integration](Stream-Deck-Integration)** - Host controls with 6 button module

---

## ✨ Key Features

### 🎯 Complete Game Experience
- **Fastest Finger First** - Classic qualifying round with up to 8 contestants
- **Main Game** - Full 15-question money ladder
- **Multiple Lifelines** - 50:50, Phone a Friend, Ask the Audience, and more
- **Safety Nets** - Configurable milestone amounts
- **Dual Currency Support** - Run games with two different currencies simultaneously

### 🎨 Professional Presentation
- **6 Background Options** - Pre-rendered backgrounds for TV screen
- **Multi-Monitor Support** - Screen assignments at launch with persistent settings
- **Smooth Animations** - Confetti effects, transitions, and reveals
- **Scalable Display** - Auto-scales to any resolution from 1920x1080 base

### 🔊 Immersive Audio
- **Full Sound System** - Authentic game show sounds
- **Custom Sound Sets** - Create your own sound profiles
- **Dynamic Music** - Tension builds with prize levels
- **Volume Controls** - Independent control for music, effects, and voice

### 🌐 Audience Participation (Web/Mobile)
- **Fastest Finger First** - Real-time ordering via web interface
- **Ask the Audience** - Web-based voting platform
- **Real-time Updates** - SignalR-based instant communication
- **Mobile Friendly** - Progressive Web App for cross-platform support

### 🎹 Advanced Controls
- **Keyboard Hotkeys** - Full game control from keyboard
- **Stream Deck Support** - Host controls utilizing 6 button module

### 📊 Statistics & Analytics
- **Game Telemetry** - Track every game played
- **Excel Export** - XLSX reports with dual currency breakdown
- **Performance Metrics** - Analyze question difficulty and win rates

### 🛡️ Reliability
- **First-Run Wizard** - Guided database setup on first launch
- **Crash Recovery** - Automatic watchdog with GitHub issue creation
- **GitHub Integration** - OAuth-authenticated crash reporting with duplicate detection
- **Data Privacy** - Automatic sanitization of sensitive information
- **Error Logging** - Comprehensive diagnostic logs
- **Graceful Degradation** - Handles errors without disrupting gameplay

---

## 🚀 Latest Release

**Version**: 1.0.6  
**Release Date**: January 12, 2026  
**Status**: Stable Release  
**Download**: [Releases Page](https://github.com/jdelgado-dtlabs/TheMillionaireGame/releases)

### What's New in v1.0.6
- **First-Run Wizard** - Automated database setup with LocalDB/SQL Server choice
- **GitHub Crash Reporting** - OAuth authentication with automated issue submission
- **Data Sanitization** - Automatic removal of sensitive data from crash reports
- **Enhanced Watchdog** - Improved freeze detection and process recovery
- **Bundled LocalDB** - SqlLocalDB.msi included in installer (no internet required)
- **24 Test Cases** - Complete wizard validation across 5 pages
- **Settings Migration** - Database-backed configuration (XML removed)

---

## 📖 Quick Start

1. **[Check System Requirements](System-Requirements)** - Ensure compatibility
2. **[Download & Install](Installation)** - Get the latest release
3. **[Follow Quick Start Guide](Quick-Start-Guide)** - Configure and play
4. **[Explore User Guide](User-Guide)** - Learn advanced features

---

## 🎬 Demo

*Screenshots and demo videos coming soon*

The application features multiple display interfaces for different roles:

### Desktop Application Views

1. **Control Panel** - Operator's command center with full game control
2. **TV Screen** - Main display for contestants and audience
3. **Host Screen** - Private host view with answers and contestant info
4. **Guest Screen** - Additional audience display
5. **FFF Offline Mode** - Local Fastest Finger First interface
6. **FFF Online Mode** - Web-based FFF with live contestant feeds

### Mobile/Web Application

7. **Mobile Web Interface** - Progressive Web App for audience participation
   - **FFF Online** - Participate in Fastest Finger First rounds
   - **Ask the Audience** - Vote on answer choices in real-time
   - Direct URL joining (IP address and port)
   - Live result updates via SignalR

---

## 💡 Use Cases

- **Game Show Events** - Host live game show events
- **Fundraisers** - Engaging entertainment for charity events
- **Parties & Celebrations** - Interactive entertainment
- **Education** - Gamified learning and quizzes
- **Corporate Training** - Fun team building exercises

---

## 🛠️ Technology Stack

- **Framework**: .NET 8 (Windows Desktop)
- **UI**: Windows Forms with GDI+ rendering
- **Database**: SQL Server (LocalDB or full server)
- **Web**: ASP.NET Core with SignalR for real-time updates
- **Graphics**: Pre-rendered 1920x1080 assets with auto-scaling
- **Audio**: CSCore library with DSP capabilities

---

## 📋 Project Status

### Current Version: v1.0.6 (Stable)
- ✅ Core game mechanics
- ✅ Graphics engine
- ✅ Sound system
- ✅ Web integration with mobile optimization
- ✅ First-run setup wizard
- ✅ GitHub crash reporting
- ✅ Crash monitoring with automated issue creation
- ✅ Telemetry system
- ✅ Stream Deck integration
- ✅ Multi-monitor support
- ✅ Complete documentation

### Roadmap
- v1.1: ETC Ion lighting console integration
- v1.2: Yamaha TF audio console plugin
- v1.3: Unified plugin manager architecture
- v1.5: Full theme system implementation

---

## 🤝 Contributing

We welcome contributions! See our [Contributing Guide](Contributing) for details on:
- Code standards
- Development workflow
- Pull request process
- Bug reporting

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](https://github.com/jdelgado-dtlabs/TheMillionaireGame/blob/master-csharp/LICENSE) file for details.

---

## 🙏 Credits

- **C# Version**: Jean Francois Delgado ([@jdelgado-dtlabs](https://github.com/jdelgado-dtlabs)) - Complete rewrite and modernization
- **Original VB.NET Concept**: Marco Loenen - Original VB.NET implementation (no longer in this repository)
- **Sound Effects**: Various sources (see attributions)
- **Graphics**: Pre-rendered assets for 1920x1080

> **Note**: This C# version is a complete independent rewrite by Jean Francois Delgado. The original VB.NET version was created by Marco Loenen and has been archived.

---

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/jdelgado-dtlabs/TheMillionaireGame/issues)
- **Discussions**: [GitHub Discussions](https://github.com/jdelgado-dtlabs/TheMillionaireGame/discussions)
- **Wiki**: You're reading it!

---

**Ready to get started?** Head to the [Installation Guide](Installation) and begin your journey!
