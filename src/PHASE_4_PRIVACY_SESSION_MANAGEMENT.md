# Phase 4: Privacy-First Session Management

## Overview
Phase 4 implements a privacy-first, ephemeral session approach for the audience participation system. Unlike traditional Progressive Web Apps (PWA) that encourage installation and persistent storage, this implementation focuses on temporary, one-time use cases with automatic cleanup after the show ends.

**Version**: 0.6.2-2512  
**Status**: ✅ Implemented  
**Date**: December 2025

## Philosophy

### Design Principles
1. **No Installation Required** - Users access via browser, no app installation
2. **Ephemeral Sessions** - Data expires after show ends (4-hour maximum)
3. **Privacy-First** - No persistent identifying information
4. **Auto-Cleanup** - Automatic cache and data clearing
5. **One-Time Use** - Each show is treated as a fresh session

### Security Rationale
- **GDPR/Privacy Compliance**: Minimal data retention
- **Storage Optimization**: No device bloat after show
- **Security Best Practice**: No client-side copies of application or data
- **Fresh Start**: Returning participants treated as new users

## Implementation Details

### 1. Cache Prevention (Server-Side)

**File**: `Program.cs`

Added middleware to prevent browser caching of application files:

```csharp
// Add cache prevention headers for privacy/security
app.Use(async (context, next) =>
{
    // Prevent caching of HTML, JS, and CSS files for ephemeral sessions
    var path = context.Request.Path.Value?.ToLowerInvariant();
    if (path != null && (path.EndsWith(".html") || path.EndsWith(".js") || path.EndsWith(".css") || path == "/"))
    {
        context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate, max-age=0";
        context.Response.Headers["Pragma"] = "no-cache";
        context.Response.Headers["Expires"] = "0";
    }
    await next();
});
```

**Headers Applied**:
- `Cache-Control: no-cache, no-store, must-revalidate, max-age=0`
- `Pragma: no-cache`
- `Expires: 0`

### 2. Client-Side Meta Tags

**File**: `index.html`

Added privacy-focused meta tags:

```html
<!-- Privacy & Security Meta Tags -->
<meta name="robots" content="noindex, nofollow, noarchive">
<meta http-equiv="Cache-Control" content="no-cache, no-store, must-revalidate">
<meta http-equiv="Pragma" content="no-cache">
<meta http-equiv="Expires" content="0">
<meta name="referrer" content="no-referrer">
```

**Purpose**:
- Prevent search engine indexing
- Disable browser caching
- Block referrer tracking
- Enhance privacy

### 3. Session Management

**File**: `app.js`

#### Configuration Constants

```javascript
const SESSION_CONFIG = {
    maxSessionDuration: 4 * 60 * 60 * 1000, // 4 hours (typical show duration)
    warningBeforeExpiry: 15 * 60 * 1000,    // 15 minutes warning
    checkInterval: 60 * 1000                 // Check every minute
};

const STORAGE_KEYS = {
    PARTICIPANT_ID: 'waps_participant_id',
    SESSION_ID: 'waps_session_id',
    DISPLAY_NAME: 'waps_display_name',
    AUTO_SESSION_ID: 'waps_auto_session_id',
    SESSION_TIMESTAMP: 'waps_session_timestamp'  // NEW
};
```

#### Session Expiry Timer

**Function**: `startSessionExpiryTimer()`

- Starts when user joins session successfully
- Checks every minute for session expiry
- Shows warning 15 minutes before expiry
- Auto-disconnects and clears data when expired

**Flow**:
1. Store session start timestamp in localStorage
2. Check elapsed time every minute
3. If 15 minutes remaining → show warning message
4. If expired → disconnect SignalR → clear all data → return to join screen

#### Data Cleanup

**Function**: `clearSessionData()`

Comprehensive cleanup function that:
- Clears all state variables (`currentSessionId`, `currentParticipantId`, etc.)
- Removes all localStorage items (using `STORAGE_KEYS` mapping)
- Clears sessionStorage completely
- Stops session expiry timer
- Logs cleanup for debugging

**Triggered By**:
- Session expiry (automatic)
- User clicks "Leave" button
- Page unload (if not connected)
- Browser back/forward cache restoration

#### Cleanup Event Handlers

**Function**: `setupCleanupHandlers()`

Handles various browser events for privacy:

```javascript
// 1. Page Unload
window.addEventListener('beforeunload', () => {
    // Clear data if not actively connected
    if (!connection || connection.state !== signalR.HubConnectionState.Connected) {
        clearSessionData();
    }
});

// 2. Visibility Change
document.addEventListener('visibilitychange', () => {
    if (document.hidden) {
        console.log("Page hidden - maintaining session but ready for cleanup");
    }
});

// 3. Back/Forward Cache (bfcache)
window.addEventListener('pageshow', (event) => {
    if (event.persisted) {
        // Force reload if page restored from cache
        window.location.reload();
    }
});
```

**Purpose**:
- **beforeunload**: Clean up when user navigates away
- **visibilitychange**: Monitor but maintain active sessions when tab hidden
- **pageshow**: Force fresh load if browser restores from cache

## User Experience

### Session Lifecycle

1. **Join Session**
   - User enters display name
   - Auto-generated session code applied
   - Connects via SignalR
   - Session timestamp stored
   - **Expiry timer starts** ⏱️

2. **Active Participation**
   - User remains connected
   - Participates in FFF/ATA
   - Session monitored every minute
   - Data temporarily stored in localStorage

3. **15-Minute Warning** ⚠️
   - Warning message displayed
   - "Session will expire in X minutes. The show will end soon."
   - Gives users heads-up before expiry

4. **Session Expiry** 🔒
   - After 4 hours maximum
   - SignalR connection stopped
   - "Your session has expired. Thank you for participating!"
   - 3-second delay for message visibility
   - **Complete data wipe**
   - Return to join screen

5. **Manual Leave** 👋
   - User clicks "Leave" button
   - Connection stopped
   - **Immediate data wipe**
   - Return to join screen

6. **Browser Close** 🚪
   - If connected: Session maintained (quick return possible)
   - If disconnected: Data wiped for privacy
   - Back/forward cache: Force reload

## Technical Specifications

### Storage Strategy

| Data Type | Storage Location | Lifetime | Cleanup Trigger |
|-----------|-----------------|----------|----------------|
| Participant ID | localStorage | Session duration | Manual/Auto cleanup |
| Session ID | localStorage | Session duration | Manual/Auto cleanup |
| Display Name | localStorage | Session duration | Manual/Auto cleanup |
| Session Timestamp | localStorage | Session duration | Manual/Auto cleanup |
| SignalR State | Memory | Connection lifetime | Disconnect |

### Timing Configuration

| Parameter | Default Value | Description |
|-----------|---------------|-------------|
| Max Session Duration | 4 hours | Typical game show length |
| Warning Period | 15 minutes | User notification before expiry |
| Check Interval | 1 minute | How often to check for expiry |

### Cache Control

| File Type | Cache Headers | Effect |
|-----------|---------------|--------|
| HTML (/, *.html) | no-cache, no-store, must-revalidate | Always fetch fresh |
| JavaScript (*.js) | no-cache, no-store, must-revalidate | Always fetch fresh |
| CSS (*.css) | no-cache, no-store, must-revalidate | Always fetch fresh |
| Static Assets | Standard caching | Images/fonts can cache |

## Benefits

### For Users
✅ No installation required - just open URL  
✅ No app icon cluttering home screen  
✅ No persistent data on device  
✅ No storage bloat  
✅ Privacy-respecting experience  

### For Producers
✅ No long-term data liability  
✅ GDPR/privacy compliance  
✅ Fresh start each show  
✅ No user data management burden  
✅ Automatic cleanup - no manual intervention  

### For Security
✅ Minimal attack surface  
✅ No persistent client-side data  
✅ No long-lived sessions  
✅ Automatic expiry enforcement  
✅ Fresh sessions prevent replay attacks  

## Testing Checklist

- [ ] Join session → verify timestamp stored
- [ ] Wait 1 minute → verify timer running (check console)
- [ ] Simulate 3h 45m elapsed → verify warning message appears
- [ ] Simulate 4h elapsed → verify auto-disconnect and cleanup
- [ ] Click "Leave" button → verify immediate cleanup
- [ ] Close tab while connected → verify data persists (quick return)
- [ ] Close tab while disconnected → verify data wiped
- [ ] Refresh page → verify forced reload from bfcache
- [ ] Check browser cache → verify no HTML/JS/CSS cached
- [ ] Check localStorage after cleanup → verify empty

## Future Enhancements

### Potential Additions
- **Server-Side Session Expiry**: Coordinate with backend session timeout
- **Configurable Duration**: Admin can adjust 4-hour limit per show
- **Grace Period**: Allow brief reconnection after expiry
- **Session Recovery**: Optional "resume session" within 5-minute window
- **Analytics Opt-In**: Optional anonymous usage statistics

### Not Planned (By Design)
- ❌ PWA manifest.json (no installation)
- ❌ Service worker (no offline caching)
- ❌ Persistent storage (no IndexedDB/Cache API)
- ❌ Background sync (no background tasks)
- ❌ Push notifications (no persistent engagement)

## Migration Notes

### Changes from Phase 3
- Added session expiry timer
- Added automatic cleanup handlers
- Added privacy meta tags
- Added server-side cache prevention
- Modified leave button to use `clearSessionData()`
- Removed any PWA-related artifacts (manifest.json)

### Breaking Changes
None - all changes are additive and backward compatible.

### Version Bump
- From: `0.6.1-2512` (Code refactoring)
- To: `0.6.2-2512` (Privacy & session management)

## Conclusion

Phase 4 successfully implements a privacy-first, ephemeral session approach that:
- Respects user privacy with minimal data retention
- Provides automatic cleanup after show completion
- Prevents device bloat and storage concerns
- Ensures security through short-lived sessions
- Maintains excellent user experience with clear warnings

This approach is ideal for one-time events like game shows where:
- Audience participation is temporary
- No long-term user relationship needed
- Privacy and data minimization are priorities
- Device storage conservation is valued

The system now provides a native-feeling web experience without the persistence of traditional installed apps.

---

**Next Phase**: Phase 5 - Responsive Mobile Enhancements (Optional)
