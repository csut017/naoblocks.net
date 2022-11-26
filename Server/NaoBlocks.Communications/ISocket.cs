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