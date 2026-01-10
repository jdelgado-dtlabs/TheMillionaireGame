# Progressive Web App (PWA) Implementation Plan
**Feature Branch**: `feature/pwa-native-app-experience`  
**Date**: January 9, 2026  
**Status**: PLANNING  

---

## Overview
Transform the Millionaire Game web-based audience participation system into a Progressive Web App (PWA) that behaves like a native mobile application. This will enable users to install the app on their home screen, work offline (with limitations), and provide a native app-like experience.

---

## Goals

### Primary Objectives
1. **Installability**: Enable "Add to Home Screen" on iOS, Android, and desktop
2. **App-Like Experience**: Remove browser UI, use app icons, splash screens
3. **Offline Support**: Cache static assets for faster loading and basic offline functionality
4. **Native Feel**: Smooth animations, touch interactions, app-like navigation
5. **Persistence**: App icon stays on home screen like a native app

### Non-Goals (Out of Scope)
- Full offline gameplay (game requires real-time server connection)
- Background sync for game state
- Push notifications (not needed for live audience participation)

---

## Technical Requirements

### 1. Web App Manifest (`manifest.json`)
**Priority**: HIGH | **Effort**: 1-2 hours

**File**: `src/MillionaireGame.Web/wwwroot/manifest.json`

**Required Properties**:
```json
{
  "name": "Millionaire Game Audience",
  "short_name": "Millionaire",
  "description": "Who Wants to be a Millionaire - Audience Participation System",
  "start_url": "/",
  "display": "standalone",
  "background_color": "#000033",
  "theme_color": "#FFD700",
  "orientation": "portrait-primary",
  "scope": "/",
  "icons": [
    {
      "src": "/icons/icon-72x72.png",
      "sizes": "72x72",
      "type": "image/png",
      "purpose": "any"
    },
    {
      "src": "/icons/icon-96x96.png",
      "sizes": "96x96",
      "type": "image/png",
      "purpose": "any"
    },
    {
      "src": "/icons/icon-128x128.png",
      "sizes": "128x128",
      "type": "image/png",
      "purpose": "any"
    },
    {
      "src": "/icons/icon-144x144.png",
      "sizes": "144x144",
      "type": "image/png",
      "purpose": "any"
    },
    {
      "src": "/icons/icon-152x152.png",
      "sizes": "152x152",
      "type": "image/png",
      "purpose": "any"
    },
    {
      "src": "/icons/icon-192x192.png",
      "sizes": "192x192",
      "type": "image/png",
      "purpose": "any maskable"
    },
    {
      "src": "/icons/icon-384x384.png",
      "sizes": "384x384",
      "type": "image/png",
      "purpose": "any"
    },
    {
      "src": "/icons/icon-512x512.png",
      "sizes": "512x512",
      "type": "image/png",
      "purpose": "any maskable"
    }
  ],
  "categories": ["entertainment", "games"],
  "lang": "en-US",
  "dir": "ltr"
}
```

**Key Properties Explained**:
- `display: standalone` - Removes browser UI (address bar, back button)
- `orientation: portrait-primary` - Locks to portrait mode (optimal for mobile game)
- `maskable` icons - Android adaptive icons (work with different shapes)
- `theme_color` - Gold color for Android status bar
- `background_color` - Navy blue for splash screen

**Icon Sizes Required**:
- **72x72**: Low-res Android devices
- **96x96**: Medium Android devices
- **128x128**: High-res Android devices
- **144x144**: iPad, high-DPI Android
- **152x152**: iOS home screen
- **192x192**: Standard Android (required minimum)
- **384x384**: High-DPI displays
- **512x512**: Android splash screen (required minimum)

---

### 2. Service Worker (`sw.js`)
**Priority**: HIGH | **Effort**: 3-4 hours

**File**: `src/MillionaireGame.Web/wwwroot/sw.js`

**Purpose**:
- Cache static assets (HTML, CSS, JS, images, fonts)
- Enable offline fallback page
- Improve load performance
- Required for PWA installability on Chrome/Android

**Caching Strategy**:

```javascript
// Cache versioning
const CACHE_NAME = 'millionaire-v1.0.5';
const RUNTIME_CACHE = 'millionaire-runtime';

// Static assets to cache (Cache First strategy)
const STATIC_ASSETS = [
  '/',
  '/index.html',
  '/css/app.css',
  '/js/app.js',
  '/logo.png',
  '/favicon.ico',
  '/icons/icon-192x192.png',
  '/icons/icon-512x512.png'
  // Add more as needed
];

// Install event - cache static assets
self.addEventListener('install', (event) => {
  event.waitUntil(
    caches.open(CACHE_NAME)
      .then(cache => cache.addAll(STATIC_ASSETS))
      .then(() => self.skipWaiting())
  );
});

// Activate event - clean up old caches
self.addEventListener('activate', (event) => {
  event.waitUntil(
    caches.keys().then(cacheNames => {
      return Promise.all(
        cacheNames.map(cacheName => {
          if (cacheName !== CACHE_NAME && cacheName !== RUNTIME_CACHE) {
            return caches.delete(cacheName);
          }
        })
      );
    }).then(() => self.clients.claim())
  );
});

// Fetch event - serve from cache, fallback to network
self.addEventListener('fetch', (event) => {
  // Skip non-GET requests
  if (event.request.method !== 'GET') return;
  
  // Skip SignalR and API requests (always go to network)
  if (event.request.url.includes('/hubs/') || 
      event.request.url.includes('/api/')) {
    return;
  }
  
  event.respondWith(
    caches.match(event.request)
      .then(cachedResponse => {
        if (cachedResponse) {
          return cachedResponse;
        }
        
        return fetch(event.request).then(response => {
          // Cache successful responses for runtime cache
          if (response && response.status === 200) {
            const responseToCache = response.clone();
            caches.open(RUNTIME_CACHE)
              .then(cache => cache.put(event.request, responseToCache));
          }
          return response;
        });
      })
  );
});
```

**Important Considerations**:
- **DO NOT** cache SignalR connections (`/hubs/*`)
- **DO NOT** cache API endpoints (`/api/*`)
- **DO** cache static assets (HTML, CSS, JS, images)
- **DO** update cache version when deploying new versions

---

### 3. Service Worker Registration
**Priority**: HIGH | **Effort**: 30 minutes

**File**: `src/MillionaireGame.Web/wwwroot/js/app.js`

**Add to initialization**:
```javascript
// Register Service Worker for PWA support
if ('serviceWorker' in navigator) {
  window.addEventListener('load', () => {
    navigator.serviceWorker.register('/sw.js')
      .then(registration => {
        console.log('✓ Service Worker registered:', registration.scope);
        
        // Check for updates periodically
        registration.addEventListener('updatefound', () => {
          const newWorker = registration.installing;
          newWorker.addEventListener('statechange', () => {
            if (newWorker.state === 'installed' && navigator.serviceWorker.controller) {
              console.log('New version available! Refresh to update.');
              // Optionally show update notification to user
            }
          });
        });
      })
      .catch(error => {
        console.error('Service Worker registration failed:', error);
      });
  });
}
```

---

### 4. App Icons
**Priority**: HIGH | **Effort**: 1-2 hours

**Directory**: `src/MillionaireGame.Web/wwwroot/icons/`

**Required Icon Sizes**:
- `icon-72x72.png`
- `icon-96x96.png`
- `icon-128x128.png`
- `icon-144x144.png`
- `icon-152x152.png`
- `icon-192x192.png` ✓ (REQUIRED for Chrome)
- `icon-384x384.png`
- `icon-512x512.png` ✓ (REQUIRED for Chrome)

**Design Requirements**:
- Use existing logo as base
- Gold (`#FFD700`) on navy blue (`#000033`) background
- Center logo with padding
- Export as PNG with transparency
- Consider "maskable" variants for Android (safe area)

**Maskable Icon Design**:
- Keep important content within 80% safe zone
- Android will crop/mask to various shapes (circle, squircle, rounded square)

**Apple Touch Icons** (for iOS):
```html
<!-- In index.html -->
<link rel="apple-touch-icon" sizes="180x180" href="/icons/apple-touch-icon.png">
```

---

### 5. HTML Meta Tag Updates
**Priority**: MEDIUM | **Effort**: 15 minutes

**File**: `src/MillionaireGame.Web/wwwroot/index.html`

**Add/Update**:
```html
<head>
  <!-- Existing meta tags -->
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0, viewport-fit=cover">
  
  <!-- PWA Manifest -->
  <link rel="manifest" href="/manifest.json">
  
  <!-- iOS Meta Tags (already have some, add missing) -->
  <meta name="apple-mobile-web-app-capable" content="yes">
  <meta name="apple-mobile-web-app-status-bar-style" content="black-translucent">
  <meta name="apple-mobile-web-app-title" content="Millionaire">
  <link rel="apple-touch-icon" href="/icons/apple-touch-icon.png">
  
  <!-- Android/Chrome -->
  <meta name="mobile-web-app-capable" content="yes">
  <meta name="theme-color" content="#FFD700">
  
  <!-- Windows Tile -->
  <meta name="msapplication-TileColor" content="#000033">
  <meta name="msapplication-TileImage" content="/icons/icon-144x144.png">
  
  <!-- Description for app stores -->
  <meta name="description" content="Who Wants to be a Millionaire - Live Audience Participation">
</head>
```

---

### 6. Splash Screens (iOS)
**Priority**: MEDIUM | **Effort**: 2-3 hours

**File**: `src/MillionaireGame.Web/wwwroot/index.html`

iOS doesn't use manifest splash screens, requires specific meta tags for each device resolution:

```html
<!-- iPhone X, XS, 11 Pro (1125x2436) -->
<link rel="apple-touch-startup-image" href="/splash/iphone-x.png" 
      media="(device-width: 375px) and (device-height: 812px) and (-webkit-device-pixel-ratio: 3)">

<!-- iPhone XR, 11 (828x1792) -->
<link rel="apple-touch-startup-image" href="/splash/iphone-xr.png" 
      media="(device-width: 414px) and (device-height: 896px) and (-webkit-device-pixel-ratio: 2)">

<!-- iPhone XS Max, 11 Pro Max (1242x2688) -->
<link rel="apple-touch-startup-image" href="/splash/iphone-xs-max.png" 
      media="(device-width: 414px) and (device-height: 896px) and (-webkit-device-pixel-ratio: 3)">

<!-- iPhone 8, 7, 6s, 6 (750x1334) -->
<link rel="apple-touch-startup-image" href="/splash/iphone-8.png" 
      media="(device-width: 375px) and (device-height: 667px) and (-webkit-device-pixel-ratio: 2)">

<!-- iPhone 8 Plus, 7 Plus, 6s Plus, 6 Plus (1242x2208) -->
<link rel="apple-touch-startup-image" href="/splash/iphone-8-plus.png" 
      media="(device-width: 414px) and (device-height: 736px) and (-webkit-device-pixel-ratio: 3)">

<!-- iPad Pro 12.9" (2048x2732) -->
<link rel="apple-touch-startup-image" href="/splash/ipad-pro-12.png" 
      media="(device-width: 1024px) and (device-height: 1366px) and (-webkit-device-pixel-ratio: 2)">
```

**Splash Screen Design**:
- Navy blue background (`#000033`)
- Centered logo
- "Millionaire Game" text below logo
- Match brand colors

**Android**: Uses `background_color` and icon from manifest automatically

---

### 7. Install Prompt Handling
**Priority**: LOW | **Effort**: 1 hour

**File**: `src/MillionaireGame.Web/wwwroot/js/app.js`

**Optional**: Show custom install prompt

```javascript
let deferredPrompt;

window.addEventListener('beforeinstallprompt', (e) => {
  // Prevent Chrome 67 and earlier from automatically showing the prompt
  e.preventDefault();
  deferredPrompt = e;
  
  console.log('PWA install prompt available');
  
  // Optionally show custom install button
  // showInstallButton();
});

window.addEventListener('appinstalled', () => {
  console.log('✓ PWA installed successfully');
  deferredPrompt = null;
});
```

---

### 8. Offline Fallback Page
**Priority**: LOW | **Effort**: 1 hour

**File**: `src/MillionaireGame.Web/wwwroot/offline.html`

**Purpose**: Show friendly message when offline and trying to access uncached content

```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Offline - Millionaire Game</title>
    <style>
        body {
            font-family: 'Segoe UI', Arial, sans-serif;
            background: linear-gradient(135deg, #000033, #001155);
            color: white;
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: 100vh;
            text-align: center;
            padding: 20px;
        }
        h1 { color: #FFD700; }
        .message { font-size: 1.2em; margin: 20px 0; }
    </style>
</head>
<body>
    <div>
        <h1>📡 No Connection</h1>
        <p class="message">
            The Millionaire Game requires an active connection to the game server.<br>
            Please check your network connection and try again.
        </p>
        <button onclick="location.reload()">Retry Connection</button>
    </div>
</body>
</html>
```

---

### 9. ASP.NET Core MIME Type Configuration
**Priority**: MEDIUM | **Effort**: 15 minutes

**File**: `src/MillionaireGame/Hosting/WebServerHost.cs`

**Ensure proper MIME types for manifest and service worker**:

```csharp
// In ConfigureApp method
var contentTypeProvider = new FileExtensionContentTypeProvider();
contentTypeProvider.Mappings[".webmanifest"] = "application/manifest+json";
contentTypeProvider.Mappings[".json"] = "application/json";
contentTypeProvider.Mappings[".js"] = "application/javascript";
```

---

## Implementation Phases

### Phase 1: Core PWA Setup (3-4 hours)
1. Create `manifest.json` with all required properties
2. Generate app icons (72x72 through 512x512)
3. Add manifest link to `index.html`
4. Update meta tags for iOS/Android
5. Test manifest validation (Chrome DevTools)

**Success Criteria**:
- ✓ Manifest passes validation in Chrome DevTools
- ✓ "Add to Home Screen" prompt appears on Chrome Android
- ✓ App icons display correctly

### Phase 2: Service Worker (3-4 hours)
1. Create `sw.js` with caching strategies
2. Add service worker registration to `app.js`
3. Test caching in Chrome DevTools (Application → Cache Storage)
4. Verify SignalR connections NOT cached
5. Test offline static asset loading

**Success Criteria**:
- ✓ Service worker registers successfully
- ✓ Static assets cached on first visit
- ✓ Page loads from cache when server stopped
- ✓ SignalR still connects (not cached)

### Phase 3: iOS Support (2-3 hours)
1. Create Apple touch icons
2. Generate splash screens for common iOS devices
3. Add iOS-specific meta tags
4. Test on iOS Safari

**Success Criteria**:
- ✓ "Add to Home Screen" works on iOS
- ✓ App opens without Safari UI
- ✓ Splash screen displays on iOS
- ✓ App icon looks correct on home screen

### Phase 4: Polish & Testing (2-3 hours)
1. Create offline fallback page
2. Add install prompt handling (optional)
3. Test on multiple devices (Android, iOS, desktop)
4. Verify PWA checklist (Lighthouse audit)
5. Update documentation

**Success Criteria**:
- ✓ Lighthouse PWA score > 90
- ✓ Installs correctly on Android, iOS, Chrome desktop
- ✓ App feels native (no browser UI)
- ✓ Smooth performance

---

## Testing Checklist

### Chrome Android
- [ ] Manifest detected by Chrome
- [ ] "Add to Home Screen" prompt appears
- [ ] App installs to home screen
- [ ] App opens in standalone mode (no address bar)
- [ ] App icon displays correctly
- [ ] Theme color applied to status bar
- [ ] Service worker caches assets
- [ ] Page loads offline from cache
- [ ] SignalR connects (requires network)

### Safari iOS
- [ ] "Add to Home Screen" available in Share menu
- [ ] App installs to home screen
- [ ] App opens without Safari UI
- [ ] Splash screen displays
- [ ] App icon displays correctly
- [ ] Status bar styled correctly
- [ ] Wake lock works
- [ ] Fullscreen/scroll-to-hide works

### Chrome Desktop
- [ ] Install icon appears in address bar
- [ ] App installs as desktop PWA
- [ ] App opens in standalone window
- [ ] App icon in taskbar/dock
- [ ] Window persists across sessions

### Edge Desktop
- [ ] Similar to Chrome desktop
- [ ] Pinning to Start menu (Windows)

---

## File Structure After Implementation

```
src/MillionaireGame.Web/wwwroot/
├── index.html (updated with PWA meta tags)
├── manifest.json (NEW)
├── sw.js (NEW - Service Worker)
├── offline.html (NEW - Offline fallback)
├── css/
│   └── app.css (existing)
├── js/
│   └── app.js (updated with SW registration)
├── icons/ (NEW directory)
│   ├── icon-72x72.png
│   ├── icon-96x96.png
│   ├── icon-128x128.png
│   ├── icon-144x144.png
│   ├── icon-152x152.png
│   ├── icon-192x192.png ✓ REQUIRED
│   ├── icon-384x384.png
│   ├── icon-512x512.png ✓ REQUIRED
│   └── apple-touch-icon.png (180x180)
└── splash/ (NEW directory - optional iOS)
    ├── iphone-x.png
    ├── iphone-xr.png
    ├── iphone-xs-max.png
    ├── iphone-8.png
    ├── iphone-8-plus.png
    └── ipad-pro-12.png
```

---

## Benefits

### User Experience
- **Native Feel**: App behaves like installed native app
- **Home Screen Icon**: Easy access, no browser needed
- **Faster Loading**: Cached assets load instantly
- **Offline Capability**: Basic UI loads without network (game requires connection)
- **Immersive**: No browser UI clutter

### Technical Benefits
- **No App Store**: Bypass Apple/Google app stores
- **Instant Updates**: No app store approval process
- **Cross-Platform**: Single codebase for all platforms
- **Web Standards**: Built on standard web technologies
- **SEO Friendly**: Still accessible via URL

### Business Benefits
- **Lower Barrier**: Users don't need to find/install from app store
- **Faster Distribution**: Share URL, instant installation
- **Analytics**: Track installations via service worker
- **Cost Effective**: No separate native development needed

---

## Risks & Mitigations

### Risk: Service Worker Caching Issues
**Impact**: MEDIUM  
**Mitigation**: 
- Use versioned cache names (`millionaire-v1.0.5`)
- Clear old caches on activation
- Exclude SignalR/API routes from caching
- Test cache updates on each deployment

### Risk: iOS Limitations
**Impact**: LOW  
**Mitigation**:
- iOS Safari has weaker PWA support than Android
- Splash screens require multiple images (tedious)
- Service worker support exists but limited
- Graceful degradation approach

### Risk: Browser Compatibility
**Impact**: LOW  
**Mitigation**:
- Feature detection (`if ('serviceWorker' in navigator)`)
- Graceful fallback to regular web app
- Progressive enhancement approach

### Risk: Cache Size Growth
**Impact**: LOW  
**Mitigation**:
- Limit runtime cache size
- Implement cache cleanup strategy
- Only cache essential assets

---

## Success Metrics

### Installation
- [ ] PWA installs on Android Chrome
- [ ] PWA installs on iOS Safari
- [ ] PWA installs on Chrome Desktop
- [ ] Lighthouse PWA score > 90

### Performance
- [ ] First load < 2 seconds
- [ ] Cached load < 500ms
- [ ] Service worker activates within 100ms

### User Experience
- [ ] No browser UI visible in standalone mode
- [ ] App feels responsive (no janky animations)
- [ ] Wake lock keeps screen on
- [ ] Fullscreen/hidden address bar works

---

## Documentation Updates

After implementation:
1. Update `README.md` with PWA installation instructions
2. Add "Install App" section to User Guide
3. Document service worker caching strategy
4. Create troubleshooting guide for PWA issues
5. Update CHANGELOG.md with PWA features

---

## Estimated Timeline

- **Phase 1 (Core PWA)**: 3-4 hours
- **Phase 2 (Service Worker)**: 3-4 hours
- **Phase 3 (iOS Support)**: 2-3 hours
- **Phase 4 (Polish & Testing)**: 2-3 hours

**Total**: 10-14 hours

---

## Next Steps

1. **Review Plan**: User approval/feedback on approach
2. **Icon Design**: Create/export app icons in all required sizes
3. **Implementation**: Follow phases in order
4. **Testing**: Test on real devices (Android, iOS)
5. **Deployment**: Update production with PWA features

---

## References

- [MDN: Progressive Web Apps](https://developer.mozilla.org/en-US/docs/Web/Progressive_web_apps)
- [Web App Manifest Spec](https://www.w3.org/TR/appmanifest/)
- [Service Worker API](https://developer.mozilla.org/en-US/docs/Web/API/Service_Worker_API)
- [PWA Checklist](https://web.dev/pwa-checklist/)
- [Lighthouse PWA Audits](https://web.dev/lighthouse-pwa/)
- [iOS PWA Support](https://webkit.org/blog/8042/service-workers-and-offline/)
