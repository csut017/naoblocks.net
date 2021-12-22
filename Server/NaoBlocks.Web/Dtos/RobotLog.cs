using Data = NaoBlocks.Engine.Data;

namespace NaoBlocks.Web.Dtos
{
    /// <summary>
    /// A Data Transfer Object for a robot log.
    /// </summary>
    public class RobotLog
    {
        /// <summary>
        /// Gets or sets the conversation id.
        /// </summary>
        public long ConversationId { get; set; }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// Gets the lines for the log.
        /// </summary>
        public IList<RobotLogLine>? Lines { get; private set; }

        /// <summary>
        /// Gets or sets when the log was added.
        /// </summary>
        public DateTime WhenAdded { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets when the log was updated.
        /// </summary>
        public DateTime WhenLastUpdated { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Converts a database entity to a Data Transfer Object.
        /// </summary>
        /// <param name="value">The database entity.</param>
        /// <param name="includeLines">Whether to include the lines or not.</param>
        /// <param name="converstion">The associated conversation.</param>
        /// <returns>A new <see cref="RobotLog"/> instance containing the required properties.</returns>
        public static RobotLog FromModel(Data.RobotLog value, bool includeLines, Data.Conversation? converstion = null)
        {
            var robotLog = new RobotLog
            {
                ConversationId = value.Conversation.ConversationId,
                UserName = value.Conversation.UserName,
                WhenAdded = value.WhenAdded,
                WhenLastUpdated = value.WhenLastUpdated
            };
            if (converstion != null)
            {
                robotLog.ConversationId = converstion.ConversationId;
                robotLog.UserName = converstion.UserName;
            }
            if (includeLines)
            {
                robotLog.Lines = value.Lines.Select(RobotLogLine.FromModel).ToList();
            }

            return robotLog;
        }
    }
}
