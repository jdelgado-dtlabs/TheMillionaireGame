# The Millionaire Game - C# Edition

![millionairebanner](https://github.com/user-attachments/assets/7cce2260-9a8b-4752-9fd8-060e4ee42450)

## 🎮 Welcome to The Millionaire Game - Modern C# Edition!

**Version**: 0.8.0-2512 (December 2025)

This is the **modernized C# version** of The Millionaire Game, a self-written application based on the popular TV show "Who Wants to be a Millionaire". This version maintains all the functionality of the original VB.NET version while bringing it to modern .NET with improved architecture, maintainability, and a complete web-based audience participation system (WAPS).

### 🔄 Current Status

**Version 0.8.0-2512 Features:**
- ✅ Core models and data structures
- ✅ Settings management with XML persistence
- ✅ Database layer with question repository
- ✅ Game logic services
- ✅ Complete Control Panel UI
- ✅ Progressive answer reveal system
- ✅ **CSCore audio system with DSP** (silence detection, audio queue, crossfading)
- ✅ **Audio Settings UI** (complete configuration in Options dialog)
- ✅ **Shutdown system with progress dialog** (component-level visibility, GameConsole logging)
- ✅ **Question Editor** with CSV import/export and sound pack management
- ✅ Audio transitions with automatic silence-based progression
- ✅ Lifeline implementations (50:50, Phone-a-Friend, Ask the Audience)
- ✅ Dynamic lifeline assignment via settings
- ✅ Host, Guest, and TV screen implementations
- ✅ Money tree graphical display with animations
- ✅ Dual currency support with per-level selection
- ✅ Game state management
- ✅ Monitor selection with WMI metadata
- ✅ Full-screen mode with auto-show capabilities
- ✅ Web-Based Audience Participation System (WAPS)
- ✅ FFF (Fastest Finger First) with mobile web interface
- ✅ Real-time SignalR communication for audience participation
- ✅ QR code joining for mobile devices
- ✅ Progressive Web App (PWA) for cross-platform support
- ✅ Device telemetry and privacy-compliant data collection
- ✅ **Workspace reorganization** (clean structure, VB.NET archived)
- 🚧 Real ATA voting integration (placeholder results currently)
- 🚧 FFF Online as independent "game within a game" feature
- 🚧 Switch the Question lifeline (pending)

**Future Vision (Post v1.0):**
- 🎯 FFF Online complete integration with graphics
- 🎯 Real ATA voting with actual participant votes
- 🎯 Multi-session support for concurrent games
- 🎯 OBS/Streaming platform integration
- 🎯 Elgato Stream Deck plugin
- 🎯 Web-based mobile interface (FFF/ATA)
- 🎯 QR code display system for audience participation

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
│   │   ├── SoundService.cs          # Audio playback
│   │   └── ScreenUpdateService.cs   # Screen coordination
│   └── lib/                         # Resources (sounds, images)
├── MillionaireGame.Core/            # Core library
│   ├── Models/                      # Data models
│   │   ├── GameState.cs
│   │   ├── Question.cs
│   │   ├── Lifeline.cs
│   │   └── ...
│   ├── Database/                    # Data access
│   │   ├── GameDatabaseContext.cs
│   │   └── QuestionRepository.cs
│   ├── Settings/                    # Configuration
│   │   ├── ApplicationSettings.cs
│   │   └── SqlSettings.cs
│   ├── Game/                        # Game logic
│   │   └── GameService.cs
│   └── Helpers/                     # Utility classes
├── MillionaireGame.QuestionEditor/  # Question editor
└── MillionaireGame.FFFGuest/        # FFF client
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
- ✅ Three lifelines: 50:50, Phone-a-Friend (30s timer), Ask the Audience (2min timer)
- ✅ Risk Mode (2nd safety net disabled)
- ✅ Free Safety Net Mode
- ✅ SQL Server support (Local & Remote)
- ✅ Question Editor with full CSV import/export
- ✅ Game outcome tracking (Win/Walk Away/Wrong Answer)
- ✅ Milestone prize calculations
- ✅ Auto-show winnings feature with mutual exclusivity
- ✅ Closing sequence with cancellation support

### In Progress
- 🚧 Switch the Question lifeline
- 🚧 Fastest Finger First networking

## 💾 Database Compatibility

The C# version uses the **same database schema** as the VB version, meaning:
- ✅ Existing question databases work without modification
- ✅ No data migration needed
- ✅ Can run alongside VB version

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

The same 6 lifelines are supported:

1. **50:50** - Remove two wrong answers
2. **Plus One** (Phone-a-Friend) - 30 seconds to consult
3. **Ask The Audience** - Virtual audience vote
4. **Switch Question** - Get a different question
5. **Double Dip** - Two chances to answer
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

### Version 0.2-2512 (✅ Current)
- [x] Project structure and core library
- [x] Complete Control Panel with game flow
- [x] Host, Guest, and TV screens
- [x] Question Editor with CSV support
- [x] Sound engine with question-specific audio
- [x] Three lifelines: 50:50, PAF, ATA
- [x] Progressive answer reveal system
- [x] Game outcome tracking and winnings display
- [x] Closing sequence management

### Version 0.3 (⏳ Planned)
- [ ] Switch the Question lifeline implementation
- [ ] Double Dip lifeline
- [ ] Ask the Host lifeline
- [ ] FFF networking and online features
- [ ] Enhanced screen transitions

### Version 1.0 (⏳ In Progress - Target: Q1 2026)
- [ ] FFF Online integration as "game within a game" feature
- [ ] Real ATA voting with actual participant data
- [ ] Hotkey mapping for lifelines (F8-F11)
- [ ] Complete CSV import/export in Question Editor
- [ ] Comprehensive end-to-end testing
- [ ] Release builds and installers
- [ ] User documentation

## 📚 Documentation

### Active Documentation
- **[CHANGELOG.md](CHANGELOG.md)** - Version history and changes
- **[DEVELOPMENT_CHECKPOINT.md](DEVELOPMENT_CHECKPOINT.md)** - Current development status
- **[docs/active/](docs/active/)** - Current planning documents
  - PROJECT_AUDIT_2025.md - Comprehensive project audit (Dec 2025)
  - PRE_1.0_FINAL_CHECKLIST.md - v1.0 completion checklist
- **[docs/reference/](docs/reference/)** - Architecture documentation
  - WEB_SYSTEM_IMPLEMENTATION_PLAN.md - WAPS architecture

### Historical Documentation
- **[ARCHIVE.md](ARCHIVE.md)** - Historical session logs (v0.2-v0.3)
- **[docs/archive/](docs/archive/)** - Completed phases and planning documents

## 📝 Contributing

Contributions to the C# migration are welcome! Please:
1. Maintain compatibility with the original VB database schema
2. Follow C# coding conventions
3. Add XML documentation to public APIs
4. Write async methods for I/O operations

## 📜 License

Same license as the original project.

## 👏 Original Credits

**Created by**: Macronair  
**Original Project**: https://github.com/Macronair/TheMillionaireGame  
**Original Language**: Visual Basic .NET  

This C# version is a loving modernization that preserves the vision and functionality of the original while bringing it to modern .NET.

## 📺 Demo

Check out the original project for demo videos and screenshots. The C# version will look and function identically!

[![The Millionaire Game Demo 2024](https://img.youtube.com/vi/jj5qvg3xTR0/0.jpg)](https://youtu.be/jj5qvg3xTR0)

---

**Questions?** Check the [original README](../README.md) for gameplay instructions and setup guides.
