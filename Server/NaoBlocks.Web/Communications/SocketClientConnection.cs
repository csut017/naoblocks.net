using System.Net.Sockets;

namespace NaoBlocks.Web.Communications
{
    /// <summary>
    /// A client connection that uses low-level sockets.
    /// </summary>
    public class SocketClientConnection
        : ClientConnectionBase, IClientConnection
    {
        private readonly ILogger<SocketClientConnection> logger;
        private readonly IMessageProcessor messageProcessor;
        private readonly Socket socket;

        /// <summary>
        /// Initialises a new <see cref="SocketClientConnection"/> instance.
        /// </summary>
        /// <param name="socket">The socket to use.</param>
        /// <param name="type">The type of client.</param>
        /// <param name="messageProcessor">The processor to use for handling incoming messages.</param>
        /// <param name="logger">The logger to use.</param>
        public SocketClientConnection(Socket socket, ClientConnectionType type, IMessageProcessor messageProcessor, ILogger<SocketClientConnection> logger)
        {
            this.Type = type;
            this.socket = socket;
            this.Type = type;
            this.messageProcessor = messageProcessor;
            this.logger = logger;
        }

        /// <summary>
        /// Closes the connection.
        /// </summary>
        public override void Close()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Starts the message processing loop.
        /// </summary>
        public override Task StartAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Disposes of the internal resources.
        /// </summary>
        /// <param name="disposing">True if the instance is being disposed.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Close();
            }
        }
    }
}