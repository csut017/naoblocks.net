using NaoBlocks.Engine.Generators;
using Xunit;

namespace NaoBlocks.Engine.Tests.Generators
{
    public class SnapshotsListsTests
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
            var generator = new SnapshotsLists();
            Assert.Equal(allowed, generator.IsFormatAvailable(format));
        }
    }
}