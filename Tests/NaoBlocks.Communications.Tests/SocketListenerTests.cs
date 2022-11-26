using Moq;
using NaoBlocks.Common;
using System.Net;
using System.Net.Sockets;
using Xunit;

namespace NaoBlocks.Communications.Tests
{
    public class SocketListenerTests
    {
        [Fact]
        public void CloseClosesUnderlyingSocket()
        {
            // Arrange
            var factory = new Mock<ISocketFactory>();
            var socket = new Mock<ISocket>();
            var manualEvent = new ManualResetEvent(false);
            socket.Setup(s => s.Close())
                .Callback(() => manualEvent.Set());
            socket.Setup(s => s.BeginAccept(It.IsAny<AsyncCallback>(), It.IsAny<ISocket>()))
                .Callback<AsyncCallback, ISocket>((callback, _) =>
                {
                    var result = new Mock<IAsyncResult>();
                    result.SetupGet(x => x.IsCompleted)
                    .Returns(false);
                    manualEvent.WaitOne();
                    callback(result.Object);
                });
            factory.Setup(f => f.New(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                .Returns(socket.Object);
            var listener = new SocketListener(new IPEndPoint(IPAddress.Loopback, 80), factory.Object);
            Task.Factory.StartNew(listener.Start, TaskCreationOptions.LongRunning);
            WaitForStartUp(listener);

            // Act
            listener.Close();

            // Assert
            socket.Verify(s => s.Close(), Times.Once);
        }

        [Fact]
        public void CloseHandlesNonStartedSOcket()
        {
            // Arrange
            var factory = new Mock<ISocketFactory>();
            var socket = new Mock<ISocket>();
            factory.Setup(f => f.New(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                .Returns(socket.Object);
            var listener = new SocketListener(new IPEndPoint(IPAddress.Loopback, 80), factory.Object);

            // Act
            listener.Close();

            // Assert
            socket.Verify(s => s.Close(), Times.Never);
        }

        [Fact]
        public void DisposeClosesUnderlyingSocket()
        {
            // Arrange
            var factory = new Mock<ISocketFactory>();
            var socket = new Mock<ISocket>();
            var manualEvent = new ManualResetEvent(false);
            socket.Setup(s => s.Close())
                .Callback(() => manualEvent.Set());
            socket.Setup(s => s.BeginAccept(It.IsAny<AsyncCallback>(), It.IsAny<ISocket>()))
                .Callback<AsyncCallback, ISocket>((callback, _) =>
                {
                    var result = new Mock<IAsyncResult>();
                    result.SetupGet(x => x.IsCompleted)
                    .Returns(false);
                    manualEvent.WaitOne();
                    callback(result.Object);
                });
            factory.Setup(f => f.New(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                .Returns(socket.Object);
            var listener = new SocketListener(new IPEndPoint(IPAddress.Loopback, 80), factory.Object);
            Task.Factory.StartNew(listener.Start, TaskCreationOptions.LongRunning);
            WaitForStartUp(listener);

            // Act
            listener.Dispose();

            // Assert
            socket.Verify(s => s.Close(), Times.Once);
        }

        [Fact]
        public void SocketListenerHandlesIncomingConnection()
        {
            // Arrange
            var endPoint = new IPEndPoint(IPAddress.Loopback, 5000);
            using var listener = new SocketListener(endPoint, new DefaultSocketFactory());
            var manualEvent = new ManualResetEvent(false);
            listener.ClientConnected += (sender, e) => { manualEvent.Set(); };
            Task.Factory.StartNew(listener.Start, TaskCreationOptions.LongRunning);
            WaitForStartUp(listener);

            // Act
            var socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(endPoint);

            // Assert
            Assert.True(manualEvent.WaitOne(TimeSpan.FromSeconds(1)), "Client has not connected");

            // Clean-up
            socket.Close();
        }

        [Theory]
        [InlineData(ClientMessageType.Authenticate, 8, null)]
        [InlineData(ClientMessageType.RobotDebugMessage, 800, null)]
        [InlineData(ClientMessageType.NoRobotsAvailable, 80, "one=1")]
        public async Task SocketListenerHandlesIncomingMessage(ClientMessageType messageType, int conversation, string? data)
        {
            // Arrange
            var endPoint = new IPEndPoint(IPAddress.Loopback, 5000);
            using var listener = new SocketListener(endPoint, new DefaultSocketFactory());
            var manualEvent = new ManualResetEvent(false);
            listener.ClientConnected += (sender, e) => { manualEvent.Set(); };
            ClientMessage? receivedMessage = null;
            listener.MessageReceived += (sender, e) =>
            {
                receivedMessage = e;
                manualEvent.Set();
            };
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Factory.StartNew(listener.Start, TaskCreationOptions.LongRunning);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            WaitForStartUp(listener);

            // Connect
            var socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(endPoint);
            Assert.True(manualEvent.WaitOne(TimeSpan.FromSeconds(1)), "Client has not connected");

            // Act
            var client = new Client(SocketWrapper.Wrap(socket));
            var message = new ClientMessage(messageType)
            {
                ConversationId = conversation
            };
            message.PopulateMessageData(data);
            manualEvent.Reset();
            await client.SendMessageAsync(message, TimeSpan.FromSeconds(5));
            manualEvent.WaitOne();

            // Assert
            Assert.Equal(message.Type, receivedMessage?.Type);
            Assert.Equal(message.ConversationId, receivedMessage?.ConversationId);
            Assert.Equal(data ?? string.Empty, message.GenerateDataString());

            // Clean-up
            socket.Close();
        }

        private static void WaitForStartUp(SocketListener listener)
        {
            var count = 30;
            while (!listener.IsListening && (count-- > 0))
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.1));
            }

            if (count <= 0) throw new Exception("Listener has not started within the time period");
        }
    }
}