namespace NaoBlocks.Engine.Generators
{
    /// <summary>
    /// Defines a paragraph in a page.
    /// </summary>
    public class PageParagraph
    {
        /// <summary>
        /// Gets the blocks in the paragraph.
        /// </summary>
        public List<PageBlock> Blocks { get; private set; } = new List<PageBlock>();

        /// <summary>
        /// Adds a new block.
        /// </summary>
        /// <param name="contents">The text of the block.</param>
        /// <param name="emphasize">Whether the block should be emphasized or not.</param>
        /// <returns>The <see cref="PageParagraph"/> instance.</returns>
        public PageParagraph AddBlock(object contents, bool emphasize = false)
        {
            var block = new PageBlock(contents, emphasize);
            this.Blocks.Add(block);
            return this;
        }
    }
}