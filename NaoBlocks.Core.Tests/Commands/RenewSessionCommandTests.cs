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
    public class RenewSessionCommandTests
    {
        [Fact]
        public async Task ApplyRequiresSession()
        {
            var command = new RenewSession();
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await command.ApplyAsync(null));
        }

        [Fact]
        public async Task ApplySetsWhenAddedAndWhenExpires()
        {
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var command = new RenewSession
            {
                Session = new Session { WhenExpires = new DateTime(2018, 12, 31) },
                WhenExecuted = new DateTime(2019, 1, 1)
            };
            var result = (await command.ApplyAsync(sessionMock.Object)).As<Session>();
            Assert.Equal(new DateTime(2019, 1, 2), result.Output?.WhenExpires);
        }

        [Fact]
        public async Task ValidateChecksForUser()
        {
            var data = new Session[0].AsRavenQueryable();

            var sessionMock = new Mock<IAsyncDocumentSession>();
            sessionMock.Setup(s => s.Query<Session>(null, null, false)).Returns(data);
            var command = new RenewSession
            {
                UserId = "testing"
            };
            var result = await command.ValidateAsync(sessionMock.Object);
            var expected = new[]
            {
                "User does not have a current session"
            };
            Assert.Equal(expected, result.Select(r => r.Error));
        }

        [Fact]
        public async Task ValidateMustBeCalledBeforeApply()
        {
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var command = new RenewSession { UserId = "testing" };
            await Assert.ThrowsAsync<InvalidCallOrderException>(async () => await command.ApplyAsync(sessionMock.Object));
        }

        [Fact]
        public async Task ValidatePassesAllChecks()
        {
            var data = new[]{
                new Session { WhenExpires = new DateTime(2019, 1, 1) }
            };

            var sessionMock = new Mock<IAsyncDocumentSession>();
            sessionMock.Setup(s => s.Query<Session>(null, null, false)).Returns(data.AsRavenQueryable());

            var command = new RenewSession { UserId = "testing", WhenExecuted = new DateTime(2019, 1, 1) };
            var result = await command.ValidateAsync(sessionMock.Object);
            var expected = new string[0];
            Assert.Equal(expected, result.Select(r => r.Error));
            Assert.Equal(data[0], command.Session);
        }

        [Fact]
        public async Task ValidateRequiresId()
        {
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var command = new RenewSession { };
            var result = await command.ValidateAsync(sessionMock.Object);
            var expected = new[]
            {
                "User ID is required"
            };
            Assert.Equal(expected, result.Select(r => r.Error));
        }

        [Fact]
        public async Task ValidateRequiresSession()
        {
            var command = new RenewSession();
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await command.ValidateAsync(null));
        }
    }
}