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
        [InlineData("[123]say([456]'Hello')", "Function:say(Constant:Hello)")]
        [InlineData("reset()\nsay('hello')", "Function:reset\nFunction:say(Constant:hello)")]
        [InlineData("start{\nwait(1)\n}", "Function:start{Function:wait(Constant:1)}")]
        [InlineData(
			"start{\nchangeLEDColour(LEFT_EYE, '#ff0000')\n}",
			"Function:start{Function:changeLEDColour(Constant:LEFT_EYE,Constant:#ff0000)}")]
        [InlineData(
			"reset()\nstart{\n  say('abc')\n  rest()\n}",
			"Function:reset\nFunction:start{Function:say(Constant:abc),Function:rest}")]
        [InlineData("say(round(1))", "Function:say(Function:round(Constant:1))")]
        [InlineData("say(@test)", "Function:say(Variable:test)")]
        public void TestBasicParsingTheory(string input, string expected)
        {
            var parser = Parser.New(input);
            var output = parser.Parse();
            Assert.Empty(output.Errors);
            Assert.Equal(expected, string.Join("\n", output.Nodes.Select(n => n.ToString())));
        }

        [Theory]
        [InlineData("reset()", "Function:reset[:IDENTIFIER]")]
        [InlineData("[123]rest()", "[[123]Function:rest[:IDENTIFIER]")]
        public void TestFullParsingTheory(string input, string expected)
        {
            var parser = Parser.New(input);
            var output = parser.Parse();
            Assert.Empty(output.Errors);
            var options = new AstNode.DisplayOptions { IncludeSourceIDs = true, IncludeTokenTypes = true };
            Assert.Equal(expected, string.Join("\n", output.Nodes.Select(n => n.ToString(options))));
        }
    }
}
