using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace S7CommPlusDriver.Legitimation
{
    static class LegitimationCrypto
    {

        /// <summary>
        /// SHA256
        /// </summary>
        /// <param name="data">Data to hash</param>
        /// <returns>Hash</returns>
        public static byte[] sha256(byte[] data)
        {
            using (SHA256 sha = SHA256.Create())
            {
                return sha.ComputeHash(data);
            }
        }

        /// <summary>
        /// Encrypt AES256CBC
        /// </summary>
        /// <param name="plainBytes">Plain data</param>
        /// <param name="key">Encryption key</param>
        /// <param name="iv">Init vector</param>
        /// <returns>Encrypted data</returns>
        public static byte[] EncryptAesCbc(byte[] plainBytes, byte[] key, byte[] iv)
        {
            byte[] encryptedBytes = null;

            // Set up the encryption objects
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // Encrypt the input plaintext using the AES algorithm
                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                {
                    encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                }
            }

            return encryptedBytes;
        }

    }
}
