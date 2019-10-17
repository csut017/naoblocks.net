using NaoBlocks.Core.Models;
using NaoBlocks.Parser;
using Raven.Client.Documents.Session;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Core.Commands
{
    public class CompileCodeCommand
        : OutputCommandBase<RobotCodeCompilation>
    {
        public string Code { get; set; }

        public override Task<IEnumerable<string>> ValidateAsync(IAsyncDocumentSession session)
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(this.Code))
            {
                errors.Add("No code to compile");
            }

            return Task.FromResult(errors.AsEnumerable());
        }

        protected override async Task DoApplyAsync(IAsyncDocumentSession session, CommandResult result)
        {
            var parser = CodeParser.New(this.Code);
            var parseResult = await parser.ParseAsync().ConfigureAwait(false);
            this.Output = new RobotCodeCompilation(parseResult);
        }
    }
}