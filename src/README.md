# The Millionaire Game - C# Edition

![millionairebanner](https://github.com/user-attachments/assets/7cce2260-9a8b-4752-9fd8-060e4ee42450)

## 🎮 Welcome to The Millionaire Game - Modern C# Edition!

**Version**: 1.0.0 (January 2026)

This is the **modernized C# version** of The Millionaire Game, a self-written application based on the popular TV show "Who Wants to be a Millionaire". This version maintains all the functionality of the original VB.NET version while bringing it to modern .NET with improved architecture, maintainability, and a complete web-based audience participation system (WAPS).

**Build Status**: ✅ **PERFECT** (0 warnings, 0 errors)

### 🔄 Current Status

**Version 1.0.0 Features:**
- ✅ Core models and data structures
- ✅ Settings management with XML persistence
- ✅ **Unified SQL Server database** (WAPS migrated from SQLite to SQL Server)
- ✅ Question repository with full CRUD operations
- ✅ Game logic services
- ✅ Complete Control Panel UI
- ✅ Progressive answer reveal system
- ✅ **CSCore audio system with DSP** (silence detection, audio queue, crossfading)
- ✅ **Audio Settings UI** (complete configuration in Options dialog)
- ✅ **Shutdown system with progress dialog** (component-level visibility, GameConsole logging)
- ✅ **Question Editor** with CSV import/export and sound pack management
- ✅ Audio transitions with automatic silence-based progression
- ✅ **All six lifelines implemented**: 50:50, Phone-a-Friend, Ask the Audience, Double Dip, Ask the Host, Switch the Question
- ✅ Dynamic lifeline assignment via settings
- ✅ Host, Guest, and TV screen implementations
- ✅ Money tree graphical display with animations
- ✅ Dual currency support with per-level selection
- ✅ Game state management
- ✅ Monitor selection with WMI metadata
- ✅ Full-screen mode with auto-show capabilities
- ✅ **Web-Based Audience Participation System (WAPS)** with unified SQL Server backend
- ✅ **FFF (Fastest Finger First)** with mobile web interface
- ✅ Real-time SignalR communication for audience participation
- ✅ QR code joining for mobile devices
- ✅ Progressive Web App (PWA) for cross-platform support
- ✅ Device telemetry and privacy-compliant data collection
- ✅ **Workspace reorganization** (clean structure, VB.NET archived)
- ✅ **Real ATA voting integration** with actual participant votes
- ✅ **FFF Online** as independent "game within a game" feature
- ✅ **Zero-warning build** with modern best practices

**Ready for v1.0 Release:**
- 🎯 All core features complete
- 🎯 Perfect build quality (0 warnings, 0 errors)
- 🎯 Full database migration complete
- 🎯 Comprehensive documentation in place
- 🎯 Testing in progress

## 🆕 What's New in the C# Version?

### Technical Improvements
- **Modern .NET 8.0** - Latest framework with best performance
- **Async/Await Throughout** - Better responsiveness
- **Nullable Reference Types** - Fewer null reference errors
- **Clean Architecture** - Separated Core library from UI
- **Repository Pattern** - Better data access
- **Event-Driven Design** - Cleaner UI updates
- **XML Documentation** - Better IntelliSense support

### Developer Experience
- **SDK-Style Projects** - Simplified project files
- **Modern C# Features** - Pattern matching, records, etc.
- **Better Testability** - Dependency injection ready
- **Consistent Naming** - C# conventions throughout

## 📋 System Requirements

- **Windows 10/11** (Windows Forms application)
- **.NET 8.0 Runtime** or SDK
- **SQL Server Express** (2019 or later) or **LocalDB**
- **4GB RAM** minimum
- **Multiple monitors recommended** for full experience

## 🚀 Getting Started

### Building from Source

```bash
# Clone the repository
git clone https://github.com/Macronair/TheMillionaireGame.git
cd TheMillionaireGame

# Checkout the C# branch
git checkout master-csharp

# Navigate to source folder
cd src

# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run the main application
cd MillionaireGame
dotnet run
```

### For Development

Open `TheMillionaireGame.sln` in Visual Studio 2022 or later.

## 📁 Project Structure

```
src/
├── TheMillionaireGame.sln           # Solution file
├── MillionaireGame/                 # Main application
│   ├── Program.cs                   # Entry point
│   ├── Forms/                       # UI Forms
│   │   ├── ControlPanelForm.cs      # Main control panel
│   │   ├── HostScreenForm.cs        # Host display
│   │   ├── GuestScreenForm.cs       # Guest display
│   │   ├── TVScreenForm.cs          # TV/Audience display
│   │   └── ...                      # Other dialogs
│   ├── Services/                    # Application services
│   │   ├── SoundService.cs          # Audio playback with DSP
│   │   └── ScreenUpdateService.cs   # Screen coordination
│   └── lib/                         # Resources (sounds, images)
├── MillionaireGame.Core/            # Core library
│   ├── Models/                      # Data models
│   │   ├── GameState.cs
│   │   ├── Question.cs
│   │   ├── Lifeline.cs
│   │   └── ...
│   ├── Database/                    # Data access (unified SQL Server)
│   │   ├── GameDatabaseContext.cs
│   │   └── QuestionRepository.cs
│   ├── Settings/                    # Configuration
│   │   ├── ApplicationSettings.cs
│   │   └── SqlSettings.cs
│   ├── Game/                        # Game logic
│   │   ├── GameService.cs
│   │   └── LifelineManager.cs
│   └── Helpers/                     # Utility classes
├── MillionaireGame.Web/             # Web API and SignalR (WAPS)
│   ├── Hubs/                        # SignalR hubs
│   ├── Controllers/                 # API controllers
│   ├── Database/                    # WAPS database context
│   └── wwwroot/                     # Web assets
└── docs/                            # Comprehensive documentation
    ├── INDEX.md                     # Documentation navigation
    ├── START_HERE.md                # Quick start guide
    └── ...                          # Architecture, guides, sessions
```

## 🎯 Features

### Fully Implemented
- ✅ Complete Control Panel UI with game flow management
- ✅ Progressive answer reveal system (Question → A → B → C → D)
- ✅ Multiple screen support (Host, Guest, TV/Audience)
- ✅ **CSCore audio system with DSP** (silence detection, audio queue with crossfading, no manual timing)
- ✅ **Audio Settings UI** (comprehensive configuration in Options dialog)
- ✅ **Shutdown progress dialog** (real-time component tracking, GameConsole logging)
- ✅ Question-specific sound system with automatic silence-based transitions
- ✅ **All six lifelines**: 50:50, Phone-a-Friend (30s timer), Ask the Audience (2min timer), Double Dip, Ask the Host, Switch the Question
- ✅ Risk Mode (2nd safety net disabled)
- ✅ Free Safety Net Mode
- ✅ **Unified SQL Server database** (questions, FFF, ATA all in one database)
- ✅ Question Editor with full CSV import/export
- ✅ Game outcome tracking (Win/Walk Away/Wrong Answer)
- ✅ Milestone prize calculations
- ✅ Auto-show winnings feature with mutual exclusivity
- ✅ Closing sequence with cancellation support
- ✅ **Web-Based Audience Participation System (WAPS)** with real-time voting
- ✅ **FFF (Fastest Finger First)** online mode with mobile web interface

## 💾 Database

**Unified SQL Server Database** - All game data in one database:
- ✅ Questions and answer options
- ✅ FFF (Fastest Finger First) session data
- ✅ ATA (Ask the Audience) voting data
- ✅ Device telemetry and participant tracking

**Database Compatibility**:
- Uses the **same core question schema** as the VB version
- Enhanced with WAPS tables for web participation
- No migration needed for existing question databases (automated merge on first run)

## ⚙️ Configuration

### SQL Settings (`sql.xml`)
```xml
<SQLInfo>
  <UseRemoteServer>false</UseRemoteServer>
  <UseLocalDB>false</UseLocalDB>
  <LocalInstance>SQLEXPRESS</LocalInstance>
  <HideAtStart>false</HideAtStart>
</SQLInfo>
```

### Application Settings (`config.xml`)
Compatible with VB version - includes:
- Lifeline configuration
- Screen settings
- Sound file paths
- FFF server settings
- Game behavior options

## 🎵 Lifelines

All 6 lifelines are fully implemented:

1. **50:50** - Remove two wrong answers
2. **Plus One** (Phone-a-Friend) - 30 seconds to consult
3. **Ask The Audience** - Real-time audience vote via web interface
4. **Switch Question** - Get a different question at the same level
5. **Double Dip** - Two chances to answer the same question
6. **Ask The Host** - Host gives their opinion

Each can be configured for availability:
- Always available
- After Question 5
- After Question 10
- Risk Mode only

## 🏗️ Architecture Highlights

### Core Library (`MillionaireGame.Core`)
Clean, testable business logic with no UI dependencies:

```csharp
// Example: Using the Game Service
var gameService = new GameService();
gameService.ChangeLevel(5);
gameService.ChangeMode(GameMode.Risk);
gameService.UseLifeline(LifelineType.FiftyFifty);
```

### Repository Pattern
```csharp
// Example: Getting a random question
var repository = new QuestionRepository(connectionString);
var question = await repository.GetRandomQuestionAsync(
    level: 10, 
    DifficultyType.Specific
);
```

### Event-Driven UI Updates
```csharp
// Example: Responding to level changes
gameService.LevelChanged += (sender, e) => {
    UpdateMoneyDisplay(e.NewLevel);
};
```

## 🧪 Development Roadmap

### Version 1.0.0 (✅ Current - January 2026)
- [x] Project structure and core library
- [x] Complete Control Panel with game flow
- [x] Host, Guest, and TV screens
- [x] Question Editor with CSV support
- [x] Sound engine with question-specific audio and DSP
- [x] All six lifelines: 50:50, PAF, ATA, Switch, Double Dip, Ask the Host
- [x] Progressive answer reveal system
- [x] Game outcome tracking and winnings display
- [x] Closing sequence management
- [x] **Unified SQL Server database** (WAPS migration complete)
- [x] **Web-Based Audience Participation System** with real-time voting
- [x] **FFF Online** as independent feature
- [x] **Zero-warning build** (0 warnings, 0 errors)

### Version 1.0 (⏳ In Progress - Target: Q1 2026)
- [ ] End-to-end testing (all game scenarios)
- [ ] Performance testing and optimization
- [ ] Release build creation
- [ ] User documentation completion
- [ ] Installation package

### Post v1.0 (Future Enhancements)
- [ ] Multi-session support for concurrent games
- [ ] OBS/Streaming platform integration
- [ ] Elgato Stream Deck plugin
- [ ] Enhanced mobile web interface
- [ ] Advanced analytics and game statistics

## 📚 Documentation

### Quick Navigation
- **[START_HERE.md](docs/START_HERE.md)** - Development quick start and current priorities
- **[INDEX.md](docs/INDEX.md)** - Complete documentation navigation guide
- **[V1.0_RELEASE_STATUS.md](docs/V1.0_RELEASE_STATUS.md)** - Release readiness tracking

### Active Documentation
- **[CHANGELOG.md](CHANGELOG.md)** - Version history and changes
- **[DEVELOPMENT_CHECKPOINT.md](DEVELOPMENT_CHECKPOINT.md)** - Current development status
- **[docs/guides/](docs/guides/)** - How-to guides and tutorials
- **[docs/reference/](docs/reference/)** - Architecture documentation

### Historical Documentation
- **[docs/archive/](docs/archive/)** - Completed phases and planning documents
- **[docs/sessions/](docs/sessions/)** - Development session logs

## 📝 Contributing

Contributions are welcome! Please:
1. Follow C# coding conventions and project structure
2. Add XML documentation to public APIs
3. Write async methods for I/O operations
4. Maintain attribution as specified in the LICENSE file
5. Test thoroughly before submitting pull requests

## 📜 License

This project is licensed under the **MIT License** - see the [LICENSE](../LICENSE) file for details.

### Attribution Requirements

Any derivative works, modifications, or distributions must include attribution to:
- **Jean Francois Delgado** (C# modernization and development, 2024-2026)
- **Marco Loenen (Macronair)** (original VB.NET creator, 2017-2024)

## 👏 Credits

**C# Modernization & Development**: Jean Francois Delgado ([@jdelgado-dtlabs](https://github.com/jdelgado-dtlabs)) (2024-2026)  
**Original VB.NET Creator**: Marco Loenen ([@Macronair](https://github.com/Macronair)) (2017-2024)  
**Original Project**: https://github.com/Macronair/TheMillionaireGame  

This C# version is a complete rewrite that substantially extends and modernizes the original concept with enhanced features, improved architecture, and new capabilities while honoring the inspiration and foundation provided by the original work.

## 📺 Demo

Check out the original project for demo videos and screenshots. The C# version will look and function identically!

[![The Millionaire Game Demo 2024](https://img.youtube.com/vi/jj5qvg3xTR0/0.jpg)](https://youtu.be/jj5qvg3xTR0)

---

**Questions?** Check the [original README](../README.md) for gameplay instructions and setup guides.
