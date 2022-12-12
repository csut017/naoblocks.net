using NaoBlocks.Engine.Commands;
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
                new[] { "Name", "Age", "Gender", "Password", "Role", "Type", "Toolbox", "View", "Allocation", "Robot"},
                new[] { "Tane", "", "Male", "One", "Student", "Nao", "Default", "Blocks", "Any", "" },
                new[] { "Wahine", "10", "Female", "Two", "Student", "Nao", "Default", "Tangibles", "Require", "Mihīni" },
                new[] { "Rangatahi", "15", "", "Three", "Teacher", "Nao", "Default", "Tangibles", "Prefer", "Mihīni" },
            });
            var command = new ParseUsersImport();
            AddExcelDataToCommand(package, command);
            using var store = InitialiseDatabase(
                new RobotType { Name = "Nao" },
                new Robot { MachineName = "Mihīni" });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);

            // Act
            var errors = await engine.ValidateAsync(command);
            Assert.Empty(errors);
            var result = await engine.ExecuteAsync(command);

            // Assert
            Assert.True(result.WasSuccessful, "Command failed");
            var actual = result.As<IEnumerable<ItemImport<User>>>()
                                .Output?
                                .Select(u => $"{u.Item?.Name},{u.Item?.StudentDetails?.Age},{u.Item?.StudentDetails?.Gender},{u.Item?.Role},{u.Item?.PlainPassword},{u.Item?.Settings.RobotType},{u.Item?.Settings.Toolbox},{u.Item?.Settings.ViewMode},{u.Item?.Settings.AllocationMode},{u.Item?.Settings.RobotId}")
                                .ToArray();
            Assert.Equal<string[]>(
                new[] {
                    "Tane,,Male,Student,One,Nao,Default,0,0,",
                    "Wahine,10,Female,Student,Two,Nao,Default,1,1,Mihīni",
                    "Rangatahi,15,,Teacher,Three,Nao,Default,1,2,Mihīni",
                },
                actual);
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
                result.As<IEnumerable<ItemImport<User>>>().Output?.Select(u => u.Item?.StudentDetails?.Age ?? 0).ToArray());
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
                result.As<IEnumerable<ItemImport<User>>>().Output?.Select(u => u.Item?.Settings.RobotId).ToArray());
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
                result.As<IEnumerable<ItemImport<User>>>().Output?.Select(u => u.Item?.Settings.AllocationMode ?? -1).ToArray());
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
                result.As<IEnumerable<ItemImport<User>>>().Output?.Select(u => u.Item?.Name).ToArray());
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
                result.As<IEnumerable<ItemImport<User>>>().Output?.Select(u => u.Item?.PlainPassword).ToArray());
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
                result.As<IEnumerable<ItemImport<User>>>().Output?.Select(u => u.Item?.Settings.RobotType).ToArray());
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
                result.As<IEnumerable<ItemImport<User>>>().Output?.Select(u => u.Item?.Role.ToString()).ToArray());
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
                result.As<IEnumerable<ItemImport<User>>>().Output?.Select(u => u.Item?.Settings.Toolbox).ToArray());
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
                result.As<IEnumerable<ItemImport<User>>>().Output?.Select(u => u.Item?.StudentDetails?.Gender ?? string.Empty).ToArray());
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
                result.As<IEnumerable<ItemImport<User>>>().Output?.Select(u => u.Item?.Settings.ViewMode ?? -1).ToArray());
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
                new[] { "Name", "Type"},
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
        public async Task ValidateAsyncChecksRobotExists()
        {
            // Arrange
            var package = new ExcelPackage();
            package.Workbook.Worksheets.Add("Data").GenerateData(new[]
            {
                new[] { "Name", "Type", "Robot"},
                new[] { "Tane", "Nao", "Mihīni" },
            });
            var command = new ParseUsersImport();
            AddExcelDataToCommand(package, command);
            using var store = InitialiseDatabase(new RobotType { Name = "Nao" });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);

            // Act
            var errors = await engine.ValidateAsync(command);

            // Assert
            Assert.Equal(new[] { "Unknown robot 'Mihīni' [row 2]" }, FakeEngine.GetErrors(errors));
        }

        [Fact]
        public async Task ValidateAsyncChecksRobotExistsHandlesFriendlyName()
        {
            // Arrange
            var package = new ExcelPackage();
            package.Workbook.Worksheets.Add("Data").GenerateData(new[]
            {
                new[] { "Name", "Type", "Robot"},
                new[] { "Tane", "Nao", "Mihīni" },
            });
            var command = new ParseUsersImport();
            AddExcelDataToCommand(package, command);
            using var store = InitialiseDatabase(
                new RobotType { Name = "Nao" },
                new Robot { MachineName = "Karetao", FriendlyName = "Mihīni" });
            using var session = store.OpenAsyncSession();
            var engine = new FakeEngine(session);

            // Act
            var errors = await engine.ValidateAsync(command);

            // Assert
            Assert.Empty(errors);
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

        [Fact]
        public async Task ValidateAsyncHandlesNonExcel()
        {
            // Arrange
            var command = new ParseUsersImport();
            command.Data = new MemoryStream(Encoding.UTF8.GetBytes("This is a text file"));

            var engine = new FakeEngine();

            // Act
            var errors = await engine.ValidateAsync(command);

            // Assert
            Assert.Equal(new[] { "Unable to open file: The file is not a valid Package file. If the file is encrypted, please supply the password in the constructor." }, FakeEngine.GetErrors(errors));
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