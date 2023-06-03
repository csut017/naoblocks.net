using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Web.Tests
{
    public class FakeEngine : IExecutionEngine
    {
        private readonly Queue<CommandCall> expectedCommands = new();
        private readonly Dictionary<Type, ReportGenerator> generators = new();
        private readonly FakeLogger<FakeEngine> logger = new();
        private readonly Dictionary<Type, DataQuery> queries = new();

        public bool CommitCalled { get; set; }

        public CommandBase? LastCommand { get; private set; }

        public ILogger Logger
        {
            get { return logger; }
        }

        public Func<CommandBase, IEnumerable<CommandError>>? OnValidate { get; set; }

        public Task CommitAsync()
        {
            this.CommitCalled = true;
            return Task.CompletedTask;
        }

        public Task<CommandResult> ExecuteAsync(CommandBase command, string? source = null)
        {
            this.LastCommand = command;
            Assert.True(this.expectedCommands.Any(), "Unexpected command call");
            var nextCommand = this.expectedCommands.Dequeue();
            Assert.Equal(nextCommand.Type, command.GetType());
            return Task.FromResult(nextCommand.Result);
        }

        public void ExpectCommand<TCommand>()
            where TCommand : CommandBase
        {
            this.expectedCommands.Enqueue(new CommandCall(typeof(TCommand), CommandResult.New(1)));
        }

        public void ExpectCommand<TCommand>(CommandResult result)
            where TCommand : CommandBase
        {
            this.expectedCommands.Enqueue(new CommandCall(typeof(TCommand), result));
        }

        public TGenerator Generator<TGenerator>()
            where TGenerator : ReportGenerator, new()
        {
            return (TGenerator)this.generators[typeof(TGenerator)];
        }

        public IEnumerable<string> GetLogMessages()
        {
            return this.logger.Messages;
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
            return (TQuery)this.queries[typeof(TQuery)];
        }

        public void RegisterGenerator<T>(T genertor)
            where T : ReportGenerator
        {
            generators.Add(typeof(T), genertor);
        }

        public void RegisterQuery<T>(T query)
            where T : DataQuery
        {
            queries.Add(typeof(T), query);
        }

        public Task<IEnumerable<CommandError>> RestoreAsync(CommandBase command)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<CommandError>> ValidateAsync(CommandBase command)
        {
            return Task.FromResult(this.OnValidate == null
                ? Array.Empty<CommandError>().AsEnumerable()
                : this.OnValidate(command));
        }

        public void Verify()
        {
            Assert.Empty(this.expectedCommands.Select(c => c.Type.Name).ToArray());
        }

        /// <summary>
        /// Setup the controller so a call to LoadUser will retrieve a user
        /// </summary>
        /// <param name="controller">The controller to configure</param>
        /// <param name="name">The user's name.</param>
        /// <param name="role">The user's role.</param>
        /// <returns>A <see cref="User"/> instance and the mock query.</returns>
        internal (User, Mock<UserData>) ConfigureUser(ControllerBase controller, string name, UserRole role)
        {
            var user = new User
            {
                Name = name,
                Role = role,
                Id = "users/1"
            };
            var identity = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, name),
                    new Claim(ClaimTypes.Role, role.ToString())
                });
            var principal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };

            var query = new Mock<UserData>();
            this.RegisterQuery(query.Object);
            query.Setup(q => q.RetrieveByIdAsync(name))
                .Returns(Task.FromResult((User?)user));

            return (user, query);
        }

        private class CommandCall
        {
            public CommandCall(Type type, CommandResult result)
            {
                this.Type = type;
                this.Result = result;
            }

            public CommandResult Result { get; private set; }

            public Type Type { get; private set; }
        }
    }
}