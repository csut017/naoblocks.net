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
        /// Gets or sets the user involved in the conversation.
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// Gets the uninitialised conversation.
        /// </summary>
        public static Conversation None
        {
            get { return new Conversation { ConversationId = -1 }; }
        }
    }
}