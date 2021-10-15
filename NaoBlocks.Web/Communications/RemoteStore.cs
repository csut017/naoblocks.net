using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NaoBlocks.Client;
using NaoBlocks.Common;
using NaoBlocks.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NaoBlocks.Web.Communications
{
    public sealed class RemoteStore : IRemoteStore, IDisposable
    {
        private readonly IDictionary<string, ClientWrapper> connections = new Dictionary<string, ClientWrapper>();
        private readonly ILogger<RemoteStore> logger;
        private readonly IOptions<AppSettings> appSettings;

        public RemoteStore(ILogger<RemoteStore> logger, IOptions<AppSettings> appSettings)
        {
            this.logger = logger;
            this.appSettings = appSettings;
        }

        public string CheckRemote(string robot)
        {
            if (!this.connections.TryGetValue(robot, out ClientWrapper? client))
            {
                return "FAIL";
            }

            var msg = client.GetMessage(new TimeSpan(0, 0, 5));
            if (msg == null)
            {
                return "PASS";
            }

            return "TODO";
        }

        public void Dispose()
        {
            foreach (var client in this.connections.Values)
            {
                client.Dispose();
            }
        }

        public async Task<string> StartAsync(string robot, string password)
        {
            if (this.connections.ContainsKey(robot))
            {
                return "FAIL";
            }

            var settings = this.appSettings.Value;
            var client = new Connection(settings.InternalAddress, password, true, robot);
            try
            {
                await client.ConnectAsync();
                this.connections.Add(robot, new ClientWrapper(client));
                return client.ServerVersion;
            }
            catch (Exception error)
            {
                this.logger.LogInformation($"Unable to connect {robot}: {error.Message}");
                client.Dispose();
                return "FAIL";
            }
        }

        public async Task<string> StopAsync(string robot)
        {
            if (!this.connections.TryGetValue(robot, out ClientWrapper? client))
            {
                return "OK";
            }

            await client.Client.DisconnectAsync();
            client.Dispose();
            this.connections.Remove(robot);
            return "OK";
        }

        private sealed class ClientWrapper : IDisposable
        {
            private readonly Queue<ClientMessage> messages = new Queue<ClientMessage>();
            private readonly AutoResetEvent waitHandle = new AutoResetEvent(false);

            public ClientWrapper(Connection client)
            {
                this.Client = client;
                this.Client.OnMessageReceived.Subscribe(msg =>
                {
                    this.messages.Enqueue(msg);
                    this.waitHandle.Set();
                });
            }

            public Connection Client { get; }

            public ClientMessage? GetMessage(TimeSpan timeout)
            {
                this.waitHandle.WaitOne(timeout);
                this.waitHandle.Reset();
                return this.messages.TryDequeue(out ClientMessage? msg) ? msg : null;
            }

            public void Dispose()
            {
                this.waitHandle.Dispose();
                this.Client.Dispose();
            }
        }
    }
}
