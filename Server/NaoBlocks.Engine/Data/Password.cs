using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace NaoBlocks.Engine.Data
{
    /// <summary>
    /// A password.
    /// </summary>
    public class Password
    {
        /// <summary>
        /// Gets an empty password.
        /// </summary>
        public static Password Empty
        {
            get { return new Password(); }
        }

        /// <summary>
        /// Gets or sets the hashed password.
        /// </summary>
        public string Hash { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password salt.
        /// </summary>
        public string Salt { get; set; } = string.Empty;

        /// <summary>
        /// Starts and encrypts a password.
        /// </summary>
        /// <param name="password">The plaintext password to hash.</param>
        /// <returns>A <see cref="Password"/> instance containing the hashed password.</returns>
        public static Password New(string password)
        {
            var value = new Password();
            value.Encrypt(password);
            return value;
        }

        /// <summary>
        /// Hashes a password.
        /// </summary>
        /// <param name="password">The plaintext password to hash.</param>
        public void Encrypt(string password)
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            this.Salt = Convert.ToBase64String(salt);
            this.Hash = HashPassword(password, salt);
        }

        /// <summary>
        /// Verifies that a plaintext password is the same as the hashed password.
        /// </summary>
        /// <param name="password">The plaintext password to hash.</param>
        /// <returns>True if the passwords are the same, false otherwise.</returns>
        public bool Verify(string? password)
        {
            var salt = Convert.FromBase64String(this.Salt);
            var hash = HashPassword(password ?? string.Empty, salt);
            return this.Hash == hash;
        }

        private static string HashPassword(string password, byte[] salt)
        {
            return Convert.ToBase64String(
                KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, 10000, 256 / 8));
        }

        /// <summary>
        /// An equality comparer for checking if two <see cref="Password"/> instances are the same.
        /// </summary>
        public class Comparer : IEqualityComparer<Password>
        {
            /// <summary>
            /// Determines whether the specified password are equal.
            /// </summary>
            /// <param name="x">The first password to check.</param>
            /// <param name="y">The second password to check.</param>
            /// <returns>True if the passwords are the same, false otherwise.</returns>
            public bool Equals(Password? x, Password? y)
            {
                if ((x == null) || (y == null)) return (x == null) && (y == null);
                return (x.Hash == y.Hash) && (x.Salt == y.Salt);
            }

            /// <summary>
            /// Returns a hash code for the specified password.
            /// </summary>
            /// <param name="password">The password to generate the hash for.</param>
            /// <returns>A hash code for the password.</returns>
            public int GetHashCode(Password password)
            {
                return (password.Hash + password.Salt).GetHashCode(StringComparison.Ordinal);
            }
        }
    }
}
