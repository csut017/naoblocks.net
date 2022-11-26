using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;

namespace NaoBlocks.Communications
{
    /// <summary>
    /// Wraps a <see cref="Socket"/> instance.
    /// </summary>
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
        [ExcludeFromCodeCoverage]
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
        /// <param name="socket">The object to wrap.</param>
        /// <returns>An <see cref="ISocket"/> instance.</returns>
        public static ISocket Wrap(object socket)
        {
            if (socket is ISocket safeSocket) return safeSocket;
            if (socket is Socket rawSocket) return new SocketWrapper(rawSocket);
            throw new ArgumentException("Unable to wrap socket: have you passed the correct type (Socket or ISocket)");
        }

        /// <summary>
        /// Begins to accept an incoming connection request.
        /// </summary>
        /// <param name="asyncCallback">The callback to call when the connection is accepted.</param>
        /// <param name="listener">The <see cref="ISocket"/> instance accepting the connection.</param>
        [ExcludeFromCodeCoverage]
        public void BeginAccept(AsyncCallback asyncCallback, ISocket listener)
        {
            this.socket.BeginAccept(asyncCallback, listener);
        }

        /// <summary>
        /// Begins a receive operation.
        /// </summary>
        /// <param name="buffer">The buffer to receive into.</param>
        /// <param name="offset">The offset within the buffer.</param>
        /// <param name="size">The maximum size that can be received.</param>
        /// <param name="flags">The socket flags to use.</param>
        /// <param name="callback">The callback function to call when receiving has finished.</param>
        /// <param name="state">An optional state object.</param>
        [ExcludeFromCodeCoverage]
        public void BeginReceive(byte[] buffer, int offset, int size, SocketFlags flags, AsyncCallback callback, object state)
        {
            this.socket.BeginReceive(buffer, offset, size, flags, callback, state);
        }

        /// <summary>
        /// Binds to an end point.
        /// </summary>
        /// <param name="endPoint">The end point to bind to.</param>
        [ExcludeFromCodeCoverage]
        public void Bind(IPEndPoint endPoint)
        {
            this.socket.Bind(endPoint);
        }

        /// <summary>
        /// Closes the socket.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public void Close()
        {
            this.socket.Close();
        }

        /// <summary>
        /// Ends an accept call.
        /// </summary>
        /// <param name="result">The <see cref="IAsyncResult"/> containing the details.</param>
        /// <returns>A <see cref="ISocket"/> instance.</returns>
        [ExcludeFromCodeCoverage]
        public ISocket EndAccept(IAsyncResult result)
        {
            return new SocketWrapper(this.socket.EndAccept(result));
        }

        /// <summary>
        /// Ends a receive operation.
        /// </summary>
        /// <param name="result">The result to wait on.</param>
        /// <returns>The number of bytes received.</returns>
        [ExcludeFromCodeCoverage]
        public int EndReceive(IAsyncResult result)
        {
            return this.socket.EndReceive(result);
        }

        /// <summary>
        /// Starts listening on the socket.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public void Listen()
        {
            this.socket.Listen();
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
        [ExcludeFromCodeCoverage]
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