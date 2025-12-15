# The Millionaire Game - C# Migration

## Overview
This is the C# migration of The Millionaire Game, originally written in Visual Basic .NET. This migration modernizes the codebase while maintaining all original functionality.

## Project Structure

```
src/
├── MillionaireGame/                    # Main WinForms application
├── MillionaireGame.Core/               # Core business logic and models
│   ├── Models/                         # Data models
│   ├── Database/                       # Database access layer
│   ├── Settings/                       # Configuration and settings
│   ├── Game/                           # Game logic
│   ├── Lifelines/                      # Lifeline implementations
│   └── Sound/                          # Sound engine
├── MillionaireGame.QuestionEditor/     # Question editor application
└── MillionaireGame.FFFGuest/           # Fastest Finger First client
```

## Technology Stack

- **.NET 8.0** - Modern, cross-platform .NET framework
- **Windows Forms** - UI framework (maintaining compatibility with original design)
- **SQL Server** - Database (compatible with original LocalDB/SQL Express)
- **C# 12** - Latest C# language features

## Migration Status

### ✅ Completed
- [x] Project structure and solution setup
- [x] Core data models (Question, Player, Lifeline, GameState, etc.)
- [x] Settings management (ApplicationSettings, SqlSettings)
- [x] .NET 8.0 project files with proper references

### 🚧 In Progress
- [ ] Database access layer
- [ ] Game logic classes
- [ ] UI forms migration

### ⏳ Pending
- [ ] Sound engine
- [ ] Lifeline implementations
- [ ] Fastest Finger First networking
- [ ] Control Panel UI
- [ ] Host/Guest/TV screens
- [ ] Question Editor
- [ ] Testing and validation
- [ ] Resource files migration

## Key Improvements Over Original

1. **Modern C# Features**
   - Nullable reference types for better null safety
   - Record types for immutable data
   - Pattern matching
   - Enhanced async/await support

2. **Better Architecture**
   - Clear separation of concerns (Core library vs UI)
   - Dependency injection ready
   - More testable code structure

3. **Improved Type Safety**
   - Enums instead of magic numbers
   - Strong typing throughout
   - No late binding

4. **Performance**
   - Modern .NET runtime optimizations
   - Better memory management
   - Faster startup times

5. **Maintainability**
   - Consistent naming conventions (PascalCase, camelCase)
   - XML documentation comments
   - Modern project format (SDK-style)

## Building the Project

### Prerequisites
- Visual Studio 2022 or later
- .NET 8.0 SDK
- SQL Server Express or LocalDB

### Build Commands
```bash
cd src
dotnet restore
dotnet build
```

### Running the Application
```bash
cd src/MillionaireGame
dotnet run
```

## Migration Notes

### Breaking Changes from VB Version
None intended - the C# version maintains full compatibility with existing databases and configuration files.

### Database Compatibility
The C# version uses the same database schema as the VB version, ensuring smooth transition.

### Configuration Files
- `config.xml` - Application settings (compatible with VB version)
- `sql.xml` - SQL connection settings (compatible with VB version)

## Development Guidelines

### Code Style
- Use C# naming conventions
- Add XML documentation comments to public APIs
- Follow async/await patterns for I/O operations
- Use dependency injection where appropriate

### Error Handling
- Use exceptions for exceptional cases
- Log errors appropriately
- Provide user-friendly error messages

## Original Credits
This application was originally created by Macronair. The C# migration preserves the original functionality while modernizing the codebase.

### Original Project
- **Author**: Macronair
- **Repository**: https://github.com/Macronair/TheMillionaireGame
- **Original Language**: Visual Basic .NET
- **Framework**: .NET Framework 4.8

## License
Same as original project. See main README.md for details.

## Contributing
When contributing to this migration:
1. Maintain compatibility with original database schema
2. Follow C# coding conventions
3. Add appropriate unit tests
4. Update this migration documentation
