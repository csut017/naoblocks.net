using System.Collections.Generic;
using System.Linq;

namespace NaoBlocks.Core.Commands
{
    public class BatchCommandResult
        : CommandResult<IEnumerable<CommandResult>>
    {
        public BatchCommandResult(int number, IEnumerable<CommandResult> results)
            : base(number, results)
        {
            if (!results.All(r => r.WasSuccessful)) this.Error = "One or more commands failed";
        }

        public override IEnumerable<CommandError>? ToErrors()
        {
            if (string.IsNullOrEmpty(this.Error))
            {
                return null;
            }

            var errors = new List<CommandError>();
            foreach (var result in this.Output.Where(r => !r.WasSuccessful))
            {
                errors.AddRange(result.ToErrors());
            }

            return errors;
        }
    }
}