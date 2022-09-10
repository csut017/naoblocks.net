using NaoBlocks.Common;

namespace NaoBlocks.Web.Communications
{
    /// <summary>
    /// A communications hub for working with websocket clients.
    /// </summary>
    public interface IHub
        : IDisposable
    {
        /// <summary>
        /// Adds a new <see cref="IClientConnection"/>.
        /// </summary>
        /// <param name="client">The <see cref="IClientConnection"/> to add.</param>
        void AddClient(IClientConnection client);

        /// <summary>
        /// Adds a new monitor <see cref="IClientConnection"/>.
        /// </summary>
        /// <param name="client">The <see cref="IClientConnection"/> to add.</param>
        void AddMonitor(IClientConnection client);

        /// <summary>
        /// Retrieves all the current connections regardless of type.
        /// </summary>
        /// <returns>All the current connections.</returns>
        IEnumerable<IClientConnection> GetAllClients();

        /// <summary>
        /// Retrieves a <see cref="IClientConnection"/> instance by its identifier.
        /// </summary>
        /// <param name="id">The client identifier.</param>
        /// <returns>The <see cref="IClientConnection"/> if found, null otherwise.</returns>
        IClientConnection? GetClient(long id);

        /// <summary>
        /// Retrieves all the current connections.
        /// </summary>
        /// <param name="type">The client type to retrieve.</param>
        /// <returns>All the current connections of the specified type.</returns>
        IEnumerable<IClientConnection> GetClients(ClientConnectionType type);

        /// <summary>
        /// Retrieves all the current monitor connections.
        /// </summary>
        /// <returns>All the current monitor connections.</returns>
        IEnumerable<IClientConnection> GetMonitors();

        /// <summary>
        /// Removes a client.
        /// </summary>
        /// <param name="client">The <see cref="IClientConnection"/> to remove.</param>
        void RemoveClient(IClientConnection client);

        /// <summary>
        /// Removes a monitor.
        /// </summary>
        /// <param name="client">The <see cref="IClientConnection"/> to remove.</param>
        void RemoveMonitor(IClientConnection client);

        /// <summary>
        /// Sends a message to all connections.
        /// </summary>
        /// <param name="message">The message to send.</param>
        void SendToAll(ClientMessage message);

        /// <summary>
        /// Sends a message to all connections of a specific type.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="clientType">The type of client to send the messages to.</param>
        void SendToAll(ClientMessage message, ClientConnectionType clientType);

        /// <summary>
        /// Sends a message to all monitors.
        /// </summary>
        /// <param name="message">The message to send.</param>
        void SendToMonitors(ClientMessage message);

        /// <summary>
        /// Starts the hub's local processing loops.
        /// </summary>
        void Start();
    }
}