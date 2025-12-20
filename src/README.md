# The Millionaire Game - C# Edition

![millionairebanner](https://github.com/user-attachments/assets/7cce2260-9a8b-4752-9fd8-060e4ee42450)

## 🎮 Welcome to The Millionaire Game - Modern C# Edition!

**Version**: 0.2-2512 (December 2025)

This is the **modernized C# version** of The Millionaire Game, a self-written application based on the popular TV show "Who Wants to be a Millionaire". This version maintains all the functionality of the original VB.NET version while bringing it to modern .NET with improved architecture and maintainability.

### 🔄 Current Status

**Version 0.2-2512 Features:**
- ✅ Core models and data structures
- ✅ Settings management
- ✅ Database layer
- ✅ Game logic services
- ✅ Complete Control Panel UI
- ✅ Progressive answer reveal system
- ✅ Sound engine (Question-specific audio system)
- ✅ Audio transitions with 500ms timing
- ✅ Lifeline implementations (50:50, Phone-a-Friend, Ask the Audience)
- ✅ Host, Guest, and TV screen implementations
- ✅ Game state management
- ✅ Question Editor with CSV import/export
- 🚧 Switch the Question lifeline (pending)
- 🚧 FFF networking (pending)

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
- ✅ Question-specific sound system with audio transitions
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

### Version 1.0 (⏳ Future)
- [ ] Feature parity with VB.NET version
- [ ] Comprehensive testing
- [ ] Release builds and installers
- [ ] User documentation

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
