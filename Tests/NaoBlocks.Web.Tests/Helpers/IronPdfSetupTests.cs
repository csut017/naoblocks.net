using Microsoft.AspNetCore.Builder;
using NaoBlocks.Web.Helpers;
using System.Linq;
using Xunit;

namespace NaoBlocks.Web.Tests.Helpers
{
    public class IronPdfSetupTests
    {
        [Fact]
        public void InitialiseHandlesInvalidLicense()
        {
            // Arrange
            var builder = WebApplication.CreateBuilder();
            builder.Configuration["ironPdf:license"] = "invalid key";
            var logger = new FakeLogger<string>();

            // Act
            IronPdfSetup.Initialise(builder, logger);

            // Assert
            Assert.Equal(
                new[]
                {
                    "INFORMATION: Initialising IronPDF",
                    "WARNING: Unable to initialise IronPDF: License key is invalid"
                },
                logger.Messages.ToArray());
        }

        [Fact]
        public void InitialiseHandlesMissingLicense()
        {
            // Arrange
            var builder = WebApplication.CreateBuilder();
            var logger = new FakeLogger<string>();

            // Act
            IronPdfSetup.Initialise(builder, logger);

            // Assert
            Assert.Equal(
                new[]
                {
                    "WARNING: Unable to initialise IronPDF: License key not set"
                },
                logger.Messages.ToArray());
        }
    }
}