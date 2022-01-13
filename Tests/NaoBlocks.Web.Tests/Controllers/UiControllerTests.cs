using Microsoft.AspNetCore.Mvc;
using Moq;
using NaoBlocks.Common;
using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Queries;
using NaoBlocks.Web.Controllers;
using NaoBlocks.Web.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

using Angular = NaoBlocks.Definitions.Angular;

namespace NaoBlocks.Web.Tests.Controllers
{
    public class UiControllerTests
    {
        [Fact]
        public async Task GetComponentRetrievesComponent()
        {
            // Arrange
            var logger = new FakeLogger<UiController>();
            var engine = new FakeEngine();
            var manager = new UiManager();
            var query = new Mock<UIDefinitionData>();
            engine.RegisterQuery(query.Object);
            var definition = new FakeUIDefinition { Data = "output" };
            query.Setup(q => q.RetrieveByNameAsync("angular"))
                .Returns(Task.FromResult((UIDefinition?)new UIDefinition { Definition = definition }));
            definition.ExpectGenerate("converter");
            var controller = new UiController(logger, engine, manager);

            // Act
            var result = await controller.GetComponent("angular", "converter");

            // Assert
            definition.Verify();
            var fileStreamResult = Assert.IsType<FileStreamResult>(result);
            Assert.Equal(ContentTypes.Txt, fileStreamResult.ContentType);
            using var reader = new StreamReader(fileStreamResult.FileStream);
            var text = reader.ReadToEnd();
            Assert.Equal("output", text);
        }

        [Fact]
        public async Task GetComponentHandlesMissingDefinition()
        {
            // Arrange
            var logger = new FakeLogger<UiController>();
            var engine = new FakeEngine();
            var manager = new UiManager();
            var query = new Mock<UIDefinitionData>();
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.RetrieveByNameAsync("angular"))
                .Returns(Task.FromResult((UIDefinition?)null));
            var controller = new UiController(logger, engine, manager);

            // Act
            var result = await controller.GetComponent("angular", "converter");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteCallsDelete()
        {
            // Arrange
            var logger = new FakeLogger<UiController>();
            var engine = new FakeEngine();
            var manager = new UiManager();
            engine.ExpectCommand<DeleteUIDefinition>();
            var controller = new UiController(logger, engine, manager);

            // Act
            var response = await controller.Delete("angular");

            // Assert
            var result = Assert.IsType<ExecutionResult>(response.Value);
            Assert.True(result.Successful, "Expected result to be successful");
            engine.Verify();
        }

        [Fact]
        public async Task PostCallsAdds()
        {
            // Arrange
            var logger = new FakeLogger<UiController>();
            var engine = new FakeEngine();
            var manager = new UiManager();
            manager.Register<Angular.Definition>("angular");
            engine.ExpectCommand<AddUIDefinition>();
            var controller = new UiController(logger, engine, manager);
            controller.SetRequestBody("{}");

            // Act
            var response = await controller.Post("angular");

            // Assert
            var result = Assert.IsType<ExecutionResult>(response.Value);
            Assert.True(result.Successful, "Expected result to be successful");
            engine.Verify();
            var command = Assert.IsType<AddUIDefinition>(engine.LastCommand);
            Assert.Equal("angular", command.Name);
            Assert.IsType<Angular.Definition>(command.Definition);
        }

        [Theory]
        [InlineData("")]
        [InlineData("bad")]
        public async Task PostValidatesData(string body)
        {
            // Arrange
            var logger = new FakeLogger<UiController>();
            var engine = new FakeEngine();
            var manager = new UiManager();
            manager.Register<Angular.Definition>("angular");
            engine.ExpectCommand<AddUIDefinition>();
            var controller = new UiController(logger, engine, manager);
            controller.SetRequestBody(body);

            // Act
            var response = await controller.Post("angular");

            // Assert
            var result = Assert.IsType<BadRequestObjectResult>(response.Result);
        }
    }
}
