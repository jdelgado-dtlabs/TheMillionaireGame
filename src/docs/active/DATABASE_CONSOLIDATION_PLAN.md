# Database Consolidation Plan
**The Millionaire Game - C# Edition**  
**Target**: v1.0  
**Date**: December 31, 2025  
**Estimated Time**: 3-4 hours  
**Priority**: CRITICAL (Required before testing)

---

## 🎯 Objective

Consolidate all data storage into a single SQL Server database, eliminating XML file dependencies and creating a professional production-ready architecture.

## 📊 Current Architecture Issues

### **Split Storage Problem**:
- **Settings**: Stored in XML files (settings.xml)
- **WAPS Data**: SQL Server database (SQL 2022 LocalDB)
- **Complications**:
  - Split storage complicates backups
  - Manual file + database backup required
  - Unprofessional for production deployment
  - XML files prone to corruption
  - No transactional consistency between settings and game data

### **Current Implementation**:
```
ApplicationSettings (XML)
  └─ settings.xml
     └─ Audio settings
     └─ Display settings
     └─ Database connection strings
     └─ Web server configuration

GameDatabase (SQL Server)
  ├─ Questions table
  ├─ FFFSubmissions table
  ├─ ATAVotes table
  └─ Participants table
```

---

## 🏗️ Target Architecture

### **Unified SQL Server Database**:
```
GameDatabase (SQL Server) - SINGLE SOURCE OF TRUTH
  ├─ Questions table (existing)
  ├─ FFFSubmissions table (existing)
  ├─ ATAVotes table (existing)
  ├─ Participants table (existing)
  └─ Settings table (NEW)
     ├─ SettingId (INT, PK, IDENTITY)
     ├─ Category (NVARCHAR(50)) - e.g., "Audio", "Display", "Database"
     ├─ Key (NVARCHAR(100)) - e.g., "MasterVolume", "ScreenResolution"
     ├─ Value (NVARCHAR(MAX)) - JSON or string value
     ├─ DataType (NVARCHAR(20)) - "String", "Int", "Bool", "Double"
     └─ LastModified (DATETIME2)
```

### **Benefits**:
- ✅ Single database backup captures everything
- ✅ Transactional consistency across all data
- ✅ Professional production architecture
- ✅ Simplified deployment (one connection string)
- ✅ SQL Server's reliability and performance
- ✅ Easy migration between environments
- ✅ Built-in backup/restore tools

---

## 📋 Implementation Phases

### **Phase 1: Create Settings Table** (30 minutes)

**Tasks**:
1. Create Settings table schema in SQL Server
2. Add indexes on (Category, Key) for fast lookups
3. Create stored procedures for CRUD operations (optional)
4. Test table creation and basic operations

**SQL Schema**:
```sql
CREATE TABLE Settings (
    SettingId INT PRIMARY KEY IDENTITY(1,1),
    Category NVARCHAR(50) NOT NULL,
    [Key] NVARCHAR(100) NOT NULL,
    Value NVARCHAR(MAX) NOT NULL,
    DataType NVARCHAR(20) NOT NULL, -- 'String', 'Int', 'Bool', 'Double'
    LastModified DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT UQ_Settings_CategoryKey UNIQUE (Category, [Key])
);

CREATE INDEX IX_Settings_Category ON Settings(Category);
CREATE INDEX IX_Settings_CategoryKey ON Settings(Category, [Key]);
```

**Files to Modify**:
- `MillionaireGame.Core/Database/GameDatabaseContext.cs` (add Settings DbSet)
- Create migration script if using EF migrations

---

### **Phase 2: Update ApplicationSettingsRepository** (1.5 hours)

**Current Implementation** (XML-based):
```csharp
// MillionaireGame.Core/Settings/ApplicationSettingsRepository.cs
public class ApplicationSettingsRepository
{
    private readonly string _settingsPath;
    
    public ApplicationSettings Load()
    {
        // Loads from settings.xml
    }
    
    public void Save(ApplicationSettings settings)
    {
        // Saves to settings.xml
    }
}
```

**New Implementation** (SQL-based):
```csharp
public class ApplicationSettingsRepository
{
    private readonly GameDatabaseContext _dbContext;
    
    public ApplicationSettings Load()
    {
        // Loads from SQL Settings table
        // Deserialize values by DataType
    }
    
    public void Save(ApplicationSettings settings)
    {
        // Saves to SQL Settings table
        // Uses transactions for consistency
    }
    
    private void MigrateFromXmlIfNeeded()
    {
        // One-time migration on first run
        // Read settings.xml
        // Write to SQL
        // Rename settings.xml to settings.xml.migrated
    }
}
```

**Tasks**:
1. Inject GameDatabaseContext into ApplicationSettingsRepository
2. Implement Load() to read from Settings table
3. Implement Save() to write to Settings table with transaction
4. Add MigrateFromXmlIfNeeded() for one-time migration
5. Update constructor to accept DbContext
6. Test load/save operations

**Files to Modify**:
- `MillionaireGame.Core/Settings/ApplicationSettingsRepository.cs`
- `MillionaireGame/Program.cs` (DI configuration)

---

### **Phase 3: Update Service Registration** (30 minutes)

**Tasks**:
1. Update Program.cs dependency injection
2. Ensure GameDatabaseContext is available to ApplicationSettingsRepository
3. Remove XML file path dependencies
4. Update configuration flow

**Files to Modify**:
- `MillionaireGame/Program.cs`

**Before**:
```csharp
var settingsRepo = new ApplicationSettingsRepository("settings.xml");
var settings = settingsRepo.Load();
```

**After**:
```csharp
services.AddDbContext<GameDatabaseContext>(options => 
    options.UseSqlServer(connectionString));
services.AddSingleton<ApplicationSettingsRepository>();
```

---

### **Phase 4: Migration Logic** (1 hour)

**Automatic XML → SQL Migration**:
```csharp
private void MigrateFromXmlIfNeeded()
{
    var xmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.xml");
    
    if (!File.Exists(xmlPath))
        return; // No XML to migrate
    
    // Check if migration already done
    var existingSettings = _dbContext.Settings.Any();
    if (existingSettings)
        return; // Already migrated
    
    try
    {
        // Load from XML
        var xmlSettings = LoadFromXml(xmlPath);
        
        // Save to SQL
        SaveToSql(xmlSettings);
        
        // Rename XML to prevent re-migration
        File.Move(xmlPath, xmlPath + ".migrated");
        
        GameConsole.Info("Settings migrated from XML to SQL Server");
    }
    catch (Exception ex)
    {
        GameConsole.Error($"Migration failed: {ex.Message}");
        throw;
    }
}
```

**Tasks**:
1. Implement migration logic
2. Test with existing settings.xml
3. Verify all settings preserved
4. Handle migration errors gracefully
5. Log migration status

---

### **Phase 5: Remove XML Dependencies** (30 minutes)

**Tasks**:
1. Remove XML serialization code from ApplicationSettings.cs
2. Remove file I/O operations
3. Update error handling for database operations
4. Clean up unused XML-related methods
5. Update documentation

**Files to Modify**:
- `MillionaireGame.Core/Settings/ApplicationSettings.cs`
- `MillionaireGame.Core/Settings/ApplicationSettingsRepository.cs`

---

### **Phase 6: Verify WAPS Integration** (30 minutes)

**Tasks**:
1. Confirm FFFSubmissions, ATAVotes, Participants tables accessible
2. Test WAPS data access with consolidated database
3. Verify SessionService.cs uses correct connection
4. Test FFF Online and ATA Online modes
5. Confirm no connection string conflicts

**Files to Verify**:
- `MillionaireGame.Web/Services/SessionService.cs`
- `MillionaireGame.Core/Database/GameDatabaseContext.cs`
- `MillionaireGame.Web/Program.cs`

---

### **Phase 7: Testing** (1 hour)

**Test Cases**:
1. **Fresh Install**:
   - Run application with no settings.xml
   - Verify default settings loaded
   - Modify settings in Options dialog
   - Confirm settings saved to SQL
   - Restart application
   - Verify settings persisted

2. **XML Migration**:
   - Place existing settings.xml in application folder
   - Run application
   - Verify settings migrated to SQL
   - Confirm settings.xml renamed to .migrated
   - Verify all settings values correct

3. **WAPS Functionality**:
   - Start web server
   - Test FFF Online with participants
   - Test ATA Online with voting
   - Verify data persisted to same database
   - Check Participants table

4. **Settings Persistence**:
   - Change audio settings → Verify SQL update
   - Change display settings → Verify SQL update
   - Change database connection → Verify SQL update
   - Restart application → Verify all settings loaded

5. **Error Handling**:
   - Test with database unavailable
   - Verify graceful error messages
   - Test with corrupted settings row
   - Verify fallback to defaults

6. **Backup/Restore**:
   - Backup database using SQL Server tools
   - Modify settings
   - Restore database backup
   - Verify settings restored correctly

---

## 🚨 Risks and Mitigation

### **Risk 1: Data Loss During Migration**
**Mitigation**:
- Rename XML file to .migrated (don't delete)
- Keep XML as backup until migration verified
- Add rollback mechanism if migration fails

### **Risk 2: Connection String Circular Dependency**
**Problem**: Connection string stored in settings, but need connection to load settings
**Solution**:
- Store connection string in App.config as fallback
- Load connection string from config first
- Use that to connect and load other settings

### **Risk 3: Performance Impact**
**Mitigation**:
- Cache settings in memory after load
- Only hit database on Save() operations
- Use indexes on Settings table for fast lookups

### **Risk 4: Breaking Existing Installations**
**Mitigation**:
- Automatic migration on first run
- Preserve XML file as backup
- Comprehensive testing before release

---

## 📝 Files to Modify

### **Core Changes**:
1. `MillionaireGame.Core/Database/GameDatabaseContext.cs` - Add Settings DbSet
2. `MillionaireGame.Core/Settings/ApplicationSettingsRepository.cs` - SQL implementation
3. `MillionaireGame.Core/Settings/ApplicationSettings.cs` - Remove XML serialization

### **Dependency Injection**:
4. `MillionaireGame/Program.cs` - Update service registration

### **Migration**:
5. `MillionaireGame.Core/Settings/SettingsMigrator.cs` (NEW) - XML → SQL migration logic

### **Configuration**:
6. `MillionaireGame/App.config` - Add connection string fallback

---

## ✅ Acceptance Criteria

- [ ] Settings table created in SQL Server
- [ ] All settings load from SQL on application start
- [ ] All settings save to SQL when modified
- [ ] XML settings automatically migrated on first run
- [ ] No XML file dependencies in code
- [ ] WAPS tables accessible from same database
- [ ] Single backup captures all data (settings + game data)
- [ ] All tests pass (fresh install, migration, WAPS, persistence)
- [ ] Performance acceptable (no lag loading settings)
- [ ] Error handling graceful (database unavailable)

---

## 📊 Success Metrics

**Before**:
- 2 data sources (XML + SQL Server)
- Manual backup of 2 locations
- Potential sync issues between settings and game data

**After**:
- 1 data source (SQL Server)
- Single backup command
- Transactional consistency
- Professional production architecture

---

## 🔄 Rollback Plan

If consolidation fails or introduces critical bugs:

1. Revert to previous commit (feature branch)
2. Restore settings.xml from .migrated backup
3. Use XML-based ApplicationSettingsRepository
4. Investigate issues before retry

---

## 📅 Timeline

**Total**: 3-4 hours
- Phase 1: Create Settings Table (30 min)
- Phase 2: Update Repository (1.5 hours)
- Phase 3: Service Registration (30 min)
- Phase 4: Migration Logic (1 hour)
- Phase 5: Remove XML (30 min)
- Phase 6: Verify WAPS (30 min)
- Phase 7: Testing (1 hour)

**Critical Path**: Must complete before end-to-end testing

---

**Status**: Ready to implement  
**Next Step**: Create feature/database-consolidation branch and begin Phase 1
