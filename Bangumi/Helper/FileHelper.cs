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
        public static StorageFolder cacheFolder = ApplicationData.Current.LocalCacheFolder;
        /// <summary>
        /// 写入文件。
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="userFileName"></param>
        /// <param name="encrytion"></param>
        /// <returns></returns>
        public static async Task<bool> WriteToFileAsync(string msg, string fileName, bool encrytion = false)
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
            catch (Exception e)
            {
                Console.WriteLine(e);
                DeleteLocalFile(fileName);
                return false;
            }
        }

        /// <summary>
        /// 从文件读取。
        /// </summary>
        /// <param name="userFileName"></param>
        /// <param name="encrytion"></param>
        /// <returns></returns>
        public static async Task<string> ReadFromFileAsync(string fileName, bool encrytion = false)
        {
            try
            {
                if (File.Exists(localFolder.Path + "\\" + fileName))
                {
                    StorageFile storageFile = await localFolder.GetFileAsync(fileName);
                    if (encrytion)
                    {
                        IBuffer buffMsg = await FileIO.ReadBufferAsync(storageFile);
                        return await EncryptionHelper.TokenDecryptionAsync(buffMsg);
                    }
                    else
                    {
                        return await FileIO.ReadTextAsync(storageFile);
                    }
                }
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
        public static async Task<bool> WriteToCacheFileAsync(string msg, string fileName)
        {
            StorageFile storageFile = await cacheFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            try
            {
                await FileIO.WriteTextAsync(storageFile, msg);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                DeleteCacheFile(fileName);
                return false;
            }
        }

        /// <summary>
        /// 从临时文件读取。
        /// </summary>
        /// <param name="userFileName"></param>
        /// <param name="encrytion"></param>
        /// <returns></returns>
        public static async Task<string> ReadFromCacheFileAsync(string fileName)
        {
            try
            {
                if (File.Exists(cacheFolder.Path + "\\" + fileName))
                {
                    StorageFile storageFile = await cacheFolder.GetFileAsync(fileName);
                    return await FileIO.ReadTextAsync(storageFile);
                }
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
        /// 删除localFolder中的文件
        /// </summary>
        /// <param name="filename">在localFolder中的文件名</param>
        public static void DeleteLocalFile(string filename)
        {
            if (File.Exists(localFolder.Path + "\\" + filename))
                File.Delete(localFolder.Path + "\\" + filename);
        }

        /// <summary>
        /// 删除cacheFolder中的文件
        /// </summary>
        /// <param name="filename">在cacheFolder中的文件名</param>
        public static void DeleteCacheFile(string filename)
        {
            if (File.Exists(cacheFolder.Path + "\\" + filename))
                File.Delete(cacheFolder.Path + "\\" + filename);
        }
    }
}
