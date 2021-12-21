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
        private readonly FakeLogger<FakeEngine> logger = new();
        private readonly Dictionary<Type, DataQuery> queries = new();
        private readonly Dictionary<Type, ReportGenerator> generators = new();
        private readonly Queue<CommandCall> expectedCommands = new();
        private bool useExpectedCommand;

        public Func<CommandBase, CommandResult>? OnExecute { get; set; }

        public Func<CommandBase, IEnumerable<CommandError>>? OnValidate { get; set; }

        public ILogger Logger
        {
            get { return logger; }
        }

        public bool CommitCalled { get; set; }

        public CommandBase? LastCommand { get; private set; }

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

            this.LastCommand = command;
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

        public TGenerator Generator<TGenerator>()
            where TGenerator : ReportGenerator, new()
        {
            return (TGenerator)this.generators[typeof(TGenerator)];
        }

        public void RegisterQuery<T>(T query)
            where T : DataQuery
        {
            queries.Add(typeof(T), query);
        }

        public void RegisterGenerator<T>(T genertor)
            where T : ReportGenerator
        {
            generators.Add(typeof(T), genertor);
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

        /// <summary>
        /// Setup the controller so a call to LoadUser will retrieve a user
        /// </summary>
        /// <param name="controller">The controller to configure</param>
        /// <param name="name">The user's name.</param>
        /// <param name="role">The user's role.</param>
        /// <returns>A <see cref="User"/> instance.</returns>
        internal User ConfigureUser(ControllerBase controller, string name, UserRole role)
        {
            var user = new User
            {
                Name = name,
                Role = role
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
            query.Setup(q => q.RetrieveByNameAsync(name))
                .Returns(Task.FromResult((User?)user));

            return user;
        }

        public IEnumerable<string> GetLogMessages()
        {
            return this.logger.Messages;
        }
    }
}
