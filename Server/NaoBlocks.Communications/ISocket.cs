using System.Net;
using System.Net.Sockets;

namespace NaoBlocks.Communications
{
    /// <summary>
    /// Defines an interface for a socket.
    /// </summary>
    public interface ISocket
    {
        /// <summary>
        /// Gets the remote end point.
        /// </summary>
        EndPoint? RemoteEndPoint { get; }

        /// <summary>
        /// Begins to accept an incoming connection request.
        /// </summary>
        /// <param name="asyncCallback">The callback to call when the connection is accepted.</param>
        /// <param name="listener">The <see cref="ISocket"/> instance accepting the connection.</param>
        void BeginAccept(AsyncCallback asyncCallback, ISocket listener);

        /// <summary>
        /// Begins a receive operation.
        /// </summary>
        /// <param name="buffer">The buffer to receive into.</param>
        /// <param name="offset">The offset within the buffer.</param>
        /// <param name="size">The maximum size that can be received.</param>
        /// <param name="flags">The socket flags to use.</param>
        /// <param name="callback">The callback function to call when receiving has finished.</param>
        /// <param name="state">An optional state object.</param>
        void BeginReceive(byte[] buffer, int offset, int size, SocketFlags flags, AsyncCallback callback, object state);

        /// <summary>
        /// Binds to an end point.
        /// </summary>
        /// <param name="endPoint">The end point to bind to.</param>
        void Bind(IPEndPoint endPoint);

        /// <summary>
        /// Closes the socket.
        /// </summary>
        void Close();

        /// <summary>
        /// Ends an accept call.
        /// </summary>
        /// <param name="result">The <see cref="IAsyncResult"/> containing the details.</param>
        /// <returns>A <see cref="ISocket"/> instance.</returns>
        ISocket EndAccept(IAsyncResult result);

        /// <summary>
        /// Ends a receive operation.
        /// </summary>
        /// <param name="result">The result to wait on.</param>
        /// <returns>The number of bytes received.</returns>
        int EndReceive(IAsyncResult result);

        /// <summary>
        /// Starts listening on the socket.
        /// </summary>
        void Listen();

        /// <summary>
        /// Send some data over the socket.
        /// </summary>
        /// <param name="buffer">The buffer containing the data to send.</param>
        /// <param name="offset">The starting offset in the buffer.</param>
        /// <param name="size">The amount of data to use.</param>
        /// <param name="socketFlags">The flags to use.</param>
        /// <param name="timeout">The timeout period.</param>
        /// <returns>The result of the send.</returns>
        Task<int> SendAync(byte[] buffer, int offset, int size, SocketFlags socketFlags, TimeSpan timeout);
    }
}