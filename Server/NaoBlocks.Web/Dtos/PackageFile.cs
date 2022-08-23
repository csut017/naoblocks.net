namespace NaoBlocks.Web.Dtos
{
    /// <summary>
    /// Defines a DTO for a robot package file.
    /// </summary>
    public class PackageFile
    {
        /// <summary>
        /// The hascode for the file.
        /// </summary>
        public string? Hash { get; set; }

        /// <summary>
        /// The name of the file.
        /// </summary>
        public string? Name { get; set; }
    }
}