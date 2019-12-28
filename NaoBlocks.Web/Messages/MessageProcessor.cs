using NaoBlocks.Web.Communications;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace NaoBlocks.Web.Messages
{
    public class MessageProcessor : IMessageProcessor
    {
        private readonly IDictionary<ClientMessageType, TypeProcessor> _processors
            = new Dictionary<ClientMessageType, TypeProcessor> { };

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
                    client.SendMessage(this.GenerateErrorResponse(message, $"Unable to process message: {err.Message}"));
                }
            }
            else
            {
                client.SendMessage(this.GenerateErrorResponse(message, $"Unable to find processor for {message.Type}"));
            }
        }

        private ClientMessage GenerateErrorResponse(ClientMessage request, string message)
        {
            var response = new ClientMessage
            {
                Type = ClientMessageType.ErrorResponse,
                ConversationId = request.ConversationId,
                ClientId = request.ClientId
            };
            response.Values["error"] = message;
            return response;
        }
    }
}