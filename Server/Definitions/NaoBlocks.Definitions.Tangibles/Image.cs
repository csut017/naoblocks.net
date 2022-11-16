namespace NaoBlocks.Definitions.Tangibles
{
    /// <summary>
    /// Defines a reusable image.
    /// </summary>
    public class ImageDefinition
    {
        /// <summary>
        /// Gets or sets the image data.
        /// </summary>
        /// <remarks>
        /// The image is a data URL containing the actual image.
        /// </remarks>
        public string? Image { get; set; }

        /// <summary>
        /// Gets or sets the name of the image.
        /// </summary>
        public string? Name { get; set; }
    }
}