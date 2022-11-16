using Xunit;

namespace NaoBlocks.Engine.Tests
{
    public class CommandResultTests
    {
        [Fact]
        public void NewGeneratesANewInstance()
        {
            var result = CommandResult.New(5);
            Assert.Equal(5, result.Number);
            Assert.True(result.WasSuccessful);
        }

        [Fact]
        public void NewGeneratesANewInstanceWithData()
        {
            var result = CommandResult.New(5, "Data");
            Assert.Equal(5, result.Number);
            Assert.True(result.WasSuccessful);
            var typed = Assert.IsType<CommandResult<string>>(result);
            Assert.Equal("Data", typed.Output);
        }

        [Fact]
        public void AsUnboxes()
        {
            var result = CommandResult.New(5, "Data").As<string>();
            Assert.Equal("Data", result.Output);
        }

        [Fact]
        public void WasSuccessfulIsTrueWhenNoError()
        {
            var result = new CommandResult(1);
            Assert.True(result.WasSuccessful);
        }

        [Fact]
        public void WasSuccessfulIsFalseWithError()
        {
            var result = new CommandResult(1, "error");
            Assert.False(result.WasSuccessful);
        }

        [Fact]
        public void ToErrorsReturnsErrorArray()
        {
            var result = new CommandResult(1, "error");
            var errors = result.ToErrors();
            Assert.NotEmpty(errors);
        }

        [Fact]
        public void ToErrorsReturnsEmptyArray()
        {
            var result = new CommandResult(1);
            var errors = result.ToErrors();
            Assert.Empty(errors);
        }

        [Fact]
        public void ConstructorGeneratesEmptyInstance()
        {
            var result = new CommandResult<string>(1);
            Assert.Equal(1, result.Number);
            Assert.True(result.WasSuccessful);
            Assert.Null(result.Output);
        }
    }
}
