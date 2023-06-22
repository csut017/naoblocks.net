using Microsoft.Extensions.Logging;
using NaoBlocks.Common;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public ILogger Logger => throw new System.NotImplementedException();

        internal MockingRavenDbWrapper DatabaseSession { get; private set; }

        public async Task CommitAsync()
        {
            await this.DatabaseSession.SaveChangesAsync();
        }

        public async Task<CommandResult> ExecuteAsync(CommandBase command, string? source = null)
        {
            var result = await command.ExecuteAsync(this.DatabaseSession, this);
            return result;
        }

        public TGenerator Generator<TGenerator>()
            where TGenerator : ReportGenerator, new()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<CommandLog> HydrateCommandLogs(IEnumerable<string> logs)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<string> ListDehydratedCommandLogsAsync(DateTime fromTime, DateTime toTime, params CommandTarget[] targets)
        {
            throw new NotImplementedException();
        }

        public TQuery Query<TQuery>()
            where TQuery : DataQuery, new()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<CommandError>> RestoreAsync(CommandBase command)
        {
            return await command.RestoreAsync(this.DatabaseSession);
        }

        public async Task<IEnumerable<CommandError>> ValidateAsync(CommandBase command)
        {
            return await command.ValidateAsync(this.DatabaseSession, this);
        }

        internal static string[] GetErrors(IEnumerable<CommandError> errors)
        {
            return errors.Select(e => e.Error).ToArray();
        }
    }
}