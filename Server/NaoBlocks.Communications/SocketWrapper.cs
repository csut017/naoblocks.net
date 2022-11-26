using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;

namespace NaoBlocks.Communications
{
    /// <summary>
    /// Wraps a <see cref="Socket"/> instance.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class SocketWrapper
        : ISocket
    {
        private readonly Socket socket;

        private SocketWrapper(Socket socket)
        {
            this.socket = socket;
        }

        /// <summary>
        /// Gets the remote end point.
        /// </summary>
        public EndPoint? RemoteEndPoint
        {
            get
            {
                return this.socket.RemoteEndPoint;
            }
        }

        /// <summary>
        /// Wraps a <see cref="Socket"/> instance.
        /// </summary>
        public static ISocket Wrap(Socket socket)
        {
            return new SocketWrapper(socket);
        }

        /// <summary>
        /// Send some data over the socket.
        /// </summary>
        /// <param name="buffer">The buffer containing the data to send.</param>
        /// <param name="offset">The starting offset in the buffer.</param>
        /// <param name="size">The amount of data to use.</param>
        /// <param name="socketFlags">The flags to use.</param>
        /// <param name="timeout">The timeout period.</param>
        /// <returns>The result of the send.</returns>
        public async Task<int> SendAync(byte[] buffer, int offset, int size, SocketFlags socketFlags, TimeSpan timeout)
        {
            var result = this.socket.BeginSend(
                buffer,
                offset,
                size,
                socketFlags,
                null,
                null);
            var sendTask = Task<int>.Factory.FromAsync(
                result,
                _ => this.socket.EndSend(result));
            if (sendTask != await Task.WhenAny(sendTask, Task.Delay(timeout)).ConfigureAwait(false))
            {
                throw new TimeoutException();
            }
            return sendTask.Result;
        }
    }
}