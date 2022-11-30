using NaoBlocks.Engine.Generators;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Generators
{
    public class GeneratorTests
    {
        [Fact]
        public async Task GeneratesCsv()
        {
            // Arrange
            var generator = BuildDocument();

            // Act
            var (stream, _) = await generator.GenerateAsync(ReportFormat.Csv, "don't care");

            // Assert
            using var reader = new StreamReader(stream);
            var text = await reader.ReadToEndAsync();
            Assert.Equal(
                @"Heading,Content for page 1
Column 1,Column 2
Cell 1,Cell 2
",
                text);
        }

        [Fact]
        public async Task GeneratesExcel()
        {
            // Arrange
            var generator = BuildDocument();

            // Act
            var (stream, _) = await generator.GenerateAsync(ReportFormat.Excel, "don't care");

            // Assert
            Assert.NotEqual(0, stream.Length);
        }

        [Theory]
        [InlineData(ReportFormat.Unknown)]
        [InlineData(ReportFormat.Xml)]
        [InlineData(ReportFormat.Zip)]
        public async Task GeneratesFails(ReportFormat format)
        {
            // Arrange
            var generator = BuildDocument();

            // Act
            var ex = await Assert.ThrowsAsync<ApplicationException>(async () => await generator.GenerateAsync(format, "don't care"));

            // Assert
            Assert.Equal(
                $"Unable to generate report: format '{format}' has not been implemented",
                ex.Message);
        }

        [Fact]
        public async Task GeneratesPdf()
        {
            // Arrange
            var generator = BuildDocument();

            // Act
            var (stream, _) = await generator.GenerateAsync(ReportFormat.Pdf, "don't care");

            // Assert
            Assert.NotEqual(0, stream.Length);
        }

        [Fact]
        public async Task GeneratesText()
        {
            // Arrange
            var generator = BuildDocument();

            // Act
            var (stream, _) = await generator.GenerateAsync(ReportFormat.Text, "don't care");

            // Assert
            using var reader = new StreamReader(stream);
            var text = await reader.ReadToEndAsync();
            Assert.Equal(
                @"Page #1
=======
Heading: Content for page 1
Table #1
========
Column 1,Column 2
Cell 1,Cell 2
",
                text);
        }

        private static Generator BuildDocument()
        {
            var generator = new Generator
            {
                IsLandScape = false,
                Title = "Test Document"
            };
            var page = generator.AddPage("Page #1");
            page.AddParagraph(
                new PageBlock("Heading", true),
                new PageBlock("Content for page 1"));
            var table = generator.AddTable("Table #1");
            table.AddRow(TableRowType.Header, "Column 1", "Column 2");
            table.AddRow(TableRowType.Data, "Cell 1", "Cell 2");
            return generator;
        }
    }
}