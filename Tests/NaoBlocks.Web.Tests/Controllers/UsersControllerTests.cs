using Microsoft.AspNetCore.Mvc;
using Moq;
using NaoBlocks.Common;
using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Queries;
using NaoBlocks.Web.Controllers;
using System.Threading.Tasks;
using Xunit;

using Data = NaoBlocks.Engine.Data;

namespace NaoBlocks.Web.Tests.Controllers
{
    public class UsersControllerTests
    {
        [Fact]
        public async Task GetUserRetrievesUserViaQuery()
        {
            // Arrange
            var logger = new FakeLogger<UsersController>();
            var engine = new FakeEngine();
            var query = new Mock<UserData>();
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.RetrieveByNameAsync("Mia"))
                .Returns(Task.FromResult((Data.User?)new Data.User { Name = "Mia" }));
            var controller = new UsersController(
                logger,
                engine);

            // Act
            var response = await controller.GetUser("Mia");

            // Assert
            Assert.NotNull(response.Value);
            Assert.Equal("Mia", response.Value?.Name);
        }

        [Fact]
        public async Task GetUserRetrievesUserHandlesMissingUser()
        {
            // Arrange
            var logger = new FakeLogger<UsersController>();
            var engine = new FakeEngine();
            var query = new Mock<UserData>();
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.RetrieveByNameAsync("Mia"))
                .Returns(Task.FromResult((Data.User?)null));
            var controller = new UsersController(
                logger,
                engine);

            // Act
            var response = await controller.GetUser("Mia");

            // Assert
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task SetDefaultAddressCallsStoreDefaultAddress()
        {
            // Arrange
            var logger = new FakeLogger<UsersController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<DeleteUser>();
            var controller = new UsersController(
                logger,
                engine);

            // Act
            var response = await controller.Delete("Mia");

            // Assert
            var result = Assert.IsType<ExecutionResult>(response.Value);
            Assert.True(result.Successful, "Expected result to be successful");
            engine.Verify();
        }
    }
}
