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

        /// <summary>
        /// Creates a new mDNS service manager
        /// </summary>
        /// <param name="serviceName">Service name (becomes serviceName.local)</param>
        /// <param name="port">Port number the web server is listening on</param>
        public MDnsServiceManager(string serviceName, int port)
        {
            ServiceName = serviceName ?? "wwtbam";
            Port = port;

            // Create service discovery instance
            _serviceDiscovery = new ServiceDiscovery();

            // Create service profile for HTTP service
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

            try
            {
                _serviceDiscovery.Advertise(_serviceProfile);
                _isAdvertising = true;

                // Show appropriate URL based on port
                string accessUrl = Port == 80
                    ? $"http://{ServiceName}.local"
                    : $"http://{ServiceName}.local:{Port}";

                Console.WriteLine($"[mDNS] Advertising service: {ServiceName}.local on port {Port}");
                Console.WriteLine($"[mDNS] Service can be accessed at: {accessUrl}");

                // List advertised IP addresses
                var addresses = GetLocalIPAddresses();
                Console.WriteLine($"[mDNS] Advertising {addresses.Length} IP address(es):");
                foreach (var addr in addresses)
                {
                    Console.WriteLine($"[mDNS]   - {addr}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[mDNS] Failed to start advertising: {ex.Message}");
                Console.WriteLine($"[mDNS] Service will still be accessible via IP address");
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
                    Console.WriteLine("[mDNS] No network interfaces found, using loopback");
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
            if (_disposed)
                return;

            StopAdvertising();
            _serviceDiscovery?.Dispose();
            _disposed = true;
        }
    }
}
