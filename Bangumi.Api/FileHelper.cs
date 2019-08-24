using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Bangumi.Api
{
    public static class FileHelper
    {
        #region 异步读写文件

        /// <summary>
        /// 异步读文件
        /// </summary>
        /// <param name="fileName">文件路径全名</param>
        /// <returns></returns>
        public static async Task<string> ReadTextAsync(string fileName)
        {
            using (var reader = File.OpenText(fileName))
            {
                return await reader.ReadToEndAsync();
            }
        }

        /// <summary>
        /// 异步写文件
        /// </summary>
        /// <param name="fileName">文件路径全名</param>
        /// <param name="data">待写入文本</param>
        /// <returns></returns>
        public static async Task WriteTextAsync(string fileName, string data)
        {
            using (var writer = File.CreateText(fileName))
            {
                await writer.WriteAsync(data);
            }
        }

        #endregion

        #region 加解密读写文件
        /// <summary>
        /// 加密并写入文件。
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static async Task EncryptAndWriteFileAsync(string fileName, string data)
        {
            try
            {
                await EncryptionHelper.Write(fileName, data);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        /// <summary>
        /// 从文件读取并解密。
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static async Task<string> ReadAndDecryptFileAsync(string fileName)
        {
            try
            {
                if (File.Exists(fileName))
                {
                    return await EncryptionHelper.Read(fileName);
                }
                return "";
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return "";
            }
        }

        #endregion


        /// <summary>
        /// 删除存在的文件
        /// </summary>
        /// <param name="filename">文件完整路径</param>
        public static void DeleteFile(string filename)
        {
            if (File.Exists(filename))
                File.Delete(filename);
        }

    }
}
