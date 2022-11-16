using Moq;
using NaoBlocks.Engine;
using NaoBlocks.Web.Communications;
using Xunit;

namespace NaoBlocks.Web.Tests.Communications
{
    public class DefaultEngineFactoryTests
    {
        [Fact]
        public void InitialiseStartsNewEngineAndSession()
        {
            // Arrange
            var logger = new FakeLogger<ExecutionEngine>();
            var database = new Mock<IDatabase>();
            var factory = new DefaultEngineFactory(database.Object, logger);
            database.Setup(d => d.StartSession())
                .Returns(new Mock<IDatabaseSession>().Object)
                .Verifiable();

            // Act
            var (engine, session) = factory.Initialise();

            // Assert
            Assert.NotNull(engine);
            Assert.NotNull(session);
            database.Verify();
        }
    }
}
