using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace NaoBlocks.Web.Communications
{
    /// <summary>
    /// Base class containing common functionality for <see cref="IClientConnection"/> implementations.
    /// </summary>
    public abstract class ClientConnectionBase
    {
        private readonly IList<IClientConnection> listeners = new List<IClientConnection>();
        private readonly IList<ClientMessage> messageLog = new List<ClientMessage>();
        private readonly object messageLogLock = new();
        private readonly ConcurrentQueue<ClientMessage> queue = new();

        /// <summary>
        /// Fired when the connection is closed.
        /// </summary>
        public event EventHandler<EventArgs>? Closed;

        /// <summary>
        /// Gets or sets the hub this client is associated with.
        /// </summary>
        public IHub? Hub { get; set; }

        /// <summary>
        /// Gets or sets the id of this client.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets whether this client is closing.
        /// </summary>
        public bool IsClosing { get; protected set; }

        /// <summary>
        /// Gets the recent notifications.
        /// </summary>
        public IList<NotificationAlert> Notifications { get; } = new List<NotificationAlert>();

        /// <summary>
        /// Gets or sets the associated robot details (for robot clients).
        /// </summary>
        public Robot? Robot { get; set; }

        /// <summary>
        /// Gets or sets the robot status (for robot clients).
        /// </summary>
        public RobotStatus? RobotDetails { get; set; }

        /// <summary>
        /// Gets the client status.
        /// </summary>
        public ClientStatus Status { get; protected set; } = new ClientStatus();

        /// <summary>
        /// Gets the type of connection.
        /// </summary>
        public ClientConnectionType Type { get; protected set; }

        /// <summary>
        /// Gets or sets the associated user (for user clients).
        /// </summary>
        public User? User { get; set; }

        /// <summary>
        /// Adds a new listener for this client.
        /// </summary>
        /// <param name="listener">The <see cref="IClientConnection"/> that will listen.</param>
        public void AddListener(IClientConnection listener)
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
        public abstract void Close();

        /// <summary>
        /// Disposes of this connection.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
        /// <param name="listener">The listening <see cref="IClientConnection"/> to remove.</param>
        public void RemoveListener(IClientConnection listener)
        {
            if (this.listeners.Contains(listener)) this.listeners.Remove(listener);
        }

        /// <summary>
        /// Retrieves the current listeners.
        /// </summary>
        /// <returns>The current listeners.</returns>
        public IEnumerable<IClientConnection> RetrieveListeners()
        {
            var messages = this.listeners.ToArray();
            return messages;
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
        /// Queues a message for sending.
        /// </summary>
        /// <param name="message">The <see cref="ClientMessage"/> to send.</param>
        public void SendMessage(ClientMessage message)
        {
            this.queue.Enqueue(message);
        }

        /// <summary>
        /// Starts the message processing loop.
        /// </summary>
        public abstract Task StartAsync();

        /// <summary>
        /// Disposes of the internal resources.
        /// </summary>
        /// <param name="disposing">True if the instance is being disposed.</param>
        protected abstract void Dispose(bool disposing);

        /// <summary>
        /// Raises the Closed event.
        /// </summary>
        protected void RaiseClosed()
        {
            this.Closed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Attempts to retrieve the next message in the queue.
        /// </summary>
        /// <param name="result">The next message if available, null otherwise.</param>
        /// <returns>True if a message was retrieved, false otherwise.</returns>
        protected bool TryGetNextMessage(out ClientMessage? result)
        {
            return this.queue.TryDequeue(out result);
        }
    }
}