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
        /// <param name="isMonitor">Whether the connection is a monitor or not.</param>
        void AddClient(ClientConnection client, bool isMonitor);

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
        /// <returns>All the current connections.</returns>
        IEnumerable<ClientConnection> GetClients(ClientConnectionType type);

        /// <summary>
        /// Sends a message to all connections.
        /// </summary>
        /// <param name="message">The message to send.</param>
        void SendToAll(ClientMessage message);

        /// <summary>
        /// Sends a message to all connections of a specific type.
        /// </summary>
        /// <param name="message">The message to send.</param>
        void SendToAll(ClientMessage message, ClientConnectionType clientType);

        /// <summary>
        /// Sends a message to all monitors.
        /// </summary>
        /// <param name="message">The message to send.</param>
        void SendToMonitors(ClientMessage message);

        void RemoveClient(ClientConnection client);
    }
}
