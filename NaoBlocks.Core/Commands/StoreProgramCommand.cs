using NaoBlocks.Core.Models;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Core.Commands
{
    public class StoreProgramCommand
        : CommandBase<CodeProgram>
    {
        public string? Code { get; set; }

        public string? UserId { get; set; }

        public async override Task<IEnumerable<CommandError>> ValidateAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var errors = new List<CommandError>();
            if (string.IsNullOrWhiteSpace(this.UserId))
            {
                errors.Add(this.Error($"UserID is required for storing a program"));
            }

            if (string.IsNullOrWhiteSpace(this.Code))
            {
                errors.Add(this.Error($"Code is required for storing a program"));
            }

            if (!errors.Any())
            {
                var userExists = await session.Query<User>().AnyAsync(u => u.Id == this.UserId).ConfigureAwait(false);
                if (!userExists)
                {
                    errors.Add(this.Error($"User does not exist"));
                }
            }

            return errors.AsEnumerable();
        }

        protected override async Task<CommandResult> DoApplyAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            var program = new CodeProgram
            {
                UserId = this.UserId ?? "<Unknown>",
                Code = this.Code ?? string.Empty
            };
            await session.StoreAsync(program).ConfigureAwait(false);
            return this.Result(program);
        }
    }
}