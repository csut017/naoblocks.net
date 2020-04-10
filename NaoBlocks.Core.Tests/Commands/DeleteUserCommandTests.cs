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
    public class DeleteUserCommandTests
    {
        [Fact]
        public async Task ApplyDeleteUser()
        {
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var command = new DeleteUser { Name = "Bob" };
            var result = await command.ApplyAsync(sessionMock.Object);
            Assert.True(result.WasSuccessful);
            sessionMock.Verify(s => s.Delete(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task ApplyRequiresSession()
        {
            var command = new DeleteUser();
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

            var command = new DeleteUser { Name = "Bob", Role = UserRole.Teacher };
            var result = await command.ValidateAsync(sessionMock.Object);
            var expected = new string[0];
            Assert.Equal(expected, result.Select(r => r.Error));
        }

        [Fact]
        public async Task ValidateHandlesMissingUser()
        {
            var data = new User[0].AsRavenQueryable();

            var sessionMock = new Mock<IAsyncDocumentSession>();
            sessionMock.Setup(s => s.Query<User>(null, null, false)).Returns(data);

            var command = new DeleteUser { Name = "Bob", Role = UserRole.Teacher };
            var result = await command.ValidateAsync(sessionMock.Object);
            var expected = new[]
            {
                "Teacher Bob does not exist"
            };
            Assert.Equal(expected, result.Select(r => r.Error));
        }

        [Fact]
        public async Task ValidateRequiresName()
        {
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var command = new DeleteUser();
            var result = await command.ValidateAsync(sessionMock.Object);
            var expected = new[]
            {
                "Student name is required"
            };
            Assert.Equal(expected, result.Select(r => r.Error));
        }

        [Fact]
        public async Task ValidateRequiresNameForTeacher()
        {
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var command = new DeleteUser { Role = UserRole.Teacher };
            var result = await command.ValidateAsync(sessionMock.Object);
            var expected = new[]
            {
                "Teacher name is required"
            };
            Assert.Equal(expected, result.Select(r => r.Error));
        }

        [Fact]
        public async Task ValidateRequiresSession()
        {
            var command = new DeleteUser();
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await command.ValidateAsync(null));
        }
    }
}