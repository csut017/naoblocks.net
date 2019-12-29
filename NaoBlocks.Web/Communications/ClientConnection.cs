﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace NaoBlocks.Web.Communications
{
    public class ClientConnection : IDisposable
    {
        private readonly ConcurrentQueue<ClientMessage> _queue = new ConcurrentQueue<ClientMessage>();
        private readonly WebSocket _socket;

        public ClientConnection(WebSocket socket)
        {
            this._socket = socket;
        }

        public event EventHandler<EventArgs>? Closed;

        public IHub? Hub { get; set; }

        public long Id { get; set; }

        public ClientConnectionType Type { get; set; } = ClientConnectionType.Unknown;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void SendMessage(ClientMessage message)
        {
            this._queue.Enqueue(message);
        }

        public async Task StartAsync(CancellationToken cancelToken)
        {
            var pushTask = Task.Run(async () => await this.PushMessagesAsync(cancelToken));
            try
            {
                while (true)
                {
                    var (response, message) = await this.ReceiveFullMessageAsync(cancelToken);
                    if (response.MessageType == WebSocketMessageType.Close)
                        break;
                }
                await this._socket.CloseAsync(WebSocketCloseStatus.Empty, string.Empty, CancellationToken.None);
                this.Closed?.Invoke(this, EventArgs.Empty);
            }
            catch (WebSocketException ex)
            {
                switch (ex.WebSocketErrorCode)
                {
                    case WebSocketError.ConnectionClosedPrematurely:
                        this.Closed?.Invoke(this, EventArgs.Empty);
                        break;

                    default:
                        throw;
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._socket.Dispose();
            }
        }

        private async Task PushMessagesAsync(CancellationToken cancelToken)
        {
            while (!cancelToken.IsCancellationRequested)
            {
                if (this._queue.TryDequeue(out ClientMessage? message))
                {
                    if (message != null)
                    {
                        await this._socket.SendAsync(message.ToArray(), WebSocketMessageType.Text, true, cancelToken);
                    }
                }
                else
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
            }
        }

        private async Task<(WebSocketReceiveResult, IEnumerable<byte>)> ReceiveFullMessageAsync(CancellationToken cancelToken)
        {
            WebSocketReceiveResult response;
            var message = new List<byte>();

            var buffer = new byte[4096];
            do
            {
                response = await this._socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancelToken);
                message.AddRange(new ArraySegment<byte>(buffer, 0, response.Count));
            } while (!response.EndOfMessage);

            return (response, message);
        }
    }
}