using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace NaoBlocks.Web.Helpers
{
    /// <summary>
    /// Stores the list of possible client addresses.
    /// </summary>
    public static class ClientAddressList
    {
        private static readonly List<string> addresses = new();

        /// <summary>
        /// Adds a new client address.
        /// </summary>
        /// <param name="address">The address to add.</param>
        public static void Add(params string[] address)
        {
            addresses.AddRange(address);
        }

        /// <summary>
        /// Clears the existing addresses.
        /// </summary>
        public static void Clear()
        {
            addresses.Clear();
        }

        /// <summary>
        /// Gets the addresses that a client can use to connect to this server.
        /// </summary>
        /// <returns>A readonly only view of the possible addresses.</returns>
        public static ReadOnlyCollection<string> Get()
        {
            return new ReadOnlyCollection<string>(addresses);
        }

        /// <summary>
        /// Initialises the list of IP addresses.
        /// </summary>
        public static void Initialise()
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
                    ClientAddressList.Add($"http://{ip4Address}:5000");
                }
            }
        }
    }
}