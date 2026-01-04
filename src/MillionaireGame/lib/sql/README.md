# Database Initialization Script

## File: `init_database.sql`

This SQL script creates and populates the questions and FFF questions tables for The Millionaire Game v1.0.0.

## What's Included

- **80 Main Questions** (Levels 1-15)
- **41 FFF Questions** (Fastest Finger First)
- Complete table schemas with proper constraints

## Usage

### Prerequisites
- SQL Server Express 2019+ or SQL Server 2022
- Database `dbMillionaire` must exist

### Run the Script

**SQL Server Management Studio (SSMS)**:
1. Open SSMS and connect to your server
2. Open `init_database.sql`
3. Execute (F5)

**Command Line (sqlcmd)**:
```bash
sqlcmd -S localhost\SQLEXPRESS -d dbMillionaire -E -i init_database.sql
```

**PowerShell**:
```powershell
Invoke-Sqlcmd -ServerInstance "localhost\SQLEXPRESS" -Database "dbMillionaire" -InputFile "init_database.sql"
```

## Important Notes

- ⚠️ **This will DELETE existing questions** if tables already exist
- ✅ Other tables are created automatically by the application
- 📝 Includes all original questions from the VB.NET version

## Verification

After running, verify the data:

```sql
SELECT COUNT(*) FROM questions;        -- Should return 80
SELECT COUNT(*) FROM fff_questions;    -- Should return 41
```

---

**The Millionaire Game v1.0.0**  
Database initialization script
