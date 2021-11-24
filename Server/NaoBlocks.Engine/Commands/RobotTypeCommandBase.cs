using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// Abstract <see cref="CommandBase"/> that provides helper functionality for working with robot types.
    /// </summary>
    public abstract class RobotTypeCommandBase
        : CommandBase
    {
        /// <summary>
        /// Attempt to retrieve and validate a robot type.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <param name="name">The robot type name.</param>
        /// <param name="errors">Where to append any validation errors.</param>
        /// <returns>The <see cref="User"/> instance, if valid, or null otherwise.</returns>
        protected async Task<RobotType?> ValidateAndRetrieveRobotType(IDatabaseSession session, string? name, List<CommandError> errors)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                errors.Add(this.GenerateError($"Robot type name is required"));
            }

            RobotType? robotType = null;
            if (!errors.Any())
            {
                robotType = await session.Query<RobotType>()
                                .FirstOrDefaultAsync(u => u.Name == name)
                                .ConfigureAwait(false);
                if (robotType == null) errors.Add(this.GenerateError($"Robot type {name} does not exist"));
            }

            return robotType;
        }
    }
}