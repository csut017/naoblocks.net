using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using System.Threading.Tasks;

namespace NaoBlocks.Web.Communications.Messages
{
    public class MessageProcessor : IMessageProcessor
    {
        private readonly IHub _hub;
        private readonly ILogger<MessageProcessor> _logger;
        private readonly IDictionary<ClientMessageType, TypeProcessor> _processors;

        public MessageProcessor(IHub hub, ILogger<MessageProcessor> logger)
        {
            this._processors = new Dictionary<ClientMessageType, TypeProcessor>
            {
                {  ClientMessageType.Authenticate, this.Authenticate },
                { ClientMessageType.RequestRobot, this.AllocateRobot },
                { ClientMessageType.TransferProgram, this.TransferRobot },
                { ClientMessageType.StartProgram, this.StartProgram },
                { ClientMessageType.StopProgram, this.StopProgram },
            };
            this._hub = hub;
            this._logger = logger;
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

        private Task AllocateRobot(ClientConnection client, ClientMessage message)
        {
            Thread.Sleep(1000);
            client.SendMessage(GenerateResponse(message, ClientMessageType.RobotAllocated));
            return Task.CompletedTask;
        }

        private Task Authenticate(ClientConnection client, ClientMessage message)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            if (message.Values.TryGetValue("token", out string? token))
            {
                var jwtToken = tokenHandler.ReadJwtToken(token);
                client.SendMessage(GenerateResponse(message, ClientMessageType.Authenticated));
            }
            else
            {
                client.SendMessage(GenerateErrorResponse(message, "Token is missing"));
            }
            return Task.CompletedTask;
        }

        private Task StartProgram(ClientConnection client, ClientMessage message)
        {
            Thread.Sleep(1000);
            client.SendMessage(GenerateResponse(message, ClientMessageType.ProgramStarted));
            return Task.CompletedTask;
        }

        private Task StopProgram(ClientConnection client, ClientMessage message)
        {
            client.SendMessage(GenerateResponse(message, ClientMessageType.ProgramStopped));
            return Task.CompletedTask;
        }

        private Task TransferRobot(ClientConnection client, ClientMessage message)
        {
            Thread.Sleep(1000);
            client.SendMessage(GenerateResponse(message, ClientMessageType.ProgramTransferred));
            return Task.CompletedTask;
        }
    }
}