using NaoBlocks.Engine;

namespace NaoBlocks.Web.Communications
{
    /// <summary>
    /// A default engine factory that generates database sessions from a database instance.
    /// </summary>
    public class DefaultEngineFactory
        : IEngineFactory
    {
        private readonly IDatabase database;
        private readonly ILogger<ExecutionEngine> logger;

        /// <summary>
        /// Initialises a new <see cref="DefaultEngineFactory"/> instance.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="logger">The logger.</param>
        public DefaultEngineFactory(IDatabase database, ILogger<ExecutionEngine> logger)
        {
            this.database = database;
            this.logger = logger;
        }

        /// <summary>
        /// Generate a new <see cref="IExecutionEngine"/> instance.
        /// </summary>
        /// <returns>The new <see cref="IExecutionEngine"/> and <see cref="IDatabaseSession"/>instances.</returns>
        public (IExecutionEngine, IDatabaseSession) Initialise()
        {
            var session = this.database.StartSession();
            var engine = new ExecutionEngine(this.database, session, this.logger);
            return (engine, session);
        }
    }
}
