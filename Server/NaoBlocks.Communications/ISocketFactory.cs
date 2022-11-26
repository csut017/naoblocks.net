using System.Net.Sockets;

namespace NaoBlocks.Communications
{
    /// <summary>
    /// Interface for generating socket instances.
    /// </summary>
    public interface ISocketFactory
    {
        /// <summary>
        /// Generates a new instance of a <see cref="ISocket"/>.
        /// </summary>
        /// <param name="addressFamily">The address family.</param>
        /// <param name="socketType">The socket type.</param>
        /// <param name="protocolType">The protocol type.</param>
        /// <returns>A new <see cref="ISocket"/> instance.</returns>
        ISocket New(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType);
    }
}