# UPnP/SSDP Service Discovery Implementation Plan
**Status:** 📋 Planning  
**Branch:** feature/upnp-ssdp-discovery  
**Created:** January 9, 2026

## Overview
Add UPnP (Universal Plug and Play) / SSDP (Simple Service Discovery Protocol) alongside existing mDNS to provide better Windows network discovery support for the Millionaire Game web server.

## Problem Statement
- **mDNS limitations:** Windows doesn't resolve its own .local domains (though other devices work fine)
- **Current workaround:** Users must manually type IP address (or scan QR code)
- **Goal:** Provide automatic discovery that works well on Windows networks

## Why UPnP/SSDP?

### Advantages
✅ **Native Windows Support** - Built into Windows since Vista  
✅ **Network Discovery Integration** - Shows up in Windows network browser  
✅ **No Client Configuration** - Works out of the box on Windows devices  
✅ **Complements mDNS** - mDNS for Apple/Android, UPnP for Windows  
✅ **Standard Protocol** - Used by printers, smart TVs, media servers (RFC 6763)  
✅ **Firewall Friendly** - Uses standard ports (1900/UDP)

### Limitations
⚠️ **Network Segment** - Only works on local subnet (like mDNS)  
⚠️ **Some Enterprise Networks** - May block multicast  
⚠️ **Not a Replacement** - Still need IP fallback option

## Technical Specifications

### Protocol Details
- **SSDP:** Simple Service Discovery Protocol (UDP port 1900)
- **Multicast Address:** 239.255.255.250:1900 (IPv4) or FF02::C (IPv6)
- **HTTP Subset:** Uses HTTP-like messages over UDP
- **Device Description:** XML file describing the service

### Message Types
1. **NOTIFY (Alive)** - Server announces availability
2. **NOTIFY (Byebye)** - Server announces shutdown
3. **M-SEARCH** - Client searches for services
4. **M-SEARCH Response** - Server responds to searches

### Service Type
```
urn:schemas-upnp-org:service:MillionaireGameShow:1
```

## Architecture

### Component Structure
```
MillionaireGame.Web/
├── Services/
│   ├── MDnsServiceManager.cs          (existing - keep)
│   ├── UPnPDiscoveryService.cs        (NEW)
│   └── NetworkDiscoveryManager.cs     (NEW - orchestrates both)
└── Models/
    └── DeviceDescription.cs           (NEW - UPnP device XML)

MillionaireGame/
└── Hosting/
    └── WebServerHost.cs               (modify - integrate UPnP)
```

### Service Lifecycle
```
Application Start
    ↓
NetworkDiscoveryManager.Initialize()
    ├── Start mDNS (existing)
    └── Start UPnP/SSDP (new)
        ├── Create device description XML
        ├── Start SSDP listener (port 1900)
        ├── Send NOTIFY alive messages
        └── Listen for M-SEARCH requests
    ↓
[Running - respond to discovery requests]
    ↓
Application Stop
    ↓
NetworkDiscoveryManager.Shutdown()
    ├── Stop mDNS (existing)
    └── Stop UPnP/SSDP (new)
        ├── Send NOTIFY byebye messages
        └── Stop SSDP listener
```

## Implementation Tasks

### Phase 1: Core UPnP/SSDP Service ✅ Estimated: 4-6 hours

**Task 1.1: Create UPnP Device Description Model**
- File: `MillionaireGame.Web/Models/DeviceDescription.cs`
- Purpose: Generate XML device description per UPnP specs
- Required fields:
  - Device type: `urn:schemas-upnp-org:device:MillionaireGameShow:1`
  - Friendly name: "Millionaire Game Audience Participation"
  - Manufacturer: "Millionaire Game Project"
  - Model name/number
  - Serial number (use machine ID)
  - UDN (Unique Device Name): Generate from MAC address
  - Presentation URL: `http://{ip}:{port}/`
- Output: Well-formed XML string

**Task 1.2: Create SSDP Discovery Service**
- File: `MillionaireGame.Web/Services/UPnPDiscoveryService.cs`
- Responsibilities:
  - Send NOTIFY alive messages on startup (3 times, 100ms apart for reliability)
  - Listen for M-SEARCH requests on UDP 1900
  - Respond to M-SEARCH with device location
  - Send NOTIFY byebye on shutdown
  - Host device description XML at `/upnp/description.xml`
- Dependencies: 
  - UdpClient for multicast
  - HttpListener subset or integrate with existing Kestrel
- Error handling:
  - Graceful failure if port 1900 already in use
  - Log warnings but don't crash web server
  - Timeout on multicast send (don't block)

**Task 1.3: SSDP Message Formatting**
- Implement NOTIFY message:
  ```
  NOTIFY * HTTP/1.1
  HOST: 239.255.255.250:1900
  CACHE-CONTROL: max-age=1800
  LOCATION: http://192.168.x.x:port/upnp/description.xml
  NT: urn:schemas-upnp-org:device:MillionaireGameShow:1
  NTS: ssdp:alive
  SERVER: Windows/10 UPnP/2.0 MillionaireGame/1.0
  USN: uuid:{device-uuid}::urn:schemas-upnp-org:device:MillionaireGameShow:1
  ```

- Implement M-SEARCH response:
  ```
  HTTP/1.1 200 OK
  CACHE-CONTROL: max-age=1800
  EXT:
  LOCATION: http://192.168.x.x:port/upnp/description.xml
  SERVER: Windows/10 UPnP/2.0 MillionaireGame/1.0
  ST: urn:schemas-upnp-org:device:MillionaireGameShow:1
  USN: uuid:{device-uuid}::urn:schemas-upnp-org:device:MillionaireGameShow:1
  ```

### Phase 2: Integration with Web Server ✅ Estimated: 2-3 hours

**Task 2.1: Create Network Discovery Manager**
- File: `MillionaireGame.Web/Services/NetworkDiscoveryManager.cs`
- Purpose: Unified interface for both mDNS and UPnP
- Methods:
  - `StartAdvertising(string hostname, int port, string ipAddress)`
  - `StopAdvertising()`
  - `IsAdvertising` property
  - `GetDiscoveryStatus()` - returns status of both services
- Handles:
  - Starting both services in parallel
  - Graceful degradation if one fails
  - Comprehensive logging

**Task 2.2: Modify WebServerHost Integration**
- File: `MillionaireGame/Hosting/WebServerHost.cs`
- Changes:
  - Replace direct `MDnsServiceManager` calls with `NetworkDiscoveryManager`
  - Add UPnP status to server ready message
  - Update startup logs to show both discovery methods
  - Ensure proper cleanup on shutdown

**Task 2.3: Device Description Endpoint**
- Add route: `GET /upnp/description.xml`
- Controller or middleware to serve generated device description
- Set proper Content-Type: `text/xml`
- Cache device description (regenerate only on IP change)

### Phase 3: UI Updates ✅ Estimated: 1-2 hours

**Task 3.1: Control Panel Display**
- File: `MillionaireGame/Forms/ControlPanelForm.cs`
- Updates:
  - Show UPnP status alongside mDNS status
  - Display both discovery URLs:
    - `http://wwtbam.local` (mDNS)
    - Network discovery name (UPnP)
  - Add tooltip explaining each method

**Task 3.2: Web Console Logging**
- Add `WebServerConsole` messages:
  - `[UPnP] Starting SSDP advertisement...`
  - `[UPnP] ✓ SSDP service is now advertising`
  - `[UPnP] Received M-SEARCH request from {ip}`
  - `[UPnP] Service stopped`
  - Error messages with troubleshooting hints

### Phase 4: Testing & Documentation ✅ Estimated: 2-3 hours

**Task 4.1: Testing Scenarios**
- [ ] UPnP service starts without errors
- [ ] Device appears in Windows Network → Network Devices
- [ ] Clicking device opens web interface
- [ ] M-SEARCH requests receive responses
- [ ] Both mDNS and UPnP work simultaneously
- [ ] Graceful failure if port 1900 unavailable
- [ ] Proper cleanup on application exit
- [ ] No port conflicts between mDNS (5353) and UPnP (1900)

**Task 4.2: Documentation Updates**
- Update `wiki/Quick-Start-Guide.md`:
  - Add section on UPnP discovery
  - Explain Windows Network discovery
  - Screenshots of finding device in network browser
- Update `wiki/Troubleshooting.md`:
  - UPnP not working checklist
  - Port 1900 conflicts
  - Firewall rules for UPnP
- Update `README.md`:
  - Mention UPnP/SSDP alongside mDNS
  - Note platform support differences

## NuGet Packages Needed

### Option 1: Pure .NET Implementation (Recommended)
No additional packages - use built-in `UdpClient` and XML serialization.

**Pros:**
- ✅ No dependencies
- ✅ Full control over implementation
- ✅ Smaller binary size

**Cons:**
- ⚠️ More code to write/maintain
- ⚠️ Must handle protocol details manually

### Option 2: Use UPnP Library
Package: `RSSDP` (Portable UPnP library)

```xml
<PackageReference Include="Rssdp" Version="4.0.4" />
```

**Pros:**
- ✅ Handles protocol details
- ✅ Well-tested implementation
- ✅ Active maintenance

**Cons:**
- ⚠️ Additional dependency
- ⚠️ Less control over behavior

**Recommendation:** Start with pure .NET implementation for learning/control. Can switch to RSSDP later if needed.

## Configuration Options

### Settings to Add
```json
{
  "NetworkDiscovery": {
    "EnableMDNS": true,
    "EnableUPnP": true,
    "UPnPCacheMaxAge": 1800,
    "UPnPNotifyInterval": 900,
    "DeviceFriendlyName": "Millionaire Game Audience Participation",
    "DeviceManufacturer": "Millionaire Game Project"
  }
}
```

### Environment Variables (Optional)
- `DISABLE_MDNS=true` - Disable mDNS only
- `DISABLE_UPNP=true` - Disable UPnP only
- `UPNP_PORT=1900` - Change UPnP port (for testing)

## Security Considerations

### Minimal Risk
- UPnP/SSDP only advertises presence, not control
- No UPnP Control Point implementation (we're just a device)
- No port forwarding/IGD features
- Read-only device description

### Best Practices
- ✅ Validate all incoming M-SEARCH messages
- ✅ Rate limit responses (max 1 per second per client)
- ✅ Use random delays (0-100ms) before responding to M-SEARCH
- ✅ Proper XML escaping in device description
- ✅ Bind to specific IP (not 0.0.0.0) when possible

## Firewall Rules

### Windows Firewall
The application already has firewall rules for web server. UPnP adds:
- **Inbound UDP 1900** - SSDP listener
- **Outbound UDP 1900** - SSDP announcements (to multicast group)

### Auto-configuration
- Check if Windows Firewall allows UDP 1900
- Log warning if blocked (don't auto-add rule - security risk)
- Provide instructions in docs for manual configuration

## Performance Impact

### Network Traffic
- **Startup:** 3 NOTIFY messages (~1 KB total)
- **Runtime:** Response to M-SEARCH only when requested (negligible)
- **Periodic:** Optional re-announcement every 15 minutes (~300 bytes)
- **Shutdown:** 1 NOTIFY byebye message (~300 bytes)

**Total:** < 5 KB per session - minimal impact

### CPU/Memory
- **Additional memory:** ~2-5 MB (UDP socket + buffers)
- **CPU impact:** Near zero (event-driven, only active on requests)
- **Threads:** 1 additional background thread for SSDP listener

## Migration Path

### Phase 1 Implementation (This Plan)
- Pure discovery only
- No advanced UPnP features
- Coexists with mDNS

### Future Enhancements (Optional)
- UPnP Events (notify clients of state changes)
- Wake-on-LAN for dormant displays
- DLNA media renderer support (for audio/video streaming)

## Risks & Mitigation

| Risk | Impact | Likelihood | Mitigation |
|------|--------|------------|------------|
| Port 1900 already in use | Service fails to start | Medium | Graceful fallback, log warning, mDNS still works |
| Multicast blocked on network | Discovery doesn't work | Low-Medium | Document requirement, provide IP fallback |
| Windows Firewall blocks UDP | Discovery doesn't work | Medium | Document firewall setup, detect and warn |
| XML parsing errors | Device description invalid | Low | Strict validation, unit tests |
| Conflicts with other UPnP devices | Interference | Very Low | Use unique device type URN |

## Success Criteria

✅ **Must Have:**
- UPnP service starts without errors
- Device visible in Windows Network explorer
- Clicking device opens web interface
- Works alongside mDNS without conflicts
- Graceful failure if port unavailable
- Comprehensive error logging

✅ **Nice to Have:**
- Configurable friendly name
- Status display in Control Panel
- Metrics (M-SEARCH requests received/responded)

## Timeline Estimate

| Phase | Tasks | Estimated Time |
|-------|-------|----------------|
| Phase 1 | Core UPnP/SSDP Service | 4-6 hours |
| Phase 2 | Web Server Integration | 2-3 hours |
| Phase 3 | UI Updates | 1-2 hours |
| Phase 4 | Testing & Documentation | 2-3 hours |
| **Total** | | **9-14 hours** |

## Open Questions

1. **Should we auto-restart UPnP if it fails?** 
   - Recommendation: No - log error and continue with mDNS only

2. **Custom device icon in Windows?**
   - Possible via device description `<iconList>` element
   - Requires hosting PNG/JPG files at specific URLs
   - Add in Phase 2 if desired

3. **IPv6 support?**
   - UPnP supports it (FF02::C multicast)
   - Add after IPv4 working (Phase 1.5)

4. **Should UPnP be enabled by default?**
   - Recommendation: Yes - provides better Windows experience
   - Can be disabled via config if conflicts occur

## References

- [UPnP Device Architecture 2.0](http://www.upnp.org/specs/arch/UPnP-arch-DeviceArchitecture-v2.0.pdf)
- [SSDP Internet Draft](https://tools.ietf.org/html/draft-cai-ssdp-v1-03)
- [RSSDP Library](https://github.com/Yortw/RSSDP)
- [Windows UPnP API Documentation](https://docs.microsoft.com/en-us/windows/win32/upnp/universal-plug-and-play-start-page)

## Next Steps

1. Review this plan and approve/modify as needed
2. Create skeleton classes for Phase 1
3. Implement core SSDP protocol
4. Test discovery on Windows 10/11
5. Integrate with existing web server
6. Update documentation

---

**Ready to proceed?** Comment on this plan with any questions or changes before implementation begins.
