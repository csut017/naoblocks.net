using Moq;
using NaoBlocks.Core.Commands;
using NaoBlocks.Core.Models;
using Raven.Client.Documents.Session;
using RavenDB.Mocks;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Core.Tests.Commands
{
    public class FinishSessionCommandTests
    {
        [Fact]
        public async Task ApplyHandlesMissingSession()
        {
            var sessions = new Session[0];
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var command = new FinishSessionCommand { UserId = "Hello", WhenExecuted = new DateTime(2019, 1, 2) };
            sessionMock.Setup(s => s.Query<Session>(null, null, false)).Returns(sessions.AsRavenQueryable());
            var result = (await command.ApplyAsync(sessionMock.Object));
            Assert.Null(result as CommandResult<Session>);
        }

        [Fact]
        public async Task ApplyRequiresSession()
        {
            var command = new FinishSessionCommand();
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await command.ApplyAsync(null));
        }

        [Fact]
        public async Task ApplyUpdatesExistingSession()
        {
            var sessions = new[]
            {
                new Session { WhenExpires = new DateTime(2019, 1, 2, 0, 1, 0)}
            };
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var command = new FinishSessionCommand { UserId = "Hello", WhenExecuted = new DateTime(2019, 1, 2) };
            sessionMock.Setup(s => s.Query<Session>(null, null, false)).Returns(sessions.AsRavenQueryable());
            var result = (await command.ApplyAsync(sessionMock.Object)).As<Session>();
            Assert.Equal(new DateTime(2019, 1, 1, 23, 59, 0), result.Output?.WhenExpires);
        }

        [Fact]
        public async Task ValidateRequiresSession()
        {
            var command = new FinishSessionCommand();
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await command.ValidateAsync(null));
        }

        [Fact]
        public async Task ValidateRequiresUserId()
        {
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var command = new FinishSessionCommand { UserId = string.Empty };
            var result = await command.ValidateAsync(sessionMock.Object);
            var expected = new[]
            {
                "User ID is required"
            };
            Assert.Equal(expected, result.Select(r => r.Error));
        }
    }
}