using NaoBlocks.Engine.Data;
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
    }
}