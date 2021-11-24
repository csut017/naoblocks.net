using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// Abstract <see cref="CommandBase"/> that provides helper functionality for working with robots.
    /// </summary>
    public abstract class RobotCommandBase
        : CommandBase
    {
        /// <summary>
        /// Attempt to retrieve and validate a robot.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <param name="name">The robot name.</param>
        /// <param name="errors">Where to append any validation errors.</param>
        /// <returns>The <see cref="User"/> instance, if valid, or null otherwise.</returns>
        protected async Task<Robot?> ValidateAndRetrieveRobot(IDatabaseSession session, string? name, List<CommandError> errors)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                errors.Add(this.GenerateError($"Robot name is required"));
            }

            Robot? Robot = null;
            if (!errors.Any())
            {
                Robot = await session.Query<Robot>()
                                .FirstOrDefaultAsync(u => u.MachineName == name)
                                .ConfigureAwait(false);
                if (Robot == null) errors.Add(this.GenerateError($"Robot {name} does not exist"));
            }

            return Robot;
        }
    }
}