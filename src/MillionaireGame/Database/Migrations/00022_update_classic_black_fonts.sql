-- ============================================================================
-- Migration: 00022_update_classic_black_fonts
-- Description: Copy all font settings from the preset "Classic Gold" to the
--              preset "Classic Black" so Classic Black uses identical fonts
--              across `ThemeStraps` and `ThemeMoneyTree`.
-- Author: Automated Patch
-- Date: 2026-01-17
-- ============================================================================

-- Ensure both source and target themes exist
IF EXISTS (SELECT 1 FROM Themes WHERE ThemeName = 'Classic Gold')
   AND EXISTS (SELECT 1 FROM Themes WHERE ThemeName = 'Classic Black')
BEGIN
    PRINT 'Copying font settings from Classic Gold to Classic Black...';

    DECLARE @GoldId INT = (SELECT ThemeId FROM Themes WHERE ThemeName = 'Classic Gold');
    DECLARE @BlackId INT = (SELECT ThemeId FROM Themes WHERE ThemeName = 'Classic Black');

    -- Update strap font properties by matching StrapType
    UPDATE tb
    SET tb.FontFamily = src.FontFamily,
        tb.FontSize   = src.FontSize,
        tb.FontColor  = src.FontColor,
        tb.FontBold   = src.FontBold,
        tb.FontItalic = src.FontItalic
    FROM ThemeStraps tb
    INNER JOIN ThemeStraps src ON src.StrapType = tb.StrapType AND src.ThemeId = @GoldId
    WHERE tb.ThemeId = @BlackId;

    -- Update the money tree typography
    IF EXISTS (SELECT 1 FROM ThemeMoneyTree WHERE ThemeId = @GoldId)
    BEGIN
        UPDATE ttb
        SET ttb.FontFamily = src.FontFamily,
            ttb.FontSize   = src.FontSize,
            ttb.FontBold   = src.FontBold
        FROM ThemeMoneyTree ttb
        INNER JOIN ThemeMoneyTree src ON src.ThemeId = @GoldId
        WHERE ttb.ThemeId = @BlackId;

        PRINT 'Updated ThemeMoneyTree fonts.';
    END
    ELSE
    BEGIN
        PRINT 'No ThemeMoneyTree entry for Classic Gold; skipping money tree font copy.';
    END

    PRINT 'Updated ThemeStraps fonts for Classic Black.';
END
ELSE
BEGIN
    PRINT 'Classic Gold or Classic Black theme not present; skipping migration 00022.';
END

PRINT 'Migration 00022_update_classic_black_fonts completed.';
GO
