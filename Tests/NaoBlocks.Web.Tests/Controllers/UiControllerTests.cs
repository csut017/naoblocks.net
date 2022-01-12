using Microsoft.AspNetCore.Mvc;
using Moq;
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
    }
}
