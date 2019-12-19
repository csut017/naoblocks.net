using System;

namespace NaoBlocks.Core.Commands
{
    public class CommandLog
    {
        public CommandBase Command { get; set; }

        public CommandResult Result { get; set; }

        public string Type { get; set; }

        public DateTime WhenApplied { get; set; }
    }
}