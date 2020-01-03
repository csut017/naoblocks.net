using Microsoft.Extensions.Logging;
using NaoBlocks.Core.Models;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Web.Communications
{
    public class MessageProcessor : IMessageProcessor
    {
        private readonly IHub _hub;
        private readonly ILogger<MessageProcessor> _logger;
        private readonly IDictionary<ClientMessageType, TypeProcessor> _processors;
        private readonly IAsyncDocumentSession _session;

        public MessageProcessor(IHub hub, ILogger<MessageProcessor> logger, IAsyncDocumentSession session)
        {
            this._processors = new Dictionary<ClientMessageType, TypeProcessor>
            {
                {  ClientMessageType.Authenticate, this.Authenticate },
                { ClientMessageType.RequestRobot, this.AllocateRobot },
                { ClientMessageType.TransferProgram, this.TransferProgramToRobot },
                { ClientMessageType.StartProgram, this.StartProgram },
                { ClientMessageType.StopProgram, this.StopProgram },
                { ClientMessageType.ProgramDownloaded, this.BroadcastMessage(ClientMessageType.ProgramTransferred) },
                { ClientMessageType.ProgramStarted, this.BroadcastMessage(ClientMessageType.ProgramStarted) },
                { ClientMessageType.ProgramFinished, this.BroadcastMessage(ClientMessageType.ProgramFinished) },
                { ClientMessageType.ProgramStopped, this.BroadcastMessage(ClientMessageType.ProgramStopped) },
                { ClientMessageType.RobotDebugMessage, this.BroadcastMessage(ClientMessageType.RobotDebugMessage, true) },
                { ClientMessageType.RobotError, this.BroadcastMessage(ClientMessageType.RobotError, true) },
                { ClientMessageType.RobotStateUpdate, this.BroadcastMessage(ClientMessageType.RobotStateUpdate, true) },
            };
            this._hub = hub;
            this._logger = logger;
            this._session = session;
        }

        private delegate Task TypeProcessor(ClientConnection client, ClientMessage message);

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Message processor should catch any errors and pass it to the client")]
        public async Task ProcessAsync(ClientConnection client, ClientMessage message)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (this._processors.TryGetValue(message.Type, out TypeProcessor? processor) && (processor != null))
            {
                try
                {
                    this._logger.LogInformation($"Processing message type {message.Type}");
                    await processor(client, message);
                }
                catch (Exception err)
                {
                    this._logger.LogWarning($"An error occurred while processing {message.Type}: {err.Message}");
                    client.SendMessage(GenerateErrorResponse(message, $"Unable to process message: {err.Message}"));
                }
            }
            else
            {
                this._logger.LogWarning($"Unable to find processor for message type {message.Type}");
                client.SendMessage(GenerateErrorResponse(message, $"Unable to find processor for {message.Type}"));
            }
        }

        private static ClientMessage GenerateErrorResponse(ClientMessage request, string message)
        {
            var response = new ClientMessage
            {
                Type = ClientMessageType.Error,
                ConversationId = request.ConversationId
            };
            response.Values["error"] = message;
            return response;
        }

        private static ClientMessage GenerateResponse(ClientMessage request, ClientMessageType type)
        {
            var response = new ClientMessage
            {
                Type = type,
                ConversationId = request.ConversationId
            };
            return response;
        }

        private static bool ValidateRequest(ClientConnection client, ClientMessage message)
        {
            if (client.User == null)
            {
                client.SendMessage(GenerateResponse(message, ClientMessageType.NotAuthenticated));
                return false;
            }

            return true;
        }

        private Task AllocateRobot(ClientConnection client, ClientMessage message)
        {
            if (!ValidateRequest(client, message)) return Task.CompletedTask;

            var rnd = new Random();
            var nextRobot = this._hub.GetClients(ClientConnectionType.Robot)
                .OrderBy(r => rnd.Next())
                .FirstOrDefault(r => r.Status.IsAvailable);

            if (nextRobot == null)
            {
                client.SendMessage(GenerateResponse(message, ClientMessageType.NoRobotsAvailable));
                return Task.CompletedTask;
            }

            nextRobot.Status.IsAvailable = false;
            var response = GenerateResponse(message, ClientMessageType.RobotAllocated);
            response.Values["robot"] = nextRobot.Id.ToString(CultureInfo.InvariantCulture);
            client.SendMessage(response);
            return Task.CompletedTask;
        }

        private async Task Authenticate(ClientConnection client, ClientMessage message)
        {
            if (!message.Values.TryGetValue("token", out string? token))
            {
                client.SendMessage(GenerateErrorResponse(message, "Token is missing"));
                return;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var sessionId = jwtToken.Claims.FirstOrDefault(c => c.Type == "SessionId");
            if (sessionId == null)
            {
                client.SendMessage(GenerateErrorResponse(message, "Token is invalid"));
                return;
            }

            var session = await this._session.LoadAsync<Session>(sessionId.Value);
            if ((session == null) || (session.WhenExpires < DateTime.UtcNow))
            {
                client.SendMessage(GenerateErrorResponse(message, "Session is invalid"));
                return;
            }

            if (session.IsRobot)
            {
                client.Robot = await this._session.LoadAsync<Robot>(session.UserId);
            }
            else
            {
                client.User = await this._session.LoadAsync<User>(session.UserId);
            }

            client.SendMessage(GenerateResponse(message, ClientMessageType.Authenticated));
        }

        private TypeProcessor BroadcastMessage(ClientMessageType messageType, bool includeValues = false)
        {
            return (ClientConnection client, ClientMessage message) =>
            {
                var msg = GenerateResponse(message, messageType);
                if (includeValues)
                {
                    foreach (var (key, value) in message.Values)
                    {
                        msg.Values[key] = value;
                    }
                }
                client.NotifyListeners(msg);
                return Task.CompletedTask;
            };
        }

        private bool RetrieveRobot(ClientConnection client, ClientMessage message, out ClientConnection? robotClient)
        {
            robotClient = null;
            if (!message.Values.TryGetValue("robot", out string? robotCode))
            {
                client.SendMessage(GenerateErrorResponse(message, "Robot is missing"));
                return false;
            }

            if (!int.TryParse(robotCode, out int robotId))
            {
                client.SendMessage(GenerateErrorResponse(message, "Robot id is invalid"));
                return false;
            }

            robotClient = this._hub.GetClient(robotId);
            if (robotClient == null)
            {
                client.SendMessage(GenerateErrorResponse(message, "Robot is not longer connected"));
                return false;
            }

            return true;
        }

        private Task StartProgram(ClientConnection client, ClientMessage message)
        {
            if (!ValidateRequest(client, message)) return Task.CompletedTask;
            if (!this.RetrieveRobot(client, message, out ClientConnection? robotClient)) return Task.CompletedTask;

            if (!message.Values.TryGetValue("program", out string? programId))
            {
                client.SendMessage(GenerateErrorResponse(message, "Program is missing"));
                return Task.CompletedTask;
            }

            var clientMessage = GenerateResponse(message, ClientMessageType.StartProgram);
            clientMessage.Values.Add("program", programId);
            robotClient?.SendMessage(clientMessage);
            return Task.CompletedTask;
        }

        private Task StopProgram(ClientConnection client, ClientMessage message)
        {
            if (!ValidateRequest(client, message)) return Task.CompletedTask;

            client.SendMessage(GenerateResponse(message, ClientMessageType.ProgramStopped));
            return Task.CompletedTask;
        }

        private Task TransferProgramToRobot(ClientConnection client, ClientMessage message)
        {
            if (!ValidateRequest(client, message)) return Task.CompletedTask;
            if (!this.RetrieveRobot(client, message, out ClientConnection? robotClient)) return Task.CompletedTask;

            if (!message.Values.TryGetValue("program", out string? programId))
            {
                client.SendMessage(GenerateErrorResponse(message, "Program is missing"));
                return Task.CompletedTask;
            }

            var clientMessage = GenerateResponse(message, ClientMessageType.DownloadProgram);
            clientMessage.Values.Add("program", programId);
            robotClient?.SendMessage(clientMessage);
            return Task.CompletedTask;
        }
    }
}