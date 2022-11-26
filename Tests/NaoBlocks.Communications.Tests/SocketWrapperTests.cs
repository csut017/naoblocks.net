using System.Net.Sockets;
using Xunit;

namespace NaoBlocks.Communications.Tests
{
    public class SocketWrapperTests
    {
        [Fact]
        public void WrapFailsOnNonSocket()
        {
            var error = Assert.Throws<ArgumentException>(() => { SocketWrapper.Wrap("failure"); });
            Assert.Equal("Unable to wrap socket: have you passed the correct type (Socket or ISocket)", error.Message);
        }

        [Fact]
        public void WrapHandlesRawSockets()
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var wrapped = SocketWrapper.Wrap(socket);
            Assert.NotNull(wrapped);
        }

        [Fact]
        public void WrapHandlesWrappedSockets()
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var wrapped = SocketWrapper.Wrap(socket);
            var second = SocketWrapper.Wrap(wrapped);
            Assert.Same(wrapped, second);
        }
    }
}