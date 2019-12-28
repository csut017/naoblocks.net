using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Core.Commands
{
    public class BatchCommand
        : CommandBase<IEnumerable<CommandResult>>
    {
        public BatchCommand(params CommandBase[] commands)
        {
            this.Commands = new List<CommandBase>(commands);
        }

        public IList<CommandBase> Commands { get; private set; }

        public async override Task<IEnumerable<CommandError>> ValidateAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var errors = new List<CommandError>();
            var number = 0;
            foreach (var command in this.Commands)
            {
                command.Number = number++;
                errors.AddRange(await command.ValidateAsync(session).ConfigureAwait(false));
            }

            return errors.AsEnumerable();
        }

        protected override async Task<CommandResult> DoApplyAsync(IAsyncDocumentSession? session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            var results = new List<CommandResult>();
            foreach (var command in this.Commands)
            {
                var result = await command.ApplyAsync(session).ConfigureAwait(false);
                results.Add(result);
            }
            var finalResult = new BatchCommandResult(this.Number, results);
            return finalResult;
        }
    }
}