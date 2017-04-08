using System;
using System.Security.Cryptography;

namespace Midgard.UtilitiesN4.Services
{
    /// <summary>
    /// Shamelessly appropriated from ASP.NET Identity Framework
    /// I don't understand enough about byte arrays to accurately describe the implementation, but it creates the storable password from a
    /// byte array with the PRF type, salt, and iteration count at particular positions in the array.
    /// </summary>
    public static class IdentityBasedHasher
    {
        private static readonly int _saltSize = 128 / 8;
        private static readonly int _iterCount = 10000;
        private static readonly int _keyBytes = 256 / 8;

        public static string HashPassword(string password)
        {
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }

            byte[] salt;
            byte[] subkey;
            using (var deriveBytes = new Rfc2898DeriveBytes(password, _saltSize, _iterCount))
            {
                salt = deriveBytes.Salt;
                subkey = deriveBytes.GetBytes(_keyBytes);
            }

            var outputBytes = new byte[1 + _saltSize + _keyBytes];
            Buffer.BlockCopy(salt, 0, outputBytes, 1, _saltSize);
            Buffer.BlockCopy(subkey, 0, outputBytes, 1 + _saltSize, _keyBytes);
            return Convert.ToBase64String(outputBytes);
        }

        public static bool VerifyHashedPassword(string hashedPassword, string guess)
        {
            if (hashedPassword == null)
            {
                return false;
            }
            if (guess == null)
            {
                throw new ArgumentNullException("password");
            }

            var hashedPasswordBytes = Convert.FromBase64String(hashedPassword);

            if (hashedPasswordBytes.Length != (1 + _saltSize + _keyBytes) || hashedPasswordBytes[0] != 0x00)
            {
                // Wrong length or version header
                return false;
            }

            var salt = new byte[_saltSize];
            Buffer.BlockCopy(hashedPasswordBytes, 1, salt, 0, _saltSize);
            var storedSubkey = new byte[_keyBytes];
            Buffer.BlockCopy(hashedPasswordBytes, 1 + _saltSize, storedSubkey, 0, _keyBytes);

            byte[] generatedSubkey;
            using (var deriveBytes = new Rfc2898DeriveBytes(guess, salt, _keyBytes))
            {
                generatedSubkey = deriveBytes.GetBytes(_keyBytes);
            }
            return ByteArraysEqual(storedSubkey, generatedSubkey);
        }

        private static bool ByteArraysEqual(byte[] a, byte[] b)
        {
            if (a == null && b == null)
            {
                return true;
            }
            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }
            var areSame = true;
            for (var i = 0; i < a.Length; i++)
            {
                areSame &= (a[i] == b[i]);
            }
            return areSame;
        }

        public static string ToHashString(this byte[] ba)
        {
            return Convert.ToBase64String(ba);
        }

        public static byte[] FromHashString(this string str)
        {
            return Convert.FromBase64String(str);
        }
    }
}
