using System;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.DataProtection;
using Windows.Storage.Streams;

namespace Bangumi.Helper
{
    class EncryptionHelper
    {
        /// <summary>
        /// 加密字符串。
        /// </summary>
        /// <param name="strMsg"></param>
        /// <returns></returns>
        public static async Task<IBuffer> TokenEncryptionAsync(string strMsg)
        {
            // Create a DataProtectionProvider object for the specified descriptor.
            DataProtectionProvider Provider = new DataProtectionProvider("LOCAL=user");

            try
            {
                // Encode the plaintext input message to a buffer.
                IBuffer buffMsg = CryptographicBuffer.ConvertStringToBinary(strMsg, BinaryStringEncoding.Utf8);

                // Encrypt the message.
                IBuffer buffProtected = await Provider.ProtectAsync(buffMsg);

                return buffProtected;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 解密字符串。
        /// </summary>
        /// <param name="buffProtected"></param>
        /// <returns></returns>
        public static async Task<string> TokenDecryptionAsync(IBuffer buffProtected)
        {
            // Create a DataProtectionProvider object.
            DataProtectionProvider Provider = new DataProtectionProvider();

            try
            {
                // Decrypt the protected message specified on input.
                IBuffer buffUnprotected = await Provider.UnprotectAsync(buffProtected);

                // Convert the unprotected message from an IBuffer object to a string.
                String strClearText = CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, buffUnprotected);

                // Return the plaintext string.
                return strClearText;
            }
            catch (Exception)
            {
                return "";
            }
        }

    }
}
