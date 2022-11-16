using Microsoft.AspNetCore.Mvc;
using Moq;
using NaoBlocks.Common;
using NaoBlocks.Engine.Queries;
using NaoBlocks.Web.Controllers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

using Data = NaoBlocks.Engine.Data;

namespace NaoBlocks.Web.Tests.Controllers
{
    public class LogsControllerTests
    {
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
        public async Task GetLogsHandlesMissingRobot()
        {
            // Arrange
            var logger = new FakeLogger<LogsController>();
            var engine = new FakeEngine();
            var conversationQuery = new Mock<ConversationData>();
            var robotQuery = new Mock<RobotData>();
            var results = ListResult.New(new[]
            {
                new Data.RobotLog()
            });
            engine.RegisterQuery(conversationQuery.Object);
            engine.RegisterQuery(robotQuery.Object);
            robotQuery.Setup(q => q.RetrieveByNameAsync("Mihīni", false))
                .Returns(Task.FromResult((Data.Robot?)null));
            conversationQuery.Setup(q => q.RetrieveRobotLogsPageAsync("Mihīni", 0, It.IsAny<int>()))
                .Returns(Task.FromResult(results));
            var controller = new LogsController(
                logger,
                engine);

            // Act
            var response = await controller.GetLogs("Mihīni", null, null);

            // Assert
            Assert.IsType<NotFoundResult>(response.Result);
            Assert.Null(response.Value);
        }

        [Fact]
        public async Task GetLogsRetrievesLogs()
        {
            // Arrange
            var logger = new FakeLogger<LogsController>();
            var engine = new FakeEngine();
            var conversationQuery = new Mock<ConversationData>();
            var robotQuery = new Mock<RobotData>();
            var results = ListResult.New(new[]
            {
                new Data.RobotLog {
                    Conversation = new Data.Conversation { ConversationId = 123456}
                }
            });
            engine.RegisterQuery(conversationQuery.Object);
            engine.RegisterQuery(robotQuery.Object);
            robotQuery.Setup(q => q.RetrieveByNameAsync("Mihīni", false))
                .Returns(Task.FromResult((Data.Robot?)new Data.Robot { MachineName = "Mihīni" }));
            conversationQuery.Setup(q => q.RetrieveRobotLogsPageAsync("Mihīni", 0, It.IsAny<int>()))
                .Returns(Task.FromResult(results));
            var controller = new LogsController(
                logger,
                engine);

            // Act
            var response = await controller.GetLogs("Mihīni", null, null);

            // Assert
            Assert.Null(response.Result);
            Assert.Equal(1, response?.Value?.Count);
            Assert.Equal(
                new long[] { 123456 },
                response?.Value?.Items?.Select(l => l.ConversationId).ToArray());
        }

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
    }
}