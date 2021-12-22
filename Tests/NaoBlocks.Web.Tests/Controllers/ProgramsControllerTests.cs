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
    public class ProgramsControllerTests
    {
        [Fact]
        public async Task GetRetrievesUserViaQuery()
        {
            // Arrange
            var logger = new FakeLogger<ProgramsController>();
            var engine = new FakeEngine();
            var controller = new ProgramsController(
                logger,
                engine);
            (_, var userQuery) = engine.ConfigureUser(controller, "Mia", Data.UserRole.Teacher);
            userQuery.Setup(q => q.RetrieveByNameAsync("Moana"))
                .Returns(Task.FromResult((Data.User?)new Data.User { Name = "Moana" }));
            var codeQuery = new Mock<CodeData>();
            codeQuery.Setup(q => q.RetrieveCodeAsync("Moana", 3))
                .Returns(Task.FromResult((Data.CodeProgram?)new Data.CodeProgram { Name = "hōtaka", Code = "go()" }));
            engine.RegisterQuery(codeQuery.Object);

            // Act
            var response = await controller.Get("3", "Moana");

            // Assert
            Assert.NotNull(response.Value);
            Assert.Equal("hōtaka", response.Value?.Name);
            Assert.Equal("go()", response.Value?.Code);
        }

        [Fact]
        public async Task GetHandlesMissingUser()
        {
            // Arrange
            var logger = new FakeLogger<ProgramsController>();
            var engine = new FakeEngine();
            var controller = new ProgramsController(
                logger,
                engine);
            engine.ConfigureUser(controller, "Mia", Data.UserRole.Teacher);
            var codeQuery = new Mock<CodeData>();
            codeQuery.Setup(q => q.RetrieveCodeAsync("Mia", 3))
                .Returns(Task.FromResult((Data.CodeProgram?)new Data.CodeProgram { Name = "hōtaka" }));
            engine.RegisterQuery(codeQuery.Object);

            // Act
            var response = await controller.Get("3", null);

            // Assert
            Assert.NotNull(response.Value);
            Assert.Equal("hōtaka", response.Value?.Name);
        }

        [Fact]
        public async Task GetHandlesStudent()
        {
            // Arrange
            var logger = new FakeLogger<ProgramsController>();
            var engine = new FakeEngine();
            var controller = new ProgramsController(
                logger,
                engine);
            engine.ConfigureUser(controller, "Mia", Data.UserRole.Student);
            var codeQuery = new Mock<CodeData>();
            codeQuery.Setup(q => q.RetrieveCodeAsync("Mia", 3))
                .Returns(Task.FromResult((Data.CodeProgram?)new Data.CodeProgram { Name = "hōtaka" }));
            engine.RegisterQuery(codeQuery.Object);

            // Act
            var response = await controller.Get("3", "Moana");

            // Assert
            Assert.NotNull(response.Value);
            Assert.Equal("hōtaka", response.Value?.Name);
        }

        [Fact]
        public async Task GetHandlesMissingProgam()
        {
            // Arrange
            var logger = new FakeLogger<ProgramsController>();
            var engine = new FakeEngine();
            var controller = new ProgramsController(
                logger,
                engine);
            (_, var userQuery) = engine.ConfigureUser(controller, "Mia", Data.UserRole.Teacher);
            userQuery.Setup(q => q.RetrieveByNameAsync("Moana"))
                .Returns(Task.FromResult((Data.User?)new Data.User { Name = "Moana" }));
            var codeQuery = new Mock<CodeData>();
            codeQuery.Setup(q => q.RetrieveCodeAsync("Moana", 3))
                .Returns(Task.FromResult((Data.CodeProgram?)null));
            engine.RegisterQuery(codeQuery.Object);

            // Act
            var response = await controller.Get("3", "Moana");

            // Assert
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task GetHandlesUnknownUser()
        {
            // Arrange
            var logger = new FakeLogger<ProgramsController>();
            var engine = new FakeEngine();
            var controller = new ProgramsController(
                logger,
                engine);
            engine.ConfigureUser(controller, "Mia", Data.UserRole.Teacher);

            // Act
            var response = await controller.Get("3", "Moana");

            // Assert
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task GetHandlesBadData()
        {
            // Arrange
            var logger = new FakeLogger<ProgramsController>();
            var engine = new FakeEngine();
            var controller = new ProgramsController(
                logger,
                engine);
            engine.ConfigureUser(controller, "Mia", Data.UserRole.Teacher);

            // Act
            var response = await controller.Get("Bad", "Moana");

            // Assert
            Assert.IsType<BadRequestObjectResult>(response.Result);
        }

        [Fact]
        public async Task GetUnhandlesRequestWithoutUser()
        {
            // Arrange
            var logger = new FakeLogger<ProgramsController>();
            var engine = new FakeEngine();
            var controller = new ProgramsController(
                logger,
                engine);

            // Act
            var response = await controller.Get("3", "Mia");

            // Assert
            Assert.IsType<UnauthorizedResult>(response.Result);
        }

        [Fact]
        public async Task DeleteCallsDelete()
        {
            // Arrange
            var logger = new FakeLogger<ProgramsController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<DeleteProgram>();
            var controller = new ProgramsController(
                logger,
                engine);

            // Act
            var response = await controller.Delete("3", "Mia");

            // Assert
            var result = Assert.IsType<ExecutionResult>(response.Value);
            Assert.True(result.Successful, "Expected result to be successful");
            engine.Verify();
        }

        [Fact]
        public async Task DeleteHandlesInvalidData()
        {
            // Arrange
            var logger = new FakeLogger<ProgramsController>();
            var engine = new FakeEngine();
            var controller = new ProgramsController(
                logger,
                engine);

            // Act
            var response = await controller.Delete("Bad", "Mia");

            // Assert
            Assert.IsType<BadRequestObjectResult>(response.Result);
        }

        [Fact]
        public async Task ListHandlesUnknownUser()
        {
            // Arrange
            var logger = new FakeLogger<ProgramsController>();
            var engine = new FakeEngine();
            var controller = new ProgramsController(
                logger,
                engine);
            engine.ConfigureUser(controller, "Mia", Data.UserRole.Teacher);

            // Act
            var response = await controller.List(0, 2, "Moana", null);

            // Assert
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task ListUnhandlesRequestWithoutUser()
        {
            // Arrange
            var logger = new FakeLogger<ProgramsController>();
            var engine = new FakeEngine();
            var controller = new ProgramsController(
                logger,
                engine);

            // Act
            var response = await controller.List(0, 2, "Moana", null);

            // Assert
            Assert.IsType<UnauthorizedResult>(response.Result);
        }

        [Fact]
        public async Task ListRetrievesPrograms()
        {
            var programs = ListResult.New(new[]
            {
                new Data.CodeProgram(),
                new Data.CodeProgram()
            });

            // Arrange
            var logger = new FakeLogger<ProgramsController>();
            var engine = new FakeEngine();
            var controller = new ProgramsController(
                logger,
                engine);
            (_, var userQuery) = engine.ConfigureUser(controller, "Mia", Data.UserRole.Teacher);
            userQuery.Setup(q => q.RetrieveByNameAsync("Moana"))
                .Returns(Task.FromResult((Data.User?)new Data.User { Name = "Moana" }));
            var codeQuery = new Mock<CodeData>();
            codeQuery.Setup(q => q.RetrieveForUserAsync("Moana", 0, 2, false))
                .Returns(Task.FromResult(programs));
            engine.RegisterQuery(codeQuery.Object);

            // Act
            var response = await controller.List(0, 2, "Moana", null);

            // Assert
            Assert.Equal(0, response.Value?.Page);
            Assert.Equal(2, response.Value?.Count);
            Assert.NotEmpty(response.Value?.Items);
        }

        [Fact]
        public async Task ListHandlesStudentRequest()
        {
            var programs = ListResult.New(new[]
            {
                new Data.CodeProgram(),
                new Data.CodeProgram()
            });

            // Arrange
            var logger = new FakeLogger<ProgramsController>();
            var engine = new FakeEngine();
            var controller = new ProgramsController(
                logger,
                engine);
            engine.ConfigureUser(controller, "Mia", Data.UserRole.Student);
            var codeQuery = new Mock<CodeData>();
            codeQuery.Setup(q => q.RetrieveForUserAsync("Mia", 0, 2, false))
                .Returns(Task.FromResult(programs));
            engine.RegisterQuery(codeQuery.Object);

            // Act
            var response = await controller.List(0, 2, "Moana", null);

            // Assert
            Assert.Equal(0, response.Value?.Page);
            Assert.Equal(2, response.Value?.Count);
            Assert.NotEmpty(response.Value?.Items);
        }

        [Fact]
        public async Task ListHandlesMissingUser()
        {
            var programs = ListResult.New(new[]
            {
                new Data.CodeProgram(),
                new Data.CodeProgram()
            });

            // Arrange
            var logger = new FakeLogger<ProgramsController>();
            var engine = new FakeEngine();
            var controller = new ProgramsController(
                logger,
                engine);
            engine.ConfigureUser(controller, "Mia", Data.UserRole.Teacher);
            var codeQuery = new Mock<CodeData>();
            codeQuery.Setup(q => q.RetrieveForUserAsync("Mia", 0, 2, false))
                .Returns(Task.FromResult(programs));
            engine.RegisterQuery(codeQuery.Object);

            // Act
            var response = await controller.List(0, 2, null, null);

            // Assert
            Assert.Equal(0, response.Value?.Page);
            Assert.Equal(2, response.Value?.Count);
            Assert.NotEmpty(response.Value?.Items);
        }

        [Fact]
        public async Task ListHandlesAllPrograms()
        {
            var programs = ListResult.New(new[]
            {
                new Data.CodeProgram(),
                new Data.CodeProgram()
            });

            // Arrange
            var logger = new FakeLogger<ProgramsController>();
            var engine = new FakeEngine();
            var controller = new ProgramsController(
                logger,
                engine);
            engine.ConfigureUser(controller, "Mia", Data.UserRole.Teacher);
            var codeQuery = new Mock<CodeData>();
            codeQuery.Setup(q => q.RetrieveForUserAsync("Mia", 0, 2, true))
                .Returns(Task.FromResult(programs));
            engine.RegisterQuery(codeQuery.Object);

            // Act
            var response = await controller.List(0, 2, null, "all");

            // Assert
            Assert.Equal(0, response.Value?.Page);
            Assert.Equal(2, response.Value?.Count);
            Assert.NotEmpty(response.Value?.Items);
        }

        [Fact]
        public async Task ListHandlesNoItems()
        {
            var programs = new ListResult<Data.CodeProgram>
            {
                Count = 0,
                Page = 0
            };

            // Arrange
            var logger = new FakeLogger<ProgramsController>();
            var engine = new FakeEngine();
            var controller = new ProgramsController(
                logger,
                engine);
            engine.ConfigureUser(controller, "Mia", Data.UserRole.Teacher);
            var codeQuery = new Mock<CodeData>();
            codeQuery.Setup(q => q.RetrieveForUserAsync("Mia", 0, 2, true))
                .Returns(Task.FromResult(programs));
            engine.RegisterQuery(codeQuery.Object);

            // Act
            var response = await controller.List(0, 2, null, "all");

            // Assert
            Assert.Equal(0, response.Value?.Page);
            Assert.Equal(0, response.Value?.Count);
            Assert.Null(response.Value?.Items);
        }
    }
}
