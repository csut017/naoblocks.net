namespace NaoBlocks.Engine.Tests.Database
{
    /// <remarks>
    /// This test contains some nasty code (waiting on <see cref="Task"/> instances.) This is required as
    /// RavenDB always uses the same port: so we cannot run multiple of these tests at the same time! To
    /// get around this issue, we ensure only one test is run by using multi-thread locking. And async does
    /// not allow locks!
    /// </remarks>
    //public class RavenDbDatabaseTests
    //{
    //    private const int timeoutInSeconds = 30;
    //    private static readonly object lockObject = new();

    //    [Fact]
    //    public void NewInitialisesEmbeddedDatabase()
    //    {
    //        lock (lockObject)
    //        {
    //            // Arrange
    //            var logger = new FakeLogger<RavenDbDatabase>();
    //            using var instance = EmbeddedServer.Instance;

    //            // Act
    //            var config = new RavenDbConfiguration
    //                { UseEmbedded = true, EmbeddedServerUrl = "http://127.0.0.1:8092" };
    //            RavenDbDatabase.New(logger, config, "/base").Wait(TimeSpan.FromSeconds(timeoutInSeconds));

    //            // Assert
    //            Assert.Equal(
    //                new[]
    //                {
    //                    "INFORMATION: Initialising database store",
    //                    "INFORMATION: Setting database options",
    //                    "INFORMATION: Embedded database can be accessed on http://127.0.0.1:8092",
    //                    "INFORMATION: Starting embedded server",
    //                    "INFORMATION: Server started in _x_ms",
    //                    "INFORMATION: Getting document store",
    //                    "INFORMATION: Store retrieved in _x_ms",
    //                    "INFORMATION: Starting database store",
    //                    "INFORMATION: Generating indices",
    //                    "INFORMATION: Indices generated in _x_ms"
    //                },
    //                logger.Messages.Select(RemoveTimingInformation).ToArray());
    //        }
    //    }

    //    [Fact]
    //    public void NewInitialisesEmbeddedDatabaseByDefault()
    //    {
    //        lock (lockObject)
    //        {
    //            // Arrange
    //            var logger = new FakeLogger<RavenDbDatabase>();
    //            using var instance = EmbeddedServer.Instance;

    //            // Act
    //            var config = new RavenDbConfiguration { EmbeddedServerUrl = "http://127.0.0.1:8092" };
    //            RavenDbDatabase.New(logger, config, "/base").Wait(TimeSpan.FromSeconds(timeoutInSeconds));

    //            // Assert
    //            Assert.Equal(
    //                new[]
    //                {
    //                    "INFORMATION: Initialising database store",
    //                    "INFORMATION: Setting database options",
    //                    "INFORMATION: Embedded database can be accessed on http://127.0.0.1:8092",
    //                    "INFORMATION: Starting embedded server",
    //                    "INFORMATION: Server started in _x_ms",
    //                    "INFORMATION: Getting document store",
    //                    "INFORMATION: Store retrieved in _x_ms",
    //                    "INFORMATION: Starting database store",
    //                    "INFORMATION: Generating indices",
    //                    "INFORMATION: Indices generated in _x_ms"
    //                },
    //                logger.Messages.Select(RemoveTimingInformation).ToArray());
    //        }
    //    }

    //    [Fact]
    //    public void NewInitialisesEmbeddedDatabaseWithDataDirectory()
    //    {
    //        lock (lockObject)
    //        {
    //            // Arrange
    //            var logger = new FakeLogger<RavenDbDatabase>();
    //            using var instance = EmbeddedServer.Instance;

    //            // Act
    //            var config = new RavenDbConfiguration
    //            {
    //                UseEmbedded = true,
    //                DataDirectory = @"C:\temp\data",
    //                EmbeddedServerUrl = "http://127.0.0.1:8092"
    //            };
    //            RavenDbDatabase.New(logger, config, "/base").Wait(TimeSpan.FromSeconds(timeoutInSeconds));

    //            // Assert
    //            Assert.Equal(
    //                new[]
    //                {
    //                    "INFORMATION: Initialising database store",
    //                    "INFORMATION: Setting database options",
    //                    @"INFORMATION: => DataDirectory=C:\temp\data",
    //                    "INFORMATION: Embedded database can be accessed on http://127.0.0.1:8092",
    //                    "INFORMATION: Starting embedded server",
    //                    "INFORMATION: Server started in _x_ms",
    //                    "INFORMATION: Getting document store",
    //                    "INFORMATION: Store retrieved in _x_ms",
    //                    "INFORMATION: Starting database store",
    //                    "INFORMATION: Generating indices",
    //                    "INFORMATION: Indices generated in _x_ms"
    //                },
    //                logger.Messages.Select(RemoveTimingInformation).ToArray());
    //        }
    //    }

    //    [Fact]
    //    public void StartSessionStartsSession()
    //    {
    //        lock (lockObject)
    //        {
    //            // Arrange
    //            var logger = new FakeLogger<RavenDbDatabase>();
    //            using var instance = EmbeddedServer.Instance;
    //            var config = new RavenDbConfiguration { EmbeddedServerUrl = "http://127.0.0.1:8092" };
    //            var db = RavenDbDatabase.New(logger, config, "/base").Result;

    //            // Act
    //            using var session = db.StartSession();

    //            // Assert
    //            Assert.NotNull(session);
    //        }
    //    }

    //    private static string RemoveTimingInformation(string line)
    //    {
    //        return Regex.Replace(line, "[0-9,]+ms", "_x_ms");
    //    }
    //}
}