using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// Command for adding a new robot type.
    /// </summary>
    public class AddRobotType
        : CommandBase
    {
        /// <summary>
        /// Gets or sets the name of the robot type.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Validates the values for the new robot type.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during validation.</returns>
        public async override Task<IEnumerable<CommandError>> ValidateAsync(IDatabaseSession session)
        {
            var errors = new List<CommandError>();
            if (string.IsNullOrWhiteSpace(this.Name))
            {
                errors.Add(this.GenerateError($"Name is required for a robot type"));
            }

            if (!errors.Any() && await session.Query<RobotType>().AnyAsync(s => s.Name == this.Name).ConfigureAwait(false))
            {
                errors.Add(this.GenerateError($"Robot type with name {this.Name} already exists"));
            }

            return errors.AsEnumerable();
        }

        /// <summary>
        /// Adds the robot type to the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>A <see cref="CommandResult"/> containing the results of execution.</returns>
        protected override async Task<CommandResult> DoExecuteAsync(IDatabaseSession session)
        {
            var robot = new RobotType
            {
                Name = this.Name!.Trim(),
                IsDefault = false,
                WhenAdded = this.WhenExecuted
            };

            await session.StoreAsync(robot).ConfigureAwait(false);
            return CommandResult.New(this.Number, robot);
        }
    }
}
