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

### 3. WAPS Lobby and State Change Updates 🔴
**Status**: Not Started  
**Estimated Time**: 4-5 hours  
**Priority**: HIGH

**Requirements**:

**Application Start & Lobby States**:
- [ ] Initial lobby on first entry (allows users to verify/test browser functions)
- [ ] Game start on "Host Intro" → Transition to Waiting Lobby
- [ ] New users after game start → Automatically enter Waiting Lobby

**FFF Game Flow (9 states)**:
- [ ] State 1: "Pick Player" clicked → FFF Lobby ("Get ready to play!")
- [ ] State 2: Question reveal → Display question and answer options
- [ ] State 3: Timer expires with response → "Calculating your response..."
- [ ] State 3a: Timer expires without response → "Thanks for participating!"
- [ ] State 4: Correct order revealed → Show result ("Correct!" or "Incorrect") with time if correct
- [ ] State 5: Winner revealed → Winner: "You Win! Head up to the stage to play Who Wants to be a Millionaire!"
- [ ] State 5a: Non-winners → "Thanks for participating!"
- [ ] State 6: FFF Control Panel closed → Return all to Waiting Lobby

**ATA (Ask the Audience) Flow (4 states)**:
- [ ] State 1: ATA activated → "Get ready to vote!"
- [ ] State 2: Voting begins → Display question and 4 answers with vote buttons
- [ ] State 3: Submit vote → User can select one answer and submit
- [ ] State 4: Voting complete → Display results graph with user's vote highlighted
- [ ] State 5: ATA complete → Return to Waiting Lobby

**Game Complete**:
- [ ] Display "Thank you for participating! Please close your browser to clear this from your device."
- [ ] Auto-disconnect from web service
- [ ] Clear cache on browser close or 10-minute timer
- [ ] Force window close if possible

**Technical Implementation**:
- [ ] Create GameStateType enum (Lobby, Waiting, FFFActive, FFFCalculating, FFFResults, ATAReady, ATAVoting, ATAResults, GameComplete)
- [ ] Implement SignalR hub method: BroadcastGameState(GameStateType state, object data)
- [ ] Web client JavaScript: Handle state transitions and update UI accordingly
- [ ] Update ControlPanelForm/FFFControlPanel to broadcast state changes
- [ ] Update LifelineManager to broadcast ATA state changes
- [ ] Test state synchronization with 10+ concurrent clients

**Acceptance Criteria**:
- All web clients receive state updates in real-time
- UI transitions smoothly between game states
- No clients stuck in incorrect states
- Clear visual feedback at every stage
- Automatic cleanup on game completion

**Blockers**: None (SignalR infrastructure complete)

---

## 🔧 Important Improvements (Should Complete)

### 4. FFF Online Graphics Enhancement 🟢
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

## 📦 Completed Tasks (Archived)

### ✅ Question Editor CSV Features
**Status**: COMPLETE  
**Completed**: December 27, 2025  
**Time Taken**: ~45 minutes  
**Priority**: LOW-MEDIUM

**Implemented**:
- [x] CSV Import button (ImportQuestionsForm.cs)
- [x] CSV Export button (ExportQuestionsForm.cs)
- [x] CSV format validation on import
- [x] Error handling for malformed files
- [x] Proper CSV escaping (quotes, commas)
- [x] ATA percentages in export
- [x] Error reporting with line numbers

**Result**: Users can now import and export questions via CSV with full validation and error reporting.

---

### ✅ Sound Pack Removal
**Status**: COMPLETE  
**Completed**: December 27, 2025  
**Time Taken**: ~15 minutes  
**Priority**: LOW

**Implemented**:
- [x] "Remove Sound Pack" in OptionsDialog.cs
- [x] Confirmation dialog before removal
- [x] Restore default sounds if current pack removed
- [x] Update UI to reflect removal
- [x] Protection for Default pack (cannot be removed)

**Result**: Users can remove installed sound packs with proper confirmation and automatic fallback to Default.

---

## 🔍 Testing & Quality Assurance

### 5. End-to-End Testing
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

### 7. End-to-End Testing
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
- [ ] Web client state transitions (all lobby states)

**Performance Tests**:
- [ ] 50+ concurrent web participants
- [ ] Answer submission load testing
- [ ] Vote aggregation accuracy with high concurrency
- [ ] Memory leaks during extended gameplay
- [ ] State synchronization with rapid state changes

**Compatibility Tests**:
- [ ] Windows 10 x64
- [ ] Windows 11 x64
- [ ] SQL Server Express 2019+
- [ ] SQLite database operations
- [ ] Multiple browsers (Chrome, Edge, Firefox, Safari)
- [ ] Mobile devices (iOS, Android)

---

### 6. Bug Fixes
**Estimated Time**: 4 hours  
**Reserved for issues found during testing**

---

### 7. Documentation Updates
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
| Critical Path | 3 | 0 | 3 | 0% |
| Important | 1 | 0 | 1 | 0% |
| Testing & QA | 3 | 0 | 3 | 0% |
| Completed (Archived) | 2 | 2 | 0 | 100% |
| **Total** | **9** | **2** | **7** | **22%** |

**Estimated Total Hours**: 19-27 hours  
**Target Completion**: January 15, 2026 (assuming 10-15 hours/week)

**Note**: FFF Online mostly complete (~80%), only TV animations and web client state integration remaining.

---

## 📋 Weekly Milestones

### Week 1 (Dec 29 - Jan 4)
- [ ] WAPS Lobby and State Updates - Phase 1 (3 hours): Enum, SignalR methods, basic states
- [ ] FFF Online TV Screen Animations (2 hours)
- [ ] Real ATA Voting Integration (3 hours)

**Target**: 8 hours, 40% complete

### Week 2 (Jan 5 - Jan 11)
- [ ] WAPS Lobby and State Updates - Phase 2 (2 hours): FFF state flow, ATA flow
- [ ] FFF Online Web Client State Integration (1 hour)
- [ ] FFF Online Graphics Enhancement (3 hours)
- [ ] End-to-End Testing - Phase 1 (2 hours)

**Target**: 8 hours, 70% complete

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
- [ ] WAPS lobby and state management complete (all 13+ states)
- [ ] FFF Online TV animations complete
- [ ] FFF Online web client state integration complete
- [ ] Real ATA voting displays accurate percentages
- [ ] All lifelines function correctly
- [ ] FFF Offline and Online modes work independently
- [ ] Main game flow from Q1-Q15 works flawlessly
- [ ] Web clients transition smoothly between all game states

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
- WAPS lobby state management (complex state machine with 13+ states)
- End-to-end testing (may reveal unexpected issues)

### High Risk
- None

### Critical Path Dependencies
- FFF Online Graphics Enhancement depends on TV Animations completion
- WAPS state management affects both FFF and ATA flows
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
- Added Task #6: WAPS Lobby and State Change Updates (4-5 hours, HIGH priority)
  * 13+ distinct client states across application, FFF, ATA, and game completion
  * Comprehensive state machine for web client experience
  * Automatic lobby management and cleanup
- Renumbered Testing & QA tasks (7, 8, 9)
- Updated progress tracking: 9 total tasks, 2 complete (22% done), 19-27 hours remaining
- Adjusted weekly milestones to include WAPS state work
- Updated Definition of Done to include state management
- Updated risk assessment with WAPS state complexity
- Extended target completion to January 15, 2026

**Previous Updates**:
- Marked FFF Online as mostly complete (~80% done)
- Remaining: TV screen animations and web client state integration
- Moved "Hotkey Mapping" to Post-1.0 (will develop with Stream Deck integration)
- Moved "Lifeline Icon Polish" to Post-1.0 (eliminated - current setup sufficient)
- Marked CSV Import/Export complete (Task #4)
- Marked Sound Pack Removal complete (Task #5)
