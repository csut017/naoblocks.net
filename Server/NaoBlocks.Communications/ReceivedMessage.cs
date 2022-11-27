using NaoBlocks.Common;

namespace NaoBlocks.Communications
{
    /// <summary>
    /// A message that was received from a client.
    /// </summary>
    public class ReceivedMessage
        : ClientMessage
    {
        /// <summary>
        /// Instantiates a new <see cref="ReceivedMessage"/> instance.
        /// </summary>
        /// <param name="client">The client that received the message.</param>
        /// <param name="type">The message type.</param>
        public ReceivedMessage(IClient client, ClientMessageType type)
            : base(type)
        {
            this.Client = client;
        }

        /// <summary>
        /// Gets the client that received the message.
        /// </summary>
        public IClient Client { get; }
    }
}