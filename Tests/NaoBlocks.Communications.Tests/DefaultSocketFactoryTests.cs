using System.Net.Sockets;
using Xunit;

namespace NaoBlocks.Communications.Tests
{
    public class DefaultSocketFactoryTests
    {
        [Fact]
        public void NewGeneratesNewWrapper()
        {
            // Arrange
            var factory = new DefaultSocketFactory();

            // Act
            var socket = factory.New(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Assert
            Assert.IsType<SocketWrapper>(socket);
        }
    }
}