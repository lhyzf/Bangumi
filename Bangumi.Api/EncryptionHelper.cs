using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Bangumi.Api
{
    class EncryptionHelper
    {
        // 随意修改
        private static byte[] entropy = new byte[16]
        {
            0x30, 0x31, 0x30, 0x31, 0x30, 0x31, 0x30, 0x31,
            0x30, 0x31, 0x30, 0x31, 0x30, 0x31, 0x30, 0x31
        };


        public static async Task Write(string fileName, string data)
        {
            try
            {
                // Create the original data to be encrypted
                byte[] toEncrypt = UnicodeEncoding.ASCII.GetBytes(data);

                // Create a file.
                using (FileStream fStream = new FileStream(fileName, FileMode.OpenOrCreate))
                {
                    //Debug.WriteLine("Original data: " + UnicodeEncoding.ASCII.GetString(toEncrypt));
                    Debug.WriteLine("Encrypting and writing to disk...");

                    // Encrypt a copy of the data to the stream.
                    int bytesWritten = await EncryptDataToStream(toEncrypt, entropy, DataProtectionScope.CurrentUser, fStream);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("ERROR: " + e.Message);
            }
        }

        public static async Task<string> Read(string fileName)
        {
            try
            {
                Debug.WriteLine("Reading data from disk and decrypting...");

                // Open the file.
                using (FileStream fStream = new FileStream(fileName, FileMode.Open))
                {
                    // Read from the stream and decrypt the data.
                    byte[] decryptData = await DecryptDataFromStream(entropy, DataProtectionScope.CurrentUser, fStream);

                    //Debug.WriteLine("Decrypted data: " + UnicodeEncoding.ASCII.GetString(decryptData));
                    return UnicodeEncoding.ASCII.GetString(decryptData);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("ERROR: " + e.Message);
                return "";
            }
        }

        private static byte[] CreateRandomEntropy()
        {
            // Create a byte array to hold the random value.
            byte[] entropy = new byte[16];

            // Create a new instance of the RNGCryptoServiceProvider.
            // Fill the array with a random value.
            new RNGCryptoServiceProvider().GetBytes(entropy);

            // Return the array.
            return entropy;
        }

        public static async Task<int> EncryptDataToStream(byte[] Buffer, byte[] Entropy, DataProtectionScope Scope, Stream S)
        {
            if (Buffer == null)
                throw new ArgumentNullException("Buffer");
            if (Buffer.Length <= 0)
                throw new ArgumentException("Buffer");
            if (Entropy == null)
                throw new ArgumentNullException("Entropy");
            if (Entropy.Length <= 0)
                throw new ArgumentException("Entropy");
            if (S == null)
                throw new ArgumentNullException("S");

            int length = 0;

            // Encrypt the data and store the result in a new byte array. The original data remains unchanged.
            byte[] encryptedData = ProtectedData.Protect(Buffer, Entropy, Scope);

            // Write the encrypted data to a stream.
            if (S.CanWrite && encryptedData != null)
            {
                await S.WriteAsync(encryptedData, 0, encryptedData.Length);

                length = encryptedData.Length;
            }

            // Return the length that was written to the stream. 
            return length;
        }

        public static async Task<byte[]> DecryptDataFromStream(byte[] Entropy, DataProtectionScope Scope, Stream S)
        {
            if (S == null)
                throw new ArgumentNullException("S");
            if (S.Length <= 0)
                throw new ArgumentException("S");
            if (Entropy == null)
                throw new ArgumentNullException("Entropy");
            if (Entropy.Length <= 0)
                throw new ArgumentException("Entropy");


            byte[] inBuffer = new byte[S.Length];
            byte[] outBuffer;

            // Read the encrypted data from a stream.
            if (S.CanRead)
            {
                await S.ReadAsync(inBuffer, 0, (int)S.Length);

                outBuffer = ProtectedData.Unprotect(inBuffer, Entropy, Scope);
            }
            else
            {
                throw new IOException("Could not read the stream.");
            }

            // Return the length that was written to the stream. 
            return outBuffer;
        }
    }
}
