using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace NaoBlocks.Parser.Tests
{
    public class ParserTests
    {
        [Fact]
        public void TestParseHandlesEmptyInput()
        {
            var parser = Parser.New(string.Empty);
            var output = parser.Parse();
            var expected = new[]
            {
                "Nothing to parse"
            };
            Assert.Equal(expected.AsEnumerable(), output.Errors.Select(e => e.Message));
        }

        [Theory]
        [InlineData("reset()", "Function:reset")]
        [InlineData("[123]rest()", "Function:rest")]
        public void TestParsingTheory(string input, string expected)
        {
            var parser = Parser.New(input);
            var output = parser.Parse();
            Assert.Empty(output.Errors);
            Assert.Equal(expected, string.Join(Environment.NewLine, output.Nodes.Select(n => n.ToString())));
        }
    }
}
