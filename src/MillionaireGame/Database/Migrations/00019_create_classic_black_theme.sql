-- ============================================================================
-- Migration: 00019_create_classic_black_theme
-- Description: Seed a new preset theme "Classic Black" which mirrors Classic Gold
--              styling but uses a dark grey / black gradient for straps to match
--              legacy PNG strap appearance. Adds Theme, Backgrounds, Straps,
--              and MoneyTree entries. Safe to run idempotently.
-- Author: Development Team
-- Date: 2026-01-17
-- ============================================================================

-- Only insert if a theme named 'Classic Black' does not already exist
IF NOT EXISTS (SELECT 1 FROM Themes WHERE ThemeName = 'Classic Black')
BEGIN
    PRINT 'Seeding preset theme: Classic Black';

    INSERT INTO Themes (ThemeName, ThemeType, IsActive, Description, Author, Version)
    VALUES ('Classic Black', 'Preset', 0, 'Classic strap shapes with dark grey/black gradient (PNG look-alike)', 'MillionaireGame Team', '1.0.0');

    DECLARE @ClassicBlackId INT = SCOPE_IDENTITY();

    -- TV Screen Background (reuse existing dark background if available)
    INSERT INTO ThemeBackgrounds (ThemeId, ComponentType, ImagePath, ScaleMode)
    VALUES (@ClassicBlackId, 'TVScreen', 'embedded://background6.png', 'Fill');

    -- Money Tree Background (reuse default money tree bg)
    INSERT INTO ThemeBackgrounds (ThemeId, ComponentType, ImagePath, ScaleMode)
    VALUES (@ClassicBlackId, 'MoneyTree', 'embedded://moneytree_bg.png', 'Fill');

    -- Question Strap (dark grey -> lighter grey gradient, black border)
    INSERT INTO ThemeStraps (
        ThemeId, StrapType, SvgShape, PrimaryColor, SecondaryColor, GradientEnabled, GradientAngle,
        EffectType, EffectIntensity, EffectColor, BorderEnabled, BorderColor, BorderWidth, FontFamily, FontSize, FontColor, FontBold)
    VALUES (
        @ClassicBlackId, 'Question', 'Classic', '#1f1f1f', '#4b4b4b', 1, 90,
        'Outline', 60, '#000000', 1, '#000000', 2, 'Times New Roman', 24, '#FFFFFF', 1);

    -- Answer Strap (same dark palette, slightly smaller font)
    INSERT INTO ThemeStraps (
        ThemeId, StrapType, SvgShape, PrimaryColor, SecondaryColor, GradientEnabled, GradientAngle,
        EffectType, EffectIntensity, EffectColor, BorderEnabled, BorderColor, BorderWidth, FontFamily, FontSize, FontColor, FontBold)
    VALUES (
        @ClassicBlackId, 'Answer', 'Classic', '#1f1f1f', '#4b4b4b', 1, 90,
        'Outline', 60, '#000000', 1, '#000000', 2, 'Arial', 22, '#FFFFFF', 1);

    -- Money Tree colors: dark inactive/active, gold highlight retained for readability
    INSERT INTO ThemeMoneyTree (
        ThemeId, InactiveColor, ActiveColor, CompletedColor, SafeHavenColor,
        HighlightEnabled, HighlightType, HighlightColor, FontFamily, FontSize, FontBold)
    VALUES (
        @ClassicBlackId, '#404040', '#2b2b2b', '#FFFFFF', '#666666',
        1, 'PulsingGlow', '#FFD700', 'Arial Bold', 18, 1);

    PRINT 'Seeded: Classic Black';
END
ELSE
BEGIN
    PRINT 'Classic Black theme already exists - skipping seed.';
END

PRINT 'Migration 00019_create_classic_black_theme completed.';
GO
