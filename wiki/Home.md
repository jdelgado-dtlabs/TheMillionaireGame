# The Millionaire Game - Wiki

Welcome to The Millionaire Game documentation! This is a feature-rich Windows application that recreates the classic game show experience with modern enhancements.

![The Millionaire Game](images/logo.png)

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
- **[Stream Deck Integration](Stream-Deck-Integration)** - Control with Stream Deck *(Coming Soon)*

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
- **Multi-Monitor Support** - Separate control panel and TV screen
- **Smooth Animations** - Confetti effects, transitions, and reveals
- **Scalable Display** - Auto-scales to any resolution from 1920x1080 base

### 🔊 Immersive Audio
- **Full Sound System** - Authentic game show sounds
- **Custom Sound Sets** - Create your own sound profiles
- **Dynamic Music** - Tension builds with prize levels
- **Volume Controls** - Independent control for music, effects, and voice

### 🌐 Audience Participation (Web/Mobile)
- **Fastest Finger First** - Online FFF mode with real-time ordering
- **Ask the Audience** - Live audience voting via mobile web interface
- **Real-time Updates** - SignalR-based instant communication
- **Mobile Friendly** - Progressive Web App for cross-platform support

### 🎹 Advanced Controls
- **Keyboard Hotkeys** - Full game control from keyboard
- **Stream Deck Support** - Physical button control *(Coming Soon)*
- **Flexible Workflow** - Multiple paths to accomplish tasks

### 📊 Statistics & Analytics
- **Game Telemetry** - Track every game played
- **Excel Export** - XLSX reports with dual currency breakdown
- **Performance Metrics** - Analyze question difficulty and win rates

### 🛡️ Reliability
- **Crash Recovery** - Automatic watchdog monitoring
- **Error Logging** - Comprehensive diagnostic logs
- **Graceful Degradation** - Handles errors without disrupting gameplay

---

## 🚀 Latest Release

**Version**: 1.0.0 (Pre-release)  
**Status**: Release Candidate  
**Download**: [Releases Page](https://github.com/Macronair/TheMillionaireGame/releases)

### What's New
- Complete C# rewrite from VB.NET
- Modern .NET 8 framework
- Enhanced graphics engine
- Web audience participation
- Crash monitoring system
- Comprehensive telemetry

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
   - QR code joining for easy access
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

### Current Version: Pre-v1.0
- ✅ Core game mechanics
- ✅ Graphics engine
- ✅ Sound system
- ✅ Web integration
- ✅ Crash monitoring
- ✅ Telemetry system
- 🔄 Stream Deck integration (In Progress)
- 📝 Documentation (In Progress)

### Roadmap
- v1.0: Initial public release with complete feature set
- v1.2: Full theme system implementation
- Future: Plugin system, mobile app

---

## 🤝 Contributing

We welcome contributions! See our [Contributing Guide](Contributing) for details on:
- Code standards
- Development workflow
- Pull request process
- Bug reporting

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](https://github.com/Macronair/TheMillionaireGame/blob/master-csharp/LICENSE) file for details.

---

## 🙏 Credits

- **C# Version**: Jean Francois Delgado ([@jdelgado-dtlabs](https://github.com/jdelgado-dtlabs))
- **Original VB.NET Version**: Marco Loenen ([@Macronair](https://github.com/Macronair))
- **Sound Effects**: Various sources (see attributions)
- **Graphics**: Pre-rendered assets for 1920x1080

---

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/Macronair/TheMillionaireGame/issues)
- **Discussions**: [GitHub Discussions](https://github.com/Macronair/TheMillionaireGame/discussions)
- **Wiki**: You're reading it!

---

**Ready to get started?** Head to the [Installation Guide](Installation) and begin your journey!
