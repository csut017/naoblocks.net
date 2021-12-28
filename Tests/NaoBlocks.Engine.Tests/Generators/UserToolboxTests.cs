using NaoBlocks.Engine.Generators;
using Xunit;

namespace NaoBlocks.Engine.Tests.Generators
{
    public class UserToolboxTests
    {
        [Theory]
        [InlineData(ReportFormat.Unknown, false)]
        [InlineData(ReportFormat.Zip, false)]
        [InlineData(ReportFormat.Pdf, false)]
        [InlineData(ReportFormat.Excel, false)]
        [InlineData(ReportFormat.Text, true)]
        [InlineData(ReportFormat.Csv, false)]
        public void IsFormatAvailableChecksAllowedTypes(ReportFormat format, bool allowed)
        {
            var generator = new UserToolbox();
            Assert.Equal(allowed, generator.IsFormatAvailable(format));
        }
    }
}