# Session Document: Phase 7 - Strap Integration Plan

**Date:** January 15, 2026  
**Phase:** 7 (Strap System Integration)  
**Status:** Planning  
**Branch:** `feature/theming-system`

## Context

User testing revealed that while the Classic Gold theme is selected in settings, the game screens still display the original PNG-based straps. This is expected behavior - **Phase 5 only integrated background theming**, not straps.

### Current State

**✅ What Works:**
- Theme database with 6 preset themes
- ThemeService with CRUD operations
- SvgStrapRenderer fully functional
- Background theming applied to TV screen
- Theme settings UI with preview

**❌ What Doesn't Work:**
- Question straps still use PNG textures (`TextureManager`)
- Answer straps still use PNG textures
- Money tree colors aren't themed
- Typography/fonts aren't themed

### Technical Analysis

**Strap Rendering Locations:**
1. **TVScreenForm.cs** (Line 229): `DrawQuestionStrap()`
2. **HostScreenForm.cs** (Line ~199): `DrawQuestionStrap()`
3. **GuestScreenForm.cs** (Line 177): `DrawQuestionStrap()`
4. **TVScreenForm.cs** (Line 269): `DrawAnswerBox()` (for answer straps)
5. **HostScreenForm.cs**: Similar answer box rendering
6. **GuestScreenForm.cs**: Similar answer box rendering

**Current Implementation:**
```csharp
private void DrawQuestionStrap(System.Drawing.Graphics g)
{
    var texture = TextureManager.GetTexture(TextureManager.ElementType.QuestionStrap, CurrentTextureSet);
    
    if (texture != null)
    {
        DrawScaledImage(g, texture, 
            _questionStrapBounds.X, _questionStrapBounds.Y, 
            _questionStrapBounds.Width, _questionStrapBounds.Height);
    }
    
    // Draw text...
}
```

**Target Implementation:**
```csharp
private void DrawQuestionStrap(System.Drawing.Graphics g)
{
    // Check if theme is active
    if (_activeTheme != null && _svgStrapRenderer != null)
    {
        // Get question strap from theme
        var questionStrap = _activeTheme.Straps.FirstOrDefault(s => s.StrapType == "Question");
        if (questionStrap != null)
        {
            // Render with SVG
            var bounds = new Rectangle(
                (int)_questionStrapBounds.X, 
                (int)_questionStrapBounds.Y,
                (int)_questionStrapBounds.Width,
                (int)_questionStrapBounds.Height);
            
            _svgStrapRenderer.RenderStrapToGraphics(g, questionStrap, 
                _currentQuestion?.QuestionText ?? "", bounds);
            return; // Skip legacy rendering
        }
    }
    
    // Fallback to legacy PNG textures
    var texture = TextureManager.GetTexture(TextureManager.ElementType.QuestionStrap, CurrentTextureSet);
    if (texture != null)
    {
        DrawScaledImage(g, texture, 
            _questionStrapBounds.X, _questionStrapBounds.Y, 
            _questionStrapBounds.Width, _questionStrapBounds.Height);
            
        // Draw text with legacy method...
    }
}
```

## Implementation Plan

### Phase 7.1: Screen Form Theme Integration (High Priority)

**Files to Modify:**
1. `TVScreenForm.cs`
2. `HostScreenForm.cs` 
3. `GuestScreenForm.cs`

**Changes Needed:**
- Add `private CompleteTheme? _activeTheme` field
- Add `private SvgStrapRenderer? _svgStrapRenderer` field
- Load theme in `Initialize()` method (already exists for TVScreenForm)
- Modify `DrawQuestionStrap()` to check for active theme
- Modify `DrawAnswerBox()` to check for active theme
- Implement `RefreshTheme()` method (already exists for TVScreenForm)

**Strap Type Mapping:**
- `"Question"` → Question strap at top
- `"Answer"` → Answer straps (A, B, C, D)
- Different states: Normal, Final (selected), Correct (revealed)

**Complexity:** Medium (3 screen forms to update, careful state management)

### Phase 7.2: Money Tree Theming (Medium Priority)

**Current:** Money tree uses hardcoded PNG overlays and colors  
**Target:** Apply `ThemeMoneyTree` colors to money ladder display

**Files to Modify:**
1. Money tree rendering code (need to locate)
2. Money ladder color logic

**Complexity:** Medium (need to find money tree rendering logic)

### Phase 7.3: Typography/Font Theming (Low Priority)

**Current:** Fonts are hardcoded in strap drawing methods  
**Target:** Use `ThemeStrap.FontFamily`, `FontSize`, `FontColor` from theme

**Note:** SvgStrapRenderer already handles fonts internally when rendering. This is mostly complete.

### Phase 7.4: Testing & Polish

- Test all 6 preset themes with live game
- Verify strap animations work
- Test fallback to legacy PNG straps
- Performance testing (SVG rendering vs PNG blit)
- Memory usage validation

## Risk Assessment

**⚠️ Breaking Changes:**
- Modifying strap rendering could break existing game flow
- Need to maintain backward compatibility with PNG straps
- Different screen forms may have slightly different rendering patterns

**🔄 Rollback Strategy:**
- Keep legacy TextureManager path fully functional
- Theme system is opt-in (if no theme active, use legacy)
- Can disable theme integration with feature flag if needed

**🐛 Potential Issues:**
- Performance: SVG rendering may be slower than PNG blitting
- Text layout: SVG text rendering vs. legacy text drawing
- Animation timing: Need to coordinate with existing animations
- Memory: Multiple SvgStrapRenderer instances across screens

## Estimated Effort

- **Phase 7.1** (Screen Integration): 2-3 hours
- **Phase 7.2** (Money Tree): 1-2 hours
- **Phase 7.3** (Typography): 30 minutes (mostly verification)
- **Phase 7.4** (Testing): 1-2 hours

**Total:** ~5-8 hours of focused development

## Decision Point

**User has two options:**

### Option A: Proceed with Phase 7 Now
- Full strap integration across all screens
- Themed game experience end-to-end
- Requires 5-8 hours commitment
- Testing in live game environment

### Option B: Defer Phase 7, Focus on Theme Design
- Current theme system fully functional for backgrounds
- User can design/refine theme aesthetics in OptionsDialog
- Strap integration can be done later
- Less technical risk during theme design phase

**Recommendation:** Given user wants to work on theme designs first, **Option B** is more pragmatic. The theme infrastructure is complete, just not fully integrated into gameplay yet. This allows user to:
1. Design better looking themes using the preview system
2. Test theme switching without gameplay interruptions
3. Tackle strap integration when themes are finalized

## Next Steps (If Proceeding with Phase 7)

1. Create Phase 7 task list in TODO system
2. Start with TVScreenForm strap integration (highest visibility)
3. Test with Classic Gold theme
4. Extend to HostScreenForm and GuestScreenForm
5. Add money tree color theming
6. Comprehensive testing across all 6 presets

## Next Steps (If Deferring Phase 7)

1. Document current limitation in user guide
2. Focus on theme design/refinement in OptionsDialog
3. User can modify preset themes or create new ones
4. Phase 7 integration when themes are finalized

---

**Current Status:** Awaiting user decision on how to proceed.
