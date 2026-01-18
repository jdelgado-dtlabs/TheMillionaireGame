# Database Access Architecture Review
**Date:** January 17, 2026  
**Status:** Analysis Complete - Refactoring Recommended

## Executive Summary

The codebase **DOES have a repository pattern** for database access, but it's **not fully centralized** and has several architectural inconsistencies that need addressing:

### ✅ **Good Practices Currently in Place:**
1. **Repository Pattern** - All database tables have dedicated repository classes
2. **Connection String Management** - Centralized via `SqlSettingsManager`
3. **No Direct SQL in Forms** - UI layer doesn't directly execute SQL
4. **Service Layer** - Services act as intermediaries between UI and repositories

### ⚠️ **Issues Identified:**
1. **Duplicate FFFQuestionRepository** - Two identical implementations (Web and Core namespaces)
2. **No Dependency Injection** - Repositories manually instantiated with `new`
3. **Connection String Passed Everywhere** - Not using a shared database context
4. **Inconsistent Patterns** - Web project uses Entity Framework, Core uses ADO.NET
5. **Manual Repository Creation** - Services create their own repository instances

---

## Current Architecture

### 1. Repository Classes (ADO.NET - Core Project)
All in `MillionaireGame.Core.Database/`:

| Repository | Purpose | Tables |
|------------|---------|--------|
| `QuestionRepository` | Main game questions | `questions` |
| `FFFQuestionRepository` | Fastest Finger First questions | `fff_questions` |
| `ThemeRepository` | Theme definitions | `Themes` |
| `ThemeStrapRepository` | Strap configurations | `ThemeStraps` |
| `ThemeMoneyTreeRepository` | Money tree configurations | `ThemeMoneyTree` |
| `ThemeBackgroundRepository` | Background images | `ThemeBackgrounds` |
| `ThemePackRepository` | Theme pack metadata | `ThemePacks` |
| `ApplicationSettingsRepository` | App settings | `ApplicationSettings` |
| `TelemetryRepository` | Usage telemetry | `Telemetry` |

**Pattern Used:**
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
        // ... execute query
    }
}
```

### 2. Entity Framework DbContext (Web Project)
**`WAPSDbContext`** in `MillionaireGame.Web.Data/`:
- Manages: Sessions, Participants, FFFAnswers, ATAVotes, ParticipantHistory
- Uses: Entity Framework Core with proper configurations
- Pattern: Standard EF Core DbContext with migrations

### 3. Database Context Classes
- **`GameDatabaseContext`** (`Core.Database`) - Database creation, table initialization, connection management
- **`WAPSDbContext`** (`Web.Data`) - EF Core context for web tables

### 4. Service Layer
Services instantiate repositories in their constructors:
```csharp
public class ThemeService
{
    private readonly ThemeRepository _themeRepository;
    private readonly ThemeBackgroundRepository _backgroundRepository;
    
    public ThemeService(string connectionString)
    {
        _themeRepository = new ThemeRepository(connectionString);
        _backgroundRepository = new ThemeBackgroundRepository(connectionString);
    }
}
```

### 5. Connection String Management
**Centralized via `SqlSettingsManager`:**
```csharp
var sqlSettings = new SqlSettingsManager();
var connectionString = sqlSettings.Settings.GetConnectionString("dbMillionaire");
```

---

## Problems Identified

### 🔴 **Critical: Duplicate FFFQuestionRepository**
**Location:**
- `MillionaireGame.Core.Database.FFFQuestionRepository`
- `MillionaireGame.Web.Database.FFFQuestionRepository`

**Issue:**
- Both implementations are **identical**
- Both query the same `fff_questions` table
- Different namespaces cause confusion
- FFFOnlinePanel was incorrectly using Web version

**Fix Required:** Delete Web version, use Core version everywhere

---

### 🟡 **Medium: No Dependency Injection**

**Current Pattern:**
```csharp
// ❌ Manual instantiation everywhere
var questionRepo = new QuestionRepository(connectionString);
var themeService = new ThemeService(connectionString);
```

**Problems:**
1. **Tight Coupling** - Hard to mock for testing
2. **Connection String Duplication** - Passed to every service/repository
3. **No Lifecycle Management** - Each instance creates its own connections
4. **Hard to Replace** - Can't swap implementations

**Recommended Pattern:**
```csharp
// ✅ Dependency injection
services.AddScoped<IQuestionRepository, QuestionRepository>();
services.AddScoped<IThemeService, ThemeService>();

// Usage in forms/services
public ControlPanelForm(IQuestionRepository questionRepo, IThemeService themeService)
{
    _questionRepo = questionRepo;
    _themeService = themeService;
}
```

---

### 🟡 **Medium: Inconsistent Data Access Patterns**

**Two Different Approaches:**

| Component | Pattern | Technology | Location |
|-----------|---------|------------|----------|
| Core repositories | Manual ADO.NET | SqlConnection/SqlCommand | `Core.Database` |
| Web WAPS | Entity Framework | DbContext | `Web.Data` |

**Problems:**
1. **Learning Curve** - Developers must know both patterns
2. **Code Duplication** - Connection management repeated
3. **Inconsistent Error Handling** - Different approaches per pattern
4. **No Transaction Support** - Can't coordinate across patterns

**Options:**
- **Option A:** Migrate everything to Entity Framework
- **Option B:** Keep ADO.NET but use a shared base repository class
- **Option C:** Hybrid approach with clear boundaries

---

### 🟢 **Minor: No Base Repository Class**

**Current State:** Each repository duplicates:
- Connection string storage
- Connection creation/disposal
- Error handling patterns
- Parameter binding

**Recommended:**
```csharp
public abstract class BaseRepository
{
    protected readonly string ConnectionString;
    
    protected BaseRepository(string connectionString)
    {
        ConnectionString = connectionString;
    }
    
    protected async Task<SqlConnection> OpenConnectionAsync()
    {
        var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        return connection;
    }
}

public class QuestionRepository : BaseRepository
{
    public QuestionRepository(string connectionString) : base(connectionString) { }
    
    public async Task<Question?> GetRandomQuestionAsync(int questionNumber)
    {
        using var connection = await OpenConnectionAsync();
        // ... execute query
    }
}
```

---

## Recommended Refactoring Plan

### **Phase 1: Immediate Fixes (High Priority)**

#### 1.1 Eliminate Duplicate FFFQuestionRepository ✅ **CRITICAL**
- **Action:** Delete `MillionaireGame.Web.Database.FFFQuestionRepository`
- **Update:** Change FFFOnlinePanel to use Core version
- **Reason:** Avoid confusion, single source of truth
- **Effort:** 30 minutes
- **Impact:** Bug prevention, code clarity

#### 1.2 Fix FFFOnlinePanel Database Connection ✅ **COMPLETED**
- **Action:** Pass connection string from ControlPanelForm
- **Reason:** Use validated connection string from DI
- **Status:** Already fixed in this session

---

### **Phase 2: Introduce Base Repository (Medium Priority)**

#### 2.1 Create BaseRepository Class
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
        command.Parameters.AddRange(parameters);
        
        return await command.ExecuteNonQueryAsync();
    }
}
```

#### 2.2 Refactor All Repositories to Inherit BaseRepository
- Update each repository to extend `BaseRepository`
- Replace manual connection creation with `OpenConnectionAsync()`
- Use helper methods for common operations
- **Effort:** 2-3 hours (10 repositories)
- **Benefit:** Reduced code duplication, consistent error handling

---

### **Phase 3: Introduce Dependency Injection (Long Term)**

#### 3.1 Define Repository Interfaces
Create interfaces for all repositories:
```csharp
public interface IQuestionRepository
{
    Task<Question?> GetRandomQuestionAsync(int questionNumber);
    Task<List<Question>> GetQuestionsByLevelAsync(int level);
    Task MarkQuestionAsUsedAsync(int questionId);
    // ... other methods
}

public class QuestionRepository : BaseRepository, IQuestionRepository
{
    // Implementation
}
```

#### 3.2 Set Up Service Provider
**File:** `Program.cs` (Main entry point)

```csharp
var services = new ServiceCollection();

// Register connection string
var sqlSettings = new SqlSettingsManager();
var connectionString = sqlSettings.Settings.GetConnectionString("dbMillionaire");
services.AddSingleton(connectionString); // or configure options pattern

// Register repositories
services.AddScoped<IQuestionRepository, QuestionRepository>();
services.AddScoped<IFFFQuestionRepository, FFFQuestionRepository>();
services.AddScoped<IThemeRepository, ThemeRepository>();
// ... register all repositories

// Register services
services.AddScoped<IGameService, GameService>();
services.AddScoped<IThemeService, ThemeService>();
services.AddScoped<ISoundService, SoundService>();

// Register forms (with DI)
services.AddTransient<ControlPanelForm>();
services.AddTransient<FFFWindow>();

var serviceProvider = services.BuildServiceProvider();

// Start application
var mainForm = serviceProvider.GetRequiredService<ControlPanelForm>();
Application.Run(mainForm);
```

#### 3.3 Update Forms and Services
Change constructors to accept dependencies:
```csharp
public class ControlPanelForm : Form
{
    private readonly IQuestionRepository _questionRepo;
    private readonly IGameService _gameService;
    private readonly ISoundService _soundService;
    
    public ControlPanelForm(
        IQuestionRepository questionRepo,
        IGameService gameService,
        ISoundService soundService)
    {
        _questionRepo = questionRepo;
        _gameService = gameService;
        _soundService = soundService;
        
        InitializeComponent();
    }
}
```

**Effort:** 1-2 weeks (requires refactoring entire application)  
**Benefit:** Testability, maintainability, flexibility

---

### **Phase 4: Unify Data Access Patterns (Future Consideration)**

#### Option A: Migrate to Entity Framework Core
**Pros:**
- Modern ORM with strong tooling
- Migrations support (already used for WAPS tables)
- LINQ queries (more readable)
- Automatic change tracking

**Cons:**
- Performance overhead for simple queries
- Learning curve for team
- Requires significant refactoring
- May not be needed for simple CRUD operations

#### Option B: Keep ADO.NET with Improved Abstraction
**Pros:**
- Maintains current performance
- Minimal refactoring needed
- Team already familiar with pattern
- Fine-grained control over SQL

**Cons:**
- More manual code
- No automatic migrations
- Manual object mapping

**Recommendation:** **Option B** (Keep ADO.NET with BaseRepository)
- Application doesn't have complex relationships requiring ORM
- Performance is important for real-time game operations
- Team is comfortable with ADO.NET
- BaseRepository provides sufficient abstraction

---

## Implementation Priority

### ✅ **HIGH PRIORITY (Do Now)**
1. **Delete duplicate FFFQuestionRepository** (Web version)
2. **Update all references** to use Core version

### 🟡 **MEDIUM PRIORITY (Next Sprint)**
3. **Create BaseRepository class**
4. **Refactor 10 repositories** to inherit from BaseRepository
5. **Document repository usage** in developer guide

### 🔵 **LOW PRIORITY (Future)**
6. **Introduce repository interfaces** (for testing)
7. **Set up dependency injection** (major refactor)
8. **Add unit tests** for repositories

---

## Conclusion

The codebase **already follows good practices** with the repository pattern, but needs refinement:

### **Current State:** 🟡 **ADEQUATE**
- Repositories exist and are used consistently
- No direct SQL in UI layer
- Connection strings managed centrally

### **After Phase 1:** 🟢 **GOOD**
- No duplicate code
- All references correct
- Consistent namespace usage

### **After Phase 2:** 🟢 **VERY GOOD**
- Shared base class
- Reduced duplication
- Consistent error handling

### **After Phase 3:** 🔵 **EXCELLENT**
- Full dependency injection
- Highly testable
- Maximum flexibility

---

## Next Steps

1. **Review this plan** with team
2. **Execute Phase 1** (immediate fixes) - ~1 hour
3. **Schedule Phase 2** (base repository) - ~4 hours
4. **Decide on Phase 3** (DI) - requires architectural discussion

**Recommendation:** Implement **Phase 1 immediately**, **Phase 2 next week**, defer Phase 3 for post-v1.0.7 release.
