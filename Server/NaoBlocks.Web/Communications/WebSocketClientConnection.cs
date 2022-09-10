using NaoBlocks.Common;
using System.Net.WebSockets;

namespace NaoBlocks.Web.Communications
{
    /// <summary>
    /// Defines a client connection using WebSockets.
    /// </summary>
    public class WebSocketClientConnection
        : ClientConnectionBase, IClientConnection
    {
        private readonly CancellationTokenSource cancellationSource = new();
        private readonly ILogger<WebSocketClientConnection> logger;
        private readonly IMessageProcessor messageProcessor;
        private readonly WebSocket socket;
        private bool isRunning;

        /// <summary>
        /// Initialises a new <see cref="WebSocketClientConnection"/> instance.
        /// </summary>
        /// <param name="socket">The socket to use.</param>
        /// <param name="type">The type of client.</param>
        /// <param name="messageProcessor">The processor to use for handling incoming messages.</param>
        /// <param name="logger">The logger to use.</param>
        public WebSocketClientConnection(WebSocket socket, ClientConnectionType type, IMessageProcessor messageProcessor, ILogger<WebSocketClientConnection> logger)
        {
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
            this.logger.LogInformation($"Closing socket connection {this.Id}: server closed");
            this.cancellationSource.Cancel();
            if (this.isRunning)
            {
                this.IsClosing = true;
            }
            else
            {
                this.RaiseClosed();
            }
        }

        /// <summary>
        /// Starts the message processing loop.
        /// </summary>
        public override async Task StartAsync()
        {
            this.logger.LogInformation($"Starting socket connection {this.Id}");
            this.IsClosing = false;
            this.isRunning = true;
            var pushTask = Task.Run(async () => await this.PushMessagesAsync(this.cancellationSource.Token));
            try
            {
                while (!this.IsClosing)
                {
                    var (response, message) = await this.ReceiveFullMessageAsync(this.cancellationSource.Token);
                    if (response.MessageType == WebSocketMessageType.Close)
                    {
                        this.logger.LogInformation($"Closing socket connection {this.Id}: client closed");
                        break;
                    }

                    if (message.Any())
                    {
                        var msg = ClientMessage.FromArray(message.ToArray());
                        await this.messageProcessor.ProcessAsync(this, msg);
                    }
                }
                await this.socket.CloseAsync(WebSocketCloseStatus.Empty, string.Empty, CancellationToken.None);
                this.isRunning = false;
                this.RaiseClosed();
            }
            catch (WebSocketException ex)
            {
                switch (ex.WebSocketErrorCode)
                {
                    case WebSocketError.ConnectionClosedPrematurely:
                        this.logger.LogInformation($"Closing socket connection {this.Id}: closed on error");
                        this.isRunning = false;
                        this.RaiseClosed();
                        break;

                    default:
                        throw;
                }
            }
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
                this.socket.Dispose();
                this.cancellationSource.Dispose();
            }
        }

        /// <summary>
        /// Internal message processing loop for sending messages.
        /// </summary>
        private async Task PushMessagesAsync(CancellationToken cancelToken)
        {
            while (!cancelToken.IsCancellationRequested)
            {
                if (this.TryGetNextMessage(out var message))
                {
                    await this.socket.SendAsync(message!.ToArray(), WebSocketMessageType.Text, true, cancelToken);
                }
                else
                {
                    Thread.Sleep(TimeSpan.FromSeconds(0.1));
                }
            }
        }

        /// <summary>
        /// Extract a message from the web socket and stitch together the parts.
        /// </summary>
        private async Task<(WebSocketReceiveResult, IEnumerable<byte>)> ReceiveFullMessageAsync(CancellationToken cancelToken)
        {
            WebSocketReceiveResult response;
            var message = new List<byte>();

            var buffer = new byte[4096];
            do
            {
                response = await this.socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancelToken);
                message.AddRange(new ArraySegment<byte>(buffer, 0, response.Count));
            } while (!response.EndOfMessage);

            return (response, message);
        }
    }
}