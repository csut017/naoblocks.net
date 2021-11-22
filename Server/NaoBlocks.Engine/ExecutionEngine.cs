using Microsoft.Extensions.Logging;
using NaoBlocks.Common;

namespace NaoBlocks.Engine
{
    /// <summary>
    /// An engine for executing commands and storing them in a database.
    /// </summary>
    public class ExecutionEngine : IExecutionEngine
    {
        private readonly IDatabaseSession session;
        private readonly IDatabase database;

        /// <summary>
        /// Initialises a new <see cref="ExecutionEngine"/> instance.
        /// </summary>
        /// <param name="database">A reference to the database. Used to start new sessions if required.</param>
        /// <param name="session">A reference to the current database session.</param>
        /// <param name="logger">A logger.</param>
        public ExecutionEngine(IDatabase database, IDatabaseSession session, ILogger<ExecutionEngine> logger)
        {
            this.database = database;
            this.session = session;
            this.Logger = logger;
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        public ILogger Logger { get; private set; }

        /// <summary>
        /// Executes a command and stores the resulting execution log.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <returns>The result of execution.</returns>
        public async Task<CommandResult> ExecuteAsync(CommandBase command)
        {
            var result = await command.ExecuteAsync(this.session).ConfigureAwait(false);
            this.Logger.LogInformation("Command executed");
            var log = new CommandLog
            {
                WhenApplied = command.WhenExecuted,
                Command = command,
                Result = result,
                Type = command.GetType().Name
            };
            if (log.Type.EndsWith("Command", StringComparison.InvariantCulture)) log.Type = log.Type[0..^7];

            // Always store the command log - use a seperate session to ensure it is saved
            using (var logSession = this.database.StartSession())
            {
                await logSession.StoreAsync(log).ConfigureAwait(false);
                await logSession.SaveChangesAsync().ConfigureAwait(false);
                this.Logger.LogTrace("Log saved");
            }

            return result;
        }

        /// <summary>
        /// Saves (commits) the changes to the database.
        /// </summary>
        public async Task CommitAsync()
        {
            await this.session.SaveChangesAsync().ConfigureAwait(false);
            this.Logger.LogTrace("Session saved");
        }

        /// <summary>
        /// Validates a command.
        /// </summary>
        /// <param name="command">The command to validate.</param>
        /// <returns>Any errors that occurred during validation.</returns>
        /// <remarks>
        /// If errors is empty, then the command is assumed to be validated.
        /// </remarks>
        public async Task<IEnumerable<CommandError>> ValidateAsync(CommandBase command)
        {
            var errors = await command.ValidateAsync(this.session);
            if (errors.Any())
            {
                this.Logger.LogWarning("Command failed validation");
            }
            else
            {
                this.Logger.LogInformation("Command validated");
            }
            return errors;
        }
    }
}
