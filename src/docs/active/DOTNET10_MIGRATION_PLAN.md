# .NET 10 Migration Plan - v2.0

**Status:** Planning  
**Target Release:** v2.0 (Major Version)  
**Created:** January 12, 2026  
**Owner:** Development Team

## Overview

Migrate TheMillionaireGame solution from .NET 8 to .NET 10 (LTS) to leverage the latest performance improvements, new language features, and ensure long-term support through November 2028.

**Note:** This is targeted for v2.0 rather than v1.x due to library ecosystem maturity concerns. Several critical dependencies barely achieved .NET 8.0 support recently and need time to stabilize .NET 10 compatibility before we can safely migrate.

## Current State

- **Framework:** .NET 8.0 (LTS until November 2026)
- **Projects:**
  - `MillionaireGame` - .NET 8.0 Windows
  - `MillionaireGame.Core` - .NET 8.0
  - `MillionaireGame.Web` - .NET 8.0
  - `MillionaireGame.Watchdog` - .NET 8.0
  - `MillionaireGame.Watchdog.Tests` - .NET 8.0
- **Language Version:** C# 12.0 (implicit with .NET 8)

## Goals

1. **Primary:** Update all projects to target .NET 10.0
2. **Secondary:** Adopt new .NET 10 features where beneficial
3. **Tertiary:** Remove deprecated APIs and migrate to modern alternatives
4. **Quality:** Maintain zero regression in functionality

## Phase 1: Compatibility Assessment

### 1.1 Dependency Audit
- [ ] Review all NuGet packages for .NET 10 compatibility
- [ ] Identify packages requiring updates
- [ ] Identify packages with breaking changes
- [ ] Document alternative packages for incompatible dependencies
- [ ] **CRITICAL:** Monitor library ecosystem for stable .NET 10 support
- [ ] Wait for dependencies to mature beyond initial .NET 10 support

**Key Packages to Verify:**
- Microsoft.Data.SqlClient
- Microsoft.AspNetCore.SignalR
- System.Drawing.Common
- StreamDeckSharp (if used)
- Any lighting/sound plugin dependencies

**Library Maturity Assessment:**
- [ ] Track GitHub issues/releases for key dependencies
- [ ] Verify community adoption of .NET 10 versions
- [ ] Test beta/RC versions of critical libraries
- [ ] Document minimum library versions required for stability

### 1.2 Code Compatibility Scan
- [ ] Use .NET Upgrade Assistant to analyze solution
- [ ] Run Roslyn analyzers with .NET 10 target
- [ ] Identify deprecated API usage
- [ ] Document breaking changes impact

**Known Areas to Review:**
- File I/O operations (new simplified APIs in .NET 10)
- LINQ operations (performance improvements)
- Async patterns (enhanced ValueTask support)
- Serialization (System.Text.Json improvements)
- HTTP client usage (improved DefaultHttpClient)

### 1.3 Third-Party Library Compatibility
- [ ] Plugin architecture compatibility
  - [ ] Lighting plugins (ETC Ion, custom implementations)
  - [ ] Sound plugins (Yamaha TF)
  - [ ] Stream Deck integration
- [ ] Database layer (Entity Framework or ADO.NET)
- [ ] Web server components (SignalR, ASP.NET Core)

## Phase 2: Migration Strategy

### 2.1 Project File Updates

**Update all `.csproj` files:**
```xml
<!-- FROM -->
<TargetFramework>net8.0</TargetFramework>
<TargetFramework>net8.0-windows</TargetFramework>

<!-- TO -->
<TargetFramework>net10.0</TargetFramework>
<TargetFramework>net10.0-windows</TargetFramework>
```

**Update language version (if needed):**
```xml
<LangVersion>latest</LangVersion> <!-- C# 13.0 with .NET 10 -->
```

### 2.2 NuGet Package Updates

**Priority Order:**
1. Microsoft.* packages (framework-provided)
2. ASP.NET Core packages
3. Database packages
4. Third-party libraries
5. Plugin dependencies

**Process:**
```powershell
# List outdated packages
dotnet list package --outdated

# Update packages per project
dotnet add package [PackageName] --version [NewVersion]
```

### 2.3 Code Modernization

#### Deprecated APIs to Replace

**Windows Forms (if applicable):**
- Review any Windows-specific API changes
- Update control initialization patterns

**File I/O:**
```csharp
// MIGRATE FROM:
using (var stream = File.OpenRead(path))
{
    // ...
}

// TO (simplified in .NET 10):
await using var stream = File.OpenRead(path);
// Or use new File.ReadAllTextAsync improvements
```

**LINQ Performance:**
- Replace `ToList()` with `ToArray()` where immutable results are expected
- Leverage new `Order()` and `OrderDescending()` methods
- Use `TryGetNonEnumeratedCount()` for optimization

**Async Patterns:**
```csharp
// MIGRATE FROM:
Task<Result> SomeMethodAsync()

// TO (where appropriate):
ValueTask<Result> SomeMethodAsync() // Better performance for hot paths
```

#### New Features to Adopt

**1. Primary Constructors (C# 12/13):**
```csharp
// Consider for simple classes
public class GameSettings(string name, int value)
{
    public string Name => name;
    public int Value => value;
}
```

**2. Collection Expressions:**
```csharp
// MIGRATE FROM:
var questions = new List<Question> { q1, q2, q3 };

// TO:
List<Question> questions = [q1, q2, q3];
```

**3. Improved Pattern Matching:**
- Leverage enhanced switch expressions
- Use list patterns for collection matching

**4. Time Abstraction (`TimeProvider`):**
```csharp
// MIGRATE FROM:
DateTime.UtcNow

// TO (for testability):
_timeProvider.GetUtcNow()
```

**5. Source Generators:**
- Consider for settings serialization
- Use for logging improvements
- Evaluate for plugin metadata generation

## Phase 3: Testing Strategy

### 3.1 Automated Testing
- [ ] Run full unit test suite against .NET 10
- [ ] Run integration tests
- [ ] Validate watchdog functionality
- [ ] Test web server endpoints
- [ ] Verify SignalR real-time communication

### 3.2 Manual Testing Checklist
- [ ] **Game Flow:** Full game playthrough
- [ ] **Control Panel:** All buttons, settings, lifelines
- [ ] **Audience Participation:** Web interface, voting
- [ ] **Lighting Integration:** Test all lighting plugins
- [ ] **Sound System:** Test all sound plugins
- [ ] **Database Operations:** Settings persistence, game state
- [ ] **Crash Recovery:** Watchdog monitoring and recovery
- [ ] **Stream Deck:** External control integration

### 3.3 Performance Validation
- [ ] Benchmark startup time (should improve in .NET 10)
- [ ] Measure memory usage under load
- [ ] Profile async operations
- [ ] Validate UI responsiveness
- [ ] Test with audience participation (100+ connections)

### 3.4 Compatibility Testing
- [ ] Windows 10 (21H2 or later)
- [ ] Windows 11
- [ ] Test on systems with/without .NET 10 runtime installed
- [ ] Verify installer bundles correct runtime version

## Phase 4: Deployment Updates

### 4.1 Runtime Requirements
- **NEW:** .NET 10 Desktop Runtime required
- **OLD:** .NET 8 Desktop Runtime (remove from installer)

### 4.2 Installer Updates

**Update `MillionaireGameSetup.iss`:**
```iss
; Update prerequisite check
Source: "dotnet-desktop-runtime-10.x.x-win-x64.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall
```

**Update documentation:**
- [ ] System Requirements (minimum .NET 10)
- [ ] Installation Guide
- [ ] Building from Source

### 4.3 Publishing Configuration

**Update publish commands:**
```powershell
# Main Application
dotnet publish MillionaireGame/MillionaireGame.csproj `
  -c Release -r win-x64 --no-self-contained `
  -p:PublishSingleFile=true -o ../publish

# Watchdog
dotnet publish MillionaireGame.Watchdog/MillionaireGame.Watchdog.csproj `
  -c Release -r win-x64 --no-self-contained `
  -p:PublishSingleFile=true -o ../publish
```

### 4.4 CI/CD Updates (if applicable)
- [ ] Update build pipelines to use .NET 10 SDK
- [ ] Update Docker images (if used)
- [ ] Update GitHub Actions workflows

## Phase 5: Documentation

### 5.1 Code Documentation
- [ ] Update XML comments with new API patterns
- [ ] Document breaking changes in CHANGELOG.md
- [ ] Add migration notes for plugin developers

### 5.2 User Documentation
- [ ] Update System Requirements
- [ ] Update Installation Guide
- [ ] Create "Upgrading from v1.0.x" guide
- [ ] Update Quick Start Guide

### 5.3 Developer Documentation
- [ ] Update Building from Source
- [ ] Update Contributing guidelines
- [ ] Document new .NET 10 features used
- [ ] Update debugging/troubleshooting guides

## Phase 6: Rollout Plan

### 6.1 Pre-Release
1. **Alpha Release:** Internal testing with .NET 10
2. **Beta Release:** Limited user testing
3. **Release Candidate:** Full feature testing

### 6.2 Release
1. **Version:** v2.0 (major version - breaking change)
2. **Release Notes:** Comprehensive changelog with migration guide
3. **Migration Guide:** Step-by-step upgrade instructions from v1.x
4. **Support:** Active monitoring for issues; v1.x maintained for critical fixes

### 6.3 Backward Compatibility
- **Decision:** Drop .NET 8 support in v2.0 (major version bump)
- **Rationale:** Major version allows breaking changes; both are LTS versions
- **Support Window:** Maintain v1.x branch with .NET 8 for critical fixes through 2026
- **User Impact:** Clearly communicate breaking change in major version

## Known Risks & Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| **Library ecosystem immaturity** | **High** | **Wait for stable releases; monitor GitHub issues; test early versions** |
| Plugin incompatibility | High | Test all plugins; provide updated plugin SDK |
| Third-party library breaking changes | Medium | Identify early; implement adapters if needed |
| Performance regression | Low | Comprehensive benchmarking; .NET 10 typically faster |
| User adoption issues | Medium | Clear upgrade path; bundle runtime in installer |
| Database schema changes | Low | No schema changes planned; use migration scripts if needed |

## Timeline Estimate

| Phase | Duration | Dependencies |
|-------|----------|--------------|
| Phase 0: Library Monitoring | 3-6 months | Wait for ecosystem maturity |
| Phase 1: Assessment | 1 week | Phase 0 complete |
| Phase 2: Migration | 2 weeks | Phase 1 complete |
| Phase 3: Testing | 2 weeks | Phase 2 complete |
| Phase 4: Deployment | 1 week | Phase 3 complete |
| Phase 5: Documentation | 1 week | Parallel with Phases 2-4 |
| Phase 6: Rollout | 2 weeks | All phases complete |
| **TOTAL** | **4-7 months** | Library ecosystem dependent |

**Note:** Phase 0 is flexible based on dependency maturity. Begin assessment once critical libraries have stable .NET 10 releases with community validation.

## Success Criteria

- [ ] All projects build successfully with .NET 10
- [ ] Zero regressions in functionality
- [ ] All automated tests pass
- [ ] Manual testing checklist 100% complete
- [ ] Performance equal to or better than .NET 8 baseline
- [ ] All plugins compatible or updated
- [ ] Documentation updated and accurate
- [ ] Installer successfully deploys on clean Windows 10/11 systems

## References

- [.NET 10 Release Notes](https://docs.microsoft.com/dotnet/core/whats-new/dotnet-10)
- [C# 13.0 What's New](https://docs.microsoft.com/dotnet/csharp/whats-new/csharp-13)
- [.NET Upgrade Assistant](https://docs.microsoft.com/dotnet/core/porting/upgrade-assistant-overview)
- [Breaking Changes in .NET 10](https://docs.microsoft.com/dotnet/core/compatibility/10.0)

## Notes

- This migration is targeted for v2.0 to properly signal the breaking change
- Library ecosystem maturity is the primary gating factor for timeline
- Monitor key dependencies (especially those that recently gained .NET 8 support) for stable .NET 10 releases
- v1.x will remain on .NET 8 with maintenance support through 2026
- Consider v1.1-1.9 releases for feature additions while ecosystem matures
- Plugin developers should be notified 2 weeks before v2.0 release
- Create a rollback plan if critical issues emerge post-release
- Coordinate with First Run Wizard feature (current branch) for v1.x releases
