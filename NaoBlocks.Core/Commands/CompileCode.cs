using NaoBlocks.Core.Commands.Helpers;
using NaoBlocks.Core.Models;
using NaoBlocks.Parser;
using Raven.Client.Documents.Session;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Core.Commands
{
    public class CompileCode
        : CommandBase<CompiledCodeProgram>
    {
        public string? Code { get; set; }

        public override Task<IEnumerable<CommandError>> ValidateAsync(IAsyncDocumentSession? session)
        {
            var errors = new List<CommandError>();
            if (string.IsNullOrWhiteSpace(this.Code))
            {
                errors.Add(this.GenerateError("No code to compile"));
            }

            return Task.FromResult(errors.AsEnumerable());
        }

        protected override async Task<CommandResult> DoApplyAsync(IAsyncDocumentSession? session)
        {
            var parser = CodeParser.New(this.Code ?? string.Empty);
            var parseResult = await parser.ParseAsync().ConfigureAwait(false);
            return this.Result(new CompiledCodeProgram(parseResult));
        }
    }
}