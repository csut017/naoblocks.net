using Microsoft.AspNetCore.Mvc;
using Moq;
using NaoBlocks.Engine;
using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Generators;
using NaoBlocks.Engine.Queries;
using NaoBlocks.Web.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Web.Tests.Helpers
{
    public class ControllerHelpersTests
    {
        [Fact]
        public async Task GenerateRobotReportHandlesMissingName()
        {
            // Arrange
            var engine = new FakeEngine();
            var controller = new Mock<ControllerBase>();
            controller.Setup(c => c.BadRequest(It.IsAny<object>()))
                .Returns(new Func<object, BadRequestObjectResult>(err => new BadRequestObjectResult(err)))
                .Verifiable();

            // Act
            var result = await controller.Object.GenerateRobotReport<RobotsList>(engine, null, null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            controller.Verify();
        }

        [Fact]
        public async Task GenerateRobotReportHandlesUnknownRobot()
        {
            // Arrange
            var engine = new FakeEngine();
            var query = new Mock<RobotData>();
            engine.RegisterQuery(query.Object);
            var controller = new Mock<ControllerBase>();
            controller.Setup(c => c.NotFound())
                .Returns(new NotFoundResult())
                .Verifiable();

            // Act
            var result = await controller.Object.GenerateRobotReport<RobotsList>(engine, null, "Karetao");

            // Assert
            Assert.IsType<NotFoundResult>(result);
            controller.Verify();
        }

        [Fact]
        public async Task GenerateRobotTypeReportHandlesMissingName()
        {
            // Arrange
            var engine = new FakeEngine();
            var controller = new Mock<ControllerBase>();
            controller.Setup(c => c.BadRequest(It.IsAny<object>()))
                .Returns(new Func<object, BadRequestObjectResult>(err => new BadRequestObjectResult(err)))
                .Verifiable();

            // Act
            var result = await controller.Object.GenerateRobotTypeReport<RobotsList>(engine, null, null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            controller.Verify();
        }

        [Fact]
        public async Task GenerateRobotTypeReportHandlesUnknownRobotType()
        {
            // Arrange
            var engine = new FakeEngine();
            var query = new Mock<RobotTypeData>();
            engine.RegisterQuery(query.Object);
            var controller = new Mock<ControllerBase>();
            controller.Setup(c => c.NotFound())
                .Returns(new NotFoundResult())
                .Verifiable();

            // Act
            var result = await controller.Object.GenerateRobotTypeReport<RobotsList>(engine, null, "Karetao");

            // Assert
            Assert.IsType<NotFoundResult>(result);
            controller.Verify();
        }

        [Fact]
        public async Task LoadUserHandlesMissingUser()
        {
            var engine = new FakeEngine();
            var controller = new Mock<ControllerBase>();
            var user = await controller.Object.LoadUserAsync(engine);
            Assert.Null(user);
        }

        [Fact]
        public async Task LoadUserRetrievesUser()
        {
            var engine = new FakeEngine();
            var controller = new Mock<ControllerBase>();
            engine.ConfigureUser(controller.Object, "Mia", UserRole.Teacher);
            var user = await controller.Object.LoadUserAsync(engine);
            Assert.Equal("Mia", user?.Name);
        }

        [Fact]
        public void MakeArgsParsesArguments()
        {
            // Arrange
            var controller = new Mock<ControllerBase>();

            // Act
            var args = controller.Object.MakeArgs("tahi", "rua=two", "toru=");

            // Assert
            Assert.Equal(
                new[]
                {
                    "tahi->",
                    "rua->two",
                    "toru->"
                },
                args.Select(kvp => $"{kvp.Key}->{kvp.Value}").ToArray());
        }

        [Fact]
        public void ParseFlagsParsesArguments()
        {
            // Arrange
            var controller = new Mock<ControllerBase>();

            // Act
            var args = controller.Object.ParseFlags("tahi,,rua");

            // Assert
            Assert.Equal(
                new[]
                {
                    "tahi->yes",
                    "rua->yes"
                },
                args.Select(kvp => $"{kvp.Key}->{kvp.Value}").ToArray());
        }

        [Theory]
        [InlineData(null, true, ReportFormat.Excel)]
        [InlineData("", true, ReportFormat.Excel)]
        [InlineData("pdf", true, ReportFormat.Pdf)]
        [InlineData("Pdf", true, ReportFormat.Pdf)]
        [InlineData("garbage", false, ReportFormat.Unknown)]
        public void TryConvertFormatHandlesConversion(string? value, bool expectConvert, ReportFormat expectedFormat)
        {
            // Arrange
            var controller = new Mock<ControllerBase>();

            // Act
            var wasConverted = controller.Object.TryConvertFormat(value, out var convertedFormat);

            // Assert
            Assert.Equal(expectConvert, wasConverted);
            Assert.Equal(expectedFormat, convertedFormat);
        }

        [Theory]
        [InlineData(null, null, 0, 25)]
        [InlineData(1, 2, 1, 2)]
        [InlineData(null, -1, 0, 25)]
        [InlineData(null, 200, 0, 100)]
        public void ValidatePageArgumentsChecksArguments(int? inPage, int? inSize, int outPage, int outSize)
        {
            var controller = new Mock<ControllerBase>();
            (int page, int size) = ControllerHelpers.ValidatePageArguments(
                controller.Object, inPage, inSize);
            Assert.Equal(outPage, page);
            Assert.Equal(outSize, size);
        }
    }
}