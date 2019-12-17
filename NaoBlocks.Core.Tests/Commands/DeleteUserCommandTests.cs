using Moq;
using NaoBlocks.Core.Commands;
using NaoBlocks.Core.Models;
using Raven.Client.Documents.Session;
using RavenDB.Mocks;
using System;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Core.Tests.Commands
{
    public class DeleteUserCommandTests
    {
        [Fact]
        public async Task ApplyDeleteUser()
        {
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var command = new DeleteUserCommand { Name = "Bob" };
            var result = await command.ApplyAsync(sessionMock.Object);
            sessionMock.Verify(s => s.Delete(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task ApplyRequiresSession()
        {
            var command = new DeleteUserCommand();
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await command.ApplyAsync(null));
        }

        [Fact]
        public async Task ValidateHandlesExistingUser()
        {
            var data = new[]
            {
                new User { Name = "Old" }
            }.AsRavenQueryable();

            var sessionMock = new Mock<IAsyncDocumentSession>();
            sessionMock.Setup(s => s.Query<User>(null, null, false)).Returns(data);

            var command = new DeleteUserCommand { Name = "Bob", Role = UserRole.Teacher };
            var result = await command.ValidateAsync(sessionMock.Object);
            var expected = new string[0];
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task ValidateHandlesMissingUser()
        {
            var data = new User[0].AsRavenQueryable();

            var sessionMock = new Mock<IAsyncDocumentSession>();
            sessionMock.Setup(s => s.Query<User>(null, null, false)).Returns(data);

            var command = new DeleteUserCommand { Name = "Bob", Role = UserRole.Teacher };
            var result = await command.ValidateAsync(sessionMock.Object);
            var expected = new[]
            {
                "Teacher Bob does not exist"
            };
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task ValidateRequiresName()
        {
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var command = new DeleteUserCommand();
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
            var command = new DeleteUserCommand { Role = UserRole.Teacher };
            var result = await command.ValidateAsync(sessionMock.Object);
            var expected = new[]
            {
                "Teacher name is required"
            };
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task ValidateRequiresSession()
        {
            var command = new DeleteUserCommand();
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await command.ValidateAsync(null));
        }
    }
}