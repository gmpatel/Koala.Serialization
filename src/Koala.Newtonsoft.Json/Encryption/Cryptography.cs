using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Properties;

namespace Newtonsoft.Json.Encryption
{
    public static class Cryptography
    {
        private static byte[] GenerateKeyFiles()
        {
            using (var myAes = Aes.Create())
            {
                myAes.KeySize = 256;
                File.WriteAllBytes("abra.kagm", myAes.Key);
                File.WriteAllBytes("abra.iagm", myAes.IV);

                return myAes.IV;
            }
        }

        public static byte[] Encrypt(this string plainText)
        {
            var bytes = Encoding.ASCII.GetBytes(plainText);
            return bytes.Encrypt();
        }

        public static byte[] Encrypt(this byte[] plainText)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (GetGeneratorKey() == null || GetGeneratorKey().Length <= 0)
                throw new ArgumentNullException("Key");
            if (GetGeneratorIV() == null || GetGeneratorIV().Length <= 0)
                throw new ArgumentNullException("IV");

            byte[] encrypted;
            // Create an RijndaelManaged object
            // with the specified key and IV.
            using (var rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = GetGeneratorKey();
                rijAlg.IV = GetGeneratorIV();

                // Create an encryptor to perform the stream transform.
                var encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for encryption.
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new BinaryWriter(csEncrypt))
                        {

                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }
        
        public static string EncryptString(this string plainText)
        {

            var encBytes = plainText.Encrypt();
            return Encoding.ASCII.GetString(encBytes);
        }

        public static string DecryptToString(this byte[] cipherText)
        {
            var bytes = cipherText.Decrypt();
            return Encoding.ASCII.GetString(bytes);
        }

        public static byte[] Decrypt(this byte[] cipherText)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (GetGeneratorKey() == null || GetGeneratorKey().Length <= 0)
                throw new ArgumentNullException("Key");
            if (GetGeneratorIV() == null || GetGeneratorIV().Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            byte[] plaintext = null;

            // Create an RijndaelManaged object
            // with the specified key and IV.
            using (var rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = GetGeneratorKey();
                rijAlg.IV = GetGeneratorIV();

                // Create a decryptor to perform the stream transform.
                var decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for decryption.
                using (var msDecrypt = new MemoryStream(cipherText))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new MemoryStream())
                        {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            csDecrypt.CopyTo(srDecrypt);
                            plaintext = srDecrypt.ToArray();
                        }
                    }
                }

            }
            return plaintext;
        }

        private static byte[] GetGeneratorKey()
        {
            return Resources.abrak;
        }

        private static byte[] GetGeneratorIV()
        {
            return Resources.abrai;
        }
    }
}
