# mDNS Service Discovery - Implementation Plan

**Status:** 📋 Planning Phase  
**Branch:** `feature/web-state-sync` (add to existing branch)  
**Date:** January 8, 2026  
**Complexity:** Medium  
**Estimated Time:** 2-3 hours

## Executive Summary

Implement mDNS (Multicast DNS) service discovery to allow the web server to be accessed via `wwtbam.local` instead of requiring users to know the IP address. This enables captive portal redirects and simplifies device connection in hotspot scenarios.

---

## Use Case

**Problem:** Users connecting to a WiFi hotspot need to know the server's IP address (e.g., `http://192.168.137.1:5278`) to access the web application.

**Solution:** With mDNS, devices can access the server using a friendly domain name:
- `http://wwtbam.local:5278` - main access
- Captive portal redirects work seamlessly
- Zero-configuration networking (Bonjour/Zeroconf)

**User Experience:**
1. Host starts game and web server (default port 5278, configurable)
2. mDNS automatically advertises `wwtbam.local` with current port
3. Participants connect to WiFi hotspot
4. Access game via:
   - `http://wwtbam.local` (if port 80)
   - `http://wwtbam.local:5278` (if default port)
   - `http://wwtbam.local:[custom_port]` (if custom port)
5. No IP address lookup needed

**Port Considerations:**
- **Port 80:** Cleanest URL (`http://wwtbam.local`), requires admin rights or proxy
- **Port 5278 (default):** Works without admin, requires port in URL
- **Custom Port:** Full flexibility, requires port in URL

---

## Technical Architecture

### mDNS Overview

**Multicast DNS (mDNS):**
- RFC 6762 standard
- Uses multicast group `224.0.0.251` (IPv4) or `FF02::FB` (IPv6)
- Port `5353` (UDP)
- `.local` TLD (top-level domain)
- Works on local network only

**Service Advertisement:**
- DNS-SD (DNS Service Discovery) - RFC 6763
- Advertises HTTP service with port and metadata
- Responds to queries for `wwtbam.local`
- Compatible with Windows, macOS, Linux, mobile devices

### Implementation Components

```
WebServerHost.cs
    └── mDNS Service Manager
        ├── Makaretu.Dns.Multicast (NuGet package)
        ├── ServiceProfile (HTTP service definition)
        ├── MulticastService (advertiser)
        └── Lifecycle management (start/stop with web server)
```

---

## Library Selection

### Recommended: Makaretu.Dns.Multicast

**Pros:**
- ✅ Pure .NET implementation (no native dependencies)
- ✅ RFC 6762/6763 compliant
- ✅ Cross-platform (Windows, Linux, macOS)
- ✅ Actively maintained
- ✅ Simple API
- ✅ NuGet package available: `Makaretu.Dns.Multicast`

**Cons:**
- Requires firewall rules for UDP 5353 (handled automatically on most systems)
- Windows Firewall may prompt on first run

### Alternative: Zeroconf

**Pros:**
- ✅ Simpler high-level API
- ✅ Cross-platform

**Cons:**
- ❌ Less flexible for custom service types
- ❌ Heavier dependency chain

**Decision:** Use **Makaretu.Dns.Multicast** for better control and RFC compliance.

---

## Implementation Plan

### Phase 1: Add mDNS Package (15 minutes)

#### Step 1.1: Add NuGet Package

**File:** `src/MillionaireGame.Web/MillionaireGame.Web.csproj`

```xml
<ItemGroup>
  <!-- Existing packages -->
  <PackageReference Include="Makaretu.Dns.Multicast" Version="0.28.0" />
</ItemGroup>
```

### Phase 2: Implement mDNS Service (1-1.5 hours)

#### Step 2.1: Create mDNS Service Manager

**File:** `src/MillionaireGame.Web/Services/MDnsServiceManager.cs` (NEW)

```csharp
using Makaretu.Dns;
using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace MillionaireGame.Web.Services
{
    /// <summary>
    /// Manages mDNS service advertisement for the web server
    /// </summary>
    public class MDnsServiceManager : IDisposable
    {
        private readonly ServiceDiscovery _serviceDiscovery;
        private readonly ServiceProfile _serviceProfile;
        private bool _isAdvertising;

        public string ServiceName { get; }
        public int Port { get; }

        public MDnsServiceManager(string serviceName, int port)
        {
            ServiceName = serviceName ?? "wwtbam";
            Port = port; // Port from WebServerHost settings (default 5278)

            // Create service discovery instance
            _serviceDiscovery = new ServiceDiscovery();

            // Create service profile
            _serviceProfile = new ServiceProfile(
                instanceName: ServiceName,
                serviceName: "_http._tcp",
                port: (ushort)port,
                addresses: GetLocalIPAddresses()
            );

            // Add TXT records for additional metadata
            _serviceProfile.Resources.Add(new TXTRecord
            {
                Name = _serviceProfile.FullyQualifiedName,
                Strings = new[]
                {
                    "path=/",
                    "game=millionaire",
                    "version=1.0.5"
                }
            });
        }

        /// <summary>
        /// Start advertising the service on the network
        /// </summary>
        public void StartAdvertising()
        {
            if (_isAdvertising)
                return;

            try
            {
                _serviceDiscovery.Advertise(_serviceProfile);
                _isAdvertising = true;
                
                Console.WriteLine($"[mDNS] Advertising service: {ServiceName}.local:{Port}");
                
                // Show appropriate URL based on port
                string accessUrl = Port == 80 
                    ? $"http://{ServiceName}.local" 
                    : $"http://{ServiceName}.local:{Port}";
                Console.WriteLine($"[mDNS] Service can be accessed at: {accessUrl}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[mDNS] Failed to start advertising: {ex.Message}");
            }
        }

        /// <summary>
        /// Stop advertising the service
        /// </summary>
        public void StopAdvertising()
        {
            if (!_isAdvertising)
                return;

            try
            {
                _serviceDiscovery.Unadvertise(_serviceProfile);
                _isAdvertising = false;
                Console.WriteLine($"[mDNS] Stopped advertising service: {ServiceName}.local");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[mDNS] Error stopping advertisement: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all local IP addresses for the machine
        /// </summary>
        private IPAddress[] GetLocalIPAddresses()
        {
            var addresses = new List<IPAddress>();

            try
            {
                var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(ni => ni.OperationalStatus == OperationalStatus.Up)
                    .Where(ni => ni.NetworkInterfaceType != NetworkInterfaceType.Loopback);

                foreach (var networkInterface in interfaces)
                {
                    var ipProperties = networkInterface.GetIPProperties();
                    foreach (var ip in ipProperties.UnicastAddresses)
                    {
                        // Include IPv4 and IPv6 addresses
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ||
                            ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                        {
                            addresses.Add(ip.Address);
                        }
                    }
                }

                if (addresses.Count == 0)
                {
                    // Fallback: add localhost
                    addresses.Add(IPAddress.Loopback);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[mDNS] Error getting network interfaces: {ex.Message}");
                addresses.Add(IPAddress.Loopback);
            }

            return addresses.ToArray();
        }

        public void Dispose()
        {
            StopAdvertising();
            _serviceDiscovery?.Dispose();
        }
    }
}
```

#### Step 2.2: Integrate into WebServerHost

**File:** `src/MillionaireGame/Hosting/WebServerHost.cs`

**Add field:**
```csharp
private MDnsServiceManager? _mdnsService;
```

**In StartAsync() method (after web server starts):**
```csharp
// S
    // Show appropriate URL based on port
    string accessUrl = _port == 80 
        ? "http://wwtbam.local" 
        : $"http://wwtbam.local:{_port}";
    WebServerConsole.Info($"[WebServer] mDNS service advertising at: {accessUrl}");
    
    // Warn if not using port 80
    if (_port != 80)
    {
        WebServerConsole.Info($"[WebServer] Note: Port {_port} requires including port in URL. Set port to 80 for cleanest URLs.");
    }
try
{
    _mdnsService = new MDnsServiceManager("wwtbam", _port);
    _mdnsService.StartAdvertising();
    WebServerConsole.Info($"[WebServer] mDNS service advertising at: http://wwtbam.local:{_port}");
}
catch (Exception ex)
{
    WebServerConsole.Warn($"[WebServer] Failed to start mDNS service: {ex.Message}");
    // Continue without mDNS - not critical
}
```

**In StopAsync() method:**
```csharp
// Stop mDNS advertisement
_mdnsService?.StopAdvertising();
_mdnsService?.Dispose();
_mdnsService = null;
```

### Phase 3: Configuration (30 minutes)

#### Step 3.1: Add Settings

**Option A: Hardcode (simplest)**
- Service name: `wwtbam`
- Domain: `wwtbam.local`

**Option B: Configurable (future enhancement)**
Add to ApplicationSettings:
```csharp
public string MDnsServiceName { get; set; } = "wwtbam";
public bool EnableMDns { get; set; } = true;
```

**Recommendation:** Start with Option A, add Option B later if needed.

### Phase 4: Testing (30-45 minutes)
:
     - `http://wwtbam.local` (if port 80)
     - `http://wwtbam.local:5278` (if default port)
     - `http://wwtbam.local:[configured_port]` (if custom)
#### Test Scenarios

1. **Service Advertisement**
   - Start web server
   - Verify mDNS logs show "Advertising service"
   - Check no errors in console

2. **DNS Resolution**
   - From another device on same network
   - Test: `ping wwtbam.local` (should resolve to server IP)
   - Test: Open browser to `http://wwtbam.local:5000`
[configured_port]` from device
   - Verify state sync works
   - Test with port 80 for cleanest experience
   - Test with port 5278 (default) to verify port handling
   - Use discovery tools:
     - **Windows:** `dns-sd -B _http._tcp`
     - **macOS/Linux:** `avahi-browse -a` or `dns-sd -B _http._tcp`
   - Should see "wwtbam" service listed

4. **Multiple Interfaces**
   - Test with WiFi + Ethernet enabled
   - Verify service advertises on all active interfaces

5. **Hotspot Scenario**
   - Enable WiFi hotspot on host machine
   - Connect device to hotspot
   - Access `http://wwtbam.local:5000` from device
   - Verify state sync works

6. **Server Restart**
   - Stop and restart web server
   - Verify service re-advertises correctly
   - Check no resource leaks

#### Test Checklist

- [ ] Service starts without errors
- [ ] `wwtbam.local` resolves to correct IP
- [ ] Web interface accessible via domain name
- [ ] Service stops cleanly on server shutdown
- [ ] Works across multiple network interfaces
- [ ] Mobile devices can discover and connect
- [ ] No firewall conflicts (may prompt on first run)

---

## Firewall Considerations

### Windows Firewall

**Automatic Handling:**
- Windows Firewall may prompt on first run
- User should click "Allow access" for Private networks
- UDP port 5353 (mDNS) needs to be open

**Manual Configuration (if needed):**
```powershell
# Allow mDNS inbound
New-NetFirewallRule -DisplayName "mDNS (UDP-In)" -Direction Inbound -Protocol UDP -LocalPort 5353 -Action Allow

# Allow mDNS outbound
New-NetFirewallRule -DisplayName "mDNS (UDP-Out)" -Direction Outbound -Protocol UDP -LocalPort 5353 -Action Allow
```

### Corporate Networks

**Limitations:**
- Some corporate networks block multicast traffic
- mDNS may not work across VLANs
- Works best on flat home/small office networks

**Fallback:**
- Application still works via IP address
- mDNS is optional enhancement

---

## Alternative: Captive Portal Configuration

### For Hotspot Scenarios
:
     - `http://wwtbam.local` (if port 80 - best experience)
     - `http://wwtbam.local:5278` (if default port)
   - mDNS resolves domain to host IP

2. **Static Redirect (manual alternative):**
   - Configure hotspot gateway redirect
   - Point to `http://192.168.137.1:[port]` (typical hotspot IP)

**Port 80 Benefits for Captive Portals:**
- Cleanest URL (no port number needed)
- Better mobile browser compatibility
- More professional appearance
- Standard HTTP port

**Setting Port 80:**
- Requires administrator rights or reverse proxy
- Set in Application Settings → Web Server Port
- Alternative: Use nginx/Apache as reverse proxy on port 80 → forward to 5278

**Recommendation:** Use port 80 for captive portal scenarios, or document full URL with port number
2. **Static Redirect (manual alternative):**
   - Configure hotspot gateway redirect
   - Point to `http://192.168.137.1:5000` (typical hotspot IP)

**Recommendation:** Use mDNS for flexibility - works regardless of hotspot IP assignment.

---

## Code Organization

### New Files

```
src/MillionaireGame.Web/
    └── Services/
        └── MDnsServiceManager.cs (NEW - 150 lines)
```

### Modified Files

```
src/MillionaireGame.Web/
    └── MillionaireGame.Web.csproj (add NuGet package)

src/MillionaireGame/
    └── Hosting/
        └── WebServerHost.cs (integrate mDNS lifecycle)
```

---

## Dependencies

### NuGet Package

**Package:** `Makaretu.Dns.Multicast`  
**Version:** 0.28.0 (latest stable)  
**License:** MIT  278`"

**After (Port 80):**
> "Connect to the host's WiFi and navigate to `http://wwtbam.local`"

**After (Default Port 5278):**
> "Connect to the host's WiFi and navigate to `http://wwtbam.local:5278`"

**After (Custom Port):**
> "Connect to the host's WiFi and navigate to `http://wwtbam.local:[port]`"

**Recommendation:** Set port to 80 in settings for the cleanest user experience.
### System Requirements

- **.NET 8.0:** Already required
- **Network Access:** UDP multicast must be enabled[port]`
- Some corporate networks block mDNS

**Q: Do I need to include the port number?**
- **Port 80:** No - use `http://wwtbam.local`
- **Other ports:** Yes - use `http://wwtbam.local:[port]`
- Check Control Panel or Settings for configured port
- Default port is 5278ssion on first run
- **Platform:** Windows, macOS, Linux (cross-platform)

---

## User Documentation Updates

### Quick Start Guide

**Before:**
> "Connect to the host's WiFi and navigate to `http://[HOST_IP]:5000`"

**After:**
> "Connect to the host's WiFi and navigate to `http://wwtbam.local:5000`"

### Troubleshooting Section

**Add to docs:**

**Q: "wwtbam.local" not found?**
- Ensure devices are on same network
- Check Windows Firewall allowed the application
- Try IP address as fallback: `http://192.168.137.1:5000`
- Some corporate networks block mDNS

**Q: Firewall prompt appears?**
- Click "Allow access" for Private networks
- This enables mDNS discovery on local network

---

## Success Criteria

✅ **Phase 1 Complete:**
- Makaretu.Dns.Multicast package installed
- Build succeeds with no errors

✅ **Phase 2 Complete:**
- MDnsServiceManager cwith correct port (`http://wwtbam.local` or `http://wwtbam.local:[port]`)
   - Display in control panel for easy mobile access
   - Update QR code if port change
- Logs show "Advertising service" on startup

✅ **Phase 3 Complete:**
- Service name hardcoded as "wwtbam"
- Domain accessible at `wwtbam.local`

✅ **System Ready:**
- Mobile device can access `http://wwtbam.local:5000`
- Service discovery tools show "wwtbam" service
- State sync works through domain name
- Server stops cleanly without resource leaks

---

## Future Enhancements

### Phase 2 Features (Post-MVP)

1. **Configurable Service Name**
   - Allow user to customize domain name
   - Setting in ApplicationSettings

2. **Service Browser UI**
   - Show available mDNS services in control panel
   - Help users find server on network

3. **QR Code Generation**
   - Generate QR code for `http://wwtbam.local:5000`
   - Display in control panel for easy mobile access

4. **Network Interface Selection**
   - Allow choosing which interfaces to advertise on
   - Useful for multi-homed systems

5. **IPv6 Support**
   - Ensure works with IPv6-only networks
   - Dual-stack advertisement

---

## Risk Mitigation

### Risk 1: Firewall Blocks mDNS
**Impact:** Service not discoverable  
**Mitigation:** 
- Clear user prompt to allow firewall access
- Fallback to IP address always available
- Document manual firewall configuration

### Risk 2: Name Collision
**Impact:** Another device using "wwtbam.local"  
**Mitigation:**
- mDNS protocol handles collisions automatically
- Will append number: "wwtbam-2.local"
- Log collision events

### Risk 3: Corporate Network Restrictions
**Impact:** mDNS blocked by network policy  
**Mitigation:**
- Document limitation in user guide
- IP address access always works
- Not a critical feature

### Risk 4: Resource Leaks
**Impact:** Memory/socket leaks on repeated start/stop  
**Mitigation:**
- Proper IDisposable implementation
- Thorough testing of lifecycle
- Monitor with repeated start/stop cycles

---Port Configuration Notes

### Default Port: 5278

The web server defaults to port **5278** but is configurable in Application Settings.

**Port Detection:**
- MDnsServiceManager receives port from WebServerHost
- Port comes from `_port` field in WebServerHost (set during initialization)
- WebServerHost reads port from ApplicationSettings

**URL Format:**
- Port 80: `http://wwtbam.local` (standard HTTP)
- Port 443: `https://wwtbam.local` (standard HTTPS - if implemented)
- Other ports: `http://wwtbam.local:[port]`

**Best Practices:**
- **Captive Portal Use:** Set port to 80 for cleanest experience
- **Home Network:** Default 5278 is fine, users can handle port in URL
- **Mobile Hotspot:** Port 80 recommended for ease of use
- **Development:** Use default 5278 to avoid admin rights requirement

### Reverse Proxy Alternative

If port 80 is needed but admin rights unavailable:

**nginx Configuration:**
```nginx
server {
    listen 80;
    server_name wwtbam.local;
    
    location / {
        proxy_pass http://localhost:5278;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
    }
}
```

## Questions to Address Before Implementation

1. ✅ Should service name be configurable? → **No, hardcode "wwtbam" for MVP**
2. ✅ What if mDNS fails to start? → **Log warning, continue without it (non-critical)**
3. ✅ Support IPv6? → **Yes, library handles automatically**
4. ✅ Which network interfaces? → **All active interfaces (automatic)**
5. ✅ Firewall configuration? → **Document manual steps, rely on Windows prompt**
6. ✅ Port handling? → **Get from WebServerHost._port, display appropriate URL format**
7. ✅ Recommend port 80? → **Yes for captive portals, document reverse proxy alternative
|-------|------|------|----------|
| 1 | Add NuGet package | 15min | HIGH |
| 2 | Implement MDnsServiceManager | 45min | HIGH |
| 3 | Integrate into WebServerHost | 30min | HIGH |
| 4 | Testing and validation | 45min | HIGH |
| 5 | Documentation updates | 15min | MEDIUM |

**Total: 2.5 hours**

---

## Notes for Implementation

1. **Keep It Simple:** Hardcode "wwtbam" for now, make configurable later
2. **Non-Critical Feature:** Don't block server start if mDNS fails
3. **Log Everything:** Use WebServerConsole for visibility
4. **Test on Mobile:** Primary use case is phone/tablet access
5. **Firewall Prompt:** Warn user in documentation

---

## Questions to Address Before Implementation

1. ✅ Should service name be configurable? → **No, hardcode "wwtbam" for MVP**
2. ✅ What if mDNS fails to start? → **Log warning, continue without it (non-critical)**
3. ✅ Support IPv6? → **Yes, library handles automatically**
4. ✅ Which network interfaces? → **All active interfaces (automatic)**
5. ✅ Firewall configuration? → **Document manual steps, rely on Windows prompt**

---

**Status:** Ready for implementation  
**Next Step:** Add Makaretu.Dns.Multicast package and begin Phase 1
