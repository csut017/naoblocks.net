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
    public class AddRobotCommandTests
    {
        [Fact]
        public async Task ApplyRequiresSession()
        {
            var command = new AddRobotCommand();
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await command.ApplyAsync(null));
        }

        [Fact]
        public async Task ApplySetsWhenAdded()
        {
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var command = new AddRobotCommand { MachineName = "testing-robot", WhenExecuted = new DateTime(2019, 1, 1) };
            await command.ApplyAsync(sessionMock.Object);
            Assert.Equal(command.WhenExecuted, command.Output.WhenAdded);
        }

        [Fact]
        public async Task ApplyStoresRobot()
        {
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var command = new AddRobotCommand { MachineName = "testing-robot" };
            var result = await command.ApplyAsync(sessionMock.Object);
            sessionMock.Verify(s => s.StoreAsync(It.IsAny<Robot>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ValidateChecksForExistingRobot()
        {
            var data = new[]
            {
                new Robot { MachineName = "Old" }
            }.AsRavenQueryable();
            data.Operations.Any = s => true;

            var sessionMock = new Mock<IAsyncDocumentSession>();
            sessionMock.Setup(s => s.Query<Robot>(null, null, false)).Returns(data);

            var command = new AddRobotCommand { MachineName = "Old" };
            var result = await command.ValidateAsync(sessionMock.Object);
            var expected = new[]
            {
                "Robot with name Old already exists"
            };
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task ValidatePassesAllChecks()
        {
            var data = new Robot[0].AsRavenQueryable();
            var sessionMock = new Mock<IAsyncDocumentSession>();
            sessionMock.Setup(s => s.Query<Robot>(null, null, false)).Returns(data);

            var command = new AddRobotCommand { MachineName = "Old", FriendlyName = "Old Robot" };
            var result = await command.ValidateAsync(sessionMock.Object);
            var expected = new string[0];
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task ValidateRequiresName()
        {
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var command = new AddRobotCommand { MachineName = string.Empty };
            var result = await command.ValidateAsync(sessionMock.Object);
            var expected = new[]
            {
                "Machine name is required for a robot"
            };
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task ValidateRequiresSession()
        {
            var command = new AddRobotCommand();
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await command.ValidateAsync(null));
        }

        [Fact]
        public async Task ValidateSetsMissingFriendlyName()
        {
            var data = new Robot[0].AsRavenQueryable();
            var sessionMock = new Mock<IAsyncDocumentSession>();
            sessionMock.Setup(s => s.Query<Robot>(null, null, false)).Returns(data);

            var command = new AddRobotCommand { MachineName = "Old" };
            var result = await command.ValidateAsync(sessionMock.Object);
            var expected = new string[0];
            Assert.Equal(command.MachineName, command.FriendlyName);
        }
    }
}