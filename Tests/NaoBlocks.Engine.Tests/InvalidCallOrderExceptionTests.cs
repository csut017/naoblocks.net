using System;
using Xunit;

namespace NaoBlocks.Engine.Tests
{
    public class InvalidCallOrderExceptionTests
    {
        [Fact]
        public void ConstructorSetsDefaultMessage()
        {
            var exception = new InvalidCallOrderException();
            Assert.Equal("Invalid call order", exception.Message);
            Assert.Null(exception.InnerException);
        }

        [Fact]
        public void ConstructorSetsMessage()
        {
            var exception = new InvalidCallOrderException("Testing");
            Assert.Equal("Testing", exception.Message);
            Assert.Null(exception.InnerException);
        }

        [Fact]
        public void ConstructorHandlesNullMessage()
        {
            var exception = new InvalidCallOrderException(null);
            Assert.Equal("Invalid call order", exception.Message);
            Assert.Null(exception.InnerException);
        }

        [Fact]
        public void ConstructorSetsMessageAndInner()
        {
            var inner = new Exception();
            var exception = new InvalidCallOrderException("Testing", inner);
            Assert.Equal("Testing", exception.Message);
            Assert.Same(inner, exception.InnerException);
        }

        [Fact]
        public void ConstructorHandlesNullMessageWithInner()
        {
            var inner = new Exception();
            var exception = new InvalidCallOrderException(null, inner);
            Assert.Equal("Invalid call order", exception.Message);
            Assert.Same(inner, exception.InnerException);
        }
    }
}
