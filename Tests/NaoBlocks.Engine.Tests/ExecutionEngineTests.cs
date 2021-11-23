using Moq;
using NaoBlocks.Common;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests
{
    public class ExecutionEngineTests
    {
        [Fact]
        public async Task CommitCallsSave()
        {
            // Arrange
            var database = new Mock<IDatabase>();
            var session = new Mock<IDatabaseSession>();
            var logger = new FakeLogger<ExecutionEngine>();
            var engine = new ExecutionEngine(database.Object, session.Object, logger);

            // Act
            await engine.CommitAsync();

            // Assert
            session.Verify(s => s.SaveChangesAsync(), Times.Once);
            Assert.Equal(new string[] { "TRACE: Session saved" }, logger.Messages.ToArray());
        }

        [Fact]
        public async Task ExecuteRunsThroughProcess()
        {
            // Arrange
            var database = new Mock<IDatabase>();
            var session = new Mock<IDatabaseSession>();
            database.Setup(d => d.StartSession()).Returns(session.Object);
            var logger = new FakeLogger<ExecutionEngine>();
            var engine = new ExecutionEngine(database.Object, session.Object, logger);
            var command = new FakeCommand();

            // Act
            var result = await engine.ExecuteAsync(command);

            // Assert
            Assert.True(command.ExecuteCalled);
            database.Verify(d => d.StartSession(), Times.Once);
            session.Verify(s => s.StoreAsync(It.IsAny<CommandLog>()), Times.Once);
            session.Verify(s => s.SaveChangesAsync(), Times.Once);
            Assert.Equal(new string[] {
                    "INFORMATION: Command executed",
                    "TRACE: Log saved"
                }, logger.Messages.ToArray());
        }

        [Fact]
        public async Task RestoreRunsThroughProcessWithoutErrors()
        {
            // Arrange
            var database = new Mock<IDatabase>();
            var session = new Mock<IDatabaseSession>();
            var logger = new FakeLogger<ExecutionEngine>();
            var engine = new ExecutionEngine(database.Object, session.Object, logger);
            var command = new FakeCommand();

            // Act
            await engine.RestoreAsync(command);

            // Assert
            Assert.True(command.RestoreCalled);
            Assert.Equal(new string[] {
                    "INFORMATION: Command restored"
                }, logger.Messages.ToArray());
        }

        [Fact]
        public async Task RestoreRunsThroughProcessWithErrors()
        {
            // Arrange
            var database = new Mock<IDatabase>();
            var session = new Mock<IDatabaseSession>();
            var logger = new FakeLogger<ExecutionEngine>();
            var engine = new ExecutionEngine(database.Object, session.Object, logger);
            var command = new FakeCommand
            {
                OnRestoration = () => new CommandError[] { new CommandError(1, "Two") }
            };

            // Act
            await engine.RestoreAsync(command);

            // Assert
            Assert.True(command.RestoreCalled);
            Assert.Equal(new string[] {
                    "WARNING: Unable to restore command"
                }, logger.Messages.ToArray());
        }

        [Fact]
        public async Task ValidateRunsThroughProcessWithoutErrors()
        {
            // Arrange
            var database = new Mock<IDatabase>();
            var session = new Mock<IDatabaseSession>();
            var logger = new FakeLogger<ExecutionEngine>();
            var engine = new ExecutionEngine(database.Object, session.Object, logger);
            var command = new FakeCommand();

            // Act
            await engine.ValidateAsync(command);

            // Assert
            Assert.True(command.ValidateCalled);
            Assert.Equal(new string[] {
                    "INFORMATION: Command validated"
                }, logger.Messages.ToArray());
        }

        [Fact]
        public async Task ValidateRunsThroughProcessWithErrors()
        {
            // Arrange
            var database = new Mock<IDatabase>();
            var session = new Mock<IDatabaseSession>();
            var logger = new FakeLogger<ExecutionEngine>();
            var engine = new ExecutionEngine(database.Object, session.Object, logger);
            var command = new FakeCommand
            {
                OnValidation = () => new CommandError[] { new CommandError(1, "Two") }
            };

            // Act
            await engine.ValidateAsync(command);

            // Assert
            Assert.True(command.ValidateCalled);
            Assert.Equal(new string[] {
                    "WARNING: Command failed validation"
                }, logger.Messages.ToArray());
        }
    }
}
