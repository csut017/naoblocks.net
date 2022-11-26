using System.Net.Sockets;

namespace NaoBlocks.Communications
{
    /// <summary>
    /// The default socket factory.
    /// </summary>
    /// <remarks>
    /// This factory generates wrappers around a new <see cref="Socket"/> instance.
    /// </remarks>
    public class DefaultSocketFactory
        : ISocketFactory
    {
        /// <summary>
        /// Generates a new instance of a <see cref="ISocket"/>.
        /// </summary>
        /// <param name="addressFamily">The address family.</param>
        /// <param name="socketType">The socket type.</param>
        /// <param name="protocolType">The protocol type.</param>
        /// <returns>A new <see cref="ISocket"/> instance.</returns>
        public ISocket New(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
        {
            var socket = new Socket(
                addressFamily,
                socketType,
                protocolType);
            return SocketWrapper.Wrap(socket);
        }
    }
}