using Moq;
using NaoBlocks.Core.Commands;
using Raven.Client.Documents.Session;
using System;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Core.Tests.Commands
{
    public class CommandBaseTests
    {
        [Fact]
        public async Task ApplyHandlesError()
        {
            var command = new FakeCommand { ThrowException = true };
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var result = await command.ApplyAsync(sessionMock.Object);
            Assert.Equal("Unexpected error: Failing!", result.Error);
        }

        [Fact]
        public async Task ApplySetsResultNumber()
        {
            var command = new FakeCommand { Number = 10, Id = "5" };
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var result = await command.ApplyAsync(sessionMock.Object);
            Assert.Equal(10, result.Number);
        }

        [Fact]
        public async Task DefaultCheckCanRollbackReturnsFalse()
        {
            var command = new FakeCommand();
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var result = await command.CheckCanRollbackAsync(sessionMock.Object);
            Assert.False(result);
        }

        [Fact]
        public async Task DefaultRollbackFails()
        {
            var command = new FakeCommand();
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var result = await command.RollBackAsync(sessionMock.Object);
            Assert.False(result.WasSuccessful);
            Assert.Equal("Command does not allow rolling back", result.Error);
        }

        [Fact]
        public async Task DefaultValidateWorks()
        {
            var command = new FakeCommand();
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var result = await command.ValidateAsync(sessionMock.Object);
            Assert.Empty(result);
        }

        private class FakeCommand : CommandBase
        {
            public bool ThrowException { get; set; }

            protected override Task DoApplyAsync(IAsyncDocumentSession session, CommandResult result)
            {
                if (this.ThrowException) throw new Exception("Failing!");
                return Task.CompletedTask;
            }
        }
    }
}