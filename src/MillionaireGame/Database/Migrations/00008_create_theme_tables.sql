-- ============================================================================
-- Migration: 00008_create_theme_tables
-- Description: Creates theme system tables for visual customization including
--              backgrounds, straps, money tree styling, and theme packs.
--              Enables users to create custom themes and import theme packs.
-- Author: Development Team
-- Date: 2026-01-14
-- Dependencies: None (standalone theme system)
-- ============================================================================

-- Table 1: Themes
-- Stores theme definitions (presets, user profiles, and custom themes)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Themes')
BEGIN
    CREATE TABLE Themes (
        ThemeId INT PRIMARY KEY IDENTITY(1,1),
        ThemeName NVARCHAR(100) NOT NULL,
        ThemeType NVARCHAR(20) NOT NULL CHECK (ThemeType IN ('Preset', 'UserProfile1', 'UserProfile2', 'Custom')),
        ThemePackId INT NULL,                       -- Reference to pack if imported (NULL for built-in)
        IsActive BIT DEFAULT 0,                     -- Only one active theme at a time
        Description NVARCHAR(500),
        Author NVARCHAR(100),
        Version NVARCHAR(20),
        CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        ModifiedDate DATETIME2 NOT NULL DEFAULT GETDATE()
    );

    CREATE INDEX IX_Themes_Active ON Themes(IsActive) WHERE IsActive = 1;
    CREATE INDEX IX_Themes_Type ON Themes(ThemeType);
    CREATE INDEX IX_Themes_PackId ON Themes(ThemePackId) WHERE ThemePackId IS NOT NULL;
    
    PRINT 'Created table: Themes';
END
ELSE
BEGIN
    PRINT 'Table already exists: Themes';
END
GO

-- Table 2: ThemeBackgrounds
-- Stores background configurations for different components (TV Screen, Money Tree)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ThemeBackgrounds')
BEGIN
    CREATE TABLE ThemeBackgrounds (
        ThemeBackgroundId INT PRIMARY KEY IDENTITY(1,1),
        ThemeId INT NOT NULL,
        ComponentType NVARCHAR(50) NOT NULL CHECK (ComponentType IN ('TVScreen', 'MoneyTree', 'General')),
        ImagePath NVARCHAR(500),                    -- Path relative to theme pack or absolute
        ChromaKeyEnabled BIT DEFAULT 0,
        ChromaKeyColor NVARCHAR(20),                -- HEX color (e.g., "#00FF00")
        ChromaKeyTolerance INT DEFAULT 50,          -- 0-100
        ScaleMode NVARCHAR(20) DEFAULT 'Fill' CHECK (ScaleMode IN ('Stretch', 'Fill', 'Fit', 'Center')),
        PositionX INT DEFAULT 0,
        PositionY INT DEFAULT 0,
        Transparency INT DEFAULT 100,               -- 0-100 (100 = opaque)
        FOREIGN KEY (ThemeId) REFERENCES Themes(ThemeId) ON DELETE CASCADE
    );

    CREATE INDEX IX_ThemeBackgrounds_ThemeId ON ThemeBackgrounds(ThemeId);
    CREATE INDEX IX_ThemeBackgrounds_ComponentType ON ThemeBackgrounds(ComponentType);
    
    PRINT 'Created table: ThemeBackgrounds';
END
ELSE
BEGIN
    PRINT 'Table already exists: ThemeBackgrounds';
END
GO

-- Table 3: ThemeStraps
-- Stores strap (question/answer overlay) configurations with SVG-based styling
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ThemeStraps')
BEGIN
    CREATE TABLE ThemeStraps (
        ThemeStrapId INT PRIMARY KEY IDENTITY(1,1),
        ThemeId INT NOT NULL,
        StrapType NVARCHAR(50) NOT NULL CHECK (StrapType IN ('Question', 'Answer', 'MoneyAmount', 'PlayerName', 'HostMessage')),
        SvgShape NVARCHAR(50) NOT NULL DEFAULT 'Classic',
        
        -- Color Configuration
        PrimaryColor NVARCHAR(20) NOT NULL DEFAULT '#8B4513',    -- Brown/Gold
        SecondaryColor NVARCHAR(20),                             -- For gradients
        GradientEnabled BIT DEFAULT 0,
        GradientAngle INT DEFAULT 90 CHECK (GradientAngle >= 0 AND GradientAngle <= 360),
        
        -- Effects
        EffectType NVARCHAR(50) CHECK (EffectType IN ('None', 'Sheen', 'Glow', 'Shadow', 'Metallic', 'Silk', 'Glass')),
        EffectIntensity INT DEFAULT 50 CHECK (EffectIntensity >= 0 AND EffectIntensity <= 100),
        EffectColor NVARCHAR(20),
        
        -- Border
        BorderEnabled BIT DEFAULT 1,
        BorderColor NVARCHAR(20) DEFAULT '#000000',
        BorderWidth INT DEFAULT 2 CHECK (BorderWidth >= 0),
        BorderStyle NVARCHAR(20) DEFAULT 'Solid' CHECK (BorderStyle IN ('Solid', 'Dashed', 'Dotted', 'Double')),
        
        -- Typography
        FontFamily NVARCHAR(100) DEFAULT 'Arial',
        FontSize INT DEFAULT 24 CHECK (FontSize > 0),
        FontColor NVARCHAR(20) DEFAULT '#FFFFFF',
        FontBold BIT DEFAULT 0,
        FontItalic BIT DEFAULT 0,
        
        -- Animation
        AnimationEnabled BIT DEFAULT 0,
        AnimationType NVARCHAR(50) CHECK (AnimationType IN ('None', 'FadeIn', 'SlideIn', 'ZoomIn', 'Bounce')),
        AnimationDuration INT DEFAULT 500 CHECK (AnimationDuration >= 0),
        
        FOREIGN KEY (ThemeId) REFERENCES Themes(ThemeId) ON DELETE CASCADE
    );

    CREATE INDEX IX_ThemeStraps_ThemeId ON ThemeStraps(ThemeId);
    CREATE INDEX IX_ThemeStraps_StrapType ON ThemeStraps(StrapType);
    
    PRINT 'Created table: ThemeStraps';
END
ELSE
BEGIN
    PRINT 'Table already exists: ThemeStraps';
END
GO

-- Table 4: ThemeMoneyTree
-- Stores money tree display configurations
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ThemeMoneyTree')
BEGIN
    CREATE TABLE ThemeMoneyTree (
        ThemeMoneyTreeId INT PRIMARY KEY IDENTITY(1,1),
        ThemeId INT NOT NULL,
        BackgroundImagePath NVARCHAR(500),
        
        -- Amount Colors
        InactiveColor NVARCHAR(20) DEFAULT '#808080',           -- Gray
        ActiveColor NVARCHAR(20) DEFAULT '#FFD700',             -- Gold
        CompletedColor NVARCHAR(20) DEFAULT '#00FF00',          -- Green
        SafeHavenColor NVARCHAR(20) DEFAULT '#0080FF',          -- Blue
        
        -- Highlight Effect
        HighlightEnabled BIT DEFAULT 1,
        HighlightType NVARCHAR(50) DEFAULT 'PulsingGlow' CHECK (HighlightType IN ('None', 'PulsingGlow', 'Flash', 'Shine', 'Border')),
        HighlightColor NVARCHAR(20) DEFAULT '#FFFF00',
        HighlightIntensity INT DEFAULT 80 CHECK (HighlightIntensity >= 0 AND HighlightIntensity <= 100),
        
        -- Typography
        FontFamily NVARCHAR(100) DEFAULT 'Arial Bold',
        FontSize INT DEFAULT 18 CHECK (FontSize > 0),
        FontBold BIT DEFAULT 1,
        
        FOREIGN KEY (ThemeId) REFERENCES Themes(ThemeId) ON DELETE CASCADE,
        CONSTRAINT UQ_ThemeMoneyTree_ThemeId UNIQUE (ThemeId)  -- One money tree config per theme
    );

    CREATE INDEX IX_ThemeMoneyTree_ThemeId ON ThemeMoneyTree(ThemeId);
    
    PRINT 'Created table: ThemeMoneyTree';
END
ELSE
BEGIN
    PRINT 'Table already exists: ThemeMoneyTree';
END
GO

-- Table 5: ThemePacks
-- Stores theme pack metadata (imported ZIP packages with themes + assets)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ThemePacks')
BEGIN
    CREATE TABLE ThemePacks (
        ThemePackId INT PRIMARY KEY IDENTITY(1,1),
        PackName NVARCHAR(100) NOT NULL,
        PackVersion NVARCHAR(20) NOT NULL,
        Author NVARCHAR(100),
        Description NVARCHAR(1000),
        InstallPath NVARCHAR(500) NOT NULL,         -- Path to extracted pack (lib/themepacks/[PackName]/)
        ImportDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT UQ_ThemePack_Name UNIQUE (PackName)
    );

    CREATE INDEX IX_ThemePacks_PackName ON ThemePacks(PackName);
    
    PRINT 'Created table: ThemePacks';
END
ELSE
BEGIN
    PRINT 'Table already exists: ThemePacks';
END
GO

-- Now add foreign key constraint from Themes to ThemePacks (had to wait until both tables exist)
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Themes_ThemePacks')
BEGIN
    ALTER TABLE Themes
    ADD CONSTRAINT FK_Themes_ThemePacks FOREIGN KEY (ThemePackId)
        REFERENCES ThemePacks(ThemePackId) ON DELETE SET NULL;
    
    PRINT 'Added foreign key: FK_Themes_ThemePacks';
END
GO

-- ============================================================================
-- Seed Built-in Preset Themes
-- ============================================================================

-- Check if presets already exist (prevent duplicate seeding)
IF NOT EXISTS (SELECT * FROM Themes WHERE ThemeType = 'Preset')
BEGIN
    PRINT 'Seeding built-in preset themes...';
    
    -- Preset 1: Classic Gold (Default Active Theme)
    INSERT INTO Themes (ThemeName, ThemeType, IsActive, Description, Author, Version)
    VALUES ('Classic Gold', 'Preset', 1, 'Traditional gold and brown theme with elegant styling', 'MillionaireGame Team', '1.0.0');
    
    DECLARE @ClassicGoldId INT = SCOPE_IDENTITY();
    
    -- Classic Gold: TV Screen Background
    INSERT INTO ThemeBackgrounds (ThemeId, ComponentType, ImagePath, ScaleMode)
    VALUES (@ClassicGoldId, 'TVScreen', 'embedded://background1.png', 'Fill');
    
    -- Classic Gold: Money Tree Background
    INSERT INTO ThemeBackgrounds (ThemeId, ComponentType, ImagePath, ScaleMode)
    VALUES (@ClassicGoldId, 'MoneyTree', 'embedded://moneytree_bg.png', 'Fill');
    
    -- Classic Gold: Question Strap
    INSERT INTO ThemeStraps (ThemeId, StrapType, SvgShape, PrimaryColor, SecondaryColor, GradientEnabled, GradientAngle, EffectType, EffectIntensity, BorderEnabled, FontFamily, FontSize, FontColor, FontBold)
    VALUES (@ClassicGoldId, 'Question', 'Classic', '#8B4513', '#D4AF37', 1, 90, 'Silk', 60, 1, 'Copperplate Gothic', 24, '#FFFFFF', 1);
    
    -- Classic Gold: Answer Strap
    INSERT INTO ThemeStraps (ThemeId, StrapType, SvgShape, PrimaryColor, SecondaryColor, GradientEnabled, GradientAngle, EffectType, EffectIntensity, BorderEnabled, FontFamily, FontSize, FontColor, FontBold)
    VALUES (@ClassicGoldId, 'Answer', 'Classic', '#8B4513', '#D4AF37', 1, 90, 'Silk', 60, 1, 'Arial', 22, '#FFFFFF', 1);
    
    -- Classic Gold: Money Tree
    INSERT INTO ThemeMoneyTree (ThemeId, InactiveColor, ActiveColor, CompletedColor, SafeHavenColor, HighlightEnabled, HighlightType, FontFamily, FontSize, FontBold)
    VALUES (@ClassicGoldId, '#808080', '#FFD700', '#00FF00', '#0080FF', 1, 'PulsingGlow', 'Arial Bold', 18, 1);
    
    PRINT 'Seeded: Classic Gold';
    
    -- Preset 2: Modern Blue
    INSERT INTO Themes (ThemeName, ThemeType, IsActive, Description, Author, Version)
    VALUES ('Modern Blue', 'Preset', 0, 'Clean blue and silver theme with modern aesthetics', 'MillionaireGame Team', '1.0.0');
    
    DECLARE @ModernBlueId INT = SCOPE_IDENTITY();
    
    INSERT INTO ThemeBackgrounds (ThemeId, ComponentType, ImagePath, ScaleMode)
    VALUES (@ModernBlueId, 'TVScreen', 'embedded://background2.png', 'Fill');
    
    INSERT INTO ThemeBackgrounds (ThemeId, ComponentType, ImagePath, ScaleMode)
    VALUES (@ModernBlueId, 'MoneyTree', 'embedded://moneytree_bg.png', 'Fill');
    
    INSERT INTO ThemeStraps (ThemeId, StrapType, SvgShape, PrimaryColor, SecondaryColor, GradientEnabled, GradientAngle, EffectType, EffectIntensity, BorderEnabled, FontFamily, FontSize, FontColor, FontBold)
    VALUES (@ModernBlueId, 'Question', 'Modern', '#0047AB', '#87CEEB', 1, 90, 'Glass', 70, 1, 'Segoe UI', 24, '#FFFFFF', 0);
    
    INSERT INTO ThemeStraps (ThemeId, StrapType, SvgShape, PrimaryColor, SecondaryColor, GradientEnabled, GradientAngle, EffectType, EffectIntensity, BorderEnabled, FontFamily, FontSize, FontColor, FontBold)
    VALUES (@ModernBlueId, 'Answer', 'Modern', '#0047AB', '#87CEEB', 1, 90, 'Glass', 70, 1, 'Segoe UI', 22, '#FFFFFF', 0);
    
    INSERT INTO ThemeMoneyTree (ThemeId, InactiveColor, ActiveColor, CompletedColor, SafeHavenColor, HighlightEnabled, HighlightType, FontFamily, FontSize, FontBold)
    VALUES (@ModernBlueId, '#808080', '#00BFFF', '#00FF00', '#0080FF', 1, 'Shine', 'Segoe UI', 18, 1);
    
    PRINT 'Seeded: Modern Blue';
    
    -- Preset 3: Elegant Red
    INSERT INTO Themes (ThemeName, ThemeType, IsActive, Description, Author, Version)
    VALUES ('Elegant Red', 'Preset', 0, 'Deep red and gold theme with luxurious styling', 'MillionaireGame Team', '1.0.0');
    
    DECLARE @ElegantRedId INT = SCOPE_IDENTITY();
    
    INSERT INTO ThemeBackgrounds (ThemeId, ComponentType, ImagePath, ScaleMode)
    VALUES (@ElegantRedId, 'TVScreen', 'embedded://background3.png', 'Fill');
    
    INSERT INTO ThemeBackgrounds (ThemeId, ComponentType, ImagePath, ScaleMode)
    VALUES (@ElegantRedId, 'MoneyTree', 'embedded://moneytree_bg.png', 'Fill');
    
    INSERT INTO ThemeStraps (ThemeId, StrapType, SvgShape, PrimaryColor, SecondaryColor, GradientEnabled, GradientAngle, EffectType, EffectIntensity, BorderEnabled, FontFamily, FontSize, FontColor, FontBold)
    VALUES (@ElegantRedId, 'Question', 'Classic', '#8B0000', '#FFD700', 1, 90, 'Metallic', 80, 1, 'Georgia', 24, '#FFFFFF', 1);
    
    INSERT INTO ThemeStraps (ThemeId, StrapType, SvgShape, PrimaryColor, SecondaryColor, GradientEnabled, GradientAngle, EffectType, EffectIntensity, BorderEnabled, FontFamily, FontSize, FontColor, FontBold)
    VALUES (@ElegantRedId, 'Answer', 'Classic', '#8B0000', '#FFD700', 1, 90, 'Metallic', 80, 1, 'Georgia', 22, '#FFFFFF', 1);
    
    INSERT INTO ThemeMoneyTree (ThemeId, InactiveColor, ActiveColor, CompletedColor, SafeHavenColor, HighlightEnabled, HighlightType, FontFamily, FontSize, FontBold)
    VALUES (@ElegantRedId, '#808080', '#DC143C', '#00FF00', '#FFD700', 1, 'PulsingGlow', 'Georgia', 18, 1);
    
    PRINT 'Seeded: Elegant Red';
    
    -- Preset 4: Bold Green
    INSERT INTO Themes (ThemeName, ThemeType, IsActive, Description, Author, Version)
    VALUES ('Bold Green', 'Preset', 0, 'Vibrant green and white theme with dynamic styling', 'MillionaireGame Team', '1.0.0');
    
    DECLARE @BoldGreenId INT = SCOPE_IDENTITY();
    
    INSERT INTO ThemeBackgrounds (ThemeId, ComponentType, ImagePath, ScaleMode)
    VALUES (@BoldGreenId, 'TVScreen', 'embedded://background4.png', 'Fill');
    
    INSERT INTO ThemeBackgrounds (ThemeId, ComponentType, ImagePath, ScaleMode)
    VALUES (@BoldGreenId, 'MoneyTree', 'embedded://moneytree_bg.png', 'Fill');
    
    INSERT INTO ThemeStraps (ThemeId, StrapType, SvgShape, PrimaryColor, SecondaryColor, GradientEnabled, GradientAngle, EffectType, EffectIntensity, BorderEnabled, FontFamily, FontSize, FontColor, FontBold)
    VALUES (@BoldGreenId, 'Question', 'Rounded', '#006400', '#90EE90', 1, 90, 'Glow', 90, 1, 'Impact', 24, '#FFFFFF', 1);
    
    INSERT INTO ThemeStraps (ThemeId, StrapType, SvgShape, PrimaryColor, SecondaryColor, GradientEnabled, GradientAngle, EffectType, EffectIntensity, BorderEnabled, FontFamily, FontSize, FontColor, FontBold)
    VALUES (@BoldGreenId, 'Answer', 'Rounded', '#006400', '#90EE90', 1, 90, 'Glow', 90, 1, 'Impact', 22, '#FFFFFF', 1);
    
    INSERT INTO ThemeMoneyTree (ThemeId, InactiveColor, ActiveColor, CompletedColor, SafeHavenColor, HighlightEnabled, HighlightType, FontFamily, FontSize, FontBold)
    VALUES (@BoldGreenId, '#808080', '#32CD32', '#00FF00', '#FFD700', 1, 'Flash', 'Impact', 18, 1);
    
    PRINT 'Seeded: Bold Green';
    
    -- Preset 5: Professional Purple
    INSERT INTO Themes (ThemeName, ThemeType, IsActive, Description, Author, Version)
    VALUES ('Professional Purple', 'Preset', 0, 'Purple with silver accents for professional broadcasts', 'MillionaireGame Team', '1.0.0');
    
    DECLARE @ProfessionalPurpleId INT = SCOPE_IDENTITY();
    
    INSERT INTO ThemeBackgrounds (ThemeId, ComponentType, ImagePath, ScaleMode)
    VALUES (@ProfessionalPurpleId, 'TVScreen', 'embedded://background5.png', 'Fill');
    
    INSERT INTO ThemeBackgrounds (ThemeId, ComponentType, ImagePath, ScaleMode)
    VALUES (@ProfessionalPurpleId, 'MoneyTree', 'embedded://moneytree_bg.png', 'Fill');
    
    INSERT INTO ThemeStraps (ThemeId, StrapType, SvgShape, PrimaryColor, SecondaryColor, GradientEnabled, GradientAngle, EffectType, EffectIntensity, BorderEnabled, FontFamily, FontSize, FontColor, FontBold)
    VALUES (@ProfessionalPurpleId, 'Question', 'Sharp', '#4B0082', '#C0C0C0', 1, 90, 'Silk', 70, 1, 'Calibri', 24, '#FFFFFF', 0);
    
    INSERT INTO ThemeStraps (ThemeId, StrapType, SvgShape, PrimaryColor, SecondaryColor, GradientEnabled, GradientAngle, EffectType, EffectIntensity, BorderEnabled, FontFamily, FontSize, FontColor, FontBold)
    VALUES (@ProfessionalPurpleId, 'Answer', 'Sharp', '#4B0082', '#C0C0C0', 1, 90, 'Silk', 70, 1, 'Calibri', 22, '#FFFFFF', 0);
    
    INSERT INTO ThemeMoneyTree (ThemeId, InactiveColor, ActiveColor, CompletedColor, SafeHavenColor, HighlightEnabled, HighlightType, FontFamily, FontSize, FontBold)
    VALUES (@ProfessionalPurpleId, '#808080', '#9370DB', '#00FF00', '#C0C0C0', 1, 'Shine', 'Calibri', 18, 0);
    
    PRINT 'Seeded: Professional Purple';
    
    -- Preset 6: Midnight Black
    INSERT INTO Themes (ThemeName, ThemeType, IsActive, Description, Author, Version)
    VALUES ('Midnight Black', 'Preset', 0, 'Premium black and gold theme with dramatic lighting', 'MillionaireGame Team', '1.0.0');
    
    DECLARE @MidnightBlackId INT = SCOPE_IDENTITY();
    
    INSERT INTO ThemeBackgrounds (ThemeId, ComponentType, ImagePath, ScaleMode)
    VALUES (@MidnightBlackId, 'TVScreen', 'embedded://background6.png', 'Fill');
    
    INSERT INTO ThemeBackgrounds (ThemeId, ComponentType, ImagePath, ScaleMode)
    VALUES (@MidnightBlackId, 'MoneyTree', 'embedded://moneytree_bg.png', 'Fill');
    
    INSERT INTO ThemeStraps (ThemeId, StrapType, SvgShape, PrimaryColor, SecondaryColor, GradientEnabled, GradientAngle, EffectType, EffectIntensity, BorderEnabled, FontFamily, FontSize, FontColor, FontBold)
    VALUES (@MidnightBlackId, 'Question', 'Sharp', '#000000', '#FFD700', 1, 90, 'Metallic', 90, 1, 'Times New Roman', 24, '#FFD700', 1);
    
    INSERT INTO ThemeStraps (ThemeId, StrapType, SvgShape, PrimaryColor, SecondaryColor, GradientEnabled, GradientAngle, EffectType, EffectIntensity, BorderEnabled, FontFamily, FontSize, FontColor, FontBold)
    VALUES (@MidnightBlackId, 'Answer', 'Sharp', '#000000', '#FFD700', 1, 90, 'Metallic', 90, 1, 'Times New Roman', 22, '#FFD700', 1);
    
    INSERT INTO ThemeMoneyTree (ThemeId, InactiveColor, ActiveColor, CompletedColor, SafeHavenColor, HighlightEnabled, HighlightType, FontFamily, FontSize, FontBold)
    VALUES (@MidnightBlackId, '#404040', '#FFD700', '#00FF00', '#FFFFFF', 1, 'PulsingGlow', 'Times New Roman', 18, 1);
    
    PRINT 'Seeded: Midnight Black';
    
    PRINT 'All preset themes seeded successfully!';
END
ELSE
BEGIN
    PRINT 'Preset themes already exist, skipping seed.';
END
GO

-- ============================================================================
-- Migration Complete
-- ============================================================================

PRINT '============================================================================';
PRINT 'Migration 00008_create_theme_tables completed successfully!';
PRINT 'Created 5 tables: Themes, ThemeBackgrounds, ThemeStraps, ThemeMoneyTree, ThemePacks';
PRINT 'Seeded 6 built-in preset themes: Classic Gold (active), Modern Blue, Elegant Red,';
PRINT '  Bold Green, Professional Purple, Midnight Black';
PRINT '============================================================================';
GO
