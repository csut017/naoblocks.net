using NaoBlocks.Common;
using NaoBlocks.Communications;

namespace NaoBlocks.Web.Communications
{
    /// <summary>
    /// A client connection that uses low-level sockets.
    /// </summary>
    public class SocketClientConnection
        : ClientConnectionBase, IClientConnection
    {
        private readonly Client client;
        private readonly ILogger<SocketClientConnection> logger;
        private readonly IMessageProcessor messageProcessor;

        /// <summary>
        /// Initialises a new <see cref="SocketClientConnection"/> instance.
        /// </summary>
        /// <param name="client">The underlying <see cref="Client"/> instance.</param>
        /// <param name="type">The type of client.</param>
        /// <param name="messageProcessor">The processor to use for handling incoming messages.</param>
        /// <param name="logger">The logger to use.</param>
        public SocketClientConnection(Client client, ClientConnectionType type, IMessageProcessor messageProcessor, ILogger<SocketClientConnection> logger)
        {
            this.Type = type;
            this.client = client;
            this.messageProcessor = messageProcessor;
            this.logger = logger;
            this.client.MessageReceived += OnMessageReceived;
        }

        /// <summary>
        /// Closes the connection.
        /// </summary>
        public override void Close()
        {
            this.logger.LogInformation($"Closing socket connection {this.Id}: server closed");

            this.RaiseClosed();
        }

        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <param name="message">The <see cref="ClientMessage"/> to send.</param>
        public override void SendMessage(ClientMessage message)
        {
            this.client
                .SendMessageAsync(message, TimeSpan.FromSeconds(5))
                .Wait();
        }

        /// <summary>
        /// Disposes of the internal resources.
        /// </summary>
        /// <param name="disposing">True if the instance is being disposed.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.client.MessageReceived -= OnMessageReceived;
                this.Close();
            }
        }

        private void OnMessageReceived(object? sender, ReceivedMessage e)
        {
            this.logger.LogInformation($"Message received from {e.Client.FullName} [{e.Type}]");
            this.messageProcessor.ProcessAsync(this, e).Wait();
        }
    }
}