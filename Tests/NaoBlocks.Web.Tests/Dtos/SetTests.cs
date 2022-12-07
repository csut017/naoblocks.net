using NaoBlocks.Web.Dtos;
using System.Linq;
using Xunit;

namespace NaoBlocks.Web.Tests.Dtos
{
    public class SetTests
    {
        [Fact]
        public void NewFromArrayWorks()
        {
            var set = Set.New("one", "two");
            Assert.Equal(
                new[] { "one", "two" },
                set.Items?.ToArray());
        }

        [Fact]
        public void NewFromEnumerableWorks()
        {
            var set = Set.New(new[] { "one", "two" }.AsEnumerable());
            Assert.Equal(
                new[] { "one", "two" },
                set.Items?.ToArray());
        }

        [Fact]
        public void NewWithoutArgsWorks()
        {
            var set = new Set<string>();
            Assert.Null(set.Items);
        }
    }
}