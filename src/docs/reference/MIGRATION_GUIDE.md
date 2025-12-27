# Migration Guide: VB.NET to C#

This guide provides detailed information about migrating from the VB.NET version to the C# version of The Millionaire Game.

## Table of Contents
1. [Why Migrate to C#?](#why-migrate-to-c)
2. [What's Different?](#whats-different)
3. [Migration Approach](#migration-approach)
4. [Key Architectural Changes](#key-architectural-changes)
5. [Code Comparison](#code-comparison)
6. [Data Migration](#data-migration)
7. [For Developers](#for-developers)
8. [Troubleshooting](#troubleshooting)

## Why Migrate to C#?

### Technical Benefits
1. **Modern Language Features**
   - Nullable reference types for better null safety
   - Pattern matching for cleaner code
   - Records for immutable data
   - Enhanced LINQ support

2. **Better Performance**
   - .NET 8.0 runtime optimizations
   - Faster startup times
   - Reduced memory footprint
   - Improved garbage collection

3. **Improved Development Experience**
   - More consistent language syntax
   - Better IDE support and refactoring tools
   - Larger C# community and resources
   - More modern libraries and packages

4. **Long-term Maintainability**
   - C# is the primary language for .NET
   - More active development and updates
   - Easier to find C# developers
   - Better documentation

### Functional Benefits
- **100% Feature Parity**: All VB features will be migrated
- **Database Compatibility**: Uses the same database schema
- **Settings Compatibility**: Config files work in both versions
- **Gradual Migration**: Can run both versions during transition

## What's Different?

### Language Syntax
```vb
' VB.NET
Public Class Game
    Public Shared level As Integer = 0
    
    Public Shared Sub ChangeLevel(ByVal newLvl As Integer)
        Game.level = newLvl
        SetValues()
    End Sub
End Class
```

```csharp
// C#
public class GameService
{
    private readonly GameState _gameState;
    
    public void ChangeLevel(int newLevel)
    {
        _gameState.CurrentLevel = newLevel;
        UpdateMoneyValues();
    }
}
```

### Project Structure

**VB.NET Version:**
```
Het DJG Toernooi/
  Source_Scripts/
    Classes/
    DatabaseAndSettings/
    Lifelines/
    Sound Engine/
  Windows/
  My Project/
```

**C# Version:**
```
src/
  MillionaireGame.Core/      # Business logic
    Models/
    Database/
    Settings/
    Game/
    Lifelines/
    Sound/
  MillionaireGame/           # UI application
    Forms/
    Controls/
```

### Key Architectural Changes

1. **Separation of Concerns**
   - VB: All code in one project
   - C#: Core library separate from UI

2. **Dependency Injection**
   - VB: Static classes everywhere
   - C#: Instance-based services

3. **Async/Await**
   - VB: Synchronous database calls
   - C#: Async operations throughout

4. **Event Handling**
   - VB: Direct form manipulation
   - C#: Event-driven service layer

## Migration Approach

### Phase 1: Core Foundation ✅
**What we migrated:**
- Data models (Question, Player, Lifeline, etc.)
- Settings management
- Database access layer
- Core game logic

**Mapping:**
| VB.NET File | C# Equivalent |
|-------------|---------------|
| `Question.vb` | `Models/Question.cs` |
| `Game.vb` | `Game/GameService.cs` |
| `Data.vb` | `Database/GameDatabaseContext.cs` |
| `QDatabase.vb` | `Database/QuestionRepository.cs` |
| `ApplicationSettings.vb` | `Settings/ApplicationSettings.cs` |
| `SQLInfo.vb` | `Settings/SqlSettings.cs` |

### Phase 2: UI Migration 🚧
**In Progress:**
- Control Panel → `Forms/ControlPanelForm.cs`
- Host Screen → `Forms/HostScreenForm.cs`
- Guest Screen → `Forms/GuestScreenForm.cs`
- TV Screen → `Forms/TVScreenForm.cs`

### Phase 3: Advanced Features ⏳
**Pending:**
- Sound Engine
- Lifeline implementations
- FFF networking
- Question Editor

## Code Comparison

### Creating a Question

**VB.NET:**
```vb
Dim newQ As New QDatabase
newQ.newQuestion()

' Direct UI manipulation
ControlPanel.txtQuestion.Text = question
HostScreen.txtQuestion.Text = question
GuestScreen.txtQuestion.Text = question
```

**C#:**
```csharp
var repository = new QuestionRepository(connectionString);
var question = await repository.GetRandomQuestionAsync(level);

// Event-driven UI update
QuestionLoaded?.Invoke(this, new QuestionLoadedEventArgs(question));
```

### Changing Game Level

**VB.NET:**
```vb
Public Shared Sub ChangeLevel(ByVal newLvl As Integer)
    Game.level = newLvl
    ControlPanel.nmrLevel.Value = newLvl
    SetValues()
    ControlPanel.txtCorrect.Text = Game.varCorrect
End Sub
```

**C#:**
```csharp
public void ChangeLevel(int newLevel)
{
    var oldLevel = _gameState.CurrentLevel;
    _gameState.CurrentLevel = newLevel;
    UpdateMoneyValues();
    
    LevelChanged?.Invoke(this, 
        new GameLevelChangedEventArgs(oldLevel, newLevel));
}
```

### Using a Lifeline

**VB.NET:**
```vb
If LifelineManager.Lifeline1.IsUsed = False Then
    LifelineManager.Lifeline1.IsUsed = True
    Lifeline5050.Activate()
End If
```

**C#:**
```csharp
try 
{
    gameService.UseLifeline(LifelineType.FiftyFifty);
    // Handle successful use
}
catch (InvalidOperationException ex)
{
    // Lifeline already used or not available
    MessageBox.Show(ex.Message);
}
```

## Data Migration

### Database Schema
**No changes needed!** The C# version uses the exact same schema:

```sql
-- Same tables work for both versions
CREATE TABLE questions (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Question NVARCHAR(MAX),
    A NVARCHAR(500),
    B NVARCHAR(500),
    C NVARCHAR(500),
    D NVARCHAR(500),
    CorrectAnswer NVARCHAR(1),
    -- ... etc
)
```

### Configuration Files

**config.xml** - Works in both versions:
```xml
<AppSettings>
  <TotalLifelines>4</TotalLifelines>
  <Lifeline1>5050</Lifeline1>
  <!-- ... same structure ... -->
</AppSettings>
```

**sql.xml** - Works in both versions:
```xml
<SQLInfo>
  <UseRemoteServer>false</UseRemoteServer>
  <UseLocalDB>false</UseLocalDB>
  <!-- ... same structure ... -->
</SQLInfo>
```

### Migration Steps

1. **No database changes needed**
   - C# version reads same database
   - Can run both versions concurrently

2. **Settings are compatible**
   - Both read config.xml
   - Both read sql.xml
   - No conversion needed

3. **Sound files stay in place**
   - Same folder structure
   - Same file references
   - No file moves required

## For Developers

### Setting Up C# Development

1. **Install Prerequisites**
   ```bash
   # Install .NET 8.0 SDK
   winget install Microsoft.DotNet.SDK.8
   
   # Or download from:
   # https://dotnet.microsoft.com/download/dotnet/8.0
   ```

2. **Clone and Build**
   ```bash
   git clone https://github.com/Macronair/TheMillionaireGame.git
   cd TheMillionaireGame
   git checkout master-csharp
   cd src
   dotnet restore
   dotnet build
   ```

3. **Open in IDE**
   - Visual Studio 2022: Open `TheMillionaireGame.sln`
   - VS Code: Open `src` folder with C# extension
   - JetBrains Rider: Open solution file

### Contributing to Migration

**To migrate a VB form:**

1. Create corresponding C# form:
   ```bash
   cd src/MillionaireGame/Forms
   # Create NewForm.cs and NewForm.Designer.cs
   ```

2. Convert VB logic to C#:
   - Remove `Shared` → use dependency injection
   - `ByVal` parameters → just `type name`
   - `Sub` → `void` or `async Task`
   - `Function` → return type

3. Use services instead of static classes:
   ```csharp
   // Inject services in constructor
   public ControlPanelForm(GameService gameService)
   {
       _gameService = gameService;
       _gameService.LevelChanged += OnLevelChanged;
   }
   ```

4. Update MIGRATION.md with progress

### Code Style Guidelines

**Naming Conventions:**
- Classes: `PascalCase`
- Methods: `PascalCase`
- Private fields: `_camelCase`
- Properties: `PascalCase`
- Local variables: `camelCase`

**Patterns to Use:**
- Dependency injection for services
- Async/await for I/O operations
- Events for UI updates
- Repositories for data access
- Services for business logic

**Patterns to Avoid:**
- Static classes (except extensions/utilities)
- Direct form-to-form communication
- Synchronous database calls
- Late binding

## Troubleshooting

### Build Issues

**Problem:** "SDK not found"
```
Solution: Install .NET 8.0 SDK
```

**Problem:** "Nullable reference types error"
```csharp
// Solution: Add null checks or use null-forgiving operator
string? value = GetValue();
if (value != null)
{
    UseValue(value);
}
```

### Runtime Issues

**Problem:** Database connection fails
```
Solution: Check SQL Server is running and sql.xml is configured
```

**Problem:** Settings not loading
```csharp
// Solution: Ensure config.xml exists in application directory
var settings = new ApplicationSettingsManager();
settings.LoadSettings(); // Creates default if missing
```

### Migration Questions

**Q: Can I use my existing database?**  
A: Yes! The C# version uses the same schema.

**Q: Will my sounds/graphics work?**  
A: Yes! Same file structure and references.

**Q: Can I run both versions?**  
A: Yes! They can coexist perfectly.

**Q: What about my custom questions?**  
A: They'll work without any changes.

**Q: How long until UI is done?**  
A: Check MIGRATION.md for current status. UI migration is actively in progress.

## Getting Help

- **GitHub Issues**: Report bugs or ask questions
- **Discussions**: Share ideas for the migration
- **Original README**: Game setup and usage info

## Additional Resources

- [C# Programming Guide](https://docs.microsoft.com/en-us/dotnet/csharp/)
- [.NET 8.0 Documentation](https://docs.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)
- [Windows Forms in .NET](https://docs.microsoft.com/en-us/dotnet/desktop/winforms/)
- [Async Programming](https://docs.microsoft.com/en-us/dotnet/csharp/async)

---

**Questions about migration?** Open an issue on GitHub!
