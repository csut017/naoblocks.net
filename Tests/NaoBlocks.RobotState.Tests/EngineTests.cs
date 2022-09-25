using NaoBlocks.Common;

namespace NaoBlocks.RobotState.Tests
{
    public class EngineTests
    {
        [Fact]
        public void ConstructorAddsResetFunction()
        {
            var engine = new Engine();
            Assert.Equal(
                new[] { "reset" },
                engine.CurrentFunctions.Select(f => f.Name).ToArray());
        }

        [Fact]
        public void ConstructorChecksFunctionsAreUnique()
        {
            var ex = Assert.Throws<EngineException>(() => new Engine(
                new TestFunction("non-unique name"),
                new TestFunction("non-unique name")));
            Assert.Equal(
                "Function 'non-unique name' is already defined, each function name must be unique",
                ex.Message);
        }

        [Fact]
        public void ConstructorChecksFunctionsCannotOverrideDefault()
        {
            var ex = Assert.Throws<EngineException>(() => new Engine(
                new TestFunction("reset")));
            Assert.Equal(
                "Function 'reset' is already defined, each function name must be unique",
                ex.Message);
        }

        [Fact]
        public void DefaultCurrentNodeIsNegativeOne()
        {
            var engine = new Engine();
            Assert.Equal(-1, engine.CurrentNode);
        }

        [Fact]
        public async Task FindFunctionFindsCustomFunction()
        {
            // Arrange
            const string funcName = "a custom function";
            var engine = new Engine(
                new TestFunction(funcName));
            await engine.ResetAsync();

            // Act
            var function = engine.FindFunction(funcName);

            // Assert
            Assert.Equal(
                funcName,
                function?.Name);
        }

        [Fact]
        public async Task FindFunctionFindsDefaultFunction()
        {
            // Arrange
            var engine = new Engine();
            await engine.ResetAsync();

            // Act
            var function = engine.FindFunction("go");

            // Assert
            Assert.Equal(
                "go",
                function?.Name);
        }

        [Fact]
        public void FindFunctionFindsResetFunction()
        {
            // Arrange
            var engine = new Engine();

            // Act
            var function = engine.FindFunction("reset");

            // Assert
            Assert.Equal(
                "reset",
                function?.Name);
        }

        [Fact]
        public async Task FindFunctionHandlesMissingFunction()
        {
            // Arrange
            var engine = new Engine();
            await engine.ResetAsync();

            // Act
            var function = engine.FindFunction("does not exist");

            // Assert
            Assert.Null(function);
        }

        [Fact]
        public async Task InitialiseHandlesInvalidProgram()
        {
            // Arrange
            var engine = new Engine();

            // Act
            var error = await Assert.ThrowsAsync<EngineException>(async () => await engine.InitialiseAsync("go("));

            // Assert
            Assert.Equal(
                "Unable to parse program code:\r\n* Unable to parse function arg {:EOF} [0:3]",
                error.Message);
        }

        [Fact]
        public async Task InitialiseSetsInitialStateFromNodes()
        {
            // Arrange
            var engine = new Engine();
            var nodes = new[]
            {
                new AstNode(AstNodeType.Function, new Token(TokenType.Constant, "go"), string.Empty)
            };

            // Act
            await engine.InitialiseAsync(nodes);

            // Assert
            var program = engine.ToString();
            Assert.Equal(
                $"Program:0=>Function:go{Environment.NewLine}Variables:{{}}",
                program);
        }

        [Fact]
        public async Task InitialiseSetsInitialStateFromString()
        {
            // Arrange
            var engine = new Engine();

            // Act
            await engine.InitialiseAsync("go()");

            // Assert
            var program = engine.ToString();
            Assert.Equal(
                $"Program:0=>Function:go{Environment.NewLine}Variables:{{}}",
                program);
        }

        [Fact]
        public async Task ResetAsyncAddsCustomFunction()
        {
            // Arrange
            const string funcName = "a custom function";
            var engine = new Engine(
                new TestFunction(funcName));

            // Act
            await engine.ResetAsync();

            // Assert
            Assert.Contains(
                funcName,
                engine.CurrentFunctions.Select(f => f.Name));
        }

        [Theory]
        [InlineData("reset")]
        [InlineData("start")]
        [InlineData("go")]
        public async Task ResetAsyncAddsTheDefaultFunction(string name)
        {
            // Arrange
            var engine = new Engine();

            // Act
            await engine.ResetAsync();

            // Assert
            Assert.Contains(
                name,
                engine.CurrentFunctions.Select(f => f.Name));
        }

        [Fact]
        public async Task ResetAsyncHandlesMultipleCalls()
        {
            // Arrange
            var engine = new Engine();

            // Act
            await engine.ResetAsync();
            await engine.ResetAsync();

            // Assert
            var count = engine.CurrentFunctions.Where(f => f.Name == "reset").Count();
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task StartAsyncChecksForProgram()
        {
            // Arrange
            var engine = new Engine();

            // Act
            var ex = await Assert.ThrowsAsync<EngineException>(async () => await engine.StartAsync());

            // Assert
            Assert.Equal(
                "Cannot start execution: no program loaded",
                ex.Message);
        }

        [Fact]
        public async Task StartAsyncSetsState()
        {
            // Arrange
            var engine = new Engine();
            await engine.InitialiseAsync("reset()");

            // Act
            await engine.StartAsync();

            // Assert
            Assert.Equal(0, engine.CurrentNode);
        }
    }
}