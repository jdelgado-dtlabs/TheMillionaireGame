# Documentation Archiving Plan
**The Millionaire Game - C# Edition**  
**Date**: December 24, 2025  
**Purpose**: Organize completed planning documents and session logs

---

## 📁 Proposed Archive Structure

```
src/
├── docs/                              (NEW - Create this folder)
│   ├── archive/
│   │   ├── planning/                  (Completed planning documents)
│   │   │   ├── LIFELINE_REFACTORING_PLAN.md
│   │   │   └── PHASE_5_INTEGRATION_PLAN.md
│   │   ├── sessions/                  (Historical session logs)
│   │   │   ├── SESSION_SUMMARY_PHASE_3.md
│   │   │   ├── SESSION_SUMMARY_PHASE_4.md
│   │   │   ├── FFF_OFFLINE_MODE_SESSION.md
│   │   │   └── FFF_GRAPHICS_MIGRATION_PLAN.md
│   │   └── phases/                    (Completed phase documentation)
│   │       ├── PHASE_3_COMPLETE.md
│   │       ├── PHASE_4_PRIVACY_SESSION_MANAGEMENT.md
│   │       ├── PHASE_5_COMPLETE.md
│   │       ├── PHASE_5.0.1_COMPLETE.md
│   │       ├── PHASE_5.1_COMPLETE.md
│   │       └── PHASE_5.2_COMPLETE.md
│   ├── active/                        (Current working documents)
│   │   ├── PROJECT_AUDIT_2025.md      (KEEP - Recent audit)
│   │   ├── PRE_1.0_FINAL_CHECKLIST.md (KEEP - Active checklist)
│   │   └── FFF_ONLINE_FLOW_DOCUMENT.md (TO BE CREATED)
│   └── reference/                     (Keep for development reference)
│       └── WEB_SYSTEM_IMPLEMENTATION_PLAN.md
├── CHANGELOG.md                       (KEEP - Root level)
├── DEVELOPMENT_CHECKPOINT.md          (KEEP - Root level)
├── README.md                          (KEEP - Root level)
└── ARCHIVE.md                         (KEEP - Historical reference)
```

---

## 📦 Documents to Archive

### Phase Documentation (Move to `docs/archive/phases/`)

#### 1. PHASE_3_COMPLETE.md
**Status**: Complete  
**Content**: Lifeline system refactoring  
**Action**: Archive to `docs/archive/phases/`  
**Reason**: Phase 3 is complete and documented in DEVELOPMENT_CHECKPOINT.md

#### 2. PHASE_4_PRIVACY_SESSION_MANAGEMENT.md
**Status**: Complete  
**Content**: Device telemetry and privacy notice implementation  
**Action**: Archive to `docs/archive/phases/`  
**Reason**: Phase 4 is complete and documented in DEVELOPMENT_CHECKPOINT.md

#### 3. PHASE_5_COMPLETE.md
**Status**: Complete  
**Content**: Main application integration (WebServerHost)  
**Action**: Archive to `docs/archive/phases/`  
**Reason**: Phase 5 base is complete, useful as reference but not active

#### 4. PHASE_5.0.1_COMPLETE.md
**Status**: Complete  
**Content**: Session management enhancements  
**Action**: Archive to `docs/archive/phases/`  
**Reason**: Minor phase update, documented elsewhere

#### 5. PHASE_5.1_COMPLETE.md
**Status**: Complete  
**Content**: FFF Control Panel integration  
**Action**: Archive to `docs/archive/phases/`  
**Reason**: Phase 5.1 complete, integrated into main codebase

#### 6. PHASE_5.2_COMPLETE.md
**Status**: Recently completed  
**Content**: FFF Web Participant Interface with rankings  
**Action**: Archive to `docs/archive/phases/`  
**Reason**: Most recent phase, but complete. Keep for 1-2 months then archive.  
**Alternative**: Keep in root until v1.0 release

---

### Planning Documents (Move to `docs/archive/planning/`)

#### 7. LIFELINE_REFACTORING_PLAN.md
**Status**: Implementation complete  
**Content**: Lifeline system architecture and refactoring goals  
**Action**: Archive to `docs/archive/planning/`  
**Reason**: User confirmed all lifelines complete. Keep as architecture reference.  
**Note**: Document describes current implementation, useful for future refactoring

#### 8. PHASE_5_INTEGRATION_PLAN.md
**Status**: Superseded by PHASE_5_COMPLETE.md  
**Content**: Planning document for Phase 5 work  
**Action**: Archive to `docs/archive/planning/`  
**Reason**: Planning phase complete, actual work documented in PHASE_5_COMPLETE.md

#### 9. FFF_GRAPHICS_MIGRATION_PLAN.md
**Status**: Partially implemented (offline complete, online pending)  
**Content**: Graphics implementation plan for FFF contestant display  
**Action**: Archive to `docs/archive/planning/` OR keep in root  
**Reason**: Offline graphics complete, online graphics still TODO  
**Alternative**: Keep until FFF Online graphics implemented

---

### Session Logs (Move to `docs/archive/sessions/`)

#### 10. SESSION_SUMMARY_PHASE_3.md
**Status**: Historical record  
**Content**: Development session notes for Phase 3  
**Action**: Archive to `docs/archive/sessions/`  
**Reason**: Session work complete, content in PHASE_3_COMPLETE.md and ARCHIVE.md

#### 11. SESSION_SUMMARY_PHASE_4.md
**Status**: Historical record  
**Content**: Development session notes for Phase 4  
**Action**: Archive to `docs/archive/sessions/`  
**Reason**: Session work complete, content in PHASE_4 docs

#### 12. FFF_OFFLINE_MODE_SESSION.md
**Status**: Implementation session notes  
**Content**: Detailed FFF Offline development session  
**Action**: Archive to `docs/archive/sessions/`  
**Reason**: Work complete, documented in multiple places

---

### Documents to KEEP in Root

#### CHANGELOG.md ✅
**Reason**: Active changelog for all versions  
**Usage**: Updated with every release  
**Action**: Keep in root directory

#### DEVELOPMENT_CHECKPOINT.md ✅
**Reason**: Current development status tracking  
**Usage**: Updated after each major phase  
**Action**: Keep in root directory  
**TODO**: Update to v0.7.0, add Phase 5.2 section

#### README.md ✅
**Reason**: Project overview and getting started guide  
**Usage**: First file users/developers read  
**Action**: Keep in root directory  
**TODO**: Update version to 0.7.0, reflect WAPS completion

#### ARCHIVE.md ✅
**Reason**: Historical session logs for v0.2-v0.3  
**Usage**: Reference for early development decisions  
**Action**: Keep in root directory

---

### Documents to KEEP in `docs/active/`

#### PROJECT_AUDIT_2025.md ✅ (NEW)
**Reason**: Comprehensive audit of current state  
**Usage**: Reference for completion status and remaining work  
**Action**: Move to `docs/active/` after user review

#### PRE_1.0_FINAL_CHECKLIST.md ✅ (NEW)
**Reason**: Active checklist for v1.0 completion  
**Usage**: Track progress toward release  
**Action**: Keep in `docs/active/`, archive after v1.0 release

#### FFF_ONLINE_FLOW_DOCUMENT.md (TO BE CREATED)
**Reason**: Critical planning document for FFF Online feature  
**Usage**: Guide for immediate development work  
**Action**: Create in `docs/active/`, archive after implementation

---

### Documents to KEEP in `docs/reference/`

#### WEB_SYSTEM_IMPLEMENTATION_PLAN.md ✅
**Reason**: Comprehensive WAPS architecture reference  
**Usage**: Reference for web system design and API patterns  
**Action**: Move to `docs/reference/`  
**TODO**: Mark Phases 1-5 as complete with ✅ checkmarks

---

## 🔄 Migration Steps

### Step 1: Create Directory Structure
```bash
cd src/
mkdir -p docs/archive/planning
mkdir -p docs/archive/sessions
mkdir -p docs/archive/phases
mkdir -p docs/active
mkdir -p docs/reference
```

### Step 2: Move Phase Documentation
```bash
# Move completed phase docs
git mv PHASE_3_COMPLETE.md docs/archive/phases/
git mv PHASE_4_PRIVACY_SESSION_MANAGEMENT.md docs/archive/phases/
git mv PHASE_5_COMPLETE.md docs/archive/phases/
git mv PHASE_5.0.1_COMPLETE.md docs/archive/phases/
git mv PHASE_5.1_COMPLETE.md docs/archive/phases/

# Decision: Keep PHASE_5.2_COMPLETE.md in root until after v1.0
# OR move now:
# git mv PHASE_5.2_COMPLETE.md docs/archive/phases/
```

### Step 3: Move Planning Documents
```bash
git mv PHASE_5_INTEGRATION_PLAN.md docs/archive/planning/

# Decision: Keep or archive LIFELINE_REFACTORING_PLAN.md?
# OPTION A: Archive (work complete)
git mv LIFELINE_REFACTORING_PLAN.md docs/archive/planning/

# OPTION B: Keep as reference
# git mv LIFELINE_REFACTORING_PLAN.md docs/reference/

# Decision: Keep or archive FFF_GRAPHICS_MIGRATION_PLAN.md?
# OPTION A: Archive (offline complete, online pending but defined)
git mv FFF_GRAPHICS_MIGRATION_PLAN.md docs/archive/planning/

# OPTION B: Keep until online graphics implemented
# (Leave in root for now)
```

### Step 4: Move Session Logs
```bash
git mv SESSION_SUMMARY_PHASE_3.md docs/archive/sessions/
git mv SESSION_SUMMARY_PHASE_4.md docs/archive/sessions/
git mv FFF_OFFLINE_MODE_SESSION.md docs/archive/sessions/
```

### Step 5: Move Active Documents
```bash
git mv PROJECT_AUDIT_2025.md docs/active/
git mv PRE_1.0_FINAL_CHECKLIST.md docs/active/
```

### Step 6: Move Reference Documents
```bash
git mv WEB_SYSTEM_IMPLEMENTATION_PLAN.md docs/reference/
```

### Step 7: Update Cross-References
After moving files, search for broken internal links:
```bash
# Search for references to moved files
grep -r "PHASE_3_COMPLETE" src/*.md
grep -r "SESSION_SUMMARY" src/*.md
grep -r "WEB_SYSTEM_IMPLEMENTATION" src/*.md
```

Update relative paths in:
- DEVELOPMENT_CHECKPOINT.md
- ARCHIVE.md  
- CHANGELOG.md
- Any other docs referencing moved files

### Step 8: Commit Changes
```bash
git add .
git commit -m "docs: Archive completed phase and planning documents

Organized documentation into structured folders:
- docs/archive/phases/ - Completed phase documentation
- docs/archive/sessions/ - Historical session logs
- docs/archive/planning/ - Completed planning documents
- docs/active/ - Current working documents
- docs/reference/ - Architecture and design reference

Moved 12 documents to appropriate locations.
Updated cross-references in root-level documents."
```

---

## 📊 Impact Analysis

### Files Affected
- **To Archive**: 12 files
- **To Keep in Root**: 4 files (CHANGELOG, DEVELOPMENT_CHECKPOINT, README, ARCHIVE)
- **To Create**: 2-3 files (FFF_ONLINE_FLOW_DOCUMENT.md, etc.)
- **To Update**: 3 files (DEVELOPMENT_CHECKPOINT, README, WEB_SYSTEM_IMPLEMENTATION_PLAN)

### Benefits
1. **Cleaner root directory** - Only active/essential docs visible
2. **Clear structure** - Easy to find historical vs. active docs
3. **Better navigation** - Organized by document type and status
4. **Preserved history** - Nothing deleted, just organized
5. **Future-proof** - Clear pattern for archiving future phases

### Risks
- **Broken links** - Internal document cross-references need updating
- **Workflow disruption** - Team members accustomed to root-level docs
- **Git history** - File moves create renames in git log (minor issue)

### Mitigation
- Update all cross-references before committing
- Document new structure in README.md
- Use `git mv` to preserve file history
- Keep commit message clear about reorganization

---

## 🎯 Alternative Approach

If concerned about disruption, consider **gradual migration**:

### Phase A: Archive Only Session Logs (Low Risk)
```bash
# Step 1: Archive session logs first (least referenced)
git mv SESSION_SUMMARY_PHASE_3.md docs/archive/sessions/
git mv SESSION_SUMMARY_PHASE_4.md docs/archive/sessions/
git mv FFF_OFFLINE_MODE_SESSION.md docs/archive/sessions/
```

### Phase B: Archive Completed Planning Docs (Medium Risk)
```bash
# Step 2: Archive planning docs after verifying no active references
git mv PHASE_5_INTEGRATION_PLAN.md docs/archive/planning/
git mv LIFELINE_REFACTORING_PLAN.md docs/archive/planning/
```

### Phase C: Archive Phase Documentation (Higher Risk)
```bash
# Step 3: Archive phase docs after updating DEVELOPMENT_CHECKPOINT links
git mv PHASE_3_COMPLETE.md docs/archive/phases/
# ... (rest of phase docs)
```

### Phase D: Organize Active/Reference (Post v1.0)
```bash
# Step 4: Create active/reference structure after v1.0 release
# (Less disruptive to do this after major milestone)
```

---

## 📝 Recommendations

### Immediate Actions (Before Next Development Session)
1. **Create `docs/` folder structure** - Set up organization
2. **Archive session logs** - Low-hanging fruit, least disruptive
3. **Move planning docs** - Clear out completed plans
4. **Update DEVELOPMENT_CHECKPOINT.md** - Reflect v0.7.0 state

### Before v1.0 Release
1. **Archive all completed phase docs** - Clean up for release
2. **Move WEB_SYSTEM_IMPLEMENTATION_PLAN to reference/** - Clear architecture doc
3. **Update README.md** - Document new folder structure

### After v1.0 Release
1. **Archive PRE_1.0_FINAL_CHECKLIST.md** - No longer active
2. **Archive PROJECT_AUDIT_2025.md** - Historical snapshot
3. **Create v1.0 Post-Mortem document** - Capture lessons learned

---

## ✅ Decision Points for User

**Please decide**:

1. **Archive PHASE_5.2_COMPLETE.md now or wait?**
   - Option A: Archive now (work complete)
   - Option B: Keep in root until v1.0 (recent work, good visibility)

2. **Archive LIFELINE_REFACTORING_PLAN.md or keep as reference?**
   - Option A: Archive (implementation complete)
   - Option B: Keep in reference (useful for architecture understanding)

3. **Archive FFF_GRAPHICS_MIGRATION_PLAN.md now or wait?**
   - Option A: Archive now (offline complete, plan served its purpose)
   - Option B: Keep until online graphics implemented

4. **Full migration now or gradual approach?**
   - Option A: Full migration in one commit
   - Option B: Gradual (sessions → planning → phases → active/reference)

5. **Create `docs/` structure immediately or wait until v1.0?**
   - Option A: Now (cleaner development environment)
   - Option B: Post-v1.0 (less disruption during final push)

---

*Document Created*: December 24, 2025  
*Status*: Proposal - Awaiting User Decision  
*Next Step*: User approval, then execute migration
