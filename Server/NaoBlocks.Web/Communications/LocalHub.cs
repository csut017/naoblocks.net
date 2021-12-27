using NaoBlocks.Common;
using System.Globalization;

namespace NaoBlocks.Web.Communications
{
    /// <summary>
    /// Defines a local, in-memory communications hub.
    /// </summary>
    public class LocalHub : IHub
    {
        private readonly IDictionary<long, ClientConnection> clients = new Dictionary<long, ClientConnection>();
        private long nextClientId;
        private bool disposedValue;
        private readonly HashSet<ClientConnection> monitors = new();
        private readonly ReaderWriterLockSlim clientsLock = new();
        private readonly ReaderWriterLockSlim monitorLock = new();

        /// <summary>
        /// Adds a new <see cref="ClientConnection"/>.
        /// </summary>
        /// <param name="client">The <see cref="ClientConnection"/> to add.</param>
        public void AddClient(ClientConnection client)
        {
            var isLocked = false;
            try
            {
                isLocked = this.clientsLock.TryEnterWriteLock(TimeSpan.FromSeconds(5));
                if (isLocked)
                {
                    client.Id = ++this.nextClientId;
                    client.Hub = this;
                    this.clients.Add(client.Id, client);
                    client.Closed += (o, e) => this.RemoveClient(client);
                }
                else
                {
                    throw new TimeoutException("Unable to add client");
                }
            }
            finally
            {
                if (isLocked) this.clientsLock.ExitWriteLock();
            }

            var msg = GenerateAddClientMessage(client);
            this.SendToMonitors(msg);
        }

        /// <summary>
        /// Retrieves a <see cref="ClientConnection"/> instance by its identifier.
        /// </summary>
        /// <param name="id">The client identifier.</param>
        /// <returns>The <see cref="ClientConnection"/> if found, null otherwise.</returns>
        public ClientConnection? GetClient(long id)
        {
            this.clientsLock.TryEnterReadLock(TimeSpan.FromSeconds(1));
            try
            {
                return this.clients.TryGetValue(id, out ClientConnection? client) ? client : null;
            }
            finally
            {
                this.clientsLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Retrieves all the current connections.
        /// </summary>
        /// <param name="type">The client type to retrieve.</param>
        /// <returns>All the current connections.</returns>
        public IEnumerable<ClientConnection> GetClients(ClientConnectionType type)
        {
            this.clientsLock.EnterReadLock();
            try
            {
                var clients = this.clients
                    .Where(c => c.Value.Type == type)
                    .Select(c => c.Value)
                    .ToArray();
                return clients;
            }
            finally
            {
                this.clientsLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Retrieves all the current connections regardless of type.
        /// </summary>
        /// <returns>All the current connections.</returns>
        public IEnumerable<ClientConnection> GetAllClients()
        {
            this.clientsLock.EnterReadLock();
            try
            {
                var clients = this.clients
                    .Select(c => c.Value)
                    .ToArray();
                return clients;
            }
            finally
            {
                this.clientsLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Sends a message to all connections.
        /// </summary>
        /// <param name="message">The message to send.</param>
        public void SendToAll(ClientMessage message)
        {
            var monitors = this.GetAllClients();
            SendToAllInternal(message, monitors);
        }

        /// <summary>
        /// Sends a message to all connections of a specific type.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="clientType">The type of client to send the messages to.</param>
        public void SendToAll(ClientMessage message, ClientConnectionType clientType)
        {
            var monitors = this.GetClients(clientType);
            SendToAllInternal(message, monitors);
        }

        /// <summary>
        /// Sends a message to all monitors.
        /// </summary>
        /// <param name="message">The message to send.</param>
        public void SendToMonitors(ClientMessage message)
        {
            var monitors = this.GetMonitors();
            SendToAllInternal(message, monitors);
        }

        /// <summary>
        /// Removes a client.
        /// </summary>
        /// <param name="client">The <see cref="ClientConnection"/> to remove.</param>
        public void RemoveClient(ClientConnection client)
        {
            var isLocked = false;
            try
            {
                isLocked = this.clientsLock.TryEnterWriteLock(TimeSpan.FromSeconds(5));
                if (isLocked)
                {
                    client.Hub = null;
                    this.clients.Remove(client.Id);
                }
                else
                {
                    throw new TimeoutException("Unable to add client");
                }
            }
            finally
            {
                if (isLocked) this.clientsLock.ExitWriteLock();
            }

            // If we cannot remove the client, an error will be thrown earlier, therefore, this call will be safe
            var msg = new ClientMessage
            {
                Type = ClientMessageType.ClientRemoved
            };
            msg.Values["ClientId"] = client.Id.ToString(CultureInfo.InvariantCulture);
            this.SendToMonitors(msg);
        }

        /// <summary>
        /// Retrieves all the current monitor connections.
        /// </summary>
        /// <returns>All the current monitor connections.</returns>
        public IEnumerable<ClientConnection> GetMonitors()
        {
            this.monitorLock.EnterReadLock();
            try
            {
                var monitors = this.monitors
                    .ToArray();
                return monitors;
            }
            finally
            {
                this.monitorLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Adds a new monitor <see cref="ClientConnection"/>.
        /// </summary>
        /// <param name="client">The <see cref="ClientConnection"/> to add.</param>
        public void AddMonitor(ClientConnection client)
        {
            var isLocked = false;
            try
            {
                isLocked = this.monitorLock.TryEnterWriteLock(TimeSpan.FromSeconds(5));
                if (isLocked)
                {
                    this.monitors.Add(client);
                    client.Closed += (o, e) => this.RemoveMonitor(client);
                }
                else
                {
                    throw new TimeoutException("Unable to add monitor");
                }
            }
            finally
            {
                if (isLocked) this.monitorLock.ExitWriteLock();
            }

            var clients = this.GetAllClients();
            foreach (var existing in clients)
            {
                var msg = GenerateAddClientMessage(existing);
                client.SendMessage(msg);
            }
        }

        /// <summary>
        /// Generates the message for adding a client.
        /// </summary>
        /// <param name="client">The <see cref="ClientConnection"/> that is being notified.</param>
        /// <returns>A <see cref="ClientMessage"/> containing the details about the client.</returns>
        private static ClientMessage GenerateAddClientMessage(ClientConnection client)
        {
            var msg = new ClientMessage
            {
                Type = ClientMessageType.ClientAdded
            };
            msg.Values["ClientId"] = client.Id.ToString(CultureInfo.InvariantCulture);
            msg.Values["Type"] = "Unknown";
            if (client.Type == ClientConnectionType.Robot)
            {
                msg.Values["Type"] = "robot";
                msg.Values["SubType"] = client?.Robot?.Type?.Name ?? "Unknown";
                msg.Values["Name"] = client?.Robot?.FriendlyName ?? "Unknown";
                msg.Values["state"] = client?.Status?.Message ?? "Unknown";
            }
            else
            {
                msg.Values["Type"] = "user";
                msg.Values["SubType"] = client?.User?.Role.ToString() ?? "Unknown";
                msg.Values["Name"] = client?.User?.Name ?? "Unknown";
            }

            return msg;
        }

        /// <summary>
        /// Removes a monitor.
        /// </summary>
        /// <param name="client">The <see cref="ClientConnection"/> to remove.</param>
        public void RemoveMonitor(ClientConnection client)
        {
            var isLocked = false;
            try
            {
                isLocked = this.monitorLock.TryEnterWriteLock(TimeSpan.FromSeconds(5));
                if (isLocked)
                {
                    if (this.monitors.Contains(client)) this.monitors.Remove(client);
                }
                else
                {
                    throw new TimeoutException("Unable to remove monitor");
                }
            }
            finally
            {
                if (isLocked) this.monitorLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Disposes of this hub.
        /// </summary>
        /// <param name="disposing">Whether this call is via GC or not.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.clientsLock.Dispose();
                    this.monitorLock.Dispose();
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// Cleans up all resources used by this hub.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Sends a message to all clients in the enumerable.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="clients">The clients to send the message to.</param>
        private static void SendToAllInternal(ClientMessage message, IEnumerable<ClientConnection> clients)
        {
            foreach (var client in clients)
            {
                client.SendMessage(message);
            }
        }
    }
}