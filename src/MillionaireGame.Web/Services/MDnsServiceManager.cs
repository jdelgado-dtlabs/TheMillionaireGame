using Makaretu.Dns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace MillionaireGame.Web.Services
{
    /// <summary>
    /// Manages mDNS service advertisement for the web server.
    /// Allows clients to access the server via wwtbam.local instead of IP address.
    /// </summary>
    public class MDnsServiceManager : IDisposable
    {
        private readonly ServiceDiscovery _serviceDiscovery;
        private readonly ServiceProfile _serviceProfile;
        private bool _isAdvertising;
        private bool _disposed;

        public string ServiceName { get; }
        public int Port { get; }
        public string BindAddress { get; }

        /// <summary>
        /// Log helper that uses WebServerConsole via reflection if available
        /// </summary>
        private static void LogInfo(string message)
        {
            try
            {
                var consoleType = Type.GetType("MillionaireGame.Utilities.WebServerConsole, MillionaireGame");
                if (consoleType != null)
                {
                    var method = consoleType.GetMethod("Info", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    method?.Invoke(null, new object[] { message });
                    return;
                }
            }
            catch { }
            Console.WriteLine(message);
        }

        private static void LogWarn(string message)
        {
            try
            {
                var consoleType = Type.GetType("MillionaireGame.Utilities.WebServerConsole, MillionaireGame");
                if (consoleType != null)
                {
                    var method = consoleType.GetMethod("Warn", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    method?.Invoke(null, new object[] { message });
                    return;
                }
            }
            catch { }
            Console.WriteLine(message);
        }

        private static void LogError(string message)
        {
            try
            {
                var consoleType = Type.GetType("MillionaireGame.Utilities.WebServerConsole, MillionaireGame");
                if (consoleType != null)
                {
                    var method = consoleType.GetMethod("Error", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    method?.Invoke(null, new object[] { message });
                    return;
                }
            }
            catch { }
            Console.WriteLine(message);
        }

        /// <summary>
        /// Creates a new mDNS service manager
        /// </summary>
        /// <param name="serviceName">Service name (becomes serviceName.local)</param>
        /// <param name="port">Port number the web server is listening on</param>
        /// <param name="bindAddress">IP address the server is bound to (0.0.0.0, 127.0.0.1, or specific IP)</param>
        public MDnsServiceManager(string serviceName, int port, string bindAddress)
        {
            ServiceName = serviceName ?? "wwtbam";
            Port = port;
            BindAddress = bindAddress ?? "0.0.0.0";

            LogInfo($"[mDNS] Initializing mDNS service: {ServiceName}.local on port {Port}");
            LogInfo($"[mDNS] Bind address: {BindAddress}");

            // Get addresses to advertise
            var addresses = GetAdvertisableAddresses();
            if (addresses.Length == 0)
            {
                LogInfo("[mDNS] No addresses available for advertising (server may be localhost-only)");
                return; // Don't create service discovery if no addresses
            }

            LogInfo($"[mDNS] Will advertise {addresses.Length} address(es):");
            foreach (var addr in addresses)
            {
                LogInfo($"[mDNS]   - {addr}");
            }

            // Create service discovery instance
            _serviceDiscovery = new ServiceDiscovery();

            // Create service profile for HTTP service
            _serviceProfile = new ServiceProfile(
                instanceName: ServiceName,
                serviceName: "_http._tcp",
                port: (ushort)port,
                addresses: addresses
            );

            // Add A/AAAA records for hostname resolution (critical for .local domain to resolve)
            var hostName = $"{ServiceName}.local";
            foreach (var address in addresses)
            {
                if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    // IPv4 - Add A record
                    _serviceProfile.Resources.Add(new ARecord
                    {
                        Name = hostName,
                        Address = address,
                        TTL = TimeSpan.FromSeconds(120)
                    });
                }
                else if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                {
                    // IPv6 - Add AAAA record
                    _serviceProfile.Resources.Add(new AAAARecord
                    {
                        Name = hostName,
                        Address = address,
                        TTL = TimeSpan.FromSeconds(120)
                    });
                }
            }

            // Add TXT records for additional metadata
            _serviceProfile.Resources.Add(new TXTRecord
            {
                Name = _serviceProfile.FullyQualifiedName,
                Strings = new List<string>
                {
                    "path=/",
                    "game=millionaire",
                    "version=1.0.5",
                    $"port={port}"
                }
            });
        }

        /// <summary>
        /// Start advertising the service on the network
        /// </summary>
        public void StartAdvertising()
        {
            if (_isAdvertising || _disposed)
                return;

            // Check if service was properly initialized
            if (_serviceDiscovery == null || _serviceProfile == null)
            {
                LogInfo("[mDNS] Service not initialized (no addresses to advertise)");
                return;
            }

            try
            {
                LogInfo($"[mDNS] Starting mDNS advertisement for {ServiceName}.local...");
                LogInfo($"[mDNS] Listening on UDP port 5353 for mDNS queries");
                
                _serviceDiscovery.Advertise(_serviceProfile);
                _isAdvertising = true;

                // Show appropriate URL based on port
                string accessUrl = Port == 80
                    ? $"http://{ServiceName}.local"
                    : $"http://{ServiceName}.local:{Port}";

                LogInfo($"[mDNS] ✓ mDNS service is now advertising");
                LogInfo($"[mDNS] Access URL: {accessUrl}");
                LogInfo($"[mDNS] Note: Windows may not resolve its own .local domains. Test from another device.");
            }
            catch (Exception ex)
            {
                LogError($"[mDNS] Failed to start advertising: {ex.Message}");
                LogInfo($"[mDNS] Service will still be accessible via IP address");
            }
        }

        /// <summary>
        /// Stop advertising the service
        /// </summary>
        public void StopAdvertising()
        {
            if (!_isAdvertising || _disposed)
                return;

            try
            {
                _serviceDiscovery.Unadvertise(_serviceProfile);
                _isAdvertising = false;
                LogInfo($"[mDNS] Stopped advertising service: {ServiceName}.local");
            }
            catch (Exception ex)
            {
                LogError($"[mDNS] Error stopping advertisement: {ex.Message}");
            }
        }

        /// <summary>
        /// Get IP addresses that should be advertised based on bind address
        /// </summary>
        private IPAddress[] GetAdvertisableAddresses()
        {
            // If bound to localhost only, don't advertise (mDNS for external access only)
            if (BindAddress == "127.0.0.1" || BindAddress == "localhost")
            {
                LogInfo("[mDNS] Server bound to localhost only - mDNS not applicable");
                return Array.Empty<IPAddress>();
            }

            var addresses = GetLocalIPAddresses();

            // If bound to all interfaces (0.0.0.0), advertise all
            if (BindAddress == "0.0.0.0")
            {
                return addresses;
            }

            // If bound to specific IP, only advertise that IP
            if (IPAddress.TryParse(BindAddress, out var specificIP))
            {
                var filtered = addresses.Where(a => a.Equals(specificIP)).ToArray();
                if (filtered.Length == 0)
                {
                    LogWarn($"[mDNS] Warning: Bind address {BindAddress} not found in active interfaces");
                }
                return filtered;
            }

            // Default: advertise all
            return addresses;
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
                        // Include IPv4 addresses (skip link-local IPv6)
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            addresses.Add(ip.Address);
                        }
                        else if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6 &&
                                !ip.Address.IsIPv6LinkLocal)
                        {
                            // Include global IPv6 addresses
                            addresses.Add(ip.Address);
                        }
                    }
                }

                if (addresses.Count == 0)
                {
                    // Fallback: add localhost
                    LogWarn("[mDNS] No network interfaces found, using loopback");
                    addresses.Add(IPAddress.Loopback);
                }
            }
            catch (Exception ex)
            {
                LogError($"[mDNS] Error getting network interfaces: {ex.Message}");
                addresses.Add(IPAddress.Loopback);
            }

            return addresses.ToArray();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            StopAdvertising();
            _serviceDiscovery?.Dispose();
            _disposed = true;
        }
    }
}
