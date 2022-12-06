using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Queries;
using NaoBlocks.Web.Controllers;
using NaoBlocks.Web.Helpers;
using NaoBlocks.Web.Tests.Helpers;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Data = NaoBlocks.Engine.Data;
using Generators = NaoBlocks.Engine.Generators;
using Transfer = NaoBlocks.Web.Dtos;

namespace NaoBlocks.Web.Tests.Controllers
{
    public class RobotTypesControllerTests
    {
        public const string ToolBoxXml = "<toolbox/>";

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
        [InlineData(null, ReportFormat.Json, "application/json", "karetao-definition.json")]
        [InlineData("json", ReportFormat.Json, "application/json", "karetao-definition.json")]
        [InlineData("JSON", ReportFormat.Json, "application/json", "karetao-definition.json")]
        [InlineData(".json", ReportFormat.Json, "application/json", "karetao-definition.json")]
        public async Task ExportDefinitionGeneratesReport(string? format, ReportFormat expected, string contentType, string fileName)
        {
            // Arrange
            var engine = new FakeEngine();
            var generator = new Mock<Generators.RobotTypeDefinition>();
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
            var response = await controller.ExportDefinition("karetao", format);

            // Assert
            var streamResult = Assert.IsType<FileStreamResult>(response);
            generator.Verify();
            Assert.Equal(contentType, streamResult.ContentType);
            Assert.Equal(fileName, streamResult.FileDownloadName);
        }

        [Fact]
        public async Task ExportDefinitionHandlesInvalidInput()
        {
            // Arrange
            var engine = new FakeEngine();
            var query = new Mock<RobotTypeData>();
            query.Setup(q => q.RetrieveByNameAsync("karetao"))
                .Returns(Task.FromResult((Data.RobotType?)new Data.RobotType { Name = "karetao" }));
            engine.RegisterQuery(query.Object);
            var controller = InitialiseController(engine);

            // Act
            var response = await controller.ExportDefinition("karetao", "garbage");

            // Assert
            Assert.IsType<BadRequestObjectResult>(response);
        }

        [Fact]
        public async Task ExportDefinitionHandlesUnknownFormat()
        {
            // Arrange
            var engine = new FakeEngine();
            var generator = new Mock<Generators.RobotTypeDefinition>();
            engine.RegisterGenerator(generator.Object);
            var query = new Mock<RobotTypeData>();
            query.Setup(q => q.RetrieveByNameAsync("karetao"))
                .Returns(Task.FromResult((Data.RobotType?)new Data.RobotType { Name = "karetao" }));
            engine.RegisterQuery(query.Object);
            var controller = InitialiseController(engine);

            // Act
            var response = await controller.ExportDefinition("karetao", "unknown");

            // Assert
            Assert.IsType<BadRequestObjectResult>(response);
        }

        [Theory]
        [InlineData(null, ReportFormat.Excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "karetao-logs.xlsx")]
        [InlineData("csv", ReportFormat.Csv, "text/csv", "karetao-logs.csv")]
        [InlineData("CSV", ReportFormat.Csv, "text/csv", "karetao-logs.csv")]
        [InlineData(".csv", ReportFormat.Csv, "text/csv", "karetao-logs.csv")]
        public async Task ExportDetailsGeneratesReport(string? format, ReportFormat expected, string contentType, string fileName)
        {
            // Arrange
            var engine = new FakeEngine();
            var generator = new Mock<Generators.RobotTypeExport>();
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
            var engine = new FakeEngine();
            var query = new Mock<RobotTypeData>();
            query.Setup(q => q.RetrieveByNameAsync("karetao"))
                .Returns(Task.FromResult((Data.RobotType?)new Data.RobotType { Name = "karetao" }));
            engine.RegisterQuery(query.Object);
            var controller = InitialiseController(engine);

            // Act
            var response = await controller.ExportDetails("karetao", "garbage");

            // Assert
            Assert.IsType<BadRequestObjectResult>(response);
        }

        [Fact]
        public async Task ExportDetailsHandlesUnknownFormat()
        {
            // Arrange
            var engine = new FakeEngine();
            var generator = new Mock<Generators.RobotTypeExport>();
            engine.RegisterGenerator(generator.Object);
            var query = new Mock<RobotTypeData>();
            query.Setup(q => q.RetrieveByNameAsync("karetao"))
                .Returns(Task.FromResult((Data.RobotType?)new Data.RobotType { Name = "karetao" }));
            engine.RegisterQuery(query.Object);
            var controller = InitialiseController(engine);

            // Act
            var response = await controller.ExportDetails("karetao", "unknown");

            // Assert
            Assert.IsType<BadRequestObjectResult>(response);
        }

        [Theory]
        [InlineData(null, ReportFormat.Excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "types.xlsx")]
        [InlineData("csv", ReportFormat.Csv, "text/csv", "types.csv")]
        [InlineData("CSV", ReportFormat.Csv, "text/csv", "types.csv")]
        [InlineData(".csv", ReportFormat.Csv, "text/csv", "types.csv")]
        public async Task ExportGeneratesReport(string? format, ReportFormat expected, string contentType, string fileName)
        {
            // Arrange
            var engine = new FakeEngine();
            var generator = new Mock<Generators.RobotTypesList>();
            var result = Tuple.Create((Stream)new MemoryStream(), fileName);
            generator.Setup(g => g.GenerateAsync(expected))
                .Returns(Task.FromResult(result))
                .Verifiable();
            generator.Setup(g => g.IsFormatAvailable(expected))
                .Returns(true)
                .Verifiable();
            engine.RegisterGenerator(generator.Object);
            var controller = InitialiseController(engine);

            // Act
            var response = await controller.Export(format);

            // Assert
            var streamResult = Assert.IsType<FileStreamResult>(response);
            generator.Verify();
            Assert.Equal(contentType, streamResult.ContentType);
            Assert.Equal(fileName, streamResult.FileDownloadName);
        }

        [Fact]
        public async Task ExportHandlesInvalidInput()
        {
            // Arrange
            var engine = new FakeEngine();
            var controller = InitialiseController(engine);

            // Act
            var response = await controller.Export("garbage");

            // Assert
            Assert.IsType<BadRequestObjectResult>(response);
        }

        [Fact]
        public async Task ExportHandlesUnknownFormat()
        {
            // Arrange
            var engine = new FakeEngine();
            var generator = new Mock<Generators.RobotTypesList>();
            engine.RegisterGenerator(generator.Object);
            var controller = InitialiseController(engine);

            // Act
            var response = await controller.Export("unknown");

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
            var query = new Mock<RobotTypeData>();
            query.Setup(q => q.RetrieveByNameAsync("karetao"))
                .Returns(Task.FromResult((Data.RobotType?)new Data.RobotType { Name = "karetao" }));
            engine.RegisterQuery(query.Object);
            var controller = InitialiseController(engine);

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
            var engine = new FakeEngine();
            var query = new Mock<RobotTypeData>();
            query.Setup(q => q.RetrieveByNameAsync("karetao"))
                .Returns(Task.FromResult((Data.RobotType?)new Data.RobotType { Name = "karetao" }));
            engine.RegisterQuery(query.Object);
            var controller = InitialiseController(engine);

            // Act
            var response = await controller.ExportLogs("karetao", "garbage");

            // Assert
            Assert.IsType<BadRequestObjectResult>(response);
        }

        [Fact]
        public async Task ExportLogsHandlesUnknownFormat()
        {
            // Arrange
            var engine = new FakeEngine();
            var generator = new Mock<Generators.RobotLogs>();
            engine.RegisterGenerator(generator.Object);
            var query = new Mock<RobotTypeData>();
            query.Setup(q => q.RetrieveByNameAsync("karetao"))
                .Returns(Task.FromResult((Data.RobotType?)new Data.RobotType { Name = "karetao" }));
            engine.RegisterQuery(query.Object);
            var controller = InitialiseController(engine);

            // Act
            var response = await controller.ExportLogs("karetao", "unknown");

            // Assert
            Assert.IsType<BadRequestObjectResult>(response);
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
        public async Task GeneratePackageFileListHandlesTxtFormat()
        {
            // Arrange
            var engine = new FakeEngine();
            var basePath = Path.Combine(Path.GetTempPath(), "NaoBlocks-6");
            var controller = InitialiseController(engine, rootPath: basePath);
            var query = new Mock<RobotTypeData>();
            query.Setup(q => q.RetrieveByNameAsync("karetao"))
                .Returns(Task.FromResult((Data.RobotType?)new Data.RobotType { Name = "karetao" }));
            engine.RegisterQuery(query.Object);
            try
            {
                var packagePath = Path.Combine(basePath, "packages", "karetao");
                Directory.CreateDirectory(packagePath);
                await File.WriteAllTextAsync(Path.Combine(packagePath, "fake.txt"),
                    RobotTypeFilePackageTests.FakeFileContents);

                // Act
                var response = await controller.GeneratePackageFileList("karetao");

                // Assert
                var fileResult = Assert.IsType<FileContentResult>(response);
                var data = Encoding.UTF8.GetString(fileResult.FileContents);
                Assert.Equal(RobotTypeFilePackageTests.FileListContents, data.Trim());
            }
            finally
            {
                Directory.Delete(basePath, true);
            }
        }

        [Fact]
        public async Task GeneratePackageFileListHandlesUnknownRobot()
        {
            // Arrange
            var engine = new FakeEngine();
            var controller = InitialiseController(engine);
            var query = new Mock<RobotTypeData>();
            query.Setup(q => q.RetrieveByNameAsync("karetao"))
                .Returns(Task.FromResult((Data.RobotType?)null));
            engine.RegisterQuery(query.Object);

            // Act
            var response = await controller.GeneratePackageFileList("karetao");

            // Assert
            Assert.IsType<NotFoundResult>(response);
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

        [Theory]
        [InlineData("123", HttpStatusCode.OK)]
        [InlineData("ZwI4sdXTSrE1AVW7bVvWKn0IPX2CQuHWs1nqZINj03I=", HttpStatusCode.NotModified)]
        [InlineData("\"ZwI4sdXTSrE1AVW7bVvWKn0IPX2CQuHWs1nqZINj03I=\"", HttpStatusCode.NotModified)]
        public async Task GetPackageFileChecksETag(string etag, HttpStatusCode expected)
        {
            // Arrange
            var engine = new FakeEngine();
            var basePath = Path.Combine(Path.GetTempPath(), $"NaoBlocks-22-{expected}");
            var controller = InitialiseController(engine, rootPath: basePath);
            controller.SetRequestHeader("ETag", etag);
            var query = new Mock<RobotTypeData>();
            query.Setup(q => q.RetrieveByNameAsync("karetao"))
                .Returns(Task.FromResult((Data.RobotType?)new Data.RobotType { Name = "karetao" }));
            engine.RegisterQuery(query.Object);

            try
            {
                var fullPath = Path.Combine(basePath, "packages", "karetao");
                Directory.CreateDirectory(fullPath);
                await File.WriteAllTextAsync(
                    Path.Combine(fullPath, "missing.txt"),
                    RobotTypeFilePackageTests.FakeFileContents);

                // Act
                var response = await controller.GetPackageFile("karetao", "missing.txt");

                // Assert
                if (expected == HttpStatusCode.OK)
                {
                    var result = Assert.IsType<FileStreamResult>(response);
                    result.FileStream.Close();
                }
                else
                {
                    var result = Assert.IsType<StatusCodeResult>(response);
                    Assert.Equal((int)expected, result.StatusCode);
                }
            }
            finally
            {
                if (Directory.Exists(basePath)) Directory.Delete(basePath, true);
            }
        }

        [Fact]
        public async Task GetPackageFileHandlesUnknownRobot()
        {
            // Arrange
            var engine = new FakeEngine();
            var controller = InitialiseController(engine);
            var query = new Mock<RobotTypeData>();
            query.Setup(q => q.RetrieveByNameAsync("karetao"))
                .Returns(Task.FromResult((Data.RobotType?)null));
            engine.RegisterQuery(query.Object);

            // Act
            var response = await controller.GetPackageFile("karetao", "missing.txt");

            // Assert
            Assert.IsType<NotFoundResult>(response);
        }

        [Fact]
        public async Task GetPackageFileListHandlesJsonFormat()
        {
            // Arrange
            var engine = new FakeEngine();
            var basePath = Path.Combine(Path.GetTempPath(), "NaoBlocks-5");
            var controller = InitialiseController(engine, rootPath: basePath);
            var query = new Mock<RobotTypeData>();
            query.Setup(q => q.RetrieveByNameAsync("karetao"))
                .Returns(Task.FromResult((Data.RobotType?)new Data.RobotType { Name = "karetao" }));
            engine.RegisterQuery(query.Object);
            try
            {
                var packagePath = Path.Combine(basePath, "packages", "karetao");
                Directory.CreateDirectory(packagePath);
                await File.WriteAllTextAsync(Path.Combine(packagePath, RobotTypeFilePackage.FileListName),
                    RobotTypeFilePackageTests.FileListContents);

                // Act
                var response = await controller.GetPackageFileList("karetao", ".json");

                // Assert
                var jsonResult = Assert.IsType<JsonResult>(response);
                var json = JsonConvert.SerializeObject(jsonResult.Value);
                Assert.Equal(
                    "{\"Count\":1,\"Items\":[{\"Hash\":\"ZwI4sdXTSrE1AVW7bVvWKn0IPX2CQuHWs1nqZINj03I=\",\"Name\":\"fake.txt\"}],\"Page\":0}",
                    json);
            }
            finally
            {
                Directory.Delete(basePath, true);
            }
        }

        [Fact]
        public async Task GetPackageFileListHandlesTxtFormat()
        {
            // Arrange
            var engine = new FakeEngine();
            var basePath = Path.Combine(Path.GetTempPath(), "NaoBlocks-4");
            var controller = InitialiseController(engine, rootPath: basePath);
            var query = new Mock<RobotTypeData>();
            query.Setup(q => q.RetrieveByNameAsync("karetao"))
                .Returns(Task.FromResult((Data.RobotType?)new Data.RobotType { Name = "karetao" }));
            engine.RegisterQuery(query.Object);
            try
            {
                var packagePath = Path.Combine(basePath, "packages", "karetao");
                Directory.CreateDirectory(packagePath);
                await File.WriteAllTextAsync(Path.Combine(packagePath, RobotTypeFilePackage.FileListName),
                    RobotTypeFilePackageTests.FileListContents);

                // Act
                var response = await controller.GetPackageFileList("karetao", ".txt");

                // Assert
                var fileResult = Assert.IsType<FileContentResult>(response);
                var data = Encoding.UTF8.GetString(fileResult.FileContents);
                Assert.Equal(RobotTypeFilePackageTests.FileListContents, data);
            }
            finally
            {
                Directory.Delete(basePath, true);
            }
        }

        [Fact]
        public async Task GetPackageFileListHandlesUnknownRobot()
        {
            // Arrange
            var engine = new FakeEngine();
            var controller = InitialiseController(engine);
            var query = new Mock<RobotTypeData>();
            query.Setup(q => q.RetrieveByNameAsync("karetao"))
                .Returns(Task.FromResult((Data.RobotType?)null));
            engine.RegisterQuery(query.Object);

            // Act
            var response = await controller.GetPackageFileList("karetao");

            // Assert
            Assert.IsType<NotFoundResult>(response);
        }

        [Fact]
        public async Task GetPackageFileRetrievesFile()
        {
            // Arrange
            var engine = new FakeEngine();
            var basePath = Path.Combine(Path.GetTempPath(), "NaoBlocks-21");
            var controller = InitialiseController(engine, rootPath: basePath);
            controller.SetRequestHeader("missing", "not here");
            var query = new Mock<RobotTypeData>();
            query.Setup(q => q.RetrieveByNameAsync("karetao"))
                .Returns(Task.FromResult((Data.RobotType?)new Data.RobotType { Name = "karetao" }));
            engine.RegisterQuery(query.Object);

            try
            {
                var fullPath = Path.Combine(basePath, "packages", "karetao");
                Directory.CreateDirectory(fullPath);
                await File.WriteAllTextAsync(
                    Path.Combine(fullPath, "missing.txt"),
                    RobotTypeFilePackageTests.FakeFileContents);

                // Act
                var response = await controller.GetPackageFile("karetao", "missing.txt");

                // Assert
                var result = Assert.IsType<FileStreamResult>(response);
                using var reader = new StreamReader(result.FileStream);
                var data = reader.ReadToEnd();
                Assert.Equal(RobotTypeFilePackageTests.FakeFileContents, data);
            }
            finally
            {
                if (Directory.Exists(basePath)) Directory.Delete(basePath, true);
            }
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
            var response = await controller.ImportToolbox("karetao", "testing", null, "no");

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
            var response = await controller.ImportToolbox("karetao", "testing", flag, "no");

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
            var response = await controller.ImportToolbox("karetao", "testing", null, "no");

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
            Assert.NotEmpty(response.Items ?? Array.Empty<Transfer.RobotType>());
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
        public async Task PostValuesCallsCorrectCommand()
        {
            // Arrange
            var robotType = new Data.RobotType();
            var engine = new FakeEngine();
            var robotQuery = new Mock<RobotTypeData>();
            robotQuery.Setup(q => q.RetrieveByNameAsync("karetao"))
                .Returns(Task.FromResult<Data.RobotType?>(robotType));
            engine.RegisterQuery(robotQuery.Object);
            engine.ExpectCommand<UpdateCustomValuesForRobotType>();
            var controller = InitialiseController(engine);

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
            var engine = new FakeEngine();
            var robotQuery = new Mock<RobotTypeData>();
            engine.RegisterQuery(robotQuery.Object);
            engine.ExpectCommand<UpdateCustomValuesForRobot>();
            var controller = InitialiseController(engine);

            // Act
            var values = Transfer.Set.New(
                Data.NamedValue.New("one", "tahi"));
            var response = await controller.PostValues("karetao", values);

            // Assert
            Assert.IsType<NotFoundResult>(response.Result);
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

        [Fact]
        public async Task UploadPackageFileHandlesUnknownRobot()
        {
            // Arrange
            var engine = new FakeEngine();
            var controller = InitialiseController(engine);
            var query = new Mock<RobotTypeData>();
            query.Setup(q => q.RetrieveByNameAsync("karetao"))
                .Returns(Task.FromResult((Data.RobotType?)null));
            engine.RegisterQuery(query.Object);

            // Act
            var response = await controller.UploadPackageFile("karetao", "missing.txt");

            // Assert
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task UploadPackageFileSavesFile()
        {
            // Arrange
            var engine = new FakeEngine();
            var basePath = Path.Combine(Path.GetTempPath(), "NaoBlocks-20");
            var controller = InitialiseController(engine, rootPath: basePath);
            controller.SetRequestBody(RobotTypeFilePackageTests.FakeFileContents);
            var query = new Mock<RobotTypeData>();
            query.Setup(q => q.RetrieveByNameAsync("karetao"))
                .Returns(Task.FromResult((Data.RobotType?)new Data.RobotType { Name = "karetao" }));
            engine.RegisterQuery(query.Object);

            try
            {
                // Act
                var response = await controller.UploadPackageFile("karetao", "missing.txt");

                // Assert
                Assert.IsType<ExecutionResult>(response.Value);
                Assert.True(File.Exists(Path.Combine(basePath, "packages", "karetao", "missing.txt")));
            }
            finally
            {
                if (Directory.Exists(basePath)) Directory.Delete(basePath, true);
            }
        }

        private static RobotTypesController InitialiseController(
            FakeEngine? engine = null,
            FakeLogger<RobotTypesController>? logger = null,
            string? rootPath = null)
        {
            logger ??= new FakeLogger<RobotTypesController>();
            engine ??= new FakeEngine();
            var env = InitialiseEnvironment(rootPath);
            var controller = new RobotTypesController(
                logger,
                engine,
                env.Object);
            return controller;
        }

        private static Mock<IWebHostEnvironment> InitialiseEnvironment(string? value = null)
        {
            var path = value ?? Path.GetTempPath();
            var env = new Mock<IWebHostEnvironment>();
            env.Setup(e => e.WebRootPath).Returns(path);
            return env;
        }
    }
}