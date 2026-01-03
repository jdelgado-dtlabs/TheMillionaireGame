# Lifeline Hotkey Mapping Implementation Plan

**Date**: December 31, 2025  
**Branch**: `feature/lifeline-hotkeys`  
**Priority**: Medium  
**Estimated Time**: 30 minutes  
**Status**: ✅ COMPLETE

---

## 📋 Overview

Implement keyboard shortcuts (F8-F11) for direct lifeline activation, enabling complete keyboard control of the game without mouse interaction. This improves live production workflow and enables future Stream Deck integration.

---

## 🎯 Current Hotkey Map

### ✅ Already Implemented
```
┌─────────────────────────────────────────────────────┐
│ GAME FLOW CONTROLS                                  │
├─────────────────────────────────────────────────────┤
│ F1          → Select Answer A                       │
│ F2          → Select Answer B                       │
│ F3          → Select Answer C                       │
│ F4          → Select Answer D                       │
│ F5          → Load New Question                     │
│ F6          → Reveal/Lock Answer                    │
│ F7          → Lights Down                           │
│ Page Up     → Level Up (manual control)             │
│ Page Down   → Level Down (manual control)           │
│ End         → Walk Away                             │
│ R           → Toggle Risk Mode                      │
└─────────────────────────────────────────────────────┘
```

### ⚠️ Planned (Ctrl+1-4) - Currently Disabled
```
┌─────────────────────────────────────────────────────┐
│ DEFERRED TO v1.2 (See TODO comments in code)       │
├─────────────────────────────────────────────────────┤
│ Ctrl+1-4    → Reserved for Stream Deck integration  │
└─────────────────────────────────────────────────────┘
```

---

## 🚀 Proposed Hotkey Map

### ✨ NEW: Lifeline Controls (F8-F11)
```
┌─────────────────────────────────────────────────────┐
│ LIFELINE CONTROLS                                   │
├─────────────────────────────────────────────────────┤
│ F8          → Activate Lifeline 1                   │
│ F9          → Activate Lifeline 2                   │
│ F10         → Activate Lifeline 3                   │
│ F11         → Activate Lifeline 4                   │
└─────────────────────────────────────────────────────┘
```

**Lifeline Button Mapping**:
- **btnLifeline1** (F8) → Configurable (default: 50:50)
- **btnLifeline2** (F9) → Configurable (default: Phone a Friend)
- **btnLifeline3** (F10) → Configurable (default: Ask the Audience)
- **btnLifeline4** (F11) → Configurable (default: Switch the Question)

**Note**: Lifeline order is configurable in Settings → Lifelines tab. The hotkeys map to button positions, not specific lifelines.

---

## 🏗️ Implementation Plan

### Phase 1: Update HotkeyHandler Class (10 minutes)

**File**: `src/MillionaireGame/Services/HotkeyHandler.cs`

**Changes**:
1. Add private readonly Action fields:
   ```csharp
   private readonly Action? _onF8;  // Lifeline 1
   private readonly Action? _onF9;  // Lifeline 2
   private readonly Action? _onF10; // Lifeline 3
   private readonly Action? _onF11; // Lifeline 4
   ```

2. Add constructor parameters:
   ```csharp
   Action? onF8 = null,
   Action? onF9 = null,
   Action? onF10 = null,
   Action? onF11 = null
   ```

3. Add switch cases in `ProcessKeyPress()`:
   ```csharp
   case Keys.F8:
       _onF8?.Invoke();
       break;
   case Keys.F9:
       _onF9?.Invoke();
       break;
   case Keys.F10:
       _onF10?.Invoke();
       break;
   case Keys.F11:
       _onF11?.Invoke();
       break;
   ```

4. Update XML documentation comments

### Phase 2: Update ControlPanelForm Constructor (5 minutes)

**File**: `src/MillionaireGame/Forms/ControlPanelForm.cs`

**Changes**:
1. Add lifeline hotkey handlers to HotkeyHandler initialization:
   ```csharp
   _hotkeyHandler = new HotkeyHandler(
       onF1: () => btnA.PerformClick(),
       onF2: () => btnB.PerformClick(),
       onF3: () => btnC.PerformClick(),
       onF4: () => btnD.PerformClick(),
       onF5: () => btnNewQuestion.PerformClick(),
       onF6: () => btnReveal.PerformClick(),
       onF7: () => btnLightsDown.PerformClick(),
       onF8: () => btnLifeline1.PerformClick(),   // NEW
       onF9: () => btnLifeline2.PerformClick(),   // NEW
       onF10: () => btnLifeline3.PerformClick(),  // NEW
       onF11: () => btnLifeline4.PerformClick(),  // NEW
       onPageUp: LevelUp,
       onPageDown: LevelDown,
       onEnd: () => btnWalk.PerformClick(),
       onR: () => btnActivateRiskMode.PerformClick()
   );
   ```

2. Hotkeys will respect current lifeline button states:
   - **Inactive Mode** (grey, disabled) → No action
   - **Demo Mode** (yellow, clickable) → Trigger demo
   - **Standby Mode** (orange, disabled) → No action
   - **Active Mode** (green, clickable) → Execute lifeline

### Phase 3: Remove Obsolete TODO Comments (5 minutes)

**File**: `src/MillionaireGame/Services/HotkeyHandler.cs`

**Changes**:
Remove the Ctrl+1-4 TODO comments (lines 134-155):
- These were placeholders for lifeline mapping
- F8-F11 is the final implementation
- Ctrl+1-4 remains unused for future features (e.g., Stream Deck)

### Phase 4: Update Documentation (10 minutes)

**Files to Update**:
1. `CHANGELOG.md` - Add to v0.9.8 changes
2. `V1.0_RELEASE_STATUS.md` - Mark hotkey mapping as complete
3. `PRE_1.0_FINAL_CHECKLIST.md` - Remove from deferred items
4. `START_HERE.md` - Update feature list

---

## 🧪 Testing Strategy

### Manual Testing
1. **Basic Hotkey Test**:
   - Start game, reach Q1
   - Press F8/F9/F10/F11 and verify correct lifeline activates
   - Verify hotkeys respect button enabled state

2. **State Mode Test**:
   - Test in Inactive mode (before Explain Game) → No action
   - Test in Demo mode (during Explain Game) → Demo triggers
   - Test in Standby mode (after Lights Down) → No action
   - Test in Active mode (after all 4 answers shown) → Lifeline activates

3. **Configuration Test**:
   - Change lifeline order in Settings
   - Verify F8-F11 still map to button positions (not specific lifelines)

4. **Conflict Test**:
   - Verify no conflicts with existing hotkeys
   - Test all F1-F11 keys in sequence

### Edge Cases
- Lifeline already used (button disabled)
- Lifeline unavailable in current level range
- Web server offline (ATA fallback to offline mode)
- Rapid hotkey presses (debounce not needed - button click handles state)

---

## ⚠️ Risk Assessment

### Low Risk
- **Reason**: Uses existing `PerformClick()` pattern (proven safe)
- **Mitigation**: Lifeline buttons handle all state logic internally
- **Validation**: Button enabled state checked before click

### No Breaking Changes
- **Existing hotkeys unchanged**
- **New hotkeys additive only**
- **Settings/configuration unaffected**

---

## 📊 Success Criteria

- [x] F8-F11 hotkeys map to btnLifeline1-4
- [x] Hotkeys respect button enabled state
- [x] No conflicts with existing hotkeys
- [x] Documentation updated
- [x] Build succeeds with 0 new warnings (3 pre-existing AudioMixer warnings)
- [ ] Manual testing completed
- [ ] User confirms hotkey behavior

---

## 📝 Future Considerations

### Post-v1.0 (v1.2 Target)
**Elgato Stream Deck Integration**:
- Custom button images for each lifeline
- LED feedback for lifeline availability
- Multi-action buttons (e.g., press for demo, hold for activate)
- Scene switching automation

**Ctrl+1-4 remains available** for Stream Deck command mapping or other future features.

---

## 📚 Related Documents

- `HotkeyHandler.cs` - Core hotkey processing
- `ControlPanelForm.cs` - Hotkey registration and button mapping
- `LifelineManager.cs` - Lifeline business logic
- `PRE_1.0_FINAL_CHECKLIST.md` - Original deferred item

---

**Document Created**: December 31, 2025  
**Purpose**: Implementation plan for lifeline hotkey mapping (F8-F11)  
**Next Step**: Review plan with user, then implement
