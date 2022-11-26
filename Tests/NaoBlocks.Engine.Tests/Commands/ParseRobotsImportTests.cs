using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Data;
using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Commands
{
    public class ParseRobotsImportTests
    {
        public ParseRobotsImportTests()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        [Fact]
        public async Task ExecuteAsyncParsesAllValues()
        {
            // Arrange
            var package = new ExcelPackage();
            package.Workbook.Worksheets.Add("Data").GenerateData(new[]
            {
                new[] { "machine name", "friendly name", "type", "password"},
                new[] { "Mihīni", "Karetao-1", "Nao", "One" },
                new[] { "Mihīni", "Karetao-2", "Nao", "Two" },
            });
            var command = new ParseRobotsImport
            {
                SkipValidation = true
            };
            AddExcelDataToCommand(package, command);
            var engine = new FakeEngine();

            // Act
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
            var result = await engine.ExecuteAsync(command);

            // Assert
            Assert.True(result.WasSuccessful, "Command failed");
            Assert.Equal(
                new[] {
                    "Mihīni,Karetao-1,Nao,One",
                    "Mihīni,Karetao-2,Nao,Two",
                },
                result.As<IEnumerable<Robot>>().Output?.Select(r => $"{r.MachineName},{r.FriendlyName},{r.RobotTypeId},{r.PlainPassword}").ToArray());
        }

        [Fact]
        public async Task ExecuteAsyncParsesFriendlyName()
        {
            // Arrange
            var package = new ExcelPackage();
            package.Workbook.Worksheets.Add("Data").GenerateData(new[]
            {
                new[] { "Friendly name"},
                new[] { "Mihīni" }
            });
            var command = new ParseRobotsImport
            {
                SkipValidation = true
            };
            AddExcelDataToCommand(package, command);
            var engine = new FakeEngine();

            // Act
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
            var result = await engine.ExecuteAsync(command);

            // Assert
            Assert.True(result.WasSuccessful, "Command failed");
            Assert.Equal(
                new[] { "Mihīni" },
                result.As<IEnumerable<Robot>>().Output?.Select(r => r.FriendlyName).ToArray());
        }

        [Fact]
        public async Task ExecuteAsyncParsesMachineName()
        {
            // Arrange
            var package = new ExcelPackage();
            package.Workbook.Worksheets.Add("Data").GenerateData(new[]
            {
                new[] { "Machine name"},
                new[] { "Mihīni" }
            });
            var command = new ParseRobotsImport
            {
                SkipValidation = true
            };
            AddExcelDataToCommand(package, command);
            var engine = new FakeEngine();

            // Act
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
            var result = await engine.ExecuteAsync(command);

            // Assert
            Assert.True(result.WasSuccessful, "Command failed");
            Assert.Equal(
                new[] { "Mihīni" },
                result.As<IEnumerable<Robot>>().Output?.Select(r => r.MachineName).ToArray());
        }

        [Fact]
        public async Task ExecuteAsyncParsesPassword()
        {
            // Arrange
            var package = new ExcelPackage();
            package.Workbook.Worksheets.Add("Data").GenerateData(new[]
            {
                new[] { "Password"},
                new[] { "One" }
            });
            var command = new ParseRobotsImport
            {
                SkipValidation = true
            };
            AddExcelDataToCommand(package, command);
            var engine = new FakeEngine();

            // Act
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
            var result = await engine.ExecuteAsync(command);

            // Assert
            Assert.True(result.WasSuccessful, "Command failed");
            Assert.Equal(
                new[] { "One" },
                result.As<IEnumerable<Robot>>().Output?.Select(r => r.PlainPassword).ToArray());
        }

        [Fact]
        public async Task ExecuteAsyncParsesType()
        {
            // Arrange
            var package = new ExcelPackage();
            package.Workbook.Worksheets.Add("Data").GenerateData(new[]
            {
                new[] { "Type"},
                new[] { "Mihīni" }
            });
            var command = new ParseRobotsImport
            {
                SkipValidation = true
            };
            AddExcelDataToCommand(package, command);
            var engine = new FakeEngine();

            // Act
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
            var result = await engine.ExecuteAsync(command);

            // Assert
            Assert.True(result.WasSuccessful, "Command failed");
            Assert.Equal(
                new[] { "Mihīni" },
                result.As<IEnumerable<Robot>>().Output?.Select(r => r.RobotTypeId).ToArray());
        }

        [Fact]
        public async Task ValidateAsyncCheckForEmptyExcelFile()
        {
            // Arrange
            var package = new ExcelPackage();
            package.Workbook.Worksheets.Add("Data");
            var command = new ParseRobotsImport();
            AddExcelDataToCommand(package, command);
            var engine = new FakeEngine();

            // Act
            var errors = await engine.ValidateAsync(command);

            // Assert
            Assert.Equal(new[] { "Unable to parse Excel workbook: No data" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidateAsyncChecksForData()
        {
            // Arrange
            var command = new ParseRobotsImport();
            var engine = new FakeEngine();

            // Act
            var errors = await engine.ValidateAsync(command);

            // Assert
            Assert.Equal(new[] { "Data is required" }, FakeEngine.GetErrors(errors));
        }

        private static void AddExcelDataToCommand(ExcelPackage package, ParseRobotsImport command)
        {
            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Seek(0, SeekOrigin.Begin);
            command.Data = stream;
        }
    }
}