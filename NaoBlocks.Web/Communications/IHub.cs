using NaoBlocks.Core.Models;
using System;
using System.Collections.Generic;

namespace NaoBlocks.Web.Communications
{
    public interface IHub : IDisposable
    {
        void AddClient(ClientConnection client);

        ClientConnection? GetClient(long id);

        IEnumerable<ClientConnection> GetClients(ClientConnectionType type);

        void SendToAll(ClientMessage message);

        void SendToAll(ClientMessage message, ClientConnectionType clientType);

        void SendToMonitors(ClientMessage message);

        void AddMonitor(ClientConnection client);

        void RemoveMonitor(ClientConnection client);
    }
}