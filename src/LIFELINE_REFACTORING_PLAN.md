# Lifeline System Refactoring Plan
**Date**: December 20, 2025  
**Version**: 0.3 (Planned)  
**Goal**: Modularize lifeline system for better maintainability and dynamic configuration

---

## Overview

The current lifeline system is hardcoded with specific buttons (btn5050, btnPAF, btnATA, btnSTQ) and uses radio buttons for availability settings. This refactoring will:

1. Replace radio buttons with ComboBoxes for cleaner UI
2. Make the total lifelines selector functional
3. Allow dynamic lifeline type assignment (e.g., make Lifeline 1 be PAF instead of 50:50)
4. Modularize the system for easy addition of new lifelines in the future

---

## Current System Analysis

### Control Panel Buttons
- **btn5050** - Lifeline slot 1 (default: 50:50)
- **btnPAF** - Lifeline slot 2 (default: Phone-a-Friend)
- **btnATA** - Lifeline slot 3 (default: Ask the Audience)
- **btnSTQ** - Lifeline slot 4 (default: Switch the Question - not implemented)

### Settings Storage
```csharp
// ApplicationSettings.cs
public int TotalLifelines { get; set; } = 4;
public string Lifeline1 { get; set; } = "5050";
public string Lifeline2 { get; set; } = "plusone";
public string Lifeline3 { get; set; } = "ata";
public string Lifeline4 { get; set; } = "switch";
public int Lifeline1Available { get; set; } = 0; // 0=Always, 1=AfterQ5, 2=AfterQ10, 3=RiskMode
public int Lifeline2Available { get; set; } = 0;
public int Lifeline3Available { get; set; } = 0;
public int Lifeline4Available { get; set; } = 0;
```

### Current Issues
1. **Radio buttons**: 16 radio buttons total (4 per lifeline) - cluttered UI
2. **TotalLifelines unused**: Selector does nothing
3. **Fixed button assignment**: Cannot change which lifeline goes in which slot
4. **Hardcoded logic**: Each lifeline type has specific click handlers and logic

---

## Refactoring Goals

### 1. Settings Dialog (OptionsDialog) Changes

#### Replace Radio Buttons with ComboBoxes
**Before**:
```
Lifeline 1
  Type: [50:50 ▼]
  ○ Always Available
  ○ After Question 5
  ○ After Question 10
  ○ In Risk Mode Only
```

**After**:
```
Lifeline 1
  Type: [50:50 ▼]
  Availability: [Always Available ▼]
```

**Implementation**:
- Remove 16 RadioButtons (4 per lifeline × 4 lifelines)
- Add 4 ComboBoxes: `cmbLifeline1Availability`, `cmbLifeline2Availability`, etc.
- ComboBox items: "Always Available", "After Question 5", "After Question 10", "In Risk Mode Only"
- Update `SetLifelineAvailability()` and `GetLifelineAvailability()` methods

#### Make Total Lifelines Selector Functional
**Behavior**:
- `numTotalLifelines` value: 1-4
- When value changes:
  - Enable/disable corresponding group boxes (grpLifeline1-4)
  - Grey out disabled group boxes
  - Update control panel button visibility when settings saved

**Implementation**:
```csharp
private void numTotalLifelines_ValueChanged(object sender, EventArgs e)
{
    int total = (int)numTotalLifelines.Value;
    
    grpLifeline1.Enabled = total >= 1;
    grpLifeline2.Enabled = total >= 2;
    grpLifeline3.Enabled = total >= 3;
    grpLifeline4.Enabled = total >= 4;
    
    MarkChanged();
}
```

### 2. Control Panel (ControlPanelForm) Changes

#### Modular Lifeline Button System

**Current**: Hardcoded button names and click handlers
```csharp
private void btn5050_Click(object? sender, EventArgs e)
{
    HandleLifelineClick(1, btn5050);
}
```

**New**: Dynamic button configuration array
```csharp
private Button[] _lifelineButtons;
private LifelineConfig[] _lifelineConfigs;

private void InitializeLifelineSystem()
{
    _lifelineButtons = new[] { btn5050, btnPAF, btnATA, btnSTQ };
    _lifelineConfigs = new LifelineConfig[4];
    LoadLifelineConfiguration();
    ApplyLifelineConfiguration();
}

private void LoadLifelineConfiguration()
{
    var settings = _appSettings.Settings;
    
    _lifelineConfigs[0] = new LifelineConfig
    {
        Type = ParseLifelineType(settings.Lifeline1),
        Availability = (LifelineAvailability)settings.Lifeline1Available,
        IsEnabled = settings.TotalLifelines >= 1
    };
    
    // ... repeat for lifelines 2-4
}

private void ApplyLifelineConfiguration()
{
    for (int i = 0; i < 4; i++)
    {
        var button = _lifelineButtons[i];
        var config = _lifelineConfigs[i];
        
        // Set button visibility
        button.Visible = config.IsEnabled;
        
        // Set button text based on lifeline type
        button.Text = GetLifelineDisplayName(config.Type);
        
        // Update button state based on availability
        UpdateLifelineButtonState(button, config, _gameService.State.CurrentLevel);
    }
}
```

#### New Helper Class: LifelineConfig
```csharp
public class LifelineConfig
{
    public LifelineType Type { get; set; }
    public LifelineAvailability Availability { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsUsed { get; set; }
}
```

#### Dynamic Click Handler
```csharp
private void LifelineButton_Click(object? sender, EventArgs e)
{
    if (sender is not Button button)
        return;
        
    int slotIndex = Array.IndexOf(_lifelineButtons, button);
    if (slotIndex < 0 || slotIndex >= _lifelineConfigs.Length)
        return;
        
    var config = _lifelineConfigs[slotIndex];
    ExecuteLifeline(config.Type, button, slotIndex + 1);
}

private void ExecuteLifeline(LifelineType type, Button button, int slotNumber)
{
    switch (type)
    {
        case LifelineType.FiftyFifty:
            Execute5050(button, slotNumber);
            break;
        case LifelineType.PlusOne:
            ExecutePAF(button, slotNumber);
            break;
        case LifelineType.AskTheAudience:
            ExecuteATA(button, slotNumber);
            break;
        case LifelineType.SwitchQuestion:
            ExecuteSTQ(button, slotNumber);
            break;
        // Future lifelines...
    }
}
```

### 3. Lifeline Type Mapping

**String to Enum Mapping**:
```csharp
private LifelineType ParseLifelineType(string typeString)
{
    return typeString?.ToLowerInvariant() switch
    {
        "5050" => LifelineType.FiftyFifty,
        "plusone" => LifelineType.PlusOne,
        "ata" => LifelineType.AskTheAudience,
        "switch" => LifelineType.SwitchQuestion,
        "doubledip" => LifelineType.DoubleDip,
        "askhost" => LifelineType.AskTheHost,
        _ => LifelineType.None
    };
}

private string GetLifelineDisplayName(LifelineType type)
{
    return type switch
    {
        LifelineType.FiftyFifty => "50:50",
        LifelineType.PlusOne => "Phone",
        LifelineType.AskTheAudience => "Audience",
        LifelineType.SwitchQuestion => "Switch",
        LifelineType.DoubleDip => "Double Dip",
        LifelineType.AskTheHost => "Ask Host",
        _ => "Lifeline"
    };
}
```

---

## Implementation Steps

### Phase 1: Settings Dialog Refactoring

1. **OptionsDialog.Designer.cs**:
   - Remove 16 RadioButton declarations and initializations
   - Add 4 ComboBox declarations: `cmbLifeline1Availability` through `cmbLifeline4Availability`
   - Update InitializeComponent() to create and configure new ComboBoxes
   - Position ComboBoxes where radio buttons were (y-coordinate ~65)
   - Add `numTotalLifelines.ValueChanged` event subscription

2. **OptionsDialog.cs**:
   - Replace `SetLifelineAvailability(RadioButton, RadioButton, RadioButton, RadioButton, int)` 
     with `SetLifelineAvailability(ComboBox, int)`
   - Replace `GetLifelineAvailability(RadioButton, RadioButton, RadioButton, RadioButton)` 
     with `GetLifelineAvailability(ComboBox)`
   - Add `numTotalLifelines_ValueChanged(object, EventArgs)` handler
   - Update LoadSettings() to use new methods
   - Update SaveSettings() to use new methods

### Phase 2: Control Panel Modularization

3. **ControlPanelForm.cs**:
   - Add `LifelineConfig[]` field
   - Add `Button[]` field for lifeline buttons
   - Create `InitializeLifelineSystem()` method (called in constructor)
   - Create `LoadLifelineConfiguration()` method
   - Create `ApplyLifelineConfiguration()` method
   - Create `ParseLifelineType(string)` helper
   - Create `GetLifelineDisplayName(LifelineType)` helper
   - Update existing click handlers to route through dynamic system
   - OR: Replace click handlers entirely with unified `LifelineButton_Click()`

4. **Refactor Execution Methods**:
   - Rename `btn5050_Click` → `Execute5050(Button, int)`
   - Rename `btnPAF_Click` → `ExecutePAF(Button, int)`
   - Rename `btnATA_Click` → `ExecuteATA(Button, int)`
   - Add `ExecuteSTQ(Button, int)` for Switch the Question
   - Create router `ExecuteLifeline(LifelineType, Button, int)`

### Phase 3: GameService Integration

5. **GameService.cs**:
   - Update lifeline initialization to use configuration from settings
   - Add method to get lifeline config by slot number
   - Ensure `UpdateLifelineStates()` works with dynamic configuration

6. **Testing Scenarios**:
   - Change Lifeline 1 from 50:50 to PAF → Button should say "Phone"
   - Set TotalLifelines to 2 → Only first 2 buttons visible
   - Set availability to "After Q5" → Button disabled until Q5
   - Save and reload settings → Configuration persists correctly

---

## Files to Modify

### Critical Changes
1. **OptionsDialog.Designer.cs** - Remove radio buttons, add comboboxes (~150 lines changed)
2. **OptionsDialog.cs** - Update availability methods (~40 lines changed)
3. **ControlPanelForm.cs** - Modularize lifeline system (~200 lines changed)

### Supporting Changes
4. **ApplicationSettings.cs** - No changes needed (already has necessary fields)
5. **GameService.cs** - Minor updates for dynamic configuration (~20 lines)

---

## Benefits of This Refactoring

### User Experience
- ✅ Cleaner settings UI (ComboBox vs 4 radio buttons per lifeline)
- ✅ Total lifelines selector actually works
- ✅ Can assign any lifeline to any slot
- ✅ Easier to understand and configure

### Developer Experience
- ✅ Easy to add new lifelines (add to enum, add case in switch)
- ✅ No need to add new buttons or handlers
- ✅ Configuration-driven instead of hardcoded
- ✅ Less code duplication

### Maintainability
- ✅ Single execution router for all lifelines
- ✅ Consistent lifeline behavior
- ✅ Easier testing (config-based)
- ✅ Future-proof architecture

---

## Migration Path

### Existing Configs
Old configs will continue to work:
- Settings fields remain the same
- String values ("5050", "plusone", etc.) still used
- Availability integers (0-3) unchanged

### New Features
- Users can now change lifeline types per slot
- Users can set number of active lifelines
- Control panel adapts automatically

---

## Testing Checklist

### Settings Dialog
- [ ] ComboBoxes display correct initial values
- [ ] Changing availability updates settings
- [ ] Changing lifeline type updates settings
- [ ] TotalLifelines selector enables/disables group boxes
- [ ] All four lifelines can be configured independently
- [ ] Settings persist across app restarts

### Control Panel
- [ ] Buttons show correct lifeline names
- [ ] Only N buttons visible based on TotalLifelines
- [ ] Click handler routes to correct lifeline execution
- [ ] Lifeline 1 can be PAF (button says "Phone", acts like PAF)
- [ ] Lifeline 2 can be 50:50 (button says "50:50", removes 2 answers)
- [ ] Availability rules work correctly (unlock at Q5/Q10/Risk)
- [ ] Lifeline state management (enabled/disabled/used) works

### Edge Cases
- [ ] All 4 lifelines set to same type (e.g., all 50:50)
- [ ] TotalLifelines = 1 (only first button visible)
- [ ] TotalLifelines = 4, then changed to 2 mid-game
- [ ] Lifeline type changed between games
- [ ] Invalid/missing configuration (graceful degradation)

---

## Future Enhancements (Post-Refactoring)

1. **Visual Lifeline Icons**: Instead of text, use icons for each lifeline type
2. **Lifeline Descriptions**: Tooltip or help text explaining what each does
3. **Lifeline History**: Track which lifelines were used in each game
4. **Custom Lifelines**: Allow users to create custom lifeline types (advanced)
5. **Lifeline Combos**: Special effects when using multiple lifelines together

---

## Implementation Timeline

**Estimated Time**: 3-4 hours

1. Phase 1 (Settings Dialog): 1-1.5 hours
2. Phase 2 (Control Panel): 1.5-2 hours  
3. Phase 3 (Testing & Polish): 0.5-1 hour

**Completion Target**: End of current session or next session

---

## Notes

- Designer file changes are verbose but straightforward
- Must ensure click handlers are properly routed
- Button visibility changes require proper layout/anchoring
- Settings must be backwards-compatible
- Extensive testing needed due to core functionality change

---

**End of Plan Document**

