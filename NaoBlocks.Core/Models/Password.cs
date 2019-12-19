using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace NaoBlocks.Core.Models
{
    public class Password
    {
        public static Password Empty
        {
            get { return new Password(); }
        }

        public string Hash { get; set; } = string.Empty;

        public string Salt { get; set; } = string.Empty;

        public static Password New(string password)
        {
            var value = new Password();
            value.Encrypt(password);
            return value;
        }

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

        [SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Comparer for parent class.")]
        public class Comparer : IEqualityComparer<Password>
        {
            public bool Equals(Password x, Password y)
            {
                if ((x == null) || (y == null)) return (x == null) && (y == null);
                return (x.Hash == y.Hash) && (x.Salt == y.Salt);
            }

            public int GetHashCode(Password obj)
            {
                if (obj == null) throw new ArgumentNullException(nameof(obj));
                return (obj.Hash + obj.Salt).GetHashCode(StringComparison.Ordinal);
            }
        }
    }
}