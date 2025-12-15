# The Millionaire Game - C# Edition

![millionairebanner](https://github.com/user-attachments/assets/7cce2260-9a8b-4752-9fd8-060e4ee42450)

## 🎮 Welcome to The Millionaire Game - Modern C# Edition!

This is the **modernized C# version** of The Millionaire Game, a self-written application based on the popular TV show "Who Wants to be a Millionaire". This version maintains all the functionality of the original VB.NET version while bringing it to modern .NET with improved architecture and maintainability.

### 🔄 Migration Status

This C# version is currently under active development. See [MIGRATION.md](MIGRATION.md) for detailed status.

**Current Status:**
- ✅ Core models and data structures
- ✅ Settings management
- ✅ Database layer
- ✅ Game logic services
- 🚧 UI forms (in progress)
- ⏳ Sound engine (pending)
- ⏳ Lifeline implementations (pending)

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
│   └── (Forms and UI to be added)
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
│   └── ...
├── MillionaireGame.QuestionEditor/  # Question editor
└── MillionaireGame.FFFGuest/        # FFF client
```

## 🎯 Features

All features from the original VB.NET version are planned for migration:

- ✅ Customizable lifelines (up to 4)
- ✅ Multiple screen support (Host, Guest, TV/Audience)
- ✅ Fastest Finger First with online features
- ✅ Risk Mode (2nd safety net disabled)
- ✅ Free Safety Net Mode
- ✅ SQL Server support (Local & Remote)
- ✅ Question Editor with CSV import/export
- 🚧 Sound engine (in progress)
- 🚧 Complete UI (in progress)

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

See [MIGRATION.md](MIGRATION.md) for detailed migration progress.

### Phase 1: Foundation (✅ Complete)
- [x] Project structure
- [x] Core models
- [x] Settings management
- [x] Database layer
- [x] Game service

### Phase 2: UI Migration (🚧 In Progress)
- [ ] Control Panel
- [ ] Host Screen
- [ ] Guest Screen  
- [ ] TV Screen
- [ ] Question Editor

### Phase 3: Advanced Features (⏳ Pending)
- [ ] Sound engine
- [ ] Lifeline implementations
- [ ] FFF networking
- [ ] Resource management

### Phase 4: Polish & Release (⏳ Pending)
- [ ] Testing
- [ ] Documentation
- [ ] Release builds
- [ ] Migration guides

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
