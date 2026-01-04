# Crash Handler & Watchdog System Documentation

## Overview
The Crash Handler & Watchdog System provides automated crash detection, reporting, and recovery for The Millionaire Game. This system monitors the main application for crashes and freezes, generates comprehensive diagnostic reports, and helps maintain application stability.

## Architecture

### Components
1. **MillionaireGame.Watchdog** - Standalone console application that monitors the main application
2. **HeartbeatService** - Integrated into main application to send periodic status updates
3. **HeartbeatListener** - Named pipe server in watchdog to receive heartbeats
4. **ProcessMonitor** - Manages application process lifecycle and crash detection
5. **CrashReportGenerator** - Creates detailed diagnostic reports

### Communication Flow
```
Main Application (MillionaireGame.exe)
    |
    | (HeartbeatService sends heartbeat every 5 seconds)
    |
    v
Named Pipe: "MillionaireGame.Heartbeat"
    |
    v
Watchdog (MillionaireGame.Watchdog.exe)
    |
    | (HeartbeatListener receives and validates)
    |
    v
ProcessMonitor
    |
    ├─> Detects clean shutdown (State: "ShuttingDown")
    ├─> Detects crash (non-zero exit code)
    └─> Detects freeze (no heartbeat for 15+ seconds)
    |
    v
CrashReportGenerator
    |
    └─> Saves diagnostic report to:
        %LOCALAPPDATA%\MillionaireGame\CrashReports\
```

## Heartbeat Protocol

### HeartbeatMessage Structure
```csharp
{
    "Timestamp": "2024-01-15T14:32:05.123Z",
    "State": "Running",              // Running, ShuttingDown
    "ThreadCount": 42,
    "MemoryUsageMB": 156,
    "CurrentActivity": "Playing Game Round 5"
}
```

### Heartbeat Intervals
- **Send Interval**: 5 seconds
- **Timeout Threshold**: 15 seconds (no heartbeat = frozen)
- **Initial Delay**: 2 seconds (allow app to initialize)

### Application States
- **Running**: Normal operation
- **ShuttingDown**: Graceful exit in progress (prevents false crash detection)

## Crash Detection

### Crash Criteria
The watchdog detects a crash when:
1. Process exits with non-zero exit code (not 0 or 1)
2. Process exits without sending "ShuttingDown" state
3. No heartbeat received for 15+ seconds (frozen/hung)

### Common Exit Codes
- `0`: Clean exit
- `1`: User-requested exit
- `-1`: General error
- `-532462766` (0xE0434352): .NET unhandled exception
- `-1073741819` (0xC0000005): Access violation
- `-1073740791` (0xC00000FD): Stack overflow
- `-1073741510` (0xC000013A): Application hang

## Crash Reports

### Report Location
Crash reports are saved to:
```
%LOCALAPPDATA%\MillionaireGame\CrashReports\
```

Typical path:
```
C:\Users\[Username]\AppData\Local\MillionaireGame\CrashReports\
```

### Report Format
```
====================================
MILLIONAIRE GAME CRASH REPORT
====================================
Crash Time: 2024-01-15 14:35:42
Report Generated: 2024-01-15 14:35:43

[PROCESS INFORMATION]
- Process ID: 12345
- Exit Code: -532462766 (0xE0434352)
- Exit Code Meaning: .NET unhandled exception
- Running Time: 00:12:34
- Last Heartbeat: 2024-01-15 14:35:30 (12.5s ago)
- Was Responsive: No (frozen/hung)

[APPLICATION STATE]
- Build Info: 1.0.0
- Last State: Running
- Last Activity: Playing Game Round 5

[SYSTEM INFORMATION]
- OS: Microsoft Windows NT 10.0.22631.0
- 64-bit OS: True
- 64-bit Process: True
- Processor Count: 8
- .NET Runtime: 8.0.0

[RESOURCE USAGE (Last Known)]
- Memory: 156 MB
- Threads: 42

[RECENT LOGS]
(Last 100 lines from GameConsole.log)

====================================
END OF CRASH REPORT
====================================
```

### Report Retention
- Maximum 10 reports are kept
- Older reports are automatically deleted
- Reports are named: `CrashReport_YYYYMMDD_HHMMSS.txt`

## Usage

### Running with Watchdog
Use the provided launcher script:
```batch
Launch-WithWatchdog.bat
```

Or manually:
```batch
MillionaireGame.Watchdog.exe "path\to\MillionaireGame.exe"
```

### Running without Watchdog
The main application can run standalone:
```batch
MillionaireGame.exe
```

**Note**: Without the watchdog, crashes will not be monitored or reported. The HeartbeatService will silently fail to connect (expected behavior).

## Integration Points

### Main Application (Program.cs)
```csharp
// Start heartbeat service
_heartbeatService = new HeartbeatService();
_heartbeatService.Start();
_heartbeatService.SetActivity("Initializing");

// Update activity as needed
_heartbeatService.SetActivity("Playing Game Round 5");

// Stop on exit
_heartbeatService.Stop();
_heartbeatService.Dispose();
```

### Activity Tracking
Update the current activity to help diagnose where crashes occur:

```csharp
_heartbeatService.SetActivity("Loading questions");
_heartbeatService.SetActivity("Starting new game");
_heartbeatService.SetActivity("Playing round 5");
_heartbeatService.SetActivity("Processing lifeline");
_heartbeatService.SetActivity("Saving telemetry");
```

## Testing Crash Detection

### Test Scenario 1: Clean Exit
1. Launch with watchdog
2. Exit normally through UI
3. **Expected**: No crash report

### Test Scenario 2: Forced Termination
1. Launch with watchdog
2. Kill process via Task Manager
3. **Expected**: Crash report generated

### Test Scenario 3: Unhandled Exception
1. Modify code to throw unhandled exception
2. Launch with watchdog
3. Trigger exception
4. **Expected**: Crash report with exception details

### Test Scenario 4: Freeze Detection
1. Modify code to create infinite loop (no UI updates)
2. Launch with watchdog
3. Wait 15+ seconds
4. **Expected**: Watchdog detects freeze, terminates process, generates report

## Troubleshooting

### Watchdog Not Detecting Crashes
- **Issue**: Crashes occur but no reports generated
- **Cause**: Application not running through watchdog
- **Solution**: Use `Launch-WithWatchdog.bat` to start

### False Positive Freeze Detection
- **Issue**: Watchdog reports freeze during long operations
- **Cause**: Operation takes >15 seconds, heartbeat thread blocked
- **Solution**: Ensure long operations run on background threads, not UI thread

### Named Pipe Connection Failures
- **Issue**: Heartbeat service logs connection errors
- **Cause**: Watchdog not running (expected)
- **Solution**: Normal when running without watchdog. Errors are silently ignored.

### Missing Crash Reports
- **Issue**: Crash occurred but report file not found
- **Cause**: Report directory creation failed or permissions issue
- **Solution**: Check permissions for `%LOCALAPPDATA%\MillionaireGame\` directory

## Performance Impact

### Overhead
- **HeartbeatService**: Minimal (<1% CPU)
  - Timer fires every 5 seconds
  - Named pipe write is non-blocking
  - Failures are silently ignored
  
- **Watchdog Process**: Negligible
  - Lightweight console application
  - No UI rendering
  - Named pipe listener is event-driven

### Memory Usage
- **HeartbeatService**: ~50 KB
- **Watchdog Process**: ~10 MB

## Security Considerations

### Named Pipe Security
- Pipe name: `MillionaireGame.Heartbeat`
- Direction: One-way (Main → Watchdog)
- Scope: Local machine only
- Permissions: Current user only

### Crash Report Data
Crash reports may contain:
- Application state and activity
- Memory usage statistics
- Recent log entries
- System information

**Privacy**: Do not share crash reports publicly without reviewing contents. They are saved locally only.

## Future Enhancements

### Potential Improvements
1. **Automatic Restart**: Option to automatically restart application after crash
2. **Crash Statistics**: Track crash frequency and patterns
3. **Remote Reporting**: Submit crash reports to developer
4. **Mini Dump Capture**: Full memory dump for advanced debugging
5. **Performance Monitoring**: Extended telemetry (CPU, I/O, network)

### Integration with Telemetry
Future versions may integrate crash data with the game telemetry system for comprehensive session diagnostics.

## Version History

### v1.0.0 (Initial Release)
- Heartbeat-based crash detection
- Named pipe communication
- Comprehensive crash reports
- Freeze detection (15-second timeout)
- Automatic report cleanup
- Integration with GameConsole logging

## Support

For issues or questions regarding the crash handler system:
1. Check crash reports in `%LOCALAPPDATA%\MillionaireGame\CrashReports\`
2. Review GameConsole.log for heartbeat diagnostics
3. Verify watchdog is running when testing crash detection
4. Check this documentation for troubleshooting steps
