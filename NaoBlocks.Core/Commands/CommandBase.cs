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
        public string? Id { get; set; }

        public int Number { get; set; } = 0;

        [JsonIgnore]
        public DateTime WhenExecuted { get; set; } = DateTime.UtcNow;

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "This method needs to handle any failures.")]
        public async Task<CommandResult> ApplyAsync(IAsyncDocumentSession? session)
        {
            try
            {
                return await this.DoApplyAsync(session).ConfigureAwait(false);
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (InvalidCallOrderException)
            {
                throw;
            }
            catch (Exception error)
            {
                return new CommandResult(this.Number, $"Unexpected error: {error.Message}");
            }
        }

        public virtual Task<bool> CheckCanRollbackAsync(IAsyncDocumentSession? session)
        {
            return Task.FromResult(false);
        }

        public virtual Task<CommandResult> RollBackAsync(IAsyncDocumentSession? session)
        {
            return Task.FromResult(new CommandResult(this.Number, "Command does not allow rolling back"));
        }

        public virtual Task<IEnumerable<CommandError>> ValidateAsync(IAsyncDocumentSession? session)
        {
            return Task.FromResult(new List<CommandError>().AsEnumerable());
        }

        protected abstract Task<CommandResult> DoApplyAsync(IAsyncDocumentSession? session);

        protected CommandError Error(string error)
        {
            return new CommandError(this.Number, error);
        }

        protected virtual CommandResult Result()
        {
            return new CommandResult(this.Number);
        }
    }

    public abstract class CommandBase<T>
        : CommandBase
        where T : class
    {
        protected CommandResult<T> Result(T value)
        {
            return new CommandResult<T>(this.Number, value);
        }
    }
}