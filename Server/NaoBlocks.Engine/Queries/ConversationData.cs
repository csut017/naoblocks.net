using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Indices;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Queries
{
    /// <summary>
    /// Queries for accessing conversation data.
    /// </summary>
    public class ConversationData
        : DataQuery
    {
        /// <summary>
        /// Retrieve a conversation by their identifier.
        /// </summary>
        /// <param name="id">The conversation id.</param>
        /// <returns>The <see cref="Conversation"/> instance if found, null otherwise.</returns>
        public virtual async Task<Conversation?> RetrieveByIdAsync(long id)
        {
            var result = await this.Session
                .Query<Conversation>()
                .FirstOrDefaultAsync(c => c.ConversationId == id);
            return result;
        }

        /// <summary>
        /// Attempts to retrieve a robot log by the conversation id.
        /// </summary>
        /// <param name="conversationId">The conversation id.</param>
        /// <param name="machineName">The machine name of the robot.</param>
        /// <returns>The <see cref="RobotLog"/> instance if found, null otherwise.</returns>
        public virtual async Task<RobotLog?> RetrieveRobotLogAsync(long conversationId, string machineName)
        {
            var result = await this.Session
                .Query<RobotLogByMachineName.Result, RobotLogByMachineName>()
                .Where(rl => rl.MachineName == machineName && rl.ConversationId == conversationId)
                .OfType<RobotLog>()
                .FirstOrDefaultAsync();
            return result;
        }
    }
}
