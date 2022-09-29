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
        private const int BUFFER_SIZE = 1024;
        private readonly ManualResetEvent allDone = new(false);
        private readonly CancellationTokenSource tokenSource = new();

        /// <summary>
        /// Fired when a client connects.
        /// </summary>
        public event EventHandler<Client>? ClientConnected;

        /// <summary>
        /// Fired when a client disconnects.
        /// </summary>
        public event EventHandler<Client>? ClientDisconnected;

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
        public void Start(IPEndPoint endPoint)
        {
            using Socket listener = new(
                endPoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            listener.Bind(endPoint);
            listener.Listen();
            while (true)
            {
                this.allDone.Reset();
                listener.BeginAccept(
                    new AsyncCallback(this.AcceptCallback),
                    listener);
                this.allDone.WaitOne();
            }
        }

        private void AcceptCallback(IAsyncResult result)
        {
            this.allDone.Set();
            var listener = (Socket)result.AsyncState!;
            var handler = listener.EndAccept(result);
            Client client = new(handler);
            var state = new SocketState(handler, client);
            this.ClientConnected?.Invoke(this, client);
            handler.BeginReceive(
                state.Buffer,
                0,
                BUFFER_SIZE,
                SocketFlags.None,
                new AsyncCallback(ReceiveCallback),
                state);
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

        private void ReceiveCallback(IAsyncResult result)
        {
            var state = (SocketState)result.AsyncState!;
            var handler = state.Socket;
            var received = handler.EndReceive(result);
            if (received == 0)
            {
                handler.Close();
                this.ClientDisconnected?.Invoke(this, state.Client);
                return;
            }

            for (var loop = 0; loop < received; loop++)
            {
                var character = state.Buffer[loop];
                state.MessageData[state.DataPosition++] = character;
                if (state.IsInDataSegment)
                {
                    if (character != 0) continue;
                    GenerateInternalMessage(state.MessageType, state.MessageData, state.DataPosition);
                    state.IsInDataSegment = false;
                    state.DataPosition = 0;
                }
                else
                {
                    if (state.DataPosition < 4) continue;

                    state.MessageType = state.MessageData[1];
                    state.MessageType = state.MessageData[0] + (state.MessageType << 256);
                    state.SequenceNumber = state.MessageData[3];
                    state.SequenceNumber = state.MessageData[2] + (state.SequenceNumber << 256);
                    state.IsInDataSegment = true;
                    state.DataPosition = 0;
                }
            }

            handler.BeginReceive(
                state.Buffer,
                0,
                BUFFER_SIZE,
                SocketFlags.None,
                new AsyncCallback(ReceiveCallback),
                state);
        }

        private class SocketState
        {
            public SocketState(Socket socket, Client client)
            {
                this.Socket = socket;
                this.Client = client;
            }

            public byte[] Buffer { get; } = new byte[BUFFER_SIZE];

            public Client Client { get; }

            public int DataPosition { get; set; }

            public bool IsInDataSegment { get; set; }

            public byte[] MessageData { get; } = new byte[BUFFER_SIZE];

            public int MessageType { get; set; }

            public int SequenceNumber { get; set; }

            public Socket Socket { get; }
        }
    }
}