using Microsoft.AspNetCore.Mvc;
using Moq;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Queries;
using NaoBlocks.Web.Controllers;
using System.Threading.Tasks;
using Xunit;

using Data = NaoBlocks.Engine.Data;
using Transfer = NaoBlocks.Web.Dtos;

namespace NaoBlocks.Web.Tests.Controllers
{
    public class RobotsControllerTests
    {
        [Fact]
        public async Task DeleteCallsDelete()
        {
            // Arrange
            var logger = new FakeLogger<RobotsController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<DeleteRobot>();
            var controller = new RobotsController(
                logger,
                engine);

            // Act
            var response = await controller.Delete("karetao");

            // Assert
            var result = Assert.IsType<ExecutionResult>(response.Value);
            Assert.True(result.Successful, "Expected result to be successful");
            engine.Verify();
        }

        [Fact]
        public async Task GetRetrievesUserViaQuery()
        {
            // Arrange
            var logger = new FakeLogger<RobotsController>();
            var engine = new FakeEngine();
            var controller = new RobotsController(
                logger,
                engine);
            var query = new Mock<RobotData>();
            query.Setup(q => q.RetrieveByNameAsync("karetao", true))
                .Returns(Task.FromResult((Data.Robot?)new Data.Robot { MachineName = "karetao" }));
            engine.RegisterQuery(query.Object);

            // Act
            var response = await controller.Get("karetao");

            // Assert
            Assert.NotNull(response.Value);
            Assert.Equal("karetao", response.Value?.MachineName);
        }

        [Fact]
        public async Task GetHandlesMissingRobot()
        {
            // Arrange
            var logger = new FakeLogger<RobotsController>();
            var engine = new FakeEngine();
            var controller = new RobotsController(
                logger,
                engine);
            var query = new Mock<RobotData>();
            query.Setup(q => q.RetrieveByNameAsync("karetao", true))
                .Returns(Task.FromResult((Data.Robot?)null));
            engine.RegisterQuery(query.Object);

            // Act
            var response = await controller.Get("karetao");

            // Assert
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task ListRetrievesViaQuery()
        {
            // Arrange
            var logger = new FakeLogger<RobotsController>();
            var engine = new FakeEngine();
            var query = new Mock<RobotData>();
            var result = ListResult.New(
                new[] { new Data.Robot() });
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.RetrievePageAsync(0, 25, null))
                .Returns(Task.FromResult(result));
            var controller = new RobotsController(
                logger,
                engine);

            // Act
            var response = await controller.List(null, null, null);

            // Assert
            Assert.Equal(1, response.Value?.Count);
            Assert.Equal(0, response.Value?.Page);
            Assert.NotEmpty(response.Value?.Items);
        }

        [Fact]
        public async Task ListHandlesTypeFilter()
        {
            // Arrange
            var logger = new FakeLogger<RobotsController>();
            var engine = new FakeEngine();
            var robotQuery = new Mock<RobotData>();
            var typeQuery = new Mock<RobotTypeData>();
            var result = ListResult.New(
                new[] { new Data.Robot() });
            engine.RegisterQuery(robotQuery.Object);
            engine.RegisterQuery(typeQuery.Object);
            robotQuery.Setup(q => q.RetrievePageAsync(0, 25, "types/1"))
                .Returns(Task.FromResult(result));
            typeQuery.Setup(q => q.RetrieveByNameAsync("karetao"))
                .Returns(Task.FromResult((Data.RobotType?)new Data.RobotType { Name = "karetao", Id = "types/1" }));
            var controller = new RobotsController(
                logger,
                engine);

            // Act
            var response = await controller.List(null, null, "karetao");

            // Assert
            Assert.Equal(1, response.Value?.Count);
            Assert.Equal(0, response.Value?.Page);
            Assert.NotEmpty(response.Value?.Items);
        }

        [Fact]
        public async Task ListHandlesUnknownType()
        {
            // Arrange
            var logger = new FakeLogger<RobotsController>();
            var engine = new FakeEngine();
            var typeQuery = new Mock<RobotTypeData>();
            engine.RegisterQuery(typeQuery.Object);
            typeQuery.Setup(q => q.RetrieveByNameAsync("karetao"))
                .Returns(Task.FromResult((Data.RobotType?)null));
            var controller = new RobotsController(
                logger,
                engine);

            // Act
            var response = await controller.List(null, null, "karetao");

            // Assert
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task ListHandlesNullData()
        {
            // Arrange
            var logger = new FakeLogger<RobotsController>();
            var engine = new FakeEngine();
            var query = new Mock<RobotData>();
            var result = new ListResult<Data.Robot>();
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.RetrievePageAsync(0, 25, null))
                .Returns(Task.FromResult(result));
            var controller = new RobotsController(
                logger,
                engine);

            // Act
            var response = await controller.List(null, null, null);

            // Assert
            Assert.Equal(0, response.Value?.Count);
            Assert.Equal(0, response.Value?.Page);
            Assert.Null(response.Value?.Items);
        }
    }
}
