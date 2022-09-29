using NaoBlocks.Common;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NaoBlocks.Utility.SocketHost
{
    /// <summary>
    /// Listener for socket connections.
    /// </summary>
    public class SocketListener
    {
        private readonly CancellationTokenSource tokenSource = new();

        /// <summary>
        /// Fired whenever a message is received.
        /// </summary>
        public event EventHandler<ClientMessage>? MessageReceived;

        /// <summary>
        /// Cancels listening.
        /// </summary>
        public void Cancel()
        {
            tokenSource.Cancel();
        }

        /// <summary>
        /// Starts listening on the specified port.
        /// </summary>
        /// <param name="endPoint">The end point to listen on.</param>
        public async Task StartAsync(IPEndPoint endPoint)
        {
            using Socket listener = new(
                endPoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            listener.Bind(endPoint);
            listener.Listen();
            var handler = await listener.AcceptAsync(this.tokenSource.Token);
            int messageType = 0;
            int sequenceNum = 0;
            var isInData = false;
            var receivedData = new byte[1_024];
            var dataPos = 0;
            while (handler.Connected && !this.tokenSource.IsCancellationRequested)
            {
                var buffer = new byte[1_024];
                var received = await handler.ReceiveAsync(buffer, SocketFlags.None, this.tokenSource.Token);
                if (received == 0)
                {
                    await handler.DisconnectAsync(false);
                }
                else
                {
                    for (var loop = 0; loop < received; loop++)
                    {
                        var character = buffer[loop];
                        receivedData[dataPos++] = character;
                        if (isInData)
                        {
                            if (character != 0) continue;
                            GenerateInternalMessage(messageType, receivedData, dataPos);
                            isInData = false;
                            dataPos = 0;
                        }
                        else
                        {
                            if (dataPos < 4) continue;

                            messageType = receivedData[1];
                            messageType = receivedData[0] + (messageType << 256);
                            sequenceNum = receivedData[3];
                            sequenceNum = receivedData[2] + (receivedData[3] << 256);
                            isInData = true;
                            dataPos = 0;
                        }
                    }
                }
            }
        }

        private void GenerateInternalMessage(int messageType, byte[] receivedData, int dataPos)
        {
            var message = new ClientMessage((ClientMessageType)messageType);
            if (dataPos > 0)
            {
                var values = Encoding
                    .UTF8
                    .GetString(receivedData, 0, dataPos - 1)
                    .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                foreach (var value in values)
                {
                    var equalPos = value.IndexOf('=');
                    if (equalPos < 0)
                    {
                        message.Values.Add(value, string.Empty);
                    }
                    else
                    {
                        var key = value[0..equalPos];
                        var thisValue = value[(equalPos + 1)..];
                        message.Values.Add(key, thisValue);
                    }
                }
            }

            this.MessageReceived?.Invoke(this, message);
        }
    }
}