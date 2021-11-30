using NaoBlocks.Common;
using NaoBlocks.Engine.Data;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// Command for adding a blockset to a robot type.
    /// </summary>
    public class AddBlockSet
        : RobotTypeCommandBase
    {
        private RobotType? robotType;

        /// <summary>
        /// Gets or sets the name of the blockset.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the robot type.
        /// </summary>
        public string? RobotType { get; set; }

        /// <summary>
        /// Gets or sets the categories.
        /// </summary>
        public string? Categories { get; set; }

        /// <summary>
        /// Validates the user and generates the token.
        /// </summary>
        /// <param name="session">The database session.</param>
        /// <returns>Any valdiation errors.</returns>
        /// <param name="engine"></param>
        public async override Task<IEnumerable<CommandError>> ValidateAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            var errors = new List<CommandError>();
            if (string.IsNullOrWhiteSpace(this.Name))
            {
                errors.Add(this.GenerateError($"Name is required for a block set"));
            }

            if (string.IsNullOrWhiteSpace(this.Categories))
            {
                errors.Add(this.GenerateError($"Categories is required for a block set"));
            }

            if (string.IsNullOrWhiteSpace(this.RobotType))
            {
                errors.Add(this.GenerateError($"Robot Type is required for a block set"));
            }

            if (!errors.Any())
            {
                this.robotType = await this.ValidateAndRetrieveRobotType(session, this.RobotType, errors);
            }

            return errors.AsEnumerable();
        }

        /// <summary>
        /// Adds the blockset to the robot type.
        /// </summary>
        /// <param name="session">The database session.</param>
        /// <returns>A <see cref="CommandResult"/> containing the asbtract syntax tree.</returns>
        /// <param name="engine"></param>
        protected override Task<CommandResult> DoExecuteAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            ValidateExecutionState(this.robotType);
            var set = new BlockSet
            {
                Name = (this.Name!).Trim(),
                BlockCategories = this.Categories!
            };

            this.robotType!.BlockSets.Add(set);
            return Task.FromResult(CommandResult.New(this.Number, set));
        }

        /// <summary>
        /// Attempts to restore the command from the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during restoration.</returns>
        public async override Task<IEnumerable<CommandError>> RestoreAsync(IDatabaseSession session)
        {
            var errors = new List<CommandError>();
            this.robotType = await this.ValidateAndRetrieveRobotType(session, this.RobotType, errors).ConfigureAwait(false);
            return errors.AsEnumerable();
        }
    }
}
