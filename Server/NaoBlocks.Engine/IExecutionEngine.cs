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
        /// Commits (saves) the results of the execution.
        /// </summary>
        Task CommitAsync();

        /// <summary>
        /// Dehydrates the command logs in a period of time for the specified target systems.
        /// </summary>
        /// <param name="fromTime">The starting date and time.</param>
        /// <param name="toTime">The finishing date and time.</param>
        /// <param name="targets">The sub-systems to include.</param>
        /// <returns>A enumerable of strings containing the dehydrated command logs.</returns>
        IAsyncEnumerable<string> DehydrateCommandLogsAsync(DateTime fromTime, DateTime toTime, params CommandTarget[] targets);

        /// <summary>
        /// Executes a command.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <returns>The result of executing the command.</returns>
        Task<CommandResult> ExecuteAsync(CommandBase command);

        /// <summary>
        /// Retrieves a report generator.
        /// </summary>
        /// <typeparam name="TGenerator">The type of generator to retrieve.</typeparam>
        /// <returns>An instance of the generator.</returns>
        TGenerator Generator<TGenerator>()
           where TGenerator : ReportGenerator, new();

        /// <summary>
        /// Retrieves a database query.
        /// </summary>
        /// <typeparam name="TQuery">The type of query to retrieve.</typeparam>
        /// <returns>An instance of the query.</returns>
        TQuery Query<TQuery>()
            where TQuery : DataQuery, new();

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