﻿using NaoBlocks.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace NaoBlocks.Client
{
    /// <summary>
    /// Connection to a NaoBlocks.Net server.
    /// </summary>
    public class Connection : IDisposable
    {
        public const int receiveBufferSize = 8192;
        private readonly string address;
        private readonly string password;
        private readonly bool useSecure;
        private readonly string robotName;
        private CancellationTokenSource cancellation;
        private ClientWebSocket webSocket;
        private string token = string.Empty;
        private Subject<ClientMessage> messageReceived = new Subject<ClientMessage>();

        public Connection(string address, string password, bool useSecure = true, string robotName = null)
        {
            this.OnMessageReceived = this.messageReceived.AsObservable();
            this.address = address;
            this.password = password;
            this.useSecure = useSecure;
            this.robotName = robotName;
        }

        ~Connection()
        {
            this.Dispose(false);
        }

        public async Task ConnectAsync()
        {
            // Make sure we can connect
            if (this.webSocket != null)
            {
                if (this.webSocket.State == WebSocketState.Open) return;
                else this.webSocket.Dispose();
            }

            // Get the server version first
            using (var httpClient = new HttpClient())
            {
                var versionAddress = $"{(this.useSecure ? "https" : "http")}://{this.address}/api/v1/version";
                var response = await httpClient.GetAsync(versionAddress);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var versionInfo = JsonConvert.DeserializeObject<VersionInformation>(json);
                this.ServerVersion = versionInfo.Version;

                // Then authenticate
                var loginAddress = $"{(this.useSecure ? "https" : "http")}://{this.address}/api/v1/session";
                var req = new
                {
                    name = this.robotName ?? Environment.MachineName,
                    password = password,
                    role = "robot"
                };
                response = await httpClient.PostAsJsonAsync(loginAddress, req);
                response.EnsureSuccessStatusCode();
                json = await response.Content.ReadAsStringAsync();
                var sessionInfo = JsonConvert.DeserializeObject<ExecutionResult<TokenSession>>(json);
                this.token = sessionInfo.Successful ? sessionInfo.Output.Token : string.Empty;
                if (string.IsNullOrEmpty(this.token))
                {
                    this.CleanUpAfterDisconnect();
                    throw new ApplicationException("Unable to login");
                }
            }

            // Open the websocket and start processing messages
            this.webSocket = new ClientWebSocket();
            if (this.cancellation != null) this.cancellation.Dispose();
            this.cancellation = new CancellationTokenSource();
            var socketAddress = $"{(this.useSecure ? "wss" : "ws")}://{this.address}/api/v1/connections/robot";
            await this.webSocket.ConnectAsync(new Uri(socketAddress), this.cancellation.Token);
            await Task.Factory.StartNew(this.ReceiveLoop, this.cancellation.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public async Task DisconnectAsync()
        {
            if (this.webSocket is null) return;
            if (this.webSocket.State == WebSocketState.Open)
            {
                this.cancellation.CancelAfter(TimeSpan.FromSeconds(2));
                await this.webSocket.CloseOutputAsync(WebSocketCloseStatus.Empty, "", CancellationToken.None);
                await this.webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            }

            this.CleanUpAfterDisconnect();
        }

        public async Task<AstNode[]> RetrieveCodeAsync(ClientMessage message)
        {
            if (message.Type != ClientMessageType.DownloadProgram)
            {
                return new AstNode[0];
            }

            var userID = message.Values["user"];
            var programID = message.Values["program"];
            return await this.RetrieveCodeAsync(userID, programID);
        }

        public async Task<AstNode[]> RetrieveCodeAsync(string userID, string programID)
        {
            using (var client = new HttpClient())
            {
                var address = $"{(this.useSecure ? "https" : "http")}://{this.address}/api/v1/code/{userID}/{programID}";
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.token);
                var response = await client.GetAsync(address);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var program = JsonConvert.DeserializeObject<ExecutionResult<CompiledCodeProgram>>(json);
                return program.Output.Nodes.ToArray();
            }
        }

        private async Task ReceiveLoop()
        {
            // Set authenticate message so everything is ready to run
            this.IsConnected = true;
            await this.SendMessageAsync(new ClientMessage(ClientMessageType.Authenticate, new { token = this.token }));

            // Then, start waiting for responses
            var loopToken = this.cancellation.Token;
            MemoryStream outputStream = null;
            WebSocketReceiveResult receiveResult = null;
            var buffer = new byte[receiveBufferSize];
            try
            {
                while (!loopToken.IsCancellationRequested)
                {
                    outputStream = new MemoryStream(receiveBufferSize);
                    do
                    {
                        receiveResult = await this.webSocket.ReceiveAsync(buffer, this.cancellation.Token);
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

        public bool IsConnected { get; private set; } = false;

        public string ServerVersion { get; private set; } = string.Empty;

        public event EventHandler OnClosed;

        public IObservable<ClientMessage> OnMessageReceived { get; private set; } 

        public async Task SendMessageAsync(ClientMessage message)
        {
            var data = message.ToArray();
            await this.webSocket.SendAsync(data, WebSocketMessageType.Text, true, this.cancellation.Token);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.DisconnectAsync().Wait();
        }

        public void Dispose()
        {
            this.Dispose(true);
        }
    }
}