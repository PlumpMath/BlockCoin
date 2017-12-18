using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;

namespace BlockCoin
{

    public enum Supported_HA
    {
        SHA256, SHA384, SHA512
    }

    public class Hashing
    {
        /// <summary>
        /// Defining constants for our security standard
        /// </summary>
        private const int KEYSIZE = 256;
        private const int DERIVATIONITERATIONS = 1000;
        private static readonly byte[] SALT_BYTES = new byte[10] { 55, 8, 55, 12, 52, 1, 55, 32, 16, 8 };

        public static string ComputeHash(string plainText, Supported_HA hash, byte[] salt)
        {
            int minSaltLength = 4, maxSaltLength = 16;

            byte[] saltBytes = null;
            if (salt != null)
            {
                saltBytes = salt;
            }
            else
            {
                saltBytes = SALT_BYTES;
            }

            byte[] plainData = ASCIIEncoding.UTF8.GetBytes(plainText);
            byte[] plainDataWithSalt = new byte[plainData.Length + saltBytes.Length];

            for (int x = 0; x < plainData.Length; x++)
                plainDataWithSalt[x] = plainData[x];
            for (int n = 0; n < saltBytes.Length; n++)
                plainDataWithSalt[plainData.Length + n] = saltBytes[n];

            byte[] hashValue = null;

            switch (hash)
            {
                case Supported_HA.SHA256:
                    SHA256Managed sha = new SHA256Managed();
                    hashValue = sha.ComputeHash(plainDataWithSalt);
                    sha.Dispose();
                    break;
                case Supported_HA.SHA384:
                    SHA384Managed sha1 = new SHA384Managed();
                    hashValue = sha1.ComputeHash(plainDataWithSalt);
                    sha1.Dispose();
                    break;
                case Supported_HA.SHA512:
                    SHA512Managed sha2 = new SHA512Managed();
                    hashValue = sha2.ComputeHash(plainDataWithSalt);
                    sha2.Dispose();
                    break;
            }

            byte[] result = new byte[hashValue.Length + saltBytes.Length];
            for (int x = 0; x < hashValue.Length; x++)
                result[x] = hashValue[x];
            for (int n = 0; n < saltBytes.Length; n++)
                result[hashValue.Length + n] = saltBytes[n];

            return Convert.ToBase64String(result);
        }
        public static bool Confirm(string plainText, string hashValue, Supported_HA hash)
        {
            byte[] hashBytes = Convert.FromBase64String(hashValue);
            int hashSize = 0;

            switch (hash)
            {
                case Supported_HA.SHA256:
                    hashSize = 32;
                    break;
                case Supported_HA.SHA384:
                    hashSize = 48;
                    break;
                case Supported_HA.SHA512:
                    hashSize = 64;
                    break;
            }

            byte[] saltBytes = new byte[hashBytes.Length - hashSize];

            for (int x = 0; x < saltBytes.Length; x++)
                saltBytes[x] = hashBytes[hashSize + x];

            string newHash = ComputeHash(plainText, hash, saltBytes);

            return (hashValue == newHash);
        }

        /// <summary>
        /// Generates a random array of 256 bits (32 bytes) of random data
        /// </summary>
        /// <returns></returns>
        private static byte[] Generate256BitsOfRandomEntropy()
        {
            byte[] randomBytes = new byte[32]; // 32 Bytes will give us 256 bits.
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                // Fill the array with cryptographically secure random bytes.
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }

        public static string Encrypt(string plainText, string passPhrase)
        {
            // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
            // so that the same Salt and IV values can be used when decrypting.  
            var saltStringBytes = SALT_BYTES;
            var ivStringBytes = Generate256BitsOfRandomEntropy();
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DERIVATIONITERATIONS))
            {
                var keyBytes = password.GetBytes(KEYSIZE / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                cryptoStream.FlushFinalBlock();
                                // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
                                var cipherTextBytes = saltStringBytes;
                                cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                                cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Convert.ToBase64String(cipherTextBytes);
                            }
                        }
                    }
                }
            }
        }

        public static string Decrypt(string cipherText, string passPhrase)
        {
            // Get the complete stream of bytes that represent:
            // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
            var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
            // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
            var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(KEYSIZE / 8).ToArray();
            // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
            var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(KEYSIZE / 8).Take(KEYSIZE / 8).ToArray();
            // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
            var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((KEYSIZE / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((KEYSIZE / 8) * 2)).ToArray();

            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DERIVATIONITERATIONS))
            {
                var keyBytes = password.GetBytes(KEYSIZE / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream(cipherTextBytes))
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                var plainTextBytes = new byte[cipherTextBytes.Length];
                                var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Encrypts a file with a given pass-phrase.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="passPhrase"></param>
        public static void EncryptFile(string filePath, string passPhrase)
        {
            //generate a random salt
            byte[] saltStringBytes = Generate256BitsOfRandomEntropy();
            //generate inital vector 
            byte[] ivStringBytes = Generate256BitsOfRandomEntropy();

            byte[] filePlainBytes = File.ReadAllBytes(filePath);

            //generate the password from the passPhrase and the salt
            using (Rfc2898DeriveBytes password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DERIVATIONITERATIONS))
            {
                //convert our pasword to bytes
                byte[] keyBytes = password.GetBytes(KEYSIZE / 8);

                using (var symmetricKey = new RijndaelManaged())
                {
                    //setup the key
                    symmetricKey.BlockSize = KEYSIZE;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;

                    using (ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(filePlainBytes, 0, filePlainBytes.Length);
                                cryptoStream.FlushFinalBlock();
                                //create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
                                byte[] cipherTextBytes = saltStringBytes;
                                cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                                cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();

                                memoryStream.Close();
                                cryptoStream.Close();

                                //write encrypted data to file
                                using (FileStream fileStream = new FileStream("encrypted.enc", FileMode.Create))
                                {
                                    fileStream.Write(cipherTextBytes, 0, cipherTextBytes.Length);
                                }
                            }
                        }
                    }

                }

            }
        }

        /// <summary>
        /// Decrypts a file with a given pass-phrase.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="passPhrase"></param>
        public static void DecryptFile(string filePath, string passPhrase)
        {
            // Get the complete stream of bytes that represent:
            // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]

            // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
            byte[] cipherTextBytesWithSaltAndIv = File.ReadAllBytes(filePath);
            byte[] saltStringBytes = cipherTextBytesWithSaltAndIv.Take(KEYSIZE / 8).ToArray();
            // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
            byte[] ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(KEYSIZE / 8).Take(KEYSIZE / 8).ToArray();
            // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
            byte[] cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((KEYSIZE / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((KEYSIZE / 8) * 2)).ToArray();

            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DERIVATIONITERATIONS))
            {
                byte[] keyBytes = password.GetBytes(KEYSIZE / 8);
                using (RijndaelManaged symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                    {
                        using (MemoryStream memoryStream = new MemoryStream(cipherTextBytes))
                        {
                            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                var plainTextBytes = new byte[cipherTextBytes.Length];
                                var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                memoryStream.Close();
                                cryptoStream.Close();
                                //write encrypted data to file
                                using (FileStream fileStream = new FileStream("decrypted.den", FileMode.Create))
                                {
                                    fileStream.Write(plainTextBytes, 0, decryptedByteCount);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Computes an un-salted SHA256 hash for file comparison.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GenerateSHA256(string filePath)
        {
            using (FileStream stream = File.OpenRead(filePath))
            {
                var sha = new SHA256Managed();
                byte[] checksum = sha.ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", String.Empty);
            }
        }
    }
}
