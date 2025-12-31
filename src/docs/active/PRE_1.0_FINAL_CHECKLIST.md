# Pre-1.0 Final Checklist
**The Millionaire Game - C# Edition**  
**Target Version**: 1.0  
**Estimated Completion**: Mid-January 2026

---

## 🎯 Critical Path (Must Complete for v1.0)

### 1. Host Notes/Messaging System ✅
**Status**: ✅ COMPLETE  
**Completed**: December 30, 2025  
**Time Taken**: ~2.5 hours  
**Priority**: HIGH

**Description**: Real-time messaging system from Control Panel to Host Screen

**Completed**:
- [x] Event-based messaging infrastructure (HostMessageEventArgs)
- [x] Control Panel UI: Multi-line TextBox + Send button
- [x] Keyboard shortcuts: Enter to send, Alt+Enter for newline
- [x] Host Screen display: Semi-transparent message box (300-400px)
- [x] Message validation (no empty messages)
- [x] Thread-safe UI updates with BeginInvoke()
- [x] Non-blocking message delivery
- [x] Message persistence during question changes
- [x] Preview window integration with event subscription

**Technical Implementation**:
- MessageSent event in ControlPanelForm
- HostScreenForm.OnMessageReceived() method
- Message box at (180, 570) with dynamic height
- Explanation box at (180, 490) for question clues
- All 80 questions updated with contextual explanations

**Result**: Fully functional host messaging system operational. Messages appear instantly on Host Screen with no UI blocking. Explanation system provides contextual clues for hosts during gameplay.

**Reference**: HOST_NOTES_SYSTEM_PLAN.md, CHANGELOG.md (v0.8.1-2512)

---

### 2. ATA Dual-Mode System (Online/Offline) ✅
**Status**: ✅ COMPLETE  
**Completed**: December 30, 2025  
**Time Taken**: 3.5 hours  
**Branch**: feature/ata-dual-mode (commit ee6d006)

**Architecture**: Two-mode system similar to FFF (Online/Offline)

#### **Phase 1: ATA Offline Enhancement** ✅ COMPLETE
**Improve placeholder results to simulate realistic audience voting**

**Completed**:
- [x] Modified GeneratePlaceholderResults() → GenerateATAPercentages() in Question.cs
- [x] Correct answer gets 40-70% of votes (random within range)
- [x] Remaining 20-60% spread across incorrect answers
- [x] Percentages always sum to 100%
- [x] Maintains smooth display animations
- [x] Existing offline functionality intact
- [x] Updated all screen forms (Host, Guest, TV) to use dynamic generation
- [x] Removed hardcoded ATA percentage columns from database

**Result**: Realistic voting distribution with correct answer favored but not 100%. Works offline without web server.

#### **Phase 2: ATA Online Implementation** ✅ COMPLETE
**Real-time voting with WAPS database integration**

**Completed**:
- [x] Create ATAOnline mode detection (check web server running)
- [x] Query WAPS database for real-time vote counts
- [x] Implement vote aggregation service in SessionService.cs
- [x] Display actual percentages as votes come in
- [x] Update results in real-time on all screens (Host, Guest, TV)
- [x] Test with multiple concurrent voters (3 clients tested, scales to 50+)
- [x] Handle edge cases (0 votes fallback to offline mode)
- [x] Graceful fallback to offline mode if web server unavailable
- [x] Multi-phase voting flow (Intro 120s → Voting 60s → Results persist until answer)
- [x] Vote persistence with duplicate prevention
- [x] Auto-completion when all participants voted
- [x] Hub consolidation (GameHub replaces FFFHub + ATAHub)
- [x] Session persistence and auto-reconnection
- [x] Results clear when answer selected (not auto-hide)

**Technical Implementation**:
- SignalR Events: ATAIntroStarted, VotingStarted, VotesUpdated, VotingEnded, ATACleared
- Database: ATAVotes table with SessionId, ParticipantId, SelectedOption, QuestionText
- Service scope pattern prevents DbContext disposal errors
- ClearATAFromScreens() method called on answer selection

**Result**: Full dual-mode ATA system operational with real-time voting from web clients. Tested with multiple simultaneous voters.

---

### 3. WAPS Lobby and State Change Updates ✅
**Status**: ✅ COMPLETE  
**Completed**: December 31, 2025  
**Time Taken**: ~2 hours  
**Priority**: HIGH

**Completed**:

**Application Start & Lobby States**:
- [x] Initial lobby on first entry (allows users to verify/test browser functions)
- [x] Game start on "Host Intro" → Transition to Waiting Lobby
- [x] New users after game start → Automatically enter Waiting Lobby

**FFF Game Flow (9 states)**:
- [x] State 1: "Pick Player" clicked → FFF Lobby ("Get ready to play!")
- [x] State 2: Question reveal → Display question and answer options
- [x] State 3: Timer expires with response → "Calculating your response..."
- [x] State 3a: Timer expires without response → "Thanks for participating!"
- [x] State 4: Correct order revealed → Show result ("Correct!" or "Incorrect") with time if correct
- [x] State 5: Winner revealed → Winner: "You Win! Head up to the stage to play Who Wants to be a Millionaire!"
- [x] State 5a: Non-winners → "Thanks for participating!"
- [x] State 6: FFF Control Panel closed → Return all to Waiting Lobby

**ATA (Ask the Audience) Flow (4 states)**:
- [x] State 1: ATA activated → "Get ready to vote!"
- [x] State 2: Voting begins → Display question and 4 answers with vote buttons
- [x] State 3: Submit vote → User can select one answer and submit
- [x] State 4: Voting complete → Display results graph with user's vote highlighted
- [x] State 5: ATA complete → Return to Waiting Lobby

**Game Complete**:
- [x] Display "Thank you for participating! Please close your browser to clear this from your device."
- [x] Auto-disconnect from web service
- [x] Clear cache on browser close or 10-minute timer
- [x] Force window close if possible

**Technical Implementation**:
- [x] Create GameStateType enum (Lobby, Waiting, FFFActive, FFFCalculating, FFFResults, ATAReady, ATAVoting, ATAResults, GameComplete)
- [x] Implement SignalR hub method: BroadcastGameState(GameStateType state, object data)
- [x] Web client JavaScript: Handle state transitions and update UI accordingly
- [x] Update ControlPanelForm/FFFControlPanel to broadcast state changes
- [x] Update LifelineManager to broadcast ATA state changes
- [x] Test state synchronization with 10+ concurrent clients

**Result**: Complete lobby state management system operational. All web clients receive real-time state updates with smooth UI transitions. No clients stuck in incorrect states. Automatic cleanup on game completion works as expected.

---

### 4. Winner Celebration Animation (Confetti) ✅
**Status**: ✅ COMPLETE  
**Completed**: December 31, 2025  
**Time Taken**: ~3 hours  
**Priority**: MEDIUM

**Description**: Animated confetti celebration effect for game winners (Q11+)

**Completed**:
- [x] Physics-based particle system (100 particles per screen)
- [x] Velocity, rotation, gravity, and respawning mechanics
- [x] Question level-based triggering (Q11+ only, walk away at Q10 gives Q9 prize)
- [x] System.Threading.Timer for reliable animation (bypasses Windows Forms message pump)
- [x] Thread-safe UI updates with Invoke()
- [x] Reset button integration and state clearing
- [x] Performance optimization (15 FPS, reduced from initial 33 FPS)
- [x] IsPreview flag to disable confetti on preview screens
- [x] Fixed Q11-14 animation freeze (timer type issue)
- [x] Reduced EffectsMixer logging frequency (1/50 calls)

**Technical Implementation**:
- ConfettiParticle class: X, Y, VelocityY, VelocityX, Rotation, RotationSpeed, Color, Size
- Timer: System.Threading.Timer at 67ms interval (15 FPS)
- InitializeConfetti(): Creates 100 particles starting Y=-500 to 0 (above screen)
- UpdateConfetti(): Updates positions, respawns at top when Y>1080
- IGameScreen.IsPreview property: Skips confetti on preview screen instances

**Performance Notes**:
- Preview screen was rendering 3 full screens simultaneously (300 total particles)
- Added IsPreview flag to TVScreenFormScalable, GuestScreenForm, HostScreenForm, TVScreenForm
- Preview instances marked with IsPreview=true to skip intensive animations
- Primary TV screen still gets full confetti effect

**Result**: Smooth confetti animation for game winners without performance degradation. Preview screen performance significantly improved by disabling confetti on preview instances.

---

### 5. Code Cleanup: TVScreenForm Removal ✅
**Status**: ✅ COMPLETE  
**Completed**: December 31, 2025  
**Time Taken**: ~15 minutes  
**Priority**: LOW-MEDIUM

**Description**: Remove deprecated TVScreenForm.cs and cleanup legacy code references

**Completed**:
- [x] Deleted TVScreenForm.cs (566 lines)
- [x] Deleted TVScreenForm.Designer.cs
- [x] Removed dead type check in ControlPanelForm.cs (line 975-978)
- [x] Verified no instantiation of TVScreenForm anywhere in codebase
- [x] Confirmed all code uses TVScreenFormScalable exclusively

**Analysis**:
- TVScreenForm marked as deprecated with comment "This form is being phased out in favor of TVScreenFormScalable"
- Never instantiated anywhere (only TVScreenFormScalable is created)
- Had legacy type checks checking `if (_tvScreen is TVScreenForm)` that were never true
- Missing features: PAF timer, ATA timer, lifeline icons, FFF display, confetti animation

**Result**: Removed 566+ lines of dead deprecated code. Codebase is cleaner and more maintainable. All TV screen functionality consolidated in TVScreenFormScalable.

---

## 🔬 Testing & Quality Assurance

### 6. Preview Screen Performance Optimization 🔴
**Status**: Not Started  
**Estimated Time**: 2-3 hours  
**Priority**: MEDIUM

**Description**: Optimize preview screen rendering to reduce performance overhead when displaying 3 screens simultaneously

**Current Issue**:
- PreviewPanel renders each screen at full 1920x1080 resolution on every paint
- Creates full bitmap → Calls RenderScreen → Scales down using HighQualityBicubic interpolation
- 3 screens × full resolution rendering = significant performance hit
- Confetti disabled on preview (IsPreview flag), but rendering pipeline still expensive

**Proposed Solutions**:
- [ ] **Option A: Render at lower resolution** - Create bitmap at preview panel size instead of 1920x1080
- [ ] **Option B: Cached rendering** - Only re-render when screen state changes, not on every paint
- [ ] **Option C: Lower quality scaling** - Use NearestNeighbor or Bilinear instead of HighQualityBicubic
- [ ] **Option D: Reduce refresh rate** - Throttle preview panel invalidation to 10-15 FPS
- [ ] **Option E: Simplified rendering mode** - Add RenderPreview() method with reduced detail

**Implementation Notes**:
- PreviewPanel.PreviewPanel_Paint() in PreviewScreenForm.cs (lines 330-412)
- Currently uses reflection to call protected RenderScreen method
- ScaleX/ScaleY factors calculated per-frame in ScalableScreenBase
- DoubleBuffered enabled but still creating new bitmap each frame

**Acceptance Criteria**:
- Preview window renders smoothly with all 3 screens visible
- No noticeable lag or stuttering during animations
- Maintain acceptable visual quality
- CPU usage reduced by 30-50% when preview is open

**Technical Debt**: Consider long-term solution for preview architecture

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

## 📦 Completed Tasks (Archived)

### ✅ FFF Online as "Game Within a Game" Feature
**Status**: ✅ COMPLETE (December 27, 2025)  
**Total Time**: ~8 hours  
**Priority**: MEDIUM-HIGH

**Completed**:
- [x] FFF Online platform fully built with WAPS integration
- [x] Control panel integration complete
- [x] Separation between FFF Offline (local) and FFF Online (web)
- [x] FFFControlPanel wired into main Control Panel
- [x] End-to-end flow tested: Participants join → Question starts → Answers submitted → Winner calculated
- [x] WAPS infrastructure complete and operational
- [x] TV screen animations for FFF Online implemented
- [x] Web client state integration complete with all phases
- [x] Show Winners screen with participant times display
- [x] Winner confirmation screen with time display
- [x] Graceful webserver shutdown with client notification
- [x] Automatic cache clearing on server shutdown
- [x] Detailed logging for webserver lifecycle
- [x] Lifeline state preservation through Lights Down
- [x] Screen menu availability during debug mode
- [x] All FFF SignalR messages implemented (QuestionStarted, TimerExpired, RevealingWinner, WinnerConfirmed, NoWinner, ResetToLobby, ServerShuttingDown)

**Acceptance Criteria**: ✅ ALL MET
- TV screen displays animated FFF Online sequences with participant times
- Web participants see current FFF phase on their devices
- State changes are communicated clearly to all participants
- Smooth transitions between FFF phases
- Proper cleanup on server shutdown

**Blockers**: None

---

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

### 3. Bug Fixes
**Estimated Time**: 4 hours  
**Reserved for issues found during testing**

---

### 4. Documentation Updates
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
| Critical Path | 4 | 1 | 3 | 25% |
| Important | 1 | 0 | 1 | 0% |
| Testing & QA | 3 | 0 | 3 | 0% |
| Completed (Archived) | 2 | 2 | 0 | 100% |
| **Total** | **10** | **3** | **7** | **30%** |

**Estimated Total Hours**: 18.5-27 hours  
**Target Completion**: January 15, 2026 (assuming 10-15 hours/week)

**Note**: FFF Online mostly complete (~80%), only TV animations and web client state integration remaining.

---

## 📋 Weekly Milestones

### Week 1 (Dec 29 - Jan 4)
- [ ] Host Notes/Messaging System (2-3 hours): Event infrastructure, UI components, keyboard handling
- [ ] WAPS Lobby and State Updates - Phase 1 (3 hours): Enum, SignalR methods, basic states
- [ ] FFF Online TV Screen Animations (2 hours)

**Target**: 7-8 hours, 35% complete

### Week 2 (Jan 5 - Jan 11)
- [ ] Real ATA Voting Integration (3 hours)
- [ ] WAPS Lobby and State Updates - Phase 2 (2 hours): FFF state flow, ATA flow
- [ ] FFF Online Web Client State Integration (1 hour)
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
- [ ] Host notes/messaging system functional
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

**December 30, 2025**:
- **COMPLETED**: ATA Phase 1 - Offline Enhancement (GenerateATAPercentages())
  * Implemented dynamic ATA percentage generation (40-70% for correct answer)
  * Updated all screen forms to use new generation method
  * Removed hardcoded ATA_A/B/C/D columns from database
- Added Task #1: Host Notes/Messaging System (2-3 hours, HIGH priority)
  * Real-time messaging from Control Panel to Host Screen
  * Event-based architecture with keyboard shortcuts
  * Non-blocking message delivery
- Renumbered all subsequent tasks (ATA is now #2, WAPS is now #3, etc.)
- Updated progress tracking: 10 total tasks, 2 complete (20% done), 21-30 hours remaining
- Adjusted Week 1 milestone to prioritize Host Notes system
- Updated Definition of Done to include messaging system
- Reference: HOST_NOTES_SYSTEM_PLAN.md

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

