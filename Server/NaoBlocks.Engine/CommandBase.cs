using NaoBlocks.Common;
using Newtonsoft.Json;

namespace NaoBlocks.Engine
{
    /// <summary>
    /// Base class that all commands need to implement.
    /// </summary>
    /// <remarks>
    /// This class provides the basic infrastructure for validating and executing a command.
    /// </remarks>
    public abstract class CommandBase
    {
        /// <summary>
        /// Gets or sets the unique identifier of the command.
        /// </summary>
        [JsonIgnore]
        public string? Id { get; set; }

        /// <summary>
        /// Gets or sets the command number.
        /// </summary>
        public int Number { get; set; } = 0;

        /// <summary>
        /// Gets or sets when the command was executed.
        /// </summary>
        [JsonIgnore]
        public DateTime WhenExecuted { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Attempts to execute the command.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>The result of the execution.</returns>
        public async Task<CommandResult> ExecuteAsync(IDatabaseSession session)
        {
            try
            {
                return await this.DoExecuteAsync(session).ConfigureAwait(false);
            }
            catch (InvalidCallOrderException)
            {
                // These exceptions should be passed onto the engine to handle
                throw;
            }
            catch (Exception error)
            {
                return new CommandResult(this.Number, $"Unexpected error: {error.Message}");
            }
        }

        /// <summary>
        /// Checks if the command can be rolled back.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>True if the command can be rolled back (reversed), false otherwise.</returns>
        public virtual Task<bool> CheckCanRollbackAsync(IDatabaseSession session)
        {
            return Task.FromResult(false);
        }

        /// <summary>
        /// Rolls back (reverses) a command.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>The result from reversing the command.</returns>
        public virtual Task<CommandResult> RollBackAsync(IDatabaseSession session)
        {
            return Task.FromResult(new CommandResult(this.Number, "Command does not allow rolling back"));
        }

        /// <summary>
        /// Attempts to validate the command.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>The errors from validation. Empty if there are no errors.</returns>
        public virtual Task<IEnumerable<CommandError>> ValidateAsync(IDatabaseSession session)
        {
            return Task.FromResult(new List<CommandError>().AsEnumerable());
        }

        /// <summary>
        /// Performs the actual execution. This method must be implemented in all derived classes.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>The result of execution.</returns>
        protected abstract Task<CommandResult> DoExecuteAsync(IDatabaseSession session);

        /// <summary>
        /// Generates a new result.
        /// </summary>
        /// <returns>The new <see cref="CommandResult"/> instance.</returns> 
        protected virtual CommandResult GenerateResult()
        {
            return new CommandResult(this.Number);
        }

        /// <summary>
        /// Generates an error.
        /// </summary>
        /// <param name="error">The error message.</param>
        /// <returns>The new <see cref="CommandError"/> instance.</returns>
        protected CommandError GenerateError(string error)
        {
            return new CommandError(this.Number, error);
        }
    }

    /// <summary>
    /// A <see cref="CommandBase"/> that returns a data result.
    /// </summary>
    /// <typeparam name="T">The type of data.</typeparam>
    public abstract class CommandBase<T>
        : CommandBase
        where T : class
    {
        /// <summary>
        /// Allows a derived class to set the result.
        /// </summary>
        /// <param name="value">The new result.</param>
        /// <returns>A <see cref="CommandResult{T}"/> containing the result data.</returns>
        protected CommandResult<T> Result(T value)
        {
            return new CommandResult<T>(this.Number, value);
        }
    }
}