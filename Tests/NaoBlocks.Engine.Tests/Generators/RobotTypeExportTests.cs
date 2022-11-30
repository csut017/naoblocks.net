using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Generators;
using NaoBlocks.Engine.Indices;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Generators
{
    public class RobotTypeExportTests
        : DatabaseHelper
    {
        [Fact]
        public async Task GenerateAsyncGeneratesReport()
        {
            // Arrange
            var robotType = new RobotType { Id = "types/1", Name = "Nao" };
            robotType.Toolboxes.Add(new Toolbox { Name = "Default", IsDefault = true });
            robotType.Toolboxes[0].Categories.Add(new ToolboxCategory { Name = "Category #1" });
            robotType.Toolboxes[0].Categories.Add(new ToolboxCategory { Name = "Category #2" });
            robotType.Toolboxes.Add(new Toolbox { Name = "Secondary", IsDefault = false });
            robotType.CustomValues.Add(new NamedValue { Name = "One", Value = "Tahi" });
            robotType.CustomValues.Add(new NamedValue { Name = "Two", Value = "Rua" });
            var robot = new Robot { MachineName = "karetao", FriendlyName = "Mihīni", Id = "robots/1", RobotTypeId = robotType.Id, IsInitialised = true };
            using var store = InitialiseDatabase(
                robot);
            await this.InitialiseIndicesAsync(store, new RobotLogByRobotTypeId());
            using var session = store.OpenAsyncSession();
            var generator = InitialiseGenerator<RobotTypeExport>(session);

            // Act
            var output = await generator.GenerateAsync(ReportFormat.Text, robotType);

            // Assert
            Assert.Equal("Robot-Type-Export-Nao.txt", output.Item2);
            using var reader = new StreamReader(output.Item1);
            var text = await reader.ReadToEndAsync();
            Assert.Equal(
                    @$"Details
=======
Name: Nao
Is Default: False
Toolboxes
=========
Name,Is Default,Uses Events,Categories,Definition
Default,Yes,No,""Category #1,Category #2"",<toolbox><category name=""Category #1"" colour=""0"" optional=""no"" /><category name=""Category #2"" colour=""0"" optional=""no"" /></toolbox>
Secondary,No,No,,<toolbox />
Values
======
Name,Default Value
One,Tahi
Two,Rua
Robots
======
Machine Name,Friendly Name,When Added,Initialized
karetao,Mihīni,0001-01-01,Yes
",
                text);
        }

        [Theory]
        [ReportFormatData(ReportFormat.Excel, ReportFormat.Csv, ReportFormat.Text, ReportFormat.Xml, ReportFormat.Pdf)]
        public void IsFormatAvailableChecksAllowedTypes(ReportFormat format, bool allowed)
        {
            var generator = new RobotTypeExport();
            Assert.Equal(allowed, generator.IsFormatAvailable(format));
        }
    }
}