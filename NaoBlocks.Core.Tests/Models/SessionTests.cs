using NaoBlocks.Core.Models;
using Xunit;

namespace NaoBlocks.Core.Tests.Models
{
    public class SessionTests
    {
        [Fact]
        public void GenerateNewKeyGeneratesRandomKeys()
        {
            var session = new Session();
            session.GenerateNewKey();
            var key = session.Key;
            session.GenerateNewKey();
            Assert.NotEqual(key, session.Key);
        }
    }
}