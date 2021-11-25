using NaoBlocks.Engine.Data;
using Xunit;

namespace NaoBlocks.Engine.Tests.Data
{
    public class PasswordTests
    {
        [Theory]
        [InlineData("pw1", "pw1", "s1", "s1", true)]
        [InlineData("pw1", "pw1", "s1", "s2", false)]
        [InlineData("pw1", "pw2", "s1", "s1", false)]
        [InlineData("pw1", "pw2", "s1", "s2", false)]
        public void ComparerChecksHashAndSalt(string hash1, string hash2, string salt1, string salt2, bool expected)
        {
            var pw1 = new Password { Hash = hash1, Salt = salt1 };
            var pw2 = new Password { Hash = hash2, Salt = salt2 };
            var comparer = new Password.Comparer();
            Assert.Equal(expected, comparer.Equals(pw1, pw2));
        }

        [Theory]
        [InlineData(null, null, true)]
        [InlineData("first", null, false)]
        [InlineData(null, "second", false)]
        public void ComparerChecksNullConditions(string? password1, string? password2, bool expected)
        {
            var pw1 = password1 == null ? null : Password.New(password1);
            var pw2 = password2 == null ? null : Password.New(password2);
            var comparer = new Password.Comparer();
            Assert.Equal(expected, comparer.Equals(pw1, pw2));
        }

        [Fact]
        public void ComparerGeneratesHasCode()
        {
            var password = Password.New("Testing");
            var comparer = new Password.Comparer();
            Assert.NotEqual(0, comparer.GetHashCode(password));
        }

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
        public void VerifyHandlesNull()
        {
            var password = new Password();
            password.Encrypt("go-away");
            Assert.False(password.Verify(null));
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
