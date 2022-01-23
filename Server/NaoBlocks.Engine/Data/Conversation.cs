namespace NaoBlocks.Engine.Data
{
    /// <summary>
    /// Defines a conversation between the server and a robot.
    /// </summary>
    public class Conversation
    {
        /// <summary>
        /// Gets or sets the identifier of the conversation.
        /// </summary>
        public long ConversationId { get; set; }

        /// <summary>
        /// Gets or sets the id of the source involved in the conversation.
        /// </summary>
        public string? SourceId { get; set; }

        /// <summary>
        /// Gets or sets the name of the source.
        /// </summary>
        public string? SourceName { get; set; }

        /// <summary>
        /// Gets or sets the type of source (User or Robot).
        /// </summary>
        public string? SourceType { get; set; }

        /// <summary>
        /// Gets the uninitialised conversation.
        /// </summary>
        public static Conversation None
        {
            get { return new Conversation { ConversationId = -1 }; }
        }
    }
}