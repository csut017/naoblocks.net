using Transfer = NaoBlocks.Web.Dtos;
using Xunit;
using NaoBlocks.Parser;
using System.Linq;
using System.Threading.Tasks;

namespace NaoBlocks.Web.Tests.Dtos
{
    public class AstNodeTests
    {
        [Fact]
        public void FromModelHandlesNull()
        {
            var dto = Transfer.AstNode.FromModel(null);
            Assert.Null(dto);
        }

        [Fact]
        public async Task FromModelConvertsEntity()
        {
            var parser = CodeParser.New("go()");
            var result = await parser.ParseAsync();
            var dto = Transfer.AstNode.FromModel(result.Nodes.First());
            Assert.Equal("Function", dto?.Type);
            Assert.Equal("go", dto?.Token?.Value);
            Assert.Null(dto?.Children);
            Assert.Null(dto?.Arguments);
        }

        [Fact]
        public async Task FromModelConvertsArguments()
        {
            var parser = CodeParser.New("say('Hello')");
            var result = await parser.ParseAsync();
            var dto = Transfer.AstNode.FromModel(result.Nodes.First());
            Assert.Equal(new[] { "Constant:Hello" }, dto?.Arguments?.Select(n => n.Type + ":" + n.Token.Value).ToArray());
        }

        [Fact]
        public async Task FromModelConvertsChildren()
        {
            var parser = CodeParser.New("start{\nrest()\n}");
            var result = await parser.ParseAsync();
            var dto = Transfer.AstNode.FromModel(result.Nodes.First());
            Assert.Equal(new[] { "Function:rest" }, dto?.Children?.Select(n => n.Type + ":" + n.Token.Value).ToArray());
        }

        [Fact]
        public void EmptyReturnsNoValues()
        {
            var entity = Transfer.AstNode.Empty;
            Assert.Equal(string.Empty, entity?.Token?.Type);
            Assert.Equal(string.Empty, entity?.Type);
        }
    }
}
