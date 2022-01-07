using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace NaoBlocks.Client.Common.Tests
{
    public class FakeWebSocketClient
        : IWebSocket
    {
        public List<FakeWebSocketMessage> IncomingMessages { get; } = new List<FakeWebSocketMessage>();

        public ConcurrentQueue<FakeWebSocketMessage> OutgoingMessages { get; set; } = new ConcurrentQueue<FakeWebSocketMessage>();

        public WebSocketState State { get; set; }

        public string Address { get; private set; } = string.Empty;

        public bool IsClosed { get; private set; }

        public bool IsOutputClosed { get; private set; }

        public bool IsDisposed { get; private set; }

        public Task CloseAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken)
        {
            this.IsClosed = true;
            return Task.CompletedTask;
        }

        public Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken)
        {
            this.IsOutputClosed = true;
            return Task.CompletedTask;
        }

        public Task ConnectAsync(Uri uri, CancellationToken cancellationToken)
        {
            this.Address = uri.ToString();
            this.State = WebSocketState.Open;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            this.IsDisposed = true;
            GC.SuppressFinalize(this);
        }

        public Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!this.OutgoingMessages.TryDequeue(out var message))
                {
                    Task.Delay(100, cancellationToken);
                    continue;
                }

                message.Buffer.CopyTo(buffer);
                this.State = message.State;
                return Task.FromResult(
                    new WebSocketReceiveResult(0, message.MessageType, true));
            }

            return Task.FromResult(
                new WebSocketReceiveResult(0, WebSocketMessageType.Close, true, WebSocketCloseStatus.NormalClosure, string.Empty));
        }

        public Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            this.IncomingMessages.Add(new FakeWebSocketMessage
            {
                Buffer = buffer,
                MessageType = messageType
            });
            return Task.CompletedTask;
        }
    }
}