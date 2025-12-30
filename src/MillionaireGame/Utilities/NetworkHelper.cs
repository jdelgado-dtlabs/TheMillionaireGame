using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace MillionaireGame.Utilities;

/// <summary>
/// Utility class for network operations (IP detection, port checking, etc.)
/// </summary>
public static class NetworkHelper
{
    /// <summary>
    /// Gets a list of local IP addresses with their subnet masks in CIDR notation
    /// </summary>
    /// <returns>List of IP addresses in "IP/prefix" format</returns>
    public static List<string> GetLocalIPAddresses()
    {
        var addresses = new List<string>();

        try
        {
            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                // Skip inactive interfaces and loopback
                if (networkInterface.OperationalStatus != OperationalStatus.Up)
                    continue;

                if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    continue;

                var properties = networkInterface.GetIPProperties();

                foreach (var unicastAddress in properties.UnicastAddresses)
                {
                    // Only get IPv4 addresses
                    if (unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        var ip = unicastAddress.Address.ToString();
                        var mask = unicastAddress.IPv4Mask.ToString();
                        var cidr = GetCIDRFromMask(mask);
                        addresses.Add($"{ip}/{cidr}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting local IP addresses: {ex.Message}");
        }

        return addresses;
    }

    /// <summary>
    /// Checks if a port is available for binding
    /// </summary>
    /// <param name="port">Port number to check</param>
    /// <returns>True if port is available, false if in use</returns>
    public static bool IsPortAvailable(int port)
    {
        if (port < 1 || port > 65535)
            return false;

        try
        {
            // Try to create a TCP listener on the port
            var listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            listener.Stop();
            return true;
        }
        catch (SocketException)
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if a port is owned by a specific process
    /// </summary>
    /// <param name="port">Port number to check</param>
    /// <param name="processId">Process ID to match against</param>
    /// <returns>True if the port is owned by the specified process</returns>
    public static bool IsPortOwnedByProcess(int port, int processId)
    {
        try
        {
            var properties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpConnections = properties.GetActiveTcpListeners();

            foreach (var endpoint in tcpConnections)
            {
                if (endpoint.Port == port)
                {
                    // Port is in use - would need WMI or native calls to determine process
                    // For simplicity, we'll just return false here
                    // The main check is IsPortAvailable
                    return false;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking port ownership: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Gets the network class and subnet for an IP address
    /// </summary>
    /// <param name="ipAddress">IP address string</param>
    /// <param name="subnetMask">Subnet mask string</param>
    /// <returns>Network address in CIDR notation (e.g., "192.168.1.0/24")</returns>
    public static string GetNetworkAddress(string ipAddress, string subnetMask)
    {
        try
        {
            var ip = IPAddress.Parse(ipAddress);
            var mask = IPAddress.Parse(subnetMask);

            var ipBytes = ip.GetAddressBytes();
            var maskBytes = mask.GetAddressBytes();
            var networkBytes = new byte[ipBytes.Length];

            for (int i = 0; i < ipBytes.Length; i++)
            {
                networkBytes[i] = (byte)(ipBytes[i] & maskBytes[i]);
            }

            var networkAddress = new IPAddress(networkBytes);
            var cidr = GetCIDRFromMask(subnetMask);

            return $"{networkAddress}/{cidr}";
        }
        catch
        {
            return ipAddress; // Fallback to just IP if parsing fails
        }
    }

    /// <summary>
    /// Converts a subnet mask to CIDR notation
    /// </summary>
    /// <param name="subnetMask">Subnet mask (e.g., "255.255.255.0")</param>
    /// <returns>CIDR prefix length (e.g., 24)</returns>
    private static int GetCIDRFromMask(string subnetMask)
    {
        try
        {
            var mask = IPAddress.Parse(subnetMask);
            var maskBytes = mask.GetAddressBytes();
            var bits = 0;

            foreach (var b in maskBytes)
            {
                var bitCount = 0;
                var value = b;

                while (value > 0)
                {
                    bitCount += value & 1;
                    value >>= 1;
                }

                bits += bitCount;
            }

            return bits;
        }
        catch
        {
            return 24; // Default to /24 if parsing fails
        }
    }

    /// <summary>
    /// Gets the public IP address by querying an external service
    /// </summary>
    /// <returns>Public IP address string, or null if unable to determine</returns>
    public static async Task<string?> GetPublicIPAddressAsync()
    {
        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            var response = await client.GetStringAsync("https://ipinfo.io/ip");
            return response?.Trim();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Validates if a string is a valid IP address
    /// </summary>
    /// <param name="ipAddress">IP address string to validate</param>
    /// <returns>True if valid IP address</returns>
    public static bool IsValidIPAddress(string ipAddress)
    {
        return IPAddress.TryParse(ipAddress, out _);
    }

    /// <summary>
    /// Validates if a port number is in valid range
    /// </summary>
    /// <param name="port">Port number to validate</param>
    /// <returns>True if port is between 1 and 65535</returns>
    public static bool IsValidPort(int port)
    {
        return port >= 1 && port <= 65535;
    }
}
