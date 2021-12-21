using NaoBlocks.Engine;
using NaoBlocks.Web.Helpers;
using System;
using Xunit;

namespace NaoBlocks.Web.Tests.Helpers
{
    public class ContentTypesTests
    {
        [Theory]
        [InlineData(ReportFormat.Pdf, ContentTypes.Pdf)]
        [InlineData(ReportFormat.Excel, ContentTypes.Xlsx)]
        public void ConvertHandlesValidFormats(ReportFormat format, string expected)
        {
            Assert.Equal(expected, ContentTypes.Convert(format));
        }

        [Fact]
        public void ConvertHandlesInValidFormats()
        {
            Assert.Throws<ApplicationException>(() => ContentTypes.Convert(ReportFormat.Unknown));
        }
    }
}
