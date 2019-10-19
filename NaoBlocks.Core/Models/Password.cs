using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Security.Cryptography;

namespace NaoBlocks.Core.Models
{
    public class Password
    {
        public string Hash { get; set; }
        public string Salt { get; set; }

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

        public bool Verify(string password)
        {
            var salt = Convert.FromBase64String(this.Salt);
            var hash = HashPassword(password, salt);
            return this.Hash == hash;
        }

        private static string HashPassword(string password, byte[] salt)
        {
            return Convert.ToBase64String(
                KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, 10000, 256 / 8));
        }
    }
}