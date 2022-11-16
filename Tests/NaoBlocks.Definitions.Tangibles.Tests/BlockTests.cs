using System.Linq;
using Xunit;

namespace NaoBlocks.Definitions.Tangibles.Tests
{
    public class BlockTests
    {
        [Fact]
        public void NewHandlesMultipleNumbers()
        {
            var block = Block.New(new[] { 1, 2, 3 }, "name", "image", "generator");
            Assert.Equal(new[] { 1, 2, 3 }, block.Numbers.ToArray());
            Assert.Equal("name", block.Name);
            Assert.Equal("image", block.Image);
            Assert.Equal("generator", block.Generator);
        }

        [Fact]
        public void NewHandlesSingleNumber()
        {
            var block = Block.New(1, "name", "image", "generator");
            Assert.Equal(new[] { 1 }, block.Numbers.ToArray());
            Assert.Equal("name", block.Name);
            Assert.Equal("image", block.Image);
            Assert.Equal("generator", block.Generator);
        }
    }
}