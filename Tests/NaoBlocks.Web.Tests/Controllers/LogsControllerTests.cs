using Microsoft.AspNetCore.Mvc;
using Moq;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Queries;
using NaoBlocks.Web.Controllers;
using NaoBlocks.Web.Dtos;
using System;
using System.Collections.Generic;
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

        [Fact]
        public async Task PostHandlesRobotTypeWithoutDirectLogging()
        {
            // Arrange
            var logger = new FakeLogger<LogsController>();
            var engine = new FakeEngine();
            var query = new Mock<RobotData>();
            var robot = new Data.Robot
            {
                Type = new Data.RobotType
                {
                    AllowDirectLogging = false
                }
            };
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.RetrieveByNameAsync("Mihīni", true))
                .Returns(Task.FromResult<Data.Robot?>(robot));
            var controller = new LogsController(
                logger,
                engine);

            // Act
            var request = new LogRequest
            {
                Action = "Any"
            };
            var response = await controller.Post("Mihīni", request);

            // Assert
            Assert.Null(response.Value);
            Assert.IsType<ForbidResult>(response.Result);
        }

        [Fact]
        public async Task PostHandlesUnknownAction()
        {
            // Arrange
            var logger = new FakeLogger<LogsController>();
            var engine = new FakeEngine();
            var query = new Mock<RobotData>();
            var robot = new Data.Robot
            {
                Type = new Data.RobotType
                {
                    AllowDirectLogging = true
                }
            };
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.RetrieveByNameAsync("Mihīni", true))
                .Returns(Task.FromResult<Data.Robot?>(robot));
            var controller = new LogsController(
                logger,
                engine);

            // Act
            var request = new LogRequest
            {
                Action = "Unknown"
            };
            var response = await controller.Post("Mihīni", request);

            // Assert
            var result = Assert.IsType<LogResult>(response.Value);
            Assert.Equal("Unknown action 'Unknown'", result.Error);
        }

        [Fact]
        public async Task PostHandlesUnknownRobot()
        {
            // Arrange
            var logger = new FakeLogger<LogsController>();
            var engine = new FakeEngine();
            var query = new Mock<RobotData>();
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.RetrieveByNameAsync("Mihīni", true))
                .Returns(Task.FromResult((Data.Robot?)null));
            var controller = new LogsController(
                logger,
                engine);

            // Act
            var request = new LogRequest
            {
                Action = "Any"
            };
            var response = await controller.Post("Mihīni", request);

            // Assert
            Assert.Null(response.Value);
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task PostHandlesUnsetAction()
        {
            // Arrange
            var logger = new FakeLogger<LogsController>();
            var engine = new FakeEngine();
            var query = new Mock<RobotData>();
            var robot = new Data.Robot
            {
                Type = new Data.RobotType
                {
                    AllowDirectLogging = true
                }
            };
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.RetrieveByNameAsync("Mihīni", true))
                .Returns(Task.FromResult<Data.Robot?>(robot));
            var controller = new LogsController(
                logger,
                engine);

            // Act
            var request = new LogRequest();
            var response = await controller.Post("Mihīni", request);

            // Assert
            var result = Assert.IsType<LogResult>(response.Value);
            Assert.Equal("Unknown action ''", result.Error);
        }

        [Fact]
        public async Task PostLogHandlesExecutionErrors()
        {
            // Arrange
            var logger = new FakeLogger<LogsController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<Batch>(new CommandResult(1, "Fake error"));
            var query = new Mock<RobotData>();
            var robot = new Data.Robot
            {
                Type = new Data.RobotType
                {
                    AllowDirectLogging = true
                }
            };
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.RetrieveByNameAsync("Mihīni", true))
                .Returns(Task.FromResult<Data.Robot?>(robot));
            var controller = new LogsController(
                logger,
                engine);

            // Act
            var request = new LogRequest
            {
                Action = "log",
                Messages = new[] { "Testing" }
            };
            var response = await controller.Post("Mihīni", request);

            // Assert
            var result = Assert.IsType<LogResult>(response.Value);
            Assert.Equal("Command failed: Fake error", result.Error);
        }

        [Fact]
        public async Task PostLogHandlesValidationErrors()
        {
            // Arrange
            var logger = new FakeLogger<LogsController>();
            var engine = new FakeEngine
            {
                OnValidate = c => new[] { new CommandError(1, "Testing") }
            };
            var query = new Mock<RobotData>();
            var robot = new Data.Robot
            {
                Type = new Data.RobotType
                {
                    AllowDirectLogging = true
                }
            };
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.RetrieveByNameAsync("Mihīni", true))
                .Returns(Task.FromResult<Data.Robot?>(robot));
            var controller = new LogsController(
                logger,
                engine);

            // Act
            var request = new LogRequest
            {
                Action = "log",
                Messages = new[] { "Testing" }
            };
            var response = await controller.Post("Mihīni", request);

            // Assert
            var result = Assert.IsType<LogResult>(response.Value);
            Assert.Equal("Command is invalid: Testing", result.Error);
        }

        [Fact]
        public async Task PostsAddHandlesNoLines()
        {
            // Arrange
            var logger = new FakeLogger<LogsController>();
            var engine = new FakeEngine();
            var query = new Mock<RobotData>();
            var robot = new Data.Robot
            {
                Type = new Data.RobotType
                {
                    AllowDirectLogging = true
                }
            };
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.RetrieveByNameAsync("Mihīni", true))
                .Returns(Task.FromResult<Data.Robot?>(robot));
            var controller = new LogsController(
                logger,
                engine);

            // Act
            var request = new LogRequest
            {
                Action = "log"
            };
            var response = await controller.Post("Mihīni", request);

            // Assert
            var result = Assert.IsType<LogResult>(response.Value);
            Assert.Null(result.Error);
            engine.Verify();
        }

        [Fact]
        public async Task PostsAddsLogLines()
        {
            // Arrange
            var logger = new FakeLogger<LogsController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<Batch>(CommandResult.New(1));
            var query = new Mock<RobotData>();
            var robot = new Data.Robot
            {
                Type = new Data.RobotType
                {
                    AllowDirectLogging = true
                }
            };
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.RetrieveByNameAsync("Mihīni", true))
                .Returns(Task.FromResult<Data.Robot?>(robot));
            var controller = new LogsController(
                logger,
                engine);

            // Act
            var request = new LogRequest
            {
                Action = "log",
                Messages = new[] { "Testing" }
            };
            var response = await controller.Post("Mihīni", request);

            // Assert
            var result = Assert.IsType<LogResult>(response.Value);
            Assert.Null(result.Error);
            engine.Verify();
        }

        [Fact]
        public async Task PostsAddsLogLinesWithTimeStamps()
        {
            // Arrange
            var logger = new FakeLogger<LogsController>();
            var commands = new List<CommandBase>();
            var engine = new FakeEngine
            {
                OnValidate = c =>
                {
                    commands.AddRange(((Batch)c).Commands);
                    return Array.Empty<CommandError>();
                }
            };
            engine.ExpectCommand<Batch>(CommandResult.New(1));
            var query = new Mock<RobotData>();
            var robot = new Data.Robot
            {
                Type = new Data.RobotType
                {
                    AllowDirectLogging = true
                }
            };
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.RetrieveByNameAsync("Mihīni", true))
                .Returns(Task.FromResult<Data.Robot?>(robot));
            var controller = new LogsController(
                logger,
                engine);

            // Act
            var request = new LogRequest
            {
                Action = "log",
                Messages = new[] { "Testing", "5:Timestamp" }
            };
            var response = await controller.Post("Mihīni", request);

            // Assert
            var result = Assert.IsType<LogResult>(response.Value);
            Assert.Null(result.Error);
            engine.Verify();
            var lines = commands.OfType<AddToRobotLog>()
                .Select(c => c.Description)
                .ToArray();
            Assert.Equal(
                new[]
                {
                    "Testing",
                    "Timestamp"
                },
                lines);
        }

        [Fact]
        public async Task PostsInitialiseConversation()
        {
            // Arrange
            var logger = new FakeLogger<LogsController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<StartRobotConversation>(CommandResult.New(1, new Data.Conversation { ConversationId = 2 }));
            engine.ExpectCommand<AddToRobotLog>(CommandResult.New(2));
            var query = new Mock<RobotData>();
            var robot = new Data.Robot
            {
                Type = new Data.RobotType
                {
                    AllowDirectLogging = true
                }
            };
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.RetrieveByNameAsync("Mihīni", true))
                .Returns(Task.FromResult<Data.Robot?>(robot));
            var controller = new LogsController(
                logger,
                engine);

            // Act
            var request = new LogRequest
            {
                Action = "init"
            };
            var response = await controller.Post("Mihīni", request);

            // Assert
            var result = Assert.IsType<LogResult>(response.Value);
            Assert.Null(result.Error);
            engine.Verify();
        }

        [Fact]
        public async Task PostsInitialiseHandlesExecutionError()
        {
            // Arrange
            var logger = new FakeLogger<LogsController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<StartRobotConversation>(new CommandResult(1, "Fake error"));
            var query = new Mock<RobotData>();
            var robot = new Data.Robot
            {
                Type = new Data.RobotType
                {
                    AllowDirectLogging = true
                }
            };
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.RetrieveByNameAsync("Mihīni", true))
                .Returns(Task.FromResult<Data.Robot?>(robot));
            var controller = new LogsController(
                logger,
                engine);

            // Act
            var request = new LogRequest
            {
                Action = "init"
            };
            var response = await controller.Post("Mihīni", request);

            // Assert
            var result = Assert.IsType<LogResult>(response.Value);
            Assert.Equal("Command failed: Fake error", result.Error);
        }

        [Fact]
        public async Task PostsInitialiseHandlesValidationError()
        {
            // Arrange
            var logger = new FakeLogger<LogsController>();
            var engine = new FakeEngine
            {
                OnValidate = c => new[] { new CommandError(1, "Testing") }
            };
            var query = new Mock<RobotData>();
            var robot = new Data.Robot
            {
                Type = new Data.RobotType
                {
                    AllowDirectLogging = true
                }
            };
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.RetrieveByNameAsync("Mihīni", true))
                .Returns(Task.FromResult<Data.Robot?>(robot));
            var controller = new LogsController(
                logger,
                engine);

            // Act
            var request = new LogRequest
            {
                Action = "init"
            };
            var response = await controller.Post("Mihīni", request);

            // Assert
            var result = Assert.IsType<LogResult>(response.Value);
            Assert.Equal("Command is invalid: Testing", result.Error);
        }
    }
}