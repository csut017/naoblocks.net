using NaoBlocks.Common;

namespace NaoBlocks.Web.Communications
{
    /// <summary>
    /// Defines a message processor.
    /// </summary>
    public interface IMessageProcessor
    {
        /// <summary>
        /// Processes a message.
        /// </summary>
        /// <param name="client">The client that received the message.</param>
        /// <param name="message">The message to process.</param>
        Task ProcessAsync(ClientConnection client, ClientMessage message);
    }
}