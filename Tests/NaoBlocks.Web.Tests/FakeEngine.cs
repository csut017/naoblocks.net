using Microsoft.Extensions.Logging;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Web.Tests
{
    public class FakeEngine : IExecutionEngine
    {
        private readonly ILogger logger = new FakeLogger<FakeEngine>();
        private readonly IDictionary<Type, DataQuery> queries = new Dictionary<Type, DataQuery>();
        private readonly Queue<CommandCall> expectedCommands = new();
        private bool useExpectedCommand;

        public Func<CommandBase, CommandResult>? OnExecute { get; set; }

        public Func<CommandBase, IEnumerable<CommandError>>? OnValidate { get; set; }

        public ILogger Logger
        {
            get { return logger; }
        }

        public bool CommitCalled { get; set; }

        public Task CommitAsync()
        {
            this.CommitCalled = true;
            return Task.CompletedTask;
        }

        public Task<CommandResult> ExecuteAsync(CommandBase command)
        {
            if (this.useExpectedCommand)
            {
                Assert.True(this.expectedCommands.Any(), "Unexpected command call");
                var nextCommand = this.expectedCommands.Dequeue();
                Assert.Equal(nextCommand.Type, command.GetType());
            }

            return Task.FromResult(this.OnExecute == null 
                ? CommandResult.New(command.Number) 
                : this.OnExecute(command));
        }

        public Task<IEnumerable<CommandError>> ValidateAsync(CommandBase command)
        {
            return Task.FromResult(this.OnValidate == null 
                ? Array.Empty<CommandError>().AsEnumerable()
                : this.OnValidate(command));
        }

        public Task<IEnumerable<CommandError>> RestoreAsync(CommandBase command)
        {
            throw new NotImplementedException();
        }

        public TQuery Query<TQuery>()
            where TQuery : DataQuery, new()
        {
            return (TQuery)this.queries[typeof(TQuery)];
        }

        public void RegisterQuery<T>(T query)
            where T: DataQuery
        {
            queries.Add(typeof(T), query);
        }

        public void ExpectCommand<TCommand>()
            where TCommand : CommandBase
        {
            this.expectedCommands.Enqueue(new CommandCall(typeof(TCommand)));
            this.useExpectedCommand = true;
        }

        public void Verify()
        {
            Assert.Empty(this.expectedCommands.Select(c => c.Type.Name).ToArray());
        }

        private class CommandCall
        {
            public CommandCall(Type type)
            {
                Type = type;
            }

            public Type Type { get; set; } 
        }
    }
}
