# FFF Online Issues - Session 2026-01-15

## Issue: Intro + Explain Button Not Enabling

**Status:** Unresolved - Deferred for later investigation

### Symptoms
- FFF Online panel's "1. Intro + Explain" button remains disabled (gray)
- Button should enable (light green) when:
  - Questions are loaded (`hasQuestionsAvailable` = true)
  - At least 1 participant is connected (`hasParticipants` = true)
- User reports 1 client is connected but button still won't enable

### Recent Changes
- Added automatic question loading in `FFFOnlinePanel.cs`:
  - `LoadQuestionsAsync()` now called on control Load and VisibleChanged events
  - This should populate `_questions` list automatically

### Suspected Root Cause
**Database Schema Mismatch:**
- FFF Questions table may have extra columns that don't match the code's expected schema
- When `LoadQuestionsAsync()` runs, it might be failing silently due to column mismatch
- Result: `_questions` list remains empty, `hasQuestionsAvailable` = false
- Button stays disabled because first condition not met

### Code Location
- **File:** `src/MillionaireGame/Forms/FFFOnlinePanel.cs`
- **Method:** `UpdateUIState()` (line ~590)
- **Condition:** `btnIntroExplain.Enabled = hasQuestionsAvailable && hasParticipants;`
- **Loading:** `LoadQuestionsAsync()` (line ~123) calls `_fffRepository.GetAllQuestionsAsync()`

### Investigation Steps for Future
1. Check `FFFQuestions` table schema in database
2. Compare with `FFFQuestion` model in `MillionaireGame.Web.Models`
3. Look for extra columns that code doesn't expect
4. Check GameConsole logs during panel load for errors in `LoadQuestionsAsync()`
5. Verify `_questions.Count` after load attempt

### Related Files
- `src/MillionaireGame/Forms/FFFOnlinePanel.cs` - Control panel logic
- `src/MillionaireGame.Web/Database/FFFQuestionRepository.cs` - Database access
- `src/MillionaireGame.Web/Models/FFFQuestion.cs` - Question model
- Database table: `FFFQuestions`

### Workaround
None available - feature non-functional

### Next Steps
1. Run the application and check GameConsole for errors when FFF Online panel loads
2. Verify database schema matches code expectations
3. Fix schema mismatch if found (likely needs migration to remove extra columns)
4. Test button enabling with fixed schema

---

**Note:** This issue was discovered during Phase 7.1 SVG strap integration testing. The FFF contestant straps have been successfully updated to use SVG rendering, but the overall FFF Online flow cannot be tested until this initialization issue is resolved.
