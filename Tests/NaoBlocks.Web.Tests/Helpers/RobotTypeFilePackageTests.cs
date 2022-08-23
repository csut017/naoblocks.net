using NaoBlocks.Engine.Data;
using NaoBlocks.Web.Helpers;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Web.Tests.Helpers
{
    public class RobotTypeFilePackageTests
    {
        public const string FakeFileContents = "This file is not real, you can ignore it";
        public const string FileListContents = "fake.txt,ZwI4sdXTSrE1AVW7bVvWKn0IPX2CQuHWs1nqZINj03I=";

        [Fact]
        public async Task GenerateListAsyncGeneratesFile()
        {
            var workingFolder = Path.Combine(Path.GetTempPath(), "NaoBlocks-1");
            try
            {
                var packagePath = Path.Combine(workingFolder, "packages", "Mihīni");
                Directory.CreateDirectory(packagePath);
                var dataFile = Path.Combine(packagePath, "fake.txt");
                await File.WriteAllTextAsync(dataFile, FakeFileContents);
                var data = await RobotTypeFilePackage.GenerateListAsync(
                    new RobotType { Name = "Mihīni" },
                    workingFolder);
                var result = Encoding.UTF8.GetString(data);
                Assert.True(File.Exists(Path.Combine(packagePath, RobotTypeFilePackage.FileListName)));
                Assert.Equal(FileListContents, result.TrimEnd());
            }
            finally
            {
                if (Directory.Exists(workingFolder))
                {
                    Directory.Delete(workingFolder, true);
                }
            }
        }

        [Theory]
        [InlineData("123", HttpStatusCode.OK)]
        [InlineData("ZwI4sdXTSrE1AVW7bVvWKn0IPX2CQuHWs1nqZINj03I=", HttpStatusCode.NotModified)]
        public async Task RetrieveFileAsyncChecksETag(string etag, HttpStatusCode expected)
        {
            var workingFolder = Path.Combine(Path.GetTempPath(), $"NaoBlocks-12-{expected}");
            try
            {
                var fullPath = Path.Combine(workingFolder, "packages", "Mihīni");
                Directory.CreateDirectory(fullPath);
                await File.WriteAllTextAsync(Path.Combine(fullPath, "fake.txt"), FakeFileContents);
                var result = await RobotTypeFilePackage.RetrieveFileAsync(
                    new RobotType { Name = "Mihīni" },
                    workingFolder,
                    "fake.txt",
                    etag);
                Assert.Equal(expected, result.StatusCode);
                if (result.DataStream != null) result.DataStream.Close();
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
        public async Task RetrieveFileAsyncHandlesMissingFile()
        {
            var workingFolder = Path.Combine(Path.GetTempPath(), "NaoBlocks-9");
            try
            {
                Directory.CreateDirectory(Path.Combine(workingFolder, "packages", "Mihīni"));
                var result = await RobotTypeFilePackage.RetrieveFileAsync(
                    new RobotType { Name = "Mihīni" },
                    workingFolder,
                    "fake.txt",
                    string.Empty);
                Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
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
        public async Task RetrieveFileAsyncHandlesMissingFolder()
        {
            var workingFolder = Path.Combine(Path.GetTempPath(), "NaoBlocks-10");
            try
            {
                Directory.CreateDirectory(workingFolder);
                var result = await RobotTypeFilePackage.RetrieveFileAsync(
                    new RobotType { Name = "Mihīni" },
                    workingFolder,
                    "fake.txt",
                    string.Empty);
                Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
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
        public async Task RetrieveFileAsyncRetrievesFile()
        {
            var workingFolder = Path.Combine(Path.GetTempPath(), "NaoBlocks-11");
            try
            {
                var fullPath = Path.Combine(workingFolder, "packages", "Mihīni");
                Directory.CreateDirectory(fullPath);
                await File.WriteAllTextAsync(Path.Combine(fullPath, "fake.txt"), FakeFileContents);
                var result = await RobotTypeFilePackage.RetrieveFileAsync(
                    new RobotType { Name = "Mihīni" },
                    workingFolder,
                    "fake.txt",
                    string.Empty);
                Assert.Equal(HttpStatusCode.OK, result.StatusCode);
                Assert.NotNull(result.DataStream);
                using var reader = new StreamReader(result.DataStream!);
                var data = await reader.ReadToEndAsync();
                Assert.Equal(FakeFileContents, data);
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
                await File.WriteAllTextAsync(dataFile, FileListContents);
                var data = await RobotTypeFilePackage.RetrieveListAsync(
                    new RobotType { Name = "Mihīni" },
                    workingFolder);
                var result = Encoding.UTF8.GetString(data);
                Assert.Equal(FileListContents, result);
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

        [Fact]
        public async Task StorePackageFileAsyncOverwritesFile()
        {
            var workingFolder = Path.Combine(Path.GetTempPath(), "NaoBlocks-8");
            try
            {
                using var stream = new MemoryStream();
                stream.Write(Encoding.UTF8.GetBytes(FakeFileContents));
                stream.Seek(0, SeekOrigin.Begin);
                Directory.CreateDirectory(Path.Combine(workingFolder, "packages", "Mihīni"));
                var fullPath = Path.Combine(workingFolder, "packages", "Mihīni", "fake.txt");
                await File.WriteAllTextAsync(fullPath, "Overwrite this");

                await RobotTypeFilePackage.StorePackageFileAsync(
                    new RobotType { Name = "Mihīni" },
                    workingFolder,
                    "fake.txt",
                    stream);

                var data = await File.ReadAllTextAsync(fullPath);
                Assert.Equal(FakeFileContents, data);
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
        public async Task StorePackageFileAsyncSavesNewFile()
        {
            var workingFolder = Path.Combine(Path.GetTempPath(), "NaoBlocks-7");
            try
            {
                using var stream = new MemoryStream();
                stream.Write(Encoding.UTF8.GetBytes(FakeFileContents));
                stream.Seek(0, SeekOrigin.Begin);
                Directory.CreateDirectory(workingFolder);
                await RobotTypeFilePackage.StorePackageFileAsync(
                    new RobotType { Name = "Mihīni" },
                    workingFolder,
                    "fake.txt",
                    stream);

                var fullPath = Path.Combine(workingFolder, "packages", "Mihīni", "fake.txt");
                var data = await File.ReadAllTextAsync(fullPath);
                Assert.Equal(FakeFileContents, data);
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