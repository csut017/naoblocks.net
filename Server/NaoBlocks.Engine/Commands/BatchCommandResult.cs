using NaoBlocks.Common;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// The result from executing a batch of commands.
    /// </summary>
    public class BatchCommandResult
        : CommandResult<IEnumerable<CommandResult>>
    {
        /// <summary>
        /// Initialises a new <see cref="BatchCommandResult"/> instance from a set of results.
        /// </summary>
        /// <param name="number">The command number.</param>
        /// <param name="results">The results from the includec commands.</param>
        public BatchCommandResult(int number, IEnumerable<CommandResult> results)
            : base(number, results)
        {
            if (!results.All(r => r.WasSuccessful)) this.Error = "One or more commands failed";
        }

        /// <summary>
        /// Generates the errors to pass on.
        /// </summary>
        /// <returns>The errors from both the command and the batched commands.</returns>
        public override IEnumerable<CommandError> ToErrors()
        {
            if (string.IsNullOrEmpty(this.Error) || (this.Output == null))
            {
                return Array.Empty<CommandError>();
            }

            var errors = new List<CommandError>();
            foreach (var result in this.Output.Where(r => !r.WasSuccessful))
            {
                errors.AddRange(result.ToErrors());
            }

            return errors;
        }
    }
}