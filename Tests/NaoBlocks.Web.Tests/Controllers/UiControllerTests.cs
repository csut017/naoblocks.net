using Microsoft.AspNetCore.Mvc;
using Moq;
using NaoBlocks.Common;
using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Queries;
using NaoBlocks.Web.Controllers;
using NaoBlocks.Web.Helpers;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

using Angular = NaoBlocks.Definitions.Angular;

namespace NaoBlocks.Web.Tests.Controllers
{
    public class UiControllerTests
    {
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
        public async Task ExportHandlesMissingDefinition()
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
            var result = await controller.Export("angular");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ExportHandlesReturnsDefinition()
        {
            // Arrange
            var logger = new FakeLogger<UiController>();
            var engine = new FakeEngine();
            var manager = new UiManager();
            var query = new Mock<UIDefinitionData>();
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.RetrieveByNameAsync("angular"))
                .Returns(Task.FromResult((UIDefinition?)new UIDefinition
                {
                    Name = "angular",
                    Definition = new FakeUIDefinition { Data = "one" }
                }));
            var controller = new UiController(logger, engine, manager);

            // Act
            var result = await controller.Export("angular");

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var data = Assert.IsType<FakeUIDefinition>(json.Value);
            Assert.Equal("one", data.Data);
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
            Assert.Equal("text/plain", fileStreamResult.ContentType);
            using var reader = new StreamReader(fileStreamResult.FileStream);
            var text = reader.ReadToEnd();
            Assert.Equal("output", text);
        }

        [Fact]
        public async Task GetDescriptionHandlesDefinition()
        {
            // Arrange
            var logger = new FakeLogger<UiController>();
            var engine = new FakeEngine();
            var manager = new UiManager();
            var query = new Mock<UIDefinitionData>();
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.RetrieveByNameAsync("angular"))
                .Returns(Task.FromResult((UIDefinition?)new UIDefinition
                {
                    Name = "angular",
                    Definition = FakeUIDefinition.New(
                        UIDefinitionItem.New("first"),
                        UIDefinitionItem.New("second"))
                }));
            var controller = new UiController(logger, engine, manager);

            // Act
            var result = await controller.GetDescription("angular");

            // Assert
            Assert.Equal(
                new[] { "first", "second" },
                result.Value?.Items?.Select(i => i.Name).ToArray());
        }

        [Fact]
        public async Task GetDescriptionHandlesEmptyDefinition()
        {
            // Arrange
            var logger = new FakeLogger<UiController>();
            var engine = new FakeEngine();
            var manager = new UiManager();
            var query = new Mock<UIDefinitionData>();
            engine.RegisterQuery(query.Object);
            query.Setup(q => q.RetrieveByNameAsync("angular"))
                .Returns(Task.FromResult((UIDefinition?)new UIDefinition { Name = "angular" }));
            var controller = new UiController(logger, engine, manager);

            // Act
            var result = await controller.GetDescription("angular");

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetDescriptionHandlesMissingDefinition()
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
            var result = await controller.GetDescription("angular");

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task ListRetrievesAllRegistered()
        {
            // Arrange
            var logger = new FakeLogger<UiController>();
            var engine = new FakeEngine();
            var query = new Mock<UIDefinitionData>();
            query.Setup(q => q.RetrievePageAsync(0, It.IsAny<int>()))
                .Returns(Task.FromResult(ListResult.New(new[]
                {
                    new UIDefinition { Name = "angular" }
                })));
            engine.RegisterQuery(query.Object);
            var manager = new UiManager();
            manager.Register<Angular.Definition>("angular", () => "default");
            var controller = new UiController(logger, engine, manager);

            // Act
            var response = await controller.List(null, null);

            // Assert
            Assert.Equal(1, response.Count);
            Assert.Equal(0, response.Page);
            Assert.Equal(
                new[] { "angular=>Angular: Blockly" },
                response.Items?.Select(d => $"{d.Key}=>{d.Name}").ToArray());
        }

        [Fact]
        public async Task PostCallsAdd()
        {
            // Arrange
            var logger = new FakeLogger<UiController>();
            var engine = new FakeEngine();
            var manager = new UiManager();
            manager.Register<Angular.Definition>("angular", () => "default");
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
        [InlineData("yes")]
        [InlineData("yEs")]
        [InlineData("YES")]
        public async Task PostCallsBatch(string replace)
        {
            // Arrange
            var logger = new FakeLogger<UiController>();
            var engine = new FakeEngine();
            var manager = new UiManager();
            manager.Register<Angular.Definition>("angular", () => "default");
            engine.ExpectCommand<Batch>();
            var controller = new UiController(logger, engine, manager);
            controller.SetRequestBody("{}");

            // Act
            var response = await controller.Post("angular", replace);

            // Assert
            var result = Assert.IsType<ExecutionResult>(response.Value);
            Assert.True(result.Successful, "Expected result to be successful");
            engine.Verify();
            var batch = Assert.IsType<Batch>(engine.LastCommand);
            var deleteCommand = Assert.IsType<DeleteUIDefinition>(batch.Commands[0]);
            Assert.Equal("angular", deleteCommand.Name);
            var addCommand = Assert.IsType<AddUIDefinition>(batch.Commands[1]);
            Assert.Equal("angular", addCommand.Name);
            Assert.IsType<Angular.Definition>(addCommand.Definition);
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
            manager.Register<Angular.Definition>("angular", () => "default");
            engine.ExpectCommand<AddUIDefinition>();
            var controller = new UiController(logger, engine, manager);
            controller.SetRequestBody(body);

            // Act
            var response = await controller.Post("angular");

            // Assert
            Assert.IsType<BadRequestObjectResult>(response.Result);
        }
    }
}