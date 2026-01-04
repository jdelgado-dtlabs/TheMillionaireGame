# System Requirements

Before installing The Millionaire Game, ensure your system meets the following requirements.

---

## Minimum Requirements

### Operating System
- **Windows 10** (64-bit) - Version 1809 or later
- **Windows 11** (64-bit) - All versions

> ⚠️ **Note**: This application is Windows-only. macOS and Linux are not supported.

### Runtime
- **.NET 8 Desktop Runtime** (x64) - **REQUIRED**
  - Essential for application execution
  - Approximately 50 MB download
  - **Download**: [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)
  - Select "Desktop Runtime" for Windows x64

### Database
- **SQL Server Express** - **REQUIRED**
  - Free edition from Microsoft
  - Approximately 200 MB download
  - **Download**: [SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads)
  - Or use full SQL Server if available
  
> 💡 **Note**: SQL Server LocalDB (included with .NET SDK) is NOT sufficient for runtime. SQL Server Express or full SQL Server is required.

### Hardware
- **Processor**: Dual-core CPU (2 GHz or faster)
- **RAM**: 4 GB minimum, 8 GB recommended
- **Storage**: 
  - 300 MB for application files
  - Additional 500 MB for question databases (optional)
- **Graphics**: 
  - DirectX 9 compatible graphics card
  - Support for 1920x1080 resolution

### Display
- **Control Panel** (Main Interface) - **REQUIRED**
  - Minimum: 1280x720 (HD)
  - Recommended: 1366x768 or higher
  - This is your primary operator interface
  
- **TV Screen, Host Screen, Guest Screen** (Optional)
  - Can be opened on separate monitors or same display
  - Recommended: 1920x1080 (Full HD) per screen
  - Minimum: 1280x720 (HD)
  - Higher resolutions supported (4K, etc.)
  
- **Preview Window** (Built-in)
  - Integrated preview of TV/Host/Guest screens
  - Available within Control Panel for supervision
  - Useful for single-monitor setups

---

## Recommended Configuration

### For Best Experience
- **OS**: Windows 11 (64-bit)
- **Processor**: Quad-core CPU (3 GHz+)
- **RAM**: 8 GB or more
- **Display**: Dual monitors (1920x1080 each)
- **Network**: Ethernet or WiFi for audience participation
- **Audio**: External speakers or sound system
- **Database**: SQL Server Express or full SQL Server

---

## Database Requirements

The application **requires** SQL Server Express (minimum) or SQL Server (full version).

### SQL Server Express (Minimum Required)
- **Download**: [SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads)
- Free edition from Microsoft
- Sufficient for single-machine setups
- 10 GB database size limit (more than enough)

### SQL Server (Recommended for Advanced Use)
- Full-featured version
- Required for multi-machine setups
- Centralized question database
- Multi-user access
- Remote database hosting
- Advanced backup and recovery

> ⚠️ **Important**: SQL Server LocalDB is NOT sufficient. You must install SQL Server Express or full SQL Server.

### Network Requirements
**For Web Audience Participation:**
- Local network (LAN) or internet connection
- HTTP/HTTPS ports accessible (default: 5000, 5001)
- Firewall configured to allow incoming connections
- Router port forwarding (for internet access)

---

## Compatibility Notes

### Supported Windows Versions
| Version | Support Status | Notes |
|---------|---------------|-------|
| Windows 11 | ✅ Full Support | Recommended |
| Windows 10 (1809+) | ✅ Full Support | Requires updates |
| Windows 10 (older) | ⚠️ Limited | Update recommended |
| Windows 8.1 | ❌ Not Supported | EOL by Microsoft |
| Windows 7 | ❌ Not Supported | EOL by Microsoft |

### .NET Runtime
- **Required**: .NET 8 Desktop Runtime (x64)
- **NOT Compatible**: .NET Framework 4.x (legacy)
- **NOT Compatible**: .NET Core 3.1 or earlier

### Display Scaling
The application supports Windows display scaling:
- 100% (native) - Best performance
- 125% - Fully supported
- 150% - Fully supported
- 200% - Supported with minor UI adjustments

---

## Performance Considerations

### Single Monitor Setup (Basic)
- Control Panel only (required)
- Use built-in preview window to monitor TV/Host/Guest screens
- Ideal for testing, development, or solo operation
- Fully functional for all game features

### Dual Monitor Setup (Recommended)
- Control Panel on one monitor (operator view)
- TV Screen on second monitor (audience/contestant view)
- Best for live events and presentations
- Clear separation of operator and player views

### Multi-Monitor Setup (Advanced)
- Control Panel on primary monitor
- TV Screen, Host Screen, and/or Guest Screen on additional monitors
- Maximum flexibility for large productions
- Each screen independently positioned and sized

---

## Known Limitations

### Platform
- **Windows-only**: No plans for macOS or Linux versions
- Requires x64 (64-bit) system architecture
- ARM64 support untested

### Single Instance
- Only one instance can run at a time
- Application enforces single-instance policy
- Prevents database conflicts

### Network Features
- Web server requires network access
- May need firewall/antivirus configuration
- Port conflicts possible with other applications

---

## Checking Your System

### Verify Windows Version
1. Press `Win + R`
2. Type `winver` and press Enter
3. Check version number (should be 1809+)

### Verify .NET Installation
1. Open PowerShell
2. Run: `dotnet --list-runtimes`
3. Look for `Microsoft.WindowsDesktop.App 8.0.x`

### Verify SQL Server Installation
1. Open PowerShell
2. Run: `sqlcmd -S localhost -Q "SELECT @@VERSION"`
3. Should display SQL Server version information
4. If command not found, SQL Server is not installed

### Check Display Resolution
1. Right-click desktop → Display Settings
2. Verify resolution under "Display resolution"
3. Note if multiple monitors detected

---

## Upgrading Your System

### If .NET 8 Not Installed
1. Visit [.NET 8 Download Page](https://dotnet.microsoft.com/download/dotnet/8.0)
2. Download "Desktop Runtime x64" installer
3. Run installer (requires administrator)
4. Restart computer (recommended)

### If SQL Server Not Installed
1. Visit [SQL Server Express Download](https://www.microsoft.com/sql-server/sql-server-downloads)
2. Download "Express" edition
3. Run installer (requires administrator)
4. Choose "Basic" installation for simplest setup
5. Restart computer after installation

### If Windows Too Old
1. Check for Windows Updates
2. Install latest feature updates
3. Or upgrade to Windows 11 (free for eligible systems)

---

## Next Steps

Once your system meets the requirements:
1. Proceed to **[Installation](Installation)** guide
2. Download the latest release
3. Start playing!

---

**Questions?** Check the [Troubleshooting](Troubleshooting) page or [report an issue](https://github.com/jdelgado-dtlabs/TheMillionaireGame/issues).
