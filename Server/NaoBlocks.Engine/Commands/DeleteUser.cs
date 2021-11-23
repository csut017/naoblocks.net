using NaoBlocks.Common;
using NaoBlocks.Engine.Data;

namespace NaoBlocks.Engine.Commands
{
    public class DeleteUser
        : UserCommandBase
    {
        private User? person;

        /// <summary>
        /// Gets or sets the user's name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the user's role.
        /// </summary>
        public UserRole? Role { get; set; }

        /// <summary>
        /// Attempts to retrieve the existing user.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during validation.</returns>
        public async override Task<IEnumerable<CommandError>> ValidateAsync(IDatabaseSession session)
        {
            var errors = new List<CommandError>();
            this.person = await this.ValidateAndRetrieveUser(session, this.Name, this.Role, errors)
                .ConfigureAwait(false);
            return errors.AsEnumerable();
        }

        /// <summary>
        /// Deletes the user.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>A <see cref="CommandResult"/> containing the results of execution.</returns>
        protected override Task<CommandResult> DoExecuteAsync(IDatabaseSession session)
        {
            session.Delete(this.person);
            return Task.FromResult(CommandResult.New(this.Number));
        }
    }
}
