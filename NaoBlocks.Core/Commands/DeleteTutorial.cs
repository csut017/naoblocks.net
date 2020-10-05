using NaoBlocks.Core.Commands.Helpers;
using NaoBlocks.Core.Models;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Core.Commands
{
    public class DeleteTutorial
        : CommandBase
    {
        private Tutorial? tutorial;

        public string? Category { get; set; }

        public string? Name { get; set; }

        public async override Task<IEnumerable<CommandError>> ValidateAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var errors = new List<CommandError>();
            if (string.IsNullOrWhiteSpace(this.Name))
            {
                errors.Add(this.GenerateError($"Name is required to delete a tutorial"));
            }

            if (string.IsNullOrWhiteSpace(this.Category))
            {
                errors.Add(this.GenerateError($"Category is required to delete a tutorial"));
            }

            if (!errors.Any())
            {
                this.tutorial = await session.Query<Tutorial>()
                                            .FirstOrDefaultAsync(t => t.Name == this.Name && t.Category == this.Category)
                                            .ConfigureAwait(false);
                if (this.tutorial == null) errors.Add(this.GenerateError($"Tutorial '{this.Name}' does not exist in '{this.Category}'"));
            }

            return errors.AsEnumerable();
        }

        protected override Task<CommandResult> DoApplyAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            session.Delete(this.tutorial);
            return Task.FromResult(CommandResult.New(this.Number));
        }
    }
}