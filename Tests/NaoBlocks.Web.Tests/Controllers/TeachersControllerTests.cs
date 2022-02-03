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
    public class TeachersControllerTests
    {
        [Fact]
        public async Task DeleteCallsDelete()
        {
            // Arrange
            var logger = new FakeLogger<TeachersController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<DeleteUser>();
            var controller = new TeachersController(
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
        public async Task GetTeacherRetrievesTeacherHandlesMissingTeacher()
        {
            // Arrange
            var logger = new FakeLogger<TeachersController>();
            var engine = new FakeEngine();
            var query = new Mock<UserData>();
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.RetrieveByNameAsync("Mia"))
                .Returns(Task.FromResult((Data.User?)null));
            var controller = new TeachersController(
                logger,
                engine);

            // Act
            var response = await controller.GetTeacher("Mia");

            // Assert
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task GetTeacherRetrievesTeacherHandlesNonTeacherRole()
        {
            // Arrange
            var logger = new FakeLogger<TeachersController>();
            var engine = new FakeEngine();
            var query = new Mock<UserData>();
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.RetrieveByNameAsync("Mia"))
                .Returns(Task.FromResult((Data.User?)new Data.User
                {
                    Name = "Mia",
                    Role = Data.UserRole.Student
                }));
            var controller = new TeachersController(
                logger,
                engine);

            // Act
            var response = await controller.GetTeacher("Mia");

            // Assert
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task GetTeacherRetrievesTeacherViaQuery()
        {
            // Arrange
            var logger = new FakeLogger<TeachersController>();
            var engine = new FakeEngine();
            var query = new Mock<UserData>();
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.RetrieveByNameAsync("Mia"))
                .Returns(Task.FromResult((Data.User?)new Data.User
                {
                    Name = "Mia",
                    Role = Data.UserRole.Teacher
                }));
            var controller = new TeachersController(
                logger,
                engine);

            // Act
            var response = await controller.GetTeacher("Mia");

            // Assert
            Assert.NotNull(response.Value);
            Assert.Equal("Mia", response.Value?.Name);
        }

        [Fact]
        public async Task GetTeachersHandlesNullData()
        {
            // Arrange
            var logger = new FakeLogger<TeachersController>();
            var engine = new FakeEngine();
            var query = new Mock<UserData>();
            var result = new ListResult<Data.User>();
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.RetrievePageAsync(0, 25, Data.UserRole.Teacher))
                .Returns(Task.FromResult(result));
            var controller = new TeachersController(
                logger,
                engine);

            // Act
            var response = await controller.GetTeachers(null, null);

            // Assert
            Assert.Equal(0, response.Count);
            Assert.Equal(0, response.Page);
            Assert.Null(response.Items);
        }

        [Fact]
        public async Task GetTeachersRetrievesViaQuery()
        {
            // Arrange
            var logger = new FakeLogger<TeachersController>();
            var engine = new FakeEngine();
            var query = new Mock<UserData>();
            var result = ListResult.New(
                new[] {
                    new Data.User { Role = Data.UserRole.Teacher }
                });
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.RetrievePageAsync(0, 25, Data.UserRole.Teacher))
                .Returns(Task.FromResult(result));
            var controller = new TeachersController(
                logger,
                engine);

            // Act
            var response = await controller.GetTeachers(null, null);

            // Assert
            Assert.Equal(1, response.Count);
            Assert.Equal(0, response.Page);
            Assert.NotEmpty(response.Items);
        }

        [Fact]
        public async Task PostCallsAddUser()
        {
            // Arrange
            var logger = new FakeLogger<TeachersController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<AddUser>(
                CommandResult.New(1, new Data.User()));
            var controller = new TeachersController(
                logger,
                engine);

            // Act
            var Teacher = new Transfer.Teacher { Name = "Mia" };
            var response = await controller.Post(Teacher);

            // Assert
            var result = Assert.IsType<ExecutionResult<Transfer.Teacher>>(response.Value);
            Assert.True(result.Successful, "Expected result to be successful");
            engine.Verify();
            var command = Assert.IsType<AddUser>(engine.LastCommand);
            Assert.Equal(Data.UserRole.Teacher, command.Role);
        }

        [Fact]
        public async Task PostValidatesIncomingData()
        {
            // Arrange
            var logger = new FakeLogger<TeachersController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<AddUser>();
            var controller = new TeachersController(
                logger,
                engine);

            // Act
            var response = await controller.Post(null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(response.Result);
        }

        [Fact]
        public async Task PutCallsUpdateTeacher()
        {
            // Arrange
            var logger = new FakeLogger<TeachersController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<UpdateUser>(
                CommandResult.New(1, new Data.User()));
            var controller = new TeachersController(
                logger,
                engine);

            // Act
            var Teacher = new Transfer.Teacher { Name = "Mia", Role = "Student" };
            var response = await controller.Put("Maia", Teacher);

            // Assert
            var result = Assert.IsType<ExecutionResult<Transfer.Teacher>>(response.Value);
            Assert.True(result.Successful, "Expected result to be successful");
            engine.Verify();

            var command = Assert.IsType<UpdateUser>(engine.LastCommand);
            Assert.Equal(Data.UserRole.Teacher, command.Role);
        }

        [Fact]
        public async Task PutValidatesIncomingData()
        {
            // Arrange
            var logger = new FakeLogger<TeachersController>();
            var engine = new FakeEngine();
            var controller = new TeachersController(
                logger,
                engine);

            // Act
            var response = await controller.Put("Maia", null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(response.Result);
        }
    }
}