# First-Run Database Wizard - Implementation Plan

**Status:** ✅ Complete (Tested & Deployed)  
**Priority:** High  
**Target Version:** 1.0.6  
**Created:** 2026-01-10  
**Completed:** 2026-01-12

---

## Overview

Create a first-run wizard that appears when `sql.xml` doesn't exist, guiding users through database setup before the main application starts. This eliminates the current confusing "Database not found" MessageBox and provides a professional, guided setup experience.

---

## Problem Statement

Currently, when the application runs for the first time:
1. It attempts to load `sql.xml` settings
2. If settings don't exist, default settings are created
3. Application tries to connect and shows MessageBox if database doesn't exist
4. User has no guidance on what database to use or how to configure it
5. Setup is manual and error-prone

**Goal:** Provide a guided, user-friendly wizard for initial database configuration that:
- Uses LocalDB by default (bundled with installer)
- Allows advanced users to connect to existing SQL Server instances
- Tests connections before proceeding
- Creates the database automatically
- Optionally loads sample trivia data
- Saves configuration for future use

---

## Architecture

### Detection Logic (Program.cs)

**Before current database initialization:**
```csharp
// Check for command-line database type hint from installer
string dbTypeHint = null;
var args = Environment.GetCommandLineArgs();
for (int i = 0; i < args.Length - 1; i++)
{
    if (args[i] == "--db-type" && i + 1 < args.Length)
    {
        dbTypeHint = args[i + 1].ToLower(); // "sqlserver" or null (LocalDB default)
        break;
    }
}

// Check if sql.xml exists
var sqlSettings = new SqlSettingsManager();
if (!File.Exists(sqlSettings.FilePath))
{
    // Show first-run wizard with database type hint
    using var wizard = new FirstRunWizard(dbTypeHint);
    if (wizard.ShowDialog() != DialogResult.OK)
    {
        // User cancelled - cannot run without database
        return;
    }
}

// Now load settings and continue normally
sqlSettings.LoadSettings();
// ... existing code ...
```

**Note:** The installer detects existing SQL Server installations and passes `--db-type` parameter:
- No flag (default) = LocalDB detected or no SQL Server found
- `--db-type=sqlserver` = SQL Server Express or full SQL Server detected (both work identically)

---

## UI Design

### Form Layout

**Header Section:**
- Dark panel with centered title
- Title: "Welcome to The Millionaire Game"
- Subtitle: "Let's set up your database"

**Database Type Selection:**

Two mutually exclusive radio button options:

#### 1. ⚪ **LocalDB (Automatic)** [Default - Recommended]
- Simplest option - zero configuration needed
- Uses: `(LocalDB)\MSSQLLocalDB`
- Installed automatically with the game
- Lightweight, on-demand database (only runs when app is active)
- Perfect for single-user game show hosting
- **Status Label** showing:
  - "LocalDB detected and ready" (green, if available)
  - "LocalDB will be created on first run" (blue, if not initialized)
  - "LocalDB not installed - please reinstall the game" (red, if missing)

#### 2. ⚪ **SQL Server (Advanced)**
- For users who want to use existing SQL Server installation
- **ComboBox (dropdown)** - SSMS-style server selection:
  - Auto-populated with detected local instances via `SqlDataSourceEnumerator`
  - Shows instances like: `localhost\SQLEXPRESS`, `localhost\MSSQLSERVER`
  - **"<Browse for more...>"** option at bottom of list
  - When selected: Shows additional connection fields
- **Status Label** (during scan):
  - "Scanning for SQL Server instances..." (blue)
  - "Found X local instance(s)" (green)
  - "No instances detected" (gray)

**When "<Browse for more...>" is selected:**
- **GroupBox: "Server Connection Details"** (appears below dropdown)
  - Server/Instance: `[____________]` (TextBox) - e.g., "192.168.1.100" or "myserver.domain.com\INSTANCE"
  - Port: `[1433]` (NumericUpDown, 1-65535)
  - Database: `[dbMillionaire]` (TextBox, pre-filled)
  - Authentication: `[Windows Auth ▼]` or `[SQL Server Auth ▼]` (ComboBox)
  - Username: `[____________]` (TextBox, enabled only for SQL Server Auth)
  - Password: `[____________]` (TextBox, masked with ●, enabled only for SQL Server Auth)

**Connection Testing:**
- **Button:** "Test Connection" (always enabled)
- **Status Label:** Shows connection test results
  - "Ready to test connection" (gray, initial)
  - "Testing..." (blue, during test)
  - "✓ Server connection successful!" (green)
  - "✓ Server connected! Database 'dbMillionaire' already exists." (green)
  - "✓ Server connected! Database will be created on finish." (blue)
  - "✗ Connection failed: [error message]" (red)

**Database Creation Options:**
- **Checkbox:** ☑ "Load sample trivia data (init_database.sql)"
  - **Disabled by default**
  - Only enabled after successful connection test
  - Only shown/enabled if database doesn't already exist
  - Loads 80 questions + 44 FFF questions from `lib/sql/init_database.sql`

**Action Buttons:**
- **Test Connection** - Tests connection, checks if DB exists, enables Finish button and sample data checkbox
- **Finish** - **Disabled by default**, only enabled after successful connection test. Saves sql.xml, creates database, loads sample data (if checked)
- **Cancel** - Shows confirmation, exits application

---

## Technical Implementation

### 1. LocalDB Detection

```csharp
private async Task<bool> DetectLocalDBAsync()
{
    try
    {
        var connectionString = "Server=(LocalDB)\\MSSQLLocalDB;Integrated Security=true;TrustServerCertificate=True;";
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        return true;
    }
    catch
    {
        return false;
    }
}
```

**Notes:**
- Run on form load to verify LocalDB availability
- Update status label based on result
- If not available, show warning to reinstall game

### 2. SQL Server Instance Enumeration (SSMS-Style)

```csharp
using System.Data.Sql;

private async Task EnumerateSqlInstancesAsync()
{
    await Task.Run(() =>
    {
        var instanceTable = SqlDataSourceEnumerator.Instance.GetDataSources();
        
        // Add detected local instances
        foreach (DataRow row in instanceTable.Rows)
        {
            string? serverName = row["ServerName"]?.ToString();
            string? instanceName = row["InstanceName"]?.ToString();
            
            if (!string.IsNullOrEmpty(serverName))
            {
                string fullInstanceName;
                if (!string.IsNullOrEmpty(instanceName))
                {
                    fullInstanceName = $"{serverName}\\{instanceName}";
                }
                else
                {
                    fullInstanceName = serverName;
                }
                
                // Only add localhost instances to the initial list
                if (serverName.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                    serverName.Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase))
                {
                    cboServerInstances.Items.Add(fullInstanceName);
                }
            }
        }
        
        // Always add the "Browse for more..." option at the end
        cboServerInstances.Items.Add("<Browse for more...>");
    });
}
```

**Notes:**
- Run asynchronously on form load to avoid UI freeze
- May take 10-15 seconds to complete
- Show "Scanning..." status during enumeration
- Only show LOCAL instances in dropdown (not network instances)
- Always include "<Browse for more...>" option
- Handle exceptions gracefully (network issues, permissions)

### 3. Connection Testing Logic

```csharp
private async Task<bool> TestConnectionAsync()
{
    // 1. Build SqlConnectionSettings from UI
    UpdateSettingsFromUI();
    
    // 2. Test server connection (without database)
    var connectionString = _sqlSettings.GetConnectionString();
    using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();
    
    // 3. Check if database exists
    var dbContext = new GameDatabaseContext(_sqlSettings.GetConnectionString());
    bool dbExists = await dbContext.DatabaseExistsAsync();
    
    return dbExists;
}
```

### 4. Database Creation Flow

**On "Finish" button click:**

1. **Pre-condition Check**
   - Button is only enabled after successful connection test
   - Connection must have been tested and passed
   - No additional validation needed (already done during test)

2. **Save Configuration**
   ```csharp
   _settingsManager.Settings = _sqlSettings;
   _settingsManager.SaveSettings(); // Creates sql.xml
   ```

3. **Create Database** (if doesn't exist)
   ```csharp
   var dbContext = new GameDatabaseContext(_sqlSettings.GetConnectionString());
   await dbContext.CreateDatabaseAsync();
   ```
   - Creates `dbMillionaire` database
   - Creates all tables with current schema (questions, fff_questions, Sessions, Participants, etc.)
   - Uses existing `GameDatabaseContext.CreateDatabaseAsync()` method

4. **Load Sample Data** (if checkbox checked)
   ```csharp
   await LoadSampleDataAsync();
   ```
   - Locate `init_database.sql` from multiple possible paths
   - Parse SQL script and split by `GO` statements
   - Execute each batch sequentially
   - Handle errors gracefully

5. **Success**
   - Show success message
   - Close wizard with `DialogResult.OK`
   - Application continues to main form

### 5. Sample Data Loading

**File Location Resolution:**
```csharp
// File should be in lib/sql/ relative to the application directory
string sqlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lib", "sql", "init_database.sql");

if (!File.Exists(sqlFilePath))
{
    throw new FileNotFoundException("Could not find init_database.sql file", sqlFilePath);
}
```

**Script Execution:**
- Read entire SQL file
- Split by `GO` statements (handles different line endings)
- Execute each batch separately with try-catch
- Use 120-second command timeout (large INSERT statements)
- Skip empty batches and comment-only lines

---

## Code Reuse Strategy

### Reuse from DatabaseSettingsDialog

**UI Components:**
- Remote server GroupBox layout
- Connection test button logic
- Status label patterns

**Connection Logic:**
- Test connection implementation
- Error message formatting

**Settings Management:**
- SqlConnectionSettings creation
- SqlSettingsManager usage

**Differences:**
- LocalDB: Checkbox → Radio button (mutually exclusive)
- SQL Express: Not separate → Dedicated option with instance enumeration
- UI State: Always editable → Locked after successful test
- Purpose: Edit existing → Initial setup only
- Cancel behavior: Safe → Exits application

### Reuse from GameDatabaseContext

- `DatabaseExistsAsync()` - Check if database exists
- `CreateDatabaseAsync()` - Create database with schema
- Connection string building

---

## Error Handling

### Connection Failures
- **Symptom:** Cannot connect to server
- **Action:** Show clear error message in status label
- **Recovery:** Keep wizard open, allow user to modify settings and retry

### SQL Server Express Not Found
- **Symptom:** Instance enumeration returns empty or times out
- **Action:** Show warning label suggesting reinstall or alternative
- **Recovery:** User can select LocalDB or Remote options instead

### Database Creation Failure
- **Symptom:** CreateDatabaseAsync() throws exception
- **Action:** Show error dialog with full message
- **Recovery:** Keep wizard open, allow retry after fixing permissions/configuration

### Sample Data Loading Failure
- **Symptom:** init_database.sql not found or SQL execution fails
- **Action:** Show error but continue (database is created)
- **Recovery:** User can manually run init_database.sql later

### Cancel Confirmation
- **Dialog:** "Are you sure you want to cancel the setup?\n\nThe application cannot run without a database connection."
- **Options:** Yes (exits app), No (returns to wizard)

---

## UI Behavior Specifications

### Radio Button States

| Selected Option | Server Dropdown | Server Status Label | Connection Details GroupBox |
|----------------|-----------------|---------------------|-----------------------------|
| LocalDB (Automatic) | Disabled | Visible (shows LocalDB status) | Hidden |
| SQL Server (Advanced) | Enabled | Visible (shows scan status) | Hidden (until "<Browse for more...>" selected) |

**Additional State - When "<Browse for more...>" is selected:**
| Condition | Connection Details GroupBox | Auth Dropdown | Username/Password Fields |
|-----------|----------------------------|---------------|-------------------------|
| Browse selected | Visible | Enabled | Enabled based on Auth type |

### Button States

| Condition | Test Button | Finish Button | Cancel Button | Sample Data Checkbox |
|-----------|-------------|---------------|---------------|----------------------|
| Initial   | Enabled     | Disabled      | Enabled       | Disabled             |
| Testing   | Disabled    | Disabled      | Enabled       | Disabled             |
| Test Pass (DB exists) | Enabled | Enabled | Enabled | Disabled (grayed out) |
| Test Pass (DB new) | Enabled | Enabled | Enabled | Enabled |
| Test Fail | Enabled     | Disabled      | Enabled       | Disabled             |
| Finishing | Disabled    | Disabled      | Disabled      | Disabled             |

### Status Label Colors

- **Gray:** Initial/neutral state
- **Blue:** In progress (testing, scanning, creating)
- **Green:** Success
- **Red:** Error/warning

---

## Installer Intelligence (Inno Setup)

### SQL Server Detection Hierarchy

The installer (`MillionaireGameSetup.iss`) intelligently detects existing SQL Server installations and passes the appropriate `--db-type` parameter to the application on first run.

**Detection Priority:**

1. **SQL Server (any edition)** (`--db-type=sqlserver`)
   - Includes both SQL Server Express and full SQL Server
   - Registry check: `HKLM\SOFTWARE\Microsoft\Microsoft SQL Server\InstalledInstances` for any instances
   - File system check: `{pf}\Microsoft SQL Server\{version}\Setup Bootstrap\` directories
   - **Rationale:** SQL Server and SQL Server Express work identically from a connection perspective. The same dialog works for both local and remote instances.

2. **LocalDB** (no flag - default behavior)
   - File system check: `{pf}\Microsoft SQL Server\{version}\Tools\Binn\SqlLocalDB.exe`
   - Only selected if SQL Server/Express are NOT installed
   - Prevents false positives (LocalDB is often bundled with full SQL Server)

**Key Behaviors:**

- **First-time install with SQL Server/Express:** Database choice page is skipped, passes `--db-type=sqlserver`
- **First-time install with LocalDB only:** Database choice page is skipped, no parameter passed (LocalDB is default)
- **First-time install without any SQL Server:** Database choice page appears, user selects installation option
- **Upgrade install:** No detection, uses existing configuration

**Example Scenarios:**

| Installed SQL | Installer Behavior | First-Run Wizard Behavior | Parameter |
|---------------|-------------------|---------------------------|-----------|
| SQL Server 2022 | Skips database choice | Auto-selects "SQL Server" option | `--db-type=sqlserver` |
| SQL Server Express 2019 | Skips database choice | Auto-selects "SQL Server" option | `--db-type=sqlserver` |
| LocalDB only | Skips database choice | Auto-selects "LocalDB (Automatic)" option | None (default) |
| SQL Server + LocalDB | Skips database choice | Auto-selects "SQL Server" (LocalDB ignored) | `--db-type=sqlserver` |
| None | Shows database choice page | User selects from all options | Based on selection |

---

## Components Affected by LocalDB Migration

### Installation & Deployment
1. **Inno Setup Installer (`installer/MillionaireGameSetup.iss`)**
   - Add SQL Server Express LocalDB runtime (SqlLocalDB.msi) to installer
   - Silent install: `/quiet IACCEPTSQLLOCALDBLICENSETERMS=YES`
   - Size: ~40-50 MB additional
   - **TODO:** Download SqlLocalDB.msi from Microsoft
   - **TODO:** Add to installer script with prerequisite check

2. **README.md and Installation Documentation**
   - Update system requirements to mention LocalDB
   - Remove references to "SQL Server Express must be installed"
   - Update to: "Database included - no separate SQL installation needed"

3. **Wiki Documentation (`wiki/Installation.md`, `wiki/System-Requirements.md`)**
   - Update installation steps
   - Remove SQL Server Express prerequisites
   - Simplify database setup instructions

### Application Code Changes

4. **SqlConnectionSettings (`src/MillionaireGame.Core/Settings/SqlSettings.cs`)**
   - Update default settings to use LocalDB
   - Change `UseLocalDB = false` to `UseLocalDB = true` in defaults
   - Change `LocalInstance = "SQLEXPRESS"` to not used by default

5. **DatabaseSettingsDialog (`src/MillionaireGame/Forms/Options/DatabaseSettingsDialog.cs`)**
   - Update to match new wizard design (two options instead of three)
   - Add SSMS-style dropdown for SQL Server option
   - Update UI layout to match FirstRunWizard

6. **Program.cs (`src/MillionaireGame/Program.cs`)**
   - Add first-run wizard detection
   - Update database initialization logic

### Documentation Updates

7. **Release Notes**
   - Document LocalDB change in next release
   - Migration notes for existing users
   - Explain benefits of LocalDB

8. **User Guide (`wiki/User-Guide.md`)**
   - Update database configuration section
   - Simplify troubleshooting steps
   - Remove SQL Server service management instructions

### Testing Requirements

9. **Test on Clean Windows Installation**
   - Verify LocalDB installs correctly
   - Test first-run wizard with LocalDB
   - Confirm database creation works
   - Test with sample data loading

10. **Test SQL Server Option**
    - Verify instance detection works
    - Test "Browse for more..." functionality
    - Test remote SQL Server connections
    - Test both Windows Auth and SQL Auth

11. **Migration Testing**
    - Test existing users with SQL Express
    - Verify sql.xml compatibility
    - Confirm existing databases still work

### Future Removal (Post-LocalDB Migration)

12. **Uninstall SQL Server Express**
    - After confirming LocalDB works correctly
    - Document uninstall process for dev team
    - Clean up any SQL Server Express references

---

## Files to Create

### New Files
1. **`src/MillionaireGame/Forms/FirstRunWizard.cs`**
   - Main form logic
   - Connection testing
   - Database creation orchestration
   - Sample data loading

2. **`src/MillionaireGame/Forms/FirstRunWizard.Designer.cs`**
   - Form UI design
   - Control declarations
   - Event handlers
   - Layout specifications

### Modified Files
1. **`src/MillionaireGame/Program.cs`**
   - Add first-run detection (before SqlSettingsManager.LoadSettings)
   - Show wizard if sql.xml doesn't exist
   - Exit application if wizard cancelled

---

## Testing Checklist

**Testing Completed:** 2026-01-11 ✅ All tests passed

### First-Run Scenarios
- [x] Fresh install (no sql.xml)
- [x] Deleted sql.xml (simulated first-run)
- [x] sql.xml exists (wizard should NOT show)

### LocalDB Option
- [x] Test connection succeeds
- [x] Database created successfully
- [x] Sample data loads correctly
- [x] sql.xml saved with correct LocalDB settings

### SQL Server Express Option
- [x] Instance enumeration finds SQLEXPRESS
- [x] Instance enumeration finds custom instances
- [x] No instances found → appropriate warning
- [x] Selected instance connects successfully
- [x] Database created on selected instance

### Remote Server Option
- [x] Valid credentials → connection succeeds
- [x] Invalid credentials → shows error
- [x] Wrong server → shows error
- [x] Firewall blocked → shows error
- [x] All fields required validation

### Database Creation
- [x] Database doesn't exist → creates successfully
- [x] Database already exists → skips creation, shows message
- [x] Sample data checkbox enabled/disabled correctly
- [x] Sample data loads all 80 questions + 44 FFF questions

### Error Recovery
- [x] Test fails → can retry with different settings
- [x] Database creation fails → shows error, allows retry
- [x] Sample data fails → continues without sample data
- [x] Cancel confirmation works correctly

### Bug Fixes During Testing
**Issue:** Settings not being saved - wizard would create default settings regardless of user input
- **Root Cause:** SqlConnectionSettings property had private setter, preventing new settings from being assigned
- **Fix:** Changed property setter to public and ensured settings are saved to SettingsManager before serialization
- **Resolution:** Settings now properly persist to sql.xml with user-configured values
- **Status:** ✅ Fixed and verified

---

## Integration with Existing Code

### SqlConnectionSettings Changes
**Current fields used:**
- `UseRemoteServer` - true for Remote, false for Local/LocalDB
- `UseLocalDB` - true for LocalDB, false for SQL Express/Remote
- `LocalInstance` - stores selected instance name (e.g., "SQLEXPRESS")
- `RemoteServer` - remote server address
- `RemotePort` - remote server port
- `RemoteDatabase` - always "dbMillionaire"
- `RemoteLogin` - remote username
- `RemotePassword` - remote password

**No changes needed** - existing structure supports all wizard options.

### Program.cs Integration Point

**Current code (line ~59):**
```csharp
var sqlSettings = new SqlSettingsManager();
sqlSettings.LoadSettings();
```

**New code:**
```csharp
var sqlSettings = new SqlSettingsManager();

// Check for first-run (no sql.xml)
if (!File.Exists(Path.Combine(sqlSettings.GetSettingsPath(), "sql.xml")))
{
    using var wizard = new FirstRunWizard();
    if (wizard.ShowDialog() != DialogResult.OK)
    {
        GameConsole.Warn("[Startup] Database setup cancelled by user - exiting");
        return;
    }
}

sqlSettings.LoadSettings();
```

---

## User Experience Flow

### Happy Path (LocalDB)
1. User launches app for first time
2. Wizard appears: "Welcome to The Millionaire Game"
3. LocalDB radio is pre-selected (default)
4. "Finish" button is disabled, "Load sample trivia data" checkbox is disabled
5. User clicks "Test Connection" (required)
6. Status: "✓ Server connected! Database will be created on finish."
7. "Finish" button now enabled, "Load sample trivia data" checkbox now enabled
8. User checks "Load sample trivia data"
9. User clicks "Finish"
10. Status: "Creating database..." → "Loading sample data..." → "✓ Setup complete!"
11. Wizard closes, main app starts

### SQL Express Path
1. Wizard appears
2. User selects "SQL Server Express (Local)"
3. Dropdown shows "SQLEXPRESS" (or other detected instances)
4. "Finish" button is disabled
5. User clicks "Test Connection" (required)
6. Status: "✓ Server connected! Database will be created on finish."
7. "Finish" button now enabled
8. User clicks "Finish"
9. Database created, app starts

### Remote Server Path
1. Wizard appears
2. User selects "SQL Server (Remote)"
3. Remote GroupBox enables
4. "Finish" button is disabled
5. User enters server, port, username, password
6. User clicks "Test Connection" (required)
7. Status: "✓ Server connection successful!"
8. "Finish" button now enabled
9. User clicks "Finish"
10. Database created, app starts

### Error Path
1. User selects option ("Finish" button is disabled)
2. User clicks "Test Connection"
3. Status: "✗ Connection failed: Login failed for user..."
4. "Finish" button remains disabled
5. User corrects settings
6. User clicks "Test Connection" again
7. Status: "✓ Success!"
8. "Finish" button now enabled
9. User clicks "Finish"

### Cancel Path
1. User clicks "Cancel"
2. Dialog: "Are you sure you want to cancel? App cannot run without database."
3. User clicks "Yes"
4. Application exits

---

## Design Decisions

| Decision | Rationale |
|----------|-----------|
| Modal dialog | Database is required - block app start until configured |
| LocalDB as default | Perfect for single-user app, bundled with installer, zero config |
| Radio buttons (not dropdown) | Makes options clearer, easier to understand |
| Combined SQL Server option | Simplifies UI, one option for all SQL Server scenarios |
| SSMS-style server dropdown | Familiar to SQL users, intuitive "Browse" pattern |
| Instance auto-detection | Eliminates guesswork, improves UX |
| Test before Finish (mandatory) | Prevents bad configuration from being saved, ensures connection works |
| Finish disabled until test | Forces user to validate connection before proceeding |
| Optional sample data | Users may want empty database for custom questions |
| Cancel exits app | Database is mandatory for operation |
| No "Skip" option | Must configure database to use app |

---

## Future Enhancements (Post-v1.1)

- [ ] Add "Advanced" button for connection string customization
- [ ] Support for Azure SQL Database connection strings
- [ ] Import existing database option
- [ ] Multi-step wizard for complex scenarios
- [ ] Tooltips explaining each option
- [ ] Link to documentation/troubleshooting
- [ ] Remember last successful connection (for retry)
- [ ] "Use Existing Database" option (skip creation)

---

## Success Criteria

**All criteria met as of 2026-01-12:**

- ✅ No more confusing MessageBoxes on first run
- ✅ Users can set up database in under 2 minutes
- ✅ SQL Express instance detection works reliably
- ✅ LocalDB option works without any configuration
- ✅ Remote server option supports all SQL Server versions
- ✅ Sample data loads successfully
- ✅ sql.xml created correctly for all options
- ✅ Clear error messages guide users to solutions
- ✅ Application exits gracefully on cancel
- ✅ Settings persistence bug fixed and verified

---

## Related Documentation

- [DatabaseSettingsDialog](../MillionaireGame/Forms/Options/DatabaseSettingsDialog.cs) - Existing settings dialog
- [SqlSettings.cs](../MillionaireGame.Core/Settings/SqlSettings.cs) - Connection settings structure
- [GameDatabaseContext.cs](../MillionaireGame.Core/Database/GameDatabaseContext.cs) - Database creation logic
- [init_database.sql](../../publish/lib/sql/init_database.sql) - Sample data script

---

## Implementation Notes

### CRITICAL DEVELOPMENT STANDARDS
- **NO BLOCKING UI:** Use async/await for all database operations
- **ERROR HANDLING:** Comprehensive try-catch around all external interactions
- **LOGGING:** Use `GameConsole.Debug/Info/Warn/Error` for logging (NOT Console.WriteLine)
- **CODE COMMENTS:** Maintain clear comments for complex logic

### Async Patterns
```csharp
private async void btnTest_Click(object sender, EventArgs e)
{
    btnTest.Enabled = false;
    try
    {
        await TestConnectionAsync();
    }
    finally
    {
        btnTest.Enabled = true;
    }
}
```

### Status Updates
```csharp
lblStatus.Text = "Testing...";
lblStatus.ForeColor = Color.Blue;
Application.DoEvents(); // Force UI update
await SomeLongOperation();
lblStatus.Text = "✓ Success!";
lblStatus.ForeColor = Color.Green;
```

---

## Open Questions

1. **Should we support MySQL/PostgreSQL in the future?**
   - Not in v1.1, but architecture should allow for extensibility

2. **What if user has both LocalDB and SQL Express?**
   - User can choose either option - both are valid

3. **Should we validate database name format?**
   - Currently hardcoded to "dbMillionaire" - validation not needed

4. **What about connection encryption/SSL settings?**
   - Currently using `TrustServerCertificate=True` - should be configurable in Advanced mode (future)

5. **Should wizard be resizable?**
   - No - fixed size for consistent layout

---

## Implementation Priority

**Status: ✅ COMPLETE - All phases finished**

**Phase 1 - Core Functionality:** ✅ Complete
1. ✅ Create FirstRunWizard form with basic layout
2. ✅ Add radio button options (LocalDB, SQL Express, Remote)
3. ✅ Implement SQL Server instance enumeration
4. ✅ Add connection testing logic
5. ✅ Implement database creation flow

**Phase 2 - Integration:** ✅ Complete
6. ✅ Modify Program.cs to show wizard on first-run
7. ✅ Test all connection scenarios
8. ✅ Verify sql.xml creation
9. ✅ Fix settings persistence bug

**Phase 3 - Polish:** ✅ Complete
10. ✅ Add sample data loading
11. ✅ Improve error messages
12. ✅ Add status animations/colors
13. ✅ Testing and bug fixes

**Phase 4 - Installer Integration:** ✅ Complete
14. ✅ Implement SQL Server detection in installer
15. ✅ Add command-line parameter support
16. ✅ Test auto-detection scenarios

---

**Final Status:** ✅ Ready for production deployment in v1.0.6
