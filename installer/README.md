# Installer Build Instructions

This directory contains the Inno Setup script for creating The Millionaire Game installer.

## Prerequisites

1. **Inno Setup 6.0 or later**
   - Download from: https://jrsoftware.org/isinfo.php
   - Install the full version (includes compiler)

2. **SQL Server LocalDB Installer** (Auto-downloaded during build)
   - **Download URL**: https://download.microsoft.com/download/3/8/d/38de7036-2433-4207-8eae-06e247e17b25/SqlLocalDB.msi
   - **File Size**: ~50 MB
   - **Location**: Automatically downloaded to `installer/lib/sql/SqlLocalDB.msi` during compilation
   - **Purpose**: Bundled with installer for LocalDB option (no download at install time)
   
   **Note**: The installer script automatically downloads SqlLocalDB.msi if missing during compilation.
   
   **Manual download** (optional):
   ```powershell
   # Only needed if automatic download fails
   New-Item -ItemType Directory -Path "installer\lib\sql" -Force
   Invoke-WebRequest -Uri "https://download.microsoft.com/download/3/8/d/38de7036-2433-4207-8eae-06e247e17b25/SqlLocalDB.msi" -OutFile "installer\lib\sql\SqlLocalDB.msi"
   ```

3. **Built Application**
   - The `publish/` folder must be populated with the built application
   - Run from project root: `cd src; dotnet publish MillionaireGame/MillionaireGame.csproj -c Release -r win-x64 --no-self-contained -p:PublishSingleFile=true -o ../publish`
   - Also publish Watchdog: `dotnet publish MillionaireGame.Watchdog/MillionaireGame.Watchdog.csproj -c Release -r win-x64 --no-self-contained -p:PublishSingleFile=true -o ../publish`

## Building the Installer

### Using Inno Setup GUI
1. Open `MillionaireGameSetup.iss` in Inno Setup Compiler
2. Click **Build** → **Compile**
3. Installer will be created in `installer/output/MillionaireGameSetup-v1.0.0.exe`

### Using Command Line
```powershell
# Assuming Inno Setup is installed in default location
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" MillionaireGameSetup.iss
```

## Installer Features

### Database Options (New in v1.0.6)
User can choose database during installation:
- **LocalDB (Recommended)**: Bundled with installer (~50 MB), zero configuration
- **SQL Server 2022 Express**: Downloaded at install time (~1.5 GB), full-featured
- **Remote Server**: Connect to existing SQL Server (no installation)

### Automatic Dependency Detection & Installation
- **.NET 8.0 Desktop Runtime**: Automatically downloads and installs if missing
- **SQL Server LocalDB**: Bundled with installer (installed silently if user selects LocalDB)
- **SQL Server Express**: Downloads and installs if user selects it (or detects existing installation)

### Installation Options
- **Desktop Icon**: Optional (checked by default)
- **Database Initialization**: Optional (unchecked by default)
  - Creates `dbMillionaire` database if it doesn't exist
  - Runs `init_database.sql` to create tables and import questions
  - Imports 80 main questions and 41 FFF questions

### What Gets Installed
- Main application executable (~34 MB single-file)
- Watchdog crash monitor (~0.2 MB single-file)
- Web server DLL (~1 MB)
- Core library DLL (~0.5 MB)
- Sound files (~176 MB)
- Image assets (~1 MB)
- Stream Deck DLLs (~0.3 MB)
- SQL Server LocalDB installer (~50 MB in lib/sql/)
- Database initialization script (lib/sql/)
- Native SQL Client DLLs

**Total Installer Size**: ~265 MB (includes bundled LocalDB)

### Start Menu Items
- The Millionaire Game (launch application)
- Database Initialization Script (opens SQL file)
- SQL Setup Instructions (opens README)
- Uninstall

## Output

**Installer filename**: `MillionaireGameSetup-v1.0.0.exe`
**Expected size**: ~230 MB (includes all assets)
**Location**: `installer/output/`

## Customization

Edit `MillionaireGameSetup.iss` to customize:
- Version number (line 7): `#define MyAppVersion "1.0.0"`
- Publisher info (line 8): `#define MyAppPublisher`
- URLs and branding
- Installation directory defaults
- Compression settings

## Database Initialization

The installer can optionally initialize the SQL Server database:
1. Checkbox appears on the finish page (unchecked by default)
2. If checked, PowerShell script runs to:
   - Connect to `localhost\SQLEXPRESS`
   - Create `dbMillionaire` database if it doesn't exist
   - Execute `init_database.sql`
   - Display success/error message

Users can also initialize manually later using the installed SQL script.

## Troubleshooting

**Error: Cannot find source files**
- Ensure `publish/` folder exists in project root
- Rebuild the application first

**Error: Cannot compile**
- Install Inno Setup 6.0 or later
- Check file paths in the script are correct

**SQL Server not detected**
- Installer checks both 32-bit and 64-bit registry keys
- Looks for SQLEXPRESS or any MSSQL instance
- Users can skip SQL Server installation if they have another instance

## Version History

- **v1.0.0** (2026-01-04): Initial installer with .NET Runtime, SQL Server Express detection, and database initialization
