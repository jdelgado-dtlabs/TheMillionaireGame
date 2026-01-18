-- Migration: 00009_fix_background_paths.sql
-- Purpose: Fix theme background paths to match actual embedded resource names
-- Author: System
-- Date: 2026-01-15

-- Fix Classic Gold background path (background1.png -> 01_bkg.png)
UPDATE ThemeBackgrounds 
SET ImagePath = 'embedded://01_bkg.png' 
WHERE ImagePath = 'embedded://background1.png' 
  AND ComponentType = 'TVScreen';

-- Fix Modern Blue background path (background2.png -> 02_bkg.png)
UPDATE ThemeBackgrounds 
SET ImagePath = 'embedded://02_bkg.png' 
WHERE ImagePath = 'embedded://background2.png' 
  AND ComponentType = 'TVScreen';

-- Fix Elegant Red background path (background3.png -> 03_bkg.png)
UPDATE ThemeBackgrounds 
SET ImagePath = 'embedded://03_bkg.png' 
WHERE ImagePath = 'embedded://background3.png' 
  AND ComponentType = 'TVScreen';

-- Fix Bold Green background path (background4.png -> 04_bkg.png)
UPDATE ThemeBackgrounds 
SET ImagePath = 'embedded://04_bkg.png' 
WHERE ImagePath = 'embedded://background4.png' 
  AND ComponentType = 'TVScreen';

-- Fix Professional Purple background path (background5.png -> 05_bkg.png)
UPDATE ThemeBackgrounds 
SET ImagePath = 'embedded://05_bkg.png' 
WHERE ImagePath = 'embedded://background5.png' 
  AND ComponentType = 'TVScreen';

-- Fix Midnight Black background path (background6.png -> 06_bkg.png)
UPDATE ThemeBackgrounds 
SET ImagePath = 'embedded://06_bkg.png' 
WHERE ImagePath = 'embedded://background6.png' 
  AND ComponentType = 'TVScreen';

PRINT 'Migration 00009: Fixed background paths to match embedded resource names';
