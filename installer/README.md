# Installer Build Instructions

This directory contains the Inno Setup script for creating The Millionaire Game installer.

## Prerequisites

1. **Inno Setup 6.0 or later**
   - Download from: https://jrsoftware.org/isinfo.php
   - Install the full version (includes compiler)

2. **Built Application**
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

### Automatic Dependency Detection & Installation
- **.NET 8.0 Desktop Runtime**: Automatically downloads and installs if missing
- **SQL Server Express**: Detects if SQL Server is installed, offers to download/install if missing

### Installation Options
- **Desktop Icon**: Optional (checked by default)
- **Database Initialization**: Optional (unchecked by default)
  - Creates `dbMillionaire` database if it doesn't exist
  - Runs `init_database.sql` to create tables and import questions
  - Imports 80 main questions and 41 FFF questions

### What Gets Installed
- Main application executable (~46 MB)
- Watchdog crash monitor (~0.2 MB)
- Sound files (~176 MB)
- Image assets (~1 MB)
- Stream Deck DLLs (~0.3 MB)
- Database initialization script (root folder)
- SQL setup instructions (root folder)

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
