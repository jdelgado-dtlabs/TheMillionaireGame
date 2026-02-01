# Session: Stack Overflow Crash Fixes

**Date**: February 1, 2026  
**Branch**: `master-v1.0.7`  
**Commit**: `eec74f0`

## 🎯 Objective

Fix critical stack overflow crashes (exit code 0xC00000FD) reported in production that caused the application to terminate unexpectedly.

---

## 🐛 Problem Analysis

### Crash Report Details

**Exit Code**: `-1073741571 (0xC00000FD)` - Stack Overflow  
**Symptom**: Application crashed after ~5 seconds during initialization  
**Last Log Entries**: Repeated StreamDeck image loading errors

```
[14:07:28] [ERROR] [StreamDeck] ❌ IMAGE NOT FOUND: C:\Program Files\The Millionaire Game\lib\image\streamdeck\blank.png
```

### Root Causes Identified

Two separate infinite recursion issues were discovered:

1. **AudioCueQueue.cs** - Audio processing recursion
2. **StreamDeckService.cs** - Image loading recursion

---

## 🔧 Fixes Implemented

### Fix #1: AudioCueQueue.cs - Audio Processing

**File**: `src/MillionaireGame/Services/AudioCueQueue.cs`  
**Lines**: 645-710

**Problem**:
When audio cues returned 0 samples (empty/corrupted files), the `Read()` method recursively called itself:

```csharp
// OLD CODE - DANGEROUS RECURSION
if (read == 0) {
    if (_nextCue != null) {
        _currentCue = _nextCue;
        return Read(buffer, offset, count); // ← RECURSIVE
    }
    else if (_normalQueue.Count > 0) {
        _currentCue = _normalQueue.Dequeue();
        return Read(buffer, offset, count); // ← RECURSIVE
    }
}
```

**Solution**:
Replaced recursion with iterative loop:

```csharp
// NEW CODE - SAFE ITERATION
int retryCount = 0;
const int maxRetries = 100; // Safety limit

while (retryCount < maxRetries) {
    // Try to get next cue
    if (_nextCue != null) {
        _currentCue = _nextCue;
        _nextCue = null;
    }
    else if (_normalQueue.Count > 0) {
        _currentCue = _normalQueue.Dequeue();
    }
    else {
        // Queue empty - return silence
        Array.Clear(buffer, offset, count);
        return count;
    }
    
    // Try reading from new cue
    read = _currentCue.Source.Read(buffer, offset, count);
    
    if (read > 0) {
        break; // Success - exit loop
    }
    
    // Cue was empty - log warning and try next
    GameConsole.Warn($"[AudioCueQueue] Cue returned 0 samples: {Path.GetFileName(_currentCue.FilePath)}");
    _currentCue.Dispose();
    _currentCue = null;
    retryCount++;
}
```

**Benefits**:
- ✅ Prevents stack overflow from consecutive empty audio files
- ✅ Logs warnings for each problematic file
- ✅ Safety limit prevents infinite loops
- ✅ Gracefully degrades to silence if all cues fail

---

### Fix #2: StreamDeckService.cs - Image Loading

**File**: `src/MillionaireGame/Services/StreamDeckService.cs`  
**Lines**: 192-260

**Problem**:
When `blank.png` was not found on disk, infinite recursion occurred:

1. `SetButtonImage(row, col, "blank.png")` - file not found
2. Calls `SetButtonBlank(row, col)` as fallback
3. `SetButtonBlank()` calls `SetButtonImage(row, col, "blank.png")`
4. Back to step 1 → **INFINITE LOOP** → **STACK OVERFLOW**

**Solution**:
Two-part fix:

**Part A: Load from Embedded Resources**

Images are embedded at build time via `.csproj`:
```xml
<EmbeddedResource Include="lib\image\streamdeck\*.png" />
```

Updated code to load from embedded resources first:

```csharp
SixLabors.ImageSharp.Image<Rgb24>? image = null;

// Try file system first (for development)
string imagePath = Path.Combine(_imageBasePath, filename);
if (File.Exists(imagePath)) {
    GameConsole.Debug($"[StreamDeck] Loading from file: {imagePath}");
    image = SixLabors.ImageSharp.Image.Load<Rgb24>(imagePath);
}
else {
    // Load from embedded resources (production)
    var assembly = System.Reflection.Assembly.GetExecutingAssembly();
    var resourceName = $"MillionaireGame.lib.image.streamdeck.{filename}";
    
    using var stream = assembly.GetManifestResourceStream(resourceName);
    if (stream != null) {
        image = SixLabors.ImageSharp.Image.Load<Rgb24>(stream);
        GameConsole.Debug($"[StreamDeck] ✓ Loaded from embedded resource");
    }
}
```

**Part B: Prevent Recursion**

Added guard to prevent recursive calls:

```csharp
if (stream == null) {
    // Resource not found either
    if (filename != "blank.png") {
        // For other images, try blank.png as fallback
        SetButtonBlank(row, col);
        return;
    }
    else {
        // blank.png itself is missing - generate programmatically
        image = new SixLabors.ImageSharp.Image<Rgb24>(80, 80, new Rgb24(0, 0, 0));
        GameConsole.Warn($"[StreamDeck] Using programmatic black image");
    }
}
```

**Benefits**:
- ✅ Works in production where files aren't copied to Program Files
- ✅ No recursion - checks if missing file IS blank.png
- ✅ Programmatic fallback (black square) if all else fails
- ✅ Maintains development workflow (loads from files if available)

---

## 🛠️ Additional Changes

### global.json

**File**: `src/global.json` (new file)

Added SDK version pinning to prevent .NET 10 SDK interference:

```json
{
  "sdk": {
    "version": "8.0.417",
    "rollForward": "latestPatch"
  }
}
```

**Purpose**: Ensures builds use .NET 8 SDK even when .NET 10 SDK is installed

---

## ✅ Testing & Validation

### Build Verification
- ✅ Clean build with .NET 8 SDK
- ✅ Zero errors, only expected async warnings
- ✅ Published successfully (45.62 MB)

### Runtime Testing
- ✅ Application starts without crashes
- ✅ No stack overflow errors (0xC00000FD)
- ✅ StreamDeck images load from embedded resources
- ✅ Audio system handles missing files gracefully

---

## 📊 Impact Assessment

### Before Fix
- ❌ Application crashed within 5 seconds
- ❌ Exit code: 0xC00000FD (Stack Overflow)
- ❌ Unusable in production

### After Fix
- ✅ Application runs normally
- ✅ Handles missing resources gracefully
- ✅ Comprehensive error logging
- ✅ Production-ready

---

## 🎓 Lessons Learned

1. **Recursion Risks**: Always prefer iteration over recursion for operations that could repeat many times
2. **Resource Loading**: Embedded resources are essential for deployed applications - don't rely on file system paths
3. **Defensive Coding**: Always add safeguards (max retries, recursion guards) when dealing with external resources
4. **Logging**: Diagnostic logging was critical for identifying the root cause from crash reports

---

## 📝 Notes for Future Development

- Monitor `GameConsole.Warn` logs for repeated empty audio file warnings
- Consider pre-validating audio files during theme pack import
- StreamDeck images are now 100% embedded - no need to copy to Program Files
- The `maxRetries = 100` limit in AudioCueQueue may need adjustment based on real-world usage

---

## 📦 Deliverables

- ✅ Fixed AudioCueQueue.cs (iterative audio processing)
- ✅ Fixed StreamDeckService.cs (embedded resource loading)
- ✅ Added global.json (SDK version pinning)
- ✅ Published build (45.62 MB exe + 0.29 MB watchdog)
- ✅ This session document

---

**Status**: ✅ **COMPLETE**  
**Ready for Production**: Yes  
**Breaking Changes**: None
