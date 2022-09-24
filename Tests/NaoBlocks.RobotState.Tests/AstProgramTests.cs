using NaoBlocks.Common;

namespace NaoBlocks.RobotState.Tests
{
    public class AstProgramTests
    {
        [Fact]
        public void IndexerReturnsNode()
        {
            // Arrange
            var nodes = new[]
            {
                new AstNode(AstNodeType.Empty, Token.Empty, "One"),
                new AstNode(AstNodeType.Empty, Token.Empty, "Two"),
            };

            // Act
            var program = AstProgram.New(nodes);

            // Assert
            Assert.Equal("One", program[0].Node.SourceId);
            Assert.Equal("Two", program[1].Node.SourceId);
        }

        [Fact]
        public void IndexerReturnsNodeWithTree()
        {
            // Arrange
            var nodes = new[]
            {
                new AstNode(AstNodeType.Empty, Token.Empty, "One"),
                new AstNode(AstNodeType.Empty, Token.Empty, "Two"),
            };
            nodes[0].Arguments.Add(new AstNode(AstNodeType.Empty, Token.Empty, "Three"));

            // Act
            var program = AstProgram.New(nodes);

            // Assert
            Assert.Equal("One", program[0].Node.SourceId);
            Assert.Equal("Three", program[1].Node.SourceId);
            Assert.Equal("Two", program[2].Node.SourceId);
        }

        [Fact]
        public void NewGeneratesProgram()
        {
            // Arrange
            var nodes = new[]
            {
                new AstNode(AstNodeType.Empty, Token.Empty, "One"),
                new AstNode(AstNodeType.Empty, Token.Empty, "Two"),
            };

            // Act
            var program = AstProgram.New(nodes);

            // Assert
            Assert.Equal(
                new[] { "One", "Two" },
                program.RootNodes.Select(n => n.Node.SourceId).ToArray());
        }

        [Fact]
        public void NewIndexesArguments()
        {
            // Arrange
            var nodes = new[]
            {
                new AstNode(AstNodeType.Empty, Token.Empty, "One"),
                new AstNode(AstNodeType.Empty, Token.Empty, "Two"),
            };
            nodes[0].Arguments.Add(new AstNode(AstNodeType.Empty, Token.Empty, "Three"));

            // Act
            var program = AstProgram.New(nodes);

            // Assert
            Assert.Equal(
                new[] { 0, 2 },
                program.RootNodes.Select(n => n.Index).ToArray());
        }

        [Fact]
        public void NewIndexesArgumentsAndChildren()
        {
            // Arrange
            var nodes = new[]
            {
                new AstNode(AstNodeType.Empty, Token.Empty, "One"),
                new AstNode(AstNodeType.Empty, Token.Empty, "Two"),
            };
            nodes[0].Arguments.Add(new AstNode(AstNodeType.Empty, Token.Empty, "Three"));
            nodes[0].Children.Add(new AstNode(AstNodeType.Empty, Token.Empty, "Four"));

            // Act
            var program = AstProgram.New(nodes);

            // Assert
            Assert.Equal(
                new[] { 0, 3 },
                program.RootNodes.Select(n => n.Index).ToArray());
        }

        [Fact]
        public void NewIndexesChildren()
        {
            // Arrange
            var nodes = new[]
            {
                new AstNode(AstNodeType.Empty, Token.Empty, "One"),
                new AstNode(AstNodeType.Empty, Token.Empty, "Two"),
            };
            nodes[0].Children.Add(new AstNode(AstNodeType.Empty, Token.Empty, "Three"));

            // Act
            var program = AstProgram.New(nodes);

            // Assert
            Assert.Equal(
                new[] { 0, 2 },
                program.RootNodes.Select(n => n.Index).ToArray());
        }

        [Fact]
        public void NewIndexesNodes()
        {
            // Arrange
            var nodes = new[]
            {
                new AstNode(AstNodeType.Empty, Token.Empty, "One"),
                new AstNode(AstNodeType.Empty, Token.Empty, "Two"),
            };

            // Act
            var program = AstProgram.New(nodes);

            // Assert
            Assert.Equal(
                new[] { 0, 1 },
                program.RootNodes.Select(n => n.Index).ToArray());
        }

        [Fact]
        public void NewLocksAllNodes()
        {
            // Arrange
            var nodes = new[]
            {
                new AstNode(AstNodeType.Empty, Token.Empty, "One"),
                new AstNode(AstNodeType.Empty, Token.Empty, "Two"),
            };
            nodes[0].Arguments.Add(new AstNode(AstNodeType.Empty, Token.Empty, "Three"));
            nodes[0].Children.Add(new AstNode(AstNodeType.Empty, Token.Empty, "Four"));

            // Act
            var program = AstProgram.New(nodes);

            // Assert
            for (var loop = 0; loop < program.Count; loop++)
            {
                Assert.True(program[loop].IsLocked, $"Node {loop} is not locked");
            }
        }

        [Fact]
        public void NewSetsNext()
        {
            // Arrange
            var nodes = new[]
            {
                new AstNode(AstNodeType.Empty, Token.Empty, "One"),
                new AstNode(AstNodeType.Empty, Token.Empty, "Two"),
            };

            // Act
            var program = AstProgram.New(nodes);

            // Assert
            Assert.Equal(
                program[1].Index,
                program[0].Next);
        }

        [Theory]
        [InlineData(-1, "0=>Empty:(1=>Empty:){2=>Empty:}\r\n3=>Empty:")]
        [InlineData(0, "0**=>Empty:(1=>Empty:){2=>Empty:}\r\n3=>Empty:")]
        [InlineData(2, "0=>Empty:(1=>Empty:){2**=>Empty:}\r\n3=>Empty:")]
        public void ToStringFlagsNode(int node, string expected)
        {
            // Arrange
            var nodes = new[]
            {
                new AstNode(AstNodeType.Empty, Token.Empty, "One"),
                new AstNode(AstNodeType.Empty, Token.Empty, "Two"),
            };
            nodes[0].Arguments.Add(new AstNode(AstNodeType.Empty, Token.Empty, "Three"));
            nodes[0].Children.Add(new AstNode(AstNodeType.Empty, Token.Empty, "Four"));
            var program = AstProgram.New(nodes);

            // Act
            var result = program.ToString(node);

            // Assert
            Assert.Equal(
                expected,
                result);
        }

        [Fact]
        public void ToStringHandlesEmptyProgram()
        {
            // Arrange
            var program = AstProgram.New(Array.Empty<AstNode>());

            // Act
            var result = program.ToString();

            // Assert
            Assert.Equal(
                string.Empty,
                result);
        }

        [Fact]
        public void ToStringHandlesNodeTree()
        {
            // Arrange
            var nodes = new[]
            {
                new AstNode(AstNodeType.Empty, Token.Empty, "One"),
                new AstNode(AstNodeType.Empty, Token.Empty, "Two"),
            };
            nodes[0].Arguments.Add(new AstNode(AstNodeType.Empty, Token.Empty, "Three"));
            nodes[0].Children.Add(new AstNode(AstNodeType.Empty, Token.Empty, "Four"));
            var program = AstProgram.New(nodes);

            // Act
            var result = program.ToString();

            // Assert
            Assert.Equal(
                "0=>Empty:(1=>Empty:){2=>Empty:}\r\n3=>Empty:",
                result);
        }

        [Fact]
        public void ToStringHandlesSingleNode()
        {
            // Arrange
            var nodes = new[]
            {
                new AstNode(AstNodeType.Empty, Token.Empty, "One"),
            };
            var program = AstProgram.New(nodes);

            // Act
            var result = program.ToString();

            // Assert
            Assert.Equal(
                "0=>Empty:",
                result);
        }
    }
}