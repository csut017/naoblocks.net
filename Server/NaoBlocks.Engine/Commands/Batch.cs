using NaoBlocks.Common;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// A batch of commands that need to execute together.
    /// </summary>
    public class Batch
        : CommandBase
    {
        /// <summary>
        /// Initialise a new <see cref="Batch"/> instance.
        /// </summary>
        /// <param name="commands">The commands to batch.</param>
        public Batch(params CommandBase[] commands)
        {
            this.Commands = new List<CommandBase>(commands);
        }

        /// <summary>
        /// Gets or sets the commands to execute.
        /// </summary>
        public IList<CommandBase> Commands { get; set; }

        /// <summary>
        /// Checks all the batch commands are valid.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>The errors from child commands.</returns>
        /// <param name="engine">The <see cref="IExecutionEngine"/> to use.</param>
        public async override Task<IEnumerable<CommandError>> ValidateAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            var errors = new List<CommandError>();
            var number = 0;
            foreach (var command in this.Commands)
            {
                command.Number = number++;
                errors.AddRange(await command.ValidateAsync(session, engine).ConfigureAwait(false));
            }

            return errors.AsEnumerable();
        }

        /// <summary>
        /// Executes all the commands in the batch/
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>The overall result of the execution.</returns>
        /// <param name="engine">The <see cref="IExecutionEngine"/> to use.</param>
        protected override async Task<CommandResult> DoExecuteAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            var results = new List<CommandResult>();
            foreach (var command in this.Commands)
            {
                var result = await command.ExecuteAsync(session, engine).ConfigureAwait(false);
                results.Add(result);
            }
            var finalResult = new BatchCommandResult(this.Number, results);
            return finalResult;
        }
    }
}
