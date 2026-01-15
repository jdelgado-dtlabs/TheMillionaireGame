# Theming System Feature Branch Setup - Session Summary

**Date:** January 14, 2026  
**Branch:** `feature/theming-system` (created from `master-v1.0.7`)  
**Status:** Planning Complete, Ready for Implementation

---

## Session Objectives

1. ✅ Create feature branch from master-v1.0.7
2. ✅ Review THEMING_SYSTEM_PLAN.md
3. ✅ Analyze current codebase structure
4. ✅ Identify integration points and patterns
5. ✅ Update plan with codebase-specific requirements

---

## Key Findings from Codebase Review

### 1. Settings Architecture (CRITICAL CHANGE)

**FINDING:** Settings are stored in SQL Server database, NOT XML files.

- **Storage:** `ApplicationSettings` table with key-value pairs
- **Repository:** `ApplicationSettingsRepository` in `MillionaireGame.Core/Database/`
- **Manager:** `ApplicationSettingsManager` handles lifecycle
- **Pattern:** Nested settings objects (like `BroadcastSettings`) are flattened to key-value pairs

**IMPACT ON PLAN:**
- Theme definitions will use separate database tables (as planned)
- But must integrate with existing `ApplicationSettingsRepository` pattern
- No XML serialization needed (despite `[XmlElement]` attributes in code)

### 2. Database Migration System

**PATTERN:**
- Migrations in `src/MillionaireGame/Database/Migrations/`
- Format: `NNNNN_descriptive_name.sql` (e.g., `00008_create_theme_tables.sql`)
- Latest: `00007_add_telemetry_tables.sql`
- Next: `00008_create_theme_tables.sql`
- Embedded resources, run automatically on startup
- Must be idempotent (use `IF NOT EXISTS`)

**ACTION:**
- Create migration `00008_create_theme_tables.sql` for Phase 1
- Follow template in `Migrations/README.md`

### 3. UI Architecture

**FINDING:** Theme settings go in `OptionsDialog`, NOT `ControlPanelForm`

- **Location:** Game Menu → Settings → new "Themes" tab
- **Structure:** `TabControl` with multiple `TabPage` objects
- **Existing Tabs:** Broadcast, Screens, Lifelines, Money Tree, Sounds, Stream Deck, Audience
- **Pattern:** Lazy initialization, change tracking with `MarkChanged()`
- **Events:** `SettingsApplied` event when Apply/OK clicked

**IMPACT ON PLAN:**
- Original plan said "Control Panel → Broadcast Tab → Theme"
- CORRECTED to: "Game Menu → Settings → Themes Tab"
- Must follow existing `OptionsDialog` patterns
- Integration is simpler than originally thought

### 4. Repository Location

**FINDING:** Repositories belong in `MillionaireGame.Core/Database/`

- NOT in main `MillionaireGame` project
- Core project contains shared data access logic
- Main project contains UI and application-specific services

**ACTION:**
- All theme repositories go in `MillionaireGame.Core/Database/`
- Follow pattern from `ApplicationSettingsRepository.cs`

### 5. Existing Background System

**CRITICAL INTEGRATION POINT:**

- `BroadcastSettings` class exists in `MillionaireGame.Core/Settings/`
- `BackgroundRenderer.cs` exists in `MillionaireGame/Graphics/`
- Already supports:
  - Prerendered backgrounds
  - Chroma key mode
  - Background selection UI in OptionsDialog Broadcast tab
- Stored via `ApplicationSettingsRepository`

**IMPACT ON PLAN:**
- Theme backgrounds should EXTEND, not replace, current system
- Add `ThemeId` field to `BroadcastSettings` to link to active theme
- Preserve `SelectedBackgroundPath` for backward compatibility
- Migration: Convert existing backgrounds to "Legacy" theme entries

---

## Plan Updates Made

### 1. Phase 1: Database & Repository Layer

**UPDATED:**
- ✅ Corrected repository location to `MillionaireGame.Core/Database/`
- ✅ Added migration script naming convention
- ✅ Removed unit test requirement (project doesn't have extensive tests)
- ✅ Added note about following `ApplicationSettingsRepository` pattern

### 2. Phase 2: Core Services & Models

**UPDATED:**
- ✅ Service location: `MillionaireGame.Core/Services/`
- ✅ Follow `MoneyTreeService.cs` pattern
- ✅ Use dependency injection compatible patterns

### 3. Phase 4: UI Components

**UPDATED:**
- ✅ Changed integration point from ControlPanelForm to OptionsDialog
- ✅ Added TabPage to existing TabControl
- ✅ Follow existing tab initialization patterns
- ✅ Use `MarkChanged()` for change tracking

### 4. Phase 5: Integration

**UPDATED:**
- ✅ Added critical note about `BackgroundRenderer` and `BroadcastSettings` integration
- ✅ Added migration path for existing background configurations
- ✅ Clarified settings changes occur in OptionsDialog, not ControlPanelForm

### 5. New Section: Implementation Notes

**ADDED:**
- 📝 Complete codebase findings documentation
- 📝 Key integration points identified
- 📝 Storage decision rationale (separate tables vs. JSON)
- 📝 Code examples for following existing patterns
- 📝 Critical integration points with existing background system

---

## Critical Architectural Decisions

### Decision 1: Theme Storage

**OPTIONS:**
- A) Separate database tables (as planned)
- B) JSON in ApplicationSettings table

**DECISION:** Option A (Separate Tables)

**RATIONALE:**
- Better data integrity with foreign keys
- Easier to query and manage
- Supports theme packs with asset references
- Allows for complex theme configurations
- Aligns with telemetry tables pattern (see `00007_add_telemetry_tables.sql`)

### Decision 2: Background System Integration

**APPROACH:** Extend, Don't Replace

**STRATEGY:**
1. Add `ThemeId INT NULL` to `BroadcastSettings`
2. Keep existing `SelectedBackgroundPath` field
3. If `ThemeId` is set, use theme background; otherwise use legacy path
4. Migration creates "Legacy" theme for existing custom backgrounds

### Decision 3: UI Location

**APPROACH:** OptionsDialog Tab (Not ControlPanelForm)

**RATIONALE:**
- Settings naturally belong in Settings dialog
- ControlPanelForm is for game control, not configuration
- Follows existing pattern (all visual settings in OptionsDialog)
- Simpler implementation (reuse existing tab infrastructure)

---

## Next Steps for Implementation

### Phase 1: Database & Repository (Start Here)

1. **Create Migration Script**
   - File: `src/MillionaireGame/Database/Migrations/00008_create_theme_tables.sql`
   - Include all tables: Themes, ThemeBackgrounds, ThemeStraps, ThemeMoneyTree, ThemePresets, ThemePacks
   - Add indexes for performance
   - Seed built-in preset themes

2. **Implement Repositories**
   - `ThemeRepository.cs` in `MillionaireGame.Core/Database/`
   - `ThemeBackgroundRepository.cs`
   - `ThemeStrapRepository.cs`
   - `ThemeMoneyTreeRepository.cs`
   - `ThemePresetRepository.cs`
   - `ThemePackRepository.cs`
   - Follow async/await pattern from `ApplicationSettingsRepository`

3. **Test Migration**
   - Run application to trigger migration
   - Verify tables created successfully
   - Verify preset themes seeded

### Phase 2: Core Services & Models

4. **Create Model Classes**
   - `Theme.cs` in `MillionaireGame.Core/Models/`
   - `ThemeBackground.cs`
   - `ThemeStrap.cs`
   - `ThemeMoneyTree.cs`
   - Use properties that map to database columns

5. **Implement ThemeService**
   - `ThemeService.cs` in `MillionaireGame.Core/Services/`
   - Business logic for theme loading, validation, application
   - Follow `MoneyTreeService` pattern

### Phase 3: SVG Strap System

6. **Design SVG Shapes**
   - Create base SVG templates
   - Implement effect filters (sheen, glow, metallic, etc.)

7. **Create Renderer**
   - `SvgStrapRenderer.cs`
   - Convert SVG to Image for display

### Phase 4: UI Components

8. **Add Themes Tab**
   - Modify `OptionsDialog.Designer.cs`
   - Add `tabThemes` TabPage

9. **Create Theme Panel**
   - `ThemeSettingsPanel.cs` user control
   - Theme selector ComboBox
   - Component settings panels

### Phase 5: Integration

10. **Integrate with BackgroundRenderer**
    - Extend `BackgroundRenderer.cs` to use themes
    - Update `BroadcastSettings` with `ThemeId`
    - Create migration for existing backgrounds

11. **Wire Up Events**
    - OptionsDialog change tracking
    - Settings applied event
    - Screen updates when theme changes

---

## Files to Create

### Database
- [ ] `src/MillionaireGame/Database/Migrations/00008_create_theme_tables.sql`

### Core Project (MillionaireGame.Core)
- [ ] `Database/ThemeRepository.cs`
- [ ] `Database/ThemeBackgroundRepository.cs`
- [ ] `Database/ThemeStrapRepository.cs`
- [ ] `Database/ThemeMoneyTreeRepository.cs`
- [ ] `Database/ThemePresetRepository.cs`
- [ ] `Database/ThemePackRepository.cs`
- [ ] `Models/Theme.cs`
- [ ] `Models/ThemeBackground.cs`
- [ ] `Models/ThemeStrap.cs`
- [ ] `Models/ThemeMoneyTree.cs`
- [ ] `Services/ThemeService.cs`

### Main Project (MillionaireGame)
- [ ] `Graphics/SvgStrapRenderer.cs`
- [ ] `Graphics/Effects/SilkSheenEffect.cs`
- [ ] `Graphics/Effects/MetallicEffect.cs`
- [ ] `Graphics/Effects/GlowEffect.cs`
- [ ] `Forms/ThemeSettings/ThemeSettingsPanel.cs` (user control)
- [ ] `Forms/ThemeSettings/BackgroundSettingsPanel.cs`
- [ ] `Forms/ThemeSettings/StrapSettingsPanel.cs`
- [ ] `Forms/ThemeSettings/MoneyTreeSettingsPanel.cs`

### Files to Modify
- [ ] `src/MillionaireGame/Forms/Options/OptionsDialog.Designer.cs` (add Themes tab)
- [ ] `src/MillionaireGame/Forms/Options/OptionsDialog.cs` (theme settings logic)
- [ ] `src/MillionaireGame/Graphics/BackgroundRenderer.cs` (theme integration)
- [ ] `src/MillionaireGame.Core/Settings/BroadcastSettings.cs` (add ThemeId field)
- [ ] `src/MillionaireGame.Core/Database/ApplicationSettingsRepository.cs` (theme settings support)

---

## Testing Plan

### Unit Tests (Optional)
- Repository CRUD operations
- Theme validation logic
- SVG rendering output

### Integration Tests
- Theme loading from database
- Theme application to screens
- Settings persistence
- Migration from legacy backgrounds

### Manual Tests
- Create custom theme
- Switch between presets
- Import/export theme pack
- Performance with complex SVG straps
- Memory leak testing (SVG rendering)

---

## Risk Mitigation

| Risk | Mitigation Strategy |
|------|-------------------|
| SVG rendering performance | Implement caching, async rendering, preview debouncing |
| Database migration failures | Idempotent SQL, thorough testing, backup instructions |
| Breaking existing backgrounds | Preserve backward compatibility, create legacy migration |
| Complex UI overwhelming users | Start with presets, hide advanced options initially |
| Memory leaks from SVG | Proper IDisposable implementation, memory profiling |

---

## Success Criteria

### Phase 1 Complete When:
- ✅ Migration script runs successfully
- ✅ All theme tables created
- ✅ Preset themes seeded in database
- ✅ All repositories implemented and functional

### Overall Project Complete When:
- ✅ All 6 built-in presets working
- ✅ 2 user profiles can be saved/loaded
- ✅ Theme pack import/export functional
- ✅ SVG straps render with all effects
- ✅ Themes persist across restarts
- ✅ No memory leaks or performance issues
- ✅ Documentation complete
- ✅ User acceptance testing passed

---

## Documentation Updates Needed

### During Development
- [ ] Update THEMING_SYSTEM_PLAN.md as implementation progresses
- [ ] Document any deviations from plan with rationale
- [ ] Add inline code comments for complex logic
- [ ] Update CHANGELOG.md with feature progress

### Before Release
- [ ] User guide for theme system
- [ ] Theme creation tutorial
- [ ] Theme pack creation guide
- [ ] API documentation for ThemeService
- [ ] Migration guide for existing users

---

## Conclusion

The feature branch `feature/theming-system` is now ready for implementation. The plan has been updated to align with the actual codebase architecture, and clear implementation steps have been identified.

**Key Takeaways:**
1. Database-first approach (no XML files)
2. OptionsDialog integration (not ControlPanelForm)
3. Extend existing background system (don't replace)
4. Follow established patterns in the codebase
5. Start with Phase 1 (database migration and repositories)

**Ready to Begin:** Phase 1 - Database & Repository Layer

**Estimated Timeline:** 8 weeks (as per original plan)

---

**Next Session:** Begin implementing migration script `00008_create_theme_tables.sql`
