-- Migration: 00010_remove_theme_backgrounds.sql
-- Purpose: Remove theme background entries - backgrounds are handled by separate settings, not per-theme
-- Author: System
-- Date: 2026-01-15

-- Delete all theme background entries since backgrounds are managed independently
DELETE FROM ThemeBackgrounds;

PRINT 'Migration 00010: Removed all theme background entries - backgrounds managed separately';
