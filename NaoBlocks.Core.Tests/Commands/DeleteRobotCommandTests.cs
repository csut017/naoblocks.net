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
    public class DeleteRobotCommandTests
    {
        [Fact]
        public async Task ApplyDeletesRobot()
        {
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var command = new DeleteRobotCommand { MachineName = "Bob" };
            var result = await command.ApplyAsync(sessionMock.Object);
            Assert.True(result.WasSuccessful);
            sessionMock.Verify(s => s.Delete(It.IsAny<Robot>()), Times.Once);
        }

        [Fact]
        public async Task ApplyRequiresSession()
        {
            var command = new DeleteRobotCommand();
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await command.ApplyAsync(null));
        }

        [Fact]
        public async Task ValidateHandlesExistingRobot()
        {
            var data = new[]
            {
                new Robot { MachineName = "Old" }
            }.AsRavenQueryable();

            var sessionMock = new Mock<IAsyncDocumentSession>();
            sessionMock.Setup(s => s.Query<Robot>(null, null, false)).Returns(data);

            var command = new DeleteRobotCommand { MachineName = "Bob" };
            var result = await command.ValidateAsync(sessionMock.Object);
            var expected = new string[0];
            Assert.Equal(expected, result.Select(r => r.Error));
        }

        [Fact]
        public async Task ValidateHandlesMissingRobot()
        {
            var data = new Robot[0].AsRavenQueryable();

            var sessionMock = new Mock<IAsyncDocumentSession>();
            sessionMock.Setup(s => s.Query<Robot>(null, null, false)).Returns(data);

            var command = new DeleteRobotCommand { MachineName = "Bob" };
            var result = await command.ValidateAsync(sessionMock.Object);
            var expected = new[]
            {
                "Robot Bob does not exist"
            };
            Assert.Equal(expected, result.Select(r => r.Error));
        }

        [Fact]
        public async Task ValidateRequiresName()
        {
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var command = new DeleteRobotCommand();
            var result = await command.ValidateAsync(sessionMock.Object);
            var expected = new[]
            {
                "Machine name is required for robot"
            };
            Assert.Equal(expected, result.Select(r => r.Error));
        }

        [Fact]
        public async Task ValidateRequiresSession()
        {
            var command = new DeleteRobotCommand();
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await command.ValidateAsync(null));
        }
    }
}