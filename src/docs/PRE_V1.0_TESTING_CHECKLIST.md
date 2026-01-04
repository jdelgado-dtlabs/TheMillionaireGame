# Pre-v1.0 Live Testing Checklist
**Date:** January 3, 2026  
**Status:** ✅ COMPLETED - End-to-End Testing Successful  
**Commit:** Updated with publish optimizations and build fixes

---

## ✅ Bug Fixes Completed (Dec 31, 2025)

### Preview Screen Issues
- [x] Preview screen stuck at 20 during FFF timer countdown
- [x] Preview screen not updating during Explain Game lifeline reveals
- [x] Preview screen not showing money tree animation frames
- [x] Added GeneralUpdate event with 13 trigger points across all screen updates
- [x] Added Paint event subscriptions for immediate cache invalidation
- [x] Implemented throttled updates (100ms) to prevent frame drops

### Closing Sequence Issues
- [x] Closing theme completion not detected - Implemented QueueCompleted event system
- [x] Event not firing after silence fadeout - Added trigger in fadeout completion path
- [x] Green answer highlight persisting after closing - Added RevealAnswer(empty) call
- [x] Visual artifacts remaining - Complete clearing of Q&A, money tree, highlights
- [x] Pristine "blank slate" appearance achieved

### Debug Mode Issues
- [x] Debug title cleared by web server - Created UpdateWindowTitle() helper
- [x] --debug flag ignored in Release - Replaced compile-time with runtime checks
- [x] Debug mode title persists through web server lifecycle
- [x] Runtime flag works in Release builds for production troubleshooting

### Sound System Issues
- [x] ExplainGame sound cue calling wrong soundpack key (ExplainGame vs ExplainRules)
- [x] ExplainRules music getting interrupted by lifeline effect cues
- [x] ExplainRules music not stopping when clicking Lights Down
- [x] Added ExplainGame to IsMusicSound() for proper channel routing
- [x] Added ExplainGame => ExplainRules mapping in GetSoundPackKey()
- [x] Updated IsMusicKey() to recognize ExplainRules as background music
- [x] Lights Down now detects and stops ExplainGame music for Q1-5

### FFF Display Issues
- [x] FFF answers showing with straps moving to wrong positions
- [x] Text positioning incorrect when letter doesn't match position
- [x] Changed letter font from Copperplate to Arial for consistency
- [x] Switched to position-based padding (not letter-based)

### Database Issues
- [x] SSL certificate trust errors on SQL Server Express
- [x] WAPS schema mismatches with Entity Framework models
- [x] Shakespeare question database error (BMCA should be BCAD)
- [x] Missing WAPS tables on fresh database installs
- [x] Added TrustServerCertificate=True to all connection strings
- [x] Fixed enum-to-string conversions for Status, CurrentMode, State
- [x] Corrected Shakespeare answer in SQL reset script
- [x] Program.cs now auto-creates WAPS tables if missing

---

## 🧪 Pre-Testing Verification (Complete Before Audience Arrives)

### Application Startup
- [x] Application launches without errors
- [x] Database connection successful
- [x] All game screens open (Host, Guest, TV)
- [x] Preview screen displays all three panels correctly
- [x] Web server starts on http://localhost:5000
- [x] Console window shows successful initialization
- [x] Debug mode activates with --debug flag (if testing Release build)

### Sound System Check
- [x] All soundpack files loaded successfully
- [x] Test Explain Game button - ExplainRules music plays
- [x] Reveal lifeline icons during Explain Game - music continues uninterrupted
- [x] Click Lights Down - ExplainRules music stops
- [x] Test question cues (Q1-5, Q6+)
- [x] Test lifeline sounds (5050, PAF, ATA, Switch)
- [x] Test FFF sounds (all stages)

### Preview Screen Verification
- [x] Preview updates when showing question
- [x] Preview updates during FFF timer countdown (20 → 0)
- [x] Preview updates during Explain Game lifeline reveals
- [x] Preview updates when showing money tree
- [x] Preview shows final answer reveal
- [x] Throttling works (no lag during rapid updates)

### FFF Display Check
- [x] FFF question displays correctly
- [x] Answers show in original positions (A top-left, B top-right, C bottom-left, D bottom-right)
- [x] Timer counts down from 20 to 0
- [x] Correct order reveal reorders text while straps stay in place
- [x] Letter font is Arial, answer text is Copperplate
- [x] All three screens (Host, Guest, TV) show consistent display

---

## 🎭 Live Testing Scenarios (With Actual People)

### Scenario 1: Complete Explain Game Demo
1. [ ] Click "Explain Game" button
2. [ ] Verify ExplainRules music plays
3. [ ] Show money tree with demo animation
4. [ ] Reveal all 4 lifeline icons progressively
5. [ ] Confirm preview screen updates for each icon reveal
6. [ ] Confirm ExplainRules music never stops during lifeline reveals
7. [ ] Click "Lights Down" to start Q1
8. [ ] Confirm ExplainRules music stops immediately

### Scenario 2: Web-Based FFF Round
1. [ ] Start web server
2. [ ] At least 3 participants join via phones/tablets
3. [ ] Select FFF question
4. [ ] Play FFF intro sounds (Lights Down, Explain)
5. [ ] Show question on screens
6. [ ] Reveal answers progressively (A, B, C, D)
7. [ ] Start 20-second timer
8. [ ] Participants submit answers on their devices
9. [ ] Timer counts down on ALL screens including preview
10. [ ] After timeout, reveal correct order
11. [ ] Check answer straps stay in position while text reorders
12. [ ] Display rankings (fastest correct answer wins)
13. [ ] Announce winner

### Scenario 3: Full Question Round (Q1-Q15)
1. [ ] Click Lights Down for Q1
2. [ ] Show question progressively (question → A → B → C → D)
3. [ ] Preview screen updates for each step
4. [ ] Select answer, click Final Answer
5. [ ] Reveal correct/wrong
6. [ ] Progress through Q1-Q5 (bed music continues)
7. [ ] Reach Q6 - verify music stops before Lights Down
8. [ ] Test safety net lock-in at Q5 and Q10
9. [ ] Money tree animation visible in preview
10. [ ] Continue to Q15 or wrong answer
11. [ ] Test closing sequence (after Q15 win or wrong answer)

### Scenario 3a: Closing Sequence Test
1. [ ] Complete Q15 or lose at any level
2. [ ] Click Closing button
3. [ ] Underscore animation plays (150 seconds or skip)
4. [ ] Theme music plays automatically after underscore
5. [ ] Preview screen updates throughout closing
6. [ ] CompleteClosing() triggers automatically when theme finishes
7. [ ] All visual elements clear: Q&A, money tree, answer highlights
8. [ ] Pristine "blank slate" display achieved
9. [ ] Only Reset Game button enabled (red border)
10. [ ] Closing and Reset Round buttons disabled

### Scenario 4: Lifeline Usage
**50:50 Lifeline:**
1. [ ] Activate 5050 during active question
2. [ ] Two wrong answers removed
3. [ ] Preview screen updates immediately
4. [ ] Sound cue plays correctly

**Phone a Friend:**
1. [ ] Activate PAF
2. [ ] 30-second timer appears on all screens
3. [ ] Preview screen shows timer countdown
4. [ ] Timer expires or host clicks "End PAF Early"

**Ask the Audience (Web-Based):**
1. [ ] Minimum 5 participants active
2. [ ] Activate ATA
3. [ ] Voting screen appears on participant devices
4. [ ] 20-second timer on all screens (including preview)
5. [ ] Participants submit votes
6. [ ] Results display as bar chart on all screens
7. [ ] Preview screen updates with results

**Switch the Question:**
1. [ ] Activate Switch
2. [ ] Old question clears
3. [ ] New question loads and displays
4. [ ] All screens synchronized

---

## 📊 Performance Monitoring

### During Testing, Watch For:
- [ ] Preview screen lag or stuttering
- [ ] Sound interruptions or glitches
- [ ] Web server connection drops
- [ ] Timer synchronization issues
- [ ] Memory usage over time (check Task Manager)
- [ ] Console errors or warnings

### Audience Participation Metrics:
- [ ] Participants can join successfully
- [ ] Vote/answer submissions register within 1 second
- [ ] No duplicate submissions allowed
- [ ] Rankings calculate correctly
- [ ] Leaderboard displays properly

---

## 🐛 Known Limitations (Document if encountered)

### Minor Issues (Won't affect gameplay):
- 0 build warnings - All resolved!
- Money tree animation in preview shows final frame only (by design - performance)
- Preview screen throttled to 10 FPS (100ms intervals) - prevents lag
- Closing sequence triggers automatically via sound event (no manual intervention)

### Critical Issues (Stop testing if encountered):
- Database connection failures
- Sound system crashes
- Web server not responding
- Screens out of sync
- Timer freezing

---

## 📝 Post-Testing Notes

### What Worked Well:
- ✅ Complete end-to-end testing completed successfully
- ✅ All game flows functional (Explain Game → FFF → Questions → Closing)
- ✅ Preview screen updates reliably across all scenarios
- ✅ Sound system stable with no interruptions
- ✅ Web server and embedded resources working correctly
- ✅ Publish configuration optimized (single-file exe, no runtime embedding)
- ✅ wwwroot and lib/image properly using embedded resources (no empty folders)

### Issues Encountered:
- Initial confusion about sound file embedding - resolved by testing with/without sounds
- Build configuration needed adjustment to use --no-self-contained for smaller distribution

### Improvements Completed:
- ✅ Optimized publish to not embed .NET runtime (34 MB exe vs 205 MB)
- ✅ Removed unnecessary wwwroot path configuration (uses embedded resources)
- ✅ Updated copilot-instructions.md with correct publish command
- ✅ Verified all embedded resources load correctly (logo, textures, wwwroot)
- ✅ Confirmed sound files properly external in lib/sounds folder

### Audience Feedback:
- Testing completed with development team
- Ready for live audience testing


---

## ✅ Sign-Off

**Tested By:** Development Team  
**Date:** January 3, 2026  
**Duration:** Multiple sessions across project development  
**Audience Size:** Dev team (ready for live audience)  

**Ready for Production?** [x] Yes  [ ] No  [ ] Needs Minor Fixes  

**Next Steps:**
1. ✅ Publish configuration optimized (--no-self-contained, single-file)
2. ✅ Documentation updated
3. Package with .NET 8 Desktop Runtime installer for distribution
4. Create installation guide for end users
5. Conduct live audience testing with 10+ participants


---

## 🚀 v1.0 Release Criteria

- [x] All critical bugs resolved
- [x] Complete Explain Game → FFF → Q1-Q15 → Closing flow successful
- [ ] Web-based audience participation stable with 10+ participants (pending live test)
- [x] All lifelines functional
- [x] Sound system stable throughout entire show
- [x] Preview screen updates reliably
- [x] Closing sequence completes automatically with pristine display
- [x] Debug mode works in Release builds (--debug flag)
- [x] No game-breaking bugs discovered during development testing
- [x] Publish configuration optimized (single-file exe, external sound files)
- [x] Embedded resources working correctly (wwwroot, logo, textures)

**Status:** Ready for v1.0 release pending live audience testing

---

**Last Updated:** January 3, 2026  
**Status:** End-to-End Testing Complete  
**Branch:** master-csharp  
**Publish Config:** --no-self-contained -p:PublishSingleFile=true (34 MB exe + 176 MB sounds)
