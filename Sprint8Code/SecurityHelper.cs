using System;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Configuration;

namespace CyberApp_FIA.Services
{
    public class SecurityHelper
    {
        public static byte[] GenerateSalt(int size = 16)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var salt = new byte[size];
                rng.GetBytes(salt);
                return salt;
            }
        }

        /// <summary>
        /// PBKDF2 password hashing (same parameters as sign-up so verification matches).
        /// Uses 100,000 iterations and returns a 32-byte (256-bit) derived key.
        /// Note: In .NET Framework, Rfc2898DeriveBytes uses HMACSHA1 by default.
        /// </summary>
        public static byte[] HashPassword(string password, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 250_000, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(32);     // 256-bit hash
            }
        }

        /// <summary>
        /// Constant-time byte array comparison to mitigate timing attacks.
        /// Returns true only if arrays are same length and all bytes match.
        /// </summary>
        public static bool SecureEquals(byte[] a, byte[]b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }
            return diff == 0;
        }

        private static byte[] GetEmailKey()
        {
            var keyBased64 = ConfigurationManager.AppSettings["EmailEncryptionKey"];
            return Convert.FromBase64String(keyBased64);
        }

        public static string EncryptEmail(string plaintext)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = GetEmailKey();
                aes.GenerateIV();   // Generate random IV per encryption

                using (var encryptor = aes.CreateEncryptor())
                using (var ms = new MemoryStream())
                {
                    // Prepend IV to ciphertext
                    ms.Write(aes.IV, 0, aes.IV.Length);

                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(plaintext);
                    }

                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public static string DecryptEmail(string ciphertext)
        {
            var fullCipher = Convert.FromBase64String(ciphertext);

            using (var aes = Aes.Create())
            {
                aes.Key = GetEmailKey();

                // Extract IV from beginning
                var iv = new byte[16];
                Array.Copy(fullCipher, iv, iv.Length);
                aes.IV = iv;

                using (var decryptor = aes.CreateDecryptor())
                using (var ms = new MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}
