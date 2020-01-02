using Moq;
using NaoBlocks.Core.Commands;
using NaoBlocks.Core.Models;
using Raven.Client.Documents.Session;
using RavenDB.Mocks;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Core.Tests.Commands
{
    public class StartSessionCommandTests
    {
        [Fact]
        public async Task ApplyRequiresSession()
        {
            var command = new StartUserSessionCommand();
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await command.ApplyAsync(null));
        }

        [Fact]
        public async Task ApplySetsWhenAddedAndWhenExpires()
        {
            var sessions = new Session[0];
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var command = new StartUserSessionCommand { UserId = "Hello", WhenExecuted = new DateTime(2019, 1, 1) };
            sessionMock.Setup(s => s.Query<Session>(null, null, false)).Returns(sessions.AsRavenQueryable());
            var result = (await command.ApplyAsync(sessionMock.Object)).As<Session>();
            Assert.Equal(command.WhenExecuted, result.Output?.WhenAdded);
            Assert.Equal(new DateTime(2019, 1, 2), result.Output?.WhenExpires);
        }

        [Fact]
        public async Task ApplyStoresSession()
        {
            var users = new[]{
                new User { Password = Password.New("Hello") }
            }.AsRavenQueryable();
            var sessions = new Session[0];
            var sessionMock = new Mock<IAsyncDocumentSession>();
            sessionMock.Setup(s => s.Query<User>(null, null, false)).Returns(users);
            sessionMock.Setup(s => s.Query<Session>(null, null, false)).Returns(sessions.AsRavenQueryable());
            var command = new StartUserSessionCommand { UserId = "users/1" };
            await command.ValidateAsync(sessionMock.Object);
            var result = await command.ApplyAsync(sessionMock.Object);
            Assert.True(result.WasSuccessful);
            sessionMock.Verify(s => s.StoreAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ApplyUpdatesExistingSession()
        {
            var users = new[]{
                new User { Password = Password.New("Hello") }
            }.AsRavenQueryable();
            var sessions = new[]
            {
                new Session { WhenExpires = new DateTime(2019, 1, 2)}
            };
            var sessionMock = new Mock<IAsyncDocumentSession>();
            sessionMock.Setup(s => s.Query<User>(null, null, false)).Returns(users);
            sessionMock.Setup(s => s.Query<Session>(null, null, false)).Returns(sessions.AsRavenQueryable());
            var command = new StartUserSessionCommand { UserId = "users/1", WhenExecuted = new DateTime(2019, 1, 2, 1, 0, 0) };
            await command.ValidateAsync(sessionMock.Object);
            var result = await command.ApplyAsync(sessionMock.Object);
            sessionMock.Verify(s => s.StoreAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()), Times.Never);
            Assert.Equal(new DateTime(2019, 1, 3, 1, 0, 0), sessions[0].WhenExpires);
        }

        [Fact]
        public async Task ValidateChecksForUser()
        {
            var data = new User[0].AsRavenQueryable();

            var sessionMock = new Mock<IAsyncDocumentSession>();
            sessionMock.Setup(s => s.Query<User>(null, null, false)).Returns(data);
            var command = new StartUserSessionCommand
            {
                Name = "Old",
                Password = "testing"
            };
            var result = await command.ValidateAsync(sessionMock.Object);
            var expected = new[]
            {
                "Unknown or invalid user"
            };
            Assert.Equal(expected, result.Select(r => r.Error));
        }

        [Fact]
        public async Task ValidateChecksPassword()
        {
            var data = new[]{
                new User { Password = Password.New("different") }
            }.AsRavenQueryable();

            var sessionMock = new Mock<IAsyncDocumentSession>();
            sessionMock.Setup(s => s.Query<User>(null, null, false)).Returns(data);
            var command = new StartUserSessionCommand
            {
                Name = "Old",
                Password = "testing"
            };
            var result = await command.ValidateAsync(sessionMock.Object);
            var expected = new[]
            {
                "Unknown or invalid user"
            };
            Assert.Equal(expected, result.Select(r => r.Error));
        }

        [Fact]
        public async Task ValidateMustBeCalledBeforeApply()
        {
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var command = new StartUserSessionCommand { Name = "Bob", Password = string.Empty };
            await Assert.ThrowsAsync<InvalidCallOrderException>(async () => await command.ApplyAsync(sessionMock.Object));
        }

        [Fact]
        public async Task ValidatePassesAllChecks()
        {
            const string userId = "users/1";
            var data = new[]{
                new User
                {
                    Id = userId,
                    Password = Password.New("Hello"),
                    Role = UserRole.Administrator
                }
            }.AsRavenQueryable();
            var sessionMock = new Mock<IAsyncDocumentSession>();
            sessionMock.Setup(s => s.Query<User>(null, null, false)).Returns(data);

            var command = new StartUserSessionCommand { Name = "Bob", Password = "Hello" };
            var result = await command.ValidateAsync(sessionMock.Object);
            var expected = new string[0];
            Assert.Equal(expected, result.Select(r => r.Error));
            Assert.Equal(userId, command.UserId);
            Assert.Equal(UserRole.Administrator, command.Role);
        }

        [Fact]
        public async Task ValidateRequiresName()
        {
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var command = new StartUserSessionCommand { Password = string.Empty };
            var result = await command.ValidateAsync(sessionMock.Object);
            var expected = new[]
            {
                "User name is required"
            };
            Assert.Equal(expected, result.Select(r => r.Error));
        }

        [Fact]
        public async Task ValidateRequiresPassword()
        {
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var command = new StartUserSessionCommand { Name = "Bob" };
            var result = await command.ValidateAsync(sessionMock.Object);
            var expected = new[]
            {
                "Password is required"
            };
            Assert.Equal(expected, result.Select(r => r.Error));
        }

        [Fact]
        public async Task ValidateRequiresSession()
        {
            var command = new StartUserSessionCommand();
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await command.ValidateAsync(null));
        }
    }
}