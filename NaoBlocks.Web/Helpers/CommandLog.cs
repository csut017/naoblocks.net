using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Web.Helpers
{
    public class CommandLog
    {
        public CommandBase Command { get; set; }

        public string Type { get; set; }

        public CommandResult Result { get; set; }

        public DateTime WhenApplied { get; set; }
    }
}
