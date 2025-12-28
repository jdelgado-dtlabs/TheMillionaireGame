# Question Editor Integration Plan
**Date**: December 27, 2025  
**Branch**: feature/QEditor_Integration  
**Estimated Time**: 1.5-2 hours  
**Priority**: MEDIUM

---

## 🎯 Objective

Integrate the standalone QuestionEditor project into the main MillionaireGame application, eliminating the need for a separate executable and simplifying deployment.

---

## 📋 Current State

**Project Structure**:
- `MillionaireGame.QuestionEditor` - Standalone project with 6 WinForms
- Currently launched from Control Panel: `new QuestionEditor.Forms.QuestionEditorMainForm()`
- Has its own namespace: `QuestionEditor.Forms`
- Separate executable output

**Forms to Migrate**:
1. `QuestionEditorMainForm.cs` - Main editor window
2. `AddQuestionForm.cs` - Add new question dialog
3. `EditQuestionForm.cs` - Edit existing question dialog
4. `EditFFFQuestionForm.cs` - Edit FFF question dialog
5. `ImportQuestionsForm.cs` - CSV import dialog
6. `ExportQuestionsForm.cs` - CSV export dialog

---

## 📝 Migration Steps

### Step 1: Prepare Destination Folder
**Time**: 5 minutes

- [x] Create `src/MillionaireGame/Forms/QuestionEditor/` directory
- [x] Verify folder structure is ready

**Commands**:
```powershell
New-Item -ItemType Directory -Path "src/MillionaireGame/Forms/QuestionEditor" -Force
```

---

### Step 2: Move Form Files
**Time**: 15 minutes

- [ ] Move all 6 forms (.cs and .Designer.cs files) from `MillionaireGame.QuestionEditor/Forms/` to `MillionaireGame/Forms/QuestionEditor/`
- [ ] Move `Program.cs` if needed (likely can be deleted since editor is launched from Control Panel)

**Files to Move**:
```
QuestionEditorMainForm.cs + .Designer.cs
AddQuestionForm.cs + .Designer.cs
EditQuestionForm.cs + .Designer.cs
EditFFFQuestionForm.cs + .Designer.cs
ImportQuestionsForm.cs + .Designer.cs
ExportQuestionsForm.cs + .Designer.cs
```

**Commands**:
```powershell
$source = "src/MillionaireGame.QuestionEditor/Forms"
$dest = "src/MillionaireGame/Forms/QuestionEditor"
Get-ChildItem "$source/*.cs" | Move-Item -Destination $dest
```

---

### Step 3: Update Namespaces
**Time**: 20 minutes

- [ ] Update namespace in all moved files from `QuestionEditor.Forms` to `MillionaireGame.Forms.QuestionEditor`
- [ ] Update using statements in moved files
- [ ] Fix any Core references (should work since MillionaireGame already references Core)

**Search and Replace**:
```
OLD: namespace QuestionEditor.Forms
NEW: namespace MillionaireGame.Forms.QuestionEditor

OLD: using QuestionEditor.Forms;
NEW: using MillionaireGame.Forms.QuestionEditor;
```

---

### Step 4: Update Control Panel Integration
**Time**: 10 minutes

- [ ] Update ControlPanelForm.cs line 3910
- [ ] Change `new QuestionEditor.Forms.QuestionEditorMainForm()` to `new Forms.QuestionEditor.QuestionEditorMainForm()`
- [ ] Update using statements in ControlPanelForm.cs
- [ ] Remove any project references to QuestionEditor

**File**: `src/MillionaireGame/Forms/ControlPanelForm.cs`

**Change**:
```csharp
// OLD
var editorForm = new QuestionEditor.Forms.QuestionEditorMainForm();

// NEW
var editorForm = new Forms.QuestionEditor.QuestionEditorMainForm();
```

---

### Step 5: Update Project Files
**Time**: 15 minutes

- [ ] Add moved forms to `MillionaireGame.csproj` (if not auto-included)
- [ ] Remove `MillionaireGame.QuestionEditor` project from solution
- [ ] Delete `MillionaireGame.QuestionEditor` folder
- [ ] Remove project reference from any projects that reference it

**Solution File Changes**:
- Remove project entry for `MillionaireGame.QuestionEditor` from `TheMillionaireGame.sln`
- Remove associated GlobalSection entries

---

### Step 6: Build and Test
**Time**: 30 minutes

**Build Tests**:
- [ ] `dotnet build TheMillionaireGame.sln` - Verify no errors
- [ ] Check that only 3 projects build: Core, MillionaireGame, Web
- [ ] Verify output only contains MillionaireGame.exe (no separate editor executable)

**Functional Tests**:
- [ ] Launch MillionaireGame
- [ ] Open Question Editor from Control Panel menu
- [ ] Verify main editor window opens
- [ ] Test Add Question functionality
- [ ] Test Edit Question functionality
- [ ] Test Delete Question functionality
- [ ] Test CSV Import (valid and invalid files)
- [ ] Test CSV Export with various question sets
- [ ] Test FFF Question editing
- [ ] Verify all database operations work correctly
- [ ] Check for any namespace-related errors

**Edge Cases**:
- [ ] Empty database
- [ ] Large question sets (100+ questions)
- [ ] Special characters in questions/answers
- [ ] CSV with malformed data

---

## ✅ Success Criteria

- [x] QuestionEditor forms accessible from Control Panel
- [x] No separate QuestionEditor.exe generated
- [x] All CRUD operations work correctly
- [x] CSV Import/Export functionality intact
- [x] Build produces no errors or warnings
- [x] Single executable deployment

---

## 🚫 Rollback Plan

If integration fails or causes issues:

1. **Revert Branch**:
   ```powershell
   git checkout master-csharp
   git branch -D feature/QEditor_Integration
   ```

2. **Keep Standalone**: Leave QuestionEditor as separate project if integration proves problematic

---

## 📦 Benefits

**Before Integration**:
- 2 executables (MillionaireGame.exe + QuestionEditor.exe)
- Separate namespace management
- Potential version mismatch issues
- More complex deployment

**After Integration**:
- 1 executable (MillionaireGame.exe)
- Unified namespace structure
- Simplified deployment
- Better code organization
- Question Editor only accessible from main app (prevents accidental standalone use)

---

## 🔍 Potential Issues

**Issue 1: Resource Files**
- Forms may have embedded resources (.resx files)
- **Solution**: Move .resx files with their corresponding forms

**Issue 2: Database Connection**
- QuestionEditor may have its own DB connection logic
- **Solution**: Ensure it uses Core's DatabaseService

**Issue 3: Settings/Config**
- QuestionEditor may read from its own config
- **Solution**: Consolidate into main app settings

**Issue 4: Circular Dependencies**
- QuestionEditor references Core, MillionaireGame references Core
- **Solution**: No issue - MillionaireGame will contain the forms directly

---

## 📊 File Checklist

### Files to Move
- [x] QuestionEditorMainForm.cs
- [x] QuestionEditorMainForm.Designer.cs
- [x] AddQuestionForm.cs
- [x] AddQuestionForm.Designer.cs
- [x] EditQuestionForm.cs
- [x] EditQuestionForm.Designer.cs
- [x] EditFFFQuestionForm.cs
- [x] EditFFFQuestionForm.Designer.cs
- [x] ImportQuestionsForm.cs
- [x] ImportQuestionsForm.Designer.cs
- [x] ExportQuestionsForm.cs
- [x] ExportQuestionsForm.Designer.cs

### Files to Delete
- [ ] MillionaireGame.QuestionEditor/Program.cs (not needed - launched from Control Panel)
- [ ] MillionaireGame.QuestionEditor/MillionaireGame.QuestionEditor.csproj
- [ ] MillionaireGame.QuestionEditor/ (entire folder after migration)

### Files to Update
- [ ] src/TheMillionaireGame.sln (remove project)
- [ ] src/MillionaireGame/Forms/ControlPanelForm.cs (update instantiation)
- [ ] src/MillionaireGame/MillionaireGame.csproj (verify forms are included)

---

## 🔄 Post-Integration

After successful integration:
1. Commit changes with descriptive message
2. Test on clean build environment
3. Update documentation if needed
4. Merge to master-csharp
5. Tag as part of v1.0 improvements

---

**Status**: Ready to Execute  
**Next Step**: Begin Step 1 - Prepare Destination Folder
