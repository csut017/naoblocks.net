using Newtonsoft.Json;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Web.Helpers
{
    public abstract class CommandBase
    {
        [JsonIgnore]
        public string Id { get; set; }

        public int Number { get; set; }

        public async Task<CommandResult> ApplyAsync(IAsyncDocumentSession session)
        {
            var result = new CommandResult(this.Number);
            try
            {
                await this.DoApplyAsync(session, result);
            }
            catch (Exception error)
            {
                result.Error = "Unexpected error: " + error.Message;
            }

            return result;
        }

        public abstract Task DoApplyAsync(IAsyncDocumentSession session, CommandResult result);

        public virtual Task<IEnumerable<string>> ValidateAsync(IAsyncDocumentSession session)
        {
            return Task.FromResult(new List<string>().AsEnumerable());
        }

        public virtual Task<CommandResult> RollBackAsync(IAsyncDocumentSession session)
        {
            return Task.FromResult(new CommandResult(this.Number, "Command does not allow rolling back"));
        }
    }
}
