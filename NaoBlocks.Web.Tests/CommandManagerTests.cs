using Moq;
using NaoBlocks.Web.Helpers;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Web.Tests
{
    public class CommandManagerTests
    {
        [Fact]
        public async Task ValidateAsyncCallsCommandsMethod()
        {
            var noErrors = new string[] { }.AsEnumerable();
            var command = new Mock<CommandBase>();
            command.Setup(x => x.ValidateAsync(It.IsNotNull<IAsyncDocumentSession>()))
                .Returns(Task.FromResult(noErrors));
            var store = new Mock<IDocumentStore>(); 
            var session = new Mock<IAsyncDocumentSession>();
            var manager = new CommandManager(store.Object, session.Object);
            var result = await manager.ValidateAsync(command.Object);
            Assert.Equal(noErrors, result);
            command.Verify(x => x.ValidateAsync(It.IsNotNull<IAsyncDocumentSession>()), Times.Once);
        }
    }
}
