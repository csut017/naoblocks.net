using Moq;
using NaoBlocks.Web.Helpers;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Web.Tests.Helpers
{
    public class CommandManagerTests
    {
        [Fact]
        public async Task ApplyAsyncCallsCommandsMethod()
        {
            var command = new FakeCommand();
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var storeMock = new Mock<IDocumentStore>();
            storeMock.Setup(s => s.OpenAsyncSession()).Returns(sessionMock.Object);
            var manager = new CommandManager(storeMock.Object, sessionMock.Object);
            var result = await manager.ApplyAsync(command);
            Assert.True(command.ApplyCalled);
        }

        [Fact]
        public async Task ValidateAsyncCallsCommandsMethod()
        {
            var commandResult = new string[] { }.AsEnumerable();
            var commandMock = new Mock<CommandBase>();
            commandMock.Setup(x => x.ValidateAsync(It.IsNotNull<IAsyncDocumentSession>()))
                .Returns(Task.FromResult(commandResult));
            var storeMock = new Mock<IDocumentStore>();
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var manager = new CommandManager(storeMock.Object, sessionMock.Object);
            var result = await manager.ValidateAsync(commandMock.Object);
            Assert.Equal(commandResult, result);
            commandMock.Verify(x => x.ValidateAsync(It.IsNotNull<IAsyncDocumentSession>()), Times.Once);
        }

        private class FakeCommand : CommandBase
        {
            public bool ApplyCalled { get; set; }

            protected override Task DoApplyAsync(IAsyncDocumentSession session, CommandResult result)
            {
                this.ApplyCalled = true;
                return Task.CompletedTask;
            }
        }
    }
}