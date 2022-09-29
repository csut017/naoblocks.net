using NaoBlocks.Common;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NaoBlocks.Utility.SocketHost
{
    /// <summary>
    /// A client connection.
    /// </summary>
    public class Client
    {
        private readonly byte[] buffer = new byte[1024];
        private readonly Socket handler;
        private int sequence;

        /// <summary>
        /// Initialises a new <see cref="Client"/> instance.
        /// </summary>
        /// <param name="handler">The socket to the client.</param>
        public Client(Socket handler)
        {
            this.handler = handler;
            this.RemoteEndPoint = handler.RemoteEndPoint;
        }

        /// <summary>
        /// Gets the client's full name
        /// </summary>
        public string FullName
        {
            get => string.IsNullOrEmpty(this.Name)
                ? $"#{this.Index}"
                : $"{this.Name} [#${this.Index}]";
        }

        /// <summary>
        /// Gets or sets the index of the client.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the name of the client.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets the remote endpoint.
        /// </summary>
        public EndPoint? RemoteEndPoint { get; }

        /// <summary>
        /// Sends a message to the client.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="timeout">The timeout duration.</param>
        public async Task<Result<int>> SendMessageAsync(ClientMessage message, TimeSpan timeout)
        {
            int messageType = (int)message.Type;
            this.buffer[0] = (byte)(messageType & 255);
            this.buffer[1] = (byte)(messageType >> 8);
            this.buffer[2] = (byte)(this.sequence & 255);
            this.buffer[3] = (byte)(this.sequence >> 8);
            this.sequence++;

            var values = string.Join(
                ",",
                message.Values.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            var valuesBytes = Encoding.UTF8.GetBytes(values);
            valuesBytes.CopyTo(this.buffer, 4);
            var size = valuesBytes.Length + 4;
            this.buffer[size] = 0;

            try
            {
                var result = this.handler.BeginSend(
                    this.buffer,
                    0,
                    size + 1,
                    SocketFlags.None,
                    null,
                    null);
                var sendTask = Task<int>.Factory.FromAsync(
                    result,
                    _ => this.handler.EndSend(result));
                if (sendTask != await Task.WhenAny(sendTask, Task.Delay(timeout)).ConfigureAwait(false))
                {
                    throw new TimeoutException();
                }

                return Result.Ok(sendTask.Result);
            }
            catch (SocketException ex)
            {
                return Result.Fail<int>(
                    new Exception("SendMessageAsync failed", ex));
            }
            catch (TimeoutException)
            {
                return Result.Fail<int>(
                    new Exception("SendMessageAsync timed out"));
            }
        }
    }
}