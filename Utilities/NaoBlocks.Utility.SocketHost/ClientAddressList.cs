using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace NaoBlocks.Utility.SocketHost
{
    /// <summary>
    /// Helper for retrieving client addresses.
    /// </summary>
    public static class ClientAddressList
    {
        /// <summary>
        /// Retrieves all the possible client IP addresses.
        /// </summary>
        /// <returns>The list of possible addresses.</returns>
        public static IEnumerable<IPAddress> RetrieveAddresses()
        {
            var availableInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                .OrderByDescending(c => c.Speed)
                .Where(c => c.NetworkInterfaceType != NetworkInterfaceType.Loopback && c.OperationalStatus == OperationalStatus.Up);
            foreach (var availableInterface in availableInterfaces)
            {
                var props = availableInterface.GetIPProperties();
                var ip4Addresses = props.UnicastAddresses
                    .Where(c => c.Address.AddressFamily == AddressFamily.InterNetwork)
                    .Select(c => c.Address)
                    .ToArray();
                foreach (var ip4Address in ip4Addresses)
                {
                    yield return ip4Address;
                }
            }

            yield break;
        }
    }
}