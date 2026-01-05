# GitHub Wiki Documentation Plan

**Status**: Planning Phase  
**Priority**: High (Pre-v1.0 Release)  
**Created**: January 3, 2026  
**Target**: Complete after Stream Deck Integration

---

## Overview

This document outlines the structure and content for The Millionaire Game's GitHub Wiki. The wiki will serve as the primary public documentation for end users, developers, and contributors.

---

## Wiki Structure

### 1. **Home Page**
- **Purpose**: Landing page with quick navigation
- **Content**:
  - Project overview and features
  - Quick start guide
  - Navigation links to all wiki sections
  - Latest release information
  - Screenshots/demo video

### 2. **Getting Started**

#### 2.1 System Requirements
- Operating System: Windows 10/11 (64-bit)
- .NET 8 Desktop Runtime (download link)
- Minimum 4GB RAM
- 1920x1080 display recommended for TV screen
- Optional: Secondary monitor for control panel
- Optional: SQL Server for custom question databases

#### 2.2 Installation
- Download latest release from GitHub Releases
- Extract to desired location
- Install .NET 8 Desktop Runtime if not present
- First launch configuration
- Database setup options (embedded vs SQL Server)

#### 2.3 Quick Start Guide
- Launch the application
- Configure displays (control panel vs TV screen)
- Load question set
- Start your first game
- Basic gameplay controls

### 3. **Building from Source**

#### 3.1 Development Requirements
- Visual Studio 2022 or VS Code with C# Dev Kit
- .NET 8 SDK
- Git for version control
- Optional: SQL Server for database development

#### 3.2 Cloning the Repository
```bash
git clone https://github.com/Macronair/TheMillionaireGame.git
cd TheMillionaireGame
git checkout master-csharp
```

#### 3.3 Building the Solution
**Using VS Code Tasks**:
- Open workspace in VS Code
- Run task: `build` (Ctrl+Shift+B)
- Build output: `src/MillionaireGame/bin/Debug/net8.0-windows/`

**Using Command Line**:
```powershell
cd src
dotnet build TheMillionaireGame.sln
```

**Using Visual Studio**:
- Open `src/TheMillionaireGame.sln`
- Build → Build Solution (Ctrl+Shift+B)

#### 3.4 Running in Development
**Option 1: VS Code Task**
- Run task: `run`
- Automatically stops existing processes, builds, and launches

**Option 2: Manual Launch**
```powershell
cd src/MillionaireGame/bin/Debug/net8.0-windows/
.\MillionaireGame.exe
```

**Note**: Application automatically launches with crash monitoring watchdog

#### 3.5 Publishing Release Build
```powershell
cd src
dotnet publish MillionaireGame/MillionaireGame.csproj `
  -c Release `
  -r win-x64 `
  --no-self-contained `
  -p:PublishSingleFile=true `
  -o ../publish
```

**Output**: Single executable (~34 MB) requiring .NET 8 Desktop Runtime

### 4. **User Guide**

#### 4.1 Interface Overview
- Control Panel layout and sections
- TV Screen display modes
- Host Screen features
- Guest/Audience screen

#### 4.2 Game Setup
- Creating/selecting game profiles
- Money tree configuration
- Dual currency setup
- Lifeline configuration
- Sound settings
- Display/monitor assignment

#### 4.3 Question Management
- Creating question sets
- Importing questions (CSV/SQL)
- Organizing by difficulty
- Question editor interface
- Backup and restore

#### 4.4 Running a Game
- Starting a new game
- Fastest Finger First mode
- Main game controls
- Using lifelines
- Safety net mechanics
- Walking away
- Winning/losing outcomes

#### 4.5 Audience Participation (Web Integration)
- Enabling web server
- Audience voting for Ask the Audience
- Connection instructions for participants
- Network configuration

#### 4.6 Stream Deck Integration
- *(Will be populated after Stream Deck implementation)*
- Setup and configuration
- Button mapping
- Custom actions
- Profiles

### 5. **Architecture & Technical Details**

#### 5.1 Project Structure
```
src/
├── MillionaireGame/          # Main Windows Forms application
├── MillionaireGame.Core/     # Core game logic library
├── MillionaireGame.Web/      # Web server for audience participation
├── MillionaireGame.Watchdog/ # Crash monitoring service
└── docs/                     # Documentation
```

#### 5.2 Key Components
- **ControlPanelForm**: Main game controller
- **TVScreenForm**: Primary display renderer
- **GameService**: Core game state management
- **MoneyTreeService**: Prize/currency calculations
- **TelemetryService**: Game statistics tracking
- **LifelineManager**: Lifeline orchestration

#### 5.3 Database Schema
- Questions table structure
- Game profiles
- Money tree configurations
- Telemetry data

#### 5.4 Graphics Rendering
- Custom drawing engine
- Scalable vector-based graphics
- Animation system (confetti, transitions)
- Theme system (5 built-in themes)

#### 5.5 Sound System
- Audio engine architecture
- Sound profile management
- Custom sound sets
- Volume control hierarchy

### 6. **Configuration Files**

#### 6.1 App.config
- Connection strings
- Application settings
- Logging configuration

#### 6.2 Settings Storage
- User preferences location
- Profile storage
- Recent games tracking

#### 6.3 Custom Sound/Image Assets
- Directory structure (`lib/sounds/`, `lib/image/`)
- File format requirements
- Adding custom assets

### 7. **Advanced Features**

#### 7.1 Hotkey Configuration
- Customizing keyboard shortcuts
- Global vs application hotkeys
- Conflict resolution

#### 7.2 Multiple Display Setup
- Configuring 2-3 monitor setups
- Screen assignment strategies
- Resolution handling

#### 7.3 Telemetry & Analytics
- Game statistics tracking
- Excel export (XLSX format)
- Dual currency breakdown
- Performance metrics

#### 7.4 Crash Recovery
- Automatic watchdog monitoring
- Crash report generation
- Log analysis
- Recovery procedures

### 8. **Troubleshooting**

#### 8.1 Common Issues
- Application won't start
- Display/scaling problems
- Sound not playing
- Database connection errors
- Performance issues

#### 8.2 Log Files
- Location: `Logs/` folder
- Types: Game logs, telemetry, crash reports
- Reading diagnostic information

#### 8.3 Known Limitations
- Windows-only (no macOS/Linux support)
- Single-instance application
- Network requirements for audience features

#### 8.4 FAQ
- Can I use custom questions?
- How do I add more themes?
- Can I use different currencies?
- How to backup my data?
- Multi-language support?

### 9. **Contributing**

#### 9.1 Development Guidelines
- Coding standards (follow project .editorconfig)
- No `MessageBox` in game operations (use GameConsole logging)
- Git workflow (master-csharp branch)
- Pull request process

#### 9.2 Reporting Issues
- Using GitHub Issues
- Bug report template
- Feature request template
- Security issues

#### 9.3 Testing
- Manual testing checklist (PRE_V1.0_TESTING_CHECKLIST.md)
- Building test question sets
- Verifying crash handler

#### 9.4 Documentation
- Updating wiki pages
- Code comments expectations
- Session documentation examples

### 10. **Release Notes**

#### 10.1 Version History
- Link to CHANGELOG.md
- Major feature additions by version
- Breaking changes

#### 10.2 Migration Guides
- VB.NET to C# (archived)
- Upgrading from older versions

### 11. **License & Credits**

#### 11.1 License Information
- Project license
- Third-party dependencies
- Asset attributions

#### 11.2 Credits
- Original VB.NET version
- C# rewrite contributors
- Sound/graphics resources

---

## Content Creation Phases

### Phase 1: Essential Documentation (Pre-v1.0)
- [ ] Home page with navigation
- [ ] System requirements
- [ ] Installation guide
- [ ] Quick start guide
- [ ] Building from source
- [ ] Basic user guide (game setup, running games)
- [ ] Troubleshooting common issues

### Phase 2: Detailed Documentation (Post-v1.0)
- [ ] Complete user guide (all features)
- [ ] Stream Deck integration guide
- [ ] Architecture documentation
- [ ] Configuration reference
- [ ] Advanced features guide
- [ ] FAQ expansion

### Phase 3: Community Documentation (Ongoing)
- [ ] Contributing guidelines
- [ ] API documentation (if applicable)
- [ ] Plugin/extension development (future)
- [ ] Tutorial videos/screenshots
- [ ] Community examples

---

## Writing Guidelines

### Style
- Clear, concise language
- Step-by-step instructions with screenshots
- Code blocks with syntax highlighting
- Consistent formatting and terminology
- Use proper Markdown for navigation

### Terminology
- **Host**: Show facilitator/operator
- **Player/Contestant**: Person playing the game
- **Audience**: Live audience or web participants
- **Control Panel**: Operator interface
- **TV Screen**: Main display
- **Money Tree**: Prize ladder

### Screenshots
- Capture at 1920x1080 when possible
- Annotate key areas with arrows/highlights
- Save as PNG for clarity
- Store in wiki's images folder

### Code Examples
- Test all commands before documenting
- Include expected output
- Note platform-specific variations
- Provide copy-paste ready snippets

---

## Maintenance

### Regular Updates
- After each release: Update version numbers, release notes
- When features change: Update relevant user guide sections
- When bugs fixed: Update troubleshooting section
- Community feedback: Add to FAQ

### Review Schedule
- Pre-release: Full review of changed sections
- Quarterly: General accuracy review
- Annually: Complete documentation audit

---

## Success Metrics

- Users can build from source without asking questions
- Installation success rate (track via issues)
- Reduced "how to" questions in Issues
- Community contributions to wiki
- Positive feedback on documentation clarity

---

## Next Steps

1. ✅ Complete Stream Deck Integration implementation
2. Create initial wiki structure on GitHub
3. Write Phase 1 content (essential documentation)
4. Add screenshots and diagrams
5. Internal review and testing
6. Publish with v1.0 release
7. Iterate based on user feedback

---

## Notes

- Keep documentation in sync with codebase
- Consider video tutorials for complex topics (post-v1.0)
- Explore internationalization if community interest grows
- Wiki is public - avoid internal development details
- Link to source code when referencing specific components
