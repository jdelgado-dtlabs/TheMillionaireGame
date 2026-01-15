# Theming System - Quick Start Checklist

**Branch:** `feature/theming-system`  
**Status:** Ready for Phase 1 Implementation

---

## ✅ Pre-Implementation Complete

- [x] Feature branch created from master-v1.0.7
- [x] Codebase review completed
- [x] Implementation plan updated
- [x] Integration points identified
- [x] Session documentation created

---

## 📋 Phase 1: Database & Repository Layer (Week 1)

### Migration Script
- [ ] Create `00008_create_theme_tables.sql` in `src/MillionaireGame/Database/Migrations/`
- [ ] Define `Themes` table
- [ ] Define `ThemeBackgrounds` table
- [ ] Define `ThemeStraps` table
- [ ] Define `ThemeMoneyTree` table
- [ ] Define `ThemePresets` table
- [ ] Define `ThemePacks` table
- [ ] Add indexes for performance
- [ ] Seed 6 built-in preset themes
- [ ] Test migration runs successfully

### Repositories (in `MillionaireGame.Core/Database/`)
- [ ] Create `ThemeRepository.cs`
  - [ ] GetActiveThemeAsync()
  - [ ] GetThemeByIdAsync()
  - [ ] GetAllThemesAsync()
  - [ ] SaveThemeAsync()
  - [ ] SetActiveThemeAsync()
  - [ ] DeleteThemeAsync()
- [ ] Create `ThemeBackgroundRepository.cs`
  - [ ] GetBackgroundsByThemeIdAsync()
  - [ ] SaveBackgroundAsync()
  - [ ] DeleteBackgroundAsync()
- [ ] Create `ThemeStrapRepository.cs`
  - [ ] GetStrapsByThemeIdAsync()
  - [ ] SaveStrapAsync()
  - [ ] DeleteStrapAsync()
- [ ] Create `ThemeMoneyTreeRepository.cs`
  - [ ] GetMoneyTreeByThemeIdAsync()
  - [ ] SaveMoneyTreeAsync()
- [ ] Create `ThemePresetRepository.cs`
  - [ ] GetAllPresetsAsync()
  - [ ] GetPresetByIdAsync()
- [ ] Create `ThemePackRepository.cs`
  - [ ] GetAllPacksAsync()
  - [ ] SavePackAsync()
  - [ ] DeletePackAsync()

### Testing
- [ ] Run application to trigger migration
- [ ] Verify all tables created
- [ ] Verify preset themes loaded
- [ ] Test repository CRUD operations

---

## 📋 Phase 2: Core Services & Models (Week 2)

### Models (in `MillionaireGame.Core/Models/`)
- [ ] Create `Theme.cs`
- [ ] Create `ThemeBackground.cs`
- [ ] Create `ThemeStrap.cs`
- [ ] Create `ThemeMoneyTree.cs`

### Services (in `MillionaireGame.Core/Services/`)
- [ ] Create `ThemeService.cs`
  - [ ] LoadThemeAsync()
  - [ ] ApplyThemeAsync()
  - [ ] ValidateTheme()
  - [ ] GetAvailableThemes()
- [ ] Create `ThemePackParser.cs` (for XML parsing)
- [ ] Create `ThemePackHandler.cs` (for ZIP operations)

### Testing
- [ ] Test theme loading
- [ ] Test theme validation
- [ ] Test theme application

---

## 📋 Phase 3: SVG Strap System (Week 3)

### SVG Shapes & Effects
- [ ] Design base SVG shapes (Classic, Modern, Rounded, etc.)
- [ ] Create SVG effect filters (sheen, glow, metallic, silk)
- [ ] Implement gradient system

### Renderer (in `MillionaireGame/Graphics/`)
- [ ] Create `SvgStrapRenderer.cs`
  - [ ] RenderStrap()
  - [ ] ApplyEffect()
  - [ ] ConvertToImage()
- [ ] Create effect classes
  - [ ] `SilkSheenEffect.cs`
  - [ ] `MetallicEffect.cs`
  - [ ] `GlowEffect.cs`
  - [ ] `ShadowEffect.cs`

### Testing
- [ ] Test SVG rendering
- [ ] Test effect application
- [ ] Performance testing
- [ ] Memory leak testing

---

## 📋 Phase 4: UI Components (Week 4-5)

### OptionsDialog Integration
- [ ] Modify `OptionsDialog.Designer.cs`
  - [ ] Add `tabThemes` TabPage
  - [ ] Add to `tabControl.Controls`
- [ ] Modify `OptionsDialog.cs`
  - [ ] Add `LoadThemeSettings()` method
  - [ ] Add `SaveThemeSettings()` method
  - [ ] Wire up change tracking
  - [ ] Add `SettingsApplied` event handler

### Theme Settings Panel (in `MillionaireGame/Forms/ThemeSettings/`)
- [ ] Create `ThemeSettingsPanel.cs` user control
  - [ ] Theme selector ComboBox
  - [ ] Component list panel
  - [ ] Settings detail panel
- [ ] Create `BackgroundSettingsPanel.cs`
  - [ ] Image selection
  - [ ] Chroma key settings
  - [ ] Preview thumbnail
- [ ] Create `StrapSettingsPanel.cs`
  - [ ] SVG shape selector
  - [ ] Color pickers
  - [ ] Effect settings
  - [ ] Live preview
- [ ] Create `MoneyTreeSettingsPanel.cs`
  - [ ] Background selection
  - [ ] Color settings
  - [ ] Typography settings

### Testing
- [ ] Test tab navigation
- [ ] Test settings loading
- [ ] Test settings saving
- [ ] Test preview updates

---

## 📋 Phase 5: Integration (Week 6)

### Background System Integration
- [ ] Modify `BroadcastSettings.cs`
  - [ ] Add `ThemeId` field
  - [ ] Preserve `SelectedBackgroundPath`
- [ ] Modify `BackgroundRenderer.cs`
  - [ ] Load theme backgrounds
  - [ ] Handle legacy backgrounds
- [ ] Create migration for existing backgrounds

### Screen Integration
- [ ] Apply theme to TV screen
- [ ] Apply theme straps to questions/answers
- [ ] Apply theme to money tree display

### Testing
- [ ] Test theme application to screens
- [ ] Test legacy background migration
- [ ] Test theme switching
- [ ] Performance testing

---

## 📋 Phase 6: Preset Themes & Assets (Week 7)

### Preset Themes
- [ ] Design "Classic Gold" theme
- [ ] Design "Modern Blue" theme
- [ ] Design "Elegant Red" theme
- [ ] Design "Bold Green" theme
- [ ] Design "Professional Purple" theme
- [ ] Design "Midnight Black" theme

### Assets
- [ ] Create/source background images
- [ ] Generate preview thumbnails
- [ ] Update seed data in migration

### Testing
- [ ] Test all presets
- [ ] Verify visual consistency
- [ ] User feedback

---

## 📋 Phase 7: Testing & Polish (Week 8)

### Testing
- [ ] Unit tests (if applicable)
- [ ] Integration tests
- [ ] UI/UX testing
- [ ] Performance testing
- [ ] Memory leak checks
- [ ] Cross-component consistency
- [ ] User acceptance testing

### Documentation
- [ ] Update user guide
- [ ] Create theme creation tutorial
- [ ] Create theme pack guide
- [ ] Update CHANGELOG.md
- [ ] Code documentation review

### Polish
- [ ] Bug fixes
- [ ] Performance optimizations
- [ ] UI refinements
- [ ] Error handling improvements

---

## 🎯 Definition of Done

- [ ] All 6 presets working
- [ ] 2 user profiles functional
- [ ] Theme pack import/export working
- [ ] SVG straps rendering correctly
- [ ] No memory leaks
- [ ] No performance issues
- [ ] Documentation complete
- [ ] Code reviewed
- [ ] User acceptance passed
- [ ] Ready for merge to master-v1.0.7

---

## 📝 Notes

**Critical Reminders:**
- All repositories go in `MillionaireGame.Core/Database/`
- UI integration is in `OptionsDialog`, not `ControlPanelForm`
- Extend existing `BroadcastSettings`, don't replace
- Follow async/await patterns consistently
- Use `IF NOT EXISTS` in migration scripts
- No `MessageBox` in production code (use `GameConsole`)
- Test migration on clean database before committing

**Next Steps:**
1. Start with Phase 1: Create migration script
2. Implement repositories
3. Test database layer thoroughly before moving to Phase 2

---

**Last Updated:** January 14, 2026  
**Ready to Begin:** Phase 1 - Migration Script
