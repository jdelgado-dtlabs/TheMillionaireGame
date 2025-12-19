# Database Table Unification Plan

## Current State
- **questions** table: Regular game questions with A, B, C, D answers
- **fff_questions** table: Fastest Finger First questions with same structure

## Problem
- Duplicate table structures
- Separate repositories and management
- Error "Answer1" suggests confusion between naming conventions

## Proposed Solution: Unified Questions Table

### New Schema
Merge both tables into a single `questions` table with:

```sql
CREATE TABLE questions (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Question NVARCHAR(MAX) NOT NULL,
    A NVARCHAR(500) NOT NULL,
    B NVARCHAR(500) NOT NULL,
    C NVARCHAR(500) NOT NULL,
    D NVARCHAR(500) NOT NULL,
    CorrectAnswer NVARCHAR(10) NOT NULL, -- For FFF: stores order like "BADC", For Regular: stores "A", "B", "C", or "D"
    Level INT NOT NULL, -- For FFF: 0 or ignored, For Regular: 1-15
    Used BIT NOT NULL DEFAULT 0,
    Note NVARCHAR(MAX) NULL, -- Stores explanation for regular questions
    Difficulty_Type NVARCHAR(50) NULL, -- NULL for FFF questions
    LevelRange NVARCHAR(50) NULL, -- NULL for FFF questions
    Custom_FiftyFifty BIT NULL,
    FiftyFifty_1 NVARCHAR(10) NULL,
    FiftyFifty_2 NVARCHAR(10) NULL,
    Custom_ATA BIT NULL,
    ATA_A INT NULL,
    ATA_B INT NULL,
    ATA_C INT NULL,
    ATA_D INT NULL,
    IsFFF BIT NOT NULL DEFAULT 0 -- NEW: TRUE for FFF questions, FALSE for regular
)
```

### Migration Steps

1. **Add IsFFF Column**
   ```sql
   ALTER TABLE questions ADD IsFFF BIT NOT NULL DEFAULT 0;
   ```

2. **Migrate FFF Questions**
   ```sql
   INSERT INTO questions 
   (Question, A, B, C, D, CorrectAnswer, Level, Used, IsFFF)
   SELECT Question, A, B, C, D, CorrectAnswer, ISNULL(Level, 0), Used, 1
   FROM fff_questions;
   ```

3. **Drop Old FFF Table**
   ```sql
   DROP TABLE fff_questions;
   ```

### Code Changes

1. **Question Model** - Already supports all fields
   - Add `public bool IsFFF { get; set; } = false;`

2. **Remove FFFQuestion Model** - No longer needed
   - FFF questions are just Question objects with IsFFF=true

3. **QuestionRepository Updates**
   - `GetRandomQuestionAsync()` - Add IsFFF filter
   - `GetRandomFFFQuestionAsync()` - Query with IsFFF=true
   - Remove FFFQuestionRepository entirely

4. **Question Editor Updates**
   - Single grid with filter toggle for FFF/Regular
   - Or tabs that filter by IsFFF flag
   - Add/Edit forms check IsFFF to show/hide relevant fields

### Benefits

✅ Single source of truth for all questions
✅ Simplified codebase - one repository instead of two
✅ Easier to manage and maintain
✅ No confusion between Answer1-4 vs A-D naming
✅ Consistent database structure
✅ Can easily add new question types in future

### Implementation Order

1. ✅ Fix FFFQuestionRepository to use A,B,C,D columns (DONE)
2. Test Question Editor with current structure
3. Add IsFFF column to questions table
4. Migrate data from fff_questions
5. Update Question model
6. Update QuestionRepository
7. Remove FFFQuestion model and repository
8. Update Question Editor UI
9. Test thoroughly
10. Drop fff_questions table

## Notes
- Keep backup of both tables before migration
- Update all queries to filter by IsFFF where appropriate
- For FFF questions, CorrectAnswer stores the ordering (e.g., "BADC")
- For Regular questions, CorrectAnswer stores single letter ("A", "B", "C", or "D")
