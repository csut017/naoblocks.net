using NaoBlocks.Common;
using NaoBlocks.Engine.Data;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// A command for storing a program snapshot for a user.
    /// </summary>
    public class StoreSnapshot
        : UserCommandBase
    {
        private User? user;

        /// <summary>
        /// Gets or sets the source of the snapshot.
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// Gets or sets the current state of the snapshot.
        /// </summary>
        public string? State { get; set; }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// Gets the named values.
        /// </summary>
        public IList<NamedValue> Values { get; } = new List<NamedValue>();

        /// <summary>
        /// Validates the snapshot.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during validation.</returns>
        public async override Task<IEnumerable<CommandError>> ValidateAsync(IDatabaseSession session)
        {
            var errors = new List<CommandError>();
            if (string.IsNullOrWhiteSpace(this.State))
            {
                errors.Add(this.GenerateError($"State is required"));
            }

            if (string.IsNullOrWhiteSpace(this.UserName))
            {
                errors.Add(this.GenerateError($"Username is required"));
            }

            if (!errors.Any())
            {
                this.user = await this.ValidateAndRetrieveUser(session, this.UserName, UserRole.User, errors);
            }

            return errors.AsEnumerable();
        }

        /// <summary>
        /// Stores the snapshot in the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>A <see cref="CommandResult"/> containing the results of execution.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the command has not been validated.</exception>
        protected async override Task<CommandResult> DoExecuteAsync(IDatabaseSession session)
        {
            ValidateExecutionState(this.user?.Id);
            var snapshot = new Snapshot
            {
                Source = string.IsNullOrWhiteSpace(this.Source) ? "Unknown" : this.Source,
                State = this.State!,
                User = this.user,
                UserId = this.user!.Id!,
                WhenAdded = this.WhenExecuted
            };
            foreach (var value in this.Values)
            {
                snapshot.Values.Add(value);
            }

            await session.StoreAsync(snapshot).ConfigureAwait(false);

            return CommandResult.New(this.Number, snapshot);
        }

        /// <summary>
        /// Attempts to restore the command from the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during restoration.</returns>
        public async override Task<IEnumerable<CommandError>> RestoreAsync(IDatabaseSession session)
        {
            var errors = new List<CommandError>();
            this.user = await this.ValidateAndRetrieveUser(session, this.UserName, UserRole.User, errors).ConfigureAwait(false);
            return errors.AsEnumerable();
        }
    }
}
