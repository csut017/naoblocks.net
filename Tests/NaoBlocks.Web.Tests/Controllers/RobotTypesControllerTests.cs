﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Queries;
using NaoBlocks.Web.Controllers;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Data = NaoBlocks.Engine.Data;
using Generators = NaoBlocks.Engine.Generators;
using Transfer = NaoBlocks.Web.Dtos;

namespace NaoBlocks.Web.Tests.Controllers
{
    public class RobotTypesControllerTests
    {
        private const string ToolBoxXml = "<toolbox/>";

        [Fact]
        public async Task DeleteCallsDelete()
        {
            // Arrange
            var engine = new FakeEngine();
            engine.ExpectCommand<DeleteRobotType>();
            var controller = InitialiseController(engine);

            // Act
            var response = await controller.Delete("karetao");

            // Assert
            var result = Assert.IsType<ExecutionResult>(response.Value);
            Assert.True(result.Successful, "Expected result to be successful");
            engine.Verify();
            Assert.Equal("karetao", ((DeleteRobotType)engine.LastCommand!).Name);
        }

        [Fact]
        public async Task DeleteToolboxCallsDelete()
        {
            // Arrange
            var engine = new FakeEngine();
            engine.ExpectCommand<DeleteToolbox>();
            var controller = InitialiseController(engine);

            // Act
            var response = await controller.DeleteToolbox("karetao", "testing");

            // Assert
            var result = Assert.IsType<ExecutionResult>(response.Value);
            Assert.True(result.Successful, "Expected result to be successful");
            engine.Verify();
            Assert.Equal("karetao", ((DeleteToolbox)engine.LastCommand!).RobotTypeName);
            Assert.Equal("testing", ((DeleteToolbox)engine.LastCommand!).ToolboxName);
        }

        [Theory]
        [InlineData(null, ReportFormat.Zip, "application/zip", "karetao-package.zip")]
        [InlineData("Zip", ReportFormat.Zip, "application/zip", "karetao-package.zip")]
        [InlineData("zip", ReportFormat.Zip, "application/zip", "karetao-package.zip")]
        [InlineData("ZIP", ReportFormat.Zip, "application/zip", "karetao-package.zip")]
        public async Task ExportPackageGeneratesReport(string? format, ReportFormat expected, string contentType, string fileName)
        {
            // Arrange
            var engine = new FakeEngine();
            var generator = new Mock<Generators.RobotTypePackage>();
            var result = Tuple.Create((Stream)new MemoryStream(), fileName);
            generator.Setup(g => g.GenerateAsync(expected))
                .Returns(Task.FromResult(result))
                .Verifiable();
            generator.Setup(g => g.IsFormatAvailable(expected))
                .Returns(true)
                .Verifiable();
            engine.RegisterGenerator(generator.Object);
            var query = new Mock<RobotTypeData>();
            query.Setup(q => q.RetrieveByNameAsync("karetao"))
                .Returns(Task.FromResult((Data.RobotType?)new Data.RobotType { Name = "karetao" }));
            engine.RegisterQuery(query.Object);
            var controller = InitialiseController(engine);

            // Act
            var response = await controller.ExportPackage("karetao", format);

            // Assert
            var streamResult = Assert.IsType<FileStreamResult>(response);
            generator.Verify();
            Assert.Equal(contentType, streamResult.ContentType);
            Assert.Equal(fileName, streamResult.FileDownloadName);
        }

        [Fact]
        public async Task ExportPackageHandlesInvalidInput()
        {
            // Arrange
            var engine = new FakeEngine();
            var query = new Mock<RobotTypeData>();
            query.Setup(q => q.RetrieveByNameAsync("karetao"))
                .Returns(Task.FromResult((Data.RobotType?)new Data.RobotType { Name = "karetao" }));
            engine.RegisterQuery(query.Object);
            var controller = InitialiseController(engine);

            // Act
            var response = await controller.ExportPackage("karetao", "garbage");

            // Assert
            Assert.IsType<BadRequestObjectResult>(response);
        }

        [Fact]
        public async Task ExportPackageHandlesUnknownFormat()
        {
            // Arrange
            var engine = new FakeEngine();
            var generator = new Mock<Generators.RobotTypePackage>();
            engine.RegisterGenerator(generator.Object);
            var query = new Mock<RobotTypeData>();
            query.Setup(q => q.RetrieveByNameAsync("karetao"))
                .Returns(Task.FromResult((Data.RobotType?)new Data.RobotType { Name = "karetao" }));
            engine.RegisterQuery(query.Object);
            var controller = InitialiseController(engine);

            // Act
            var response = await controller.ExportPackage("karetao", "unknown");

            // Assert
            Assert.IsType<BadRequestObjectResult>(response);
        }

        [Theory]
        [InlineData(null, ReportFormat.Xml, "application/xml", "Testing-toolbox.xml")]
        [InlineData("xml", ReportFormat.Xml, "application/xml", "Testing-toolbox.xml")]
        [InlineData("Xml", ReportFormat.Xml, "application/xml", "Testing-toolbox.xml")]
        [InlineData("XML", ReportFormat.Xml, "application/xml", "Testing-toolbox.xml")]
        public async Task ExportToolboxGeneratesReport(string? format, ReportFormat expected, string contentType, string fileName)
        {
            // Arrange
            var engine = new FakeEngine();
            var generator = new Mock<Generators.RobotTypeToolbox>();
            var result = Tuple.Create((Stream)new MemoryStream(), fileName);
            generator.Setup(g => g.GenerateAsync(expected))
                .Returns(Task.FromResult(result))
                .Verifiable();
            generator.Setup(g => g.IsFormatAvailable(expected))
                .Returns(true)
                .Verifiable();
            engine.RegisterGenerator(generator.Object);
            var query = new Mock<RobotTypeData>();
            query.Setup(q => q.RetrieveByNameAsync("karetao"))
                .Returns(Task.FromResult((Data.RobotType?)new Data.RobotType { Name = "karetao" }));
            engine.RegisterQuery(query.Object);
            var controller = InitialiseController(engine);

            // Act
            var response = await controller.ExportToolbox("karetao", "Testing", format);

            // Assert
            var streamResult = Assert.IsType<FileStreamResult>(response.Result);
            generator.Verify();
            Assert.Equal(contentType, streamResult.ContentType);
            Assert.Equal(fileName, streamResult.FileDownloadName);
        }

        [Fact]
        public async Task ExportToolboxHandlesInvalidInput()
        {
            // Arrange
            var engine = new FakeEngine();
            var generator = new Mock<Generators.RobotTypeToolbox>();
            engine.RegisterGenerator(generator.Object);
            var query = new Mock<RobotTypeData>();
            query.Setup(q => q.RetrieveByNameAsync("karetao"))
                .Returns(Task.FromResult((Data.RobotType?)new Data.RobotType { Name = "karetao" }));
            engine.RegisterQuery(query.Object);
            var controller = InitialiseController(engine);

            // Act
            var response = await controller.ExportToolbox("karetao", "garbage");

            // Assert
            Assert.IsType<BadRequestObjectResult>(response.Result);
        }

        [Fact]
        public async Task ExportToolboxHandlesUnknownFormat()
        {
            // Arrange
            var engine = new FakeEngine();
            var generator = new Mock<Generators.RobotTypeToolbox>();
            engine.RegisterGenerator(generator.Object);
            var query = new Mock<RobotTypeData>();
            query.Setup(q => q.RetrieveByNameAsync("karetao"))
                .Returns(Task.FromResult((Data.RobotType?)new Data.RobotType { Name = "karetao" }));
            engine.RegisterQuery(query.Object);
            var controller = InitialiseController(engine);

            // Act
            var response = await controller.ExportToolbox("karetao", "unknown");

            // Assert
            Assert.IsType<BadRequestObjectResult>(response.Result);
        }

        [Fact]
        public async Task GetHandlesMissing()
        {
            // Arrange
            var engine = new FakeEngine();
            var controller = InitialiseController(engine);
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
            var engine = new FakeEngine();
            var controller = InitialiseController(engine);
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
        public async Task GetToolboxHandlesMissingRobotType()
        {
            // Arrange
            var engine = new FakeEngine();
            var controller = InitialiseController(engine);
            var query = new Mock<RobotTypeData>();
            query.Setup(q => q.RetrieveByNameAsync("karetao"))
                .Returns(Task.FromResult((Data.RobotType?)null));
            engine.RegisterQuery(query.Object);

            // Act
            var response = await controller.GetToolbox("karetao", "unknown");

            // Assert
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task GetToolboxHandlesMissingToolbox()
        {
            // Arrange
            var engine = new FakeEngine();
            var controller = InitialiseController(engine);
            var query = new Mock<RobotTypeData>();
            query.Setup(q => q.RetrieveByNameAsync("karetao"))
                .Returns(Task.FromResult((Data.RobotType?)new Data.RobotType { Name = "karetao" }));
            engine.RegisterQuery(query.Object);

            // Act
            var response = await controller.GetToolbox("karetao", "unknown");

            // Assert
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task GetToolboxRetrievesViaQuery()
        {
            // Arrange
            var engine = new FakeEngine();
            var controller = InitialiseController(engine);
            var query = new Mock<RobotTypeData>();
            var robotType = new Data.RobotType { Name = "karetao" };
            robotType.Toolboxes.Add(new Data.Toolbox { Name = "toolbox" });
            query.Setup(q => q.RetrieveByNameAsync("karetao"))
                .Returns(Task.FromResult((Data.RobotType?)robotType));
            engine.RegisterQuery(query.Object);

            // Act
            var response = await controller.GetToolbox("karetao", "toolbox");

            // Assert
            Assert.NotNull(response.Value);
            Assert.Equal("toolbox", response.Value?.Name);
            Assert.Equal(
                "<toolbox />",
                response.Value?.Definition);
        }

        [Fact]
        public async Task ImportToolboxCallsCommand()
        {
            // Arrange
            var engine = new FakeEngine();
            engine.ExpectCommand<ImportToolbox>(
                CommandResult.New(1, new Data.RobotType()));
            var controller = InitialiseController(engine);
            controller.SetRequestBody(ToolBoxXml);

            // Act
            var response = await controller.ImportToolbox("karetao", "testing", null);

            // Assert
            var result = Assert.IsType<ExecutionResult<Transfer.RobotType>>(response.Value);
            Assert.True(result.Successful, "Expected result to be successful");
            engine.Verify();

            var command = Assert.IsType<ImportToolbox>(engine.LastCommand);
            Assert.Equal("karetao", command.RobotTypeName);
            Assert.Equal("testing", command.ToolboxName);
            Assert.Equal(ToolBoxXml, command.Definition);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("no", false)]
        [InlineData("NO", false)]
        [InlineData("unknown", false)]
        [InlineData("yes", true)]
        [InlineData("Yes", true)]
        [InlineData("YES", true)]
        [InlineData("false", false)]
        [InlineData("True", true)]
        public async Task ImportToolboxSetDefaultFlagCorrectly(string? flag, bool expected)
        {
            // Arrange
            var engine = new FakeEngine();
            engine.ExpectCommand<ImportToolbox>(
                CommandResult.New(1, new Data.RobotType()));
            var controller = InitialiseController(engine);
            controller.SetRequestBody(ToolBoxXml);

            // Act
            var response = await controller.ImportToolbox("karetao", "testing", flag);

            // Assert
            var result = Assert.IsType<ExecutionResult<Transfer.RobotType>>(response.Value);
            Assert.True(result.Successful, "Expected result to be successful");
            engine.Verify();

            var command = Assert.IsType<ImportToolbox>(engine.LastCommand);
            Assert.Equal(expected, command.IsDefault);
        }

        [Fact]
        public async Task ImportToolboxValidatesIncomingData()
        {
            // Arrange
            var controller = InitialiseController();
            controller.SetRequestBody(null);

            // Act
            var response = await controller.ImportToolbox("karetao", "testing", null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(response.Result);
        }

        [Fact]
        public async Task ListHandlesNullData()
        {
            // Arrange
            var engine = new FakeEngine();
            var query = new Mock<RobotTypeData>();
            var result = new ListResult<Data.RobotType>();
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.RetrievePageAsync(0, 25))
                .Returns(Task.FromResult(result));
            var controller = InitialiseController(engine);

            // Act
            var response = await controller.List(null, null);

            // Assert
            Assert.Equal(0, response.Count);
            Assert.Equal(0, response.Page);
            Assert.Null(response.Items);
        }

        [Fact]
        public async Task ListRetrievesViaQuery()
        {
            // Arrange
            var engine = new FakeEngine();
            var query = new Mock<RobotTypeData>();
            var result = ListResult.New(
                new[] { new Data.RobotType() });
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.RetrievePageAsync(0, 25))
                .Returns(Task.FromResult(result));
            var controller = InitialiseController(engine);

            // Act
            var response = await controller.List(null, null);

            // Assert
            Assert.Equal(1, response.Count);
            Assert.Equal(0, response.Page);
            Assert.NotEmpty(response.Items);
        }

        [Fact]
        public async Task PostCallsAddsRobot()
        {
            // Arrange
            var engine = new FakeEngine();
            engine.ExpectCommand<AddRobotType>(
                CommandResult.New(1, new Data.RobotType()));
            var controller = InitialiseController(engine);

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
            var controller = InitialiseController();

            // Act
            var response = await controller.Post(null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(response.Result);
        }

        [Fact]
        public async Task PutCallsCommand()
        {
            // Arrange
            var engine = new FakeEngine();
            engine.ExpectCommand<UpdateRobotType>(
                CommandResult.New(1, new Data.RobotType()));
            var controller = InitialiseController(engine);

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
            var controller = InitialiseController();

            // Act
            var response = await controller.Put("karetao", null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(response.Result);
        }

        [Fact]
        public async Task SetAsDefaultCallsCommand()
        {
            // Arrange
            var engine = new FakeEngine();
            engine.ExpectCommand<SetDefaultRobotType>(
                CommandResult.New(1, new Data.RobotType()));
            var controller = InitialiseController(engine);

            // Act
            var response = await controller.SetAsDefault("karetao");

            // Assert
            var result = Assert.IsType<ExecutionResult<Transfer.RobotType>>(response.Value);
            Assert.True(result.Successful, "Expected result to be successful");
            engine.Verify();

            var command = Assert.IsType<SetDefaultRobotType>(engine.LastCommand);
            Assert.Equal("karetao", command.Name);
        }

        private static RobotTypesController InitialiseController(
            FakeEngine? engine = null,
            FakeLogger<RobotTypesController>? logger = null)
        {
            logger ??= new FakeLogger<RobotTypesController>();
            engine ??= new FakeEngine();
            var env = InitialiseEnvironment();
            var controller = new RobotTypesController(
                logger,
                engine,
                env.Object);
            return controller;
        }

        private static Mock<IWebHostEnvironment> InitialiseEnvironment(string value = "http://test")
        {
            var env = new Mock<IWebHostEnvironment>();
            env.Setup(e => e.WebRootPath).Returns(value);
            return env;
        }
    }
}