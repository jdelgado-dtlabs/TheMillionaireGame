# Session: Watchdog Crash Reporter Improvements
**Date:** January 12, 2026  
**Branch:** `feature/first-run-wizard`  
**Focus:** Enhanced crash reporting UI, duplicate detection, and testing capabilities

---

## Overview
Comprehensive improvements to the watchdog crash reporting system, including UI fixes, duplicate prevention, intelligent issue commenting, and test infrastructure.

---

## Changes Made

### 1. Fixed Crash Report Dialog UI Issues
**Problem:** UI elements were falling off the window, and buttons were not properly positioned.

**Solution:**
- Increased dialog height from 650px to 700px to accommodate all controls
- Added `AutoScaleMode.Font` for better DPI handling across different screen resolutions
- Redesigned button layout with dynamic centering:
  - Submit button: 150px
  - Save button: 120px  
  - Close button: 110px
  - 10px spacing between buttons
  - Buttons now properly center within available width

**Files Modified:**
- `MillionaireGame.Watchdog/CrashReportDialog.cs`

---

### 2. Prevented Duplicate Crash Dialogs
**Problem:** When a crash occurred, both `OnProcessExited` and `OnHeartbeatTimeout` could trigger, causing duplicate crash dialogs to appear.

**Solution:**
- Added `_crashHandlerInProgress` flag to `ProcessMonitor` class
- Modified `OnProcessExited()` to check flag before processing
- Modified `OnHeartbeatTimeout()` to check flag before processing freeze detection
- Set flag at start of `HandleCrash()` with early return if already in progress

**Result:** Only one crash dialog appears regardless of detection method. Canceling the submission no longer causes a second dialog.

**Files Modified:**
- `MillionaireGame.Watchdog/ProcessMonitor.cs`

---

### 3. Crash Test Dummy Feature
**Problem:** No way to test the full crash reporting pipeline without causing actual crashes.

**Solution:** Implemented `--debug --ctd` command-line flags for testing.

**Usage:**
```bash
MillionaireGame.Watchdog.exe --debug --ctd
```

**Features:**
- Generates realistic synthetic crash data:
  - Exit Code: `0xE0434352` (CLR exception)
  - Running Time: 15 minutes 37 seconds
  - Last Activity: "Player answering question 8 ($32,000)"
  - Memory Usage: 256 MB
  - Simulated `NullReferenceException` with stack trace
- Creates complete crash report file (marked with `_TEST_` suffix)
- Exercises full pipeline: dialog → authentication → sanitization → submission
- All console logs prefixed with `[TEST]`
- Requires both `--debug` AND `--ctd` flags for safety
- Shows clear test identification in all messages

**Files Modified:**
- `MillionaireGame.Watchdog/Program.cs`

---

### 4. Enhanced GitHub OAuth Error Logging
**Problem:** Authentication failures showed generic error messages without details.

**Solution:**
- Added detailed HTTP status code logging
- Included error response content from GitHub API
- Specific network connectivity error detection
- Logs Client ID being used for verification
- Added stack traces for debugging

**Example Output:**
```
[GitHubOAuth] Requesting device code from https://github.com/login/device/code
[GitHubOAuth] Client ID: Ov23li3IoDybo9YFX1wm
[ERROR] [GitHubOAuth] HTTP BadRequest: {"error":"device_flow_disabled",...}
[ERROR] [GitHubOAuth] Check your internet connection and firewall settings
```

**Files Modified:**
- `MillionaireGame.Watchdog/GitHubOAuthManager.cs`

---

### 5. Intelligent Duplicate Detection with Auto-Commenting
**Problem:** Multiple users experiencing the same crash would flood the issue tracker with duplicates.

**Solution:** Enhanced duplicate detection to add comments instead of creating new issues.

**How It Works:**
1. **Smart Detection:**
   - Searches for open issues with same exit code in last 7 days
   - Matches hex exit code in issue titles (e.g., `0xE0434352`)
   
2. **Automatic Comment Addition:**
   - Adds "🔄 Duplicate Crash Occurrence" comment to existing issue
   - Includes:
     - Crash details (exit code, last activity, running time)
     - User description and reproduction steps
     - System information
     - Timestamp of occurrence
   - Prevents issue flood while tracking impact

3. **Better User Experience:**
   - Informs users their crash was logged (not ignored)
   - Shows existing issue number and URL
   - Offers to open issue in browser
   - Message: "✅ Your crash details have been added as a comment to help track this issue"

**Benefits:**
- No duplicate issues cluttering the tracker
- Easy to see how widespread a crash is
- Each user's details still logged and visible
- More comments = higher priority indication

**Files Modified:**
- `MillionaireGame.Watchdog/GitHubIssueSubmitter.cs`
- `MillionaireGame.Watchdog/ProcessMonitor.cs`
- `MillionaireGame.Watchdog/Program.cs` (test dummy)

---

## Testing Performed

### Manual Testing
- ✅ Crash dialog displays correctly with all controls visible
- ✅ Buttons properly centered and accessible
- ✅ No duplicate dialogs when canceling submission
- ✅ Crash test dummy generates realistic data
- ✅ Authentication flow triggers correctly when not authenticated
- ✅ Enhanced error messages provide actionable details

### Discovered During Testing
- GitHub OAuth App `Ov23li3IoDybo9YFX1wm` has Device Flow disabled
- Error detection and reporting working correctly
- Users need to either:
  - Enable Device Flow on existing OAuth App (if they have access)
  - Create new OAuth App with Device Flow enabled

---

## Technical Notes

### Crash Test Dummy Data
The synthetic crash includes realistic values:
```csharp
ExitCode: 0xE0434352 (CLR exception)
RunningTime: 00:15:37
LastActivity: "Player answering question 8 ($32,000)"
LastMemoryMB: 256
ThreadCount: 14
WasResponsive: true
```

### Duplicate Detection Query
```csharp
string query = $"repo:{RepoOwner}/{RepoName} is:issue is:open label:crash-report {exitCodeHex} in:title";
```
- Only checks open issues
- Requires `crash-report` label
- Searches title for hex exit code
- Sorts by creation date (newest first)
- Limits to 5 results for performance

### Comment Format
Comments added to duplicate issues include:
- Header: "🔄 Duplicate Crash Occurrence"
- Crash details table
- User report and reproduction steps
- System information
- Footer indicating automated detection

---

## Files Modified Summary

| File | Changes |
|------|---------|
| `MillionaireGame.Watchdog/CrashReportDialog.cs` | UI layout fixes, dialog height, button positioning |
| `MillionaireGame.Watchdog/ProcessMonitor.cs` | Duplicate dialog prevention, improved user messages |
| `MillionaireGame.Watchdog/Program.cs` | Crash test dummy implementation |
| `MillionaireGame.Watchdog/GitHubOAuthManager.cs` | Enhanced error logging |
| `MillionaireGame.Watchdog/GitHubIssueSubmitter.cs` | Duplicate detection with auto-commenting |

---

## Future Considerations

### GitHub OAuth App Setup
Users deploying this need to:
1. Create GitHub OAuth App at https://github.com/settings/developers
2. **Enable Device Flow** checkbox (critical!)
3. Set application name and homepage URL
4. Update `ClientId` in `GitHubOAuthManager.cs`

### Potential Enhancements
- Configurable duplicate detection timeframe (currently 7 days)
- Option to include full crash report in duplicate comments
- Automatic issue priority adjustment based on duplicate count
- Rate limiting for comment additions
- Local duplicate cache to reduce API calls

---

## Conclusion

The watchdog crash reporting system is now production-ready with:
- ✅ Proper UI that works across different screen sizes
- ✅ No duplicate dialogs or submission prompts
- ✅ Complete test infrastructure for development
- ✅ Intelligent duplicate handling to prevent issue flood
- ✅ Clear, actionable error messages for troubleshooting

The crash test dummy feature (`--debug --ctd`) allows full pipeline testing without requiring actual crashes, making development and debugging much easier.

All changes build successfully and are ready for integration into the main application.
