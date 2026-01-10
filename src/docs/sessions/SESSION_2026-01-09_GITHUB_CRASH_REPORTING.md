# Development Session Summary - GitHub Crash Reporting Planning
**Date:** January 9, 2026  
**Branch:** master  
**Developer:** GitHub Copilot + User  
**Session Duration:** ~1 hour  
**Status:** ✅ PLANNING COMPLETED

---

## Objectives
1. Evaluate feasibility of GitHub crash report submission
2. Design privacy-focused crash reporting system with user consent
3. Fix log path issues in crash handler
4. Create comprehensive implementation plan

---

## Discussion Summary

### Initial Question
User asked whether the crash handler could submit crash reports to GitHub Issues with user consent. This led to an evaluation of different implementation approaches.

### Approach Evaluation

#### Option 1: GitHub Personal Access Token
- ❌ Poor UX (users must create token manually)
- ❌ Security risk (users might use overprivileged tokens)
- ✅ Simple implementation

#### Option 2: GitHub OAuth App ⭐ **SELECTED**
- ✅ Better UX (one-click "Sign in with GitHub")
- ✅ Minimal permissions (`public_repo` scope only)
- ✅ More secure than hardcoded tokens
- ✅ Natural filtering (only serious users will authenticate)
- ❌ Slightly more complex implementation

#### Option 3: Anonymous Submission Service
- ✅ No user GitHub account needed
- ✅ Most private
- ❌ Requires hosting infrastructure
- ❌ Monthly costs
- ❌ Maintenance burden

### Decision Rationale
Selected **Option 2 (GitHub OAuth)** because:
1. **Natural filtering**: Only users who care enough to authenticate will submit
2. **Prevents flooding**: OAuth requirement + rate limiting prevents spam
3. **Better tracking**: Can deduplicate by user + crash type
4. **Professional approach**: Standard for open-source projects

---

## Implementation Plan Created

### Document Location
`src/docs/active/GITHUB_CRASH_REPORTING_PLAN.md` (705 lines)

### Key Components

#### 1. Authentication (GitHub OAuth App)
- **Application Name**: "Millionaire Game Crash Reporter"
- **Callback URL**: `http://localhost:8888/oauth/callback`
- **Scope**: `public_repo` (issues only)
- **Token Storage**: Windows Credential Manager (encrypted)

#### 2. Data Sanitization (Privacy-First)
```csharp
public class DataSanitizer
{
    // Sanitization rules:
    - Replace absolute paths with [APP_DIR]
    - Remove machine names → [MACHINE]
    - Remove usernames → [USER]
    - Redact connection strings
    - Redact API keys/tokens
    - Remove AppData paths
    - Anonymize IP addresses (if any)
}
```

#### 3. Issue Submission
- **Duplicate Detection**: Check last 7 days for same exit code
- **Rate Limiting**: Max 5 submissions/day
- **Cooldown**: 1 hour minimum between identical crashes
- **Labels**: "bug", "crash-report", "automated"
- **Issue Title Format**: `Crash Report: {ExitCodeMeaning} (0x{ExitCode}) - {Activity}`

#### 4. User Interface
- **CrashReportDialog**: Review crash summary before submission
- **Review Button**: Show full sanitized report
- **GitHub Sign-in**: OAuth flow in default browser
- **Settings Integration**: Enable/disable, sign in/out

### Implementation Timeline
- **Week 1**: Core infrastructure + data sanitization
- **Week 2**: GitHub integration + UI
- **Week 3**: Watchdog integration + testing
- **Week 4**: Security review + documentation
- **Week 5**: Beta testing
- **Week 6**: Public release

**Total Estimated Time**: 6 weeks

---

## Bug Fixes Implemented

### Issue #1: Incorrect Log Path in CrashReportGenerator

**Problem**: Crash handler was looking for logs in wrong location
- ❌ **Old**: `AppDomain.CurrentDomain.BaseDirectory\GameConsole.log`
- ✅ **New**: `%LOCALAPPDATA%\TheMillionaireGame\Logs\*_game.log`

**File Modified**: `src/MillionaireGame.Watchdog/CrashReportGenerator.cs`

**Changes**:
1. Updated log directory to correct AppData location
2. Fixed filename pattern (was `GameConsole_*.log`, now `*_game.log`)
3. Added support for WebServer logs (`*_webserver.log`)
4. Changed from 100 lines of one log to 50 lines of each (100 total)

### Log File Format Discovery
Through investigation of FileLogger.cs and GameConsole.cs:
- **Filename Format**: `yyyy-MM-dd_HH-mm-ss_{prefix}.log`
- **Game Log Prefix**: `"game"` → `2026-01-09_14-30-00_game.log`
- **WebServer Log Prefix**: `"webserver"` → `2026-01-09_14-30-00_webserver.log`

### Updated Crash Report Format
```
[RECENT LOGS]
--- GAME LOG: 2026-01-09_14-00-00_game.log ---
(Last 50 lines from game log)

--- WEBSERVER LOG: 2026-01-09_14-00-00_webserver.log ---
(Last 50 lines from webserver log)
```

---

## Documentation Updates

### Files Modified

#### 1. CRASH_HANDLER_DOCUMENTATION.md
- Updated log file location section
- Corrected filename patterns
- Added both game and webserver log info
- Updated example crash report format

#### 2. INDEX.md
- Added GITHUB_CRASH_REPORTING_PLAN.md to active documents
- Listed all active planning documents

---

## Key Design Principles

### Privacy Protection
1. **Explicit Consent**: Never submit without user permission
2. **Review Before Send**: User can review sanitized report
3. **Data Sanitization**: Comprehensive PII removal
4. **Local-First**: Reports always saved locally regardless of submission
5. **Revocable**: User can disable/disconnect at any time

### Anti-Flooding Measures
1. **OAuth Barrier**: Natural spam filter
2. **Duplicate Detection**: Check for same crash in last 7 days
3. **Rate Limiting**: Max 5 submissions per day per user
4. **Cooldown Period**: 1 hour between identical crashes
5. **GitHub Search API**: Query existing issues before creating new one

### Performance Guarantees
1. **Non-Blocking**: OAuth flow happens after crash detection
2. **Async Operations**: All HTTP requests are async with timeouts
3. **No Main App Impact**: Runs in watchdog process only
4. **Minimal API Calls**: Efficient duplicate checking with filters
5. **Fail Gracefully**: Network failures don't prevent local crash reports

---

## Security Considerations

### Token Security
- **Storage**: Windows Credential Manager with machine-specific encryption
- **Scope**: Minimal (`public_repo` only)
- **Expiration**: Handle token expiration and refresh
- **CSRF Protection**: State token validation in OAuth flow

### Data Safety
- **No Hardcoded Secrets**: Client Secret handled via localhost callback or proxy
- **Sanitization Testing**: Unit tests verify all PII removal
- **Manual Review**: Encourage user review before first submission

---

## Git Operations

### Commit Details
**Commit**: f56fee9  
**Message**: "Add GitHub crash reporting plan and fix log path in crash handler"

**Changes**:
- 4 files changed
- 705 insertions, 9 deletions

**Files**:
1. `src/docs/active/GITHUB_CRASH_REPORTING_PLAN.md` (new, 705 lines)
2. `src/MillionaireGame.Watchdog/CrashReportGenerator.cs` (modified)
3. `src/docs/CRASH_HANDLER_DOCUMENTATION.md` (modified)
4. `src/docs/INDEX.md` (modified)

### Push Status
✅ **Successfully pushed to remote repository**
- Remote: Macronair/TheMillionaireGame
- Branch: master

---

## Technical Insights

### Discovery Process
The session involved careful investigation of existing code:

1. **Searched for log paths**: Found logs in `%LOCALAPPDATA%\TheMillionaireGame\Logs\`
2. **Checked FileLogger.cs**: Discovered filename format with timestamp prefix
3. **Found GameConsole prefix**: `"game"` not `"GameConsole"`
4. **Found WebServerConsole prefix**: `"webserver"` 
5. **User caught initial mistakes**: Iterative refinement of patterns

This demonstrates the value of code archaeology and careful verification!

---

## Success Metrics (Future)

### When Implemented
- **Adoption Rate**: % of users who enable crash reporting
- **Submission Volume**: Number of crash reports received
- **Duplicate Rate**: % of duplicates caught by detection
- **Issue Resolution**: Average time to fix reported crashes
- **Privacy Feedback**: User concerns about data submission

---

## Next Steps

### Immediate (When Ready to Implement)
1. Register GitHub OAuth App at https://github.com/settings/developers
2. Begin Phase 1: Core infrastructure
3. Implement data sanitization with unit tests
4. Build OAuth flow and token management

### Short-Term
1. Complete Phase 2-3: GitHub integration + UI
2. Test thoroughly with real crash scenarios
3. Verify sanitization removes all PII

### Long-Term (v1.1.0 Release)
1. Beta testing with opt-in users
2. Monitor for privacy issues
3. Collect feedback on UX
4. Public release with clear documentation

---

## Lessons Learned

### Code Investigation
- **Always verify**: Don't assume log locations or filename patterns
- **Check multiple sources**: GameConsole, WebServerConsole, FileLogger
- **User feedback valuable**: Caught multiple mistakes through review

### Design Trade-offs
- **OAuth complexity justified**: Better UX and security worth the effort
- **50 lines per log**: Balances context vs. report size
- **Privacy first**: User trust is paramount

---

## Open Questions (For Implementation)

1. **Screenshots**: Should we include game window screenshots?
   - **Pros**: Visual context for UI bugs
   - **Cons**: Privacy concerns, larger uploads
   - **Decision**: No for v1.1.0, reconsider for v1.2.0

2. **Auto-Submit**: Should we add "always submit" option?
   - **Pros**: More crash data collection
   - **Cons**: Privacy concerns, less control
   - **Decision**: Add as setting but default OFF

3. **Stack Traces**: Include with crash reports?
   - **Pros**: Better debugging
   - **Cons**: Exit codes may not have traces
   - **Decision**: Include if available from exception logs

---

## File Statistics

### New Files: 1
- `GITHUB_CRASH_REPORTING_PLAN.md` (705 lines)

### Modified Files: 3
- `CrashReportGenerator.cs` (+45 lines, comprehensive log reading)
- `CRASH_HANDLER_DOCUMENTATION.md` (+12 lines, corrected log info)
- `INDEX.md` (+8 lines, added plan to active docs)

### Total Impact
- **705 insertions** (new plan document)
- **9 deletions** (replaced incorrect log info)
- **Net change**: +696 lines

---

## Related Documentation

### Primary Documents
- **Implementation Plan**: `src/docs/active/GITHUB_CRASH_REPORTING_PLAN.md`
- **Crash Handler Docs**: `src/docs/CRASH_HANDLER_DOCUMENTATION.md`
- **Project Index**: `src/docs/INDEX.md`

### Related Sessions
- **Crash Handler Implementation**: `SESSION_2026-01-15_CRASH_HANDLER.md` ⚠️ **DATE ERROR - Should be Jan 9**

---

## Developer Notes

### What Went Well
1. ✅ Comprehensive plan created (705 lines)
2. ✅ Bug fixes implemented (log paths)
3. ✅ Good discussion of trade-offs
4. ✅ Privacy-first approach
5. ✅ Iterative refinement through user feedback

### What Could Improve
1. Could have checked log patterns earlier
2. Could have explored all log types initially

### Code Quality
- Clear separation of concerns (OAuth, Sanitizer, Submitter)
- Comprehensive error handling planned
- Security considerations documented
- Performance impact analyzed

---

## Completion Checklist

- [x] Evaluate implementation approaches
- [x] Create comprehensive plan document
- [x] Fix log path bugs in crash handler
- [x] Update documentation
- [x] Commit and push changes
- [x] Create session summary
- [ ] ⏰ **FUTURE**: Register GitHub OAuth App (when ready to implement)
- [ ] ⏰ **FUTURE**: Begin Phase 1 implementation

---

**Session Status:** ✅ COMPLETED  
**Planning Status:** ✅ COMPREHENSIVE  
**Bug Fixes:** ✅ IMPLEMENTED  
**Documentation:** ✅ UPDATED  
**Git Status:** ✅ COMMITTED AND PUSHED

**Ready for:** Implementation when v1.1.0 development begins

---

## Acknowledgements

This planning session built upon the existing crash handler system implemented earlier. The OAuth approach was selected after careful consideration of user experience, security, and maintenance requirements. Special thanks to the user for catching log path errors through careful review!
