using Moq;
using System;
using Xunit;

namespace NaoBlocks.Engine.Tests
{
    public class DataQueryTests
    {
        [Fact]
        public void SessionFailsIfNotInitialised()
        {
            var query = new FakeQuery();
            Assert.Throws<InvalidOperationException>(() => query.Session);
        }

        [Fact]
        public void SessionReturnsSession()
        {
            var query = new FakeQuery();
            var session = new Mock<IDatabaseSession>();
            query.InitialiseSession(session.Object);
            Assert.Same(session.Object, query.Session);
        }

        private class FakeQuery
            : DataQuery
        { }
    }
}
