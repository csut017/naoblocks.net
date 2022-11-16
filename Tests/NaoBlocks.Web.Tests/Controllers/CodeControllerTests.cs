using Microsoft.AspNetCore.Mvc;
using Moq;
using NaoBlocks.Common;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Queries;
using NaoBlocks.Parser;
using NaoBlocks.Web.Controllers;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

using Data = NaoBlocks.Engine.Data;
using Transfer = NaoBlocks.Web.Dtos;

namespace NaoBlocks.Web.Tests.Controllers
{
    public class CodeControllerTests
    {
        [Fact]
        public async Task GetProgramRetrievesUserViaQuery()
        {
            // Arrange
            var logger = new FakeLogger<CodeController>();
            var engine = new FakeEngine();
            var userQuery = new Mock<UserData>();
            var codeQuery = new Mock<CodeData>();
            engine.RegisterQuery(userQuery.Object);
            engine.RegisterQuery(codeQuery.Object);
            userQuery.Setup(q => q.RetrieveByNameAsync("Mia"))
                .Returns(Task.FromResult((Data.User?)new Data.User { Id = "users/1", Name = "Mia" }));
            codeQuery.Setup(q => q.RetrieveCodeAsync("users/1", 1))
                .Returns(Task.FromResult((Data.CodeProgram?)new Data.CodeProgram { Code = "go()"}));
            engine.ExpectCommand<CompileCode>(
                CommandResult.New(1, new Data.CompiledCodeProgram(new ParseResult())));
            var controller = new CodeController(
                logger,
                engine);

            // Act
            var response = await controller.Get("Mia", 1);

            // Assert
            Assert.NotNull(response.Value);
            Assert.True(response.Value?.Successful);
        }

        [Fact]
        public async Task GetProgramRetrievesUserHandlesMissingUser()
        {
            // Arrange
            var logger = new FakeLogger<CodeController>();
            var engine = new FakeEngine();
            var query = new Mock<UserData>();
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.RetrieveByNameAsync("Mia"))
                .Returns(Task.FromResult((Data.User?)null));
            var controller = new CodeController(
                logger,
                engine);

            // Act
            var response = await controller.Get("Mia", 1);

            // Assert
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task GetProgramRetrievesUserHandlesMissingProgram()
        {
            // Arrange
            var logger = new FakeLogger<CodeController>();
            var engine = new FakeEngine();
            var userQuery = new Mock<UserData>();
            var codeQuery = new Mock<CodeData>();
            engine.RegisterQuery(userQuery.Object);
            engine.RegisterQuery(codeQuery.Object);
            userQuery.Setup(q => q.RetrieveByNameAsync("Mia"))
                .Returns(Task.FromResult((Data.User?)new Data.User { Name = "Mia" }));
            codeQuery.Setup(q => q.RetrieveCodeAsync("Mia", 1))
                .Returns(Task.FromResult((Data.CodeProgram?)null));
            var controller = new CodeController(
                logger,
                engine);

            // Act
            var response = await controller.Get("Mia", 1);

            // Assert
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task CompileCompilesCode()
        {
            // Arrange
            var logger = new FakeLogger<CodeController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<CompileCode>(
                CommandResult.New(1, new Data.CompiledCodeProgram(new ParseResult())));
            var controller = new CodeController(
                logger,
                engine);

            // Act
            var program = new Transfer.CodeProgram
            {
                Code = "go()"
            };
            var response = await controller.Compile(program);

            // Assert
            Assert.NotNull(response.Value);
            Assert.True(response.Value?.Successful);
        }

        [Fact]
        public async Task CompileHandlesMissingUser()
        {
            // Arrange
            var logger = new FakeLogger<CodeController>();
            var engine = new FakeEngine();
            engine.ExpectCommand<CompileCode>(
                CommandResult.New(1, new Data.CompiledCodeProgram(new ParseResult())));
            var controller = new CodeController(
                logger,
                engine);

            // Act
            var program = new Transfer.CodeProgram
            {
                Code = "go()",
                Store = true
            };
            var response = await controller.Compile(program);

            // Assert
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Fact]
        public async Task CompileStoresProgram()
        {
            // Arrange
            var logger = new FakeLogger<CodeController>();
            var results = new CommandResult[]
            {
                CommandResult.New(2, new Data.CompiledCodeProgram(new ParseResult())),
                CommandResult.New(3, new Data.CodeProgram { Number = 14916 })
            };
            var engine = new FakeEngine();
            engine.ExpectCommand<Batch>(
                CommandResult.New(1, results.AsEnumerable()));
            var controller = new CodeController(
                logger,
                engine);
            engine.ConfigureUser(controller, "Mia", Data.UserRole.Student);

            // Act
            var program = new Transfer.CodeProgram
            {
                Code = "go()",
                Store = true
            };
            var response = await controller.Compile(program);

            // Assert
            Assert.NotNull(response.Value);
            Assert.True(response.Value?.Successful);
            Assert.Equal(14916, response.Value?.Output?.ProgramId);
        }
    }
}
