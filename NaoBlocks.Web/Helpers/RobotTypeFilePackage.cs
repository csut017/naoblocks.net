using Microsoft.AspNetCore.Http;
using NaoBlocks.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace NaoBlocks.Web.Helpers
{
    public static partial class RobotTypeFilePackage
    {
        private const string fileListName = "fileList.txt";

        public static async Task<byte[]> GenerateListAsync(RobotType robotType, string rootFolder)
        {
            if (robotType == null) throw new ArgumentNullException(nameof(robotType));
            var robotTypePath = Path.Combine(rootFolder, "packages", robotType.Name);
            if (!Directory.Exists(robotTypePath)) Directory.CreateDirectory(robotTypePath);

            var robotTypeListPath = Path.Combine(robotTypePath, fileListName);
            if (File.Exists(robotTypeListPath)) File.Delete(robotTypeListPath);
            var lines = new List<string>();
            using (var shaHash = SHA256.Create()) {
                foreach (var fileName in Directory.EnumerateFiles(robotTypePath))
                {
                    if (fileName == fileListName) continue;
                    using (var file = File.OpenRead(fileName))
                    {
                        lines.Add(Path.GetFileName(fileName) + "," + Convert.ToBase64String(shaHash.ComputeHash(file)));
                    }
                }
            }

            await File.WriteAllLinesAsync(robotTypeListPath, lines);
            return await File.ReadAllBytesAsync(robotTypeListPath);
        }

        public static async Task<byte[]> RetrieveListAsync(RobotType robotType, string rootFolder)
        {
            if (robotType == null) throw new ArgumentNullException(nameof(robotType));
            var robotTypePath = Path.Combine(rootFolder, "packages", robotType.Name);
            if (!Directory.Exists(robotTypePath)) Directory.CreateDirectory(robotTypePath);

            var robotTypeListPath = Path.Combine(robotTypePath, fileListName);
            if (File.Exists(robotTypeListPath)) return await File.ReadAllBytesAsync(robotTypeListPath);
            return Array.Empty<byte>();
        }

        public static Task<FileDetails> RetrieveFileAsync(RobotType robotType, string rootFolder, string file, string etag)
        {
            if (robotType == null) throw new ArgumentNullException(nameof(robotType));
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

        public static async Task StorePackageFile(RobotType robotType, string rootFolder, string filename, string fileData)
        {
            if (robotType == null) throw new ArgumentNullException(nameof(robotType));
            var robotTypePath = Path.Combine(rootFolder, "packages", robotType.Name);
            if (!Directory.Exists(robotTypePath)) Directory.CreateDirectory(robotTypePath);

            var packageFilename = Path.Combine(robotTypePath, filename);
            if (File.Exists(packageFilename)) File.Delete(packageFilename);
            using (var stream = File.Create(packageFilename))
            {
                using (var writer = new StreamWriter(stream))
                {
                    await writer.WriteAsync(fileData);
                }
            }
        }
    }
}
