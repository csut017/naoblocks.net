using System.Security.Cryptography;
using System.Text;

namespace NaoBlocks.Engine
{
    /// <summary>
    /// A helper utility for using DPAPI with protected data (e.g. security tokens.)
    /// </summary>
    public static class ProtectedDataUtility
    {
        /// <summary>
        /// Descrypts a value using DPAPI.
        /// </summary>
        /// <param name="value">The value to decrypt.</param>
        /// <returns>The decrypted value.</returns>
        public static string DecryptValue(string value)
        {
            var bytes = Convert.FromBase64String(value);
#pragma warning disable CA1416 // Validate platform compatibility
            var decryptedBytes = ProtectedData.Unprotect(bytes, null, DataProtectionScope.CurrentUser);
#pragma warning restore CA1416 // Validate platform compatibility
            value = Encoding.ASCII.GetString(decryptedBytes);
            return value;
        }

        /// <summary>
        /// Encrypts a value using DPAPI.
        /// </summary>
        /// <param name="value">The value to encrypt.</param>
        /// <returns>The encrypted value.</returns>
        public static string EncryptValue(string value)
        {
            var length = value.Length % 16;
            if (length > 0) value += new string(' ', 16 - length);
            var bytes = Encoding.ASCII.GetBytes(value);
#pragma warning disable CA1416 // Validate platform compatibility
            var encryptedBytes = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
#pragma warning restore CA1416 // Validate platform compatibility
            value = Convert.ToBase64String(encryptedBytes);
            return value;
        }
    }
}