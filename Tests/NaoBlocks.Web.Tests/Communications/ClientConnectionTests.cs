using Moq;
using NaoBlocks.Common;
using NaoBlocks.Web.Communications;
using System;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Web.Tests.Communications
{
    public class ClientConnectionTests
    {
        [Fact]
        public void SendMessageQueuesMessage()
        {
            // Arrange
            var socket = new Mock<WebSocket>();
            var processor = new Mock<IMessageProcessor>();
            var message = new ClientMessage();
            var client = new StandardClientConnection(socket.Object, ClientConnectionType.User, processor.Object);

            // Act
            client.SendMessage(message);

            // Assert
            Assert.NotEmpty(client.RetrievePendingMessages());
        }

        [Fact]
        public void AddListenerAddsAListener()
        {
            // Arrange
            var socket = new Mock<WebSocket>();
            var processor = new Mock<IMessageProcessor>();
            var client = new StandardClientConnection(socket.Object, ClientConnectionType.User, processor.Object);
            var listener = new StandardClientConnection(socket.Object, ClientConnectionType.User, processor.Object);

            // Act
            client.AddListener(listener);

            // Assert
            Assert.Contains(client.RetrieveListeners(), l => object.ReferenceEquals(l, listener));
        }

        [Fact]
        public void RemoveListenerRemovesAnExistingListener()
        {
            // Arrange
            var socket = new Mock<WebSocket>();
            var processor = new Mock<IMessageProcessor>();
            var client = new StandardClientConnection(socket.Object, ClientConnectionType.User, processor.Object);
            var listener = new StandardClientConnection(socket.Object, ClientConnectionType.User, processor.Object);

            // Act
            client.AddListener(listener);
            client.RemoveListener(listener);

            // Assert
            Assert.Empty(client.RetrieveListeners());
        }

        [Fact]
        public void RemoveListenerHandlesMissingListener()
        {
            // Arrange
            var socket = new Mock<WebSocket>();
            var processor = new Mock<IMessageProcessor>();
            var client = new StandardClientConnection(socket.Object, ClientConnectionType.User, processor.Object);
            var listener = new StandardClientConnection(socket.Object, ClientConnectionType.User, processor.Object);

            // Act
            client.RemoveListener(listener);

            // Assert
            Assert.Empty(client.RetrieveListeners());
        }

        [Fact]
        public void CloseFiresEventIfNotRunning()
        {
            // Arrange
            var socket = new Mock<WebSocket>();
            var processor = new Mock<IMessageProcessor>();
            var client = new StandardClientConnection(socket.Object, ClientConnectionType.User, processor.Object);
            var closeCalled = false;
            client.Closed += (o, e) => closeCalled = true;

            // Act
            Assert.False(client.IsClosing);
            client.Close();

            // Assert
            Assert.True(closeCalled);
        }

        [Fact]
        public void DisposeCleansUp()
        {
            // Arrange
            var socket = new Mock<WebSocket>();
            socket.Setup(s => s.Dispose()).Verifiable();
            var processor = new Mock<IMessageProcessor>();
            var client = new StandardClientConnection(socket.Object, ClientConnectionType.User, processor.Object);

            // Act
            client.Dispose();

            // Assert
            socket.VerifyAll();
        }

        [Fact]
        public void StartAsyncStartsProcessingLoop()
        {
            // Arrange
            var socket = new Mock<WebSocket>();
            socket.Setup(s => s.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()))
                .Returns((ArraySegment<byte> s, CancellationToken c) =>
                {
                    c.WaitHandle.WaitOne();
                    return Task.FromResult(new WebSocketReceiveResult(0, WebSocketMessageType.Close, true));
                });
            var processor = new Mock<IMessageProcessor>();
            var client = new StandardClientConnection(socket.Object, ClientConnectionType.User, processor.Object);

            // Act
            var task = Task.Run(async () => await client.StartAsync());
            Thread.Sleep(TimeSpan.FromSeconds(1));
            client.Close();

            // Assert
            Assert.True(task.Wait(TimeSpan.FromSeconds(10)));
        }

        [Fact]
        public void StartAsyncHandlesIncomingMessage()
        {
            // Arrange
            var socket = new Mock<WebSocket>();
            var message = new ClientMessage(ClientMessageType.ProgramTransferred);
            var first = true;
            socket.Setup(s => s.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()))
                .Returns((ArraySegment<byte> s, CancellationToken c) =>
                {
                    if (first)
                    {
                        first = false;
                        var data = new Memory<byte>(message.ToArray());
                        data.CopyTo(s.AsMemory());
                        return Task.FromResult(new WebSocketReceiveResult(data.Length, WebSocketMessageType.Text, true));
                    }

                    return Task.FromResult(new WebSocketReceiveResult(0, WebSocketMessageType.Text, true));
                });
            var processor = new Mock<IMessageProcessor>();
            var client = new StandardClientConnection(socket.Object, ClientConnectionType.User, processor.Object);
            processor.Setup(p => p.ProcessAsync(client, It.IsAny<ClientMessage>()))
                .Verifiable();

            // Act
            var task = Task.Run(async () => await client.StartAsync());
            Thread.Sleep(TimeSpan.FromSeconds(1));
            client.Close();

            // Assert
            Assert.True(task.Wait(TimeSpan.FromSeconds(10)));
            processor.VerifyAll();
        }

        [Fact]
        public void StartAsyncSendsMessages()
        {
            // Arrange
            var socket = new Mock<WebSocket>();
            var message = new ClientMessage(ClientMessageType.ProgramTransferred);
            socket.Setup(s => s.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()))
                .Returns((ArraySegment<byte> s, CancellationToken c) =>
                {
                    c.WaitHandle.WaitOne();
                    return Task.FromResult(new WebSocketReceiveResult(0, WebSocketMessageType.Text, true));
                });
            var processor = new Mock<IMessageProcessor>();
            var client = new StandardClientConnection(socket.Object, ClientConnectionType.User, processor.Object);

            // Act
            client.SendMessage(new ClientMessage(ClientMessageType.ProgramTransferred));
            var task = Task.Run(async () => await client.StartAsync());
            Thread.Sleep(TimeSpan.FromSeconds(1));
            client.Close();

            // Assert
            Assert.True(task.Wait(TimeSpan.FromSeconds(10)));
            socket.Verify(s => s.SendAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<WebSocketMessageType>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()));
        }

        [Fact]
        public void StartAsyncHandlesSocketClose()
        {
            // Arrange
            var socket = new Mock<WebSocket>();
            socket.Setup(s => s.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()))
                .Returns(() =>
                {
                    return Task.FromResult(new WebSocketReceiveResult(0, WebSocketMessageType.Close, true));
                });
            var processor = new Mock<IMessageProcessor>();
            var client = new StandardClientConnection(socket.Object, ClientConnectionType.User, processor.Object);

            // Act
            var task = Task.Run(async () => await client.StartAsync());
            Thread.Sleep(TimeSpan.FromSeconds(1));
            client.Close();

            // Assert
            Assert.True(task.Wait(TimeSpan.FromSeconds(10)));
            Assert.False(task.IsFaulted);
        }

        [Fact]
        public void StartAsyncHandlesCloseError()
        {
            // Arrange
            var socket = new Mock<WebSocket>();
            socket.Setup(s => s.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()))
                .Throws(new WebSocketException(WebSocketError.ConnectionClosedPrematurely));
            var processor = new Mock<IMessageProcessor>();
            var client = new StandardClientConnection(socket.Object, ClientConnectionType.User, processor.Object);

            // Act
            var task = Task.Run(async () => await client.StartAsync());

            // Assert
            Assert.True(task.Wait(TimeSpan.FromSeconds(10)));
            Assert.False(task.IsFaulted);
        }

        [Fact]
        public void StartAsyncHandlesGeneralError()
        {
            // Arrange
            var socket = new Mock<WebSocket>();
            socket.Setup(s => s.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception());
            var processor = new Mock<IMessageProcessor>();
            var client = new StandardClientConnection(socket.Object, ClientConnectionType.User, processor.Object);

            // Act
            var task = Task.Run(async () => await client.StartAsync());
            Thread.Sleep(TimeSpan.FromSeconds(1));

            // Assert
            Assert.True(task.IsFaulted);
        }

        [Fact]
        public void ClosedEventIsCalled()
        {
            // Arrange
            var socket = new Mock<WebSocket>();
            socket.Setup(s => s.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()))
                .Returns((ArraySegment<byte> s, CancellationToken c) =>
                {
                    c.WaitHandle.WaitOne();
                    return Task.FromResult(new WebSocketReceiveResult(0, WebSocketMessageType.Close, true));
                });
            var processor = new Mock<IMessageProcessor>();
            var client = new StandardClientConnection(socket.Object, ClientConnectionType.User, processor.Object);
            var closedCalled = false;
            client.Closed += (o, e) => { closedCalled = true; };

            // Act
            var task = Task.Run(async () => await client.StartAsync());
            client.Close();
            task.Wait(TimeSpan.FromSeconds(1));

            // Assert
            Assert.True(closedCalled);
        }

        [Fact]
        public void LogMessageAddsAMessage()
        {
            // Arrange
            var socket = new Mock<WebSocket>();
            var processor = new Mock<IMessageProcessor>();
            var client = new StandardClientConnection(socket.Object, ClientConnectionType.User, processor.Object);

            // Act
            var msg = new ClientMessage(ClientMessageType.Authenticate);
            client.LogMessage(msg);

            // Assert
            var msgs = Task.Run(() => client.GetMessageLogAsync()).Result;
            Assert.Equal(new[] { "Authenticate" }, msgs.Select(m => m.Type.ToString()).ToArray());
        }

        [Fact]
        public void LogMessageCapsAt100()
        {
            // Arrange
            var socket = new Mock<WebSocket>();
            var processor = new Mock<IMessageProcessor>();
            var client = new StandardClientConnection(socket.Object, ClientConnectionType.User, processor.Object);

            // Act
            for (var loop = 0; loop < 105; loop++)
            {
                var msg = new ClientMessage(ClientMessageType.Authenticate);
                client.LogMessage(msg);
            }

            // Assert
            var msgCount = Task.Run(() => client.GetMessageLogAsync()).Result.Count;
            Assert.Equal(100, msgCount);
        }

        [Fact]
        public void NotifyListenersAddsAMessageToAllListeners()
        {
            // Arrange
            var socket = new Mock<WebSocket>();
            var processor = new Mock<IMessageProcessor>();
            var client = new StandardClientConnection(socket.Object, ClientConnectionType.User, processor.Object);
            var listener1 = new StandardClientConnection(socket.Object, ClientConnectionType.User, processor.Object);
            var listener2 = new StandardClientConnection(socket.Object, ClientConnectionType.User, processor.Object);
            client.AddListener(listener1);
            client.AddListener(listener2);

            // Act
            var msg = new ClientMessage(ClientMessageType.Authenticated);
            client.NotifyListeners(msg);

            // Assert
            Assert.Contains(listener1.RetrievePendingMessages(), m => m.Type == ClientMessageType.Authenticated);
            Assert.Contains(listener2.RetrievePendingMessages(), m => m.Type == ClientMessageType.Authenticated);
        }

        [Fact]
        public void AddNotificationAddsMessage()
        {
            // Arrange
            var socket = new Mock<WebSocket>();
            var processor = new Mock<IMessageProcessor>();
            var client = new StandardClientConnection(socket.Object, ClientConnectionType.User, processor.Object);

            // Act
            var notification = new NotificationAlert();
            client.AddNotification(notification);

            // Assert
            Assert.Contains(notification, client.Notifications);
        }

        [Fact]
        public void AddNotificationLimitsNumberOfStoredNotifications()
        {
            // Arrange
            var socket = new Mock<WebSocket>();
            var processor = new Mock<IMessageProcessor>();
            var client = new StandardClientConnection(socket.Object, ClientConnectionType.User, processor.Object);

            // Act
            for (var loop = 0; loop < 30; loop++)
            {
                var notification = new NotificationAlert();
                client.AddNotification(notification);
            }

            // Assert
            var length = client.Notifications.Count;
            Assert.Equal(20, length);
        }
    }
}
