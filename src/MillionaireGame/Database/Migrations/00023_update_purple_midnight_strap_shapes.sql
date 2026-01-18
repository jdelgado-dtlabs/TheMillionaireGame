-- ============================================================================
-- Migration: 00023_update_purple_midnight_strap_shapes
-- Description: Make Professional Purple and Midnight Black straps symmetrical
--              by setting their Question/Answer/AnswerLabel SvgShape to 'Rounded'.
-- Author: Automated Patch
-- Date: 2026-01-17
-- ============================================================================

DECLARE @ThemesToUpdate TABLE (ThemeName NVARCHAR(200));
INSERT INTO @ThemesToUpdate (ThemeName) VALUES ('Professional Purple'), ('Midnight Black');

PRINT 'Updating strap SvgShape to Rounded for selected themes...';

DECLARE @ThemeId INT;
DECLARE theme_cursor CURSOR LOCAL FAST_FORWARD FOR
SELECT ThemeId FROM Themes WHERE ThemeName IN (SELECT ThemeName FROM @ThemesToUpdate);

OPEN theme_cursor;
FETCH NEXT FROM theme_cursor INTO @ThemeId;
WHILE @@FETCH_STATUS = 0
BEGIN
    -- Update Question, Answer, and AnswerLabel straps to Rounded if they exist
    UPDATE ThemeStraps
    SET SvgShape = 'Rounded'
    WHERE ThemeId = @ThemeId AND StrapType IN ('Question', 'Answer', 'AnswerLabel')
      AND SvgShape <> 'Rounded';

    FETCH NEXT FROM theme_cursor INTO @ThemeId;
END

CLOSE theme_cursor;
DEALLOCATE theme_cursor;

PRINT 'Migration 00023_update_purple_midnight_strap_shapes completed.';
GO
