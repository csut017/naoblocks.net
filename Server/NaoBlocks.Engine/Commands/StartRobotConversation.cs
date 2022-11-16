using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// A command for starting a conversation for a robot.
    /// </summary>
    public class StartRobotConversation
        : RobotCommandBase
    {
        private Robot? robot;

        /// <summary>
        /// Gets or sets the robot machine name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the type of conversation.
        /// </summary>
        public ConversationType Type { get; set; } = ConversationType.Unknown;

        /// <summary>
        /// Attempts to restore the command from the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during restoration.</returns>
        public override async Task<IEnumerable<CommandError>> RestoreAsync(IDatabaseSession session)
        {
            var errors = new List<CommandError>();
            this.robot = await this.ValidateAndRetrieveRobot(session, this.Name, errors).ConfigureAwait(false);
            return errors.AsEnumerable();
        }

        /// <summary>
        /// Validates the robot via their machine name.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during validation.</returns>
        /// <param name="engine"></param>
        public override async Task<IEnumerable<CommandError>> ValidateAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            var errors = new List<CommandError>();

            if (string.IsNullOrWhiteSpace(this.Name))
            {
                errors.Add(this.GenerateError($"Name is required"));
            }

            if (!errors.Any())
            {
                this.robot = await this.ValidateAndRetrieveRobot(session, this.Name, errors)
                    .ConfigureAwait(false);
            }

            return errors.AsEnumerable();
        }

        /// <summary>
        /// Stores the program in the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>A <see cref="CommandResult"/> containing the results of execution.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the command has not been validated.</exception>
        /// <param name="engine"></param>
        protected override async Task<CommandResult> DoExecuteAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            ValidateExecutionState(this.robot);

            var systemValues = await session.Query<SystemValues>().FirstOrDefaultAsync();
            if (systemValues == null)
            {
                systemValues = new SystemValues();
                await session.StoreAsync(systemValues);
            }
            var conversationId = ++systemValues.NextConversationId;
            var conversation = new Conversation
            {
                ConversationId = conversationId,
                ConversationType = this.Type,
                SourceId = this.robot!.Id,
                SourceName = this.robot.MachineName,
                SourceType = "Robot"
            };
            await session.StoreAsync(conversation);

            return CommandResult.New(this.Number, conversation);
        }
    }
}