using Microsoft.Extensions.Logging;
using NaoBlocks.Common;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;
using NaoBlocks.Engine.Commands;
using System;

namespace NaoBlocks.Engine.Tests
{
    public class FakeEngine : IExecutionEngine
    {
        public FakeEngine()
            : this(null)
        {
        }

        public FakeEngine(IAsyncDocumentSession? session)
        {
            this.DatabaseSession = new MockingRavenDbWrapper(session);
        }

        internal MockingRavenDbWrapper DatabaseSession { get; private set; }

        public ILogger Logger => throw new System.NotImplementedException();

        public async Task CommitAsync()
        {
            await this.DatabaseSession.SaveChangesAsync();
        }

        internal static string[] GetErrors(IEnumerable<CommandError> errors)
        {
            return errors.Select(e => e.Error).ToArray();
        }

        public async Task<CommandResult> ExecuteAsync(CommandBase command)
        {
            var result = await command.ExecuteAsync(this.DatabaseSession);
            return result;
        }

        public async Task<IEnumerable<CommandError>> ValidateAsync(CommandBase command)
        {
            return await command.ValidateAsync(this.DatabaseSession);
        }

        public async Task<IEnumerable<CommandError>> RestoreAsync(CommandBase command)
        {
            return await command.RestoreAsync(this.DatabaseSession);
        }
    }
}
