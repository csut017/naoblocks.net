using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace NaoBlocks.Client.Common
{
    /// <summary>
    /// Abstracts a websocket.
    /// </summary>
    public interface IWebSocket: IDisposable
    {
        /// <summary>
        /// Connect to a WebSocket server.
        /// </summary>
        /// <param name="uri">The URI of the websocket server.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> instance.</param>
        Task ConnectAsync(Uri uri, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the set of the socket.
        /// </summary>
        WebSocketState State { get; }

        /// <summary>
        /// Close the websocket.
        /// </summary>
        /// <param name="closeStatus">The WebSocket close status.</param>
        /// <param name="statusDescription">A description of the close status.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> instance.</param>
        Task CloseAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken);

        /// <summary>
        /// Close the websocket output.
        /// </summary>
        /// <param name="closeStatus">The WebSocket close status.</param>
        /// <param name="statusDescription">A description of the close status.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> instance.</param>
        Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken);

        /// <summary>
        /// Attempts to retrieve some values from the socket.
        /// </summary>
        /// <param name="buffer">The buffer to retrieve the values into.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> instance.</param>
        /// <returns>The result of retrieving.</returns>
        Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken);

        /// <summary>
        /// Sends some data to the socket.
        /// </summary>
        /// <param name="buffer">The buffer containing the data.</param>
        /// <param name="messageType">The type of message being sent.</param>
        /// <param name="endOfMessage">Whether this is the end of the message.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> instance.</param>
        Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken);
    }
}
