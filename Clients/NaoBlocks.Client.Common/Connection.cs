using NaoBlocks.Common;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace NaoBlocks.Client.Common
{
    /// <summary>
    /// A connection to a NaoBlocks.Net server.
    /// </summary>
    public class Connection
        : IDisposable
    {
        private const int receiveBufferSize = 8192;
        private readonly string address;
        private readonly Subject<ClientMessage> messageReceived = new();
        private readonly string password;
        private readonly string robotName;
        private CancellationTokenSource? cancellation;
        private string token = string.Empty;
        private IWebSocket? webSocket;

        /// <summary>
        /// Initialiase a new <see cref="Connection"/> instance.
        /// </summary>
        /// <param name="address">The address of the server to connect to.</param>
        /// <param name="password">The password to use.</param>
        /// <param name="useSecure">Whether to use secure mode (https) or not.</param>
        /// <param name="robotName">The name of the robot to pass to the server. Defaults to the current machine name.</param>
        public Connection(string address, string password, bool useSecure = true, string? robotName = null)
        {
            this.OnMessageReceived = this.messageReceived.AsObservable();
            this.address = address;
            this.password = password;
            this.IsSecure = useSecure;
            this.robotName = robotName ?? Environment.MachineName;
        }

        /// <summary>
        /// Cleans up any unmanaged resources.
        /// </summary>
        ~Connection()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// An event handler for when the connection is closed.
        /// </summary>
        public event EventHandler? OnClosed;

        /// <summary>
        /// Gets or sets a function for initialising a new <see cref="HttpClient"/> instance.
        /// </summary>
        /// <remarks>
        /// This property is mainly for allowing unit tests on the <see cref="Connection"/> class.
        /// </remarks>
        public Func<HttpClient> InitialiseHttpClient { get; set; } = () => new HttpClient();

        /// <summary>
        /// Gets or sets a function for initialising a new <see cref="WebSocket"/> instance.
        /// </summary>
        /// <remarks>
        /// This property is mainly for allowing unit tests on the <see cref="Connection"/> class.
        /// </remarks>
        public Func<IWebSocket> InitialiseWebSocket { get; set; } = () => new ClientWebSocketWrapper(new ClientWebSocket());

        /// <summary>
        /// Gets whether the connection is connected to the server.
        /// </summary>
        public bool IsConnected { get; private set; } = false;

        /// <summary>
        /// Gets whether the connection is secure (uses HTTPS).
        /// </summary>
        public bool IsSecure { get; private set; } = false;

        /// <summary>
        /// An observable connecting the received messages.
        /// </summary>
        public IObservable<ClientMessage> OnMessageReceived { get; private set; }

        /// <summary>
        /// Gets the current server version.
        /// </summary>
        public string ServerVersion { get; private set; } = string.Empty;

        /// <summary>
        /// Attempts to connect to the server.
        /// </summary>
        /// <exception cref="ApplicationException">Thrown when there is an error connecting.</exception>
        public async Task ConnectAsync()
        {
            // Make sure we can connect
            if (this.webSocket != null)
            {
                if (this.webSocket.State == WebSocketState.Open) return;
                else this.webSocket.Dispose();
            }

            this.token = string.Empty;
            using (var httpClient = this.InitialiseHttpClient())
            {
                // Get the server version first
                var versionInfo = await this.DoRetrieveServerVersion(httpClient);
                this.ServerVersion = string.IsNullOrEmpty(versionInfo?.Version) ? "<Unknown>" : versionInfo!.Version;

                // Then authenticate
                var loginAddress = $"{(this.IsSecure ? "https" : "http")}://{this.address}/api/v1/session";
                var req = new
                {
                    name = this.robotName,
                    password,
                    role = "robot"
                };

                var response = await httpClient.PostAsJsonAsync(loginAddress, req);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var sessionInfo = JsonConvert.DeserializeObject<ExecutionResult<UserSessionResult>>(json);
                if (sessionInfo?.Successful ?? false) this.token = sessionInfo?.Output?.Token ?? string.Empty;
                if (string.IsNullOrEmpty(this.token))
                {
                    this.CleanUpAfterDisconnect();
                    throw new ApplicationException("Unable to login");
                }
            }

            // Open the websocket and start processing messages
            this.webSocket = this.InitialiseWebSocket();
            this.cancellation = new CancellationTokenSource();
            var socketAddress = $"{(this.IsSecure ? "wss" : "ws")}://{this.address}/api/v1/connections/robot";
            await this.webSocket.ConnectAsync(new Uri(socketAddress), this.cancellation.Token);
#pragma warning disable CS4014 // We want this call to run in the background, so we don't care that we continue immediately
            Task.Factory.StartNew(this.ReceiveLoop, this.cancellation.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
#pragma warning restore CS4014
        }

        /// <summary>
        /// Disconnect from the server.
        /// </summary>
        public async Task DisconnectAsync()
        {
            if (this.webSocket is null) return;
            if (this.webSocket.State == WebSocketState.Open)
            {
                this.cancellation?.CancelAfter(TimeSpan.FromSeconds(2));
                await this.webSocket.CloseOutputAsync(WebSocketCloseStatus.Empty, "", CancellationToken.None);
                await this.webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            }

            this.CleanUpAfterDisconnect();
        }

        /// <summary>
        /// Cleans up the resources for this <see cref="Connection"/> instance.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Attempt to retrieve the code for a program using a message.
        /// </summary>
        /// <param name="message">The source message detailing the program to retrieve.</param>
        /// <returns>The <see cref="AstNode"/> instances containing the compiled program.</returns>
        public async Task<AstNode[]> RetrieveCodeAsync(ClientMessage message)
        {
            if (message.Type != ClientMessageType.DownloadProgram)
            {
                return Array.Empty<AstNode>();
            }

            var userID = message.Values["user"];
            var programID = message.Values["program"];
            return await this.RetrieveCodeAsync(userID, programID);
        }

        /// <summary>
        /// Attempt to retrieve the code for a program by user name and program id.
        /// </summary>
        /// <param name="userName">The name of the user.</param>
        /// <param name="programNumber">The number of the program.</param>
        /// <returns>The <see cref="AstNode"/> instances containing the compiled program.</returns>
        public async Task<AstNode[]> RetrieveCodeAsync(string userName, string programNumber)
        {
            using var client = this.InitialiseHttpClient();
            var address = $"{(this.IsSecure ? "https" : "http")}://{this.address}/api/v1/code/{userName}/{programNumber}";
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.token);
            var response = await client.GetAsync(address);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var program = JsonConvert.DeserializeObject<ExecutionResult<CompiledCodeProgram>>(json);
            return program?.Output?.Nodes?.ToArray() ?? Array.Empty<AstNode>();
        }

        /// <summary>
        /// Attempts to retrieve the current server version.
        /// </summary>
        /// <returns>A <see cref="VersionInformation"/> instance containing the version information.</returns>
        public async Task<VersionInformation> RetrieveServerVersion()
        {
            using var httpClient = this.InitialiseHttpClient();
            var version = await this.DoRetrieveServerVersion(httpClient);
            return version;
        }

        /// <summary>
        /// Sends a message to the server.
        /// </summary>
        /// <param name="message">The <see cref="ClientMessage"/> to send.</param>
        public async Task SendMessageAsync(ClientMessage message)
        {
            if (this.webSocket == null) throw new ApplicationException("Unable to send message: no websocket");

            var cancellationToken = this.cancellation?.Token ?? CancellationToken.None;
            var data = message.ToArray();
            await this.webSocket.SendAsync(data, WebSocketMessageType.Text, true, cancellationToken);
        }

        /// <summary>
        /// Cleans up the resources for this <see cref="Connection"/> instance.
        /// </summary>
        /// <param name="disposing">Whether the instance is being disposed or not.</param>
        protected virtual void Dispose(bool disposing)
        {
            this.DisconnectAsync().Wait();
        }

        /// <summary>
        /// Clean up all the resources for the socket and any downloaded details.
        /// </summary>
        private void CleanUpAfterDisconnect()
        {
            this.IsConnected = false;
            this.ServerVersion = string.Empty;
            this.token = string.Empty;
            this.webSocket?.Dispose();
            this.webSocket = null;
            this.cancellation?.Dispose();
            this.cancellation = null;
        }

        private async Task<VersionInformation> DoRetrieveServerVersion(HttpClient httpClient)
        {
            var versionAddress = $"{(this.IsSecure ? "https" : "http")}://{this.address}/api/v1/version";
            var response = await httpClient.GetAsync(versionAddress);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var versionInfo = JsonConvert.DeserializeObject<VersionInformation>(json);
            return versionInfo!;
        }

        /// <summary>
        /// Main processing loop for receiving messages.
        /// </summary>
        private async Task ReceiveLoop()
        {
            // Set authenticate message so everything is ready to run
            this.IsConnected = true;
            await this.SendMessageAsync(new ClientMessage(ClientMessageType.Authenticate, new { token = this.token! }));

            // Then, start waiting for responses
            var loopToken = this.cancellation?.Token ?? CancellationToken.None;
            MemoryStream? outputStream = null;
            WebSocketReceiveResult? receiveResult;
            var buffer = new byte[receiveBufferSize];
            try
            {
                while (!loopToken.IsCancellationRequested)
                {
                    outputStream = new MemoryStream(receiveBufferSize);
                    do
                    {
                        receiveResult = await this.webSocket!.ReceiveAsync(buffer, loopToken);
                        if (receiveResult.MessageType != WebSocketMessageType.Close)
                            outputStream.Write(buffer, 0, receiveResult.Count);
                    }
                    while (!receiveResult.EndOfMessage);

                    // If the socket is closed, then shutdown the connection
                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        this.OnClosed?.Invoke(this, EventArgs.Empty);
                        this.CleanUpAfterDisconnect();
                        break;
                    }
                    outputStream.Position = 0;

                    // Parse the message and invoke an event
                    var msg = ClientMessage.FromArray(outputStream.ToArray());
                    await outputStream.DisposeAsync();
                    this.messageReceived.OnNext(msg);
                }
            }
            catch (TaskCanceledException) { }
            finally
            {
                outputStream?.Dispose();
            }
        }
    }
}