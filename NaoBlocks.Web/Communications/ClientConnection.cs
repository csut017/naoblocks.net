using NaoBlocks.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NaoBlocks.Web.Communications
{
    public class ClientConnection : IDisposable
    {
        private readonly CancellationTokenSource _cancellationSource = new CancellationTokenSource();
        private readonly IMessageProcessor _messageProcessor;
        private readonly ConcurrentQueue<ClientMessage> _queue = new ConcurrentQueue<ClientMessage>();
        private readonly WebSocket _socket;
        private readonly IList<ClientConnection> _listeners = new List<ClientConnection>();
        private readonly IList<ClientMessage> _messageLog = new List<ClientMessage>();
        private readonly object _messageLogLock = new object();

        public ClientConnection(WebSocket socket, ClientConnectionType type, IMessageProcessor messageProcessor)
        {
            this._socket = socket;
            this.Type = type;
            this._messageProcessor = messageProcessor;
        }

        public event EventHandler<EventArgs>? Closed;

        public IHub? Hub { get; set; }

        public long Id { get; set; }

        public bool IsClosing { get; private set; }

        public Robot? Robot { get; set; }

        public ClientStatus Status { get; } = new ClientStatus();

        public RobotStatus? RobotDetails { get; set; }

        public IList<NotificationAlert> Notifications { get; } = new List<NotificationAlert>();

        public ClientConnectionType Type { get; private set; }

        public User? User { get; set; }

        public void AddListener(ClientConnection listener)
        {
            if (listener == null) throw new ArgumentNullException(nameof(listener));

            this._listeners.Add(listener);
            listener.Closed += (o, e) => this.RemoveListener(listener);
        }

        public void Close()
        {
            this._cancellationSource.Cancel();
            this.IsClosing = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void LogMessage(ClientMessage message)
        {
            lock (this._messageLogLock)
            {
                this._messageLog.Add(message.Clone());
                if (this._messageLog.Count > 100)
                {
                    this._messageLog.RemoveAt(0);
                }
            }
        }

        public Task<IReadOnlyCollection<ClientMessage>> GetMessageLogAsync()
        {
            var clone = new ClientMessage[this._messageLog.Count];
            lock (this._messageLogLock)
            {
                this._messageLog.CopyTo(clone, 0);
            }
            IReadOnlyCollection<ClientMessage> data = new ReadOnlyCollection<ClientMessage>(clone);
            return Task.FromResult(data);
        }

        public void NotifyListeners(ClientMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            foreach (var listener in this._listeners)
            {
                listener.SendMessage(message.Clone());
            }
        }

        public void RemoveListener(ClientConnection listener)
        {
            if (this._listeners.Contains(listener)) this._listeners.Remove(listener);
        }

        public void SendMessage(ClientMessage message)
        {
            this._queue.Enqueue(message);
        }

        public async Task StartAsync()
        {
            var pushTask = Task.Run(async () => await this.PushMessagesAsync(this._cancellationSource.Token));
            try
            {
                while (!this.IsClosing)
                {
                    var (response, message) = await this.ReceiveFullMessageAsync(this._cancellationSource.Token);
                    if (response.MessageType == WebSocketMessageType.Close)
                        break;

                    if (message != null)
                    {
                        var json = Encoding.UTF8.GetString(message.ToArray());
                        var msg = JsonConvert.DeserializeObject<ClientMessage>(json);
                        await this._messageProcessor.ProcessAsync(this, msg);
                    }
                }
                await this._socket.CloseAsync(WebSocketCloseStatus.Empty, string.Empty, CancellationToken.None);
                this.Closed?.Invoke(this, EventArgs.Empty);
            }
            catch (WebSocketException ex)
            {
                switch (ex.WebSocketErrorCode)
                {
                    case WebSocketError.ConnectionClosedPrematurely:
                        this.Closed?.Invoke(this, EventArgs.Empty);
                        break;

                    default:
                        throw;
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._socket.Dispose();
                this._cancellationSource.Dispose();
            }
        }

        private async Task PushMessagesAsync(CancellationToken cancelToken)
        {
            while (!cancelToken.IsCancellationRequested)
            {
                if (this._queue.TryDequeue(out ClientMessage? message))
                {
                    if (message != null)
                    {
                        await this._socket.SendAsync(message.ToArray(), WebSocketMessageType.Text, true, cancelToken);
                    }
                }
                else
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
            }
        }

        private async Task<(WebSocketReceiveResult, IEnumerable<byte>)> ReceiveFullMessageAsync(CancellationToken cancelToken)
        {
            WebSocketReceiveResult response;
            var message = new List<byte>();

            var buffer = new byte[4096];
            do
            {
                response = await this._socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancelToken);
                message.AddRange(new ArraySegment<byte>(buffer, 0, response.Count));
            } while (!response.EndOfMessage);

            return (response, message);
        }
    }
}