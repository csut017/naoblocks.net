using Moq;
using NaoBlocks.Core.Commands;
using NaoBlocks.Core.Models;
using Raven.Client.Documents.Session;
using RavenDB.Mocks;
using System;
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
            var command = new StartSessionCommand();
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await command.ApplyAsync(null));
        }

        [Fact]
        public async Task ApplySetsWhenAdded()
        {
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var command = new StartSessionCommand { UserId = "Hello", WhenExecuted = new DateTime(2019, 1, 1) };
            await command.ApplyAsync(sessionMock.Object);
            Assert.Equal(command.WhenExecuted, command.Output?.WhenAdded);
        }

        [Fact]
        public async Task ApplyStoresSession()
        {
            var data = new[]{
                new User
                {
                    Password = Password.New("Hello")
                }
            }.AsRavenQueryable();
            var sessionMock = new Mock<IAsyncDocumentSession>();
            sessionMock.Setup(s => s.Query<User>(null, null, false)).Returns(data);
            var command = new StartSessionCommand { UserId = "users/1" };
            await command.ValidateAsync(sessionMock.Object);
            var result = await command.ApplyAsync(sessionMock.Object);
            sessionMock.Verify(s => s.StoreAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ValidateChecksForUser()
        {
            var data = new User[0].AsRavenQueryable();

            var sessionMock = new Mock<IAsyncDocumentSession>();
            sessionMock.Setup(s => s.Query<User>(null, null, false)).Returns(data);
            var command = new StartSessionCommand
            {
                Name = "Old",
                Password = "testing"
            };
            var result = await command.ValidateAsync(sessionMock.Object);
            var expected = new[]
            {
                "Unknown or invalid user"
            };
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task ValidateChecksPassword()
        {
            var data = new[]{
                new User { Password = Password.New("different") }
            }.AsRavenQueryable();

            var sessionMock = new Mock<IAsyncDocumentSession>();
            sessionMock.Setup(s => s.Query<User>(null, null, false)).Returns(data);
            var command = new StartSessionCommand
            {
                Name = "Old",
                Password = "testing"
            };
            var result = await command.ValidateAsync(sessionMock.Object);
            var expected = new[]
            {
                "Unknown or invalid user"
            };
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task ValidateMustBeCalledBeforeApply()
        {
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var command = new StartSessionCommand { Name = "Bob", Password = string.Empty };
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

            var command = new StartSessionCommand { Name = "Bob", Password = "Hello" };
            var result = await command.ValidateAsync(sessionMock.Object);
            var expected = new string[0];
            Assert.Equal(expected, result);
            Assert.Equal(userId, command.UserId);
            Assert.Equal(UserRole.Administrator, command.Role);
        }

        [Fact]
        public async Task ValidateRequiresName()
        {
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var command = new StartSessionCommand { Password = string.Empty };
            var result = await command.ValidateAsync(sessionMock.Object);
            var expected = new[]
            {
                "User name is required"
            };
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task ValidateRequiresPassword()
        {
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var command = new StartSessionCommand { Name = "Bob" };
            var result = await command.ValidateAsync(sessionMock.Object);
            var expected = new[]
            {
                "Password is required"
            };
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task ValidateRequiresSession()
        {
            var command = new StartSessionCommand();
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await command.ValidateAsync(null));
        }
    }
}