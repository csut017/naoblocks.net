using NaoBlocks.Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace NaoBlocks.Web.Communications
{
    public sealed class Hub : IHub
    {
        private readonly IDictionary<long, ClientConnection> _clients = new Dictionary<long, ClientConnection>();
        private readonly ReaderWriterLockSlim _clientsLock = new ReaderWriterLockSlim();
        private long _nextClientId;

        private readonly HashSet<ClientConnection> _monitors = new HashSet<ClientConnection>();
        private readonly ReaderWriterLockSlim _monitorLock = new ReaderWriterLockSlim();

        public void AddClient(ClientConnection? client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            var isLocked = false;
            try
            {
                isLocked = this._clientsLock.TryEnterWriteLock(TimeSpan.FromSeconds(5));
                if (isLocked)
                {
                    client.Id = this._nextClientId++;
                    client.Hub = this;
                    this._clients.Add(client.Id, client);
                    client.Closed += (o, e) => this.RemoveClient(client);
                }
                else
                {
                    throw new TimeoutException("Unable to add client");
                }
            }
            finally
            {
                if (isLocked) this._clientsLock.ExitWriteLock();
            }
        }

        private void RemoveClient(ClientConnection? client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            var isLocked = false;
            try
            {
                isLocked = this._clientsLock.TryEnterWriteLock(TimeSpan.FromSeconds(5));
                if (isLocked)
                {
                    client.Hub = null;
                    this._clients.Remove(client.Id);

                    var msg = new ClientMessage
                    {
                        Type = ClientMessageType.ClientRemoved
                    };
                    msg.Values["ClientId"] = client.Id.ToString(CultureInfo.InvariantCulture);
                    this.SendToMonitors(msg);
                }
                else
                {
                    throw new TimeoutException("Unable to add client");
                }
            }
            finally
            {
                if (isLocked) this._clientsLock.ExitWriteLock();
            }
        }

        public void AddMonitor(ClientConnection client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));

            var isLocked = false;
            try
            {
                isLocked = this._monitorLock.TryEnterWriteLock(TimeSpan.FromSeconds(5));
                if (isLocked)
                {
                    this._monitors.Add(client);
                    client.Closed += (o, e) => this.RemoveMonitor(client);
                }
                else
                {
                    throw new TimeoutException("Unable to add monitor");
                }
            }
            finally
            {
                if (isLocked) this._monitorLock.ExitWriteLock();
            }

            this._clientsLock.EnterReadLock();
            try
            {
                foreach (var existing in this._clients.Values)
                {
                    var msg = new ClientMessage
                    {
                        Type = ClientMessageType.ClientAdded
                    };
                    msg.Values["ClientId"] = existing.Id.ToString(CultureInfo.InvariantCulture);
                    msg.Values["Type"] = "Unknown";
                    if (existing.Type == ClientConnectionType.Robot)
                    {
                        msg.Values["Type"] = "robot";
                        msg.Values["SubType"] = existing?.Robot?.Type?.Name ?? "Unknown";
                        msg.Values["Name"] = existing?.Robot?.FriendlyName ?? "Unknown";
                        msg.Values["state"] = existing?.Status?.Message ?? "Unknown";
                    }
                    else
                    {
                        msg.Values["Type"] = "user";
                        msg.Values["SubType"] = existing?.User?.Role.ToString() ?? "Unknown";
                        msg.Values["Name"] = existing?.User?.Name ?? "Unknown";
                    }

                    client.SendMessage(msg);
                }
            }
            finally
            {
                this._clientsLock.ExitReadLock();
            }
        }

        public void Dispose()
        {
            this._clientsLock.Dispose();
        }

        public ClientConnection? GetClient(long id)
        {
            this._clientsLock.EnterReadLock();
            try
            {
                return this._clients.TryGetValue(id, out ClientConnection? client) ? client : null;
            }
            finally
            {
                this._clientsLock.ExitReadLock();
            }
        }

        public IEnumerable<ClientConnection> GetClients(ClientConnectionType type)
        {
            this._clientsLock.EnterReadLock();
            try
            {
                var clients = this._clients
                    .Where(c => c.Value.Type == type)
                    .Select(c => c.Value)
                    .ToArray();
                return clients;
            }
            finally
            {
                this._clientsLock.ExitReadLock();
            }
        }

        public void RemoveMonitor(ClientConnection client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));

            var isLocked = false;
            try
            {
                isLocked = this._monitorLock.TryEnterWriteLock(TimeSpan.FromSeconds(5));
                if (isLocked)
                {
                    if (this._monitors.Contains(client)) this._monitors.Remove(client);
                }
                else
                {
                    throw new TimeoutException("Unable to remove monitor");
                }
            }
            finally
            {
                if (isLocked) this._monitorLock.ExitWriteLock();
            }
        }

        public void SendToAll(ClientMessage message)
        {
            this._clientsLock.EnterReadLock();
            try
            {
                this.SendToAllInternal(message, this._clients.Values);
            }
            finally
            {
                this._clientsLock.ExitReadLock();
            }
        }

        public void SendToAll(ClientMessage message, ClientConnectionType clientType)
        {
            this._clientsLock.EnterReadLock();
            try
            {
                this.SendToAllInternal(message, this._clients.Values.Where(c => c.Type == clientType));
            }
            finally
            {
                this._clientsLock.ExitReadLock();
            }
        }

        public void SendToMonitors(ClientMessage message)
        {
            this._monitorLock.EnterReadLock();
            try
            {
                this.SendToAllInternal(message, this._monitors);
            }
            finally
            {
                this._monitorLock.ExitReadLock();
            }
        }

        private void SendToAllInternal(ClientMessage message, IEnumerable<ClientConnection> clients)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            foreach (var client in clients)
            {
                client.SendMessage(message);
            }
        }
    }
}