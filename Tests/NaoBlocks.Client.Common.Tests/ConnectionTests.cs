using NaoBlocks.Common;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Client.Common.Tests
{
    public class ConnectionTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CallingConnectionAsyncTwiceIsIgnored(bool withDelay)
        {
            // Arrange
            var client = new FakeHttpMessageHandler();
            client.AddOutgoing(
                HttpStatusCode.OK,
                JsonConvert.SerializeObject(new VersionInformation()));
            client.AddOutgoing(
                HttpStatusCode.OK,
                GenerateLoginResponse("welcome"));
            var socket = new FakeWebSocketClient();
            using var connection = new Connection("localhost", "1234");
            connection.InitialiseHttpClient = () => new HttpClient(client);
            connection.InitialiseWebSocket = () => socket;

            // Act
            await connection.ConnectAsync();
            if (withDelay) await Task.Delay(TimeSpan.FromSeconds(0.5));
            await connection.ConnectAsync();
            await connection.DisconnectAsync();
        }

        [Fact]
        public async Task ConnectAsyncHandlesInvalidLogin()
        {
            // Arrange
            var client = new FakeHttpMessageHandler();
            client.AddOutgoing(
                HttpStatusCode.OK,
                JsonConvert.SerializeObject(new VersionInformation()));
            client.AddOutgoing(
                HttpStatusCode.OK,
                GenerateLoginResponse(string.Empty, "Cannot login"));
            using var connection = new Connection("localhost", "1234");
            connection.InitialiseHttpClient = () => new HttpClient(client);

            // Act
            var error = await Assert.ThrowsAsync<ApplicationException>(
                async () => await connection.ConnectAsync());

            // Assert
            Assert.Equal(new[]
            {
                "https://localhost/api/v1/version",
                "https://localhost/api/v1/session"
            }, client.IncomingMessages.Select(r => r.RequestUri?.ToString()).ToArray());
            Assert.Equal("Unable to login", error?.Message);
        }

        [Fact]
        public async Task ConnectAsyncHandlesLoginFailure()
        {
            // Arrange
            var client = new FakeHttpMessageHandler();
            client.AddOutgoing(
                HttpStatusCode.OK,
                JsonConvert.SerializeObject(new VersionInformation()));
            client.AddOutgoing(
                HttpStatusCode.BadRequest,
                GenerateLoginResponse(string.Empty, "Cannot login"));
            using var connection = new Connection("localhost", "1234");
            connection.InitialiseHttpClient = () => new HttpClient(client);

            // Act
            var error = await Assert.ThrowsAsync<HttpRequestException>(
                async () => await connection.ConnectAsync());

            // Assert
            Assert.Equal(new[]
            {
                "https://localhost/api/v1/version",
                "https://localhost/api/v1/session"
            }, client.IncomingMessages.Select(r => r.RequestUri?.ToString()).ToArray());
            Assert.Equal("Response status code does not indicate success: 400 (Bad Request).", error?.Message);
        }

        [Fact]
        public async Task ConnectAsyncHandlesMissingToken()
        {
            // Arrange
            var client = new FakeHttpMessageHandler();
            client.AddOutgoing(
                HttpStatusCode.OK,
                JsonConvert.SerializeObject(new VersionInformation()));
            client.AddOutgoing(
                HttpStatusCode.OK,
                GenerateLoginResponse(string.Empty));
            using var connection = new Connection("localhost", "1234");
            connection.InitialiseHttpClient = () => new HttpClient(client);

            // Act
            var error = await Assert.ThrowsAsync<ApplicationException>(
                async () => await connection.ConnectAsync());

            // Assert
            Assert.Equal(new[]
            {
                "https://localhost/api/v1/version",
                "https://localhost/api/v1/session"
            }, client.IncomingMessages.Select(r => r.RequestUri?.ToString()).ToArray());
            Assert.Equal("Unable to login", error?.Message);
        }

        [Fact]
        public async Task ConnectAsyncHandlesVersionFailure()
        {
            // Arrange
            var client = new FakeHttpMessageHandler();
            client.AddOutgoing(
                HttpStatusCode.ServiceUnavailable,
                JsonConvert.SerializeObject(new VersionInformation()));
            using var connection = new Connection("localhost", "1234");
            connection.InitialiseHttpClient = () => new HttpClient(client);

            // Act
            var error = await Assert.ThrowsAsync<HttpRequestException>(
                async () => await connection.ConnectAsync());

            // Assert
            Assert.Equal(new[]
            {
                "https://localhost/api/v1/version"
            }, client.IncomingMessages.Select(r => r.RequestUri?.ToString()).ToArray());
            Assert.Equal("Response status code does not indicate success: 503 (Service Unavailable).", error?.Message);
        }

        [Fact]
        public async Task ConnectAsyncRetrievesVersion()
        {
            // Arrange
            var client = new FakeHttpMessageHandler();
            client.AddOutgoing(
                HttpStatusCode.OK,
                JsonConvert.SerializeObject(new VersionInformation
                {
                    Version = "1.2.3.4"
                }));
            client.AddOutgoing(
                HttpStatusCode.OK,
                GenerateLoginResponse("welcome"));
            var socket = new FakeWebSocketClient();
            using var connection = new Connection("localhost", "1234");
            connection.InitialiseHttpClient = () => new HttpClient(client);
            connection.InitialiseWebSocket = () => socket;

            // Act
            await connection.ConnectAsync();
            await Task.Delay(TimeSpan.FromSeconds(0.5));
            Assert.True(connection.IsConnected, "Expected to be connected");

            // Assert
            Assert.Equal("1.2.3.4", connection.ServerVersion);
            await connection.DisconnectAsync();
        }

        [Theory]
        [InlineData(true, "https", "wss")]
        [InlineData(false, "http", "ws")]
        public async Task ConnectionAsyncStartsProcessingLoop(bool isSecure, string httpPrefix, string wsPrefix)
        {
            // Arrange
            var client = new FakeHttpMessageHandler();
            client.AddOutgoing(
                HttpStatusCode.OK,
                JsonConvert.SerializeObject(new VersionInformation()));
            client.AddOutgoing(
                HttpStatusCode.OK,
                GenerateLoginResponse("welcome"));
            var socket = new FakeWebSocketClient();
            using var connection = new Connection("localhost", "1234", isSecure);
            connection.InitialiseHttpClient = () => new HttpClient(client);
            connection.InitialiseWebSocket = () => socket;

            // Act
            await connection.ConnectAsync();
            await Task.Delay(TimeSpan.FromSeconds(0.5));
            Assert.True(connection.IsConnected, "Expected to be connected");
            await connection.DisconnectAsync();

            // Assert
            Assert.Equal(new[]
            {
                $"{httpPrefix}://localhost/api/v1/version",
                $"{httpPrefix}://localhost/api/v1/session"
            }, client.IncomingMessages.Select(r => r.RequestUri?.ToString()).ToArray());
            Assert.Equal($"{wsPrefix}://localhost/api/v1/connections/robot", socket.Address);
        }

        [Fact]
        public async Task DisconnectAsyncWorksWhenDisconnected()
        {
            // Arrange
            using var connection = new Connection("localhost", "1234");

            // Act
            await connection.DisconnectAsync();

            // Assert
            Assert.False(connection.IsConnected);
        }

        [Fact]
        public async Task HandlesCloseMessage()
        {
            // Arrange
            var client = new FakeHttpMessageHandler();
            client.AddOutgoing(
                HttpStatusCode.OK,
                JsonConvert.SerializeObject(new VersionInformation()));
            client.AddOutgoing(
                HttpStatusCode.OK,
                GenerateLoginResponse("welcome"));
            var socket = new FakeWebSocketClient();
            using var connection = new Connection("localhost", "1234");
            connection.InitialiseHttpClient = () => new HttpClient(client);
            connection.InitialiseWebSocket = () => socket;

            // Act
            var waiter = new AutoResetEvent(false);
            connection.OnClosed += (o, e) => waiter.Set();
            await connection.ConnectAsync();
            await Task.Delay(TimeSpan.FromSeconds(0.5));
            Assert.True(connection.IsConnected, "Expected to be connected");
            socket.AddOutgoingCloseMessage();
            Assert.True(waiter.WaitOne(TimeSpan.FromSeconds(5)), "Connection was not closed");

            // Assert
            Assert.False(connection.IsConnected, "Expected to be disconnected");
        }

        [Fact]
        public async Task HandlesIncomingMessage()
        {
            // Arrange
            var client = new FakeHttpMessageHandler();
            client.AddOutgoing(
                HttpStatusCode.OK,
                JsonConvert.SerializeObject(new VersionInformation()));
            client.AddOutgoing(
                HttpStatusCode.OK,
                GenerateLoginResponse("welcome"));
            var socket = new FakeWebSocketClient();
            using var connection = new Connection("localhost", "1234");
            connection.InitialiseHttpClient = () => new HttpClient(client);
            connection.InitialiseWebSocket = () => socket;

            // Act
            await connection.ConnectAsync();
            await Task.Delay(TimeSpan.FromSeconds(0.5));
            Assert.True(connection.IsConnected, "Expected to be connected");
            ClientMessage? messageRecieved = null;
            var waiter = new AutoResetEvent(false);
            var message = connection.OnMessageReceived.Subscribe(message =>
            {
                messageRecieved = message;
                waiter.Set();
            });
            socket.AddOutgoingMessage(
                new ClientMessage(ClientMessageType.DownloadProgram));
            Assert.True(waiter.WaitOne(TimeSpan.FromSeconds(5)), "Message was not received");
            await connection.DisconnectAsync();

            // Assert
            Assert.Equal(ClientMessageType.DownloadProgram, messageRecieved?.Type);
        }

        [Fact]
        public void InitialiseHttpClientReturnsClient()
        {
            // Arrange
            using var connection = new Connection("localhost", "1234");

            // Act
            using var client = connection.InitialiseHttpClient();

            // Assert
            Assert.IsType<HttpClient>(client);
        }

        [Fact]
        public void InitialiseWebSocketReturnsWrapper()
        {
            // Arrange
            using var connection = new Connection("localhost", "1234");

            // Act
            using var socket = connection.InitialiseWebSocket();

            // Assert
            Assert.IsType<ClientWebSocketWrapper>(socket);
        }

        [Theory]
        [InlineData(true, "https://localhost/api/v1/code/Mia/987")]
        [InlineData(false, "http://localhost/api/v1/code/Mia/987")]
        public async Task RetrieveCodeAsyncHandlesData(bool isSecure, string expectedUrl)
        {
            // Arrange
            var client = new FakeHttpMessageHandler();
            client.AddOutgoing(
                HttpStatusCode.OK,
                GenerateProgramResponse(
                            GenerateFunctionNode("reset"),
                            GenerateFunctionNode("go")));
            using var connection = new Connection("localhost", "1234", useSecure: isSecure);
            connection.InitialiseHttpClient = () => new HttpClient(client);
            var message = new ClientMessage(
                ClientMessageType.DownloadProgram,
                new
                {
                    user = "Mia",
                    program = "987"
                });

            // Act
            var nodes = await connection.RetrieveCodeAsync(message);

            // Assert
            Assert.Equal(new[] {
                    "Function:reset",
                    "Function:go"
                }, nodes.Select(n => n.ToString()).ToArray());
            var targetUrl = client.IncomingMessages.FirstOrDefault()?.RequestUri?.ToString();
            Assert.Equal(expectedUrl, targetUrl);
        }

        [Fact]
        public async Task RetrieveCodeAsyncHandlesInvalidSourceMessage()
        {
            // Arrange
            using var connection = new Connection("localhost", "1234");

            // Act
            var nodes = await connection.RetrieveCodeAsync(new ClientMessage(ClientMessageType.Authenticate));

            // Assert
            Assert.Empty(nodes);
        }

        [Fact]
        public async Task RetrieveServerVersionWorks()
        {
            // Arrange
            var client = new FakeHttpMessageHandler();
            client.AddOutgoing(
                HttpStatusCode.OK,
                JsonConvert.SerializeObject(new VersionInformation
                {
                    Version = "1.2.3.4"
                }));
            using var connection = new Connection("localhost", "1234");
            connection.InitialiseHttpClient = () => new HttpClient(client);

            // Act
            var version = await connection.RetrieveServerVersion();

            // Assert
            Assert.Equal("1.2.3.4", version?.Version);
        }

        [Fact]
        public async Task SendMessageAsyncFailsWhenNotConnected()
        {
            // Arrange
            using var connection = new Connection("localhost", "1234");

            // Act
            await Assert.ThrowsAsync<ApplicationException>(async () => await connection.SendMessageAsync(new ClientMessage()));
        }

        private static AstNode GenerateFunctionNode(string name)
        {
            return new AstNode(
                AstNodeType.Function,
                new Token(TokenType.Identifier, name),
                string.Empty);
        }

        private static StringContent GenerateLoginResponse(string token, params string[] errors)
        {
            var response = ExecutionResult.New(
                new UserSessionResult
                {
                    Token = token
                });

            response.ExecutionErrors = errors.Select((e, c) => new CommandError(c, e)).ToArray();
            var json = JsonConvert.SerializeObject(response);
            return new StringContent(json);
        }

        private static StringContent GenerateProgramResponse(params AstNode[] nodes)
        {
            var program = new CompiledCodeProgram(nodes);
            var message = ExecutionResult.New(program);
            var json = JsonConvert.SerializeObject(message);
            return new StringContent(json);
        }
    }
}