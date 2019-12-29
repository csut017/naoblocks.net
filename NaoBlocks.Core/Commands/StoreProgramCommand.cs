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
    public class StoreProgramCommand
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
                errors.Add(this.Error($"Code is required for storing a program"));
            }

            if (this.RequireName && string.IsNullOrWhiteSpace(this.Name))
            {
                errors.Add(this.Error($"Name is required for storing a program"));
            }

            if (this.User == null)
            {
                if (string.IsNullOrWhiteSpace(this.UserId))
                {
                    errors.Add(this.Error($"UserID is required for storing a program"));
                }

                if (!errors.Any())
                {
                    this.User = await session.Query<User>().FirstOrDefaultAsync(u => u.Id == this.UserId).ConfigureAwait(false);
                    if (this.User == null)
                    {
                        errors.Add(this.Error($"User does not exist"));
                    }
                }
            }
            else
            {
                this.UserId = this.User.Id;
            }

            return errors.AsEnumerable();
        }

        protected override Task<CommandResult> DoApplyAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (this.User == null) throw new InvalidCallOrderException();

            var addProgram = true;
            CodeProgram? program = null;
            if (!string.IsNullOrEmpty(this.Name))
            {
                program = this.User.Programs
                    .FirstOrDefault(p => p.Name == this.Name);
                if (program != null)
                {
                    program.Code = this.Code ?? string.Empty;
                    addProgram = false;
                }
            }

            if (addProgram)
            {
                program = new CodeProgram
                {
                    Name = this.Name,
                    Code = this.Code ?? string.Empty,
                    WhenAdded = this.WhenExecuted,
                    Number = this.User.NextProgramNumber++
                };
                this.User.Programs.Add(program);
            }
            CommandResult result = this.Result(program);
            return Task.FromResult(result);
        }
    }
}