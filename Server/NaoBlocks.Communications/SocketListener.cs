using NaoBlocks.Common;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NaoBlocks.Communications
{
    /// <summary>
    /// Listener for socket connections.
    /// </summary>
    public class SocketListener
        : IDisposable
    {
        private const int BUFFER_SIZE = 1024;
        private readonly ManualResetEvent allDone = new(false);
        private readonly IPEndPoint endPoint;
        private readonly Socket listener;
        private bool disposedValue;
        private bool isOpen;

        /// <summary>
        /// Initialises a new <see cref="SocketListener"/> instance.
        /// </summary>
        /// <param name="endPoint">The end point to listen on.</param>
        public SocketListener(IPEndPoint endPoint)
        {
            this.listener = new(
                endPoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);
            this.endPoint = endPoint;
        }

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
        public event EventHandler<ReceivedMessage>? MessageReceived;

        /// <summary>
        /// Closes the socket.
        /// </summary>
        public void Close()
        {
            if (!this.isOpen) return;
            this.isOpen = false;
            this.listener.Close();
            this.allDone.WaitOne();
        }

        /// <summary>
        /// Cleans up the resources.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Starts listening on the specified port.
        /// </summary>
        public void Start()
        {
            this.listener.Bind(this.endPoint);
            this.listener.Listen();
            this.isOpen = true;
            while (this.isOpen)
            {
                this.allDone.Reset();
                listener.BeginAccept(
                    new AsyncCallback(this.AcceptCallback),
                    listener);
                this.allDone.WaitOne();
            }
        }

        /// <summary>
        /// Cleans up the resources.
        /// </summary>
        /// <param name="disposing">Are we in disposing mode or not.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.Close();
                }

                disposedValue = true;
            }
        }

        private void AcceptCallback(IAsyncResult result)
        {
            this.allDone.Set();
            if (!this.isOpen) return;

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

        private void GenerateInternalMessage(SocketState state)
        {
            var message = new ReceivedMessage(state.Client, (ClientMessageType)state.MessageType);
            if (state.DataPosition > 0)
            {
                var values = Encoding
                    .UTF8
                    .GetString(state.MessageData, 0, state.DataPosition - 1)
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
            state.Client.FireMessageReceived(this, message);
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            var state = (SocketState)result.AsyncState!;
            var handler = state.Socket;
            int received;
            try
            {
                received = handler.EndReceive(result);
            }
            catch (SocketException)
            {
                received = 0;
            }

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
                    GenerateInternalMessage(state);
                    state.IsInDataSegment = false;
                    state.DataPosition = 0;
                }
                else
                {
                    if (state.DataPosition < 4) continue;

                    state.MessageType = state.MessageData[1];
                    state.MessageType = state.MessageData[0] + (state.MessageType << 8);
                    state.SequenceNumber = state.MessageData[3];
                    state.SequenceNumber = state.MessageData[2] + (state.SequenceNumber << 8);
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