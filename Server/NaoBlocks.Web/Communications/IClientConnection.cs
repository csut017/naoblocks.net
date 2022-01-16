using NaoBlocks.Common;
using NaoBlocks.Engine.Data;

namespace NaoBlocks.Web.Communications
{
    /// <summary>
    /// A client connection.
    /// </summary>
    public interface IClientConnection
        : IDisposable
    {
        /// <summary>
        /// Gets or sets the hub this client is associated with.
        /// </summary>
        IHub? Hub { get; set; }

        /// <summary>
        /// Gets or sets the id of this client.
        /// </summary>
        long Id { get; set; }

        /// <summary>
        /// Gets whether this client is closing.
        /// </summary>
        bool IsClosing { get; }

        /// <summary>
        /// Gets the recent notifications.
        /// </summary>
        IList<NotificationAlert> Notifications { get; }

        /// <summary>
        /// Gets or sets the associated robot details (for robot clients).
        /// </summary>
        Robot? Robot { get; set; }

        /// <summary>
        /// Gets or sets the robot status (for robot clients).
        /// </summary>
        RobotStatus? RobotDetails { get; set; }

        /// <summary>
        /// Gets the client status.
        /// </summary>
        ClientStatus Status { get; }

        /// <summary>
        /// Gets the type of connection.
        /// </summary>
        ClientConnectionType Type { get; }

        /// <summary>
        /// Gets or sets the associated user (for user clients).
        /// </summary>
        User? User { get; set; }

        /// <summary>
        /// Fired when the connection is closed.
        /// </summary>
        event EventHandler<EventArgs>? Closed;

        /// <summary>
        /// Adds a new listener for this client.
        /// </summary>
        /// <param name="listener">The <see cref="IClientConnection"/> that will listen.</param>
        void AddListener(IClientConnection listener);

        /// <summary>
        /// Adds a <see cref="NotificationAlert"/> instance.
        /// </summary>
        /// <param name="alert">The <see cref="NotificationAlert"/> instance.</param>
        void AddNotification(NotificationAlert alert);

        /// <summary>
        /// Closes the connection.
        /// </summary>
        void Close();

        /// <summary>
        /// Retrieves the message log.
        /// </summary>
        /// <returns>The messages in the log.</returns>
        Task<IReadOnlyCollection<ClientMessage>> GetMessageLogAsync();

        /// <summary>
        /// Logs a message.
        /// </summary>
        /// <param name="message">The <see cref="ClientMessage"/> to log.</param>
        void LogMessage(ClientMessage message);

        /// <summary>
        /// Send a message to each listener.
        /// </summary>
        /// <param name="message">The <see cref="ClientMessage"/> to send.</param>
        void NotifyListeners(ClientMessage message);

        /// <summary>
        /// Removes a listening client.
        /// </summary>
        /// <param name="listener">The listening <see cref="IClientConnection"/> to remove.</param>
        void RemoveListener(IClientConnection listener);

        /// <summary>
        /// Retrieves the current listeners.
        /// </summary>
        /// <returns>The current listeners.</returns>
        IEnumerable<IClientConnection> RetrieveListeners();

        /// <summary>
        /// Retrieves the pending messages.
        /// </summary>
        /// <returns>The pending messages.</returns>
        IEnumerable<ClientMessage> RetrievePendingMessages();

        /// <summary>
        /// Queues a message for sending.
        /// </summary>
        /// <param name="message">The <see cref="ClientMessage"/> to send.</param>
        void SendMessage(ClientMessage message);

        /// <summary>
        /// Starts the message processing loop.
        /// </summary>
        Task StartAsync();
    }
}