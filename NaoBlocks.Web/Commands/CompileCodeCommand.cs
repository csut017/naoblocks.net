using NaoBlocks.Parser;
using NaoBlocks.Web.Helpers;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Web.Commands
{
    public class CompileCodeCommand
        : OutputCommandBase<Dtos.RobotCodeCompilation>
    {
        public string Code { get; set; }

        protected override async Task DoApplyAsync(IAsyncDocumentSession session, CommandResult result)
        {
            var parser = CodeParser.New(this.Code);
            var parseResult = await parser.ParseAsync();
            this.Output = new Dtos.RobotCodeCompilation(parseResult);
        }

        public override Task<IEnumerable<string>> ValidateAsync(IAsyncDocumentSession session)
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(this.Code))
            {
                errors.Add("No code to compile");
            }

            return Task.FromResult(errors.AsEnumerable());
        }
    }
}
