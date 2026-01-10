# Release Notes - Version 1.0.5
**The Millionaire Game - C# Edition**  
**Release Date**: January 9, 2026  
**Build Status**: ✅ Production Ready

---

## 🎯 Overview

Version 1.0.5 focuses on **mobile/tablet optimization** and **critical bug fixes** for the Fastest Finger First (FFF) online system. This release enhances the audience participation experience with improved device detection, responsive design fixes, and diagnostic tools for live show troubleshooting.

---

## ✨ What's New

### Enhanced Mobile/Tablet Detection
- **Multi-Strategy Device Detection**: Implements 4 detection strategies to accurately identify tablets and mobile devices
  - User-Agent pattern matching
  - Android-specific checks (Android without Mobile flag)
  - Screen size heuristics (>=768px width or height)
  - Touch capability + screen size combination
- **Proper Tablet Classification**: Android tablets now correctly detected as "Tablet" instead of "Desktop"
- **Mobile Feature Activation**: Tablets now receive wake lock, fullscreen mode, and haptic feedback
- **Console Logging**: Each detection path logs to console for debugging

### On-Screen Debug Panel
- **Live Show Diagnostics**: Fixed-position overlay showing device information
- **Information Displayed**:
  - Device type (Mobile/Tablet/Desktop)
  - Screen resolution
  - Touch support status
  - Wake lock status
  - User agent string (truncated)
- **Auto-Hide**: Panel automatically disappears after 10 seconds
- **Manual Close**: Red X button for immediate dismissal
- **Mobile/Tablet Only**: Only displays on Mobile and Tablet devices
- **Crash Prevention**: Comprehensive error handling with 100ms DOM readiness delay
- **Use Case**: Helps operators diagnose audience member connection issues during live shows

### Mobile Container Optimization
- **Dynamic Height Calculation**: JavaScript calculates container max-height using actual `window.innerHeight - 40px`
- **More Reliable than Viewport Units**: Avoids CSS vh/dvh inconsistencies across mobile browsers
- **Orientation Support**: Automatically recalculates on resize and orientation change (100ms delay)
- **Vertical Centering**: Uses `margin: auto 0` for proper centering with overflow support
- **Internal Scrolling**: Maintains `overflow-y: auto` when content exceeds calculated height

---

## 🐛 Critical Bug Fixes

### FFF No-Winner Scenario Handling
**Problem**: When no participants answered correctly in FFF mode, clicking "Confirm Winner" threw an error instead of showing the retry message.

**Root Cause**: 
- `UpdateUIState()` didn't distinguish between single-winner and no-winner scenarios
- `btnWinner_Click()` had overly strict guard preventing no-winner code execution

**Solution**:
- Split `UpdateUIState()` logic into 3 branches: multi-winner/single-winner/no-winner
- No-winner button now shows **orange color** as visual indicator
- Removed strict guard to allow no-winner handling code to execute
- QuestionReady state explicitly disables all downstream buttons to prevent re-clicking

**Result**: 
- Clicking Confirm Winner with no winners properly displays "❌ No Winners" message
- Plays wrong answer sound
- Broadcasts NoWinner to web clients
- Resets to QuestionReady state for retry

### Web UI Submit Button Visual State
**Problem**: Submit button appeared greyed out but was still functionally clickable on new FFF questions.

**Root Cause**: 'disabled-mode' CSS class wasn't removed when re-enabling button.

**Solution**:
- `startFFFQuestion()` now explicitly removes 'disabled-mode' class alongside setting `disabled = false`
- Answer items properly re-enabled with `pointerEvents: 'auto'` and `opacity: 1`

**Result**: Submit button appears and functions correctly on all new questions.

### Web Screen Scrolling Issues
**Problem**: Timer expiration caused orange message boxes to appear below viewport without scrolling ability.

**Root Cause**: Screen transitions didn't ensure new content was visible.

**Solution**:
- Added `window.scrollTo({ top: 0, behavior: 'smooth' })` to `showScreen()` function
- All screen transitions now automatically scroll to top

**Result**: Newly displayed content always visible without manual scrolling.

### Wake Lock Debugging Enhancements
**Improvements**:
- Added `document.visibilityState` check before requesting wake lock
- Enhanced error logging with emoji indicators (✓, ⚠️, ❌, 💡)
- Specific handling for `NotAllowedError` (requires user interaction)
- Logs wake lock type and released status for diagnostics

---

## 🔄 Changes

### FFF Instructions Removed from Web Client
- Removed yellow instruction box from web interface
- Instructions explained pregame by host during setup phase
- Cleaner, more streamlined participant interface

---

## 📊 Technical Details

### Files Modified (14 commits)
1. `FFFOnlinePanel.cs` - FFF no-winner flow fixes (commits 3620524, 92ef1d9)
2. `app.js` - Mobile detection, debug panel, container height, scrolling (commits 9b66083, eec1e65, eb598a8, 2475473, ef71101, e0d7e1a, 3f0edd7)
3. `index.html` - Debug panel structure, removed instructions (commits d790e6a, ef71101)
4. `app.css` - Container positioning and overflow (commits a02528b, eec1e65, eb598a8)

### JavaScript Version
- Bumped to **0.6.4-ephemeral** (Ephemeral Native-Like Experience with mobile fixes)

### Browser Compatibility
- Tested on Chrome Android (tablets and phones)
- Tested on Mobile Safari (iOS)
- Tested on Desktop Chrome, Firefox, Edge

### Build Status
- ✅ Build succeeded with 2 acceptable warnings (MDnsServiceManager nullable fields)
- ✅ All 14 commits pushed to origin/master-v1.0.5
- ✅ Application tested on Desktop, Mobile, and Tablet devices

---

## 📦 Deployment

### Requirements
- .NET 8 Desktop Runtime (x64)
- Windows 10/11 (64-bit)
- SQL Server LocalDB or SQL Server Express

### Installation
1. Run `MillionaireGameSetup.exe`
2. Follow installation wizard
3. Launch application via Start Menu or Desktop shortcut

### Upgrade from v1.0.1
- Application automatically migrates existing database schema
- Settings and question data preserved
- No manual intervention required

---

## 🎮 Usage Notes

### Mobile/Tablet Participation
- Android tablets now properly detected and receive mobile features
- Wake lock keeps screen on during gameplay
- Fullscreen mode hides browser chrome for immersive experience
- Haptic feedback on button presses (10ms vibration)
- Debug panel helps troubleshoot connection issues

### Operator Guide
- **FFF No-Winner Scenario**: Orange "Confirm Winner" button indicates no winners, click to show retry message
- **Debug Panel**: Visible on audience mobile/tablet devices for 10 seconds after page load
- **Network Discovery**: Continue using wwtbam.local for automatic IP discovery

---

## 🔮 What's Next

Version 1.0.6 development planning:
- Database migration system for future schema changes
- Progressive Web App (PWA) enhancements
- UPnP/SSDP discovery alternatives
- State synchronization improvements

---

## 📝 Credits

**Development**: Jean Francois Delgado  
**Based on Original Work**: Marco Loenen (Macronair)  
**License**: See LICENSE file

---

## 🐛 Known Issues

- None reported for v1.0.5

---

## 📞 Support

For issues, questions, or feature requests:
- GitHub Issues: https://github.com/Macronair/TheMillionaireGame/issues
- Wiki: https://github.com/Macronair/TheMillionaireGame/wiki
