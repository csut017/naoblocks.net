using NaoBlocks.Engine.Generators;
using Xunit;

namespace NaoBlocks.Engine.Tests.Generators
{
    public class RobotTypePackageTests
    {
        [Theory]
        [ReportFormatData(ReportFormat.Zip)]
        public void IsFormatAvailableChecksAllowedTypes(ReportFormat format, bool allowed)
        {
            var generator = new RobotTypePackage();
            Assert.Equal(allowed, generator.IsFormatAvailable(format));
        }
    }
}