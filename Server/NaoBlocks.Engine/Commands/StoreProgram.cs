using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using Raven.Client.Documents;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// A command for storing a program.
    /// </summary>
    public class StoreProgram
        : UserCommandBase
    {
        private User? user;

        /// <summary>
        /// Gets or sets the code to store.
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// Gets or sets the program name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets a flag to check the name.
        /// </summary>
        public bool RequireName { get; set; } = false;

        /// <summary>
        /// Gets or sets the name of the user to associate the program with.
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// Validates the program settings.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during validation.</returns>
        public async override Task<IEnumerable<CommandError>> ValidateAsync(IDatabaseSession session)
        {
            var errors = new List<CommandError>();
            if (string.IsNullOrWhiteSpace(this.Code))
            {
                errors.Add(this.GenerateError($"Code is required for storing a program"));
            }

            if (this.RequireName && string.IsNullOrWhiteSpace(this.Name))
            {
                errors.Add(this.GenerateError($"Name is required for storing a program"));
            }

            if (string.IsNullOrWhiteSpace(this.UserName))
            {
                errors.Add(this.GenerateError($"User name is required"));
            }

            if (!errors.Any())
            {
                this.user = await this.ValidateAndRetrieveUser(session, this.UserName, UserRole.User, errors).ConfigureAwait(false);
            }

            return errors.AsEnumerable();
        }

        /// <summary>
        /// Stores the program in the database.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>A <see cref="CommandResult"/> containing the results of execution.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the command has not been validated.</exception>
        protected async override Task<CommandResult> DoExecuteAsync(IDatabaseSession session)
        {
            ValidateExecutionState(this.user);
            CodeProgram? program = null;
            var name = this.Name;
            if (!string.IsNullOrEmpty(name))
            {
                name = name.Trim();
                program = await session.Query<CodeProgram>()
                    .FirstOrDefaultAsync(p => p.Name == name && p.UserId == this.user!.Id)
                    .ConfigureAwait(false);
                if (program != null)
                {
                    program.Code = this.Code!;
                }
            }

            if (program == null)
            {
                program = new CodeProgram
                {
                    Name = name,
                    Code = this.Code!,
                    WhenAdded = this.WhenExecuted,
                    Number = this.user!.NextProgramNumber++,
                    UserId = this.user.Id!
                };
                await session.StoreAsync(program).ConfigureAwait(false);
            }
            return CommandResult.New(this.Number, program);
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
