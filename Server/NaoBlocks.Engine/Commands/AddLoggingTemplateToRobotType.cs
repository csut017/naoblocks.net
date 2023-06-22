using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// A command for adding a logging template to a robot type.
    /// </summary>
    [CommandTarget(CommandTarget.RobotType)]
    public class AddLoggingTemplateToRobotType
        : CommandBase
    {
        private RobotType? robotType;

        /// <summary>
        /// Gets or sets the category (text to match.)
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// Gets or sets the message type.
        /// </summary>
        public ClientMessageType MessageType { get; set; } = ClientMessageType.Unknown;

        /// <summary>
        /// Gets or sets the name of the robot type.
        /// </summary>
        public string? RobotTypeName { get; set; }

        /// <summary>
        /// Gets or sets the text (text to include in log.)
        /// </summary>
        public string? Text { get; set; }

        /// <summary>
        /// Gets or sets the value names (name of data.)
        /// </summary>
        public string[]? ValueNames { get; set; }

        /// <summary>
        /// Attempts to restore the command from the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during restoration.</returns>
        public override async Task<IEnumerable<CommandError>> RestoreAsync(IDatabaseSession session)
        {
            var errors = new List<CommandError>();
            this.robotType = await session.Query<RobotType>()
                .FirstOrDefaultAsync(rt => rt.Name == this.RobotTypeName)
                .ConfigureAwait(false);
            if (this.robotType == null)
            {
                errors.Add(this.GenerateError($"Robot Type '{this.RobotTypeName}' does not exist"));
            }
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
            if (string.IsNullOrWhiteSpace(this.Category))
            {
                errors.Add(this.GenerateError($"Category is required"));
            }

            if (string.IsNullOrWhiteSpace(this.Text))
            {
                this.Text = this.Category;
            }

            if (string.IsNullOrWhiteSpace(this.RobotTypeName))
            {
                errors.Add(this.GenerateError($"Robot Type Name is required"));
            }

            if (this.MessageType == ClientMessageType.Unknown)
            {
                errors.Add(this.GenerateError($"Message Type is required"));
            }

            if (!errors.Any())
            {
                this.robotType = await session.Query<RobotType>()
                    .FirstOrDefaultAsync(rt => rt.Name == this.RobotTypeName)
                    .ConfigureAwait(false);
                if (this.robotType == null)
                {
                    errors.Add(this.GenerateError($"Robot Type '{this.RobotTypeName}' does not exist"));
                }
                else
                {
                    var exists = this.robotType
                        .LoggingTemplates
                        .Any(lt => lt.Category.Equals(this.Category, StringComparison.InvariantCultureIgnoreCase));
                    if (exists)
                    {
                        errors.Add(this.GenerateError($"Category '{this.Category}' already exists"));
                    }
                }
            }

            this.ValueNames ??= Array.Empty<string>();
            return errors.AsEnumerable();
        }

        /// <summary>
        /// Adds the robot to the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>A <see cref="CommandResult"/> containing the results of execution.</returns>
        /// <param name="engine"></param>
        protected override Task<CommandResult> DoExecuteAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            ValidateExecutionState(this.robotType);
            this.robotType!.LoggingTemplates.Add(
                new LoggingTemplate
                {
                    Category = this.Category!,
                    Text = this.Text!,
                    MessageType = this.MessageType,
                    ValueNames = this.ValueNames!,
                });
            this.robotType.WhenLastUpdated = this.WhenExecuted;
            return Task.FromResult(
                CommandResult.New(this.Number, this.robotType!));
        }
    }
}