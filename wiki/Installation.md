# Installation Guide

This guide walks you through installing The Millionaire Game on your Windows computer.

---

## Prerequisites

Before installing, ensure your system meets the [System Requirements](System-Requirements).

**You will need:**
- Windows 10/11 x64
- .NET 8 Desktop Runtime (Will be downloaded and installed if not already)
- SQL Server Express (Will be downloaded and installed if not already)
- Approximately 400 MB free disk space (Includes application and SQL database)

---

## Installation Methods

> **Note**: The installer is built with Inno Setup. For technical details about the installation process, see the setup script at `installer/MillionaireGameSetup.iss` in the repository.

### Standard Installation

The Windows installer handles all dependencies and configuration automatically.

#### Step 1: Download the Installer
1. Visit the [Releases Page](https://github.com/jdelgado-dtlabs/TheMillionaireGame/releases)
2. Find the latest release (e.g., `v1.0.0`)
3. Download `MillionaireGameSetup.exe`

#### Step 2: Run the Installer
1. Double-click `MillionaireGameSetup.exe`
2. Windows SmartScreen may appear:
   - Click "More info"
   - Click "Run anyway"
3. Follow the installation wizard:
   - Accept the license agreement
   - Choose installation scope (per user or per system)
   - Choose installation folder (default: `C:\Program Files\The Millionaire Game\`)
   - Select Start Menu folder
   - Choose optional components:
     - Desktop shortcut
     - **Initialize SQL Server database** (creates dbMillionaire and imports sample questions) - *Unchecked by default*
   - The installer will automatically detect and install missing dependencies:
     - .NET 8 Desktop Runtime (if needed)
     - SQL Server Express (if needed)
4. Click **Install**
5. Wait for installation to complete (may take several minutes if dependencies are being installed)
6. Click **Finish**

#### Step 3: First Launch
1. Launch from Start Menu or desktop shortcut
2. The application will:
   - Create default configuration files
   - Connect to SQL Server Express database
   - Verify installation integrity
3. Control Panel opens - you're ready to configure and play!

---

## Post-Installation Setup

### Database Setup

The application requires **SQL Server Express** (installed automatically by the installer).

#### Database Initialization

During installation, you can optionally check **"Initialize SQL Server database"** to automatically:
- Create the `dbMillionaire` database
- Import **80 trivia questions** for the main game (4-level difficulty system)
  - Level 1 (Easy): Questions 1-5 - 20 questions
  - Level 2 (Medium): Questions 6-10 - 20 questions
  - Level 3 (Hard): Questions 11-14 - 20 questions
  - Level 4 (Million): Question 15 - 20 questions
- Import **44 ordering questions** for Fastest Finger First

**If you skipped database initialization during install:**
1. Locate `init_database.sql` in the installation folder
2. Run the script manually against your SQL Server Express instance
3. This will create the database and populate the question tables

**Database Schema (v1.0):**
The database uses a simplified 4-level difficulty system that aligns with the natural progression of the game. Each question is assigned to one of four levels based on difficulty and prize tier.

**Included Questions:**
These generic trivia questions are free to use and serve as templates for creating your own question sets. You can manage questions using the built-in Question Editor (**Tools** → **Question Editor**).

**Note:** The application automatically creates settings and WAPS (Web Audience Participation System) tables on first launch. Only the question tables require manual initialization if not done during installation.

---

### Sound Pack Management

The application includes a default sound pack. Custom sound packs are managed through the built-in interface.

#### Adding Custom Sound Packs
1. Open **Control Panel** → **Game** → **Settings** → **Sounds** tab → **Soundpack** tab
2. Click **Export Example** to get a blank sound pack ZIP with XML structure
3. Add your audio files to the ZIP and update XML mappings
4. Click **Import** and select your customized ZIP
5. The application validates and installs the sound pack automatically

#### Removing Sound Packs
1. Navigate to the same **Soundpack** tab
2. Select the sound pack to remove
3. Click **Remove**

> **Warning**: The default sound pack cannot be removed through the interface. If you manually delete the default sound pack files, you will lose all audio functionality.

> **Note**: For complete sound pack documentation, see the [Sound Pack System Guide](Sound-Pack-System) *(Coming Soon)*

---

### Display Configuration

The application provides multiple screen outputs for different purposes:

**Available Screens:**
- **TV Screen** - Main display for audience throughout the entire game. Can be captured with streaming software and broadcast to platforms that support individual window streaming.
- **Host Screen** - Private screen for the game host showing answers and contestant information. Used only during the main game.
- **Guest Screen** - Display for the contestant showing their view. Used only during the main game.
- **Preview Screen** - Supervision tool for the Control Panel operator to monitor all three screens simultaneously. Has limitations: TV screen animations cannot be scaled properly, so some visual updates may not display accurately in preview. Not intended for streaming.

#### Single Monitor Setup
1. Launch application
2. Control Panel opens by default
3. Open individual screens via **Screens** menu as needed
4. Use **Preview Screen** to supervise what's being displayed without opening all screens
5. Manual window positioning is supported but not persistent

#### Multi-Monitor Setup (Recommended)
1. Connect monitors (supports 2, 3, or 4 monitor setups)
2. Configure Windows display settings (Extend displays)
3. Launch application
4. Go to **Game** → **Settings** → **Screens** tab
5. Assign each screen to a specific display:
   - Each display can only be assigned to one screen
   - Once assigned, the screen will automatically maximize on that display at startup
   - Assignments are persistent across application restarts
6. Configured screens will automatically open and position on their assigned displays

**Note**: Manual drag-and-drop positioning works but is not saved between sessions. Use the Settings > Screens tab for persistent display assignments.

---

## Verifying Installation

1. Launch The Millionaire Game from Start Menu or desktop shortcut
2. The application will start and open the Control Panel
3. If the database is not available, the application will display an error message and will not start
4. For a complete walkthrough of running your first game, follow the [Quick Start Guide](Quick-Start-Guide)

**Note**: The database is required for the application to operate. If there are any database connectivity issues, the application will not launch and will display an error explaining the problem.

---

## Updating the Application

1. Download the new installer from the [Releases Page](https://github.com/jdelgado-dtlabs/TheMillionaireGame/releases)
2. Run the installer
3. The installer will:
   - Detect the existing installation
   - Automatically uninstall the previous version
   - Install the new version
4. Your user data is preserved:
   - Database remains intact
   - Settings are maintained
   - Custom sound packs are retained

**Note**: This process works for both updates and reinstallation of the same version. Great for restoring the application to pristine condition if issues occur.

---

## Uninstalling

### Standard Installation
1. Windows Settings → Apps → Installed Apps
2. Find "The Millionaire Game"
3. Click "Uninstall"
4. Follow uninstaller prompts

**Optional Cleanup:**
Remove user data (not deleted by uninstaller):
```
%APPDATA%\MillionaireGame\
%LOCALAPPDATA%\MillionaireGame\
```

**Program Files Location:**
```
C:\Program Files\The Millionaire Game\
```

---

## Firewall Configuration

If using web audience participation features, configure Windows Firewall:

### Automatic (Recommended)
1. Launch The Millionaire Game
2. Enable Web Server in settings
3. Windows Firewall prompt appears
4. Click "Allow access"

### Manual Configuration
1. Windows Settings → Update & Security → Windows Security
2. Firewall & network protection → Advanced settings
3. Inbound Rules → New Rule
4. Program: Browse to `MillionaireGame.exe`
5. Allow connection
6. Apply to all profiles
7. Name: "The Millionaire Game"

---

## Troubleshooting Installation

### Issue: "Database initialization failed"
**Solution:** 
- Verify SQL Server Express is installed and running
- Check SQL Server service status in Windows Services
- Ensure sufficient disk space (~100 MB for database)
- Run application as administrator
- Check antivirus/firewall isn't blocking SQL Server
- Try running `init_database.sql` manually from installation folder

**Other Issues:**
- Dependency installation (-.NET 8, SQL Server Express) is handled automatically by the installer
- Sound pack issues are handled by the application's fallback to default sounds
- For additional troubleshooting, see the [Troubleshooting Guide](Troubleshooting)

---

## Next Steps

Installation complete! Continue to:
- **[Quick Start Guide](Quick-Start-Guide)** - Start playing immediately
- **[User Guide](User-Guide)** - Learn all features
- **[Troubleshooting](Troubleshooting)** - Solve common issues

---

**Need help?** [Report an issue](https://github.com/jdelgado-dtlabs/TheMillionaireGame/issues) or check [Troubleshooting](Troubleshooting).
