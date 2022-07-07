using NaoBlocks.Engine.Generators;
using Xunit;

namespace NaoBlocks.Engine.Tests.Generators
{
    public class StudentExportTests
    {
        [Theory]
        [ReportFormatData(ReportFormat.Excel, ReportFormat.Pdf, ReportFormat.Text, ReportFormat.Csv)]
        public void IsFormatAvailableChecksAllowedTypes(ReportFormat format, bool allowed)
        {
            var generator = new StudentExport();
            Assert.Equal(allowed, generator.IsFormatAvailable(format));
        }
    }
}