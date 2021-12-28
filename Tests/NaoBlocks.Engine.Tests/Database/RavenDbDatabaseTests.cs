using NaoBlocks.Engine.Database;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Database
{
    public class RavenDbDatabaseTests
    {
        [Fact]
        public async Task NewInitialisesEmbeddedDatabase()
        {
            // Arrange
            var logger = new FakeLogger<RavenDbDatabase>();

            // Act
            var config = new RavenDbConfiguration { UseEmbedded = true };
            await RavenDbDatabase.New(logger, config, "/base");

            // Assert
            Assert.Equal(
                new[]
                {
                    "INFORMATION: Initialising database store",
                    "INFORMATION: Setting database options",
                    "INFORMATION: Embedded database can be accessed on http://127.0.0.1:8088",
                    "INFORMATION: Starting embedded server"
                },
                logger.Messages.ToArray());
        }
    }
}