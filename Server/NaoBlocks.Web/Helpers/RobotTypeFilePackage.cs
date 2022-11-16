using NaoBlocks.Engine.Data;
using System.Net;
using System.Security.Cryptography;

namespace NaoBlocks.Web.Helpers
{
    /// <summary>
    /// Helper class for retrieving files for a robot.
    /// </summary>
    public static class RobotTypeFilePackage
    {
        /// <summary>
        /// The file list filename.
        /// </summary>
        public const string FileListName = "fileList.txt";

        /// <summary>
        /// Generates the package list list for a robot.
        /// </summary>
        /// <param name="robotType">The robot type.</param>
        /// <param name="rootFolder">The root folder for the web site.</param>
        /// <returns>An array of bytes containing the data.</returns>
        public static async Task<byte[]> GenerateListAsync(RobotType robotType, string rootFolder)
        {
            var robotTypePath = Path.Combine(rootFolder, "packages", robotType.Name);
            if (!Directory.Exists(robotTypePath)) Directory.CreateDirectory(robotTypePath);

            var robotTypeListPath = Path.Combine(robotTypePath, FileListName);
            if (File.Exists(robotTypeListPath)) File.Delete(robotTypeListPath);
            var lines = new List<string>();
            using (var shaHash = SHA256.Create())
            {
                foreach (var fileName in Directory.EnumerateFiles(robotTypePath))
                {
                    if (fileName == FileListName) continue;
                    using (var file = File.OpenRead(fileName))
                    {
                        lines.Add(Path.GetFileName(fileName) + "," + Convert.ToBase64String(shaHash.ComputeHash(file)));
                    }
                }
            }

            await File.WriteAllLinesAsync(robotTypeListPath, lines);
            return await File.ReadAllBytesAsync(robotTypeListPath);
        }

        /// <summary>
        /// Attempts to retrieve a file for a robot type.
        /// </summary>
        /// <param name="robotType">The robot type.</param>
        /// <param name="rootFolder">The root folder for the web server.</param>
        /// <param name="file">The name of the file to retrieve.</param>
        /// <param name="etag">The etag for the last retrieved version of the file.</param>
        /// <returns>A <see cref="FileDetails"/> containing the result.</returns>
        public static Task<FileDetails> RetrieveFileAsync(RobotType robotType, string rootFolder, string file, string etag)
        {
            var robotTypePath = Path.Combine(rootFolder, "packages", robotType.Name);
            if (!Directory.Exists(robotTypePath)) return Task.FromResult(new FileDetails { StatusCode = HttpStatusCode.NotFound });

            var filePath = Path.Combine(robotTypePath, file);
            if (!File.Exists(filePath)) return Task.FromResult(new FileDetails { StatusCode = HttpStatusCode.NotFound });

            if (!string.IsNullOrEmpty(etag))
            {
                using (var shaHash = SHA256.Create())
                {
                    using (var fileToCheck = File.OpenRead(filePath))
                    {
                        var fileHash = Convert.ToBase64String(shaHash.ComputeHash(fileToCheck));
                        if (fileHash.Equals(etag, StringComparison.Ordinal))
                        {
                            return Task.FromResult(new FileDetails { StatusCode = HttpStatusCode.NotModified });
                        }

                        fileToCheck.Close();
                    }
                }
            }

            var details = new FileDetails
            {
                StatusCode = HttpStatusCode.OK,
                DataStream = File.OpenRead(filePath)
            };
            return Task.FromResult(details);
        }

        /// <summary>
        /// Retrieves the list of lists to download to the robot.
        /// </summary>
        /// <param name="robotType">The robot type.</param>
        /// <param name="rootFolder">The root folder for the web site.</param>
        /// <returns>An array of bytes containing the data.</returns>
        public static async Task<byte[]> RetrieveListAsync(RobotType robotType, string rootFolder)
        {
            var robotTypePath = Path.Combine(rootFolder, "packages", robotType.Name);
            if (!Directory.Exists(robotTypePath)) Directory.CreateDirectory(robotTypePath);

            var robotTypeListPath = Path.Combine(robotTypePath, FileListName);
            if (File.Exists(robotTypeListPath)) return await File.ReadAllBytesAsync(robotTypeListPath);
            return Array.Empty<byte>();
        }

        /// <summary>
        /// Stores a package file.
        /// </summary>
        /// <param name="robotType">The robot type.</param>
        /// <param name="rootFolder">The root folder for the web server.</param>
        /// <param name="file">The name of the file to retrieve.</param>
        /// <param name="fileData">A <see cref="Stream"/> containing the data.</param>
        public static async Task StorePackageFileAsync(RobotType robotType, string rootFolder, string file, Stream fileData)
        {
            var robotTypePath = Path.Combine(rootFolder, "packages", robotType.Name);
            if (!Directory.Exists(robotTypePath)) Directory.CreateDirectory(robotTypePath);

            var packageFilename = Path.Combine(robotTypePath, file);
            if (File.Exists(packageFilename)) File.Delete(packageFilename);
            using var stream = File.Create(packageFilename);
            await fileData.CopyToAsync(stream);
        }

        /// <summary>
        /// The details on a retrieved file.
        /// </summary>
        public struct FileDetails
        {
            /// <summary>
            /// Gets or sets the data stream.
            /// </summary>
            public Stream? DataStream { get; set; }

            /// <summary>
            /// Gets or sets the status code to return.
            /// </summary>
            public HttpStatusCode StatusCode { get; set; }
        }
    }
}