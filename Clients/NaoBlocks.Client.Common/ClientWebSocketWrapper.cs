using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace NaoBlocks.Client.Common
{
    /// <summary>
    /// Internal wrapper for a <see cref="ClientWebSocket"/> instance.
    /// </summary>
    public class ClientWebSocketWrapper
        : IWebSocket
    {
        private readonly ClientWebSocket inner;

        /// <summary>
        /// Initialises a new <see cref="ClientWebSocketWrapper"/> instance.
        /// </summary>
        /// <param name="inner">The <see cref="ClientWebSocket"/> to wrap.</param>
        public ClientWebSocketWrapper(ClientWebSocket inner)
        {
            this.inner = inner;
        }

        /// <summary>
        /// Gets the set of the socket.
        /// </summary>
        public WebSocketState State
        {
            get { return inner.State; }
        }

        /// <summary>
        /// Connect to a WebSocket server.
        /// </summary>
        /// <param name="uri">The URI of the websocket server.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> instance.</param>
        public async Task ConnectAsync(Uri uri, CancellationToken cancellationToken)
        {
            await this.inner.ConnectAsync(uri, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Disposes any resources used by the socket client.
        /// </summary>
        public void Dispose()
        {
            this.inner.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Close the websocket.
        /// </summary>
        /// <param name="closeStatus">The WebSocket close status.</param>
        /// <param name="statusDescription">A description of the close status.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> instance.</param>
        public async Task CloseAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken)
        {
            await this.inner.CloseAsync(closeStatus, statusDescription, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Close the websocket output.
        /// </summary>
        /// <param name="closeStatus">The WebSocket close status.</param>
        /// <param name="statusDescription">A description of the close status.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> instance.</param>
        public async Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken)
        {
            await this.inner.CloseOutputAsync(closeStatus, statusDescription, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Attempts to retrieve some values from the socket.
        /// </summary>
        /// <param name="buffer">The buffer to retrieve the values into.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> instance.</param>
        /// <returns>The result of retrieving.</returns>
        public async Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
        {
            return await this.inner.ReceiveAsync(buffer, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends some data to the socket.
        /// </summary>
        /// <param name="buffer">The buffer containing the data.</param>
        /// <param name="messageType">The type of message being sent.</param>
        /// <param name="endOfMessage">Whether this is the end of the message.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> instance.</param>
        public async Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            await this.inner.SendAsync(buffer, messageType, endOfMessage, cancellationToken).ConfigureAwait(false);
        }
    }
}
