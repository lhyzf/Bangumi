using Bangumi.Api.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Bangumi.Api.Services
{
    public static class HttpHelper
    {
        /// <summary>
        /// 使用 Get 方法获取数据。
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<string> GetAsync(string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Timeout = 15000;
                // 请求添加时间，防止搜索过于频繁
                Cookie cookie = new Cookie("chii_searchDateLine", DateTime.Now.ToString(),"/", "api.bgm.tv");
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookie);
                HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    return await reader.ReadToEndAsync();
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse response = (HttpWebResponse)ex.Response;
                if (response != null)
                {
                    Debug.WriteLine("response.StatusCode:" + response.StatusCode);
                }
                throw ex;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Network request fail.(Get)");
                Debug.WriteLine(e.Message);
                throw e;
            }
        }

        /// <summary>
        /// 使用 Post 方法提交数据。
        /// </summary>
        /// <param name="url"></param>
        /// <param name="post"></param>
        /// <returns></returns>
        public static async Task<string> PostAsync(string url, string post = "")
        {
            try
            {
                byte[] requestBytes = Encoding.ASCII.GetBytes(post);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                Stream requestStream = await request.GetRequestStreamAsync();
                requestStream.Write(requestBytes, 0, requestBytes.Length);
                HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    return await reader.ReadToEndAsync();
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse response = (HttpWebResponse)ex.Response;
                if (response != null)
                {
                    Debug.WriteLine("response.StatusCode:" + response.StatusCode);
                }
                throw ex;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Network request fail.(Post)");
                Debug.WriteLine(e.Message);
                throw e;
            }

        }
    }
}
