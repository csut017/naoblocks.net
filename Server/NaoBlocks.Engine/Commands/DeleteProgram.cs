using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using Raven.Client.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaoBlocks.Engine.Commands
{
    /// <summary>
    /// A command to delete a program.
    /// </summary>
    public class DeleteProgram
        : UserCommandBase
    {
        private User? person;

        /// <summary>
        /// Gets or sets the program number.
        /// </summary>
        public long? ProgramNumber { get; set; }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// Attempts to retrieve the existing user.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>Any errors that occurred during validation.</returns>
        /// <param name="engine"></param>
        public async override Task<IEnumerable<CommandError>> ValidateAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            var errors = new List<CommandError>();

            if (this.ProgramNumber == null)
            {
                errors.Add(this.GenerateError("Program number is required"));
            }

            if (!errors.Any())
            {
                this.person = await this.ValidateAndRetrieveUser(session, this.UserName, UserRole.User, errors)
                    .ConfigureAwait(false);
            }

            return errors.AsEnumerable();
        }

        /// <summary>
        /// Deletes the user.
        /// </summary>
        /// <param name="session">The database session to use.</param>
        /// <returns>A <see cref="CommandResult"/> containing the results of execution.</returns>
        /// <param name="engine"></param>
        protected override async Task<CommandResult> DoExecuteAsync(IDatabaseSession session, IExecutionEngine engine)
        {
            ValidateExecutionState(this.person);
            var program = await session.Query<CodeProgram>()
                .FirstOrDefaultAsync(p => p.UserId == this.person!.Id && p.Number == this.ProgramNumber)
                .ConfigureAwait(false);
            if (program != null) session.Delete(program);
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
            this.person = await this.ValidateAndRetrieveUser(session, this.UserName, UserRole.User, errors)
                .ConfigureAwait(false);
            return errors.AsEnumerable();
        }
    }
}
