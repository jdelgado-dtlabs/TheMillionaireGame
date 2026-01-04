# Building from Source

This guide explains how to compile The Millionaire Game from source code for development, customization, or contributing to the project.

---

## Prerequisites

### Required Software

1. **Operating System**
   - Windows 10/11 (64-bit)
   - macOS/Linux supported for development but cannot run the application (Windows Forms)

2. **Development Environment** (Choose One)
   - **Visual Studio 2022** (Community, Professional, or Enterprise)
     - Workload: ".NET desktop development"
     - Download: [Visual Studio 2022](https://visualstudio.microsoft.com/)
   - **VS Code** with C# Dev Kit
     - Install: [VS Code](https://code.visualstudio.com/)
     - Extension: [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)

3. **.NET 8 SDK**
   - Download: [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
   - Verify installation: `dotnet --version` (should show 8.0.x)

4. **Git**
   - Download: [Git for Windows](https://git-scm.com/download/win)
   - Verify installation: `git --version`

### Optional Tools
- **SQL Server Management Studio (SSMS)** - For database development
- **Git GUI Client** - GitHub Desktop, GitKraken, etc.
- **PowerShell 7+** - Enhanced terminal experience

---

## Cloning the Repository

### Using Git Command Line

```powershell
# Clone the repository
git clone https://github.com/Macronair/TheMillionaireGame.git

# Navigate to the project
cd TheMillionaireGame

# Switch to the active development branch
git checkout master-csharp

# Verify branch
git branch
# Should show: * master-csharp
```

### Using GitHub Desktop
1. File → Clone Repository
2. URL: `https://github.com/Macronair/TheMillionaireGame`
3. Choose local path
4. Click "Clone"
5. Current Branch → Switch to `master-csharp`

---

## Project Structure

```
TheMillionaireGame/
├── src/                              # All source code
│   ├── TheMillionaireGame.sln        # Main solution file
│   ├── MillionaireGame/              # Main Windows Forms application
│   │   ├── MillionaireGame.csproj    # Project file
│   │   ├── Program.cs                # Entry point
│   │   ├── Forms/                    # UI forms (Control Panel, TV Screen)
│   │   ├── Services/                 # Business logic services
│   │   └── bin/Debug/                # Build output
│   ├── MillionaireGame.Core/         # Core game logic library
│   │   ├── Game/                     # Game state management
│   │   ├── Database/                 # Data access layer
│   │   └── Models/                   # Data models
│   ├── MillionaireGame.Web/          # Web server for audience features
│   │   ├── Controllers/              # API endpoints
│   │   ├── Hubs/                     # SignalR hubs
│   │   └── wwwroot/                  # Static web files
│   ├── MillionaireGame.Watchdog/     # Crash monitoring service
│   └── docs/                         # Documentation
├── wiki/                             # Wiki documentation (GitHub Wiki)
├── publish/                          # Release builds
└── installer/                        # Installer scripts
```

---

## Building the Solution

### Method 1: Visual Studio 2022

1. **Open Solution**
   - File → Open → Project/Solution
   - Navigate to `src\TheMillionaireGame.sln`
   - Click "Open"

2. **Restore NuGet Packages**
   - Automatic on first build
   - Or: Right-click solution → Restore NuGet Packages

3. **Build**
   - Build → Build Solution (`Ctrl+Shift+B`)
   - Or: Right-click solution → Build Solution

4. **Check Output**
   ```
   src/MillionaireGame/bin/Debug/net8.0-windows/
   ├── MillionaireGame.exe           # Main executable
   ├── MillionaireGame.dll           # Main assembly
   ├── MillionaireGame.Core.dll      # Core library
   ├── MillionaireGame.Web.dll       # Web server
   └── [dependencies]                # NuGet packages
   ```

5. **Run**
   - Debug → Start Debugging (`F5`)
   - Or: Debug → Start Without Debugging (`Ctrl+F5`)

### Method 2: VS Code

1. **Open Workspace**
   - File → Open Folder
   - Select `TheMillionaireGame` folder
   - VS Code detects the .NET project

2. **Install Recommended Extensions**
   - When prompted, install:
     - C# Dev Kit
     - .NET Extension Pack

3. **Build Using Task**
   - Terminal → Run Task (`Ctrl+Shift+B`)
   - Select: `build`
   - Or: Press `Ctrl+Shift+B` (default build task)

4. **Build Using Command**
   ```powershell
   cd src
   dotnet build TheMillionaireGame.sln
   ```

5. **Run Using Task**
   - Terminal → Run Task
   - Select: `run`
   - This automatically:
     - Stops any running instances
     - Builds the solution
     - Launches the application

6. **Run Using Command**
   ```powershell
   cd src/MillionaireGame/bin/Debug/net8.0-windows
   .\MillionaireGame.exe
   ```

### Method 3: Command Line (Any Editor)

```powershell
# Navigate to source directory
cd src

# Restore dependencies (first time only)
dotnet restore TheMillionaireGame.sln

# Build in Debug mode
dotnet build TheMillionaireGame.sln

# Or build in Release mode
dotnet build TheMillionaireGame.sln -c Release

# Run the application
cd MillionaireGame/bin/Debug/net8.0-windows
.\MillionaireGame.exe
```

---

## Building Individual Projects

### Main Application Only
```powershell
cd src
dotnet build MillionaireGame/MillionaireGame.csproj
```

### Core Library Only
```powershell
cd src
dotnet build MillionaireGame.Core/MillionaireGame.Core.csproj
```

### Web Server Only
```powershell
cd src
dotnet build MillionaireGame.Web/MillionaireGame.Web.csproj
```

### Watchdog Only
```powershell
cd src
dotnet build MillionaireGame.Watchdog/MillionaireGame.Watchdog.csproj
```

---

## Publishing Release Builds

### Standard Release (Framework-Dependent)

Requires .NET 8 Desktop Runtime on target machine (~34 MB executable).

```powershell
cd src

# Publish single-file executable
dotnet publish MillionaireGame/MillionaireGame.csproj `
  -c Release `
  -r win-x64 `
  --no-self-contained `
  -p:PublishSingleFile=true `
  -o ../publish
```

**Output**: `publish\MillionaireGame.exe` (~34 MB)

### Self-Contained Release (Not Recommended)

Includes .NET runtime (~204 MB executable). Use only if target machines cannot install .NET.

```powershell
cd src

dotnet publish MillionaireGame/MillionaireGame.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -o ../publish-standalone
```

**Output**: `publish-standalone\MillionaireGame.exe` (~204 MB)

> ⚠️ **Not Recommended**: Self-contained builds are significantly larger and make updates harder. Prefer framework-dependent deployment with .NET Runtime installer.

### Publish Structure
```
publish/
├── MillionaireGame.exe               # Main executable
├── MillionaireGame.Web.dll           # Web server
├── MillionaireGame.Watchdog.exe      # Crash monitor
├── lib/
│   ├── sounds/                       # Audio files
│   │   └── Default/                  # Default sound set
│   └── image/                        # Images
├── init_database.sql                 # Database setup script
└── [native DLLs]                     # SQL Client, etc.
```

---

## Development Workflow

### Before Building
**CRITICAL**: Always stop running instances to avoid file locks!

```powershell
# Stop all MillionaireGame processes
Stop-Process -Name "MillionaireGame*" -Force -ErrorAction SilentlyContinue

# Then build
cd src
dotnet build TheMillionaireGame.sln
```

### Using VS Code Tasks
The project includes pre-configured tasks:

**Build Task** (`Ctrl+Shift+B`):
```json
{
  "label": "build",
  "command": "dotnet",
  "args": ["build", "src/TheMillionaireGame.sln"]
}
```

**Run Task**:
```json
{
  "label": "run",
  "type": "shell",
  "command": "Stop-Process; dotnet build; Start-Process MillionaireGame.exe"
}
```

**Watch Task** (Auto-rebuild on file changes):
```json
{
  "label": "watch",
  "command": "dotnet",
  "args": ["watch", "run", "--project", "src/TheMillionaireGame.sln"]
}
```

### Git Workflow

1. **Create Feature Branch**
   ```powershell
   git checkout master-csharp
   git pull origin master-csharp
   git checkout -b feature/my-feature
   ```

2. **Make Changes**
   - Edit code
   - Test thoroughly
   - Follow project coding standards

3. **Commit Changes**
   ```powershell
   git add .
   git commit -m "feat: Add my feature description"
   ```

4. **Push to GitHub**
   ```powershell
   git push origin feature/my-feature
   ```

5. **Create Pull Request**
   - Visit GitHub repository
   - Click "Compare & pull request"
   - Target branch: `master-csharp`
   - Describe changes
   - Submit for review

---

## Troubleshooting Build Issues

### Issue: "SDK Not Found"
**Error**: `The specified SDK 'Microsoft.NET.Sdk' was not found`

**Solution**:
```powershell
# Verify .NET 8 SDK installed
dotnet --version

# Should show 8.0.x
# If not, download and install .NET 8 SDK
```

### Issue: "NU1101: Unable to find package"
**Error**: NuGet restore failed

**Solution**:
```powershell
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore packages
cd src
dotnet restore TheMillionaireGame.sln
```

### Issue: "File is being used by another process"
**Error**: Build fails with file lock errors

**Solution**:
```powershell
# Stop all MillionaireGame processes
Stop-Process -Name "MillionaireGame" -Force -ErrorAction SilentlyContinue
Stop-Process -Name "MillionaireGame.Watchdog" -Force -ErrorAction SilentlyContinue

# Clean build artifacts
cd src
dotnet clean TheMillionaireGame.sln

# Rebuild
dotnet build TheMillionaireGame.sln
```

### Issue: "The command 'dotnet' is not found"
**Error**: PowerShell cannot find `dotnet` command

**Solution**:
```powershell
# Add .NET to PATH (restart terminal after)
# Or download and install .NET 8 SDK

# Verify installation
where.exe dotnet
# Should show: C:\Program Files\dotnet\dotnet.exe
```

### Issue: "Build succeeded but application won't run"
**Error**: EXE starts then immediately closes

**Solution**:
```powershell
# Check for missing dependencies
cd src/MillionaireGame/bin/Debug/net8.0-windows
.\MillionaireGame.exe

# Check console output for errors
# Or check Logs/ folder for error logs
```

### Issue: Database Initialization Errors
**Error**: "Cannot connect to database"

**Solution**:
- Ensure SQL Server LocalDB installed (.NET SDK includes it)
- Check `App.config` connection string
- Run application as administrator (first time only)

---

## Running Tests

*Note: Unit tests are under development. This section will be updated when test projects are added.*

```powershell
# Future: Run all tests
dotnet test src/TheMillionaireGame.sln

# Future: Run specific test project
dotnet test src/MillionaireGame.Tests/MillionaireGame.Tests.csproj
```

---

## Code Style and Standards

### Follow Project Guidelines
- No `MessageBox` or blocking dialogs in game operations
- Use `GameConsole` logging instead of `Console.WriteLine`
- Follow existing patterns in codebase
- Use `.editorconfig` settings (auto-applied in VS/VS Code)

### Logging Levels
```csharp
GameConsole.Debug("Detailed diagnostic info");
GameConsole.Info("General information");
GameConsole.Warn("Warning messages");
GameConsole.Error("Error messages");
```

### Avoid
```csharp
// ❌ DON'T - Blocks game flow
MessageBox.Show("Error occurred");

// ❌ DON'T - Not captured in logs
Console.WriteLine("Debug info");

// ✅ DO - Non-blocking, logged
GameConsole.Error("Error occurred");
```

---

## Next Steps

### After Successful Build
1. **Run Application**: Test all features
2. **Review Documentation**: Check [User Guide](User-Guide)
3. **Explore Codebase**: Start with `Program.cs` and `ControlPanelForm.cs`
4. **Make Changes**: Create feature branch and experiment

### Contributing
See [Contributing Guide](Contributing) for:
- Code standards
- Pull request process
- Issue reporting
- Community guidelines

### Resources
- **Architecture Overview**: [Architecture](Architecture)
- **Database Schema**: [Database Documentation](Database-Schema)
- **API Reference**: *Coming soon*

---

## Need Help?

- **Build Issues**: Check [Troubleshooting](Troubleshooting)
- **Git Help**: [Git Documentation](https://git-scm.com/doc)
- **.NET Issues**: [.NET Documentation](https://docs.microsoft.com/dotnet/)
- **Report Bugs**: [GitHub Issues](https://github.com/Macronair/TheMillionaireGame/issues)

---

**Ready to contribute?** Check out [open issues](https://github.com/Macronair/TheMillionaireGame/issues) or propose new features!
