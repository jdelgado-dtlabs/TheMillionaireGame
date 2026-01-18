# Theming System Checklist (Active)

Updated: 2026-01-17

Purpose: Short actionable checklist for tracking remaining work on the theming system.

- [x] Integrate SVG strap renderer across TV/Host/Guest
- [x] Centralize glyph-based text centering (`ScalableScreenBase.DrawScaledTextWithOutline()`)
- [x] Wire theme-change propagation (`ScreenUpdateService.RefreshThemes()` + `IGameScreen.RefreshTheme()`)
- [x] Seed `Classic Black` preset (migration `00019`)
- [x] Brighten Classic Black money-tree text (migration `00021`)
- [x] Copy Classic Gold fonts to Classic Black (migration `00022`)
- [x] Fix Professional Purple / Midnight Black strap symmetry (migration `00023`)
- [x] Increase TV strap font scale to improve legibility (`TvFontScale = 1.6f` in `TVScreenForm`)

Remaining / Recommended:

- [ ] Run automated Theme QA: capture TV/Host/Guest screenshots for all built-in presets and review
- [ ] Expose `TvFontScale` as a user-configurable setting in the Themes UI (OptionsDialog)
- [ ] Add accessibility contrast validation tests for presets (WCAG-based checks)
- [ ] Add unit/integration tests for `SvgStrapRenderer` and `SvgMoneyTreeRenderer`
- [ ] Implement ThemePack import/export UI and documentation
- [ ] Performance profiling on large-screen render paths (SVG rasterization and caching)

Owner: Theme subsystem team
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

## ✅ Phase 1: Database & Repository Layer (Week 1) - COMPLETE

### Migration Script
- [x] Create `00008_create_theme_tables.sql` in `src/MillionaireGame/Database/Migrations/`
- [x] Define `Themes` table
- [x] Define `ThemeBackgrounds` table
- [x] Define `ThemeStraps` table
- [x] Define `ThemeMoneyTree` table
- [x] Define `ThemePacks` table (Note: ThemePresets merged into Themes with ThemeType='Preset')
- [x] Add indexes for performance
- [x] Seed 6 built-in preset themes
- [x] Test migration runs successfully

### Repositories (in `MillionaireGame.Core/Database/`)
- [x] Create `ThemeRepository.cs`
  - [x] GetActiveThemeAsync()
  - [x] GetThemeByIdAsync()
  - [x] GetAllThemesAsync()
  - [x] SaveThemeAsync()
  - [x] SetActiveThemeAsync()
  - [x] DeleteThemeAsync()
  - [x] GetThemesByTypeAsync()
- [x] Create `ThemeBackgroundRepository.cs`
  - [x] GetBackgroundsByThemeIdAsync()
  - [x] SaveBackgroundAsync()
  - [x] DeleteBackgroundAsync()
- [x] Create `ThemeStrapRepository.cs`
  - [x] GetStrapsByThemeIdAsync()
  - [x] SaveStrapAsync()
  - [x] DeleteStrapAsync()
- [x] Create `ThemeMoneyTreeRepository.cs`
  - [x] GetMoneyTreeByThemeIdAsync()
  - [x] SaveMoneyTreeAsync()
- [x] Create `ThemePackRepository.cs`
  - [x] GetAllPacksAsync()
  - [x] GetPackByIdAsync()
  - [x] SavePackAsync()
  - [x] DeletePackAsync()

### Testing
- [x] Run application to trigger migration
- [x] Verify all tables created
- [x] Verify preset themes loaded
- [x] Test repository CRUD operations

**Status:** ✅ Complete - Commit 2d0220a (January 14, 2026)

---

## ✅ Phase 2: Core Services & Models (Week 2) - COMPLETE

### Models (in `MillionaireGame.Core/Models/`)
- [x] Create `Theme.cs`
- [x] Create `ThemeBackground.cs`
- [x] Create `ThemeStrap.cs`
- [x] Create `ThemeMoneyTree.cs`
- [x] Create `ThemePack.cs`
- [x] Create `CompleteTheme.cs` (composite model)

### Services (in `MillionaireGame.Core/Services/`)
- [x] Create `ThemeService.cs`
  - [x] LoadActiveThemeAsync()
  - [x] GetCompleteThemeAsync()
  - [x] ApplyThemeAsync()
  - [x] GetAllThemesAsync()
  - [x] GetThemesByTypeAsync()
  - [x] SaveCompleteThemeAsync()
  - [x] DuplicateThemeAsync()
  - [x] DeleteThemeAsync()
- [x] Create `ThemePackParser.cs` (for XML parsing)
- [x] Create `ThemePackHandler.cs` (for ZIP operations)

### Testing
- [x] Test theme loading
- [x] Test theme validation (implicit in service methods)
- [x] Test theme application

**Status:** ✅ Complete - Commit 4facd1f (January 14, 2026)

---

## ✅ Phase 3: SVG Strap System (Week 3) - COMPLETE

### SVG Shapes & Effects
- [x] Design base SVG shapes (Classic, Modern, Rounded, Sharp, etc.)
- [x] Create SVG effect filters (sheen, glow, metallic, silk, glass, shadow)
- [x] Implement gradient system

### Renderer (in `MillionaireGame.Core/Graphics/`)
- [x] Create `SvgStrapRenderer.cs`
  - [x] RenderStrap()
  - [x] ApplyEffect()
  - [x] RenderToImage() (for preview)
- [x] Create `StrapShapes.cs` (SVG shape definitions)
- [x] Create `StrapEffects.cs` (SVG effect definitions)
- [x] Create `StrapPreviewControl.cs` (UI preview component)

### Testing
- [x] Test SVG rendering (via preview control)
- [x] Test effect application
- [x] Performance testing (deferred to Phase 7)
- [x] Memory leak testing (deferred to Phase 7)

**Status:** ✅ Complete - Commit 8f08aff (January 14, 2026)

---

## ✅ Phase 4: UI Components (Week 4-5) - COMPLETE

### OptionsDialog Integration
- [x] Modify `OptionsDialog.Designer.cs`
  - [x] Add `tabThemes` TabPage
  - [x] Add to `tabControl.Controls`
- [x] Modify `OptionsDialog.cs`
  - [x] Add `LoadThemeSettings()` method
  - [x] Add `InitializeThemeSettingsPanel()` method
  - [x] Wire up change tracking (_hasChanges)
  - [x] Add `SettingsApplied` event handler

### Theme Settings Panel (in `MillionaireGame/Controls/`)
- [x] Create `ThemeSettingsPanel.cs` user control
  - [x] Theme selector ComboBox
  - [x] Strap preview panel (StrapPreviewControl)
  - [x] Theme action buttons (Apply, Duplicate, Delete)
  - [x] Color pickers (primary, strap, text)
  - [x] Effect controls (type, intensity)
  - [x] Money tree color settings
- [x] Create `ThemeSettingsPanel.Designer.cs`
  - [x] Complete UI layout with all controls
- [x] Integrate with OptionsDialog Themes tab

**Note:** Phase 4 implemented a single comprehensive `ThemeSettingsPanel` instead of separate panels for Background/Strap/MoneyTree. All settings are consolidated in one intuitive interface.

### Testing
- [x] Test tab navigation
- [x] Test settings loading (async)
- [x] Test settings saving (Apply button)
- [x] Test preview updates (real-time)
- [x] Test theme duplication
- [x] Test theme deletion

**Status:** ✅ Complete - Commits a8f2240, d23fbdc, 126f8eb (January 14, 2026)

---

## ✅ Phase 5: Integration (Week 6) - COMPLETE

### Background System Integration
- [x] Modify `BackgroundRenderer.cs`
  - [x] Add `ThemeService` parameter to constructor (optional, maintains backward compatibility)
  - [x] Load theme backgrounds via `GetCompleteThemeAsync()`
  - [x] Implement fallback chain: theme → legacy path → black
  - [x] Handle legacy backgrounds seamlessly
- [x] Modify `TVScreenForm.cs`
  - [x] Initialize `ThemeService` with connection string
  - [x] Load active theme on startup (async)
  - [x] Pass `ThemeService` to `BackgroundRenderer`
  - [x] Add `RefreshTheme()` method for live updates

### Screen Integration
- [x] Apply theme to TV screen background
- [x] Theme straps integration (deferred to future phase - straps currently use legacy system)
- [x] Money tree theming (deferred to future phase - money tree currently uses legacy colors)
- [x] Multi-screen theme propagation
  - [x] Create `RefreshThemes()` in `ScreenUpdateService`
  - [x] Wire up `ControlPanelForm.SettingsApplied` to refresh all screens

### Testing
- [x] Test theme application to TV screen
- [x] Test legacy background compatibility
- [x] Test theme switching (immediate application)
- [x] Performance testing (async loading, cache clearing)
- [x] Multi-screen propagation verification
- [x] Build verification (0 errors, 7 warnings)

**Status:** ✅ Complete - Commit e037c47 (January 15, 2026)

---

## ✅ Phase 6: Preset Themes & Assets (Week 7) - COMPLETE

### Preset Themes
- [x] Design "Classic Gold" theme (default)
  - [x] Primary: #8B4513/#D4AF37 (Saddle Brown/Gold)
  - [x] Effect: Silk
  - [x] Money tree: $1-$500 White, $1K-$32K Gold, $64K-$125K Blue, $250K-$1M Orange
- [x] Design "Modern Blue" theme
  - [x] Primary: #0047AB/#87CEEB (Cobalt/Sky Blue)
  - [x] Effect: Glass
  - [x] Money tree: $1-$500 White, $1K-$32K Light Blue, $64K-$125K Blue, $250K-$1M Gold
- [x] Design "Elegant Red" theme
  - [x] Primary: #8B0000/#FFD700 (Dark Red/Gold)
  - [x] Effect: Metallic
  - [x] Money tree: $1-$500 White, $1K-$32K Gold, $64K-$125K Red, $250K-$1M Dark Red
- [x] Design "Bold Green" theme
  - [x] Primary: #006400/#90EE90 (Dark Green/Light Green)
  - [x] Effect: Glow
  - [x] Money tree: $1-$500 White, $1K-$32K Light Green, $64K-$125K Green, $250K-$1M Gold
- [x] Design "Professional Purple" theme
  - [x] Primary: #4B0082/#C0C0C0 (Indigo/Silver)
  - [x] Effect: Silk
  - [x] Money tree: $1-$500 White, $1K-$32K Silver, $64K-$125K Purple, $250K-$1M Gold
- [x] Design "Midnight Black" theme
  - [x] Primary: #000000/#FFD700 (Black/Gold)
  - [x] Effect: Metallic
  - [x] Money tree: $1-$500 White, $1K-$32K Silver, $64K-$125K Gold, $250K-$1M Black

### Assets
- [x] Database seed data completed in migration 00008
- [x] All themes include complete specifications (metadata, backgrounds, straps, money tree colors)
- [x] Preview thumbnail generation (handled by StrapPreviewControl dynamically)

### Testing
- [x] All presets seeded in database
- [x] Visual consistency verified via ThemeSettingsPanel preview
- [x] Theme selection and application tested

**Status:** ✅ Complete - Migration 00008 (January 11, 2026), Documentation commit c9bd884 (January 15, 2026)

---

## ✅ Phase 7.1: Comprehensive Strap Enhancements - COMPLETE

### Visual Enhancements
- [x] Increased border width from 2px to 4px on all straps
- [x] Changed effect type from 'Shadow' to 'Outline' for clearer appearance
- [x] Added white outline effect at 40% intensity (reduced from 80%)
- [x] Applied to all 6 preset themes (18 straps total)

### Text Readability
- [x] Added 2px black outlines to all text rendering
  - [x] Question text with `DrawScaledTextWithWrapAndOutline()`
  - [x] Answer labels (A:, B:, C:, D:) with `DrawScaledTextWithOutline()`
  - [x] Answer text with `DrawScaledTextWithWrapAndOutline()`
  - [x] Winnings display with `DrawScaledTextWithOutline()`

### Font Corrections
- [x] Fixed Classic Gold theme fonts
  - [x] Question strap: "Copperplate Gothic Bold"
  - [x] Answer strap: "Copperplate Gothic Bold"
  - [x] AnswerLabel strap: "Arial"

### SVG Conversion Complete
- [x] Converted winnings strap from PNG to SVG rendering
- [x] All straps now use theme fonts (no hardcoded fonts)
- [x] Eliminated all PNG dependencies for straps
- [x] Maintained backward compatibility with PNG fallback

### Database Migrations
- [x] Migration 00014: Rollback partial changes from failed migration
- [x] Migration 00017: Comprehensive enhancements
  - [x] Dynamic StrapType constraint lookup (add AnswerLabel)
  - [x] Fixed EffectType constraint to match code (Outline/3D/Emboss; removed Silk/Metallic/Glass)
  - [x] INSERT AnswerLabel straps with final values (no redundant UPDATEs)
  - [x] UPDATE Question/Answer straps with borders/outline + Classic Gold font fixes

### Code Quality
- [x] Added `DrawScaledTextWithOutline()` to ScalableScreenBase
- [x] Added `DrawScaledTextWithWrapAndOutline()` to TVScreenForm
- [x] Reusable text outline rendering methods
- [x] Clean abstraction with proper theme font usage

**Status:** ✅ Complete - Commit d897c28 (January 16, 2026)  
**Documentation:** [Session 2026-01-16 Strap Enhancements Complete](../sessions/2026-01-16_strap_enhancements_complete.md)

---

## 📋 Phase 7.2: Money Tree SVG Conversion & Theme Integration - IN PROGRESS

**Goal:** Convert money tree from PNG graphics to SVG rendering with full theme color integration.

### Current State Analysis
- **Database:** `ThemeMoneyTree` table exists with color configurations for all 6 preset themes ✅
- **Repository:** `ThemeMoneyTreeRepository` complete with CRUD operations ✅
- **Service:** `ThemeService` loads money tree data into `CompleteTheme` objects ✅
- **Rendering:** ~~Currently uses PNG assets~~ NOW USES SVG! ✅
- **Text:** ~~Hardcoded "Copperplate Gothic Bold" font and Color.Gold/White~~ NOW USES THEME FONTS AND COLORS! ✅

### Implementation Tasks
- [x] Create SVG money tree shape generator
  - [x] Design money tree SVG shape (vertical ladder with 15 levels)
  - [x] Level highlight/position indicator system
  - [x] Safety net level visual differentiation
  - [x] Current level highlight animation support
- [x] Apply theme colors from `ThemeMoneyTree` table
  - [x] InactiveColor (future levels not yet reached)
  - [x] ActiveColor (current level highlight)
  - [x] CompletedColor (levels already passed)
  - [x] SafeHavenColor (safety net levels)
  - [x] HighlightColor (glow/pulsing effect)
- [x] Replace PNG rendering in `DrawMoneyTreeGraphical()`
  - [x] Remove `TextureManager.GetTexture()` calls for MoneyTreeBase
  - [x] Remove PNG position overlay system
  - [x] Implement SVG rendering with `_activeTheme.MoneyTree` colors
  - [x] Maintain slide-in animation from right
- [x] Update text rendering in `DrawMoneyTreeText()`
  - [x] Use theme fonts instead of hardcoded "Copperplate Gothic Bold"
  - [x] Apply `ThemeMoneyTree` font properties (FontFamily, FontSize, FontBold)
  - [x] Apply theme colors for different level states
  - [x] Add text outline support (consistent with straps)
- [x] Host/Guest screen integration
  - [x] Update `HostScreenForm` money tree rendering
  - [x] Update `GuestScreenForm` money tree rendering
  - [x] Ensure multi-screen consistency
- [ ] Theme preview updates
  - [ ] Add money tree preview to `ThemeSettingsPanel`
  - [ ] Show color tier samples in preview
  - [ ] Update preview when colors change

### Testing & Validation
- [ ] Build verification (0 errors) ✅ PASSED
- [ ] All 6 preset themes render correctly with proper colors
- [ ] Safety net levels display with correct colors
- [ ] Current level highlight animation works
- [ ] Text readability with theme colors and outlines
- [ ] Multi-screen theme propagation (TV, Host, Guest)
- [ ] Theme switching applies money tree colors immediately
- [ ] PNG fallback for backward compatibility (if needed)

### Documentation
- [ ] Update session document for Phase 7.2
- [ ] Document `ThemeMoneyTree` color properties
- [ ] SVG rendering technical documentation
- [ ] Update CHANGELOG.md

**Priority:** High - Next phase after Phase 7.1  
**Estimated Scope:** Medium (SVG shape design + color integration)  
**Status:** 🚧 IN PROGRESS - SVG renderer implemented, all screens updated, testing pending

---

## 📋 Phase 7.3: Testing & Polish - OPTIONAL/FUTURE

**Note:** Phase 7.3 represents future enhancements and optimizations. Core theming functionality is complete in Phases 1-6, enhanced in Phase 7.1-7.2.

### Testing (Deferred)
- [ ] Comprehensive unit test suite
- [ ] Integration test scenarios
- [ ] UI/UX usability study
- [ ] SVG rendering performance benchmarking (straps + money tree)
- [ ] Memory profiling and leak detection
- [ ] Cross-component consistency audit
- [ ] User acceptance testing with real users

### Documentation (Partially Complete)
- [x] Internal technical documentation (session documents)
- [x] Migration guide (migrations 00008, 00014, 00017)
- [x] Code documentation (inline comments)
- [ ] User-facing theme creation tutorial
- [ ] Theme pack XML format specification
- [ ] CHANGELOG.md update for theming feature

### Polish (Future Enhancements)
- [ ] Bug fixes from user feedback
- [ ] SVG rendering performance optimizations
- [ ] UI/UX refinements based on usage patterns
- [ ] Enhanced error handling for edge cases
- [ ] Accessibility improvements (color contrast, keyboard navigation)
- [ ] Theme animation effects (transitions, fades)
- [ ] Custom effect types beyond Outline/Glow/Shadow

---

## 🎯 Definition of Done

### ✅ Core Functionality (Complete)
- [x] All 6 preset themes working (Classic Gold, Modern Blue, Elegant Red, Bold Green, Professional Purple, Midnight Black)
- [x] Theme CRUD operations functional (Create via duplication, Read, Update, Delete)
- [x] Theme application to TV screen backgrounds
- [x] Live theme preview in settings panel
- [x] Multi-screen theme propagation
- [x] Database persistence layer complete
- [x] SVG strap rendering with theme fonts and colors (Phase 7.1)
- [x] Text outlines for improved readability (Phase 7.1)
- [x] Enhanced borders and outline effects (Phase 7.1)

### 🚧 Extended Features (Partial/Future)
- [x] Theme metadata (Name, Description, Author, Version, IsDefault, IsReadOnly)
- [x] Theme components (Backgrounds, Straps, MoneyTree data structures)
- [x] SVG strap rendering fully integrated (Phase 7.1)
- [ ] Money tree SVG rendering with theme colors (Phase 7.2 - next priority)
- [ ] Theme pack import/export (XML format - ThemePackParser/Handler created but not integrated into UI)
- [ ] User profile association (schema supports, but UI not implemented)

### 📊 Success Criteria
- [x] **Build Status:** Clean build (0 errors, warnings acceptable for nullable references)
- [x] **Database:** All tables created, seeded with 6 presets
- [x] **UI:** Settings panel fully functional with live preview
- [x] **Integration:** Background system uses themes, legacy compatibility maintained
- [x] **Performance:** Async loading, responsive UI
- [x] **Code Quality:** Repository pattern, service layer, separation of concerns

### 🔮 Future Work
- **Money Tree Integration (Phase 7.2 - NEXT):** Convert PNG graphics to SVG rendering with theme colors
- **Theme Packs:** Complete UI for XML import/export functionality
- **User Profiles:** Create profile management UI, associate themes with profiles
- **Performance:** SVG caching, memory optimization, benchmarking
- **Testing:** Unit tests, integration tests, performance tests
- **Documentation:** End-user guide, theme creation tutorial

**Overall Status:** ✅ **Phases 1-6 Complete, Phase 7.1 Complete** - Core theming system fully functional with enhanced straps, Phase 7.2 (Money Tree SVG) is next priority

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
