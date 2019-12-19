using Newtonsoft.Json;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Core.Commands
{
    public abstract class CommandBase
    {
        [JsonIgnore]
        public string Id { get; set; }

        public int Number { get; set; }

        [JsonIgnore]
        public DateTime WhenExecuted { get; set; } = DateTime.UtcNow;

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "This method needs to handle any failures.")]
        public async Task<CommandResult> ApplyAsync(IAsyncDocumentSession session)
        {
            var result = new CommandResult(this.Number);
            try
            {
                await this.DoApplyAsync(session, result).ConfigureAwait(false);
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (Exception error)
            {
                result.Error = "Unexpected error: " + error.Message;
            }

            return result;
        }

        public virtual Task<bool> CheckCanRollbackAsync(IAsyncDocumentSession session)
        {
            return Task.FromResult(false);
        }

        public virtual Task<CommandResult> RollBackAsync(IAsyncDocumentSession session)
        {
            return Task.FromResult(new CommandResult(this.Number, "Command does not allow rolling back"));
        }

        public virtual Task<IEnumerable<string>> ValidateAsync(IAsyncDocumentSession session)
        {
            return Task.FromResult(new List<string>().AsEnumerable());
        }

        protected abstract Task DoApplyAsync(IAsyncDocumentSession session, CommandResult result);
    }
}