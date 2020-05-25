using NaoBlocks.Core.Models;
using Raven.Client.Documents.Indexes;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace NaoBlocks.Web.Indexes
{
    public class RobotLogByMachineName : AbstractIndexCreationTask<RobotLog>
    {
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

        [SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Index result class")]
        public class Result
        {
            public long ConversationId { get; set; }

            public string MachineName { get; set; } = string.Empty;

            public DateTime WhenAdded { get; set; }
        }
    }
}