-- ============================================================================
-- Migration: 00021_update_classic_black_moneytree_text_color
-- Description: Brighten the money-tree text colors for the preset "Classic Black"
--              to improve legibility on dark straps. This version avoids
--              referencing non-existent columns and is safe/idempotent.
-- Author: Automated Patch
-- Date: 2026-01-17
-- ============================================================================

IF EXISTS (SELECT 1 FROM Themes WHERE ThemeName = 'Classic Black')
BEGIN
    PRINT 'Patching Classic Black money-tree colors for improved legibility...';

    DECLARE @ThemeId INT = (SELECT ThemeId FROM Themes WHERE ThemeName = 'Classic Black');

    IF EXISTS (SELECT 1 FROM ThemeMoneyTree WHERE ThemeId = @ThemeId)
    BEGIN
        UPDATE ThemeMoneyTree
        SET InactiveColor = '#A9A9A9',   -- DarkGray (brighter than previous)
            ActiveColor   = '#D3D3D3'    -- LightGray (brighter for active row)
        WHERE ThemeId = @ThemeId;

        PRINT 'Updated ThemeMoneyTree colors for Classic Black.';
    END
    ELSE
    BEGIN
        PRINT 'No ThemeMoneyTree entry found for Classic Black; skipping update.';
    END
END
ELSE
BEGIN
    PRINT 'Classic Black theme not found; skipping migration 00021.';
END

PRINT 'Migration 00021_update_classic_black_moneytree_text_color completed.';
GO
