using NaoBlocks.Common;
using Newtonsoft.Json;
using System.Reflection;

namespace NaoBlocks.Engine
{
    /// <summary>
    /// Base class that all commands need to implement.
    /// </summary>
    /// <remarks>
    /// This class provides the basic infrastructure for validating and executing a command.
    /// </remarks>
    public abstract class CommandBase
        : IDisposable
    {
        /// <summary>
        /// Perform any final clean-up
        /// </summary>
        ~CommandBase()
        {
            Dispose(false);
        }

        /// <summary>
        /// Gets or sets the unique identifier of the command.
        /// </summary>
        [JsonIgnore]
        public string? Id { get; set; }

        /// <summary>
        /// Gets or sets whether any validation errors should be ignored.
        /// </summary>
        [JsonIgnore]
        public bool IgnoreValidationErrors { get; set; }

        /// <summary>
        /// Gets or sets the command number.
        /// </summary>
        public int Number { get; set; } = 0;

        /// <summary>
        /// Gets or sets whether validation failed.
        /// </summary>
        [JsonIgnore]
        public bool ValidationFailed { get; set; }

        /// <summary>
        /// Gets or sets when the command was executed.
        /// </summary>
        [JsonIgnore]
        public DateTime WhenExecuted { get; set; } = DateTime.UtcNow;

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
        /// Checks if this command is for a specified target subsystem.
        /// </summary>
        /// <param name="target">The target subsystem to check.</param>
        /// <returns>True if the command is for a sub-system; false otherwise.</returns>
        public virtual bool CheckForTarget(CommandTarget target)
        {
            var attributes = this.GetType().GetCustomAttributes<CommandTargetAttribute>(true);
            var hasTarget = attributes.Any(a => a.Target == target);
            return hasTarget;
        }

        /// <summary>
        /// Disposes of any resources.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Attempts to execute the command.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>The result of the execution.</returns>
        /// <param name="engine">The <see cref="IExecutionEngine"/> to use.</param>
        public async Task<CommandResult> ExecuteAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            try
            {
                return await this.DoExecuteAsync(session, engine).ConfigureAwait(false);
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
        /// Attempts to restore the command from a saved state.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>The errors from restoring the state. Empty if there are no errors.</returns>
        public virtual Task<IEnumerable<CommandError>> RestoreAsync(IDatabaseSession session)
        {
            return Task.FromResult(new List<CommandError>().AsEnumerable());
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
        /// <param name="engine">The <see cref="IExecutionEngine"/> to use.</param>
        /// <returns>The errors from validation. Empty if there are no errors.</returns>
        public virtual Task<IEnumerable<CommandError>> ValidateAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            return Task.FromResult(new List<CommandError>().AsEnumerable());
        }

        /// <summary>
        /// Validates that a state object has been set prior to execution.
        /// </summary>
        /// <param name="state">The state object to check.</param>
        /// <exception cref="InvalidOperationException">Thrown if the state variable is null.</exception>
        protected static void ValidateExecutionState(params object?[] state)
        {
            foreach (object? entity in state)
            {
                if (entity == null)
                {
                    throw new InvalidOperationException("Command is not in a valid state. Need to call either ValidateAsync or RestoreAsync");
                }
            }
        }

        /// <summary>
        /// Perform any final cleanup.
        /// </summary>
        /// <param name="disposing">Whether to clean up managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        /// Performs the actual execution. This method must be implemented in all derived classes.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <param name="engine">The <see cref="IExecutionEngine"/> to use.</param>
        /// <returns>The result of execution.</returns>
        protected abstract Task<CommandResult> DoExecuteAsync(IDatabaseSession session, IExecutionEngine engine);

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
}