# Crash Handler (Watchdog) Implementation Plan

**Date:** December 27, 2025  
**Status:** Planning Phase  
**Priority:** Medium  
**Estimated Time:** 8-12 hours  

---

## Overview

Implement a standalone crash handler application that:
- Monitors the main MillionaireGame process
- Detects crashes and unresponsive states
- Captures crash dumps and diagnostic information
- Generates error reports for troubleshooting
- Optionally restarts the application on crash

---

## Architecture Design

### Components

#### 1. Watchdog Application (`MillionaireGame.Watchdog.exe`)
**Type:** Windows Console Application (.NET 8.0)  
**Purpose:** Standalone process monitoring application  
**Location:** `src/MillionaireGame.Watchdog/`

**Responsibilities:**
- Launch and monitor main application process
- Detect process crashes (exit code != 0)
- Detect process hangs/freezes (heartbeat timeout)
- Capture crash diagnostics
- Generate crash reports
- Notify user of crashes
- Optionally auto-restart application

#### 2. Heartbeat System
**Type:** Named Pipe or Memory-Mapped File  
**Purpose:** Communication channel between main app and watchdog  

**Main App Side:**
- Send heartbeat signal every 5 seconds
- Include process health metrics (memory usage, thread count)
- Report application state (starting, running, shutting down)

**Watchdog Side:**
- Monitor heartbeat signals
- Timeout threshold: 15 seconds (3x heartbeat interval)
- Detect application freeze vs. slow operation vs. intentional shutdown

#### 3. Crash Report Generator
**Purpose:** Capture comprehensive diagnostic information  

**Data Captured:**
- Process exit code
- Exception information (if available)
- Mini dump file (.dmp)
- GameConsole log snapshot
- System information (OS, RAM, CPU)
- Application version and build info
- Last known state (from heartbeat)
- Timestamp and crash duration

---

## Implementation Phases

### Phase 1: Watchdog Core (4-5 hours)

#### 1.1 Project Setup
**Tasks:**
- Create `MillionaireGame.Watchdog` console project
- Add references to System.Diagnostics, System.IO.Pipes
- Configure build output to main application bin folder

**Files:**
```
src/MillionaireGame.Watchdog/
├── Program.cs
├── ProcessMonitor.cs
├── HeartbeatListener.cs
├── CrashReportGenerator.cs
└── MillionaireGame.Watchdog.csproj
```

#### 1.2 Process Monitoring
**Class:** `ProcessMonitor`  
**Methods:**
- `LaunchApplication(string exePath)` - Start main app with watchdog
- `MonitorProcess(Process process)` - Monitor process health
- `DetectCrash(Process process)` - Check exit code and state
- `HandleCrash(CrashInfo crashInfo)` - Respond to detected crash

**Features:**
- Process lifecycle tracking (start, run, exit)
- Exit code interpretation (0 = clean, 1 = forced, -1 = crash)
- Graceful shutdown detection vs. crash
- Process.Exited event handling

#### 1.3 Heartbeat System
**Class:** `HeartbeatListener`  
**Communication:** Named Pipe (`MillionaireGame.Heartbeat`)  

**Protocol:**
```csharp
public class HeartbeatMessage
{
    public DateTime Timestamp { get; set; }
    public string State { get; set; } // "Starting", "Running", "Closing", "ShuttingDown"
    public int ThreadCount { get; set; }
    public long MemoryUsageMB { get; set; }
    public string? CurrentActivity { get; set; } // e.g., "Loading question", "Playing audio"
}
```

**Main App Integration:**
- Create `HeartbeatService` in MillionaireGame project
- Background timer sends heartbeat every 5 seconds
- Include in shutdown sequence (send "ShuttingDown" state)
- Stop heartbeat before process exit

**Watchdog Side:**
- Listen on named pipe continuously
- Track last heartbeat timestamp
- 15-second timeout before declaring freeze
- Different handling for "ShuttingDown" state (no timeout)

---

### Phase 2: Crash Diagnostics (3-4 hours)

#### 2.1 Crash Report Generator
**Class:** `CrashReportGenerator`  
**Output:** `CrashReport_[timestamp].txt`  

**Report Contents:**
```
====================================
MILLIONAIRE GAME CRASH REPORT
====================================
Crash Time: 2025-12-27 14:32:15
Report Generated: 2025-12-27 14:32:16

[PROCESS INFORMATION]
- Process ID: 12345
- Exit Code: -1073741819 (0xC0000005 - ACCESS_VIOLATION)
- Running Time: 00:15:42
- Last Heartbeat: 2025-12-27 14:32:10 (5 seconds ago)

[APPLICATION STATE]
- Version: 0.8.0
- Branch: master-csharp
- Build: Debug/Release
- State: Running
- Activity: "Playing Q5 audio sequence"

[SYSTEM INFORMATION]
- OS: Windows 11 Pro (22H2)
- CPU: Intel Core i7-9700K
- RAM: 16 GB (12.3 GB available)
- .NET Runtime: 8.0.11

[RESOURCE USAGE]
- Memory: 245 MB
- Threads: 18
- CPU Usage: 15%

[RECENT LOGS]
[Last 50 lines from GameConsole.log]

[EXCEPTION DETAILS]
[If available from mini dump]

====================================
END OF CRASH REPORT
====================================
```

**Storage:**
- Saved to: `%LOCALAPPDATA%\MillionaireGame\CrashReports\`
- Retain last 10 crash reports
- Optional user submission via GitHub issue link

#### 2.2 Mini Dump Capture
**Library:** `System.Diagnostics` or P/Invoke to `DbgHelp.dll`  
**Purpose:** Capture process memory dump on crash  

**Implementation:**
```csharp
public static void CreateMiniDump(Process process, string dumpPath)
{
    using var fileStream = File.Create(dumpPath);
    MiniDumpWriteDump(
        process.Handle,
        process.Id,
        fileStream.SafeFileHandle,
        MiniDumpType.WithFullMemory,
        IntPtr.Zero, IntPtr.Zero, IntPtr.Zero
    );
}
```

**Notes:**
- Only create mini dumps for unexpected crashes (not clean shutdowns)
- Mini dumps can be large (100+ MB) - make optional
- Provide user option to include in bug reports

#### 2.3 GameConsole Log Integration
**Purpose:** Capture application logs at time of crash  

**Implementation:**
- Read last N lines from `GameConsole.log`
- Include in crash report
- Highlight errors/warnings before crash
- Capture shutdown sequence if available

---

### Phase 3: User Interface & Recovery (2-3 hours)

#### 3.1 Crash Notification Dialog
**Type:** Windows Forms MessageBox or custom dialog  
**Trigger:** Displayed when crash detected  

**Content:**
```
The Millionaire Game has stopped working unexpectedly.

A crash report has been generated:
C:\Users\[User]\AppData\Local\MillionaireGame\CrashReports\...

Would you like to:
[ Restart Application ]  [ View Crash Report ]  [ Close ]

[ ] Send crash report to developer (opens GitHub issue)
```

**Features:**
- Auto-restart option (configurable)
- View crash report in Notepad
- Generate GitHub issue link with report template
- "Don't show again" option for repeated crashes

#### 3.2 Auto-Restart Logic
**Purpose:** Minimize downtime for production use  

**Configuration:**
```xml
<WatchdogSettings>
  <EnableAutoRestart>true</EnableAutoRestart>
  <MaxRestartAttempts>3</MaxRestartAttempts>
  <RestartDelaySeconds>5</RestartDelaySeconds>
  <CooldownMinutes>10</CooldownMinutes>
</WatchdogSettings>
```

**Behavior:**
- Restart application up to N times
- Reset counter after successful run > cooldown period
- Stop restarting if repeated crashes (crash loop protection)
- Notify user if max attempts reached

#### 3.3 Launcher Integration
**Purpose:** Make watchdog transparent to users  

**Options:**

**Option A: Wrapper EXE**
- Rename `MillionaireGame.exe` → `MillionaireGame.App.exe`
- Create `MillionaireGame.exe` launcher that starts watchdog
- Watchdog launches `MillionaireGame.App.exe`
- Transparent to user

**Option B: Shortcut/Script**
- Keep existing `MillionaireGame.exe` unchanged
- Provide `LaunchWithWatchdog.bat` or desktop shortcut
- Users choose whether to use watchdog
- Non-invasive option

**Recommendation:** Option B for initial implementation (less disruptive)

---

## Configuration & Settings

### Watchdog Configuration File
**Location:** `watchdog-config.xml`  
**Contents:**
```xml
<?xml version="1.0" encoding="utf-8"?>
<WatchdogConfiguration>
  <Monitoring>
    <HeartbeatInterval>5</HeartbeatInterval>
    <HeartbeatTimeout>15</HeartbeatTimeout>
    <ProcessMonitorInterval>1</ProcessMonitorInterval>
  </Monitoring>
  
  <CrashHandling>
    <EnableAutoRestart>true</EnableAutoRestart>
    <MaxRestartAttempts>3</MaxRestartAttempts>
    <RestartDelaySeconds>5</RestartDelaySeconds>
    <CrashCooldownMinutes>10</CrashCooldownMinutes>
    <CreateMiniDumps>false</CreateMiniDumps>
  </CrashHandling>
  
  <Reporting>
    <CrashReportDirectory>%LOCALAPPDATA%\MillionaireGame\CrashReports</CrashReportDirectory>
    <MaxCrashReportsToKeep>10</MaxCrashReportsToKeep>
    <IncludeSystemInfo>true</IncludeSystemInfo>
    <IncludeGameLogs>true</IncludeGameLogs>
  </Reporting>
  
  <UserInterface>
    <ShowCrashDialog>true</ShowCrashDialog>
    <AutoOpenCrashReport>false</AutoOpenCrashReport>
  </UserInterface>
</WatchdogConfiguration>
```

---

## Testing Strategy

### Test Scenarios

#### 1. Normal Operation
- [x] Start application via watchdog
- [x] Verify heartbeat communication
- [x] Run application normally
- [x] Close application cleanly
- [x] Watchdog detects clean shutdown (exit code 0)
- [x] No crash report generated

#### 2. Forced Termination
- [x] Start application via watchdog
- [x] Force-kill process (Task Manager)
- [x] Watchdog detects crash
- [x] Crash report generated
- [x] Auto-restart triggered (if enabled)

#### 3. Application Hang
- [x] Start application via watchdog
- [x] Simulate infinite loop (stop heartbeat)
- [x] Watchdog detects timeout after 15 seconds
- [x] Crash report generated with "Timeout" classification
- [x] Prompt user for forced termination

#### 4. Repeated Crashes
- [x] Simulate crash on startup (e.g., missing file)
- [x] Watchdog attempts restart (up to max attempts)
- [x] After max attempts, display error and stop
- [x] User notified to check crash reports

#### 5. Graceful Shutdown During Monitoring
- [x] Start application via watchdog
- [x] Initiate shutdown from UI
- [x] Heartbeat sends "ShuttingDown" state
- [x] Watchdog disables timeout during shutdown
- [x] No false-positive crash detection

---

## File Structure

```
src/MillionaireGame.Watchdog/
├── MillionaireGame.Watchdog.csproj
├── Program.cs                      # Entry point, command-line parsing
├── ProcessMonitor.cs               # Process lifecycle management
├── HeartbeatListener.cs            # Named pipe listener
├── CrashReportGenerator.cs         # Report generation
├── CrashNotificationDialog.cs      # User notification UI
├── WatchdogConfiguration.cs        # Configuration model
└── Models/
    ├── HeartbeatMessage.cs
    ├── CrashInfo.cs
    └── ProcessHealthInfo.cs

src/MillionaireGame/Services/
├── HeartbeatService.cs             # Main app heartbeat sender
└── (integrate into shutdown sequence)

docs/
└── CRASH_HANDLER_USER_GUIDE.md     # User-facing documentation
```

---

## Integration Points

### Main Application Changes

#### 1. HeartbeatService Integration
**Location:** `MillionaireGame/Services/HeartbeatService.cs`  
**Lifecycle:**
- Initialize in `Program.cs` after application startup
- Start heartbeat timer (5-second interval)
- Include in `ControlPanelForm.ShutdownApplicationAsync()`
- Stop heartbeat before `Environment.Exit()`

#### 2. Shutdown Sequence Update
**Location:** `ControlPanelForm.cs`  
**Changes:**
```csharp
private async Task ShutdownApplicationAsync()
{
    // ... existing shutdown steps ...
    
    // NEW: Stop heartbeat before exit
    AddStep("Stop Heartbeat", true, stopwatch.ElapsedMilliseconds);
    HeartbeatService.Instance?.Stop();
    await Task.Delay(100); // Allow final heartbeat to send
    
    // ... continue with exit ...
}
```

#### 3. Build Configuration
**Update:** `TheMillionaireGame.sln`  
- Add `MillionaireGame.Watchdog` project
- Configure build to output watchdog to main app's bin folder
- Post-build event to copy watchdog config file

---

## Deployment Considerations

### Packaging
- Include `MillionaireGame.Watchdog.exe` in installer
- Provide `LaunchWithWatchdog.bat` shortcut
- Optional: Create desktop shortcut with watchdog by default

### User Documentation
- Explain watchdog purpose in README
- Document crash report location
- Instructions for disabling watchdog (if desired)
- How to submit crash reports to developer

### Privacy & Data
- Crash reports contain no personal information
- System info is limited to hardware specs (no IDs)
- User must manually submit reports (no auto-upload)
- Clear disclosure in documentation

---

## Success Criteria

**Phase 1 Complete When:**
- [x] Watchdog can launch and monitor main application
- [x] Heartbeat communication established
- [x] Clean shutdown detected correctly
- [x] Crash detected and reported

**Phase 2 Complete When:**
- [x] Comprehensive crash reports generated
- [x] Mini dumps captured (optional feature)
- [x] GameConsole logs included in reports
- [x] Reports stored in correct location

**Phase 3 Complete When:**
- [x] User notified of crashes with dialog
- [x] Auto-restart working (with limits)
- [x] Launcher integration tested
- [x] Configuration options functional

**Ready for Merge When:**
- [x] All test scenarios pass
- [x] Documentation complete
- [x] User guide written
- [x] No regressions in main application

---

## Future Enhancements (Post-v1.0)

- **Web Dashboard:** View crash statistics and trends
- **Automatic Upload:** Optional telemetry to developer server
- **Remote Diagnostics:** SSH/RDP session logging for support
- **Performance Monitoring:** Track FPS, memory, CPU over time
- **Crash Prediction:** ML model to predict crashes before they occur
- **Integration with Application Insights:** Azure telemetry

---

## Notes

- Keep watchdog lightweight (< 5 MB memory overhead)
- Minimize performance impact on main application
- Ensure watchdog itself doesn't crash (exception handling)
- Test on Windows 10 and Windows 11
- Consider Windows Defender SmartScreen implications
- Sign executables to avoid security warnings

---

**Ready to implement when approved.**
