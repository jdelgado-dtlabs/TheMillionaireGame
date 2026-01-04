# Installation Guide

This guide walks you through installing The Millionaire Game on your Windows computer.

---

## Prerequisites

Before installing, ensure your system meets the [System Requirements](System-Requirements).

**You will need:**
- Windows 10/11 (64-bit)
- .NET 8 Desktop Runtime
- Approximately 300 MB free disk space

---

## Installation Methods

### Method 1: Standard Installation (Recommended)

This method uses the Windows installer for easy setup and updates.

#### Step 1: Download the Installer
1. Visit the [Releases Page](https://github.com/Macronair/TheMillionaireGame/releases)
2. Find the latest release (e.g., `v1.0.0`)
3. Download `MillionaireGameSetup.exe`

#### Step 2: Install .NET 8 Runtime (If Not Installed)
1. If prompted, the installer will detect missing .NET 8 Runtime
2. Download from: [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)
3. Run the .NET installer first
4. Restart and return to the game installer

#### Step 3: Run the Installer
1. Double-click `MillionaireGameSetup.exe`
2. Windows SmartScreen may appear:
   - Click "More info"
   - Click "Run anyway"
3. Follow the installation wizard:
   - Accept the license agreement
   - Choose installation folder (default: `C:\Program Files\The Millionaire Game\`)
   - Select Start Menu folder
   - Choose desktop shortcut option
4. Click **Install**
5. Wait for installation to complete (~1-2 minutes)
6. Click **Finish**

#### Step 4: First Launch
1. Launch from Start Menu or desktop shortcut
2. The application will:
   - Initialize database (first run only)
   - Create default configuration files
   - Load sample question set
3. Welcome screen appears - you're ready to play!

---

### Method 2: Portable Installation

For users who prefer portable/USB installations without system modifications.

#### Step 1: Download Portable Package
1. Visit the [Releases Page](https://github.com/Macronair/TheMillionaireGame/releases)
2. Download `MillionaireGame-Portable-v1.0.0.zip`

#### Step 2: Extract Files
1. Right-click the ZIP file → Extract All
2. Choose destination folder (e.g., `C:\Games\MillionaireGame` or USB drive)
3. Extract all contents

#### Step 3: Install .NET 8 Runtime
The portable version still requires .NET 8 Desktop Runtime installed on the system.
- Download: [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)
- Install if not present

#### Step 4: Run the Application
1. Navigate to extracted folder
2. Double-click `MillionaireGame.exe`
3. First-run initialization will occur

**Portable Mode Features:**
- No system registry changes
- All data stored in application folder
- Can run from USB drive
- Easy backup (copy entire folder)

---

## Post-Installation Setup

### Database Configuration

By default, the application uses **SQL Server LocalDB** (embedded database).

#### Option A: Use LocalDB (Default)
No configuration needed! LocalDB is automatically configured on first launch.

**Database Location:**
```
%LOCALAPPDATA%\The Millionaire Game\Database\
```

#### Option B: Use SQL Server
For advanced users with existing SQL Server installations:

1. Open `App.config` in installation folder
2. Modify connection string:
```xml
<connectionStrings>
  <add name="MillionaireDB" 
       connectionString="Server=YOUR_SERVER;Database=MillionaireGame;Integrated Security=True;" 
       providerName="System.Data.SqlClient" />
</connectionStrings>
```
3. Run `init_database.sql` script on your SQL Server
4. Restart application

---

### Sound Files Setup

The application includes default sounds, but you can add custom sound sets.

#### Default Sounds Location
```
[Installation Folder]\lib\sounds\Default\
```

#### Adding Custom Sounds
1. Create new folder: `lib\sounds\MyCustomSet\`
2. Copy sound files (see [Sound File Requirements](#sound-file-requirements))
3. Restart application
4. Select sound set in Settings → Audio

---

### Display Configuration

#### Single Monitor Setup
1. Launch application
2. Control Panel and TV Screen open on same display
3. Arrange windows as needed (Control Panel bottom, TV Screen top)

#### Dual Monitor Setup (Recommended)
1. Connect second monitor to computer
2. Configure Windows display settings (Extend displays)
3. Launch application
4. Drag TV Screen to primary display (e.g., TV/projector)
5. Keep Control Panel on secondary display (operator screen)

---

## Verifying Installation

### Quick Test
1. Launch The Millionaire Game
2. In Control Panel, click "New Game"
3. Click "Start FFF" (Fastest Finger First)
4. Verify:
   - ✅ TV Screen displays correctly
   - ✅ Sounds play when buttons clicked
   - ✅ No error messages in console log

### Full Test
Follow the [Quick Start Guide](Quick-Start-Guide) to run a complete game.

---

## Updating the Application

### Standard Installation Update
1. Download newer installer from Releases page
2. Run installer - it will detect existing installation
3. Choose "Update" or "Reinstall"
4. Your settings and databases are preserved

### Portable Installation Update
1. Download new portable ZIP
2. Extract to temporary folder
3. **Backup your data:**
   - Copy `Data\` folder (databases)
   - Copy `App.config` (if customized)
   - Copy `lib\sounds\` (custom sounds)
4. Replace program files with new version
5. Restore backed-up data folders

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
%LOCALAPPDATA%\The Millionaire Game\
%APPDATA%\The Millionaire Game\
```

### Portable Installation
Simply delete the folder containing the application.

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

## Sound File Requirements

If adding custom sound sets, follow these specifications:

### Required Files
```
lib/sounds/[YourSetName]/
├── intro.mp3               # Opening music
├── fff_question.mp3        # FFF question reading
├── fff_thinking.mp3        # FFF countdown
├── fff_reveal.mp3          # FFF results
├── question_reading.mp3    # Main game question
├── thinking_music_1.mp3    # Level 1-5
├── thinking_music_2.mp3    # Level 6-10
├── thinking_music_3.mp3    # Level 11-15
├── final_answer.mp3        # Final answer cue
├── correct_answer.mp3      # Correct answer reveal
├── wrong_answer.mp3        # Wrong answer
├── walk_away.mp3           # Player walks away
├── lifeline_5050.mp3       # 50:50 activation
├── lifeline_phone.mp3      # Phone a Friend
├── lifeline_audience.mp3   # Ask the Audience
└── win_game.mp3            # Top prize win
```

### Audio Specifications
- **Format**: MP3, WAV, OGG, or FLAC
- **Bitrate**: 128-320 kbps (MP3)
- **Sample Rate**: 44.1 kHz or 48 kHz
- **Channels**: Mono or Stereo

> **Note**: Audio powered by CSCore library with DSP capabilities (silence detection, crossfading)

---

## Troubleshooting Installation

### Issue: ".NET 8 not found"
**Solution:** Install .NET 8 Desktop Runtime from Microsoft

### Issue: "Database initialization failed"
**Solution:** 
- Check disk space (need ~100 MB)
- Run as administrator
- Check antivirus isn't blocking

### Issue: "Application won't start"
**Solution:**
- Check Event Viewer for errors
- Verify .NET version: `dotnet --list-runtimes`
- Reinstall application

### Issue: "Sounds not playing"
**Solution:**
- Verify sound files exist in `lib\sounds\Default\`
- Check Windows volume mixer
- Test with different sound set

### Issue: "TV Screen not appearing"
**Solution:**
- Check if blocked by taskbar (set taskbar to auto-hide)
- Verify display resolution supported
- Check graphics drivers updated

---

## Next Steps

Installation complete! Continue to:
- **[Quick Start Guide](Quick-Start-Guide)** - Start playing immediately
- **[User Guide](User-Guide)** - Learn all features
- **[Troubleshooting](Troubleshooting)** - Solve common issues

---

**Need help?** [Report an issue](https://github.com/Macronair/TheMillionaireGame/issues) or check [Troubleshooting](Troubleshooting).
