using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Net.WebSockets;

namespace NaoBlocks.Web.Communications
{
    /// <summary>
    /// Defines a client connection.
    /// </summary>
    public class ClientConnection
        : IDisposable
    {
        private readonly CancellationTokenSource cancellationSource = new CancellationTokenSource();
        private readonly IMessageProcessor messageProcessor;
        private readonly ConcurrentQueue<ClientMessage> queue = new ConcurrentQueue<ClientMessage>();
        private readonly WebSocket socket;
        private readonly IList<ClientConnection> listeners = new List<ClientConnection>();
        private readonly IList<ClientMessage> messageLog = new List<ClientMessage>();
        private readonly object messageLogLock = new object();
        private bool isRunning;

        /// <summary>
        /// Initialises a new <see cref="ClientConnection"/> instance.
        /// </summary>
        /// <param name="socket">The socket to use.</param>
        /// <param name="type">The type of client.</param>
        /// <param name="messageProcessor">The processor to use for handling incoming messages.</param>
        public ClientConnection(WebSocket socket, ClientConnectionType type, IMessageProcessor messageProcessor)
        {
            this.socket = socket;
            this.Type = type;
            this.messageProcessor = messageProcessor;
        }

        /// <summary>
        /// Gets or sets the hub this client is associated with.
        /// </summary>
        public IHub? Hub { get; set; }

        /// <summary>
        /// Gets the type of connection.
        /// </summary>
        public ClientConnectionType Type { get; private set; }

        /// <summary>
        /// Queues a message for sending.
        /// </summary>
        /// <param name="message">The <see cref="ClientMessage"/> to send.</param>
        public void SendMessage(ClientMessage message)
        {
            this.queue.Enqueue(message);
        }

        /// <summary>
        /// Retrieves the pending messages.
        /// </summary>
        /// <returns>The pending messages.</returns>
        public IEnumerable<ClientMessage> RetrievePendingMessages()
        {
            var messages = this.queue.ToArray();
            return messages;
        }

        /// <summary>
        /// Retrieves the current listeners.
        /// </summary>
        /// <returns>The current listeners.</returns>
        public IEnumerable<ClientConnection> RetrieveListeners()
        {
            var messages = this.listeners.ToArray();
            return messages;
        }

        /// <summary>
        /// Fired when the connection is closed.
        /// </summary>
        public event EventHandler<EventArgs>? Closed;

        /// <summary>
        /// Gets or sets the id of this client.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets whether this client is closing.
        /// </summary>
        public bool IsClosing { get; private set; }

        /// <summary>
        /// Gets or sets the associated for (for robot clients).
        /// </summary>
        public Robot? Robot { get; set; }

        /// <summary>
        /// Gets the client status.
        /// </summary>
        public ClientStatus Status { get; } = new ClientStatus();

        /// <summary>
        /// Gets or sets the robot status (for robot clients).
        /// </summary>
        public RobotStatus? RobotDetails { get; set; }

        /// <summary>
        /// Gets the recent notifications.
        /// </summary>
        public IList<NotificationAlert> Notifications { get; } = new List<NotificationAlert>();

        /// <summary>
        /// Gets or sets the associated user (for user clients).
        /// </summary>
        public User? User { get; set; }

        /// <summary>
        /// Adds a new listener for this client.
        /// </summary>
        /// <param name="listener">The <see cref="ClientConnection"/> that will listen.</param>
        public void AddListener(ClientConnection listener)
        {
            this.listeners.Add(listener);
            listener.Closed += (o, e) => this.RemoveListener(listener);
        }

        /// <summary>
        /// Adds a <see cref="NotificationAlert"/> instance.
        /// </summary>
        /// <param name="alert">The <see cref="NotificationAlert"/> instance.</param>
        public void AddNotification(NotificationAlert alert)
        {
            this.Notifications.Add(alert);
            if (this.Notifications.Count > 20) this.Notifications.RemoveAt(0);
        }

        /// <summary>
        /// Closes the connection.
        /// </summary>
        public void Close()
        {
            this.cancellationSource.Cancel();
            if (this.isRunning)
            {
                this.IsClosing = true;
            }
            else
            {
                this.Closed?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Disposes of this connection.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Logs a message.
        /// </summary>
        /// <param name="message">The <see cref="ClientMessage"/> to log.</param>
        public void LogMessage(ClientMessage message)
        {
            lock (this.messageLogLock)
            {
                this.messageLog.Add(message.Clone());
                if (this.messageLog.Count > 100)
                {
                    this.messageLog.RemoveAt(0);
                }
            }
        }

        /// <summary>
        /// Retrieves the message log.
        /// </summary>
        /// <returns>The messages in the log.</returns>
        public Task<IReadOnlyCollection<ClientMessage>> GetMessageLogAsync()
        {
            var clone = new ClientMessage[this.messageLog.Count];
            lock (this.messageLogLock)
            {
                this.messageLog.CopyTo(clone, 0);
            }
            IReadOnlyCollection<ClientMessage> data = new ReadOnlyCollection<ClientMessage>(clone);
            return Task.FromResult(data);
        }

        /// <summary>
        /// Send a message to each listener.
        /// </summary>
        /// <param name="message">The <see cref="ClientMessage"/> to send.</param>
        public void NotifyListeners(ClientMessage message)
        {
            foreach (var listener in this.listeners)
            {
                listener.SendMessage(message.Clone());
            }
        }

        /// <summary>
        /// Removes a listening client.
        /// </summary>
        /// <param name="listener">The listening <see cref="ClientConnection"/> to remove.</param>
        public void RemoveListener(ClientConnection listener)
        {
            if (this.listeners.Contains(listener)) this.listeners.Remove(listener);
        }

        /// <summary>
        /// Starts the message processing loop.
        /// </summary>
        public async Task StartAsync()
        {
            this.IsClosing = false;
            this.isRunning = true;
            var pushTask = Task.Run(async () => await this.PushMessagesAsync(this.cancellationSource.Token));
            try
            {
                while (!this.IsClosing)
                {
                    var (response, message) = await this.ReceiveFullMessageAsync(this.cancellationSource.Token);
                    if (response.MessageType == WebSocketMessageType.Close)
                        break;

                    if (message.Any())
                    {
                        var msg = ClientMessage.FromArray(message.ToArray());
                        await this.messageProcessor.ProcessAsync(this, msg);
                    }
                }
                await this.socket.CloseAsync(WebSocketCloseStatus.Empty, string.Empty, CancellationToken.None);
                this.isRunning = false;
                this.Closed?.Invoke(this, EventArgs.Empty);
            }
            catch (WebSocketException ex)
            {
                switch (ex.WebSocketErrorCode)
                {
                    case WebSocketError.ConnectionClosedPrematurely:
                        this.isRunning = false;
                        this.Closed?.Invoke(this, EventArgs.Empty);
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
        protected virtual void Dispose(bool disposing)
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
                if (this.queue.TryDequeue(out ClientMessage? message))
                {
                    await this.socket.SendAsync(message.ToArray(), WebSocketMessageType.Text, true, cancelToken);
                }
                else
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
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