using Microsoft.AspNetCore.Mvc;
using Moq;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Queries;
using NaoBlocks.Web.Controllers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

using Data = NaoBlocks.Engine.Data;
using Transfer = NaoBlocks.Web.Dtos;

namespace NaoBlocks.Web.Tests.Controllers
{
    public class SnapshotsControllerTests
    {
        [Fact]
        public async Task PostCallsStoreSnapshot()
        {
            // Arrange
            var logger = new FakeLogger<SnapshotsController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<StoreSnapshot>(
                CommandResult.New(1, new Data.Snapshot()));
            var controller = new SnapshotsController(
                logger,
                engine);
            engine.ConfigureUser(controller, "Mia", Data.UserRole.Student);

            // Act
            var snapshot = new Transfer.Snapshot();
            var response = await controller.Post(snapshot);

            // Assert
            var result = Assert.IsType<ExecutionResult<Transfer.Snapshot>>(response.Value);
            Assert.True(result.Successful, "Expected result to be successful");
            engine.Verify();
        }

        [Fact]
        public async Task PostChecksForUser()
        {
            // Arrange
            var logger = new FakeLogger<SnapshotsController>();
            var engine = new FakeEngine();
            var controller = new SnapshotsController(
                logger,
                engine);

            // Act
            var snapshot = new Transfer.Snapshot();
            var response = await controller.Post(snapshot);

            // Assert
            var result = Assert.IsType<UnauthorizedResult>(response.Result);
        }

        [Fact]
        public async Task PostCopiesData()
        {
            // Arrange
            var logger = new FakeLogger<SnapshotsController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<StoreSnapshot>(
                CommandResult.New(1, new Data.Snapshot()));
            var controller = new SnapshotsController(
                logger,
                engine);
            engine.ConfigureUser(controller, "Mia", Data.UserRole.Student);

            // Act
            var snapshot = new Transfer.Snapshot
            {
                Source = "hōtaka",
                State = "tika",
                User = "Mia",
                Values = new List<Data.NamedValue>
                {
                    new Data.NamedValue{ Name = "tahi", Value = "rua"}
                }
            };
            var response = await controller.Post(snapshot);

            // Assert
            var command = Assert.IsType<StoreSnapshot>(engine.LastCommand);
            Assert.Equal("hōtaka", command.Source);
            Assert.Equal("tika", command.State);
            Assert.Equal("users/1", command.UserName);
            Assert.Equal(
                new[] { "tahi=rua"}, 
                command.Values?.Select(nv => $"{nv.Name}={nv.Value}").ToArray());
        }

        [Fact]
        public async Task PostValidatesIncomingData()
        {
            // Arrange
            var logger = new FakeLogger<SnapshotsController>();
            var engine = new FakeEngine();
            var controller = new SnapshotsController(
                logger,
                engine);

            // Act
            var response = await controller.Post(null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(response.Result);
        }
    }
}
