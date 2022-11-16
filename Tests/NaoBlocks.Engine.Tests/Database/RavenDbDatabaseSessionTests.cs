using Moq;
using NaoBlocks.Engine.Data;
using NaoBlocks.Engine.Database;
using NaoBlocks.Engine.Indices;
using Raven.Client.Documents.Session;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace NaoBlocks.Engine.Tests.Database
{
    public class RavenDbDatabaseSessionTests
    {
        [Fact]
        public void CheckCacheHandlesMissingItem()
        {
            // Arrange
            var session = new Mock<IAsyncDocumentSession>();
            using var db = new RavenDbDatabaseSession(session.Object);

            // Act
            var user = new User();
            db.CacheItem("Karetao", user);
            var item = db.GetFromCache<User>("Mihīni");

            // Assert
            Assert.Null(item);
        }

        [Fact]
        public void CheckCacheReturnsExistingItem()
        {
            // Arrange
            var session = new Mock<IAsyncDocumentSession>();
            using var db = new RavenDbDatabaseSession(session.Object);

            // Act
            var user = new User();
            db.CacheItem("Karetao", user);
            var item = db.GetFromCache<User>("Karetao");

            // Assert
            Assert.Same(user, item);
        }

        [Fact]
        public void DeleteCallsSession()
        {
            // Arrange
            var session = new Mock<IAsyncDocumentSession>();
            session.Setup(s => s.Delete(It.IsAny<User>()))
                .Verifiable();
            using var db = new RavenDbDatabaseSession(session.Object);

            // Act
            var user = new User();
            db.Delete(user);

            // Assert
            session.Verify();
        }

        [Fact]
        public async Task LoadAsyncCallsSession()
        {
            // Arrange
            var session = new Mock<IAsyncDocumentSession>();
            session.Setup(s => s.LoadAsync<User>("1", It.IsAny<CancellationToken>()))
                .Verifiable();
            using var db = new RavenDbDatabaseSession(session.Object);

            // Act
            var user = new User();
            await db.LoadAsync<User>("1");

            // Assert
            session.Verify();
        }

        [Fact]
        public void QueryCallsSession()
        {
            // Arrange
            var session = new Mock<IAsyncDocumentSession>();
            session.Setup(s => s.Query<User>(null, null, false))
                .Verifiable();
            using var db = new RavenDbDatabaseSession(session.Object);

            // Act
            var user = new User();
            db.Query<User>();

            // Assert
            session.Verify();
        }

        [Fact]
        public void QueryWithIndexCallsSession()
        {
            // Arrange
            var session = new Mock<IAsyncDocumentSession>();
            session.Setup(s => s.Query<User, RobotLogByMachineName>())
                .Verifiable();
            using var db = new RavenDbDatabaseSession(session.Object);

            // Act
            var user = new User();
            db.Query<User, RobotLogByMachineName>();

            // Assert
            session.Verify();
        }

        [Fact]
        public async Task SaveChangesAsyncCallsSession()
        {
            // Arrange
            var session = new Mock<IAsyncDocumentSession>();
            session.Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Verifiable();
            using var db = new RavenDbDatabaseSession(session.Object);

            // Act
            var user = new User();
            await db.SaveChangesAsync();

            // Assert
            session.Verify();
        }

        [Fact]
        public async Task StoreAsyncCallsSession()
        {
            // Arrange
            var session = new Mock<IAsyncDocumentSession>();
            session.Setup(s => s.StoreAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .Verifiable();
            using var db = new RavenDbDatabaseSession(session.Object);

            // Act
            var user = new User();
            await db.StoreAsync(user);

            // Assert
            session.Verify();
        }
    }
}