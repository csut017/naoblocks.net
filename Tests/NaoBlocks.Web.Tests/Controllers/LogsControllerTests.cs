using Microsoft.AspNetCore.Mvc;
using Moq;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Queries;
using NaoBlocks.Parser;
using NaoBlocks.Web.Controllers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

using Data = NaoBlocks.Engine.Data;
using Transfer = NaoBlocks.Web.Dtos;

namespace NaoBlocks.Web.Tests.Controllers
{
    public class LogsControllerTests
    {
        [Fact]
        public async Task GetRetrievesViaQuery()
        {
            // Arrange
            var now = DateTime.Now;
            var logger = new FakeLogger<LogsController>();
            var engine = new FakeEngine();
            var query = new Mock<ConversationData>();
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.RetrieveRobotLogAsync(4, "Mihīni"))
                .Returns(Task.FromResult((Data.RobotLog?)new Data.RobotLog { WhenAdded = now }));
            var controller = new LogsController(
                logger,
                engine);

            // Act
            var response = await controller.Get("Mihīni", "4");

            // Assert
            Assert.NotNull(response.Value);
            Assert.Equal(now, response.Value?.WhenAdded);
        }

        [Fact]
        public async Task GetHandlesMissingLog()
        {
            // Arrange
            var logger = new FakeLogger<LogsController>();
            var engine = new FakeEngine();
            var query = new Mock<ConversationData>();
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.RetrieveRobotLogAsync(4, "Mihīni"))
                .Returns(Task.FromResult((Data.RobotLog?)null));
            var controller = new LogsController(
                logger,
                engine);

            // Act
            var response = await controller.Get("Mihīni", "4");

            // Assert
            Assert.IsType<NotFoundResult>(response.Result);
            Assert.Null(response.Value);
        }

        [Fact]
        public async Task GetHandlesInvalidData()
        {
            // Arrange
            var logger = new FakeLogger<LogsController>();
            var engine = new FakeEngine();
            var query = new Mock<ConversationData>();
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.RetrieveRobotLogAsync(4, "Mihīni"))
                .Returns(Task.FromResult((Data.RobotLog?)null));
            var controller = new LogsController(
                logger,
                engine);

            // Act
            var response = await controller.Get("Mihīni", "bad");

            // Assert
            Assert.Null(response.Value);
            Assert.IsType<BadRequestObjectResult>(response.Result);
        }
    }
}
