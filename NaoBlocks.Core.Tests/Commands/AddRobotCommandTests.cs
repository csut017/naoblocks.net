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
    public class AddRobotCommandTests
    {
        [Fact]
        public async Task ApplyRequiresSession()
        {
            var command = new AddRobot();
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await command.ApplyAsync(null));
        }

        [Fact]
        public async Task ApplySetsWhenAdded()
        {
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var command = new AddRobot { MachineName = "testing-robot", WhenExecuted = new DateTime(2019, 1, 1) };
            var result = (await command.ApplyAsync(sessionMock.Object)).As<Robot>();
            Assert.Equal(command.WhenExecuted, result.Output?.WhenAdded);
        }

        [Fact]
        public async Task ApplyStoresRobot()
        {
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var command = new AddRobot { MachineName = "testing-robot" };
            var result = await command.ApplyAsync(sessionMock.Object);
            Assert.True(result.WasSuccessful);
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

            var command = new AddRobot { MachineName = "Old" };
            var result = await command.ValidateAsync(sessionMock.Object);
            var expected = new[]
            {
                "Robot with name Old already exists"
            };
            Assert.Equal(expected, result.Select(r => r.Error));
        }

        [Fact]
        public async Task ValidatePassesAllChecks()
        {
            var data = new Robot[0].AsRavenQueryable();
            var sessionMock = new Mock<IAsyncDocumentSession>();
            sessionMock.Setup(s => s.Query<Robot>(null, null, false)).Returns(data);

            var command = new AddRobot { MachineName = "Old", FriendlyName = "Old Robot" };
            var result = await command.ValidateAsync(sessionMock.Object);
            var expected = new string[0];
            Assert.Equal(expected, result.Select(r => r.Error));
        }

        [Fact]
        public async Task ValidateRequiresName()
        {
            var sessionMock = new Mock<IAsyncDocumentSession>();
            var command = new AddRobot { MachineName = string.Empty };
            var result = await command.ValidateAsync(sessionMock.Object);
            var expected = new[]
            {
                "Machine name is required for a robot"
            };
            Assert.Equal(expected, result.Select(r => r.Error));
        }

        [Fact]
        public async Task ValidateRequiresSession()
        {
            var command = new AddRobot();
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await command.ValidateAsync(null));
        }

        [Fact]
        public async Task ValidateSetsMissingFriendlyName()
        {
            var data = new Robot[0].AsRavenQueryable();
            var sessionMock = new Mock<IAsyncDocumentSession>();
            sessionMock.Setup(s => s.Query<Robot>(null, null, false)).Returns(data);

            var command = new AddRobot { MachineName = "Old" };
            var result = await command.ValidateAsync(sessionMock.Object);
            var expected = new string[0];
            Assert.Equal(command.MachineName, command.FriendlyName);
        }
    }
}