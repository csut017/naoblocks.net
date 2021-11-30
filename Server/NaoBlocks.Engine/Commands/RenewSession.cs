using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using Newtonsoft.Json;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// A command for renewing an existing user session.
    /// </summary>
    public class RenewSession
        : CommandBase
    {
        private Session? session;

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// Validates the user and checks there is a session.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during validation.</returns>
        /// <param name="engine"></param>
        public async override Task<IEnumerable<CommandError>> ValidateAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            var errors = new List<CommandError>();
            if (string.IsNullOrWhiteSpace(this.UserName))
            {
                errors.Add(this.GenerateError($"User name is required"));
            }

            if (!errors.Any())
            {
                var user = await session.Query<User>()
                    .FirstOrDefaultAsync(u => u.Name == this.UserName)
                    .ConfigureAwait(false);
                if (user == null)
                {
                    errors.Add(this.GenerateError("Unknown or invalid user"));
                }
                else
                {
                    this.session = await session.Query<Session>()
                        .FirstOrDefaultAsync(u => u.UserId == user.Id && u.WhenExpires > this.WhenExecuted)
                        .ConfigureAwait(false);
                    if (this.session == null)
                    {
                        errors.Add(this.GenerateError("User does not have a current session"));
                    }
                }
            }

            return errors.AsEnumerable();
        }

        /// <summary>
        /// Stores the program in the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>A <see cref="CommandResult"/> containing the results of execution.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the command has not been validated.</exception>
        /// <param name="engine"></param>
        protected override Task<CommandResult> DoExecuteAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            ValidateExecutionState(this.session);
            var now = this.WhenExecuted;
            this.session!.WhenExpires = now.AddDays(1);
            return Task.FromResult(CommandResult.New(this.Number, this.session));
        }

        /// <summary>
        /// Attempts to restore the command from the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during restoration.</returns>
        public async override Task<IEnumerable<CommandError>> RestoreAsync(IDatabaseSession session)
        {
            var errors = new List<CommandError>();
            var user = await session.Query<User>()
                .FirstOrDefaultAsync(u => u.Name == this.UserName)
                .ConfigureAwait(false);
            if (user == null)
            {
                errors.Add(this.GenerateError("Unable to retrieve user"));
            }
            else
            {
                this.session = await session.Query<Session>()
                    .FirstOrDefaultAsync(u => u.UserId == user.Id && u.WhenExpires > this.WhenExecuted)
                    .ConfigureAwait(false);
                if (this.session == null)
                {
                    errors.Add(this.GenerateError("Unable to retrieve session"));
                }
            }
            return errors.AsEnumerable();
        }
    }
}
