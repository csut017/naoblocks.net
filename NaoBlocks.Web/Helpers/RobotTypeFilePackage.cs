using NaoBlocks.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace NaoBlocks.Web.Helpers
{
    public static class RobotTypeFilePackage
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
    }
}
