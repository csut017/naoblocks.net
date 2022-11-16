using NaoBlocks.Engine.Data;
using Raven.Client.Documents.Indexes;

namespace NaoBlocks.Engine.Indices
{
    /// <summary>
    /// Index for retrieving robot logs by the type of the robot.
    /// </summary>
    public class RobotLogByRobotTypeId
        : AbstractIndexCreationTask<RobotLog>
    {
        /// <summary>
        /// Initialises a new <see cref="RobotLogByRobotTypeId"/> instance.
        /// </summary>
        public RobotLogByRobotTypeId()
        {
            this.Map = logs => from log in logs
                               select new
                               {
                                   log.Conversation.ConversationId,
                                   LoadDocument<Robot>(log.RobotId).RobotTypeId,
                                   log.WhenAdded
                               };
        }

        /// <summary>
        /// The result when using the index.
        /// </summary>
        public class Result
        {
            /// <summary>
            /// Gets or sets the conversation id the log was added for.
            /// </summary>
            public long ConversationId { get; set; }

            /// <summary>
            /// Gets or sets the type id of the robot generating the log.
            /// </summary>
            public string RobotTypeId { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets when the log was added.
            /// </summary>
            public DateTime WhenAdded { get; set; }
        }
    }
}