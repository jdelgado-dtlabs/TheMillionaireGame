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
   - Location: `%LocalAppData%\MillionaireGame\CrashReports\`
   - Open most recent report
   - Check for specific error

2. **Database Issues**
   - Reset SQL Server Express database:
     ```sql
     -- Connect to SQL Server Express and run:
     DROP DATABASE IF EXISTS dbMillionaire;
     ```
   - Or use application's database reset option if available
   - Application will recreate database on next launch

3. **Reset Configuration**
   - Settings stored in SQL Server Express database:
     ```sql
     -- Connect to SQL Server Express and run:
     USE dbMillionaire;
     DELETE FROM ApplicationSettings;
     -- Or drop and recreate entire database:
     USE master;
     DROP DATABASE IF EXISTS dbMillionaire;
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

3. **Check Full Screen Settings**
   - Settings → Screens tab → Multiple Monitor Control
   - Verify correct monitor selected for TV Screen
   - Enable/disable "Auto Full Screen TV Screen" as needed

4. **Restart Application**
   - Close and reopen to reset window positions
   - Windows will appear on primary monitor by default

### TV Screen Too Small / Wrong Size

**Symptom**: TV Screen doesn't fill screen or is wrong resolution

**Solutions:**

1. **Enable Full Screen**
   - Settings → Screens tab → Multiple Monitor Control
   - Check "Auto Full Screen TV Screen"
   - Select correct monitor from dropdown
   - Restart application

2. **Maximize Window Manually**
   - Click maximize button on TV Screen window
   - This enters borderless fullscreen mode
   - Press ESC to exit fullscreen

3. **Check Display Scaling**
   - Windows Settings → Display
   - Set scaling to 100% for best results
   - Higher scaling (125%, 150%) may affect layout

4. **Verify Resolution**
   - Right-click desktop → Display Settings
   - Set to native resolution (usually 1920x1080)
   - Update graphics drivers if resolution issues persist

### Display Flickering / Tearing

**Symptom**: Screen flickers or shows tearing during animations

**Solutions:**

1. **Update Graphics Drivers**
   - Download latest from manufacturer (NVIDIA, AMD, Intel)
   - Outdated drivers are common cause of rendering issues

2. **Check Windows Display Settings**
   - Right-click desktop → Display Settings → Advanced Display
   - Verify monitor refresh rate is set correctly (usually 60Hz)

3. **Disable Desktop Composition**
   - Right-click application EXE → Properties → Compatibility
   - Check "Disable fullscreen optimizations"
   - May help with tearing issues

4. **Restart Application**
   - Graphics state may be corrupted
   - Close and reopen application

---

## Audio Issues

### No Sound Playing

**Symptom**: Complete silence, no sounds at all

**Solutions:**

1. **Check Application Audio Gains**
   - Settings → Audio Settings → Audio Processing
   - Verify Master Gain is not set to minimum (-20dB)
   - Verify Effects Gain and Music Gain are not at minimum
   - Default is 0dB for all gains

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

4. **Select Correct Sound Pack**
   - Settings → Sounds tab → Soundpack tab
   - Select "Default"
   - Click "Apply"

5. **Test Windows Sound**
   - Play other audio (YouTube, music player)
   - If Windows sound also not working, check Windows audio settings

### Some Sounds Missing

**Symptom**: Some sounds play, others don't

**Solutions:**

1. **Check Channel Gains**
   - Settings → Audio Settings → Audio Processing
   - Verify Effects Gain and Music Gain are not set to minimum (-20dB)
   - Default is 0dB for both channels

2. **Verify Sound Files**
   - Navigate to: `C:\Program Files\The Millionaire Game\lib\sounds\Default\`
   - Check for missing files (sound files should be .mp3, .wav, or .ogg)
   - If files missing, reinstall application

3. **Audio Format Issues**
   - Sound files must be MP3, WAV, or OGG
   - Check file isn't corrupted (try playing in media player)

### Sound Crackling / Distorted

**Symptom**: Audio plays but sounds distorted

**Solutions:**

1. **Update Audio Drivers**
   - Device Manager → Sound, video and game controllers
   - Update audio device drivers

2. **Select Different Audio Device**
   - Settings → Mixer tab
   - Try selecting a different audio output device
   - Some devices handle audio processing better than others

3. **Close Other Audio Applications**
   - Other apps can interfere with audio
   - Close unnecessary programs

4. **Check Limiter Settings**
   - Settings → Audio Settings → Audio Processing
   - Try toggling "Enable Limiter" checkbox
   - Limiter prevents audio clipping but may affect quality

### Audio Playback Issues

**Symptom**: Sounds cut off early or don't play completely

**Solutions:**

1. **Adjust Silence Detection**
   - Settings → Audio Settings → Silence Detection
   - Increase "Silence Duration (ms)" if sounds cutting off too early
   - Increase "Threshold (dB)" if sounds stopping prematurely
   - Disable "Enable Silence Detection" to play sounds fully

2. **Check Crossfade Settings**
   - Settings → Audio Settings → Crossfade
   - Disable "Enable Crossfade" if sounds overlapping incorrectly

3. **Reduce System Load**
   - Close background applications
   - Audio issues often from high CPU usage

---

## Database Issues

### "Cannot Connect to Database"

**Symptom**: Error on startup about database connection

**Solutions:**

1. **SQL Server Express Not Installed**
   - SQL Server Express is required (installed automatically by installer)
   - Verify installation:
     ```powershell
     Get-Service | Where-Object {$_.Name -like "*SQL*"}
     # Should show MSSQL$SQLEXPRESS or similar
     ```
   - If missing, reinstall application or install SQL Server Express separately

2. **Database Corrupt**
   ```sql
   -- Backup database first (optional)
   BACKUP DATABASE dbMillionaire TO DISK = 'C:\Backup\dbMillionaire.bak';
   
   -- Drop and recreate
   DROP DATABASE IF EXISTS dbMillionaire;
   -- Application will recreate on next launch
   ```

3. **Connection String Issue**
   - Open `App.config` in installation folder
   - Verify connection string format:
   ```xml
   <add name="MillionaireDB" 
        connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=dbMillionaire;Integrated Security=True" />
   ```

4. **Permissions Issue**
   - Run application as administrator (first time only)
   - Grants database creation permissions

### Questions Not Loading

**Symptom**: Game starts but no questions available

**Solutions:**

1. **Open Question Editor**
   - Control Panel → Game menu → Editor
   - Opens standalone Question Editor window
   - View question count per difficulty level in Regular Questions and FFF Questions tabs

2. **Import Questions from CSV**
   - In Question Editor: Toolbar → Import button
   - Select CSV file with questions
   - See User Guide for CSV format

3. **Verify Database**
   ```powershell
   # Check if SQL Server Express service is running
   Get-Service MSSQL$SQLEXPRESS
   # Should show Status: Running
   ```

4. **Reset Used Questions**
   - In Question Editor: Toolbar → Reset Used button
   - Marks all questions as unused
   - Allows questions to be reused

### Telemetry Not Saving

**Symptom**: Games played but no history in Telemetry tab

**Solutions:**

1. **Telemetry is Always Enabled**
   - There is no setting to disable telemetry
   - All games are automatically tracked in the database
   - If telemetry not appearing, it's a database issue

2. **Database Space**
   - Check disk space on drive containing `%LocalAppData%\TheMillionaireGame\`
   - Need at least 100 MB free

3. **Database Permissions**
   - Verify SQL Server Express has write permissions
   - Check Windows user has access to SQL Server Express instance

---

## Web Server Issues

### Web Server Won't Start

**Symptom**: "Start Web Server" button does nothing or shows error

**Solutions:**

1. **Port Already in Use**
   - Another application using port 5278
   - Check with:
   ```powershell
   netstat -ano | findstr :5278
   ```
   - Change port in Settings → Audience

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
   - Settings → Audience
   - Verify bind address: `0.0.0.0` (all interfaces) or `localhost` (local only)
   - Reset to defaults if unsure

### Audience Can't Connect

**Symptom**: Web server running but audience can't access

**Solutions:**

1. **Check Bind Address Configuration**
   - Settings → Audience
   - Select either:
     - A local IP address with `/prefix` (e.g., `192.168.1.100/24 - Local Network`)
     - OR `0.0.0.0 - All Interfaces (Open to All)` for all networks
   - Do NOT use `127.0.0.1 - Localhost Only` (only allows connections from the host computer)
   - Local IP restricts to that network subnet, 0.0.0.0 allows all interfaces
   - Restart web server after changing setting

2. **Check Network Connection**
   - Ensure audience on same WiFi network
   - Try pinging computer:
   ```powershell
   # On host computer, find IP
   ipconfig
   # Note IPv4 address (e.g., 192.168.1.100)
   
   # On audience device, ping
   ping 192.168.1.100
   ```

3. **Firewall Rules**
   - Windows Firewall must allow incoming connections
   - Create inbound rule for port 5278

4. **Use Correct URL**
   - Not `localhost` (only works on host computer)
   - Use IP address shown in Control Panel
   - Example: `http://192.168.1.100:5278`

5. **Router AP Isolation**
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

2. **Update Graphics Drivers**
   - Outdated drivers can cause slowness

3. **Disable Background Processes**
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

3. **Disable Audio Processing**
   - Settings → Sounds tab → Audio Settings tab
   - Disable "Enable Limiter" to reduce CPU usage
   - Disable "Enable Silence Detection" if not needed
   - Disable "Enable Crossfade" if not needed

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
   - Each lifeline can only be used once per game

2. **Lifeline Not Available Yet**
   - Lifelines only become active after all 4 answers are revealed
   - Check lifeline button color: Grey = used/unavailable, Green = active

3. **Total Lifelines Setting**
   - Settings → Lifelines tab
   - Check "Total number of lifelines" setting
   - If set to less than 4, some lifelines will be unavailable

### Money Tree Not Updating

**Symptom**: Prize level doesn't advance after correct answer

**Solutions:**

1. **Check Game Console**
   - Look for errors in console log
   - May indicate state corruption

2. **Restart Game**
   - Control Panel → Reset Game button (Delete key)
   - Clears game state and starts fresh

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
   - From `%LocalAppData%\MillionaireGame\CrashReports\`

4. **Configuration Files**
   - `App.config`
   - Database: SQL Server Express (dbMillionaire database)

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

### Database Repair

If database corrupted beyond recovery:

```sql
-- Full reset (WARNING: Loses all data)
-- Connect to SQL Server Express and run:
USE master;
GO
DROP DATABASE IF EXISTS dbMillionaire;
GO
-- Application recreates database on next launch
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
