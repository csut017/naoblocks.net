using Newtonsoft.Json;

namespace NaoBlocks.Engine.Data
{
    /// <summary>
    /// Defines the types of conversation.
    /// </summary>
    [JsonConverter(typeof(TolerantEnumConverter))]
    public enum ConversationType
    {
        /// <summary>
        /// The conversation type is unknown
        /// </summary>
        Unknown,

        /// <summary>
        /// The conversation is for a robot initialisation.
        /// </summary>
        Initialisation,

        /// <summary>
        /// The conversation is for a program execution.
        /// </summary>
        Program,

        /// <summary>
        /// The conversation is a user-triggered signal (e.g. for changing a session.)
        /// </summary>
        Signal,

        /// <summary>
        /// The conversation is a logging conversation (e.g. a robot-initialised logging.)
        /// </summary>
        Logging,
    }
}