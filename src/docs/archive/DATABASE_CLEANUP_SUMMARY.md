# Database Cleanup Summary

## Overview
Cleaned up the questions and fff_questions tables by removing unused columns and creating fresh seed data with factually correct questions.

## Changes Made

### 1. Questions Table
**Removed Columns:**
- `Custom_FiftyFifty` - Unused
- `FiftyFifty_1` - Unused
- `FiftyFifty_2` - Unused
- `Custom_ATA` - Unused
- `ATA_A`, `ATA_B`, `ATA_C`, `ATA_D` - Replaced with dynamic generation

**Kept Columns:**
- `Id`, `Question`, `A`, `B`, `C`, `D`, `CorrectAnswer`
- `Level`, `Used`, `Note`, `Difficulty_Type`, `LevelRange`

**New Data:**
- 80 factually correct questions (20 per difficulty level)
- Level 1-5: Easy questions ($100-$1,000)
- Level 6-10: Medium questions ($2,000-$32,000)
- Level 11-14: Hard questions ($64,000-$500,000)
- Level 15: Million dollar questions

### 2. FFF Questions Table
**Removed Columns:**
- `Level` - Unused for FFF
- `Note` - Unused for FFF

**Kept Columns:**
- `Id`, `Question`, `A`, `B`, `C`, `D`, `CorrectAnswer`, `Used`

**Updated Data:**
- 40 reviewed and corrected Fastest Finger First questions
- All answers verified to contain only A, B, C, D
- Questions cover various categories: geography, history, time ordering, science, etc.

### 3. Code Changes

**Question.cs Model:**
- Removed ATAPercentageA/B/C/D properties
- Added GenerateATAPercentages() method to create random percentages favoring correct answer
- Kept AnswerALabel/B/C/D for FFF reveal functionality

**QuestionRepository.cs:**
- Removed ATA column references from INSERT/UPDATE queries
- Removed ATA percentage mapping from MapQuestion method
- Simplified parameter binding

**Screen Forms:**
- Updated GuestScreenForm, HostScreenForm, TVScreenForm to use GenerateATAPercentages()
- Replaced hardcoded ATA percentages with dynamic generation
- ATA results now favor correct answer (40-70%) with random distribution for wrong answers

**Question Editor:**
- Removed ATA percentage column visibility code
- Removed ATA import/export from CSV functionality
- CSV format now: Question, A, B, C, D, CorrectAnswer, Level, Explanation

## Running the Scripts

Execute the SQL scripts in order:

```powershell
# 1. Reset questions table
sqlcmd -S ".\SQLEXPRESS" -d dbMillionaire -E -i "src\docs\database\01_reset_questions_table.sql"

# 2. Reset fff_questions table
sqlcmd -S ".\SQLEXPRESS" -d dbMillionaire -E -i "src\docs\database\02_reset_fff_questions_table.sql"
```

## Impact

**Benefits:**
- Cleaner database schema with only columns actually used by the application
- Fresh seed data with verified factually correct questions
- Dynamic ATA generation creates more varied gameplay
- Removed dependency on pre-configured ATA percentages
- Simplified question management and editing

**Breaking Changes:**
- Old questions will be lost (backup before running scripts if needed)
- CSV import/export format changed (no longer includes ATA percentages)
- ATA results are now random each time (not stored in database)

## Notes

- The WAPS database still contains the ATAVotes table for real audience voting
- When real votes exist, they override the generated percentages
- AnswerLabel properties kept for FFF correct order reveal functionality
- All factual information in questions verified for accuracy
