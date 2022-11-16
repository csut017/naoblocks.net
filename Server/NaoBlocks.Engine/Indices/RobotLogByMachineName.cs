using NaoBlocks.Engine.Data;
using Raven.Client.Documents.Indexes;

namespace NaoBlocks.Engine.Indices
{
    /// <summary>
    /// Index for retrieving robot logs by the machine name of the robot.
    /// </summary>
    public class RobotLogByMachineName 
        : AbstractIndexCreationTask<RobotLog>
    {
        /// <summary>
        /// Initialises a new <see cref="RobotLogByMachineName"/> instance.
        /// </summary>
        public RobotLogByMachineName()
        {
            this.Map = logs => from log in logs
                               select new
                               {
                                   log.Conversation.ConversationId,
                                   LoadDocument<Robot>(log.RobotId).MachineName,
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
            /// Gets or sets the machine name of the robot generating the log.
            /// </summary>
            public string MachineName { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets when the log was added.
            /// </summary>
            public DateTime WhenAdded { get; set; }
        }
    }
}
