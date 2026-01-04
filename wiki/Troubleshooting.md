# Troubleshooting

Solutions to common issues and problems with The Millionaire Game.

---

## Quick Fixes

Before diving into specific issues, try these general fixes:

1. **Restart the Application**
2. **Update to Latest Version** - Check [Releases](https://github.com/jdelgado-dtlabs/TheMillionaireGame/releases)
3. **Check .NET Runtime** - Ensure .NET 8 Desktop Runtime installed
4. **Review Logs** - Check `%LocalAppData%\TheMillionaireGame\Logs\` for error details
5. **Run as Administrator** - Right-click → Run as administrator

---

## Installation Issues

### Application Won't Install

**Symptom**: Installer fails or shows errors

**Solutions:**

1. **Check Prerequisites**
   ```powershell
   # Verify .NET 8 installed
   dotnet --list-runtimes
   # Should show: Microsoft.WindowsDesktop.App 8.0.x
   ```

2. **Install .NET 8 Runtime**
   - Download: [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)
   - Install and restart computer

3. **Disable Antivirus Temporarily**
   - Some antivirus software blocks installation
   - Disable during install, re-enable after

4. **Check Disk Space**
   - Need ~500 MB free space
   - Clear temporary files if needed

5. **Run Installer as Administrator**
   - Right-click installer → Run as administrator

### "Windows Protected Your PC" Warning

**Symptom**: SmartScreen blocks installation

**Solution:**
1. Click **"More info"**
2. Click **"Run anyway"**
3. This is normal for new/unsigned applications

---

## Launch Issues

### Application Won't Start

**Symptom**: Double-click icon, nothing happens

**Solutions:**

1. **Check Task Manager**
   - `Ctrl+Shift+Esc` → Processes
   - Look for "MillionaireGame.exe"
   - If present, end task and retry

2. **Verify .NET Installation**
   ```powershell
   dotnet --list-runtimes
   ```
   - Must have `Microsoft.WindowsDesktop.App 8.0.x`

3. **Check Event Viewer**
   - `Win+R` → `eventvwr.msc`
   - Windows Logs → Application
   - Look for MillionaireGame errors
   - Note error code/message

4. **Run from Command Line** (see detailed errors)
   ```powershell
   cd "C:\Program Files\The Millionaire Game"
   .\MillionaireGame.exe
   ```

5. **Reinstall Application**
   - Uninstall completely
   - Delete leftover files:
     - `C:\Program Files\The Millionaire Game\`
     - `%LocalAppData%\TheMillionaireGame\`
   - Reinstall fresh

### Application Crashes on Startup

**Symptom**: Opens briefly then closes

**Solutions:**

1. **Check Crash Reports**
   - Location: `%LocalAppData%\TheMillionaireGame\CrashReports\`
   - Open most recent report
   - Check for specific error

2. **Database Issues**
   - Delete corrupt database:
     ```powershell
     Remove-Item "$env:LOCALAPPDATA\TheMillionaireGame\Database\*" -Recurse
     ```
   - Application will recreate on next launch

3. **Reset Configuration**
   - Settings stored in database, delete ApplicationSettings table:
     ```powershell
     # Delete entire database to reset all settings
     Remove-Item "$env:LOCALAPPDATA\TheMillionaireGame\Database\*" -Recurse
     ```
   - Application will recreate with defaults on next launch

4. **Graphics Driver Update**
   - Outdated drivers can cause crashes
   - Update from manufacturer's website (NVIDIA, AMD, Intel)

### "Application is Already Running"

**Symptom**: Error message preventing launch

**Solutions:**

1. **Close Existing Instance**
   ```powershell
   Stop-Process -Name "MillionaireGame*" -Force
   ```

2. **Check Background Processes**
   - Task Manager → Details tab
   - Look for `MillionaireGame.exe` or `MillionaireGame.Watchdog.exe`
   - End all processes

3. **Orphaned Lock File**
   ```powershell
   Remove-Item "$env:LOCALAPPDATA\TheMillionaireGame\.lock"
   ```

---

## Display Issues

### TV Screen Not Appearing

**Symptom**: Control Panel opens but no TV Screen

**Solutions:**

1. **Check Behind Other Windows**
   - Press `Alt+Tab` to cycle windows
   - Look for "TV Screen - The Millionaire Game"

2. **Check Monitor Configuration**
   - Windows Display Settings
   - Ensure "Extend displays" selected
   - TV Screen may be on disconnected monitor

3. **Restore Window Position**
   - Settings → Display → "Reset Window Positions"
   - Restart application

4. **Force TV Screen Visible**
   - Control Panel → View → "Show TV Screen"
   - Or press `F11` (toggles full screen)

### TV Screen Too Small / Wrong Size

**Symptom**: TV Screen doesn't fill screen or is wrong resolution

**Solutions:**

1. **Enter Full Screen Mode**
   - Press `F11` in TV Screen window
   - Or: Control Panel → "Full Screen" button

2. **Check Display Scaling**
   - Windows Settings → Display
   - Set scaling to 100% for best results
   - Or adjust application to match scaling

3. **Verify Resolution**
   - Right-click desktop → Display Settings
   - Set to native resolution (usually 1920x1080)
   - Restart application

4. **Graphics Rendering Issue**
   - Update graphics drivers
   - Settings → Display → Use "Software Rendering" (slower but compatible)

### Display Flickering / Tearing

**Symptom**: Screen flickers or shows tearing during animations

**Solutions:**

1. **Enable VSync**
   - Settings → Display → "Enable VSync"
   - Reduces tearing but may add input lag

2. **Update Graphics Drivers**
   - Download latest from manufacturer

3. **Reduce Animation Effects**
   - Settings → Display → "Reduce Animations"
   - Disables confetti and some transitions

4. **Check Monitor Refresh Rate**
   - Display Settings → Advanced Display
   - Set to monitor's native refresh rate (60Hz, 144Hz, etc.)

---

## Audio Issues

### No Sound Playing

**Symptom**: Complete silence, no sounds at all

**Solutions:**

1. **Check Application Volume**
   - Control Panel → Audio section
   - Master Volume slider to 100%
   - Unmute if muted (`M` key)

2. **Windows Volume Mixer**
   - Right-click speaker icon → Open Volume Mixer
   - Check MillionaireGame.exe volume level
   - Ensure not muted

3. **Verify Sound Files Exist**
   ```powershell
   # Check if sound files present
   Test-Path "C:\Program Files\The Millionaire Game\lib\sounds\Default\*.mp3"
   ```
   - Should return `True`
   - If False, reinstall application

4. **Select Correct Sound Profile**
   - Settings → Audio → Sound Profile
   - Select "Default"
   - Click "Apply"

5. **Test Windows Sound**
   - Play other audio (YouTube, music player)
   - If Windows sound also not working, check Windows audio settings

### Some Sounds Missing

**Symptom**: Some sounds play, others don't

**Solutions:**

1. **Check Individual Volumes**
   - Music Volume slider
   - Effects Volume slider
   - Voice Volume slider
   - Ensure all > 0%

2. **Verify Sound Files**
   - Navigate to: `lib/sounds/[CurrentProfile]/`
   - Check for missing files (see [User Guide - Sound Files](User-Guide#audio-system))
   - Re-download or copy from Default profile

3. **Audio Format Issues**
   - Sound files must be MP3, WAV, or OGG
   - Check file isn't corrupted (try playing in media player)

### Sound Crackling / Distorted

**Symptom**: Audio plays but sounds distorted

**Solutions:**

1. **Update Audio Drivers**
   - Device Manager → Sound, video and game controllers
   - Update audio device drivers

2. **Adjust Buffer Size**
   - Settings → Audio → Advanced
   - Increase "Audio Buffer Size"
   - Larger = less crackling but more latency

3. **Close Other Audio Applications**
   - Other apps can interfere with audio
   - Close unnecessary programs

4. **Check Audio Enhancement**
   - Windows Sound Settings → Device Properties
   - Disable "Audio Enhancements"

### Audio Out of Sync

**Symptom**: Sound plays late or early

**Solutions:**

1. **Adjust Audio Delay**
   - Settings → Audio → "Audio Delay"
   - Add positive delay (ms) if sound too early
   - Add negative delay if sound too late

2. **Reduce System Load**
   - Close background applications
   - Audio sync issues often from high CPU usage

---

## Database Issues

### "Cannot Connect to Database"

**Symptom**: Error on startup about database connection

**Solutions:**

1. **LocalDB Not Installed**
   - .NET 8 SDK includes LocalDB
   - Or install separately: [SQL Server Express LocalDB](https://www.microsoft.com/sql-server/sql-server-downloads)

2. **Database File Corrupt**
   ```powershell
   # Backup existing database
   Copy-Item "$env:LOCALAPPDATA\The Millionaire Game\Database\*" "C:\Backup\" -Recurse
   
   # Delete corrupt database
   Remove-Item "$env:LOCALAPPDATA\The Millionaire Game\Database\*" -Recurse
   
   # Application will recreate on next launch
   ```

3. **Connection String Issue**
   - Open `App.config` in installation folder
   - Verify connection string format:
   ```xml
   <add name="MillionaireDB" 
        connectionString="Data Source=(localdb)\MSSQLLocalDB;..." />
   ```

4. **Permissions Issue**
   - Run application as administrator (first time only)
   - Grants database creation permissions

### Questions Not Loading

**Symptom**: Game starts but no questions available

**Solutions:**

1. **Check Questions Tab**
   - Control Panel → Questions tab
   - View question count per difficulty level

2. **Import Sample Questions**
   - Questions tab → "Import Sample Questions"
   - Loads 50 default questions

3. **Verify Database**
   ```powershell
   # Check if database exists
   Test-Path "$env:LOCALAPPDATA\TheMillionaireGame\Database\"
   ```

4. **Reimport Questions**
   - If you have backup CSV:
   - Questions tab → Import CSV
   - Select your question file

### Telemetry Not Saving

**Symptom**: Games played but no history in Telemetry tab

**Solutions:**

1. **Check Telemetry Enabled**
   - Settings → Telemetry
   - Ensure "Enable Telemetry" checked

2. **Database Space**
   - Check disk space on drive containing `%LocalAppData%\TheMillionaireGame\`
   - Need at least 100 MB free

3. **Database Permissions**
   - Database file may be read-only
   - Right-click database file → Properties
   - Uncheck "Read-only"

---

## Web Server Issues

### Web Server Won't Start

**Symptom**: "Start Web Server" button does nothing or shows error

**Solutions:**

1. **Port Already in Use**
   - Another application using port 5000/5001
   - Check with:
   ```powershell
   netstat -ano | findstr :5000
   ```
   - Change port in Settings → Web Integration

2. **Firewall Blocking**
   - Windows Firewall may block server
   - Allow through firewall:
     - Windows Security → Firewall → Allow an app
     - Add MillionaireGame.exe
     - Allow on Private and Public networks

3. **Administrator Rights**
   - Some systems require admin to bind ports
   - Run application as administrator

4. **Invalid Configuration**
   - Settings → Web Integration
   - Verify bind address: `0.0.0.0` (all interfaces) or `localhost` (local only)
   - Reset to defaults if unsure

### Audience Can't Connect

**Symptom**: Web server running but audience can't access

**Solutions:**

1. **Check Network Connection**
   - Ensure audience on same WiFi network
   - Try pinging computer:
   ```powershell
   # On host computer, find IP
   ipconfig
   # Note IPv4 address (e.g., 192.168.1.100)
   
   # On audience device, ping
   ping 192.168.1.100
   ```

2. **Firewall Rules**
   - Windows Firewall must allow incoming connections
   - Create inbound rule for port 5000/5001

3. **Use Correct URL**
   - Not `localhost` (only works on host computer)
   - Use IP address shown in Control Panel
   - Example: `http://192.168.1.100:5000`

4. **Router AP Isolation**
   - Some routers isolate devices (hotel/public WiFi)
   - Cannot communicate between devices
   - Use different network or disable AP isolation

### Voting Not Working

**Symptom**: Audience connected but votes not registering

**Solutions:**

1. **Check Browser Console**
   - On audience device: Press F12
   - Look for JavaScript errors
   - Look for WebSocket connection errors

2. **Clear Browser Cache**
   - Cached old version of web page
   - Hard refresh: `Ctrl+F5`

3. **Voting Already Closed**
   - Operator may have closed voting
   - Wait for next "Ask the Audience"

4. **SignalR Connection Lost**
   - Web server may have restarted
   - Refresh page on audience devices

---

## Performance Issues

### Application Running Slow

**Symptom**: Laggy interface, slow responses

**Solutions:**

1. **Check System Resources**
   - Task Manager → Performance tab
   - Look for high CPU or memory usage
   - Close unnecessary applications

2. **Reduce Visual Effects**
   - Settings → Display
   - Disable "Confetti Animations"
   - Enable "Reduce Animations"
   - Lower frame rate if needed

3. **Update Graphics Drivers**
   - Outdated drivers can cause slowness

4. **Disable Background Processes**
   - Close browser tabs
   - Disable Windows indexing temporarily
   - Close cloud sync (OneDrive, Dropbox)

### High CPU Usage

**Symptom**: Computer hot, fans loud, CPU usage >50%

**Solutions:**

1. **Check for Infinite Loops**
   - Shouldn't happen but may be bug
   - Check `%LocalAppData%\TheMillionaireGame\Logs\` for errors
   - Report to developers if consistent

2. **Disable Auto-Updates**
   - Windows Update running in background
   - Pause updates during event

3. **Audio Processing**
   - Settings → Audio → Reduce "Audio Quality"
   - Lower sample rate uses less CPU

### Memory Leak

**Symptom**: Memory usage increases over time, eventually crashes

**Solutions:**

1. **Restart Application Periodically**
   - Between games or during breaks
   - Clears memory

2. **Report Bug**
   - Note when memory usage increases (specific actions?)
   - Submit bug report with details

---

## Game Logic Issues

### Wrong Answer Shown as Correct

**Symptom**: Contestant gives wrong answer but marked correct

**Solutions:**

1. **Check Question Database**
   - Questions tab → Find question
   - Verify correct answer marked properly
   - Edit and fix if wrong

2. **Operator Error**
   - Operator may have clicked wrong button
   - Be careful when selecting answer

### Lifeline Not Working

**Symptom**: Click lifeline button, nothing happens

**Solutions:**

1. **Lifeline Already Used**
   - Check lifeline status indicator
   - Each lifeline can only be used once

2. **No Lifelines Configured**
   - Game Profile may have lifelines disabled
   - Edit profile → Enable lifelines

3. **Game State Issue**
   - Try restarting question
   - If persists, restart game

### Money Tree Not Updating

**Symptom**: Prize level doesn't advance after correct answer

**Solutions:**

1. **Check Game Console**
   - Look for errors in console log
   - May indicate state corruption

2. **Restart Game**
   - Save progress if possible
   - Start fresh game

---

## Hotkey Issues

### Hotkeys Not Working

**Symptom**: Keyboard shortcuts don't respond

**Solutions:**

1. **Check Application Focus**
   - Click on Control Panel window
   - Hotkeys only work when application focused

2. **Conflicting Applications**
   - Other software may capture hotkeys
   - Close overlay apps (Discord, Steam, etc.)

3. **Keyboard Layout**
   - Some layouts have different key codes
   - Change hotkeys in Settings → Hotkeys

4. **Reset to Defaults**
   - Settings → Hotkeys → "Reset to Defaults"

---

## Advanced Troubleshooting

### Collecting Diagnostic Information

If issue persists, gather this information for support:

1. **System Information**
   ```powershell
   systeminfo > systeminfo.txt
   ```

2. **Application Logs**
   - Collect all files from `%LocalAppData%\TheMillionaireGame\Logs\`

3. **Crash Reports**
   - From `%LocalAppData%\TheMillionaireGame\CrashReports\`

4. **Configuration Files**
   - `App.config`
   - Database: `%LocalAppData%\TheMillionaireGame\Database\` (contains all settings)

5. **Screenshots/Videos**
   - Capture the issue occurring
   - Use Windows Snipping Tool or OBS

### Enabling Debug Mode

For detailed logging:

1. Edit `App.config`:
   ```xml
   <add key="LogLevel" value="Debug" />
   ```

2. Restart application

3. Reproduce issue

4. Check `%LocalAppData%\TheMillionaireGame\Logs\` for detailed log files

### Safe Mode Launch

Launch with minimal features:

```powershell
cd "C:\Program Files\The Millionaire Game"
.\MillionaireGame.exe --safe-mode
```

This disables:
- Web server
- Sound system
- Advanced graphics

Useful for isolating issues.

### Database Repair

If database corrupted beyond recovery:

```powershell
# Full reset (WARNING: Loses all data)
Remove-Item "$env:LOCALAPPDATA\TheMillionaireGame\Database\*" -Recurse -Force

# Application recreates database on next launch
```

---

## Getting Help

### Before Asking for Help

1. ✅ Check this troubleshooting guide
2. ✅ Search [existing issues](https://github.com/jdelgado-dtlabs/TheMillionaireGame/issues)
3. ✅ Review application logs
4. ✅ Try on another computer (if possible)

### Reporting Issues

**Create Issue on GitHub:**
1. Go to [GitHub Issues](https://github.com/jdelgado-dtlabs/TheMillionaireGame/issues)
2. Click "New Issue"
3. Choose appropriate template
4. Provide:
   - Clear description of problem
   - Steps to reproduce
   - Expected vs actual behavior
   - System information
   - Logs/screenshots

**Include:**
- Windows version
- .NET version (`dotnet --version`)
- Application version (Help → About)
- Relevant log files
- Screenshots if visual issue

### Community Support

- **Discussions**: [GitHub Discussions](https://github.com/jdelgado-dtlabs/TheMillionaireGame/discussions)
- **Email**: [Contact maintainer]

---

## Known Issues

### Current Known Issues

Check [GitHub Issues](https://github.com/jdelgado-dtlabs/TheMillionaireGame/issues?q=is%3Aissue+is%3Aopen+label%3Abug) for current list.

### Workarounds

Some issues may have temporary workarounds documented in their GitHub issue threads.

---

## Frequently Asked Questions

See [FAQ Section](User-Guide#faq) in User Guide.

---

**Issue not listed?** [Create a new issue](https://github.com/jdelgado-dtlabs/TheMillionaireGame/issues/new) on GitHub.
