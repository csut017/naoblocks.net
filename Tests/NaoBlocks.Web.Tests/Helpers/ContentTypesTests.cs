using NaoBlocks.Engine;
using NaoBlocks.Web.Helpers;
using System;
using Xunit;

namespace NaoBlocks.Web.Tests.Helpers
{
    public class ContentTypesTests
    {
        [Fact]
        public void FromReportFormatHandlesInValidFormats()
        {
            Assert.Throws<ApplicationException>(() => ContentTypes.FromReportFormat(ReportFormat.Unknown));
        }

        [Theory]
        [InlineData(ReportFormat.Pdf, "application/pdf")]
        [InlineData(ReportFormat.Excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
        [InlineData(ReportFormat.Zip, "application/zip")]
        [InlineData(ReportFormat.Text, "text/plain")]
        [InlineData(ReportFormat.Csv, "text/csv")]
        [InlineData(ReportFormat.Xml, "application/xml")]
        public void FromReportFormatHandlesValidFormats(ReportFormat format, string expected)
        {
            Assert.Equal(expected, ContentTypes.FromReportFormat(format));
        }
    }
}