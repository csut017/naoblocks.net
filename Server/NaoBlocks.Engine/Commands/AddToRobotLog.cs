using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// A command for adding a line to a robot log.
    /// </summary>
    [CommandTarget(CommandTarget.RobotLog)]
    public class AddToRobotLog
        : RobotCommandBase
    {
        private Robot? robot;

        /// <summary>
        /// Gets or sets the associated conversation.
        /// </summary>
        public Conversation? Conversation { get; set; }

        /// <summary>
        /// Gets or sets the conversation ID.
        /// </summary>
        public long ConversationId { get; set; }

        /// <summary>
        /// Gets or sets the line description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the machine name of the robot.
        /// </summary>
        public string? MachineName { get; set; }

        /// <summary>
        /// Gets or sets the source message type.
        /// </summary>
        public ClientMessageType SourceMessageType { get; set; } = ClientMessageType.Unknown;

        /// <summary>
        /// Gets or sets whether to use the last conversation ID.
        /// </summary>
        public bool UseLastConversationId { get; set; } = false;

        /// <summary>
        /// Gets the values to associate with the line.
        /// </summary>
        public IList<NamedValue> Values { get; private set; } = new List<NamedValue>();

        /// <summary>
        /// Gets or sets when the log event occurred.
        /// </summary>
        public DateTime? WhenLogged { get; set; }

        /// <summary>
        /// Attempts to restore the command from the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during restoration.</returns>
        public override async Task<IEnumerable<CommandError>> RestoreAsync(IDatabaseSession session)
        {
            var errors = new List<CommandError>();
            this.robot = await this.ValidateAndRetrieveRobot(session, this.MachineName, errors).ConfigureAwait(false);
            if (this.Conversation == null) await this.ValidateAndRetrieveConversation(session, errors).ConfigureAwait(false);
            return errors.AsEnumerable();
        }

        /// <summary>
        /// Validates the robot details.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during validation.</returns>
        /// <param name="engine"></param>
        public override async Task<IEnumerable<CommandError>> ValidateAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            var errors = new List<CommandError>();
            if (string.IsNullOrWhiteSpace(this.MachineName))
            {
                errors.Add(this.GenerateError($"Machine name is required"));
            }

            if (string.IsNullOrWhiteSpace(this.Description))
            {
                errors.Add(this.GenerateError($"Description is required"));
            }

            if (!errors.Any())
            {
                this.robot = await this.ValidateAndRetrieveRobot(session, this.MachineName, errors).ConfigureAwait(false);
                if (this.Conversation == null)
                {
                    if (this.UseLastConversationId)
                    {
                        await ValidateAndRetrieveLastConversation(session, errors).ConfigureAwait(false);
                    }
                    else
                    {
                        await ValidateAndRetrieveConversation(session, errors).ConfigureAwait(false);
                    }
                }
            }

            return errors.AsEnumerable();
        }

        /// <summary>
        /// Adds the robot to the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>A <see cref="CommandResult"/> containing the results of execution.</returns>
        /// <param name="engine"></param>
        protected override async Task<CommandResult> DoExecuteAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            ValidateExecutionState(this.robot, this.Conversation);
            var log = await session.Query<RobotLog>()
                                    .FirstOrDefaultAsync(rl => rl.RobotId == this.robot!.Id && rl.Conversation.ConversationId == this.Conversation!.ConversationId);

            if (log == null)
            {
                log = new RobotLog
                {
                    Conversation = this.Conversation!,
                    RobotId = this.robot!.Id,
                    WhenAdded = this.WhenLogged ?? this.WhenExecuted
                };
                await session.StoreAsync(log);
            }

            var line = new RobotLogLine
            {
                Description = this.Description!,
                SourceMessageType = this.SourceMessageType,
                WhenAdded = this.WhenExecuted
            };
            foreach (var value in this.Values)
            {
                line.Values.Add(value);
            }

            log.Lines.Add(line);
            log.WhenLastUpdated = this.WhenExecuted;

            return CommandResult.New(this.Number, log);
        }

        private async Task ValidateAndRetrieveConversation(IDatabaseSession session, List<CommandError> errors)
        {
            this.Conversation = await session.Query<Conversation>()
                                .FirstOrDefaultAsync(c => c.ConversationId == this.ConversationId);
            if (this.Conversation == null)
            {
                errors.Add(this.GenerateError($"Unknown conversation {this.ConversationId}"));
            }
        }

        private async Task ValidateAndRetrieveLastConversation(IDatabaseSession session, List<CommandError> errors)
        {
            this.Conversation = await session.Query<Conversation>()
                                .OrderByDescending(c => c.ConversationId)
                                .FirstOrDefaultAsync(c => c.SourceType == "Robot" && c.SourceName == this.MachineName);
            if (this.Conversation == null)
            {
                errors.Add(this.GenerateError($"Robot {this.MachineName} does not have any conversations"));
            }
        }
    }
}