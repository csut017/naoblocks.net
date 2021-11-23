using Microsoft.Extensions.Logging;
using NaoBlocks.Common;

namespace NaoBlocks.Engine
{
    /// <summary>
    /// A command manager is responsible for validating and applying commands.
    /// </summary>
    public interface IExecutionEngine
    {
        /// <summary>
        /// Gets or sets the logger to use.
        /// </summary>
        ILogger Logger { get; }

        /// <summary>
        /// Executes a command.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <returns>The result of executing the command.</returns>
        Task<CommandResult> ExecuteAsync(CommandBase command);

        /// <summary>
        /// Commits (saves) the results of the execution.
        /// </summary>
        Task CommitAsync();

        /// <summary>
        /// Restores a command from the database.
        /// </summary>
        /// <param name="command">The command to restore.</param>
        /// <returns>Any errors that occurred during restoration.</returns>
        /// <remarks>
        /// If errors is empty, then the command is assumed to be restored.
        /// </remarks>
        Task<IEnumerable<CommandError>> RestoreAsync(CommandBase command);

        /// <summary>
        /// Validates a command.
        /// </summary>
        /// <param name="command">The command to validate.</param>
        /// <returns>Any errors that occurred during validation.</returns>
        /// <remarks>
        /// If errors is empty, then the command is assumed to be validated.
        /// </remarks>
        Task<IEnumerable<CommandError>> ValidateAsync(CommandBase command);
    }
}
