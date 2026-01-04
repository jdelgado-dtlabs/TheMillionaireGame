# System Requirements

Before installing The Millionaire Game, ensure your system meets the following requirements.

---

## Minimum Requirements

### Operating System
- **Windows 10** (64-bit) - Version 1809 or later
- **Windows 11** (64-bit) - All versions

> ⚠️ **Note**: This application is Windows-only. macOS and Linux are not supported.

### Runtime
- **.NET 8 Desktop Runtime** (x64)
  - Required for application execution
  - Approximately 50 MB download
  - **Download**: [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)
  - Select "Desktop Runtime" for Windows x64

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
- **Primary Display (TV Screen)**: 
  - 1920x1080 (Full HD) - **Recommended**
  - 1280x720 (HD) - Minimum
  - Higher resolutions supported (4K, etc.)
- **Secondary Display (Control Panel)**: 
  - Optional but highly recommended
  - 1366x768 or higher
  - Can share primary display if needed

---

## Recommended Configuration

### For Best Experience
- **OS**: Windows 11
- **Processor**: Quad-core CPU (3 GHz+)
- **RAM**: 8 GB or more
- **Display**: Dual monitors (1920x1080 each)
- **Network**: For audience participation features
- **Audio**: External speakers or sound system

---

## Optional Components

### Database Server
By default, the application uses **SQL Server LocalDB** (embedded), which is included with .NET installation.

For advanced users or multi-machine setups:
- **SQL Server Express** (free)
- **SQL Server** (full version)

Benefits:
- Centralized question database
- Multi-user access to questions
- Remote database hosting
- Advanced backup options

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

### Single Monitor Setup
- Works, but less convenient for operators
- Control panel and TV screen share same display
- Recommended for testing or casual use

### Dual Monitor Setup
- Optimal configuration
- Control panel on secondary monitor
- TV screen on primary (1920x1080)
- Full separation of operator and player views

### Triple Monitor Setup
- Advanced configuration
- Control panel + TV Screen + Host/Guest screen
- Requires manual window positioning

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

**Questions?** Check the [Troubleshooting](Troubleshooting) page or [report an issue](https://github.com/Macronair/TheMillionaireGame/issues).
