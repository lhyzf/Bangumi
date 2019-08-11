using System.IO;
using System.Threading.Tasks;

namespace Bangumi.Data
{
    public static class FileHelper
    {
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

    }
}
