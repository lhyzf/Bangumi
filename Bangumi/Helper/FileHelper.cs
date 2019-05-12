using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Bangumi.Helper
{
    class FileHelper
    {
        public static StorageFolder localFolder = ApplicationData.Current.LocalFolder;
        public static StorageFolder tempFolder = ApplicationData.Current.TemporaryFolder;
        /// <summary>
        /// 写入文件。
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="userFileName"></param>
        /// <param name="encrytion"></param>
        /// <returns></returns>
        public static async Task<bool> WriteToFile(string msg, string fileName, bool encrytion = false)
        {
            StorageFile storageFile = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            try
            {
                if (encrytion)
                {
                    var EncrytedData = await EncryptionHelper.TokenEncryptionAsync(msg);
                    await FileIO.WriteBufferAsync(storageFile, EncrytedData);
                }
                else
                {
                    await FileIO.WriteTextAsync(storageFile, msg);
                }
                return true;
            }
            catch (Exception)
            {
                await storageFile.DeleteAsync();
                return false;
            }
        }

        /// <summary>
        /// 从文件读取。
        /// </summary>
        /// <param name="userFileName"></param>
        /// <param name="encrytion"></param>
        /// <returns></returns>
        public static async Task<string> ReadFromFile(string fileName, bool encrytion = false)
        {
            try
            {
                StorageFile storageFile = await localFolder.GetFileAsync(fileName);
                if (encrytion)
                {
                    IBuffer buffMsg = await FileIO.ReadBufferAsync(storageFile);
                    return await EncryptionHelper.TokenDecryption(buffMsg);
                }
                else
                {
                    return await FileIO.ReadTextAsync(storageFile);
                }
            }
            catch (FileNotFoundException)
            {
                // Cannot find file
                Debug.WriteLine("File not found.");
                return "";
            }
            catch (IOException e)
            {
                // Get information from the exception, then throw
                // the info to the parent method.
                if (e.Source != null)
                {
                    Debug.WriteLine("IOException source: {0}", e.Source);
                }
                return "";
            }
        }

        /// <summary>
        /// 写入临时文件。
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="userFileName"></param>
        /// <param name="encrytion"></param>
        /// <returns></returns>
        public static async Task<bool> WriteToTempFile(string msg, string fileName)
        {
            StorageFile storageFile = await tempFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            try
            {
                await FileIO.WriteTextAsync(storageFile, msg);
                return true;
            }
            catch (Exception)
            {
                await storageFile.DeleteAsync();
                return false;
            }
        }

        /// <summary>
        /// 从临时文件读取。
        /// </summary>
        /// <param name="userFileName"></param>
        /// <param name="encrytion"></param>
        /// <returns></returns>
        public static async Task<string> ReadFromTempFile(string fileName)
        {
            try
            {
                StorageFile storageFile = await tempFolder.GetFileAsync(fileName);
                return await FileIO.ReadTextAsync(storageFile);
            }
            catch (FileNotFoundException)
            {
                // Cannot find file
                Debug.WriteLine("File not found.");
                return "";
            }
            catch (IOException e)
            {
                // Get information from the exception, then throw
                // the info to the parent method.
                if (e.Source != null)
                {
                    Debug.WriteLine("IOException source: {0}", e.Source);
                }
                return "";
            }
        }
    }
}
