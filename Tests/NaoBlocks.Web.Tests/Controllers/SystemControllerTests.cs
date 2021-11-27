using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Web.Tests.Controllers
{
    [Collection("ClientAddressList tests")]
    public class SystemControllerTests
    {
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
            var controller = new SystemController(
                logger,
                engine,
                hub.Object);

            // Act
            var response = await controller.Version();

            // Assert
            Assert.NotNull(response.Value);
            Assert.False(string.IsNullOrEmpty(response.Value?.Version), "Version has not been set");
            Assert.Equal(expected, response.Value?.Status);
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
            var controller = new SystemController(
                logger,
                engine,
                hub.Object);

            // Act
            var response = await controller.Initialise(new User());

            // Assert
            var httpResult = Assert.IsType<BadRequestObjectResult>(response.Result);
            var result = Assert.IsType<ExecutionResult>(httpResult.Value);
            Assert.Contains("System already initialised", TestHelpers.ExtractValidationErrors(result));
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
            query.Setup(q => q.CheckForAnyAsync()).Returns(Task.FromResult(false));
            var controller = new SystemController(
                logger,
                engine,
                hub.Object);

            // Act
            var response = await controller.Initialise(new User());

            // Assert
            var result = Assert.IsType<ExecutionResult>(response.Value);
            Assert.True(result.Successful, "Expected result to be successful");
            engine.Verify();
        }

        [Fact]
        public async Task ClientAddressesRetrievesAddresses()
        {
            // Arrange
            var logger = new FakeLogger<SystemController>();
            var hub = new Mock<IHub>();
            var engine = new FakeEngine();
            var controller = new SystemController(
                logger,
                engine,
                hub.Object);
            ClientAddressList.Clear();
            ClientAddressList.Add("Test Address");

            // Act
            var response = await controller.ClientAddresses();

            // Assert
            var result = Assert.IsType<ListResult<string>>(response.Value);
            Assert.Equal(1, result.Count);
            Assert.Equal(new[] { "Test Address" }, result.Items);
        }

        [Fact]
        public async Task ClientAddressesFileRetrievesTextFile()
        {
            // Arrange
            var logger = new FakeLogger<SystemController>();
            var hub = new Mock<IHub>();
            var engine = new FakeEngine();
            var controller = new SystemController(
                logger,
                engine,
                hub.Object);
            ClientAddressList.Clear();
            ClientAddressList.Add("http://test");
            ClientAddressList.Add("https://test");

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
    }
}
