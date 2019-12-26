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
    public class UpdateRobotCommandTests
    {
        [Fact]
        public async Task ApplyRequiresSession()
        {
            var command = new UpdateRobotCommand();
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await command.ApplyAsync(null));
        }

        [Fact]
        public async Task ApplyUpdatesFriendlyName()
        {
            var data = new[]
            {
                new Robot { FriendlyName = "Old" }
            }; ;

            var sessionMock = new Mock<IAsyncDocumentSession>();
            sessionMock.Setup(s => s.Query<Robot>(null, null, false)).Returns(data.AsRavenQueryable());

            var command = new UpdateRobotCommand { FriendlyName = "Bill", CurrentMachineName = "Bob" };
            await command.ValidateAsync(sessionMock.Object);
            var result = await command.ApplyAsync(sessionMock.Object);
            Assert.True(result.WasSuccessful);
            Assert.Equal("Bill", data[0].FriendlyName);
        }

        [Fact]
        public async Task ApplyUpdatesMachineName()
        {
            var data = new[]
            {
                new Robot { MachineName = "Old" }
            }; ;

            var sessionMock = new Mock<IAsyncDocumentSession>();
            sessionMock.Setup(s => s.Query<Robot>(null, null, false)).Returns(data.AsRavenQueryable());

            var command = new UpdateRobotCommand { MachineName = "Bill", CurrentMachineName = "Bob" };
            await command.ValidateAsync(sessionMock.Object);
            var result = await command.ApplyAsync(sessionMock.Object);
            Assert.True(result.WasSuccessful);
            Assert.Equal("Bill", data[0].MachineName);
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

            var command = new UpdateRobotCommand { CurrentMachineName = "Bob" };
            var result = await command.ValidateAsync(sessionMock.Object);
            var expected = new string[0];
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task ValidateHandlesMissingRobot()
        {
            var data = new Robot[0].AsRavenQueryable();

            var sessionMock = new Mock<IAsyncDocumentSession>();
            sessionMock.Setup(s => s.Query<Robot>(null, null, false)).Returns(data);

            var command = new UpdateRobotCommand { CurrentMachineName = "Bob" };
            var result = await command.ValidateAsync(sessionMock.Object);
            var expected = new[]
            {
                "Robot Bob does not exist"
            };
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task ValidateRequiresSession()
        {
            var command = new UpdateRobotCommand();
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await command.ValidateAsync(null));
        }
    }
}