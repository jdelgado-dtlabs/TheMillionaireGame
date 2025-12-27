# Pre-1.0 Final Checklist
**The Millionaire Game - C# Edition**  
**Target Version**: 1.0  
**Estimated Completion**: Mid-January 2026

---

## 🎯 Critical Path (Must Complete for v1.0)

### 1. FFF Online as "Game Within a Game" Feature �
**Status**: Mostly Complete - TV Animations & Web Client State Integration Remaining  
**Estimated Time**: 2-3 hours (remaining work)  
**Priority**: MEDIUM-HIGH

**Completed**:
- [x] FFF Online platform fully built with WAPS integration
- [x] Control panel integration complete
- [x] Separation between FFF Offline (local) and FFF Online (web)
- [x] FFFControlPanel wired into main Control Panel
- [x] End-to-end flow tested: Participants join → Question starts → Answers submitted → Winner calculated
- [x] WAPS infrastructure complete and operational

**Remaining Requirements**:
- [ ] Implement TV screen animations for FFF Online
- [ ] Integrate FFF Online process states into web client UI
  * Show current phase (waiting, question active, answering, results)
  * Display state transitions to participants
  * Visual feedback for each game phase

**Acceptance Criteria**:
- TV screen displays animated FFF Online sequences
- Web participants see current FFF phase on their devices
- State changes are communicated clearly to all participants
- Smooth transitions between FFF phases

**Blockers**: None

---

### 2. Real ATA Voting Integration 🔴
**Status**: Not Started  
**Estimated Time**: 2-3 hours  
**Priority**: HIGH

---

**Requirements**:
- [ ] Replace placeholder results in LifelineManager.cs line 491
- [ ] Query WAPS database for real voting results
- [ ] Display actual vote percentages on all screens (Host, Guest, TV)
- [ ] Test with multiple concurrent voters (2-50 participants)
- [ ] Validate vote aggregation accuracy
- [ ] Ensure percentages add up to 100%
- [ ] Handle edge cases (0 votes, tied percentages)

**Acceptance Criteria**:
- ATA lifeline shows real participant votes
- Percentages reflect actual voting distribution
- Results update in real-time as votes come in
- No placeholder "100% correct answer" results

**Blockers**: None (ATA voting system complete)

---

## 🔧 Important Improvements (Should Complete)

### 3. FFF Online Graphics Enhancement 🟢
**Status**: Partially Complete (Offline graphics done)  
**Estimated Time**: 3-4 hours  
**Priority**: MEDIUM

**Requirements**:
- [ ] Wire up FFFGraphics.cs for FFF Online mode
- [ ] Implement TV screen rendering for FFF Online questions
- [ ] Show contestant straps during question
- [ ] Animate selection/highlighting when winner determined
- [ ] Match visual style between offline and online modes
- [ ] Test with 2-8 participants

**Acceptance Criteria**:
- FFF Online displays contestants with graphic straps (not colored rectangles)
- Winner is visually highlighted
- Consistent look with FFF Offline mode

**Blockers**: Depends on FFF Online integration (Task #1)

---

### 4. Question Editor CSV Features 🟢
**Status**: UI exists, functionality incomplete  
**Estimated Time**: 2-3 hours  
**Priority**: LOW-MEDIUM

**Requirements**:
- [ ] Implement CSV Import button (ImportQuestionsForm.cs)
- [ ] Implement CSV Export button (ExportQuestionsForm.cs)
- [ ] CSV format validation on import
- [ ] Error handling for malformed files
- [ ] Duplicate question detection
- [ ] Preview imported questions before saving
- [ ] Export with filters (by level, difficulty)

**Acceptance Criteria**:
- Users can import questions from CSV file
- Users can export questions to CSV file
- Format matches existing CSV structure
- Errors are reported clearly

**Blockers**: None

---

### 5. Sound Pack Removal 🟢
**Status**: Not Implemented  
**Estimated Time**: 1 hour  
**Priority**: LOW

**Requirements**:
- [ ] Implement "Remove Sound Pack" in OptionsDialog.cs:971
- [ ] Confirmation dialog before removal
- [ ] Restore default sounds if current pack removed
- [ ] Update UI to reflect removal
- [ ] Test removal and restoration

**Acceptance Criteria**:
- Users can remove installed sound packs
- Default sounds are restored automatically
- No broken sound references after removal

**Blockers**: None

---

## 🔍 Testing & Quality Assurance

### 6. End-to-End Testing
**Estimated Time**: 4 hours

**Test Scenarios**:
- [ ] Complete game from question 1 to win (£1,000,000)
- [ ] Complete game with walk away
- [ ] Complete game with wrong answer
- [ ] All lifelines (50:50, PAF, ATA) with real voting
- [ ] FFF Offline with 2-8 players
- [ ] FFF Online with web participants
- [ ] Risk Mode gameplay
- [ ] Free Safety Net Mode
- [ ] Monitor selection and full-screen
- [ ] Sound playback for all cues
- [ ] Settings persistence and loading

**Performance Tests**:
- [ ] 50+ concurrent web participants
- [ ] Answer submission load testing
- [ ] Vote aggregation accuracy with high concurrency
- [ ] Memory leaks during extended gameplay

**Compatibility Tests**:
- [ ] Windows 10 x64
- [ ] Windows 11 x64
- [ ] SQL Server Express 2019+
- [ ] SQLite database operations

---

### 7. Bug Fixes
**Estimated Time**: 4 hours  
**Reserved for issues found during testing**

---

### 8. Documentation Updates
**Estimated Time**: 2 hours

**Documents to Update**:
- [ ] README.md → Version 0.7.0, reflect WAPS completion
- [ ] DEVELOPMENT_CHECKPOINT.md → Add Phase 5.2, update to v0.7.0
- [ ] WEB_SYSTEM_IMPLEMENTATION_PLAN.md → Mark all phases complete
- [ ] CHANGELOG.md → Final v1.0 entry
- [ ] Create TESTING_PLAN.md
- [ ] Create USER_GUIDE.md or update existing documentation

---

## ⏸️ Deferred to Post-1.0

These items are explicitly NOT required for v1.0 release:

### Hotkey Mapping for Lifelines
- Implement F8-F11 → Lifeline Buttons 1-4
- **Reason**: Will be developed in tandem with Elgato Stream Deck integration
- **Post-1.0 Priority**: Medium (v1.2 target)
- **Reference**: HotkeyHandler.cs lines 135, 139, 143, 147

### Lifeline Icon Polish
- Complete lifeline image loading, visual updates when used
- **Reason**: Current setup works well, additional complexity not needed
- **Post-1.0 Priority**: None (eliminated)
- **Reference**: ControlPanelForm.cs lines 307, 682

### Multi-Session Support
- Replace hardcoded "LIVE" session ID
- **Reason**: User confirmed "not needed" for v1.0
- **Post-1.0 Priority**: Low-Medium (nice to have)

### Database Schema Enhancement
- Column renaming for randomized answer order
- **Reason**: Optional feature for future flexibility
- **Post-1.0 Priority**: Low

### OBS/Streaming Integration
- Browser source compatibility
- Scene switching automation
- **Reason**: Advanced feature for power users
- **Post-1.0 Priority**: High (v1.1 target)

### Elgato Stream Deck Plugin
- Custom button actions for game control
- **Reason**: External hardware integration
- **Post-1.0 Priority**: Medium (v1.2 target)

### Screen Dimming Feature
- "Lights Down" effect during gameplay
- **Reason**: Marked as "effect unnecessary" in DEVELOPMENT_CHECKPOINT.md
- **Post-1.0 Priority**: None (eliminated)

### Disconnect/Reconnection Handling
- Graceful participant disconnect detection
- Reconnection with state restoration
- **Reason**: Nice-to-have, not critical for gameplay
- **Post-1.0 Priority**: Medium

---

## 📊 Progress Tracking

### Overall Completion Status

| Category | Tasks | Complete | Remaining | % Done |
|----------|-------|----------|-----------|--------|
| Critical Path | 2 | 0 | 2 | 0% |
| Important | 3 | 0 | 3 | 0% |
| Testing & QA | 3 | 0 | 3 | 0% |
| **Total** | **8** | **0** | **8** | **0%** |

**Estimated Total Hours**: 15-22 hours  
**Target Completion**: January 10, 2026 (assuming 10-15 hours/week)

**Note**: FFF Online mostly complete (~80%), only TV animations and web client state integration remaining.

---

## 📋 Weekly Milestones

### Week 1 (Dec 29 - Jan 4)
- [ ] FFF Online TV Screen Animations (2 hours)
- [ ] FFF Online Web Client State Integration (1 hour)
- [ ] Real ATA Voting Integration (3 hours)
- [ ] CSV Import/Export (3 hours)

**Target**: 9 hours, 50% complete

### Week 2 (Jan 5 - Jan 11)
- [ ] FFF Online Graphics Enhancement (3 hours)
- [ ] Sound Pack Removal (1 hour)
- [ ] End-to-End Testing - Phase 1 (2 hours)

**Target**: 6 hours, 75% complete

### Week 3 (Jan 12 - Jan 18)
- [ ] End-to-End Testing - Phase 2 (2 hours)
- [ ] Bug Fixes (4 hours)
- [ ] Documentation Updates (2 hours)
- [ ] Final Polish & Release Prep (1 hour)

**Target**: 9 hours, 100% complete

---

## ✅ Definition of Done

Version 1.0 is ready for release when:

### Functionality
- [ ] FFF Online TV animations complete
- [ ] FFF Online web client state integration complete
- [ ] Real ATA voting displays accurate percentages
- [ ] All lifelines function correctly
- [ ] FFF Offline and Online modes work independently
- [ ] Main game flow from Q1-Q15 works flawlessly

### Quality
- [ ] Zero critical bugs in production code paths
- [ ] No crashes during normal gameplay
- [ ] 50+ concurrent participants tested successfully
- [ ] All TODOs addressed or documented as post-1.0

### Documentation
- [ ] README.md reflects v1.0 feature set
- [ ] CHANGELOG.md has complete v1.0 entry
- [ ] User guide covers all features
- [ ] Developer documentation up to date

### Polish
- [ ] All screens render correctly at 1920x1080
- [ ] Sounds play without glitches
- [ ] UI is responsive and intuitive
- [ ] No placeholder text or debug messages visible

---

## 🚨 Risk Assessment

### Low Risk
- FFF Online web client state integration (SignalR already implemented)
- CSV Import/Export (standard file I/O)
- Sound pack removal (straightforward feature)

### Medium Risk
- Real ATA voting integration (database query complexity)
- FFF Online TV animations (depends on existing implementation)
- FFF Online graphics enhancement (requires FFFGraphics.cs integration)
- End-to-end testing (may reveal unexpected issues)

### High Risk
- None (FFF Online integration already complete)

### Critical Path Dependencies
- FFF Online Graphics Enhancement depends on TV Animations completion
- End-to-end testing depends on all features being complete
- Release depends on successful testing and bug fixes

---

## 📞 Support & Resources

### Development Environment
- Visual Studio 2022 (17.8+)
- .NET 8.0 SDK
- SQL Server Express 2019+
- Web browser DevTools for PWA testing

### Reference Documentation
- Original VB.NET project for feature parity verification
- WEB_SYSTEM_IMPLEMENTATION_PLAN.md for WAPS architecture
- PHASE_5.2_COMPLETE.md for SignalR patterns
- LIFELINE_REFACTORING_PLAN.md for lifeline system

### Testing Resources
- Multiple devices for PWA testing (phone, tablet, desktop)
- QR code scanner app
- Network monitoring tools for SignalR debugging

---

*Document Created*: December 24, 2025  
*Last Updated*: December 27, 2025  
*Next Review*: Weekly during development

---

## 📝 Changelog

**December 27, 2025**:
- Marked FFF Online as mostly complete (~80% done)
- Remaining: TV screen animations and web client state integration
- Moved "Hotkey Mapping" to Post-1.0 (will develop with Stream Deck integration)
- Moved "Lifeline Icon Polish" to Post-1.0 (eliminated - current setup sufficient)
- Reduced total task count from 10 to 8
- Updated time estimates: 15-22 hours remaining (down from 20-30 hours)
- Adjusted weekly milestones to reflect changes
- Updated risk assessment (removed "High Risk" category)
