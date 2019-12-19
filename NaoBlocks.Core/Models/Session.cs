using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Security.Cryptography;

namespace NaoBlocks.Core.Models
{
    public class Session
    {
        public string Id { get; set; }

        public string Key { get; set; }

        public UserRole Role { get; set; }

        public string UserId { get; set; }

        public DateTime WhenAdded { get; set; }

        public DateTime WhenExpires { get; set; }

        public void GenerateNewKey()
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var guid = Guid.NewGuid().ToString();
            this.Key = Convert.ToBase64String(
                KeyDerivation.Pbkdf2(guid, salt, KeyDerivationPrf.HMACSHA256, 10000, 256 / 8));
        }
    }
}