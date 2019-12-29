﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NaoBlocks.Web.Communications
{
    public class Hub : IHub
    {
        private readonly IDictionary<long, ClientConnection> _clients = new Dictionary<long, ClientConnection>();
        private readonly ReaderWriterLockSlim _clientsLock = new ReaderWriterLockSlim();
        private long _nextClientId;

        public void AddClient(ClientConnection? client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            this._clientsLock.EnterWriteLock();
            try
            {
                client.Id = this._nextClientId++;
                client.Hub = this;
                this._clients.Add(client.Id, client);
            }
            finally
            {
                this._clientsLock.ExitWriteLock();
            }

            client.Closed += (o, e) =>
            {
                this._clientsLock.EnterWriteLock();
                try
                {
                    client.Hub = null;
                    this._clients.Remove(client.Id);
                }
                finally
                {
                    this._clientsLock.ExitWriteLock();
                }
            };
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

        public void SendToAll(ClientMessage message)
        {
            this.SendToAllInternal(message, this._clients.Values);
        }

        public void SendToAll(ClientMessage message, ClientConnectionType clientType)
        {
            this.SendToAllInternal(message, this._clients.Values.Where(c => c.Type == clientType));
        }

        private void SendToAllInternal(ClientMessage message, IEnumerable<ClientConnection> clients)
        {
            this._clientsLock.EnterReadLock();
            try
            {
                foreach (var client in clients)
                {
                    client.SendMessage(message);
                }
            }
            finally
            {
                this._clientsLock.ExitReadLock();
            }
        }
    }
}