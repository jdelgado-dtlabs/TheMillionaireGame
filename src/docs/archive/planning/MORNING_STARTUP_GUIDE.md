# Morning Startup Guide - December 25, 2025

## Quick Context (Read This First)

**Last Session**: Sound system freezing debugging (7+ failed attempts with NAudio)  
**Decision Made**: Migrate to CSCore library  
**Status**: Planning complete, ready to implement  
**Time Needed**: ~7-9 hours

---

## What Happened Last Night

You spent the evening debugging sound system freezing issues where clicking buttons (Reveal, Question, Lock-in, Lights Down) would freeze the UI. The root cause is NAudio's blocking disposal pattern.

After 7 different approaches failed, we identified that the fundamental issue is architectural - you can't fix it with workarounds. We need to either:
- **Option A**: Refactor NAudio into separate music/effects channels
- **Option B**: Migrate to CSCore (better for future broadcasting)

You chose **Option B (CSCore)** because you confirmed broadcasting to OBS/streaming is a future requirement, and CSCore's architecture is purpose-built for that.

---

## Files to Read This Morning (In Order)

1. **THIS FILE** (you're here) - Quick overview
2. **`SOUND_SYSTEM_REFACTORING_PLAN.md`** (in this directory) - Full implementation plan
3. **`DEVELOPMENT_CHECKPOINT.md`** (in src/) - Project state summary

---

## Commands to Run First

```bash
# 1. Navigate to project
cd c:\Users\djtam\OneDrive\Documents\Coding\Project\Millionaire\TheMillionaireGame

# 2. Make sure you're on master-csharp
git status
git branch

# 3. Create feature branch for CSCore work
git checkout -b feature/cscore-sound-system

# 4. Install CSCore
cd src\MillionaireGame
dotnet add package CSCore

# 5. Verify build still works
cd ..
dotnet build TheMillionaireGame.sln
```

---

## What You're Building

**Two separate audio channels:**
- **Music Channel**: Plays looping bed music (Q1-5 continuous, changes at Q6+)
- **Effects Channel**: Plays one-shot sounds (reveal, correct/wrong, lifelines)

**Using CSCore because:**
- Better async/await support (no blocking)
- Built-in audio mixing
- Easy multi-output routing (for future OBS integration)
- Professional source → mixer → output pipeline

---

## Implementation Phases

**Phase 1** (30 min): Install CSCore, test basic playback  
**Phase 2** (1-2 hrs): Build MusicChannel.cs  
**Phase 3** (1-2 hrs): Build EffectsChannel.cs  
**Phase 4** (1 hr): Create mixer  
**Phase 5** (2 hrs): Update SoundService API  
**Phase 6** (2 hrs): Test everything  

**Total**: 7-9 hours

---

## Critical Requirements (Don't Forget)

### Sound Behavior by Question:
- **Q1-4**: Bed music loops, no final answer sound
- **Q5**: Stop all sounds, then play correct answer
- **Q6+**: Stop bed music when loading new question

### Buttons That Must Not Freeze:
- ✓ Reveal button (any question)
- ✓ Question button (Q6+)
- ✓ Lock-in button (safety net animation)
- ✓ Lights Down button (Q6+)
- ✓ Stop All Audio button

### App Shutdown:
- ✓ Must close cleanly even with sounds playing
- ✓ No zombie processes

---

## What NOT to Do

**DO NOT try these NAudio fixes** (all tested, all failed):
- ❌ Task.Run wrappers
- ❌ Monitor.TryEnter locks
- ❌ Removing Stop() calls
- ❌ Background thread disposal
- ❌ Fire-and-forget disposal
- ❌ Clearing dictionary without disposing
- ❌ Any disposal from event handlers

If you see these patterns in code, they're from failed attempts - remove them during CSCore migration.

---

## If You Get Stuck

### CSCore Problems?
1. Check CSCore documentation/examples
2. Look for sample code on GitHub
3. After 3 major blockers or 12 hours, switch to **Plan A: NAudio multi-channel** (see plan document)

### Questions About Original Behavior?
- Check VB.NET original: `Het DJG Toernooi/` directory
- Sound files: `lib/sounds/` directory
- Current NAudio code: `src/MillionaireGame/Services/SoundService.cs`

---

## Testing Checklist (Phase 6)

When you finish implementation, test these scenarios:

**Music Tests:**
- [ ] Q1-4: Bed music loops without stopping
- [ ] Q5: Sounds stop, correct answer plays
- [ ] Q6+: Music changes when loading new question

**Button Tests:**
- [ ] Reveal button: No freeze (test Q1, Q5, Q10)
- [ ] Question button: No freeze at Q6+
- [ ] Lock-in: No freeze during safety net
- [ ] Lights Down: No freeze at Q6+
- [ ] Stop All Audio: Works instantly

**Stability Tests:**
- [ ] App shutdown: Clean exit with sounds playing
- [ ] Play for 30+ minutes: No memory leaks
- [ ] Multiple button clicks rapidly: No crashes

---

## Success Criteria

You'll know you're done when:
1. ✅ All buttons responsive (no freezing)
2. ✅ Music behaves correctly Q1-15
3. ✅ App closes cleanly
4. ✅ No memory leaks after extended play
5. ✅ Code is cleaner and more maintainable

---

## After Completion

1. Test thoroughly (use checklist above)
2. Update `SOUND_SYSTEM_REFACTORING_PLAN.md` with results
3. Update `DEVELOPMENT_CHECKPOINT.md` with completion
4. Commit changes to feature branch
5. Create PR to master-csharp
6. Tag version after merge

---

## Quick Reference

**Plan Document**: `docs/active/SOUND_SYSTEM_REFACTORING_PLAN.md`  
**Current Sound Code**: `src/MillionaireGame/Services/SoundService.cs`  
**Feature Branch**: `feature/cscore-sound-system`  
**CSCore Docs**: https://github.com/filoe/cscore  

**Good luck! The plan is solid, CSCore is the right choice, and you have clear steps to follow.**
