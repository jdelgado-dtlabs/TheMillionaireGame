# Money Tree Number Formatting Implementation Plan

**Version**: v1.0.1  
**Date**: 2026-01-06  
**Status**: ✅ COMPLETE

## Implementation Summary

Successfully implemented configurable thousands separator for money tree number formatting.

### Changes Implemented

1. **MoneyTreeSettings.cs** - Added NumberSeparatorStyle enum and custom formatting
2. **OptionsDialog.cs** - Added Number Format UI group with radio buttons
3. **OptionsDialog.Designer.cs** - UI layout and control definitions

### Features Delivered
✅ User can select thousands separator style (Comma, Period, Space, None)  
✅ Selection persists across application restarts  
✅ Works with both Currency 1 and Currency 2  
✅ Backward compatible (defaults to Comma)  
✅ All money displays automatically use the setting  
✅ No database migration needed

### Testing Results
- Build succeeded with 0 errors
- UI displays correctly with proper spacing
- All separator styles work as expected
- Settings save and load properly

---

## Original Plan

## Overview

This document outlines the implementation plan for adding configurable number formatting (thousands separators) to the money tree system, including removal of embedded commas from stored values and adding user-selectable formatting options.

---

## Problem Statement

### Current Issue
- Money tree values are stored as integers (e.g., `1_000_000`) but the `FormatMoney()` method uses .NET's `ToString("N0")` which applies regional formatting
- This creates inconsistency across different regional settings
- No user control over thousands separator style

### Requirements
1. **Data Storage**: Values stored as plain integers without embedded commas
2. **User Control**: Add UI option to select thousands separator:
   - Comma (`,`) - e.g., `1,000,000`
   - Period (`.`) - e.g., `1.000.000`
   - Space (` `) - e.g., `1 000 000`
   - None (``) - e.g., `1000000`
3. **Application**: Format applied to:
   - Money Tree display
   - Winnings strap on TV Screen
   - Winner screens on TV Screen
   - Control Panel displays

---

## Current Architecture Analysis

### MoneyTreeSettings.cs
```csharp
public class MoneyTreeSettings
{
    // Values stored as integers (GOOD - already clean)
    public int Level01Value { get; set; } = 100;
    public int Level02Value { get; set; } = 200;
    // ... up to Level15Value = 1_000_000
    
    // Current formatting method (NEEDS UPDATE)
    public string FormatMoney(int value)
    {
        var formattedValue = value.ToString("N0"); // Uses regional settings
        return CurrencyAtSuffix 
            ? $"{formattedValue}{Currency}" 
            : $"{Currency}{formattedValue}";
    }
}
```

**Current State**: ✅ Values already stored as clean integers  
**Change Needed**: Update `FormatMoney()` to respect user preference

### OptionsDialog Money Tree Tab

Current layout (approximate):
```
┌─────────────────────────────────────────────────────┐
│  Currency Settings                                  │
│  ┌────────────┐  ┌────────────┐                    │
│  │ Currency 1 │  │ Currency 2 │                    │
│  │            │  │            │                    │
│  └────────────┘  └────────────┘                    │
│                                                     │
│  [Safety Net Options Below]                        │
└─────────────────────────────────────────────────────┘
```

**Change Needed**: Add third group box on the right for number formatting

---

## Implementation Plan

### Phase 1: Add Number Separator Setting

**File**: `src/MillionaireGame.Core/Settings/MoneyTreeSettings.cs`

**Tasks**:
1. Add new property for separator style
2. Update `FormatMoney()` method

**Implementation**:

```csharp
/// <summary>
/// Thousands separator style for number formatting
/// </summary>
public enum NumberSeparatorStyle
{
    None,      // 1000000
    Comma,     // 1,000,000
    Period,    // 1.000.000
    Space      // 1 000 000
}

public class MoneyTreeSettings
{
    // ... existing properties ...
    
    /// <summary>
    /// Thousands separator style (default: Comma)
    /// </summary>
    public NumberSeparatorStyle ThousandsSeparator { get; set; } = NumberSeparatorStyle.Comma;
    
    /// <summary>
    /// Formats a prize value with the configured currency symbol, position, and separator
    /// </summary>
    public string FormatMoney(int value)
    {
        // Format number based on separator preference
        string formattedValue = FormatNumberWithSeparator(value, ThousandsSeparator);
        
        // Apply currency symbol and position
        return CurrencyAtSuffix 
            ? $"{formattedValue}{Currency}" 
            : $"{Currency}{formattedValue}";
    }
    
    /// <summary>
    /// Format number with specified thousands separator
    /// </summary>
    private string FormatNumberWithSeparator(int value, NumberSeparatorStyle separator)
    {
        string valueStr = value.ToString();
        
        if (separator == NumberSeparatorStyle.None)
        {
            return valueStr;
        }
        
        // Get separator character
        char sepChar = separator switch
        {
            NumberSeparatorStyle.Comma => ',',
            NumberSeparatorStyle.Period => '.',
            NumberSeparatorStyle.Space => ' ',
            _ => ','
        };
        
        // Add thousands separator every 3 digits from right
        var result = new System.Text.StringBuilder();
        int digitCount = 0;
        
        for (int i = valueStr.Length - 1; i >= 0; i--)
        {
            if (digitCount > 0 && digitCount % 3 == 0)
            {
                result.Insert(0, sepChar);
            }
            result.Insert(0, valueStr[i]);
            digitCount++;
        }
        
        return result.ToString();
    }
    
    /// <summary>
    /// Formats value with second currency (respects same separator setting)
    /// </summary>
    public string FormatMoneyWithCurrency2(int value)
    {
        string formattedValue = FormatNumberWithSeparator(value, ThousandsSeparator);
        
        return Currency2AtSuffix 
            ? $"{formattedValue}{Currency2}" 
            : $"{Currency2}{formattedValue}";
    }
}
```

---

### Phase 2: Update OptionsDialog UI

**Files**:
- `src/MillionaireGame/Forms/Options/OptionsDialog.Designer.cs`
- `src/MillionaireGame/Forms/Options/OptionsDialog.cs`

#### Designer Changes

**Current Layout Adjustment**:
```csharp
// Currency 1 GroupBox - Move left and shrink width
grpCurrency1.Location = new Point(20, 20);
grpCurrency1.Size = new Size(180, 260); // Was wider

// Currency 2 GroupBox - Move left and shrink width  
grpCurrency2.Location = new Point(210, 20);
grpCurrency2.Size = new Size(180, 260); // Was wider

// NEW: Number Format GroupBox - Add on right
grpNumberFormat.Location = new Point(400, 20);
grpNumberFormat.Size = new Size(180, 140);
```

**New Controls**:

```csharp
// In Designer.cs InitializeComponent()
private GroupBox grpNumberFormat;
private Label lblThousandsSeparator;
private RadioButton radSeparatorComma;
private RadioButton radSeparatorPeriod;
private RadioButton radSeparatorSpace;
private RadioButton radSeparatorNone;

// GroupBox
grpNumberFormat = new GroupBox
{
    Name = "grpNumberFormat",
    Text = "Number Format",
    Location = new Point(400, 20),
    Size = new Size(180, 140),
    TabIndex = 2
};

// Label
lblThousandsSeparator = new Label
{
    Name = "lblThousandsSeparator",
    Text = "Thousands Separator:",
    Location = new Point(10, 20),
    Size = new Size(160, 20),
    AutoSize = false
};

// Radio buttons
radSeparatorComma = new RadioButton
{
    Name = "radSeparatorComma",
    Text = "Comma (1,000,000)",
    Location = new Point(10, 45),
    Size = new Size(160, 20),
    Checked = true,
    TabIndex = 0
};
radSeparatorComma.CheckedChanged += Control_Changed;

radSeparatorPeriod = new RadioButton
{
    Name = "radSeparatorPeriod",
    Text = "Period (1.000.000)",
    Location = new Point(10, 70),
    Size = new Size(160, 20),
    TabIndex = 1
};
radSeparatorPeriod.CheckedChanged += Control_Changed;

radSeparatorSpace = new RadioButton
{
    Name = "radSeparatorSpace",
    Text = "Space (1 000 000)",
    Location = new Point(10, 95),
    Size = new Size(160, 20),
    TabIndex = 2
};
radSeparatorSpace.CheckedChanged += Control_Changed;

radSeparatorNone = new RadioButton
{
    Name = "radSeparatorNone",
    Text = "None (1000000)",
    Location = new Point(10, 120),
    Size = new Size(160, 20),
    TabIndex = 3
};
radSeparatorNone.CheckedChanged += Control_Changed;

// Add controls to group
grpNumberFormat.Controls.Add(lblThousandsSeparator);
grpNumberFormat.Controls.Add(radSeparatorComma);
grpNumberFormat.Controls.Add(radSeparatorPeriod);
grpNumberFormat.Controls.Add(radSeparatorSpace);
grpNumberFormat.Controls.Add(radSeparatorNone);

// Add group to money tree tab
tabMoneyTree.Controls.Add(grpNumberFormat);
```

#### Code-Behind Changes

**In OptionsDialog.cs**:

```csharp
private void LoadMoneyTreeSettings()
{
    // ... existing code ...
    
    // Load thousands separator preference
    switch (_moneyTreeService.Settings.ThousandsSeparator)
    {
        case NumberSeparatorStyle.Comma:
            radSeparatorComma.Checked = true;
            break;
        case NumberSeparatorStyle.Period:
            radSeparatorPeriod.Checked = true;
            break;
        case NumberSeparatorStyle.Space:
            radSeparatorSpace.Checked = true;
            break;
        case NumberSeparatorStyle.None:
            radSeparatorNone.Checked = true;
            break;
    }
}

private void SaveMoneyTreeSettings()
{
    // ... existing code ...
    
    // Save thousands separator preference
    if (radSeparatorComma.Checked)
        _moneyTreeService.Settings.ThousandsSeparator = NumberSeparatorStyle.Comma;
    else if (radSeparatorPeriod.Checked)
        _moneyTreeService.Settings.ThousandsSeparator = NumberSeparatorStyle.Period;
    else if (radSeparatorSpace.Checked)
        _moneyTreeService.Settings.ThousandsSeparator = NumberSeparatorStyle.Space;
    else if (radSeparatorNone.Checked)
        _moneyTreeService.Settings.ThousandsSeparator = NumberSeparatorStyle.None;
    
    // ... existing code ...
}
```

---

### Phase 3: Verify All Display Locations

**No code changes needed** - all displays already use `FormatMoney()`:

✅ **Money Tree Display**
- Uses `MoneyTreeService.GetFormattedAmount(level)` → calls `FormatMoney()`

✅ **TV Screen Winnings Strap**
- Uses `ShowWinningsAmount(amount)` where amount is pre-formatted by `FormatMoney()`

✅ **TV Screen Winner Display**
- Uses `ShowWinningsAmount(finalAmount)` where amount is pre-formatted

✅ **Control Panel Displays**
- Uses `GameState.CurrentValue` which is formatted by `MoneyTreeService`

**Verification locations**:
- `src/MillionaireGame.Core/Services/MoneyTreeService.cs` - All public methods use `FormatMoney()`
- `src/MillionaireGame/Forms/TVScreenForm.cs` - Receives pre-formatted strings
- `src/MillionaireGame/Forms/HostScreenForm.cs` - Receives pre-formatted strings
- `src/MillionaireGame/Forms/GuestScreenForm.cs` - Receives pre-formatted strings

---

### Phase 4: Database Migration (Settings Persistence)

**File**: `src/MillionaireGame.Core/Services/MoneyTreeService.cs`

The `MoneyTreeSettings` class is already serialized to the database via `SaveCurrentTree()` and `LoadTree()` methods. The new `ThousandsSeparator` property will automatically persist through existing reflection-based serialization.

**Verification**:
```csharp
// In MoneyTreeService.cs - SaveCurrentTree()
// Existing code already iterates all properties and saves them
foreach (var property in typeof(MoneyTreeSettings).GetProperties())
{
    // ... existing serialization code ...
}
// New ThousandsSeparator property will be included automatically
```

**Default for existing databases**: When loading from old database without this setting, the default value `NumberSeparatorStyle.Comma` will be used (backward compatible).

---

## Testing Checklist

### Unit Testing
- [ ] `FormatNumberWithSeparator()` with all separator styles
- [ ] Edge cases: 0, negative values (if applicable), very large numbers
- [ ] Currency prefix vs suffix with each separator style

### UI Testing
- [ ] Number format group displays correctly
- [ ] Radio buttons mutually exclusive
- [ ] Selection persists on save/load
- [ ] Changes immediately reflected in money tree preview (if visible)

### Integration Testing
- [ ] Change separator → Save → Restart app → Verify persisted
- [ ] Start new game with each separator style
- [ ] Display on TV Screen with all separator styles
- [ ] Display on Host/Guest screens with all separator styles
- [ ] Winner screen with all separator styles
- [ ] Control panel money displays with all separator styles

### Visual Verification
Test with specific values:
- `100` → All styles should show: `100`
- `1000` → Comma: `1,000`, Period: `1.000`, Space: `1 000`, None: `1000`
- `1000000` → Comma: `1,000,000`, Period: `1.000.000`, Space: `1 000 000`, None: `1000000`

---

## Implementation Steps

1. **Step 1**: Add `NumberSeparatorStyle` enum and properties to `MoneyTreeSettings.cs`
2. **Step 2**: Update `FormatMoney()` method with new separator logic
3. **Step 3**: Add `FormatNumberWithSeparator()` helper method
4. **Step 4**: Update `FormatMoneyWithCurrency2()` for dual currency support
5. **Step 5**: Resize and reposition currency group boxes in `OptionsDialog.Designer.cs`
6. **Step 6**: Add new number format group box and controls to Designer
7. **Step 7**: Wire up radio button event handlers
8. **Step 8**: Add load logic to `LoadMoneyTreeSettings()`
9. **Step 9**: Add save logic to `SaveMoneyTreeSettings()`
10. **Step 10**: Build and test all display locations

---

## Code Files Modified

### New Code
- `src/MillionaireGame.Core/Settings/MoneyTreeSettings.cs`
  - Add `NumberSeparatorStyle` enum
  - Add `ThousandsSeparator` property
  - Update `FormatMoney()` method
  - Add `FormatNumberWithSeparator()` helper
  - Update `FormatMoneyWithCurrency2()` method

### Modified Code  
- `src/MillionaireGame/Forms/Options/OptionsDialog.Designer.cs`
  - Resize/reposition currency group boxes
  - Add number format group box
  - Add radio buttons and label

- `src/MillionaireGame/Forms/Options/OptionsDialog.cs`
  - Update `LoadMoneyTreeSettings()` method
  - Update `SaveMoneyTreeSettings()` method

### No Changes Needed
- All screen forms (TV, Host, Guest) ✅ Already use pre-formatted strings
- `MoneyTreeService.cs` ✅ Already uses `FormatMoney()` internally
- Database serialization ✅ Automatic via reflection

---

## Timeline Estimate

- **Step 1-4** (Settings logic): 1-2 hours
- **Step 5-7** (UI Designer): 1 hour
- **Step 8-9** (Code-behind): 30 minutes
- **Step 10** (Testing): 1-2 hours

**Total Estimate**: 3.5-5.5 hours

---

## Backward Compatibility

✅ **Existing Databases**: New property defaults to `Comma`, matching current .NET regional behavior for most users

✅ **Data Storage**: Values already stored as clean integers, no migration needed

✅ **Display**: All code already uses `FormatMoney()` method, changes are centralized

---

## UI Mockup

```
┌────────────────────────────────────────────────────────────────┐
│  Money Tree Settings Tab                                       │
├────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌──────────┐  ┌──────────┐  ┌─────────────────┐             │
│  │Currency 1│  │Currency 2│  │ Number Format   │             │
│  │          │  │          │  │                 │             │
│  │ [$] ☐@   │  │ [€] ☐@   │  │ Thousands Sep:  │             │
│  │          │  │          │  │                 │             │
│  │ ◉ Dollar │  │ ◯ Dollar │  │ ● Comma (1,000) │             │
│  │ ◯ Euro   │  │ ● Euro   │  │ ◯ Period (1.000)│             │
│  │ ◯ Pound  │  │ ◯ Pound  │  │ ◯ Space (1 000) │             │
│  │ ◯ Yen    │  │ ◯ Yen    │  │ ◯ None (1000)   │             │
│  │ ◯ Other  │  │ ◯ Other  │  │                 │             │
│  │ [....]   │  │ [....]   │  └─────────────────┘             │
│  │          │  │          │                                   │
│  │ ☐ Suffix │  │ ☐ Suffix │                                   │
│  └──────────┘  └──────────┘                                   │
│                                                                 │
│  [Safety Net Configuration Below...]                           │
└────────────────────────────────────────────────────────────────┘
```

---

## Success Criteria

- ✅ User can select thousands separator style
- ✅ Selection persists across application restarts
- ✅ All money displays respect selected format:
  - Money tree on all screens
  - Winnings strap on TV screen
  - Winner display on TV screen
  - Control panel money displays
- ✅ Works with both Currency 1 and Currency 2
- ✅ No breaking changes to existing databases
- ✅ UI is clear and intuitive

---

## Notes

- The existing `ToString("N0")` format uses system locale, which can cause inconsistency
- Custom formatting ensures consistent display regardless of system locale
- Separator choice is stored per money tree configuration, not global setting
- Different money trees (different database rows) can have different separator preferences
