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
        /// Adds a new <see cref="ClientConnection"/>.
        /// </summary>
        /// <param name="client">The <see cref="ClientConnection"/> to add.</param>
        void AddClient(ClientConnection client);

        /// <summary>
        /// Retrieves a <see cref="ClientConnection"/> instance by its identifier.
        /// </summary>
        /// <param name="id">The client identifier.</param>
        /// <returns>The <see cref="ClientConnection"/> if found, null otherwise.</returns>
        ClientConnection? GetClient(long id);

        /// <summary>
        /// Retrieves all the current connections.
        /// </summary>
        /// <param name="type">The client type to retrieve.</param>
        /// <returns>All the current connections of the specified type.</returns>
        IEnumerable<ClientConnection> GetClients(ClientConnectionType type);

        /// <summary>
        /// Retrieves all the current connections regardless of type.
        /// </summary>
        /// <returns>All the current connections.</returns>
        IEnumerable<ClientConnection> GetAllClients();

        /// <summary>
        /// Retrieves all the current monitor connections.
        /// </summary>
        /// <returns>All the current monitor connections.</returns>
        IEnumerable<ClientConnection> GetMonitors();

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
        /// Removes a client.
        /// </summary>
        /// <param name="client">The <see cref="ClientConnection"/> to remove.</param>
        void RemoveClient(ClientConnection client);

        /// <summary>
        /// Adds a new monitor <see cref="ClientConnection"/>.
        /// </summary>
        /// <param name="client">The <see cref="ClientConnection"/> to add.</param>
        void AddMonitor(ClientConnection client);

        /// <summary>
        /// Removes a monitor.
        /// </summary>
        /// <param name="client">The <see cref="ClientConnection"/> to remove.</param>
        void RemoveMonitor(ClientConnection client);
    }
}