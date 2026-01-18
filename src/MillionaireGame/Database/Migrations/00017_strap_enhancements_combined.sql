-- Migration 00017: Comprehensive strap enhancements for Phase 7.1
-- Combines: AnswerLabel strap addition, border/outline enhancements, and Classic Gold font fixes
-- This migration adds AnswerLabel strap type, increases borders, adds white outline effects,
-- and corrects Classic Gold fonts to match original design.

-- ============================================================================
-- PART 1: Add AnswerLabel strap type to schema
-- ============================================================================

-- Drop existing StrapType constraint (find it dynamically since name is auto-generated)
DECLARE @ConstraintName NVARCHAR(200);
SELECT @ConstraintName = name 
FROM sys.check_constraints 
WHERE parent_object_id = OBJECT_ID('ThemeStraps') 
  AND definition LIKE '%StrapType%';

IF @ConstraintName IS NOT NULL
BEGIN
    DECLARE @SQL NVARCHAR(500) = 'ALTER TABLE ThemeStraps DROP CONSTRAINT ' + QUOTENAME(@ConstraintName);
    EXEC sp_executesql @SQL;
    PRINT 'Dropped existing StrapType constraint: ' + @ConstraintName;
END
GO

-- Add new constraint with AnswerLabel
ALTER TABLE ThemeStraps 
ADD CONSTRAINT CK_ThemeStraps_StrapType 
CHECK (StrapType IN ('Question', 'Answer', 'AnswerLabel', 'MoneyAmount', 'PlayerName', 'HostMessage'));
GO

PRINT 'Updated StrapType constraint to include AnswerLabel';

-- ============================================================================
-- PART 2: Fix EffectType constraint to match actual implemented effects
-- ============================================================================

-- Drop existing EffectType constraint (find it dynamically)
DECLARE @EffectConstraintName NVARCHAR(200);
SELECT @EffectConstraintName = name 
FROM sys.check_constraints 
WHERE parent_object_id = OBJECT_ID('ThemeStraps') 
  AND definition LIKE '%EffectType%';

IF @EffectConstraintName IS NOT NULL
BEGIN
    DECLARE @EffectSQL NVARCHAR(500) = 'ALTER TABLE ThemeStraps DROP CONSTRAINT ' + QUOTENAME(@EffectConstraintName);
    EXEC sp_executesql @EffectSQL;
    PRINT 'Dropped existing EffectType constraint: ' + @EffectConstraintName;
END
GO

-- Add new constraint with actually implemented effect types
ALTER TABLE ThemeStraps 
ADD CONSTRAINT CK_ThemeStraps_EffectType 
CHECK (EffectType IN ('None', 'Glow', 'Shadow', '3D', 'Outline', 'Emboss'));
GO

PRINT 'Updated EffectType constraint to include Outline and other implemented effects';

-- ============================================================================
-- PART 3: Insert AnswerLabel straps with final values (BorderWidth=4, Outline effect at 40%)
-- ============================================================================

-- Classic Gold: AnswerLabel (Arial font, 4px border, white outline at 40% intensity)
DECLARE @ClassicGoldId INT = (SELECT ThemeId FROM Themes WHERE ThemeName = 'Classic Gold');
INSERT INTO ThemeStraps (ThemeId, StrapType, SvgShape, PrimaryColor, SecondaryColor, GradientEnabled, GradientAngle, EffectType, EffectIntensity, EffectColor, BorderEnabled, BorderWidth, FontFamily, FontSize, FontColor, FontBold)
VALUES (@ClassicGoldId, 'AnswerLabel', 'Classic', '#8B4513', '#D4AF37', 1, 90, 'Outline', 40, '#FFFFFF', 1, 4, 'Arial', 28, '#FFFFFF', 1);

-- Modern Blue: AnswerLabel
DECLARE @ModernBlueId INT = (SELECT ThemeId FROM Themes WHERE ThemeName = 'Modern Blue');
INSERT INTO ThemeStraps (ThemeId, StrapType, SvgShape, PrimaryColor, SecondaryColor, GradientEnabled, GradientAngle, EffectType, EffectIntensity, EffectColor, BorderEnabled, BorderWidth, FontFamily, FontSize, FontColor, FontBold)
VALUES (@ModernBlueId, 'AnswerLabel', 'Modern', '#0047AB', '#87CEEB', 1, 90, 'Outline', 40, '#FFFFFF', 1, 4, 'Segoe UI', 28, '#FFFFFF', 1);

-- Elegant Red: AnswerLabel
DECLARE @ElegantRedId INT = (SELECT ThemeId FROM Themes WHERE ThemeName = 'Elegant Red');
INSERT INTO ThemeStraps (ThemeId, StrapType, SvgShape, PrimaryColor, SecondaryColor, GradientEnabled, GradientAngle, EffectType, EffectIntensity, EffectColor, BorderEnabled, BorderWidth, FontFamily, FontSize, FontColor, FontBold)
VALUES (@ElegantRedId, 'AnswerLabel', 'Modern', '#8B0000', '#DC143C', 1, 90, 'Outline', 40, '#FFFFFF', 1, 4, 'Georgia', 28, '#FFFFFF', 1);

-- Bold Green: AnswerLabel
DECLARE @BoldGreenId INT = (SELECT ThemeId FROM Themes WHERE ThemeName = 'Bold Green');
INSERT INTO ThemeStraps (ThemeId, StrapType, SvgShape, PrimaryColor, SecondaryColor, GradientEnabled, GradientAngle, EffectType, EffectIntensity, EffectColor, BorderEnabled, BorderWidth, FontFamily, FontSize, FontColor, FontBold)
VALUES (@BoldGreenId, 'AnswerLabel', 'Modern', '#006400', '#32CD32', 1, 90, 'Outline', 40, '#FFFFFF', 1, 4, 'Impact', 28, '#FFFFFF', 1);

-- Professional Purple: AnswerLabel
DECLARE @ProfessionalPurpleId INT = (SELECT ThemeId FROM Themes WHERE ThemeName = 'Professional Purple');
INSERT INTO ThemeStraps (ThemeId, StrapType, SvgShape, PrimaryColor, SecondaryColor, GradientEnabled, GradientAngle, EffectType, EffectIntensity, EffectColor, BorderEnabled, BorderWidth, FontFamily, FontSize, FontColor, FontBold)
VALUES (@ProfessionalPurpleId, 'AnswerLabel', 'Modern', '#4B0082', '#8A2BE2', 1, 90, 'Outline', 40, '#FFFFFF', 1, 4, 'Calibri', 28, '#FFFFFF', 1);

-- Midnight Black: AnswerLabel
DECLARE @MidnightBlackId INT = (SELECT ThemeId FROM Themes WHERE ThemeName = 'Midnight Black');
INSERT INTO ThemeStraps (ThemeId, StrapType, SvgShape, PrimaryColor, SecondaryColor, GradientEnabled, GradientAngle, EffectType, EffectIntensity, EffectColor, BorderEnabled, BorderWidth, FontFamily, FontSize, FontColor, FontBold)
VALUES (@MidnightBlackId, 'AnswerLabel', 'Modern', '#000000', '#2F4F4F', 1, 90, 'Outline', 40, '#FFFFFF', 1, 4, 'Times New Roman', 28, '#FFFFFF', 1);

PRINT 'Added AnswerLabel straps with final border and outline settings for all 6 preset themes';

-- ============================================================================
-- PART 4: Update existing Question and Answer straps (borders, outline at 40%, Classic Gold fonts)
-- ============================================================================

-- Classic Gold: Update Question and Answer with Copperplate Gothic Bold font + border/outline
UPDATE ThemeStraps
SET BorderWidth = 4,
    EffectType = 'Outline',
    EffectColor = '#FFFFFF',
    EffectIntensity = 40,
    FontFamily = 'Copperplate Gothic Bold'
WHERE ThemeId = (SELECT ThemeId FROM Themes WHERE ThemeName = 'Classic Gold')
  AND StrapType IN ('Question', 'Answer');

-- Modern Blue: Update Question and Answer with border/outline only
UPDATE ThemeStraps
SET BorderWidth = 4,
    EffectType = 'Outline',
    EffectColor = '#FFFFFF',
    EffectIntensity = 40
WHERE ThemeId = (SELECT ThemeId FROM Themes WHERE ThemeName = 'Modern Blue')
  AND StrapType IN ('Question', 'Answer');

-- Elegant Red: Update Question and Answer with border/outline only
UPDATE ThemeStraps
SET BorderWidth = 4,
    EffectType = 'Outline',
    EffectColor = '#FFFFFF',
    EffectIntensity = 40
WHERE ThemeId = (SELECT ThemeId FROM Themes WHERE ThemeName = 'Elegant Red')
  AND StrapType IN ('Question', 'Answer');

-- Bold Green: Update Question and Answer with border/outline only
UPDATE ThemeStraps
SET BorderWidth = 4,
    EffectType = 'Outline',
    EffectColor = '#FFFFFF',
    EffectIntensity = 40
WHERE ThemeId = (SELECT ThemeId FROM Themes WHERE ThemeName = 'Bold Green')
  AND StrapType IN ('Question', 'Answer');

-- Professional Purple: Update Question and Answer with border/outline only
UPDATE ThemeStraps
SET BorderWidth = 4,
    EffectType = 'Outline',
    EffectColor = '#FFFFFF',
    EffectIntensity = 40
WHERE ThemeId = (SELECT ThemeId FROM Themes WHERE ThemeName = 'Professional Purple')
  AND StrapType IN ('Question', 'Answer');

-- Midnight Black: Update Question and Answer with border/outline only
UPDATE ThemeStraps
SET BorderWidth = 4,
    EffectType = 'Outline',
    EffectColor = '#FFFFFF',
    EffectIntensity = 40
WHERE ThemeId = (SELECT ThemeId FROM Themes WHERE ThemeName = 'Midnight Black')
  AND StrapType IN ('Question', 'Answer');

PRINT 'Updated existing Question/Answer straps with borders and outline at 40% intensity; Fixed Classic Gold fonts';

PRINT 'Migration 00017 completed: Comprehensive strap enhancements applied successfully';
GO
