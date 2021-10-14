using NaoBlocks.Common;
using NaoBlocks.Core.Commands.Helpers;
using NaoBlocks.Core.Models;
using Newtonsoft.Json;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Core.Commands
{
    public class StoreProgram
        : CommandBase<CodeProgram>
    {
        public string? Code { get; set; }

        public string? Name { get; set; }

        public bool RequireName { get; set; } = false;

        [JsonIgnore]
        public User? User { get; set; }

        public string? UserId { get; set; }

        public async override Task<IEnumerable<CommandError>> ValidateAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var errors = new List<CommandError>();
            if (string.IsNullOrWhiteSpace(this.Code))
            {
                errors.Add(this.GenerateError($"Code is required for storing a program"));
            }

            if (this.RequireName && string.IsNullOrWhiteSpace(this.Name))
            {
                errors.Add(this.GenerateError($"Name is required for storing a program"));
            }

            if (this.User == null)
            {
                if (string.IsNullOrWhiteSpace(this.UserId))
                {
                    errors.Add(this.GenerateError($"UserID is required for storing a program"));
                }

                if (!errors.Any())
                {
                    this.User = await session.Query<User>().FirstOrDefaultAsync(u => u.Id == this.UserId).ConfigureAwait(false);
                    if (this.User == null)
                    {
                        errors.Add(this.GenerateError($"User does not exist"));
                    }
                }
            }
            else
            {
                this.UserId = this.User.Id;
            }

            return errors.AsEnumerable();
        }

        protected async override Task<CommandResult> DoApplyAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (this.User == null) throw new InvalidCallOrderException();

            CodeProgram? program = null;
            if (!string.IsNullOrEmpty(this.Name))
            {
                program = await session.Query<CodeProgram>()
                    .FirstOrDefaultAsync(p => p.Name == this.Name && p.UserId == this.User.Name)
                    .ConfigureAwait(false);
                if (program != null)
                {
                    program.Code = this.Code ?? string.Empty;
                }
            }

            if (program == null)
            {
                program = new CodeProgram
                {
                    Name = this.Name,
                    Code = this.Code ?? string.Empty,
                    WhenAdded = this.WhenExecuted,
                    Number = this.User.NextProgramNumber++,
                    UserId = this.User.Name
                };
                await session.StoreAsync(program).ConfigureAwait(false);
            }
            return this.Result(program);
        }
    }
}