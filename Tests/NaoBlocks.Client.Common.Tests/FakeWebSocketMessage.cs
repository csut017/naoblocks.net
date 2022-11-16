using System;
using System.Net.WebSockets;

namespace NaoBlocks.Client.Common.Tests
{
    public class FakeWebSocketMessage
    {
        public WebSocketMessageType MessageType { get; set; }

        public ArraySegment<byte> Buffer { get; set; }

        public WebSocketState State { get; set; }
    }
}
