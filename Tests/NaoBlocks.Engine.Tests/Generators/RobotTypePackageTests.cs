using NaoBlocks.Engine.Generators;
using Xunit;

namespace NaoBlocks.Engine.Tests.Generators
{
    public class RobotTypePackageTests
    {
        [Theory]
        [InlineData(ReportFormat.Unknown, false)]
        [InlineData(ReportFormat.Zip, true)]
        [InlineData(ReportFormat.Pdf, false)]
        [InlineData(ReportFormat.Text, false)]
        [InlineData(ReportFormat.Excel, false)]
        [InlineData(ReportFormat.Csv, false)]
        public void IsFormatAvailableChecksAllowedTypes(ReportFormat format, bool allowed)
        {
            var generator = new RobotTypePackage();
            Assert.Equal(allowed, generator.IsFormatAvailable(format));
        }
    }
}