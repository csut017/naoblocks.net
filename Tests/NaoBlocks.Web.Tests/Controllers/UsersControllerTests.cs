using Microsoft.AspNetCore.Mvc;
using Moq;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Queries;
using NaoBlocks.Web.Controllers;
using System.Threading.Tasks;
using Xunit;

using Data = NaoBlocks.Engine.Data;
using Transfer = NaoBlocks.Web.Dtos;

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
            var response = await controller.Get("Mia");

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
            var response = await controller.Get("Mia");

            // Assert
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task DeleteCallsDelete()
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

        [Fact]
        public async Task PutCallsUpdateUser()
        {
            // Arrange
            var logger = new FakeLogger<UsersController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<UpdateUser>(
                CommandResult.New(1, new Data.User()));
            var controller = new UsersController(
                logger,
                engine);

            // Act
            var user = new Transfer.User { Name = "Mia", Role = "Student" };
            var response = await controller.Put("Maia", user);

            // Assert
            var result = Assert.IsType<ExecutionResult<Transfer.User>>(response.Value);
            Assert.True(result.Successful, "Expected result to be successful");
            engine.Verify();
        }

        [Fact]
        public async Task PutValidatesIncomingData()
        {
            // Arrange
            var logger = new FakeLogger<UsersController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<AddUser>();
            var controller = new UsersController(
                logger,
                engine);

            // Act
            var response = await controller.Put("Maia", null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(response.Result);
        }

        [Theory]
        [InlineData("Student", Data.UserRole.Student)]
        [InlineData("Teacher", Data.UserRole.Teacher)]
        [InlineData("", Data.UserRole.Unknown)]
        [InlineData("Missing", Data.UserRole.Unknown)]
        public async Task PutConvertsRole(string roleText, Data.UserRole expected)
        {
            // Arrange
            var logger = new FakeLogger<UsersController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<UpdateUser>(
                CommandResult.New(1, new Data.User()));
            var controller = new UsersController(
                logger,
                engine);

            // Act
            var user = new Transfer.User { Name = "Mia", Role = roleText };
            var response = await controller.Put("Maia", user);

            // Assert
            var result = Assert.IsType<ExecutionResult<Transfer.User>>(response.Value);
            Assert.True(result.Successful, "Expected result to be successful");
            var command = Assert.IsType<UpdateUser>(engine.LastCommand);
            Assert.Equal(expected, command.Role);
        }

        [Fact]
        public async Task PostCallsAddUser()
        {
            // Arrange
            var logger = new FakeLogger<UsersController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<AddUser>(
                CommandResult.New(1, new Data.User()));
            var controller = new UsersController(
                logger,
                engine);

            // Act
            var user = new Transfer.User { Name = "Mia", Role = "Student" };
            var response = await controller.Post(user);

            // Assert
            var result = Assert.IsType<ExecutionResult<Transfer.User>>(response.Value);
            Assert.True(result.Successful, "Expected result to be successful");
            engine.Verify();
        }

        [Fact]
        public async Task PostValidatesIncomingData()
        {
            // Arrange
            var logger = new FakeLogger<UsersController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<AddUser>();
            var controller = new UsersController(
                logger,
                engine);

            // Act
            var response = await controller.Post(null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(response.Result);
        }

        [Theory]
        [InlineData("Student", Data.UserRole.Student)]
        [InlineData("Teacher", Data.UserRole.Teacher)]
        [InlineData("", Data.UserRole.Unknown)]
        [InlineData("Missing", Data.UserRole.Unknown)]
        public async Task PostConvertsRole(string roleText, Data.UserRole expected)
        {
            // Arrange
            var logger = new FakeLogger<UsersController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<AddUser>(
                CommandResult.New(1, new Data.User()));
            var controller = new UsersController(
                logger,
                engine);

            // Act
            var user = new Transfer.User { Name = "Mia", Role = roleText };
            var response = await controller.Post(user);

            // Assert
            var result = Assert.IsType<ExecutionResult<Transfer.User>>(response.Value);
            Assert.True(result.Successful, "Expected result to be successful");
            var command = Assert.IsType<AddUser>(engine.LastCommand);
            Assert.Equal(expected, command.Role);
        }

        [Fact]
        public async Task GetUsersRetrievesViaQuery()
        {
            // Arrange
            var logger = new FakeLogger<UsersController>();
            var engine = new FakeEngine();
            var query = new Mock<UserData>();
            var result = ListResult.New(
                new[] { new Data.User() });
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.RetrievePageAsync(0, 25))
                .Returns(Task.FromResult(result));
            var controller = new UsersController(
                logger,
                engine);

            // Act
            var response = await controller.List(null, null);

            // Assert
            Assert.Equal(1, response.Count);
            Assert.Equal(0, response.Page);
            Assert.NotEmpty(response.Items);
        }

        [Fact]
        public async Task GetUsersHandlesNullData()
        {
            // Arrange
            var logger = new FakeLogger<UsersController>();
            var engine = new FakeEngine();
            var query = new Mock<UserData>();
            var result = new ListResult<Data.User>();
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.RetrievePageAsync(0, 25))
                .Returns(Task.FromResult(result));
            var controller = new UsersController(
                logger,
                engine);

            // Act
            var response = await controller.List(null, null);

            // Assert
            Assert.Equal(0, response.Count);
            Assert.Equal(0, response.Page);
            Assert.Null(response.Items);
        }
    }
}
