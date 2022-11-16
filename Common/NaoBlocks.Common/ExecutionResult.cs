namespace NaoBlocks.Common
{
    /// <summary>
    /// The result of a common execution.
    /// </summary>
    public class ExecutionResult
    {
        /// <summary>
        /// Gets or sets the errors that occurred during execution.
        /// </summary>
        public IEnumerable<CommandError> ExecutionErrors { get; set; } = new List<CommandError>();

        /// <summary>
        /// Returns true if execution was successful (no errors), or false otherwise.
        /// </summary>
        public bool Successful
        {
            get
            {
                if (this.ValidationErrors.Any()) return false;
                if (this.ExecutionErrors.Any()) return false;
                return true;
            }
        }

        /// <summary>
        /// Gets or sets the errors that occurred during validation.
        /// </summary>
        public IEnumerable<CommandError> ValidationErrors { get; set; } = new List<CommandError>();

        /// <summary>
        /// Generates a new <see cref="ExecutionResult{TOutput}"/> with data.
        /// </summary>
        /// <typeparam name="TOutput">The type of data returned.</typeparam>
        /// <param name="value">The data value to return.</param>
        /// <returns>The new <see cref="ExecutionResult{TOutput}"/> instance.</returns>
        public static ExecutionResult<TOutput> New<TOutput>(TOutput value)
        {
            return new ExecutionResult<TOutput>
            {
                Output = value
            };
        }
    }

    /// <summary>
    /// An <see cref="ExecutionResult"/> that contains data from the command.
    /// </summary>
    /// <typeparam name="TOutput">The type of data being returned.</typeparam>
    public class ExecutionResult<TOutput>
        : ExecutionResult
    {
        /// <summary>
        /// Gets or sets the data from the command execution.
        /// </summary>
        public TOutput? Output { get; set; }
    }
}
