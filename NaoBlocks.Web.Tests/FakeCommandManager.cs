using Moq;
using NaoBlocks.Web.Helpers;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaoBlocks.Web.Tests
{
    public class FakeCommandManager
        : ICommandManager
    {
        public FakeCommandManager()
        {
            this.Session = new Mock<IAsyncDocumentSession>().Object;
            this.ApplyCommand = async c => await c.ApplyAsync(this.Session);
        }

        public Func<CommandBase, Task<CommandResult>> ApplyCommand { get; set; }

        public IAsyncDocumentSession Session { get; set; }

        public async Task<CommandResult> ApplyAsync(CommandBase command)
        {
            var result = await this.ApplyCommand(command);
            return result;
        }

        public Task CommitAsync()
        {
            return Task.CompletedTask;
        }

        public async Task<IEnumerable<string>> ValidateAsync(CommandBase command)
        {
            return await command.ValidateAsync(this.Session);
        }
    }
}