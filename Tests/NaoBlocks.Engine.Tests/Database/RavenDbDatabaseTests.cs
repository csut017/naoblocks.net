using NaoBlocks.Engine.Database;
using Raven.Embedded;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Database
{
    public class RavenDbDatabaseTests
    {
        [Fact(Skip = "Disabled test due to some issue with reserving the port multiple times")]
        public async Task NewInitialisesEmbeddedDatabaseByDefault()
        {
            // Arrange
            var logger = new FakeLogger<RavenDbDatabase>();
            using var instance = EmbeddedServer.Instance;

            // Act
            await RavenDbDatabase.New(logger, null, "/base");

            // Assert
            Assert.Equal(
                new[]
                {
                    "INFORMATION: Initialising database store",
                    "INFORMATION: Embedded database can be accessed on http://127.0.0.1:8090",
                    "INFORMATION: Starting embedded server",
                    "INFORMATION: Starting database store",
                    "INFORMATION: Generating indexes"
                },
                logger.Messages.ToArray());
        }

        [Fact(Skip = "Disabled test due to some issue with reserving the port multiple times")]
        public async Task NewInitialisesEmbeddedDatabase()
        {
            // Arrange
            var logger = new FakeLogger<RavenDbDatabase>();
            using var instance = EmbeddedServer.Instance;

            // Act
            var config = new RavenDbConfiguration { UseEmbedded = true };
            await RavenDbDatabase.New(logger, config, "/base");

            // Assert
            Assert.Equal(
                new[]
                {
                    "INFORMATION: Initialising database store",
                    "INFORMATION: Setting database options",
                    "INFORMATION: Embedded database can be accessed on http://127.0.0.1:8090",
                    "INFORMATION: Starting embedded server",
                    "INFORMATION: Starting database store",
                    "INFORMATION: Generating indexes"
                },
                logger.Messages.ToArray());
        }

        [Fact(Skip = "Disabled test due to some issue with reserving the port multiple times")]
        public async Task NewInitialisesEmbeddedDatabaseWithFrameworkPath()
        {
            // Arrange
            var logger = new FakeLogger<RavenDbDatabase>();
            using var instance = EmbeddedServer.Instance;

            // Act
            var config = new RavenDbConfiguration
            {
                UseEmbedded = true,
                FrameworkVersion = "6.0.1",
                DotNetPath = @"C:\Program Files\dotnet\dotnet"
            };
            await RavenDbDatabase.New(logger, config, "/base");

            // Assert
            Assert.Equal(
                new[]
                {
                    "INFORMATION: Initialising database store",
                    "INFORMATION: Setting database options",
                    @"INFORMATION: => DotNetPath=C:\Program Files\dotnet\dotnet",
                    "INFORMATION: => FrameworkVersion=6.0.1",
                    "INFORMATION: Embedded database can be accessed on http://127.0.0.1:8090",
                    "INFORMATION: Starting embedded server",
                    "INFORMATION: Starting database store",
                    "INFORMATION: Generating indexes"
                },
                logger.Messages.ToArray());
        }

        [Fact(Skip = "Disabled test due to some issue with reserving the port multiple times")]
        public async Task NewInitialisesEmbeddedDatabaseWithDataDirectory()
        {
            // Arrange
            var logger = new FakeLogger<RavenDbDatabase>();
            using var instance = EmbeddedServer.Instance;

            // Act
            var config = new RavenDbConfiguration
            {
                UseEmbedded = true,
                DataDirectory = @"C:\temp\data"
            };
            await RavenDbDatabase.New(logger, config, "/base");

            // Assert
            Assert.Equal(
                new[]
                {
                    "INFORMATION: Initialising database store",
                    "INFORMATION: Setting database options",
                    @"INFORMATION: => DataDirectory=C:\temp\data",
                    "INFORMATION: Embedded database can be accessed on http://127.0.0.1:8090",
                    "INFORMATION: Starting embedded server",
                    "INFORMATION: Starting database store",
                    "INFORMATION: Generating indexes"
                },
                logger.Messages.ToArray());
        }

        [Fact(Skip = "Disabled test due to some issue with reserving the port multiple times")]
        public async Task StartSessionStartsSession()
        {
            // Arrange
            var logger = new FakeLogger<RavenDbDatabase>();
            using var instance = EmbeddedServer.Instance;
            var db = await RavenDbDatabase.New(logger, null, "/base");

            // Act
            using var session = db.StartSession();

            // Assert
            Assert.NotNull(session);
        }
    }
}