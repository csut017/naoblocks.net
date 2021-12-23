using Microsoft.AspNetCore.Hosting;
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
    public class RobotTypesControllerTests
    {
        [Fact]
        public async Task DeleteCallsDelete()
        {
            // Arrange
            var logger = new FakeLogger<RobotTypesController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<DeleteRobotType>();
            Mock<IWebHostEnvironment> env = InitialiseEnvironment();
            var controller = new RobotTypesController(
                logger,
                engine,
                env.Object);

            // Act
            var response = await controller.Delete("karetao");

            // Assert
            var result = Assert.IsType<ExecutionResult>(response.Value);
            Assert.True(result.Successful, "Expected result to be successful");
            engine.Verify();
        }

        [Fact]
        public async Task GetHandlesMissing()
        {
            // Arrange
            var logger = new FakeLogger<RobotTypesController>();
            var engine = new FakeEngine();
            Mock<IWebHostEnvironment> env = InitialiseEnvironment();
            var controller = new RobotTypesController(
                logger,
                engine,
                env.Object);
            var query = new Mock<RobotTypeData>();
            query.Setup(q => q.RetrieveByNameAsync("karetao"))
                .Returns(Task.FromResult((Data.RobotType?)null));
            engine.RegisterQuery(query.Object);

            // Act
            var response = await controller.Get("karetao");

            // Assert
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task GetRetrievesViaQuery()
        {
            // Arrange
            var logger = new FakeLogger<RobotTypesController>();
            var engine = new FakeEngine();
            Mock<IWebHostEnvironment> env = InitialiseEnvironment();
            var controller = new RobotTypesController(
                logger,
                engine,
                env.Object);
            var query = new Mock<RobotTypeData>();
            query.Setup(q => q.RetrieveByNameAsync("karetao"))
                .Returns(Task.FromResult((Data.RobotType?)new Data.RobotType { Name = "karetao" }));
            engine.RegisterQuery(query.Object);

            // Act
            var response = await controller.Get("karetao");

            // Assert
            Assert.NotNull(response.Value);
            Assert.Equal("karetao", response.Value?.Name);
        }

        [Fact]
        public async Task PostCallsAddsRobot()
        {
            // Arrange
            var logger = new FakeLogger<RobotTypesController>();
            var engine = new FakeEngine
            {
                OnExecute = c => CommandResult.New(1, new Data.RobotType())
            };
            engine.ExpectCommand<AddRobotType>();
            Mock<IWebHostEnvironment> env = InitialiseEnvironment();
            var controller = new RobotTypesController(
                logger,
                engine,
                env.Object);

            // Act
            var type = new Transfer.RobotType { Name = "karetao" };
            var response = await controller.Post(type);

            // Assert
            var result = Assert.IsType<ExecutionResult<Transfer.RobotType>>(response.Value);
            Assert.True(result.Successful, "Expected result to be successful");
            engine.Verify();
            var command = Assert.IsType<AddRobotType>(engine.LastCommand);
            Assert.Equal("karetao", command.Name);
        }

        [Fact]
        public async Task PostValidatesIncomingData()
        {
            // Arrange
            var logger = new FakeLogger<RobotTypesController>();
            var engine = new FakeEngine();
            Mock<IWebHostEnvironment> env = InitialiseEnvironment();
            var controller = new RobotTypesController(
                logger,
                engine,
                env.Object);

            // Act
            var response = await controller.Post(null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(response.Result);
        }

        [Fact]
        public async Task PutCallsUpdateRobot()
        {
            // Arrange
            var logger = new FakeLogger<RobotTypesController>();
            var engine = new FakeEngine
            {
                OnExecute = c => CommandResult.New(1, new Data.RobotType())
            };
            engine.ExpectCommand<UpdateRobotType>();
            Mock<IWebHostEnvironment> env = InitialiseEnvironment();
            var controller = new RobotTypesController(
                logger,
                engine,
                env.Object);

            // Act
            var type = new Transfer.RobotType { Name = "Mihīni" };
            var response = await controller.Put("karetao", type);

            // Assert
            var result = Assert.IsType<ExecutionResult<Transfer.RobotType>>(response.Value);
            Assert.True(result.Successful, "Expected result to be successful");
            engine.Verify();

            var command = Assert.IsType<UpdateRobotType>(engine.LastCommand);
            Assert.Equal("Mihīni", command.Name);
        }

        [Fact]
        public async Task PutValidatesIncomingData()
        {
            // Arrange
            var logger = new FakeLogger<RobotTypesController>();
            var engine = new FakeEngine();
            Mock<IWebHostEnvironment> env = InitialiseEnvironment();
            var controller = new RobotTypesController(
                logger,
                engine,
                env.Object);

            // Act
            var response = await controller.Put("karetao", null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(response.Result);
        }

        private static Mock<IWebHostEnvironment> InitialiseEnvironment(string value = "http://test")
        {
            var env = new Mock<IWebHostEnvironment>();
            env.Setup(e => e.WebRootPath).Returns(value);
            return env;
        }
    }
}