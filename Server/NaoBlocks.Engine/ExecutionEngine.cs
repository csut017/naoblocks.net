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
        private readonly Dictionary<Type, DataQuery> queries = new();
        private readonly Dictionary<Type, ReportGenerator> generators = new();

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
            var result = await command.ExecuteAsync(this.session, this).ConfigureAwait(false);
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
        /// Restores a command from the database.
        /// </summary>
        /// <param name="command">The command to restore.</param>
        /// <returns>Any errors that occurred during restoration.</returns>
        /// <remarks>
        /// If errors is empty, then the command is assumed to be restored.
        /// </remarks>
        public async Task<IEnumerable<CommandError>> RestoreAsync(CommandBase command)
        {
            var errors = await command.RestoreAsync(this.session);
            if (errors.Any())
            {
                this.Logger.LogWarning("Unable to restore command");
            }
            else
            {
                this.Logger.LogInformation("Command restored");
            }

            return errors;
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
            var errors = await command.ValidateAsync(this.session, this);
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

        /// <summary>
        /// Retrieves a database query.
        /// </summary>
        /// <typeparam name="TQuery">The type of query to retrieve.</typeparam>
        /// <returns>An instance of the query.</returns>
        public TQuery Query<TQuery>()
            where TQuery : DataQuery, new()
        {
            if (this.queries.TryGetValue(typeof(TQuery), out var query)) return (TQuery)query;
            query = new TQuery();
            query.InitialiseSession(this.session);
            this.queries.Add(typeof(TQuery), query);
            return (TQuery)query;
        }

        /// <summary>
        /// Retrieves a report generator.
        /// </summary>
        /// <typeparam name="TGenerator">The type of generator to retrieve.</typeparam>
        /// <returns>An instance of the generator.</returns>
        public TGenerator Generator<TGenerator>()
            where TGenerator : ReportGenerator, new()
        {
            if (this.generators.TryGetValue(typeof(TGenerator), out var generator)) return (TGenerator)generator;
            generator = new TGenerator();
            generator.InitialiseSession(this.session);
            this.generators.Add(typeof(TGenerator), generator);
            return (TGenerator)generator;
        }
    }
}
