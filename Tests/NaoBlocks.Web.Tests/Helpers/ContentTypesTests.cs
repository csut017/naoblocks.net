using NaoBlocks.Engine;
using NaoBlocks.Web.Helpers;
using System;
using Xunit;

namespace NaoBlocks.Web.Tests.Helpers
{
    public class ContentTypesTests
    {
        [Fact]
        public void ConvertHandlesInValidFormats()
        {
            Assert.Throws<ApplicationException>(() => ContentTypes.Convert(ReportFormat.Unknown));
        }

        [Theory]
        [InlineData(ReportFormat.Pdf, ContentTypes.Pdf)]
        [InlineData(ReportFormat.Excel, ContentTypes.Xlsx)]
        [InlineData(ReportFormat.Zip, ContentTypes.Zip)]
        public void ConvertHandlesValidFormats(ReportFormat format, string expected)
        {
            Assert.Equal(expected, ContentTypes.Convert(format));
        }
    }
}