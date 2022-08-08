using Microsoft.AspNetCore.Hosting.Server;
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

using Data = NaoBlocks.Engine.Data;
using ResourceManager = NaoBlocks.Web.Resources.Manager;

namespace NaoBlocks.Web.Tests.Controllers
{
    [Collection("ClientAddressList tests")]
    public class SystemControllerTests
    {
        [Fact]
        public async Task ClientAddressesFileRetrievesTextFile()
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
            var response = await controller.ClientAddressesFile();

            // Assert
            var result = Assert.IsType<FileContentResult>(response);
            Assert.Equal("connect.txt", result.FileDownloadName);
            Assert.Equal(ContentTypes.Txt, result.ContentType);
            Assert.Equal(
                string.Join("\n", new[] { "http://test,,no", "https://test,,yes" }),
                Encoding.UTF8.GetString(result.FileContents));
        }

        [Fact]
        public async Task ClientAddressesRetrievesAddresses()
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
            var response = await controller.ClientAddresses();

            // Assert
            var result = Assert.IsType<ListResult<string>>(response.Value);
            Assert.Equal(1, result.Count);
            Assert.Equal(new[] { "Test Address" }, result.Items);
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
    }
}