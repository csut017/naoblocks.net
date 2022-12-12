﻿using NaoBlocks.Engine.Commands;
using NaoBlocks.Engine.Data;
using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Commands
{
    public class ParseRobotsImportTests : DatabaseHelper
    {
        public ParseRobotsImportTests()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        [Fact]
        public async Task ExecuteAsyncChecksForDuplicateRobots()
        {
            // Arrange
            var package = new ExcelPackage();
            package.Workbook.Worksheets.Add("Data").GenerateData(new[]
            {
                new[] { "Machine name", "Type"},
                new[] { "Karetao", "Nao" }
            });
            var command = new ParseRobotsImport();
            AddExcelDataToCommand(package, command);
            using var store = InitialiseDatabase(
                new RobotType { Name = "Nao" },
                new Robot { MachineName = "Karetao" });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);

            // Act
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
            var result = await engine.ExecuteAsync(command);

            // Assert
            Assert.True(result.WasSuccessful, "Command failed");
            Assert.Equal(
                new[] { "Robot 'Karetao' already exists" },
                result.As<IEnumerable<ItemImport<Robot>>>().Output?.Select(r => r.Message).ToArray());
        }

        [Fact]
        public async Task ExecuteAsyncChecksMachineName()
        {
            // Arrange
            var package = new ExcelPackage();
            package.Workbook.Worksheets.Add("Data").GenerateData(new[]
            {
                new[] { "Machine name", "Type"},
                new[] { "", "Nao" }
            });
            var command = new ParseRobotsImport();
            AddExcelDataToCommand(package, command);
            using var store = InitialiseDatabase(new RobotType { Name = "Nao" });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);

            // Act
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
            var result = await engine.ExecuteAsync(command);

            // Assert
            Assert.True(result.WasSuccessful, "Command failed");
            Assert.Equal(
                new[] { "Machine name is required" },
                result.As<IEnumerable<ItemImport<Robot>>>().Output?.Select(r => r.Message).ToArray());
        }

        [Fact]
        public async Task ExecuteAsyncChecksRobotTypeExists()
        {
            // Arrange
            var package = new ExcelPackage();
            package.Workbook.Worksheets.Add("Data").GenerateData(new[]
            {
                new[] { "Machine name", "Type"},
                new[] { "Karetao", "Nao" }
            });
            var command = new ParseRobotsImport();
            AddExcelDataToCommand(package, command);
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);

            // Act
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
            var result = await engine.ExecuteAsync(command);

            // Assert
            Assert.True(result.WasSuccessful, "Command failed");
            Assert.Equal(
                new[] { "Unknown robot type 'Nao'" },
                result.As<IEnumerable<ItemImport<Robot>>>().Output?.Select(r => r.Message).ToArray());
        }

        [Fact]
        public async Task ExecuteAsyncChecksRobotTypeIsSet()
        {
            // Arrange
            var package = new ExcelPackage();
            package.Workbook.Worksheets.Add("Data").GenerateData(new[]
            {
                new[] { "Machine name", "Type"},
                new[] { "Karetao", "" }
            });
            var command = new ParseRobotsImport();
            AddExcelDataToCommand(package, command);
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);

            // Act
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
            var result = await engine.ExecuteAsync(command);

            // Assert
            Assert.True(result.WasSuccessful, "Command failed");
            Assert.Equal(
                new[] { "Robot type is required" },
                result.As<IEnumerable<ItemImport<Robot>>>().Output?.Select(r => r.Message).ToArray());
        }

        [Fact]
        public async Task ExecuteAsyncHandlesMultipleRows()
        {
            // Arrange
            var package = new ExcelPackage();
            package.Workbook.Worksheets.Add("Data").GenerateData(new[]
            {
                new[] { "name", "friendly name", "type", "password"},
                new[] { "Mihīni-1", "Karetao-1", "Nao", "One" },
                new[] { "Mihīni-2", "", "Nao", "Two" },
            });
            var command = new ParseRobotsImport();
            AddExcelDataToCommand(package, command);
            using var store = InitialiseDatabase(new RobotType { Name = "Nao" });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);

            // Act
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
            var result = await engine.ExecuteAsync(command);

            // Assert
            Assert.True(result.WasSuccessful, "Command failed");
            Assert.Equal(
                new[] {
                    "Mihīni-1,Karetao-1,Nao,One",
                    "Mihīni-2,Mihīni-2,Nao,Two",
                },
                result.As<IEnumerable<ItemImport<Robot>>>().Output?.Select(r => $"{r.Item?.MachineName},{r.Item?.FriendlyName},{r.Item?.RobotTypeId},{r.Item?.PlainPassword}").ToArray());
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
                result.As<IEnumerable<ItemImport<Robot>>>().Output?.Select(r => $"{r.Item?.MachineName},{r.Item?.FriendlyName},{r.Item?.RobotTypeId},{r.Item?.PlainPassword}").ToArray());
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
                result.As<IEnumerable<ItemImport<Robot>>>().Output?.Select(r => r.Item?.FriendlyName).ToArray());
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
                result.As<IEnumerable<ItemImport<Robot>>>().Output?.Select(r => r.Item?.MachineName).ToArray());
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
                result.As<IEnumerable<ItemImport<Robot>>>().Output?.Select(r => r.Item?.PlainPassword).ToArray());
        }

        [Fact]
        public async Task ExecuteAsyncParsesShortNames()
        {
            // Arrange
            var package = new ExcelPackage();
            package.Workbook.Worksheets.Add("Data").GenerateData(new[]
            {
                new[] { "machine", "friendly", "type", "password"},
                new[] { "Mihīni", "Karetao", "Nao", "One" },
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
                    "Mihīni,Karetao,Nao,One",
                },
                result.As<IEnumerable<ItemImport<Robot>>>().Output?.Select(r => $"{r.Item?.MachineName},{r.Item?.FriendlyName},{r.Item?.RobotTypeId},{r.Item?.PlainPassword}").ToArray());
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
                result.As<IEnumerable<ItemImport<Robot>>>().Output?.Select(r => r.Item?.RobotTypeId).ToArray());
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

        [Fact]
        public async Task ValidateAsyncHandlesNonExcel()
        {
            // Arrange
            var command = new ParseRobotsImport();
            command.Data = new MemoryStream(Encoding.UTF8.GetBytes("This is a text file"));

            var engine = new FakeEngine();

            // Act
            var errors = await engine.ValidateAsync(command);

            // Assert
            Assert.Equal(new[] { "Unable to open file: The file is not a valid Package file. If the file is encrypted, please supply the password in the constructor." }, FakeEngine.GetErrors(errors));
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