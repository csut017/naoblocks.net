using NaoBlocks.Common;
using NaoBlocks.Communications;
using System.Globalization;
using System.Net;
using System.Net.Sockets;

namespace NaoBlocks.Web.Communications
{
    /// <summary>
    /// Defines a local, in-memory communications hub.
    /// </summary>
    public class LocalHub : IHub
    {
        private readonly Semaphore allDone = new(0, 1);
        private readonly CancellationTokenSource cancellationSource = new();
        private readonly IDictionary<long, IClientConnection> clients = new Dictionary<long, IClientConnection>();
        private readonly ReaderWriterLockSlim clientsLock = new();
        private readonly ILogger<LocalHub> logger;
        private readonly ReaderWriterLockSlim monitorLock = new();
        private readonly HashSet<IClientConnection> monitors = new();
        private readonly IServiceProvider services;
        private bool disposedValue;
        private long nextClientId;
        private SocketListener? socketListener;

        /// <summary>
        /// Initialises a new <see cref="LocalHub"/> instance.
        /// </summary>
        public LocalHub(ILogger<LocalHub> logger, IHostApplicationLifetime? appLifetime, IServiceProvider services)
        {
            this.logger = logger;
            this.services = services;
            appLifetime?.ApplicationStopping.Register(() =>
            {
                this.cancellationSource.Cancel();

                this.socketListener?.Close();
                this.logger.LogInformation("Socket listener closed");
                allDone.WaitOne();
            });
        }

        /// <summary>
        /// Adds a new <see cref="IClientConnection"/>.
        /// </summary>
        /// <param name="client">The <see cref="IClientConnection"/> to add.</param>
        public void AddClient(IClientConnection client)
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
        /// Adds a new monitor <see cref="IClientConnection"/>.
        /// </summary>
        /// <param name="client">The <see cref="IClientConnection"/> to add.</param>
        public void AddMonitor(IClientConnection client)
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
        /// Cleans up all resources used by this hub.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Retrieves all the current connections regardless of type.
        /// </summary>
        /// <returns>All the current connections.</returns>
        public IEnumerable<IClientConnection> GetAllClients()
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
        /// Retrieves a <see cref="IClientConnection"/> instance by its identifier.
        /// </summary>
        /// <param name="id">The client identifier.</param>
        /// <returns>The <see cref="IClientConnection"/> if found, null otherwise.</returns>
        public IClientConnection? GetClient(long id)
        {
            this.clientsLock.TryEnterReadLock(TimeSpan.FromSeconds(1));
            try
            {
                return this.clients.TryGetValue(id, out IClientConnection? client) ? client : null;
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
        public IEnumerable<IClientConnection> GetClients(ClientConnectionType type)
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
        /// Retrieves all the current monitor connections.
        /// </summary>
        /// <returns>All the current monitor connections.</returns>
        public IEnumerable<IClientConnection> GetMonitors()
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
        /// Removes a client.
        /// </summary>
        /// <param name="client">The <see cref="IClientConnection"/> to remove.</param>
        public void RemoveClient(IClientConnection client)
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
        /// Removes a monitor.
        /// </summary>
        /// <param name="client">The <see cref="IClientConnection"/> to remove.</param>
        public void RemoveMonitor(IClientConnection client)
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
        /// Starts the hub's processing loops.
        /// </summary>
        public void Start()
        {
            new Task(
                async () => await this.CheckClientsAreAvailable(this.cancellationSource.Token),
                this.cancellationSource.Token,
                TaskCreationOptions.LongRunning).Start();
            new Task(
                async () => await this.RunSocketListener(this.cancellationSource.Token),
                this.cancellationSource.Token,
                TaskCreationOptions.LongRunning).Start();
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
                    this.cancellationSource.Cancel();
                    this.cancellationSource.Dispose();
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// Generates the message for adding a client.
        /// </summary>
        /// <param name="client">The <see cref="IClientConnection"/> that is being notified.</param>
        /// <returns>A <see cref="ClientMessage"/> containing the details about the client.</returns>
        private static ClientMessage GenerateAddClientMessage(IClientConnection client)
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
        /// Sends a message to all clients in the enumerable.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="clients">The clients to send the message to.</param>
        private static void SendToAllInternal(ClientMessage message, IEnumerable<IClientConnection> clients)
        {
            foreach (var client in clients)
            {
                client.SendMessage(message);
            }
        }

        /// <summary>
        /// Internal message processing loop to check robot clients are not in a stalled state.
        /// </summary>
        /// <param name="cancelToken">The cancellation token to use in cancelling this loop.</param>
        private async Task CheckClientsAreAvailable(CancellationToken cancelToken)
        {
            this.logger.LogInformation("Starting stalled client check");
            while (!cancelToken.IsCancellationRequested)
            {
                logger.LogInformation("Checking for stalled robots");
                var clients = this.GetClients(ClientConnectionType.Robot);
                var deadline = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(2));
                foreach (var client in clients.Where(c => c.RobotDetails != null))
                {
                    if (!client.Status.IsAvailable && (client.RobotDetails!.LastUpdateTime < deadline))
                    {
                        logger.LogInformation($"Robot {client.Robot?.MachineName} appears to be stalled: sending stop message");
                        client.SendMessage(new ClientMessage(ClientMessageType.StopProgram));
                    }
                }

                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(2), cancelToken);
                }
                catch (TaskCanceledException)
                {
                    // Don't care about this exception, just means the task was cancelled
                }
            }

            this.logger.LogInformation("Stopped stalled client check");
            allDone.Release();
        }

        /// <summary>
        /// Internal system for listening for incoming socket requests.
        /// </summary>
        /// <param name="cancelToken">The cancellation token to use in cancelling this system.</param>
        private async Task RunSocketListener(CancellationToken cancelToken)
        {
            var hostInfo = await Dns.GetHostEntryAsync(Dns.GetHostName(), cancelToken);
            var endpoint = new IPEndPoint(
                hostInfo.AddressList.First(a => a.AddressFamily == AddressFamily.InterNetwork),
                5002);
            try
            {
                this.logger.LogInformation($"Starting to listen on {endpoint}");
                this.socketListener = new SocketListener(endpoint);
                var connections = new Dictionary<int, SocketClientConnection>();
                var nextClient = 0;
                this.socketListener.ClientConnected += (_, c) =>
                {
                    this.logger.LogInformation($"Client connected from {c.RemoteEndPoint}");
                    var connection = new SocketClientConnection(
                                                c,
                                                ClientConnectionType.Unknown,
                                                this.services.GetService<IMessageProcessor>()!,
                                                this.services.GetService<ILogger<SocketClientConnection>>()!);
                    this.AddClient(connection);
                    c.Index = ++nextClient;
                    connections.Add(c.Index, connection);
                };
                this.socketListener.ClientDisconnected += (_, c) =>
                {
                    this.logger.LogInformation($"Client disconnected from {c.RemoteEndPoint}");
                    var connection = connections[c.Index];
                    this.RemoveClient(connection);
                    connection.Dispose();
                };
                new Task(
                    () => this.socketListener.Start(),
                    TaskCreationOptions.LongRunning).Start();
            }
            catch (Exception error)
            {
                this.logger.LogError(error, "An error occurred during socket initialisation");
            }
        }
    }
}