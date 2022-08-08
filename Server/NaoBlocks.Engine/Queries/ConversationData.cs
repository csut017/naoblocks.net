using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Indices;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

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

        /// <summary>
        /// Attempts to retrieve the logs for a robot.
        /// </summary>
        /// <param name="machineName">The machine name of the robot.</param>
        /// <param name="pageNum">The page number to retrieve.</param>
        /// <param name="pageSize">The number of logs in the page.</param>
        /// <returns>The <see cref="RobotLog"/> instances.</returns>
        public virtual async Task<ListResult<RobotLog>> RetrieveRobotLogsPageAsync(string machineName, int pageNum, int pageSize)
        {
            if (this.Session.Query<RobotLogByMachineName.Result, RobotLogByMachineName>() is not IRavenQueryable<RobotLogByMachineName.Result> query)
            {
                return new ListResult<RobotLog>();
            }

            var logs = await query
                .Statistics(out QueryStatistics stats)
                .Where(rl => rl.MachineName == machineName)
                .OfType<RobotLog>()
                .OrderByDescending(rl => rl.WhenAdded)
                .Skip(pageNum * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return ListResult.New(logs, stats.TotalResults, pageNum);
        }
    }
}