using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace NaoBlocks.Web.Communications.Messages
{
    public class MessageProcessor : IMessageProcessor
    {
        private readonly IHub _hub;
        private readonly IDictionary<ClientMessageType, TypeProcessor> _processors;

        public MessageProcessor(IHub hub)
        {
            this._processors = new Dictionary<ClientMessageType, TypeProcessor>
            {
                {  ClientMessageType.Authenticate, this.Authenticate }
            };
            this._hub = hub;
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
                    await processor(client, message);
                }
                catch (Exception err)
                {
                    client.SendMessage(GenerateErrorResponse(message, $"Unable to process message: {err.Message}"));
                }
            }
            else
            {
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
    }
}