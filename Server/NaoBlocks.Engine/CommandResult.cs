using NaoBlocks.Common;

namespace NaoBlocks.Engine
{
    /// <summary>
    /// The result of executing a command.
    /// </summary>
    public class CommandResult
    {
        /// <summary>
        /// Initialises a new <see cref="CommandResult"/> instance.
        /// </summary>
        /// <param name="number">The command number.</param>
        /// <param name="error">An optional string containing any error message.</param>
        public CommandResult(int number, string? error = null)
        {
            this.Number = number;
            this.Error = error;
        }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// Gets the command number.
        /// </summary>
        public int Number { get; private set; }

        /// <summary>
        /// Gets whether the command was successful or not.
        /// </summary>
        public bool WasSuccessful
        {
            get { return string.IsNullOrEmpty(this.Error); }
        }

        /// <summary>
        /// Initialises a new <see cref="CommandResult"/> instance.
        /// </summary>
        /// <param name="number">The command number.</param>
        /// <returns>The new <see cref="CommandResult"/> instance.</returns>
        public static CommandResult New(int number)
        {
            return new CommandResult(number);
        }

        /// <summary>
        /// Initialise a new <see cref="CommandResult{T}"/> instance with data.
        /// </summary>
        /// <typeparam name="T">The type of the data.</typeparam>
        /// <param name="number">The command number.</param>
        /// <param name="value">The data value to contain.</param>
        /// <returns>The new <see cref="CommandResult{T}"/> instance.</returns>
        public static CommandResult New<T>(int number, T value)
            where T : class
        {
            return new CommandResult<T>(number, value);
        }

        /// <summary>
        /// Converts to the data containing version (if possible.)
        /// </summary>
        /// <typeparam name="T">The type of data.</typeparam>
        /// <returns>The <see cref="CommandResult{T}"/> version.</returns>
        public CommandResult<T> As<T>()
            where T : class
        {
            return (CommandResult<T>)this;
        }

        /// <summary>
        /// Extracts all the errors out into an enumerable.
        /// </summary>
        /// <returns>The <see cref="IEnumerable{CommandError}"/> containing the errors.</returns>
        /// <remarks>
        /// If there are no errors, then the enumerable till be empty.
        /// </remarks>
        public virtual IEnumerable<CommandError> ToErrors()
        {
            if (string.IsNullOrEmpty(this.Error))
            {
                return Array.Empty<CommandError>();
            }

            return new[] {
                    new CommandError(this.Number, this.Error)
                };
        }
    }

    /// <summary>
    /// A <see cref="CommandResult"/> that contains data.
    /// </summary>
    /// <typeparam name="T">The type of data.</typeparam>
    public class CommandResult<T> : CommandResult
        where T : class
    {
        /// <summary>
        /// Initialise an empty <see cref="CommandResult{T}"/> instance.
        /// </summary>
        /// <param name="number">The command number.</param>
        /// <param name="error">An optional string containing any error message.</param>
        public CommandResult(int number, string? error = null)
            : base(number, error)
        {
        }

        /// <summary>
        /// Initialise a <see cref="CommandResult{T}"/> instance with data.
        /// </summary>
        /// <param name="number">The command number.</param>
        /// <param name="output">The data for the result.</param>
        public CommandResult(int number, T output)
            : base(number)
        {
            this.Output = output;
        }

        /// <summary>
        /// Gets or sets the result data.
        /// </summary>
        public T? Output { get; set; }
    }
}