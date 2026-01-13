# Session Checkpoint: Answer Letter Wrapping Fix
**Date:** January 8, 2026  
**Branch:** master-v1.0.5  
**Status:** Analysis Complete - Ready for Implementation

## Issue Identified
Graphical glitches occurring on all screens where answer letters (A:, B:, C:, D:) are wrapping incorrectly. The letter and colon are being split across lines.

## Root Cause Analysis
The answer letter rendering area is allocated only **60 pixels width**, which is insufficient when scaled. The text rendering system is wrapping the content, separating the letter from its colon.

### Affected Code Locations
All three screen forms have identical issue in their `DrawAnswerBox` methods:

1. **HostScreenForm.cs** - Line ~267
   - File: `src/MillionaireGame/Forms/HostScreenForm.cs`
   
2. **GuestScreenForm.cs** - Line ~247
   - File: `src/MillionaireGame/Forms/GuestScreenForm.cs`
   
3. **TVScreenForm.cs** - Line ~297
   - File: `src/MillionaireGame/Forms/TVScreenForm.cs`

### Current Code (Problematic)
```csharp
DrawScaledText(g, letter + ":", letterFont, letterBrush,
    bounds.X + letterLeftPadding, bounds.Y + 15,
    60, bounds.Height - 30,  // ← TOO NARROW
    letterFormat);
```

## Proposed Solution
Increase the letter rendering width from **60 to 80 pixels** to provide adequate space.

### Implementation Changes
Replace the width parameter in all three files:

**Change:**
```csharp
// OLD
DrawScaledText(g, letter + ":", letterFont, letterBrush,
    bounds.X + letterLeftPadding, bounds.Y + 15,
    60, bounds.Height - 30,
    letterFormat);

// NEW
DrawScaledText(g, letter + ":", letterFont, letterBrush,
    bounds.X + letterLeftPadding, bounds.Y + 15,
    80, bounds.Height - 30,  // Increased from 60 to 80
    letterFormat);
```

## Implementation Plan

### Step 1: Apply Code Changes
Use `multi_replace_string_in_file` to update all three files simultaneously:
- HostScreenForm.cs - Line 267
- GuestScreenForm.cs - Line 247
- TVScreenForm.cs - Line 297

### Step 2: Build & Test
```powershell
Stop-Process -Name "MillionaireGame*" -Force -ErrorAction SilentlyContinue
cd src
dotnet build TheMillionaireGame.sln
```

### Step 3: Verification Checklist
- [ ] Host screen displays letters correctly
- [ ] Guest screen displays letters correctly
- [ ] TV screen displays letters correctly
- [ ] Test both left positions (A, C) and right positions (B, D)
- [ ] Verify no overlap between letter and answer text
- [ ] Test at different screen resolutions
- [ ] Test during FFF reveal with custom labels
- [ ] Test during answer selection states
- [ ] Test during reveal states (correct/wrong answers)

## Alternative Adjustments (If Needed)
If 80 pixels proves insufficient:
- Try 90-100 pixels
- Consider adding `StringFormat.FormatFlags = StringFormatFlags.NoWrap`
- Verify `textLeftPadding` doesn't cause overlap issues

## Next Steps
1. Implement the width change in all three files
2. Build the solution
3. Run the application and test all scenarios
4. Update CHANGELOG.md with fix details
5. Commit changes to master-v1.0.5 branch

## Context Notes
- This is a visual rendering bug affecting all game screens
- No functional logic changes required
- Simple parameter adjustment with immediate visual impact
- Should be safe to merge once verified visually

## Session End
Ready for implementation when user returns.
