using Microsoft.AspNetCore.Mvc;
using Moq;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Queries;
using NaoBlocks.Web.Controllers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

using Data = NaoBlocks.Engine.Data;
using Generators = NaoBlocks.Engine.Generators;
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

        [Theory]
        [InlineData(null, ReportFormat.Excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "karetao-logs.xlsx")]
        [InlineData("csv", ReportFormat.Csv, "text/csv", "karetao-logs.csv")]
        [InlineData("CSV", ReportFormat.Csv, "text/csv", "karetao-logs.csv")]
        [InlineData(".csv", ReportFormat.Csv, "text/csv", "karetao-logs.csv")]
        public async Task ExportDetailsGeneratesReport(string? format, ReportFormat expected, string contentType, string fileName)
        {
            // Arrange
            var logger = new FakeLogger<RobotsController>();
            var engine = new FakeEngine();
            var generator = new Mock<Generators.RobotExport>();
            var result = Tuple.Create((Stream)new MemoryStream(), fileName);
            generator.Setup(g => g.GenerateAsync(expected))
                .Returns(Task.FromResult(result))
                .Verifiable();
            generator.Setup(g => g.IsFormatAvailable(expected))
                .Returns(true)
                .Verifiable();
            engine.RegisterGenerator(generator.Object);
            var query = new Mock<RobotData>();
            query.Setup(q => q.RetrieveByNameAsync("karetao", false))
                .Returns(Task.FromResult((Data.Robot?)new Data.Robot { MachineName = "karetao" }));
            engine.RegisterQuery(query.Object);
            var controller = new RobotsController(
                logger,
                engine);

            // Act
            var response = await controller.ExportDetails("karetao", format);

            // Assert
            var streamResult = Assert.IsType<FileStreamResult>(response);
            generator.Verify();
            Assert.Equal(contentType, streamResult.ContentType);
            Assert.Equal(fileName, streamResult.FileDownloadName);
        }

        [Fact]
        public async Task ExportDetailsHandlesInvalidInput()
        {
            // Arrange
            var logger = new FakeLogger<RobotsController>();
            var engine = new FakeEngine();
            var query = new Mock<RobotData>();
            query.Setup(q => q.RetrieveByNameAsync("karetao", false))
                .Returns(Task.FromResult((Data.Robot?)new Data.Robot { MachineName = "karetao" }));
            engine.RegisterQuery(query.Object);
            var controller = new RobotsController(
                logger,
                engine);

            // Act
            var response = await controller.ExportDetails("karetao", "garbage");

            // Assert
            Assert.IsType<BadRequestObjectResult>(response);
        }

        [Fact]
        public async Task ExportDetailsHandlesUnknownFormat()
        {
            // Arrange
            var logger = new FakeLogger<RobotsController>();
            var engine = new FakeEngine();
            var generator = new Mock<Generators.RobotExport>();
            engine.RegisterGenerator(generator.Object);
            var query = new Mock<RobotData>();
            query.Setup(q => q.RetrieveByNameAsync("karetao", false))
                .Returns(Task.FromResult((Data.Robot?)new Data.Robot { MachineName = "karetao" }));
            engine.RegisterQuery(query.Object);
            var controller = new RobotsController(
                logger,
                engine);

            // Act
            var response = await controller.ExportDetails("karetao", "unknown");

            // Assert
            Assert.IsType<BadRequestObjectResult>(response);
        }

        [Theory]
        [InlineData(null, ReportFormat.Excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Robots-List.xlsx")]
        [InlineData("xlsx", ReportFormat.Excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Robots-List.xlsx")]
        [InlineData("Pdf", ReportFormat.Pdf, "application/pdf", "Robots-List.pdf")]
        [InlineData("pdf", ReportFormat.Pdf, "application/pdf", "Robots-List.pdf")]
        [InlineData("PDF", ReportFormat.Pdf, "application/pdf", "Robots-List.pdf")]
        public async Task ExportListGeneratesReport(string? format, ReportFormat expected, string contentType, string fileName)
        {
            // Arrange
            var logger = new FakeLogger<RobotsController>();
            var engine = new FakeEngine();
            var generator = new Mock<Generators.RobotsList>();
            var result = Tuple.Create((Stream)new MemoryStream(), fileName);
            generator.Setup(g => g.GenerateAsync(expected))
                .Returns(Task.FromResult(result))
                .Verifiable();
            generator.Setup(g => g.IsFormatAvailable(expected))
                .Returns(true)
                .Verifiable();
            engine.RegisterGenerator(generator.Object);
            var controller = new RobotsController(
                logger,
                engine);

            // Act
            var response = await controller.ExportList(format);

            // Assert
            var streamResult = Assert.IsType<FileStreamResult>(response);
            generator.Verify();
            Assert.Equal(contentType, streamResult.ContentType);
            Assert.Equal(fileName, streamResult.FileDownloadName);
        }

        [Fact]
        public async Task ExportListHandlesInvalidInput()
        {
            // Arrange
            var logger = new FakeLogger<RobotsController>();
            var engine = new FakeEngine();
            var controller = new RobotsController(
                logger,
                engine);

            // Act
            var response = await controller.ExportList("garbage");

            // Assert
            Assert.IsType<BadRequestObjectResult>(response);
        }

        [Fact]
        public async Task ExportListHandlesUnknownFormat()
        {
            // Arrange
            var logger = new FakeLogger<RobotsController>();
            var engine = new FakeEngine();
            var generator = new Mock<Generators.RobotsList>();
            engine.RegisterGenerator(generator.Object);
            var controller = new RobotsController(
                logger,
                engine);

            // Act
            var response = await controller.ExportList("unknown");

            // Assert
            Assert.IsType<BadRequestObjectResult>(response);
        }

        [Theory]
        [InlineData(null, ReportFormat.Excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "karetao-logs.xlsx")]
        [InlineData("csv", ReportFormat.Csv, "text/csv", "karetao-logs.csv")]
        [InlineData("CSV", ReportFormat.Csv, "text/csv", "karetao-logs.csv")]
        [InlineData(".csv", ReportFormat.Csv, "text/csv", "karetao-logs.csv")]
        public async Task ExportLogsGeneratesReport(string? format, ReportFormat expected, string contentType, string fileName)
        {
            // Arrange
            var logger = new FakeLogger<RobotsController>();
            var engine = new FakeEngine();
            var generator = new Mock<Generators.RobotLogs>();
            var result = Tuple.Create((Stream)new MemoryStream(), fileName);
            generator.Setup(g => g.GenerateAsync(expected))
                .Returns(Task.FromResult(result))
                .Verifiable();
            generator.Setup(g => g.IsFormatAvailable(expected))
                .Returns(true)
                .Verifiable();
            engine.RegisterGenerator(generator.Object);
            var query = new Mock<RobotData>();
            query.Setup(q => q.RetrieveByNameAsync("karetao", false))
                .Returns(Task.FromResult((Data.Robot?)new Data.Robot { MachineName = "karetao" }));
            engine.RegisterQuery(query.Object);
            var controller = new RobotsController(
                logger,
                engine);

            // Act
            var response = await controller.ExportLogs("karetao", format);

            // Assert
            var streamResult = Assert.IsType<FileStreamResult>(response);
            generator.Verify();
            Assert.Equal(contentType, streamResult.ContentType);
            Assert.Equal(fileName, streamResult.FileDownloadName);
        }

        [Fact]
        public async Task ExportLogsHandlesInvalidInput()
        {
            // Arrange
            var logger = new FakeLogger<RobotsController>();
            var engine = new FakeEngine();
            var query = new Mock<RobotData>();
            query.Setup(q => q.RetrieveByNameAsync("karetao", false))
                .Returns(Task.FromResult((Data.Robot?)new Data.Robot { MachineName = "karetao" }));
            engine.RegisterQuery(query.Object);
            var controller = new RobotsController(
                logger,
                engine);

            // Act
            var response = await controller.ExportLogs("karetao", "garbage");

            // Assert
            Assert.IsType<BadRequestObjectResult>(response);
        }

        [Fact]
        public async Task ExportLogsHandlesUnknownFormat()
        {
            // Arrange
            var logger = new FakeLogger<RobotsController>();
            var engine = new FakeEngine();
            var generator = new Mock<Generators.RobotLogs>();
            engine.RegisterGenerator(generator.Object);
            var query = new Mock<RobotData>();
            query.Setup(q => q.RetrieveByNameAsync("karetao", false))
                .Returns(Task.FromResult((Data.Robot?)new Data.Robot { MachineName = "karetao" }));
            engine.RegisterQuery(query.Object);
            var controller = new RobotsController(
                logger,
                engine);

            // Act
            var response = await controller.ExportLogs("karetao", "unknown");

            // Assert
            Assert.IsType<BadRequestObjectResult>(response);
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
        public async Task ImportCallsCommand()
        {
            // Arrange
            var robots = new List<Data.Robot>();
            var logger = new FakeLogger<RobotsController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<ParseRobotsImport>(
                CommandResult.New(1, robots.AsReadOnly()));
            var controller = new RobotsController(
                logger,
                engine);
            controller.SetRequestFiles("first");

            // Act
            var response = await controller.Import("parse");

            // Assert
            Assert.IsType<ExecutionResult<ListResult<Transfer.Robot>>>(response.Value);
            engine.Verify();
        }

        [Fact]
        public async Task ImportChecksAction()
        {
            // Arrange
            var logger = new FakeLogger<RobotsController>();
            var engine = new FakeEngine();
            var controller = new RobotsController(
                logger,
                engine);

            // Act
            var response = await controller.Import("unknown");

            // Assert
            Assert.IsType<BadRequestObjectResult>(response.Result);
        }

        [Fact]
        public async Task ImportChecksForAnyFiles()
        {
            // Arrange
            var logger = new FakeLogger<RobotsController>();
            var engine = new FakeEngine();
            var controller = new RobotsController(
                logger,
                engine);
            controller.SetRequestFiles();

            // Act
            var response = await controller.Import("parse");

            // Assert
            Assert.IsType<BadRequestObjectResult>(response.Result);
        }

        [Fact]
        public async Task ImportChecksForFormInput()
        {
            // Arrange
            var logger = new FakeLogger<RobotsController>();
            var engine = new FakeEngine();
            var controller = new RobotsController(
                logger,
                engine);

            // Act
            var response = await controller.Import("parse");

            // Assert
            Assert.IsType<BadRequestObjectResult>(response.Result);
        }

        [Fact]
        public async Task ImportChecksForOnlyOneFile()
        {
            // Arrange
            var logger = new FakeLogger<RobotsController>();
            var engine = new FakeEngine();
            var controller = new RobotsController(
                logger,
                engine);
            controller.SetRequestFiles("first", "second");

            // Act
            var response = await controller.Import("parse");

            // Assert
            Assert.IsType<BadRequestObjectResult>(response.Result);
        }

        [Fact]
        public async Task ImportIncudesParseDetails()
        {
            // Arrange
            var robots = new List<Data.Robot>
            {
                new Data.Robot { Message = "Parsed"}
            };
            var logger = new FakeLogger<RobotsController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<ParseRobotsImport>(
                CommandResult.New(1, robots.AsReadOnly()));
            var controller = new RobotsController(
                logger,
                engine);
            controller.SetRequestFiles("first");

            // Act
            var response = await controller.Import("parse");

            // Assert
            var output = Assert.IsType<ExecutionResult<ListResult<Transfer.Robot>>>(response.Value);
            engine.Verify();
            Assert.Equal(
                new[] { "Parsed" },
                output?.Output?.Items?.Select(r => r.Parse?.Message).ToArray());
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
            Assert.NotEmpty(response.Value?.Items ?? Array.Empty<Transfer.Robot>());
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
            Assert.NotEmpty(response.Value?.Items ?? Array.Empty<Transfer.Robot>());
        }

        [Fact]
        public async Task PostCallsAddsRobot()
        {
            // Arrange
            var logger = new FakeLogger<RobotsController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<AddRobot>(
                CommandResult.New(1, new Data.Robot()));
            var controller = new RobotsController(
                logger,
                engine);

            // Act
            var robot = new Transfer.Robot { MachineName = "karetao" };
            var response = await controller.Post(robot);

            // Assert
            var result = Assert.IsType<ExecutionResult<Transfer.Robot>>(response.Value);
            Assert.True(result.Successful, "Expected result to be successful");
            engine.Verify();
            var command = Assert.IsType<AddRobot>(engine.LastCommand);
            Assert.Equal("karetao", command.MachineName);
        }

        [Fact]
        public async Task PostValidatesIncomingData()
        {
            // Arrange
            var logger = new FakeLogger<RobotsController>();
            var engine = new FakeEngine();
            var controller = new RobotsController(
                logger,
                engine);

            // Act
            var response = await controller.Post(null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(response.Result);
        }

        [Fact]
        public async Task PostValuesCallsCorrectCommand()
        {
            // Arrange
            var robot = new Data.Robot();
            var logger = new FakeLogger<RobotsController>();
            var engine = new FakeEngine();
            var robotQuery = new Mock<RobotData>();
            robotQuery.Setup(q => q.RetrieveByNameAsync("karetao", false))
                .Returns(Task.FromResult<Data.Robot?>(robot));
            engine.RegisterQuery(robotQuery.Object);
            engine.ExpectCommand<UpdateCustomValuesForRobot>();
            var controller = new RobotsController(
                logger,
                engine);

            // Act
            var values = Transfer.Set.New(
                Data.NamedValue.New("one", "tahi"));
            var response = await controller.PostValues("karetao", values);

            // Assert
            var result = Assert.IsType<ExecutionResult>(response.Value);
            Assert.True(result.Successful, "Expected result to be successful");
            engine.Verify();
        }

        [Fact]
        public async Task PostValuesCheckRobotExists()
        {
            // Arrange
            var logger = new FakeLogger<RobotsController>();
            var engine = new FakeEngine();
            var robotQuery = new Mock<RobotData>();
            robotQuery.Setup(q => q.RetrieveByNameAsync("karetao", false))
                .Returns(Task.FromResult<Data.Robot?>(null));
            engine.RegisterQuery(robotQuery.Object);
            var controller = new RobotsController(
                logger,
                engine);

            // Act
            var values = Transfer.Set.New(
                Data.NamedValue.New("one", "tahi"));
            var response = await controller.PostValues("karetao", values);

            // Assert
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task PutCallsUpdateRobot()
        {
            // Arrange
            var logger = new FakeLogger<RobotsController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<UpdateRobot>(
                CommandResult.New(1, new Data.Robot()));
            var controller = new RobotsController(
                logger,
                engine);

            // Act
            var robot = new Transfer.Robot { FriendlyName = "Mihīni" };
            var response = await controller.Put("karetao", robot);

            // Assert
            var result = Assert.IsType<ExecutionResult<Transfer.Robot>>(response.Value);
            Assert.True(result.Successful, "Expected result to be successful");
            engine.Verify();

            var command = Assert.IsType<UpdateRobot>(engine.LastCommand);
            Assert.Equal("karetao", command.MachineName);
            Assert.Equal("Mihīni", command.FriendlyName);
        }

        [Fact]
        public async Task PutValidatesIncomingData()
        {
            // Arrange
            var logger = new FakeLogger<RobotsController>();
            var engine = new FakeEngine();
            var controller = new RobotsController(
                logger,
                engine);

            // Act
            var response = await controller.Put("karetao", null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(response.Result);
        }

        [Fact]
        public async Task RegisterCallsAddsRobot()
        {
            // Arrange
            var logger = new FakeLogger<RobotsController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<RegisterRobot>(
                CommandResult.New(1, new Data.Robot()));
            var controller = new RobotsController(
                logger,
                engine);

            // Act
            var robot = new Transfer.Robot { MachineName = "karetao" };
            var response = await controller.Register(robot);

            // Assert
            var result = Assert.IsType<ExecutionResult<Transfer.Robot>>(response.Value);
            Assert.True(result.Successful, "Expected result to be successful");
            engine.Verify();
            var command = Assert.IsType<RegisterRobot>(engine.LastCommand);
            Assert.Equal("karetao", command.MachineName);
        }

        [Fact]
        public async Task RegisterValidatesIncomingData()
        {
            // Arrange
            var logger = new FakeLogger<RobotsController>();
            var engine = new FakeEngine();
            var controller = new RobotsController(
                logger,
                engine);

            // Act
            var response = await controller.Register(null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(response.Result);
        }
    }
}