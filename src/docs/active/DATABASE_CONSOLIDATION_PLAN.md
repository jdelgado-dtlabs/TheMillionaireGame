# Database Consolidation Plan
**The Millionaire Game - C# Edition**  
**Target**: v1.0  
**Date**: December 31, 2025  
**Estimated Time**: 2-3 hours  
**Priority**: CRITICAL (Required before testing)

---

## 🎯 Objective

Migrate WAPS (Web-Based Audience Participation System) from SQLite to SQL Server Express, consolidating all application data into a single database for professional production architecture.

## 📊 Current Architecture Issues

### **Split Database Problem**:
- **Main Game Data**: SQL Server Express (dbMillionaire)
  - Questions table
  - ApplicationSettings table
  - FFFQuestions table
  
- **WAPS Data**: SQLite file database (waps.db)
  - Sessions table
  - Participants table
  - FFFAnswers table
  - ATAVotes table
  
- **Complications**:
  - Two separate databases to manage
  - SQLite file can be locked/corrupted
  - Backup requires two separate operations
  - Cannot use SQL Server transactions across both databases
  - Unprofessional split architecture
  - SQLite dependency adds complexity

### **Current Implementation**:
```
dbMillionaire (SQL Server Express)
  ├─ Questions
  ├─ FFFQuestions
  └─ ApplicationSettings

waps.db (SQLite File)
  ├─ Sessions
  ├─ Participants
  ├─ FFFAnswers
  └─ ATAVotes
```

---

## 🏗️ Target Architecture

### **Unified SQL Server Express Database**:
```
dbMillionaire (SQL Server Express) - SINGLE DATABASE
  ├─ Questions table (existing)
  ├─ FFFQuestions table (existing)
  ├─ ApplicationSettings table (existing)
  ├─ Sessions table (from SQLite → SQL Server)
  ├─ Participants table (from SQLite → SQL Server)
  ├─ FFFAnswers table (from SQLite → SQL Server)
  └─ ATAVotes table (from SQLite → SQL Server)
```

### **Benefits**:
- ✅ Single database backup captures everything
- ✅ Transactional consistency across all data
- ✅ Professional production architecture
- ✅ Simplified deployment (one connection string)
- ✅ SQL Server's reliability and ACID guarantees
- ✅ No SQLite file locking issues
- ✅ Better performance for concurrent web participants
- ✅ Built-in backup/restore tools
- ✅ Eliminates SQLite dependency

---

## 📋 Implementation Phases

### **Phase 1: Add WAPS Tables to SQL Server** (30 minutes)

**Tasks**:
1. Update GameDatabaseContext.cs to include WAPS table creation
2. Add Sessions, Participants, FFFAnswers, ATAVotes tables to CreateDatabaseAsync()
3. Match existing WAPSDbContext schema exactly (foreign keys, indexes, constraints)
4. Test table creation in dbMillionaire

**SQL Schema to Add**:
```sql
-- Sessions table
CREATE TABLE Sessions (
    Id NVARCHAR(450) PRIMARY KEY,
    HostName NVARCHAR(100) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    StartedAt DATETIME2 NULL,
    EndedAt DATETIME2 NULL,
    Status NVARCHAR(50) NOT NULL
);

CREATE INDEX IX_Sessions_CreatedAt ON Sessions(CreatedAt);

-- Participants table
CREATE TABLE Participants (
    Id NVARCHAR(450) PRIMARY KEY,
    SessionId NVARCHAR(450) NOT NULL,
    DisplayName NVARCHAR(50) NOT NULL,
    ConnectionId NVARCHAR(450) NULL,
    JoinedAt DATETIME2 NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    DeviceType NVARCHAR(50) NULL,
    Browser NVARCHAR(100) NULL,
    FOREIGN KEY (SessionId) REFERENCES Sessions(Id) ON DELETE CASCADE
);

CREATE INDEX IX_Participants_SessionId ON Participants(SessionId);
CREATE INDEX IX_Participants_ConnectionId ON Participants(ConnectionId);

-- FFFAnswers table
CREATE TABLE FFFAnswers (
    Id INT PRIMARY KEY IDENTITY(1,1),
    SessionId NVARCHAR(450) NOT NULL,
    ParticipantId NVARCHAR(450) NOT NULL,
    QuestionId INT NOT NULL,
    AnswerSequence NVARCHAR(20) NOT NULL,
    SubmittedAt DATETIME2 NOT NULL,
    TimeTaken FLOAT NOT NULL,
    IsCorrect BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (SessionId) REFERENCES Sessions(Id) ON DELETE CASCADE
);

CREATE INDEX IX_FFFAnswers_SessionId_QuestionId ON FFFAnswers(SessionId, QuestionId);
CREATE INDEX IX_FFFAnswers_SubmittedAt ON FFFAnswers(SubmittedAt);

-- ATAVotes table
CREATE TABLE ATAVotes (
    Id INT PRIMARY KEY IDENTITY(1,1),
    SessionId NVARCHAR(450) NOT NULL,
    ParticipantId NVARCHAR(450) NOT NULL,
    QuestionText NVARCHAR(500) NOT NULL,
    SelectedOption NVARCHAR(1) NOT NULL,
    SubmittedAt DATETIME2 NOT NULL,
    FOREIGN KEY (SessionId) REFERENCES Sessions(Id) ON DELETE CASCADE
);

CREATE INDEX IX_ATAVotes_SessionId ON ATAVotes(SessionId);
CREATE INDEX IX_ATAVotes_SubmittedAt ON ATAVotes(SubmittedAt);
```

**Files to Modify**:
- `MillionaireGame.Core/Database/GameDatabaseContext.cs` - Add WAPS tables to CreateDatabaseAsync()

---

### **Phase 2: Update WAPSDbContext for SQL Server** (45 minutes)

**Current Implementation** (SQLite):
```csharp
// WebServerHost.cs - ConfigureServices()
services.AddDbContext<WAPSDbContext>(options =>
{
    var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "waps.db");
    options.UseSqlite($"Data Source={dbPath}");
});
```

**New Implementation** (SQL Server):
```csharp
services.AddDbContext<WAPSDbContext>(options =>
{
    options.UseSqlServer(_sqlConnectionString);
});
```

**Tasks**:
1. Update WebServerHost.cs ConfigureServices() to use UseSqlServer instead of UseSqlite
2. Update MillionaireGame.Web.csproj to replace SQLite packages with SQL Server
3. Remove SQLite NuGet packages
4. Add Microsoft.EntityFrameworkCore.SqlServer package
5. Test DbContext initialization

**Files to Modify**:
- `MillionaireGame/Hosting/WebServerHost.cs` (lines 336-339)
- `MillionaireGame.Web/MillionaireGame.Web.csproj`

---

### **Phase 3: Improve Database Cleanup Logic** (30 minutes)

**Current "Hack"** (lines 177-196 in WebServerHost.cs):
```csharp
// Ensure database is created
context.Database.EnsureCreated();

// Clear all tables to ensure clean state on startup
try
{
    // Delete in correct order to respect foreign key constraints
    var deletedVotes = context.ATAVotes.ExecuteDelete();
    var deletedAnswers = context.FFFAnswers.ExecuteDelete();
    var deletedParticipants = context.Participants.ExecuteDelete();
    var deletedSessions = context.Sessions.ExecuteDelete();
    
    WebServerConsole.Info($"[WebServer] Database cleared...");
}
catch (Exception ex)
{
    WebServerConsole.Warn($"[WebServer] Could not clear database tables: {ex.Message}");
}
```

**Problem with Current Approach**:
- `EnsureCreated()` won't work with SQL Server managed database
- Tables already exist in dbMillionaire
- Need to check if tables exist before clearing
- Should use proper migration approach

**Improved Implementation**:
```csharp
// Ensure WAPS tables exist (they should be created by GameDatabaseContext)
// No need for EnsureCreated() since main app creates database

// Clear WAPS tables for clean session state on startup
try
{
    // Delete in correct order to respect foreign key constraints
    var deletedVotes = await context.ATAVotes.ExecuteDeleteAsync();
    var deletedAnswers = await context.FFFAnswers.ExecuteDeleteAsync();
    var deletedParticipants = await context.Participants.ExecuteDeleteAsync();
    var deletedSessions = await context.Sessions.ExecuteDeleteAsync();
    
    WebServerConsole.Info($"[WebServer] WAPS data cleared: {deletedSessions} sessions, {deletedParticipants} participants, {deletedAnswers} FFF answers, {deletedVotes} ATA votes");
}
catch (Exception ex)
{
    WebServerConsole.Error($"[WebServer] Failed to clear WAPS data: {ex.Message}");
    throw; // Don't start web server if we can't clear old data
}
```

**Alternative: Add ClearWAPSData method to SessionService**:
```csharp
public async Task ClearAllSessionDataAsync()
{
    await _context.Database.BeginTransactionAsync();
    try
    {
        await _context.ATAVotes.ExecuteDeleteAsync();
        await _context.FFFAnswers.ExecuteDeleteAsync();
        await _context.Participants.ExecuteDeleteAsync();
        await _context.Sessions.ExecuteDeleteAsync();
        
        await _context.Database.CommitTransactionAsync();
        _logger.LogInformation("All WAPS session data cleared");
    }
    catch
    {
        await _context.Database.RollbackTransactionAsync();
        throw;
    }
}
```

**Tasks**:
1. Remove `EnsureCreated()` call (tables managed by GameDatabaseContext)
2. Make ExecuteDelete calls async
3. Add proper error handling and transaction support
4. Consider adding ClearWAPSData method to SessionService
5. Test database cleanup on web server startup

**Files to Modify**:
- `MillionaireGame/Hosting/WebServerHost.cs` (StartAsync method)
- `MillionaireGame.Web/Services/SessionService.cs` (optional)

---

### **Phase 4: Remove SQLite Dependencies** (15 minutes)

**Tasks**:
1. Remove SQLite NuGet packages from MillionaireGame.Web.csproj:
   - Microsoft.EntityFrameworkCore.Sqlite
   - Microsoft.Data.Sqlite.Core
   - SQLitePCLRaw.* packages
2. Add SQL Server package if not present:
   - Microsoft.EntityFrameworkCore.SqlServer (Version 8.0.*)
3. Clean and rebuild solution
4. Verify no SQLite references remain

**Files to Modify**:
- `MillionaireGame.Web/MillionaireGame.Web.csproj`

**Before**:
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.*" />
<PackageReference Include="QRCoder" Version="1.7.0" />
<PackageReference Include="System.Data.SqlClient" Version="4.9.0" />
```

**After**:
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.*" />
<PackageReference Include="QRCoder" Version="1.7.0" />
<PackageReference Include="System.Data.SqlClient" Version="4.9.0" />
```

---

### **Phase 5: Testing** (45 minutes)

**Test Cases**:

1. **Database Creation**:
   - Delete dbMillionaire database
   - Run application
   - Verify all tables created (Questions, ApplicationSettings, Sessions, Participants, FFFAnswers, ATAVotes)
   - Check table schemas match expectations

2. **Web Server Startup**:
   - Start application
   - Start WAPS web server
   - Verify WAPS tables cleared on startup
   - Check WebServerConsole logs for cleanup confirmation
   - No SQLite file (waps.db) should be created

3. **WAPS Functionality**:
   - Start web server
   - Open browser to join URL
   - Create participant
   - Verify participant saved to SQL Server Participants table
   - Test FFF Online submission
   - Verify FFFAnswers saved to SQL Server
   - Test ATA Online voting
   - Verify ATAVotes saved to SQL Server

4. **Session Management**:
   - Create session via web server
   - Add multiple participants
   - Submit FFF answers
   - Submit ATA votes
   - Restart web server
   - Verify all data cleared on restart
   - Verify no orphaned data

5. **Concurrent Users**:
   - Start web server
   - Connect 10+ participants simultaneously
   - All submit FFF answers at same time
   - Verify all answers persisted correctly
   - No database locking issues
   - No transaction conflicts

6. **Backup/Restore**:
   - Backup dbMillionaire using SQL Server Management Studio
   - Modify game settings
   - Add WAPS participants
   - Restore database backup
   - Verify settings and WAPS data restored correctly

7. **Error Handling**:
   - Test with database unavailable
   - Verify graceful error messages
   - Test with corrupted connection string
   - Verify web server doesn't start with proper error

---

## 🚨 Risks and Mitigation

### **Risk 1: Data Loss During Migration**
**Mitigation**:
- SQLite waps.db is temporary session data (cleared on startup anyway)
- No persistent data to migrate
- If needed, can query old waps.db and insert into SQL Server

### **Risk 2: Schema Mismatch**
**Problem**: WAPSDbContext schema doesn't match SQL Server tables
**Solution**:
- Copy exact schema from WAPSDbContext.OnModelCreating()
- Test thoroughly before merging
- Use EF migrations to ensure consistency

### **Risk 3: Performance Difference**
**Problem**: SQL Server might be slower than SQLite for some operations
**Mitigation**:
- SQL Server is actually faster for concurrent writes
- Proper indexing already defined in WAPSDbContext
- Connection pooling improves performance

### **Risk 4: Connection String Issues**
**Problem**: Wrong connection string passed to WAPSDbContext
**Mitigation**:
- Use same connection string as GameDatabaseContext
- Test connection before web server starts
- Add validation in WebServerHost constructor

---

## 📝 Files to Modify

### **Database Schema**:
1. `MillionaireGame.Core/Database/GameDatabaseContext.cs` - Add WAPS table creation

### **DbContext Configuration**:
2. `MillionaireGame/Hosting/WebServerHost.cs` - Update to UseSqlServer, improve cleanup logic

### **Package References**:
3. `MillionaireGame.Web/MillionaireGame.Web.csproj` - Replace SQLite with SQL Server packages

### **Optional Enhancements**:
4. `MillionaireGame.Web/Services/SessionService.cs` - Add ClearAllSessionDataAsync method

---

## ✅ Acceptance Criteria

- [ ] WAPS tables created in SQL Server dbMillionaire database
- [ ] WAPSDbContext uses SQL Server instead of SQLite
- [ ] No waps.db file created on web server startup
- [ ] All SQLite NuGet packages removed
- [ ] Web server starts successfully with SQL Server
- [ ] Participants can join and data saves to SQL Server
- [ ] FFF Online submissions save to SQL Server
- [ ] ATA Online votes save to SQL Server
- [ ] Database cleanup works on web server restart
- [ ] No database locking issues with concurrent users
- [ ] Single backup captures all data (game + WAPS)
- [ ] All tests pass (startup, WAPS, concurrent, backup)

---

## 📊 Success Metrics

**Before**:
- 2 databases (SQL Server + SQLite file)
- SQLite file locking issues possible
- Backup requires database + file copy
- Split architecture

**After**:
- 1 database (SQL Server only)
- No file locking issues
- Single backup command
- Professional unified architecture
- Better concurrent write performance

---

## 🔄 Rollback Plan

If consolidation fails or introduces critical bugs:

1. Revert to previous commit on feature branch
2. SQLite waps.db will be recreated automatically
3. Main game database (dbMillionaire) unchanged
4. Investigate issues before retry

---

## 📅 Timeline

**Total**: 2-3 hours
- Phase 1: Add WAPS Tables to SQL Server (30 min)
- Phase 2: Update WAPSDbContext (45 min)
- Phase 3: Improve Cleanup Logic (30 min)
- Phase 4: Remove SQLite Dependencies (15 min)
- Phase 5: Testing (45 min)

**Critical Path**: Must complete before end-to-end testing

---

**Status**: Ready to implement  
**Next Step**: Phase 1 - Add WAPS tables to GameDatabaseContext.CreateDatabaseAsync()
