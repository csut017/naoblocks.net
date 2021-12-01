using Transfer = NaoBlocks.Web.Dtos;
using Data = NaoBlocks.Common;
using Xunit;

namespace NaoBlocks.Web.Tests.Dtos
{
    public class TokenTests
    {
        [Fact]
        public void FromModelHandlesNull()
        {
            var dto = Transfer.Token.FromModel(null);
            Assert.Null(dto);
        }

        [Fact]
        public void FromModelConvertsEntity()
        {
            var entity = new Data.Token
            {
                Type = Data.TokenType.Identifier,
                Value = "Testing",
                LineNumber = 2,
                LinePosition = 4
            };
            var dto = Transfer.Token.FromModel(entity);
            Assert.Equal("Testing", dto?.Value);
            Assert.Equal("Identifier", dto?.Type);
            Assert.Equal(2, dto?.LineNumber);
            Assert.Equal(4, dto?.LinePosition);
        }

        [Fact]
        public void EmptyReturnsNoValues()
        {
            var entity = Transfer.Token.Empty;
            Assert.Equal(string.Empty, entity?.Value);
            Assert.Equal(string.Empty, entity?.Type);
        }
    }
}
