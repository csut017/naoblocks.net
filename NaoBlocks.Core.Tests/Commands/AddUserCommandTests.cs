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
    public class AddUserCommandTests
    {
        [Fact]
        public async Task ApplyEncryptsPassword()
        {
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var command = new AddUserCommand { Name = "Bob", Password = "Hello" };
            var result = await command.ApplyAsync(sessionMock.Object);
            sessionMock.Verify(s => s.StoreAsync(It.Is<User>(u => !string.IsNullOrWhiteSpace(u.Password.Hash)), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ApplyRequiresSession()
        {
            var command = new AddUserCommand();
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await command.ApplyAsync(null));
        }

        [Fact]
        public async Task ApplySetsWhenAdded()
        {
            User newUser = null;
            var sessionMock = new Mock<IAsyncDocumentSession>();
            sessionMock.Setup(s => s.StoreAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .Callback<object, CancellationToken>((u, c) =>
                {
                    newUser = u as User;
                });
            var command = new AddUserCommand { Name = "Bob", Password = "Hello", WhenExecuted = new DateTime(2019, 1, 1) };
            var result = await command.ApplyAsync(sessionMock.Object);
            Assert.Equal(command.WhenExecuted, newUser.WhenAdded);
        }

        [Fact]
        public async Task ApplyStoresUser()
        {
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var command = new AddUserCommand { Name = "Bob", Password = string.Empty };
            var result = await command.ApplyAsync(sessionMock.Object);
            sessionMock.Verify(s => s.StoreAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ValidateChecksForExistingUser()
        {
            var data = new[]
            {
                new User { Name = "Old" }
            }.AsRavenQueryable();
            data.Operations.Any = s => true;

            var sessionMock = new Mock<IAsyncDocumentSession>();
            sessionMock.Setup(s => s.Query<User>(null, null, false)).Returns(data);
            var command = new AddUserCommand
            {
                Name = "Old",
                Password = "testing",
                Role = UserRole.Teacher
            };
            var result = await command.ValidateAsync(sessionMock.Object);
            var expected = new[]
            {
                "Teacher with name Old already exists"
            };
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task ValidateRequiresName()
        {
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var command = new AddUserCommand { Password = string.Empty };
            var result = await command.ValidateAsync(sessionMock.Object);
            var expected = new[]
            {
                "Student name is required"
            };
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task ValidateRequiresNameForTeacher()
        {
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var command = new AddUserCommand { Password = string.Empty, Role = UserRole.Teacher };
            var result = await command.ValidateAsync(sessionMock.Object);
            var expected = new[]
            {
                "Teacher name is required"
            };
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task ValidateRequiresPassword()
        {
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var command = new AddUserCommand { Name = "Bob" };
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
            var command = new AddUserCommand();
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await command.ValidateAsync(null));
        }
    }
}