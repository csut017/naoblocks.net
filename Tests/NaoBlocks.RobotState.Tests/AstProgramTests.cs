using NaoBlocks.Common;

namespace NaoBlocks.RobotState.Tests
{
    public class AstProgramTests
    {
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
    }
}