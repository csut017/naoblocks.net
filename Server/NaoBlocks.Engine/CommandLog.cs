﻿namespace NaoBlocks.Engine
{
    /// <summary>
    /// A log of how a command was executed.
    /// </summary>
    public class CommandLog
    {
        /// <summary>
        /// Gets or sets the command that was executed.
        /// </summary>
        public CommandBase? Command { get; set; }

        /// <summary>
        /// Gets or sets the result of execution.
        /// </summary>
        public CommandResult? Result { get; set; }

        /// <summary>
        /// Gets or sets a human-readbale string that contains the name of the command.
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets then the command was executed.
        /// </summary>
        public DateTime WhenApplied { get; set; } = DateTime.UtcNow;
    }
}