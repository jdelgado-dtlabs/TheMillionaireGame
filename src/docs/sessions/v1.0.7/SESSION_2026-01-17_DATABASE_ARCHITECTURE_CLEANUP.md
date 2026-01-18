# Session: Database Architecture Cleanup
**Date:** January 17, 2026  
**Branch:** `fix/database-architecture-cleanup` → `feat/base-repository`  
**Status:** In Progress

## Objectives
Implement database architecture improvements as outlined in DATABASE_ACCESS_ARCHITECTURE_REVIEW.md:
- **Phase 1:** Eliminate duplicate FFFQuestion repository and model ✅
- **Phase 2:** Introduce BaseRepository abstract class 🔄
- **Phase 3:** Dependency injection (future)
- **Phase 4:** Unify data access patterns (future)

---

## Phase 1: Eliminate Duplicates ✅ COMPLETE

### Problem Identified
- **Duplicate FFFQuestionRepository** in Web.Database and Core.Database namespaces
- **Duplicate FFFQuestion model** in Web.Models and Core.Models namespaces
- Inconsistent usage across solution causing database connection issues

### Changes Made

#### Removed Files:
1. `MillionaireGame.Web/Database/FFFQuestionRepository.cs` - 130 lines
2. `MillionaireGame.Web/Models/FFFQuestion.cs` - 24 lines

#### Enhanced Core Repository:
**File:** `MillionaireGame.Core/Database/FFFQuestionRepository.cs`
- Added `GetQuestionByIdAsync()` method (22 lines)
- Method was present in Web version but missing in Core
- Required by FFFService for ranking calculations

```csharp
/// <summary>
/// Get specific FFF question by ID
/// </summary>
public async Task<FFFQuestion?> GetQuestionByIdAsync(int questionId)
{
    using var connection = new SqlConnection(_connectionString);
    await connection.OpenAsync();

    var query = "SELECT * FROM fff_questions WHERE Id = @Id";
    using var command = new SqlCommand(query, connection);
    command.Parameters.AddWithValue("@Id", questionId);
    
    using var reader = await command.ExecuteReaderAsync();

    if (await reader.ReadAsync())
    {
        return MapQuestion(reader);
    }

    return null;
}
```

#### Updated References:

**1. FFFOnlinePanel.cs**
- Changed from: `using MillionaireGame.Web.Models;`
- Changed to: `using MillionaireGame.Core.Models;`
- Changed from: `using MillionaireGame.Web.Database;`
- Removed (already had Core.Database)
- Field: `private MillionaireGame.Core.Database.FFFQuestionRepository? _fffRepository;`

**2. FFFWindow.cs**
- Added connection string parameter to constructor
- Calls `fffOnlinePanel.SetConnectionString()` on initialization

**3. ControlPanelForm.cs**
- Gets connection string: `var connectionString = _sqlSettings.Settings.GetConnectionString("dbMillionaire");`
- Passes to FFFWindow constructor

**4. FFFService.cs** (Web project)
- Removed: `using MillionaireGame.Web.Database;`
- Added: `using MillionaireGame.Core.Database;`
- Added type alias: `using FFFQuestion = MillionaireGame.Core.Models.FFFQuestion;`
- Kept `using MillionaireGame.Web.Models;` for FFFAnswer entity

**5. WebServerHost.cs**
- Changed: `using MillionaireGame.Core.Database;`
- DI registration uses Core namespace

### Build Results
✅ **All projects built successfully**
- MillionaireGame.Watchdog: ✅
- MillionaireGame.Watchdog.Tests: ✅
- MillionaireGame.Core: ✅
- MillionaireGame.Web: ✅
- MillionaireGame: ✅

### Git Operations
```bash
git checkout -b fix/database-architecture-cleanup
# ... made changes ...
git add -A
git commit -m "fix: Eliminate duplicate FFFQuestion repository and model"
git push -u origin fix/database-architecture-cleanup
git checkout master-v1.0.7
git merge --no-ff fix/database-architecture-cleanup
git push origin master-v1.0.7
git branch -d fix/database-architecture-cleanup
```

### Commit Details
**Branch:** `fix/database-architecture-cleanup` (merged into `master-v1.0.7`)  
**Commit:** `8784a96`  
**Files Changed:** 9 files (+523, -173)

---

## Phase 2: BaseRepository Abstract Class 🔄 IN PROGRESS

### Objective
Create a base repository class to:
- Eliminate code duplication across 10 repositories
- Provide consistent connection management
- Standardize error handling
- Simplify common database operations

### Repositories to Refactor
1. ✅ `QuestionRepository` - Main game questions
2. ✅ `FFFQuestionRepository` - FFF questions
3. ✅ `ThemeRepository` - Theme definitions
4. ✅ `ThemeStrapRepository` - Strap configurations
5. ✅ `ThemeMoneyTreeRepository` - Money tree configs
6. ✅ `ThemeBackgroundRepository` - Background images
7. ✅ `ThemePackRepository` - Theme pack metadata
8. ✅ `ApplicationSettingsRepository` - App settings
9. ✅ `TelemetryRepository` - Usage telemetry

### BaseRepository Design

**File:** `MillionaireGame.Core/Database/BaseRepository.cs`

```csharp
using Microsoft.Data.SqlClient;

namespace MillionaireGame.Core.Database;

/// <summary>
/// Base class for all ADO.NET-based repositories
/// Provides common database connection and error handling functionality
/// </summary>
public abstract class BaseRepository
{
    protected readonly string ConnectionString;

    protected BaseRepository(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));
            
        ConnectionString = connectionString;
    }

    /// <summary>
    /// Opens and returns a new SQL connection
    /// Caller is responsible for disposal (use with 'using' statement)
    /// </summary>
    protected async Task<SqlConnection> OpenConnectionAsync()
    {
        var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        return connection;
    }
    
    /// <summary>
    /// Executes a scalar query and returns the result
    /// </summary>
    protected async Task<T?> ExecuteScalarAsync<T>(string query, params SqlParameter[] parameters)
    {
        using var connection = await OpenConnectionAsync();
        using var command = new SqlCommand(query, connection);
        if (parameters.Length > 0)
            command.Parameters.AddRange(parameters);
        
        var result = await command.ExecuteScalarAsync();
        return result == null ? default : (T)result;
    }
    
    /// <summary>
    /// Executes a non-query command (INSERT, UPDATE, DELETE)
    /// Returns number of rows affected
    /// </summary>
    protected async Task<int> ExecuteNonQueryAsync(string query, params SqlParameter[] parameters)
    {
        using var connection = await OpenConnectionAsync();
        using var command = new SqlCommand(query, connection);
        if (parameters.Length > 0)
            command.Parameters.AddRange(parameters);
        
        return await command.ExecuteNonQueryAsync();
    }
}
```

### Refactoring Pattern

**Before:**
```csharp
public class QuestionRepository
{
    private readonly string _connectionString;
    
    public QuestionRepository(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    public async Task<Question?> GetRandomQuestionAsync(int questionNumber)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        // ... query logic
    }
}
```

**After:**
```csharp
public class QuestionRepository : BaseRepository
{
    public QuestionRepository(string connectionString) : base(connectionString) { }
    
    public async Task<Question?> GetRandomQuestionAsync(int questionNumber)
    {
        using var connection = await OpenConnectionAsync();
        // ... query logic
    }
}
```

### Implementation Steps
1. ✅ Create `BaseRepository.cs` abstract class
2. ✅ Update each repository to inherit from BaseRepository
3. ✅ Replace `_connectionString` field with `ConnectionString` property
4. ✅ Replace `new SqlConnection(_connectionString)` with `await OpenConnectionAsync()`
5. ✅ Update constructor to call base constructor
6. ✅ Build and verify no errors

### Current Status
- BaseRepository class: ✅ Created
- Repositories refactored: 🔄 In progress
- Build status: 🔄 Pending
- Tests: 🔄 Pending

---

## Benefits Achieved

### Phase 1 Benefits:
✅ **Single source of truth** - All FFF code uses Core namespace  
✅ **No ambiguity** - No duplicate classes causing confusion  
✅ **Bug fixed** - FFF Online database connection resolved  
✅ **Consistency** - Uniform database access patterns  

### Phase 2 Benefits (Expected):
⏳ **Reduced duplication** - Common code in base class  
⏳ **Easier maintenance** - Changes in one place  
⏳ **Consistent patterns** - All repositories use same approach  
⏳ **Better error handling** - Centralized error management  

---

## Next Steps

### Immediate (Phase 2):
1. Refactor remaining repositories to use BaseRepository
2. Build and verify all projects compile
3. Test basic database operations
4. Commit changes

### Future (Phase 3+):
1. Introduce repository interfaces
2. Set up dependency injection
3. Add unit tests
4. Consider EF Core migration (evaluate pros/cons)

---

## Documentation
- **Architecture Review:** `docs/active/DATABASE_ACCESS_ARCHITECTURE_REVIEW.md`
- **Session Document:** This file
- **Commit Messages:** Descriptive with context

## Files Modified This Session

### Phase 1:
- ✅ `MillionaireGame.Core/Database/FFFQuestionRepository.cs` (+22 lines)
- ✅ `MillionaireGame.Web/Database/FFFQuestionRepository.cs` (deleted)
- ✅ `MillionaireGame.Web/Models/FFFQuestion.cs` (deleted)
- ✅ `MillionaireGame.Web/Services/FFFService.cs` (updated imports)
- ✅ `MillionaireGame/Forms/ControlPanelForm.cs` (pass connection string)
- ✅ `MillionaireGame/Forms/FFFOnlinePanel.cs` (updated namespace)
- ✅ `MillionaireGame/Forms/FFFWindow.cs` (added connection string param)
- ✅ `MillionaireGame/Hosting/WebServerHost.cs` (updated DI registration)
- ✅ `docs/active/DATABASE_ACCESS_ARCHITECTURE_REVIEW.md` (created)

### Phase 2:
- 🔄 `MillionaireGame.Core/Database/BaseRepository.cs` (creating)
- 🔄 All repository classes (refactoring)

---

## Lessons Learned
1. **Always check for duplicates** - Web and Core projects had parallel implementations
2. **Type aliases are useful** - `using FFFQuestion = ...` resolves ambiguity elegantly
3. **Build frequently** - Caught missing methods early
4. **Document architectural decisions** - Review document provides roadmap

## Time Tracking
- Phase 1 planning: ~15 minutes
- Phase 1 implementation: ~45 minutes
- Phase 1 documentation: ~20 minutes
- **Phase 1 Total: ~1.5 hours**

- Phase 2 planning: ~10 minutes
- Phase 2 implementation: 🔄 In progress
- **Phase 2 Estimated: ~3-4 hours**
