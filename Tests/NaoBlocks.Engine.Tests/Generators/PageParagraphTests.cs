using NaoBlocks.Engine.Generators;
using System.Linq;
using Xunit;

namespace NaoBlocks.Engine.Tests.Generators
{
    public class PageParagraphTests
    {
        [Fact]
        public void AddBlockAddsABlock()
        {
            var paragraph = new PageParagraph();
            paragraph.AddBlock("testing 1");
            paragraph.AddBlock("testing 2", true);
            Assert.Equal(
                new[]
                {
                    "testing 1",
                    "<b>testing 2</b>",
                },
                paragraph.Blocks.Select(b => b.IsEmphasized ? $"<b>{b.Contents}</b>" : b.Contents).ToArray());
        }
    }
}