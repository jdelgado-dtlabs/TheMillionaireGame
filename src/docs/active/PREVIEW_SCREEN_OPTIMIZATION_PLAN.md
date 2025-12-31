# Preview Screen Performance Optimization Plan

**Created**: December 31, 2025  
**Priority**: MEDIUM (Optional for v1.0)  
**Estimated Time**: 2-3 hours  
**Target**: 30-50% CPU reduction when preview window is open

---

## 🔴 Problem Statement

### Current Performance Issue
The preview screen renders 3 full-resolution screens (Host, Guest, TV) simultaneously, causing significant performance overhead:

1. **Full Resolution Rendering**: Each screen renders at 1920x1080 on every paint
2. **Bitmap Creation**: Creates new bitmap for each screen every frame
3. **High-Quality Scaling**: Uses HighQualityBicubic interpolation to scale down
4. **No Caching**: Re-renders entire screen even when nothing changes
5. **Combined Load**: 3 screens × full rendering = 3× performance cost

### Performance Impact
- CPU usage spikes when preview window is open
- Noticeable lag/stuttering during animations
- Confetti disabled on preview (IsPreview flag helps but not enough)
- Preview panel becomes sluggish with 3 active screens

### Current Code Location
**File**: `src/MillionaireGame/Forms/PreviewScreenForm.cs`  
**Method**: `PreviewPanel_Paint()` (lines 330-412)

```csharp
private void PreviewPanel_Paint(object? sender, PaintEventArgs e)
{
    if (_screen == null) return;

    // Get the screen's actual client size for proper aspect ratio
    var screenForm = _screen as Form;
    int designWidth = 1920;
    int designHeight = 1080;
    
    // Create a bitmap to render the screen at its design resolution
    using (var bitmap = new Bitmap(designWidth, designHeight))  // ⚠️ EXPENSIVE
    using (var g = System.Drawing.Graphics.FromImage(bitmap))
    {
        g.Clear(Color.Black);
        
        // Set up high-quality rendering
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;  // ⚠️ EXPENSIVE
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

        // Call the screen's protected RenderScreen method via reflection
        var renderMethod = _screen.GetType().GetMethod("RenderScreen", 
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        renderMethod?.Invoke(_screen, new object[] { g });  // ⚠️ FULL RENDER

        // Scale and draw the bitmap to fit the panel
        e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        e.Graphics.DrawImage(bitmap, x, y, scaledWidth, scaledHeight);  // ⚠️ EXPENSIVE SCALING
    }
}
```

---

## 🎯 Optimization Strategies

### Strategy 1: Cached Rendering (RECOMMENDED) ⭐
**Impact**: HIGH | **Complexity**: MEDIUM | **Risk**: LOW

**Approach**: Only re-render when screen state actually changes

**Implementation**:
```csharp
private Bitmap? _cachedBitmap = null;
private bool _needsRedraw = true;

private void PreviewPanel_Paint(object? sender, PaintEventArgs e)
{
    if (_screen == null) return;

    // Only re-render if state changed
    if (_needsRedraw || _cachedBitmap == null)
    {
        _cachedBitmap?.Dispose();
        _cachedBitmap = RenderScreenToBitmap();
        _needsRedraw = false;
    }
    
    // Just draw the cached bitmap
    e.Graphics.DrawImage(_cachedBitmap, x, y, scaledWidth, scaledHeight);
}

public void InvalidateCache()
{
    _needsRedraw = true;
    Invalidate();
}
```

**Requirements**:
- Add `InvalidateCache()` to PreviewPanel class
- Call `InvalidateCache()` when screen state changes (question updates, answer selection, etc.)
- Subscribe to state change events from ScreenUpdateService
- Properly dispose cached bitmaps on panel disposal

**Pros**:
- Massive performance gain (render only when needed)
- No quality loss
- Maintains full 1920x1080 rendering

**Cons**:
- Need to track when to invalidate cache
- More complex state management
- Memory overhead for cached bitmaps (3 × 1920×1080 × 4 bytes ≈ 24MB)

---

### Strategy 2: Lower Resolution Rendering
**Impact**: HIGH | **Complexity**: LOW | **Risk**: MEDIUM

**Approach**: Render directly at preview panel size instead of 1920x1080

**Implementation**:
```csharp
private void PreviewPanel_Paint(object? sender, PaintEventArgs e)
{
    if (_screen == null) return;

    // Render at preview size directly (e.g., 640x360)
    int previewWidth = Width;
    int previewHeight = (int)(Width * 9f / 16f);
    
    using (var bitmap = new Bitmap(previewWidth, previewHeight))
    using (var g = System.Drawing.Graphics.FromImage(bitmap))
    {
        // Scale rendering to preview size
        float scale = previewWidth / 1920f;
        g.ScaleTransform(scale, scale);
        
        // Render at lower resolution
        var renderMethod = _screen.GetType().GetMethod("RenderScreen", 
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        renderMethod?.Invoke(_screen, new object[] { g });
        
        // No scaling needed - bitmap is already preview size
        e.Graphics.DrawImage(bitmap, 0, 0);
    }
}
```

**Pros**:
- Much smaller bitmap (640×360 vs 1920×1080 = 10× smaller)
- No expensive scaling step
- Simple implementation

**Cons**:
- Lower visual quality in preview
- ScaleX/ScaleY in screen rendering may not work correctly with pre-scaled graphics
- May need to adjust text sizes and rendering

---

### Strategy 3: Reduced Quality Scaling
**Impact**: MEDIUM | **Complexity**: LOW | **Risk**: LOW

**Approach**: Use faster interpolation modes

**Implementation**:
```csharp
// Change from HighQualityBicubic to Bilinear or NearestNeighbor
g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear;
e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear;
```

**Pros**:
- Minimal code change
- Immediate performance gain (~20-30%)
- No structural changes

**Cons**:
- Slightly lower visual quality
- Still rendering at full resolution
- Not as effective as other strategies

---

### Strategy 4: Throttled Refresh Rate
**Impact**: MEDIUM | **Complexity**: LOW | **Risk**: LOW

**Approach**: Limit preview panel invalidation to 10-15 FPS

**Implementation**:
```csharp
private DateTime _lastRedraw = DateTime.MinValue;
private const int MinRefreshIntervalMs = 67; // ~15 FPS

private void PreviewPanel_Paint(object? sender, PaintEventArgs e)
{
    var now = DateTime.Now;
    if ((now - _lastRedraw).TotalMilliseconds < MinRefreshIntervalMs)
    {
        // Skip this frame
        return;
    }
    _lastRedraw = now;
    
    // Render normally...
}
```

**Pros**:
- Simple to implement
- No quality loss at reasonable frame rates
- Works well with other strategies

**Cons**:
- Preview may appear less responsive
- Doesn't reduce rendering cost, just frequency
- Still expensive when it does render

---

## 📋 Recommended Implementation Plan

### Phase 1: Cached Rendering (Primary Optimization)
**Goal**: Eliminate redundant rendering when screen state hasn't changed

**Tasks**:
1. Add cached bitmap fields to PreviewPanel class
2. Implement InvalidateCache() method
3. Subscribe to ScreenUpdateService events to detect state changes
4. Update PreviewPanel_Paint() to use cached rendering
5. Add proper disposal of cached bitmaps
6. Test with all 3 screens active

**State Change Events to Monitor**:
- QuestionUpdated
- AnswerSelected
- AnswerRevealed
- ATAResultsUpdated
- LifelineActivated
- ScreenReset

**Code Changes**:
- `PreviewPanel` class in PreviewScreenForm.cs
- Subscribe to IGameScreen events or ScreenUpdateService broadcasts
- Add `_cachedBitmaps` Dictionary<string, Bitmap> for 3 screens

---

### Phase 2: Combine with Throttling (Secondary Optimization)
**Goal**: Further reduce CPU usage during animations

**Tasks**:
1. Add frame timing to PreviewPanel_Paint()
2. Set minimum interval to 67ms (15 FPS)
3. Skip frames when called too frequently
4. Test with confetti disabled (already done via IsPreview)

**Expected Result**:
- Preview updates at most 15 times per second
- Smooth enough for UI updates
- Significant CPU reduction during animations

---

### Phase 3: Optional Quality Reduction (Fallback)
**Goal**: If caching + throttling isn't enough, reduce scaling quality

**Tasks**:
1. Change InterpolationMode from HighQualityBicubic to Bilinear
2. Test visual quality difference
3. Make configurable in settings if needed

---

## 🧪 Testing Plan

### Performance Metrics
**Before Optimization** (Baseline):
- [ ] Measure CPU usage with preview window open (3 screens)
- [ ] Measure frame rate during confetti animation
- [ ] Measure memory usage
- [ ] Note any visible lag or stuttering

**After Phase 1** (Cached Rendering):
- [ ] Verify CPU usage reduced by 40-60%
- [ ] Confirm preview updates correctly on state changes
- [ ] Check memory usage (expect +24MB for cached bitmaps)
- [ ] Test all game scenarios (questions, answers, lifelines, FFF, ATA)

**After Phase 2** (Throttling):
- [ ] Verify additional 10-20% CPU reduction
- [ ] Confirm 15 FPS is smooth enough for preview
- [ ] Test during confetti animation (should be disabled anyway)

**After Phase 3** (Quality Reduction) - If needed:
- [ ] Compare visual quality Bicubic vs Bilinear
- [ ] Verify acceptable quality for preview use

### Test Scenarios
1. **Static Screen**: Preview with question displayed, no animations
2. **Question Flow**: Load question → Show answers → Select → Reveal
3. **Lifelines**: Activate 50:50, PAF, ATA in preview
4. **FFF Online**: Run FFF with preview open
5. **Winner Screen**: Q15 win with confetti (should be disabled in preview)
6. **Multiple Resets**: Reset game multiple times, check for memory leaks
7. **Extended Use**: Leave preview open for 30+ minutes during gameplay

---

## 📊 Success Criteria

### Performance Targets
- ✅ CPU usage reduced by 30-50% with preview open
- ✅ No visible lag or stuttering in preview window
- ✅ Preview updates correctly on all state changes
- ✅ Memory usage increase < 30MB (acceptable for caching)
- ✅ Frame rate stable at 15 FPS minimum

### Quality Requirements
- ✅ Preview maintains acceptable visual quality
- ✅ All 3 screens visible and readable
- ✅ Text remains legible at preview size
- ✅ Animations appear smooth (if not disabled)

### Stability
- ✅ No crashes or exceptions
- ✅ No memory leaks during extended use
- ✅ Proper cleanup on preview window close
- ✅ No impact on primary screen performance

---

## 🔧 Implementation Notes

### Key Files to Modify
1. **PreviewScreenForm.cs** (Primary)
   - PreviewPanel class (lines 330-412)
   - Add caching infrastructure
   - Subscribe to state change events

2. **ScreenUpdateService.cs** (Secondary)
   - May need to add events if not already present
   - Ensure all state changes are broadcast

3. **ScalableScreenBase.cs** (Reference)
   - Understand scaling mechanism
   - Ensure RenderScreen() is compatible with caching

### Potential Challenges
1. **Cache Invalidation**: Determining when to invalidate cache
2. **Event Subscription**: Ensuring all state changes trigger cache invalidation
3. **Memory Management**: Proper disposal of cached bitmaps
4. **Thread Safety**: Preview may render on different thread than state updates

### Rollback Plan
If optimization causes issues:
1. Keep original PreviewPanel_Paint() as `PreviewPanel_Paint_Original()`
2. Add toggle flag `_useOptimizedRendering`
3. Can quickly switch back if problems arise

---

## 📝 Documentation Updates

After implementation:
1. Update PRE_1.0_FINAL_CHECKLIST.md (mark complete)
2. Add entry to CHANGELOG.md
3. Update START_HERE.md
4. Document performance improvements with before/after metrics
5. Add code comments explaining caching mechanism

---

## 🎯 Estimated Timeline

- **Phase 1 (Cached Rendering)**: 1.5-2 hours
  - 30 min: Code implementation
  - 30 min: Event subscription and integration
  - 30-60 min: Testing and debugging

- **Phase 2 (Throttling)**: 30 minutes
  - 15 min: Code implementation
  - 15 min: Testing

- **Phase 3 (Quality)**: 15 minutes (if needed)
  - 5 min: Code change
  - 10 min: Testing

**Total**: 2-3 hours

---

## ✅ Completion Checklist

- [ ] Phase 1: Cached rendering implemented
- [ ] Phase 2: Throttling added
- [ ] Phase 3: Quality reduction (if needed)
- [ ] Performance metrics documented
- [ ] All test scenarios passed
- [ ] Code reviewed and cleaned up
- [ ] Documentation updated
- [ ] Committed and pushed to feature branch
- [ ] Merged to master-csharp
- [ ] PRE_1.0_FINAL_CHECKLIST.md updated
