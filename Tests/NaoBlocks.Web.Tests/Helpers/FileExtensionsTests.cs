using NaoBlocks.Engine;
using NaoBlocks.Web.Helpers;
using System;
using Xunit;

namespace NaoBlocks.Web.Tests.Helpers
{
    public class FileExtensionsTests
    {
        [Fact]
        public void FromReportFormatHandlesInValidFormats()
        {
            Assert.Throws<ApplicationException>(() => FileExtensions.FromReportFormat(ReportFormat.Unknown));
        }

        [Theory]
        [InlineData(ReportFormat.Pdf, "pdf")]
        [InlineData(ReportFormat.Excel, "xlsx")]
        [InlineData(ReportFormat.Zip, "zip")]
        [InlineData(ReportFormat.Text, "txt")]
        [InlineData(ReportFormat.Csv, "csv")]
        [InlineData(ReportFormat.Xml, "xml")]
        public void FromReportFormatHandlesValidFormats(ReportFormat format, string expected)
        {
            Assert.Equal(expected, FileExtensions.FromReportFormat(format));
        }

        [Fact]
        public void ToReportFormatHandlesInValidFormats()
        {
            Assert.Throws<ApplicationException>(() => FileExtensions.ToReportFormat("unknown"));
        }

        [Theory]
        [InlineData("pdf", ReportFormat.Pdf)]
        [InlineData("xlsx", ReportFormat.Excel)]
        [InlineData("zip", ReportFormat.Zip)]
        [InlineData("txt", ReportFormat.Text)]
        [InlineData("csv", ReportFormat.Csv)]
        [InlineData("xml", ReportFormat.Xml)]
        [InlineData(".xml", ReportFormat.Xml)]
        [InlineData("XML", ReportFormat.Xml)]
        public void ToReportFormatHandlesValidFormats(string extension, ReportFormat expected)
        {
            Assert.Equal(expected, FileExtensions.ToReportFormat(extension));
        }
    }
}