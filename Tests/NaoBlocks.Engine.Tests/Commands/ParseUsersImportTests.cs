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
    public class ParseUsersImportTests : DatabaseHelper
    {
        public ParseUsersImportTests()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
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
            var command = new ParseUsersImport();
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
            //Assert.Equal(
            //    new[] {
            //        "Mihīni-1,Karetao-1,Nao,One",
            //        "Mihīni-2,Mihīni-2,Nao,Two",
            //    },
            //    result.As<IEnumerable<User>>().Output?.Select(r => $"{r.MachineName},{r.FriendlyName},{r.RobotTypeId},{r.PlainPassword}").ToArray());
        }

        [Fact]
        public async Task ExecuteAsyncParsesAge()
        {
            // Arrange
            var package = new ExcelPackage();
            package.Workbook.Worksheets.Add("Data").GenerateData(new[]
            {
                new[] { "Age"},
                new[] { "5" }
            });
            var command = new ParseUsersImport
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
                new[] { 5 },
                result.As<IEnumerable<User>>().Output?.Select(r => r.StudentDetails?.Age ?? 0).ToArray());
        }

        [Fact]
        public async Task ExecuteAsyncParsesAllocatedRobot()
        {
            // Arrange
            var package = new ExcelPackage();
            package.Workbook.Worksheets.Add("Data").GenerateData(new[]
            {
                new[] { "Allocated Robot"},
                new[] { "Whero" }
            });
            var command = new ParseUsersImport
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
                new[] { "Whero" },
                result.As<IEnumerable<User>>().Output?.Select(r => r.Settings.RobotId).ToArray());
        }

        [Fact]
        public async Task ExecuteAsyncParsesAllocationMode()
        {
            // Arrange
            var package = new ExcelPackage();
            package.Workbook.Worksheets.Add("Data").GenerateData(new[]
            {
                new[] { "Allocation Mode"},
                new[] { "Any" }
            });
            var command = new ParseUsersImport
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
                new[] { 0 },
                result.As<IEnumerable<User>>().Output?.Select(r => r.Settings.AllocationMode).ToArray());
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
            var command = new ParseUsersImport
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
            //Assert.Equal(
            //    new[] {
            //        "Mihīni,Karetao-1,Nao,One",
            //        "Mihīni,Karetao-2,Nao,Two",
            //    },
            //    result.As<IEnumerable<User>>().Output?.Select(r => $"{r.MachineName},{r.FriendlyName},{r.RobotTypeId},{r.PlainPassword}").ToArray());
        }

        [Fact]
        public async Task ExecuteAsyncParsesName()
        {
            // Arrange
            var package = new ExcelPackage();
            package.Workbook.Worksheets.Add("Data").GenerateData(new[]
            {
                new[] { "Name"},
                new[] { "Tahi" }
            });
            var command = new ParseUsersImport
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
                new[] { "Tahi" },
                result.As<IEnumerable<User>>().Output?.Select(r => r.Name).ToArray());
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
            var command = new ParseUsersImport
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
                result.As<IEnumerable<User>>().Output?.Select(r => r.PlainPassword).ToArray());
        }

        [Fact]
        public async Task ExecuteAsyncParsesRobotType()
        {
            // Arrange
            var package = new ExcelPackage();
            package.Workbook.Worksheets.Add("Data").GenerateData(new[]
            {
                new[] { "Robot Type"},
                new[] { "One" }
            });
            var command = new ParseUsersImport
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
                result.As<IEnumerable<User>>().Output?.Select(r => r.Settings.RobotType).ToArray());
        }

        [Fact]
        public async Task ExecuteAsyncParsesRole()
        {
            // Arrange
            var package = new ExcelPackage();
            package.Workbook.Worksheets.Add("Data").GenerateData(new[]
            {
                new[] { "Role"},
                new[] { "Student" }
            });
            var command = new ParseUsersImport
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
                new[] { "Student" },
                result.As<IEnumerable<User>>().Output?.Select(r => r.Role.ToString()).ToArray());
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
            var command = new ParseUsersImport
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
            //Assert.Equal(
            //    new[] {
            //        "Mihīni,Karetao,Nao,One",
            //    },
            //    result.As<IEnumerable<User>>().Output?.Select(r => $"{r.MachineName},{r.FriendlyName},{r.RobotTypeId},{r.PlainPassword}").ToArray());
        }

        [Fact]
        public async Task ExecuteAsyncParsesToolbox()
        {
            // Arrange
            var package = new ExcelPackage();
            package.Workbook.Worksheets.Add("Data").GenerateData(new[]
            {
                new[] { "Toolbox"},
                new[] { "One" }
            });
            var command = new ParseUsersImport
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
                result.As<IEnumerable<User>>().Output?.Select(r => r.Settings.Toolbox).ToArray());
        }

        [Fact]
        public async Task ExecuteAsyncParsesType()
        {
            // Arrange
            var package = new ExcelPackage();
            package.Workbook.Worksheets.Add("Data").GenerateData(new[]
            {
                new[] { "Gender"},
                new[] { "Tane" }
            });
            var command = new ParseUsersImport
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
                new[] { "Tane" },
                result.As<IEnumerable<User>>().Output?.Select(r => r.StudentDetails?.Gender ?? string.Empty).ToArray());
        }

        [Fact]
        public async Task ExecuteAsyncParsesViewMode()
        {
            // Arrange
            var package = new ExcelPackage();
            package.Workbook.Worksheets.Add("Data").GenerateData(new[]
            {
                new[] { "View Mode"},
                new[] { "Tangibles" }
            });
            var command = new ParseUsersImport
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
                new[] { 1 },
                result.As<IEnumerable<User>>().Output?.Select(r => r.Settings.ViewMode).ToArray());
        }

        [Fact]
        public async Task ValidateAsyncCheckForEmptyExcelFile()
        {
            // Arrange
            var package = new ExcelPackage();
            package.Workbook.Worksheets.Add("Data");
            var command = new ParseUsersImport();
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
            var command = new ParseUsersImport();
            var engine = new FakeEngine();

            // Act
            var errors = await engine.ValidateAsync(command);

            // Assert
            Assert.Equal(new[] { "Data is required" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidateAsyncChecksForDuplicateUsers()
        {
            // Arrange
            var package = new ExcelPackage();
            package.Workbook.Worksheets.Add("Data").GenerateData(new[]
            {
                new[] { "Name", "Robot"},
                new[] { "Tane", "Nao" }
            });
            var command = new ParseUsersImport();
            AddExcelDataToCommand(package, command);
            using var store = InitialiseDatabase(
                new RobotType { Name = "Nao" },
                new User { Name = "Tane" });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);

            // Act
            var errors = await engine.ValidateAsync(command);

            // Assert
            Assert.Equal(new[] { "User 'Tane' already exists [row 2]" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidateAsyncChecksName()
        {
            // Arrange
            var package = new ExcelPackage();
            package.Workbook.Worksheets.Add("Data").GenerateData(new[]
            {
                new[] { "Name", "Type"},
                new[] { "", "Nao" }
            });
            var command = new ParseUsersImport();
            AddExcelDataToCommand(package, command);
            using var store = InitialiseDatabase(new RobotType { Name = "Nao" });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);

            // Act
            var errors = await engine.ValidateAsync(command);

            // Assert
            Assert.Equal(new[] { "Name is required [row 2]" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidateAsyncChecksRobotTypeExists()
        {
            // Arrange
            var package = new ExcelPackage();
            package.Workbook.Worksheets.Add("Data").GenerateData(new[]
            {
                new[] { "Name", "Type"},
                new[] { "Tane", "Nao" }
            });
            var command = new ParseUsersImport();
            AddExcelDataToCommand(package, command);
            using var store = InitialiseDatabase();
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);

            // Act
            var errors = await engine.ValidateAsync(command);

            // Assert
            Assert.Equal(new[] { "Unknown robot type 'Nao' [row 2]" }, FakeEngine.GetErrors(errors));
        }

        private static void AddExcelDataToCommand(ExcelPackage package, ParseUsersImport command)
        {
            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Seek(0, SeekOrigin.Begin);
            command.Data = stream;
        }
    }
}