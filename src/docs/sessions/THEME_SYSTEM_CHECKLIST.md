# Theme System Checklist

Status: snapshot as of 2026-01-17

Completed
- SVG money-tree renderer implemented and integrated.
- Centralized glyph-based vertical centering in `ScalableScreenBase.DrawScaledTextWithOutline()`.
- TV/Host/Guest alignment for money-tree text and flashing behavior standardized.
- Preview NRE fixed when invalidating theme preview cache.
- Theme change propagation wired: `ScreenUpdateService.RefreshThemes()` and `IGameScreen.RefreshTheme()` implemented.
- Classic Black preset added via migration (`00019`), money-tree color update (`00021`), font copy (`00022`), and strap-shape symmetry (`00023`).
- TV strap font scale increased (`TvFontScale = 1.6f`) to improve readability on large displays.
- Session notes added and workspace changes committed.

Remaining / Recommended
- Restart app in staging to run migrations and validate database seeding (if not already done).
- Perform visual QA: capture TV/Host/Guest screenshots for each preset; review for alignment, contrast, and legibility.
- Perform accessibility contrast checks for dark themes (Classic Black, Midnight Black).
- Make `TvFontScale` configurable via settings or theme metadata (UX improvement).
- Add an automated screenshot harness to regularly validate themes across resolutions.
- Remove any test-only UI (e.g., programmatic theme creation buttons) from mainline code.
- Update user-facing docs and release notes describing theme presets and known limitations.
- Add unit/integration tests where feasible for theme-loading and strap rendering paths.

Acceptance Criteria
- All presets render clearly on 80" TVs with default `TvFontScale`.
- Classic Black has readable money-tree text and strap labels (no contrast/regression issues).
- Theme migrations apply cleanly in staging without errors.
- Theme change applies immediately across TV/Host/Guest when selected in settings.
