using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NaoBlocks.Web.Communications
{
    public class ConnectionManager
    {
        private readonly ConcurrentDictionary<string, WebSocket> connections = new ConcurrentDictionary<string, WebSocket>();

        public bool Add(WebSocket socket, string ipAddress)
        {
            return this.connections.TryAdd(ipAddress, socket);
        }

        public async Task Remove(string ipAddress)
        {
            if (this.connections.TryRemove(ipAddress, out WebSocket socket))
            {
                await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
                socket.Dispose();
            }
        }

        public async Task SendToConnection(string ipAddress, string message)
        {
            if (this.connections.TryGetValue(ipAddress, out WebSocket socket))
            {
                if (socket.State == WebSocketState.Open)
                {
                    await socket.SendAsync(
                        new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None);
                }
                else
                {
                    throw new Exception($"WebSocket for {ipAddress} is not open");
                }
            }
            else
            {
                throw new Exception($"Socket for {ipAddress} does not exist");
            }
        }
    }
}