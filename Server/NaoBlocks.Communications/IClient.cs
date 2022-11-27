using NaoBlocks.Common;
using System.Net;

namespace NaoBlocks.Communications
{
    /// <summary>
    /// A client connection.
    /// </summary>
    public interface IClient
    {
        /// <summary>
        /// Fired whenever a message is received.
        /// </summary>
        event EventHandler<ReceivedMessage>? MessageReceived;

        /// <summary>
        /// Gets the client's full name
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// Gets or sets the index of the client.
        /// </summary>
        int Index { get; set; }

        /// <summary>
        /// Gets or sets the name of the client.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets the remote endpoint.
        /// </summary>
        EndPoint? RemoteEndPoint { get; }

        /// <summary>
        /// Restarts the internal sequence number.
        /// </summary>
        void RestartSequence();

        /// <summary>
        /// Sends a message to the client.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="timeout">The timeout duration.</param>
        /// <returns>The outcome of the send.</returns>
        Task<Result<int>> SendMessageAsync(ClientMessage message, TimeSpan timeout);
    }
}