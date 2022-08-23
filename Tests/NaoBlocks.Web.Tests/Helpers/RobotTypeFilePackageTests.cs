using NaoBlocks.Engine.Data;
using NaoBlocks.Web.Helpers;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Web.Tests.Helpers
{
    public class RobotTypeFilePackageTests
    {
        private const string fakeFileContents = "This file is not real, you can ignore it";
        private const string fileListContents = "fake.txt,ZwI4sdXTSrE1AVW7bVvWKn0IPX2CQuHWs1nqZINj03I=";

        [Fact]
        public async Task GenerateListAsyncGeneratesFile()
        {
            var workingFolder = Path.Combine(Path.GetTempPath(), "NaoBlocks-1");
            try
            {
                var packagePath = Path.Combine(workingFolder, "packages", "Mihīni");
                Directory.CreateDirectory(packagePath);
                var dataFile = Path.Combine(packagePath, "fake.txt");
                await File.WriteAllTextAsync(dataFile, fakeFileContents);
                var data = await RobotTypeFilePackage.GenerateListAsync(
                    new RobotType { Name = "Mihīni" },
                    workingFolder);
                var result = Encoding.UTF8.GetString(data);
                Assert.True(File.Exists(Path.Combine(packagePath, RobotTypeFilePackage.FileListName)));
                Assert.Equal(fileListContents, result.TrimEnd());
            }
            finally
            {
                if (Directory.Exists(workingFolder))
                {
                    Directory.Delete(workingFolder, true);
                }
            }
        }

        [Fact]
        public async Task RetrieveListAsyncHandlesExistingFile()
        {
            var workingFolder = Path.Combine(Path.GetTempPath(), "NaoBlocks-2");
            try
            {
                var packagePath = Path.Combine(workingFolder, "packages", "Mihīni");
                Directory.CreateDirectory(packagePath);
                var dataFile = Path.Combine(packagePath, RobotTypeFilePackage.FileListName);
                await File.WriteAllTextAsync(dataFile, fileListContents);
                var data = await RobotTypeFilePackage.RetrieveListAsync(
                    new RobotType { Name = "Mihīni" },
                    workingFolder);
                var result = Encoding.UTF8.GetString(data);
                Assert.Equal(fileListContents, result);
            }
            finally
            {
                if (Directory.Exists(workingFolder))
                {
                    Directory.Delete(workingFolder, true);
                }
            }
        }

        [Fact]
        public async Task RetrieveListAsyncHandlesMissingFile()
        {
            var workingFolder = Path.Combine(Path.GetTempPath(), "NaoBlocks-3");
            try
            {
                Directory.CreateDirectory(workingFolder);
                var data = await RobotTypeFilePackage.RetrieveListAsync(
                    new RobotType { Name = "Mihīni" },
                    workingFolder);
                Assert.Empty(data);
            }
            finally
            {
                if (Directory.Exists(workingFolder))
                {
                    Directory.Delete(workingFolder, true);
                }
            }
        }
    }
}