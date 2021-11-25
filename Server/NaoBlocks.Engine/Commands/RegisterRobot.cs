
using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// A command for registering an unknown robot.
    /// </summary>
    public class RegisterRobot
        : CommandBase
    {
        /// <summary>
        /// Gets or sets the machine name of the robot.
        /// </summary>
        public string? MachineName { get; set; }

        /// <summary>
        /// Validates the robot details.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during validation.</returns>
        public async override Task<IEnumerable<CommandError>> ValidateAsync(IDatabaseSession session)
        {
            var errors = new List<CommandError>();
            if (string.IsNullOrWhiteSpace(this.MachineName))
            {
                errors.Add(this.GenerateError($"Machine name is required for a robot"));
            }

            if (!errors.Any())
            {
                if (await session.Query<Robot>().AnyAsync(s => s.MachineName == this.MachineName).ConfigureAwait(false))
                {
                    errors.Add(this.GenerateError($"Robot with name {this.MachineName} already exists"));
                }
            }

            return errors.AsEnumerable();
        }

        /// <summary>
        /// Adds the robot to the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>A <see cref="CommandResult"/> containing the results of execution.</returns>
        protected override async Task<CommandResult> DoExecuteAsync(IDatabaseSession session)
        {
            var robot = new Robot
            {
                MachineName = this.MachineName ?? "<Unknown>",
                FriendlyName = this.MachineName ?? "<Unknown>",
                IsInitialised = false,
                WhenAdded = this.WhenExecuted
            };
            await session.StoreAsync(robot).ConfigureAwait(false);
            return CommandResult.New(this.Number, robot);
        }
    }
}
