namespace NaoBlocks.Definitions.Tangibles
{
    /// <summary>
    /// Defines a programmable block in the UI.
    /// </summary>
    public class Block
    {
        /// <summary>
        /// Gets or sets the NaoLang generator code for the block.
        /// </summary>
        /// <remarks>
        /// The generator code is used to convert the tangible to NaoLang. The JavaScript will be directly downloaded into the client UI.
        /// </remarks>
        public string? Generator { get; set; }

        /// <summary>
        /// Gets or sets the image of the block.
        /// </summary>
        /// <remarks>
        /// The image is a data URL containing the image of the block.
        /// </remarks>
        public string? Image { get; set; }

        /// <summary>
        /// Gets or sets the name of the block.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets the TopCodes numbers.
        /// </summary>
        /// <remarks>
        /// Each block must have one or more TopCodes numbers.
        /// </remarks>
        public IList<int> Numbers { get; private set; } = new List<int>();

        /// <summary>
        /// Gets or sets the text of the block.
        /// </summary>
        public string? Text { get; set; }

        /// <summary>
        /// Generates a new <see cref="Block"/> definition.
        /// </summary>
        /// <param name="number">The top code number.</param>
        /// <param name="name">The name of the block.</param>
        /// <param name="image">A data URL containing the image.</param>
        /// <param name="generator">The generator to use.</param>
        /// <returns>The new <see cref="Block"/> definition.</returns>
        public static Block New(int number, string? name, string? image, string? generator)
        {
            return New(new[] { number }, name, image, generator);
        }

        /// <summary>
        /// Generates a new <see cref="Block"/> definition.
        /// </summary>
        /// <param name="numbers">The top code numbers.</param>
        /// <param name="name">The name of the block.</param>
        /// <param name="image">A data URL containing the image.</param>
        /// <param name="generator">The generator to use.</param>
        /// <returns>The new <see cref="Block"/> definition.</returns>
        public static Block New(int[] numbers, string? name, string? image, string? generator)
        {
            var block = new Block
            {
                Name = name,
                Image = image,
                Generator = generator,
                Numbers = new List<int>(numbers)
            };
            return block;
        }
    }
}