# Session: Publish Process Fixes and Release Updates
**Date:** January 10, 2026  
**Focus:** Critical publishing issues, installer rebuild, and release management

## 🎯 Session Objectives
1. Fix missing Watchdog component in published builds
2. Optimize publish output by removing redundant files
3. Rebuild and update v1.0.5 installer
4. Update v1.0.0 release with critical warnings

## 🔧 Issues Identified

### Critical: Missing Watchdog in Publish
- **Problem:** The `MillionaireGame.Watchdog.exe` was not being included when publishing
- **Impact:** Application wouldn't start without the crash monitoring component
- **Root Cause:** Solution-level publish with `-p:PublishSingleFile=true` doesn't work with mixed project types (exe + library)

### Redundant StreamDeck DLLs
- **Problem:** StreamDeck DLLs were being copied to both root and `lib\StreamDeck\`
- **Impact:** Unnecessary duplication in publish output
- **Root Cause:** Redundant copy operations in build targets

### Locale Folders Pollution
- **Problem:** Empty locale folders (de, es, fr, it, ja, ko, pt-BR, ru, zh-Hans, zh-Hant) created in publish output
- **Impact:** Cluttered output directory with unnecessary folders
- **Root Cause:** Satellite resource assemblies for localization (app is English-only)

## ✅ Solutions Implemented

### 1. Fixed Publish Process
**File:** `.github/copilot-instructions.md`
- Updated publishing instructions to require separate publishes for main app and watchdog
- Added requirement to clean publish folder first
- Clarified that solution-level publish with single-file packaging is not supported

**Commands:**
```powershell
Remove-Item -Path "../publish" -Recurse -Force -ErrorAction SilentlyContinue
cd src
dotnet publish MillionaireGame/MillionaireGame.csproj -c Release -r win-x64 --no-self-contained -p:PublishSingleFile=true -o ../publish
dotnet publish MillionaireGame.Watchdog/MillionaireGame.Watchdog.csproj -c Release -r win-x64 --no-self-contained -p:PublishSingleFile=true -o ../publish
```

### 2. Optimized StreamDeck DLL Handling
**File:** `src/MillionaireGame/MillionaireGame.csproj`

**Changes:**
- Removed `StreamDeckFiles` copying from both `CopyLibAssets` and `CopyLibAssetsToPublish` targets
- Kept only sounds and SQL files in lib folder
- StreamDeck DLLs remain in application root via `<Private>true</Private>` setting

**Before:**
```xml
<StreamDeckFiles Include="lib\StreamDeck\**\*.*" />
<Copy SourceFiles="@(StreamDeckFiles)" ... />
```

**After:**
```xml
<!-- StreamDeck DLLs are copied to output root automatically via Reference Private=true -->
```

### 3. Removed Locale Folders
**File:** `src/MillionaireGame/MillionaireGame.csproj`

**Added Property:**
```xml
<!-- Exclude satellite assemblies (localization) - app is English only -->
<SatelliteResourceLanguages>en</SatelliteResourceLanguages>
```

**Result:** Publish output now only contains `lib/sounds/` and `lib/sql/` - no more empty locale folders

### 4. Native Library Embedding
**File:** `src/MillionaireGame/MillionaireGame.csproj`

**Added Property:**
```xml
<!-- Include native libraries in single file bundle (extracted at runtime) -->
<IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
```

**Result:** Native DLLs like `Microsoft.Data.SqlClient.SNI.dll` are now embedded in the executable

## 📦 Installer Rebuild

### Actions Taken
1. Cleaned publish directory
2. Published both main app and watchdog with fixes
3. Built new installer with Inno Setup (198.9 MB)
4. Verified Watchdog presence in installer

### Installer Contents (Fixed)
- ✅ `MillionaireGame.exe` (47.43 MB) - Main application
- ✅ `MillionaireGame.Watchdog.exe` (0.18 MB) - **NOW INCLUDED**
- ✅ StreamDeck DLLs in root (HidSharp.dll, OpenMacroBoard.SDK.dll, StreamDeckSharp.dll)
- ✅ `lib/sounds/` - Audio files (~176 MB)
- ✅ `lib/sql/` - Database scripts
- ❌ No locale folders
- ❌ No `lib/StreamDeck/` directory

## 🚀 Release Management

### v1.0.5 Release Updated
- Deleted old installer (missing watchdog)
- Uploaded new installer with all fixes
- **Action:** `gh release upload v1.0.5 "installer\output\MillionaireGameSetup-v1.0.5.exe" --clobber`

### v1.0.0 Release Deprecated
- **Deleted installer** due to critical bug
- **Updated release notes** with prominent warning
- **Issue:** Multi-monitor support bug causes excessive WMI calls leading to system freezes and potential Windows corruption
- **Severity:** Can require system rebuild in worst cases
- **Recommendation:** Use v1.0.5 or later

## 📊 File Size Comparison

### Before Fixes
- Main exe: ~34 MB (estimated, without proper single-file packaging)
- Watchdog: **MISSING**
- Extra folders: 10+ empty locale directories
- Duplicate DLLs: lib/StreamDeck/*.dll (redundant)

### After Fixes
- Main exe: 47.43 MB (properly single-file packaged)
- Watchdog: 0.18 MB ✅
- Clean structure: Only lib/sounds/ and lib/sql/
- No duplicates: StreamDeck DLLs only in root

## 🎓 Lessons Learned

1. **Multi-Project Publishing:** .NET doesn't support solution-level publish with `-p:PublishSingleFile=true` when mixing library and executable projects
2. **Reference Assembly Copying:** `<Private>true</Private>` automatically copies DLLs to output - no need for manual copy tasks
3. **Satellite Assemblies:** Use `<SatelliteResourceLanguages>en</SatelliteResourceLanguages>` to prevent unnecessary locale folders
4. **Clean First:** Always clean publish directory before publishing to avoid stale files and conflicts
5. **Verify Critical Components:** The Watchdog is essential - application won't start without it

## 📝 Documentation Updates

### Updated Files
- `.github/copilot-instructions.md` - Publishing instructions corrected
- `src/MillionaireGame/MillionaireGame.csproj` - Build targets optimized

### GitHub Releases
- v1.0.5: Installer updated with fixed build
- v1.0.0: Marked as deprecated with critical warning

## ✅ Verification

### Publish Output Verified
```
publish/
├── MillionaireGame.exe (47.43 MB) ✅
├── MillionaireGame.Watchdog.exe (0.18 MB) ✅
├── HidSharp.dll
├── OpenMacroBoard.SDK.dll
├── StreamDeckSharp.dll
├── lib/
│   ├── sounds/ (176 MB) ✅
│   └── sql/ ✅
└── AppData Folder.lnk
```

### Build Success
- Both projects published successfully
- Single-file executables created
- No locale folders
- No redundant files
- Clean directory structure

## 🔄 Next Steps

1. Monitor v1.0.5 downloads for any issues
2. Consider adding automated publish script in CI/CD
3. Document multi-monitor WMI bug fix in changelog
4. Update wiki with corrected publish instructions

## 📌 Important Notes

- **ALWAYS** publish both MillionaireGame and MillionaireGame.Watchdog projects
- **ALWAYS** clean the publish folder first to prevent conflicts
- **NEVER** use solution-level publish with `-p:PublishSingleFile=true`
- The Watchdog is **MANDATORY** - application won't start without it

---
**Session completed successfully** - All issues resolved, installer rebuilt, releases updated.
