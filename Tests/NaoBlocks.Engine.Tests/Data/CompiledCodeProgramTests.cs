using NaoBlocks.Common;
using NaoBlocks.Engine.Data;
using NaoBlocks.Parser;
using Xunit;

namespace NaoBlocks.Engine.Tests.Data
{
    public class CompiledCodeProgramTests
    {
        [Fact]
        public void ConstructorInitialisesFromResult()
        {
            var result = new ParseResult();
            var program = new CompiledCodeProgram(result)
            {
                ProgramId = 4268
            };
            Assert.Null(program.Errors);
            Assert.Null(program.Nodes);
            Assert.Equal(4268, program.ProgramId);
        }

        [Fact]
        public void ConstructorHandlesErrors()
        {
            var result = new ParseResult();
            result.Errors.Add(new ParseError("Test", Token.Empty));
            var program = new CompiledCodeProgram(result);
            Assert.NotNull(program.Errors);
            Assert.Null(program.Nodes);
            Assert.Null(program.ProgramId);
        }

        [Fact]
        public void ConstructorHandlesNodes()
        {
            var result = new ParseResult();
            result.Nodes.Add(new AstNode(AstNodeType.Empty, Token.Empty, string.Empty));
            var program = new CompiledCodeProgram(result);
            Assert.Null(program.Errors);
            Assert.NotNull(program.Nodes);
            Assert.Null(program.ProgramId);
        }
    }
}
