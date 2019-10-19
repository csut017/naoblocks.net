using NaoBlocks.Core.Models;
using Xunit;

namespace NaoBlocks.Core.Tests.Models
{
    public class PasswordTests
    {
        [Fact]
        public void EncryptGeneratesSaltAndHashTest()
        {
            var password = new Password();
            password.Encrypt("testing");
            Assert.False(string.IsNullOrWhiteSpace(password.Hash));
            Assert.False(string.IsNullOrWhiteSpace(password.Salt));
        }

        [Fact]
        public void VerifyFailsWithIncorrectPassword()
        {
            var password = new Password();
            password.Encrypt("go-away");
            Assert.False(password.Verify("Hello"));
        }

        [Fact]
        public void VerifyPassesWithCorrectPassword()
        {
            var password = new Password();
            password.Encrypt("hello");
            Assert.True(password.Verify("hello"));
        }
    }
}