using Microsoft.AspNetCore.Mvc;
using Moq;
using NaoBlocks.Engine.Data;
using NaoBlocks.Web.Helpers;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Web.Tests.Helpers
{
    public class ControllerHelpersTests
    {
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
    }
}
