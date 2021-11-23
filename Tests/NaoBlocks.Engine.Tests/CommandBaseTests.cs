using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests
{
    public class CommandBaseTests
    {
        [Fact]
        public async Task ExecuteHandlesError()
        {
            var command = new FakeCommand { Error = new Exception("Failing!") };
            var sessionMock = new Mock<IDatabaseSession>();
            var result = await command.ExecuteAsync(sessionMock.Object);
            Assert.Equal("Unexpected error: Failing!", result.Error);
        }

        [Fact]
        public async Task ExecuteHandlesInvalidOrder()
        {
            var command = new FakeCommand { Error = new InvalidCallOrderException() };
            var sessionMock = new Mock<IDatabaseSession>();
            await Assert.ThrowsAsync<InvalidCallOrderException>(async () => await command.ExecuteAsync(sessionMock.Object));
        }

        [Fact]
        public async Task ExecuteSetsResultNumber()
        {
            var command = new FakeCommand { Number = 10, Id = "5" };
            var sessionMock = new Mock<IDatabaseSession>();
            var result = await command.ExecuteAsync(sessionMock.Object);
            Assert.Equal(10, result.Number);
        }

        [Fact]
        public async Task DefaultCheckCanRollbackReturnsFalse()
        {
            var command = new FakeCommand();
            var sessionMock = new Mock<IDatabaseSession>();
            var result = await command.CheckCanRollbackAsync(sessionMock.Object);
            Assert.False(result);
        }

        [Fact]
        public async Task DefaultRollbackFails()
        {
            var command = new FakeCommand();
            var sessionMock = new Mock<IDatabaseSession>();
            var result = await command.RollBackAsync(sessionMock.Object);
            Assert.False(result.WasSuccessful);
            Assert.Equal("Command does not allow rolling back", result.Error);
        }

        [Fact]
        public async Task DefaultValidateWorks()
        {
            var command = new FakeCommand();
            var sessionMock = new Mock<IDatabaseSession>();
            var result = await command.ValidateAsync(sessionMock.Object);
            Assert.Empty(result);
        }

        [Fact]
        public async Task DefaultRestoreWorks()
        {
            var command = new FakeCommand();
            var sessionMock = new Mock<IDatabaseSession>();
            var result = await command.RestoreAsync(sessionMock.Object);
            Assert.Empty(result);
        }
    }
}
