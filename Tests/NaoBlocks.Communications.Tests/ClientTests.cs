using Moq;
using NaoBlocks.Common;
using System.Net.Sockets;
using System.Text;
using Xunit;

namespace NaoBlocks.Communications.Tests
{
    public class ClientTests
    {
        [Theory]
        [InlineData(1, "", "#1")]
        [InlineData(2, "Mihīni", "Mihīni [#2]")]
        public void FullNameHandlesName(int index, string name, string expected)
        {
            // Arrange
            var socketMock = new Mock<ISocket>();
            var client = new Client(socketMock.Object)
            {
                Index = index,
                Name = name
            };

            // Act and assert
            Assert.Equal(expected, client.FullName);
        }

        [Theory]
        [InlineData(ClientMessageType.RobotAllocated, 1, null, "0c000000010000")]
        [InlineData(ClientMessageType.ClientAdded, 2, null, "4e040000020000")]
        [InlineData(ClientMessageType.Authenticated, 531, null, "02000000130200")]
        [InlineData(ClientMessageType.DownloadProgram, 3, "one=1", "1600000003006f6e653d3100")]
        [InlineData(ClientMessageType.Error, 512, "one=1,two=2", "e803000000026f6e653d312c74776f3d3200")]
        public async Task SendMessageAsyncEncodesMessage(ClientMessageType messageType, int conversation, string? data, string expected)
        {
            // Arrange
            var resultMock = new Mock<IAsyncResult>();
            var socketMock = new Mock<ISocket>();
            var buffer = string.Empty;
            socketMock.Setup(s => s.SendAync(
                    It.IsAny<byte[]>(),
                    0,
                    It.IsAny<int>(),
                    SocketFlags.None,
                    It.IsAny<TimeSpan>()))
                .Callback<byte[], int, int, SocketFlags, TimeSpan>((buf, offset, length, _, _) => buffer = ConvertToByteString(buf, offset, length))
                .Returns(Task.FromResult(5));
            var client = new Client(socketMock.Object);
            client.RestartSequence();
            var message = new ClientMessage(messageType)
            {
                ConversationId = conversation
            };
            message.PopulateMessageData(data);

            // Act
            var result = await client.SendMessageAsync(message, TimeSpan.FromSeconds(5));

            // Assert
            Assert.Equal(expected, buffer);
        }

        [Fact]
        public async Task SendMessageAsyncHandlesSocketError()
        {
            // Arrange
            var resultMock = new Mock<IAsyncResult>();
            var socketMock = new Mock<ISocket>();
            socketMock.Setup(s => s.SendAync(
                    It.IsAny<byte[]>(),
                    0,
                    It.IsAny<int>(),
                    SocketFlags.None,
                    It.IsAny<TimeSpan>()))
                .Throws(new SocketException());
            var client = new Client(socketMock.Object);
            var message = new ClientMessage(ClientMessageType.Authenticate);

            // Act
            var result = await client.SendMessageAsync(message, TimeSpan.FromSeconds(5));

            // Assert
            Assert.False(result.Success, "SendMessageAsync() was succesful");
            Assert.Equal("SendMessageAsync failed", result.Error?.Message);
        }

        [Fact]
        public async Task SendMessageAsyncHandlesTimeout()
        {
            // Arrange
            var resultMock = new Mock<IAsyncResult>();
            var socketMock = new Mock<ISocket>();
            socketMock.Setup(s => s.SendAync(
                    It.IsAny<byte[]>(),
                    0,
                    It.IsAny<int>(),
                    SocketFlags.None,
                    It.IsAny<TimeSpan>()))
                .Throws(new TimeoutException());
            var client = new Client(socketMock.Object);
            var message = new ClientMessage(ClientMessageType.Authenticate);

            // Act
            var result = await client.SendMessageAsync(message, TimeSpan.FromSeconds(5));

            // Assert
            Assert.False(result.Success, "SendMessageAsync() was succesful");
            Assert.Equal("SendMessageAsync timed out", result.Error?.Message);
        }

        [Fact]
        public async Task SendMessageAsyncIncrementsSequenceNumber()
        {
            // Arrange
            var resultMock = new Mock<IAsyncResult>();
            var socketMock = new Mock<ISocket>();
            var buffer = string.Empty;
            socketMock.Setup(s => s.SendAync(
                    It.IsAny<byte[]>(),
                    0,
                    It.IsAny<int>(),
                    SocketFlags.None,
                    It.IsAny<TimeSpan>()))
                .Callback<byte[], int, int, SocketFlags, TimeSpan>((buf, offset, length, _, _) => buffer = ConvertToByteString(buf, offset, length))
                .Returns(Task.FromResult(5));
            var client = new Client(socketMock.Object);
            client.RestartSequence();
            var message = new ClientMessage(ClientMessageType.RobotDebugMessage);

            // Act
            var sequence = new List<string>();
            await client.SendMessageAsync(message, TimeSpan.FromSeconds(5));
            sequence.Add(buffer.Substring(4, 2));
            await client.SendMessageAsync(message, TimeSpan.FromSeconds(5));
            sequence.Add(buffer.Substring(4, 2));
            await client.SendMessageAsync(message, TimeSpan.FromSeconds(5));
            sequence.Add(buffer.Substring(4, 2));

            // Assert
            Assert.Equal(
                new[] { "00", "01", "02" },
                sequence.ToArray());
        }

        [Fact]
        public async Task SendMessageAsyncWorks()
        {
            // Arrange
            var resultMock = new Mock<IAsyncResult>();
            var socketMock = new Mock<ISocket>();
            socketMock.Setup(s => s.SendAync(
                    It.IsAny<byte[]>(),
                    0,
                    It.IsAny<int>(),
                    SocketFlags.None,
                    It.IsAny<TimeSpan>()))
                .Returns(Task.FromResult(5));
            var client = new Client(socketMock.Object);
            var message = new ClientMessage(ClientMessageType.Authenticate);

            // Act
            var result = await client.SendMessageAsync(message, TimeSpan.FromSeconds(5));

            // Assert
            Assert.True(result.Success, "SendMessageAsync() was not succesful");
        }

        private static string ConvertToByteString(byte[] bytes, int start, int size)
        {
            var builder = new StringBuilder();
            for (int i = start; i < start + size; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }
}