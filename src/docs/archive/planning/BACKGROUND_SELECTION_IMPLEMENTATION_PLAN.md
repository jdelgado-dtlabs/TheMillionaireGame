# Background Selection Feature - Implementation Plan

**Date:** December 31, 2025  
**Branch:** `feature/background-graphics`  
**Target:** v1.0 Release  
**Status:** 🔄 IN PROGRESS

---

## 📋 Overview

Add a **Background** tab to the Broadcast settings that allows users to choose between:
1. **Prerendered backgrounds** from the existing theme folders
2. **Chroma key** with customizable solid colors for green screen compositing

This enables streamers and broadcasters to:
- Use thematic game backgrounds for standalone broadcasts
- Key out the background for OBS/vMix/Wirecast overlay workflows
- Customize chroma key colors to avoid conflicts with game UI elements

---

## 🎯 Feature Requirements

### 1. Background Tab in Broadcast Settings

**Location:** Control Panel → Broadcast Tab → New "Background" Sub-tab

**UI Layout:**
```
┌─────────────────────────────────────────────────┐
│ Background Selection                            │
├─────────────────────────────────────────────────┤
│                                                 │
│ ○ Prerendered Background                       │
│   [Dropdown: Select Theme Background     ▼]    │
│   Preview: [200x150 thumbnail]                 │
│                                                 │
│ ○ Chroma Key (Solid Color)                     │
│   Recommended: [Blue #0000FF] [Green #00FF00]  │
│   Custom:      [Color Picker...] [#______]     │
│   ⚠️ Warning: Avoid colors used in game UI     │
│                                                 │
│         [Apply]  [Reset to Default]            │
└─────────────────────────────────────────────────┘
```

### 2. Prerendered Background Options

**Source:** Load from existing theme folders in `lib/textures/themes/`

**Available Themes:**
- Classic Theme: `res/NG_16000000_BG.png`, `res/NG_500000_BG.png`, etc.
- International Themes: `res/NG_16000000_BG.png` (different variations)
- Custom user themes if added

**Implementation:**
- Scan theme folders for background images
- Display theme name + level tier (e.g., "Classic - $1 Million")
- Show preview thumbnail (scaled to 200x150)
- Apply selected background to TV screen

### 3. Chroma Key Color Selection

**Recommended Colors:**
| Color | Hex Code | Use Case | Game Conflict? |
|-------|----------|----------|----------------|
| **Blue** | `#0000FF` | Most common chroma key | ✅ Safe - minimal blue in UI |
| **Magenta** | `#FF00FF` | Alternative to green | ✅ Safe - not used in game |
| **Cyan** | `#00FFFF` | Less common but effective | ⚠️ Used in some answer states |
| Green | `#00FF00` | Traditional chroma | ❌ AVOID - used in correct answers |
| Red | `#FF0000` | Rarely used | ❌ AVOID - used in wrong answers |
| Orange | `#FF8000` | Warm alternative | ⚠️ Used in money tree |
| Yellow | `#FFFF00` | High visibility | ❌ AVOID - used in money values |

**Default Recommendation:** **Blue (#0000FF)** - safest choice with no game UI conflicts

**Custom Color Picker:**
- Use Windows `ColorDialog` for selection
- Display hex code input field for precise values
- Show warning if color matches game UI elements (green, red, yellow, orange)
- Allow override with confirmation

### 4. Settings Storage

**Add to AppSettings.xml:**
```xml
<BroadcastSettings>
  <BackgroundMode>Prerendered</BackgroundMode> <!-- or "ChromaKey" -->
  <SelectedBackground>Classic/res/NG_16000000_BG.png</SelectedBackground>
  <ChromaKeyColor>#0000FF</ChromaKeyColor>
</BroadcastSettings>
```

**Default Values:**
- Mode: `Prerendered`
- Background: Current theme's default background
- ChromaKey Color: `#0000FF` (Blue)

---

## 🏗️ Implementation Phases

### Phase 1: Settings Infrastructure (30 min)
**Files:**
- `MillionaireGame.Core/Settings/AppSettings.cs`
- `MillionaireGame.Core/Settings/BroadcastSettings.cs` (new)

**Tasks:**
1. Create `BroadcastSettings` class with background properties
2. Add load/save methods to `SettingsManager`
3. Add validation for color codes
4. Add helper to scan theme folders for backgrounds

### Phase 2: UI Implementation (45 min)
**Files:**
- `MillionaireGame/Forms/ControlPanelForm.cs`
- `MillionaireGame/Forms/ControlPanelForm.Designer.cs`

**Tasks:**
1. Add "Background" tab to Broadcast tab control
2. Add radio buttons for mode selection
3. Add dropdown for prerendered background selection
4. Add color picker button + hex input field
5. Add recommended color quick-select buttons
6. Add preview thumbnail display
7. Wire up event handlers

### Phase 3: Background Rendering (30 min)
**Files:**
- `MillionaireGame/Forms/TVScreenFormScalable.cs`
- `MillionaireGame/Graphics/BackgroundRenderer.cs` (new)

**Tasks:**
1. Create `BackgroundRenderer` class to handle background drawing
2. Modify `TVScreenFormScalable.OnPaint()` to use selected background
3. Support both image backgrounds and solid color fill
4. Handle scaling for different resolutions
5. Ensure proper layering (background → game elements → overlay)

### Phase 4: Theme Integration (20 min)
**Files:**
- `MillionaireGame/Helpers/ThemeManager.cs`

**Tasks:**
1. Add method to list available backgrounds per theme
2. Cache background images for performance
3. Handle theme changes (update background dropdown)
4. Validate background file paths

### Phase 5: Testing & Polish (15 min)
**Tasks:**
1. Test background changes during active game
2. Verify chroma key colors render correctly
3. Test OBS/vMix capture with chroma key
4. Ensure performance is not impacted
5. Add tooltips and help text

---

## 🎨 Technical Details

### Color Conflict Detection

**Game UI Colors to Check Against:**
```csharp
public static class GameColors
{
    public static Color CorrectAnswer = Color.FromArgb(0, 255, 0);      // Green
    public static Color WrongAnswer = Color.FromArgb(255, 0, 0);        // Red
    public static Color MoneyValue = Color.FromArgb(255, 255, 0);       // Yellow
    public static Color FiftyFifty = Color.FromArgb(255, 165, 0);       // Orange
    public static Color PhoneAFriend = Color.FromArgb(0, 255, 255);     // Cyan
    public static Color AskTheAudience = Color.FromArgb(128, 0, 128);   // Purple
}

public static bool IsColorConflict(Color chromaKey)
{
    const int threshold = 30; // RGB difference threshold
    
    foreach (var gameColor in new[] { CorrectAnswer, WrongAnswer, ... })
    {
        if (ColorDistance(chromaKey, gameColor) < threshold)
            return true;
    }
    return false;
}
```

### Background Rendering Priority

**Layering Order (bottom to top):**
1. **Background** (prerendered image OR solid chroma color)
2. **Money Tree** (if visible)
3. **Question Display**
4. **Answer Options** (A/B/C/D)
5. **Lifeline Indicators**
6. **Timer/Clock**
7. **Overlays** (dimming, transitions)

### Performance Considerations

- **Cache loaded background images** - don't reload on every paint
- **Use `Graphics.Clear()` for solid colors** - faster than drawing rectangles
- **Scale backgrounds once** when resolution changes, not every frame
- **Dispose old background** when changing to prevent memory leaks

---

## 🚀 Future Development Items

### Alpha Channel / Transparent Background Support
**Status:** 🔮 FUTURE CONSIDERATION  
**Priority:** Medium  
**Complexity:** High

**Description:**
Instead of chroma keying, render the TV screen with a **transparent background** that OBS/streaming software can directly composite over other sources.

**Technical Approach:**
```csharp
// Option 1: Render to transparent PNG
Bitmap frame = new Bitmap(1920, 1080, PixelFormat.Format32bppArgb);
Graphics g = Graphics.FromImage(frame);
g.Clear(Color.Transparent); // Alpha = 0
// Draw game elements...

// Option 2: Use WPF/DirectX for true alpha channel output
// - Convert WinForms to WPF (major refactor)
// - Use DirectX surface sharing
// - Requires OBS NDI/Spout/Syphon plugin
```

**Challenges:**
- **WinForms limitation:** Forms cannot be truly transparent with game rendering
- **Window capture:** OBS captures window chrome (borders, title bar)
- **Requires plugin:** NDI, Spout (Windows), or Syphon (Mac) for alpha channel
- **Performance:** Additional rendering overhead

**Alternative Solutions:**
1. **NDI Output Plugin** - Stream frame buffer with alpha to OBS
2. **Virtual Camera** - Use DirectShow filter for transparency
3. **Spout/Syphon** - GPU texture sharing with OBS

**Research Needed:**
- ✅ Chroma keying is the industry standard for Windows game overlays
- ⚠️ True alpha requires plugin ecosystem (NDI most common)
- ⚠️ May require separate rendering thread for 60fps output
- 🔍 Investigate `obs-websocket` for direct frame injection

**Recommendation:**
Start with chroma key implementation. Add alpha channel support in v1.1+ if users request it and technical feasibility is confirmed through research/prototyping.

---

## ✅ Success Criteria

- [ ] User can select any prerendered background from current theme
- [ ] User can enable chroma key mode with recommended colors
- [ ] User can select custom chroma key color via color picker
- [ ] Warning shown when selecting colors that conflict with game UI
- [ ] Background changes apply immediately (or on next question)
- [ ] Settings persist across application restarts
- [ ] Chroma key works cleanly with OBS/vMix/Wirecast
- [ ] No performance degradation during gameplay
- [ ] Default background behavior unchanged for existing users

---

## 📝 Testing Plan

### Manual Testing
1. **Prerendered Backgrounds:**
   - Select each available theme background
   - Verify correct image loads and scales
   - Check that game elements render on top

2. **Chroma Key:**
   - Test each recommended color (Blue, Magenta, Cyan)
   - Verify solid color fills entire background
   - Test in OBS with chroma key filter
   - Ensure no color spill on game elements

3. **Color Conflicts:**
   - Try selecting Green → verify warning appears
   - Try selecting Red → verify warning appears
   - Verify user can override warning

4. **Settings Persistence:**
   - Change background, restart app → verify setting saved
   - Switch modes → verify correct mode loads on restart

5. **Performance:**
   - Monitor FPS during background changes
   - Test with complex backgrounds vs solid colors
   - Verify no memory leaks when switching repeatedly

### OBS Integration Testing
1. Add TV screen window as source in OBS
2. Apply Chroma Key filter with matching color
3. Verify clean key with no edge artifacts
4. Test with various lighting conditions
5. Verify game elements not affected by key

---

## 📦 Deliverables

1. **Code:**
   - `BroadcastSettings.cs` - Settings model
   - `BackgroundRenderer.cs` - Rendering logic
   - Control Panel UI updates
   - TV Screen rendering updates

2. **Documentation:**
   - User guide for background selection
   - OBS setup guide for chroma keying
   - Technical notes on alpha channel future work

3. **Testing:**
   - Test results document
   - OBS integration screenshots/video

---

## 🔗 Related Files

**Code:**
- `MillionaireGame/Forms/ControlPanelForm.cs`
- `MillionaireGame/Forms/TVScreenFormScalable.cs`
- `MillionaireGame.Core/Settings/AppSettings.cs`
- `MillionaireGame.Core/Graphics/ThemeManager.cs`

**Assets:**
- `lib/textures/themes/*/res/*.png` - Background images

**Documentation:**
- This plan document
- Future: User guide in docs/guides/
