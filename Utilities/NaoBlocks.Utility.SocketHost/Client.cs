using System.Net;
using System.Net.Sockets;

namespace NaoBlocks.Utility.SocketHost
{
    /// <summary>
    /// A client connection.
    /// </summary>
    public class Client
    {
        private readonly Socket handler;

        /// <summary>
        /// Initialises a new <see cref="Client"/> instance.
        /// </summary>
        /// <param name="handler">The socket to the client.</param>
        public Client(Socket handler)
        {
            this.handler = handler;
            this.RemoteEndPoint = handler.RemoteEndPoint;
        }

        /// <summary>
        /// Gets or sets the name of the client.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets the remote endpoint.
        /// </summary>
        public EndPoint? RemoteEndPoint { get; }
    }
}