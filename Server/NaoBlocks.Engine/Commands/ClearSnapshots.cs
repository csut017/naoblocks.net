using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// A command for clearing all program snapshots for a user.
    /// </summary>
    public class ClearSnapshots
        : UserCommandBase
    {
        private User? user;

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        public string? UserName { get; set; }


        /// <summary>
        /// Validates the snapshot.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during validation.</returns>
        /// <param name="engine"></param>
        public async override Task<IEnumerable<CommandError>> ValidateAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            var errors = new List<CommandError>();
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
        /// <param name="engine"></param>
        protected async override Task<CommandResult> DoExecuteAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            ValidateExecutionState(this.user);
            var snapshots = await session.Query<Snapshot>()
                .Where(cp => cp.UserId == this.user!.Id)
                .ToListAsync()
                .ConfigureAwait(false);
            foreach (var snapshot in snapshots)
            {
                session.Delete(snapshot);
            }

            return CommandResult.New(this.Number);
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
