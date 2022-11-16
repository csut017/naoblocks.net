using NaoBlocks.Engine.Data;
using System.Linq;
using Xunit;

namespace NaoBlocks.Engine.Tests.Data
{
    public class UIDefinitionItemTests
    {
        [Fact]
        public void NewSetsChildren()
        {
            var item = UIDefinitionItem.New(
                "name",
                null,
                new[]{
                    UIDefinitionItem.New("first"),
                    UIDefinitionItem.New("second")
                });
            Assert.Equal("name", item.Name);
            Assert.Null(item.Description);
            Assert.Equal(
                new[] { "first", "second" },
                item.Children.Select(c => c.Name).ToArray());
        }

        [Fact]
        public void NewSetsValues()
        {
            var item = UIDefinitionItem.New("name", "description");
            Assert.Equal("name", item.Name);
            Assert.Equal("description", item.Description);
        }
    }
}