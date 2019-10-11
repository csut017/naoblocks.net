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
        [InlineData(
			"[123]turn([456]45)",
			"[[123]Function:turn[:IDENTIFIER]([[456]Constant:45[:NUMBER])")]
        [InlineData(
			"[123]turn([456]-45)",
			"[[123]Function:turn[:IDENTIFIER]([[456]Constant:-45[:NUMBER])")]
        [InlineData(
			"[123]say([456]'Hello')",
			"[[123]Function:say[:IDENTIFIER]([[456]Constant:Hello[:TEXT])")]
        [InlineData(
			"reset()\nsay('hello')",
			"Function:reset[:IDENTIFIER]\n"+
				"Function:say[:IDENTIFIER](Constant:hello[:TEXT])")]
        [InlineData(
			"start{\nwait(1)\n}",
			"Function:start[:IDENTIFIER]{Function:wait[:IDENTIFIER](Constant:1[:NUMBER])}")]
        [InlineData(
			"start{\nchangeLEDColour(LEFT_EYE, #ff0000)\n}",
			"Function:start[:IDENTIFIER]{Function:changeLEDColour[:IDENTIFIER]("+
				"Constant:LEFT_EYE[:CONSTANT],Constant:ff0000[:COLOUR])}")]
        [InlineData(
			"reset()\nstart{\n  say('abc')\n  rest()\n}",
			"Function:reset[:IDENTIFIER]\n"+
				"Function:start[:IDENTIFIER]{Function:say[:IDENTIFIER](Constant:abc[:TEXT]),"+
				"Function:rest[:IDENTIFIER]}")]
        [InlineData(
			"[123]changeLEDColour(CHEST, ([456]randomColour()))",
			"[[123]Function:changeLEDColour[:IDENTIFIER](Constant:CHEST[:CONSTANT],[[456]Function:randomColour[:IDENTIFIER])")]
        [InlineData(
			"if(TRUE) {\nwait(1)\n\n}\n",
			"Compound:if[:GENERATED]{"+
				"Function:if[:IDENTIFIER](Constant:TRUE[:BOOLEAN]){"+
				"Function:wait[:IDENTIFIER](Constant:1[:NUMBER])}}")]
        [InlineData(
			"if(TRUE) {\nwait(1)\n}\nelseif(FALSE) {\nsay('hello')\n}\n",
			"Compound:if[:GENERATED]{"+
				"Function:if[:IDENTIFIER](Constant:TRUE[:BOOLEAN]){"+
				"Function:wait[:IDENTIFIER](Constant:1[:NUMBER])},"+
				"Function:elseif[:IDENTIFIER](Constant:FALSE[:BOOLEAN]){"+
				"Function:say[:IDENTIFIER](Constant:hello[:TEXT])}}")]
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
