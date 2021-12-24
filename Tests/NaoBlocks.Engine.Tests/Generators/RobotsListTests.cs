using NaoBlocks.Engine.Generators;
using Xunit;

namespace NaoBlocks.Engine.Tests.Generators
{
    public class RobotsListTests
    {
        [Theory]
        [InlineData(ReportFormat.Unknown, false)]
        [InlineData(ReportFormat.Zip, false)]
        [InlineData(ReportFormat.Pdf, true)]
        [InlineData(ReportFormat.Text, true)]
        [InlineData(ReportFormat.Excel, true)]
        [InlineData(ReportFormat.Csv, true)]
        public void IsFormatAvailableChecksAllowedTypes(ReportFormat format, bool allowed)
        {
            var generator = new RobotsList();
            Assert.Equal(allowed, generator.IsFormatAvailable(format));
        }
    }
}