namespace NaoBlocks.Engine.Generators
{
    /// <summary>
    /// Defines a block in a page.
    /// </summary>
    public class PageBlock
    {
        /// <summary>
        /// Initialises a new <see cref="PageBlock"/> instance.
        /// </summary>
        /// <param name="contents">The text for the block.</param>
        /// <param name="emphasize">Whether the block should be emphasized or not.</param>
        public PageBlock(object contents, bool emphasize = false)
        {
            this.Contents = contents;
            this.IsEmphasized = emphasize;
        }

        /// <summary>
        /// Defines the text in the block.
        /// </summary>
        public object Contents { get; set; }

        /// <summary>
        /// Gets or sets whether this block should be emphasized.
        /// </summary>
        public bool IsEmphasized { get; set; }

        /// <summary>
        /// Implicitly casts to a <see cref="PageBlock"/>.
        /// </summary>
        /// <param name="value">The value in the block.</param>
        public static implicit operator PageBlock(string value) => new(value);
    }
}