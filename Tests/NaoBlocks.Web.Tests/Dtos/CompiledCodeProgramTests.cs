using Transfer = NaoBlocks.Web.Dtos;
using Data = NaoBlocks.Engine.Data;
using Xunit;
using NaoBlocks.Parser;
using NaoBlocks.Common;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Web.Tests.Dtos
{
    public class CompiledCodeProgramTests
    {
        [Fact]
        public void FromModelHandlesNull()
        {
            var dto = Transfer.CompiledCodeProgram.FromModel(null);
            Assert.Null(dto);
        }

        [Fact]
        public void FromModelConvertsEntity()
        {
            var result = new ParseResult();
            var entity = new Data.CompiledCodeProgram(result)
            {
                ProgramId = 14916
            };
            var dto = Transfer.CompiledCodeProgram.FromModel(entity);
            Assert.Equal(14916, dto?.ProgramId);
            Assert.Null(dto?.Nodes);
            Assert.Null(dto?.Errors);
        }

        [Fact]
        public void FromModelConvertsErrors()
        {
            var result = new ParseResult();
            result.Errors.Add(new ParseError("Testing", Token.Empty));
            var entity = new Data.CompiledCodeProgram(result);
            var dto = Transfer.CompiledCodeProgram.FromModel(entity);
            Assert.Equal(new[] { "Testing" }, dto?.Errors?.Select(e => e.Message).ToArray());
            Assert.Null(dto?.Nodes);
        }

        [Fact]
        public async Task FromModelConvertsNodes()
        {
            var parser = CodeParser.New("go()");
            var result = await parser.ParseAsync();
            var entity = new Data.CompiledCodeProgram(result);
            var dto = Transfer.CompiledCodeProgram.FromModel(entity);
            Assert.Null(dto?.Errors);
            Assert.Equal(new[] { "Function:go" }, dto?.Nodes?.Select(n => n.Type + ":" + n.Token.Value).ToArray());
        }
    }
}
