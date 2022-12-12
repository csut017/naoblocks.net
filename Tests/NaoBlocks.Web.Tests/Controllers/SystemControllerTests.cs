using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Queries;
using NaoBlocks.Web.Communications;
using NaoBlocks.Web.Controllers;
using NaoBlocks.Web.Dtos;
using NaoBlocks.Web.Helpers;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

using Data = NaoBlocks.Engine.Data;

using Generators = NaoBlocks.Engine.Generators;
using ResourceManager = NaoBlocks.Web.Resources.Manager;

namespace NaoBlocks.Web.Tests.Controllers
{
    [Collection("ClientAddressList tests")]
    public class SystemControllerTests
    {
        [Theory]
        [InlineData("https://127.0.0.1,,yes")]
        [InlineData("http://test,,no\nhttps://test,,yes", "http://test", "https://test")]
        public async Task ClientAddressesFileRetrievesTextFile(string expected, params string[] addressList)
        {
            // Arrange
            var logger = new FakeLogger<SystemController>();
            var hub = new Mock<IHub>();
            var engine = new FakeEngine();
            var manager = new Mock<UiManager>();
            var server = InitialiseServerWithAddresses(addressList);
            var controller = new SystemController(
                logger,
                engine,
                hub.Object,
                manager.Object,
                server.Object);

            // Act
            var response = await controller.ClientAddressesFile();

            // Assert
            var result = Assert.IsType<FileContentResult>(response);
            Assert.Equal("connect.txt", result.FileDownloadName);
            Assert.Equal("text/plain", result.ContentType);
            Assert.Equal(
                expected,
                Encoding.UTF8.GetString(result.FileContents));
        }

        [Theory]
        [InlineData("https://127.0.0.1")]
        [InlineData("http://test\nhttps://test", "http://test", "https://test")]
        public async Task ClientAddressesRetrievesAddresses(string expected, params string[] addressList)
        {
            // Arrange
            var logger = new FakeLogger<SystemController>();
            var hub = new Mock<IHub>();
            var engine = new FakeEngine();
            var manager = new Mock<UiManager>();
            var server = InitialiseServerWithAddresses(addressList);
            var controller = new SystemController(
                logger,
                engine,
                hub.Object,
                manager.Object,
                server.Object);

            // Act
            var response = await controller.ClientAddresses();

            // Assert
            var result = Assert.IsType<ListResult<string>>(response.Value);
            string[] expectedItems = expected.Split('\n');
            Assert.Equal(expectedItems.Length, result.Count);
            Assert.Equal(expectedItems, result.Items ?? Array.Empty<string>());
        }

        [Theory]
        [InlineData(null, ReportFormat.Csv, "text/csv", "all-logs.csv")]
        [InlineData("csv", ReportFormat.Csv, "text/csv", "all-logs.csv")]
        [InlineData("CSV", ReportFormat.Csv, "text/csv", "all-logs.csv")]
        [InlineData(".csv", ReportFormat.Csv, "text/csv", "all-logs.csv")]
        public async Task ExportAllConfigurationGeneratesReport(string? format, ReportFormat expected, string contentType, string fileName)
        {
            // Arrange
            var loggerMock = new FakeLogger<SystemController>();
            var hub = new Mock<IHub>();
            var engine = new FakeEngine();
            var manager = new Mock<UiManager>();
            var controller = new SystemController(
                loggerMock,
                engine,
                hub.Object,
                manager.Object,
                new Mock<IServer>().Object);

            var generator = new Mock<Generators.AllLists>();
            var result = Tuple.Create((Stream)new MemoryStream(), fileName);
            generator.Setup(g => g.GenerateAsync(expected))
                .Returns(Task.FromResult(result))
                .Verifiable();
            generator.Setup(g => g.IsFormatAvailable(expected))
                .Returns(true)
                .Verifiable();
            engine.RegisterGenerator(generator.Object);

            // Act
            var response = await controller.ExportAllConfiguration(format);

            // Assert
            var streamResult = Assert.IsType<FileStreamResult>(response);
            generator.Verify();
            Assert.Equal(contentType, streamResult.ContentType);
            Assert.Equal(fileName, streamResult.FileDownloadName);
        }

        [Fact]
        public async Task ExportAllConfigurationHandlesInvalidInput()
        {
            // Arrange
            var loggerMock = new FakeLogger<SystemController>();
            var hub = new Mock<IHub>();
            var engine = new FakeEngine();
            var manager = new Mock<UiManager>();
            var controller = new SystemController(
                loggerMock,
                engine,
                hub.Object,
                manager.Object,
                new Mock<IServer>().Object);

            // Act
            var response = await controller.ExportAllConfiguration("garbage");

            // Assert
            Assert.IsType<BadRequestObjectResult>(response);
        }

        [Fact]
        public async Task ExportAllConfigurationHandlesUnknownFormat()
        {
            // Arrange
            var loggerMock = new FakeLogger<SystemController>();
            var hub = new Mock<IHub>();
            var engine = new FakeEngine();
            var manager = new Mock<UiManager>();
            var controller = new SystemController(
                loggerMock,
                engine,
                hub.Object,
                manager.Object,
                new Mock<IServer>().Object);

            // Act
            var response = await controller.ExportAllConfiguration("unknown");

            // Assert
            Assert.IsType<BadRequestObjectResult>(response);
        }

        [Fact]
        public async Task ExportAllConfigurationSetsAdministratorFlag()
        {
            // Arrange
            var loggerMock = new FakeLogger<SystemController>();
            var hub = new Mock<IHub>();
            var engine = new FakeEngine();
            var manager = new Mock<UiManager>();
            var controller = new SystemController(
                loggerMock,
                engine,
                hub.Object,
                manager.Object,
                new Mock<IServer>().Object);
            engine.ConfigureUser(controller, "Mia", Data.UserRole.Administrator);

            var generator = new Mock<Generators.AllLists>();
            var result = Tuple.Create((Stream)new MemoryStream(), "all-logs.csv");
            generator.Setup(g => g.GenerateAsync(ReportFormat.Csv))
                .Returns(Task.FromResult(result))
                .Verifiable();
            generator.Setup(g => g.IsFormatAvailable(ReportFormat.Csv))
                .Returns(true)
                .Verifiable();
            engine.RegisterGenerator(generator.Object);

            // Act
            var response = await controller.ExportAllConfiguration(".csv");

            // Assert
            var streamResult = Assert.IsType<FileStreamResult>(response);
            generator.Verify();
            Assert.Equal("yes", generator.Object.GetArgumentOrDefault("types", "not set!"));
        }

        [Theory]
        [InlineData(null, ReportFormat.Csv, "text/csv", "all-logs.csv")]
        [InlineData("csv", ReportFormat.Csv, "text/csv", "all-logs.csv")]
        [InlineData("CSV", ReportFormat.Csv, "text/csv", "all-logs.csv")]
        [InlineData(".csv", ReportFormat.Csv, "text/csv", "all-logs.csv")]
        public async Task ExportCodeProgramsGeneratesReport(string? format, ReportFormat expected, string contentType, string fileName)
        {
            // Arrange
            var loggerMock = new FakeLogger<SystemController>();
            var hub = new Mock<IHub>();
            var engine = new FakeEngine();
            var manager = new Mock<UiManager>();
            var controller = new SystemController(
                loggerMock,
                engine,
                hub.Object,
                manager.Object,
                new Mock<IServer>().Object);

            var generator = new Mock<Generators.CodePrograms>();
            var result = Tuple.Create((Stream)new MemoryStream(), fileName);
            generator.Setup(g => g.GenerateAsync(expected))
                .Returns(Task.FromResult(result))
                .Verifiable();
            generator.Setup(g => g.IsFormatAvailable(expected))
                .Returns(true)
                .Verifiable();
            engine.RegisterGenerator(generator.Object);

            // Act
            var response = await controller.ExportCodePrograms(format);

            // Assert
            var streamResult = Assert.IsType<FileStreamResult>(response);
            generator.Verify();
            Assert.Equal(contentType, streamResult.ContentType);
            Assert.Equal(fileName, streamResult.FileDownloadName);
        }

        [Fact]
        public async Task ExportCodeProgramsHandlesInvalidInput()
        {
            // Arrange
            var loggerMock = new FakeLogger<SystemController>();
            var hub = new Mock<IHub>();
            var engine = new FakeEngine();
            var manager = new Mock<UiManager>();
            var controller = new SystemController(
                loggerMock,
                engine,
                hub.Object,
                manager.Object,
                new Mock<IServer>().Object);

            // Act
            var response = await controller.ExportCodePrograms("garbage");

            // Assert
            Assert.IsType<BadRequestObjectResult>(response);
        }

        [Fact]
        public async Task ExportCodeProgramsHandlesUnknownFormat()
        {
            // Arrange
            var loggerMock = new FakeLogger<SystemController>();
            var hub = new Mock<IHub>();
            var engine = new FakeEngine();
            var manager = new Mock<UiManager>();
            var controller = new SystemController(
                loggerMock,
                engine,
                hub.Object,
                manager.Object,
                new Mock<IServer>().Object);

            // Act
            var response = await controller.ExportCodePrograms("unknown");

            // Assert
            Assert.IsType<BadRequestObjectResult>(response);
        }

        [Theory]
        [InlineData(null, ReportFormat.Csv, "text/csv", "all-logs.csv")]
        [InlineData("csv", ReportFormat.Csv, "text/csv", "all-logs.csv")]
        [InlineData("CSV", ReportFormat.Csv, "text/csv", "all-logs.csv")]
        [InlineData(".csv", ReportFormat.Csv, "text/csv", "all-logs.csv")]
        public async Task ExportRobotLogsGeneratesReport(string? format, ReportFormat expected, string contentType, string fileName)
        {
            // Arrange
            var loggerMock = new FakeLogger<SystemController>();
            var hub = new Mock<IHub>();
            var engine = new FakeEngine();
            var manager = new Mock<UiManager>();
            var controller = new SystemController(
                loggerMock,
                engine,
                hub.Object,
                manager.Object,
                new Mock<IServer>().Object);

            var generator = new Mock<Generators.RobotLogs>();
            var result = Tuple.Create((Stream)new MemoryStream(), fileName);
            generator.Setup(g => g.GenerateAsync(expected))
                .Returns(Task.FromResult(result))
                .Verifiable();
            generator.Setup(g => g.IsFormatAvailable(expected))
                .Returns(true)
                .Verifiable();
            engine.RegisterGenerator(generator.Object);

            // Act
            var response = await controller.ExportRobotLogs(format);

            // Assert
            var streamResult = Assert.IsType<FileStreamResult>(response);
            generator.Verify();
            Assert.Equal(contentType, streamResult.ContentType);
            Assert.Equal(fileName, streamResult.FileDownloadName);
        }

        [Fact]
        public async Task ExportRobotLogsHandlesInvalidInput()
        {
            // Arrange
            var loggerMock = new FakeLogger<SystemController>();
            var hub = new Mock<IHub>();
            var engine = new FakeEngine();
            var manager = new Mock<UiManager>();
            var controller = new SystemController(
                loggerMock,
                engine,
                hub.Object,
                manager.Object,
                new Mock<IServer>().Object);

            // Act
            var response = await controller.ExportRobotLogs("garbage");

            // Assert
            Assert.IsType<BadRequestObjectResult>(response);
        }

        [Fact]
        public async Task ExportRobotLogsHandlesUnknownFormat()
        {
            // Arrange
            var loggerMock = new FakeLogger<SystemController>();
            var hub = new Mock<IHub>();
            var engine = new FakeEngine();
            var manager = new Mock<UiManager>();
            var controller = new SystemController(
                loggerMock,
                engine,
                hub.Object,
                manager.Object,
                new Mock<IServer>().Object);

            // Act
            var response = await controller.ExportRobotLogs("unknown");

            // Assert
            Assert.IsType<BadRequestObjectResult>(response);
        }

        [Fact]
        public async Task GetSiteConfigurationRetrievesAddress()
        {
            // Arrange
            var logger = new FakeLogger<SystemController>();
            var hub = new Mock<IHub>();
            var engine = new FakeEngine();
            var query = new Mock<SystemData>();
            engine.RegisterQuery(query.Object);
            var manager = new Mock<UiManager>();
            query.Setup(q => q.RetrieveSystemValuesAsync()).Returns(Task.FromResult(new Data.SystemValues { DefaultAddress = "123" }));
            var controller = new SystemController(
                logger,
                engine,
                hub.Object,
                manager.Object,
                new Mock<IServer>().Object);

            // Act
            var response = await controller.GetSiteConfiguration();

            // Assert
            Assert.NotNull(response.Value);
            Assert.Equal("123", response.Value?.DefaultAddress);
        }

        [Fact]
        public async Task InitialiseCallsAddUser()
        {
            // Arrange
            var logger = new FakeLogger<SystemController>();
            var hub = new Mock<IHub>();
            var engine = new FakeEngine();
            var query = new Mock<UserData>();
            engine.RegisterQuery(query.Object);
            engine.ExpectCommand<AddUser>();
            var manager = new Mock<UiManager>();
            query.Setup(q => q.CheckForAnyAsync()).Returns(Task.FromResult(false));
            var controller = new SystemController(
                logger,
                engine,
                hub.Object,
                manager.Object,
                new Mock<IServer>().Object);

            // Act
            var response = await controller.Initialise(new InitialisationSettings());

            // Assert
            var result = Assert.IsType<ExecutionResult>(response.Value);
            Assert.True(result.Successful, "Expected result to be successful");
            engine.Verify();
        }

        [Theory]
        [InlineData(true, false, "AddUser", "AddUIDefinition")]
        [InlineData(false, true, "AddUser", "AddRobotType", "SetDefaultRobotType", "ImportToolbox")]
        [InlineData(true, true, "AddUser", "AddUIDefinition", "AddRobotType", "SetDefaultRobotType", "ImportToolbox")]
        public async Task InitialiseCallsBatchCommands(bool useUiDefinitions, bool addNaoRobot, params string[] expectedCommands)
        {
            // Arrange
            var logger = new FakeLogger<SystemController>();
            var hub = new Mock<IHub>();
            var engine = new FakeEngine();
            var query = new Mock<UserData>();
            engine.RegisterQuery(query.Object);
            engine.ExpectCommand<Batch>();
            var manager = new UiManager();
            manager.Register<Definitions.Angular.Definition>("Angular", () => ResourceManager.AngularUITemplate);
            query.Setup(q => q.CheckForAnyAsync()).Returns(Task.FromResult(false));
            var controller = new SystemController(
                logger,
                engine,
                hub.Object,
                manager,
                new Mock<IServer>().Object);

            // Act
            var response = await controller.Initialise(new InitialisationSettings
            {
                AddNaoRobot = addNaoRobot,
                UseDefaultUi = useUiDefinitions
            });

            // Assert
            var result = Assert.IsType<ExecutionResult>(response.Value);
            Assert.True(result.Successful, "Expected result to be successful");
            engine.Verify();
            var batchCommand = Assert.IsType<Batch>(engine.LastCommand);
            Assert.Equal(
                expectedCommands,
                batchCommand.Commands.Select(c => c.GetType().Name).ToArray());
        }

        [Fact]
        public async Task InitialiseFailsWhenSystemHasUsers()
        {
            // Arrange
            var logger = new FakeLogger<SystemController>();
            var hub = new Mock<IHub>();
            var engine = new FakeEngine();
            var query = new Mock<UserData>();
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.CheckForAnyAsync()).Returns(Task.FromResult(true));
            var manager = new Mock<UiManager>();
            var controller = new SystemController(
                logger,
                engine,
                hub.Object,
                manager.Object,
                new Mock<IServer>().Object);

            // Act
            var response = await controller.Initialise(new InitialisationSettings());

            // Assert
            var httpResult = Assert.IsType<BadRequestObjectResult>(response.Result);
            var result = Assert.IsType<ExecutionResult>(httpResult.Value);
            Assert.Contains("System already initialised", TestHelpers.ExtractValidationErrors(result));
        }

        [Fact]
        public async Task SetDefaultAddressCallsStoreDefaultAddress()
        {
            // Arrange
            var logger = new FakeLogger<SystemController>();
            var hub = new Mock<IHub>();
            var engine = new FakeEngine();
            engine.ExpectCommand<StoreDefaultAddress>(
                CommandResult.New(1, new Data.SystemValues { DefaultAddress = "1234" }));
            var manager = new Mock<UiManager>();
            var controller = new SystemController(
                logger,
                engine,
                hub.Object,
                manager.Object,
                new Mock<IServer>().Object);

            // Act
            var response = await controller.SetDefaultAddress(
                new SiteConfiguration { DefaultAddress = "4321" });

            // Assert
            var result = Assert.IsType<ExecutionResult<SiteConfiguration>>(response.Value);
            Assert.True(result.Successful, "Expected result to be successful");
            engine.Verify();
            Assert.Equal("1234", result?.Output?.DefaultAddress);
        }

        [Fact]
        public async Task SetDefaultAddressHandlesNoData()
        {
            // Arrange
            var logger = new FakeLogger<SystemController>();
            var hub = new Mock<IHub>();
            var engine = new FakeEngine();
            var manager = new Mock<UiManager>();
            var controller = new SystemController(
                logger,
                engine,
                hub.Object,
                manager.Object,
                new Mock<IServer>().Object);

            // Act
            var response = await controller.SetDefaultAddress(null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(response.Result);
        }

        [Theory]
        [InlineData(true, "ready")]
        [InlineData(false, "pending")]
        public async Task VersionReturnsSystemState(bool hasUsers, string expected)
        {
            // Arrange
            var logger = new FakeLogger<SystemController>();
            var hub = new Mock<IHub>();
            var engine = new FakeEngine();
            var query = new Mock<UserData>();
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.CheckForAnyAsync()).Returns(Task.FromResult(hasUsers));
            var manager = new Mock<UiManager>();
            var controller = new SystemController(
                logger,
                engine,
                hub.Object,
                manager.Object,
                new Mock<IServer>().Object);

            // Act
            var response = await controller.Version();

            // Assert
            Assert.NotNull(response.Value);
            Assert.False(string.IsNullOrEmpty(response.Value?.Version), "Version has not been set");
            Assert.Equal(expected, response.Value?.Status);
        }

        [Fact]
        public async Task WhoAmIFailsIfUserNotSet()
        {
            // Arrange
            var loggerMock = new FakeLogger<SystemController>();
            var hub = new Mock<IHub>();
            var engine = new FakeEngine();
            var manager = new Mock<UiManager>();
            var controller = new SystemController(
                loggerMock,
                engine,
                hub.Object,
                manager.Object,
                new Mock<IServer>().Object);

            // Act
            var response = await controller.WhoAmI();

            // Assert
            var actual = Assert.IsType<ActionResult<User>>(response);
            Assert.IsType<NotFoundResult>(actual.Result);
        }

        [Fact]
        public async Task WhoAmIReturnsCurrentUser()
        {
            // Arrange
            var loggerMock = new FakeLogger<SystemController>();
            var hub = new Mock<IHub>();
            var engine = new FakeEngine();
            var manager = new Mock<UiManager>();
            var controller = new SystemController(
                loggerMock,
                engine,
                hub.Object,
                manager.Object,
                new Mock<IServer>().Object);
            engine.ConfigureUser(controller, "Mia", Data.UserRole.Teacher);

            // Act
            var response = await controller.WhoAmI();

            // Assert
            var actual = Assert.IsType<ActionResult<User>>(response);
            var user = Assert.IsType<User>(actual.Value);
            Assert.Equal("Mia", user.Name);
        }

        private static Mock<IServer> InitialiseServerWithAddresses(string[] addressList)
        {
            var server = new Mock<IServer>();
            var features = new Mock<IFeatureCollection>();
            var addresses = new Mock<IServerAddressesFeature>();
            server.Setup(s => s.Features)
                .Returns(features.Object);
            features.Setup(f => f.Get<IServerAddressesFeature>())
                .Returns(addresses.Object);
            addresses.Setup(a => a.Addresses)
                .Returns(addressList);
            return server;
        }
    }
}